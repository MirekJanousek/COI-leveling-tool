# Quickstart: COI Terrain Leveling Tool

## Prerequisites

1. Install Captain of Industry Update 4.2 `v0.8.6a` on Windows.
2. Install the .NET Framework 4.8 Developer Pack and a current Visual Studio with C# build tools.
3. Set `COI_ROOT` to the game installation root, for example:

   ```powershell
   [Environment]::SetEnvironmentVariable('COI_ROOT', 'V:\SteamLibrary\steamapps\common\Captain of Industry', 'User')
   ```

4. Restart the terminal/IDE after setting the variable.

The official mod guide recommends Visual Studio. `dotnet build` is useful for command-line checks, but MaFi notes that some deployment automation may not work identically outside Visual Studio.

## Required Feasibility Gate

Before implementing general feature code, run the `v0.8.6a` compatibility tests/spikes and record evidence for:

- material-free Unity-only construction and fractional cost support;
- elevation and axis-aligned drag behavior for the custom 1x1 layout entity;
- all-or-none simulation-thread line creation;
- four-vertex height mutation, changed-terrain save tracking, verification, and rollback;
- idempotent completion handling, normal entity cleanup, and one no-refund failure notification.

If a required behavior lacks a supported public API, stop and update the plan/spec. Do not substitute Harmony, private reflection, or raw save/terrain writes.

## Build

From the repository root:

```powershell
dotnet build COILevelingTool.slnx -c Release
```

Expected mod output:

```text
src/COILevelingTool/bin/Release/net48/COILevelingTool.dll
```

The project deployment target copies the package to:

```text
%APPDATA%\Captain of Industry\Mods\COILevelingTool\
├── manifest.json
├── readme.txt
└── COILevelingTool.dll
```

## Automated Tests

```powershell
dotnet test tests/COILevelingTool.Tests/COILevelingTool.Tests.csproj -c Release
```

The contract suite requires `COI_ROOT` and must verify runtime game version `0.8.6a`, `Mafi.Core 0.8.6.0`, required public signatures, terrain-designation size, and manifest version bounds.

## In-Game Verification

1. Launch Captain of Industry and enable `COILevelingTool` for a sandbox test save.
2. Use Update 4's flat sandbox map or run `enable_sandbox` on a test save.
3. Research/unlock retaining walls and confirm the leveling tool appears in the same Terraforming menu.
4. Place and fully construct a retaining wall.
5. Select an empty 1x1 square in the same canonical 4x4 terrain-designation cell without overlapping the wall.
6. Set an elevation, place one tool, finish it with Unity, and verify:
   - the four bounding vertices reach the chosen height;
   - no other stored vertex changes;
   - the wall remains intact;
   - the temporary entity disappears;
   - the terrain persists after save/reload.
7. Repeat with a straight dragged line and confirm every square is previewed at one elevation.
8. Include one invalid square in a line and verify that zero structures are created and no costs change.
9. Repeat at a vehicle-inaccessible site and verify Unity completion succeeds despite any access warning.
10. Inject mutation failures after vertex writes 1–4 and verify height rollback, entity removal, and exact Unity/product balances.
11. Test wall orientations/positions, parent-cell boundaries, map edges, occupied/reserved squares, repeated/adjacent operations, current-height no-op, cancellation, save during construction, and other terrain-affecting mods.

Because terrain vertices are shared, neighboring squares may visually slope toward the changed boundary. Verification compares stored vertex coordinates: only the four vertices bounding each selected square may be written.

## Logs and Diagnostics

Inspect:

```text
%APPDATA%\Captain of Industry\Logs
```

Every compatibility or leveling failure must include a stable reason code, target/parent coordinates, requested height, cost mode, and rollback/removal result. An unsupported game version must disable the tool without disrupting the base game.

## Packaging and Removal

- Package the mod directory as the ZIP root and keep its folder name equal to manifest `id`.
- Set `min_game_version` and `max_verified_game_version` to `0.8.6a`.
- Enable `can_add_to_saved_game` only after add-to-save testing passes.
- Enable `can_remove_from_saved_game` only after clean, planned, and under-construction removal cases are proven safe. Until then, document that all pending leveling structures must be completed or cancelled before removing the mod.
- Release notes must state exact game version `0.8.6a`, actual cost mode/Unity price, known asset/drag limitations, and completed in-game evidence.
