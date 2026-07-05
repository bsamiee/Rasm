# [RASM_APPHOST_API_TRACEEVENT]

`Microsoft.Diagnostics.Tracing.TraceEvent` (perfview) is the managed ETW/EventPipe decode engine: an `EventPipeEventSource` (or `ETWTraceEventSource`) reads a runtime-event stream and dispatches strongly-typed `TraceEvent` records through provider parsers (`Process()` drives the pump), and `TraceLog` post-processes a stream into an indexed `.etlx` with resolved call stacks. It is the one categorical decoder for the nettrace `EventStream` the `api-diagnostics-client.md` `EventPipeSession` produces — TraceEvent turns raw runtime events into typed CPU/GC/exception/allocation records for the support-bundle event artifact.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Diagnostics.Tracing.TraceEvent`
- package: `Microsoft.Diagnostics.Tracing.TraceEvent`
- version: `3.2.4`
- license: `MIT`
- assembly: `Microsoft.Diagnostics.Tracing.TraceEvent`
- namespace: `Microsoft.Diagnostics.Tracing` (source/session), `Microsoft.Diagnostics.Tracing.Etlx` (TraceLog), `Microsoft.Diagnostics.Tracing.Parsers` (provider parsers), `Microsoft.Diagnostics.Tracing.Session` (live ETW)
- target: `netstandard2.0`; 1981 types across 50 namespaces
- dependency floor: `Microsoft.Diagnostics.FastSerialization` (`IFastSerializable` stream format), `Microsoft.Diagnostics.NETCore.Client` (the EventPipe session producer — this folder pins `0.2.661903`, which supersedes the transitive floor), `Dia2Lib`/`TraceReloggerLib` (native-symbol/relogger interop, Windows)
- asset: runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: source and dispatch surfaces
- rail: observability

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]      | [RAIL]                                                     |
| :-----: | :------------------------------------------------------ | :----------------- | :-------------------------------------------------------- |
|  [01]   | `Microsoft.Diagnostics.Tracing.TraceEventDispatcher`    | abstract base      | the event pump — `Process()` and parser-callback dispatch |
|  [02]   | `Microsoft.Diagnostics.Tracing.EventPipeEventSource`    | source             | reads a nettrace stream/file (the EventPipe decode entry) |
|  [03]   | `Microsoft.Diagnostics.Tracing.ETWTraceEventSource`     | source             | reads a Windows ETW `.etl` file                           |
|  [04]   | `Microsoft.Diagnostics.Tracing.Etlx.TraceLog`           | indexed log        | post-processed `.etlx` with resolved call stacks          |
|  [05]   | `Microsoft.Diagnostics.Tracing.Session.TraceEventSession` | live ETW session (Windows) | `EnableProvider` + real-time `Source` (kernel/user providers) |

[PUBLIC_TYPE_SCOPE]: parser surfaces
- rail: observability

| [INDEX] | [SYMBOL]                                                          | [TYPE_FAMILY] | [RAIL]                                                    |
| :-----: | :--------------------------------------------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `Microsoft.Diagnostics.Tracing.Parsers.ClrTraceEventParser`      | parser        | CLR GC/exception/JIT/allocation typed events             |
|  [02]   | `Microsoft.Diagnostics.Tracing.Parsers.SampleProfilerTraceEventParser` | parser  | CPU sample-profiler stacks (the profiler EventPipe provider) |
|  [03]   | `Microsoft.Diagnostics.Tracing.Parsers.DynamicTraceEventParser`  | parser        | manifest/EventSource-declared providers, resolved dynamically |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stream decode (EventPipe — the AppHost path)
- rail: observability

| [INDEX] | [MEMBER]                                                | [KIND]   | [RETURN]              |
| :-----: | :------------------------------------------------------ | :------- | :-------------------- |
|  [01]   | `new EventPipeEventSource(Stream stream)`               | ctor     | `EventPipeEventSource` — decode a live `EventPipeSession.EventStream` |
|  [02]   | `new EventPipeEventSource(string fileName)`             | ctor     | `EventPipeEventSource` — decode a captured `.nettrace` file |
|  [03]   | `EventPipeEventSource.Clr` / `.Dynamic` / `.Kernel`     | property | the built-in parser instances for callback subscription |
|  [04]   | `TraceEventDispatcher.Process()`                        | call     | `bool` — pump the stream to completion, firing callbacks |
|  [05]   | `EventPipeEventSource.HeaderKeyValuePairs`              | property | `Dictionary<string,string>` — trace header metadata     |
|  [06]   | `EventPipeEventSource.Dispose()`                        | resource | `void`                |

[ENTRYPOINT_SCOPE]: post-processing and live ETW (Windows)
- rail: observability

| [INDEX] | [MEMBER]                                                          | [KIND]      | [RETURN]              |
| :-----: | :--------------------------------------------------------------- | :---------- | :-------------------- |
|  [01]   | `TraceLog.OpenOrConvert(string etlxOrEtlFilePath)`               | static call | `TraceLog` — indexed log with call stacks |
|  [02]   | `TraceEventSession(string sessionName)` + `EnableProvider(...)` + `Source`  | ctor/call   | live real-time ETW dispatch (Windows only) |

## [04]-[IMPLEMENTATION_LAW]

[TRACEEVENT_TOPOLOGY]:
- the decode idiom is subscribe-then-pump: construct a `TraceEventDispatcher` (`EventPipeEventSource` for nettrace, `ETWTraceEventSource` for `.etl`), register typed callbacks on a parser (`source.Clr.GCHeapStats += e => ...`, `source.Dynamic.All += ...`), then `source.Process()` drives the stream to EOF firing every callback synchronously on the pump thread; the `TraceEvent` record passed to a callback is REUSED per event, so any retained field must be copied (`e.Clone()`).
- `EventPipeEventSource` supports nettrace format versions 3-6; it is cross-platform (netstandard2.0) and is the correct decoder for the `EventPipeSession.EventStream` on macOS/Linux/Windows. `ETWTraceEventSource`/`TraceEventSession` are Windows-only ETW paths; the AppHost host-neutral spine uses the EventPipe path and treats live ETW as a Windows-host capture option only.
- `TraceLog.OpenOrConvert` builds the indexed `.etlx` (call stacks resolved, events time-ordered) when stack-attributed analysis is needed; the streaming `EventPipeEventSource.Process()` path is the low-overhead default for a bounded capture window.
- GCDUMP BOUNDARY: this 3.2.4 assembly ships NO managed-heap-graph reader — `DotNetHeapDumpGraphReader`, `GCHeapDump`, and `MemoryGraph` are absent from every shipped assembly (verified against the restored package). The `.gcdump` graph format has no owner in this catalog; a heap-object-graph artifact routes to its own maintained owner (the `dotnet-gcdump` tool), never a phantom type asserted here. This package owns event-STREAM decode, not heap-GRAPH capture.

[LOCAL_ADMISSION]:
- TraceEvent is the decode half of the support-bundle event artifact: `api-diagnostics-client.md` `DiagnosticsClient.StartEventPipeSession` produces the `EventStream`, and `new EventPipeEventSource(stream)` here decodes it; the two compose one `event-trace` `SupportArtifact` row (`Observability/bundles#CAPTURE_PIPELINE`), never two capture paths.
- the parser/provider selection (CPU sample profiler, GC keywords, exceptions) is policy DATA on the artifact row — the `EventPipeProvider` set passed to `StartEventPipeSession` and the callbacks subscribed here derive from the same row, so a capture profile is one data decision, never a call-site literal.
- decode runs on a dedicated pump inside the capture window's `DeadlineClass` bound; a malformed or truncated stream ends `Process()` with the partial events already dispatched, folded to `SupportReceipt.Partial` — never a thrown decode exception aborting the bundle, and never a bare `Error.New`.
- the decoded event summary (GC pause histogram, allocation top-N, exception counts) is written under the redaction/truncation law before it enters the manifest; raw event payloads never cross the wire un-redacted.

