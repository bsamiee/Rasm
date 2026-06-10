# Discriminated Unions — Closed Variant Families and Generated Total Dispatch

[FAMILY_FORMS]:
- Two generated union families exist. Regular: `[Union]` on an abstract partial class or abstract partial record; cases are nested types derived from the owner, discovered recursively at every nesting depth; the owner is always a reference type. Ad-hoc: `[Union<T1,...,T5>]` (generic form, 2–5 member types) or `[AdHocUnion(typeof(T1), ...)]` (non-generic mirror with identical per-member options) on a partial class, partial struct, or ref partial struct; member types may be named types, arrays, or the owner's own type parameters.
- One union attribute per type (a second is an analyzer error); records cannot be ad-hoc owners; ad-hoc owners reject primary constructors; both owners must be partial; an ad-hoc union below two member types is rejected.
- Regular `[Union]` is a sealed attribute whose only options are `SwitchMethods`, `MapMethods`, `ConversionFromValue`, `SwitchMapStateParameterName`, and `NestedUnionParameterNames`. The ad-hoc forms (`[Union<T1..T5>]` and `[AdHocUnion]`) derive from `UnionAttributeBase`, which owns the wider surface: those five plus `ConversionToValue`, `ConstructorAccessModifier`, `DefaultStringComparison`, `SkipToString`, `SkipEqualityComparison`, `UseSingleBackingField`, and `FactoryMethodGeneration`; ad-hoc further adds per-member `TxName` / `TxIsNullableReferenceType` / `TxIsStateless`. Options such as `ConstructorAccessModifier` are legal only on ad-hoc owners and do not exist on the regular-union attribute.
- Selection pressure: ad-hoc composes already-existing types into one payload alternative with value semantics and no hierarchy; regular mints a new closed concept whose cases carry distinct named payloads and can host case-owned behavior. An ad-hoc union spelling exactly success-or-failure re-derives `Fin<T>`/`Validation<Error,T>` — rails own outcome transport; unions own domain variant vocabularies whose cases demand distinct continuations at call sites.

[CASE_KIND_REGIME]:
- The owner's declaration kind is family-global by language law — records derive only from records, classes only from classes — so `abstract partial record` versus `abstract partial class` on the root fixes every case's kind at once: equality, `ToString`, `with`, and `Deconstruct` are a root-level regime decision, never a per-case choice.
- An abstract record owner synthesizes `==`/`!=` routing through virtual `Equals` plus `EqualityContract`: two owner-typed references compare by runtime case and payload — whole-family structural equality with zero hand-written code. An abstract class owner leaves owner-typed `==` as reference identity; no structural comparison exists anywhere in the family unless hand-written.
- `EqualityContract` makes cross-case equality constant-false even when two cases carry identical payload shapes, so a record family needs no discriminator guard before comparing — distinct folds, set membership, and memo keys are case-exact for free.
- Record-rooted families are flat by analyzer law (every record case sealed, TTRESG055; no nested unions inside records); depth — intermediate abstract cases, nested dispatch surfaces, stop-at boundaries — requires a class root and surrenders generated structural equality. The trade is exactly evidence family (record root: value semantics, flat) versus intent tree (class root: depth, reference identity).
- Regular unions generate no equality and no `ToString` — identity is case-owned: record cases get per-case value equality, class cases keep reference identity. The owner's kind is family-global by language law (records derive only from records, classes only from classes), so every case inherits the same equality regime as the root; the choice of record versus class is made once at the owner declaration and cannot be varied per case.
- `with` is legal at the owner static type because the synthesized clone is virtual, but it can rebind only owner-declared members there; rebinding case payloads demands the case static type — non-destructive payload mutation lives behind a type pattern, and its result stays inside the family.
- Positional record cases ship `Deconstruct`, so boundary probes compose recursive patterns (`x is Owner.CaseA(var p, _)`) without touching dispatch; interior code still owes totality to generated `Switch`, because language `switch` over a class hierarchy never proves exhaustiveness and the silencing `_` arm is precisely the hole the generated surface exists to close.
- Record cases print case-name-plus-payload; ad-hoc unions forward the raw member's `ToString`, erasing which alternative is active from every log line — two unions unequal by case can render identically. Where the active case matters observationally, project through `Map` to labeled text; `ToString` on an ad-hoc union is a payload printer, not an identity printer.

