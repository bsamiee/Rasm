# [THINKTECTURE_API]

[IMPORTANT] Thinktecture source generation belongs to the project package graph. Active package and global-using facts live in package-state docs.

## [1][SURFACES]

| [INDEX] | [SURFACE]                          | [OWNS]                                 | [DOMAIN_USE]                                              |
| :-----: | ---------------------------------- | -------------------------------------- | --------------------------------------------------------- |
|   [1]   | `[ValueObject<T>]`                 | Primitive value admission.             | Tolerances, identifiers, names, scalar boundaries.        |
|   [2]   | `[ComplexValueObject]`             | Composite equality identity.           | Ranges, sample windows, boundary descriptors.             |
|   [3]   | `[SmartEnum]`, `[SmartEnum<TKey>]` | Closed keyed registries with behavior. | Modes, metrics, port kinds, operation policies.           |
|   [4]   | `[Union]`, `[Union<T1,...>]`       | Closed variants with payload.          | Operation intent, result shape, UI/native state.          |
|   [5]   | `[AdHocUnion]`                     | Compact local alternatives.            | Narrow value-level choices only.                          |
|   [6]   | `[ObjectFactory<T>]`               | Explicit external value conversion.    | Serialization/model-binding only with real boundary need. |

Attribute property detail: `objects.md` (value objects), `union-attributes.md` (union dispatch and operator policy), `enums.md` (`UseDelegateFromConstructor`).

## [2][PUBLIC_CONTRACTS]

| [INDEX] | [CONTRACT]                                  | [USE]                                                   |
| :-----: | ------------------------------------------- | ------------------------------------------------------- |
|   [1]   | `IObjectFactory<T>`                         | Generated object-factory marker.                        |
|   [2]   | `IObjectFactory<T,TValue,TValidationError>` | Static factory/validation contract.                     |
|   [3]   | `ISmartEnum<TKey>`                          | Keyed smart-enum contract.                              |
|   [4]   | `ISmartEnum<T,TKey,TValidationError>`       | Generic smart-enum contract with validation error type. |
|   [5]   | `IValidationError<T>`, `ValidationError`    | Generator validation error shape.                       |

## [3][RULES]

- Prefer generated factories, `Items`, `Get`, `TryGet`, `Switch`, and `Map` directly.
- Bridge generator validation once into the domain error rail.
- Keep optional integration packages out of active docs until package graph state and a consumer make them active.
- Keep partial dispatch opt-in and explicit; default to exhaustive generated dispatch.
