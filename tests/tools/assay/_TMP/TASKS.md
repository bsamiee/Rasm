# Assay Test Tasks

## Harness And Wire Owners

- Derive the detail-variant set from `AnyDetail.__value__` in the assay test fixture owner and reuse it for `detail_st`, registered detail strategies, and `core/test_model.py`.
- Add shared `unwrap_ok`, `unwrap_error`, and `assert_result_status` result oracles, then replace repeated `is_ok()` and `is_error()` assertions while preserving local evidence checks.
- Add one module-level `Envelope` decoder and `read_one_envelope` oracle for text and binary output; route CLI, automation, registry, and benchmark envelope decoding through it.
- Add a CLI result object for observability tests that carries decoded envelope, exit code, raw stdout bytes, and raw stderr bytes.
- Use `dirty-equals` for dynamic envelope, report, event, diagnostic, and artifact partials; use `inline-snapshot` only for normalized stable public wire JSON.
- Convert path rejection, run-id validation, and other repeated inline-loop examples into parameterized law matrices.
- Add automatic `property` marker application for Hypothesis-backed tests under `tests/tools/assay`, and assert `pytest -m property --collect-only` collects the current property laws.

## Wire Model And Schema

- Add a law proving every `AnyDetail` variant subclasses `Detail`, every variant has a unique `msgspec` tag, and `Report.detail` inspects to exactly the derived union.
- Add `msgspec.inspect.type_info` or schema-component assertions for stable wire fields on `Envelope`, `Report`, `ApiSource`, `ApiSurface`, and automation `Trigger`/`Action` tagged unions.
- Add unknown-field rejection laws for registered wire structs with `forbid_unknown_fields=True`.
- Replace isolated `Completed` and `Fault` JSON examples with a registered-struct round-trip oracle using the shared wire round-trip harness.
- Round-trip generated populated `detail_st` values, including non-default `ApiSource` payloads, through `validate_detail` and the deterministic msgspec codec.
- Cover exit-code-to-envelope projection in owner-level wire tests.

## CLI, Registry, And History

- Replace one-off main-entry parse tests with a table covering empty argv, bare claim, unknown top-level command, unknown verb under a real claim, invalid enum values, invalid numeric values, malformed `ASSAY_*` settings, surplus positional tokens, and normal `self-test`.
- For parse/config cases, assert emitted envelope claim, verb, status, `error_context.failing_step`, and a stable message fragment.
- Add direct help and version capture tests because Cyclopts owns those operator-text paths outside the normal envelope contract; assert `--version` matches `pyproject.toml`.
- Assert normal and parse-fault CLI calls emit exactly one JSON envelope on stdout and keep diagnostics on stderr.
- Add registry laws proving `REGISTRY` has unique `(claim, verb)` pairs and `build_app(REGISTRY)` parses every registered claim/verb leaf.
- Add a public `self-test` registry/CLI test that emits one envelope, persists history, includes registry/catalog census rows, and keeps diagnostics off stdout while live probes are monkeypatched.
- Add public `delta` tests that seed persisted history through `ArtifactStore`, invoke the command boundary, restore clipped full reports before comparison, handle missing or corrupt full-report artifacts, and avoid persisting parse-fault envelopes.
- Add registry probe-cache tests for load, store, malformed-cache recovery, and write-failure behavior through `ArtifactStore`.

## ArtifactStore And Settings

- Build a file-plus-memory `ArtifactStore` law matrix for `write_bytes`, `write_text`, `open_write`, `list`, `list_details`, `find`, `find_details`, `info`, `glob`, `remove`, `write_history`, `load_history`, `retain_history`, `write_full_report`, `restore_full_report`, `resolve_artifacts`, create-only behavior, transaction behavior, read-after-remove behavior, metadata parity, and escaped-path rejection.
- Cover corrupt full-report artifacts, missing artifacts, empty history, equal mtimes, direct backend path precedence, latest selection across file and memory stores, ambiguity diagnostics, and ambiguity note text.
- Add generated safe path segments plus explicit traversal and backend-escape examples to store path laws.
- Add settings tests using `monkeypatch` and temporary files for `ASSAY_ROOT`, `ASSAY_RUN_ID`, nested artifact backend env values, OTLP endpoint alias precedence, and proof that `.env` and secrets files are ignored by constructed `AssaySettings`.

