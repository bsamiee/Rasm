# [TYPESCRIPT_COMPUTATION]

Every working body between the decode seam and the rail is one expression over already-admitted values: a single-pass fold into a typed seed, a lazy `Iterable` pipeline materialized once at its tail, an index-free window algebra, a `mapAccum` Mealy step, a structural recursion destructured over its closed family, or a measured kernel that detaches an immutable result. The value crossed its boundary upstream, so the interior is total over it and never re-validates; the result composes combinators at the depth the collection modules already reach — never a `for` loop pushing into an array, an index counter the value's own structure already answers, or a second pass over a sequence one seeded pass closes. This page owns the pure, in-process computation algebra: the shape a working body takes after its values are admitted and before its outcome rides a rail.

Six siblings own material this algebra composes as settled: a fold whose accumulator is a carrier and the abort-versus-accumulate disposition are `rails-and-effects.md`'s; the carrier discriminant and the `Stream.mapAccum` lift of the step written here are `streams.md`'s; containers, algebra instances, and the merge tables are `values.md`'s; `Match` terminals and overload surfaces are `surfaces-and-dispatch.md`'s; cross-fiber cells are `concurrency.md`'s; the kernel mark's legality and its cast algebra are `language.md`'s — this page owns when a kernel is earned and which algorithmic forms it takes. What remains is the computation algebra, and its one collapse is uniform: a multi-pass scatter folds into one seeded pass, a materialized intermediate dissolves into a lazy combinator, a hand-indexed window becomes a window operator, a cell beside a traversal becomes a `mapAccum` step, an unbounded native recursion becomes a frontier kernel, and a hand memo map becomes a tabulation fold.

## [01]-[INDEX]

This table maps a computational shape to the form that owns it; the most specific shape wins.

| [INDEX] | [COMPUTATION]                              | [OWNING_FORM]                                                                 | [REJECTED_FORM]                          |
| :-----: | :----------------------------------------- | :---------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | several quantities over one sequence       | `Array.reduce` into one readonly seed; statistics project at read             | a `sum`/`min`/`max`/`length` pass apiece |
|  [02]   | running or cumulative trace                | `Array.scan`, the same `(step, SEED)` pair                                    | an index loop appending each step        |
|  [03]   | multi-stage transform over a large source  | lazy `Iterable` pipeline, one tail materialization                            | a fully built array per stage            |
|  [04]   | sliding or fixed windows                   | `Array.window` / `Array.chunksOf`                                             | `slice(i, i + n)` index stepping         |
|  [05]   | parallel or Cartesian co-iteration         | `Array.zipWith` / `Array.cartesianWith`                                       | nested `for` loops, a hand index pair    |
|  [06]   | keyed, prefix, or breach grouping          | `Array.groupBy` / `Array.span` / `Array.splitWhere`                           | a mutable bucket accumulator             |
|  [07]   | element-wise transform with threaded state | `Array.mapAccum` Mealy step                                                   | a `let` rebound across a `map`           |
|  [08]   | closed transition system                   | vocabulary rows carrying `(next, emit)`; one fold drives                      | a `switch` advancing a mutable phase     |
|  [09]   | stateful actor behind a request surface    | `Machine.makeWith` procedure rows, booted scoped                              | a class of mutable fields and methods    |
|  [10]   | bounded recursion over a closed family     | one `(leaf, join)` algebra threaded by one traversal                          | a recursive function per reduction       |
|  [11]   | input-scaled recursion depth               | frontier kernel over an immutable `List` stack; rail `Effect.iterate`         | native recursion past the stack ceiling  |
|  [12]   | graph-shaped traversal or path cost        | `Graph.dfs`/`Graph.topo`/`Graph.dijkstra` walker reads                        | a hand frontier over adjacency state     |
|  [13]   | overlapping subproblems                    | tabulation fold threading the solved table; `Effect.cachedFunction` effectful | a hand memo `Map` beside the function    |
|  [14]   | measured hot path                          | marked kernel — `MutableHashMap`/`TypedArray` draft, detached immutable       | ambient mutation in domain flow          |

## [02]-[SEED_FOLD]

