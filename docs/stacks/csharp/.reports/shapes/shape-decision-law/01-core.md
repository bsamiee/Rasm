# Shape Decision Law

[OWNER_SELECTION]:
- Five discriminants decide the owner before any attribute is written: admission (does raw material cross a trust boundary here), identity regime (key, structural, case, or reference), variant arity (one shape vs N alternatives), payload timing (case data fixed at declaration vs constructed per occurrence), and openness (closed vocabulary vs foreign extension). Every misplaced shape traces to one mis-answered discriminant.
- One scalar carrying an invariant, normalization, or non-default equality earns `[ValueObject<T>]`. The generated key member defaults to a private field `_value` — the provider shape is erased at admission and re-exposed only through explicit conversion or `IConvertible<T>.ToValue()`; a public `Value` property is an opt-in (`KeyMemberAccessModifier = AccessModifier.Public`, `KeyMemberKind = MemberKind.Property`), not the default. Concept law: the owner publishes domain semantics, never the raw key.
- N fields forming one concept under structural identity earn `[ComplexValueObject]`. The sum-encoded-as-product test rejects it: when any field is meaningful only under some discriminant value, the concept is a union, and the complex value object is the wrong shape regardless of field count.
- A bounded vocabulary whose case data is fixed at declaration earns `[SmartEnum<TKey>]` when identity must cross a wire (key identity), or keyless `[SmartEnum]` when the vocabulary exists purely for process-local behavior selection (no serialization is generated without an object factory). Items are singletons; any per-occurrence payload disqualifies the shape and pushes to a union.
- Closed alternatives with per-occurrence payload and shared base semantics earn a regular `[Union]` class hierarchy; one value ranging over two to five unrelated existing types with no shared invariant earns an ad-hoc `[Union<T1,...>]`/`[AdHocUnion(...)]`. The regular/ad-hoc split is relatedness of cases — do the cases mean anything outside this union — not arity or convenience.
- Interior product data with no invariant, no admission, and no identity beyond structure stays a plain `record`/`readonly record struct`. Generated attributes on such a type buy analyzer ceremony (partial, private constructor, readonly enforcement, comparer demands) plus a factory that decides nothing — pure decoration.
- A wire contract earns a protocol DTO record only when its topology diverges from the domain owner (different field graph, versioned members, protocol naming). When the wire shape is one scalar, `[ObjectFactory<TWire>]` deletes the DTO entirely; the owner carries the protocol projection as a declared aspect.
- Singleton-vs-instance is the sharpest smart-enum/union discriminant: a smart enum is a fixed set of named instances dispatched by identity; a union is a fixed set of shapes dispatched by case. Vocabulary that starts sprouting constructor arguments at use sites has already become a union.
- Value object vs ad-hoc union for the same data: presence of any invariant, normalization, or admission rule selects the value object; the ad-hoc union is structural pass-through and validates nothing.

[CLOSURE_LAW]:
- Union closure is analyzer-enforced, not conventional: the base must be sealed or expose only private constructors (TTRESG054); a non-abstract derived union may not be less accessible than its base (TTRESG056); derived cases must not be generic (TTRESG053); record-based unions must be sealed, so any multi-level nesting forces class cases (TTRESG055). An inner type that fails to derive from the union is flagged as a stray (TTRESG106).
- Smart-enum case types invert visibility by depth: first-level derived item classes must be private (TTRESG014), deeper levels public (TTRESG015). Identity lives in the public `static readonly` item fields while the case types stay invisible — consumers can never type-dispatch on a case class, only on item identity or generated `Switch`. Static properties are not items (TTRESG101); an itemless enum is flagged (TTRESG100); an enum with no derived item types must be sealed (TTRESG037).
- A type owns exactly one shape: duplicate or mixed family attributes are rejected outright (TTRESG063/064/065/066), and an ad-hoc union demands at least two members (TTRESG067) — degenerate one-member unions cannot exist.
- `ConstructorAccessModifier` on a union privatizes the generated implicit conversion operators along with the constructors — token-gated construction: the only way in is a hand-written factory on the owner. On value objects the constructor is private by default; on unions it is public by default and must be tightened deliberately.
- Complex value object members are pushed toward `required` (TTRESG104) with `init` accessors forced private (TTRESG042) and fields/properties forced read-only (TTRESG001/003, including base-class members TTRESG034/035): the generated factory initializes through the object initializer while external bypass construction is structurally impossible.

