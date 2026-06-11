# Unions — Distilled

[FAMILY_FORMS]:
- Two generated union families. Regular: `[Union]` on an abstract partial class or abstract partial record; cases are nested types derived from the owner, discovered recursively at every nesting depth; the owner is always a reference type. Ad-hoc: `[Union<T1,...,T5>]` (2–5 member types) or `[AdHocUnion(typeof(T1), ...)]` (non-generic mirror, identical per-member options) on a partial class, partial struct, or ref partial struct; member types may be named types, arrays, or the owner's own type parameters.
- Hard shape edges: one union attribute per type (second is an analyzer error); records cannot be ad-hoc owners; ad-hoc owners reject primary constructors; both owners must be partial; an ad-hoc union below two member types is rejected.
- The attribute surfaces are asymmetric: regular `[Union]` is sealed and owns only `SwitchMethods`, `MapMethods`, `ConversionFromValue`, `SwitchMapStateParameterName`, `NestedUnionParameterNames`. The ad-hoc forms derive from a wider base adding `ConversionToValue`, `ConstructorAccessModifier`, `DefaultStringComparison`, `SkipToString`, `SkipEqualityComparison`, `UseSingleBackingField`, `FactoryMethodGeneration`, plus per-member `TxName`/`TxIsNullableReferenceType`/`TxIsStateless` — options such as `ConstructorAccessModifier` are legal only on ad-hoc owners and do not exist on the regular attribute.
- Selection pressure: ad-hoc composes already-existing types into one payload alternative with value semantics and no hierarchy; regular mints a new closed concept whose cases carry distinct named payloads and can host case-owned behavior. An ad-hoc union spelling exactly success-or-failure re-derives the outcome rails — rails own outcome transport; unions own domain variant vocabularies whose cases demand distinct continuations at call sites.

[CASE_KIND_REGIME]:
- The owner's declaration kind is family-global: `abstract partial record` versus `abstract partial class` on the root fixes every case's kind at once, so equality, `ToString`, `with`, and `Deconstruct` are a root-level regime decision, never a per-case choice. Regular unions generate no equality and no `ToString` — identity is case-owned: record cases get per-case value equality, class cases keep reference identity.
- A record owner synthesizes `==`/`!=` routing through virtual `Equals` plus `EqualityContract`: two owner-typed references compare by runtime case and payload — whole-family structural equality with zero hand-written code. A class owner leaves owner-typed `==` as reference identity; no structural comparison exists anywhere in the family unless hand-written.
- `EqualityContract` makes cross-case equality constant-false even when two cases carry identical payload shapes, so a record family needs no discriminator guard before comparing — distinct folds, set membership, and memo keys are case-exact for free.
- Record-rooted families are flat by analyzer law (every record case sealed; no nested unions inside records); depth — intermediate abstract cases, nested dispatch surfaces, stop-at boundaries — requires a class root and surrenders generated structural equality. The trade is exactly evidence family (record root: value semantics, flat) versus intent tree (class root: depth, reference identity).
- `with` is legal at the owner static type because the synthesized clone is virtual, but it can rebind only owner-declared members there; rebinding case payloads demands the case static type — non-destructive payload mutation lives behind a type pattern, and its result stays inside the family.
- Positional record cases ship `Deconstruct`, so boundary probes compose recursive patterns (`x is Owner.CaseA(var p, _)`) without touching dispatch; interior code still owes totality to generated `Switch`, because language `switch` over a class hierarchy never proves exhaustiveness and the silencing `_` arm is precisely the hole the generated surface closes.

