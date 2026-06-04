# [H1][ASSAY_ROUTING_DESIGN]
>**Dictum:** *Routing is the one language-specific surface; everything it consumes is a shared source, everything it emits is one uniform `Routed`.*

`core/routing.py` turns a change-set or explicit paths into routed inputs per `Language`. The asymmetry — C# resolves owning-project closures from the `.csproj` graph; Python/TypeScript/Docs resolve suffix globs from the change-set — lives here keyed by `Language.strategy` (`"closure"` | `"glob"`) and never leaks into `Tool` rows, which stay uniform. **No standalone `Strategy` enum** — merged into `Language` (`model.md` §1).

**Canonical:** [`TYPE_SYSTEM.md`](TYPE_SYSTEM.md) §2 (`Language.strategy`) · [`model.md`](model.md) · [`CRITIQUE-SHAPES.md`](CRITIQUE-SHAPES.md) P1.6 (`RouteScope` Literal retired).

---
## [1][CONTRACT]
>**Dictum:** *One signature, two strategy arms, one `Routed`; `place(routed, tool)` projects argv tails.*

`Language` carries `strategy: str` and `suffixes` as `__new__` payloads (`model.md`). Dispatch matches `language.strategy == "closure"` vs `"glob"`.

```python
type RoutePaths = tuple[Path, ...]

class Scope(StrEnum):
    CHANGED = "changed"
    FULL = "full"

class Routed(msgspec.Struct, frozen=True, gc=False):
    language: Language
    scope: Scope                                     # CHANGED grows a closure; FULL lists the whole target
    files: tuple[str, ...]                            # sorted root-relative — FILES / INCLUDE / GLOB source
    projects: tuple[str, ...]                         # dependents closure (CLOSURE arm) — PROJECT / SOLUTION source
    groups: tuple[tuple[str, tuple[str, ...]], ...]   # owner -> files — INCLUDE source ((), for GLOB arm)
    full_triggers: tuple[str, ...]                    # change-set rows that escalated scope to FULL

def route(language: Language, paths: RoutePaths = (), *, source: Source = LOCAL) -> Result[Routed, Fault]:
    enumerated = source.enumerate(paths) if paths else source.changed()
    return enumerated.bind(
        lambda files: _closure(language, files, source) if language.strategy == "closure" else _glob(language, files)
    )
```

`Routed` is one shape across all languages; each arm fills the relevant subset and leaves the rest empty. The `Input` axis is a *total projection* from `Routed` to a tuple of argv tails (one per fanned `Check`), keyed by placement and never by language.

| [INPUT]    | [PROJECTION FROM `Routed`]                                      | [FAN-OUT] | [CONSUMERS]                       |
| ---------- | -------------------------------------------------------------- | :-------: | --------------------------------- |
| `FILES`    | `(routed.files,)` appended to the command                      |     1     | `ruff`, `ty`, `mypy`, `tsc`, `biome`, `pytest`, `vitest` |
| `INCLUDE`  | `(project, "--include", *files)` per `routed.groups` entry     |    N      | `dotnet format`                   |
| `PROJECT`  | `(project,)` per `routed.projects` entry                       |    N      | `dotnet restore` / `build`        |
| `SOLUTION` | `(settings.solution,)`                                         |     1     | full-scope build, `dotnet sln`    |
| `GLOB`     | suffix globs derived from `routed.files`/`language.suffixes`   |     1     | `ast-grep`, `dotnet-stryker`      |
| `NONE`     | `()`                                                           |     1     | scopeless analyzers               |

```python
def place(routed: Routed, tool: Tool, *, settings: AssaySettings) -> tuple[tuple[str, ...], ...]:
    match tool.input:
        case Input.FILES:    return ((*routed.files,),) if routed.files else ()
        case Input.INCLUDE:  return tuple((project, "--include", *files) for project, files in routed.groups)
        case Input.PROJECT:  return tuple((project,) for project in routed.projects)
        case Input.SOLUTION: return ((str(settings.solution),),)
        case Input.GLOB:     return ((*_globs(routed),),)
        case Input.NONE:     return ((),)
```

