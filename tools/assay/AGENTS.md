# [H1][ASSAY_AGENTS]

`tools/assay` methodology + standards for every future agent editing this tree.
Root `AGENTS.md`/`CLAUDE.md` own universal policy; this file is assay-specific.
`coding-python` skill is required before any `.py` edit.

---
## [1][ARCHITECTURE_IN_ONE_BREATH]

One **Engine** (`core/engine.run_check`/`fan_out`) runs every program in every language.
Programs are **data rows** (`Tool` in `composition/catalog.py`), not modules.
One **rail** per `Claim` selects rows, routes inputs, folds outcomes into one `Report`.
One **`Envelope`** on stdout per invocation (automation: one per fire, NDJSON).
Cross-cutting behavior attaches **only** as a slot-ordered aspect stack at **two seams**:

| [SEAM] | [STACK] | [WHY] |
| ------ | ------- | ----- |
| Rail runner (`composition/registry`) | `checked ▷ logged ▷ traced` | Parent span + logging; a rail is a `Hom`, not a `Spawn`. |
| `run_check` (`core/engine`) | `checked ▷ traced ▷ retried` | Per-`Check` child span; retry on spawn only; logging stays on the parent. |

`Slot(IntEnum){checked=0, logged=1, traced=2, retried=3}` — `compose` sorts and rejects inversions as a decoration-time `TypeError`.
The automation arm (`automation/`) shares Engine, leases, settings, and `_emit`; it is a first-class arm, not a `Claim`.

---
## [2][POLYMORPHIC_ADT_AND_AOT_DISCIPLINE]

**Axis `StrEnum`s carry behavior payloads** — `Runner.prefix`, `Input.flag`/`scoped`, `Language.strategy`/`suffixes`, `Mode.stream`/`writes`.
One member instance serves Cyclopts token, `msgspec` wire value, and `match` key simultaneously.
**One `Detail`/`Report`/`Envelope` shape** crosses every rail; `Detail` is a `msgspec` tagged union keyed by `kind` with explicit short tags (`verify`/`test`/`package`/`api`/`resolution`/`diagnostic`) and `forbid_unknown_fields`.

**How to extend correctly — exactly one touch per concern:**
- Add a program: one `Tool` row in `catalog.py`.
- Add a language: one `Language` member + one routing arm in `routing.py` + rows in `catalog.py`.
- Add a verb: one `Bind` row in `REGISTRY` + a frozen `@dataclass` `Params` class in the owning `rails/<claim>.py`.
- Add a `Detail` variant: one new `msgspec.Struct` subclassing `Detail` with a new explicit short `tag`, one entry in the `AnyDetail` union, one `_DETAIL_DECODER` rebuild.
- Add an aspect: one new `Layer` factory in `core/aspect.py` + a new `Slot` member + a new seam entry. The `compose` sort picks it up automatically.
- Add an automation trigger/action: one tagged case on `Trigger`/`Action` unions in `automation/model.py`.

**Never** add a parallel type, a parallel param, a second rail shape, a new module for a program, or a helper file for indirection.
Functionality is never removed to reduce LOC — density is concept count, not byte count.

---
## [3][DEEP_EXTERNAL_LIB_STACK_AS_A_DISCIPLINE]

Use every library **at its intended power**. Hand-rolling a lower-level reimplementation is a first-class defect.

