# [RASM_APPHOST_API_TELEMETRY_ABSTRACTIONS]

`Microsoft.Extensions.Telemetry.Abstractions` owns the contract-and-attribute half of the observability rail: the source-generator attributes a log method or metric instrument is authored from, and the seam contracts a sink, enricher, sampler, or latency exporter binds against. Implementing policy binds in `Microsoft.Extensions.Telemetry`; this package ships the contracts and generators alone.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Telemetry.Abstractions`
- package: `Microsoft.Extensions.Telemetry.Abstractions`
- assembly: `Microsoft.Extensions.Telemetry.Abstractions`
- namespace: `Microsoft.Extensions.Logging`, `Microsoft.Extensions.Diagnostics.Buffering`, `Microsoft.Extensions.Diagnostics.Enrichment`, `Microsoft.Extensions.Diagnostics.Latency`, `Microsoft.Extensions.Diagnostics.Metrics`, `Microsoft.Extensions.Http.Diagnostics`, `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: structured logging family

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]       | [CAPABILITY]                           |
| :-----: | :--------------------------- | :------------------ | :------------------------------------- |
|  [01]   | `LogPropertiesAttribute`     | generator attribute | expands object properties into tags    |
|  [02]   | `LogPropertyIgnoreAttribute` | generator attribute | excludes a property from expansion     |
|  [03]   | `TagProviderAttribute`       | generator attribute | custom tag projection method           |
|  [04]   | `TagNameAttribute`           | generator attribute | renames an emitted log tag             |
|  [05]   | `LoggerMessageState`         | state carrier       | tags and classified-tag redaction set  |
|  [06]   | `LoggerMessageHelper`        | generator runtime   | thread-local state and value stringify |
|  [07]   | `ITagCollector`              | collector contract  | tag emission target                    |
|  [08]   | `LoggingSampler`             | sampler base        | per-entry sampling decision            |

[PUBLIC_TYPE_SCOPE]: buffering and enrichment family

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [CAPABILITY]                     |
| :-----: | :------------------------ | :----------------- | :------------------------------- |
|  [01]   | `LogBuffer`               | buffer base        | enqueue and flush contract       |
|  [02]   | `GlobalLogBuffer`         | buffer base        | process-global buffering         |
|  [03]   | `PerRequestLogBuffer`     | buffer base        | request-scoped buffering         |
|  [04]   | `ILogEnricher`            | enricher contract  | per-record tag enrichment        |
|  [05]   | `IStaticLogEnricher`      | enricher contract  | once-per-provider tag enrichment |
|  [06]   | `IEnrichmentTagCollector` | collector contract | enrichment tag emission target   |

[PUBLIC_TYPE_SCOPE]: latency context family

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]     | [CAPABILITY]                                |
| :-----: | :---------------------------------- | :---------------- | :------------------------------------------ |
|  [01]   | `ILatencyContext`                   | context contract  | checkpoint, measure, and tag recording      |
|  [02]   | `ILatencyContextProvider`           | provider contract | context creation                            |
|  [03]   | `ILatencyContextTokenIssuer`        | issuer contract   | name-to-token resolution                    |
|  [04]   | `ILatencyDataExporter`              | exporter contract | latency data export                         |
|  [05]   | `LatencyData`                       | data value        | checkpoint, measure, and tag spans          |
|  [06]   | `Checkpoint`                        | sample value      | named checkpoint with elapsed and frequency |
|  [07]   | `Measure`                           | sample value      | named latency measure value                 |
|  [08]   | `Tag`                               | sample value      | named latency tag value                     |
|  [09]   | `CheckpointToken`                   | token value       | pre-registered checkpoint handle            |
|  [10]   | `MeasureToken`                      | token value       | pre-registered measure handle               |
|  [11]   | `TagToken`                          | token value       | pre-registered tag handle                   |
|  [12]   | `NullLatencyContext`                | null object       | no-op latency context                       |
|  [13]   | `LatencyContextRegistrationOptions` | options carrier   | checkpoint, measure, and tag name registry  |

[PUBLIC_TYPE_SCOPE]: metrics and request metadata family

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]       | [CAPABILITY]                                 |
| :-----: | :-------------------------------- | :------------------ | :------------------------------------------- |
|  [01]   | `CounterAttribute`                | generator attribute | strongly typed counter factory               |
|  [02]   | `GaugeAttribute`                  | generator attribute | strongly typed gauge factory                 |
|  [03]   | `HistogramAttribute`              | generator attribute | strongly typed histogram factory             |
|  [04]   | `TagNameAttribute`                | generator attribute | renames a metric tag dimension               |
|  [05]   | `RequestMetadata`                 | metadata value      | outgoing request route metadata              |
|  [06]   | `IOutgoingRequestContext`         | context contract    | ambient request metadata                     |
|  [07]   | `IDownstreamDependencyMetadata`   | metadata contract   | dependency route declaration                 |
|  [08]   | `HttpRouteParameterRedactionMode` | redaction mode enum | route parameter redaction policy             |
|  [09]   | `TelemetryConstants`              | constant set        | request-metadata key and redaction sentinels |

