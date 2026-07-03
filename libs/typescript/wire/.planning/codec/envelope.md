# [WIRE_ENVELOPE]

`codec/envelope.ts` decodes the typed receipt family — `ReceiptEnvelopeWire`, `HlcStampWire`, `TenantContextWire` from `Rasm.AppHost`, `RenderReceiptWire` from `Rasm.AppUi/Render` — and holds invariant 5: the C# typed-receipt family never collapses into one erased TS receipt. Receipt evidence decodes INTO `state/evidence/receipt`'s vocabulary, the stamp and tenant land the kernel `Hlc` and `TenantContext` values (adopted-verbatim names), and the render receipt — the frame-hash proof `ui/viewer` probes read — is the one wire-owned shape here. Four distinct decode surfaces ride one assembled owner; each keeps its kind, its fields, and its consumer.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                        |
| :-----: | :--------------- | :------------------------------------------------------------------------------ |
|   [1]   | `RECEIPT_FAMILY` | envelope, stamp, and tenant decodes into `state`/`kernel` vocabulary             |
|   [2]   | `RENDER_RECEIPT` | the wire-owned `RenderReceipt` frame-hash proof + the assembled owner            |

## [2]-[RECEIPT_FAMILY]

- Owner: the three composed schemas — `_receipt` landing `state`'s `Receipt` evidence family, `_stamp` landing the kernel `Hlc`, `_tenant` landing the kernel `TenantContext` — each one `ProtoCodec.family` composition, zero local twins.
- Entry: through the assembled `Envelope` owner in `[3]`; `state/evidence/receipt` and `state/evidence/availability` consume decoded values via `#vocab` wiring at the app root.
- Growth: a new receipt kind is a C# case, a `state` vocabulary row, and zero edits here — the envelope decode is total over the family because `state`'s union owns the cases and this module composes it whole.
- Law: never erased (invariant 5) — the envelope's body decodes as `state`'s tagged receipt union with every kind distinct; a flattened `{ kind: string, payload: unknown }` landing shape is the named collapse defect.
- Law: the stamp is kernel material — `HlcStampWire`'s two-half cell decodes through the kernel `Hlc` admission (physical half first, logical second, both little-endian — the kernel's port law, invariant 3); this module never reads a byte pair by hand.
- Law: `TenantContext` crosses verbatim — one name, one owning side (invariant 8); the decoded value is the same vocabulary `telemetry` resources and `store` scopes derive from, so tenancy is one value everywhere.
- Boundary: the proto engine and read posture are `codec/proto.ts`'s; availability evidence decoded from the same AppHost plane types against `state/evidence/availability` and rides the same envelope union row.

```typescript
import { Hlc, TenantContext } from "@rasm/ts/kernel"
import { Receipt } from "@rasm/ts/state"
import { Schema } from "effect"
import { ProtoCodec } from "./proto.ts"

const _receipt: Schema.Schema<Receipt, Uint8Array> = ProtoCodec.family(ProtoCodec.suite.ReceiptEnvelopeWire, Receipt)
const _stamp: Schema.Schema<Hlc, Uint8Array> = ProtoCodec.family(ProtoCodec.suite.HlcStampWire, Hlc.FromStamp)
const _tenant: Schema.Schema<TenantContext, Uint8Array> = ProtoCodec.family(ProtoCodec.suite.TenantContextWire, TenantContext)
```

## [3]-[RENDER_RECEIPT]

- Owner: `RenderReceipt` — the wire-owned frame-hash proof: which viewport frame, which content key, whether the captured hash matched, and when; plus `Envelope`, the assembled owner publishing all four decode surfaces.
- Entry: `Envelope.receipt`/`Envelope.stamp`/`Envelope.tenant`/`Envelope.render` — the schema per family; `Envelope.decode(family, octets)` the modality-polymorphic one-shot whose return follows the family key through the mapped contract.
- Receipt: `RenderReceipt` IS the proof value `ui/viewer` `probe/receipt` displays — a failed match is evidence rendered to the operator, never a fault on this rail.
- Growth: a new render-proof axis (a pass label, a capture duration) is one field mirroring the C# projection; a new receipt-plane family is one census row plus one owner member.
- Law: the render receipt is decode-only projection — `matched` is C#-computed; TS never re-hashes a frame to second-guess it, because the hash ran where the frame lived.
- Boundary: the probe UI consumes through `#vocab`; benchmark claims with host fingerprints are `codec/claim.ts`'s identity gate, not this proof.

```typescript
import { ContentKey } from "@rasm/ts/kernel"
import { Effect, type ParseResult } from "effect"

class RenderReceipt extends Schema.Class<RenderReceipt>("RenderReceipt")({
  view: Schema.NonEmptyString,
  key: ContentKey.FromCell,
  matched: Schema.Boolean,
  at: Schema.DateTimeUtc,
}) {}

const _rows = {
  ReceiptEnvelopeWire: _receipt,
  HlcStampWire: _stamp,
  TenantContextWire: _tenant,
  RenderReceiptWire: ProtoCodec.family(ProtoCodec.suite.RenderReceiptWire, RenderReceipt),
} as const

declare namespace Envelope {
  type Family = keyof typeof _rows
  type Decoded<K extends Family> = Schema.Schema.Type<(typeof _rows)[K]>
  type Shape = {
    readonly Render: typeof RenderReceipt
    readonly receipt: (typeof _rows)["ReceiptEnvelopeWire"]
    readonly stamp: (typeof _rows)["HlcStampWire"]
    readonly tenant: (typeof _rows)["TenantContextWire"]
    readonly render: (typeof _rows)["RenderReceiptWire"]
    readonly decode: <K extends Family>(family: K, octets: Uint8Array) => Effect.Effect<Decoded<K>, ParseResult.ParseError>
  }
  type _Keys<K extends Family = keyof typeof _rows> = K
}

const Envelope: Envelope.Shape = {
  Render: RenderReceipt,
  receipt: _rows.ReceiptEnvelopeWire,
  stamp: _rows.HlcStampWire,
  tenant: _rows.TenantContextWire,
  render: _rows.RenderReceiptWire,
  decode: (family, octets) => Schema.decodeUnknown(_rows[family])(octets),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Envelope }
```
