using System.Collections.Generic;

namespace COILevelingTool.Prototypes;

public enum FeasibilityCostMode
{
    UnityOnly,
    FiveConcreteSlabs
}

public sealed record LevelingStructureCandidate(
    string PrototypeId,
    string ToolbarGroupId,
    string ResearchNodeId,
    int Width,
    int Height,
    FeasibilityCostMode CostMode,
    int ConcreteSlabs,
    decimal RequestedUnity);

public static class LevelingStructureFeasibilityData
{
    public const string RetainingWallToolbarGroup = "Toolbar.Terraforming.RetainingWalls";
    public const string RetainingWallResearchNode = "Research.RetainingWalls";

    public static IReadOnlyList<LevelingStructureCandidate> Candidates { get; } = new[]
    {
        new LevelingStructureCandidate("COILevelingTool.UnitySpike", RetainingWallToolbarGroup,
            RetainingWallResearchNode, 1, 1, FeasibilityCostMode.UnityOnly, 0, 0.05m),
        new LevelingStructureCandidate("COILevelingTool.SlabSpike", RetainingWallToolbarGroup,
            RetainingWallResearchNode, 1, 1, FeasibilityCostMode.FiveConcreteSlabs, 5, 0),
    };
}