---
## [2][SHARED_DISCOVERY]
>**Dictum:** *Change-set union and fd enumeration are language-agnostic; both arms consume them, neither owns them.*

The change-set is the sorted-deduped union of three git probes (folded over a `Source`, identical to `Workspace.changed`): unstaged tracked, staged, and untracked-but-not-ignored. Enumeration of an explicit directory path expands through `fd` with the shared `PROJECT_EXCLUDE_DIRS`; an explicit file passes through verbatim; a non-existent explicit path is a `Fault`.

| [SOURCE STEP]   | [INVOCATION]                                                       | [ROLE]                                  |
| --------------- | ----------------------------------------------------------------- | --------------------------------------- |
| `changed()`     | `git diff --name-only --diff-filter=ACDMRTUXB` ∪ `git diff --cached …` ∪ `git ls-files --others --exclude-standard` | derive the change-set when `paths` is empty |
| `enumerate(p)`  | dir → `fd -H -t f . <dir> --exclude …`; file → verbatim; missing → `Fault` | expand explicit `paths` to root-relative rows |
| project index   | `fd -H -e csproj . --exclude …` → `{dir: csproj}` (CLOSURE arm only) | one-time graph seed                     |

Both arms receive an already-resolved `tuple[str, ...]` of root-relative rows; discovery never knows the requesting language.

---
## [3][CSHARP_CLOSURE_ARM]
>**Dictum:** *Owning project by nearest-ancestor ranking; closure by reverse `ProjectReference` reachability — the dependents that must rebuild.*

The single `CLOSURE` arm folds the change-set into seed projects + INCLUDE groups + escalations, then expands the seeds to their dependents over the parsed `ProjectReference` graph.

1. **Index (cached per run).** `{dir: rel_csproj}` for every discovered `.csproj`; the map is the owner lookup table and the closure node set.
2. **Owner.** Rank `(file.parent, *file.parent.parents)` against the index; a single index hit is the owner, zero hits is `no owning project`, more than one ancestor hosting a `.csproj` is `ambiguous owner`.
3. **Per-file fold.** A relevant `.cs` file contributes its owner to seeds and `(owner, file)` to INCLUDE groups; a `.csproj` contributes its owner; an orphan full-suffix file (`.cs/.csproj/.props/.targets` with no owner) escalates `scope` to `full` rather than faulting.
4. **Dependents closure (fixpoint, frozenset set-algebra).** Parse each project's `ProjectReference Include="*.csproj"` (excluding `OutputItemType="Analyzer"` and `ReferenceOutputAssembly="false"`), resolve includes to root-relative, then grow seeds by every project whose references intersect the current set, iterated to a fixpoint. This is the *affected-dependents* set, not the dependency set: changing `P` rebuilds everything that references `P`.

```python
def _dependents(seeds: frozenset[str], index: ProjectIndex, source: Source) -> frozenset[str]:
    graph = {rel: _refs(rel, index, source) for rel in index.values()}     # _refs cached per run
    return reduce(
        lambda current, _: current | frozenset(p for p, refs in graph.items() if p not in current and current & refs),
        range(len(graph)),
        seeds,
    )
```

`scope="full"` lists the whole target instead: parity (`dotnet sln list` Δ fd-index must be empty, else a parity `Fault`) feeds `projects`, and `place` reads `SOLUTION`. The `GLOB` languages never enter this arm.

---
## [4][GLOB_ARM]
>**Dictum:** *One arm for Python, TypeScript, and Docs; suffix payload is the only variance.*

