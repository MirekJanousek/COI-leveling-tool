using System;
using System.Linq;
using System.Reflection;
using COILevelingTool.Compatibility.V086;
using Mafi.Core.Terrain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace COILevelingTool.Tests.Contract;

[TestClass]
public sealed class V086CapabilityContractTests
{
    private static readonly BindingFlags PublicApi = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

    [TestMethod]
    public void InstalledCoreAssemblyHasPinnedVersion()
    {
        Assert.AreEqual(new Version(0, 8, 6, 0), typeof(TerrainManager).Assembly.GetName().Version);
    }

    [TestMethod]
    [DataRow("Mafi.Core.Terrain.TerrainManager", "GetHeight")]
    [DataRow("Mafi.Core.Terrain.TerrainManager", "SetHeightPreserveRelativeLayersNoPhysics")]
    [DataRow("Mafi.Core.Terrain.TerrainManager", "NotifyTileHeightLayersChanged")]
    [DataRow("Mafi.Core.Entities.Static.IConstructionManager", "EntityConstructed")]
    [DataRow("Mafi.Core.Entities.Static.IConstructionManager", "GetConstructionProgress")]
    [DataRow("Mafi.Core.Economy.IAssetTransactionManager", "StoreProduct")]
    [DataRow("Mafi.Core.Population.IUpointsManager", "GenerateUnity")]
    [DataRow("Mafi.Core.Terrain.TerrainOccupancyManager", "IsOccupiedAt")]
    [DataRow("Mafi.Core.Entities.EntitiesManager", "TryRemoveAndDestroyEntity")]
    public void RequiredCoreMemberIsPublic(string typeName, string memberName)
    {
        var type = typeof(TerrainManager).Assembly.GetType(typeName, true)!;
        Assert.IsTrue(type.GetMember(memberName, PublicApi).Any(), $"Missing public {typeName}.{memberName}");
    }

    [TestMethod]
    public void CanonicalDesignationMappingIsPublic()
    {
        var type = typeof(TerrainManager).Assembly.GetType(
            "Mafi.Core.Terrain.Designation.TerrainDesignationsManager", true)!;
        Assert.IsTrue(type.GetMember("GetCanonicalDesignationRange", PublicApi).Any(),
            "Missing public canonical designation mapping API.");
    }

    [TestMethod]
    public void ProbeEnablesOnlyExactSupportedProfile()
    {
        var core = typeof(TerrainManager).Assembly;
        var unity = Assembly.Load("Mafi.Unity");
        var probe = new CapabilityProbeV086(core, unity);

        var supported = probe.Probe("v0.8.6a");
        var wrongPatch = probe.Probe("v0.8.6");

        CollectionAssert.DoesNotContain(supported.Capabilities.Values.ToArray(), false);
        Assert.IsTrue(supported.IsEnabled);
        Assert.IsFalse(wrongPatch.IsEnabled);
    }
}
