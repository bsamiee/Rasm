# [TRANSPORT]

UXP cannot listen, and a plugin serving calls to a process outside its host dials that process and serves over the connection.

## [01]-[SERVING]

`RpcServer` reads its transport from a `SocketServer` service's `run` member, and a dialing `SocketServer` makes the plugin the server. Plugins stack `RpcServer.layer(group, { concurrency: 1 })` over `layerProtocolSocketServer` over that service, its `WebSocket` from `Socket.layerWebSocketConstructorGlobal`:

```ts
declare global {
    interface ImportMetaEnv {
        readonly LINK: { readonly endpoint: string; readonly redial: `${number} ${Duration.Unit}` };
    }
}

const dialing = (query: Record.ReadonlyRecord<string, string>) =>
    Layer.effect(SocketServer.SocketServer)(
        Effect.gen(function* () {
            const endpoint = yield* Effect.fromResult(Url.fromString(import.meta.env.LINK.endpoint));
            const { reader, writer } = yield* Socket.makeWebSocket(Url.setUrlParams(endpoint, query).href);
            return {
                address: NetAddress.inetAddressUnsafe(NetAddress.ipv4Loopback, Number(endpoint.port)),
                run: (handler) =>
                    reader.pipe(
                        Effect.flatMap((opened) => handler(Socket.make({ reader: Effect.succeed(opened), writer }))),
                        Effect.scoped,
                        Effect.tapCauseIf(Cause.hasDies, Uxp.logged),
                        Effect.exit,
                        Effect.andThen(Effect.sleep(import.meta.env.LINK.redial)),
                        Effect.forever,
                    ),
            };
        }),
    );
```

`query` holds the plugin's encoded identity. Builds define `import.meta.env.LINK` from the app's one link value (route path, redial spacing) and the port the peer daemon's route listens on, and no plugin passes link values at runtime. Each `reader` run opens one WebSocket that its `Effect.scoped` closes, and one `redial` sleep follows a refused dial and a clean close alike.

- `RpcServer` turns every socket error but a close into a defect, and `run` logs each die before `Effect.exit` ends the connection
- One `RpcGroup` is the whole contract, and typed failures reach the caller through its error schema, `Cause` errors through a `Schema.declare` with `toCodecJson`
- Host packages split a `contract` entry the peer and build import in Node, holding no host module, from a `host` entry exporting the link layer
- Served calls and panel actions run one at a time through the permit and preference scope the group middleware applies
- Handlers returning `Rpc.fork` answer while a host call runs
- Group middleware holds host state for each call and covers calls added before `middleware`, and a forked call joins with `add` after it
- Calls a client sends while another call runs go in `requiredForClient` middleware, and every client builds it with `layerClient`
- Each stream holds its call's concurrency permit until it ends, and streams a peer follows for the link's lifetime return `Rpc.fork`
- Manifest denials the `WebSocket` constructor throws are defects
- `fingerprintPayloads` fails a call between ends built from different contracts with a fingerprint mismatch
- Schema binary encodes `Uint8Array` fields as bytes where JSON serialization encodes text

## [02]-[PEER]

Peers accept the socket and hold one `RpcClient` per accepted connection:

| [INDEX] | [OPTION]       | [VALUE]              | [REASON]                                                                |
| :-----: | :------------- | :------------------- | :---------------------------------------------------------------------- |
|  [01]   | `pingInterval` | `Duration.infinity`  | Host calls block RPC pongs, UXP answers WebSocket pings off that thread |
|  [02]   | `retryPolicy`  | `Schedule.recurs(0)` | Accepted sockets cannot be redialed                                     |
|  [03]   | Serialization  | Plugin's layer       | Ends that differ fail every call                                        |

- UXP dials with `Origin: file://`, and peers upgrade a request from that origin alone
- Plugins send their application bundle URL and extension id as the dial URL's query, and peers decode it through their identity schema
- Socket close alone reports a dead plugin, and a redial naming a held extension and application replaces that link before its stale socket closes
- Calls select the link serving an application by bundle URL, and a miss fails with every linked URL as candidate
- Calls that fail with `RpcClientError` may have taken effect in the host, and callers read host state before repeating one
