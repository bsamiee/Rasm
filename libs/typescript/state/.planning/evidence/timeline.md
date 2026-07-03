# [STATE_TIMELINE]

`evidence/timeline.ts` owns the evidence feed: one process-local `Entry` family wrapping the three evidence vocabularies — receipt envelopes, progress marks, availability snapshots — and the `Timeline` fold that maintains an `Hlc`-ordered, capacity-bounded, coalescing feed per lane. The feed is the `EvidenceFeed`/`EvidenceTimeline` projection the C# AppUi renders (seam `AU:61`) re-owned as a fold: insertion is a `SortedMap` write under a composed event-time order — never an array re-sort — eviction is a single head drop under the cap, coalescing replaces the previous entry of the same subject and kind instead of stacking repeats, and the whole feed is one more `Fold.Plan` row so live views and durable projections consume it like any other fold.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                          | [SURFACE]                                           |
| :-----: | :-------------- | :----------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | [ENTRY_FAMILY]  | the tagged evidence entry union and its projections                | `Timeline.Entry`, `Timeline.at/lane/subject`             |
|  [02]   | [FEED_FOLD]     | the ordered feed state, absorb step, policy row, merge, fold plan  | `Timeline.Feed`, `Timeline.absorb/.merge/.plan`          |
|  [03]   | [FEED_READS]    | window and lane reads over the folded feed                         | `Timeline.window`, `Timeline.recent`                     |

## [2]-[ENTRY_FAMILY]