[CLOSURE_MECHANICS]:
- Closure is constructor reachability, not attribute law: the analyzer demands owner constructors be private (TTRESG009), every case sealed or private-constructors-only (TTRESG054), record cases sealed unconditionally (TTRESG055) — the attribute contributes discovery and dispatch; the access modifier contributes the closure. When the owner declares no constructor the generator emits a private parameterless one; declaring any owner constructor suppresses the generated one, and the analyzer immediately re-demands privacy on the declared replacement.
- Multi-level case trees therefore require class owners, each intermediate abstract case carrying a private constructor that its own nested cases still reach by containment. A non-abstract case less accessible than its owner is an error (TTRESG056); a case declaring its own type parameters aborts generation for the whole union (TTRESG053); a nested type that does not derive from the owner is only a warning (TTRESG106) — nested-but-foreign types are the single shape the closure scan tolerates.
- Cases nested inside other cases are members of the root union too; an intermediate abstract case becomes its own dispatch surface only by carrying its own `[Union]`, and the nested union's generated `Map` deliberately hides the parent's — dispatch granularity follows the receiver's static type.
- Phantom-case hazard: case discovery is a containment walk that recurses only through class-kind containers. A case nested beneath a struct or interface intermediate inside the owner is invisible to both generation and analysis, yet deriving there is fully legal (private-member access extends to any nesting depth) — the family compiles clean with a phantom case whose first dispatch lands in the default arm's runtime throw. The nested-but-foreign audit reads only first-level members, so depth also hides foreign types from that warning. Non-class intermediates inside a union owner are forbidden shape, enforceable only by review.
- A case-less union owner generates nothing at all: discovery bails before emission — no metadata row, no private constructor, no dispatch surface, and no diagnostic reports the empty family. The closed family materializes retroactively when its first case lands; consumers written ahead of it fail to find `Switch` rather than failing exhaustiveness.
- Case payload design is ordinary type design with triple duty: positional record or primary-constructor class parameters are simultaneously the named payload, the single-argument-constructor feed for owner-level conversion operators, and the value generated dispatch hands to the case callback — one declaration drives admission, conversion, and dispatch.
- Unions carry no validation partial: each case is a full type that admits its own payload, and the union composes already-admitted material. Closed custom admission requires non-public construction plus suppressed conversions plus a hand-written factory returning a rail value.

[GENERATED_DISPATCH]:
- Per member set the generator can emit eight `Switch` overloads (action/func × stateless/state-threaded × total/partial) and two `Map` overloads; partial forms exist only under `SwitchMapMethodsGeneration.DefaultWithPartialOverloads`; `None` suppresses the family entirely — the data-carrier setting for wire-shape unions.
- Total `Switch`/`Map` parameters cover every non-abstract member; parameter names derive from case names (camelCase, keyword-escaped); nested cases are prefixed with intermediate type names under `NestedUnionParameterNameGeneration.Default`, while `Simple` drops the prefix and produces duplicate-parameter compile errors the moment sibling subtrees reuse a case name.
- Regular dispatch is a most-derived-first type-pattern `switch (this)` ordered by topological sort of the case hierarchy (leaves before ancestors); the default arm throws `ArgumentOutOfRangeException` naming the runtime type's full name — unreachable for an intact closure, reachable from another assembly subverting the private constructor or via the phantom-case route. Ad-hoc dispatch switches on a private 1-based `int _valueIndex` (0 = uninitialized struct) — an allocation-free jump table rather than runtime type tests.
- State-threaded forms (`Switch<TState>`, `Switch<TState,TResult>`) thread exactly one explicit state value; both `TState` and `TResult` carry the `allows ref struct` anti-constraint, so span-shaped state and results flow through dispatch without boxing.
- Generated members carry `DebuggerStepThroughAttribute` and `DebuggerNonUserCodeAttribute` — stepping into a `Switch` lands directly in the chosen callback — but `Exception.StackTrace` from the default-arm throw still contains the generated `Switch` frame (no `[StackTraceHidden]` is emitted), making the default arm's runtime-type message the entire triage signal for cross-assembly closure subversion.
- Callback parameters are annotated consumed-before-return for analysis tooling, and IDE quick actions scaffold the complete named-argument set for every dispatch form — add the case, re-scaffold every red call site, and the compile break becomes the repair loop.