`_glob(language, files)` filters the shared change-set by `language.suffixes`, returning `Routed(files=…, projects=(), groups=())`. `place` then maps the rows directly (`FILES`) or to glob patterns (`GLOB`); there is no project graph, no owner, no parse. Adding a language is one `Language` member with its suffix payload — no new arm.

---
## [5][FSSPEC_SOURCE]
>**Dictum:** *A new filesystem is a new `Source`, never a new `route()`.*

`route(language, paths, *, source)` injects a `Source` Protocol — the seam every discovery and `.csproj` read already passes through. The `route()` signature, `Language.strategy` dispatch, and `Routed` shape are untouched; only the source binding changes.

```python
class Source(Protocol):
    def changed(self) -> Result[tuple[str, ...], Fault]: ...
    def enumerate(self, paths: RoutePaths) -> Result[tuple[str, ...], Fault]: ...
    def read(self, rel: str) -> Result[bytes, Fault]: ...      # csproj XML for the CLOSURE arm
```

`LOCAL` wraps git + `fd` + `Path`; a future `FsspecSource` wraps `fsspec.AbstractFileSystem` (remote, in-memory, archived worktrees), supplying `changed`/`enumerate`/`read` over a virtual tree. The CLOSURE arm parses `.csproj` via `source.read`, so a non-local worktree resolves the project graph unchanged.

---
## [6][OPTIMIZATION_AND_FAULTS]
>**Dictum:** *Cache the index once per run; do every closure in frozenset algebra; return absence as `Fault`, never a raise.*

| [LEVER]                | [MECHANISM]                                                                 |
| ---------------------- | -------------------------------------------------------------------------- |
| Index reuse            | `{dir: csproj}` and `_refs(project)` memoized for the run (keyed on `root`); the fd scan and XML parse run once. |
| Set-algebra closures   | seeds, dependents, and full-triggers are `frozenset`; union/intersection only; fixpoint via `reduce` over `range(len(graph))`. |
| Stable ordering        | `Routed` tuples are sorted at the boundary for deterministic argv and warm per-closure build keys (`sha256(sorted(projects))`). |

| [FAULT]              | [TRIGGER]                                                  | [STATUS]   |
| ------------------- | ---------------------------------------------------------- | ---------- |
| `no owning project` | relevant C# file whose ancestry hosts no indexed `.csproj` (and not a full-suffix orphan) | `FAILED`   |
| `ambiguous owner`   | more than one ancestor directory hosts a `.csproj`         | `FAILED`   |
| `path missing`      | explicit `paths` entry does not exist                     | `FAILED`   |
| `parity drift`      | full scope: `dotnet sln list` Δ fd-index is non-empty     | `FAILED`   |
| process/parse       | git/`fd` non-zero or unreadable `.csproj`                 | propagated `ProcessFault` |

---
## [7][OPEN_DECISIONS]
>**Dictum:** *Full-trigger policy is data in settings; routing only applies it.*

- **Trigger vocabulary placement.** `FULL_TRIGGER_FILES` (`Directory.Packages.props`, `Directory.Build.props/targets`, `Workspace.slnx`, `.editorconfig`, `global.json`, `.config/dotnet-tools.json`) and `STATIC_FULL_TRIGGER_PREFIXES` (`tools/cs-analyzer/`) belong in `AssaySettings` as data; the CLOSURE arm reads them and escalates `scope` to `full`. Routing owns the *mechanism* (`_route_need: row -> full | owner | none`, pre-planned before any closure resolves), settings own the *vocabulary*.
- **Pre-plan fast path.** When the change-set carries a full-trigger, route emits `scope="full"` with `full_triggers` populated and skips owner/closure resolution entirely; when no row is relevant, it returns an empty `changed` `Routed` without touching the index.
- **Cross-language triggers (pending).** Full-trigger is C#-only today. If Python/TS config (`pyproject.toml`, `ruff`/`biome` config) must force a full lint, `_route_need` becomes a per-`Language` policy table rather than a single C# arm — deferred until a consumer demands it.
