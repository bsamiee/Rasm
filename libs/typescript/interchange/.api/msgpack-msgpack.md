# [API_CATALOGUE] @msgpack/msgpack

`@msgpack/msgpack` supplies synchronous and asynchronous MessagePack encode/decode operations, stateful `Encoder` and `Decoder` classes, extension type registration via `ExtensionCodec`, and timestamp codec helpers for binary framing at the interchange boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@msgpack/msgpack`
- package: `@msgpack/msgpack`
- module: `@msgpack/msgpack` (barrel at `dist.esm/index.d.ts`)
- asset: runtime library
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

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]     | [RAIL]                                                                |
| :-----: | :------------------ | :---------------- | :-------------------------------------------------------------------- |
|  [01]   | `EncoderOptions<C>` | options interface | depth, buffer size, sort keys, float/int flags, extension codec       |
|  [02]   | `DecoderOptions<C>` | options interface | length limits, bigint mode, raw strings, key decoder, extension codec |

[PUBLIC_TYPE_SCOPE]: extension codec types
- rail: wire

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [RAIL]                                     |
| :-----: | :------------------------ | :-------------- | :----------------------------------------- |
|  [01]   | `ExtensionCodecType<C>`   | codec contract  | `tryToEncode` + `decode` interface         |
|  [02]   | `ExtensionEncoderType<C>` | encoder fn type | `(input, ctx) => Uint8Array \| fn \| null` |
|  [03]   | `ExtensionDecoderType<C>` | decoder fn type | `(data, extType, ctx) => unknown`          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: synchronous codec
- rail: wire

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]    | [RAIL]                                                           |
| :-----: | :------------------------------ | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `encode(value, options?)`       | top-level encoder | `Uint8Array` slice of internal buffer                            |
|  [02]   | `decode(buffer, options?)`      | top-level decoder | `unknown` from `ArrayLike \| ArrayBufferView \| ArrayBufferLike` |
|  [03]   | `decodeMulti(buffer, options?)` | multi decoder     | `Generator<unknown>` of multiple values                          |

[ENTRYPOINT_SCOPE]: asynchronous codec
- rail: wire

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :---------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `decodeAsync(streamLike, options?)`       | async decoder  | `Promise<unknown>` from readable stream       |
|  [02]   | `decodeArrayStream(streamLike, options?)` | async decoder  | `AsyncGenerator<unknown>` of array elements   |
|  [03]   | `decodeMultiStream(streamLike, options?)` | async decoder  | `AsyncGenerator<unknown>` of multiple objects |

[ENTRYPOINT_SCOPE]: stateful class operations
- rail: wire

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]      | [RAIL]                                                |
| :-----: | :---------------------------------- | :------------------ | :---------------------------------------------------- |
|  [01]   | `new Encoder(options?)`             | encoder constructor | stateful encoder with per-instance options            |
|  [02]   | `encoder.encode(object)`            | encode method       | `Uint8Array` copy of encoded output                   |
|  [03]   | `encoder.encodeSharedRef(object)`   | encode method       | `Uint8Array` reference to internal buffer (fast path) |
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
|  [01]   | `new ExtensionCodec()`                     | codec constructor | empty typed extension registry         |
|  [02]   | `codec.register({ type, encode, decode })` | registration      | add extension type handler             |
|  [03]   | `ExtensionCodec.defaultCodec`              | static instance   | default codec with built-in extensions |

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
- `encode` returns a `Uint8Array` slice of an internal buffer; its `byteOffset` and `byteLength` must be used when constructing a `Buffer` or slicing further
- `encoder.encodeSharedRef` returns a reference to the internal buffer — do not hold the reference across subsequent `encode` calls
- `decode` input accepts `ArrayLike<number>`, `ArrayBufferView`, or `ArrayBufferLike`; throws `RangeError` on incomplete data, `DecodeError` on invalid data
- `DecoderOptions.useBigInt64` enables `DataView.getBigInt64`/`getBigUint64`; requires ES2020; defaults `false`
- `EncoderOptions.sortKeys` produces canonical encodings at a performance cost; disabled by default
- `ExtensionCodec.defaultCodec` registers the timestamp extension (`EXT_TIMESTAMP = -1`) by default

[LOCAL_ADMISSION]:
- Extension type numbers must be in the range 0–127 for user-defined extensions; negative numbers are reserved for built-in extensions.
- For stateless one-shot encode/decode, use the top-level `encode`/`decode` functions; create `Encoder`/`Decoder` instances only when options are shared across many calls.
- `decodeAsync` and stream variants accept any `ReadableStreamLike`; the internal implementation handles chunking and buffering.

[RAIL_LAW]:
- Package: `@msgpack/msgpack`
- Owns: MessagePack binary encoding, decoding, extension type registration, timestamp codec
- Accept: any JavaScript value as encode input; `ArrayLike`, `ArrayBufferView`, or `ArrayBufferLike` as decode input
- Reject: JSON intermediaries for binary framing; hand-rolled MsgPack format parsing
