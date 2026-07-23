# [RASM_API_EXTENSIONS_TELEMETRY]

`Microsoft.Extensions.Telemetry` governs log volume, buffering, enrichment, and redaction activation over the one `ILogger` seam, and mints the pooled latency ledger that times in-flight phases without a child span. Its contract assembly carries the emission grammar, the enricher and buffer contracts, and the latency tokens an instrumented library binds, so every activation verb stays composition-root surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Telemetry`
- package: `Microsoft.Extensions.Telemetry` (MIT)
- assembly: `Microsoft.Extensions.Telemetry`
- contract assembly: `Microsoft.Extensions.Telemetry.Abstractions`
- namespace: `Microsoft.Extensions.Logging`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Diagnostics.Enrichment`, `Microsoft.Extensions.Diagnostics.Latency`, `Microsoft.Extensions.Diagnostics.Buffering`, `Microsoft.Extensions.Diagnostics.Sampling`
- asset: runtime library
- rail: log governance
- ruled gate: `EXTEXP0003` on `LogPropertiesAttribute.Transitive`

## [02]-[PUBLIC_TYPES]

[GRAMMAR_TYPES]: emission grammar and its tag-collection state (contract assembly)

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :--------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `LogPropertiesAttribute`     | attribute     | expands one parameter's members into tags       |
|  [02]   | `LogPropertyIgnoreAttribute` | attribute     | drops one member from the expansion             |
|  [03]   | `TagProviderAttribute`       | attribute     | routes an unannotatable type to a method        |
|  [04]   | `TagNameAttribute`           | attribute     | renames one tag at its declaration              |
|  [05]   | `ITagCollector`              | interface     | provider-method write seam, classified overload |
|  [06]   | `LoggerMessageState`         | class         | pooled per-record tag and classified-tag state  |

- `LogPropertiesAttribute`: `SkipNullProperties` `OmitReferenceName` `Transitive`
- `TagProviderAttribute`: `ProviderType` `ProviderMethod` `OmitReferenceName`
- `LoggerMessageState.ClassifiedTag`: `Name` `Value` `Classifications`

[GOVERNANCE_TYPES]: sampling, buffering, enrichment, and redaction policy

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]  | [CAPABILITY]                                    |
| :-----: | :------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `LoggingSampler`                       | abstract class | per-entry sample verdict a host subclasses      |
|  [02]   | `RandomProbabilisticSamplerOptions`    | options        | `Rules` rule-row list                           |
|  [03]   | `RandomProbabilisticSamplerFilterRule` | rule row       | adds a `Probability` weight to the selector     |
|  [04]   | `GlobalLogBufferingOptions`            | options        | flush window, size caps, `Rules` rule-row list  |
|  [05]   | `LogBufferingFilterRule`               | rule row       | adds an `Attributes` predicate to the selector  |
|  [06]   | `LogBuffer`                            | abstract class | flush and enqueue contract                      |
|  [07]   | `GlobalLogBuffer`                      | abstract class | process-wide ring the composition root resolves |
|  [08]   | `PerRequestLogBuffer`                  | abstract class | request-scoped ring a pipeline host resolves    |
|  [09]   | `ILogEnricher`                         | interface      | per-record tag projection                       |
|  [10]   | `IStaticLogEnricher`                   | interface      | per-provider tag projection                     |
|  [11]   | `IEnrichmentTagCollector`              | interface      | enricher write seam                             |
|  [12]   | `ApplicationLogEnricherOptions`        | options        | service-identity tag switches                   |
|  [13]   | `ProcessLogEnricherOptions`            | options        | process and thread tag switches                 |
|  [14]   | `LoggerEnrichmentOptions`              | options        | exception-frame admission onto the log signal   |
|  [15]   | `LoggerRedactionOptions`               | options        | keys each redaction token by tag name           |

- `LogBufferingFilterRule` and `RandomProbabilisticSamplerFilterRule`: `CategoryName` `LogLevel` `EventId` `EventName`
- `GlobalLogBufferingOptions`: `AutoFlushDuration` `MaxLogRecordSizeInBytes` `MaxBufferSizeInBytes` `Rules`
- `ApplicationLogEnricherOptions`: `EnvironmentName` `ApplicationName` `DeploymentRing` `BuildVersion`
- `ProcessLogEnricherOptions`: `ProcessId` `ThreadId`
- `LoggerEnrichmentOptions`: `CaptureStackTraces` `UseFileInfoForStackTraces` `IncludeExceptionMessage` `MaxStackTraceLength`

