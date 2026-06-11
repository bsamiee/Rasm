# Carrier-Polymorphic Dispatch Fold

[CARRIER_AS_RETURN_NOT_PARAMETER]:
- The generated case eliminator threads its state argument by reference, so making the eliminator return a carrier `K<M, A>` rather than a concrete rail collapses every rail variant of one entrypoint into a single declaration. A dispatch method written `static K<M, Result> Run<M>(RequestUnion req) where M : Monad<M>, Fallible<DomainError, M>` is simultaneously the `Fin`, `Eff`, `IO`, `Validation`, and `StateT` entrypoint — the caller picks the carrier at the call site by binding `M`, and no second dispatch body is authored. The anti-pattern this deletes is the sibling family `RunFin`, `RunEff`, `RunValidation`: three bodies enumerating the same case arms, differing only in the concrete rail they close over.
- The carrier's failure channel is `Fallible<E, M>`, generic in the error type `E`. Pinning `E` to a domain error `[Union]` rather than the framework error type makes the entrypoint's failure surface itself a closed family: `M.Fail<Result>(DomainError.NotFound(key))` is the only raise spelling, and `Fallible.error<M, A>(...)` (the `E = Error` specialization) is the rejected wider form whenever the concern owns a typed error vocabulary. The discriminant on a failed dispatch is recoverable from the typed `E`, not from a string code.
- Dependencies do not enter as parameters when the carrier carries them. `Has<M, DepA>.Ask` yields `K<M, DepA>` and `Readable<M, Env>.Asks(env => env.DepA)` projects one field; the entrypoint binds them inside its own computation, so a `Run<M>(RequestUnion req)` signature carries exactly the request and nothing else. A trailing `IDepA dep, IDepB dep2` parameter tail beside the request is the dependency-smuggling form — the dispatch constraint `where M : Has<M, DepA>, Has<M, DepB>` moves it onto the carrier where the runtime injects it once.

[STATE_THREADED_SWITCH_OVER_A_CARRIER]:
- `Stateful<M, S>` makes the dispatch context part of the carrier rather than a threaded value argument. `Stateful.state<M, S, A>(s => (value, newState))` runs an atomic read-modify-write over the carrier's inner state in one combinator; `Stateful.local<M, S, A>(setter, operation)` runs `operation` against a mutated state and restores the prior state on exit — the dispatch arm that needs a scoped context override gets it without a state parameter on the entrypoint and without a manual save/restore pair. The save/restore is the combinator's body, not the arm's.
- Generated `Switch<TState>(state, arm1, arm2, ...)` does not allocate a closure because the per-arm delegates receive `state` as an explicit parameter; pairing this with a carrier-typed `TState` keeps the whole dispatch allocation-free even when the arms produce effects, because the effect is the return value, not captured state. The arms stay `static` lambdas — capturing nothing — and the carrier is reconstructed, never mutated through a captured reference.

```csharp
extension(RequestUnion req)
{
    public K<M, Result> Run<M>()
        where M : Monad<M>, Stateful<M, Session>, Fallible<DomainError, M> =>
        req.Switch(
            state: default(Unit),
            create: static (_, c) =>
                from sess in Stateful.get<M, Session>()
                from r in M.Pure(c.Apply(sess))
                from _ in Stateful.modify<M, Session>(s => s.Touch())
                select r,
            remove: static (_, d) =>
                Stateful.gets<M, Session, bool>(s => s.Owns(d.Key))
                    .Bind(owned => owned
                        ? M.Pure(Result.Removed(d.Key))
                        : M.Fail<Result>(DomainError.NotOwned(d.Key))));
}
```

