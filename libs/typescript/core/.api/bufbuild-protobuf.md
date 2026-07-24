# [TS_CORE_API_BUFBUILD_PROTOBUF]

`@bufbuild/protobuf` owns the schema-first proto runtime under every C#-minted `*Wire` decode and the descriptor-reflection engine the drift gate walks. A message is plain data branded by `$typeName`; every operation takes the descriptor first, and `create(schema, init?)` is the sole constructor.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@bufbuild/protobuf`
- package: `@bufbuild/protobuf` (Apache-2.0 AND BSD-3-Clause)
- module: `.` codec + descriptors, `./codegenv1`/`./codegenv2` generated-code boot, `./reflect` dynamic accessor, `./wkt` well-known types, `./wire` low-level primitives, `./txtpb` text-format codec; `type: module`, ESM + CJS dual, `sideEffects: false`
- runtime: universal — Node, browser, Bun, worker; no DOM, no peer, zero deps
- rail: proto codec + descriptor reflection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: message model + descriptor family — the schema-first core every op discriminates on
- `Message` is data (`$typeName` brand), `Desc*` the schema; `MessageShape<Desc>`/`MessageInitShape<Desc>` are the derived runtime and init types a `codec/*` page types decoded values by, never a re-declared interface.

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]     | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------ | :---------------- | :---------------------------------------------------- |
|  [01]   | `Message<TypeName>`                                     | message brand     | the `{ readonly $typeName }` brand of a decoded proto |
|  [02]   | `MessageShape<Desc>`                                    | derived shape     | the runtime value of a decoded message                |
|  [03]   | `MessageInitShape<Desc>`                                | derived shape     | the `create` init — partial + oneof-tagged            |
|  [04]   | `MessageJsonType<Desc>`                                 | derived shape     | the JSON projection of the message                    |
|  [05]   | `MessageValidType<Desc>`                                | derived shape     | the validated form                                    |
|  [06]   | `DescMessage` / `DescEnum` / `DescField` / `DescOneof`  | descriptor union  | the reflected schema graph the drift gate walks ([A]) |
|  [07]   | `DescFile` / `DescService` / `DescMethod`               | descriptor union  | file/service/method descriptor nodes                  |
|  [08]   | `DescExtension` / `DescEnumValue` / `DescComments`      | descriptor union  | extension, enum-value, and doc-comment leaves         |
|  [09]   | `ScalarType` (enum)                                     | scalar vocabulary | the scalar leaf type of a `DescField` (values in [B]) |
|  [10]   | `ScalarValue<T, LongAsString>`                          | scalar vocabulary | maps a `ScalarType` to its TS type (INT64→`bigint`)   |
|  [11]   | `UnknownField` (`{ no, wireType, data }`)               | unknown field     | preserved fields under `readUnknownFields`            |
|  [12]   | `DescMethodUnary` / `DescMethodServerStreaming`         | method kinds      | unary + server-streaming method descriptors           |
|  [13]   | `DescMethodClientStreaming` / `DescMethodBiDiStreaming` | method kinds      | client-streaming + bidi; `interchange/invoke` reads   |

- [DESCFIELD]: `DescField` discriminates scalar/list/message/enum/map on `fieldKind` with `number`/`name` coordinates — `scalar: ScalarType`, `message: DescMessage`/`enum: DescEnum` refs, `listKind`/`mapKey`/`mapKind` + leaf arms, and `delimitedEncoding`/`packed`/`longAsString` wire facts. `DescMessage.fields`/`DescService.methods` are the walk edges; `DescMethod.methodKind` closes on `"unary" | "server_streaming" | "client_streaming" | "bidi_streaming"`, `localName` the TS member name.
- [SCALARTYPE]: `ScalarType` values are `DOUBLE`/`FLOAT`/`INT64`/`UINT64`/`INT32`/`FIXED*`/`BOOL`/`STRING`/`BYTES`/`UINT32`/`SFIXED*`/`SINT32`/`SINT64` — a `DescField` scalar leaf mapped to its TS type by `ScalarValue`.

