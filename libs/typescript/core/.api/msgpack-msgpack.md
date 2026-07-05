# [@msgpack/msgpack] — MessagePack union decode for the CrdtOpWire + OpLog CRDT rails

`@msgpack/msgpack` is the MessagePack codec `interchange/codec` decodes the C#-minted `CrdtOpWire` union with and `interchange/codec` streams the `OpLog` through. The two load-bearing capabilities are the `ExtensionCodec` and the `context` thread: the 16-byte `Hlc` cell rides a C#-minted MessagePack extension type, so `interchange/codec` registers one `ExtensionCodec` row (`register({ type, decode })`) that decodes the fixed blob INTO the kernel `Hlc`, and the `context` (`ContextOf`/`SplitUndefined`) parameter threads the `value/identity` interner through the decode so the mint stays decode-once. The reusable `Decoder`/`Encoder` classes carry the full option surface — `useBigInt64:true` (the i64 HLC counter and ordinals decode as `bigint`, not lossy `number`), the `maxStrLength`/`maxBinLength`/`maxArrayLength`/`maxMapLength`/`maxExtLength` untrusted-frame ceilings feeding `interchange/codec`, the `keyDecoder`/`mapKeyConverter` options for hot repeated op keys (the default `keyDecoder` is a shared cached key decoder), and `Encoder.sortKeys` canonical encode / `encodeSharedRef` zero-copy egress. The streaming mirror is the CRDT log rail: `decodeMultiStream`/`decodeArrayStream` are `AsyncGenerator`s over a `ReadableStreamLike` that fold straight into an `effect` `Stream`, so `OpLog` frames arrive backpressured, decode into a `Data.taggedEnum` op family, and flow to `store/journal` and `state/crdt` without a monolithic buffer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@msgpack/msgpack`
- package: `@msgpack/msgpack`
- version: `3.1.3`
- license: `ISC`
- effect-peer: none — decode output crosses `effect` `Schema.decodeUnknown` and `Stream.fromAsyncIterable` at the `interchange/codec`/`interchange/codec` seams (`.api/effect.md`)
- catalog-verdict: KEEP — the MessagePack arm of the multi-codec wire plane: `@bufbuild/protobuf` owns the descriptor-typed proto families, `cbor-x` canonical CBOR, `rfc6902` the JSON-Patch egress, this owns the `CrdtOpWire`/`OpLog` MessagePack union — one codec per C# mint format, no overlap
- runtime: isomorphic, `sideEffects:false`, `engines.node >= 18` (`DataView.getBigInt64` for `useBigInt64`); dual `dist.esm/index.mjs` + `dist.cjs/index.cjs`; zero runtime deps
- modules: `decode`/`decodeMulti`, `decodeAsync`/`decodeArrayStream`/`decodeMultiStream`, `Decoder`/`DecoderOptions`, `Encoder`/`EncoderOptions`, `ExtensionCodec`/`ExtData`, `DecodeError`, `timestamp` (`EXT_TIMESTAMP`), `context` (`ContextOf`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the codec, its extension registry, and the context thread
- rail: interchange/codec
- `Decoder`/`Encoder` are the configured-once instances. `ExtensionCodec` maps a MessagePack ext type-byte to a decoder; `ExtData` is a raw undecoded ext (type + bytes). The `context` option adds a read/write field every extension decoder receives (typed via the internal `ContextOf`/`SplitUndefined`, not a barrel import) — the kernel-identity thread. `DecoderOptions`/`EncoderOptions` are flat `Readonly<Partial<...>>` policy records.

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY]   | [CONSUMER / BOUNDARY]                                            |
| :-----: | :---------------------------------------------------------------- | :-------------- | :-------------------------------------------------------------- |
|  [01]   | `Decoder<ContextType>`                                             | decoder         | `interchange/codec`/`interchange/codec` configured-once decode; sync + async methods |
|  [02]   | `Encoder<ContextType>` (`.encode`, `.encodeSharedRef`)            | encoder         | rare `wire`-owned egress; `encodeSharedRef` zero-copy into a worker transfer |
|  [03]   | `DecoderOptions<C>`                                                | policy record   | `useBigInt64`/`extensionCodec`/`context`/`max*Length`/`keyDecoder`/`mapKeyConverter` |
|  [04]   | `EncoderOptions<C>`                                                | policy record   | `sortKeys` (canonical)/`useBigInt64`/`ignoreUndefined`/`forceIntegerToFloat` |
|  [05]   | `ExtensionCodec<C>` (`.register`, `.tryToEncode`, `.decode`, `defaultCodec`) | ext registry | the 16-byte `Hlc` ext row + domain CRDT ext rows; one codec per decoder |
|  [06]   | `ExtData` (`type`, `data`)                                         | raw ext         | an unregistered ext type surfaced verbatim for `Match` dispatch |
|  [07]   | `ExtensionDecoderType<C>` / `ExtensionEncoderType<C>` / `ExtensionCodecType<C>` | ext fn types | the `(data, type, context) => value` decode signature the `Hlc` row implements |
|  [08]   | `context` option + `Decoder<ContextType>` generic                 | context thread  | the `value/identity` interner passed into every ext decode; decode-once mint (internally `ContextOf`/`SplitUndefined`) |
|  [09]   | `DecodeError extends Error`                                        | decode fault    | malformed-frame throw caught at `Effect.try` → `interchange/codec` |
|  [10]   | `EXT_TIMESTAMP` (`-1`) + `encodeTimestampExtension`/`decodeTimestampExtension` | built-in ext | reserved ext `-1` for `Date` fields; auto-registered in `ExtensionCodec.defaultCodec`, retained beside the domain rows |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: single-frame decode, streaming decode, and egress
- rail: interchange/codec
- The sync `decode`/`decodeMulti` cover a buffered frame; the async `decodeAsync`/`decodeArrayStream`/`decodeMultiStream` accept a `ReadableStreamLike` (a whatwg `ReadableStream` or an `AsyncIterable`) and yield an `AsyncGenerator` — the `effect` `Stream` source for the CRDT log. Every entry threads the same `DecoderOptions` (extension codec, context, limits).

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `decode(buffer, options?): unknown`                                                                 | one frame      | `interchange/codec` single `CrdtOpWire` → `Schema.decodeUnknown` |
|  [02]   | `decodeMulti(buffer, options?): Generator<unknown>`                                                 | sync multi     | concatenated ops in one buffered frame                   |
|  [03]   | `decodeMultiStream(streamLike, options?): AsyncGenerator<unknown>`                                  | stream multi   | `interchange/codec` `OpLog` log → `Stream.fromAsyncIterable` → `store/journal` |
|  [04]   | `decodeArrayStream(streamLike, options?): AsyncGenerator<unknown>`                                  | stream array   | a top-level MessagePack array streamed element-by-element |
|  [05]   | `decodeAsync(streamLike, options?): Promise<unknown>`                                               | async one      | one large frame arriving in chunks                       |
|  [06]   | `new Decoder({ extensionCodec, context, useBigInt64, ...limits })`                                  | configured     | the reused decoder carrying the `Hlc` ext + interner context |
|  [07]   | `extensionCodec.register({ type, encode, decode })`                                                 | ext row        | the C#-minted 16-byte `Hlc` ext type → kernel `Hlc`      |
|  [08]   | `new Encoder({ sortKeys:true }).encode(v)` / `.encodeSharedRef(v)`                                  | egress         | canonical re-encode; `encodeSharedRef` for a zero-copy `Worker` transfer |