A body that needs several quantities from one sequence folds them in one pass into a typed seed whose fields are the running invariants. The collapse is the multi-pass scatter: `sum`, `min`, `max`, and `length` over one collection are four traversals; one `Array.reduce(values, SEED, step)` is one. The seed is the growth site — a new statistic is one field plus one line in the step — and the same `(step, SEED)` pair serves the terminal fold and the running trace, so the two forms cannot drift.

[STRUCT_SEED]:
- Law: the seed is a plain readonly record, never `Data`-constructed — it is fold-local and no container consumes its identity, so construction-time `Equal`/`Hash` buys one dead allocation per element; three or more named running quantities earn the record, and a two-quantity seed stays a readonly tuple.
- Law: the fold carries raw associative accumulators — counts, sums, sums of squares, extrema — and derived statistics project at read through the `Option`-returning partial owners; a running mean or variance carried as fold state is the rejected form because it cannot combine across two partial folds.
- Law: partial folds fuse through the seed's own `Semigroup.struct` row table — the seed shape doubles as the merge table, so lane-partitioned folds combine row-by-row under `combineMany` and the fusion is recoverable from one declaration; the instance algebra is `values.md`'s, composed here.
- Reject: a `let` rebound across steps; a `sum`/`min`/`max` pass apiece over one sequence; a mutated accumulator object; a `Data.struct` seed.

[SCAN_TRACE]:
- Law: `Array.scan(values, SEED, step)` shares the fold's exact `(step, SEED)` pair and keeps every intermediate seed the fold discards — the return is `NonEmptyArray` with the seed first, so the origin's presence is a type fact, and a consumer that must not see it drops it at the read, never by patching the step; `Array.scanRight` threads the suffix trace from the right.
- Boundary: a fold whose accumulator is `Option`/`Either` or whose faults must accumulate is `rails-and-effects.md`'s carrier algebra, and the same trace over a live feed is `Stream.scan`, `streams.md`'s — this seed is a pure value, total over admitted elements.

```typescript
import * as Semigroup from "@effect/typeclass/Semigroup"
import * as NumberInstances from "@effect/typeclass/data/Number"
import { Array, Number, Option, pipe } from "effect"

type Sketch = {
  readonly count: number
  readonly total: number
  readonly squared: number
  readonly floor: number
  readonly crest: number
}

const _SEED: Sketch = { count: 0, total: 0, squared: 0, floor: Infinity, crest: -Infinity } // plain readonly seed: fold-local, no container consumes its identity, so Data construction buys nothing

const _stepped = (sketch: Sketch, load: number): Sketch => ({ // raw associative accumulators only: which field advances lives in the construction, never an if ladder
  count: sketch.count + 1,
  total: sketch.total + load,
  squared: sketch.squared + load * load,
  floor: Number.min(sketch.floor, load),
  crest: Number.max(sketch.crest, load),
})

const _merged: Semigroup.Semigroup<Sketch> = Semigroup.struct({ // the seed shape doubles as its own merge table: partial folds fuse row-by-row through the composed instance
  count: NumberInstances.SemigroupSum,
  total: NumberInstances.SemigroupSum,
  squared: NumberInstances.SemigroupSum,
  floor: NumberInstances.SemigroupMin,
  crest: NumberInstances.SemigroupMax,
})

const sketched = (loads: ReadonlyArray<number>): Sketch =>
  Array.reduce(loads, _SEED, _stepped)                        // several quantities, one pass: the sum/min/max/length scatter is deleted

const traced = (loads: ReadonlyArray<number>): Array.NonEmptyArray<Sketch> =>
  Array.scan(loads, _SEED, _stepped)                          // the same (step, SEED) pair: the trace keeps every seed the fold discards, origin emitted first

const fused = (lanes: Array.NonEmptyReadonlyArray<ReadonlyArray<number>>): Sketch =>
  _merged.combineMany(sketched(Array.headNonEmpty(lanes)), Array.map(Array.tailNonEmpty(lanes), sketched)) // partitioned lanes fold independently and fuse: only raw accumulators merge

const _PACE: readonly [count: number, total: number] = [0, 0] // a two-quantity seed stays a readonly tuple: the named record is earned at three running invariants

const paced = (loads: ReadonlyArray<number>): Option.Option<number> =>
  pipe(
    Array.reduce(loads, _PACE, ([count, total], load) => [count + 1, total + load] as const),
    ([count, total]) => Number.divide(total, count),          // partiality folds once at the read: the empty fold is a value, never a NaN
  )

const spread = (sketch: Sketch): Option.Option<number> =>
  Option.map(Number.divide(sketch.squared, sketch.count), (moment) => moment - (sketch.total / sketch.count) ** 2) // derived statistics project at read: a carried mean cannot combine across partial folds

// --- [EXPORTS] --------------------------------------------------------------------------

export { fused, paced, sketched, spread, traced }
export type { Sketch }
```

