# [PY_BRANCH_API_PROTOBUF_PY]

`protobuf-py` owns the estate wire message runtime: every generated `_pb.py` class subclasses `Message` and carries its own binary and ProtoJSON codecs, each generated module hands back a `desc()` file descriptor, and one `Registry` resolves type names for `Any`, JSON `@type`, and extensions. Generator and runtime pin as one pair, and `protobuf.plugin` mints an estate generator against those same descriptors.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `protobuf-py`
- package: `protobuf-py` (Apache-2.0)
- module: `protobuf`
- namespaces: `protobuf`, `protobuf.wkt`, `protobuf.plugin`, `protobuf._codegen`
- abi: `protobuf-py-ext` ships `protobuf_ext.NativeMessage` as the slot-backed field store seated into every generated class's MRO on the CPython wheel matrix; every other interpreter binds a pure-Python store carrying the same surface
- rail: transport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: message family

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                                           |
| :-----: | :----------- | :------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `Message`    | generic base  | root of every generated class; `Message[Literal[field, ...]]` types the field-name set |
|  [02]   | `Enum`       | IntEnum base  | generated enum base; an open enum mints a pseudo-member for an unknown number          |
|  [03]   | `Oneof`      | value         | `Oneof[Literal[field], T]` pair a oneof attribute holds; `None` spells unset           |
|  [04]   | `Extension`  | generic value | `Extension[Extendee, T]` proto2 handle read and written through `message[ext]`         |
|  [05]   | `Registry`   | registry      | type-name resolver for `Any` packing, JSON `@type`, and extension lookup               |
|  [06]   | `ScalarType` | enum          | proto scalar vocabulary every `DescFieldValue*` names                                  |

[ScalarType]: `DOUBLE` `FLOAT` `INT64` `UINT64` `INT32` `FIXED64` `FIXED32` `BOOL` `STRING` `BYTES` `UINT32` `SFIXED32` `SFIXED64` `SINT32` `SINT64`

[PUBLIC_TYPE_SCOPE]: descriptor family
- every schema `Desc*` carries `proto`, its `descriptor_pb` message, and surfaces every proto-declared deprecation bit as `deprecated`; `DescOneof` carries `proto` alone, and `DescComments` / `DescUnknownField` carry neither.
- `DescMessage.type` / `DescEnum.type` / `DescExtension.type` bind the Python class, minting a synthetic one under `protobuf._message` for a descriptor a `Registry` built at runtime.
- `DescMethod.method_kind` is the literal `unary` / `server_streaming` / `client_streaming` / `bidi_streaming`, and `idempotency` a `MethodOptions.IdempotencyLevel`.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                                         |
| :-----: | :----------------- | :------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `DescFile`         | file schema   | `name` `edition` `dependencies` `messages` `enums` `extensions` `services`           |
|  [02]   | `DescMessage`      | message       | `type_name` `name` `file` `parent` `fields` `oneofs` `members` `nested_*` and `type` |
|  [03]   | `DescField`        | field         | `name` `local_name` `number` `json_name` `value` `presence` under its `parent`       |
|  [04]   | `DescOneof`        | oneof         | `name` `local_name` `fields` under its `parent`                                      |
|  [05]   | `DescEnum`         | enum          | `type_name` `values` `open` and `type`; `open` decides pseudo-member minting         |
|  [06]   | `DescEnumValue`    | enum value    | `name` `local_name` `number` under its `parent`                                      |
|  [07]   | `DescExtension`    | extension     | `extendee` `number` `json_name` `value` `presence` and `type`                        |
|  [08]   | `DescService`      | service       | `type_name` `name` `file` `methods`                                                  |
|  [09]   | `DescMethod`       | method        | `method_kind` literal, `input` `output` `idempotency`                                |
|  [10]   | `DescComments`     | comments      | `leading` `trailing` `leading_detached` `source_path` from source info               |
|  [11]   | `DescUnknownField` | unknown       | `number` + `value`; keys `message[...]` for wire residue the schema never names      |

