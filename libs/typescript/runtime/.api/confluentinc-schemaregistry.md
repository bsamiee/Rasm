# [TS_RUNTIME_API_CONFLUENTINC_SCHEMAREGISTRY]

`@confluentinc/schemaregistry` binds Confluent Schema Registry to Avro, Protobuf, and JSON Schema codecs. Runtime compiles each Kafka contract into one subject, exact schema identity, compatibility policy, rule registry, and matched key/value serde pair before the broker row becomes ready.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: registry identity, policy, codecs, framing, and rule execution

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :-------------------------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `SchemaRegistryClient` / `Client`             | client        | cached registry REST surface and lifecycle                      |
|  [02]   | `ClientConfig`                                | config        | base URLs, auth, Axios defaults, retry, and cache policy        |
|  [03]   | `SchemaInfo` / `SchemaMetadata`               | schema value  | schema text, references, metadata, rules, ID, GUID, and version |
|  [04]   | `Compatibility`                               | policy enum   | subject compatibility levels, including transitive modes        |
|  [05]   | `SerdeType.KEY` / `SerdeType.VALUE`           | role enum     | key/value subject and frame role                                |
|  [06]   | `SerializerConfig` / `DeserializerConfig`     | codec config  | identity selection, caches, subject strategy, and frame hooks   |
|  [07]   | `Serializer` / `Deserializer`                 | codec base    | family-neutral async encode/decode surface                      |
|  [08]   | `AvroSerializer` / `AvroDeserializer`         | codec pair    | Avro schema derivation, rules, migration, and framing           |
|  [09]   | `ProtobufSerializer` / `ProtobufDeserializer` | codec pair    | descriptor references, message indexes, rules, and framing      |
|  [10]   | `JsonSerializer` / `JsonDeserializer`         | codec pair    | JSON Schema validation, rules, migration, and framing           |
|  [11]   | `SubjectNameStrategyType`                     | policy enum   | associated, topic, record, or topic-record derivation           |
|  [12]   | `SubjectNameStrategyFunc`                     | policy        | custom sync or async subject derivation                         |
|  [13]   | `SchemaId`                                    | frame value   | numeric ID or GUID plus Protobuf message indexes                |
|  [14]   | `RuleRegistry` / `RuleSet` / `Rule`           | rule engine   | executor, action, override, and schema-carried rule roster      |
|  [15]   | `RestError` / `SerializationError`            | fault         | registry protocol or codec/framing failure                      |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client admission, exact identity, matched codecs, and frame inspection

| [INDEX] | [SURFACE]                                                            | [SHAPE]   | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------- | :-------- | :---------------------------------------- |
|  [01]   | `new SchemaRegistryClient(ClientConfig)`                             | ctor      | scoped client with bounded caches         |
|  [02]   | `register(string, SchemaInfo, boolean?)`                             | promise   | register and return numeric ID            |
|  [03]   | `registerFullResponse(string, SchemaInfo, boolean?)`                 | promise   | register and return metadata              |
|  [04]   | `getId(string, SchemaInfo, boolean?)`                                | promise   | resolve existing schema identity          |
|  [05]   | `getVersion(string, SchemaInfo, boolean?, boolean?)`                 | promise   | resolve exact subject version             |
|  [06]   | `getSchemaMetadata(string, number, boolean?, string?)`               | promise   | read one subject generation               |
|  [07]   | `getLatestSchemaMetadata(string, string?)`                           | promise   | read the subject head                     |
|  [08]   | `getBySubjectAndId(string, number, string?)`                         | promise   | resolve a numeric writer schema           |
|  [09]   | `getByGuid(string, string?)`                                         | promise   | resolve a GUID writer schema              |
|  [10]   | `testSubjectCompatibility(string, SchemaInfo)`                       | promise   | prove candidate compatibility             |
|  [11]   | `getCompatibility(string)` / `updateCompatibility(string, enum)`     | promise   | read or set subject policy                |
|  [12]   | `new <Family>Serializer(Client, SerdeType, Config, RuleRegistry?)`   | ctor      | construct one family encoder              |
|  [13]   | `serialize(string, unknown, IHeaders?) -> Promise<Buffer>`           | instance  | execute write rules and frame             |
|  [14]   | `new <Family>Deserializer(Client, SerdeType, Config, RuleRegistry?)` | ctor      | construct one family decoder              |
|  [15]   | `deserialize(string, Buffer, IHeaders?) -> Promise<unknown>`         | instance  | resolve, migrate, execute rules, decode   |
|  [16]   | `RuleRegistry.registerExecutor` / `registerAction`                   | instance  | install schema-rule implementations       |
|  [17]   | `new SchemaId(string).fromBytes(Buffer)`                             | frame     | read ID or GUID plus message indexes      |
|  [18]   | `client.clearCaches()` / `client.close()`                            | lifecycle | invalidate evidence and release transport |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One descriptor derives subject, `SerdeType`, schema family, `SchemaInfo`, normalization, compatibility, rules, ID, and version.
- Boot proves rules, subject policy, compatibility, and exact identity before codec construction.
- `SchemaRegistryClient` owns mutex-guarded ID, GUID, schema, latest, version, and metadata caches.
- Cache misses call REST; neither `useSchemaId` nor deserialization creates an offline path.
- Default framing writes magic-byte-v0 and numeric ID before payload bytes; Protobuf adds message indexes.
- Explicit `HeaderSchemaIdSerializer` selection moves magic-byte-v1 and GUID into key/value headers.
- `DualSchemaIdDeserializer` accepts header or payload framing.

[STACKING]:
- `@confluentinc/kafka-javascript`: `Serializer.serialize` supplies `ProducerRecord.messages[].value`.
- `Deserializer.deserialize` consumes delivered `message.value` and shared `IHeaders`; framing encloses only the broker payload.
- `effect`: `SchemaRegistryClient` rides `Effect.acquireRelease`; registry and codec promises lift through `Effect.tryPromise`.
- `RestError.status` and `errorCode` classify the typed registry fault.
- within-lib: one client feeds matched Avro, Protobuf, or JSON codec constructors.
- Both directions share `SerdeType`, explicit `SubjectNameStrategyFunc`, and `RuleRegistry`.

[LOCAL_ADMISSION]:
- Registration belongs to the boot control plane.
- Message codecs disable auto-registration, bind admitted `useSchemaId`, and use one explicit subject function.
- Prewarm `getBySubjectAndId(subject, id)` for every admitted numeric frame.
- `useSchemaId`, deserializer cache misses, and `getIdFullResponse` retain registry calls.
- Keep one client and one matched codec pair per admitted contract family and key/value role.
- Descriptor policy owns cache capacity and latest TTL.
- Supply one explicit `RuleRegistry`; admit every required executor and action before codec construction.
- Decode codec output through contract `Schema.decodeUnknown`; framing proves writer identity, not interior domain type.
