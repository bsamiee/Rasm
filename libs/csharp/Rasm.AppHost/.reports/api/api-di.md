# [RASM_APPHOST_API_DI]

`Microsoft.Extensions.DependencyInjection` supplies service registration, scope ownership, keyed service lookup, and activation.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.DependencyInjection`
- package: `Microsoft.Extensions.DependencyInjection`
- assembly: `Microsoft.Extensions.DependencyInjection`
- namespace: `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: composition

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: composition family
- rail: composition

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]          | [CAPABILITY]                 |
| :-----: | :---------------------- | :---------------------- | :--------------------------- |
|   [1]   | `IServiceCollection`    | contract surface        | defines boundary contract    |
|   [2]   | `ServiceDescriptor`     | registration descriptor | anchors composition contract |
|   [3]   | `ServiceProvider`       | resolved container      | anchors composition contract |
|   [4]   | `IServiceScope`         | contract surface        | defines boundary contract    |
|   [5]   | `IServiceScopeFactory`  | contract surface        | defines boundary contract    |
|   [6]   | `IKeyedServiceProvider` | contract surface        | defines boundary contract    |
|   [7]   | `ActivatorUtilities`    | activation helper       | anchors composition contract |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: DI operations
- rail: composition

| [INDEX] | [SURFACE]            | [CALL_SHAPE] | [CAPABILITY]              |
| :-----: | :------------------- | :----------- | :------------------------ |
|   [1]   | `AddSingleton`       | DI extension | admits configured surface |
|   [2]   | `AddScoped`          | DI extension | admits configured surface |
|   [3]   | `AddTransient`       | DI extension | admits configured surface |
|   [4]   | `AddKeyedSingleton`  | DI extension | admits configured surface |
|   [5]   | `CreateScope`        | factory call | creates configured handle |
|   [6]   | `GetRequiredService` | lookup call  | resolves typed value      |
|   [7]   | `CreateInstance`     | factory call | creates configured handle |

## [4]-[IMPLEMENTATION_LAW]

[COMPOSITION_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.DependencyInjection.Extensions`
- lifetime families: singleton, scoped, transient, keyed
- descriptor shapes: implementation type, factory, instance
- scope law: root provider owns singleton state; created scopes own scoped disposal
- activation law: `ActivatorUtilities` is boundary activation, not hidden service lookup

[LOCAL_ADMISSION]:
- AppHost ports are constructor-visible dependencies registered at composition roots.
- Keyed services model bounded policy variants where the key is part of AppHost policy.
- Runtime code receives dependencies through explicit records and constructors, never provider lookups.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.DependencyInjection`
- Owns: composition and lifetime scopes
- Accept: registrations stay at composition roots
- Reject: service locator logic

