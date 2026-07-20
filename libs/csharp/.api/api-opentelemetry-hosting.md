# [RASM_API_OPENTELEMETRY_HOSTING]

`OpenTelemetry.Extensions.Hosting` binds the SDK to the generic host: `AddOpenTelemetry()` on `IServiceCollection` mints one `OpenTelemetryBuilder` whose `WithTracing`/`WithMetrics`/`WithLogging` delegates configure the three providers, and the host owns provider construction, flush, and shutdown. AppHost's hosted root composes here; hostless plugin roots ride `Sdk.Create*ProviderBuilder` instead.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Extensions.Hosting`
- package: `OpenTelemetry.Extensions.Hosting`
- assembly: `OpenTelemetry.Extensions.Hosting`
- namespace: `OpenTelemetry`, `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: telemetry composition

## [02]-[PUBLIC_TYPES]

[BUILDER_TYPES]: the DI composition builder
- rail: telemetry composition

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE] | [CAPABILITY]                                        |
| :-----: | :-------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `OpenTelemetryBuilder`            | builder        | three-signal configuration over one `Services` seat |
|  [02]   | `OpenTelemetryServicesExtensions` | registration   | `AddOpenTelemetry(IServiceCollection)`              |

`OpenTelemetryBuilder` carries `Services` and the fluent verbs `ConfigureResource(Action<ResourceBuilder>)`, `WithTracing([Action<TracerProviderBuilder>])`, `WithMetrics([Action<MeterProviderBuilder>])`, and `WithLogging([Action<LoggerProviderBuilder>][, Action<OpenTelemetryLoggerOptions>])`; it implements the `IOpenTelemetryBuilder` seam cross-cutting extensions such as `UseOtlpExporter` target.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: host composition
- rail: telemetry composition

| [INDEX] | [SURFACE]           | [KIND]          | [CAPABILITY]                                         |
| :-----: | :------------------ | :-------------- | :--------------------------------------------------- |
|  [01]   | `AddOpenTelemetry`  | registration    | one builder per host; repeated calls return one seat |
|  [02]   | `ConfigureResource` | identity verb   | augments all three providers together                |
|  [03]   | `WithTracing`       | trace delegate  | receives the `TracerProviderBuilder`                 |
|  [04]   | `WithMetrics`       | metric delegate | receives the `MeterProviderBuilder`                  |
|  [05]   | `WithLogging`       | log delegate    | provider builder + `OpenTelemetryLoggerOptions` legs |

## [04]-[IMPLEMENTATION_LAW]

[HOSTING_TOPOLOGY]:
- root: one `AddOpenTelemetry()` per process admits all three signals; `ConfigureResource` is the only resource verb reaching all three providers
- lifetime: provider construction, flush, and shutdown ride the host — shutdown drains traces and metrics before the log provider, because logs evidence the drain

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): the delegates receive that package's builders; sampler, view, exemplar, and processor rows compose inside them.
- `OpenTelemetry.Exporter.OpenTelemetryProtocol`(`api-opentelemetry-exporter-otlp.md`): `UseOtlpExporter()` chains directly off this builder.
- instrumentation packages (`api-otel-instrumentation-*.md`): their `Add*Instrumentation` verbs land inside the `WithTracing`/`WithMetrics` delegates.

[LOCAL_ADMISSION]:
- Hosted composition is AppHost-root-only; no library and no host-boundary plugin references this package.
- `serviceInstanceId` pins from the suite boot mint inside `ConfigureResource` — auto-generation anonymizes restart lineage.

[RAIL_LAW]:
- Package: `OpenTelemetry.Extensions.Hosting`
- Owns: DI-hosted three-signal provider composition and host-bound provider lifetime
- Accept: one builder per hosted process, configured through the three delegates
- Reject: a second `AddOpenTelemetry` root fragmenting resource identity; hosted composition inside plugin load contexts
