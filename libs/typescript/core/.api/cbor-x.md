# [TS_CORE_API_CBOR_X]

`cbor-x` is the CBOR codec `interchange/codec` decodes the C#-minted `SnapshotHeader` with, and the one place the folder touches RFC 8949 bytes. The decode is byte-exact and content-stable: `Rasm.Persistence/Element` emits canonical CBOR (deterministic key order, shortest-form integers) so the `XxHash128` seed-zero content key over the frame is reproducible, and `wire` decodes-and-re-verifies without ever re-minting. The load-bearing surface is not the top-level `decode` — it is a configured-once `Decoder`/`Encoder` (`Encoder extends Decoder`) with `useRecords:false` (the default `true` engages cbor-x's proprietary shared-structure record extension, tag-105, which no C# CBOR writer speaks and which is the named cross-language drift defect), `mapsAsObjects:true`, `useBigInt64`-equivalent `largeBigIntToFloat:false` for i64 fidelity, plus the `Tag`/`addExtension` custom-tag registry, the runtime `setSizeLimits` untrusted-frame DoS gate feeding `interchange/codec`, the `encodeAsIterable`/`encodeAsAsyncIterable`/`encodeIter`/`decodeIter` chunked-codec mirrors, the node `DecoderStream`/`EncoderStream` Transforms, and the `./decode-no-eval`/`./index-no-eval` CSP-safe builds plus the `cbor-extract` native decode accelerator (`isNativeAccelerationEnabled`). One shipped quirk the boundary owner internalizes: in catalog-bound the runtime barrel exports `setSizeLimits`/`decodeIter`/`encodeIter`/`REUSE_BUFFER_MODE` while `index.d.ts` mislabels the size gate `setMaxLimits` (runtime-absent) and re-exports a phantom `MAX_LIMITS_OPTIONS`. Every decoded value lands through a `Schema.decodeUnknown` into owned vocabulary; the codec instance never leaks raw.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cbor-x`
- package: `cbor-x`
- license: `MIT`
- effect-peer: none — decode output crosses into `effect` `Schema.decodeUnknown` at the `interchange/codec` seam (`.api/effect.md`); no bundled peer
- catalog-verdict: KEEP — the CBOR arm of the multi-codec wire plane: `@bufbuild/protobuf` owns the descriptor-typed proto families, `@msgpack/msgpack` the MessagePack CRDT union, `rfc6902` the JSON-Patch egress, this owns canonical CBOR — one codec per C# mint format, no overlap
- runtime: isomorphic (`module` `./index.js`, `main` `./dist/node.cjs`); buffer `decode`/`encode` run in browser + node; `DecoderStream`/`EncoderStream` are node `stream.Transform` only; `cbor-extract` native accel is an optional node addon
- modules / subpaths: `.` (full codec), `./encode`, `./decode`, `./decode-no-eval` + `./index-no-eval` (CSP-safe, no `new Function` record compiler), `./package.json`
- surface drift (catalog-bound): runtime `index.js` exports `setSizeLimits`/`decodeIter`/`encodeIter`/`REUSE_BUFFER_MODE`; `index.d.ts` instead declares `setMaxLimits` (runtime-absent) and phantoms `MAX_LIMITS_OPTIONS` — the `interchange/codec` boundary owner reconciles with a local `declare module 'cbor-x'` augmentation and calls `setSizeLimits`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the codec pair and its tag/extension model
- rail: interchange/codec
- `Decoder` is the reusable configured-once decoder; `Encoder extends Decoder` adds `encode` for the rare egress crossing. `Tag` wraps a CBOR tagged value; `Extension`/`addExtension` register a domain tag decoding INTO owned vocabulary. `Options` is one flat policy record — interop-critical fields are `useRecords`, `mapsAsObjects`, `structures`, `tagUint8Array`.

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY] | [CONSUMER_BOUNDARY]                                                    |
| :-----: | :--------------------------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `Decoder` (`.decode`, `.decodeMultiple`) | decoder       | `SnapshotHeader` decode; one instance per configured policy            |
|  [02]   | `Encoder extends Decoder` (`.encode`)    | codec pair    | the rare `wire`-owned egress; decode + encode in one instance          |
|  [03]   | `Options`                                | policy record | `useRecords:false`/`mapsAsObjects:true`/`tagUint8Array` interop set    |
|  [04]   | `Tag` (`.value`, `.tag`)                 | tagged value  | decoded CBOR tag (259 map, 2/3 bignum, domain) `Match`-dispatched      |
|  [05]   | `Extension<T, R>`                        | tag extension | `addExtension` row (`Class`/`tag`/`encode`/`decode`) → owned shape     |
|  [06]   | `SizeLimitOptions`                       | DoS bound     | `maxArraySize`/`maxMapSize`/`maxObjectSize` for `setSizeLimits`        |
|  [07]   | `FLOAT32_OPTIONS`                        | float policy  | `NEVER`/`ALWAYS`/`DECIMAL_ROUND`/`DECIMAL_FIT`; `useFloat32` precision |
|  [08]   | `DecoderStream` / `EncoderStream`        | node stream   | node `Transform` framed decode; NOT the Effect `Stream` path           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: decode, multi-frame, egress, and the global registries
- rail: interchange/codec
- The interop-correct entry is `new Decoder({ useRecords:false, mapsAsObjects:true }).decode(bytes)`, never the top-level `decode` (which binds a shared default `useRecords:true` instance). `decodeMultiple` walks concatenated frames; `encodeAsAsyncIterable` chunks egress. `addExtension`/`setSizeLimits` mutate global state and run once at module init.

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                         |
| :-----: | :------------------------------------------------ | :------------- | :---------------------------------------------------------- |
|  [01]   | `new Decoder(options?).decode(bytes)`             | decode         | one-frame decode → `Schema.decodeUnknown(SnapshotHeader)`   |
|  [02]   | `Decoder.decodeMultiple(bytes, forEach?)`         | multi-frame    | concatenated snapshot/segment frames in one buffer          |
|  [03]   | `new Encoder(options?).encode(value): Buffer`     | egress         | rare `wire`-owned re-encode; `variableMapSize`/`useFloat32` |
|  [04]   | `encodeAsIterable(v)`                             | chunked write  | large-payload streaming egress as a sync iterable           |
|  [05]   | `encodeAsAsyncIterable(v)`                        | chunked write  | large-payload streaming egress as an async iterable         |
|  [06]   | `encodeIter(iter)` / `decodeIter(iter)`           | chunked codec  | iterator encode/decode (runtime-real, type-invisible)       |
|  [07]   | `addExtension({ Class, tag, encode, decode })`    | tag registry   | register a domain CBOR tag once at init; global registry    |
|  [08]   | `setSizeLimits(SizeLimitOptions)`                 | DoS gate       | array/map/object ceilings before untrusted decode           |
|  [09]   | `roundFloat32(n)`                                 | float fit      | half-even fit an egress float32                             |
|  [10]   | `clearSource()`                                   | shared reset   | reset the shared-structure record state                     |
|  [11]   | `isNativeAccelerationEnabled`                     | accel probe    | `cbor-extract` native decode probe                          |
|  [12]   | `import { Decoder } from "cbor-x/decode-no-eval"` | CSP build      | strict-CSP lane — record compiler off, no `new Function`    |

