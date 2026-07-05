# [RASM_PERSISTENCE_API_SCHEMAREGISTRY_SERDES_AVRO]

`Confluent.SchemaRegistry.Serdes.Avro` is the registry-governed Avro data-plane codec: `AvroSerializer<T>` and `AvroDeserializer<T>` implement the `Confluent.Kafka` `IAsyncSerializer<T>`/`IAsyncDeserializer<T>` slots directly, framing each payload with a Confluent schema-id and delegating evolution to a shared `ISchemaRegistryClient`. `T` is unconstrained: an `Avro.Generic.GenericRecord` (schema-driven, no codegen) or an `Avro.Specific.ISpecificRecord` POCO (`avrogen`-generated) both flow through the same serde, with the writer schema derived from the value's runtime Avro schema and registered/looked-up under the configured `SubjectNameStrategy`. It is the row-oriented, schema-evolving leg of the `Version/egress#EGRESS_SINK` rail, distinct from the abstract-model `Chr.Avro` path and from the schemaless `MessagePack` wire format.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Confluent.SchemaRegistry.Serdes.Avro`
- package: `Confluent.SchemaRegistry.Serdes.Avro`
- version: `2.14.2`
- license: Apache-2.0
- assembly: `Confluent.SchemaRegistry.Serdes.Avro`
- namespace: `Confluent.SchemaRegistry.Serdes`
- target: multi-target (`net462`, `net6.0`, `net8.0`, `netstandard2.0`); the `net10.0` consumer binds `lib/net8.0`
- managed: pure-managed AnyCPU, no native asset
- composes: `Confluent.SchemaRegistry` (registry client, `SchemaId` framing, `RuleRegistry`), `Confluent.Kafka` (the `IAsyncSerializer<T>`/`IAsyncDeserializer<T>`/`SerializationContext` codec contract), and `Apache.Avro` (`Avro.Generic.GenericRecord`, `Avro.Specific.ISpecificRecord`, the Avro reflect schema builder)
- rail: cdc-egress (Avro)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: serde family
- rail: cdc-egress (Avro)

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]    | [RAIL]                                          |
| :-----: | :------------------------ | :--------------- | :---------------------------------------------- |
|  [01]   | `AvroSerializer<T>`       | async serializer | `IAsyncSerializer<T>`; registers + frames the id |
|  [02]   | `AvroDeserializer<T>`     | async deserializer | `IAsyncDeserializer<T>`; resolves id + decodes |
|  [03]   | `AvroSerializerConfig`    | serializer config | register/normalize/buffer/subject/id-strategy   |
|  [04]   | `AvroDeserializerConfig`  | deserializer config | latest-version + id-strategy                  |

[PUBLIC_TYPE_SCOPE]: composed contract family (re-stated from siblings)
- rail: cdc-egress (Avro)

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]   | [ORIGIN]                                        |
| :-----: | :------------------------------ | :-------------- | :---------------------------------------------- |
|  [01]   | `IAsyncSerializer<T>`           | codec slot      | `Confluent.Kafka` — the `SetValueSerializer` target |
|  [02]   | `IAsyncDeserializer<T>`         | codec slot      | `Confluent.Kafka` — the `SetValueDeserializer` target |
|  [03]   | `SerializationContext`          | codec context   | `Confluent.Kafka` — component (`Key`/`Value`), topic, headers |
|  [04]   | `ISchemaRegistryClient`         | registry client | `Confluent.SchemaRegistry` — the shared registry leg |
|  [05]   | `RuleRegistry`                  | rule registry   | `Confluent.SchemaRegistry` — CSFLE/migration executors |
|  [06]   | `Avro.Generic.GenericRecord`    | dynamic record  | `Apache.Avro` — schema-driven `T`, no codegen   |
|  [07]   | `Avro.Specific.ISpecificRecord` | generated record | `Apache.Avro` — `avrogen` POCO `T`             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction
- rail: cdc-egress (Avro)

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :--------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `new AvroSerializer<T>(client, config?, ruleRegistry?)`    | ctor           | binds registry + config + optional rule set     |
|  [02]   | `new AvroDeserializer<T>(client, config?, ruleRegistry?)`  | ctor           | binds registry + config + optional rule set     |
|  [03]   | `new AvroSerializer<T>(client)` / `new AvroDeserializer<T>(client)` | ctor   | registry-only, default config                   |
|  [04]   | `AvroSerializer<T>.DefaultInitialBufferSize` (`= 1024`)    | const          | initial serialize buffer in bytes               |

