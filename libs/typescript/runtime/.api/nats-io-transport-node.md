# [TS_RUNTIME_API_NATS_IO_TRANSPORT_NODE]

`@nats-io/transport-node` mints the node/bun native TCP/TLS dial `@nats-io/nats-core` omits and re-exports the core surface, so a node consumer draws connection, subject, and header vocabulary from this one package. `connect` yields the same `NatsConnection` the browser lane's `wsconnect` returns — one connection capability behind two dial surfaces, a boot-time platform choice.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the node-dial option delta over core `ConnectionOptions`; core message, credential, and connection types re-export from `@nats-io/nats-core` (`.api/nats-io-nats-core.md`).

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :----------------------- | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `NodeConnectionOptions`  | options       | `Omit<ConnectionOptions, "tls">` swapping the `tls` field for the node shape  |
|  [02]   | `NodeTlsOptions`         | tls options   | core `TlsOptions` plus `rejectUnauthorized?` — CA/cert posture from `Setting` |
|  [03]   | `NatsConnection`         | connection    | re-export — the scoped capability `net/pubsub` acquires and drains            |
|  [04]   | `Authenticator` / `Auth` | credential    | re-export — the CONNECT-frame credential the node dial carries identically    |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: node-lane dialing; lifecycle, credential, and message members ride the re-exported core surface.

| [INDEX] | [SURFACE]                                        | [SHAPE] | [CAPABILITY]                                                             |
| :-----: | :----------------------------------------------- | :------ | :----------------------------------------------------------------------- |
|  [01]   | `connect(opts?): Promise<NatsConnection>`        | factory | node TCP/TLS dial minting the scoped connection                          |
|  [02]   | `setTransportFactory(factory)`                   | static  | swaps the socket implementation; proof substrate, never production       |
|  [03]   | `re-export * from '@nats-io/nats-core/internal'` | static  | core connection, credential, and message vocabulary — the INTERNAL entry |

## [03]-[IMPLEMENTATION_LAW]

[STACKING]:
- `@nats-io/nats-core` (`.api/nats-io-nats-core.md`): `connect` and `wsconnect` both yield `NatsConnection`; the node lane draws core vocabulary through this package's re-export, never importing both packages. The authenticator factories and the credential types come through that re-export unchanged, so a credential projection is written once against the core spelling and holds on either lane.
- `@nats-io/jetstream` (`.api/nats-io-jetstream.md`), `@nats-io/kv` (`.api/nats-io-kv.md`), `@nats-io/obj` (`.api/nats-io-obj.md`): each takes the `NatsConnection` transport-blind — durability, revision-CAS, and blob rows compose identically over a TCP or websocket connection.
- `effect` (`.api/effect.md`): `Effect.acquireRelease` brackets `connect`/`drain`, `Effect.tryPromise` converts promise members at the engine boundary, `Stream.fromAsyncIterable` lifts the async-iterable surfaces.
- `proc/config` `Setting`: config rows carry the dial origin; no connection literal exists in the engine.

[LOCAL_ADMISSION]:
- Acquire one connection per process inside the engine Layer; `connect` versus `wsconnect` is a boot-time platform choice, never a per-call decision.
- Release through `drain()` so in-flight subscription delivery completes before the socket drops.
- TLS posture rides `NodeTlsOptions` from a `Setting` row; a hardcoded `rejectUnauthorized: false` is the named defect, since CA roots and client certs are deploy-plane facts.
- The CONNECT-frame credential is the core catalog's law verbatim — a thunk-form `authenticator`, never a message header, never a literal on a rotating plane. TLS is the transport's identity to the broker and the authenticator is the workload's; neither substitutes for the other.
- The socket implementation is a package internal reached only through `setTransportFactory`, so nothing constructs a transport directly.
