# [API_CATALOGUE] @bufbuild/protobuf

`@bufbuild/protobuf` is the descriptor runtime, message-instance model, and wire/JSON/text codec for Protobuf editions proto2/proto3/2023/2024, and the substrate every interchange rail folds onto: `create`/`clone`/`equals`/`isMessage` on the instance model, `toBinary`/`fromBinary`/`toJson`/`fromJson` codec pairs, the `@bufbuild/protobuf/reflect` descriptor walk the `descriptor.md` evolution gate classifies, the `@bufbuild/protobuf/wire` `sizeDelimitedDecodeStream`/`BinaryReader`/`base64Encode` low-level surface the segment-stream and framing rails compose, and the `@bufbuild/protobuf/wkt` `FieldMask`/`Any`/`Timestamp`/`Struct` well-known types the patch and codec rails lower.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@bufbuild/protobuf`
- package: `@bufbuild/protobuf` (2.12.1, Apache-2.0 AND BSD-3-Clause)
- module format: dual ESM (`dist/esm`) + CJS (`dist/commonjs`), `type: module`; barrel export `.` plus five deep subpaths `./codegenv2`, `./reflect`, `./wkt`, `./wire`, `./codegenv1` (legacy)
- runtime target: isomorphic (browser, node, worker); no native addon, no `engines` floor; 64-bit scalar fields decode to `bigint`, so a V8 BigInt runtime is load-bearing
- asset: pure-TypeScript runtime library shipping `.js` + content-ful `.d.ts`; the `2.12.1` store entry materializes the full descriptor/reflection declarations (`descriptors.d.ts` `DescField` five-arm union, `registry.d.ts` `FileRegistry`), so those spellings are disk-confirmed, not source-inferred
- rail: wire — descriptor runtime, instance model, binary/JSON/text codec, registry, reflection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: descriptor family (immutable read-only interfaces)
- rail: wire

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]         | [BOUNDARY_NOTE]                                                       |
| :-----: | :-------------- | :-------------------- | :------------------------------------------------------------------- |
|  [01]   | `DescFile`      | file descriptor       | `edition`, `dependencies`, `messages`, `enums`, `services`, `proto`  |
|  [02]   | `DescMessage`   | message descriptor    | `fields`/`field`/`oneofs`/`members`/`nestedMessages`/`typeName`/`proto` |
|  [03]   | `DescField`     | field descriptor      | 5-arm union discriminated on `fieldKind`                             |
|  [04]   | `DescEnum`      | enum descriptor       | `values` (`DescEnumValue[]`), `open` flag, `typeName`               |
|  [05]   | `DescEnumValue` | enum-value descriptor | `name`, `number`, `localName`                                        |
|  [06]   | `DescService`   | service descriptor    | `methods` (`DescMethod[]`), `method` (localName map), `typeName`    |
|  [07]   | `DescMethod`    | method descriptor     | `methodKind`, `input`/`output` (`DescMessage`)                      |
|  [08]   | `DescOneof`     | oneof descriptor      | `fields` (`DescField[]`), grouped members                          |
|  [09]   | `DescExtension` | extension descriptor  | `extendee`, `typeName`, `oneof`-stripped field arms                |
|  [10]   | `DescComments`  | source comments       | `leading`/`trailing`/`leadingDetached` source doc                  |
|  [11]   | `AnyDesc`       | descriptor union      | `kind`-discriminated union of all nine `Desc*`                     |
|  [12]   | `SupportedEdition` | edition subset     | `EDITION_PROTO2\|PROTO3\|2023\|2024`; runtime rejects others        |

[PUBLIC_TYPE_SCOPE]: method-kind typed descriptors
- rail: wire

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]        | [BOUNDARY_NOTE]                  |
| :-----: | :------------------------------- | :------------------- | :------------------------------- |
|  [01]   | `DescMethodUnary<I,O>`           | typed method desc    | `methodKind: "unary"`            |
|  [02]   | `DescMethodServerStreaming<I,O>` | typed method desc    | `methodKind: "server_streaming"` |
|  [03]   | `DescMethodClientStreaming<I,O>` | typed method desc    | `methodKind: "client_streaming"` |
|  [04]   | `DescMethodBiDiStreaming<I,O>`   | typed method desc    | `methodKind: "bidi_streaming"`   |
|  [05]   | `DescMethodStreaming<I,O>`       | union of 3 streaming | client/server/bidi union         |

[PUBLIC_TYPE_SCOPE]: message type extractors
- rail: wire

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]     | [BOUNDARY_NOTE]                                    |
| :-----: | :-------------------------- | :---------------- | :------------------------------------------------ |
|  [01]   | `Message<TypeName>`         | base message type | `$typeName` brand + optional `$unknown` fields    |
|  [02]   | `MessageShape<Desc>`        | decoded shape     | runtime JS type `create`/`fromBinary` yield        |
|  [03]   | `MessageInitShape<Desc>`    | init shape        | partial init `create()` accepts                    |
|  [04]   | `MessageJsonType<Desc>`     | JSON shape        | JSON type (requires `json_types=true` codegen)     |
|  [05]   | `MessageValidType<Desc>`    | valid shape       | required-field-narrowed type (`valid_types` codegen) |
|  [06]   | `EnumShape<Desc>`           | enum value type   | numeric or typed enum value                        |
|  [07]   | `EnumJsonType<Desc>`        | enum JSON type    | string enum name or `null`                         |
|  [08]   | `ExtensionValueShape<Desc>` | extension value   | value type for an extension field                  |
|  [09]   | `Extendee<Desc>`            | extendee message  | message an extension extends                        |
|  [10]   | `UnknownField`              | unknown field     | `no`, `wireType`, `data`; preserved across codec   |

[PUBLIC_TYPE_SCOPE]: registry, scalar, and codec options
- rail: wire

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]    | [BOUNDARY_NOTE]                                                        |
| :-----: | :----------------------- | :--------------- | :-------------------------------------------------------------------- |
|  [01]   | `Registry`               | read registry    | `get(name)` + `getMessage`/`getEnum`/`getExtension`/`getService`/`getExtensionFor`; `[Symbol.iterator]` |
|  [02]   | `MutableRegistry`        | mutable registry | `add(desc)`/`remove(desc)` over a `Registry`                          |
|  [03]   | `FileRegistry`           | file registry    | `Registry` + `files` (`Iterable<DescFile>`) + `getFile(name)`         |
|  [04]   | `ScalarType`             | numeric enum     | 14 scalar field kinds (`DOUBLE`…`SINT64`); no `GROUP`/`MESSAGE`       |
|  [05]   | `BinaryWriteOptions`     | write options    | `writeUnknownFields` (default `true`)                                 |
|  [06]   | `BinaryReadOptions`      | read options     | `readUnknownFields` (default `true`), `readerFactory`                 |
|  [07]   | `JsonWriteOptions`       | write options    | `alwaysEmitImplicit`, `enumAsInteger`, `useProtoFieldName`, `registry` |
|  [08]   | `JsonReadOptions`        | read options     | `ignoreUnknownFields`, `registry`                                     |
|  [09]   | `JsonWriteStringOptions` | write options    | `JsonWriteOptions` + `prettySpaces`                                   |
|  [10]   | `JsonValue` / `JsonObject` | JSON carrier   | codec-neutral JSON value the `toJson`/`fromJson` pair moves            |

[PUBLIC_TYPE_SCOPE]: generated-code runtime (`@bufbuild/protobuf/codegenv2`)
- rail: wire

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]    | [BOUNDARY_NOTE]                                                     |
| :-----: | :------------------ | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `GenFile`           | file token       | alias of `DescFile` the emitted `_pb.ts` binds                     |
|  [02]   | `GenMessage<S,Opt>` | message token    | `DescMessage` branded to runtime shape `S`; the `<Name>Schema` type |
|  [03]   | `GenEnum<S,J>`      | enum token       | `DescEnum` branded to numeric shape + JSON type                    |
|  [04]   | `GenExtension<E,V>` | extension token  | `DescExtension` branded to extendee + value                        |
|  [05]   | `GenService<M>`     | service token    | `DescService` branded to `GenServiceMethods`; feeds `createClient` |
|  [06]   | `GenServiceMethods` | method map       | `Record<name, Pick<DescMethod,"input"\|"output"\|"methodKind">>`    |

[PUBLIC_TYPE_SCOPE]: reflection surface (`@bufbuild/protobuf/reflect`)
- rail: wire

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]      | [BOUNDARY_NOTE]                                                  |
| :-----: | :----------------- | :----------------- | :-------------------------------------------------------------- |
|  [01]   | `ReflectMessage`   | dynamic message    | `desc`, `fields`, `get`/`set`/`clear`/`isSet`/`findNumber`      |
|  [02]   | `ReflectList<V>`   | dynamic list       | `Iterable<V>`; `size`, `get`, `add`, `set`                     |
|  [03]   | `ReflectMap<K,V>`  | dynamic map        | `ReadonlyMap<K,V>` + `set`/`delete` over field entries          |
|  [04]   | `OneofADT`         | oneof algebra      | `{ case: undefined } \| { case: name; value }` discriminant      |
|  [05]   | `Path` / `PathBuilder` | typed field path | descriptor-typed path steps `buildPath`/`parsePath` produce      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: message lifecycle and equality
- rail: wire

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]    | [BOUNDARY_NOTE]                                             |
| :-----: | :---------------------------------------------- | :---------------- | :--------------------------------------------------------- |
|  [01]   | `create(schema, init?)`                         | constructor       | zero-value `MessageShape`, optionally `MessageInitShape`   |
|  [02]   | `clone(schema, message)`                        | deep copy         | structural copy; the pre-patch snapshot for rollback       |
|  [03]   | `equals(schema, a, b, options?)`                | structural eq     | field-wise equality; `EqualsOptions.unpackAny`             |
|  [04]   | `isMessage(arg, schema?)`                       | type guard        | narrows `unknown` to `MessageShape<Desc>` by `$typeName`   |
|  [05]   | `isFieldSet(message, field)`                    | presence probe    | explicit-presence test for a `DescField`                   |
|  [06]   | `clearField(message, field)`                    | field clear       | reset one field to its zero/unset value                    |

[ENTRYPOINT_SCOPE]: binary and JSON codec pairs
- rail: wire

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY]    | [BOUNDARY_NOTE]                              |
| :-----: | :---------------------------------------------------- | :---------------- | :------------------------------------------ |
|  [01]   | `toBinary(schema, message, options?)`                 | binary serializer | `Uint8Array`; `writeUnknownFields` default `true` |
|  [02]   | `fromBinary(schema, bytes, options?)`                 | binary parser     | `MessageShape` from `Uint8Array`            |
|  [03]   | `mergeFromBinary(schema, target, bytes, options?)`    | binary merge      | merge bytes into an existing message        |
|  [04]   | `toJson(schema, message, options?)`                   | JSON serializer   | `JsonValue`; `registry` needed for `Any`    |
|  [05]   | `toJsonString(schema, message, options?)`             | JSON serializer   | JSON string; `prettySpaces` via options     |
|  [06]   | `fromJson(schema, json, options?)`                    | JSON parser       | `MessageShape` from `JsonValue`             |
|  [07]   | `fromJsonString(schema, json, options?)`              | JSON parser       | `MessageShape` from JSON string             |
|  [08]   | `mergeFromJson` / `mergeFromJsonString`               | JSON merge        | merge JSON value/string into a target       |
|  [09]   | `enumToJson(descEnum, value)` / `enumFromJson(descEnum, json)` | enum codec | enum ↔ JSON string/null                      |
|  [10]   | `isEnumJson(descEnum, value)`                         | enum guard        | narrows `unknown` to `EnumJsonType`         |

[ENTRYPOINT_SCOPE]: extensions and Int64 support
- rail: wire

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [BOUNDARY_NOTE]                                        |
| :-----: | :-------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `hasExtension` / `getExtension` / `setExtension` / `clearExtension` | extension access | typed extension field read/write on an extendee  |
|  [02]   | `hasOption(element, option)` / `getOption(element, option)` | option access | custom-option read on any `Desc*`               |
|  [03]   | `createExtensionContainer(extension, value?)` | extension carrier | `[ReflectMessage, () => value]` for standalone ext |
|  [04]   | `protoInt64`                                  | Int64 support  | `parse`/`enc`/`zero`/`uParse`/`uEnc`; HLC `bigint` half codec |

[ENTRYPOINT_SCOPE]: registry construction
- rail: wire

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY]   | [BOUNDARY_NOTE]                                    |
| :-----: | :-------------------------------------- | :--------------- | :------------------------------------------------ |
|  [01]   | `createRegistry(...input)`              | registry factory | read `Registry` from descriptors/registries        |
|  [02]   | `createMutableRegistry(...input)`       | registry factory | `MutableRegistry` with add/remove                  |
|  [03]   | `createFileRegistry(fileDescriptorSet)` | file registry    | `FileRegistry` from a `FileDescriptorSet`          |
|  [04]   | `createFileRegistry(proto, resolve)`    | file registry    | from a single proto + import resolver              |
|  [05]   | `createFileRegistry(...registries)`     | file registry    | merged `FileRegistry`                              |
|  [06]   | `minimumEdition` / `maximumEdition`     | edition bounds   | `SupportedEdition` floor/ceiling constants         |

[ENTRYPOINT_SCOPE]: descriptor reflection walk (`@bufbuild/protobuf/reflect`)
- rail: wire

The `descriptor.md` evolution gate reads the running `FileDescriptorSet` through these; `registry.files` is the roster `createFileRegistry` yields, `nestedTypes`/`usedTypes` are the two walkers that visit nested and transitively-referenced types without per-level recursion, and every descriptor member is a read-only property of the immutable interfaces.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]    | [BOUNDARY_NOTE]                                                    |
| :-----: | :------------------------------------------- | :---------------- | :---------------------------------------------------------------- |
|  [01]   | `reflect(schema, message?, opts?)`           | message reflect   | `ReflectMessage`; `fields`, `get`/`set`/`clear`/`isSet`           |
|  [02]   | `reflectList(field, ...)` / `reflectMap(field, ...)` | container reflect | dynamic list/map view over a repeated/map field           |
|  [03]   | `nestedTypes(desc)`                          | recursive walker  | `Iterable<DescMessage\|DescEnum\|DescExtension\|DescService>` by `kind` |
|  [04]   | `usedTypes(descMessage)`                     | dependency walker | `Iterable<DescMessage\|DescEnum>` transitively referenced         |
|  [05]   | `parentTypes(desc)`                          | ancestry          | `Parent[]` enclosing-scope chain for a descriptor                 |
|  [06]   | `qualifiedName(desc)`                        | naming            | fully qualified proto name of any `AnyDesc`                       |
|  [07]   | `protoCamelCase` / `protoSnakeCase` / `safeObjectProperty` | name transforms | `name` ↔ `localName` ↔ safe accessor derivation           |
|  [08]   | `buildPath(schema)` / `parsePath(schema, path)` / `pathToString(path)` | typed path | descriptor-typed field path the `FieldMask` lower resolves |
|  [09]   | `scalarEquals` / `scalarZeroValue` / `isScalarZeroValue` | scalar helpers | typed scalar equality and zero-value derivation           |
|  [10]   | `isReflectMessage` / `isReflectList` / `isReflectMap` / `isObject` / `isOneofADT` | guards | narrow dynamic reflection values                       |

[ENTRYPOINT_SCOPE]: low-level wire operations (`@bufbuild/protobuf/wire`)
- rail: wire

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY]   | [BOUNDARY_NOTE]                                                    |
| :-----: | :----------------------------------------------------- | :--------------- | :---------------------------------------------------------------- |
|  [01]   | `BinaryWriter` / `BinaryReader`                        | wire cursor      | tag/varint/LEN framing primitives under `toBinary`/`fromBinary`   |
|  [02]   | `WireType`                                             | numeric enum     | `Varint`/`Bit64`/`LengthDelimited`/`StartGroup`/`EndGroup`/`Bit32` |
|  [03]   | `sizeDelimitedEncode(desc, message, options?)`         | length-prefixed  | one length-delimited frame `Uint8Array`                           |
|  [04]   | `sizeDelimitedDecodeStream(desc, asyncIterable, opts?)` | stream decoder   | `AsyncIterable<MessageShape>`; the segment-stream/proto leg source |
|  [05]   | `sizeDelimitedPeek(data)`                              | frame peek       | `{ eof, size, offset }` union — frame header or eof, without consuming |
|  [06]   | `base64Encode(bytes, encoding?)` / `base64Decode(str)` | base64 codec     | `"std"\|"std_raw"\|"url"`; pairs with connect `encodeBinaryHeader` |
|  [07]   | `parseTextFormatScalarValue` / `parseTextFormatEnumValue` | text-format parse | protobuf text-format scalar/enum ingest for fixtures           |
|  [08]   | `configureTextEncoding(te)` / `getTextEncoding()`      | encoder hook     | inject a `TextEncoder`/`TextDecoder` in non-DOM workers          |
|  [09]   | `FLOAT32_MAX` / `INT32_MAX` / `UINT32_MAX` / `INT32_MIN` | range constants | float32/int32 clamp bounds for scalar validation                |

[ENTRYPOINT_SCOPE]: generated-code boot helpers (`@bufbuild/protobuf/codegenv2`)
- rail: wire

The emitted `<file>_pb.ts` calls these; hand-written interchange source never does — it imports the emitted `<Name>Schema` and passes it to `create`/`fromBinary`.

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [BOUNDARY_NOTE]                                       |
| :-----: | :------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `fileDesc(b64, imports?)`              | file boot      | rehydrate a `DescFile` from the embedded descriptor  |
|  [02]   | `messageDesc(file, path, ...paths)`    | message token  | `GenMessage` at a descriptor path                    |
|  [03]   | `enumDesc(file, path, ...paths)` / `tsEnum(desc)` | enum token | `GenEnum` + the TS enum object                   |
|  [04]   | `serviceDesc(file, path, ...paths)`    | service token  | `GenService` the `createClient` call binds           |
|  [05]   | `extDesc(file, path, ...paths)`        | extension token | `GenExtension` for a generated extension            |

[ENTRYPOINT_SCOPE]: well-known types (`@bufbuild/protobuf/wkt`)
- rail: wire

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [BOUNDARY_NOTE]                                             |
| :-----: | :---------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `FieldMask` + `FieldMaskSchema`                  | WKT + schema   | `paths: string[]`; the C# `FieldMask` partial-update wire  |
|  [02]   | `Timestamp` + `TimestampSchema`                 | WKT + schema   | `seconds: bigint`, `nanos: number`                         |
|  [03]   | `Duration` + `DurationSchema`                   | WKT + schema   | `seconds: bigint`, `nanos: number`                         |
|  [04]   | `Any` + `AnySchema`                             | WKT + schema   | `typeUrl: string`, `value: Uint8Array`                     |
|  [05]   | `Struct`/`Value`/`ListValue`/`NullValue` + schemas | WKT + schema | dynamic JSON-shaped payload container                      |
|  [06]   | `timestampFromDate` / `timestampDate` / `timestampNow` / `timestampFromMs` / `timestampMs` | converters | `Date`/epoch-ms ↔ `Timestamp`               |
|  [07]   | `durationFromMs` / `durationMs`                 | converters     | epoch-ms ↔ `Duration`                                      |
|  [08]   | `anyPack(schema, message[, into])` / `anyUnpack(any, registry\|schema)` / `anyUnpackTo(any, schema, target)` / `anyIs(any, schema\|typeName)` | Any codec | typed ↔ `Any` pack/unpack/probe |
|  [09]   | `isWrapper` / `isWrapperDesc` / `hasCustomJsonRepresentation` | WKT guards | wrapper-type and custom-JSON detection             |

## [04]-[IMPLEMENTATION_LAW]

[DESCRIPTOR_TOPOLOGY] (disk-confirmed against `2.12.1` `.d.ts`):
- `DescField` is a five-arm union `(descFieldScalar | descFieldList | descFieldMessage | descFieldEnum | descFieldMap) & descFieldCommon`, discriminated on `fieldKind: "scalar"|"list"|"message"|"enum"|"map"`; the `list` arm carries `listKind: "scalar"|"enum"|"message"` and the `map` arm `mapKey`/`mapKind`.
- `field.oneof` (`DescOneof | undefined`) rides ONLY the singular `scalar`/`enum`/`message` arms — narrow on `fieldKind` before access; `field.jsonName`/`field.name`/`field.number`/`field.localName` are on the common base; `DescExtension` omits `oneof`.
- `DescMessage` exposes `fields` (declaration-order), `field` (localName→`DescField`), `oneofs`, `members` (ordered field+oneof), `nestedMessages`/`nestedEnums`/`nestedExtensions`, `typeName`, and `proto` (`DescriptorProto`); there is NO top-level `reservedRange` accessor — read `message.proto.reservedRange` (`{ start, end }`, `end` exclusive).
- Packed posture has no direct accessor: read `field.proto.options?.packed` on the raw `FieldDescriptorProto` (a repeated scalar defaults packed unless `options.packed === false`).
- `SupportedEdition` covers `EDITION_PROTO2`/`PROTO3`/`2023`/`2024` bounded by `minimumEdition`/`maximumEdition`; no other edition is accepted at runtime.

[CODEC_LAW]:
- `create(schema, init?)` is the SOLE instantiation entry; the generated class is the `GenMessage` descriptor, never a `new`-able constructor.
- `toBinary`/`fromBinary` are canonical binary; `writeUnknownFields`/`readUnknownFields` default `true` so unknown-field round-trip is preserved.
- `toJson`/`fromJson` require `registry` in options only for `Any` and extension round-trips; omit otherwise. `FileRegistry` (not bare `Registry`) is required when resolving `FileDescriptorProto` imports.

[STACKS_WITH]:
- `effect` (`.api/effect.md`): the single ingress rail is `fromBinary(Schema, bytes)` (or `fromJson` for the JSON leg) → `Schema.decodeUnknown(RowSchema)` → the branded `Codec/codec.md` row, and a thrown protobuf parse or a `ParseError` maps through `Ingress/fault.md` `faultDetailRail.fromConnect` into the `Data.TaggedEnum` fault family, never a raw throw crossing the fold; the proto segment-stream leg wraps `sizeDelimitedDecodeStream(RowSchema, socketAsyncIterable)` in a `Stream.fromAsyncIterable`, mapping each `MessageShape` through the branded decode and lifting a mid-stream parse fault to a `Stream` failure — the native length-delimited owner, never a hand-rolled `sizeDelimitedPeek` loop
- `@connectrpc/connect` (`.api/connectrpc-connect.md`): the `DescService`/`DescMethod` descriptors `createClient` keys on and the `Registry` `findDetails` decodes `Any`-wrapped error details against; `base64Encode(bytes, "std")` mirrors connect's `encodeBinaryHeader` at the `-bin` trailer boundary
- `hash-wasm` (`.api/hash-wasm.md`): `protoInt64.parse`/`enc` own the HLC two-half `bigint` codec the `Codec/parity.md` LE↔BE normalize feeds into `createXXHash128`, and `equals(Schema, a, b)` is the parity oracle over the `ONE_WIRE_FIXTURE_CORPUS` round-trip (`fromBinary`→`toBinary`) — structural, presence-aware equality no `JSON.stringify` compare can match
- `@msgpack/msgpack` (`.api/msgpack-msgpack.md`): the proto `sizeDelimitedDecodeStream` leg is the peer of the msgpack `decodeMultiStream` sync leg — both native length-delimited owners feeding one `Stream.fromAsyncIterable` seam, with `readUnknownFields`/`writeUnknownFields` staying `true` so an additive C# field survives as an `UnknownField` and round-trips out under the `Additive` verdict
- `rfc6902` (`.api/rfc6902.md`): `Codec/patch.md` lowers a `FieldMask` (`FieldMaskSchema`) to `rfc6902` operations by resolving each dotted path through `parsePath(schema, path)` → typed `Path` → `pathToString`, so a recorded intent addresses a descriptor-valid field, not a stringly path; `isFieldSet`/`clearField` apply the `remove`/presence arms and `clone` snapshots the pre-apply message for the error-accumulating fold's rollback
- `descriptor.md` evolution gate: `createFileRegistry(runningSet)` → `registry.files` → per-`DescFile` `nestedTypes` → `DescMessage.fields` (declaration-order `DescField`) + `usedTypes` for the transitive closure, hashing `field.number`/`field.name`/`field.fieldKind` into the declaration-order checksum; the `Identical`/`Additive`/`Breaking` verdict gates dial-time client construction before the first `createClient` call

[RAIL_LAW]:
- Package: `@bufbuild/protobuf`
- Owns: descriptor type system, instance model, binary/JSON/text codec, registry, reflection, well-known types
- Accept: generated `GenMessage`/`GenService` tokens from `protoc-gen-es`; `FileDescriptorSet` from `buf build`
- Reject: hand-rolled binary encoding, `new MessageClass()`, manual field-number handling, custom message base classes, stringly field paths where `parsePath` yields a typed `Path`