[CLOSURE_LAWS]:
- Closure is mechanical, not conventional: when the owner declares no constructor the generator emits a private parameterless one; the analyzer requires owner constructors to be private (TTRESG009), every case to be sealed or private-constructors-only (TTRESG054), and record cases to be sealed unconditionally (TTRESG055) — so multi-level case trees require class owners, each intermediate abstract case carrying a private constructor that its own nested cases can still reach by containment.
- A non-abstract case less accessible than its owner is an error (TTRESG056); a case declaring its own type parameters aborts generation for the whole union (TTRESG053); a nested type that does not derive from the owner is only a warning (TTRESG106) — nested-but-foreign types are the single shape the closure scan tolerates.
- Cases nested inside other cases are members of the root union too; an intermediate abstract case becomes its own dispatch surface only by carrying its own `[Union]`; the nested union's generated `Map` deliberately hides the parent's `Map`, so dispatch granularity follows the receiver's static type.
- Case payload design is ordinary type design: positional record or primary-constructor class parameters are simultaneously the named payload, the single-argument-constructor feed for owner-level conversion operators, and the value the generated dispatch hands to the case callback — one declaration drives admission, conversion, and dispatch.
- Unions carry no validation partial: each case is a full type that admits its own payload (value objects, validated factories), and the union composes already-admitted material. Closed custom admission requires non-public construction plus suppressed conversions plus a hand-written factory returning a rail value.
- A case-less union owner generates nothing at all: discovery bails before emission, so no metadata row, no private constructor, and no dispatch surface exist, and no diagnostic reports the empty family. The closed family materializes retroactively when its first case lands; until then the owner is an ordinary abstract type, and consumers written ahead of the first case fail to find `Switch` rather than failing exhaustiveness.
- Case discovery is a containment walk that recurses only through class-kind containers. A case nested beneath a struct or interface intermediate inside the owner is invisible to both generation and analysis, yet deriving there is fully legal — private-member access extends to types nested at any depth — so the family compiles clean with a phantom case whose first dispatch lands in the default arm's runtime throw. The nested-but-foreign audit reads only first-level members, so depth additionally hides foreign types from that warning. Non-class intermediates inside a union owner are forbidden shape, enforceable only by review.
- Closure is constructor reachability, not attribute law: the analyzer's private-constructor demand on the owner is the entire proof that derivation is containment-scoped, and the generated parameterless constructor is merely its default instantiation — declaring any owner constructor suppresses the generated one, and the analyzer immediately re-demands privacy on the declared replacement. The attribute contributes discovery and dispatch; the access modifier contributes the closure.

