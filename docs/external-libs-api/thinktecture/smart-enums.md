# [H1][THINKTECTURE_SMART_ENUMS]
>**Dictum:** *A smart enum is a generated registry, not a dressed-up enum.*

<br>

[IMPORTANT] Use smart enums for closed vocabularies that carry behavior, metadata, type lookup, or wire keys. Prefer them over native enums when values need generated lookup, validation, dispatch, or item-specific methods.

[IMPORTANT] Baseline: stable pin `10.2.0`. Keep derived dictionaries only when lookup key differs from generated key; otherwise use generated `Items`, `Get`, `TryGet`, parse, and validation.

---
## [1][DECLARATION]
>**Dictum:** *The key type determines lookup and boundary behavior.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | --- |
| [1] | `[SmartEnum]` | Keyless item set with generated equality and dispatch. |
| [2] | `[SmartEnum<TKey>]` | Keyed item set with generated lookup, parse, conversion, comparison, and dispatch. |
| [3] | `KeyMemberName`, `KeyMemberKind`, and `KeyMemberAccessModifier` | Controls generated key member shape. |
| [4] | `SkipIComparable`, `SkipIParsable`, `SkipISpanParsable`, `SkipIFormattable` | Controls generated BCL interop interfaces. |
| [5] | `EqualityComparisonOperators` | Controls generated equality operators for keyed and keyless smart enums. |
| [6] | `SkipToString` | Suppresses generated display text when a custom representation owns it. |
| [7] | `DisableSpanBasedJsonConversion` | Disables span-based JSON conversion for keyed string smart enums. |

[IMPORTANT] Use stable keys at host or persistence boundaries. Use `string` keys for protocol names and `int` keys for compact numeric wire formats.

---
## [2][GENERATED_REGISTRY]
>**Dictum:** *Generated lookup replaces reflection and manual dictionaries.*

<br>

| [INDEX] | [GENERATED_MEMBER] | [ROLE] |
| :-----: | ------------------ | ---- |
| [1] | `Items` | Immutable generated item set. |
| [2] | `Get` | Key lookup that expects a valid key. |
| [3] | `TryGet` | Non-throwing key lookup. |
| [4] | `Validate` | Static validation for keyed construction and binding. |
| [5] | `Parse` and `TryParse` | Text parsing when parsing interfaces are generated. |
| [6] | `UnknownSmartEnumIdentifierException` | Exception raised by trusted lookup paths when the key has no matching item. |

[CRITICAL] Start from `Items`, `Get`, `TryGet`, and generated parse members before adding lookup tables. Add a derived map only when the lookup key is not the generated key, such as `Type` to `Kind`.

---
## [3][ITEM_BEHAVIOR]
>**Dictum:** *Behavior attached to an item removes downstream branching.*

<br>

| [INDEX] | [PATTERN] | [ROLE] |
| :-----: | --------- | ---- |
| [1] | Constructor metadata | Stores labels, requirements, native types, or topology classes per item. |
| [2] | Constructor delegates | Stores item-specific behavior without service maps. |
| [3] | Constructor predicate flags | Stores subset membership such as scalar, reportable, default, or host-visible state. |
| [4] | `[UseDelegateFromConstructor]` | Generates delegate-backed members from constructor parameters. |
| [5] | `UseDelegateFromConstructorAttribute.DelegateName` | Selects constructor delegate by name when multiple delegates exist. |
| [6] | `ValidateConstructorArguments` | Normalizes or rejects item constructor arguments during static item creation. |
| [7] | Instance methods over stored delegates | Exposes behavior as `item.Compute(...)`, `item.Bind(...)`, or equivalent domain vocabulary. |
| [8] | Generic smart-enum contracts | Allows enum-polymorphic algorithms when multiple vocabularies share the same generated behavior. |

[IMPORTANT] Prefer behavior-on-enum when the behavior is closed with the item set. Prefer a separate algebra only when behavior is independently extensible.

[CRITICAL] Model subsets as item metadata and derive sets from `Items`. Avoid detached `ReferenceEquals` chains or separate subset lists when a constructor flag can own membership.

---
## [4][DISPATCH]
>**Dictum:** *Generated dispatch keeps vocabulary exhaustive.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | --- |
| [1] | `Switch` | Exhaustive branch over all declared items. |
| [2] | `Map` | Exhaustive projection over all declared items. |
| [3] | `SwitchMethods` and `MapMethods` | Controls which dispatch methods are generated. |
| [4] | `SwitchMapStateParameterName` | Names the state parameter for state-threaded overloads. |
| [5] | State overloads | Pass immutable context into static lambdas without closure allocations. |

[CRITICAL] Use named arguments for generated dispatch. This preserves case alignment across generator changes and satisfies analyzer expectations.

---
## [5][COMPARISON_AND_CONVERSION]
>**Dictum:** *Keys compare like the protocol they represent.*

<br>

| [INDEX] | [SETTING] | [CAPABILITY] |
| :-----: | --------- | ------------ |
| [1] | `KeyMemberEqualityComparerAttribute<TAccessor,TKey>` | Key equality policy. |
| [2] | `KeyMemberComparerAttribute<TAccessor,TKey>` | Key ordering policy. |
| [3] | `SkipIComparable`, `ComparisonOperators`, `EqualityComparisonOperators` | Comparison and equality generation. |
| [4] | `ConversionToKeyMemberType` and `ConversionFromKeyMemberType` | Key interop. |
| [5] | `SerializationFrameworks` | Serialization participation with matching integration packages. |

---
## [6][RASM_ANCHORS]
>**Dictum:** *Rasm smart enums carry native geometry and GH2 behavior.*

<br>

| [INDEX] | [SYMBOL] | [KEY] | [ROLE] |
| :-----: | -------- | ---: | ---- |
| [1] | `Kind` | `int` | Maps Rhino/GH value types to topology and type identity. |
| [2] | `MassKind` | `int` | Carries mass-property requirements, compute delegates, and aggregate delegates. |
| [3] | `StatKind` | `int` | Selects statistical aggregate behavior. |
| [4] | `MeshDefect` | `int` | Names mesh defect categories. |
| [5] | `MeshMetric` | `int` | Defines mesh measurement vocabulary. |
| [6] | `PortKind` | `string` | Maps GH2 parameter types and generated constructor delegates. |

[IMPORTANT] `Kind.Of`, `GeometryKernel.KindOf`, and `PortKind.From` use generated `Items` as the registry base. Keep derived lookups tied to `Items` so new enum items become visible in one place.

[CRITICAL] Promote validation catalogs to smart enums when checks have stable keys, applicability predicates, and execution delegates. Generated `Items` then owns check discovery and grouping.
