# [TS_RUNTIME_API_NATS_IO_NATS_CORE]

`@nats-io/nats-core` is the transport-agnostic NATS client core: the `NatsConnection` publish/subscribe/request surface over subject hierarchies, the `headers()`/`MsgHdrs` codec carrying `Nats-Msg-Id` dedup identity, subject wildcard algebra (`*`/`>`), and the `wsconnect` browser-lane WebSocket dial. `net/pubsub` composes the connection capability; `@nats-io/transport-node` owns the node/bun TCP/TLS dial and `@nats-io/jetstream` layers durability over the same connection.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@nats-io/nats-core`
- package: `@nats-io/nats-core` (Apache-2.0)
- module: ESM + CJS dual
- runtime: any W3C-WebSocket runtime (node, bun, browser) via `wsconnect`; no `node:*` import
- server: websocket listener enabled
- rail: fanout transport capability the `net/pubsub` engine row composes

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: connection and message vocabulary; `NatsConnection` members are the [03] entrypoints, `ConnectionOptions` fields keyed below

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

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CONSUMER]                                                        |
| :-----: | :-------------------------------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `wsconnect(options): Promise<NatsConnection>` | dial           | the one connection acquisition, `Effect.acquireRelease`-bracketed |
|  [02]   | `nc.drain(): Promise<void>`                   | teardown       | the release arm — flushes subscriptions before close              |
|  [03]   | `nc.closed(): Promise<void \| Error>`         | lifecycle      | the settled-close observation a supervisor reads                  |
|  [04]   | `nc.publish(subject, payload?, opts?)`        | core publish   | fire-and-forget; `{ headers?, reply? }`, no persistence           |
|  [05]   | `nc.subscribe(subject, opts?)`                | core subscribe | ephemeral delivery; absent listeners miss, JetStream owns replay  |
|  [06]   | `nc.request(subject, payload?, opts?)`        | request-reply  | RPC-shaped exchange over the same connection                      |

## [04]-[IMPLEMENTATION_LAW]

[STACKING]:
- `@nats-io/jetstream`(`.api/nats-io-jetstream.md`): `jetstream(nc)` and `jetstreamManager(nc)` take this connection; every durability guarantee lives there.
- `@nats-io/transport-node`(`.api/nats-io-transport-node.md`): the node/bun native TCP/TLS `connect` yields the same `NatsConnection`; `wsconnect` here is the browser lane.
- `effect`(`.api/effect.md`): the connection is a scoped `Effect.acquireRelease` over `wsconnect`/`drain`; promise members convert through `Effect.tryPromise`, async-iterable surfaces lift through `Stream.fromAsyncIterable`.
- `proc/config` `Setting`: dial origin and dedup window are config rows; no connection literal exists in the engine.

[LOCAL_ADMISSION]:
- Acquire exactly one connection per process inside the engine Layer; a second dial is a root decision, never a per-call act.
- Release through `drain()`; in-flight subscription delivery completes before the socket drops.
- A fanout that may not lose subscribed work rides the JetStream surface; core delivery is fire-and-forget.
- Transport selection (`wsconnect` vs `@nats-io/transport-node` `connect`) is a boot-time platform choice per `Setting`, never a per-call or mixed import.

[RAIL_LAW]:
- Package: `@nats-io/nats-core`
- Owns: connection capability, subject and header vocabulary, websocket transport, connection lifecycle
- Accept: one scoped `wsconnect` acquisition drained on release, headers minted through `headers()`, options from config rows
- Reject: per-call dials, bare `close()` teardown, core delivery where a JetStream guarantee row is named, raw `WebSocket` handling beside the client
