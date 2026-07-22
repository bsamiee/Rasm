# [RASM_APPHOST_API_HEALTHCHECKS_NATS]

`AspNetCore.HealthChecks.Nats` (Xabaril) mints one sealed `IHealthCheck` that proves NATS broker reachability by opening the injected `INatsConnection` — the pooled connection the app already registers, resolved from DI unless a factory overrides it. Connection liveness IS the reachability signal: the probe carries no options type, message factory, or result detail. It enters the AppHost capability-health fold as one `remote`-tagged contributor row over the shared pooled connection, so broker degradation routes through the existing `ReducedRemote` degradation rule.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AspNetCore.HealthChecks.Nats`
- package: `AspNetCore.HealthChecks.Nats`
- license: `Apache-2.0`
- assembly: `HealthChecks.Nats`
- namespace: `HealthChecks.Nats`, `Microsoft.Extensions.DependencyInjection`
- target: `net8.0` (also `netstandard2.0`); the `net10.0` consumer binds the `net8.0` asset, `RefSafetyRules(11)` nullable-annotated
- depends: `Microsoft.Extensions.Diagnostics.HealthChecks` (`IHealthCheck`/`HealthCheckResult`/`HealthCheckRegistration`) admitted in this folder; `NATS.Net` (`INatsConnection`/`INatsClient`/`NatsConnection`) arrives transitively, so the folder holds no direct `NATS.Net` reference
- asset: runtime library
- rail: health

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: probe and registration family

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]        | [CAPABILITY]                          |
| :-----: | :--------------------------------- | :------------------- | :------------------------------------ |
|  [01]   | `NatsHealthCheck`                  | `IHealthCheck` probe | NATS connection-liveness reachability |
|  [02]   | `NatsHealthCheckBuilderExtensions` | static extensions    | `AddNats` registration                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration and probe (`NatsHealthCheckBuilderExtensions`, default name `"nats"`)

`AddNats` ends with the shared Xabaril registration suffix `string? name = "nats"`, `HealthStatus? failureStatus`, `IEnumerable<string>? tags`, `TimeSpan? timeout`.

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]         |
| :-----: | :------------------------------------------------------------------------ | :------- | :------------------- |
|  [01]   | `AddNats(Func<IServiceProvider, INatsConnection>?)`                       | static   | connection admission |
|  [02]   | `NatsHealthCheck.CheckHealthAsync(HealthCheckContext, CancellationToken)` | instance | reachability probe   |

- `AddNats`: a null `clientFactory` resolves the concrete `NatsConnection` from DI, else falls back to `INatsConnection`, so the probe shares the app's pooled connection; `failureStatus` null defaults to `Unhealthy`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One sealed `NatsHealthCheck(INatsConnection) : IHealthCheck`; connection liveness is the reachability signal, so the probe carries no options overload, message factory, result-detail dictionary, sync mirror, or JetStream/consumer assertion.
- `CheckHealthAsync` awaits `((INatsClient)connection).ConnectAsync()` — idempotent on an open `NatsConnection`, a cheap liveness ping rather than a reconnect — returning `Healthy()` on success and a message-less `Unhealthy()` on any caught exception.

[STACKING]:
- `api-health`(`.api/api-health.md`): the probe implements the `Microsoft.Extensions.Diagnostics.HealthChecks` abstractions `IHealthCheck`/`HealthCheckResult`/`HealthCheckRegistration` that `AddNats` registers.
- within-lib: `AddNats` binds the SAME pooled `NatsConnection` the durable-drain/broker publish rail composes, and the health fold admits `NatsHealthCheck` as the `DriverProbe.Nats` (`Remote`-tagged) contributor row that `HealthReport.Snapshot` projects, so a broker partition degrades the publish path and the probe in lockstep.

[LOCAL_ADMISSION]:
- Admitted as one `Remote`-tagged contributor row over the shared pooled `INatsConnection` — the `DriverProbe.Nats` row, never a parallel `AddNats` registration face or a second connection vocabulary; `NatsOpts` (URL, TLS, auth, ping cadence) is defined once on the app's connection.
- A connect failure crosses the fold as a typed `HealthCheckResult`, never a thrown exception; the message-less `Unhealthy()` is enriched with name and tag at the row since the package attaches no detail.

[RAIL_LAW]:
- Package: `AspNetCore.HealthChecks.Nats`
- Owns: NATS broker reachability as one `remote`-tagged contributor probe over the shared `INatsConnection`
- Accept: the pooled `NatsConnection` resolved from DI or a factory, and a bounded probe cadence
- Reject: a second NATS connection vocabulary, a JetStream/consumer assertion in the probe, or a thrown probe failure crossing the health fold
