# [RASM_PERSISTENCE_API_SCHEMAREGISTRY_SERDES_PROTOBUF]

`Confluent.SchemaRegistry.Serdes.Protobuf` is the registry-governed Protobuf codec on the Kafka value/key slot: it registers a message's whole `FileDescriptorSet` with its import references, frames each payload with the schema id and a message-index path, and parses the binary back into the generated message type. `T` binds a `Google.Protobuf`-generated message by compile-time constraint, so the field set is a build artifact and no dynamic message reaches the durable path. Protobuf-typed topics take this codec as the binary leg of the `Version/egress#EGRESS_SINK` rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Confluent.SchemaRegistry.Serdes.Protobuf`
- package: `Confluent.SchemaRegistry.Serdes.Protobuf`
- license: Apache-2.0
- assembly: `Confluent.SchemaRegistry.Serdes.Protobuf`
- namespace: `Confluent.SchemaRegistry.Serdes`, `Confluent.SchemaRegistry.Serdes.Protobuf`
- managed: pure-managed AnyCPU, no native asset
- rail: cdc-egress (Protobuf)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: serde, config, and bundled `confluent.type` descriptors

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]       | [CAPABILITY]                                                      |
| :-----: | :--------------------------- | :------------------ | :---------------------------------------------------------------- |
|  [01]   | `ProtobufSerializer<T>`      | async serializer    | registers the descriptor set, frames id + message-index           |
|  [02]   | `ProtobufDeserializer<T>`    | async deserializer  | resolves the id, parses into a fresh `new T()`                    |
|  [03]   | `ProtobufSerializerConfig`   | serializer config   | `: SerdeConfig`; register, normalize, buffer, subject, references |
|  [04]   | `ProtobufDeserializerConfig` | deserializer config | `: SerdeConfig`; latest-version, id-strategy, framing             |
|  [05]   | `Meta`                       | field-meta message  | `Doc`, `Params`, `Tags` — the per-field annotation carrier        |
|  [06]   | `Decimal`                    | numeric message     | `Value`, `Precision`, `Scale` — exact decimal on the wire         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction
- [SHAPE]: ctor

| [INDEX] | [SURFACE]                                                                                      | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `new ProtobufSerializer<T>(ISchemaRegistryClient, ProtobufSerializerConfig, RuleRegistry)`     | binds registry, config, rule set      |
|  [02]   | `new ProtobufDeserializer<T>(ISchemaRegistryClient, ProtobufDeserializerConfig, RuleRegistry)` | binds registry, typed config, rules   |
|  [03]   | `new ProtobufDeserializer<T>(ISchemaRegistryClient, IEnumerable<KeyValuePair<string,string>>)` | binds registry with raw K/V config    |
|  [04]   | `new ProtobufDeserializer<T>(IEnumerable<KeyValuePair<string,string>>)`                        | registry-less decode, id framing only |

[ENTRYPOINT_SCOPE]: codec invocation
- [SHAPE]: instance

| [INDEX] | [SURFACE]                                                                       | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------------------------ | :------------------------------------------------- |
|  [01]   | `SerializeAsync(T, SerializationContext) -> Task<byte[]>`                       | registers the descriptor set, frames id + index    |
|  [02]   | `DeserializeAsync(ReadOnlyMemory<byte>, bool, SerializationContext) -> Task<T>` | reads id + message-index, parses the generated `T` |
|  [03]   | `ProducerBuilder.SetValueSerializer(IAsyncSerializer<T>)`                       | mounts the serde on the producer value slot        |
|  [04]   | `ConsumerBuilder.SetValueDeserializer(IAsyncDeserializer<T>)`                   | mounts the serde on the consumer value slot        |

- Tombstones short-circuit before the registry: `SerializeAsync` returns `null` for a `null` value, `DeserializeAsync` for `isNull`.

[ENTRYPOINT_SCOPE]: config knobs — subject, reference-subject, and id-framing strategy fix once at build, before the first send
- [SHAPE]: property

