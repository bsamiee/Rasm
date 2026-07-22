# [RASM_PERSISTENCE_API_CHR_AVRO_BINARY]

`Chr.Avro.Binary` compiles a `Schema` into a reflection-free binary Avro codec: `BinarySerializerBuilder`/`BinaryDeserializerBuilder` assemble a LINQ `Expression` tree and `Compile()` it into a `BinarySerializer<T>`/`BinaryDeserializer<T>` delegate built once per `(type, schema)` pair and invoked on every message. Reading rides the stack-only `BinaryReader` `ref struct` over a `ReadOnlySpan<byte>`, writing the `Stream`-backed `BinaryWriter`. This leg owns the raw Avro binary wire with no framing or schema id; the registry-prefixed envelope is `Chr.Avro.Confluent` (`api-chr-avro-confluent`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Chr.Avro.Binary`
- package: `Chr.Avro.Binary` (MIT)
- assembly: `Chr.Avro.Binary`
- namespace: `Chr.Avro.Serialization`
- bound asset: `lib/net6.0` (consumer binds; package multi-targets `net6.0`/`netstandard2.0`)
- depends: `Chr.Avro` (`api-chr-avro`) — the schema model and abstract `*BuilderCase` framework this leg specializes; pure-managed AnyCPU, no native asset
- rail: avro-codec

## [02]-[PUBLIC_TYPES]

[CODEC_FACADE_TYPES]: builder facades, contracts, and compiled delegates

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :--------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `BinarySerializerBuilder`    | class         | `Schema` → compiled `BinarySerializer<T>`   |
|  [02]   | `BinaryDeserializerBuilder`  | class         | `Schema` → compiled `BinaryDeserializer<T>` |
|  [03]   | `IBinarySerializerBuilder`   | interface     | `BuildDelegateExpression<T>(Schema)` seam   |
|  [04]   | `IBinaryDeserializerBuilder` | interface     | `BuildDelegateExpression<T>(Schema)` seam   |
|  [05]   | `BinarySerializer<T>`        | delegate      | `void (T, BinaryWriter)`                    |
|  [06]   | `BinaryDeserializer<T>`      | delegate      | `T (ref BinaryReader)`                      |

[CODEC_PRIMITIVE_TYPES]: the wire reader and writer

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :------------- | :------------ | :--------------------------------- |
|  [01]   | `BinaryReader` | struct        | zero-copy `ref struct` span decode |
|  [02]   | `BinaryWriter` | class         | disposable `Stream` encode         |

[READER_OPS]: `Index` `ReadBoolean` `ReadInteger` `ReadSingle` `ReadDouble` `ReadString` `ReadBytes` `ReadBytesSpan` `ReadFixed` `ReadFixedSpan`
[WRITER_OPS]: `WriteBoolean` `WriteInteger` `WriteSingle` `WriteDouble` `WriteString` `WriteBytes` `WriteFixed` `Dispose`

[CODEC_CONTEXT_TYPES]: build context and case framework

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :------------------------------------ | :------------ | :------------------------------------ |
|  [01]   | `BinarySerializerBuilderContext`      | class         | per-build expression cache            |
|  [02]   | `BinaryDeserializerBuilderContext`    | class         | per-build expression cache            |
|  [03]   | `BinarySerializerBuilderCaseResult`   | class         | matched write-expression or unmatched |
|  [04]   | `BinaryDeserializerBuilderCaseResult` | class         | matched read-expression or unmatched  |
|  [05]   | `IBinarySerializerBuilderCase`        | interface     | one schema-shape → write rule         |
|  [06]   | `IBinaryDeserializerBuilderCase`      | interface     | one schema-shape → read rule          |

Each `Binary<Shape>SerializerBuilderCase` (deserializer mirror) specializes the abstract `*BuilderCase` in `api-chr-avro` and supplies the leaf binary read/write expression; the default builders compose one case per schema shape.

[SCHEMA_SHAPE_CASES]: `Array` `Boolean` `Bytes` `Date` `Decimal` `Double` `Duration` `Enum` `Fixed` `Float` `Int` `Long` `Map` `Null` `Record` `String` `Time` `Timestamp` `Union`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: codec compilation and wire invocation

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `new BinarySerializerBuilder(BindingFlags?)`                   | ctor     | default serializer case set                    |
|  [02]   | `BinarySerializerBuilder.BuildDelegate<T>(Schema, Context?)`   | instance | compiled `BinarySerializer<T>` write delegate  |
|  [03]   | `BuildDelegateExpression<T>(Schema, Context?)`                 | instance | uncompiled `Expression<BinarySerializer<T>>`   |
|  [04]   | `new BinaryDeserializerBuilder(BindingFlags?)`                 | ctor     | default deserializer case set                  |
|  [05]   | `BinaryDeserializerBuilder.BuildDelegate<T>(Schema, Context?)` | instance | compiled `BinaryDeserializer<T>` read delegate |
|  [06]   | `BuildDelegateExpression<T>(Schema, Context?)`                 | instance | uncompiled `Expression<BinaryDeserializer<T>>` |

- custom cases: `new BinarySerializerBuilder(IEnumerable<Func<IBinarySerializerBuilder, IBinarySerializerBuilderCase>>)` (deserializer mirror) replaces the default case list.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Namespace `Chr.Avro.Serialization` is shared with the core's abstract framework; `BinarySerializerBuilder`/`BinaryDeserializerBuilder` ARE the `ISerializerBuilder<TContext>`/`IDeserializerBuilder<TContext>` framework (`api-chr-avro`) bound to `BinaryWriter`/`BinaryReader` leaves.
- `BuildDelegate<T>` dispatches each `Schema` node to its `IBinarySerializerBuilderCase`, assembles the `Expression` tree, and `Compile()`s it — building is the cost and invocation is cheap, so the delegate caches per `(type, schema)` pair.
- `BuildDelegateExpression<T>` exposes the un-compiled `Expression<…>` so a larger pipeline inlines the codec into one compiled expression rather than paying a delegate-call boundary.
- `BinaryReader` cannot escape to the heap or cross an `async` boundary — the read path is zero-allocation; the deserializer takes it by ref so `Index` advances the consumed-byte cursor in the caller's frame.
- `BinaryWriter` wraps a `Stream` by value and `Dispose()` flushes; integer encoding is Avro zig-zag varint over `long`, `Fixed` carries no length prefix, and `Bytes`/`String` carry a varint length prefix.

[STACKING]:
- `Chr.Avro`(`.api/api-chr-avro`): the codec compiles against the `Schema` that `SchemaBuilder.BuildSchema<T>()` derives; a reader `Schema` differing from the writer resolves at build time — `RecordField.Default` back-fills absent fields and `NamedSchema.Aliases` remaps renamed ones into the compiled delegate.
- `Chr.Avro.Confluent`(`.api/api-chr-avro-confluent`): `SchemaRegistrySerializerBuilder.SerializerBuilder` holds an `IBinarySerializerBuilder` and calls `BuildDelegateExpression<T>` for the body codec, then prefixes the wire-format-1 magic byte and schema id — the registry serde IS this codec under a registry-id frame.
- `Element/codec` profile: the compiled `BinarySerializer<T>`/`BinaryDeserializer<T>` are profile-cached internals; a batched Avro body loops `deserializer(ref reader)` over one span, `reader.Index` advancing across records with no reader re-allocation.

[LOCAL_ADMISSION]:
- `BinarySerializer<T>`/`BinaryDeserializer<T>` delegates and the source `Schema` are `Element/codec` profile data, never public Persistence vocabulary.
- `BinarySerializerBuilder` builds from a `SchemaBuilder`-derived `Schema`, never a hand-authored schema in Persistence code.
- `BinaryReader` stays stack-resident: a deserializer capturing it into a closure or crossing an `async` await is rejected, so decode the whole record synchronously before yielding.
- Raw Avro binary carries no schema id; a body that must self-identify routes through the registry serde (`api-chr-avro-confluent`), never a hand-prefixed magic byte here.

[RAIL_LAW]:
- Package: `Chr.Avro.Binary`
- Owns: the Avro binary codec — schema-to-delegate compilation, zero-copy span decode, stream encode
- Accept: `BuildDelegate<T>`/`BuildDelegateExpression<T>` compilation, cached delegate invocation, build-time schema resolution
- Reject: per-message recompilation, heap-escaping `BinaryReader`, hand-rolled Avro varint framing
