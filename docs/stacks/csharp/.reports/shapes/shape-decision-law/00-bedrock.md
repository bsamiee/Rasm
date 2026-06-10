# Shape Decision Law

[OWNER_SELECTION]:
- Five discriminants decide the owner before any attribute is written: admission (does raw material cross a trust boundary here), identity regime (key, structural, case, or reference), variant arity (one shape vs N alternatives), payload timing (case data fixed at declaration vs constructed per occurrence), and openness (closed vocabulary vs foreign extension). Every misplaced shape traces to one mis-answered discriminant.
- A scalar carrying an invariant, normalization, or non-default equality earns `[ValueObject<T>]`. The generated key member defaults to a private field — the provider shape is silently erased at any key-typed use site through the implicit conversion or `IConvertible<T>.ToValue()`; a public `Value` property is an opt-in via `KeyMemberAccessModifier` and `KeyMemberKind`, not the default. The implicit default makes evidence erasure frictionless — raw keys surface at assignment without a cast — which is itself a design pressure: calling code that operates on the raw key without conversion is operating on erased evidence by the type's own design, and domain semantics propagate only on the owner type. The owner publishes domain semantics, never the raw key.
- N fields forming one concept under structural identity earn `[ComplexValueObject]`. The sum-encoded-as-product test rejects it: when any field is meaningful only under some discriminant value, the concept is a union, and the complex value object is the wrong shape regardless of field count.
- A bounded vocabulary whose case data is fixed at declaration earns `[SmartEnum<TKey>]` when identity must cross a wire (key identity), or keyless `[SmartEnum]` when the vocabulary exists purely for process-local behavior selection (no serialization is generated without an object factory). Items are singletons; any per-occurrence payload disqualifies the shape and pushes to a union.
- Closed alternatives with per-occurrence payload and shared base semantics earn a regular `[Union]` class hierarchy; one value ranging over two to five unrelated existing types with no shared invariant earns an ad-hoc `[Union<T1,...>]` or `[AdHocUnion(...)]`. The regular/ad-hoc split is relatedness of cases — do the cases carry meaning outside this union — not arity or convenience.
- Interior product data with no invariant, no admission, and no identity beyond structure stays a plain `record` or `readonly record struct`. Generated attributes on such a type buy analyzer ceremony — partial split, private constructor, readonly enforcement, comparer demands — plus a factory that decides nothing.
- A wire contract earns a protocol DTO record only when its topology diverges from the domain owner (different field graph, versioned members, protocol naming). When the wire shape is one scalar, `[ObjectFactory<TWire>]` deletes the DTO entirely; the owner carries the protocol projection as a declared aspect.
- Singleton-vs-instance is the sharpest smart-enum/union discriminant: a smart enum is a fixed set of named instances dispatched by identity; a union is a fixed set of shapes dispatched by case. Vocabulary that starts sprouting constructor arguments at use sites has already become a union.
- Value object vs ad-hoc union for the same data: the presence of any invariant, normalization, or admission rule selects the value object; the ad-hoc union is structural pass-through and validates nothing.
- Every keyed owner implements the static-abstract factory contract `IObjectFactory<T, TValue, TValidationError>` — static `TValidationError? Validate(TValue? value, IFormatProvider? provider, out T? item)` — and keyed smart enums additionally implement `ISmartEnum<TKey, T, TValidationError>` with static `Items`, `Get`, and `TryGet`. One generic constraint therefore owns boundary plumbing across every owner kind: admission sweeps, cache hydration, and configuration readers are written once against the contract, and re-deciding an owner's kind later never touches the generic plumbing. Marker forms (`IObjectFactory<T>`, `ISmartEnum<TKey>`, `IKeyedObject<TKey>`) carry kind evidence for non-generic infrastructure.
- `Validate` threads `IFormatProvider?` through the canonical admission signature: culture is an explicit admission input on the owner contract, never an ambient read — parse-shaped owners receive formatting policy at the call, and generic admission code forwards it without knowing whether the key is numeric, temporal, or textual.
- Duplicate member types in an ad-hoc union are a deliberate modeling tool: the same representation under two names yields a union whose slots are semantically distinct, whose only ingress is the named factories, and whose equality discriminates the slot before the payload — equal payloads under different names are unequal values. The state a wrapper pair or a boolean tag would have smuggled in lives in the discriminator:

```csharp
[Union<string, string>(T1Name = "Draft", T2Name = "Published")]
public partial class Copy;

static bool Promoted(Copy before, Copy after) =>
    before.IsDraft && after.IsPublished && before.AsDraft == after.AsPublished;
```

