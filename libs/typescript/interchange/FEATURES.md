# [INTERCHANGE_FEATURES]

The realized capability list for the wire boundary. Every feature is a row or case on a budgeted owner, never a new surface; mechanics live at the `.planning/` page#cluster anchor named on each row, and the owner's realization state is read from `ARCHITECTURE.md` `[OWNER_REGISTRY]`.

## [1]-[TRANSPORT_AND_CODEGEN]

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]                  |
| :-----: | :----------------------------------------------------------------------------- | :----------------------------- |
|   [1]   | One shared grpc-web transport with one polymorphic interceptor stamp            | transport#TRANSPORT_AND_CLIENTS |
|   [2]   | Four browser-dialable generated clients off one descriptor set                  | transport#TRANSPORT_AND_CLIENTS |
|   [3]   | Transport-capability tuple keyed by method kind across http2 and grpcWeb        | transport#TRANSPORT_AND_CLIENTS |
|   [4]   | Chunked frame transport: server-stream-down suspend, unary-chunked-up sequence  | transport#TRANSPORT_AND_CLIENTS |
|   [5]   | Capability SDK codegen leg: descriptor catalog, polymorphic invoke, MCP projection | transport#CODEGEN_TOOLING    |
|   [6]   | Build-time descriptor codegen edge emitting the generated module set             | transport#CODEGEN_TOOLING       |

## [2]-[CODEC_RAILS_AND_FAULTS]

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]           |
| :-----: | :----------------------------------------------------------------------------- | :----------------------- |
|   [7]   | Six-codec polymorphic decode/encode rail family keyed by codec                  | codec-rails#CODEC_RAILS  |
|   [8]   | Decode-enforcement brands: guid, contentKey, ordinal, hlcLogical, discriminant  | codec-rails#CODEC_RAILS  |
|   [9]   | Embedded geometry GeoJSON projection off the proto payload oneof                 | codec-rails#CODEC_RAILS  |
|  [10]   | Content-addressed artifact-frame reassembly with 64-KiB chunks and digest check  | codec-rails#CODEC_RAILS  |
|  [11]   | Exhaustive fault reconstruction bound as `Match.tagsExhaustive` over the full set | codec-rails#FAULT_FAMILY |

## [3]-[GATEWAY_AND_QUARANTINE]

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]                                |
| :-----: | :----------------------------------------------------------------------------- | :------------------------------------------- |
|  [12]   | Contract-drift quarantine fold over unknown/disconnect/additive/breaking         | gateway-and-quarantine#GATEWAY_AND_QUARANTINE |
|  [13]   | Canonical contract inventory mapping every consumed wire page                    | gateway-and-quarantine#CONTRACT_INVENTORY     |
|  [14]   | Outbound command gateway: captureSupport, setDegradation, reloadOptions verbs    | gateway-and-quarantine#GATEWAY_AND_QUARANTINE |
|  [15]   | Deep-link intent registry mapping stable string keys to gateway verb + payload   | gateway-and-quarantine#GATEWAY_AND_QUARANTINE |