[GENERATED_DISPATCH]:
- Per member set the generator can emit eight `Switch` overloads (action/func × stateless/state-threaded × total/partial) and two `Map` overloads; partial forms exist only under `SwitchMapMethodsGeneration.DefaultWithPartialOverloads`; `None` suppresses the family entirely (data-carrier unions for wire shapes).
- Total `Switch`/`Map` parameters cover every non-abstract member; parameter names derive from case names (camelCase, keyword-escaped); nested cases are prefixed with intermediate type names under `NestedUnionParameterNameGeneration.Default` (`refusalExpired`), while `Simple` drops the prefix and produces duplicate-parameter compile errors the moment sibling subtrees reuse a case name.
- Regular dispatch is a most-derived-first type-pattern `switch (this)` whose arms are ordered via topological sort of the case hierarchy (leaves before their ancestors, reversed from the parent-first walk); the default arm throws an `ArgumentOutOfRangeException` naming the runtime type's full name — unreachable for an intact closure, but reachable from another assembly subverting the private constructor or via the phantom-case route (a case nested under a struct or interface intermediate inside the owner, invisible to generation yet derivable by containment).
- Ad-hoc dispatch switches on a private 1-based `int _valueIndex` (0 = uninitialized struct) — an allocation-free jump table rather than runtime type tests.
- State-threaded forms (`Switch<TState>`, `Switch<TState,TResult>`) thread exactly one explicit state value; both `TState` and `TResult` carry the `allows ref struct` anti-constraint, so span-shaped state and results flow through dispatch without boxing.
- `Map<TResult>` takes one eager value per case — every branch value is evaluated before dispatch; `Map` is for constant verdict tables and cheap projections, `Switch` for computation. `MapPartially` parameters are `Argument<TResult>`, a readonly ref struct with an implicit lift from `TResult` and an `IsSet` flag, so "not provided" is distinguishable from a deliberately provided `default(TResult)`.
- Partial-form `@default` asymmetry: func `SwitchPartially` requires `@default` as its first parameter; action `SwitchPartially` makes it optional and nullable; `MapPartially` requires a `@default` value. Per-case resolution is hierarchical, not flat: exact handler, else the nearest non-abstract ancestor's handler walking up the case tree, else `@default`.
- `Map`'s eagerness amplifies allocation, not merely computation: every branch value is constructed per call, so a `Map` whose arms allocate builds N-1 dead values per dispatch — `Map` arms must be preallocated constants or cheap projections; any allocating arm belongs in `Switch`. For task-shaped arms the hazard is execution, not merely allocation: every branch's task is already running before dispatch selects one, the losers are abandoned mid-flight, their side effects land anyway, and their failures surface as unobserved-task faults far from the call. No task-valued expression belongs in a `Map` arm; the func-form `Switch` with the result type instantiated to the task shape is the only async dispatch. The safe computation-shaped `Map` arms are cold carriers — effect descriptions and already-settled rail values — because selection and execution stay in separate phases.
- Generated members carry `DebuggerStepThroughAttribute` and `DebuggerNonUserCodeAttribute`: stepping into a `Switch` lands directly in the chosen callback in any Just-My-Code debugger view; `Exception.StackTrace` from the default-arm throw still contains the generated `Switch` frame because no `[StackTraceHidden]` is emitted, making the default arm's runtime-type message the entire triage signal for cross-assembly closure subversion.
- Callback parameters are annotated consumed-before-return for analysis tooling, and IDE quick actions scaffold the complete named-argument set for `Switch`/`Map` and the partial forms — add the case, re-scaffold every red call site, and the compile break becomes the repair loop.

[DISPATCH_BOUNDARIES]:
- `[UnionSwitchMapOverload(StopAt = new[] { typeof(CaseX) })]` (multiple allowed) generates extra Switch/Map overloads whose member set removes the entire derived subtree of each stop type and keeps the stop type itself even when abstract; abstract non-stop members are dropped; a stop type absent from the hierarchy yields no overload; overloads with an identical member set to an existing one deduplicate — identical member sets yield identical parameter names and types, so the surviving overload binds every coarse call site unchanged and no call site breaks from deduplication alone. The real breakage surface is a stop type leaving the hierarchy entirely: the absent-type rule fires, the stop-at overload collapses to a smaller set, and consumers written against the former coarse shape lose the named callback they depended on; treat each declared stop boundary as a published overload whose existence is contingent on the stop type remaining in the hierarchy.
- Stop-at overloads let a coarse consumer treat a nested case subtree as one case while sibling leaves stay exhaustive; because callback arguments must be named, same-arity overloads resolve cleanly by parameter name.
- Deep trees route at two granularities with both levels total: a stop-at overload on the root folds a subtree behind its abstract stop type, and the stop-typed handler re-dispatches on that intermediate's own nested generated surface — coarse consumers see one arm, fine consumers see leaves, and a case added anywhere breaks exactly the dispatch level that names its subtree.

