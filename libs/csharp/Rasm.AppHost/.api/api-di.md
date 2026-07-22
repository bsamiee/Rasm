# [RASM_APPHOST_API_DI]

`Microsoft.Extensions.DependencyInjection` owns the AppHost composition rail: every registration mints one `ServiceDescriptor` onto an `IServiceCollection`, `BuildServiceProvider` freezes that graph into a validated `ServiceProvider`, and resolution runs through lifetime-scoped and keyed lookup. Its boundary is composition — descriptors land only at composition roots, and `ActivatorUtilities` constructs boundary objects the container never registered.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.DependencyInjection`
- package: `Microsoft.Extensions.DependencyInjection`
- assembly: `Microsoft.Extensions.DependencyInjection`
- contract_assembly: `Microsoft.Extensions.DependencyInjection.Abstractions`
- namespace: `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.DependencyInjection.Extensions`
- rail: composition

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider implementation

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY] | [CAPABILITY]               |
| :-----: | :-------------------------------------------- | :------------ | :------------------------- |
|  [01]   | `DefaultServiceProviderFactory`               | class         | host provider bridge       |
|  [02]   | `ServiceCollectionContainerBuilderExtensions` | class         | root provider build        |
|  [03]   | `ServiceProvider`                             | class         | service resolution         |
|  [04]   | `ServiceProviderOptions`                      | class         | provider validation policy |

[PUBLIC_TYPE_SCOPE]: composition contracts

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY] | [CAPABILITY]              |
| :-----: | :--------------------------------------- | :------------ | :------------------------ |
|  [01]   | `IServiceCollection`                     | interface     | registration boundary     |
|  [02]   | `ServiceCollection`                      | class         | registration staging      |
|  [03]   | `ServiceDescriptor`                      | class         | registration identity     |
|  [04]   | `ServiceLifetime`                        | enum          | scope policy              |
|  [05]   | `IServiceScope`                          | interface     | scoped lifetime           |
|  [06]   | `AsyncServiceScope`                      | struct        | async scoped lifetime     |
|  [07]   | `IServiceScopeFactory`                   | interface     | scope construction        |
|  [08]   | `IServiceProviderFactory<TBuilder>`      | interface     | host builder interop      |
|  [09]   | `IServiceProviderIsService`              | interface     | optional resolution probe |
|  [10]   | `IServiceProviderIsKeyedService`         | interface     | keyed resolution probe    |
|  [11]   | `IKeyedServiceProvider`                  | interface     | keyed policy resolution   |
|  [12]   | `KeyedService`                           | class         | keyed enumerable sentinel |
|  [13]   | `FromKeyedServicesAttribute`             | class         | keyed constructor input   |
|  [14]   | `ServiceKeyAttribute`                    | class         | key injection             |
|  [15]   | `ServiceKeyLookupMode`                   | enum          | key inheritance           |
|  [16]   | `ActivatorUtilities`                     | class         | explicit construction     |
|  [17]   | `ActivatorUtilitiesConstructorAttribute` | class         | activation selection      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations

| [INDEX] | [SURFACE]                     | [SHAPE] | [CAPABILITY]                 |
| :-----: | :---------------------------- | :------ | :--------------------------- |
|  [01]   | `BuildServiceProvider`        | static  | root provider creation       |
|  [02]   | `AddSingleton`                | static  | singleton registration       |
|  [03]   | `AddScoped`                   | static  | scoped registration          |
|  [04]   | `AddTransient`                | static  | transient registration       |
|  [05]   | `AddKeyedSingleton`           | static  | keyed singleton policy       |
|  [06]   | `AddKeyedScoped`              | static  | keyed scoped policy          |
|  [07]   | `AddKeyedTransient`           | static  | keyed transient policy       |
|  [08]   | `TryAdd`                      | static  | idempotent default           |
|  [09]   | `TryAddEnumerable`            | static  | ordered extension-set add    |
|  [10]   | `TryAddKeyed{Lifetime}`       | static  | idempotent keyed default     |
|  [11]   | `Replace`                     | static  | explicit descriptor override |
|  [12]   | `RemoveAll`                   | static  | unkeyed contract reset       |
|  [13]   | `RemoveAllKeyed`              | static  | keyed contract reset         |
|  [14]   | `ServiceDescriptor.Describe`  | factory | typed descriptor factory     |
|  [15]   | `DescribeKeyed`               | factory | keyed descriptor factory     |
|  [16]   | `ServiceDescriptor.Singleton` | factory | lifetime descriptor factory  |

- `ServiceDescriptor.Singleton`: `(Type, object)`, generic-instance, and factory overloads, paralleled on `Scoped` and `Transient`.

[ENTRYPOINT_SCOPE]: resolution and activation operations

| [INDEX] | [SURFACE]                    | [SHAPE]  | [CAPABILITY]              |
| :-----: | :--------------------------- | :------- | :------------------------ |
|  [01]   | `GetService<T>`              | static   | nullable contract lookup  |
|  [02]   | `GetRequiredService<T>`      | static   | required contract lookup  |
|  [03]   | `GetServices<T>`             | static   | ordered extension lookup  |
|  [04]   | `GetKeyedService<T>`         | static   | keyed policy lookup       |
|  [05]   | `GetRequiredKeyedService<T>` | static   | required keyed policy     |
|  [06]   | `GetKeyedServices<T>`        | static   | keyed extension lookup    |
|  [07]   | `CreateScope`                | static   | synchronous scope         |
|  [08]   | `CreateAsyncScope`           | static   | asynchronous disposal     |
|  [09]   | `CreateFactory`              | static   | cached constructor plan   |
|  [10]   | `CreateInstance`             | static   | explicit boundary object  |
|  [11]   | `GetServiceOrCreateInstance` | static   | optional service fallback |
|  [12]   | `MakeReadOnly`               | instance | registration freeze       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each registration mints one `ServiceDescriptor` selecting a `ServiceLifetime` — singleton, scoped, or transient — on an axis orthogonal to the `object` service key; the root provider owns singleton state and every created scope owns its scoped disposal.
- A descriptor carries one construction form — implementation type, factory, or singleton instance — the keyed axis mirroring each form behind the service key.
- `ValidateScopes` guards scoped-from-root capture and `ValidateOnBuild` forces eager provider construction, both `ServiceProviderOptions` policy proven at `BuildServiceProvider`.
- `ActivatorUtilities` constructs a boundary object from unregistered constructor dependencies, never a hidden service-locator lookup.
- A keyed factory receives `IServiceProvider` and the `object` key; `FromKeyedServicesAttribute` selects a keyed constructor dependency, `ServiceKeyAttribute` injects the ambient key, and `ServiceKeyLookupMode` decides key inheritance.
- `KeyedService.AnyKey` selects the keyed enumerable and never resolves a single service.

[STACKING]:
- `api-hosting`(`.api/api-hosting.md`): `HostApplicationBuilder.Services` is the `IServiceCollection` this surface populates, `UseServiceProviderFactory`/`ConfigureContainer` bind an `IServiceProviderFactory<TBuilder>`, and `Build` runs `BuildServiceProvider` under the `ServiceProviderOptions` from `UseDefaultServiceProvider`.
- `api-scrutor`(`.api/api-scrutor.md`): `Scan` emits `ServiceDescriptor` rows onto the `IServiceCollection` under a `RegistrationStrategy`, and `Decorate` rewrites a descriptor to wrap the resolved service — assembly scanning resolves to descriptor registration on this rail.
- `api-options`(`.api/api-options.md`): `AddOptions`/`Configure`/`PostConfigure` register `IConfigureOptions`/`IPostConfigureOptions`/`IValidateOptions` through `TryAddEnumerable` idempotency, and `IOptions`/`IOptionsSnapshot`/`IOptionsMonitor` resolve as singleton, scoped, and singleton services.
- `api-validation-di`(`.api/api-validation-di.md`): `AddValidatorsFromAssemblies` registers each discovered `IValidator<T>` as a `ServiceDescriptor` at an explicit `ServiceLifetime`, resolved through `GetRequiredService`.
- within-lib: AppHost's one composition root folds every port record onto the `IServiceCollection`, models bounded policy variants as keyed registrations, then `MakeReadOnly` freezes the collection before `BuildServiceProvider` proves the graph under `ValidateOnBuild` and `ValidateScopes`.

[LOCAL_ADMISSION]:
- AppHost ports register as constructor-visible dependencies at composition roots.
- Keyed registrations model bounded policy variants whose key is AppHost policy.
- Descriptor mutation stays in composition-assembly setup; runtime code never mutates the collection.
- `ValidateOnBuild` and `ValidateScopes` are enabled for package proof, rejected as runtime probes.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.DependencyInjection`
- Owns: the service graph, lifetime scopes, and keyed resolution
- Accept: registrations mint descriptors at composition roots
- Reject: runtime service-locator lookups and hand-rolled provider caches
