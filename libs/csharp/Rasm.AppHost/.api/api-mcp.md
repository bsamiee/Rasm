# [RASM_APPHOST_API_MCP]

`ModelContextProtocol.Core` supplies the session, client, server, transport, and primitive surfaces for the MCP protocol; `ModelContextProtocol` provides DI composition, builder extensions, and hosted-service plumbing; `ModelContextProtocol.AspNetCore` adds HTTP transport, SSE event-stream persistence, and authentication handler registration for ASP.NET Core hosts.

## [1]-[PACKAGE_SURFACE]

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

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: session and protocol primitives — `ModelContextProtocol.Core`
- rail: mcp-protocol

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]       | [CAPABILITY]                               |
| :-----: | :--------------------- | :------------------ | :----------------------------------------- |
|   [1]   | `McpSession`           | abstract base class | shared session lifecycle for client+server |
|   [2]   | `McpServer`            | abstract class      | server-side session, extends `McpSession`  |
|   [3]   | `McpClient`            | abstract class      | client-side session, extends `McpSession`  |
|   [4]   | `McpException`         | exception           | typed protocol failure                     |
|   [5]   | `McpErrorCode`         | enum                | JSON-RPC error code vocabulary             |
|   [6]   | `RequestOptions`       | options class       | per-request timeout and cancellation       |
|   [7]   | `IMcpTaskStore`        | interface           | task-result persistence contract           |
|   [8]   | `InMemoryMcpTaskStore` | class               | in-process `IMcpTaskStore` implementation  |
|   [9]   | `McpJsonUtilities`     | static class        | `JsonSerializerOptions` and type-info      |
|  [10]   | `AIContentExtensions`  | static class        | `AIContent` ↔ MCP content conversions      |
|  [11]   | `NullProgress`         | class               | no-op `IProgress<T>` implementation        |
|  [12]   | `UriTemplate`          | class               | RFC 6570 URI template evaluation           |

[PUBLIC_TYPE_SCOPE]: server primitives — `ModelContextProtocol.Server`
- rail: mcp-protocol

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]    | [CAPABILITY]                                          |
| :-----: | :------------------------ | :--------------- | :---------------------------------------------------- |
|   [1]   | `McpServerTool`           | abstract class   | tool primitive base, implements `IMcpServerPrimitive` |
|   [2]   | `McpServerPrompt`         | abstract class   | prompt primitive base                                 |
|   [3]   | `McpServerResource`       | abstract class   | resource primitive base                               |
|   [4]   | `AIFunctionMcpServerTool` | sealed class     | `AIFunction`-backed tool adapter                      |
|   [5]   | `DelegatingMcpServerTool` | class            | delegate-wrapping tool                                |
|   [6]   | `McpServerOptions`        | sealed class     | server configuration root                             |
|   [7]   | `McpServerHandlers`       | class            | server request-handler registry                       |
|   [8]   | `McpServerFilters`        | class            | server message and request filter registry            |
|   [9]   | `McpServerToolAttribute`  | sealed attribute | marks a method as an MCP tool                         |
|  [10]   | `McpMetaAttribute`        | sealed attribute | attaches metadata to server primitives                |
|  [11]   | `StdioServerTransport`    | class            | stdio-backed server transport                         |
|  [12]   | `StreamServerTransport`   | class            | stream-backed server transport                        |

[PUBLIC_TYPE_SCOPE]: client primitives — `ModelContextProtocol.Client`
- rail: mcp-protocol

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------------------- | :------------ | :---------------------------------------- |
|   [1]   | `McpClientTool`                        | sealed class  | extends `AIFunction`; wraps a server tool |
|   [2]   | `McpClientPrompt`                      | sealed class  | client-side prompt accessor               |
|   [3]   | `McpClientResource`                    | sealed class  | client-side resource accessor             |
|   [4]   | `McpClientResourceTemplate`            | sealed class  | client-side resource template accessor    |
|   [5]   | `McpClientOptions`                     | class         | client configuration root                 |
|   [6]   | `McpClientHandlers`                    | class         | client notification handler registry      |
|   [7]   | `IClientTransport`                     | interface     | session-transport factory contract        |
|   [8]   | `HttpClientTransport`                  | class         | HTTP/SSE client transport                 |
|   [9]   | `StdioClientTransport`                 | class         | stdio client transport                    |
|  [10]   | `SseClientSessionTransport`            | class         | SSE session transport                     |
|  [11]   | `StdioClientTransportOptions`          | class         | stdio transport configuration             |
|  [12]   | `HttpClientTransportOptions`           | class         | HTTP transport configuration              |
|  [13]   | `StreamableHttpClientSessionTransport` | class         | streamable HTTP session transport         |
|  [14]   | `ClientTransportClosedException`       | class         | transport-closed typed failure            |