| [INDEX] | [SURFACE]                                                                    | [CONSUMER]   | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------------------------- | :----------- | :---------------------------------------- |
|  [01]   | `AutoRegisterSchemas` (`bool?`)                                              | serializer   | auto-register the descriptor set          |
|  [02]   | `NormalizeSchemas` (`bool?`)                                                 | serializer   | canonical-form normalization              |
|  [03]   | `UseSchemaId` (`int?`)                                                       | serializer   | force a registered descriptor id          |
|  [04]   | `BufferBytes` (`int?`)                                                       | serializer   | initial serialize buffer override         |
|  [05]   | `SkipKnownTypes` (`bool?`)                                                   | serializer   | omit `google.protobuf.*` well-known types |
|  [06]   | `UseLatestVersion` (`bool?`)                                                 | both         | pin the latest registered descriptor      |
|  [07]   | `UseLatestWithMetadata` (`IDictionary<string,string>`)                       | both         | pin the version whose `Metadata` matches  |
|  [08]   | `UseDeprecatedFormat` (`bool?`)                                              | both         | single-index framing, not the index path  |
|  [09]   | `SubjectNameStrategy` (`SubjectNameStrategy?`)                               | both         | `Topic`/`Record`/`TopicRecord`            |
|  [10]   | `SchemaIdStrategy` (`SchemaIdSerializerStrategy?`)                           | serializer   | `Prefix` or `Header` id framing           |
|  [11]   | `SchemaIdStrategy` (`SchemaIdDeserializerStrategy?`)                         | deserializer | `Prefix` or `Dual` id decoding            |
|  [12]   | `ReferenceSubjectNameStrategy` (`ReferenceSubjectNameStrategy?`)             | serializer   | `ReferenceName`/`Qualified`/`Custom`      |
|  [13]   | `CustomReferenceSubjectNameStrategy` (`ICustomReferenceSubjectNameStrategy`) | serializer   | custom `.proto` import resolver           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ProtobufSerializer<T> : AsyncSerializer<T, FileDescriptorSet> where T : IMessage<T>, new()` and `ProtobufDeserializer<T> : AsyncDeserializer<T, FileDescriptorSet> where T : class, IMessage<T>, new()`; the base owns registration, normalization, id encoding, and buffer sizing, and this serde supplies only descriptor extraction and binary parse.
- Payload framing carries the schema id, then a message-index path, then the Protobuf binary; the index selects which message inside a multi-message `.proto` the payload is, and `ISchemaIdEncoder.CalculateSize(ref SchemaId)` sizes the prefix.
- Registration is descriptor-set-wide: the serializer extracts `T`'s `FileDescriptorSet` — the `.proto` with its transitive imports — and registers each file as a `Schema` of `SchemaType.Protobuf` carrying `SchemaReference` rows for its imports, `ReferenceSubjectNameStrategy.Qualified` mapping `mypackage/myfile.proto` to `mypackage.myfile`.

[STACKING]:
- `Confluent.SchemaRegistry`(`.api/api-schemaregistry.md`): one shared `ISchemaRegistryClient` registers the `FileDescriptorSet` and its `SchemaReference` imports, `SchemaId` frames the id and message-index list, and `RuleRegistry` supplies the CSFLE and migration executors.
- `Confluent.Kafka`(`.api/api-kafka.md`): the serde IS the `IAsyncSerializer<T>`/`IAsyncDeserializer<T>` mounted on `ProducerBuilder.SetValueSerializer`/`ConsumerBuilder.SetValueDeserializer`; `SerializationContext.Component` carries the key/value split.
- `Google.Protobuf`(`../../.api/api-protobuf.md`): `IMessage<T>` is the `T` constraint and `FileDescriptor`/`FileDescriptorSet` the registered descriptor graph — the wire form the `Rasm.Compute` egress (`Version/egress#WIRE`) already emits.
- `CloudNative.CloudEvents.Kafka`(`.api/api-cloudevents.md`): `SchemaIdSerializerStrategy.Header` moves the id into `Message<TKey,TValue>.Headers` beside the CloudEvents attributes, leaving the value a clean Protobuf message for a non-Confluent consumer.
- within-lib: one serde per changefeed stream, built at composition over the single shared `CachedSchemaRegistryClient`, its strategy set frozen by config before the first send.

[LOCAL_ADMISSION]:
- Protobuf serdes carry the changefeed streams whose source-of-truth schema is the `.proto` at the `Rasm.Compute` interop boundary, holding one wire format end to end rather than re-encoding a `Google.Protobuf` message as Avro.
- `T` compiles from the canonical `.proto`, so the durable path serializes a generated message and the field set moves only through a rebuild.
- Production pins `SkipKnownTypes = true` and `ReferenceSubjectNameStrategy = Qualified`, keeping imported-`.proto` subjects deterministic across producers, with the descriptor set registered out-of-band under a governed `Compatibility` level.
- Field-level encryption reads Protobuf candidates through `RuleContext.FieldContext.Tags` matched against the schema `Metadata.Sensitive` set, the per-field annotations riding the bundled `Meta` descriptor.

[RAIL_LAW]:
- Package: `Confluent.SchemaRegistry.Serdes.Protobuf`
- Owns: registry-governed Protobuf encode/decode on the Kafka value/key slot — `FileDescriptorSet` registration with import references, message-index framing, subject and reference-subject derivation, and rule-engine field transforms.
- Accept: a build-time `ProtobufSerializer<T>`/`ProtobufDeserializer<T>` over a `Google.Protobuf`-generated `T` and the shared `ISchemaRegistryClient`, out-of-band governed registration, `SkipKnownTypes = true`, `ReferenceSubjectNameStrategy = Qualified`, and CSFLE through the shared `RuleRegistry`.
- Reject: a per-message serde instance, `AutoRegisterSchemas` on the durable changefeed producer, a non-`IMessage<T>` payload re-encoded as Protobuf, `UseDeprecatedFormat` on a governed topic, and a `Google.Protobuf` message re-encoded as Avro.
