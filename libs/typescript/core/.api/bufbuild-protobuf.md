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

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:---------------------------------------------------------------------------------- |:----------------- |:-------------------------------------------------------------------------------- |
| [01] | `Message<TypeName>` (`{ readonly $typeName }`) | message brand | the plain-object shape every decoded proto is; discriminate on `$typeName`, never `instanceof` |
| [02] | `MessageShape<Desc>` / `MessageInitShape<Desc>` / `MessageJsonType<Desc>` / `MessageValidType<Desc>` | derived shapes | `codec/*` — the runtime value, the `create` init (partial/oneof-tagged), the JSON projection, the validated form |
| [03] | `DescMessage` / `DescEnum` / `DescField` / `DescFile` / `DescService` / `DescMethod` / `DescOneof` / `DescExtension` / `DescEnumValue` / `DescComments` | descriptor union | `interchange/contract` — the reflected schema graph the drift gate walks; `DescField` is a scalar/list/message/enum/map discriminated union on `fieldKind`, with `number`/`name` coordinates, `scalar: ScalarType` on the scalar arm, `message: DescMessage`/`enum: DescEnum` refs, `listKind` + leaf on the list arm, `mapKey: ScalarType` + `mapKind` + leaf on the map arm, `delimitedEncoding`/`packed`/`longAsString` wire facts; `DescMessage.fields: DescField[]` and `DescService.methods: DescMethod[]` are the walk edges; `DescMethod.methodKind` is the closed `"unary" \| "server_streaming" \| "client_streaming" \| "bidi_streaming"` axis and `localName` the TS member name on every descriptor |
| [04] | `ScalarType` (enum: `DOUBLE`/`FLOAT`/`INT64`/`UINT64`/`INT32`/`FIXED*`/`BOOL`/`STRING`/`BYTES`/`UINT32`/`SFIXED*`/`SINT32`/`SINT64`) / `ScalarValue<T,LongAsString>` | scalar vocabulary | `interchange/contract` — the leaf type of a `DescField`; `ScalarValue` maps a `ScalarType` to its TS type (INT64→`bigint`) |
| [05] | `UnknownField` (`{ no, wireType, data }`) / `DescMethodUnary` / `DescMethodServerStreaming` / `DescMethodClientStreaming` / `DescMethodBiDiStreaming` | unknown + method kinds | preserved fields under `readUnknownFields`; the `methodKind`-tagged method descriptors `interchange/invoke` dispatches on |

[PUBLIC_TYPE_SCOPE]: generated-symbol family — what `protoc-gen-es` emits and the `codec/*` pages import
- rail: proto codec
- the generated `_pb.ts` exports a `GenMessage`/`GenEnum` const per type; it is a `Desc*` carrying the runtime + JSON type parameters so `fromBinary(schema, …)` infers `MessageShape` with zero manual typing.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------- |:---------------- |:--------------------------------------------------------------- |
| [01] | `GenFile` (`= DescFile`) / `GenMessage<Shape,Opt>` / `GenEnum<Shape,Json>` / `GenService<Methods>` / `GenExtension<Extendee,Value>` | generated symbols | `codec/*` — the schema consts the `_pb.ts` exports; a `codec` page imports the `GenMessage` and never re-declares the shape |
| [02] | `JsonValue` / `JsonObject` | JSON algebra | the recursive JSON type `toJson` returns and `fromJson` accepts; the `Struct`↔`JsonValue` bridge target |

[PUBLIC_TYPE_SCOPE]: codec options — the read/write policy knobs
- rail: proto codec
- every codec entry takes a `Partial<…Options>`; the drift-tolerant defaults (`ignoreUnknownFields`, `readUnknownFields`) are the round-trip-safe posture `interchange/contract` relies on so an unknown field is preserved, never a decode fault.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:--------------------------------------------------------------- |:------------- |:---------------------------------------------------------------- |
| [01] | `BinaryReadOptions` (`readUnknownFields`, `recursionLimit`) | binary read | `codec/*` — `readUnknownFields: true` preserves forward-compat fields; `recursionLimit` (default 100) bounds adversarial nesting |
| [02] | `BinaryWriteOptions` (`writeUnknownFields`) | binary write | egress round-trip; write back the unknown fields a partial peer decoded |
| [03] | `JsonReadOptions` (`ignoreUnknownFields`, `registry`, `recursionLimit`) / `JsonWriteOptions` (`alwaysEmitImplicit`, `enumAsInteger`, `useProtoFieldName`, `registry`) / `JsonWriteStringOptions` (`+ prettySpaces`) | json read/write | debug/text egress; `registry` resolves `Any`/extensions during JSON; `enumAsInteger` and `useProtoFieldName` control the wire dialect |

