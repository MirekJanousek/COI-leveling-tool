# Feature Specification: COI Terrain Leveling Tool

> **Implementation status (2026-07-22): Phase 2 revision approved.** Because the pinned public APIs do not expose an exact completion-time cost ledger, completed construction costs are not refunded after a terrain failure. Terrain rollback, temporary-entity cleanup, notification, and fail-closed disablement remain mandatory.

**Feature Branch**: `001-terrain-leveling-tool`

**Created**: 2026-07-21

**Status**: Draft

**Input**: User description: "Develop COI Leveling Tool, a Captain of Industry mod that lets players level terrain beside retaining walls by placing a temporary 1x1 structure at a chosen elevation. The structure levels its tile when built, disappears immediately afterward, and is available in the retaining walls menu."

## Clarifications

### Session 2026-07-21

- Q: When a dragged line contains one or more invalid sub-tiles, should any part of the line be constructed? → A: Reject the entire dragged line; no selected sub-tile is constructed or leveled.
- Q: How should the leveling structure be constructed? → A: Prefer Unity-only instant construction with no materials; if unsupported, require five concrete bricks per structure and allow Unity quick-build at approximately 0.05 Unity. Vehicle-access warnings may appear but must not prevent Unity completion.
- Q: Where may the leveling structure be placed relative to retaining walls? → A: Only on eligible empty sub-tiles associated with retaining walls; it cannot overlap retaining walls or be used elsewhere.
- Q: What makes a sub-tile eligible through proximity to a retaining wall? → A: It shares the same parent tile as a retaining wall and does not overlap the wall.
- Q: If Unity construction completes but terrain leveling fails, what happens to the structure and spent costs? → A: Remove the temporary structure and fully refund all Unity and any fallback materials. Superseded by the 2026-07-22 feasibility decision below.

### Session 2026-07-22

- Q: The public API cannot prove an exact post-completion Unity/material refund. Which behavior should the product use? → A: Do not refund completed construction costs after a terrain failure; still restore terrain, remove the temporary structure, notify the player, and disable further operations if rollback or cleanup fails.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Level a Frozen Wall Tile (Priority: P1)

As a player, I can select a 1x1 leveling structure, set its elevation, and build it on an empty sub-tile whose parent tile contains a retaining wall so that the terrain underneath is leveled to the chosen height.

**Why this priority**: This solves the core problem: terrain on a retaining-wall tile cannot be leveled through normal gameplay because the base game freezes that terrain.

**Independent Test**: On a supported game version, place a retaining wall, select the leveling structure, choose a valid elevation, and build it on one empty sub-tile in the wall's parent tile without overlapping the wall. The sub-tile reaches the selected elevation and remains usable after the temporary structure disappears.

**Acceptance Scenarios**:

1. **Given** a valid empty 1x1 target sub-tile whose parent tile contains a retaining wall, **When** the player builds the leveling structure at a valid selected elevation, **Then** all terrain within that sub-tile is leveled to that elevation and the structure disappears immediately after completing its effect.
2. **Given** a successfully leveled sub-tile, **When** the player saves and reloads the game, **Then** the resulting terrain elevation remains and no leveling structure is present.
3. **Given** terrain in the same sub-tile is uneven, **When** the leveling operation succeeds, **Then** both raised and lowered portions finish at the selected elevation.
4. **Given** the target is unreachable by construction vehicles, **When** the player completes the structure using Unity, **Then** construction succeeds without vehicle delivery and the tile is leveled normally.

---

### User Story 2 - Find and Configure the Tool (Priority: P2)

As a player working with retaining walls, I can find the leveling structure alongside retaining walls and use the game's familiar elevation controls to preview the intended placement and height before committing construction.

**Why this priority**: The tool must be discoverable and predictable enough to use without disrupting the player's retaining-wall workflow.

**Independent Test**: Open the retaining-wall construction menu, select the leveling structure, adjust its elevation, and verify that the player can preview either one 1x1 placement or a straight dragged line of consecutive 1x1 placements at the chosen elevation before construction.

**Acceptance Scenarios**:

1. **Given** the mod is enabled in a supported game, **When** the player opens the menu containing retaining walls, **Then** the leveling structure is available there as a distinct construction option.
2. **Given** the leveling structure is selected, **When** the player adjusts its elevation before placement, **Then** the preview reflects the selected height and clearly identifies every affected tile.
3. **Given** the player cancels before construction completes, **When** the cancellation is accepted, **Then** no terrain is changed.
4. **Given** the leveling structure is selected, **When** the player drags in one straight direction across consecutive sub-tiles, **Then** the preview shows one 1x1 structure on every selected sub-tile at the same chosen elevation.

