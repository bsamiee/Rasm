# [RASM_PERSISTENCE_API_SCHEMAREGISTRY_SERDES_JSON]

`Confluent.SchemaRegistry.Serdes.Json` is the registry-governed JSON-Schema data-plane codec: `JsonSerializer<T>` and `JsonDeserializer<T>` derive from the shared `AsyncSerializer<T, JsonSchema>`/`AsyncDeserializer<T, JsonSchema>` base, frame each payload with a Confluent schema id, and validate the JSON document against the registered `NJsonSchema.JsonSchema` on both write and read. `T` is constrained `where T : class`; its JSON Schema is generated from the type via `NJsonSchema.NewtonsoftJson.Generation.NewtonsoftJsonSchemaGeneratorSettings` (the serde's document codec is `Newtonsoft.Json`, never System.Text.Json) or supplied explicitly through a `Schema`-bound constructor. It is the human-readable, server-side-validated leg of the `Version/egress#EGRESS_SINK` rail — categorically distinct from the `CloudNative.CloudEvents.SystemTextJson` envelope formatter, which frames the CloudEvents envelope, not the registry-governed payload body.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Confluent.SchemaRegistry.Serdes.Json`
- package: `Confluent.SchemaRegistry.Serdes.Json`
- license: Apache-2.0
- assembly: `Confluent.SchemaRegistry.Serdes.Json`
- namespace: `Confluent.SchemaRegistry.Serdes`
- target: multi-target (`net462`, `net6.0`, `net8.0`, `netstandard2.0`); the `net10.0` consumer binds `lib/net8.0`
- managed: pure-managed AnyCPU, no native asset
- composes: `Confluent.SchemaRegistry` (registry client, `SchemaId` framing, `RuleRegistry`), `Confluent.Kafka` (the `IAsyncSerializer<T>`/`IAsyncDeserializer<T>`/`SerializationContext` codec contract), and the `NJsonSchema` chain (`NJsonSchema.JsonSchema`, `NJsonSchema.NewtonsoftJson.Generation.NewtonsoftJsonSchemaGeneratorSettings`, `NJsonSchema.Validation` + `Newtonsoft.Json` document codec)
- rail: cdc-egress (JSON Schema)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: serde family
- rail: cdc-egress (JSON Schema)

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]       | [RAIL]                                                    |
| :-----: | :----------------------- | :------------------ | :-------------------------------------------------------- |
|  [01]   | `JsonSerializer<T>`      | async serializer    | `where T : class`; generates schema, validates, frames id |
|  [02]   | `JsonDeserializer<T>`    | async deserializer  | `where T : class`; resolves id, validates, parses         |
|  [03]   | `JsonSerializerConfig`   | serializer config   | register/normalize/validate/subject/id-strategy           |
|  [04]   | `JsonDeserializerConfig` | deserializer config | latest-version + validate + id-strategy                   |

[PUBLIC_TYPE_SCOPE]: composed contract family (re-stated from siblings)
- rail: cdc-egress (JSON Schema)

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]   | [ORIGIN]                                                                   |
| :-----: | :---------------------------------------------- | :-------------- | :------------------------------------------------------------------------- |
|  [01]   | `AsyncSerializer<T, JsonSchema>`                | serde base      | `Confluent.SchemaRegistry` — shared encode base (id encoder, buffer)       |
|  [02]   | `AsyncDeserializer<T, JsonSchema>`              | serde base      | `Confluent.SchemaRegistry` — shared decode base                            |
|  [03]   | `IAsyncSerializer<T>` / `IAsyncDeserializer<T>` | codec slot      | `Confluent.Kafka` — the `SetValueSerializer`/`SetValueDeserializer` target |
|  [04]   | `SerializationContext`                          | codec context   | `Confluent.Kafka` — component, topic, headers                              |
|  [05]   | `ISchemaRegistryClient`                         | registry client | `Confluent.SchemaRegistry` — the shared registry leg                       |
|  [06]   | `RuleRegistry`                                  | rule registry   | `Confluent.SchemaRegistry` — CSFLE/migration executors                     |
|  [07]   | `JsonSchema`                                    | json schema     | `NJsonSchema` — the generated/registered draft schema                      |
|  [08]   | `NewtonsoftJsonSchemaGeneratorSettings`         | schema gen      | `NJsonSchema.NewtonsoftJson` — `T` -> `JsonSchema` settings                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction
- rail: cdc-egress (JSON Schema)

