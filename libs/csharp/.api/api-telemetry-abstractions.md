# [RASM_API_TELEMETRY_ABSTRACTIONS]

`Microsoft.Extensions.Telemetry.Abstractions` holds the contract half of the first-party telemetry rail every instrumented library binds: the generator grammar turning a `[LoggerMessage]` partial into classified tag emission and a partial factory into a typed `Counter`/`Gauge`/`Histogram`, the enricher, buffer, and sampler seams a composition root fills, the pooled latency ledger timing in-flight phases, and the outgoing-request metadata a transport boundary stamps. Every activation verb lives in `Microsoft.Extensions.Telemetry`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Telemetry.Abstractions`
- package: `Microsoft.Extensions.Telemetry.Abstractions` (MIT)
- assembly: `Microsoft.Extensions.Telemetry.Abstractions.dll`
- asset: runtime library; the shipped Roslyn generators emit the logging and metric partials
- namespace: `Microsoft.Extensions.Logging`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Diagnostics.Buffering`, `Microsoft.Extensions.Diagnostics.Enrichment`, `Microsoft.Extensions.Diagnostics.Latency`, `Microsoft.Extensions.Diagnostics.Metrics`, `Microsoft.Extensions.Http.Diagnostics`
- rail: library-tier telemetry contract behind every governed log record, generated instrument, and timed phase
- ruled gate: `EXTEXP0003` gates `LogPropertiesAttribute.Transitive`, `GaugeAttribute<T>` whole, and the `Unit` row on every metric attribute

## [02]-[PUBLIC_TYPES]

[GRAMMAR_TYPES]: log emission grammar and its pooled tag-collection state

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]                                     |
| :-----: | :--------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `LogPropertiesAttribute`     | attribute      | expands one parameter's members into tags        |
|  [02]   | `LogPropertyIgnoreAttribute` | attribute      | drops one member from the expansion              |
|  [03]   | `TagProviderAttribute`       | attribute      | routes an unannotatable type to a method         |
|  [04]   | `TagNameAttribute`           | attribute      | renames one tag at its declaration               |
|  [05]   | `ITagCollector`              | interface      | provider-method write seam, classified overload  |
|  [06]   | `LoggerMessageState`         | class          | pooled per-record tag and classified-tag state   |
|  [07]   | `LoggerMessageHelper`        | static class   | thread-local state seat and enumerable stringify |
|  [08]   | `LoggingSampler`             | abstract class | per-entry sample verdict a host subclasses       |

- `LogPropertiesAttribute`: `SkipNullProperties` `OmitReferenceName` `Transitive`
- `TagProviderAttribute(Type, string)`: `ProviderType` `ProviderMethod` `OmitReferenceName`
- `LoggerMessageState` implements `IEnrichmentTagCollector` and `IReadOnlyList<KeyValuePair<string, object?>>`, carrying `TagsCount` `ClassifiedTagsCount` `TagNamePrefix`
- `LoggerMessageState.ClassifiedTag`: nested readonly struct over `Name` `Value` `Classifications`
- `LoggerMessageHelper.ThreadLocalState`: `[ThreadStatic]` `LoggerMessageState` the generated method rents and clears per record

[METRIC_TYPES]: the generated instrument-factory grammar

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `CounterAttribute` / `CounterAttribute<T>`     | attribute     | generates a typed counter factory off a partial   |
|  [02]   | `GaugeAttribute` / `GaugeAttribute<T>`         | attribute     | generates a typed gauge factory off a partial     |
|  [03]   | `HistogramAttribute` / `HistogramAttribute<T>` | attribute     | generates a typed histogram factory off a partial |
|  [04]   | `TagNameAttribute`                             | attribute     | renames one metric tag dimension                  |

