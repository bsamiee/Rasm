# [RASM_APPHOST_API_GRPC_CLIENT]

`Grpc.Net.Client` supplies gRPC channels, HTTP-backed call invocation, service
configuration, retry, hedging, resolver, and load-balancer surfaces for remote
execution clients.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Net.Client`
- package: `Grpc.Net.Client`
- assembly: `Grpc.Net.Client`
- namespace: `Grpc.Net.Client`
- asset: runtime library
- rail: remote-client

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: channel and call contracts
- rail: remote-client

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]   | [CAPABILITY]            |
| :-----: | :---------------------- | :--------------- | :---------------------- |
|  [01]   | `GrpcChannel`           | channel root     | owns remote channel     |
|  [02]   | `GrpcChannelOptions`    | channel policy   | configures channel      |
|  [03]   | `HttpHandlerType`       | handler selector | classifies HTTP handler |
|  [04]   | `ServiceConfig`         | service policy   | configures calls        |
|  [05]   | `MethodConfig`          | method policy    | configures method calls |
|  [06]   | `MethodName`            | method selector  | selects service methods |
|  [07]   | `RetryPolicy`           | retry policy     | controls retry attempts |
|  [08]   | `HedgingPolicy`         | hedging policy   | controls parallel calls |
|  [09]   | `RetryThrottlingPolicy` | retry policy     | throttles retries       |

[PUBLIC_TYPE_SCOPE]: resolver and balancer contracts
- rail: remote-client

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]     | [CAPABILITY]             |
| :-----: | :---------------------- | :----------------- | :----------------------- |
|  [01]   | `Resolver`              | resolver root      | resolves endpoints       |
|  [02]   | `ResolverFactory`       | resolver factory   | creates resolvers        |
|  [03]   | `DnsResolverFactory`    | resolver factory   | creates DNS resolver     |
|  [04]   | `StaticResolverFactory` | resolver factory   | creates static resolver  |
|  [05]   | `ResolverResult`        | resolver output    | carries addresses        |
|  [06]   | `LoadBalancer`          | balancer root      | owns endpoint selection  |
|  [07]   | `LoadBalancerFactory`   | balancer factory   | creates balancers        |
|  [08]   | `PickFirstBalancer`     | balancer           | selects first endpoint   |
|  [09]   | `RoundRobinBalancer`    | balancer           | rotates endpoints        |
|  [10]   | `Subchannel`            | connection channel | owns endpoint connection |
|  [11]   | `SubchannelPicker`      | picker contract    | selects subchannels      |
|  [12]   | `PickResult`            | picker result      | carries selection result |

[PUBLIC_TYPE_SCOPE]: transitive `Grpc.Core.Api` call contracts
- rail: remote-client

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]      | [CAPABILITY]                        |
| :-----: | :------------------------- | :------------------ | :---------------------------------- |
|  [01]   | `Interceptor`              | interceptor base    | client call override family         |
|  [02]   | `ClientInterceptorContext` | call context struct | method, host, and options payload   |
|  [03]   | `CallInvoker`              | invocation root     | composes interceptors               |
|  [04]   | `CallInvokerExtensions`    | invoker extensions  | `Intercept` factory overloads       |
|  [05]   | `InterceptingCallInvoker`  | interceptor invoker | wraps invoker with one interceptor  |
|  [06]   | `CallOptions`              | call policy struct  | headers, deadline, and cancellation |
|  [07]   | `Metadata`                 | header collection   | metadata entries and binary values  |
|  [08]   | `RpcException`             | call failure        | status plus trailers                |
|  [09]   | `Status`                   | status struct       | code plus detail                    |
|  [10]   | `StatusCode`               | status enum         | call-failure taxonomy               |
|  [11]   | `CallCredentials`          | per-call trust      | interceptor-backed call credentials |
|  [12]   | `ChannelCredentials`       | channel trust       | transport credential selection      |
|  [13]   | `ConnectivityState`        | state enum          | channel connectivity taxonomy       |
|  [14]   | `AsyncAuthInterceptor`     | auth delegate       | async metadata injection delegate   |
|  [15]   | `AuthInterceptorContext`   | auth call context   | service URL, method, cancellation   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: channel operations
- rail: remote-client

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]    | [CAPABILITY]             |
| :-----: | :------------------------------------- | :-------------- | :----------------------- |
|  [01]   | `GrpcChannel.ForAddress`               | factory call    | creates channel          |
|  [02]   | `GrpcChannel.CreateCallInvoker`        | factory call    | creates call invoker     |
|  [03]   | `GrpcChannel.Dispose`                  | lifetime call   | closes channel           |
|  [04]   | `ServiceConfig`                        | option property | applies service policy   |
|  [05]   | `Credentials`                          | option property | applies channel security |
|  [06]   | `HttpHandler`                          | option property | selects HTTP transport   |
|  [07]   | `MaxReceiveMessageSize`                | option property | bounds response payloads |
|  [08]   | `MaxSendMessageSize`                   | option property | bounds request payloads  |
|  [09]   | `ThrowOperationCanceledOnCancellation` | option property | controls cancellation    |

[ENTRYPOINT_SCOPE]: channel-state and compression operations
- rail: remote-client

| [INDEX] | [SURFACE]                                 | [CALL_SHAPE]    | [CAPABILITY]                                          |
| :-----: | :---------------------------------------- | :-------------- | :---------------------------------------------------- |
|  [01]   | `GrpcChannel.State`                       | state property  | reports `ConnectivityState`                           |
|  [02]   | `GrpcChannel.WaitForStateChangedAsync`    | state call      | awaits departure from an observed `ConnectivityState` |
|  [03]   | `GrpcChannelOptions.CompressionProviders` | option property | registers `ICompressionProvider` rows                 |
|  [04]   | `GrpcChannelOptions.HttpVersion`          | option property | pins the channel HTTP version                         |
|  [05]   | `grpc-internal-encoding-request`          | metadata key    | selects per-call request compression                  |

[ENTRYPOINT_SCOPE]: interceptor override signatures
- rail: remote-client#CALL_SPINE

Each interceptor override is virtual and generic over `<TReq,TResp>`, receives `ClientInterceptorContext<TReq,TResp>`, and constrains both generic arguments to `class`.

| [INDEX] | [MEMBER]                   | [CALL_KIND]            | [CONTINUATION]                                     | [REQUEST_ARG]  |
| :-----: | :------------------------- | :--------------------- | :------------------------------------------------- | :------------- |
|  [01]   | `BlockingUnaryCall`        | blocking unary         | `BlockingUnaryCallContinuation<TReq,TResp>`        | `TReq request` |
|  [02]   | `AsyncUnaryCall`           | async unary            | `AsyncUnaryCallContinuation<TReq,TResp>`           | `TReq request` |
|  [03]   | `AsyncServerStreamingCall` | async server-streaming | `AsyncServerStreamingCallContinuation<TReq,TResp>` | `TReq request` |
|  [04]   | `AsyncClientStreamingCall` | async client-streaming | `AsyncClientStreamingCallContinuation<TReq,TResp>` | —              |
|  [05]   | `AsyncDuplexStreamingCall` | async duplex-streaming | `AsyncDuplexStreamingCallContinuation<TReq,TResp>` | —              |

[ENTRYPOINT_SCOPE]: `ClientInterceptorContext` struct members
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]  | [SIGNATURE]                                                                                      |
| :-----: | :-------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `Method`  | `Method<TRequest,TResponse> Method { get; }`                                                     |
|  [02]   | `Host`    | `string? Host { get; }`                                                                          |
|  [03]   | `Options` | `CallOptions Options { get; }`                                                                   |
|  [04]   | `ctor`    | `ClientInterceptorContext(Method<TRequest,TResponse> method, string? host, CallOptions options)` |

[ENTRYPOINT_SCOPE]: `CallOptions` struct members
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                | [SIGNATURE]                                                              |
| :-----: | :---------------------- | :----------------------------------------------------------------------- |
|  [01]   | `Headers`               | `Metadata? Headers { get; }`                                             |
|  [02]   | `Deadline`              | `DateTime? Deadline { get; }`                                            |
|  [03]   | `CancellationToken`     | `CancellationToken CancellationToken { get; }`                           |
|  [04]   | `Credentials`           | `CallCredentials? Credentials { get; }`                                  |
|  [05]   | `WithHeaders`           | `CallOptions WithHeaders(Metadata headers)`                              |
|  [06]   | `WithDeadline`          | `CallOptions WithDeadline(DateTime deadline)`                            |
|  [07]   | `WithCancellationToken` | `CallOptions WithCancellationToken(CancellationToken cancellationToken)` |
|  [08]   | `WithCredentials`       | `CallOptions WithCredentials(CallCredentials credentials)`               |

[ENTRYPOINT_SCOPE]: `RpcException` members
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]     | [SIGNATURE]                                                      |
| :-----: | :----------- | :--------------------------------------------------------------- |
|  [01]   | `Status`     | `Status Status { get; }`                                         |
|  [02]   | `StatusCode` | `StatusCode StatusCode { get; }`                                 |
|  [03]   | `Trailers`   | `Metadata Trailers { get; }`                                     |
|  [04]   | `ctor`       | `RpcException(Status status)`                                    |
|  [05]   | `ctor`       | `RpcException(Status status, string message)`                    |
|  [06]   | `ctor`       | `RpcException(Status status, Metadata trailers)`                 |
|  [07]   | `ctor`       | `RpcException(Status status, Metadata trailers, string message)` |

[ENTRYPOINT_SCOPE]: `StatusCode` enum members
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]             | [SIGNATURE]              |
| :-----: | :------------------- | :----------------------- |
|  [01]   | `OK`                 | `OK = 0`                 |
|  [02]   | `Cancelled`          | `Cancelled = 1`          |
|  [03]   | `Unknown`            | `Unknown = 2`            |
|  [04]   | `InvalidArgument`    | `InvalidArgument = 3`    |
|  [05]   | `DeadlineExceeded`   | `DeadlineExceeded = 4`   |
|  [06]   | `NotFound`           | `NotFound = 5`           |
|  [07]   | `AlreadyExists`      | `AlreadyExists = 6`      |
|  [08]   | `PermissionDenied`   | `PermissionDenied = 7`   |
|  [09]   | `ResourceExhausted`  | `ResourceExhausted = 8`  |
|  [10]   | `FailedPrecondition` | `FailedPrecondition = 9` |
|  [11]   | `Aborted`            | `Aborted = 10`           |
|  [12]   | `OutOfRange`         | `OutOfRange = 11`        |
|  [13]   | `Unimplemented`      | `Unimplemented = 12`     |
|  [14]   | `Internal`           | `Internal = 13`          |
|  [15]   | `Unavailable`        | `Unavailable = 14`       |
|  [16]   | `DataLoss`           | `DataLoss = 15`          |
|  [17]   | `Unauthenticated`    | `Unauthenticated = 16`   |

[ENTRYPOINT_SCOPE]: credential composition members
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                          | [SIGNATURE]                                                                                                |
| :-----: | :-------------------------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `CallCredentials.Compose`         | `static CallCredentials Compose(params CallCredentials[] credentials)`                                     |
|  [02]   | `CallCredentials.FromInterceptor` | `static CallCredentials FromInterceptor(AsyncAuthInterceptor interceptor)`                                 |
|  [03]   | `ChannelCredentials.Create`       | `static ChannelCredentials Create(ChannelCredentials channelCredentials, CallCredentials callCredentials)` |
|  [04]   | `ChannelCredentials.Insecure`     | `static ChannelCredentials Insecure { get; }`                                                              |
|  [05]   | `ChannelCredentials.SecureSsl`    | `static ChannelCredentials SecureSsl { get; }`                                                             |

[ENTRYPOINT_SCOPE]: `CallInvokerExtensions` intercept factory members
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                          | [SIGNATURE]                                                                                    |
| :-----: | :-------------------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `CallInvokerExtensions.Intercept` | `static CallInvoker Intercept(this CallInvoker invoker, Interceptor interceptor)`              |
|  [02]   | `CallInvokerExtensions.Intercept` | `static CallInvoker Intercept(this CallInvoker invoker, params Interceptor[] interceptors)`    |
|  [03]   | `CallInvokerExtensions.Intercept` | `static CallInvoker Intercept(this CallInvoker invoker, Func<Metadata, Metadata> interceptor)` |
|  [04]   | `InterceptingCallInvoker.ctor`    | `InterceptingCallInvoker(CallInvoker invoker, Interceptor interceptor)`                        |

[ENTRYPOINT_SCOPE]: `AsyncAuthInterceptor` delegate and `AuthInterceptorContext` members
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                                   | [SIGNATURE]                                                                                         |
| :-----: | :----------------------------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `AsyncAuthInterceptor`                     | `delegate Task AsyncAuthInterceptor(AuthInterceptorContext context, Metadata metadata)`             |
|  [02]   | `AuthInterceptorContext.ctor`              | `AuthInterceptorContext(string serviceUrl, string methodName)`                                      |
|  [03]   | `AuthInterceptorContext.ctor`              | `AuthInterceptorContext(string serviceUrl, string methodName, CancellationToken cancellationToken)` |
|  [04]   | `AuthInterceptorContext.ServiceUrl`        | `string ServiceUrl { get; }`                                                                        |
|  [05]   | `AuthInterceptorContext.MethodName`        | `string MethodName { get; }`                                                                        |
|  [06]   | `AuthInterceptorContext.CancellationToken` | `CancellationToken CancellationToken { get; }`                                                      |

[ENTRYPOINT_SCOPE]: `SocketsHttpHandler` keepalive members
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                                            | [SIGNATURE]                                                           |
| :-----: | :-------------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `SocketsHttpHandler.KeepAlivePingDelay`             | `TimeSpan KeepAlivePingDelay { get; set; }`                           |
|  [02]   | `SocketsHttpHandler.KeepAlivePingTimeout`           | `TimeSpan KeepAlivePingTimeout { get; set; }`                         |
|  [03]   | `SocketsHttpHandler.KeepAlivePingPolicy`            | `HttpKeepAlivePingPolicy KeepAlivePingPolicy { get; set; }`           |
|  [04]   | `SocketsHttpHandler.EnableMultipleHttp2Connections` | `bool EnableMultipleHttp2Connections { get; set; }`                   |
|  [05]   | `HttpKeepAlivePingPolicy.WithActiveRequests`        | `WithActiveRequests = 0` — ping only when active requests are pending |
|  [06]   | `HttpKeepAlivePingPolicy.Always`                    | `Always = 1` — ping even on idle connections                          |

[ENTRYPOINT_SCOPE]: compression provider types
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                                         | [SIGNATURE]                                                                         |
| :-----: | :----------------------------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `ICompressionProvider.EncodingName`              | `string EncodingName { get; }`                                                      |
|  [02]   | `ICompressionProvider.CreateCompressionStream`   | `Stream CreateCompressionStream(Stream stream, CompressionLevel? compressionLevel)` |
|  [03]   | `ICompressionProvider.CreateDecompressionStream` | `Stream CreateDecompressionStream(Stream stream)`                                   |
|  [04]   | `GzipCompressionProvider.ctor`                   | `GzipCompressionProvider(CompressionLevel defaultCompressionLevel)`                 |
|  [05]   | `GzipCompressionProvider.EncodingName`           | `string EncodingName { get; }` returns `"gzip"`                                     |
|  [06]   | `DeflateCompressionProvider.ctor`                | `DeflateCompressionProvider(CompressionLevel defaultCompressionLevel)`              |
|  [07]   | `DeflateCompressionProvider.EncodingName`        | `string EncodingName { get; }` returns `"deflate"`                                  |

[ENTRYPOINT_SCOPE]: `GrpcChannelOptions` property surface — transport and payload
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                                  | [SIGNATURE]                                                       |
| :-----: | :---------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `Credentials`                             | `ChannelCredentials? Credentials { get; set; }`                   |
|  [02]   | `MaxSendMessageSize`                      | `int? MaxSendMessageSize { get; set; }`                           |
|  [03]   | `MaxReceiveMessageSize`                   | `int? MaxReceiveMessageSize { get; set; }`                        |
|  [04]   | `MaxRetryAttempts`                        | `int? MaxRetryAttempts { get; set; }`                             |
|  [05]   | `MaxRetryBufferSize`                      | `long? MaxRetryBufferSize { get; set; }`                          |
|  [06]   | `MaxRetryBufferPerCallSize`               | `long? MaxRetryBufferPerCallSize { get; set; }`                   |
|  [07]   | `CompressionProviders`                    | `IList<ICompressionProvider>? CompressionProviders { get; set; }` |
|  [08]   | `HttpClient`                              | `HttpClient? HttpClient { get; set; }`                            |
|  [09]   | `HttpHandler`                             | `HttpMessageHandler? HttpHandler { get; set; }`                   |
|  [10]   | `DisposeHttpClient`                       | `bool DisposeHttpClient { get; set; }`                            |
|  [11]   | `ThrowOperationCanceledOnCancellation`    | `bool ThrowOperationCanceledOnCancellation { get; set; }`         |
|  [12]   | `UnsafeUseInsecureChannelCallCredentials` | `bool UnsafeUseInsecureChannelCallCredentials { get; set; }`      |
|  [13]   | `ServiceConfig`                           | `ServiceConfig? ServiceConfig { get; set; }`                      |
|  [14]   | `DisableResolverServiceConfig`            | `bool DisableResolverServiceConfig { get; set; }`                 |
|  [15]   | `ServiceProvider`                         | `IServiceProvider? ServiceProvider { get; set; }`                 |
|  [16]   | `HttpVersion`                             | `Version? HttpVersion { get; set; }`                              |
|  [17]   | `HttpVersionPolicy`                       | `HttpVersionPolicy? HttpVersionPolicy { get; set; }`              |
|  [18]   | `LoggerFactory`                           | `ILoggerFactory? LoggerFactory { get; set; }`                     |

[ENTRYPOINT_SCOPE]: `GrpcChannelOptions` property surface — reconnect backoff
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                  | [SIGNATURE]                                                    |
| :-----: | :------------------------ | :------------------------------------------------------------- |
|  [01]   | `MaxReconnectBackoff`     | `TimeSpan? MaxReconnectBackoff { get; set; }` — default 120 s  |
|  [02]   | `InitialReconnectBackoff` | `TimeSpan InitialReconnectBackoff { get; set; }` — default 1 s |

[ENTRYPOINT_SCOPE]: policy and balancing operations
- rail: remote-client

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]    | [CAPABILITY]             |
| :-----: | :----------------------------- | :-------------- | :----------------------- |
|  [01]   | `RetryPolicy`                  | policy object   | sets retry bounds        |
|  [02]   | `HedgingPolicy`                | policy object   | sets hedge bounds        |
|  [03]   | `RoundRobinConfig`             | balancer config | selects round robin      |
|  [04]   | `PickFirstConfig`              | balancer config | selects first endpoint   |
|  [05]   | `Resolver.Start` / `Refresh`   | resolver call   | drives endpoint updates  |
|  [06]   | `Subchannel.RequestConnection` | channel call    | starts connection        |
|  [07]   | `SubchannelPicker.Pick`        | picker call     | selects subchannel       |
|  [08]   | `PickResult.ForSubchannel`     | result factory  | returns selected channel |
|  [09]   | `PickResult.ForFailure`        | result factory  | returns failed selection |

## [04]-[IMPLEMENTATION_LAW]

[CHANNEL_POLICY]:
- namespace: `Grpc.Net.Client`
- channel root: `GrpcChannel`
- policy root: `GrpcChannelOptions`
- transport root: HTTP handler selection stays explicit at channel creation
- payload bounds: send and receive message sizes are part of remote execution policy

[INTERCEPTOR_SURFACE]:
- namespace: `Grpc.Core.Interceptors`
- base class: `abstract class Interceptor` in `Grpc.Core.Api` 2.80.0
- client overrides: `BlockingUnaryCall`, `AsyncUnaryCall`, `AsyncServerStreamingCall`, `AsyncClientStreamingCall`, `AsyncDuplexStreamingCall` — each virtual with a matching continuation delegate type
- context type: `ClientInterceptorContext<TRequest,TResponse>` struct carrying `Method`, `Host`, and `Options`
- composition: `CallInvokerExtensions.Intercept(this CallInvoker, Interceptor)`, `Intercept(this CallInvoker, params Interceptor[])`, and `Intercept(this CallInvoker, Func<Metadata,Metadata>)` build chains; multiple interceptors invoked in argument order
- runtime wrapper: `InterceptingCallInvoker(CallInvoker, Interceptor)` is `internal sealed` — acquire only through `CallInvokerExtensions.Intercept`
- server overrides: `UnaryServerHandler`, `ClientStreamingServerHandler`, `ServerStreamingServerHandler`, `DuplexStreamingServerHandler` — out of scope for the Compute client rail

[KEEPALIVE_POLICY]:
- keepalive is owned by `SocketsHttpHandler` (BCL) when passed as `GrpcChannelOptions.HttpHandler`
- properties: `SocketsHttpHandler.KeepAlivePingDelay`, `KeepAlivePingTimeout`, `KeepAlivePingPolicy` (`HttpKeepAlivePingPolicy.WithActiveRequests` / `Always`), `EnableMultipleHttp2Connections`
- reconnect backoff: `GrpcChannelOptions.InitialReconnectBackoff` (default 1 s) and `MaxReconnectBackoff` (default 120 s) control exponential backoff between connection attempts

[COMPRESSION_SURFACE]:
- namespace: `Grpc.Net.Compression` (in `Grpc.Net.Common` 2.80.0)
- interface: `ICompressionProvider` with `EncodingName`, `CreateCompressionStream`, `CreateDecompressionStream`
- built-in providers: `GzipCompressionProvider(CompressionLevel)` → encoding `"gzip"`; `DeflateCompressionProvider(CompressionLevel)` → encoding `"deflate"` (wraps `ZLibStream`)
- registration: `GrpcChannelOptions.CompressionProviders` accepts `IList<ICompressionProvider>`
- per-call selection: `grpc-internal-encoding-request` metadata key

[REMOTE_RESILIENCE]:
- namespace: `Grpc.Net.Client.Configuration`
- retry: `RetryPolicy` sets attempts, backoff, status codes, and commit behavior
- hedging: `HedgingPolicy` sets parallel attempt limits and delay
- service config: method policy stays data-driven and does not enter generated clients

[LOCAL_ADMISSION]:
- Compute remote calls enter through client-side channels only.
- Server-side gRPC packages remain outside the Compute package graph.
- Resolver and balancer surfaces are admitted only through remote execution policy.
- Generated clients are typed edge adapters over Compute request and receipt algebra.

[RAIL_LAW]:
- Package: `Grpc.Net.Client`
- Owns: client channels, call invocation, client policy
- Accept: measured remote execution calls
- Reject: server hosting surface
