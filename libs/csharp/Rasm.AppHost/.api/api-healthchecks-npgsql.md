# [RASM_APPHOST_API_HEALTHCHECKS_NPGSQL]

`AspNetCore.HealthChecks.NpgSql` (Xabaril) mints one concrete `NpgSqlHealthCheck : IHealthCheck` proving PostgreSQL reachability through a scalar query over an admitted `Npgsql` connection or the SAME pooled `NpgsqlDataSource` the Persistence store owns. It enters the AppHost capability-health fold as one `Store`-tagged contributor, so a faulted database degrades the host through the health rail rather than a thrown exception.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AspNetCore.HealthChecks.NpgSql`
- package: `AspNetCore.HealthChecks.NpgSql`
- license: `Apache-2.0`
- assembly: `HealthChecks.NpgSql`
- namespace: `HealthChecks.NpgSql`
- namespace: `Microsoft.Extensions.DependencyInjection`
- target: `net8.0` (also `netstandard2.0`); the `net10.0` consumer binds the `net8.0` asset, `RefSafetyRules(11)` nullable-annotated
- dependency floor: `Microsoft.Extensions.Diagnostics.HealthChecks` (`IHealthCheck`/`HealthCheckResult`/`HealthCheckRegistration`), `Npgsql` (`NpgsqlConnection`, `NpgsqlCommand`, `NpgsqlDataSource`)
- asset: runtime library
- rail: health

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: probe and options family

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]        | [CAPABILITY]                                      |
| :-----: | :------------------------- | :------------------- | :------------------------------------------------ |
|  [01]   | `NpgSqlHealthCheck`        | `IHealthCheck` probe | Postgres scalar-query reachability                |
|  [02]   | `NpgSqlHealthCheckOptions` | probe options        | connection-string XOR data-source + query + hooks |

[PUBLIC_MEMBER_SCOPE]: `NpgSqlHealthCheckOptions`

| [INDEX] | [MEMBER]                                                     | [SHAPE]           |
| :-----: | :----------------------------------------------------------- | :---------------- |
|  [01]   | `NpgSqlHealthCheckOptions(string connectionString)`          | ctor              |
|  [02]   | `NpgSqlHealthCheckOptions(NpgsqlDataSource dataSource)`      | ctor              |
|  [03]   | `string? ConnectionString { get; internal set; }`            | connection source |
|  [04]   | `NpgsqlDataSource? DataSource { get; internal set; }`        | pooled source     |
|  [05]   | `string CommandText { get; set; }`                           | probe query       |
|  [06]   | `Action<NpgsqlConnection>? Configure { get; set; }`          | pre-open hook     |
|  [07]   | `Func<object?, HealthCheckResult>? HealthCheckResultBuilder` | result grader     |

[MEMBER_BEHAVIOR]:
- `NpgSqlHealthCheckOptions(string)`: throws `ArgumentNullException` on empty; `ConnectionString`/`DataSource` are `internal set`, mutually exclusive, and enter only through a ctor.
- `CommandText`: defaults `"SELECT 1;"` and throws on empty at probe build.
- `Configure`: runs before `OpenAsync` for per-connection setup.
- `HealthCheckResultBuilder`: grades the scalar `object?`; a null builder returns `Healthy()`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration and probe operations (`NpgSqlHealthCheckBuilderExtensions`, default name `"npgsql"`)

Every `AddNpgSql` extends `IHealthChecksBuilder` and ends with `string? name`, `HealthStatus? failureStatus`, `IEnumerable<string>? tags`, `TimeSpan? timeout`; the string, factory, and data-source forms carry `string healthQuery = "SELECT 1;"` and `Action<NpgsqlConnection>? configure` before that suffix. Each overload adds one `HealthCheckRegistration(name ?? "npgsql", ...)`, and a null `failureStatus` defaults to `HealthStatus.Unhealthy`.

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `AddNpgSql(string)`                                                         | static   | admit a fixed connection string       |
|  [02]   | `AddNpgSql(Func<IServiceProvider, string>)`                                 | static   | resolve the connection string from DI |
|  [03]   | `AddNpgSql(Func<IServiceProvider, NpgsqlDataSource>?)`                      | static   | share the pooled DI data-source       |
|  [04]   | `AddNpgSql(NpgSqlHealthCheckOptions)`                                       | static   | admit the full options surface        |
|  [05]   | `NpgSqlHealthCheck.CheckHealthAsync(HealthCheckContext, CancellationToken)` | instance | open, scalar-probe, grade             |

- `NpgSqlHealthCheck.CheckHealthAsync`: any exception maps to the registration `FailureStatus`, and the connection and command dispose in `finally`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `NpgSqlHealthCheck : IHealthCheck` is the whole surface — `NpgSqlHealthCheckOptions` (two ctors, connection-string XOR data-source), the four `AddNpgSql` overloads, one scalar probe; no sync mirror, no multi-statement probe, no schema/migration assertion.
- `CheckHealthAsync` opens from `DataSource.CreateConnection()` or `new NpgsqlConnection(ConnectionString)`, runs `Configure`, `OpenAsync`, `ExecuteScalarAsync(CommandText)`, then grades through `HealthCheckResultBuilder` or `Healthy()`.
- Overload [03] with a null `dbDataSourceFactory` resolves `sp.GetRequiredService<NpgsqlDataSource>()` and caches it on the captured options, so the probe opens from the SAME pool the application uses and pool exhaustion surfaces as a degraded probe.

[STACKING]:
- `api-health.md`(`.api/api-health.md`): `NpgSqlHealthCheck` implements the `IHealthCheck` the fold registers through `HealthCheckRegistration`, and a faulted scalar returns a typed `HealthCheckResult` folded by `HealthReport.Snapshot` into a `HealthSnapshot.Entry`, never an exception crossing the report.
- health fold (`.planning/Observability/health.md`): `HealthContributorRow.Driver(DriverProbe.Postgres, cadence, NpgSqlHealthCheck)` adapts the check into one `Store`-tagged contributor through the one driver adapter, and overload [03]'s null `dbDataSourceFactory` binds the SAME pooled `NpgsqlDataSource` the Persistence store owns — connection credentials, type mappings, and pool settings defined once.

[LOCAL_ADMISSION]:
- Overload [03] (null factory) binds the pooled `NpgsqlDataSource` the Persistence store registers, never re-spelling a connection vocabulary or opening an out-of-pool connection.
- `CommandText` stays the cheap reachability probe (`SELECT 1;`); a deeper assertion (replication lag, a sentinel row) rides `HealthCheckResultBuilder` over a richer scalar, never a second registration.

[RAIL_LAW]:
- Package: `AspNetCore.HealthChecks.NpgSql`
- Owns: PostgreSQL reachability as one `Store`-tagged contributor probe
- Accept: a shared pooled `NpgsqlDataSource`, a cheap reachability query, an optional scalar grader
- Reject: an out-of-pool connection vocabulary, a migration/schema assertion in the probe, or a thrown probe failure crossing the health fold
