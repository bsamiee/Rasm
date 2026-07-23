# [RASM_APPHOST_API_TRACEEVENT]

`Microsoft.Diagnostics.Tracing.TraceEvent` (perfview) owns managed ETW/EventPipe stream decode: an `EventPipeEventSource` reads a runtime-event stream and dispatches strongly-typed `TraceEvent` records through provider parsers under `Process()`, and `TraceLog` post-processes a stream into a stack-resolved `.etlx` index. It is the sole decoder for the nettrace `EventPipeSession.EventStream`, turning raw runtime events into typed CPU/GC/exception/allocation records for the support-bundle event artifact.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Diagnostics.Tracing.TraceEvent`
- package: `Microsoft.Diagnostics.Tracing.TraceEvent` (MIT)
- assembly: `Microsoft.Diagnostics.Tracing.TraceEvent`
- namespace: `Microsoft.Diagnostics.Tracing` (source/session), `Microsoft.Diagnostics.Tracing.Etlx` (TraceLog), `Microsoft.Diagnostics.Tracing.Parsers` (provider parsers), `Microsoft.Diagnostics.Tracing.Session` (live ETW)
- target: `netstandard2.0`
- depends: `Microsoft.Diagnostics.FastSerialization` (`IFastSerializable` stream format), `Microsoft.Diagnostics.NETCore.Client` (EventPipe session producer), `Dia2Lib`/`TraceReloggerLib` (native-symbol and relogger interop, Windows)
- asset: runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: source and dispatch surfaces, namespace `Microsoft.Diagnostics.Tracing`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]             |
| :-----: | :-------------------------- | :------------ | :----------------------- |
|  [01]   | `TraceEventDispatcher`      | abstract base | event pump and dispatch  |
|  [02]   | `EventPipeEventSource`      | source        | nettrace stream and file |
|  [03]   | `ETWTraceEventSource`       | source        | Windows ETW file         |
|  [04]   | `Etlx.TraceLog`             | indexed log   | stack-resolved `.etlx`   |
|  [05]   | `Session.TraceEventSession` | live session  | Windows real-time ETW    |

[PUBLIC_TYPE_SCOPE]: parser surfaces, namespace `Microsoft.Diagnostics.Tracing.Parsers`

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :------------------------------- | :------------ | :----------------------------------- |
|  [01]   | `ClrTraceEventParser`            | parser        | GC/JIT/exception/allocation events   |
|  [02]   | `SampleProfilerTraceEventParser` | parser        | CPU sample stack events              |
|  [03]   | `DynamicTraceEventParser`        | parser        | manifest and `EventSource` providers |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stream decode (EventPipe path)

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :-------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `EventPipeEventSource(Stream)`                      | ctor     | decode a live `EventPipeSession.EventStream`        |
|  [02]   | `EventPipeEventSource(string)`                      | ctor     | decode a captured `.nettrace` file                  |
|  [03]   | `EventPipeEventSource.Clr` / `.Dynamic` / `.Kernel` | property | built-in parser instances for callback subscription |
|  [04]   | `TraceEventDispatcher.Process() -> bool`            | instance | pump the stream to EOF, firing callbacks            |
|  [05]   | `EventPipeEventSource.HeaderKeyValuePairs`          | property | trace header metadata dictionary                    |
|  [06]   | `EventPipeEventSource.Dispose()`                    | instance | release the source                                  |

[ENTRYPOINT_SCOPE]: post-processing and live ETW (Windows)

| [INDEX] | [SURFACE]                                    | [SHAPE] | [CAPABILITY]                              |
| :-----: | :------------------------------------------- | :------ | :---------------------------------------- |
|  [01]   | `TraceLog.OpenOrConvert(string) -> TraceLog` | static  | indexed `.etlx` with resolved call stacks |
|  [02]   | `TraceEventSession(string)`                  | ctor    | live real-time ETW dispatch (Windows)     |

- `TraceEventSession`: bind kernel and user providers via `EnableProvider(...)`, read the dispatch source via `.Source`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Decode is subscribe-then-pump: construct a `TraceEventDispatcher` (`EventPipeEventSource` for nettrace, `ETWTraceEventSource` for `.etl`), register typed callbacks on a parser (`source.Clr.GCHeapStats += ...`, `source.Dynamic.All += ...`), then `Process()` drives the stream to EOF firing every callback synchronously on the pump thread. A callback's `TraceEvent` record is reused per event, so a retained field copies through `e.Clone()`.
- `EventPipeEventSource` decodes nettrace versions 3-6 cross-platform and is the correct decoder for `EventPipeSession.EventStream` on every host; `ETWTraceEventSource` and `TraceEventSession` are Windows-only ETW paths the host-neutral spine treats as a Windows-host capture option.
- `TraceLog.OpenOrConvert` builds the stack-resolved, time-ordered `.etlx` index when stack-attributed analysis is needed; streaming `Process()` is the low-overhead default for a bounded capture window.
- This package owns event-STREAM decode; a `.gcdump` heap-object-graph artifact routes to its own owner, the `dotnet-gcdump` tool.

[STACKING]:
- `Microsoft.Diagnostics.NETCore.Client`(`.api/api-diagnostics-client.md`): `EventPipeSession.EventStream` feeds `new EventPipeEventSource(stream).Process()` on a dedicated pump — the one capture→decode hand-off composing the `event-trace` `SupportArtifact` row.
- capture fan: the `event-trace` artifact row owns capture caps and redaction; TraceEvent contributes the decode-and-summarize step, never a second capture surface.
- fault band: decode failures land as the typed `SupportFault` case in registry band 4810, the dump-capture band.
- receipt: the decoded summary is one `SupportReceipt` row content-hashed through `Rasm.Domain.ContentHash.Of`.

[LOCAL_ADMISSION]:
- TraceEvent is the decode half of the support-bundle event artifact: `DiagnosticsClient.StartEventPipeSession` produces the stream and `new EventPipeEventSource(stream)` decodes it, composing one `event-trace` `SupportArtifact` row.
- Parser and provider selection is policy DATA on the artifact row — the `EventPipeProvider` set and the subscribed callbacks derive from the same row, so a capture profile is one data decision, never a call-site literal.
- Decode runs on a dedicated pump inside the capture window's `DeadlineClass` bound; a malformed or truncated stream ends `Process()` with the partial events dispatched, folded to `SupportReceipt.Partial`.
- Decoded event summary output — GC pause histogram, allocation top-N, exception counts — passes the redaction and truncation law before entering the manifest; raw event payloads never cross the wire un-redacted.

[RAIL_LAW]:
- Package: `Microsoft.Diagnostics.Tracing.TraceEvent`
- Owns: ETW/EventPipe event-STREAM decode into typed `TraceEvent` records for the support-bundle event artifact
- Accept: the `EventPipeSession.EventStream`, a policy-driven parser and provider set, and a bounded pump inside the capture window
- Reject: a `.gcdump` heap-graph claim (routes to `dotnet-gcdump`), a retained un-cloned `TraceEvent` record, an unbounded live ETW session on the host-neutral path, or a thrown decode fault crossing the bundle pipeline