[PUBLIC_TYPE_SCOPE]: field-value shapes

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :----------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `DescFieldValue`         | union         | `Scalar` / `Message` / `Enum` / `List` / `Map` — every field shape |
|  [02]   | `DescFieldValueSingular` | union         | `Scalar` / `Message` / `Enum` — the oneof-eligible narrowing       |
|  [03]   | `DescFieldValueScalar`   | value         | `scalar` `default_value` `oneof`                                   |
|  [04]   | `DescFieldValueMessage`  | value         | `message` `delimited_encoding` `oneof`                             |
|  [05]   | `DescFieldValueEnum`     | value         | `enum` `default_value` `oneof`                                     |
|  [06]   | `DescFieldValueList`     | value         | `element` `packed` `delimited_encoding`                            |
|  [07]   | `DescFieldValueMap`      | value         | `key` `value`                                                      |

[PUBLIC_TYPE_SCOPE]: well-known types (`protobuf.wkt`)
- `protobuf.wkt` re-exports every `<name>_pb` module generated for `google/protobuf/*.proto` beside every message and enum those modules declare, so one import reaches the module or the symbol, and a conversion mixin rides each message protobuf defines conversions for.
- `protobuf.wkt.Enum` is `google.protobuf.Enum`, the reflection message — take the generated-enum base as `protobuf.Enum` and never off `wkt`.

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :----------------------------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `Any`                                            | well-known    | `type_url` + `value` envelope; `pack` / `unpack` / `is_type`   |
|  [02]   | `Timestamp` / `Duration`                         | well-known    | instant and signed span; datetime, nanos, seconds conversions  |
|  [03]   | `Struct` / `Value` / `ListValue` / `NullValue`   | well-known    | JSON-shaped dynamic body; `from_python` / `to_python`          |
|  [04]   | `FieldMask`                                      | well-known    | `paths` list for partial updates                               |
|  [05]   | `Empty`                                          | well-known    | unit message                                                   |
|  [06]   | `FileDescriptorSet`                              | descriptor    | compiled file set; `to_registry()` builds a `Registry` from it |
|  [07]   | `Edition`                                        | enum          | proto edition vocabulary two module constants bound            |
|  [08]   | `CodeGeneratorRequest` / `CodeGeneratorResponse` | plugin wire   | the protoc plugin wire `protobuf.plugin.run` speaks            |

[wrappers]: `BoolValue` `BytesValue` `DoubleValue` `FloatValue` `Int32Value` `Int64Value` `StringValue` `UInt32Value` `UInt64Value`
[descriptor_pb]: `FileDescriptorProto` `DescriptorProto` `FieldDescriptorProto` `OneofDescriptorProto` `EnumDescriptorProto` `EnumValueDescriptorProto` `ServiceDescriptorProto` `MethodDescriptorProto` `SourceCodeInfo` `GeneratedCodeInfo` `ExtensionRangeOptions` `FileOptions` `MessageOptions` `FieldOptions` `OneofOptions` `EnumOptions` `EnumValueOptions` `ServiceOptions` `MethodOptions` `UninterpretedOption` `FeatureSet` `FeatureSetDefaults` `SymbolVisibility`
[reflection]: `Type` `Field` `Enum` `EnumValue` `Option` `Syntax` `SourceContext` `Api` `Method` `Mixin` `Version`
[features]: `CppFeatures` + `ext_cpp`, `CSharpFeatures` + `ext_csharp`, `GoFeatures` + `ext_go`, `JavaFeatures` + `ext_java`
[modules]: `any_pb` `api_pb` `c_sharp_features_pb` `cpp_features_pb` `descriptor_pb` `duration_pb` `empty_pb` `field_mask_pb` `go_features_pb` `java_features_pb` `plugin_pb` `source_context_pb` `struct_pb` `timestamp_pb` `type_pb` `wrappers_pb`