[ENTRYPOINT_SCOPE]: codec invocation (Confluent.Kafka slot)
- rail: cdc-egress (Avro)

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :--------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `SerializeAsync(value, context)` -> `Task<byte[]>`         | encode         | registers/resolves the schema, frames the id, writes Avro binary |
|  [02]   | `DeserializeAsync(data, isNull, context)` -> `Task<T>`     | decode         | reads the id, fetches the writer schema, decodes against the reader schema |
|  [03]   | `producerBuilder.SetValueSerializer(avroSerializer)`       | wiring         | mounts the serde on the `Confluent.Kafka` value slot |
|  [04]   | `consumerBuilder.SetValueDeserializer(avroDeserializer)`   | wiring         | mounts the serde on the consumer value slot     |

[ENTRYPOINT_SCOPE]: config tunables (`AvroSerializerConfig` / `AvroDeserializerConfig`)
- rail: cdc-egress (Avro)

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :----------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `AutoRegisterSchemas` (`bool?`)            | serializer     | auto-register on first use (production: `false`) |
|  [02]   | `NormalizeSchemas` (`bool?`)               | serializer     | canonical-form normalization before register/lookup |
|  [03]   | `UseLatestVersion` (`bool?`)               | both           | pin the latest registered version as writer/reader |
|  [04]   | `UseSchemaId` (`int?`)                      | serializer     | force a specific registered schema id           |
|  [05]   | `BufferBytes` (`int?`)                      | serializer     | initial serialize buffer override               |
|  [06]   | `SubjectNameStrategy` (`SubjectNameStrategy?`) | both        | `Topic`/`Record`/`TopicRecord` subject derivation |
|  [07]   | `SchemaIdStrategy` (`SchemaIdSerializerStrategy?`) | serializer | `Prefix` (magic byte) vs. `Header` id framing  |
|  [08]   | `SchemaIdStrategy` (`SchemaIdDeserializerStrategy?`) | deserializer | `Prefix` vs. `Dual` id reading                |
|  [09]   | `UseLatestWithMetadata` (`IDictionary<string,string>`) | both    | pin the version whose registry `Metadata` matches |

## [04]-[IMPLEMENTATION_LAW]

[AVRO_SERDE_TOPOLOGY]:
- `AvroSerializer<T> : IAsyncSerializer<T>` and `AvroDeserializer<T> : IAsyncDeserializer<T>` — the serde mirrors the async `Confluent.Kafka` codec contract directly (Avro does not derive from the `AsyncSerializer<T,TParsed>` two-parameter base the Protobuf/Json serdes use; the parsed-schema type is internal to its `GenericSerializerImpl`/`SpecificSerializerImpl` dispatch).
- `T` is unconstrained. The serde dispatches on the runtime type: a `GenericRecord` carries its `Avro.Schema` and serializes schema-driven; an `ISpecificRecord` exposes its generated `Schema` for the writer schema; a plain POCO is mapped through the Avro reflect builder. The writer schema is the value's Avro schema, registered/resolved under `SubjectNameStrategy`; the wire payload is the framed schema id plus Avro binary, never the embedded schema.
- the wire framing is governed by the shared `Confluent.SchemaRegistry` id encoder (`PrefixSchemaIdEncoder` default, magic byte `0x00`); `SchemaIdStrategy = Header` moves the id to a Kafka header. The deserializer reads the id (prefix or, under `Dual`, header), fetches the writer schema by id through the cached client, and decodes against the reader schema `T` applying Avro reader/writer resolution rules.
- `DeserializeAsync(ReadOnlyMemory<byte> data, bool isNull, SerializationContext context)` — `isNull` distinguishes a Kafka tombstone (null value) from an empty payload; the serde returns `default(T)` for a tombstone rather than faulting.
- `SerializeAsync(T value, SerializationContext context)` returns `Task<byte[]>`; the `SerializationContext.Component` (`MessageComponentType.Key`/`Value`) drives the `<topic>-key`/`<topic>-value` subject split under `SubjectNameStrategy.Topic`.

