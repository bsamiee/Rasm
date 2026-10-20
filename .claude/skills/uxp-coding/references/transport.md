# [TRANSPORT]

UXP cannot listen, and a plug-in serving calls to a process outside its host dials that process and serves over the connection.

## [01]-[SERVING]

`RpcServer` reads its transport from one `SocketServer` service with a `run` member, and a `SocketServer` that dials makes the plug-in the server: `RpcServer.layer` over `layerProtocolSocketServer` over that service, with no hand-built protocol.

Plug-in runtime scopes own socket serving and status UI independently of document operations:
- `run` acquires the reader in one scope per connection, and the handler's socket reads that connection and ends with its close
- `Effect.retry` with a spaced schedule surrounds that scoped connection, and `Effect.forever` redials at once after a clean close
- Retries inside one scope keep each failed dial's WebSocket and finalizers until that scope closes
- Link state is one `SubscriptionRef` set where the reader acquisition and `pull` fail or succeed, and a close fails `pull` with the code and reason
- Host middleware sets each running call in a second `SubscriptionRef`, and one status view renders both
- One `RpcGroup` is the whole contract, imported by both ends, and a typed failure reaches the caller through its schema
- Calls run one at a time over synchronous DOM calls and modal scopes, and a handler returning `Rpc.fork` answers while a host call runs
- Group middleware holds host state for each call and covers calls added before `middleware`, and a forked call joins with `add` after it
- Calls a client sends while another call runs go in `requiredForClient` middleware, and every client builds it with `layerClient`
- Streams a peer follows for the link's lifetime return `Rpc.fork`, since each holds its call's concurrency permit until it ends
- UXP delivers a refused connection as a close event, and a manifest denial thrown from the `WebSocket` constructor is a defect
- `fingerprintPayloads` fails a call between ends built from different contracts with a fingerprint mismatch
- Schema binary encodes `Uint8Array` fields as bytes where JSON serialization encodes text

## [02]-[PEER]

Peers accept the socket and hold one `RpcClient` per accepted connection:

| [INDEX] | [OPTION]       | [VALUE]              | [DECIDING_FACT]                                                         |
| :-----: | :------------- | :------------------- | :---------------------------------------------------------------------- |
|  [01]   | `pingInterval` | `Duration.infinity`  | Host calls block RPC pongs, UXP answers WebSocket pings off that thread |
|  [02]   | `retryPolicy`  | `Schedule.recurs(0)` | Accepted sockets cannot be redialed                                     |
|  [03]   | Serialization  | Plug-in's layer      | Ends that differ fail every call                                        |

- UXP dials with `Origin: file://`, and peers upgrade a request from that origin alone
- Plug-ins send their identity as the dial URL's query, and peers decode it through their identity schema
- Socket close alone reports a dead plug-in
- Calls that fail with `RpcClientError` may have taken effect in the host, and callers read host state before repeating one
