# [STATE_VECTOR]

`causal/vector.ts` owns the causality shapes: `Vector` — the per-replica version vector whose comparison is the four-way causal ordering — and `Commit` — the content-keyed commit owner carrying its branch head and Merkle summary statics for anti-entropy comparison. The commit/branch/version-vector/Merkle wire shapes C# mints at `Rasm.Persistence/Version/commits` decode through `wire/codec/version` INTO these owners (seam `PE:46`), so the interior speaks exactly this vocabulary and no TS re-mint of a wire shape exists. Join and meet are `Merge` instances declared on the owner — the same lattice values `causal/order` folds into delivery frontiers — and Merkle divergence is a tier-descent fold that touches only the buckets whose digests differ, never a full-leaf scan.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                            | [SURFACE]                                       |
| :-----: | :--------------- | :----------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | [VECTOR_OWNER]   | the version-vector class, four-way comparison, join/meet instances | `Vector`, `Vector.compare/join/meet/observe/covers` |
|  [02]   | [COMMIT_FAMILY]  | commit/branch shapes and the Merkle summary with divergence fold   | `Commit`, `Commit.Branch/Merkle/byStamp/diverges`  |

## [2]-[VECTOR_OWNER]

- Owner: `Vector` — one `Schema.Class` whose `clocks` field is the decoded `HashMap` of replica counters; comparison, dominance, increment, and the two lattice instances ride the class as statics, so one import carries the shape, the codec target, and the whole causal-order algebra.
- Packages: `effect` (`Schema`, `HashMap`, `Order`, `Option`, `Number`); `@effect/typeclass` (`Semigroup.make`); `@rasm/ts/kernel` (`ContentKey`, `Hlc`); `../crdt/merge.ts` (`Merge.Instance` for join/meet).
- Law: `Vector.compare` answers the happened-before question structurally — `"before"` when strictly dominated, `"after"` when strictly dominating, `"equal"` on identical clocks, `"concurrent"` when each side carries a count the other lacks — and the ordering vocabulary anchors on the interior tuple so `causal/order` and `query/window` derive the same literal union.
- Law: `join` is the pointwise-max lattice (`posture` semilattice, empty `Vector.zero`) — the merge every delivery advances `seen` by; `meet` is the pointwise-min GLB over the key union with absent-as-zero, so a replica that never acked pins the frontier at zero rather than being skipped — the stability-frontier semantics `causal/order` requires.
- Law: replica identity is the interior `_Replica` brand (`ReplicaId`) reaching consumers only as `Vector.Replica` — one spelling for presence actors, delivery origins, and ack keys; a free-floating replica-id export is the named defect.
- Growth: a new causal comparison read is a static on `Vector`; a new ordering verdict is a `_ORDERINGS` row — consumers dispatching on `Vector.Ordering` break loudly at the missing arm.
- Boundary: `wire/codec/version` decodes the C# version-vector wire into `Vector`; `store/journal` persists commits — both consume this owner and this owner imports neither.