[REQUEST_STREAM_COLLAPSES_INTO_ONE_TRAVERSAL]:
- Plural and batch modalities do not need a second entrypoint when the singular entrypoint returns a carrier. `Traversable.traverseM(req => req.Run<M>(), requests)` lifts a `K<T, RequestUnion>` of requests into `K<M, K<T, Result>>` — one monadic traversal threads the carrier's short-circuit (first failure aborts) through the whole batch, and the singular arm is reused verbatim. `Traversable.traverse` (applicative, not monadic) is the accumulating sibling: over a `Validation`-shaped carrier it gathers every failure instead of stopping at the first, so the singular/plural/accumulating triad is three call sites over one arm, never three bodies.
- `Applicative.Action(ma, mb)` sequences two carrier computations and discards the first result; `Applicative.Actions(iterable)` folds a non-empty sequence the same way. A command-stream entrypoint whose results are write-only (fire each effect, keep none) is `Applicative.actions(requests.Map(r => r.Run<M>()))` — the discard is the combinator's semantics, not a `_ =` at every site. `Applicative.Replicate(count, fa)` is the bounded-repeat form, and `Applicative.Between(open, close, body)` brackets a dispatch with paired enter/exit effects, both without a loop construct.
- `Foldable.foldM(reduce, seed, requests)` reduces a request stream into one accumulated carrier value: `Func<Acc, RequestUnion, K<M, Acc>>` is the per-step reducer and the carrier threads the running accumulation, so a stream that must produce one summary value rather than one result per request is a single `foldM`, not an external accumulator mutated in a loop. `Foldable.foldWhileM` / `foldUntilM` add an early-stop predicate over `(State, Value)` so a saturating reduction halts without exhausting the stream — the bounded-batch entrypoint with no count parameter.

[CHOICE_AS_FALLBACK_DISPATCH]:
- `Choice.choose(fa, fb)` is fallback dispatch on the carrier: run the first computation, and on its failure run the second. A try-primary-then-secondary entrypoint is `primary.Run<M>() | secondary.Run<M>()` over an `Alternative`-carrier — the `|` is `Choose`, not a boolean branch, and the cases stay independent computations rather than nested conditionals. `Choice.some(fa)` and `Choice.many(fa)` re-run one computation collecting results until failure (one-or-more / zero-or-more), giving a retry-collecting entrypoint whose stop condition is the carrier's own failure rather than a counter — though the carrier's `Apply` must override the lazy form or the recursion stack-overflows.
- `Fallible<E, M>.Catch(fa, predicate, handler)` is the carrier-level recovery primitive — `Func<E, bool>` selects which failures the `Func<E, K<M, A>>` handler intercepts, and unmatched failures propagate. With `E` a domain error `[Union]`, the predicate pattern-matches the error case, so per-case recovery is `Catch(comp, e => e is DomainError.Transient, e => retryOnce)` — recovery policy reads the typed error directly, and the recovery handler returns a carrier so it can itself dispatch, retry, or escalate.

[CARRIER_INTERPRETED_ONCE_AT_THE_EDGE]:
- The carrier-polymorphic dispatch body is pure description; nothing runs until the concrete carrier is interpreted. `MonadUnliftIO<M>.toIO(ma)` reifies a carrier computation as `K<M, IO<A>>`, and `MonadUnliftIO.mapIO(io => io.Timeout(span), ma)` injects a composition-time aspect — timeout, fork, bracket — into the carrier without a parameter on the entrypoint and without the dispatch body knowing the policy exists. The timeout is an `IO`-level transform applied after dispatch, so the entrypoint signature never grows a deadline or a token tail.
- Because the entrypoint returns `K<M, A>` for a constrained-but-unbound `M`, the same generated `Switch` arms are reachable under a pure test carrier (deterministic state, no `IO`) and under the production effect carrier — the dispatch is verified against the cheap carrier and run against the expensive one with byte-identical arms. The carrier is the only thing that changes between the unit-proof call site and the runtime call site; the dispatch surface is shared, so a test that exercises every case arm under the test carrier is simultaneously a proof of the production entrypoint's totality.
- `Applicative.Apply` over a `Memo<F, A>` operand short-circuits the second computation when the first already fails, so an independent-fan-out entrypoint built with applicative `apply` (rather than monadic `Bind`) both parallelizes the independent arms and skips the unreachable ones — the applicative shape is the load-bearing choice when dispatch arms do not depend on each other's results, and `Bind` is the rejected form there because it forces a false sequential dependency the carrier would otherwise exploit.
