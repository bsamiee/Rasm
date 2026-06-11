# [RASM_APPHOST_API_COMPANION]

Companion APIs supply process bootstrap, dependency composition, configuration, health, telemetry export, outbound resilience, and validation over the AppHost runtime rail.

## [1]-[SURFACES]

This table is a lookup by companion package.

| [INDEX] | [PACKAGE]                                       | [ASSEMBLY]                                | [LOCAL_RAIL] |
| :-----: | :---------------------------------------------- | :---------------------------------------- | :----------- |
|   [1]   | `Microsoft.Extensions.Hosting`                  | `Microsoft.Extensions.Hosting`            | bootstrap    |
|   [2]   | `Microsoft.Extensions.DependencyInjection`      | `Microsoft.Extensions.DependencyInjection` | composition  |
|   [3]   | `Microsoft.Extensions.Configuration`            | `Microsoft.Extensions.Configuration`      | config       |
|   [4]   | `Microsoft.Extensions.Configuration.Binder`     | `Microsoft.Extensions.Configuration.Binder` | config     |
|   [5]   | `Microsoft.Extensions.Options`                  | `Microsoft.Extensions.Options`            | config       |
|   [6]   | `Scrutor`                                       | `Scrutor`                                 | composition  |
|   [7]   | `Microsoft.Extensions.Diagnostics.HealthChecks` | `Microsoft.Extensions.Diagnostics.HealthChecks` | health |
|   [8]   | `Serilog`                                       | `Serilog`                                 | export       |
|   [9]   | `OpenTelemetry`                                 | `OpenTelemetry`                           | export       |
|  [10]   | `OpenTelemetry.Extensions.Hosting`              | `OpenTelemetry.Extensions.Hosting`        | export       |
|  [11]   | `Microsoft.Extensions.Http.Resilience`          | `Microsoft.Extensions.Http.Resilience`    | resilience   |
|  [12]   | `FluentValidation`                              | `FluentValidation`                        | validation   |
|  [13]   | `FluentValidation.DependencyInjectionExtensions` | `FluentValidation.DependencyInjectionExtensions` | validation |

## [2]-[API_LOCATORS]

This table is a lookup by namespace.

| [INDEX] | [ASSEMBLY]                                | [NAMESPACE]                         | [USING]                              | [API_LOCATOR] |
| :-----: | :---------------------------------------- | :---------------------------------- | :----------------------------------- | :------------ |
|   [1]   | `Microsoft.Extensions.Hosting`            | `Microsoft.Extensions.Hosting`      | `Microsoft.Extensions.Hosting`       | `.cache/nuget/packages/microsoft.extensions.hosting/` |
|   [2]   | `Microsoft.Extensions.DependencyInjection` | `Microsoft.Extensions.DependencyInjection` | `Microsoft.Extensions.DependencyInjection` | `.cache/nuget/packages/microsoft.extensions.dependencyinjection/` |
|   [3]   | `Microsoft.Extensions.Configuration`      | `Microsoft.Extensions.Configuration` | `Microsoft.Extensions.Configuration` | `.cache/nuget/packages/microsoft.extensions.configuration/` |
|   [4]   | `Microsoft.Extensions.Options`            | `Microsoft.Extensions.Options`      | `Microsoft.Extensions.Options`       | `.cache/nuget/packages/microsoft.extensions.options/` |
|   [5]   | `Scrutor`                                 | `Microsoft.Extensions.DependencyInjection` | `Microsoft.Extensions.DependencyInjection` | `.cache/nuget/packages/scrutor/` |
|   [6]   | `Microsoft.Extensions.Diagnostics.HealthChecks` | `Microsoft.Extensions.Diagnostics.HealthChecks` | `Microsoft.Extensions.Diagnostics.HealthChecks` | `.cache/nuget/packages/microsoft.extensions.diagnostics.healthchecks/` |
|   [7]   | `Serilog`                                 | `Serilog`                           | `Serilog`                            | `.cache/nuget/packages/serilog/` |
|   [8]   | `OpenTelemetry`                           | `OpenTelemetry`                     | `OpenTelemetry`                      | `.cache/nuget/packages/opentelemetry/` |
|   [9]   | `Microsoft.Extensions.Http.Resilience`    | `Microsoft.Extensions.DependencyInjection` | `Microsoft.Extensions.DependencyInjection` | `.cache/nuget/packages/microsoft.extensions.http.resilience/` |
|  [10]   | `FluentValidation`                        | `FluentValidation`                  | `FluentValidation`                   | `.cache/nuget/packages/fluentvalidation/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]              | [ENTRY_SURFACE]                  | [LOCAL_RAIL] |
| :-----: | :------------------------- | :------------------------------- | :----------- |
|   [1]   | `HostApplicationBuilder`   | companion process bootstrap      | bootstrap    |
|   [2]   | `IServiceCollection`       | service registration             | composition  |
|   [3]   | `IConfiguration`           | configuration tree               | config       |
|   [4]   | `IOptions<T>`              | validated policy value           | config       |
|   [5]   | Scrutor scan API           | assembly scan and decoration     | composition  |
|   [6]   | `IHealthCheck`             | health projection                | health       |
|   [7]   | Serilog logger API         | structured sink projection       | export       |
|   [8]   | OpenTelemetry builder API  | trace and metric exporter        | export       |
|   [9]   | resilience builder API     | outbound HTTP policy             | resilience   |
|  [10]   | `IValidator<T>`            | external shape validation        | validation   |

## [4]-[REJECTED]

This table is a lookup by rejected API.

| [INDEX] | [REJECT]                   | [LOCAL_RAIL] | [REASON]                     |
| :-----: | :------------------------- | :----------- | :--------------------------- |
|   [1]   | service locator access     | composition  | runtime ports own dependency |
|   [2]   | stacked retry handlers     | resilience   | one owner per outbound hop   |
|   [3]   | exporter-owned domain rail | export       | exporters project signals    |
