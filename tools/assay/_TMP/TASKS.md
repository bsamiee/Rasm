# Assay Tool Tasks

## API Source And Surface Model

- Refactor `tools/assay/rails/api.py` so source resolution returns and consumes `ApiSource` directly, with `Path` values kept only as local IO-boundary projections.
- Delete rail-private `_Source`, `_api_source`, `_Detail`, and long-lived `_Surface`/`_Body` mirrors where the root `ApiSource` and `ApiSurface` already own the evidence.
- Construct `ApiSurface` through one emission fold that embeds the canonical `ApiSource` and avoids duplicated `source_kind`, `source_id`, and `version` fields.
- Keep any retained path-rich runtime cache mechanically derived from `ApiSource` and named as an IO projection rather than a second source model.
- Replace API source-kind branch sprawl with behavior rows keyed by `SourceKind`; each row owns source names, resolution, inventory, surface extraction, member extraction, artifact suffixes, and unsupported-source diagnostics for host assemblies, NuGet packages, Python distributions, TypeScript declarations, and tool/store-backed sources.
- Preserve NuGet package roots, frameworks, owners, nuspec, XML/assembly assets, selected paths, Python distribution sources, TypeScript declaration sources, host source candidates, miss reasons, and ArtifactStore-backed `api show` behavior.
- Make API doctor compact rows include `ApiSource.primary_assembly` when version and package root are empty so host assembly rows remain useful without opening the full inventory artifact.
- Replace private API-source seam tests with public CLI or registry tests for `api doctor`, `api resolve`, `api query`, `api show`, strict misses, direct artifact paths, ambiguous artifact tokens, and source details.

## Tree-Sitter And Code Query Kernel

- Collapse TypeScript tree-sitter grammar, parser, query, node-text, parse-error, query-error, capture, and declaration walking primitives into one existing owner consumed by both `rails/code.py` and `rails/api.py`; prefer `rails/code.py` unless implementation proves a stronger owner.
- Keep API-specific declaration, export, roster, and member projection in `rails/api.py` while reusing the shared code-query kernel.
- Cache language, parser, grammar, and query construction by language, suffix, TS/TSX grammar choice, and query source.
- Make TypeScript API declaration extraction query-driven where tree-sitter captures cover the shape, avoiding recursive walkers for covered declarations.
- Prove parser/query reuse with constructor-count tests and bounded multi-file query benchmarks.
- Route code listing artifacts through the active `ArtifactScope` instead of reopening `settings.store()` in `rails/code.py`.
- Normalize ast-grep completions once before both report folding and row/listing projection; add a parseable JSON `returncode == 1` test that reports empty without defect rows.

## ArtifactStore, Settings, History, And Coverage

- Collapse `ArtifactStore` path handling into one target/address resolver that accepts safe store-relative segments or a previously emitted backend path after containment validation.
- Route public operations such as `read_bytes`, `read_text`, `write_bytes`, `write_text`, `exists`, `info`, `remove`, `open_write`, metadata lookup, and direct emitted-path reads through the resolver; delete parallel `*_path` method families.
- Replace `glob`, `list`, `list_details`, `find`, and `find_details` with one walk/listing owner that normalizes fsspec rows, detail rows, recursive results, direct children, glob matches, metadata mtimes, and backend errors.
- Fold store-only helpers such as detail-path construction, mtime extraction, artifact matching, root-part splitting, and file-kind checks into `ArtifactStore`; keep only helpers genuinely shared with `AssaySettings.artifact`.
- Preserve file and memory backends, direct emitted-path reads, latest resolution, basename and substring lookup, full-report restoration, stream writes, history, local output adoption, and retention behavior.
- Define `ArtifactStore.write_many` failure semantics by validating all destinations before the first write, creating parents through the store owner, and surfacing unsupported transactions or write errors without mismatched proof artifacts.
- Make history ordering one metadata-backed contract across retention, listing, and delta prior selection, using mtime or created time plus run id as the stable tiebreaker and run-id ordering only as metadata-absent fallback.
- Delete registry history pass-through wrappers such as `_retain`, `_run_ids`, `_load_run`, and `_restore_full_report`; call `ArtifactStore` history methods directly from registry code or through a single store-owned selection operation.
- Route coverage artifact adoption through `ArtifactStore.adopt_file` or an equivalent store-owned method. Keep `.artifacts/python/coverage` only as the external coverage-tool output boundary, require current command receipts before adoption, and emit backend-owned artifact paths for file and memory backends.
- Move coverage output selection out of fixed command literals into settings, scope, catalog, or store ownership so JSON, XML, LCOV, and total-report outputs are generated under a run-scoped local boundary before adoption.
- Make lease storage an explicit local engine boundary, named for POSIX leases, so non-file artifact backends keep artifacts/history/show on the configured backend while leases remain local filesystem guards or fail with explicit unsupported evidence.
- Tighten `ArtifactBackend` root validation: reject relative traversal outside the workspace, normalize intentional absolute file roots, strip trailing separators for non-file roots, and reject empty non-file roots that would share a global namespace.
- Add settings-source laws proving `.env` and file-secret inputs are ignored with real temp files, init and environment sources apply in declared order, `ASSAY_OTEL_ENDPOINT` wins over OTLP aliases, and nested artifact backend env values construct the backend used by `settings.store()`.

