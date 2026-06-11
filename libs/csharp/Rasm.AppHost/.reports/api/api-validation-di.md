# [RASM_APPHOST_API_VALIDATION_DI]

`FluentValidation.DependencyInjectionExtensions` supplies validator discovery and DI registration.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FluentValidation.DependencyInjectionExtensions`
- package: `FluentValidation.DependencyInjectionExtensions`
- assembly: `FluentValidation.DependencyInjectionExtensions`
- namespace: `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: validation

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: registration family
- rail: validation

| [INDEX] | [SYMBOL]                             | [PACKAGE_ROLE]         | [CAPABILITY]                |
| :-----: | :----------------------------------- | :--------------------- | :-------------------------- |
|   [1]   | `ServiceCollectionExtensions`        | registration extension | anchors validation contract |
|   [2]   | `AssemblyScanner`                    | validator scanner      | anchors validation contract |
|   [3]   | `AssemblyScanner.AssemblyScanResult` | result value           | projects receipt state      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations
- rail: validation

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]         | [CAPABILITY]              |
| :-----: | :------------------------------------ | :------------------- | :------------------------ |
|   [1]   | `AddValidatorsFromAssembly`           | DI extension         | admits configured surface |
|   [2]   | `AddValidatorsFromAssemblies`         | DI extension         | admits configured surface |
|   [3]   | `AddValidatorsFromAssemblyContaining` | DI extension         | admits configured surface |
|   [4]   | `RegisterValidatorsFromAssembly`      | scanner registration | admits validators         |

## [4]-[IMPLEMENTATION_LAW]

[VALIDATION_REGISTRATION_TOPOLOGY]:
- namespaces: `FluentValidation`, `Microsoft.Extensions.DependencyInjection`
- scan source: assembly, assemblies, marker type
- scan result: validator interface type plus validator implementation type
- registration knobs: lifetime, include internal types, filter predicate
- service shape: validators register as `IValidator<T>`

[LOCAL_ADMISSION]:
- Validator scanning happens at composition roots.
- Lifetimes are explicit policy; validators with scoped dependencies do not register as singleton.
- Scan filters are deterministic and package-owned.

[RAIL_LAW]:
- Package: `FluentValidation.DependencyInjectionExtensions`
- Owns: validator registration
- Accept: validators enter composition roots
- Reject: manual validator registries

