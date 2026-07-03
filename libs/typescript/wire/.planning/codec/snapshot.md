# [WIRE_SNAPSHOT]

`codec/snapshot.ts` is the CBOR arm of the multi-codec plane: the one place the folder touches RFC 8949 bytes. It decodes the C#-minted `SnapshotHeader` — canonical CBOR from `Rasm.Persistence/Element`, deterministic key order and shortest-form integers — through a configured-once `cbor-x` decoder whose interop posture (`useRecords: false`, `mapsAsObjects: true`, `tagUint8Array: true`) is the cross-language contract, re-verifies the content key through the kernel mint without ever re-canonicalizing, and feeds `store` snapshot intake through `#vocab` at the app root. The module carries the shipped 1.6.4 type-surface quirk as a local augmentation so `setSizeLimits` composes typed, and the DoS ceiling registers as a Layer node, never a domain-module load statement. `[R10]` rides this page: the canonical-decode member surface is verified before `SnapshotHeader` goes load-bearing.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                          |
| :-----: | :------------ | :-------------------------------------------------------------------------------- |
|   [1]   | `CBOR_ENGINE` | the configured decoder, the quirk augmentation, the ceiling Layer, the byte schema |
|   [2]   | `HEADER`      | the `SnapshotHeader` owner: decode, key re-verify, multi-frame walk                |

## [2]-[CBOR_ENGINE]

- Owner: the interior engine — one `Decoder` constructed at module scope under the `_WIRE` policy row, the `declare module "cbor-x"` augmentation reconciling the 1.6.4 type drift, the `_cbor` transform folding decode throws onto the one `ParseError` rail, and `_GateLive`, the `Layer<never>` registration node that arms the global size ceilings.
- Packages: `cbor-x` 1.6.4 — the only module in the folder importing it; the census fences `cbor-x` to this page.
- Entry: interior only — `SnapshotHeader` composes `_cbor`; no raw decoder value or undecoded CBOR object leaves this module.
- Growth: a domain CBOR tag is one `addExtension` row registered inside `_GateLive` beside the ceilings — the tag decodes INTO owned vocabulary at registration, never surfaces as a raw `Tag` in domain flow.
- Law: the configured instance is the only decode path — the top-level `decode` binds a shared default with `useRecords: true`, cbor-x's proprietary tag-105 record dialect no C# writer speaks; a top-level `decode` call on cross-language bytes is the named drift defect.
- Law: the quirk capture is the augmentation, co-located here — 1.6.4 runtime exports `setSizeLimits` while `index.d.ts` mislabels it `setMaxLimits` and phantoms `MAX_LIMITS_OPTIONS`; the augmentation declares only verified runtime truth, the mislabeled member is never called, and downstream composes the corrected surface without re-discovering the mismatch.
- Law: the ceiling precedes untrusted decode structurally — `setSizeLimits` mutates global engine state, so it rides a `Layer.effectDiscard` registration node the app root merges before any snapshot Layer; a load-time statement in this module is the boot-edge violation the Layer form exists to prevent.
- Law: `tagUint8Array: true` keeps byte fields as `Uint8Array` — content-key cells arrive as views suitable for verification, never copied `Buffer`s; the strict-CSP browser lane swaps the import specifier to `cbor-x/decode-no-eval` with zero other edits.
- Boundary: an overrun or malformed frame is a `ParseError` at this seam; the quarantine divert that classifies and holds it composes at the consuming stream, `fault/quarantine.ts`.

```typescript
import { Decoder, setSizeLimits } from "cbor-x"
import { Either, Layer, ParseResult, Schema, Effect } from "effect"

declare module "cbor-x" {
  function setSizeLimits(limits: { readonly maxArraySize?: number; readonly maxMapSize?: number; readonly maxObjectSize?: number }): void
}

const _WIRE = { useRecords: false, mapsAsObjects: true, tagUint8Array: true } as const
const _CEILINGS = { maxArraySize: 65536, maxMapSize: 16384, maxObjectSize: 1048576 } as const

const _decoder = new Decoder(_WIRE)

const _GateLive: Layer.Layer<never> = Layer.effectDiscard(Effect.sync(() => setSizeLimits(_CEILINGS)))

const _cbor: Schema.Schema<unknown, Uint8Array> = Schema.transformOrFail(Schema.Uint8ArrayFromSelf, Schema.Unknown, {
  strict: true,
  decode: (octets, _options, ast) =>
    Either.try({ try: () => _decoder.decode(octets), catch: (defect) => new ParseResult.Type(ast, octets, String(defect)) }),
  encode: (value, _options, ast) => Either.left(new ParseResult.Forbidden(ast, value, "<decode-only>")),
})
```