## Engine, Streams, SSH, Leases, And Resilience

- Record retry-attempt evidence for transient spawn, connect, and broken-pipe failures. Keep recovered retries on `Completed.notes`, return exhausted retry facts on `Fault`, and prove non-retryable defects execute once.
- Unify local and SSH stream capture in `core/engine.py` through one owner-local drain fold that accepts stream label, receive operation, sink, path, and execution plan while preserving tail bytes, line counts, cancellation behavior, full process artifacts, and remote status mapping.
- Split process capture storage from .NET artifact-path and CLI-home injection so bridge, API, and in-process tool runs can persist process artifacts without changing command semantics.
- Pass capture-capable scopes through bridge client runs and API ilspy/in-process runs where useful, and emit readable `ArtifactKind.PROCESS` logs through `ArtifactStore`.
- Preserve SSH signal evidence rather than flattening signal-only exits to an unqualified status code; expose signal name or mapped signal status in receipts or diagnostics.
- Add remote execution evidence for active trace/baggage propagation, `ASSAY_RUN_ID`, signal mapping, SSH status mapping, and fan-out connection reuse.
- Add fan-out deadline behavior preserving tuple length, input order, completed slots, and timeout faults only for expired slots.
- Add large-stream proof that full artifacts persist before bounded receipt tails are clipped.
- Add stale lease owner-block diagnostics and lock laws for empty owner bytes, corrupt owner bytes, stale owners, lost steal races, and live holders with `psutil.AccessDenied`.
- Define leased operation behavior for non-file artifact backends: reject before POSIX lock acquisition with explicit unsupported evidence or add a store-native atomic lease path with equivalent stale-owner semantics.
- Replace automation CPU gating with warmed nonblocking `psutil.cpu_percent(interval=None)` strategy and prove fire paths do not impose fixed sleeps while still skipping work above threshold.

## Aspect, AOT, Logging, And OTel

- Route `tools.assay.bootstrap_error()` through CLI dispatch before command execution so malformed `ASSAY_*` settings emit one config envelope from the preserved bootstrap fault and tracing setup does not construct and discard a second settings error.
- Strengthen aspect validation proof through public owners: registry validation envelopes, engine validation faults, and checked-layer beartype violations should return validation faults rather than escaping uncaught exceptions.
- Prove rail log/span correlation with CI JSON logging and active spans: stdout remains one envelope, stderr carries rail finish events with bound context, callsite fields, trace id, span id, and no context leakage between invocations.
- Prove active trace and baggage injection into local subprocess and SSH environments rather than only preserving explicit `traceparent` and `baggage` env inputs.
- Add provider lifecycle isolation: envelope dispatch happens before force-flush and shutdown, and provider failures write diagnostics to stderr without extra stdout envelopes.
- Collapse duplicated sync and async span/context body in `core.aspect.traced()` only if the resulting projection reduces type suppressions while preserving `ParamSpec` inference, signature preservation, `__wrapped__`, duplicate-decoration idempotency, span stamping, and typed `Result` returns.
- Remove `opentelemetry-instrumentation-logging` from `pyproject.toml` and `uv.lock` unless logging instrumentation is wired through the existing tracing/aspect lifecycle with stdout isolation, stderr diagnostics, context propagation, flush, and shutdown proof.

## Automation

- Replace `Watch.filter: str` with a msgspec-decoded owner vocabulary that projects to watchfiles filter behavior while preserving stable wire tags.
- Keep cron parsing at the `aiocron` boundary and timezone resolution at `ZoneInfo`; invalid cron and timezone values should emit setup-fault envelopes without adding a hand-written cron grammar.
- Add finite automation trigger tests for `Watch` and `Schedule`, using bounded AnyIO scopes and monkeypatched trigger sources to prove sorted batches, bounded ticks, NDJSON envelope counts, action order, cancellation, and scheduler cleanup.
- Add automation action matrix tests for malformed `Rail.params`, `Program`, `Sequence` ordering, fail-fast statuses, governed skip envelopes and counts, repeated/coalesced fires, CPU threshold boundaries, and both debounce collapse modes.
- Collapse private infinite-loop/state scaffolding in `automation/engine.py`: remove `_forever`, replace the list-wrapped `_RunState` mutation cell in `_hardened_fire` with the smallest owner-local state shape that preserves coalescing, and delete empty match arms.

