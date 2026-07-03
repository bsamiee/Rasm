# [PYTHON_TESTING]

Authoring law for every Python spec, kit member, and tool suite under `tests/python`. The cross-language proof law — oracle grades, the witness mandate, the banned-shape core — is [tests/README.md](../README.md). Anything in this tree — kit modules, suite folders, fixtures, tooling policy — is rebuilt ground-up the moment a denser shape exists; no compatibility shims, no aliased old surfaces, no band-aids, and breaking existing specs is never a reason to preserve chaff.

## [01]-[KIT_SURFACE]

`tests/python/_testkit` is the one project-agnostic kit; extend the owning module, never add a helper file:

| [INDEX] | [MODULE]        | [OWNS]                                                                                                    |
| :-----: | :-------------- | :----------------------------------------------------------------------------------------------------------- |
|  [01]   | `spec.py`       | pure assertion oracles: algebraic laws, `refutes`, the matrix folds, `Result`/`Option` rail asserts, `assert_roundtrip`, `model_based` plus the stateful/guided-search vocabulary (`rule`, `initialize`, `precondition`, `invariant`, `Bundle`, `consumes`, `multiple`, `target`) |
|  [02]   | `strategies.py` | `resolve(subject)` — the one Hypothesis strategy resolver over msgspec and pydantic-core schema algebras; defaulted struct fields sample presence and absence so the `UNSET`/omitted wire lane generates |
|  [03]   | `seams.py`      | the `Shape` call-shape union (`Sync`/`Async`/`FanOut`/`Factory`), `SeamProbe`, `Loopback`, `loopback_server`/`grpc_loopback`, `VariantWriter`, `TmpRoot`/`tmp_root`, `NdjsonOracle`, `autospec_proc`, module-attr doubles |
|  [04]   | `env.py`        | declarative environment doubles: `SshHost`, `RemoteFS`, `ObjectStore`, `Provisioned`, one polymorphic `provision` dispatch |
|  [05]   | `bench.py`      | `BenchCase` registry rows, `bench_params`/`run_bench`/`run_registry`, absolute-budget gates, sustained-regression detection |
|  [06]   | `laws.py`       | `@spec` law registration, declarative `COVERS` consumption, `auto_exempt`, `LawRecord`/`MANIFEST`, SUT registration, `assert_law_coverage` — the law-coverage census gate |
|  [07]   | `runtime.py`    | the pytest plugin: Hypothesis profiles + example database, marker auto-application, observability and profiling artifact routing |

`test_spec.py`, `test_strategies.py`, `test_seams.py`, `test_env.py`, and `test_policy.py` are the kit's own falsification suites: every oracle proven able to pass and to fail — the guided-search vocabulary included — the strategy resolver's constraint/omission/sign/type-form algebra (exclusive edges, decimal multiples, and digit budgets included), the seam substrate (call-shape recording, fixture writers, decode oracles, process doubles), environment doubles plus loopback capsules, and the SUT-agnostic policy meta-laws over registered packages, declared markers, benchmark hooks — including the sustained-regression fold — observability routing, the snapshot rail's report-versus-fix split, Hypothesis profile lane invariants, and litter containment. A kit capability without a falsification law here is unproven and gets deleted or proven, never trusted. Per-package suites live in `tests/python/libs/<package>/` mirroring `libs/python`, and each package `conftest.py` composes the shared kit instead of redeclaring it; tool suites live in `tests/python/tools/<tool>/`.

## [02]-[LAW_REGISTRATION]

`@spec(subject)` is the one registration surface: it emits a `LawRecord` into the manifest, applies the Hypothesis profile stack, and with `given=True` injects `resolve(subject)` as the generated argument — the subject algebra matches the resolver's (classes, PEP 695 aliases, and parameterized type forms all inject; bare callables refuse) — one decorator owns strategy injection, profile selection, marker application, deadline override, and coverage attribution:

```python conceptual
@spec(Shape, mutation=True, events=(lambda drawn: f"kind={drawn.kind}",))
def test_shape_roundtrip(shape: Shape) -> None:
    assert_roundtrip(shape, Shape)
```

Coverage credit derives from two sources only: `@spec(subject)` decorations and one declarative `COVERS: tuple[object, ...]` per test module, consumed at collection — entries are types or callables whose laws live in that module without a `@spec` anchor. Double-decoration is a registration failure, not a silent stack. `assert_law_coverage` folds the manifest against each registered SUT package's public surface and fails on uncovered symbols and on import failures alike — an unimportable module is uncovered surface, never a silent skip. The census is subset-aware: `register_sut` derives each package's suite root, `uncollected_laws` names every on-disk law module the session never imported, and the coverage gate censuses only fully-collected packages while skipping the partial ones by name — a targeted run never reports false gaps and never silently passes them. `auto_exempt` removes StrEnums, method-free frozen structs, and value-only symbols (constants, tables, codecs, aliases, ContextVars) from the census by predicate; anything else exempts per symbol with a recorded justification through `register_sut(..., exempt=...)`, never a blanket package exemption.

An unpinned `@spec` law follows the session-active Hypothesis profile, so the mutation and CI lanes govern every law through their CLI profile selection; naming `profile=` pins that registered profile deliberately, and `timeout=` is a per-law deadline in seconds.

