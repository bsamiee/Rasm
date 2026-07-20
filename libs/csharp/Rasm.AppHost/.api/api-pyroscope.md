# [RASM_APPHOST_API_PYROSCOPE]

`Pyroscope` hosts the native continuous-profiler agent as a process-wide `Profiler` singleton — dynamic tags, per-signal tracking toggles, ingest credentials, and span-context correlation that degrade to a no-op when the native library is absent. `Pyroscope.OpenTelemetry` bridges the OpenTelemetry trace pipeline into that singleton: a `BaseProcessor<Activity>` propagates each root span's `SpanId` into the profiler as a `profileId`, correlating traces to profiles through the `pyroscope.profile.id` span tag. `Profiler.Instance` is the singleton seam the two packages meet at.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Pyroscope`
- package: `Pyroscope`
- assembly: `Pyroscope`
- namespace: `Pyroscope`
- asset: runtime library
- rail: profiling

[PACKAGE_SURFACE]: `Pyroscope.OpenTelemetry`
- package: `Pyroscope.OpenTelemetry`
- assembly: `Pyroscope.OpenTelemetry`
- namespace: `Pyroscope.OpenTelemetry`
- asset: runtime library
- rail: telemetry

## [02]-[PUBLIC_TYPES]

[AGENT_TYPES]: native profiler control (namespace `Pyroscope`)
- rail: profiling

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [RAIL]                                         |
| :-----: | :------------------ | :----------------- | :--------------------------------------------- |
|  [01]   | `Profiler`          | profiler singleton | native profiler control seam                   |
|  [02]   | `LabelSet`          | label frame        | immutable tag set with `Activate`/`BuildUpon`  |
|  [03]   | `LabelSet.Builder`  | label builder      | `Add`/`Build` over a prior set                 |
|  [04]   | `LabelsWrapper`     | scoped label span  | runs an action under a `LabelSet`              |

[BRIDGE_TYPES]: span processor family (namespace `Pyroscope.OpenTelemetry`)
- rail: telemetry

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [RAIL]                             |
| :-----: | :----------------------- | :------------- | :--------------------------------- |
|  [01]   | `PyroscopeSpanProcessor` | span processor | `BaseProcessor<Activity>` subclass |

## [03]-[ENTRYPOINTS]

[AGENT_SCOPE]: profiler control surface (namespace `Pyroscope`)
- rail: profiling

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY]     | [RAIL]                                         |
| :-----: | :------------------------------------------ | :----------------- | :--------------------------------------------- |
|  [01]   | `Profiler.Instance`                         | singleton accessor | lazy process-wide profiler handle              |
|  [02]   | `SetProfileId`                              | correlation seam   | sets or clears the active profile id           |
|  [03]   | `SetSpanContext`                            | correlation seam   | local-root span id plus trace id hi/lo         |
|  [04]   | `SetDynamicTag` / `SetDynamicTags`          | dynamic tags       | set or replace tags, `ClearDynamicTags` clears |
|  [05]   | `Set*TrackingEnabled`                       | signal toggles     | `CPU`/`Allocation`/`Contention`/`Exception`    |
|  [06]   | `SetAuthToken` / `SetBasicAuth`             | ingest auth        | server credential seam                         |
|  [07]   | `LabelsWrapper.Do`                          | scoped labels      | runs an action under a `LabelSet`, then resets |
|  [08]   | `LabelSet.BuildUpon` / `Build` / `Activate` | label lifecycle    | derive, seal, and activate a tag frame         |

[BRIDGE_SCOPE]: processor lifecycle hooks (namespace `Pyroscope.OpenTelemetry`)
- rail: telemetry

| [INDEX] | [SURFACE]                | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :----------------------- | :-------------- | :------------------------------------------------ |
|  [01]   | `OnStart(Activity data)` | span start hook | sets `profileId` on profiler for root spans       |
|  [02]   | `OnEnd(Activity data)`   | span end hook   | resets `profileId` to `0` on root span completion |

## [04]-[IMPLEMENTATION_LAW]

[AGENT_TOPOLOGY]:
- singleton: `Profiler.Instance` resolves a lazy process-wide handle via `LazyInitializer.EnsureInitialized`; one profiler owns the process.
- correlation: `SetProfileId(ulong)` and `SetSpanContext(ulong localRootSpanId, ulong traceIdHi, ulong traceIdLo)` bind the active profile to a trace; the bridge package drives `SetProfileId`.
- tags: `SetDynamicTag`/`SetDynamicTags` mutate per-scope profile tags, `ClearDynamicTags` resets them, and `LabelSet` + `LabelsWrapper.Do` scope a tag frame around a delegate.
- signals: `SetCPUTrackingEnabled`, `SetAllocationTrackingEnabled`, `SetContentionTrackingEnabled`, and `SetExceptionTrackingEnabled` toggle per-signal capture; `SetAuthToken`/`SetBasicAuth` carry ingest credentials.
- degradation: native interop faults (`DllNotFoundException`) are swallowed per call, so every `Profiler` method is a no-op when the native profiler library is absent.

[PYROSCOPE_TOPOLOGY]:
- namespace: `Pyroscope.OpenTelemetry`; one public type
- root span detection: `data.Parent?.HasRemoteParent ?? true` — a span is root when it has no parent or its parent is remote
- profile id: `SpanId.ToString()` converted to `ulong` via `Convert.ToUInt64(hexString.ToUpper(), 16)`
- span tag: `pyroscope.profile.id` is added to the `Activity` so the profile id appears in the trace
- `Profiler.Instance.SetProfileId(0uL)` resets the profiler state when the root span ends
- constructor exceptions are caught and written to `Console.WriteLine`; they do not propagate

[STACKING]:
- `PyroscopeSpanProcessor` writes profile ids INTO the `Pyroscope` agent: `OnStart` calls `Profiler.Instance.SetProfileId(spanId)` and `OnEnd` calls `Profiler.Instance.SetProfileId(0uL)`. `Profiler.Instance` is the singleton seam — the OpenTelemetry package holds no profiler of its own.
- `OpenTelemetry`(`api-opentelemetry.md`): `PyroscopeSpanProcessor` registers on the tracer provider through `AddProcessor` beside the OTLP exporter; the agent package needs no OpenTelemetry reference to run standalone.

[LOCAL_ADMISSION]:
- Register `PyroscopeSpanProcessor` with the OpenTelemetry tracer provider as an additional processor alongside the OTLP exporter; the processor operates on root spans only, and nested spans inherit the profiler state from the active root.
- Agent configuration — tracking toggles, dynamic tags, and ingest auth — enters through `Profiler.Instance` at the composition root; direct `SetProfileId` calls stay out of application code, owned solely by the span processor.
- No configuration properties on `PyroscopeSpanProcessor`; it is stateless beyond its lifecycle hook calls.

[RAIL_LAW]:
- Package: `Pyroscope`, `Pyroscope.OpenTelemetry`
- Owns: native continuous-profiler control and Pyroscope profile-id injection into OpenTelemetry span metadata
- Accept: `Profiler.Instance` configuration at the composition root; trace-provider registration via `AddProcessor<PyroscopeSpanProcessor>()`
- Reject: manual `Profiler.Instance.SetProfileId` calls outside the processor; a second profiler handle beside the singleton
