# [TS_API_BUFBUILD_PROTOBUF]

`@bufbuild/protobuf` owns the schema-first proto runtime under every corpus-family decode — binary, ProtoJSON, and the size-delimited stream frame — and the descriptor registry `Any` unpacking, Connect details, and protovalidate resolve through. Messages are plain data branded by `$typeName`; every operation takes the descriptor first, and `create(schema, init?)` is the sole constructor.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: message model + descriptor family — the schema-first core every op discriminates on
- `Message` is data (`$typeName` brand), `Desc*` the schema; `MessageShape<Desc>`/`MessageInitShape<Desc>` are the derived runtime and init types a `codec/*` page types decoded values by, never a re-declared interface.

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]     | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------ | :---------------- | :---------------------------------------------------- |
|  [01]   | `Message<TypeName>`                                     | message brand     | the `{ readonly $typeName }` brand of a decoded proto |
|  [02]   | `MessageShape<Desc>`                                    | derived shape     | the runtime value of a decoded message                |
|  [03]   | `MessageInitShape<Desc>`                                | derived shape     | the `create` init — partial + oneof-tagged            |
|  [04]   | `MessageJsonType<Desc>`                                 | derived shape     | the JSON projection of the message                    |
|  [05]   | `MessageValidType<Desc>`                                | derived shape     | the validated form                                    |
|  [06]   | `DescMessage` / `DescEnum` / `DescField` / `DescOneof`  | descriptor union  | the reflected schema graph; no branch page walks it   |
|  [07]   | `DescFile` / `DescService` / `DescMethod`               | descriptor union  | file/service/method descriptor nodes                  |
|  [08]   | `DescExtension` / `DescEnumValue` / `DescComments`      | descriptor union  | extension, enum-value, and doc-comment leaves         |
|  [09]   | `ScalarType` (enum)                                     | scalar vocabulary | the scalar leaf type of a `DescField`                 |
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

[PUBLIC_TYPE_SCOPE]: `protoc-gen-es` options — the `opt` vocabulary the root `buf.gen.yaml` row selects from; an unrecognized key fails the run

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :---------------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `target=js\|ts\|dts`                      | option        | which files emit; `+`-combined, default `js+dts`                  |
|  [02]   | `import_extension=none\|js\|ts`           | option        | extension on the plugin's OWN relative imports; default `none`    |
|  [03]   | `erasable_syntax=true`                    | option        | proto enums as an `objEnum` `as const` rather than a TS `enum`    |
|  [04]   | `js_import_style=module\|legacy_commonjs` | option        | ESM `import`/`export` or CommonJS `require`; default `module`     |
|  [05]   | `json_types=true`                         | option        | emits the JSON shape type beside each message and enum            |
|  [06]   | `valid_types=<rules>`                     | option        | required-rule fields as non-optional properties                   |
|  [07]   | `keep_empty_files=true`                   | option        | emits a source that generated no code; the default drops it       |
|  [08]   | `ts_nocheck=true`                         | option        | prefixes the emission with `@ts-nocheck`                          |
|  [09]   | `rewrite_imports=<pattern>:<target>`      | option        | redirects a matched import path; withheld from the preamble stamp |
|  [10]   | `elide_plugin_version` / `bootstrap_wkt`  | option        | drops the version from the stamp; bootstraps the well-known types |

- [VALID_TYPES]: `valid_types` takes `legacy_required` and `protovalidate_required`, `+`-combined; upstream ships it and `erasable_syntax` on its unstable track, so a tree generated under either re-verifies on every plugin bump.
- [OPTION_STAMP]: each emitted file opens with `@generated by protoc-gen-es v<version> with parameter "<opts>"`, so both the plugin version and the option list are regeneration triggers a stale tree announces about itself.
- [BRANCH_OPTIONS]: three options answer the root `tsconfig.json`. `target=ts` binds today's emission, since the default publishes a `.js`+`.d.ts` pair to a branch whose gate is `tsc --noEmit` over sources. `erasable_syntax=true` emits the live corpus's enums as erasable objects, and `import_extension=ts` gives every generated peer import the same extension the branch sources use under `allowImportingTsExtensions`.

