# [RASM_APPHOST_API_OTEL_HOSTING]

`OpenTelemetry.Extensions.Hosting` supplies Generic Host registration for OpenTelemetry tracing and metrics providers.

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

- api_role: builder surface
- local_use: constructs configured root

| [INDEX] | [SYMBOL]                         |
| :-----: | :------------------------------- |
|   [1]   | `OpenTelemetryBuilder`           |
|   [2]   | `OpenTelemetryBuilderExtensions` |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: hosting operations
- rail: telemetry

| [INDEX] | [SURFACE]           | [CALL_SHAPE]           | [CAPABILITY]              |
| :-----: | :------------------ | :--------------------- | :------------------------ |
|   [1]   | `AddOpenTelemetry`  | DI extension           | admits configured surface |
|   [2]   | `WithTracing`       | fluent option          | applies policy value      |
|   [3]   | `WithMetrics`       | fluent option          | applies policy value      |
|   [4]   | `ConfigureResource` | configuration delegate | applies policy value      |
|   [5]   | `WithLogging`       | fluent option          | applies policy value      |

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
