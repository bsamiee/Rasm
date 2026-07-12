# [RASM_APPHOST_API_HEALTHCHECKS_NPGSQL]

`AspNetCore.HealthChecks.NpgSql` (Xabaril) is a single concrete `IHealthCheck` that proves PostgreSQL reachability by executing a scalar query (`SELECT 1;` by default) over an admitted `Npgsql` connection or — the integration-bearing path — the SAME `NpgsqlDataSource` the Persistence store already pools. It is one `store`-tagged probe row feeding the AppHost capability-health fold; a faulted database routes through the `ReadOnly` degradation rule rather than a thrown exception.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AspNetCore.HealthChecks.NpgSql`

- package: `AspNetCore.HealthChecks.NpgSql`
- license: `Apache-2.0`
- assembly: `HealthChecks.NpgSql`
- namespace: `HealthChecks.NpgSql`
- namespace: `Microsoft.Extensions.DependencyInjection`
- target: `net8.0` (also `netstandard2.0`); the `net10.0` consumer binds the `net8.0` asset, `RefSafetyRules(11)` nullable-annotated
- dependency floor: `Microsoft.Extensions.Diagnostics.HealthChecks` (`IHealthCheck`/`HealthCheckResult`/`HealthCheckRegistration`), `Npgsql` (`NpgsqlConnection`, `NpgsqlCommand`, `NpgsqlDataSource`) — both admitted in this folder at higher pins (`Npgsql` 10.x)
- asset: runtime library
- rail: health

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: probe and options family

- rail: health

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]        | [RAIL]                                           |
| :-----: | :------------------------- | :------------------- | :----------------------------------------------- |
|  [01]   | `NpgSqlHealthCheck`        | `IHealthCheck` probe | Postgres scalar-query reachability               |
|  [02]   | `NpgSqlHealthCheckOptions` | probe options        | connection-string OR data-source + query + hooks |

[PUBLIC_MEMBER_SCOPE]: `NpgSqlHealthCheckOptions`

- rail: health

| [INDEX] | [MEMBER]                                                     | [SHAPE]           |
| :-----: | :----------------------------------------------------------- | :---------------- |
|  [01]   | `NpgSqlHealthCheckOptions(string connectionString)`          | ctor              |
|  [02]   | `NpgSqlHealthCheckOptions(NpgsqlDataSource dataSource)`      | ctor              |
|  [03]   | `string? ConnectionString { get; internal set; }`            | connection source |
|  [04]   | `NpgsqlDataSource? DataSource { get; internal set; }`        | pooled source     |
|  [05]   | `string CommandText { get; set; }`                           | probe query       |
|  [06]   | `Action<NpgsqlConnection>? Configure { get; set; }`          | pre-open hook     |
|  [07]   | `Func<object?, HealthCheckResult>? HealthCheckResultBuilder` | result grader     |

[MEMBER_CONTRACTS]:

- `NpgSqlHealthCheckOptions(string connectionString)` sets `ConnectionString` and throws on empty input.
- `NpgSqlHealthCheckOptions(NpgsqlDataSource dataSource)` sets `DataSource` for pooled-source integration.
- `ConnectionString` is mutually exclusive with `DataSource` and is set only through its constructor.
- `DataSource.CreateConnection()` supplies the connection when `DataSource` is present, so the application source remains shared.
- `CommandText` defaults to `"SELECT 1;"` and throws on empty input during probe construction.
- `Configure` runs before `OpenAsync` for per-connection setup.
- `HealthCheckResultBuilder` grades the scalar result; a null builder returns `Healthy()`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations (`NpgSqlHealthCheckBuilderExtensions`, default name `"npgsql"`)

- rail: health

| [INDEX] | [ADMISSION] | [SOURCE]             |
| :-----: | :---------- | :------------------- |
|  [01]   | string      | connection string    |
|  [02]   | factory     | dependency injection |
|  [03]   | data source | pooled source        |
|  [04]   | options     | options instance     |

[REGISTRATION_CONTRACTS]:

- [STRING]: `AddNpgSql(this IHealthChecksBuilder, string connectionString, string healthQuery = "SELECT 1;", Action<NpgsqlConnection>? configure, string? name, HealthStatus? failureStatus, IEnumerable<string>? tags, TimeSpan? timeout)` admits a fixed connection string.
- [FACTORY]: `AddNpgSql(this IHealthChecksBuilder, Func<IServiceProvider, string> connectionStringFactory, string healthQuery = "SELECT 1;", Action<NpgsqlConnection>? configure, string? name, HealthStatus? failureStatus, IEnumerable<string>? tags, TimeSpan? timeout)` resolves the connection string from dependency injection once.
- [DATA_SOURCE]: `AddNpgSql(this IHealthChecksBuilder, Func<IServiceProvider, NpgsqlDataSource>? dbDataSourceFactory = null, string healthQuery = "SELECT 1;", Action<NpgsqlConnection>? configure, string? name, HealthStatus? failureStatus, IEnumerable<string>? tags, TimeSpan? timeout)` resolves `NpgsqlDataSource` from dependency injection when the factory is null and shares the pooled source.
- [OPTIONS]: `AddNpgSql(this IHealthChecksBuilder, NpgSqlHealthCheckOptions options, string? name, HealthStatus? failureStatus, IEnumerable<string>? tags, TimeSpan? timeout)` admits the full options surface, including `HealthCheckResultBuilder`.

[ENTRYPOINT_SCOPE]: probe operation

- rail: health

[PROBE_OPERATION]:

- Surface: `NpgSqlHealthCheck.CheckHealthAsync(HealthCheckContext, CancellationToken)`.
- Execution: Opens a connection, invokes `ExecuteScalarAsync(CommandText)`, and grades the result.
- Failure: Any exception maps to `FailureStatus`.

## [04]-[IMPLEMENTATION_LAW]

[NPGSQL_TOPOLOGY]:

- one type: `NpgSqlHealthCheck : IHealthCheck`; the public surface is `NpgSqlHealthCheck`, `NpgSqlHealthCheckOptions` (two public ctors), and the four `AddNpgSql` extension overloads. No sync mirror, no multi-statement probe, no schema/migration assertion.
- probe mechanics: `CheckHealthAsync` builds the connection from `DataSource.CreateConnection()` when `DataSource` is set, otherwise `new NpgsqlConnection(ConnectionString)`; invokes `Configure?.Invoke(connection)`, `await OpenAsync(ct)`, sets `CommandText`, `await ExecuteScalarAsync(ct)`, then grades the scalar via `HealthCheckResultBuilder(result)` or `HealthCheckResult.Healthy()`. Connection and command are disposed in `finally`; any exception returns `new HealthCheckResult(context.Registration.FailureStatus, ex.Message, ex)`.
- data-source path: overload [03] with a null `dbDataSourceFactory` resolves `sp.GetRequiredService<NpgsqlDataSource>()` at first probe and caches it on the captured options — the connection the probe opens comes from the SAME pool the application uses, so the probe exercises real pool acquisition (and exhaustion shows up as a degraded probe).
- result grader: `HealthCheckResultBuilder` receives the scalar (`object?`) and returns any `HealthCheckResult`, the seam for asserting the returned value, attaching `Data`, or downgrading to `Degraded`; absent, a successful scalar is `Healthy()`.
- registration policy: every overload adds a `HealthCheckRegistration(name ?? "npgsql", factory, failureStatus, tags, timeout)`; `failureStatus` null defaults to `HealthStatus.Unhealthy`; the connection-string ctor and the `CommandText` setter throw `ArgumentNullException` on empty input at registration/probe-build time.

[LOCAL_ADMISSION]:

- The probe is one `HealthContributorRow.Peer` row tagged `Store`, never a parallel store-health surface — its `Probe` adapts `NpgSqlHealthCheck.CheckHealthAsync` and registers through `HealthSurface.Register`, sharing `DeadlineClass.HealthProbe` and the cadence policy with every other contributor.
- The probe binds the SAME `NpgsqlDataSource` the Persistence store registers (overload [03], null factory), so connection-string credentials, type mappings, and pool settings are defined once; the probe never re-spells a connection vocabulary or opens an out-of-pool connection.
- A connect/query failure is a typed `HealthCheckResult` with `FailureStatus` and `ex.Message`, folded by `HealthReport.Snapshot` into a `HealthSnapshot.Entry` — never a thrown exception crossing the fold.
- `CommandText` stays the cheap reachability probe (`SELECT 1;`); deeper assertions (replication lag, a sentinel row) ride `HealthCheckResultBuilder` over a richer scalar, not a second registration.

[STACK]:

- health fold: `HealthContributorRow.Peer(name: "npgsql", tag: HealthContributorRow.Store, cadence, probe: ct => new ValueTask<HealthCheckResult>(npgsqlCheck.CheckHealthAsync(ctx, ct)))` is the canonical row; `HealthSurface.Register(...)` admits it.
- degradation rail: a `Store`-tagged unhealthy entry drives `Rule(HealthContributorRow.Store, HealthStatus.Unhealthy, DegradationLevel.ReadOnly)` — a faulted Postgres degrades the host to `ReadOnly` (writes shed, reads from cache) with the existing hysteresis, no probe-local branching.
- data-source reuse: the `NpgsqlDataSource` is the one the Persistence layer pools (the same source the EF-Core `Npgsql.EntityFrameworkCore.PostgreSQL` provider and NodaTime/NetTopologySuite plugins bind); the probe shares pool pressure with production queries so pool exhaustion surfaces as a degraded `store` row, and `TenantContext`/RLS (`current_setting('rasm.tenant')`) is set on the shared connection, not invented by the probe.
- wire-health projection: the contributor result flows into `HealthServiceImpl.SetStatus` through the tag-predicate mapping, so store reachability reaches the gRPC health service.
- resilience boundary: the probe deadline is `DeadlineClass.HealthProbe`; database retry is excluded from the `Wire/outbound` hop law (the store execution strategy owns it), so the probe never shares an outbound retry budget.

[RAIL_LAW]:

- Package: `AspNetCore.HealthChecks.NpgSql`
- Owns: PostgreSQL reachability as one `store`-tagged contributor probe
- Accept: a shared `NpgsqlDataSource`, a cheap reachability query, and an optional scalar grader
- Reject: an out-of-pool connection vocabulary, a migration/schema assertion in the probe, or a thrown probe failure crossing the health fold