[DISPATCH_FORM]:
- Three dispatch forms cover every shape decision: generated `Switch`/`Map` (the call site supplies arms; the family is closed and growth breaks every call site at compile time), case-owned delegate rows (`[UseDelegateFromConstructor]`: one partial method, N per-item implementations wired through the constructor), and frozen tables (derived correspondences, legitimate only when the policy cannot live on the case). Selection: call-site-varying behavior → Switch; case-constant behavior with uniform signature → delegate rows; behavior keyed by something the family does not own → table.
- Exhaustiveness is the product: adding a case changes the generated `Switch`/`Map` signature, so every consumer fails to compile until it answers the new case. The named-argument analyzer (TTRESG046) binds arms to cases by name, making arm order irrelevant and case renames loud.
- State-threaded overloads put `TState` first and pair with the static-lambda hint (TTRESG1001); generated `Switch`/`Map` declare `allows ref struct` on `TState` and `TResult`, so a `ReadOnlySpan<char>` can be the dispatch state or result with zero capture and zero boxing:

```csharp
[Union<TypeParamRef1, string>(T1Name = "Parsed", T2Name = "Raw")]
public partial struct Field<T> where T : notnull;

static int Width(Field<int> field, ReadOnlySpan<char> pad) =>
    field.Switch(pad,
        parsed: static (span, value) => value + span.Length,
        raw:    static (span, text) => text.Length + span.Length);
```

- Partial coverage is an owner decision, not a call-site choice: only `SwitchMapMethodsGeneration.DefaultWithPartialOverloads` emits `SwitchPartially`/`MapPartially` with a `@default` arm. A family that must always be totally handled never generates the partial forms — non-exhaustive dispatch is made inexpressible at the type.
- `[UnionSwitchMapOverload(StopAt = ...)]` generates overloads that treat an intermediate union as a single arm without descending into its cases — dispatch granularity over a nested hierarchy is declared at the owner, and grouped handling delegates to the intermediate union's own `Switch`. Nested naming is a declared policy: the default composes intermediate type names into arm parameters (`failureNotFound`); `NestedUnionParameterNameGeneration.Simple` uses leaf names and turns cross-branch name collisions into compile errors.
- `Map` is the inline value table — one constant per case, no lambdas. Secondary correspondences (case → caption, case → cost) derive from the primary family through `Map` instead of a parallel dictionary that can drift.
- Smart-enum `Switch` dispatches through a generated per-item ordinal index, not key comparison; key lookup is already a frozen table — a lazily built `FrozenDictionary` with a `ReadOnlySpan<char>` alternate lookup for string keys. Hand-building a key→item dictionary, or a span-keyed parse path, duplicates infrastructure the owner already generated.
- Delegate-row constraints shape the behavior split: the partial method must be partial (TTRESG050) and non-generic (TTRESG051 — generic per-item behavior moves to derived item types); `DelegateName` forces a named delegate type, which the generator also synthesizes automatically when ref/out parameters appear. Within a smart enum: delegate rows for uniform-signature behavior, derived item types for multi-member or generic behavior, instance properties for data columns — and since the generated constructor synthesizes its parameter list from declared instance properties, adding a column breaks every item declaration: table integrity enforced by the compiler.

```csharp
[SmartEnum<string>]
public sealed partial class Backoff
{
    public static readonly Backoff Fixed = new("fixed", static (attempt, unit) => unit);
    public static readonly Backoff Linear = new("linear", static (attempt, unit) => attempt * unit);
    public static readonly Backoff Squared = new("squared", static (attempt, unit) => attempt * attempt * unit);

    [UseDelegateFromConstructor]
    public partial TimeSpan Delay(int attempt, TimeSpan unit);
}
```

