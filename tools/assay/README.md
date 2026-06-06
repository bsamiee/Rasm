# [ASSAY_OPERATOR]

`tools.assay` is the Rasm polyglot quality-operator implementation for C#, Python, TypeScript, Bash, SQL, docs, bridge, package, and API proof. Root quality routing still names `tools.quality`; use Assay for Assay-owned work and explicit operator validation.

## [1][STATUS]

Status: active Assay operator; not the root-canonical quality route.
Use: Assay-owned development and explicit Assay validation.
Machine contract: normal CLI invocations emit one JSON `Envelope` on stdout; diagnostics ride stderr.
Automation: programmatic arm through `drive(trigger, action, settings)`; no registered root `watch` CLI.
Root quality policy, bridge routing, and repo-wide command ownership stay outside this README.

## [2][FIRST_COMMAND]

```bash copy-safe
uv run python -m tools.assay self-test
```

Verify: stdout contains one JSON `Envelope`; `status`/`exit_code` are the only process-result source; stderr may contain structlog events or tool diagnostics. Use `--rhino` only when the local Rhino bridge lane is intentionally part of the smoke.

## [3][FLOW]

```mermaid
---
config:
  layout: elk
  look: neo
  theme: base
  elk:
    mergeEdges: false
    nodePlacementStrategy: BRANDES_KOEPF
    cycleBreakingStrategy: GREEDY_MODEL_ORDER
---
flowchart LR
  accTitle: Assay orchestration boundary
  accDescr: CLI commands and automation fires enter rail or program execution, rail handlers select tools and route inputs into Check rows, the engine returns Completed receipts or Faults, and emit writes Report or error Envelopes to stdout.

  cli["CLI argv"] --> registry["REGISTRY\nBind rows"]
  registry --> rail["rail(bind)\nsettings + scope"]
  auto["Automation fire"] --> drive["drive(Action)\nlimiter/coalesce"]
  drive -->|"Rail"| rail
  drive -->|"Program / Sequence"| engine

  rail --> plan["rail-local plan\nselect TOOLS + route inputs"]
  plan --> checks["Check rows\nargv | INPROC"]
  checks --> engine["Engine\nrun_check / fan_out"]

  engine -->|"Completed"| fold["fold -> Report"]
  engine -->|"Fault"| diag["_distill -> Diagnostic"]
  fold --> emit["_emit\nEnvelope.report"]
  diag --> emitError["_emit\nEnvelope.error"]
  emit --> stdout["stdout\none JSON line"]
  emitError --> stdout
  drive -.-> ndjson["automation: NDJSON\nper fire / leaf"]

  rail -.-> railAspect["rail seam\nchecked -> logged -> traced"]
  engine -.-> engineAspect["engine seam\nchecked + traced around retried"]
```

Text equivalent: CLI argv resolves through `REGISTRY` into a `Bind`; the rail owns settings, scope, routing, check construction, engine dispatch, and fold. The engine runs `Check` rows and returns either `Completed` receipts for a `Report` or a `Fault` for an error `Envelope`. Automation uses the same engine and registry rails but emits NDJSON per fire or sequence leaf.

## [4][COMMANDS]

Command rows are the curated operator surface, not generated help. Run nested commands as `uv run python -m tools.assay <family> <verb> ...`; root commands omit `<family>`. Exhaustive parameter signatures stay in source and Cyclopts help.

This table is a lookup by command surface and verb set:

| [INDEX] | [SURFACE] | [VERBS]                                                         |
| :-----: | :-------- | :-------------------------------------------------------------- |
|   [1]   | root      | `self-test`, `delta`                                            |
|   [2]   | `static`  | `fix`, `report`, `build`, `full`, `plan`                        |
|   [3]   | `code`    | `search`, `rewrite`, `query`                                    |
|   [4]   | `test`    | `run`, `list`, `coverage`                                       |
|   [5]   | `bridge`  | `verify`, `doctor`, `launch`, `quit`, `check`, `clean`, `build` |
|   [6]   | `package` | `stage`, `deploy`, `publish`, `list`, `plan`                    |
|   [7]   | `api`     | `doctor`, `resolve`, `query`, `show`                            |
|   [8]   | `docs`    | `check`                                                         |

### [4.1][ROOT_COMMANDS]

Verbs: `self-test`, `delta`
Inputs: `self-test` accepts `--rhino`; `delta` accepts `<run_id>` and `--against <run_id>`.
Output: `Envelope.report`; `delta` carries `RunDelta` detail.
Use: `self-test` runs composition/catalog census; `--rhino` opts into bridge-aware smoke. `delta` compares retained history under `.artifacts/assay/history`.
Example:
    `uv run python -m tools.assay delta <run_id> --against <run_id>`

### [4.2][STATIC_COMMANDS]

Verbs: `fix`, `report`, `build`, `full`, `plan`
Inputs: `[paths...]`, `--language`
Output: shared `Report`; `plan` emits route/build-scope artifacts.
Use: `fix` mutates; `report` diagnoses; `build` compiles the routed closure; `full` runs the build-shaped closure rail.
Example:
    `uv run python -m tools.assay static plan --language csharp libs/csharp/Rasm`

