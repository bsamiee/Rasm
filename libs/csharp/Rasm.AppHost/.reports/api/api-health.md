# [RASM_APPHOST_API_HEALTH]

`Microsoft.Extensions.Diagnostics.HealthChecks` supplies typed health contributors, health reports, and publisher surfaces.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Diagnostics.HealthChecks`
- package: `Microsoft.Extensions.Diagnostics.HealthChecks`
- assembly: `Microsoft.Extensions.Diagnostics.HealthChecks`
- namespace: `Microsoft.Extensions.Diagnostics.HealthChecks`
- asset: runtime library
- rail: health

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: health family
- rail: health

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE]    | [CAPABILITY]              |
| :-----: | :---------------------------- | :---------------- | :------------------------ |
|   [1]   | `IHealthCheck`                | contract surface  | defines boundary contract |
|   [2]   | `HealthCheckResult`           | result value      | projects receipt state    |
|   [3]   | `HealthReport`                | result value      | projects receipt state    |
|   [4]   | `HealthReportEntry`           | result value      | projects receipt state    |
|   [5]   | `HealthStatus`                | health value      | projects health state     |
|   [6]   | `HealthCheckContext`          | operation context | carries operation state   |
|   [7]   | `IHealthCheckPublisher`       | contract surface  | defines boundary contract |
|   [8]   | `HealthCheckPublisherOptions` | policy object     | carries policy input      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: health operations
- rail: health

| [INDEX] | [SURFACE]          | [CALL_SHAPE]       | [CAPABILITY]              |
| :-----: | :----------------- | :----------------- | :------------------------ |
|   [1]   | `AddHealthChecks`  | DI extension       | admits configured surface |
|   [2]   | `AddCheck`         | DI extension       | admits configured surface |
|   [3]   | `CheckHealthAsync` | health probe       | evaluates contributors    |
|   [4]   | `PublishAsync`     | publisher callback | emits health report       |
|   [5]   | `Healthy`          | result factory     | creates healthy result    |
|   [6]   | `Degraded`         | result factory     | creates degraded result   |
|   [7]   | `Unhealthy`        | result factory     | creates unhealthy result  |

## [4]-[IMPLEMENTATION_LAW]

[HEALTH_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Diagnostics.HealthChecks`
- result states: healthy, degraded, unhealthy
- contributor contract: `IHealthCheck.CheckHealthAsync`
- report shape: aggregate status plus keyed `HealthReportEntry` values
- publisher shape: `IHealthCheckPublisher` emits reports on configured cadence

[LOCAL_ADMISSION]:
- Health checks project capability state; they do not own runtime state.
- Degraded health maps to typed degradation receipts with usable capability detail.
- Publishers are projection surfaces and never replace support receipts.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Diagnostics.HealthChecks`
- Owns: capability health projection
- Accept: health maps to runtime state
- Reject: string status probes
