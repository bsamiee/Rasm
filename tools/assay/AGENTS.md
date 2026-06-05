# [ASSAY_AGENTS]

Scope: `tools/assay/` only. Root policy owns universal Python and docs behavior; `coding-python` is required before any `.py` edit. `README.md` owns the command surface, operator workflow, and user-facing tool reference.

## [1][READ_ORDER]

- When changing command behavior, operator text, or public wire expectations, read `README.md`.
- When editing a module, read the target module fully first.
- When adding a rail, verb, tool row, wire detail, or stdout shape, read `core/model.py`, `composition/catalog.py`, and `composition/registry.py`.
- When changing execution, aspect composition, routing, leases, or subprocess behavior, read `core/engine.py`, `core/aspect.py`, and `core/routing.py`.
- When changing automation triggers, actions, or fire loops, read `automation/model.py` and `automation/engine.py`.

## [2][ARCHITECTURE_CONTRACT]

Assay is one engine over data rows. Programs are catalog rows, rails select rows, the engine runs checks, and one envelope shape crosses every invocation. Cross-cutting behavior attaches only through ordered aspect seams.

## [3][EXTENSION_GRAMMAR]

- Program: add one catalog row.
- Language: add one language axis member, routing arm, and catalog rows.
- Verb: add one registry bind and one owning params type.
- Detail: add one tagged detail variant and union registration.
- Aspect: add one layer, one slot, and one seam entry.
- Automation trigger or action: add one tagged union case.
- In-process tool: add one catalog row with a thunk folded through the same engine rail.

## [4][ENGINEERING_RULES]

- Axis enums carry behavior payloads; do not split vocabulary, CLI tokens, wire values, and match keys into parallel types.
- One report, detail, envelope, artifact, match, count, bind, and params model crosses rails unless a new tagged case is the required extension.
- Keep registration data-driven. Data rows and folds own behavior; decorators and helper modules are rejected indirection.
- Keep stdout writes behind the one emitter and keep diagnostics on stderr.
- Improve tools through deeper internal behavior, resilience, routing, and typed failure, not agent-facing knobs.
- Cap inline collections only when the wire marks truncation and points to the persisted full artifact.

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

## [7][STOP_RULES]

Static checks are not enough for `.py` behavior changes. If the changed rail cannot be exercised through a runtime smoke path, state the gap instead of claiming operator readiness.