[PUBLIC_TYPE_SCOPE]: codec options — the read/write policy knobs
- every codec entry takes a `Partial<…Options>`, and the two readers DEFAULT OPPOSITE WAYS: `fromBinary` keeps unknown fields (`readUnknownFields: true`) while `fromJson` REFUSES them (`ignoreUnknownFields: false`) — so the reader posture is spelled once as `interchange/format`'s `_READ`/`_JSON_READ` rows and passed at every site; source compatibility is decided by the corpus breaking gate before generation.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                                          |
| :-----: | :--------------------------- | :------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `BinaryReadOptions`          | binary read   | `readUnknownFields` default TRUE keeps forward-compat fields; `recursionLimit` 100    |
|  [02]   | `BinaryWriteOptions`         | binary write  | `writeUnknownFields` — write back a partial peer's unknown fields                     |
|  [03]   | `JsonReadOptions`            | json read     | `ignoreUnknownFields` default FALSE (refuses), `registry` for `Any`, `recursionLimit` |
|  [04]   | `JsonWriteOptions`           | json write    | `alwaysEmitImplicit`, `enumAsInteger`, `useProtoFieldName`, `registry` — wire dialect |
|  [05]   | `JsonWriteStringOptions`     | json write    | `JsonWriteOptions` + `prettySpaces`                                                   |
|  [06]   | `SizeDelimitedDecodeOptions` | framed read   | `BinaryReadOptions` + `readMaxBytes` — per-message stream cap, default 64 MiB         |
|  [07]   | `TextReadOptions`            | text read     | `registry` (`Any`/extensions), `recursionLimit` default 100                           |
|  [08]   | `TextWriteOptions`           | text write    | `printUnknownFields` (default false, by-number, non-round-trippable), `registry`      |

[PUBLIC_TYPE_SCOPE]: registry + reflect — the registry every resolver shares; the reflect surface has no branch consumer

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [CAPABILITY]                                                                        |
| :-----: | :--------------------- | :--------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `Registry`             | type registry    | resolver `get`/`getMessage`/`getEnum`/`getExtension`/`getExtensionFor`/`getService` |
|  [02]   | `MutableRegistry`      | mutable registry | `Registry` + `add`/`remove` for incremental registration                            |
|  [03]   | `FileRegistry`         | file registry    | `Registry` + `files`/`getFile`; the `createFileRegistry` result                     |
|  [04]   | `ReflectMessage`       | dynamic accessor | field-by-field read/write over a `DescMessage`, no generated type; unmined          |
|  [05]   | `ReflectList<V>`       | dynamic accessor | list-field accessor; `isReflectList`/`isReflectMap`/`isReflectMessage` guard them   |
|  [06]   | `ReflectMap<K,V>`      | dynamic accessor | map-field accessor; unmined                                                         |
|  [07]   | `Path` / `PathBuilder` | field path       | typed field address; `pathToString` renders a protovalidate `Violation.field`       |
|  [08]   | `UnknownEnum`          | open enum        | the branded number an open enum decodes a foreign value to; `isUnknownEnum` guards  |
|  [09]   | `FieldError`           | reflect error    | `isFieldError` guard; raised by a reflect write the descriptor refuses              |
|  [10]   | `SupportedEdition`     | edition bound    | `minimumEdition`/`maximumEdition` — the editions this runtime boots a descriptor at |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: message lifecycle — construct, copy, compare, all schema-first

