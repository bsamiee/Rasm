# [RASM_PERSISTENCE_API_CHR_AVRO]

`Chr.Avro` owns the abstract Avro schema-model core: the `Schema` algebra, the `LogicalType` decorators, the reflection-driven `SchemaBuilder` deriving a schema from a CLR `Type`, and the open expression-builder codec framework the binary and registry legs specialize. It ships no encoder and no JSON codec — `Chr.Avro.Binary` carries the binary codec and the first-party Confluent serde the registry frame — so schema derivation is its whole governance, and it supplies the evolution-safe interchange rail the `Arrow`/`Parquet`/`MessagePack`/`CBOR` set lacks.

## [01]-[PUBLIC_TYPES]

[SCHEMA_MODEL_TYPES]: the `Schema` algebra (namespace `Chr.Avro.Abstract`)

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]      | [CAPABILITY]                                                 |
| :-----: | :---------------- | :----------------- | :----------------------------------------------------------- |
|  [01]   | `Schema`          | abstract root      | base node; carries an optional `LogicalType`                 |
|  [02]   | `PrimitiveSchema` | abstract primitive | base of the scalar cases                                     |
|  [03]   | `ComplexSchema`   | abstract complex   | base of array/map/named cases                                |
|  [04]   | `NamedSchema`     | abstract named     | `Name`/`Namespace`/`FullName`/`Aliases` carrier              |
|  [05]   | `RecordSchema`    | record case        | `ICollection<RecordField> Fields` + `Documentation`          |
|  [06]   | `EnumSchema`      | enum case          | `ICollection<string> Symbols` + `string? Default`            |
|  [07]   | `FixedSchema`     | fixed case         | `int Size` fixed-length byte blob                            |
|  [08]   | `ArraySchema`     | array case         | `Schema Item` element type                                   |
|  [09]   | `MapSchema`       | map case           | `Schema Value` value type (string keys)                      |
|  [10]   | `UnionSchema`     | union case         | `ICollection<Schema> Schemas` ordered branches               |
|  [11]   | `RecordField`     | field declaration  | `Name`/`Schema Type`/`DefaultValue? Default`/`Documentation` |

[PRIMITIVE_CASES] (each `: PrimitiveSchema`): `BooleanSchema` `IntSchema` `LongSchema` `FloatSchema` `DoubleSchema` `BytesSchema` `StringSchema` `NullSchema`.

`NamedSchema` (base of `RecordSchema`/`EnumSchema`/`FixedSchema`) derives `FullName` from `Name`+`Namespace` and carries `Aliases` for evolution aliasing; `EnumSchema.Default` and the `Documentation` fields round-trip the registry JSON.

`DefaultValue` (`abstract T? ToObject<T>()`) and `ObjectDefaultValue<TValue>` carry a record field's typed default for read-time schema-resolution back-fill.

Construction faults `InvalidSchemaException`/`InvalidNameException`/`InvalidSymbolException` (`Chr.Avro.Abstract`) and `UnknownSchemaException` (`Chr.Avro.Representation`, an unrecognized JSON node on read) catch to a typed Persistence failure at the build/parse boundary, never escaping inward.

[LOGICAL_TYPE_TYPES]: `LogicalType` decorators (namespace `Chr.Avro.Abstract`)

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]      | [CAPABILITY]                                       |
| :-----: | :-------------------------------- | :----------------- | :------------------------------------------------- |
|  [01]   | `LogicalType`                     | abstract decorator | annotates a base schema with semantic meaning      |
|  [02]   | `DecimalLogicalType`              | decimal            | `int Precision` + `int Scale` over `bytes`/`fixed` |
|  [03]   | `DateLogicalType`                 | date               | days-since-epoch over `int`                        |
|  [04]   | `UuidLogicalType`                 | uuid               | RFC 4122 over `string`                             |
|  [05]   | `DurationLogicalType`             | duration           | months/days/millis over `fixed(12)`                |
|  [06]   | `TimeLogicalType`                 | abstract time      | base of the time-of-day precisions                 |
|  [07]   | `MillisecondTimeLogicalType`      | time (ms)          | time-of-day, `int` millis                          |
|  [08]   | `MicrosecondTimeLogicalType`      | time (µs)          | time-of-day, `long` micros                         |
|  [09]   | `TimestampLogicalType`            | abstract timestamp | base of the timestamp precisions                   |
|  [10]   | `MillisecondTimestampLogicalType` | timestamp (ms)     | instant, `long` millis-since-epoch                 |
|  [11]   | `MicrosecondTimestampLogicalType` | timestamp (µs)     | instant, `long` micros-since-epoch                 |
|  [12]   | `NanosecondTimestampLogicalType`  | timestamp (ns)     | instant, `long` nanos-since-epoch                  |