## [3]-[HEADER]

- Owner: `SnapshotHeader` — one `Schema.Class`: the wire-owned decoded shape (no sibling folder owns snapshot headers), its decode statics, the parity re-verify, and the concatenated-frame walk all ride the class.
- Entry: `SnapshotHeader.FromBytes` the composed byte schema; `SnapshotHeader.verified(octets)` the decode-and-verify rail — decode, re-mint the key over the held octets through the kernel delegate, refuse a mismatch as `WireFault` reason `parity`; `SnapshotHeader.frames(octets)` the `decodeMultiple` walk over concatenated segment frames as a `Stream`.
- Receipt: the header IS the receipt — content key, element census, journal frontier, ordered segment rows, mint instant; `store/journal` snapshot intake consumes it via `#vocab` wiring at the app root, never a `store -> wire` import.
- Growth: a new header field is one field row here mirroring the C# mint; a new segment axis is a field on the segment block — the census row and the contracts-corpus fixture move in the same wave.
- Law: decode-and-verify, never re-mint — the key re-verifies through `ContentKey.mint` over the exact held octets (LE→BE normalization lives inside the kernel delegate); re-canonicalizing the CBOR, hashing a re-encode, or a second content-address notion is the invariant-2 defect.
- Law: the header's own segments are ordered facts — `ordinal` is dense from zero and `Schema.NonEmptyArray` proves at least one segment at the type; a gap is the C# writer's impossibility, so a decoded gap dies as defect rather than folding to a fault arm no consumer can act on.
- Law: `frontier` is the journal watermark the snapshot folds up to — `store` resumes the op stream strictly after it; the field decodes through the kernel `Hlc` cell admission, never a hand-read byte pair.
- Boundary: framed arrival over the artifact rail composes `frame/artifact.ts` reassembly first and hands assembled octets here; `[R10]` gates the load-bearing claim on the verified member surface of the canonical decode.

```typescript
import { ContentKey, Hlc } from "@rasm/ts/kernel"
import { Effect, Option, ParseResult, Schema, Stream } from "effect"
import { WireFault } from "../fault/quarantine.ts"

const _Segment = Schema.Struct({
  ordinal: Schema.Int.pipe(Schema.nonNegative()),
  extent: Schema.Int.pipe(Schema.positive()),
  key: ContentKey.FromCell,
})

class SnapshotHeader extends Schema.Class<SnapshotHeader>("SnapshotHeader")({
  key: ContentKey.FromCell,
  element: Schema.Int.pipe(Schema.nonNegative()),
  frontier: Hlc.FromCell,
  segments: Schema.NonEmptyArray(_Segment),
  minted: Schema.DateTimeUtc,
}) {
  static readonly GateLive: Layer.Layer<never> = _GateLive
  static readonly FromBytes: Schema.Schema<SnapshotHeader, Uint8Array> = _cbor.pipe(Schema.compose(SnapshotHeader, { strict: false }))
  static readonly decode: (octets: Uint8Array) => Effect.Effect<SnapshotHeader, ParseResult.ParseError> = Schema.decodeUnknown(SnapshotHeader.FromBytes)
  static readonly verified = (octets: Uint8Array): Effect.Effect<SnapshotHeader, ParseResult.ParseError | WireFault> =>
    Effect.gen(function* () {
      const header = yield* SnapshotHeader.decode(octets)
      const minted = yield* ContentKey.mint(octets)
      return ContentKey.same(minted, header.key)
        ? header
        : yield* new WireFault({
            family: "SnapshotHeader",
            reason: "parity",
            detail: "<header-key-mismatch>",
            evidence: Option.some({ actual: minted, expected: header.key }),
          })
    })
  static readonly frames = (octets: Uint8Array): Stream.Stream<SnapshotHeader, ParseResult.ParseError> =>
    Stream.fromIterable(Option.getOrElse(Option.fromNullable(_decoder.decodeMultiple(octets)), () => [])).pipe(
      Stream.mapEffect(Schema.decodeUnknown(SnapshotHeader), { concurrency: 1 }),
    )
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { SnapshotHeader }
```