Hypothesis profiles are registered once in `runtime.py` and selected by name: `rasm` (default), `rasm-ci`, `rasm-stress`, `rasm-debug`, `rasm-adversarial` (deep example budget for hostile-input degradation laws), `rasm-mutation` (derandomized, database-free, short traces to preserve kill-signal budget), `rasm-stateful` (long traces for interleaving counterexamples), and `rasm-parity` (derandomized and database-free for byte-stable cross-tool comparison). The example database lives under `.cache/hypothesis` with an optional read-only CI replay multiplex; observability output routes to `.artifacts/python/hypothesis` when `TESTS_OBSERVABILITY` is set.

## [03]-[ORACLES]

Algebraic, matrix, rail, and stateful proofs ride the kit oracles:
- Algebraic families prove through the `spec` oracles — `roundtrip`, `identity`, `idempotent`, `involution`, `inverse`, `commutative`, `associative`, `distributive`, `absorbing`, `identity_element`, `monotone`, `permutation_invariant`, `metamorphic`, `metamorphic_sweep` — each parameterized by an explicit equality policy.
- Every law family carries a refuting witness through `refutes`: a known-broken input the law must fail on, proving the law can fail at all.
- Case families fold as matrices, never fact-per-case spam: `validity_matrix` over `ValidityCase` rows, `projection_matrix` over `ProjectionCase` rows, `support_matrix` over probe rows. A new case is a row.
- Rail outcomes prove through `assert_ok`, `assert_error`, `assert_error_status`, `assert_some`, and `assert_none` — never truthiness checks or `isinstance` dances on the carrier.
- Stateful subjects prove through `model_based` over a `RuleBasedStateMachine` under the `rasm-stateful` profile, composing `rule`/`initialize`/`precondition`/`Bundle`/`consumes` from the kit surface; `target` guides search toward extremal observations under the `rasm-stress` profile's target phase. Process-boundary output decodes through `NdjsonOracle`, never string-contains scraping.

## [04]-[SEAMS]

Seam substitution dispatches on the `Shape` variant — `Sync`, `Async`, `FanOut`, and `Factory` are the call shapes of one substitution owner, installed through `SeamProbe` with recorded calls projected via `projected`. Loopback servers ride `loopback_server`/`Loopback` under the `network` marker, and `grpc.aio` services — the server-runtime and tessellation-daemon lanes — ride `grpc_loopback`, which binds an ephemeral port and yields the endpoint with a connected channel; filesystem fixtures ride `VariantWriter` and `tmp_root`; remote environments ride the `provision` dispatch over `SshHost`/`RemoteFS`/`ObjectStore`, where `ObjectStore` serves a real S3-compatible loopback endpoint projected as an `s3fs` view carrying the s3-native egress surfaces — e-tags and presigned GET — that the in-process memory double honestly refuses. Where a public driver exists, drive it: genuinely white-box seams (stall verdicts, crash recovery) earn direct probes, everything else goes through the public surface.

## [05]-[LANES_MARKERS]

The marker taxonomy is closed and declared in `pyproject.toml`; the runtime plugin auto-applies `network` and `property` from fixture and Hypothesis membership:

| [INDEX] | [MARKER]     | [MEANING]                                                                              |
| :-----: | :----------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `property`   | Hypothesis-driven law                                                                  |
|  [02]   | `network`    | real INET sockets lifted: loopback servers or egress; excluded from mutation lanes     |
|  [03]   | `subprocess` | spawns the real CLI in a child interpreter; excluded from mutation lanes               |
|  [04]   | `benchmark`  | measurement session, excluded from the default run                                     |
|  [05]   | `mutation`   | mutation-acceptance and survivor-triage laws                                           |

The default run is the unit lane: sockets disabled through pytest-socket, benchmarks deselected, the `rasm` profile active. The `network` and `subprocess` markers are the Python spelling of the integration lane. The mutation lane is a staged gate under assay: mutmut policy lives in `pyproject.toml` `[tool.mutmut]` with the absolute-path coverage side-file `.config/coverage-mutmut.ini`, and `subprocess`-marked tests stay out because children execute the unmutated tree. The config's per-mutant timeout bounds cap any bare `mutmut run`; concurrency is CLI-owned (`--max-children`, assay-governed).

## [06]-[SNAPSHOTS]

inline-snapshot owns genuine wire goldens only — payloads an independent producer emits — with storage under `.cache/inline-snapshot` and mismatch reporting that never auto-mutates snapshots. dirty-equals carries partial-structure assertions inside larger facts. inline-snapshot is also the Python round-trip rail for `tests/contracts/` assets under the corpus law in [tests/contracts/README.md](../contracts/README.md).

## [07]-[DENSITY_AND_BANS]

A spec module is strong when one resolved strategy attacks construction, projection, failure categories, and an independent oracle together. Matrix folding is the default idiom: fault tables, promotion rows, params families, and projector sets collapse into row-driven folds where a new behavior is a row, never a new test function.

[BANNED_SHAPES]:
- Tautologies: frozen-raises checks, StrEnum roundtrips, `isinstance` on literals, `__all__` mirrors, encode-equals-encode, and meta-tests about test code.
- Blanket exemptions: burning a symbol out of the coverage gate without a per-symbol law-or-permanent-exempt ruling.
