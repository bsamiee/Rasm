# [PYTHON_TESTING]

Authoring law for every Python spec, kit member, and tool suite under `tests/python`. Every suite composes `tests/python/_testkit` through the root conftest registration; a helper the kit already owns is composed, never redeclared.

## [01]-[ROUTER]

- [01]-[RULINGS](RULINGS.md): Settled Python-tree testing decisions — package admissions, oracle discriminants, structure retirements.
- [02]-[API](.api/): Dev-tool API catalogs, one per test-stack package; kit members and specs transcribe at catalog-verified spellings.
- [03]-[CONTRACTS](../contracts/README.md): Corpus consumer law — byte-identical round-trips over every C#-emitted asset.

## [02]-[TOPOLOGY]

- Per-package suites live in `tests/python/libs/<package>/` mirroring `libs/python`; tool suites live in `tests/python/tools/<tool>/`.
- One root `tests/python/conftest.py` is the registration owner: `register_tree` derives every `libs/python` SUT from disk shape — a package registers the moment it carries source.
- Package `conftest.py` files compose fixtures and seams only; a tool suite registers itself in its own conftest.

## [03]-[KIT]

`tests/python/_testkit` is the one project-agnostic kit:
- [01]-`spec.py`: Pure assertion oracles — algebraic laws, `refutes`, matrix folds, the `close` tolerance algebra, rail asserts, `model_based`.
- [02]-`strategies.py`: `resolve(subject)` — the one Hypothesis resolver over msgspec and pydantic-core algebras; defaulted fields sample absence.
- [03]-`seams.py`: `Shape` call-shape union, `SeamProbe`, loopback and grpc capsules, `VariantWriter`, `tmp_root`, `NdjsonOracle`, process doubles.
- [04]-`env.py`: Declarative environment doubles — `SshHost`, `RemoteFS`, `ObjectStore` — under one polymorphic `provision` dispatch.
- [05]-`bench.py`: `BenchCase` registry rows, absolute-budget gates, and sustained-regression detection.
- [06]-`corpus.py`: Contract-corpus proof — `load_manifest`, the `audit` pin-state fold, `assert_corpus_roundtrip` byte-identity.
- [07]-`laws.py`: `@spec` registration, `COVERS` consumption, `auto_exempt`, SUT registration, and the `assert_law_coverage` census gate.
- [08]-`runtime.py`: Runs the pytest plugin — Hypothesis profiles and example database, marker auto-application, artifact routing.

Falsification suites ride beside the kit modules as `test_<module>.py`, `test_policy.py` carrying the SUT-agnostic policy meta-laws.

Every oracle is proven able to pass and to fail; a kit capability without a falsification law here is unproven and gets deleted or proven, never trusted.

## [04]-[LAWS]

`@spec(subject)` is the one registration surface: it emits a `LawRecord` into the manifest, applies the Hypothesis profile stack, and with `given=True` injects `resolve(subject)` as the generated argument — the subject algebra matches the resolver's (classes, PEP 695 aliases, and parameterized type forms all inject; bare callables refuse) — one decorator owns strategy injection, profile selection, marker application, deadline override, and coverage attribution:

```python conceptual
@spec(Shape, mutation=True, events=(lambda drawn: f"kind={drawn.kind}",))
def test_shape_roundtrip(shape: Shape) -> None:
    assert_roundtrip(shape, Shape)
```

- Coverage credit derives from two sources only: `@spec(subject)` decorations and one declarative `COVERS: tuple[object, ...]` per test module, consumed at collection; double-decoration is a registration failure, not a silent stack.
- `assert_law_coverage` folds the manifest against each registered SUT package's public surface and fails on uncovered symbols and import failures alike — an unimportable module is uncovered surface, never a silent skip.
- Its census is subset-aware: `register_sut` derives each package's suite root, `uncollected_laws` names every on-disk law module the session never imported, and the gate censuses only fully-collected packages while skipping partial ones by name — a targeted run never reports false gaps and never silently passes them.
- `auto_exempt` removes StrEnums, method-free frozen structs, and value-only symbols by predicate; anything else exempts per symbol with a recorded justification through `register_sut(..., exempt=...)`, never a blanket package exemption.
- An unpinned `@spec` law follows the session-active Hypothesis profile, so the mutation and CI lanes govern every law through their CLI profile selection; `profile=` pins deliberately, and `timeout=` is a per-law deadline in seconds.

Hypothesis profiles are registered once in `runtime.py` and selected by name: `rasm` (default), `rasm-ci`, `rasm-stress`, `rasm-debug`, `rasm-adversarial` (deep example budget for hostile-input degradation laws), `rasm-mutation` (derandomized, database-free, short traces to preserve kill-signal budget), `rasm-stateful` (long traces for interleaving counterexamples), and `rasm-parity` (derandomized and database-free for byte-stable cross-tool comparison).

Example databases live under `.cache/hypothesis` with an optional read-only CI replay multiplex; observability output routes to `.artifacts/python/hypothesis` when `TESTS_OBSERVABILITY` is set.

## [05]-[ORACLES]