[MAP_EVALUATION_LAW]:
- `Map<TResult>` takes one eager value per case — every branch value is evaluated before dispatch — so `Map` is for preallocated constant verdict tables and cheap projections, `Switch` for computation. The eagerness amplifies allocation, not merely computation: a `Map` whose arms allocate builds N-1 dead values per dispatch; any allocating arm belongs in `Switch`.
- For task-shaped arms the hazard is execution: every branch's task is already running before dispatch selects one, the losers are abandoned mid-flight, their side effects land anyway, and their failures surface as unobserved-task faults far from the call. No task-valued expression belongs in a `Map` arm; the func-form `Switch` with `TResult` instantiated to the task shape is the only async dispatch. The safe computation-shaped `Map` arms are cold carriers — effect descriptions and already-settled rail values — because selection and execution stay in separate phases.
- `MapPartially` parameters are `Argument<TResult>`, a readonly ref struct with an implicit lift from `TResult` and an `IsSet` flag, so "not provided" is distinguishable from a deliberately provided `default(TResult)`.
- Partial-form `@default` asymmetry: func `SwitchPartially` requires `@default` as its first parameter; action `SwitchPartially` makes it optional and nullable; `MapPartially` requires a `@default` value. Per-case resolution is hierarchical, not flat: exact handler, else the nearest non-abstract ancestor's handler walking up the case tree, else `@default`.
- Partial dispatch routes a newly added case to `@default` silently — every partial overload is a place exhaustiveness no longer guards. Reserve partial forms for semantically total defaults (telemetry, fallthrough rendering), never for routing, and enable `DefaultWithPartialOverloads` only on unions whose consumers genuinely need sparse handling; carrier-polymorphic folds stay on the total forms, because a partial fold silently defaults the new case in every carrier specialization at once.

[STOP_AT_BOUNDARIES]:
- `[UnionSwitchMapOverload(StopAt = new[] { typeof(CaseX) })]` (multiple allowed) generates extra Switch/Map overloads whose member set removes the entire derived subtree of each stop type and keeps the stop type itself even when abstract; abstract non-stop members are dropped; a stop type absent from the hierarchy yields no overload; overloads with identical member sets deduplicate — identical sets yield identical parameter names and types, so the surviving overload binds every coarse call site unchanged and deduplication alone breaks nothing.
- The real breakage surface is a stop type leaving the hierarchy entirely: the absent-type rule fires, the stop-at overload collapses to a smaller set, and consumers written against the former coarse shape lose the named callback they depended on — treat each declared stop boundary as a published overload whose existence is contingent on the stop type remaining in the hierarchy.
- Stop-at overloads let a coarse consumer treat a nested case subtree as one case while sibling leaves stay exhaustive; because callback arguments must be named, same-arity overloads resolve cleanly by parameter name. Deep trees route at two granularities with both levels total: the stop-typed handler re-dispatches on the intermediate's own nested generated surface, and a case added anywhere breaks exactly the dispatch level that names its subtree.
- The coarse handler is also the natural seam to re-enter a different carrier: the outer fold runs under a short-circuiting carrier while the stopped subtree re-dispatches under an accumulating one — blast-radius granularity and failure-accumulation granularity are configured at the same declared boundary, and the carrier change rides that seam without either fold naming the other's carrier.

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

[CALL_SITE_LAWS]:
- Positional callback arguments on any union `Switch`/`Map` are an analyzer error (TTRESG046) — every case argument is name-colon'd; the state argument is exempt. Case reorder is therefore call-site-safe, but renaming a case is a source-breaking change at every dispatch site: generated parameter names are public API.
- The named-argument law is mechanical: the single positional slot is a leading argument whose original parameter type is a method type parameter distinct from the method's return type — exactly the state parameter of state-threaded forms. `Map`'s first arm fails the test because its parameter type is the return type itself, so every `Map` argument is named including the first; the partial forms' default slot is spelled `@default:`.
- `SwitchMapStateParameterName` exists for collision pressure: state name and case argument names share one parameter list, so a case whose derived argument name collides with `state` forces renaming the state parameter, not the case.
- `TResult` inference fails when branches return different-but-compatible types; pin the type argument (`Switch<TCommon>(...)`) instead of upcasting inside branches.
- The static-lambda advisory (TTRESG1001) inspects only `Switch`/`SwitchPartially` invocations in member-access form; `Map` arms are values, never inspected. A clean pass does not prove every arm is capture-free — capture review on wide dispatches stays manual, and all captured state moves into the threaded `TState` parameter to satisfy the diagnostic.
- The partial-map arm carrier is a ref struct and arm expressions convert to it at evaluation, so an `await` to the right of any already-evaluated arm in a `MapPartially` argument list is a compile error — evaluated carriers cannot spill across the await. The `Switch` forms are immune (heap delegates); the stable async form hoists awaited values into locals before the dispatch expression.
- Async lambda arms and span-shaped state are mutually exclusive: async lambdas cannot declare ref-struct parameters, so an async-lambda arm requires heap-shaped state. A synchronous lambda arm may legally thread span state and return a task-shaped `TResult` — the exclusion is on the lambda's async modifier, not on the result type. Choose the state shape before writing arms; no overload bridges the two.

