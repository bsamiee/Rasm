# [H1][ROUTING]
>**Dictum:** *Routing is the one language-specific surface; everything it consumes flows through a single `Source`, everything it emits is one uniform `Routed`.*

## [1][PURPOSE]

`core/routing.py` folds a git change-set or explicit `paths` into routed inputs per `Language`. The strategy asymmetry — C# resolves owning-`.csproj` reverse-dependency closures from the project graph; Python/TypeScript/Bash/SQL/Docs resolve suffix globs from the change-set — lives here keyed by `Language.strategy` (`"closure"` | `"glob"`, D6) and never leaks into `Tool` rows, which stay uniform (ARCHITECTURE §17 invariant 6). There is no standalone `Strategy` enum (D6) and no `RouteScope` `Literal` (D24): `Scope` is a `StrEnum`. The module's public surface is `route` + `place`, backed by the `_dependents` fixpoint and the sole `Source` Protocol (D25); `route` injects a `Source`, so a sixth filesystem is a new `Source`, never a new `route` (D39).

## [2][CANONICAL_SHAPES]

`Scope(StrEnum){CHANGED,FULL}` is the route-scope axis (D24, `Routed.scope`). `Routed` is one shape across every `Language`; each strategy arm fills the relevant subset and leaves the rest empty. The C# `CLOSURE` arm populates `projects`/`groups`; the `GLOB` arm leaves both `()`. `Source` is the lone `Protocol` in the corpus (D25, ARCHITECTURE §5).

```python
type RoutePaths = tuple[str, ...]
type ProjectIndex = Mapping[str, str]                       # owner-dir -> root-relative .csproj

class Scope(StrEnum):                                        # D24 — not RouteScope Literal
    CHANGED = "changed"                                      # CHANGED grows a dependents closure
    FULL = "full"                                            # FULL lists the whole target

class Routed(Base, frozen=True):                            # Base = msgspec struct, gc=False, omit_defaults
    language: Language
    scope: Scope
    files: tuple[str, ...] = ()                              # sorted root-relative — FILES/INCLUDE/GLOB source
    projects: tuple[str, ...] = ()                           # dependents closure — PROJECT source
    groups: tuple[tuple[str, tuple[str, ...]], ...] = ()     # owner -> files — INCLUDE source (() for GLOB)
    full_triggers: tuple[str, ...] = ()                      # change-set rows that escalated scope to FULL

class Source(Protocol):                                      # D25 — the SOLE Protocol in assay
    def changed(self) -> Result[tuple[str, ...], Fault]: ...
    def enumerate(self, paths: RoutePaths) -> Result[tuple[str, ...], Fault]: ...
    def read(self, rel: str) -> Result[bytes, Fault]: ...    # .csproj XML for the CLOSURE arm
```

The default `LOCAL` binding roots every glob/read at `upath.UPath(AssaySettings().root)` (D39). `UPath` is a `pathlib` drop-in: a plain local root stays a `PosixUPath` byte-identical to `Path`, while a `memory://`/remote root reads (`read`) and enumerates the change-set with zero local IO. `_root` carries the `UPath`; `read` does `(_root / rel).read_bytes()`, `_expand` probes `(_root / target).is_dir()/.is_file()`, and the git/`fd` spawn passes `str(_root)` as `cwd` (the backend `path`, never the URI). Backend selection is therefore the root's protocol, not a `route` parameter or a parallel `Source`.

`route(language, paths, *, source, settings)` discriminates `paths` (explicit → `source.enumerate`; empty → `source.changed`), then binds the resolved rows into the strategy arm. `source` defaults to `None` and falls back to the module-level `LOCAL` binding internally (a default-`LOCAL` parameter would bind the singleton at def-time). `settings` is optional and threads the Tier-A FULL-escalation vocabulary (`settings.trigger_files`/`settings.trigger_prefixes`) into the `CLOSURE` arm via `_closure` → `_escalate`; `None` falls back to the `_TRIGGER_FILES`/`_TRIGGER_PREFIXES` module constants, so a settings-free `route(...)` (and the `memory://` property tests) keep the canonical vocabulary while a rail-supplied `settings` lets the operator widen the escalation set via `ASSAY_*` env. `Language.strategy` is the dispatch key, never the `Language` identity, so adding `BASH`/`SQL` (D36) adds no arm.