## [03]-[LAZY_AND_WINDOW]

A multi-stage transform selects eager or lazy by materialization cost, and its windows never spell an index. Eager `Array` combinators own the bounded collection in hand; the `Iterable` module owns the pipeline whose source is large, generated, or consumed once — each stage an iterator over the previous, exactly one materialization at the tail. Window, chunk, co-iteration, and grouping are named operators, so the moment slice arithmetic, a nested loop, or a bucket accumulator appears, an operator already owns the shape.

[LAZY_PIPELINE]:
- Law: laziness is a memory contract — `Iterable.map`/`filter`/`filterMap`/`take`/`takeWhile`/`drop`/`flatMap`/`flatten`/`dedupeAdjacent` compose without materializing and hold bounded memory over an unbounded source; the tail materializes once — `Array.fromIterable` for a collection, `Iterable.reduce` for a value — and a fully built array mid-chain re-buys the allocation laziness deleted.
- Law: production is an anamorphism — `Iterable.unfold(seed, step)` generates from a seed with `Option` termination, and `Iterable.range`/`Iterable.makeBy` name the arithmetic sources — so a `function*` generator in domain flow is a statement seam smuggled in as production.
- Reject: a `[...feed]` spread or intermediate array between stages; a hand `while` loop growing an array where `unfold` states the production; laziness over a small bounded collection already in hand — the eager combinators read shorter and allocate once.

[WINDOW_ALGEBRA]:
- Law: an index is never spelled — `Array.window(values, width)` yields every full sliding window and only full windows, `Array.chunksOf(values, width)` yields fixed pieces keeping the shorter last, and the two differ by name, never by a flag; position, where genuinely needed, is the callback's own index parameter, which every fold and map already passes.
- Law: co-iteration is a named pairing — `Array.zipWith(self, that, combine)` pairs positionally and truncates to the shorter operand, the stated semantics a caller proves length against; adjacent difference is `zipWith` against the self dropped by one, and `Array.cartesianWith(self, that, combine)` collapses the nested loop into one operator.
- Law: grouping selects by discipline — `Array.groupBy(values, key)` builds the keyed record of `NonEmptyArray` runs in one pass, `Array.span(values, predicate)` splits the satisfying prefix, `Array.splitWhere` splits at the first breach, and `Iterable.groupWith` groups adjacent runs under an equivalence; a mutable bucket object grown in a loop restates all four.
- Reject: `values[index + 1]` window arithmetic; a `for i`/`for j` cross product; a run accumulator tracking a `last` variable; index-keyed objects standing where `groupBy` owns the keying.