| [INDEX] | [SURFACE]                                                                  | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `create<Desc>(schema, init?: MessageInitShape<Desc>): MessageShape<Desc>`  | static  | the only message constructor                 |
|  [02]   | `clone<Desc>(schema, message): MessageShape<Desc>`                         | static  | deep structural copy                         |
|  [03]   | `merge<Desc>(schema, target, source): void`                                | static  | proto merge fold                             |
|  [04]   | `equals<Desc>(schema, a, b, options?: EqualsOptions): boolean`             | static  | deep proto equality                          |
|  [05]   | `isMessage<Desc>(arg, schema?): arg is MessageShape<Desc>`                 | static  | narrows a decode result; checks `$typeName`  |
|  [06]   | `protoInt64: Int64Support`                                                 | const   | `bigint`↔`{lo,hi}` bridge                    |
|  [07]   | `isFieldSet<Desc>(message, field): boolean` / `clearField(message, field)` | static  | presence read and clear over one `DescField` |
|  [08]   | `isUnknownEnum(desc, value): value is UnknownEnum`                         | static  | classify a foreign value on an open enum     |

- [MERGE]: `merge` folds `source` into `target` — repeated fields concatenate, singular fields overwrite.
- [EQUALS]: `EqualsOptions` = `registry`/`unpackAny`/`extensions`/`unknown`; structural, never reference equality.
- [PROTOINT64]: `parse`/`uParse`/`enc`/`uEnc`/`dec`/`uDec`/`zero`/`supported` — the `bigint`↔`{lo,hi}` bridge for INT64/UINT64; `parse` a string/number, `zero` the identity.

[ENTRYPOINT_SCOPE]: binary codec — the wire ingress/egress every `codec/*` page runs
- import whole-message codecs from `@bufbuild/protobuf`, the `sizeDelimited*` family from `@bufbuild/protobuf/wire`; `codec` pages compose the high-level `fromBinary`/`toBinary`, never `BinaryReader`/`BinaryWriter` unless authoring a custom field.

| [INDEX] | [SURFACE]                                                         | [SHAPE] | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------------- | :------ | :-------------------------------------------- |
|  [01]   | `fromBinary<Desc>(schema, bytes, options?): MessageShape<Desc>`   | static  | decode a conforming `*Wire` payload           |
|  [02]   | `mergeFromBinary<Desc>(schema, target, bytes, options?)`          | static  | accumulate a partial into a message           |
|  [03]   | `toBinary<Desc>(schema, message, options?): Uint8Array`           | static  | deterministic descriptor-order egress         |
|  [04]   | `sizeDelimitedEncode<Desc>(desc, message, options?): Uint8Array`  | static  | length-prefix a frame for egress              |
|  [05]   | `sizeDelimitedDecodeStream<Desc>(desc, iterable, options?)`       | static  | streaming decode; `readMaxBytes` caps a frame |
|  [06]   | `sizeDelimitedPeek(data)`                                         | static  | read a frame header without consuming         |
|  [07]   | `BinaryReader` (class)                                            | ctor    | tag/varint/fixed reader for custom fields     |
|  [08]   | `BinaryWriter` (class)                                            | ctor    | tag/varint/fixed writer for custom fields     |
|  [09]   | `WireType` (enum)                                                 | enum    | the field wire-type discriminant              |
|  [10]   | `base64Encode(bytes, encoding?: "std"\|"std_raw"\|"url"): string` | static  | base64 for the JSON `bytes` dialect           |
|  [11]   | `base64Decode(str): Uint8Array`                                   | static  | base64 decode for the JSON `bytes` dialect    |
|  [12]   | `configureTextEncoding(textEncoding): void`                       | static  | swap the encoder pair on a host lacking one   |
|  [13]   | `parseTextFormatScalarValue(ScalarType, string)`                  | static  | parse one text-format scalar leaf             |
|  [14]   | `parseTextFormatEnumValue(DescEnum, string): number`              | static  | parse one text-format enum literal            |

