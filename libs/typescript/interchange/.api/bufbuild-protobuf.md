# [API_CATALOGUE] @bufbuild/protobuf

`@bufbuild/protobuf` supplies the descriptor type system, generated message runtime, and wire-codec operations for Protobuf editions 2023/2024/proto2/proto3: descriptor interfaces (`DescMessage`, `DescService`, `DescField`, …), type extractors (`MessageShape`, `MessageInitShape`, `EnumShape`), `create` for zero-value instantiation, `toBinary`/`fromBinary` for binary codec, `toJson`/`fromJson`/`toJsonString` for JSON codec, and `Registry`/`FileRegistry` for type lookup.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@bufbuild/protobuf`
- package: `@bufbuild/protobuf`
- module: `@bufbuild/protobuf` (barrel); deep imports at `@bufbuild/protobuf/codegenv1`, `@bufbuild/protobuf/codegenv2`, `@bufbuild/protobuf/reflect`, `@bufbuild/protobuf/wkt`, `@bufbuild/protobuf/wire`
- asset: runtime library
- rail: wire

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: descriptor family
- rail: wire

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]         | [RAIL]                              |
| :-----: | :-------------- | :-------------------- | :---------------------------------- |
|  [01]   | `DescFile`      | file descriptor       | proto file, edition, imports, types |
|  [02]   | `DescMessage`   | message descriptor    | fields, oneofs, nested types        |
|  [03]   | `DescField`     | field descriptor      | discriminated by `fieldKind`        |
|  [04]   | `DescEnum`      | enum descriptor       | values, open/closed flag            |
|  [05]   | `DescEnumValue` | enum-value descriptor | numeric value, localName            |
|  [06]   | `DescService`   | service descriptor    | methods map, typeName               |
|  [07]   | `DescMethod`    | method descriptor     | `methodKind`, input, output         |
|  [08]   | `DescOneof`     | oneof descriptor      | grouped fields                      |
|  [09]   | `DescExtension` | extension descriptor  | extendee, typeName                  |
|  [10]   | `AnyDesc`       | union of all descs    | kind-discriminated descriptor union |

[PUBLIC_TYPE_SCOPE]: method-kind typed descriptors
- rail: wire

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]        | [RAIL]                           |
| :-----: | :------------------------------- | :------------------- | :------------------------------- |
|  [01]   | `DescMethodUnary<I,O>`           | typed method desc    | `methodKind: "unary"`            |
|  [02]   | `DescMethodServerStreaming<I,O>` | typed method desc    | `methodKind: "server_streaming"` |
|  [03]   | `DescMethodClientStreaming<I,O>` | typed method desc    | `methodKind: "client_streaming"` |
|  [04]   | `DescMethodBiDiStreaming<I,O>`   | typed method desc    | `methodKind: "bidi_streaming"`   |
|  [05]   | `DescMethodStreaming<I,O>`       | union of 3 streaming | client/server/bidi union         |

[PUBLIC_TYPE_SCOPE]: message type extractors
- rail: wire

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]     | [RAIL]                                    |
| :-----: | :-------------------------- | :---------------- | :---------------------------------------- |
|  [01]   | `Message<TypeName>`         | base message type | `$typeName` + optional `$unknown` fields  |
|  [02]   | `MessageShape<Desc>`        | decoded shape     | runtime JS type for a `DescMessage`       |
|  [03]   | `MessageInitShape<Desc>`    | init shape        | partial init accepted by `create()`       |
|  [04]   | `MessageJsonType<Desc>`     | JSON shape        | JSON type (requires `json_types=true`)    |
|  [05]   | `EnumShape<Desc>`           | enum value type   | numeric or typed enum value               |
|  [06]   | `EnumJsonType<Desc>`        | enum JSON type    | string enum name or null                  |
|  [07]   | `ExtensionValueShape<Desc>` | extension value   | value type for an extension field         |
|  [08]   | `UnknownField`              | unknown field     | `no`, `wireType`, `data` for unknown data |

[PUBLIC_TYPE_SCOPE]: registry family
- rail: wire

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]    | [RAIL]                              |
| :-----: | :---------------- | :--------------- | :---------------------------------- |
|  [01]   | `Registry`        | read registry    | lookup by fully qualified type name |
|  [02]   | `MutableRegistry` | mutable registry | add/remove descriptors at runtime   |
|  [03]   | `FileRegistry`    | file registry    | registry plus file lookup by name   |

[PUBLIC_TYPE_SCOPE]: well-known types (`@bufbuild/protobuf/wkt`)
- rail: wire

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [RAIL]                                                                  |
| :-----: | :---------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `FieldMask`       | WKT message   | `paths: string[]` repeated dotted-path set; `google.protobuf.FieldMask` |
|  [02]   | `FieldMaskSchema` | `GenMessage`  | descriptor for `create`/`fromBinary`/`fromJson` of `FieldMask`          |
|  [03]   | `Timestamp`       | WKT message   | `seconds: bigint`, `nanos: number`; `google.protobuf.Timestamp`         |
|  [04]   | `TimestampSchema` | `GenMessage`  | descriptor for `Timestamp` instantiation                                |
|  [05]   | `Duration`        | WKT message   | `seconds: bigint`, `nanos: number`; `google.protobuf.Duration`          |
|  [06]   | `Any`             | WKT message   | `typeUrl: string`, `value: Uint8Array`; `google.protobuf.Any`           |

[PUBLIC_TYPE_SCOPE]: scalar types
- rail: wire

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [RAIL]                               |
| :-----: | :----------- | :------------ | :----------------------------------- |
|  [01]   | `ScalarType` | numeric enum  | 14 scalar field kinds (no GROUP/MSG) |

[PUBLIC_TYPE_SCOPE]: codec options
- rail: wire

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                                                 |
| :-----: | :----------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `BinaryWriteOptions`     | write options | `writeUnknownFields` flag                                              |
|  [02]   | `BinaryReadOptions`      | read options  | `readUnknownFields` flag                                               |
|  [03]   | `JsonWriteOptions`       | write options | `alwaysEmitImplicit`, `enumAsInteger`, `useProtoFieldName`, `registry` |
|  [04]   | `JsonReadOptions`        | read options  | `ignoreUnknownFields`, `registry`                                      |
|  [05]   | `JsonWriteStringOptions` | write options | `JsonWriteOptions` + `prettySpaces`                                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: message lifecycle
- rail: wire

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY]    | [RAIL]                                     |
| :-----: | :---------------------------------------------------- | :---------------- | :----------------------------------------- |
|  [01]   | `create(schema, init?)`                               | constructor       | zero-value message, optionally initialized |
|  [02]   | `toBinary(schema, message, options?)`                 | binary serializer | `Uint8Array` from message                  |
|  [03]   | `fromBinary(schema, bytes, options?)`                 | binary parser     | message from `Uint8Array`                  |
|  [04]   | `mergeFromBinary(schema, target, bytes, options?)`    | binary merge      | merge bytes into existing message          |
|  [05]   | `toJson(schema, message, options?)`                   | JSON serializer   | `JsonValue` from message                   |
|  [06]   | `toJsonString(schema, message, options?)`             | JSON serializer   | JSON string from message                   |
|  [07]   | `fromJson(schema, json, options?)`                    | JSON parser       | message from `JsonValue`                   |
|  [08]   | `fromJsonString(schema, json, options?)`              | JSON parser       | message from JSON string                   |
|  [09]   | `mergeFromJson(schema, target, json, options?)`       | JSON merge        | merge JSON into existing message           |
|  [10]   | `mergeFromJsonString(schema, target, json, options?)` | JSON merge        | merge JSON string into existing message    |
|  [11]   | `enumToJson(descEnum, value)`                         | enum serializer   | enum value to JSON string/null             |
|  [12]   | `enumFromJson(descEnum, json)`                        | enum parser       | JSON string/null to enum value             |

[ENTRYPOINT_SCOPE]: registry construction
- rail: wire

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY]   | [RAIL]                                      |
| :-----: | :-------------------------------------- | :--------------- | :------------------------------------------ |
|  [01]   | `createRegistry(...input)`              | registry factory | read `Registry` from descriptors            |
|  [02]   | `createMutableRegistry(...input)`       | registry factory | `MutableRegistry` with add/remove           |
|  [03]   | `createFileRegistry(fileDescriptorSet)` | file registry    | `FileRegistry` from `FileDescriptorSet`     |
|  [04]   | `createFileRegistry(proto, resolve)`    | file registry    | `FileRegistry` from single proto + resolver |
|  [05]   | `createFileRegistry(...registries)`     | file registry    | merged `FileRegistry` from registries       |

[ENTRYPOINT_SCOPE]: descriptor reflection walk
- rail: wire

The descriptor-evolution gate (`Contract/descriptor.md`) reads the descriptor surface through these iteration accessors and the `@bufbuild/protobuf/reflect` `nestedTypes` walker; `registry.files` is the file roster a `createFileRegistry(set)` yields, and every member below is a read-only property of the immutable descriptor interfaces.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY]   | [RAIL]                                                                          |
| :-----: | :----------------------------------------------------- | :--------------- | :------------------------------------------------------------------------------ |
|  [01]   | `registry.files`                                       | file roster      | iterable of `DescFile` the registry resolved                                    |
|  [02]   | `nestedTypes(file)`                                    | recursive walker | `@bufbuild/protobuf/reflect`; yields `type.kind` message/enum/extension/service |
|  [03]   | `file.messages` / `message.typeName`                   | message roster   | top-level `DescMessage` iterable; fully qualified name                          |
|  [04]   | `message.fields` / `field.name`                        | field roster     | declaration-order `DescField` iterable; source field name                       |
|  [05]   | `field.number` / `field.localName`                     | field identity   | proto field number; ECMAScript-safe accessor name                               |
|  [06]   | `field.fieldKind` / `field.listKind`                   | field kind       | `scalar`/`enum`/`message`/`list`/`map`; `list`-arm element kind                 |
|  [07]   | `file.enums` / `enum.values`                           | enum roster      | `DescEnum` iterable; `DescEnumValue` iterable with `value.name`                 |
|  [08]   | `file.services` / `service.methods`                    | service roster   | `DescService` iterable; `DescMethod` iterable with `method.name`                |
|  [09]   | `method.methodKind` / `method.input` / `method.output` | method shape     | `unary`/`server_streaming`/…; input and output `DescMessage`                    |
|  [10]   | `reflect(schema, message)`                             | message reflect  | `@bufbuild/protobuf/reflect`; `reflected.fields`, `clear(field)`, `getOption`   |

[ENTRYPOINT_SCOPE]: well-known-type helpers (`@bufbuild/protobuf/wkt`)
- rail: wire

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                                              |
| :-----: | :--------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `anyPack(schema, message)`         | Any pack       | wrap a typed message into `Any` (`typeUrl`+`value`) |
|  [02]   | `anyUnpack(any, registryOrSchema)` | Any unpack     | unwrap `Any` by registry or descriptor schema       |
|  [03]   | `anyUnpackTo(any, schema, target)` | Any unpack     | unwrap `Any` into an existing target message        |

## [04]-[IMPLEMENTATION_LAW]

[DESCRIPTOR_TOPOLOGY]:
- descriptor objects are immutable read-only interfaces; `DescField` is a discriminated union on `fieldKind`: `"scalar"`, `"message"`, `"enum"`, `"list"`, `"map"`
- `DescMessage.field` maps `localName` (ECMAScript-safe) to `DescField`; `DescService.method` maps `localName` to `DescMethod`
- `DescMessage` also exposes `fields` (declaration-order iterable), `typeName`, and the nested rosters; `DescField` carries `name`, `number`, `localName`, `fieldKind`, and the `list`-arm `listKind`; `DescMethod` carries `methodKind`, `input`, and `output` as `DescMessage` references; `DescEnum` carries `values` of `DescEnumValue` with `name`
- `nestedTypes(file)` from `@bufbuild/protobuf/reflect` recursively walks a `DescFile` yielding `type.kind` of `"message" | "enum" | "extension" | "service"`, the one walker that visits nested messages and enums without a per-level recursion
- generated code (from `protoc-gen-es`) produces `GenMessage<RuntimeShape>` objects that extend `DescMessage` and bind `MessageShape<Desc>` to the concrete class
- `SupportedEdition` covers `EDITION_PROTO2`, `EDITION_PROTO3`, `EDITION_2023`, `EDITION_2024`; no other editions are accepted at runtime

[DESCRIPTOR_DIFF_GAP]:
- Resolved spellings (grounded against the `@bufbuild/protobuf` v2 descriptor source): `FileRegistry.files` is `Iterable<DescFile>` (not an array — materialize before `Array` combinators); `nestedTypes(file)` yields `Iterable<DescMessage | DescEnum | DescExtension | DescService>` discriminated on `kind`; `field.jsonName` is on the `DescField` common base; `field.oneof` (`DescOneof | undefined`) is carried ONLY on the singular `scalar`/`enum`/`message` arms, NOT on `list`/`map` — narrow on `fieldKind` before access; `message.oneofs` is `DescOneof[]` and `oneof.fields` is `DescField[]`; `enum.values` is `DescEnumValue[]` with `value.name`/`value.number`; `method.input`/`method.output` are `DescMessage` with `typeName`.
- Reserved ranges live on the raw proto, NOT a top-level descriptor member: `message.proto.reservedRange` is `DescriptorProto_ReservedRange[]` with `start: number` (inclusive) and `end: number` (exclusive) — there is NO `message.reservedRanges` accessor and NO `range.from`/`range.to`.
- Packed posture has no direct mirror of the C# `FieldDescriptor.IsPacked`: read `field.proto.options?.packed` against the raw `FieldDescriptorProto` (a repeated scalar defaults packed unless `options.packed === false`).
- Disk gap: the `@bufbuild/protobuf@2.12.0` pnpm store entry is a content-less stub on disk, so these spellings are source-grounded but not yet re-confirmed against the materialized `.d.ts`.
- Close when: the descriptor `.d.ts` is materialized in `node_modules`, or the emitted `src/gen/descriptor_pb.ts` confirms the spellings, at `surfaceRowsOf` transcription-lock.

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