- `RequestMetadata` route members: `RequestRoute`, `RequestName`, `DependencyName` default `"unknown"` and `MethodType` defaults `"GET"`; `IDownstreamDependencyMetadata` declares `DependencyName`, `UniqueHostNameSuffixes`, and a `RequestMetadata` route set.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: service registration

| [INDEX] | [SURFACE]                                  | [SHAPE] | [CAPABILITY]                   |
| :-----: | :----------------------------------------- | :------ | :----------------------------- |
|  [01]   | `AddLogEnricher<T>`                        | static  | admits per-record enricher     |
|  [02]   | `AddStaticLogEnricher<T>`                  | static  | admits static enricher         |
|  [03]   | `RegisterCheckpointNames(params string[])` | static  | pre-registers checkpoint names |
|  [04]   | `RegisterMeasureNames(params string[])`    | static  | pre-registers measure names    |
|  [05]   | `RegisterTagNames(params string[])`        | static  | pre-registers tag names        |
|  [06]   | `AddNullLatencyContext()`                  | static  | installs no-op latency context |

[ENTRYPOINT_SCOPE]: buffering and sampling runtime

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                |
| :-----: | :------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `TryEnqueue(IBufferedLogger, in LogEntry) -> bool` | instance | buffers a log record        |
|  [02]   | `Flush()`                                          | instance | replays buffered records    |
|  [03]   | `ShouldSample(in LogEntry) -> bool`                | instance | per-entry sampling decision |

[ENTRYPOINT_SCOPE]: latency context runtime

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :---------------------------------------------- | :------- | :---------------------------- |
|  [01]   | `GetCheckpointToken(string) -> CheckpointToken` | instance | resolves checkpoint token     |
|  [02]   | `GetMeasureToken(string) -> MeasureToken`       | instance | resolves measure token        |
|  [03]   | `GetTagToken(string) -> TagToken`               | instance | resolves tag token            |
|  [04]   | `AddCheckpoint(CheckpointToken)`                | instance | records a latency checkpoint  |
|  [05]   | `AddMeasure`                                    | instance | accumulates a latency measure |
|  [06]   | `RecordMeasure`                                 | instance | sets a latency measure        |
|  [07]   | `SetTag`                                        | instance | tags the latency context      |
|  [08]   | `Freeze()`                                      | instance | seals latency data for export |

[ENTRYPOINT_SCOPE]: request metadata runtime

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :------------------------------------ | :------- | :---------------------------------- |
|  [01]   | `SetRequestMetadata(RequestMetadata)` | instance | sets outgoing request route         |
|  [02]   | `AddDownstreamDependencyMetadata`     | static   | registers dependency route metadata |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Logging and metric attributes drive source generators; a generated log method marshals tags through `LoggerMessageHelper.ThreadLocalState`, a thread-local `LoggerMessageState`, and a classified tag carries its `DataClassificationSet` so a redaction-aware sink redacts before emit, with `HttpRouteParameterRedactionMode` scoping a route parameter.
- Latency names register once at composition into `LatencyContextRegistrationOptions`; runtime code resolves each name to a token, records through the token, then `Freeze` hands `LatencyData` to `ILatencyDataExporter.ExportAsync` — no name lookup on the hot path.
- Enrichment, buffering, and sampling are contract-only seams here; each carries no bound rule.
- Request metadata is boundary material for an outgoing dependency call, carried in the `IOutgoingRequestContext` ambient slot keyed by `TelemetryConstants.RequestMetadataKey`.

[STACKING]:
- `Microsoft.Extensions.Telemetry`(`api-telemetry.md`): implements these enrichment, buffering, sampling, redaction, and latency contracts — the policy binds there, the contract and attributes here.
- `Microsoft.Extensions.Http.Diagnostics`(`api-http-diagnostics.md`): consumes `RequestMetadata` and `IDownstreamDependencyMetadata`, owns the `AddDownstreamDependencyMetadata` DI registration, and fills the `IEnrichmentTagCollector` and resolves the `HttpRouteParameterRedactionMode` its outbound logger applies.
- `OpenTelemetry`(`api-otel.md`): the metric attributes' generated `Counter`/`Gauge`/`Histogram` instruments and enriched log tags project onto the meter and log provider pipelines.
- `Microsoft.Extensions.Logging.Abstractions`(`api-logging.md`): the source-generated log methods bind `ILogger` and `EventId` from the logging contract this generator extends.
- Observability composition root (`.planning/Observability/telemetry.md`): registers these contracts once, threading the latency context, enrichers, and sampler policy into the telemetry binding.

[LOCAL_ADMISSION]:
- Generator attributes annotate a logging or metric surface at its owning module; a tag stays a bounded dimension, never smuggled domain payload.
- Latency names pre-register at composition; runtime code records through tokens only.
- A buffering, sampling, or enrichment rule binds in `Microsoft.Extensions.Telemetry`, never at the log call site.
- Outgoing request metadata binds at the transport boundary, never as domain state.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Telemetry.Abstractions`
- Owns: the source-generator attributes and telemetry seam contracts
- Accept: enrichment, buffering, sampling, latency, and metric contracts as composition seams
- Reject: call-site telemetry policy and an unclassified sensitive tag
