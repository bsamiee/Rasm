# Generated Dispatch as a Carrier Seam

[ARM_AS_KLEISLI_POINT]:
- Each non-abstract case is the natural Kleisli point `A -> K<F, B>`: the case payload is the `A`, the arm body is the lift into `F`, and the generated total `Switch` is the only place the family's continuation is selected — every arm produces a rail value, the dispatcher never sees a bare value, and the carrier's own combination sequences the children. Arm-set uniformity is load-bearing, not stylistic: a bare-value arm beside rail-valued arms would force a result type the carrier cannot combine, so a constant or never-failing arm writes the carrier's pure lift rather than a concrete success constructor — which simultaneously keeps the arm portable across every carrier the fold is later specialized to.
- A recursive case's arm is a traversal, not a hand-written sequencer: it collects child folds as kinded values and combines them through the applicative tuple combinator, naming the pure combine and nothing about success-versus-failure. One arm body is therefore correct under fail-fast and accumulate alike, the failure policy is inherited from the carrier and never spelled in the arm, and the only edit that flips the whole fold between policies is the trait its `where` clause demands.

```csharp
public static K<F, B> Fold<F, B>(this Node source, Func<Leaf, K<F, B>> onLeaf, Func<B, B, B> combine)
    where F : Applicative<F> =>
    source.Switch<Func<Node, K<F, B>>, K<F, B>>(
        onLeaf,
        leaf:   static (f, l)  => f(l),
        branch: static (f, br) => Applicative.apply((br.Left.Fold(f, combine), br.Right.Fold(f, combine)), combine));
```

[STATE_AND_RESULT_CHANNEL_ORTHOGONALITY]:
- The state-threaded dispatch and the carrier are orthogonal channels: the state parameter rides the environment — bindings, depth budget, configuration — while the result parameter is instantiated to the kinded carrier. The environment never leaks into the failure rail and the rail never leaks into the environment, so a reader-shaped fold and a fallible fold compose on one dispatch without nesting two monads whenever the environment is plain data; the second monad is earned only when the environment itself is effectful.
- The ref-struct result channel and the kinded heap carrier are mutually exclusive on one fold: generated dispatch admits a stack-only result type, so a span-shaped accumulator can be the dispatch result without boxing — but a kinded carrier is heap-shaped, so the two result disciplines are chosen per fold by whether the accumulation must escape the stack, never mixed in one dispatch.

[STOP_AT_AS_CARRIER_REENTRY]:
- A stop-at overload folds a case subtree behind one abstract handler, and that coarse handler is the natural seam to re-enter a different carrier: the outer fold runs under a monadic carrier for short-circuit while the stopped subtree re-dispatches on its own nested surface under an accumulating carrier. Blast-radius granularity and failure-accumulation granularity are configured at the same declared boundary — a case added inside the subtree breaks exactly the inner fold's totality, and the carrier change rides the same seam without either fold naming the other's carrier.

[LOCKSTEP_VERSION_LAW]:
- Because the case list is part of every dispatch method's signature, a carrier-polymorphic fold's arm set is re-signatured on case addition and every specialization at every carrier breaks at compile time in lockstep — the carrier abstraction widens reuse without widening the silent-failure surface, since totality is proven per dispatch site regardless of which carrier it was specialized to. Partial overloads poison this guarantee precisely where it is most valuable: a new case routes to the default arm silently in every carrier at once, so carrier-polymorphic folds stay on the total forms and reserve partial dispatch for defaults that are genuinely total semantics rather than routing.
