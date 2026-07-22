# [RASM_PERSISTENCE_API_SCHEMAREGISTRY_SERDES_AVRO]

`Confluent.SchemaRegistry.Serdes.Avro` is the registry-governed Avro data-plane codec: `AvroSerializer<T>`/`AvroDeserializer<T>` implement the `Confluent.Kafka` async codec slots directly, frame each payload with a schema id, and delegate evolution to a shared `ISchemaRegistryClient`. `T` is unconstrained — an `Avro.Generic.GenericRecord` (schema-driven, no codegen) or an `avrogen` `Avro.Specific.ISpecificRecord` POCO. It is the row-oriented, schema-evolving leg of the `Version/egress` sink, distinct from the abstract-model `Chr.Avro` path and the schemaless `MessagePack` wire.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Confluent.SchemaRegistry.Serdes.Avro`
- package: `Confluent.SchemaRegistry.Serdes.Avro` (Apache-2.0)
- assembly: `Confluent.SchemaRegistry.Serdes.Avro`
- namespace: `Confluent.SchemaRegistry.Serdes`
- managed: pure-managed AnyCPU, no native asset
- rail: cdc-egress (Avro)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: serde family

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]       | [CAPABILITY]                                     |
| :-----: | :----------------------- | :------------------ | :----------------------------------------------- |
|  [01]   | `AvroSerializer<T>`      | async serializer    | `IAsyncSerializer<T>`; registers + frames the id |
|  [02]   | `AvroDeserializer<T>`    | async deserializer  | `IAsyncDeserializer<T>`; resolves id + decodes   |
|  [03]   | `AvroSerializerConfig`   | serializer config   | register/normalize/buffer/subject/id-strategy    |
|  [04]   | `AvroDeserializerConfig` | deserializer config | latest-version + id-strategy                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction

| [INDEX] | [SURFACE]                                                 | [SHAPE] | [CAPABILITY]                                |
| :-----: | :-------------------------------------------------------- | :------ | :------------------------------------------ |
|  [01]   | `new AvroSerializer<T>(client, config?, ruleRegistry?)`   | ctor    | binds registry + config + optional rule set |
|  [02]   | `new AvroDeserializer<T>(client, config?, ruleRegistry?)` | ctor    | binds registry + config + optional rule set |
|  [03]   | `new AvroSerializer<T>(client)`                           | ctor    | registry-only, default config               |
|  [04]   | `AvroSerializer<T>.DefaultInitialBufferSize` (`= 1024`)   | const   | initial serialize buffer in bytes           |

[ENTRYPOINT_SCOPE]: codec invocation (Confluent.Kafka slot)

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                                                |
| :-----: | :-------------------------------------------------------- | :------- | :---------------------------------------------------------- |
|  [01]   | `SerializeAsync(T, SerializationContext) -> Task<byte[]>` | instance | registers/resolves schema, frames id, writes Avro binary    |
|  [02]   | `DeserializeAsync(data, isNull, context) -> Task<T>`      | instance | reads id, fetches writer schema, decodes against reader `T` |
|  [03]   | `producerBuilder.SetValueSerializer(serializer)`          | instance | mounts the serde on the `Confluent.Kafka` value slot        |
|  [04]   | `consumerBuilder.SetValueDeserializer(deserializer)`      | instance | mounts the serde on the consumer value slot                 |

[ENTRYPOINT_SCOPE]: config tunables (`AvroSerializerConfig` / `AvroDeserializerConfig`)

| [INDEX] | [SURFACE]                                              | [CONSUMER]   | [CAPABILITY]                                        |
| :-----: | :----------------------------------------------------- | :----------- | :-------------------------------------------------- |
|  [01]   | `AutoRegisterSchemas` (`bool?`)                        | serializer   | auto-register on first use (production: `false`)    |
|  [02]   | `NormalizeSchemas` (`bool?`)                           | serializer   | canonical-form normalization before register/lookup |
|  [03]   | `UseLatestVersion` (`bool?`)                           | both         | pin the latest registered version as writer/reader  |
|  [04]   | `UseSchemaId` (`int?`)                                 | serializer   | force a specific registered schema id               |
|  [05]   | `BufferBytes` (`int?`)                                 | serializer   | initial serialize buffer override                   |
|  [06]   | `SubjectNameStrategy` (`SubjectNameStrategy?`)         | both         | `Topic`/`Record`/`TopicRecord` subject derivation   |
|  [07]   | `SchemaIdStrategy` (`SchemaIdSerializerStrategy?`)     | serializer   | `Prefix` (magic byte) vs. `Header` id framing       |
|  [08]   | `SchemaIdStrategy` (`SchemaIdDeserializerStrategy?`)   | deserializer | `Prefix` vs. `Dual` id reading                      |
|  [09]   | `UseLatestWithMetadata` (`IDictionary<string,string>`) | both         | pin the version whose registry `Metadata` matches   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `T` is unconstrained; the serde dispatches on runtime type — a `GenericRecord` serializes schema-driven off its `Avro.Schema`, an `ISpecificRecord` off its generated `Schema`, a POCO through the Avro reflect builder. Writer schema is the value's Avro schema registered/resolved under `SubjectNameStrategy`; the wire payload is the framed schema id and Avro binary, never the embedded schema.
- Wire id framing is the shared `Confluent.SchemaRegistry` encoder: `Prefix` writes magic byte `0x00` + int32 id, `Header` moves the id to a Kafka header. Decode reads the id (prefix, or header under `Dual`), fetches the writer schema by id, and resolves it against reader `T` under Avro reader/writer rules.
- `DeserializeAsync` `isNull` distinguishes a Kafka tombstone from an empty payload — a tombstone returns `default(T)`, never a fault.
- `SerializeAsync`'s `SerializationContext.Component` (`Key`/`Value`) drives the `<topic>-key`/`<topic>-value` subject split under `SubjectNameStrategy.Topic`.

[STACKING]:
- `Confluent.Kafka`(`.api/api-kafka.md`): the serde IS the `IAsyncSerializer<T>`/`IAsyncDeserializer<T>` mounted on the `ProducerBuilder.SetValueSerializer`/`ConsumerBuilder.SetValueDeserializer` value/key slot; `SerializationContext.Component` carries the key/value split.
- `Confluent.SchemaRegistry`(`.api/api-schemaregistry.md`): each payload frames through the shared `SchemaId` encoder, and evolution and CSFLE ride the single shared `CachedSchemaRegistryClient` + `RuleRegistry` — the serde binds one shared client, never a per-message instance.
- `Apache.Avro` body codec: `GenericRecord` carries its `Avro.Schema`, `ISpecificRecord` exposes its generated `Schema`, a POCO maps through the reflect builder — the reference Avro binary body under the framed id.
- within-lib: built once per changefeed stream, sharing the one `CachedSchemaRegistryClient` with the Protobuf and Json serdes on the egress rail.

[LOCAL_ADMISSION]:
- Durable path pins `T` to the generated `ISpecificRecord` op-log projection so a schema drift is a build break; `GenericRecord` is admitted only for the dynamic catalog-replay lane where the reader schema is itself data.
- Production sets `AutoRegisterSchemas = false` and registers the writer schema out-of-band under a `Backward`/`FullTransitive` `Compatibility` level, rejecting an incompatible producer at deploy; `NormalizeSchemas = true` collapses formatting drift to one registered id.
- CSFLE rides the shared `RuleRegistry` (`api-schemaregistry`): a `RuleMode.WriteRead` `DomainRule` transforms the Avro `bytes`/`string` fields tagged in the `Metadata` sensitive-field set, the same rule engine the Protobuf and Json serdes use.
- `Chr.Avro` (`api-chr-avro`) is the distinct off-Kafka path — a CLR-`Type`-derived `Schema` model over an expression-tree body codec sharing no codec with `Apache.Avro`; it never co-frames a payload this serde owns.

[RAIL_LAW]:
- Package: `Confluent.SchemaRegistry.Serdes.Avro`
- Owns: registry-governed Avro encode/decode on the Kafka value/key slot — schema-id framing, `SubjectNameStrategy` derivation, generic and specific Avro `T`, and rule-engine field transforms during serialize/deserialize.
- Accept: a build-time serde over the shared `ISchemaRegistryClient`, a generated `ISpecificRecord` `T` on the durable path, `AutoRegisterSchemas = false` with out-of-band registration, `NormalizeSchemas = true`, and CSFLE through the shared `RuleRegistry`.
- Reject: a per-message serde instance, `AutoRegisterSchemas` on the durable producer, a `GenericRecord` `T` where the event shape is statically known, hand-rolled magic-byte framing, and double-framing through both this serde and `Chr.Avro.Confluent`.