[BUILDER_TYPES]: schema construction and the case framework

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]      | [CAPABILITY]                                                               |
| :-----: | :------------------------------ | :----------------- | :------------------------------------------------------------------------- |
|  [01]   | `ISchemaBuilder`                | builder contract   | `BuildSchema<T>(SchemaBuilderContext?)`                                    |
|  [02]   | `SchemaBuilder`                 | reflection builder | CLR `Type` → `Schema` over an ordered case list                            |
|  [03]   | `ISchemaBuilderCase`            | case contract      | one type-shape → schema rule                                               |
|  [04]   | `SchemaBuilderCase`             | abstract case      | base of the per-shape builder cases                                        |
|  [05]   | `SchemaBuilderCaseResult`       | case result        | matched schema or unmatched signal                                         |
|  [06]   | `SchemaBuilderContext`          | build context      | shared schema cache across a recursive build                               |
|  [07]   | `EnumBehavior`                  | build policy       | `Symbolic` (named) / `Integral` (ordinal `int`) / `Nominal` (string) enums |
|  [08]   | `TemporalBehavior`              | build policy       | `Iso8601` / `EpochMicroseconds` / `EpochMilliseconds` / `EpochNanoseconds` |
|  [09]   | `NullableReferenceTypeBehavior` | build policy       | `None` / `All` / `Annotated` union-nullability                             |

[SHAPE_CASES] (each `: SchemaBuilderCase, ISchemaBuilderCase`): `Boolean` `Int` `Long` `Float` `Double` `Bytes` `String` `Uri` `Date` `Time` `Timestamp` `Decimal` `Duration` `Enum` `Array` `Map` `Record` `Union` `Uuid`.

[SERIALIZATION_FRAMEWORK_TYPES]: the open codec framework (namespace `Chr.Avro.Serialization`)

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]         | [CAPABILITY]                                |
| :-----: | :-------------------------------------------------- | :-------------------- | :------------------------------------------ |
|  [01]   | `ISerializerBuilder<TContext>`                      | serializer contract   | encoding-specific serializer-builder seam   |
|  [02]   | `IDeserializerBuilder<TContext>`                    | deserializer contract | encoding-specific deserializer-builder seam |
|  [03]   | `ISerializerBuilderCase<TContext, TResult>`         | case contract         | one schema-shape → write-expression rule    |
|  [04]   | `IDeserializerBuilderCase<TContext, TResult>`       | case contract         | one schema-shape → read-expression rule     |
|  [05]   | `ExpressionBuilder`                                 | expression base       | shared `Expression`-tree emission helpers   |
|  [06]   | `SerializerBuilderCase` / `DeserializerBuilderCase` | abstract case         | per-schema-shape codec case bases           |

[CODEC_SHAPE_CASES] the binary/JSON legs specialize (each `: SerializerBuilderCase`/`: DeserializerBuilderCase`): `Array` `Boolean` `Bytes` `Date` `Decimal` `Double` `Duration` `Enum` `Fixed` `Float` `Int` `Long` `Map` `Null` `Record` `String` `Time` `Timestamp` `Union`.

[REPRESENTATION_TYPES]: the schema read/write seam (namespace `Chr.Avro.Representation`)

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]  | [CAPABILITY]                                            |
| :-----: | :-------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `ISchemaReader<TContext>`               | read contract  | `Schema Read(Stream, TContext?)`                        |
|  [02]   | `ISchemaWriter<TContext>`               | write contract | `void Write(Schema, Stream, bool canonical, TContext?)` |
|  [03]   | `SchemaReaderCase` / `SchemaWriterCase` | abstract case  | per-schema-shape representation case bases              |

`JsonSchemaReader`/`JsonSchemaWriter` (and `IJsonSchemaReader`/`IJsonSchemaWriter`), the concrete implementations of these contracts, ship in the transitive `Chr.Avro.Json`, not this assembly.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: schema construction from a CLR type
- `SchemaBuilder` ctors take the `(memberVisibility, enumBehavior, nullableReferenceTypeBehavior, temporalBehavior)` policy quartet (shared with `CreateDefaultCaseBuilders`) or a custom `IEnumerable<Func<ISchemaBuilder, ISchemaBuilderCase>>` case list.

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------ | :------- | :--------------------------------------------- |
|  [01]   | `new SchemaBuilder(memberVisibility, …)`                            | ctor     | default ordered case set under explicit policy |
|  [02]   | `new SchemaBuilder(IEnumerable<Func<…>>)`                           | ctor     | builds from a fully custom case list           |
|  [03]   | `SchemaBuilder.BuildSchema<T>(SchemaBuilderContext?)` → `Schema`    | instance | derives an Avro schema from `typeof(T)`        |
|  [04]   | `SchemaBuilder.BuildSchema(Type, SchemaBuilderContext?)` → `Schema` | instance | derives a schema from a runtime `Type`         |
|  [05]   | `SchemaBuilder.CreateDefaultCaseBuilders(…)`                        | factory  | the default case list for custom composition   |
|  [06]   | `new SchemaBuilderContext()`                                        | ctor     | the schema cache for one recursive build       |