[PUBLIC_TYPE_SCOPE]: plugin framework (`protobuf.plugin`)

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `Schema` | protocol      | `files_to_generate` `all_files` `options`; `generate_file` opens one output  |
|  [02]   | `File`   | protocol      | `print` `doc` `scope` `type_checking` emitters beside `ident` and `preamble` |
|  [03]   | `Ident`  | value         | `name` `module` `type_only` import-aware identifier for a generated symbol   |
|  [04]   | `Module` | value         | `path` of one generated module; mints the idents and submodules under it     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: message codecs
- JSON emitters carry: `registry`, `always_emit_implicit`, `print_enums_as_ints`, `use_proto_field_name`; JSON readers carry: `ignore_unknown_fields`, `registry`.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------ | :------- | :------------------------------------------------ |
|  [01]   | `Message(**fields)`                                           | ctor     | keyword construction; a oneof takes `Oneof(f, v)` |
|  [02]   | `Message.to_binary(*, write_unknown_fields) -> bytes`         | instance | binary wire encode                                |
|  [03]   | `Message.from_binary(data, *, ignore_unknown_fields) -> Self` | factory  | binary wire decode into a NEW message             |
|  [04]   | `Message.to_json(*, ...) -> str`                              | instance | ProtoJSON encode                                  |
|  [05]   | `Message.from_json(json, *, ...) -> Self`                     | factory  | ProtoJSON decode into a NEW message               |
|  [06]   | `Message.has_field(key, /)` / `Message.clear_field(key, /)`   | instance | presence read and reset on one field              |
|  [07]   | `Message.desc() -> DescMessage`                               | factory  | the class's descriptor                            |
|  [08]   | `message[key]` / `message[key] = value` / `del message[key]`  | operator | descriptor-, unknown-, or extension-keyed slot    |
|  [09]   | `key in message` / `iter(message)`                            | operator | presence test; iteration over set `DescField`     |
|  [10]   | `merge_from(target, source, *, ignore_unknown_fields)`        | static   | in-place merge of one message into another        |
|  [11]   | `merge_from_binary(message, data, *, ignore_unknown_fields)`  | static   | in-place binary merge                             |
|  [12]   | `merge_from_json(message, json, *, ...)`                      | static   | in-place ProtoJSON merge                          |
|  [13]   | `message_to_json_value(message, /, *, ...) -> JsonValue`      | static   | ProtoJSON as Python values, no string round-trip  |
|  [14]   | `message_from_json_value(message_type, data, *, ...)`         | static   | Python JSON values into a NEW message             |
|  [15]   | `enum_is_unknown(value, /) -> bool`                           | static   | detect a pseudo-member an open enum minted        |
|  [16]   | `Enum.desc() -> DescEnum`                                     | factory  | the enum's descriptor                             |
|  [17]   | `Extension.desc() -> DescExtension`                           | instance | the extension handle's descriptor                 |
|  [18]   | `copy.replace(message, **fields)` / `__replace__`             | instance | NEW message with the named slots swapped          |
|  [19]   | `copy.copy` / `copy.deepcopy` / `pickle.dumps(message)`       | instance | value copies; pickling rides `to_binary()` bytes  |

- `message[key]`: `key` takes a `DescField`, a `DescUnknownField`, or an `Extension` — a plain field-name string reaches the slot as a bare attribute instead.

[ENTRYPOINT_SCOPE]: descriptors and registry

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------------------ | :------- | :----------------------------------------------------- |
|  [01]   | `<module>_pb.desc() -> DescFile`                                    | static   | the generated module's file descriptor                 |
|  [02]   | `Registry(*items)` / `Registry.add(*items)`                         | ctor     | seat a file, descriptor, class, extension, or registry |
|  [03]   | `Registry.message(type_name)` / `.enum` / `.service` / `.extension` | instance | `None`-on-miss lookup by type name                     |
|  [04]   | `Registry.file(path) -> DescFile`                                   | instance | lookup by proto path; files seat through `DescFile`    |
|  [05]   | `Registry.extension_for(type_info, number)`                         | instance | extension by extendee and field number                 |
|  [06]   | `iter(registry)`                                                    | operator | every seated file, message, enum, extension, service   |
|  [07]   | `FileDescriptorSet.to_registry() -> Registry`                       | instance | registry from a compiled descriptor set                |
|  [08]   | `minimum_supported_edition` / `maximum_supported_edition`           | static   | module `Edition` constants bounding the runtime's span |

- `Registry.extension_for`: `type_info` takes a `DescMessage`, a `Message` instance, or a type name — a message CLASS falls past the match and raises `UnboundLocalError`.
- `FileDescriptorSet.to_registry`: every dependency precedes its dependent in `file`, else the build raises `ValueError` naming the unresolved import.

