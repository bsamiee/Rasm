# [RASM_PERSISTENCE_API_NPGSQL_OTEL]

`Npgsql.OpenTelemetry` wires Npgsql tracing and metrics into OpenTelemetry
provider builders.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.OpenTelemetry`
- package: `Npgsql.OpenTelemetry`
- assembly: `Npgsql.OpenTelemetry`
- namespace: `Npgsql`
- driver package: `Npgsql`
- asset: runtime library
- rail: telemetry

## [02]-[PUBLIC_TYPES]

[TELEMETRY_TYPES]: provider builder extensions
- rail: telemetry

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]   | [CAPABILITY]             |
| :-----: | :-------------------------------- | :--------------- | :----------------------- |
|  [01]   | `TracerProviderBuilderExtensions` | tracer extension | subscribes Npgsql spans  |
|  [02]   | `MeterProviderBuilderExtensions`  | meter extension  | subscribes Npgsql meters |

The extension signatures are:
- `AddNpgsql(TracerProviderBuilder) : TracerProviderBuilder`
- `AddNpgsqlInstrumentation(MeterProviderBuilder, Action<NpgsqlMetricsOptions>?) : MeterProviderBuilder`

`NpgsqlMetricsOptions` is declared by the `Npgsql` driver package with a parameterless ctor.
Histogram and cardinality posture ride OpenTelemetry meter-view configuration, not options properties.

Tracing depth is configured via `NpgsqlDataSourceBuilder.ConfigureTracing(Action<NpgsqlTracingOptionsBuilder>)`, which is a driver-level concern catalogued in `api-npgsql.md`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: tracing wiring
- rail: telemetry

[TRACING_WIRING]:
- Surface: `AddNpgsql`
- Call shape: `AddNpgsql(TracerProviderBuilder) : TracerProviderBuilder`
- Capability: subscribes Npgsql spans

[ENTRYPOINT_SCOPE]: metrics wiring
- rail: telemetry

[METRICS_WIRING]:
- Surface: `AddNpgsqlInstrumentation`
- Call shape: `MeterProviderBuilder` plus optional `Action<NpgsqlMetricsOptions>`
- Capability: subscribes Npgsql meters

## [04]-[IMPLEMENTATION_LAW]

[TELEMETRY_PROFILE]:
- namespace: `Npgsql`
- tracer root: `AddNpgsql` on `TracerProviderBuilder`
- meter root: `AddNpgsqlInstrumentation` on `MeterProviderBuilder` with optional `Action<NpgsqlMetricsOptions>`
- options root: `NpgsqlMetricsOptions` declared by the `Npgsql` driver; parameterless ctor; no settable property knobs at the admitted version
- tracing depth root: `NpgsqlTracingOptionsBuilder` via `NpgsqlDataSourceBuilder.ConfigureTracing` — driver-layer concern, not OTel package concern

[LOCAL_ADMISSION]:
- Npgsql telemetry enters through the composition-root telemetry pipeline only.
- Span and meter names are driver facts, not Persistence vocabulary.
- Metrics options are declared by the driver and configured at the telemetry boundary.
- Telemetry wiring cannot leak into store profiles or query surfaces.
- Tracing configuration (filters, span-name providers, enrichment, first-response event, physical-open tracing) belongs to `NpgsqlDataSourceBuilder.ConfigureTracing`, not to this package.

[RAIL_LAW]:
- Package: `Npgsql.OpenTelemetry`
- Owns: Npgsql OpenTelemetry wiring
- Accept: composition-root tracer and meter admission
- Reject: telemetry calls inside store profiles
