# [CORE_FOLD]

`Fold` owns lawful keyed folds, ordinal D2 replay, full `Fold.AsOf` trace coordinates, and event-time windows.

## [01]-[INDEX]

- [02]-[PLAN_CONTRACT]: the plan shape, delta currency, change row, and the pure/stream folds; `Fold`.
- [03]-[TIME_COORDINATE]: full HLC-plus-ordinal read coordinates and ordinal-only D2 time; `Fold.AsOf`.
- [04]-[MEMORY_LANE]: the in-memory and ordered handles plus the lens reads over every handle; `Replay.memory/.ordered/.view/.feed`.
- [05]-[DATAFLOW_VERBS]: joins, semijoins, rollups, bounded boards, and fixpoints; `Replay`.
- [07]-[WATERMARK_PANES]: event-time completeness, the lateness verdict, and window policy folds; `Window`.

## [02]-[PLAN_CONTRACT]

[PLAN_CONTRACT]:
- Growth: a new fold is one `Fold.Plan` value binding an existing merge instance; a consumer binds that value instead of re-declaring the fold.
- Packages: `@electric-sql/d2mini`, `@electric-sql/d2ts`; `effect`; `../value/clock.ts` (`Clock`); `../value/fault.ts` (`Fault`); `./causal.ts` (`Causal`); `./merge.ts` (`Merge`).

```typescript
import * as Mini from "@electric-sql/d2mini"
import * as Diff from "@electric-sql/d2ts"
import {
  Array, Chunk, Data, Duration, Effect, Either, Equal, Equivalence, HashMap, HashSet, Match, Option, Order, type ParseResult, pipe,
  Predicate, Record, Ref, Schema, SortedMap, Stream, Subscribable, SubscriptionRef,
} from "effect"
import { Clock } from "../value/clock.ts"
import { Fault } from "../value/fault.ts"
import { Causal } from "./causal.ts"
import { Merge } from "./merge.ts"

declare namespace Fold {
  type CellPart = Schema.Schema.Type<typeof _CellPart>
  type Cell = Schema.Schema.Type<typeof _Cell>
  type Plan<Op, K, S> = {
    readonly name: string
    readonly key: (op: Op) => K
    readonly cell: (key: K) => Cell
    readonly keyAlike: Equivalence.Equivalence<K>
    readonly lift: (op: Op) => S
    readonly merge: Merge.Instance<S>
    readonly identity: Option.Option<(op: Op) => Cell>
  }
  type Multiplicity = Schema.Schema.Type<typeof _Multiplicity>
  type Delta<A> = ReadonlyArray<readonly [A, Multiplicity]>
  type Change<K, S> = { readonly key: K; readonly cell: Cell; readonly state: Option.Option<S> }
  type Table<K, S> = HashMap.HashMap<K, S>
  type Step<Op, K, S> = (table: Table<K, S>, op: Op) => readonly [Table<K, S>, Change<K, S>]
  type Shape = {
    readonly Fault: typeof ReplayFault
    readonly Cell: typeof _Cell
    readonly CellPart: typeof _CellPart
    readonly cell: (parts: Array.NonEmptyReadonlyArray<CellPart>) => Cell
    readonly step: <Op, K, S>(plan: Plan<Op, K, S>) => Step<Op, K, S>
    readonly trace: <Op, K, S>(
      plan: Plan<Op, K, S>,
    ) => <E, R>(ops: Stream.Stream<Op, E, R>) => Stream.Stream<Change<K, S>, E, R>
    readonly run: {
      <Op, K, S>(plan: Plan<Op, K, S>, ops: ReadonlyArray<Op>): Table<K, S>
      <Op, K, S, E, R>(plan: Plan<Op, K, S>, ops: Stream.Stream<Op, E, R>): Effect.Effect<Table<K, S>, E, R>
    }
    readonly AsOf: typeof AsOf
    readonly Replay: Replay.Shape
    readonly Window: Window.Shape
  }
}

const _Cell = Schema.NonEmptyString.pipe(Schema.brand("FoldCell"))
const _CellPart = Schema.Union(Schema.String, Schema.Finite, Schema.BigIntFromSelf)
const _Multiplicity = Schema.Int.pipe(Schema.filter((count) => count !== 0), Schema.brand("FoldMultiplicity"))
const _multiplicity = Schema.decodeSync(_Multiplicity)
const _cell = (parts: Array.NonEmptyReadonlyArray<Fold.CellPart>): Fold.Cell =>
  Schema.decodeSync(_Cell)(Array.join(Array.map(parts, (part) => {
    const tag = typeof part === "bigint" ? "i" : typeof part === "number" ? "n" : "s"
    const value = typeof part === "number" && Object.is(part, -0) ? "-0" : String(part)
    return `${tag}${value.length}:${value}`
  }), ""))

const _absorb = <Op, K, S>(plan: Fold.Plan<Op, K, S>) => (table: Fold.Table<K, S>, op: Op): Fold.Table<K, S> => {
  const key = plan.key(op)
  const lifted = plan.lift(op)
  return HashMap.modifyAt(table, key, (slot) =>
    Option.some(Option.match(slot, {
      onNone: () => lifted,
      onSome: (held) => plan.merge.combine.combine(held, lifted),
    })))
}

const _step = <Op, K, S>(plan: Fold.Plan<Op, K, S>): Fold.Step<Op, K, S> => (table, op) => {
  const key = plan.key(op)
  const cell = plan.cell(key)
  const lifted = plan.lift(op)
  const next = HashMap.modifyAt(table, key, (slot) =>
    Option.some(Option.match(slot, {
      onNone: () => lifted,
      onSome: (held) => plan.merge.combine.combine(held, lifted),
    })))
  return [next, { key, cell, state: HashMap.get(next, key) }] as const
}

const _trace = <Op, K, S>(plan: Fold.Plan<Op, K, S>) =>
<E, R>(ops: Stream.Stream<Op, E, R>): Stream.Stream<Fold.Change<K, S>, E, R> =>
  Stream.mapAccum(ops, HashMap.empty<K, S>(), _step(plan))

const _isBatch = <Op, E, R>(ops: ReadonlyArray<Op> | Stream.Stream<Op, E, R>): ops is ReadonlyArray<Op> =>
  globalThis.Array.isArray(ops)

function run<Op, K, S>(plan: Fold.Plan<Op, K, S>, ops: ReadonlyArray<Op>): Fold.Table<K, S>
function run<Op, K, S, E, R>(
  plan: Fold.Plan<Op, K, S>,
  ops: Stream.Stream<Op, E, R>,
): Effect.Effect<Fold.Table<K, S>, E, R>
function run<Op, K, S, E, R>(
  plan: Fold.Plan<Op, K, S>,
  ops: ReadonlyArray<Op> | Stream.Stream<Op, E, R>,
): Fold.Table<K, S> | Effect.Effect<Fold.Table<K, S>, E, R> {
  return _isBatch(ops)
    ? Array.reduce(ops, HashMap.empty<K, S>(), _absorb(plan))
    : Stream.runFold(ops, HashMap.empty<K, S>(), _absorb(plan))
}

const _Fold = { Cell: _Cell, CellPart: _CellPart, cell: _cell, step: _step, trace: _trace, run } as const
```

## [03]-[TIME_COORDINATE]

[TIME_COORDINATE]:
- Law: `Clock.Hlc` remains full-width; only the admitted non-negative ordinal projects to D2's one-dimensional safe-integer time.
- Law: the trace records the full `AsOf` beside each ordinal, so history never reconstructs or narrows an HLC from an engine coordinate.
- Law: `AsOf.covers` orders one journal lane by ordinal; `AsOf.Order` retains the full stamp-plus-ordinal total order for presentation and tokens.
- Growth: a branch axis is a key-space partition (a plan key row), never a fourth coordinate.
- Boundary: the data branch mints `AsOf` values from journal positions; history scrubbing consumes `read`/`diff` through served views.

```typescript
const _Sequence = Schema.BigIntFromSelf.pipe(Schema.nonNegativeBigInt(), Schema.brand("Sequence"))
const _sequence = Schema.decodeSync(_Sequence)
const _WINDOW = 9007199254740991n

class AsOf extends Schema.Class<AsOf>("AsOf")(
  Schema.Struct({
    stamp: Clock.Hlc,
    ordinal: _Sequence,
  }).pipe(Schema.filter((asOf) => asOf.ordinal <= _WINDOW)),
) {
  static readonly alike = Schema.equivalence(AsOf)
  static readonly Order: Order.Order<AsOf> = Order.combine(
    Order.mapInput(Clock.Hlc.Order, (asOf: AsOf) => asOf.stamp),
    Order.mapInput(Order.bigint, (asOf: AsOf) => asOf.ordinal),
  )
  static readonly genesis: AsOf = new AsOf({ stamp: Clock.Hlc.genesis, ordinal: _sequence(0n) })
  static at(stamp: Clock.Hlc, sequence: bigint): AsOf {
    return new AsOf({ stamp, ordinal: _sequence(sequence) })
  }
  static time(asOf: AsOf): AsOf.Time {
    return [Number(asOf.ordinal)] as const
  }
  static covers(upper: AsOf, lower: AsOf): boolean {
    return lower.ordinal <= upper.ordinal
  }
}

declare namespace AsOf {
  type Time = readonly [ordinal: number]
}
```

## [04]-[MEMORY_LANE]

[MEMORY_LANE]:
- Law: `ReplayFault` carries one `Fault.Class.family` issue, so each reason grades and renders its own subject instead of sharing one class word.
- Law: four legs partition the refusals — `admission` for offered material, `coordinate` for a stamp against the frontier, `window` for a band read, `trace` for the retained index.
- Law: a window refusal names the sealed-to-floor band it fell outside, so the reader learns which bound to move.
- Growth: a new lens modality is one overload line plus one projection arm.
- Growth: a new refusal is one `Fault.Class.row` on the replay family; its subject and renderer land with it.

