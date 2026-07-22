using System.Collections.Generic;
using System.Linq;
using Mafi.Base;
using Mafi.Base.Prototypes.Buildings;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;

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

/// <summary>
/// Temporary, deliberately plain prototypes used only by the Phase 2 in-game gate.
/// The final prototype replaces these after one cost mode is proven.
/// </summary>
public sealed class LevelingStructureFeasibilityData : IModData
{
    private const string TileSurfaceCubePrefab = "Assets/Base/Terrain/TileSurfaceCube.prefab";
    public const string RetainingWallToolbarGroup = "RetainingWallProto";
    public const string RetainingWallResearchNode = "ResearchRetainingWalls";

    public static readonly MachineProto.ID UnityCandidateId =
        Ids.Machines.CreateId("COILevelingTool_UnitySpike");

    public static readonly MachineProto.ID SlabCandidateId =
        Ids.Machines.CreateId("COILevelingTool_SlabSpike");

    public static IReadOnlyList<LevelingStructureCandidate> Candidates { get; } = new[]
    {
        new LevelingStructureCandidate(UnityCandidateId.Value, RetainingWallToolbarGroup,
            RetainingWallResearchNode, 1, 1, FeasibilityCostMode.UnityOnly, 0, 0.05m),
        new LevelingStructureCandidate(SlabCandidateId.Value, RetainingWallToolbarGroup,
            RetainingWallResearchNode, 1, 1, FeasibilityCostMode.FiveConcreteSlabs, 5, 0),
    };

    public void RegisterData(ProtoRegistrator registrator)
    {
        var unity = BuildCandidate(registrator, "Leveling spike (Unity candidate)", UnityCandidateId,
            new EntityCostsTpl.Builder());
        var slabs = BuildCandidate(registrator, "Leveling spike (5 Concrete Slabs)", SlabCandidateId,
            new EntityCostsTpl.Builder().Product(5, Ids.Products.ConcreteSlab));

        var retainingWalls = registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.RetainingWalls);
        retainingWalls.AddProtoToUnlock(unity, false);
        retainingWalls.AddProtoToUnlock(slabs, false);
    }

    private static MachineProto BuildCandidate(
        ProtoRegistrator registrator,
        string name,
        MachineProto.ID id,
        EntityCostsTpl costs)
    {
        var retainingWall = registrator.PrototypesDb.All<RetainingWallProto>().First();
        var temporaryIcon = registrator.PrototypesDb.GetOrThrow<ProductProto>(Ids.Products.ConcreteSlab).IconPath;
        return registrator.MachineProtoBuilder
            .Start(name, id)
            .Description("Phase 2 feasibility structure. Use only in a disposable sandbox save.")
            .SetCost(costs)
            .SetLayout(new[] { "[1]" })
            .SetCategories(retainingWall.Graphics.Categories[0].CategoryProto.Id)
            .SetPrefabPath(TileSurfaceCubePrefab)
            // A built-in non-wall icon keeps this feasibility build self-contained. The final
            // generated box icon lives in assets/icons and requires MaFi's Unity bundle workflow.
            .SetCustomIconPath(temporaryIcon)
            .DisableBoost()
            .BuildAndAdd();
    }
}
