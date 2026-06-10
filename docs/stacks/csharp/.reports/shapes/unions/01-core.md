# Discriminated Unions — Closed Variant Families and Generated Total Dispatch

[FAMILY_FORMS]:
- Two generated union families exist. Regular: `[Union]` on an abstract partial class or abstract partial record; cases are nested types derived from the owner, discovered recursively at every nesting depth; the owner is always a reference type. Ad-hoc: `[Union<T1,...,T5>]` (generic form, 2–5 member types) or `[AdHocUnion(typeof(T1), ...)]` (non-generic mirror with identical per-member options) on a partial class, partial struct, or ref partial struct; member types may be named types, arrays, or the owner's own type parameters.
- One union attribute per type (a second is an analyzer error); records cannot be ad-hoc owners; ad-hoc owners reject primary constructors; both owners must be partial; an ad-hoc union below two member types is rejected.
- Regular-union option surface: `SwitchMethods`, `MapMethods`, `ConversionFromValue` (implicit by default), `SwitchMapStateParameterName` (`"state"` by default), `NestedUnionParameterNames`. The ad-hoc surface adds `ConversionToValue` (explicit by default), `ConstructorAccessModifier`, `DefaultStringComparison` (ordinal-ignore-case by default), `SkipToString`, `SkipEqualityComparison`, `UseSingleBackingField`, `FactoryMethodGeneration`, and per-member `TxName` / `TxIsNullableReferenceType` / `TxIsStateless`.
- Selection pressure: ad-hoc composes already-existing types into one payload alternative with value semantics and no hierarchy; regular mints a new closed concept whose cases carry distinct named payloads and can host case-owned behavior. An ad-hoc union spelling exactly success-or-failure re-derives `Fin<T>`/`Validation<Error,T>` — rails own outcome transport; unions own domain variant vocabularies whose cases demand distinct continuations at call sites.
[CLOSURE_LAWS]:
- Closure is mechanical, not conventional: when the owner declares no constructor the generator emits a private parameterless one; the analyzer requires owner constructors to be private (TTRESG009), every case to be sealed or private-constructors-only (TTRESG054), and record cases to be sealed unconditionally (TTRESG055) — so multi-level case trees require class owners, each intermediate abstract case carrying a private constructor that its own nested cases can still reach by containment.
- A non-abstract case less accessible than its owner is an error (TTRESG056); a case declaring its own type parameters aborts generation for the whole union (TTRESG053); a nested type that does not derive from the owner is only a warning (TTRESG106) — nested-but-foreign types are the single shape the closure scan tolerates.
- Cases nested inside other cases are members of the root union too; an intermediate abstract case becomes its own dispatch surface only by carrying its own `[Union]`; the nested union's generated `Map` deliberately hides the parent's `Map`, so dispatch granularity follows the receiver's static type.
- Case payload design is ordinary type design: positional record or primary-constructor class parameters are simultaneously the named payload, the single-argument-constructor feed for owner-level conversion operators, and the value the generated dispatch hands to the case callback — one declaration drives admission, conversion, and dispatch.
- There is no validation partial on a union: each case is a full type that admits its own payload (value objects, validated factories); the union composes admitted material. Closed custom admission = non-public construction plus suppressed conversions plus a hand-written factory returning a rail value.
[GENERATED_DISPATCH]:
- Per member set the generator can emit eight `Switch` overloads (action/func × stateless/state-threaded × total/partial) and two `Map` overloads; partial forms exist only under `SwitchMapMethodsGeneration.DefaultWithPartialOverloads`; `None` suppresses the family entirely (data-carrier unions for wire shapes).
- Total `Switch`/`Map` parameters cover every non-abstract member; parameter names derive from case names (camelCase, keyword-escaped); nested cases are prefixed with intermediate type names under `NestedUnionParameterNameGeneration.Default` (`refusalExpired`), while `Simple` drops the prefix and produces duplicate-parameter compile errors the moment sibling subtrees reuse a case name.
- Regular dispatch is a type-pattern `switch (this)` whose arms are ordered most-derived-first via a topological sort of the case hierarchy, so a concrete intermediate case never swallows its descendants; the default arm throws — unreachable for an intact closure, reachable only when another assembly subverts it.
- Ad-hoc dispatch switches on a private 1-based `int _valueIndex` (0 = uninitialized struct) — an allocation-free jump table rather than runtime type tests.
- State-threaded forms (`Switch<TState>`, `Switch<TState,TResult>`) thread exactly one explicit state value; both `TState` and `TResult` carry the `allows ref struct` anti-constraint, so span-shaped state and results flow through dispatch without boxing; an info-level analyzer (TTRESG1001) flags non-static lambdas on `Switch`/`Map`, steering every capture into the state parameter.
- `Map<TResult>` takes one eager value per case — every branch value is evaluated before dispatch; `Map` is for constant verdict tables and cheap projections, `Switch` for computation. `MapPartially` parameters are `Argument<TResult>`, a readonly ref struct with an implicit lift from `TResult` and an `IsSet` flag, so "not provided" is distinguishable from a deliberately provided `default(TResult)`.
- Partial-form `@default` asymmetry: func `SwitchPartially` requires `@default` as its first parameter; action `SwitchPartially` makes it optional and nullable; `MapPartially` requires a `@default` value. Per-case resolution is hierarchical, not flat: exact handler, else the nearest non-abstract ancestor's handler walking up the case tree, else `@default`.
[DISPATCH_BOUNDARIES]:
- `[UnionSwitchMapOverload(StopAt = new[] { typeof(CaseX) })]` (multiple allowed) generates extra Switch/Map overloads whose member set removes the entire derived subtree of each stop type and keeps the stop type itself even when abstract; abstract non-stop members are dropped; a stop type absent from the hierarchy yields no overload; sets identical to an existing overload deduplicate by member-set equality.
- Stop-at overloads let a coarse consumer treat a nested case subtree as one case while sibling leaves stay exhaustive; because callback arguments must be named, same-arity overloads resolve cleanly by parameter name.
[CALL_SITE_LAWS]:
- Positional callback arguments on any union `Switch`/`Map` are an analyzer error (TTRESG046) — every case argument is name-colon'd; the state argument is exempt. Case reorder is therefore call-site-safe, but renaming a case is a source-breaking change at every dispatch site: generated parameter names are public API.
- `SwitchMapStateParameterName` exists for collision pressure: state name and case argument names share one parameter list, so a case whose derived argument name collides with `state` forces renaming the state parameter, not the case.
- `TResult` inference fails when branches return different-but-compatible types; pin the type argument (`Switch<TCommon>(...)`) instead of upcasting inside branches.

