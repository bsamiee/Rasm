# [H1][CYCLOPTS_FIELD_RESEARCH]
>**Dictum:** *World-class Cyclopts apps register commands from data, push globals into a meta-app, and let the return value own the exit code — exactly assay's three seams.*

Scope: real Cyclopts ≥3/≥4 apps (last ~12 mo), official advanced docs, distilled into a registry-driven pattern for assay. Verified against `cyclopts==4.16.1` API (`https://cyclopts.readthedocs.io/en/latest/api.html`).

---
## [1][CONCRETE_EXAMPLES]
>**Dictum:** *Six shipping CLIs; note how each builds its tree and where its globals live.*

| [PROJECT] | [URL] | [PIN] | [WHAT TO STEAL] |
| --------- | ----- | ----- | --------------- |
| **PrefectHQ/prefect** | `https://github.com/PrefectHQ/prefect/blob/main/src/prefect/cli/_app.py` | `cyclopts>=4.8.0` | **Data-driven lazy registration:** ~29 sub-apps added as rows `_app.command("prefect.cli.deploy:deploy_app", name="deploy", alias="deployments", help=...)`; the `"module:attr"` string is imported only on dispatch. Globals (`--profile`,`--prompt`) live in `@_app.meta.default` (a root callback) which does env setup then `_app(tokens)`. `help_flags`/`version_flags` tuned; a `_normalize_top_level_flags` pre-pass rewrites short flags before meta parses. |
| **mopidy/mopidy** | `https://github.com/mopidy/mopidy/blob/main/src/mopidy/_app/extensions.py` | `cyclopts>=3.12` | **Plugin registry → tree fold:** each extension record carries an optional `command: cyclopts.App`; `init_commands(app)` loops enabled records and mounts each sub-`App`. Tree is a projection of a record table, not hand-wired. |
| **so-rose/blext** | `https://github.com/so-rose/blext/blob/main/blext/cli/_context.py` | `cyclopts>=3.1.5` | **Shared config module:** one `_context.py` exports the root `APP`, a themed `rich.Console`, ordered `cyclopts.Group(...)` (sorted help sections), and `Annotated` param-type aliases (`ParameterConfig = Annotated[GlobalConfig, Parameter(name='cfg')]`). Sub-apps import these; config is centralized, not duplicated. Uses `Parameter(name='*')` to flatten a struct's fields to top-level tokens. |
| **gerlero/styro** | `https://github.com/gerlero/styro/blob/main/src/styro/__main__.py` | `cyclopts>=3.24,<5` | **Async-native:** every command is `async def`; `App(version=_version_callback)` where the callback is itself `async` (network check). Minimal, clean `__main__`. |
| **cpatrickalves/plane-cli** | `https://github.com/cpatrickalves/plane-cli` | `cyclopts>=3.0` | Per-resource sub-app modules (`commands/states.py`, `work_items.py`) each owning one `App`, composed at root — clean noun→verb topology. |
| **uw-cryo/lidar_tools** | `https://github.com/uw-cryo/lidar_tools/blob/main/src/lidar_tools/cli.py` | `cyclopts>=4.3.0` | Scientific pipeline CLI on the 4.x line; single-file `App` with typed `Path`/`Literal` params and validators. |

---
## [2][PATTERNS_AND_ANTIPATTERNS]
>**Dictum:** *Steal data-as-tree and return-value-as-exit; reject per-command flag duplication and stdout pollution.*

**Steal:**
- **Registry → tree as a fold.** `App.command(obj, name=..., help=...)` accepts a function *or* an `App` and is callable in a loop/comprehension; both Prefect (string rows) and Mopidy (record rows) prove tree-from-data. No decorator is required (`https://cyclopts.readthedocs.io/en/stable/commands.html`).
- **Meta-app owns globals.** `@app.meta.default(*tokens, --global-flags)` → setup → `app(tokens)` is the canonical place for cross-cutting flags; inherits parent config and merges help (`https://cyclopts.readthedocs.io/en/latest/meta_app.html`).
- **Return value owns exit.** `result_action="return_value"` to embed/test; the `__cyclopts_returncode__()` protocol lets a *returned object* declare its own exit code (`https://cyclopts.readthedocs.io/en/v4.16.0/packaging.html`).
- **Centralized config module + `Group(sort_key=...)`** for ordered help and shared `Console`/param-type `Annotated` aliases (blext).
- **Flatten a params struct** with `Parameter(name='*')` so dataclass/NamedTuple fields become top-level tokens (blext; assay `main.md` §4).

