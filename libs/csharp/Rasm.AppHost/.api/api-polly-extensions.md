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

[METER_IDENTITY]:
- `internal sealed class Polly.Telemetry.TelemetrySource` holds the meter `Polly` as a process-wide static singleton, version-stamped from the assembly informational version — so the scope is a WIRE STRING a consumer spells, never a member it binds, and every pipeline in the process writes ONE scope no matter how many builders configure telemetry.
- `ConfigureTelemetry` arms the listener that CONSTRUCTS and writes these instruments on that shared meter; it neither owns the meter nor admits it to a provider, so a pipeline configured without the scope admitted publishes to a meter no reader subscribes, while `AddMeter("Polly")` subscribes by name and stays order-independent against every such registration.
- This package opens NO `ActivitySource`: resilience telemetry is metrics beside `ILogger` records, and a trace-signal admission for the scope registers an empty source.

| [INDEX] | [INSTRUMENT]                                 | [KIND]              | [UNIT] | [RECORDS]                                 |
| :-----: | :------------------------------------------- | :------------------ | :----: | :---------------------------------------- |
|  [01]   | `resilience.polly.strategy.events`           | `Counter<int>`      |  none  | one resilience event raised by a strategy |
|  [02]   | `resilience.polly.strategy.attempt.duration` | `Histogram<double>` |  `ms`  | one execution attempt                     |
|  [03]   | `resilience.polly.pipeline.duration`         | `Histogram<double>` |  `ms`  | one whole-pipeline execution              |

- Emitted dimensions are `event.name`, `event.severity`, `pipeline.name`, `pipeline.instance`, `strategy.name`, `operation.key`, and `exception.type`, with `attempt.number` and `attempt.handled` on the attempt histogram alone; each nullable dimension is OMITTED rather than defaulted when its source is null, and `exception.type` renders the fault's full type name.
- `pipeline.name` and `pipeline.instance` render the registry key through `BuilderNameFormatter` and `InstanceNameFormatter`, and the instance formatter defaults to null — so an instance dimension exists only where a formatter row lands.
- `strategy.name` carries the options row's `Name`, which every shipped options ctor STAMPS with its own strategy kind, so an unnamed row publishes `Retry`, `Timeout`, `CircuitBreaker`, `Hedging`, `Fallback`, `RateLimiter`, or `Chaos.<plane>` rather than an absent dimension — and two rows of one kind in one pipeline MERGE their series under that shared spelling until a distinct `Name` separates them.
- `operation.key` carries the `ResilienceContext` operation key, so a pooled context fetched without one publishes every execution under an absent dimension.
- `event.name` VARIES on the counter alone: the listener dispatches on the argument type, so the two duration histograms stamp the constants `PipelineExecuted` and `ExecutionAttempt` and a reader partitioning either grades one bucket — chaos and every other named event reach the counter arm and nowhere else.
- Chaos events carry the `On` prefix their strategy names drop, so `Chaos.OnLatency`, `Chaos.OnFault`, `Chaos.OnOutcome`, and `Chaos.OnBehavior` are `event.name` values while `Chaos.Latency` and its siblings are `strategy.name` values; the two never interchange at a partition.
- `Polly.Telemetry.ResilienceTelemetryTags` holds every key as an `internal` const, so a consumer spells each as a wire literal and no bindable member exists; `strategy.type` and `error.type` appear nowhere in the assembly.
- Instrument writes short-circuit on the instrument's own `Enabled` flag, so an unsubscribed scope skips tag construction entirely and a configured-but-unadmitted pipeline pays nothing.

[STACKING]:
- `Polly.Core`(`.api/api-polly-core.md`): `ConfigureTelemetry<TBuilder>` binds any `ResiliencePipelineBuilder`/`ResiliencePipelineBuilder<T>`; `SeverityProviderArguments` carries the emitted `ResilienceEvent`/`ResilienceTelemetrySource`, and `AddResiliencePipelineRegistry<TKey>` wires the `ResiliencePipelineRegistry<TKey>`/`ResiliencePipelineProvider<TKey>` keyed-resolution surface.
- `OpenTelemetry`(`libs/csharp/.api/api-opentelemetry.md`): `Observability/telemetry#TELEMETRY_IDENTITY` admits the `Polly` scope to `AddMeter` through its `ForeignSource` row, which is the whole path this meter takes to a `MeterProvider`; `MeteringEnricher.Enrich` then appends `EnrichmentContext.Tags` as dimensions on the admitted streams, and `TelemetryOptions.LoggerFactory` binds the observability logging rail.
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
