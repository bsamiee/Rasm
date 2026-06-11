# [CSHARP_SHAPES]

Every concept takes exactly one owner, and five discriminants select it before any attribute is written: admission (raw material crossing a trust boundary), identity regime (key, structural, case, or reference), variant arity (one shape or N alternatives), payload timing (case data fixed at declaration or constructed per occurrence), and openness (closed vocabulary or foreign extension). The selection fixes where change detonates, what equality means, and which capabilities derive — every misplaced shape traces to one mis-answered discriminant.

## [1]-[OWNER_CHOOSER]

When a concept matches several signatures, the most specific row wins.

| [INDEX] | [CONCEPT_SIGNATURE]                            | [OWNER]                          | [IDENTITY]      |
| :-----: | :--------------------------------------------- | :------------------------------- | :-------------- |
|   [1]   | invariant-bearing scalar                       | `[ValueObject<TKey>]`            | key             |
|   [2]   | N fields, one concept, no discriminator        | `[ComplexValueObject]`           | structural      |
|   [3]   | bounded vocabulary, wire-keyed identity        | `[SmartEnum<TKey>]`              | key             |
|   [4]   | bounded vocabulary, process-local behavior     | `[SmartEnum]` keyless            | reference       |
|   [5]   | closed alternatives, per-occurrence payload    | `[Union]`                        | case            |
|   [6]   | one value over 2–5 unrelated types             | `[Union<T1,...>]` ad-hoc         | slot then value |
|   [7]   | interior product, no invariant, no admission   | record or readonly record struct | structural      |
|   [8]   | combinable capability set                      | vocabulary items in a frozen set | key             |
|   [9]   | runtime-sourced vocabulary                     | keyed owner plus frozen registry | key             |
|  [10]   | cross-product or externally sourced policy key | frozen table                     | composite key   |
|  [11]   | foreign wire enum, ABI bits, or kernel ordinal | language enum at the seam only   | ordinal         |
|  [12]   | foreign code must add cases                    | manual interface or hierarchy    | declared        |

## [2]-[DECISION_LAW]

[OWNER_SELECTION]:
- `SelectOwner(concept)`: choose by singleton-versus-instance, field coverage, relatedness, invariant, admission boundary, churn rate, payload timing, and openness.
- `UseSmartEnum(vocabulary)`: fixed named instances, identity dispatch, behavior columns, key lookup, and declared growth absorption.
- `UseUnion(family)`: per-occurrence payload, case dispatch, stored call modality, transport carrier under `SwitchMapMethodsGeneration.None`, and ad-hoc members meaningful outside the family.
- `UseComplexValueObject(product)`: require every field under every value; move discriminator-dependent fields to `UseUnion`; keep non-admitting, no-invariant, default-comparer products as `record`.

[COLLAPSE_FUNCTIONS]:
- `CollapseFamily(family)`: keep generated closure; delete sibling regrowth, nullable payload bags, enum-dictionary pairs, protocol shadows, owner wrappers, and overload-only modality.
- `MergeSamePayload(cases)`: collapse only passive, non-generic, non-fault cases with identical semantics; preserve marker, behavior, fault, and named-semantic cases.
- `ReplaceFlags(capability)`: model combinable capability as vocabulary items in a frozen set; keep behavior in columns, membership as set algebra, and policy as a fold.
- `UseLanguageEnum(seam)`: permit only foreign wire enum, ABI bit layout, or measured-kernel ordinal; re-close at seam conversion.

[BEHAVIOR_FUNCTIONS]:
- `PlaceBehavior(selector)`: use generated dispatch for call-site variation, columns or case members for family-owned uniform behavior, and frozen tables for cross-products, startup-admitted external policy, or outside-family keys.
- `RejectExternalDictionary(key)`: move item-keyed dictionaries into columns, dispatch, or total startup-checked tables; fold repeated full-coverage identical dispatch to a column.

[CHANGE_FUNCTIONS]:
- `PlaceGrowthCost(owner)`: send new union cases to exhaustive `Switch`, smart-enum items to constructors, complex members to factories, and key migrations to conversion and wire seams.
- `TuneGrowthAbsorption(owner)`: let columns under `SwitchMapMethodsGeneration.None` absorb item growth; let generated `Switch` push additions to consumers.
- `TightenInvariant(owner)`: treat narrower factories as data migration plus tests because compile signal is zero.

