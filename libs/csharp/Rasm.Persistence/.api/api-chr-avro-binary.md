# [RASM_PERSISTENCE_API_CHR_AVRO_BINARY]

`Chr.Avro.Binary` is the Avro binary-encoding codec leg over the `Chr.Avro` schema model (`api-chr-avro`): the `BinarySerializerBuilder`/`BinaryDeserializerBuilder` facades that compile a `Schema` into a `BinarySerializer<T>`/`BinaryDeserializer<T>` delegate via a LINQ `Expression` tree, the zero-copy `BinaryReader` `ref struct` over `ReadOnlySpan<byte>`, the `BinaryWriter` over a `Stream`, and the per-schema-shape `Binary*BuilderCase` set specializing the abstract codec framework. The compiled-delegate model is the load-bearing capability — `BuildDelegate<T>(schema)` produces a reflection-free, JIT-compiled codec that is built once per `(type, schema)` pair and invoked on every message at near-hand-written speed. This leg owns the raw Avro binary wire (no framing, no schema id); the registry-prefixed wire-format-1 envelope is `Chr.Avro.Confluent` (`api-chr-avro-confluent`), which composes this builder under the hood.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Chr.Avro.Binary`
- package: `Chr.Avro.Binary`
- version: `10.13.1`
- license: MIT
- assembly: `Chr.Avro.Binary`
- namespace: `Chr.Avro.Serialization`
- bound asset: `lib/net6.0` (consumer-bound; package multi-targets `net6.0`/`netstandard2.0` — there is NO `net8.0`/`net10.0` asset, so the `net10.0` consumer binds the `net6.0` surface)
- dependencies: `Chr.Avro` `10.13.1` (the schema model + abstract `*BuilderCase` framework this leg specializes); pure-managed AnyCPU, no native asset
- rail: avro-codec

## [02]-[PUBLIC_TYPES]

[CODEC_FACADE_TYPES]: builder facades and compiled delegates
- rail: avro-codec

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]        | [CAPABILITY]                                       |
| :-----: | :----------------------------- | :-------------------- | :------------------------------------------------- |
|  [01]   | `BinarySerializerBuilder`      | serializer facade     | `Schema` → `BinarySerializer<T>` (compiled)        |
|  [02]   | `BinaryDeserializerBuilder`    | deserializer facade   | `Schema` → `BinaryDeserializer<T>` (compiled)      |
|  [03]   | `IBinarySerializerBuilder`     | facade contract       | `BuildDelegateExpression<T>(schema)` seam          |
|  [04]   | `IBinaryDeserializerBuilder`   | facade contract       | `BuildDelegateExpression<T>(schema)` seam          |
|  [05]   | `BinarySerializer<T>`          | compiled delegate     | `delegate void (T value, BinaryWriter writer)`     |
|  [06]   | `BinaryDeserializer<T>`        | compiled delegate     | `delegate T (ref BinaryReader reader)`             |

[CODEC_PRIMITIVE_TYPES]: the wire reader and writer
- rail: avro-codec

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE] | [CAPABILITY]                                              |
| :-----: | :-------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `BinaryReader`  | wire decoder   | `ref struct` over `ReadOnlySpan<byte>` — zero-copy decode |
|  [02]   | `BinaryWriter`  | wire encoder   | `sealed class : IDisposable` over a `Stream`             |

`BinaryReader` (`ref struct`, stack-only, single-pass): `readonly long Index { get; }` (cursor); `ReadBoolean()`, `ReadInteger()` → `long` (zig-zag varint), `ReadSingle()`/`ReadDouble()`, `ReadString()`, `ReadBytes()` → `byte[]`, `ReadBytesSpan()` → `ReadOnlySpan<byte>` (zero-copy), `ReadFixed(int)` → `byte[]`, `ReadFixedSpan(int)` → `ReadOnlySpan<byte>`.
`BinaryWriter` (`Stream`-backed): `WriteBoolean(bool)`, `WriteInteger(int)`/`WriteInteger(long)` (zig-zag varint), `WriteSingle(float)`/`WriteDouble(double)`, `WriteString(string)`, `WriteBytes(byte[])`/`WriteBytes(ReadOnlySpan<byte>)`, `WriteFixed(byte[])`/`WriteFixed(ReadOnlySpan<byte>)`, `Dispose()`.

[CODEC_CONTEXT_TYPES]: build context and case framework
- rail: avro-codec

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]   | [CAPABILITY]                                       |
| :-----: | :-------------------------------- | :--------------- | :------------------------------------------------- |
|  [01]   | `BinarySerializerBuilderContext`  | build context    | per-build expression cache (recursive schemas)     |
|  [02]   | `BinaryDeserializerBuilderContext` | build context   | per-build expression cache                          |
|  [03]   | `BinarySerializerBuilderCaseResult` | case result    | matched write-expression or unmatched signal        |
|  [04]   | `BinaryDeserializerBuilderCaseResult` | case result  | matched read-expression or unmatched signal         |
|  [05]   | `IBinarySerializerBuilderCase` / `IBinaryDeserializerBuilderCase` | case contract | one schema-shape → binary read/write rule |

Per-schema-shape codec cases (each `: <Shape>SerializerBuilderCase, IBinarySerializerBuilderCase` / the deserializer mirror), the 19 cases the facade composes by default: `Array`, `Boolean`, `Bytes`, `Date`, `Decimal`, `Double`, `Duration`, `Enum`, `Fixed`, `Float`, `Int`, `Long`, `Map`, `Null`, `Record`, `String`, `Time`, `Timestamp`, `Union`. Each derives from the matching abstract case in `Chr.Avro.Serialization` (`api-chr-avro`) and supplies the leaf binary read/write expression.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: codec compilation
- rail: avro-codec

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------------------- | :----------- | :------------------------------------------------- |
|  [01]   | `new BinarySerializerBuilder()`                                            | ctor         | builds the default 19-case serializer set          |
|  [02]   | `new BinarySerializerBuilder(IEnumerable<Func<IBinarySerializerBuilder, IBinarySerializerBuilderCase>>)` | ctor | builds from a custom case list |
|  [03]   | `BinarySerializerBuilder.BuildDelegate<T>(Schema, BinarySerializerBuilderContext?)` → `BinarySerializer<T>` | build call | compiles a write delegate (`.Compile()`) |
|  [04]   | `BinarySerializerBuilder.BuildDelegateExpression<T>(Schema, ...)` → `Expression<BinarySerializer<T>>` | build call | the un-compiled expression tree (inline into a larger pipeline) |
|  [05]   | `new BinaryDeserializerBuilder()`                                          | ctor         | builds the default 19-case deserializer set        |
|  [06]   | `BinaryDeserializerBuilder.BuildDelegate<T>(Schema, BinaryDeserializerBuilderContext?)` → `BinaryDeserializer<T>` | build call | compiles a read delegate |
|  [07]   | `BinaryDeserializerBuilder.BuildDelegateExpression<T>(Schema, ...)` → `Expression<BinaryDeserializer<T>>` | build call | the un-compiled read expression tree |

[ENTRYPOINT_SCOPE]: wire read and write
- rail: avro-codec

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE] | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------- | :----------- | :------------------------------------------------- |
|  [01]   | `serializer(value, new BinaryWriter(stream))`   | delegate call | invokes the compiled write delegate against a stream |
|  [02]   | `var reader = new BinaryReader(span); var v = deserializer(ref reader)` | delegate call | zero-copy read from a span |
|  [03]   | `reader.Index`                                  | cursor        | bytes consumed (multi-item framing offset)         |
|  [04]   | `BinaryReader.ReadBytesSpan()` / `ReadFixedSpan(int)` | zero-copy read | element bytes without an allocating copy      |
|  [05]   | `BinaryWriter.Dispose()`                        | drain         | flushes and releases the stream-backed writer      |

## [04]-[IMPLEMENTATION_LAW]

[AVRO_CODEC_TOPOLOGY]:
- namespace: `Chr.Avro.Serialization` (shared with the core's abstract framework) — the `Binary*` types are the concrete specialization: `BinarySerializerBuilder : ExpressionBuilder, IBinarySerializerBuilder, ISerializerBuilder<BinarySerializerBuilderContext>` and the deserializer mirror `: ExpressionBuilder, IBinaryDeserializerBuilder, IDeserializerBuilder<BinaryDeserializerBuilderContext>` (`api-chr-avro` `[SERIALIZATION_FRAMEWORK_TYPES]`), so the binary leg IS the `ISerializerBuilder<TContext>` framework bound to `BinaryWriter`/`BinaryReader` leaves. Both builders also expose a `new BinarySerializerBuilder(BindingFlags memberVisibility)` ctor alongside the custom-case-list ctor
- `BuildDelegate<T>(schema)` assembles a LINQ `Expression` tree by dispatching each `Schema` node to its matching `IBinarySerializerBuilderCase`, then `Compile()`s the tree into a `BinarySerializer<T>`/`BinaryDeserializer<T>` — the result is a JIT-compiled, reflection-free codec; building is the cost, invoking is cheap, so the delegate is cached per `(type, schema)` pair
- `BuildDelegateExpression<T>` exposes the un-compiled `Expression<…>` so a larger pipeline can inline the Avro codec into a single compiled expression rather than paying a delegate-call boundary
- `BinaryReader` is a `ref struct` over `ReadOnlySpan<byte>`: it cannot escape to the heap or cross an `async` boundary, which is what makes the read path zero-allocation; `Index` exposes the consumed-byte cursor for multi-record framing. The deserializer delegate takes it `by ref` so the cursor advances in the caller's frame
- `BinaryWriter` wraps a `Stream` (not a span) and is `IDisposable`; the serializer delegate takes it by value and writes through to the stream — `Dispose()` flushes
- integer encoding is Avro zig-zag varint (`ReadInteger`/`WriteInteger` operate on `long`); `Fixed` reads/writes a known-length blob with no length prefix, `Bytes`/`String` carry a varint length prefix

[INTEGRATION_LAW]:
- The codec composes the schema model directly: `var schema = new SchemaBuilder().BuildSchema<T>()` (`api-chr-avro`) then `var write = new BinarySerializerBuilder().BuildDelegate<T>(schema)` is the canonical two-step — the schema is built once, the delegate compiled once, both cached, and only the delegate invocation is on the per-message hot path.
- This leg is the engine under the OFF-Kafka registry serde: `SchemaRegistrySerializerBuilder` (`api-chr-avro-confluent`) holds an `IBinarySerializerBuilder` and calls `BuildDelegateExpression<T>(schema)` to produce the body codec, then frames it with the wire-format-1 magic byte + schema id — so that serde IS this binary codec plus a registry-id prefix, never a separate encoder. This is precisely the carve that keeps the Chr.Avro stack distinct from the canonical Kafka serde: `Confluent.SchemaRegistry.Serdes.Avro` (`api-schemaregistry-serdes-avro`) owns the Kafka slot but frames its body with the `Apache.Avro` REFERENCE codec over `GenericRecord`/`ISpecificRecord`, NOT this expression-compiled delegate — so this leg's compiled-codec engine is reached only through the off-Kafka `Chr.Avro.Confluent` path, never on the Kafka backbone the first-party serde owns.
- Reader/writer schema resolution stacks at build time: passing a reader `Schema` that differs from the writer `Schema` (added/removed fields with `RecordField.Default`, `NamedSchema.Aliases`) makes the compiled deserializer back-fill defaults and remap aliased fields — Avro's evolution guarantee, resolved into the compiled delegate rather than at read time per record.
- Multi-record framing reads through one `BinaryReader` over a buffer: after each `deserializer(ref reader)` the `reader.Index` advances, so a batched Avro body (e.g. an object-store blob, `api-objectstore`) decodes as a loop over one span without re-allocating the reader — the `ref struct` keeps it stack-resident across the loop.

[LOCAL_ADMISSION]:
- The compiled `BinarySerializer<T>`/`BinaryDeserializer<T>` delegates are profile-cached `Element/codec` internals, not public Persistence vocabulary; the schema and the cached delegate are profile data.
- The codec is built from a `SchemaBuilder`-derived `Schema`, never against a hand-authored schema in Persistence code.
- `BinaryReader` must stay stack-resident — a deserializer that captures the reader into a closure or crosses an `async` await is a rejected form; decode the whole record synchronously before yielding.
- Raw Avro binary carries no schema id; a body that must self-identify its schema routes through the registry serde (`api-chr-avro-confluent`), never a hand-prefixed magic byte here.

[RAIL_LAW]:
- Package: `Chr.Avro.Binary`
- Owns: the Avro binary-encoding codec — schema-to-delegate compilation, zero-copy span decode, stream encode
- Accept: `BuildDelegate<T>` / `BuildDelegateExpression<T>` compilation, cached delegate invocation, schema-resolution at build time
- Reject: per-message recompilation, heap-escaping `BinaryReader`, hand-rolled Avro varint framing
