# [WIRE_CRDT]

`codec/crdt.ts` is the MessagePack arm of the multi-codec plane and the CRDT op boundary: `Pack`, the configured-once MessagePack engine every msgpack census family shares — extension registry carrying the C#-minted 16-byte `Hlc` cell, kernel interner threaded as decode context, `useBigInt64` i64 fidelity, untrusted-frame ceilings — and `CrdtOp`, the decoded op family the C# `CrdtOpWire` union lands as. `state/crdt`'s merge algebra is generic over the op vocabulary; this family is its wire-minted instance, published through `#vocab`, so `state` folds ops without importing `wire` machinery. `[R9]` rides this page: `@msgpack/msgpack` drops if the platform `MsgPack` module verifiably covers standalone-payload decode with extension and context parity.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                              |
| :-----: | :--------------- | :------------------------------------------------------------------------------------ |
|   [1]   | `MSGPACK_ENGINE` | `Pack`: the ext registry, the interner context, the configured decoder, the byte schema |
|   [2]   | `OP_FAMILY`      | `CrdtOp`: the decoded C# op union, its decode statics, the zero-copy egress            |

## [2]-[MSGPACK_ENGINE]

- Owner: `Pack` — the folder's one MessagePack seam: `_EXT` the C#-owned extension type-byte row, `_extensions` the `ExtensionCodec` whose `Hlc` row decodes the fixed 16-byte cell through the kernel interner carried on `context`, `_decoder`/`_encoder` the configured instances, and the generic byte schema plus stream lift sibling msgpack pages (`codec/oplog.ts`, `codec/version.ts`) compose.
- Packages: `@msgpack/msgpack` — the only importing module; the census fences msgpack to this page's dependents, and the version pin lives in the one workspace catalog.
- Entry: `Pack.schema(owned)` the composed byte→owned schema; `Pack.stream(family)` the backpressured multi-frame walk; `Pack.encode`/`Pack.transfer` the canonical and zero-copy egress.
- Growth: a new C#-minted extension type is one `_extensions.register` row decoding INTO owned vocabulary — the type-byte is C#-owned positive range and never re-numbered TS-side; a new ceiling axis is a `_CEILINGS` field.
- Law: the ext row is the mint seam, decode-once and encode-true — the 16-byte `Hlc` cell decodes through `context.intern` inside the extension decoder (the kernel `Hlc.FromBytes` layout twin, run synchronously), so interning rides decode state, never a module singleton, and the encode arm re-emits the same sixteen bytes through the twin so a decoded op round-trips the worker transfer without flattening its stamps; a TS re-mint of the cell layout is the named cross-language drift defect.
- Law: `useBigInt64: true` is non-negotiable — the HLC physical counter, ordinals, and version-vector entries are i64; a decoder without it truncates past 2^53 silently.
- Law: the top-level `decode` is unreachable by construction — it cannot see the shared `ExtensionCodec`, so every decode rides the configured instance; ceilings (`maxStrLength`, `maxBinLength`, `maxArrayLength`, `maxMapLength`, `maxExtLength`) are instance options, armed before any untrusted byte.
- Law: an unregistered extension surfaces as `ExtData` and dispatches through `Match.instanceOf` at the seam into a `ParseError` — never dropped, never a raw `ExtData` in domain flow.
- Boundary: engine throws fold to `ParseResult.Type` inside the transform — one admission rail; the built-in `EXT_TIMESTAMP` (-1) stays registered on the default codec path for `Date` fields and crosses into `DateTime` at the owned schema.

```typescript
import { Decoder, Encoder, ExtensionCodec, decodeMultiStream } from "@msgpack/msgpack"
import { Hlc } from "@rasm/ts/kernel"
import { Either, Option, ParseResult, Schema, Stream, type Types } from "effect"
import type { Inventory } from "../contract/drift.ts"
import { WireFault } from "../fault/quarantine.ts"

const _EXT = { hlc: 8 } as const
const _CEILINGS = { maxStrLength: 65536, maxBinLength: 16777216, maxArrayLength: 65536, maxMapLength: 16384, maxExtLength: 16777216 } as const

declare namespace Pack {
  type Context = { readonly intern: (cell: Uint8Array) => Hlc }
  type Shape = Types.Simplify<{
    readonly ext: typeof _EXT
    readonly schema: <A, I>(owned: Schema.Schema<A, I>) => Schema.Schema<A, Uint8Array>
    readonly stream: (family: Inventory.Family) => (frames: ReadableStream<Uint8Array> | AsyncIterable<Uint8Array>) => Stream.Stream<unknown, WireFault>
    readonly encode: (value: unknown) => Uint8Array
    readonly transfer: (value: unknown) => Uint8Array
  }>
}

const _context: Pack.Context = { intern: Schema.decodeSync(Hlc.FromBytes) }

const _cell = Schema.encodeSync(Hlc.FromBytes)

const _extensions: ExtensionCodec<Pack.Context> = new ExtensionCodec<Pack.Context>()
_extensions.register({
  type: _EXT.hlc,
  encode: (value) => (value instanceof Hlc ? _cell(value) : null),
  decode: (data, _type, context) => context.intern(data),
})

const _decoder = new Decoder<Pack.Context>({ extensionCodec: _extensions, context: _context, useBigInt64: true, ..._CEILINGS })
const _encoder = new Encoder<Pack.Context>({ extensionCodec: _extensions, context: _context, useBigInt64: true, sortKeys: true })

const _bytes: Schema.Schema<unknown, Uint8Array> = Schema.transformOrFail(Schema.Uint8ArrayFromSelf, Schema.Unknown, {
  strict: true,
  decode: (octets, _options, ast) =>
    Either.try({ try: () => _decoder.decode(octets), catch: (defect) => new ParseResult.Type(ast, octets, String(defect)) }),
  encode: (value, _options, ast) =>
    Either.try({ try: () => _encoder.encode(value), catch: (defect) => new ParseResult.Type(ast, value, String(defect)) }),
})

const Pack: Pack.Shape = {
  ext: _EXT,
  schema: (owned) => _bytes.pipe(Schema.compose(owned, { strict: false })),
  stream: (family) => (frames) =>
    Stream.fromAsyncIterable(
      decodeMultiStream(frames, { extensionCodec: _extensions, context: _context, useBigInt64: true, ..._CEILINGS }),
      (defect) => new WireFault({ family, reason: "malformed", detail: String(defect), evidence: Option.none() }),
    ),
  encode: (value) => _encoder.encode(value),
  transfer: (value) => _encoder.encodeSharedRef(value),
}
```