| [INDEX] | [SURFACE]                                                                                     | [ENTRY_FAMILY] | [RAIL]                                           |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `new JsonSerializer<T>(client, config?, jsonSchemaGeneratorSettings?, ruleRegistry?)`         | ctor           | generates the schema from `T` via NJsonSchema    |
|  [02]   | `new JsonSerializer<T>(client, schema, config?, jsonSchemaGeneratorSettings?, ruleRegistry?)` | ctor           | uses an explicit registered `Schema`             |
|  [03]   | `new JsonDeserializer<T>(client, config, jsonSchemaGeneratorSettings?, ruleRegistry?)`        | ctor           | registry + typed config                          |
|  [04]   | `new JsonDeserializer<T>(client, schema, config?, jsonSchemaGeneratorSettings?)`              | ctor           | explicit reader `Schema`                         |
|  [05]   | `new JsonDeserializer<T>(config?, jsonSchemaGeneratorSettings?)`                              | ctor           | registry-less decode (id framing only, no fetch) |

[ENTRYPOINT_SCOPE]: codec invocation (Confluent.Kafka slot)
- rail: cdc-egress (JSON Schema)

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                                                                               |
| :-----: | :------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `SerializeAsync(value, context)` -> `Task<byte[]>`       | encode         | generates/registers the schema, validates the document, frames id, writes UTF-8 JSON |
|  [02]   | `DeserializeAsync(data, isNull, context)` -> `Task<T>`   | decode         | reads id, validates against the writer schema, parses into `T`                       |
|  [03]   | `producerBuilder.SetValueSerializer(jsonSerializer)`     | wiring         | mounts the serde on the `Confluent.Kafka` value slot                                 |
|  [04]   | `consumerBuilder.SetValueDeserializer(jsonDeserializer)` | wiring         | mounts the serde on the consumer value slot                                          |

[ENTRYPOINT_SCOPE]: config tunables (`JsonSerializerConfig` / `JsonDeserializerConfig`)
- rail: cdc-egress (JSON Schema)

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY] | [RAIL]                                                                         |
| :-----: | :----------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `AutoRegisterSchemas` (`bool?`)                                                      | serializer     | auto-register the generated schema (production: `false`)                       |
|  [02]   | `NormalizeSchemas` (`bool?`)                                                         | serializer     | canonical-form normalization                                                   |
|  [03]   | `UseLatestVersion` (`bool?`)                                                         | both           | pin the latest registered schema version                                       |
|  [04]   | `UseSchemaId` (`int?`)                                                               | serializer     | force a specific registered schema id                                          |
|  [05]   | `BufferBytes` (`int?`)                                                               | serializer     | initial serialize buffer override                                              |
|  [06]   | `Validate` (`bool?`)                                                                 | both           | validate the JSON document against the schema (the JSON-Schema-distinct guard) |
|  [07]   | `ValidateBeforeDomainRules` (`bool?`)                                                | both           | run schema validation ahead of the CSFLE/domain rules                          |
|  [08]   | `LatestCompatibilityStrict` (`bool?`)                                                | serializer     | strict compatibility check against the latest version                          |
|  [09]   | `SubjectNameStrategy` (`SubjectNameStrategy?`)                                       | both           | `Topic`/`Record`/`TopicRecord` subject derivation                              |
|  [10]   | `SchemaIdStrategy` (`SchemaIdSerializerStrategy?` / `SchemaIdDeserializerStrategy?`) | both           | `Prefix` vs. `Header` / `Dual` id framing                                      |
|  [11]   | `UseLatestWithMetadata` (`IDictionary<string,string>`)                               | both           | pin the schema version whose `Metadata` matches                                |

## [04]-[IMPLEMENTATION_LAW]

[JSON_SERDE_TOPOLOGY]:
- `JsonSerializer<T> : AsyncSerializer<T, JsonSchema> where T : class` and `JsonDeserializer<T> : AsyncDeserializer<T, JsonSchema> where T : class` — the parsed-schema type is `NJsonSchema.JsonSchema`. The shared `AsyncSerializer<T,TParsed>` base owns `autoRegisterSchema`, `normalizeSchemas`, the `ISchemaIdEncoder` (default `PrefixSchemaIdEncoder`), and `initialBufferSize`; this serde supplies JSON Schema generation, document validation, and the JSON read/write.
- the document codec is `Newtonsoft.Json` and the schema engine is `NJsonSchema 11.0.2`: the schemaless `JsonSerializer<T>` ctor generates the `JsonSchema` from `T` through `NewtonsoftJsonSchemaGeneratorSettings`; the `Schema`-bound ctor uses a registered schema verbatim. This is structurally a different JSON stack from the admitted System.Text.Json surfaces (`NodaTime.Serialization.SystemTextJson`, `CloudNative.CloudEvents.SystemTextJson`) — a System.Text.Json `JsonSerializerOptions` never configures this serde.
- the wire payload is the framed schema id plus the UTF-8 JSON document. `Validate` gates both directions on JSON-Schema conformance: on write the document is validated against the schema before framing; on read the decoded document is validated against the writer schema. `ValidateBeforeDomainRules` orders validation ahead of the rule-engine domain transforms so an invalid document is rejected before a field-encryption rule runs.
- `DeserializeAsync(ReadOnlyMemory<byte> data, bool isNull, SerializationContext context)` parses the JSON into `T`; `isNull` returns the tombstone path. The registry-less ctor (`new JsonDeserializer<T>(config?)`) decodes id framing only without a registry fetch — admitted only where validation/evolution is not enforced.

