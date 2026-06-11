# Transformer-Threaded Receipts

[STATE_THREADS_THROUGH_THE_FUNCTION]:
- A receipt accumulated as transformer state never lives in a cell: the state transformer wraps `Func<S, K<M, (A Value, S State)>>` — state is the argument threaded into the run and the second tuple element threaded out, never a field. Each bind runs with the incoming state, pulls the advanced state out of the inner monad, and feeds it into the continuation's run; the consumer reads the final receipt as the second tuple element of the top-level run — purely functional accumulation with no atom, no spin loop, and no contention semantics.
- The threading is strictly left-to-right and builds a left-nested tower of inner-bind closures forced only at the final run, so a long fold accumulates one closure per step — the state-thread cost is closure nesting, not allocation of intermediate state cells. Map passes the state untouched beside the projected value; pure injects through the inner monad and pairs the incoming state unchanged.
- The state-effect vocabulary is a trait, not methods on the type: the read-the-whole-state form derives from the read-and-map form applied to identity — one declared correspondence yields the projection family, never a second primitive.

[WRITER_IS_A_DIFFERENCE_LIST]:
- The log transformer wraps `Func<W, K<M, (A Value, W Output)>>` under a monoid constraint and threads the accumulator in as an argument rather than returning a fresh log per step: the run is seeded with the monoid identity, and every bind feeds the first computation's accumulated output as the input accumulator of the second. This difference-list shape is why the writer avoids the quadratic left-nested combine of a log-returning writer — the combine is one threaded parameter, appended on the right.
- Emission appends the new fact as the right operand of the existing accumulator, so a fold of receipt emissions lands in emission order. The listen/pass/censor projections each call the seeded run and re-prepend the prior accumulator — nesting them inside a tight loop reconstructs the monoid combine per iteration, so the cheap form is one outer listen or censor over the whole fold, never one per step.
- A fact stream expressed as the writer's accumulator is a monoid append threaded through the effect with no cell and no observer subscription: the stream is the transformer output, projected by the consumer's fold over the final accumulator — replay, current-state projection, and audit are folds over one value, with no subscription race and no delivery-window discipline.

[THREE_CHANNELS_ONE_FUNCTION]:
- The fused reader-writer-state transformer collapses all three threadings into one wrapped function over a triple, and one bind threads every channel in its correct discipline simultaneously — environment held constant, log and state advanced:

```csharp
new RWST<R, W, S, M, B>(input =>
    ma.runRWST(input).Bind(x =>
        f(x.Value).runRWST((input.Env, x.Output, x.State))));
```

- The constant-environment / advanced-output-and-state split is visible in the one continuation call: the environment is re-supplied unchanged while output and state carry forward. This is the canonical shape for an effect that reads a runtime, emits a receipt log, and threads accumulating algorithm state in one pass — replacing three parallel cells and three observer wirings with one function and one fold over the final triple. The modify/gets/censor/local projections are case projections of this single carrier, not sibling surfaces.
- Stack-safe iteration is deliberately not bind-recursion: each transformer's recursion driver delegates to the inner monad's trampoline with a loop/done step tag, and an unexpected tag is a hard not-supported failure rather than a silent stall. A custom inner monad stacked underneath must honor the loop contract, or deep transformer iteration surfaces the exception from inside the trampoline instead of overflowing the stack.

[CELL_VERSUS_THREAD_SELECTION]:
- A threaded receipt dies at the run boundary: cross-run accumulation, cross-thread visibility, or observation by code outside the pipeline still requires a state cell. Within one run the transformer output is strictly stronger — pure, replayable, free of the multiple-execution-under-contention hazard that constrains every cell swap function, and free of the subscribe-then-snapshot initialization discipline observers need.
- Contended append-only streams stay on a cell because the monoid thread has no concurrent writers by construction: when multiple producers must interleave facts, the cell's compare-and-swap is the serialization point and batching multiple appends into one swap is the contention lever. The selection rule: one producer inside one run threads the receipt; many producers or many runs fold into a cell — and a design that wants both layers the threaded receipt per run, folding each run's final output into the cell once at the run edge, one swap per run instead of one per fact.
