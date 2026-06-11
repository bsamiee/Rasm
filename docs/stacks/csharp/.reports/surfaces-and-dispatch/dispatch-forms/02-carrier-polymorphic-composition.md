# Carrier-Polymorphic Composition of Generated Dispatch Arms

[ARM_RETURN_CARRIER_GOVERNS_THE_COMBINATION_ALGEBRA]:
- A generated `Switch` arm is unconstrained in its return type, so an arm body may return `K<F, A>` — a value lifted into a carrier whose traits, not the dispatch form, decide how independent arms combine. The dispatch form selects which arm runs; the carrier `F` selects the algebra that fuses the results of several arms or several dispatches. These are orthogonal axes: collapsing them — choosing the carrier to suit the call site rather than the failure semantics — is the error that produces a `Switch` arm probing a second mechanism to finish its work.
- One carrier exposes a complete algebra through trait witnesses: `Functor` gives `Map`, `Applicative` gives `Pure`/`Apply`/`lift`, `Monad` gives `Bind`/`Recur`, `Choice` gives `Choose`, `SemigroupK`/`MonoidK` give `Combine`/`Empty`, `Fallible<E,F>` gives `Fail`/`Catch`, `Traversable` gives `Traverse`/`TraverseM`. Selecting a carrier admits or forbids each combinator at compile time; an arm that needs accumulation must return a carrier whose `Apply` accumulates, and that is a definition-time choice of `F`, not a runtime branch.
- The discriminant for combination is dependency between arms: when later arms consume earlier results, the sequence is monadic (`Bind`) and short-circuits; when arms are independent, the combination is applicative (`Apply`/`lift`) and — for the accumulating carriers — reports every failure. Independence is the structural fact that licenses error accumulation; a `Bind` chain over independent arms silently discards all failures after the first and is the form to collapse into an applicative combine.

[N_ARY_INDEPENDENT_DISPATCH_THROUGH_TUPLE_APPLY]:
- A tuple of carriers carries a direct applicative join: `(K<F,A>, K<F,B>, ..., K<F,J>).Apply(func)` exists for arities two through ten, taking an uncurried `Func<A, B, ..., R>` and returning `K<F, R>`. Each tuple slot is the result of one dispatch — one generated `Switch`, one frozen-table probe lifted with `Pure`, one delegate-row invocation lifted into `F` — and the join runs all slots under the carrier's `Apply`, threading no shared state and capturing nothing. This is the canonical surface for combining several parallel dispatches whose results are independent; the curried `Applicative.lift<F, A, B, C, D>(f, fa, fb, fc)` family is the same join when slot count is fixed and the function is more naturally written curried.
- The combination's failure behavior is a property of `F` alone and identical across every arity of the tuple `Apply`. With `F = Validation<E>` (where `E : Monoid<E>`) the join evaluates every slot and the `Apply` threads the failure monoid, so a three-slot dispatch reports the union of all three failures in one pass. With `F = Eff<RT>` or `F = IO` the join short-circuits on the first failing slot. The same call site — same tuple, same `func` — switches between accumulate-all and abort-first by changing only the carrier the arms return; no branching, no flag, no second code path.

```csharp
// Three independent dispatches over three closed vocabularies, combined once.
// F = Validation<Seq<Error>> accumulates every arm's failure; F = Eff aborts on first.
// Each Switch is closure-free and returns K<F, _>; the tuple Apply threads F's algebra.
static K<F, Order> Assemble<F>(Region r, Tier t, Channel c)
    where F : Applicative<F> =>
    (r.Switch(state: unit,
        domestic: static (_, d) => F.Pure(d.Zone),
        foreign:  static (_, f) => F.Pure(f.Zone)),
     t.Switch(state: unit,
        gold:     static (_, _) => F.Pure(Priority.High),
        standard: static (_, _) => F.Pure(Priority.Normal)),
     c.Switch(state: unit,
        web:      static (_, w) => F.Pure(w.Route),
        partner:  static (_, p) => F.Pure(p.Route)))
    .Apply(static (zone, prio, route) => new Order(zone, prio, route));
```

[ACCUMULATE_VERSUS_SHORT_CIRCUIT_IS_ONE_CARRIER_TWO_TRAITS]:
- An accumulating carrier implements both `Applicative` and `Monad`, and the two paths diverge by construction: its `Apply` carries a `Semigroup` instance over the failure type and appends failures from both operands, while its `Bind` has only one value to inspect on the failure path and therefore cannot append. The accumulating-versus-aborting choice is thus not two carriers but two trait-routes through one carrier — `Apply`/`lift`/tuple-`Apply` accumulate, `Bind`/`SelectMany`/query-comprehension short-circuit. A dispatch that must report every independent validation failure uses the applicative route over the same type whose monadic route it uses for the dependent steps that follow.
- The accumulating carrier constrains its failure type to a monoid, which is the type-level statement that failures are combinable: the failure channel must itself be a `Monoid` so two failures have a defined join. This pushes the accumulation policy into the failure type's algebra rather than into call-site folding — the right failure shape is a monoidal collection of errors, and the carrier supplies the append for free at every applicative join.
- The abort-first carriers expose the same `Traversable` split: `Traverse` runs under `Applicative` and is the accumulating sweep over a collection of dispatches, `TraverseM` runs under `Monad` and is the short-circuiting sweep. Choosing `Traverse` with an accumulating carrier reports every element's failure; choosing `TraverseM` with an effectful carrier aborts on the first. The collection-level form and the tuple-level form are the same accumulate/short-circuit decision at different arities, and both are governed by the trait the carrier's method resolves to, never by an imperative loop.

