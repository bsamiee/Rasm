# [TYPESCRIPT_STREAMS]

Dataflow earns a `Stream` at exactly three signals: the source is unbounded or arrives over time, the consumer must observe elements before the source ends, or a window, backpressure bound, or resource lifetime shapes the flow. `Stream<A, E, R>` is the multi-valued `Effect` — the same typed error channel, the same requirement channel, chunked pull underneath — so a pipeline is one declaration whose state, windows, fan geometry, ingress policy, and batch collapse all attach as typed values on the owner, and the moment none of the three signals holds, the carrier is refused: bounded pure data folds without one, bounded effectful data traverses the rail.

Four siblings own material this page composes as settled: the `Effect` carrier algebra and `Schedule` policy values are `rails-and-effects.md`'s, `Queue`/`PubSub` mechanics and fiber ownership are `concurrency.md`'s, `Chunk` and the collection algebra are `values.md`'s, and edge decode is `boundaries.md`'s. What remains is this page's algebra — the carrier discriminant, the Mealy accumulator, window policy, fan geometry, the scoped ingress bridge, and the request resolver that collapses N+1 — and its one collapse is uniform: hand-rolled timers, counters, buffers, tag maps, and dedup caches dissolve into the declared operator that already owns the geometry.

## [01]-[INDEX]

This table maps a dataflow shape to the form that owns it; the most specific shape wins.

| [INDEX] | [DATAFLOW]                                | [OWNING_FORM]                                         | [REJECTED_FORM]                      |
| :-----: | :---------------------------------------- | :---------------------------------------------------- | :----------------------------------- |
|  [01]   | bounded pure reduction over admitted data | `Chunk.reduce` fold, no carrier                       | a `Stream` over in-memory data       |
|  [02]   | bounded collection, effect per element    | `Effect.forEach` with explicit `{ concurrency }`      | hand fiber loop; stream ceremony     |
|  [03]   | unbounded, incremental, or scoped source  | `Stream` pipeline into one `run*` terminal            | whole-feed materialization           |
|  [04]   | element-to-element state                  | `Stream.mapAccum` Mealy step                          | `Ref` mutated inside `map`           |
|  [05]   | running trace of an accumulator           | `Stream.scan`, seed emitted first                     | a fold that swallows intermediates   |
|  [06]   | count-or-latency window                   | `Stream.groupedWithin(width, patience)`               | timer fiber plus mutable buffer      |
|  [07]   | budget-closed, cadence-flushed batch      | `Stream.aggregateWithin` + `Sink.foldWeighted`        | count windows over uneven payloads   |
|  [08]   | static N consumers of one source          | `Stream.broadcast(n, lag)` scoped tuple               | re-running the source per consumer   |
|  [09]   | per-key partitioned processing            | `Stream.groupByKey` + `GroupBy.evaluate`              | keyed map of arrays, then re-stream  |
|  [10]   | heterogeneous source union                | `Stream.mergeWithTag` derived tagged merge            | hand-tagged `map` before `merge`     |
|  [11]   | keyed alignment of two sorted feeds       | `Stream.zipAllSortedByKeyWith`                        | materialize both sides and join      |
|  [12]   | callback or queue ingress                 | `Stream.asyncScoped` / `Stream.fromQueue`             | listener pushing into an outer array |
|  [13]   | N identical lookups in one flow           | `Request.TaggedClass` + `RequestResolver.makeBatched` | per-element query — the N+1          |

## [02]-[PIPELINE_SELECTION]

Three discriminants select the carrier — boundedness, effectfulness, incrementality — and the selection is a memory-and-latency contract, not a style: a `Stream` holds a bounded working set over an unbounded source, an `Effect.forEach` holds the whole collection and its results, a pure fold holds one accumulator. One entrypoint owns the modalities the concept genuinely serves, discriminating on the input value.

