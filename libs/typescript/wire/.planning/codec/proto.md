# [WIRE_PROTO]

`codec/proto.ts` is the proto-suite owner: the one configured protobuf-es engine every proto-framed wire family decodes through, the generated-schema suite table that binds each census family to its `GenMessage`, the shared type `Registry` that resolves `Any` payloads and descriptor options, and the two suite-level decodes that land in sibling vocabularies — the `QuantityFamily` SI-scalar decode into the kernel `Quantity` (invariant 4) and the `FaultDetailWire` row that is `fault/detail.ts`'s vocabulary hook. Sibling proto pages compose `ProtoCodec.family` and `ProtoCodec.stream`; none re-configures read options, re-wraps `fromBinary`, or mints a second registry. The generated `proto_pb.ts` is `@bufbuild/protoc-gen-es` output pinned lockstep with the runtime at 2.12.1 — build artifact, imported, never hand-edited.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER] | [OWNS]                                                                                  |
| :-----: | :-------- | :--------------------------------------------------------------------------------------- |
|   [1]   | `ENGINE`  | the configured proto engine: read/write policy, byte↔message transform, framed streaming  |
|   [2]   | `SUITE`   | the census-guarded `GenMessage` table + the shared `Registry` + the assembled owner       |
|   [3]   | `QUANTITY`| the `QuantityWire` decode into the kernel `Quantity` — SI canonicalization crosses, never re-runs |

## [2]-[ENGINE]

- Owner: the interior engine trio — `_READ`/`_WRITE` policy rows, the `_Message` foreign-identity schema, and `_frame`, the one `Schema.transformOrFail` fold from held octets to a live proto message; `_stream` lifts the size-delimited frame walk onto the `Stream` rail.
- Entry: `ProtoCodec.frame(gen)` for the raw message schema; `ProtoCodec.family(gen, owned)` for the composed byte→owned-vocabulary schema every codec page exports; `ProtoCodec.stream(gen, family)` for length-prefixed frame feeds.
- Growth: a read-policy axis (a wider recursion bound for a named adversarial surface) is a `_BOUNDS` field; the transform shape never changes.
- Law: engine throws join the one admission rail — `fromBinary`/`toBinary` defects fold to `ParseResult.Type` inside the transform, so a malformed proto is a `ParseError` like every other admission failure; a codec fault family beside the decode rail is the rejected second vocabulary.
- Law: `readUnknownFields: true` and `writeUnknownFields: true` are the drift-safe posture — an unknown field is preserved evidence for the descriptor gate, never a decode fault, and a partial peer's round-trip re-emits what it did not understand.
- Law: `_Message` admits the foreign message by identity through `Schema.declare` over `isMessage` — the decoded value is plain `$typeName`-branded data, discriminated by `isMessage(value, schema)`, never `instanceof`; the message never leaves this module untyped because `family` composes the owned vocabulary immediately.
- Law: `recursionLimit` bounds adversarial nesting before any allocation grows — the overrun surfaces as `ParseError` at the seam and the quarantine divert classifies it at intake.
- Law: 64-bit fields are `bigint` end to end — `protoInt64` bridges construction sites; a `Number`-coerced i64 loses precision past 2^53 and is the named defect.
- Boundary: which owned schema a family composes is each codec page's decision; the quarantine divert every framed stream composes is `fault/quarantine.ts`; `toBinary` canonical bytes as content-key input is `frame/artifact.ts`'s verify law.

