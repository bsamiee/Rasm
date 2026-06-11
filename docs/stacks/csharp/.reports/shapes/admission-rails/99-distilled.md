# Admission Rails — Distilled

[GENERATED_ADMISSION_SPINE]:
- Of the generated three-outcome spine, exactly one projection is rail-fit: `TError? Validate(TValue value, IFormatProvider? provider, out T? obj)` carries the typed fault and the admitted instance in a single call; `Create` and `TryCreate` are generated on top of it, so bridging either re-derives information the spine already produced.
- `T Create(TValue)` throws `System.ComponentModel.DataAnnotations.ValidationException` built from `validationError.ToString()` — the typed fault is erased to a string at the throw site, so a catch-based bridge can never recover the fault case; the exception path is unfit for rail projection by construction. The generated explicit conversion from key type to owner compiles to `Create`, making every interior cast from raw key a hidden throw site — casts from key type are boundary-only spellings.
- The two `TryCreate` overloads differ in evidence: the 2-arg form passes `out _` and discards the fault; the 3-arg form calls `Validate` directly with `[NotNullWhen(true)]` on the instance and `[NotNullWhen(false)]` on the fault — flow analysis after pattern projection is identical to a direct `Validate` call, so the 3-arg form adds nothing over `Validate` for a bridge.
- The generated `IParsable`/`ISpanParsable` surface routes through `Validate` and converts the fault to `FormatException` (message = `fault.ToString()`); `TryParse` drops the fault entirely. The parse surface exists for framework binding contracts; a domain bridge entering through `Parse` voluntarily downgrades from typed fault to string.
- Keyed enumeration owners generate the same `Validate` triple plus `Get` (throws a dedicated unknown-identifier exception carrying the enum type and offending value) and `TryGet`; `Validate` accepts `[AllowNull] string` and `ReadOnlySpan<char>` overloads, so wire keys admit without allocation and a vocabulary miss arrives as the same typed fault family as any value-object rejection — one fault vocabulary spans range checks and bounded-vocabulary misses. The generator hard-codes the ordinal-ignore-case string comparer when no key-member equality comparer is declared, and the admitted instance carries its declaration-time key, not the wire spelling — vocabulary admission is simultaneously case normalization, with no hook involved.
- For reference-typed keys the generated `Validate` rejects `null` before invoking user hooks, manufacturing the fault through the configured error type's static `Create(string)` — hooks never observe null, and the null-rejection message is owned by the fault family, not by call sites.
- No factory path guards the user hooks: `Validate` invokes `ValidateFactoryArguments(ref validationError, ref value)` bare, `TryCreate` delegates to `Validate`, and `Create` only converts the returned fault to a throw — a hook that throws detonates every admission surface including the nominally non-throwing `TryCreate`. The error channel is the only safe failure path; a hook wrapping a throwing parser must absorb the exception internally and convert it to the fault family.
- `Create` and `TryCreate` hard-code `null` as the `IFormatProvider` when delegating — only a direct `Validate` call and the generated parse surface ever thread a real provider. Culture-sensitive normalization in hooks treats a null provider as the invariant default; a rail bridge entering through `Validate` is the only admission spelling that can carry culture.
- Complex owners emit one null guard per non-nullable reference member, ahead of the hook, in member declaration order, each returning immediately with a fault manufactured through the family's `Create(string)` — admission inside a single owner is short-circuit by construction at both the null tier and the hook tier (one `ref` error slot). Accumulation has exactly two homes: inside the hook by folding into the family's aggregate case, or across owners at the bridge; the generated spine never accumulates, so independence between faults must be reified as separate owners before the applicative bridge can surface them together.
- When the key argument is named `provider`, the generator renames the format-provider parameter to `formatProvider`; named-argument call sites using `provider:` then bind the raw key. Positional bridges never notice; reflective or named-argument callers can silently swap roles.

[ONE_HOP_RAIL_BRIDGE]:
- The entire projection from generated outcome to rail is one expression: `Owner.Validate(raw, provider, out var item) is { } fault ? fault : item!` — the property pattern discriminates and the rail's implicit conversions lift the result (`Validation<F, A>` lifts implicitly from both `F` and `A`; `Fin<A>` from both `A` and the error base). A fault subtype lifts into `Validation<Error, T>` without a cast because the standard reference conversion to the error base composes with the user-defined implicit lift.
- The static-abstract factory contract `IObjectFactory<T, TValue, TError>` — implemented by every generated owner, enumerations and value objects alike — supports a single polymorphic bridge as a generic static extension block; fixing the fault type in the constraint makes inference total, the owner binding from the receiver and the raw type from the argument:

```csharp
public static class AdmissionBridge
{
    extension<TOwner, TRaw>(TOwner)
        where TOwner : class, IObjectFactory<TOwner, TRaw, Fault>
        where TRaw : notnull, allows ref struct
    {
        public static Validation<Error, TOwner> Admit(TRaw raw, IFormatProvider? provider = null) =>
            TOwner.Validate(raw, provider, out var owned) is { } fault ? fault : owned!;

        public static Fin<TOwner> AdmitFin(TRaw raw, IFormatProvider? provider = null) =>
            TOwner.Validate(raw, provider, out var owned) is { } fault ? Fin.Fail<TOwner>(fault) : owned!;
    }
}
```