```typescript
const _OrderLens = Schema.Struct({
  limit: Schema.optional(Schema.Int.pipe(Schema.positive())),
  offset: Schema.optional(Schema.Int.pipe(Schema.nonNegative())),
})

declare namespace Replay {
  type Fault = ReplayFault
  type Snapshot<K, S> = {
    readonly table: Fold.Table<K, S>
    readonly wave: Chunk.Chunk<Fold.Change<K, S>>
    readonly sealed: Option.Option<AsOf>
    readonly floor: Option.Option<AsOf>
    readonly coordinates: HashMap.HashMap<number, AsOf>
  }
  type Handle<Push, K, S> = {
    readonly push: Push
    readonly state: Subscribable.Subscribable<Fold.Table<K, S>>
  }
  type Live<Push, K, S> = Handle<Push, K, S> & {
    readonly wave: Subscribable.Subscribable<Chunk.Chunk<Fold.Change<K, S>>>
  }
  type Memory<Op, K, S> = Live<(delta: Fold.Delta<Op>) => Effect.Effect<void, ReplayFault>, K, S>
  type OrderLens = Schema.Schema.Type<typeof _OrderLens>
  type Ordered<Op, K, S> = {
    readonly push: (delta: Fold.Delta<Op>) => Effect.Effect<void, ReplayFault>
    readonly ranks: Subscribable.Subscribable<Chunk.Chunk<readonly [K, S]>>
  }
  type JoinKind = "inner" | "left" | "right" | "full" | "anti"
  type Trace<K, S> = {
    readonly cell: (key: K) => Fold.Cell
    readonly absorb: (
      version: Diff.Version,
      rows: ReadonlyArray<readonly [Diff.KeyValue<Fold.Cell, readonly [K, S]>, number]>,
    ) => void
    readonly compact: (upTo: AsOf) => void
    readonly diff: (after: AsOf, upTo: AsOf) => Effect.Effect<Fold.Delta<readonly [K, S]>>
    readonly versions: (key: K) => ReadonlyArray<number>
    readonly read: (upTo: AsOf) => Effect.Effect<Fold.Table<K, S>, ReplayFault>
  }
  type Clocked<K, S> = {
    readonly compact: (upTo: AsOf) => Effect.Effect<void, ReplayFault>
    readonly diff: (after: AsOf, upTo: AsOf) => Effect.Effect<Fold.Delta<readonly [K, S]>, ReplayFault>
    readonly frontier: Subscribable.Subscribable<Option.Option<AsOf>>
    readonly history: (key: K) => Effect.Effect<ReadonlyArray<AsOf>>
    readonly read: (upTo: AsOf) => Effect.Effect<Fold.Table<K, S>, ReplayFault>
    readonly seal: (frontier: AsOf) => Effect.Effect<void, ReplayFault>
  }
  type Timed<Push, K, S> = Handle<Push, K, S> & Clocked<K, S>
  type At<Push> = (at: AsOf, delta: Push) => Effect.Effect<void, ReplayFault>
  type Joined<OpL, OpR, K, P> = Handle<(delta: Fold.Delta<Input<OpL, OpR>>) => Effect.Effect<void, ReplayFault>, K, P>
  type Matched<Op, K, S> = Handle<(delta: Fold.Delta<Input<Op, K>>) => Effect.Effect<void, ReplayFault>, K, S>
  type Agg<Op> =
    | { readonly kind: "count" }
    | { readonly kind: "avg" | "max" | "median" | "min" | "mode" | "sum"; readonly of: (op: Op) => number }
  type Rollup<Aggs> = { readonly [Column in keyof Aggs]: number }
  type AggregateOperator<Op> =
    | ReturnType<typeof Mini.groupByOperators.count<Op>>
    | ReturnType<typeof Mini.groupByOperators.avg<Op>>
    | ReturnType<typeof Mini.groupByOperators.max<Op>>
    | ReturnType<typeof Mini.groupByOperators.median<Op>>
    | ReturnType<typeof Mini.groupByOperators.min<Op>>
    | ReturnType<typeof Mini.groupByOperators.mode<Op>>
    | ReturnType<typeof Mini.groupByOperators.sum<Op>>
  type Grouped<Op, By, Aggs> = Handle<
    (delta: Fold.Delta<Op>) => Effect.Effect<void, ReplayFault>,
    string,
    By & Rollup<Aggs>
  > & {
    readonly name: string
  }
  type Topped<Op, K, S> = {
    readonly push: (delta: Fold.Delta<Op>) => Effect.Effect<void, ReplayFault>
    readonly boards: Subscribable.Subscribable<Fold.Table<Fold.Cell, Chunk.Chunk<readonly [K, S]>>>
  }
  type JoinedAt<OpL, OpR, K, P> = Timed<At<Fold.Delta<Input<OpL, OpR>>>, K, P>
  type GroupedAt<Op, By, Aggs> = Timed<At<Fold.Delta<Op>>, string, By & Rollup<Aggs>> & {
    readonly name: string
  }
  type DiffAggregateOperator<Op> =
    | ReturnType<typeof Diff.groupByOperators.count<Op>>
    | ReturnType<typeof Diff.groupByOperators.avg<Op>>
    | ReturnType<typeof Diff.groupByOperators.max<Op>>
    | ReturnType<typeof Diff.groupByOperators.median<Op>>
    | ReturnType<typeof Diff.groupByOperators.min<Op>>
    | ReturnType<typeof Diff.groupByOperators.mode<Op>>
    | ReturnType<typeof Diff.groupByOperators.sum<Op>>
  type Closure = {
    readonly push: (at: AsOf, edges: Fold.Delta<readonly [Fold.Cell, Fold.Cell]>) => Effect.Effect<void, ReplayFault>
    readonly seal: (frontier: AsOf) => Effect.Effect<void, ReplayFault>
    readonly reach: Subscribable.Subscribable<Fold.Table<Fold.Cell, HashSet.HashSet<Fold.Cell>>>
  }
  type Versioned<Op, K, S> = Timed<At<Fold.Delta<Op>>, K, S> & {
    readonly wave: Subscribable.Subscribable<Chunk.Chunk<Fold.Change<K, S>>>
  }
  type Shape = {
    readonly Fault: typeof ReplayFault
    readonly Input: typeof _Input
    readonly OrderLens: typeof _OrderLens
    readonly delta: <Op>(ops: ReadonlyArray<Op>) => Fold.Delta<Op>
    readonly memory: <Op, K, S>(plan: Fold.Plan<Op, K, S>) => Effect.Effect<Memory<Op, K, S>>
    readonly ordered: <Op, K, S>(
      plan: Fold.Plan<Op, K, S>,
      rank: Order.Order<S>,
      lens: OrderLens,
    ) => Effect.Effect<Ordered<Op, K, S>>
    readonly joined: {
      <OpL, OpR, K, SL, SR>(
        left: Fold.Plan<OpL, K, SL>,
        right: Fold.Plan<OpR, K, SR>,
      ): Effect.Effect<Joined<OpL, OpR, K, readonly [SL, SR]>>
      <OpL, OpR, K, SL, SR>(
        left: Fold.Plan<OpL, K, SL>,
        right: Fold.Plan<OpR, K, SR>,
        spec: { readonly kind: JoinKind },
      ): Effect.Effect<Joined<OpL, OpR, K, readonly [Option.Option<SL>, Option.Option<SR>]>>
      <OpL, OpR, K, SL, SR>(
        left: Fold.Plan<OpL, K, SL>,
        right: Fold.Plan<OpR, K, SR>,
        spec: { readonly origin: AsOf },
      ): Effect.Effect<JoinedAt<OpL, OpR, K, readonly [SL, SR]>>
      <OpL, OpR, K, SL, SR>(
        left: Fold.Plan<OpL, K, SL>,
        right: Fold.Plan<OpR, K, SR>,
        spec: { readonly kind: JoinKind; readonly origin: AsOf },
      ): Effect.Effect<JoinedAt<OpL, OpR, K, readonly [Option.Option<SL>, Option.Option<SR>]>>
    }
    readonly matched: <Op, K, S>(plan: Fold.Plan<Op, K, S>) => Effect.Effect<Matched<Op, K, S>>
    readonly grouped: {
      <Op, By extends Readonly<Record<string, boolean | number | string>>, Aggs extends Readonly<Record<string, Agg<Op>>>>(spec: {
        readonly name: string
        readonly by: (op: Op) => By
        readonly aggs: Aggs
      }): Effect.Effect<Grouped<Op, By, Aggs>>
      <Op, By extends Readonly<Record<string, boolean | number | string>>, Aggs extends Readonly<Record<string, Agg<Op>>>>(spec: {
        readonly name: string
        readonly by: (op: Op) => By
        readonly aggs: Aggs
        readonly origin: AsOf
      }): Effect.Effect<GroupedAt<Op, By, Aggs>>
    }
    readonly topped: <Op, K, S>(
      plan: Fold.Plan<Op, K, S>,
      spec: { readonly by: (key: K, state: S) => Fold.Cell; readonly rank: Order.Order<S>; readonly take: number },
    ) => Effect.Effect<Topped<Op, K, S>>
    readonly closure: (origin: AsOf) => Effect.Effect<Closure>
    readonly versioned: <Op, K, S>(plan: Fold.Plan<Op, K, S>, origin: AsOf) => Effect.Effect<Versioned<Op, K, S>>
    readonly view: {
      <Op, K, S>(handle: Memory<Op, K, S>): Subscribable.Subscribable<Fold.Table<K, S>>
      <Op, K, S>(handle: Versioned<Op, K, S>): Subscribable.Subscribable<Fold.Table<K, S>>
      <Op, K, S>(handle: Memory<Op, K, S> | Versioned<Op, K, S>, key: K): Subscribable.Subscribable<Option.Option<S>>
      <Op, K, S>(handle: Ordered<Op, K, S>): Subscribable.Subscribable<Chunk.Chunk<readonly [K, S]>>
    }
    readonly feed: <Op, K, S>(handle: Memory<Op, K, S> | Versioned<Op, K, S>) => Stream.Stream<Fold.Change<K, S>>
  }
}

interface ReplayInputDefinition extends Data.TaggedEnum.WithGenerics<2> {
  readonly taggedEnum: Replay.Input<this["A"], this["B"]>
}
const _Input = Data.taggedEnum<ReplayInputDefinition>()
declare namespace Replay {
  type Input<L, R> = Data.TaggedEnum<{
    Left: { readonly value: L }
    Right: { readonly value: R }
  }>
}

// Every window refusal names the BAND it fell outside, never the coordinate alone: a read before the seal and a read
// under the compaction floor are the same word only until an operator has to choose which bound to move.
const _Band = Schema.Struct({
  floor: Schema.OptionFromSelf(AsOf),
  sealed: Schema.OptionFromSelf(AsOf),
})
const _edge = (bound: Option.Option<AsOf>): string =>
  Option.match(bound, { onNone: () => "-", onSome: (held) => String(held.ordinal) })
const _band = (band: typeof _Band.Type): string => `${_edge(band.floor)}..${_edge(band.sealed)}`

// One family grades the whole refusal surface and each reason renders its OWN subject, so a torn retained index and a
// rejected order lens stop reading as one severity. Classes are elected per refusal: offered material the plan cannot
// replay is `invalid`, key parts no cell grammar admits are `malformed`, a coordinate a later seal or frontier read
// reopens is `conflicted`, history outside the sealed band is `absent`, and a reconstruction that yields two live rows
// for one cell is the one `breached` arm. `leg` partitions the page's own surfaces — `admission` refuses what a caller
// offered, `coordinate` refuses a stamp against the frontier, `window` refuses a band read, `trace` refuses the index.
const _replayFamily = Fault.Class.family(
  ["spec", "cell", "identity", "group", "push", "seal", "read", "diff", "compact", "invariant"] as const,
  {
    spec: Fault.Class.row({
      class: "invalid",
      leg: "admission",
      detail: Schema.Struct({ column: Schema.NonEmptyString }),
      render: ({ column }) => `handle spec column ${column} carries a value this lane's schema refuses`,
    }),
    cell: Fault.Class.row({
      class: "invalid",
      leg: "admission",
      detail: Schema.Struct({ cell: _Cell }),
      render: ({ cell }) => `cell ${cell} is claimed by two keys this plan does not equate`,
    }),
    identity: Fault.Class.row({
      class: "invalid",
      leg: "admission",
      detail: Schema.Struct({ plan: Schema.NonEmptyString, contribution: Schema.OptionFromSelf(_Cell) }),
      render: ({ contribution, plan }) =>
        Option.match(contribution, {
          onNone: () => `plan ${plan} merges non-idempotently and projects no identity, so no delta replays`,
          onSome: (cell) => `plan ${plan} restates identity ${cell} under a second key, state, or retraction`,
        }),
    }),
    group: Fault.Class.row({
      class: "malformed",
      leg: "admission",
      detail: Schema.Struct({ columns: Schema.NonEmptyArray(Schema.String) }),
      render: ({ columns }) => `group columns ${Array.join(columns, ",")} carry non-finite parts no cell admits`,
    }),
    push: Fault.Class.row({
      class: "conflicted",
      leg: "coordinate",
      detail: Schema.Struct({ at: AsOf }),
      render: ({ at }) => `push at ordinal ${at.ordinal} claims a coordinate this lane already settled`,
    }),
    seal: Fault.Class.row({
      class: "conflicted",
      leg: "coordinate",
      detail: Schema.Struct({ frontier: AsOf, sealed: AsOf }),
      render: ({ frontier, sealed }) => `seal at ordinal ${frontier.ordinal} retreats behind the sealed ${sealed.ordinal}`,
    }),
    read: Fault.Class.row({
      class: "absent",
      leg: "window",
      detail: Schema.Struct({ upTo: AsOf, ..._Band.fields }),
      render: (subject) => `read at ordinal ${subject.upTo.ordinal} is unanswerable inside the sealed band ${_band(subject)}`,
    }),
    diff: Fault.Class.row({
      class: "absent",
      leg: "window",
      detail: Schema.Struct({ after: AsOf, upTo: AsOf, ..._Band.fields }),
      render: (subject) =>
        `diff over ordinals ${subject.after.ordinal}..${subject.upTo.ordinal} is unanswerable inside the sealed band ${
          _band(subject)
        }`,
    }),
    compact: Fault.Class.row({
      class: "conflicted",
      leg: "window",
      detail: Schema.Struct({ upTo: AsOf, ..._Band.fields }),
      render: (subject) => `compaction to ordinal ${subject.upTo.ordinal} conflicts with the sealed band ${_band(subject)}`,
    }),
    invariant: Fault.Class.row({
      class: "breached",
      leg: "trace",
      detail: Schema.Struct({ cell: _Cell, survivors: Schema.Int.pipe(Schema.positive()) }),
      render: ({ cell, survivors }) => `cell ${cell} reconstructs ${survivors} live rows at one coordinate`,
    }),
  },
)

