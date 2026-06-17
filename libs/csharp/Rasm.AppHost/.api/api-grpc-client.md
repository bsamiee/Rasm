# [RASM_COMPUTE_API_GRPC_CLIENT]

`Grpc.Net.Client` supplies gRPC channels, HTTP-backed call invocation, service
configuration, retry, hedging, resolver, and load-balancer surfaces for remote
execution clients.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Net.Client`
- package: `Grpc.Net.Client`
- assembly: `Grpc.Net.Client`
- namespace: `Grpc.Net.Client`
- asset: runtime library
- rail: remote-client

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: channel and call contracts
- rail: remote-client

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]   | [CAPABILITY]            |
| :-----: | :---------------------- | :--------------- | :---------------------- |
|   [1]   | `GrpcChannel`           | channel root     | owns remote channel     |
|   [2]   | `GrpcChannelOptions`    | channel policy   | configures channel      |
|   [3]   | `HttpHandlerType`       | handler selector | classifies HTTP handler |
|   [4]   | `ServiceConfig`         | service policy   | configures calls        |
|   [5]   | `MethodConfig`          | method policy    | configures method calls |
|   [6]   | `MethodName`            | method selector  | selects service methods |
|   [7]   | `RetryPolicy`           | retry policy     | controls retry attempts |
|   [8]   | `HedgingPolicy`         | hedging policy   | controls parallel calls |
|   [9]   | `RetryThrottlingPolicy` | retry policy     | throttles retries       |

[PUBLIC_TYPE_SCOPE]: resolver and balancer contracts
- rail: remote-client

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]     | [CAPABILITY]             |
| :-----: | :---------------------- | :----------------- | :----------------------- |
|   [1]   | `Resolver`              | resolver root      | resolves endpoints       |
|   [2]   | `ResolverFactory`       | resolver factory   | creates resolvers        |
|   [3]   | `DnsResolverFactory`    | resolver factory   | creates DNS resolver     |
|   [4]   | `StaticResolverFactory` | resolver factory   | creates static resolver  |
|   [5]   | `ResolverResult`        | resolver output    | carries addresses        |
|   [6]   | `LoadBalancer`          | balancer root      | owns endpoint selection  |
|   [7]   | `LoadBalancerFactory`   | balancer factory   | creates balancers        |
|   [8]   | `PickFirstBalancer`     | balancer           | selects first endpoint   |
|   [9]   | `RoundRobinBalancer`    | balancer           | rotates endpoints        |
|  [10]   | `Subchannel`            | connection channel | owns endpoint connection |
|  [11]   | `SubchannelPicker`      | picker contract    | selects subchannels      |
|  [12]   | `PickResult`            | picker result      | carries selection result |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: channel operations
- rail: remote-client

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]    | [CAPABILITY]             |
| :-----: | :------------------------------------- | :-------------- | :----------------------- |
|   [1]   | `GrpcChannel.ForAddress`               | factory call    | creates channel          |
|   [2]   | `GrpcChannel.CreateCallInvoker`        | factory call    | creates call invoker     |
|   [3]   | `GrpcChannel.Dispose`                  | lifetime call   | closes channel           |
|   [4]   | `ServiceConfig`                        | option property | applies service policy   |
|   [5]   | `Credentials`                          | option property | applies channel security |
|   [6]   | `HttpHandler`                          | option property | selects HTTP transport   |
|   [7]   | `MaxReceiveMessageSize`                | option property | bounds response payloads |
|   [8]   | `MaxSendMessageSize`                   | option property | bounds request payloads  |
|   [9]   | `ThrowOperationCanceledOnCancellation` | option property | controls cancellation    |

[ENTRYPOINT_SCOPE]: channel-state and compression operations
- rail: remote-client

| [INDEX] | [SURFACE]                                 | [CALL_SHAPE]    | [CAPABILITY]                                          |
| :-----: | :---------------------------------------- | :-------------- | :---------------------------------------------------- |
|   [1]   | `GrpcChannel.State`                       | state property  | reports `ConnectivityState`                           |
|   [2]   | `GrpcChannel.WaitForStateChangedAsync`    | state call      | awaits departure from an observed `ConnectivityState` |
|   [3]   | `GrpcChannelOptions.CompressionProviders` | option property | registers `ICompressionProvider` rows                 |
|   [4]   | `GrpcChannelOptions.HttpVersion`          | option property | pins the channel HTTP version                         |
|   [5]   | `grpc-internal-encoding-request`          | metadata key    | selects per-call request compression                  |

[PUBLIC_TYPE_SCOPE]: transitive `Grpc.Core.Api` call contracts
- rail: remote-client

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]      | [CAPABILITY]                        |
| :-----: | :------------------------- | :------------------ | :---------------------------------- |
|   [1]   | `Interceptor`              | interceptor base    | client call override family         |
|   [2]   | `ClientInterceptorContext` | call context struct | method, host, and options payload   |
|   [3]   | `CallInvoker`              | invocation root     | composes interceptors               |
|   [4]   | `CallInvokerExtensions`    | invoker extensions  | `Intercept` factory overloads       |
|   [5]   | `InterceptingCallInvoker`  | interceptor invoker | wraps invoker with one interceptor  |
|   [6]   | `CallOptions`              | call policy struct  | headers, deadline, and cancellation |
|   [7]   | `Metadata`                 | header collection   | metadata entries and binary values  |
|   [8]   | `RpcException`             | call failure        | status plus trailers                |
|   [9]   | `Status`                   | status struct       | code plus detail                    |
|  [10]   | `StatusCode`               | status enum         | call-failure taxonomy               |
|  [11]   | `CallCredentials`          | per-call trust      | interceptor-backed call credentials |
|  [12]   | `ChannelCredentials`       | channel trust       | transport credential selection      |
|  [13]   | `ConnectivityState`        | state enum          | channel connectivity taxonomy       |
|  [14]   | `AsyncAuthInterceptor`     | auth delegate       | async metadata injection delegate   |
|  [15]   | `AuthInterceptorContext`   | auth call context   | service URL, method, cancellation   |

[ENTRYPOINT_SCOPE]: interceptor override signatures
- source: `Grpc.Core.Api` 2.80.0 — `Grpc.Core.Interceptors.Interceptor` decompile
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                   | [SIGNATURE]                                                                                                                                                                                                                                      | [USED_BY]              | [EVIDENCE]       |
| :-----: | :------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------- | :--------------- |
|   [1]   | `BlockingUnaryCall`        | `virtual TResponse BlockingUnaryCall<TReq,TResp>(TReq request, ClientInterceptorContext<TReq,TResp> context, BlockingUnaryCallContinuation<TReq,TResp> continuation) where TReq : class where TResp : class`                                     | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [2]   | `AsyncUnaryCall`           | `virtual AsyncUnaryCall<TResp> AsyncUnaryCall<TReq,TResp>(TReq request, ClientInterceptorContext<TReq,TResp> context, AsyncUnaryCallContinuation<TReq,TResp> continuation) where TReq : class where TResp : class`                               | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [3]   | `AsyncServerStreamingCall` | `virtual AsyncServerStreamingCall<TResp> AsyncServerStreamingCall<TReq,TResp>(TReq request, ClientInterceptorContext<TReq,TResp> context, AsyncServerStreamingCallContinuation<TReq,TResp> continuation) where TReq : class where TResp : class` | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [4]   | `AsyncClientStreamingCall` | `virtual AsyncClientStreamingCall<TReq,TResp> AsyncClientStreamingCall<TReq,TResp>(ClientInterceptorContext<TReq,TResp> context, AsyncClientStreamingCallContinuation<TReq,TResp> continuation) where TReq : class where TResp : class`          | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [5]   | `AsyncDuplexStreamingCall` | `virtual AsyncDuplexStreamingCall<TReq,TResp> AsyncDuplexStreamingCall<TReq,TResp>(ClientInterceptorContext<TReq,TResp> context, AsyncDuplexStreamingCallContinuation<TReq,TResp> continuation) where TReq : class where TResp : class`          | remote-lane#CALL_SPINE | decompile 2.80.0 |

[ENTRYPOINT_SCOPE]: `ClientInterceptorContext` struct members
- source: `Grpc.Core.Api` 2.80.0 — `Grpc.Core.Interceptors.ClientInterceptorContext<TRequest,TResponse>` decompile
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]  | [SIGNATURE]                                                                                      | [USED_BY]              | [EVIDENCE]       |
| :-----: | :-------- | :----------------------------------------------------------------------------------------------- | :--------------------- | :--------------- |
|   [1]   | `Method`  | `Method<TRequest,TResponse> Method { get; }`                                                     | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [2]   | `Host`    | `string? Host { get; }`                                                                          | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [3]   | `Options` | `CallOptions Options { get; }`                                                                   | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [4]   | `ctor`    | `ClientInterceptorContext(Method<TRequest,TResponse> method, string? host, CallOptions options)` | remote-lane#CALL_SPINE | decompile 2.80.0 |

[ENTRYPOINT_SCOPE]: `CallOptions` struct members
- source: `Grpc.Core.Api` 2.80.0 — `Grpc.Core.CallOptions` decompile
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                | [SIGNATURE]                                                              | [USED_BY]              | [EVIDENCE]       |
| :-----: | :---------------------- | :----------------------------------------------------------------------- | :--------------------- | :--------------- |
|   [1]   | `Headers`               | `Metadata? Headers { get; }`                                             | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [2]   | `Deadline`              | `DateTime? Deadline { get; }`                                            | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [3]   | `CancellationToken`     | `CancellationToken CancellationToken { get; }`                           | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [4]   | `Credentials`           | `CallCredentials? Credentials { get; }`                                  | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [5]   | `WithHeaders`           | `CallOptions WithHeaders(Metadata headers)`                              | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [6]   | `WithDeadline`          | `CallOptions WithDeadline(DateTime deadline)`                            | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [7]   | `WithCancellationToken` | `CallOptions WithCancellationToken(CancellationToken cancellationToken)` | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [8]   | `WithCredentials`       | `CallOptions WithCredentials(CallCredentials credentials)`               | remote-lane#CALL_SPINE | decompile 2.80.0 |

[ENTRYPOINT_SCOPE]: `RpcException` members
- source: `Grpc.Core.Api` 2.80.0 — `Grpc.Core.RpcException` decompile
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]     | [SIGNATURE]                                                      | [USED_BY]              | [EVIDENCE]       |
| :-----: | :----------- | :--------------------------------------------------------------- | :--------------------- | :--------------- |
|   [1]   | `Status`     | `Status Status { get; }`                                         | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [2]   | `StatusCode` | `StatusCode StatusCode { get; }`                                 | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [3]   | `Trailers`   | `Metadata Trailers { get; }`                                     | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [4]   | `ctor`       | `RpcException(Status status)`                                    | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [5]   | `ctor`       | `RpcException(Status status, string message)`                    | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [6]   | `ctor`       | `RpcException(Status status, Metadata trailers)`                 | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [7]   | `ctor`       | `RpcException(Status status, Metadata trailers, string message)` | remote-lane#CALL_SPINE | decompile 2.80.0 |

[ENTRYPOINT_SCOPE]: `StatusCode` enum members
- source: `Grpc.Core.Api` 2.80.0 — `Grpc.Core.StatusCode` decompile
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]             | [SIGNATURE]              | [USED_BY]              | [EVIDENCE]       |
| :-----: | :------------------- | :----------------------- | :--------------------- | :--------------- |
|   [1]   | `OK`                 | `OK = 0`                 | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [2]   | `Cancelled`          | `Cancelled = 1`          | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [3]   | `Unknown`            | `Unknown = 2`            | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [4]   | `InvalidArgument`    | `InvalidArgument = 3`    | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [5]   | `DeadlineExceeded`   | `DeadlineExceeded = 4`   | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [6]   | `NotFound`           | `NotFound = 5`           | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [7]   | `AlreadyExists`      | `AlreadyExists = 6`      | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [8]   | `PermissionDenied`   | `PermissionDenied = 7`   | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [9]   | `ResourceExhausted`  | `ResourceExhausted = 8`  | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [10]   | `FailedPrecondition` | `FailedPrecondition = 9` | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [11]   | `Aborted`            | `Aborted = 10`           | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [12]   | `OutOfRange`         | `OutOfRange = 11`        | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [13]   | `Unimplemented`      | `Unimplemented = 12`     | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [14]   | `Internal`           | `Internal = 13`          | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [15]   | `Unavailable`        | `Unavailable = 14`       | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [16]   | `DataLoss`           | `DataLoss = 15`          | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [17]   | `Unauthenticated`    | `Unauthenticated = 16`   | remote-lane#CALL_SPINE | decompile 2.80.0 |

[ENTRYPOINT_SCOPE]: credential composition members
- source: `Grpc.Core.Api` 2.80.0 — `Grpc.Core.CallCredentials` / `Grpc.Core.ChannelCredentials` decompile
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                          | [SIGNATURE]                                                                                                | [USED_BY]              | [EVIDENCE]       |
| :-----: | :-------------------------------- | :--------------------------------------------------------------------------------------------------------- | :--------------------- | :--------------- |
|   [1]   | `CallCredentials.Compose`         | `static CallCredentials Compose(params CallCredentials[] credentials)`                                     | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [2]   | `CallCredentials.FromInterceptor` | `static CallCredentials FromInterceptor(AsyncAuthInterceptor interceptor)`                                 | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [3]   | `ChannelCredentials.Create`       | `static ChannelCredentials Create(ChannelCredentials channelCredentials, CallCredentials callCredentials)` | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [4]   | `ChannelCredentials.Insecure`     | `static ChannelCredentials Insecure { get; }`                                                              | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [5]   | `ChannelCredentials.SecureSsl`    | `static ChannelCredentials SecureSsl { get; }`                                                             | remote-lane#CALL_SPINE | decompile 2.80.0 |

[ENTRYPOINT_SCOPE]: `CallInvokerExtensions` intercept factory members
- source: `Grpc.Core.Api` 2.80.0 — `Grpc.Core.Interceptors.CallInvokerExtensions` decompile
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                          | [SIGNATURE]                                                                                    | [USED_BY]              | [EVIDENCE]       |
| :-----: | :-------------------------------- | :--------------------------------------------------------------------------------------------- | :--------------------- | :--------------- |
|   [1]   | `CallInvokerExtensions.Intercept` | `static CallInvoker Intercept(this CallInvoker invoker, Interceptor interceptor)`              | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [2]   | `CallInvokerExtensions.Intercept` | `static CallInvoker Intercept(this CallInvoker invoker, params Interceptor[] interceptors)`    | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [3]   | `CallInvokerExtensions.Intercept` | `static CallInvoker Intercept(this CallInvoker invoker, Func<Metadata, Metadata> interceptor)` | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [4]   | `InterceptingCallInvoker.ctor`    | `InterceptingCallInvoker(CallInvoker invoker, Interceptor interceptor)`                        | remote-lane#CALL_SPINE | decompile 2.80.0 |

[ENTRYPOINT_SCOPE]: `AsyncAuthInterceptor` delegate and `AuthInterceptorContext` members
- source: `Grpc.Core.Api` 2.80.0 — `Grpc.Core.AsyncAuthInterceptor` / `Grpc.Core.AuthInterceptorContext` decompile
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                                   | [SIGNATURE]                                                                                         | [USED_BY]              | [EVIDENCE]       |
| :-----: | :----------------------------------------- | :-------------------------------------------------------------------------------------------------- | :--------------------- | :--------------- |
|   [1]   | `AsyncAuthInterceptor`                     | `delegate Task AsyncAuthInterceptor(AuthInterceptorContext context, Metadata metadata)`             | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [2]   | `AuthInterceptorContext.ctor`              | `AuthInterceptorContext(string serviceUrl, string methodName)`                                      | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [3]   | `AuthInterceptorContext.ctor`              | `AuthInterceptorContext(string serviceUrl, string methodName, CancellationToken cancellationToken)` | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [4]   | `AuthInterceptorContext.ServiceUrl`        | `string ServiceUrl { get; }`                                                                        | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [5]   | `AuthInterceptorContext.MethodName`        | `string MethodName { get; }`                                                                        | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [6]   | `AuthInterceptorContext.CancellationToken` | `CancellationToken CancellationToken { get; }`                                                      | remote-lane#CALL_SPINE | decompile 2.80.0 |

[ENTRYPOINT_SCOPE]: `SocketsHttpHandler` keepalive members (BCL — passed as `GrpcChannelOptions.HttpHandler`)
- source: BCL `System.Net.Http` net10.0 — `System.Net.Http.SocketsHttpHandler` decompile
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                                            | [SIGNATURE]                                                           | [USED_BY]              | [EVIDENCE]        |
| :-----: | :-------------------------------------------------- | :-------------------------------------------------------------------- | :--------------------- | :---------------- |
|   [1]   | `SocketsHttpHandler.KeepAlivePingDelay`             | `TimeSpan KeepAlivePingDelay { get; set; }`                           | remote-lane#CALL_SPINE | decompile net10.0 |
|   [2]   | `SocketsHttpHandler.KeepAlivePingTimeout`           | `TimeSpan KeepAlivePingTimeout { get; set; }`                         | remote-lane#CALL_SPINE | decompile net10.0 |
|   [3]   | `SocketsHttpHandler.KeepAlivePingPolicy`            | `HttpKeepAlivePingPolicy KeepAlivePingPolicy { get; set; }`           | remote-lane#CALL_SPINE | decompile net10.0 |
|   [4]   | `SocketsHttpHandler.EnableMultipleHttp2Connections` | `bool EnableMultipleHttp2Connections { get; set; }`                   | remote-lane#CALL_SPINE | decompile net10.0 |
|   [5]   | `HttpKeepAlivePingPolicy.WithActiveRequests`        | `WithActiveRequests = 0` — ping only when active requests are pending | remote-lane#CALL_SPINE | decompile net10.0 |
|   [6]   | `HttpKeepAlivePingPolicy.Always`                    | `Always = 1` — ping even on idle connections                          | remote-lane#CALL_SPINE | decompile net10.0 |

[ENTRYPOINT_SCOPE]: compression provider types
- source: `Grpc.Net.Common` 2.80.0 — `Grpc.Net.Compression` namespace decompile
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                                         | [SIGNATURE]                                                                         | [USED_BY]              | [EVIDENCE]       |
| :-----: | :----------------------------------------------- | :---------------------------------------------------------------------------------- | :--------------------- | :--------------- |
|   [1]   | `ICompressionProvider.EncodingName`              | `string EncodingName { get; }`                                                      | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [2]   | `ICompressionProvider.CreateCompressionStream`   | `Stream CreateCompressionStream(Stream stream, CompressionLevel? compressionLevel)` | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [3]   | `ICompressionProvider.CreateDecompressionStream` | `Stream CreateDecompressionStream(Stream stream)`                                   | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [4]   | `GzipCompressionProvider.ctor`                   | `GzipCompressionProvider(CompressionLevel defaultCompressionLevel)`                 | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [5]   | `GzipCompressionProvider.EncodingName`           | `string EncodingName { get; }` returns `"gzip"`                                     | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [6]   | `DeflateCompressionProvider.ctor`                | `DeflateCompressionProvider(CompressionLevel defaultCompressionLevel)`              | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [7]   | `DeflateCompressionProvider.EncodingName`        | `string EncodingName { get; }` returns `"deflate"`                                  | remote-lane#CALL_SPINE | decompile 2.80.0 |

[ENTRYPOINT_SCOPE]: `GrpcChannelOptions` full property surface
- source: `Grpc.Net.Client` 2.80.0 — `GrpcChannelOptions` decompile
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                                  | [SIGNATURE]                                                       | [USED_BY]              | [EVIDENCE]       |
| :-----: | :---------------------------------------- | :---------------------------------------------------------------- | :--------------------- | :--------------- |
|   [1]   | `Credentials`                             | `ChannelCredentials? Credentials { get; set; }`                   | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [2]   | `MaxSendMessageSize`                      | `int? MaxSendMessageSize { get; set; }`                           | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [3]   | `MaxReceiveMessageSize`                   | `int? MaxReceiveMessageSize { get; set; }`                        | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [4]   | `MaxRetryAttempts`                        | `int? MaxRetryAttempts { get; set; }`                             | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [5]   | `MaxRetryBufferSize`                      | `long? MaxRetryBufferSize { get; set; }`                          | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [6]   | `MaxRetryBufferPerCallSize`               | `long? MaxRetryBufferPerCallSize { get; set; }`                   | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [7]   | `CompressionProviders`                    | `IList<ICompressionProvider>? CompressionProviders { get; set; }` | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [8]   | `HttpClient`                              | `HttpClient? HttpClient { get; set; }`                            | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [9]   | `HttpHandler`                             | `HttpMessageHandler? HttpHandler { get; set; }`                   | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [10]   | `DisposeHttpClient`                       | `bool DisposeHttpClient { get; set; }`                            | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [11]   | `ThrowOperationCanceledOnCancellation`    | `bool ThrowOperationCanceledOnCancellation { get; set; }`         | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [12]   | `UnsafeUseInsecureChannelCallCredentials` | `bool UnsafeUseInsecureChannelCallCredentials { get; set; }`      | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [13]   | `ServiceConfig`                           | `ServiceConfig? ServiceConfig { get; set; }`                      | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [14]   | `DisableResolverServiceConfig`            | `bool DisableResolverServiceConfig { get; set; }`                 | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [15]   | `ServiceProvider`                         | `IServiceProvider? ServiceProvider { get; set; }`                 | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [16]   | `HttpVersion`                             | `Version? HttpVersion { get; set; }`                              | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [17]   | `HttpVersionPolicy`                       | `HttpVersionPolicy? HttpVersionPolicy { get; set; }`              | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [18]   | `LoggerFactory`                           | `ILoggerFactory? LoggerFactory { get; set; }`                     | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [19]   | `MaxReconnectBackoff`                     | `TimeSpan? MaxReconnectBackoff { get; set; }` — default 120 s     | remote-lane#CALL_SPINE | decompile 2.80.0 |
|  [20]   | `InitialReconnectBackoff`                 | `TimeSpan InitialReconnectBackoff { get; set; }` — default 1 s    | remote-lane#CALL_SPINE | decompile 2.80.0 |

[ENTRYPOINT_SCOPE]: policy and balancing operations
- rail: remote-client

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]    | [CAPABILITY]             |
| :-----: | :----------------------------- | :-------------- | :----------------------- |
|   [1]   | `RetryPolicy`                  | policy object   | sets retry bounds        |
|   [2]   | `HedgingPolicy`                | policy object   | sets hedge bounds        |
|   [3]   | `RoundRobinConfig`             | balancer config | selects round robin      |
|   [4]   | `PickFirstConfig`              | balancer config | selects first endpoint   |
|   [5]   | `Resolver.Start` / `Refresh`   | resolver call   | drives endpoint updates  |
|   [6]   | `Subchannel.RequestConnection` | channel call    | starts connection        |
|   [7]   | `SubchannelPicker.Pick`        | picker call     | selects subchannel       |
|   [8]   | `PickResult.ForSubchannel`     | result factory  | returns selected channel |
|   [9]   | `PickResult.ForFailure`        | result factory  | returns failed selection |

## [4]-[IMPLEMENTATION_LAW]

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
