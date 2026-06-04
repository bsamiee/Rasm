# [H1][ASSAY_MAIN_DESIGN]
>**Dictum:** *The command tree is a projection of REGISTRY; stdout carries one Envelope; the exit code is RailStatus.*

`__main__.py` owns four seams only: build the Cyclopts tree from `REGISTRY`, configure `structlog` to stderr, run the matched rail runner (which already emits the Envelope), and convert its return into `SystemExit`. No verb, flag, or sub-app is hand-written. Verified against `cyclopts==4.16.1`, `structlog==25.5.0`, Python `>=3.14`.

**Canonical:** [`snippets/cli.py.md`](snippets/cli.py.md) · [`registry.md`](registry.md) · [`AOT.md`](AOT.md) §5 (structlog at `tools/assay/__init__.py` import). Leaves return **`Envelope`**; `resolve_returncode` reads `Envelope.__cyclopts_returncode__`.

---
## [1][TREE_FROM_REGISTRY]
>**Dictum:** *Iterate rows; never name a command.*

`REGISTRY` (in `composition/registry.py`) is the single source of truth: a `tuple[Bind, ...]` where each `Bind` carries `claim: Claim`, `verb: str`, `params: type[Params]` (frozen `@dataclass`), `handler: Callable[[AssaySettings, ArtifactScope, ParamsT], Result[Report, Fault]]`, and `help: str`. `__main__` folds rows into one app:

```python
app = App(name="assay", help="Rasm polyglot quality operator.", default_parameter=Parameter(show_default=False))
subs: dict[Claim, App] = {}
for row in REGISTRY:
    sub = subs.setdefault(row.claim, App(name=row.claim))
    sub.command(_adapt(row), name=row.verb, help=row.help)
for sub in subs.values():
    app.command(sub)
```

- `setdefault(row.claim, App(...))` materializes one sub-app per claim (`static`, `test`, `bridge`, `package`, `api`, `docs`) lazily from the rows — no fixed sub-app list.
- `_leaf(row)` returns one generic closure (see §4); `sub.command(obj, name=...)` accepts a callable directly, so registration is data, not decorators.
- Adding a rail or verb is a new `REGISTRY` row plus a handler; `__main__` is never edited. This satisfies INVARIANT 3 and EXTENSIBILITY rows [1]/[3].

---
## [2][STRUCTLOG_DISCIPLINE]
>**Dictum:** *Diagnostics to stderr; stdout is the Envelope and nothing else.*

`main` configures `structlog` before running the app; the renderer is selected by output mode (human vs CI), and the sink is always `stderr`:

```python
renderer = structlog.processors.JSONRenderer() if json_logs else structlog.dev.ConsoleRenderer()
structlog.configure(
    processors=(
        structlog.contextvars.merge_contextvars,        # claim/verb/run_id bound by the runner
        structlog.processors.add_log_level,
        structlog.processors.TimeStamper(fmt="iso", utc=True),
        renderer,
    ),
    logger_factory=structlog.PrintLoggerFactory(file=sys.stderr),
    cache_logger_on_first_use=True,
)
```

- `merge_contextvars` folds the runner's `bound_contextvars(claim=, verb=, run_id=)` into every line; ISO/UTC timestamps; `ConsoleRenderer` for humans, `JSONRenderer` for CI.
- The **only** writer to `sys.stdout.buffer` is `_emit` in the runner (`msgspec.json.encode(envelope) + b"\n"`). Engine subprocess bytes, faults, and logs route to `stderr`. Structlog configure lives in `tools/assay/__init__.py` per `AOT.md` §5 (not duplicated in `__main__.py` when import order is pinned).

---
## [3][MAIN_FLOW]
>**Dictum:** *Configure, run, exit; the runner already spoke.*

```python
# tools/assay/__main__.py — see snippets/cli.py.md
app = build_app(REGISTRY)

@app.meta.default
def meta(*tokens: str) -> int:
    result_obj = app(tokens, result_action="return_value", backend="asyncio")
    return match result_obj:
        case None: 0
        case env: resolve_returncode(env)   # Envelope.__cyclopts_returncode__ -> exit_code

def main(argv: list[str] | None = None) -> int:
    return meta(*([] if argv is None else argv))

if __name__ == "__main__":
    raise SystemExit(main())
```

