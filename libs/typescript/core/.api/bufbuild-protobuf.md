# [TS_CORE_API_BUFBUILD_PROTOBUF]

`@bufbuild/protobuf` is protobuf-ES catalog-bound: the runtime every C#-minted proto `*Wire` shape decodes through, and the reflection engine the drift gate walks. It is schema-first, not message-methods — a message is plain data branded by `$typeName`, never a class instance, and every operation takes the descriptor as its first argument: `create(schema, init?)`, `fromBinary(schema, bytes)`, `toBinary(schema, msg)`, `fromJson(schema, json)`, `toJson(schema, msg)`. Two decode paths share one runtime: the GENERATED path binds `GenMessage` schemas emitted by `@bufbuild/protoc-gen-es` (the `interchange/format`, `interchange/codec`, `interchange/codec`, `frame/geometry` pages import a `_pb.ts` schema and call `fromBinary`), and the REFLECTION path binds descriptors with no generated code — `createFileRegistry(fromBinary(FileDescriptorSetSchema, bytes))` turns a C#-minted `FileDescriptorSet` into runtime `DescMessage`/`DescField` descriptors that `interchange/contract` diffs into a typed `ContractDrift` verdict and that the `reflect(desc, msg)` + `buildPath(schema)` surface walks for content-key field parity. It carries the `./reflect` dynamic accessor, the `./wire` low-level codec primitives (`BinaryReader`/`BinaryWriter`/`WireType`, base6 catalog, size-delimited streaming, text-format), and the `./wkt` well-known types (`Any` packing under a registry, `Timestamp`/`Duration` ↔ JS `Date`/ms, the `Struct`↔`JsonValue` bridge). It is the runtime under `@connectrpc/connect` (`interchange/invoke`); the codec siblings `cbor-x` (`interchange/codec`), `@msgpack/msgpack` (`interchange/codec`), and `rfc690 catalog` (`interchange/format`) own the non-proto wire families this one never touches. Effect `Schema` is the seam above it: proto is the wire shape, `Schema.decode` parses decoded proto into branded `kernel` vocabulary — never a hand-rolled field validator, never a proto message reused as a domain model.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@bufbuild/protobuf`
- package: `@bufbuild/protobuf`
- license: `Apache-2.0 AND BSD-3-Clause` (BSD covers the varint/base64 code adapted from `google-protobuf`)
- deps: none — zero transitive footprint, the reason it is the codec substrate
- peer: none; `type: module`, ESM + CJS dual (`dist/esm`, `dist/commonjs`), `sideEffects: false` (fully tree-shakeable — an unused subpath costs zero bytes)
- exports: `.` (codec + descriptors), `./codegenv1` + `./codegenv2` (the generated-code boot surface), `./reflect` (dynamic accessor), `./wkt` (well-known types), `./wire` (low-level primitives); `./package.json`
- runtime target: universal — Node, browser, Bun, worker; no DOM, no platform binding. The `codec/*` pages run it in the app thread; `frame/geometry` streaming decode runs it in a worker
- rail: proto codec + descriptor reflection (the runtime under every `codec/*` proto page, `interchange/contract` drift, `interchange/codec`, `frame/geometry`, and `interchange/invoke` transport)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: message model + descriptor family — the schema-first core every op discriminates on
- rail: proto codec
- a `Message` is data (`$typeName` brand), a `Desc*` is the schema. `MessageShape<Desc>`/`MessageInitShape<Desc>` are the derived runtime and constructor types the generated schema resolves; `codec/*` pages type decoded values by these, never by a re-declared interface.

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                                   |
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

- [A]-[DESCFIELD]: `DescField` is a scalar/list/message/enum/map discriminated union on `fieldKind`, with `number`/`name` coordinates: `scalar: ScalarType` on the scalar arm, `message: DescMessage`/`enum: DescEnum` refs, `listKind` + leaf on the list arm, `mapKey: ScalarType` + `mapKind` + leaf on the map arm, and `delimitedEncoding`/`packed`/`longAsString` wire facts. `DescMessage.fields: DescField[]` and `DescService.methods: DescMethod[]` are the walk edges; `DescMethod.methodKind` is the closed `"unary" | "server_streaming" | "client_streaming" | "bidi_streaming"` axis and `localName` the TS member name on every descriptor.
- [B]-[SCALARTYPE]: `ScalarType` enum values are `DOUBLE`/`FLOAT`/`INT64`/`UINT64`/`INT32`/`FIXED*`/`BOOL`/`STRING`/`BYTES`/`UINT32`/`SFIXED*`/`SINT32`/`SINT64`; the leaf of a `DescField` scalar arm, mapped to its TS type by `ScalarValue`.