**Avoid:**
- **Per-command global flags.** Re-declaring `--json/--strict` on every verb (instead of a meta-app) — N-way drift.
- **Printing payloads to stdout from handlers** (Mopidy/styro `print` freely) — fatal for assay's one-Envelope contract; route all diagnostics to stderr.
- **`match`/dict dispatch *inside* a mega-command** (predecessor `package_cmd`) — collapses types and re-creates stringly routing.
- **Eager imports of all command modules** — Prefect explicitly uses lazy `"module:attr"` strings to avoid 29-module startup cost.
- **Relying on `version_flags`/`help_flags` defaults** when subcommands also accept `--version` (Prefect disables them and short-circuits manually).

---
## [3][BEST_REFERENCE_PATTERN]
>**Dictum:** *Prefect's row-registration × the `__cyclopts_returncode__` protocol = REGISTRY → tree → one Envelope → RailStatus exit, with zero per-rail projector.*

The single best distillation: make the **Envelope implement `__cyclopts_returncode__`**, register **every leaf by folding `REGISTRY`**, and let Cyclopts derive the exit code from the returned Envelope — the handler returns evidence, never an int.

```python
# composition/registry.py  (verified shapes vs cyclopts 4.16.1)
from typing import Annotated
from cyclopts import App, Parameter

class Envelope(msgspec.Struct, frozen=True, gc=False, kw_only=True):
    rail: str; verb: str; status: RailStatus; data: Report | None = None; error: Fault | None = None
    def __cyclopts_returncode__(self) -> int:        # cyclopts reads this to set sys.exit code
        return self.status.exit_code

def _leaf(bind: Bind):                                # one generic adapter, no per-verb code
    annotated = Annotated[bind.params, Parameter(name="*")]   # flatten struct fields to tokens
    def command(params: annotated) -> Envelope:               # returns the Envelope, not an int
        return rail(bind)(params)                             # rail emits one JSON line to stdout, returns Envelope
    command.__annotations__ = {"params": annotated, "return": Envelope}
    command.__name__ = bind.verb
    return command

def build(registry: tuple[Bind, ...]) -> App:
    app = App(name="assay", help="Rasm polyglot quality operator.", result_action="return_value")
    subs = {c: app.command(App(name=c)) for c in dict.fromkeys(b.claim for b in registry)}  # one sub-App per claim
    [subs[b.claim].command(_leaf(b), name=b.verb, help=b.help) for b in registry]           # fold rows → leaves
    return app
```

`result_action="return_value"` keeps `app(argv)` from calling `sys.exit` internally (`__main__` owns the `SystemExit`); the returned Envelope's `__cyclopts_returncode__` supplies the code (`https://cyclopts.readthedocs.io/en/latest/app_calling.html`). The `{c: ... for c in dict.fromkeys(...)}` comprehension materializes sub-apps without a mutable `setdefault` loop. This is Prefect's row-registration generalized to one fold + Mopidy's "App-as-command" + the return-code protocol assay already wants.

---
## [4][UNDERUSED_CAPABILITIES_IDEAL_FOR_ASSAY]
>**Dictum:** *The agent-first features almost nobody ships are the ones assay most needs.*

