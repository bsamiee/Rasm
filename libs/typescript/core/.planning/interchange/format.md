# [CORE_FORMAT]

The four format engines of the interchange plane — one arm per C#-minted byte dialect, each a configured-once pure engine behind one `Schema.transformOrFail` fold so every malformed payload is a `ParseError` on the one admission rail and no second codec fault vocabulary exists. `Proto` is the protobuf-es engine: read/write posture, the census-guarded `GenMessage` suite table, the singular type registry, the size-delimited frame stream both directions, and the custom-option read. `Cbor` is the canonical RFC 8949 decoder with the interop posture no cross-language byte may bypass, the `setSizeLimits` DoS gate, and the chunked `decodeIter` lane for multi-segment payloads. `Pack` is the MessagePack engine carrying the sixteen-byte `Hlc` extension row, the interner context thread, i64 fidelity, and the `ExtData` foreign-ext seam. `Patch` is the RFC 6902 engine: the six-op `Operation` union over the branch pointer brand, the `Pointer` traversal codec, the clone-fenced value-rail apply, the content-key-reconciled minimal diff, and the OCC test-guard egress. Engines are format mechanics only — which family decodes through which arm, what a decoded value lands as, and every verification are the codec registry's concern. The module is `core/src/interchange/format.ts`; a new proto family is one suite row, a new engine posture axis is one policy field, and a fifth dialect is a new engine owner beside these four, never a widening of one.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                     | [PUBLIC] |
| :-----: | :---------------- | :--------------------------------------------------------------------------- | :------- |
|  [01]   | `ENGINE_FOLD`     | the shared defect-to-`ParseError` fold every arm's transform composes         | interior |
|  [02]   | `PROTO_ENGINE`    | read/write posture, frame/family/stream, the suite table, the one registry   | `Proto`  |
|  [03]   | `CBOR_ENGINE`     | the configured canonical decoder, the DoS gate, the quirk augmentation        | `Cbor`   |
|  [04]   | `MSGPACK_ENGINE`  | the `Hlc`-ext codec pair, the interner context, stream and zero-copy egress  | `Pack`   |
|  [05]   | `JSONPATCH_ENGINE`| the op union, apply, diff, OCC guards, and the patch content key             | `Patch`  |

## [2]-[ENGINE_FOLD]

[ENGINE_FOLD]:
- Owner: `_lifted`, the one transform builder every byte arm instantiates — a `(decode, encode)` function pair folds into a `Schema.transformOrFail` from held octets to `Schema.Unknown`, each direction catching the engine's throw through `Either.try` into `ParseResult.Type`, so a codec defect joins the admission rail at the seam it occurred and the interior never meets a raw engine throw.
- Law: engines configure once at module scope — instance options, ceilings, extension registries are module-init facts; a per-call engine construction re-mints codec state and is the rejected form.
- Law: a decode-only arm states its posture as a typed refusal — the encode direction fails `ParseResult.Forbidden` so an accidental egress through a decode-only schema dies at the boundary as evidence, never as silent bytes.
- Growth: a fifth dialect instantiates `_lifted` with its own engine pair; the fold shape never changes.
- Packages: `effect` (`Schema`, `ParseResult`, `Either`).

```typescript
import { Array, Effect, Either, Layer, Option, ParseResult, pipe, Predicate, Schema, Stream, type Types } from "effect"

const _lifted = (
  decode: (octets: Uint8Array) => unknown,
  encode: (value: unknown) => Uint8Array | undefined,
): Schema.Schema<unknown, Uint8Array> =>
  Schema.transformOrFail(Schema.Uint8ArrayFromSelf, Schema.Unknown, {
    strict: true,
    decode: (octets, _options, ast) =>
      Either.try({ try: () => decode(octets), catch: (defect) => new ParseResult.Type(ast, octets, String(defect)) }),
    encode: (value, _options, ast) =>
      Either.try({ try: () => encode(value), catch: (defect) => new ParseResult.Type(ast, value, String(defect)) }).pipe(
        Either.flatMap(Either.liftPredicate(
          (octets): octets is Uint8Array => octets !== undefined,
          () => new ParseResult.Forbidden(ast, value, "<decode-only>"),
        )),
      ),
  })
```

## [3]-[PROTO_ENGINE]

