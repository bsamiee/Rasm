# Admission Rails — Bedrock

[GENERATED_OUTCOME_TRIPLE]:
- One projection of the three-outcome spine is rail-fit: `TError? Validate(TValue value, IFormatProvider? provider, out T? obj)` carries both the typed fault and the admitted instance in a single call. `Create` and `TryCreate` are generated on top of `Validate`; bridging either one re-derives information the spine already produced.
- `T Create(TValue)` throws `System.ComponentModel.DataAnnotations.ValidationException` constructed from `validationError.ToString()` — the typed fault object is erased to a string at the throw site. A catch-based bridge can never recover the fault case, making the exception path unfit for rail projection by construction.
- The generated explicit conversion operator from key type to owner compiles to `Create` — every interior cast from raw key to owner is a hidden throw site; casts from key type are boundary-only spellings.
- The two `TryCreate` overloads differ in evidence: the 2-arg form passes `out _` and discards the fault; the 3-arg form (`out T? obj, out TError? validationError`) calls `Validate` directly with `[NotNullWhen(true)]` on `obj` and `[NotNullWhen(false)]` on `validationError`. Flow analysis after `if`-free pattern projection is identical to a direct `Validate` call, so the 3-arg form adds nothing over `Validate` for a rail bridge.
- The generated `IParsable`/`ISpanParsable` surface routes through `Validate` and converts the fault to `FormatException` (message = `fault.ToString()`); `TryParse` drops the fault entirely. The parse surface exists for framework binding contracts; a domain bridge that enters through `Parse` voluntarily downgrades from typed fault to string.
- Keyed enumeration owners generate the same `Validate` triple plus `Get` (throws a dedicated unknown-identifier exception carrying the enum type and offending value) and `TryGet`. `Validate` accepts `[AllowNull] string` and `ReadOnlySpan<char>` overloads, so wire keys admit without allocation and a vocabulary miss arrives as the same typed fault family as any value-object rejection — one fault vocabulary spans range checks and bounded-vocabulary misses.
- For reference-typed keys the generated `Validate` rejects `null` before invoking user hooks, manufacturing the fault through the static `Create(string)` of the configured error type — user validation hooks never observe null, and the null-rejection message is owned by the fault family, not by call sites.
- No factory path guards the user hooks: the generated `Validate` invokes `ValidateFactoryArguments(ref validationError, ref value)` bare, `TryCreate` delegates to `Validate`, and `Create` only converts the returned fault to a throw. A hook that throws detonates every admission surface including the nominally non-throwing `TryCreate`. The error channel is the only safe failure path; a hook wrapping a throwing parser must absorb the exception internally and convert it to the fault family.
- `Create` and `TryCreate` hard-code `null` as the `IFormatProvider` argument when delegating to `Validate` — only a direct `Validate` call and the generated parse surface ever thread a real provider. Culture-sensitive normalization in hooks must treat a null provider as the invariant default; a rail bridge that enters through `Validate` is the only admission spelling that can carry culture.
- Complex owners emit one null guard per non-nullable reference member, ahead of the hook, in member declaration order, each returning immediately with a fault manufactured through the family's `Create(string)`. Admission inside a single owner is short-circuit by construction, at both the null tier and the hook tier (one `ref` error slot). Accumulation has exactly two homes: inside the hook by folding into the family's aggregate case, or across owners at the bridge. The generated spine itself never accumulates — the structural argument for pushing admission down to leaf owners is that independence between faults must be reified as separate owners before the applicative bridge can surface them together.
- The `EmptyStringInFactoryMethodsYieldsNull` knob deletes `[NotNullWhen(true)]` from both generated `TryCreate` overloads — under that knob `TryCreate(...) == true` no longer proves a non-null instance, so the boolean surface inherits the three-valued contract without any signature change. The `NullInFactoryMethodsYieldsNull` knob instead decorates `Create` with `[return: NotNullIfNotNull(...)]`, which keeps flow analysis exact. The two knobs thus differ not only in semantics but in how much of the contract survives in annotations — a bridge generated-contract audit reads the attributes, not the docs.
- When the key argument is named `provider`, the generator renames the format-provider parameter to `formatProvider`; named-argument call sites using `provider:` then bind the raw key. Bridges pass positionally and never notice; reflective or named-argument callers can silently swap roles.

