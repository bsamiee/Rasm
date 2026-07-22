# [RASM_APPHOST_API_HEALTH]

`Microsoft.Extensions.Diagnostics.HealthChecks` mints the capability-health surface every Rasm.AppHost process reads: an `IHealthCheck` contributor that `HealthCheckService` evaluates under `HealthCheckRegistration` policy, aggregates into a `HealthReport`, and pushes on cadence through `IHealthCheckPublisher`. Failure status, tags, and timeout ride the registration, so a faulted capability returns a typed `HealthCheckResult` rather than a thrown exception. Rasm.AppHost adapts every probe into one `HealthContributorRow` fold, and this package is the probe mechanic that fold consumes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Diagnostics.HealthChecks`
- package: `Microsoft.Extensions.Diagnostics.HealthChecks`
- assembly: `Microsoft.Extensions.Diagnostics.HealthChecks`
- assembly: `Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions` (contract types)
- namespace: `Microsoft.Extensions.Diagnostics.HealthChecks`
- asset: runtime library
- rail: health

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: health runtime family

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]      | [CAPABILITY]         |
| :-----: | :---------------------------- | :----------------- | :------------------- |
|  [01]   | `HealthCheckService`          | evaluator service  | health execution     |
|  [02]   | `HealthCheckServiceOptions`   | registration store | contributor registry |
|  [03]   | `HealthCheckPublisherOptions` | publisher policy   | report cadence       |
|  [04]   | `IHealthChecksBuilder`        | builder contract   | registration rail    |

[PUBLIC_TYPE_SCOPE]: health contract family

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]        | [CAPABILITY]               |
| :-----: | :------------------------ | :------------------- | :------------------------- |
|  [01]   | `IHealthCheck`            | contributor contract | capability probe           |
|  [02]   | `HealthCheckContext`      | operation context    | registration input         |
|  [03]   | `HealthCheckRegistration` | named contributor    | probe identity             |
|  [04]   | `HealthCheckResult`       | contributor result   | probe receipt              |
|  [05]   | `HealthReport`            | aggregate report     | health projection          |
|  [06]   | `HealthReportEntry`       | named report entry   | contributor projection     |
|  [07]   | `HealthStatus`            | status enum          | healthy/degraded/unhealthy |
|  [08]   | `IHealthCheckPublisher`   | publisher contract   | report projection          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations

| [INDEX] | [SURFACE]                  | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :------------------------- | :------- | :------------------------------ |
|  [01]   | `AddHealthChecks`          | static   | opens the health builder        |
|  [02]   | `IHealthChecksBuilder.Add` | instance | admits one registration         |
|  [03]   | `AddCheck<T>`              | static   | typed probe admission           |
|  [04]   | `AddCheck(instance)`       | static   | fixed-instance probe admission  |
|  [05]   | `AddCheck(delegate)`       | static   | inline delegate probe admission |
|  [06]   | `AddAsyncCheck`            | static   | async delegate probe admission  |
|  [07]   | `AddTypeActivatedCheck<T>` | static   | argument-bound probe admission  |

[ENTRYPOINT_SCOPE]: evaluation and result operations

| [INDEX] | [SURFACE]                       | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :------------------------------ | :------- | :---------------------------------- |
|  [01]   | `CheckHealthAsync()`            | instance | full evaluation                     |
|  [02]   | `CheckHealthAsync(predicate)`   | instance | evaluation filtered by registration |
|  [03]   | `IHealthCheck.CheckHealthAsync` | instance | one contributor probe               |
|  [04]   | `PublishAsync`                  | instance | publisher callback on cadence       |
|  [05]   | `HealthCheckResult.Healthy`     | factory  | healthy result                      |
|  [06]   | `HealthCheckResult.Degraded`    | factory  | degraded result                     |
|  [07]   | `HealthCheckResult.Unhealthy`   | factory  | unhealthy result                    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every probe folds through `IHealthCheck.CheckHealthAsync`, returning one `HealthCheckResult` of `Healthy`/`Degraded`/`Unhealthy` carrying description, duration, exception, data, and tags.
- `HealthCheckRegistration` carries the probe policy — name, factory, failure status, tags, timeout, delay, period — so failure handling is registration data, never probe-local branching.
- `HealthCheckService` aggregates registrations into a `HealthReport` of keyed `HealthReportEntry` values, and `IHealthCheckPublisher` pushes that report on the `HealthCheckPublisherOptions` cadence of delay, period, predicate, and timeout.

[STACKING]:
- `AspNetCore.HealthChecks.{NpgSql,Redis,Kafka,Nats,Uris,System}`(`.api/api-healthchecks-*.md`): each ships one concrete `IHealthCheck` the `HealthContributorRow.Driver` adapter folds into one contributor row carrying that dependency's tag and failure status, never a parallel `Add*` registration face.
- `HealthContributorRow` adapts every `IHealthCheck` through `Peer`/`Gauge`/`Driver`, `HealthSurface.Register` folds the row span into `HealthCheckRegistration`s, `HealthReport.Snapshot(Instant, CorrelationId)` projects the aggregate into the wire-neutral `HealthSnapshot`, and `DegradationCell` binds as the one `IHealthCheckPublisher`, folding each `HealthReport` into a degradation level in one atomic swap.

[LOCAL_ADMISSION]:
- Health checks project capability state, never own runtime state; a probe returns a typed `HealthCheckResult`, and a thrown exception crossing the fold is the deleted form.
- Every check enters through the one `HealthContributorRow` adapter carrying its tag for filtered projection, never a parallel `Add*` face.
- Degraded maps to a usable-capability receipt, so a faulted dependency degrades the host rather than failing it.
- `IHealthCheckPublisher` is the projection seam and `DegradationCell` the one publisher, never a second health store.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Diagnostics.HealthChecks`
- Owns: capability-health projection — probe evaluation, report aggregation, publisher cadence
- Accept: an `IHealthCheck` folded into a `HealthContributorRow` and graded by typed `HealthCheckResult`
- Reject: a string status probe or a thrown probe failure crossing the health fold
