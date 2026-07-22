using System;
using System.Collections.Generic;
using System.Linq;
using Mafi;
using Mafi.Core.Terrain;

namespace COILevelingTool.Compatibility.V086;

public sealed class CoiTerrainAdapterV086 : ICoiTerrainAdapter
{
    private readonly TerrainManager m_terrain;
    private readonly Func<bool> m_isSimulationThread;
    private readonly Action<int>? m_beforeWrite;

    public CoiTerrainAdapterV086(TerrainManager terrain, Func<bool> isSimulationThread, Action<int>? beforeWrite = null)
    {
        m_terrain = terrain ?? throw new ArgumentNullException(nameof(terrain));
        m_isSimulationThread = isSimulationThread ?? throw new ArgumentNullException(nameof(isSimulationThread));
        m_beforeWrite = beforeWrite;
    }

    public AdapterResult Snapshot(IReadOnlyList<GridPoint> vertices, out TerrainState snapshot)
    {
        var heights = new Dictionary<GridPoint, TerrainHeight>();
        snapshot = new TerrainState(heights);
        if (!m_isSimulationThread()) return AdapterResult.Fail(AdapterFailure.MissingCapability, "Terrain access requires the simulation thread.");

        foreach (var point in vertices.Distinct().OrderBy(x => x.X).ThenBy(x => x.Y))
        {
            var tile = new Tile2i(point.X, point.Y);
            if (!m_terrain.IsValidCoord(tile) || m_terrain.IsOffLimitsOrInvalid(tile))
                return AdapterResult.Fail(AdapterFailure.OutOfBounds, $"Terrain vertex {point} is invalid or off-limits.");
            heights.Add(point, new TerrainHeight(m_terrain.GetHeight(tile).Value.RawValue));
        }
        return AdapterResult.Success();
    }

    public AdapterResult ApplyWithoutPhysics(TerrainState snapshot, TerrainHeight elevation)
    {
        if (!m_isSimulationThread()) return AdapterResult.Fail(AdapterFailure.MissingCapability, "Terrain mutation requires the simulation thread.");
        var written = 0;
        try
        {
            foreach (var point in snapshot.Heights.Keys.OrderBy(x => x.X).ThenBy(x => x.Y))
            {
                m_beforeWrite?.Invoke(written + 1);
                Set(point, elevation);
                written++;
            }
            if (!Verify(snapshot.Heights.Keys.ToArray(), elevation))
                throw new InvalidOperationException("Terrain height verification failed.");
            return AdapterResult.Success();
        }
        catch (Exception ex)
        {
            var rollback = RestoreInternal(snapshot);
            return rollback.Succeeded
                ? AdapterResult.Fail(AdapterFailure.TerrainWriteFailed, ex.Message)
                : AdapterResult.Fail(AdapterFailure.RollbackFailed, ex.Message + " Rollback: " + rollback.Detail);
        }
    }

    public bool Verify(IReadOnlyList<GridPoint> vertices, TerrainHeight elevation)
    {
        foreach (var point in vertices.Distinct())
            if (m_terrain.GetHeight(new Tile2i(point.X, point.Y)).Value.RawValue != checked((int)elevation.RawValue))
                return false;
        return true;
    }

    public AdapterResult Restore(TerrainState snapshot)
    {
        if (!m_isSimulationThread()) return AdapterResult.Fail(AdapterFailure.MissingCapability, "Terrain restore requires the simulation thread.");
        return RestoreInternal(snapshot);
    }

    private AdapterResult RestoreInternal(TerrainState snapshot)
    {
        try
        {
            foreach (var item in snapshot.Heights.OrderBy(x => x.Key.X).ThenBy(x => x.Key.Y)) Set(item.Key, item.Value);
            foreach (var item in snapshot.Heights)
                if (m_terrain.GetHeight(new Tile2i(item.Key.X, item.Key.Y)).Value.RawValue != checked((int)item.Value.RawValue))
                    return AdapterResult.Fail(AdapterFailure.RollbackFailed, $"Rollback verification failed at {item.Key}.");
            return AdapterResult.Success();
        }
        catch (Exception ex)
        {
            return AdapterResult.Fail(AdapterFailure.RollbackFailed, ex.Message);
        }
    }

    private void Set(GridPoint point, TerrainHeight height)
    {
        var tile = new Tile2i(point.X, point.Y);
        var indexed = tile.ExtendIndex(m_terrain);
        m_terrain.SetHeightPreserveRelativeLayersNoPhysics(indexed, new HeightTilesF(Fix32.FromRaw(checked((int)height.RawValue))));
    }
}
