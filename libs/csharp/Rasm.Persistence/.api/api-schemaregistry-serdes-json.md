# [RASM_PERSISTENCE_API_SCHEMAREGISTRY_SERDES_JSON]

`Confluent.SchemaRegistry.Serdes.Json` mints the registry-governed JSON-Schema data-plane codec: `JsonSerializer<T>` and `JsonDeserializer<T>` (both `where T : class`, both over the shared `AsyncSerializer<T, JsonSchema>`/`AsyncDeserializer<T, JsonSchema>` base) frame each payload with a Confluent schema id and validate the document against the registered `NJsonSchema.JsonSchema` on write and read. Its `Newtonsoft.Json` document codec encodes the registry-governed payload body of the `Version/egress` rail, the body the `CloudNative.CloudEvents.SystemTextJson` envelope formatter never touches.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Confluent.SchemaRegistry.Serdes.Json`
- package: `Confluent.SchemaRegistry.Serdes.Json` (Apache-2.0)
- assembly: `Confluent.SchemaRegistry.Serdes.Json`
- namespace: `Confluent.SchemaRegistry.Serdes`
- managed: pure-managed AnyCPU, no native asset
- rail: cdc-egress (JSON Schema)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: serde family

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]       | [CAPABILITY]                                     |
| :-----: | :----------------------- | :------------------ | :----------------------------------------------- |
|  [01]   | `JsonSerializer<T>`      | async serializer    | generate schema, validate, frame id              |
|  [02]   | `JsonDeserializer<T>`    | async deserializer  | resolve id, validate, parse                      |
|  [03]   | `JsonSerializerConfig`   | serializer config   | register/normalize/validate/subject/id knobs     |
|  [04]   | `JsonDeserializerConfig` | deserializer config | latest-version/validate/id-strategy knobs        |
|  [05]   | `JsonSchemaResolver`     | reference resolver  | `GetResolvedSchema()` folds `$ref` rows into one |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction
- [SHAPE]: ctor

Every ctor takes an optional `jsonSchemaGeneratorSettings` the table omits.

| [INDEX] | [SURFACE]                                                       | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `new JsonSerializer<T>(client, config?, ruleRegistry?)`         | generate the schema from `T` via NJsonSchema |
|  [02]   | `new JsonSerializer<T>(client, schema, config?, ruleRegistry?)` | use an explicit registered `Schema`          |
|  [03]   | `new JsonDeserializer<T>(client, config, ruleRegistry?)`        | registry plus typed config                   |
|  [04]   | `new JsonDeserializer<T>(client, schema, config?)`              | explicit reader `Schema`                     |
|  [05]   | `new JsonDeserializer<T>(config?)`                              | registry-less decode, id framing only        |

[ENTRYPOINT_SCOPE]: codec invocation on the `Confluent.Kafka` slot
- [SHAPE]: instance

| [INDEX] | [SURFACE]                                            | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `SerializeAsync(value, context) -> Task<byte[]>`     | generate schema, validate doc, frame id, write JSON |
|  [02]   | `DeserializeAsync(data, isNull, context) -> Task<T>` | read id, validate against writer schema, parse      |
|  [03]   | `producerBuilder.SetValueSerializer(serializer)`     | mount the serde on the value slot                   |
|  [04]   | `consumerBuilder.SetValueDeserializer(deserializer)` | mount on the consumer value slot                    |

[ENTRYPOINT_SCOPE]: config tunables (`JsonSerializerConfig` / `JsonDeserializerConfig`)
- [SHAPE]: property

| [INDEX] | [SURFACE]                                              | [CONSUMER]   | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------- | :----------- | :---------------------------------------- |
|  [01]   | `AutoRegisterSchemas` (`bool?`)                        | serializer   | auto-register the generated schema        |
|  [02]   | `NormalizeSchemas` (`bool?`)                           | serializer   | canonical-form normalization              |
|  [03]   | `UseLatestVersion` (`bool?`)                           | both         | pin the latest registered version         |
|  [04]   | `UseSchemaId` (`int?`)                                 | serializer   | force a specific registered id            |
|  [05]   | `BufferBytes` (`int?`)                                 | serializer   | initial serialize buffer override         |
|  [06]   | `Validate` (`bool?`)                                   | both         | validate the document against the schema  |
|  [07]   | `ValidateBeforeDomainRules` (`bool?`)                  | both         | validation ahead of the domain rules      |
|  [08]   | `LatestCompatibilityStrict` (`bool?`)                  | serializer   | strict compat check against latest        |
|  [09]   | `SubjectNameStrategy` (`SubjectNameStrategy?`)         | both         | `Topic`/`Record`/`TopicRecord` derivation |
|  [10]   | `SchemaIdStrategy` (`SchemaIdSerializerStrategy?`)     | serializer   | `Prefix` magic byte vs `Header` framing   |
|  [11]   | `SchemaIdStrategy` (`SchemaIdDeserializerStrategy?`)   | deserializer | `Prefix` vs `Dual` id reading             |
|  [12]   | `UseLatestWithMetadata` (`IDictionary<string,string>`) | both         | pin the version whose `Metadata` matches  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `JsonSerializer<T> : AsyncSerializer<T, JsonSchema>` and `JsonDeserializer<T> : AsyncDeserializer<T, JsonSchema>`, parsed-schema type `NJsonSchema.JsonSchema`; the shared base owns `autoRegisterSchema`, `normalizeSchemas`, the `ISchemaIdEncoder` (default `PrefixSchemaIdEncoder`), and `initialBufferSize`, while this serde supplies JSON-Schema generation, document validation, and the JSON read/write.
- Document codec is `Newtonsoft.Json` through the `NJsonSchema.NewtonsoftJson` chain, generation riding `NewtonsoftJsonSchemaGeneratorSettings`; a System.Text.Json `JsonSerializerOptions` never configures this serde.
- Wire payload is the framed schema id ahead of the UTF-8 JSON document; `Validate` gates both directions on JSON-Schema conformance, and `ValidateBeforeDomainRules` orders that validation ahead of the rule-engine transforms so an invalid document is rejected before a field-encryption rule runs.

[STACKING]:
- `Confluent.SchemaRegistry`(`.api/api-schemaregistry.md`): binds the shared `ISchemaRegistryClient`, derives from its `AsyncSerializer<T, JsonSchema>` base, frames every payload with `SchemaId`, and reads `RuleRegistry` for CSFLE and migration over one shared `CachedSchemaRegistryClient`, never a per-message instance.
- `Confluent.Kafka`(`.api/api-kafka.md`): the serde IS the `IAsyncSerializer<T>`/`IAsyncDeserializer<T>` over `SerializationContext`, mounted on the `SetValueSerializer`/`SetValueDeserializer` value slot.
- `NJsonSchema` schema engine: `JsonSchemaResolver.GetResolvedSchema()` folds `Schema.References` `$ref` rows into the one draft `JsonSchema` both directions validate against.
- `CloudNative.CloudEvents.SystemTextJson`(`.api/api-cloudevents.md`): its `JsonEventFormatter` frames the CloudEvents envelope on `Message<TKey,TValue>` headers/body while this serde codes the payload body `Data`; `SchemaIdStrategy = Header` writes the id to `SchemaId.VALUE_SCHEMA_ID_HEADER` (`__value_schema_id`) disjoint from the `ce_`-prefixed envelope headers, and `SchemaIdDeserializerStrategy.Dual` reads a `Header`- or `Prefix`-framed id so both producers interoperate.

[LOCAL_ADMISSION]:
- `T` is the op-event projection record (a `class`); its generated `JsonSchema` registers out-of-band, so the serde never auto-evolves the contract. Production sets `AutoRegisterSchemas = false` and `Validate = true` under a `Backward`/`FullTransitive` `Compatibility` level, rejecting an invalid or incompatible document at the codec, with `LatestCompatibilityStrict = true` guarding against a stale schema.
- Field-level encryption rides the shared `RuleRegistry`: a `RuleMode.WriteRead` domain rule encrypts the JSON string fields the schema `Metadata` marks sensitive, validated first under `ValidateBeforeDomainRules = true`.
- Topics whose consumers want self-describing JSON under registry-enforced schema select this serde over the Avro and Protobuf legs.

[RAIL_LAW]:
- Package: `Confluent.SchemaRegistry.Serdes.Json`
- Owns: registry-governed JSON-Schema encode/decode on the Kafka value/key slot — `NJsonSchema` generation/registration, server-side `Validate`, `SubjectNameStrategy` derivation, and rule-engine field transforms ordered around validation.
- Accept: a build-time `JsonSerializer<T>`/`JsonDeserializer<T>` over a `class` `T` on the shared `ISchemaRegistryClient`, `AutoRegisterSchemas = false` with out-of-band governed registration, `Validate = true` with `ValidateBeforeDomainRules` under CSFLE, and CSFLE through the shared `RuleRegistry`.
- Reject: a per-message serde instance, `AutoRegisterSchemas` on the durable changefeed producer, `Validate = false` on an external-integration topic, configuring the serde as System.Text.Json, and conflating this body codec with the `CloudNative.CloudEvents.SystemTextJson` envelope formatter.
