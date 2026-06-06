# [THINKTECTURE_OBJECTS]

[IMPORTANT] Value objects define admission and equality. They do not replace LanguageExt rails.

## [1][VALUE_OBJECTS]

| [INDEX] | [CAPABILITY]             | [RULE]                                                                                 |
| :-----: | ------------------------ | -------------------------------------------------------------------------------------- |
|   [1]   | Factory generation       | Use generated `Create`, `TryCreate`, `Validate`, or configured factory names directly. |
|   [2]   | Custom validation errors | Convert generator errors once into Rasm `Error` or `Fault`.                            |
|   [3]   | Comparers                | Put string comparison policy on the generated declaration.                             |
|   [4]   | Conversion operators     | Enable only when interop removes boundary noise.                                       |
|   [5]   | Arithmetic operators     | Use only where failure cannot be hidden.                                               |

### [1.1][ATTRIBUTE_PROPERTIES]

| [INDEX] | [PROPERTY]                                                  | [USE]                                                |
| :-----: | ----------------------------------------------------------- | ---------------------------------------------------- |
|   [1]   | `KeyMemberName = "Value"`                                   | Branded scalar key field for equality and parsing    |
|   [2]   | `KeyMemberAccessModifier = AccessModifier.Public`           | Expose key when boundary protocol requires it        |
|   [3]   | `[KeyMemberEqualityComparer<TAccessor,TKey>]` / `[KeyMemberComparer<TAccessor,TKey>]` | Ordinal or ignore-case key comparison (arity source: `union-attributes.md §2`) |
|   [4]   | `SkipFactoryMethods`                                        | Custom `Fin` construction rail — rare                |
|   [5]   | `EqualityComparisonOperators` / `ComparisonOperators`       | `[ValueObject<T>]` only — not `[ComplexValueObject]` |

## [2][COMPLEX_OBJECTS]

Use complex value objects for normalized ranges, sample windows, tolerance bundles, formula variable sets, and boundary descriptors. Do not create parallel records beside a value object for the same concept.

## [3][DANGEROUS_OPTIONS]

| [INDEX] | [OPTION]                                | [RASM_POLICY]                                             |
| :-----: | --------------------------------------- | --------------------------------------------------------- |
|   [1]   | `SkipFactoryMethods`                    | Rare custom-rail choice; document lost generated surface. |
|   [2]   | `AllowDefaultStructs`                   | Boundary-only; default values often hide invalid state.   |
|   [3]   | `NullInFactoryMethodsYieldsNull`        | External binding only; domain absence uses `Option<T>`.   |
|   [4]   | `EmptyStringInFactoryMethodsYieldsNull` | Text boundary only with explicit protocol semantics.      |

## [4][RAIL_BRIDGE]

- Generated validation admits or rejects raw values.
- LanguageExt carries failure through `Fin<T>` or `Validation<Error,T>`.
- Rhino/GH/MathNet projections preserve native validity and tolerance in error detail.
- Do not wrap generated factories in single-call helpers.

## [5][V10_SHAPE]

`Directory.Packages.props` pins the `Thinktecture.Runtime.Extensions` version. Verify hard shape constraints with `uv run python -m tools.quality api query Thinktecture.Runtime.Extensions <symbol>`:

- `[ComplexValueObject]` requires `partial class` OR `partial struct` — **never** `record` or `record struct` (generator doubles emitted members).
- Properties are `{ get; }` only — no `{ get; init; }`, no positional record params.
- `ValidateFactoryArguments(ref ValidationError?, ref T1, ref T2, ...)` — all params `ref`, camelCase of property names. Property/param name mismatch fires the TTRESG analyzer diagnostic.
- Constructor is generated `private`; construction is via generated `Create` (throws `ValidationException`) / `TryCreate` (bool result + out item / out error) / `Validate` (returns nullable error).
- No ordering operators (`<`, `<=`, `>`, `>=`) on `[ComplexValueObject]`; only `[ValueObject<T>]` gets `EqualityComparisonOperators`/`ComparisonOperators`. Hand-write `IComparable<T>` in the partial if multi-field ordering is needed.
- `default(T)` is a compile error by default — set `AllowDefaultStructs = true` ONLY when zero-init semantics are genuinely valid (rare for invariant-bearing types).
- Multi-field struct value objects require `[StructLayout(LayoutKind.Auto)]` to satisfy Meziantou `MA0008`.

### [5.1][CUSTOM_VALIDATION_ERROR]

`[ValidationError<TCustom>]` + `TCustom : IValidationError<TCustom>` (with static `TCustom.Create(string)`) replaces per-call-site `ValidationException` → `MapFail` bridging. Generated `Validate` returns `TCustom?` directly into the Fin/Validation rail.

### [5.2][SMARTENUM_FACTORY_INITIALIZERS]

`[SmartEnum<TKey>]` codegen accepts any expression returning T as an item initializer — not just `new(...)` literals. Private static factory helpers compress repetitive case definitions (e.g. 23 cases × 3 lines → 23 × 1 line via 4 factory helpers that wrap shared cast + admits predicate). Generated `Items`/`Get`/`TryGet`/`Parse` behave identically.