[PROTO_ENGINE]:
- Owner: `Proto`, the protobuf-es engine — `_READ`/`_WRITE` posture rows, the `_Message` foreign-identity schema, `frame(gen)` the raw byte-to-message schema, `family(gen, owned)` the composed byte-to-owned-vocabulary schema every proto registry row instantiates, `stream(gen)` the size-delimited frame walk lifted to `Stream`, `delimit(gen, message)` the length-prefixed egress twin of that walk, `peek(octets)` the frame-header triage read, `option(options, ext)` the custom-option read over any `Desc*.proto.options` carrier, the `_suite` `GenMessage` table over the ordered `_names` tuple, and the one `createRegistry` value `Any` unpacking and error-detail decode resolve against.
- Law: `readUnknownFields: true` and `writeUnknownFields: true` are the drift-safe posture — an unknown field is preserved evidence the contract gate grades, never a decode fault, and a partial peer's round-trip re-emits what it did not understand; `recursionLimit` bounds adversarial nesting before allocation grows, surfacing as `ParseError` at the seam.
- Law: `_Message` admits the foreign message by identity through `Schema.declare` over `isMessage` — a decoded message is `$typeName`-branded plain data, never `instanceof`-discriminated, and it leaves this module only through `family`'s composed owned vocabulary.
- Law: 64-bit fields are `bigint` end to end — `protoInt64` bridges construction sites, and a `Number`-coerced i64 loses precision past 2^53 and is the named defect.
- Law: the suite is the only site touching the generated `interchange_pb.ts` — sibling pages import `Proto`, never the emit; the generated module is `@bufbuild/protoc-gen-es` output pinned lockstep with the runtime by the workspace catalog, regenerated atomically with a census edit, and never hand-edited.
- Law: the registry is program-wide singular — a second `createRegistry` call forks `Any` and detail resolution and is the drift defect; `peek` reads a size-delimited header without consuming — `sizeDelimitedPeek` answers `{ size, offset, eof }`, an incomplete varint folds to `Option.none`, and the settled pair carries `size` with `offset` so the holder of a tail proves `offset + size` against the held extent — the triage the quarantine rail classifies `truncated` against.
- Law: `delimit` is the framed egress twin — `sizeDelimitedEncode` under the `_WRITE` posture, composed only where an emission joins a size-delimited transport; quarantine replay octets stay whole-message form, the codec registry's own decode coordinate, so ingress framing and replay framing never entangle.
- Law: `option` is the annotation read — `hasExtension` guards, `getExtension` reads, and the pair rides `Option` over the carrier's `proto.options` message, so a descriptor annotation (the SI-scalar `QuantityFamily` row on `QuantityWire`, a field-behavior mark) resolves typed through the generated extension descriptor and no consumer touches the raw options message; an absent options carrier or unset extension is `Option.none`, never a sentinel.
- Law: `toBinary` canonical bytes are the content-key input — proto is deterministic per field order, so the codec parity combinator hashes engine egress and never a second serialization.
- Growth: a new proto wire family is one census row, one regenerated emit, and one `_suite` row — the tuple/table guards break until all three agree; a read-posture axis is a `_READ` field.
- Boundary: which family binds which suite row, quarantine classification, and every landing shape are the codec registry's; descriptor reflection over the `FileDescriptorSet` is the contract gate's altitude.
- Packages: `@bufbuild/protobuf` (`fromBinary`, `toBinary`, `isMessage`, `sizeDelimitedDecodeStream`, `sizeDelimitedEncode`, `sizeDelimitedPeek`, `createRegistry`, `getExtension`, `hasExtension`, `DescExtension`, `DescMessage`, `Extendee`, `ExtensionValueShape`, `Message`, `Registry`); `effect` (`Schema`, `Stream`, `Either`, `Option`, `Array`, `pipe`); generated `./interchange_pb.ts`.

