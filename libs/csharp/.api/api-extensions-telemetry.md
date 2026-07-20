# [RASM_API_EXTENSIONS_TELEMETRY]

`Microsoft.Extensions.Telemetry` is the R9 telemetry rail over the in-box `ILogger`: trace-slaved and probabilistic log sampling, the global log buffer, enrichment rows, the redaction activation seam, and the pooled latency-context ledger. Contract assembly `Microsoft.Extensions.Telemetry.Abstractions` carries the source-generated emission grammar — `[LogProperties]`, `[TagProvider]`, `[TagName]` — the enricher contracts, the latency tokens, and the buffer contracts, so instrumented libraries reference the abstractions alone while the composition root activates the rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Telemetry`
- package: `Microsoft.Extensions.Telemetry`
- assembly: `Microsoft.Extensions.Telemetry`
- contract assembly: `Microsoft.Extensions.Telemetry.Abstractions` — emission grammar attributes, `ILogEnricher`/`IStaticLogEnricher`, `ILatencyContext` family, `GlobalLogBuffer`
- namespace: `Microsoft.Extensions.Logging`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Diagnostics.Enrichment`, `Microsoft.Extensions.Diagnostics.Latency`, `Microsoft.Extensions.Diagnostics.Buffering`, `Microsoft.Extensions.Diagnostics.Sampling`
- asset: runtime library
- rail: log governance

## [02]-[PUBLIC_TYPES]

[GRAMMAR_TYPES]: emission grammar (contract assembly)
- rail: log governance

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]      | [CAPABILITY]                                            |
| :-----: | :--------------------------- | :------------------ | :------------------------------------------------------ |
|  [01]   | `LogPropertiesAttribute`     | payload expansion   | `SkipNullProperties`, `OmitReferenceName`, `Transitive` |
|  [02]   | `LogPropertyIgnoreAttribute` | expansion exclusion | drops one member from the expansion                     |
|  [03]   | `TagProviderAttribute`       | foreign projection  | `(Type, string)` method row for unannotatable types     |
|  [04]   | `TagNameAttribute`           | declared rename     | tag-vocabulary edits break at rebuild, never wire drift |

These attributes extend the in-box `[LoggerMessage]` generator; the generated-emission monopoly and the six-hole arity ceiling are `docs/stacks/csharp/domain/diagnostics.md` law composed here as the grammar's home.

[GOVERNANCE_TYPES]: sampling, buffering, enrichment, redaction
- rail: log governance

| [INDEX] | [SYMBOL]                              | [PACKAGE_ROLE]     | [CAPABILITY]                                                                                        |
| :-----: | :------------------------------------ | :----------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `RandomProbabilisticSamplerOptions`   | sampler rows       | `Rules` — per-category/level probability rows          |
|  [02]   | `GlobalLogBufferingOptions`           | buffer square      | flush cadence, record and buffer byte caps, `Rules`    |
|  [03]   | `GlobalLogBuffer` / `LogBuffer`       | buffer contract    | `Flush()` replays held records on incident             |
|  [04]   | `PerRequestLogBuffer`                 | scoped buffer      | request-scoped sibling of the global ring              |
|  [05]   | `ILogEnricher` / `IStaticLogEnricher` | enricher contracts | per-record and per-provider tag projection             |
|  [06]   | `IEnrichmentTagCollector`             | collector seam     | receives enricher tag writes                           |
|  [07]   | `ApplicationLogEnricherOptions`       | enricher rows      | service metadata rows                                  |
|  [08]   | `LoggerEnrichmentOptions`             | exception detail   | stack-capture and message-admission knobs              |
|  [09]   | `LoggerRedactionOptions`              | redaction seam     | `ApplyDiscriminator` folds the tag name into the token |

`GlobalLogBufferingOptions` carries `AutoFlushDuration`, `MaxLogRecordSizeInBytes`, `MaxBufferSizeInBytes`, and `Rules`; `ApplicationLogEnricherOptions` carries `EnvironmentName`, `ApplicationName`, `DeploymentRing`, and `BuildVersion`; `LoggerEnrichmentOptions` carries `CaptureStackTraces`, `UseFileInfoForStackTraces`, `IncludeExceptionMessage`, and `MaxStackTraceLength`.

[LATENCY_TYPES]: the in-flight latency ledger (contract assembly)
- rail: log governance

