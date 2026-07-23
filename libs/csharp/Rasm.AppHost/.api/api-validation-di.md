# [RASM_APPHOST_API_VALIDATION_DI]

`ServiceCollectionExtensions` folds an assembly scan into DI registration: every `AbstractValidator<T>` a scanned assembly carries registers under its `IValidator<T>` interface on the `IServiceCollection` at one explicit `ServiceLifetime`. Its boundary is the composition root — the scan runs once at bootstrap, and each validator resolves through the service provider the boundary op reads.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FluentValidation.DependencyInjectionExtensions`
- package: `FluentValidation.DependencyInjectionExtensions` (Apache-2.0)
- assembly: `FluentValidation.DependencyInjectionExtensions`
- namespace: `FluentValidation`
- rail: validation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: registration extension

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                |
| :-----: | :---------------------------- | :------------ | :-------------------------- |
|  [01]   | `ServiceCollectionExtensions` | class         | validator scan-and-register |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: assembly scan registration
- shared tail on every entrypoint: `ServiceLifetime lifetime = Scoped, Func<AssemblyScanResult, bool> filter = null, bool includeInternalTypes = false`

| [INDEX] | [SURFACE]                                            | [SHAPE] | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------- | :------ | :-------------------------------------------- |
|  [01]   | `AddValidatorsFromAssemblies(IEnumerable<Assembly>)` | static  | scan an assembly set, register each validator |
|  [02]   | `AddValidatorsFromAssembly(Assembly)`                | static  | scan one assembly                             |
|  [03]   | `AddValidatorsFromAssemblyContaining(Type)`          | static  | scan the marker type's assembly               |
|  [04]   | `AddValidatorsFromAssemblyContaining<T>()`           | static  | scan the generic marker's assembly            |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each `AssemblyScanResult` registers its validator twice — the `IValidator<T>` interface through `TryAddEnumerable`, the concrete type through `TryAdd` — both at the chosen `ServiceLifetime`.
- `filter` gates each scan result before registration; an unmatched result never registers.
- Default `ServiceLifetime` is `Scoped`; a validator with scoped dependencies never registers as singleton.

[STACKING]:
- `api-validation`(`.api/api-validation.md`): `AssemblyScanner` discovers every `AbstractValidator<T>` subtype, and each `AssemblyScanResult` carries the `InterfaceType`/`ValidatorType` pair this surface registers.
- `api-di`(`.api/api-di.md`): each registration mints a `ServiceDescriptor` onto the `IServiceCollection` under `TryAddEnumerable`/`TryAdd` idempotency, resolved through `GetRequiredService<IValidator<T>>`.
- within-lib: AppHost scans its boundary assembly once at composition, resolves `IValidator<T>` per policy or request record, and folds the accumulated `ValidationResult` onto the typed rail.

[LOCAL_ADMISSION]:
- AppHost scans its boundary assembly at the composition root under a `filter` admitting only boundary validators; runtime code never registers validators.

[RAIL_LAW]:
- Package: `FluentValidation.DependencyInjectionExtensions`
- Owns: validator discovery and DI registration
- Accept: scanned `IValidator<T>` implementations enter the composition root at an explicit lifetime
- Reject: per-validator manual registration and hand-rolled validator registries
