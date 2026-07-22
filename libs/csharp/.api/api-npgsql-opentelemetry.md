# [RASM_API_NPGSQL_OPENTELEMETRY]

`Npgsql.OpenTelemetry` admits the driver's own telemetry by name: one tracer verb subscribes the `"Npgsql"` `ActivitySource`, one meter verb subscribes the `"Npgsql"` meter. Neither verb installs a listener, subscriber, or processor, so span shaping binds at the data-source builder and stream shaping at provider view rows.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.OpenTelemetry`
- package: `Npgsql.OpenTelemetry` (PostgreSQL, The Npgsql Development Team)
- assembly: `Npgsql.OpenTelemetry`
- namespace: `Npgsql`
- depends: `Npgsql`
- rail: storage instrumentation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider-builder extension holders, one per signal

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]               |
| :-----: | :-------------------------------- | :------------ | :------------------------- |
|  [01]   | `TracerProviderBuilderExtensions` | class         | tracer-side admission verb |
|  [02]   | `MeterProviderBuilderExtensions`  | class         | meter-side admission verb  |

[PUBLIC_TYPE_SCOPE]: `"Npgsql"` meter instruments, every row tagged `db.client.connection.pool.name` from `NpgsqlDataSourceBuilder.Name`

| [INDEX] | [INSTRUMENT]                                   | [UNIT]         | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `db.client.operation.duration`                 | `s`            | operation duration histogram                    |
|  [02]   | `db.client.operation.failed`                   | `{command}`    | failed command count                            |
|  [03]   | `db.client.operation.npgsql.executing`         | `{command}`    | in-flight command level                         |
|  [04]   | `db.client.operation.npgsql.bytes_read`        | `By`           | read byte volume                                |
|  [05]   | `db.client.operation.npgsql.bytes_written`     | `By`           | written byte volume                             |
|  [06]   | `db.client.operation.npgsql.prepared_ratio`    | —              | prepared-execution percentage per collection    |
|  [07]   | `db.client.connection.count`                   | `{connection}` | occupancy split by `db.client.connection.state` |
|  [08]   | `db.client.connection.max`                     | `{connection}` | pool ceiling                                    |
|  [09]   | `db.client.connection.npgsql.create_time`      | `s`            | physical connection create histogram            |
|  [10]   | `db.client.connection.npgsql.pending_requests` | `{request}`    | queued open-connection requests                 |
|  [11]   | `db.client.connection.npgsql.timeouts`         | `{timeout}`    | pool acquisition timeouts                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: name subscription on the provider builders

| [INDEX] | [SURFACE]                                                                       | [SHAPE] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------------------ | :------ | :----------------------------------------- |
|  [01]   | `AddNpgsql(TracerProviderBuilder)`                                              | static  | subscribes the `"Npgsql"` `ActivitySource` |
|  [02]   | `AddNpgsqlInstrumentation(MeterProviderBuilder, Action<NpgsqlMetricsOptions>?)` | static  | subscribes the `"Npgsql"` meter            |

- `AddNpgsqlInstrumentation`: `NpgsqlMetricsOptions` declares no members.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Both verbs resolve to one name row, installing no listener, subscriber, or processor.
- Driver emission gates on the subscription: `ActivitySource.HasListeners()` and each instrument's `Enabled` short-circuit ahead of any tag build, so an unsubscribed process pays one predicate per operation.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): each verb is one builder row equal to its `AddSource`/`AddMeter` spelling, and `AddView` rows shape the resulting streams.
- `OpenTelemetry.Extensions.Hosting`(`api-opentelemetry-hosting.md`): both rows land inside the `WithTracing` and `WithMetrics` delegates `AddOpenTelemetry()` mints.
- `System.Diagnostics.DiagnosticSource`(`api-diagnostics-activity.md`): the subscribed source mints `Client`-kind spans over commands, COPY operations, and physical opens, tagged `db.query.text`, `db.operation.name`, `db.namespace`, and `db.response.status_code` from the SQLSTATE.
- `Npgsql`(`Rasm.Persistence/.api/api-npgsql.md`): `NpgsqlDataSourceBuilder.ConfigureTracing` folds every span filter, name provider, and enrichment callback into one per-data-source pass, and `Name` keys the pool dimension every instrument tags — one builder shapes both signals.

[LOCAL_ADMISSION]:
- Each data source sets `Name` per logical database; an unnamed source folds every pool series onto one default key.
- Provider builders alone reference this package — `Npgsql` carries every emitting surface, so no library-tier reference forms.

[RAIL_LAW]:
- Package: `Npgsql.OpenTelemetry`
- Owns: name-level admission of the driver `ActivitySource` and meter onto the provider builders
- Accept: one tracer row and one meter row per provider, span depth at the data-source builder, stream depth at view rows
- Reject: a hand-rolled ADO.NET span or duration meter over `NpgsqlCommand`