class ReplayFault extends Schema.TaggedError<ReplayFault>()("ReplayFault", {
  case: _replayFamily.payload,
}) {
  get class(): Fault.Class.Kind {
    return _replayFamily.classOf(this.case.reason)
  }
  get leg(): string {
    return _replayFamily.legOf(this.case.reason)
  }
  override get message(): string {
    return _replayFamily.render(this.case)
  }
}

const _admission = <Op, K, S>(plan: Fold.Plan<Op, K, S>) => {
  const held = new Map<Fold.Cell, K>()
  const witnesses = new Map<Fold.Cell, readonly [K, S]>()
  const counts = new Map<Fold.Cell, number>()
  return (delta: Fold.Delta<Op>): Effect.Effect<void, ReplayFault> => {
    const rows = Array.map(delta, ([op, count]) => {
      const key = plan.key(op)
      return [plan.cell(key), key, plan.lift(op), Option.map(plan.identity, (identity) => identity(op)), count] as const
    })
    if (!Merge.idempotent(plan.merge) && Option.isNone(plan.identity)) {
      return Effect.fail(new ReplayFault({
        case: { reason: "identity", plan: plan.name, contribution: Option.none() },
      }))
    }
    const stagedWitnesses = new Map(witnesses)
    const staged = new Map(counts)
    const collision = Array.findFirst(rows, ([cell, key, state, identity, count]) => {
      if (Option.exists(Option.fromNullable(held.get(cell)), (prior) => !plan.keyAlike(prior, key))) return true
      if (Option.isNone(identity)) return count < 0
      const contribution = identity.value
      const prior = stagedWitnesses.get(contribution)
      if (prior !== undefined && (!plan.keyAlike(prior[0], key) || !plan.merge.alike(prior[1], state))) return true
      if (prior === undefined) stagedWitnesses.set(contribution, [key, state])
      const next = (staged.get(contribution) ?? 0) + count
      if (!globalThis.Number.isSafeInteger(next) || next < 0) return true
      if (next === 0) staged.delete(contribution)
      else staged.set(contribution, next)
      return false
    })
    return Option.match(collision, {
      onNone: () => Effect.sync(() => {
        for (const [cell, key] of rows) held.set(cell, key)
        witnesses.clear()
        for (const [identity, row] of stagedWitnesses) witnesses.set(identity, row)
        counts.clear()
        for (const [identity, count] of staged) counts.set(identity, count)
      }),
      onSome: ([cell, , , identity, count]) => Effect.fail(new ReplayFault({
        case: Option.isSome(identity) || count < 0
          ? { reason: "identity", plan: plan.name, contribution: Option.some(Option.getOrElse(identity, () => cell)) }
          : { reason: "cell", cell },
      })),
    })
  }
}

const _unique = <A>(values: Array<[A, number]>): Array<[A, number]> =>
  Array.reduce(values, 0, (total, [, count]) => total + count) > 0
    ? Option.match(Array.findFirst(values, ([, count]) => count > 0), {
        onNone: () => [],
        onSome: ([value]) => [[value, 1]],
      })
    : []

const _scaled = <S>(instance: Merge.Instance<S>, state: S, count: number): Option.Option<S> => {
  const loop = (power: S, remaining: number, held: Option.Option<S>): Option.Option<S> =>
    remaining === 0
      ? held
      : loop(
          instance.combine.combine(power, power),
          Math.floor(remaining / 2),
          remaining % 2 === 0
            ? held
            : Option.match(held, { onNone: () => Option.some(power), onSome: (value) => Option.some(instance.combine.combine(value, power)) }),
        )
  return count <= 0 ? Option.none() : Merge.idempotent(instance) ? Option.some(state) : loop(state, count, Option.none())
}

const _reduced = <S>(instance: Merge.Instance<S>) => (values: Array<[S, number]>): Array<[S, number]> =>
  pipe(
    Array.filterMap(values, ([state, count]) => _scaled(instance, state, count)),
    (survivors) => Array.isNonEmptyReadonlyArray(survivors)
      ? [[instance.combine.combineMany(Array.headNonEmpty(survivors), Array.tailNonEmpty(survivors)), 1]]
      : [],
  )

const _patch = <K, S>(table: Fold.Table<K, S>, change: Fold.Change<K, S>): Fold.Table<K, S> =>
  Option.match(change.state, {
    onNone: () => HashMap.remove(table, change.key),
    onSome: (state) => HashMap.set(table, change.key, state),
  })

const _changes = <K, S>(
  rows: ReadonlyArray<readonly [readonly [K, S], number]>,
  cell: (key: K) => Fold.Cell,
): ReadonlyArray<Fold.Change<K, S>> => {
  const kept = Array.filterMap(rows, ([row, count]) => (count > 0 ? Option.some(row) : Option.none()))
  const dropped = Array.filterMap(rows, ([row, count]) => (count < 0 ? Option.some(row[0]) : Option.none()))
  return [
    ...Array.filterMap(dropped, (key) =>
      Array.some(kept, ([survivor]) => Equal.equals(survivor, key))
        ? Option.none()
        : Option.some<Fold.Change<K, S>>({ key, cell: cell(key), state: Option.none() })),
    ...Array.map(kept, ([key, state]): Fold.Change<K, S> => ({ key, cell: cell(key), state: Option.some(state) })),
  ]
}

const _rows = <A>(delta: Fold.Delta<A>): Array<[A, number]> => Array.map(delta, ([value, count]) => [value, count])

const _cellReduced = <K, S>(instance: Merge.Instance<S>) =>
(values: Array<[readonly [K, S], number]>): Array<[readonly [K, S], number]> => {
  const key = Array.findFirst(values, ([, count]) => count > 0)
  const state = _reduced(instance)(Array.map(values, ([[, value], count]) => [value, count]))
  return Option.match(Option.zip(key, Array.head(state)), {
    onNone: () => [],
    onSome: ([[[domain]], [folded]]) => [[[domain, folded] as const, 1]],
  })
}

const _domainRows = <K, S>(
  rows: ReadonlyArray<readonly [readonly [Fold.Cell, readonly [K, S]], number]>,
): ReadonlyArray<readonly [readonly [K, S], number]> =>
  Array.map(rows, ([[, domain], count]) => [domain, count] as const)

