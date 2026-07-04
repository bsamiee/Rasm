# [CORE_COMMIT]

The commit-graph anti-entropy owner: `Commit` — the content-keyed commit class carrying its parents, causal vector, stamp, and author — with the branch head shape and the Merkle summary riding it as statics, so the whole commit-graph vocabulary travels one import and the interchange decode seam targets one owner family. The commit/branch/Merkle wire shapes C# mints at its persistence version plane decode INTO these owners, and the interior speaks exactly this vocabulary with zero TS re-mints. Merkle comparison is a tier-descent fold that touches only the buckets whose digests differ — comparison cost proportional to the divergence — and summary construction is one bottom-up fold delegating every tier digest to the `contentKey` mint, so building and comparing summaries share one identity and one fanout law. The module is `core/src/state/commit.ts`; a deeper anti-entropy question is a field row on the summary plus a projection, never a second summary shape.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                          | [PUBLIC]                                    |
| :-----: | :--------------- | :----------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `COMMIT_OWNER`   | the content-keyed commit class, the branch head shape, its orders  | `Commit`, `Commit.Branch`, `Commit.byStamp`     |
|  [02]   | `MERKLE_SUMMARY` | summary construction, fanout law, and the divergence descent       | `Commit.Merkle`, `Commit.summarize`, `Commit.diverges` |

## [2]-[COMMIT_OWNER]

[COMMIT_OWNER]:
- Owner: `Commit` — the content-keyed commit class (`key`, `parents`, `vector`, `stamp`, `author`) — one decoded shape for the whole version plane, with `byStamp` as the total commit order over the `Hlc` stamp.
- Law: `Commit.Branch` is a pure-data head pointer (`Schema.Struct`) — name brand, head key, vector — behavior never accretes on it; a branch gaining behavior is a `Schema.Class` promotion of one declaration.
- Law: identity composes, never re-mints — `key` and `parents` are `contentKey` brands, `vector` is the `causal` lattice value, `author` is `Vector.Replica` — so a commit crossing the anti-entropy seam carries exactly the identities every other plane already speaks.
- Law: transitive commit-graph reachability (ancestor closure, merge-base descent) is `fold#DATAFLOW_VERBS`'s fixpoint lane fed with `[parent, child]` edges — this owner declares the shapes and the summary fold, never a hand frontier over the graph.
- Growth: a new commit axis (signer, message key) is one field; a new graph read is one static composing the existing fields.
- Boundary: the interchange codec owns the wire twins; the durable journal persists commits and anchors snapshots — both consume this owner and this owner imports neither.
- Packages: `effect` (`Schema`, `Array`, `Effect`, `Order`); `../value/contentKey.ts` (`ContentKey`, `Digest`); `../value/clock.ts` (`Hlc`); `./causal.ts` (`Vector`).

```typescript
import { Array, Effect, Order, Schema } from "effect"
import { Hlc } from "../value/clock.ts"
import { ContentKey, Digest } from "../value/contentKey.ts"
import { Vector } from "./causal.ts"

class Commit extends Schema.Class<Commit>("Commit")({
  key: ContentKey,
  parents: Schema.Array(ContentKey),
  vector: Vector,
  stamp: Hlc,
  author: Vector.Replica,
}) {
  static readonly byStamp: Order.Order<Commit> = Order.mapInput(Hlc.Order, (commit: Commit) => commit.stamp)
  static readonly Branch = Schema.Struct({
    name: Schema.NonEmptyString.pipe(Schema.brand("BranchName")),
    head: ContentKey,
    vector: Vector,
  })
}

declare namespace Commit {
  type Branch = typeof Commit.Branch.Type
  type Merkle = typeof _Merkle.Type
}
```

## [3]-[MERKLE_SUMMARY]