[CLOSURE_LAW]:
- Union closure is analyzer-enforced, not conventional: the base must be sealed or expose only private constructors (TTRESG054); a non-abstract derived union may not be less accessible than its base (TTRESG056); derived cases must not be generic (TTRESG053); record-based unions must be sealed, so multi-level nesting forces class cases (TTRESG055). An inner type that fails to derive from the union is flagged (TTRESG106).
- Smart-enum case types invert visibility by depth: first-level derived item classes must be private (TTRESG014), deeper levels public (TTRESG015). Identity lives in the public `static readonly` item fields while the case types stay invisible — consumers can never type-dispatch on a case class, only on item identity or generated `Switch`. Static properties are not items (TTRESG101); an itemless enum is flagged (TTRESG100); an enum with no derived item types must be sealed (TTRESG037).
- A type owns exactly one shape: duplicate or mixed family attributes are rejected outright (TTRESG063/064/065/066), and an ad-hoc union requires at least two members (TTRESG067) — degenerate one-member unions cannot exist.
- Complex value object members are pushed toward `required` (TTRESG104) with `init` accessors forced private (TTRESG042) and fields/properties forced read-only (TTRESG001/003, including base-class members TTRESG034/035): the generated factory initializes through the object initializer while external bypass construction is structurally impossible.
- Case discovery is lexically scoped: regular-union cases must be declared nested inside the owner's body, so the owner's body is the case list — file or namespace organization cannot fragment a family, and a derived type declared outside simply never joins it.
- Intermediate grouping is a per-node dispatch decision: a non-sealed nested class with private constructors groups cases without becoming a dispatch surface, while placing the union attribute on the intermediate makes the generator emit it abstract with a private constructor and give it its own exhaustive dispatch — node-owned dispatch and root-level stop-at overloads are complementary granularities over the same tree.
- A struct ad-hoc union's discriminator reserves zero for uninitialized, and a `default` instance throws `InvalidOperationException` from `Switch`, `Map`, `Value`, and the typed accessors — at use, not at creation. Generated unions implement `IDisallowDefaultValue`, so expression-level default use is analyzer-flagged, but array allocation and collection growth materialize defaults invisibly: struct unions hydrated from pooled or array storage need a liveness gate, or the concept takes the class form.
- Regular-union dispatch switches on the runtime type with a throwing default arm, and the generator sorts case arms by inheritance hierarchy — refusing to generate when the hierarchy cannot be ordered. A foreign subclass admitted through a widened constructor compiles at every call site and detonates at first dispatch: the closure rules are runtime-totality guarantees, not style.
- Ad-hoc conversion operators withdraw exactly where the language forbids them — type-parameter members, interface members, `object` members, and duplicate member types — and any one trigger flips factory generation on for all members (`Create{MemberName}`), keeping ingress symmetric: no union has half conversion-ingress and half factory-ingress by accident. `FactoryMethodGeneration.Always` forces the named verbs even where conversions exist; `None` under a trigger leaves constructors as the only route. One accessibility knob governs the whole ingress surface: `ConstructorAccessModifier` sets constructors, conversion operators, and factory methods together — token-gating everything to a hand-written factory on the owner. The default postures differ by owner kind: value objects start with a private constructor (admission-first by default), unions start public and require explicit tightening — the two defaults encode their primary use: admission at boundary versus structural pass-through at interior.
- Default member naming derives from type names, so an unnamed union dispatches and projects through `@string:`, `int32:`, `IsString`, `AsInt32` — keyword-escaped, representation-leaking identifiers at every call site. Naming every member via `T1Name`/`T2Name` is therefore not cosmetic: arm names are the named-argument contract, and the rename converts representation vocabulary into domain vocabulary across all dispatch sites at once.
- Stateless members store nothing but the discriminator: parameterless factory, accessor returning `default`, equality by index alone — outcome markers ride at zero bytes beside payload members instead of as sibling sentinel types.
- Member nullability is declared at the attribute (`T1IsNullableReferenceType`, `T2IsNullableReferenceType`) because type arguments cannot carry the annotation: null becomes an admissible state of that member and its accessor returns the annotated type. Undeclared, a null reference member is rejected — absence inside a union member is an owner decision, not an accident of reference types.

[DISPATCH_FORM]:
- Four dispatch forms cover every shape decision: generated `Switch`/`Map` (the call site supplies arms; the family is closed and growth breaks every call site at compile time), case-owned delegate rows (`[UseDelegateFromConstructor]`: one partial method, N per-item implementations wired through the constructor), abstract members on a regular union's base with per-case overrides (the union analogue of delegate rows — a new case cannot compile until it answers, and no call site changes), and frozen tables (derived correspondences, legitimate only when the policy cannot live on the case). Selection: call-site-varying behavior → Switch; behavior intrinsic to the case with a uniform signature → abstract member overrides or delegate rows; behavior keyed by something the family does not own → table. With `SwitchMapMethodsGeneration.None`, case-owned members become the only consumption form and the family grows fully owner-locally.
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

