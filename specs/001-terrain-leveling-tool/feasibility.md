# Feasibility Evidence

## Phase 1 skeleton — 2026-07-22

Environment:

- .NET SDK: `10.0.102`; MSBuild `18.0.7`
- Target framework: `.NET Framework 4.8` (`net48`)
- Game root used for this run: `V:\SteamLibrary\steamapps\common\Captain of Industry`
- Managed assembly versions: `Mafi`, `Mafi.Core`, `Mafi.Base`, and `Mafi.Unity` all `0.8.6.0`
- Unity player product version: `6000.3.19f1 (7689f4515d75)`

Commands and results:

```powershell
$env:COI_ROOT='V:\SteamLibrary\steamapps\common\Captain of Industry'
dotnet build COILevelingTool.slnx -c Release
dotnet test tests/COILevelingTool.Tests/COILevelingTool.Tests.csproj -c Release --no-build
```

- Release build: PASS, 0 warnings, 0 errors.
- Skeleton tests: PASS, 1 passed, 0 failed.
- Mod DLL: `src/COILevelingTool/bin/Release/net48/COILevelingTool.dll`.
- Package: `artifacts/COILevelingTool-0.1.0.zip`.
- ZIP inspection: PASS; root folder `COILevelingTool/` contains only the DLL, manifest, and player readme.

## Exact-version target revision

The installed `changelog.txt` starts with `v0.8.6a`; the managed assemblies remain versioned `0.8.6.0`. On 2026-07-22 the approved target was revised from `v0.8.6` build 609 to exact runtime version `v0.8.6a`. No authoritative numeric build identifier for `v0.8.6a` was found in the installed changelog or recent game logs, so the planned compatibility probe must validate the runtime version string and required public capabilities rather than infer the patch from assembly version alone.

The former version blocker is resolved. The Release skeleton was rebuilt and tested against the revised installed target, completing T007. Phase 2 remains gated by its capability and in-game feasibility tasks.
