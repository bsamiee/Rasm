# [RASM_APPHOST_API_OTEL_HOSTING]

`OpenTelemetry.Extensions.Hosting` supplies Generic Host registration for OpenTelemetry tracing, metrics, logging, resource configuration, and provider lifetime.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Extensions.Hosting`
- package: `OpenTelemetry.Extensions.Hosting`
- assembly: `OpenTelemetry.Extensions.Hosting`
- namespace: `OpenTelemetry`
- asset: runtime library
- rail: telemetry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hosting integration
- rail: telemetry

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                   |
| :-----: | :-------------------------------- | :------------ | :----------------------- |
|  [01]   | `OpenTelemetryBuilder`            | host builder  | telemetry provider setup |
|  [02]   | `OpenTelemetryServicesExtensions` | DI extension  | service registration     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: hosting operations
- rail: telemetry

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]    | [RAIL]                  |
| :-----: | :------------------------------ | :---------------- | :---------------------- |
|  [01]   | `AddOpenTelemetry`              | DI extension      | opens telemetry builder |
|  [02]   | `OpenTelemetryBuilder.Services` | service access    | exposes service graph   |
|  [03]   | `ConfigureResource`             | resource delegate | resource identity setup |
|  [04]   | `WithTracing()`                 | tracing admission | default trace provider  |
|  [05]   | `WithTracing(Action)`           | tracing delegate  | trace provider setup    |
|  [06]   | `WithMetrics()`                 | metrics admission | default meter provider  |
|  [07]   | `WithMetrics(Action)`           | metrics delegate  | meter provider setup    |
|  [08]   | `WithLogging()`                 | logging admission | default logger provider |
|  [09]   | `WithLogging(Action)`           | logging delegate  | logger provider setup   |
|  [10]   | `WithLogging(Action, Options)`  | logging delegate  | logger options setup    |

## [04]-[IMPLEMENTATION_LAW]

[HOSTING_TELEMETRY_TOPOLOGY]:
- namespaces: `OpenTelemetry`, `Microsoft.Extensions.DependencyInjection`
- provider rails: tracing, metrics, logging
- resource rail: service identity, instance identity, deployment attributes
- registration surface: `AddOpenTelemetry(IServiceCollection)`
- lifetime: providers are hosted-service owned through the Generic Host

[LOCAL_ADMISSION]:
- Host registration wires tracing, metrics, and logging projection together.
- Resource identity is derived from runtime identity and process identity.
- Provider lifetime follows host lifetime.

[RAIL_LAW]:
- Package: `OpenTelemetry.Extensions.Hosting`
- Owns: host-integrated telemetry setup
- Accept: host registration wires providers
- Reject: manual provider lifetime management