```typescript
import {
  createRegistry,
  type DescExtension,
  type DescMessage,
  type Extendee,
  type ExtensionValueShape,
  fromBinary,
  getExtension,
  hasExtension,
  isMessage,
  type Message,
  type Registry,
  sizeDelimitedDecodeStream,
  sizeDelimitedEncode,
  sizeDelimitedPeek,
  toBinary,
} from "@bufbuild/protobuf"
import * as pb from "./interchange_pb.ts"

const _READ = { readUnknownFields: true, recursionLimit: 24 } as const
const _WRITE = { writeUnknownFields: true } as const

const _Message: Schema.Schema<Message> = Schema.declare((input: unknown): input is Message => isMessage(input))

const _frame = (gen: DescMessage): Schema.Schema<Message, Uint8Array> =>
  _lifted(
    (octets) => fromBinary(gen, octets, _READ),
    (value) => (isMessage(value, gen) ? toBinary(gen, value, _WRITE) : undefined),
  ).pipe(Schema.compose(_Message, { strict: false }))

const _names = [
  "ReceiptEnvelopeWire", "HlcStampWire", "TenantContextWire", "AvailabilityWire", "RenderReceiptWire",
  "FaultDetailWire", "QuantityWire", "ElementGraphWire", "NodeWire", "RelationshipWire",
  "ProgressMarkWire", "CredentialPemWire", "BenchmarkClaimWire", "HostFingerprintWire",
  "BindingStatusWire", "CoercedValueWire", "WriteReceiptWire", "FlagVerdictWire",
  "ControlIntentWire", "LayoutConstraintWire", "BcfTopicWire", "BcfViewpointWire",
  "GeoFeatureWire", "BimWire", "DiffWire", "IdsAuditWire",
  "MaterialWire", "OpenPbrGroupsWire", "AppearanceSummaryWire",
  "ArtifactFrameWire", "GeometryPayloadWire", "GeometryResidencyWire",
  "CommandPayloadWire", "SupportCaptureWire", "CapabilityDescriptorWire",
] as const

const _suite = {
  ReceiptEnvelopeWire: pb.ReceiptEnvelopeWireSchema,
  HlcStampWire: pb.HlcStampWireSchema,
  TenantContextWire: pb.TenantContextWireSchema,
  AvailabilityWire: pb.AvailabilityWireSchema,
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

declare namespace Proto {
  type Name = keyof typeof _suite
  type Shape = Types.Simplify<{
    readonly names: typeof _names
    readonly suite: typeof _suite
    readonly registry: Registry
    readonly frame: typeof _frame
    readonly family: <A, I>(gen: DescMessage, owned: Schema.Schema<A, I>) => Schema.Schema<A, Uint8Array>
    readonly stream: (gen: DescMessage) => (frames: AsyncIterable<Uint8Array>) => Stream.Stream<Message, unknown>
    readonly delimit: (gen: DescMessage, message: Message) => Uint8Array
    readonly peek: (octets: Uint8Array) => Option.Option<{ readonly size: number; readonly offset: number }>
    readonly option: <Ext extends DescExtension>(
      options: Extendee<Ext> | undefined,
      ext: Ext,
    ) => Option.Option<ExtensionValueShape<Ext>>
  }>
  type _Rows<T extends Record<(typeof _names)[number], DescMessage> = typeof _suite> = T
  type _Keys<K extends (typeof _names)[number] = Name> = K
}

const Proto: Proto.Shape = {
  names: _names,
  suite: _suite,
  registry: createRegistry(...Array.map(_names, (name) => _suite[name])),
  frame: _frame,
  family: (gen, owned) => _frame(gen).pipe(Schema.compose(owned, { strict: false })),
  stream: (gen) => (frames) => Stream.fromAsyncIterable(sizeDelimitedDecodeStream(gen, frames, _READ), (defect) => defect),
  delimit: (gen, message) => sizeDelimitedEncode(gen, message, _WRITE),
  peek: (octets) =>
    pipe(sizeDelimitedPeek(octets), (header) =>
      header.size === null ? Option.none() : Option.some({ size: header.size, offset: header.offset })),
  option: (options, ext) =>
    Option.fromNullable(options).pipe(
      Option.flatMap((carrier) => (hasExtension(carrier, ext) ? Option.some(getExtension(carrier, ext)) : Option.none())),
    ),
}
```

## [4]-[CBOR_ENGINE]

