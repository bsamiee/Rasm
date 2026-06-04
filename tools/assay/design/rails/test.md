# [H1][TEST_RAIL]
>**Dictum:** *`test` is one more thin fold over the same Engine; `run`/`list`/`coverage` differ by `Mode` + `Params`, and mutation/benchmark/coverage/fixtures/target are `TestParams` fields, never verbs.*

## [1][PURPOSE]

`rails/test.py` owns `Claim.TEST` across the three orchestrated runner families (C# `dotnet test` MTP + Coverlet + `dotnet-stryker`; Python `pytest` + `coverage` + `mutmut` + `pytest-benchmark`; TypeScript `vitest` only) and carries the single `TestRun` `Detail` (tag `"test"`). It owns no executor logic: every verb is one adapter over the shared `thin_rail` that `select(Claim.TEST, language)`s catalog rows, lets the Engine `fan_out` the eligible `Check`s, and folds the `Completed` outcomes into one `Report`. The rail never re-implements capture, count derivation, or leases — it parameterizes them: counts derive **only** in `core/model.fold`, `killed`/`survived`/`coverage` ride the `TestRun` detail via the catalog `parse_tests` Parser, and `mutation.lock` is acquired inside the Engine, not the rail.

## [2][CANONICAL_SHAPES]

This doc owns `TestParams` (frozen `@dataclass`, Cyclopts `Parameter(name="*")`-flattened) defined here and imported by `registry`. It inherits `BaseParams{paths, language}` and adds the eight test-only fields; `mutation`/`benchmark`/`coverage`/`fixtures`/`target`/`all`/`no_build`/`filter` are `TestParams` fields, not verbs. The `TestRun` detail shape is owned by `core/model.py` and aligned **verbatim** to `model.md` (`mutation: str`, `coverage: float | None`, `killed`/`survived`/`selected: int`) — reproduced here for the seam only, never re-declared.

```python
@dataclass(frozen=True, slots=True, kw_only=True)
class BaseParams:                                        # owned in rails/<claim>.py, imported by registry
    paths: tuple[str, ...] = ()                          # explicit targets; empty → change-set route
    language: Language | None = None                     # None selects every TEST language slice

@dataclass(frozen=True, slots=True, kw_only=True)
class TestParams(BaseParams):                            # frozen, Cyclopts-flattened; thin-Params
    target: Path | None = None                           # override default test project/closure
    all: bool = False                                    # whole .slnx / workspace projects
    no_build: bool = False                               # reuse warm binaries
    mutation: bool = False                               # Stryker (C#) / mutmut (Py); TS unsupported
    benchmark: bool = False                              # BenchmarkDotNet (C#) / pytest-benchmark (Py)
    coverage: bool = False                               # Coverlet (C#) / coverage.py (Py)
    fixtures: bool = False                               # pytest --dead-fixtures/--dup-fixtures (list mode)
    filter: str = ""                                     # MTP filter expr / pytest -k / vitest -t
```

`TestRun` (in `core/model.py`, reproduced for the seam): `mutation: str = "off"`, `coverage: Annotated[float, Meta(ge=0, le=100)] | None = None`, `killed: int = 0`, `survived: int = 0`, `selected: int = 0` — `selected` is the **count** of selected tests, not a tuple. `Detail` inherits `Base(frozen=True, gc=False, omit_defaults=True)` and adds `forbid_unknown_fields=True, tag_field="kind"`, so a drifting emitter fails loudly at decode. `killed`/`survived`/`coverage` are populated onto the detail by the catalog `parse_tests` Parser (`Parser = Callable[[Completed], AnyDetail | None]`); a non-mutation run leaves `mutation="off"` and the counts at their defaults.

## [3][VALIDATED_SNIPPET]

`thin_rail(settings, scope, params, *, claim, verb, mode)` is **not** a `Handler`; the per-verb adapters (`run`/`list`/`coverage`) close over `(verb, mode)` and **are** the `Handler`s `REGISTRY` binds (`Handler[**P] = Callable[[AssaySettings, ArtifactScope, P], Result[Report, Fault]]`, registry.md). The core pattern: `_route_language` narrows `language` to a concrete `Language` (`route` is singular — `Routed` has one `language`, never a plural field — so `None` collapses to `Language.PYTHON`), the eligibility guard drops mutation rows unless the default target stands and the runner supports them, the mutation gap is logged when a requested lane is structurally inapplicable, `fan_out` returns `tuple[Result[Completed, Fault], ...]`, and `_completeds` short-circuits to the first error slot — matching the verified tagged-union shape `Result(tag="error", error=…)`, never an `Error(x)` class pattern (`Ok`/`Error` are `expression` factory functions). The success `Completed`s pass to `core/model.fold` where counts derive; the `TestRun` detail comes from `_detail`, which takes the **first** `parse_tests` decode carrying a real mutation lane (`mutation != "off"`) so a barren receipt never masks the Stryker/`mutmut` row. `match` is statement form only; `thin_rail` returns a plain `Result` and so is never `@effect.result`-wrapped.

```python
from tools.assay.composition.catalog import parse_tests, select   # intra-package
from tools.assay.core.engine import fan_out
from tools.assay.core.model import Check, Claim, Fault, Language, Mode, Report, TestRun, Tool, fold
from tools.assay.core.routing import route                         # route(language: Language, paths) — singular

_NO_MUTATION = frozenset({"vitest"})                    # TS gap, logged not dropped

def _eligible(tool: Tool, params: TestParams) -> bool:
    match (tool.mode, params.mutation):                 # mutation rows survive only on the default target
        case (Mode.MUTATION, False):
            return False
        case (Mode.MUTATION, True):
            return params.target is None and not params.all and tool.name not in _NO_MUTATION
        case (Mode.RESTORE, _) | (Mode.BUILD, _):
            return not params.no_build
        case _:
            return True

def _completeds(slots: tuple[Result[Completed, Fault], ...]) -> Result[tuple[Completed, ...], Fault]:
    match next((s for s in slots if s.is_error()), None):     # a Fault slot dominates the error channel
        case Result(tag="error", error=fault):                # NOT Error(x): Ok/Error are factory fns
            return Error(fault)
        case _:
            return Ok(tuple(s.ok for s in slots))             # success Completeds → fold; empty → Ok(()) EMPTY

def _detail(done: tuple[Completed, ...], params: TestParams) -> AnyDetail | None:
    match params.mutation:
        case False:
            return None
        case True:                                            # first decode carrying a real lane, not a barren one
            return next((d for c in done if (d := parse_tests(c)) is not None and _is_mutation(d)), None)

def _route_language(language: Language | None) -> Language:   # route is singular; polyglot None → one file set
    match language:
        case None:
            return Language.PYTHON
        case _:
            return language

def thin_rail(settings: AssaySettings, scope: ArtifactScope, params: TestParams,
              *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:    # NOT a Handler
    rows = _rows(claim, params, mode)
    match _mutation_gap(params, rows):
        case True:
            _LOG.warning(_GAP_NOTE, claim=claim.value, verb=verb, language=params.language)
        case False:
            pass
    return route(_route_language(params.language), params.paths).bind(
        lambda routed: _completeds(
            fan_out(_checks(rows, routed), settings=settings, scope=scope, routed=routed),
        ).map(lambda done: fold(claim, verb, done, detail=_detail(done, params))),
    )

def run(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:   # Handler
    return thin_rail(settings, scope, params, claim=Claim.TEST, verb="run", mode=Mode.RUN)

def list(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:  # Handler
    return thin_rail(settings, scope, params, claim=Claim.TEST, verb="list", mode=Mode.LIST)

def coverage(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:  # Handler
    return thin_rail(settings, scope, params, claim=Claim.TEST, verb="coverage", mode=Mode.RUN)
```

`run`/`list`/`coverage` are the adapters `REGISTRY` binds: each is `(AssaySettings, ArtifactScope, TestParams) -> Result[Report, Fault]`, matching `Handler` exactly. `coverage` is `Mode.RUN` plus `params.coverage=True` flowing through the Coverlet/`coverage.py` row tail; `list` is `Mode.LIST` and surfaces the `--dead-fixtures`/`--dup-fixtures` audit when `params.fixtures` is set. The `parse_tests` Parser decodes Stryker's JSON reporter / mutmut's result table straight onto a `TestRun` detail; counts (`Counts(ok, failed, total)`) derive **only** inside `fold`, never on the detail. `_rows`/`_checks`/`_mutation_gap`/`_is_mutation` are the pure projections `thin_rail` composes (elided above for the core pattern).

## [4][SEAMS]

| [NEIGHBOR] | [IMPORT] | [CONTRACT] |
| ---------- | -------- | ---------- |
| `composition/catalog.py` | `select(Claim.TEST, language)`, `parse_tests` | `select` returns the by-language `Tool` rows from the 39-row catalog (`pytest`/`dotnet test`/`vitest`/`coverage`/`mutmut`/`dotnet-stryker`/`pytest-benchmark`); `parse_tests` is the `TestRun` Parser attached by reference, decoding every receipt to a defaulted `TestRun(mutation="off")`. |
| `core/engine.py` | `fan_out` | `fan_out(checks, *, settings, scope, routed) -> tuple[Result[Completed, Fault], ...]`; acquires `locks/mutation.lock` (global exclusive) for `Mode.MUTATION` rows; BUSY/TIMEOUT → `Fault{argv, status, message}` on the error channel. |
| `core/routing.py` | `route(language, paths)` | Called **once** per verb via `_route_language`; `route` takes a **concrete** `Language` (not `None`); returns `Result[Routed, Fault]`; `Routed.language` is singular. `place` is invoked by the Engine, never here. |
| `core/model.py` | `BaseParams`, `Check`, `Claim`, `Fault`, `fold`, `Language`, `Mode`, `Report`, `TestRun` (+ `AnyDetail`/`Completed`/`Tool` under `TYPE_CHECKING`) | `fold(claim, verb, outcomes, *, detail)` owns count math; `TestRun(Detail, tag="test")` shape per model.md. `Fault`/`Report` import **unconditionally** so beartype `@checked` resolves the `-> Result[Report, Fault]` forward-ref under PEP 649. |
| `composition/registry.py` | `Bind(Claim.TEST, verb, run\|list\|coverage, TestParams, …)` × 3 | The three adapters are the bound `Handler`s; the runner weaves `checked ▷ logged ▷ traced` over the `_narrow`ed handler and `_emit` folds the `Result` with `match` into one `Envelope`. Retry correlation is bound in `traced` (the engine seam, where `logged` is forbidden), not in `logged`, so each `stamina` retry logs under the same `run_id` as its spawn. |

## [5][EXTENSIBILITY]

A fourth test runner (e.g. a Rust `cargo test`) is one catalog `Tool` row + one `Language` member + one routing arm — `rails/test.py` is untouched; a new evidence field is a `TestParams`/`TestRun` field plus its Parser, never a new verb or struct.

## [6][CONSIDERATIONS]

- **TS mutation gap is structural, not a TODO.** `vitest` is excluded by `_NO_MUTATION` and never reaches `fan_out`; a `--mutation` request on a TS-only `Routed` (or one whose `target`/`all` displaced the default project) folds to `EMPTY` (exit 0), not `FAULTED`, because the precondition is valid but inapplicable. `_mutation_gap` detects it, `_LOG.warning(_GAP_NOTE, …)` records the parity note, and `TestRun.mutation` stays `"off"`.
- **Mutation eligibility must gate before the `Check` is built, never after `fan_out`.** Routing a `Mode.MUTATION` row whose target was overridden would still acquire `mutation.lock` and strand the global lane on a guaranteed-reject; `_eligible` filtering inside `_rows` keeps the exclusive lease untouched, so concurrent agents never `busy`-storm (exit 5) on a run that was never eligible.
- **`killed`/`survived` are Parser-derived, never fold-derived.** `core/model.fold` counts `Completed` outcomes (checks), not mutants; conflating mutant kill-ratio into `Counts` would re-introduce the per-rail count struct the design retired. Because `parse_tests` decodes every receipt to a defaulted `TestRun`, `_detail` takes the **first** decode whose `mutation != "off"` (via `_is_mutation`), so a barren `pytest`/`dotnet test` receipt never masks the Stryker/`mutmut` row that actually ran.
