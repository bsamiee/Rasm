# [RASM_APPHOST_API_MCP]

`ModelContextProtocol.Core` supplies the session, client, server, transport, and primitive surfaces for the MCP protocol; `ModelContextProtocol` provides DI composition, builder extensions, and hosted-service plumbing; `ModelContextProtocol.AspNetCore` adds HTTP transport, SSE event-stream persistence, and authentication handler registration for ASP.NET Core hosts.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ModelContextProtocol.Core`
- package: `ModelContextProtocol.Core`
- assembly: `ModelContextProtocol`
- namespace: `ModelContextProtocol`, `ModelContextProtocol.Client`, `ModelContextProtocol.Server`, `ModelContextProtocol.Protocol`
- asset: runtime library
- rail: mcp-protocol

[PACKAGE_SURFACE]: `ModelContextProtocol`
- package: `ModelContextProtocol`
- assembly: `ModelContextProtocol`
- namespace: `ModelContextProtocol`, `ModelContextProtocol.Server`, `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: mcp-host

[PACKAGE_SURFACE]: `ModelContextProtocol.AspNetCore`
- package: `ModelContextProtocol.AspNetCore`
- assembly: `ModelContextProtocol.AspNetCore`
- namespace: `ModelContextProtocol.AspNetCore`, `ModelContextProtocol.AspNetCore.Authentication`, `Microsoft.AspNetCore.Builder`, `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: mcp-host

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: session and protocol primitives — `ModelContextProtocol.Core`
- rail: mcp-protocol

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]       | [CAPABILITY]                                                                            |
| :-----: | :-------------------------- | :------------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `McpSession`                | abstract base class | shared session lifecycle for client+server                                              |
|  [02]   | `McpServer`                 | abstract class      | server-side session, extends `McpSession`                                               |
|  [03]   | `McpClient`                 | abstract class      | client-side session, extends `McpSession`                                               |
|  [04]   | `McpException`              | exception           | typed protocol failure                                                                  |
|  [05]   | `McpErrorCode`              | enum                | JSON-RPC error code vocabulary                                                          |
|  [06]   | `RequestOptions`            | options class       | per-request timeout and cancellation                                                    |
|  [07]   | `IMcpTaskStore`             | interface           | task-result persistence contract                                                        |
|  [08]   | `InMemoryMcpTaskStore`      | class               | in-process `IMcpTaskStore` implementation                                               |
|  [09]   | `McpJsonUtilities`          | static class        | `JsonSerializerOptions` and type-info                                                   |
|  [10]   | `AIContentExtensions`       | static class        | `AIContent` ↔ MCP content conversions                                                   |
|  [11]   | `NullProgress`              | class               | no-op `IProgress<T>` implementation                                                     |
|  [12]   | `ProgressNotificationValue` | sealed class        | progress payload: `required float Progress`, `float? Total`, `string? Message` (`init`) |
|  [13]   | `UriTemplate`               | class               | RFC 6570 URI template evaluation                                                        |
|  [14]   | `Implementation`            | sealed class        | `ModelContextProtocol.Protocol` identity: `required string Name`, `required string Version`, `Title` — the `McpClientOptions.ClientInfo`/`McpServerOptions.ServerInfo` value |

[PUBLIC_TYPE_SCOPE]: server primitives — `ModelContextProtocol.Server`
- rail: mcp-protocol

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]    | [CAPABILITY]                                          |
| :-----: | :--------------------------- | :--------------- | :---------------------------------------------------- |
|  [01]   | `McpServerTool`              | abstract class   | tool primitive base, implements `IMcpServerPrimitive` |
|  [02]   | `McpServerPrompt`            | abstract class   | prompt primitive base                                 |
|  [03]   | `McpServerResource`          | abstract class   | resource primitive base                               |
|  [04]   | `AIFunctionMcpServerTool`    | sealed class     | `AIFunction`-backed tool adapter                      |
|  [05]   | `DelegatingMcpServerTool`    | class            | delegate-wrapping tool                                |
|  [06]   | `McpServerToolCreateOptions` | sealed class     | tool create policy bag                                |
|  [07]   | `McpServerOptions`           | sealed class     | server configuration root                             |
|  [08]   | `McpServerHandlers`          | class            | server request-handler registry                       |
|  [09]   | `McpServerFilters`           | class            | server message and request filter registry            |
|  [10]   | `McpServerToolAttribute`     | sealed attribute | marks a method as an MCP tool                         |
|  [11]   | `McpMetaAttribute`           | sealed attribute | attaches metadata to server primitives                |
|  [12]   | `StdioServerTransport`       | class            | stdio-backed server transport                         |
|  [13]   | `StreamServerTransport`      | class            | stream-backed server transport                        |

[TOOL_CREATE_OPTIONS]:
- Identity: `Name`, `Title`, and `Description`.
- Safety annotations: `ReadOnly`, `Destructive`, `Idempotent`, and `OpenWorld`.
- Structured output: `UseStructuredContent` and `OutputSchema`.
- Serializer and schema policy: `SerializerOptions` and `SchemaCreateOptions`.
- Metadata: `Metadata`.
- Services: `Services`.

[PUBLIC_TYPE_SCOPE]: client primitives — `ModelContextProtocol.Client`
- rail: mcp-protocol

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                                                                                                                                                                                    |
| :-----: | :------------------------------- | :------------ | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `McpClientTool`                  | sealed class  | extends `AIFunction`; wraps a server tool                                                                                                                                                                       |
|  [02]   | `McpClientPrompt`                | sealed class  | client-side prompt accessor                                                                                                                                                                                     |
|  [03]   | `McpClientResource`              | sealed class  | client-side resource accessor                                                                                                                                                                                   |
|  [04]   | `McpClientResourceTemplate`      | sealed class  | client-side resource template accessor                                                                                                                                                                          |
|  [05]   | `McpClientOptions`               | class         | client configuration root                                                                                                                                                                                       |
|  [06]   | `McpClientHandlers`              | class         | client notification handler registry                                                                                                                                                                            |
|  [07]   | `IClientTransport`               | interface     | session-transport factory contract                                                                                                                                                                              |
|  [08]   | `HttpClientTransport`            | sealed class  | `IClientTransport`; HTTP transport selecting streamable-vs-SSE by `HttpClientTransportOptions.TransportMode`                                                                                                    |
|  [09]   | `StdioClientTransport`           | sealed class  | `IClientTransport`; stdio client transport                                                                                                                                                                      |
|  [10]   | `StreamClientTransport`          | sealed class  | `IClientTransport`; paired-stream client transport (the third public implementor; declared in the `ModelContextProtocol.Protocol` namespace, ctor `(Stream serverInput, Stream serverOutput, ILoggerFactory?)`) |
|  [11]   | `StdioClientTransportOptions`    | class         | stdio transport configuration (`required string Command`, `Arguments`, `Name`)                                                                                                                                  |
|  [12]   | `HttpClientTransportOptions`     | class         | HTTP transport configuration (`required Uri Endpoint`, `HttpTransportMode TransportMode`, `Name`)                                                                                                               |
|  [13]   | `HttpTransportMode`              | enum          | `AutoDetect`/`StreamableHttp`/`Sse` — `HttpClientTransport` connect-time mode selecting the SDK-internal streamable/SSE session transport                                                                       |
|  [14]   | `ClientTransportClosedException` | class         | transport-closed typed failure                                                                                                                                                                                  |

[PUBLIC_TYPE_SCOPE]: DI and builder — `Microsoft.Extensions.DependencyInjection` (in `ModelContextProtocol`)
- rail: mcp-host

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :---------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `IMcpServerBuilder`                 | interface     | server builder contract                            |
|  [02]   | `IMcpMessageFilterBuilder`          | interface     | message-filter builder contract                    |
|  [03]   | `IMcpRequestFilterBuilder`          | interface     | request-filter builder contract                    |
|  [04]   | `McpServerBuilderExtensions`        | static class  | `WithTools`, `WithPrompts`, `WithResources` fluent |
|  [05]   | `McpMessageFilterBuilderExtensions` | static class  | `AddIncomingFilter`, `AddOutgoingFilter`           |
|  [06]   | `McpRequestFilterBuilderExtensions` | static class  | per-operation filter registration                  |

[PUBLIC_TYPE_SCOPE]: ASP.NET Core host — `ModelContextProtocol.AspNetCore`
- rail: mcp-host

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :---------------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `HttpServerTransportOptions`        | class         | HTTP server transport configuration            |
|  [02]   | `ISessionMigrationHandler`          | interface     | session migration between instances contract   |
|  [03]   | `McpEndpointRouteBuilderExtensions` | static class  | `MapMcp(pattern)` route registration           |
|  [04]   | `HttpMcpServerBuilderExtensions`    | static class  | `WithHttpTransport`, `AddAuthorizationFilters` |
|  [05]   | `McpAuthenticationExtensions`       | static class  | `AddMcp` authentication scheme builder         |
|  [06]   | `McpAuthenticationDefaults`         | static class  | scheme name constants                          |
|  [07]   | `McpAuthenticationOptions`          | class         | extends `AuthenticationSchemeOptions`          |
|  [08]   | `McpAuthenticationHandler`          | class         | `AuthenticationHandler` implementation         |
|  [09]   | `ResourceMetadataRequestContext`    | class         | resource metadata request context              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: server registration
- rail: mcp-host

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY]  | [CAPABILITY]                                                                  |
| :-----: | :-------------------------------------------------------------------- | :-------------- | :---------------------------------------------------------------------------- |
|  [01]   | `AddMcpServer(this IServiceCollection)`                               | DI registration | registers server, returns `IMcpServerBuilder`                                 |
|  [02]   | `WithTools<TToolType>(this IMcpServerBuilder)`                        | builder fluent  | discovers `[McpServerTool]` on type                                           |
|  [03]   | `WithTools(this IMcpServerBuilder, IEnumerable<McpServerTool>)`       | builder fluent  | registers a programmatic tool set                                             |
|  [04]   | `WithPrompts<TPromptType>(this IMcpServerBuilder)`                    | builder fluent  | discovers prompts on type                                                     |
|  [05]   | `WithResources<TResourceType>(this IMcpServerBuilder)`                | builder fluent  | discovers resources on type                                                   |
|  [06]   | `WithToolsFromAssembly(this IMcpServerBuilder, Assembly?)`            | builder fluent  | assembly-scan tool discovery                                                  |
|  [07]   | `WithListToolsHandler(this IMcpServerBuilder, McpRequestHandler<..>)` | builder fluent  | registers explicit list-tools handler                                         |
|  [08]   | `WithCallToolHandler(this IMcpServerBuilder, McpRequestHandler<..>)`  | builder fluent  | registers explicit call-tool handler                                          |
|  [09]   | `WithHttpTransport(this IMcpServerBuilder, ...)`                      | builder fluent  | attaches HTTP/SSE server transport                                            |
|  [10]   | `MapMcp(this IEndpointRouteBuilder, pattern)`                         | routing         | maps MCP endpoint at given route pattern                                      |
|  [11]   | `AddMcp(this AuthenticationBuilder, ...)`                             | auth            | registers MCP authentication scheme                                           |
|  [12]   | `McpServerTool.Create(AIFunction, McpServerToolCreateOptions?)`       | static factory  | adopts an `AIFunction` as a programmatic tool                                 |
|  [13]   | `McpServerTool.Create(Delegate, McpServerToolCreateOptions?)`         | static factory  | builds a tool from a delegate                                                 |
|  [14]   | `WithStdioServerTransport(this IMcpServerBuilder)`                    | builder fluent  | mounts the stdio transport (host `ModelContextProtocol` package, NOT `.Core`) |

[ENTRYPOINT_SCOPE]: client construction
- rail: mcp-protocol

| [INDEX] | [SURFACE]                                                                                                                                                                                                        | [ENTRY_FAMILY] | [CAPABILITY]                                                                                                                                                                                                                                                           |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `McpClient.CreateAsync(IClientTransport, McpClientOptions?, ILoggerFactory?, CT)`                                                                                                                                | static factory | constructs and initializes `McpClient`                                                                                                                                                                                                                                 |
|  [02]   | `McpClient.ResumeSessionAsync(IClientTransport, ResumeClientSessionOptions, ...)`                                                                                                                                | static factory | resumes a detached session                                                                                                                                                                                                                                             |
|  [03]   | `McpClient.ListToolsAsync(RequestOptions?, CT)`                                                                                                                                                                  | session call   | enumerates server tools                                                                                                                                                                                                                                                |
|  [04]   | `McpClient.CallToolAsync(string toolName, IReadOnlyDictionary<string,object?>? args, IProgress<ProgressNotificationValue>? progress, RequestOptions?, CT)`                                                       | session call   | invokes named server tool; returns `ValueTask<CallToolResult>`. The `IProgress` positional precedes `RequestOptions?`, so callers pass `progress`/`options`/`cancellationToken` by name                                                                                |
|  [05]   | `McpClient.ListPromptsAsync(RequestOptions?, CT)`                                                                                                                                                                | session call   | enumerates server prompts                                                                                                                                                                                                                                              |
|  [06]   | `McpClient.GetPromptAsync(name, args, RequestOptions?)`                                                                                                                                                          | session call   | retrieves prompt content                                                                                                                                                                                                                                               |
|  [07]   | `McpClient.ListResourcesAsync(RequestOptions?, CT)`                                                                                                                                                              | session call   | enumerates server resources                                                                                                                                                                                                                                            |
|  [08]   | `McpClient.SubscribeToResourceAsync(string uri, RequestOptions?, CT)` / `SubscribeToResourceAsync(string uri, Func<ResourceUpdatedNotificationParams,CancellationToken,ValueTask> handler, RequestOptions?, CT)` | session call   | subscribes to resource update events; the base overload returns `Task`, the per-uri handler overload registers the update handler at subscribe and returns `Task<IAsyncDisposable>` (`handler` precedes `RequestOptions?`, pass `options`/`cancellationToken` by name) |
|  [09]   | `McpClient.ListResourceTemplatesAsync(RequestOptions?, CT)`                                                                                                                                                      | session call   | enumerates server resource templates; returns `ValueTask<IList<McpClientResourceTemplate>>` (the `McpClientResourceTemplate` type [4] carries `UriTemplate`)                                                                                                           |
|  [10]   | `McpClientTool.InvokeAsync(args, CT)`                                                                                                                                                                            | tool call      | `AIFunction` contract invocation                                                                                                                                                                                                                                       |
|  [11]   | `McpClientOptions.ClientInfo` (set in the `CreateAsync` options bag)                                                                                                                                             | option property | `Implementation?` client identity (`Name`/`Version`) advertised at the initialize handshake                                                                                                                                                                            |

[ENTRYPOINT_SCOPE]: server options surface
- rail: mcp-protocol

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY]  | [CAPABILITY]                                                                                              |
| :-----: | :--------------------------------------------- | :-------------- | :-------------------------------------------------------------------------------------------------------- |
|  [01]   | `McpServerOptions.ServerInfo`                  | option property | `Implementation` name and version info                                                                    |
|  [02]   | `McpServerOptions.Capabilities`                | option property | `ServerCapabilities` advertised to clients                                                                |
|  [03]   | `McpServerOptions.ProtocolVersion`             | option property | date-versioned protocol string                                                                            |
|  [04]   | `McpServerOptions.InitializationTimeout`       | option property | handshake timeout; default 60 s                                                                           |
|  [05]   | `McpServerOptions.ServerInstructions`          | option property | system instructions delivered to clients                                                                  |
|  [06]   | `McpServerOptions.ToolCollection`              | option property | `McpServerPrimitiveCollection<McpServerTool>`                                                             |
|  [07]   | `McpServerOptions.PromptCollection`            | option property | `McpServerPrimitiveCollection<McpServerPrompt>`                                                           |
|  [08]   | `McpServerOptions.ResourceCollection`          | option property | `McpServerResourceCollection`                                                                             |
|  [09]   | `McpServerOptions.TaskStore`                   | option property | `IMcpTaskStore?` backing out-of-band task results; default `InMemoryMcpTaskStore` when polling is enabled |
|  [10]   | `McpServerOptions.SendTaskStatusNotifications` | option property | `bool` — emit `notifications/tasks/status` as a long-running task advances                                |

[ENTRYPOINT_SCOPE]: server session long-running verbs — `ModelContextProtocol.Server` (`McpServer`)
- rail: mcp-protocol

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY] | [CAPABILITY]                                                                                                                             |
| :-----: | :------------------------------------------------------------------------------------------ | :------------- | :--------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `McpServer.SampleAsync(CreateMessageRequestParams, CT)`                                     | server→client  | server-initiated LLM sampling; returns `ValueTask<CreateMessageResult>`                                                                  |
|  [02]   | `McpServer.SampleAsync(IEnumerable<ChatMessage>, ChatOptions?, JsonSerializerOptions?, CT)` | server→client  | `IChatClient`-shaped sampling overload; returns `Task<ChatResponse>`                                                                     |
|  [03]   | `McpServer.AsSamplingChatClient(JsonSerializerOptions?)`                                    | bridge         | adapts server sampling to an `IChatClient`; returns `IChatClient`                                                                        |
|  [04]   | `McpServer.ElicitAsync(ElicitRequestParams, CT)`                                            | server→client  | structured mid-call input request; returns `ValueTask<ElicitResult>`                                                                     |
|  [05]   | `McpServer.ElicitAsync<T>(string message, RequestOptions?, CT)`                             | server→client  | typed elicitation; returns `ValueTask<ElicitResult<T>>` bound to a generated schema                                                      |
|  [06]   | `McpServer.GetTaskAsync(string taskId, CT)`                                                 | task poll      | returns `ValueTask<McpTask>` carrying current task status (also `GetTaskAsync(string, string? sessionId, CT)` → `Task<McpTask?>`)        |
|  [07]   | `McpServer.GetTaskResultAsync<T>(string taskId, JsonSerializerOptions?, CT)`                | task poll      | returns `ValueTask<T?>` decoding the stored task-result payload                                                                          |
|  [08]   | `McpServer.WaitForTaskResultAsync<T>(string taskId, JsonSerializerOptions?, CT)`            | task await     | awaits terminal status; returns `ValueTask<(McpTask Task, T? Result)>`                                                                   |
|  [09]   | `McpServer.PollTaskUntilCompleteAsync(string taskId, CT)`                                   | task poll      | drives the poll loop to a terminal status; returns `ValueTask<McpTask>` (also `PollTaskUntilCompleteAsync(string, RequestOptions?, CT)`) |

[PUBLIC_TYPE_SCOPE]: request-context surface — `ModelContextProtocol.Server`
- rail: mcp-protocol

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                                                                                                                                          |
| :-----: | :------------------ | :------------ | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `MessageContext`    | base class    | declares `.Server` (`McpServer`), `.Services` (`IServiceProvider?`), `.User` (`ClaimsPrincipal?`), `.Items` (`IDictionary<object,object>?`)                                           |
|  [02]   | `RequestContext<T>` | sealed class  | `MessageContext` inheritor adding `.Params` (`T?`), `.MatchedPrimitive`, `.JsonRpcRequest`, and `EnablePollingAsync(TimeSpan, CT)`; constructed from `(McpServer, JsonRpcRequest, T)` |

## [04]-[IMPLEMENTATION_LAW]

[SESSION_TOPOLOGY]:
- namespace roots: `ModelContextProtocol`, `ModelContextProtocol.Client`, `ModelContextProtocol.Server`
- session base: `McpSession : IAsyncDisposable`; both `McpServer` and `McpClient` extend it
- tool integration: `McpClientTool : AIFunction` — client-side tools surface as `AIFunction` instances
- primitive base: `McpServerTool`, `McpServerPrompt`, `McpServerResource` all implement `IMcpServerPrimitive`
- attribute discovery: `[McpServerTool]` on public methods drives reflection-based tool registration
- task persistence: `IMcpTaskStore` / `InMemoryMcpTaskStore` back the out-of-band task-result protocol; `McpServerOptions.TaskStore` wires the store and `SendTaskStatusNotifications` toggles `notifications/tasks/status`
- request context: `MessageContext` is the declaring owner of `.Server`/`.Services`/`.User`/`.Items`; `RequestContext<T> : MessageContext` adds `.Params`/`.MatchedPrimitive`/`.JsonRpcRequest`/`EnablePollingAsync(TimeSpan, CT)` and arrives through the `(McpServer, JsonRpcRequest, T)` ctor — never read `.Server` as a direct `RequestContext<T>` property, it is inherited from `MessageContext`
- server long-running verbs: `McpServer.SampleAsync` / `AsSamplingChatClient` / `ElicitAsync` / `ElicitAsync<T>` / `GetTaskAsync` / `GetTaskResultAsync<T>` / `WaitForTaskResultAsync<T>` / `PollTaskUntilCompleteAsync` are `McpServer` session members; `AsSamplingChatClient` is the SDK server-sampling-to-`IChatClient` bridge declared on `McpServer` (host/Core), NOT an `Microsoft.Extensions.AI` member, and is filed here, never in `api-extensions-ai.md`
- stdio transport attribution: `WithStdioServerTransport(IMcpServerBuilder)` is declared on `Microsoft.Extensions.DependencyInjection.McpServerBuilderExtensions` in the **host `ModelContextProtocol`** package beside `WithTools`/`WithPrompts`/`WithResources`, NOT in `ModelContextProtocol.Core`; the `StdioServerTransport`/`StreamServerTransport` types are Core, the builder shortcut is host-package

[LOCAL_ADMISSION]:
- DI registration entry: `services.AddMcpServer()` returns `IMcpServerBuilder`; `WithHttpTransport()` attaches HTTP/SSE
- HTTP host entry: `MapMcp(pattern)` registers the endpoint; default pattern is `""`
- Auth extension: `authBuilder.AddMcp()` registers scheme; `McpAuthenticationHandler` implements token exchange
- Client construction: `McpClient.CreateAsync(IClientTransport, McpClientOptions?, ILoggerFactory?, CT)` is the sole construction point; `ResumeSessionAsync` resumes a detached session
- JSON options: `McpJsonUtilities.DefaultOptions` carries the canonical `JsonSerializerOptions`; use it at boundaries
- long-running task admission: `McpServerOptions.TaskStore` + `SendTaskStatusNotifications` arm the out-of-band task protocol; a tool method opts a request into polling through `RequestContext<T>.EnablePollingAsync(interval, ct)`, and a client drives the result through `McpServer.GetTaskAsync`/`WaitForTaskResultAsync<T>`/`PollTaskUntilCompleteAsync`
- server-initiated legs: `SampleAsync` (server→client LLM sampling) and `ElicitAsync`/`ElicitAsync<T>` (mid-call structured input) require a stateful session — over HTTP they force `HttpServerTransportOptions.Stateless = false`; `AsSamplingChatClient(JsonSerializerOptions?)` projects the sampling leg as an `IChatClient` the in-process reasoning loop reuses

[RAIL_LAW]:
- Package: `ModelContextProtocol.Core` + `ModelContextProtocol` + `ModelContextProtocol.AspNetCore`
- Owns: MCP session, server primitives, client tools, transport selection, DI registration, HTTP hosting, the server long-running task/sampling/elicitation session surface
- Accept: session-scoped calls through `McpServer`/`McpClient`; tool invocation through `McpClientTool.InvokeAsync`; long-running calls through `RequestContext<T>.EnablePollingAsync` + the `McpServer` task verbs
- Reject: hand-rolled JSON-RPC framing, out-of-session protocol message construction, reading `.Server`/`.Services`/`.User`/`.Items` as direct `RequestContext<T>` members (they are inherited from `MessageContext`), filing `AsSamplingChatClient` under the MEAI catalogue
