# [TS_CORE_API_MSGPACK_MSGPACK]

`@msgpack/msgpack` is the MessagePack codec `interchange/codec` uses for the explicit thirteen-slot `OpLogEntry` envelope and the remaining producer-owned MessagePack families. The seventh position (`Payload`, index 6) stays opaque bytes; a `crdt` row passes those bytes to generated protobuf. One configured-once `Decoder` also carries the standalone 16-byte `Hlc` extension into the kernel `Hlc`, and the `context` thread rides the `value/identity` interner through every ext decode so the mint stays decode-once.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the codec pair, its extension registry, and the context thread
- concern: interchange/codec
- `Decoder`/`Encoder` are configured-once instances; `ExtensionCodec` maps an ext type-byte to a decoder and `ExtData` is a raw undecoded ext. `context` adds a field every ext decoder receives — the kernel-identity thread, typed via internal `ContextOf`/`SplitUndefined`. `DecoderOptions`/`EncoderOptions` are flat `Readonly<Partial<...>>` policy records.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]  | [CONSUMER_BOUNDARY]                                                |
| :-----: | :---------------------------------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `Decoder<C>`                              | decoder        | configured-once decode; sync + async stream methods                |
|  [02]   | `Encoder<C>`                              | encoder        | rare `wire` egress; `encode` copies where `encodeSharedRef` views  |
|  [03]   | `DecoderOptions<C>`                       | policy record  | `useBigInt64`/`extensionCodec`/`context`/`max*Length`/`keyDecoder` |
|  [04]   | `EncoderOptions<C>`                       | policy record  | `sortKeys`/`useBigInt64`/`ignoreUndefined`/`forceIntegerToFloat`   |
|  [05]   | `ExtensionCodec<C>`                       | ext registry   | `.register`/`.tryToEncode`/`.decode`/`defaultCodec`                |
|  [06]   | `ExtData`                                 | raw ext        | `type`/`data`; unregistered ext for `Match` dispatch               |
|  [07]   | `Extension{Decoder,Encoder,Codec}Type<C>` | ext fn types   | `(data, type, context) => value` decode signature                  |
|  [08]   | `context` + `Decoder<C>` generic          | context thread | `value/identity` interner into every ext decode; decode-once       |
|  [09]   | `DecodeError extends Error`               | decode fault   | malformed-frame throw caught at `Effect.try`                       |
|  [10]   | `EXT_TIMESTAMP` (`-1`)                    | built-in ext   | `encode`/`decodeTimestampExtension` for `Date`; auto-registered    |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: single-frame decode, streaming decode, and egress
- concern: interchange/codec
- Sync `decode`/`decodeMulti` cover a buffered frame; async `decodeAsync`/`decodeArrayStream`/`decodeMultiStream` accept a `ReadableStreamLike` and yield an `AsyncGenerator` — the `effect` `Stream` source for the CRDT log. Every entry threads the same `DecoderOptions`.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                              |
| :-----: | :----------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `decode(buffer, options?): unknown`                                | one frame      | complete positional envelope → owned schema      |
|  [02]   | `decodeMulti(buffer, options?): Generator<unknown>`                | sync multi     | concatenated ops in one buffered frame           |
|  [03]   | `decodeMultiStream(streamLike, options?): AsyncGenerator<unknown>` | stream multi   | `OpLog` log → `Stream.fromAsyncIterable`         |
|  [04]   | `decodeArrayStream(streamLike, options?): AsyncGenerator<unknown>` | stream array   | top-level array streamed element-by-element      |
|  [05]   | `decodeAsync(streamLike, options?): Promise<unknown>`              | async one      | one large frame arriving in chunks               |
|  [06]   | `new Decoder({ extensionCodec, context, useBigInt64, ...limits })` | configured     | reused decoder: `Hlc` ext + interner context     |
|  [07]   | `extensionCodec.register({ type, encode, decode })`                | ext row        | contract 16-byte `Hlc` ext → kernel `Hlc`        |
|  [08]   | `new Encoder({ sortKeys:true }).encode(v)` / `.encodeSharedRef(v)` | egress         | canonical re-encode; a view onto the live buffer |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `interchange/codec` registers one `ExtensionCodec` row `{ type, decode }` reading the contract 16-byte `Hlc` extension cell into the kernel `Hlc`; `OpLogEntry` decodes as the producer's explicit thirteen-slot primitive array, and its raw payload is handed to generated `CrdtOpWire` protobuf admission only after the family and content key are verified.
- `context` threads the mint decode-once: `DecoderOptions.context` (`ContextOf<C>`) passes into every `ExtensionDecoderType` call, so the `value/identity` interner and the `Hlc` node-id table ride the decode as state, and the mint happens once inside the ext decoder at the boundary.
- `useBigInt64:true` decodes int64/uint64 tokens as `bigint`; compact positive tokens remain exact `number` values that the owner schema widens to `bigint` before domain use.
- `decodeMultiStream` is backpressured but silently ends when EOF leaves an incomplete value; it is not a strict frame boundary.
- `OpLog` ingress retains length-framed carry, rejects non-empty terminal residue, then supplies complete payloads to the decoder.
- `Encoder.encode` copies out of the encoder's internal buffer while `encodeSharedRef` returns `bytes.subarray(0, pos)` — a view onto that REUSED buffer which the next `encodeSharedRef` on the same instance overwrites, and a re-entrant call clones the encoder rather than corrupting the outer frame. Transferring the view's `ArrayBuffer` detaches the encoder's own storage and poisons every later encode, so `encodeSharedRef` is a same-tick read and never a `Transferable` handoff; `interchange/codec` hands bytes across the codec boundary that outlive the call and therefore takes `encode`.