- The rail runner returns an **`Envelope`**; `resolve_returncode` maps `RailStatus.exit_code`. `__main__` never builds an Envelope.
- **No-args / help:** returns `None` → exit `0`; no Envelope (`snippets/cli.py.md` §1 contract test).
- `result_action="return_value"` + `resolve_returncode(Envelope)` pins one exit path.

---
## [4][PARAM_MAPPING]
>**Dictum:** *Cyclopts flattens the params type; no projector exists.*

Each verb's parameters are one frozen `@dataclass` (`bind.params`). One generic adapter binds it without per-command code, using `Parameter(name="*")` to lift fields to top-level tokens:

```python
def _leaf(bind: Bind) -> Callable[..., Envelope]:
    runner = rail(bind)
    ann = Annotated[bind.params, Parameter(name="*")]
    def command(params: ann = bind.params()) -> Envelope:
        return runner(params)
    command.__annotations__ = {"params": ann, "return": Envelope}
    return command
```

- Verified: `Annotated[P, Parameter(name="*")]` expands dataclass fields into positional args and `--flags` under the verb (`assay static build --strict`).
- **Pinned:** frozen `@dataclass` for CLI params (`TID251` bans `typing.NamedTuple`); `msgspec.Struct` stays wire-only. Handler receives a fully-typed `Params` instance.
- Setting `command.__annotations__` lets one closure serve every row; the type is data carried by the bind.

---
## [5][EXIT_AND_ONE_ENVELOPE]
>**Dictum:** *Exit code is RailStatus; one Envelope even on fault.*

The exit code originates solely from `RailStatus.exit_code` payloads; `__main__` performs zero status arithmetic.

| [STATUS]            | [EXIT] |
| ------------------- | :----: |
| `ok` / `empty` / `skip` | 0 |
| `failed`            | 1 |
| `unsupported`       | 3 |
| `busy` / `timeout`  | 5 |

- Success path: handler `Report.status -> RailStatus -> exit_code`. Fault path: `Fault.status` (engine non-zero, `busy` from a held lease, `timeout`, `unsupported` language) maps the same way. Faults are `Result`, never raised across the rail boundary (CORE_NEEDS 4).
- **One-Envelope-on-fault:** the runner wraps the handler call so any unexpected exception is caught, converted to a `Fault(status=FAILED)`, emitted as the single Envelope (exit 1), and logged to stderr. Thus every matched invocation emits exactly one line.
- **Pre-rail boundary:** a Cyclopts parse error (unknown verb/flag) precedes runner entry — Cyclopts writes the error to stderr and exits non-zero with no Envelope. This is the deliberate seam: the one-Envelope guarantee covers matched rails, not malformed argv.

---
## [6][OPEN_DECISIONS]
>**Dictum:** *Name the unresolved seams; do not pre-bake them.*

- **Global flags (`--json`, `--strict`):** model as a Cyclopts *meta app* (`@app.meta.default` wrapping `app(tokens)`) so both flags apply across every registry verb without per-row edits. `--json` selects the stderr renderer (§2); `--strict` promotes `empty`/`skip`/warn into a non-zero exit by re-mapping `RailStatus.exit_code` at the runner via a contextvar — **not** by branching in `__main__`. Alternative: env-only via `AssaySettings` (`ASSAY_LOG_FORMAT`, `ASSAY_STRICT`); decide between agent-ergonomic flags and pure-env config.
- **Watch mode integration:** `watch` is one `REGISTRY` row whose handler loops `watchfiles.watch` and re-invokes the inner rail runner per change. This collides with INVARIANT 1: a long-lived watch emits *many* Envelopes. Open: (a) emit one JSONL Envelope per cycle (relax the rule to "one Envelope per *rail cycle*"), or (b) emit a single terminal summary Envelope on exit and keep per-cycle results on stderr. Leaning (a) for agent streaming; needs an explicit `--watch` contract on the entrypoint.
- **Async backend:** rails fan out under `anyio`; Cyclopts 4.16 `__call__` accepts `backend="asyncio"|"trio"`. Decide whether `main` selects a backend globally or each handler owns its own `anyio.run`, to avoid nested event loops at the entrypoint.