[CALL_SITE_LAWS]:
- Positional callback arguments on any union `Switch`/`Map` are an analyzer error (TTRESG046) — every case argument is name-colon'd; the state argument is exempt. Case reorder is therefore call-site-safe, but renaming a case is a source-breaking change at every dispatch site: generated parameter names are public API.
- `SwitchMapStateParameterName` exists for collision pressure: state name and case argument names share one parameter list, so a case whose derived argument name collides with `state` forces renaming the state parameter, not the case.
- `TResult` inference fails when branches return different-but-compatible types; pin the type argument (`Switch<TCommon>(...)`) instead of upcasting inside branches.
- The named-argument law is mechanical: the single positional slot is a leading argument whose original parameter type is a method type parameter distinct from the method's return type — which matches exactly the state parameter of state-threaded forms. `Map`'s first arm fails the test because its parameter type is the return type itself, so every `Map` argument is named including the first; the partial forms' default slot is spelled `@default:`.
- The static-lambda advisory (TTRESG1001) inspects only `Switch`/`SwitchPartially` invocations in member-access form; `Map` arms are values, not lambdas, and are never inspected by that diagnostic. A clean pass does not prove every arm is capture-free — capture review on wide dispatches is still manual, and all captured state must move into the threaded `TState` parameter to satisfy the diagnostic.
- The partial-map arm carrier is a ref struct, and arm expressions convert to it at evaluation, so an `await` appearing to the right of any already-evaluated arm in a `MapPartially` argument list is a compile error — the evaluated carriers cannot be spilled across the await. The `Switch` forms are immune (their arguments are heap delegates); the stable async form hoists awaited values into locals before the dispatch expression.
- Async lambda arms and span-shaped state are mutually exclusive: async lambdas cannot declare ref-struct parameters, so any `Switch` arm written as an async lambda requires heap-shaped state. A synchronous lambda arm may legally thread span state and return a task-shaped `TResult` — the exclusion is on the lambda's async modifier, not on the result type. Choose the state shape before writing arms; no overload bridges async lambdas and ref-struct state.

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

[VERSION_SURFACE]:
- The case list is part of every dispatch method's signature, which makes case addition a binary break, not merely a source break: each `Switch`/`Map`/partial overload is re-signatured with one more parameter, the old overload ceases to exist, and consumer assemblies compiled against the previous shape fail at JIT bind with a missing-method fault — loud, early, and impossible to misroute. Optional parameters on the partial forms are baked into caller IL at compile time, so partial dispatch is equally binary-broken; no generated dispatch surface is hot-swap stable across case addition.
- Case rename breaks on two independent axes: generated callback parameter names are source-level API (named-argument law makes every call site spell them), and the nested case type name is embedded in every consumer's callback signatures and type patterns, so rename is also a binary break via type load. Payload-shape changes inside a case that keep the case name and arity leave dispatch call sites untouched and break only the arms that touch the changed member — the dispatch surface partitions blast radius by axis: identity changes hit every consumer, payload changes hit only handlers.
- Language type patterns are the only binary-stable consumers across a case-list change — and they are exactly the consumers that surrendered totality. Shipping a union across an assembly boundary commits every consumer to lockstep rebuilds; inside one build graph, that cost is zero.

[AD_HOC_REPRESENTATION]:
- Storage: one typed readonly field per stateful unique member plus the int discriminator; reference-type members collapse into one shared `object? _obj` field automatically once two or more distinct stateful non-type-parameter reference members exist; `UseSingleBackingField` forces the single object field for everything, boxing struct members at construction and making `Value` return the raw field. Struct owners receive `[StructLayout(LayoutKind.Auto)]` unless a layout is user-declared.
- `TxIsStateless` stores only the discriminator for that member: accessors return `default(T)`, intra-case equality is constant-true, the hash is the member type's `typeof` hash, and stateless reference members auto-report as nullable (`TxIsNullableReferenceType` inherits the stateless flag for reference types) — prefer empty structs as stateless markers to avoid the null channel.
- `default` of a struct ad-hoc union is poisoned, not valid: index 0 makes `Value`/`ToString`/`GetHashCode`/`Equals`/`Switch`/`Map` throw `InvalidOperationException` naming the struct type and advising initialization; `As{Member}` on the wrong case throws `InvalidOperationException` naming the requested type; impossible discriminator indices throw `IndexOutOfRangeException` from `Value` and from both `Switch` action and func forms, while `Map`/`MapPartially` throw `ArgumentOutOfRangeException` — the split is `Value`+`Switch` versus `Map`, not per-surface. The default-poison analyzer catches literal `default(T)`, bare `new T()`, and undeclared required fields (TTRESG047, TTRESG104) — but `new T[n]` elements, `default(TGeneric)` flowing through unconstrained generics, and uninitialized struct fields nested inside other structs all evade it; the runtime throw set is the backstop, which means hashing or logging a poisoned union is itself the crash site.
- `Is{Member}` is the one generated probe on a possibly-default struct union that cannot throw — a pure discriminator compare; guard rehydration, pooling, and array-scan seams with it before touching any other generated member.
- Ref-struct ad-hoc owners are legal: `Equals(object?)` hard-returns false and the equatable/operator interfaces are skipped; `allows ref struct` anti-constraints on the owner's own type parameters are rejected (TTRESG073).
- Struct versus class ad-hoc owners trade failure channels, not safety: struct buys allocation-free transport plus a poisoned default; class buys an ordinary null channel plus per-value allocation — choose by which absence discipline the surrounding code already polices.

