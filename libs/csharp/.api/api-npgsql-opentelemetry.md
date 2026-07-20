# [RASM_API_NPGSQL_OPENTELEMETRY]

`Npgsql.OpenTelemetry` subscribes the Npgsql driver's native emission onto the provider builders: one tracer verb admits the `"Npgsql"` `ActivitySource`, one meter verb admits the `"Npgsql"` meter's db-semconv instruments. Depth knobs live on the driver — `NpgsqlDataSourceBuilder.ConfigureTracing` shapes spans per data source, `NpgsqlDataSourceBuilder.Name` keys the pool dimension — so this package is pure composition-root wiring over Persistence-owned data sources.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.OpenTelemetry`
- package: `Npgsql.OpenTelemetry`
- assembly: `Npgsql.OpenTelemetry`
- driver package: `Npgsql` — declares the `ActivitySource`, the meter, `NpgsqlMetricsOptions`, and `NpgsqlTracingOptionsBuilder`
- namespace: `Npgsql`
- asset: runtime library
- rail: storage instrumentation

## [02]-[PUBLIC_TYPES]

[EXTENSION_TYPES]: provider builder extensions
- rail: storage instrumentation

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]   | [CAPABILITY]                               |
| :-----: | :-------------------------------- | :--------------- | :----------------------------------------- |
|  [01]   | `TracerProviderBuilderExtensions` | tracer extension | subscribes the `"Npgsql"` `ActivitySource` |
|  [02]   | `MeterProviderBuilderExtensions`  | meter extension  | subscribes the `"Npgsql"` meter            |

`AddNpgsql(TracerProviderBuilder)` and `AddNpgsqlInstrumentation(MeterProviderBuilder, Action<NpgsqlMetricsOptions>? = default)` are the whole surface; `NpgsqlMetricsOptions` is a knob-free driver carrier, so bucketing and cardinality posture ride meter-view rows on the provider, never options properties.

[METER_ROSTER]: `"Npgsql"` meter instruments (driver-owned; this catalog is the roster's repo carrier)
- rail: storage instrumentation
- dimension: every instrument carries `db.client.connection.pool.name` = `NpgsqlDataSourceBuilder.Name`

| [INDEX] | [INSTRUMENT]                                | [UNIT]         | [CAPABILITY]                   |
| :-----: | :------------------------------------------ | :------------- | :----------------------------- |
|  [01]   | `db.client.operation.duration`              | `s`            | semconv operation histogram    |
|  [02]   | `db.client.connection.count`                | `{connection}` | pool occupancy, dim `state`    |
|  [03]   | `db.client.connection.max`                  | `{connection}` | pool ceiling                   |
|  [04]   | `db.client.operation.npgsql.bytes_read`     | `By`           | driver-namespaced read volume  |
|  [05]   | `db.client.operation.npgsql.bytes_written`  | `By`           | driver-namespaced write volume |
|  [06]   | `db.client.operation.npgsql.executing`      | `{command}`    | in-flight commands             |
|  [07]   | `db.client.operation.npgsql.prepared_ratio` | `1`            | prepared-statement hit ratio   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: admission
- rail: storage instrumentation

| [INDEX] | [SURFACE]                  | [KIND]           | [CAPABILITY]                                                            |
| :-----: | :------------------------- | :--------------- | :---------------------------------------------------------------------- |
|  [01]   | `AddNpgsql`                | trace admission  | `TracerProviderBuilder` — command, batch, COPY, and physical-open spans |
|  [02]   | `AddNpgsqlInstrumentation` | metric admission | `MeterProviderBuilder` + knob-free options delegate                     |

## [04]-[IMPLEMENTATION_LAW]

[DRIVER_TOPOLOGY]:
- subscription root: both verbs register at the AppHost composition root; the driver dependency stays Persistence-owned, and AppHost — a PORT peer with no downward reference — reaches only the source and meter names
- depth root: `NpgsqlDataSourceBuilder.ConfigureTracing(Action<NpgsqlTracingOptionsBuilder>)` — command/batch filters, span-name providers, enrichment callbacks, `EnableFirstResponseEvent(bool)`, `EnablePhysicalOpenTracing(bool)`, and the COPY-operation trio
- pool identity: `NpgsqlDataSourceBuilder.Name` set per logical database so `db.client.connection.pool.name` yields stable dashboard keys instead of connection-string defaults

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): `AddNpgsql` is a one-line convenience over `AddSource("Npgsql")`; `AddMeter("Npgsql")` is the equivalent meter spelling — either row, never both, never a shim.
- pg-server observability (`pg_stat_statements`/`pg_stat_io` harvests) rides Persistence receipt slots and the collector's receiver path — this package carries the driver-client leg only.

[LOCAL_ADMISSION]:
- No third-party ADO.NET instrumentation package exists beside this one for Npgsql, and none is needed — the driver emits natively.
- Dashboards key on the semconv instrument names above; the driver-namespaced `*.npgsql.*` rows are driver facts, never re-minted as Rasm vocabulary.

[RAIL_LAW]:
- Package: `Npgsql.OpenTelemetry`
- Owns: driver span and meter subscription at the composition root
- Accept: two registration rows plus data-source-level tracing depth on the Persistence builders
- Reject: generic ADO.NET instrumentation shims; telemetry wiring inside store profiles or query surfaces