---

### User Story 3 - Reject Unsafe Placement (Priority: P3)

As a player, I receive normal placement feedback when leveling would be invalid or unsafe, and the operation leaves the terrain and existing structures unchanged.

**Why this priority**: Terrain mutation must not damage saves, structures, map boundaries, or tiles beyond the player's explicit selection.

**Independent Test**: Attempt placement in each prohibited situation and verify that construction cannot complete, a comprehensible rejection is shown, and terrain and structures remain unchanged.

**Acceptance Scenarios**:

1. **Given** a target sub-tile containing a conflicting building or structure, **When** the player attempts to place or complete the leveling structure, **Then** the operation is rejected without changing terrain or the conflicting object.
2. **Given** a target at or beyond the playable map boundary, **When** the player attempts placement, **Then** the operation cannot mutate terrain outside the playable map.
3. **Given** the selected elevation cannot be applied safely, **When** construction is attempted, **Then** the operation fails atomically and communicates that it did not complete.
4. **Given** a dragged line containing at least one invalid sub-tile, **When** the player attempts to commit the line, **Then** the entire line is rejected and no selected sub-tile is constructed or leveled.
5. **Given** a target sub-tile that overlaps a retaining wall or whose parent tile contains no retaining wall, **When** the player attempts placement, **Then** the target is rejected without changing terrain or structures.
6. **Given** a completed leveling structure whose terrain mutation fails, **When** failure handling completes, **Then** the temporary structure is removed, the sub-tile remains unchanged, the player is notified that completed construction costs are not refunded, and no additional charge is made.

### Edge Cases