- Each metric attribute carries `Name` `Unit` and, off its two constructors, either a `TagNames` string roster or a strongly typed `Type` whose members become the dimensions.
- Generic arms fix the measurement type, and `Microsoft.Extensions.Diagnostics.Metrics.TagNameAttribute` stands distinct from the `Microsoft.Extensions.Logging` one, so a file consuming both disambiguates by namespace.

[GOVERNANCE_TYPES]: the buffering and enrichment seams a composition root fills

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [CAPABILITY]                                    |
| :-----: | :------------------------ | :------------- | :---------------------------------------------- |
|  [01]   | `LogBuffer`               | abstract class | flush and enqueue contract                      |
|  [02]   | `GlobalLogBuffer`         | abstract class | process-wide ring the composition root resolves |
|  [03]   | `PerRequestLogBuffer`     | abstract class | request-scoped ring a pipeline host resolves    |
|  [04]   | `ILogEnricher`            | interface      | per-record tag projection                       |
|  [05]   | `IStaticLogEnricher`      | interface      | per-provider tag projection                     |
|  [06]   | `IEnrichmentTagCollector` | interface      | enricher write seam                             |

[LATENCY_TYPES]: the in-flight latency ledger

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]   | [CAPABILITY]                                    |
| :-----: | :---------------------------------- | :-------------- | :---------------------------------------------- |
|  [01]   | `ILatencyContext`                   | interface       | one operation's checkpoint, measure, tag ledger |
|  [02]   | `ILatencyContextProvider`           | interface       | per-operation context mint off the pool         |
|  [03]   | `ILatencyContextTokenIssuer`        | interface       | resolves a registered name to its token         |
|  [04]   | `ILatencyDataExporter`              | interface       | frozen-ledger export contract                   |
|  [05]   | `LatencyData`                       | readonly struct | frozen span set with its duration basis         |
|  [06]   | `Checkpoint` / `Measure` / `Tag`    | readonly struct | the three frozen sample kinds                   |
|  [07]   | `CheckpointToken`                   | readonly struct | phase-stamp handle                              |
|  [08]   | `MeasureToken`                      | readonly struct | accumulator handle                              |
|  [09]   | `TagToken`                          | readonly struct | pivot-dimension handle                          |
|  [10]   | `LatencyContextRegistrationOptions` | options         | the registered name vocabulary                  |

- `ILatencyContext` extends `IDisposable`, so a context returns to its pool at the `using` boundary and a leaked context starves the pool.
- `CheckpointToken`, `MeasureToken`, and `TagToken`: `Name` `Position`
- `LatencyData`: `Checkpoints` `Tags` `Measures` project as `ReadOnlySpan<T>` over pooled `ArraySegment<T>` backing, beside `DurationTimestamp` `DurationTimestampFrequency`
- `Checkpoint`: `Name` `Elapsed` `Frequency`; `Measure`: `Name` `Value` (`long`); `Tag`: `Name` `Value` (`string`)
- `LatencyContextRegistrationOptions`: `CheckpointNames` `MeasureNames` `TagNames`

[REQUEST_TYPES]: outgoing-request metadata and its redaction posture

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :-------------------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `RequestMetadata`                 | class         | one outgoing call's route and dependency name |
|  [02]   | `IOutgoingRequestContext`         | interface     | ambient per-call metadata slot                |
|  [03]   | `IDownstreamDependencyMetadata`   | interface     | declares a dependency's route set             |
|  [04]   | `HttpRouteParameterRedactionMode` | enum          | `Strict` \| `Loose` \| `None`                 |
|  [05]   | `TelemetryConstants`              | constant set  | metadata key, sentinels, and the two headers  |

- `RequestMetadata`: `RequestRoute` `RequestName` `DependencyName` each default `"unknown"`, `MethodType` defaults `"GET"`; the three-arg constructor takes `(methodType, requestRoute, requestName)`.
- `IDownstreamDependencyMetadata`: `DependencyName` `UniqueHostNameSuffixes` `RequestMetadata` route set.
- `TelemetryConstants`: `RequestMetadataKey` (`"Extensions-RequestMetadata"`) `Unknown` `Redacted` `ClientApplicationNameHeader` `ServerApplicationNameHeader`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `IServiceCollection` registration

