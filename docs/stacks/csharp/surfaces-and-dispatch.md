# [SURFACES_AND_DISPATCH]

A concern with many features keeps one dense surface, never a family of shallow ones: one entrypoint absorbs every verb, arity, and modality; one trait core derives a whole operation family; one carrier-polymorphic arrow dispatches `Fin`, `Eff`, `IO`, and `Validation` at once; one keyed declaration is table, vocabulary, and admission factory together. Verbs collapse into a request `[Union]` under one total dispatch, so a new verb breaks every site instead of growing a sibling method. Arity collapses into `params ReadOnlySpan<T>`, collection expressions, and one carrier — the discriminant is the value's shape, never a mode flag beside it. Knob sets collapse into policy values; optional context enters as `Option<T>` or one runtime record. Five dispatch forms are selected by where ownership lives, the carrier is orthogonal to the form and decides accumulate-versus-abort, and aspects split at one seam: definition-time weaves below the admission boundary in generator order, composition-time transformers above it in author order.

## [1]-[FORM_CHOOSER]

When a concern matches several rows, the most specific wins; the carrier axis is read after the form is fixed.

| [INDEX] | [CONCERN_SIGNATURE]                           | [FORM]                                            | [REJECTED_FORM]                   |
| :-----: | :-------------------------------------------- | :------------------------------------------------ | :-------------------------------- |
|   [1]   | verb family, shared preamble                  | request `[Union]` + total `Switch`                | sibling `Create`/`Update` methods |
|   [2]   | one verb, varying arity                       | `params ReadOnlySpan<T>` absorber                 | per-arity overload family         |
|   [3]   | consumer owns logic, vocabulary owns coverage | state-threaded `Switch`                           | distributed `_`-armed `switch`    |
|   [4]   | vocabulary item is the behavior               | `[UseDelegateFromConstructor]` row                | repeated full-coverage `Switch`   |
|   [5]   | key is a value, result is static data         | `Lazy<FrozenDictionary>` index                    | dictionary restating rows         |
|   [6]   | receiver is foreign, behavior is local        | extension block                                   | wrapper that renames receiver     |
|   [7]   | input shape, not nominal type, discriminates  | structural pattern                                | `is`-chain over open input        |
|   [8]   | one body serving every carrier                | `K<F,A>` trait-constrained arrow                  | `RunFin`/`RunEff` sibling family  |
|   [9]   | admit every present and future owner          | `IObjectFactory<TOwner,TValue,TError>` constraint | per-owner converter copy          |
|  [10]   | optional context with identity                | one `Option<ContextRecord>`                       | `T? a, T? b, bool x` flag tail    |

## [2]-[ENTRYPOINT_LAW]

[REQUEST_COLLAPSE]:
- Law: one concern exposes one entrypoint; a verb family is a `[Union]` with one case per verb under one total dispatch.
- Law: each sibling's preamble becomes its case constructor and the shared validation becomes the dispatch prologue.
- Law: full-coverage generated `Switch` is the totality proof; a new case breaks every dispatch site at compile time, never a runtime-silent `_` arm.
- Use: `[Union<T1,...>]` ad-hoc when the request is a positional sum of already-defined payloads extracted by `IsTi`/`AsTi`; regular `[Union]` named cases when cases carry distinct fields and `value is CaseName` extracts.
- Reject: a request-union-shaped success-or-failure carrier; rails own outcome transport.
- Boundary: programs that must be inspected or re-interpreted reify verbs as a closed instruction functor under one interpreter arrow; the request union with total dispatch is the form when they only run.

[SEALED_ADMISSION]:
- Law: a regular `[Union]` seals through reachability, not knobs — private owner and case constructors are the closure, one hand-written `public static` factory the sole ingress, and the generated `Switch` the sole egress; the `Conversion*`/`ConstructorAccessModifier`/`FactoryMethodGeneration` knobs are ad-hoc-only and do not exist here.
- Law: `SwitchMapMethodsGeneration.None` deletes the generated dispatch so an extension-block dispatch algebra cannot be bypassed by a second, less-restrictive surface; `SwitchMethods` and `MapMethods` suppress independently.
- Use: a `private partial record` nested in the owning service when construction must scope to one owner; an internal factory opens the whole assembly, not the enclosing type.
- Boundary: input validates once at the factory, behavior is reached once through dispatch, and no case is constructible outside the owner.

[SURFACE_ATTACHMENT]:
- Law: the module's public surface is the request union plus one dispatch entrypoint attached through an extension block, routing entirely over the public generated dispatch without touching the partial.
- Boundary: `[OverloadResolutionPriority]` steers only members co-declared in one static class; cross-class same-name extensions compare by ordinary betterness regardless of priority.

