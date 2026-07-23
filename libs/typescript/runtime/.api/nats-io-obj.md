# [TS_RUNTIME_API_NATS_IO_OBJ]

`@nats-io/obj` materializes chunked large-binary storage over a JetStream stream on the same core connection the fanout engine holds: `Objm` mints and opens stores, `put(meta, readableStream)` chunks the body by `meta.options.max_chunk_size` and digests it via `js-sha256`, `get` returns an `ObjectResult` whose `.data` is a `ReadableStream<Uint8Array>` and whose `.error` is a deferred fault Promise that must be observed, `link`/`linkStore` alias objects and whole stores without copying bytes, and `seal` freezes a store read-only. It is a SEPARATE package, not a jetstream sub-export — the ObjectStore topology row of `net/pubsub`, a transient blob-passing engine on the pubsub plane. It is a DISTINCT engine from the content-addressed S3-style object plane (`data` `object/store`): the sha-256 digest here is wire integrity, never a second content identity — the core `ContentKey` remains the one addressing vocabulary, and durable objects live on the object plane, never in a NATS store.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@nats-io/obj`
- package: `@nats-io/obj` (Apache-2.0)
- module format: ESM + CJS dual; a distinct member of the catalog-bound modular family — depends on `@nats-io/jetstream` + `@nats-io/nats-core` (admitted) and pulls `js-sha256` for chunk digests
- runtime target: wherever the core connection runs — node, bun, browser over websockets; bodies cross as WHATWG `ReadableStream<Uint8Array>`
- rail: ObjectStore topology row (`net/pubsub`); transient blob passing, never the object plane

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: store admin, the object surface, and the result shapes; `Objm`/`ObjectStore` members are the [03] entrypoints
- rail: boundaries

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]  | [CONSUMER]                                                                                |
| :-----: | :-------------- | :------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `Objm`          | store admin    | store ensure at engine Layer build over the core connection                               |
|  [02]   | `ObjectStore`   | object surface | one surface; stream-vs-blob is a member selection on payload size                         |
|  [03]   | `ObjectInfo`    | receipt        | the object fact — name, size, chunk, digest; `meta.options.max_chunk_size` rules chunking |
|  [04]   | `ObjectResult`  | read result    | `data: ReadableStream<Uint8Array>`, `error: Promise` — both are consumed                  |
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
|  [07]   | `os.link(name, meta)` / `os.linkStore(name, bucket)` | alias          | byte-free aliasing of one object or a whole store             |
|  [08]   | `os.delete(name)`                                    | remove         | → `Promise<PurgeResponse>`; removal evidence                  |
|  [09]   | `os.seal()` / `os.status(opts?)` / `os.destroy()`    | admin          | read-only freeze, introspection, teardown                     |

## [04]-[IMPLEMENTATION_LAW]

[STACKS_WITH]:
- `@nats-io/jetstream` (`.api/nats-io-jetstream.md`): the store IS a stream of chunk messages; `watch` rides an ordered consumer minted with `AckPolicy.None` and cannot ack — a replay/tail surface by construction; any at-least-once processing of object-change events rides a durable `AckPolicy.Explicit` consumer on the backing stream, never an ack against the watch iterator.
- `@nats-io/nats-core` (`.api/nats-io-nats-core.md`): the same one scoped `wsconnect` connection fans into fanout, KV, and object rows; a store never dials.
- `effect` (`.api/effect.md`): promise members convert through `Effect.tryPromise`; a `put` body mints from an Effect `Stream` via `Stream.toReadableStream`, and a `get` body lifts back through `Stream.fromReadableStream` with `.error` joined into the failure channel so a mid-stream chunk fault is a typed failure, not a silent truncation.
- `data` `object/store`: the content-addressed S3 plane is the durable object system — `ContentKey` addressing, conditional-put idempotency, presigned grants. This row passes transient blobs between processes on the pubsub plane; promotion to durability is a write through the object plane, never a `linkStore` masquerade.

[LOCAL_ADMISSION]:
- Stream-first transfer: `put`/`get` with WHATWG streams is the default; `putBlob`/`getBlob` are admitted only for small bounded payloads.
- Always observe `ObjectResult.error` — a consumed `.data` with an unobserved `.error` is the named silent-truncation defect.
- Alias with `link`/`linkStore` instead of re-putting bytes; a copy where a link suffices is rejected.
- Ensure stores at engine Layer build from topology rows; `seal` is the terminal state of a store whose write phase is ruled complete.

[RAIL_LAW]:
- Package: `@nats-io/obj`
- Owns: store administration, chunked stream put/get with per-chunk digests, blob convenience members, object census and change tail, byte-free links, seal/status/destroy lifecycle
- Accept: Layer-build store ensure, `Stream.toReadableStream`/`Stream.fromReadableStream` at the body seam, `.error` joined to the typed failure channel, links over copies, `putBlob`/`getBlob` only for bounded payloads
- Reject: the store as system of record or as a second content-addressing vocabulary, unobserved `.error` channels, per-call dials, whole-body materialization where a stream flows, ack calls against ordered watch iterators
