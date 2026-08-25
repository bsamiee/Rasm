# [CORE_FEED]

`Feed` owns the tenant-scoped, contribution-identified evidence and document timeline fold.

## [01]-[INDEX]

- [02]-[DOCUMENT_REF]: the content-keyed result-document reference and its column band; `Feed.Document`.
- [03]-[ENTRY_FAMILY]: the tagged entry union and its total projections; `Feed.Entry`, `Feed.at/.lane/.subject`.
- [04]-[FEED_FOLD]: the ordered feed state, absorb step, policy row, merge, fold plan; `Feed.absorb/.merge/.plan`.
- [05]-[FEED_READS]: window and recency reads over the folded feed; `Feed.window/.recent`.

## [02]-[DOCUMENT_REF]

[DOCUMENT_REF]:

```typescript
import * as Semigroup from "@effect/typeclass/Semigroup"
import { Array, Chunk, Data, Duration, Equal, Equivalence, HashMap, Match, Option, Order, Schema, SortedMap, Stream } from "effect"
import { Clock } from "../value/clock.ts"
import { Digest } from "../value/contentKey.ts"
import { Identity } from "../value/identity.ts"
import { Quantity } from "../value/quantity.ts"
import { Shape } from "../value/schema.ts"
import { Evidence } from "./evidence.ts"
import { Fold } from "./fold.ts"
import { Merge } from "./merge.ts"

const _Column = Schema.Struct({
  name: Schema.NonEmptyString,
  kind: Schema.Literal("bool", "int", "real", "text", "stamp"),
  role: Schema.Literal("key", "measure", "category", "detail"),
  dimension: Schema.optionalWith(Quantity.Dimension, { as: "Option" }),
  precision: Schema.optionalWith(Schema.Int.pipe(Schema.nonNegative()), { as: "Option" }),
  rank: Schema.optionalWith(Schema.Int.pipe(Schema.nonNegative()), { as: "Option" }),
  nullable: Schema.Boolean,
}).pipe(Schema.filter((column) =>
  (column.role !== "measure" || column.kind === "int" || column.kind === "real")
  && (Option.isNone(column.dimension) || column.kind === "int" || column.kind === "real")
  && (Option.isNone(column.precision) || column.kind === "int" || column.kind === "real")
  && (column.role !== "key" || !column.nullable)))

const _DocumentBase = {
  key: Digest.Key.content,
  label: Schema.NonEmptyString,
  extent: Schema.Int.pipe(Schema.nonNegative()),
  origin: Schema.optionalWith(Digest.Key.content, { as: "Option" }),
  stamp: Clock.Hlc,
  tenant: Identity.Tenant,
} as const
const _Document = Schema.Union(
  Schema.Struct({ ..._DocumentBase, media: Schema.Literal("tabular"), columns: Schema.NonEmptyArray(_Column) }),
  Schema.Struct({ ..._DocumentBase, media: Schema.Literal("text"), language: Schema.NonEmptyString }),
  Schema.Struct({
    ..._DocumentBase,
    media: Schema.Literal("image"),
    width: Schema.Int.pipe(Schema.positive()),
    height: Schema.Int.pipe(Schema.positive()),
  }),
  Schema.Struct({ ..._DocumentBase, media: Schema.Literal("model"), format: Schema.Literal("3dm", "glb", "step") }),
  Schema.Struct({ ..._DocumentBase, media: Schema.Literal("binary"), mime: Schema.NonEmptyString }),
).pipe(Schema.filter((document) => document.media === "tabular"
  ? Array.dedupe(Array.map(document.columns, (column) => column.name)).length === document.columns.length
    && (
      Array.every(document.columns, (column) => Option.isNone(column.rank))
      || (
        Array.every(document.columns, (column) => Option.isSome(column.rank))
        && Array.dedupe(Array.filterMap(document.columns, (column) => column.rank)).length === document.columns.length
      )
    )
  : document.media !== "image"
    || (globalThis.Number.isSafeInteger(document.width * document.height) && document.extent === document.width * document.height)))
```

## [03]-[ENTRY_FAMILY]

[ENTRY_FAMILY]:
- Law: `Outcome` lands one CloudEvent — `subject`, `time` with `sequence`, the `rasm.tenant` baggage member, `traceparent`, and `Event.address` arrive as columns and `data` as `Evidence.Outcome` — so contribution identity is the operation's `(source, id)` address and never a payload cell.
- Growth: a new evidence vocabulary joins as one case; the feed, reads, and plan absorb it with zero edits beyond the demanded record rows.

