# [RASM_APPHOST_API_TELEMETRY]

`Microsoft.Extensions.Telemetry` admits log buffering, enrichment, redaction, sampling, and latency context into the observability rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Telemetry`

- package: `Microsoft.Extensions.Telemetry`
- assembly: `Microsoft.Extensions.Telemetry`
- namespace: `Microsoft.Extensions.Diagnostics.Buffering`
- namespace: `Microsoft.Extensions.Diagnostics.Enrichment`
- namespace: `Microsoft.Extensions.Diagnostics.Latency`
- namespace: `Microsoft.Extensions.Diagnostics.Sampling`
- namespace: `Microsoft.Extensions.Logging`
- namespace: `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: buffering and enrichment family

- rail: observability

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]     | [RAIL]                   |
| :-----: | :------------------------------ | :---------------- | :----------------------- |
|  [01]   | `GlobalLogBufferingOptions`     | buffering options | global log buffer policy |
|  [02]   | `LogBufferingFilterRule`        | filter rule       | buffered event selection |
|  [03]   | `ApplicationLogEnricherOptions` | enricher options  | application dimensions   |
|  [04]   | `ProcessLogEnricherOptions`     | enricher options  | process dimensions       |

[PUBLIC_TYPE_SCOPE]: redaction, sampling, and latency family

- rail: observability

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]      | [RAIL]                   |
| :-----: | :------------------------------------- | :----------------- | :----------------------- |
|  [01]   | `LoggerRedactionOptions`               | redaction options  | classified log redaction |
|  [02]   | `LoggerEnrichmentOptions`              | enrichment options | tag collection policy    |
|  [03]   | `RandomProbabilisticSamplerOptions`    | sampler options    | probabilistic policy     |
|  [04]   | `RandomProbabilisticSamplerFilterRule` | sampler rule       | sampled event selection  |
|  [05]   | `LatencyContextOptions`                | latency options    | latency context policy   |
|  [06]   | `LatencyConsoleOptions`                | exporter options   | latency console export   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: logging operations

- rail: observability
- call shape: `ILoggingBuilder` extension

| [INDEX] | [SURFACE]                       | [RAIL]                      |
| :-----: | :------------------------------ | :-------------------------- |
|  [01]   | `AddGlobalBuffer`               | global log buffering        |
|  [02]   | `EnableEnrichment`              | enriched logging            |
|  [03]   | `EnableRedaction`               | classified redaction        |
|  [04]   | `AddTraceBasedSampler`          | trace-based sampling        |
|  [05]   | `AddRandomProbabilisticSampler` | probability sampling        |
|  [06]   | `AddSampler<T>`                 | custom sampler registration |

[ENTRYPOINT_SCOPE]: service operations

- rail: observability
- call shape: `IServiceCollection` extension

| [INDEX] | [SURFACE]                       | [RAIL]                                       |
| :-----: | :------------------------------ | :------------------------------------------- |
|  [01]   | `AddApplicationLogEnricher`     | application dimensions                       |
|  [02]   | `AddServiceLogEnricher`         | obsolete predecessor of application enricher |
|  [03]   | `AddProcessLogEnricher`         | process dimensions                           |
|  [04]   | `AddLatencyContext`             | latency context                              |
|  [05]   | `AddConsoleLatencyDataExporter` | latency console export                       |

## [04]-[IMPLEMENTATION_LAW]

[TELEMETRY_TOPOLOGY]:

- namespaces: buffering, enrichment, latency, sampling, logging
- buffering surface: global log buffer options and filter rules
- enrichment surface: application, service, and process tag producers
- redaction surface: logger redaction options and redaction logging extension
- sampling surface: trace-based, probabilistic, and custom sampler registration
- latency surface: latency context registration and console exporter options

[LOCAL_ADMISSION]:

- Telemetry policy is registered at composition boundaries.
- Enrichment adds bounded dimensions and never smuggles domain payloads into logs.
- Redaction is a classifier-backed logging policy, not string cleanup at call sites.
- Sampling decides observability volume before sinks see events.

[RAIL_LAW]:

- Package: `Microsoft.Extensions.Telemetry`
- Owns: first-party telemetry policy helpers
- Accept: buffering, enrichment, redaction, sampling, and latency policy
- Reject: log-call-local telemetry policy
