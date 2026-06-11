# [RASM_COMPUTE_API_GRPC_CLIENT]

`Grpc.Net.Client` supplies HTTP/2 client channels, channel options, call invokers, deadlines, and client transport credentials.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Net.Client`
- package: `Grpc.Net.Client`
- assembly: `Grpc.Net.Client`
- namespace: `Grpc.Net.Client`
- asset: runtime library
- rail: remote

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: gRPC client family
- rail: remote

| [INDEX] | [SYMBOL]                                       | [PACKAGE_ROLE]  | [CAPABILITY]              |
| :-----: | :--------------------------------------------- | :-------------- | :------------------------ |
|   [1]   | `GrpcChannel`                                  | client channel  | owns transport state      |
|   [2]   | `GrpcChannelOptions`                           | channel options | carries policy input      |
|   [3]   | `GrpcChannel.ForAddress`                       | channel factory | creates configured handle |
|   [4]   | `GrpcChannel.CreateCallInvoker`                | invoker factory | creates configured handle |
|   [5]   | `CallOptions`                                  | call contract   | carries policy input      |
|   [6]   | `AsyncUnaryCall<TResponse>`                    | call contract   | anchors remote contract   |
|   [7]   | `AsyncServerStreamingCall<TResponse>`          | call contract   | stages payload bytes      |
|   [8]   | `AsyncClientStreamingCall<TRequest,TResponse>` | call contract   | stages payload bytes      |
|   [9]   | `AsyncDuplexStreamingCall<TRequest,TResponse>` | call contract   | stages payload bytes      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: gRPC operations
- rail: remote

| [INDEX] | [SURFACE]               | [CALL_SHAPE]    | [CAPABILITY]              |
| :-----: | :---------------------- | :-------------- | :------------------------ |
|   [1]   | `ForAddress`            | factory call    | creates configured handle |
|   [2]   | `CreateCallInvoker`     | factory call    | creates configured handle |
|   [3]   | `ShutdownAsync`         | async operation | executes async work       |
|   [4]   | `WithDeadline`          | call option     | applies policy value      |
|   [5]   | `WithCancellationToken` | call option     | applies policy value      |
|   [6]   | `ResponseStream`        | call stream     | reads server stream       |
|   [7]   | `RequestStream`         | call stream     | writes client stream      |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Grpc.Net.Client`
- Owns: remote client transport
- Accept: remote lanes consume AppHost policy through client channels and call contracts
- Reject: server-side gRPC package