[LATENCY_TYPES]: the in-flight latency ledger (contract assembly)

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :---------------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `ILatencyContext`                   | interface     | one operation's checkpoint, measure, tag ledger |
|  [02]   | `ILatencyContextProvider`           | interface     | per-operation context mint off the pool         |
|  [03]   | `ILatencyContextTokenIssuer`        | interface     | resolves a registered name to its token         |
|  [04]   | `CheckpointToken`                   | struct        | phase-stamp handle                              |
|  [05]   | `MeasureToken`                      | struct        | accumulator handle                              |
|  [06]   | `TagToken`                          | struct        | pivot-dimension handle                          |
|  [07]   | `LatencyData`                       | struct        | frozen span set with its duration basis         |
|  [08]   | `ILatencyDataExporter`              | interface     | frozen-ledger export contract                   |
|  [09]   | `NullLatencyContext`                | class         | no-op provider and issuer for headless hosts    |
|  [10]   | `LatencyContextRegistrationOptions` | options       | the registered name vocabulary                  |
|  [11]   | `LatencyContextOptions`             | options       | `ThrowOnUnregisteredNames` boot-fail switch     |
|  [12]   | `LatencyConsoleOptions`             | options       | console projection switches                     |

- `CheckpointToken`, `MeasureToken`, and `TagToken`: `Name` `Position`
- `LatencyData`: `Checkpoints` `Measures` `Tags` `DurationTimestamp` `DurationTimestampFrequency`
- `Checkpoint`: `Name` `Elapsed` `Frequency`; `Measure` and `Tag` carry `Name` `Value`
- `LatencyConsoleOptions`: `OutputCheckpoints` `OutputMeasures` `OutputTags`

## [03]-[ENTRYPOINTS]

Every options-bearing verb carries an `IConfiguration`/`IConfigurationSection` overload binding those options from configuration.

[ENTRYPOINT_SCOPE]: `ILoggingBuilder` activation

| [INDEX] | [SURFACE]                                                                  | [SHAPE] | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------------- | :------ | :------------------------------------- |
|  [01]   | `AddTraceBasedSampler()`                                                   | static  | slaves log volume to the trace verdict |
|  [02]   | `AddRandomProbabilisticSampler(double, LogLevel?)`                         | static  | one probability rule capped by level   |
|  [03]   | `AddRandomProbabilisticSampler(Action<RandomProbabilisticSamplerOptions>)` | static  | the full rule-row list                 |
|  [04]   | `AddSampler<T>()`                                                          | static  | DI-activated custom sampler            |
|  [05]   | `AddSampler(LoggingSampler)`                                               | static  | pre-constructed sampler instance       |
|  [06]   | `AddGlobalBuffer(LogLevel?)`                                               | static  | buffers at and below one level         |
|  [07]   | `AddGlobalBuffer(Action<GlobalLogBufferingOptions>)`                       | static  | rule rows, size caps, flush window     |
|  [08]   | `EnableEnrichment(Action<LoggerEnrichmentOptions>)`                        | static  | activates both enricher cost classes   |
|  [09]   | `EnableRedaction(Action<LoggerRedactionOptions>)`                          | static  | activates the redactor seam            |

- `AddGlobalBuffer`: a record over `MaxLogRecordSizeInBytes`, one matching no rule, or one inside the `AutoFlushDuration` window after a flush emits live.

[ENTRYPOINT_SCOPE]: `IServiceCollection` registration

| [INDEX] | [SURFACE]                                                          | [SHAPE] | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------------------- | :------ | :---------------------------------------- |
|  [01]   | `AddLogEnricher<T>()`                                              | static  | DI-activated per-record enricher          |
|  [02]   | `AddLogEnricher(ILogEnricher)`                                     | static  | pre-constructed per-record enricher       |
|  [03]   | `AddStaticLogEnricher<T>()`                                        | static  | DI-activated per-provider enricher        |
|  [04]   | `AddStaticLogEnricher(IStaticLogEnricher)`                         | static  | pre-constructed per-provider enricher     |
|  [05]   | `AddApplicationLogEnricher(Action<ApplicationLogEnricherOptions>)` | static  | shipped service-identity tag rows         |
|  [06]   | `AddProcessLogEnricher(Action<ProcessLogEnricherOptions>)`         | static  | shipped process and thread tag rows       |
|  [07]   | `AddLatencyContext(Action<LatencyContextOptions>)`                 | static  | registers provider, issuer, and pool      |
|  [08]   | `AddNullLatencyContext()`                                          | static  | seats the no-op ledger                    |
|  [09]   | `RegisterCheckpointNames(params string[])`                         | static  | checkpoint vocabulary                     |
|  [10]   | `RegisterMeasureNames(params string[])`                            | static  | measure vocabulary                        |
|  [11]   | `RegisterTagNames(params string[])`                                | static  | tag vocabulary                            |
|  [12]   | `AddConsoleLatencyDataExporter(Action<LatencyConsoleOptions>)`     | static  | console exporter for operator-local hosts |