[PUBLIC_TYPE_SCOPE]: generated-symbol family — what `protoc-gen-es` emits and the `codec/*` pages import
- generated `_pb.ts` exports a `GenMessage`/`GenEnum` const per type, each a `Desc*` carrying runtime + JSON type params so `fromBinary(schema, …)` infers `MessageShape` with zero manual typing; a `codec` page imports these consts, never re-declaring the shape.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [CAPABILITY]                                                                      |
| :-----: | :----------------------------- | :--------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `GenFile` (`= DescFile`)       | generated symbol | the generated file-descriptor const                                               |
|  [02]   | `GenMessage<Shape,Opt>`        | generated symbol | the message schema `fromBinary`/`create`/`toBinary` bind                          |
|  [03]   | `GenEnum<Shape,Json>`          | generated symbol | the enum schema carrying runtime + JSON params                                    |
|  [04]   | `GenService<Methods>`          | generated symbol | the service descriptor `createClient` consumes                                    |
|  [05]   | `GenExtension<Extendee,Value>` | generated symbol | the extension schema for custom options                                           |
|  [06]   | `JsonValue` / `JsonObject`     | JSON algebra     | recursive JSON `toJson` returns / `fromJson` accepts; `Struct`↔`JsonValue` target |

[PUBLIC_TYPE_SCOPE]: codec options — the read/write policy knobs
- every codec entry takes a `Partial<…Options>`; the drift-tolerant defaults (`ignoreUnknownFields`, `readUnknownFields`) preserve an unknown field rather than fault, the round-trip-safe posture `interchange/contract` relies on.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                                          |
| :-----: | :--------------------------- | :------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `BinaryReadOptions`          | binary read   | `readUnknownFields:true` keeps forward-compat fields; `recursionLimit` default 100    |
|  [02]   | `BinaryWriteOptions`         | binary write  | `writeUnknownFields` — write back a partial peer's unknown fields                     |
|  [03]   | `JsonReadOptions`            | json read     | `ignoreUnknownFields` (drift-safe default), `registry`, `recursionLimit`              |
|  [04]   | `JsonWriteOptions`           | json write    | `alwaysEmitImplicit`, `enumAsInteger`, `useProtoFieldName`, `registry` — wire dialect |
|  [05]   | `JsonWriteStringOptions`     | json write    | `JsonWriteOptions` + `prettySpaces`                                                   |
|  [06]   | `SizeDelimitedDecodeOptions` | framed read   | `BinaryReadOptions` + `readMaxBytes` — per-message stream cap, default 64 MiB         |
|  [07]   | `TextReadOptions`            | text read     | `registry` (`Any`/extensions), `recursionLimit` default 100                           |
|  [08]   | `TextWriteOptions`           | text write    | `printUnknownFields` (default false, by-number, non-round-trippable), `registry`      |

[PUBLIC_TYPE_SCOPE]: registry + reflect — the reflection surface the drift gate and content-key walk consume

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [CAPABILITY]                                                                        |
| :-----: | :--------------------- | :--------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `Registry`             | type registry    | resolver `get`/`getMessage`/`getEnum`/`getExtension`/`getExtensionFor`/`getService` |
|  [02]   | `MutableRegistry`      | mutable registry | `Registry` + `add`/`remove` for incremental registration                            |
|  [03]   | `FileRegistry`         | file registry    | `Registry` + `files`/`getFile`; the `createFileRegistry` result                     |
|  [04]   | `ReflectMessage`       | dynamic accessor | field-by-field read/write over a `DescMessage`, no generated type                   |
|  [05]   | `ReflectList<V>`       | dynamic accessor | list-field accessor in the drift walk                                               |
|  [06]   | `ReflectMap<K,V>`      | dynamic accessor | map-field accessor in the drift walk                                                |
|  [07]   | `Path` / `PathBuilder` | field path       | typed field-mask address into a `DescMessage`; parity/patch target                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: message lifecycle — construct, copy, compare, all schema-first

| [INDEX] | [SURFACE]                                                                 | [SHAPE] | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------------ | :------ | :------------------------------------------ |
|  [01]   | `create<Desc>(schema, init?: MessageInitShape<Desc>): MessageShape<Desc>` | static  | the only message constructor                |
|  [02]   | `clone<Desc>(schema, message): MessageShape<Desc>`                        | static  | deep structural copy                        |
|  [03]   | `merge<Desc>(schema, target, source): void`                               | static  | proto merge fold ([A])                      |
|  [04]   | `equals<Desc>(schema, a, b, options?: EqualsOptions): boolean`            | static  | deep proto equality ([B])                   |
|  [05]   | `isMessage<Desc>(arg, schema?): arg is MessageShape<Desc>`                | static  | narrows a decode result; checks `$typeName` |
|  [06]   | `protoInt64: Int64Support`                                                | const   | `bigint`↔`{lo,hi}` bridge ([C])             |

