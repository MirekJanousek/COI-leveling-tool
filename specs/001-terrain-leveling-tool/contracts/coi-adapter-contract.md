# Captain of Industry Adapter Contract (`v0.8.6a`)

All direct game API calls belong in `Compatibility/V086`. Pure placement, terrain, and completion services depend on these contracts rather than concrete MaFi types.

## Capability Probe

```text
Probe() -> CapabilityProfileV086
```

Must verify game build, core assembly version, grid constant, retaining-wall prototype/entity access, occupancy and bounds queries, terrain read/write/notification behavior, placement extension points, completion event, and normal entity removal.

The placement capability is enabled only when the mod can publicly register and activate a dedicated placement controller, display previews at an explicit absolute height anchor, adjust that anchor beyond the stock machine placer's local +/-1 range, and submit one simulation command for the complete selection.

Failure is non-throwing at game startup: return a disabled profile, log missing signatures, and prevent tool registration or activation.

## Eligibility Adapter

```text
ResolveParent(targetSquare) -> ParentDesignationCell
FindConstructedRetainingWalls(parent) -> walls
ValidateOccupancy(targetSquare) -> PlacementValidationResult
ValidateBounds(targetSquare, fourVertices) -> PlacementValidationResult
```

Rules:

- Parent mapping uses the game's terrain-designation size.
- Entity queries cover all sixteen squares in the parent.
- Only a fully constructed retaining wall grants eligibility.
- The target itself must be empty and unreserved under normal placement validation.
- Bounds are rejected, never clamped.

## Placement Adapter

```text
Preview(selection, elevation) -> preview
Commit(selection, elevation) -> batchResult
```

Rules:

- Preview validation is advisory; commit repeats all checks on the simulation thread.
- The requested elevation is an explicit absolute height shared by the selection; it is not derived from or clamped to the stock machine placer's local terrain-relative range.
- Automated metadata tests and an in-game spike must prove preview/commit parity at least four height steps above and below local terrain.
- Commit creates zero entities until the whole selection and self-collision checks succeed.
- A batch failure after creation begins must remove all entities created by that batch before construction begins, so no construction cost has been charged.
- Internal UI patching and private reflection are prohibited; absent a complete public controller lifecycle disables placement and fails the feasibility gate.

## Terrain Adapter

```text
Snapshot(fourVertices) -> TerrainSnapshot
ApplyWithoutPhysics(snapshot, requestedElevation) -> TerrainMutationResult
Restore(snapshot) -> TerrainMutationResult
Verify(fourVertices, expectedElevation) -> bool
```

Rules:

- Execute only on the simulation thread.
- Apply and restore use supported high-level APIs preserving relative layers and suppressing uncontrolled terrain physics.
- Deduplicate shared vertices deterministically.
- Notify changed terrain exactly as required by the chosen setter; do not double-notify.
- A success is returned only after all four stored heights verify.
- Restore is attempted after any partial write, exception, or verification mismatch.

## Construction Adapter

```text
OnConstructed(entity) -> event
RemoveTemporaryEntity(entity, reason) -> result
```

Rules:

- Filter completion events by the exact leveling prototype.
- Removal must invoke normal entity destruction cleanup and be safe from duplicate completion events.
- A failed completed operation does not refund its already-consumed construction cost and must make no additional charge.

## Diagnostics Contract

Every rejected/failed operation logs a stable reason code and, where available:

- game and core assembly versions;
- entity/prototype ID;
- target square and canonical parent origin;
- four bounding vertices and requested elevation;
- construction mode and displayed cost;
- write, verification, rollback, removal, and notification results.

Player-facing errors use localized ordinary validation messages; post-construction failures also use one actionable notification.
