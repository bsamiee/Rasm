# [ASSAY_TESTS_AGENTS]

Scope: `tests/tools/assay/` only. Parent policy owns universal Python and test behavior; this overlay owns Assay-specific proof shape for the tool, benchmarks, fixtures, and `_TMP` review reports.

## [1][READ_BEHAVIOR]

- When changing a production owner under `tools/assay`, update the test at the same owner boundary: `core`, `composition`, `rails`, `automation`, or `_benchmarks`.
- When changing shared fixtures, strategies, harnesses, artifact stores, subprocess doubles, SSH doubles, or benchmark harnesses, read `conftest.py` before adding local setup.
- When changing wire structs or detail unions, read `core/model.py`, `tests/tools/assay/conftest.py`, and the model tests before adding examples.
- When changing report, history, show, list, preview, or artifact behavior, read `composition/test_settings.py`, `composition/test_registry.py`, and the consuming rail test.
- When adding or resolving `_TMP` audit material, keep it source evidence only; promote durable rules into `tools/assay/AGENTS.md`, this file, source, README, or tests.

## [2][PROOF_CONTRACT]

Tests prove Assay owners, not old Quality shapes. Preserve capability through current `Envelope`, `Report`, `Artifact`, `Match`, tagged detail, `Tool`, `Check`, `ArtifactStore`, and engine contracts. Do not add compatibility snapshots, stale command aliases, or output fixtures that make obsolete shapes authoritative.

For every behavior change, pick the narrowest executable owner proof:
- Wire behavior uses msgspec round-trip, schema, and detail-union laws.
- Settings and storage behavior uses file and memory backend store laws.
- Routing behavior uses `Source` doubles and engine-owned discovery checks.
- Engine behavior uses `run_check`, `run_check_async`, `fan_out`, stream artifacts, retry, lease, SSH, and resource snapshot boundaries.
- Rail behavior uses params, catalog selection, route projection, fold, artifacts, strictness, and diagnostics at the rail boundary.
- Automation behavior uses NDJSON envelope framing, limiter/coalescing, watch/schedule/manual triggers, and async execution ownership.
- AOT/aspect behavior uses import isolation, checked/logged/traced layer proof, stderr-only diagnostics, and provider lifecycle proof.
- Benchmarks measure an existing user-meaningful path and assert status correctness or a bounded invariant, not only elapsed time.

## [3][FIXTURE_CONTRACT]

- Add shared strategies or harness helpers only when at least two tests consume them or when a law-style strategy replaces repeated literals.
- Prefer parametrized laws over one-off example clusters when the same owner shape is exercised across statuses, backends, languages, or modes.
- Keep intentional private-boundary probes local and explicit; a private import in tests must prove an owner invariant that cannot be reached through the public rail.
- Use real libraries where they own the behavior: pytest fixtures/capsys/monkeypatch, Hypothesis strategies, msgspec encode/decode, dirty-equals/inline-snapshot where already established, fsspec memory stores, AnyIO test tools, and psutil/SSH doubles from the harness.
- Do not hand-roll subprocess, filesystem, clock, socket, or coverage behavior when the existing harness or approved test library owns it.

## [4][BENCHMARK_CONTRACT]

- Benchmarks live under `_benchmarks` and measure Assay owner paths: CLI dispatch, registry invocation, engine spawn/fan-out, routing, storage backends, automation, and model encoding.
- A benchmark name states the path it measures. If the setup uses changed paths, changed routing, memory backend, or in-process dispatch, the name says so.
- Benchmark tests keep payloads bounded and deterministic; they assert the operation produced the correct status, rows, bytes, or artifacts before timing is accepted.
- Do not add benchmark-only helpers or state machines that no benchmark consumes.

## [5][REJECTIONS]

- No skipped proof for a landed production behavior.
- No shape-only tests where a behavior test can hit the owner boundary.
- No docs-only proof for engine, artifact, lock, package, bridge, API, automation, or AOT behavior.
- No direct local file assertion when the report contract is an `ArtifactStore` path.
- No one-off expected strings repeated across tests when a fixture, strategy, enum, or owner projection can supply the value.
- No benchmark with no correctness assertion.
- No `_TMP` finding treated as instruction until it is promoted into an active owner file.

## [6][STOP_RULES]

If a behavior cannot be proved without live Rhino, deploy, publish, secret-consuming endpoints, or destructive mutation, keep the production path source-audited and add isolated tests for the owner contract. Mark the host/runtime proof gap instead of weakening the unit test into a fake success.