const _engine = <Row>(
  graph: { readonly finalize: () => void; readonly run: () => void },
  wire: (emit: (row: Row) => void) => void,
): Effect.Effect<{
  readonly drive: <A>(send: () => void, publish: (rows: ReadonlyArray<Row>) => Effect.Effect<A>) => Effect.Effect<A>
}> =>
  Effect.map(Effect.makeSemaphore(1), (gate) => {
    // BOUNDARY ADAPTER: the engine sink is a void callback and the drain is a statement seam; the pending buffer never escapes the closure
    const pending: Array<Row> = []
    wire((row) => pending.push(row))
    graph.finalize()
    return {
      drive: (send, publish) =>
        gate.withPermits(1)(Effect.flatMap(
          Effect.sync(() => {
            send()
            graph.run()
            return pending.splice(0, pending.length)
          }),
          publish,
        )),
    }
  })

// One versioned trace owner beside the graph scaffold: the engine `Index` a d2ts sink fills IS the retained history,
// so time travel, retention, the net-diff, and the per-key coordinate census are four reads over one structure and no
// lane rolls a slice log beside it. A versioned lane declares its operators and its publish fold; the whole coordinate
// surface arrives from here, which is why `versioned`, `joined`, `grouped`, and `topped` share one implementation of it.
const _trace = <K, S>(cell: (key: K) => Fold.Cell): Replay.Trace<K, S> => {
  const held = new Diff.Index<Fold.Cell, readonly [K, S]>()
  const at = (asOf: AsOf): Diff.Version => Diff.v([...AsOf.time(asOf)])
  const surviving = (key: Fold.Cell, upTo: AsOf): Effect.Effect<Option.Option<readonly [K, S]>, ReplayFault> => {
    const rows = pipe(
      new Diff.MultiSet(held.reconstructAt(key, at(upTo))).consolidate().getInner(),
      Array.filterMap(([state, count]) => (count > 0 ? Option.some(state) : Option.none())),
    )
    return rows.length <= 1
      ? Effect.succeed(Array.head(rows))
      : Effect.fail(new ReplayFault({ case: { reason: "invariant", cell: key, survivors: rows.length } }))
  }
  return {
    cell,
    absorb: (version, rows) => {
      // BOUNDARY ADAPTER: the engine sink is a void callback; the append rides the same synchronous drain the graph runs
      for (const [[key, state], count] of rows) held.addValue(key, version, [state, count])
    },
    compact: (upTo) => held.compact(new Diff.Antichain([at(upTo)])),
    diff: (after, upTo) =>
      Effect.sync(() =>
        Array.flatMap(held.keys(), (key) =>
          pipe(
            new Diff.MultiSet(held.reconstructAt(key, at(after)))
              .negate()
              .concat(new Diff.MultiSet(held.reconstructAt(key, at(upTo))))
              .consolidate()
              .getInner(),
            Array.map(([[domain, state], count]) => [[domain, state] as const, _multiplicity(count)] as const),
          ))),
    versions: (key) => Array.map(held.versions(cell(key)), (version) => version.getInner()[0]),
    read: (upTo) => Effect.map(
      Effect.forEach(held.keys(), (key) => surviving(key, upTo)),
      (rows) => Array.reduce(rows, HashMap.empty<K, S>(), (table, row) =>
        Option.match(row, {
          onNone: () => table,
          onSome: ([domain, state]) => HashMap.set(table, domain, state),
        })),
    ),
  }
}

const _sunk = <K, S>(trace: Replay.Trace<K, S>, emit: (change: Fold.Change<K, S>) => void) =>
(message: Diff.Message<Diff.KeyValue<Fold.Cell, readonly [K, S]>>): void => {
  if (message.type === Diff.MessageType.DATA) {
    const rows = message.data.collection.getInner()
    trace.absorb(message.data.version, rows)
    _changes(_domainRows(rows), trace.cell).forEach(emit)
  }
}

const _published = <K, S>(
  live: SubscriptionRef.SubscriptionRef<Replay.Snapshot<K, S>>,
) =>
(drained: ReadonlyArray<Fold.Change<K, S>>, sealed = Option.none<AsOf>(), at = Option.none<AsOf>()): Effect.Effect<void> =>
  Ref.update(live, (held) => ({
    ...held,
    table: Array.reduce(drained, held.table, _patch),
    wave: Chunk.fromIterable(drained),
    sealed: Option.orElse(sealed, () => held.sealed),
    coordinates: Option.match(at, {
      onNone: () => held.coordinates,
      onSome: (coordinate) => HashMap.set(held.coordinates, AsOf.time(coordinate)[0], coordinate),
    }),
  }))

// `seal` is the only frontier writer and it writes what it SENT, so the watermark read is exact rather than parsed back
// out of engine messages; `compact` rides the same permit because the trace and the drain share one JS thread.
const _timed = <K, S>(
  trace: Replay.Trace<K, S>,
  drive: <A>(send: () => void, publish: (rows: ReadonlyArray<Fold.Change<K, S>>) => Effect.Effect<A>) => Effect.Effect<A>,
  advance: (frontier: Diff.Version) => void,
  publish: (
    drained: ReadonlyArray<Fold.Change<K, S>>,
    sealed?: Option.Option<AsOf>,
    at?: Option.Option<AsOf>,
  ) => Effect.Effect<void>,
  live: SubscriptionRef.SubscriptionRef<Replay.Snapshot<K, S>>,
): Replay.Clocked<K, S> => ({
  compact: (upTo) => Effect.flatMap(Ref.get(live), (held) =>
    Option.exists(held.sealed, (sealed) => AsOf.covers(sealed, upTo))
      && !Option.exists(held.floor, (floor) => !AsOf.covers(upTo, floor))
      ? drive(
          () => trace.compact(upTo),
          () => Ref.update(live, (state) => ({
            ...state,
            floor: Option.some(upTo),
            coordinates: HashMap.filter(state.coordinates, (_coordinate, ordinal) => ordinal >= Number(upTo.ordinal)),
          })),
        )
      : Effect.fail(new ReplayFault({
          case: { reason: "compact", upTo, floor: held.floor, sealed: held.sealed },
        }))),
  diff: (after, upTo) => Effect.flatMap(Ref.get(live), (held) =>
    AsOf.covers(upTo, after)
      && Option.exists(held.sealed, (sealed) => AsOf.covers(sealed, upTo))
      && !Option.exists(held.floor, (floor) => !AsOf.covers(after, floor))
      ? trace.diff(after, upTo)
      : Effect.fail(new ReplayFault({
          case: { reason: "diff", after, upTo, floor: held.floor, sealed: held.sealed },
        }))),
  frontier: Subscribable.map(live, (held) => held.sealed),
  history: (key) => Effect.map(Ref.get(live), (held) =>
    Array.filterMap(trace.versions(key), (ordinal) => HashMap.get(held.coordinates, ordinal))),
  read: (upTo) => Effect.flatMap(Ref.get(live), (held) =>
    !Option.exists(held.sealed, (sealed) => AsOf.covers(sealed, upTo))
      || Option.exists(held.floor, (floor) => !AsOf.covers(upTo, floor))
      ? Effect.fail(new ReplayFault({ case: { reason: "read", upTo, floor: held.floor, sealed: held.sealed } }))
      : trace.read(upTo)),
  // Filtering to the OFFENDING seal binds the frontier the refusal names, so the raise reads both coordinates rather
  // than asserting a retreat it never held.
  seal: (frontier) =>
    Effect.flatMap(Ref.get(live), (held) =>
      Option.match(Option.filter(held.sealed, (sealed) => !AsOf.covers(frontier, sealed)), {
        onNone: () => drive(
          () => advance(Diff.v([...AsOf.time(frontier)])),
          (drained) => publish(drained, Option.some(frontier), Option.some(frontier)),
        ),
        onSome: (sealed) => Effect.fail(new ReplayFault({ case: { reason: "seal", frontier, sealed } })),
      })),
})

const _pushAt = <K, S, A>(
  live: SubscriptionRef.SubscriptionRef<Replay.Snapshot<K, S>>,
  at: AsOf,
  push: Effect.Effect<A, ReplayFault>,
): Effect.Effect<A, ReplayFault> =>
  Effect.flatMap(Ref.get(live), (held) =>
    Option.exists(held.sealed, (sealed) => at.ordinal <= sealed.ordinal)
      || Option.exists(HashMap.get(held.coordinates, AsOf.time(at)[0]), (prior) => !AsOf.alike(prior, at))
      ? Effect.fail(new ReplayFault({ case: { reason: "push", at } }))
      : push)

const _keyed = <Op, K, S>(graph: Mini.D2, plan: Fold.Plan<Op, K, S>) => {
  const input = graph.newInput<Op>()
  const staged = Option.match(plan.identity, {
    onNone: () => input.pipe(
      Mini.map((op: Op): Mini.KeyValue<Fold.Cell, readonly [K, S]> => {
        const key = plan.key(op)
        return [plan.cell(key), [key, plan.lift(op)] as const]
      }),
      Mini.reduce(_cellReduced(plan.merge)),
    ),
    onSome: (identity) => input.pipe(
      Mini.map((op: Op): Mini.KeyValue<Fold.Cell, readonly [Fold.Cell, K, S]> => {
        const key = plan.key(op)
        return [identity(op), [plan.cell(key), key, plan.lift(op)] as const]
      }),
      Mini.reduce(_unique<readonly [Fold.Cell, K, S]>),
      Mini.map(([, [domain, key, state]]): Mini.KeyValue<Fold.Cell, readonly [K, S]> => [domain, [key, state] as const]),
      Mini.reduce(_cellReduced(plan.merge)),
    ),
  })
  return {
    input,
    admit: _admission(plan),
    staged,
  }
}

