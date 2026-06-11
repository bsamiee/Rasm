# Transformer Stack Capability Order

[ONE_INNER_BIND_DELEGATION]:
- Every transformer's bind is written exactly once, purely in terms of the immediately-inner monad's bind — no layer re-implements the layers below it. The reader form runs the inner bind under a constant environment; the state form threads the named-tuple state through the continuation's run; the short-circuit forms match the inner case and either continue or collapse. A deep stack composes by one-level delegation — the spine is a chain of single hops, never a flattened reimplementation — so adding a transformer is additive at zero semantic cost to the layers below, and the cost of a stack is exactly one bind allocation per layer per step.
- The state carrier is the only one whose inner bound value is not the bare value but a value-state product, and that asymmetry is the root cause of the capability law: a monad whose inner payload is a product cannot be projected back to a bare effect without discarding the state thread.

[CAPABILITY_BY_STACK_ORDER]:
- The entire composition-time effect surface — fork, await, timeout, bracket, repeat, retry, fold, post, local, uninterruptible — is declared once as default members on the unlift trait, each a one-line map over the projected inner effect, and the whole surface hinges on one abstract projection from the carrier to its bare effect. A transformer earns the entire algebra by supplying that projection; a transformer that cannot project — any product-payload layer — forfeits all of it.
- Stack order is therefore a capability decision, not a cosmetic one: a layer that needs retry, bracket, fork, or timeout must sit on the unliftable side of any product-shaped layer, and a state layer placed outside the effect boundary makes the outer effect un-bracketable. Order changes what the stack can do, not merely what it means.
- Unlift is a runtime contract, not a compile-time one: the unlift cascade resolves through an optional shadow surface, so a stack whose innermost monad never supplies the lift type-checks and then throws a wrapped exceptional at run time on first use. The optional sub-trait split is also the mechanism by which a pure carrier participates in every monadic arrow yet rejects effect lifting at the type level — capability is additive and à la carte, queried at the boundary, never all-or-nothing.
- The capability-constraint form is simultaneously the abstraction and the proof obligation: one body quantified over the conjunction of state, reader, unlift, and monad constraints replaces one algorithm per concrete stack, and a constraint set that includes unlift is unsatisfiable by any ordering that buries the effect-capable layer under a product-shaped one — the constraint that compiles asserts a legal ordering.

```csharp
static K<M, Report> Pipeline<M>(Input raw)
    where M : Stateful<M, Budget>, Readable<M, Config>, MonadUnliftIO<M>, Monad<M> =>
    from cfg   in Readable.asks<M, Config, Threshold>(static c => c.Limit)
    from spent in Stateful.get<M, Budget>()
    from _     in Stateful.modify<M, Budget>(b => b.Charge(raw.Cost))
    from step  in MonadUnliftIO.mapIO<M, Raw, Raw>(
                      io => io.Timeout(TimeSpan.FromSeconds(2)).Retry(Backoff),
                      Acquire(raw))
    from done  in Readable.local<M, Config, Report>(
                      c => c.Sandboxed(),
                      Finalize(step, cfg, spent))
    select done;
```

[LIFT_CASCADE_VS_PURE]:
- Lifting an inner action one layer is a direct type-class dispatch wrapping the inner computation in the layer's constructor — the reader ignores its environment, the state layer pairs the value with unchanged state; lifting across N layers is N nested one-level lifts. Lifting a raw effect up a stack is the effect-lift cascade, where each layer delegates to the next inner layer until the effect base overrides. The distinction is decisive: layer-lift of a pure injection places a value-shaped placeholder at the current layer with no effect semantics, while the effect lift threads a real effect through every intermediate layer to the floor — conflating them silently substitutes a value where an effect was intended.

[FRAGMENT_BIND_ABSORPTION]:
- Each transformer overloads its query-expression bind per inner-fragment kind, so one comprehension mixes layer-local effects with no explicit lift: environment asks, state gets/puts/modifies, pure literals, bare effects, and inner-monad steps each bind through the overload matching their shape. The transformer is the dispatch surface that erases lift ceremony at the query boundary, and a pipeline peppered with explicit single-layer lifts between domain steps is the rejected form — it re-describes a lift the overload set already performs.

[STACK_LOCAL_PROJECTION]:
- Environment narrowing and environment rewriting are distinct combinators: the narrowing form changes the reader's environment type through a wide-to-narrow projection function — a sub-pipeline written against a narrow environment composes into a richer parent structurally, with no marker interface — while the rewriting form preserves the type and maps the value for one scoped sub-effect.
- The two hoists are orthogonal: one transforms the inner monad's bound computation while preserving both the layer and the monad type; the other transforms the monad type itself under a fixed layer, with the target carrier constrained to supply the choice or alternative structure the hoist relies on. The type-hoist swaps the effect base under a fixed reader or state layer without unwinding the stack; the value-hoist transforms the carried effect without touching either.
