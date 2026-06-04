# [H1][ASSAY_AGENTS]
>**Dictum:** *Edit by ledger: tools are rows, behavior is enums, cross-cutting is two seams — touch nothing else.*

Scope: `tools/assay/` only. Root `AGENTS.md` and `CLAUDE.md` own universal policy; this file is the canonical assay edit delta. `ARCHITECTURE.md` is LAW — it absorbed the former `TYPE_SYSTEM.md`/`AOT.md`/`IMPLEMENTATION.md` shards (§5/§6/§14). Cite the §11 ledger (`D1`–`D52`); never re-litigate a locked row. Operator contract (Envelope, exit codes, migration) is `README.md`; per-module shapes are `design/<layer>/<file>.md` (§15).

## [1][LOAD_ORDER]

1. `CLAUDE.md` §5.2 quality gates (quality verbs still apply until assay parity).
2. Root `AGENTS.md` → this file.
3. `ARCHITECTURE.md` — §4 shapes, §5 type system, §6 aspects, §8 exit algebra, §11 ledger, §13 registry.
4. `README.md` before invoking any verb.
5. The owning `design/<layer>/<file>.md` for the module under edit.
6. `.claude/skills/coding-python/SKILL.md` before any `.py` edit.

Keep this file delta-only; encyclopedic detail stays in `ARCHITECTURE.md` and `design/`.

## [2][OWNERSHIP]

| [PATH] | [OWNS] | [NEVER] |
| ------ | ------ | ------- |
| `core/status.py` | `RailStatus` (incl. `FAULTED=2`), `join`, `fold`, `from_returncode` | Second status enum or `status: str` field |
| `core/model.py` | Axis enums (`Runner`/`Input`/`Language`/`Mode`/`Claim`), `Tool`, `Check`, `Report`, `Detail`, `Envelope`, `Artifact`, `Match`, `Counts`, `Routed`, `Bind`, `Parser` alias | Env settings, CLI params, per-program argv builders |
| `core/engine.py` | `run_check`, `fan_out`, `_splice`, `psutil`-verified leases, process capture | Rail folds, Cyclopts, `Engine`/`Parser` Protocol |
| `core/routing.py` | `route`, `place(routed, tool, *, settings)`, `Scope`, `Source` Protocol (sole justified Protocol) | Catalog rows, Envelope emit |
| `core/aspect.py` | `checked`/`logged`/`traced`/`retried`, `compose`/`assemble`/`compose_spawn`/`_once`, `Slot` | Inline structlog/otel/stamina in rails |
| `composition/settings.py` | `AssaySettings`, `artifact()`, `ArtifactStore` (`fsspec` seam, `Configuration`/`LogFormat`) | msgspec wire structs |
| `composition/catalog.py` | `TOOLS` rows (incl. Bash/SQL), `select(claim, language)`, ~6 referenced parsers | Handlers, registry, inline parser bodies |
| `composition/registry.py` | `REGISTRY` binds, `rail`, `_emit` (sole stdout writer), `build_app` | Tool argv logic |
| `rails/{static,test,docs}.py` | One `thin_rail` + 3 thin adapters → `Result[Report, Fault]` | New status types, helper modules |
| `rails/{bridge,package,api}.py` | 3 bespoke folds + `VerifySummary`/`PackageRun`/`ApiSurface` detail | Catalog argv, second stdout writer |
| `automation/model.py` | `Trigger` (`Watch`/`Schedule`/`Manual`) + `Action` (`Rail`/`Program`/`Sequence`) tagged unions | Treating automation as a `Claim` |
| `automation/engine.py` | One `anyio` task-group loop (`watchfiles`/`aiocron`), one Envelope per fire (NDJSON) | A second `_emit`; per-fire `BUSY` retry |
| `__main__.py` / `__init__.py` | `build_app`, `meta` → `resolve_returncode`; configure gates + optional claw | Business logic; second stdout writer |

Add a program: one `Tool` row in `catalog.py`. Add a language: one `Language` member + one routing arm + rows (D36). Add a verb: one `Bind` + frozen `@dataclass` params (D19). Add a rail: one `rails/<claim>.py` fold — never a new engine module.

## [3][SPAM_TRIPWIRES]

Stop and collapse before merging if any appear:

- Free `typing.Literal` aliases for vocabularies already on a `StrEnum`; no standalone `Strategy`/`RouteScope`/`CAPTURE`/`STREAM` (D6/D24/D4).
- `NamedTuple` CLI params (`TID251`) — frozen `@dataclass` + Cyclopts `Parameter(name="*")` flatten (D19).
- Second model system on wire shapes (pydantic `BaseModel` for `Report`/`Envelope`); dual pydantic+msgspec on one field.
- `Engine` or `Parser` `Protocol` — engine is module functions; `Parser = Callable[[Completed], Detail | None]` attached by reference (D22/D25).
- Per-rail report structs (`StaticPlanReport`, `Api*Report`, …) — `Report` + tagged `detail` (D11/D13).
- `Fault` carrying `returncode` or `detail` — it is `{argv, status=FAULTED, message}`; `logged`/`traced` read `f.status`/`f.message` (**D28**).
- `return match …` — `match` is statement form only; bind in a `case`, then `return` (**D27**).
- `from effect import result` — import `from expression import effect`; `@effect.result[T,E]()` wraps **generators** only (`yield`); a plain returning function folds its `Result` via `match` (D26).
- `helpers.py`, `*Util`, `common_*`, single-call indirection; `artifact_paths: dict[str,str]` (use `tuple[Artifact, ...]`).
- `@tool`/`@rail` registration decorators — registration is data (`TOOLS`, `REGISTRY`).
- `@retried` on the rail runner; `@logged` on `run_check` (§6 seam table).
- stdout writes outside `_emit` (including a cataloged tool printing JSON).
- `worst(...)` — the fold is `RailStatus.join` (max-by-severity) + module `fold` (D15).

## [4][SEAMS]

Cross-cutting behavior attaches **only** as slot-ordered aspect decorators at **two seams** (`Slot(IntEnum){checked,logged,traced,retried}`, `checked=0` outermost):

- Rail runner (`composition/registry.py`): `checked ▷ logged ▷ traced` — **no** `retried` (a rail is `Hom`, not `Spawn`; D2).
- `run_check` (`core/engine.py`): `checked ▷ traced ▷ retried` — **no** `logged` on the hot path; logging stays on the parent (D3).

`structlog.configure` runs once at `__init__.py` import bound to `sys.stderr`; OTel exports only when the OTLP endpoint is set (gated provider install); `beartype` at seams only — `beartype_this_package()` only under `ASSAY_CLAW` as the first `__init__.py` statement. `@retried` retries transient spawn faults and **never** `BUSY`/`TIMEOUT` (§8). Full layer table + stamina predicates: ARCHITECTURE §6.

Neighbor seams to honor when editing: rails call `engine.run_check`/`fan_out` (D14, never `Engine.run`); rails project argv only via `routing.place(routed, tool, *, settings)` (D23, the sole argv-tail projector); rails select rows via `catalog.select(claim, language)`; every rail folds to `Result[Report, Fault]` and returns to `registry.rail`, which is the only caller of `_emit`. Bridge/package/api leases (`bridge.lock`, `package-stage.lock`) and the build-closure lease live in `engine`, not the rails (§7).

## [5][SHAPE_RULES]

- `msgspec` owns all wire/evidence structs; `Detail` is a tagged union with explicit short tags (`verify`/`test`/`package`/`api`, never `classname.lower()`) + `forbid_unknown_fields` (D13). The sole escape hatch for irregular evidence is `msgspec.defstruct` from catalog metadata.
- `pydantic-settings` owns `AssaySettings` only; sources collapse to `(init_settings, env_settings)` — no `CliSettingsSource`, no dotenv (D20). The two systems never model the same value (§5).
- One `StrEnum` member = Cyclopts token = `msgspec` wire value = `match` key; choices are derived, exit codes ride enum payloads.
- `Envelope` uses `claim` + `report: Report | None` + `error: Fault | None` (D10) — never quality's `rail`/`data`/`evidence` ladder. `Envelope.exit_code == status.exit_code` always; `__cyclopts_returncode__(self) -> int: return self.exit_code` (D29/D30).
- Non-zero process exit → `Completed` (`FAILED`) via `from_returncode`, never `Fault` (D12). `Fault` (`FAULTED`/`BUSY`/`TIMEOUT`) rides `Envelope.error` only.

## [6][VALIDATED_SNIPPET]

`meta` collapse and the statement-form `match` fold (D26/D27/D30) — valid Python 3.14, `cyclopts` 4.16 + `expression` 5.6:

```python
from cyclopts import App
from cyclopts.core import resolve_returncode
from expression import Result, Ok, Error

app = App(name="assay")

def rail(fn):  # fn: () -> Result[Report, Fault]; _emit is the sole stdout writer
    def runner(*args: object, **kw: object) -> int:
        result = fn(*args, **kw)
        match result:
            case Ok(report):
                return _emit(Envelope(claim=report.claim, report=report))
            case Error(fault):
                return _emit(Envelope(claim=fault.argv[0], error=fault))
        raise AssertionError("unreachable")  # exhaustive over Result

def meta(tokens: list[str]) -> int:
    return resolve_returncode(app(tokens, result_action="return_value", backend="asyncio"))
```

`_emit` writes one `Envelope` JSON to stdout and returns `envelope.exit_code`; `resolve_returncode(None) == 0` covers verbs that emit and return nothing.

## [7][DEPENDENCIES]

Declare consumers in root `pyproject.toml` before use — no shadow imports. Installed/load-bearing plus the four day-one adoptions (ARCHITECTURE §10, locked D34/D37/D39):

| [LIB] | [TIER] | [CONSUMER / ROLE] |
| ----- | ------ | ----------------- |
| `msgspec` / `pydantic-settings` / `expression` / `anyio` / `cyclopts` | core | Wire structs / `AssaySettings` / `Result` rails / process exec + `fan_out` / CLI tree |
| `beartype` / `structlog` / `opentelemetry-*` / `stamina` | aspect | `@checked` / `@logged` (stderr) / `@traced` (gated) / `@retried` (spawn only) |
| `psutil` | adopted | `core/engine` leases — `(pid, create_time)` liveness steals stale holders (D34); optional fleet CPU governor |
| `watchfiles` | adopted | `automation/engine` — debounced `awatch` filesystem `Watch` trigger, anyio `stop_event` (D37) |
| `aiocron` | adopted | `automation/engine` — `crontab(spec, start=False)` `Schedule` trigger under one task group (D37) |
| `fsspec` | adopted | `composition/settings` `ArtifactStore` — backend-agnostic `.artifacts`; `memory://` for tests (D39) |
| `httpx` | thin | OTLP-HTTP egress / `api` fetch only; `trust_env=False`, `Timeout(10)`; droppable |

Orchestrated programs are catalog **rows**, not deps: C# `dotnet`/`dotnet-stryker`/`ilspycmd`/`yak`; Python `ruff`/`ty`/`mypy`/`pytest`/`coverage`/`mutmut`/`validate-pyproject`/`ast-grep`/`py_analyzer`; TS `tsc`/`biome`/`knip`/`sherif`/`vitest`; **Bash** `shellcheck -f json1`/`shfmt -d`; **SQL** `sqlfluff lint --dialect postgres`/`squawk`; Docs `mmdc`. Bash/SQL land under `static report`/`fix` (D36); they are rows in `catalog.py` + arms in `routing.py`, never files (§15). When editing those rows, the language skill is `coding-bash` / `coding-pg` per root `CLAUDE.md` §1.

## [8][VALIDATION_LADDER]

Per ARCHITECTURE §14 gate, after each touched `.py`:

```bash
uv run ruff check tools/assay
uv run ty check tools/assay
```

Wire law once `tests/tools/assay/` exists: `encode(decode(x)) == x` for `Envelope`/`Report`; an unknown `detail` field fails decode. Markdown-only edits run no static/test/bridge rail unless the user requests proof or move-only preservation fails (`git diff --check`). C#-touching rails still route through `uv run python -m tools.quality static build [paths]` until assay `static build` is registered.

## [9][CONSIDERATIONS]

- The `design/` tree is ARCHITECTURE-sanctioned (§15); the legacy `.design/` shards plus `IMPLEMENTATION.md`/`TYPE_SYSTEM.md`/`AOT.md` were harvested into ARCHITECTURE + the `design/` docs and **removed**. Cite ARCHITECTURE sections and `design/<layer>/<file>.md`, never a shard path — reintroducing one re-opens a contradiction the ledger closed.
- The one-Envelope invariant (§17.1) has exactly one sanctioned exception: `automation/engine` streams one Envelope per fire as NDJSON through the same `_emit`. Any other multi-write is a regression — a cataloged tool that prints JSON to stdout silently breaks the orchestrator's exit-code read.
- `Mode.WRITE` rows (D4) are the only mutation surface; a `static fix` that mutates outside a `WRITE`-tagged row, or a closure build that drops the stable `.artifacts/assay/build/<closure>/` path for a run-scoped one (D32), defeats warm-tree reuse and the lease model in one stroke.