| [INDEX] | [SURFACE]                                    | [SHAPE] | [CAPABILITY]                          |
| :-----: | :------------------------------------------- | :------ | :------------------------------------ |
|  [01]   | `AddLogEnricher<T>() where T : ILogEnricher` | static  | DI-activated per-record enricher      |
|  [02]   | `AddLogEnricher(ILogEnricher)`               | static  | pre-constructed per-record enricher   |
|  [03]   | `AddStaticLogEnricher<T>()`                  | static  | DI-activated per-provider enricher    |
|  [04]   | `AddStaticLogEnricher(IStaticLogEnricher)`   | static  | pre-constructed per-provider enricher |
|  [05]   | `RegisterCheckpointNames(params string[])`   | static  | checkpoint vocabulary                 |
|  [06]   | `RegisterMeasureNames(params string[])`      | static  | measure vocabulary                    |
|  [07]   | `RegisterTagNames(params string[])`          | static  | pivot vocabulary                      |
|  [08]   | `AddNullLatencyContext()`                    | static  | seats the no-op ledger                |

[ENTRYPOINT_SCOPE]: record, ledger, and export verbs

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `ILogEnricher.Enrich(IEnrichmentTagCollector)`                                | instance | writes one record's tags                |
|  [02]   | `IEnrichmentTagCollector.Add(string, object)`                                 | instance | one enriched tag                        |
|  [03]   | `ITagCollector.Add(string, object?, DataClassificationSet)`                   | instance | one classified provider tag             |
|  [04]   | `LoggerMessageState.AddClassifiedTag(string, object?, DataClassificationSet)` | instance | one classified tag onto pooled state    |
|  [05]   | `LoggerMessageState.ReserveTagSpace(int)`                                     | instance | pre-sizes the pooled tag array          |
|  [06]   | `LoggerMessageHelper.Stringify(IEnumerable?)`                                 | static   | renders an enumerable tag value         |
|  [07]   | `LoggingSampler.ShouldSample<TState>(in LogEntry<TState>) -> bool`            | instance | the custom sampler verdict              |
|  [08]   | `LogBuffer.TryEnqueue<TState>(IBufferedLogger, in LogEntry<TState>)`          | instance | admits one record to the ring           |
|  [09]   | `LogBuffer.Flush()`                                                           | instance | replays held records on incident        |
|  [10]   | `ILatencyContextProvider.CreateContext() -> ILatencyContext`                  | instance | one context per operation               |
|  [11]   | `ILatencyContextTokenIssuer.GetCheckpointToken(string)`                       | instance | resolves a phase name once              |
|  [12]   | `ILatencyContextTokenIssuer.GetMeasureToken(string)`                          | instance | resolves a measure name once            |
|  [13]   | `ILatencyContextTokenIssuer.GetTagToken(string)`                              | instance | resolves a pivot name once              |
|  [14]   | `ILatencyContext.AddCheckpoint(CheckpointToken)`                              | instance | stamps one phase boundary               |
|  [15]   | `ILatencyContext.AddMeasure(MeasureToken, long)`                              | instance | accumulates into a measure              |
|  [16]   | `ILatencyContext.RecordMeasure(MeasureToken, long)`                           | instance | sets a measure absolutely               |
|  [17]   | `ILatencyContext.SetTag(TagToken, string)`                                    | instance | last write wins per tag                 |
|  [18]   | `ILatencyContext.Freeze()`                                                    | instance | seals the context against further state |
|  [19]   | `ILatencyContext.LatencyData`                                                 | property | the frozen span set                     |
|  [20]   | `ILatencyDataExporter.ExportAsync(LatencyData, CancellationToken)`            | instance | drains one frozen ledger                |
|  [21]   | `IOutgoingRequestContext.SetRequestMetadata(RequestMetadata)`                 | instance | stamps one outgoing call's route        |

