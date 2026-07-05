# [CORE_FEED]

The evidence timeline aggregator: one process-local `Entry` family wrapping the evidence vocabularies — receipt envelopes, progress marks, availability shifts, and the page-owned content-keyed `DocumentRef` — and the `Feed` fold that maintains an `Hlc`-ordered, capacity-bounded, coalescing timeline per tenant lane. The feed is the evidence-timeline projection the C# AppUi renders, re-owned as a fold: insertion is a `SortedMap` write under a composed event-time order — never an array re-sort — eviction is a single head drop under the cap, coalescing replaces the previous entry of the same subject and kind instead of stacking repeats, and the whole feed is one more `fold#PLAN_CONTRACT` plan row so live views and durable projections consume it like any other fold. `DocumentRef` is the result-document evidence: a `ContentKey` reference plus a media row and an optional column band, producer-opaque by construction — the same shape carries any runtime's result artifact, and a `tabular` band types what charts, tables, and views bind against the self-describing payload. The module is `core/src/state/feed.ts`; a new evidence vocabulary joins as one entry case, a new media kind is one literal row, a new read is one projection member.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                            | [PUBLIC]                                            |
| :-----: | :--------------- | :------------------------------------------------------------------ | :------------------------------------------------------ |
|  [01]   | `DOCUMENT_REF`   | the content-keyed result-document reference and its column band     | `Feed.Document`                                          |
|  [02]   | `ENTRY_FAMILY`   | the tagged entry union and its total projections                    | `Feed.Entry`, `Feed.at/.lane/.subject`                   |
|  [03]   | `FEED_FOLD`      | the ordered feed state, absorb step, policy row, merge, fold plan   | `Feed.absorb/.merge/.plan`                               |
|  [04]   | `FEED_READS`     | window and recency reads over the folded feed                       | `Feed.window/.recent`                                    |

## [2]-[DOCUMENT_REF]

