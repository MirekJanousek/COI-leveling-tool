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

## Terrain API spike — 2026-07-22

The installed `Mafi.Core.xml` contract for `TerrainManager.SetHeightPreserveRelativeLayersNoPhysics` states that the operation does not invoke events or set the changed bit. The adapter now pairs every apply and restore write with the public `NotifyTileHeightLayersChanged` API, whose contract explicitly sets the changed flag and defers the height-change event.

Automated contract coverage proves deterministic four-vertex snapshotting, bounds rejection before mutation, changed notification for every write, verification, and rollback after injected failures before writes 1–4. The Release suite passes with 27 tests. Save/reload persistence and actual simulation-thread behavior remain part of the T020C in-game matrix and are not claimed by this automated spike.

Construction and placement access bridges plus fault-injection tests have been added as supporting work, but T015–T018 remain open until the bridges are bound to and proven through the actual public `v0.8.6a` services.

## Phase 2 gate result — FAIL (plan revision required)

Installed `v0.8.6a` public metadata exposes `IConstructionManager.EntityConstructed`, `GetConstructionProgress`, construction buffers, normal entity removal, product storage, and Unity generation. However, `IEntityConstructionProgress` contains only entity and priority state. Neither it nor the completion event exposes an exact ledger containing both products and Unity actually charged, and the public API does not provide a completion-time Unity charge-capture operation.

This fails the adapter contract requirement to capture actual material and Unity charges before construction progress is discarded. Recomputing from prototype costs would not prove the actual charge and is explicitly prohibited by the accounting contract. The capability profile therefore reports `construction.exact_charge_capture=false` and remains disabled even on the exact supported version.

Per the constitution and the Phase 2 gate, no user-story implementation may begin. The spec and plan need an approved revision choosing one of these product changes:

1. remove post-completion Unity refund guarantees and accept only pre-mutation failure handling;
2. use a no-cost temporary entity and perform a separately controlled public Unity transaction whose amount the mod owns; or
3. approve a narrowly documented non-public integration exception (highest compatibility risk and not recommended).

The in-game matrix is deferred because the public metadata gate already fails. No private reflection, Harmony patch, raw save edit, or undocumented hook was substituted.

## Approved gate revision — option 2

The user approved option 2 on 2026-07-22: completed construction costs are not refunded when the subsequent terrain operation fails. FR-028, SC-011, the adapter/gameplay contracts, research, data model, quickstart, plan, and task descriptions now require terrain rollback, normal temporary-entity cleanup, no additional charge, and one explicit no-refund failure notification.

Exact charge capture and compensation are no longer required capabilities, so the exact-version capability profile can enable when all remaining public signatures are present. The remaining T020A–T020C gate must still prove unrestricted elevation, Unity construction behavior, line atomicity, save tracking, rollback, cleanup, notification, and the selected cost mode before user-story implementation begins.

## Initial T020 in-game feasibility observations — 2026-07-22

Test environment: Captain of Industry `v0.8.6a` sandbox with the Release feasibility package.

Observed passes:

- both the product-free Unity candidate and five-Concrete-Slab candidate load in the retaining-wall Terraforming category;
- both candidates can be selected and placed as independent 1x1 layout entities;
- the neutral base-game tile-surface cube is displayed instead of creating a retaining wall;
- ordinary placement collision rejects overlap with a constructed retaining wall.

Observed limitations/failures:

- the ordinary machine placement path permits only one elevation step above or below the local terrain; larger requested elevation offsets cannot be selected;
- a candidate completed at `terrain + 1` does not level terrain. This is not yet evidence of a terrain-adapter failure because T012/T019 currently register only the placement/cost prototypes; completion forwarding into the terrain spike is not wired;
- the custom generated toolbar icon cannot be consumed as a loose PNG. The current package uses the built-in Concrete Slab icon pending MaFi's supported Unity asset-bundle workflow.

Gate interpretation: retaining-wall menu integration, 1x1 placement, both cost modes, and wall collision are proven. Required general elevation selection is **not proven** on the machine placement surface and currently fails the gameplay contract. T020A–T020C replace the former single gate while the public dedicated-controller route is investigated; user-story implementation remains blocked.

## Placement plan revision — unrestricted elevation required

The user rejected a +/-1 product limitation because retaining-wall correction routinely needs larger elevation differences. The stock machine placement surface is therefore permanently rejected for this feature rather than accepted as a degraded fallback.

The revised T020A–T020C gate investigates a dedicated mod-owned placement controller. It must use a complete public `Mafi.Unity` lifecycle, own an explicit absolute height anchor, preview and submit a straight selection at least four height steps below and above local terrain, and hand the whole selection to one simulation-thread command for final validation and atomic creation. Merely finding public types in metadata does not pass the gate: registration, activation, preview, adjustment, submission, and deactivation must all work in the running game without private reflection or patches.

If this controller cannot be proven on `v0.8.6a`, the planned mod is infeasible under the constitution and implementation stops before T021. The plan does not permit silently reverting to the stock +/-1 behavior.