```typescript
declare namespace Feed {
  type Column = Schema.Schema.Type<typeof _Column>
  type Document = Schema.Schema.Type<typeof _Document>
  type Correlation = Schema.Schema.Type<typeof _Correlation>
  type Subject = Fold.Cell
  type Entry = Data.TaggedEnum<{
    Outcome: {
      readonly outcome: Evidence.Outcome
      readonly subject: Digest.Key<"content">
      readonly stamp: Clock.Hlc
      readonly tenant: Identity.Tenant
      readonly address: Digest.Key<"content">
      readonly correlation: Option.Option<Correlation>
    }
    Progress: { readonly tally: Evidence.Tally }
    Shift: { readonly snapshot: Evidence.Availability }
    Document: { readonly ref: Document }
  }>
}

const _Correlation = Schema.NonEmptyString.pipe(Schema.brand("FeedCorrelation"))
const _Entry = Data.taggedEnum<Feed.Entry>()

const _at: (entry: Feed.Entry) => Clock.Hlc = _Entry.$match({
  Outcome: ({ stamp }) => stamp,
  Progress: ({ mark }) => mark.stamp,
  Shift: ({ snapshot }) => snapshot.since,
  Document: ({ ref }) => ref.stamp,
})

const _lane: (entry: Feed.Entry) => Identity.Tenant.Scope = _Entry.$match({
  Outcome: ({ tenant }) => tenant.scope,
  Progress: ({ mark }) => mark.tenant.scope,
  Shift: ({ snapshot }) => snapshot.tenant.scope,
  Document: ({ ref }) => ref.tenant.scope,
})

const _subject: (entry: Feed.Entry) => Fold.Cell = _Entry.$match({
  Outcome: ({ tenant, subject }) => Fold.cell([tenant.scope, "evidence", subject]),
  Progress: ({ mark }) => Fold.cell([mark.tenant.scope, "evidence", mark.operation]),
  Shift: ({ snapshot }) => Fold.cell([snapshot.tenant.scope, "availability"]),
  Document: ({ ref }) => Fold.cell([
    ref.tenant.scope,
    "evidence",
    Option.getOrElse(ref.origin, () => ref.key),
  ]),
})

const _correlation: (entry: Feed.Entry) => Option.Option<Feed.Correlation> = _Entry.$match({
  Outcome: ({ correlation }) => correlation,
  Progress: () => Option.none(),
  Shift: () => Option.none(),
  Document: () => Option.none(),
})

const _none = Fold.cell(["none"])
const _optional = <A>(value: Option.Option<A>, cell: (held: A) => Fold.Cell): Fold.Cell =>
  Option.match(value, { onNone: () => _none, onSome: (held) => Fold.cell(["some", cell(held)]) })
const _verdictCell = (verdict: Evidence.Availability.Verdict): Fold.Cell => Match.valueTags(verdict, {
  Available: () => Fold.cell(["Available"]),
  Gated: ({ reason, until }) => Fold.cell(["Gated", reason, _optional(until, (stamp) =>
    Fold.cell([stamp.physical, stamp.logical]))]),
  Withheld: ({ level, reason }) => Fold.cell(["Withheld", level, reason]),
})
const _documentCell = (document: Feed.Document): Fold.Cell => Match.value(document).pipe(
  Match.when({ media: "tabular" }, (ref) => Fold.cell([
    ref.media,
    ...Array.map(ref.columns, (column) => Fold.cell([
      column.name,
      column.kind,
      column.role,
      _optional(column.dimension, (dimension) => Fold.cell([dimension.symbol])),
      _optional(column.precision, (precision) => Fold.cell([precision])),
      _optional(column.rank, (rank) => Fold.cell([rank])),
      String(column.nullable),
    ])),
  ])),
  Match.when({ media: "text" }, (ref) => Fold.cell([ref.media, ref.language])),
  Match.when({ media: "image" }, (ref) => Fold.cell([ref.media, ref.width, ref.height])),
  Match.when({ media: "model" }, (ref) => Fold.cell([ref.media, ref.format])),
  Match.when({ media: "binary" }, (ref) => Fold.cell([ref.media, ref.mime])),
  Match.exhaustive,
)
const _contribution: (entry: Feed.Entry) => Fold.Cell = _Entry.$match({
  Outcome: ({ address }) => Fold.cell([address]),
  Progress: ({ mark }) => Fold.cell([
    mark.tenant.scope,
    mark.operation,
    _optional(mark.parent, (parent) => Fold.cell([parent])),
    mark.stage,
    mark.done,
    _optional(mark.total, (total) => Fold.cell([total])),
    mark.stamp.physical,
    mark.stamp.logical,
  ]),
  Shift: ({ snapshot }) => Fold.cell([
    snapshot.tenant.scope,
    snapshot.level,
    snapshot.since.physical,
    snapshot.since.logical,
    ...Array.sort(Array.map(HashMap.toEntries(snapshot.commands), ([command, verdict]) =>
      Fold.cell([command, _verdictCell(verdict)])), Order.string),
  ]),
  Document: ({ ref }) => Fold.cell([
    ref.tenant.scope,
    ref.key,
    ref.label,
    ref.extent,
    _optional(ref.origin, (origin) => Fold.cell([origin])),
    ref.stamp.physical,
    ref.stamp.logical,
    _documentCell(ref),
  ]),
})
```

