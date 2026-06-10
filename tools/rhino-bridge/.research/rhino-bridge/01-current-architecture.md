# [01] Current Architecture — C# Tool Deep Map

Ground-truth map of `tools/rhino-bridge` as of 2026-06-10 (HEAD `ece2f0c0d`). Three projects, 2158 LOC of C# across 7 source files, driven by the Python operator (`tools/assay/rails/bridge.py`; legacy `tools/quality` still present side-by-side). All line references verified against working-tree source.

## [1]-[PROJECT_TOPOLOGY]

| [INDEX] | [PROJECT] | [ASSEMBLY] | [LOC] | [ROLE] |
| :-----: | --------- | ---------- | ----: | ------ |
| [1] | `protocol/` | `Rasm.RhinoBridge.Protocol.dll` | 538 | Wire DTOs, framing, status vocabulary, marker grammar, timeout policy, smoke template, atomic IO |
| [2] | `plugin/` | `rasm-bridge.rhp` | 644 | Rhino plugin: named-pipe server, UI-thread idle dispatch, RhinoCode compile/execute, GH2 pre-load |
| [3] | `client/` | `Rasm.RhinoBridge.Client` (exe) | 976 | CLI verbs, launch/quit lifecycle, MSBuild projection, scenario assembly, shadow staging, phase JSON |

Per-file responsibility:

| [FILE] | [SECTIONS] | [OWNS] |
| ------ | ---------- | ------ |
| `protocol/BridgeWire.cs` | PhaseStatus, BridgeMarker, BridgeEndpoint, request/reply/report records, BridgeFault, TimeoutPolicy, BridgeWire statics, PhaseStatusConverter, BoundaryIO | The entire wire contract plus shared constants; also the C#-source `SmokeTemplate` string (lines 427-489) and the LanguageExt bootstrap snippet (327-329) |
| `plugin/Rhino/Host.cs` | BridgeHostState, `Host : PlugIn`, 3 Rhino commands | Plugin lifecycle (`PlugInLoadTime.AtStartup`, start deferred to first `RhinoApp.Idle`), `RasmBridgeStart/Stop/Status` commands |
| `plugin/Rhino/Transport.cs` | BridgeCommand union (Hello/Doctor/Execute/Quit), BridgeServer | 4 async pipe accept loops, single-client gate, idle-queue UI dispatch, endpoint file write/heal/delete, command-window + `HostUtils.OnExceptionReport` capture, GH2 reflection snapshot, host-plugin pre-load |
| `plugin/Rhino/CodeEngine.cs` | CodeOutcome, CodeEngine | Sole `Rhino.Runtime.Code` boundary: language registration, `RunContext` (NeverCache, isolated resolver), compile-diagnostic extraction, GH2 `CompileReference.FromAssembly` injection |
| `client/Program.cs` | PhaseClassification, ScenarioLine, PhaseAggregate, BridgeResult, Program | All verbs' phase pipelines, launch/reconcile/force-quit, scenario assembly + reference staging, result JSON emission, exit-code mapping |
| `client/ClientVerb.cs` | CheckTarget `[Union]`, ClientVerb `[Union]`, CliOptions/CliInvocation | Argv parse into verb union (`doctor|launch|check|clean|quit`), extension-dispatched check target (`.csproj|.csx|.cs`), single option `--result` |
| `client/Build.cs` | ReferenceFile, ProjectBuild, SourceOwner(Evaluation), ProcessResult | MSBuild `-getProperty/-getItem` JSON parse, deps.json runtime-asset walk, source-ownership evaluation, bounded subprocess runner |

Dependency posture (lock files, all `net10.0` / `osx-arm64`):

| [PROJECT] | [DIRECT_PACKAGES] | [NOTE] |
| --------- | ----------------- | ------ |
| protocol | LanguageExt.Core 5.0.0-beta-77, Thinktecture.Runtime.Extensions 10.2.0, analyzers | Neither library is used by `BridgeWire.cs` itself — deliberately, so `Protocol.dll` ships dependency-free into Rhino (README §4.1 states the `.rhp` packages neither lib) |
| plugin | analyzers only | Host assemblies (RhinoCommon, Rhino.Runtime.Code, RhinoCodePlatform.Rhino3D) resolve via `Directory.Build.props` HintPaths under `$(RhinoWipResourcesPath)` with `Private=false`; `LangVersion` 14.0 repo-wide |
| client | LanguageExt.Core, Thinktecture (used: `Fin`, `[Union]`, `[SmartEnum]`), analyzers | Plain exe; invoked by Python via `dotnet run --no-build --project` |

## [2]-[WIRE_FORMAT]