[CARRIER_SEAMS]:
- Each non-abstract case is the natural Kleisli point: the case payload is the input, the arm body is the lift into the carrier, and generated total `Switch` is the only place the family's continuation is selected — every arm produces a rail value, the dispatcher never sees a bare value, and the carrier's own combination sequences the children.
- Arm-set uniformity is load-bearing, not stylistic: a bare-value arm beside rail-valued arms forces a result type the carrier cannot combine, so a constant or never-failing arm writes the carrier's pure lift rather than a concrete success constructor — which simultaneously keeps the arm portable across every carrier the fold is later specialized to.
- A recursive case's arm is a traversal, not a hand-written sequencer: it collects child folds as kinded values and combines them through the applicative tuple combinator, naming the pure combine and nothing about success-versus-failure. One arm body is therefore correct under fail-fast and accumulate alike; the failure policy is inherited from the carrier, never spelled in the arm, and the only edit that flips the whole fold between policies is the trait its `where` clause demands.
- The state channel and the result channel are orthogonal: state rides the environment — bindings, depth budget, configuration — while the result parameter is instantiated to the carrier. The environment never leaks into the failure rail and the rail never leaks into the environment, so a reader-shaped fold and a fallible fold compose on one dispatch without nesting two monads whenever the environment is plain data; the second monad is earned only when the environment itself is effectful.
- The ref-struct result channel and the kinded heap carrier are mutually exclusive on one fold: generated dispatch admits a stack-only result type, so a span-shaped accumulator can be the dispatch result without boxing — but a kinded carrier is heap-shaped, so the two result disciplines are chosen per fold by whether the accumulation must escape the stack, never mixed in one dispatch.

```csharp
public static K<F, B> Fold<F, B>(this Node source, Func<Leaf, K<F, B>> onLeaf, Func<B, B, B> combine)
    where F : Applicative<F> =>
    source.Switch<Func<Node, K<F, B>>, K<F, B>>(
        onLeaf,
        leaf:   static (f, l)  => f(l),
        branch: static (f, br) => Applicative.apply((br.Left.Fold(f, combine), br.Right.Fold(f, combine)), combine));
```

[VERSION_SURFACE]:
- The case list is part of every dispatch method's signature, which makes case addition a binary break, not merely a source break: each `Switch`/`Map`/partial overload is re-signatured with one more parameter, the old overload ceases to exist, and consumer assemblies compiled against the previous shape fail at JIT bind with a missing-method fault — loud, early, impossible to misroute; that propagation is the contract, not a cost. Optional parameters on the partial forms are baked into caller IL, so partial dispatch is equally binary-broken; no generated dispatch surface is hot-swap stable across case addition.
- A carrier-polymorphic fold's arm set is re-signatured on case addition too, so every specialization at every carrier breaks at compile time in lockstep — the carrier abstraction widens reuse without widening the silent-failure surface, since totality is proven per dispatch site regardless of carrier.
- Case rename breaks on two independent axes: generated callback parameter names are source-level API (the named-argument law makes every call site spell them), and the nested case type name is embedded in every consumer's callback signatures and type patterns, so rename is also a binary break via type load. Payload-shape changes that keep the case name and arity leave dispatch call sites untouched and break only the arms touching the changed member — the dispatch surface partitions blast radius by axis: identity changes hit every consumer, payload changes hit only handlers.
- Language type patterns are the only binary-stable consumers across a case-list change — and they are exactly the consumers that surrendered totality. Shipping a union across an assembly boundary commits every consumer to lockstep rebuilds; inside one build graph, that cost is zero.