[CARRIER_SELECT]:
- Law: bounded plus pure folds without a carrier — `Chunk.reduce` over the admitted collection; bounded plus effectful traverses the rail — `Effect.forEach` with the degree explicit; unbounded, incremental, windowed, or resource-scoped dataflow is a `Stream`, the only form whose consumption is chunked pull with backpressure.
- Law: `Stream<A, E, R>` shares the rail's channels — faults are tagged values in `E`, capability rides `R` — and returns to the rail only at a terminal: `Stream.runFold` for one value, `Stream.run(sink)` for a composed consumer, `Stream.runDrain` for effects-only flow, `Stream.runForEach` for a per-element effect, and `Stream.runCollect` only at a tail already proven bounded, because it materializes the whole remainder.
- Law: one entrypoint owns batch and feed — overload signatures discriminate `Chunk` from `Stream` on the input value and conditionally return the value or the rail; seed and step are shared declarations, so the modalities cannot drift, and the overload set with its statement seam is `surfaces-and-dispatch.md`'s settled form.
- Reject: `Stream.fromIterable` wrapped around in-memory data to fold it; `Stream.runCollect` on an unbounded source; a `digestChunk`/`digestStream` twin pair; an array pushed inside `runForEach` — the fold owns accumulation.
- Boundary: `Chunk` algebra is `values.md`'s; `Effect.forEach` degrees and fiber ownership are `concurrency.md`'s — this page owns the selection between them and everything the `Stream` branch opens.

```typescript
import { Chunk, type Effect, Number, Stream } from "effect"

type Reading = { readonly key: string; readonly value: number }
type Digest = { readonly count: number; readonly total: number; readonly high: number }

const _SEED: Digest = { count: 0, total: 0, high: -Infinity }

const _folded = (digest: Digest, reading: Reading): Digest => ({
  count: digest.count + 1,
  total: digest.total + reading.value,
  high: Number.max(digest.high, reading.value),
})

function digest(readings: Chunk.Chunk<Reading>): Digest
function digest<E, R>(readings: Stream.Stream<Reading, E, R>): Effect.Effect<Digest, E, R>
function digest<E, R>(
  readings: Chunk.Chunk<Reading> | Stream.Stream<Reading, E, R>,
): Digest | Effect.Effect<Digest, E, R> {
  return Chunk.isChunk(readings)
    ? Chunk.reduce(readings, _SEED, _folded)
    : Stream.runFold(readings, _SEED, _folded)
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { digest }
export type { Digest, Reading }
```

## [03]-[STATEFUL_TRANSFORM]

State that threads element-to-element lives inside the pipeline as a fold accumulator, never beside it in a cell: the accumulator is an immutable value the step consumes and reconstructs, so the pipeline's state is recoverable from its declaration and invisible to every other fiber by construction.

[MEALY_ACCUM]:
- Law: `Stream.mapAccum(seed, (state, element) => [state, output])` is the Mealy machine — the emitted shape decouples from the carried state, the state never travels downstream, and a whole key space rides one immutable accumulator (`HashMap` in, `HashMap` out), so per-key memory is a fold fact, never a cell registry.
- Law: `Stream.scan(seed, step)` when every intermediate accumulator is itself the output — the running trace; it emits the seed first, so a consumer that must not see the origin states `Stream.drop(1)` at the pipe, never a patched step.
- Law: `Stream.mapAccumEffect` and `Stream.scanEffect` lift the same step onto the rail when it reads capability or fails — state threading is unchanged; the signature move is `readonly [S, A2]` becoming `Effect<readonly [S, A2], E2, R2>`.
- Reject: a `Ref` mutated inside `Stream.map` or `Stream.tap` — state the pipeline cannot see; `Stream.runFold` where downstream still consumes — the fold terminates, the scan keeps flowing; an outer `let` written from `tap`.
- Boundary: cross-fiber shared state is `concurrency.md`'s `Ref`/`STM`; the accumulator here is pipeline-local and single-fiber by construction, so reaching for a cell inside a pipeline marks state that belongs to the fold.
- Boundary: the Mealy step's `(state, element) => [state, output]` shape is `computation.md`'s law — this page owns its lift into the stream and everything the carrier adds.

```typescript
import { HashMap, Number, Option, Stream } from "effect"

type Sample = { readonly key: string; readonly value: number }
type Motion = { readonly key: string; readonly shift: number; readonly arrival: boolean }

const _advanced = (
  last: HashMap.HashMap<string, number>,
  sample: Sample,
): readonly [HashMap.HashMap<string, number>, Motion] => [
  HashMap.set(last, sample.key, sample.value),
  Option.match(HashMap.get(last, sample.key), {
    onNone: (): Motion => ({ key: sample.key, shift: 0, arrival: true }),
    onSome: (prior): Motion => ({ key: sample.key, shift: sample.value - prior, arrival: false }),
  }),
]

const motions = <E, R>(samples: Stream.Stream<Sample, E, R>): Stream.Stream<Motion, E, R> =>
  Stream.mapAccum(samples, HashMap.empty<string, number>(), _advanced)

const crest = <E, R>(samples: Stream.Stream<Sample, E, R>): Stream.Stream<number, E, R> =>
  Stream.scan(samples, -Infinity, (high, sample) => Number.max(high, sample.value))

// --- [EXPORTS] --------------------------------------------------------------------------

export { crest, motions }
export type { Motion, Sample }
```

