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
- package: `@modelcontextprotocol/sdk` (version ``, license MIT, `type: module`)
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

```ts contract
declare class Client<RequestT extends Request = Request, NotificationT extends Notification = Notification, ResultT extends Result = Result>
  extends Protocol<ClientRequest | RequestT, ClientNotification | NotificationT, ClientResult | ResultT> {
  constructor(clientInfo: Implementation, options?: ClientOptions)
  connect(transport: Transport, options?: RequestOptions): Promise<void>
  registerCapabilities(capabilities: ClientCapabilities): void
  getServerCapabilities(): ServerCapabilities | undefined
  getServerVersion(): Implementation | undefined
  getInstructions(): string | undefined
  listTools(params?: ListToolsRequest["params"], options?: RequestOptions): Promise<ListToolsResult>
  callTool(params: CallToolRequest["params"], resultSchema?: typeof CallToolResultSchema, options?: RequestOptions): Promise<CallToolResult>
  listResources(params?, options?): Promise<ListResourcesResult>
  listResourceTemplates(params?, options?): Promise<ListResourceTemplatesResult>
  readResource(params: ReadResourceRequest["params"], options?): Promise<ReadResourceResult>
  subscribeResource(params, options?): Promise<Result>; unsubscribeResource(params, options?): Promise<Result>
  listPrompts(params?, options?): Promise<ListPromptsResult>
  getPrompt(params: GetPromptRequest["params"], options?): Promise<GetPromptResult>
  complete(params: CompleteRequest["params"], options?): Promise<CompleteResult>
  setLoggingLevel(level: LoggingLevel, options?): Promise<Result>; ping(options?): Promise<Result>
  sendRootsListChanged(): Promise<void>
  get experimental(): { tasks: ExperimentalClientTasks<RequestT, NotificationT, ResultT> }
}
type ClientOptions = ProtocolOptions & {
  capabilities?: ClientCapabilities
  jsonSchemaValidator?: jsonSchemaValidator // default AjvJsonSchemaValidator; CfWorkerJsonSchemaValidator for edge/workers
  listChanged?: ListChangedHandlers          // onChanged callbacks for tools/prompts/resources
}
```

## [03]-[TRANSPORTS]

[PUBLIC_TYPE_SCOPE]: pluggable client transports — rail: mcp-client

One `Transport` interface, four client implementations plus an in-memory pair for tests. `ai/tool.ts` picks by
server locality: `Stdio` spawns a local server process; `StreamableHTTP` (the current remote transport) POSTs +
SSE-streams with OAuth + reconnection + session resumption; `SSE` is the retired remote transport; `InMemory`
pairs a client and server in-process for kit-driven specs.

```ts contract
interface Transport {
  start(): Promise<void>                        // Client.connect() calls this implicitly
  send(message: JSONRPCMessage, options?: TransportSendOptions): Promise<void>
  close(): Promise<void>
  onclose?: () => void; onerror?: (error: Error) => void
  onmessage?: <T extends JSONRPCMessage>(message: T, extra?: MessageExtraInfo) => void
  sessionId?: string; setProtocolVersion?: (version: string) => void
}
declare class StdioClientTransport implements Transport { constructor(server: StdioServerParameters) /* command,args,env,cwd,stderr */ }
declare function getDefaultEnvironment(): Record<string, string> // safe env allowlist for spawned servers
declare class StreamableHTTPClientTransport implements Transport {
  constructor(url: URL, opts?: StreamableHTTPClientTransportOptions) // { authProvider?; requestInit?; fetch?; reconnectionOptions?; sessionId? }
  get sessionId(): string | undefined
  terminateSession(): Promise<void>; finishAuth(authorizationCode: string): Promise<void>; setProtocolVersion(v: string): void
}
declare class SSEClientTransport implements Transport { /* legacy remote transport */ }
declare class WebSocketClientTransport implements Transport {}
declare class InMemoryTransport implements Transport { static createLinkedPair(): [InMemoryTransport, InMemoryTransport] }
```

## [04]-[PROTOCOL_AND_AUTH]

[PUBLIC_TYPE_SCOPE]: request lifecycle + OAuth 2.0 client — rail: mcp-client

`Protocol` is the base every call rides: `RequestOptions` carries the timeout/cancellation/progress controls the
Effect boundary maps onto `Effect.timeout`/interruption. `client/auth` is the full OAuth flow for authenticating to
remote servers — the `StreamableHTTPClientTransport.authProvider` slot consumes an `OAuthClientProvider`, and on
401 the transport refreshes or redirects, throwing `UnauthorizedError` when interactive auth is required.

```ts contract
const DEFAULT_REQUEST_TIMEOUT_MSEC = 60000
type RequestOptions = {
  onprogress?: ProgressCallback; signal?: AbortSignal
  timeout?: number; resetTimeoutOnProgress?: boolean; maxTotalTimeout?: number
}
interface ProtocolOptions { /* enforceStrictCapabilities?, debouncedNotificationMethods?, … */ }
interface RequestHandlerExtra<SendRequestT, SendNotificationT> { /* signal, sessionId, sendNotification, sendRequest, authInfo, … */ }

// OAuth 2.0 (client/auth): interface + flow functions the transport authProvider composes.
interface OAuthClientProvider { readonly redirectUrl; readonly clientMetadata; tokens(); saveTokens(); redirectToAuthorization(); /* … */ }
declare function auth(provider: OAuthClientProvider, options): Promise<AuthResult>
declare function discoverAuthorizationServerMetadata(...): Promise<...>
declare function startAuthorization(...): Promise<...>; declare function exchangeAuthorization(...): Promise<...>
declare function refreshAuthorization(...): Promise<...>; declare function registerClient(...): Promise<...>
declare class UnauthorizedError extends Error {}
```

## [05]-[WIRE_SCHEMAS]

[PUBLIC_TYPE_SCOPE]: the Zod protocol schemas — rail: mcp-client

`types.d.ts` is the full MCP wire as Zod schemas (325 exports) — every request/result/notification. The design
does not adopt Zod internally: it reads the inferred result types and re-parses each payload through `effect/Schema`
at the boundary. The tool-annotation hints are the seam onto native `Tool` annotations.

| [INDEX] | [SYMBOL]                                                                         | [FAMILY]   | [CAPABILITY]                                                      |
| :-----: | :------------------------------------------------------------------------------- | :--------- | :---------------------------------------------------------------- |
|  [01]   | `CallToolResultSchema`                                                           | Zod schema | tool result (content blocks + `structuredContent`/`isError`)      |
|  [02]   | `ListToolsResultSchema`                                                          | Zod schema | tool roster (inputSchema/outputSchema/annotations)                |
|  [03]   | `ToolSchema` (annotations)                                                       | Zod schema | `readOnlyHint`/`destructiveHint`/`idempotentHint`/`openWorldHint` |
|  [04]   | `ReadResourceResultSchema`                                                       | Zod schema | resource contents (text/blob)                                     |
|  [05]   | `GetPromptResultSchema`                                                          | Zod schema | prompt messages                                                   |
|  [06]   | `JSONRPCMessageSchema`                                                           | Zod schema | transport frame                                                   |
|  [07]   | `ImplementationSchema` / `ClientCapabilitiesSchema` / `ServerCapabilitiesSchema` | Zod schema | handshake facts                                                   |
|  [08]   | `AjvJsonSchemaValidator` / `CfWorkerJsonSchemaValidator`                         | class      | tool output-schema validators                                     |

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