| [LIB] | [TIER] | [INTENDED SEAM] |
| ----- | ------ | --------------- |
| `msgspec` | core | `Struct` freeze/gc/hash; tagged-union `Detail`; cached `Encoder`/`Decoder`; `structs.replace` for dynamic arg splice; `Meta` constraints at decode. |
| `pydantic-settings` | core | `AssaySettings` only; `(init_settings, env_settings)` sources; `AliasChoices`; `model_copy(update=…)` for remote settings. |
| `expression` | core | `Result`/`Ok`/`Error` rails; `Block.fold`/`block.of_seq`/`block.collect` for concatMap; `@effect.result` for generator ROP only (never a plain returning function). |
| `anyio` | core | `create_task_group`; `CapacityLimiter`; `run_process`/`open_process`; `fail_after`; single `anyio.run` — never nested. |
| `cyclopts` | core | `App`; `Parameter(name="*")` flatten; `resolve_returncode`; `__cyclopts_returncode__`. |
| `beartype` | aspect | `@checked` seam at `BeartypeConf(strategy=O1)`; resolves forward-refs at CALL time. |
| `structlog` | aspect | `@logged`; `WriteLoggerFactory(sys.stderr)`; `bound_contextvars`; `ring_processor` appends the invocation ring in place. |
| `opentelemetry` | aspect | `@traced`; one span per call; `baggage`/`context` for fleet correlation; `BatchSpanProcessor`; gated on configured OTLP endpoint. |
| `stamina` | aspect | `@retried` on spawn only; `RetryHook` closes the baggage↔contextvars loop; never retries `BUSY`/`TIMEOUT`. |
| `psutil` | adopted | Lease liveness — `(pid, create_time)` steal stale holders; optional fleet CPU governor. |
| `watchfiles` | adopted | `awatch` debounced filesystem `Watch` trigger; anyio `stop_event`. |
| `aiocron` | adopted | `crontab(spec, start=False)` `Schedule` trigger under one task group; zero data-store. |
| `fsspec`/`universal-pathlib` | adopted | `ArtifactStore` backend abstraction; `UPath` for all path fields; `memory://` gives zero-IO test isolation. |
| `asyncssh` | adopted | `_run_remote` backend in `core/engine`; `conn.run`/`create_process` for `exec_target=ssh://…`. |

---
## [4][HARD_ANTI-SPAM_DOCTRINE]

Stop and collapse before merging any of these:

- Free `typing.Literal` aliases for vocabularies already on a `StrEnum`.
- `NamedTuple` CLI params — frozen `@dataclass` + Cyclopts `Parameter(name="*")` flatten only.
- Second model system on wire shapes (pydantic `BaseModel` for `Report`/`Envelope`).
- `Engine`/`Parser` `Protocol` — engine is module functions; `Parser = Callable[[Completed], AnyDetail | None]`.
- Per-rail report structs — `Report` + tagged `detail` union only.
- `Fault` with `returncode` or `detail` — it is `{argv, status=FAULTED, message}` only.
- `return match …` — `match` is statement form; bind in `case`, then `return`.
- `helpers.py`, `*Util`, `common_*`, single-call indirection.
- `@tool`/`@rail` registration decorators — registration is data rows.
- `@retried` on the rail runner; `@logged` on `run_check`.
- Stdout writes outside `_emit`.
- `worst(…)` — the fold is `RailStatus.join` (max-by-severity) + module `fold`.
- Parallel types modeling one concept (≥3 triggers the collapse).

---
## [5][LOCKED_ENGINEERING_PRINCIPLES]

*Absorb these as durable principles; do not re-litigate.*

**(a) PEP 758 / ruff format canonical form.** Python 3.14 ships PEP 758 — parenless `except A, B, C:` is valid AND is what `ruff format --target-version py314` produces (it strips grouping parens). Never re-add parens; `ruff format --check` will fail if you do.

**(b) `@checked` beartype / unconditional imports.** `beartype` resolves a function's forward-ref annotations from its module's `__globals__` at first CALL, not at import. Any type in a `@checked`-wrapped signature must be imported **unconditionally** (never under `TYPE_CHECKING`). The `# noqa: TC001` markers on those imports and the `# unconditional for beartype` comments are load-bearing — removing them silently disarms runtime shape validation. Static gates (ruff/ty/mypy/`py_compile`/import) all pass on a function whose beartype annotations are shadowed under `TYPE_CHECKING`; the crash only surfaces at the first real call.

