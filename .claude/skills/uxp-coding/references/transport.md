# [TRANSPORT]

UXP cannot listen, and a plug-in serving calls to a process outside its host dials that process and serves over the connection.

## [01]-[SERVING]

`RpcServer` reads its transport from one `SocketServer` service with a `run` member, and a `SocketServer` that dials makes the plug-in the server:

```ts
import { host } from 'adobe:uxp';
import type { NetAddress } from 'effect/net';
import { Rpc, RpcGroup, RpcSerialization, RpcServer } from 'effect/rpc';
import { Socket, SocketServer } from 'effect/socket';

class Thrown extends Schema.TaggedError<Thrown>()('Thrown', { cause: Schema.Defect() }) {}

const Calls = RpcGroup.make(
    Rpc.make('Version', { success: Schema.String }),
    Rpc.make('Run', { payload: { source: Schema.String }, success: Schema.Json, error: Thrown }),
);

const served = <R>(
    run: (source: string) => Effect.Effect<Schema.Json, Thrown, R>,
    { peer, redial, concurrency }: { readonly peer: NetAddress.InetAddress; readonly redial: Duration.Input; readonly concurrency: number },
): Layer.Layer<never, never, R> =>
    RpcServer.layer(Calls, { concurrency, disableTracing: true }).pipe(
        Layer.provide(Calls.toLayer({ Version: () => Effect.succeed(host.version), Run: ({ source }) => run(source) })),
        Layer.provide(RpcServer.layerProtocolSocketServer),
        Layer.provide(RpcSerialization.layerSchemaBinary({ fingerprintPayloads: true })),
        Layer.provide(
            Layer.succeed(SocketServer.SocketServer)({
                address: peer,
                run: (handler) =>
                    Socket.makeWebSocket(`ws://localhost:${peer.port}`, { openTimeout: redial }).pipe(
                        Effect.flatMap(flow(handler, Effect.scoped)),
                        Effect.ignore,
                        Effect.andThen(Effect.sleep(redial)),
                        Effect.forever,
                        Effect.provide(Socket.layerWebSocketConstructorGlobal),
                    ),
            }),
        ),
    );
```

`Calls` is the whole contract, imported by both ends, and a typed failure arrives at the caller as `Thrown` through its schema. `peer` is a validated loopback address that the dial names as `localhost` at its port. A refused dial fails `makeWebSocket` with a `SocketError` that `Effect.ignore` drops before the sleep and the next dial, and each connection's handler runs in its own scope. The plug-in's runtime builds `served` once and closes the socket on disposal.

- `concurrency` is the count of host calls that can overlap, 1 where DOM calls are synchronous or one modal scope serializes them
- UXP delivers a refused connection as a close event, and a manifest denial thrown from the `WebSocket` constructor is a defect
- `fingerprintPayloads` fails a call between ends built from different contracts with a fingerprint mismatch
- Schema binary encodes `Uint8Array` fields as bytes where JSON serialization encodes text

## [02]-[PEER]

Peers accept the socket and hold one `RpcClient` per accepted connection:

| [INDEX] | [OPTION]       | [VALUE]              | [DECIDING_FACT]                                                       |
| :-----: | :------------- | :------------------- | :-------------------------------------------------------------------- |
|  [01]   | `pingInterval` | Above longest call   | Synchronous host calls block the plug-in's one thread, pongs included |
|  [02]   | `retryPolicy`  | `Schedule.recurs(0)` | Accepted sockets cannot be redialed                                   |
|  [03]   | Serialization  | Plug-in's layer      | Ends that differ fail every call                                      |

- Socket close reports a dead plug-in, since UXP replies to protocol pings off its JavaScript thread
- Calls that fail with `RpcClientError` may have taken effect in the host, and callers read host state before repeating one