[PUBLIC_TYPE_SCOPE]: registry + reflect — the reflection surface the drift gate and content-key walk consume
- rail: descriptor reflection

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:--------------------------------------------------------------- |:-------------- |:--------------------------------------------------------------- |
| [01] | `Registry` (`get`/`getMessage`/`getEnum`/`getExtension`/`getExtensionFor`/`getService`) | type registry | `codec/*` — the `unpackAny`/extension resolver; `getExtensionFor(extendee, no)` finds an extension by field number |
| [02] | `MutableRegistry` (`+ add`/`remove`) / `FileRegistry` (`+ files`, `getFile`) | mutable / file registry | `interchange/contract` — `FileRegistry` is what `createFileRegistry` returns; iterate `files` to walk the descriptor graph |
| [03] | `ReflectMessage` / `ReflectList<V>` / `ReflectMap<K,V>` | dynamic accessor | `interchange/contract` — field-by-field read/write over a descriptor with no generated type; the drift walk and content-key projection |
| [04] | `Path` / `PathBuilder` | field path | `interchange/codec`, `interchange/format` — a typed field-mask address into a `DescMessage`; the parity/patch-target vocabulary |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: message lifecycle — construct, copy, compare (all schema-first)
- rail: proto codec

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:---------------------------------------------------------------------------------------------- |:------------- |:----------------------------------------------------------- |
| [01] | `create<Desc>(schema, init?: MessageInitShape<Desc>): MessageShape<Desc>` | constructor | `codec/*` — the ONLY way to mint a message; `init` is partial + oneof-tagged, defaults fill the rest |
| [02] | `clone<Desc>(schema, message): MessageShape<Desc>` / `merge<Desc>(schema, target, source): void` | copy / merge | deep copy; `merge` folds `source` into `target` (proto merge semantics — repeated concatenate, singular overwrite) |
| [03] | `equals<Desc>(schema, a, b, options?: EqualsOptions): boolean` | structural eq | `EqualsOptions` = `registry`/`unpackAny`/`extensions`/`unknown`; deep proto equality, not reference |
| [04] | `isMessage<Desc>(arg, schema?): arg is MessageShape<Desc>` | type guard | narrow an `unknown` decode result; with `schema` it also checks `$typeName` |
| [05] | `protoInt64: Int64Support` (`parse`/`uParse`/`enc`/`uEnc`/`dec`/`uDec`/`zero`/`supported`) | 64-bit bridge | `codec/*` — the `bigint`↔`{lo,hi}` bridge for INT64/UINT64 fields; `parse` a string/number to `bigint` |

[ENTRYPOINT_SCOPE]: binary codec — the wire ingress/egress every `codec/*` page runs
- rail: proto codec
- the canonical entries are `fromBinary`/`toBinary` (whole message) and the `sizeDelimited*` family (length-prefixed frames). `BinaryReader`/`BinaryWriter` are the sub-message primitives; `codec` pages compose the high-level entries, never the low-level reader unless authoring a custom field.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:---------------------------------------------------------------------------------------------- |:------------- |:----------------------------------------------------------- |
| [01] | `fromBinary<Desc>(schema, bytes, options?): MessageShape<Desc>` / `mergeFromBinary<Desc>(schema, target, bytes, options?)` | binary read | `codec/*` — decode a C#-minted `*Wire` payload; `merge` accumulates a partial into an existing message |
| [02] | `toBinary<Desc>(schema, message, options?): Uint8Array` | binary write | egress + the CANONICAL byte source for the `value/identity` `XxHash128` content key (proto is deterministic per field order) |
| [03] | `sizeDelimitedEncode<Desc>(desc, message, options?): Uint8Array` / `sizeDelimitedDecodeStream<Desc>(desc, iterable: AsyncIterable<Uint8Array>, options?)` / `sizeDelimitedPeek(data): { size: number; offset: number; eof: false } \| { size: null; offset: null; eof: true }` | framed stream | `interchange/codec`, `frame/geometry` — length-prefixed streaming decode over an async byte iterable; `peek` reads a frame header without consuming — a complete varint answers `size`+`offset` with `eof: false`, an incomplete one answers nulls with `eof: true` |
| [04] | `BinaryReader` / `BinaryWriter` (classes) / `WireType` (enum) / `base64Encode(bytes, encoding?: "std"\|"std_raw"\|"url")` / `base64Decode(str): Uint8Array` | low-level primitives | `frame/*`, `codec/*` — tag/varint/fixed reader+writer for custom fields; base64 for the JSON `bytes` dialect |

[ENTRYPOINT_SCOPE]: JSON codec — the debug/text mirror of the binary rail
- rail: proto codec

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:---------------------------------------------------------------------------------------------- |:------------- |:----------------------------------------------------------- |
| [01] | `fromJson<Desc>(schema, json: JsonValue, options?)` / `fromJsonString<Desc>(schema, string, options?)` / `mergeFromJson` / `mergeFromJsonString` | json read | text ingress (fixtures, config); `ignoreUnknownFields` is the drift-safe default |
| [02] | `toJson<Desc>(schema, message, options?): JsonValue` / `toJsonString<Desc>(schema, message, options?): string` | json write | `interchange/codec` diagnostic projection, snapshot fixtures; `prettySpaces` for readable dumps |
| [03] | `enumToJson<Desc>(descEnum, value)` / `enumFromJson<Desc>(descEnum, json)` / `isEnumJson<Desc>(descEnum, value)` | enum json | enum name↔number crossing; guard an untrusted enum literal |

