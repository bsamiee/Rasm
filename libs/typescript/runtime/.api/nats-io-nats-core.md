# [TS_RUNTIME_API_NATS_IO_NATS_CORE]

`@nats-io/nats-core` is the transport-agnostic NATS client core: the `NatsConnection` publish/subscribe/request surface over subject hierarchies, the `headers()`/`MsgHdrs` codec carrying `Nats-Msg-Id` dedup identity, subject wildcard algebra (`*`/`>`), and the `wsconnect` browser-lane WebSocket dial. `net/pubsub` composes the connection capability; `@nats-io/transport-node` owns the node/bun TCP/TLS dial and `@nats-io/jetstream` layers durability over the same connection.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: connection and message vocabulary; `NatsConnection` members are the [03] entrypoints, `ConnectionOptions` fields keyed below

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CONSUMER]                                                                            |
| :-----: | :---------------------- | :------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `NatsConnection`        | connection    | the scoped capability `net/pubsub` acquires and drains                                |
|  [02]   | `Subscription`          | subscription  | async iterable of `Msg`; ephemeral fanout, JetStream supersedes it                    |
|  [03]   | `Msg`                   | message       | `subject`, `data`, `headers?`, `reply?`, `respond`; raw frame folded to `Envelope`    |
|  [04]   | `MsgHdrs` / `headers()` | header codec  | `append`/`set`/`get`, iterable; `Nats-Msg-Id` dedup identity carriage                 |
|  [05]   | `Empty`                 | payload       | the zero-byte payload constant                                                        |
|  [06]   | `ConnectionOptions`     | options       | dial configuration from `Setting.fanout` rows; fields keyed below                     |
|  [07]   | `Authenticator`         | credential    | `(nonce?: string) => Auth` — the sync callback rebuilt per handshake                  |
|  [08]   | `Auth`                  | credential    | `NoAuth \| TokenAuth \| UserPass \| NKeyAuth \| JwtAuth` — the CONNECT credential set |
|  [09]   | `Status`                | lifecycle     | the `status()` discriminated union `pulse` folds; five of eleven mean a loss          |

- [06]-[CONNECTIONOPTIONS]: `servers`, `name`, `authenticator`, `reconnect`, `maxReconnectAttempts`, `ignoreAuthErrorAbort`, `token`/`user`/`pass`, `tls`, `pingInterval`, `maxPingOut`.
- [07]-[AUTHENTICATOR]: every factory takes a value OR a thunk — the thunk form is the rotation rail, since the client re-invokes the authenticator on each handshake.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: dialing, lifecycle, and core messaging

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CONSUMER]                                                             |
| :-----: | :-------------------------------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `wsconnect(options): Promise<NatsConnection>` | dial           | the one connection acquisition, `Effect.acquireRelease`-bracketed      |
|  [02]   | `nc.drain(): Promise<void>`                   | teardown       | the release arm — flushes subscriptions before close                   |
|  [03]   | `nc.closed(): Promise<void \| Error>`         | lifecycle      | the settled-close observation a supervisor reads                       |
|  [04]   | `nc.publish(subject, payload?, opts?)`        | core publish   | fire-and-forget; `{ headers?, reply? }`, no persistence                |
|  [05]   | `nc.subscribe(subject, opts?)`                | core subscribe | ephemeral delivery; absent listeners miss, JetStream owns replay       |
|  [06]   | `nc.request(subject, payload?, opts?)`        | request-reply  | RPC-shaped exchange over the same connection                           |
|  [07]   | `nc.status(): AsyncIterable<Status>`          | lifecycle      | the out-of-band evidence no publish or consume await reaches           |
|  [08]   | `nc.reconnect(): Promise<void>`               | lifecycle      | forced re-dial — the ONE rail a rotated credential reaches the wire on |

[ENTRYPOINT_SCOPE]: credential authenticators, each writing its own connect-record keys

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [CONSUMER]                                                  |
| :-----: | :------------------------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `tokenAuthenticator(token \| () => token)`               | token          | `auth_token` for the bearer lane an auth-callout reads      |
|  [02]   | `usernamePasswordAuthenticator(user \| fn, pass? \| fn)` | user-pass      | `user`/`pass` on the connect record                         |
|  [03]   | `nkeyAuthenticator(seed? \| () => seed)`                 | nkey           | signs the server nonce, writing `nkey`/`sig`                |
|  [04]   | `jwtAuthenticator(jwt \| () => jwt, seed?)`              | jwt            | `jwt` plus the nonce signature where a seed is supplied     |
|  [05]   | `credsAuthenticator(creds \| () => creds)`               | creds          | parses a creds file and delegates to `jwtAuthenticator`     |
|  [06]   | `buildAuthenticator(opts): Authenticator`                | composite      | folds `authenticator`/`token`/`user`/`pass` to one callback |

[ERRORS]: `AuthorizationError` `UserAuthenticationExpiredError` `ClosedConnectionError` `DrainingConnectionError` `ConnectionError` `TimeoutError` `NoRespondersError` `PermissionViolationError` `RequestError` `ProtocolError`

- `errors` carries every typed rejection a dial, request, or supervisor read discriminates; a rejection outside it folds to the engine's `dial` reason.

## [03]-[IMPLEMENTATION_LAW]

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
- Credentials belong on `ConnectionOptions`, never on `MsgHdrs`: the server authenticates the CONNECT frame, and a header token authenticates a payload nobody checks while the connection stays anonymous.
- Supply the THUNK form of every authenticator factory. The client rebuilds `Connect` from `options.authenticator(nonce)` each time a server INFO lands on a fresh dial, so a thunk re-reads its source per handshake and a literal freezes the process's first credential.
- The callback is synchronous, so no credential fetch happens inside it: the authenticator reads a mutable cell some supervisor keeps fresh.
- A refreshed credential reaches a LIVE connection only through `reconnect()`, which drops in-flight requests and rejects against a closed or draining client.
- Leave `ignoreAuthErrorAbort` unset: the default aborts reconnect after two identical authentication refusals in a row, which is what keeps an unbounded `maxReconnectAttempts` from hot-looping a dead credential.