[ONE_HOP_PROJECTION]:
- The entire projection from generated outcome to rail is one expression: `Owner.Validate(raw, provider, out var item) is { } fault ? fault : item!` — the property pattern discriminates, and the rail's implicit conversions lift the result (`Validation<F, A>` lifts implicitly from both `F` and `A`; `Fin<A>` lifts implicitly from both `A` and the error base). A fault subtype lifts into `Validation<Error, T>` without a cast because the standard reference conversion to the error base composes with the user-defined implicit lift.
- The static-abstract factory contract `IObjectFactory<T, TValue, TError>` (implemented by every generated owner — enumerations and value objects alike) supports a single polymorphic bridge as a generic static extension block. Fixing the fault type in the constraint makes inference total — the owner binds from the receiver and the raw type binds from the argument:

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

- Call sites read owner-dotted with zero type arguments — `Port.Admit(8080)`, `Host.AdmitFin("<value-a>")`, `Mode.Admit("<key-a>")` — so the bridge is one declaration for the whole domain. A leading-type-parameter generic method cannot achieve this because generic argument inference is all-or-nothing; the extension receiver is the only position from which the owner type infers.
- The null-suppression bang in the success arm is justified only by the generated contract (fault null ⇒ instance non-null). Owners configured to yield null for absent input break that contract deliberately, and the bridge for them is three-valued: fault ⇒ `Fail`, null instance ⇒ `None`, instance ⇒ `Some` — projecting into `Validation<Error, Option<T>>` or collapsing absence to a typed fault at the call site, but never the bang.
- `Fin<A>` failure construction in generic code uses `Fin.Fail<A>(error)`; `Prelude.Fail<E>(error)` returns the `Fail<E>` carrier struct (not `Fin<A>`), and `Prelude.FinFail<A>` is marked `[Obsolete]` in favour of `Fin.Fail`. Target-typed `Fail(error)` resolves to `Fin<A>` via the implicit `Fail<Error>` → `Fin<A>` conversion only where the assignment context provides the type; in generic code without that context `Fin.Fail<A>(error)` is the unambiguous spelling. The nested `Fail`/`Succ` case records are types, not factory methods, and direct `new` on them is a longer spelling of the same value.
- Failure-side rewrites stay on the rail: `MapFail` re-codes a fault without leaving the carrier, `BindFail` substitutes a recovery admission, and `BiBind` folds both sides. A bridge that pattern-matches mid-pipeline to re-wrap errors reimplements these combinators.
- The factory contract declares its raw-value parameter as `TValue : notnull, allows ref struct` — the admission interface itself admits byref-like raw types. A generic rail bridge whose raw-type parameter omits `allows ref struct` silently narrows the contract and excludes every span-keyed admission overload from the polymorphic path; the anti-constraint must be mirrored on the bridge's type parameter. The raw value is consumed inside the call, so the rail carrier never needs to hold the span — only the parameter position is byref-like.
- The factory contract is covariant in its error parameter (`out TValidationError`), and `IValidationError<out T>` is itself covariant. C# satisfies generic constraints through variant interface conversions even when the interface carries static abstract members — an owner implementing the contract with a derived fault case satisfies a bridge constrained on the fault base, and the static-abstract `Validate` call dispatches to the owner's implementation with its result viewed as the base. One base-constrained bridge therefore serves a fault lattice, not a single fault type: each owner declares the most precise case of the family, while the bridge, carriers, and recovery vocabulary uniformly see the base. The precise derived case must re-satisfy the validation-error contract itself (a shadowing `static new Create` returning the derived type), because the `[ValidationError<T>]` attribute constraint is self-referential and checked at definition time — a malformed lattice member fails before generation runs.

