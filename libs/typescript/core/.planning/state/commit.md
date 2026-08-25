# [CORE_COMMIT]

The commit-graph anti-entropy owner: `Commit` — the content-keyed commit class carrying its parents, causal vector, stamp, and author — with the branch head shape and the Merkle summary machinery riding it as statics, so the whole commit-graph vocabulary travels one import and the interchange decode seam targets one owner family. The commit/branch/Merkle wire shapes C# mints at its persistence version plane decode INTO these owners, and the interior speaks exactly this vocabulary with zero TS re-mints. Merkle comparison is a tier-descent fold that touches only the buckets whose digests differ — comparison cost proportional to the divergence — and summary construction canonicalizes the leaf set (sorted, deduplicated) then folds bottom-up delegating every tier digest to the `value/contentKey` mint, so building and comparing summaries share one identity, one canonical order, and one fanout law. The module is `core/src/state/commit.ts`; a deeper anti-entropy question is a field row on the summary plus a projection, never a second summary shape.

## [01]-[INDEX]

- [02]-[SUMMARY_MINT]: the Merkle shape, tier construction over the content mint, divergence; interior `_Merkle`, `_summarize`, `_diverges`.
- [03]-[COMMIT_OWNER]: the commit class assembling shapes, orders, and the summary algebra; `Commit`.

## [02]-[SUMMARY_MINT]

[SUMMARY_MINT]:

```typescript
import { Array, Data, Effect, Equal, Number, Option, Order, pipe, Schema } from "effect"
import { Clock } from "../value/clock.ts"
import { Digest } from "../value/contentKey.ts"
import { Fault } from "../value/fault.ts"
import { Causal } from "./causal.ts"

const _Fanout = Schema.Int.pipe(Schema.between(2, 256), Schema.brand("CommitFanout"))

class _Merkle extends Schema.Class<_Merkle>("Commit.Merkle")(
  Schema.Struct({
    fanout: _Fanout,
    tiers: Schema.NonEmptyArray(Schema.NonEmptyArray(Digest.Key.content)),
  }).pipe(Schema.filter(({ fanout, tiers }) =>
    Array.lastNonEmpty(tiers).length === 1
    && (tiers.length === 1
      || Array.every(Array.range(0, tiers.length - 2), (at) => tiers[at + 1].length === Math.ceil(tiers[at].length / fanout))))),
) {}

type _DivergenceValue = Data.TaggedEnum<{
  Exact: { readonly buckets: ReadonlyArray<number> }
  Full: { readonly left: ReadonlyArray<number>; readonly right: ReadonlyArray<number> }
}>
const _Divergence = Data.taggedEnum<_DivergenceValue>()

const _summaryFamily = Fault.Class.family(["tier", "root"] as const, {
  tier: Fault.Class.row({
    class: "invalid",
    leg: "merkle",
    detail: Schema.Struct({
      at: Schema.Int.pipe(Schema.nonNegative()),
      expected: Digest.Key.content,
      actual: Digest.Key.content,
    }),
    render: ({ actual, at, expected }) => `tier ${at} diverges; rebuild roots at ${expected}, summary declares ${actual}`,
  }),
  root: Fault.Class.row({
    class: "invalid",
    leg: "merkle",
    detail: Schema.Struct({ expected: Digest.Key.content, actual: Digest.Key.content }),
    render: ({ actual, expected }) => `every shared tier agrees while the root differs; rebuild ${expected}, summary ${actual}`,
  }),
})

class _SummaryFault extends Schema.TaggedError<_SummaryFault>()("CommitSummaryFault", {
  case: _summaryFamily.payload,
}) {
  get class(): Fault.Class.Kind {
    return _summaryFamily.classOf(this.case.reason)
  }
  override get message(): string {
    return _summaryFamily.render(this.case)
  }
}

const _utf8 = new TextEncoder()

const _byLeaf: Order.Order<Digest.Key<"content">> = Order.string

const _encoded = (bucket: ReadonlyArray<Digest.Key<"content">>): Uint8Array =>
  _utf8.encode(Array.join(Array.map(bucket, (key) => `${key.length}:${key}`), ""))

const _lifted = (
  tier: ReadonlyArray<Digest.Key<"content">>,
  fanout: number,
): Effect.Effect<ReadonlyArray<Digest.Key<"content">>> =>
  Effect.forEach(Array.chunksOf(tier, fanout), (bucket) => Digest.mint("content", _encoded(bucket)), { concurrency: "inherit" })

const _tiered = (
  tier: ReadonlyArray<Digest.Key<"content">>,
  fanout: Schema.Schema.Type<typeof _Fanout>,
  built: ReadonlyArray<ReadonlyArray<Digest.Key<"content">>>,
): Effect.Effect<Array.NonEmptyReadonlyArray<ReadonlyArray<Digest.Key<"content">>>> =>
  tier.length <= 1
    ? Effect.succeed([...built, tier])
    : Effect.flatMap(_lifted(tier, fanout), (up) => _tiered(up, fanout, [...built, tier]))

const _summarize = (
  leaves: Array.NonEmptyReadonlyArray<Digest.Key<"content">>,
  fanout: Schema.Schema.Type<typeof _Fanout>,
): Effect.Effect<_Merkle> =>
  Effect.map(
    _tiered(pipe(leaves, Array.sort(_byLeaf), Array.dedupe), fanout, []),
    (tiers) => new _Merkle({ fanout, tiers }),
  )

const _diverges = (self: _Merkle, that: _Merkle): Commit.Divergence =>
  self.fanout !== that.fanout || self.tiers.length !== that.tiers.length
    ? _Divergence.Full({
        left: Array.map(Array.headNonEmpty(self.tiers), (_digest, at) => at),
        right: Array.map(Array.headNonEmpty(that.tiers), (_digest, at) => at),
      })
    : _Divergence.Exact({ buckets: Array.reduce(
        Array.reverse(Array.range(0, Number.max(self.tiers.length, that.tiers.length) - 1)),
        [0] as ReadonlyArray<number>,
        (candidates, tier) =>
          pipe([self.tiers[tier] ?? [], that.tiers[tier] ?? []] as const, ([left, right]) =>
            Array.flatMap(candidates, (parent) =>
              Array.filterMap(
                tier === self.tiers.length - 1
                  ? Array.of(parent)
                  : Array.makeBy(self.fanout, (step) => parent * self.fanout + step),
                (at) => (left[at] === right[at] ? Option.none() : Option.some(at)),
              ))),
      ) })
```

