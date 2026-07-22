# Phase 0 Research: COI Terrain Leveling Tool

**Date**: 2026-07-22

## Target Game and Compatibility Pin

**Decision**: Target Captain of Industry Update 4.2, game `v0.8.6a`, and `Mafi.Core 0.8.6.0`. Set both `min_game_version` and `max_verified_game_version` to `0.8.6a` for the first release and reverify the exact runtime version before packaging.

**Rationale**: Update 4.2 was released on 2026-07-20. The installed changelog reports `v0.8.6a`, and the installed core assemblies report `0.8.6.0`. No authoritative numeric build identifier for this patch was found locally, so the compatibility gate verifies the exact runtime game version string plus assembly capabilities. Update 4 requires mods to be recompiled and use the new manifest format.

**Alternatives considered**: Targeting `0.8.0` broadly was rejected because the mod API is experimental and Update 4.2 changed Unity and construction/drag behavior. Tracking experimental branches was rejected as contrary to the stable-version requirement.

**Sources**:

- https://www.captain-of-industry.com/post/update42-is-out
- https://www.captain-of-industry.com/post/update4-is-out
- Installed `%APPDATA%/Captain of Industry/Logs` and `Mafi.Core.dll` metadata

## Official Toolchain and Packaging

**Decision**: Use C# with an SDK-style `net48` library project derived from MaFi's official `ExampleMod`. Reference installed game assemblies through `COI_ROOT`; do not introduce an unofficial runtime mod framework or a fictional COI NuGet SDK. Build in Release with Visual Studio as the supported path and keep `dotnet build` as a CI/convenience path.

**Rationale**: The official repository requires .NET Framework 4.8, directly references `Mafi`, `Mafi.Core`, `Mafi.Base`, and `Mafi.Unity`, deploys to `%APPDATA%/Captain of Industry/Mods/<id>`, and packages `manifest.json` plus the compiled DLL. The repository warns that mod APIs remain experimental.

**Alternatives considered**: Harmony/private reflection was rejected by the constitution. BepInEx and community APIs were rejected because the required primitives are present in the game's public assemblies and a smaller integration surface is safer.

**Sources**:

- https://github.com/MaFi-Games/Captain-of-industry-modding
- https://github.com/MaFi-Games/Captain-of-industry-modding/blob/main/src/ExampleMod/ExampleMod.csproj

## Prototype, Menu, and Research Integration

**Decision**: Register a custom 1x1 `LayoutEntityProto`/entity following the installed retaining-wall pattern, use the toolbar entries returned for the base Terraforming message group so the tool appears alongside retaining walls, and add the prototype to the existing retaining-wall research unlock rather than creating a new research node.

**Rationale**: The official example proves layout, category, prefab, cost, and prototype registration. Installed `RetainingWallsData` uses the Terraforming group, and public `ResearchNodeProto.AddProtoToUnlock` supports adding a prototype to the existing `Ids.Research.RetainingWalls` node.

**Alternatives considered**: A new toolbar category or research node adds unrelated UI/progression. Always-unlocked placement was rejected because retaining-wall work should become available with retaining walls.

**Sources**:

- https://github.com/MaFi-Games/Captain-of-industry-modding/blob/main/src/ExampleMod/ExampleMachineData.cs
- https://github.com/MaFi-Games/Captain-of-industry-modding/blob/main/src/ExampleMod/ExampleResearchData.cs
- Installed `Mafi.Base.xml` and `Mafi.Base.dll` public API metadata

## Grid Model and Eligibility

**Decision**: Map the specification's parent tile to the game's canonical 4x4 terrain-designation cell. Derive its origin and size from `TerrainDesignation.SIZE_TILES` (asserted as `4` for `v0.8.6a`) rather than scattering a magic number. A target is one unoccupied 1x1 terrain square in that cell; the cell must contain at least one fully constructed `RetainingWallEntity`, and the target square itself must not overlap any entity or reservation.

