# [H1][ASSAY_IMPLEMENTATION]
>**Dictum:** *Build from the leaves of the dependency graph; each file lands complete and verifiable.*

<br>

Construction guide for `assay`. It fixes the order of file creation, the core needs every file satisfies, and the per-file contract that defines done. `ARCHITECTURE.md` holds the abstraction; this document holds the build. Every library API claim is verified against `docs/external-libs` and the upstream reference before use.

| [DOC] | [ROLE] |
| ----- | ------ |
| `README.md` | Agent operator: Envelope stdout, rails, leases, quality migration |
| `AGENTS.md` | Edit routing, tripwires, validation ladder |
| `.design/TYPE_SYSTEM.md` | Wire shapes, enum policy, retired anti-patterns |
| `.design/AOT.md` | Aspect compose order; no `Engine`/`Parser` Protocol |

[CRITICAL] **Agent-only I/O:** `composition/registry._emit` is the sole stdout writer — one `msgspec` JSON `Envelope` line per verb. `__main__` returns `resolve_returncode(envelope)`. Diagnostics and child streams go to stderr/artifact paths only (`README.md`).

---
## [1][CORE_NEEDS]
>**Dictum:** *The same discipline binds every file; state it once.*

<br>

These needs hold for all modules and form the acceptance floor independent of file role.

| [INDEX] | [NEED]             | [REALIZATION]                                                             |
| :-----: | ------------------ | ------------------------------------------------------------------------ |
|   [1]   | Two model systems  | `pydantic-settings` for env config; `msgspec.Struct(frozen)` for all else. |
|   [2]   | Behavior in enums  | Discriminants are `StrEnum` with `__new__` payloads, not `Literal` aliases. |
|   [3]   | One status algebra | `RailStatus` carries `exit_code` and aliases; nothing else encodes status. |
|   [4]   | Typed rails        | Operations return `Result[T, Fault]`; no exception control flow in domain logic. |
|   [5]   | Aspect seams       | Cross-cutting libraries attach as `core/aspect.py` decorators, never inline. |
|   [6]   | No spam            | New strings land only in an enum member or a `Tool` row; no helper modules. |

---
## [2][BUILD_ORDER]
>**Dictum:** *A file is built only after every file it imports is complete.*

<br>

The order tracks the acyclic chain `core` then `composition` then `rails` then `registry` then `__main__`. Each stage compiles and type-checks before the next begins.

| [INDEX] | [STAGE] | [FILE]                                      | [DEPENDS ON]                                          |
| :-----: | :-----: | ------------------------------------------- | ---------------------------------------------------- |
|   [1]   |    1    | `core/status.py`                            | None.                                                |
|   [2]   |    2    | `core/model.py`                             | `core/status.py`.                                    |
|   [3]   |    3    | `core/aspect.py`                            | `core/model.py`; `beartype`, `structlog`, `opentelemetry`, `stamina`. |
|   [4]   |    4    | `composition/settings.py`                   | `core/status.py`.                                    |
|   [5]   |    5    | `core/engine.py`                            | `core/model.py`, `core/aspect.py`, `composition/settings.py`. |
|   [6]   |    6    | `core/routing.py`                           | `core/model.py`, `core/engine.py`.                   |
|   [7]   |    7    | `composition/catalog.py`                    | `core/model.py`.                                     |
|   [8]   |    8    | `rails/static.py`, `test.py`, `docs.py`     | `composition/catalog.py`, `core/routing.py`, `core/engine.py`. |
|   [9]   |    9    | `rails/bridge.py`, `package.py`, `api.py`   | Stage 8 set plus `Detail` variants in `core/model.py`. |
|  [10]   |   10    | `composition/registry.py`                   | `rails/*`, `composition/settings.py`, `core/aspect.py`. |
|  [11]   |   11    | `__main__.py`                               | `composition/registry.py`.                           |

---
## [3][FILE_CONTRACTS]
>**Dictum:** *Each file declares its produced surface, key members, and acceptance.*

<br>

### [3.1][CORE]

The primitives carry the shape, execution, and aspect discipline the rest of the tool reuses.