[PUBLIC_TYPE_SCOPE]: DI and builder — `Microsoft.Extensions.DependencyInjection` (in `ModelContextProtocol`)
- rail: mcp-host

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :---------------------------------- | :------------ | :------------------------------------------------- |
|   [1]   | `IMcpServerBuilder`                 | interface     | server builder contract                            |
|   [2]   | `IMcpMessageFilterBuilder`          | interface     | message-filter builder contract                    |
|   [3]   | `IMcpRequestFilterBuilder`          | interface     | request-filter builder contract                    |
|   [4]   | `McpServerBuilderExtensions`        | static class  | `WithTools`, `WithPrompts`, `WithResources` fluent |
|   [5]   | `McpMessageFilterBuilderExtensions` | static class  | `AddIncomingFilter`, `AddOutgoingFilter`           |
|   [6]   | `McpRequestFilterBuilderExtensions` | static class  | per-operation filter registration                  |

[PUBLIC_TYPE_SCOPE]: ASP.NET Core host — `ModelContextProtocol.AspNetCore`
- rail: mcp-host

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :---------------------------------- | :------------ | :--------------------------------------------- |
|   [1]   | `HttpServerTransportOptions`        | class         | HTTP server transport configuration            |
|   [2]   | `ISessionMigrationHandler`          | interface     | session migration between instances contract   |
|   [3]   | `McpEndpointRouteBuilderExtensions` | static class  | `MapMcp(pattern)` route registration           |
|   [4]   | `HttpMcpServerBuilderExtensions`    | static class  | `WithHttpTransport`, `AddAuthorizationFilters` |
|   [5]   | `McpAuthenticationExtensions`       | static class  | `AddMcp` authentication scheme builder         |
|   [6]   | `McpAuthenticationDefaults`         | static class  | scheme name constants                          |
|   [7]   | `McpAuthenticationOptions`          | class         | extends `AuthenticationSchemeOptions`          |
|   [8]   | `McpAuthenticationHandler`          | class         | `AuthenticationHandler` implementation         |
|   [9]   | `ResourceMetadataRequestContext`    | class         | resource metadata request context              |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: server registration
- rail: mcp-host

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY]  | [CAPABILITY]                                  |
| :-----: | :-------------------------------------------------------------------- | :-------------- | :-------------------------------------------- |
|   [1]   | `AddMcpServer(this IServiceCollection)`                               | DI registration | registers server, returns `IMcpServerBuilder` |
|   [2]   | `WithTools<TToolType>(this IMcpServerBuilder)`                        | builder fluent  | discovers `[McpServerTool]` on type           |
|   [3]   | `WithPrompts<TPromptType>(this IMcpServerBuilder)`                    | builder fluent  | discovers prompts on type                     |
|   [4]   | `WithResources<TResourceType>(this IMcpServerBuilder)`                | builder fluent  | discovers resources on type                   |
|   [5]   | `WithToolsFromAssembly(this IMcpServerBuilder, Assembly?)`            | builder fluent  | assembly-scan tool discovery                  |
|   [6]   | `WithListToolsHandler(this IMcpServerBuilder, McpRequestHandler<..>)` | builder fluent  | registers explicit list-tools handler         |
|   [7]   | `WithCallToolHandler(this IMcpServerBuilder, McpRequestHandler<..>)`  | builder fluent  | registers explicit call-tool handler          |
|   [8]   | `WithHttpTransport(this IMcpServerBuilder, ...)`                      | builder fluent  | attaches HTTP/SSE server transport            |
|   [9]   | `MapMcp(this IEndpointRouteBuilder, pattern)`                         | routing         | maps MCP endpoint at given route pattern      |
|  [10]   | `AddMcp(this AuthenticationBuilder, ...)`                             | auth            | registers MCP authentication scheme           |

[ENTRYPOINT_SCOPE]: client construction
- rail: mcp-protocol

