# [RHINO_BRIDGE]

## [01]-[REQUIREMENTS]

- macOS with Rhino 9 installed under `/Applications`, or `RHINO_APP_PATH` set to one Rhino `.app` bundle.
- Restored .NET project graph for the bridge projects and test projects that own typed scenarios.

## [02]-[COMMAND_SURFACE]

[SUPERVISOR]: `tools/rhino-bridge/Supervisor/Supervisor.csproj`
[STUB]: `tools/rhino-bridge/Stub/Stub.csproj`

| [INDEX] | [COMMAND]                                                       | [EFFECT]                                                        |
| :-----: | :-------------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `dotnet build <supervisor>`                                     | Compile the bridge and scenario closure                         |
|  [02]   | `dotnet run --project <supervisor> -- verify '{"$type":"all"}'` | Stage, run, unload, prepare quit, fold every typed scenario     |
|  [03]   | `dotnet run --project <supervisor> -- status`                   | Launch/reuse Rhino 9 and return endpoint, host, RPC, capability |
|  [04]   | `dotnet run --project <supervisor> -- quit`                     | Prepare Rhino/GH2 documents, then run the quit ladder           |
|  [05]   | `dotnet msbuild <stub> -t:YakInstall`                           | Build/install local Rhino 9 Yak package without publishing      |

## [03]-[MACHINE_CONTRACT]

[STDOUT]:
- Supervisor stdout carries exactly one `SessionEnvelope` JSON document.

[STDERR]:
- Supervisor stderr carries structured diagnostic JSON lines such as `session.terminal`.
- Stderr does not replace the stdout envelope.

[STATUS]:
- Supervisor usage errors exit `2`.

[READ_ORDER]:
1. Read top-level `status`.
2. Read `scenarioStatus` and `sessionStatus`.
3. Read `firstScenarioFailure` and `firstSessionFault`.
4. Read `fault.prescription` when `fault` is present.
5. Read `certificatePath`, then `bridge-certificate.json`.
6. Read `scenarios[]` for per-scenario verdicts.
7. Read `artifactRefs[]` and `evidenceCounts`.
8. Read `reportDir` only through certificate-listed artifacts.

[ARTIFACTS]:
- Certificate: `<reportDir>/bridge-certificate.json`.
- Scenario spool: `<reportDir>/events/<scenario>.jsonl`.
- Probe spool: `<reportDir>/events/probe.jsonl`.
- View captures: `<reportDir>/captures/<scenario>/<sequence>-<label>.png`.
- GH2 captures: `<reportDir>/gh2/<scenario>/<sequence>-<label>.png`.
- Manifests: `<reportDir>/manifests/<scenario>.*.json`.
- Scratch manifest: `<reportDir>/scratch/<scenario>/scratch.manifest.json`.
- Cargo stage: `<reportDir>/stage/<contentHash>/`.
- Endpoint: `~/.rasm/rhino-bridge-rbx.json`.
- Lease: `~/.rasm/rhino-bridge-rbx.lease`.
- Quit journal: `~/.rasm/rhino-bridge-quits.jsonl`.

## [04]-[ARCHITECTURE]

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Bridge session architecture
    accDescr: Supervisor launches Rhino, Rhino loads the stub and shell, the supervisor and shell exchange RPC, and the shell swaps cargo over typed scenarios.
    Supervisor --> Endpoint["~/.rasm/rhino-bridge-rbx.json"]
    Supervisor --> Rhino["Rhino 9"]
    Rhino --> Stub["Stub plugin"]
    Stub --> Shell["Shell ALC"]
    Supervisor <--> Pipe["Named pipe + StreamJsonRpc"]
    Pipe <--> Shell
    Shell --> Cargo["Cargo ALC"]
    Cargo --> Scenarios["Test assemblies with [RhinoScenario] methods"]