[CBOR_ENGINE]:
- Owner: `Cbor`, the canonical-CBOR engine — one configured `Decoder` under the cross-language posture (`useRecords: false`, `mapsAsObjects: true`, `tagUint8Array: true`), `frame` the decode-only byte schema, `frames` the concatenated multi-frame walk, `chunked` the iterator lane over `decodeIter` for a segment set that outgrows one buffer, and `GateLive` the boot-edge `Layer` arming the `setSizeLimits` ceilings before any untrusted decode.
- Law: the top-level `decode`/`encode` bind cbor-x's shared default instance whose `useRecords: true` engages the proprietary tag-105 record dialect no C# writer speaks — a top-level call on cross-language bytes is the drift defect, and this engine's configured instance is the only decoder the plane touches.
- Law: C#-minted CBOR is content-stable — deterministic key order, shortest-form integers — so the decoded header's content key re-verifies against the held octets through the codec parity combinator; the engine never re-canonicalizes and never re-mints.
- Law: the quirk augmentation is the owner's capture — the shipped `index.d.ts` mislabels the size gate `setMaxLimits` and phantoms `MAX_LIMITS_OPTIONS` while the runtime exports `setSizeLimits` and `decodeIter`; the `declare module` block beside the engine declares only verified runtime truth, so downstream composes `setSizeLimits` typed and never re-discovers the mismatch.
- Law: `tagUint8Array: true` keeps byte fields as `Uint8Array` views for content-key verification; a decoded `Tag` value dispatches at the consuming registry row, never here.
- Law: `chunked` rides the same posture — `decodeIter` takes the `_POSTURE` options record, so the multi-segment `SnapshotHeader` roster streams through the canonical decoder and the module-level default instance (whose `useRecords: true` speaks the tag-105 dialect) stays untouched on this lane too.
- Growth: a ceiling axis is one `_CEILINGS` field; a posture axis is one `_POSTURE` field every lane inherits.
- Boundary: the `SnapshotHeader` landing shape, its parity verify, and quarantine classification are the codec registry's; the ceilings guard this engine's decode plane and the frame rail's assembly budgets are `value` `Ingress` rows enforced at the frame page.
- Packages: `cbor-x` (`Decoder`, `setSizeLimits`, `decodeIter` via the local augmentation); `effect` (`Schema`, `Layer`, `Effect`, `Stream`).

```typescript
import { decodeIter, Decoder, setSizeLimits } from "cbor-x"

declare module "cbor-x" {
  function setSizeLimits(limits: {
    readonly maxArraySize?: number
    readonly maxMapSize?: number
    readonly maxObjectSize?: number
  }): void
  function decodeIter(
    iterable: AsyncIterable<Uint8Array>,
    options?: { readonly useRecords?: boolean; readonly mapsAsObjects?: boolean; readonly tagUint8Array?: boolean },
  ): AsyncIterableIterator<unknown>
}

const _POSTURE = { useRecords: false, mapsAsObjects: true, tagUint8Array: true } as const
const _CEILINGS = { maxArraySize: 65536, maxMapSize: 16384, maxObjectSize: 1048576 } as const

const _cborDecoder = new Decoder(_POSTURE)

const Cbor: {
  readonly frame: Schema.Schema<unknown, Uint8Array>
  readonly frames: (octets: Uint8Array) => ReadonlyArray<unknown>
  readonly chunked: (segments: AsyncIterable<Uint8Array>) => Stream.Stream<unknown, unknown>
  readonly GateLive: Layer.Layer<never>
} = {
  frame: _lifted((octets) => _cborDecoder.decode(octets), () => undefined),
  frames: (octets) => _cborDecoder.decodeMultiple(octets) ?? [],
  chunked: (segments) => Stream.fromAsyncIterable(decodeIter(segments, _POSTURE), (defect) => defect),
  GateLive: Layer.effectDiscard(Effect.sync(() => setSizeLimits(_CEILINGS))),
}
```

## [5]-[MSGPACK_ENGINE]