- **`__cyclopts_returncode__` protocol** — return the Envelope, drop the entire `exit_code` projector ladder. Near-zero adoption in the wild; perfect for a status-bearing payload.
- **Meta-app global flags** (`--json`,`--strict`): one `@app.meta.default` re-maps the renderer + a `strict` contextvar across all verbs; assay's `main.md` §6 already plans this — Prefect proves it at scale.
- **`cyclopts.config.Env("ASSAY_")` + `Toml(...)`** with documented precedence (CLI > Env > TOML > default) gives agent env-binding for free; no manual `os.environ` reads (`https://cyclopts.readthedocs.io/en/stable/config_file.html`).
- **`Parameter(validator=validators.Number(gte=…))` / `validators.Path(exists=True)`** and **group validators** (`MutuallyExclusive`, `all_or_none`) push input law to the parse boundary — argv errors fail *before* the rail (the deliberate pre-Envelope seam in `main.md` §5) (`https://cyclopts.readthedocs.io/en/stable/parameter_validators.html`, `.../group_validators.html`).
- **Async backend** (`app(tokens, backend="asyncio")`) — styro proves async commands; assay's `anyio` fan-out can ride this for the watch/bridge rails.
- **`name="*"` sub-app flattening** — collapse a single-verb claim (`assay docs` not `assay docs check`) without restructuring REGISTRY.
- **AutoRegistry + `Literal[tuple(registry)]`** to auto-derive a parameter's choice set from a registry (`https://cyclopts.readthedocs.io/en/latest/autoregistry.html`) — assay's `Claim`/`Language` StrEnums already serve this role natively.

---
## [5][PREDECESSOR_CONTRAST]
>**Dictum:** *`tools/quality/__main__.py` hand-wired what the best projects fold from data; that asymmetry is the whole rewrite.*

| [LEGACY DEFECT (`tools/quality/__main__.py`)] | [BEST-PRACTICE FIX] |
| --------------------------------------------- | ------------------- |
| Four hand-named sub-apps (`static`,`test_app`,`bridge`,`api`, L38–43) + two `partial()` registration loops (L469–476) — a second source of truth beside the verbs. | Prefect/Mopidy register from **one row table**; assay folds `REGISTRY` once (§3). Adding a rail = one row. |
| `rail[T]()` takes **eight projector callables** (`data/status/exit_code/notes/…`, L195–218) and `emit_success` four more — exit logic scattered per verb. | Return the Envelope; `__cyclopts_returncode__` derives exit. **Zero projectors.** |
| Per-verb exit math (`api_exit_code`, `bridge_exit_code`, `static_status`, L427–463) — many status algebras. | One `RailStatus` enum carries `exit_code` per member (assay `status.md`); nothing else computes a code. |
| `--strict` handled inside `api_gate` only (L145,176); no cross-verb global. | One `@app.meta.default` applies `--strict`/`--json` to every verb (§4). |
| Mega-commands re-parse strings: `package_cmd` `match slug` (L111–127), `api_gate` re-derives `kind`/`symbol` from a positional `value` (L153–156) — stringly routing. | Typed `Params` struct per leaf, flattened via `Parameter(name='*')`; the table *is* the dispatch — no in-command `match`. |
| Re-encode ladder (`quality_payload`→`payload_json`→`decode_json_value`→`json_value`, L375–415) shimming `Completed`→dict. | `Report`/`Envelope` are `msgspec.Struct`; serialize inline — no re-encode ladder. |

---
## [6][FURTHER_CONSIDERATION]
>**Dictum:** *Verify three sharp edges before committing the fold.*

- **Build-time confirm `__cyclopts_returncode__` fires under `result_action="return_value"`.** Docs state the protocol applies where a built-in action "would otherwise use `0`"; `return_value` *returns the object* rather than exiting, so `__main__` must call `cyclopts.resolve_returncode(envelope)` (or read `.status.exit_code`) explicitly. Pick one and pin it with a test (`https://cyclopts.readthedocs.io/en/v4.16.0/packaging.html`).
- **Esoteric — `App.parse_args(tokens)` in the meta returns `(command, bound, ignored)`,** where `ignored` maps `Parameter(parse=False)` names to their *types*; this is the documented seam to inject a shared `AssaySettings`/`Scope` object into every handler without it appearing on the CLI (Prefect-style DI, `meta_app.html`). Underused and cleaner than a contextvar for `--strict`.
- **Esoteric — short-flag greediness across the meta boundary.** Prefect had to add `_normalize_top_level_flags` because the meta's `*tokens` greedily consumes `-p` as a global before subcommand parsing. If assay adds any short global flag, the same pre-pass (or long-only globals) is mandatory — a non-obvious failure mode only a high-scale adopter surfaces.
- **Esoteric — `name="*"` flatten caveats:** parent commands win name collisions, no extra kwargs allowed, only `App` instances flatten (not functions/strings) — relevant if a claim is later collapsed to a single verb.