- `sizeDelimitedPeek`: returns `{ size, offset, eof:false }` for a complete varint header, `{ size:null, offset:null, eof:true }` when the varint is incomplete.
- `sizeDelimitedDecodeStream`: `readMaxBytes` (default 64 MiB) caps one stream message and raises a bare `Error` before the frame buffers; `interchange/format`'s `framed` folds `sizeDelimitedPeek` on the rail instead so the overrun refuses TYPED with its measured size under `Shape.Ingress.floor.bytes`, and the stream decoder stays unmined for that one reason.

[ENTRYPOINT_SCOPE]: JSON codec — the debug/text mirror of the binary rail

| [INDEX] | [SURFACE]                                               | [SHAPE] | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------ | :------ | :---------------------------------------------------- |
|  [01]   | `fromJson<Desc>(schema, json: JsonValue, options?)`     | static  | text ingress; pass `_JSON_READ` — it REFUSES unknowns |
|  [02]   | `fromJsonString<Desc>(schema, string, options?)`        | static  | text ingress from a JSON string                       |
|  [03]   | `mergeFromJson`                                         | static  | fold JSON into an existing message                    |
|  [04]   | `mergeFromJsonString`                                   | static  | fold a JSON string into a message                     |
|  [05]   | `toJson<Desc>(schema, message, options?): JsonValue`    | static  | diagnostic projection; text egress                    |
|  [06]   | `toJsonString<Desc>(schema, message, options?): string` | static  | readable dump; `prettySpaces`                         |
|  [07]   | `enumToJson<Desc>(descEnum, value)`                     | static  | enum number→name crossing                             |
|  [08]   | `enumFromJson<Desc>(descEnum, json)`                    | static  | enum name→number crossing                             |
|  [09]   | `isEnumJson<Desc>(descEnum, value)`                     | static  | guard an untrusted enum literal                       |

[ENTRYPOINT_SCOPE]: text-format codec — the `./txtpb` whole-message mirror (txtpbfmt-shaped, BigInt-only)
- import `toText`/`fromText`/`mergeFromText` from `@bufbuild/protobuf/txtpb`; 64-bit fields render as `bigint` with no string fall-back and `toText`/`fromText` throw where `BigInt` is absent. `printUnknownFields` prints by number and is NOT round-trippable — `fromText` rejects number-named fields.

| [INDEX] | [SURFACE]                                                    | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `toText<Desc>(schema, message, options?): string`            | static  | txtpbfmt-formatted diagnostic dump           |
|  [02]   | `fromText<Desc>(schema, text, options?): MessageShape<Desc>` | static  | parse a text-format payload                  |
|  [03]   | `mergeFromText<Desc>(schema, target, text, options?)`        | static  | fold a text payload into an existing message |

[ENTRYPOINT_SCOPE]: registry + reflection — the descriptor-driven path with no generated code
- `createFileRegistry(FileDescriptorSet)` turns a compiled descriptor set into runtime descriptors, and `reflect` reads fields by descriptor alone; the generated `file_*` consts already carry every estate descriptor, so a decoded set enters only where a peer ships one.

