# [RASM_PERSISTENCE_API_NPGSQL_OTEL]

`Npgsql.OpenTelemetry` wires Npgsql tracing and metrics into OpenTelemetry
provider builders.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.OpenTelemetry`
- package: `Npgsql.OpenTelemetry`
- assembly: `Npgsql.OpenTelemetry`
- namespace: `Npgsql`
- driver package: `Npgsql`
- asset: runtime library
- rail: telemetry

## [2]-[PUBLIC_TYPES]

[TELEMETRY_TYPES]: provider builder extensions
- rail: telemetry

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]   | [CAPABILITY]                                                                                           |
| :-----: | :-------------------------------- | :--------------- | :----------------------------------------------------------------------------------------------------- |
|   [1]   | `TracerProviderBuilderExtensions` | tracer extension | `AddNpgsql(TracerProviderBuilder) : TracerProviderBuilder`                                             |
|   [2]   | `MeterProviderBuilderExtensions`  | meter extension  | `AddNpgsqlInstrumentation(MeterProviderBuilder, Action<NpgsqlMetricsOptions>?) : MeterProviderBuilder` |

`NpgsqlMetricsOptions` is declared by the `Npgsql` driver package (parameterless ctor; histogram and cardinality posture ride OpenTelemetry meter-view configuration, not options properties). Tracing depth — command/batch/COPY filters, span-name providers, enrichment callbacks, `EnableFirstResponseEvent`, `EnablePhysicalOpenTracing` — is configured via `NpgsqlDataSourceBuilder.ConfigureTracing(Action<NpgsqlTracingOptionsBuilder>)`, which is a driver-level concern catalogued in `api-npgsql.md`.

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: tracing wiring
- rail: telemetry

| [INDEX] | [SURFACE]   | [CALL_SHAPE]                                               | [CAPABILITY]            |
| :-----: | :---------- | :--------------------------------------------------------- | :---------------------- |
|   [1]   | `AddNpgsql` | `AddNpgsql(TracerProviderBuilder) : TracerProviderBuilder` | subscribes Npgsql spans |

[ENTRYPOINT_SCOPE]: metrics wiring
- rail: telemetry

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                           | [CAPABILITY]             |
| :-----: | :------------------------- | :----------------------------------------------------------------------------------------------------- | :----------------------- |
|   [1]   | `AddNpgsqlInstrumentation` | `AddNpgsqlInstrumentation(MeterProviderBuilder, Action<NpgsqlMetricsOptions>?) : MeterProviderBuilder` | subscribes Npgsql meters |

## [4]-[IMPLEMENTATION_LAW]

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