Algebraic, matrix, rail, and stateful proofs ride the kit oracles:
- Algebraic families prove through the `spec` oracles — `roundtrip`, `identity`, `idempotent`, `involution`, `inverse`, `commutative`, `associative`, `distributive`, `absorbing`, `identity_element`, `monotone`, `permutation_invariant`, `metamorphic`, `metamorphic_sweep` — each parameterized by an explicit equality policy.
- Every law family carries a refuting witness through `refutes`: a known-broken input the law must fail on, proving the law can fail at all.
- Case families fold as matrices, never fact-per-case spam: `validity_matrix` over `ValidityCase` rows, `projection_matrix` over `ProjectionCase` rows, `support_matrix` over probe rows. A new case is a row, and a fold handed the `subtests` fixture (the `RowCarrier` protocol) reports every breached row independently instead of stopping at the first; every fold refuses an empty row set as proving nothing.
- Rail outcomes prove through `assert_ok`, `assert_error`, `assert_error_status`, `assert_some`, and `assert_none` — never truthiness checks on the carrier; `RuntimeRail` values are `expression` Results, so the same asserts own them and `attr="tag"` matches a `BoundaryFault` arm. `isinstance` narrowing a wire union to its arm is the sanctioned discriminated-union assertion; `isinstance` on the carrier itself is the banned dance.
- A tool-output passthrough claim asserts a message substring only beside its `assert_error_status` identity check — the status sentinel is the stable contract, the message the operational cause.
- Numeric, array, quantity, struct, rail-carried, and container facts prove under one tolerance policy: `close(rel_tol=, abs_tol=)` slots into any oracle's `eq` axis, `Result`/`Option`/`Block` carriers compare payload-recursively, and `assert_close` names the first diverging structural path. Deadline, retry, and drain laws run in wall-microseconds under `autojump_backend`.
- Stateful subjects prove through `model_based` over a `RuleBasedStateMachine` under the `rasm-stateful` profile, composing `rule`/`initialize`/`precondition`/`Bundle`/`consumes` from the kit surface; `target` guides search toward extremal observations under the `rasm-stress` profile's target phase. Process-boundary output decodes through `NdjsonOracle`, never string-contains scraping.

## [06]-[SEAMS]

- Seam substitution dispatches on the `Shape` variant — `Sync`, `Async`, `FanOut`, and `Factory` are the call shapes of one substitution owner, installed through `SeamProbe` with recorded calls projected via `projected`.
- Loopback servers ride `loopback_server`/`Loopback` under the `network` marker; `grpc.aio` services — the server-runtime and tessellation-daemon lanes — ride `grpc_loopback`, which binds an ephemeral port and yields the endpoint with a connected channel.
- Filesystem fixtures ride `VariantWriter` and `tmp_root`; remote environments ride the `provision` dispatch over `SshHost`/`RemoteFS`/`ObjectStore`, where `ObjectStore` serves a real S3-compatible loopback endpoint projected as an `s3fs` view carrying the s3-native egress surfaces — e-tags and presigned GET — that the in-process memory double honestly refuses.
- Where a public driver exists, drive it: genuinely white-box seams (stall verdicts, crash recovery) earn direct probes, everything else goes through the public surface.

## [07]-[LANES]

Marker taxonomy is closed and declared in `pyproject.toml`; the runtime plugin auto-applies `network` and `property` from fixture and Hypothesis membership:

| [INDEX] | [MARKER]     | [MEANING]                                                                          |
| :-----: | :----------- | :--------------------------------------------------------------------------------- |
|  [01]   | `property`   | Hypothesis-driven law                                                              |
|  [02]   | `network`    | real INET sockets lifted: loopback servers or egress; excluded from mutation lanes |
|  [03]   | `subprocess` | spawns the real CLI in a child interpreter; excluded from mutation lanes           |
|  [04]   | `benchmark`  | measurement session, excluded from the default run                                 |
|  [05]   | `mutation`   | mutation-acceptance and survivor-triage laws                                       |

A default run is the unit lane: sockets disabled through pytest-socket, benchmarks deselected, the `rasm` profile active. `network` and `subprocess` markers are the Python spelling of the integration lane. Mutation is a staged gate under assay: mutmut policy lives in `pyproject.toml` `[tool.mutmut]` with the absolute-path coverage side-file `.config/coverage-mutmut.ini`, and `subprocess`-marked tests stay out because children execute the unmutated tree. Its per-mutant timeout bounds cap any bare `mutmut run`; concurrency is CLI-owned (`--max-children`, assay-governed).

## [08]-[SNAPSHOTS]

inline-snapshot owns genuine wire goldens only — payloads an independent producer emits — with storage under `.cache/inline-snapshot` and mismatch reporting that never auto-mutates snapshots. dirty-equals carries partial-structure assertions inside larger facts. Contract-corpus proof rides `corpus.py`: `load_manifest` decodes `tests/contracts/MANIFEST.md`, the `audit` fold gates pin-state honesty and producer-anchor resolution, and `assert_corpus_roundtrip` proves byte-identical re-encode over every emitted `REAL` asset under the corpus law.

## [09]-[DENSITY_AND_BANS]

A spec module is strong when one resolved strategy attacks construction, projection, failure categories, and an independent oracle together. Matrix folding is the default idiom: fault tables, promotion rows, params families, and projector sets collapse into row-driven folds where a new behavior is a row, never a new test function.

[BANNED_SHAPES]:
- Tautologies: frozen-raises checks, StrEnum roundtrips, `isinstance` on literals, `__all__` mirrors, encode-equals-encode, and meta-tests about test code.
- Blanket exemptions: burning a symbol out of the coverage gate without a per-symbol law-or-permanent-exempt ruling.
- Kit bypass: a spec-local assertion helper, tolerance constant, or strategy that shadows an existing kit owner — extend the owning kit module instead.
