# [RASM_APPHOST_API_DI]

`Microsoft.Extensions.DependencyInjection` supplies service registration, provider construction, scope ownership, keyed service lookup, and boundary activation.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.DependencyInjection`
- package: `Microsoft.Extensions.DependencyInjection`
- assembly: `Microsoft.Extensions.DependencyInjection`
- contract_assembly: `Microsoft.Extensions.DependencyInjection.Abstractions`
- namespace: `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: composition

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider implementation
- rail: composition

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY]     | [RAIL]               |
| :-----: | :-------------------------------------------- | :---------------- | :------------------- |
|   [1]   | `DefaultServiceProviderFactory`               | provider factory  | host provider bridge |
|   [2]   | `ServiceCollectionContainerBuilderExtensions` | provider builder  | root provider build  |
|   [3]   | `ServiceProvider`                             | resolved provider | service resolution   |
|   [4]   | `ServiceProviderOptions`                      | validation policy | provider proof       |

[PUBLIC_TYPE_SCOPE]: composition contracts
- rail: composition

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]          | [RAIL]                  |
| :-----: | :--------------------------------------- | :--------------------- | :---------------------- |
|   [1]   | `IServiceCollection`                     | descriptor collection  | registration boundary   |
|   [2]   | `ServiceCollection`                      | mutable collection     | registration staging    |
|   [3]   | `ServiceDescriptor`                      | descriptor algebra     | registration identity   |
|   [4]   | `ServiceLifetime`                        | lifetime enum          | scope policy            |
|   [5]   | `IServiceScope`                          | disposable scope       | scoped lifetime         |
|   [6]   | `AsyncServiceScope`                      | async disposable scope | async lifetime          |
|   [7]   | `IServiceScopeFactory`                   | scope factory          | scope construction      |
|   [8]   | `IServiceProviderFactory<TBuilder>`      | provider factory seam  | host builder interop    |
|   [9]   | `IServiceProviderIsService`              | availability probe     | optional resolution     |
|  [10]   | `IServiceProviderIsKeyedService`         | keyed availability     | keyed optional lookup   |
|  [11]   | `IKeyedServiceProvider`                  | keyed lookup contract  | keyed policy resolution |
|  [12]   | `KeyedService`                           | keyed sentinel         | enumerable keyed lookup |
|  [13]   | `FromKeyedServicesAttribute`             | parameter attribute    | keyed constructor input |
|  [14]   | `ServiceKeyAttribute`                    | parameter attribute    | key injection           |
|  [15]   | `ServiceKeyLookupMode`                   | keyed lookup enum      | key inheritance         |
|  [16]   | `ActivatorUtilities`                     | boundary activator     | explicit construction   |
|  [17]   | `ActivatorUtilitiesConstructorAttribute` | constructor selector   | activation selection    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations
- rail: composition

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY]           | [RAIL]                 |
| :-----: | :--------------------------- | :----------------------- | :--------------------- |
|   [1]   | `BuildServiceProvider`       | provider build           | root provider creation |
|   [2]   | `AddSingleton`               | lifetime registration    | singleton admission    |
|   [3]   | `AddScoped`                  | lifetime registration    | scoped admission       |
|   [4]   | `AddTransient`               | lifetime registration    | transient admission    |
|   [5]   | `AddKeyedSingleton`          | keyed registration       | keyed singleton policy |
|   [6]   | `AddKeyedScoped`             | keyed registration       | keyed scoped policy    |
|   [7]   | `AddKeyedTransient`          | keyed registration       | keyed transient policy |
|   [8]   | `TryAdd`                     | idempotent descriptor    | default registration   |
|   [9]   | `TryAddEnumerable`           | enumerable descriptor    | ordered extension set  |
|  [10]   | `TryAddKeyed{Lifetime}`      | idempotent keyed entry   | keyed default policy   |
|  [11]   | `Replace`                    | descriptor replacement   | explicit override      |
|  [12]   | `RemoveAll`                  | descriptor removal       | unkeyed contract reset |
|  [13]   | `RemoveAllKeyed`             | descriptor removal       | keyed contract reset   |
|  [14]   | `ServiceDescriptor.Describe` | descriptor factory       | typed descriptor shape |
|  [15]   | `DescribeKeyed`              | keyed descriptor factory | keyed descriptor shape |

[ENTRYPOINT_SCOPE]: resolution and activation operations
- rail: composition

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY]        | [RAIL]                    |
| :-----: | :--------------------------- | :-------------------- | :------------------------ |
|   [1]   | `GetService<T>`              | optional lookup       | nullable contract lookup  |
|   [2]   | `GetRequiredService<T>`      | required lookup       | required contract lookup  |
|   [3]   | `GetServices<T>`             | enumerable lookup     | ordered extension lookup  |
|   [4]   | `GetKeyedService<T>`         | optional keyed lookup | keyed policy lookup       |
|   [5]   | `GetRequiredKeyedService<T>` | required keyed lookup | required keyed policy     |
|   [6]   | `GetKeyedServices<T>`        | keyed enumerable      | keyed extension lookup    |
|   [7]   | `CreateScope`                | scope factory         | synchronous scope         |
|   [8]   | `CreateAsyncScope`           | async scope factory   | asynchronous disposal     |
|   [9]   | `CreateFactory`              | activation compiler   | cached constructor plan   |
|  [10]   | `CreateInstance`             | activation factory    | explicit boundary object  |
|  [11]   | `GetServiceOrCreateInstance` | activation fallback   | optional service fallback |
|  [12]   | `MakeReadOnly`               | collection lock       | registration freeze       |

## [4]-[IMPLEMENTATION_LAW]

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
