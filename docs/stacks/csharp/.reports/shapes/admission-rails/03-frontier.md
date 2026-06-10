# Admission Rails — Frontier

[COVARIANT_FAULT_LATTICE]:
- The factory contract declares its error parameter covariantly (`out TValidationError`, legal because the fault appears only in return position), and `IValidationError<out T>` is itself covariant. C# satisfies generic constraints through variant interface conversions even when the interface carries static abstract members — an owner implementing the contract with a derived fault case satisfies a bridge constrained on the fault base, and the static-abstract `Validate` call dispatches to the owner's implementation with its result viewed as the base.
- One base-constrained bridge therefore serves a fault lattice, not a fault type: each owner declares the most precise case of the family as its validation-error type — its own codes, its own generator-authored text — while the bridge, the carriers, and the recovery vocabulary uniformly see the base. The single-bridge inference argument and per-owner fault precision are not in tension; the variance reconciles them.
- The precise case must re-satisfy the validation-error contract itself (a shadowing `static new` `Create` returning the derived type), because the error-declaration attribute's type-parameter constraint is self-referential — class-constrained and self-implementing — and is checked at the attribute site, so a malformed lattice member fails before any generation runs.
- Because the generated spine manufactures all generator-authored faults through the configured error type's `Create`, per-owner derived cases also partition generator text by owner — the one population of faults a single shared family can never attribute.

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

