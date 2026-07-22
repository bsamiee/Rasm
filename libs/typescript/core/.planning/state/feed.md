# [CORE_FEED]

The evidence timeline aggregator: one process-local `Entry` family wrapping the evidence vocabularies — receipt envelopes, progress marks, availability shifts, and the page-owned content-keyed `DocumentRef` — and the `Feed` fold that maintains an `Hlc`-ordered, capacity-bounded, coalescing timeline per tenant lane. The feed is the evidence-timeline projection the C# AppUi renders, re-owned as a fold: insertion is a `SortedMap` write under a composed event-time order — never an array re-sort — eviction is a single head drop under the cap, absorption geometry is a posture row (`stack` appends, `coalesce` replaces per subject-and-kind, `latest` compacts per subject) whose slot projection is data, and the whole feed is one more `fold#PLAN_CONTRACT` plan row so live views and durable projections consume it like any other fold. `DocumentRef` is the result-document evidence: a `ContentKey` reference plus a media row and an optional column band, producer-opaque by construction — the same shape carries any runtime's result artifact, and a `tabular` band types what charts, tables, and views bind against the self-describing payload. The module is `core/src/state/feed.ts`; a new evidence vocabulary joins as one entry case, a new media kind is one literal row, a new read is one projection member.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                  | [PUBLIC]                               |
| :-----: | :------------------ | :---------------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `DOCUMENT_REF`      | the content-keyed result-document reference and its column band         | `Feed.Document`                        |
|  [02]   | `ENTRY_FAMILY`      | the tagged entry union and its total projections                        | `Feed.Entry`, `Feed.at/.lane/.subject` |
|  [03]   | `FEED_FOLD`         | the ordered feed state, absorb step, policy row, merge, fold plan       | `Feed.absorb/.merge/.plan`             |
|  [04]   | `EVIDENCE_TIMELINE` | the AppUi evidence-timeline wire counterpart absorbed onto the one feed | `Feed.timeline`                        |
|  [05]   | `FEED_READS`        | window and recency reads over the folded feed                           | `Feed.window/.recent`                  |

## [02]-[DOCUMENT_REF]

