# [TS_RUNTIME_API_MODELCONTEXTPROTOCOL_SDK]

The reference MCP SDK, admitted for ONE lane: `ai/tool.ts` uses `Client` + a client transport to consume
external MCP servers (their tools become `Tool.providerDefined`-style rows in an app toolkit). The entire
`./server` subpath — `McpServer`, `server/mcp`, `server/stdio`, `server/streamableHttp` — is out of scope:
Rasm hosts MCP on the native `@effect/ai` `McpServer`/`McpSchema`, and this SDK never re-owns hosting. The
SDK is Promise-based and validates with Zod (`types.d.ts` schemas), so the boundary transcribes it into the
Effect ecosystem: `Effect.tryPromise` per call, `Scope`/`acquireRelease` for the transport lifecycle, and a
re-parse of each result through `effect/Schema` into native shapes. External tool annotations
(`readOnlyHint`/`destructiveHint`/`idempotentHint`/`openWorldHint`) project one-to-one onto `@effect/ai`'s
`Tool.Readonly`/`Destructive`/`Idempotent`/`OpenWorld` at the seam.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@modelcontextprotocol/sdk`
- package: `@modelcontextprotocol/sdk` (MIT)
- entry: subpath exports `./client`, `./server` (OUT OF SCOPE), `./validation`, `./validation/ajv`, `./validation/cfworker`, `./experimental`, `./experimental/tasks`, `./*`
- in-scope modules: `client/index`, `client/{stdio,streamableHttp,sse,websocket}`, `shared/{protocol,transport,auth}`, `client/auth`, `types`, `inMemory`, `validation/*`, `experimental/tasks/client`
- asset: Promise-based MCP client on a pluggable transport; Zod-schema protocol wire; OAuth 2.0 client flow
- boundary: Promise + Zod — transcribe to `Effect` + `Schema` at `ai/tool.ts`
- rail: mcp-client

## [02]-[CLIENT]

[PUBLIC_TYPE_SCOPE]: the MCP client + capability calls — rail: mcp-client

`new Client(info, options)` then `connect(transport)` runs the initialize handshake; the call methods mirror the
MCP capabilities. `callTool` auto-validates structured output against the tool's `outputSchema` using the
configured `jsonSchemaValidator`. `setRequestHandler` registers client-side responders (elicitation, sampling,
roots). `.experimental.tasks` adds streaming/long-running tool execution.

| [INDEX] | [SYMBOL]                                                            | [FAMILY] | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------ | :------- | :------------------------------------- |
|  [01]   | `Client`                                                            | class    | MCP client over a `Transport`          |
|  [02]   | `Client#connect`                                                    | method   | attach transport + initialize          |
|  [03]   | `Client#listTools / callTool`                                       | method   | discover + invoke server tools         |
|  [04]   | `Client#listResources / readResource / subscribeResource`           | method   | resource access + updates              |
|  [05]   | `Client#listPrompts / getPrompt`                                    | method   | server prompt access                   |
|  [06]   | `Client#complete`                                                   | method   | argument auto-completion               |
|  [07]   | `Client#getServerCapabilities / getServerVersion / getInstructions` | method   | post-init server facts                 |
|  [08]   | `Client#setRequestHandler`                                          | method   | client-side elicitation/sampling/roots |
|  [09]   | `Client#experimental.tasks`                                         | property | streaming/task tool execution          |
|  [10]   | `getSupportedElicitationModes`                                      | function | capability-derived elicitation modes   |

[CLIENT]: `Client(Implementation,ClientOptions?)` `Client.connect(Transport,RequestOptions?) -> Promise<void>` `Client.registerCapabilities(ClientCapabilities) -> void` `Client.getServerCapabilities() -> ServerCapabilities|undefined` `Client.getServerVersion() -> Implementation|undefined` `Client.getInstructions() -> string|undefined` `Client.listTools(ListToolsRequest["params"]?,RequestOptions?) -> Promise<ListToolsResult>` `Client.callTool(CallToolRequest["params"],typeof CallToolResultSchema?,RequestOptions?) -> Promise<CallToolResult>` `Client.listResources(unknown?,unknown?) -> Promise<ListResourcesResult>` `Client.listResourceTemplates(unknown?,unknown?) -> Promise<ListResourceTemplatesResult>` `Client.readResource(ReadResourceRequest["params"],unknown?) -> Promise<ReadResourceResult>` `Client.subscribeResource(unknown,unknown?) -> Promise<Result>` `Client.unsubscribeResource(unknown,unknown?) -> Promise<Result>` `Client.listPrompts(unknown?,unknown?) -> Promise<ListPromptsResult>` `Client.getPrompt(GetPromptRequest["params"],unknown?) -> Promise<GetPromptResult>` `Client.complete(CompleteRequest["params"],unknown?) -> Promise<CompleteResult>` `Client.setLoggingLevel(LoggingLevel,unknown?) -> Promise<Result>` `Client.ping(unknown?) -> Promise<Result>` `Client.sendRootsListChanged() -> Promise<void>` `Client.experimental: {tasks:ExperimentalClientTasks<RequestT,NotificationT,ResultT>}`
[CLIENT_OPTIONS]: `ClientOptions = ProtocolOptions&{…}`

## [03]-[TRANSPORTS]

[PUBLIC_TYPE_SCOPE]: pluggable client transports — rail: mcp-client

One `Transport` interface, four client implementations plus an in-memory pair for tests. `ai/tool.ts` picks by
server locality: `Stdio` spawns a local server process; `StreamableHTTP` (the current remote transport) POSTs +
SSE-streams with OAuth + reconnection + session resumption; `SSE` is the retired remote transport; `InMemory`
pairs a client and server in-process for kit-driven specs.

[TRANSPORT]: `Transport.start() -> Promise<void>` `Transport.send(JSONRPCMessage,TransportSendOptions?) -> Promise<void>` `Transport.close() -> Promise<void>` `Transport.onclose: ()=>void` `Transport.onerror: (error:Error)=>void` `Transport.onmessage: <T extends JSONRPCMessage>(message:T,extra?:MessageExtraInfo)=>void` `Transport.sessionId: string` `Transport.setProtocolVersion: (version:string)=>void`
[STDIO_CLIENT_TRANSPORT]: `StdioClientTransport(StdioServerParameters)`
[STREAMABLE_HTTPCLIENT_TRANSPORT]: `StreamableHTTPClientTransport(URL,StreamableHTTPClientTransportOptions?)` `StreamableHTTPClientTransport.sessionId: string|undefined` `StreamableHTTPClientTransport.terminateSession() -> Promise<void>` `StreamableHTTPClientTransport.finishAuth(string) -> Promise<void>` `StreamableHTTPClientTransport.setProtocolVersion(string) -> void`
[IN_MEMORY_TRANSPORT]: `InMemoryTransport.createLinkedPair() -> [InMemoryTransport,InMemoryTransport]`
[SURFACES]: `getDefaultEnvironment() -> Record<string,string>`

## [04]-[PROTOCOL_AND_AUTH]

[PUBLIC_TYPE_SCOPE]: request lifecycle + OAuth 2.0 client — rail: mcp-client

`Protocol` is the base every call rides: `RequestOptions` carries the timeout/cancellation/progress controls the
Effect boundary maps onto `Effect.timeout`/interruption. `client/auth` is the full OAuth flow for authenticating to
remote servers — the `StreamableHTTPClientTransport.authProvider` slot consumes an `OAuthClientProvider`, and on
401 the transport refreshes or redirects, throwing `UnauthorizedError` when interactive auth is required.

[SURFACES]: `DEFAULT_REQUEST_TIMEOUT_MSEC` `RequestOptions` `ProtocolOptions` `RequestHandlerExtra` `OAuthClientProvider` `auth` `discoverAuthorizationServerMetadata` `startAuthorization` `exchangeAuthorization` `refreshAuthorization` `registerClient` `UnauthorizedError`

## [05]-[WIRE_SCHEMAS]

[PUBLIC_TYPE_SCOPE]: the Zod protocol schemas — rail: mcp-client

`types.d.ts` is the full MCP wire as Zod schemas (325 exports) — every request/result/notification. The design
does not adopt Zod internally: it reads the inferred result types and re-parses each payload through `effect/Schema`
at the boundary. The tool-annotation hints are the seam onto native `Tool` annotations.

| [INDEX] | [SYMBOL]                      | [FAMILY]   | [CAPABILITY]                                                      |
| :-----: | :---------------------------- | :--------- | :---------------------------------------------------------------- |
|  [01]   | `CallToolResultSchema`        | Zod schema | tool result (content blocks + `structuredContent`/`isError`)      |
|  [02]   | `ListToolsResultSchema`       | Zod schema | tool roster (inputSchema/outputSchema/annotations)                |
|  [03]   | `ToolSchema` (annotations)    | Zod schema | `readOnlyHint`/`destructiveHint`/`idempotentHint`/`openWorldHint` |
|  [04]   | `ReadResourceResultSchema`    | Zod schema | resource contents (text/blob)                                     |
|  [05]   | `GetPromptResultSchema`       | Zod schema | prompt messages                                                   |
|  [06]   | `JSONRPCMessageSchema`        | Zod schema | transport frame                                                   |
|  [07]   | `ImplementationSchema`        | Zod schema | `Implementation` handshake fact                                   |
|  [08]   | `ClientCapabilitiesSchema`    | Zod schema | client capability handshake fact                                  |
|  [09]   | `ServerCapabilitiesSchema`    | Zod schema | server capability handshake fact                                  |
|  [10]   | `AjvJsonSchemaValidator`      | class      | default tool output-schema validator                              |
|  [11]   | `CfWorkerJsonSchemaValidator` | class      | edge/workers tool output-schema validator                         |

## [06]-[INTEGRATION]

[STACK]: `ai/tool.ts` external-server consumption — rail: mcp-client
- Own the connection as a `Scope`d resource: `acquireRelease(Effect.tryPromise(() => client.connect(transport)),
 () => Effect.promise(() => client.close()))` so a server process/HTTP session is released on interruption. Wrap
 every call in `Effect.tryPromise`, mapping `UnauthorizedError`/transport rejections into one typed `McpClientError`
 rail — no raw Promise escapes.
- `listTools` -> project each external tool into an app-toolkit row: its JSON-Schema `inputSchema` decodes to an
 `effect/Schema`, `callTool` becomes the handler, and the `readOnlyHint`/`destructiveHint`/`idempotentHint`/
 `openWorldHint` hints map onto `@effect/ai` `Tool.Readonly`/`Destructive`/`Idempotent`/`OpenWorld`. The native
 `LanguageModel` toolkit then treats an external MCP tool identically to a local one.
- `callTool`'s auto-validation stays useful, but the boundary re-parses `structuredContent` through the tool's own
 `effect/Schema` so downstream code composes native shapes, never Zod inferred types.

[STACK]: universal-tier rails — rail: mcp-client
- `effect`: `Scope`/`acquireRelease` own the transport lifecycle; `Effect.tryPromise` + `Match.tag` own the error
 rail; `Schema` re-parses results; `Effect.timeout`/interruption map onto `RequestOptions.timeout`/`signal`.
- `@effect/platform`: remote transports honor `fetch`/`requestInit` — thread the `net/client` policy `HttpClient`'s
 fetch so timeout/retry/proxy stay uniform; local `Stdio` servers spawn through `proc/exec`.
- `security`/`session`: the OAuth `authProvider` composes `security`'s runtime-neutral OAuth ceremony + browser
 token storage, so remote MCP auth reuses the one session lane rather than a second OAuth notion.
- `@effect/vitest`: `InMemoryTransport.createLinkedPair()` drives kit-driven specs that exercise the client against an
 in-process server with no network.

[BOUNDARY]: hosting stays native — rail: mcp-client
- The `./server` subpath (`McpServer`, `server/mcp`, `server/stdio`, `server/streamableHttp`, `server/completable`)
 is never imported. Rasm MCP hosting is `@effect/ai` `McpServer.toolkit` + `layerStdio`/`layerHttp` on `ai/tool.ts`;
 this SDK is strictly the outbound client that consumes other servers.