| [INDEX] | [SURFACE]                                                                         | [ENTRY_FAMILY] | [CAPABILITY]                           |
| :-----: | :-------------------------------------------------------------------------------- | :------------- | :------------------------------------- |
|   [1]   | `McpClient.CreateAsync(IClientTransport, McpClientOptions?, ILoggerFactory?, CT)` | static factory | constructs and initializes `McpClient` |
|   [2]   | `McpClient.ResumeSessionAsync(IClientTransport, ResumeClientSessionOptions, ...)` | static factory | resumes a detached session             |
|   [3]   | `McpClient.ListToolsAsync(RequestOptions?, CT)`                                   | session call   | enumerates server tools                |
|   [4]   | `McpClient.CallToolAsync(name, args, RequestOptions?)`                            | session call   | invokes named server tool              |
|   [5]   | `McpClient.ListPromptsAsync(RequestOptions?, CT)`                                 | session call   | enumerates server prompts              |
|   [6]   | `McpClient.GetPromptAsync(name, args, RequestOptions?)`                           | session call   | retrieves prompt content               |
|   [7]   | `McpClient.ListResourcesAsync(RequestOptions?, CT)`                               | session call   | enumerates server resources            |
|   [8]   | `McpClient.SubscribeToResourceAsync(uri, CT)`                                     | session call   | subscribes to resource update events   |
|   [9]   | `McpClientTool.InvokeAsync(args, CT)`                                             | tool call      | `AIFunction` contract invocation       |

[ENTRYPOINT_SCOPE]: server options surface
- rail: mcp-protocol

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY]  | [CAPABILITY]                                    |
| :-----: | :--------------------------------------- | :-------------- | :---------------------------------------------- |
|   [1]   | `McpServerOptions.ServerInfo`            | option property | `Implementation` name and version info          |
|   [2]   | `McpServerOptions.Capabilities`          | option property | `ServerCapabilities` advertised to clients      |
|   [3]   | `McpServerOptions.ProtocolVersion`       | option property | date-versioned protocol string                  |
|   [4]   | `McpServerOptions.InitializationTimeout` | option property | handshake timeout; default 60 s                 |
|   [5]   | `McpServerOptions.ServerInstructions`    | option property | system instructions delivered to clients        |
|   [6]   | `McpServerOptions.ToolCollection`        | option property | `McpServerPrimitiveCollection<McpServerTool>`   |
|   [7]   | `McpServerOptions.PromptCollection`      | option property | `McpServerPrimitiveCollection<McpServerPrompt>` |
|   [8]   | `McpServerOptions.ResourceCollection`    | option property | `McpServerResourceCollection`                   |

## [4]-[IMPLEMENTATION_LAW]

[SESSION_TOPOLOGY]:
- namespace roots: `ModelContextProtocol`, `ModelContextProtocol.Client`, `ModelContextProtocol.Server`
- session base: `McpSession : IAsyncDisposable`; both `McpServer` and `McpClient` extend it
- tool integration: `McpClientTool : AIFunction` — client-side tools surface as `AIFunction` instances
- primitive base: `McpServerTool`, `McpServerPrompt`, `McpServerResource` all implement `IMcpServerPrimitive`
- attribute discovery: `[McpServerTool]` on public methods drives reflection-based tool registration
- task persistence: `IMcpTaskStore` / `InMemoryMcpTaskStore` back the out-of-band task-result protocol

[LOCAL_ADMISSION]:
- DI registration entry: `services.AddMcpServer()` returns `IMcpServerBuilder`; `WithHttpTransport()` attaches HTTP/SSE
- HTTP host entry: `MapMcp(pattern)` registers the endpoint; default pattern is `""`
- Auth extension: `authBuilder.AddMcp()` registers scheme; `McpAuthenticationHandler` implements token exchange
- Client construction: `McpClient.CreateAsync(IClientTransport, McpClientOptions?, ILoggerFactory?, CT)` is the sole construction point; `ResumeSessionAsync` resumes a detached session
- JSON options: `McpJsonUtilities.DefaultOptions` carries the canonical `JsonSerializerOptions`; use it at boundaries

[RAIL_LAW]:
- Package: `ModelContextProtocol.Core` + `ModelContextProtocol` + `ModelContextProtocol.AspNetCore`
- Owns: MCP session, server primitives, client tools, transport selection, DI registration, HTTP hosting
- Accept: session-scoped calls through `McpServer`/`McpClient`; tool invocation through `McpClientTool.InvokeAsync`
- Reject: hand-rolled JSON-RPC framing, out-of-session protocol message construction