## [04]-[IMPLEMENTATION_LAW]

[CBOR_TOPOLOGY]:
- one configured decoder, never the shared default: the top-level `decode`/`Decoder`/`Encoder` bind cbor-x's default `Options` with `useRecords:true`, which serializes/reads objects through the proprietary shared-`structures` record extension (tag-105) — a cbor-x-only wire dialect. C#-minted CBOR is standard RFC 8949, so `interchange/codec` constructs `new Decoder({ useRecords:false, mapsAsObjects:true })` once and reuses it; a top-level `decode` call on cross-language bytes is the drift defect.
- content-stable, decode-and-verify: `SnapshotHeader` bytes are canonical (deterministic key order, shortest-form ints) at the C# writer, so the `XxHash128` seed-zero key over the frame is reproducible. `wire` decodes and re-verifies the key through the one `value/identity` mint (`frame` delegates); it never re-canonicalizes or re-mints — a second content-address notion is the named defect.
- tags decode into vocabulary: a decoded `Tag` (259 non-string-key map, 2/3 bignum, or a domain tag registered via `addExtension`) is `Match`-dispatched into an owned shape at the seam; `tagUint8Array:true` keeps byte fields as `Uint8Array` for content-key verification rather than a copied `Buffer`.
- the DoS gate precedes decode: untrusted frames pass `setSizeLimits` array/map/object ceilings; an overrun or malformed frame throws, caught at the `Effect.try` boundary and routed to the `interchange/codec` poison buffer, never a raw throw into domain code.
- the boundary owner internalizes the shipped surface drift: in catalog-bound the runtime exports `setSizeLimits`/`decodeIter`/`encodeIter`/`REUSE_BUFFER_MODE` while `index.d.ts` declares the runtime-absent `setMaxLimits` and phantoms `MAX_LIMITS_OPTIONS`; a local `declare module 'cbor-x'` augmentation at the `interchange/codec` owner reconciles the type surface so downstream code composes `setSizeLimits` typed, never re-discovering the mismatch — the exact provider-quirk-into-owner pattern.