- Recursive case payloads are unconstrained — a case may hold members of the union type itself — so recursive sums fold two ways on one owner without interference: case-owned recursion through overridden abstract members, and call-site catamorphism through `Switch` re-entry:

```csharp
[Union]
public abstract partial class Expr
{
    private Expr() { }

    public abstract int Depth { get; }

    public sealed class Lit : Expr
    {
        public required decimal Value { get; init; }
        public override int Depth => 1;
    }

    public sealed class Add : Expr
    {
        public required Expr Left { get; init; }
        public required Expr Right { get; init; }
        public override int Depth => 1 + int.Max(Left.Depth, Right.Depth);
    }
}

static decimal Eval(Expr expr) =>
    expr.Switch(
        lit: static l => l.Value,
        add: static a => Eval(a.Left) + Eval(a.Right));
```

- Partial coverage is an owner decision, not a call-site choice: only `SwitchMapMethodsGeneration.DefaultWithPartialOverloads` emits `SwitchPartially`/`MapPartially` with a `@default` arm. A family that must always be totally handled never generates the partial forms — non-exhaustive dispatch is made inexpressible at the type.
- `[UnionSwitchMapOverload(StopAt = ...)]` generates overloads that treat an intermediate union as a single arm without descending into its cases — dispatch granularity over a nested hierarchy is declared at the owner, and grouped handling delegates to the intermediate union's own `Switch`. An intermediate node that itself carries the union attribute generates its own `Switch`/`Map` hiding the root's (the generator suppresses the member-hiding warning deliberately), so the static type of a reference selects dispatch granularity: a branch-typed reference dispatches over that branch's cases, a root-typed reference over the root arms. Nested naming is a declared policy: the default composes intermediate type names into arm parameters (`failureNotFound`); `NestedUnionParameterNameGeneration.Simple` uses leaf names and turns cross-branch name collisions into compile errors.
- `Map` is the inline value table — one constant per case, no lambdas. Secondary correspondences (case → caption, case → cost) derive from the primary family through `Map` instead of a parallel dictionary that can drift.
- Smart-enum `Switch` dispatches through a generated per-item ordinal index, not key comparison; key lookup is a lazily built `FrozenDictionary` with a `ReadOnlySpan<char>` alternate lookup for string keys. Hand-building a key-to-item dictionary, or a span-keyed parse path, duplicates infrastructure the owner already generated.
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

- Frozen tables earn existence in exactly three places: cross-product keys (case × case correspondences no single family owns), externally sourced policy admitted at startup, and keys outside any generated family. Everything else is either `Map`, a delegate row, or an instance property. A frozen table keyed by generated vocabulary regains the closure the dictionary form lost: totality is provable at composition time by comparing table cardinality against the product of the vocabularies' item counts, converting the partial-function risk of lookup tables into a one-time startup invariant instead of a per-call gamble. Raw-primitive keys forfeit the proof — nothing enumerates their domain:

```csharp
[SmartEnum<string>]
public sealed partial class Region
{
    public static readonly Region East = new("east");
    public static readonly Region West = new("west");
}

[SmartEnum<string>]
public sealed partial class Tier
{
    public static readonly Tier Basic = new("basic");
    public static readonly Tier Premium = new("premium");
}

public static class Rates
{
    private static readonly FrozenDictionary<(Region, Tier), decimal> Table =
        new Dictionary<(Region, Tier), decimal>
        {
            [(Region.East, Tier.Basic)] = 0.10m,
            [(Region.East, Tier.Premium)] = 0.25m,
            [(Region.West, Tier.Basic)] = 0.12m,
            [(Region.West, Tier.Premium)] = 0.28m,
        }.ToFrozenDictionary() switch
        {
            { Count: var n } table when n == Region.Items.Count * Tier.Items.Count => table,
            _ => throw new InvalidOperationException("<rate-table-incomplete>"),
        };

    public static decimal Of(Region region, Tier tier) => Table[(region, tier)];
}
```