```python
def route(language: Language, paths: RoutePaths = (), *, source: Source | None = None, settings: AssaySettings | None = None) -> Result[Routed, Fault]:
    src = source if source is not None else LOCAL
    enumerated = src.enumerate(paths) if paths else src.changed()
    return enumerated.bind(
        lambda files: _closure(language, files, src, settings) if language.strategy == "closure" else _glob(language, files)
    )

def _escalate(files: tuple[str, ...], settings: AssaySettings | None) -> tuple[str, ...]:
    trigger_files = settings.trigger_files if settings is not None else _TRIGGER_FILES         # None -> module constant fallback
    trigger_prefixes = settings.trigger_prefixes if settings is not None else _TRIGGER_PREFIXES
    return tuple(f for f in files if f in trigger_files or any(f.startswith(prefix) for prefix in trigger_prefixes))
```

`place(routed, tool, *, settings)` is the sole argv-tail projector (D23, ARCHITECTURE §17 invariant 6); it is a *total projection* from `Routed` keyed by the `Input` axis, never by `Language`, and needs `settings` for the `SOLUTION` path.

| [INPUT]    | [PROJECTION FROM `Routed`]                                  | [FAN] | [CONSUMERS]                                              |
| ---------- | ---------------------------------------------------------- | :---: | ------------------------------------------------------- |
| `FILES`    | `(routed.files,)` appended to the command                  |   1   | `ruff`, `ty`, `mypy`, `tsc`, `biome`, `pytest`, `vitest`, `shellcheck`, `shfmt`, `sqlfluff` |
| `INCLUDE`  | `(project, "--include", *files)` per `routed.groups`       |   N   | `dotnet format`                                         |
| `PROJECT`  | `(project,)` per `routed.projects`                         |   N   | `dotnet restore`/`build`                                |
| `SOLUTION` | `(str(settings.solution),)`                                |   1   | full-scope build, `dotnet sln` (D23 — needs `settings`) |
| `GLOB`     | `**/*<suffix>` per `language.suffixes` (via `_globs`)      |   1   | `ast-grep`, `dotnet-stryker`, `squawk`                  |
| `NONE`     | `()`                                                       |   1   | scopeless analyzers                                     |

## [3][VALIDATED_SNIPPET]

The `place` total projection (D23, statement-form `match` per D27) plus the `_dependents` reverse-reachability fixpoint in pure `frozenset` set-algebra. `_dependents` grows seeds by every project whose `ProjectReference` set intersects the current closure, iterated `len(graph)` times to a guaranteed fixpoint — the *affected-dependents* set (changing `P` rebuilds everything referencing `P`), not the dependency set. `_refs` is memoized by the per-run `graph` dict built once inside `_dependents` — never a process-static `@cache` that would bleed across runs of a mutated worktree. `place` closes its six-arm `match` with `assert_never`, so a seventh `Input` is a type error rather than a silent empty tail. The fixpoint never raises: a `.csproj` read fault or unparseable XML degrades to `frozenset()` (an isolated node) inside `_refs`, keeping `_dependents` total.

```python
from functools import reduce
from typing import assert_never

def place(routed: Routed, tool: Tool, *, settings: AssaySettings) -> tuple[tuple[str, ...], ...]:
    match tool.input:                                       # D27 — statement form; never `return match`
        case Input.FILES:    return ((*routed.files,),) if routed.files else ()
        case Input.INCLUDE:  return tuple((project, *Input.INCLUDE.flag, *files) for project, files in routed.groups)
        case Input.PROJECT:  return tuple((project,) for project in routed.projects)
        case Input.SOLUTION: return ((str(settings.solution),),)
        case Input.GLOB:     return ((*_globs(routed),),)
        case Input.NONE:     return ((),)
        case never:          assert_never(never)            # exhaustive — seventh Input is a type error

def _dependents(seeds: frozenset[str], index: ProjectIndex, source: Source) -> frozenset[str]:
    graph = {rel: _refs(rel, source) for rel in index.values()}          # _refs memoized by this per-run dict
    return reduce(
        lambda current, _: current | frozenset(p for p, refs in graph.items() if p not in current and bool(current & refs)),
        range(len(graph)),
        seeds,
    )
```

## [4][SEAMS]