[ENTRYPOINT_SCOPE]: schema model construction

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :-------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `new RecordSchema(string name, IEnumerable<RecordField>?)`      | ctor     | constructs a record with named fields      |
|  [02]   | `new RecordField(string name, Schema type)`                     | ctor     | one field; set `.Default`/`.Documentation` |
|  [03]   | `new EnumSchema(string name, IEnumerable<string>?)`             | ctor     | symbolic enum                              |
|  [04]   | `new UnionSchema(IEnumerable<Schema>?)`                         | ctor     | ordered union (e.g. `[null, T]` nullable)  |
|  [05]   | `new ArraySchema(Schema item)` / `new MapSchema(Schema value)`  | ctor     | array / string-keyed map                   |
|  [06]   | `new FixedSchema(string name, int size)`                        | ctor     | fixed-length byte blob                     |
|  [07]   | `schema.LogicalType = new DecimalLogicalType(precision, scale)` | property | attaches a logical type to a base schema   |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every codec is a LINQ `Expression` tree `Compile()`d once to a delegate from a `Schema` derived once from a CLR `Type`; no per-message reflection on the hot path.
- `SchemaBuilder.BuildSchema<T>` forwards to `BuildSchema(typeof(T))`, walking the type graph through the ordered `ISchemaBuilderCase` list where the first match yields a `SchemaBuilderCaseResult`; `SchemaBuilderContext` caches by type so recursive/cyclic records resolve once.
- Three build policies drive evolution: `EnumBehavior` picks `Symbolic`/`Integral`/`Nominal`, `NullableReferenceTypeBehavior.Annotated` reads C# nullable-reference annotations to emit `[null, T]` unions vs `All`/`None`, `TemporalBehavior` picks ISO-8601 vs epoch micro/milli/nanosecond logical timestamps.
- `SchemaBuilder`'s ctor defaults `nullableReferenceTypeBehavior` to `Annotated` while `CreateDefaultCaseBuilders` defaults it to `None`, so a factory-built custom case list loses annotation-aware nullability unless passed explicitly.
- `RecordField.Default` (typed `DefaultValue`) back-fills a reader-schema field absent from the writer schema — the evolution back-compat seam.

[STACKING]:
- Owning pages: `Element/codec` derives the Avro schema model and `api-chr-avro-binary` compiles the body codec. `Version/egress` may carry an independently supplied absolute `dataschema` URI, while `Confluent.SchemaRegistry.Serdes.Avro` owns registry subject/version and schema-id framing.
- `SchemaBuilder.BuildSchema<T>()` mints the `Schema` once per type and `BinarySerializerBuilder.BuildDelegate<T>(schema)` (`api-chr-avro-binary`) compiles the codec against it — never rebuilt per message; the derived schema's JSON representation is what the registry subject registers, and the Confluent serde owns the id prefix from there.
- Boundary against the first-party Confluent serde: this core's carve is the expression-tree-compiled CLR-schema-DERIVATION path; the canonical Kafka-wire Avro serde is `Confluent.SchemaRegistry.Serdes.Avro` (`api-schemaregistry-serdes-avro`) over the `Apache.Avro` codec on `GenericRecord`/`ISpecificRecord`, and the two arms never frame the same Kafka payload.
- Boundary against the schemaless codecs: `MessagePack` (`api-messagepack`) and `CBOR` (`api-cbor`) own self-describing schema-free blobs, Avro owns the schema-governed evolution-safe row whose field add/remove/rename rides `RecordField.Default` + `NamedSchema.Aliases`; the codec choice is body profile data.
- Logical-type mapping stacks onto the temporal/decimal owners at the edge: `TimestampLogicalType` → a `NodaTime`/`DateTimeOffset` instant and `DecimalLogicalType` → a `System.Decimal`, so the boundary adapter maps to the canonical internal type rather than leaking the Avro wrapper inward.

[LOCAL_ADMISSION]:
- Chr.Avro is the schema-model + codegen owner inside the Avro interchange profile, not public Persistence vocabulary; the `Schema`, logical types, and build policies are profile data.
- Schemas derive from the canonical record type via `SchemaBuilder`, never hand-authored as JSON text inside Persistence code.
- Schema evolution rides `RecordField.Default` + `NamedSchema.Aliases` on the model, never ad-hoc payload patching.
- `Chr.Avro.Binary` owns the encoder and `Chr.Avro.Json` the JSON codec; this core ships neither, so a catalog citing a binary `Encode`/`Decode` or a `JsonSchemaReader` against this assembly is wrong.