## [04]-[IMPLEMENTATION_LAW]

[MSGPACK_TOPOLOGY]:
- the ext type is the union discriminant carrier: the 16-byte `Hlc` cell is a MessagePack extension type-byte the C# writer mints; `interchange/codec` registers one `ExtensionCodec` row `{ type, decode }` that reads the fixed 16 bytes into the kernel `Hlc`. The `CrdtOpWire` op union itself decodes as a tagged array/map whose discriminant selects the op arm; the ext registry and the tag dispatch are two tables, never a branch ladder.
- `context` threads the mint, decode-once: `DecoderOptions.context` (`ContextOf<C>`) is passed into every `ExtensionDecoderType` call, so the `value/identity` interner and the `Hlc` node-id table ride the decode as state instead of a module singleton — the mint happens once, inside the ext decoder, at the seam.
- i64 fidelity is a decode option, not a cast: `useBigInt64:true` decodes MessagePack int64/uint64 as `bigint` (the HLC physical-time counter, ordinals, version-vector entries); a decoder without it silently truncates past 2^53 and is the named precision defect.
- streaming is the log rail: `decodeMultiStream` is an `AsyncGenerator` over a `ReadableStreamLike`; the `OpLog` frames arrive backpressured and the codec stays a pure function — the Effect `Stream` owns concurrency and the poison-frame halt, never an event emitter.

