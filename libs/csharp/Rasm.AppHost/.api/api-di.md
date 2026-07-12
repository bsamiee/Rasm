# [RASM_APPHOST_API_DI]

`Microsoft.Extensions.DependencyInjection` supplies service registration, provider construction, scope ownership, keyed service lookup, and boundary activation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.DependencyInjection`
- package: `Microsoft.Extensions.DependencyInjection`
- assembly: `Microsoft.Extensions.DependencyInjection`
- contract_assembly: `Microsoft.Extensions.DependencyInjection.Abstractions`
- namespace: `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: composition

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider implementation
- rail: composition

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY]     | [RAIL]               |
| :-----: | :-------------------------------------------- | :---------------- | :------------------- |
|  [01]   | `DefaultServiceProviderFactory`               | provider factory  | host provider bridge |
|  [02]   | `ServiceCollectionContainerBuilderExtensions` | provider builder  | root provider build  |
|  [03]   | `ServiceProvider`                             | resolved provider | service resolution   |
|  [04]   | `ServiceProviderOptions`                      | validation policy | provider proof       |

[PUBLIC_TYPE_SCOPE]: composition contracts
- rail: composition

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]          | [RAIL]                  |
| :-----: | :--------------------------------------- | :--------------------- | :---------------------- |
|  [01]   | `IServiceCollection`                     | descriptor collection  | registration boundary   |
|  [02]   | `ServiceCollection`                      | mutable collection     | registration staging    |
|  [03]   | `ServiceDescriptor`                      | descriptor algebra     | registration identity   |
|  [04]   | `ServiceLifetime`                        | lifetime enum          | scope policy            |
|  [05]   | `IServiceScope`                          | disposable scope       | scoped lifetime         |
|  [06]   | `AsyncServiceScope`                      | async disposable scope | async lifetime          |
|  [07]   | `IServiceScopeFactory`                   | scope factory          | scope construction      |
|  [08]   | `IServiceProviderFactory<TBuilder>`      | provider factory seam  | host builder interop    |
|  [09]   | `IServiceProviderIsService`              | availability probe     | optional resolution     |
|  [10]   | `IServiceProviderIsKeyedService`         | keyed availability     | keyed optional lookup   |
|  [11]   | `IKeyedServiceProvider`                  | keyed lookup contract  | keyed policy resolution |
|  [12]   | `KeyedService`                           | keyed sentinel         | enumerable keyed lookup |
|  [13]   | `FromKeyedServicesAttribute`             | parameter attribute    | keyed constructor input |
|  [14]   | `ServiceKeyAttribute`                    | parameter attribute    | key injection           |
|  [15]   | `ServiceKeyLookupMode`                   | keyed lookup enum      | key inheritance         |
|  [16]   | `ActivatorUtilities`                     | boundary activator     | explicit construction   |
|  [17]   | `ActivatorUtilitiesConstructorAttribute` | constructor selector   | activation selection    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations
- rail: composition

| [INDEX] | [SURFACE]                     | [CAPABILITY]                 |
| :-----: | :---------------------------- | :--------------------------- |
|  [01]   | `BuildServiceProvider`        | root provider creation       |
|  [02]   | `AddSingleton`                | singleton registration       |
|  [03]   | `AddScoped`                   | scoped registration          |
|  [04]   | `AddTransient`                | transient registration       |
|  [05]   | `AddKeyedSingleton`           | keyed singleton policy       |
|  [06]   | `AddKeyedScoped`              | keyed scoped policy          |
|  [07]   | `AddKeyedTransient`           | keyed transient policy       |
|  [08]   | `TryAdd`                      | idempotent default           |
|  [09]   | `TryAddEnumerable`            | ordered extension-set add    |
|  [10]   | `TryAddKeyed{Lifetime}`       | idempotent keyed default     |
|  [11]   | `Replace`                     | explicit descriptor override |
|  [12]   | `RemoveAll`                   | unkeyed contract reset       |
|  [13]   | `RemoveAllKeyed`              | keyed contract reset         |
|  [14]   | `ServiceDescriptor.Describe`  | typed descriptor factory     |
|  [15]   | `DescribeKeyed`               | keyed descriptor factory     |
|  [16]   | `ServiceDescriptor.Singleton` | lifetime descriptor factory  |

[LIFETIME_DESCRIPTOR_OVERLOADS]: `ServiceDescriptor.Singleton` exposes `Singleton(Type, object)` plus generic instance and factory overloads, with parallel `Scoped` and `Transient` factories.

[ENTRYPOINT_SCOPE]: resolution and activation operations
- rail: composition

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY]        | [RAIL]                    |
| :-----: | :--------------------------- | :-------------------- | :------------------------ |
|  [01]   | `GetService<T>`              | optional lookup       | nullable contract lookup  |
|  [02]   | `GetRequiredService<T>`      | required lookup       | required contract lookup  |
|  [03]   | `GetServices<T>`             | enumerable lookup     | ordered extension lookup  |
|  [04]   | `GetKeyedService<T>`         | optional keyed lookup | keyed policy lookup       |
|  [05]   | `GetRequiredKeyedService<T>` | required keyed lookup | required keyed policy     |
|  [06]   | `GetKeyedServices<T>`        | keyed enumerable      | keyed extension lookup    |
|  [07]   | `CreateScope`                | scope factory         | synchronous scope         |
|  [08]   | `CreateAsyncScope`           | async scope factory   | asynchronous disposal     |
|  [09]   | `CreateFactory`              | activation compiler   | cached constructor plan   |
|  [10]   | `CreateInstance`             | activation factory    | explicit boundary object  |
|  [11]   | `GetServiceOrCreateInstance` | activation fallback   | optional service fallback |
|  [12]   | `MakeReadOnly`               | collection lock       | registration freeze       |

## [04]-[IMPLEMENTATION_LAW]

[COMPOSITION_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.DependencyInjection.Extensions`
- lifetime families: singleton, scoped, transient, keyed
- descriptor shapes: implementation type, factory, instance
- keyed descriptor shapes: keyed implementation type, keyed factory, keyed instance
- scope law: root provider owns singleton state; created scopes own scoped disposal
- validation law: `ValidateScopes` guards scoped-from-root capture; `ValidateOnBuild` guards provider construction
- activation law: `ActivatorUtilities` is boundary activation, not hidden service lookup

[KEYED_TOPOLOGY]:
- key input: `object` service key
- parameter attributes: `FromKeyedServicesAttribute`, `ServiceKeyAttribute`
- lookup modes: explicit key, inherited key, no inherited key
- sentinel: `KeyedService.AnyKey` selects keyed enumerables and never resolves a single service
- factory shape: keyed factories receive `IServiceProvider` and the service key

[LOCAL_ADMISSION]:
- AppHost ports are constructor-visible dependencies registered at composition roots.
- Keyed services model bounded policy variants where the key is part of AppHost policy.
- Descriptor mutation is allowed only in composition assembly setup.
- Provider validation is enabled for package proof and rejected for runtime probing.
- Runtime code receives dependencies through explicit records and constructors, never provider lookups.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.DependencyInjection`
- Owns: composition and lifetime scopes
- Accept: registrations stay at composition roots
- Reject: service locator logic