```typescript
import { Array, Iterable, Number, Option, Record, pipe } from "effect"

type Beat = readonly [lane: string, load: number]

const _laddered = (ceiling: number, floor: number): Iterable<number> =>
  Iterable.unfold(ceiling, (rung) => (rung >= floor ? Option.some([rung, rung / 2]) : Option.none())) // the anamorphism: production is a seeded combinator with Option termination, never a hand loop

const profiled = (batches: Iterable<Iterable<Beat>>, gate: number, cap: number): Record<string, number> =>
  pipe(
    Iterable.flatten(batches),                                 // every stage is an iterator over the previous: bounded memory holds an unbounded source
    Iterable.takeWhile(([, load]) => load >= 0),               // the prefix trim: a break statement dissolved into a named bound
    Iterable.filter(([, load]) => load >= gate),
    Iterable.take(cap),
    Array.fromIterable,                                        // the one materialization, at the tail — everything above stays unevaluated until this pull
    Array.groupBy(([lane]) => lane),
    Record.map((run) => Number.sumAll(Array.map(run, ([, load]) => load))),
  )

const meshed = (loads: ReadonlyArray<number>, ceiling: number, floor: number): {
  readonly paired: ReadonlyArray<number>
  readonly crossed: ReadonlyArray<number>
} => ({
  paired: Array.fromIterable(Iterable.zipWith(loads, _laddered(ceiling, floor), (load, decay) => load * decay)), // positional pairing truncates at the shorter operand: the ladder bounds the take
  crossed: Array.cartesianWith(loads, Array.fromIterable(_laddered(ceiling, floor)), (load, decay) => load * decay), // the Cartesian pairing: nested for loops collapse into one operator
})

const paned = (loads: ReadonlyArray<number>, width: number): {
  readonly drifts: ReadonlyArray<number>
  readonly glides: ReadonlyArray<number>
  readonly bursts: ReadonlyArray<number>
} => ({
  drifts: Array.zipWith(loads, Array.drop(loads, 1), (prior, next) => next - prior), // adjacent difference: co-iteration against the self dropped by one, no index spelled
  glides: Array.map(Array.window(loads, width), (pane) => Number.sumAll(pane) / width), // full sliding windows only: the operator owns the bound the slice loop re-derives
  bursts: Array.map(Array.chunksOf(loads, width), Number.sumAll), // fixed pieces, shorter last kept: chunk and window differ by name, never by flag
})

const staged = (loads: ReadonlyArray<number>, gate: number): {
  readonly ramp: ReadonlyArray<number>
  readonly steady: ReadonlyArray<number>
  readonly spike: ReadonlyArray<number>
} =>
  pipe(
    Array.span(loads, (load) => load < gate),                  // the satisfying prefix splits by predicate, the remainder splits at first breach
    ([ramp, rest]) => pipe(Array.splitWhere(rest, (load) => load >= gate * 2), ([steady, spike]) => ({ ramp, steady, spike })),
  )

// --- [EXPORTS] --------------------------------------------------------------------------

export { meshed, paned, profiled, staged }
export type { Beat }
```

## [04]-[STATEFUL_STEP]

State that threads element-to-element is a Mealy step — one `(state, element) => [state, output]` declaration — and stateful computation has exactly three rungs: the step folded over a collection, the step's rows lifted into a vocabulary table when the state space closes, and the actor when the state must outlive one traversal and answer requests. Every rung is the same shape written once; a `let` beside a `map`, a `switch` advancing a mutable phase, and a class of mutable fields are one defect at three sizes.

[MEALY_STEP]:
- Law: `Array.mapAccum(values, seed, step)` threads the accumulator and emits per element, returning `[state, outputs]` — the carried state decouples from the emitted shape, and the step's third parameter is the index, so a hand counter beside the fold restates the signature.
- Law: the step is written once and both carriers consume it — the same declaration lifts unchanged into `Stream.mapAccum` when the data becomes incremental; the lift and its pipeline law are `streams.md`'s.
- Reject: a `let` rebound across `Array.map`; a module-level cell advanced by a traversal; a step hand-specialized per carrier.

[TRANSITION_LADDER]:
- Law: a closed transition system is a vocabulary table, not control flow — one `as const` anchor keyed by state whose rows carry `(next, emit)` per input kind, driven by one `mapAccum` reading rows — so a new phase or input is a row, never a branch; the anchor's derivation and guard mechanics are `derivation.md`'s.
- Law: the ladder tops at the actor — `Machine.makeWith<State, Input>()` from `@effect/experimental` declares the state's request surface as procedure rows: `Machine.procedures.make(initial)` grown by `Machine.procedures.add<Req>()(tag, handler)`, each handler `({ request, state })` returning `[reply, state]` on the rail, and `Machine.boot` yields the scoped `Actor` whose `send` serializes every request against one state on one fiber.
- Law: persistence and recovery are definition facts — `boot`'s `previousState` resumes a machine mid-life, `Machine.retry` re-initializes through a `Schedule` policy value, and the `Machine.makeSerializable` variant round-trips state through `Machine.snapshot`/`Machine.restore`.
- Boundary: the actor exists because its state outlives one traversal and answers a request surface — state shared across arbitrary fibers with no request surface is `concurrency.md`'s cell selection.
- Reject: a class of mutable fields and methods; a `switch` over phases mutating an outer variable; a reply table declared beside the `(next, emit)` rows it duplicates.