[PUBLIC_TYPE_SCOPE]: generated-symbol family — what `protoc-gen-es` emits and the `codec/*` pages import
- rail: proto codec
- the generated `_pb.ts` exports a `GenMessage`/`GenEnum` const per type; it is a `Desc*` carrying the runtime + JSON type parameters so `fromBinary(schema, …)` infers `MessageShape` with zero manual typing. A `codec` page imports these consts and never re-declares the shape.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                                                               |
| :-----: | :----------------------------- | :--------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `GenFile` (`= DescFile`)       | generated symbol | the generated file-descriptor const                                               |
|  [02]   | `GenMessage<Shape,Opt>`        | generated symbol | the message schema `fromBinary`/`create`/`toBinary` bind                          |
|  [03]   | `GenEnum<Shape,Json>`          | generated symbol | the enum schema carrying runtime + JSON params                                    |
|  [04]   | `GenService<Methods>`          | generated symbol | the service descriptor `createClient` consumes                                    |
|  [05]   | `GenExtension<Extendee,Value>` | generated symbol | the extension schema for custom options                                           |
|  [06]   | `JsonValue` / `JsonObject`     | JSON algebra     | recursive JSON `toJson` returns / `fromJson` accepts; `Struct`↔`JsonValue` target |

[PUBLIC_TYPE_SCOPE]: codec options — the read/write policy knobs
- rail: proto codec
- every codec entry takes a `Partial<…Options>`; the drift-tolerant defaults (`ignoreUnknownFields`, `readUnknownFields`) are the round-trip-safe posture `interchange/contract` relies on so an unknown field is preserved, never a decode fault.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CONSUMER_BOUNDARY]                                                                   |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `BinaryReadOptions`      | binary read   | `readUnknownFields:true` keeps forward-compat fields; `recursionLimit` default 100    |
|  [02]   | `BinaryWriteOptions`     | binary write  | `writeUnknownFields` — write back a partial peer's unknown fields                     |
|  [03]   | `JsonReadOptions`        | json read     | `ignoreUnknownFields` (drift-safe default), `registry`, `recursionLimit`              |
|  [04]   | `JsonWriteOptions`       | json write    | `alwaysEmitImplicit`, `enumAsInteger`, `useProtoFieldName`, `registry` — wire dialect |
|  [05]   | `JsonWriteStringOptions` | json write    | `JsonWriteOptions` + `prettySpaces`                                                   |

[PUBLIC_TYPE_SCOPE]: registry + reflect — the reflection surface the drift gate and content-key walk consume
- rail: descriptor reflection
- the `interchange/contract` drift walk drives the reflect accessors; `Path`/`PathBuilder` name the field-mask target `interchange/codec` and `interchange/format` also consume.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                                                                 |
| :-----: | :--------------------- | :--------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `Registry`             | type registry    | resolver `get`/`getMessage`/`getEnum`/`getExtension`/`getExtensionFor`/`getService` |
|  [02]   | `MutableRegistry`      | mutable registry | `Registry` + `add`/`remove` for incremental registration                            |
|  [03]   | `FileRegistry`         | file registry    | `Registry` + `files`/`getFile`; the `createFileRegistry` result                     |
|  [04]   | `ReflectMessage`       | dynamic accessor | field-by-field read/write over a `DescMessage`, no generated type                   |
|  [05]   | `ReflectList<V>`       | dynamic accessor | list-field accessor in the drift walk                                               |
|  [06]   | `ReflectMap<K,V>`      | dynamic accessor | map-field accessor in the drift walk                                                |
|  [07]   | `Path` / `PathBuilder` | field path       | typed field-mask address into a `DescMessage`; parity/patch target                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: message lifecycle — construct, copy, compare (all schema-first)
- rail: proto codec

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                         |
| :-----: | :------------------------------------------------------------------------ | :------------- | :------------------------------------------ |
|  [01]   | `create<Desc>(schema, init?: MessageInitShape<Desc>): MessageShape<Desc>` | constructor    | the only message constructor                |
|  [02]   | `clone<Desc>(schema, message): MessageShape<Desc>`                        | copy           | deep structural copy                        |
|  [03]   | `merge<Desc>(schema, target, source): void`                               | merge          | proto merge fold ([A])                      |
|  [04]   | `equals<Desc>(schema, a, b, options?: EqualsOptions): boolean`            | structural eq  | deep proto equality ([B])                   |
|  [05]   | `isMessage<Desc>(arg, schema?): arg is MessageShape<Desc>`                | type guard     | narrows a decode result; checks `$typeName` |
|  [06]   | `protoInt64: Int64Support`                                                | 64-bit bridge  | `bigint`↔`{lo,hi}` bridge ([C])             |