[ENTRYPOINT_SCOPE]: well-known conversions

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `Any.pack(message)`                                            | factory  | typed envelope over any message                |
|  [02]   | `Any.unpack(type_info) -> T \| None`                           | instance | `None` on type mismatch                        |
|  [03]   | `Any.is_type(type_info) -> bool`                               | instance | discriminate without decoding the payload      |
|  [04]   | `Timestamp.now()` / `Timestamp.from_datetime(dt, /)`           | factory  | instant from the clock or a `datetime`         |
|  [05]   | `Timestamp.from_nanos(nanos, /)` / `.from_seconds(seconds, /)` | factory  | instant from numbers                           |
|  [06]   | `Timestamp.to_datetime()` / `.to_nanos()` / `.to_seconds()`    | instance | instant projection; `to_datetime` rides UTC    |
|  [07]   | `Duration.from_timedelta(td, /)` / `.from_nanos(nanos, /)`     | factory  | span construction; `.from_seconds(seconds, /)` |
|  [08]   | `Duration.to_timedelta()` / `.to_nanos()` / `.to_seconds()`    | instance | span projection                                |
|  [09]   | `Struct.from_python(data)` / `Struct.to_python()`              | factory  | JSON-shaped body from and to Python values     |
|  [10]   | `Value.from_python(value)` / `ListValue.from_python(values)`   | factory  | scalar and list members; `to_python()` inverts |

- `Value.to_python`: every proto number lands as `float`, so an int crossing a `Struct` body returns `1.0` and an exact-integer slot rides its own scalar field.

[ENTRYPOINT_SCOPE]: plugin and generated-module boot
- `plugin.run` carries: `minimum_edition`, `maximum_edition`.

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `plugin.run(name, version, options_or_generate, generate, /, *, ...)` | static   | protoc plugin main; options parse to a dataclass |
|  [02]   | `plugin.get_comments(desc)`                                           | static   | source-info comments for one descriptor          |
|  [03]   | `plugin.get_package_comments(file)` / `get_syntax_comments(file)`     | static   | file-level comments                              |
|  [04]   | `Schema.generate_file(path_or_desc, suffix, /) -> File`               | instance | open one output file on the generator response   |
|  [05]   | `File.print(*args)` / `File.preamble(desc)`                           | instance | emit one line; emit the generated-file header    |
|  [06]   | `File.doc(*args)` / `File.scope(*args)` / `File.type_checking()`      | instance | docstring, indented block, `TYPE_CHECKING` guard |
|  [07]   | `File.ident(name, *, type_only) -> Ident`                             | instance | identifier local to the file under generation    |
|  [08]   | `Ident.for_desc(desc, *, type_only, escape_module_with_hash)`         | factory  | import-aware identifier for a generated symbol   |
|  [09]   | `Module.for_desc(desc, suffix, *, escape_with_hash)`                  | factory  | module path for a descriptor's generated file    |
|  [10]   | `Module(path)` / `.ident(name, *, type_only)` / `.module(name)`       | ctor     | module path and the symbols it exports           |
|  [11]   | `_codegen.file_desc(proto_bytes, dependencies, stubs) -> DescFile`    | static   | boots a generated module's `_DESC`               |
|  [12]   | `_codegen.boot(proto, stubs) -> DescFile`                             | static   | boots the `descriptor_pb` bootstrap module       |
|  [13]   | `_codegen.unset(default) -> Any`                                      | static   | field-default sentinel the bootstrap declares    |

