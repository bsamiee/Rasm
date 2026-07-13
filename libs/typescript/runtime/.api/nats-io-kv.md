# [TS_RUNTIME_API_NATS_IO_KV]

`@nats-io/kv` materializes a key-value bucket over a JetStream stream on the same core connection the fanout engine holds: `Kvm` mints and opens buckets, `KV.update(key, data, version)` is the revision-CAS optimistic-concurrency member (a stale revision rejects, never overwrites), `watch` tails key changes as an ordered replay-plus-live iterator, `history` replays a key's full revision chain, and `delete`/`purge` split tombstone-with-history from erased history. It is a SEPARATE package, not a jetstream sub-export — the KV topology row of `net/pubsub`, riding the one `wsconnect` connection that fans into fanout, revision-state, and blob-store. Its distributed revision-CAS is the server-side arm of the coordination port whose browser arm is Web Locks — leader claims and mutexes compile to `create`/`update` rows, never into the fanout surface. Every entry carries `revision`, `created`, and an `operation` discriminant (`"PUT" | "DEL" | "PURGE"`), so reads are versioned facts, not bare values; the bucket is bounded distributed state, never the system of record.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@nats-io/kv`
- package: `@nats-io/kv` (Apache-2.0, Synadia/nats.io)
- module format: ESM + CJS dual; a distinct member of the catalog-bound modular family — depends on `@nats-io/jetstream` + `@nats-io/nats-core`, both already admitted
- runtime target: wherever the core connection runs — node, bun, browser over websockets
- rail: KV topology row (`net/pubsub`); the distributed arm of the coordination port

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: bucket admin, the KV surface, and the entry fact; `Kvm`/`KV` members are the [03] entrypoints, the `KvEntry` field roster is keyed below the table
- rail: boundaries

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CONSUMER]                                                                              |
| :-----: | :----------------------- | :------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `Kvm`                    | bucket admin  | bucket ensure at engine Layer build over the core connection                            |
|  [02]   | `KV`                     | kv surface    | the one revision-state surface; write mode is a member selection, never a wrapper       |
|  [03]   | `KvEntry`                | entry fact    | the versioned read/watch unit; `.string()`/`.json()` decode helpers; fields keyed below |
|  [04]   | `QueuedIterator<string>` | key census    | from `keys`; lifted through `Stream.fromAsyncIterable` like every NATS iterator         |

- [03]-[KVENTRY]: `bucket`, `key`, `value: Uint8Array`, `created`, `revision`, `delta?`, `operation: "PUT" \| "DEL" \| "PURGE"`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: bucket lifecycle, writes, reads, and replay; write members `create`/`update`/`put` return `Promise<number>` (the new revision)
- rail: boundaries

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CONSUMER]                                                |
| :-----: | :---------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `new Kvm(nc)` → `create`/`open`/`list`                      | mint           | bucket ensure per topology row, like stream ensure        |
|  [02]   | `kv.create(k, data, markerTTL?)`                            | claim write    | create-if-absent claim/lock mint                          |
|  [03]   | `kv.update(k, data, version, timeout?)`                     | CAS write      | the OCC arm; stale `version` rejects, default write       |
|  [04]   | `kv.put(k, data, opts?)`                                    | plain write    | last-writer-wins only where the row genuinely rules it    |
|  [05]   | `kv.get(k, opts?: { revision })`                            | point read     | → `Promise<KvEntry \| null>`; `revision` time-travel      |
|  [06]   | `kv.delete(k, opts?)` / `kv.purge(k, opts?)`                | remove         | tombstone-with-history vs erased history                  |
|  [07]   | `kv.watch(opts?)` / `kv.history(opts)` / `kv.keys(filter?)` | replay + tail  | change tail, revision replay, key census; async iterators |
|  [08]   | `kv.status()` / `kv.destroy()`                              | admin          | bucket introspection and teardown                         |

## [04]-[IMPLEMENTATION_LAW]

[STACKS_WITH]:
- `@nats-io/jetstream` (`.api/nats-io-jetstream.md`): the bucket IS a stream and `watch`/`history` ride JetStream consumers — the ordered-consumer law carries over: a nameless ordered consumer is minted with `AckPolicy.None` and cannot ack, so `watch`/`history` are replay surfaces by construction; at-least-once processing of KV change events rides a durable `AckPolicy.Explicit` consumer on the bucket's backing stream, never an ack against the watch iterator.
- `@nats-io/nats-core` (`.api/nats-io-nats-core.md`): the same one scoped `wsconnect` connection — the interior connection capability fans into fanout, KV, and object rows; a bucket never dials.
- `effect` (`.api/effect.md`): promise members convert through `Effect.tryPromise` at the seam; `watch`/`history`/`keys` lift through `Stream.fromAsyncIterable` under a scoped bracket; a CAS rejection folds to a typed conflict fault whose retry ride is a `Schedule` policy, never a bare loop.
- coordination port siblings: Web Locks (`navigator.locks`) is the browser cross-tab arm; `create`/`update` revision-CAS is the distributed arm — one port, two rows, and neither is a `Fanout` member.

[LOCAL_ADMISSION]:
- Write CAS-first: `update` with the read revision is the default; a bare `put` is admitted only where last-writer-wins is the ruled semantics of the key row.
- Mint claims through `create` — create-if-absent is the lock/leader primitive; polling a `get` for absence is rejected.
- Treat `watch` as replay-plus-tail, never a work queue — queue semantics belong to the durable-consumer fanout lane.
- Ensure buckets at engine Layer build from topology rows; bucket shape never lives beside a call site.

[RAIL_LAW]:
- Package: `@nats-io/kv`
- Owns: bucket administration, revision-CAS writes, versioned point reads with revision time-travel, watch/history replay, key census, tombstone-versus-purge removal
- Accept: Layer-build bucket ensure, `update`-first writes with typed CAS conflict folds, `create` as the claim mint, iterators lifted through `Stream.fromAsyncIterable` under scoped brackets
- Reject: blind `put` overwrites where a revision was read, `watch` consumed as a work queue, a second dial beside the engine connection, the bucket standing as system of record, ack calls against ordered watch iterators