```csharp conceptual
[Union]
public abstract partial record Request {
    private Request() { }
    public sealed record Open(CodeValue Code) : Request;
    public sealed record Amend(CodeValue Code, int Delta) : Request;
    public sealed record Close(CodeValue Code) : Request;
}

public static class RequestSurface {
    public static Fin<Request> Admit(ReadOnlySpan<char> verb, CodeValue code, int delta) =>
        Verb.Validate(verb, null, out var v) is { } error
            ? FinFail<Request>(Error.New(error.ToString() ?? "unknown verb"))
            : FinSucc(v.Switch(
                open:  static s => (Request)new Request.Open(s.Code),
                amend: static s => new Request.Amend(s.Code, s.Delta),
                close: static s => new Request.Close(s.Code),
                state: (Code: code, Delta: delta)));

    extension(Request request) {
        public Fin<Receipt> Dispatch(Ledger ledger) => request.Switch(
            open:  static (l, o) => l.Open(o.Code),
            amend: static (l, a) => l.Amend(a.Code, a.Delta),
            close: static (l, c) => l.Close(c.Code),
            state: ledger);
    }
}
```

## [3]-[MODAL_ARITY]

[ARITY_ABSORPTION]:
- Law: singular, multi-item, and empty call sites collapse into one `params ReadOnlySpan<T>` signature; the compiler promotes constant primitive elements to the data segment and stack-allocates dynamic literals, so the allocation-free property is the target type's, not the call's.
- Law: a `params ReadOnlySpan<T>` parameter is implicitly `scoped`; the buffer cannot escape without `[UnscopedRef]`, and an override must restate `params` to preserve the bound.
- Use: a collection-expression builder on the carrier so `[a, b, c]` routes through one type-level span-fed builder, making arity a property of the literal.
- Reject: two collection-shaped entrypoints whose element types differ by a reference conversion — a covariant span beside a covariant array is a hard ambiguity; one span at the most-derived element type is the only ambiguity-free shape.
- Boundary: a zero-argument-capable entrypoint owns exactly one `params` element type; a second element-typed `params` overload forecloses the empty call.

[MODALITY_FOLD]:
- Law: singular, plural-preserving, and plural-reducing are call shapes over one arm — singular is `Map`/`Bind`, plural-preserving is `Traverse`/`Sequence` with container shape intact, plural-reducing is monoid-keyed `Fold`/`FoldMap` where the `Monoid` is the policy value selecting the reduction.
- Law: `TraverseM` threads the carrier's short-circuit reusing the singular arm verbatim; applicative `Traverse` over a `Validation`-shaped carrier is the accumulating sibling — same arm, the carrier alone switching the policy.
- Use: `foldWhileM`/`foldUntilM` over `(State, Value)` for the bounded-batch entrypoint with no count parameter; `oneOf` over an `Alternative` carrier for first-success, total over an empty spread because `Empty` is supplied.
- Reject: a batch flag, a mode flag, or a count beside a span whose `Length` already answers it; counts and modes derive from `span.Length` or the dispatched case, never the signature.
- Boundary: after traversal the container is uniformly in the carrier, never mixed admitted-and-raw, which keeps the next entrypoint's discriminant recoverable from shape alone.

```csharp conceptual
public static class BatchSurface {
    public static K<F, Iterable<Receipt>> Run<F>(Ledger ledger, params ReadOnlySpan<Request> requests)
        where F : Monad<F> =>
        Iterable<Request>.FromSpan(requests)
            .TraverseM(request => request.Lift<F>(ledger));

    public static Validation<Error, Tally> Reduce(Ledger ledger, params ReadOnlySpan<Request> requests) =>
        Iterable<Request>.FromSpan(requests)
            .Traverse(request => request.Lift<Validation<Error>>(ledger).Map(static r => r.Tally))
            .As()
            .Map(static tallies => tallies.Fold(static (sum, next) => sum + next, Tally.Empty));

    extension(Request request) {
        public K<F, Receipt> Lift<F>(Ledger ledger) where F : Applicative<F> =>
            request.Switch(
                open:  static (l, o) => l.Open<F>(o.Code),
                amend: static (l, a) => l.Amend<F>(a.Code, a.Delta),
                close: static (l, c) => l.Close<F>(c.Code),
                state: ledger);
    }
}
```

## [4]-[PARAMETER_ALGEBRA]