[DOCUMENT_REF]:
- Owner: `DocumentRef` — the content-keyed result-document evidence: `key` (the artifact coordinate — the one identity any runtime's mint agrees on), `media` (the consumer-routing row), `label`, `extent`, `origin` (the producing operation or command key, optional), `columns` (the optional column band), `stamp`, `tenant`; reached as `Feed.Document`, so one import carries the feed and the shape a document view binds.
- Law: the reference is producer-opaque by construction — its fields are the content key, the media row, and the self-description band; no producer discriminant exists on the shape, so a C#-minted and a Python-minted result artifact carry through identical values and zero consumer branches on origin runtime.
- Law: the column band is the binding contract, not validation — a `tabular` document's payload is self-describing and the band states what a view binds: `name`, `kind` (the closed logical axis), `dimension` (the `value/quantity` SI vector, carried so a magnitude column renders as a projection over the SI value, never a `{value, unit}` re-decode), `nullable`; a band/payload disagreement surfaces as consumer evidence, never a re-validation here.
- Law: references fold from receipt evidence — an `Applied` receipt's `touched` keys name the produced artifacts, so app composition mints `DocumentRef` values from evidence already on the feed; this page owns the vocabulary and the fold, never a fetch.
- Growth: a new media kind is one literal row; a new column axis (a precision hint, a display rank) is one `_Column` field.
- Packages: `@effect/typeclass` (`Semigroup.make`); `effect` (`Chunk`, `Data`, `Equal`, `HashMap`, `Option`, `Order`, `Schema`, `SortedMap`); `../value/clock.ts` (`Hlc`); `../value/identity.ts` (`TenantContext`); `../value/contentKey.ts` (`ContentKey`); `../value/quantity.ts` (`Dimension`); `./evidence.ts` (`Availability`, `ProgressMark`, `ReceiptEnvelope`); `./merge.ts` (`Merge`); `./fold.ts` (`Fold`).

```typescript
import * as Semigroup from "@effect/typeclass/Semigroup"
import { Chunk, Data, Equal, HashMap, Option, Order, Schema, SortedMap } from "effect"
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
  dimension: Schema.optionalWith(Dimension, { as: "Option" }),
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

## [3]-[ENTRY_FAMILY]

[ENTRY_FAMILY]:
- Owner: `Feed.Entry` — a `Data.taggedEnum` over `Receipt`, `Progress`, and `Shift` cases wrapping the sibling owners whole plus the `Document` case wrapping the page-owned `DocumentRef`; the family is process-local projection vocabulary (the members already own their wire twins or, for the reference, never had one), so the enum form costs zero codecs and carries structural equality for coalescing; the type rides the owner's merged namespace, so `Feed` is the module's one export.
- Law: projections are total record dispatches — `_at` (the entry's event stamp), `_lane` (the tenant lane), `_subject` (the coalescing coordinate: command key, operation key, tenant scope, or document key — re-emission of one document coalesces idempotently) — one `$match` record each, so a new entry kind is one case plus three record rows the compiler demands.
- Law: cross-vocabulary correlation at live altitude — receipts joined to progress by operation key, documents joined to their origin receipts — is `fold#DATAFLOW_VERBS`'s `joined` handle; the feed aggregates for rendering and never correlates by hand.
- Growth: a new evidence vocabulary joins as one case; the feed, reads, and plan absorb it with zero edits beyond the demanded record rows.

```typescript
declare namespace Feed {
  type Column = typeof _Column.Type
  type Document = DocumentRef
  type Entry = Data.TaggedEnum<{
    Receipt: { readonly envelope: ReceiptEnvelope }
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
```

## [4]-[FEED_FOLD]

[FEED_FOLD]:
- Owner: `Feed.State` — the ordered rows (`SortedMap` keyed by the composed `[stamp, subject, tag]` order) plus the coalescing index (`HashMap` from `[subject, tag]` to the live row key); the absorb step writes both in one advance, so ordering and coalescing are two reads of one fold state, never two structures maintained by different call sites.
- Law: the row key order composes event stamp, then subject, then tag — total over distinct entries — so concurrent same-stamp evidence from different subjects interleaves deterministically and the feed order is reproducible from any replay: REPLAY_LAW at feed altitude.
- Law: the policy row is data — `cap` bounds the census with a single head eviction per insert (the census grows by at most one, so eviction is one `SortedMap.remove` of `headOption`, never a sweep), `coalesce` toggles replacement of the previous same-subject same-kind row.
- Law: coalescing keeps the greatest key per slot — an arrival whose key does not outrank the live pointer is superseded evidence the absorb drops — so absorb order cannot bury newer evidence and the coalescing lane stays commutative; a distinct-entry tie on one composed key is a stamp collision the HLC-monotone mint excludes.
- Law: the feed merges as an instance — union of rows re-absorbed under the same policy — commutative and idempotent because row identity is the composed key and eviction always drops the global minimum, so lane-partitioned feeds fuse under `Merge.fold` like every other lattice, and `Feed.plan` partitions by tenant lane so every altitude runs the identical fold; `alike` compares rank-paired entries key-inclusively, so state equality never leans on the key-derives-from-entry invariant.
- Boundary: a serving edge frames the fold's live view; the durable feed projection is the data branch binding `Feed.plan`; AppUi rendering consumes served rows and never re-sorts.

```typescript
declare namespace Feed {
  type Key = readonly [stamp: Hlc, subject: string, kind: Entry["_tag"]]
  type Policy = { readonly cap: number; readonly coalesce: boolean }
  type State = {
    readonly rows: SortedMap.SortedMap<Key, Entry>
    readonly live: HashMap.HashMap<readonly [string, Entry["_tag"]], Key>
  }
  type Shape = {
    readonly Entry: typeof _Entry
    readonly Document: typeof DocumentRef
    readonly at: (entry: Entry) => Hlc
    readonly lane: (entry: Entry) => TenantContext
    readonly subject: (entry: Entry) => string
    readonly empty: State
    readonly absorb: (policy: Policy) => (state: State, entry: Entry) => State
    readonly merge: (policy: Policy) => Merge.Instance<State>
    readonly plan: (policy: Policy) => Fold.Plan<Entry, TenantContext, State>
    readonly window: (state: State, bounds: { readonly from: Option.Option<Hlc>; readonly until: Option.Option<Hlc> }) => Chunk.Chunk<Entry>
    readonly recent: (state: State, take: number) => Chunk.Chunk<Entry>
  }
}

const _byKey: Order.Order<Feed.Key> = Order.combine(
  Order.mapInput(Hlc.Order, (key: Feed.Key) => key[0]),
  Order.combine(
    Order.mapInput(Order.string, (key: Feed.Key) => key[1]),
    Order.mapInput(Order.string, (key: Feed.Key) => key[2]),
  ),
)

const _placed = (state: Feed.State, slot: readonly [string, Feed.Entry["_tag"]], key: Feed.Key, entry: Feed.Entry): Feed.State => ({
  rows: SortedMap.set(state.rows, key, entry),
  live: HashMap.set(state.live, slot, key),
})

const _evicted = (policy: Feed.Policy, state: Feed.State): Feed.State =>
  SortedMap.size(state.rows) <= policy.cap
    ? state
    : Option.match(SortedMap.headOption(state.rows), {
        onNone: () => state,
        onSome: ([head, entry]) => ({
          rows: SortedMap.remove(state.rows, head),
          live: Option.match(HashMap.get(state.live, Data.tuple(_subject(entry), entry._tag)), {
            onNone: () => state.live,
            onSome: (pointer) =>
              Equal.equals(pointer, head)
                ? HashMap.remove(state.live, Data.tuple(_subject(entry), entry._tag))
                : state.live,
          }),
        }),
      })

const _absorb = (policy: Feed.Policy) => (state: Feed.State, entry: Feed.Entry): Feed.State => {
  const slot = Data.tuple(_subject(entry), entry._tag)
  const key: Feed.Key = Data.tuple(_at(entry), _subject(entry), entry._tag)
  return policy.coalesce
    ? Option.match(HashMap.get(state.live, slot), {
        onNone: () => _evicted(policy, _placed(state, slot, key, entry)),
        onSome: (prior) =>
          Order.lessThan(_byKey)(prior, key)
            ? _evicted(policy, _placed({ rows: SortedMap.remove(state.rows, prior), live: state.live }, slot, key, entry))
            : state,
      })
    : _evicted(policy, _placed(state, slot, key, entry))
}

const _empty: Feed.State = { rows: SortedMap.empty(_byKey), live: HashMap.empty() }
```

## [5]-[FEED_READS]

[FEED_READS]:
- Owner: the read family — `window` folds the rows inside optional `Hlc` bounds, `recent` takes the newest slice from the tail; both are projections of the ordered state, so no read allocates beyond its answer and no read observes an unordered intermediate.
- Law: reads never expose the coalescing index — consumers see entries in event-time order only; the index is fold-interior state co-located with its owner.
- Growth: a new read (per-kind lane, subject history) is one projection member over the same two structures.

```typescript
const Feed: Feed.Shape = {
  Entry: _Entry,
  Document: DocumentRef,
  at: _at,
  lane: _lane,
  subject: _subject,
  empty: _empty,
  absorb: _absorb,
  merge: (policy) =>
    Merge.instance({
      combine: Semigroup.make((self: Feed.State, that: Feed.State) =>
        SortedMap.reduce(that.rows, self, (acc, entry) => _absorb(policy)(acc, entry))),
      posture: { commutative: true, idempotent: true },
      alike: (self, that) =>
        SortedMap.size(self.rows) === SortedMap.size(that.rows)
        && Chunk.every(
          Chunk.zip(Chunk.fromIterable(self.rows), Chunk.fromIterable(that.rows)),
          ([left, right]) => Equal.equals(left[0], right[0]) && Equal.equals(left[1], right[1]),
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
  window: (state, bounds) =>
    Chunk.filter(
      Chunk.fromIterable(SortedMap.values(state.rows)),
      (entry) =>
        Option.match(bounds.from, { onNone: () => true, onSome: (from) => !Order.lessThan(Hlc.Order)(_at(entry), from) })
        && Option.match(bounds.until, { onNone: () => true, onSome: (until) => Order.lessThan(Hlc.Order)(_at(entry), until) }),
    ),
  recent: (state, take) => Chunk.takeRight(Chunk.fromIterable(SortedMap.values(state.rows)), take),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Feed }
```
