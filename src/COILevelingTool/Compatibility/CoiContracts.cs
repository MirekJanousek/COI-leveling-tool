using System;
using System.Collections.Generic;

namespace COILevelingTool.Compatibility;

public readonly record struct GridPoint(int X, int Y);
public readonly record struct TerrainHeight(long RawValue);

public enum AdapterFailure
{
    None, UnsupportedVersion, MissingCapability, InvalidTarget, OutOfBounds,
    Occupied, Reserved, TerrainWriteFailed, VerificationFailed, RollbackFailed,
    PlacementFailed, CleanupFailed, CompensationFailed, DuplicateOperation
}

public readonly record struct AdapterResult(bool Succeeded, AdapterFailure Failure, string Detail)
{
    public static AdapterResult Success() => new(true, AdapterFailure.None, string.Empty);
    public static AdapterResult Fail(AdapterFailure failure, string detail) => new(false, failure, detail);
}

public sealed record TerrainState(IReadOnlyDictionary<GridPoint, TerrainHeight> Heights);
public sealed record PlacementRequest(IReadOnlyList<GridPoint> Targets, TerrainHeight Elevation);
public sealed record PlacementBatchResult(AdapterResult Result, IReadOnlyList<object> CreatedEntities);
public sealed record ChargeLedger(IReadOnlyDictionary<string, int> Products, long UnityRaw);
public sealed record CompensationResult(AdapterResult Result, IReadOnlyList<string> CompletedSteps);