- [A]-[MERGE]: `merge` folds `source` into `target` — repeated fields concatenate, singular fields overwrite.
- [B]-[EQUALS]: `EqualsOptions` = `registry`/`unpackAny`/`extensions`/`unknown`; structural, never reference equality.
- [C]-[PROTOINT64]: `parse`/`uParse`/`enc`/`uEnc`/`dec`/`uDec`/`zero`/`supported` — the `bigint`↔`{lo,hi}` bridge for INT64/UINT64; `parse` a string/number, `zero` the identity.

[ENTRYPOINT_SCOPE]: binary codec — the wire ingress/egress every `codec/*` page runs
- rail: proto codec
- the canonical entries are `fromBinary`/`toBinary` (whole message) and the `sizeDelimited*` family (length-prefixed frames). `BinaryReader`/`BinaryWriter` are the sub-message primitives; `codec` pages compose the high-level entries, never the low-level reader unless authoring a custom field.
- the `sizeDelimited*` family walks an `AsyncIterable<Uint8Array>`; `sizeDelimitedPeek` returns `{ size, offset, eof:false }` for a complete varint header or `{ size:null, offset:null, eof:true }` when the varint is incomplete.

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                           |
| :-----: | :---------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `fromBinary<Desc>(schema, bytes, options?): MessageShape<Desc>`   | binary read    | decode a C#-minted `*Wire` payload            |
|  [02]   | `mergeFromBinary<Desc>(schema, target, bytes, options?)`          | binary read    | accumulate a partial into a message           |
|  [03]   | `toBinary<Desc>(schema, message, options?): Uint8Array`           | binary write   | egress + the canonical content-key bytes      |
|  [04]   | `sizeDelimitedEncode<Desc>(desc, message, options?): Uint8Array`  | framed stream  | length-prefix a frame for egress              |
|  [05]   | `sizeDelimitedDecodeStream<Desc>(desc, iterable, options?)`       | framed stream  | streaming decode over the async byte iterable |
|  [06]   | `sizeDelimitedPeek(data)`                                         | framed stream  | read a frame header without consuming         |
|  [07]   | `BinaryReader` (class)                                            | low-level      | tag/varint/fixed reader for custom fields     |
|  [08]   | `BinaryWriter` (class)                                            | low-level      | tag/varint/fixed writer for custom fields     |
|  [09]   | `WireType` (enum)                                                 | low-level      | the field wire-type discriminant              |
|  [10]   | `base64Encode(bytes, encoding?: "std"\|"std_raw"\|"url"): string` | low-level      | base64 for the JSON `bytes` dialect           |
|  [11]   | `base64Decode(str): Uint8Array`                                   | low-level      | base64 decode for the JSON `bytes` dialect    |

[ENTRYPOINT_SCOPE]: JSON codec — the debug/text mirror of the binary rail
- rail: proto codec

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                    |
| :-----: | :------------------------------------------------------ | :------------- | :----------------------------------------------------- |
|  [01]   | `fromJson<Desc>(schema, json: JsonValue, options?)`     | json read      | text ingress; `ignoreUnknownFields` drift-safe default |
|  [02]   | `fromJsonString<Desc>(schema, string, options?)`        | json read      | text ingress from a JSON string                        |
|  [03]   | `mergeFromJson`                                         | json read      | fold JSON into an existing message                     |
|  [04]   | `mergeFromJsonString`                                   | json read      | fold a JSON string into a message                      |
|  [05]   | `toJson<Desc>(schema, message, options?): JsonValue`    | json write     | diagnostic projection; snapshot fixtures               |
|  [06]   | `toJsonString<Desc>(schema, message, options?): string` | json write     | readable dump; `prettySpaces`                          |
|  [07]   | `enumToJson<Desc>(descEnum, value)`                     | enum json      | enum number→name crossing                              |
|  [08]   | `enumFromJson<Desc>(descEnum, json)`                    | enum json      | enum name→number crossing                              |
|  [09]   | `isEnumJson<Desc>(descEnum, value)`                     | enum json      | guard an untrusted enum literal                        |

