# [RASM_APPHOST_API_TELEMETRY]

`Microsoft.Extensions.Telemetry` governs log volume, buffering, enrichment, and redaction activation over the one `ILogger` boundary, and mints the pooled latency ledger that times in-flight phases without a child span. Every verb it ships extends `ILoggingBuilder` or `IServiceCollection` and binds one policy row, so no governance decision rides a log call site. Its contract half — the emission grammar, the enricher, buffer, and sampler contracts, and the latency tokens an instrumented library binds — ships in `Microsoft.Extensions.Telemetry.Abstractions` and homes at the branch tier.

## [01]-[PUBLIC_TYPES]

[POLICY_OPTIONS]: the option rows every activation verb binds

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :------------------------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `GlobalLogBufferingOptions`            | options       | flush window, record and buffer size caps, and the `Rules` row list |
|  [02]   | `LogBufferingFilterRule`               | rule row      | selects buffered records, adding an attribute predicate             |
|  [03]   | `RandomProbabilisticSamplerOptions`    | options       | the probability `Rules` row list                                    |
|  [04]   | `RandomProbabilisticSamplerFilterRule` | rule row      | weights a matched selector with a `Probability`                     |
|  [05]   | `LoggerEnrichmentOptions`              | options       | exception-frame admission onto the log signal                       |
|  [06]   | `LoggerRedactionOptions`               | options       | `ApplyDiscriminator` posture on the redacted tag value              |
|  [07]   | `ApplicationLogEnricherOptions`        | options       | service-identity tag switches                                       |
|  [08]   | `ProcessLogEnricherOptions`            | options       | process and thread tag switches                                     |
|  [09]   | `LatencyContextOptions`                | options       | `ThrowOnUnregisteredNames` boot-fail switch                         |
|  [10]   | `LatencyConsoleOptions`                | options       | console projection switches, all three defaulting on                |

- `GlobalLogBufferingOptions`: `AutoFlushDuration` `MaxLogRecordSizeInBytes` (51200) `MaxBufferSizeInBytes` (524288000) `Rules`
- `LogBufferingFilterRule` and `RandomProbabilisticSamplerFilterRule`: `CategoryName` `LogLevel` `EventId` `EventName`, each get-only off the constructor
- `LogBufferingFilterRule.Attributes`: `IReadOnlyList<KeyValuePair<string, object?>>?` — the log-state predicate the sampler rule has no counterpart for
- `RandomProbabilisticSamplerFilterRule.Probability`: `double`
- `LoggerEnrichmentOptions`: `CaptureStackTraces` `UseFileInfoForStackTraces` `IncludeExceptionMessage` `MaxStackTraceLength` (4096)
- `ApplicationLogEnricherOptions`: `EnvironmentName` and `ApplicationName` default on, `DeploymentRing` and `BuildVersion` default off
- `ProcessLogEnricherOptions`: `ProcessId` defaults on, `ThreadId` defaults off

[TAG_VOCABULARIES]: the emitted dimension names each shipped enricher writes

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :------------------------ | :------------ | :------------------------------------------------------------------ |
|  [01]   | `ApplicationEnricherTags` | constant set  | `ApplicationName` `EnvironmentName` `DeploymentRing` `BuildVersion` |
|  [02]   | `ProcessEnricherTagNames` | constant set  | `ProcessId` `ThreadId`                                              |

- `ApplicationEnricherTags` values are semconv-spelled — `service.name`, `deployment.environment`, `service.version` — beside the non-semconv `DeploymentRing`; `ProcessEnricherTagNames` writes `process.pid` and `thread.id`.
- Each set exposes `DimensionNames` as an `IReadOnlyList<string>`, so a governance table censuses the emitted vocabulary off the package rather than a hand-copied literal.

## [02]-[ENTRYPOINTS]

Every options-bearing verb carries a parameterless overload, an `Action<TOptions>` overload, and a configuration overload — `IConfiguration` on the two `ILoggingBuilder` verbs that take one, `IConfigurationSection` elsewhere.

[ENTRYPOINT_SCOPE]: `ILoggingBuilder` activation

