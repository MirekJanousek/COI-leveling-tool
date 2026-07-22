using System.Linq;
using COILevelingTool.Prototypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace COILevelingTool.Tests.Contract;

[TestClass]
public sealed class LevelingStructureFeasibilityContractTests
{
    [TestMethod]
    public void DefinesBothOneByOneCostCandidatesForSameUnlockAndToolbar()
    {
        var candidates = LevelingStructureFeasibilityData.Candidates;

        Assert.AreEqual(2, candidates.Count);
        Assert.IsTrue(candidates.All(x => x.Width == 1 && x.Height == 1));
        Assert.IsTrue(candidates.All(x => x.ToolbarGroupId == LevelingStructureFeasibilityData.RetainingWallToolbarGroup));
        Assert.IsTrue(candidates.All(x => x.ResearchNodeId == LevelingStructureFeasibilityData.RetainingWallResearchNode));
        Assert.IsTrue(candidates.Any(x => x.CostMode == FeasibilityCostMode.UnityOnly && x.ConcreteSlabs == 0 && x.RequestedUnity == 0.05m));
        Assert.IsTrue(candidates.Any(x => x.CostMode == FeasibilityCostMode.FiveConcreteSlabs && x.ConcreteSlabs == 5));
    }
}