- [MERGE]: `merge` folds `source` into `target` — repeated fields concatenate, singular fields overwrite.
- [EQUALS]: `EqualsOptions` = `registry`/`unpackAny`/`extensions`/`unknown`; structural, never reference equality.
- [PROTOINT64]: `parse`/`uParse`/`enc`/`uEnc`/`dec`/`uDec`/`zero`/`supported` — the `bigint`↔`{lo,hi}` bridge for INT64/UINT64; `parse` a string/number, `zero` the identity.

[ENTRYPOINT_SCOPE]: binary codec — the wire ingress/egress every `codec/*` page runs
- import whole-message codecs from `@bufbuild/protobuf`, the `sizeDelimited*` family from `@bufbuild/protobuf/wire`; `codec` pages compose the high-level `fromBinary`/`toBinary`, never `BinaryReader`/`BinaryWriter` unless authoring a custom field.

| [INDEX] | [SURFACE]                                                         | [SHAPE] | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------------- | :------ | :-------------------------------------------- |
|  [01]   | `fromBinary<Desc>(schema, bytes, options?): MessageShape<Desc>`   | static  | decode a C#-minted `*Wire` payload            |
|  [02]   | `mergeFromBinary<Desc>(schema, target, bytes, options?)`          | static  | accumulate a partial into a message           |
|  [03]   | `toBinary<Desc>(schema, message, options?): Uint8Array`           | static  | egress + the canonical content-key bytes      |
|  [04]   | `sizeDelimitedEncode<Desc>(desc, message, options?): Uint8Array`  | static  | length-prefix a frame for egress              |
|  [05]   | `sizeDelimitedDecodeStream<Desc>(desc, iterable, options?)`       | static  | streaming decode; `readMaxBytes` caps a frame |
|  [06]   | `sizeDelimitedPeek(data)`                                         | static  | read a frame header without consuming         |
|  [07]   | `BinaryReader` (class)                                            | ctor    | tag/varint/fixed reader for custom fields     |
|  [08]   | `BinaryWriter` (class)                                            | ctor    | tag/varint/fixed writer for custom fields     |
|  [09]   | `WireType` (enum)                                                 | enum    | the field wire-type discriminant              |
|  [10]   | `base64Encode(bytes, encoding?: "std"\|"std_raw"\|"url"): string` | static  | base64 for the JSON `bytes` dialect           |
|  [11]   | `base64Decode(str): Uint8Array`                                   | static  | base64 decode for the JSON `bytes` dialect    |

- `sizeDelimitedPeek`: returns `{ size, offset, eof:false }` for a complete varint header, `{ size:null, offset:null, eof:true }` when the varint is incomplete.
- `sizeDelimitedDecodeStream`: `readMaxBytes` (default 64 MiB) caps one stream message and raises before the frame buffers — the DoS guard the `frame/geometry` worker decode pins to its payload ceiling.

[ENTRYPOINT_SCOPE]: JSON codec — the debug/text mirror of the binary rail

| [INDEX] | [SURFACE]                                               | [SHAPE] | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------ | :------ | :----------------------------------------------------- |
|  [01]   | `fromJson<Desc>(schema, json: JsonValue, options?)`     | static  | text ingress; `ignoreUnknownFields` drift-safe default |
|  [02]   | `fromJsonString<Desc>(schema, string, options?)`        | static  | text ingress from a JSON string                        |
|  [03]   | `mergeFromJson`                                         | static  | fold JSON into an existing message                     |
|  [04]   | `mergeFromJsonString`                                   | static  | fold a JSON string into a message                      |
|  [05]   | `toJson<Desc>(schema, message, options?): JsonValue`    | static  | diagnostic projection; snapshot fixtures               |
|  [06]   | `toJsonString<Desc>(schema, message, options?): string` | static  | readable dump; `prettySpaces`                          |
|  [07]   | `enumToJson<Desc>(descEnum, value)`                     | static  | enum number→name crossing                              |
|  [08]   | `enumFromJson<Desc>(descEnum, json)`                    | static  | enum name→number crossing                              |
|  [09]   | `isEnumJson<Desc>(descEnum, value)`                     | static  | guard an untrusted enum literal                        |

