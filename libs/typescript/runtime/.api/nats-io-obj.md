# [TS_RUNTIME_API_NATS_IO_OBJ]

`@nats-io/obj` chunks large binaries across a JetStream stream over the one core connection the fanout engine holds — a distinct package, not a jetstream sub-export, standing as the ObjectStore topology row of `net/pubsub` for transient blob passing; per-chunk sha-256 digests are wire integrity, never content identity, so `ContentKey` stays the sole addressing vocabulary and durable objects live on the object plane, never in a NATS store.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@nats-io/obj`
- package: `@nats-io/obj` (Apache-2.0)
- module format: ESM + CJS dual
- runtime target: isomorphic — node, bun, browser over websockets
- rail: ObjectStore topology row (`net/pubsub`); transient blob passing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: store admin, the object surface, and the read/receipt shapes
- rail: boundaries

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]  | [CONSUMER]                                                                                |
| :-----: | :-------------- | :------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `Objm`          | store admin    | store ensure at engine Layer build over the core connection                               |
|  [02]   | `ObjectStore`   | object surface | one surface; stream-vs-blob is a member selection on payload size                         |
|  [03]   | `ObjectInfo`    | receipt        | the object fact — name, size, chunk, digest; `meta.options.max_chunk_size` rules chunking |
|  [04]   | `ObjectResult`  | read result    | `data: ReadableStream<Uint8Array>`, `error: Promise<Error \| null>` — both are consumed   |
|  [05]   | `PurgeResponse` | receipt        | from `delete`; removal evidence                                                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: store lifecycle, chunked transfer, and aliasing
- rail: boundaries

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :--------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `new Objm(nc)` → `create`/`open`/`list`              | mint           | store ensure per topology row                                 |
|  [02]   | `os.put(meta, rs)`                                   | chunked write  | → `Promise<ObjectInfo>`; chunked, digested per chunk          |
|  [03]   | `os.putBlob(meta, data)`                             | blob write     | small bounded payload; whole body materializes                |
|  [04]   | `os.getBlob(name)`                                   | blob read      | → `Promise<Uint8Array \| null>`; small bounded payload        |
|  [05]   | `os.get(name)`                                       | chunked read   | → `Promise<ObjectResult \| null>`; observe `.data` + `.error` |
|  [06]   | `os.info(name)` / `os.list()` / `os.watch(opts?)`    | census + tail  | object facts, store census, change tail                       |
|  [07]   | `os.link(name, info)` / `os.linkStore(name, bucket)` | alias          | byte-free aliasing of one object or a whole store             |
|  [08]   | `os.delete(name)`                                    | remove         | → `Promise<PurgeResponse>`; removal evidence                  |
|  [09]   | `os.seal()` / `os.status(opts?)` / `os.destroy()`    | admin          | read-only freeze, introspection, teardown                     |

## [04]-[IMPLEMENTATION_LAW]

[STACKING]:
- `@nats-io/jetstream` (`.api/nats-io-jetstream.md`): the store IS a stream of chunk messages; `watch` rides a nameless ordered consumer minted with `AckPolicy.None` that cannot ack — a replay/tail surface by construction, so at-least-once processing of object-change events rides a durable `AckPolicy.Explicit` consumer on the backing stream, never an ack against the watch iterator.
- `@nats-io/nats-core` (`.api/nats-io-nats-core.md`): the one scoped `wsconnect` connection fans into fanout, KV, and object rows; a store never dials.
- `effect` (`.api/effect.md`): promise members convert through `Effect.tryPromise`; a `put` body mints its `ReadableStream` from an Effect `Stream` via `Stream.toReadableStream`, and `get` lifts back through `Stream.fromReadableStream` with `.error` joined into the failure channel so a mid-stream chunk fault surfaces as a typed failure, never a silent truncation.
- `data` `object/store`: the content-addressed S3 plane owns durable objects; this row passes transient blobs, and promotion to durability writes through the object plane, never a `linkStore` masquerade.

[LOCAL_ADMISSION]:
- `put`/`get` over WHATWG streams is the default transfer; `putBlob`/`getBlob` admit only for small bounded payloads.
- Observe `ObjectResult.error` on every `get` — a consumed `.data` with an unobserved `.error` is the named silent-truncation defect.
- Alias with `link`/`linkStore`; a byte copy where a link suffices rejects.
- Ensure stores at engine Layer build from topology rows; `seal` is the terminal state of a store whose write phase is ruled complete.

[RAIL_LAW]:
- Package: `@nats-io/obj`
- Owns: store administration, chunked stream put/get with per-chunk digests, blob convenience members, object census and change tail, byte-free links, seal/status/destroy lifecycle
- Accept: Layer-build store ensure, `Stream.toReadableStream`/`Stream.fromReadableStream` at the body seam, `.error` joined to the typed failure channel, links over copies, `putBlob`/`getBlob` for bounded payloads only
- Reject: the store as system of record or second content-addressing vocabulary, unobserved `.error` channels, per-call dials, whole-body materialization where a stream flows, ack calls against ordered watch iterators
