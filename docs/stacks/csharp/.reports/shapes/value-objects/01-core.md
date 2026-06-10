# Generated Value Objects — Core Admission Surface

[OWNER_DECLARATION]:
- `[ValueObject<TKey>]` and `[ComplexValueObject]` attach to a `partial` class or struct; the generator weaves factories, identity, equality, conversion, parsing, and metadata into the owner, leaving the visible body for invariants only: the validation partial, optional comparer attributes, and domain operations.
- The generated interface set on a keyed owner is the dispatch contract: `IEquatable<T>`, `IKeyedObject<TKey>`, `IConvertible<TKey>` (explicit-interface `ToValue()`, aggressively inlined), `IObjectFactory<T,TKey,TError>` (static abstract `Validate`), and an internal metadata-owner interface; complex owners drop the keyed interfaces and gain `IEqualityOperators<T,T,bool>`.
- One attribute per owner: stacking both attributes on the same type is an analyzer error; constructors must all be private (the generated constructor's accessibility is the `ConstructorAccessModifier` property, default private); primary constructors are rejected because they cannot be private.
- Generic owners are first-class: `[ValueObject<TypeParamRef1>]` on `Box<T>` generates a fully generic owner — generic factory triad, generic conversions — and each closed constructed type materializes its own static metadata instance; `TypeParamRef1`–`TypeParamRef5` are positional placeholders into the owner's type parameters.
- Struct owners are stamped `[StructLayout(LayoutKind.Auto)]` by the generator; field-order layout assumptions and unmanaged-interop use of a value-object struct are void by construction unless the author declares an explicit layout attribute, which suppresses the default.

[FACTORY_TRIAD]:
- `Validate` is the primal factory; `Create` and `TryCreate` are generated derivations that call it. Scalar shape: `static TError? Validate(TKey? value, IFormatProvider? provider, out T? obj)` (struct owners emit a non-nullable `out T obj`); complex shape drops the provider and takes one parameter per member. Only `Validate` returns the typed error and the instance in one call — it is the sole signature worth bridging to a result rail.
- `Create` throws `ValidationException` carrying `validationError.ToString()` — the typed error is string-flattened and lost on the throwing path. `TryCreate` ships two overloads (`out obj` and `out obj, out error`) with full `NotNullWhen(true)`/`NotNullWhen(false)` flow annotations.
- `CreateFactoryMethodName` and `TryCreateFactoryMethodName` rename the generated pair (for example, `From`/`TryFrom`), letting the owner's public grammar match domain language while `Validate` keeps its contract name for the static-abstract interface.
- `FactoryPostInit()` is a generated instance partial invoked after successful construction on the factory path only — a definition-time advice point for derived-cache priming that constructor-route materialization never touches; an undeclared body compiles away to nothing.
- The generated outcome is projected once at the admission boundary; `Validate` collapses error and instance into a single rail conversion:

```csharp
public static Fin<Tag> Admit(string raw) =>
   Tag.Validate(raw, null, out var tag) is { } error
      ? Fin<Tag>.Fail(Error.New(error.Message))
      : Fin<Tag>.Succ(tag!);
```

[VALIDATION_PARTIALS]:
- The validation hook is a static partial with the error slot first and every argument by `ref`: scalar `static partial void ValidateFactoryArguments(ref TError? validationError, ref TKey value)`; complex `static partial void ValidateFactoryArguments(ref TError? validationError, ref M1 a, ref M2 b)`. Writing back through a `ref` parameter is the sanctioned normalization channel — trim, clamp, and canonicalize before capture; the stored state is the post-hook value.
- An undeclared partial is elided by the compiler; the hook is pay-for-play per owner.
- Framework null gates run before the user hook: class owners reject a null key with a generated message; complex owners null-check every reference-type member per member — the hook body can assume non-null inputs.
- Two validation seams exist and are not symmetric: `ValidateFactoryArguments` runs only on the factory path; `ValidateConstructorArguments(ref ...)` runs inside the private constructor on every construction path, including persistence rehydration that materializes instances via the raw constructor. Normalization placed only in the factory hook is silently bypassed on the constructor path; an invariant that must survive rehydration belongs in the constructor hook, which can only throw, not report a typed error.

```csharp
static partial void ValidateFactoryArguments(ref TagError? validationError, ref string value)
{
   value = value.Trim();
   validationError = value.Length is 0 ? TagError.Create("<message-a>") : null;
}
```

[ERROR_CONTRACT]:
- `IValidationError<TSelf>` demands exactly one member: `static abstract TSelf Create(string message)` — no message property, no base class. Any sealed type with a static `Create(string)` satisfies it; `[ValidationError<TError>]` on the owner rewires every generated signature — `Validate` return type, `TryCreate` out-parameter, the `IObjectFactory` arity, the type-converter arity — to the custom error type.
- Framework-originated failures route through the same contract: null-key rejection, parse-validation failure, and converter errors are all minted via `TError.Create(message)` — a custom error type therefore observes every admission failure, and its `Create` must accept arbitrary framework prose, not a closed vocabulary.
- The throwing path calls `validationError.ToString()`; a custom error type that does not override `ToString()` produces exceptions whose message is the error type's fully-qualified name. `ToString()` returning the message is a load-bearing obligation the interface does not state.

```csharp
public sealed class TagError : IValidationError<TagError>
{
   public string Message { get; }
   private TagError(string message) => Message = message;
   public static TagError Create(string message) => new(message);
   public override string ToString() => Message;
}
```

[KEY_MEMBER_IDENTITY]:
- The default key member is `private readonly TKey _value` — a private field, not a public property. The key escapes only through the generated conversion operator and the explicit-interface `ToValue()`; exposing `.Value` requires `KeyMemberAccessModifier = AccessModifier.Public, KeyMemberKind = MemberKind.Property`, and `KeyMemberName` follows the declared kind. Default-hidden identity is deliberate: consumers compare and dispatch on the owner, not its raw key.
- `SkipKeyMember = true` hands the key member to the author; the generator targets the declared member by `KeyMemberName`, and a property implementation must have a private `init` accessor — a public `init` is an analyzer error, closing the object-initializer bypass around the factory.
- A nullable key member is rejected by the analyzer; absence belongs to the factory's null-handling modes or to an option carrier outside the owner.

[EQUALITY_COMPARISON_POLICY]:
- String-keyed owners default to ordinal-case-insensitive equality, hashing, and ordering, silently merging identifiers differing only in case. The analyzer warns (TTRESG048) until the policy is explicit via `[KeyMemberEqualityComparer<TAccessor, TKey>]`; complex owners with string members get the same default and the parallel warning (TTRESG049) until `DefaultStringComparison` is set on the attribute.
- Comparer policy travels as accessor types: `ComparerAccessors.StringOrdinal`, `.StringOrdinalIgnoreCase`, culture variants, and `Default<T>` are typeclass-style carriers with static comparer properties; the generated `Equals`, `GetHashCode`, `CompareTo`, and all relational operators call through the accessor statically — zero allocation, and one attribute swap changes every comparison surface at once.
- Equality and ordering comparers are paired by diagnostics: a comparer without its counterpart draws an analyzer warning, because hash containers and sorted containers would disagree on identity.
- `[MemberEqualityComparer<...>]` on any complex-owner member flips equality membership from all-members to opt-in: unmarked members vanish from `Equals`, `GetHashCode`, and the generated `ToString` while remaining factory parameters and stored state — and `DefaultStringComparison` becomes inert for them. Two instances differing only in an unmarked member compare equal; the change is an equality-semantics rewrite, not a local tweak.
- `[IgnoreMember]` removes a stored member from the generator's world entirely (no factory parameter, no equality participation); immutability of the ignored member is the author's obligation. Expression-bodied computed properties are invisible to the generator without any attribute.
- Relational operators couple downward: `ComparisonOperators` broader than `EqualityComparisonOperators` draws a divergence warning (TTRESG105) — set the pair together or not at all.

[CONVERSION_SURFACES]:
- The generated operator matrix encodes three risk tiers, each with its own attribute property: safe owner→key conversion (`ConversionToKeyMemberType`, default implicit); unsafe owner→key conversion that can throw (`UnsafeConversionToKeyMemberType`, default explicit, for cases such as a class owner with a struct key where a null owner dereferences); and key→owner conversion (`ConversionFromKeyMemberType`, default explicit), which routes through `Create` and therefore throws `ValidationException` on bad input.
- The key→owner cast is admission-preserving but exception-shaped: `(Tag)"<value-a>"` is a hidden `Create` call. Setting `ConversionFromKeyMemberType = ConversionOperatorsGeneration.Implicit` manufactures an implicit conversion that throws — the most hazardous configuration on the surface; it also cannot exist when factories are skipped, because the operator body is the factory.
- `IConvertible<TKey>.ToValue()` being an explicit interface implementation keeps the raw key off the owner's IntelliSense surface while remaining reachable for generic boundary code constrained on the interface.

[DERIVED_OPERATORS_PARSING_FORMATTING]:
- Arithmetic operator generation defaults on whenever the key type implements the corresponding generic-math interface: a bare `[ValueObject<int>]` ships `+ - * /` and their `checked` variants with no configuration. Every operator body routes the raw result back through `Create` — arithmetic re-enters admission, so `a - b` on validated operands can throw `ValidationException` when the result violates the invariant. An innocent-looking subtraction is a validation site; owners with range invariants must treat generated arithmetic as a policy decision.
- `OperatorsGeneration.DefaultWithKeyTypeOverloads` adds mixed-operand forms in both directions (`owner op key`, `key op owner`), each returning the owner and each re-validating — ergonomic at call sites, but it reopens raw-key arithmetic adjacent to the owner.
- `IParsable<T>`/`ISpanParsable<T>` generation composes two static-abstract hops: parse the key via the key type's own parsable implementation (threading the `IFormatProvider`), then validate via `IObjectFactory`. The exception taxonomy splits by entry point: `Parse` throws `FormatException` (string-flattened error), `Create` throws `ValidationException`, `TryParse` discards the typed error entirely, and only `Validate`/`TryCreate` preserve it.
- `IFormattable` and `ToString()` delegate to the key; complex owners interpolate equality members as `{ A = ..., B = ... }` — opt-in equality membership also edits the diagnostic string. `SkipIFormattable`, `SkipToString`, `SkipIComparable`, `SkipIParsable` (which forces `SkipISpanParsable`) prune surfaces individually; `SkipFactoryMethods` is the master cut: it kills factories, type converter, object-factory interface, key→owner conversion, parsing, and forces all arithmetic to none, leaving a pure structural shell.

[STRUCT_ZERO_INIT_GATES]:
- Struct owners implement `IDisallowDefaultValue` by default, and the analyzer escalates zero-init to a compile error (TTRESG047) at `default` and parameterless-`new` sites — the admission bypass via zero-init is closed at declaration sites.
- The gate has holes: array allocation (`new T[n]` then indexing) compiles cleanly and materializes an unvalidated instance, as do generic `default(T)` in unconstrained code and field zero-init. A struct owner whose invariant excludes the zero value must still tolerate observing it from those routes or remain a class.
- `AllowDefaultStructs = true` legalizes zero-init and emits a canonical instance as a `public static readonly` field named by `DefaultInstancePropertyName` (default `Empty`). The analyzer forbids the setting in two sound cases: a struct owner with a reference-type key (default would hold a null key behind a non-nullable member) and a complex struct owner containing members that themselves disallow defaults — zero-init permission cannot be granted outward when an inner owner denies it.
- The gate extends to optional parameters: `= default` on a parameter of a disallow-default owner is the same compile error, and the canonical static instance is not a constant expression, so it cannot be a parameter default. Optionality enters as an `Option<T>` parameter resolved against the canonical instance, or as overloads.

[NULL_YIELD_MODES]:
- `NullInFactoryMethodsYieldsNull = true` converts null input from rejection into success-with-null: `Validate` returns a null error and a null instance, `Create` becomes nullable-returning, and `TryCreate` returns true with a null out-value — the `NotNullWhen(true)` annotation is removed. `EmptyStringInFactoryMethodsYieldsNull = true` extends the same to empty and whitespace strings and force-enables the null mode; both are inert for struct owners, struct keys, or skipped factories.
- Any single-projection bridge over `Validate` must handle the (null error, null instance) quadrant explicitly — `obj!` on the success path is wrong under these modes. The honest projection target is an option-of-owner with absence decided at the boundary, never propagated as null inward.

[ALTERNATE_ADMISSION_AND_SERIALIZATION]:
- `[ObjectFactory<TValue>]` adds a second admission key type and demands a hand-written `static Validate(TValue value, IFormatProvider? provider, out T item)` returning the owner's error type — the custom factory joins the triad grammar rather than inventing a parallel one. When `UseForSerialization` is non-none or the persistence flag is set, an instance `TValue ToValue()` becomes mandatory: alternate admission must be a round-trip, not a one-way parse.
- Multiple object factories partition responsibilities under analyzer enforcement: at most one may own persistence materialization, at most one may own model binding, and serialization frameworks may not overlap across factories — each wire concern resolves to exactly one admission route.
- `SerializationFrameworks` flags on the owner attribute and `UseForSerialization` on object factories select which converters bind, but converter code materializes only when the corresponding integration assembly is present — on a runtime without an integration package the flags are dormant metadata. The always-present seam is the generated `TypeConverter` registration, which exists unless factories are skipped.
- Factory-routed deserialization re-validates and re-normalizes; constructor-routed materialization runs only the constructor hook. Values normalized into storage by the factory and read back via the constructor path bypass current normalization — normalization changes are data migrations, not code edits.

[METADATA_DISPATCH]:
- Every generated owner publishes a static metadata record through an internal interface: key type, validation-error type, key-extraction delegates and expression-tree forms, a throwing from-key delegate (via `Create`), a non-throwing weakly-typed `TryGetFromKey` (via `Validate`), and a from-key-via-constructor expression that bypasses factory validation by design, for persistence materialization of already-admitted values.
- Complex owners reify their member set as `MemberInfo` lists extracted from an anonymous-type projection expression at static-init time — frameworks bind members without reflection-over-attributes scans, and the member list is exactly the equality-membership-filtered assignable set.
- The metadata family is a closed variant set with exhaustive `Switch`/`Map` over the shape kinds (keyed value object, complex value object, and sibling generated shapes), including state-threaded overloads taking a context argument — boundary code dispatches on the kind of generated owner a type is without type-testing the owner itself.