[TYPED_FAULT_FAMILIES]:
- The validation-error contract is a covariant, class-constrained interface with one static abstract member, `static abstract TSelf Create(string message)`. The error-rail base for non-exceptional failures is a positional record `(string Message, int Code, Option<Error> Inner)`. A single record satisfies both contracts at once — one fault type that is simultaneously the generated owner's validation error and a first-class rail error, eliminating every per-call-site error-translation hop.
- Every message the generator itself authors (null argument, vocabulary miss) is manufactured through the family's static `Create(string)` — so the `Create` case is the family's textual catch-all, while validation hooks construct precise cases directly. Designing the family means designing two tiers: one string-bearing case for generator-authored text, N structured cases for hook-authored evidence. Because the generator manufactures its text through the configured error type's `Create`, per-owner derived cases also partition generator text by owner — the one population a shared family can never attribute without the derived-case lattice pattern.
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

- When the per-owner precision pattern is used, derived cases embed in the shared family as nested members with their own `IValidationError<TDerived>` implementation; the shared base's `[Union]`-generated dispatch includes them as leaf arms, so closed-dispatch guarantees apply at every tier of the lattice:

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

- The union generator emits exhaustive `Switch`/`Map` over the fault cases — fault handling inherits the same closed-dispatch guarantees as any domain union, so a recovery routine that omits a fault case is a compile error, not a logging gap. The partial-overloads setting (`DefaultWithPartialOverloads`) adds partial variants with optional per-case handlers and one mandatory default arm — closed-world recovery and open-world triage from a single declaration, with the closed form still compile-breaking on a new fault case. Fault hierarchies deeper than two levels use `[UnionSwitchMapOverload(StopAt = new[] { typeof(IntermediateCase) })]` to generate stop-at overloads: a severity tier — recoverable versus fatal as intermediate abstract cases — dispatches as two arms at the boundary and leaf-exhaustively in interior recovery, as overloads of the same method.
- Fault identity has two distinct relations whose conflation is a live bug source. Record `Equals` is structural over all positional members (same code, different message ⇒ not equal; different runtime case type ⇒ not equal), while `Is` matches by code. `Is`, `HasCode`, `IsType<E>`, and `Filter<E>` form the recovery algebra; catch-style matching must go through `Is`/`HasCode`, never `==`.
- The catch-value overload (`catch(E error, ...)`) and the typed-subtype `catch<E>` differ fundamentally: the value form matches by the generic failure type's `Equals` (structural for records — case type and every member must coincide), while the `Error`-specialized typed form matches by `e is E || e.IsType<E>()` (aggregate-recursive type test). The same catch expression changes meaning when a seam widens the failure type from a typed family to the error base — the predicate form and the integer-code form are the only relation-stable spellings across that widening.
- Typed-subtype `catch<E>` on an aggregate failure filters to matching members and invokes the handler via `ForAllM` — every per-member recovery binds monadically and the last one's value wins. The `FoldM` variant (via separate overload) composes per-member recoveries through first-success choice instead. Handlers that perform one substitute admission per fault are last-write-wins under `ForAllM`; batch-aware recovery matches the aggregate explicitly with a predicate over count and membership, or extracts typed members once and folds deliberately.
- The error base is a monoid: `+` flattens into a many-errors carrier whose `Count`, `Head`, `Tail`, and `AsIterable` expose the flat stream. On aggregates, expectedness is conjunctive and exceptionality is disjunctive (all-expected ⇒ expected; any-exceptional ⇒ exceptional), so one exceptional infiltrator flips the whole aggregate's disposition — triage gates on `IsExceptional` before reading fault cases.
- `Filter<E>()` reaches through the aggregate and returns only faults of the requested subtype (empty aggregate when none match). Typed faults survive accumulation and remain recoverable after union with foreign errors, which licenses `Error` as the universal failure currency.
- Catch-style matching on the error base is two-mode: a non-zero code makes `Is` compare codes and ignore messages (localization-stable matching by design); a zero code degrades `Is` to message-string equality. A fault family that leaves the code at its default has opted into string-matching recovery without noticing — every structured case claims a non-zero code, and the string-bearing generator-text case is the only tolerable zero-code member. The error base lifts implicitly from a code-and-message tuple but only explicitly from a bare string: code-bearing minting is the path of least resistance by conversion design, and every code-zero fault requires a visible cast to create. Provenance-threading mint overloads take an inner error alongside message or code-message — the canonical field-labeling move at an accumulating bridge is wrapping each field's fault as the inner of a field-naming fault before combination, so the flat aggregate of a multi-field admission remains attributable per field without positional reconstruction.
- The runtime reserves a contiguous negative code band for control-signal errors: cancelled (−2000000001), timed out (−2000000002), sequence empty (−2000000003), closed (−2000000004), parse (−2000000005), end of stream (−2000000010), validation failed (−2000000011), and several IO-trait gaps — two distinct reserved errors share code −2000000008. Domain fault codes must be allocated outside this band; code-keyed recovery is integer equality, and a collision makes a domain fault impersonate a cancellation or timeout to every generic handler upstream.
- Exception-captured faults take the exception's `HResult` as their code — the code space of exceptional errors is the host's HRESULT space, not the domain's. Code-keyed recovery distinguishes the two populations by expectedness first; a numeric code only means what its side of the expected/exceptional split says it means.
- Aggregates recurse every membership predicate (`Is`, code possession, type possession, exception possession) over their members but expose their own fixed code as a property. An aggregate never satisfies code-possession for its own aggregate code unless a member carries it. Recovery must never key on the aggregate marker code; it keys on member evidence reached through the recursive predicates or through typed extraction.

