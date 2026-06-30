# [RASM_PERSISTENCE_API_SCHEMAREGISTRY_SERDES_PROTOBUF]

`Confluent.SchemaRegistry.Serdes.Protobuf` is the registry-governed Protobuf data-plane codec: `ProtobufSerializer<T>` and `ProtobufDeserializer<T>` derive from the shared `AsyncSerializer<T, FileDescriptorSet>`/`AsyncDeserializer<T, FileDescriptorSet>` base, frame each payload with a Confluent schema id plus the Protobuf message-index path, and register the `.proto` `FileDescriptorSet` (with transitive imports) through a shared `ISchemaRegistryClient`. `T` is constrained `where T : IMessage<T>, new()` (serializer) / `where T : class, IMessage<T>, new()` (deserializer), so `T` is exactly a `Google.Protobuf`-generated message — the admitted `Google.Protobuf 3.35.1` wire format the `Rasm.Compute` egress (`Sync/egress#WIRE`) produces. It is the schema-evolving binary leg of the `Sync/egress#EGRESS_SINK` rail for Protobuf-typed topics, distinct from the Avro and JSON-Schema serdes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Confluent.SchemaRegistry.Serdes.Protobuf`
- package: `Confluent.SchemaRegistry.Serdes.Protobuf`
- version: `2.14.2`
- license: Apache-2.0
- assembly: `Confluent.SchemaRegistry.Serdes.Protobuf`
- namespace: `Confluent.SchemaRegistry.Serdes` (plus `Confluent.SchemaRegistry.Serdes.Protobuf` for the bundled `Meta`/`Decimal` confluent.type descriptors)
- target: multi-target (`net462`, `net6.0`, `net8.0`, `netstandard2.0`); the `net10.0` consumer binds `lib/net8.0`
- managed: pure-managed AnyCPU, no native asset
- composes: `Confluent.SchemaRegistry` (registry client, `SchemaId` message-index framing, `RuleRegistry`), `Confluent.Kafka` (the `IAsyncSerializer<T>`/`IAsyncDeserializer<T>`/`SerializationContext` codec contract), `Google.Protobuf` (the `IMessage<T>` generated-message constraint and binary wire format), and the `protobuf-net.Reflection`/`protobuf-net.Core` descriptor chain (`.proto` text -> `FileDescriptorSet` for registration)
- rail: cdc-egress (Protobuf)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: serde family
- rail: cdc-egress (Protobuf)

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]    | [RAIL]                                          |
| :-----: | :--------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `ProtobufSerializer<T>`      | async serializer | `where T : IMessage<T>, new()`; registers descriptor + frames id |
|  [02]   | `ProtobufDeserializer<T>`    | async deserializer | `where T : class, IMessage<T>, new()`; resolves id + parses |
|  [03]   | `ProtobufSerializerConfig`   | serializer config | register/normalize/buffer/subject/ref-strategy  |
|  [04]   | `ProtobufDeserializerConfig` | deserializer config | latest-version + id-strategy + deprecated-format |

[PUBLIC_TYPE_SCOPE]: composed contract family (re-stated from siblings)
- rail: cdc-egress (Protobuf)

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]   | [ORIGIN]                                        |
| :-----: | :------------------------------------ | :-------------- | :---------------------------------------------- |
|  [01]   | `AsyncSerializer<T, FileDescriptorSet>` | serde base     | `Confluent.SchemaRegistry` — shared encode base (id encoder, buffer) |
|  [02]   | `AsyncDeserializer<T, FileDescriptorSet>` | serde base   | `Confluent.SchemaRegistry` — shared decode base |
|  [03]   | `IAsyncSerializer<T>` / `IAsyncDeserializer<T>` | codec slot | `Confluent.Kafka` — the `SetValueSerializer`/`SetValueDeserializer` target |
|  [04]   | `SerializationContext`                | codec context   | `Confluent.Kafka` — component, topic, headers   |
|  [05]   | `ISchemaRegistryClient`               | registry client | `Confluent.SchemaRegistry` — the shared registry leg |
|  [06]   | `RuleRegistry`                        | rule registry   | `Confluent.SchemaRegistry` — CSFLE/migration executors |
|  [07]   | `IMessage<T>` / `FileDescriptor`      | message + descriptor | `Google.Protobuf` — generated `T` and its reflection |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction
- rail: cdc-egress (Protobuf)

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :------------------------------------------------------------ | :------------- | :---------------------------------------------- |
|  [01]   | `new ProtobufSerializer<T>(client, config?, ruleRegistry?)`   | ctor           | binds registry + config + optional rule set     |
|  [02]   | `new ProtobufDeserializer<T>(client, config, ruleRegistry?)`  | ctor           | binds registry + config + optional rule set     |
|  [03]   | `new ProtobufDeserializer<T>(client, config?)`                | ctor           | registry + raw K/V config                       |
|  [04]   | `new ProtobufDeserializer<T>(config?)`                        | ctor           | registry-less decode (id framing only, no schema fetch) |

[ENTRYPOINT_SCOPE]: codec invocation (Confluent.Kafka slot)
- rail: cdc-egress (Protobuf)

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :--------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `SerializeAsync(value, context)` -> `Task<byte[]>`         | encode         | registers the `FileDescriptorSet`, frames id + message-index, writes Protobuf binary |
|  [02]   | `DeserializeAsync(data, isNull, context)` -> `Task<T>`     | decode         | reads id + message-index, parses into the generated `T` |
|  [03]   | `producerBuilder.SetValueSerializer(protobufSerializer)`   | wiring         | mounts the serde on the `Confluent.Kafka` value slot |
|  [04]   | `consumerBuilder.SetValueDeserializer(protobufDeserializer)` | wiring       | mounts the serde on the consumer value slot     |

[ENTRYPOINT_SCOPE]: config tunables (`ProtobufSerializerConfig` / `ProtobufDeserializerConfig`)
- rail: cdc-egress (Protobuf)

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :----------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `AutoRegisterSchemas` (`bool?`)                        | serializer     | auto-register the descriptor set (production: `false`) |
|  [02]   | `NormalizeSchemas` (`bool?`)                           | serializer     | canonical-form normalization                    |
|  [03]   | `UseLatestVersion` (`bool?`)                           | both           | pin the latest registered descriptor version    |
|  [04]   | `UseSchemaId` (`int?`)                                  | serializer     | force a specific registered descriptor id       |
|  [05]   | `SkipKnownTypes` (`bool?`)                             | serializer     | omit well-known `google.protobuf.*` types from the registered set |
|  [06]   | `UseDeprecatedFormat` (`bool?`)                        | both           | legacy single-message-index framing (interop)   |
|  [07]   | `ReferenceSubjectNameStrategy` (`ReferenceSubjectNameStrategy?`) | serializer | `ReferenceName`/`Qualified`/`Custom` for imported `.proto` references |
|  [08]   | `CustomReferenceSubjectNameStrategy` (`ICustomReferenceSubjectNameStrategy`) | serializer | custom resolver for `.proto` import subjects |
|  [09]   | `SubjectNameStrategy` (`SubjectNameStrategy?`)         | both           | `Topic`/`Record`/`TopicRecord` subject derivation |
|  [10]   | `SchemaIdStrategy` (`SchemaIdSerializerStrategy?` / `SchemaIdDeserializerStrategy?`) | both | `Prefix` vs. `Header` / `Dual` id framing      |
|  [11]   | `UseLatestWithMetadata` (`IDictionary<string,string>`) | both          | pin the descriptor version whose `Metadata` matches |

## [04]-[IMPLEMENTATION_LAW]

[PROTOBUF_SERDE_TOPOLOGY]:
- `ProtobufSerializer<T> : AsyncSerializer<T, FileDescriptorSet> where T : IMessage<T>, new()` and `ProtobufDeserializer<T> : AsyncDeserializer<T, FileDescriptorSet> where T : class, IMessage<T>, new()` — the parsed-schema type is `Google.Protobuf.Reflection.FileDescriptorSet`, and the constraint binds `T` to a `Google.Protobuf`-generated message. The shared `AsyncSerializer<T,TParsed>` base owns `autoRegisterSchema`, `normalizeSchemas`, the `ISchemaIdEncoder schemaIdEncoder` (default `PrefixSchemaIdEncoder`), and `initialBufferSize`; this serde supplies only the Protobuf descriptor extraction and binary parse.
- the wire payload is the framed schema id plus a Protobuf message-index path plus the Protobuf binary. The message-index disambiguates which message inside a multi-message `.proto` the payload is; `UseDeprecatedFormat = true` selects the legacy single-index framing for interop with older Confluent clients, and `SchemaId` carries the message-index list (`CalculateIdSize` sizes the framing accordingly).
- registration is descriptor-set-wide: the serializer extracts the `FileDescriptorSet` for `T` (the `.proto` plus its transitive imports, minus the well-known `google.protobuf.*` types when `SkipKnownTypes = true`) and registers each `.proto` as a `Schema` of `SchemaType.Protobuf` with `SchemaReference` rows for its imports. `ReferenceSubjectNameStrategy` derives the subject for each imported `.proto`: `Qualified` maps `mypackage/myfile.proto` to `mypackage.myfile`.
- `DeserializeAsync(ReadOnlyMemory<byte> data, bool isNull, SerializationContext context)` parses the Protobuf binary into a fresh `new T()` (hence the `new()` constraint); `isNull` returns the tombstone path. The registry-less ctor (`new ProtobufDeserializer<T>(config?)`) decodes id framing only and never fetches a writer schema — admitted only where evolution is not enforced.

[LOCAL_ADMISSION]:
- The Protobuf serde is the binary leg of the changefeed egress for the `Rasm.Compute` op events that are natively `Google.Protobuf` messages (`Sync/egress#WIRE`): built once per stream, mounted on the `Confluent.Kafka` `SetValueSerializer`/`SetValueDeserializer` slot, and sharing the single `CachedSchemaRegistryClient` with the Avro and JSON serdes. The codec is fixed at build, never per call.
- `T` is the `Google.Protobuf`-generated message type compiled from the canonical `.proto`; the compile-time constraint makes the field set a build artifact, so the serde never serializes a hand-built dynamic message on the durable path. The serde is chosen over Avro for exactly the streams whose source-of-truth schema is `.proto` (the Compute interop boundary), keeping one wire format end-to-end rather than re-encoding Protobuf as Avro.
- Production sets `AutoRegisterSchemas = false` and `SkipKnownTypes = true`, registering the descriptor set out-of-band under a `Backward`/`FullTransitive` `Compatibility` level so an incompatible `.proto` is rejected at deploy. `ReferenceSubjectNameStrategy = Qualified` keeps imported-`.proto` subjects deterministic across producers.
- Field-level encryption rides the shared `RuleRegistry`: a `DomainRule` of `RuleMode.WriteRead` naming a field-encryption `IRuleExecutor` (Protobuf `bytes`/`string` fields surfaced through `RuleContext.FieldContext.Tags` and matched against the schema `Metadata.Sensitive` set, with field-meta carried in the bundled `Confluent.SchemaRegistry.Serdes.Protobuf.Meta` descriptors) wraps/unwraps per-field DEKs against the `Element/identity#KEY_ENVELOPE` `EnvelopeKeyring` KMS clients (the `KmsProvider` axis owned at `Element/identity#AUTHORITY`) — the same rule engine the Avro and Json serdes use.
- The framing strategy aligns with the registry catalog: `SchemaIdSerializerStrategy.Header` moves the id to a Kafka header so the value payload stays a clean Protobuf message for a downstream non-Confluent Protobuf consumer, pairing with the `CloudNative.CloudEvents.Kafka` header binding on the same `Message<TKey, TValue>`.

[RAIL_LAW]:
- Package: `Confluent.SchemaRegistry.Serdes.Protobuf`
- Owns: registry-governed Protobuf encode/decode on the Kafka value/key slot — `FileDescriptorSet` registration with import references, message-index framing, `SubjectNameStrategy`/`ReferenceSubjectNameStrategy` subject derivation, and rule-engine field transforms.
- Accept: a build-time `ProtobufSerializer<T>`/`ProtobufDeserializer<T>` over a `Google.Protobuf`-generated `T` and the shared `ISchemaRegistryClient`, `AutoRegisterSchemas = false` with out-of-band governed registration, `SkipKnownTypes = true`, `ReferenceSubjectNameStrategy = Qualified`, and CSFLE through the shared `RuleRegistry`.
- Reject: a per-message serde instance, `AutoRegisterSchemas` on the durable changefeed producer, a non-`IMessage<T>` payload (re-encoding a POCO as Protobuf), `UseDeprecatedFormat` outside an explicit legacy-interop window, and re-encoding a `Google.Protobuf` message as Avro instead of using this serde.
