# [API_CATALOGUE] @bufbuild/protobuf

`@bufbuild/protobuf` supplies the descriptor type system, generated message runtime, and wire-codec operations for Protobuf editions 2023/2024/proto2/proto3: descriptor interfaces (`DescMessage`, `DescService`, `DescField`, …), type extractors (`MessageShape`, `MessageInitShape`, `EnumShape`), `create` for zero-value instantiation, `toBinary`/`fromBinary` for binary codec, `toJson`/`fromJson`/`toJsonString` for JSON codec, and `Registry`/`FileRegistry` for type lookup.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@bufbuild/protobuf`
- package: `@bufbuild/protobuf`
- module: `@bufbuild/protobuf` (barrel); deep imports at `@bufbuild/protobuf/codegenv1`, `@bufbuild/protobuf/codegenv2`, `@bufbuild/protobuf/reflect`, `@bufbuild/protobuf/wkt`, `@bufbuild/protobuf/wire`
- asset: runtime library
- rail: wire

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: descriptor family
- rail: wire

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]         | [RAIL]                              |
| :-----: | :-------------- | :-------------------- | :---------------------------------- |
|   [1]   | `DescFile`      | file descriptor       | proto file, edition, imports, types |
|   [2]   | `DescMessage`   | message descriptor    | fields, oneofs, nested types        |
|   [3]   | `DescField`     | field descriptor      | discriminated by `fieldKind`        |
|   [4]   | `DescEnum`      | enum descriptor       | values, open/closed flag            |
|   [5]   | `DescEnumValue` | enum-value descriptor | numeric value, localName            |
|   [6]   | `DescService`   | service descriptor    | methods map, typeName               |
|   [7]   | `DescMethod`    | method descriptor     | `methodKind`, input, output         |
|   [8]   | `DescOneof`     | oneof descriptor      | grouped fields                      |
|   [9]   | `DescExtension` | extension descriptor  | extendee, typeName                  |
|  [10]   | `AnyDesc`       | union of all descs    | kind-discriminated descriptor union |

[PUBLIC_TYPE_SCOPE]: method-kind typed descriptors
- rail: wire

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]        | [RAIL]                           |
| :-----: | :------------------------------- | :------------------- | :------------------------------- |
|   [1]   | `DescMethodUnary<I,O>`           | typed method desc    | `methodKind: "unary"`            |
|   [2]   | `DescMethodServerStreaming<I,O>` | typed method desc    | `methodKind: "server_streaming"` |
|   [3]   | `DescMethodClientStreaming<I,O>` | typed method desc    | `methodKind: "client_streaming"` |
|   [4]   | `DescMethodBiDiStreaming<I,O>`   | typed method desc    | `methodKind: "bidi_streaming"`   |
|   [5]   | `DescMethodStreaming<I,O>`       | union of 3 streaming | client/server/bidi union         |

[PUBLIC_TYPE_SCOPE]: message type extractors
- rail: wire

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]     | [RAIL]                                    |
| :-----: | :-------------------------- | :---------------- | :---------------------------------------- |
|   [1]   | `Message<TypeName>`         | base message type | `$typeName` + optional `$unknown` fields  |
|   [2]   | `MessageShape<Desc>`        | decoded shape     | runtime JS type for a `DescMessage`       |
|   [3]   | `MessageInitShape<Desc>`    | init shape        | partial init accepted by `create()`       |
|   [4]   | `MessageJsonType<Desc>`     | JSON shape        | JSON type (requires `json_types=true`)    |
|   [5]   | `EnumShape<Desc>`           | enum value type   | numeric or typed enum value               |
|   [6]   | `EnumJsonType<Desc>`        | enum JSON type    | string enum name or null                  |
|   [7]   | `ExtensionValueShape<Desc>` | extension value   | value type for an extension field         |
|   [8]   | `UnknownField`              | unknown field     | `no`, `wireType`, `data` for unknown data |

[PUBLIC_TYPE_SCOPE]: registry family
- rail: wire

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]    | [RAIL]                              |
| :-----: | :---------------- | :--------------- | :---------------------------------- |
|   [1]   | `Registry`        | read registry    | lookup by fully qualified type name |
|   [2]   | `MutableRegistry` | mutable registry | add/remove descriptors at runtime   |
|   [3]   | `FileRegistry`    | file registry    | registry plus file lookup by name   |

[PUBLIC_TYPE_SCOPE]: well-known types (`@bufbuild/protobuf/wkt`)
- rail: wire

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [RAIL]                                                                  |
| :-----: | :---------------- | :------------ | :---------------------------------------------------------------------- |
|   [1]   | `FieldMask`       | WKT message   | `paths: string[]` repeated dotted-path set; `google.protobuf.FieldMask` |
|   [2]   | `FieldMaskSchema` | `GenMessage`  | descriptor for `create`/`fromBinary`/`fromJson` of `FieldMask`          |
|   [3]   | `Timestamp`       | WKT message   | `seconds: bigint`, `nanos: number`; `google.protobuf.Timestamp`         |
|   [4]   | `TimestampSchema` | `GenMessage`  | descriptor for `Timestamp` instantiation                                |
|   [5]   | `Duration`        | WKT message   | `seconds: bigint`, `nanos: number`; `google.protobuf.Duration`          |
|   [6]   | `Any`             | WKT message   | `typeUrl: string`, `value: Uint8Array`; `google.protobuf.Any`           |

[PUBLIC_TYPE_SCOPE]: scalar types
- rail: wire

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [RAIL]                               |
| :-----: | :----------- | :------------ | :----------------------------------- |
|   [1]   | `ScalarType` | numeric enum  | 14 scalar field kinds (no GROUP/MSG) |

[PUBLIC_TYPE_SCOPE]: codec options
- rail: wire

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                                                 |
| :-----: | :----------------------- | :------------ | :--------------------------------------------------------------------- |
|   [1]   | `BinaryWriteOptions`     | write options | `writeUnknownFields` flag                                              |
|   [2]   | `BinaryReadOptions`      | read options  | `readUnknownFields` flag                                               |
|   [3]   | `JsonWriteOptions`       | write options | `alwaysEmitImplicit`, `enumAsInteger`, `useProtoFieldName`, `registry` |
|   [4]   | `JsonReadOptions`        | read options  | `ignoreUnknownFields`, `registry`                                      |
|   [5]   | `JsonWriteStringOptions` | write options | `JsonWriteOptions` + `prettySpaces`                                    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: message lifecycle
- rail: wire

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY]    | [RAIL]                                     |
| :-----: | :---------------------------------------------------- | :---------------- | :----------------------------------------- |
|   [1]   | `create(schema, init?)`                               | constructor       | zero-value message, optionally initialized |
|   [2]   | `toBinary(schema, message, options?)`                 | binary serializer | `Uint8Array` from message                  |
|   [3]   | `fromBinary(schema, bytes, options?)`                 | binary parser     | message from `Uint8Array`                  |
|   [4]   | `mergeFromBinary(schema, target, bytes, options?)`    | binary merge      | merge bytes into existing message          |
|   [5]   | `toJson(schema, message, options?)`                   | JSON serializer   | `JsonValue` from message                   |
|   [6]   | `toJsonString(schema, message, options?)`             | JSON serializer   | JSON string from message                   |
|   [7]   | `fromJson(schema, json, options?)`                    | JSON parser       | message from `JsonValue`                   |
|   [8]   | `fromJsonString(schema, json, options?)`              | JSON parser       | message from JSON string                   |
|   [9]   | `mergeFromJson(schema, target, json, options?)`       | JSON merge        | merge JSON into existing message           |
|  [10]   | `mergeFromJsonString(schema, target, json, options?)` | JSON merge        | merge JSON string into existing message    |
|  [11]   | `enumToJson(descEnum, value)`                         | enum serializer   | enum value to JSON string/null             |
|  [12]   | `enumFromJson(descEnum, json)`                        | enum parser       | JSON string/null to enum value             |

[ENTRYPOINT_SCOPE]: registry construction
- rail: wire

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY]   | [RAIL]                                      |
| :-----: | :-------------------------------------- | :--------------- | :------------------------------------------ |
|   [1]   | `createRegistry(...input)`              | registry factory | read `Registry` from descriptors            |
|   [2]   | `createMutableRegistry(...input)`       | registry factory | `MutableRegistry` with add/remove           |
|   [3]   | `createFileRegistry(fileDescriptorSet)` | file registry    | `FileRegistry` from `FileDescriptorSet`     |
|   [4]   | `createFileRegistry(proto, resolve)`    | file registry    | `FileRegistry` from single proto + resolver |
|   [5]   | `createFileRegistry(...registries)`     | file registry    | merged `FileRegistry` from registries       |

[ENTRYPOINT_SCOPE]: well-known-type helpers (`@bufbuild/protobuf/wkt`)
- rail: wire

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                                              |
| :-----: | :--------------------------------- | :------------- | :-------------------------------------------------- |
|   [1]   | `anyPack(schema, message)`         | Any pack       | wrap a typed message into `Any` (`typeUrl`+`value`) |
|   [2]   | `anyUnpack(any, registryOrSchema)` | Any unpack     | unwrap `Any` by registry or descriptor schema       |
|   [3]   | `anyUnpackTo(any, schema, target)` | Any unpack     | unwrap `Any` into an existing target message        |

## [4]-[IMPLEMENTATION_LAW]

[DESCRIPTOR_TOPOLOGY]:
- descriptor objects are immutable read-only interfaces; `DescField` is a discriminated union on `fieldKind`: `"scalar"`, `"message"`, `"enum"`, `"list"`, `"map"`
- `DescMessage.field` maps `localName` (ECMAScript-safe) to `DescField`; `DescService.method` maps `localName` to `DescMethod`
- generated code (from `protoc-gen-es`) produces `GenMessage<RuntimeShape>` objects that extend `DescMessage` and bind `MessageShape<Desc>` to the concrete class
- `SupportedEdition` covers `EDITION_PROTO2`, `EDITION_PROTO3`, `EDITION_2023`, `EDITION_2024`; no other editions are accepted at runtime

[LOCAL_ADMISSION]:
- `create(schema, init?)` is the single message instantiation entry; never `new MessageClass()` — the generated class is the descriptor, not a constructor.
- `toBinary`/`fromBinary` are the canonical binary wire operations; `BinaryWriteOptions.writeUnknownFields` defaults `true` (round-trip preserves unknown fields).
- `toJson`/`fromJson` require `registry` in options only for `google.protobuf.Any` and extension round-trips; omit registry otherwise.
- `Registry` from `createRegistry` covers type lookup by fully-qualified name; `FileRegistry` adds file-level lookup and is required when resolving `FileDescriptorProto` imports.

[RAIL_LAW]:
- Package: `@bufbuild/protobuf`
- Owns: descriptor type system, message instantiation, binary codec, JSON codec, registry
- Accept: generated descriptor objects from `protoc-gen-es`; `FileDescriptorSet` from the protobuf compiler
- Reject: hand-rolled binary encoding, manual field number handling, custom message base classes