[LAZY_SLOT_EVALUATION_VIA_MEMO]:
- Every applicative join overload has a parallel form taking `Memo<F, A>` in place of `K<F, A>`, and `Memo` is the lazy-and-cached lift: its single value is computed on first force and retained, with `Reset()` to invalidate and `Clone()` to fork an unforced copy. A slot whose dispatch is expensive enters the join as a `Memo` so it is forced only if the carrier's `Apply` actually demands it — under a short-circuiting carrier an earlier failure can leave a later `Memo` slot unforced, turning the eager n-ary join into a demand-driven one without changing the combinator.
- `Choose` carries the same eager/lazy pair: `Choose(fa, fb)` forces both, `Choose(fa, Memo<F, B> fb)` forces the alternative only when the first structure does not succeed. The lazy alternative is the correct form for fallback dispatch whose alternative branch is costly — a primary `Switch` arm with a `Memo`-wrapped recovery dispatch evaluates the recovery only on the primary's failure, which the bare two-`K` form cannot express because it forces both operands.

[ALTERNATIVE_DISPATCH_AS_OPERATOR_ALGEBRA]:
- Three operators on `K<F, A>` give three distinct combination semantics over carriers, and conflating them is the failure to resolve what the composition means. `|` is `Choose` — unconditional first-success-wins fallback: `primaryDispatch | fallbackDispatch` runs the second only if the first does not succeed, with no error inspection; a `Pure<A>` right operand is the constant-recovery terminus. `+` is `Combine` from `SemigroupK` — monoidal merge of two structures of the same shape, which for collection-like carriers concatenates and for choice-like carriers is the alternative; this is structurally distinct from accumulating-`Apply`, which merges failure channels while `+` merges success structures. The predicate-guarded recovery operator from `Fallible<E,F>` is the third — it inspects the error and recovers only on a match, where `|`-as-`Choose` recovers on any non-success.
- `Fallible<E, F>` reduces the entire alternative-and-recovery surface to two static-abstract members: `Fail<A>(E)` lifts an error into the carrier and `Catch<A>(fa, predicate, handler)` recovers selectively. Every named catch combinator and every re-fail form is a specialization of this pair over a chosen carrier; the carrier-generic `|`-with-catch composes left-associatively so inner handlers see errors before outer ones. Because `Fallible` is a trait, the recovery algebra is available on any carrier that witnesses it, and a dispatch arm's recovery is written once against `K<F, A>` rather than per concrete effect type.

```csharp
// Three operators, three semantics, all over one carrier-polymorphic dispatch.
// |  Choose  : first-success fallback, no error inspection; Pure terminus is constant recovery.
// +  Combine : SemigroupK merge of success structures (collection concat / choice union).
// The Memo alternative forces the costly recovery dispatch only on primary non-success.
static K<F, Route> Resolve<F>(Request req, Memo<F, Route> recovery)
    where F : Choice<F>, MonoidK<F>, Applicative<F> =>
    req.Switch(state: unit,
        primary:  static (_, p) => F.Pure(p.Route),
        degraded: static (_, _) => F.Empty<Route>())  // MonoidK identity: contributes nothing to +, fails over via |
    .Choose(recovery);
```

[STACK_SAFE_SEQUENTIAL_DISPATCH_VIA_RECUR]:
- Dependent dispatch that loops — each step's result selecting the next dispatch — is not a `Bind` chain of unbounded depth but `Recur<A, B>(seed, step)`, the monad's tail-recursive bind: `step` returns `K<M, Next<A, B>>` where `Next.Loop(a)` continues with a new seed and `Next.Done(b)` terminates. This is the stack-safe driver for iterative dispatch over a closed vocabulary where the live case picks the successor; the carrier's `Actions` (sequencing an entire stream of dispatches) is itself implemented on `Recur`, so unbounded sequential dispatch never grows the stack regardless of length.
- The loop primitive carries no imperative residue: `Next` is a closed two-case family, `Loop` and `Done` are its lifts, and the step function is a pure `A -> K<M, Next<A, B>>`. A dispatch that consumes a stream until a terminal case appears expresses the termination as a `Done` arm and the continuation as a `Loop` arm of one generated `Switch`, and `Recur` drives it — the state-threaded dispatch and the stack-safe loop are one expression with no `while`, no accumulator mutation, and no explicit recursion depth.

[INTEGRATION_PRESSURE_THE_JOIN_OWNS_NO_LOGIC]:
- The applicative join function is pure construction over already-dispatched values: `static (a, b, c) => new T(a, b, c)`. When that function contains a branch, the branch is a fourth dispatch that belongs in a fourth slot, not in the join — a join body that inspects its arguments to decide structure is dispatch smuggled into combination and is the signal to lift the decision into its own carrier-returning `Switch`. The join's only legitimate content is total construction; everything conditional is another arm.
- Mixing carriers across slots of one join is a compile error by construction — every slot is `K<F, _>` for one shared `F` — which forces the failure semantics of a multi-dispatch to be uniform and decided once. A dispatch whose arms genuinely need different effect capabilities narrows per arm into a common carrier before the join (each arm lifts its narrower effect into the shared `F`), so the heterogeneity lives inside the arms and the combination stays single-carrier; the join never sees more than one algebra.
