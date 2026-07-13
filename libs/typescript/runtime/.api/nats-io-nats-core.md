# [TS_RUNTIME_API_NATS_IO_NATS_CORE]

`@nats-io/nats-core` is the catalog-bound modular NATS client's transport-agnostic core: the `NatsConnection` surface (publish/subscribe/request over subject hierarchies, `drain`/`close`/`closed` lifecycle, `status` events), the `headers()`/`MsgHdrs` message-header codec (the carrier of `Nats-Msg-Id` dedup identity), subject wildcard algebra (`.`-tokenized, `*`/`>` wildcards), and the `wsconnect` WebSocket transport that runs on node, bun, and browsers without a native transport package. It is pure client substrate — `net/pubsub`'s jetstream engine row composes it for the connection capability, and `@nats-io/jetstream` layers the persistence surface over the same connection. The TCP transport lives in `@nats-io/transport-node`, which this branch does not admit: the websocket listener is the one dial surface, a server deployment fact the deploy plane provisions.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@nats-io/nats-core`
- package: `@nats-io/nats-core` (Apache-2.0, Synadia/nats.io)
- module format: ESM + CJS dual; the catalog-bound modular split of the retired monolithic `nats` package
- runtime target: any W3C-WebSocket runtime — node, bun, browser — via `wsconnect`; no `node:*` import in the core
- peer: none; `@nats-io/jetstream` consumes the `NatsConnection` this package mints
- server: NATS 2.14.x with the websocket listener enabled; JetStream durability posture (fsync `sync_interval`, replicas) is server configuration the deploy plane owns
- rail: fanout transport capability (`net/pubsub#JETSTREAM_ROW`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: connection and message vocabulary; `NatsConnection` members are the [03] entrypoints, the `ConnectionOptions` field roster is keyed below the table
- rail: boundaries

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CONSUMER]                                                                         |
| :-----: | :---------------------- | :------------ | :--------------------------------------------------------------------------------- |
|  [01]   | `NatsConnection`        | connection    | the scoped capability `net/pubsub` acquires and drains                             |
|  [02]   | `Subscription`          | subscription  | async iterable of `Msg`; ephemeral fanout, JetStream supersedes it                 |
|  [03]   | `Msg`                   | message       | `subject`, `data`, `headers?`, `reply?`, `respond`; raw frame folded to `Envelope` |
|  [04]   | `MsgHdrs` / `headers()` | header codec  | `append`/`set`/`get`, iterable; `Nats-Msg-Id` dedup identity carriage              |
|  [05]   | `Empty`                 | payload       | the zero-byte payload constant                                                     |
|  [06]   | `ConnectionOptions`     | options       | dial configuration from `Setting.fanout` rows; fields keyed below                  |

- [06]-[CONNECTIONOPTIONS]: `servers`, `name`, `reconnect`, `maxReconnectAttempts`, `token`/`user`/`pass`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: dialing and lifecycle
- rail: system-apis

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CONSUMER]                                                        |
| :-----: | :-------------------------------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `wsconnect(options): Promise<NatsConnection>` | dial           | the one connection acquisition, `Effect.acquireRelease`-bracketed |
|  [02]   | `nc.drain(): Promise<void>`                   | teardown       | the release arm — flushes subscriptions before close              |
|  [03]   | `nc.closed(): Promise<void \| Error>`         | lifecycle      | the settled-close observation a supervisor reads                  |
|  [04]   | `nc.publish(subject, payload?, opts?)`        | core publish   | fire-and-forget; `{ headers?, reply? }`, no persistence           |
|  [05]   | `nc.subscribe(subject, opts?)`                | core subscribe | ephemeral delivery; absent listeners miss, JetStream owns replay  |
|  [06]   | `nc.request(subject, payload?, opts?)`        | request-reply  | growth row — RPC-shaped exchange over the same connection         |

## [04]-[IMPLEMENTATION_LAW]

[STACKS_WITH]:
- `@nats-io/jetstream` (`.api/nats-io-jetstream.md`): `jetstream(nc)` and `jetstreamManager(nc)` take this connection; every durability guarantee lives there — core publish/subscribe alone is at-most-once fanout.
- `effect` (`.api/effect.md`): the connection is a scoped acquisition (`Effect.acquireRelease` over `wsconnect`/`drain`); promise members convert through `Effect.tryPromise` at the engine seam; the async-iterable surfaces lift through `Stream.fromAsyncIterable`.
- `proc/config` `Setting`: the dial origin and dedup window are described config rows; no connection literal exists in the engine.

[LOCAL_ADMISSION]:
- Acquire exactly one connection per process inside the engine Layer; a second connection is a root decision, never a per-call dial.
- Release through `drain()`, never bare `close()` — in-flight subscription delivery completes before the socket drops.
- Core publish/subscribe is not the engine row — a fanout that may not lose subscribed work rides the JetStream surface; core delivery is fire-and-forget by contract.
- The TCP transport (`@nats-io/transport-node`) is unadmitted — a latency case that outgrows websockets is a manifest decision with its own catalogue, never a silent import.

[RAIL_LAW]:
- Package: `@nats-io/nats-core`
- Owns: the connection capability, subject and header vocabulary, the websocket transport, connection lifecycle
- Accept: one scoped `wsconnect` acquisition drained on release, headers minted through `headers()`, options from config rows
- Reject: per-call dials, bare `close()` teardown, core-NATS delivery standing where a JetStream guarantee row is named, raw `WebSocket` handling beside the client
