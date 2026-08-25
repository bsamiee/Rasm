# [RASM_API_OPENTELEMETRY_HOSTING]

`OpenTelemetry.Extensions.Hosting` seats provider ownership in the generic host: one builder over the application `IServiceCollection` carries all three signals through the DI graph, and construction, flush, and disposal ride host start and stop.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: DI composition builder and the extension verb minting it

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :-------------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `OpenTelemetryBuilder`            | sealed class  | three-signal configuration over one `Services` seat |
|  [02]   | `OpenTelemetryServicesExtensions` | static class  | mints the builder onto an `IServiceCollection`      |

`OpenTelemetryBuilder` implements `IOpenTelemetryBuilder`; cross-cutting exporter and enrichment verbs extend that interface and chain off the instance the fluent verbs return.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: host composition — every row past `AddOpenTelemetry` is an `OpenTelemetryBuilder` member returning that same builder.

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `OpenTelemetryServicesExtensions.AddOpenTelemetry(IServiceCollection)`           | static   | mints the builder and hosted seat      |
|  [02]   | `Services`                                                                       | property | service seat every verb registers onto |
|  [03]   | `ConfigureResource(Action<ResourceBuilder>)`                                     | instance | one identity across all three signals  |
|  [04]   | `WithTracing()`                                                                  | instance | admits tracing at SDK defaults         |
|  [05]   | `WithTracing(Action<TracerProviderBuilder>)`                                     | instance | shapes the tracer provider inline      |
|  [06]   | `WithMetrics()`                                                                  | instance | admits metrics at SDK defaults         |
|  [07]   | `WithMetrics(Action<MeterProviderBuilder>)`                                      | instance | shapes the meter provider inline       |
|  [08]   | `WithLogging()`                                                                  | instance | admits logging at SDK defaults         |
|  [09]   | `WithLogging(Action<LoggerProviderBuilder>)`                                     | instance | shapes the logger provider inline      |
|  [10]   | `WithLogging(Action<LoggerProviderBuilder>, Action<OpenTelemetryLoggerOptions>)` | instance | adds the log-bridge options leg        |

- `OpenTelemetryServicesExtensions.AddOpenTelemetry`: mints a fresh builder per call over the same `IServiceCollection` and inserts the hosted seat once, so repeated calls converge on one provider set.
- `OpenTelemetryBuilder.WithLogging`: only the two-delegate overload admits a null leg; the single-delegate overload faults on a null configure.
- Two homes carry this verb and neither is wrong: `OpenTelemetryBuilder` declares the INSTANCE overloads here, each forwarding to the `IOpenTelemetryBuilder` STATIC extension `api-opentelemetry.md` rows, so a fence naming the concrete builder binds the instance form and one naming the interface binds the extension.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- root: one `AddOpenTelemetry()` per host, `ConfigureResource` the only verb stamping identity onto all three providers at once
- materialization: host start builds every configured provider through the hosted seat; a signal left unconfigured stays disabled behind one `OpenTelemetry-Extensions-Hosting` EventSource warning

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): each signal delegate receives that package's provider builder, so sampler, view, exemplar, and processor rows compose inside them.
- `OpenTelemetry.Exporter.OpenTelemetryProtocol`(`api-opentelemetry-exporter-otlp.md`): `UseOtlpExporter` extends `IOpenTelemetryBuilder`, claiming all three signals off this builder once, after every `With*` leg.
- `OpenTelemetry.Extensions`(`api-opentelemetry-extensions.md`): `AddBaggageActivityProcessor` registers inside the `WithTracing` delegate, promoting propagated baggage onto every span at start.
- `OpenTelemetry.Resources.*`(`api-otel-resources.md`): each `Add<X>Detector` chains inside the `ConfigureResource` delegate, folding host, OS, process, container, and runtime attributes into the one resource.
- `Microsoft.Extensions.Logging.Abstractions`(`api-logging-abstractions.md`): `WithLogging` registers the `OpenTelemetry`-named `ILoggerProvider` onto the host logging builder, so every library's `ILogger` record reaches the log provider.
- instrumentation packages (`api-otel-instrumentation-*.md`): each `Add*Instrumentation` verb lands inside the `WithTracing` or `WithMetrics` delegate.
- `Rasm.AppHost`: its observability root threads one `AddOpenTelemetry()` chain — identity through `ConfigureResource`, then instrumentation, view, and per-signal `AddOtlpExporter` rows inside the three delegates — folded as a state-threaded builder pass, so a new signal row lands as data; the cross-signal claim verb stays declined there because its reader, processor, and transport options are internal.

[LOCAL_ADMISSION]:
- `serviceInstanceId` pins from the suite boot mint inside `ConfigureResource`; auto-generation anonymizes restart lineage.