[MSGPACK_ENGINE]:
- Owner: `Pack`, the MessagePack engine — one `ExtensionCodec` carrying the C#-minted sixteen-byte `Hlc` cell as extension row `_EXT.hlc`, decoding through `Hlc.FromBytes` so the two-half little-endian layout has exactly one spelling; one configured `Decoder`/`Encoder` pair under `useBigInt64: true` and the `max*Length` ceilings; `schema(owned)` the composed byte-to-owned schema, `stream` the backpressured multi-frame walk, `encode`/`transfer` the canonical and zero-copy egress, and the `Alien`/`alien` foreign-ext seam.
- Law: the ext registry and the union tag are two tables, never a branch ladder — the `Hlc` ext row decodes the fixed cell into the kernel stamp inside the codec, and the op union's own discriminant dispatches at the landing schema; an unregistered ext surfaces as `ExtData`, never dropped, and `Pack.Alien` — the `Schema.declare` identity over the engine class — is the one seam a consuming row's dispatch arm composes, with `Pack.alien(ext, cell)` the encode-side mint so the class construction never leaks past this module.
- Law: the interner context threads the mint — `context.intern` is `Schema.decodeSync(Hlc.FromBytes)` handed into every ext decode, so the stamp mints once at the seam and no module-level mint singleton exists; a TS re-mint of the sixteen-byte layout is the named cross-language drift defect.
- Law: `useBigInt64: true` is i64 fidelity — HLC counters, sequence ordinals, and version counts decode as `bigint`; a decoder without it silently truncates past 2^53 and is the precision defect. The default cached key decoder already owns hot repeated map keys; no `keyDecoder` override exists to tune.
- Law: `sortKeys: true` on the encoder is canonical egress — re-encoded quarantine octets and transfer payloads are byte-stable; `transfer` returns the encoder's shared-buffer view for a zero-copy worker crossing, dead to this side the moment the buffer transfers.
- Growth: a domain ext row is one `register` call beside the `Hlc` row in the C#-owned positive type range; a ceiling axis is one `_CEILINGS` field.
- Boundary: the `CrdtOp` union, the oplog stream row with its gap Mealy, and the version-plane landings are the codec registry's; the built-in `EXT_TIMESTAMP` row stays registered on the default codec path for `Date` fields.
- Packages: `@msgpack/msgpack` (`Decoder`, `Encoder`, `ExtData`, `ExtensionCodec`, `decodeMultiStream`); `effect` (`Schema`, `Stream`); `../value/clock.ts` (`Hlc`).

```typescript
import { Decoder as PackDecoder, decodeMultiStream, Encoder as PackEncoder, ExtData, ExtensionCodec } from "@msgpack/msgpack"
import { Hlc } from "../value/clock.ts"

const _EXT = { hlc: 8 } as const
const _PACK_CEILINGS = {
  maxStrLength: 65536,
  maxBinLength: 16777216,
  maxArrayLength: 65536,
  maxMapLength: 16384,
  maxExtLength: 16777216,
} as const

declare namespace Pack {
  type Context = { readonly intern: (cell: Uint8Array) => Hlc }
  type Shape = {
    readonly ext: typeof _EXT
    readonly Alien: Schema.Schema<ExtData>
    readonly alien: (ext: number, cell: Uint8Array) => ExtData
    readonly schema: <A, I>(owned: Schema.Schema<A, I>) => Schema.Schema<A, Uint8Array>
    readonly stream: (frames: ReadableStream<Uint8Array> | AsyncIterable<Uint8Array>) => Stream.Stream<unknown, unknown>
    readonly encode: (value: unknown) => Uint8Array
    readonly transfer: (value: unknown) => Uint8Array
  }
}

const _context: Pack.Context = { intern: Schema.decodeSync(Hlc.FromBytes) }
const _cell = Schema.encodeSync(Hlc.FromBytes)

const _extensions: ExtensionCodec<Pack.Context> = new ExtensionCodec<Pack.Context>()
_extensions.register({
  type: _EXT.hlc,
  encode: (value) => (value instanceof Hlc ? _cell(value) : null),
  decode: (data, _type, context) => context.intern(data),
})

const _packOptions = { extensionCodec: _extensions, context: _context, useBigInt64: true, ..._PACK_CEILINGS } as const
const _packDecoder = new PackDecoder<Pack.Context>(_packOptions)
const _packEncoder = new PackEncoder<Pack.Context>({ ..._packOptions, sortKeys: true })

const Pack: Pack.Shape = {
  ext: _EXT,
  Alien: Schema.declare((input: unknown): input is ExtData => input instanceof ExtData),
  alien: (ext, cell) => new ExtData(ext, cell),
  schema: (owned) =>
    _lifted((octets) => _packDecoder.decode(octets), (value) => _packEncoder.encode(value)).pipe(
      Schema.compose(owned, { strict: false }),
    ),
  stream: (frames) => Stream.fromAsyncIterable(decodeMultiStream(frames, _packOptions), (defect) => defect),
  encode: (value) => _packEncoder.encode(value),
  transfer: (value) => _packEncoder.encodeSharedRef(value),
}
```