[ENTRYPOINT_SCOPE]: record, ledger, and export verbs

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `ILogEnricher.Enrich(IEnrichmentTagCollector)`                     | instance | writes one record's tags                |
|  [02]   | `IEnrichmentTagCollector.Add(string, object)`                      | instance | one enriched tag                        |
|  [03]   | `ITagCollector.Add(string, object, DataClassificationSet)`         | instance | one classified provider tag             |
|  [04]   | `LoggingSampler.ShouldSample<TState>(LogEntry<TState>) -> bool`    | instance | the custom sampler verdict              |
|  [05]   | `LogBuffer.TryEnqueue<TState>(IBufferedLogger, LogEntry<TState>)`  | instance | admits one record to the ring           |
|  [06]   | `GlobalLogBuffer.Flush()`                                          | instance | replays held records on incident        |
|  [07]   | `ILatencyContextProvider.CreateContext() -> ILatencyContext`       | instance | one context per operation               |
|  [08]   | `ILatencyContextTokenIssuer.GetCheckpointToken(string)`            | instance | resolves a phase name once              |
|  [09]   | `ILatencyContextTokenIssuer.GetMeasureToken(string)`               | instance | resolves a measure name once            |
|  [10]   | `ILatencyContextTokenIssuer.GetTagToken(string)`                   | instance | resolves a pivot name once              |
|  [11]   | `ILatencyContext.AddCheckpoint(CheckpointToken)`                   | instance | stamps one phase boundary               |
|  [12]   | `ILatencyContext.AddMeasure(MeasureToken, long)`                   | instance | accumulates into a measure              |
|  [13]   | `ILatencyContext.RecordMeasure(MeasureToken, long)`                | instance | sets a measure absolutely               |
|  [14]   | `ILatencyContext.SetTag(TagToken, string)`                         | instance | last write wins per tag                 |
|  [15]   | `ILatencyContext.Freeze()`                                         | instance | seals the context against further state |
|  [16]   | `ILatencyContext.LatencyData`                                      | property | the frozen span set                     |
|  [17]   | `ILatencyDataExporter.ExportAsync(LatencyData, CancellationToken)` | instance | drains one frozen ledger                |

- `ILatencyContext.AddCheckpoint`: one stamp per context, so a re-entrant phase records a measure instead.
- `ILatencyContextTokenIssuer`: an unregistered name yields a positionless token whose writes drop, until `LatencyContextOptions.ThrowOnUnregisteredNames` promotes the lookup to a boot failure.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every activation verb seats one `ExtendedLoggerFactory`, so governance applies once and every registered provider observes the same record.
- `AddTraceBasedSampler` declares once what the trace root decided, so logs and spans rise and fall as one population; a probability rule thins the chatty floor by maximum level and never the error ceiling.
- Sampler and buffer rows select on category, level, and event identity, buffer rows adding an attribute predicate; a category matching no row passes whole.
- Latency vocabulary registers at composition and resolves to positional tokens once, so a recorded phase costs an array write.

[STACKING]:
- `Microsoft.Extensions.Compliance.Redaction`(`api-redaction.md`): `EnableRedaction` binds the `IRedactorProvider` that package registers, and `LoggerMessageState.ClassifiedTag` carries the `DataClassificationSet` selecting each generated tag's redactor; `ApplyDiscriminator` appends the tag name to the value before redaction, so one raw value redacts to a distinct token per tag and correlation holds inside a tag name alone.
- `Microsoft.Extensions.Logging.Abstractions`(`api-logging-abstractions.md`): `[LogProperties]`, `[TagProvider]`, and `[TagName]` ride the in-box `[LoggerMessage]` partial, so payload expansion, foreign projection, and tag naming land on one generated declaration.
- `OpenTelemetry`(`api-opentelemetry.md`): governance runs ahead of every provider on the shared builder, so the records `AddOpenTelemetry` bridges onto the OTLP rail arrive sampled, buffered, enriched, and redacted.
- `Rasm.AppHost` `Observability/telemetry#SIGNAL_GOVERNANCE`: one chain seats sampler, redaction, enrichment, and buffer on `ILoggingBuilder`, then the enricher and latency rows on `IServiceCollection`; `LatencySpine` threads one `ILatencyContext` through the drain, outbound, and support folds and freezes it at the export band.

[LOCAL_ADMISSION]:
- Instrumented libraries reference the contract assembly alone — grammar attributes, enricher contracts, latency tokens; every activation verb is composition-root surface.
- Audit-grade categories exclude from sampler and buffer rules by rule construction, never a runtime check.
- An enricher is a pure projection to bounded tags; a dimension needing I/O is a design error at the row.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Telemetry`
- Owns: log volume governance, buffering, enrichment rows, redaction activation, and the latency ledger
- Accept: declaration-time rule rows and registered vocabularies over the one `ILogger` seam
- Reject: per-signal sampling probabilities beside the trace verdict; hand-rolled trace-id enrichers where the record carries typed ids natively; a `Stopwatch` timing a phase an issued token records
