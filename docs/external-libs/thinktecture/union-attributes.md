# [H1][THINKTECTURE_UNION_ATTRIBUTES]
>**Dictum:** *Union attributes control generated dispatch shape.*

<br>

[IMPORTANT] Pin **`Thinktecture.Runtime.Extensions`** at the version pinned in `Directory.Packages.props`. Verify attribute properties in local package XML.

Thinktecture `[Union]` generates case types, `.Switch()`, `.Map()` — **not** `operator +`/`|` on the union type in Thinktecture v10.

---
## [1][UNION_ATTRIBUTES]
>**Dictum:** *Generated dispatch replaces repeated switch arms.*

<br>

| [INDEX] | [PROPERTY]                                 | [EFFECT]                                              |
| :-----: | ------------------------------------------ | ----------------------------------------------------- |
|   [1]   | `[Union]`                                  | Case types, `.Switch()`, `.Map()`                     |
|   [2]   | `SwitchMapStateParameterName`              | State arg on `.Switch`/`.Map`; default `state`        |
|   [3]   | `SwitchMethods`, `MapMethods`              | Dispatch generation level — `[ENUMS]` [3]             |
|   [4]   | `[UnionSwitchMapOverload]`                 | Partial overloads at `StopAt`; requires `[ENUMS]` [3] |
|   [5]   | `NestedUnionParameterNames`                | Nested-case param naming — `[ENUMS]` [5]              |
|   [6]   | `FactoryMethodGeneration`                  | Case factory emission — `[ENUMS]` [6]                 |
|   [7]   | `ConversionFromValue`, `ConversionToValue` | Value↔union operators — `[ENUMS]` [7]                 |
|   [8]   | `[POLICY]`                                 | Equality, backing field, ctor access                  |

[ENUMS]
- [3] `SwitchMapMethodsGeneration`: None, Default, DefaultWithPartialOverloads
- [5] `NestedUnionParameterNameGeneration`: Default (intermediate type names), Simple
- [6] `FactoryMethodGeneration`: Default, None, Always
- [7] `ConversionOperatorsGeneration`: None, Implicit, Explicit — `ConversionFromValue` default Implicit; `ConversionToValue` default Explicit

[POLICY]
- `SkipEqualityComparison` — omit generated equality comparers
- `UseSingleBackingField` — one backing field shared by all cases
- `ConstructorAccessModifier` — `UnionConstructorAccessModifier` Private, Internal, Public; default Public

```csharp
[Union(
    SwitchMapStateParameterName = "context",
    SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads,
    MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
[UnionSwitchMapOverload(StopAt = typeof(FailureCase))]
public abstract partial record Command { /* cases */ }
```

Ad-hoc unions: `[Union<T1,T2,...>]` and `[AdHocUnion(typeof(...))]` — up to five members with per-slot `T{n}Name`, `T{n}IsStateless`, `T{n}IsNullableReferenceType`.

---
## [2][VALUE_OBJECT_ATTRIBUTES]
>**Dictum:** *Value objects enforce boundary invariants at construction.*

<br>

| [INDEX] | [ATTRIBUTE]                                   | [ROLE]                                                      |
| :-----: | --------------------------------------------- | ----------------------------------------------------------- |
|   [1]   | `[ValueObject<T>]`                            | Single-field brand; operator props (`AdditionOperators`, …) |
|   [2]   | `[ComplexValueObject]`                        | Multi-field VO — partial class/struct only                  |
|   [3]   | `[ValidationError]`/`[ValidationError<T>]`    | `ValidateFactoryArguments(ref TFault?, …)`                  |
|   [4]   | `[KeyMemberEqualityComparer<TAccessor,TKey>]` | Custom key equality                                         |
|   [5]   | `[KeyMemberComparer<TAccessor,TKey>]`         | Custom key ordering                                         |
|   [6]   | `[MemberEqualityComparer<TAccessor,TMember>]` | Complex VO member equality                                  |
|   [7]   | `[IgnoreMember]`                              | Exclude member from generated equality                      |
|   [8]   | `[ObjectFactory]`/`[ObjectFactory<T>]`        | Deser/EF/model-binding factories                            |

`EquatableValueObject` is **absent** from the Thinktecture v10 public API.

Bridge Thinktecture validation once into LanguageExt rails at the boundary — see `objects.md`.

Set **`SerializationFrameworks = SerializationFrameworks.None`** on VOs and SmartEnums when JSON/MessagePack integration packages are not pinned — core generator defaults to `All`.

---
## [3][SMART_ENUM_ATTRIBUTES]
>**Dictum:** *Smart enums replace stringly constants with total dispatch.*

<br>

| [INDEX] | [ATTRIBUTE]                                                | [ROLE]                            |
| :-----: | ---------------------------------------------------------- | --------------------------------- |
|   [1]   | `[SmartEnum]`                                              | Keyless smart enum                |
|   [2]   | `[SmartEnum<TKey>]`                                        | Keyed smart enum                  |
|   [3]   | `SwitchMethods`/`MapMethods`/`SwitchMapStateParameterName` | Same knobs as §1 unions           |
|   [4]   | `[UseDelegateFromConstructor(DelegateName="…")]`           | SmartEnum delegate-backed partial |

See `enums.md` for SmartEnum patterns.

---
## [4][WHEN_NOT_TO_USE_UNION]
>**Dictum:** *Generic or ref-struct constraints block Thinktecture union codegen.*

<br>

Use plain `abstract record` + manual `this switch` when:
- Generic over state (`Transition<TState>`, `Request<T>`, …).
- `allows ref struct` conflicts with generated case shapes.

---
## [5][RULES]
>**Dictum:** *Dispatch attributes are architecture, not decoration.*

<br>

- Prefer `SwitchMapStateParameterName` over duplicating context in every case payload.
- Keep exhaustive dispatch: generated `.Switch` / `.Map` or manual total `switch`.
- Hand `operator +`/`|` on domain types are **application-defined** — not generated by Thinktecture.
- Cross-reference `unions.md`, `objects.md`, `sourcegen.md`.
