# [RASM_API_PYROSCOPE_OPENTELEMETRY]

`Pyroscope.OpenTelemetry` correlates continuous profiles with traces: one span processor stamps `pyroscope.profile.id` on every root span and hands the span context to the Pyroscope profiler, so a trace click-through lands on the exact flame-graph slice. Its companion `Pyroscope` package owns the profiler agent itself; this package owns only the span-to-profile join.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Pyroscope.OpenTelemetry`
- package: `Pyroscope.OpenTelemetry`
- assembly: `Pyroscope.OpenTelemetry`
- profiler package: `Pyroscope` — the push-mode profiler agent whose `Profiler.Instance` this processor drives
- namespace: `Pyroscope.OpenTelemetry`
- asset: runtime library
- rail: profiling correlation

## [02]-[PUBLIC_TYPES]

[PROCESSOR_TYPES]: the correlation processor
- rail: profiling correlation

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE] | [CAPABILITY]                                             |
| :-----: | :----------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `PyroscopeSpanProcessor` | span processor | `BaseProcessor<Activity>` stamping root-span profile ids |

`OnStart` runs only on root spans — local roots and remote-parented entries — converting the span/trace ids into the profiler's span context and tagging the span `pyroscope.profile.id`; `OnEnd` clears the profiler's span context so non-root interior spans never re-stamp.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration
- rail: profiling correlation

| [INDEX] | [SURFACE]                                    | [KIND]        | [CAPABILITY]                              |
| :-----: | :------------------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `AddProcessor(new PyroscopeSpanProcessor())` | processor row | one row on the tracer builder per process |

## [04]-[IMPLEMENTATION_LAW]

[PROFILE_TOPOLOGY]:
- root: one `PyroscopeSpanProcessor` per tracer provider, registered where the provider is built; the profiler agent pushes profiles independently, and the processor only threads span identity into them

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): an `AddProcessor` row — registration order places it before exporting processors so the tag exists when the span exports.
- `TraceBased` exemplars complete the triangle: metric→trace via exemplar, trace→profile via `pyroscope.profile.id`.

[LOCAL_ADMISSION]:
- Profiles are push-mode like every estate signal; the processor composes only where the Pyroscope agent runs, an AppHost service-root decision.

[RAIL_LAW]:
- Package: `Pyroscope.OpenTelemetry`
- Owns: span-to-profile correlation stamping
- Accept: one processor row per tracer provider beside the push profiler agent
- Reject: per-span manual profile tagging; a second profile-id vocabulary