- A target sub-tile lies at the playable map boundary or its mutation footprint cannot be proven to stay inside the map.
- A target sub-tile contains, overlaps, or is reserved by another building, transport, retaining wall, blueprint, or construction job.
- A dragged line extends onto one or more sub-tiles whose parent tiles contain no retaining wall.
- Multiple leveling structures are planned on the same sub-tile or on adjacent sub-tiles at different elevations.
- A dragged line crosses an invalid, occupied, reserved, or out-of-bounds tile.
- A drag reverses direction, turns a corner, or ceases to describe one straight line of consecutive tiles.
- The target elevation equals the tile's current elevation.
- The selected elevation is outside the game's permitted construction range or would produce an invalid terrain state.
- Construction is cancelled, interrupted, removed, or invalidated before the terrain operation completes.
- The game is saved or closed while the leveling structure is planned or under construction.
- A terrain mutation fails partway through or cannot be verified as safe before execution.
- A leveling structure completes but its terrain mutation fails after Unity or fallback materials have been spent.
- Construction vehicles cannot reach a valid placement; the game may warn about access, but Unity completion remains available.
- The pinned game version cannot represent a Unity-only, material-free structure or cannot represent a cost of exactly 0.05 Unity.
- Another mod changes construction menus, retaining walls, placement rules, or terrain behavior.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The mod MUST provide a distinct buildable leveling structure with an exact 1x1-sub-tile footprint.
- **FR-002**: The leveling structure MUST be available in the same player-facing construction menu or category as retaining walls.
- **FR-003**: Players MUST be able to select the leveling structure's elevation before committing its construction, within the elevation range allowed by the game.
- **FR-004**: Before commitment, the placement preview MUST identify every sub-tile that may be changed and the selected elevation.
- **FR-005**: On successful construction, the mod MUST set all mutable terrain within the selected empty sub-tile's boundary to the selected elevation when its parent tile contains a retaining wall.
- **FR-006**: The mod MUST NOT mutate terrain outside the explicitly selected sub-tile or straight line of consecutive sub-tiles.
- **FR-007**: The leveling structure MUST disappear immediately after a successful terrain change, without requiring player demolition or leaving a persistent building, blueprint, or construction remnant.
- **FR-008**: The resulting terrain elevation MUST persist through the game's normal save and load cycle.
- **FR-009**: Before changing terrain, the mod MUST validate map boundaries, elevation validity, conflicting occupancy or reservations, and whether the complete requested mutation can be performed safely.
- **FR-010**: If validation fails, construction is cancelled or interrupted before mutation, or a safe outcome cannot be guaranteed, the mod MUST leave terrain and existing structures unchanged.
- **FR-011**: Each structure's terrain mutation MUST be atomic from the player's perspective: either the whole mutable area within that structure's sub-tile reaches the requested elevation or that sub-tile does not change.
- **FR-012**: Repeated or adjacent leveling operations MUST affect only each explicitly selected sub-tile and MUST produce deterministic results for the same initial terrain and chosen elevation.
- **FR-013**: The leveling structure MUST be completable using Unity without construction-vehicle access or material delivery.
- **FR-014**: The mod MUST provide player-visible placement or failure feedback consistent with ordinary construction behavior when an operation is invalid.
- **FR-015**: The supported game version MUST be pinned during planning and verified before release; unsupported versions MUST be declared in release information rather than silently represented as supported.
- **FR-016**: Required dependencies and known incompatibilities MUST be declared with the mod, and detected compatibility failures MUST prevent unsafe terrain mutation and provide diagnostic information.
- **FR-017**: The feature MUST directly support leveling empty sub-tiles whose parent tiles contain retaining walls; overlap with retaining walls, placement in parent tiles without retaining walls, freeform area terraforming, non-straight multi-sub-tile placement, and removal or alteration of retaining walls are outside scope.
- **FR-018**: Players MUST be able to drag the leveling structure in one straight direction to select a consecutive line of 1x1 structures on sub-tiles, all sharing the chosen elevation and subject to the game's valid placement boundary and supported drag length.
- **FR-019**: The mod MUST validate every sub-tile in a dragged line before committing any part of that line.
- **FR-020**: If any sub-tile in a dragged line is invalid, the mod MUST reject the entire line without constructing a leveling structure or changing terrain on any selected sub-tile.
- **FR-021**: If supported by the pinned game version, each leveling structure MUST use Unity-only instant construction with no material cost.
- **FR-022**: If Unity-only material-free construction is not supported, each leveling structure MUST instead require exactly five concrete bricks and MUST retain the game's Unity quick-build option.
- **FR-023**: In the fallback construction mode, the target quick-build cost MUST be 0.05 Unity per structure when configurable; otherwise, the mod MUST use the nearest representable or game-calculated Unity cost and document the actual value.
- **FR-024**: Vehicle-access warnings MAY be displayed for unreachable placements, but lack of vehicle access MUST NOT prevent the player from completing the structure with Unity.
- **FR-025**: The active construction mode, material requirement, and Unity cost MUST be visible to the player before committing placement.
- **FR-026**: A target sub-tile MUST be empty, MUST NOT overlap a retaining wall, and MUST share a parent tile with at least one retaining wall; a sub-tile failing any of these conditions MUST be invalid.
- **FR-027**: Every sub-tile in a dragged line MUST independently satisfy the retaining-wall parent-tile rule; if any sub-tile does not, the entire line MUST be rejected under FR-020.
- **FR-028**: If a completed leveling structure cannot finish its terrain mutation successfully, the mod MUST restore its original terrain, remove the temporary structure, and notify the player that completed construction costs are not refunded. Failure handling MUST NOT make any additional charge.

### Key Entities

- **Leveling Structure**: A temporary 1x1-sub-tile construction instance selected individually or as part of a dragged line; its relevant attributes are target sub-tile, chosen elevation, placement state, construction state, and validation outcome.
- **Leveling Placement**: One committed selection consisting of either a single target sub-tile or a straight line of consecutive target sub-tiles at one chosen elevation; the placement is accepted or rejected as a whole.
- **Leveling Operation**: A requested atomic terrain change for one completed leveling structure; it relates that structure to one target sub-tile and records whether validation and completion succeeded.
- **Target Sub-Tile**: An empty player-visible 1x1 terrain boundary eligible for mutation because its parent tile contains a retaining wall, including its current terrain profile, occupancy, wall overlap, map-boundary status, and requested elevation.
- **Parent Tile**: The larger terrain grouping used to establish eligibility; it must contain at least one retaining wall for its empty, non-overlapping sub-tiles to accept the leveling structure.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of successful test placements, terrain inside the selected sub-tile finishes at the chosen elevation, no terrain outside that sub-tile changes, and the temporary structure is absent immediately afterward.
- **SC-002**: At least 90% of first-time test players can locate the tool, select an elevation, and complete one valid leveling operation beside a retaining wall within two minutes without external instructions.
- **SC-003**: Across at least 50 valid operations covering raising, lowering, repeated placement, and adjacent tiles, every operation produces the same expected result for the same starting state and selection.
- **SC-004**: Across all defined invalid-placement and interrupted-operation tests, 100% leave terrain and pre-existing structures unchanged.
- **SC-005**: After saving and reloading each successful test case, 100% retain the leveled terrain and contain no persistent leveling structure or construction remnant.
- **SC-006**: Players can visually identify every affected tile and the intended elevation before committing construction in 100% of supported placement scenarios.
- **SC-007**: In 100% of tests where a dragged line contains at least one invalid sub-tile, the entire line is rejected and no selected sub-tile is constructed or changed.
- **SC-008**: In 100% of valid straight-line drag tests, the number of temporary structures created equals the number of consecutive sub-tiles previewed, and every selected sub-tile reaches the shared chosen elevation.
- **SC-009**: In 100% of valid placements unreachable by construction vehicles, the player can complete every leveling structure using Unity and obtain the same terrain result as at a reachable location.
- **SC-010**: In 100% of tests on sub-tiles overlapping retaining walls or belonging to parent tiles without retaining walls, placement is rejected and no terrain or structure changes.
- **SC-011**: In 100% of simulated post-construction terrain failures, the target sub-tile remains unchanged, the temporary structure is absent, no additional charge is made, and the player receives one actionable failure notification stating that completed construction costs are not refunded.