[ENTRYPOINT_SCOPE]: registry + reflection — the descriptor-driven path with no generated code
- rail: descriptor reflection
- `createFileRegistry(FileDescriptorSet)` is the hinge: it turns a compiled descriptor set (`buf build -o descriptor.binpb`, or the C#-minted set the seam ships) into runtime descriptors, so `interchange/contract` diffs schema versions and `reflect` reads fields by descriptor alone.

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY]  | [CONSUMER_BOUNDARY]                         |
| :-----: | :------------------------------------------------------- | :-------------- | :------------------------------------------ |
|  [01]   | `createFileRegistry(fileDescriptorSet)`                  | file registry   | decode the C#-minted set, then walk `files` |
|  [02]   | `createFileRegistry(proto, resolve)`                     | file registry   | build from a proto with a resolver          |
|  [03]   | `createFileRegistry(...registries)`                      | file registry   | merge existing registries                   |
|  [04]   | `createRegistry(...input)`                               | type registry   | assemble the `Any`/extension resolver       |
|  [05]   | `createMutableRegistry(...input)`                        | type registry   | the incremental-registration form           |
|  [06]   | `reflect<Desc>(desc, message?, …): ReflectMessage`       | dynamic reflect | field-by-field read/write over a descriptor |
|  [07]   | `reflectList<V>(field,…)`                                | dynamic reflect | list-field reflect accessor                 |
|  [08]   | `reflectMap<K,V>(field,…)`                               | dynamic reflect | map-field reflect accessor                  |
|  [09]   | `buildPath(schema): PathBuilder`                         | field path      | build a typed field-mask address            |
|  [10]   | `parsePath(schema, path, options?): Path`                | field path      | parse a field-mask string to a `Path`       |
|  [11]   | `pathToString(path)`                                     | field path      | render a `Path` to string                   |
|  [12]   | `InvalidPathError`                                       | field path      | the field-mask parse error                  |
|  [13]   | `qualifiedName(desc)`                                    | reflect helper  | descriptor qualified-name                   |
|  [14]   | `protoCamelCase(s)` / `protoSnakeCase(s)`                | reflect helper  | field-name case canonicalization            |
|  [15]   | `safeObjectProperty(s)`                                  | reflect helper  | safe property-name projection               |
|  [16]   | `scalarEquals` / `scalarZeroValue` / `isScalarZeroValue` | reflect helper  | scalar default + equality in the drift walk |

[ENTRYPOINT_SCOPE]: extensions + well-known types — `Any` packing, time bridges, the `Struct` codec
- rail: proto codec
- custom options carry the `FaultDetail` vocabulary hook and the SI-scalar `QuantityFamily` annotations `interchange/format` reads.
- the `./wkt` schema consts — `FileDescriptorSetSchema`/`FileDescriptorProtoSchema`/`StructSchema`/`ValueSchema`/`ListValueSchema`/`AnySchema`/`TimestampSchema`/`DurationSchema` — decode the descriptor set (`interchange/contract`) and bridge a `JsonValue` through `Struct`/`Value`.

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                     |
| :-----: | :------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `getExtension`                                     | extensions     | read a custom option off a `Desc*`      |
|  [02]   | `setExtension`                                     | extensions     | set a custom option on a `Desc*`        |
|  [03]   | `clearExtension`                                   | extensions     | clear a custom option                   |
|  [04]   | `hasExtension`                                     | extensions     | test a custom option's presence         |
|  [05]   | `getOption`                                        | extensions     | read a descriptor option                |
|  [06]   | `hasOption`                                        | extensions     | test a descriptor option                |
|  [07]   | `createExtensionContainer`                         | extensions     | mint an extension container             |
|  [08]   | `anyPack<Desc>(schema, message[, into])`           | Any codec      | box a message into an `Any`             |
|  [09]   | `anyUnpack(any, registry)`                         | Any codec      | unbox via a `Registry` by type URL      |
|  [10]   | `anyUnpack<Desc>(any, schema)`                     | Any codec      | unbox against a known schema            |
|  [11]   | `anyUnpackTo`                                      | Any codec      | unbox into an existing target           |
|  [12]   | `anyIs(any, schema\|typeName)`                     | Any codec      | test a boxed type                       |
|  [13]   | `timestampFromDate(date)` / `timestampDate(ts)`    | time bridge    | `Timestamp`↔JS `Date`                   |
|  [14]   | `timestampFromMs` / `timestampMs` / `timestampNow` | time bridge    | `Timestamp`↔ms, and now                 |
|  [15]   | `durationFromMs` / `durationMs`                    | time bridge    | `Duration`↔ms                           |
|  [16]   | `isWrapper(msg)` / `isWrapperDesc(desc)`           | wkt guards     | wrapper-type detection during JSON      |
|  [17]   | `hasCustomJsonRepresentation(desc)`                | wkt guards     | custom-JSON representation detection    |
|  [18]   | `configureTextEncoding`                            | text           | pluggable `TextEncoder` for non-browser |
|  [19]   | `parseTextFormatScalarValue`                       | text           | parse a text-format scalar value        |
|  [20]   | `parseTextFormatEnumValue`                         | text           | parse a text-format enum value          |

