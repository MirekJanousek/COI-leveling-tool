using System.Collections.Generic;
using COILevelingTool.Compatibility;

namespace COILevelingTool.Diagnostics;

public sealed record LevelingDiagnosticContext(
    LevelingFailureCode Code,
    string GameVersion,
    string CoreVersion,
    string? PrototypeId = null,
    GridPoint? Target = null,
    GridPoint? ParentOrigin = null,
    IReadOnlyList<GridPoint>? Vertices = null,
    TerrainHeight? Elevation = null,
    string? CostMode = null,
    string? RollbackStatus = null,
    string? RefundStatus = null,
    string? Detail = null);