[DOCUMENT_REF]:
- Owner: `DocumentRef` — the content-keyed result-document evidence: `key` (the artifact coordinate — the one identity any runtime's mint agrees on), `media` (the consumer-routing row), `label`, `extent`, `origin` (the producing operation or command key, optional), `columns` (the optional column band), `stamp`, `tenant`; reached as `Feed.Document`, so one import carries the feed and the shape a document view binds.
- Law: the reference is producer-opaque by construction — its fields are the content key, the media row, and the self-description band; no producer discriminant exists on the shape, so a C#-minted and a Python-minted result artifact carry through identical values and zero consumer branches on origin runtime.
- Law: the column band is the binding contract, not validation — a `tabular` document's payload is self-describing and the band states what a view binds: `name`, `kind` (the closed logical axis), `role` (the binding axis a grid or chart consumes — `key` identifies rows, `measure` aggregates, `category` groups and facets, `detail` renders inert), `dimension` (the `value/quantity` SI vector, carried so a magnitude column renders as a projection over the SI value, never a `{value, unit}` re-decode), `precision` (the optional fraction-digit render hint), `rank` (the optional display order — an absent rank defers to band order), `nullable`; a band/payload disagreement surfaces as consumer evidence, never a re-validation here.
- Law: references fold from receipt evidence — an `Applied` receipt's `touched` keys name the produced artifacts, so app composition mints `DocumentRef` values from evidence already on the feed; this page owns the vocabulary and the fold, never a fetch.
- Growth: a new media kind is one literal row; a new column axis (a sort collation, a format mask) is one `_Column` field; a new binding role is one `role` literal every band-driven view absorbs as a dispatch arm.
- Packages: `@effect/typeclass` (`Semigroup.make`); `effect` (`Chunk`, `Data`, `Equal`, `Equivalence`, `HashMap`, `Option`, `Order`, `Schema`, `SortedMap`, `Stream`); `../value/clock.ts` (`Hlc`); `../value/identity.ts` (`TenantContext`); `../value/contentKey.ts` (`ContentKey`); `../value/quantity.ts` (`Dimension`); `./evidence.ts` (`Availability`, `ProgressMark`, `ReceiptEnvelope`); `./merge.ts` (`Merge`); `./fold.ts` (`Fold`).

```typescript signature
import * as Semigroup from "@effect/typeclass/Semigroup"
import { Chunk, Data, Equal, Equivalence, HashMap, Option, Order, Schema, SortedMap, Stream } from "effect"
import { Hlc } from "../value/clock.ts"
import { ContentKey } from "../value/contentKey.ts"
import { TenantContext } from "../value/identity.ts"
import { Dimension } from "../value/quantity.ts"
import { Availability, ProgressMark, ReceiptEnvelope } from "./evidence.ts"
import { Fold } from "./fold.ts"
import { Merge } from "./merge.ts"

const _Column = Schema.Struct({
  name: Schema.NonEmptyString,
  kind: Schema.Literal("bool", "int", "real", "text", "stamp"),
  role: Schema.Literal("key", "measure", "category", "detail"),
  dimension: Schema.optionalWith(Dimension, { as: "Option" }),
  precision: Schema.optionalWith(Schema.Int.pipe(Schema.nonNegative()), { as: "Option" }),
  rank: Schema.optionalWith(Schema.Int.pipe(Schema.nonNegative()), { as: "Option" }),
  nullable: Schema.Boolean,
})

class DocumentRef extends Schema.Class<DocumentRef>("DocumentRef")({
  key: ContentKey,
  media: Schema.Literal("tabular", "text", "image", "model", "binary"),
  label: Schema.NonEmptyString,
  extent: Schema.Int.pipe(Schema.nonNegative()),
  origin: Schema.optionalWith(ContentKey, { as: "Option" }),
  columns: Schema.optionalWith(Schema.NonEmptyArray(_Column), { as: "Option" }),
  stamp: Hlc,
  tenant: TenantContext,
}) {}
```

## [03]-[ENTRY_FAMILY]

[ENTRY_FAMILY]:
- Owner: `Feed.Entry` — a `Data.taggedEnum` over `Receipt`, `Progress`, and `Shift` cases wrapping the sibling owners whole plus the `Document` case wrapping the page-owned `DocumentRef`; the receipt case also carries its optional timeline correlation because that coordinate belongs to the projection envelope, not the decoded command evidence. The family is process-local projection vocabulary (the members already own their wire twins or, for the reference, never had one), so the enum form costs zero codecs and carries structural equality for coalescing; the type rides the owner's merged namespace, so `Feed` is the module's one export.
- Law: projections are total record dispatches — `_at` (the entry's event stamp), `_lane` (the tenant lane), `_subject` (the coalescing coordinate: command key, operation key, tenant scope, or document key — re-emission of one document coalesces idempotently), `_correlation` (the optional timeline coordinate) — one `$match` record each, so a new entry kind is one case plus four record rows the compiler demands.
- Law: cross-vocabulary correlation at live altitude — receipts joined to progress by operation key, documents joined to their origin receipts — is `fold#DATAFLOW_VERBS`'s `joined` handle; the feed aggregates for rendering and never correlates by hand.
- Growth: a new evidence vocabulary joins as one case; the feed, reads, and plan absorb it with zero edits beyond the demanded record rows.

```typescript signature
declare namespace Feed {
  type Column = typeof _Column.Type
  type Document = DocumentRef
  type Entry = Data.TaggedEnum<{
    Receipt: { readonly envelope: ReceiptEnvelope; readonly correlation: Option.Option<string> }
    Progress: { readonly mark: ProgressMark }
    Shift: { readonly snapshot: Availability }
    Document: { readonly ref: DocumentRef }
  }>
}

const _Entry = Data.taggedEnum<Feed.Entry>()

const _at: (entry: Feed.Entry) => Hlc = _Entry.$match({
  Receipt: ({ envelope }) => envelope.stamp,
  Progress: ({ mark }) => mark.stamp,
  Shift: ({ snapshot }) => snapshot.since,
  Document: ({ ref }) => ref.stamp,
})

const _lane: (entry: Feed.Entry) => TenantContext = _Entry.$match({
  Receipt: ({ envelope }) => envelope.tenant,
  Progress: ({ mark }) => mark.tenant,
  Shift: ({ snapshot }) => snapshot.tenant,
  Document: ({ ref }) => ref.tenant,
})

const _subject: (entry: Feed.Entry) => string = _Entry.$match({
  Receipt: ({ envelope }) => envelope.command,
  Progress: ({ mark }) => mark.operation,
  Shift: ({ snapshot }) => snapshot.tenant.scope,
  Document: ({ ref }) => ref.key,
})

const _correlation: (entry: Feed.Entry) => Option.Option<string> = _Entry.$match({
  Receipt: ({ correlation }) => correlation,
  Progress: () => Option.none(),
  Shift: () => Option.none(),
  Document: () => Option.none(),
})
```

## [04]-[FEED_FOLD]

[FEED_FOLD]:
- Owner: `Feed.State` — the ordered rows (`SortedMap` keyed by the composed `[stamp, subject, tag, correlation]` order) plus the coalescing index (`HashMap` from the posture-projected slot to the live row key); the absorb step writes both in one advance, so ordering and coalescing are two reads of one fold state, never two structures maintained by different call sites.
- Law: the row key order composes event stamp, then subject, tag, and optional correlation — total over independently correlated timeline entries — so concurrent same-stamp evidence from different subjects or timelines interleaves deterministically and the feed order is reproducible from any replay: REPLAY_LAW at feed altitude.
- Law: the policy row is data both ways — `Feed.Policy` is the one schema, its branded positive-integer `cap` bounds the census with a single head eviction per insert (the census grows by at most one, so eviction is one `SortedMap.remove` of `headOption`, never a sweep), `posture` names a `_POSTURES` row whose `slot` projection IS the absorption geometry, and `batch` carries the positive `size` plus `Schema.Duration` horizon that `Feed.grouped` applies through `Stream.groupedWithin`: `stack` projects no slot (every entry appends), `coalesce` projects `[subject, tag]` (the previous same-subject same-kind row replaces), `latest` projects `[subject]` alone (one live row per subject regardless of kind — the compacted timeline a subject-history lane serves). A zero, negative, fractional, unknown posture, or invalid duration fails at policy admission before any fold or live grouping runs; one absorb body reads the projected slot, a new posture is one row, never a body branch, and timeout/size knobs never escape the carrier onto the stream entrypoint.
- Law: every slotted posture keeps the greatest key per slot — an arrival whose key does not outrank the live pointer is superseded evidence the absorb drops — so absorb order cannot bury newer evidence and the slotted lanes stay commutative; a distinct-entry tie on one composed key is a stamp collision the HLC-monotone mint excludes.
- Law: the feed merges as an instance — union of rows re-absorbed under the same policy — commutative and idempotent because row identity is the composed key and eviction always drops the global minimum, so lane-partitioned feeds fuse under `Merge.fold` like every other lattice, and `Feed.plan` partitions by tenant lane so every altitude runs the identical fold; `alike` derives — `Equal.equivalence` mapped onto the rows `SortedMap`, whose structural equality is the ordered, key-inclusive pairwise entry comparison the container itself carries — so state equality composes the collection's own proof and never leans on the key-derives-from-entry invariant.
- Boundary: a serving edge frames the fold's live view; the durable feed projection is the data branch binding `Feed.plan`; AppUi rendering consumes served rows and never re-sorts.

```typescript signature
const _POSTURE_KEYS = ["stack", "coalesce", "latest"] as const
const _Cap = Schema.Int.pipe(Schema.positive(), Schema.brand("FeedCap"))
const _BatchSize = Schema.Int.pipe(Schema.positive(), Schema.brand("FeedBatchSize"))
const _Take = Schema.Int.pipe(Schema.nonNegative(), Schema.brand("FeedTake"))
const _Policy = Schema.Struct({
  cap: _Cap,
  posture: Schema.Literal(..._POSTURE_KEYS),
  batch: Schema.Struct({ size: _BatchSize, within: Schema.Duration }),
})

declare namespace Feed {
  type Key = readonly [stamp: Hlc, subject: string, kind: Entry["_tag"], correlation: string]
  type Posture = (typeof _POSTURE_KEYS)[number]
  type Slot = readonly [string, ...ReadonlyArray<string>]
  type Policy = typeof _Policy.Type
  type State = {
    readonly rows: SortedMap.SortedMap<Key, Entry>
    readonly live: HashMap.HashMap<Slot, Key>
  }
  type Shape = {
    readonly Entry: typeof _Entry
    readonly Document: typeof DocumentRef
    readonly Policy: typeof _Policy
    readonly Take: typeof _Take
    readonly at: (entry: Entry) => Hlc
    readonly lane: (entry: Entry) => TenantContext
    readonly subject: (entry: Entry) => string
    readonly correlation: (entry: Entry) => Option.Option<string>
    readonly empty: State
    readonly absorb: (policy: Policy) => (state: State, entry: Entry) => State
    readonly timeline: (policy: Policy) => (state: State, timeline: Timeline) => State
    readonly merge: (policy: Policy) => Merge.Instance<State>
    readonly plan: (policy: Policy) => Fold.Plan<Entry, TenantContext, State>
    readonly grouped: <E, R>(entries: Stream.Stream<Entry, E, R>, policy: Policy) => Stream.Stream<Chunk.Chunk<Entry>, E, R>
    readonly window: (state: State, bounds: { readonly from: Option.Option<Hlc>; readonly until: Option.Option<Hlc> }) => Chunk.Chunk<Entry>
    readonly recent: (state: State, take: typeof _Take.Type) => Chunk.Chunk<Entry>
  }
}

const _byKey: Order.Order<Feed.Key> = Order.combine(
  Order.mapInput(Hlc.Order, (key: Feed.Key) => key[0]),
  Order.combine(
    Order.mapInput(Order.string, (key: Feed.Key) => key[1]),
    Order.combine(
      Order.mapInput(Order.string, (key: Feed.Key) => key[2]),
      Order.mapInput(Order.string, (key: Feed.Key) => key[3]),
    ),
  ),
)

const _POSTURES = {
  stack: { slot: (_subject: string, _kind: Feed.Entry["_tag"]) => Option.none<Feed.Slot>() },
  coalesce: { slot: (subject: string, kind: Feed.Entry["_tag"]) => Option.some<Feed.Slot>(Data.tuple(subject, kind)) },
  latest: { slot: (subject: string, _kind: Feed.Entry["_tag"]) => Option.some<Feed.Slot>(Data.tuple(subject)) },
} as const satisfies Record<Feed.Posture, { readonly slot: (subject: string, kind: Feed.Entry["_tag"]) => Option.Option<Feed.Slot> }>

const _placed = (state: Feed.State, slot: Option.Option<Feed.Slot>, key: Feed.Key, entry: Feed.Entry): Feed.State => ({
  rows: SortedMap.set(state.rows, key, entry),
  live: Option.match(slot, { onNone: () => state.live, onSome: (held) => HashMap.set(state.live, held, key) }),
})

const _evicted = (policy: Feed.Policy, state: Feed.State): Feed.State =>
  SortedMap.size(state.rows) <= policy.cap
    ? state
    : Option.match(SortedMap.headOption(state.rows), {
        onNone: () => state,
        onSome: ([head, entry]) => ({
          rows: SortedMap.remove(state.rows, head),
          live: Option.match(
            Option.flatMap(_POSTURES[policy.posture].slot(_subject(entry), entry._tag), (slot) =>
              Option.map(HashMap.get(state.live, slot), (pointer) => [slot, pointer] as const)),
            {
              onNone: () => state.live,
              onSome: ([slot, pointer]) => (Equal.equals(pointer, head) ? HashMap.remove(state.live, slot) : state.live),
            },
          ),
        }),
      })

const _absorb = (policy: Feed.Policy) => (state: Feed.State, entry: Feed.Entry): Feed.State => {
  const slot = _POSTURES[policy.posture].slot(_subject(entry), entry._tag)
  const key: Feed.Key = Data.tuple(_at(entry), _subject(entry), entry._tag, Option.getOrElse(_correlation(entry), () => ""))
  return Option.match(Option.flatMap(slot, (held) => HashMap.get(state.live, held)), {
    onNone: () => _evicted(policy, _placed(state, slot, key, entry)),
    onSome: (prior) =>
      Order.lessThan(_byKey)(prior, key)
        ? _evicted(policy, _placed({ rows: SortedMap.remove(state.rows, prior), live: state.live }, slot, key, entry))
        : state,
  })
}

const _empty: Feed.State = { rows: SortedMap.empty(_byKey), live: HashMap.empty() }
```

## [05]-[EVIDENCE_TIMELINE]

[EVIDENCE_TIMELINE]:
- Owner: `Feed.Timeline` — the AppUi `Diagnostics/evidence` counterpart: the exported `EvidenceTimelineWire` (correlation + envelope rows) decodes at the `state/evidence` `ReceiptEnvelope` wire twin, and `Feed.timeline` absorbs the decoded envelopes onto the ONE feed — no sibling feed shape, no second timeline owner.
- Law: the correlation key survives as the receipt entry's own `correlation` column and participates in ordered row identity, while skew bands re-derive from the `Hlc` stamps at render — the wire's `band` pairs are producer-side render hints the fold never re-stores, so two independently correlated timelines cannot overwrite one another and timeline ingest still lands on the same feed owner as live absorb.
- Growth: a new evidence wire row is one envelope case at the `state/evidence` twin; this fold absorbs it with zero edits.

```typescript signature
declare namespace Feed {
  type Timeline = { readonly correlation: string; readonly envelopes: Chunk.Chunk<ReceiptEnvelope> }
}

const _timeline = (policy: Feed.Policy) => (state: Feed.State, timeline: Feed.Timeline): Feed.State =>
  Chunk.reduce(timeline.envelopes, state, (folded, envelope) =>
    _absorb(policy)(folded, _Entry.Receipt({ envelope, correlation: Option.some(timeline.correlation) })))
```

## [06]-[FEED_READS]

[FEED_READS]:
- Owner: the read family — `window` folds the rows inside optional `Hlc` bounds, and `recent` keeps a rolling answer bounded by the admitted `Feed.Take`; both reduce the ordered state directly, so neither materializes the full timeline before selecting its answer and neither observes an unordered intermediate.
- Law: reads never expose the coalescing index — consumers see entries in event-time order only; the index is fold-interior state co-located with its owner.
- Growth: a new read (per-kind lane, subject history) is one projection member over the same two structures.

```typescript signature
const Feed: Feed.Shape = {
  Entry: _Entry,
  Document: DocumentRef,
  Policy: _Policy,
  Take: _Take,
  at: _at,
  lane: _lane,
  subject: _subject,
  correlation: _correlation,
  empty: _empty,
  absorb: _absorb,
  timeline: _timeline,
  merge: (policy) =>
    Merge.instance({
      combine: Semigroup.make((self: Feed.State, that: Feed.State) =>
        SortedMap.reduce(that.rows, self, (acc, entry) => _absorb(policy)(acc, entry))),
      posture: { commutative: true, idempotent: true },
      alike: Equivalence.mapInput(
        Equal.equivalence<SortedMap.SortedMap<Feed.Key, Feed.Entry>>(),
        (state: Feed.State) => state.rows,
      ),
      empty: Option.some(_empty),
    }),
  plan: (policy) =>
    Fold.plan({
      name: "state/feed",
      key: _lane,
      lift: (entry) => _absorb(policy)(_empty, entry),
      merge: Feed.merge(policy),
    }),
  grouped: (entries, policy) => Stream.groupedWithin(entries, policy.batch.size, policy.batch.within),
  window: (state, bounds) =>
    SortedMap.reduce(state.rows, Chunk.empty<Feed.Entry>(), (kept, entry) =>
      (
        Option.match(bounds.from, { onNone: () => true, onSome: (from) => !Order.lessThan(Hlc.Order)(_at(entry), from) })
        && Option.match(bounds.until, { onNone: () => true, onSome: (until) => Order.lessThan(Hlc.Order)(_at(entry), until) })
      )
        ? Chunk.append(kept, entry)
        : kept),
  recent: (state, take) =>
    take === 0
      ? Chunk.empty()
      : SortedMap.reduce(state.rows, Chunk.empty<Feed.Entry>(), (kept, entry) =>
          Chunk.append(Chunk.size(kept) < take ? kept : Chunk.drop(kept, 1), entry)),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Feed }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