[BEHAVIOR_PLACEMENT]:
- The generated surface of a regular union is exactly four member families — the metadata row, the optional private constructor, the dispatch family, and conversion operators from unique payload types — nothing else: no per-case probes, no generated ordering; equality, printing, and deconstruction are record-infrastructure or case-owned, never generator output.
- Extension blocks are the third behavior placement alongside case virtuals and call-site dispatch: operation families attach to the closed owner as instance-shaped members and properties whose bodies are total `Switch` — adding a case still breaks them at compile time, since the property body is a dispatch call site like any other — without reopening the owner or scattering arms across consumers. Static extension members host seed and combinator families on the owner's name; a per-case receiver block puts payload-specific operations on the case static type alone, visible exactly after a type pattern narrows.
- The per-behavior decision is "who must change when a case is added": the owner file (virtual member, closed-world edit in one place) or every call site (dispatch, compile-time sweep of all consumers). A behavior whose correctness depends on seeing all cases at once belongs in `Switch`; a behavior intrinsically each case's own belongs on the case. A hot, wide family pushes per-case behavior onto the cases as virtual members and reserves generated `Switch` for consumer seams where compile-time totality is the point — the dispatch forms differ in cost model, not just ownership.
- Neither union family generates any ordering surface. Rank over cases is a `Map` to preallocated ordinals — the verdict-table idiom doubles as the comparison key — and a comparer composes as an extension member over that projection; hand-implementing comparison interfaces on the owner re-opens a surface the family deliberately lacks.
- Operator algebra over a union (combining two verdicts, sequencing two outcomes) lives in extension operator declarations keyed to the owner; conversion operators are the exception the language reserves to the declaring type, and the generator already owns those — combination is extension territory, admission is generator territory.

[FAMILY_ARCHETYPES]:
- Operation/intent families: one regular union per verb family with per-case payloads and one state-threaded `Switch(state, static ...)` as the single total dispatcher returning a rail value — callbacks stay static, context rides `TState`, failure rides the rail, and "unhandled verb" ceases to exist as a runtime concept.
- Result/evidence families: one regular union with a case per outcome kind, each carrying its typed evidence payload; `Map` renders constant verdict tables, stop-at overloads let coarse consumers treat an evidence subtree as one verdict while fine consumers stay exhaustive — the same family serves both fidelities without parallel types. Record root is the natural fit: flat sealed cases, value equality, zero hand-written comparison code.
- Union and inheritance-with-virtuals compose, not compete: cases are ordinary types, so case-owned invariants live as abstract or virtual members on the owner while consumer-owned reactions live in generated `Switch`.

[RECURSIVE_FAMILIES]:
- Self-referential families get exactly the right conversion ergonomics for free: the operator-generation rule that skips payload types related to the case by inheritance means a case carrying the owner type never receives a lifting operator, while leaf cases over foreign payload types keep theirs — induction steps are always constructed explicitly, leaf admission stays implicit, and the asymmetry is generated, not policed.
- The catamorphism shape is an extension member recursing through state-threaded `Switch`: the environment rides the state parameter, the failure rail rides the result type, callbacks stay static because the case payload and the threaded state are the only inputs, and the recursion lands back on the extension member so the fold is named once.
- Generated dispatch is depth-honest: each recursion level costs one type-test ladder plus one delegate invocation and two stack frames, so unbounded or hostile input demands an explicit-stack kernel at the admission boundary — a named statement exemption — rather than recursive dispatch in the interior.
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

[GENERIC_FAMILIES]:
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

