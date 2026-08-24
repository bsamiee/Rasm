# [PYTHON_TESTING]

Authoring law for every Python spec, kit member, and tool suite under `tests/python`. Every suite composes `tests/python/_testkit` through the root conftest registration; a helper the kit already owns is composed, never redeclared.

## [01]-[ROUTER]

- [01]-[RULINGS](RULINGS.md): Settled Python-tree testing decisions — package admissions, oracle discriminants, structure retirements.
- [02]-[API](.api/): Dev-tool API catalogs, one per test-stack package; kit members and specs transcribe at catalog-verified spellings.
- [03]-[CONTRACTS](../contracts/README.md): Corpus conformance law — Python proves verified vectors through their elected oracle.

## [02]-[TOPOLOGY]

- Per-package suites live in `tests/python/libs/<package>/` mirroring `libs/python`; tool suites live in `tests/python/tools/<tool>/`.
- Root `tests/python/conftest.py` owns registration: `register_tree` derives every SUT from disk shape, and a generated out root owes no suite.
- Package `conftest.py` files compose fixtures and seams only; a tool suite registers itself in its own conftest.

## [03]-[KIT]

`tests/python/_testkit` is the one project-agnostic kit:
- [01]-`spec.py`: Pure assertion oracles — algebraic laws, `refutes`, matrix folds, the `close` tolerance algebra, rail asserts, `model_based`.
- [02]-`strategies.py`: `resolve(subject)` — the one Hypothesis resolver over msgspec and pydantic-core algebras; defaulted fields sample absence.
- [03]-`seams.py`: `Shape` call-shape union, `SeamProbe`, the loopback capsule, `VariantWriter`, `tmp_root`, `NdjsonOracle`, process doubles.
- [04]-`env.py`: Declarative environment doubles — `SshHost`, `RemoteFS`, `ObjectStore` — under one polymorphic `provision` dispatch.
- [05]-`bench.py`: `BenchCase` registry rows, absolute-budget gates, and sustained-regression detection.
- [06]-`corpus.py`: Live `manifest.json` proof — `assert_corpus` composes Assay receipts with Python binding and package proofs.
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

- Coverage credit derives from `@spec(subject)` and one module-level `COVERS` alone, consumed at collection; double-decoration fails registration.
- `assert_law_coverage` folds the manifest against each registered SUT's public surface, an unimportable module reading as uncovered, never a skip.
- Census is subset-aware: `uncollected_laws` names every unimported law module and the gate censuses whole packages, skipping partials by name.
- `auto_exempt` removes value-only symbols by predicate; every other exemption rides `register_sut(exempt=...)` per symbol with its justification.
- Unpinned `@spec` laws follow the session-active Hypothesis profile; `profile=` pins one deliberately and `timeout=` sets a per-law deadline.

Hypothesis profiles are registered once in `runtime.py` and selected by name: `rasm` (default), `rasm-ci`, `rasm-stress`, `rasm-debug`, `rasm-adversarial` (deep example budget for hostile-input degradation laws), `rasm-mutation` (derandomized, database-free, short traces to preserve kill-signal budget), `rasm-stateful` (long traces for interleaving counterexamples), and `rasm-parity` (derandomized and database-free for byte-stable cross-tool comparison).

Example databases live under `.cache/hypothesis` with an optional read-only CI replay multiplex; observability output routes to `.artifacts/python/hypothesis` when `TESTS_OBSERVABILITY` is set.

## [05]-[ORACLES]

Algebraic, matrix, rail, and stateful proofs ride the kit oracles:
- Algebraic families prove through the `spec` oracles under an explicit equality policy; a new family lands as one oracle beside them.
- Every law family carries a refuting witness through `refutes`: a known-broken input the law must fail on, proving the law can fail at all.
- Case families fold as matrices where a new case is a row; a fold handed `subtests` reports each breach independently and refuses an empty row set.
- Rail outcomes prove through the kit's `assert_*` gates, `attr="tag"` matching a `BoundaryFault` arm; `isinstance` narrows a union, never a carrier.
- Tool-output passthrough asserts a message substring only beside `assert_error_status`: the status is the contract, the message the cause.
- One tolerance policy covers every fact through `close(rel_tol=, abs_tol=)` on any `eq` axis; timing laws run microseconds under `autojump_backend`.
- Stateful subjects prove through `model_based` over a `RuleBasedStateMachine` under `rasm-stateful`; `NdjsonOracle` decodes process-boundary output.

## [06]-[SEAMS]

- Seam substitution dispatches on the `Shape` variant, installed through `SeamProbe`, with recorded calls read back through `projected`.
- Loopback servers ride `loopback_server`/`Loopback` under the `network` marker; a serve capsule lands beside the first suite reaching it.
- Filesystem fixtures ride `VariantWriter` and `tmp_root`, while `provision` dispatches remote environments and `ObjectStore` serves real S3 loopback.
- Public drivers carry every proof they expose, a white-box seam like a stall verdict or crash recovery earning the direct probe.

## [07]-[LANES]

Marker taxonomy is closed and declared in `pyproject.toml`; the runtime plugin auto-applies `network` and `property` from fixture and Hypothesis membership:

| [INDEX] | [MARKER]     | [MEANING]                                                                          |
| :-----: | :----------- | :--------------------------------------------------------------------------------- |
|  [01]   | `property`   | Hypothesis-driven law                                                              |
|  [02]   | `network`    | real INET sockets lifted: loopback servers or egress; excluded from mutation lanes |
|  [03]   | `subprocess` | spawns the real CLI in a child interpreter; excluded from mutation lanes           |
|  [04]   | `benchmark`  | measurement session, excluded from the default run                                 |
|  [05]   | `mutation`   | mutation-acceptance and survivor-triage laws                                       |

Default runs are the unit lane: sockets disabled through pytest-socket, benchmarks deselected, the `rasm` profile active. `network` and `subprocess` markers are the Python spelling of the integration lane. Mutation is a staged gate under assay: mutmut policy lives in `tools/assay/pyproject.toml` `[tool.mutmut]` with the absolute-path coverage side-file `.config/coverage-mutmut.ini`, and `subprocess`-marked tests stay out because children execute the unmutated tree. Its per-mutant timeout bounds cap any bare `mutmut run`; concurrency is CLI-owned (`--max-children`, assay-governed).

## [08]-[SNAPSHOTS]

Inline-snapshot owns genuine wire goldens only: payloads an independent producer emits. Storage routes through `.cache/inline-snapshot`; mismatch reporting never auto-mutates snapshots. Dirty-equals carries partial-structure assertions inside larger facts. Blocked cases count without evidence and mint no bytes. Verified vectors elect `semantic-conformance`, `semantic-roundtrip`, `value-parity`, `external-digest`, or `publisher-digest`.

Assay receipts own corpus oracles; generated descriptors and package resources own Python-local proof.

## [09]-[DENSITY_AND_BANS]

Spec modules are strong when one resolved strategy attacks construction, projection, failure categories, and an independent oracle together. Matrix folding is the default idiom: fault tables, promotion rows, params families, and projector sets collapse into row-driven folds where a new behavior is a row, never a new test function.

[BANNED_SHAPES]:
- Tautologies: frozen-raises checks, StrEnum roundtrips, `isinstance` on literals, `__all__` mirrors, and meta-tests about test code.
- Blanket exemptions: burning a symbol out of the coverage gate without a per-symbol law-or-permanent-exempt ruling.
- Kit bypass: a spec-local assertion helper, tolerance constant, or strategy shadowing a kit owner; extend the owning kit module instead.
