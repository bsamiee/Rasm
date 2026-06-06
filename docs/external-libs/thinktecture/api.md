# [THINKTECTURE_API]

[IMPORTANT] Rasm pins `Thinktecture.Runtime.Extensions` at the version pinned in `Directory.Packages.props`. `Directory.Build.props` supplies the package reference and global `Thinktecture` using.

## [1][SOURCE_TRUTH]

| [INDEX] | [SOURCE]                                                                         | [USE]                                                    |
| :-----: | -------------------------------------------------------------------------------- | -------------------------------------------------------- |
|   [1]   | `Directory.Packages.props`                                                       | Confirms the pinned `Thinktecture.Runtime.Extensions` version. |
|   [2]   | `uv run python -m tools.quality api resolve Thinktecture.Runtime.Extensions all` | Public generated support API assets.                     |
|   [3]   | Package nuspec                                                                   | Analyzer, refactoring, source-generator package closure. |
|   [4]   | Official wiki and Context7                                                       | Secondary source for generator behavior.                 |

## [2][SURFACES]

| [INDEX] | [SURFACE]                          | [OWNS]                                 | [RASM_USE]                                                |
| :-----: | ---------------------------------- | -------------------------------------- | --------------------------------------------------------- |
|   [1]   | `[ValueObject<T>]`                 | Primitive value admission.             | Tolerances, identifiers, names, scalar boundaries.        |
|   [2]   | `[ComplexValueObject]`             | Composite equality identity.           | Ranges, sample windows, boundary descriptors.             |
|   [3]   | `[SmartEnum]`, `[SmartEnum<TKey>]` | Closed keyed registries with behavior. | Modes, metrics, port kinds, operation policies.           |
|   [4]   | `[Union]`, `[Union<T1,...>]`       | Closed variants with payload.          | Operation intent, result shape, UI/native state.          |
|   [5]   | `[AdHocUnion]`                     | Compact local alternatives.            | Narrow value-level choices only.                          |
|   [6]   | `[ObjectFactory<T>]`               | Explicit external value conversion.    | Serialization/model-binding only with real boundary need. |

Attribute property detail: `objects.md` (value objects), `union-attributes.md` (union dispatch and operator policy), `enums.md` (`UseDelegateFromConstructor`).

## [3][PUBLIC_CONTRACTS]

| [INDEX] | [CONTRACT]                                  | [USE]                                                   |
| :-----: | ------------------------------------------- | ------------------------------------------------------- |
|   [1]   | `IObjectFactory<T>`                         | Generated object-factory marker.                        |
|   [2]   | `IObjectFactory<T,TValue,TValidationError>` | Static factory/validation contract.                     |
|   [3]   | `ISmartEnum<TKey>`                          | Keyed smart-enum contract.                              |
|   [4]   | `ISmartEnum<T,TKey,TValidationError>`       | Generic smart-enum contract with validation error type. |
|   [5]   | `IValidationError<T>`, `ValidationError`    | Generator validation error shape.                       |

## [4][RULES]

- Prefer generated factories, `Items`, `Get`, `TryGet`, `Switch`, and `Map` directly.
- Bridge generator validation once into LanguageExt `Error`, `Fin`, or `Validation`.
- Keep optional integration packages out of active docs until Rasm pins and consumes them.
- Keep partial dispatch opt-in and explicit; default to exhaustive generated dispatch.
