# Admission Rails — Core

[GENERATED_OUTCOME_TRIPLE]:
- One projection of the three-outcome spine is rail-fit: `TError? Validate(TValue value, IFormatProvider? provider, out T? obj)` carries both the typed fault and the admitted instance in a single call. `Create` and `TryCreate` are generated on top of `Validate`; bridging either one re-derives information the spine already produced.
- `T Create(TValue)` throws `System.ComponentModel.DataAnnotations.ValidationException` constructed from `validationError.ToString()` — the typed fault object is erased to a string at the throw site. A catch-based bridge can never recover the fault case, making the exception path unfit for rail projection by construction.
- The generated explicit conversion operator from key type to owner compiles to `Create` — every interior cast from raw key to owner is a hidden throw site; casts from key type are boundary-only spellings.
- The two `TryCreate` overloads differ in evidence: the 2-arg form passes `out _` and discards the fault; the 3-arg form (`out T? obj, out TError? validationError`) calls `Validate` directly with `[NotNullWhen(true)]` on `obj` and `[NotNullWhen(false)]` on `validationError`. Flow analysis after `if`-free pattern projection is identical to a direct `Validate` call, so the 3-arg form adds nothing over `Validate` for a rail bridge.
- The generated `IParsable`/`ISpanParsable` surface routes through `Validate` and converts the fault to `FormatException` (message = `fault.ToString()`); `TryParse` drops the fault entirely. The parse surface exists for framework binding contracts; a domain bridge that enters through `Parse` voluntarily downgrades from typed fault to string.
- Keyed enumeration owners generate the same `Validate` triple plus `Get` (throws a dedicated unknown-identifier exception carrying the enum type and offending value) and `TryGet`. `Validate` accepts `[AllowNull] string` and `ReadOnlySpan<char>` overloads, so wire keys admit without allocation and a vocabulary miss arrives as the same typed fault family as any value-object rejection — one fault vocabulary spans range checks and bounded-vocabulary misses.
- For reference-typed keys the generated `Validate` rejects `null` before invoking user hooks, manufacturing the fault through the static `Create(string)` of the configured error type — user validation hooks never observe null, and the null-rejection message is owned by the fault family, not by call sites.

[ONE_HOP_PROJECTION]:
- The entire projection from generated outcome to rail is one expression: `Owner.Validate(raw, provider, out var item) is { } fault ? fault : item!` — the property pattern discriminates, and the rail's implicit conversions lift the result (`Validation<F, A>` lifts implicitly from both `F` and `A`; `Fin<A>` lifts implicitly from both `A` and the error base). A fault subtype lifts into `Validation<Error, T>` without a cast because the standard reference conversion to the error base composes with the user-defined implicit lift.
- The static-abstract factory contract `IObjectFactory<T, TValue, TError>` (implemented by every generated owner — enumerations and value objects alike) supports a single polymorphic bridge as a generic static extension block. Fixing the fault type in the constraint makes inference total — the owner binds from the receiver and the raw type binds from the argument:

```csharp
public static class AdmissionBridge
{
    extension<TOwner, TRaw>(TOwner)
        where TOwner : class, IObjectFactory<TOwner, TRaw, Fault>
        where TRaw : notnull
    {
        public static Validation<Error, TOwner> Admit(TRaw raw, IFormatProvider? provider = null) =>
            TOwner.Validate(raw, provider, out var owned) is { } fault ? (Error)fault : owned!;

        public static Fin<TOwner> AdmitFin(TRaw raw, IFormatProvider? provider = null) =>
            TOwner.Validate(raw, provider, out var owned) is { } fault ? FinFail<TOwner>(fault) : owned!;
    }
}
```

- Call sites read owner-dotted with zero type arguments — `Port.Admit(8080)`, `Host.AdmitFin("<value-a>")`, `Mode.Admit("<key-a>")` — so the bridge is one declaration for the whole domain. A leading-type-parameter generic method cannot achieve this because generic argument inference is all-or-nothing; the extension receiver is the only position from which the owner type infers.
- The null-suppression bang in the success arm is justified only by the generated contract (fault null ⇒ instance non-null). Owners configured to yield null for absent input break that contract deliberately, and the bridge for them is three-valued: fault ⇒ `Fail`, null instance ⇒ `None`, instance ⇒ `Some` — projecting into `Validation<Error, Option<T>>` or collapsing absence to a typed fault at the call site, but never the bang.
- `Fin<A>` failure construction in generic code uses the prelude constructor (`FinFail<A>(error)`); the nested `Fail`/`Succ` case records are types, not factory methods, and direct `new` on them is a longer spelling of the same value.
- Failure-side rewrites stay on the rail: `MapFail` re-codes a fault without leaving the carrier, `BindFail` substitutes a recovery admission, and `BiBind` folds both sides. A bridge that pattern-matches mid-pipeline to re-wrap errors reimplements these combinators.