[EXEMPTION_FUNCTIONS]:
- `UseManualFamily(axis)`: hand-roll only for foreign case extension, interface-required owners, unliftable generic payloads, ad-hoc arity past five, or runtime-sourced vocabulary admitted into keyed owner plus frozen registry.
- `ExtendForeignBase(vocabulary)`: keep generated vocabulary; thread key first and base parameters last; reject host base class as manual case-extension pressure.

[COMPOSITION_FUNCTIONS]:
- `ComposeOwners(graph)`: nest owners without revalidation; preserve admission, equality, comparer policy, default-safety, and totality; project wire shape through one object factory instead of flattening.
- `StackIdentity(owner)`: keep structural, key, case, equality, and ordering regimes local; declare identity-without-order at the layer that owns it.
- `KeyByGeneratedOwner(owner)`: use full generated-owner keying only across assembly boundaries; in same-compilation composition, carry raw key plus vocabulary owner and one two-hop admission expression.

## [3]-[VALUE_OBJECTS]

[ADMISSION_FACTORY]:
- Use: `Validate` as the admission factory; `Create` throws flattened fault text, `TryCreate` downgrades evidence, and culture-sensitive admission stays on `Validate`.
- Law: `ValidateFactoryArguments` canonicalizes by `ref` before storage and owns fresh-input rejection; `ValidateConstructorArguments` runs on every construction path, including rehydration, and owns invariant-of-record drift.
- Law: non-`void` hook returns carry admission evidence into `FactoryPostInit` only on genuine admission; rehydrated material must re-derive evidence-backed state.
- Reject: per-call-site error translation; static `Create(string)` receives rendered text only, raw-value evidence captures at the bridge, and the dual-contract fault family deletes translation hops.

[KEY_AND_IDENTITY_POLICY]:
- Law: the raw key stays private except conversion and explicit-interface egress; consumers compare and dispatch on the owner, and key-type migration breaks at the boundary.
- Law: comparer policy is a type argument; `IEqualityComparerAccessor<T>` and `IComparerAccessor<T>` swing equality, hashing, ordering, relational operators, and `CompareTo` together, with comparers cached in `static readonly` fields.
- Law: string keys default to ordinal-ignore-case across generated surfaces but never inherit policy; every string-bearing layer declares one accessor type, and divergence is the defect.
- Accept: complex-owner equality membership as opt-in member comparers; unmarked members vanish from equality, hashing, and diagnostic text, while collection members use reference identity unless a sequence comparer is attached.

```csharp conceptual
public sealed class FieldKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.OrdinalIgnoreCase;

    public static IEqualityComparer<string> EqualityComparer => Policy;

    public static IComparer<string> Comparer => Policy;
}

[ValueObject<string>]
[KeyMemberEqualityComparer<FieldKeyPolicy, string>]
[KeyMemberComparer<FieldKeyPolicy, string>]
public readonly partial struct FieldKey;

public static class FieldRanks {
    public static ImmutableSortedDictionary<FieldKey, int> From(IEnumerable<FieldKey> keys) =>
        keys.Distinct().Order().Select(static (key, rank) => (key, rank))
            .ToImmutableSortedDictionary(static row => row.key, static row => row.rank);
}
```

[OPERATOR_ALGEBRA]:
- Law: operator axes are algebra grants; enabled axes declare closure, generated operators stay homogeneous, and cross-dimension operations are hand-written against the foreign result type.
- Law: operator bodies re-enter admission through the throwing factory; `checked` adds key-math overflow trapping, so the call-site context selects the failure species.
- Law: no identity element is synthesized; seeds, zeros, bounds, and units are admitted values, `INumber<T>` kernels never see owners, and exact binary operator interfaces are the owner-compatible constraint form.
- Law: comparison grants are monotone; ordering accessors can synthesize comparison past key algebra, equality generation coerces upward, and impossible key axes emit nothing.
- Boundary: key-typed overloads accept unadmitted raw operands under owner comparer policy; narrow integral keys compute wide, narrow, and re-admit unchecked, while `checked` throws before narrowing.

```csharp conceptual
[ValueObject<double>(
    MultiplyOperators = OperatorsGeneration.None,
    DivisionOperators = OperatorsGeneration.None,
    ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
public readonly partial struct Extent {
    public static Area operator *(Extent left, Extent right) => Area.Create((double)left * (double)right);
}

[ValueObject<double>(MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Area;

public static class Measures {
    public static TQuantity Total<TQuantity>(IEnumerable<TQuantity> parts, TQuantity seed)
        where TQuantity : IAdditionOperators<TQuantity, TQuantity, TQuantity> =>
        parts.Aggregate(seed, static (sum, next) => sum + next);

    public static bool Clears(Extent extent, double bound) => extent > bound;
}
```

