# [RASM_APPHOST_API_VALIDATION_DI]

`FluentValidation.DependencyInjectionExtensions` supplies validator discovery, assembly scanning, DI registration, scan filters, and explicit lifetimes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FluentValidation.DependencyInjectionExtensions`

- package: `FluentValidation.DependencyInjectionExtensions`
- assembly: `FluentValidation.DependencyInjectionExtensions`
- namespace: `FluentValidation`
- asset: runtime library
- rail: validation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: registration family

- rail: validation

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]          | [RAIL]                 |
| :-----: | :----------------------------------- | :--------------------- | :--------------------- |
|  [01]   | `ServiceCollectionExtensions`        | registration extension | validator registration |
|  [02]   | `AssemblyScanner`                    | validator scanner      | validator discovery    |
|  [03]   | `AssemblyScanner.AssemblyScanResult` | scan result            | discovered validator   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations

- rail: validation

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY]    | [RAIL]                     |
| :-----: | :------------------------------------------ | :---------------- | :------------------------- |
|  [01]   | `AddValidatorsFromAssemblies`               | assembly set scan | multi-assembly admission   |
|  [02]   | `AddValidatorsFromAssembly`                 | assembly scan     | assembly admission         |
|  [03]   | `AddValidatorsFromAssemblyContaining(Type)` | marker scan       | marker-type admission      |
|  [04]   | `AddValidatorsFromAssemblyContaining<T>`    | marker scan       | generic marker admission   |
|  [05]   | `ServiceLifetime` parameter                 | lifetime policy   | scoped/singleton/transient |
|  [06]   | `filter` parameter                          | scan filter       | result inclusion gate      |
|  [07]   | `includeInternalTypes` parameter            | visibility policy | internal validator scan    |

## [04]-[IMPLEMENTATION_LAW]

[VALIDATION_REGISTRATION_TOPOLOGY]:

- namespaces: `FluentValidation`
- scan source: assembly, assemblies, marker type
- scan result: validator interface type plus validator implementation type
- registration knobs: lifetime, include internal types, filter predicate
- service shape: validators register as `IValidator<T>`

[LOCAL_ADMISSION]:

- Validator scanning happens at composition roots.
- Lifetimes are explicit policy; validators with scoped dependencies do not register as singleton.
- Scan filters are deterministic and package-owned.
- Validator resolution goes through the service provider directly; validator factories stay rejected.

[RAIL_LAW]:

- Package: `FluentValidation.DependencyInjectionExtensions`
- Owns: validator registration
- Accept: validators enter composition roots
- Reject: manual validator registries
