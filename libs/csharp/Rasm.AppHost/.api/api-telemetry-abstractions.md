# [RASM_APPHOST_API_TELEMETRY_ABSTRACTIONS]

`Microsoft.Extensions.Telemetry.Abstractions` admits structured logging attributes, log enrichment contracts, log buffering contracts, latency context measurement, metric generator attributes, and outgoing request metadata into the observability rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Telemetry.Abstractions`
- package: `Microsoft.Extensions.Telemetry.Abstractions`
- assembly: `Microsoft.Extensions.Telemetry.Abstractions`
- namespace: `Microsoft.Extensions.Logging`
- namespace: `Microsoft.Extensions.Diagnostics.Buffering`
- namespace: `Microsoft.Extensions.Diagnostics.Enrichment`
- namespace: `Microsoft.Extensions.Diagnostics.Latency`
- namespace: `Microsoft.Extensions.Diagnostics.Metrics`
- namespace: `Microsoft.Extensions.Http.Diagnostics`
- namespace: `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: structured logging family
- rail: observability

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]      | [CAPABILITY]                           |
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
- rail: observability

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]     | [CAPABILITY]                     |
| :-----: | :------------------------ | :----------------- | :------------------------------- |
|  [01]   | `LogBuffer`               | buffer base        | enqueue and flush contract       |
|  [02]   | `GlobalLogBuffer`         | buffer base        | process-global buffering         |
|  [03]   | `PerRequestLogBuffer`     | buffer base        | request-scoped buffering         |
|  [04]   | `ILogEnricher`            | enricher contract  | per-record tag enrichment        |
|  [05]   | `IStaticLogEnricher`      | enricher contract  | once-per-provider tag enrichment |
|  [06]   | `IEnrichmentTagCollector` | collector contract | enrichment tag emission target   |

[PUBLIC_TYPE_SCOPE]: latency context family
- rail: observability

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE]    | [CAPABILITY]                                        |
| :-----: | :---------------------------------- | :---------------- | :-------------------------------------------------- |
|  [01]   | `ILatencyContext`                   | context contract  | checkpoint, measure, and tag recording              |
|  [02]   | `ILatencyContextProvider`           | provider contract | context creation                                    |
|  [03]   | `ILatencyContextTokenIssuer`        | issuer contract   | name-to-token resolution                            |
|  [04]   | `ILatencyDataExporter`              | exporter contract | latency data export                                 |
|  [05]   | `LatencyData`                       | data value        | tags, checkpoints, and measures spans with duration |
|  [06]   | `Checkpoint`                        | sample value      | named checkpoint with elapsed and frequency         |
|  [07]   | `Measure`                           | sample value      | named latency measure value                         |
|  [08]   | `Tag`                               | sample value      | named latency tag value                             |
|  [09]   | `CheckpointToken`                   | token value       | pre-registered checkpoint handle                    |
|  [10]   | `MeasureToken`                      | token value       | pre-registered measure handle                       |
|  [11]   | `TagToken`                          | token value       | pre-registered tag handle                           |
|  [12]   | `NullLatencyContext`                | null object       | no-op latency context                               |
|  [13]   | `LatencyContextRegistrationOptions` | options carrier   | checkpoint, measure, and tag name registry          |

[PUBLIC_TYPE_SCOPE]: metrics and request metadata family
- rail: observability

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]      | [CAPABILITY]                                 |
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

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: service registration
- rail: observability

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                     | [CAPABILITY]                   |
| :-----: | :------------------------ | :------------------------------- | :----------------------------- |
|  [01]   | `AddLogEnricher<T>`       | generic or instance registration | admits per-record enricher     |
|  [02]   | `AddStaticLogEnricher<T>` | generic or instance registration | admits static enricher         |
|  [03]   | `RegisterCheckpointNames` | `params string[]` names          | pre-registers checkpoint names |
|  [04]   | `RegisterMeasureNames`    | `params string[]` names          | pre-registers measure names    |
|  [05]   | `RegisterTagNames`        | `params string[]` names          | pre-registers tag names        |
|  [06]   | `AddNullLatencyContext`   | `IServiceCollection` extension   | installs no-op latency context |

[ENTRYPOINT_SCOPE]: buffering runtime operations
- rail: observability

[BUFFERING_RUNTIME]:

| [INDEX] | [SURFACE]      | [CALL_SHAPE]          | [CAPABILITY]                |
| :-----: | :------------- | :-------------------- | :-------------------------- |
|  [01]   | `TryEnqueue`   | buffered log entry    | buffers a log record        |
|  [02]   | `Flush`        | buffer command        | replays buffered records    |
|  [03]   | `ShouldSample` | `in LogEntry<TState>` | per-entry sampling decision |

[LATENCY_RUNTIME]:

| [INDEX] | [SURFACE]            | [CALL_SHAPE]     | [CAPABILITY]                  |
| :-----: | :------------------- | :--------------- | :---------------------------- |
|  [01]   | `GetCheckpointToken` | name lookup      | resolves checkpoint token     |
|  [02]   | `GetMeasureToken`    | name lookup      | resolves measure token        |
|  [03]   | `GetTagToken`        | name lookup      | resolves tag token            |
|  [04]   | `AddCheckpoint`      | checkpoint token | records a latency checkpoint  |
|  [05]   | `AddMeasure`         | token and value  | accumulates a latency measure |
|  [06]   | `RecordMeasure`      | token and value  | sets a latency measure        |
|  [07]   | `SetTag`             | token and value  | tags the latency context      |
|  [08]   | `Freeze`             | context command  | seals latency data for export |

[REQUEST_METADATA_RUNTIME]:

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]         | [CAPABILITY]                        |
| :-----: | :-------------------------------- | :------------------- | :---------------------------------- |
|  [01]   | `SetRequestMetadata`              | context mutation     | sets outgoing request route         |
|  [02]   | `AddDownstreamDependencyMetadata` | service registration | registers dependency route metadata |

## [04]-[IMPLEMENTATION_LAW]

[ABSTRACTION_TOPOLOGY]:
- attribute surface: `[LogProperties]`, `[TagProvider]`, `[TagName]`, and metric attributes (`[Counter]`, `[Gauge]`, `[Histogram]`, `[TagName]`) drive source generators; generated log methods marshal tags through `LoggerMessageHelper.ThreadLocalState`, a thread-local `LoggerMessageState`
- redaction surface: `LoggerMessageState.ClassifiedTag` (`Name`, `Value`, `DataClassificationSet Classifications`) carries data classification for redaction-aware sinks; `HttpRouteParameterRedactionMode` scopes route parameter redaction
- enrichment surface: `ILogEnricher` and `IStaticLogEnricher` feed `IEnrichmentTagCollector`
- latency surface: names register once at composition (`RegisterCheckpointNames`/`RegisterMeasureNames`/`RegisterTagNames`) into `LatencyContextRegistrationOptions` (`CheckpointNames`/`MeasureNames`/`TagNames`); `ILatencyContextTokenIssuer.GetCheckpointToken`/`GetMeasureToken`/`GetTagToken` resolve each name to its token; runtime code records through the resolved tokens, then `Freeze` and the `ILatencyDataExporter.ExportAsync(LatencyData, CancellationToken)` exporter contract close the seam; `LatencyData` exposes `Checkpoints`/`Measures`/`Tags` spans with `DurationTimestamp` and `DurationTimestampFrequency`, where `Checkpoint` carries `Name`/`Elapsed`/`Frequency`, `Measure` carries `Name`/`Value`, and `Tag` carries `Name`/`Value`
- request-metadata surface: `RequestMetadata` (namespace `Microsoft.Extensions.Http.Diagnostics`) is a `class` with settable `string RequestRoute` (default `"unknown"`), `string RequestName` (default `"unknown"`), `string DependencyName` (default `"unknown"`), and `string MethodType` (default `"GET"`) members, a parameterless constructor, and a `(string methodType, string requestRoute, string requestName = "unknown")` constructor; `IOutgoingRequestContext` carries `RequestMetadata? RequestMetadata { get; }` and `void SetRequestMetadata(RequestMetadata metadata)`; `IDownstreamDependencyMetadata` carries `string DependencyName`, `ISet<string> UniqueHostNameSuffixes`, and `ISet<RequestMetadata> RequestMetadata`; `TelemetryConstants.RequestMetadataKey` (`"Extensions-RequestMetadata"`) names the ambient metadata slot, with `Unknown`/`Redacted` redaction sentinels
- dependency-registration surface: the `AddDownstreamDependencyMetadata` DI registration is owned by package `Microsoft.Extensions.Http.Diagnostics` on `Microsoft.Extensions.DependencyInjection.HttpDiagnosticsServiceCollectionExtensions`, with overloads `(IServiceCollection, IDownstreamDependencyMetadata)` and `(IServiceCollection)` (generic-derived), both returning `IServiceCollection`
- implementation split: policy implementations and enricher registrations live in `Microsoft.Extensions.Telemetry`; contracts and generator attributes stay in this package

[LOCAL_ADMISSION]:
- Generator attributes annotate logging surfaces at the owning module; tags stay bounded dimensions.
- Latency names are pre-registered at composition; runtime code records through tokens only.
- Buffering and sampling contracts are policy seams; sinks and rules bind in the implementation package.
- Request metadata is boundary material for outgoing dependency calls, not domain state.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Telemetry.Abstractions`
- Owns: telemetry contracts and generator attributes
- Accept: enrichment, buffering, sampling, latency, and metric contracts as composition seams
- Reject: call-site telemetry policy and unclassified sensitive tags