## [6]-[JSONPATCH_ENGINE]

[JSONPATCH_ENGINE]:
- Owner: `Patch`, the RFC 6902 engine — the six-op `Operation` union whose `path`/`from` fields carry the branch `Refined.JsonPointer` brand so a malformed pointer dies at admission, never inside the apply engine; `FromJson` the fused string codec; `apply` the clone-fenced value-rail application; `diff` the content-key-reconciled minimal patch; `guarded` the OCC egress prefixing `createTests` pre-image proofs; `encode` and `key` the egress projections; `pointer` the RFC 6901 traversal-and-token codec riding rfc6902's own `Pointer`.
- Law: rfc6902 stays the pure engine under the Schema boundary — the C#-authored `JsonPatchDocument` decodes once through the union, `applyPatch` returns one result slot per op with errors as values, and the first non-`null` slot folds through the `Match.instanceOf` triage into the interchange fault vocabulary at the consuming registry row; a raw operation array or a thrown apply fault never crosses.
- Law: mutation is fenced by a clone — the engine applies in place, so `apply` clones the target before `applyPatch` and the decoded base stays immutable on the rail.
- Law: the diff is parameterized, never enumerated — `_reconciled` is the one `VoidableDiff` hook: content-keyed values replace whole on key change and fall through to `diffAny` otherwise; a per-shape differ roster is the rejected form, and a new entity family is a hook arm.
- Law: egress mints through `_mintedDoc` — the engine emits RFC 6901-valid pointers by construction, so the interior `Schema.decodeSync` brand mint is the trusted-construction channel over proven inputs and the throw path is structurally unreachable; `diff` and `guarded` land the branded `Document` every consumer already speaks.
- Law: the egress is self-guarding — `guarded` emits `test` ops over the base pre-image ahead of the mutation so the C# OCC append refuses a stale patch before applying; `key` mints the patch content key through the one `Digest` content row over the encoded document, so `EntityEdit` identity is the branch identity.
- Law: `TestError` slots carry `{ actual, expected }` both ways — the drift report reads evidence as data; an op outside the closed six is contract-drift material graded at the contract page's verdict vocabulary.
- Law: pointer authority splits by altitude — the `Refined.JsonPointer` brand owns admission grammar at the schema seam, and `pointer` owns traversal and the `~0`/`~1` token codec through `Pointer.fromJSON`/`escapeToken`/`unescapeToken`, so a hand-rolled escape or path walk beside either owner is the re-implementation defect; `pointer.evaluate` reads a branded path against a value as `Option` off the evaluation triple's own parent-and-key presence — a resident `null` leaf is `Option.some(null)`, only a missing location folds to `Option.none`, and nothing throws.
- Growth: an apply-policy axis is one `_APPLY` field; a new reconciliation family is one `_reconciled` arm.
- Boundary: the `JsonPatchDocument` census row, the slot-to-fault classification (`WireFault.fromSlot`), and the OCC/conflict fault reasons are the codec registry's; the version-plane append that consumes the guard is the data branch's.
- Packages: `rfc6902` (`applyPatch`, `createPatch`, `createTests`), `rfc6902/diff` (`isDestructive`, `VoidableDiff`), `rfc6902/pointer` (`Pointer`, `escapeToken`, `unescapeToken`), `rfc6902/patch` (`InvalidOperationError`, `MissingError`, `TestError`), `rfc6902/util` (`clone`); `effect` (`Schema`, `ParseResult`, `Effect`, `Array`, `Option`, `Predicate`, `pipe`); `../value/schema.ts` (`Refined`); `../value/contentKey.ts` (`ContentKey`, `Digest`).