- `ILatencyContext.AddCheckpoint`: one stamp per context, so a re-entrant phase records a measure instead.
- `ILatencyContextTokenIssuer` resolves an unregistered name to a positionless token whose writes drop, until `LatencyContextOptions.ThrowOnUnregisteredNames` promotes the lookup to a boot failure.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Logging and metric attributes drive generators: a generated log method rents `LoggerMessageHelper.ThreadLocalState`, writes tags and classified tags onto it, and returns it cleared, so a record costs no per-call allocation.
- Classified tags carry their `DataClassificationSet` to the sink, so redaction selects a redactor per classification before any provider observes the value.
- Latency names register once at composition into `LatencyContextRegistrationOptions`; runtime code resolves each name to a positional token, records through the token, then `Freeze` hands `LatencyData` to `ILatencyDataExporter.ExportAsync` — no name lookup and no allocation on the hot path.
- Enrichment, buffering, and sampling are contract-only seams here; each carries no bound rule until a composition root activates one.
- Request metadata is boundary material for an outgoing dependency call, carried in the `IOutgoingRequestContext` ambient slot keyed by `TelemetryConstants.RequestMetadataKey`, with `HttpRouteParameterRedactionMode` scoping how a route parameter survives into the record.

[STACKING]:
- `Microsoft.Extensions.Telemetry`(`Rasm.AppHost/.api/api-telemetry.md`): realizes every seam declared here — a sampler, enricher, buffer, or latency registration binds the concrete policy at the composition root while the contract and attributes stay library-tier.
- `Microsoft.Extensions.Logging.Abstractions`(`api-logging-abstractions.md`): `[LogProperties]`, `[TagProvider]`, and `[TagName]` ride the in-box `[LoggerMessage]` partial, so payload expansion, foreign projection, and tag naming land on one generated declaration, and `LoggingSampler` reads that package's `LogEntry<TState>`.
- `Microsoft.Extensions.Compliance.Redaction`(`api-redaction.md`): supplies the `DataClassificationSet` vocabulary the classified-tag overloads carry and the `Redactor` each classification selects.
- `Microsoft.Extensions.Http.Diagnostics`(`Rasm.AppHost/.api/api-http-diagnostics.md`): consumes `RequestMetadata` and `IDownstreamDependencyMetadata`, owns the dependency-route registration, and resolves the `HttpRouteParameterRedactionMode` its outbound logger applies.
- `System.Diagnostics.Metrics`(`api-diagnostics-metrics.md`): the generated `Counter`/`Gauge`/`Histogram` factories mint in-box instruments off an `IMeterFactory`-resolved `Meter`, so a generated instrument and a hand-minted one share one meter and one export path.
- `Rasm.Materials` `Projection/observability`: binds `ILatencyContext` as the checkpoint ledger over eager constructions; `Rasm.Rhino` and `Rasm.Grasshopper` bind the grammar attributes alone at their host capsules.

[LOCAL_ADMISSION]:
- Instrumented libraries reference this contract assembly alone — grammar attributes, enricher contracts, latency tokens — and every activation verb stays composition-root surface.
- Tags stay bounded dimensions, and smuggled domain payload on one is the defect the classification set exists to make visible.
- Latency names pre-register at composition and runtime records through tokens only.
- Generated metric factories name their instrument in the estate metric grammar, never a package-local spelling.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Telemetry.Abstractions`
- Owns: the source-generator log and metric grammar, the enricher, buffer, and sampler contracts, the latency ledger, and outgoing-request metadata
- Accept: attribute-declared emission on a library surface; contracts injected and filled at a composition root
- Reject: call-site telemetry policy; an unclassified sensitive tag; a `Stopwatch` timing a phase an issued token records
