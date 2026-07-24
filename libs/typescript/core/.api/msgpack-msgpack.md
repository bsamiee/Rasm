# [TS_CORE_API_MSGPACK_MSGPACK]

`@msgpack/msgpack` is the MessagePack codec `interchange/codec` decodes the C#-minted `CrdtOpWire` union and streams the `OpLog` through. A configured-once `Decoder` carries the load-bearing surface: one `ExtensionCodec` row decodes the 16-byte `Hlc` extension cell into the kernel `Hlc`, and the `context` thread rides the `value/identity` interner through every ext decode so the mint stays decode-once.

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
|  [02]   | `Encoder<C>`                              | encoder        | rare `wire` egress; `encode`/`encodeSharedRef` zero-copy view      |
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
|  [07]   | `extensionCodec.register({ type, encode, decode })`                | ext row        | C#-minted 16-byte `Hlc` ext → kernel `Hlc`       |
|  [08]   | `new Encoder({ sortKeys:true }).encode(v)` / `.encodeSharedRef(v)` | egress         | canonical re-encode; zero-copy `Worker` transfer |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `interchange/codec` registers one `ExtensionCodec` row `{ type, decode }` reading the C#-minted 16-byte `Hlc` extension cell into the kernel `Hlc`, the ext type-byte carrying the union discriminant; the `CrdtOpWire` op union decodes as a tagged array/map whose discriminant selects the op arm, so the ext registry and the tag dispatch stay two tables, never a branch ladder.
- `context` threads the mint decode-once: `DecoderOptions.context` (`ContextOf<C>`) passes into every `ExtensionDecoderType` call, so the `value/identity` interner and the `Hlc` node-id table ride the decode as state, and the mint happens once inside the ext decoder at the seam.
- `useBigInt64:true` decodes MessagePack int64/uint64 as `bigint` — the HLC physical-time counter, ordinals, and version-vector entries; a decoder without it truncates past 2^53, the named precision defect.
- `decodeMultiStream` is an `AsyncGenerator` over a `ReadableStreamLike` so `OpLog` frames arrive backpressured and the codec stays a pure function; the Effect `Stream` owns concurrency and the poison-frame halt.

[STACKING]:
- Stack with the codec siblings (`@bufbuild/protobuf`/`cbor-x`/`rfc6902`, `core/.api/`): the interchange plane is multi-codec and `interchange/codec` is the MessagePack arm — this owns the `CrdtOpWire`/`OpLog` union, the 16-byte `Hlc` cell riding a MessagePack ext type here, never a proto field. A `codec` page picks ONE codec by the C# mint format; each sibling's format ownership binds at its own `RAIL_LAW`.
- Stack with `effect` `Stream` (`.api/effect.md`): `Stream.fromAsyncIterable(decodeMultiStream(bytes, opts), onError)` is the CRDT log source; `Stream.mapEffect` decodes each op with bounded concurrency and `Stream.haltWhen` ends on a quarantine signal — the journal decode is one backpressured pipeline into `data/journal/append`.
- Stack with `effect` `Data`/`Match` (`.api/effect.md`): the decoded `CrdtOpWire` discriminant dispatches through `Data.taggedEnum().$match`/`Match.exhaustive` into the `state/crdt` op family (a missing arm is a compile error); an unregistered ext surfaces as `ExtData` and is `Match`-dispatched, never dropped.
- Stack with `effect` `Schema` (`.api/effect.md`): `decode` output crosses `Schema.decodeUnknown(CrdtOpSchema)` once; `useBigInt64:true` feeds the `Schema.BigIntFromSelf`/branded HLC fields, so the interior sees a branded `Hlc`/`bigint`, never a raw MessagePack value.
- Stack with `value/identity`: the `ExtensionCodec` `Hlc` row mints through the interner carried on `context`; `wire` composes the mint and never re-implements the 16-byte layout — a TS re-mint of the `Hlc` cell is the named cross-language drift defect.
- Stack with `@effect/platform` `Worker` (`.api/effect-platform.md`): `Encoder.encodeSharedRef` returns a view over the encoder's internal `ArrayBuffer` for a zero-copy `Transferable` into the decode worker; `interchange/codec` bounds untrusted frames with the `max*Length` ceilings before decode.

[LOCAL_ADMISSION]:
- construct one `new Decoder({ extensionCodec, context, useBigInt64:true, ...max*Length })` per policy and register the `Hlc` and domain CRDT ext rows once; the top-level `decode` cannot see the 16-byte `Hlc` ext without the shared `ExtensionCodec`.
- thread the `value/identity` interner through `context`; the ext decoder is where the `Hlc` is interned.
- decode output crosses `Schema.decodeUnknown` before a consumer reads it; a raw MessagePack object or an `ExtData` reaching `state/crdt` undispatched is the leak defect.
- `EXT_TIMESTAMP` (`-1`) stays registered for `Date` fields; domain ext type-bytes are the C#-owned positive range, numbered once on the C# side.

[RAIL_LAW]:
- Package: `@msgpack/msgpack`
- Owns: MessagePack decode of the `CrdtOpWire` union and the `OpLog` log (`decode`/`decodeMulti`, `decodeMultiStream`/`decodeArrayStream`/`decodeAsync`, `Decoder`/`Encoder`), the `ExtensionCodec`/`ExtData` registry carrying the 16-byte `Hlc` cell, the `context` interner thread, `useBigInt64` i64 fidelity, the `max*Length` DoS ceilings, and `sortKeys`/`encodeSharedRef` canonical/zero-copy egress
- Accept: a configured-once `Decoder` with the `Hlc` `ExtensionCodec` row and the `value/identity` `context`, `useBigInt64:true`, `decodeMultiStream` folded into an `effect` `Stream`, decode output crossing `Schema.decodeUnknown` into a `Data.taggedEnum` op family, `max*Length` before untrusted decode
- Reject: top-level `decode` without the shared `ExtensionCodec`, a TS re-mint of the `Hlc` cell or a module-singleton interner, `useBigInt64:false` on HLC frames, a raw MessagePack value or undispatched `ExtData` in domain code, an event-emitter frame loop where an Effect `Stream` owns the walk