[SPAN_GRAMMAR_ADMISSION]:
- The factory-source attribute's own type parameter declares `allows ref struct`, so an owner may declare a span-of-char admission grammar directly; the compelled static abstract then admits UTF-16 wire buffers with the typed fault on rejection and zero string materialization on the rejection path.
- Declaring the span grammar upgrades the generated span-parse surface for free: span `Parse` routes through the declared span `Validate` (throwing a format exception built from the fault's text), and span `TryParse` routes through it discarding the fault — the framework binding path becomes zero-copy as a side effect of the domain grammar.
- Generated span admission exists without declaration only on keyed enumerations; keyed value objects never receive a generated span surface because their admission constructs rather than looks up — span admission for constructed owners is exactly the declared-grammar route, and the grammar decides where the one unavoidable string materialization happens (after structural rejection checks, not before).
- A single extension-block bridge spans both worlds when its raw-type parameter mirrors the anti-constraint (`where TRaw : notnull, allows ref struct`) — compile-and-run proven, including an optional format-provider parameter coexisting with the byref-like raw parameter, because the raw value is consumed inside the call and never enters the carrier.

[VOCABULARY_LOOKUP_PHYSICS]:
- String-keyed vocabulary admission is case-insensitive by default: both the construction dictionary and the frozen lookup take the ordinal-ignore-case comparer unless a key-member equality comparer is declared. Wire keys differing only in case admit the same canonical item, and the admitted instance carries its declaration-time key, not the wire spelling — vocabulary admission is simultaneously case normalization, with no hook involved.
- Admission of vocabulary is interning, not construction: generated equality is reference equality, and each item's hash is computed once at construction from the case-folded key (or the declared comparer). Success allocates nothing, downstream identity is pointer comparison, and hash identity tracks the lookup relation rather than the raw spelling.
- The lookup materializes lazily under `Lazy<T>` with `ExecutionAndPublication` semantics: duplicate identifiers under the case-folded comparer and null keys throw during `GetLookups`, and `Lazy<T>` caches the exception so the identical exception rethrows on every subsequent admission. Two items differing only in case are a definition error that poisons the entire vocabulary at its first use, invisible at compile time and at type load, unrecoverable by retry.
- On .NET 9+, the generated `Lookups` struct acquires a `FrozenDictionary<string, T>.AlternateLookup<ReadOnlySpan<char>>` alongside the primary frozen dictionary for string keys; on earlier targets the alternate-lookup field is absent. A hand-rolled key-member equality comparer that does not implement the span alternate-comparer contract causes lookup materialization itself to throw at first admission on .NET 9+ — a custom vocabulary relation must be span-capable, not merely string-capable, when targeting .NET 9 or later.

[CONVERSION_OPERATOR_POLICY]:
- Inbound key-to-owner conversion — the hidden throwing admission spelling — is a definition-time policy: explicit by default (`ConversionFromKeyMemberType = Explicit`), configurable to none, and forced to none when factories are skipped because the operator body delegates to the factory. Setting it to none erases the cast-admission spelling from the entire codebase, leaving the rail bridge as the only inbound spelling that compiles.
- Outbound owner-to-key projection generates as two distinct operators: a safe null-propagating nullable-to-nullable conversion carrying a not-null-if-not-null annotation (implicit by default via `ConversionToKeyMemberType = Implicit`), and an "unsafe" non-nullable conversion generated only for reference owners with non-nullable keys, which throws a bare null-reference exception on a null owner (explicit by default). Struct owners with non-nullable keys instead receive an additional no-throw non-nullable overload — the split exists precisely where nullness can lie.
- The safe implicit operator converts an absent owner into an absent key silently — outbound absence smuggling. Optional domain values project outward by mapping over the option to the key, never by letting a nullable reference flow through the implicit conversion, or the two distinct decisions (is it absent? may it be absent here?) collapse exactly as they do inbound with sentinels.

[FAULT_MINTING_CONVERSIONS]:
- The error base lifts implicitly from a code-and-message tuple but only explicitly from a bare string: code-bearing minting is the path of least resistance by conversion design, and every code-zero fault — whose catch relation degrades to message-string equality — requires a visible cast to create. A bridge or hook that mints a fault from a bare string announces its own string-matched recovery in the source text.
- Provenance-threading mint overloads take an inner error alongside message or code-message — the canonical field-labeling move at an accumulating bridge is wrapping each field's fault as the inner of a field-naming fault before combination, so the flat aggregate of a multi-field admission remains attributable per field without positional reconstruction.

[CATCH_RELATION_ASYMMETRY]:
- Value-keyed catch has two relations selected silently by the pipeline's failure type: the generic-failure form matches by structural equality — case type and every member, message included, must coincide — while the error-base-specialized form matches by the containment relation (code-keyed, message-keyed only at code zero, aggregate-recursive). The same catch expression changes meaning when a seam widens the failure type from the typed family to the base currency, and nothing diagnoses the flip.
- The containment relation also recurses on the argument side: testing a fault against a precomputed aggregate answers membership against every member — a catch keyed on a batch of known faults matches any one of them, which is set-membership triage, not equality.
- The predicate form and the integer-code form are the only relation-stable spellings across failure-type changes; triage written against the typed family survives widening only when keyed by predicate or code, never by value.

[BATCH_DISTRIBUTIVE_RECOVERY]:
- Typed-subtype catch (`catch<E>`) matches when the failure is the subtype or contains one (aggregate-recursive type test), and on an aggregate it filters to the matching members and invokes the handler once per member via `ForAllM` — every per-member recovery binds monadically and the last one's value wins. Recovering an accumulated three-fault admission failure runs the handler three times and keeps one result; the distribution is in the combinator, not the call site.
- The fold variant (`FoldM`) composes the per-member recoveries through the monoid of computations instead — first-success choice rather than bind-through-all. The two combinators differ exactly in how multiple matching members combine, and neither asks the handler whether it is batch-aware.
- Handlers that perform one substitute admission per fault are last-write-wins under `ForAllM`; batch-aware recovery either matches the aggregate explicitly with a predicate over count and membership, or extracts the typed members once and folds deliberately — letting the distribution law choose is the bug.

[TIERED_TRIAGE_DISPATCH]:
- Generated dispatch on a union-shaped fault family has an opt-in partial tier: default generation emits exhaustive switch and map with state-threaded overloads; the partial-overloads setting adds partial variants with optional per-case handlers and one mandatory default arm — closed-world recovery and open-world triage from a single declaration, with the closed form still compile-breaking on a new fault case.
- Multi-level fault hierarchies generate stop-at overloads: a repeatable attribute names intermediate cases at which dispatch stops descending, producing additional switch and map overloads that treat each named tier as one arm beside its leaf siblings. A severity tier — recoverable versus fatal as intermediate abstract cases — dispatches as two arms at the boundary and leaf-exhaustively in interior recovery, as overloads of the same method.

[WIRE_FAULT_CONTRACT]:
- The error family's serialization contract is exactly code plus message: the expected case marks its inner error non-serialized, the exceptional case carries only code and message, and only the aggregate serializes its member list. Inner-chained provenance is process-local by declared contract, not by serializer accident — the field-labeling chain from the bridge survives precisely until the wire, where each labeling fault keeps its own code and message and loses its chain.
- Derived fault cases are polymorphic records over the base, so a boundary serializer bound to the static base type erases case identity and structured payloads; what crosses a process boundary is the triple of code, message, and aggregate shape. Codes must therefore be self-sufficient for remote triage — the structured case payloads and the inner chain are in-process diagnosis material, and a fault family designed only for in-process matching silently becomes message-matched on the far side of a wire.

[TUPLE_APPLY_SPELLING]:
- Carrier tuples apply uncurried functions directly — `(Host.Admit(h), Port.Admit(p)).Apply(static (a, b) => new Endpoint(a, b))` — at arities two through five via the `Applicative<F>` extension, with no curried function lift and no operator chain; the result is trait-kinded and closes to the concrete carrier with the downcast or unary plus.
- The tuple spelling binds the trait-level applicative, whose accumulating instance resolves the failure semigroup at runtime — the highest-arity, lowest-ceremony admission spelling carries the weakest accumulation guarantee: with the error base as failure currency it accumulates; with a typed family lacking its own trait instance it silently keeps the first fault. A fault family intended for tuple-apply composition must carry its own semigroup instance or the N-field bridge degrades to first-fault reporting with no diagnostic.
