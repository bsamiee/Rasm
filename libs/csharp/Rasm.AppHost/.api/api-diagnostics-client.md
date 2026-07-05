# [RASM_APPHOST_API_DIAGNOSTICS_CLIENT]

`Microsoft.Diagnostics.NETCore.Client` (dotnet/diagnostics) is the managed client for the .NET runtime diagnostics IPC endpoint: a `DiagnosticsClient` bound to a process id writes a process dump (`WriteDump(DumpType, path)`), starts an `EventPipeSession` streaming runtime events, resumes a suspended runtime, and enumerates diagnosable processes — the one categorical owner of process-dump capture and the EventPipe stream the support-bundle capture fan admits behind the `[DUMP_ADMISSION]` gate, never a hand-rolled `createdump` shell-out.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Diagnostics.NETCore.Client`
- package: `Microsoft.Diagnostics.NETCore.Client`
- version: `0.2.661903`
- license: `MIT`
- assembly: `Microsoft.Diagnostics.NETCore.Client`
- namespace: `Microsoft.Diagnostics.NETCore.Client`
- namespace: `Microsoft.Diagnostics.NETCore.Client.WebSocketServer`
- target: `net8.0`; 83 types across 2 namespaces
- dependency floor: BCL only (`System.IO.Pipes` IPC over the runtime diagnostics socket)
- asset: runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and session surfaces
- rail: observability

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [RAIL]                                                     |
| :-----: | :------------------------------ | :------------ | :--------------------------------------------------------- |
|  [01]   | `DiagnosticsClient`             | sealed client | one process's diagnostics endpoint — dump, EventPipe, resume |
|  [02]   | `EventPipeSession`              | `IDisposable` session | live runtime-event stream (`EventStream`), `Stop`/`StopAsync` |
|  [03]   | `EventPipeProvider`             | provider spec | one ETW-style provider (`Name`/`EventLevel`/`Keywords`/`Arguments`) |
|  [04]   | `EventPipeSessionConfiguration` | session config | provider set + circular buffer + rundown policy            |

[PUBLIC_TYPE_SCOPE]: policy and fault surfaces
- rail: observability

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [RAIL]                                                          |
| :-----: | :----------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `DumpType`                     | enum          | `Normal=1`/`WithHeap`/`Triage`/`Full` — dump completeness       |
|  [02]   | `WriteDumpFlags`               | `[Flags]` enum | `None=0`/`LoggingEnabled=1`/`VerboseLoggingEnabled=2`/`CrashReportEnabled=4` |
|  [03]   | `PerfMapType`                  | enum          | perf-map generation kind for native-symbol resolution          |
|  [04]   | `ServerNotAvailableException`  | exception     | the target process endpoint is gone (mapped, never propagated)  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and process discovery
- rail: observability

| [INDEX] | [MEMBER]                                              | [KIND]      | [RETURN]              |
| :-----: | :--------------------------------------------------- | :---------- | :-------------------- |
|  [01]   | `DiagnosticsClient(int processId)`                   | ctor        | `DiagnosticsClient`   |
|  [02]   | `DiagnosticsClient.GetPublishedProcesses()`          | static call | `IEnumerable<int>`    |

[ENTRYPOINT_SCOPE]: process-dump capture
- rail: observability

| [INDEX] | [MEMBER]                                                                      | [KIND] | [RETURN] |
| :-----: | :--------------------------------------------------------------------------- | :----- | :------- |
|  [01]   | `WriteDump(DumpType dumpType, string dumpPath, bool logDumpGeneration = false)` | call   | `void`   |
|  [02]   | `WriteDump(DumpType dumpType, string dumpPath, WriteDumpFlags flags)`         | call   | `void`   |
|  [03]   | `WriteDumpAsync(DumpType dumpType, string dumpPath, bool logDumpGeneration, CancellationToken token)` | call | `Task` |
|  [04]   | `WriteDumpAsync(DumpType dumpType, string dumpPath, WriteDumpFlags flags, CancellationToken token)` | call | `Task` |

[ENTRYPOINT_SCOPE]: EventPipe session and runtime control
- rail: observability

| [INDEX] | [MEMBER]                                                                                            | [KIND]   | [RETURN]                  |
| :-----: | :------------------------------------------------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `StartEventPipeSession(IEnumerable<EventPipeProvider> providers, bool requestRundown = true, int circularBufferMB = 256)` | call | `EventPipeSession`        |
|  [02]   | `StartEventPipeSession(EventPipeSessionConfiguration config)`                                       | call     | `EventPipeSession`        |
|  [03]   | `StartEventPipeSessionAsync(IEnumerable<EventPipeProvider> providers, bool requestRundown, int circularBufferMB = 256, CancellationToken token = default)` | call | `Task<EventPipeSession>`  |
|  [04]   | `EventPipeSession.EventStream`                                                                      | property | `Stream` — the live nettrace stream |
|  [05]   | `EventPipeSession.Stop()` / `StopAsync(CancellationToken)`                                          | call     | `void` / `Task`           |
|  [06]   | `ResumeRuntime()`                                                                                   | call     | `void` — un-suspend a diagnostics-startup-paused runtime |
|  [07]   | `GetProcessEnvironment()` / `SetEnvironmentVariable(string, string)`                                | call     | `Dictionary<string,string>` / `void` |
|  [08]   | `EnablePerfMap(PerfMapType)` / `DisablePerfMap()`                                                   | call     | `void`                    |

## [04]-[IMPLEMENTATION_LAW]

[DIAGNOSTICS_TOPOLOGY]:
- `DiagnosticsClient(int processId)` targets one process's runtime diagnostics IPC endpoint (a Unix domain socket / named pipe published by the runtime); `GetPublishedProcesses()` enumerates every locally diagnosable runtime. The AppHost target is the host process itself (self-dump) or a companion child whose pid the companion control host owns.
- `WriteDump` blocks while the runtime serializes a minidump to `dumpPath`; `DumpType.WithHeap`/`Full` include managed+native heap and are large, `Triage` redacts PII by construction, `Normal` is stacks-only. `WriteDumpFlags.CrashReportEnabled` emits the companion crash-report JSON beside the dump. A missing endpoint throws `ServerNotAvailableException` (the process exited), mapped at the boundary, never propagated.
- `StartEventPipeSession` opens a streaming session; `EventPipeSession.EventStream` is a forward-only `Stream` in the nettrace format that a parser (`api-traceevent.md` `EventPipeEventSource`) consumes live. `Stop()`/`StopAsync()` flush and close; the session is `IDisposable` and the stream must be drained on a dedicated pump, never the trigger thread.
- `EventPipeProvider(name, EventLevel, keywords, arguments)` names one provider (e.g. `Microsoft-DotNETCore-SampleProfiler` for CPU, `Microsoft-Windows-DotNETRuntime` with GC keywords); `circularBufferMB` bounds the in-runtime buffer so a slow consumer drops events rather than growing unbounded.

[LOCAL_ADMISSION]:
- Process-dump and EventPipe capture are the `[DUMP_ADMISSION]` fill for the support-bundle capture fan: the `process-dump` artifact row (`Observability/bundles#CAPTURE_PIPELINE`) adapts `DiagnosticsClient.WriteDump` behind the fan's caps/redaction/truncation law — the dump file is one `SupportArtifact` written under the window's size cap, its path recorded in the `SupportReceipt` manifest, never streamed into telemetry.
- The dump completeness is policy DATA on the artifact row, not a call-site literal: a `Triage`/`Normal` dump for routine capture, `WithHeap`/`Full` only under an explicit escalation trigger, so a heavyweight full-heap dump never rides an ordinary capture window; `WriteDumpFlags` (logging/crash-report) is the same row's flag column.
- `WriteDump` and `EventPipeSession` faults (`ServerNotAvailableException`, IO) map to the capture fan's typed `SupportFault` (registry band 4810), folded into `SupportReceipt.Partial` for the failed artifact — never a thrown exception aborting the whole bundle window, and never a bare `Error.New`.
- The client is bounded to the same host/companion process tree the AppHost owns; it is never a general remote-process attach surface, and dump/EventPipe capture rides `DeadlineClass` bounds so a stuck runtime endpoint degrades one artifact row rather than wedging the capture pipeline.

