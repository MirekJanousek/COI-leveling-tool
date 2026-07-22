# Tasks: COI Terrain Leveling Tool

**Input**: Design documents from `/specs/001-terrain-leveling-tool/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: Tests are required by the project constitution for terrain calculations, validation, mutation bounds, failure behavior, compatibility, and every player-visible story. Write each story's automated tests first and confirm they fail before implementing that story.

**Organization**: Tasks are grouped by user story. The feasibility gate in Phase 2 blocks every user story and must not be bypassed with private reflection, Harmony patches, raw terrain writes, or undocumented hooks.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel with other marked tasks in the same phase because it uses different files and has no dependency on their incomplete work
- **[Story]**: Maps the task to a specification user story (`US1`, `US2`, or `US3`)
- Every task names its concrete output path

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the official Update 4.2 mod/test structure and reproducible build/package configuration.

- [X] T001 Create `COILevelingTool.slnx` and scaffold the official `net48` mod project with direct `COI_ROOT` references in `src/COILevelingTool/COILevelingTool.csproj`
- [X] T002 [P] Create the Update 4 manifest with mod ID, version `0.1.0`, DLL, `0.8.6a` version bounds, and conservative save add/remove flags in `src/COILevelingTool/manifest.json`
- [X] T003 [P] Add player installation, supported-build, logging, and pending-structure removal guidance in `src/COILevelingTool/readme.txt`
- [X] T004 [P] Create the MSTest `net48` project and reference the mod project plus installed game assemblies in `tests/COILevelingTool.Tests/COILevelingTool.Tests.csproj`
- [X] T005 [P] Configure shared nullable, warnings, deterministic build, and formatting rules in `Directory.Build.props` and `.editorconfig`
- [X] T006 Add Release deployment and ZIP packaging targets modeled on MaFi's ExampleMod to `src/COILevelingTool/COILevelingTool.csproj`
- [X] T007 Run the empty-skeleton Release build and tests, then record toolchain/version/output evidence in `specs/001-terrain-leveling-tool/feasibility.md`

**Checkpoint**: The empty mod and test project compile against installed game `v0.8.6a`, and packaging produces the expected mod directory.

---

## Phase 2: Foundational Feasibility and Safety Gate (Blocking)

**Purpose**: Prove the public game APIs and establish shared compatibility, diagnostics, terrain, placement, and construction boundaries.

**CRITICAL**: Do not begin any user-story implementation until T008–T020C pass. If a required capability fails, stop and update the spec/plan; do not substitute undocumented integration.

- [X] T008 [P] Define game-independent adapter interfaces and result contracts in `src/COILevelingTool/Compatibility/ICoiTerrainAdapter.cs`, `src/COILevelingTool/Compatibility/ICoiPlacementAdapter.cs`, and `src/COILevelingTool/Compatibility/ICoiConstructionAdapter.cs`
- [X] T009 [P] Implement stable failure reason codes and structured diagnostic context models in `src/COILevelingTool/Diagnostics/LevelingFailureCode.cs` and `src/COILevelingTool/Diagnostics/LevelingDiagnosticContext.cs`
- [X] T010 [P] Write failing installed-assembly contract tests for runtime game version `0.8.6a`, `Mafi.Core 0.8.6.0`, designation size, occupancy, terrain, placement, completion, and removal signatures in `tests/COILevelingTool.Tests/Contract/V086CapabilityContractTests.cs`
- [X] T011 Implement the exact-version capability profile and fail-closed startup probe in `src/COILevelingTool/Compatibility/V086/CapabilityProfileV086.cs` and `src/COILevelingTool/Compatibility/V086/CapabilityProbeV086.cs`
- [X] T012 [P] Create a minimal 1x1 feasibility prototype using the retaining-wall toolbar group/research node and both candidate cost modes in `src/COILevelingTool/Prototypes/LevelingStructureFeasibilityData.cs`
- [X] T013 [P] Write failing four-vertex snapshot, partial-write rollback, bounds, changed-event, and save-tracking spike tests in `tests/COILevelingTool.Tests/Contract/V086TerrainAdapterContractTests.cs`
- [X] T014 Implement the public preserve-relative-layers/no-physics terrain spike behind snapshot/apply/verify/restore operations in `src/COILevelingTool/Compatibility/V086/CoiTerrainAdapterV086.cs`
- [X] T015 [P] Write failing completion forwarding, duplicate-event idempotency, normal entity cleanup, cleanup-failure, and no-additional-charge spike tests in `tests/COILevelingTool.Tests/Contract/V086ConstructionAdapterContractTests.cs`
- [X] T016 Implement construction completion, idempotent entity cleanup, and no-refund failure behavior in `src/COILevelingTool/Compatibility/V086/CoiConstructionAdapterV086.cs`
- [X] T017 [P] Write failing elevation, straight-drag, whole-line revalidation, self-collision, and zero-partial-creation spike tests in `tests/COILevelingTool.Tests/Contract/V086PlacementAdapterContractTests.cs`
- [X] T018 Implement the public placement/elevation/whole-line command spike in `src/COILevelingTool/Compatibility/V086/CoiPlacementAdapterV086.cs`
- [X] T019 Wire capability-gated prototype/dependency registration and feasibility logging in `src/COILevelingTool/Mod/COILevelingToolMod.cs` and `src/COILevelingTool/Diagnostics/LevelingDiagnostics.cs`
- [ ] T020A Write installed-metadata contract tests for a complete public dedicated-controller lifecycle (registration, activation, cursor/preview, absolute height adjustment, deactivation, and simulation-command submission) in `tests/COILevelingTool.Tests/Contract/V086DedicatedPlacementControllerContractTests.cs`; do not treat type presence alone as sufficient
- [ ] T020B Implement a feasibility-only dedicated placement controller with an explicit absolute height anchor and straight-line preview in `src/COILevelingTool/Unity/Placement/V086/LevelingPlacementControllerV086.cs`, using no private reflection or patched stock behavior
- [ ] T020C Execute the in-game `v0.8.6a` feasibility matrix at local terrain offsets of at least -4 and +4, prove preview/commit elevation parity and all-or-none line submission, select Unity-only or five-Concrete-Slab fallback behavior, and document actual Unity pricing and every pass/fail result in `specs/001-terrain-leveling-tool/feasibility.md`; if the public controller lifecycle or any safety-critical gate fails, mark the feature infeasible on `v0.8.6a` and stop

**Checkpoint**: The selected public integration path is proven on `v0.8.6a`, the unsupported path is disabled, and all adapter contract tests pass.

---

## Phase 3: User Story 1 — Level a Frozen Wall Sub-Tile (Priority: P1) MVP

**Goal**: A player completes one valid 1x1 leveling structure in a retaining-wall parent cell; its four vertices reach the chosen elevation, terrain persists, and the temporary structure disappears.

**Independent Test**: On `v0.8.6a`, construct a retaining wall, place one leveling structure on an eligible empty square in the same canonical 4x4 designation cell, finish with Unity at a vehicle-inaccessible site, and verify four-vertex leveling, no other vertex writes, wall integrity, immediate entity removal, save/load persistence, and rollback plus no-refund notification on injected failure.

### Tests for User Story 1

- [ ] T021 [P] [US1] Write failing canonical 4x4 parent mapping, target eligibility, and four-bounding-vertex tests in `tests/COILevelingTool.Tests/Unit/TargetSubTileTests.cs`
- [ ] T022 [P] [US1] Write failing terrain apply/verify/rollback tests with injected failures after writes 1–4 in `tests/COILevelingTool.Tests/Unit/TerrainLevelingServiceTests.cs`
- [ ] T023 [P] [US1] Write failing selected cost-policy and displayed-cost tests in `tests/COILevelingTool.Tests/Unit/ConstructionCostPolicyTests.cs`
- [ ] T024 [P] [US1] Write failing completion lifecycle tests for revalidation, mutation, save notification, idempotent cleanup, and one no-refund failure notification in `tests/COILevelingTool.Tests/Unit/ConstructionCompletionServiceTests.cs`

### Implementation for User Story 1

- [ ] T025 [P] [US1] Implement canonical designation-cell mapping and completed-wall membership in `src/COILevelingTool/Placement/ParentDesignationCell.cs`
- [ ] T026 [P] [US1] Implement the 1x1 target model, four-vertex footprint, bounds inputs, and eligibility state in `src/COILevelingTool/Placement/TargetSubTile.cs`
- [ ] T027 [P] [US1] Implement deterministic unique-vertex snapshots and mutation outcomes in `src/COILevelingTool/Terrain/TerrainSnapshot.cs` and `src/COILevelingTool/Terrain/TerrainMutationResult.cs`
- [ ] T028 [P] [US1] Implement the selected Unity/five-Concrete-Slab cost policy and displayed cost in `src/COILevelingTool/Construction/ConstructionCostPolicy.cs`
- [ ] T029 [US1] Implement single-target wall, occupancy, reservation, bounds, elevation, and four-vertex preflight in `src/COILevelingTool/Placement/PlacementValidator.cs`
- [ ] T030 [US1] Implement simulation-thread snapshot/apply/verify/rollback orchestration without terrain physics in `src/COILevelingTool/Terrain/TerrainLevelingService.cs`
- [ ] T031 [US1] Implement completion event filtering, target revalidation, idempotency, immediate entity removal, and one no-refund failure notification in `src/COILevelingTool/Construction/ConstructionCompletionService.cs`
- [ ] T032 [US1] Register the US1 services and completion subscription through the verified capability profile in `src/COILevelingTool/Mod/COILevelingToolMod.cs`
- [ ] T033 [US1] Execute and record the independent single-square, inaccessible-Unity, save/load, current-height, and injected-failure verification in `specs/001-terrain-leveling-tool/verification/us1-leveling.md`

**Checkpoint**: User Story 1 works independently as a single-square MVP and all US1 automated/in-game tests pass.

---

## Phase 4: User Story 2 — Find, Configure, and Drag the Tool (Priority: P2)

**Goal**: The tool is unlocked and displayed with retaining walls, previews a chosen elevation, and creates one independent 1x1 structure per valid square in a straight dragged line.

**Independent Test**: Open the Terraforming/retaining-wall menu, select the tool, adjust elevation, preview one square and a straight line, commit a wholly valid line, and verify the created structure count and shared elevation match the preview.

### Tests for User Story 2

- [ ] T034 [P] [US2] Write failing axis, consecutiveness, no-turn/no-reversal, shared-elevation, and vertex-deduplication tests in `tests/COILevelingTool.Tests/Unit/LineSelectionTests.cs`
- [ ] T035 [P] [US2] Write failing toolbar-group, retaining-wall unlock, 1x1 layout, elevation, and selected-cost contract tests in `tests/COILevelingTool.Tests/Contract/LevelingStructurePrototypeTests.cs`
- [ ] T036 [P] [US2] Write failing preview/commit parity and whole-line zero-partial-creation tests in `tests/COILevelingTool.Tests/Unit/LinePlacementServiceTests.cs`

### Implementation for User Story 2

- [ ] T037 [P] [US2] Implement ordered axis-aligned line selection, shared elevation, and unique-vertex enumeration in `src/COILevelingTool/Placement/LineSelection.cs`
- [ ] T038 [P] [US2] Define stable prototype, research, localization, and diagnostic IDs in `src/COILevelingTool/Prototypes/LevelingToolIds.cs`
- [ ] T039 [US2] Replace the feasibility prototype with the final 1x1 layout, selected cost mode, retaining-wall group, existing research unlock, elevation, and game-native preview assets in `src/COILevelingTool/Prototypes/LevelingStructureData.cs`
- [ ] T040 [US2] Implement whole-line preview validation and simulation-thread all-or-none commit orchestration in `src/COILevelingTool/Placement/LinePlacementService.cs`
- [ ] T041 [US2] Implement localized preview and invalid-placement presentation using ordinary game validation feedback in `src/COILevelingTool/Placement/PlacementPresentation.cs`
- [ ] T042 [US2] Wire final prototype registration, elevation controls, line placement, and cost display in `src/COILevelingTool/Mod/COILevelingToolMod.cs`
- [ ] T043 [US2] Execute and record menu, unlock, elevation, single preview, valid drag, cancellation, vehicle warning, and created-count verification in `specs/001-terrain-leveling-tool/verification/us2-placement.md`

**Checkpoint**: User Story 2 is independently demonstrable from menu discovery through valid straight-line construction.

---

## Phase 5: User Story 3 — Reject Unsafe Placement and Recover Safely (Priority: P3)

**Goal**: Invalid targets and incompatible environments fail closed with clear feedback; terrain, structures, and player balances remain safe.

**Independent Test**: Attempt every prohibited placement and injected failure on `v0.8.6a`, including one invalid square in a dragged line, and verify zero partial construction, no unintended terrain/structure changes, diagnostics, one no-refund notification, and operation disablement after unrecoverable rollback or cleanup failure.

### Tests for User Story 3

- [ ] T044 [P] [US3] Write failing boundary, off-limits, occupied, reserved, wall-overlap, no-wall-parent, incomplete-wall, invalid-elevation, self-collision, and non-linear-selection tests in `tests/COILevelingTool.Tests/Unit/PlacementValidatorSafetyTests.cs`
- [ ] T045 [P] [US3] Write failing unsupported-version, missing-capability, single-notification, and normal-game-preservation tests in `tests/COILevelingTool.Tests/Unit/CompatibilityFailureTests.cs`
- [ ] T046 [P] [US3] Write failing rollback-failure, cleanup-failure, duplicate-notification, and global-disable tests in `tests/COILevelingTool.Tests/Unit/FailureRecoveryTests.cs`
- [ ] T047 [P] [US3] Write failing structured reason-code and required-context logging tests in `tests/COILevelingTool.Tests/Unit/LevelingDiagnosticsTests.cs`

### Implementation for User Story 3

- [ ] T048 [US3] Complete all negative validation branches and localized failure results in `src/COILevelingTool/Placement/PlacementValidator.cs` and `src/COILevelingTool/Placement/PlacementFailure.cs`
- [ ] T049 [US3] Enforce final commit revalidation, whole-batch cleanup, and zero-cost/zero-terrain partial failure in `src/COILevelingTool/Compatibility/V086/CoiPlacementAdapterV086.cs`
- [ ] T050 [US3] Implement rollback/cleanup escalation, operation disablement, one no-refund player notification, and structured logs in `src/COILevelingTool/Construction/ConstructionCompletionService.cs` and `src/COILevelingTool/Diagnostics/LevelingDiagnostics.cs`
- [ ] T051 [US3] Enforce unsupported-version and missing-capability fail-closed behavior without disturbing base-game registration in `src/COILevelingTool/Mod/COILevelingToolMod.cs`
- [ ] T052 [US3] Execute and record the full unsafe-placement, invalid-line, map-edge, occupancy, other-terrain-mod, rollback, cleanup, no-refund notification, and unsupported-version matrix in `specs/001-terrain-leveling-tool/verification/us3-safety.md`

**Checkpoint**: User Story 3 passes all negative/fault-injection tests, and every unsafe condition produces the specified unchanged or safely recovered outcome.

---

## Phase 6: Polish and Cross-Cutting Release Readiness

**Purpose**: Verify performance, compatibility, documentation, removal, packaging, and the complete quickstart scenario.

- [ ] T053 [P] Add 100-sub-tile validation benchmarks and maximum-supported-drag measurements in `tests/COILevelingTool.Tests/Performance/PlacementPerformanceTests.cs`
- [ ] T054 Optimize only measured placement/vertex hot paths to meet the 5 ms isolated and 16 ms in-game targets in `src/COILevelingTool/Placement/LinePlacementService.cs` and record before/after evidence in `specs/001-terrain-leveling-tool/verification/performance.md`
- [ ] T055 [P] Add manifest schema/version/add-to-save/remove-from-save contract coverage in `tests/COILevelingTool.Tests/Contract/ManifestContractTests.cs`
- [ ] T056 Verify clean, planned, and under-construction mod add/remove behavior and set the proven manifest flags in `src/COILevelingTool/manifest.json`, documenting limitations in `src/COILevelingTool/readme.txt`
- [ ] T057 [P] Update installation, controls, Concrete Slab terminology, shared-vertex slope behavior, costs, compatibility, logs, and known limitations in `README.md`
- [ ] T058 Run the full Release build and automated suite and record commands/results in `specs/001-terrain-leveling-tool/verification/automated-tests.md`
- [ ] T059 Execute every step in `specs/001-terrain-leveling-tool/quickstart.md` on `v0.8.6a` and record consolidated screenshots/log evidence in `specs/001-terrain-leveling-tool/verification/release-check.md`
- [ ] T060 Produce and inspect the distributable ZIP, verifying root folder, manifest, DLL, readme, version bounds, and absence of development-only files in `artifacts/COILevelingTool-0.1.0.zip`

**Checkpoint**: Release package, automated evidence, in-game evidence, compatibility statements, and documentation are complete.

---

## Dependencies and Execution Order

### Phase Dependencies

- **Phase 1 — Setup**: No dependencies.
- **Phase 2 — Foundational gate**: Depends on Phase 1 and blocks all story work. T020A–T020C are the explicit go/no-go decision.
- **Phase 3 — US1**: Depends on Phase 2 only; delivers the single-square MVP.
- **Phase 4 — US2**: Depends on Phase 2 only for independent preview/placement work, but merges most safely after US1 because both wire `COILevelingToolMod.cs`.
- **Phase 5 — US3**: Depends on Phase 2 only for independent validation/failure work, but final integration follows US1/US2 because it hardens their services.
- **Phase 6 — Polish**: Depends on all stories selected for release.

### User Story Dependency Graph

```text
Setup
  -> Foundational feasibility/safety gate
       -> US1: single-square leveling (MVP)
       -> US2: discovery, elevation, and straight drag
       -> US3: unsafe rejection and recovery
            -> Polish and release readiness
