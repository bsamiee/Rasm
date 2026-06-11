# [RASM_COMPUTE_API_REMOTE]

Remote APIs supply gRPC client execution, protobuf messages, protocol generation, endpoint identity, payload bounds, deadlines, and retry-owner receipts.

## [1]-[SURFACES]

This table is a lookup by remote package.

| [INDEX] | [PACKAGE]          | [ASSEMBLY]          | [LOCAL_RAIL] |
| :-----: | :----------------- | :------------------ | :----------- |
|   [1]   | `Grpc.Net.Client`  | `Grpc.Net.Client`   | remote       |
|   [2]   | `Google.Protobuf`  | `Google.Protobuf`   | remote       |
|   [3]   | `Grpc.Tools`       | generation tool     | proto        |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]         | [NAMESPACE]        | [USING]             | [API_LOCATOR] |
| :-----: | :----------------- | :----------------- | :------------------ | :------------ |
|   [1]   | `Grpc.Net.Client`  | `Grpc.Net.Client`  | `Grpc.Net.Client`   | `.cache/nuget/packages/grpc.net.client/` |
|   [2]   | `Google.Protobuf`  | `Google.Protobuf`  | `Google.Protobuf`   | `.cache/nuget/packages/google.protobuf/` |
|   [3]   | `Grpc.Tools`       | generated MSBuild  | project item metadata | `.cache/nuget/packages/grpc.tools/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]      | [ENTRY_SURFACE]            | [LOCAL_RAIL] |
| :-----: | :----------------- | :------------------------- | :----------- |
|   [1]   | `GrpcChannel`      | remote client channel      | remote       |
|   [2]   | generated clients  | typed remote operation     | remote       |
|   [3]   | protobuf messages  | payload contract           | remote       |
|   [4]   | protobuf codecs    | message serialization      | remote       |
|   [5]   | proto generation   | generated service contract | proto        |

## [4]-[REJECTED]

This table is a lookup by rejected package.

| [INDEX] | [REJECT]            | [LOCAL_RAIL] | [REASON]                |
| :-----: | :------------------ | :----------- | :---------------------- |
|   [1]   | server-side gRPC    | remote       | companion owns server   |
|   [2]   | compute-local retry | remote       | AppHost owns retry      |