[FACTORY_CONTRACT_SURFACE]:
- Declaring an additional admission source generates only wiring: the owner-marker interface, the typed factory interface, conditionally the outbound-conversion interface, and one runtime metadata row (value type, validation-error type, framework flags, optional constructor-conversion expression). The `Validate` body is compelled by the static abstract and authored by the owner. The owner kind is unrestricted: keyed and keyless enumerations, simple and complex value objects, ad-hoc unions, and regular union roots all take the attribute.
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

- The factory-source attribute's own type parameter declares `allows ref struct`, so an owner may declare a span-of-char admission grammar directly; the compelled static abstract then admits UTF-16 wire buffers with the typed fault on rejection and zero string materialization on the rejection path. Declaring the span grammar upgrades the generated span-parse surface for free: span `Parse` routes through the declared span `Validate` (throwing a format exception built from the fault's text), and span `TryParse` routes through it discarding the fault — the framework binding path becomes zero-copy as a side effect of the domain grammar. Generated span admission exists without declaration only on keyed enumerations; keyed value objects never receive a generated span surface because their admission constructs rather than looks up — span admission for constructed owners is the declared-grammar route, and the grammar decides where the one unavoidable string materialization happens (after structural rejection checks, not before).
- Two-way conversion is not opt-in by method but by consequence: any serialization framework flag or persistence flag on the factory attribute forces the outbound interface, whose `ToValue()` the owner must implement — the round-trip law (admit after project is identity modulo normalization) becomes a compile-visible obligation the moment the owner faces a wire.
- The trusted-rehydration flag declares a constructor accepting the foreign value, letting materialization from already-admitted storage skip `Validate` entirely; it is rejected on enumerations (their identity map cannot be bypassed). This is the single sanctioned no-validation path, and its scope is precisely data that passed admission before persistence — using it for any other ingress reintroduces the unvalidated interior the whole architecture exists to prevent.
- Multiple factory attributes on one owner are analyzer-partitioned: overlapping serialization frameworks, duplicate model-binding claims, and duplicate persistence claims across attributes are compile errors — each framework concern resolves to exactly one admission grammar, so wire routing is total and unambiguous at definition time.

[APPLICATIVE_ACCUMULATION]:
- Multi-field admission composes applicatively at the bridge — every field admits independently and all faults surface in one pass. Both the combinator spelling `fun(ctor).Map(va).Apply(vb).As()` and the operator spelling `*` compose the same value; the operator form accepts uncurried multi-parameter functions directly on either side:

```csharp
var admitted = (fun((Host h, Port p) => new Endpoint(h, p)) * Host.Admit(rawHost) * Port.Admit(rawPort)).As();
Fin<Endpoint> collapsed = admitted.ToFin();
```

