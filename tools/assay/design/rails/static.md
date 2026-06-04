# [H1][STATIC_RAIL]
>**Dictum:** *Five static verbs are one parameterized fold over a `Mode`-filtered, language-routed slice of the catalog — `fix`/`report`/`build`/`full`/`plan` differ by `(verb, mode)` + `Params`, never by function.*

## [1][PURPOSE]

`rails/static.py` is the thinnest of the three polyglot folds. It owns `Claim.STATIC` across every orchestrated language and carries **no** `Detail` variant: every outcome collapses into the canonical `Report.status`/`counts`/`artifacts`/`results`. The rail never builds argv, never spawns, never leases — it `select`s `Tool` rows, `route`s inputs per language, hands `Check`s to the `Engine`, and `fold`s the `Completed` outcomes. The five per-verb adapters **are** the `Handler`s; four (`fix`/`report`/`build`/`full`) delegate to one `thin_rail` — `fix` mutates, `report`/`build`/`full` diagnose at increasing scope — and `plan` is the zero-run adapter that short-circuits before the Engine (routing projection → `notes`/`artifacts`). `DOCS` rides the polyglot fan even though its `Claim` is `DOCS`, not `STATIC`: `select(Claim.STATIC, …)` simply yields no `DOCS` row, so the empty slice folds to an honest `EMPTY` without a per-language guard.

## [2][CANONICAL_SHAPES]

`StaticParams` subclasses the shared `BaseParams` (frozen `kw_only` `@dataclass`, `TID251` bans `NamedTuple`; Cyclopts-flattened via `Parameter(name="*")`), inheriting `paths` and `language` and adding **nothing**: the static rail carries no `strict` field — `strict` is added solely by `api`/`docs` Params. `language=None` selects every language slice (polyglot fan-out). `BaseParams` is the no-cycle leaf owned by `core/model.py`; `registry` imports `StaticParams` from this module and never re-declares it.

```python
@dataclass(frozen=True, slots=True, kw_only=True)
class BaseParams:                       # owned by core/model.py (the no-cycle leaf), not the rail
    paths: tuple[str, ...] = ()
    language: Language | None = None

@dataclass(frozen=True, slots=True, kw_only=True)
class StaticParams(BaseParams):         # inherits paths/language; no strict (static has none)
    pass
```

Per-verb behavior is a `(verb, mode)` pair bound at the registry seam, never branched inside the body; the FULL scope is resolved *inside* `routing` (trigger-file escalation), never threaded as a parameter:

| [VERB] | [MODE] | [ROW FILTER] | [ROUTE SCOPE] | [PARITY] |
| ------ | ------ | ------------ | ------------- | -------- |
| `fix` | `Mode.WRITE` | `t.mode is WRITE` | `CHANGED` | `dotnet format` + `ruff format`/`ruff check --fix` + `shfmt -w` + `sqlfluff fix` write twins |
| `report` | `Mode.CHECK` | `t.mode is mode` | `CHANGED` | non-mutating ladder (`--verify-no-changes`, `ruff check`, `ty`/`mypy`, `shellcheck`, `sqlfluff lint`, `biome ci`) |
| `build` | `Mode.BUILD` | `t.mode is BUILD` | `CHANGED` closure | closure-leased restore + build + analyzers (`build-<closure>.lock`) |
| `full` | `Mode.BUILD` | `t.mode is BUILD` | `FULL` (trigger-escalated) | `.slnx` parity; reads `SOLUTION` |
| `plan` | (none — short-circuit) | (none) | `CHANGED` | zero checks; owners/triggers/closure-sha into `notes`/`artifacts` |

`Mode.CHECK` is the catalog default, so `report` filters on `t.mode is mode` against the unannotated rows. `full` differs from `build` by `verb="full"` only; both pass `mode=Mode.BUILD`. `fold` (`core/model.py`) seeds `EMPTY`, joins by max-severity, derives `Counts(ok, failed, total)`, and projects parser `Match` rows — the parser is a `Tool` field consumed in the Engine, **not** a `fold` keyword. A non-zero process exit already rode the success channel as `Completed{status=FAILED}`, so the fold never sees a `Fault`.

## [3][VALIDATED_SNIPPET]

The canonical core is the polyglot fan: `_routed` routes every requested language through its own `route`, `_dispatch` fans each routed slice through the Engine under its OWN `Routed`, and one `fold` consumes the concatenated successes. `route` resolves one `Language` per call and takes **no** `scope` argument — the FULL escalation is a `CLOSURE`-arm concern internal to `routing`.

```python
def _languages(selected: Language | None) -> tuple[Language, ...]:
    match selected:
        case None:
            return tuple(Language)        # the polyglot fan
        case language:
            return (language,)

def _checks(routed: Routed, mode: Mode) -> tuple[Check, ...]:
    return tuple(Check(tool=t, paths=routed.files) for t in select(Claim.STATIC, routed.language) if t.mode is mode)

def _routed(languages: tuple[Language, ...], paths: tuple[str, ...]) -> Result[Block[Routed], Fault]:
    return sequence(block.of_seq(route(language, paths) for language in languages))  # route(lang, paths) — no scope kwarg

def thin_rail(settings, scope, params, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    return _routed(_languages(params.language), params.paths).bind(            # plain fn folds Result; NOT @effect.result
        lambda routed: sequence(
            routed.collect(lambda r: block.of_seq(_dispatch(r, settings=settings, scope=scope, mode=mode)))  # Block.collect = native concatMap, not a hand-rolled flatten
        ).map(lambda done: fold(claim, verb, tuple(done)))                     # fold(claim, verb, outcomes) — no parser kwarg
    )

def _dispatch(routed, *, settings, scope, mode) -> tuple[Result[Completed, Fault], ...]:
    match _checks(routed, mode):
        case ():
            return ()                                                         # empty fan, no phantom EMPTY slot
        case checks:
            return fan_out(checks, settings=settings, scope=scope, routed=routed)
```