[ABSENCE_AND_DEFAULT]:
- Law: null-yield modes turn blank input into success-with-null; only `EmptyStringInFactoryMethodsYieldsNull` removes `NotNullWhen`, so bridges audit generated attributes and project `Option<T>`.
- Law: struct owners reject `default`, but arrays, unconstrained generic `default`, and field zero-init still mint ghosts; one outer storage seam reads the key member and rejects them.
- Law: owner-typed optional parameters are impossible because owners are never compile-time constants; absence enters as `Option<T>` or an overload, never `= default`.
- Boundary: default-hostility is transitive, `IDisallowDefaultValue` overrides `AllowDefaultStructs`, legitimate struct defaults are named canonical instances, and class-vs-struct choice selects named null policy versus layout poison.

[INGRESS_AND_EGRESS]:
- Law: conversion direction follows the trust gradient; owner-to-key conversion is implicit evidence erasure, key-to-owner conversion is explicit admission, and aggregate reconstruction climbs through explicit admissions.
- Boundary: key-to-owner casts are hidden throwing admission; keep them at boundaries, remove them with `ConversionFromKeyMemberType` or factory skipping, and account for class reference-key null propagation before factory admission.
- Law: class-owner null contracts are operator-family local; equality tolerates null, comparison and arithmetic throw, unsafe egress faults, and no family lends null-safety to another.
- Law: one accessibility token gates constructors, conversions, and factories, with domain factory verbs on the owner; `SkipFactoryMethods` removes constructing surfaces and leaves only equality, comparison, and egress over already-admitted values.

## [4]-[SMART_ENUMS]

[VOCABULARY_DECLARATION]:
- Law: the declaration list is the vocabulary; `public static readonly` fields fix item membership, dispatch indices, callback order, and metadata identifiers, while static properties and case-typed fields vanish from `Items` and dispatch at warning severity.
- Law: keyed vocabularies carry two independent orders: `Items` by declaration and comparison by key comparer; one key policy swings lookup, hash, comparison, and operators.
- Law: domain rank is an item column, never a bent comparer; range dispatch needs numeric keys to keep future thresholds total, and later-row references defer behind delegates because initialization order can capture null before materialization protects it.
- Accept: keyless vocabularies only for behavior rows: items, dispatch, reference identity, no lookup, no conversions, no parsing; wire identity requires declared `[ObjectFactory<TValue>]`.

[LOOKUP_LIFECYCLE]:
- Law: validity belongs to keys, never instances; no invalid item is constructible, callers choose `Get`, `TryGet`, or `Validate`, and exception catching is the wrong verb.
- Law: startup probes force the publication-locked lazy, assign indices, build the frozen lookup, fail-fast duplicate keys, and cache one poisoning across metadata consumers walking `Items`.
- Law: derived indexes project from `Items` through accessors, never eager static initializers; the accessor read supplies the materialization edge, and string-keyed vocabularies use the zero-allocation span alternate lookup.
- Boundary: items are static per load context and generic instantiation; each closed type argument has its own materialization and poisoning, and values cross isolation seams as keys re-admitted on the far side.

[DISPATCH_AND_ROWS]:
- Law: `Switch` is the total dispatch surface: integer jump table, write-once index cell, one hot-path field read, state-threaded forms with span-shaped state and results, and `static` lambdas.
- Law: totality is method arity; new items add parameters to total overloads, stale exhaustive dispatch fails to compile, and named callback arguments are the reorder shield.
- Reject: language `switch`, key-pattern probes, and guard chains; items are singletons, not constants, and only generated `Switch` is total.
- Law: behavior rows ladder through `[UseDelegateFromConstructor]` columns, generated dispatch, then abstract members with private nested derived cases; constructor arity forces columns, and shared inputs collapse into composite delegates.
- Boundary: partial dispatch is a presence test, not routing; `SwitchPartially` cannot distinguish omitted and null arms, omitted `@default` is no-op, `MapPartially` preserves legal defaults, and 1,000 items retain lookup but lose dispatch.

