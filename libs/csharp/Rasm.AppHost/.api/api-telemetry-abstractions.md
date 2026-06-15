# [RASM_APPHOST_API_TELEMETRY_ABSTRACTIONS]

`Microsoft.Extensions.Telemetry.Abstractions` admits structured logging attributes,
log enrichment contracts, log buffering contracts, latency context measurement, metric
generator attributes, and outgoing request metadata into the observability rail.

## [1]-[PACKAGE_SURFACE]

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

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: structured logging family
- rail: observability

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]      | [CAPABILITY]                           |
| :-----: | :--------------------------- | :------------------ | :------------------------------------- |
|   [1]   | `LogPropertiesAttribute`     | generator attribute | expands object properties into tags    |
|   [2]   | `LogPropertyIgnoreAttribute` | generator attribute | excludes a property from expansion     |
|   [3]   | `TagProviderAttribute`       | generator attribute | custom tag projection method           |
|   [4]   | `TagNameAttribute`           | generator attribute | renames an emitted tag                 |
|   [5]   | `LoggerMessageState`         | state carrier       | tags plus classified-tag redaction set |
|   [6]   | `ITagCollector`              | collector contract  | tag emission target                    |
|   [7]   | `LoggingSampler`             | sampler base        | per-entry sampling decision            |

[PUBLIC_TYPE_SCOPE]: buffering and enrichment family
- rail: observability

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]     | [CAPABILITY]                     |
| :-----: | :------------------------ | :----------------- | :------------------------------- |
|   [1]   | `LogBuffer`               | buffer base        | enqueue and flush contract       |
|   [2]   | `GlobalLogBuffer`         | buffer base        | process-global buffering         |
|   [3]   | `PerRequestLogBuffer`     | buffer base        | request-scoped buffering         |
|   [4]   | `ILogEnricher`            | enricher contract  | per-record tag enrichment        |
|   [5]   | `IStaticLogEnricher`      | enricher contract  | once-per-provider tag enrichment |
|   [6]   | `IEnrichmentTagCollector` | collector contract | enrichment tag emission target   |

[PUBLIC_TYPE_SCOPE]: latency context family
- rail: observability

| [INDEX] | [SYMBOL]                                        | [PACKAGE_ROLE]    | [CAPABILITY]                           |
| :-----: | :---------------------------------------------- | :---------------- | :------------------------------------- |
|   [1]   | `ILatencyContext`                               | context contract  | checkpoint, measure, and tag recording |
|   [2]   | `ILatencyContextProvider`                       | provider contract | context creation                       |
|   [3]   | `ILatencyContextTokenIssuer`                    | issuer contract   | name-to-token resolution               |
|   [4]   | `ILatencyDataExporter`                          | exporter contract | latency data export                    |
|   [5]   | `LatencyData`                                   | data value        | tags, checkpoints, measures spans      |
|   [6]   | `Checkpoint` / `Measure` / `Tag`                | sample values     | named latency samples                  |
|   [7]   | `CheckpointToken` / `MeasureToken` / `TagToken` | token values      | pre-registered name handles            |
|   [8]   | `NullLatencyContext`                            | null object       | no-op latency context                  |

[PUBLIC_TYPE_SCOPE]: metrics and request metadata family
- rail: observability

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]      | [CAPABILITY]                     |
| :-----: | :-------------------------------- | :------------------ | :------------------------------- |
|   [1]   | `CounterAttribute`                | generator attribute | strongly typed counter factory   |
|   [2]   | `GaugeAttribute`                  | generator attribute | strongly typed gauge factory     |
|   [3]   | `HistogramAttribute`              | generator attribute | strongly typed histogram factory |
|   [4]   | `RequestMetadata`                 | metadata value      | outgoing request route metadata  |
|   [5]   | `IOutgoingRequestContext`         | context contract    | ambient request metadata         |
|   [6]   | `IDownstreamDependencyMetadata`   | metadata contract   | dependency route declaration     |
|   [7]   | `HttpRouteParameterRedactionMode` | redaction mode enum | route parameter redaction policy |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: service registration
- rail: observability

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                     | [CAPABILITY]                   |
| :-----: | :------------------------ | :------------------------------- | :----------------------------- |
|   [1]   | `AddLogEnricher<T>`       | generic or instance registration | admits per-record enricher     |
|   [2]   | `AddStaticLogEnricher<T>` | generic or instance registration | admits static enricher         |
|   [3]   | `RegisterCheckpointNames` | `params string[]` names          | pre-registers checkpoint names |
|   [4]   | `RegisterMeasureNames`    | `params string[]` names          | pre-registers measure names    |
|   [5]   | `RegisterTagNames`        | `params string[]` names          | pre-registers tag names        |
|   [6]   | `AddNullLatencyContext`   | `IServiceCollection` extension   | installs no-op latency context |

