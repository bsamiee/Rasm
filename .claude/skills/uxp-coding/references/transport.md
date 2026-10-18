# [TRANSPORT]

UXP cannot listen, and a plug-in serving calls to a process outside its host dials that process and serves over the connection.

## [01]-[SERVING]

`RpcServer` reads its transport from one `SocketServer` service with a `run` member, and a `SocketServer` that dials makes the plug-in the server.

Plug-in runtime scopes own socket serving and status UI independently of document operations:
- One `RpcGroup` is the whole contract, imported by both ends, and a typed failure reaches the caller through its schema
- Calls run one at a time over synchronous DOM calls and modal scopes, and a handler returning `Rpc.fork` answers while a host call runs
- Middleware a host library adds to its group holds host state for each call, and the group omits a forked call before the middleware and adds it back
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

- Socket close alone reports a dead plug-in
- Calls that fail with `RpcClientError` may have taken effect in the host, and callers read host state before repeating one