| [NEIGHBOR]                 | [SEAM]                                                                                          |
| -------------------------- | ---------------------------------------------------------------------------------------------- |
| `core/model.py`            | Consumes `Language`/`Input`/`Tool`/`Fault`/`Base`; `Routed` is a `Base` (`msgspec` struct) (§4). `Scope` is owned **here** (ARCHITECTURE §5 enum-home table), imported by `model`/`engine` for `Check`/receipt context. |
| `core/engine.py`           | `run_check(check, *, settings, scope, routed)` takes `Routed` as a parameter (D8) and calls `_argv` → `place(routed, tool, *, settings)` to build the argv tail per fanned `Check`. `route` is invoked once per rail before `fan_out`. |
| `composition/settings.py`  | `place` reads `settings.solution` (D23); `_LocalSource._root` wraps `AssaySettings().root` in `upath.UPath`. The FULL-escalation vocabulary is `settings.trigger_files`/`settings.trigger_prefixes` (Tier-A env-overridable) threaded through `route(..., settings=)` → `_closure` → `_escalate`; `routing` retains `_TRIGGER_FILES`/`_TRIGGER_PREFIXES` as the `settings=None` fallback so a settings-free `route` (and property tests) keep the canonical set. Routing owns the escalation *mechanism*; the *vocabulary* is operator-tunable via `ASSAY_TRIGGER_FILES`/`ASSAY_TRIGGER_PREFIXES`. The `LOCAL` `Source` binds git+`fd`+`UPath`; the `UPath` root selects the fsspec backend (local `PosixUPath` default, `memory://`/remote on demand), so a new backend is a root change, not a new `Source` (D39). |
| `composition/catalog.py`   | `Tool.input`/`Tool.language` rows drive which `place` arm fires; `route`/`place` never name a `Tool` by `name`. |
| `rails/{static,test,docs}` | Thin rails call `route` once, then `place` per selected `Tool` row, then `fan_out`; the C#-only `bridge`/`package`/`api` rails route through the same `CLOSURE` arm or `NONE`. |

Shared discovery is language-agnostic: `changed()` is the sorted-deduped union of three git probes (`git diff --name-only --diff-filter=ACDMRTUXB` ∪ `git diff --cached …` ∪ `git ls-files --others --exclude-standard`); `enumerate(p)` expands a dir via `fd -H -t f . <dir> --exclude …`, passes a file verbatim, and faults on a missing path. Discovery never knows the requesting `Language`.

## [5][EXTENSIBILITY]

A new `Language` is one `model.Language` member carrying `strategy`+`suffixes`; a glob language adds zero routing code (D36, BASH/SQL already day-one). A new filesystem is either a new `UPath` root for the `LOCAL` binding (`memory://`, `s3://`, `filecache::s3`) or — for a non-fsspec change-set provider — one `Source` implementation injected at `route(..., source=)`; the `route` signature, `Language.strategy` dispatch, and `Routed` shape are untouched (D25/D39).

## [6][CONSIDERATIONS]

- **Fixpoint bound is exact, not heuristic.** `reduce(…, range(len(graph)), seeds)` iterates once per node, the worst-case longest reverse-reachability chain in a DAG; each pass is monotone-growing under union, so the closure is reached no later than the final iteration and re-running a settled set is idempotent. Replacing `range(len(graph))` with an early-stop `while current != prev` would re-introduce imperative branching (ARCHITECTURE §4 prohibition) for a sub-linear constant-factor win on already-cached graphs — keep the bounded `reduce`.
- **`scope="full"` is a pre-plan fast path, not a fold outcome.** When the change-set carries a trigger-file member or a trigger-prefix descendant, the `CLOSURE` arm emits `Routed(scope=FULL, full_triggers=…)` and skips owner/closure resolution entirely (no `source.enumerate`, no `_dependents`); `place` then reads `SOLUTION`. Escalation is a pure `_escalate(files, settings)` set-membership test that reads `settings.trigger_files`/`settings.trigger_prefixes` when a rail supplies `settings` and falls back to the `_TRIGGER_FILES`/`_TRIGGER_PREFIXES` module constants when `None`, so the common single-file edit stays at one `fd`/git probe + one bounded `reduce`; full-scope parity verification lives downstream in the engine, off this hot path.
- **`Source.read` is the worktree-portability lever, now realized via `UPath`.** Because the `CLOSURE` arm parses every `.csproj` via `source.read` and `LOCAL` reads through `upath.UPath` (never `pathlib.Path.read_bytes`), rooting `_LocalSource._root` at `memory://` resolves the project graph with zero local IO for property-based routing tests, and a `filecache::s3` archived-worktree root resolves the same closure unchanged — the graph algorithm is filesystem-blind by construction (D39). Verified: a `memory://` worktree of `.csproj` nodes yields the identical reverse-dependency closure with no disk touch. Local routing is byte-identical because a plain root is a `PosixUPath` (`pathlib` drop-in); the only URI-aware seam is `cwd=str(_root)` at the git/`fd` spawn, which the URI-free local path satisfies unchanged.