[AD_HOC_SURFACE]:
- Per member: `Is{Name}`, `As{Name}` (throws `InvalidOperationException` naming the type on mismatch), an object-typed `Value`, per-member constructors, implicit operators from member types, and explicit operators to member types that are cast-assertions throwing on the wrong case — `Switch` is the safe projection; conversions are boundary affordances, never interior control flow.
- Member names default to the type name; duplicate member types receive ordinal-suffixed names and need `TxName` for a usable surface; duplicates share one private `(value, valueIndex)` constructor so only factories can select the case; conversion operators are skipped for duplicates, interfaces, `object`, and type parameters — the mapping is ambiguous or illegal there.
- `FactoryMethodGeneration.Default` auto-emits `Create{Name}` for all members the moment any member is a type parameter, an interface, `object`, or a duplicate; `Always` forces factories with uniform shape; `None` suppresses them even when triggers exist.
- `ConstructorAccessModifier` flows to constructors and generated factories, but conversion operators are always emitted `public static` (a language constraint) — a non-public modifier without `ConversionFromValue = ConversionOperatorsGeneration.None` leaves the implicit operators as a public creation path that bypasses the private constructors; closing admission requires setting both.
- Generated implicit conversions make an ad-hoc union a parameter-side absorber: one parameter of the union type replaces an overload per member, and collection expressions lift heterogeneous raw members element-wise through those conversions — a single `params ReadOnlySpan<U>` or array entrypoint accepts a mixed batch with zero ceremony at every call site. The absorption boundary is exactly the language's: members typed `object`, interface, or type parameter legally cannot own conversion operators, so their presence flips the whole family to factory-method admission — call shape degrades from juxtaposition to named construction the moment the member vocabulary abstracts.

```csharp
[Union<string, long>(T1Name = "Tag", T2Name = "Serial")]
public partial struct Marker;

// Collection expressions lift mixed raw members through generated implicit conversions:
// one entrypoint, no overload family, no per-element allocation.
Marker[] batch = ["alpha", 42L, "beta"];
var weight = batch.Sum(static m => m.Switch(tag: static t => t.Length, serial: static _ => 8));
```

[EQUALITY_IDENTITY]:
- Ad-hoc equality is discriminator-first then member-dispatched: string members compare and hash with the configured `DefaultStringComparison` — case-insensitive union equality is the silent default until overridden with `StringComparison.Ordinal`; type-parameter members use `EqualityComparer<T>.Default`; the hash is the active member's hash without discriminator mixing, so cross-case hash collisions are possible while equality stays exact. `==`/`!=` are implemented through the generic-math equality-operators interface.
- `SkipToString` and `SkipEqualityComparison` opt out of each surface independently.

[GENERICS]:
- Regular union owners may be generic; nested cases sit inside the generic scope and close over the owner's parameters without declaring their own — the family is generic exactly once, at the root.
- Ad-hoc owners go generic through `TypeParamRef1..5` placeholder types standing for the owner's type parameters in both attribute forms; a placeholder index above the owner's arity is an error (TTRESG071), a placeholder on a non-generic owner is an error (TTRESG072), and a generic owner using no placeholder is a dead-generics warning (TTRESG107).
- Type-parameter members get typed backing fields, comparer-based equality, and factories instead of operators; a union of one type parameter and concrete companions is the generated, domain-named analogue of an either-shaped container.
- A generic union root is a phantom-typestate carrier: cases close over the root's type parameters without declaring their own, so one declaration yields a family per marker instantiation, operations select legal phases by constraint, and dispatch stays total within each phase. The cost is invariance — no common view spans instantiations, so storage and transport boundaries need either an explicit erasing projection or a separate union over the instantiations; phantom phases are free at dispatch and paid at the container.

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