## [04]-[FEED_FOLD]

[FEED_FOLD]:

```typescript
const _POSTURE_KEYS = ["stack", "coalesce", "latest"] as const
const _Posture = Shape.vocabulary(_POSTURE_KEYS, {
  stack: { slot: (_subject: Feed.Subject, _kind: Feed.Entry["_tag"]) => Option.none<Feed.Slot>() },
  coalesce: { slot: (subject: Feed.Subject, kind: Feed.Entry["_tag"]) => Option.some<Feed.Slot>(Data.tuple(subject, kind)) },
  latest: { slot: (subject: Feed.Subject, _kind: Feed.Entry["_tag"]) => Option.some<Feed.Slot>(Data.tuple(subject)) },
})
const _Cap = Schema.Int.pipe(Schema.positive(), Schema.brand("FeedCap"))
const _BatchSize = Schema.Int.pipe(Schema.positive(), Schema.brand("FeedBatchSize"))
const _Take = Schema.Int.pipe(Schema.nonNegative(), Schema.brand("FeedTake"))
const _Policy = Schema.Struct({
  cap: _Cap,
  posture: _Posture.schema,
  batch: Schema.Struct({
    size: _BatchSize,
    within: Schema.Duration.pipe(Schema.filter((duration) => Duration.isFinite(duration) && Duration.greaterThan(duration, Duration.zero))),
  }),
})
const _Window = Schema.Struct({
  from: Schema.optionalWith(Clock.Hlc, { as: "Option" }),
  until: Schema.optionalWith(Clock.Hlc, { as: "Option" }),
}).pipe(Schema.filter((window) => Option.match(Option.zip(window.from, window.until), {
  onNone: () => true,
  onSome: ([from, until]) => Order.lessThanOrEqualTo(Clock.Hlc.Order)(from, until),
})))
const _StatePolicy: unique symbol = Symbol.for("@rasm/core/Feed/StatePolicy")

declare namespace Feed {
  type Key = readonly [
    stamp: Clock.Hlc,
    subject: Subject,
    kind: Entry["_tag"],
    correlation: Option.Option<Correlation>,
    contribution: Fold.Cell,
  ]
  type Posture = (typeof _POSTURE_KEYS)[number]
  type Window = Schema.Schema.Type<typeof _Window>
  type Slot = readonly [Subject, ...ReadonlyArray<string>]
  type Policy = Schema.Schema.Type<typeof _Policy>
  type State<P extends Policy = Policy> = {
    readonly policy: P
    readonly rows: SortedMap.SortedMap<Key, Entry>
    readonly live: HashMap.HashMap<Slot, Key>
    readonly [_StatePolicy]: P
  }
  type Shape = {
    readonly Entry: typeof _Entry
    readonly Document: typeof _Document
    readonly Policy: typeof _Policy
    readonly Window: typeof _Window
    readonly Take: typeof _Take
    readonly at: (entry: Entry) => Clock.Hlc
    readonly lane: (entry: Entry) => Identity.Tenant.Scope
    readonly subject: (entry: Entry) => Subject
    readonly correlation: (entry: Entry) => Option.Option<Correlation>
    readonly empty: <P extends Policy>(policy: P) => State<P>
    readonly absorb: <P extends Policy>(state: State<P>, entry: Entry) => State<P>
    readonly merge: <P extends Policy>(policy: P) => Merge.Instance<State<P>>
    readonly plan: <P extends Policy>(policy: P) => Fold.Plan<Entry, Identity.Tenant.Scope, State<P>>
    readonly grouped: <E, R>(entries: Stream.Stream<Entry, E, R>, policy: Policy) => Stream.Stream<Chunk.Chunk<Entry>, E, R>
    readonly window: (
      state: State,
      bounds: Window,
    ) => Chunk.Chunk<Entry>
    readonly recent: (state: State, take: Schema.Schema.Type<typeof _Take>) => Chunk.Chunk<Entry>
  }
}

const _byKey: Order.Order<Feed.Key> = Order.combine(
  Order.mapInput(Clock.Hlc.Order, (key: Feed.Key) => key[0]),
  Order.combine(
    Order.mapInput(Order.string, (key: Feed.Key) => key[1]),
    Order.combine(
      Order.mapInput(Order.string, (key: Feed.Key) => key[2]),
      Order.combine(
        Order.mapInput(
          Order.tuple(Order.boolean, Order.string),
          (key: Feed.Key) => [Option.isSome(key[3]), Option.getOrElse(key[3], () => "")] as const,
        ),
        Order.mapInput(Order.string, (key: Feed.Key) => key[4]),
      ),
    ),
  ),
)

const _placed = <P extends Feed.Policy>(
  state: Feed.State<P>,
  slot: Option.Option<Feed.Slot>,
  key: Feed.Key,
  entry: Feed.Entry,
): Feed.State<P> => ({
  ...state,
  rows: SortedMap.set(state.rows, key, entry),
  live: Option.match(slot, { onNone: () => state.live, onSome: (held) => HashMap.set(state.live, held, key) }),
})

const _evicted = <P extends Feed.Policy>(state: Feed.State<P>): Feed.State<P> =>
  SortedMap.size(state.rows) <= state.policy.cap
    ? state
    : Option.match(SortedMap.headOption(state.rows), {
        onNone: () => state,
        onSome: ([head, entry]) => ({
          ...state,
          rows: SortedMap.remove(state.rows, head),
          live: Option.match(
            Option.flatMap(_Posture.at(state.policy.posture).slot(_subject(entry), entry._tag), (slot) =>
              Option.map(HashMap.get(state.live, slot), (pointer) => [slot, pointer] as const)),
            {
              onNone: () => state.live,
              onSome: ([slot, pointer]) => (Equal.equals(pointer, head) ? HashMap.remove(state.live, slot) : state.live),
            },
          ),
        }),
      })

const _absorb = <P extends Feed.Policy>(state: Feed.State<P>, entry: Feed.Entry): Feed.State<P> => {
  const slot = _Posture.at(state.policy.posture).slot(_subject(entry), entry._tag)
  const key: Feed.Key = Data.tuple(_at(entry), _subject(entry), entry._tag, _correlation(entry), _contribution(entry))
  return Option.match(Option.flatMap(slot, (held) => HashMap.get(state.live, held)), {
    onNone: () => _evicted(_placed(state, slot, key, entry)),
    onSome: (prior) =>
      Order.lessThan(_byKey)(prior, key)
        ? _evicted(_placed({ ...state, rows: SortedMap.remove(state.rows, prior) }, slot, key, entry))
        : state,
  })
}

const _empty = <P extends Feed.Policy>(policy: P): Feed.State<P> => ({
  policy,
  rows: SortedMap.empty(_byKey),
  live: HashMap.empty(),
  [_StatePolicy]: policy,
})
```