```csharp
[Union(SwitchMapStateParameterName = "ctx")]
[UnionSwitchMapOverload(StopAt = new[] { typeof(Refusal) })]
public abstract partial class Verdict
{
    public sealed class Cleared(string token) : Verdict
    {
        public string Token { get; } = token;
    }

    public abstract partial class Refusal : Verdict
    {
        private Refusal() { }

        public sealed class Expired : Refusal;

        public sealed class Forged(string detail) : Refusal
        {
            public string Detail { get; } = detail;
        }
    }
}

// Full overload is exhaustive over leaves (cleared, refusalExpired, refusalForged);
// the StopAt overload folds the Refusal subtree into one named handler.
public static Fin<string> Admit(Verdict verdict, int quorum) =>
    verdict.Switch(
        quorum,
        cleared: static (q, ok) => ok.Token.Length >= q
            ? FinSucc(ok.Token)
            : FinFail<string>(Error.New("<thin-token>")),
        refusal: static (_, no) => FinFail<string>(Error.New(no.ToString())));
```

[AD_HOC_REPRESENTATION]:
- Storage: one typed readonly field per stateful unique member plus the int discriminator; reference-type members collapse into one shared `object? _obj` field automatically once two or more distinct stateful non-type-parameter reference members exist; `UseSingleBackingField` forces the single object field for everything, boxing struct members at construction and making `Value` return the raw field. Struct owners receive `[StructLayout(LayoutKind.Auto)]` unless a layout is user-declared.
- `TxIsStateless` stores only the discriminator for that member: accessors return `default(T)`, intra-case equality is constant-true, the hash is the member type's `typeof` hash, and stateless reference members auto-report as nullable — prefer empty structs as stateless markers to avoid the null channel.
- `default` of a struct ad-hoc union is poisoned, not valid: index 0 makes `Value`/`ToString`/`GetHashCode`/`Equals` throw; the owner implements a disallow-default marker interface that drives an analyzer error on `default(T)` and bare `new T()` (TTRESG047) and a required-member demand on fields (TTRESG104) — invalid admission fails at compile time where reachable, at first touch otherwise.
- Ref-struct ad-hoc owners are legal: `Equals(object?)` hard-returns false and the equatable/operator interfaces are skipped; `allows ref struct` anti-constraints on the owner's own type parameters are rejected (TTRESG073).
[AD_HOC_SURFACE]:
- Per member: `Is{Name}`, `As{Name}` (throws naming the actual member kind on mismatch), an object-typed `Value`, per-member constructors, implicit operators from member types, and explicit operators to member types that are cast-assertions throwing on the wrong case — `Switch` is the safe projection; conversions are boundary affordances, never interior control flow.
- Member names default to the type name; duplicate member types receive ordinal-suffixed names and need `TxName` for a usable surface; duplicates share one private `(value, valueIndex)` constructor so only factories can select the case; conversion operators are skipped for duplicates, interfaces, `object`, and type parameters — the mapping is ambiguous or illegal there.
- `FactoryMethodGeneration.Default` auto-emits `Create{Name}` for all members the moment any member is a type parameter, an interface, `object`, or a duplicate; `Always` forces factories with uniform shape; `None` suppresses them even when triggers exist.
- `ConstructorAccessModifier` flows to constructors and generated factories, but conversion operators are always emitted `public static` (a language constraint) — so a non-public modifier without `ConversionFromValue = None` leaves the implicit operators as a public creation path that bypasses the private constructors; closing admission requires setting both.
[EQUALITY_IDENTITY]:
- Ad-hoc equality is discriminator-first then member-dispatched: string members compare and hash with the configured `DefaultStringComparison` — case-insensitive union equality is the silent default until overridden; type-parameter members use `EqualityComparer<T>.Default`; the hash is the active member's hash without discriminator mixing, so cross-case hash collisions are possible while equality stays exact. `==`/`!=` are implemented through the generic-math equality-operators interface.
- Regular unions generate no equality and no `ToString` — identity is case-owned: record cases get per-case value equality, class cases keep reference identity; mixing both inside one union is legitimate (evidence cases as records, lifecycle-ish cases as classes) but must be chosen, not inherited by accident.
- Ad-hoc `ToString` forwards the active member (strings raw, others null-safe); `SkipToString` and `SkipEqualityComparison` opt out of each surface independently.
[GENERICS]:
- Regular union owners may be generic; nested cases sit inside the generic scope and close over the owner's parameters without declaring their own — the family is generic exactly once, at the root.
- Ad-hoc owners go generic through `TypeParamRef1..5` placeholder types standing for the owner's type parameters in both attribute forms; a placeholder index above the owner's arity is an error (TTRESG071), a placeholder on a non-generic owner is an error (TTRESG072), and a generic owner using no placeholder is a dead-generics warning (TTRESG107).
- Type-parameter members get typed backing fields, comparer-based equality, and factories instead of operators; a union of one type parameter and concrete companions is the generated, domain-named analogue of an either-shaped container.