[ENTRYPOINT_SCOPE]: text-format codec — the `./txtpb` whole-message mirror (txtpbfmt-shaped, BigInt-only)
- import `toText`/`fromText`/`mergeFromText` from `@bufbuild/protobuf/txtpb`; 64-bit fields render as `bigint` with no string fall-back and `toText`/`fromText` throw where `BigInt` is absent. `printUnknownFields` prints by number and is NOT round-trippable — `fromText` rejects number-named fields.

| [INDEX] | [SURFACE]                                                    | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `toText<Desc>(schema, message, options?): string`            | static  | txtpbfmt-formatted diagnostic dump           |
|  [02]   | `fromText<Desc>(schema, text, options?): MessageShape<Desc>` | static  | parse a text-format payload                  |
|  [03]   | `mergeFromText<Desc>(schema, target, text, options?)`        | static  | fold a text payload into an existing message |

[ENTRYPOINT_SCOPE]: registry + reflection — the descriptor-driven path with no generated code
- `createFileRegistry(FileDescriptorSet)` is the hinge: it turns a compiled descriptor set into runtime descriptors, so `interchange/contract` diffs schema versions and `reflect` reads fields by descriptor alone.

| [INDEX] | [SURFACE]                                                | [SHAPE] | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------- | :------ | :------------------------------------------ |
|  [01]   | `createFileRegistry(fileDescriptorSet)`                  | static  | decode the C#-minted set, then walk `files` |
|  [02]   | `createFileRegistry(proto, resolve)`                     | static  | build from a proto with a resolver          |
|  [03]   | `createFileRegistry(...registries)`                      | static  | merge existing registries                   |
|  [04]   | `createRegistry(...input)`                               | static  | assemble the `Any`/extension resolver       |
|  [05]   | `createMutableRegistry(...input)`                        | static  | the incremental-registration form           |
|  [06]   | `reflect<Desc>(desc, message?, …): ReflectMessage`       | static  | field-by-field read/write over a descriptor |
|  [07]   | `reflectList<V>(field,…)`                                | static  | list-field reflect accessor                 |
|  [08]   | `reflectMap<K,V>(field,…)`                               | static  | map-field reflect accessor                  |
|  [09]   | `buildPath(schema): PathBuilder`                         | static  | build a typed field-mask address            |
|  [10]   | `parsePath(schema, path, options?): Path`                | static  | parse a field-mask string to a `Path`       |
|  [11]   | `pathToString(path)`                                     | static  | render a `Path` to string                   |
|  [12]   | `InvalidPathError`                                       | ctor    | the field-mask parse error                  |
|  [13]   | `qualifiedName(desc)`                                    | static  | descriptor qualified-name                   |
|  [14]   | `protoCamelCase(s)` / `protoSnakeCase(s)`                | static  | field-name case canonicalization            |
|  [15]   | `safeObjectProperty(s)`                                  | static  | safe property-name projection               |
|  [16]   | `scalarEquals` / `scalarZeroValue` / `isScalarZeroValue` | static  | scalar default + equality in the drift walk |

[ENTRYPOINT_SCOPE]: extensions + well-known types — `Any` packing, time bridges, the `Struct` codec
- `./wkt` schema consts (`FileDescriptorSetSchema`/`StructSchema`/`ValueSchema`/`ListValueSchema`/`AnySchema`/`TimestampSchema`/`DurationSchema` …) decode the descriptor set and bridge a `JsonValue` through `Struct`/`Value`; custom options carry the `FaultDetail` and SI-scalar `QuantityFamily` annotation hooks `interchange/format` reads.

