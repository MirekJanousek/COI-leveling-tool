# Gameplay Contract

## Availability

- The leveling tool appears in the same Terraforming construction group as retaining walls.
- It unlocks with the existing retaining-wall research.
- It is enabled only on the verified `v0.8.6a` compatibility profile.
- The player sees the active cost mode and cost before committing placement.

## Selection and Preview

- The player may select one 1x1 terrain square or drag one consecutive axis-aligned line.
- Every previewed square uses the same chosen elevation.
- A square is eligible only when it is empty, does not overlap a retaining wall, and belongs to the canonical 4x4 terrain-designation cell containing a fully constructed retaining wall.
- The preview identifies every affected square and the requested elevation.
- Every invalid square receives ordinary player-facing placement feedback.

## Commit

- The complete line is revalidated on the simulation thread.
- If any square is invalid, no structure is created anywhere in the line and no cost or terrain change occurs.
- If all squares are valid, one independent temporary structure is created per square.
- Cancellation before an individual structure completes causes no terrain change.

## Construction and Cost

- Preferred mode is material-free Unity-only completion if public `v0.8.6a` APIs support it.
- Otherwise, each structure costs exactly five Concrete Slabs and retains native pay-with-Unity quick build.
- The target Unity cost is `0.05` per structure only when the supported game API can represent/configure it; otherwise the displayed native calculated value is authoritative.
- Lack of vehicle access may produce a base-game warning but does not block Unity completion.

## Completion

- When one structure completes, the mod validates its target again.
- The operation writes exactly the four terrain vertices bounding that square to the chosen elevation.
- No other stored terrain vertex is written. Because vertices are shared, adjacent squares may visually slope toward the changed boundary.
- Success removes the temporary structure immediately and requires no player demolition.
- Changed terrain survives save/load through the game's terrain serialization.

## Failure and Refund

- Any preflight, mutation, or verification failure restores the four original vertex heights.
- The failed temporary structure is removed.
- Exact charged Unity and fallback products are refunded once.
- Other structures from an already accepted line that completed successfully are not reverted.
- A rollback or refund failure disables subsequent operations and produces a critical notification and detailed log entry.

## Unsupported Version

- The mod must not attempt terrain mutation on an unverified game/core version or missing capability profile.
- Normal game behavior remains available.
- The player receives one actionable compatibility warning; details are written to `%APPDATA%/Captain of Industry/Logs`.