[WIRE_ADMISSION_FACTORY]:
- `[ObjectFactory<TValue>]` on a union owner declares a second admission route with analyzer-enforced shape: a user-written `static TError? Validate(TValue value, IFormatProvider? provider, out T item)` (TTRESG061), plus an instance `TValue ToValue()` the moment serialization or persistence is requested (TTRESG062). The generator then implements the static-abstract factory interface and the conversion interface, and publishes a factory-metadata row carrying the value type and per-consumer routing flags.
- The factory's failure channel is typed, not stringly: it defaults to the library validation-error and swaps to any type implementing the static-`Create(string)` error interface, so wire rejection speaks the same typed failure vocabulary as the rest of the boundary — one message-to-error constructor everywhere, declared once on the owner.
- `HasCorrespondingConstructor = true` is a proof obligation, not a hint: the owner must expose a single-argument constructor of the factory value type (TTRESG059), which metadata then exposes as a constructor-shaped conversion expression — an expression-tree admission path persistence providers can compile and translate where a method-call `Validate` is opaque.
- Multiple factory attributes partition by concern under analyzer guard: overlapping serialization-framework flags are an error, at most one factory may claim persistence, and at most one may claim model binding — every downstream consumer resolves exactly one admission grammar, mechanically.
- `[ObjectFactory<string>]` additionally generates the parse pair: `Parse` funnels through `Validate` and throws a format exception carrying the validation error's text; `TryParse` is the non-throwing rail. Both are public statics, so convention-based endpoint parameter binding discovers them with no registration.
- Deserialization is admission: every generated converter routes the wire value through the same `Validate`, so hostile or stale payloads fail into the serializer's error channel and a half-built union cannot exist in memory — interior code never re-checks wire trust.
- Conversion-route selection takes the last factory satisfying the consumer's filter, not the first: factory attribute declaration order is load-bearing the moment two factories could satisfy one consumer, and appending a factory can silently re-route an existing consumer — order factory attributes from general to specific and treat reordering as a behavior change.

```csharp
[Union]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.All, UseForModelBinding = true)]
public abstract partial record Channel
{
    public sealed record Mail(string Address) : Channel;
    public sealed record Page(string Number) : Channel;

    internal static ValidationError? Validate(string? value, IFormatProvider? provider, out Channel? item)
    {
        item = value?.Split(':') switch
        {
            ["mail", var address] => new Mail(address),
            ["page", var number] => new Page(number),
            _ => null,
        };
        return item is null ? new ValidationError($"<unparsable: {value}>") : null;
    }

    public string ToValue() => this.Switch(
        mail: static m => $"mail:{m.Address}",
        page: static p => $"page:{p.Number}");
}

// One grammar serves every consumer: JSON converters, persistence conversion, and
// endpoint binding all route through Validate; Parse failure carries the error text.
var direct = Channel.Parse("page:411", provider: null);
```

[WIRE_SHAPE_PRESSURE]:
- Regular-union cases are real types, so polymorphic JSON is the platform's own: per-leaf `[JsonDerivedType]` rows on the owner. Resolution is exact-runtime-type — an unregistered leaf fails serialization rather than degrading, and abstract intermediates are registrable only as targets of the opt-in nearest-ancestor fallback. That fallback is the wire mirror of stop-at dispatch overloads: coarse consumers read a whole case subtree as one shape while the family stays closed.
- The polymorphism discriminator must lead the JSON object unless out-of-order metadata reading is enabled on the serializer options; producers that reorder or stream properties break round-tripping silently — pin the option at the boundary owner, never per call site.
- Discriminator strings restate case identity the program already knows; derive them from `nameof(Owner.Case)` or the published member list so renaming a case breaks compilation, not production decoding.
- Ad-hoc unions never carry a discriminator by design — single-value factory admission is the only wire route. The per-serializer converters are emitted only when that serializer's integration assembly is referenced: converter generation is keyed off the dependency graph at definition time, so adding the reference retroactively widens every factory-marked union with no source edit.
- One `[ObjectFactory<string>(UseForSerialization = All)]` on the owner collapses every wire concern — JSON, persistence, binding — onto one canonical grammar with one parser and one printer; the cost is opacity (no per-property wire schema). Correct for identifier-like unions; wrong for document-like evidence payloads whose properties downstream systems must address.
- Persistence by inheritance: cases map as a single-table hierarchy with one discriminator column and a value row per leaf; conventions do not discover nested types, so each case is registered explicitly — the closed member list turns that registration into a fold over the family rather than a hand-enumerated block, and per-case columns are configured on the case entity, never the owner.

