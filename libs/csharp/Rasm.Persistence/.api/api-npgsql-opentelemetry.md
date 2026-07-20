# [RASM_PERSISTENCE_API_NPGSQL_OPENTELEMETRY]

`Npgsql.OpenTelemetry` subscribes the driver's native span and meter emission onto the provider builders over Persistence-owned data sources; the AppHost composition root registers both verbs and Persistence keeps every depth knob at its own builder seam. Substrate canonical members live at `libs/csharp/.api/api-npgsql-opentelemetry.md`; this overlay carries only the Persistence delta — the registration altitudes, the query-depth split, and the knob-free options ruling the store pages compose.

## [01]-[SUBSTRATE_CANONICAL]

[SUBSTRATE_CANONICAL]: `libs/csharp/.api/api-npgsql-opentelemetry.md`
- extension type roster, the `"Npgsql"` meter instrument table, and package/asset facts live on the substrate catalog — this overlay never re-states them
- rail: storage instrumentation

## [02]-[PERSISTENCE_BINDINGS]

- `Store/provisioning#STORE_PROFILE` carries the observability row — `AddNpgsql()` on the tracer builder and `AddNpgsqlInstrumentation()` on the meter builder — as registration data the AppHost root consumes, two altitudes on the same registration stack the `StoreInterceptor` rail composes, never code inside an operation body.
- Tracing depth binds at the Persistence-owned `NpgsqlDataSourceBuilder.ConfigureTracing` seam and pool identity at `NpgsqlDataSourceBuilder.Name`, so the driver-layer knobs stay on the data-source builders the store profiles own.
- Driver spans compose beside EF Core's native `Activity` emission into one trace, so the beta EF instrumentation package stays rejected — native emission already carries those spans.

## [03]-[IMPLEMENTATION_LAW]

[QUERY_DEPTH_SPLIT]:
- driver-client leg: this package's meter/span subscription — operation duration, pool level, byte volume at the AppHost root
- server-truth leg: the `pg_stat_statements`/`pg_stat_io` harvest receipts under the `store.stat.statements`/`store.stat.io` slots (`Store/observability#PG_STAT_HARVEST`) — per-statement and per-backend evidence this package never carries
- `auto_explain` is server-log posture the provisioning roster owns, outside the managed rail

[LOCAL_ADMISSION]:
- `NpgsqlMetricsOptions` is knob-free, so bucketing and cardinality posture ride `AddView` rows on the meter builder; the estate subscribes the `Npgsql` meter by name and `AddNpgsqlInstrumentation` is the equivalent package spelling whose delegate adds nothing over it.
- Telemetry wiring never leaks into store profiles or query surfaces; span and meter names stay driver facts, never Persistence vocabulary.

[RAIL_LAW]:
- Package: `Npgsql.OpenTelemetry`
- Owns: the Persistence registration-row contribution and the data-source-level depth seam
- Accept: composition-root tracer and meter admission as registration-stack altitudes; `ConfigureTracing`/`Name` depth on Persistence builders
- Reject: telemetry calls inside store profiles; a beta EF instrumentation package where native `Activity` emission already carries the spans
