# [API_CATALOGUE] @msgpack/msgpack

`@msgpack/msgpack` supplies MessagePack binary framing at the interchange boundary: synchronous and streaming encode/decode free functions, reusable stateful `Encoder`/`Decoder` classes carrying per-instance options (`useBigInt64`, the five `max*Length` decode bounds, `sortKeys`), a `<ContextType>`-threaded `ExtensionCodec` registry, and the built-in timestamp codec. The interchange branch reuses one `Decoder({ useBigInt64: true })` across the snapshot, sync-segment stream, and CRDT-op payload, mapping 64-bit integers to `bigint`; the write mirror stays out of the browser branch (`encode` is `Option.none` on the messagepack codec row).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@msgpack/msgpack`
- package: `@msgpack/msgpack` (3.1.3, ISC)
- module format: dual ESM (`dist.esm/index.mjs`) + CJS (`dist.cjs/index.cjs`); no `exports` map, so resolution rides `main`/`module`/`types`; barrel type at `dist.esm/index.d.ts`
- runtime target: isomorphic (browser, node, worker), `engines.node >= 18`; `useBigInt64`/`decodeMulti`/stream decode depend on ES2020 `DataView.getBigInt64`/`getBigUint64` and async iteration
- asset: pure-TypeScript runtime library; no native addon, no wasm
- rail: wire

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec classes and errors
- rail: wire

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]    | [RAIL]                                   |
| :-----: | :------------------ | :--------------- | :--------------------------------------- |
|  [01]   | `Encoder<C>`        | stateful encoder | reusable encode with `EncoderOptions<C>` |
|  [02]   | `Decoder<C>`        | stateful decoder | reusable decode with `DecoderOptions<C>` |
|  [03]   | `ExtensionCodec<C>` | extension codec  | typed extension type registry            |
|  [04]   | `ExtData`           | ext data carrier | unregistered extension type + raw bytes  |
|  [05]   | `DecodeError`       | decode failure   | invalid MessagePack data error           |

[PUBLIC_TYPE_SCOPE]: option shapes
- rail: wire

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]     | [RAIL]                                                                                    |
| :-----: | :------------------ | :---------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `EncoderOptions<C>` | options interface | `useBigInt64`, `maxDepth` (100), `initialBufferSize` (2048), `sortKeys`, `forceFloat32`, `forceIntegerToFloat`, `ignoreUndefined`, `extensionCodec` |
|  [02]   | `DecoderOptions<C>` | options interface | `useBigInt64`, `rawStrings`, `maxStrLength`/`maxBinLength`/`maxArrayLength`/`maxMapLength`/`maxExtLength` (each `UINT32_MAX` default), `keyDecoder` (`null` disables), `mapKeyConverter`, `extensionCodec` |

[PUBLIC_TYPE_SCOPE]: extension codec types
- rail: wire

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [RAIL]                                                       |
| :-----: | :------------------------ | :-------------- | :----------------------------------------------------------- |
|  [01]   | `ExtensionCodecType<C>`   | codec contract  | `tryToEncode(obj, ctx)` + `decode(data, extType, ctx)` interface |
|  [02]   | `ExtensionEncoderType<C>` | encoder fn type | `(input, ctx) => Uint8Array \| ((dataPos: number) => Uint8Array) \| null` |
|  [03]   | `ExtensionDecoderType<C>` | decoder fn type | `(data: Uint8Array, extType: number, ctx) => unknown`        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: synchronous codec
- rail: wire

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]    | [RAIL]                                                                    |
| :-----: | :------------------------------ | :---------------- | :------------------------------------------------------------------------ |
|  [01]   | `encode(value, options?)`       | top-level encoder | `Uint8Array<ArrayBuffer>` slice of an internal buffer                     |
|  [02]   | `decode(buffer, options?)`      | top-level decoder | `unknown` from `ArrayLike<number> \| ArrayBufferView \| ArrayBufferLike`   |
|  [03]   | `decodeMulti(buffer, options?)` | multi decoder     | `Generator<unknown>` of multiple values from one buffer                   |

[ENTRYPOINT_SCOPE]: asynchronous codec
- rail: wire

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :---------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `decodeAsync(streamLike, options?)`       | async decoder  | `Promise<unknown>` from `ReadableStreamLike`  |
|  [02]   | `decodeArrayStream(streamLike, options?)` | async decoder  | `AsyncGenerator<unknown>` of array elements   |
|  [03]   | `decodeMultiStream(streamLike, options?)` | async decoder  | `AsyncGenerator<unknown>` of multiple objects |

[ENTRYPOINT_SCOPE]: stateful class operations
- rail: wire

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]      | [RAIL]                                                |
| :-----: | :---------------------------------- | :------------------ | :---------------------------------------------------- |
|  [01]   | `new Encoder(options?)`             | encoder constructor | stateful encoder with per-instance options            |
|  [02]   | `encoder.encode(object)`            | encode method       | `Uint8Array<ArrayBuffer>` copy of encoded output      |
|  [03]   | `encoder.encodeSharedRef(object)`   | encode method       | `Uint8Array<ArrayBuffer>` reference to internal buffer (fast path) |
|  [04]   | `new Decoder(options?)`             | decoder constructor | stateful decoder with per-instance options            |
|  [05]   | `decoder.decode(buffer)`            | decode method       | `unknown` from buffer                                 |
|  [06]   | `decoder.decodeMulti(buffer)`       | decode method       | `Generator<unknown>` of multiple values               |
|  [07]   | `decoder.decodeAsync(stream)`       | async decode        | `Promise<unknown>` from async stream                  |
|  [08]   | `decoder.decodeArrayStream(stream)` | async decode        | `AsyncGenerator<unknown>` of array elements           |
|  [09]   | `decoder.decodeStream(stream)`      | async decode        | `AsyncGenerator<unknown>` of multiple objects         |

[ENTRYPOINT_SCOPE]: extension codec
- rail: wire

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]    | [RAIL]                                 |
| :-----: | :----------------------------------------- | :---------------- | :------------------------------------- |
|  [01]   | `new ExtensionCodec<C>()`                  | codec constructor | empty typed extension registry         |
|  [02]   | `codec.register({ type, encode, decode })` | registration      | add an extension type handler (`type: number`) |
|  [03]   | `ExtensionCodec.defaultCodec`              | static instance   | default codec with the built-in timestamp extension |

[ENTRYPOINT_SCOPE]: timestamp helpers
- rail: wire

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY]    | [RAIL]                                      |
| :-----: | :------------------------------------ | :---------------- | :------------------------------------------ |
|  [01]   | `EXT_TIMESTAMP`                       | constant          | extension type number for timestamps (`-1`) |
|  [02]   | `encodeDateToTimeSpec(date)`          | timestamp encoder | `Date` to `{ sec, nsec }` time spec         |
|  [03]   | `encodeTimeSpecToTimestamp(timeSpec)` | timestamp encoder | time spec to `Uint8Array` timestamp ext     |
|  [04]   | `decodeTimestampToTimeSpec(data)`     | timestamp decoder | `Uint8Array` to `{ sec, nsec }` time spec   |
|  [05]   | `encodeTimestampExtension`            | ext encoder fn    | encoder function for the timestamp ext      |
|  [06]   | `decodeTimestampExtension`            | ext decoder fn    | decoder function for the timestamp ext      |

## [04]-[IMPLEMENTATION_LAW]

[CODEC_TOPOLOGY]:
- `encode` returns a `Uint8Array<ArrayBuffer>` slice of an internal buffer; its `byteOffset` and `byteLength` must be used when constructing a `Buffer` or slicing further
- `encoder.encodeSharedRef` returns a reference to the internal buffer — do not hold the reference across subsequent `encode` calls
- `decode` input accepts `ArrayLike<number>`, `ArrayBufferView`, or `ArrayBufferLike`; the async variants accept a `ReadableStreamLike` (`AsyncIterable<T> | ReadableStream<T>`); every decode throws `RangeError` on incomplete/empty data and `DecodeError` on invalid data
- `DecoderOptions.useBigInt64` (and its `EncoderOptions` mirror) enables `DataView.getBigInt64`/`getBigUint64`; requires ES2020; defaults `false`
- the five `DecoderOptions.max*Length` bounds each default `UINT32_MAX` — they are the package-level DoS ceiling; a bounded ingress SETS them from its decode budget rather than relying on the effectively-unbounded default
- `EncoderOptions.sortKeys` produces canonical, byte-comparable encodings at a performance cost (disabled by default); `ignoreUndefined` drops `undefined`-valued keys like `JSON.stringify`; `maxDepth` defaults `100`, `initialBufferSize` `2048`
- `ExtensionCodec` threads a `<ContextType>` value through `tryToEncode`/`decode` so a decode carries typed side-context; `ExtensionCodec.defaultCodec` registers the timestamp extension (`EXT_TIMESTAMP = -1`) by default

[STACKS_WITH]:
- `effect` (`.api/effect.md`): the async `Decoder.decodeArrayStream(source)` `AsyncGenerator` lifts into a backpressured `Stream.Stream<OpLogEntryWire, ParseError>` through `Stream.fromAsyncIterable` bounded by the one-frame `Stream.buffer({ capacity: 1, strategy: "suspend" })` window (`Codec/codec.md` `decodeSegmentStream`); each yielded element decodes through `Schema.decodeUnknown(OpLogEntryWire)`, so the raw `unknown` never escapes the codec seam and a `ParseError` lifts onto the `Effect` error channel
- `Ingress/refinement.md` decode-budget seam: the `DecodeBudget` ceilings that gate `Stream.take(budget.maxFrames)` at the Effect layer ALSO drive the `Decoder`'s `maxArrayLength`/`maxMapLength`/`maxBinLength` package bounds, so an oversized single array/map/binary faults inside `decode` before it allocates, not only after the frame count trips at the Stream layer
- `Codec/parity.md` bigint two-half seam (`hash-wasm` LE↔BE partner): `useBigInt64: true` maps MessagePack 64-bit integers to `bigint`, and the fixed-width snapshot header rides a `DataView.getBigUint64(offset, false)` big-endian read — the `littleEndian: false` flag is load-bearing against the C# `BinaryPrimitives.WriteUInt64BigEndian` order, and `HlcTwoHalfParity` asserts the two-64-bit-half composition so a half-swap never folds a fresh HLC op as stale
- CRDT payload seam (`Codec/codec.md` `CrdtOpWire`): the same reused `snapshotDecoder.decode(payload)` reads the C# `[MessagePack.Union]` two-element `[tag, [fields]]` array only when `columnFamily === "crdt"`, normalizing the numeric union tag to the literal `op` discriminant through `CRDT_OP_TAG` before `Schema.decodeUnknown(CrdtOpWire)` — the `ExtensionCodec` registry stays empty (`SnapshotExtensionRows` is `never`), the CRDT union riding the plain array shape, not a registered extension

[LOCAL_ADMISSION]:
- Extension type numbers must be in the range 0–127 for user-defined extensions; negative numbers are reserved for built-in extensions (`EXT_TIMESTAMP = -1`).
- For stateless one-shot encode/decode, use the top-level `encode`/`decode` functions; create one reused `Encoder`/`Decoder` instance when options (`useBigInt64`, the decode bounds) are shared across many calls, never a fresh `Decoder` per segment.
- `decodeAsync` and stream variants accept any `ReadableStreamLike`; the internal implementation handles chunking and buffering, so pass a web `ReadableStream<Uint8Array>` directly.
- Set the `max*Length` bounds from the ingress decode budget; the `UINT32_MAX` default is not a safe untrusted-input ceiling.

[RAIL_LAW]:
- Package: `@msgpack/msgpack`
- Owns: MessagePack binary encoding, decoding, streaming decode, extension type registration, and the timestamp codec
- Accept: any JavaScript value as encode input; `ArrayLike`, `ArrayBufferView`, or `ArrayBufferLike` as decode input; a `ReadableStreamLike` for stream decode; a `<ContextType>` value threaded through the extension codec
- Reject: JSON intermediaries for binary framing; hand-rolled MsgPack format parsing; an unbounded `Decoder` on untrusted ingress where the decode budget should set `max*Length`; a fresh `Decoder` per segment where one reused instance carries the shared options