[CONVERSION_PROJECTION]:
- Regular unions generate operators on the owner from each case's public single-argument constructor parameter type — only when that parameter type is unique across all cases' single-argument constructors, the case is non-abstract, declares no required members, and the parameter type is neither interface, `object`, nor in a base/derived relation with the case itself. Payload-type collision anywhere in the family silently deletes both operators: conversion ergonomics degrade as case payloads converge, by design, and dispatch remains the only total surface.
- Both families implement a metadata-owner contract through a static interface member exposing the closed member list (type members for regular, member types for ad-hoc), with a runtime lookup from `Type` to that metadata for serializer and mapper integration; consuming it directly trips TTRESG1000 everywhere outside generated code. The defensible consumption zone is family-law tests and infrastructure adapters — folding the published member list to assert every case owns a wire row or a render arm gives closed-family introspection without assembly scanning, and library reshaping surfaces as a test break instead of a production decode failure.
- The metadata surface is itself a closed variant family with hand-written total `Switch`/`Map` over its kind cases — the package's own infrastructure speaks the dispatch grammar it generates, and consuming it obeys the same named-argument call-site laws.

[RECURSIVE_FAMILIES]:
- Self-referential families get exactly the right conversion ergonomics for free: the operator-generation rule that skips payload types related to the case by inheritance means a case carrying the owner type never receives a lifting operator, while leaf cases over foreign payload types keep theirs — induction steps are always constructed explicitly, leaf admission stays implicit, and the asymmetry is generated, not policed.
- The catamorphism shape is an extension member recursing through state-threaded `Switch`: the environment rides the state parameter, the failure rail rides the result type, callbacks stay static because the case payload and the threaded state are the only inputs, and the recursion lands back on the extension member so the fold is named once. Each recursion level costs one type-test ladder plus one delegate invocation and two stack frames; generated dispatch is depth-honest, so unbounded or hostile input demands an explicit-stack kernel at the admission boundary — a named statement exemption — rather than recursive dispatch in the interior.
- Rose-shaped cases carrying immutable sequences of the owner fold with collection combinators inside a single arm, and rail-preserving recursion over children is a traversal, not a loop: the arm maps children through the fold and sequences the rail, so one failing child fails the node with no partial accumulation.

```csharp
[Union]
public abstract partial record Formula
{
    public sealed record Num(double Value) : Formula;
    public sealed record Sym(string Name) : Formula;
    public sealed record Sum(Formula Left, Formula Right) : Formula;
}

public static class FormulaOps
{
    extension(Formula source)
    {
        public int Depth => source.Switch(
            num: static _ => 1,
            sym: static _ => 1,
            sum: static s => 1 + Math.Max(s.Left.Depth, s.Right.Depth));

        public Fin<double> Eval(HashMap<string, double> env) => source.Switch(
            env,
            num: static (_, n) => FinSucc(n.Value),
            sym: static (e, s) => e.Find(s.Name).ToFin(Error.New($"<unbound: {s.Name}>")),
            sum: static (e, s) =>
                from l in s.Left.Eval(e)
                from r in s.Right.Eval(e)
                select l + r);
    }

    extension(Formula.Sum source)
    {
        public Formula.Sum Mirrored => new(source.Right, source.Left);
    }
}
```

