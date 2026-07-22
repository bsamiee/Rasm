# [RASM_APPHOST_API_POLLY_EXTENSIONS]

`Polly.Extensions` binds resilience telemetry onto Polly.Core pipelines: `ConfigureTelemetry` inserts a diagnostic strategy at the pipeline head, `TelemetryOptions` carries the logging, severity, formatting, and metering policy, and `PollyServiceCollectionExtensions` registers keyed pipelines into the DI container.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Polly.Extensions`
- package: `Polly.Extensions`
- assembly: `Polly.Extensions`
- namespace: `Polly`, `Polly.Telemetry`, `Polly.DependencyInjection`
- asset: runtime library
- rail: resilience telemetry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: telemetry configuration and enrichment family

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :--------------------------------------------- | :------------ | :------------------------------ |
|  [01]   | `TelemetryResiliencePipelineBuilderExtensions` | class         | builder telemetry configuration |
|  [02]   | `TelemetryOptions`                             | class         | telemetry policy carrier        |
|  [03]   | `MeteringEnricher`                             | class         | metric enricher base            |
|  [04]   | `EnrichmentContext<TResult, TArgs>`            | struct        | metric enrichment context       |
|  [05]   | `SeverityProviderArguments`                    | struct        | severity callback input         |
|  [06]   | `PollyServiceCollectionExtensions`             | class         | DI pipeline registration        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pipeline telemetry configuration

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `ConfigureTelemetry(ILoggerFactory) -> TBuilder`   | static   | logger-backed telemetry      |
|  [02]   | `ConfigureTelemetry(TelemetryOptions) -> TBuilder` | static   | option-backed telemetry      |
|  [03]   | `TelemetryOptions.LoggerFactory`                   | property | telemetry logging provider   |
|  [04]   | `TelemetryOptions.MeteringEnrichers`               | property | metric enricher collection   |
|  [05]   | `TelemetryOptions.SeverityProvider`                | property | event severity classifier    |
|  [06]   | `TelemetryOptions.ResultFormatter`                 | property | outcome-to-metric projection |
|  [07]   | `TelemetryOptions.TelemetryListeners`              | property | event listener collection    |

[ENTRYPOINT_SCOPE]: enrichment and severity authoring

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]              |
| :-----: | :-------------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `MeteringEnricher.Enrich(in EnrichmentContext<TResult, TArgs>)` | instance | metric dimension emission |
|  [02]   | `EnrichmentContext<TResult, TArgs>.Tags`                        | property | mutable metric tag list   |
|  [03]   | `EnrichmentContext<TResult, TArgs>.TelemetryEvent`              | property | enriched event payload    |
|  [04]   | `SeverityProviderArguments.Event`                               | property | resilience event          |
|  [05]   | `SeverityProviderArguments.Source`                              | property | telemetry source          |
|  [06]   | `SeverityProviderArguments.Context`                             | property | resilience context        |

[ENTRYPOINT_SCOPE]: DI pipeline registration

| [INDEX] | [SURFACE]                             | [SHAPE] | [CAPABILITY]                |
| :-----: | :------------------------------------ | :------ | :-------------------------- |
|  [01]   | `AddResiliencePipeline(TKey, Action)` | static  | keyed pipeline registration |
|  [02]   | `AddResiliencePipelines(Action)`      | static  | bulk pipeline registration  |
|  [03]   | `AddResiliencePipelineRegistry()`     | static  | registry-only registration  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ConfigureTelemetry<TBuilder>` inserts a diagnostic strategy at the head of the composite pipeline, so telemetry observes every downstream strategy.
- Metric enrichment, event severity, and result formatting are value-level callbacks on `TelemetryOptions` reading resilience event arguments, never mutating the outcome.

[STACKING]:
- `Polly.Core`(`.api/api-polly-core.md`): `ConfigureTelemetry<TBuilder>` binds any `ResiliencePipelineBuilder`/`ResiliencePipelineBuilder<T>`; `SeverityProviderArguments` carries the emitted `ResilienceEvent`/`ResilienceTelemetrySource`, and `AddResiliencePipelineRegistry<TKey>` wires the `ResiliencePipelineRegistry<TKey>`/`ResiliencePipelineProvider<TKey>` keyed-resolution surface.
- `OpenTelemetry`(`.api/api-otel.md`): `MeteringEnricher.Enrich` appends `EnrichmentContext.Tags` as metric dimensions on the resilience `Meter`, projected through `MeterProvider`; `TelemetryOptions.LoggerFactory` binds the observability logging rail.
- DI composition: `PollyServiceCollectionExtensions.AddResiliencePipeline*` folds every keyed pipeline into `IServiceCollection` (`.api/api-di.md`), resolved through the registry and configured by `AddOptions<TelemetryOptions>`.

[LOCAL_ADMISSION]:
- Resilience telemetry configures the pipeline builder at composition, never inside handled operations.
- Metric enrichment reads bounded telemetry event arguments, never arbitrary domain state.
- Severity is a value-level callback over resilience event arguments; result formatting projects the outcome for observability and cannot mutate it.

[RAIL_LAW]:
- Package: `Polly.Extensions`
- Owns: resilience pipeline telemetry and keyed DI pipeline registration
- Accept: builder-level telemetry policy and keyed pipeline registration
- Reject: per-call logging wrappers around resilience pipelines
