using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Mods;
using COILevelingTool.Compatibility.V086;
using COILevelingTool.Diagnostics;
using COILevelingTool.Prototypes;
using Mafi.Core.Terrain;
using Mafi.Unity.Ui.Controllers.LayoutEntityPlacing;

namespace COILevelingTool.Mod;

public sealed class COILevelingToolMod : DataOnlyMod
{
    private readonly LevelingDiagnostics m_diagnostics = new(message => Log.Info(message));

    public COILevelingToolMod(ModManifest manifest) : base(manifest)
    {
        Log.Info("COILevelingTool: constructed; feature registration remains disabled until the v0.8.6a capability gate passes.");
    }

    public override void RegisterPrototypes(ProtoRegistrator registrator)
    {
        var profile = new CapabilityProbeV086(
            typeof(TerrainManager).Assembly,
            typeof(StaticEntityMassPlacer).Assembly)
            .Probe(CapabilityProfileV086.SupportedGameVersion);

        if (!profile.IsEnabled)
        {
            m_diagnostics.Report(new LevelingDiagnosticContext(
                profile.GameVersion != CapabilityProfileV086.SupportedGameVersion
                    ? LevelingFailureCode.UnsupportedGameVersion
                    : profile.CoreVersion != CapabilityProfileV086.SupportedCoreVersion
                        ? LevelingFailureCode.UnsupportedCoreVersion
                        : LevelingFailureCode.MissingCapability,
                profile.GameVersion,
                profile.CoreVersion,
                Detail: MissingCapabilities(profile)), notifyOnce: true);
            Log.Info("COILevelingTool: feasibility prototypes were not registered because the capability gate failed.");
            return;
        }

        registrator.RegisterData<LevelingStructureFeasibilityData>();
        Log.Info("COILevelingTool: v0.8.6a capability gate passed; registered both Phase 2 feasibility candidates.");
    }

    public override void MigrateJsonConfig(VersionSlim savedVersion, Dict<string, object> savedValues)
    {
    }

    private static string MissingCapabilities(CapabilityProfileV086 profile)
    {
        var missing = new System.Collections.Generic.List<string>();
        foreach (var capability in profile.Capabilities)
            if (!capability.Value)
                missing.Add(capability.Key);
        return missing.Count == 0 ? "version mismatch" : string.Join(",", missing);
    }
}