## [04]-[WINDOWING]

A window is a pair of policy values — a `Sink` deciding what closes a batch, a `Schedule` deciding when one may emit — so every batching behavior is declared at the pipe and none is a timer fiber racing a mutable buffer.

[WINDOW_POLICY]:
- Law: `Stream.groupedWithin(width, patience)` owns count-or-latency windows — whichever bound trips first flushes, so a lull never strands a partial page; bare `Stream.grouped(width)` is legal only where latency is provably irrelevant, because a slowed source stalls its tail forever.
- Law: `Stream.aggregateWithin(sink, schedule)` separates the two policies — the sink closes a batch by its own algebra, the schedule paces emission — and divides the pipeline into two asynchronous islands: upstream feeds the sink on one fiber while downstream pulls aggregates on another, so aggregation continues between pulls.
- Law: `Stream.transduce(sink)` re-applies the sink with no schedule — pure structural aggregation; `Stream.rechunk(n)` when only chunk size changes, not meaning.
- Law: cadence arrives as a composed `Schedule` value — `Schedule.fixed` for wall-clock cadence, `Schedule.spaced` for pause-after-emit; the policy algebra is `rails-and-effects.md`'s, consumed here as a value.

[WEIGHTED_SINK]:
- Law: `Sink.foldWeighted({ initial, maxCost, cost, body })` closes a window by budget — `cost` prices each element against the running state, so byte, row, and unit budgets replace count guesses over uneven payloads; the fold state is the batch itself and the budget is a parameter, never a constant inside the body.
- Law: `Sink.foldWeightedDecompose` when one element can exceed the budget — `decompose` splits it into simpler elements instead of emitting an over-budget batch, and an indivisible element crosses the threshold by the sink's own stated fallback.
- Use: `Sink.fold`/`Sink.foldUntil`/`Sink.collectAllN` for predicate and count closure; `Sink.zip` and `Sink.race` keep composed consumers one `Sink` value so the terminal `Stream.run(sink)` receives one consumer.
- Reject: count-only batching of variable-size payloads; a byte counter threaded through `mapAccum` to fake a weighted window; a flush raced against a hand timer.

```typescript
import { Chunk, type Duration, Function, Schedule, Sink, Stream } from "effect"

type Entry = { readonly key: string; readonly payload: string }
type Parcel = { readonly entries: Chunk.Chunk<Entry>; readonly weight: number }

const _EMPTY: Parcel = { entries: Chunk.empty(), weight: 0 }

const _packed = (budget: number): Sink.Sink<Parcel, Entry, Entry> =>
  Sink.foldWeighted({
    initial: _EMPTY,
    maxCost: budget,
    cost: (_, entry) => entry.payload.length,
    body: (parcel, entry) => ({
      entries: Chunk.append(parcel.entries, entry),
      weight: parcel.weight + entry.payload.length,
    }),
  })

const parcels: {
  (budget: number, cadence: Duration.DurationInput): <E, R>(
    entries: Stream.Stream<Entry, E, R>,
  ) => Stream.Stream<Parcel, E, R>
  <E, R>(
    entries: Stream.Stream<Entry, E, R>,
    budget: number,
    cadence: Duration.DurationInput,
  ): Stream.Stream<Parcel, E, R>
} = Function.dual(
  3,
  <E, R>(
    entries: Stream.Stream<Entry, E, R>,
    budget: number,
    cadence: Duration.DurationInput,
  ): Stream.Stream<Parcel, E, R> =>
    Stream.aggregateWithin(entries, _packed(budget), Schedule.fixed(cadence)),
)

// --- [EXPORTS] --------------------------------------------------------------------------

export { parcels }
export type { Entry, Parcel }
```

## [05]-[FAN_GEOMETRY]

