# [TS_CORE_API_MSGPACK_MSGPACK]

`@msgpack/msgpack` is the MessagePack codec `interchange/codec` decodes the contract `CrdtOpWire` union and streams the `OpLog` through. One configured-once `Decoder` carries the load-bearing surface: one `ExtensionCodec` row decodes the 16-byte `Hlc` extension cell into the kernel `Hlc`, and the `context` thread rides the `value/identity` interner through every ext decode so the mint stays decode-once.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@msgpack/msgpack`
- package: `@msgpack/msgpack` (ISC)
- rail: MessagePack decode of the `CrdtOpWire` union and the `OpLog` stream — the MessagePack arm of the multi-codec interchange plane, one codec selected per C# mint format
- runtime: isomorphic, `sideEffects:false`, zero runtime deps; dual `dist.esm/index.mjs` ESM + `dist.cjs/index.cjs`
- effect-peer: none — decode output crosses `effect` `Schema.decodeUnknown` and `Stream.fromAsyncIterable` at the `interchange/codec` seam (`.api/effect.md`)
- modules: `decode`/`decodeMulti`, `decodeAsync`/`decodeArrayStream`/`decodeMultiStream`, `Decoder`/`DecoderOptions`, `Encoder`/`EncoderOptions`, `ExtensionCodec`/`ExtData`, `DecodeError`, `timestamp` (`EXT_TIMESTAMP`), `context` (`ContextOf`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the codec pair, its extension registry, and the context thread
- rail: interchange/codec
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

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: single-frame decode, streaming decode, and egress
- rail: interchange/codec
- Sync `decode`/`decodeMulti` cover a buffered frame; async `decodeAsync`/`decodeArrayStream`/`decodeMultiStream` accept a `ReadableStreamLike` and yield an `AsyncGenerator` — the `effect` `Stream` source for the CRDT log. Every entry threads the same `DecoderOptions`.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                              |
| :-----: | :----------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `decode(buffer, options?): unknown`                                | one frame      | single `CrdtOpWire` → `Schema.decodeUnknown`     |
|  [02]   | `decodeMulti(buffer, options?): Generator<unknown>`                | sync multi     | concatenated ops in one buffered frame           |
|  [03]   | `decodeMultiStream(streamLike, options?): AsyncGenerator<unknown>` | stream multi   | `OpLog` log → `Stream.fromAsyncIterable`         |
|  [04]   | `decodeArrayStream(streamLike, options?): AsyncGenerator<unknown>` | stream array   | top-level array streamed element-by-element      |
|  [05]   | `decodeAsync(streamLike, options?): Promise<unknown>`              | async one      | one large frame arriving in chunks               |
|  [06]   | `new Decoder({ extensionCodec, context, useBigInt64, ...limits })` | configured     | reused decoder: `Hlc` ext + interner context     |
|  [07]   | `extensionCodec.register({ type, encode, decode })`                | ext row        | contract 16-byte `Hlc` ext → kernel `Hlc`        |
|  [08]   | `new Encoder({ sortKeys:true }).encode(v)` / `.encodeSharedRef(v)` | egress         | canonical re-encode; a view onto the live buffer |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `interchange/codec` registers one `ExtensionCodec` row `{ type, decode }` reading the contract 16-byte `Hlc` extension cell into the kernel `Hlc`; the `CrdtOpWire` op union decodes as a FLAT array whose slot-0 integer tag selects the op arm and whose remaining slots are the producer's keyed roster, so the ext registry and the tag dispatch stay two tables, never a branch ladder.
- `context` threads the mint decode-once: `DecoderOptions.context` (`ContextOf<C>`) passes into every `ExtensionDecoderType` call, so the `value/identity` interner and the `Hlc` node-id table ride the decode as state, and the mint happens once inside the ext decoder at the seam.
- `useBigInt64:true` decodes MessagePack int64/uint64 as `bigint` — the HLC physical-time counter, ordinals, and version-vector entries; a decoder without it truncates past 2^53, the named precision defect.
- `decodeMultiStream` is backpressured but silently ends when EOF leaves an incomplete value; it is not a strict frame boundary.
- `OpLog` ingress retains length-framed carry, rejects non-empty terminal residue, then supplies complete payloads to the decoder.
- `Encoder.encode` copies out of the encoder's internal buffer while `encodeSharedRef` returns `bytes.subarray(0, pos)` — a view onto that REUSED buffer which the next `encodeSharedRef` on the same instance overwrites, and a re-entrant call clones the encoder rather than corrupting the outer frame. Transferring the view's `ArrayBuffer` detaches the encoder's own storage and poisons every later encode, so `encodeSharedRef` is a same-tick read and never a `Transferable` handoff; `interchange/codec` hands bytes across the codec seam that outlive the call and therefore takes `encode`.

[STACKING]:
- Stack with `interchange/codec`: one registry row selects MessagePack for the op-log family and carries the HLC extension through this codec.
- Stack with `effect` `Stream` (`.api/effect.md`): the strict framing adapter feeds complete payloads into decode, then `Stream.mapEffect` lands each op with bounded concurrency and quarantine interruption.
- Stack with `effect` `Data`/`Match` (`.api/effect.md`): the decoded `CrdtOpWire` discriminant dispatches through `Data.taggedEnum().$match`/`Match.exhaustive` into `interchange/codec`'s `CrdtOp` family (a missing arm is a compile error); the union is closed at the producer's arms, so an unrostered tag refuses at decode rather than dispatching.
- Stack with `effect` `Schema` (`.api/effect.md`): `decode` output crosses `Schema.decodeUnknown(CrdtOpSchema)` once; `useBigInt64:true` feeds the `Schema.BigIntFromSelf`/branded HLC fields, so the interior sees a branded `Hlc`/`bigint`, never a raw MessagePack value.
- Stack with `value/identity`: the `ExtensionCodec` `Hlc` row mints through the interner carried on `context`; `wire` composes the mint and never re-implements the 16-byte layout — a TS re-mint of the `Hlc` cell is the named cross-language drift defect.

[LOCAL_ADMISSION]:
- construct one `new Decoder({ extensionCodec, context, useBigInt64:true, ...max*Length })` per policy and register the `Hlc` and domain CRDT ext rows once; the top-level `decode` cannot see the 16-byte `Hlc` ext without the shared `ExtensionCodec`.
- bound untrusted ingress on `DecoderOptions` alone — `maxStrLength`, `maxBinLength`, `maxArrayLength`, `maxMapLength`, `maxExtLength` refuse an oversized frame before allocation, while `maxDepth` is an `EncoderOptions` nesting ceiling and gates nothing on decode.
- thread the `value/identity` interner through `context`; the ext decoder is where the `Hlc` is interned.
- decode output crosses `Schema.decodeUnknown` before a consumer reads it; a raw MessagePack object or an `ExtData` reaching a `core/state` consumer undispatched is the leak defect.
- raw `decodeMultiStream` never owns framed ingress because its successful EOF cannot distinguish a clean boundary from truncation.
- `EXT_TIMESTAMP` (`-1`) stays registered for `Date` fields; domain ext type-bytes ride the contract-allocated positive range, each byte numbered once at the corpus.

[RAIL_LAW]:
- Package: `@msgpack/msgpack`
- Owns: MessagePack decode of the `CrdtOpWire` union and the `OpLog` log (`decode`/`decodeMulti`, `decodeMultiStream`/`decodeArrayStream`/`decodeAsync`, `Decoder`/`Encoder`), the `ExtensionCodec`/`ExtData` registry carrying the 16-byte `Hlc` cell, the `context` interner thread, `useBigInt64` i64 fidelity, the `DecoderOptions` `max*Length` DoS ceilings, and `sortKeys` canonical egress beside the `encodeSharedRef` reused-buffer view
- Accept: a configured decoder with the HLC extension and interner context, bigint fidelity, strict external framing, schema landing, pre-decode length limits, and `encode` wherever the bytes outlive the call
- Reject: top-level decode without the shared extension, a second HLC mint, module-global interning, lossy int64 decode, raw `decodeMultiStream` on framed ingress, raw decoded values in domain code, an `encodeSharedRef` view retained past the next encode or transferred as a `Transferable`, and emitter loops where Effect owns streaming
