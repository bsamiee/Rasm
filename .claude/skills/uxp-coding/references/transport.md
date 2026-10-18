# [TRANSPORT]

UXP cannot listen, and a plug-in serving calls to a process outside its host dials that process and serves over the connection.

## [01]-[SERVING]

`RpcServer` reads its transport from one `SocketServer` service with a `run` member, and a `SocketServer` that dials makes the plug-in the server.

`serve` from `@rasm/mcp/link` dials the peer and serves an `RpcGroup` over the socket, redialing after each close:
- One `RpcGroup` is the whole contract, imported by both ends, and a typed failure reaches the caller through its schema
- Handlers calling the host share one `Semaphore` permit where DOM calls are synchronous or one modal scope serializes them
- `RpcServer` runs calls with unbounded concurrency by default, answering a call that touches no host state while a host call holds the permit
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
