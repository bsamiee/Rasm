# [TS_CORE_API_CBOR_X]

`cbor-x` owns canonical-CBOR decode of the C#-minted `SnapshotHeader` — the folder's one RFC 8949 boundary. Every decode runs through a configured-once `Decoder` (`useRecords:false`), never the top-level `decode` whose default record extension is a cbor-x-only dialect no C# writer speaks; decoded bytes re-verify their content key and cross `Schema.decodeUnknown` into owned vocabulary, never leaking raw.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cbor-x`
- package: `cbor-x` (MIT)
- module: subpath exports `.`, `./encode`, `./decode`, `./decode-no-eval`, `./index-no-eval` (ESM `./index.js`, CJS `./dist/node.cjs`)
- runtime: isomorphic — buffer `decode`/`encode` in browser + node; `DecoderStream`/`EncoderStream` are node `stream.Transform` only; `cbor-extract` native accel is an optional node addon
- peer: none
- rail: interchange/codec

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the codec pair and its tag/extension/policy model

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :-------------------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `Decoder`                         | decoder       | reusable configured-once decoder, one per policy                       |
|  [02]   | `Encoder extends Decoder`         | codec pair    | adds `encode` for the rare wire-owned egress                           |
|  [03]   | `Options`                         | policy record | flat interop policy; `useRecords:false` is the pivot                   |
|  [04]   | `Tag`                             | tagged value  | decoded CBOR tag, `Match`-dispatched by number                         |
|  [05]   | `Extension<T, R>`                 | tag extension | `addExtension` row decoding a tag into owned shape                     |
|  [06]   | `SizeLimitOptions`                | DoS bound     | array/map/object ceilings for `setSizeLimits`                          |
|  [07]   | `FLOAT32_OPTIONS`                 | float policy  | `useFloat32` precision: `NEVER`/`ALWAYS`/`DECIMAL_ROUND`/`DECIMAL_FIT` |
|  [08]   | `DecoderStream` / `EncoderStream` | node stream   | node `Transform` framing, not the Effect `Stream` path                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: decode, multi-frame, egress, and the global registries

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                              |
| :-----: | :---------------------------------------------- | :------- | :-------------------------------------------------------- |
|  [01]   | `new Decoder(options?).decode(bytes)`           | instance | one-frame decode → `Schema.decodeUnknown(SnapshotHeader)` |
|  [02]   | `Decoder.decodeMultiple(bytes, forEach?)`       | instance | concatenated snapshot/segment frames in one buffer        |
|  [03]   | `new Encoder(options?).encode(value) -> Buffer` | instance | rare wire-owned re-encode                                 |
|  [04]   | `encodeAsIterable` / `encodeAsAsyncIterable`    | static   | large-payload streaming egress, sync or async             |
|  [05]   | `encodeIter(iter)` / `decodeIter(iter)`         | static   | iterator codec, runtime-real and type-invisible           |
|  [06]   | `addExtension({ Class, tag, encode, decode })`  | static   | register a domain tag once at init; global registry       |
|  [07]   | `setSizeLimits(SizeLimitOptions)`               | static   | array/map/object ceilings before untrusted decode         |
|  [08]   | `roundFloat32(n)`                               | static   | half-even fit an egress float32                           |
|  [09]   | `clearSource()`                                 | static   | reset the shared-structure record state                   |
|  [10]   | `isNativeAccelerationEnabled`                   | property | `cbor-extract` native decode probe                        |
|  [11]   | `Decoder` (`cbor-x/decode-no-eval`)             | instance | strict-CSP build, record compiler off, no `new Function`  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one configured `Decoder` (`useRecords:false`, `mapsAsObjects:true`) owns every decode; the top-level `decode`/`Decoder` default (`useRecords:true`) engages the shared-`structures` record extension (tag-105), a cbor-x-only dialect no C# RFC 8949 writer speaks, so a top-level call on cross-language bytes is the drift defect.
- `SnapshotHeader` bytes are canonical at the C# writer (deterministic key order, shortest-form ints); `wire` decodes and re-verifies the `XxHash128` seed-zero content key through the one `value/identity` mint, never re-canonicalizing or re-minting — a second content-address notion is the drift defect.

