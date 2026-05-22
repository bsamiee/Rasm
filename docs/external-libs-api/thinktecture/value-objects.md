# [H1][THINKTECTURE_VALUE_OBJECTS]
>**Dictum:** *Primitive values enter the domain through one generated gate.*

<br>

[IMPORTANT] Value objects are preferred for bounded primitives. They make invalid construction impossible at normal call sites and cheap to validate at host boundaries.

[IMPORTANT] Baseline: stable pin `10.2.0`. Use generated factories for domain primitive admission; use BCL `field` only for trivial property normalization without generated construction, parsing, conversion, or analyzer support.

---
## [1][DECLARATION]
>**Dictum:** *The attribute declaration is the API contract.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | --- |
| [1] | `[ValueObject<TKey>]` | Single-key value object over a primitive, BCL value, or domain key type. |
| [2] | `KeyMemberName` | Public member name for the wrapped key, commonly `Value`. |
| [3] | `KeyMemberAccessModifier` | Exposes or hides the key member according to boundary needs. |
| [4] | `KeyMemberKind` | Chooses field or property generation. |
| [5] | `SkipKeyMember` | Suppresses generated key member only when a manually declared member owns the same contract. |
| [6] | `ConstructorAccessModifier` | Controls raw constructor exposure; keep external construction on generated factories. |
| [7] | `NullInFactoryMethodsYieldsNull` | Allows class value-object factories to return `null` for `null` key input. |
| [8] | `EmptyStringInFactoryMethodsYieldsNull` | Allows string-key class factories to return `null` for empty or whitespace input. |
| [9] | `AllowDefaultStructs` | Allows explicit `default` or parameterless construction for struct value objects. |
| [10] | `DefaultInstancePropertyName` | Names generated static default instance property for struct value objects. |
| [11] | `SkipEqualityComparison` | Excludes a member from generated equality in complex value objects where it is not identity. |

[CRITICAL] Do not create local `Of`, `From`, or `Try` helpers around generated factories unless the method adds boundary semantics such as caller-name capture or rail conversion.

---
## [2][FACTORIES]
>**Dictum:** *Factory methods are the validation boundary.*

<br>

| [INDEX] | [GENERATED_MEMBER] | [ROLE] |
| :-----: | ------------------ | ---- |
| [1] | `Create` | Throws or returns according to generated contract; use only where invalid input is impossible or already validated. |
| [2] | `TryCreate` | Non-throwing construction with validation error output. |
| [3] | `Validate` | Static validation surface used by generated factories and generic rails. |
| [4] | `IObjectFactory<TSelf,TValue,TValidationError>` | Generic factory contract for static validation. |
| [5] | `CreateFactoryMethodName` and `TryCreateFactoryMethodName` | Renames generated factories to match public vocabulary. |
| [6] | `IObjectFactory<TSelf>` | Marker implemented by generated factories; do not use directly. |
| [7] | `IConvertible<TValue>.ToValue()` | Returns custom value representation for object-factory conversion. |

[IMPORTANT] `SkipFactoryMethods = true` removes generated factories and dependent generated support such as object-factory participation, conversion from key, parsable interfaces, arithmetic operators, and serializer converters unless an explicit object factory supplies the boundary contract.

---
## [3][VALIDATION]
>**Dictum:** *Normalize and reject before the instance exists.*

<br>

| [INDEX] | [HOOK] | [USE] |
| :-----: | ------ | --- |
| [1] | `static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref TKey value)` | Validate and normalize single-key values before construction. |
| [2] | `static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref T1 a, ref T2 b, ...)` | Validate and normalize complex value-object members before construction. |
| [3] | `ValidationError` | Carry generator-native failure messages without exceptions. |
| [4] | `[ValidationError<T>]` | Replace the default validation error type with a custom error contract. |
| [5] | `IValidationError<T>` | Custom validation error factory contract used by generated validation. |
| [6] | `IValidationError<T>.Create` | Creates custom validation error values from generated validation messages. |

[CRITICAL] Prefer factory validation over constructor validation. Constructor validation is exception-oriented and composes poorly with JSON, model binding, EF converters, and LanguageExt rails.

---
## [4][COMPARISON]
>**Dictum:** *String comparison is never implicit domain policy.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | --- |
| [1] | `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal,TKey>]` | Case-sensitive ordinal equality for string keys. |
| [2] | `[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase,TKey>]` | Case-insensitive ordinal ordering for string keys. |
| [3] | `[MemberEqualityComparer<TAccessor,TMember>]` | Per-member equality in complex value objects. |
| [4] | `DefaultStringComparison` | Complex value-object default for string members. |
| [5] | `SkipEqualityComparison` | Suppresses generated equality only for framework or identity-owned types. |

[IMPORTANT] Attach comparer attributes to the generated type rather than adding local comparer fields. This keeps equality, hashing, parsing, and lookup aligned.

---
## [5][CONVERSIONS_AND_OPERATORS]
>**Dictum:** *Generate interop only where it removes boundary noise.*

<br>

| [INDEX] | [SETTING] | [CAPABILITY] |
| :-----: | --------- | ------------ |
| [1] | `ConversionToKeyMemberType` | Converts a value object to its key type. |
| [2] | `ConversionFromKeyMemberType` | Converts a key type into a value object through generated construction. |
| [3] | `UnsafeConversionToKeyMemberType` | Emits unsafe key conversion for call sites that bypass validation deliberately. |
| [4] | `SkipIParsable` and `SkipISpanParsable` | Controls generated parsing contracts. |
| [5] | `SkipIComparable` and `ComparisonOperators` | Controls ordering support. |
| [6] | `AdditionOperators`, `SubtractionOperators`, `MultiplyOperators`, `DivisionOperators` | Generates numeric operator support for numeric wrappers. |
| [7] | `SkipIFormattable` | Controls formatting support. |

[CRITICAL] Generate conversions and operators for universal syntax only when the resulting call site remains rail-safe. Boundary code never hides validation failure behind implicit conversion.

---
## [6][PROMOTION_RULES]
>**Dictum:** *Recurring boundary invariants deserve generated identity.*

<br>

| [INDEX] | [CANDIDATE] | [GENERATED_SHAPE] |
| :-----: | ----------- | --------------- |
| [1] | Single bounded scalar | `[ValueObject<T>]`. |
| [2] | Reused pair or range with equality semantics | `[ComplexValueObject]`. |
| [3] | File, port, or wire identifier with format policy | `[ValueObject<string>]` plus string comparer attributes. |
| [4] | Toleranced sample or selector state crossing boundaries | `[ComplexValueObject]` with factory validation. |

[IMPORTANT] Promote recurring primitive and tuple validation into generated value objects before adding local validation maps or acceptance helpers.

---
## [7][RASM_ANCHORS]
>**Dictum:** *Rasm value objects define the tolerance boundary.*

<br>

| [INDEX] | [SYMBOL] | [SURFACE] | [ROLE] |
| :-----: | -------- | --------- | ---- |
| [1] | `AbsoluteTolerance` | `[ValueObject<double>]` | Finite positive model tolerance. |
| [2] | `RelativeTolerance` | `[ValueObject<double>]` | Fractional tolerance in `[0,1)`. |
| [3] | `AngleTolerance` | `[ValueObject<double>]` | Finite radian tolerance in model range. |
| [4] | `Op` | `[ValueObject<string>]` plus string comparers | Operation identity and failure labels. |
| [5] | `OpAcceptance.TryCreateValidated<TVO>` | `IObjectFactory<TVO,double,ValidationError>` | Shared bridge from generated factories to `Validation<Error,TVO>`. |