- Frozen tables earn existence in exactly three places: cross-product keys (case × case correspondences no single family owns), externally sourced policy admitted at startup, and keys outside any generated family. Everything else is either `Map`, a delegate row, or an instance property.

[IDENTITY_AND_COMPARERS]:
- Identity has three regimes and the owner choice fixes the regime: key identity (keyed value objects and smart enums — equality, hash, ordering all delegate to the key through a declared comparer), structural identity (complex value objects and ad-hoc unions — member-wise with comparer policy), and case identity (regular unions — the generator emits no `Equals`/`GetHashCode` for the hierarchy; record cases get value equality, class cases reference equality, and a mixed hierarchy silently mixes regimes per case).
- `OrdinalIgnoreCase` is the pervasive silent default: string-keyed smart-enum lookup, generated string equality, complex value object `DefaultStringComparison`, and ad-hoc union `DefaultStringComparison` all default to it — two values differing only in case are equal and collide in hash containers. The analyzers refuse to leave string identity implicit on value objects (TTRESG048/049); ordinal-sensitive domains must declare `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]` or `DefaultStringComparison = StringComparison.Ordinal` at the owner.
- Comparer policy is typeclass-shaped: `IEqualityComparerAccessor<T>`/`IComparerAccessor<T>` expose static comparer properties, with `ComparerAccessors.*` prebuilt and the comparer type checked against the member type (TTRESG041). Equality and ordering comparers must be declared as a pair (TTRESG102/103) or hashing and sorting disagree on identity; `ComparisonOperators` coerces `EqualityComparisonOperators` upward because ordering presupposes equality.
- `OperatorsGeneration.DefaultWithKeyTypeOverloads` adds operator overloads against the raw key type — heterogeneous comparison and arithmetic (`amount + 42m`) without converting either side, keeping the owner in play at use sites that would otherwise cast out.
- `[IgnoreMember]` removes a member from generated equality and factory synthesis with a caller-guaranteed immutability obligation; `SkipEqualityComparison` hands identity entirely to the host for entity semantics or interned instances. Container-level identity ships ready-made: `StringKeyedObjectComparer<T>` for string-keyed owners in sets and dictionaries, `ProjectionEqualityComparer<T,TKey>` for projection-keyed collections.

[DEFAULT_VALUE_LATTICE]:
- `IDisallowDefaultValue` marks owners whose `default` is not a value; the analyzer flags default-initialized variables (TTRESG047), and because an owner can never be a compile-time constant, optional parameters of owner type are inexpressible — absence enters as an option-typed parameter or an overload, never `= default`.
- Default-safety composes transitively through the member closure rather than being declared locally: a struct value object with a reference-type key must not allow defaults (TTRESG057), and a complex struct whose members disallow defaults must not allow them either (TTRESG058). One default-hostile member poisons the entire product, by analyzer.
- A struct that legitimately has a default opts in with `AllowDefaultStructs = true` and names it via `DefaultInstancePropertyName` (`Empty`, `Zero`) — a canonical named instance replaces an anonymous bit pattern, and the name is the documentation of why default is legal.
- Struct/class selection on value objects is an absence-semantics decision: a class admits null at the factory edge with `NullInFactoryMethodsYieldsNull`/`EmptyStringInFactoryMethodsYieldsNull` absorbing degenerate input into null instead of an error; a struct trades away absence for layout and inherits the default lattice above.

[GENERATION_MECHANICS]:
- The partial split is a reading contract: the user-declared partial holds only domain members; factories, equality, conversions, parsing, and serializer wiring land in the generated partial. Primary constructors are rejected (TTRESG043) because the generator owns the private constructor — case and item construction routes are never user-improvisable.
- The construction advice chain is a typed pipeline: `static partial ... ValidateFactoryArguments(ref TError? validationError, ref TMember ...)` normalizes arguments in place through `ref` and may declare any return type; a non-void return switches the generated declaration to `private` and forwards the returned value into `partial void FactoryPostInit(T factoryArgumentsValidationError)`, invoked on the freshly constructed instance — evidence computed once during static validation arrives at the instance after construction, and unimplemented partial-void calls compile away to nothing:

```csharp
[ValueObject<string>]
public sealed partial class Channel
{
    private static partial string? ValidateFactoryArguments(
        ref ValidationError? validationError, ref string value)
    {
        var submitted = value;
        value = value.Trim();
        validationError = value.Length is 0 ? new ValidationError("<blank>") : null;
        return submitted.Length == value.Length ? null : submitted;
    }

    // generated counterpart: partial void FactoryPostInit(string? factoryArgumentsValidationError);
    partial void FactoryPostInit(string? factoryArgumentsValidationError) =>
        System.Diagnostics.Debug.WriteLine(factoryArgumentsValidationError);
}
```

- `static partial void ValidateConstructorArguments(ref ...)` runs inside the generated constructor for throw-style invariants on trusted construction paths — the exception-shaped hook for callers that bypass the factory verbs by design (rehydration), distinct from the error-shaped factory hook.
- Factory verbs come as a fixed triple — throwing `Create`, boolean `TryCreate`, error-returning `Validate` — renameable via `CreateFactoryMethodName`/`TryCreateFactoryMethodName` when the domain verb is not "create" (`Parse`/`TryParse` aligns the owner with the BCL parsing idiom).
- The validation error type is a definition-time design axis: `[ValidationError<TError>]` with the static `TError Create(string)` contract substitutes a domain error type into every generated factory, parser, and converter — error vocabulary is declared on the owner, not mapped at call sites.
- `TypeParamRef1..5` compute generic owners from one declaration: attribute positions bind to the declaring type's own parameters across `[SmartEnum<TypeParamRef1>]`, `[ValueObject<TypeParamRef1>]`, and `[Union<TypeParamRef1, ...>]`. Parameters used as keys require `notnull` (TTRESG074), unreferenced parameters are flagged (TTRESG107), and `allows ref struct` parameters are rejected on ad-hoc unions (TTRESG073).
- Conversion operator defaults encode evidence direction: owner→key is implicit (erasing evidence is free), key→owner is explicit (creating evidence validates), and throw-capable owner→key conversions are segregated under `UnsafeConversionToKeyMemberType`. Ad-hoc unions mirror this: value→union implicit, union→value explicit.
- Regular unions generate value→case conversion operators only when a non-abstract, non-interface case without required members has a single-argument constructor whose parameter type is unique across the family — argument-type routing exists exactly where it is unambiguous and silently withdraws when two cases would compete.
- Ad-hoc union layout is a declared memory decision: per-member fields by default (struct members inline, union size approaches the sum), `UseSingleBackingField` boxes everything into one object field (pointer-sized, allocation per struct construction), and `TXIsStateless` stores only the discriminator for marker cases (accessors return `default`). Marker-case sibling types collapse into stateless members at zero bytes.
- The generator is observable through MSBuild build properties (log file path, log level, invocation counter) — generation drift is diagnosable from build output without decompiling generated sources.

[SERIALIZATION_SEPARATION]:
- `SerializationFrameworks` defaults to `All`: a keyed owner is wire-capable through its key representation across the JSON and binary stacks by default. The separation decision is therefore explicit in both directions — `None` severs codec from the owner when a protocol DTO owns the wire; leaving the default makes the key representation the contract and deletes the DTO.
- `[ObjectFactory<TWire>]` redirects the wire shape per concern: static `TError? Validate(TWire value, IFormatProvider? provider, out T item)` admits, instance `TWire ToValue()` projects, and `UseForSerialization` partitions frameworks among factories with overlap rejected (TTRESG070). Persistence and binding routes are singular (TTRESG068/069). `HasCorrespondingConstructor = true` declares a validation-bypassing rehydration route through a single-argument constructor (TTRESG059) and is forbidden on smart enums (TTRESG060) — rehydration of a vocabulary must resolve to existing items, never construct new ones.
- Keyless smart enums serialize only through an object factory: vocabulary identity without wire identity is the default posture, and wire exposure is a declared exception rather than an ambient capability.
- A complex value object plus `[ObjectFactory<string>]` collapses a structural concept to a parse/format micro-grammar — one textual representation owned by the type instead of a field-wise DTO plus mapper pair.
- String-keyed owners read JSON through `ReadOnlySpan<char>` by default (`DisableSpanBasedJsonConversion` opts out) — wire admission without intermediate string allocation, riding the same alternate-lookup frozen table as in-process lookup.
- The `SkipFactoryMethods` cascade is the proof that every boundary capability derives from admission: disabling factories strips the `TypeConverter`, `IObjectFactory`, `IParsable`/`ISpanParsable`, arithmetic operators, key→owner conversion, and all serializer converters (unless an object factory carries serialization independently). None of those surfaces are free-standing; all are factory-derived.
- Deserialization failure surfaces as the framework-native exception carrying the validation error text — the wire edge converts typed admission errors into protocol faults inside generated converters, with no user code in the path.

