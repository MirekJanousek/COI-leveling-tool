# Implementation Plan: COI Terrain Leveling Tool

**Branch**: `001-terrain-leveling-tool` | **Date**: 2026-07-22 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-terrain-leveling-tool/spec.md`

## Summary

Build a Captain of Industry Update 4.2 mod that registers a temporary 1x1 layout entity beside retaining walls, supports elevation and straight-line placement, and levels the four terrain vertices bounding each selected square when construction completes. The mod targets game `v0.8.6a` and `Mafi.Core 0.8.6.0`, follows the official .NET Framework 4.8 mod template, reuses the retaining-wall toolbar group and research unlock, and stores no custom save data.

All game-facing work is isolated behind a pinned `v0.8.6a` compatibility layer. Before feature implementation, executable spikes must prove that the public APIs can provide safe terrain rollback, all-or-none line placement, construction accounting/refunds, and the requested Unity construction behavior. Unsupported behavior fails closed; no Harmony patch, private reflection, raw save edit, or undocumented internal hook is allowed without a new approved plan exception.

## Technical Context

**Language/Version**: C# (`LangVersion=latest`) targeting .NET Framework 4.8 (`net48`)

**Primary Dependencies**: Installed Captain of Industry `v0.8.6a` assemblies referenced through `COI_ROOT`, including `Mafi.dll`, `Mafi.Core.dll`, `Mafi.Base.dll`, and `Mafi.Unity.dll`; no runtime third-party mod framework

**Storage**: No mod-owned persistent data; successful terrain changes use the game's changed-terrain serialization, while temporary construction state uses the game's entity/construction save system

**Testing**: MSTest-based unit and contract tests for pure validation/accounting logic; assembly capability tests against the installed `v0.8.6a` metadata; repeatable in-game sandbox verification and log inspection

**Target Platform**: Windows PC, Captain of Industry Update 4.2 `v0.8.6a`; Unity player `6000.3.19f1`

**Project Type**: Independently installable Captain of Industry executable mod (single C# mod assembly plus test project)

**Performance Goals**: Placement validation is linear in selected sub-tiles, performs no full-map scan, and validates a 100-sub-tile line in under 5 ms in isolated benchmarks; in-game maximum-length preview/commit causes no frame or simulation stall above 16 ms attributable to the mod on the reference machine

**Constraints**: Simulation-thread mutation only; reject rather than clamp at map bounds; mutate only the four vertices bounding each selected 1x1 square; no uncontrolled terrain physics; no retaining-wall overlap; all sub-tiles in a line prevalidated before creation; exact version capability probe at startup; no private API patching; custom Unity asset bundles deferred until MaFi resolves the documented Unity-editor-version mismatch

**Scale/Scope**: Single-player, one selected square or one axis-aligned consecutive line up to the game's supported drag length; each accepted structure becomes an independent completion operation; no general terraforming, multiplayer synchronization, wall mutation, or freeform area placement

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Official guidance**: Target is Update 4.2 `v0.8.6a`. The official MaFi modding repository and installed XML API documentation are authoritative. The plan uses `IMod`/prototype registration, Update 4 manifests, installed game references, and public `v0.8.6a` APIs only.
- **Core scope**: Eligibility is restricted to an empty 1x1 terrain square whose canonical 4x4 terrain-designation parent contains a fully constructed retaining wall. Parent cells without a wall and squares overlapping a wall are rejected.
- **Terrain and save safety**: The adapter preflights occupancy, bounds, four-vertex representability, wall eligibility, and the complete dragged line. It snapshots four vertex heights, performs simulation-thread mutation without terrain physics, verifies the result, and rolls back on failure. No mod save record is required; save/load persistence is verified in-game.
- **Compatibility and minimal intrusion**: A version-gated adapter is the only game-facing layer. Missing capabilities disable placement/mutation and emit one actionable diagnostic. No unrelated behavior is replaced, and custom assets are avoided for the first implementation.
- **Evidence-driven quality**: Pure logic, compatibility signatures, rollback injection, shared-vertex behavior, line atomicity, and refund idempotency receive automated coverage. A sandbox matrix verifies wall variants, bounds, occupancy, save/load, inaccessible sites, drag rejection, rollback, refunds, and removal.
- **Documentation and release readiness**: Quickstart and release work cover installation, `COI_ROOT`, supported build, controls, canonical Concrete Slab terminology, limitations, logs, mod removal, and verification evidence.

**Gate Result (pre-research)**: **PASS** — the spec defines scope, bounds, failure recovery, compatibility, and measurable acceptance; technical unknowns were routed to Phase 0 research.

**Gate Result (post-design)**: **PASS** — research identified public `v0.8.6a` primitives and the design contains all unsupported composite guarantees behind fail-closed feasibility gates. No constitution exception is planned.

## Project Structure

### Documentation (this feature)

```text
specs/001-terrain-leveling-tool/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── coi-adapter-contract.md
│   └── gameplay-contract.md
└── tasks.md                    # Created later by /speckit-tasks
```

### Source Code (repository root)

```text
COILevelingTool.slnx
src/
└── COILevelingTool/
    ├── COILevelingTool.csproj
    ├── manifest.json
    ├── readme.txt
    ├── Mod/
    │   └── COILevelingToolMod.cs
    ├── Prototypes/
    │   ├── LevelingStructureData.cs
    │   └── LevelingToolIds.cs
    ├── Placement/
    │   ├── LineSelection.cs
    │   ├── PlacementValidator.cs
    │   └── PlacementFailure.cs
    ├── Terrain/
    │   ├── TerrainLevelingService.cs
    │   ├── TerrainSnapshot.cs
    │   └── TerrainMutationResult.cs
    ├── Construction/
    │   ├── ConstructionCompletionService.cs
    │   ├── ConstructionCostPolicy.cs
    │   └── RefundLedger.cs
    ├── Compatibility/V086/
    │   ├── CapabilityProbeV086.cs
    │   ├── CoiConstructionAdapterV086.cs
    │   ├── CoiPlacementAdapterV086.cs
    │   └── CoiTerrainAdapterV086.cs
    └── Diagnostics/
        └── LevelingDiagnostics.cs

tests/
└── COILevelingTool.Tests/
    ├── COILevelingTool.Tests.csproj
    ├── Unit/
    ├── Contract/
    └── Performance/
```

**Structure Decision**: Use one mod assembly following MaFi's official `ExampleMod` layout, with pure policy/services kept separate from a narrow `Compatibility/V086` layer. A single test project exercises the pure logic and installed-assembly capability contract without introducing runtime dependencies into the distributed mod.