- Owner: `Timeline.Entry` — a `Data.taggedEnum` over `Receipt`, `Progress`, and `Shift` cases wrapping the sibling owners whole; the family is process-local projection vocabulary (the members already own their wire twins), so the enum form costs zero codecs and carries structural equality for coalescing; the type rides the owner's merged namespace, so `Timeline` is the module's one export.
- Packages: `effect` (`Chunk`, `Data`, `Equal`, `HashMap`, `Option`, `Order`, `SortedMap`); `@effect/typeclass` (`Semigroup.make`); `@rasm/ts/kernel` (`Hlc`, `TenantContext`); `../crdt/merge.ts`; `../fold/algebra.ts`; the three sibling evidence owners.
- Law: projections are total record dispatches — `_at` (the entry's event stamp), `_lane` (the tenant lane), `_subject` (the coalescing coordinate: command key, operation key, or tenant scope) — one `$match` record each, so a new entry kind is one case plus three record rows the compiler demands.
- Growth: a new evidence vocabulary joins as one case; the feed, reads, and plan absorb it with zero edits beyond the demanded record rows.

```typescript
import * as Semigroup from "@effect/typeclass/Semigroup"
import { Chunk, Data, Equal, HashMap, Option, Order, SortedMap } from "effect"
import { Hlc, TenantContext } from "@rasm/ts/kernel"
import { Merge } from "../crdt/merge.ts"
import { Fold } from "../fold/algebra.ts"
import { Availability } from "./availability.ts"
import { ProgressMark } from "./progress.ts"
import { ReceiptEnvelope } from "./receipt.ts"

declare namespace Timeline {
  type Entry = Data.TaggedEnum<{
    Receipt: { readonly envelope: ReceiptEnvelope }
    Progress: { readonly mark: ProgressMark }
    Shift: { readonly snapshot: Availability }
  }>
}

const _Entry = Data.taggedEnum<Timeline.Entry>()

const _at: (entry: Timeline.Entry) => Hlc = _Entry.$match({
  Receipt: ({ envelope }) => envelope.stamp,
  Progress: ({ mark }) => mark.stamp,
  Shift: ({ snapshot }) => snapshot.since,
})

const _lane: (entry: Timeline.Entry) => TenantContext = _Entry.$match({
  Receipt: ({ envelope }) => envelope.tenant,
  Progress: ({ mark }) => mark.tenant,
  Shift: ({ snapshot }) => snapshot.tenant,
})

const _subject: (entry: Timeline.Entry) => string = _Entry.$match({
  Receipt: ({ envelope }) => envelope.command,
  Progress: ({ mark }) => mark.operation,
  Shift: ({ snapshot }) => snapshot.tenant.scope,
})
```

## [3]-[FEED_FOLD]

- Owner: `Timeline.Feed` — the ordered rows (`SortedMap` keyed by the composed `[stamp, subject, tag]` order) plus the coalescing index (`HashMap` from `[subject, tag]` to the live row key); the absorb step writes both in one advance, so ordering and coalescing are two reads of one fold state, never two structures maintained by different call sites.
- Law: the row key order composes event stamp, then subject, then tag — total over distinct entries — so concurrent same-stamp evidence from different subjects interleaves deterministically and the feed order is reproducible from any replay (the REPLAY_LAW consequence at feed altitude).
- Law: the policy row is data — `cap` bounds the census with a single head eviction per insert (the census grows by at most one, so eviction is one `SortedMap.remove` of `headOption`, never a sweep), `coalesce` toggles replacement of the previous same-subject same-kind row.
- Law: coalescing keeps the greatest key per slot — an arrival whose key does not outrank the live pointer is superseded evidence the absorb drops — so absorb order cannot bury newer evidence and the coalescing lane stays commutative; a distinct-entry tie on one composed key is a stamp collision the HLC-monotone mint excludes.
- Law: the feed merges as an instance — union of rows re-absorbed under the same policy — commutative and idempotent because row identity is the composed key and eviction always drops the global minimum, so lane-partitioned feeds fuse under `Merge.fold` like every other lattice.
- Boundary: `query/live` serves the feed as a live view; the durable feed projection is `store/project` binding `Timeline.plan`; AppUi rendering consumes the served rows and never re-sorts.

```typescript
declare namespace Timeline {
  type Key = readonly [stamp: Hlc, subject: string, kind: Entry["_tag"]]
  type Policy = { readonly cap: number; readonly coalesce: boolean }
  type Feed = {
    readonly rows: SortedMap.SortedMap<Key, Entry>
    readonly live: HashMap.HashMap<readonly [string, Entry["_tag"]], Key>
  }
  type Shape = {
    readonly Entry: typeof _Entry
    readonly at: (entry: Entry) => Hlc
    readonly lane: (entry: Entry) => TenantContext
    readonly subject: (entry: Entry) => string
    readonly empty: Feed
    readonly absorb: (policy: Policy) => (feed: Feed, entry: Entry) => Feed
    readonly merge: (policy: Policy) => Merge.Instance<Feed>
    readonly plan: (policy: Policy) => Fold.Plan<Entry, TenantContext, Feed>
    readonly window: (feed: Feed, bounds: { readonly from: Option.Option<Hlc>; readonly until: Option.Option<Hlc> }) => Chunk.Chunk<Entry>
    readonly recent: (feed: Feed, take: number) => Chunk.Chunk<Entry>
  }
}

const _placed = (feed: Timeline.Feed, slot: readonly [string, Timeline.Entry["_tag"]], key: Timeline.Key, entry: Timeline.Entry): Timeline.Feed => ({
  rows: SortedMap.set(feed.rows, key, entry),
  live: HashMap.set(feed.live, slot, key),
})

const _byKey: Order.Order<Timeline.Key> = Order.combine(
  Order.mapInput(Hlc.Order, (key: Timeline.Key) => key[0]),
  Order.combine(
    Order.mapInput(Order.string, (key: Timeline.Key) => key[1]),
    Order.mapInput(Order.string, (key: Timeline.Key) => key[2]),
  ),
)

const _evicted = (policy: Timeline.Policy, feed: Timeline.Feed): Timeline.Feed =>
  SortedMap.size(feed.rows) <= policy.cap
    ? feed
    : Option.match(SortedMap.headOption(feed.rows), {
        onNone: () => feed,
        onSome: ([head, entry]) => ({
          rows: SortedMap.remove(feed.rows, head),
          live: Option.match(HashMap.get(feed.live, Data.tuple(_subject(entry), entry._tag)), {
            onNone: () => feed.live,
            onSome: (pointer) =>
              Equal.equals(pointer, head)
                ? HashMap.remove(feed.live, Data.tuple(_subject(entry), entry._tag))
                : feed.live,
          }),
        }),
      })

const _absorb = (policy: Timeline.Policy) => (feed: Timeline.Feed, entry: Timeline.Entry): Timeline.Feed => {
  const slot = Data.tuple(_subject(entry), entry._tag)
  const key: Timeline.Key = Data.tuple(_at(entry), _subject(entry), entry._tag)
  return policy.coalesce
    ? Option.match(HashMap.get(feed.live, slot), {
        onNone: () => _evicted(policy, _placed(feed, slot, key, entry)),
        onSome: (prior) =>
          Order.lessThan(_byKey)(prior, key)
            ? _evicted(policy, _placed({ rows: SortedMap.remove(feed.rows, prior), live: feed.live }, slot, key, entry))
            : feed,
      })
    : _evicted(policy, _placed(feed, slot, key, entry))
}

const _empty: Timeline.Feed = { rows: SortedMap.empty(_byKey), live: HashMap.empty() }
```

## [4]-[FEED_READS]

- Owner: the read family — `window` folds the rows inside optional `Hlc` bounds, `recent` takes the newest slice from the tail; both are projections of the ordered state, so no read allocates beyond its answer and no read observes an unordered intermediate.
- Law: reads never expose the coalescing index — consumers see entries in event-time order only; the index is fold-interior state co-located with its owner.
- Growth: a new read (per-kind lane, subject history) is one projection member over the same two structures.

```typescript
const Timeline: Timeline.Shape = {
  Entry: _Entry,
  at: _at,
  lane: _lane,
  subject: _subject,
  empty: _empty,
  absorb: _absorb,
  merge: (policy) =>
    Merge.instance({
      combine: Semigroup.make((self: Timeline.Feed, that: Timeline.Feed) =>
        SortedMap.reduce(that.rows, self, (acc, entry) => _absorb(policy)(acc, entry))),
      posture: { commutative: true, idempotent: true },
      alike: (self, that) =>
        SortedMap.size(self.rows) === SortedMap.size(that.rows)
        && Chunk.every(
          Chunk.zip(
            Chunk.fromIterable(SortedMap.values(self.rows)),
            Chunk.fromIterable(SortedMap.values(that.rows)),
          ),
          ([left, right]) => Equal.equals(left, right),
        ),
      empty: Option.some(_empty),
    }),
  plan: (policy) =>
    Fold.plan({
      name: "evidence/timeline",
      key: _lane,
      lift: (entry) => _absorb(policy)(_empty, entry),
      merge: Timeline.merge(policy),
    }),
  window: (feed, bounds) =>
    Chunk.fromIterable(SortedMap.values(feed.rows)).pipe(
      Chunk.filter((entry) =>
        Option.match(bounds.from, { onNone: () => true, onSome: (from) => !Order.lessThan(Hlc.Order)(_at(entry), from) })
        && Option.match(bounds.until, { onNone: () => true, onSome: (until) => Order.lessThan(Hlc.Order)(_at(entry), until) })),
    ),
  recent: (feed, take) => Chunk.takeRight(Chunk.fromIterable(SortedMap.values(feed.rows)), take),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Timeline }
```
