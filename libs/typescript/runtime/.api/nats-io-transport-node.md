# [TS_RUNTIME_API_NATS_IO_TRANSPORT_NODE]

`@nats-io/transport-node` mints the node/bun native TCP/TLS dial `@nats-io/nats-core` omits and re-exports the core surface, so a node consumer draws connection, subject, and header vocabulary from this one package. `connect` yields the same `NatsConnection` the browser lane's `wsconnect` returns — one connection capability behind two dial surfaces, a boot-time platform choice.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@nats-io/transport-node`
- package: `@nats-io/transport-node` (Apache-2.0)
- module: CJS; node/bun only by `node:net`/`node:tls`/`node:buffer` import
- runtime: node and bun native-socket lane; the browser lane dials `wsconnect` from `@nats-io/nats-core`
- depends: `@nats-io/nats-core`, re-exported as this package's core surface
- rail: fanout transport capability, node lane of the `net/pubsub` engine row

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the node-dial option delta over core `ConnectionOptions`; core message and connection types re-export from `@nats-io/nats-core` (`.api/nats-io-nats-core.md`).

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :---------------------- | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `NodeConnectionOptions` | options       | `Omit<ConnectionOptions, "tls">` swapping the `tls` field for the node shape  |
|  [02]   | `NodeTlsOptions`        | tls options   | core `TlsOptions` plus `rejectUnauthorized?` — CA/cert posture from `Setting` |
|  [03]   | `NodeTransport`         | transport     | the `Transport` over `node:net`; runtime substrate, never dialed raw          |
|  [04]   | `NatsConnection`        | connection    | re-export — the scoped capability `net/pubsub` acquires and drains            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: node-lane dialing; lifecycle and message members ride the re-exported `NatsConnection` surface.

| [INDEX] | [SURFACE]                                  | [SHAPE] | [CAPABILITY]                                    |
| :-----: | :----------------------------------------- | :------ | :---------------------------------------------- |
|  [01]   | `connect(opts?): Promise<NatsConnection>`  | factory | node TCP/TLS dial minting the scoped connection |
|  [02]   | `nodeResolveHost(host): Promise<string[]>` | static  | transport DNS resolution                        |
|  [03]   | `re-export * from nats-base-client`        | static  | core connection and message vocabulary          |

## [04]-[IMPLEMENTATION_LAW]

[STACKING]:
- `@nats-io/nats-core` (`.api/nats-io-nats-core.md`): `connect` and `wsconnect` both yield `NatsConnection`; the node lane draws core vocabulary through this package's re-export, never importing both packages.
- `@nats-io/jetstream` (`.api/nats-io-jetstream.md`), `@nats-io/kv` (`.api/nats-io-kv.md`), `@nats-io/obj` (`.api/nats-io-obj.md`): each takes the `NatsConnection` transport-blind — durability, revision-CAS, and blob rows compose identically over a TCP or websocket connection.
- `effect` (`.api/effect.md`): `Effect.acquireRelease` brackets `connect`/`drain`, `Effect.tryPromise` converts promise members at the engine seam, `Stream.fromAsyncIterable` lifts the async-iterable surfaces.
- `proc/config` `Setting`: config rows carry the dial origin; no connection literal exists in the engine.

[LOCAL_ADMISSION]:
- Acquire one connection per process inside the engine Layer; `connect` versus `wsconnect` is a boot-time platform choice, never a per-call decision.
- Release through `drain()` so in-flight subscription delivery completes before the socket drops.
- TLS posture rides `NodeTlsOptions` from a `Setting` row; a hardcoded `rejectUnauthorized: false` is the named defect, since CA roots and client certs are deploy-plane facts.

[RAIL_LAW]:
- Package: `@nats-io/transport-node`
- Owns: the node/bun TCP/TLS dial (`connect`), the `NodeTransport` socket implementation, the node-lane TLS option delta, the re-exported core surface
- Accept: one scoped `connect` drained on release, TLS posture from config rows, core vocabulary through the re-export
- Reject: per-call dials, bare `close()` teardown, a second `@nats-io/nats-core` import beside the re-export, a browser-lane import where `wsconnect` is the transport
