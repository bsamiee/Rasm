# [TS_RUNTIME_API_NATS_IO_KV]

`@nats-io/kv` materializes a versioned key-value bucket over a JetStream stream on the fanout engine's one core connection: every write is revision-CAS optimistic concurrency, every read a versioned fact carrying its `revision` and `operation` discriminant, and change tails replay before going live. It is the distributed arm of the coordination port — leader claims and mutexes compile to `create`/`update` revision rows, the browser arm being Web Locks — bounded distributed state, never the system of record.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@nats-io/kv`
- package: `@nats-io/kv` (Apache-2.0)
- module format: ESM + CJS dual; catalog-bound modular sibling of `@nats-io/jetstream` + `@nats-io/nats-core`
- runtime target: node, bun, browser over websockets — wherever the core connection runs
- rail: KV topology row (`net/pubsub`); the distributed arm of the coordination port

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: bucket admin, the KV surface, the versioned entry fact

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CONSUMER]                                                                              |
| :-----: | :----------------------- | :------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `Kvm`                    | bucket admin  | bucket ensure at engine Layer build over the core connection                            |
|  [02]   | `KV`                     | kv surface    | the one revision-state surface; write mode is a member selection, never a wrapper       |
|  [03]   | `KvEntry`                | entry fact    | the versioned read/watch unit; `.string()`/`.json()` decode helpers; fields keyed below |
|  [04]   | `QueuedIterator<string>` | key census    | from `keys`; lifted through `Stream.fromAsyncIterable` like every NATS iterator         |

- [03]-[KVENTRY]: `bucket`, `key`, `value: Uint8Array`, `created`, `revision`, `delta?`, `operation: "PUT" \| "DEL" \| "PURGE"`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: bucket lifecycle, writes, reads, replay; `create`/`update`/`put` return `Promise<number>`, the new revision

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [CONSUMER]                                             |
| :-----: | :----------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `new Kvm(nc)` → `create`/`open`/`list`                       | mint           | bucket ensure per topology row, like stream ensure     |
|  [02]   | `kv.create(k, data, markerTTL?)`                             | claim write    | create-if-absent claim/lock mint                       |
|  [03]   | `kv.update(k, data, version, timeout?)`                      | CAS write      | the OCC arm; stale `version` rejects, default write    |
|  [04]   | `kv.put(k, data, opts?)`                                     | plain write    | last-writer-wins only where the row genuinely rules it |
|  [05]   | `kv.get(k, opts?: { revision })`                             | point read     | → `Promise<KvEntry \| null>`; `revision` time-travel   |
|  [06]   | `kv.delete(k, opts?)` / `kv.purge(k, opts?)`                 | remove         | tombstone-with-history vs erased history               |
|  [07]   | `kv.watch(opts?)` / `kv.history(opts?)` / `kv.keys(filter?)` | replay + tail  | change tail, revision replay, key census; iterators    |
|  [08]   | `kv.status()` / `kv.destroy()`                               | admin          | bucket introspection and teardown                      |

## [04]-[IMPLEMENTATION_LAW]

[STACKING]:
- `@nats-io/jetstream` (`.api/nats-io-jetstream.md`): the bucket IS a backing stream — `watch`/`history` ride nameless ordered consumers (`AckPolicy.None`), replay surfaces by construction; at-least-once KV-event processing binds a durable `AckPolicy.Explicit` consumer on that stream, never an ack against the watch iterator.
- `@nats-io/nats-core` (`.api/nats-io-nats-core.md`): the one scoped `wsconnect` connection fans into fanout, KV, and object rows; a bucket never dials.
- `effect` (`.api/effect.md`): promise members convert through `Effect.tryPromise`; `watch`/`history`/`keys` lift through `Stream.fromAsyncIterable` under a scoped bracket; a CAS rejection folds to a typed conflict fault retried by a `Schedule` policy, never a bare loop.

[LOCAL_ADMISSION]:
- Writes are CAS-first: `update` with the read revision is default; a bare `put` admits only where the key row rules last-writer-wins.
- `create` mints claims — create-if-absent is the lock/leader primitive; polling `get` for absence is rejected.
- `watch` is replay-plus-tail, never a work queue — queue semantics ride the durable-consumer fanout lane.
- Buckets ensure at engine Layer build from topology rows; bucket shape never lives beside a call site.

[RAIL_LAW]:
- Package: `@nats-io/kv`
- Owns: bucket administration, revision-CAS writes, versioned point reads with revision time-travel, watch/history replay, key census, tombstone-versus-purge removal
- Accept: Layer-build bucket ensure, `update`-first writes with typed CAS conflict folds, `create` as the claim mint, iterators lifted through `Stream.fromAsyncIterable` under scoped brackets
- Reject: blind `put` where a revision was read, `watch` as a work queue, a second dial beside the engine connection, the bucket as system of record, ack calls against ordered watch iterators