[LOCAL_ADMISSION]:
- The Avro serde is the row-oriented evolution-safe leg of the changefeed egress: built once per stream and mounted on the `Confluent.Kafka` `ProducerBuilder.SetValueSerializer`/`ConsumerBuilder.SetValueDeserializer` slot, sharing the single `CachedSchemaRegistryClient` with the Protobuf and Json serdes. The codec instance is fixed at build, never per call — the rejected form is a per-message `new AvroSerializer<T>`.
- `T` is the generated `ISpecificRecord` projection of the op-log event (the deterministic `avrogen` POCO), not a `GenericRecord`, on the durable path: the generated type pins the field set at compile time so a schema drift is a build break, while `GenericRecord` is admitted only for the dynamic catalog-replay lane where the reader schema is itself data.
- Production sets `AutoRegisterSchemas = false` and registers the writer schema out-of-band through `ISchemaRegistryClient.RegisterSchemaWithResponseAsync` under a `Backward`/`FullTransitive` `Compatibility` level, so a producer carrying an incompatible Avro schema is rejected at deploy. `NormalizeSchemas = true` is set so logically-identical schemas resolve to one registered id rather than churning ids on formatting drift.
- Field-level encryption rides the shared `RuleRegistry`: a `DomainRule` of `RuleMode.WriteRead` naming a field-encryption `IRuleExecutor` (Avro `bytes`/`string` fields tagged in the schema `Metadata` sensitive-field set) wraps/unwraps per-field DEKs against the `Element/identity#KEY_ENVELOPE` `EnvelopeKeyring` KMS clients (the `KmsProvider` axis owned at `Element/identity#KMS_CUSTODY`) during `SerializeAsync`/`DeserializeAsync` — the same rule engine the Protobuf and Json serdes use, never an Avro-specific crypto pass.
- This serde is the CANONICAL Kafka-wire Avro framing path and owns the `Confluent.Kafka` value/key slot; its body codec is the `Apache.Avro` `1.12.1` REFERENCE codec over `GenericRecord`/`ISpecificRecord` (`avrogen` POCOs). The `Chr.Avro` admission (`api-chr-avro` + `api-chr-avro-binary` + `api-chr-avro-confluent`) is the DISTINCT path: a CLR-`Type`-derived abstract `Schema` model and an expression-tree-COMPILED reflection-free body codec — it depends on `Chr.Avro.Binary`/`Chr.Avro.Json`, NOT on `Apache.Avro`, so the two stacks share NO codec and the carve is by codec engine, not a shared one. They never both frame the same payload — `Chr.Avro.Confluent` is the rejected alternative on the Kafka slot this serde owns, and is admitted only for an OFF-Kafka registry-governed Avro body (an object-store blob or non-Kafka transport).

[RAIL_LAW]:
- Package: `Confluent.SchemaRegistry.Serdes.Avro`
- Owns: registry-governed Avro encode/decode on the Kafka value/key slot — schema-id framing, `SubjectNameStrategy` subject derivation, generic and specific Avro `T`, and rule-engine field transforms during serialize/deserialize.
- Accept: a build-time `AvroSerializer<T>`/`AvroDeserializer<T>` over the shared `ISchemaRegistryClient`, a generated `ISpecificRecord` `T` on the durable path, `AutoRegisterSchemas = false` with out-of-band governed registration, `NormalizeSchemas = true`, and CSFLE through the shared `RuleRegistry`.
- Reject: a per-message serde instance, `AutoRegisterSchemas` on the durable changefeed producer, a `GenericRecord` `T` where the event shape is statically known, hand-rolled magic-byte framing, and double-framing an Avro payload through both this serde and `Chr.Avro.Confluent`.