## [03]-[COMMIT_OWNER]

[COMMIT_OWNER]:

```typescript
class Commit extends Schema.Class<Commit>("Commit")({
  key: Digest.Key.content,
  parents: Schema.Array(Digest.Key.content),
  vector: Causal.Vector,
  stamp: Clock.Hlc,
  author: Causal.Vector.Replica,
}) {
  static readonly byStamp: Order.Order<Commit> = Order.combine(
    Order.mapInput(Clock.Hlc.Order, (commit: Commit) => commit.stamp),
    Order.mapInput(Order.string, (commit: Commit) => commit.key),
  )
  static readonly Branch = class Branch extends Schema.Class<Branch>("Commit.Branch")({
    name: Schema.NonEmptyString.pipe(Schema.brand("BranchName")),
    head: Digest.Key.content,
    vector: Causal.Vector,
  }) {}
  static readonly Fanout: typeof _Fanout = _Fanout
  static readonly Merkle: typeof _Merkle = _Merkle
  static readonly Divergence: typeof _Divergence = _Divergence
  static readonly summarize: (
    leaves: Array.NonEmptyReadonlyArray<Digest.Key<"content">>,
    fanout: Commit.Fanout,
  ) => Effect.Effect<Commit.Merkle> = _summarize
  static readonly admit = (summary: Commit.Merkle): Effect.Effect<Commit.Merkle, Commit.SummaryFault> =>
    Effect.flatMap(
      _summarize(Array.headNonEmpty(summary.tiers), summary.fanout),
      (rebuilt) => Equal.equals(rebuilt, summary)
        ? Effect.succeed(summary)
        : Effect.fail(
            new _SummaryFault({
              case: Option.match(
                Array.findFirst(
                  Array.range(0, Number.min(rebuilt.tiers.length, summary.tiers.length) - 1),
                  (tier) => !Equal.equals(rebuilt.tiers[tier], summary.tiers[tier]),
                ),
                {
                  onNone: () => ({
                    reason: "root" as const,
                    expected: Array.headNonEmpty(Array.lastNonEmpty(rebuilt.tiers)),
                    actual: Array.headNonEmpty(Array.lastNonEmpty(summary.tiers)),
                  }),
                  onSome: (at) => ({
                    reason: "tier" as const,
                    at,
                    expected: Array.headNonEmpty(Array.lastNonEmpty(rebuilt.tiers)),
                    actual: Array.headNonEmpty(Array.lastNonEmpty(summary.tiers)),
                  }),
                },
              ),
            }),
          ),
    )
  static readonly diverges: (self: Commit.Merkle, that: Commit.Merkle) => Commit.Divergence = _diverges
}

declare namespace Commit {
  type Fanout = Schema.Schema.Type<typeof _Fanout>
  type Branch = InstanceType<typeof Commit.Branch>
  type Merkle = _Merkle
  type Divergence = _DivergenceValue
  type SummaryFault = _SummaryFault
  type SummaryCase = typeof _summaryFamily.payload.Type
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Commit }
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