Fan-out multiplies consumers of one pull; fan-in funds one consumer from many sources. Both are declared geometry — scoped effects, derived unions, keyed alignment — so a source never re-runs per consumer and a merge never hand-tags what the operator derives.

[FAN_OUT]:
- Law: a statically known consumer count is `Stream.broadcast(n, lag)` — a scoped `Effect` yielding a `TupleOf<N, Stream>` that destructures into named branches; every branch must be consumed, and the driver advances at most `lag` chunks ahead of the slowest consumer, so the lag is the declared coupling between branch speeds.
- Law: dynamically attaching consumers are `Stream.share({ capacity, replay, idleTimeToLive })` — one upstream run shared by late subscribers, `replay` handing newcomers the recent window, `idleTimeToLive` keeping the upstream warm across a subscriber gap.
- Law: keyed partition is `Stream.groupByKey(f, { bufferSize })` piped to `GroupBy.evaluate((key, run) => ...)` — every key gets its own sub-stream, groups run in parallel and merge nondeterministically, and an unconsumed group deadlocks the producer at `bufferSize`: evaluate every group or narrow with `GroupBy.filter` first.
- Law: a two-way predicate split is `Stream.partition(predicate, { bufferSize })` — a scoped pair `[excluded, satisfying]`, refinement-aware so the satisfying branch narrows its element type.
- Reject: running the source once per consumer; folding into a keyed map of arrays and re-streaming per key; `broadcast` where consumers attach after the fact — `share` owns that lifetime.

[FAN_IN]:
- Law: same-shape sources merge with `Stream.merge(that, { haltStrategy })` — termination is the explicit policy `"left" | "right" | "both" | "either"`, never the accidental default; many sources are `Stream.mergeAll(streams, { concurrency })`.
- Law: different-shape sources merge with `Stream.mergeWithTag(record, { concurrency })` — the emitted tagged union derives from the record's keys, so the record is the vocabulary table, a hand-written union or pre-merge `map`-tagging restates what the operator computes, and dispatch lands as one `Match.valueTags` record.
- Law: two feeds sorted by distinct keys align with `Stream.zipAllSortedByKeyWith({ other, onSelf, onOther, onBoth, order })` — a constant-space keyed join total over all three presence cases; the caller owes distinct sorted keys, the operator's named precondition, and `Stream.zipLatest` pairs by arrival time, not key — choosing it for keyed data is the named confusion.
- Boundary: `Match.valueTags` mechanics are `surfaces-and-dispatch.md`'s; `Order` instances are `values.md`'s — both compose here as values.

```typescript
import { GroupBy, Match, Order, Stream } from "effect"

type Sample = { readonly key: string; readonly value: number }
type Pulse = { readonly key: string; readonly beat: number }
type Ledger = { readonly observed: number; readonly expected: number }

const totals = <E, R>(
  samples: Stream.Stream<Sample, E, R>,
): Stream.Stream<readonly [string, number], E, R> =>
  samples.pipe(
    Stream.groupByKey((sample) => sample.key, { bufferSize: 64 }),
    GroupBy.evaluate((key, run) =>
      run.pipe(
        Stream.scan(0, (total, sample) => total + sample.value),
        Stream.map((total) => [key, total] as const),
      ),
    ),
  )

const fused = <E1, R1, E2, R2>(
  samples: Stream.Stream<Sample, E1, R1>,
  pulses: Stream.Stream<Pulse, E2, R2>,
): Stream.Stream<number, E1 | E2, R1 | R2> =>
  Stream.map(
    Stream.mergeWithTag({ pulse: pulses, sample: samples }, { concurrency: "unbounded" }),
    Match.valueTags({
      pulse: ({ value }) => value.beat,
      sample: ({ value }) => value.value,
    }),
  )

const reconciled = <E1, R1, E2, R2>(
  observed: Stream.Stream<readonly [string, number], E1, R1>,
  expected: Stream.Stream<readonly [string, number], E2, R2>,
): Stream.Stream<[string, Ledger], E1 | E2, R1 | R2> =>
  Stream.zipAllSortedByKeyWith(observed, {
    other: expected,
    onSelf: (only): Ledger => ({ observed: only, expected: 0 }),
    onOther: (only): Ledger => ({ observed: 0, expected: only }),
    onBoth: (seen, owed): Ledger => ({ observed: seen, expected: owed }),
    order: Order.string,
  })

// --- [EXPORTS] --------------------------------------------------------------------------

export { fused, reconciled, totals }
export type { Ledger, Pulse, Sample }
```