- The tuple spelling is a second accumulating form: `(Host.Admit(h), Port.Admit(p)).Apply(static (a, b) => new Endpoint(a, b))` — the extension `K<AF, Func<A,B,C>>` `.Apply` auto-curries via `curry` and applies sequentially, at arities up to 10. The tuple form binds the trait-level applicative and carries the same semigroup-resolution behavior — with the error base as failure currency it accumulates; with a typed family lacking its own trait instance it silently keeps the first fault. A fault family intended for tuple-apply composition must carry its own semigroup instance or the N-field bridge degrades to first-fault reporting with no diagnostic.
- Monadic composition short-circuits at the first fault — comprehension syntax encodes data dependency (later admissions read earlier results), applicative syntax encodes independence (all faults wanted). Choosing between them is a statement about the dependency graph of the fields, not a style preference.
- The failure-type semigroup is resolved at runtime by reflecting for the trait instance, and the miss mode is silent on the hot path: when the failure type does not implement the semigroup trait, `Apply` keeps only the first failure (the function-carrier's fault) and discards the right operand's fault — no exception on the apply path. Only the identity operations (alternative empty, monoid-unit contexts) fail loud with an unsupported-operation throw naming the missing monoid. Deriving from the error base does not help because the instance lookup is exact in the failure type (an inherited `Semigroup<ErrorBase>` is not a `Semigroup<TFault>`). Two designs are sound: carry `Error` as the failure type (faults lift implicitly, the many-errors monoid accumulates, `Filter<TFault>` recovers), or implement the semigroup on the fault family itself with an aggregate case so accumulation stays typed end-to-end.
- Which overload binds is decided by the static type of the operands: values typed as the concrete carrier bind the runtime-resolved operators, values typed as the trait-kinded abstraction bind the compile-constrained ones. The same expression text can be constraint-checked or constraint-erased depending on whether an intermediate variable was declared as the concrete type — a reason to let the bridge return the concrete carrier and keep composition in one expression.
- The unary `+` operator on the trait-kinded carrier is the downcast to the concrete type — operator chains stay concrete without `.As()` ceremony; `>>` sequences dependent admissions and a `>>` with a lowering marker closes a trait-typed pipeline back to the concrete carrier.
- The public construction surface (`Success`, `Fail`, `Empty`) demands the full monoid at compile time, and the sequence-accepting `Fail` folds a precomputed fault batch through the monoid before wrapping — a batch of faults collected out-of-band enters the rail as one combined failure, not as a sequence bolted beside the carrier.
- Cross-field refinement that must run after independent admission threads as a guard inside the comprehension — `from _ in guard(predicate, (Error)fault)` — keeping refinement on the rail without a second validation surface; the accumulating carrier binds guards and failure values natively.
- The carrier lattice is bidirectional and lossless in both directions because the error base is the monoid: collapsing the accumulator to the sequential carrier via `ToFin` packs the accumulated aggregate into the single error slot intact (flat member stream, counts, typed extraction all survive), and the sequential carrier lifts back to the accumulator via `Match` over the implicit conversions — both error and value lift implicitly into `Validation<Error, T>`. Direction is a statement about where accumulation continues, not about evidence retention.
- Absence converts to either rail at the bridge with an explicit fault: option-to-rail projections take the fault value or a fault thunk (`ToFin(fault)`, `ToValidation<F>(() => fault)`) — absence becomes a typed fault exactly where absence is illegal and stays optional where it is legal.

[SENTINEL_NORMALIZATION]:
- Normalization belongs inside admission, not before it. The generated factory hook receives the raw value by `ref` (`static partial void ValidateFactoryArguments(ref TError? validationError, ref TValue value)`), so trimming, canonicalization, and clamping mutate the value pre-storage and the owner's key is the normalized form — call sites cannot admit an unnormalized value because the hook runs on every factory path.
- String owners opt into absent-as-null semantics via `EmptyStringInFactoryMethodsYieldsNull`: with that knob set, whitespace-only input returns null fault and null instance from `Validate` (generated as an early is-null-or-whitespace return) — blank input is absence, not a fault, and the bridge projects it to `None`. Setting `EmptyStringInFactoryMethodsYieldsNull` implies `NullInFactoryMethodsYieldsNull`; both are inert for struct owners and struct keys.
- Struct owners deny `default` by default (explicit `default`/parameterless construction is rejected unless `AllowDefaultStructs` is set), the `IDisallowDefaultValue` marker interface advertises the policy to infrastructure, and the canonical-empty property name is configurable — the default-value sentinel is unrepresentable rather than checked-for downstream.
- Struct owners that deny default values receive the `IDisallowDefaultValue` marker interface from the generator, but enforcement is analyzer-only and syntactic: explicit `default` expressions and parameterless construction are compile errors, while generic type parameters, array allocation, and uninitialized fields manufacture zero-ghosts the analyzer cannot see — and the generator emits no runtime initialization check anywhere in the construction or access paths. The runtime gate therefore belongs to the seam that receives possibly-defaulted storage: infrastructure detecting `IDisallowDefaultValue` inspects the key member directly (null for reference keys, zero-pattern for value keys) and rejects with a typed fault before the ghost crosses inward. Reading the key beats calling equality against a default instance, because generated equality routes through key comparers whose null handling is comparer-specific. Deserializers and materializers that construct structs without factories are the dominant ghost source; routing them through the declared admission source type closes the hole at definition time, which is the stronger fix.
- A trim-or-nullify string extension (null/empty/whitespace ⇒ null, else trimmed, optional max-length) exists precisely for pre-admission hygiene at DTO edges where `EmptyStringInFactoryMethodsYieldsNull` is not in play.
- Host-API sentinels (zero IDs, origin points, min-dates) are not the generated owner's concern: they normalize to `Option` at the seam before admission, then convert to a typed fault via the option-to-rail projections only where the value is mandatory — two distinct decisions (is it absent? is absence legal here?) that collapse into one bug when sentinels reach the validation hook.
- Inbound key-to-owner conversion — the hidden throwing admission spelling — is a definition-time policy: explicit by default (`ConversionFromKeyMemberType = Explicit`), configurable to none, and forced to none when factories are skipped because the operator body delegates to the factory. Setting it to none erases the cast-admission spelling from the entire codebase, leaving the rail bridge as the only inbound spelling that compiles. Outbound owner-to-key projection generates as two operators: a safe null-propagating nullable-to-nullable conversion with a `[return: NotNullIfNotNull]` annotation (implicit by default), and for reference owners with non-nullable keys an additional unsafe non-nullable conversion that throws a bare null-reference exception on a null owner (explicit by default) — struct owners with non-nullable keys instead receive a no-throw non-nullable overload. The safe implicit operator converts an absent owner into an absent key silently; optional domain values project outward by mapping over the option to the key, never by flowing a nullable reference through the implicit conversion.

[NO_DOUBLE_VALIDATION]:
- The generated owner is constructively unrevalidatable: the constructor is private, complex-owner members must have private `init` accessors (a public `init` is a compile error via analyzer), and `Validate` accepts only the raw key type — interior code holding `T` cannot re-run admission because no surface accepts an already-admitted value. The no-double-validation law is a type-system fact, not a convention.
- The two hooks split by capability: the constructor hook (`ValidateConstructorArguments`) can normalize but has no error channel and cannot produce a validation fault; the factory hook owns rejection. Invariants that must hold even for deserialization-constructed instances go in the constructor hook; faults go only in the factory hook. `FactoryPostInit` runs after successful admission for derived-state initialization.
- Skipping factory methods cascades: no type converter, no factory-contract implementation, no key-to-owner conversion operator, parse interfaces forced off, arithmetic operator generation forced off. An owner without factories is a pure interior value that can only be born from other admitted values.
- Factory method names are configurable on the owner attribute — the admission verb can match domain vocabulary repo-wide without wrappers, which removes the last excuse for a renaming bridge layer.

[VOCABULARY_LOOKUP_PHYSICS]:
- String-keyed vocabulary admission is case-insensitive by default: the generator hard-codes the ordinal-ignore-case string comparer when no key-member equality comparer is declared. Wire keys differing only in case admit the same canonical item, and the admitted instance carries its declaration-time key, not the wire spelling — vocabulary admission is simultaneously case normalization, with no hook involved.

[FALLBACK_GRAMMARS]:
- Alternative admission grammars have two composition spellings with fundamentally different semantics. `|` routes through `Choose` with eagerly evaluated operands — both admissions run before the result is selected, and on total failure only the left fault is returned (the right result is discarded). Lazy right-side evaluation is `||`: the carrier defines `operator true` (`IsSuccess`) and `operator false` (`IsFail`), so C# evaluates `a || b` as "if `a` is already a success, return `a` without evaluating `b`; otherwise evaluate `b` then apply `a | b`" — first-success with deferred right admission. Semigroup combination (`.Combine`, which routes through `SemigroupK<Validation<FAIL>>.Combine`) is failure-dominant: if either operand is a failure the result is a failure, with faults accumulated through the error type's semigroup — `Fail.Combine(Success)` and `Success.Combine(Fail)` both yield `Fail`. The `||` spelling reads as "defer the fallback grammar"; the `|` spelling reads as "the canonical grammar's rejection is the diagnosis"; the combining spelling is accumulation across two independent grammars where any admission miss is a fault:

```csharp
Validation<Error, Endpoint> first = Endpoint.Admit(text) | Endpoint.AdmitCompact(text);
Validation<Error, Endpoint> total = Endpoint.Admit(text).Combine(Endpoint.AdmitCompact(text));
```

- The carrier implements the predicate-gated catch trait, so effect-world recovery vocabulary composes directly on admission results: a catch value on the right of `|` substitutes a recovery admission only for matching faults, with non-matching faults passing through unchanged — typed-fault triage without leaving the rail or writing a match.
- `BindFail` substitutes an entire recovery admission for the failure side and may simultaneously re-type the failure (the target failure type carries its own monoid obligation); `MapFail` re-codes faults in place, and its most common bridge use is lifting a typed family into the error-base currency once, immediately before joining streams that accumulate heterogeneous faults.

[EVIDENCE_FREE_FAILURES]:
- `Filter` and `Where` on the accumulating carrier manufacture the failure monoid's identity as the rejection value — for the error base that is the empty aggregate: a failure with zero members, count zero, empty flag set, expectedness vacuously true, and a fixed aggregate code. A `where` clause inside an admission comprehension converts predicate failure into evidence-free rejection that no code-keyed, type-keyed, or membership-keyed recovery will ever match; cross-field refinement must be spelled as a guard with an explicit fault, and the lazy-thunk guard overload defers fault construction to the rejection path.
- The empty aggregate doubles as the miss-marker of typed-fault extraction: filtering an error for a fault subtype returns it when nothing matches. Triage that counts faults or folds over members must gate on emptiness first — an empty aggregate is still a failure, and treating "no matching faults" as "no failure" inverts the rail.
- The bottom error exists for value-less evaluation states and its diagnostic text names uninitialized structs and filtered-out expressions as causes — the rail has a designated citizen for "no evidence" failures, and admission code must never mint a second one.

[EXCEPTION_CHANNEL_FIDELITY]:
- Typed faults round-trip exception-only channels with reference fidelity: throwing an expected fault produces a wrapper exception that holds the fault instance, and the capture constructor on the error base unwraps any of the wrapper exceptions back to the original object — a derived fault record crosses a boundary that only speaks throw or catch and arrives as the same case, same payload, no serialization. The catch-side spelling is always the capture constructor, never direct exceptional construction, because only the former performs the unwrap.
- The capture constructor is a normalization tree: cancellation exceptions (both task and operation flavors) collapse to the reserved cancellation error, timeouts to the reserved timeout, aggregate exceptions flatten into the many-errors carrier, and only the remainder becomes exceptional. Boundary capture auto-classifies host control-flow exceptions into the reserved vocabulary; catch-side code that re-wraps a cancellation as a domain fault fights the rail's own taxonomy.
- The exception mirror of the error algebra is itself a monoid (combinable wrappers, a many-exceptions carrier, an empty), and every wrapper writes the fault code into the exception's `HResult` — foreign frames that know nothing of the rail can still key on the domain code through the standard exception surface.
- The wire serialization contract of the error base is exact: `Expected` serializes only `Message` and `Code` (`Inner` is `[IgnoreDataMember]`); `Exceptional` serializes only `Message` and `Code` (the wrapped exception field is populated only in-process via a non-serialized constructor path); `ManyErrors` serializes its member list. Inner-chained provenance is process-local by declared contract, not by serializer accident — the field-labeling chain from the bridge survives precisely until the wire, where each labeling fault keeps its own code and message and loses its chain. Because derived fault cases are polymorphic records over the base, a boundary serializer bound to the static base type erases case identity and structured payloads; codes must be self-sufficient for remote triage, and structured case payloads and inner chains are in-process diagnosis material only.
- Exceptional errors deliberately drop their captured exception on deserialization (cross-process leak prevention): stack fidelity is process-local, and the in-process throw path preserves the original stack via dispatch-info capture while the rehydrated path throws a synthetic carrier. Fault payloads that must survive process hops carry their evidence in the expected-fault record, never in the wrapped exception.
- For admission surfaces not under generated control — foreign factories that throw — the lazy capture monad is the one-hop projection: a record around a result-thunk whose lift wraps the computation and whose run applies the capture constructor's normalization to whatever escapes. Its combination operator accumulates on failure, giving foreign-grammar fallback the same evidence semantics as native accumulation. Generated owners need none of this; the capture monad exists exactly for admission code that cannot be made total.

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

- The factory attribute's framework toggles (serialization frameworks, model binding, ORM) route framework-driven deserialization through the same factory — wire admission and domain admission are one path, so a payload that deserializes is a payload that validated, and the no-double-validation law extends across the process boundary. The `HasCorrespondingConstructor` flag declares that a constructor accepting the raw type exists; the generator routes trusted rehydration through that constructor and skips factory invocation.
- DTO-to-owner translation under accumulation is the applicative composition of per-field bridges — the DTO never grows a `Validate` of its own. It is a record of raw values whose admission is the applicative expression, which keeps validation logic in exactly one place per field and makes the DTO disposable at the boundary.
- Generated span `Validate` overloads exist natively only on keyed enumerations (`[AllowNull] ReadOnlySpan<char>` overload generated alongside the string overload); keyed value objects do not receive a generated span surface — their admission constructs rather than looks up, so span admission for them requires a declared span grammar on the factory attribute, as stated in the factory-contract section. In both cases the wire path is zero-allocation on rejection, with the typed fault on miss.

[TWO_PHASE_COMPOSITION]:
- Layered admission has a canonical two-phase shape: leaf owners admit raw fields applicatively (independent, all faults surface), and the composite owner — whose generated member-wise `Validate` accepts already-admitted leaf types — binds monadically on top, because composite admission is data-dependent on leaf success and can only fail on cross-member invariants. The applicative stage yields a nested carrier; flattening is the join between phases, and a guard carries any post-construction refinement:

```csharp
public static Validation<Error, Window> Admit(RawWindow raw) =>
    from window in (fun((Lo lo, Hi hi) => Window.Admit(lo, hi)) * Lo.Admit(raw.Lo) * Hi.Admit(raw.Hi)).Flatten()
    from _ in guard(window.Span > 0, () => (Error)Fault.Create($"degenerate window: {window}"))
    select window;
```

- The carrier lattice is bidirectional and lossless because the error base is the monoid: collapsing the accumulator via `ToFin` packs the accumulated aggregate into the single error slot intact (flat member stream, counts, typed extraction all survive), and the sequential carrier lifts back to the accumulator with the aggregate re-exposed via the implicit conversions. Collapse at the frontier where flow becomes sequential; lift back where a later seam resumes parallel admission.
- The sequential carrier's terminal boundary spelling is the throw-if-failed projection, which rethrows through dispatch-info capture — original stack for exceptional faults, wrapper exception with code-bearing `HResult` for expected ones — making the outermost host boundary a one-liner that still preserves the full fidelity story.
- Comprehension syntax over the accumulating carrier supports guards natively in both positions — guard-first (precondition before any admission) and guard-after (refinement over admitted values) — via dedicated bind support on the guard shape, so refinement never forces a detour through the sequential carrier and back.