The four executing adapters are each one line closing over `(claim, verb, mode)`; `full` passes `mode=Mode.BUILD` like `build` and is distinguished only by `verb="full"`. `plan` is the lone zero-run adapter — it reuses `_routed` (the same polyglot fan), then `msgspec.structs.replace`s the routing projection onto an `EMPTY` fold without reaching `fan_out`:

```python
def full(settings, scope, params: StaticParams) -> Result[Report, Fault]:
    return thin_rail(settings, scope, params, claim=Claim.STATIC, verb="full", mode=Mode.BUILD)  # verb alone → routing escalates to FULL

def plan(settings, scope, params: StaticParams) -> Result[Report, Fault]:
    _ = scope                                                                 # zero checks: no artifact scope is spliced
    return _routed(_languages(params.language), params.paths).map(
        lambda routed: msgspec.structs.replace(
            fold(Claim.STATIC, "plan", ()),                                   # empty outcomes seed EMPTY + zero counts
            artifacts=_plan_artifacts(tuple(routed), settings),
            notes=_plan_notes(tuple(routed)),                                 # owners, full_triggers, closure sha
        )
    )
```

The static rail performs **no** `--strict` promotion (`strict` is a `BaseParams` field only on `api`/`docs`, absent here): an empty selected slice folds to a real `EMPTY`/`SKIP` `Report` and rides the success channel unmodified. A `Fault` reaches the registry seam only from a spawn/timeout/lease failure, and carries `{argv, status, message}` — **no** `returncode`/`detail`.

## [4][SEAMS]

| [SEAM] | [DIRECTION] | [CONTRACT] |
| ------ | ----------- | ---------- |
| `composition.catalog.select(claim, language=None)` | in | Sorted `tuple[Tool, ...]` slice (39-row catalog total); rail filters by `t.mode`. |
| `core.routing.route(language, paths=(), *, source=None)` | in | `Result[Routed, Fault]`; resolves ONE `Language`, FULL escalation internal (no `scope` arg); `Routed{language, scope, files, projects, groups, full_triggers}`. |
| `core.routing.place(routed, tool, *, settings)` | (via Engine) | Sole argv-tail projector — `static.py` never calls it directly. |
| `core.engine.fan_out(checks, *, settings, scope, routed)` | in | Bounded `CapacityLimiter` fan-out → `tuple[Result[Completed, Fault], ...]`; closure lease for `BUILD` rows; one `fan_out` per language. |
| `core.model.fold(claim, verb, outcomes, *, detail=None)` | in | Monoid fold → one `Report`; parser is a `Tool` field, not a `fold` kwarg; `detail=None` for this rail. |
| `composition.registry` | out | The five per-verb adapters **are** the `Handler`s `REGISTRY` binds (`thin_rail` itself is not a `Handler` — its keyword-only discriminants violate the arity); `_narrow` validates each `FunctionType` bind, then the runner weaves `checked ▷ logged ▷ traced` (no `retried`). Retry-correlation context is bound in `traced` (it runs outside the engine `retried` loop) and read back by the `stamina` hook — `logged` is rail-only and never reaches the engine seam. |

## [5][EXTENSIBILITY]

A sixth language (or a new linter) is one `Language` member + one `routing` arm + N `Tool` rows in the catalog — `thin_rail` is untouched because `select(claim, language)` and `t.mode is mode` already discriminate; a new static *verb* is one `REGISTRY` `Bind` plus its `(verb, mode)` pair, never a new function.

## [6][CONSIDERATIONS]

- `plan`'s closure hash must be computed from `Routed.projects` with the **same** `sha256(sorted-projects)[:16]` recipe `ArtifactScope.build` uses, or the planned-vs-actual `build-<closure>.lock` path diverges and the plan misrepresents which warm tree `build` will reuse. `_plan_notes` surfaces the closure sha verbatim so an operator can correlate it with a subsequent `build` lease; a glob-only language folds to a bare file-count note, never a phantom closure.
- The static rail has **no** `strict` promotion (`StaticParams` inherits only `paths`/`language`; `strict` is added solely by `api`/`docs`). A polyglot `report` with zero `PYTHON` rows but live `CSHARP` failures folds to `FAILED` (severity dominates); a slice that no-ops entirely (e.g. `--language sql` on a tree with no `.sql` changes) folds to an honest `EMPTY`/`SKIP` that rides the success channel — there is no flag here to harden that into a `FAULTED`. An agent that needs "static changed and must pass" asserts it by routing real paths.
- `fan_out` runs `WRITE`-mode `fix` rows concurrently under one `CapacityLimiter`; the formatter twins (`dotnet format --include`, `ruff format`, `shfmt -w`, `sqlfluff fix`) touch disjoint suffix sets so this is safe, but two `WRITE` rows targeting overlapping paths in one language (a future formatter + fixer pair) would race — gate such a pair behind a per-language ordering token in `select` rather than serializing the whole rail.