[POLICY_VALUES]:
- Law: configuration enters as one value carrying its own behavior — a smart-enum row with constructor delegates, a union case, or a frozen policy row — never a flag set the body re-derives; the collapsed form passes the pre-constructed case and no `if`/`switch` reconstructs the body at dispatch.
- Law: a stateless union case (`T1IsStateless = true`) is the shape for configuration-free policy entries; wrapping it in `Option<T>` stacks a second optionality on a discriminant that already models absence.
- Use: a frozen policy table whose projection comparer keyed on the request's discriminant declares which dimension governs selection, collapsing payload differences onto one row.
- Reject: a boolean parameter selecting between two bodies; a behavioral near-twin chosen by flag rather than by the value that encodes the boundary behavior.

[OPTIONAL_CONTEXT]:
- Law: `Option<T>` is the single optional-parameter form; declaration is `Option<T> x = default`, consumption is `IfNone(canonical)` where the canonical fallback is the policy owner's row, so the default derives once.
- Law: a nullable flag tail (`T? a = null, T? b = null, bool x = false`) fragments one context into parallel parameters; the collapse is one `Option<ContextRecord>` carrying the override bundle, with `T? ctx = null` the boundary-only spelling projected to `Option<T>` at admission.
- Law: skipping an expensive alternative is overload selection by the argument's static type — a `Memo` right branch binds the lazy `Choose`, a materialized value binds the eager one — never a flag.
- Boundary: a capability orthogonal to the discriminant — a cancellation token, an environment record — describes how work runs, not which case it is, and rides the carrier, never the signature.

[SUPPLIED_VERSUS_OMITTED]:
- Law: `Argument<T>` is the `readonly ref struct` carrying `Value` + `IsSet` that distinguishes "supplied a value equal to the default" from "omitted" — the distinction `Option<T>` cannot make; confined to one synchronous frame, never across `await`.
- Use: `Argument<TResult>` only on `MapPartially`'s per-case slots and on bespoke factory overloads; the generated `ValidateFactoryArguments` takes `ref T` and `Validate` takes raw `TValue?`, so `Argument<T>` in either generated seam is a compile-time mismatch.
- Boundary: a growing optional tail loses resolution to a tighter sibling whose every parameter received an argument, silently redirecting call sites; collapsing the tail to one `Option<ContextRecord>` deletes the parallel candidate.

[KNOB_TEST]:
- Law: discrimination is recoverable from the value — type, case, pattern, or arity — never from a parallel parameter; the test is whether removing the parameter loses information the value cannot reconstruct.
- Reject: a parameter that re-describes the input — a batch, many, or mode flag beside the value — as arity smuggled back into a knob.
- Reject: a timeout or deadline as an entrypoint parameter; the bound is an effect-layer aspect injected after dispatch, and the signature never grows a token tail for it.

## [5]-[DISPATCH_FORMS]

[FORM_SELECTION]:
- Law: five forms are selected by where ownership lives — state-threaded `Switch` when the consumer owns the logic but a closed vocabulary owns exhaustiveness; `[UseDelegateFromConstructor]` rows when the vocabulary item is the behavior; frozen tables when the key is a value and the result is static; extension blocks when the receiver is foreign but the behavior is local; structural patterns when the input's shape, not its nominal type, discriminates.
- Law: three or more consumers writing identical full-coverage `Switch` arms over one vocabulary is the delegate-row collapse signal — completeness becomes a constructor invariant instead of distributed call-site discipline.
- Reject: a frozen table restating a vocabulary's own delegate rows — a duplicate-entry burden with a silent missed-new-item failure; structural dispatch over a closed family, trading compile-time totality for a silent `_`.
- Boundary: mixing forms at one site signals an unresolved ownership boundary, except the two valid compositions — a `Switch` arm probing a frozen table is dispatch plus data retrieval; an extension block owns module-to-domain translation while its inner `Switch` owns per-case projection.

[TRAIT_DERIVATION]:
- Law: a carrier trait declares the smallest closed abstract core and derives the operation family as static-virtual defaults — two predicate-gated folds derive roughly fifty receiver operations, lift-plus-apply derives the effectful-sequencing surface, empty-plus-choose derives the repetition suite.
- Law: satisfying a trait costs only its abstract core; the derivation is the executable specification an override is verified against, and a carrier with no faster primitive correctly ships zero overrides.
- Use: an override only where a structurally cheaper primitive exists — an O(1) size field overrides count and inherits the other forty-nine — never altering the law the default encodes, only its cost.
- Boundary: the deferred-second-argument overloads on `choose` and `apply` are mandatory infrastructure; a carrier omitting the lazy overload silently loses the entire derived repetition surface.