[SHAPE_SPAM]:
- Three or more sibling types sharing fields for one concept collapse into one closed family; the union case list replaces the type list and the analyzer's closure rules prevent regrowth.
- The nullable payload bag — one record, N nullable fields, a kind discriminator — encodes a sum as a product: every consumer re-derives the discriminant↔field correspondence the type system already knew, every read is a null gamble, and a new kind extends the bag silently. The union's exhaustive `Switch` deletes every null check and breaks every call site when a case is added — the failure mode inverts from silent to loud.
- The enum-plus-dictionary pair splits one vocabulary across two half-owners: the language enum admits undefined values by cast, the dictionary lookup is a partial function, and the two drift independently. A smart enum closes admission (`Get`/`TryGet`/`Validate` with unknown keys typed as errors) and stores the policy as constructor state or delegate rows in the same declaration — one owner, total dispatch, no drift surface.
- Decorative generated attributes on inert data: an owner whose factory validates nothing, whose comparer policy is default, and which never crosses a boundary decides nothing — the record was already correct. The inverse is equally spam: an anemic record sitting at a boundary scatters its missing validation across every call site as ad-hoc checks.
- A two-case ad-hoc union whose cases are outcome markers rather than domain payloads duplicates an existing result rail; the union form earns existence only when both cases are domain material in their own right.
- Owner-per-protocol duplication — a domain type shadowed by a JSON type, a persistence type, and a binding type for one concept — collapses into one owner plus object factories partitioned by framework; the factory partition rules make the collapse safe by construction.

[MANUAL_OWNER_EXEMPTIONS]:
- Foreign-assembly extension: every generated family is closed by analyzer law. An extension point that outside code must populate is an interface or abstract hierarchy by construction — no generated owner can model it, and forcing one produces a closed family plus an escape hatch, which is two owners.
- Generic case payloads inside a regular union are rejected (TTRESG053): either lift the type parameter to the union itself (generic ad-hoc union via `TypeParamRef`) or write the hierarchy manually with hand-rolled closure (private constructors, sealed cases) mirroring the analyzer's rules.
- Ad-hoc arity caps at five members; past it the concept either has shared semantics (regular union) or is not one concept. The cap is a hard generator bound, not a style threshold.
- Runtime-sourced vocabulary (configuration, database rows) cannot be smart-enum items — items are compile-time `static readonly` fields. The owner becomes a keyed value object plus a frozen registry admitted once at startup; the registry is the manual analogue of the generated lookup.
- Per-item behavior that needs generic type parameters cannot ride delegate rows (TTRESG051); derived item types carry it through ordinary virtual dispatch inside the closed item hierarchy.
- Ref-struct unions are declared, not parameterized: the union type itself may be a ref struct (equality generation is suppressed; `Switch`/`Map` still emit with `allows ref struct` state and results), but `allows ref struct` type parameters are rejected — span-carrying alternatives exist only as concrete declarations.
- An owner must be a class or struct (TTRESG004); a concept that must surface as an interface — for variance, for foreign implementation, for host contracts — is outside the generated families entirely and takes the manual route with the closure disciplines applied by hand.