## [06]-[INGRESS_BACKPRESSURE]

The seam between a push world and pull geometry is a bridge with an explicit buffer policy: the subscription is a scoped acquisition whose teardown rides the `Scope`, and every rate decision — suspend, shed, shape — is a typed value at the pipe, never an implicit unbounded queue.

[SCOPED_BRIDGE]:
- Law: a callback provider becomes a stream through `Stream.asyncScoped(register, policy)` — the registration is a scoped acquisition, so `Effect.acquireRelease` inside owns subscribe and unsubscribe as one bracket and teardown fires on completion, failure, and interruption alike; `emit.single`/`emit.chunk`/`emit.fail`/`emit.end` are the only crossings, and the listener maps provider faults into the tagged family before emitting, so the error channel stays typed at the seam.
- Law: the bridge family discriminates on registration lifetime — `Stream.asyncScoped` when registration acquires, `Stream.asyncPush` when emission throughput dominates (its emit ops return `boolean`, not `Promise`), `Stream.asyncEffect` only when registration acquires nothing.
- Law: a queue or pubsub handoff is `Stream.fromQueue(dequeue, { shutdown: true })` or `Stream.fromPubSub` — `shutdown: true` couples queue shutdown to stream end so the producer's close is the consumer's completion; the `Queue` itself is `concurrency.md`'s owner, arriving here as a value, and a cell's `SubscriptionRef.changes` arrives as the same already-typed feed.
- Exemption: the provider's push callbacks are the platform-forced statement seam — emissions are `void`-discarded inside the listener, the one place the pipeline cannot be an expression.
- Reject: a listener pushing into an outer array; `asyncEffect` where the registration subscribes — teardown would have no owner; an unsubscribe deferred to a consumer's cleanup instead of the bridge's own bracket.

[FLOW_POLICY]:
- Law: buffer policy is one typed value — `{ capacity, strategy: "suspend" | "dropping" | "sliding" }` read as a decision table: `"suspend"` backpressures the producer, `"dropping"` sheds the newest, `"sliding"` sheds the oldest — and `{ capacity: "unbounded" }` is a stated decision, never a default; `Stream.buffer` also decouples producer and consumer onto separate fibers, so inserting it is a concurrency decision, not a cache.
- Law: rate shaping is `Stream.throttle({ cost, units, duration, burst, strategy })` — a declared token bucket where `"shape"` delays and `"enforce"` drops, `cost` prices a whole chunk (`Chunk.size` for per-element pricing), and `burst` prices the allowance above steady state; `Stream.debounce(duration)` owns quiescence — emit only after input pauses.
- Reject: a hand token bucket around `mapEffect`; sleep-loop pacing; shedding via a mutable counter inside `filter`.

```typescript
import { Chunk, Data, type Duration, Effect, Stream } from "effect"

class FeedFault extends Data.TaggedError("FeedFault")<{ readonly reason: string }> {}

type Feed<A> = {
  readonly subscribe: (push: (value: A) => void, stop: (reason: string) => void) => () => void
}
type Tempo = { readonly intake: number; readonly lag: number; readonly spare: Duration.DurationInput }

const tempered = <A>(feed: Feed<A>, tempo: Tempo): Stream.Stream<A, FeedFault> =>
  Stream.asyncScoped<A, FeedFault>(
    (emit) =>
      Effect.acquireRelease(
        Effect.sync(() =>
          feed.subscribe(
            (value) => void emit.single(value),
            (reason) => void emit.fail(new FeedFault({ reason })),
          ),
        ),
        (unsubscribe) => Effect.sync(unsubscribe),
      ),
    { bufferSize: tempo.intake, strategy: "sliding" },
  ).pipe(
    Stream.buffer({ capacity: tempo.lag, strategy: "suspend" }),
    Stream.throttle({ cost: Chunk.size, units: tempo.lag, duration: tempo.spare, strategy: "shape" }),
  )

// --- [EXPORTS] --------------------------------------------------------------------------

export { FeedFault, tempered }
export type { Feed, Tempo }
```

## [07]-[BATCHING]

