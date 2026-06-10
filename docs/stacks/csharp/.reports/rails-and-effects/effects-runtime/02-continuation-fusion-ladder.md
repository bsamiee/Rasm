# Continuation-Fusion Node Ladder

[NODE_AS_PARTIAL_REIFICATION]:
- A bound `Map`/`Bind` chain is not a linked list of one-operation wrappers; each composition operator self-rewrites into a *wider* specialized node that reifies several fused operations as separate record fields. The carrier is a closed family of fusion records — `Pure`, `Map`, `Bind`, `BindMap`, `BindBind`, `BindBindMap`, `BindBindMapBind` — each holding a seed plus two to four function slots, so a four-operation pipeline allocates one node, not four.
- Each node's `Invoke` reconstitutes the fused expression in source order from its slots — a `BindBindMapBind` node runs `Ff(Seed).Bind(Fg).Map(Fh).Bind(Fi)` — and returns the resulting carrier as a continuation the interpreter re-enters with no managed stack frame. Fusion is a *builder-time* allocation collapse, not a run-time one: the run cost is identical, the construction cost is amortized across the fused segment.
- The base abstraction splits on whether a step terminates or continues: a terminal node returns the raw bound value from `Invoke`, a continuation node returns the next carrier. Only continuation nodes participate in fusion, because only they carry a `Next` slot to fold an incoming operation into.

[LADDER_TRANSITION_GRAPH]:
- The node family is a deterministic state machine over the pair (current node kind, incoming operation). The seed transitions: `Pure.Map → PureMap`, `Pure.Bind → Bind`; `Bind.Map → BindMap`, `Bind.Bind → BindBind`; `BindMap.Map → BindMap` widened by one type slot, `BindMap.Bind → BindMap2`; `BindBind.Map → BindBindMap`, `BindBind.Bind →` an *in-place* `BindBind` whose second slot absorbs the new bind; `BindBindMap.Bind → BindBindMapBind`.
- The ladder saturates at four fused operations. Past saturation a node stops widening and instead **rewrites its terminal slot functionally**: `BindMap.Map` folds the new map as `x => f(Fh(x))` into the existing tail rather than allocating a fifth type parameter; `BindBindMapBind.Bind` rewrites its last slot as `x => Fi(x).Bind(g)`. The arity ceiling is a deliberate cap — beyond it, composition is closure nesting inside the final slot, keeping the type explosion bounded.
- Map-after-map always fuses into a single composed function slot, never a new node; bind-after-bind in the saturated nodes likewise threads through the tail. This means the only node-allocating transitions are the ones that *introduce a new operation kind* against an unsaturated node; repeated same-kind tails are free.

[SYNC_ASYNC_LADDER_MIRROR]:
- There are two parallel ladders sharing the transition graph: a synchronous family whose `Invoke` returns a carrier, and an asynchronous family whose `Invoke` is an `async` method returning a carrier inside a value-task. The async family is reached only at a real suspension point.
- The bridge is condition-driven inside `Invoke`: a sync map node runs its inner effect, inspects the resulting value-task's completion flag, and on completion stays on the sync ladder; on a genuinely-pending task it **spills its already-fused slots into the async mirror node** so no fusion is lost crossing the await. A completed value-task therefore never allocates an async state machine — the synchronous appearance of a fast effect is structural, not a fast-path branch bolted on.
- The async mirror has its own collapse rules that differ where async forces it: an async bind-map node, on receiving a `Bind`, transitions to an async bind-bind node folding the map into the first slot, because the map result must be re-sequenced through a bind to await correctly. The graphs are isomorphic in shape but the async side has one extra collapse the sync side does not.

```csharp
// one node holds the whole pipeline; building it allocates O(1) per saturated segment:
static IO<int> Pipeline(IO<int> seed) =>
    seed.Bind(a => Lookup(a))   // Bind         -> BindBind        (seed-rooted)
        .Bind(b => Widen(b))    // Bind         -> in-place tail fold
        .Map(c => c + 1)        // Map          -> BindBindMap
        .Bind(d => Persist(d)); // Bind         -> BindBindMapBind  (ladder saturated)
// further .Map/.Bind here rewrite the terminal slot's closure, not a 6th node.
```

[TOKEN_SUBLADDER_AND_ENVIRONMENT_FUSION]:
- Cancellation-token access is its own two-node sub-ladder (`TokenMap`, `TokenBind`) that never widens past arity two. A token node holds the env→carrier reader plus one accumulated tail function; `TokenMap.Map` folds as `x => f(F(x))`, `TokenMap.Bind` transitions to `TokenBind` carrying the same reader. The token read is deferred to `Invoke`, where the live environment token is projected and the fused tail applied — token access fuses with downstream work into a single node rather than a lift-then-bind pair.
- The token sub-ladder exists precisely because token access is so common in tight loops that a non-fusing lift would dominate allocation; folding the entire post-token chain into the reader's tail eliminates every intermediate continuation between the token read and the next suspension.

[SEQUENCE_FUSION_AND_ITERATOR_CARRY]:
- A whole effectful sequence fuses into one node carrying an iterator of pending effects plus a single `Next` continuation. `Map`/`Bind` on it rewrite only `Next` — the iterator is untouched — so post-sequence work fuses into the sequence node instead of wrapping it.
- Its `Invoke` runs the head effect, inspects completion, and forks the remaining work into a sync-driver or async-driver node that carries the *split tail* of the iterator forward alongside the same `Next`. Sequencing is therefore a self-feeding fused loop: each step re-emits a node holding the unconsumed iterator suffix and the unchanged continuation, never materializing the whole sequence as a chain of binds.

[DESIGN_PRESSURE]:
- The ladder is the reason a deep monadic pipeline is allocation-cheap to *build* and the reason an extension carrier must never hand-roll its own `Map`/`Bind` nodes: subclassing the carrier outside the four legal interpreter base types is rejected at run time, so all fusion must route through the sanctioned bases or it is lost. A custom effect step is expressed as one of the legal base nodes whose `Invoke` returns a fused continuation — never as a bespoke wrapper that the ladder cannot collapse into.
- Fusion is structural, not law-checked: a node's `Invoke` must reconstitute its slots in exact source order, because the fields encode an ordered expression, not a set. A slot-reordering or value-dropping `Invoke` produces a node that type-checks but violates monad associativity silently — the same hazard class as a structure-altering natural transformation, surfaced here at the interpreter rather than the carrier.
- Equality and hashing are structural over the function slots, so two fused nodes are equal only when their seed and every captured delegate are reference-equal — fused pipelines are not value-comparable in practice, and any caching keyed on a built carrier keys on identity, never on the logical effect it denotes. The fused node is an opaque execution plan, not a comparable description.
