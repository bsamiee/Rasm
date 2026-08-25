# [RASM_APPHOST_API_TRACEEVENT]

`Microsoft.Diagnostics.Tracing.TraceEvent` (perfview) owns managed ETW/EventPipe stream decode: an `EventPipeEventSource` reads a runtime-event stream and dispatches strongly-typed `TraceEvent` records through provider parsers under `Process()`, and `TraceLog` post-processes a stream into a stack-resolved `.etlx` index. It is the sole decoder for the nettrace `EventPipeSession.EventStream`, turning raw runtime events into typed CPU/GC/exception/allocation records for the support-bundle event artifact.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Diagnostics.Tracing.TraceEvent`
- package: `Microsoft.Diagnostics.Tracing.TraceEvent` (MIT)
- assembly: `Microsoft.Diagnostics.Tracing.TraceEvent`
- namespace: `Microsoft.Diagnostics.Tracing` (source/session), `Microsoft.Diagnostics.Tracing.Etlx` (TraceLog), `Microsoft.Diagnostics.Tracing.Parsers` (provider parsers), `Microsoft.Diagnostics.Tracing.EventPipe` (sample profiler), `Microsoft.Diagnostics.Tracing.Session` (live ETW)
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

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :------------------------ | :------------ | :----------------------------------- |
|  [01]   | `ClrTraceEventParser`     | parser        | GC/JIT/exception/allocation events   |
|  [02]   | `DynamicTraceEventParser` | parser        | manifest and `EventSource` providers |

[PUBLIC_TYPE_SCOPE]: sample-profiler surfaces, namespace `Microsoft.Diagnostics.Tracing.EventPipe`

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `SampleProfilerTraceEventParser` | parser        | CPU sample and stack-walk events                |
|  [02]   | `ClrThreadSampleTraceData`       | record        | one sample; `Type` is the `ClrThreadSampleType` |
|  [03]   | `ClrThreadStackWalkTraceData`    | record        | the frame blob paired with the sample           |
|  [04]   | `ClrThreadSampleType`            | enum          | the sample class one `ThreadSample` carries     |

- `Microsoft.Diagnostics.Tracing.EventPipe` seats the whole sample-profiler family, never the `Parsers` namespace the CLR and dynamic parsers share, so a prelude reaching the CPU rows carries a second `using`.
- `ProviderName` is `Microsoft-DotNETCore-SampleProfiler` and `ProviderGuid` its static pair, so the `EventPipeProvider` row and the parser read one identity rather than a hand-spelled provider string.

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

[ENTRYPOINT_SCOPE]: CPU sample and stack-walk decode, namespace `Microsoft.Diagnostics.Tracing.EventPipe`

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :-------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `new SampleProfilerTraceEventParser(TraceEventSource)`          | ctor     | bind the sample provider to a dispatcher       |
|  [02]   | `parser.ThreadSample += Action<ClrThreadSampleTraceData>`       | event    | one CPU sample, `Type` naming the sample class |
|  [03]   | `parser.ThreadStackWalk += Action<ClrThreadStackWalkTraceData>` | event    | the frame blob for the sample that precedes it |
|  [04]   | `ClrThreadStackWalkTraceData.FrameCount`                        | property | frame count, `EventDataLength / PointerSize`   |
|  [05]   | `ClrThreadStackWalkTraceData.InstructionPointer(int)`           | instance | `ulong` IP at a frame; index 0 is the deepest  |

- `ThreadSample` and `ThreadStackWalk` arrive as SEPARATE events on one thread's stream, the walk following its sample; a decoder correlating them holds the sample's `ThreadID` and `TimeStamp` from the record it just saw, because the walk record carries neither in its payload — its only payload name is `FrameCount`.
- `InstructionPointer(index)` returns a raw code address, never a symbolized frame name: this package resolves no symbols on a live stack walk. Frame names come from `Etlx.TraceLog.OpenOrConvert`'s stack-resolved index or from a module-and-offset projection the bundle stamps; an artifact presenting raw pointers as a call stack claims symbolization it does not have.
- `FrameCount` derives from the payload length, so a truncated record yields a short count rather than a fault, and an index at or above it reads adjacent event memory — a decoder bounds every read against `FrameCount`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Decode is subscribe-then-pump: construct a `TraceEventDispatcher` (`EventPipeEventSource` for nettrace, `ETWTraceEventSource` for `.etl`), register typed callbacks on a parser (`source.Clr.GCHeapStats += ...`, `source.Dynamic.All += ...`), then `Process()` drives the stream to EOF firing every callback synchronously on the pump thread. Callbacks reuse one `TraceEvent` record per event, so a retained field copies through `e.Clone()`.
- `EventPipeEventSource` decodes nettrace versions 3-6 cross-platform and is the correct decoder for `EventPipeSession.EventStream` on every host; `ETWTraceEventSource` and `TraceEventSession` are Windows-only ETW paths the host-neutral spine treats as a Windows-host capture option.
- `TraceLog.OpenOrConvert` builds the stack-resolved, time-ordered `.etlx` index when stack-attributed analysis is needed; streaming `Process()` is the low-overhead default for a bounded capture window.
- This package owns event-STREAM decode; a `.gcdump` heap-object-graph artifact routes to its own owner, the `dotnet-gcdump` tool.

[STACKING]:
- `Microsoft.Diagnostics.NETCore.Client`(`.api/api-diagnostics-client.md`): `EventPipeSession.EventStream` feeds `new EventPipeEventSource(stream).Process()` on a dedicated pump — the one capture→decode hand-off composing the `event-trace` `SupportArtifact` row.
- capture fan: the `event-trace` artifact row owns capture caps and redaction; TraceEvent contributes the decode-and-summarize step, never a second capture surface.
- fault band: decode failures land as the typed `SupportFault` case in registry band 4810, the dump-capture band.
- output: the decoded summary is one `SupportManifest` entry content-hashed through `Rasm.Domain.ContentHash.Of`.

[LOCAL_ADMISSION]:
- TraceEvent is the decode half of the support-bundle event artifact: `DiagnosticsClient.StartEventPipeSession` produces the stream and `new EventPipeEventSource(stream)` decodes it, composing one `event-trace` `SupportArtifact` row.
- Parser and provider selection is policy DATA on the artifact row — the `EventPipeProvider` set and the subscribed callbacks derive from the same row, so a capture profile is one data decision, never a call-site literal.
- Decode runs on a dedicated pump inside the capture window's `DeadlineClass` bound; a malformed or truncated stream ends `Process()` with the partial events dispatched and a typed `SupportFault` manifest entry.
- Decoded event summary output — GC pause histogram, allocation top-N, exception counts — passes the redaction and truncation law before entering the manifest; raw event payloads never cross the wire un-redacted.
- Profile artifacts state their symbolization posture on the row: a streaming `SampleProfilerTraceEventParser` decode yields instruction pointers alone, so the artifact carries module-relative addresses and names the absent symbol source rather than presenting pointers as frames.

[RAIL_LAW]:
- Package: `Microsoft.Diagnostics.Tracing.TraceEvent`
- Owns: ETW/EventPipe event-STREAM decode into typed `TraceEvent` records for the support-bundle event artifact
- Accept: the `EventPipeSession.EventStream`, a policy-driven parser and provider set, and a bounded pump inside the capture window
- Reject: a `.gcdump` heap-graph claim (routes to `dotnet-gcdump`), a retained un-cloned `TraceEvent` record, an unbounded live ETW session on the host-neutral path, or a thrown decode fault crossing the bundle pipeline