```csharp conceptual
[SmartEnum<string>]
public sealed partial class Gait {
    public static readonly Gait Walk = new("walk", stride: 1, StepWalk);
    public static readonly Gait Sprint = new("sprint", stride: 3, StepSprint);

    public int Stride { get; }

    [UseDelegateFromConstructor]
    public partial (double Cost, double Reach) Step(double load);

    private static (double, double) StepWalk(double load) => (load, load);
    private static (double, double) StepSprint(double load) => (load * 3d, load * 9d);
}

public static class Pacing {
    public static double Strain(Gait gait, double load) => gait.Step(load).Cost / gait.Stride;
}
```

## [5]-[UNIONS]

[FAMILY_SELECTION]:
- Law: the root declaration kind is family-global; record roots buy structural equality, cross-case comparison constant-false, and flatness, while class roots buy depth, intermediate cases, nested dispatch, and stop-at boundaries while surrendering generated equality.
- Law: closure is constructor reachability; private owner constructors, sealed or private-constructed cases, and class-kind recursion define discovery, while struct and interface intermediates become invisible phantom cases that fall to the default arm.
- Law: generic roots carry phantom typestate; cases close over root parameters, dispatch stays phase-total, and storage or transport pays invariance through erasing projections.
- Law: span alternatives are concrete only; `ref struct` type parameters are rejected, while a `ref struct` union keeps dispatch and stack-only state or results but surrenders generated equality.
- Law: cases admit their own payloads; the root carries no validation partial unless `[ObjectFactory<TValue>]` makes `Validate` the discriminating admission seam.
- Use: root-level dispatch stance; total `SwitchMethods` and `MapMethods` by default, sparse handling through `DefaultWithPartialOverloads`, and transport-only carriers through `SwitchMapMethodsGeneration.None` with arm-style consumption inexpressible.
- Reject: success-or-failure ad-hoc unions because rails own outcome transport; accept duplicate ad-hoc member types only when named slots model distinct ingress and equality discriminates slot before payload.

[DISPATCH_AND_GROWTH]:
- Law: `Map` evaluates every arm before dispatch; keep it to preallocated verdict tables and cold carriers, and move allocating or task-shaped arms to func-form `Switch`.
- Law: dispatch signatures own case lists; case addition is a binary break across consumers, lockstep across assembly boundaries, free inside one build graph.
- Law: blast radius partitions by axis; case rename breaks named arguments and embedded type names, while same-name payload changes break only arms touching the member.
- Law: `StopAt` and intermediate `[Union]` declarations select the growth axis a consumer couples to; each granularity stays total, and the coarse handler is the fold re-entry seam.
- Law: operation families attach as extension members with total dispatch; conversions stay generated, ordering stays absent, and rank projects through `Map` to preallocated ordinals.
- Law: recursive owners declare recursion placement; case-owned recursion absorbs growth locally, dispatch-route recursion breaks call sites, and partial dispatch is rejected for routing because new cases silently default.

[AD_HOC_FORM]:
- Law: storage is computed: typed fields per stateful unique member until a second stateful reference member collapses references into one `object` slot; struct members stay inline, and `TxIsStateless` identity rides the discriminator.
- Law: struct ad-hoc `default` is poison; index zero throws on first observation, never minting, and only `Is{Name}` probes are total enough for rehydration, pooling, and array scans.
- Law: equality gates by discriminator, then member under `DefaultStringComparison`; hash omits discriminator mixing, `ToString` erases the active case, and identity-bearing rendering routes through generated case dispatch or `Is{Name}` and `As{Name}` probes.
- Boundary: implicit conversions make the union a parameter absorber, replacing overloads and lifting mixed collection expressions, until interface, `object`, type-parameter, or duplicate members require `Create{Name}` factories; closing ingress sets both non-public `ConstructorAccessModifier` and `ConversionFromValue = ConversionOperatorsGeneration.None`.

```csharp conceptual
[Union<string, int, Blank>(T1Name = "Text", T2Name = "Count", T3Name = "Blank", T3IsStateless = true)]
public readonly partial struct FieldValue;

public readonly record struct Blank;

public static class FieldValueOps {
    extension(FieldValue value) {
        public string Rendered() => value.Switch(
            text: static text => text.Trim(),
            count: static count => $"<value-a>:{count}",
            blank: static _ => "<value-b>");

        public Option<string> TextOrNone() =>
            value.IsText ? Optional(value.AsText) : None;
    }

    public static ImmutableArray<string> RenderAll(params ReadOnlySpan<FieldValue> values) =>
        [.. values.ToArray().Select(static value => value.Rendered())];

    public static ImmutableArray<FieldValue> Mixed(string text, int count, Blank blank) =>
        [text, count, blank];
}
```

