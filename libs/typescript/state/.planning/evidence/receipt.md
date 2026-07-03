# [STATE_RECEIPT]

`evidence/receipt.ts` owns the decoded receipt vocabulary: `Receipt` — the closed tagged family of command outcomes the C# AppHost mints — and `ReceiptEnvelope` — the rich owner carrying the receipt with its `Hlc` stamp, `TenantContext`, content-keyed command coordinate, and optional causal basis. The C# typed-receipt family never collapses into one erased TS shape (frozen invariant 5): every kind is its own union member with kind-specific evidence fields, `wire/codec/envelope` decodes `ReceiptEnvelopeWire`/`HlcStampWire`/`TenantContextWire` INTO this owner (seam `AH:63`), and the interior composes `Hlc` and `TenantContext` from `kernel` — never re-mints. The envelope's folds are plan rows on the `fold` algebra: latest-by-stamp, lifecycle rank, and the per-command receipt table every live view and timeline consumes.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                        | [SURFACE]                                        |
| :-----: | :----------------- | :-------------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | [RECEIPT_FAMILY]   | the closed outcome union and its rank vocabulary                | `Receipt`, `Receipt.Kind`, interior `_RANKS`         |
|  [02]   | [ENVELOPE_OWNER]   | the decoded envelope, its orders, and the lifecycle fold plan   | `ReceiptEnvelope`, `.byStamp/.latest/.plan/.settled`  |

## [2]-[RECEIPT_FAMILY]

- Owner: `Receipt` — one `Schema.Union` over four tagged case owners: `Accepted` (admitted, awaiting application), `Applied` (carries the resulting causal `Vector` basis and the touched `ContentKey` set), `Refused` (carries the kernel fault classification and retryability as data), `Superseded` (carries the superseding command's key) — the `_tag` is simultaneously the wire discriminant and the dispatch key.
- Packages: `effect` (`Schema`, `Order`, `Option`); `@rasm/ts/kernel` (`ContentKey`, `FaultClass`, `Hlc`, `TenantContext`); `../causal/vector.ts`; `../crdt/merge.ts`; `../fold/algebra.ts`.
- Law: evidence is fields, never message strings — `Refused` carries `fault: FaultClass` and `retryable` so gateway retry policy reads data; a receipt kind whose evidence lives in prose is the erased-family defect invariant 5 bans.
- Law: `_RANKS` is the lifecycle lattice — `Accepted` below the three terminal kinds — anchored as an interior vocabulary row table with the guard pair, so lifecycle comparison derives from one anchor and a new kind is one union member plus one rank row, with every exhaustive dispatch breaking loudly until its arm exists.
- Boundary: the member roster mirrors the C# `Rasm.AppHost/Runtime/Ports.cs` family one-to-one at the vocabulary level; roster parity is pinned at the `wire/codec/envelope` decode seam, and a C#-side kind lands here as a union member the same release.
- Growth: a new receipt kind is one tagged case, one `_RANKS` row, and zero envelope edits.

```typescript
import { Option, Order, Schema } from "effect"
import { ContentKey, FaultClass, Hlc, TenantContext } from "@rasm/ts/kernel"
import { Vector } from "../causal/vector.ts"
import { Merge } from "../crdt/merge.ts"
import { Fold } from "../fold/algebra.ts"

const _Accepted = Schema.TaggedStruct("Accepted", {
  at: Hlc,
})

const _Applied = Schema.TaggedStruct("Applied", {
  at: Hlc,
  basis: Vector,
  touched: Schema.Array(ContentKey),
})

const _Refused = Schema.TaggedStruct("Refused", {
  at: Hlc,
  fault: FaultClass,
  retryable: Schema.Boolean,
  detail: Schema.NonEmptyString,
})

const _Superseded = Schema.TaggedStruct("Superseded", {
  at: Hlc,
  by: ContentKey,
})

const _RANKS = {
  Accepted: { rank: 0, terminal: false },
  Applied: { rank: 1, terminal: true },
  Refused: { rank: 1, terminal: true },
  Superseded: { rank: 2, terminal: true },
} as const

const Receipt: Schema.Union<[typeof _Accepted, typeof _Applied, typeof _Refused, typeof _Superseded]> = Schema.Union(
  _Accepted,
  _Applied,
  _Refused,
  _Superseded,
)
type Receipt = typeof Receipt.Type

declare namespace Receipt {
  type Kind = keyof typeof _RANKS
  type Rank = (typeof _RANKS)[Kind]
  type _Rows<T extends Record<Receipt["_tag"], { readonly rank: number; readonly terminal: boolean }> = typeof _RANKS> = T
  type _Keys<K extends Receipt["_tag"] = Kind> = K
}
```

## [3]-[ENVELOPE_OWNER]

- Owner: `ReceiptEnvelope` — the one decoded evidence owner: `command` (the content-keyed command coordinate — commands are content-addressed, seam `AH:52`), `subject` (the optional target entity key), `stamp`/`tenant` (kernel vocabulary, composed), `basis` (the optional `Vector` the receipt observed), `receipt` (the family) — with orders, the merge instance, and the fold plan riding it as statics so one import carries shape, policy, and fold.
- Law: `ReceiptEnvelope.latest` is `Merge.max` over the lifecycle-then-stamp order — the LWW demonstration at its correct altitude: rank decides (a terminal receipt outranks `Accepted` regardless of clock skew), stamp tie-breaks within a rank, and the composed `Order` is the single policy edit-site.
- Law: `ReceiptEnvelope.plan` keys by `command` — the per-command receipt table is a `fold` algebra instance, so it runs identically as a pure snapshot, a stream trace, a memory handle, or a durable projection; `settled` reads terminality from the `_RANKS` row, never a `_tag` ladder.
- Law: dedup is structural — two decodes of one wire receipt compare equal under the derived equivalence, so idempotent delivery through `Replay.memory`'s `consolidate` costs nothing here.
- Boundary: decode placement and the wire twin are `wire/codec/envelope`'s; `evidence/timeline` wraps envelopes into feed entries; `ui` consumes the folded table through `query/live` views.

```typescript
class ReceiptEnvelope extends Schema.Class<ReceiptEnvelope>("ReceiptEnvelope")({
  command: ContentKey,
  subject: Schema.optionalWith(ContentKey, { as: "Option" }),
  stamp: Hlc,
  tenant: TenantContext,
  basis: Schema.optionalWith(Vector, { as: "Option" }),
  receipt: Receipt,
}) {
  static readonly byStamp: Order.Order<ReceiptEnvelope> = Order.mapInput(
    Hlc.Order,
    (envelope: ReceiptEnvelope) => envelope.stamp,
  )
  static readonly byLifecycle: Order.Order<ReceiptEnvelope> = Order.combine(
    Order.mapInput(Order.number, (envelope: ReceiptEnvelope) => _RANKS[envelope.receipt._tag].rank),
    ReceiptEnvelope.byStamp,
  )
  static readonly latest: Merge.Instance<ReceiptEnvelope> = Merge.max(ReceiptEnvelope.byLifecycle)
  static readonly plan: Fold.Plan<ReceiptEnvelope, ContentKey, ReceiptEnvelope> = Fold.plan({
    name: "evidence/receipt",
    key: (envelope) => envelope.command,
    lift: (envelope) => envelope,
    merge: ReceiptEnvelope.latest,
  })
  get settled(): boolean {
    return _RANKS[this.receipt._tag].terminal
  }
  get outcome(): Option.Option<Receipt> {
    return this.settled ? Option.some(this.receipt) : Option.none()
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Receipt, ReceiptEnvelope }
```
