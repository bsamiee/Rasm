# [RASM_APPHOST_API_SCRUTOR]

`Scrutor` folds assembly scanning into convention-driven `ServiceDescriptor` registration on the AppHost DI rail, and decorates a resolved service behind its own public contract.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Scrutor`
- package: `Scrutor`
- assembly: `Scrutor`
- namespace: `Scrutor`, `Microsoft.Extensions.DependencyInjection`
- rail: composition

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: scan and decoration family

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :---------------------------- | :------------ | :---------------------------- |
|  [01]   | `ITypeSourceSelector`         | interface     | fluent scan pipeline root     |
|  [02]   | `IAssemblySelector`           | interface     | assembly source selection     |
|  [03]   | `ITypeSelector`               | interface     | explicit type source          |
|  [04]   | `IImplementationTypeSelector` | interface     | concrete class admission      |
|  [05]   | `IImplementationTypeFilter`   | interface     | implementation-type filtering |
|  [06]   | `IServiceTypeSelector`        | interface     | service-contract mapping      |
|  [07]   | `ILifetimeSelector`           | interface     | service lifetime mapping      |
|  [08]   | `RegistrationStrategy`        | class         | duplicate-registration policy |
|  [09]   | `ReplacementBehavior`         | enum          | replacement match behavior    |
|  [10]   | `DecorationStrategy`          | class         | decorator selection policy    |
|  [11]   | `DecoratedService<T>`         | class         | decorated-service handle      |
|  [12]   | `ServiceDescriptorAttribute`  | class         | attribute-based mapping       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: scan, source, and filter operations

| [INDEX] | [SURFACE]                     | [SHAPE]  | [CAPABILITY]                |
| :-----: | :---------------------------- | :------- | :-------------------------- |
|  [01]   | `Scan`                        | static   | start scan pipeline         |
|  [02]   | `FromEntryAssembly`           | instance | entry assembly source       |
|  [03]   | `FromApplicationDependencies` | instance | dependency closure source   |
|  [04]   | `FromAssemblyDependencies`    | instance | assembly dependency source  |
|  [05]   | `FromDependencyContext`       | instance | dependency context source   |
|  [06]   | `FromAssemblyOf<T>`           | instance | marker-type assembly source |
|  [07]   | `FromAssembliesOf`            | instance | marker-type set source      |
|  [08]   | `FromAssemblies`              | instance | explicit assembly source    |
|  [09]   | `FromTypes`                   | instance | explicit type source        |
|  [10]   | `AddClasses`                  | instance | concrete class admission    |
|  [11]   | `AssignableTo`                | instance | assignability filter        |
|  [12]   | `AssignableToAny`             | instance | multi-type assignability    |
|  [13]   | `WithAttribute`               | instance | attribute inclusion filter  |
|  [14]   | `WithoutAttribute`            | instance | attribute exclusion filter  |
|  [15]   | `InNamespaces`                | instance | namespace inclusion filter  |
|  [16]   | `NotInNamespaces`             | instance | namespace exclusion filter  |
|  [17]   | `Where`                       | instance | custom predicate filter     |

- `InNamespaces` and `NotInNamespaces` carry `Of`, `Of<T>`, and `Exact` overloads keyed on a marker type.

[ENTRYPOINT_SCOPE]: mapping, lifetime, and decoration operations

| [INDEX] | [SURFACE]                        | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :------------------------------- | :------- | :---------------------------- |
|  [01]   | `AsSelf`                         | instance | self registration             |
|  [02]   | `As<T>`                          | instance | explicit service type         |
|  [03]   | `AsImplementedInterfaces`        | instance | interface registration        |
|  [04]   | `AsSelfWithInterfaces`           | instance | self plus interfaces          |
|  [05]   | `AsMatchingInterface`            | instance | convention interface          |
|  [06]   | `UsingAttributes`                | instance | attribute-driven mapping      |
|  [07]   | `UsingRegistrationStrategy`      | instance | mid-chain conflict policy     |
|  [08]   | `WithSingletonLifetime`          | instance | singleton lifetime            |
|  [09]   | `WithScopedLifetime`             | instance | scoped lifetime               |
|  [10]   | `WithTransientLifetime`          | instance | transient lifetime            |
|  [11]   | `WithLifetime`                   | instance | explicit lifetime             |
|  [12]   | `WithServiceKey`                 | instance | keyed registration            |
|  [13]   | `RegistrationStrategy.Replace`   | factory  | replacement conflict policy   |
|  [14]   | `Decorate`                       | static   | required decoration           |
|  [15]   | `TryDecorate`                    | static   | optional decoration           |
|  [16]   | `IsDecorated`                    | static   | decoration predicate          |
|  [17]   | `GetDecoratedServices<T>`        | static   | decorated-service enumeration |
|  [18]   | `GetRequiredDecoratedService<T>` | static   | required decorated lookup     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Scan` folds one fluent chain — source selection, assembly selection, class admission, type filtering, service mapping, lifetime — into `ServiceDescriptor` rows, each intermediate stage a selector interface returning the next stage.
- `RegistrationStrategy` resolves each descriptor against the existing collection as `Append`, `Skip`, `Throw`, or `Replace`; `ReplacementBehavior` scopes what a replace matches.
- Decoration is an axis orthogonal to scanning: `Decorate` rewrites an already-registered descriptor to wrap the resolved service behind the same contract, the decorated instance resolved through a generated service key `GetDecoratedServices` reads back.

[STACKING]:
- `api-di`(`.api/api-di.md`): `Scan` emits `ServiceDescriptor` rows onto the `IServiceCollection` under a `RegistrationStrategy`, and `Decorate`/`TryDecorate` rewrite an existing descriptor — every scan result is a descriptor this rail resolves.
- within-lib: AppHost's composition root runs one `Scan` chain per assembly, narrowing `FromApplicationDependencies` through `AddClasses`, a filter, `AsImplementedInterfaces`, and `WithScopedLifetime` before `RegistrationStrategy` resolves conflicts against the hand-registered ports.

[LOCAL_ADMISSION]:
- Scanning runs only at composition bootstrap.
- Scan filters are deterministic and package-owned.
- Decoration admits a cross-cutting port only where the decorated contract stays the public contract.

[RAIL_LAW]:
- Package: `Scrutor`
- Owns: assembly scanning, convention registration, and decoration
- Accept: scan chains mint `ServiceDescriptor` rows onto the composition collection
- Reject: runtime reflection loops and hand-rolled registration conventions