[RAIL_ARMS]:
- Law: non-abstract cases are Kleisli points: payload input, arm lift, generated `Switch` continuation; constant arms use the carrier pure lift to preserve specialization portability.
- Law: recursive arms are traversals; child folds combine through tuple `Apply`, name the pure combine, and let the carrier trait constraint flip fail-fast versus accumulating behavior.
- Law: state and result channels stay orthogonal; environment rides threaded state, results ride the carrier, and a second monad appears only for effectful environments.
- Boundary: generated dispatch is depth-honest recursion; hostile or unbounded input moves to an explicit-stack kernel at admission.

```csharp conceptual
[Union]
public abstract partial record Trace {
    public sealed record Point(double Mass) : Trace;
    public sealed record Pair(Trace Left, Trace Right) : Trace;
}

public static class TraceOps {
    extension(Trace source) {
        public K<F, B> Folded<F, B>(Func<Trace.Point, K<F, B>> leaf, Func<B, B, B> merge)
            where F : Applicative<F> =>
            source.Switch<(Func<Trace.Point, K<F, B>> Leaf, Func<B, B, B> Merge), K<F, B>>(
                (leaf, merge),
                point: static (s, p) => s.Leaf(p),
                pair: static (s, p) => Applicative.apply(
                    (p.Left.Folded(s.Leaf, s.Merge), p.Right.Folded(s.Leaf, s.Merge)), s.Merge));
    }
}
```

[WIRE_SURFACE]:
- Law: union families emit no discriminator and are not wire-capable by default; boundary-crossing families fold to a scalar grammar, reshape as keyed owners, or use edge DTOs, and non-foldable cases remain interior-only.
- Law: polymorphic JSON uses per-leaf `[JsonDerivedType]`, exact runtime type resolution, unregistered-leaf failure, `nameof` or published member-list discriminators, and boundary-pinned metadata ordering.
- Boundary: deserialization is admission; generated converters route through `Validate`, prevent half-built unions, and exist only when the framework integration assembly is present.
- Reject: wire exposure without converter gates; reflection serializers can empty-object private-keyed class owners or zero-init struct owners, bypassing admission without an exception.

## [6]-[ADMISSION_RAILS]

[FAULT_FAMILIES]:
- Law: fault families have two tiers: one string-bearing case built by static `Create` for generator text, and structured cases built directly by hooks; `ToString` returns the message.
- Law: a closed union family deriving from `Expected` is validation error, rail error, and exhaustive recovery vocabulary; `StopAt` overloads split severity at boundaries.
- Law: fault identity keeps structural equality separate from code-keyed recovery; catch-style recovery uses `Is`, `HasCode`, and `IsType<E>`, never `==`, zero codes choose message matching, and domain codes stay outside the reserved negative control band.
- Law: aggregate disposition is conjunctive for expectedness and disjunctive for exceptionality; gate on `IsExceptional` before fault-case dispatch, and keep typed faults intact through accumulation, foreign-error union, exception capture, and cancellation or timeout normalization.
- Law: wire triage depends on self-sufficient codes; only message and code serialize, hooks own thrown exceptions, and throwing foreign factories project through `Try`.

```csharp conceptual
[Union]
public abstract partial record Fault : Expected, IValidationError<Fault>, Semigroup<Fault> {
    private Fault(string detail, int code) : base(detail, code, None) { }

    public static Fault Create(string message) => new Text(message);

    public sealed record Text : Fault { public Text(string detail) : base(detail, 4000) { } }
    public sealed record Bounds : Fault { public Bounds(string detail) : base(detail, 4001) { } }
    public sealed record Aggregate : Fault {
        public Aggregate(Seq<Fault> faults) : base($"{faults.Count} faults", 4099) => Faults = faults;
        public Seq<Fault> Faults { get; }
    }

    public Fault Combine(Fault rhs) => (this, rhs) switch {
        (Aggregate l, Aggregate r) => new Aggregate(l.Faults + r.Faults),
        (Aggregate l, _) => new Aggregate(l.Faults.Add(rhs)),
        (_, Aggregate r) => new Aggregate(this.Cons(r.Faults)),
        _ => new Aggregate(Seq(this, rhs)),
    };
}
```

