# Discriminated Unions — Discovery Edges, Version Surfaces, Receiver Algebra, and Effect-Arm Laws

[DISCOVERY_EDGES]:
- A case-less union owner generates nothing at all: discovery bails before emission, so no metadata row, no private constructor, and no dispatch surface exist, and no diagnostic reports the empty family. The closed family materializes retroactively when its first case lands; until then the owner is an ordinary abstract type, and consumers written ahead of the first case fail to find `Switch` rather than failing exhaustiveness.
- Case discovery is a containment walk that recurses only through class-kind containers. A case nested beneath a struct or interface intermediate inside the owner is invisible to both generation and analysis, yet deriving there is fully legal — private-member access extends to types nested at any depth — so the family compiles clean with a phantom case whose first dispatch lands in the default arm's runtime throw. This, not cross-assembly derivation, is the realistic closure-subversion channel: external assemblies are excluded by constructor accessibility before any analyzer fires, while an in-owner non-class container hides a case from the very surface that proves totality. The nested-but-foreign audit additionally reads only first-level members, so depth hides foreign types from that warning too. Non-class intermediates inside a union owner are therefore forbidden shape, enforceable only by review.
- Closure is constructor reachability, not attribute law: the analyzer's private-constructor demand on the owner is the entire proof that derivation is containment-scoped, and the generated parameterless constructor is merely its default instantiation — declaring any owner constructor suppresses the generated one, and the analyzer immediately re-demands privacy on the declared replacement. The attribute contributes discovery and dispatch; the access modifier contributes the closure.
[VERSION_SURFACE]:
- The case list is part of every dispatch method's signature, which makes case addition a binary break, not merely a source break: each `Switch`/`Map`/partial overload is re-signatured with one more parameter, the old overload ceases to exist, and consumer assemblies compiled against the previous shape fail at JIT bind with a missing-method fault — loud, early, and impossible to misroute. Optional parameters on the partial forms are baked into caller IL at compile time, so partial dispatch is equally binary-broken; no generated dispatch surface is hot-swap stable across case addition.
- Case rename breaks on two independent axes: generated callback parameter names are source-level API (named-argument law makes every call site spell them), and the nested case type name is embedded in every consumer's callback signatures and type patterns, so rename is also a binary break via type load. Conversely, payload-shape changes inside a case that keep the case name and arity leave dispatch call sites untouched and break only the arms that touch the changed member — the dispatch surface partitions blast radius by axis: identity changes hit every consumer, payload changes hit only handlers.
- Stop-at overload sets are keyed by member-set equality, so removing or sealing a leaf can collapse a stop-at overload into identity with the total overload, at which point deduplication silently deletes it and call sites bound to the coarse shape break against the union's own evolution. Treat each declared stop boundary as a published overload whose existence is contingent on the subtree staying non-trivial.
- A published union is a recompilation contract: the only binary-stable consumers across any case-list change are language type patterns, and those are precisely the consumers that surrendered totality. Shipping a union across an assembly boundary commits every consumer to lockstep rebuilds; inside one build graph that contract is free.
[CALL_SITE_MECHANICS]:
- The named-argument law is mechanical and worth knowing exactly: the single positional slot is a leading argument whose original parameter type is a method type parameter distinct from the method's return type — which matches exactly the state parameter of state-threaded forms. `Map`'s first arm fails the test because its parameter type is the return type itself, so every `Map` argument is named including the first; the partial forms' default slot is a named argument spelled with the verbatim identifier, `@default:`.
- The static-lambda advisory inspects only `Switch`/`SwitchPartially` invocations in member-access form and reports the first offending lambda per call — `Map` is exempt because its arms are values, and a second capturing lambda in the same call hides behind the first. A clean pass on the diagnostic is therefore per-call, not per-arm; capture review on wide dispatches is still manual.
- The partial-map arm carrier is a ref struct, and arm expressions convert to it at evaluation, so an `await` appearing to the right of any already-evaluated arm in a `MapPartially` argument list is a compile error — the evaluated carriers cannot be spilled across the await. The `Switch` forms are immune (their arguments are heap delegates); the stable async form hoists awaited values into locals before the dispatch expression.
- Span-shaped state and asynchronous arms are mutually exclusive by type-system law, not library choice: async lambdas cannot declare ref-struct parameters, so a `Switch` threading a span as state is synchronous by construction, and a `Switch` returning a task shape requires heap-shaped state. Choose the dispatch's regime before choosing its state type — there is no overload that gives both.
[EFFECT_ARMS]:
- `Map` arms are evaluated values, and for task-shaped arms evaluation means execution: every branch's task is already running before dispatch selects one, the losers are abandoned mid-flight, their side effects land anyway, and their failures surface as unobserved-task faults far from the call. Single-consumption task shapes add a second violation when the abandoned branch came from a pooled source. No task-valued expression belongs in a `Map` arm under any circumstance; the func-form `Switch` with the result type instantiated to the task shape is the only async dispatch.
- The safe computation-shaped `Map` arms are cold carriers: effect descriptions and already-settled rail values cost allocation when eagerly evaluated, never execution. The eagerness law thus splits by the arm's semantics — values and descriptions ride `Map`, computations and processes ride `Switch` — and a union of effect descriptions selected by `Map` then run once is a legitimate verdict-table idiom, because selection and execution stay in separate phases.
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
- The generated surface of a regular union is exactly four member families — the metadata row, the optional private constructor, the dispatch family, and conversion operators from unique payload types — and nothing else: no per-case probes, no equality, no printing, no ordering. Everything beyond dispatch is deliberately case-owned (record semantics), language-owned (type patterns, deconstruction), or attachable from outside; the owner file's job is the case vocabulary and nothing more.
- Extension blocks are the third behavior placement alongside case virtuals and call-site dispatch: operation families attach to the closed owner as instance-shaped members and properties whose bodies are total `Switch`, giving owner-adjacent behavior that adding a case still breaks at compile time — the property body is a dispatch call site like any other — without reopening the owner declaration or scattering arms across consumers. Static extension members host seed and combinator families on the owner's name, and a per-case receiver block puts payload-specific operations on the case static type alone, visible exactly after a type pattern narrows.
- Neither union family generates any ordering surface. Rank over cases is a `Map` to preallocated ordinals — the verdict-table idiom doubles as the comparison key — and a comparer composes as an extension member over that projection; hand-implementing comparison interfaces on the owner re-opens a surface the family deliberately lacks.
- Operator algebra over a union (combining two verdicts, sequencing two outcomes) lives in extension operator declarations keyed to the owner; conversion operators are the exception the language reserves to the declaring type, and the generator already owns those — the split is exact: combination is extension territory, admission is generator territory.
[COMPOSITION_LIMITS]:
- An implicit conversion chain admits at most one user-defined operator, so union-in-union composition never lifts a raw payload through two generated operators in one expression: the inner lift must be spelled — an explicit cast to the inner union or its factory — before the outer operator applies. Collection-expression absorption inherits the same ceiling: a mixed batch lifts element-wise only into the immediate union, never through a nested one.
- A generic union root is a phantom-typestate carrier: cases close over the root's type parameters without declaring their own, so one declaration yields a family per marker instantiation, operations select legal phases by constraint, and dispatch stays total within each phase. The cost is invariance — no common view spans instantiations, so storage and transport boundaries need either an explicit erasing projection or a separate union over the instantiations; phantom phases are free at dispatch and paid at the container.
- Deep trees route at two granularities with both levels total: a stop-at overload on the root folds a subtree behind its abstract stop type, and the stop-typed handler re-dispatches on that intermediate's own nested generated surface — coarse consumers see one arm, fine consumers see leaves, and a case added anywhere breaks exactly the dispatch level that names its subtree. The two-level recipe is the alternative to flattening wide families: closure stays at the root, exhaustiveness pressure distributes down the tree.
[RUNTIME_AND_BUILD_SEAM]:
- The runtime metadata lookup resolves case types to their owning family by walking base types for the static metadata property and caching the answer per queried type — infrastructure holding only a leaf instance and its runtime type reaches the closed member list without assembly scanning. The candidate filter short-circuits primitives, arrays, enums, and pointers, and nullable wrappers are unwrapped before lookup, so the seam is safe to call speculatively on arbitrary types in serializer and mapper hot paths.
- Conversion-route selection takes the last factory satisfying the consumer's filter, not the first: factory attribute declaration order is load-bearing the moment two factories could satisfy one consumer, and appending a factory can silently re-route an existing consumer — order factory attributes from general to specific and treat reordering as a behavior change.
- The generator itself is observable from the build: MSBuild properties enable file logging with level, per-process unique paths, and buffer sizing, plus an execution counter that makes incremental-cache misses measurable — the diagnosis route when generation appears stale or a build slows under wide union families. A separate property disables emission of the consumed-callback annotations, the remedy when source-sharing between projects collides the generated annotation types behind the type-conflict warning that generated dispatch already pragma-suppresses.