| [INDEX] | [SURFACE]                                                     | [SHAPE] | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------ | :------ | :------------------------------------------------ |
|  [01]   | `createFileRegistry(fileDescriptorSet)`                       | static  | decode a conforming set, then walk `files`        |
|  [02]   | `createFileRegistry(proto, resolve)`                          | static  | build from a proto with a resolver                |
|  [03]   | `createFileRegistry(...registries)`                           | static  | merge existing registries                         |
|  [04]   | `createRegistry(...input)`                                    | static  | assemble the `Any`/extension resolver             |
|  [05]   | `createMutableRegistry(...input)`                             | static  | the incremental-registration form                 |
|  [06]   | `reflect<Desc>(desc, message?, …): ReflectMessage`            | static  | field-by-field read/write over a descriptor       |
|  [07]   | `reflectList<V>(field,…)`                                     | static  | list-field reflect accessor                       |
|  [08]   | `reflectMap<K,V>(field,…)`                                    | static  | map-field reflect accessor                        |
|  [09]   | `buildPath(schema): PathBuilder`                              | static  | build a typed field-mask address                  |
|  [10]   | `parsePath(schema, path, options?): Path`                     | static  | parse a field-mask string to a `Path`             |
|  [11]   | `pathToString(path)`                                          | static  | render a `Path` to string                         |
|  [12]   | `InvalidPathError`                                            | ctor    | the field-mask parse error                        |
|  [13]   | `qualifiedName(desc)`                                         | static  | descriptor qualified-name                         |
|  [14]   | `protoCamelCase(s)` / `protoSnakeCase(s)`                     | static  | field-name case canonicalization                  |
|  [15]   | `safeObjectProperty(s)`                                       | static  | safe property-name projection                     |
|  [16]   | `scalarEquals` / `scalarZeroValue` / `isScalarZeroValue`      | static  | scalar default + equality; unmined                |
|  [17]   | `nestedTypes(desc)` / `usedTypes(desc)` / `parentTypes(desc)` | static  | descriptor graph walks; unmined                   |
|  [18]   | `isReflectList` / `isReflectMap` / `isReflectMessage`         | static  | reflect accessor guards; unmined                  |
|  [19]   | `getTextEncoding()` (`./wire`)                                | static  | read the encoder pair `configureTextEncoding` set |

[ENTRYPOINT_SCOPE]: extensions + well-known types — `Any` packing, time bridges, the `Struct` codec
- `./wkt` schema consts (`StructSchema`/`ValueSchema`/`AnySchema`/`TimestampSchema`/`DurationSchema`/`EmptySchema` …) are the descriptors `interchange/format`'s `any`/`struct`/`value` bridges and `interchange/codec`'s stamp refinements take first; `FileDescriptorSetSchema` has no branch reader because the generated `file_*` consts already carry every descriptor.

| [INDEX] | [SURFACE]                                          | [SHAPE] | [CAPABILITY]                         |
| :-----: | :------------------------------------------------- | :------ | :----------------------------------- |
|  [01]   | `getExtension`                                     | static  | read a custom option off a `Desc*`   |
|  [02]   | `setExtension`                                     | static  | set a custom option on a `Desc*`     |
|  [03]   | `clearExtension`                                   | static  | clear a custom option                |
|  [04]   | `hasExtension`                                     | static  | test a custom option's presence      |
|  [05]   | `getOption`                                        | static  | read a descriptor option             |
|  [06]   | `hasOption`                                        | static  | test a descriptor option             |
|  [07]   | `anyPack<Desc>(schema, message[, into])`           | static  | box a message into an `Any`          |
|  [08]   | `anyUnpack(any, registry)`                         | static  | unbox via a `Registry` by type URL   |
|  [09]   | `anyUnpack<Desc>(any, schema)`                     | static  | unbox against a known schema         |
|  [10]   | `anyUnpackTo`                                      | static  | unbox into an existing target        |
|  [11]   | `anyIs(any, schema\|typeName)`                     | static  | test a boxed type                    |
|  [12]   | `timestampFromDate(date)` / `timestampDate(ts)`    | static  | `Timestamp`↔JS `Date`                |
|  [13]   | `timestampFromMs` / `timestampMs` / `timestampNow` | static  | `Timestamp`↔ms, and now              |
|  [14]   | `durationFromMs` / `durationMs`                    | static  | `Duration`↔ms                        |
|  [15]   | `isWrapper(msg)` / `isWrapperDesc(desc)`           | static  | wrapper-type detection during JSON   |
|  [16]   | `hasCustomJsonRepresentation(desc)`                | static  | custom-JSON representation detection |