[CONVERSION_OPERATORS]:
- Regular unions generate operators on the owner from each case's public single-argument constructor parameter type — only when that parameter type is unique across all cases' single-argument constructors, the case is non-abstract, declares no required members, and the parameter type is neither interface, `object`, nor in a base/derived relation with the case itself. Payload-type collision anywhere in the family silently deletes both operators: conversion ergonomics degrade as case payloads converge, by design, and dispatch remains the only total surface.
- An implicit conversion chain admits at most one user-defined operator, so union-in-union composition never lifts a raw payload through two generated operators in one expression: the inner lift must be spelled — an explicit cast to the inner union or its factory — before the outer operator applies. Collection-expression absorption inherits the same ceiling: a mixed batch lifts element-wise only into the immediate union, never through a nested one.

[AD_HOC_REPRESENTATION]:
- The backing-field set is computed by an exact predicate, not one-per-member: one typed readonly field per stateful unique member plus the int discriminator, except that a shared `object?` slot is emitted once and absorbs every reference member the moment the count of stateful, non-type-parameter, non-duplicate reference members reaches two. Adding a third reference case can therefore flip the entire family's storage from typed fields to one boxed slot — a silent representation change observable only in the memory diagram, with zero source or behavior difference.
- Struct members never join the shared slot: each stateful value-type member retains a dedicated inline field, so the owner sizes to its tag plus its widest struct-member cluster plus one shared pointer once the collapse fires — never to the sum of all payloads. `[StructLayout(LayoutKind.Auto)]` is emitted unless a layout is user-declared, handing the runtime permission to overlap-pack and reorder fields and tag, deleting the padding holes a hand-declared sequential layout would freeze in. `UseSingleBackingField` forces the single object field for everything, boxing struct members at construction and making `Value` return the raw field.
- `TxIsStateless` makes a member a zero-width alternative: no field, accessor returning `default(T)`, intra-case equality constant-true, the hash the member type's `typeof` hash, the case's entire runtime identity its index — and stateless reference members auto-report as nullable (`TxIsNullableReferenceType` inherits the stateless flag), so prefer empty structs as stateless markers: strictly cheaper than a nullable reference marker because there is no null channel to mis-handle.
- Ref-struct ad-hoc owners are the extreme of the value form: `Equals(object?)` hard-returns false, the equatable and operator interfaces are skipped entirely, and `allows ref struct` anti-constraints on the owner's own type parameters are rejected (TTRESG073) — an equality-less, stack-bound carrier whose sole purpose is moving one of several span-shaped payloads through a single dispatch without heap escape.
- Struct versus class relocates the absence discipline rather than removing it, and the two invalid states differ in nameability: the struct's poison is unnameable but free to construct; the class's null is nameable and policed by the entire existing nullability apparatus. Where the union lives in large arrays or hot value-type flow, the allocation win selects the struct; where it must round-trip reflection, serialization, or any path that zero-fills storage, the class's honest null is worth the per-value allocation precisely because those paths mint the struct's poison invisibly.

[DEFAULT_POISON]:
- `default` of a struct ad-hoc union is poisoned, not valid: index 0 makes `Value`/`ToString`/`GetHashCode`/`Equals`/`Switch`/`Map` throw `InvalidOperationException` naming the struct type and advising initialization — which means hashing or logging a poisoned union is itself the crash site.
- The poison hides inside a fully-inhabited static type: a default-constructed union satisfies not-null annotations, flows through non-nullable parameters, and lives in arrays and uninitialized fields with zero diagnostic, because the invalid state is a discriminant value the type system cannot name. The only proof of validity is a runtime probe, never an annotation.
- The crash site is the first observation, not the minting site: array allocation, an unconstrained generic `default`, or a zero-filled outer struct manufactures the poison far from where hashing, logging, or comparing later throws — triage must walk back to the allocation, not the throw. The default-poison analyzer catches literal `default(T)`, bare `new T()`, and undeclared required fields (TTRESG047, TTRESG104) — but `new T[n]` elements, `default(TGeneric)` through unconstrained generics, and uninitialized struct fields nested inside other structs all evade it; the runtime throw set is the backstop.
- `Is{Member}` is the one generated probe total over the poisoned state — a pure discriminator compare reading zero as "not this case" rather than as an error. Every rehydration, pooling, array-scan, and deserialization seam guards with the probe before touching any value-bearing member, and the disjunction of all member probes reconstructs "is this value initialized at all" — the typestate test the language refuses to express, recovered as a fold over probes.
- The generated throw set is a three-tier taxonomy that localizes which invariant broke from the exception type alone: invalid-operation for the poisoned default and for wrong-case projection on a correctly initialized value; index-out-of-range for a corrupted discriminant reaching the `Value` and `Switch` surfaces; argument-out-of-range for the same corruption reaching the `Map`/`MapPartially` surfaces — the split is `Value`+`Switch` versus `Map`, not per-surface. The corrupted-discriminant arms are reachable only by binary corruption, torn struct reads, or reflection-built instances bypassing the constructors — they convert silent memory corruption into a named exception at first dispatch rather than a wrong-arm execution.