| [INDEX] | [SURFACE]                                                                  | [SHAPE] | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------------- | :------ | :------------------------------------- |
|  [01]   | `AddTraceBasedSampler()`                                                   | static  | slaves log volume to the trace verdict |
|  [02]   | `AddRandomProbabilisticSampler(double, LogLevel?)`                         | static  | one probability rule capped by level   |
|  [03]   | `AddRandomProbabilisticSampler(Action<RandomProbabilisticSamplerOptions>)` | static  | the full rule-row list                 |
|  [04]   | `AddRandomProbabilisticSampler(IConfiguration)`                            | static  | the rule-row list bound from config    |
|  [05]   | `AddSampler<T>() where T : LoggingSampler`                                 | static  | DI-activated custom sampler            |
|  [06]   | `AddSampler(LoggingSampler)`                                               | static  | pre-constructed sampler instance       |
|  [07]   | `AddGlobalBuffer(LogLevel?)`                                               | static  | buffers at and below one level         |
|  [08]   | `AddGlobalBuffer(Action<GlobalLogBufferingOptions>)`                       | static  | rule rows, size caps, flush window     |
|  [09]   | `AddGlobalBuffer(IConfiguration)`                                          | static  | the buffer policy bound from config    |
|  [10]   | `EnableEnrichment(Action<LoggerEnrichmentOptions>)`                        | static  | activates both enricher cost classes   |
|  [11]   | `EnableRedaction(Action<LoggerRedactionOptions>)`                          | static  | activates the redactor hook            |

- `AddGlobalBuffer` passes three record classes through live — one over `MaxLogRecordSizeInBytes`, one matching no rule, and one inside the `AutoFlushDuration` window after a flush.

[ENTRYPOINT_SCOPE]: `IServiceCollection` registration

| [INDEX] | [SURFACE]                                                          | [SHAPE] | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------------------- | :------ | :---------------------------------------- |
|  [01]   | `AddApplicationLogEnricher(Action<ApplicationLogEnricherOptions>)` | static  | shipped service-identity tag rows         |
|  [02]   | `AddProcessLogEnricher(Action<ProcessLogEnricherOptions>)`         | static  | shipped process and thread tag rows       |
|  [03]   | `AddLatencyContext(Action<LatencyContextOptions>)`                 | static  | registers provider, issuer, and pool      |
|  [04]   | `AddConsoleLatencyDataExporter(Action<LatencyConsoleOptions>)`     | static  | console exporter for operator-local hosts |

- Enricher registration by type or instance, latency-name registration, and the no-op ledger are contract-assembly verbs and home at the branch catalogue.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every activation verb seats one `ExtendedLoggerFactory`, so governance applies once and every registered provider observes the same record.
- `AddTraceBasedSampler` declares once what the trace root decided, so logs and spans rise and fall as one population; a probability rule thins the chatty floor by maximum level and never the error ceiling.
- Sampler and buffer rows select on category, level, and event identity, buffer rows adding an attribute predicate; a category matching no row passes whole.
- Latency vocabulary registers at composition and resolves to positional tokens once, so a recorded phase costs an array write and the pool returns each context on dispose.
- Shipped enrichers write semconv-spelled identity and process dimensions, so their tag vocabulary collides with the resource triple a detector already stamps and the governance table decides which surface owns each key.

[STACKING]:
- `Microsoft.Extensions.Telemetry.Abstractions`(`libs/dotnet/.api/api-telemetry-abstractions.md`): every verb here realizes a contract declared there — a registration supplies the `LoggingSampler`, `ILogEnricher`, or `LogBuffer` the record crosses, and `LatencyContextOptions` gates the `ILatencyContextTokenIssuer` lookup.
- `Microsoft.Extensions.Compliance.Redaction`(`libs/dotnet/.api/api-redaction.md`): `EnableRedaction` binds the `IRedactorProvider` that package registers, and `LoggerMessageState.ClassifiedTag` carries the `DataClassificationSet` selecting each generated tag's redactor; `ApplyDiscriminator` appends the tag name to the value before redaction, so one raw value redacts to a distinct token per tag and correlation holds inside a tag name alone.
- `Microsoft.Extensions.Logging.Abstractions`(`libs/dotnet/.api/api-logging-abstractions.md`): every verb extends `ILoggingBuilder` and folds ahead of `ILogger.Log<TState>` on the shared delegate cache; a buffered record replays through `IBufferedLogger`.
- `OpenTelemetry`(`libs/dotnet/.api/api-opentelemetry.md`): governance runs ahead of every provider on the shared builder, so the records `AddOpenTelemetry` bridges onto the OTLP exporters arrive sampled, buffered, enriched, and redacted.
- `Rasm.AppHost` `Observability/telemetry#SIGNAL_GOVERNANCE`: one chain seats sampler, redaction, enrichment, and buffer on `ILoggingBuilder`, then the enricher and latency rows on `IServiceCollection`; `LatencySpine` threads one `ILatencyContext` through the drain, outbound, and support folds and freezes it at the export band.

[LOCAL_ADMISSION]:
- Every verb here is composition-root surface; an instrumented library references the contract assembly alone.
- Audit-grade categories exclude from sampler and buffer rules by rule construction, never a runtime check.
- Enrichers project purely to bounded tags, and a dimension needing I/O is a design error at the row.
- Buffer capacity is a loss posture, not a durability claim — an unflushed record dies with the process.
