using System;
using System.Collections.Generic;

namespace COILevelingTool.Compatibility.V086;

public interface IPlacementAccessV086
{
    AdapterResult Validate(GridPoint target, TerrainHeight elevation);
    AdapterResult ValidateBatch(IReadOnlyList<GridPoint> targets, TerrainHeight elevation);
    AdapterResult Create(GridPoint target, TerrainHeight elevation, out object? entity);
    AdapterResult RemoveCreated(object entity);
}

public sealed class CoiPlacementAdapterV086 : ICoiPlacementAdapter
{
    private readonly IPlacementAccessV086 m_access;
    private readonly Func<bool> m_isSimulationThread;

    public CoiPlacementAdapterV086(IPlacementAccessV086 access, Func<bool> isSimulationThread)
    {
        m_access = access ?? throw new ArgumentNullException(nameof(access));
        m_isSimulationThread = isSimulationThread ?? throw new ArgumentNullException(nameof(isSimulationThread));
    }

    public AdapterResult Preview(PlacementRequest request) => ValidateAll(request);

    public PlacementBatchResult Commit(PlacementRequest request)
    {
        if (!m_isSimulationThread()) return Failed(AdapterFailure.MissingCapability, "Commit requires the simulation thread.");
        var validation = ValidateAll(request);
        if (!validation.Succeeded) return new PlacementBatchResult(validation, Array.Empty<object>());

        var created = new List<object>();
        foreach (var target in request.Targets)
        {
            var result = m_access.Create(target, request.Elevation, out var entity);
            if (!result.Succeeded || entity is null)
            {
                for (var i = created.Count - 1; i >= 0; i--) m_access.RemoveCreated(created[i]);
                return new PlacementBatchResult(
                    AdapterResult.Fail(AdapterFailure.PlacementFailed, result.Detail), Array.Empty<object>());
            }
            created.Add(entity);
        }
        return new PlacementBatchResult(AdapterResult.Success(), created);
    }

    private AdapterResult ValidateAll(PlacementRequest request)
    {
        if (request.Targets.Count == 0) return AdapterResult.Fail(AdapterFailure.InvalidTarget, "Selection is empty.");
        var unique = new HashSet<GridPoint>();
        foreach (var target in request.Targets)
        {
            if (!unique.Add(target)) return AdapterResult.Fail(AdapterFailure.InvalidTarget, "Selection self-collides.");
            var result = m_access.Validate(target, request.Elevation);
            if (!result.Succeeded) return result;
        }
        return m_access.ValidateBatch(request.Targets, request.Elevation);
    }

    private static PlacementBatchResult Failed(AdapterFailure failure, string detail) =>
        new(AdapterResult.Fail(failure, detail), Array.Empty<object>());
}