```typescript
import { applyPatch, createPatch, createTests } from "rfc6902"
import { isDestructive, type VoidableDiff } from "rfc6902/diff"
import type { InvalidOperationError, MissingError, TestError } from "rfc6902/patch"
import { escapeToken, Pointer, unescapeToken } from "rfc6902/pointer"
import { clone } from "rfc6902/util"
import { type ContentKey, Digest } from "../value/contentKey.ts"
import { Refined } from "../value/schema.ts"

const _op = Schema.Union(
  Schema.Struct({ op: Schema.Literal("add"), path: Refined.JsonPointer, value: Schema.Unknown }),
  Schema.Struct({ op: Schema.Literal("remove"), path: Refined.JsonPointer }),
  Schema.Struct({ op: Schema.Literal("replace"), path: Refined.JsonPointer, value: Schema.Unknown }),
  Schema.Struct({ op: Schema.Literal("move"), from: Refined.JsonPointer, path: Refined.JsonPointer }),
  Schema.Struct({ op: Schema.Literal("copy"), from: Refined.JsonPointer, path: Refined.JsonPointer }),
  Schema.Struct({ op: Schema.Literal("test"), path: Refined.JsonPointer, value: Schema.Unknown }),
)

const _document = Schema.Array(_op)
const _FromJson: Schema.Schema<Patch.Document, string> = Schema.parseJson(_document)
const _encodedPatch = Schema.encode(_FromJson)
const _mintedDoc = Schema.decodeSync(_document)

const _APPLY = { implicitArrayCreation: false } as const
const _utf8 = new TextEncoder()

const _reconciled: VoidableDiff = (input, output, pointer) =>
  Predicate.hasProperty(input, "key") && Predicate.isString(input.key)
    && Predicate.hasProperty(output, "key") && Predicate.isString(output.key)
    ? input.key === output.key ? [] : [{ op: "replace", path: pointer.toString(), value: output }]
    : undefined

declare namespace Patch {
  type Operation = Schema.Schema.Type<typeof _op>
  type Document = ReadonlyArray<Operation>
  type Slot = InvalidOperationError | MissingError | TestError
  type Shape = {
    readonly FromJson: typeof _FromJson
    readonly FromValue: typeof _document
    readonly destructive: (operation: Operation) => boolean
    readonly apply: (target: unknown, patch: Document) => Effect.Effect<unknown, readonly [Slot, number]>
    readonly diff: (base: unknown, next: unknown) => Document
    readonly guarded: (base: unknown, next: unknown) => Document
    readonly encode: (patch: Document) => Effect.Effect<string, ParseResult.ParseError>
    readonly key: (patch: Document) => Effect.Effect<ContentKey, ParseResult.ParseError>
    readonly pointer: {
      readonly of: (path: typeof Refined.JsonPointer.Type) => Pointer
      readonly evaluate: (path: typeof Refined.JsonPointer.Type, target: unknown) => Option.Option<unknown>
      readonly escape: (token: string) => string
      readonly unescape: (token: string) => string
    }
  }
}

const Patch: Patch.Shape = {
  FromJson: _FromJson,
  FromValue: _document,
  destructive: isDestructive,
  apply: (target, patch) =>
    Effect.suspend(() => {
      const successor = clone(target)
      const slots = applyPatch(successor, [...patch], _APPLY)
      return Option.match(
        Array.findFirst(
          Array.map(slots, (slot, index) => [slot, index] as const),
          (pair): pair is readonly [Patch.Slot, number] => pair[0] !== null,
        ),
        { onNone: () => Effect.succeed(successor), onSome: Effect.fail },
      )
    }),
  diff: (base, next) => _mintedDoc(createPatch(base, next, _reconciled)),
  guarded: (base, next) =>
    pipe(createPatch(base, next, _reconciled), (mutation) => _mintedDoc([...createTests(base, mutation), ...mutation])),
  encode: _encodedPatch,
  key: (patch) => Effect.flatMap(_encodedPatch(patch), (text) => Digest.mint("content", _utf8.encode(text))),
  pointer: {
    of: (path) => Pointer.fromJSON(path),
    evaluate: (path, target) =>
      pipe(Pointer.fromJSON(path).evaluate(target), (found) =>
        found.parent === null || Predicate.hasProperty(found.parent, found.key)
          ? Option.some(found.value)
          : Option.none()),
    escape: escapeToken,
    unescape: unescapeToken,
  },
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Cbor, Pack, Patch, Proto }
```