**(c) Engine seam type suppressions are irreducible.** Two suppressions in `core/engine.py` and `core/aspect.py` cannot be retired by explicit parametrization — they only relocate between checkers. `compose_spawn(retried())(_guarded)` keeps one mypy `[arg-type]` (no-arg generic factory; mypy binds to `Never`, ty specializes at apply). The `@wraps`-induced async `_Woven` keeps one `[assignment]`/`ty:[invalid-assignment]` (`Hom`'s sync `Result` cannot unify with `Coroutine`). Do not re-chase.

**(d) `traced` dispatches on `inspect.iscoroutinefunction`.** One factory owns both modalities. The async `awoven` branch awaits `fn(*a, **k)` inside the span context-manager so the span lifetime, baggage bind, and `_stamp` status wrap the real async work — not coroutine creation.

**(e) Engine `_argv` / `structs.replace` splice.** Rails splice DYNAMIC args (pattern/project/input) into `tool.command` via `msgspec.structs.replace(tool, command=…)`, producing a new `Tool` with an updated `command`. `Check.paths` carries file-path scope only. Never stuff dynamic args into `Check.paths`.

**(f) `_WRITES`/`_RING` are per-invocation `ContextVar`s.** The automation loop reuses `rail()` per fire; both vars are set fresh in the `try` block of `rail.run` and reset in the `finally`. A process-static `count()` would fault every fire after the first. Do not hoist them out of the per-invocation scope.

---
## [6][HOW_TO_CHECK_FOR_REAL_BUGS]

The static gate is necessary but not sufficient. A tool that passes all six static checks — `ruff check`, `ruff format --check`, `ty check`, `mypy --strict`, `py_compile`, `import tools.assay._TMP.__main__` — can still crash on every invocation due to beartype forward-ref shadowing, async-span wrapping, or nested-anyio defects. These only surface at runtime.

**Always verify a change by running it:**

```bash
uv run python -m tools.assay._TMP <claim> <verb>          # minimal smoke
uv run python -m tools.assay._TMP static plan             # zero-spawn plan fold
uv run python -m tools.assay._TMP api doctor              # api health with ilspy
uv run python -m tools.assay._TMP self-test               # preflight census
```

Inspect the single stdout Envelope; structlog diagnostics ride stderr. A `faulted` Envelope with `error_context.hint` names the failing step. Treat every "all green" static claim in docs as a hypothesis to re-verify at runtime — not established fact.

---
## [7][OWNERSHIP_TABLE]

| [PATH] | [OWNS] | [NEVER] |
| ------ | ------ | ------- |
| `core/status.py` | `RailStatus`, `join`, `fold`, `from_returncode` | Second status enum or `status: str` field |
| `core/model.py` | Axis enums, `Tool`, `Check`, `Report`, `Detail`, `Envelope`, `Artifact`, `Match`, `Counts`, `Bind`, `BaseParams`, `Parser` alias | Env settings, CLI params, per-program argv builders |
| `core/engine.py` | `run_check`, `fan_out`, `exclusive_lease`, psutil liveness | Rail folds, Cyclopts, `Engine`/`Parser` Protocol |
| `core/routing.py` | `route`, `place(routed, tool, *, settings)`, `Scope`, `Source` Protocol (sole justified Protocol) | Catalog rows, Envelope emit |
| `core/aspect.py` | `checked`/`logged`/`traced`/`retried`, `compose`/`compose_spawn`/`assemble`/`_once`, `Slot`, `ring_processor` | Inline structlog/otel/stamina in rails |
| `composition/settings.py` | `AssaySettings`, `ArtifactScope`, `ArtifactStore` (fsspec seam), `Configuration`/`LogFormat` | msgspec wire structs |
| `composition/catalog.py` | `TOOLS` rows, `select(claim, language)`, parser functions | Handlers, registry, inline parser bodies |
| `composition/registry.py` | `REGISTRY` binds, `rail`, `_emit` (sole stdout writer), `build_app` | Tool argv logic |
| `rails/{static,test,docs}.py` | One `thin_rail` + thin adapters → `Result[Report, Fault]` | New status types, helper modules |
| `rails/{bridge,package,api}.py` | Bespoke folds + typed `Detail` variants | Catalog argv, second stdout writer |
| `automation/model.py` | `Trigger`/`Action` tagged unions | Treating automation as a `Claim` |
| `automation/engine.py` | One anyio task-group drive loop (watchfiles/aiocron), one Envelope per fire | A second `_emit`, per-fire `BUSY` retry |

---
## [8][VALIDATION_LADDER]

After any `.py` change:

```bash
uv run ruff check tools/assay/_TMP
uv run ruff format --check tools/assay/_TMP
uv run ty check tools/assay/_TMP
uv run mypy --strict --explicit-package-bases tools/assay/_TMP
```

Then run the tool: static-only passes mean nothing without a runtime smoke call.
Wire law: `encode(decode(x)) == x` for `Envelope`/`Report`; an unknown `detail` field fails decode.
Markdown-only edits require no static/test rail unless the user requests proof or move-only preservation fails (`git diff --check`).
