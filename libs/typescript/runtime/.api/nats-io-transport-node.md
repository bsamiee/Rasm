# [TS_RUNTIME_API_NATS_IO_TRANSPORT_NODE]

`@nats-io/transport-node` is the modular NATS client's native TCP/TLS transport — the node-lane dial `@nats-io/nats-core` omits. Its one entrypoint `connect(opts?): Promise<NatsConnection>` mints the same `NatsConnection` websocket `wsconnect` yields, backed by a `node:net` socket upgrading to `node:tls` under `NodeTlsOptions`, and re-exports the core surface (`nats-base-client`) so a node consumer draws connection, subject, and header vocabulary from this one package. It completes the fanout set for the node runtime row — `net/pubsub`'s jetstream engine, KV, and object store each compose a `NatsConnection` transport-blind, dialing TCP here where a server carries no websocket listener while the browser lane keeps `wsconnect`: one connection capability, two dial surfaces, a boot-time platform choice.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@nats-io/transport-node`
- package: `@nats-io/transport-node` (Apache-2.0, Synadia/nats.io)
- module format: CJS (`main: index.js`, `types: lib/mod.d.ts`); bun-compatible, node-only by `node:net`/`node:tls`/`node:buffer` import
- runtime target: node and bun — the native-socket lane; a browser lane never imports it, dialing `wsconnect` from `@nats-io/nats-core` instead
- dependencies: `@nats-io/nats-core` (re-exported core surface), `@nats-io/nkeys` (nkey auth), `@nats-io/nuid` (inbox-id minting)
- server: NATS reachable on the TCP client port; `NodeTlsOptions` carries the deploy-plane CA roots, client certificates, and trust posture
- rail: fanout transport capability, node lane (`net/pubsub#JETSTREAM_ROW`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the node-dial option delta over the core `ConnectionOptions`; all core message and connection vocabulary re-exports from `@nats-io/nats-core` and is keyed in `.api/nats-io-nats-core.md`
- rail: boundaries

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CONSUMER]                                                                          |
| :-----: | :---------------------- | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `NodeConnectionOptions` | options       | `Omit<ConnectionOptions, "tls">` with `tls?: NodeTlsOptions \| null` — dial config  |
|  [02]   | `NodeTlsOptions`        | tls options   | core `TlsOptions` plus `rejectUnauthorized?` — CA/cert posture from `Setting`       |
|  [03]   | `NodeTransport`         | transport     | the `Transport` implementation over `node:net`; runtime substrate, never dialed raw |
|  [04]   | `NatsConnection`        | connection    | re-export — the identical scoped capability `net/pubsub` acquires and drains        |

- [01]-[NODECONNECTIONOPTIONS]: inherits the core roster (`servers`, `name`, `reconnect`, `maxReconnectAttempts`, `token`/`user`/`pass`, `authenticator`) and swaps the `tls` field for the node shape.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: node-lane dialing; lifecycle and message members are the re-exported `NatsConnection` surface keyed in `.api/nats-io-nats-core.md`
- rail: system-apis

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [BOUNDARY]                        |
| :-----: | :----------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `connect(opts?): Promise<NatsConnection>`  | dial           | scoped node TCP/TLS acquisition   |
|  [02]   | `nodeResolveHost(host): Promise<string[]>` | resolution     | transport DNS helper              |
|  [03]   | re-export `* from nats-base-client`        | core surface   | connection and message vocabulary |

## [04]-[IMPLEMENTATION_LAW]

[STACKS_WITH]:
- `@nats-io/nats-core` (`.api/nats-io-nats-core.md`): the sibling transport — `connect` here and `wsconnect` there both yield `NatsConnection`; a consumer imports the core vocabulary through this package's re-export on the node lane, never both packages at once.
- `@nats-io/jetstream` (`.api/nats-io-jetstream.md`), `@nats-io/kv` (`.api/nats-io-kv.md`), `@nats-io/obj` (`.api/nats-io-obj.md`): each takes the `NatsConnection` transport-blind — durability, revision-CAS, and blob rows compose identically over a TCP or websocket connection.
- `effect` (`.api/effect.md`): the connection is a scoped acquisition (`Effect.acquireRelease` over `connect`/`drain`); promise members convert through `Effect.tryPromise` at the engine seam; async-iterable surfaces lift through `Stream.fromAsyncIterable`.
- `proc/config` `Setting`: the dial origin and TLS posture are described config rows selecting this lane's `connect` over `wsconnect`; no connection literal or transport choice exists in the engine.

[LOCAL_ADMISSION]:
- Acquire exactly one connection per process inside the engine Layer; the transport (`connect` vs `wsconnect`) is a boot-time platform choice, never a per-call decision.
- Release through `drain()`, never bare `close()` — in-flight subscription delivery completes before the socket drops, identical to the websocket lane.
- Import the core vocabulary through this package's re-export on the node lane; a second import of `@nats-io/nats-core` beside it is duplicate substrate.
- TLS posture rides `NodeTlsOptions` from a `Setting` row; a hardcoded `rejectUnauthorized: false` is the named defect — CA roots and client certs are deploy-plane facts.

[RAIL_LAW]:
- Package: `@nats-io/transport-node`
- Owns: the node/bun TCP/TLS dial (`connect`), the `NodeTransport` socket implementation, the node-lane TLS option delta, the re-exported core surface
- Accept: one scoped `connect` acquisition drained on release, TLS posture from config rows, core vocabulary through the re-export
- Reject: per-call dials, bare `close()` teardown, duplicate `@nats-io/nats-core` import beside the re-export, browser-lane import where `wsconnect` is the transport, hardcoded TLS trust
