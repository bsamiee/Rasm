# [RASM_APPHOST_API_HEALTH]

`Microsoft.Extensions.Diagnostics.HealthChecks` supplies named health registration, typed health contributors, health reports, service evaluation, and publisher cadence.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Diagnostics.HealthChecks`
- package: `Microsoft.Extensions.Diagnostics.HealthChecks`
- assembly: `Microsoft.Extensions.Diagnostics.HealthChecks`
- contract_assembly: `Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions`
- namespace: `Microsoft.Extensions.Diagnostics.HealthChecks`
- asset: runtime library
- rail: health

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: health runtime family
- rail: health

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]      | [RAIL]               |
| :-----: | :---------------------------- | :----------------- | :------------------- |
|  [01]   | `HealthCheckService`          | evaluator service  | health execution     |
|  [02]   | `HealthCheckServiceOptions`   | registration store | contributor registry |
|  [03]   | `HealthCheckPublisherOptions` | publisher policy   | report cadence       |
|  [04]   | `IHealthChecksBuilder`        | builder contract   | registration rail    |

[PUBLIC_TYPE_SCOPE]: health contract family
- rail: health

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]        | [RAIL]                     |
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
- rail: health

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY]        | [RAIL]                 |
| :-----: | :------------------------- | :-------------------- | :--------------------- |
|  [01]   | `AddHealthChecks`          | service registration  | opens health builder   |
|  [02]   | `IHealthChecksBuilder.Add` | registration insert   | admits registration    |
|  [03]   | `AddCheck<T>`              | typed contributor     | typed probe admission  |
|  [04]   | `AddCheck(instance)`       | instance contributor  | fixed probe admission  |
|  [05]   | `AddCheck(delegate)`       | delegate contributor  | inline probe admission |
|  [06]   | `AddAsyncCheck`            | async delegate        | async probe admission  |
|  [07]   | `AddTypeActivatedCheck<T>` | activated contributor | argument-bound probe   |

[ENTRYPOINT_SCOPE]: evaluation and result operations
- rail: health

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]      | [RAIL]                    |
| :-----: | :------------------------------ | :------------------ | :------------------------ |
|  [01]   | `CheckHealthAsync()`            | full evaluation     | evaluates all checks      |
|  [02]   | `CheckHealthAsync(predicate)`   | filtered evaluation | evaluates selected checks |
|  [03]   | `IHealthCheck.CheckHealthAsync` | contributor probe   | emits contributor result  |
|  [04]   | `PublishAsync`                  | publisher callback  | emits aggregate report    |
|  [05]   | `HealthCheckResult.Healthy`     | result factory      | emits healthy result      |
|  [06]   | `HealthCheckResult.Degraded`    | result factory      | emits degraded result     |
|  [07]   | `HealthCheckResult.Unhealthy`   | result factory      | emits unhealthy result    |

## [04]-[IMPLEMENTATION_LAW]

[HEALTH_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Diagnostics.HealthChecks`
- result states: healthy, degraded, unhealthy
- contributor contract: `IHealthCheck.CheckHealthAsync`
- registration shape: name, factory, failure status, tags, timeout, delay, period
- report shape: aggregate status plus keyed `HealthReportEntry` values
- entry payload: status, description, duration, exception, data, tags
- publisher shape: `IHealthCheckPublisher` emits reports on configured cadence
- publisher policy: delay, period, predicate, timeout

[LOCAL_ADMISSION]:
- Health checks project capability state; they do not own runtime state.
- Named checks map to package capability contracts and carry tags for filtered projection.
- Failure status and timeout are explicit registration policy, not ad hoc exception handling.
- Degraded health maps to typed degradation receipts with usable capability detail.
- Publishers are projection surfaces and never replace support receipts.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Diagnostics.HealthChecks`
- Owns: capability health projection
- Accept: health maps to runtime state
- Reject: string status probes
