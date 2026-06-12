# [RASM_APPHOST_API_POLLY_EXTENSIONS]

`Polly.Extensions` supplies resilience telemetry options, telemetry enrichment,
metering integration, and builder extensions that add telemetry to resilience
pipelines.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Polly.Extensions`
- package: `Polly.Extensions`
- assembly: `Polly.Extensions`
- namespace: `Polly`
- namespace: `Polly.Telemetry`
- asset: runtime library
- rail: resilience telemetry

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: telemetry configuration family
- rail: resilience telemetry

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]      | [RAIL]                   |
| :-----: | :--------------------------------------------- | :----------------- | :----------------------- |
|   [1]   | `TelemetryResiliencePipelineBuilderExtensions` | builder extension  | pipeline telemetry setup |
|   [2]   | `TelemetryOptions`                             | option value       | telemetry configuration  |
|   [3]   | `SeverityProviderArguments`                    | callback argument  | event severity selection |
|   [4]   | `EnrichmentContext<TKey,TValue>`               | enrichment context | telemetry enrichment     |
|   [5]   | `MeteringEnricher`                             | enricher           | metric enrichment        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: telemetry operations
- rail: resilience telemetry

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY]    | [RAIL]                        |
| :-----: | :----------------------------------- | :---------------- | :---------------------------- |
|   [1]   | `ConfigureTelemetry(loggerFactory)`  | builder extension | logger-backed telemetry       |
|   [2]   | `ConfigureTelemetry(options)`        | builder extension | option-backed telemetry       |
|   [3]   | `TelemetryOptions.LoggerFactory`     | option value      | logging provider              |
|   [4]   | `TelemetryOptions.MeteringEnrichers` | option value      | metric dimension enrichment   |
|   [5]   | `TelemetryOptions.ResultFormatter`   | option value      | result value formatting       |
|   [6]   | `TelemetryOptions.SeverityProvider`  | option value      | event severity classification |

## [4]-[IMPLEMENTATION_LAW]

[TELEMETRY_TOPOLOGY]:
- namespaces: `Polly`, `Polly.Telemetry`
- builder surface: telemetry configuration on resilience pipeline builders
- option surface: logger factory, severity provider, result formatter, and metering enrichers
- enrichment surface: telemetry context and metering enricher values
- pipeline effect: telemetry strategy is inserted at the beginning of the composite pipeline

[LOCAL_ADMISSION]:
- Resilience telemetry is configured on the pipeline builder, not inside handled operations.
- Metric enrichment is bounded by telemetry options and never reads arbitrary domain state.
- Severity policy is a value-level callback over resilience event arguments.
- Result formatting is an observability projection and cannot mutate the outcome.

[RAIL_LAW]:
- Package: `Polly.Extensions`
- Owns: resilience pipeline telemetry configuration
- Accept: builder-level telemetry policy
- Reject: per-call logging wrappers around resilience pipelines
