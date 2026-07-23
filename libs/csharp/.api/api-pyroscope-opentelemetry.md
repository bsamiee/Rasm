# [RASM_API_PYROSCOPE_OPENTELEMETRY]

`Pyroscope.OpenTelemetry` joins continuous profiles to traces through one span processor: each root span's ids enter the profiler's thread-local span context while the span itself carries a `pyroscope.profile.id` tag, so a trace click-through resolves the exact flame-graph slice. Profiler control, label frames, and ingest credentials stay with the `Pyroscope` agent; this package owns the join alone.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Pyroscope.OpenTelemetry`
- package: `Pyroscope.OpenTelemetry` (Apache-2.0)
- assembly: `Pyroscope.OpenTelemetry`
- namespace: `Pyroscope.OpenTelemetry`
- depends: `Pyroscope`, `OpenTelemetry`
- rail: profiling correlation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: sole exported type, a tracer-pipeline processor

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :----------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `PyroscopeSpanProcessor` | class         | `BaseProcessor<Activity>` stamping root spans |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and the two overridden lifecycle hooks

| [INDEX] | [SURFACE]                                  | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :----------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `PyroscopeSpanProcessor()`                 | ctor     | knob-free construction, one per tracer provider |
|  [02]   | `PyroscopeSpanProcessor.OnStart(Activity)` | instance | writes the span context and tags the span       |
|  [03]   | `PyroscopeSpanProcessor.OnEnd(Activity)`   | instance | zeroes the profiler span context                |

- `PyroscopeSpanProcessor.OnStart`: root selection reads `Activity.Parent?.HasRemoteParent ?? true`, so a parentless span and a span whose parent is remote-parented both qualify; a fault inside the hook is caught and the span exits untagged.
- `PyroscopeSpanProcessor.OnEnd`: zeroes only under the same root predicate, so an interior span never clears the context its root installed.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One processor per tracer provider carries the whole join; the agent pushes profiles on its own cadence and the hooks thread span identity into the profiler's per-thread context.
- Only the two hooks override the base, so the join buffers nothing through drain, dispose, or an options surface.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): one `TracerProviderBuilder.AddProcessor(BaseProcessor<Activity>)` row ahead of every exporting processor so the tag exists at export; the `AddProcessor<T>()` overload resolves through `GetRequiredService<T>`, so that spelling binds only where the type is also a service registration.
- `Pyroscope`(`Rasm.AppHost/.api/api-pyroscope.md`): `Profiler.Instance.SetSpanContext(ulong, ulong, ulong)` is the sole seam — this package holds no profiler handle, and the agent's label, tracking, and credential surface stays untouched by the join.
- Within-lib: `SignalGovernance` seats the row on its `Profile` signal, so the processor and the agent enter at the one service root that resolves the profiler endpoint.

[LOCAL_ADMISSION]:
- Service app roots alone compose the row, where the CLR profiler attaches; the correlation lands only on a Linux or Windows x64 process with `PYROSCOPE_PROFILING_ENABLED` set, and elsewhere the profiler write no-ops while the span keeps its tag.

[RAIL_LAW]:
- Package: `Pyroscope.OpenTelemetry`
- Owns: span-to-profile correlation on the tracer pipeline
- Accept: one processor row per tracer provider beside the push profiler agent
- Reject: a hand-rolled `BaseProcessor<Activity>` re-deriving profile ids; a second profile-id tag vocabulary
