# Data Model: COI Terrain Leveling Tool

The mod stores no custom persistent records. These models describe transient simulation state, validation results, and compatibility boundaries; authoritative terrain and construction persistence remains owned by Captain of Industry.

## Canonical Coordinates

### ParentDesignationCell

Represents the canonical 4x4 terrain-designation cell used for retaining-wall eligibility.

| Field | Meaning |
|---|---|
| `Origin` | Canonical lower/grid origin derived with `TerrainDesignation.SIZE_TILES` |
| `SizeTiles` | Runtime-asserted designation size (`4` on game `v0.8.6a`) |
| `Area` | Sixteen terrain squares within the cell |
| `RetainingWalls` | Fully constructed retaining-wall entities found in the area |

Validation rules:

- The entire parent area must be a valid canonical cell; boundary cells are rejected if the required target vertices are off-limits.
- At least one fully constructed retaining wall must occupy a square in the cell.
- A planned or under-construction retaining wall does not grant eligibility.

### TargetSubTile

Represents one player-selected 1x1 terrain square.

| Field | Meaning |
|---|---|
| `Coordinate` | Game terrain-square coordinate |
| `Parent` | Canonical `ParentDesignationCell` |
| `BoundingVertices` | Exactly four unique terrain vertices: origin, +X, +Y, +X+Y |
| `RequestedElevation` | Placement elevation in game height units |
| `Occupancy` | Existing entity/reservation validation result |
| `WallEligibility` | Whether the parent contains a wall and the target does not overlap it |
| `BoundsStatus` | Whether square and four vertices are within playable limits |

Validation rules:

- Target must be empty and unreserved under normal layout validation.
- Target must not overlap a retaining wall or any other entity.
- Parent must contain a fully constructed retaining wall.
- All four vertices and requested elevation must be valid and representable.
- Setting four shared vertices may alter the visual slope of neighboring squares; no vertex outside `BoundingVertices` may be written.

## Placement Models

### LineSelection

An ordered selection of one or more consecutive target sub-tiles sharing one elevation.

| Field | Meaning |
|---|---|
| `Targets` | Ordered, non-empty unique `TargetSubTile` list |
| `Axis` | Single X or Y grid axis |
| `RequestedElevation` | Elevation shared by all targets |
| `ValidationResults` | One result per target plus line-level self-collision result |

Validation rules:

- Consecutive coordinates differ by exactly one unit on one axis.
- Direction never turns or reverses.
- Every target independently satisfies eligibility, occupancy, bounds, and elevation rules.
- Shared vertices between adjacent targets are deduplicated for snapshots and deterministic writes.
- The entire selection is accepted or rejected before any entity is created.

### PlacementValidationResult

| Field | Meaning |
|---|---|
| `IsValid` | True only when every check succeeds |
| `ReasonCode` | Stable machine-readable failure code |
| `PlayerMessage` | Localized placement feedback |
| `FailedTarget` | Optional coordinate responsible for failure |

Representative reason codes: `UnsupportedGameVersion`, `CapabilityMissing`, `OutsideMap`, `InvalidElevation`, `Occupied`, `Reserved`, `WallOverlap`, `NoWallInParent`, `WallNotConstructed`, `NonLinearSelection`, `SelfCollision`, `TerrainUnsafe`.

## Construction Models

### LevelingStructurePrototype

| Field | Meaning |
|---|---|
| `PrototypeId` | Stable mod-owned prototype ID |
| `Footprint` | Exact 1x1 layout |
| `ToolbarGroup` | Existing Terraforming/retaining-wall toolbar placement |
| `ResearchUnlock` | Existing retaining-wall research node |
| `CostPolicy` | Preferred Unity-only mode or supported Concrete Slab fallback |
| `ElevationEnabled` | Capability proven during feasibility spike |
| `DragEnabled` | Capability proven during feasibility spike |

### ConstructionCostPolicy

Two mutually exclusive runtime modes:

1. `UnityOnly`: material-free, supported public Unity cost path; target `0.05` Unity only if representable.
2. `ConcreteFallback`: exactly five Concrete Slabs plus native pay-with-Unity quick build; actual Unity cost is engine-calculated and shown to the player.

If neither mode can meet the contract through public APIs, capability validation fails and the tool remains disabled.

### RefundLedger

Transient, idempotent accounting snapshot captured before completion.

| Field | Meaning |
|---|---|
| `EntityId` | Leveling structure being completed |
| `ProductsCharged` | Exact product IDs and quantities actually charged |
| `UnityCharged` | Exact UPoints amount actually charged |
| `CompensationState` | `NotRequired`, `Pending`, `Completed`, or `Failed` |

Validation rules:

- Ledger is created before construction progress and charge information disappears.
- Compensation may execute at most once per entity.
- Full refund restores exact captured products and Unity, not recomputed estimates.
- A failed compensation disables further terrain operations and emits a critical diagnostic.

## Terrain Models

### TerrainSnapshot

| Field | Meaning |
|---|---|
| `VertexHeights` | Original height for each unique selected vertex |
| `RequestedElevation` | Target height |
| `GameVersion` | `0.8.6a` |
| `AssemblyVersion` | `Mafi.Core 0.8.6.0` |

### LevelingOperation

One structure completion affecting one target sub-tile.

| Field | Meaning |
|---|---|
| `EntityId` | Completed temporary structure |
| `Target` | Validated target sub-tile |
| `Snapshot` | Original four vertex heights |
| `RefundLedger` | Exact cost compensation record |
| `State` | Current lifecycle state |
| `Result` | Optional success/failure and diagnostic details |

State transitions:

```text
Planned
  -> ConstructionPending
  -> Preflight
     -> Mutating
        -> Verifying
           -> Applied -> EntityRemoved
           -> RollingBack -> FailedRefunded -> EntityRemoved
           -> RollingBack -> CompensationFailed -> Disabled
     -> RejectedBeforeMutation -> FailedRefunded -> EntityRemoved
  -> Cancelled
```

Invariants:

- Mutation begins only after simulation-thread preflight succeeds.
- Success means all four target vertices equal the requested elevation and the temporary entity is removed.
- Failure means all original heights are restored, the temporary entity is removed, and exact costs are refunded.
- No completed operation leaves mod-owned persistent state.

## Compatibility Models

### CapabilityProfileV086

Startup result proving required versioned integrations.

| Capability | Required evidence |
|---|---|
| Version | Game `v0.8.6a`; core assembly `0.8.6.0` |
| Grid | Terrain designation size and canonical mapping |
| Occupancy | Static entity and retaining-wall queries |
| Bounds | Square and vertex limit checks |
| Terrain | Read, preserve-relative/no-physics write, notify/save behavior |
| Placement | Elevation, straight drag, simulation-thread whole-line validation |
| Completion | Constructed event and safe entity removal |
| Accounting | Exact charge capture and idempotent product/Unity compensation |

Any missing required capability produces a disabled profile and one actionable player/log diagnostic.