// The same keyed staging on the versioned engine, so a plan folds identically at both altitudes and only the operator
// namespace differs — the reducer, the key projection, and the lift are one declaration read twice.
const _versionedKeyed = <Op, K, S>(graph: Diff.D2, plan: Fold.Plan<Op, K, S>) => {
  const input = graph.newInput<Op>()
  const staged = Option.match(plan.identity, {
    onNone: () => input.pipe(
      Diff.map((op: Op): Diff.KeyValue<Fold.Cell, readonly [K, S]> => {
        const key = plan.key(op)
        return [plan.cell(key), [key, plan.lift(op)] as const]
      }),
      Diff.reduce<Fold.Cell, readonly [K, S], readonly [K, S], Diff.KeyValue<Fold.Cell, readonly [K, S]>>(_cellReduced(plan.merge)),
    ),
    onSome: (identity) => input.pipe(
      Diff.map((op: Op): Diff.KeyValue<Fold.Cell, readonly [Fold.Cell, K, S]> => {
        const key = plan.key(op)
        return [identity(op), [plan.cell(key), key, plan.lift(op)] as const]
      }),
      Diff.reduce<
        Fold.Cell,
        readonly [Fold.Cell, K, S],
        readonly [Fold.Cell, K, S],
        Diff.KeyValue<Fold.Cell, readonly [Fold.Cell, K, S]>
      >(_unique<readonly [Fold.Cell, K, S]>),
      Diff.map(([, [domain, key, state]]): Diff.KeyValue<Fold.Cell, readonly [K, S]> => [domain, [key, state] as const]),
      Diff.reduce<Fold.Cell, readonly [K, S], readonly [K, S], Diff.KeyValue<Fold.Cell, readonly [K, S]>>(_cellReduced(plan.merge)),
    ),
  })
  return {
    input,
    admit: _admission(plan),
    staged,
  }
}

const _live = <K, S>(origin = Option.none<AsOf>()): Effect.Effect<SubscriptionRef.SubscriptionRef<Replay.Snapshot<K, S>>> =>
  SubscriptionRef.make({
    table: HashMap.empty<K, S>(),
    wave: Chunk.empty<Fold.Change<K, S>>(),
    sealed: origin,
    floor: Option.none<AsOf>(),
    coordinates: Option.match(origin, {
      onNone: () => HashMap.empty<number, AsOf>(),
      onSome: (asOf) => HashMap.make([AsOf.time(asOf)[0], asOf]),
    }),
  })

const _memory = <Op, K, S>(plan: Fold.Plan<Op, K, S>): Effect.Effect<Replay.Memory<Op, K, S>> =>
  Effect.gen(function* () {
    const graph = new Mini.D2()
    const keyed = _keyed(graph, plan)
    const engine = yield* _engine<Fold.Change<K, S>>(graph, (emit) =>
      keyed.staged.pipe(
        Mini.consolidate(),
        Mini.output((delta: Mini.MultiSet<Mini.KeyValue<Fold.Cell, readonly [K, S]>>) =>
          _changes(_domainRows(delta.getInner()), plan.cell).forEach(emit)),
      ))
    const live = yield* _live<K, S>()
    const publish = _published(live)
    return {
      push: (delta) =>
        Effect.zipRight(
          keyed.admit(delta),
          engine.drive(
            () => keyed.input.sendData(new Mini.MultiSet(_rows(delta))),
            publish,
          ),
        ),
      state: Subscribable.map(live, (held) => held.table),
      wave: Subscribable.map(live, (held) => held.wave),
    }
  })

const _ORDERED = "ordered" as const

const _ordered = <Op, K, S>(
  plan: Fold.Plan<Op, K, S>,
  rank: Order.Order<S>,
  lens: Replay.OrderLens,
): Effect.Effect<Replay.Ordered<Op, K, S>> =>
  Effect.gen(function* () {
    const admitted = yield* Schema.decode(_OrderLens)(lens).pipe(
      Effect.mapError(() => new ReplayFault({ case: { reason: "spec", column: "lens" } })),
    )
    const graph = new Mini.D2()
    const keyed = _keyed(graph, plan)
    const total = Order.combine(
      Order.mapInput(rank, (row: readonly [K, S]) => row[1]),
      Order.mapInput(Order.string, (row: readonly [K, S]) => plan.cell(row[0])),
    )
    const engine = yield* _engine<readonly [Mini.KeyValue<typeof _ORDERED, readonly [readonly [K, S], string]>, number]>(graph, (emit) =>
      keyed.staged.pipe(
        Mini.map(([, domain]: Mini.KeyValue<Fold.Cell, readonly [K, S]>): Mini.KeyValue<typeof _ORDERED, readonly [K, S]> =>
          [_ORDERED, domain]),
        Mini.orderByWithFractionalIndex<Mini.KeyValue<typeof _ORDERED, readonly [K, S]>, readonly [K, S]>(
          (row) => row[1],
          { comparator: total, ...admitted },
        ),
        Mini.output((delta: Mini.MultiSet<Mini.KeyValue<typeof _ORDERED, readonly [readonly [K, S], string]>>) => {
          delta.getInner().forEach(emit)
        }),
      ))
    const board = yield* SubscriptionRef.make(SortedMap.empty<string, readonly [K, S]>(Order.string))
    return {
      push: (delta) =>
        Effect.zipRight(keyed.admit(delta), engine.drive(
          () => keyed.input.sendData(new Mini.MultiSet(_rows(delta))),
          (drained) =>
            Ref.update(board, (held) =>
              Array.reduce(drained, held, (acc, [[, [[key, state], index]], count]) =>
                count > 0 ? SortedMap.set(acc, index, [key, state] as const) : SortedMap.remove(acc, index))),
        )),
      ranks: Subscribable.map(board, (held) => Chunk.fromIterable(SortedMap.values(held))),
    }
  })

function _view<Op, K, S>(handle: Replay.Memory<Op, K, S>): Subscribable.Subscribable<Fold.Table<K, S>>
function _view<Op, K, S>(handle: Replay.Versioned<Op, K, S>): Subscribable.Subscribable<Fold.Table<K, S>>
function _view<Op, K, S>(
  handle: Replay.Memory<Op, K, S> | Replay.Versioned<Op, K, S>,
  key: K,
): Subscribable.Subscribable<Option.Option<S>>
function _view<Op, K, S>(handle: Replay.Ordered<Op, K, S>): Subscribable.Subscribable<Chunk.Chunk<readonly [K, S]>>
function _view<Op, K, S>(
  handle: Replay.Memory<Op, K, S> | Replay.Ordered<Op, K, S> | Replay.Versioned<Op, K, S>,
  key?: K,
): Subscribable.Subscribable<Fold.Table<K, S> | Option.Option<S> | Chunk.Chunk<readonly [K, S]>> {
  return Predicate.hasProperty(handle, "ranks")
    ? handle.ranks
    : key === undefined
      ? handle.state
      : Subscribable.map(handle.state, (table) => HashMap.get(table, key))
}

const _feed = <Op, K, S>(
  handle: Replay.Memory<Op, K, S> | Replay.Versioned<Op, K, S>,
): Stream.Stream<Fold.Change<K, S>> => Stream.flattenIterables(handle.wave.changes)
```

## [05]-[DATAFLOW_VERBS]

[DATAFLOW_VERBS]:

```typescript
const _agg = <Op>(row: Replay.Agg<Op>): Replay.AggregateOperator<Op> =>
  row.kind === "count" ? Mini.groupByOperators.count() : Mini.groupByOperators[row.kind]((op: Op) => row.of(op))

const _operators = <Op, Aggs extends Readonly<Record<string, Replay.Agg<Op>>>>(
  aggs: Aggs,
): { readonly [Column in keyof Aggs]: Replay.AggregateOperator<Op> } => {
  // BOUNDARY ADAPTER: Record.map preserves the key census at runtime but homogenizes it in types; the mapped-key result restores that census
  return Record.map(aggs, (row): Replay.AggregateOperator<Op> => _agg(row)) as unknown as {
    readonly [Column in keyof Aggs]: Replay.AggregateOperator<Op>
  }
}

// One `Either` partition serves both altitudes: the left plan's ops and the right plan's ops leave in two multisets and
// the caller's write surface stays single whatever engine folds them.
const _sided = <OpL, OpR>(
  delta: Fold.Delta<Replay.Input<OpL, OpR>>,
): readonly [ReadonlyArray<readonly [OpL, Fold.Multiplicity]>, ReadonlyArray<readonly [OpR, Fold.Multiplicity]>] =>
  Array.partitionMap(delta, ([op, count]) =>
    _Input.$match(op, {
      Left: ({ value }): Either.Either<readonly [OpR, Fold.Multiplicity], readonly [OpL, Fold.Multiplicity]> =>
        Either.left([value, count] as const),
      Right: ({ value }) => Either.right([value, count] as const),
    }))

type _Pair<SL, SR> = readonly [SL, SR] | readonly [Option.Option<SL>, Option.Option<SR>]

