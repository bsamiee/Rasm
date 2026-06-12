# [RASM_APPHOST_API_TELEMETRY]

`Microsoft.Extensions.Telemetry` admits log buffering, enrichment, redaction,
sampling, and latency context into the observability rail.

## [1]-[PACKAGE_SURFACE]

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

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: buffering and enrichment family
- rail: observability

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]     | [RAIL]                   |
| :-----: | :------------------------------ | :---------------- | :----------------------- |
|   [1]   | `GlobalLogBufferingOptions`     | buffering options | global log buffer policy |
|   [2]   | `LogBufferingFilterRule`        | filter rule       | buffered event selection |
|   [3]   | `ApplicationLogEnricherOptions` | enricher options  | application dimensions   |
|   [4]   | `ProcessLogEnricherOptions`     | enricher options  | process dimensions       |

[PUBLIC_TYPE_SCOPE]: redaction, sampling, and latency family
- rail: observability

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]      | [RAIL]                   |
| :-----: | :------------------------------------- | :----------------- | :----------------------- |
|   [1]   | `LoggerRedactionOptions`               | redaction options  | classified log redaction |
|   [2]   | `LoggerEnrichmentOptions`              | enrichment options | tag collection policy    |
|   [3]   | `RandomProbabilisticSamplerOptions`    | sampler options    | probabilistic policy     |
|   [4]   | `RandomProbabilisticSamplerFilterRule` | sampler rule       | sampled event selection  |
|   [5]   | `LatencyContextOptions`                | latency options    | latency context policy   |
|   [6]   | `LatencyConsoleOptions`                | exporter options   | latency console export   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: logging operations
- rail: observability

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]    | [RAIL]                      |
| :-----: | :------------------------------ | :---------------- | :-------------------------- |
|   [1]   | `AddGlobalBuffer`               | logging extension | global log buffering        |
|   [2]   | `EnableEnrichment`              | logging extension | enriched logging            |
|   [3]   | `EnableRedaction`               | logging extension | classified redaction        |
|   [4]   | `AddTraceBasedSampler`          | logging extension | trace-based sampling        |
|   [5]   | `AddRandomProbabilisticSampler` | logging extension | probability sampling        |
|   [6]   | `AddSampler<T>`                 | logging extension | custom sampler registration |

[ENTRYPOINT_SCOPE]: service operations
- rail: observability

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]    | [RAIL]                                       |
| :-----: | :------------------------------ | :---------------- | :------------------------------------------- |
|   [1]   | `AddApplicationLogEnricher`     | service extension | application dimensions                       |
|   [2]   | `AddServiceLogEnricher`         | service extension | obsolete predecessor of application enricher |
|   [3]   | `AddProcessLogEnricher`         | service extension | process dimensions                           |
|   [4]   | `AddLatencyContext`             | service extension | latency context                              |
|   [5]   | `AddConsoleLatencyDataExporter` | service extension | latency console export                       |

## [4]-[IMPLEMENTATION_LAW]

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