- **Framing**: 4-byte little-endian length prefix + UTF-8 JSON body (`BridgeWire.WriteMessageAsync`/`ReadMessageAsync`, lines 372-397). Length outside `[0, 64 MiB]` throws. Clean EOF before any prefix byte yields `default` (treated as "no request").
- **Transport**: `NamedPipeServerStream`, `PipeOptions.Asynchronous | CurrentUserOnly`, byte mode, 4 server instances. Pipe name `rb-{pid}-{8-hex}`. One request frame, one reply frame, connection closed — no streaming, no multiplexing, no progress channel.
- **Message taxonomy**: exactly 4 commands — `hello`, `doctor`, `execute`, `quit` (`BridgeWire` lines 285-288; `BridgeCommand.Parse`, Transport 25-34). Request = `BridgeRequest(Command, TimeoutMs, Payload?)`. Reply = `BridgeReply(Command, Status, Data, Outputs, Diagnostics, Fault)`. Reports (`BridgeReport.Doctor/Execute/Quit`) ride inside `Data` as serialized JSON elements.
- **Status vocabulary**: `PhaseStatus` (ok/skipped/unsupported/failed/timeout/busy) carries wire string + severity rank + process exit code; aggregation = rank-max fold (`Worst`).
- **Side-channel grammar**: `BridgeMarker` — line-oriented stdout records prefixed `rasm.rhino-bridge.` with 4 variants (`return=`, `evidence=`, `capture=`, `nonce=`). This is a string protocol *inside* the JSON protocol: scenario evidence travels as parsed stdout lines, not typed wire fields. [FRAGILE: F1]
- **Endpoint discovery**: `~/.rasm/rhino-bridge.json` (`BridgeEndpoint`: pipe name, Rhino PID, process start time, assembly versions, Rhino version). Liveness = PID match + start-time within 1.0 s tolerance; staleness = pipe-name mismatch (guards PID recycling). Plugin self-heals the file on every `hello` (`EnsureEndpoint`).
- **Versioning posture**: NONE on the wire. No schema/version field exists in `BridgeRequest`, `BridgeReply`, or `BridgeResult` (verified: zero `schema` hits in C#). Client/plugin skew surfaces only as `JsonException` or silently-null missing fields. `BridgeEndpoint` carries assembly versions, but nothing checks them at connect time. The README documents a top-level `schema: rasm.rhino-bridge.v1` field that the code does not emit. [FRAGILE: F2]
- **Dead wire field**: `BridgeRequest.TimeoutMs` is populated by the client (default 15000; execute uses 35000) but never read anywhere in the plugin — server-side timeout is the fixed 300 s `IdleDispatch`; `ServerExecutionCancelable` is hard-coded `false` (Transport 254). The client cannot bound or cancel server execution. [FRAGILE: F3]

## [3]-[REQUEST_RESPONSE_LIFECYCLE]

Full `bridge verify` sequence (the dominant path), composed across Python → client → plugin:

1. **Python prelude** (`tools/assay/rails/bridge.py`): acquire process-global `bridge` lease → create report dir → `_build_closure` builds protocol → plugin → client → testkit in order (so the isolated resolver later finds `Scenario.Run`/`FactBag`) → `client_run("launch")`. Per scenario: `dotnet run --no-build --project client check <owner.csproj> <scenario.verify.csx> --result <path>` with a 180 s subprocess timeout.
2. **Client `check` (project target)**: `resolve` phase (trivial for `.csproj`; for `.cs` it runs `dotnet msbuild -getItem:Compile` against EVERY git-tracked csproj in parallel, 45 s each) → `build` phase = 3 sequential subprocesses: `dotnet restore --locked-mode`, `dotnet build --no-restore`, `dotnet msbuild -target:ResolveReferences -getProperty:... -getItem:...` parsed from stdout JSON (`ProjectBuild.Parse`). NU1004/NU1403 in output is reclassified as `nuget-lock-drift` rather than a compile error.
3. **`launch` phase**: try `hello` (3 s). If it fails: require `RHINO_WIP_APP_PATH`, run `ReconcileAsync` (force-close recorded PID via JXA `NSRunningApplication.terminate`, SIGKILL fallback, retire endpoint file, delete macOS autosave `.rhl`/`.3dm` and `Rhinoceros-*.ips` crash reports), then `open <app> --args -nosplash`.
4. **`connect` phase**: poll `hello` every 250 ms until the 90 s connect deadline.
5. **Scenario assembly** (client, in-memory): reject embedded `#r`/`#load`; classify lines via `ScenarioLine` SmartEnum (blank/comment/using/directive = preamble; first other line starts body); emit `#r` directives for shadow-staged references + authored preamble + `ScenarioBaseUsings` (+ `Eto.Drawing` if GH-aware) + `SCENARIO_NAME`/`CAPTURE_PATH` consts + LanguageExt `HashMap` bootstrap + `// --- [SCENARIO_BODY] ---` + body. References are shadow-copied to `refs/<sha256-32-hex>/` (collision dirs `00/`, `01/`...) ordered by `ReferenceLoadOrder` (FSharp.Core 0 → LanguageExt 10 → Thinktecture 20 → transitive 100 → Rasm 900 → TestKit/Protocol 910 → target 1000). Script staged to disk; `BridgeExecuteRequest(Script, ScriptPath, References, HostPlugins)` sent over the pipe (35 s transport timeout).
6. **Plugin transport**: accept loop hands the pipe to `HandlePipeAsync`; `clientGate` (SemaphoreSlim 1) → immediate `busy` reply if held; 2 s handshake deadline to read the request frame; `BridgeCommand.Parse` → `ExecuteOnRhinoAsync` enqueues an `IdleJob` onto `ConcurrentQueue` and awaits a `TaskCompletionSource` with the 300 s `IdleDispatch` timeout.
7. **UI-thread execution** (next `RhinoApp.Idle` pulse, one job per pulse): `EnsureHostPlugins` pre-loads GH2 by GUID into the default ALC (`PlugIn.LoadPlugIn`) → snapshot command history, enable `CommandWindowCaptureEnabled`, subscribe `HostUtils.OnExceptionReport` → `CodeEngine.Run`: `RhinoCode.CreateCode(uri|text)`, add already-loaded GH2 assemblies as `CompileReference.FromAssembly`, `RunScript` under `RunContext { CachePolicy = NeverCache, Options["csharp.resolver.isolate"] = true }` (scenario `#r` references load in RhinoCode's collectible Roslyn context; scenarios compile at the RhinoCode-pinned **C# 10** ceiling) → scan stdout for the last `return=` marker → unsubscribe, restore capture flag → swallowed `OnExceptionReport` faults flip an otherwise-ok status to `failed` (Transport 246).
8. **Reply + post-phases**: reply frame written; client wraps it as the `execute` phase → `liveness` phase re-`hello`s the host (after a 4 s settle sleep for GH-aware scenarios, catching deferred AppKit crashes) → `diagnostics` phase republishes RhinoCode compile diagnostics → `BridgeResult.From` folds decisive phases (lifecycle phases exempt) → pretty JSON written atomically to `--result` and echoed to stdout; exit code from worst status.
9. **Python decode**: regexes pull `evidence=facts=` and `capture=` markers out of the *truncated* execute-phase stdout in the result JSON, fold scenario receipts into the Assay envelope.

`doctor` = launch + connect + doctor request (host/runtime/assembly report, executed on the UI thread). `quit` = best-effort `quit` request (marks docs clean only — the plugin never calls `RhinoApp.Exit`, which self-SIGABRTs from an idle frame) then client-owned JXA terminate → SIGKILL → marker cleanup.

## [4]-[THREADING_MODEL]

| [CONTEXT] | [RUNS] |
| --------- | ------ |
| Thread pool (4 accept loops) | Pipe accept, frame read/write, handshake timeout, busy rejection |
| `RhinoApp.Idle` (UI thread) | ALL command dispatch: doctor, execute (scenario compile + run), quit doc-clean. One `IdleJob` dequeued per Idle pulse (`DrainIdleQueue`, Transport 124-135) |
| Client process | Everything else: builds, staging, launch, lifecycle, liveness |

Consequences: scenarios are inherently UI-thread synchronous — no server-side cancellation, no concurrent scenarios, and a scenario that blocks the UI thread wedges the host until the 300 s dispatch timeout abandons the await (the work itself keeps running; only the wait is abandoned). Dispatch latency depends on Idle pulse cadence. `OnMain` (Transport 344-357) enqueues fire-and-forget jobs whose completion source is never awaited.

## [5]-[CONSTANTS_TIMEOUTS_PATHS]

| [KIND] | [VALUE] | [WHERE] |
| ------ | ------- | ------- |
| Hello / Handshake | 3 s / 2 s | `TimeoutPolicy` (protocol 262-280); all scale via `RASM_BRIDGE_TIMEOUT_SCALE` only |
| Connect / Transport | 90 s / 35 s | same |
| QuitWait / IdleDispatch / LivenessSettle | 30 s / 300 s / 4 s | same |
| Connect poll interval | 250 ms | Program 339 |
| Accept-loop error backoff | 100 ms | Transport 149 |
| Client subprocess timeouts | dotnet steps 5 min; git 30 s; osascript 15 s; `open` 30 s; per-project owner eval 45 s | Program/Build |
| Python scenario timeout | 180 s | `bridge.py` `_SCENARIO_TIMEOUT_S` |
| Output truncation | stdout/stderr/rhino.command 32768 chars; process outputs 16384 | `OutputLimit`/`ProcessOutputLimit` |
| Max frame | 64 MiB | `MaxFrameBytes` |
| Pipe | `rb-{pid}-{8hex}`, 4 instances, name ≤ 64 chars validated client-side | Transport 58, 81-83; Program 651 |
| Endpoint file | `~/.rasm/rhino-bridge.json`, user-only perms | `BridgeWire.EndpointPath` |
| Endpoint liveness tolerance | 1.0 s process-start-time delta | `BridgeEndpoint.StartTimeToleranceSeconds` |
| GH2 plugin GUID | `8307876d-a461-4daa-bb77-eb3715925513` | `BridgeWire.GrasshopperPluginId` |
| Report root / stage dirs | `.artifacts/rhino/bridge/check/...`, `execute-{pid}-{ms}/`, `refs/<32-hex>/` | Program 393, 596, 683 |
| Crash markers deleted | `~/Library/Autosave Information/Unsaved RhinoWIP Document.3dm{,.rhl}`, `~/Library/Logs/DiagnosticReports/Rhinoceros-*.ips` | Program 260-281 |
| Env | `RHINO_WIP_APP_PATH`, `CONFIGURATION` (default Release), `RASM_BRIDGE_TIMEOUT_SCALE` | Program 96, 321; protocol 276 |
| Host assembly deny-list | 13 names (`HostAssemblyNames`) + collision watch set (FSharp.Core, LanguageExt.Core, Thinktecture) | protocol 303-317, 340-344 |
| Reference load order | table 0/10/20/100/900/910/1000 | protocol 345-361 |

## [6]-[FRAGILITY_ANNOTATIONS]

| [ID] | [SITE] | [BRITTLENESS] |
| ---- | ------ | ------------- |
| F1 | `BridgeMarker` + `bridge.py` regexes | Evidence/captures/return values are stdout-line string contracts parsed twice (C# `Scan`, Python regex). Markers beyond the 32 KiB `OutputLimit` truncation are silently severed — a chatty scenario loses its facts with `truncated: true` as the only signal. `CodeEngine` scans the un-truncated stdout for `return=` (server-side), but Python reads facts from the truncated client-side copy: asymmetric loss. |
| F2 | README §4 vs `BridgeResult` | Documented `schema: rasm.rhino-bridge.v1` field does not exist in code; documented per-name `RASM_BRIDGE_<NAME>_TIMEOUT_S` env knobs replaced by a single `RASM_BRIDGE_TIMEOUT_SCALE`; documented `--timeout-ms` option never implemented (only `--result` parses). Doc drift on the tool's own contract surface. |
| F3 | `BridgeRequest.TimeoutMs`, `ServerExecutionCancelable: false` | Client-declared timeout is dead on arrival; no cancellation channel; a runaway scenario holds the UI thread and the client gate for up to 300 s while the client already gave up at 35 s — the next invocation then sees `busy`. |
| F4 | Layered timeout mismatch | Python 180 s kills the client mid-`build` (dotnet steps allow 3 × 5 min) or mid-`connect` (90 s) on cold caches; client transport 35 s < server dispatch 300 s. Three uncoordinated timeout authorities. |
| F5 | `ConnectPhaseAsync` (Program 335-350) | 250 ms hello polling against the endpoint file for up to 90 s — no readiness signal from the plugin (e.g. no fs-watch, no pipe-creation notification); cold launch cost is pure poll. |
| F6 | `LivenessPhaseAsync` (Program 375-391) | Fixed 4 s `Task.Delay` heuristic for the GH2 deferred-paint crash window; a crash at 5 s passes liveness and surfaces as a confusing relaunch next run. |
| F7 | `ReconcileAsync`/`ClearRecoveryMarkers`/`ForceKill` (Program 235-313) | Quit is a JXA-string → SIGKILL escalation plus destructive deletion of OS crash reports and autosave files; `ForceKill` returns `true` on any exception (absent process and permission failure are indistinguishable); `RetireEndpoint`/`TryDelete` swallow all non-fatal errors. This is the package-update relaunch path the operator experiences as forced-quit/recovery-dialog roulette. |
| F8 | `CommandWindowCapture` (Transport 293-301) | Fallback diffing of `CommandHistoryWindowText` by string-prefix; interleaved output or history rotation silently yields `[]`. |
| F9 | `ReflectGrasshopperSolution` (Transport 303-335) | GH2 solution state via 5-hop reflection chain; any shape drift returns `null`/partial silently — diagnostics quietly degrade. Warning/error counts are already hard-coded `null`. |
| F10 | `ScenarioLine` + `SourceScenarioScriptAsync` (Program 503-530) | Line-classifier preamble hoisting plus injected directives/consts/bootstrap shifts all body line numbers — RhinoCode compile diagnostics point at the staged script, not the authored `.verify.csx`; no source map. A major driver of "scenario authoring is finicky". |
| F11 | `BridgeWire.LanguageExtBootstrap` (protocol 327-329) | HashMap trait-warmup string injected into every scenario to appease the isolated resolver; encodes a known LanguageExt-under-collectible-ALC failure as a permanent textual workaround (custom record-struct HashMap keys still fail). |
| F12 | `BridgeWire.SmokeTemplate` (protocol 427-489) | 60-line C# program built by string concatenation with manual escaping inside the protocol assembly — string-typed codegen with zero compile checking. |
| F13 | `ResolveSourcePhaseAsync` (Program 441-467) | `.cs` ownership = parallel `dotnet msbuild -getItem:Compile` over every tracked csproj; O(N) MSBuild evaluations per check, 45 s ceiling each. |
| F14 | `BuildPhaseAsync` (Program 399-431) | Every project check pays 3 fresh dotnet subprocesses; no warm server, no artifact-path stabilization (contrast: assay `static build` learned this per `reference_quality_gate_cold_restore`). |
| F15 | `Host.StartOnIdle` (Host 23-29) | Startup failure is a single `RhinoApp.WriteLine` — no endpoint file, no retry, no client-visible distinction from "plugin not installed"; client just polls to the 90 s deadline. |
| F16 | `AcceptLoopAsync` (Transport 136-152) | Broad catch (IOException…ArgumentOutOfRangeException) → log + 100 ms sleep + retry forever; a persistent pipe fault becomes an infinite warm loop with `lastError` only surfaced if someone calls `State()`. |
| F17 | `ProjectBuild.IsGrasshopperAware` (Build 29-31) | GH-awareness = MSBuild prop OR `Grasshopper2.dll` filename sniff — duplicated heuristics deciding usings, host plugin pre-load, and liveness settle. |
| F18 | Update cycle | Plugin ships as Yak package loaded `AtStartup` into Rhino's default plugin context: any plugin/protocol change requires stage → deploy → full Rhino quit/relaunch (through F7). No hot-reload/ALC-swap path exists for the bridge itself; only scenario code is collectible. |

## [7]-[GIT_HISTORY_FIX_ACCUMULATION]

59 commits touch `tools/rhino-bridge`, all within 2026-05-16 → 2026-06-08 (~3.5 weeks). Clusters:

| [WINDOW] | [THEME] |
| -------- | ------- |
| 05-16/17 (12 commits) | Initial probe runtime, repeated "refine/clean/fix" of check flow, diagnostics, truth checks |
| 05-21/24 | Yak packaging, install simplification, testing-rail integration |
| 05-25/26 (8 commits) | Event-driven Idle dispatch rewrite, scenario rail, deploy hardening, command capture fix, GH2 compile-surface fix |
| 05-28/29 | Mega-file split, force-quit + launch hardening, GH2 robust load (StatusBar crash era) |
| 05-31 → 06-08 | Quality-rail consolidation, assay migration, docs churn |

Pattern matches the operator's "ad-hoc minor fixes keep accumulating": three lifecycle hardening passes (quit/launch/recovery), two dispatch redesigns, and per-incident workarounds (LanguageExt bootstrap, liveness settle, lock-drift classification) layered onto a stable 4-command wire.

## [8]-[SUMMARY_JUDGMENT_INPUTS]

- The wire core (framing, status algebra, endpoint handshake) is small and sound; the fragility concentrates in (a) the stdout-marker side channel, (b) the launch/quit/recovery lifecycle owned by the client, (c) string-assembled scenario codegen with no source mapping, and (d) the absence of any wire/protocol versioning while the plugin requires a full Rhino relaunch to update.
- Linkage direction today: Python assay owns lease/build-closure/launch sequencing and re-parses client JSON + stdout markers; the C# client independently owns launch/quit/reconcile — lifecycle authority is split across two layers that both shell out to each other's domains.
- The C# 10 scenario ceiling is a RhinoCode pin (`CodeEngine.cs` line 31), not a bridge choice; any rebuild keeping RhinoCode inherits it.