```typescript
import { Machine } from "@effect/experimental"
import { Array, Effect, Request, Schedule, type Scope } from "effect"

const Transit = {                                             // the transition system is a vocabulary anchor: rows carry (next, emit), a new phase or signal is a row, never a branch
  idle: {
    pulse: { next: "live", emit: "opened" },
    fault: { next: "halted", emit: "tripped" },
    reset: { next: "idle", emit: "held" },
  },
  live: {
    pulse: { next: "live", emit: "held" },
    fault: { next: "halted", emit: "tripped" },
    reset: { next: "idle", emit: "closed" },
  },
  halted: {
    pulse: { next: "halted", emit: "held" },
    fault: { next: "halted", emit: "held" },
    reset: { next: "idle", emit: "closed" },
  },
} as const

declare namespace Transit {
  type Phase = keyof typeof Transit
  type Signal = keyof (typeof Transit)[Phase]
  type Row = (typeof Transit)[Phase][Signal]
  type Verdict = Row["emit"]
  type _Rows<T extends Record<Phase, Record<Signal, { readonly next: Phase; readonly emit: string }>> = typeof Transit> = T // row guard: the contract homes in the merged companion, the anchor keeps its literals
}

const _stepped = (phase: Transit.Phase, signal: Transit.Signal): readonly [Transit.Phase, Transit.Verdict] =>
  [Transit[phase][signal].next, Transit[phase][signal].emit]  // the Mealy shape: carried state decouples from emitted verdict — the exact step Stream.mapAccum lifts unchanged

const driven = (origin: Transit.Phase, signals: ReadonlyArray<Transit.Signal>): [Transit.Phase, ReadonlyArray<Transit.Verdict>] =>
  Array.mapAccum(signals, origin, _stepped)                   // one traversal threads the phase and emits per element; no let advances beside a map

class Feed extends Request.TaggedClass("Feed")<Transit.Verdict, never, { readonly signal: Transit.Signal }> {}
class Poll extends Request.TaggedClass("Poll")<Transit.Phase, never, {}> {}

const _relay = Machine.makeWith<Transit.Phase, Transit.Phase>()((input, previous) =>
  Machine.procedures.make<Transit.Phase>(previous ?? input).pipe( // reboot resumes: boot's previousState arrives here, so restart is a fact of the definition
    Machine.procedures.add<Feed>()("Feed", ({ request, state }) =>
      Effect.succeed([Transit[state][request.signal].emit, Transit[state][request.signal].next] as const)), // the same (next, emit) row drives the actor: the reply is the emission, the state advances
    Machine.procedures.add<Poll>()("Poll", ({ state }) => Effect.succeed([state, state] as const)), // a new request is one procedure row against the same state, never a second actor
  ),
).pipe(Machine.retry(Schedule.recurs(3)))                     // a defect re-initializes from the last state: recovery is a policy value on the definition

const booted = (
  origin: Transit.Phase,
  signals: ReadonlyArray<Transit.Signal>,
): Effect.Effect<{ readonly verdicts: ReadonlyArray<Transit.Verdict>; readonly landed: Transit.Phase }, never, Scope.Scope> =>
  Effect.gen(function* () {
    const actor = yield* Machine.boot(_relay, origin)         // the scoped actor: send serializes every request against one state on one fiber
    const verdicts = yield* Effect.forEach(signals, (signal) => actor.send(new Feed({ signal })))
    return { verdicts, landed: yield* actor.send(new Poll()) } // one table, two carriers: driven and the actor emit identical verdicts
  })

// --- [EXPORTS] --------------------------------------------------------------------------

export { Transit, booted, driven }
```