[RECEIVER_ALGEBRA]:
- The generated surface of a regular union is exactly four member families — the metadata row, the optional private constructor, the dispatch family, and conversion operators from unique payload types — and nothing else: no per-case probes, no generated ordering. Equality, printing, and deconstruction are record-infrastructure or case-owned, not generator output; the generator's contribution is the case vocabulary and total dispatch, nothing more.
- Extension blocks are the third behavior placement alongside case virtuals and call-site dispatch: operation families attach to the closed owner as instance-shaped members and properties whose bodies are total `Switch` — adding a case still breaks them at compile time, since the property body is a dispatch call site like any other — without reopening the owner declaration or scattering arms across consumers. Static extension members host seed and combinator families on the owner's name; a per-case receiver block puts payload-specific operations on the case static type alone, visible exactly after a type pattern narrows.
- Neither union family generates any ordering surface. Rank over cases is a `Map` to preallocated ordinals — the verdict-table idiom doubles as the comparison key — and a comparer composes as an extension member over that projection; hand-implementing comparison interfaces on the owner re-opens a surface the family deliberately lacks.
- Operator algebra over a union (combining two verdicts, sequencing two outcomes) lives in extension operator declarations keyed to the owner; conversion operators are the exception the language reserves to the declaring type, and the generator already owns those — the split is exact: combination is extension territory, admission is generator territory.

[COMPOSITION_LIMITS]:
- An implicit conversion chain admits at most one user-defined operator, so union-in-union composition never lifts a raw payload through two generated operators in one expression: the inner lift must be spelled — an explicit cast to the inner union or its factory — before the outer operator applies. Collection-expression absorption inherits the same ceiling: a mixed batch lifts element-wise only into the immediate union, never through a nested one.

[DESIGN_PRESSURE]:
- Adding a case to a regular union breaks every total `Switch`/`Map` call site at compile time — that propagation is the contract, not a cost; partial overloads route the new case to `@default` silently. Reserve partial dispatch for semantically total defaults (telemetry, fallthrough rendering) and never for routing; enable `DefaultWithPartialOverloads` only on unions whose consumers genuinely need sparse handling, because every partial overload is a place exhaustiveness no longer guards.
- Union and inheritance-with-virtuals compose: cases are ordinary types, so case-owned invariants live as abstract or virtual members on the owner while consumer-owned reactions live in generated `Switch`. The per-behavior decision is "who must change when a case is added" — the owner file (virtual member, closed-world edit in one place) or every call site (dispatch, compile-time sweep of all consumers); a behavior whose correctness depends on seeing all cases at once belongs in `Switch`, a behavior intrinsically each case's own belongs on the case. A hot, wide regular family pushes per-case behavior onto the cases as virtual members and reserves generated `Switch` for consumer seams where compile-time totality is the point — the dispatch forms differ in cost model, not just in who owns the behavior.
- Operation/intent families: one regular union per verb family with per-case payloads and one state-threaded `Switch(state, static ...)` as the single total dispatcher returning a rail value — callbacks stay static, context rides `TState`, failure rides `Fin<T>`/`Validation<Error,T>`, and "unhandled verb" ceases to exist as a runtime concept.
- Result/evidence families: one regular union with a case per outcome kind, each carrying its typed evidence payload; `Map` renders constant verdict tables, stop-at overloads let coarse consumers treat an evidence subtree as one verdict while fine consumers stay exhaustive — the same family serves both fidelities without parallel types. Record root is the natural fit: flat sealed cases, value equality, and zero hand-written comparison code.

[RUNTIME_AND_BUILD_SEAM]:
- The runtime metadata lookup resolves case types to their owning family by walking base types for the static metadata property and caching the answer per queried type — infrastructure holding only a leaf instance and its runtime type reaches the closed member list without assembly scanning. The candidate filter short-circuits primitives, arrays, enums, and pointers, and nullable wrappers are unwrapped before lookup, so the seam is safe to call speculatively on arbitrary types in serializer and mapper hot paths.
- The generator itself is observable from the build: MSBuild properties enable file logging with level, per-process unique paths, and buffer sizing, plus an execution counter that makes incremental-cache misses measurable — the diagnosis route when generation appears stale or a build slows under wide union families. A separate property disables emission of the consumed-callback annotations, the remedy when source-sharing between projects collides the generated annotation types behind the type-conflict warning that generated dispatch already pragma-suppresses.