## API Rail Proof

- Add public registry/CLI tests for `api doctor` that assert compact envelope output plus persisted full inventory JSON/TSV artifacts read through `ArtifactStore`.
- Decode persisted doctor inventory as `tuple[ApiSource, ...]` and assert decoded row count matches artifact line count and the emitted detail line count while stdout remains a preview.
- Add public `api resolve` and `api query` source-kind matrices over assembly, NuGet, Python distribution, TypeScript declaration, and store-backed artifact sources.
- Cover query namespace, type, member, search, decompile, roster, malformed declaration, missing package root, XML-only NuGet package, parse-error preservation, and query artifact preview through `api show`.
- Assert emitted `ApiSurface.source` carries the expected `ApiSource` fields for Python distribution and TypeScript declaration fixtures.
- Add strict API miss tests for resolve, query, and doctor that preserve candidates, inventory artifacts, and diagnostics while promoting incomplete strict reports to faulted envelopes.
- Extend `api show` public tests for direct backend paths, ambiguous artifact tokens, missing artifacts, strict empty output, full-report restoration, and latest selection across API and non-API scopes.

## Engine, Routing, SSH, And Locks

- Add a `fan_out` deadline law that runs fast and slow checks under a shared absolute deadline, preserves input order and tuple length, keeps completed slots, and maps only expired slots to timeout faults.
- Add `run_check` rows proving non-retryable spawn failures are not retried: `FileNotFoundError` maps once to unsupported and `TimeoutError` maps once to timeout.
- Expand streaming process tests with large stdout and stderr payloads beyond `stream_tail_bytes`; assert receipt tails are clipped and process artifacts contain full byte payloads with exact byte and line counts.
- Add lock laws for contended empty owner bytes, corrupt owner bytes, stale owner stealing, lost steal race, and live holder `psutil.AccessDenied`, mapping live or lost-race cases to busy.
- Decode a held lease owner block and assert resource, run id, cwd, project, mode, target, pid, and create time.
- Add SSH laws for remote command `cd <cwd>`, allowed remote env keys including `ASSAY_RUN_ID`, `_ssh_status` mapping, pooled connection reuse under `fan_out`, transient `_connect_once` retry success, and streaming SSH artifacts through `ArtifactStore`.
- Replace platform-sensitive CPU governor tests with a `_make_psutil_module` table covering affinity count, affinity `AccessDenied`, missing affinity, `cpu_count(None)` fallback, `dotnet_max_cpu`, and `mutation_max_cpu`.
- Add routing laws for changed/full/project graph failures, route failure diagnostics, and engine-owned discovery deadline/failure behavior.

## Aspect, AOT, And Observability

- Add aspect-layer laws proving `checked`, `logged`, and `traced` are load-bearing through registry or engine boundaries, including bad-argument validation faults and success-path ordering.
- Assert logged success and fault events include bound context and that context clears between invocations.
- Assert traced success and fault spans include `assay.status`, `assay.run_id`, baggage-projected attributes, and fault events.
- Add OTel lifecycle tests for endpoint-backed tracing installation selected from `AssaySettings`, monkeypatched provider resource fields, force-flush and shutdown ordering, and provider failure diagnostics without extra stdout envelopes.
- Assert active trace and baggage context is injected into local subprocess or remote environments instead of only preserving explicit input strings.

## Automation

- Add finite success-path tests for watch and schedule triggers using monkeypatched `awatch` and fake `aiocron.crontab`; assert NDJSON envelope count, decoded statuses, sorted change batches, invalid timezone setup faults, and `cron.stop()` on cancellation.
- Add action-matrix tests over `Rail`, `Program`, `Sequence`, and `Debounce`, covering malformed `Rail.params`, sequence ordering, fail-fast on failed/busy/timeout/faulted leaves, governed skip envelopes and counts, and both debounce collapse modes.
- Add watch filter tests for default and python filters, including `ignore_patterns`, ignored-path suppression, accepted-path batch normalization, and emitted envelope ordering.
- Add stateful/adversarial automation tests for debounce interleavings, cancellation cleanup, repeated or coalesced fires, schedule cron/timezone invalid cases, and CPU threshold boundaries at `0`, `1`, and exact equality.

## Static, Package, Bridge, Test, And Catalog Rails