## [05]-[STRUCTURAL_RECURSION]

A computation over a recursive structure is one algebra applied by one traversal: a `(leaf, join)` pair serves every reduction over the closed family, and depth selects the execution form — native recursion to data depth, the frontier kernel or the rail past it — never the algebra. When the structure is a graph rather than a tree, the traversal itself is owned: the walker families delete the hand frontier.

[CATAMORPHISM]:
- Law: one `(leaf, join)` algebra value serves every reduction — sum, depth, flatten, render are algebra rows, never a recursive function per reduction — and the traversal threads children through the same public surface; the fold that returns the bare algebra parameter rides tag refinement, while the concrete-result recursion earns the `Match` pipeline, whose terminal and subtraction mechanics are `surfaces-and-dispatch.md`'s.
- Law: native recursion is legal only to data depth — a structure admitted at bounded fan-in and height reads freely, and input-scaled or adversarial depth forfeits the form, because the JS call stack is a fixed platform ceiling no flag, option, or worker size raises.
- Exemption: the frontier kernel is this page's statement-bearing seam, licensed by that fixed ceiling — an immutable `List` work stack rebound each turn, `expand` frames sliding a `combine` marker beneath a branch's children and a second immutable stack carrying results until the marker joins them — so the traversal is iterative exactly where native recursion overflows; the mark's legality is `language.md`'s.
- Law: an effectful step at depth rides the rail's own trampoline — `Effect.iterate(initial, { while, body })` advances a state heap-bound, `Effect.loop` collects per step, and `Effect.suspend` defers a recursive definition so constructing the effect does not itself recurse.
- Reject: a recursion re-authored per reduction; a depth flag threaded through signatures; a result array mutated by a recursive helper; a raised stack budget standing where the frontier folds.

[GRAPH_CONSUMPTION]:
- Law: when the structure is a graph the frontier is never hand-rolled — `Graph.dfs`/`Graph.bfs`/`Graph.dfsPostOrder`/`Graph.topo` return walkers consumed as iterables through `Graph.values`/`Graph.indices`/`Graph.entries`, so traversal order is a named read; `Graph` is `@experimental`, an admission fact the pin owns.
- Law: path and condensation questions are owner reads — `Graph.dijkstra`/`Graph.astar`/`Graph.bellmanFord` return `Option` of a `PathResult` carrying `path`, `distance`, and `costs`, `Graph.floydWarshall` answers all pairs, and `Graph.stronglyConnectedComponents`/`Graph.isAcyclic` answer condensation — a hand Dijkstra over adjacency state restates the family.
- Boundary: graph construction, the `Graph.mutate` write discipline, and `Trie` prefix reads are `values.md`'s owners — this page owns only the algorithmic consumption.