[ENTRYPOINT_SCOPE]: codegen boot — the generated-code side (`./codegenv2`, authored by `protoc-gen-es`, not hand-called)
- rail: proto codec
- these are what a `_pb.ts` file calls to reconstitute descriptors from an embedded base64 `FileDescriptorProto`; a `codec` page imports the resulting `GenMessage` const, never these functions directly.

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                     |
| :-----: | :---------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `fileDesc(b64, imports?): DescFile` | file boot      | reconstruct the `DescFile` from the embedded descriptor |
|  [02]   | `boot`                              | file boot      | boot a file descriptor                                  |
|  [03]   | `bootFileDescriptorProto`           | file boot      | boot from a `FileDescriptorProto`                       |
|  [04]   | `messageDesc(file, path, …)`        | symbol boot    | index a `GenMessage` out of the file                    |
|  [05]   | `enumDesc(file, path, …)`           | symbol boot    | index a `GenEnum` out of the file                       |
|  [06]   | `serviceDesc(file, path, …)`        | symbol boot    | index a `GenService` out of the file                    |
|  [07]   | `extDesc(file, path, …)`            | symbol boot    | index a `GenExtension` out of the file                  |
|  [08]   | `tsEnum(desc)`                      | symbol boot    | materialize a TS enum object                            |

## [04]-[IMPLEMENTATION_LAW]

[SCHEMA_FIRST_TOPOLOGY]:
- The schema is always the first argument and messages are plain data. `create(schema, init?)` is the only constructor; there is no `new SomeMessage()`, no `msg.toBinary()`, no `msg.clone()` — the value carries `$typeName` and nothing else, and every operation `(schema, value, options?)` reads the descriptor to drive the fold. A `codec/*` page imports the `GenMessage` from the generated `_pb.ts`, calls `fromBinary(Schema, bytes)`, and the result type is `MessageShape<Schema>` with no manual annotation. Discriminate a decoded value by `$typeName` or `isMessage(v, Schema)`, never `instanceof`.
- INT64/UINT64 fields are `bigint` (or `string` under the long-as-string codegen), bridged by `protoInt64` — `protoInt64.parse(s)` from a string, `.zero` for the identity. Never coerce a 64-bit field through `Number`; the kernel `Hlc` 64-bit cell and any id past 2^53 loses precision otherwise.
- The binary codec is deterministic per field order, so `toBinary(schema, msg)` is the canonical byte source the `value/identity` `XxHash128` seed-zero mint hashes for a content key — the SAME mint the `frame/*` reassembly verifies against, never a second hash function.

[REFLECTION_PATH]:
- The `interchange/contract` drift gate is pure reflection: decode the C#-minted set with `fromBinary(FileDescriptorSetSchema, descriptorBytes)`, build `createFileRegistry(set)`, then walk `registry.files → DescMessage → DescField` and compare against the prior generation's descriptors. Field adds/removes/type-changes surface as a typed `ContractDrift` verdict value — schema drift is a value, never a runtime decode failure, because `readUnknownFields`/`ignoreUnknownFields` keep an unknown field preserved rather than thrown.
- `reflect(desc, message)` yields a `ReflectMessage` that reads/writes fields by `DescField` with no generated type — the `interchange/codec` content-key projection walks `ElementGraphWire`/`NodeWire`/`RelationshipWire` fields this way to compute parity, and `buildPath(schema)`/`parsePath(schema, "node.relations")` addresses a field-mask target the drift verdict and `interchange/format` both name. `ScalarType` + `scalarZeroValue` classify a leaf; `qualifiedName`/`protoCamelCase` canonicalize names across the C#↔TS casing boundary.