| [INDEX] | [SYMBOL]                                        | [PACKAGE_ROLE]  | [CAPABILITY]                                                                      |
| :-----: | :---------------------------------------------- | :-------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `ILatencyContext`                               | ledger contract | checkpoint, measure, and tag verbs over one ledger   |
|  [02]   | `ILatencyContextProvider`                       | pool contract   | `CreateContext()` per operation                      |
|  [03]   | `ILatencyContextTokenIssuer`                    | token mint      | `GetCheckpointToken`/`GetMeasureToken`/`GetTagToken` |
|  [04]   | `CheckpointToken` / `MeasureToken` / `TagToken` | resolved tokens | `Name` + `Position` — registered-vocabulary handles  |
|  [05]   | `ILatencyDataExporter`                          | export contract | receives frozen latency data                         |
|  [06]   | `LatencyContextOptions`                         | ledger policy   | `ThrowOnUnregisteredNames`                           |
|  [07]   | `LatencyConsoleOptions`                         | exporter policy | `OutputCheckpoints`, `OutputMeasures`, `OutputTags`  |

`ILatencyContext` records through `AddCheckpoint`, `AddMeasure`, `RecordMeasure`, and `SetTag`; `Freeze` seals `LatencyData` for export.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: activation on `ILoggingBuilder` and `IServiceCollection`
- rail: log governance

| [INDEX] | [SURFACE]                                                               | [KIND]            | [CAPABILITY]                                                    |
| :-----: | :---------------------------------------------------------------------- | :---------------- | :-------------------------------------------------------------- |
|  [01]   | `AddTraceBasedSampler`                                                  | volume            | slaves log volume to the trace verdict          |
|  [02]   | `AddRandomProbabilisticSampler`                                         | volume            | probability and max-level rule rows             |
|  [03]   | `AddGlobalBuffer`                                                       | buffering         | level, options, and configuration overloads     |
|  [04]   | `EnableEnrichment`                                                      | enrichment        | activates both enricher cost classes            |
|  [05]   | `EnableRedaction`                                                       | redaction         | activates the seam; options-delegate overload   |
|  [06]   | `AddLogEnricher<T>` / `AddStaticLogEnricher<T>`                         | enricher rows     | generic and instance registration               |
|  [07]   | `AddServiceLogEnricher` / `AddApplicationLogEnricher`                   | shipped enrichers | service metadata enricher rows                  |
|  [08]   | `AddProcessLogEnricher`                                                 | shipped enricher  | process id/thread rows                          |
|  [09]   | `AddLatencyContext`                                                     | latency ledger    | registers provider, pool, and token issuer      |
|  [10]   | `RegisterCheckpointNames`                                               | vocabulary        | `params string[]` rows; measure and tag mirrors |
|  [11]   | `AddConsoleLatencyDataExporter`                                         | latency export    | console exporter; options/section overloads     |

`RegisterMeasureNames` and `RegisterTagNames` mirror the checkpoint registration for the other two token kinds.

## [04]-[IMPLEMENTATION_LAW]

[GOVERNANCE_TOPOLOGY]:
- volume root: `AddTraceBasedSampler` declares once what the trace root decided — logs and spans rise and fall as one population; probabilistic rule rows thin the chatty floor by maximum level, never the error ceiling
- buffer root: `AddGlobalBuffer` holds verbose tiers in the ring and `GlobalLogBuffer.Flush()` replays them at fault transition; an oversize record bypasses and emits live
- enrichment root: `EnableEnrichment` activates static rows once per provider and per-record rows per record — cost class selects the contract, never the registration order
- latency root: vocabulary registers at composition, tokens resolve once, `ILatencyContext` records through tokens, `Freeze` seals before export — cheaper than child spans and free of sampling coupling

[STACKING]:
- `Microsoft.Extensions.Compliance.Redaction`(`api-redaction.md`): `EnableRedaction` activates the seam whose redactor map that package registers; `ApplyDiscriminator` is the cross-tag-correlation opt-in.
- `OpenTelemetry`(`api-opentelemetry.md`): sampled-in records flow to the OTel logger provider beside the Serilog projection — two providers on one `ILogger`, each with a disjoint delivery mandate.
- `Serilog` console/file rails: the operator-local projection leg; this rail governs volume and enrichment ahead of both providers.

[LOCAL_ADMISSION]:
- Libraries reference the abstractions assembly alone — grammar attributes, enricher contracts, latency tokens; every activation verb is composition-root surface.
- Audit-grade categories exclude from sampler and buffer rules by rule construction, never a runtime check.
- An enricher is a pure projection to bounded tags; a dimension needing I/O is a design error at the row.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Telemetry` (+ `Microsoft.Extensions.Telemetry.Abstractions` contracts)
- Owns: log volume governance, buffering, enrichment rows, redaction activation, and the latency-context ledger
- Accept: declaration-time rule rows over the one `ILogger` seam
- Reject: per-signal sampling probabilities beside the trace verdict; hand-rolled trace-id enrichers where the record carries typed ids natively