[KEY_ADMISSION]:
- Law: one generated keyed declaration is three dispatch forms at once — the key-to-case lookup table, the all-items sweep, and the boundary-admission factory are static-abstract views of one type, so "table or vocabulary or admission" dissolves and a call site reaches for whichever member its problem names.
- Law: the generated `Validate(TKey?, IFormatProvider?, out TItem)` is the one-hop bridge from raw external key to dispatch-ready item — open on the way in, a typed error or a vocabulary item on the way out, the subsequent `Switch` compile-time-total.
- Use: the span-keyed `Validate(ReadOnlySpan<char>, ...)` overload so protocol text admits and dispatches without materializing a string.
- Boundary: the comparer accessor is the single arbiter of key equality everywhere, so every frozen table keyed on the owner resolves the same comparer and no call site supplies a divergent one.

## [6]-[CARRIER_POLYMORPHIC_DISPATCH]

[ONE_CARRIER_SURFACE]:
- Law: the form selects which arm runs, the carrier the arms return selects how results combine — orthogonal axes; an arm returning `K<F,A>` makes one entrypoint the `Fin`, `Eff`, `IO`, and `Validation` dispatcher, and the per-carrier sibling family is the rejected form.
- Law: the trait stack is the precise capability contract — `Monad` binds, `Fallible<E,F>` fails and catches, `Choice` falls back, `Alternative` adds `Empty`, `Traversable` sweeps — and over-constraining beyond what the body uses is dead surface narrowing the admitted carrier set for nothing.
- Use: `.As()` to recover the concrete carrier exactly once at the materialization boundary; capabilities ride the carrier through `Has`/`Stateful` accessors, never a trailing dependency tail.
- Boundary: pinning `Fallible<E,M>` to a domain error union makes the failure surface a closed family, and the named catch family generated once against the trait applies to every fallible carrier via `|` with no per-monad wiring.

[INDEPENDENT_JOIN]:
- Law: independent arms combine applicatively, dependent steps bind monadically, and the choice is load-bearing — a `Bind` chain over independent arms silently discards all failures after the first.
- Law: the tuple join `(K<F,A>, ..., K<F,J>).Apply(func)` runs each slot under the carrier's `Apply` — an accumulating carrier reports the union of failures, an effect carrier short-circuits; same call site, same join, the carrier alone switching semantics.
- Reject: a branch inside the join — its only legitimate content is total construction over already-dispatched values; a branch is a fourth dispatch smuggled into combination and lifts into its own arm.
- Boundary: all slots share one `F` by construction, so failure semantics decide once; an expensive slot enters as `Memo` and a short-circuiting carrier leaves it unforced.

[ITERATIVE_DISPATCH]:
- Law: looping dependent dispatch is `Recur(seed, step)` — the step returns continue-or-done as a closed two-case family, the live case picks the successor, the stack never grows.
- Law: because the body is pure description until the carrier is interpreted, the same arms run under a deterministic test carrier and the production effect carrier — a totality proof under the cheap carrier proves the production entrypoint.
- Boundary: carrier changes are one structure-preserving arrow, never a match-and-rebuild bridge; mid-pipeline concretization defeats the polymorphism.

```csharp conceptual
public static class Assembly {
    public static K<F, Order> Assemble<F>(Region region, Tier tier, Channel channel)
        where F : Applicative<F> =>
        (region.Switch(state: unit,
            domestic: static (_, d) => F.Pure(d.Zone),
            foreign:  static (_, f) => F.Pure(f.Zone)),
         tier.Switch(state: unit,
            gold:     static (_, _) => F.Pure(Priority.High),
            standard: static (_, _) => F.Pure(Priority.Normal)),
         channel.Switch(state: unit,
            web:      static (_, w) => F.Pure(w.Route),
            partner:  static (_, p) => F.Pure(p.Route)))
        .Apply(static (zone, priority, route) => new Order(zone, priority, route));
}
```

## [7]-[TYPE_LEVEL_DISPATCH]

[OPEN_OWNER_BOUNDARY]:
- Law: the self-constrained factory contract — `where TOwner : IObjectFactory<TOwner, TValue, TError>`, sole member a static abstract `Validate` — is the inversion of case dispatch: `Switch` is closed over one owner's cases, the constrained generic is open over the unbounded family of all owners, resolved by the JIT with no instance, no vtable, no reflection.
- Law: the error vocabulary stays owner-chosen because `TError` carries its own static abstract `Create`, so a generic bridge synthesizes context-bearing errors without naming any concrete class.
- Use: a span-keyed instantiation — the span is the `TValue` under `allows ref struct` — so protocol text decodes into the closed vocabulary with zero heap traffic on both legs.
- Reject: one shallow converter per owner; one deep parameterized surface owns conversion for the whole owner set.

