# [RASM_COMPUTE_API_PROTOBUF]

`Google.Protobuf` supplies generated message contracts, parsers, codecs, repeated fields, maps, and JSON projection.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Google.Protobuf`
- package: `Google.Protobuf`
- assembly: `Google.Protobuf`
- namespace: `Google.Protobuf`
- asset: runtime library
- rail: remote-contracts

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: protobuf family
- rail: remote-contracts

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]      | [CAPABILITY]                      |
| :-----: | :---------------------- | :------------------ | :-------------------------------- |
|   [1]   | `IMessage<T>`           | contract surface    | defines boundary contract         |
|   [2]   | `MessageParser<T>`      | message parser      | anchors remote-contracts contract |
|   [3]   | `CodedInputStream`      | stream shape        | stages payload bytes              |
|   [4]   | `CodedOutputStream`     | stream shape        | stages payload bytes              |
|   [5]   | `ByteString`            | immutable bytes     | anchors remote-contracts contract |
|   [6]   | `FieldCodec<T>`         | field codec         | anchors remote-contracts contract |
|   [7]   | `RepeatedField<T>`      | repeated collection | anchors remote-contracts contract |
|   [8]   | `MapField<TKey,TValue>` | map collection      | anchors remote-contracts contract |
|   [9]   | `JsonFormatter`         | codec surface       | defines codec path                |
|  [10]   | `JsonParser`            | JSON parser         | anchors remote-contracts contract |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: protobuf operations
- rail: remote-contracts

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]       | [CAPABILITY]          |
| :-----: | :------------------------ | :----------------- | :-------------------- |
|   [1]   | `ParseFrom`               | operation call     | executes operation    |
|   [2]   | `WriteTo`                 | operation call     | executes operation    |
|   [3]   | `CalculateSize`           | size method        | measures encoded size |
|   [4]   | `MergeFrom`               | merge method       | merges message fields |
|   [5]   | `Clone`                   | copy method        | copies message value  |
|   [6]   | `ToByteArray`             | byte serialization | writes encoded bytes  |
|   [7]   | `WithFormatDefaultValues` | fluent option      | applies policy value  |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Google.Protobuf`
- Owns: wire message contracts
- Accept: remote payloads are generated contracts
- Reject: handwritten binary DTOs

