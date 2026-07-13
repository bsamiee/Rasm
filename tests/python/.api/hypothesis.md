# [hypothesis] — the property-based generation engine under `@spec` and the stateful oracles

`hypothesis` is the sole property substrate on the Python dev plane: it draws typed inputs from `SearchStrategy` values, shrinks every counterexample to a minimal witness, and persists failing examples for deterministic replay. The `_testkit` layer owns the repo-facing surface — `runtime.py` registers every profile and composes the example-database stack, `strategies.py` folds the msgspec and pydantic schema algebras into one `resolve(subject)`, and `spec.py`/`laws.py` bind `@given`, `settings`, and the stateful vocabulary into `@spec` — so a law module imports `@spec`, the oracles, and `resolve`, never raw `given` or a hand-built strategy.

## [01]-[PACKAGE_SURFACE]

- package: `hypothesis` · version `6.155.7` · license `MPL-2.0`
- namespace: `hypothesis`; strategies `hypothesis.strategies as st`; stateful `hypothesis.stateful`; database `hypothesis.database`
- asset: pure-Python wheel; profile registration and example storage live under `.cache/hypothesis` via `HYPOTHESIS_STORAGE_DIRECTORY`
- rail: property / generative law — every `@spec(given=True)` law terminates here through `hyp_given(resolve(subject))`

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                        | [KIND]            | [CAPABILITY]                                                                     |
| :-----: | :------------------------------ | :---------------- | :------------------------------------------------------------------------------- |
|  [01]   | `SearchStrategy[Ex]`            | generic class     | typed generator; `.map`/`.filter`/`.flatmap` compose the floor `resolve` returns |
|  [02]   | `settings`                      | policy class      | run-policy knobs governing deadline, example budget, phases, suppressions        |
|  [03]   | `Phase`                         | enum              | the run-phase enum; a profile lists the active subset                            |
|  [04]   | `HealthCheck`                   | enum              | the health-gate enum; a profile suppresses selected gates                        |
|  [05]   | `Verbosity`                     | enum              | reporting level for the run reporter                                             |
|  [06]   | `RuleBasedStateMachine`         | base class        | stateful subject; `rule`/`initialize`/`invariant` drive a randomized trace       |
|  [07]   | `Bundle[Ex]`                    | strategy class    | named symbolic store; `consume`/`draw_references` govern reference lifetime      |
|  [08]   | `MultipleResults[Ex]`           | result carrier    | zero-or-many bundle emission returned by `multiple(...)`                         |
|  [09]   | `DrawFn`                        | callable protocol | the `@composite` draw callback resolving a strategy in a generator body          |
|  [10]   | `ExampleDatabase`               | abstract base     | persisted-example contract; the five concrete rows compose the replay stack      |
|  [11]   | `DirectoryBasedExampleDatabase` | database          | on-disk store under `.cache/hypothesis/examples` — the durable local corpus      |
|  [12]   | `BackgroundWriteDatabase`       | database          | async-write wrapper keeping persistence off the run critical path                |
|  [13]   | `MultiplexedDatabase`           | database          | fan-out over an ordered database set: local write plus read-only replay          |
|  [14]   | `ReadOnlyDatabase`              | database          | write-suppressing wrapper; the CI replay leg never mutates the corpus            |
|  [15]   | `GitHubArtifactDatabase`        | database          | read-only fetch of a CI artifact corpus keyed by `owner`/`repo`                  |
|  [16]   | `InMemoryExampleDatabase`       | database          | ephemeral store for isolated in-process replay                                   |

[ENUM_VOCABULARY]: `settings` knobs — `deadline`/`max_examples`/`phases`/`derandomize`/`stateful_step_count`/`suppress_health_check`/`print_blob`; `Phase` — `explicit`/`reuse`/`generate`/`target`/`shrink`/`explain`; `HealthCheck` — `data_too_large`/`filter_too_much`/`too_slow`/`large_base_example`/`function_scoped_fixture`/`differing_executors`/`nested_given`.