- Call sites read owner-dotted with zero type arguments — `Port.Admit(8080)`, `Mode.Admit("<key-a>")` — one declaration for the whole domain. A leading-type-parameter generic method cannot achieve this because generic argument inference is all-or-nothing; the extension receiver is the only position from which the owner type infers.
- The null-suppression bang in the success arm is justified only by the generated contract (fault null ⇒ instance non-null). Owners configured to yield null for absent input break that contract deliberately, and their bridge is three-valued: fault ⇒ `Fail`, null instance ⇒ `None`, instance ⇒ `Some` — projecting into `Validation<Error, Option<T>>` or collapsing absence to a typed fault at the call site, never the bang.
- `Fin<A>` failure construction in generic code is `Fin.Fail<A>(error)`; `Prelude.Fail<E>(error)` returns the `Fail<E>` carrier struct, not `Fin<A>`, and `Prelude.FinFail<A>` is `[Obsolete]` in favour of `Fin.Fail`. Target-typed `Fail(error)` resolves to `Fin<A>` only where assignment context provides the type; the nested `Fail`/`Succ` case records are types, not factories, and direct `new` on them is a longer spelling of the same value.
- Failure-side rewrites stay on the rail — `MapFail` re-codes, `BindFail` substitutes a recovery admission, `BiBind` folds both sides; a bridge that pattern-matches mid-pipeline to re-wrap errors reimplements these combinators.
- The factory contract declares `TValue : notnull, allows ref struct` — the admission interface itself admits byref-like raw types. A generic bridge whose raw-type parameter omits `allows ref struct` silently narrows the contract and excludes every span-keyed admission overload from the polymorphic path; the anti-constraint must be mirrored. The raw value is consumed inside the call, so the rail carrier never holds the span — only the parameter position is byref-like.
- The contract is covariant in its error parameter (`out TValidationError`) and `IValidationError<out T>` is itself covariant; C# satisfies generic constraints through variant interface conversions even when the interface carries static abstract members. An owner implementing the contract with a derived fault case satisfies a bridge constrained on the fault base, with the static-abstract `Validate` dispatching to the owner's implementation viewed as the base — one base-constrained bridge serves a fault lattice, each owner declaring its most precise case while bridge, carriers, and recovery vocabulary uniformly see the base.

[TYPED_FAULT_FAMILIES]:
- The validation-error contract is a covariant, class-constrained interface with one static abstract member, `static abstract TSelf Create(string message)`; the error-rail base for non-exceptional failures is a positional record `(string Message, int Code, Option<Error> Inner)`. A single record satisfies both contracts at once — one fault type that is simultaneously the generated owner's validation error and a first-class rail error, eliminating every per-call-site error-translation hop.
- Every message the generator itself authors (null argument, vocabulary miss) is manufactured through the family's static `Create(string)` — the `Create` case is the family's textual catch-all while hooks construct precise cases directly. Designing the family means designing two tiers: one string-bearing case for generator-authored text, N structured cases for hook-authored evidence. Per-owner derived cases additionally partition generator text by owner — the one population a shared family can never attribute without the derived-case lattice.
- A closed union-shaped fault family is a generated union whose base derives from the expected-error record. The analyzer rejects a primary constructor on the union base (it cannot be made private), so the base takes a private constructor that nested sealed cases chain through — nesting grants access to the private constructor while sealing the family against foreign extension:

```csharp
[Union]
public abstract partial record AdmitFault : Expected, IValidationError<AdmitFault>, Semigroup<AdmitFault>
{
    private AdmitFault(string detail, int code) : base(detail, code, None) { }

    public sealed record Shape : AdmitFault { public Shape(string detail) : base(detail, 1001) { } }
    public sealed record Bounds : AdmitFault { public Bounds(string detail) : base(detail, 1002) { } }
    public sealed record Aggregate : AdmitFault
    {
        public Aggregate(Seq<AdmitFault> faults) : base($"{faults.Count} admission faults", 1099) => Faults = faults;
        public Seq<AdmitFault> Faults { get; }
    }

    public static AdmitFault Create(string message) => new Shape(message);

    public AdmitFault Combine(AdmitFault rhs) => (this, rhs) switch
    {
        (Aggregate l, Aggregate r) => new Aggregate(l.Faults + r.Faults),
        (Aggregate l, _)           => new Aggregate(l.Faults.Add(rhs)),
        (_, Aggregate r)           => new Aggregate(this.Cons(r.Faults)),
        _                          => new Aggregate(Seq(this, rhs)),
    };
}
```

- Under the per-owner precision pattern, derived cases embed in the shared family as nested members with their own `IValidationError<TDerived>` implementation — required as a shadowing `static new Create` returning the derived type, because the `[ValidationError<T>]` attribute constraint is self-referential and checked at definition time, so a malformed lattice member fails before generation runs. The base's generated dispatch includes the derived cases as leaf arms, so closed-dispatch guarantees apply at every tier:

```csharp
[Union]
public abstract partial record Fault : Expected, IValidationError<Fault>
{
    private Fault(string detail, int code) : base(detail, code, None) { }

    public static Fault Create(string message) => new Text(message);

    public sealed record Text : Fault { public Text(string detail) : base(detail, 2000) { } }

    public sealed record Range : Fault, IValidationError<Range>
    {
        public Range(string detail) : base(detail, 2001) { }
        public static new Range Create(string message) => new(message);
    }
}

[ValueObject<int>]
[ValidationError<Fault.Range>]
public sealed partial class Port;

// Port : IObjectFactory<Port, int, Fault.Range> satisfies a bridge constrained on
// IObjectFactory<TOwner, TRaw, Fault> through covariance; generator text lands in Fault.Range.
Validation<Error, Port> admitted = Port.Admit(8080);
```

