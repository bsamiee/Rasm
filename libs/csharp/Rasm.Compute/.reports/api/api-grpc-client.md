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

[ENTRYPOINT_SCOPE]: policy and balancing operations
- rail: remote-client

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]    | [CAPABILITY]             |
| :-----: | :----------------------------- | :-------------- | :----------------------- |
|   [1]   | `RetryPolicy`                  | policy object   | sets retry bounds        |
|   [2]   | `HedgingPolicy`                | policy object   | sets hedge bounds        |
|   [3]   | `RoundRobinConfig`             | balancer config | selects round robin      |
|   [4]   | `PickFirstConfig`              | balancer config | selects first endpoint   |
|   [5]   | `Resolver.ResolveAsync`        | resolver call   | resolves endpoint set    |
|   [6]   | `Subchannel.RequestConnection` | channel call    | starts connection        |
|   [7]   | `SubchannelPicker.Pick`        | picker call     | selects subchannel       |
|   [8]   | `PickResult.ForComplete`       | result factory  | returns selected call    |
|   [9]   | `PickResult.ForFailure`        | result factory  | returns failed selection |

## [4]-[IMPLEMENTATION_LAW]

[CHANNEL_POLICY]:
- namespace: `Grpc.Net.Client`
- channel root: `GrpcChannel`
- policy root: `GrpcChannelOptions`
- transport root: HTTP handler selection stays explicit at channel creation
- payload bounds: send and receive message sizes are part of remote execution policy

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