N identical lookups inside one flow are one declared request family and one resolver: call sites stay singular, the collapse is structural, and no `getMany` twin, dedup map, or hand-assembled page query survives beside the family.

[REQUEST_FAMILY]:
- Law: the lookup is a class extending `Request.TaggedClass("<tag>")<Success, Error, Fields>` — one name serving value, type, constructor, and identity: structural `Equal` over the fields is what deduplicates two requests for the same key, so the fields carry exactly the identity, and success and failure types are declared once at the family, never re-stated at call sites.
- Law: the class owner absorbs its derivations as statics — the resolver factory and the windowed consumer live on the request class, so the module exports one name and the family cannot scatter.
- Law: one `RequestResolver.makeBatched((requests) => ...)` receives the whole window as a `NonEmptyArray` and must settle every request — `Request.completeEffect` per hit, `Request.fail` per miss, and a provider-level fault fans out to every request in the window; an unsettled request suspends its caller forever, the resolver's stated contract.
- Reject: a `getMany` twin beside `get`; a hand `Map` of in-flight promises as a dedup cache; a resolver rebuilt per call site — identity instability defeats batching, so the resolver is built once and travels as a value.

[BATCH_GEOMETRY]:
- Law: call sites are `Effect.request(new <Req>({ ... }), resolver)` and the collapse is structural — `Effect.forEach(keys, ..., { batching: true })` funnels the whole traversal into one resolver window, while `Effect.withRequestBatching` and `Effect.withRequestCaching` scope the policy as rail transformers, caching deduplicating repeated keys across the flow.
- Law: in stream geometry the window is declared upstream — `Stream.groupedWithin(width, patience)` pages the key feed by count or latency, one batched `Effect.forEach` runs per page under `Stream.mapEffect` with `{ concurrency }` lanes, and `Stream.flattenIterables` restores element flow; width, patience, and lanes ride one `as const satisfies` policy row.
- Boundary: the resolver's provider call is `boundaries.md`'s seam — material is decoded before the resolver distributes it; this page owns only the collapse geometry.

```typescript
import { Array, Data, type Duration, Effect, HashMap, Option, Request, RequestResolver, Stream } from "effect"

type Shape = { readonly id: string; readonly rank: number }

class LookupFault extends Data.TaggedError("LookupFault")<{
  readonly id: string
  readonly reason: "absent" | "feed"
}> {}

type Page = (ids: ReadonlyArray<string>) => Effect.Effect<HashMap.HashMap<string, Shape>, LookupFault>

const _WINDOW = { width: 64, patience: "50 millis", lanes: 4 } as const satisfies {
  width: number
  patience: Duration.DurationInput
  lanes: number
}

class Lookup extends Request.TaggedClass("Lookup")<Shape, LookupFault, { readonly id: string }> {
  static readonly resolved = (page: Page): RequestResolver.RequestResolver<Lookup> =>
    RequestResolver.makeBatched((requests: Array.NonEmptyArray<Lookup>) =>
      page(Array.map(requests, (request) => request.id)).pipe(
        Effect.flatMap((rows) =>
          Effect.forEach(
            requests,
            (request) =>
              Request.completeEffect(
                request,
                Option.match(HashMap.get(rows, request.id), {
                  onNone: () => Effect.fail(new LookupFault({ id: request.id, reason: "absent" })),
                  onSome: Effect.succeed,
                }),
              ),
            { discard: true },
          ),
        ),
        Effect.catchAll(() =>
          Effect.forEach(
            requests,
            (request) => Request.fail(request, new LookupFault({ id: request.id, reason: "feed" })),
            { discard: true },
          ),
        ),
      ),
    )

  static readonly hydrated = <E, R>(
    ids: Stream.Stream<string, E, R>,
    resolver: RequestResolver.RequestResolver<Lookup>,
  ): Stream.Stream<Shape, E | LookupFault, R> =>
    ids.pipe(
      Stream.groupedWithin(_WINDOW.width, _WINDOW.patience),
      Stream.mapEffect(
        (batch) =>
          Effect.forEach(batch, (id) => Effect.request(new Lookup({ id }), resolver), {
            batching: true,
          }).pipe(Effect.withRequestCaching(true)),
        { concurrency: _WINDOW.lanes },
      ),
      Stream.flattenIterables,
    )
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Lookup, LookupFault }
export type { Page, Shape }
```