## [3]-[OP_FAMILY]

- Owner: `CrdtOp` — one `Schema.Union` of tagged op cases bound to a same-name `const`-plus-`type` pair; every case carries its `hlc` stamp and `actor` coordinate, and the family value carries the decode statics and the case constructors under one export.
- Entry: `CrdtOp.FromBytes` for a single buffered op; `CrdtOp.decode(octets)` the one-shot rail; construction for interior fixtures rides each case's `.make`.
- Receipt: the decoded op IS the evidence — `hlc` orders it, `actor` attributes it, and the case payload carries exactly the mutation; `state/crdt/merge` folds these values as its wire-minted instance through `#vocab`.
- Growth: a new C# op kind is one tagged case plus the union row — every exhaustive consumer (`state` merge arms, `store` journal folds) breaks loudly until its arm exists; the case roster is pinned by the `tests/contracts` corpus fixtures, and roster drift fails there before it fails here.
- Law: the union decodes as one tagged family — the C# writer mints `_tag`-discriminated maps, the `Hlc` fields arrive pre-interned by the extension row and the kernel class re-proves them at the field, and `bigint` counters flow through `Schema.BigIntFromSelf`; a positional-array op read or a `kind` field beside `_tag` is the rejected shape.
- Law: ops are immutable evidence — no op mutates after decode, and merge semantics live in `state`; this page lands values and owns zero fold arms.
- Law: zero-copy egress is transfer-scoped — `Pack.transfer` returns a view over the encoder's internal buffer, legal only as a `Transferable` handed to the `browser/transport` worker in the same expression; a held reference to the shared view is the aliasing defect.
- Boundary: the streaming journal walk is `codec/oplog.ts`; version shapes on the same engine are `codec/version.ts`; merge law, convergence fixtures, and causal order are `state`'s pages.

```typescript
import { Effect, type ParseResult, Schema } from "effect"

const _Assign = Schema.TaggedStruct("Assign", {
  key: Schema.NonEmptyString,
  path: Schema.NonEmptyString,
  value: Schema.Unknown,
  hlc: Hlc,
  actor: Schema.NonEmptyString,
})

const _Adjoin = Schema.TaggedStruct("Adjoin", {
  key: Schema.NonEmptyString,
  member: Schema.NonEmptyString,
  hlc: Hlc,
  actor: Schema.NonEmptyString,
})

const _Retire = Schema.TaggedStruct("Retire", {
  key: Schema.NonEmptyString,
  member: Schema.NonEmptyString,
  observed: Schema.Array(Hlc),
  hlc: Hlc,
  actor: Schema.NonEmptyString,
})

const _Splice = Schema.TaggedStruct("Splice", {
  key: Schema.NonEmptyString,
  anchor: Schema.NonEmptyString,
  run: Schema.Array(Schema.Unknown),
  hlc: Hlc,
  actor: Schema.NonEmptyString,
})

const _Tick = Schema.TaggedStruct("Tick", {
  key: Schema.NonEmptyString,
  delta: Schema.BigIntFromSelf,
  hlc: Hlc,
  actor: Schema.NonEmptyString,
})

const _op = Schema.Union(_Assign, _Adjoin, _Retire, _Splice, _Tick)

declare namespace CrdtOp {
  type Op = Schema.Schema.Type<typeof _op>
  type Tag = Op["_tag"]
}
type CrdtOp = CrdtOp.Op

const _FromBytes: Schema.Schema<CrdtOp, Uint8Array> = Pack.schema(_op)

const CrdtOp: typeof _op & {
  readonly FromBytes: Schema.Schema<CrdtOp, Uint8Array>
  readonly decode: (octets: Uint8Array) => Effect.Effect<CrdtOp, ParseResult.ParseError>
} = Object.assign(_op, {
  FromBytes: _FromBytes,
  decode: Schema.decodeUnknown(_FromBytes),
})

// --- [EXPORTS] --------------------------------------------------------------------------

export { CrdtOp, Pack }
```
