# [H1][CYCLOPTS_API_RESEARCH]
>**Dictum:** *Cyclopts 4.16 owns argv parsing and the command tree; assay owns one Envelope on stdout and RailStatus on exit.*

Verified: `uv run python -c "import cyclopts; print(cyclopts.__version__)"` → `4.16.1`. API truth: installed `cyclopts` package + live probes below.

---
## [1][CORE_API]
>**Dictum:** *Set `result_action` on the root `App`; pass it again on `__call__`; resolve exit from the returned object.*

| [SYMBOL] | [4.16.1 BEHAVIOR] |
| -------- | ----------------- |
| `App(...)` | `name`, `help`, `default_parameter`, `config`, `group`/`group_commands`, `validator`, `backend`, `result_action`. Constructor default `result_action=None`; `__call__` fallback is `"print_non_int_sys_exit"` unless overridden. |
| `App.command(obj, name=..., help=...)` | Mounts a `Callable` or nested `App`; returns decorator when `obj` omitted. Loop-safe. |
| `App.meta` | Child `App` sharing help/version/group/result_action; `@app.meta.default` is the global pre-pass. |
| `App.__call__(tokens, *, result_action=..., backend=...)` | Parses, runs command, applies `result_action`. Assay: always `result_action="return_value"`. |
| `App.parse_args(tokens)` | `(command, bound, ignored)` — `ignored` maps `Parameter(parse=False)` names to types (DI seam). |
| `resolve_returncode(result, default=0)` | Reads `__cyclopts_returncode__()` when present; else `default`. Export from `cyclopts`. |
| `Parameter(...)` | `name`, `converter`, `validator`, `group`, `parse`, `show_default`, `env_var`, … |
| `Group(name, sort_key=..., validator=...)` | Help sections + optional group validators. |
| `config.Env(prefix)` / `config.Toml(path)` | Config providers; composable via `App(config=(...))`. CLI tokens win over config-injected tokens when both set the same argument (Cyclopts token merge). |
| `validators.Number`, `Path`, `MutuallyExclusive`, `all_or_none` | Bound via `Parameter(validator=...)`. |
| `backend="asyncio"\|"trio"` | Required on `__call__` when the matched command is `async def`. |

**Meta-app (globals before dispatch):**
```python
app = App(name="assay", result_action="return_value", default_parameter=Parameter(show_default=False))

@app.meta.default
def meta(*tokens: str, json: bool = False, strict: bool = False) -> int:
    # configure structlog renderer from json; bind strict contextvar
    code = app(tokens, result_action="return_value")
    return code if isinstance(code, int) else resolve_returncode(code, default=0)
```

**`result_action` + return-code protocol:** `"return_value"` returns the handler result unchanged — it does **not** auto-call `resolve_returncode`. Built-in actions like `"print_non_int_sys_exit"` do. Assay must pick one exit path:

```python
code = app(argv, result_action="return_value")  # int path (main.md)
return code if isinstance(code, int) else 0
# Envelope path: return resolve_returncode(app(argv, ...), default=0)  # protocol verified → 42
```

```python
class Envelope(msgspec.Struct, frozen=True, kw_only=True):
    status: RailStatus
    def __cyclopts_returncode__(self) -> int:
        return self.status.exit_code
```

**`Parameter(name="*")` flatten (NamedTuple params):** lifts struct fields to verb-level flags. Cyclopts 4.16 requires a default when all fields are optional:

```python
def _adapt(row: Row) -> Callable[..., int]:
    ann = Annotated[row.params, Parameter(name="*")]
    def command(params: ann = row.params()) -> int:  # e.g. StaticParams() — verified required
        return rail(row, params)
    command.__annotations__ = {"params": ann, "return": int}
    return command
```

Verified: `app(["static","run","--strict"], result_action="return_value")` → `2` with `NamedTuple(strict: bool = False)`.

**Config providers:**
```python
app = App(
    config=(cyclopts.config.Env("ASSAY_"), cyclopts.config.Toml("assay.toml")),
    default_parameter=Parameter(show_default=False),
)
```
`Env(prefix="ASSAY_")` uppercases/transforms keys; `command=True` adds command-path segments to the prefix.

**Validators / async (verified probes):**
```python
@app.default
def run(n: Annotated[int, Parameter(validator=validators.Number(gte=0))] = 0) -> int: ...

@app.default
async def run() -> int:
    await asyncio.sleep(0)
    return 7
# app([], backend="asyncio", result_action="return_value") → 7
```

---
## [2][REGISTRY_TREE]
>**Dictum:** *One fold over `REGISTRY`; `setdefault` per `Claim`; generic `_adapt` per row.*

```python
from cyclopts import App, Parameter, resolve_returncode
from typing import Annotated

app = App(name="assay", help="Rasm polyglot quality operator.",
          default_parameter=Parameter(show_default=False), result_action="return_value")
subs: dict[Claim, App] = {}
for row in REGISTRY:
    subs.setdefault(row.claim, App(name=row.claim.value))
    subs[row.claim].command(_adapt(row), name=row.verb, help=row.help)
for sub in subs.values():
    app.command(sub)

def main(argv: list[str] | None = None) -> int:
    structlog.configure(...)  # stderr only — §4
    result = app(argv, result_action="return_value")
    match result:
        case int() as code: return code
        case None: return 0
        case obj if (rc := resolve_returncode(obj, default=0)) is not None: return rc
        case _: return 0
```