[MERKLE_SUMMARY]:
- Owner: the summary algebra riding `Commit` — `Commit.Merkle` summarizes a journal range as digest tiers over a declared fanout, `Commit.summarize` builds the tiers bottom-up from the leaf keys, and `Commit.diverges` descends from the root tier keeping only child buckets under diverging parents, so comparison cost is proportional to the divergence and equal roots answer in one row — the anti-entropy read the sync protocol consumes.
- Law: tier digests are `ContentKey` — every parent digest mints through `Digest.mint("content", ...)` over its child bucket's canonical bytes, the one `XxHash128` seed-zero identity, so a locally built summary and a C#-decoded summary compare bucket-for-bucket with no normalize step (invariant: one mint, delegating sites only).
- Law: fanout is summary identity — two summaries compare only at equal fanout, and a mismatched pair answers every leaf bucket of `self`, the full-sync verdict, because bucket coordinates under different fanouts name different ranges; `summarize` and `diverges` read the same fanout field, so build and compare cannot disagree.
- Law: tiers order root-first — `tiers[0]` is the root row and the last tier is the leaf census — and construction folds leaf-to-root with each pass prepending, so the stored order is the descent order and no reader reverses.
- Exemption: `_encoded` is a marked kernel — the `TextEncoder` byte crossing is the platform-forced seam that turns a bucket's joined hex into digest input; only the immutable byte array leaves, and the implementer carries the `// BOUNDARY ADAPTER` mark on its first line. The `diverges` tier descent is a measured statement kernel — candidate narrowing mutates only the local frontier arrays and the accumulator dies at the return.
- Growth: a deeper anti-entropy question (bucket counts, range stamps) is a field row on `Commit.Merkle` plus a `diverges` projection; a divergent-bucket leaf fetch is the sync protocol's read over the answered coordinates.
- Boundary: which journal ranges are summarized, summary caching, and the sync transport are data-branch and runtime-branch concerns; this owner fixes construction and comparison only.

```typescript
const _Merkle = Schema.Struct({
  fanout: Schema.Int.pipe(Schema.between(2, 256)),
  tiers: Schema.NonEmptyArray(Schema.Array(ContentKey)),
})

const _encoded = (bucket: ReadonlyArray<ContentKey>): Uint8Array => {
  return new TextEncoder().encode(bucket.join(""))
}

const _lifted = (tier: ReadonlyArray<ContentKey>, fanout: number): Effect.Effect<ReadonlyArray<ContentKey>> =>
  Effect.forEach(Array.chunksOf(tier, fanout), (bucket) => Digest.mint("content", _encoded(bucket)))

const _tiered = (
  tier: ReadonlyArray<ContentKey>,
  fanout: number,
  built: ReadonlyArray<ReadonlyArray<ContentKey>>,
): Effect.Effect<Array.NonEmptyReadonlyArray<ReadonlyArray<ContentKey>>> =>
  tier.length <= 1
    ? Effect.succeed([tier, ...built])
    : Effect.flatMap(_lifted(tier, fanout), (up) => _tiered(up, fanout, [tier, ...built]))

const _summarize = (
  leaves: Array.NonEmptyReadonlyArray<ContentKey>,
  fanout: number,
): Effect.Effect<Commit.Merkle> =>
  Effect.map(_tiered(leaves, fanout, []), (tiers) => _Merkle.make({ fanout, tiers }))

const _diverges = (self: Commit.Merkle, that: Commit.Merkle): ReadonlyArray<number> => {
  if (self.fanout !== that.fanout) return self.tiers[self.tiers.length - 1]!.map((_digest, at) => at)
  const tiers = Math.max(self.tiers.length, that.tiers.length)
  let candidates: ReadonlyArray<number> = [0]
  for (let tier = 0; tier < tiers; tier += 1) {
    const left = self.tiers[tier] ?? []
    const right = that.tiers[tier] ?? []
    const width = Math.max(left.length, right.length)
    candidates = candidates.flatMap((parent) => {
      const from = tier === 0 ? parent : parent * self.fanout
      const until = tier === 0 ? parent + 1 : from + self.fanout
      const spread: Array<number> = []
      for (let at = from; at < until && at < width; at += 1) {
        if (left[at] !== right[at]) spread.push(at)
      }
      return spread
    })
    if (candidates.length === 0) return []
  }
  return candidates
}

declare namespace Commit {
  type Summarize = (leaves: Array.NonEmptyReadonlyArray<ContentKey>, fanout: number) => Effect.Effect<Commit.Merkle>
  type Diverges = (self: Commit.Merkle, that: Commit.Merkle) => ReadonlyArray<number>
}

interface CommitStatics {
  readonly Merkle: typeof _Merkle
  readonly summarize: Commit.Summarize
  readonly diverges: Commit.Diverges
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Commit }
```