function _joined<OpL, OpR, K, SL, SR>(
  left: Fold.Plan<OpL, K, SL>,
  right: Fold.Plan<OpR, K, SR>,
): Effect.Effect<Replay.Joined<OpL, OpR, K, readonly [SL, SR]>>
function _joined<OpL, OpR, K, SL, SR>(
  left: Fold.Plan<OpL, K, SL>,
  right: Fold.Plan<OpR, K, SR>,
  spec: { readonly kind: Replay.JoinKind },
): Effect.Effect<Replay.Joined<OpL, OpR, K, readonly [Option.Option<SL>, Option.Option<SR>]>>
function _joined<OpL, OpR, K, SL, SR>(
  left: Fold.Plan<OpL, K, SL>,
  right: Fold.Plan<OpR, K, SR>,
  spec: { readonly origin: AsOf },
): Effect.Effect<Replay.JoinedAt<OpL, OpR, K, readonly [SL, SR]>>
function _joined<OpL, OpR, K, SL, SR>(
  left: Fold.Plan<OpL, K, SL>,
  right: Fold.Plan<OpR, K, SR>,
  spec: { readonly kind: Replay.JoinKind; readonly origin: AsOf },
): Effect.Effect<Replay.JoinedAt<OpL, OpR, K, readonly [Option.Option<SL>, Option.Option<SR>]>>
function _joined<OpL, OpR, K, SL, SR>(
  left: Fold.Plan<OpL, K, SL>,
  right: Fold.Plan<OpR, K, SR>,
  spec?: { readonly kind?: Replay.JoinKind; readonly origin?: AsOf },
): Effect.Effect<Replay.Joined<OpL, OpR, K, _Pair<SL, SR>> | Replay.JoinedAt<OpL, OpR, K, _Pair<SL, SR>>> {
  const origin = spec?.origin
  const kind = spec?.kind
  return origin === undefined
    ? Effect.gen(function* () {
      const graph = new Mini.D2()
      const lhs = _keyed(graph, left)
      const rhs = _keyed(graph, right)
      const paired = kind === undefined
        ? lhs.staged.pipe(
          Mini.innerJoin(rhs.staged),
          Mini.map(([cell, [[key, sl], [, sr]]]): Mini.KeyValue<Fold.Cell, readonly [K, readonly [SL, SR]]> =>
            [cell, [key, [sl, sr] as const] as const]),
        )
        : lhs.staged.pipe(
          Mini.join(rhs.staged, kind),
          Mini.map(([cell, [leftRow, rightRow]]): Mini.KeyValue<Fold.Cell, [readonly [K, SL] | null, readonly [K, SR] | null]> => {
            const key = leftRow?.[0] ?? rightRow![0]
            return [cell, [key, [Option.fromNullable(leftRow?.[1]), Option.fromNullable(rightRow?.[1])] as const] as const]
          }),
        )
      const engine = yield* _engine<Fold.Change<K, _Pair<SL, SR>>>(graph, (emit) =>
        paired.pipe(
          Mini.consolidate(),
          Mini.output((delta: Mini.MultiSet<Mini.KeyValue<Fold.Cell, readonly [K, _Pair<SL, SR>]>>) =>
            _changes(_domainRows(delta.getInner()), left.cell).forEach(emit)),
        ))
      const live = yield* _live<K, _Pair<SL, SR>>()
      return {
        push: (delta: Fold.Delta<Replay.Input<OpL, OpR>>) =>
          pipe(_sided<OpL, OpR>(delta), ([lows, rows]) =>
            Effect.zipRight(Effect.zip(lhs.admit(lows), rhs.admit(rows)), engine.drive(
              () => {
                lhs.input.sendData(new Mini.MultiSet(_rows(lows)))
                rhs.input.sendData(new Mini.MultiSet(_rows(rows)))
              },
              _published(live),
            ))),
        state: Subscribable.map(live, (held) => held.table),
      }
    })
    : Effect.gen(function* () {
      const graph = new Diff.D2({ initialFrontier: [...AsOf.time(origin)] })
      const lhs = _versionedKeyed(graph, left)
      const rhs = _versionedKeyed(graph, right)
      const trace = _trace<K, _Pair<SL, SR>>(left.cell)
      const paired = kind === undefined
        ? lhs.staged.pipe(
          Diff.innerJoin(rhs.staged),
          Diff.map(([cell, [[key, sl], [, sr]]]): Diff.KeyValue<Fold.Cell, readonly [K, readonly [SL, SR]]> =>
            [cell, [key, [sl, sr] as const] as const]),
        )
        : lhs.staged.pipe(
          Diff.join(rhs.staged, kind),
          Diff.map(([cell, [leftRow, rightRow]]): Diff.KeyValue<Fold.Cell, [readonly [K, SL] | null, readonly [K, SR] | null]> => {
            const key = leftRow?.[0] ?? rightRow![0]
            return [cell, [key, [Option.fromNullable(leftRow?.[1]), Option.fromNullable(rightRow?.[1])] as const] as const]
          }),
        )
      const engine = yield* _engine<Fold.Change<K, _Pair<SL, SR>>>(graph, (emit) =>
        paired.pipe(Diff.consolidate(), Diff.output(_sunk(trace, emit))))
      const live = yield* _live<K, _Pair<SL, SR>>(Option.some(origin))
      const publish = _published(live)
      return {
        push: (at: AsOf, delta: Fold.Delta<Replay.Input<OpL, OpR>>) => _pushAt(live, at,
          pipe(_sided<OpL, OpR>(delta), ([lows, rows]) =>
            Effect.zipRight(Effect.zip(lhs.admit(lows), rhs.admit(rows)), engine.drive(
              () => {
                lhs.input.sendData(Diff.v([...AsOf.time(at)]), new Diff.MultiSet(_rows(lows)))
                rhs.input.sendData(Diff.v([...AsOf.time(at)]), new Diff.MultiSet(_rows(rows)))
              },
              (drained) => publish(drained, Option.none(), Option.some(at)),
            )))),
        state: Subscribable.map(live, (held) => held.table),
        // both inputs advance together: a frontier that moved on one side alone would settle a half-correlated version
        ..._timed(trace, engine.drive, (frontier) => {
          lhs.input.sendFrontier(frontier)
          rhs.input.sendFrontier(frontier)
        }, publish, live),
      }
    })
}

const _matched = <Op, K, S>(plan: Fold.Plan<Op, K, S>): Effect.Effect<Replay.Matched<Op, K, S>> =>
  Effect.gen(function* () {
    const graph = new Mini.D2()
    const keyed = _keyed(graph, plan)
    const probe = graph.newInput<Mini.KeyValue<Fold.Cell, boolean>>()
    const engine = yield* _engine<Fold.Change<K, S>>(graph, (emit) =>
      keyed.staged.pipe(
        Mini.filterBy(probe),
        Mini.consolidate(),
        Mini.output((delta: Mini.MultiSet<Mini.KeyValue<Fold.Cell, readonly [K, S]>>) =>
          _changes(_domainRows(delta.getInner()), plan.cell).forEach(emit)),
      ))
    const state = yield* SubscriptionRef.make(HashMap.empty<K, S>())
    return {
      push: (delta) =>
        pipe(
          Array.partitionMap(delta, ([row, count]) =>
            _Input.$match(row, {
              Left: ({ value }): Either.Either<readonly [Mini.KeyValue<Fold.Cell, boolean>, number], readonly [Op, number]> =>
                Either.left([value, count] as const),
              Right: ({ value: key }): Either.Either<readonly [Mini.KeyValue<Fold.Cell, boolean>, number], readonly [Op, number]> => {
                const membership: Mini.KeyValue<Fold.Cell, boolean> = [plan.cell(key), true]
                const change: readonly [Mini.KeyValue<Fold.Cell, boolean>, number] = [membership, count]
                return Either.right(change)
              },
            })),
          ([ops, keys]) =>
            Effect.zipRight(keyed.admit(ops), engine.drive(
              () => {
                keyed.input.sendData(new Mini.MultiSet(_rows(ops)))
                probe.sendData(new Mini.MultiSet(_rows(keys)))
              },
              (drained) => Ref.update(state, (table) => Array.reduce(drained, table, _patch)),
            )),
        ),
      state,
    }
  })

const _versionedAgg = <Op>(row: Replay.Agg<Op>): Replay.DiffAggregateOperator<Op> =>
  row.kind === "count" ? Diff.groupByOperators.count() : Diff.groupByOperators[row.kind]((op: Op) => row.of(op))

const _versionedOperators = <Op, Aggs extends Readonly<Record<string, Replay.Agg<Op>>>>(
  aggs: Aggs,
): { readonly [Column in keyof Aggs]: Replay.DiffAggregateOperator<Op> } => {
  // BOUNDARY ADAPTER: Record.map preserves the key census at runtime but homogenizes it in types; the mapped-key result restores that census
  return Record.map(aggs, (row): Replay.DiffAggregateOperator<Op> => _versionedAgg(row)) as unknown as {
    readonly [Column in keyof Aggs]: Replay.DiffAggregateOperator<Op>
  }
}

const _groupCell = (key: string): Fold.Cell => Fold.cell([key])

// The refusal names the COLUMNS that carried `NaN` or an infinity, so an operator repairs the projection rather than
// re-deriving which of a wide grouping row poisoned the cell; the fold stops at the first offending op because one
// unadmitted key already voids the whole delta.
const _unfinite = (row: Readonly<Record<string, boolean | number | string>>): ReadonlyArray<string> =>
  Array.filterMap(
    Record.toEntries(row),
    ([column, part]) => typeof part === "number" && !globalThis.Number.isFinite(part) ? Option.some(column) : Option.none(),
  )

const _admitGroup = <Op, By extends Readonly<Record<string, boolean | number | string>>>(
  by: (op: Op) => By,
) => (delta: Fold.Delta<Op>): Effect.Effect<void, ReplayFault> =>
  Option.match(
    Array.findFirst(delta, ([op]) => Option.liftPredicate(_unfinite(by(op)), Array.isNonEmptyReadonlyArray)),
    {
      onNone: () => Effect.void,
      onSome: (columns) => Effect.fail(new ReplayFault({ case: { reason: "group", columns } })),
    },
  )

function _grouped<Op, By extends Readonly<Record<string, boolean | number | string>>, Aggs extends Readonly<Record<string, Replay.Agg<Op>>>>(
  spec: { readonly name: string; readonly by: (op: Op) => By; readonly aggs: Aggs },
): Effect.Effect<Replay.Grouped<Op, By, Aggs>>
function _grouped<Op, By extends Readonly<Record<string, boolean | number | string>>, Aggs extends Readonly<Record<string, Replay.Agg<Op>>>>(
  spec: { readonly name: string; readonly by: (op: Op) => By; readonly aggs: Aggs; readonly origin: AsOf },
): Effect.Effect<Replay.GroupedAt<Op, By, Aggs>>
function _grouped<Op, By extends Readonly<Record<string, boolean | number | string>>, Aggs extends Readonly<Record<string, Replay.Agg<Op>>>>(
  spec: { readonly name: string; readonly by: (op: Op) => By; readonly aggs: Aggs; readonly origin?: AsOf },
): Effect.Effect<Replay.Grouped<Op, By, Aggs> | Replay.GroupedAt<Op, By, Aggs>> {
  const origin = spec.origin
  return origin === undefined
    ? Effect.gen(function* () {
      const graph = new Mini.D2()
      const input = graph.newInput<Op>()
      const admit = _admitGroup(spec.by)
      const engine = yield* _engine<Fold.Change<string, By & Replay.Rollup<Aggs>>>(graph, (emit) =>
        input.pipe(
          Mini.groupBy(spec.by, _operators<Op, Aggs>(spec.aggs)),
          Mini.map(([key, state]: Mini.KeyValue<string, By & Replay.Rollup<Aggs>>) =>
            [_groupCell(key), [key, state] as const] as Mini.KeyValue<Fold.Cell, readonly [string, By & Replay.Rollup<Aggs>]>),
          Mini.output((delta: Mini.MultiSet<Mini.KeyValue<Fold.Cell, readonly [string, By & Replay.Rollup<Aggs>]>>) =>
            _changes(_domainRows(delta.getInner()), _groupCell).forEach(emit)),
        ))
      const live = yield* _live<string, By & Replay.Rollup<Aggs>>()
      return {
        name: spec.name,
        push: (delta) => Effect.zipRight(admit(delta),
          engine.drive(() => input.sendData(new Mini.MultiSet(_rows(delta))), _published(live))),
        state: Subscribable.map(live, (held) => held.table),
      }
    })
    : Effect.gen(function* () {
      const graph = new Diff.D2({ initialFrontier: [...AsOf.time(origin)] })
      const input = graph.newInput<Op>()
      const admit = _admitGroup(spec.by)
      const trace = _trace<string, By & Replay.Rollup<Aggs>>(_groupCell)
      const engine = yield* _engine<Fold.Change<string, By & Replay.Rollup<Aggs>>>(graph, (emit) =>
        input.pipe(
          Diff.groupBy(spec.by, _versionedOperators<Op, Aggs>(spec.aggs)),
          Diff.map(([key, state]: Diff.KeyValue<string, By & Replay.Rollup<Aggs>>) =>
            [_groupCell(key), [key, state] as const] as Diff.KeyValue<Fold.Cell, readonly [string, By & Replay.Rollup<Aggs>]>),
          Diff.output(_sunk(trace, emit)),
        ))
      const live = yield* _live<string, By & Replay.Rollup<Aggs>>(Option.some(origin))
      const publish = _published(live)
      return {
        name: spec.name,
        push: (at: AsOf, delta: Fold.Delta<Op>) => _pushAt(live, at,
          Effect.zipRight(admit(delta), engine.drive(() => {
            input.sendData(Diff.v([...AsOf.time(at)]), new Diff.MultiSet(_rows(delta)))
          }, (drained) => publish(drained, Option.none(), Option.some(at))))),
        state: Subscribable.map(live, (held) => held.table),
        ..._timed(trace, engine.drive, (frontier) => input.sendFrontier(frontier), publish, live),
      }
    })
}