- Cross-product keys ride value tuples for positional pairs and graduate to a readonly record struct once slots deserve names; both hash structurally through the items' generated equality, so vocabulary values are table-key-safe by construction and never need a registered comparer. Frozen construction specializes the implementation to the key population at build time and is paid once at static initialization — the read-optimized trade is the declared reason a policy table is a static singleton and never rebuilt per scope; string-keyed tables additionally expose span-keyed alternate lookup, deleting the allocation on parse-shaped probe paths.
- `MapPartially` takes a required `@default` first and each case as an optional `Argument<TResult> name = default`; the implicit `T -> Argument<T>` conversion marks any explicitly passed value as set, so an arm explicitly mapped to `null` or to the result type's default still counts as handled rather than falling to `@default`. The wrapper solves the omitted-versus-default ambiguity that optional parameters cannot express, without an overload per subset of cases.
- `SwitchPartially` in action form declares `@default` as a nullable delegate defaulting to null: omitting it makes every unhandled case a silent no-op. Selection: value-form partial dispatch is total-with-fallback by construction (the default is the one non-optional parameter); action-form partial dispatch is reserved for surfaces where no-op is the documented semantics, because a dropped case there is indistinguishable from success.
- Heterogeneous arm returns break `TResult` inference and flag the entire dispatch call rather than the offending arm; pinning the type argument (`Switch<TAbstraction>(...)`) restores per-arm diagnostics and is mandatory whenever arms return siblings under an abstraction.
- `SwitchMapMethodsGeneration.None` deletes the dispatch surface from an owner whose consumers must not enumerate cases — transport-only carriers interpreted elsewhere — making arm-style consumption inexpressible rather than discouraged. An ad-hoc union whose members are call modalities earns this setting when the carrier is queued, logged, or replayed: the modality becomes a stored value with capabilities an overload set structurally lacks, because the modality dies at overload resolution.
- Native `switch` over public union cases inverts the exhaustiveness direction: generated dispatch breaks every call site when a case is added, while a discard arm absorbs the new case silently — the nullable-payload-bag failure mode re-imported into a sound type. Smart-enum items close the other door: items are static readonly instances, not constants, so they cannot appear as case labels at all — identity dispatch is inexpressible in language switch and exists only as generated dispatch or delegate rows.
- Items enumeration order is declaration order; key order is comparer order. The two are distinct axes — presentation sequence rides declaration, semantic rank rides the key — and conflating them couples display to identity. Range dispatch over an ordered vocabulary is total over future items where an arm-per-item dispatch re-opens at every addition: threshold semantics belong to the key, case semantics to the arms.

[IDENTITY_AND_COMPARERS]:
- Identity has three regimes and the owner choice fixes the regime: key identity (keyed value objects and smart enums — equality, hash, ordering all delegate to the key through a declared comparer), structural identity (complex value objects and ad-hoc unions — member-wise with comparer policy), and case identity (regular unions — the generator emits no `Equals`/`GetHashCode` for the hierarchy; record cases get value equality, class cases reference equality, and a mixed hierarchy silently mixes regimes per case).
- `OrdinalIgnoreCase` is the pervasive silent default: `DefaultStringComparison` on both the complex value object attribute and the union attribute base defaults to it — two values differing only in case are equal and collide in hash containers. The analyzers refuse to leave string identity implicit on value objects (TTRESG048/049); ordinal-sensitive domains must declare `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]` or set `DefaultStringComparison = StringComparison.Ordinal` at the owner.
- Comparer policy is typeclass-shaped: `IEqualityComparerAccessor<T>` and `IComparerAccessor<T>` expose static comparer properties, with `ComparerAccessors.*` prebuilt and the comparer type checked against the member type (TTRESG041). Equality and ordering comparers must be declared as a pair (TTRESG102/103) or hashing and sorting disagree on identity; `ComparisonOperators` coerces `EqualityComparisonOperators` upward because ordering presupposes equality. The inverse stance — identity without order — is declared by skipping comparability with operators off and is then enforced at every sort and range expression; no configuration expresses ordered-but-not-equatable.
- `OperatorsGeneration.DefaultWithKeyTypeOverloads` adds operator overloads against the raw key type — heterogeneous comparison and arithmetic (`amount + 42m`) without converting either side, keeping the owner in play at use sites that would otherwise cast out. A vocabulary with intrinsic order earns generated comparison operators only when its key type itself carries the operator set: numeric-keyed rank ladders get `<`/`>=` plus heterogeneous key-typed overloads, while string-coded vocabularies can never reach operator form — the axis declaration is silently inert over a key without the operators. Key choice therefore decides whether order is operator-expressible: a vocabulary whose consumers dispatch by range needs the numeric key even when a string code exists, and the code becomes a column, not the key.
- `[IgnoreMember]` removes a member from generated equality and factory synthesis with a caller-guaranteed immutability obligation; `SkipEqualityComparison` hands identity entirely to the host for entity semantics or interned instances. Container-level identity ships ready-made: `StringKeyedObjectComparer<T>` for string-keyed owners in sets and dictionaries, `ProjectionEqualityComparer<T,TKey>` for projection-keyed collections.
- Generated equality for ad-hoc unions runs reference-equality short-circuit, then discriminator inequality short-circuit, then a single-member comparison through the default equality comparer or the declared string comparison baked into the method body; `GetHashCode` and `ToString` switch on the discriminator with throwing default arms — every runtime surface of the union is discriminator-rooted.