[RAIL_BRIDGE]:
- Law: rail projection is one expression: `Validate` property-pattern dispatch plus carrier implicit conversions; one generic extension bridge serves every owner, with owner inference from receiver position.
- Law: bridge constraints mirror the factory contract; `TRaw` carries `allows ref struct`, and the fault-base constraint uses covariance across precise owner faults.
- Law: success-arm `!` is legal only for non-null-yield generated contracts; null-yield owners use a three-valued bridge: fault, absence, instance, with absence projected to `Option<T>`.
- Reject: bridging through `Create`, `TryCreate`, or `IParsable`; framework parsing and downgraded factory forms discard evidence already carried by `Validate`.

```csharp conceptual
public static class Admission {
    extension<TOwner, TRaw>(TOwner)
        where TOwner : class, IObjectFactory<TOwner, TRaw, Fault>
        where TRaw : notnull, allows ref struct {
        public static Validation<Error, TOwner> Admitted(TRaw raw, IFormatProvider? culture = null) =>
            TOwner.Validate(raw, culture, out var owned) is { } fault ? fault : owned!;

        public static Validation<Error, Option<TOwner>> AdmittedMaybe(TRaw raw) =>
            TOwner.Validate(raw, null, out var owned) is { } fault ? fault : Optional(owned);

        public static Fin<TOwner> AdmittedFin(TRaw raw) =>
            TOwner.Validate(raw, null, out var owned) is { } fault ? Fin.Fail<TOwner>(fault) : owned!;
    }
}
```

[ACCUMULATION_ALGEBRA]:
- Law: combination algebra comes from the field dependency graph; the generated spine never accumulates, and accumulation lives only in a factory hook aggregate fold or a bridge over independent owners.
- Law: the failure type owns accumulation; traversal verbs cannot supply a missing semigroup, trait lookup is exact, and sound designs use the error base as currency or a fault-family semigroup with an aggregate case.
- Law: accumulating bridges preserve attribution by wrapping each member fault in a field-naming fault before combination; flat aggregates stay field-addressable without positional reconstruction.
- Law: layered admission follows a monotonic trust gradient: leaf owners applicative, composite relations monadic, carrier join between phases; refinements use `guard` with explicit faults, never filter-clause identity.
- Law: batch and carrier migration share one evidence-preserving lattice: `Traverse` accumulates, `TraverseM` aborts, `ToFin` packs the aggregate intact, and `Match` lifts back to accumulation.
- Law: alternative grammars keep distinct semantics: eager `|` keeps the left fault, `||` defers fallback, `Combine` accumulates independent grammar failures, and deferred fallback is trait-level `Memo<F,A>`.

[CONSTRAINT_PLANE]:
- Law: every generated owner implements one static-abstract factory contract; vocabularies layer enumeration above it, the owner type is the zero-witness typeclass instance, and shipped expression-tree or boxed relays are reused.
- Law: static-abstract dispatch monomorphizes per value-keyed instantiation and reaches reference-keyed owners through one devirtualizable constrained call; hot paths keep the generic bridge.
- Law: use the minimal sufficient constraint tier; projection-only algorithms do not require full vocabulary interfaces, and widening to the factory tier turns a vocabulary bridge program-wide.
- Law: `K<F,A>` admission arrows are primary; caller carriers select accumulating, short-circuiting, effectful, or transformer-stacked behavior, rejection requires `Fallible<TFail,F>`, and carrier migration is `Natural`.
- Boundary: span admission is a separate overload pinned to the span factory shape; byref-like raw types cannot use the general nullable key path, and span surfaces generate only on keyed lookups with declared span grammar.

[WIRE_OWNERSHIP]:
- Law: wire contracts earn protocol DTOs only when topology diverges; scalar wires delete DTOs through object factories, string factories become parse-format micro-grammars, and keyed owners choose key converter, `UseForSerialization` projection, or DTO severing at declaration.
- Law: surviving DTOs stay raw disposable records; they never grow `Validate`, and each field admits exactly once through applicative bridges.
- Law: factory declaration order is load-bearing; the last matching factory wins consumer resolution, and serialization, persistence, and binding each claim exactly one owner.
- Law: trusted rehydration is the only no-validation path; `HasCorrespondingConstructor` materializes already-admitted persistence truth, rejects vocabulary bypass, stays ignored by wire and binding, and reflection hydration re-enters static factory contracts per layer.
- Boundary: the metadata plane belongs to serializer and persistence adapters: owner taxonomy, compiled key projections, expression trees, filtered conversion routes; metadata dispatch in domain code marks a missing typed surface, and only the typed plane makes key mismatch unrepresentable.