```

[OWNER_MAP]:
- `Supervisor`: process boundary, lease, bundle discovery, reconcile, launch, pipe client, staging, quit ladder, session fold.
- `Stub`: dependency-zero Rhino plugin loaded by Rhino's shared plugin context.
- `Shell`: in-host RPC target, endpoint writer, UI-thread marshal, busy admission, host-plugin preload, GH2/Rhino document cleanup.
- `Cargo`: dynamically loaded scenario runner, Rhino document acquisition, GH2 component-library load, capability probes, JSONL spool, capture writer, GH2 render lane.
- `Contract`: JSON-RPC interfaces, wire records, status algebra, faults, events, selections, and `SessionEnvelope`.

[PACKAGES]:
- `Contract`: `StreamJsonRpc` carries the RPC interfaces, and Thinktecture generates the wire vocabulary beside its JSON converters.
- `Shell`: `StreamJsonRpc` serves the named pipe; `Thinktecture.Runtime.Extensions` generates the shell-local vocabulary.
- `Supervisor`: `StreamJsonRpc` drives the pipe client, and `XxHash3` keys the isolated cargo stage.
- `Cargo` composes `Rasm.ScenarioKit`, the Contract closure, `LanguageExt.Core`, and `Thinktecture.Runtime.Extensions`; `Stub` references no package.

## [05]-[FAILURE_READING]

Terminal signals map to one first repair surface. Read `fault.detail`, `reportDir`, and spool artifacts from the same `SessionEnvelope`; when relayed events and durable spool counts diverge, the spool owns evidence through the last decoded JSONL line.

[REPAIR_SURFACES]:
- Lease: inspect or release `~/.rasm/rhino-bridge-rbx.lease`.
- Launch: check launch, endpoint liveness, and shell load evidence.
- Capability: read `fault.detail` for `capability-absent`, then change the scenario requirement or host lane.
- Host: rebuild closures against the active Rhino 9 bundle.
- UI: read spool tail, captures, and `host.exception` facts under `reportDir`.
- Crash: read `.ips` summary, spool JSONL, and captured artifacts.
- Evidence: use the spool as the durable source.

| [INDEX] | [SIGNAL]              | [READ_AS]                | [SURFACE]  |
| :-----: | :-------------------- | :----------------------- | :--------- |
|  [01]   | `busy`                | leased host              | Lease      |
|  [02]   | `poisoned endpoint`   | startup before endpoint  | Package    |
|  [03]   | `connect-failed`      | pipe admission           | Launch     |
|  [04]   | `shell-skew`          | shell contract skew      | Contract   |
|  [05]   | `capability-absent`   | failed required probe    | Capability |
|  [06]   | `host-drift`          | host API drift           | Host       |
|  [07]   | `ui-wedged`           | UI progress stall        | UI         |
|  [08]   | `rhino-crash`         | host exit                | Crash      |
|  [09]   | `evidence.divergence` | relay and spool mismatch | Evidence   |

## [06]-[SCENARIO_CONTRACT]

Typed scenario entrypoints carry `[RhinoScenario("<theme>")]`, accept one `ScenarioContext`, and return `Fin<Unit>`. `ScenarioContext` admits fact and capture labels through `EvidenceName`, brackets baseline-preserving Rhino or isolated GH2 document work through `WithRhinoDocument` and `WithGrasshopperDocument`, and returns report-scoped scratch files as `FileInfo`. `CaptureSnapshot` records the active Rhino view; `CaptureGrasshopper` records the isolated GH2 canvas.

Capability requirements live on the attribute as `Requires`. `CargoHost`'s capability statics are the register Cargo probes; a scenario whose required capability is not `ok` is rejected.

Cargo snapshots the pre-scenario Rhino and GH2 state, performs failure capture before cleanup, then removes only objects and documents created by the scenario. Cleanup counts and breaches are ordinary envelope facts as well as durable spool events; a baseline breach or residual object fails the scenario instead of being hidden by a blanket clear.

## [07]-[BOUNDARIES]

- Keep `Contract` additive: existing fields, union discriminators, status ranks, and exit codes are not renamed or reused.
- Keep `Stub` dependency-zero outside Rhino host assemblies.
- Keep host assemblies in the host/default ALC and bridge RPC assemblies in the shell ALC.
- Load cargo in one isolated collectible context per host session; request release, then recycle the host instead of forcing GC or retaining a second cargo context.
- Keep Rhino and GH2 document cleanup inside `PrepareQuitAsync` before the quit ladder runs.
- Keep the quit ladder to Apple Event terminate, Cocoa force terminate, then `kill(2)` SIGKILL.