### [4.3][CODE_COMMANDS]

Verbs: `search`, `rewrite`, `query`
Inputs: `[paths...]`, `--language`, `--pattern`, `--rewrite`, `--apply`, `--max-results`
Output: `Match` rows and rail artifacts.
Use: literal search uses ripgrep; metavars use ast-grep; `query` uses in-process tree-sitter; `rewrite --apply` mutates under lease.
Example:
    `uv run python -m tools.assay code search --pattern run_check --language python tools/assay`

### [4.4][TEST_COMMANDS]

Verbs: `run`, `list`, `coverage`
Inputs: `[paths...]`, `--language`, `--no-build`, `--mutation`, `--benchmark`, `--coverage`
Source-exposed params: `--target`, `--all`, `--filter`
Output: `TestRun` detail, or `Match` rows for `list`.
Use: mutation is boolean; `target` and `all` constrain mutation eligibility, while `filter` narrows .NET list/run invocations.
Example:
    `uv run python -m tools.assay test run --language csharp tests/csharp`

### [4.5][BRIDGE_COMMANDS]

Verbs: `verify`, `doctor`, `launch`, `quit`, `check`, `clean`, `build`
Inputs: `--pattern`
Output: `VerifySummary` detail where applicable.
Use: `verify` discovers direct file, directory, then worktree glob; no matched scenario returns `unsupported`.
Example:
    `uv run python -m tools.assay bridge verify --pattern tests/csharp/libs/Rasm.Rhino`

### [4.6][PACKAGE_COMMANDS]

Verbs: `stage`, `deploy`, `publish`, `list`, `plan`
Inputs: `--slug`, `--version`
Output: `PackageRun` detail.
Use: slug and version are flags, not positionals; stage/deploy/publish are Yak/Rhino-package operations.
Example:
    `uv run python -m tools.assay package plan --slug <yak-slug> --version <version>`

### [4.7][API_COMMANDS]

Verbs: `doctor`, `resolve`, `query`, `show`
Inputs: `--key`, `--symbol`, `--kind`, `--token`, `--max-lines`, `--lines`, `--grep`, `--full`, `--strict`
Output: `ApiSurface` or `ApiResolution` detail.
Use: sources include host assemblies, NuGet packages, Python distributions, and TypeScript declarations.
Example:
    `uv run python -m tools.assay api query --key rhino-common --symbol Rhino.Geometry.Mesh`

### [4.8][DOCS_COMMANDS]

Verbs: `check`
Inputs: `[paths...]`, `--strict`
Output: shared `Report`
Use: Mermaid render validation through `mmdc`; not full Markdown standards, links, or anchor validation.
Example:
    `uv run python -m tools.assay docs check tools/assay/README.md`

## [5][OUTPUT_CONTRACT]

Assay output is machine-first. The stable consumer rule is simple: parse stdout for results, use stderr for diagnosis, and treat the process exit as a projection of the emitted status.

### [5.1][WIRE_INVARIANT]

Normal invocation: exactly one JSON `Envelope` line on stdout.
Automation exception: NDJSON, one `Envelope` per fire or sequence leaf.
Failure split: `Completed(FAILED)` means a tool ran and found defects; `Fault` means assay could not run or complete the operation.
Schema route: full field-by-field schema lives in `core/model.py`; full status algebra lives in `core/status.py`.

### [5.2][CHANNELS]

stdout
    Carries: one JSON `Envelope` per normal CLI invocation.
    Consumer rule: decode this for status, exit, report, artifacts, results, detail, and diagnostics.
    Do not parse: stderr for result data.

stderr
    Carries: structlog events, subprocess stderr, and operator diagnostics.
    Consumer rule: read for human diagnosis.
    Do not parse: stderr as the machine result channel.

automation stdout
    Carries: NDJSON, one `Envelope` per fire or sequence leaf.
    Consumer rule: consume line-by-line.
    Do not expect: one aggregate sequence envelope.

process exit
    Carries: `Envelope.exit_code` through the Cyclopts return-code hook.
    Consumer rule: treat exit as a projection of `RailStatus`.
    Do not model: a separate process-status system.

### [5.3][STATUS_MODEL]

Status tokens: `skip`, `empty`, `ok`, `unsupported`, `busy`, `timeout`, `failed`, `faulted`.
Completed channel: process success, skip, empty, unsupported, or tool-found defects.
Fault channel: operational failure under `Envelope.error` with diagnostic context.
Strictness: flags such as `--strict` can promote otherwise non-error states into a fault for that invocation.

### [5.4][PAYLOAD_MAP]

Report detail: rail-specific evidence under `report.detail`.
Rows: bounded row output under `report.results`.
Artifacts: durable files under `report.artifacts`.
Truncation: inline lists may set `truncated=true`; envelope-level caps attach a full-report artifact before clipping rows.
Result locations are the Assay contract; older tool payload shapes do not define this operator.

## [6][INTEGRATIONS]

Integrations are grouped by the reader action they change. They are capability notes, not a dependency catalog.

[PROOF_TOOLCHAINS]:

`.NET` / `dotnet`
    Enables: C# restore, build, test, API, bridge, and package rails.
    Boundary: catalog rows own invocations; root policy still controls canonical quality use.

Python tools
    Enables: Ruff, ty, mypy, pytest, coverage, mutmut, and py-analyzer proof.
    Boundary: selected by claim and language rows.

TypeScript tools
    Enables: `tsc`, Biome, Knip, Sherif, Vitest, and ast-grep proof.
    Boundary: selected by claim and language rows.

Bash and SQL tools
    Enables: ShellCheck, shfmt, sqlfluff, and squawk proof.
    Boundary: selected by static rows.

[CAPABILITY_BACKENDS]:

Mermaid CLI
    Enables: `docs check` render validation on Markdown inputs.
    Boundary: `mmdc` only; no generic Markdown validation.

Rhino bridge and Yak
    Enables: live scenario verification and package stage, deploy, or publish.
    Boundary: exclusive bridge/package resources use leases.

API extraction
    Enables: host assembly, NuGet, Python distribution, and TypeScript declaration lookup.
    Boundary: `api resolve`, `api query`, and `api show` expose the operator surface.

tree-sitter
    Enables: in-process `code query` for Python and TypeScript AST shapes.
    Boundary: grammar-backed query, not text search.

[RUNTIME_BACKENDS]:

`psutil`
    Enables: resource snapshots, fan-out sizing, stale lease liveness, and the automation CPU governor.
    Boundary: operator resilience, not user telemetry.

OpenTelemetry
    Enables: optional spans when an OTLP endpoint is configured.
    Boundary: no endpoint means no-op tracing.

fsspec / UPath
    Enables: artifact-store shape and local file default.
    Boundary: most CLI operation still assumes local or shared paths.

`asyncssh`
    Enables: remote process execution through `ASSAY_EXEC_TARGET=ssh://...`.
    Boundary: moves command execution only; routing, artifacts, and locks still need local or shared paths.

Runtime model libraries are load-bearing but not command surfaces: `msgspec` owns wire structs, `pydantic-settings` owns `ASSAY_*` settings, `cyclopts` owns CLI binding, `anyio` owns concurrency, `expression` owns `Result` folding, and `beartype`/`structlog`/OpenTelemetry/`stamina` are the aspect stack.

## [7][AUTOMATION]

Automation is a programmatic arm, not a root CLI command. `drive(trigger, action, settings)` hosts fires under one AnyIO loop and writes NDJSON output.

`Trigger`
    Cases: `Watch`, `Schedule`, `Manual`.
    Effect: starts fires from filesystem changes, cron ticks, or immediate invocation.

`Action`
    Cases: `Rail`, `Program`, `Sequence`, `Debounce`.
    Effect: runs a registry rail, direct argv program, ordered action leaves, or coalesced action.

Important behavior:
- `Rail` actions dispatch through `REGISTRY`; registry rails emit their normal `Envelope`.
- `Program` actions run direct argv through the engine and emit thinner synthetic envelopes.
- `Sequence` emits each leaf and stops on `failed`, `busy`, `timeout`, or `faulted`; it does not emit one aggregate envelope.
- A per-drive limiter serializes action leaf execution so slow fires do not re-enter.
- `cpu_threshold` is fractional: `0.8` means 80 percent.
- Debounce can be trailing-edge collapse or leading-edge drain; schedule coalescing is separate from debounce.

## [8][ARTIFACTS_OBSERVABILITY_REMOTE]

Artifacts, logs, tracing, and remote execution are operator surfaces. They do not imply that every rail persists every byte or that Assay is a cloud-native workspace.

Storage and history:

Artifact root
    Default local root is `.artifacts/assay`.
    Individual rails decide which full outputs become artifacts.
    Inspect `report.artifacts` before assuming a file exists.

History
    Registry invocations persist envelope JSON by `run_id`; `delta` reads retained history.
    Parse faults and automation program envelopes are thinner than normal rail history.
    Use retained history for comparisons, not as a substitute for rerunning the rail.

Run scopes
    Per-run scopes live under the claim/run id; stable build scopes exist for build-like closures.
    Trust emitted artifact paths over inferred directory shapes.

Observability:

Logs
    structlog writes stderr; stdout remains the machine contract.
    stderr is diagnostic context only.

Tracing
    OTel is endpoint-gated through environment configuration.
    No endpoint means no tracing export.

Environment and remote execution:

Environment
    README-worthy env vars are `ASSAY_RUN_ID`, `ASSAY_AGENT_TASK_ID`, `ASSAY_ARTIFACT_RETENTION`, `ASSAY_EXEC_TARGET`, and `ASSAY_EXEC_KNOWN_HOSTS`.
    Use env vars for correlation, retention, and execution target control only where settings expose the behavior.

Remote execution
    `ASSAY_EXEC_TARGET=ssh://...` runs processes over SSH.
    Routing, locks, package staging, bridge discovery, API discovery, and many artifacts still need local or shared paths.

fsspec
    `ArtifactStore` is fsspec-shaped.
    Normal CLI use is local file storage.