```csharp
[Union<TypeParamRef1, Probe>(
    T1Name = "Payload", T2Name = "Pending", T2IsStateless = true,
    SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads,
    ConstructorAccessModifier = UnionConstructorAccessModifier.Internal,
    ConversionFromValue = ConversionOperatorsGeneration.None)]
public partial struct Sampled<T>;

public readonly record struct Probe;

// Type-parameter member triggers factories for ALL members; Pending costs only the index.
var sampled = Sampled<double>.CreatePayload(0.25);
var label = sampled.Map(payload: "<ready>", pending: "<waiting>");
```

[CONVERSION_PROJECTION]:
- Regular unions generate operators on the owner from each case's public single-argument constructor parameter type — only when that parameter type is unique across all cases' single-argument constructors, the case is non-abstract, declares no required members, and the parameter type is neither interface, `object`, nor in a base/derived relation with the case itself. Payload-type collision anywhere in the family silently deletes both operators: conversion ergonomics degrade as case payloads converge, by design, and dispatch remains the only total surface.
- Both families implement a metadata-owner contract through a static interface member exposing the closed member list (type members for regular, member types for ad-hoc), with a runtime lookup from `Type` to that metadata for serializer and mapper integration; consuming it directly is flagged as internal-API usage — it is the reflection contract, not an application surface.
- Regular unions serialize polymorphically with standard derived-type annotations on the owner, since cases are real types; ad-hoc unions carry no wire discriminator and need an object-factory admission route for any serialized boundary.
[DESIGN_PRESSURE]:
- Adding a case to a regular union breaks every total `Switch`/`Map` call site at compile time — that propagation is the contract, not a cost; partial overloads route the new case to `@default` silently. Reserve partial dispatch for semantically total defaults (telemetry, fallthrough rendering) and never for routing; enable `DefaultWithPartialOverloads` only on unions whose consumers genuinely need sparse handling, because every partial overload is a place exhaustiveness no longer guards.
- Union versus inheritance-with-virtuals is not either/or here: cases are ordinary types, so case-owned invariants live as abstract or virtual members on the owner while consumer-owned reactions live in generated `Switch`. The per-behavior decision is "who must change when a case is added" — the owner file (virtual member, closed-world edit in one place) or every call site (dispatch, compile-time sweep of all consumers); a behavior whose correctness depends on seeing all cases at once belongs in `Switch`, a behavior that is intrinsically each case's own belongs on the case.
- Operation/intent families: one regular union per verb family with per-case payloads and one state-threaded `Switch(state, static ...)` as the single total dispatcher returning a rail value — callbacks stay static, context rides `TState`, failure rides `Fin<T>`/`Validation<Error,T>`, and "unhandled verb" ceases to exist as a runtime concept.
- Result/evidence families: one regular union with a case per outcome kind, each carrying its typed evidence payload; `Map` renders constant verdict tables, stop-at overloads let coarse consumers treat an evidence subtree as one verdict while fine consumers stay exhaustive — the same family serves both fidelities without parallel types.
