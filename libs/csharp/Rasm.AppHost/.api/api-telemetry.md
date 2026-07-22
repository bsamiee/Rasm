# [RASM_APPHOST_API_TELEMETRY]

`Microsoft.Extensions.Telemetry` realizes the first-party telemetry policy the abstractions declare as contracts, binding each at a composition boundary on `ILoggingBuilder` or `IServiceCollection`, so no policy decision rides a log call site.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Telemetry`
- package: `Microsoft.Extensions.Telemetry`
- assembly: `Microsoft.Extensions.Telemetry`
- namespace: `Microsoft.Extensions.Diagnostics.Buffering`, `Microsoft.Extensions.Diagnostics.Enrichment`, `Microsoft.Extensions.Diagnostics.Latency`, `Microsoft.Extensions.Diagnostics.Sampling`, `Microsoft.Extensions.Logging`, `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: buffering and enrichment policy

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]     | [CAPABILITY]             |
| :-----: | :------------------------------ | :---------------- | :----------------------- |
|  [01]   | `GlobalLogBufferingOptions`     | buffering options | global log buffer policy |
|  [02]   | `LogBufferingFilterRule`        | filter rule       | buffered event selection |
|  [03]   | `ApplicationLogEnricherOptions` | enricher options  | application dimensions   |
|  [04]   | `ProcessLogEnricherOptions`     | enricher options  | process dimensions       |

[PUBLIC_TYPE_SCOPE]: redaction, sampling, and latency policy

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]      | [CAPABILITY]             |
| :-----: | :------------------------------------- | :----------------- | :----------------------- |
|  [01]   | `LoggerRedactionOptions`               | redaction options  | classified log redaction |
|  [02]   | `LoggerEnrichmentOptions`              | enrichment options | tag collection policy    |
|  [03]   | `RandomProbabilisticSamplerOptions`    | sampler options    | probabilistic policy     |
|  [04]   | `RandomProbabilisticSamplerFilterRule` | sampler rule       | sampled event selection  |
|  [05]   | `LatencyContextOptions`                | latency options    | latency context policy   |
|  [06]   | `LatencyConsoleOptions`                | exporter options   | latency console export   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `ILoggingBuilder` logging-policy extensions

| [INDEX] | [SURFACE]                       | [CAPABILITY]                |
| :-----: | :------------------------------ | :-------------------------- |
|  [01]   | `AddGlobalBuffer`               | global log buffering        |
|  [02]   | `EnableEnrichment`              | enriched logging            |
|  [03]   | `EnableRedaction`               | classified redaction        |
|  [04]   | `AddTraceBasedSampler`          | trace-based sampling        |
|  [05]   | `AddRandomProbabilisticSampler` | probability sampling        |
|  [06]   | `AddSampler<T>`                 | custom sampler registration |

[ENTRYPOINT_SCOPE]: `IServiceCollection` enricher and latency extensions

| [INDEX] | [SURFACE]                       | [CAPABILITY]           |
| :-----: | :------------------------------ | :--------------------- |
|  [01]   | `AddApplicationLogEnricher`     | application dimensions |
|  [02]   | `AddProcessLogEnricher`         | process dimensions     |
|  [03]   | `AddLatencyContext`             | latency context        |
|  [04]   | `AddConsoleLatencyDataExporter` | latency console export |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Buffering: `GlobalLogBufferingOptions` carries `AutoFlushDuration`, `MaxLogRecordSizeInBytes`, `MaxBufferSizeInBytes`, and `Rules`; `LogBufferingFilterRule` selects buffered events by category, log level, event id, event name, and attributes.
- Enrichment: application and process tag producers feed each record; `LoggerEnrichmentOptions` captures stack traces through `CaptureStackTraces`, `UseFileInfoForStackTraces`, `IncludeExceptionMessage`, and `MaxStackTraceLength`.
- Redaction: `LoggerRedactionOptions.ApplyDiscriminator` gates the classifier-backed discriminator.
- Sampling: trace-based, probabilistic, and custom registrations select the sampler; `RandomProbabilisticSamplerOptions.Rules` scope probability per matched log.
- Latency: `AddLatencyContext` registers the context under `LatencyContextOptions.ThrowOnUnregisteredNames`, and `AddConsoleLatencyDataExporter` binds the `LatencyConsoleOptions` sink.

[STACKING]:
- `Microsoft.Extensions.Telemetry.Abstractions`(`api-telemetry-abstractions.md`): realizes its policy contracts — `AddApplicationLogEnricher`/`AddProcessLogEnricher` register an `IStaticLogEnricher`, `EnableEnrichment` runs the `ILogEnricher` set, `AddGlobalBuffer` binds the concrete `GlobalLogBuffer`, and every sampler registration supplies the `LoggingSampler` whose `ShouldSample(in LogEntry<TState>)` gates each entry.
- `Microsoft.Extensions.Logging.Abstractions`(`api-logging.md`): each registration extends `ILoggingBuilder`, folding its policy ahead of `ILogger.Log<TState>` on the shared delegate cache, and buffered records replay through `IBufferedLogger`.
- AppHost Observability composition root: every telemetry policy binds in the single host-builder pass that freezes the module graph.

[LOCAL_ADMISSION]:
- Enrichment adds bounded dimensions and never carries domain payload into a log.
- Redaction is a classifier-backed logging policy, not call-site string cleanup.
- Sampling decides observability volume before any sink sees it.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Telemetry`
- Owns: first-party telemetry policy realization over the abstractions contracts
- Accept: buffering, enrichment, redaction, sampling, and latency policy bound at composition
- Reject: log-call-local telemetry policy