```typescript
import { fromBinary, isMessage, type DescMessage, type Message, sizeDelimitedDecodeStream, toBinary } from "@bufbuild/protobuf"
import { Either, Option, ParseResult, Schema, Stream } from "effect"
import type { Inventory } from "../contract/drift.ts"
import { WireFault } from "../fault/quarantine.ts"

const _BOUNDS = { recursionLimit: 24 } as const
const _READ = { readUnknownFields: true, recursionLimit: _BOUNDS.recursionLimit } as const
const _WRITE = { writeUnknownFields: true } as const

const _Message: Schema.Schema<Message> = Schema.declare((input: unknown): input is Message => isMessage(input))

const _frame = (gen: DescMessage): Schema.Schema<Message, Uint8Array> =>
  Schema.transformOrFail(Schema.Uint8ArrayFromSelf, _Message, {
    strict: true,
    decode: (octets, _options, ast) =>
      Either.try({
        try: () => fromBinary(gen, octets, _READ),
        catch: (defect) => new ParseResult.Type(ast, octets, String(defect)),
      }),
    encode: (message, _options, ast) =>
      Either.try({
        try: () => toBinary(gen, message, _WRITE),
        catch: (defect) => new ParseResult.Type(ast, message, String(defect)),
      }),
  })

const _family = <A, I>(gen: DescMessage, owned: Schema.Schema<A, I>): Schema.Schema<A, Uint8Array> =>
  _frame(gen).pipe(Schema.compose(owned, { strict: false }))

const _stream = (gen: DescMessage, family: Inventory.Family) => (frames: AsyncIterable<Uint8Array>): Stream.Stream<Message, WireFault> =>
  Stream.fromAsyncIterable(
    sizeDelimitedDecodeStream(gen, frames, _READ),
    (defect) => new WireFault({ family, reason: "malformed", detail: String(defect), evidence: Option.none() }),
  )
```

## [3]-[SUITE]

- Owner: `_suite` — the interior `GenMessage` table keyed by the census's proto families (`FileDescriptorSetWire` alone rides the shipped `./wkt` schema at `contract/descriptor.ts`); the merged-hub guard demands one generated schema per census row, so a census family with no emit — or an emit the census never named — fails at this declaration. `ProtoCodec` assembles engine, suite, and registry under one export.
- Entry: `ProtoCodec.suite[family]` — the `GenMessage` a sibling page hands back to `family`/`frame`/`stream`; `ProtoCodec.registry` — the one type registry `anyUnpack`, extension reads, and `findDetails` resolve against.
- Receipt: `ProtoCodec.suite.FaultDetailWire` is the vocabulary hook — `fault/detail.ts` composes it for `FromWire` and `findDetails`; descriptor-option vocabulary annotations on the emitted descriptors read through `getOption` against this registry.
- Growth: a new proto wire family is one census row, one regenerated `proto_pb.ts`, and one `_suite` row — the guard breaks until all three agree; the registry and every engine surface pick it up with zero further edits.
- Law: the suite is the only site that touches the generated module — sibling pages import `ProtoCodec`, never `proto_pb.ts`; the generated emit is one file whose regeneration is atomic with the census edit.
- Law: the registry is program-wide singular — `Any` unpacking, extension resolution, and error-detail decode all share it; a second `createRegistry` call forks type resolution and is the drift defect.