| [INDEX] | [SURFACE]                                          | [SHAPE] | [CAPABILITY]                            |
| :-----: | :------------------------------------------------- | :------ | :-------------------------------------- |
|  [01]   | `getExtension`                                     | static  | read a custom option off a `Desc*`      |
|  [02]   | `setExtension`                                     | static  | set a custom option on a `Desc*`        |
|  [03]   | `clearExtension`                                   | static  | clear a custom option                   |
|  [04]   | `hasExtension`                                     | static  | test a custom option's presence         |
|  [05]   | `getOption`                                        | static  | read a descriptor option                |
|  [06]   | `hasOption`                                        | static  | test a descriptor option                |
|  [07]   | `createExtensionContainer`                         | static  | mint an extension container             |
|  [08]   | `anyPack<Desc>(schema, message[, into])`           | static  | box a message into an `Any`             |
|  [09]   | `anyUnpack(any, registry)`                         | static  | unbox via a `Registry` by type URL      |
|  [10]   | `anyUnpack<Desc>(any, schema)`                     | static  | unbox against a known schema            |
|  [11]   | `anyUnpackTo`                                      | static  | unbox into an existing target           |
|  [12]   | `anyIs(any, schema\|typeName)`                     | static  | test a boxed type                       |
|  [13]   | `timestampFromDate(date)` / `timestampDate(ts)`    | static  | `Timestamp`↔JS `Date`                   |
|  [14]   | `timestampFromMs` / `timestampMs` / `timestampNow` | static  | `Timestamp`↔ms, and now                 |
|  [15]   | `durationFromMs` / `durationMs`                    | static  | `Duration`↔ms                           |
|  [16]   | `isWrapper(msg)` / `isWrapperDesc(desc)`           | static  | wrapper-type detection during JSON      |
|  [17]   | `hasCustomJsonRepresentation(desc)`                | static  | custom-JSON representation detection    |
|  [18]   | `configureTextEncoding`                            | static  | pluggable `TextEncoder` for non-browser |
|  [19]   | `parseTextFormatScalarValue`                       | static  | parse a text-format scalar value        |
|  [20]   | `parseTextFormatEnumValue`                         | static  | parse a text-format enum value          |

[ENTRYPOINT_SCOPE]: codegen boot — the generated-code side (`./codegenv2`, authored by `protoc-gen-es`, not hand-called)
- a `_pb.ts` file calls these to reconstitute descriptors from an embedded base64 `FileDescriptorProto`; a `codec` page imports the resulting `GenMessage` const, never these functions.

| [INDEX] | [SURFACE]                           | [SHAPE] | [CAPABILITY]                                            |
| :-----: | :---------------------------------- | :------ | :------------------------------------------------------ |
|  [01]   | `fileDesc(b64, imports?): DescFile` | static  | reconstruct the `DescFile` from the embedded descriptor |
|  [02]   | `boot`                              | static  | boot a file descriptor                                  |
|  [03]   | `bootFileDescriptorProto`           | static  | boot from a `FileDescriptorProto`                       |
|  [04]   | `messageDesc(file, path, …)`        | static  | index a `GenMessage` out of the file                    |
|  [05]   | `enumDesc(file, path, …)`           | static  | index a `GenEnum` out of the file                       |
|  [06]   | `serviceDesc(file, path, …)`        | static  | index a `GenService` out of the file                    |
|  [07]   | `extDesc(file, path, …)`            | static  | index a `GenExtension` out of the file                  |
|  [08]   | `tsEnum(desc)`                      | static  | materialize a TS `enum` object                          |
|  [09]   | `objEnum(desc)`                     | static  | materialize an erasable `as const` enum (no TS `enum`)  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every operation reads `(schema, value, options?)` — descriptor first, the message plain data carrying only `$typeName`, and `create(schema, init?)` the sole constructor. A `codec/*` page imports the `GenMessage` from generated `_pb.ts`, `fromBinary(Schema, bytes)` infers `MessageShape<Schema>` with no annotation, and a decoded value discriminates by `$typeName` or `isMessage(v, Schema)`, never `instanceof`.
- INT64/UINT64 fields are `bigint` (or `string` under long-as-string codegen), bridged by `protoInt64` — `.parse(s)` from a string, `.zero` the identity; `Number`-coercing a 64-bit field loses precision past 2^53.
- `toBinary(schema, msg)` is deterministic per field order — the canonical byte source the `value/identity` `XxHash128` seed-zero mint hashes for a content key, the same mint `frame/*` reassembly verifies against.
- `interchange/contract`'s drift gate is pure reflection: `fromBinary(FileDescriptorSetSchema, bytes)` decodes the C#-minted set, `createFileRegistry(set)` builds it, and the walk `registry.files → DescMessage → DescField` diffs the prior generation into a typed `ContractDrift`; `readUnknownFields`/`ignoreUnknownFields` preserve an unknown field rather than throw.
- `reflect(desc, message)` yields a `ReflectMessage` reading/writing fields by `DescField` with no generated type — the `interchange/codec` content-key projection walks wire fields this way to compute parity, and `buildPath(schema)`/`parsePath(schema, path)` address a field-mask target; `ScalarType` + `scalarZeroValue` classify a leaf, `qualifiedName`/`protoCamelCase` canonicalize across the C#↔TS casing boundary.