[DEFAULT_VALUE_LATTICE]:
- `IDisallowDefaultValue` marks owners whose `default` is not a value; the analyzer flags default-initialized variables (TTRESG047), and because an owner can never be a compile-time constant, optional parameters of owner type are inexpressible — absence enters as an option-typed parameter or an overload, never `= default`.
- Default-safety composes transitively through the member closure rather than being declared locally: a struct value object with a reference-type key must not allow defaults (TTRESG057), and a complex struct whose members disallow defaults must not allow them either (TTRESG058). One default-hostile member poisons the entire product, by analyzer.
- A struct that legitimately has a default opts in with `AllowDefaultStructs = true` and names it via `DefaultInstancePropertyName` — a canonical named instance replaces an anonymous bit pattern, and the name documents why default is legal.
- Struct/class selection on value objects is an absence-semantics decision: a class admits null at the factory edge with `NullInFactoryMethodsYieldsNull` or `EmptyStringInFactoryMethodsYieldsNull` absorbing degenerate input into null instead of an error; a struct trades away absence for layout and inherits the default lattice above.

[GENERATION_MECHANICS]:
- Each owner kind carries a fixed compile-break signature, and owner selection is selection of where change detonates: a new union case lands at every exhaustive dispatch call site; a new smart-enum item with delegate rows lands at the item declaration itself, because the generated constructor demands every behavior column before the item compiles; a new complex-value-object member lands at every factory call; a key-type migration on a keyed owner lands only at conversion, serialization, and persistence seams, because interior consumers hold the owner and never the key — key encapsulation converts a pervasive break into a boundary break. Growth absorption is tunable to totality: a vocabulary whose behavior lives entirely in delegate rows and whose `Switch`/`Map` generation is set to `None` absorbs item growth with zero consumer breaks — the owner is the only file that changes — while the same vocabulary with generated dispatch pushes every addition outward to all call sites. Arm names and the dispatch state-parameter name are source contract, not style: arms bind by named argument, so renaming a case — or changing the owner-configured state parameter name (`SwitchMapStateParameterName`, default `state`) — is a deliberate source-breaking act with the same gravity as deleting a method.
- Invariant tightening on a value object is the one change with zero compile signal: the factory narrows silently, and values persisted under the old invariant now fail rehydration. The validation-bypassing rehydration constructor exists exactly for this seam — stored data outlives invariant drift — so an invariant change is a data-migration decision with a code rider, and the proof obligation moves entirely to tests because the type system stays quiet.
- The partial split is a reading contract: the user-declared partial holds only domain members; factories, equality, conversions, parsing, and serializer wiring land in the generated partial. Primary constructors are rejected (TTRESG043) because the generator owns the private constructor — case and item construction routes are never user-improvisable.
- The construction advice chain is a typed pipeline: `static partial TReturn? ValidateFactoryArguments(ref TError? validationError, ref TMember ...)` normalizes arguments in place through `ref` and may declare any return type; a non-void return switches the generated declaration to `private` and forwards the returned value into `partial void FactoryPostInit(TReturn? factoryArgumentsValidationError)`, invoked on the freshly constructed instance — evidence computed once during static validation arrives at the instance after construction, and unimplemented partial-void calls compile away to nothing:

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
- Factory verbs come as a fixed triple — throwing `Create`, boolean `TryCreate`, error-returning `Validate` — renameable via `CreateFactoryMethodName` and `TryCreateFactoryMethodName` when the domain verb is not "create" (`Parse`/`TryParse` aligns the owner with the BCL parsing idiom).
- The validation error type is a definition-time design axis: `[ValidationError<TError>]` with the static `TError Create(string)` contract substitutes a domain error type into every generated factory, parser, and converter — error vocabulary is declared on the owner, not mapped at call sites.
- `TypeParamRef1`..`TypeParamRef5` compute generic owners from one declaration: attribute positions bind to the declaring type's own parameters across `[SmartEnum<TypeParamRef1>]`, `[ValueObject<TypeParamRef1>]`, and `[Union<TypeParamRef1, ...>]`. Parameters used as keys require `notnull` (TTRESG074), unreferenced parameters are flagged (TTRESG107), and `allows ref struct` parameters are rejected on ad-hoc unions (TTRESG073).
- Conversion operator defaults encode evidence direction: owner→key is implicit (erasing evidence is free), key→owner is explicit (creating evidence validates), and throw-capable owner→key conversions are segregated under `UnsafeConversionToKeyMemberType`. Ad-hoc unions mirror this: value→union implicit (`ConversionFromValue = Implicit`), union→value explicit (`ConversionToValue = Explicit`).
- Regular unions generate value→case conversion operators only when a non-abstract, non-interface case without required members has a single-argument constructor whose parameter type is unique across the family — argument-type routing exists exactly where it is unambiguous and silently withdraws when two cases would compete.
- Ad-hoc union layout is a declared memory decision: per-member fields by default (struct members inline, union size approaches the sum), `UseSingleBackingField` boxes everything into one object field (pointer-sized, allocation per struct construction), and per-member `T1IsStateless`/`T2IsStateless` (through `T5IsStateless`) stores only the discriminator for marker cases whose accessors return `default`. The backing-field lattice has three tiers, not two: with at most one reference member every member gets a typed field; with two or more reference members the generator merges the references into one object-typed field automatically while value members keep typed fields (no boxing); `UseSingleBackingField` only adds the value members into the shared field at boxing cost — the knob's real subject is value types.
- The generator is observable through MSBuild build properties (log file path, log level, invocation counter) — generation drift is diagnosable from build output without decompiling generated sources.
- Arithmetic generation emits implementations of the numeric operator interfaces (`IAdditionOperators<T,T,T>` and siblings) including `operator checked` variants, so owners satisfy generic-math constraints directly; generation per axis is conditional on the key type itself implementing the interface or exposing the corresponding operator method — an axis declared on a non-arithmetic key is silently inert, making a no-effect axis declaration a latent lie worth deleting.
- Per-axis operator selection encodes dimensional analysis: generated products are homogeneous (`T * T -> T`) or key-typed (`T * TKey -> T`), and a like-quantity product changes dimension, so quantity-shaped owners enable addition/subtraction axes and hold multiply/divide at `None`, hand-writing only the cross-dimension operators the algebra actually has. Equality and comparison operator generation likewise declares the corresponding generic-math equality/comparison interfaces, with key-type overloads adding the heterogeneous implementations:

```csharp
[ValueObject<decimal>(
    AdditionOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    SubtractionOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    MultiplyOperators = OperatorsGeneration.None,
    DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Grams;

static TOwner Net<TOwner>(TOwner seed, IEnumerable<TOwner> deltas)
    where TOwner : IAdditionOperators<TOwner, TOwner, TOwner> =>
    deltas.Aggregate(seed, static (acc, next) => acc + next);

static IReadOnlyList<TOwner> Admitted<TOwner>(IEnumerable<string> raw, IFormatProvider? culture)
    where TOwner : IObjectFactory<TOwner, string, ValidationError> =>
    [.. raw.Select(value => (Error: TOwner.Validate(value, culture, out var owner), Owner: owner))
           .Where(static slot => slot.Error is null)
           .Select(static slot => slot.Owner!)];
```

- Interface synthesis is capability-conditional, which makes capability subtraction meaningful: `IParsable<T>` appears only when the key is `string` or itself parsable (and skipping it forcibly skips `ISpanParsable<T>` because the latter inherits the former); `IFormattable` appears only over a formattable key; `IComparable<T>` appears when the key compares — or when a key comparer accessor is declared over a non-comparable key, so ordering is grantable by declaration, not only inherited from the key.
- Skip flags are semantic statements, not omissions: a surrogate identifier holds `SkipIComparable = true` with comparison operators at `None` because its ordering is meaningless — the stance is declared on the owner and then enforced by the type system. `SkipEqualityComparison` forces both operator families to `None` downward — no configuration can express operators without identity.
- The throwing factory verb raises `System.ComponentModel.DataAnnotations.ValidationException` carrying only `validationError.ToString()`: the throw edge is framework-typed and message-shaped, while the typed error object travels exclusively on the `Validate` route. A boundary that needs the typed error must route `Validate`; choosing the throwing verb there silently flattens a structured error vocabulary to a string. One `Validate` implementation fans out into four framework-native fault shapes: the throwing factory wraps the error text in `ValidationException`, parse-interface routes wrap it in a format exception, the generated JSON converter wraps it in the JSON exception, and the throwing vocabulary lookup raises its own unknown-identifier exception carrying the enum type and offending value as data rather than flattened text. A boundary that catches by exception type around heterogeneous owners must enumerate all four or leak one route.
- Custom validation-error types are constrained by the message-only funnel: the static creation contract (`IValidationError<T>.Create(string)`) takes a bare string, and every framework-origin failure — parse, deserialization, conversion — arrives through it. Structured error payloads survive only on routes the owner itself populates inside factory validation, so an error vocabulary with mandatory structured fields lies on framework routes; every custom error type needs a designed degenerate text case, and code consuming the structured fields must treat them as optional evidence.
- `SkipKeyMember = true` with `KeyMemberName` hands the key declaration to the author — a field, or a property whose backing field is set through an `init` accessor — while factories, equality, conversions, and serializer wiring continue to generate against it. The pressure that earns it: attribute placement, documentation, or layout control on the key member itself, with zero loss of generated capability.
- The owner taxonomy itself is reified as a closed family — keyed smart enum, keyed value object, complex value object, keyless smart enum, ad-hoc union, regular union — with its own exhaustive state-threaded `Switch`/`Map`, resolvable from a runtime type via a lookup. Infrastructure that must discriminate owner kind (mappers, codec registries, schema emitters) rides that exhaustive dispatch instead of bespoke attribute reflection, inheriting compile breaks when the taxonomy grows.
- Keyed metadata carries the key projection both as a compiled delegate and as a lambda expression tree, so expression-translating consumers compose key access without the owner exposing a public key member — private-key owners stay query-translatable. Factory metadata exposes the per-framework route partition (serialization, persistence, binding) at runtime, and a static-abstract invoker bridges from runtime types to the static contracts for fully dynamic edges.
- The metadata namespace is declared unstable across releases: metadata dispatch belongs to infrastructure adapters only — domain code already holds the typed owner and never needs the mirror.