```csharp
[Union<Anchor, Drift>(T2IsStateless = true)]
public readonly partial struct Cursor;

public readonly record struct Drift;

// A scan over possibly zero-filled slots guards on the throw-free probe,
// never on Value/Switch, which fault on the poisoned default.
public static Seq<Anchor> Anchors(ReadOnlySpan<Cursor> window)
{
    var live = Seq<Anchor>.Empty;
    foreach (var c in window)               // span kernel: the named statement exemption
        live = (c.IsAnchor, c.IsDrift) switch
        {
            (true, _) => live.Add(c.AsAnchor),
            _ => live,                       // index 0 and Drift both fall here, no throw
        };
    return live;
}
```

[AD_HOC_SURFACE]:
- Per member: `Is{Name}`, `As{Name}` (throws `InvalidOperationException` naming the requested type on mismatch), an object-typed `Value`, per-member constructors, implicit operators from member types, and explicit operators to member types that are cast-assertions throwing on the wrong case — `Switch` is the safe projection; conversions are boundary affordances, never interior control flow.
- Member names default to the type name; duplicate member types receive ordinal-suffixed names and need `TxName` for a usable surface; duplicates share one private `(value, valueIndex)` constructor so only factories can select the case; conversion operators are skipped for duplicates, interfaces, `object`, and type parameters — the mapping is ambiguous or illegal there.
- `FactoryMethodGeneration.Default` auto-emits `Create{Name}` for all members the moment any member is a type parameter, an interface, `object`, or a duplicate; `Always` forces factories with uniform shape; `None` suppresses them even when triggers exist.
- `ConstructorAccessModifier` flows to constructors and generated factories, but conversion operators are always emitted `public static` (a language constraint) — a non-public modifier without `ConversionFromValue = ConversionOperatorsGeneration.None` leaves the implicit operators as a public creation path bypassing the private constructors; closing admission requires setting both.
- Generated implicit conversions make an ad-hoc union a parameter-side absorber: one parameter of the union type replaces an overload per member, and collection expressions lift heterogeneous raw members element-wise through those conversions — a single `params ReadOnlySpan<U>` or array entrypoint accepts a mixed batch with zero call-site ceremony. The absorption boundary is exactly the language's: members typed `object`, interface, or type parameter legally cannot own conversion operators, so their presence flips the whole family to factory-method admission — call shape degrades from juxtaposition to named construction the moment the member vocabulary abstracts.

```csharp
[Union<string, long>(T1Name = "Tag", T2Name = "Serial")]
public partial struct Marker;

// Collection expressions lift mixed raw members through generated implicit conversions:
// one entrypoint, no overload family, no per-element allocation.
Marker[] batch = ["alpha", 42L, "beta"];
var weight = batch.Sum(static m => m.Switch(tag: static t => t.Length, serial: static _ => 8));
```