[ENTRYPOINT_SCOPE]: registry + reflection — the descriptor-driven path with no generated code
- rail: descriptor reflection
- `createFileRegistry(FileDescriptorSet)` is the hinge: it turns a compiled descriptor set (`buf build -o descriptor.binpb`, or the C#-minted set the seam ships) into runtime descriptors, so `interchange/contract` diffs schema versions and `reflect` reads fields by descriptor alone.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:---------------------------------------------------------------------------------------------- |:------------- |:----------------------------------------------------------- |
| [01] | `createFileRegistry(fileDescriptorSet)` / `createFileRegistry(proto, resolve)` / `createFileRegistry(...registries)` | file registry | `interchange/contract` — the reflection entry; decode the C#-minted set with `fromBinary(FileDescriptorSetSchema, bytes)`, then build the registry to walk `files` |
| [02] | `createRegistry(...input)` / `createMutableRegistry(...input)` | type registry | assemble the `unpackAny`/extension resolver from generated schemas; mutable form for incremental registration |
| [03] | `reflect<Desc>(desc, message?, …): ReflectMessage` / `reflectList<V>(field,…)` / `reflectMap<K,V>(field,…)` | dynamic reflect | `interchange/contract` — field-by-field read/write over a descriptor; the drift walk and the content-key field projection |
| [04] | `buildPath(schema): PathBuilder` / `parsePath(schema, path, options?): Path` / `pathToString(path)` / `InvalidPathError` | field path | `interchange/codec`, `interchange/format` — build/parse a typed field-mask address; the parity target vocabulary the drift gate reports |
| [05] | `qualifiedName(desc)` / `protoCamelCase(s)` / `protoSnakeCase(s)` / `safeObjectProperty(s)` / `scalarEquals` / `scalarZeroValue` / `isScalarZeroValue` | reflect helpers | descriptor name canonicalization and scalar default/equality used inside the drift walk |

[ENTRYPOINT_SCOPE]: extensions + well-known types — `Any` packing, time bridges, the `Struct` codec
- rail: proto codec

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:---------------------------------------------------------------------------------------------- |:------------- |:----------------------------------------------------------- |
| [01] | `getExtension` / `setExtension` / `clearExtension` / `hasExtension` / `getOption` / `hasOption` / `createExtensionContainer` | extensions | `interchange/contract` — read custom options off a `Desc*` (the `FaultDetail` vocabulary hook, SI-scalar `QuantityFamily` annotations on `interchange/format`) |
| [02] | `anyPack<Desc>(schema, message[, into])` / `anyUnpack(any, registry)` / `anyUnpack<Desc>(any, schema)` / `anyUnpackTo` / `anyIs(any, schema\|typeName)` | `Any` codec | polymorphic payload boxing; `anyUnpack` needs the `Registry` to resolve the boxed type by URL |
| [03] | `timestampFromDate(date)` / `timestampDate(ts)` / `timestampFromMs` / `timestampMs` / `timestampNow` / `durationFromMs` / `durationMs` | time bridge | `kernel` `Hlc`/instant crossing — proto `Timestamp`↔JS `Date`/ms without a hand-rolled epoch conversion |
| [04] | `FileDescriptorSetSchema` / `FileDescriptorProtoSchema` / `StructSchema` / `ValueSchema` / `ListValueSchema` / `AnySchema` / `TimestampSchema` / `DurationSchema` (from `./wkt`) | wkt schemas | `interchange/contract` decodes `FileDescriptorSetSchema`; `Struct`/`Value` bridge a `JsonValue` into a proto field |
| [05] | `isWrapper(msg)` / `isWrapperDesc(desc)` / `hasCustomJsonRepresentation(desc)` / `configureTextEncoding` / `parseTextFormatScalarValue` / `parseTextFormatEnumValue` | wkt + text guards | wrapper-type detection during JSON; pluggable `TextEncoder` for non-browser runtimes; text-format field parsing |

[ENTRYPOINT_SCOPE]: codegen boot — the generated-code side (`./codegenv2`, authored by `protoc-gen-es`, not hand-called)
- rail: proto codec
- these are what a `_pb.ts` file calls to reconstitute descriptors from an embedded base64 `FileDescriptorProto`; a `codec` page imports the resulting `GenMessage` const, never these functions directly.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:---------------------------------------------------------------------------------------------- |:------------- |:----------------------------------------------------------- |
| [01] | `fileDesc(b64, imports?): DescFile` / `boot` / `bootFileDescriptorProto` | file boot | generated `_pb.ts` — reconstruct the `DescFile` from the embedded descriptor |
| [02] | `messageDesc(file, path, …)` / `enumDesc(file, path, …)` / `serviceDesc(file, path, …)` / `extDesc(file, path, …)` / `tsEnum(desc)` | symbol boot | generated `_pb.ts` — index the `GenMessage`/`GenEnum`/`GenService`/`GenExtension` out of the file; `tsEnum` materializes a TS enum object |

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
