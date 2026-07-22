using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Mods;

namespace COILevelingTool.Mod;

public sealed class COILevelingToolMod : DataOnlyMod
{
    public COILevelingToolMod(ModManifest manifest) : base(manifest)
    {
        Log.Info("COILevelingTool: constructed; feature registration remains disabled until the v0.8.6a capability gate passes.");
    }

    public override void RegisterPrototypes(ProtoRegistrator registrator)
    {
        Log.Info("COILevelingTool: skeleton loaded; no gameplay prototypes registered.");
    }

    public override void MigrateJsonConfig(VersionSlim savedVersion, Dict<string, object> savedValues)
    {
    }
}