[STACK]:
- capture fan: `Observability/bundles#CAPTURE_PIPELINE` binds the `process-dump` `SupportArtifact` row to `DiagnosticsClient.WriteDump(dumpType, path, flags)`; the ordered fan-in writes the dump under the window freeze, redaction runs on the manifest metadata (paths, pid), and the artifact is capped by the row's size policy.
- eventpipe → traceevent seam: `EventPipeSession.EventStream` (`Stream`) is consumed by `new EventPipeEventSource(stream).Process()` (`api-traceevent.md`) on a dedicated pump — the single verified hand-off between dump/session capture (this package) and event decoding (TraceEvent); the two catalogs compose one dump+trace row, never two capture paths.
- fault band: capture failures land as the typed `SupportFault` case in registry band 4810 (`Runtime/lifecycle#FAULT_TABLES`) — the same band that retires the `bundles.md:90` orphan `Error.New(9300)`.
- manifest receipt: every dump artifact is one `SupportReceipt` manifest row (`Observability/bundles#MANIFEST_RECEIPT`) with content hash composed through the kernel `Rasm.Domain.ContentHash.Of` entry (`[V18]`), size, and truncation flag — never a raw file path leaked to the wire.

[RAIL_LAW]:
- Package: `Microsoft.Diagnostics.NETCore.Client`
- Owns: process-dump capture and the EventPipe session stream for the host/companion process tree
- Accept: a pid-scoped `DiagnosticsClient`, dump completeness as row policy, and the `EventStream` drained by TraceEvent
- Reject: a hand-rolled `createdump` shell-out, an unbounded dump on an ordinary capture window, a general remote-process attach, or a thrown capture fault crossing the bundle pipeline
