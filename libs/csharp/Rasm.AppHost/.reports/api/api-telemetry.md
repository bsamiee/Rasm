# [RASM_APPHOST_API_TELEMETRY]

`Microsoft.Extensions.Telemetry` admits log buffering, enrichment, redaction,
sampling, latency context, pooled diagnostics helpers, and HTTP route telemetry
helpers into the observability rail.

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
- namespace: `Microsoft.Extensions.Http.Diagnostics`
- asset: runtime library
- rail: observability

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: buffering and enrichment family
- rail: observability

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]     | [RAIL]                    |
| :-----: | :------------------------------- | :---------------- | :------------------------ |
|   [1]   | `GlobalLogBufferingOptions`      | buffering options | global log buffer policy  |
|   [2]   | `LogBufferingFilterRule`         | filter rule       | buffered event selection  |
|   [3]   | `LogBufferingFilterRuleSelector` | rule selector     | buffering rule selection  |
|   [4]   | `SerializedLogRecord`            | log record value  | buffered record payload   |
|   [5]   | `DeserializedLogRecord`          | log record value  | restored record payload   |
|   [6]   | `ApplicationLogEnricherOptions`  | enricher options  | application dimensions    |
|   [7]   | `ProcessLogEnricherOptions`      | enricher options  | process dimensions        |
|   [8]   | `ProcessLogEnricher`             | enricher          | process tag producer      |
|   [9]   | `StaticProcessLogEnricher`       | enricher          | static process tag source |

[PUBLIC_TYPE_SCOPE]: redaction, sampling, latency, and HTTP route family
- rail: observability

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]      | [RAIL]                    |
| :-----: | :------------------------------------- | :----------------- | :------------------------ |
|   [1]   | `LoggerRedactionOptions`               | redaction options  | classified log redaction  |
|   [2]   | `LoggerEnrichmentOptions`              | enrichment options | tag collection policy     |
|   [3]   | `RandomProbabilisticSampler`           | sampler            | log sampling              |
|   [4]   | `RandomProbabilisticSamplerOptions`    | sampler options    | probabilistic policy      |
|   [5]   | `RandomProbabilisticSamplerFilterRule` | sampler rule       | sampled event selection   |
|   [6]   | `ILogSamplingFilterRule`               | sampler contract   | sampling rule abstraction |
|   [7]   | `LatencyContextOptions`                | latency options    | latency context policy    |
|   [8]   | `LatencyConsoleOptions`                | exporter options   | latency console export    |
|   [9]   | `HttpRouteParameter`                   | route value        | route parameter policy    |
|  [10]   | `IHttpRouteParser`                     | route parser       | route segment parsing     |
|  [11]   | `IHttpRouteFormatter`                  | route formatter    | classified route format   |

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

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]    | [RAIL]                   |
| :-----: | :------------------------------ | :---------------- | :----------------------- |
|   [1]   | `AddServiceLogEnricher`         | service extension | service dimensions       |
|   [2]   | `AddApplicationLogEnricher`     | service extension | application dimensions   |
|   [3]   | `AddProcessLogEnricher`         | service extension | process dimensions       |
|   [4]   | `AddLatencyContext`             | service extension | latency context          |
|   [5]   | `AddConsoleLatencyDataExporter` | service extension | latency console export   |
|   [6]   | `AddHttpRouteProcessor`         | service extension | HTTP route normalization |

## [4]-[IMPLEMENTATION_LAW]

[TELEMETRY_TOPOLOGY]:
- namespaces: buffering, enrichment, latency, sampling, logging, HTTP diagnostics
- buffering surface: global log buffer options, filter rules, serialized and deserialized records
- enrichment surface: application, service, and process tag producers
- redaction surface: logger redaction options and redaction logging extension
- sampling surface: trace-based, probabilistic, and custom sampler registration
- latency surface: latency context registration and console exporter options
- HTTP route surface: parser, formatter, parameter redaction mode, route processor

[LOCAL_ADMISSION]:
- Telemetry policy is registered at composition boundaries.
- Enrichment adds bounded dimensions and never smuggles domain payloads into logs.
- Redaction is a classifier-backed logging policy, not string cleanup at call sites.
- Sampling decides observability volume before sinks see events.
- Route processing normalizes telemetry labels without mutating the outbound request contract.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Telemetry`
- Owns: first-party telemetry policy helpers
- Accept: buffering, enrichment, redaction, sampling, latency, and route normalization policy
- Reject: log-call-local telemetry policy