const _BoardTake = Schema.Int.pipe(Schema.positive(), Schema.brand("FoldBoardTake"))

const _topped = <Op, K, S>(
  plan: Fold.Plan<Op, K, S>,
  spec: {
    readonly by: (key: K, state: S) => Fold.Cell
    readonly rank: Order.Order<S>
    readonly take: number
  },
): Effect.Effect<Replay.Topped<Op, K, S>> =>
  Effect.gen(function* () {
    const take = yield* Schema.decode(_BoardTake)(spec.take).pipe(
      Effect.mapError(() => new ReplayFault({ case: { reason: "spec", column: "take" } })),
    )
    const graph = new Mini.D2()
    const keyed = _keyed(graph, plan)
    const total = Order.combine(
      Order.mapInput(spec.rank, (row: readonly [K, S]) => row[1]),
      Order.mapInput(Order.string, (row: readonly [K, S]) => plan.cell(row[0])),
    )
    const engine = yield* _engine<readonly [Mini.KeyValue<Fold.Cell, readonly [readonly [K, S], string]>, number]>(graph, (emit) =>
      keyed.staged.pipe(
        Mini.map(([, [key, state]]: Mini.KeyValue<Fold.Cell, readonly [K, S]>): Mini.KeyValue<Fold.Cell, readonly [K, S]> =>
          [spec.by(key, state), [key, state] as const]),
        Mini.topKWithFractionalIndex(total, { limit: take }),
        Mini.output((delta: Mini.MultiSet<Mini.KeyValue<Fold.Cell, readonly [readonly [K, S], string]>>) => {
          delta.getInner().forEach(emit)
        }),
      ))
    const boards = yield* SubscriptionRef.make(HashMap.empty<Fold.Cell, SortedMap.SortedMap<string, readonly [K, S]>>())
    return {
      push: (delta) =>
        Effect.zipRight(keyed.admit(delta), engine.drive(
          () => keyed.input.sendData(new Mini.MultiSet(_rows(delta))),
          (drained) =>
            Ref.update(boards, (held) =>
              Array.reduce(drained, held, (acc, [[group, [row, index]], count]) =>
                HashMap.modifyAt(acc, group, (slot) =>
                  pipe(
                    Option.getOrElse(slot, () => SortedMap.empty<string, readonly [K, S]>(Order.string)),
                    (pane) => (count > 0 ? SortedMap.set(pane, index, row) : SortedMap.remove(pane, index)), // eviction is the operator's signed retraction: one fractional key leaves, the pane never re-slices
                    (pane) => (SortedMap.size(pane) === 0 ? Option.none() : Option.some(pane)),
                  )))),
        )),
      boards: Subscribable.map(boards, HashMap.map((pane) => Chunk.fromIterable(SortedMap.values(pane)))),
    }
  })

const _closure = (origin: AsOf): Effect.Effect<Replay.Closure> =>
  Effect.gen(function* () {
    const graph = new Diff.D2({ initialFrontier: [...AsOf.time(origin)] })
    const input = graph.newInput<Diff.KeyValue<Fold.Cell, Fold.Cell>>()
    const engine = yield* _engine<readonly [Diff.KeyValue<Fold.Cell, Fold.Cell>, number]>(graph, (emit) =>
      input.pipe(
        Diff.iterate((paths) =>
          paths.pipe(
            Diff.map(([from, to]): Diff.KeyValue<Fold.Cell, Fold.Cell> => [to, from]),
            Diff.innerJoin(paths),
            Diff.map(([, [tail, next]]): Diff.KeyValue<Fold.Cell, Fold.Cell> => [tail, next]),
            Diff.concat(paths),
            Diff.distinct(),
          )),
        Diff.output((message: Diff.Message<Diff.KeyValue<Fold.Cell, Fold.Cell>>) => {
          if (message.type === Diff.MessageType.DATA) message.data.collection.getInner().forEach(emit)
        }),
      ))
    const reach = yield* SubscriptionRef.make(HashMap.empty<Fold.Cell, HashSet.HashSet<Fold.Cell>>())
    const sealed = yield* Ref.make(origin)
    const drained = (rows: ReadonlyArray<readonly [Diff.KeyValue<Fold.Cell, Fold.Cell>, number]>) =>
      (table: Fold.Table<Fold.Cell, HashSet.HashSet<Fold.Cell>>): Fold.Table<Fold.Cell, HashSet.HashSet<Fold.Cell>> =>
        Array.reduce(rows, table, (acc, [[from, to], count]) =>
          HashMap.modifyAt(acc, from, (slot) =>
            pipe(
              Option.getOrElse(slot, () => HashSet.empty<Fold.Cell>()),
              (held) => (count > 0 ? HashSet.add(held, to) : HashSet.remove(held, to)),
              (next) => (HashSet.size(next) === 0 ? Option.none() : Option.some(next)),
            )))
    return {
      push: (at, edges) => Effect.flatMap(Ref.get(sealed), (frontier) =>
        at.ordinal <= frontier.ordinal
          ? Effect.fail(new ReplayFault({ case: { reason: "push", at } }))
          : engine.drive(
              () => input.sendData(
                Diff.v([...AsOf.time(at)]),
                new Diff.MultiSet(edges.map(([[from, to], count]) =>
                  [[from, to], count] as [Diff.KeyValue<Fold.Cell, Fold.Cell>, number])),
              ),
              (rows) => Ref.update(reach, drained(rows)),
            )),
      seal: (frontier) => Effect.flatMap(Ref.get(sealed), (held) =>
        frontier.ordinal < held.ordinal
          ? Effect.fail(new ReplayFault({ case: { reason: "seal", frontier, sealed: held } }))
          : engine.drive(
              () => input.sendFrontier(Diff.v([...AsOf.time(frontier)])),
              (rows) => Effect.zipRight(Ref.update(reach, drained(rows)), Ref.set(sealed, frontier)),
            )),
      reach,
    }
  })
```

## [06]-[VERSIONED_LANE]

[VERSIONED_LANE]:
- Law: `history(key)` maps `Index.versions` through the trace's admitted ordinal-to-`AsOf` registry; no HLC is decoded from D2 time.
- Growth: durable replay persists journal deltas and full `AsOf` coordinates in the data branch, then restores this owner through the same plan.
- Boundary: the data branch owns the durable and replication bindings and mints the compaction `AsOf`; this lane owns the in-process versioned fold.

```typescript
const _versioned = <Op, K, S>(
  plan: Fold.Plan<Op, K, S>,
  origin: AsOf,
): Effect.Effect<Replay.Versioned<Op, K, S>> =>
  Effect.gen(function* () {
    const graph = new Diff.D2({ initialFrontier: [...AsOf.time(origin)] })
    const keyed = _versionedKeyed(graph, plan)
    const trace = _trace<K, S>(plan.cell)
    const engine = yield* _engine<Fold.Change<K, S>>(graph, (emit) =>
      keyed.staged.pipe(Diff.consolidate(), Diff.output(_sunk(trace, emit))))
    const live = yield* _live<K, S>(Option.some(origin))
    const publish = _published(live)
    return {
      push: (at, delta) => _pushAt(live, at,
        Effect.zipRight(
          keyed.admit(delta),
          engine.drive(() => {
            keyed.input.sendData(Diff.v([...AsOf.time(at)]), new Diff.MultiSet(_rows(delta)))
          }, (drained) => publish(drained, Option.none(), Option.some(at))),
        )),
      state: Subscribable.map(live, (held) => held.table),
      wave: Subscribable.map(live, (held) => held.wave),
      ..._timed(trace, engine.drive, (frontier) => keyed.input.sendFrontier(frontier), publish, live),
    }
  })