```

US1, US2, and US3 are independently testable after the foundational gate. Recommended integration order is `US1 -> US2 -> US3` because US3 hardens shared behavior and the stories touch common registration files.

### Within Each Story

- Write the story's tests first and confirm they fail for the expected reason.
- Implement models/value objects before validators and services.
- Implement pure orchestration before the versioned adapter wiring.
- Run automated tests before the in-game verification task.
- Do not mark a story complete until its verification evidence file is populated.

## Parallel Opportunities

### Setup

After T001 establishes names/paths, T002–T005 can proceed in parallel. T006 then integrates packaging; T007 verifies the phase.

### Foundational

T008–T010 can proceed in parallel. After the shared contracts exist, the prototype (T012), terrain spike (T013–T014), construction spike (T015–T016), and placement spike (T017–T018) are separable workstreams. T019 and T020A–T020C integrate and decide the gate.

### User Story 1

```text
T021 TargetSubTile tests
T022 TerrainLevelingService tests
T023 RefundLedger tests
T024 ConstructionCompletionService tests
```

These tests can be written concurrently. T025–T028 then implement different model files in parallel before T029–T032 integrate them.

### User Story 2

```text
T034 LineSelection tests
T035 Prototype contract tests
T036 LinePlacementService tests
```

T037 and T038 can then proceed in parallel; T039–T042 integrate the story.

### User Story 3

```text
T044 Placement safety tests
T045 Compatibility failure tests
T046 Compensation failure tests
T047 Diagnostics tests
```

These tests use different files and can be written concurrently before T048–T051 harden the shared implementation.

## Implementation Strategy

### MVP First

1. Complete Setup (T001–T007).
2. Complete the mandatory feasibility/safety gate (T008–T020C).
3. Complete User Story 1 (T021–T033).
4. Stop and validate the single-square MVP independently before adding drag placement.

### Incremental Delivery

1. **Foundation**: Confirm the public Update 4.2 integration is safe and supported.
2. **US1**: Deliver one-square terrain leveling, cleanup, persistence, and explicit no-refund failure handling.
3. **US2**: Add menu/research polish, elevation preview, and straight-line placement.
4. **US3**: Complete negative handling, fail-closed compatibility, and fault recovery.
5. **Release**: Measure, document, package, and rerun the complete sandbox matrix.

## Notes

- `[P]` means file-level parallelism only; avoid concurrent edits to shared registration/service files.
- The canonical fallback resource is `Concrete Slab`, not the obsolete in-game Bricks product.
- The four vertices bounding a selected square are shared with neighboring terrain; tests compare written vertex coordinates and document possible adjacent visual slopes.
- T020C is a hard stop. Failure to provide unrestricted absolute elevation through a complete public controller lifecycle is not eligible for a +/-1 scope fallback. Unsupported fractional Unity, line atomicity, terrain rollback, or normal cleanup must be resolved by revising the product/plan, never by undocumented patching.
- Commit after each completed task or coherent test/implementation pair.