[STACKS_WITH]:
- `@connectrpc/connect` + `@connectrpc/connect-web` (`interchange/invoke`, `libs/typescript/core/.api/connectrpc-connect.md` / `connectrpc-connect-web.md`): this package is the RUNTIME under Connect — a Connect `DescMethod` carries `GenMessage` input/output schemas, and the transport calls `toBinary`/`fromBinary` internally. `interchange/invoke` picks the protocol axis (`connect` | `grpc-web`) and retryable-wire schedules; the message runtime is this one, never re-implemented.
- codec siblings `cbor-x` / `@msgpack/msgpack` / `rfc6902` (`interchange/codec` / `interchange/codec` / `interchange/format`): the interchange plane is multi-codec — proto owns the descriptor-typed families (`ElementGraph`, `FaultDetail`, `GeometryPayload`, the RPC suite), CBOR owns the canonical snapshot header, MessagePack owns the CRDT op union, RFC 6902 owns the JSON-Patch egress. A `codec` page picks ONE codec by the C# mint format; a proto page never reaches for CBOR and vice versa.
- Effect `Schema` (universal tier, `libs/typescript/.api/effect.md`): the parse-not-validate boundary above proto. `fromBinary` yields the WIRE shape; `Schema.decode(KernelSchema)(wire)` parses it into the branded `kernel` vocabulary (a `Quantity`, `TenantContext`, `Hlc`) — proto is transport, Schema is domain. A proto message is NEVER reused as a domain model, and field validation is a `Schema` refinement, never a hand-rolled check on the decoded object.
- Effect `Effect`/`Stream` (universal tier): codec calls are synchronous and can throw on malformed bytes, so `codec/*` wraps `fromBinary` in `Effect.try` folding the parse error into the folder's `Data.TaggedError` rail; `sizeDelimitedDecodeStream(desc, asyncIterable)` feeds `Stream.fromAsyncIterable` so `interchange/codec` and `frame/geometry` decode a framed byte stream as an Effect `Stream` under backpressure.
- `value/identity` (permitted edge): `toBinary` canonical bytes are the `XxHash128` content-key input the `frame/*` mint verifies — one hash seed, one mint site, proto bytes in.
- `@bufbuild/protoc-gen-es` (tooling catalog, same `catalog`): the codegen that emits the `_pb.ts` `GenMessage` consts from the C#-shared `.proto`; the generated file is build output the `codec/*` pages import, and its version tracks this runtime exactly.

[LOCAL_ADMISSION]:
- Import the generated `GenMessage` schema and call the schema-first codec (`fromBinary`/`toBinary`/`create`); never author a proto message shape by hand, never treat a decoded proto as a domain model — cross into `kernel` vocabulary through `Schema.decode` at the page boundary.
- The reflection path (`createFileRegistry` + `reflect` + `buildPath`) is `interchange/contract` only — everywhere else the generated schema is present, so use it; reflection is for the drift gate and content-key walk where the schema is data, not a type.
- 64-bit fields are `bigint` via `protoInt64`; `Timestamp`/`Duration` cross through the `timestamp*`/`duration*` bridges; `Any` payloads unpack only with a `Registry`. Keep `readUnknownFields`/`ignoreUnknownFields` on for forward-compat.
- Pick the codec by the C# mint format — proto here, CBOR/MessagePack/JSON-Patch at the sibling pages; one `codec` page, one codec.

[RAIL_LAW]:
- Package: `@bufbuild/protobuf`
- Owns: the schema-first proto runtime (`create`/`clone`/`merge`/`equals`/`isMessage`), the binary + JSON codec (`fromBinary`/`toBinary`/`fromJson`/`toJson` + size-delimited streaming), the descriptor reflection engine (`createFileRegistry`/`reflect`/`buildPath`), extensions + options, the well-known types (`Any`/`Timestamp`/`Duration`/`Struct`), and the low-level `./wire` primitives (`BinaryReader`/`BinaryWriter`/`WireType`/base64)
- Accept: generated `GenMessage` schemas from `@bufbuild/protoc-gen-es`, a C#-minted `FileDescriptorSet` decoded through `FileDescriptorSetSchema`, `protoInt64` for 64-bit fields, a `Registry` for `Any`/extension resolution, `Effect.try`/`Stream` wrapping for the error and streaming rails, `Schema.decode` as the domain boundary above the wire shape
- Reject: `new`-ing a message or calling a method on it (schema-first only), a hand-authored proto shape, a decoded proto reused as a domain model, `Number`-coercing a 64-bit field, a second content-hash over anything but `toBinary` canonical bytes, reflection where a generated schema exists, and reaching for CBOR/MessagePack/JSON-Patch on a proto family