[STACKING]:
- Stack with `interchange/codec`: one registry row selects MessagePack for the explicit op-log envelope, and generated protobuf owns the seventh position (`Payload`, index 6) only for CRDT; compression stays outside both codecs.
- Stack with `effect` `Stream` (`.api/effect.md`): the strict framing adapter feeds complete payloads into decode, then `Stream.mapEffect` lands each op with bounded concurrency and quarantine interruption.
- Stack with generated protobuf: a `family === "crdt"` envelope row verifies its exact payload content key, then `Format.proto.message(CrdtOpWireSchema)` admits the required oneof; MessagePack never dispatches an op arm.
- Stack with `effect` `Schema` (`.api/effect.md`): `decode` crosses `OpLogEntry` once; compact exact integers and int64/uint64 bigints normalize to the one bounded `bigint` domain.
- Stack with `value/identity`: the `ExtensionCodec` `Hlc` row mints through the interner carried on `context`; `wire` composes the mint and never re-implements the 16-byte layout — a TS re-mint of the `Hlc` cell is the named cross-language drift defect.

[LOCAL_ADMISSION]:
- construct one `new Decoder({ extensionCodec, context, useBigInt64:true, ...max*Length })` per policy and register the standalone `Hlc` ext row once; CRDT operations are not extension values.
- bound untrusted ingress on `DecoderOptions` alone — `maxStrLength`, `maxBinLength`, `maxArrayLength`, `maxMapLength`, `maxExtLength` refuse an oversized frame before allocation, while `maxDepth` is an `EncoderOptions` nesting ceiling and gates nothing on decode.
- thread the `value/identity` interner through `context`; the ext decoder is where the `Hlc` is interned.
- decode output crosses `Schema.decodeUnknown` before a consumer reads it; a raw MessagePack object or an `ExtData` reaching a `core/state` consumer undispatched is the leak defect.
- raw `decodeMultiStream` never owns framed ingress because its successful EOF cannot distinguish a clean boundary from truncation.
- `EXT_TIMESTAMP` (`-1`) stays registered for `Date` fields; domain ext type-bytes ride the contract-allocated positive range, each byte numbered once at the corpus.
