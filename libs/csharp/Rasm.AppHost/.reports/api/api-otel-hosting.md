# [RASM_APPHOST_API_OTEL_HOSTING]

`OpenTelemetry.Extensions.Hosting` supplies Generic Host registration for OpenTelemetry tracing, metrics, logging, resource configuration, and provider lifetime.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Extensions.Hosting`
- package: `OpenTelemetry.Extensions.Hosting`
- assembly: `OpenTelemetry.Extensions.Hosting`
- namespace: `OpenTelemetry`
- asset: runtime library
- rail: telemetry

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hosting integration
- rail: telemetry

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                    |
| :-----: | :-------------------------------- | :------------ | :------------------------ |
|   [1]   | `OpenTelemetryBuilder`            | host builder  | telemetry provider setup  |
|   [2]   | `OpenTelemetryServicesExtensions` | DI extension  | service registration      |
|   [3]   | `HostingExtensionsEventSource`    | event source  | hosting integration trace |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: hosting operations
- rail: telemetry

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]    | [RAIL]                  |
| :-----: | :------------------------------ | :---------------- | :---------------------- |
|   [1]   | `AddOpenTelemetry`              | DI extension      | opens telemetry builder |
|   [2]   | `OpenTelemetryBuilder.Services` | service access    | exposes service graph   |
|   [3]   | `ConfigureResource`             | resource delegate | resource identity setup |
|   [4]   | `WithTracing()`                 | tracing admission | default trace provider  |
|   [5]   | `WithTracing(Action)`           | tracing delegate  | trace provider setup    |
|   [6]   | `WithMetrics()`                 | metrics admission | default meter provider  |
|   [7]   | `WithMetrics(Action)`           | metrics delegate  | meter provider setup    |
|   [8]   | `WithLogging(Action)`           | logging delegate  | logger provider setup   |
|   [9]   | `WithLogging(Action, Options)`  | logging delegate  | logger options setup    |

## [4]-[IMPLEMENTATION_LAW]

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