[SERIALIZATION_SEPARATION]:
- `SerializationFrameworks` defaults to `All`: a keyed owner is wire-capable through its key representation across JSON and binary stacks by default. The separation decision is therefore explicit in both directions — `None` severs codec from the owner when a protocol DTO owns the wire; leaving the default makes the key representation the contract and deletes the DTO.
- `[ObjectFactory<TWire>]` redirects the wire shape per concern: static `TError? Validate(TWire value, IFormatProvider? provider, out T item)` admits, instance `TWire ToValue()` projects, and `UseForSerialization` partitions frameworks among factories with overlap rejected (TTRESG070). Persistence and binding routes are singular (TTRESG068/069). `HasCorrespondingConstructor = true` declares a validation-bypassing rehydration route through a single-argument constructor (TTRESG059) and is forbidden on smart enums (TTRESG060) — rehydration of a vocabulary must resolve to existing items, never construct new ones.
- Keyless smart enums serialize only through an object factory: vocabulary identity without wire identity is the default posture, and wire exposure is a declared exception rather than an ambient capability.
- A complex value object plus `[ObjectFactory<string>]` collapses a structural concept to a parse/format micro-grammar — one textual representation owned by the type instead of a field-wise DTO plus mapper pair.
- String-keyed smart enums read JSON through `ReadOnlySpan<char>` by default (`DisableSpanBasedJsonConversion` on `SmartEnumAttribute` opts out) — wire admission without intermediate string allocation, riding the same alternate-lookup frozen table as in-process lookup.
- The `SkipFactoryMethods` cascade proves that every boundary capability derives from admission: disabling factories strips the `TypeConverter`, the `IObjectFactory` marker interface, `IParsable`/`ISpanParsable`, arithmetic operators, key→owner conversion, and all serializer converters (unless an object factory carries serialization independently). None of those surfaces are free-standing; all are factory-derived.
- Deserialization failure surfaces as the framework-native exception carrying the validation error text — the wire edge converts typed admission errors into protocol faults inside generated converters, with no user code in the path.
- Neither union flavor is wire-capable alone: no type discriminator is emitted, so polymorphic serialization does not exist by construction, and the wire route is an object factory collapsing the union to one primitive representation or a protocol DTO at the edge. The design consequence runs backward into owner selection: a closed alternative set whose cases must round-trip a boundary either admits a single-scalar grammar (union plus object factory), re-shapes as a keyed owner where the key is the discriminator, or accepts an edge DTO — a union whose cases cannot fold into one grammar is interior-only by construction.
- Generated codecs attach as type-level converter attributes pointing at either a shared runtime converter factory or an owner-specific file-local converter class — codec machinery adds zero public API surface, and the file-local form is unreachable even within the same assembly. Presentation policy stays host-owned even through generated converters: the structural-owner converter consults the serializer's default-ignore condition for null and default member emission and resolves per-member custom converters from serializer options at runtime — a structural owner's wire object can therefore differ per host configuration while its admitted shape cannot. The split is exact: evidence (which members exist, which values are legal) is decided at the owner; emission (which members appear, how values render) is decided at the edge — any pressure to push emission policy onto the owner is the serialization-separation violation in its subtlest form.

[SHAPE_SPAM]:
- Three or more sibling types sharing fields for one concept collapse into one closed family; the union case list replaces the type list and the analyzer's closure rules prevent regrowth.
- The nullable payload bag — one record, N nullable fields, a kind discriminator — encodes a sum as a product: every consumer re-derives the discriminant–field correspondence the type system already knew, every read is a null gamble, and a new kind extends the bag silently. The union's exhaustive `Switch` deletes every null check and breaks every call site when a case is added — the failure mode inverts from silent to loud.
- The enum-plus-dictionary pair splits one vocabulary across two half-owners: the language enum admits undefined values by cast, the dictionary lookup is a partial function, and the two drift independently. A smart enum closes admission (`Get`/`TryGet`/`Validate` with unknown keys typed as errors) and stores the policy as constructor state or delegate rows in the same declaration — one owner, total dispatch, no drift surface.
- A generated owner that validates nothing, applies default comparer policy, and never crosses a boundary decides nothing — the record was already correct. The inverse is equally spam: an anemic record sitting at a boundary scatters its missing validation across every call site as ad-hoc checks.
- A two-case ad-hoc union whose cases are outcome markers rather than domain payloads duplicates an existing result rail; the union form earns existence only when both cases are domain material in their own right.
- Owner-per-protocol duplication — a domain type shadowed by a JSON type, a persistence type, and a binding type for one concept — collapses into one owner plus object factories partitioned by framework; the factory partition rules make the collapse safe by construction.