Dispatch: Cyclopts routes to the generated leaf → `rail(row, params)` → one `_emit` Envelope. No `match` on verb strings inside handlers.

---
## [3][FOOTGUNS_VS_QUALITY]
>**Dictum:** *Quality's tree and exit algebra are the anti-pattern assay retires.*

| [QUALITY (`tools/quality/__main__.py`)] | [ASSAY + CYCLOPTS] |
| --------------------------------------- | ------------------ |
| Four hand-built sub-apps `static`/`test_app`/`bridge`/`api` (L38–43) + `partial` loops (L469–476). | `REGISTRY` fold; adding a rail = one `Bind` row. |
| `rail[T]()` eight projectors + `emit_success` four callbacks (L195–348). | `Report`/`Fault` carry `RailStatus`; runner reads fields — zero projectors. |
| `api_exit_code(..., strict=True)` → **exit 2** on failed API payload (L443–446) while `RailStatus.FAILED` → **1** in `process.py`. | Single `RailStatus` algebra; `--strict` chooses `FAILED` vs planned `STRICT_FAILED` (exit 2) **inside the handler**, not Cyclopts. **Never** reuse quality's ad-hoc `2` without a named member. |
| `package_cmd` / `api_gate` string `match` on positionals (L111–127, L153–156). | Typed `Params` + `Parameter(name="*")`; parse errors pre-rail. |
| `main` returns `(app(argv)) or 0` only — no `resolve_returncode` (L486). | Explicit `result_action="return_value"` on **both** `App(...)` and `app(...)`; coerce `None` (help) → `0`. |
| Default `result_action=None` on sub-apps; only root sets `return_value`. | Set `result_action="return_value"` on root; inherit via `app.meta` child if used. |
| Mixed stdout: envelope JSON + possible handler prints. | **Only** `msgspec.json.encode(envelope)` on `stdout.buffer`; structlog → stderr. |

**Cyclopts-specific footguns:**
- Forgetting `result_action="return_value"` on `__call__` → default may `sys.exit` or print non-int returns.
- `Parameter(name="*")` without `params = Params()` default → `ValueError` at registration (4.16 `validate_command`).
- Meta-app `*tokens` greedily consumes shorts; prefer long flags (`--json`) or a pre-pass (Prefect pattern).
- Parse/validation errors: Cyclopts writes to **stderr**, exits non-zero, **no Envelope** — intentional pre-rail seam (`main.md` §5).
- `msgspec.Struct` for CLI params: unverified introspection; `NamedTuple` or `@dataclass`/`pydantic` confirmed for flatten.

---
## [4][STDOUT_DISCIPLINE]
>**Dictum:** *stdout is exactly one JSON line per matched verb; everything else is stderr.*

| [STREAM] | [WRITER] |
| -------- | -------- |
| `stdout.buffer` | `registry._emit` only: `msgspec.json.encode(envelope) + b"\n"` |
| `stderr` | `structlog` (`PrintLoggerFactory(file=sys.stderr)`), engine subprocess bytes, `Fault.message`, Cyclopts help/errors |

Handlers and aspects must not `print` payloads. Cyclopts `result_action` modes that print (`print_*`) are forbidden on assay's root `App`.

---
## [5][SELF_CRITIQUE]
>**Dictum:** *The registry fold is sound; the snippet is not yet coding-python grade.*

**Rating: 5/10** vs `coding-python`. The §2 `main` uses `match` with a fallback arm that papers over typing; `_adapt` is an imperative closure factory; `subs.setdefault` loop is clear but not a single expression fold; no `@effect.result` rail wrapper in the snippet.

**12/10 would:** (1) one `build_app(registry) -> App` pure constructor with `reduce`/`groupby` over `Claim`; (2) `rail` as `@effect.result` returning `Result[Envelope, Fault]` with aspects `traced > logged > retried > checked`; (3) leaf handler annotated `-> Envelope` and `main` as `return resolve_returncode(rail_out)` only — no `int | None` union; (4) meta-app sets `contextvars` via `parse=False` `AssaySettings` injected from `ignored` instead of env reads in the handler; (5) pin a one-file cyclopts contract test (flatten + `resolve_returncode` + `None` help) beside `tests/tools/assay/`.

---
## [6][FURTHER_CONSIDERATION]
- `parse_args` + `Parameter(parse=False)` injects `AssaySettings`/`Scope` via `ignored` (no contextvar).
- Lazy `App.command("module:attr", ...)` if the registry grows; `name="*"` on sub-`App` collapses single-verb claims.
- Exit **2**: reserve `STRICT_FAILED` in `status.md` before `--strict`; never quality's ad-hoc API `2`.