## Assumptions

- The initial release targets single-player use; multiplayer-specific synchronization behavior is outside scope unless the selected game version provides it transparently for ordinary construction and terrain changes.
- The target version is the latest publicly released stable, non-beta Captain of Industry version available when implementation planning begins; planning will record the exact version and applicable official modding guidance.
- Each leveling structure affects exactly one game terrain sub-tile; one all-or-nothing placement selection may contain either one structure or a straight line of consecutive 1x1 structures sharing one elevation.
- After a valid dragged line is placed, its 1x1 structures are separate construction jobs; each levels its own sub-tile when that individual job completes using Unity.
- "Immediately after building" means after the terrain change succeeds within the completed construction action, before the player must take another action.
- Planning will verify whether the pinned game version supports a material-free structure completed solely with Unity and whether a 0.05 Unity quick-build cost can be represented or configured.
- Existing retaining walls are neither removed nor repositioned by the leveling operation.
- A retaining wall is always an occupancy conflict for the leveling structure; the tool never overlaps it.
- Natural terrain seams or slopes outside the selected sub-tile are not automatically blended; players may level neighboring eligible sub-tiles separately.

## Mod Integration Constraints *(mandatory)*

- **Target Game Version**: Latest publicly released stable, non-beta Captain of Industry version at the start of implementation planning; the exact version must be pinned in the plan and release metadata.
- **Official Guidance**: Official Captain of Industry modding guidance applicable to the pinned game version; the specific supported interface and guidance version must be confirmed during planning.
- **Terrain Mutation Boundary**: Exactly the terrain contained within the explicitly selected empty sub-tile or straight line of consecutive empty sub-tiles whose parent tiles contain retaining walls and that are covered by the previewed 1x1 leveling structures; no unselected sub-tile, retaining wall, or other structure may be altered.
- **Safe Failure Behavior**: Validate every selected sub-tile before committing a placement. If any selected sub-tile is invalid, reject the entire placement and create no construction jobs. After valid placement, each structure validates and mutates only its own sub-tile atomically when its individual construction completes; a failure leaves that sub-tile unchanged, removes the failed temporary structure, makes no additional charge, and notifies the player that completed costs are not refunded without reverting other structures that already completed successfully.
- **Compatibility**: Declare the pinned game version, all required dependencies, and known conflicts involving terrain, construction menus, or retaining walls. When compatibility cannot be established, disable the mutation and expose actionable diagnostic information.
- **In-Game Verification**: On the pinned game version, create uneven empty sub-tiles in a parent tile containing a retaining wall at a location unreachable by construction vehicles, drag a straight line of 1x1 leveling structures at a chosen valid elevation, complete them using Unity, and observe that every selected sub-tile reaches that elevation, unselected sub-tiles and the wall remain unchanged, all temporary structures disappear, and the result survives save/load. Confirm that no materials are required in the preferred mode, or that exactly five concrete bricks per structure and the documented Unity quick-build cost apply in fallback mode. Repeat with one invalid sub-tile in the line and verify that the whole placement is rejected without terrain change. Attempt placement over a retaining wall and in a parent tile without a retaining wall and verify both are rejected.
