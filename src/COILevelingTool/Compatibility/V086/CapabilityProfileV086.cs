using System.Collections.Generic;

namespace COILevelingTool.Compatibility.V086;

public sealed class CapabilityProfileV086
{
    public const string SupportedGameVersion = "0.8.6a";
    public const string SupportedCoreVersion = "0.8.6.0";

    public CapabilityProfileV086(string gameVersion, string coreVersion, IReadOnlyDictionary<string, bool> capabilities)
    {
        GameVersion = gameVersion;
        CoreVersion = coreVersion;
        Capabilities = capabilities;
    }

    public string GameVersion { get; }
    public string CoreVersion { get; }
    public IReadOnlyDictionary<string, bool> Capabilities { get; }
    public bool ExactVersion => GameVersion == SupportedGameVersion && CoreVersion == SupportedCoreVersion;
    public bool IsEnabled
    {
        get
        {
            if (!ExactVersion) return false;
            foreach (var capability in Capabilities)
                if (!capability.Value) return false;
            return true;
        }
    }
}