```typescript
import { Array, Effect, Graph, List, Match, Number, Option } from "effect"

type Limb =
  | { readonly _tag: "tip"; readonly load: number }
  | { readonly _tag: "fork"; readonly limbs: ReadonlyArray<Limb> }

type Algebra<R> = { readonly tip: (load: number) => R; readonly fork: (parts: ReadonlyArray<R>) => R }

type _Frame = { readonly _tag: "expand"; readonly limb: Limb } | { readonly _tag: "combine"; readonly arity: number }

const folded = <R>(limb: Limb, algebra: Algebra<R>): R =>    // the bare-parameter fold rides tag narrowing: record dispatch computes through Unify and strands R
  limb._tag === "tip"
    ? algebra.tip(limb.load)
    : algebra.fork(Array.map(limb.limbs, (child) => folded(child, algebra)))

const _deep: (limb: Limb) => number = Match.type<Limb>().pipe( // the concrete-result recursion earns the Match pipeline: the annotated const licenses self-reference in the arms
  Match.withReturnType<number>(),
  Match.tag("tip", () => 1),
  Match.tag("fork", ({ limbs }) => 1 + Array.reduce(Array.map(limbs, (child) => _deep(child)), 0, Number.max)),
  Match.exhaustive,
)

const _massed: Algebra<number> = { tip: (load) => load, fork: Number.sumAll }

const fathomed = <R>(root: Limb, algebra: Algebra<R>): R => { // BOUNDARY ADAPTER: frontier kernel — the fixed JS call-stack ceiling licenses this statement seam; the rebound Lists never escape
  let frames: List.List<_Frame> = List.of<_Frame>({ _tag: "expand", limb: root })
  let results: List.List<R> = List.empty<R>()
  while (List.isCons(frames)) {
    const head: _Frame = frames.head
    frames = frames.tail
    if (head._tag === "combine") {
      const [parts, rest] = List.splitAt(results, head.arity)
      results = List.cons(algebra.fork(List.toArray(parts)), rest)
    } else if (head.limb._tag === "tip") {
      results = List.cons(algebra.tip(head.limb.load), results)
    } else {
      frames = Array.reduce(                                  // a combine marker slides beneath the children; consing in order resolves the last child first, so joined parts read left-to-right
        head.limb.limbs,
        List.cons<_Frame>({ _tag: "combine", arity: head.limb.limbs.length }, frames),
        (acc, limb) => List.cons<_Frame>({ _tag: "expand", limb }, acc),
      )
    }
  }
  return List.unsafeHead(results)                             // sanctioned read: the loop invariant leaves exactly one result on the stack
}

const assayed = (limb: Limb): { readonly depth: number; readonly mass: number } =>
  ({ depth: _deep(limb), mass: fathomed(limb, _massed) })     // one closed family, every reduction an algebra value or dispatch row: depth selects the execution form, never the algebra

const chased = <S, E, R>(seed: S, advance: (state: S) => Effect.Effect<Option.Option<S>, E, R>): Effect.Effect<S, E, R> =>
  Effect.map(
    Effect.iterate({ live: seed, spent: false }, {            // the rail is the trampoline: each body step returns to the driver, so depth is heap-bound
      while: (cursor) => !cursor.spent,
      body: (cursor) =>
        Effect.map(advance(cursor.live), Option.match({
          onNone: () => ({ live: cursor.live, spent: true }),
          onSome: (state) => ({ live: state, spent: false }),
        })),
    }),
    (cursor) => cursor.live,
  )

const plumbed = (limb: Limb): Effect.Effect<number> =>
  limb._tag === "tip"
    ? Effect.succeed(limb.load)
    : Effect.suspend(() => Effect.map(Effect.forEach(limb.limbs, plumbed), Number.sumAll)) // suspend defers construction: the recursion unfolds one level per fiber step, heap-bound like iterate

const charted = (plan: Graph.DirectedGraph<string, number>, source: number, target: number): {
  readonly stages: ReadonlyArray<string>
  readonly span: Option.Option<number>
  readonly knots: ReadonlyArray<ReadonlyArray<number>>
} => ({
  stages: Array.fromIterable(Graph.values(Graph.topo(plan))), // traversal order is a walker read, never a hand frontier over adjacency state
  span: Option.map(Graph.dijkstra(plan, { source, target, cost: (toll) => toll }), (path) => path.distance),
  knots: Array.filter(Graph.stronglyConnectedComponents(plan), (component) => component.length > 1), // the condensation read: every nontrivial component is a cycle to dissolve
})

// --- [EXPORTS] --------------------------------------------------------------------------

export { assayed, charted, chased, fathomed, folded, plumbed }
export type { Algebra, Limb }
```

## [06]-[MEMO_AND_KERNEL]

No pure memoization combinator ships, and that absence is the law: a pure recurrence with overlapping subproblems tabulates bottom-up as a fold that threads its solved table, effectful memoization rides owned combinators, and a mutation kernel exists only where measurement indicts the fold. The hand memo `Map`, the module-level cache object, and the unmarked hot loop are three spellings of one defect.

