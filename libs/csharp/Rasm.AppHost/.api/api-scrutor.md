# [RASM_APPHOST_API_SCRUTOR]

`Scrutor` supplies assembly scanning, type filtering, service mapping, registration
strategy, keyed lifetime selection, and decoration over the AppHost DI rail.

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

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]        | [RAIL]                   |
| :-----: | :---------------------------- | :------------------- | :----------------------- |
|   [1]   | `ITypeSourceSelector`         | scan root            | source selection         |
|   [2]   | `IAssemblySelector`           | assembly selector    | assembly selection       |
|   [3]   | `ITypeSelector`               | type selector        | explicit type selection  |
|   [4]   | `IImplementationTypeSelector` | implementation stage | class admission          |
|   [5]   | `IImplementationTypeFilter`   | type filter          | implementation filtering |
|   [6]   | `IServiceTypeSelector`        | service mapping      | service contract mapping |
|   [7]   | `ILifetimeSelector`           | lifetime mapping     | service lifetime         |
|   [8]   | `RegistrationStrategy`        | conflict policy      | duplicate registration   |
|   [9]   | `ReplacementBehavior`         | replacement policy   | replacement detail       |
|  [10]   | `DecorationStrategy`          | decoration policy    | decorator selection      |
|  [11]   | `DecoratedService<T>`         | decoration handle    | decorated service access |
|  [12]   | `ServiceDescriptorAttribute`  | service attribute    | attribute-based mapping  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: scan operations
- rail: composition

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY]    | [RAIL]                     |
| :-----: | :---------------------------- | :---------------- | :------------------------- |
|   [1]   | `Scan`                        | scan root         | starts scan pipeline       |
|   [2]   | `FromEntryAssembly`           | assembly selector | entry assembly source      |
|   [3]   | `FromApplicationDependencies` | assembly selector | dependency closure source  |
|   [4]   | `FromAssemblyDependencies`    | assembly selector | assembly dependency source |
|   [5]   | `FromDependencyContext`       | assembly selector | dependency context source  |
|   [6]   | `FromAssemblyOf<T>`           | assembly selector | marker-type source         |
|   [7]   | `FromAssembliesOf`            | assembly selector | marker-type set source     |
|   [8]   | `FromAssemblies`              | assembly selector | explicit assembly source   |
|   [9]   | `FromTypes`                   | type selector     | explicit type source       |
|  [10]   | `AddClasses`                  | class selector    | concrete class admission   |
|  [11]   | `AssignableTo`                | type filter       | assignability filter       |
|  [12]   | `AssignableToAny`             | type filter       | multi-type filter          |
|  [13]   | `WithAttribute`               | attribute filter  | attribute inclusion        |
|  [14]   | `WithoutAttribute`            | attribute filter  | attribute exclusion        |
|  [15]   | `InNamespaces`                | namespace filter  | namespace inclusion        |
|  [16]   | `NotInNamespaces`             | namespace filter  | namespace exclusion        |
|  [17]   | `Where`                       | predicate filter  | custom type filter         |

[ENTRYPOINT_SCOPE]: mapping lifetime and decoration operations
- rail: composition

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY]      | [RAIL]                   |
| :-----: | :----------------------------- | :------------------ | :----------------------- |
|   [1]   | `AsSelf`                       | service mapping     | self registration        |
|   [2]   | `As<T>`                        | service mapping     | explicit service type    |
|   [3]   | `AsImplementedInterfaces`      | service mapping     | interface registration   |
|   [4]   | `AsSelfWithInterfaces`         | service mapping     | self plus interfaces     |
|   [5]   | `AsMatchingInterface`          | service mapping     | convention interface     |
|   [6]   | `UsingAttributes`              | service mapping     | attribute mapping        |
|   [7]   | `WithSingletonLifetime`        | lifetime mapping    | singleton registration   |
|   [8]   | `WithScopedLifetime`           | lifetime mapping    | scoped registration      |
|   [9]   | `WithTransientLifetime`        | lifetime mapping    | transient registration   |
|  [10]   | `WithLifetime`                 | lifetime mapping    | explicit lifetime        |
|  [11]   | `WithServiceKey`               | keyed mapping       | keyed registration       |
|  [12]   | `RegistrationStrategy.Replace` | conflict policy     | replacement registration |
|  [13]   | `Decorate`                     | decorator admission | required decoration      |
|  [14]   | `TryDecorate`                  | decorator admission | optional decoration      |
|  [15]   | `IsDecorated`                  | decorator predicate | decoration detection     |
|  [16]   | `GetDecoratedServices<T>`      | decorator lookup    | decorated enumerable     |

## [4]-[IMPLEMENTATION_LAW]

[SCAN_TOPOLOGY]:
- namespaces: `Scrutor`, `Microsoft.Extensions.DependencyInjection`
- assembly selectors: entry assembly, dependency context, explicit assemblies, marker types
- type filters: assignable type, multi-type assignability, attribute, namespace, exact namespace, predicate
- service mapping: implemented interfaces, self, explicit type, matching interface, attribute mapping
- lifetime mapping: singleton, scoped, transient, explicit lifetime, keyed service
- registration policy: append, skip, throw, replace
- decoration policy: required decoration, optional decoration, decorated-service handles

[LOCAL_ADMISSION]:
- Scanning is bootstrap-only composition work.
- Scan filters are package-owned and deterministic; runtime reflection loops stay rejected.
- Decoration is admitted for cross-cutting ports where the decorated service contract remains the public contract.

[RAIL_LAW]:
- Package: `Scrutor`
- Owns: assembly scanning and decoration
- Accept: scans populate composition records
- Reject: reflection loops in runtime code
