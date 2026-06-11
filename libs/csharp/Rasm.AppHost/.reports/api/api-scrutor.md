# [RASM_APPHOST_API_SCRUTOR]

`Scrutor` supplies assembly scanning, type selection, registration strategies, and decoration over the AppHost DI rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Scrutor`
- package: `Scrutor`
- assembly: `Scrutor`
- namespace: `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: composition

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: scan family
- rail: composition

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE]       | [CAPABILITY]                 |
| :-----: | :---------------------------- | :------------------- | :--------------------------- |
|   [1]   | `ITypeSourceSelector`         | contract surface     | defines boundary contract    |
|   [2]   | `IImplementationTypeSelector` | contract surface     | defines boundary contract    |
|   [3]   | `IServiceTypeSelector`        | contract surface     | defines boundary contract    |
|   [4]   | `ILifetimeSelector`           | contract surface     | defines boundary contract    |
|   [5]   | `RegistrationStrategy`        | scan conflict policy | anchors composition contract |
|   [6]   | `DecorationStrategy`          | decoration policy    | anchors composition contract |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: scan operations
- rail: composition

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]           | [CAPABILITY]              |
| :-----: | :---------------------------- | :--------------------- | :------------------------ |
|   [1]   | `Scan`                        | scan root              | starts assembly scan      |
|   [2]   | `FromAssemblies`              | assembly selector      | selects assemblies        |
|   [3]   | `FromApplicationDependencies` | dependency selector    | scans dependency closure  |
|   [4]   | `AddClasses`                  | DI extension           | admits configured surface |
|   [5]   | `AssignableTo`                | type predicate         | filters implementations   |
|   [6]   | `AsImplementedInterfaces`     | service selector       | maps service contracts    |
|   [7]   | `WithScopedLifetime`          | fluent option          | applies policy value      |
|   [8]   | `Decorate`                    | decorator registration | wraps service contract    |

## [4]-[IMPLEMENTATION_LAW]

[SCAN_TOPOLOGY]:
- namespaces: `Scrutor`, `Microsoft.Extensions.DependencyInjection`
- assembly selectors: entry assembly, dependency context, explicit assemblies, marker types
- type filters: assignable type, attribute, namespace, predicate
- service mapping: implemented interfaces, self, explicit type, generic interfaces
- registration policy: append, skip, throw, replace
- decoration policy: service decoration wraps existing registrations in order

[LOCAL_ADMISSION]:
- Scanning is bootstrap-only composition work.
- Scan filters are package-owned and deterministic; runtime reflection loops stay rejected.
- Decoration is admitted for cross-cutting ports where the decorated service contract remains the public contract.

[RAIL_LAW]:
- Package: `Scrutor`
- Owns: assembly scanning and decoration
- Accept: scans populate composition records
- Reject: reflection loops in runtime code