**Rationale**: Installed public API documentation defines terrain designations as 4x4. `TerrainOccupancyManager` can query all occupying entities, and `RetainingWallEntity`/`RetainingWallProto` are public. Requiring a completed wall matches the feature's frozen-terrain use case and prevents a planned wall from granting eligibility before its final geometry exists.

**Alternatives considered**: Edge/corner adjacency was rejected because it does not match the clarified parent-cell rule. A hardcoded independent parent coordinate system was rejected because it could drift from game behavior.

**Sources**:

- Installed `Mafi.Core.xml`, `Mafi.Base.xml`, `Mafi.Core.dll`, and `Mafi.Base.dll`
- https://wiki.coigame.com/Retaining_Wall

## Terrain Mutation Boundary and Method

**Decision**: Define one selected 1x1 square as its four bounding terrain vertices. Write only those four vertices to the selected elevation with a high-level preserve-relative-layers/no-physics operation, then verify the values. Do not write any other vertex. Document that adjacent terrain triangles share these boundary vertices and may visually slope toward the new height even though no unselected vertex is written.

**Rationale**: The terrain model stores four shared vertices around each square. `TerrainManager` exposes bounds checks, height reads, preserve-relative-layer setters, changed-terrain tracking, and deferred events. Disabling terrain physics prevents avalanches from escaping the selected footprint and is consistent with the retaining-wall frozen-terrain use case.

**Alternatives considered**: Normal physics-enabled setters can trigger changes outside the footprint. Raw array writes omit change tracking/events. Setting only one vertex does not level the square.

**Sources**:

- Installed `Mafi.Core.xml` documentation for `TerrainManager`, `TerrainTile`, `RectangleTerrainArea2i`, and terrain events

## Terrain Atomicity and Save Persistence

**Decision**: Implement a simulation-thread `CoiTerrainAdapterV086` that prevalidates all four vertices, snapshots original heights, applies the four writes, verifies the result, and restores all originals on exception or mismatch. Completed terrain relies on the game's changed-terrain serialization; the mod stores no parallel terrain record.

**Rationale**: There is no public multi-vertex terrain transaction. The contained snapshot/verify/rollback adapter is the smallest safe composition of public APIs. Game documentation states changed terrain is serialized and deferred terrain events flush before save; an integration test must prove the chosen high-level setter marks and flushes changes in `v0.8.6a`.

**Alternatives considered**: Treating four writes as inherently atomic was rejected. Persisting a mod-owned terrain ledger would duplicate authoritative game state and complicate removal/migration.

**Sources**:

- Installed `Mafi.Core.xml` documentation for terrain setters, `NotifyTileHeightLayersChanged`, deferred events, and serialization

## Construction Mode and Canonical Material

**Decision**: Run a feasibility spike for material-free Unity-only completion. The supported fallback is exactly five **Concrete Slabs** per structure using `Costs.Build.Concrete(5)`, with the game's standard pay-with-Unity quick-build path available even at vehicle-inaccessible sites. Attempt a target cost of `0.05` Unity only when a supported public configuration exists; otherwise display and document the engine-calculated value.

**Rationale**: `EntityCostsTpl.Builder.UPoints(int)` does not prove fractional construction cost support. `Upoints.FromFraction(1,20)` exists, but no supported fractional construction builder route is evidenced. Concrete cost and native quick build are public and proven. "Concrete bricks" in the clarification maps to the current in-game product `Concrete Slab`; bricks were removed before the target version.

**Alternatives considered**: Inventing a fixed fractional Unity price through private patching was rejected. A free build was rejected because the specification requires a cost.

**Sources**:

- Installed `Mafi.Base.xml`/`Mafi.Core.xml` cost and construction APIs
- https://wiki.coigame.com/Concrete_Slab
- https://wiki.coigame.com/Bricks

## Drag Placement and All-or-None Commit

**Decision**: First test the stock layout-entity mass placer for axis-aligned drag and elevation. A custom public simulation command must revalidate the entire ordered line, including self-collision, before creating the first entity. If public extension points cannot enforce straight-only all-or-none placement, stop the feature implementation and request a policy/scope decision; do not patch internal UI.

