# [RASM_APPHOST_API_VALIDATION_DI]

`FluentValidation.DependencyInjectionExtensions` supplies validator discovery, assembly
scanning, DI registration, scan filters, explicit lifetimes, and service-provider
validator factories.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FluentValidation.DependencyInjectionExtensions`
- package: `FluentValidation.DependencyInjectionExtensions`
- assembly: `FluentValidation.DependencyInjectionExtensions`
- namespace: `FluentValidation`
- asset: runtime library
- rail: validation

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: registration family
- rail: validation

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]          | [RAIL]                  |
| :-----: | :----------------------------------- | :--------------------- | :---------------------- |
|   [1]   | `ServiceCollectionExtensions`        | registration extension | validator registration  |
|   [2]   | `AssemblyScanner`                    | validator scanner      | validator discovery     |
|   [3]   | `AssemblyScanner.AssemblyScanResult` | scan result            | discovered validator    |
|   [4]   | `ServiceProviderValidatorFactory`    | validator factory      | service-provider lookup |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations
- rail: validation

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY]     | [RAIL]                     |
| :-----: | :------------------------------------------ | :----------------- | :------------------------- |
|   [1]   | `AddValidatorsFromAssemblies`               | assembly set scan  | multi-assembly admission   |
|   [2]   | `AddValidatorsFromAssembly`                 | assembly scan      | assembly admission         |
|   [3]   | `AddValidatorsFromAssemblyContaining(Type)` | marker scan        | marker-type admission      |
|   [4]   | `AddValidatorsFromAssemblyContaining<T>`    | marker scan        | generic marker admission   |
|   [5]   | `AddScanResult`                             | scan-result insert | explicit result admission  |
|   [6]   | `ServiceLifetime` parameter                 | lifetime policy    | scoped/singleton/transient |
|   [7]   | `filter` parameter                          | scan filter        | result inclusion gate      |
|   [8]   | `includeInternalTypes` parameter            | visibility policy  | internal validator scan    |

## [4]-[IMPLEMENTATION_LAW]

[VALIDATION_REGISTRATION_TOPOLOGY]:
- namespaces: `FluentValidation`
- scan source: assembly, assemblies, marker type
- scan result: validator interface type plus validator implementation type
- registration knobs: lifetime, include internal types, filter predicate
- service shape: validators register as `IValidator<T>`
- factory shape: `ServiceProviderValidatorFactory` resolves validators through `IServiceProvider`

[LOCAL_ADMISSION]:
- Validator scanning happens at composition roots.
- Lifetimes are explicit policy; validators with scoped dependencies do not register as singleton.
- Scan filters are deterministic and package-owned.
- Service-provider validator resolution stays inside validation composition boundaries.

[RAIL_LAW]:
- Package: `FluentValidation.DependencyInjectionExtensions`
- Owns: validator registration
- Accept: validators enter composition roots
- Reject: manual validator registries
