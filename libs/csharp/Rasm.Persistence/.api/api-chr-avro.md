# [RASM_PERSISTENCE_API_CHR_AVRO]

`Chr.Avro` is the abstract Avro schema-model core: the `Schema` algebra (`PrimitiveSchema`/`ComplexSchema`/`NamedSchema` and their concrete cases), the `LogicalType` decorators (decimal, date, time/timestamp at three precisions, uuid, duration), the reflection-driven `SchemaBuilder` that derives an Avro schema from a CLR `Type`, and the open `ISerializerBuilderCase`/`IDeserializerBuilderCase`/`ISchemaReaderCase`/`ISchemaWriterCase` expression-builder framework the binary (`api-chr-avro-binary`) and registry (`api-chr-avro-confluent`) legs specialize. The core owns the schema model and the codegen-via-`Expression`-tree machinery but ships NO encoder and NO JSON-text reader/writer — the binary codec is `Chr.Avro.Binary`, the JSON schema/value codec is the transitive `Chr.Avro.Json`, and the Schema-Registry serde is `Chr.Avro.Confluent`. This is the row-oriented, schema-evolving interchange model the `Arrow`/`Parquet`/`MessagePack`/`CBOR` codec set lacked: a producer builds `SchemaBuilder.BuildSchema<T>()` once, the binary leg compiles a `BinarySerializer<T>` delegate against it, and the registry leg governs subject-keyed schema evolution on the Kafka backbone (`api-kafka`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Chr.Avro`
- package: `Chr.Avro`
- version: `10.13.1`
- license: MIT
- assembly: `Chr.Avro`
- namespace: `Chr.Avro`, `Chr.Avro.Abstract`, `Chr.Avro.Representation`, `Chr.Avro.Serialization`, `Chr.Avro.Infrastructure`
- bound asset: `lib/net6.0` (consumer-bound; package multi-targets `net6.0`/`netstandard2.0` — there is NO `net8.0`/`net10.0` asset, so the `net10.0` consumer binds the `net6.0` surface)
- dependencies: `Microsoft.CSharp` (dynamic dispatch), `System.Collections.Immutable`, `System.ComponentModel.Annotations`; pure-managed AnyCPU, no native asset
- companion: the JSON-text schema/value codec (`JsonSchemaReader`/`JsonSchemaWriter`/`JsonSerializerBuilder`) lives in the transitive `Chr.Avro.Json` `10.13.1` (MIT, `lib/net6.0`/`netstandard2.0`), pulled by `Chr.Avro.Confluent`; it is NOT in this core assembly
- rail: avro-schema

## [02]-[PUBLIC_TYPES]

[SCHEMA_MODEL_TYPES]: the `Schema` algebra (namespace `Chr.Avro.Abstract`)
- rail: avro-schema

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE]        | [CAPABILITY]                                         |
| :-----: | :---------------- | :-------------------- | :--------------------------------------------------- |
|  [01]   | `Schema`          | abstract root         | base node; carries an optional `LogicalType`         |
|  [02]   | `PrimitiveSchema` | abstract primitive    | base of the scalar cases                             |
|  [03]   | `ComplexSchema`   | abstract complex      | base of array/map/named cases                        |
|  [04]   | `NamedSchema`     | abstract named        | `Name`/`Namespace`/`FullName`/`Aliases` carrier      |
|  [05]   | `RecordSchema`    | record case           | `ICollection<RecordField> Fields` + `Documentation`  |
|  [06]   | `EnumSchema`      | enum case             | `ICollection<string> Symbols` + `string? Default`    |
|  [07]   | `FixedSchema`     | fixed case            | `int Size` fixed-length byte blob                    |
|  [08]   | `ArraySchema`     | array case            | `Schema Item` element type                           |
|  [09]   | `MapSchema`       | map case              | `Schema Value` value type (string keys)              |
|  [10]   | `UnionSchema`     | union case            | `ICollection<Schema> Schemas` ordered branches       |
|  [11]   | `RecordField`     | field declaration     | `Name`/`Schema Type`/`DefaultValue? Default`/`Documentation` |

Primitive cases (each `: PrimitiveSchema`): `BooleanSchema`, `IntSchema`, `LongSchema`, `FloatSchema`, `DoubleSchema`, `BytesSchema`, `StringSchema`, `NullSchema`.
`NamedSchema` (base of `RecordSchema`/`EnumSchema`/`FixedSchema`): `string Name`, `string? Namespace`, `string FullName` (namespace-qualified, derived from `Name`+`Namespace`), `ICollection<string> Aliases` (evolution aliasing). `EnumSchema.Default` (a `string?` default symbol) and `RecordSchema.Documentation`/`EnumSchema.Documentation` carry the doc/default the registry JSON round-trips.
`DefaultValue` (abstract, `Schema Schema` + `abstract T? ToObject<T>()`) + `ObjectDefaultValue<TValue>`: a record field's typed default for read-time schema-resolution back-fill.
Construction-fault rails (namespace `Chr.Avro.Abstract`/`Chr.Avro.Representation`): `InvalidSchemaException`, `InvalidNameException`, `InvalidSymbolException` (model-validity faults from `SchemaBuilder`/the model ctors), `UnknownSchemaException` (`Chr.Avro.Representation`, an unrecognized JSON schema node on read) — a build/parse boundary catches these to a typed Persistence failure, never lets them escape inward.

[LOGICAL_TYPE_TYPES]: `LogicalType` decorators (namespace `Chr.Avro.Abstract`)
- rail: avro-schema

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]      | [CAPABILITY]                                       |
| :-----: | :------------------------------ | :------------------ | :------------------------------------------------- |
|  [01]   | `LogicalType`                   | abstract decorator  | annotates a base schema with semantic meaning      |
|  [02]   | `DecimalLogicalType`            | decimal             | `int Precision` + `int Scale` over `bytes`/`fixed` |
|  [03]   | `DateLogicalType`               | date                | days-since-epoch over `int`                        |
|  [04]   | `UuidLogicalType`               | uuid                | RFC 4122 over `string`                             |
|  [05]   | `DurationLogicalType`           | duration            | months/days/millis over `fixed(12)`                |
|  [06]   | `TimeLogicalType`               | abstract time       | base of the time-of-day precisions                 |
|  [07]   | `MillisecondTimeLogicalType`    | time (ms)           | time-of-day, `int` millis                          |
|  [08]   | `MicrosecondTimeLogicalType`    | time (µs)           | time-of-day, `long` micros                         |
|  [09]   | `TimestampLogicalType`          | abstract timestamp  | base of the timestamp precisions                   |
|  [10]   | `MillisecondTimestampLogicalType` | timestamp (ms)    | instant, `long` millis-since-epoch                 |
|  [11]   | `MicrosecondTimestampLogicalType` | timestamp (µs)    | instant, `long` micros-since-epoch                 |
|  [12]   | `NanosecondTimestampLogicalType`  | timestamp (ns)    | instant, `long` nanos-since-epoch                  |

[BUILDER_TYPES]: schema construction and the case framework
- rail: avro-schema

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]        | [CAPABILITY]                                       |
| :-----: | :------------------------- | :-------------------- | :------------------------------------------------- |
|  [01]   | `ISchemaBuilder`           | builder contract      | `BuildSchema<T>(SchemaBuilderContext?)`            |
|  [02]   | `SchemaBuilder`            | reflection builder    | CLR `Type` → `Schema` over an ordered case list    |
|  [03]   | `ISchemaBuilderCase`       | case contract         | one type-shape → schema rule                        |
|  [04]   | `SchemaBuilderCase`        | abstract case         | base of the per-shape builder cases                 |
|  [05]   | `SchemaBuilderCaseResult`  | case result           | matched schema or unmatched signal                  |
|  [06]   | `SchemaBuilderContext`     | build context         | shared schema cache across a recursive build        |
|  [07]   | `EnumBehavior`             | build policy          | `Symbolic` (named) / `Integral` (ordinal `int`) / `Nominal` (string) enums |
|  [08]   | `TemporalBehavior`         | build policy          | `Iso8601` / `EpochMicroseconds` / `EpochMilliseconds` / `EpochNanoseconds` |
|  [09]   | `NullableReferenceTypeBehavior` | build policy     | `None` / `All` / `Annotated` union-nullability      |

Per-shape `SchemaBuilderCase` implementations (each `: SchemaBuilderCase, ISchemaBuilderCase`): `Boolean`, `Int`/`Long`/`Float`/`Double`, `Bytes`/`String`/`Uri`, `Date`/`Time`/`Timestamp`, `Decimal`, `Duration`, `Enum`, `Array`/`Map`, `Record`, `Union`, `Uuid` `SchemaBuilderCase`.

[SERIALIZATION_FRAMEWORK_TYPES]: the open codec framework (namespace `Chr.Avro.Serialization`)
- rail: avro-schema

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]       | [CAPABILITY]                                       |
| :-----: | :----------------------------- | :------------------- | :------------------------------------------------- |
|  [01]   | `ISerializerBuilder<TContext>` | serializer contract  | encoding-specific serializer-builder seam          |
|  [02]   | `IDeserializerBuilder<TContext>` | deserializer contract | encoding-specific deserializer-builder seam      |
|  [03]   | `ISerializerBuilderCase<TContext, TResult>` | case contract | one schema-shape → write-expression rule      |
|  [04]   | `IDeserializerBuilderCase<TContext, TResult>` | case contract | one schema-shape → read-expression rule     |
|  [05]   | `ExpressionBuilder`            | expression base      | shared `Expression`-tree emission helpers          |
|  [06]   | `SerializerBuilderCase` / `DeserializerBuilderCase` | abstract case | per-schema-shape codec case bases       |

Abstract per-shape codec cases the binary/JSON legs specialize (each `: DeserializerBuilderCase` / `: SerializerBuilderCase`): `Array`, `Boolean`, `Bytes`, `Date`, `Decimal`, `Double`, `Duration`, `Enum`, `Fixed`, `Float`, `Int`, `Long`, `Map`, `Null`, `Record`, `String`, `Time`, `Timestamp`, `Union`.

[REPRESENTATION_TYPES]: the schema read/write seam (namespace `Chr.Avro.Representation`)
- rail: avro-schema

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]      | [CAPABILITY]                                          |
| :-----: | :------------------------- | :------------------ | :--------------------------------------------------- |
|  [01]   | `ISchemaReader<TContext>`  | read contract       | `Schema Read(Stream, TContext?)` — a serialized schema → model |
|  [02]   | `ISchemaWriter<TContext>`  | write contract      | `void Write(Schema, Stream, bool canonical, TContext?)` — model → serialized schema |
|  [03]   | `SchemaReaderCase` / `SchemaWriterCase` | abstract case | per-schema-shape representation case bases  |

The concrete JSON facade implementing these (`JsonSchemaReader`/`JsonSchemaWriter`, `IJsonSchemaReader`/`IJsonSchemaWriter`) ships in the transitive `Chr.Avro.Json`, not this assembly — see `api-chr-avro-confluent` `[INTEGRATION_LAW]`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: schema construction from a CLR type
- rail: avro-schema

| [INDEX] | [SURFACE]                                                                     | [CALL_SHAPE] | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------------------------------------- | :----------- | :------------------------------------------------- |
|  [01]   | `new SchemaBuilder(memberVisibility, enumBehavior, nullableReferenceTypeBehavior, temporalBehavior)` | ctor | builds the default ordered case set under explicit policy |
|  [02]   | `new SchemaBuilder(IEnumerable<Func<ISchemaBuilder, ISchemaBuilderCase>>)`    | ctor         | builds from a fully custom case list               |
|  [03]   | `SchemaBuilder.BuildSchema<T>(SchemaBuilderContext?)` → `Schema`              | build call   | derives an Avro schema from `typeof(T)`            |
|  [04]   | `SchemaBuilder.BuildSchema(Type, SchemaBuilderContext?)` → `Schema`           | build call   | derives a schema from a runtime `Type`             |
|  [05]   | `SchemaBuilder.CreateDefaultCaseBuilders(memberVisibility, enumBehavior, nullableReferenceTypeBehavior, temporalBehavior)` | factory | the default case list for custom composition |
|  [06]   | `new SchemaBuilderContext()`                                                  | ctor         | the schema cache for one recursive build           |

[ENTRYPOINT_SCOPE]: schema model construction
- rail: avro-schema

| [INDEX] | [SURFACE]                                                  | [CALL_SHAPE] | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------- | :----------- | :-------------------------------------------- |
|  [01]   | `new RecordSchema(string name, IEnumerable<RecordField>?)` | ctor         | constructs a record with named fields         |
|  [02]   | `new RecordField(string name, Schema type)`                | ctor         | one field; set `.Default`/`.Documentation`    |
|  [03]   | `new EnumSchema(string name, IEnumerable<string>?)`        | ctor         | symbolic enum                                 |
|  [04]   | `new UnionSchema(IEnumerable<Schema>?)`                    | ctor         | ordered union (e.g. `[null, T]` nullable)     |
|  [05]   | `new ArraySchema(Schema item)` / `new MapSchema(Schema value)` | ctor     | array / string-keyed map                      |
|  [06]   | `new FixedSchema(string name, int size)`                   | ctor         | fixed-length byte blob                        |
|  [07]   | `schema.LogicalType = new DecimalLogicalType(precision, scale)` | decorate | attaches a logical type to a base schema      |

## [04]-[IMPLEMENTATION_LAW]

[AVRO_SCHEMA_TOPOLOGY]:
- `Chr.Avro.Abstract` — the `Schema` algebra and `LogicalType` decorators; `Chr.Avro.Serialization` — the open codec framework (`ISerializerBuilderCase`/`IDeserializerBuilderCase` over `ExpressionBuilder`); `Chr.Avro.Representation` — the schema read/write contracts
- `SchemaBuilder.BuildSchema<T>` walks the CLR type graph through an ordered `ISchemaBuilderCase` list, the first matching case yields a `SchemaBuilderCaseResult`; `SchemaBuilderContext` caches by type so recursive/cyclic records resolve once. `BuildSchema<T>` simply forwards to `BuildSchema(typeof(T))`
- the three build policies are load-bearing for evolution: `EnumBehavior` selects `Symbolic` (an Avro enum of named symbols) / `Integral` (an `int` ordinal) / `Nominal` (an Avro `string` carrying the symbol name); `NullableReferenceTypeBehavior.Annotated` reads C# nullable-reference annotations to emit `[null, T]` unions vs `All`/`None`; `TemporalBehavior` selects ISO-8601 string vs epoch micro/milli/nanosecond logical-type timestamps. Note the default skews: the `SchemaBuilder` ctor defaults `nullableReferenceTypeBehavior` to `Annotated`, while the static `CreateDefaultCaseBuilders` factory defaults it to `None` — a custom case list built through the factory does NOT inherit the ctor's annotation-aware nullability unless passed explicitly
- the codec framework is encoding-agnostic: a `*BuilderCase` emits a LINQ `Expression` fragment per schema shape; the concrete encoding (binary vs JSON) supplies the leaf read/write expressions, and the facade `Compile()`s the assembled tree into a delegate — there is no per-call reflection on the hot path
- `RecordField.Default` (typed `DefaultValue`) drives read-time schema resolution: when a reader schema has a field absent from the writer schema, the default back-fills it — the evolution back-compat seam

[STACKING]:
- Owning pages: the Avro schema model is the interchange-codec source on `Element/codec` (the binary leg compiles the body codec, `api-chr-avro-binary`), and the registry-governed Kafka-wire leg rides `Version/egress` via `api-chr-avro-confluent`.
- The schema model is the shared currency the binary and registry legs consume: `SchemaBuilder.BuildSchema<T>()` produces the `Schema` once, `BinarySerializerBuilder.BuildDelegate<T>(schema)` (`api-chr-avro-binary`) compiles the codec against it, and `SchemaRegistrySerializerBuilder.Build<T>(subject, ...)` (`api-chr-avro-confluent`) registers its JSON representation and prefixes the registry schema id. The model is built once per type, never per message.
- Boundary against the first-party Confluent Avro serde: this core's reason-to-exist over the `Apache.Avro` reference codec is the expression-tree-compiled CLR-schema-DERIVATION path — `SchemaBuilder` walks a CLR `Type` into a `Schema`, the binary leg compiles a reflection-free codec, and evolution is modelled on the `Schema`. The canonical KAFKA-wire Avro serde is `Confluent.SchemaRegistry.Serdes.Avro` (`api-schemaregistry-serdes-avro`), which rides the `Apache.Avro` codec over `GenericRecord`/`ISpecificRecord` (`avrogen` POCOs) and owns the `Confluent.Kafka` slot. So the Chr.Avro stack (this core + `api-chr-avro-binary` + the off-Kafka `api-chr-avro-confluent`) and the first-party Confluent serde are the two arms of the explicit carve — the CLR-derived expression-compiled path here vs the `Apache.Avro`-reference `GenericRecord`/`ISpecificRecord` Kafka path there — never two serdes framing the same Kafka payload.
- Avro vs the schemaless codecs: where `MessagePack` (`api-messagepack`) and `CBOR` (`api-cbor`) own self-describing schema-free snapshots/blobs, Avro owns the schema-governed, evolution-safe row payload — a record whose fields add/remove/rename over time routes here because `RecordField.Default` + `NamedSchema.Aliases` carry the resolution rules, which a schemaless codec cannot express. The codec choice is profile data, recorded in the body's codec receipt.
- Logical-type mapping stacks onto the temporal/decimal owners at the edge: an Avro `timestamp-micros` decodes to a `NodaTime`/`DateTimeOffset` instant and an Avro `decimal(p,s)` to a `System.Decimal`, so the boundary adapter maps `DecimalLogicalType`/`TimestampLogicalType` to the canonical internal temporal/numeric type rather than leaking the Avro logical-type wrapper inward.

[LOCAL_ADMISSION]:
- Chr.Avro is the schema-model + codegen owner inside the Avro interchange profile, not public Persistence vocabulary; the `Schema`, logical types, and build policies are profile data.
- A schema is built from the canonical record type via `SchemaBuilder`, never hand-authored as JSON text inside Persistence code; the JSON text is a registry-transport artifact owned by `Chr.Avro.Json`.
- Schema evolution (field add/remove, aliasing, default back-fill) is expressed through `RecordField.Default` and `NamedSchema.Aliases` on the model, never through ad-hoc payload patching.
- The encoder lives in `Chr.Avro.Binary` and the JSON codec in `Chr.Avro.Json`; this core ships neither — a catalog that cites a binary `Encode`/`Decode` or a `JsonSchemaReader` against this assembly is wrong.

[RAIL_LAW]:
- Package: `Chr.Avro`
- Owns: the abstract Avro schema model, logical-type decorators, reflection-driven schema derivation, and the open expression-builder codec framework
- Accept: `SchemaBuilder.BuildSchema<T>` derivation, model-level evolution via defaults/aliases, custom `ISchemaBuilderCase`/`*BuilderCase` extension
- Reject: hand-authored schema JSON in Persistence code, per-message reflection, and treating this core as an encoder