- `_codegen` is the generator's private contract: a generated module imports `file_desc` alone, the `descriptor_pb` bootstrap imports `_codegen.Message` beside `boot` and `unset`, and a hand-written module imports none of them.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- every generated module `<name>_pb.py` sits beside its proto path, imports cross-file siblings relatively, reaches well-known types through `protobuf.wkt`, and closes on `_DESC = file_desc(...)` under a `desc()` reader; `protoc-gen-py` carries one option, `init_files` (default true), and `init_files=false` leaves every generated directory a PEP 420 namespace package.
- `Message` subclasses are `__slots__` records: fields are attributes, `Message[Literal[...]]` types `has_field` / `clear_field` keys at the checker, an undeclared name raises `AttributeError` on assignment and `TypeError` in the constructor, equality is structural, and instances are unhashable.
- `Oneof` seats one `(field, value)` pair on the oneof attribute with `field` spelling the local field name, `None` spells unset, and an unnamed arm raises `ValueError` at construction.
- `has_field` reads implicit presence as value-differs-from-default and explicit presence as the set bit, so an explicit-presence field assigned `None` reads absent; an `optional` scalar constructs on `T | None`, reads its proto zero when unset under the native store, and a message slot reads `None`.
- construction and attribute assignment validate NOTHING — `to_binary` and `to_json` run the check and raise `TypeError` on a wrong-typed slot, `OverflowError` on a scalar outside its wire range, `ValueError` on a malformed oneof or value — so a decode-only consumer never fires it and an egress fence names all three.
- the value lifecycle is the message's own: `__copy__`/`__deepcopy__` copy by value, `__replace__` serves `copy.replace`, and `__getstate__`/`__setstate__` pickle as the `to_binary()` bytes, so every `cloudpickle` crossing of a message pays one binary round trip.
- `_binary_reader.DEPTH_LIMIT = 100` bounds SKIPPED-field recursion alone; declared nesting rides CPython recursion, `from_binary` carries no byte bound, and the body ceiling lives on the Connect mount (`read_max_bytes`) — a non-Connect decode path sets its own byte ceiling ahead of `from_binary`.
- `DescEnum.open` decides an unknown number's fate: an open enum mints the pseudo-member `enum_is_unknown` detects, and a closed one raises `ValueError`.
- `from_binary` / `from_json` mint a NEW message and `merge_from_binary` / `merge_from_json` overlay in place; unknown wire fields survive a binary round-trip under `write_unknown_fields`, and `ignore_unknown_fields=True` drops them at decode instead of carrying them.
- `to_binary` emits known fields in field-number order with map entries sorted by key and unknown residue trailing, while `to_json` preserves map insertion order — a digest freezes the decoded value's field-ordered projection, never either codec's output.
- `Registry` seats a whole file through its `DescFile` (`<module>_pb.desc()` or `FileDescriptorSet.to_registry()`); seating a class alone registers the message and leaves `Registry.file` empty, and `Any` / JSON `@type` resolution takes the registry on the call, an unseated type name raising `ValueError`.
- every decode, JSON, and resolution refusal raises `ValueError`, so an admission boundary catches that one type and the package declares no error hierarchy of its own.

[STACKING]:
- `connectrpc`(`.api/connectrpc.md`): generated `Message` classes are the `MethodInfo.input` / `output` types, `codec.proto_binary_codec()` / `codec.proto_json_codec(registry)` drive `to_binary` / `from_binary` / `to_json` / `from_json` beneath the protocol, and `ErrorDetail.value(registry)` unpacks a `ConnectError.details` member through a `Registry` seated from the generated `desc()` files.
- `msgspec`(`.api/msgspec.md`): `message_to_json_value` hands a ProtoJSON mapping to `msgspec.convert` at a boundary admitting a domain `Struct` from the wire, `to_builtins` inverts it into `message_from_json_value`, and `wkt.Struct.from_python` / `to_python` carry the dynamic body a struct holds as opaque JSON.
- `anyio`(`.api/anyio.md`): codec calls are CPU-bound and synchronous, so a bulk `from_binary` or `to_binary` over a large frame rides `to_thread.run_sync` off the event loop.
- `cloudpickle`(`.api/cloudpickle.md`): a message crossing the worker seam pickles as its `to_binary()` bytes through `__getstate__`, so a kernel argument or result that is a generated class costs one encode and one decode per crossing and never a deep copy of the slot store.
- `protobuf`(`.api/protobuf.md`): the two runtimes meet on descriptor bytes alone — a `google.protobuf` `FileDescriptorSet.SerializeToString()` feeds `wkt.FileDescriptorSet.from_binary(...).to_registry()`, handing the foreign Substrait and ONNX schemas a `DescMessage` view on the estate wire rail.
- within the branch, the generated `contracts` bindings are the sole producer of `Message` subclasses; a runtime-resolved schema builds its `Registry` from a `FileDescriptorSet` and reads fields through `DescMessage` and `message[desc_field]`, never through a hand-built class.

[LOCAL_ADMISSION]:
- `protobuf-py` and `protoc-gen-py` pin as one pair in the manifest, and the `_pb.py` tree regenerates on every runtime bump.
- fences call `Message.from_binary` / `to_binary` / `from_json` / `to_json` on the generated class, and the `ValueError` a parse failure raises maps to a domain fault at the admission boundary, never past it.

[RAIL_LAW]:
- Package: `protobuf-py`
- Owns: typed generated message classes, binary and ProtoJSON codecs, file descriptors and the `Registry`, well-known type conversions, and the protoc plugin framework
- Accept: `_pb.py` classes from the generated `contracts` package, `Registry(<module>_pb.desc(), ...)` for `Any` and JSON resolution, `wkt` mixins at the time and dynamic-body boundaries, `protobuf.plugin.run` for an estate generator
- Reject: hand-rolled `Message` subclasses, `google.protobuf` imports on the wire rail, digests over `to_binary()` bytes, `_codegen` members outside generated modules