- Fault recovery inherits closed-dispatch guarantees: the generated exhaustive `Switch`/`Map` over fault cases makes a recovery routine that omits a case a compile error, not a logging gap; the partial-overloads setting (`DefaultWithPartialOverloads`) adds variants with optional per-case handlers and one mandatory default arm — closed-world recovery and open-world triage from a single declaration, the closed form still compile-breaking on a new case. Hierarchies deeper than two levels use `[UnionSwitchMapOverload(StopAt = new[] { typeof(IntermediateCase) })]` to generate stop-at overloads, so a severity tier — recoverable versus fatal as intermediate abstract cases — dispatches as two arms at the boundary and leaf-exhaustively in interior recovery, as overloads of the same method.

[FAULT_RECOVERY_ALGEBRA]:
- Fault identity has two distinct relations whose conflation is a live bug source: record `Equals` is structural over all positional members (same code, different message ⇒ not equal; different runtime case type ⇒ not equal), while `Is` matches by code. `Is`, `HasCode`, `IsType<E>`, and `Filter<E>` form the recovery algebra; catch-style matching goes through `Is`/`HasCode`, never `==`.
- The catch-value overload (`catch(E error, ...)`) matches by the generic failure type's `Equals` — structural for records, case type and every member coinciding — while the `Error`-specialized typed `catch<E>` matches by `e is E || e.IsType<E>()`, an aggregate-recursive type test. The same catch expression changes meaning when a seam widens the failure type from a typed family to the error base; the predicate form and the integer-code form are the only relation-stable spellings across that widening.
- Typed-subtype `catch<E>` on an aggregate failure filters to matching members and invokes the handler via `ForAllM` — every per-member recovery binds monadically and the last one's value wins; the `FoldM` overload composes per-member recoveries through first-success choice instead. Handlers performing one substitute admission per fault are last-write-wins under `ForAllM`; batch-aware recovery matches the aggregate explicitly with a predicate over count and membership, or extracts typed members once and folds deliberately.
- The error base is a monoid: `+` flattens into a many-errors carrier whose `Count`, `Head`, `Tail`, and `AsIterable` expose the flat stream. On aggregates, expectedness is conjunctive and exceptionality is disjunctive (all-expected ⇒ expected; any-exceptional ⇒ exceptional) — one exceptional infiltrator flips the whole aggregate's disposition, so triage gates on `IsExceptional` before reading fault cases.
- `Filter<E>()` reaches through the aggregate and returns only faults of the requested subtype (empty aggregate when none match). Typed faults survive accumulation and remain recoverable after union with foreign errors — the property that licenses `Error` as the universal failure currency.
- Catch-style matching on the error base is two-mode: a non-zero code makes `Is` compare codes and ignore messages (localization-stable by design); a zero code degrades `Is` to message-string equality. A family that leaves codes at default has opted into string-matching recovery without noticing — every structured case claims a non-zero code, and the string-bearing generator-text case is the only tolerable zero-code member. The error base lifts implicitly from a code-and-message tuple but only explicitly from a bare string — code-bearing minting is the path of least resistance by conversion design, and every code-zero fault requires a visible cast to create. Provenance-threading mint overloads take an inner error alongside message or code-message: the canonical field-labeling move at an accumulating bridge wraps each field's fault as the inner of a field-naming fault before combination, so the flat aggregate of a multi-field admission remains attributable per field without positional reconstruction.
- The runtime reserves a contiguous negative code band for control-signal errors: cancelled (−2000000001), timed out (−2000000002), sequence empty (−2000000003), closed (−2000000004), parse (−2000000005), end of stream (−2000000010), validation failed (−2000000011), and several IO-trait gaps — two distinct reserved errors share −2000000008. Domain fault codes allocate outside this band; code-keyed recovery is integer equality, and a collision makes a domain fault impersonate a cancellation or timeout to every generic handler upstream.
- Exception-captured faults take the exception's `HResult` as their code — the code space of exceptional errors is the host's HRESULT space, not the domain's. Code-keyed recovery distinguishes the two populations by expectedness first; a numeric code only means what its side of the expected/exceptional split says it means.
- Aggregates recurse every membership predicate (`Is`, code possession, type possession, exception possession) over their members but expose their own fixed code as a property — an aggregate never satisfies code-possession for its own aggregate code unless a member carries it. Recovery never keys on the aggregate marker code; it keys on member evidence reached through the recursive predicates or typed extraction.