[COMBINABLE_VOCABULARY]:
- Combinable capability sets split by where per-flag behavior lives: a flags enum has nowhere to put it, so flag-test chains re-derive policy at every consumer, while vocabulary items held in a frozen or immutable set carry behavior as columns and fold — membership is set algebra, policy is a fold over member items. The flags form survives only at bit-level interop seams where a foreign ABI owns the bit layout.
- A language enum remains the correct owner in exactly three places, all boundaries: mirroring a wire schema a foreign protocol owns (converted to vocabulary at admission), bit-flag interop with host or native APIs, and ordinal array indexing inside measured kernels. Its disqualifying property is open admission — any integral cast produces an undefined value — so an enum that travels past the boundary smuggles unvalidated input inward; conversion to the closed vocabulary at the admission seam is the re-closing move.

[MANUAL_OWNER_EXEMPTIONS]:
- Foreign-assembly extension: every generated family is closed by analyzer law. An extension point that outside code must populate is an interface or abstract hierarchy by construction — no generated owner can model it, and forcing one produces a closed family plus an escape hatch, which is two owners.
- Generic case payloads inside a regular union are rejected (TTRESG053): either lift the type parameter to the union itself (generic ad-hoc union via `TypeParamRef`) or write the hierarchy manually with hand-rolled closure (private constructors, sealed cases) mirroring the analyzer's rules.
- Ad-hoc arity caps at five members; past it the concept either has shared semantics (regular union) or is not one concept. The cap is a hard generator bound, not a style threshold.
- Runtime-sourced vocabulary (configuration, database rows) cannot be smart-enum items — items are compile-time `static readonly` fields. The owner becomes a keyed value object plus a frozen registry admitted once at startup; the registry is the manual analogue of the generated lookup.
- Per-item behavior that needs generic type parameters cannot ride delegate rows (TTRESG051); derived item types carry it through ordinary virtual dispatch inside the closed item hierarchy.
- Ref-struct unions are declared, not parameterized: the union type itself may be a ref struct (equality generation is suppressed; `Switch`/`Map` still emit with `allows ref struct` state and results), but `allows ref struct` type parameters are rejected — span-carrying alternatives exist only as concrete declarations.
- An owner must be a class or struct (TTRESG004); a concept that must surface as an interface — for variance, for foreign implementation, for host contracts — is outside the generated families entirely and takes the manual route with the closure disciplines applied by hand.
- Smart enums may extend a foreign base class; the generated constructor threads key first, own instance properties second, base-class constructor parameters last — host-mandated base types are not a manual-owner pressure.
- One private generic case class closed at several type arguments yields type-indexed items: distinct items carry distinct compile-time type payloads while the vocabulary itself stays non-generic — the route for per-item type evidence (handler types, payload schemas) without genericizing the enum or storing runtime type objects.
- Vocabulary graphs — items referencing sibling items, as in transition tables — take lazily evaluated list-valued constructor columns: eager sibling references resolve to null for items not yet initialized in declaration order, and the lazy column defers resolution to first read. Edges between items are a column pressure, not a union pressure.
- Owners nest anywhere, but every enclosing type must itself be partial — a non-partial container is a generation-blocking compile error, not a degraded mode.
- Generated families fix an expression-problem stance: the case axis is closed by analyzer law while the operation axis stays open — extension members attach receiver-owned operations to a closed owner from outside, dispatching internally over the generated exhaustive forms. Foreign code that needs new operations over a family is served without touching the owner; only foreign code that must add new cases justifies leaving generated owners for an interface hierarchy. The manual-owner exemption is therefore narrower than "extension point implies manual owner": it triggers on case extension alone, never on operation extension.
- Re-validation cost makes churn rate a placement discriminant: a value rewritten per frame or per solver iteration does not belong in a validated owner — accumulation lives in a plain record or struct, and admission happens once at the seam where the result becomes domain material. Admitting inside a hot loop pays the invariant on every step for evidence nothing consumes mid-loop; admitting never leaves the seam unguarded. The decision variable is reads-of-evidence per write, not type aesthetics. Generated owners delete the external update surface structurally — constructors private, `init` accessors forced private, members forced required and read-only — so object-initializer construction, member rebinding, and `with`-style cloning are inexpressible outside the owner, even on struct owners where the language otherwise grants `with` to every struct: every derived state is a new admission through a factory.