[AD_HOC_EQUALITY_IDENTITY]:
- Equality is discriminator-gated then member-dispatched: index inequality short-circuits before any field read, so cross-case comparison never touches a payload. Under the shared-slot collapse the emitted arm re-checks the index before the field compare, because one object field serves several cases and only the index disambiguates which member's equality semantics apply to the stored reference.
- String members compare and hash with the configured `DefaultStringComparison` — case-insensitive union equality is the silent default until overridden with `StringComparison.Ordinal`; type-parameter members route equality and hash through the default comparer of the closed instantiation, computed without boxing the value type; reference-member arms are null-safe by structural emission, not operator dispatch — a null-pattern test before the member's own equality — so a genuinely-null payload and a stateless-derived null compare structurally without faulting.
- The hash is the active member's hash without discriminator mixing, so cross-case hash collisions are possible while equality stays exact. `SkipToString` and `SkipEqualityComparison` opt out of each surface independently.
- Identity printing is asymmetric across the families: record cases print case-name-plus-payload; ad-hoc unions forward the raw member's `ToString`, erasing which alternative is active from every log line — two unions unequal by case can render identically. Where the active case matters observationally, project through `Map` to labeled text; `ToString` on an ad-hoc union is a payload printer, not an identity printer.

[WIRE_ADMISSION_FACTORY]:
- `[ObjectFactory<TValue>]` on a union owner declares a second admission route with analyzer-enforced shape: a user-written `static TError? Validate(TValue value, IFormatProvider? provider, out T item)` (TTRESG061), plus an instance `TValue ToValue()` the moment serialization or persistence is requested (TTRESG062). The generator implements the static-abstract factory and conversion interfaces and publishes a factory-metadata row carrying the value type and per-consumer routing flags.
- The failure channel is typed, not stringly: it defaults to the library validation-error and swaps to any type implementing the static-`Create(string)` error interface, so wire rejection speaks the same typed failure vocabulary as the rest of the boundary — one message-to-error constructor everywhere, declared once on the owner.
- `HasCorrespondingConstructor = true` is a proof obligation, not a hint: the owner must expose a single-argument constructor of the factory value type (TTRESG059), which metadata then exposes as a constructor-shaped conversion expression — an expression-tree admission path persistence providers can compile and translate where a method-call `Validate` is opaque.
- Multiple factory attributes partition by concern under analyzer guard: overlapping serialization-framework flags are an error, at most one factory may claim persistence, at most one model binding — every downstream consumer resolves exactly one admission grammar, mechanically. But conversion-route selection takes the last factory satisfying the consumer's filter, not the first: declaration order is load-bearing the moment two factories could satisfy one consumer, and appending a factory can silently re-route an existing consumer — order factory attributes general to specific and treat reordering as a behavior change.
- `[ObjectFactory<string>]` additionally generates the parse pair: `Parse` funnels through `Validate` and throws a format exception carrying the validation error's text; `TryParse` is the non-throwing rail. Both are public statics, so convention-based endpoint parameter binding discovers them with no registration.
- Deserialization is admission: every generated converter routes the wire value through the same `Validate`, so hostile or stale payloads fail into the serializer's error channel and a half-built union cannot exist in memory — interior code never re-checks wire trust.

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
- The polymorphism discriminator must lead the JSON object unless out-of-order metadata reading is enabled on the serializer options; producers that reorder or stream properties break round-tripping silently — pin the option at the boundary owner, never per call site. Discriminator strings restate case identity the program already knows; derive them from `nameof(Owner.Case)` or the published member list so renaming a case breaks compilation, not production decoding.
- Ad-hoc unions never carry a discriminator by design — single-value factory admission is the only wire route. Per-serializer converters are emitted only when that serializer's integration assembly is referenced: converter generation is keyed off the dependency graph at definition time, so adding the reference retroactively widens every factory-marked union with no source edit.
- One `[ObjectFactory<string>(UseForSerialization = All)]` on the owner collapses every wire concern — JSON, persistence, binding — onto one canonical grammar with one parser and one printer; the cost is opacity (no per-property wire schema). Correct for identifier-like unions; wrong for document-like evidence payloads whose properties downstream systems must address.
- Persistence by inheritance: cases map as a single-table hierarchy with one discriminator column and a value row per leaf; conventions do not discover nested types, so each case is registered explicitly — the closed member list turns that registration into a fold over the family rather than a hand-enumerated block, and per-case columns are configured on the case entity, never the owner.