[EXCEPTION_CHANNEL_FIDELITY]:
- Typed faults round-trip exception-only channels with reference fidelity: throwing an expected fault produces a wrapper exception holding the fault instance, and the capture constructor on the error base unwraps any wrapper back to the original object — a derived fault record crosses a throw/catch-only boundary and arrives as the same case, same payload, no serialization. The catch-side spelling is always the capture constructor, never direct exceptional construction, because only the former performs the unwrap.
- The capture constructor is a normalization tree: cancellation exceptions (task and operation flavors) collapse to the reserved cancellation error, timeouts to the reserved timeout, aggregate exceptions flatten into the many-errors carrier, and only the remainder becomes exceptional. Boundary capture auto-classifies host control-flow exceptions into the reserved vocabulary; catch-side code that re-wraps a cancellation as a domain fault fights the rail's own taxonomy.
- The exception mirror of the error algebra is itself a monoid (combinable wrappers, a many-exceptions carrier, an empty), and every wrapper writes the fault code into the exception's `HResult` — foreign frames that know nothing of the rail still key on the domain code through the standard exception surface.
- The wire serialization contract of the error base is exact: `Expected` serializes only `Message` and `Code` (`Inner` is `[IgnoreDataMember]`); `Exceptional` serializes only `Message` and `Code` (the wrapped exception populated only in-process); `ManyErrors` serializes its member list. Inner-chained provenance is process-local by declared contract — the field-labeling chain from the bridge survives precisely until the wire, where each labeling fault keeps its own code and message and loses its chain. Because derived fault cases are polymorphic records over the base, a boundary serializer bound to the static base type erases case identity and structured payloads: codes must be self-sufficient for remote triage; structured payloads and inner chains are in-process diagnosis material only.
- Exceptional errors deliberately drop their captured exception on deserialization (cross-process leak prevention): stack fidelity is process-local, the in-process throw path preserving the original stack via dispatch-info capture while the rehydrated path throws a synthetic carrier. Fault payloads that must survive process hops carry their evidence in the expected-fault record, never in the wrapped exception.
- For admission surfaces not under generated control — foreign factories that throw — the lazy capture monad is the one-hop projection: a record around a result-thunk whose lift wraps the computation and whose run applies the capture constructor's normalization to whatever escapes; its combination operator accumulates on failure, giving foreign-grammar fallback the same evidence semantics as native accumulation. Generated owners need none of this — the capture monad exists exactly for admission code that cannot be made total.

[ADMISSION_SOURCE_CONTRACT]:
- Declaring an additional admission source generates only wiring — the owner-marker interface, the typed factory interface, conditionally the outbound-conversion interface, and one runtime metadata row (value type, validation-error type, framework flags, optional constructor-conversion expression); the `Validate` body is compelled by the static abstract and authored by the owner. The owner kind is unrestricted: keyed and keyless enumerations, simple and complex value objects, ad-hoc unions, and regular union roots all take the attribute.
- A regular union root carrying the factory attribute becomes a single admission seam for its whole closed family — the user `Validate` discriminates the grammar and constructs the case, the same generic bridge projects the outcome, and case selection collapses into admission instead of trailing it:

```csharp
[Union]
[ObjectFactory<string>]
[ValidationError<Fault>]
public abstract partial record Source
{
    public sealed record ByPath(string Path) : Source;
    public sealed record ByUrn(string Urn) : Source;

    public static Fault? Validate(string? value, IFormatProvider? provider, out Source? item)
    {
        item = value switch
        {
            ['/', ..]                => new ByPath(value),
            ['u', 'r', 'n', ':', ..] => new ByUrn(value),
            _                        => null,
        };
        return item is null ? Fault.Create($"unrecognized source grammar: '{value}'") : null;
    }
}
```

- The factory-source attribute's type parameter declares `allows ref struct`, so an owner may declare a span-of-char admission grammar directly; the compelled static abstract then admits UTF-16 wire buffers with the typed fault on rejection and zero string materialization on the rejection path. Declaring the span grammar upgrades the generated span-parse surface for free — span `Parse` routes through the declared span `Validate`, span `TryParse` routes through it discarding the fault — so the framework binding path becomes zero-copy as a side effect of the domain grammar. Generated span admission exists without declaration only on keyed enumerations; keyed value objects never receive a generated span surface because their admission constructs rather than looks up — span admission for constructed owners is the declared-grammar route, and the grammar decides where the one unavoidable string materialization happens (after structural rejection checks, not before).
- Two-way conversion is opt-in by consequence, not by method: any serialization or persistence flag on the factory attribute forces the outbound interface, whose `ToValue()` the owner must implement — the round-trip law (admit after project is identity modulo normalization) becomes a compile-visible obligation the moment the owner faces a wire.
- The trusted-rehydration flag declares a constructor accepting the foreign value (`HasCorrespondingConstructor`), letting materialization from already-admitted storage skip `Validate` entirely; it is rejected on enumerations, whose identity map cannot be bypassed. This is the single sanctioned no-validation path, scoped precisely to data that passed admission before persistence — any other ingress through it reintroduces the unvalidated interior the architecture exists to prevent.
- Multiple factory attributes on one owner are analyzer-partitioned: overlapping serialization frameworks, duplicate model-binding claims, and duplicate persistence claims are compile errors — each framework concern resolves to exactly one admission grammar, so wire routing is total and unambiguous at definition time.

[WIRE_GRAMMAR_TRANSLATION]:
- An additional admission source type is declared on the owner; the owner implements the same `Validate` triple for the foreign type, and the correct implementation parses at the edge and delegates into the generated member-wise `Validate` — one validation spine serves N wire grammars, and a wire grammar can never drift from the member rules because it terminates in them:

```csharp
[ComplexValueObject]
[ValidationError<Fault>]
[ObjectFactory<string>]
public sealed partial class Span
{
    public int Lo { get; private init; }
    public int Hi { get; private init; }

    static partial void ValidateFactoryArguments(ref Fault? validationError, ref int lo, ref int hi) =>
        validationError = lo <= hi ? null : Fault.Create($"lo {lo} above hi {hi}");

    public static Fault? Validate(string? value, IFormatProvider? provider, out Span? item)
    {
        item = null;
        return value?.Split("..") is [var l, var h]
               && int.TryParse(l, provider, out var lo)
               && int.TryParse(h, provider, out var hi)
            ? Validate(lo, hi, out item)
            : Fault.Create($"not a span literal: '{value}'");
    }
}
```