**Rationale**: Update 4.2 supports drag placement and installed code contains a generic mass placer, but its extension surface and atomic behavior are not officially documented. UI-only validation cannot provide the formal guarantee because state can change before simulation command execution.

**Alternatives considered**: Sequential base commands can partially create a line. Harmony-patching the internal placer violates minimal intrusion.

**Sources**:

- https://www.captain-of-industry.com/post/update42-is-out
- Installed `Mafi.Core.dll` public metadata for batch/static entity commands and validation APIs

## Completion, Removal, and Post-Completion Cost

**Decision**: Subscribe on the simulation thread to the public construction-completed event and filter the leveling prototype. On success, mutate and verify terrain, then remove/destroy the temporary entity. On failure, restore terrain, remove the entity, make no additional charge, notify the player that completed construction costs are not refunded, and log the result.

**Rationale**: Public completion and normal entity removal exist, but no exact post-completion products-and-Unity receipt exists. The approved 2026-07-22 product decision removes the refund guarantee rather than introducing an undocumented integration. Completion handling remains idempotent so duplicate events cannot repeat mutation or cleanup.

**Alternatives considered**: The generic quick-remove/refund command may apply difficulty/deconstruction ratios and does not prove a 100% refund. A private charge hook was rejected. A mod-controlled separate Unity transaction was not selected.

**Sources**:

- Installed `Mafi.Core.xml` documentation for `ConstructionManager`, `IConstructionProgress`, `IAssetTransactionManager`, `IUpointsManager`, and `EntitiesManager`

## Assets and Unity Editor Version

**Decision**: Avoid a custom AssetBundle for the first implementation where a distinct game-native preview/icon/prefab can satisfy usability. If a custom asset is essential, first reconcile the installed Unity player `6000.3.19f1` with the official modding repository's stale `6000.0.66f1` asset instructions and rebuild/test using the exact supported editor.

**Rationale**: Update 4.2 explicitly upgraded the rendering engine and warns modders may need to update, while the official sample has not yet updated its asset instructions. Deferring custom assets removes avoidable compatibility risk.

**Alternatives considered**: Building bundles with a guessed editor version risks unloadable or corrupt assets.

**Sources**:

- https://www.captain-of-industry.com/post/update42-is-out
- https://github.com/MaFi-Games/Captain-of-industry-modding#assets-creation

## Diagnostics and Compatibility Failure

**Decision**: At startup, probe exact required public types, methods, constants, and game/assembly versions. If any capability is missing, do not register or enable terrain mutation; emit one actionable in-game warning and log stable reason codes with version, entity, target, parent origin, vertices, elevation, cost mode, and rollback/removal status.

**Rationale**: The official guide calls the API experimental and directs modders to game logs. Fail-closed capability checks preserve normal gameplay after updates.

**Alternatives considered**: Best-effort execution on an unknown version risks save and terrain damage. A crash-on-load harms unrelated gameplay.

**Sources**:

- https://github.com/MaFi-Games/Captain-of-industry-modding#status-of-modding-support
- https://github.com/MaFi-Games/Captain-of-industry-modding#questions-issues

## Test Strategy

**Decision**: Split evidence into pure unit tests, installed-assembly capability contracts, and an in-game sandbox matrix on `v0.8.6a`. Include injected failures after each of four vertex writes and cleanup, shared-vertex adjacency, every retaining-wall orientation/position, line atomicity, map limits, occupancy/reservations, vehicle-inaccessible Unity completion, save/load, and clean mod removal.

**Rationale**: MaFi provides no official automated gameplay harness. Pure policy logic remains testable outside the game, while integration guarantees require the actual simulation and save system.

**Alternatives considered**: In-game-only testing is slow and weak at fault injection. Unit-only testing cannot prove engine integration.

**Sources**:

- Official sandbox support: https://www.captain-of-industry.com/post/update4-is-out
- Official build/log workflow: https://github.com/MaFi-Games/Captain-of-industry-modding#getting-started