```python signature
class SearchStrategy[Ex]:
    def map[T](self, pack: Callable[[Ex], T]) -> SearchStrategy[T]: ...
    def filter(self, condition: Callable[[Ex], object]) -> SearchStrategy[Ex]: ...
    def flatmap[T](self, expand: Callable[[Ex], SearchStrategy[T]]) -> SearchStrategy[T]: ...
class settings:
    @staticmethod
    def register_profile(name: str, parent: settings | None = None, **kwargs: object) -> None: ...
    @staticmethod
    def get_profile(name: str) -> settings: ...
    @staticmethod
    def load_profile(name: str) -> None: ...
    @staticmethod
    def get_current_profile_name() -> str: ...
class Bundle[Ex](SearchStrategy[Ex]):
    def __init__(self, name: str, *, consume: bool = False, draw_references: bool = True) -> None: ...
```

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                          | [KIND]      | [CAPABILITY]                                                                          |
| :-----: | :--------------------------------- | :---------- | :------------------------------------------------------------------------------------ |
|  [01]   | `given`                            | decorator   | binds strategies to test arguments via `hyp_given(resolve(subject))`                  |
|  [02]   | `settings`                         | decorator   | overlays run policy; `parent=` picks a profile, `deadline=` sets `timeout=`           |
|  [03]   | `example`                          | decorator   | seeds an always-run explicit case consumed in the `Phase.explicit` phase              |
|  [04]   | `assume`                           | guard       | discards the example when the precondition fails, against the filter budget           |
|  [05]   | `note`                             | observation | records a per-example note surfaced only on the minimal failing counterexample        |
|  [06]   | `event`                            | statistic   | tags a drawn value for the statistics fold; `@spec(events=...)` emits one per draw    |
|  [07]   | `target`                           | search bias | steers generation toward extremal observations under the `Phase.target` phase         |
|  [08]   | `reproduce_failure(version, blob)` | decorator   | replays one serialized counterexample blob deterministically                          |
|  [09]   | `is_hypothesis_test`               | predicate   | True for a `@given`-wrapped callable; the plugin auto-marks matches `property`        |
|  [10]   | `run_state_machine_as_test`        | driver      | executes a `RuleBasedStateMachine`; `model_based` calls it under the resolved profile |

```python signature
def given(*args: SearchStrategy[object] | ellipsis, **kwargs: SearchStrategy[object] | ellipsis) -> Callable[[Callable[..., None]], Callable[..., None]]: ...
def assume(condition: object) -> Literal[True]: ...
def event(value: str, payload: Any = "") -> None: ...
def target(observation: int | float, *, label: str = "") -> int | float: ...
def is_hypothesis_test(f: object) -> bool: ...
def run_state_machine_as_test(state_machine_factory: type[RuleBasedStateMachine], *, settings: settings | None = None) -> None: ...
```

[STRATEGY_ALGEBRA]: scalars — `integers`, `floats`, `decimals`, `booleans`, `none`, `text`, `binary`, `uuids`, `datetimes`, `dates`, `times`, `timedeltas`, `timezones`, `just`, `nothing` (each takes constraint keywords: `min_value`/`max_value`, `min_size`/`max_size`, `allow_nan`/`allow_infinity`, `alphabet`).

[STRATEGY_ALGEBRA]: containers + shape — `lists`, `sets`, `frozensets`, `tuples`, `dictionaries`, `fixed_dictionaries` (required plus `optional=` presence sampling), `from_regex` (`fullmatch=True`), `sampled_from` over a closed set.

[STRATEGY_ALGEBRA]: combinators + recursion — `one_of` (union over strategies), `builds` (construct from field strategies), `composite` (imperative `DrawFn` body), `deferred` (lazy self-reference), `recursive(base, extend, *, min_leaves, max_leaves)`, `data` (draw inside the test body).