[STACKING]:
- `@connectrpc/connect`(`.api/connectrpc-connect.md`), `@connectrpc/connect-web`(`.api/connectrpc-connect-web.md`): this runtime IS the Connect message layer — a `DescMethod` carries `GenMessage` input/output schemas and the transport calls `toBinary`/`fromBinary` internally, while `interchange/invoke` picks the protocol axis (`connect` | `grpc-web`) and never re-implements the runtime.
- `effect`(`libs/typescript/.api/effect.md`): `fromBinary` yields the WIRE shape and `Schema.decode(KernelSchema)(wire)` parses it into branded `kernel` vocabulary — proto is transport, `Schema` is domain, a proto message never a domain model; a synchronous codec call that throws on malformed bytes wraps in `Effect.try`, and `sizeDelimitedDecodeStream(desc, asyncIterable)` feeds `Stream.fromAsyncIterable` for backpressured framed decode.
- `cbor-x`/`@msgpack/msgpack`/`rfc6902`(`.api/`): the interchange plane is multi-codec — each sibling owns its own C#-mint format and a `codec` page composes exactly one.
- `@bufbuild/protoc-gen-es`(same `catalog`): emits the `_pb.ts` `GenMessage` consts from the C#-shared `.proto`; the generated file is build output the `codec/*` pages import.
- `value/identity` (within-lib edge): `toBinary` canonical bytes are the `XxHash128` content-key input the `frame/*` mint verifies — one seed, one mint site, proto bytes in.

[LOCAL_ADMISSION]:
- Import the generated `GenMessage` schema and call the schema-first codec (`fromBinary`/`toBinary`/`create`); cross a decoded proto into `kernel` vocabulary through `Schema.decode` at the page boundary, never hand-authoring a shape or reusing a decoded proto as a domain model.
- Reflection (`createFileRegistry` + `reflect` + `buildPath`) serves `interchange/contract` only; the generated schema rules everywhere else.
- 64-bit fields cross through `protoInt64`, `Timestamp`/`Duration` through the `timestamp*`/`duration*` bridges, `Any` unpacks only with a `Registry`, and `readUnknownFields`/`ignoreUnknownFields` stay on for forward-compat.

[RAIL_LAW]:
- Package: `@bufbuild/protobuf`
- Owns: the schema-first proto runtime (`create`/`clone`/`merge`/`equals`/`isMessage`), the binary + JSON + text codec including size-delimited streaming under `readMaxBytes`, the descriptor-reflection engine (`createFileRegistry`/`reflect`/`buildPath`), extensions and options, the well-known types (`Any`/`Timestamp`/`Duration`/`Struct`), and the low-level `./wire` primitives (`BinaryReader`/`BinaryWriter`/`WireType`/base64)
- Accept: generated `GenMessage` schemas from `@bufbuild/protoc-gen-es`, a C#-minted `FileDescriptorSet` decoded through `FileDescriptorSetSchema`, `protoInt64` for 64-bit fields, a `Registry` for `Any`/extension resolution, `Effect.try`/`Stream` for the error and streaming rails, `Schema.decode` as the domain boundary above the wire shape
- Reject: `new`-ing a message or calling a method on it, a hand-authored proto shape, a decoded proto reused as a domain model, `Number`-coercing a 64-bit field, a second content-hash over anything but `toBinary` canonical bytes, reflection where a generated schema exists, and reaching for a sibling codec on a proto family
