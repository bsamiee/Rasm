# [RASM_APPHOST_API_DIAGNOSTICS_CLIENT]

`Microsoft.Diagnostics.NETCore.Client` (dotnet/diagnostics) owns managed access to the .NET runtime diagnostics IPC endpoint: a pid-bound `DiagnosticsClient` captures a process dump and opens the EventPipe runtime-event stream. It is the sole owner of process-dump capture and the EventPipe stream, feeding the support-bundle capture fan on the observability rail and bounded to the host and companion process tree the AppHost owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Diagnostics.NETCore.Client`
- package: `Microsoft.Diagnostics.NETCore.Client` (`MIT`, dotnet/diagnostics)
- assembly: `Microsoft.Diagnostics.NETCore.Client`
- namespace: `Microsoft.Diagnostics.NETCore.Client`, `Microsoft.Diagnostics.NETCore.Client.WebSocketServer`
- target: `net8.0`
- depends: BCL only (`System.IO.Pipes` IPC over the runtime diagnostics socket)
- asset: runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and session surfaces

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]         | [CAPABILITY]                                                        |
| :-----: | :------------------------------ | :-------------------- | :------------------------------------------------------------------ |
|  [01]   | `DiagnosticsClient`             | sealed client         | one process's diagnostics endpoint — dump, EventPipe, resume        |
|  [02]   | `EventPipeSession`              | `IDisposable` session | live runtime-event stream (`EventStream`), `Stop`/`StopAsync`       |
|  [03]   | `EventPipeProvider`             | provider spec         | one ETW-style provider (`Name`/`EventLevel`/`Keywords`/`Arguments`) |
|  [04]   | `EventPipeSessionConfiguration` | session config        | provider set + circular buffer + rundown policy                     |

[PUBLIC_TYPE_SCOPE]: policy and fault surfaces

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [CAPABILITY]                                                                 |
| :-----: | :---------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `DumpType`                    | enum           | `Normal=1`/`WithHeap`/`Triage`/`Full` — dump completeness                    |
|  [02]   | `WriteDumpFlags`              | `[Flags]` enum | `None=0`/`LoggingEnabled=1`/`VerboseLoggingEnabled=2`/`CrashReportEnabled=4` |
|  [03]   | `PerfMapType`                 | enum           | perf-map generation kind for native-symbol resolution                        |
|  [04]   | `ServerNotAvailableException` | exception      | the target process endpoint is gone (mapped, never propagated)               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and process discovery

| [INDEX] | [SURFACE]                                   | [SHAPE] | [CAPABILITY]                           |
| :-----: | :------------------------------------------ | :------ | :------------------------------------- |
|  [01]   | `DiagnosticsClient(int)`                    | ctor    | bind the endpoint to one pid           |
|  [02]   | `DiagnosticsClient.GetPublishedProcesses()` | static  | enumerate locally diagnosable runtimes |

[ENTRYPOINT_SCOPE]: process-dump capture

`WriteDump` blocks while the runtime serializes a minidump to `dumpPath`; the async pair returns a `Task`.

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]        |
| :-----: | :---------------------------------------------------------------------------- | :------- | :------------------ |
|  [01]   | `WriteDump(DumpType, string, bool)`                                           | instance | logging flag        |
|  [02]   | `WriteDump(DumpType, string, WriteDumpFlags)`                                 | instance | flag set            |
|  [03]   | `WriteDumpAsync(DumpType, string, bool, CancellationToken) -> Task`           | instance | async, logging flag |
|  [04]   | `WriteDumpAsync(DumpType, string, WriteDumpFlags, CancellationToken) -> Task` | instance | async, flag set     |

[ENTRYPOINT_SCOPE]: EventPipe session and runtime control

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `StartEventPipeSession(EventPipeSessionConfiguration) -> EventPipeSession` | instance | open a live event stream         |
|  [02]   | `StartEventPipeSessionAsync(...) -> Task<EventPipeSession>`                | instance | open a live event stream (async) |
|  [03]   | `EventPipeSession.EventStream`                                             | property | forward-only nettrace `Stream`   |
|  [04]   | `EventPipeSession.Stop()`                                                  | instance | flush and close                  |
|  [05]   | `EventPipeSession.StopAsync() -> Task`                                     | instance | flush and close (async)          |
|  [06]   | `ResumeRuntime()`                                                          | instance | resume a suspended runtime       |
|  [07]   | `GetProcessEnvironment() -> Dictionary<string,string>`                     | instance | read the target's environment    |
|  [08]   | `SetEnvironmentVariable(string, string)`                                   | instance | set one target environment key   |
|  [09]   | `EnablePerfMap(PerfMapType)`                                               | instance | open perf-map emission           |
|  [10]   | `DisablePerfMap()`                                                         | instance | close perf-map emission          |

- `StartEventPipeSession`: one overload takes an `EventPipeProvider` set with a rundown flag and a circular-buffer bound, the other an `EventPipeSessionConfiguration`; the async form adds a `CancellationToken`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `DiagnosticsClient` targets one process's runtime diagnostics IPC endpoint — a Unix domain socket or named pipe published by the runtime; `GetPublishedProcesses` enumerates locally diagnosable runtimes. AppHost targets the host process itself, or a companion child whose pid the companion control host owns.
- `WriteDump` blocks while the runtime serializes the minidump: `Triage` redacts PII by construction, `Normal` is stacks-only, `WithHeap`/`Full` carry managed and native heap; `WriteDumpFlags.CrashReportEnabled` emits the companion crash-report JSON beside the dump. A missing endpoint throws `ServerNotAvailableException`, mapped at the boundary.
- `EventPipeSession.EventStream` is a forward-only nettrace `Stream` a parser consumes live; the stream drains on a dedicated pump, never the trigger thread, and the session is `IDisposable`. `circularBufferMB` bounds the in-runtime buffer so a slow consumer drops events rather than growing unbounded.

[STACKING]:
- `Microsoft.Diagnostics.Tracing.TraceEvent`(`.api/api-traceevent.md`): `EventPipeSession.EventStream` feeds `new EventPipeEventSource(stream).Process()` on a dedicated pump — the one capture→decode hand-off composing the `event-trace` artifact.
- `Microsoft.Diagnostics.Runtime`(`.api/api-diagnostics-runtime.md`): `WriteDump` writes the minidump `DataTarget.LoadDump` reads back for heap/thread triage — the capture→triage hand-off composing the `dump` + `dump-triage` receipt pair.
- capture fan (`.planning/Observability/bundles.md`): the `process-dump` `SupportArtifact` binds `DiagnosticsClient.WriteDump(DumpType, path, WriteDumpFlags)` under the window freeze; dump completeness rides `DumpPolicy` row data and capture faults map to `SupportFault` folded into `SupportReceipt.Partial`.

[LOCAL_ADMISSION]:
- Process-dump and EventPipe capture fill the support-bundle capture fan; the dump is one `SupportArtifact` under the window size cap, its path recorded in the `SupportReceipt` manifest, never streamed into telemetry.
- Dump completeness is `DumpPolicy` row data, never a call-site literal: `Triage`/`Normal` for routine capture, `WithHeap`/`Full` only under an explicit escalation trigger; `WriteDumpFlags` rides the same row.
- `DiagnosticsClient` is bounded to the host and companion process tree the AppHost owns, and capture rides `DeadlineClass` bounds so a stuck endpoint degrades one artifact row rather than wedging the pipeline.

[RAIL_LAW]:
- Package: `Microsoft.Diagnostics.NETCore.Client`
- Owns: process-dump capture and the EventPipe session stream for the host and companion process tree
- Accept: a pid-scoped `DiagnosticsClient`, dump completeness as row policy, and the `EventStream` drained by TraceEvent
- Reject: a hand-rolled `createdump` shell-out, an unbounded dump on an ordinary capture window, a general remote-process attach, or a thrown capture fault crossing the bundle pipeline