[STRATEGY_ALGEBRA]: type-directed — `from_type(type)` resolves a strategy from a type form; `register_type_strategy(type, strategy)` binds a custom generator. `resolve(subject)` (`strategies.py`) IS the repo's registration surface over these two — msgspec structs, pydantic models, PEP 695 aliases, unions, and `Literal` all fold through it.

[STATEFUL_VOCABULARY]: `rule(*, targets=(), target=None, **strategies)` and `initialize(...)` register command methods; `precondition(predicate)` guards a rule; `invariant(...)` asserts between steps; `Bundle(name)` names a symbolic store; `consumes(bundle)` removes a drawn reference; `multiple(*values)` emits zero-or-many bundle entries.

## [04]-[IMPLEMENTATION_LAW]

[HYPOTHESIS_TOPOLOGY]:
- One engine folds three surfaces: `SearchStrategy` construction, `@given` example generation, and automatic shrinking to a minimal counterexample — a law never loops or shrinks by hand.
- Every profile is registered once in `runtime.py` and selected by name — `rasm` (the pytest default via `--hypothesis-profile=rasm`), `rasm-ci`, `rasm-stress`, `rasm-debug`, `rasm-mutation` (`database=None`, `derandomize=True`, short traces), `rasm-stateful`, `rasm-parity`, `rasm-adversarial`; an unpinned `@spec` law follows the session-active profile so the CLI selection governs it.
- The example-database stack composes `BackgroundWriteDatabase(DirectoryBasedExampleDatabase(...))` for local writes, multiplexed with `ReadOnlyDatabase(GitHubArtifactDatabase(owner, repo))` only when `RASM_HYPOTHESIS_GH_REPLAY` names a CI corpus.
- `HealthCheck.filter_too_much`, `too_slow`, and `data_too_large` are the standing suppressions; recursion depth binds through engine rejection in `resolve`'s `deferred` strategies, never a manual counter.
- Observability routes through `hypothesis.internal.observability.add_observability_callback` to `.artifacts/python/hypothesis` when `TESTS_OBSERVABILITY` is set, preserving the built-in callback.

[STACKING]:
- `strategies.py`(`../_testkit/strategies.py`): `resolve(subject)` is the ONE strategy source over the msgspec node taxonomy and the pydantic-core schema algebra; the [STRATEGY_ALGEBRA] rows are the primitives it composes — a law module never hand-authors a strategy.
- `laws.py`(`../_testkit/laws.py`): `@spec` owns profile selection, `hyp_given(resolve(subject))` injection, marker application, and coverage attribution; a law decorates with `@spec`, never bare `@given`/`@settings`.
- `spec.py`(`../_testkit/spec.py`): the stateful and algebraic oracles re-export `rule`/`initialize`/`precondition`/`invariant`/`Bundle`/`consumes`/`multiple`/`target` and drive `run_state_machine_as_test` through `model_based`.
- `runtime.py`(`../_testkit/runtime.py`): owns `register_profile`, `set_hypothesis_home_dir`, the database composition, and the observability callback — a consumer selects a profile by name, never re-registers one.

[LOCAL_ADMISSION]:
- Admitted on the `tests/` dev plane only; no `plane:runtime` module imports `hypothesis`.
- The strategy registry, profile registry, and stateful driver are internalized behind `_testkit`; downstream law modules compose `@spec` and the oracles, never the engine directly.

[RAIL_LAW]:
- Package: `hypothesis`
- Owns: typed input generation, minimal-counterexample shrinking, seeded replay via the example database, profile-governed run policy, and stateful command-trace exploration.
- Accept: `resolve(subject)`-derived strategies through `@spec`; a registered profile named by `profile=`; `target`/`event` for search bias and statistics; `RuleBasedStateMachine` subjects through `model_based`.
- Reject: a hand-rolled strategy where `resolve` folds the schema; bare `@given`/`@settings` on a law (use `@spec`); a manual shrink or retry loop; re-registering a profile a consumer selects by name; any import from a `plane:runtime` folder.