[INTEGRATION_LAW]:
- Stack with the codec siblings (`@bufbuild/protobuf` / `@msgpack/msgpack` / `rfc6902`, `core/.api/`): the interchange plane is multi-codec and `interchange/codec` is the CBOR arm — `@bufbuild/protobuf` owns the descriptor-typed proto families (`ElementGraph`, `FaultDetail`, `GeometryPayload`, the RPC suite) and never mints a `SnapshotHeader`, `@msgpack/msgpack` owns the `CrdtOpWire`/`OpLog` MessagePack union, `rfc6902` owns the `JsonPatchDocument` egress. A `codec` page picks ONE codec by the C# mint format; a proto page never reaches for CBOR and `interchange/codec` never reaches for proto — the descriptor reflection and the `interchange/contract` drift gate are `@bufbuild/protobuf`'s altitude, not this one's.
- Stack with `effect` `Schema` (`.api/effect.md`): `Decoder.decode` produces an untyped value at the boundary; `Schema.decodeUnknown(SnapshotHeaderSchema)` decodes-once into the owned shape, lifting a `ParseError` into the `Effect` error channel. One `Schema` owns the decoded `SnapshotHeader`; the interior never re-reads cbor-x output.
- Stack with `value/identity`: the header's content-key field (a `Uint8Array`, preserved via `tagUint8Array`/`copyBuffers:false`) is re-verified against the `XxHash128` seed-zero mint `frame/artifact` delegates to; `wire` composes the mint, never re-implements it.
- Stack with `effect` `Match`/`Data` (`.api/effect.md`): decoded `Tag` values and CBOR union discriminants dispatch through `Match.exhaustive`/`Data.taggedEnum().$match` into the owned vocabulary; `addExtension` rows are the table, never an `if`/`switch` ladder over tag numbers.
- Stack with `effect` `Stream` + `@effect/platform` `FileSystem` (`.api/effect-platform.md`): in the node persistence lane `FileSystem.stream` yields bytes that `decodeMultiple`/`Stream.fromAsyncIterable` walk frame-by-frame; the node `DecoderStream` Transform is bypassed — the Effect `Stream` owns backpressure and the codec stays a pure buffer function.
- Stack with `interchange/codec`: a `DecodeError`/`RangeError` (frame incomplete, limit exceeded) caught at `Effect.try` becomes a quarantine row on an `effect` `Queue`, replayable, never a process throw.

[LOCAL_ADMISSION]:
- construct one `new Decoder({ useRecords:false, mapsAsObjects:true, tagUint8Array:true })` per policy at module scope; never call top-level `decode`/`encode` on cross-language bytes (they bind `useRecords:true`).
- the shared-`structures`/`getStructures`/`saveStructures`/`maxSharedStructures` record protocol is a cbor-x-internal persistence optimization — admitted only for a `wire`-owned intra-TS cache, never on a C#-crossing frame.
- decoded output crosses `Schema.decodeUnknown` before any consumer reads it; a bare cbor-x object reaching domain code is the leak defect.
- reach for `cbor-x/decode-no-eval` in a strict-CSP browser scope where the record compiler's `new Function` is blocked; the accelerator `cbor-extract` is node-only and probed via `isNativeAccelerationEnabled`.
- call the runtime-true `setSizeLimits`, never the runtime-absent `setMaxLimits`, and never cite `MAX_LIMITS_OPTIONS`; the `interchange/codec` owner carries the local `declare module` augmentation that makes `setSizeLimits`/`decodeIter`/`encodeIter` type-visible.

[RAIL_LAW]:
- Package: `cbor-x`
- Owns: canonical-CBOR decode of the `SnapshotHeader` (`Decoder`/`Encoder`, `decode`/`decodeMultiple`), the `Tag`/`addExtension` custom-tag registry, the `setSizeLimits` untrusted-frame DoS gate, the `encodeAsIterable`/`encodeAsAsyncIterable`/`encodeIter`/`decodeIter` chunked codec, the node `DecoderStream`/`EncoderStream` Transforms, and the `decode-no-eval` CSP build
- Accept: a configured-once `Decoder` with `useRecords:false`/`mapsAsObjects:true`, decode output crossing `Schema.decodeUnknown` into owned vocabulary, content-key re-verify through the one `value/identity` mint, `setSizeLimits` before untrusted decode, `Match`/`addExtension` tag dispatch
- Reject: top-level `decode`/`encode` on C#-crossing bytes, `useRecords:true` or the shared-structure protocol across the language boundary, a second content-address or re-mint, a raw cbor-x object in domain code, the node Transform streams where an Effect `Stream` owns the frame walk, a `setMaxLimits`/`MAX_LIMITS_OPTIONS` call site (runtime-absent in catalog-bound — the gate is `setSizeLimits`)