- The factory attribute's framework toggles (serialization frameworks, model binding, ORM) route framework-driven deserialization through the same factory — wire admission and domain admission are one path, a payload that deserializes is a payload that validated, and the no-double-validation law extends across the process boundary.
- DTO-to-owner translation under accumulation is the applicative composition of per-field bridges — the DTO never grows a `Validate` of its own; it is a record of raw values whose admission is the applicative expression, keeping validation logic in exactly one place per field and making the DTO disposable at the boundary.

[NORMALIZATION_AND_SENTINEL_LAW]:
- Normalization belongs inside admission, not before it: the factory hook receives the raw value by `ref`, so trimming, canonicalization, and clamping mutate the value pre-storage and the owner's key is the normalized form — call sites cannot admit an unnormalized value because the hook runs on every factory path.
- String owners opt into absent-as-null semantics via `EmptyStringInFactoryMethodsYieldsNull`: whitespace-only input returns null fault and null instance from `Validate` (generated as an early is-null-or-whitespace return) — blank input is absence, not a fault, and the bridge projects it to `None`. Setting it implies `NullInFactoryMethodsYieldsNull`; both are inert for struct owners and struct keys. The annotation cost differs: `EmptyStringInFactoryMethodsYieldsNull` deletes `[NotNullWhen(true)]` from both `TryCreate` overloads — `TryCreate(...) == true` no longer proves a non-null instance, the boolean surface inheriting a three-valued contract with no signature change — while `NullInFactoryMethodsYieldsNull` decorates `Create` with `[return: NotNullIfNotNull(...)]`, keeping flow analysis exact. A bridge generated-contract audit reads the attributes, not the docs.
- Struct owners deny `default` by default (explicit `default` and parameterless construction rejected unless `AllowDefaultStructs` is set), the `IDisallowDefaultValue` marker advertises the policy to infrastructure, and the canonical-empty property name is configurable — the default-value sentinel is unrepresentable rather than checked-for downstream. Enforcement is analyzer-only and syntactic: generic type parameters, array allocation, and uninitialized fields manufacture zero-ghosts the analyzer cannot see, and the generator emits no runtime initialization check anywhere. The runtime gate therefore belongs to the seam receiving possibly-defaulted storage: infrastructure detecting the marker inspects the key member directly (null for reference keys, zero-pattern for value keys) and rejects with a typed fault before the ghost crosses inward — reading the key beats equality against a default instance, because generated equality routes through key comparers whose null handling is comparer-specific. Deserializers and materializers constructing structs without factories are the dominant ghost source; routing them through a declared admission source closes the hole at definition time, the stronger fix.
- A trim-or-nullify string extension (null/empty/whitespace ⇒ null, else trimmed, optional max-length) exists precisely for pre-admission hygiene at DTO edges where the empty-string knob is not in play.
- Host-API sentinels (zero IDs, origin points, min-dates) are not the generated owner's concern: they normalize to `Option` at the seam before admission, then convert to a typed fault via the option-to-rail projections — `ToFin(fault)`, `ToValidation<F>(() => fault)`, fault value or fault thunk — only where the value is mandatory. Two distinct decisions (is it absent? is absence legal here?) collapse into one bug when sentinels reach the validation hook; absence becomes a typed fault exactly where absence is illegal and stays optional where it is legal.

[UNREVALIDATABLE_INTERIOR]:
- The generated owner is constructively unrevalidatable: the constructor is private, complex-owner members must have private `init` accessors (a public `init` is a compile error via analyzer), and `Validate` accepts only the raw key type — interior code holding `T` cannot re-run admission because no surface accepts an already-admitted value. The no-double-validation law is a type-system fact, not a convention.
- Inbound key-to-owner conversion — the hidden throwing admission spelling — is definition-time policy: explicit by default (`ConversionFromKeyMemberType = Explicit`), configurable to none, and forced to none when factories are skipped because the operator body delegates to the factory. Setting it to none erases the cast-admission spelling from the entire codebase, leaving the rail bridge as the only inbound spelling that compiles. Outbound owner-to-key projection generates as two operators: a safe null-propagating nullable-to-nullable conversion with `[return: NotNullIfNotNull]` (implicit by default), and for reference owners with non-nullable keys an additional unsafe non-nullable conversion that throws a bare null-reference exception on a null owner (explicit by default) — struct owners with non-nullable keys instead receive a no-throw non-nullable overload. The safe implicit operator converts an absent owner into an absent key silently; optional domain values project outward by mapping over the option to the key, never by flowing a nullable reference through the implicit conversion.
- Skipping factory methods cascades: no type converter, no factory-contract implementation, no key-to-owner conversion operator, parse interfaces forced off, arithmetic operator generation forced off — an owner without factories is a pure interior value born only from other admitted values.
- Factory method names are configurable on the owner attribute — the admission verb can match domain vocabulary repo-wide without wrappers, removing the last excuse for a renaming bridge layer.

[APPLICATIVE_ACCUMULATION]:
- Multi-field admission composes applicatively at the bridge — every field admits independently and all faults surface in one pass. The combinator spelling `fun(ctor).Map(va).Apply(vb).As()` and the operator spelling `*` compose the same value; the operator form accepts uncurried multi-parameter functions directly on either side:

```csharp
var admitted = (fun((Host h, Port p) => new Endpoint(h, p)) * Host.Admit(rawHost) * Port.Admit(rawPort)).As();
Fin<Endpoint> collapsed = admitted.ToFin();
```

- The tuple spelling is a second accumulating form: `(Host.Admit(h), Port.Admit(p)).Apply(static (a, b) => new Endpoint(a, b))` — the extension `.Apply` on `K<AF, Func<A,B,C>>` auto-curries and applies sequentially, at arities up to 10, binding the trait-level applicative with the same semigroup-resolution behavior: with the error base as failure currency it accumulates; with a typed family lacking its own trait instance it silently keeps the first fault.
- Monadic composition short-circuits at the first fault — comprehension syntax encodes data dependency (later admissions read earlier results), applicative syntax encodes independence (all faults wanted). Choosing between them is a statement about the dependency graph of the fields, not a style preference.
- The failure-type semigroup is resolved at runtime by reflecting for the closed trait instance — generic-type construction over the static failure type, then a static instance-property read wrapped as an option — so the lookup is exact in the failure type and invisible to the constraint solver. The miss mode is silent on the hot path: a failure type without the semigroup trait keeps only the first failure (the function-carrier's fault) and discards the right operand's — no exception on the apply path; only the identity operations (alternative-empty, monoid-unit contexts) fail loud naming the missing monoid. Deriving from the error base does not help — an inherited instance for the base is not an instance for the fault type. A composition that never names the identity operations never learns its semigroup was missing: the first multi-failure input, not the call site, is where degradation surfaces. Two designs are sound: carry `Error` as the failure type (faults lift implicitly, the many-errors monoid accumulates, typed extraction recovers), or implement the semigroup on the fault family itself with an aggregate case so accumulation stays typed end-to-end.
- Which overload binds is decided by the static type of the operands: values typed as the concrete carrier bind the runtime-resolved operators, values typed as the trait-kinded abstraction bind the compile-constrained ones. The same expression text is constraint-checked or constraint-erased depending on whether an intermediate variable was declared as the concrete type — a reason to let the bridge return the concrete carrier and keep composition in one expression.
- The unary `+` operator on the trait-kinded carrier is the downcast to the concrete type — operator chains stay concrete without `.As()` ceremony; `>>` sequences dependent admissions, and a `>>` with a lowering marker closes a trait-typed pipeline back to the concrete carrier.
- The public construction surface (`Success`, `Fail`, `Empty`) demands the full monoid at compile time, and the sequence-accepting `Fail` folds a precomputed fault batch through the monoid before wrapping — faults collected out-of-band enter the rail as one combined failure, never as a sequence bolted beside the carrier.
- Cross-field refinement after independent admission threads as a guard inside the comprehension — `from _ in guard(predicate, (Error)fault)` — keeping refinement on the rail without a second validation surface; the accumulating carrier binds guards and failure values natively.

[FALLBACK_GRAMMARS]:
- Alternative admission grammars have two composition spellings with fundamentally different semantics. `|` routes through `Choose` with eagerly evaluated operands — both admissions run before selection, and on total failure only the left fault is returned. Lazy right-side evaluation is `||`: the carrier defines `operator true` (`IsSuccess`) and `operator false` (`IsFail`), so `a || b` evaluates as first-success with deferred right admission. Semigroup combination (`.Combine`) is failure-dominant: if either operand fails the result fails, faults accumulated through the error type's semigroup — `Fail.Combine(Success)` and `Success.Combine(Fail)` both yield `Fail`. `||` reads as "defer the fallback grammar"; `|` as "the canonical grammar's rejection is the diagnosis"; combining as accumulation across independent grammars where any miss is a fault:

```csharp
Validation<Error, Endpoint> first = Endpoint.Admit(text) | Endpoint.AdmitCompact(text);
Validation<Error, Endpoint> total = Endpoint.Admit(text).Combine(Endpoint.AdmitCompact(text));
```

- At the trait level the same split is structural: the applicative contract carries an eager `K<F, A>` `Apply` overload and a lazy `Memo<F, A>` overload forced only after the function carrier is known, and first-success grammars compose through `Choose(K<F, A>, Memo<F, A>)` — deferred fallback admission is a trait-level `Memo` overload, never a hand-rolled control-flow trick; `||` is its concrete-carrier surface.
- The carrier implements the predicate-gated catch trait, so effect-world recovery vocabulary composes directly on admission results: a catch value on the right of `|` substitutes a recovery admission only for matching faults, non-matching faults passing through unchanged — typed-fault triage without leaving the rail or writing a match.
- `BindFail` substitutes an entire recovery admission for the failure side and may simultaneously re-type the failure (the target failure type carrying its own monoid obligation); `MapFail` re-codes faults in place, its most common bridge use lifting a typed family into the error-base currency once, immediately before joining streams that accumulate heterogeneous faults.

[EVIDENCE_FREE_FAILURES]:
- `Filter` and `Where` on the accumulating carrier manufacture the failure monoid's identity as the rejection value — for the error base, the empty aggregate: zero members, count zero, empty flag set, expectedness vacuously true, fixed aggregate code. A `where` clause inside an admission comprehension converts predicate failure into evidence-free rejection that no code-keyed, type-keyed, or membership-keyed recovery will ever match; cross-field refinement is spelled as a guard with an explicit fault, and the lazy-thunk guard overload defers fault construction to the rejection path.
- The empty aggregate doubles as the miss-marker of typed-fault extraction: filtering an error for a fault subtype returns it when nothing matches. Triage that counts faults or folds over members gates on emptiness first — an empty aggregate is still a failure, and treating "no matching faults" as "no failure" inverts the rail.
- The bottom error exists for value-less evaluation states, its diagnostic text naming uninitialized structs and filtered-out expressions as causes — the rail has a designated citizen for "no evidence" failures, and admission code never mints a second one.

[TWO_PHASE_COMPOSITION]:
- Layered admission has a canonical two-phase shape: leaf owners admit raw fields applicatively (independent, all faults surface), and the composite owner — whose generated member-wise `Validate` accepts already-admitted leaf types — binds monadically on top, because composite admission is data-dependent on leaf success and can only fail on cross-member invariants. The applicative stage yields a nested carrier; flattening is the join between phases — the carrier's own join, never a manual unwrap — and a guard carries any post-construction refinement:

```csharp
public static Validation<Error, Window> Admit(RawWindow raw) =>
    from window in (fun((Lo lo, Hi hi) => Window.Admit(lo, hi)) * Lo.Admit(raw.Lo) * Hi.Admit(raw.Hi)).Flatten()
    from _ in guard(window.Span > 0, () => (Error)Fault.Create($"degenerate window: {window}"))
    select window;
```

- For nested-collection admission the four traversable entry points select fault semantics purely by the strength the inner carrier offers: `Traverse`/`Sequence` for an applicative inner carrier, `TraverseM`/`SequenceM` for a monadic one.
- The carrier lattice is bidirectional and lossless in both directions because the error base is the monoid: collapsing the accumulator to the sequential carrier via `ToFin` packs the accumulated aggregate into the single error slot intact (flat member stream, counts, typed extraction all survive), and the sequential carrier lifts back to the accumulator via `Match` over the implicit conversions — both error and value lift implicitly. Direction is a statement about where accumulation continues, not about evidence retention: collapse at the frontier where flow becomes sequential; lift back where a later seam resumes parallel admission.
- The sequential carrier's terminal boundary spelling is the throw-if-failed projection, which rethrows through dispatch-info capture — original stack for exceptional faults, wrapper exception with code-bearing `HResult` for expected ones — making the outermost host boundary a one-liner that preserves the full fidelity story.
- Comprehension syntax over the accumulating carrier supports guards natively in both positions — guard-first (precondition before any admission) and guard-after (refinement over admitted values) — via dedicated bind support on the guard shape, so refinement never forces a detour through the sequential carrier and back.

[CARRIER_POLYMORPHIC_ARROWS]:
- An admission arrow typed `Func<TRaw, K<F, TOwner>>` — not a concrete `Func<TRaw, Validation<Error, TOwner>>` — is carrier-agnostic: one arrow drives accumulating, short-circuiting, effectful, and transformer-stacked admission depending solely on which `F` the caller supplies. The concrete-typed bridge is the special case obtained by fixing `F`; the kinded form is the primary.
- The trait the composition demands selects exactly the semantics it is allowed to use: `Applicative<F>` permits combining independent admissions but forbids inspecting one to decide the next; `Monad<F>` adds sequential dependency at the cost of parallel accumulation; `Choice<F>`/`Alternative<F>` add ordered fallback; `Fallible<TFail, F>` adds carrier-agnostic rejection. The weakest sufficient trait is the correct constraint — demanding `Monad` forecloses ever specializing the same composition to an accumulating carrier.
- Bare `Applicative<F>` cannot inject a fault — it exposes only `Pure`, never a failure constructor — so a polymorphic arrow that must reject constrains on the conjunction with `Fallible<TFail, F>`: success is `F.Pure`, rejection is `F.Fail`, the precise capability set letting one arrow both succeed in any applicative and fail in any fallible carrier.
- Carrier migration is a named arrow, not a re-admission: `Natural<TFrom, TTo>` instances expose `static abstract K<G, A> Transform<A>(K<F, A>)`, so an admitted value moves between sequential, lazy-effect, optional, and either carriers by transformation rather than unwrap-and-rewrap — carrier choice at one seam never forecloses carrier choice at the next.

[BATCH_TRAVERSAL]:
- Batch admission of a homogeneous collection is one traversal: `raws.Traverse(Owner.Admit)` lifts `Seq<TRaw>` and the kinded arrow to `K<F, Seq<TOwner>>`. `Traverse` and `TraverseM` share one fold skeleton and differ only in the combiner — `Traverse` threads through the applicative lift (independence: every element evaluated, all faults surfaced under an accumulating `F`), `TraverseM` threads through `Bind` (dependence: first failure aborts the remainder). All-faults-versus-first-fault is the choice of `F` and the verb, derived from one fold, never two code paths:

```csharp
public static K<F, Seq<TOwner>> AdmitAll<F, TOwner, TRaw>(Seq<TRaw> raws)
    where F : Applicative<F>, Fallible<Fault, F>
    where TOwner : class, IObjectFactory<TOwner, TRaw, Fault>
    where TRaw : notnull, allows ref struct =>
    raws.Traverse(raw => TOwner.Validate(raw, null, out var owned) is { } fault
        ? F.Fail<TOwner>(fault)   // kinded fault lift — accumulates under the accumulating F, halts under the sequential F
        : F.Pure(owned!))
        .As();
```

- The accumulation guarantee lives in the failure type's algebra, not in the traversal verb: applicative traversal over a fallible carrier accumulates only when the failure type carries a semigroup; with a non-monoidal failure it still type-checks and silently degenerates to first-error semantics. A traversal that must accumulate constrains the failure type's algebra, not merely the verb.
- `Sequence` is `Traverse` of identity — the transpose `K<T, K<F, A>> -> K<F, K<T, A>>` collapses a sequence of already-launched admissions into one carrier holding the sequence — and default traversal derives `Traverse` as `Sequence` composed with `Map`, so a traversable instance authors one direction and inherits the other.

[TRANSFORMER_MONOID_THREADING]:
- The validation transformer carries `Func<MonoidInstance<TFail>, K<M, Validation<TFail, A>>>` — the run function takes the monoid as a parameter instead of reflecting for it. Stacked admission therefore cannot degrade to first-fault silently: the monoid is in scope by construction at every apply, and the obligation moves to the single seam where the transformer is run, not to every apply site:

```csharp
// transformer Apply: M sequences, monoid accumulates — both in one expression
static K<ValidationT<F, M>, B> Apply<A, B>(K<ValidationT<F, M>, Func<A, B>> mf, K<ValidationT<F, M>, A> ma) =>
    new ValidationT<F, M, B>(monoid =>
        from ff in mf.As().Run(monoid)
        from fa in ma.As().Run(monoid)
        select ff.ApplyI(fa, monoid).As());
```

- The transformer's lift moves an inner-monad effect into the admission layer without admitting: an effectful source — a read that may fail in `M` — feeds the admission applicative as a peer field beside pure field admissions. The inner monad owns acquisition failure, the validation layer owns admission failure, and both surface through the one run.
- The transformer's alternative-empty manufactures the monoid identity as a failure lifted into the inner monad — an evidence-free rejection at the bottom of a stack is a well-typed inner-monad value, so triage that gates on aggregate emptiness must reach through `M` to read it.

[STATIC_ABSTRACT_CONSTRAINT_PLANE]:
- The generated vocabulary interface is an F-bounded static-abstract typeclass: `ISmartEnum<TKey, T, out TError> : ISmartEnum<TKey>, IObjectFactory<T, TKey, TError>, IObjectFactory<TKey>` with `where T : ISmartEnum<TKey>` — the self-reference is the dictionary witness, so a generic algorithm constrained on the one interface receives enumeration, lookup, admission, and key projection as static-abstract members callable through the type parameter, the owner type itself being the passed-in typeclass instance with zero runtime witness object.
- The lattice is layered so each lower tier is satisfiable by strictly more shapes than the tier above: the key-only enum interface yields key projection and identity without lookup or admission; the trinary factory interface yields admission without enumeration and is satisfied by keyed value objects equally; the bare single-parameter factory marker carries only the value-type witness and exists to let `notnull, allows ref struct` flow without naming the error type — a kernel that only needs "buildable from this raw shape" constrains on the bare marker and stays error-type-agnostic.
- Selecting the minimal tier is collapse discipline applied to constraints: a projection-only algorithm constrained on the full enum interface has over-specified and silently rejects every keyed value object that would have satisfied it; widening a bridge from the enum interface to the factory interface is exactly the act that turns a vocabulary-specific bridge into a program-wide one.
- Enumeration is itself constraint-reachable — `static abstract Items` folds through the type parameter — so one generic startup probe walking every vocabulary through the constraint forces deferred lookup materialization for the whole program in one pass, and the duplicate-key fail-fast fires inside the generic call rather than at the first production admission of each concrete type:

```csharp
public static B FoldFamily<T, TKey, TError, B>(B seed, Func<B, T, B> step)
    where T : ISmartEnum<TKey, T, TError>
    where TKey : notnull
    where TError : class, IValidationError<TError> =>
    T.Items.AsIterable().Fold(seed, step);
```

- The language forbids invoking a static-abstract member on a bare type parameter outside a constrained generic context — the entire reason the shipped relays exist: aggressively inlined generic statics whose whole body is the constrained call (`T.Validate(...)`, `TError.Create(...)`), consumed by expression trees and boxed dispatch that hold the constraint but cannot name the member. Reusing them buys the package's exact dispatch instead of re-deriving a per-owner bridge.
- Static-abstract members resolve through constrained call, monomorphized per closed value-type instantiation and shared per reference-type instantiation: a generic admission method over a value-keyed owner specializes to a direct call with no interface dispatch and no allocation, while reference-keyed owners share one body reached by a constrained call the JIT devirtualizes when the exact type is known. The cost of the constraint plane over hand-written per-owner admission is zero on value-keyed paths and one devirtualizable indirection otherwise — never a dictionary of witnesses.
- The error-construction plane is itself static-abstract: forwarding `TError.Create(message)` through the constraint manufactures the exact error subtype the owner declared — never a base placeholder — entirely from the type parameter, so a structured custom error vocabulary is honored by a fully generic bridge that never names it.
- The general static-abstract `Validate` takes its raw value in a `TValue?` nullable form a byref-like type cannot inhabit, so span admission is not an instantiation of the general key path: the span route is a separately constrained entry pinned to `ReadOnlySpan<char>` against the span-factory shape (`T : IObjectFactory<T, ReadOnlySpan<char>, TError>`). An algorithm that must accept both a heap key and a span over one owner family declares two overloads mirroring this split — no single constraint spans both, by the nullable-form mechanics rather than by policy.
