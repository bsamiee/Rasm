# [WIRE_LIVEWIRE]

`codec/livewire.ts` decodes the live-binding plane from `Rasm.AppHost/Wire` — `BindingStatusWire`, `CoercedValueWire`, `WriteReceiptWire` — the three shapes a live parameter binding round-trip produces: the binding's standing status, the value a write coerced to, and the receipt proving what landed. The three stay distinct owners under one union (invariant 5 altitude: evidence kinds never erase), and `ui/viewer` `panel/binding` consumes the decoded family through `#vocab` to render binding state, coercion feedback, and write confirmation.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                |
| :-----: | :--------------- | :------------------------------------------------------------------------- |
|   [1]   | `BINDING_FAMILY` | the three tagged owners, the feed union, the per-family decode surface      |

## [2]-[BINDING_FAMILY]

- Owner: `Livewire` — the union of three `Schema.TaggedClass` cases bound to one same-name pair: `BindingStatus` (the standing state row over the closed `phase` vocabulary), `CoercedValue` (original and coerced values with the coercion path), `WriteReceipt` (the landed write with its `Hlc` stamp); the assembled owner carries per-case schemas and the union feed decode.
- Entry: `Livewire.status`/`Livewire.coerced`/`Livewire.receipt` per-family schemas; `Livewire.feed` the union schema for the mixed binding channel — one decode, the `_tag` dispatches, and `$`-style panel dispatch stays a `Match.tagsExhaustive` record at the consumer.
- Receipt: `WriteReceipt` is the write's proof — binding coordinate, landed value, `Hlc` stamp; the panel resolves optimistic edits against it and a missing receipt is a pending write, never an assumed success.
- Growth: a new binding evidence kind is one tagged case plus the union row — the panel's exhaustive dispatch breaks until its arm exists; a new phase is one `_phases` literal the status row widens.
- Law: the phase vocabulary is closed — `bound`, `coercing`, `refused`, `detached` — and rides `Schema.Literal` from one anchor; a string-typed status field is the rejected shape.
- Law: coercion is C#-computed evidence — `CoercedValue` carries what the host DID, and TS renders it; a TS-side re-coercion or clamping pass second-guesses the authority and is rejected.
- Boundary: panel presentation and edit staging are `ui/viewer` `panel/binding`; the live channel transport arrives via `host/net` at the app root; the `Hlc` admission is the kernel's.

```typescript
import { Hlc } from "@rasm/ts/kernel"
import { Effect, type ParseResult, Schema } from "effect"
import { ProtoCodec } from "./proto.ts"

const _phases = ["bound", "coercing", "refused", "detached"] as const

class BindingStatus extends Schema.TaggedClass<BindingStatus>()("BindingStatus", {
  binding: Schema.NonEmptyString,
  phase: Schema.Literal(..._phases),
  detail: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
}) {}

class CoercedValue extends Schema.TaggedClass<CoercedValue>()("CoercedValue", {
  binding: Schema.NonEmptyString,
  offered: Schema.Unknown,
  landed: Schema.Unknown,
  path: Schema.NonEmptyString,
}) {}

class WriteReceipt extends Schema.TaggedClass<WriteReceipt>()("WriteReceipt", {
  binding: Schema.NonEmptyString,
  landed: Schema.Unknown,
  stamp: Hlc.FromStamp,
}) {}

const _feed = Schema.Union(BindingStatus, CoercedValue, WriteReceipt)

declare namespace Livewire {
  type Event = Schema.Schema.Type<typeof _feed>
  type Phase = (typeof _phases)[number]
}

const Livewire: {
  readonly Status: typeof BindingStatus
  readonly Coerced: typeof CoercedValue
  readonly Receipt: typeof WriteReceipt
  readonly status: Schema.Schema<BindingStatus, Uint8Array>
  readonly coerced: Schema.Schema<CoercedValue, Uint8Array>
  readonly receipt: Schema.Schema<WriteReceipt, Uint8Array>
  readonly feed: typeof _feed
  readonly decode: (octets: Uint8Array, family: "BindingStatusWire" | "CoercedValueWire" | "WriteReceiptWire") => Effect.Effect<Livewire.Event, ParseResult.ParseError>
} = {
  Status: BindingStatus,
  Coerced: CoercedValue,
  Receipt: WriteReceipt,
  status: ProtoCodec.family(ProtoCodec.suite.BindingStatusWire, BindingStatus),
  coerced: ProtoCodec.family(ProtoCodec.suite.CoercedValueWire, CoercedValue),
  receipt: ProtoCodec.family(ProtoCodec.suite.WriteReceiptWire, WriteReceipt),
  feed: _feed,
  decode: (octets, family) =>
    family === "BindingStatusWire"
      ? Schema.decodeUnknown(Livewire.status)(octets)
      : family === "CoercedValueWire"
        ? Schema.decodeUnknown(Livewire.coerced)(octets)
        : Schema.decodeUnknown(Livewire.receipt)(octets),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Livewire }
```