[MEMO_OWNERS]:
- Law: the pure recurrence tabulates — one fold over the ordered subproblem space threading the solved table as its seed, an indexed `ReadonlyArray` for dense keys and a `HashMap` for sparse ones — the structural dual of a memo with no cache identity, no eviction, and no stack growth, so the deep dynamic program is a fold, never a stack-bound recursion.
- Law: effectful memoization is an owned combinator — `Effect.cachedFunction(f)` memoizes per argument under structural `Equal` with an `Equivalence` as the optional second parameter, `Effect.once` collapses to one execution, and `Effect.cached` defers and memoizes one result — each returns the memoized surface inside `Effect`, so the cache lives exactly as long as the flow that built it.
- Boundary: keyed concurrent caching under capacity and TTL policy is `Cache.make`, `concurrency.md`'s contention owner — reached for when concurrent misses must collapse, never when a pure table folds.
- Reject: a hand memo `Map` beside the function; a captured mutable operand the key omits; a module-level cache object outliving every flow that reads it.

[KERNEL_EARN]:
- Law: the earn test is sequential — fold first, measure, then mark: a kernel exists only where a measured hot path indicts the fold's allocation or dispatch cost, or where a platform contract forces statements; a kernel kept after its measurement pressure disappears reverts to the fold.
- Law: the draft is scoped and the return detaches — `MutableHashMap`/`MutableList` batch structural-keyed writes, a `TypedArray` owns the numeric lane — and the kernel returns an immutable projection with no live reference escaping; the mark's legality and the cast algebra inside it are `language.md`'s.
- Reject: a `Map` or object cache in domain flow; a `TypedArray` view escaping its kernel; mutation justified by style where no measurement exists.

```typescript
import { Array, Effect, HashMap, MutableHashMap, Number, Option, pipe } from "effect"

const _BASE: ReadonlyArray<number> = [1]                      // the solved table's origin row: one path reaches zero

const tallied = (target: number, strides: ReadonlyArray<number>): number =>
  pipe(
    Array.reduce(Array.range(1, target), _BASE, (table, rung) => // tabulation is the fold dual of the memo: the solved table threads as the seed, no cache identity, no stack growth
      Array.append(table, Number.sumAll(Array.filterMap(strides, (stride) => (stride <= rung ? Array.get(table, rung - stride) : Option.none()))))),
    Array.get(target),                                        // the read is total by construction: an out-of-range target folds to the empty count
    Option.getOrElse(() => 0),
  )

const surveyed = <E, R>(keys: ReadonlyArray<string>, probe: (key: string) => Effect.Effect<number, E, R>): Effect.Effect<ReadonlyArray<number>, E, R> =>
  Effect.flatMap(Effect.cachedFunction(probe), (memoed) => Effect.forEach(keys, memoed)) // effectful memo is an owned combinator: repeated keys probe once under structural Equal

const primed = <A, E, R>(mint: Effect.Effect<A, E, R>): Effect.Effect<readonly [A, A], E, R> =>
  Effect.flatMap(Effect.cached(mint), (warm) => Effect.zip(warm, warm)) // the second pull replays the memo: one execution, and the cache lives exactly as long as the flow that built it

const binned = (loads: ReadonlyArray<number>, lens: { readonly width: number; readonly slots: number }): {
  readonly bins: ReadonlyArray<number>
  readonly peak: number
} => {                                                        // BOUNDARY ADAPTER: measured bincount kernel — the typed-array draft and the fused peak detach immutable at the return
  const draft = new Float64Array(lens.slots)
  let peak = 0
  for (const load of loads) {
    const slot = Math.min(Math.max(Math.trunc(load / lens.width), 0), lens.slots - 1)
    const risen = draft[slot]! + 1                            // sanctioned assertion: the clamp above is the bound evidence the checker cannot carry
    draft[slot] = risen
    peak = risen > peak ? risen : peak
  }
  return { bins: Array.fromIterable(draft), peak }            // the draft never escapes: the return is an immutable projection
}

const ranked = (lanes: ReadonlyArray<string>): HashMap.HashMap<string, number> => { // BOUNDARY ADAPTER: measured tally kernel — batch keyed writes against one draft, detached immutable
  const draft = MutableHashMap.empty<string, number>()
  for (const lane of lanes) {
    MutableHashMap.modifyAt(draft, lane, Option.match({ onNone: () => Option.some(1), onSome: (count) => Option.some(count + 1) }))
  }
  return HashMap.fromIterable(draft)                          // the draft dies here: consumers receive the persistent owner, never the mutable reference
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { binned, primed, ranked, surveyed, tallied }
```