```typescript
import * as Semigroup from "@effect/typeclass/Semigroup"
import { HashMap, Number, Option, Order, Schema } from "effect"
import { ContentKey, Hlc } from "@rasm/ts/kernel"
import { Merge } from "../crdt/merge.ts"

const _ORDERINGS = ["before", "after", "equal", "concurrent"] as const

const _Replica = Schema.NonEmptyString.pipe(Schema.brand("ReplicaId"))

declare namespace Vector {
  type Ordering = (typeof _ORDERINGS)[number]
  type Replica = typeof _Replica.Type
}

const _at = (clocks: HashMap.HashMap<Vector.Replica, number>, replica: Vector.Replica): number =>
  Option.getOrElse(HashMap.get(clocks, replica), () => 0)

const _dominates = (
  self: HashMap.HashMap<Vector.Replica, number>,
  that: HashMap.HashMap<Vector.Replica, number>,
): boolean => HashMap.reduce(that, true, (holds, count, replica) => holds && count <= _at(self, replica))

const _pointwise = (pick: (left: number, right: number) => number) => (self: Vector, that: Vector): Vector =>
  new Vector({
    clocks: HashMap.reduce(
      HashMap.reduce(that.clocks, self.clocks, (acc, _count, replica) => HashMap.set(acc, replica, _at(acc, replica))),
      HashMap.empty<Vector.Replica, number>(),
      (acc, _count, replica) =>
        HashMap.set(acc, replica, pick(_at(self.clocks, replica), _at(that.clocks, replica))),
    ),
  })

const _lattice = (pick: (left: number, right: number) => number, empty: Option.Option<Vector>): Merge.Instance<Vector> =>
  Merge.instance({
    combine: Semigroup.make(_pointwise(pick)),
    posture: { commutative: true, idempotent: true },
    alike: (self, that) => Vector.compare(self, that) === "equal",
    empty,
  })

class Vector extends Schema.Class<Vector>("Vector")({
  clocks: Schema.HashMap({ key: _Replica, value: Schema.Int.pipe(Schema.nonNegative()) }),
}) {
  static readonly zero: Vector = new Vector({ clocks: HashMap.empty() })
  static compare(self: Vector, that: Vector): Vector.Ordering {
    const forward = _dominates(self.clocks, that.clocks)
    const backward = _dominates(that.clocks, self.clocks)
    return forward && backward ? "equal" : forward ? "after" : backward ? "before" : "concurrent"
  }
  static covers(self: Vector, that: Vector): boolean {
    return _dominates(self.clocks, that.clocks)
  }
  static observe(self: Vector, replica: Vector.Replica): Vector {
    return new Vector({ clocks: HashMap.set(self.clocks, replica, _at(self.clocks, replica) + 1) })
  }
  static readonly join: Merge.Instance<Vector> = _lattice(Number.max, Option.some(Vector.zero))
  static readonly meet: Merge.Instance<Vector> = _lattice(Number.min, Option.none())
}
```

## [3]-[COMMIT_FAMILY]

- Owner: `Commit` — the content-keyed commit class (`key`, `parents`, `vector`, `stamp`, `author`) with the branch shape and Merkle summary riding it as statics, so the whole commit-graph vocabulary travels one import and the decode seam targets one owner family.
- Law: `Commit.Branch` is a pure-data head pointer (`Schema.Struct`) — name brand, head key, vector — behavior never accretes on it; a branch with behavior would be a `Schema.Class` case and that promotion is one declaration edit.
- Law: `Commit.Merkle` summarizes a journal range as digest tiers over a declared fanout; `Commit.diverges` descends from the root tier keeping only child buckets under diverging parents, so comparison cost is proportional to the divergence, and equal roots answer in one row — the anti-entropy read the sync protocol consumes.
- Law: fanout is summary identity — two summaries compare only at equal fanout, and a mismatched pair answers every leaf bucket of `self`, the full-sync verdict, because bucket coordinates under different fanouts name different ranges.
- Law: tier digests are `ContentKey` — the one `XxHash128` seed-zero identity minted in `kernel/identity`; this owner composes the brand and never re-mints (invariant: one mint, delegating sites only).
- Exemption: the `diverges` tier descent is a measured statement kernel — candidate narrowing mutates only the local frontier arrays and the accumulator dies at the return.
- Growth: a deeper anti-entropy question (bucket counts, range stamps) is a field row on `Commit.Merkle` plus a `diverges` projection — never a second summary shape.
- Boundary: `wire/codec/version` owns the wire twin; `store/journal/snapshot` consumes `Commit` for snapshot anchoring; Merkle closure over commit graphs (transitive reachability) is `fold/replay`'s `iterate` lane.

```typescript
class Commit extends Schema.Class<Commit>("Commit")({
  key: ContentKey,
  parents: Schema.Array(ContentKey),
  vector: Vector,
  stamp: Hlc,
  author: _Replica,
}) {
  static readonly byStamp: Order.Order<Commit> = Order.mapInput(Hlc.Order, (commit: Commit) => commit.stamp)
  static readonly Branch = Schema.Struct({
    name: Schema.NonEmptyString.pipe(Schema.brand("BranchName")),
    head: ContentKey,
    vector: Vector,
  })
  static readonly Merkle = Schema.Struct({
    fanout: Schema.Int.pipe(Schema.between(2, 256)),
    tiers: Schema.NonEmptyArray(Schema.Array(ContentKey)),
  })
  static diverges(self: Commit.Merkle, that: Commit.Merkle): ReadonlyArray<number> {
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
}

declare namespace Commit {
  type Branch = typeof Commit.Branch.Type
  type Merkle = typeof Commit.Merkle.Type
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Commit, Vector }
```