[REACH_LIMIT]:
- Law: an unresolved static abstract member is not a first-class value — no open delegate, no method group, no expression tree — so type-level static dispatch serves boundaries whose owner is resolved at compile time at the call site.
- Use: the generated static-abstract metadata surface for runtime-discovered owners, folded by a generic codec over the metadata's own generated dispatch, keeping the runtime-discovered case reflection-free.
- Boundary: choosing the static-virtual form for runtime discovery reintroduces reflection through closed-generic invocation — the exact cost the contract exists to delete.

```csharp conceptual
public static class Boundary {
    public static Fin<TOwner> Admit<TOwner, TValue, TError>(TValue value, IFormatProvider? culture = null)
        where TOwner : IObjectFactory<TOwner, TValue, TError>
        where TValue : notnull, allows ref struct
        where TError : class, IValidationError<TError> =>
        TOwner.Validate(value, culture, out var item) is { } error
            ? FinFail<TOwner>(Error.New(error.ToString() ?? "rejected"))
            : FinSucc(item!);

    public static Fin<TOwner> AdmitText<TOwner, TError>(ReadOnlySpan<char> text)
        where TOwner : IObjectFactory<TOwner, ReadOnlySpan<char>, TError>
        where TError : class, IValidationError<TError> =>
        Admit<TOwner, ReadOnlySpan<char>, TError>(text);
}
```

## [8]-[ASPECTS]

[WEAVE_TAXONOMY]:
- Law: a definition-time aspect — admission, identity, dispatch, serialization, grammar — is a property of the type, declared by attribute and present at every call site; a composition-time aspect — retry, recovery, resource lifetime, timeout, marshaling — is a property of a call site, attached as operators in the pipeline.
- Law: the classification test is per-site variance — a concern present at every use weaves at definition, a concern that varies per site composes at the site.
- Boundary: the two layers meet at exactly one seam — the admission boundary where a generated factory's typed error lifts into the carrier; policy pushed across it in either direction gains hidden control flow and stops being recoverable from its declaration.

[CONSTRUCTION_ADVICE]:
- Law: the generated factory threads advice in a fixed generator-owned order — absence guards, error-slot init, the `ValidateFactoryArguments` normalizer, construction gated on a null error, the constructor hook, post-init — and the author opts into slots, never reorders.
- Law: a non-void `ValidateFactoryArguments` return threads into post-init as a typed side channel — computation done once during validation handed forward with no field, no closure, no second pass.
- Reject: rejection logic in the constructor hook or null/blank guards in the factory hook — the weave order fixes absence below both, so either placement is dead code, and a throwing hook breaks the rail bridge above the seam.
- Boundary: expensive admission belongs in the factory hook, invariant-of-record repair in the constructor hook, which fires on every path including rehydration.

[STACKING_ORDER]:
- Law: composition-time aspects stack in author-written operator order, and the same two aspects in two orders are two policies — retry around a bracket re-runs acquire-use-release per attempt, a bracket around a retry acquires once and retries only the use.
- Law: a named catch composes via `|` as handler wrapping, left-associative — inner handlers see errors first, so a broad predicate before a narrow one shadows it with no diagnostic; the finalizer is a transparent pass-through that runs on both exits, so a catch after it still fires.
- Law: generated dispatch collapses to a value before any transformer runs, so per-case policy is unspellable by wrapping the dispatch — each arm must produce its own policy-wrapped carrier, the granularity fixed by where the carrier is introduced.
- Reject: a constrained operator on a carrier defined anywhere but an extension block — `extension<E, F, A>(K<F, A> _) where F : Fallible<E, F>` is the only mechanism for a local aspect's operator surface.

```csharp conceptual
public static class Aspects {
    public static IO<Receipt> AcquireThenRetryUse(IO<Resource> acquire, Func<Resource, IO<Receipt>> use) =>
        acquire.Bracket(
            Use: resource => use(resource).Retry(Backoff),
            Catch: static error => IO.fail<Receipt>(error),
            Fin: static resource => resource.ReleaseIO());

    public static IO<Receipt> RetryThenAcquireUse(IO<Resource> acquire, Func<Resource, IO<Receipt>> use) =>
        acquire.Bracket(
            Use: use,
            Catch: static error => IO.fail<Receipt>(error),
            Fin: static resource => resource.ReleaseIO()).Retry(Backoff);
}
```