## Static, Test, Package, Bridge, Docs, And Catalog Rails

- Make `TestParams.target` and `TestParams.all` real C# selector inputs or remove them from the CLI surface. Project target sets before check construction for `test run`, `test list`, and `test coverage`, and derive mutation eligibility from the same projection.
- Give `MutationLane.CHANGED` and `MutationLane.FULL` distinct execution behavior by encoding lane policy on the mutation owner, passing lane-specific arguments, or returning explicit unsupported evidence when a runner cannot support the requested lane.
- Preserve package stage evidence in `deploy` and `publish` reports: include stage/build receipt stream, stage artifacts, stage notes, and install/push or bridge lifecycle receipts.
- Collapse package lifecycle step execution into behavior-bearing step rows that own runner kind, mode, lease requirement, terminal status behavior, install/push/quit/refresh actions, and terminal-status projection.
- Return real bridge build evidence from `bridge build`, folding actual build receipts and build-scope artifacts instead of replacing successful builds with synthetic receipts.
- Pass artifact scopes through bridge client runs that produce useful evidence, including `bridge verify` scenario JSON artifacts and client stdout/stderr stream artifacts.
- Make escaped bridge scenario inputs observable with explicit unsupported diagnostics for absolute outside-workspace paths, `..` escapes, and glob patterns resolving outside the workspace.
- Inline `docs.py` `thin_rail` into `check` or make it private under a docs-specific name; remove `thin_rail` from `__all__` and keep strict-promotion sentinel only if registry strict mapping still needs it.
- Add docs rail command-shape tests proving routed Markdown files spawn `mmdc -i <file>` through monkeypatched `fan_out`; keep `docs check` scoped to Mermaid render validation unless catalog rows add broader docs proof.
- Move self-test probe construction, cache freshness, probe-note projection, and cache persistence out of registry command composition into the catalog, engine, or store owner that carries the behavior.
- Type the registry probe cache as msgspec-owned rows with key, token, timestamp, note, and status; malformed or stale payloads become live-probe misses.
- Add catalog-driven probe laws requiring each non-in-process tool row to provide a meaningful version/help probe or explicitly mark presence-only probes.
- Improve module tool self-test output, including `tools.py_analyzer` and `squawk`, so usage-status version probes report catalog-owned probe meaning rather than generic `present (exit N)`.
- Collapse the duplicate catalog `[TABLES]` section and enum alias constants into canonical enum members or one row-construction fold if it reduces real repetition without parallel vocabulary.
- Move pytest-benchmark storage policy out of catalog row literals; rely on pyproject-owned benchmark storage or project the storage path from settings/store ownership.

## CLI, Registry Help, README, And Safety Text

- Replace `_emit_envelope` boolean short-circuit persistence with an explicit `if persist:` branch and remove the associated type ignore at the stdout/history boundary.
- Update registry help strings so `api query` describes polyglot API extraction, `docs check` describes Mermaid render validation, `test coverage` describes coverage-enabled test rows plus coverage client artifacts, and `package list` describes the slug/project roster it returns.
- Update the operator README output contract to state that explicit `--help` and `--version` are Cyclopts meta output while command invocations and parse failures use the JSON envelope contract.
- Add compact README operator safety classification for read-only, mutating local, live-host, cleanup, deploy/publish, and secret-consuming commands, keyed to existing command rows without turning the README into another full command catalog.

## Executable Proof To Add With Implementation

- Add public API CLI/registry tests covering doctor inventory artifacts, source detail rows, query roster/search/decompile output, strict misses, ambiguity candidates, and artifact restoration.
- Add file and memory `ArtifactStore` matrices for history, full-report restoration, listing, finding, removal, direct paths, latest lookup, scoped lookup, and backend-path reads.
- Add routing tests for discovery faults from changed-file and explicit enumeration paths, including the default local changed path through engine-owned discovery.
- Add CLI tests for help, version, malformed settings, normal self-test, parse faults, stdout/stderr channel shape, exit-code projection, claim/verb selection, and `error_context.failing_step`.
- Add msgspec schema proof for `Envelope`, `Report`, `AnyDetail`, `ApiSource`, `ApiSurface`, and automation unions at the model/test owner or as a self-test artifact.
- Add static/test/package/bridge rail tests for selector behavior, mutation lanes, coverage adoption, package stage evidence, bridge build evidence, scenario artifacts, escaped scenario diagnostics, and docs command shape.
