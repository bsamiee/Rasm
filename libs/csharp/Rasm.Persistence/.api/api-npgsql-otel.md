# [RASM_PERSISTENCE_API_NPGSQL_OTEL]

`Npgsql.OpenTelemetry` subscribes the Npgsql ADO.NET driver's `ActivitySource` (spans) and its
metrics reporter (meters) into the OpenTelemetry provider builders. It is a two-method wiring
package: one tracer-builder extension and one meter-builder extension. The span/meter content, the
tracing-depth knobs (filters, span-name providers, enrichment), and `NpgsqlMetricsOptions` itself are
driver-layer concerns owned by `Npgsql` (catalogued in `api-npgsql.md`); this package only admits the
two subscriptions onto the provider builders.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.OpenTelemetry`
- package: `Npgsql.OpenTelemetry`
- license: PostgreSQL (Npgsql's BSD-style license)
- assembly: `Npgsql.OpenTelemetry`
- target: `net8.0` (single-TFM package; binds under the `net10.0` consumer with no fallback ambiguity)
- driver package: `Npgsql` (declares `NpgsqlMetricsOptions`, the `ActivitySource`, and `NpgsqlTracingOptionsBuilder`)
- namespace: `Npgsql` (both extension classes)
- asset: runtime library
- rail: telemetry

## [02]-[PUBLIC_TYPES]

[TELEMETRY_TYPES]: provider builder extensions (namespace `Npgsql`)
- rail: telemetry

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]   | [CAPABILITY]                                    |
| :-----: | :-------------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `TracerProviderBuilderExtensions` | tracer extension | subscribes the Npgsql `ActivitySource` (spans)  |
|  [02]   | `MeterProviderBuilderExtensions`  | meter extension  | subscribes the Npgsql metrics reporter (meters) |

Extension signatures (verified against the package XML/IL):
- `TracerProviderBuilderExtensions.AddNpgsql(TracerProviderBuilder builder) : TracerProviderBuilder` — subscribes the Npgsql activity source to enable OpenTelemetry tracing.
- `MeterProviderBuilderExtensions.AddNpgsqlInstrumentation(MeterProviderBuilder builder, Action<NpgsqlMetricsOptions>? options = default) : MeterProviderBuilder` — subscribes the Npgsql metrics reporter to enable OpenTelemetry metrics.

`NpgsqlMetricsOptions` is declared by the `Npgsql` driver package with a parameterless ctor and **no
settable property knobs** at `10.0.3`; the configuration delegate is the registration seam, not a
property surface. Histogram bucketing and cardinality posture ride OpenTelemetry meter-view
configuration (`AddView`/explicit-bucket boundaries on the `MeterProviderBuilder`), not options
properties.

Tracing depth (per-command filters, span-name providers, enrichment callbacks, first-response-event
emission, physical-open tracing) is configured at the data-source layer via
`NpgsqlDataSourceBuilder.ConfigureTracing(Action<NpgsqlTracingOptionsBuilder>)` — a driver concern
catalogued in `api-npgsql.md`, not a member of this package.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: tracing wiring
- rail: telemetry

[TRACING_WIRING]:
- Surface: `AddNpgsql`
- Call shape: `AddNpgsql(TracerProviderBuilder) : TracerProviderBuilder`
- Capability: subscribes the Npgsql `ActivitySource`; fluent return chains further `AddSource`/`AddProcessor` rows.

[ENTRYPOINT_SCOPE]: metrics wiring
- rail: telemetry

[METRICS_WIRING]:
- Surface: `AddNpgsqlInstrumentation`
- Call shape: `AddNpgsqlInstrumentation(MeterProviderBuilder, Action<NpgsqlMetricsOptions>? = default) : MeterProviderBuilder`
- Capability: subscribes the Npgsql metrics reporter; the optional delegate is the only options seam, and it carries no settable knobs at the admitted version.

## [04]-[IMPLEMENTATION_LAW]

[TELEMETRY_PROFILE]:
- namespace: `Npgsql`
- tracer root: `AddNpgsql` on `TracerProviderBuilder`
- meter root: `AddNpgsqlInstrumentation` on `MeterProviderBuilder` with optional `Action<NpgsqlMetricsOptions>`
- options root: `NpgsqlMetricsOptions` declared by the `Npgsql` driver; parameterless ctor; no settable property knobs at `10.0.3` — histogram/cardinality posture rides OpenTelemetry meter-view configuration
- tracing depth root: `NpgsqlTracingOptionsBuilder` via `NpgsqlDataSourceBuilder.ConfigureTracing` — driver-layer concern, not an OTel package concern

[STACKING]:
- `AddNpgsql` and `AddNpgsqlInstrumentation` register as two further altitudes on the same effect-transformer registration stack the `StoreInterceptor` rail composes (`Traces` rides `AddNpgsql(TracerProviderBuilder)`, `Meters` rides `AddNpgsqlInstrumentation(MeterProviderBuilder, Action<NpgsqlMetricsOptions>)`), so trace and meter admission are registration rows beside the EF and linq2db interceptor altitudes, never code inside an operation body.
- These spans sit beside EF Core 10's native `Activity` emission (the EF-level command spans) and the data-source-layer `ConfigureTracing` depth knobs, so the driver-source spans and the EF-source spans compose into one trace without a beta EF-instrumentation package — the beta EF/gRPC OpenTelemetry instrumentation packages stay rejected because native `Activity` emission already carries those spans.
- The metrics subscription is the connection/command-counter source; query-observability depth (slow-query rows, plan capture) rides the Postgres-side `pg_stat_statements` read view and the `auto_explain` GUC posture as a separate `store.command.slow`/`store.command.plan` fact stream, so this package owns only the driver meter/span subscription, not the query-stats surface.
- Because `NpgsqlMetricsOptions` is knob-free, all bucketing/cardinality control is expressed as OpenTelemetry meter-view rows on the `MeterProviderBuilder`, so the meter posture composes through the standard OTel view pipeline rather than a provider-specific options object.

[LOCAL_ADMISSION]:
- Npgsql telemetry enters through the composition-root telemetry pipeline only, as two builder-registration rows.
- Span and meter names are driver facts, not Persistence vocabulary.
- Metrics options are declared by the driver and configured (knob-free) at the telemetry boundary; histogram/cardinality posture is meter-view configuration.
- Telemetry wiring cannot leak into store profiles or query surfaces.
- Tracing depth (filters, span-name providers, enrichment, first-response event, physical-open tracing) belongs to `NpgsqlDataSourceBuilder.ConfigureTracing`, not to this package.

[RAIL_LAW]:
- Package: `Npgsql.OpenTelemetry`
- Owns: Npgsql OpenTelemetry span and meter subscription onto the provider builders
- Accept: composition-root tracer and meter admission as registration-stack altitudes
- Reject: telemetry calls inside store profiles; a beta EF/gRPC instrumentation package where native `Activity` emission already carries the spans
