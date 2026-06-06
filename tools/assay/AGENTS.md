# [ASSAY_AGENTS]

Scope: `tools/assay/` only. Root policy owns universal Python and docs behavior; `coding-python` is required before any `.py` edit. `README.md` owns the command surface, operator workflow, and user-facing tool reference.

## [1][READ_ORDER]

- When changing command behavior, operator text, or public wire expectations, read `README.md`.
- When editing a module, read the target module fully first.
- When adding a rail, verb, tool row, wire detail, or stdout shape, read `core/model.py`, `composition/catalog.py`, and `composition/registry.py`.
- When changing execution, aspect composition, routing, leases, or subprocess behavior, read `core/engine.py`, `core/aspect.py`, and `core/routing.py`.
- When changing artifact storage, runtime backend, history persistence, or filesystem behavior, read `composition/settings.py` for `ArtifactStore` and `ArtifactScope`, then read `composition/registry.py` for history persistence before adding public surface.
- When changing automation triggers, actions, or fire loops, read `automation/model.py` and `automation/engine.py`.

## [2][ARCHITECTURE_CONTRACT]

Assay is one engine over `Tool` and `Check` rows. `TOOLS` and `REGISTRY` select rows, `run_check` and `fan_out` execute them, `fold` builds `Report`, and `envelope` emits `Envelope`; cross-cutting behavior attaches through `Slot`, `Layer`, `SpawnLayer`, `compose`, `compose_spawn`, and `_RAIL_LAYERS`.

Runtime backend, storage, and filesystem capability is internal behavior. It extends settings, artifact store/scope, engine execution, history persistence, envelope rows, and artifact rows before it becomes operator workflow or public wire shape.

Add functionality by deepening the existing polymorphic shape that owns the behavior. Prefer one behavior-bearing enum, msgspec message model, catalog row, registry bind, tagged detail union, `Result` rail, aspect slot, or store/scope projection over many loose objects later forced into one stream. Advanced Python means precise shapes, generic owner flows, structural typing where it removes concrete coupling, and first-class use of `msgspec`, `pydantic-settings`, `anyio`, `expression`, Cyclopts, and the existing aspect stack; it does not mean flat strings, command wrappers, constant piles, or parallel models.

## [3][EXTENSION_GRAMMAR]

- Program: add one catalog row.
- Language: add one language axis member, routing arm, and catalog rows.
- Verb: add one registry bind and one owning params type.
- Detail: add one tagged detail variant and union registration.
- Aspect: add one `Slot` value only when needed, one `Layer` or `SpawnLayer`, and one `_RAIL_LAYERS` or `_spawn` composition entry.
- Automation trigger or action: add one tagged union case.
- In-process tool: add one catalog row with a thunk folded through the same engine rail.
- Runtime backend or storage behavior: extend settings, store/scope, engine execution, history persistence, and envelope/artifact rows before adding a command, flag, helper module, wrapper service, or parallel store type.
- External tool, backend, filesystem, subprocess, and provider names convert at registry, settings, store, engine, and envelope boundaries. Internal Assay code uses canonical axis values, typed details, artifact rows, and result rails; public output does not expose provider selectors, backend modes, or wrapper service names unless `README.md` proves operator action changes.
- Execution environment, cache, staging, cwd, and subprocess policy extend `AssaySettings`, `Tool`, `Check`, and `core/engine.py`; they do not live in rail call-site patches, catalog-local process wrappers, or per-tool helper functions.
- Before adding a row, flag, setting, detail, or file operation, trace the full flow from registry or rail params through routing, `Check`, engine execution, fold, envelope, artifacts, and tests. If the change only works by bypassing one of those owners, refactor the owner instead.

## [4][ENGINEERING_RULES]

- Axis enums carry behavior payloads; do not split vocabulary, CLI tokens, wire values, and match keys into parallel types.
- One report, detail, envelope, artifact, match, count, bind, and params model crosses rails unless a new tagged case is the required extension.
- Keep registration data-driven. Data rows and folds own behavior; ad hoc decorators and helper modules are rejected indirection.
- Keep stdout writes behind the one emitter and keep diagnostics on stderr.
- Improve tools through deeper internal behavior, resilience, routing, and typed failure, not agent-facing knobs, backend-branded commands, or provider selectors.
- Cap inline collections only when the wire marks truncation and points to the persisted full artifact.
- Refactor as if the capability was present from the first design: collapse duplicated branches into the owner rail, update tests at that owner boundary, and delete obsolete wrong-placement code in the same change.
- Do not build a rat-nest where strings feed constants, constants feed free unions, unions feed wrapper models, and wrappers feed special-case branches. Promote one canonical shape only when it carries reusable behavior across future commands.

## [5][BOUNDARY_RULES]

| [INDEX] | [BOUNDARY]       | [OWNER]                   |
| :-----: | :--------------- | :------------------------ |
|   [1]   | Wire model       | `core/model.py`           |
|   [2]   | Engine execution | `core/engine.py`          |
|   [3]   | Routing          | `core/routing.py`         |
|   [4]   | Aspects          | `core/aspect.py`          |
|   [5]   | Tool rows        | `composition/catalog.py`  |
|   [6]   | CLI registry     | `composition/registry.py` |
|   [7]   | Settings/store   | `composition/settings.py` |
|   [8]   | Rails            | `rails/<claim>.py`        |
|   [9]   | Automation model | `automation/model.py`     |
|  [10]   | Automation loop  | `automation/engine.py`    |

## [6][REJECTIONS]

- No parallel type, params, rail shape, status enum, report struct, parser protocol, tool module, helper file, or wrapper object for one concept.
- No free `typing.Literal` aliases for vocabularies already owned by an enum.
- No pydantic model for wire shapes owned by the message model.
- No stdout writes outside the emitter.
- No glob-as-path argument for tools that walk their own tree.
- No runtime-shape annotations hidden under `TYPE_CHECKING` when call-time validation needs them.
- No backend-branded command, storage helper, provider selector, cloud-mode flag, or parallel artifact/report shape when the existing settings, store, engine, history, envelope, and artifact owners can absorb the behavior.
- No catalog-level subprocess reimplementation for a normal external tool; use `Runner`, `Input`, `Tool`, `Check`, routing, and the engine process backend.
- No rail-local cwd, cache, environment, or artifact patches for one tool; encode reusable execution policy in settings, `Tool`, `Check`, engine, store/scope, or artifact rows.
- No package-metadata storage fixes for Python tooling; package metadata does not own Python runtime policy.

## [7][STOP_RULES]

If a `.py` behavior change cannot be exercised through its consuming rail, `REGISTRY` bind, `TOOLS` selection, `Envelope` emission, `ArtifactStore` history, or automation `Trigger`/`Action` fire, state the proof gap instead of claiming operator readiness.