[ENTRYPOINT_SCOPE]: codegen boot — the generated-code side (`./codegenv2`, authored by `protoc-gen-es`, not hand-called)
- Generated `_pb.ts` files call these to reconstitute descriptors from an embedded base64 `FileDescriptorProto`; a `codec` page imports the resulting `GenMessage` const, never these functions.

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

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every operation reads `(schema, value, options?)` — descriptor first, the message plain data carrying only `$typeName`, and `create(schema, init?)` the sole constructor. Each `codec/*` page imports the `GenMessage` from generated `_pb.ts`, `fromBinary(Schema, bytes)` infers `MessageShape<Schema>` with no annotation, and a decoded value discriminates by `$typeName` or `isMessage(v, Schema)`, never `instanceof`.
- INT64/UINT64 fields are `bigint` (or `string` under long-as-string codegen), bridged by `protoInt64` — `.parse(s)` from a string, `.zero` the identity; `Number`-coercing a 64-bit field loses precision past 2^53.
- `toBinary(schema, msg)` emits descriptor-ordered fields, but map insertion order and retained unknown-field order remain observable.
- Byte-addressed identity retains producer octets; semantic identity owns an explicit canonical projection above protobuf encoding.
- Compatibility is the corpus emission's — every binding regenerates from one reshaped source — so no page diffs descriptors at runtime; `readUnknownFields` preserves binary forward residue and `ignoreUnknownFields` states whether ProtoJSON accepts or refuses an unrecognized peer field.
- Validation is `@bufbuild/protovalidate`'s (`.api/bufbuild-protovalidate.md`) over this runtime's descriptors — the branch validates nothing by hand; `reflect`, `buildPath`/`parsePath`, and `ScalarType` have NO branch consumer (the content-key parity fold compares digests and round-trips through `Schema`, never a descriptor walk), and `pathToString` alone is read, to render a `Violation.field`.

[STACKING]:
- `@connectrpc/connect`(`.api/connectrpc-connect.md`), `@connectrpc/connect-web`(`.api/connectrpc-connect-web.md`): this runtime IS the Connect message layer — a `DescMethod` carries `GenMessage` input/output schemas and the transport calls `toBinary`/`fromBinary` internally, while `interchange/invoke` picks the protocol axis (`connect` | `grpc-web`) and never re-implements the runtime.
- `effect`(`libs/typescript/.api/effect.md`): `fromBinary`/`fromJson` yield the WIRE shape and an owned `Schema` whose ENCODED side is that `MessageShape` lifts it into branded vocabulary — proto is transport, `Schema` is domain, a proto message never a domain model; a synchronous codec call that throws on malformed bytes wraps in `Either.try` inside one `Schema.transformOrFail`, and the size-delimited frame rides a `Channel` fold over `sizeDelimitedPeek`/`sizeDelimitedEncode` (`interchange/format` `framed`) that `runtime:net/channel`'s proto row composes under `ChannelSchema.duplexUnknown`.
- `interchange/codec`: each wire-family row selects one codec and names its framing (`binary` | `json`); semantic contract families ride ProtoJSON through `fromJson`/`toJson` over their generated descriptors.
- `@bufbuild/protovalidate`(`.api/bufbuild-protovalidate.md`): `createValidator({ registry })` takes this runtime's `Registry` and `validate(schema, message)` its descriptors; `interchange/format` runs it behind the `$typeName` guard on every admission and egress.
- `value/identity` (within-lib edge): producer octets are the byte-identity input; semantic identity uses its owned canonical projection.

[LOCAL_ADMISSION]:
- Import the generated `GenMessage` schema and call the schema-first codec (`fromBinary`/`toBinary`/`create`); cross a decoded proto into `kernel` vocabulary through `Schema.decode` at the page boundary, never hand-authoring a shape or reusing a decoded proto as a domain model.
- Every branch decode imports its generated schema directly; `reflect` has no branch consumer.
- 64-bit fields are `bigint` on the decode path and need no `protoInt64` read; `Timestamp`/`Duration` cross through `timestampMs`/`durationMs` typed against `TimestampSchema`/`DurationSchema`; `Any` unpacks only against `interchange/format`'s one registry; `_READ` and `_JSON_READ` are passed at EVERY read because the JSON default refuses unknown fields.