[INTEGRATION_LAW]:
- Stack with the codec siblings (`@bufbuild/protobuf` / `cbor-x` / `rfc6902`, `core/.api/`): the interchange plane is multi-codec and `interchange/codec`/`interchange/codec` are the MessagePack arm — `@bufbuild/protobuf` owns the descriptor-typed proto families (`ElementGraph`, `FaultDetail`, `GeometryPayload`, the RPC suite) and never mints a `CrdtOpWire`, `cbor-x` owns the canonical `SnapshotHeader` CBOR, `rfc6902` owns the `JsonPatchDocument` egress. A `codec` page picks ONE codec by the C# mint format; the 16-byte `Hlc` cell rides a MessagePack ext type here, never a proto field — proto's descriptor reflection and drift gate are `@bufbuild/protobuf`'s altitude, not this one's.
- Stack with `effect` `Stream` (`.api/effect.md`): `Stream.fromAsyncIterable(decodeMultiStream(bytes, opts), onError)` is the `interchange/codec` CRDT log source; `Stream.mapEffect` decodes each op into the owned shape with bounded concurrency and `Stream.haltWhen` ends on a quarantine signal — the journal decode is one backpressured pipeline into `data/journal/append`.
- Stack with `effect` `Data`/`Match` (`.api/effect.md`): the decoded `CrdtOpWire` discriminant dispatches through `Data.taggedEnum().$match`/`Match.exhaustive` into the `state/crdt` op family (a missing arm is a compile error); an unregistered ext surfaces as `ExtData` and is `Match`-dispatched, never dropped.
- Stack with `effect` `Schema` (`.api/effect.md`): `decode` output crosses `Schema.decodeUnknown(CrdtOpSchema)` once; `useBigInt64:true` feeds the `Schema.BigIntFromSelf`/branded HLC fields, so the interior sees a branded `Hlc`/`bigint`, never a raw MessagePack value.
- Stack with `value/identity`: the `ExtensionCodec` `Hlc` row mints through the interner carried on `context`; `wire` composes the mint and never re-implements the 16-byte layout — a TS re-mint of the `Hlc` cell is the named cross-language drift defect.
- Stack with `@effect/platform` `Worker` (`.api/effect-platform.md`): `Encoder.encodeSharedRef` returns a view over the encoder's internal `ArrayBuffer` for a zero-copy `Transferable` into the `runtime browser/fetch` decode worker; `interchange/codec` bounds untrusted frames with the `max*Length` ceilings before decode.

[LOCAL_ADMISSION]:
- construct one `new Decoder({ extensionCodec, context, useBigInt64:true, ...max*Length })` per policy; register the `Hlc` and domain CRDT ext rows once — never call the top-level `decode` without the shared `ExtensionCodec`, which cannot see the 16-byte `Hlc` ext.
- thread the `value/identity` interner through `context`, never a module-level mint singleton; the ext decoder is where the `Hlc` is interned.
- decode output crosses `Schema.decodeUnknown` before a consumer reads it; a raw MessagePack object or an `ExtData` reaching `state/crdt` undispatched is the leak defect.
- the built-in timestamp ext (`EXT_TIMESTAMP` `-1`) stays registered for `Date` fields; domain ext type-bytes are the C#-owned positive range and are never re-numbered on the TS side.

[RAIL_LAW]:
- Package: `@msgpack/msgpack`
- Owns: MessagePack decode of the `CrdtOpWire` union and the `OpLog` log (`decode`/`decodeMulti`, `decodeMultiStream`/`decodeArrayStream`/`decodeAsync`, `Decoder`/`Encoder`), the `ExtensionCodec`/`ExtData` ext registry carrying the 16-byte `Hlc` cell, the `context` interner thread, `useBigInt64` i64 fidelity, the `max*Length` DoS ceilings, and `sortKeys`/`encodeSharedRef` canonical/zero-copy egress
- Accept: a configured-once `Decoder` with the `Hlc` `ExtensionCodec` row and the `value/identity` `context`, `useBigInt64:true`, `decodeMultiStream` folded into an `effect` `Stream`, decode output crossing `Schema.decodeUnknown` into a `Data.taggedEnum` op family, `max*Length` before untrusted decode
- Reject: top-level `decode` without the shared `ExtensionCodec`, a TS re-mint of the `Hlc` cell or a module-singleton interner, dropping i64 fidelity (`useBigInt64:false` on HLC frames), a raw MessagePack value or undispatched `ExtData` in domain code, an event-emitter frame loop where an Effect `Stream` owns the walk