[LOCAL_ADMISSION]:
[JSON_SERDE_EGRESS]:
The JSON serde is the human-readable, externally-consumable leg of the changefeed egress: built once per stream, mounted on the `Confluent.Kafka` `SetValueSerializer`/`SetValueDeserializer` slot, sharing the single `CachedSchemaRegistryClient` with the Avro and Protobuf serdes. The codec is fixed at build, never per call. It is chosen for topics whose downstream consumers want self-describing JSON under registry-enforced schema.
- `T` is the projection record of the op event (a `class`, satisfying the constraint); the generated `JsonSchema` is registered out-of-band so the serde never auto-evolves the contract. Production sets `AutoRegisterSchemas = false` and `Validate = true`, registering the schema under a `Backward`/`FullTransitive` `Compatibility` level so an invalid or incompatible document is rejected at the codec rather than reaching a consumer. `LatestCompatibilityStrict = true` hardens the producer against publishing under a stale schema.
- Field-level encryption rides the shared `RuleRegistry`: a `DomainRule` of `RuleMode.WriteRead` naming a field-encryption `IRuleExecutor` (JSON string fields tagged in the schema `Metadata` sensitive-field set, validated first when `ValidateBeforeDomainRules = true`) wraps/unwraps per-field DEKs against the `Element/identity#KEY_ENVELOPE` `EnvelopeKeyring` KMS clients (the `KmsProvider` axis owned at `Element/identity#KMS_CUSTODY`) — the same rule engine the Avro and Protobuf serdes use.
- This serde and the CloudEvents JSON formatter occupy orthogonal slots and never compete: `CloudNative.CloudEvents.SystemTextJson` (`JsonEventFormatter`) frames the CloudEvents envelope structure (`id`/`source`/`type`/`time` plus attributes) that rides the `Message<TKey, TValue>` headers/body, while this serde is the registry-governed codec for the payload body `Data`. The README admission "distinct from the CloudEvents JSON envelope formatter" is exactly this split — the envelope is CloudEvents+STJ, the validated body is this serde. `api-cloudevents` records the reciprocal seam from the envelope side.
- the schema-id header co-exists with the CloudEvents attribute headers on one `Message<TKey, TValue>.Headers` with no key collision: `SchemaIdStrategy = SchemaIdSerializerStrategy.Header` frames the id into the Confluent `__value_schema_id` / `__key_schema_id` header (`SchemaId.VALUE_SCHEMA_ID_HEADER` / `KEY_SCHEMA_ID_HEADER` via `HeaderSchemaIdEncoder` writing `SerializationContext.Headers`), disjoint from the `ce_`-prefixed envelope headers and the `content-type` header the CloudEvents `ToKafkaMessage` writes; `SchemaIdSerializerStrategy.Prefix` (the default) instead prefixes the body with the magic byte plus id and leaves the header bag to CloudEvents alone. The deserializer mirror `SchemaIdDeserializerStrategy.Dual` reads the id from either the header or the body prefix, so a `Header`-framed producer and a `Prefix` reader interoperate.

[RAIL_LAW]:
- Package: `Confluent.SchemaRegistry.Serdes.Json`
- Owns: registry-governed JSON-Schema encode/decode on the Kafka value/key slot — `NJsonSchema` schema generation/registration, server-side document validation (`Validate`), `SubjectNameStrategy` subject derivation, and rule-engine field transforms ordered around validation.
- Accept: a build-time `JsonSerializer<T>`/`JsonDeserializer<T>` over a `class` `T` and the shared `ISchemaRegistryClient`, `AutoRegisterSchemas = false` with out-of-band governed registration, `Validate = true` (plus `ValidateBeforeDomainRules` when CSFLE is active), and CSFLE through the shared `RuleRegistry`.
- Reject: a per-message serde instance, `AutoRegisterSchemas` on the durable changefeed producer, `Validate = false` on an external-integration topic, configuring the serde as if it were System.Text.Json, and conflating this payload codec with the `CloudNative.CloudEvents.SystemTextJson` envelope formatter.