- Add static rail tests proving `build()` or `_dispatch()` opens `ArtifactScope.build(settings, _closure_sha(routed))`, acquires `build-<closure>-<configuration>`, returns busy without `fan_out` when held, and exposes plan artifacts/preview commands derived from the same closure hash.
- Add `static full` tests proving empty params route to `Workspace.slnx` for both Debug and Release settings, aggregate both configurations, honor full-trigger routing, and short-circuit on the first fault.
- Add `static fix` tests proving write leases use `write-<language>-<route>` and held leases produce busy reports.
- Add package lifecycle tests proving `_stage_meta()` runs build, artifact copy, yak build, and commit in order inside the package-stage lease; failed build or yak failure skips/cleans later work; held package-stage lease prevents build/copy/yak/commit; and no committed package artifacts remain after failed build.
- Add package artifact tests for manifest and primary `.rhp` requirements, manifest/icon/local artifact copying, host assembly filtering, store-owned package artifact rows, duplicate/missing Yak files, deploy/publish source-audit boundaries, bridge-bound deploy/publish bridge lease use, non-bridge yak install/push, and unsafe root rejection.
- Add bridge tests proving verify lease order, report-dir setup, build closure, launch, discovery, scenario execution, build/launch fault promotion through `_affirm`, bridge build closure scope/lease, lifecycle verb bridge leases, scenario discovery for files/directories/globs, escape rejection, stale report cleanup containment, and verify output artifact adoption.
- Add mutation rail tests proving changed/full lane selection, single mutation lease acquisition, held mutation busy reports, target/all gap behavior without locking, run/mutation/coverage dispatch ordering, mutation JSON detail promotion, failed mutation folding, stream artifact persistence, staged workdir materialization, no live worktree mutation, and no leaked `mutants/` artifacts.
- Add coverage rail tests proving coverage client artifacts are adopted into `ArtifactStore` for file and memory backends, require current command receipts before adoption, expose backend-owned artifact paths, and can be read back through the store API.
- Expand catalog tests into data-driven row laws for static restore/build, test run/coverage/benchmark/mutation, package query/stage/deploy/publish, bridge verify/build, runner/input/mode/timeout/project routing, storage outputs, stage policy, and row identity uniqueness.

## Benchmarks

- Add `_benchmarks/conftest.py` or another benchmark-owned fixture surface with one benchmark protocol covering `pedantic`, `extra_info`, and `stats`; replace repeated per-file benchmark Protocol classes.
- Add a shared benchmark metadata recorder that records status, row count, truncation, artifact count, artifact bytes, stdout bytes, stderr bytes, RSS delta, CPU-time delta, subprocess count where exposed, and failed count where relevant.
- Add command benchmark rows for `api doctor`, `api resolve`, representative `api query`, `api show`, `test list`, CLI version/parse cold-start paths, and registry static plan.
- Assert each command benchmark preserves envelope status, detail shape, row counts, clipped stdout, persisted full artifacts, artifact byte counts, and stable dispatch output before accepting timing.
- Add model decode and deterministic re-encode benchmarks; assert conservative mean ceilings for fold, encode/decode, changed-path routing, busy-lock contention, file/memory store write-read, and registry static plan.
- Replace manual routing timing with pytest-benchmark rows or move proportionality to a normal routing law.
- Parametrize fan-out benchmarks over `(1, 2, 4, 8, 16)` with ordering/status assertions and result/failure count metadata.
- Extend storage benchmarks across file and memory backends for write/read, list/find, detail metadata, history write/load, full-report restore, and retention behavior.
- Add automation benchmarks for program fire, hardened fire without jitter, debounce, schedule dispatch, CPU-governor skip, and `Rail` action worker-thread offload, recording envelope counts and stdout bytes.
- Add package metadata/plan and bridge lifecycle planning benchmarks with patched process/client seams and correctness assertions.
- Keep benchmark tests marker-isolated from the default suite and assert correctness plus explicit budget or ratio expectations.

## Coverage And Test Gate Truth

- Add executable rail tests that raise Assay coverage toward the configured gate instead of lowering the coverage policy, focusing on `rails/api.py`, `rails/package.py`, `rails/static.py`, `rails/bridge.py`, `rails/code.py`, `composition/registry.py`, `automation/engine.py`, and `core/routing.py`.
- Add mutation-marked acceptance tests and assert `pytest -m mutation --collect-only` collects the mutation acceptance lane.