const Replay: Replay.Shape = {
  Fault: ReplayFault,
  Input: _Input,
  OrderLens: _OrderLens,
  delta: (ops) => Array.map(ops, (op) => [op, _multiplicity(1)] as const),
  memory: _memory,
  ordered: _ordered,
  joined: _joined,
  matched: _matched,
  grouped: _grouped,
  topped: _topped,
  closure: _closure,
  versioned: _versioned,
  view: _view,
  feed: _feed,
}
```

## [07]-[WATERMARK_PANES]

[WATERMARK_PANES]:
- Owner: `Window` folds per-replica stamped uncertainty, classifies lateness, and composes fixed or session panes over any `Fold.Plan`.
- Law: policy admission rejects non-finite or sub-millisecond spans; sliding fan-out is derived as `ceil(width / step)` and capped at 4096.

```typescript
declare namespace Window {
  type Mark = Causal.Stamped
  type Verdict = (typeof _VERDICTS)[number]
  type Disposition = (typeof _DISPOSITIONS)[number]
  type Policy = Schema.Schema.Type<typeof _Policy>
  type Fixed = Schema.Schema.Type<typeof _Fixed>
  type SessionPolicy = Schema.Schema.Type<typeof _Session>
  type Pane = _Pane
  type Key<K> = readonly [Pane, K]
  type Sessioned<Op, K, S> = {
    readonly push: (delta: Fold.Delta<Op>) => Effect.Effect<void, ReplayFault>
    readonly state: Subscribable.Subscribable<Fold.Table<Key<K>, S>>
  }
  type Shape = {
    readonly Policy: typeof _Policy
    readonly Pane: typeof _Pane
    readonly mark: (acks: HashMap.HashMap<Causal.Vector.Replica, Causal.Stamped>) => Option.Option<Mark>
    readonly verdict: (op: Causal.Stamped, mark: Mark) => Verdict
    readonly disposition: (policy: Policy, verdict: Verdict) => Disposition
    readonly spread: (policy: Fixed) => (stamp: Clock.Hlc) => Chunk.Chunk<Pane>
    readonly panes: <Op, K, S>(plan: Fold.Plan<Op, K, S>) => Fold.Plan<readonly [Pane, Op], Key<K>, S>
    readonly session: <Op, K, S>(
      plan: Fold.Plan<Op, K, S>,
      stamp: (op: Op) => Clock.Hlc,
      policy: SessionPolicy,
    ) => Effect.Effect<Sessioned<Op, K, S>>
    readonly close: <K, S>(
      table: Fold.Table<Key<K>, S>,
      mark: Mark,
    ) => readonly [closed: Fold.Table<Key<K>, S>, open: Fold.Table<Key<K>, S>]
  }
}

const _VERDICTS = ["punctual", "late", "uncertain"] as const
const _DISPOSITIONS = ["accept", "drop", "quarantine"] as const

const _VERDICT_BY_ORDER: Record<Causal.Vector.Ordering, Window.Verdict> = {
  before: "late",
  after: "punctual",
  equal: "punctual",
  concurrent: "uncertain",
}

const _earliest: Merge.Instance<Causal.Stamped> = Merge.min(
  Order.combineAll([
    Order.mapInput(Order.bigint, (stamped: Causal.Stamped) => stamped.window.earliest),
    Order.mapInput(Order.bigint, (stamped: Causal.Stamped) => stamped.window.latest),
    Order.mapInput(Clock.Hlc.Order, (stamped: Causal.Stamped) => stamped.stamp),
  ]),
)

const _Span = Schema.DurationFromSelf.pipe(
  Schema.filter((span) => Duration.isFinite(span) && Duration.greaterThanOrEqualTo(span, Duration.millis(1))),
)
const _Lateness = {
  late: Schema.Literal(..._DISPOSITIONS),
  uncertain: Schema.Literal(..._DISPOSITIONS),
} as const
const _Tumbling = Schema.TaggedStruct("Tumbling", { width: _Span, ..._Lateness })
const _Sliding = Schema.TaggedStruct("Sliding", {
  width: _Span,
  step: _Span,
  ..._Lateness,
}).pipe(
  Schema.filter((policy) =>
    (Clock.Hlc.delta(policy.width) + Clock.Hlc.delta(policy.step) - 1n) / Clock.Hlc.delta(policy.step) <= 4096n),
)
const _Session = Schema.TaggedStruct("Session", { gap: _Span, ..._Lateness })
const _Fixed = Schema.Union(_Tumbling, _Sliding)
const _Policy = Schema.Union(_Fixed, _Session)

class _Pane extends Schema.Class<_Pane>("Window.Pane")(
  Schema.Struct({ open: Schema.BigIntFromSelf, until: Schema.BigIntFromSelf }).pipe(
    Schema.filter((pane) => pane.open < pane.until),
  ),
) {}

const _pane = (open: bigint, until: bigint): Window.Pane => new _Pane({ open, until })

const _spread = (policy: Window.Fixed) => (stamp: Clock.Hlc): Chunk.Chunk<Window.Pane> =>
  Match.valueTags(policy, {
    Tumbling: ({ width }) => {
      const span = Clock.Hlc.delta(width)
      const open = (stamp.physical / span) * span
      return Chunk.of(_pane(open, open + span))
    },
    Sliding: ({ width, step }) => {
      const span = Clock.Hlc.delta(width)
      const stride = Clock.Hlc.delta(step)
      const at = stamp.physical
      const last = (at / stride) * stride
      const count = Number((span + stride - 1n) / stride)
      return Chunk.filter(
        Chunk.map(Chunk.range(0, count - 1), (back) => {
          const open = last - BigInt(back) * stride
          return _pane(open, open + span)
        }),
        (pane) => pane.open <= at && at < pane.until,
      )
    },
  })

const _panes = <Op, K, S>(
  plan: Fold.Plan<Op, K, S>,
): Fold.Plan<readonly [Window.Pane, Op], Window.Key<K>, S> =>
  ({
    name: `${plan.name}/paned`,
    key: ([pane, op]) => Data.tuple(pane, plan.key(op)),
    cell: ([pane, key]) => Fold.cell([pane.open, pane.until, plan.cell(key)]),
    keyAlike: Equal.equivalence(),
    lift: ([, op]) => plan.lift(op),
    merge: plan.merge,
    identity: Option.map(plan.identity, (identity) => ([pane, op]) => Fold.cell([pane.open, pane.until, identity(op)])),
  })

const _sessionRows = <Op, K, S>(
  plan: Fold.Plan<Op, K, S>,
  stamp: (op: Op) => Clock.Hlc,
  gap: bigint,
) => (values: Array<[readonly [K, Op], number]>): Array<[readonly [K, Window.Pane, S], number]> => {
  const key = values[0]?.[0][0]
  return key === undefined ? [] : pipe(
    Array.filterMap(values, ([[, op], count]) => Option.map(_scaled(plan.merge, plan.lift(op), count), (state) => ({ op, state }))),
    Array.sort(Order.mapInput(Clock.Hlc.Order, ({ op }) => stamp(op))),
    Array.reduce([] as Array<readonly [Window.Pane, S]>, (sessions, { op, state }) => {
      const at = stamp(op).physical
      return Array.match(sessions, {
        onEmpty: () => [[_pane(at, at + gap), state] as const],
        onNonEmpty: (held) => {
          const prior = Array.lastNonEmpty(held)
          return at <= prior[0].until
            ? [
                ...Array.initNonEmpty(held),
                [_pane(prior[0].open, at + gap), plan.merge.combine.combine(prior[1], state)] as const,
              ]
            : [...held, [_pane(at, at + gap), state] as const]
        },
      })
    }),
    Array.map(([pane, state]): [readonly [K, Window.Pane, S], number] => [[key, pane, state], 1]),
  )
}

const _session = <Op, K, S>(
  plan: Fold.Plan<Op, K, S>,
  stamp: (op: Op) => Clock.Hlc,
  policy: Window.SessionPolicy,
): Effect.Effect<Window.Sessioned<Op, K, S>> =>
  Effect.gen(function* () {
    const graph = new Mini.D2()
    const input = graph.newInput<Op>()
    const admit = _admission(plan)
    const staged = Option.match(plan.identity, {
      onNone: () => input.pipe(Mini.map((op: Op): Mini.KeyValue<Fold.Cell, readonly [K, Op]> => {
        const key = plan.key(op)
        return [plan.cell(key), [key, op] as const]
      })),
      onSome: (identity) => input.pipe(
        Mini.map((op: Op): Mini.KeyValue<Fold.Cell, readonly [Fold.Cell, K, Op]> => {
          const key = plan.key(op)
          return [identity(op), [plan.cell(key), key, op] as const]
        }),
        Mini.reduce(_unique<readonly [Fold.Cell, K, Op]>),
        Mini.map(([, [domain, key, op]]): Mini.KeyValue<Fold.Cell, readonly [K, Op]> =>
          [domain, [key, op] as const]),
      ),
    })
    const engine = yield* _engine<Fold.Change<Window.Key<K>, S>>(graph, (emit) =>
      staged.pipe(
        Mini.reduce(_sessionRows(plan, stamp, Clock.Hlc.delta(policy.gap))),
        Mini.map(([cell, [key, pane, state]]: Mini.KeyValue<Fold.Cell, readonly [K, Window.Pane, S]>): Mini.KeyValue<Fold.Cell, readonly [Window.Key<K>, S]> => {
          return [Fold.cell([pane.open, pane.until, cell]), [Data.tuple(pane, key), state] as const]
        }),
        Mini.consolidate(),
        Mini.output((delta: Mini.MultiSet<Mini.KeyValue<Fold.Cell, readonly [Window.Key<K>, S]>>) =>
          _changes(_domainRows(delta.getInner()), ([pane, key]) => Fold.cell([pane.open, pane.until, plan.cell(key)])).forEach(emit)),
      ))
    const state = yield* SubscriptionRef.make(HashMap.empty<Window.Key<K>, S>())
    return {
      push: (delta) =>
        Effect.zipRight(admit(delta), engine.drive(
          () => input.sendData(new Mini.MultiSet(_rows(delta))),
          (drained) => Ref.update(state, (table) => Array.reduce(drained, table, _patch)),
        )),
      state,
    }
  })

const _close = <K, S>(
  table: Fold.Table<Window.Key<K>, S>,
  mark: Window.Mark,
): readonly [Fold.Table<Window.Key<K>, S>, Fold.Table<Window.Key<K>, S>] => {
  const sealed = (key: Window.Key<K>): boolean => key[0].until <= mark.window.earliest
  return [
    HashMap.filter(table, (_state, key) => sealed(key)),
    HashMap.filter(table, (_state, key) => !sealed(key)),
  ] as const
}

const Window: Window.Shape = {
  Policy: _Policy,
  Pane: _Pane,
  mark: (acks) => Merge.fold(_earliest, Array.fromIterable(HashMap.values(acks))),
  verdict: (op, mark) => _VERDICT_BY_ORDER[Causal.compare(op, mark)],
  disposition: (policy, verdict) => verdict === "punctual" ? "accept" : policy[verdict],
  spread: _spread,
  panes: _panes,
  session: _session,
  close: _close,
}

const Fold: Fold.Shape = { ..._Fold, Fault: ReplayFault, AsOf, Replay, Window }

// --- [EXPORTS] --------------------------------------------------------------------------

export { Fold }
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
