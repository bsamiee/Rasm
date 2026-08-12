# [TS_RUNTIME_API_MODELCONTEXTPROTOCOL_SDK]

`@modelcontextprotocol/sdk` is the outbound MCP client `ai/tool.ts` drives to consume external servers: a `Client` over a pluggable `Transport`, the OAuth 2.0 client flow, and the Zod protocol wire. `ai/tool.ts` transcribes that Promise + Zod surface to `Effect` + `Schema` and leaves hosting native on `@effect/ai`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@modelcontextprotocol/sdk`
- package: `@modelcontextprotocol/sdk` (MIT)
- module: named subpaths `./client`, `./validation{,/ajv,/cfworker}`, `./experimental/tasks` beside a `./*` wildcard resolving every remaining module; `./server` never imported
- in-scope: `client/{index,stdio,streamableHttp,sse,websocket,auth}`, `shared/{protocol,transport,auth,stdio,mediaType}`, `types`, `inMemory`, `validation/*`, `experimental/tasks/client`
- runtime: node/bun for `Stdio` local spawn; isomorphic for HTTP and websocket transports
- rail: mcp-client

## [02]-[CLIENT]

[PUBLIC_TYPE_SCOPE]: the MCP client and its capability calls

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------ | :------------ | :-------------------------------------- |
|  [01]   | `Client`                                                            | class         | MCP client over a `Transport`           |
|  [02]   | `Client#connect`                                                    | method        | attach transport + initialize           |
|  [03]   | `Client#listTools / callTool`                                       | method        | discover + invoke server tools          |
|  [04]   | `Client#listResources / readResource / subscribeResource`           | method        | resource access + updates               |
|  [05]   | `Client#listPrompts / getPrompt`                                    | method        | server prompt access                    |
|  [06]   | `Client#complete`                                                   | method        | argument auto-completion                |
|  [07]   | `Client#getServerCapabilities / getServerVersion / getInstructions` | method        | post-init server facts                  |
|  [08]   | `Client#setRequestHandler`                                          | method        | client-side elicitation/sampling/roots  |
|  [09]   | `Client#setNotificationHandler / removeNotificationHandler`         | method        | one handler PER METHOD; second replaces |
|  [10]   | `Client#experimental.tasks`                                         | property      | streaming/task tool execution           |
|  [11]   | `getSupportedElicitationModes`                                      | function      | capability-derived elicitation modes    |

- `Client.callTool`: auto-validates `structuredContent` against the tool `outputSchema` through the configured `jsonSchemaValidator`.

[CLIENT_LIFECYCLE]: `Client(Implementation,ClientOptions?)` `Client.connect(Transport,RequestOptions?) -> Promise<void>` `Client.registerCapabilities(ClientCapabilities) -> void` (BEFORE `connect` — a capability registered after connect never reaches the initialize handshake) `Client.getServerCapabilities() -> ServerCapabilities|undefined` `Client.getServerVersion() -> Implementation|undefined` `Client.getInstructions() -> string|undefined` `Client.ping(unknown?) -> Promise<Result>`
[CLIENT_CALLS]: `Client.listTools(ListToolsRequest["params"]?,RequestOptions?) -> Promise<ListToolsResult>` `Client.callTool(CallToolRequest["params"],typeof CallToolResultSchema?,RequestOptions?) -> Promise<CallToolResult>` `Client.listPrompts(unknown?,unknown?) -> Promise<ListPromptsResult>` `Client.getPrompt(GetPromptRequest["params"],unknown?) -> Promise<GetPromptResult>` `Client.complete(CompleteRequest["params"],unknown?) -> Promise<CompleteResult>`
[CLIENT_RESOURCES]: `Client.listResources(unknown?,unknown?) -> Promise<ListResourcesResult>` `Client.listResourceTemplates(unknown?,unknown?) -> Promise<ListResourceTemplatesResult>` `Client.readResource(ReadResourceRequest["params"],unknown?) -> Promise<ReadResourceResult>` `Client.subscribeResource(unknown,unknown?) -> Promise<Result>` `Client.unsubscribeResource(unknown,unknown?) -> Promise<Result>`
[CLIENT_CONTROL]: `Client.setLoggingLevel(LoggingLevel,unknown?) -> Promise<Result>` `Client.sendRootsListChanged() -> Promise<void>` `Client.experimental: {tasks:ExperimentalClientTasks<RequestT,NotificationT,ResultT>}`
[CLIENT_IDLE]: verified members no fence composes yet — `client/middleware` (`withOAuth`/`withLogging` fetch-middleware composition; the branch's `Remote` lane owns its own budget and log rails, so declined with reason), `client/auth-extensions` `ClientCredentialsProvider` (a machine-to-machine `OAuthClientProvider` that enters through the http transport's existing `auth` slot as an app-passed value — no fence edit needed), `SSEClientTransport` (superseded by streamable HTTP; declined), `WebSocketClientTransport` (a third locality row the day a ws-only server earns it), `shared/uriTemplate` `UriTemplate.expand(variables)` (the template-expansion primitive `ai/tool`'s resource law names).
[CLIENT_OPTIONS]: `ClientOptions = ProtocolOptions&{…}`

## [03]-[TRANSPORTS]

[PUBLIC_TYPE_SCOPE]: one `Transport` interface, four client implementations, and an in-memory pair; `ai/tool.ts` selects by server locality — `Stdio` spawns a local process under a framing bound, `StreamableHTTP` POSTs and SSE-streams a remote server with OAuth, reconnection, and session resumption, `InMemory` pairs client and server in-process for specs.

[TRANSPORT]: `Transport.start() -> Promise<void>` `Transport.send(JSONRPCMessage,TransportSendOptions?) -> Promise<void>` `Transport.close() -> Promise<void>` `Transport.onclose: ()=>void` `Transport.onerror: (error:Error)=>void` `Transport.onmessage: <T extends JSONRPCMessage>(message:T,extra?:MessageExtraInfo)=>void` `Transport.sessionId: string` `Transport.setProtocolVersion: (version:string)=>void`
[STDIO_CLIENT_TRANSPORT]: `StdioClientTransport(StdioServerParameters)` `StdioServerParameters: {command,args?,env?,stderr?,cwd?,maxBufferSize?}`
[STREAMABLE_HTTPCLIENT_TRANSPORT]: `StreamableHTTPClientTransport(URL,StreamableHTTPClientTransportOptions?)` `StreamableHTTPClientTransport.sessionId: string|undefined` `StreamableHTTPClientTransport.terminateSession() -> Promise<void>` `StreamableHTTPClientTransport.finishAuth(string) -> Promise<void>` `StreamableHTTPClientTransport.setProtocolVersion(string) -> void`
[IN_MEMORY_TRANSPORT]: `InMemoryTransport.createLinkedPair() -> [InMemoryTransport,InMemoryTransport]`
[FRAMING]: `STDIO_DEFAULT_MAX_BUFFER_SIZE: number` `ReadBuffer({maxBufferSize?:number}?)` `ReadBuffer.append(Buffer) -> void` `ReadBuffer.readMessage() -> JSONRPCMessage|null` `mediaTypeEssence(string|null|undefined) -> string|undefined` `isJsonContentType(string|null|undefined) -> boolean`
[SURFACES]: `getDefaultEnvironment() -> Record<string,string>`

- `StdioServerParameters.maxBufferSize`: bounds one stdio read in bytes and falls back to `STDIO_DEFAULT_MAX_BUFFER_SIZE`; a message past the bound makes the transport emit an error and close rather than grow the buffer.
- `mediaTypeEssence`: parses the RFC 9110 media type — lowercased `type/subtype`, parameters stripped — and falls back to the segment before the first `;` on a malformed parameter section.

## [04]-[PROTOCOL_AND_AUTH]

[PUBLIC_TYPE_SCOPE]: the base `Protocol` every call rides and the OAuth 2.0 client authenticating to remote servers.

- `StreamableHTTPClientTransport.authProvider`: consumes an `OAuthClientProvider`; on 401 the transport refreshes or redirects, throwing `UnauthorizedError` when interactive auth is required.

[SURFACES]: `DEFAULT_REQUEST_TIMEOUT_MSEC` `RequestOptions` `ProtocolOptions` `RequestHandlerExtra` `OAuthClientProvider` `auth` `discoverAuthorizationServerMetadata` `startAuthorization` `exchangeAuthorization` `refreshAuthorization` `registerClient` `UnauthorizedError`

## [05]-[WIRE_SCHEMAS]

[PUBLIC_TYPE_SCOPE]: the MCP wire — `types` Zod schemas over every request, result, and notification; the boundary reads their inferred result types and re-parses each payload through `effect/Schema`.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :---------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `CallToolResultSchema`        | Zod schema    | tool result (content blocks + `structuredContent`/`isError`)      |
|  [02]   | `ListToolsResultSchema`       | Zod schema    | tool roster (inputSchema/outputSchema/annotations)                |
|  [03]   | `ToolSchema` (annotations)    | Zod schema    | `readOnlyHint`/`destructiveHint`/`idempotentHint`/`openWorldHint` |
|  [04]   | `ReadResourceResultSchema`    | Zod schema    | resource contents (text/blob)                                     |
|  [05]   | `GetPromptResultSchema`       | Zod schema    | prompt messages                                                   |
|  [06]   | `JSONRPCMessageSchema`        | Zod schema    | transport frame                                                   |
|  [07]   | `ImplementationSchema`        | Zod schema    | `Implementation` handshake fact                                   |
|  [08]   | `ClientCapabilitiesSchema`    | Zod schema    | client capability handshake fact                                  |
|  [09]   | `ServerCapabilitiesSchema`    | Zod schema    | server capability handshake fact                                  |
|  [10]   | `AjvJsonSchemaValidator`      | class         | default tool output-schema validator                              |
|  [11]   | `CfWorkerJsonSchemaValidator` | class         | edge/workers tool output-schema validator                         |

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ai/tool.ts` transcribes the Promise + Zod surface to Effect + Schema: every call wraps `Effect.tryPromise`, the transport rides a `Scope`, and each result re-parses through `effect/Schema` — no raw Promise or Zod inferred type crosses the seam.
- Stdio framing is bounded material: `ReadBuffer` holds an unterminated line whole, so `maxBufferSize` is what stops a local server flooding stdout from growing the client's heap without limit; the bound rides the dial spec beside the environment allow-list, one hardening decision per spawned server.
- `isJsonContentType`/`mediaTypeEssence` own every `Content-Type` decision, foreclosing a substring probe over the raw header — `text/plain; a=application/json` carries the JSON spelling while its media type is `text/plain`, so a substring test admits false positives and, past a `charset` parameter or a case variant, false negatives.

[STACKING]:
- `@effect/ai`(`.api/effect-ai.md`): `listTools` rows project into an app toolkit — `inputSchema` decodes to `effect/Schema`, `callTool` becomes the handler, `readOnlyHint`/`destructiveHint`/`idempotentHint`/`openWorldHint` map onto `Tool.Readonly`/`Destructive`/`Idempotent`/`OpenWorld`, and `LanguageModel` treats the external tool as a local one.
- `effect`(`.api/effect.md`): `acquireRelease` owns transport lifetime with `close()` as release; `Effect.tryPromise` + `Match.tag` fold `UnauthorizedError` and transport rejections into one `McpClientError` rail; `Effect.timeout`/interruption map onto `RequestOptions.timeout`/`signal`.
- `@effect/platform`(`.api/effect-platform.md`): HTTP and websocket transports honor the `net/client` policy `HttpClient` fetch so timeout, retry, and proxy stay uniform; `Stdio` servers spawn through `proc/exec`.
- `security`/`session`: the OAuth `authProvider` composes `security`'s runtime-neutral OAuth ceremony and browser token storage, reusing the one session lane; `@effect/vitest` drives specs over `InMemoryTransport.createLinkedPair()` with no network.
- `ai/tool` (within-lib): `Remote.dial`'s stdio arm hands `command`, `args`, `getDefaultEnvironment()`, and the spec's `maxBufferSize` into `StdioClientTransport` as one hardening row, its HTTP arm hands `authProvider` into `StreamableHTTPClientTransport`, and `_bounded` folds `Fault.Budget` onto `RequestOptions.timeout`/`maxTotalTimeout` so every crossing is priced and bounded at the same seam.

[LOCAL_ADMISSION]:
- Consume external servers only — the `./server` subpath is never imported; Rasm hosts MCP on `@effect/ai` `McpServer.toolkit` + `layerStdio`/`layerHttp`.
- Own every connection as a `Scope`d resource releasing the process or HTTP session on interruption; re-parse `structuredContent` through the tool's own `effect/Schema`, never a Zod inferred type.
- Every spawned server states its own stdio framing bound as a dial-spec value, so a hostile or runaway local process is bounded by policy rather than by the package default.

[RAIL_LAW]:
- Package: `@modelcontextprotocol/sdk`
- Owns: the outbound MCP client — capability calls, pluggable transports, the stdio framing bound, media-type parsing, protocol lifecycle, OAuth 2.0 client flow, the Zod wire schemas
- Accept: `Effect.tryPromise`-wrapped calls under a `Scope`d transport, a policy-stated `maxBufferSize` on every spawned server, `listTools` projected into the `@effect/ai` toolkit, results re-parsed through `effect/Schema`, hints mapped onto native `Tool` annotations
- Reject: raw Promise or Zod escaping the seam, an unbounded stdio read buffer, a substring probe over a raw `Content-Type` header, the `./server` hosting subpath, a second OAuth notion beside the session lane