```typescript
import { createRegistry, type Registry } from "@bufbuild/protobuf"
import { Array, type Types } from "effect"
import * as pb from "./proto_pb.ts"

const _names = [
  "ReceiptEnvelopeWire", "HlcStampWire", "TenantContextWire", "RenderReceiptWire", "FaultDetailWire", "QuantityWire",
  "ElementGraphWire", "NodeWire", "RelationshipWire", "ProgressMarkWire", "CredentialPemWire",
  "BenchmarkClaimWire", "HostFingerprintWire", "BindingStatusWire", "CoercedValueWire", "WriteReceiptWire",
  "FlagVerdictWire", "ControlIntentWire", "LayoutConstraintWire", "BcfTopicWire", "BcfViewpointWire",
  "GeoFeatureWire", "BimWire", "DiffWire", "IdsAuditWire", "MaterialWire", "OpenPbrGroupsWire",
  "AppearanceSummaryWire", "ArtifactFrameWire", "GeometryPayloadWire", "GeometryResidencyWire",
  "CommandPayloadWire", "SupportCaptureWire", "CapabilityDescriptorWire",
] as const

const _suite = {
  ReceiptEnvelopeWire: pb.ReceiptEnvelopeWireSchema,
  HlcStampWire: pb.HlcStampWireSchema,
  TenantContextWire: pb.TenantContextWireSchema,
  RenderReceiptWire: pb.RenderReceiptWireSchema,
  FaultDetailWire: pb.FaultDetailWireSchema,
  QuantityWire: pb.QuantityWireSchema,
  ElementGraphWire: pb.ElementGraphWireSchema,
  NodeWire: pb.NodeWireSchema,
  RelationshipWire: pb.RelationshipWireSchema,
  ProgressMarkWire: pb.ProgressMarkWireSchema,
  CredentialPemWire: pb.CredentialPemWireSchema,
  BenchmarkClaimWire: pb.BenchmarkClaimWireSchema,
  HostFingerprintWire: pb.HostFingerprintWireSchema,
  BindingStatusWire: pb.BindingStatusWireSchema,
  CoercedValueWire: pb.CoercedValueWireSchema,
  WriteReceiptWire: pb.WriteReceiptWireSchema,
  FlagVerdictWire: pb.FlagVerdictWireSchema,
  ControlIntentWire: pb.ControlIntentWireSchema,
  LayoutConstraintWire: pb.LayoutConstraintWireSchema,
  BcfTopicWire: pb.BcfTopicWireSchema,
  BcfViewpointWire: pb.BcfViewpointWireSchema,
  GeoFeatureWire: pb.GeoFeatureWireSchema,
  BimWire: pb.BimWireSchema,
  DiffWire: pb.DiffWireSchema,
  IdsAuditWire: pb.IdsAuditWireSchema,
  MaterialWire: pb.MaterialWireSchema,
  OpenPbrGroupsWire: pb.OpenPbrGroupsWireSchema,
  AppearanceSummaryWire: pb.AppearanceSummaryWireSchema,
  ArtifactFrameWire: pb.ArtifactFrameWireSchema,
  GeometryPayloadWire: pb.GeometryPayloadWireSchema,
  GeometryResidencyWire: pb.GeometryResidencyWireSchema,
  CommandPayloadWire: pb.CommandPayloadWireSchema,
  SupportCaptureWire: pb.SupportCaptureWireSchema,
  CapabilityDescriptorWire: pb.CapabilityDescriptorWireSchema,
} as const

const _registry: Registry = createRegistry(...Array.map(_names, (name) => _suite[name]))

declare namespace ProtoCodec {
  type Suite = Exclude<keyof Inventory.Of<"proto">, "FileDescriptorSetWire">
  type Shape = Types.Simplify<{
    readonly names: typeof _names
    readonly suite: typeof _suite
    readonly registry: Registry
    readonly frame: typeof _frame
    readonly family: typeof _family
    readonly stream: typeof _stream
    readonly quantity: Schema.Schema<Quantity, Uint8Array>
  }>
  type _Rows<T extends Record<Suite, DescMessage> = typeof _suite> = T
  type _Keys<K extends Suite = keyof typeof _suite> = K
}
```

## [4]-[QUANTITY]

- Owner: `ProtoCodec.quantity` — the one `QuantityWire` decode surface, composed from the engine and the kernel `Quantity` schema; the assembled `ProtoCodec` owner closes the module.
- Entry: `Schema.decodeUnknown(ProtoCodec.quantity)` wherever a proto payload carries an SI scalar — `codec/proto` families with quantity fields compose this schema at the field, so every SI crossing is one spelling.
- Law: SI canonicalization happened once at C# admission (invariant 4) — the wire carries magnitude plus dimension exponents and the decode transports them into the kernel `Quantity` verbatim; a `{ value, unit }` re-decode, a unit-string parse, or a TS-side conversion is the rejected re-canonicalization.
- Law: the kernel `Quantity`'s encoded side is the `QuantityWire` message shape field-for-field — the composition is `family(suite.QuantityWire, Quantity)` with zero mapping layer; divergence is a kernel-page ripple, never a local rename map.
- Boundary: `Quantity` arithmetic, dimension algebra, and display live in `kernel/schema/quantity`; this module only lands the value.

```typescript
import { Quantity } from "@rasm/ts/kernel"

const ProtoCodec: ProtoCodec.Shape = {
  names: _names,
  suite: _suite,
  registry: _registry,
  frame: _frame,
  family: _family,
  stream: _stream,
  quantity: _family(_suite.QuantityWire, Quantity),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ProtoCodec }
```