## [05]-[FEED_READS]

[FEED_READS]:
- Growth: a new read (per-kind lane, subject history) is one projection member over the same two structures.

```typescript
const Feed: Feed.Shape = {
  Entry: _Entry,
  Document: _Document,
  Policy: _Policy,
  Window: _Window,
  Take: _Take,
  at: _at,
  lane: _lane,
  subject: _subject,
  correlation: _correlation,
  empty: _empty,
  absorb: _absorb,
  merge: (policy) =>
    ({
      combine: Semigroup.make((self: Feed.State<typeof policy>, that: Feed.State<typeof policy>) =>
        SortedMap.reduce(
          that.rows,
          SortedMap.reduce(self.rows, _empty(policy), (acc, entry) => _absorb(acc, entry)),
          (acc, entry) => _absorb(acc, entry),
        )),
      law: "semilattice",
      alike: Equivalence.make((self, that) => Equal.equals(self.policy, that.policy) && Equal.equals(self.rows, that.rows)),
      empty: Option.some(_empty(policy)),
    }),
  plan: (policy) =>
    ({
      name: "state/feed",
      key: _lane,
      cell: (tenant) => Fold.cell([
        tenant,
        policy.cap,
        policy.posture,
        policy.batch.size,
        Duration.toMillis(policy.batch.within),
      ]),
      keyAlike: Equivalence.string,
      lift: (entry) => _absorb(_empty(policy), entry),
      merge: Feed.merge(policy),
      identity: Option.none(),
    }),
  grouped: (entries, policy) => Stream.groupedWithin(entries, policy.batch.size, policy.batch.within),
  window: (state, bounds) =>
    SortedMap.reduce(state.rows, Chunk.empty<Feed.Entry>(), (kept, entry) =>
      (
        Option.match(bounds.from, { onNone: () => true, onSome: (from) => !Order.lessThan(Clock.Hlc.Order)(_at(entry), from) })
        && Option.match(bounds.until, { onNone: () => true, onSome: (until) => Order.lessThan(Clock.Hlc.Order)(_at(entry), until) })
      )
        ? Chunk.append(kept, entry)
        : kept),
  recent: (state, take) =>
    take === 0
      ? Chunk.empty()
      : SortedMap.reduce(state.rows, Chunk.empty<Feed.Entry>(), (kept, entry) =>
          Chunk.append(Chunk.size(kept) < take ? kept : Chunk.drop(kept, 1), entry)),
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Feed }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