[TYPED_FAULT_FAMILIES]:
- The validation-error contract is a covariant, class-constrained interface with one static abstract member, `static abstract TSelf Create(string message)`. The error-rail base for non-exceptional failures is a positional record `(string Message, int Code, Option<Error> Inner)`. A single record satisfies both contracts at once — one fault type that is simultaneously the generated owner's validation error and a first-class rail error, eliminating every per-call-site error-translation hop.
- Every message the generator itself authors (null argument, vocabulary miss, default-struct rejection) is manufactured through the family's static `Create(string)` — so the `Create` case is the family's textual catch-all, while validation hooks construct precise cases directly. Designing the family means designing two tiers: one string-bearing case for generator-authored text, N structured cases for hook-authored evidence.
- A closed union-shaped fault family works as a generated union whose base derives from the expected-error record. The analyzer rejects a primary constructor on the union base (it cannot be made private), so the base takes a private constructor that nested sealed cases chain through — nesting grants the cases access to the private constructor while sealing the family against foreign extension:

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

- The union generator emits exhaustive `Switch`/`Map` over the fault cases — fault handling inherits the same closed-dispatch guarantees as any domain union, so a recovery routine that omits a fault case is a compile error, not a logging gap.
- Fault identity has two distinct relations whose conflation is a live bug source. Record `Equals` is structural over all positional members (same code, different message ⇒ not equal; different runtime case type ⇒ not equal), while `Is` matches by code. `Is`, `HasCode`, `IsType<E>`, and `Filter<E>` form the recovery algebra; catch-style matching must go through `Is`/`HasCode`, never `==`.
- The error base is a monoid: `+` flattens into a many-errors carrier whose `Count`, `Head`, `Tail`, and `AsIterable` expose the flat stream. On aggregates, expectedness is conjunctive and exceptionality is disjunctive (all-expected ⇒ expected; any-exceptional ⇒ exceptional), so one exceptional infiltrator flips the whole aggregate's disposition — triage gates on `IsExceptional` before reading fault cases.
- `Filter<E>()` reaches through the aggregate and returns only faults of the requested subtype (empty aggregate when none match). Typed faults survive accumulation and remain recoverable after union with foreign errors, which licenses `Error` as the universal failure currency.

[APPLICATIVE_ACCUMULATION]:
- Multi-field admission composes applicatively at the bridge — every field admits independently and all faults surface in one pass. Both the combinator spelling `fun(ctor).Map(va).Apply(vb).As()` and the operator spelling `*` compose the same value; the operator form accepts uncurried multi-parameter functions directly on either side:

```csharp
var admitted = (fun((Host h, Port p) => new Endpoint(h, p)) * Host.Admit(rawHost) * Port.Admit(rawPort)).As();
Fin<Endpoint> collapsed = admitted.ToFin();
```

- Monadic composition short-circuits at the first fault — comprehension syntax encodes data dependency (later admissions read earlier results), applicative syntax encodes independence (all faults wanted). Choosing between them is a statement about the dependency graph of the fields, not a style preference.
- The failure-type semigroup is resolved at runtime by reflecting for the trait instance, and the miss mode is silent: when the failure type does not implement the semigroup trait, `Apply` and the accumulating `&` operator keep only the first failure and discard the rest — no exception on the apply path. Deriving from the error base does not help because the instance lookup is exact in the failure type (an inherited `Semigroup<ErrorBase>` is not a `Semigroup<TFault>`). Two designs are sound: carry `Error` as the failure type (faults lift implicitly, the many-errors monoid accumulates, `Filter<TFault>` recovers), or implement the semigroup on the fault family itself with an aggregate case so accumulation stays typed end-to-end.
- The trait-instance miss has loud paths for identity operations: alternative or monoid-empty contexts throw a must-be-a-monoid error for failure types without a monoid instance. A fault family that opts into typed accumulation but participates in choice or empty contexts needs the full monoid (an identity fault), not just the semigroup.
- Cross-field refinement that must run after independent admission threads as a guard inside the comprehension — `from _ in guard(predicate, (Error)fault)` — keeping refinement on the rail without a second validation surface; the accumulating carrier binds guards and failure values natively.
- Conversion is directional by design: the accumulating carrier collapses to the sequential one (`ToFin` exists for the error-typed accumulator) but there is no inverse. The bridge produces the accumulator; collapse happens at the point where the flow becomes sequential.
- Absence converts to either rail at the bridge with an explicit fault: option-to-rail projections take the fault value or a fault thunk (`ToFin(fault)`, `ToValidation<F>(() => fault)`) — absence becomes a typed fault exactly where absence is illegal and stays optional where it is legal.

