using System;
using System.Collections.Generic;
using System.Linq;
using Mafi;
using Mafi.Core.Terrain;

namespace COILevelingTool.Compatibility.V086;

public sealed class CoiTerrainAdapterV086 : ICoiTerrainAdapter
{
    private readonly ITerrainAccessV086 m_terrain;
    private readonly Func<bool> m_isSimulationThread;
    private readonly Action<int>? m_beforeWrite;

    public CoiTerrainAdapterV086(TerrainManager terrain, Func<bool> isSimulationThread, Action<int>? beforeWrite = null)
        : this(new TerrainAccessV086(terrain), isSimulationThread, beforeWrite)
    {
    }

    public CoiTerrainAdapterV086(ITerrainAccessV086 terrain, Func<bool> isSimulationThread, Action<int>? beforeWrite = null)
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
            if (!m_terrain.IsValid(point))
                return AdapterResult.Fail(AdapterFailure.OutOfBounds, $"Terrain vertex {point} is invalid or off-limits.");
            heights.Add(point, new TerrainHeight(m_terrain.GetHeight(point)));
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
            if (m_terrain.GetHeight(point) != elevation.RawValue)
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
                if (m_terrain.GetHeight(item.Key) != item.Value.RawValue)
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
        m_terrain.SetHeightWithoutPhysics(point, height.RawValue);
        m_terrain.NotifyChanged(point);
    }
}

public interface ITerrainAccessV086
{
    bool IsValid(GridPoint point);
    long GetHeight(GridPoint point);
    void SetHeightWithoutPhysics(GridPoint point, long rawHeight);
    void NotifyChanged(GridPoint point);
}

internal sealed class TerrainAccessV086 : ITerrainAccessV086
{
    private readonly TerrainManager m_terrain;

    public TerrainAccessV086(TerrainManager terrain) =>
        m_terrain = terrain ?? throw new ArgumentNullException(nameof(terrain));

    public bool IsValid(GridPoint point)
    {
        var tile = new Tile2i(point.X, point.Y);
        return m_terrain.IsValidCoord(tile) && !m_terrain.IsOffLimitsOrInvalid(tile);
    }

    public long GetHeight(GridPoint point) => m_terrain.GetHeight(new Tile2i(point.X, point.Y)).Value.RawValue;

    public void SetHeightWithoutPhysics(GridPoint point, long rawHeight)
    {
        var indexed = new Tile2i(point.X, point.Y).ExtendIndex(m_terrain);
        m_terrain.SetHeightPreserveRelativeLayersNoPhysics(indexed, new HeightTilesF(Fix32.FromRaw(checked((int)rawHeight))));
    }

    public void NotifyChanged(GridPoint point) =>
        m_terrain.NotifyTileHeightLayersChanged(new Tile2i(point.X, point.Y).ExtendIndex(m_terrain));
}