[STACKING]:
- one codec per C# mint format: `interchange/codec` binds cbor-x to the canonical `SnapshotHeader` alone; a `codec` page picks its codec by the mint format and never overlaps a sibling arm.
- `effect` `Schema` (`.api/effect.md`): `Decoder.decode` yields an untyped value; `Schema.decodeUnknown(SnapshotHeaderSchema)` parses it once into the owned shape, lifting a `ParseError` into the `Effect` channel, and the interior never re-reads cbor-x output.
- `effect` `Match`/`Data` (`.api/effect.md`): a decoded `Tag` (259 map, 2/3 bignum, or an `addExtension` domain tag) dispatches through `Match.exhaustive`/`Data.taggedEnum().$match` into owned vocabulary; `tagUint8Array:true` keeps byte fields `Uint8Array` for content-key verification, and the `addExtension` rows are the table, never a tag-number `switch`.
- `effect` `Stream` + `@effect/platform` `FileSystem` (`.api/effect-platform.md`): the node persistence lane runs `FileSystem.stream` bytes through `decodeMultiple`/`Stream.fromAsyncIterable` frame-by-frame; the node `DecoderStream` Transform is bypassed and a `DecodeError`/`RangeError` caught at `Effect.try` becomes a replayable quarantine row on an `effect` `Queue`.
- `value/identity`: `wire` re-verifies the header content-key field (`Uint8Array`, preserved via `tagUint8Array`/`copyBuffers:false`) against the one `XxHash128` seed-zero mint `frame/artifact` delegates to, composing the mint and never re-implementing it.

[LOCAL_ADMISSION]:
- construct one `new Decoder({ useRecords:false, mapsAsObjects:true, tagUint8Array:true })` per policy at module scope; `setSizeLimits` array/map/object ceilings run before any untrusted decode.
- `wire` admits the shared-`structures`/`getStructures`/`saveStructures`/`maxSharedStructures` record protocol only for an intra-TS cache, never on a C#-crossing frame.
- reach for `cbor-x/decode-no-eval` in a strict-CSP scope where the record compiler's `new Function` is blocked; the `cbor-extract` native accelerator is node-only, probed through `isNativeAccelerationEnabled`.
- `index.d.ts` mislabels the size gate: it declares the runtime-absent `setMaxLimits` and phantoms `MAX_LIMITS_OPTIONS` while the runtime barrel exports `setSizeLimits`/`decodeIter`/`encodeIter`/`REUSE_BUFFER_MODE`; the `interchange/codec` owner carries a local `declare module 'cbor-x'` augmentation that makes `setSizeLimits`/`decodeIter`/`encodeIter` type-visible, so downstream calls `setSizeLimits` typed.

[RAIL_LAW]:
- Package: `cbor-x`
- Owns: canonical-CBOR decode of the `SnapshotHeader` (`Decoder`/`Encoder`, `decode`/`decodeMultiple`), the `Tag`/`addExtension` custom-tag registry, the `setSizeLimits` untrusted-frame DoS gate, the `encodeAsIterable`/`encodeAsAsyncIterable`/`encodeIter`/`decodeIter` chunked codec, the node `DecoderStream`/`EncoderStream` Transforms, and the `decode-no-eval` CSP build
- Accept: a configured-once `Decoder` (`useRecords:false`/`mapsAsObjects:true`), decode output crossing `Schema.decodeUnknown` into owned vocabulary, content-key re-verify through the one `value/identity` mint, `setSizeLimits` before untrusted decode, `Match`/`addExtension` tag dispatch
- Reject: top-level `decode`/`encode` on C#-crossing bytes, `useRecords:true` or the shared-structure protocol across the language boundary, a second content-address or re-mint, a raw cbor-x object in domain code, the node Transform streams where an `effect` `Stream` owns the frame walk, a `setMaxLimits`/`MAX_LIMITS_OPTIONS` call site
