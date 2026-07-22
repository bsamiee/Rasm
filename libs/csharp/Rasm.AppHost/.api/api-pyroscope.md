# [RASM_APPHOST_API_PYROSCOPE]

`Pyroscope` hosts the native continuous-profiler agent as a process-wide `Profiler` singleton — dynamic tags, per-signal capture toggles, ingest credentials, and span-context correlation, each degrading to a no-op when the native library is absent. `Pyroscope.OpenTelemetry` bridges the OpenTelemetry trace pipeline into that singleton: `PyroscopeSpanProcessor` folds every root span's context through `SetSpanContext` and stamps the `pyroscope.profile.id` tag onto the span. `Profiler.Instance` is the singleton seam both packages meet at.

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

[AGENT_TYPES]: native profiler control, namespace `Pyroscope`

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :----------------- | :------------ | :---------------------------------- |
|  [01]   | `Profiler`         | class         | native profiler control seam        |
|  [02]   | `LabelSet`         | class         | immutable tag frame with `Activate` |
|  [03]   | `LabelSet.Builder` | class         | `Add`/`Build` over a prior frame    |
|  [04]   | `LabelsWrapper`    | class         | brackets a delegate under a frame   |

[BRIDGE_TYPES]: span processor, namespace `Pyroscope.OpenTelemetry`

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :----------------------- | :------------ | :--------------------------------- |
|  [01]   | `PyroscopeSpanProcessor` | class         | `BaseProcessor<Activity>` subclass |

## [03]-[ENTRYPOINTS]

[AGENT_SCOPE]: profiler control surface, namespace `Pyroscope`

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `Profiler.Instance`                               | property | lazy process-wide profiler handle        |
|  [02]   | `Profiler.SetSpanContext(ulong, ulong, ulong)`    | instance | bind local-root span and trace id hi/lo  |
|  [03]   | `Profiler.SetDynamicTag(string, string)`          | instance | set or replace one scope tag             |
|  [04]   | `Profiler.SetDynamicTags(Dictionary)`             | instance | replace the full scope tag set           |
|  [05]   | `Profiler.ClearDynamicTags()`                     | instance | reset all scope tags                     |
|  [06]   | `Profiler.Set*TrackingEnabled(bool)`              | instance | CPU/Allocation/Contention/Exception      |
|  [07]   | `Profiler.SetAuthToken(string)`                   | instance | bearer ingest credential                 |
|  [08]   | `Profiler.SetBasicAuth(string, string)`           | instance | basic-auth ingest credential             |
|  [09]   | `LabelSet.Empty`                                  | static   | base immutable tag frame                 |
|  [10]   | `LabelSet.BuildUpon() -> Builder`                 | instance | derive a builder from a frame            |
|  [11]   | `LabelSet.Builder.Add(string, string) -> Builder` | instance | chain one tag onto the builder           |
|  [12]   | `LabelSet.Builder.Build() -> LabelSet`            | instance | seal the builder into a frame            |
|  [13]   | `LabelSet.Activate()`                             | instance | push the frame's tags onto the profiler  |
|  [14]   | `LabelsWrapper.Do(LabelSet, Action)`              | static   | run a delegate under a frame, then reset |

[BRIDGE_SCOPE]: processor lifecycle hooks, namespace `Pyroscope.OpenTelemetry`

| [INDEX] | [SURFACE]                                  | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :----------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `PyroscopeSpanProcessor.OnStart(Activity)` | instance | root-span context into the profiler |
|  [02]   | `PyroscopeSpanProcessor.OnEnd(Activity)`   | instance | clears the profiler context         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Profiler.Instance` is a lazy process-wide singleton over `LazyInitializer.EnsureInitialized`; one profiler owns the process and every native-interop fault degrades the calling method to a no-op.
- `SetSpanContext(localRootSpanId, traceIdHi, traceIdLo)` is the sole trace-to-profile correlation seam, and the zero triple clears it.
- Dynamic tags scope through `LabelSet`: `Activate` clears then re-applies the frame, and `LabelsWrapper.Do` brackets a delegate and restores the prior frame on exit.
- `PyroscopeSpanProcessor` folds only root spans (`data.Parent?.HasRemoteParent ?? true`): `OnStart` casts the span and trace ids into `SetSpanContext` and stamps `pyroscope.profile.id` (`SpanId.ToString()`) onto the `Activity`, `OnEnd` clears the context, and `OnStart` swallows any fault to `Console.WriteLine`.

[STACKING]:
- `PyroscopeSpanProcessor` writes span context into the `Pyroscope` agent through `Profiler.Instance.SetSpanContext`; the bridge holds no profiler of its own, and the agent package runs standalone without an OpenTelemetry reference.
- `OpenTelemetry`(`.api/api-otel.md`): the processor registers on `TracerProviderBuilder` through `AddProcessor` beside the OTLP exporter.

[LOCAL_ADMISSION]:
- `PyroscopeSpanProcessor` registers on the tracer provider as an added processor beside the OTLP exporter; it carries no configuration and stays stateless past its lifecycle hooks.
- Agent configuration — tracking toggles, dynamic tags, ingest auth — enters through `Profiler.Instance` at the composition root, and `SetSpanContext` stays owned by the processor alone.

[RAIL_LAW]:
- Package: `Pyroscope`, `Pyroscope.OpenTelemetry`
- Owns: native continuous-profiler control and root-span profile correlation into OpenTelemetry traces
- Accept: `Profiler.Instance` configuration at the composition root; processor registration via `AddProcessor`
- Reject: `SetSpanContext` calls outside the processor; a second profiler handle beside the singleton
