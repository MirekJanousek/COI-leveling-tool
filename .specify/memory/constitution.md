<!--
Sync Impact Report
- Version change: template (unratified) -> 1.0.0
- Modified principles: None; initial constitution created from template
- Added principles: I. Official Modding Guidance Is Authoritative; II. Retaining-Wall Leveling Is the Core Scope;
  III. Terrain and Save Safety Are Non-Negotiable; IV. Compatibility and Minimal Intrusion;
  V. Evidence-Driven Quality
- Added sections: Technical and Product Constraints; Development Workflow and Quality Gates
- Removed sections: None; template placeholders were resolved
- Templates requiring updates: ✅ plan-template.md; ✅ spec-template.md; ✅ tasks-template.md
- Runtime guidance reviewed: ✅ README.md; ✅ AGENTS.md
- Follow-up TODOs: None
-->
# COI Leveling Tool Constitution

## Core Principles

### I. Official Modding Guidance Is Authoritative
All implementation choices MUST follow the official Captain of Industry modding guides and supported mod
interfaces for the targeted game version. Every plan MUST identify the applicable official guidance, target
game version, and supported API or extension point. Any required departure MUST be documented with the
reason, compatibility risk, and a contained fallback before implementation begins. This keeps the mod aligned
with the game's supported ecosystem and reduces breakage across game updates.

### II. Retaining-Wall Leveling Is the Core Scope
The mod MUST solve terrain-leveling problems at or immediately adjacent to retaining walls. Features MUST
have a direct, documented connection to that player outcome. General terraforming, unrelated construction
automation, and broad game-system changes are out of scope unless this constitution is amended. The user
experience MUST integrate with established game concepts and controls wherever the supported mod API permits.
A narrow scope keeps behavior predictable and the mod maintainable.

### III. Terrain and Save Safety Are Non-Negotiable
Terrain changes MUST be deterministic for the same inputs, bounded to the intended area, and validated before
mutation. The mod MUST NOT knowingly create invalid terrain, damage retaining walls or unrelated structures,
corrupt saves, or leave a partially applied operation without a defined recovery path. Boundary conditions,
including map edges, overlapping structures, invalid selections, and interrupted operations, MUST be specified
and tested. Operations with uncertain safety MUST fail without changing game state.

### IV. Compatibility and Minimal Intrusion
The implementation MUST use the smallest supported integration surface that delivers the feature. It MUST
avoid replacing or patching unrelated game behavior and MUST NOT depend on undocumented internals when an
official alternative exists. Supported game and mod versions MUST be explicit. Missing or incompatible
dependencies MUST produce a clear diagnostic and preserve normal game behavior. Any unavoidable conflict
with other mods MUST be documented with its detection or mitigation strategy.

### V. Evidence-Driven Quality
Each behavior change MUST have acceptance scenarios and proportionate automated tests for logic that can be
tested outside the game. Terrain calculations, validation, bounds, and failure behavior MUST have automated
coverage. Integration with Captain of Industry MUST also be verified in a documented in-game scenario on the
targeted game version. A change is complete only when tests pass, the in-game verification is recorded, and
player-facing behavior or limitations are documented.

## Technical and Product Constraints

- Captain of Industry and the game versions declared by the active feature plan are the target platform.
- Official Captain of Industry modding documentation is the primary technical authority; plans MUST record
  the specific guide or API used and the date or version consulted.
- The mod MUST remain independently installable and removable through the game's supported mod mechanism.
- Persistent mod data, if introduced, MUST be versioned and migrated safely; removing or disabling the mod
  MUST NOT make an otherwise valid save unusable unless an unavoidable limitation is prominently documented.
- Runtime work MUST be bounded and measured against representative retaining-wall operations so ordinary
  gameplay is not visibly stalled.
- Player-facing text, configuration, diagnostics, and installation requirements MUST be documented.

## Development Workflow and Quality Gates

1. Specifications MUST define the retaining-wall use case, affected terrain boundary, failure behavior,
   supported game version, compatibility assumptions, and measurable outcomes.
2. Plans MUST cite applicable official modding guidance and pass every Constitution Check before research
   and again after design. Any exception requires an entry in Complexity Tracking with a concrete rationale.
3. Tasks MUST include safety validation, automated tests for deterministic logic and edge cases, compatibility
   checks, documentation, and a repeatable in-game verification scenario.
4. Reviews MUST trace implementation and tests to requirements and MUST reject undocumented APIs, unbounded
   terrain mutation, unrelated scope, or an unverified target-game integration.
5. Releases MUST state supported game/mod versions, installation steps, player-visible changes, known
   limitations, and the completed verification evidence.

## Governance

This constitution supersedes conflicting development practices and templates in this repository. Amendments
require a written proposal describing the changed rule, rationale, compatibility impact, and any migration
needed for existing specifications or code. The amendment MUST update dependent templates and the Sync Impact
Report in the same change.

Constitution versions follow semantic versioning: MAJOR for removing or incompatibly redefining governance,
MINOR for adding a principle or materially expanding mandatory guidance, and PATCH for non-semantic
clarifications. Every specification, plan, task list, implementation review, and release review MUST verify
compliance with the current version. Exceptions MUST be explicit, narrowly scoped, risk-assessed, and approved
before implementation; convenience alone is not sufficient justification.

**Version**: 1.0.0 | **Ratified**: 2026-07-21 | **Last Amended**: 2026-07-21
