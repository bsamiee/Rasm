# [H1][THINKTECTURE_CORE_API_MAP]
>**Dictum:** *Thinktecture turns closed domain shape into generated, analyzable code.*

<br>

[IMPORTANT] API registry for `Thinktecture.Runtime.Extensions` `10.2.0`. Use generated contracts directly; avoid wrapper factories, lookup tables, and dispatch helpers around them.

---
## [1][VALUE_OBJECTS]
>**Dictum:** *A value object owns primitive admission once.*

<br>

| [INDEX] | [SURFACE] | [CAPABILITY] | [USE] |
| :-----: | --------- | ------------ | --- |
| [1] | `[ValueObject<TKey>]` | Single-key wrapper with generated constructor, key member, factory, validation, equality, comparison, parsing, formatting, conversion, and operators. | Primitive boundaries such as tolerances, operation keys, identifiers, and bounded scalar units. |
| [2] | `[ComplexValueObject]` | Multi-member value object with generated factory, validation, equality, hash code, and string handling. | Composite immutable values where all members define identity. |
| [3] | `ValidateFactoryArguments` | Partial validation and normalization hook that receives `ref ValidationError?` and `ref` arguments. | Validate external inputs without throwing. |
| [4] | `ValidationError` | Generator-native validation result. | Bridge generated factories into `Fin<T>` or `Validation<Error,T>`. |
| [5] | `IObjectFactory<TSelf,TValue,TValidationError>` | Static validation interface for generated factories and object factories. | Generic construction rails such as tolerance validation. |

---
## [2][ENUMS]
>**Dictum:** *A smart enum is a closed registry with behavior attached to items.*

<br>

| [INDEX] | [SURFACE] | [CAPABILITY] | [USE] |
| :-----: | --------- | ------------ | --- |
| [1] | `[SmartEnum]` | Keyless closed item set with generated equality and dispatch. | Closed vocabularies that do not need wire keys. |
| [2] | `[SmartEnum<TKey>]` | Keyed closed item set with generated `Items`, `Get`, `TryGet`, validation, parsing, conversion, comparison, formatting, and dispatch. | Type registries, GH2 port kinds, operation modes, and stat families. |
| [3] | `ISmartEnum<TSelf>` and `ISmartEnum<TSelf,TKey,TValidationError>` | Generated smart-enum contracts. | Generic constraints only when an algorithm truly needs enum-polymorphic lookup. |
| [4] | `[UseDelegateFromConstructor]` | Promotes constructor delegate arguments into generated members. | Item-specific behavior without switch helpers. |

---
## [3][UNIONS]
>**Dictum:** *A union owns all valid variants and forces exhaustive dispatch.*

<br>

| [INDEX] | [SURFACE] | [CAPABILITY] | [USE] |
| :-----: | --------- | ------------ | --- |
| [1] | `[Union]` | Regular discriminated union over nested derived cases. | Domain outcomes, aspects, faults, and operation selectors. |
| [2] | `[Union<T1,T2>]` through `[Union<T1,T2,T3,T4,T5>]` | Generic ad-hoc union declaration. | Compact alternatives where nested case records would add noise. |
| [3] | `[AdHocUnion]` | Non-generic ad-hoc union declaration with fixed `T1` through `T5` `Type` constructor/property cases. | Cases requiring runtime `typeof(...)` arguments or generic type refs. |
| [4] | Generated `Switch` and `Map` | Exhaustive dispatch with named case handlers and optional state overloads. | Replace local visitor, pattern-helper, or strategy-wrapper code. |
| [5] | `SwitchPartially` and `MapPartially` | Partial dispatch where configured. | Nested or staged handling while keeping generated case names. |

---
## [4][COMPARISON_AND_CONVERSION]
>**Dictum:** *Comparison policy belongs on the generated type declaration.*

<br>

| [INDEX] | [SURFACE] | [CAPABILITY] | [USE] |
| :-----: | --------- | ------------ | --- |
| [1] | `KeyMemberEqualityComparerAttribute<TAccessor,TKey>` | Equality comparer for value-object or smart-enum keys. | String keys that must be ordinal, case-sensitive, or culture-aware. |
| [2] | `KeyMemberComparerAttribute<TAccessor,TKey>` | Ordering comparer for key members. | Sorted key behavior without custom compare methods. |
| [3] | `MemberEqualityComparerAttribute<TAccessor,TMember>` | Member comparer for complex value objects. | Composite values with string or custom equality semantics. |
| [4] | `ComparerAccessors.*` | Built-in comparer accessors for ordinal, culture, invariant, and default comparison. | Prefer built-ins over local comparer classes. |
| [5] | `OperatorsGeneration` and `ConversionOperatorsGeneration` | Equality, comparison, arithmetic, and conversion operator generation settings. | Numeric wrappers and key interop where operators reduce call-site code. |

---
## [5][GENERATOR_CONTROLS]
>**Dictum:** *Generated members follow semantics, not habit.*

<br>

| [INDEX] | [SURFACE] | [CAPABILITY] | [USE] |
| :-----: | --------- | ------------ | --- |
| [1] | `FactoryMethodGeneration` | Controls generated factory method availability for union members. | Keep construction explicit at boundaries. |
| [2] | `SwitchMapMethodsGeneration` | Controls generated dispatch methods. | Generate exhaustive dispatch for closed shapes. |
| [3] | `SerializationFrameworks` | Selects generated serialization participation. | Use only with matching integration packages or object factories. |
| [4] | `TypeParamRef1` through `TypeParamRef5` | References generic type parameters in attributes. | Generic generated shapes that need attribute-level type references. |
| [5] | `UnionSwitchMapOverloadAttribute` | Controls nested union dispatch overload generation. | Prevent nested dispatch overload sprawl. |

---
## [6][RULES]
>**Dictum:** *The smallest generated shape that preserves semantics is the default.*

<br>

- Use `[ValueObject<T>]` for a primitive with invariant-bearing identity.
- Use `[ComplexValueObject]` for multi-member identity with generated equality.
- Use `[SmartEnum<T>]` for a closed keyed registry with item behavior or lookup.
- Use `[Union]` for polymorphic variants with domain names and exhaustive dispatch.
- Use ad-hoc unions for local alternative values where nested records are heavier than the semantics.
- Use generated `Switch`/`Map` with named arguments; keep manual pattern matching for protocol boundaries and native API shape only.