[STACK]:
- eventpipe seam: `api-diagnostics-client.md` `EventPipeSession.EventStream` → `new EventPipeEventSource(stream)` → parser callbacks → `Process()` is the single verified capture→decode hand-off; the `event-trace` artifact row binds both catalogs.
- capture fan: `Observability/bundles#CAPTURE_PIPELINE` owns the artifact row, its caps, and redaction; TraceEvent contributes the decode+summarize step, not a second capture surface.
- fault band: decode failures land as the typed `SupportFault` case in registry band 4810 (`Runtime/lifecycle#FAULT_TABLES`), the same band as the dump-capture faults.
- manifest receipt: the decoded summary artifact is one `SupportReceipt` row (`Observability/bundles#MANIFEST_RECEIPT`) content-hashed through the kernel `Rasm.Domain.ContentHash.Of` entry (`[V18]`).

[RAIL_LAW]:
- Package: `Microsoft.Diagnostics.Tracing.TraceEvent`
- Owns: ETW/EventPipe event-STREAM decode into typed `TraceEvent` records for the support-bundle event artifact
- Accept: the `EventPipeSession.EventStream`, a policy-driven parser/provider set, and a bounded pump inside the capture window
- Reject: a `.gcdump` heap-GRAPH claim (no such type in 3.2.4 — routes to `dotnet-gcdump`), a retained un-cloned `TraceEvent` record, an unbounded live ETW session on the host-neutral path, or a thrown decode fault crossing the bundle pipeline