| [INDEX] | [FILE]            | [KEY MEMBERS]                                                              | [ACCEPTANCE]                                          |
| :-----: | ----------------- | ------------------------------------------------------------------------- | ---------------------------------------------------- |
|   [1]   | `core/status.py`  | `RailStatus(StrEnum)` with `exit_code`, aliases, `from_returncode`.        | One enum; member payload round-trips through msgspec. |
|   [2]   | `core/model.py`   | Enums `Runner`, `Input`, `Language`, `Claim`, `Mode`, `ArtifactKind`; `Counts`; structs `Tool`, `Check`, `Completed`, `Fault`, `Artifact`, `Match`, `Report`, `Detail`, `Envelope`; `Parser` type alias. | `Report.detail` decodes by explicit `tag=` with `forbid_unknown_fields`. |
|   [3]   | `core/aspect.py`  | Decorators `@checked`, `@traced`, `@retried`, `@logged`; one `compose` combinator. | Decorators stack without reordering effects.          |
|   [4]   | `core/engine.py`  | `run_check`, `fan_out`, stream capture, `exclusive_lease`.                 | Module functions only; faults are `Result`, never raised. |
|   [5]   | `core/routing.py` | `route(language, paths)`, `place(routed, tool)`, git change-set, `fd`, graph closure.   | `Language.strategy` only branch; no standalone `Strategy` enum.   |

### [3.2][COMPOSITION]

The configuration, data, and wiring that compose the primitives into a CLI.

| [INDEX] | [FILE]                    | [KEY MEMBERS]                                                                  | [ACCEPTANCE]                                       |
| :-----: | ------------------------- | ----------------------------------------------------------------------------- | ------------------------------------------------- |
|   [1]   | `composition/settings.py` | `AssaySettings(BaseSettings, frozen)`; `Configuration(StrEnum)`; `artifact(kind, *parts)`; `(init_settings, env_settings)` only. | One settings class; `ArtifactKind` imported from `model.py`.  |
|   [2]   | `composition/catalog.py`  | `TOOLS: tuple[Tool, ...]`; `select(claim, language)`.                          | A program is one row; selection is pure data.      |
|   [3]   | `composition/registry.py` | `REGISTRY: claim x verb -> handler`; one `rail(handler)` runner with aspects.  | One runner; no per-rail projector callables.       |
|   [4]   | `rails/*`                 | One handler per claim returning `Result[Report, Fault]`.                       | Thin folds; bespoke rails emit a `detail` tag.     |
|   [5]   | `__main__.py`             | Cyclopts tree from `REGISTRY`; `main()`; one Envelope to stdout.               | Exactly one Envelope per invocation.               |

---
## [4][TYPE_INTEGRATION]
>**Dictum:** *Compose the stack in the order the data flows; never duplicate a value across systems.*

<br>

The construction reuses one `StrEnum` instance as the Cyclopts parameter, the `msgspec` wire value, and the dispatch key, so no translation layer exists between the CLI, the payload, and the match.

| [INDEX] | [STEP]                    | [CONSTRUCT]                                                  |
| :-----: | ------------------------- | ----------------------------------------------------------- |
|   [1]   | Define vocabulary         | `StrEnum.__new__(value, *payload)` for `RailStatus`, `Runner`, `Input`. |
|   [2]   | Define shapes             | `msgspec.Struct(frozen)` with `Annotated[.., msgspec.Meta]` bounds. |
|   [3]   | Define the detail union   | Tagged base, explicit `tag=`, `forbid_unknown_fields=True` (`.design/TYPE_SYSTEM.md` §3). |
|   [4]   | Handle irregular evidence | `msgspec.defstruct` from catalog metadata; `convert` with `dec_hook`. |
|   [5]   | Compose aspects           | `core/aspect.py` on rail runner and `run_check` only (`.design/AOT.md`). |
|   [6]   | Bind config               | `pydantic-settings` with `Annotated[.., Field]` and `computed_field`. |
|   [7]   | Type the rails            | PEP 695 `type` aliases; `Parser = Callable[[Completed], Detail \| None]`; **no** `Engine`/`Parser` Protocol. |

---
## [5][VERIFICATION]
>**Dictum:** *Each stage proves itself before the next begins.*

<br>

[VERIFY] Per-stage gate, run after each file in `BUILD_ORDER`:
- [ ] `uv run ruff check tools/assay` passes with no new suppressions.
- [ ] `uv run ty check tools/assay` reports no errors.
- [ ] The new module imports cleanly under the namespace-package layout.
- [ ] A round-trip `msgspec.json.encode` then `decode` holds for any new struct.
- [ ] No `Literal` alias, helper module, status-string field, inline cross-cutting call, or second stdout writer was introduced.
- [ ] v1 modules do not import `watchfiles`, `psutil`, or `fsspec` until `pyproject.toml` declares them and `ARCHITECTURE.md` §6 gate clears.

Wire contract tests (when present): `tests/tools/assay/test_cyclopts_contract.py` per `.design/snippets/cli.py.md`.