[ENTRYPOINT_SCOPE]: runtime operations
- rail: observability

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                              | [CAPABILITY]                       |
| :-----: | :------------------------------ | :----------------------------------------------------------------------- | :--------------------------------- |
|   [1]   | `TryEnqueue`                    | buffered logger plus log entry                                           | buffers a log record               |
|   [2]   | `Flush`                         | buffer command                                                          | replays buffered records           |
|   [3]   | `ShouldSample`                  | `in LogEntry<TState>`                                                    | per-entry sampling decision        |
|   [4]   | `AddCheckpoint`                 | `CheckpointToken`                                                        | records a latency checkpoint       |
|   [5]   | `AddMeasure`                    | token plus value                                                        | accumulates a latency measure      |
|   [6]   | `RecordMeasure`                 | token plus value                                                        | sets a latency measure             |
|   [7]   | `SetTag`                        | token plus value                                                        | tags the latency context           |
|   [8]   | `Freeze`                        | context command                                                        | seals latency data for export      |
|   [9]   | `SetRequestMetadata`            | `IOutgoingRequestContext.SetRequestMetadata(RequestMetadata)`           | sets outgoing request route        |
|  [10]   | `AddDownstreamDependencyMetadata` | `HttpDiagnosticsServiceCollectionExtensions.AddDownstreamDependencyMetadata(IServiceCollection, IDownstreamDependencyMetadata)` -> `IServiceCollection` | registers dependency route metadata |

## [4]-[IMPLEMENTATION_LAW]

[ABSTRACTION_TOPOLOGY]:
- attribute surface: `[LogProperties]`, `[TagProvider]`, `[TagName]`, and metric attributes drive source generators
- redaction surface: `LoggerMessageState.ClassifiedTag` carries data classification for redaction-aware sinks; `HttpRouteParameterRedactionMode` scopes route parameter redaction
- enrichment surface: `ILogEnricher` and `IStaticLogEnricher` feed `IEnrichmentTagCollector`
- latency surface: token issuance, context recording, freeze, and exporter contracts
- request-metadata surface: `RequestMetadata` (namespace `Microsoft.Extensions.Http.Diagnostics`) is a `class` with settable `string RequestRoute` (default `"unknown"`), `string RequestName` (default `"unknown"`), `string DependencyName` (default `"unknown"`), and `string MethodType` (default `"GET"`) members, a parameterless constructor, and a `(string methodType, string requestRoute, string requestName = "unknown")` constructor; `IOutgoingRequestContext` carries `RequestMetadata? RequestMetadata { get; }` and `void SetRequestMetadata(RequestMetadata metadata)`; `IDownstreamDependencyMetadata` carries `string DependencyName`, `ISet<string> UniqueHostNameSuffixes`, and `ISet<RequestMetadata> RequestMetadata`
- dependency-registration surface: the `AddDownstreamDependencyMetadata` DI registration is owned by package `Microsoft.Extensions.Http.Diagnostics` on `Microsoft.Extensions.DependencyInjection.HttpDiagnosticsServiceCollectionExtensions`, with overloads `(IServiceCollection, IDownstreamDependencyMetadata)` and `(IServiceCollection)` (generic-derived), both returning `IServiceCollection`
- implementation split: policy implementations and enricher registrations live in `Microsoft.Extensions.Telemetry`, where `AddApplicationLogEnricher` is current and `AddServiceLogEnricher` is its obsolete predecessor

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