[SENTINEL_NORMALIZATION]:
- Normalization belongs inside admission, not before it. The generated factory hook receives the raw value by `ref` (`static partial void ValidateFactoryArguments(ref TError? validationError, ref TValue value)`), so trimming, canonicalization, and clamping mutate the value pre-storage and the owner's key is the normalized form — call sites cannot admit an unnormalized value because the hook runs on every factory path.
- String owners opt into absent-as-null semantics via `EmptyStringInFactoryMethodsYieldsNull`: with that knob set, whitespace-only input returns null fault and null instance from `Validate` (generated as an early is-null-or-whitespace return) — blank input is absence, not a fault, and the bridge projects it to `None`. Setting `EmptyStringInFactoryMethodsYieldsNull` implies `NullInFactoryMethodsYieldsNull`; both are inert for struct owners and struct keys.
- Struct owners deny `default` by default (explicit `default`/parameterless construction is rejected unless `AllowDefaultStructs` is set), the `IDisallowDefaultValue` marker interface advertises the policy to infrastructure, and the canonical-empty property name is configurable — the default-value sentinel is unrepresentable rather than checked-for downstream.
- A trim-or-nullify string extension (null/empty/whitespace ⇒ null, else trimmed, optional max-length) exists precisely for pre-admission hygiene at DTO edges where `EmptyStringInFactoryMethodsYieldsNull` is not in play.
- Host-API sentinels (zero IDs, origin points, min-dates) are not the generated owner's concern: they normalize to `Option` at the seam before admission, then convert to a typed fault via the option-to-rail projections only where the value is mandatory — two distinct decisions (is it absent? is absence legal here?) that collapse into one bug when sentinels reach the validation hook.

[NO_DOUBLE_VALIDATION]:
- The generated owner is constructively unrevalidatable: the constructor is private, complex-owner members must have private `init` accessors (a public `init` is a compile error via analyzer), and `Validate` accepts only the raw key type — interior code holding `T` cannot re-run admission because no surface accepts an already-admitted value. The no-double-validation law is a type-system fact, not a convention.
- The two hooks split by capability: the constructor hook (`ValidateConstructorArguments`) can normalize but has no error channel and cannot produce a validation fault; the factory hook owns rejection. Invariants that must hold even for deserialization-constructed instances go in the constructor hook; faults go only in the factory hook. `FactoryPostInit` runs after successful admission for derived-state initialization.
- Skipping factory methods cascades: no type converter, no factory-contract implementation, no key-to-owner conversion operator, parse interfaces forced off, arithmetic operator generation forced off. An owner without factories is a pure interior value that can only be born from other admitted values.
- Factory method names are configurable on the owner attribute — the admission verb can match domain vocabulary repo-wide without wrappers, which removes the last excuse for a renaming bridge layer.

[DTO_TRANSLATION]:
- An additional admission source type is declared on the owner (object-factory attribute with the foreign value type); the owner implements the same `Validate` triple for the foreign type, and the correct implementation parses at the edge and delegates into the generated member-wise `Validate` — one validation spine serves N wire grammars, and the wire grammar can never drift from the member rules because it terminates in them:

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

- The factory attribute's framework toggles (serialization frameworks, model binding, ORM) route framework-driven deserialization through the same factory — wire admission and domain admission are one path, so a payload that deserializes is a payload that validated, and the no-double-validation law extends across the process boundary. The `UseCorrespondingConstructor` flag short-circuits factory invocation for trusted rehydration paths.
- DTO-to-owner translation under accumulation is the applicative composition of per-field bridges — the DTO never grows a `Validate` of its own. It is a record of raw values whose admission is the applicative expression, which keeps validation logic in exactly one place per field and makes the DTO disposable at the boundary.
- Span-keyed `Validate` overloads on keyed owners admit directly from wire buffers without intermediate strings — the zero-allocation admission path for hot deserialization seams, with the same typed fault on rejection.
