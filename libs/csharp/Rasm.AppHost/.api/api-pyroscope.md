# [RASM_APPHOST_API_PYROSCOPE]

`Pyroscope.OpenTelemetry` supplies a `BaseProcessor<Activity>` span processor that propagates the active root span's `SpanId` into the Pyroscope profiler as a `profileId`, enabling trace-to-profile correlation via the `pyroscope.profile.id` span tag.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Pyroscope.OpenTelemetry`
- package: `Pyroscope.OpenTelemetry`
- assembly: `Pyroscope.OpenTelemetry`
- namespace: `Pyroscope.OpenTelemetry`
- asset: runtime library
- rail: telemetry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: span processor family
- rail: telemetry

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [RAIL]                             |
| :-----: | :----------------------- | :------------- | :--------------------------------- |
|  [01]   | `PyroscopeSpanProcessor` | span processor | `BaseProcessor<Activity>` subclass |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: processor lifecycle hooks
- rail: telemetry

| [INDEX] | [SURFACE]                | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :----------------------- | :-------------- | :------------------------------------------------ |
|  [01]   | `OnStart(Activity data)` | span start hook | sets `profileId` on profiler for root spans       |
|  [02]   | `OnEnd(Activity data)`   | span end hook   | resets `profileId` to `0` on root span completion |

## [04]-[IMPLEMENTATION_LAW]

[PYROSCOPE_TOPOLOGY]:
- namespace: `Pyroscope.OpenTelemetry`; one public type
- root span detection: `data.Parent?.HasRemoteParent ?? true` — a span is root when it has no parent or its parent is remote
- profile id: `SpanId.ToString()` converted to `ulong` via `Convert.ToUInt64(hexString.ToUpper(), 16)`
- span tag: `pyroscope.profile.id` is added to the `Activity` so the profile id appears in the trace
- `Profiler.Instance.SetProfileId(0uL)` resets the profiler state when the root span ends
- constructor exceptions are caught and written to `Console.WriteLine`; they do not propagate

[LOCAL_ADMISSION]:
- Register `PyroscopeSpanProcessor` with the OpenTelemetry tracer provider as an additional processor alongside the OTLP exporter.
- The processor operates on root spans only; nested spans inherit the profiler state from the active root.
- No configuration properties; the processor is stateless beyond its lifecycle hook calls.

[RAIL_LAW]:
- Package: `Pyroscope.OpenTelemetry`
- Owns: Pyroscope profile-id injection into OpenTelemetry span metadata
- Accept: trace provider pipeline registration via `AddProcessor<PyroscopeSpanProcessor>()`
- Reject: manual `Profiler.Instance.SetProfileId` calls outside of this processor
