# [API_CATALOGUE] @connectrpc/connect-web

`@connectrpc/connect-web` supplies the two browser-native `Transport` implementations for the Connect and gRPC-web protocols: `createConnectTransport` and `createGrpcWebTransport`, both built on the Fetch API, carrying `baseUrl`, binary/JSON format selection, interceptors, timeout defaults, and optional custom `fetch` overrides.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@connectrpc/connect-web`
- package: `@connectrpc/connect-web`
- module: `@connectrpc/connect-web`
- asset: runtime library
- rail: transport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: transport option shapes
- rail: transport

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]     | [RAIL]                                    |
| :-----: | :------------------------ | :---------------- | :---------------------------------------- |
|  [01]   | `ConnectTransportOptions` | options interface | Connect-protocol transport configuration  |
|  [02]   | `GrpcWebTransportOptions` | options interface | gRPC-web-protocol transport configuration |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: transport factories
- rail: transport

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY]    | [RAIL]                                         |
| :-----: | :-------------------------------- | :---------------- | :--------------------------------------------- |
|  [01]   | `createConnectTransport(options)` | transport factory | `Transport` using Connect protocol over fetch  |
|  [02]   | `createGrpcWebTransport(options)` | transport factory | `Transport` using gRPC-web protocol over fetch |

## [04]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- `ConnectTransportOptions.baseUrl`: required; all requests go to `<baseUrl>/<pkg>.<Service>/Method`
- `ConnectTransportOptions.useBinaryFormat`: defaults `false` (JSON wire format); set `true` for binary protobuf
- `ConnectTransportOptions.useHttpGet`: defaults `false`; enables Connect GET for idempotent unary methods
- `GrpcWebTransportOptions.useBinaryFormat`: defaults `true` (binary wire format); not all gRPC-web servers support JSON
- both options carry `interceptors?: Interceptor[]`, `jsonOptions?`, `binaryOptions?`, `fetch?`, `defaultTimeoutMs?`
- `fetch` override allows injecting `credentials`, custom headers at the fetch layer, or a test mock
- gRPC-web transport does not implement `grpc-web-text` (base64 over XHR); uses binary framing over fetch

[LOCAL_ADMISSION]:
- Both factories return the `Transport` interface from `@connectrpc/connect`; call sites hold only `Transport`, not the concrete type.
- `defaultTimeoutMs` is per-transport floor; `CallOptions.timeoutMs` overrides per-request.
- `jsonOptions` and `binaryOptions` accept partial `JsonReadOptions & JsonWriteOptions` and `BinaryReadOptions & BinaryWriteOptions` from `@bufbuild/protobuf`.

[RAIL_LAW]:
- Package: `@connectrpc/connect-web`
- Owns: browser Transport implementations for Connect and gRPC-web
- Accept: descriptors and wire-format options from `@bufbuild/protobuf`; interceptors from `@connectrpc/connect`
- Reject: Node.js-only transports (use `@connectrpc/connect-node`); gRPC/H2 cleartext (use connect-node with H2)
