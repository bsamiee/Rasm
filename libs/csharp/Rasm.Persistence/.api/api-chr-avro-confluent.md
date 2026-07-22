# [RASM_PERSISTENCE_API_CHR_AVRO_CONFLUENT]

`Chr.Avro.Confluent` frames a `Chr.Avro`-derived Avro body against the Confluent Schema Registry: `ProducerBuilderExtensions`/`ConsumerBuilderExtensions` slot `ISerializer<T>`/`IDeserializer<T>` codecs onto `Confluent.Kafka`'s builders, and the `SchemaRegistrySerializerBuilder`/`SchemaRegistryDeserializerBuilder` facades build them standalone. Each serde emits Confluent wire-format-1 over the `Chr.Avro.Binary` compiled body. This leg feeds the off-Kafka registry-governed Avro rail; the Kafka slot binds the first-party `Confluent.SchemaRegistry.Serdes.Avro`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Chr.Avro.Confluent`
- package: `Chr.Avro.Confluent` (MIT)
- assembly: `Chr.Avro.Confluent`
- namespace: `Chr.Avro.Confluent`
- bound asset: `lib/net8.0` — package multi-targets `net6.0`/`net8.0`/`netstandard2.0` with no `net10.0` asset, so the `net10.0` consumer binds the `net8.0` surface
- managed: pure-managed AnyCPU, no native asset
- depends: `Chr.Avro.Binary` (body codec), `Chr.Avro.Json` (schema JSON for registration), `Confluent.Kafka` (`api-kafka`), `Confluent.SchemaRegistry` (REST registry client)
- rail: avro-registry-serde

## [02]-[PUBLIC_TYPES]

[SERDE_FACADE_TYPES]: registry serde builders and async serializers

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :----------------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `SchemaRegistrySerializerBuilder`    | class         | builds `ISerializer<T>` bound to a registry subject/id           |
|  [02]   | `SchemaRegistryDeserializerBuilder`  | class         | builds `IDeserializer<T>` resolving by id/subject/version        |
|  [03]   | `ISchemaRegistrySerializerBuilder`   | interface     | `Build<T>(...) -> Task<ISerializer<T>>` factory contract         |
|  [04]   | `ISchemaRegistryDeserializerBuilder` | interface     | `Build<T>(...) -> Task<IDeserializer<T>>` factory contract       |
|  [05]   | `AsyncSchemaRegistrySerializer<T>`   | class         | `IAsyncSerializer<T>`; late-binds/registers per `Serialize`      |
|  [06]   | `AsyncSchemaRegistryDeserializer<T>` | class         | `IAsyncDeserializer<T>`; resolves schema-id from the wire prefix |

- `SchemaRegistrySerializerBuilder` (`IDisposable`) composes `ISchemaRegistryClient`, `ISchemaBuilder` (`api-chr-avro`), `IJsonSchemaReader`/`IJsonSchemaWriter` (`Chr.Avro.Json`), `IBinarySerializerBuilder` (`api-chr-avro-binary`); constructed from a client or a raw registry config.
- `SchemaRegistryDeserializerBuilder` (`IDisposable`) composes `ISchemaRegistryClient`, `IJsonSchemaReader`, `IBinaryDeserializerBuilder`.
- `AsyncSchemaRegistrySerializer<T>` (`: IAsyncSerializer<T>, IDisposable`) `SerializeAsync(T, SerializationContext) -> Task<byte[]>` late-binds and per-subject-caches the compiled `Func<T, byte[]>` on first serialize; both ctor forms default `registerAutomatically = Never`, `tombstoneBehavior = None`.
- `AsyncSchemaRegistryDeserializer<T>` (`: IAsyncDeserializer<T>, IDisposable`) `DeserializeAsync(ReadOnlyMemory<byte>, bool isNull, SerializationContext) -> Task<T>` reads the wire-prefix schema id, per-id-caches the compiled `Func<ReadOnlyMemory<byte>, T>`, and maps a null payload to the default `T` tombstone under `TombstoneBehavior.Strict`.

[SERDE_POLICY_TYPES]: registration and tombstone behavior

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :------------------------------ | :------------ | :---------------------------------- |
|  [01]   | `AutomaticRegistrationBehavior` | enum          | `Never` (fail-closed) / `Always`    |
|  [02]   | `TombstoneBehavior`             | enum          | `None` / `Strict` null-value policy |

[CODEC_INTERNALS_TYPES]: per-id codec adapters and wire-format rewriters, `internal` to the assembly

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :------------------------------------ | :------------ | :-------------------------------------------------------------- |
|  [01]   | `DelegateSerializer<T>`               | class         | per-id `ISerializer<T>` wrapping a write delegate + `SchemaId`  |
|  [02]   | `DelegateDeserializer<T>`             | class         | per-id `IDeserializer<T>` wrapping a read delegate + `SchemaId` |
|  [03]   | `WireFormatBytesSerializerRewriter`   | class         | frames body-codec output as `0x00` + big-endian schema id       |
|  [04]   | `WireFormatBytesDeserializerRewriter` | class         | strips/reads the `0x00` + schema-id prefix before body decode   |

- `Build<T>` and the `SetAvro*` extensions hand back the `ISerializer<T>`/`IDeserializer<T>`/`IAsyncSerializer<T>`/`IAsyncDeserializer<T>` interface; a consumer never names these four. They mark where the `Chr.Avro.Binary` body codec (`api-chr-avro-binary`) and the wire-format-1 prefix machinery live.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Kafka producer builder integration (`ProducerBuilderExtensions`)
- Every `SetAvro{Key,Value}Serializer` overload extends `ProducerBuilder<TKey, TValue>` and `DependentProducerBuilder`, takes `ISchemaRegistryClient` or a raw registry config first, and carries `registerAutomatically`, `subjectNameBuilder`, and `tombstoneBehavior` policy args; `await` marks a build-time registry round-trip.

| [INDEX] | [SURFACE]                                                          | [SHAPE] | [CAPABILITY]                                        |
| :-----: | :----------------------------------------------------------------- | :------ | :-------------------------------------------------- |
|  [01]   | `SetAvroKeySerializer(…, subjectNameBuilder)`                      | static  | late-binding key serializer, on `Serialize`         |
|  [02]   | `SetAvroValueSerializer(…, subjectNameBuilder, tombstoneBehavior)` | static  | late-binding value serializer, tombstone policy     |
|  [03]   | `await SetAvroKeySerializer(…, int id)`                            | static  | binds the key serializer to a fixed schema id       |
|  [04]   | `await SetAvroValueSerializer(…, string subject, …)`               | static  | binds the value to a subject (latest)               |
|  [05]   | `await SetAvroValueSerializer(…, string subject, int version)`     | static  | binds to an exact subject+version                   |
|  [06]   | overloads taking `IEnumerable<KeyValuePair<string,string>>`        | static  | raw registry config instead of a client             |
|  [07]   | overloads taking `ISchemaRegistrySerializerBuilder`                | static  | binds against a pre-built/shared serializer-builder |

[ENTRYPOINT_SCOPE]: Kafka consumer builder integration (`ConsumerBuilderExtensions`)
- Every `SetAvro{Key,Value}Deserializer` overload extends `ConsumerBuilder<TKey, TValue>`, takes `ISchemaRegistryClient` or a raw registry config first, and carries a `tombstoneBehavior` arg; `await` marks a build-time registry round-trip.

| [INDEX] | [SURFACE]                                                        | [SHAPE] | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------------------------- | :------ | :---------------------------------------------- |
|  [01]   | `SetAvroKeyDeserializer(…)`                                      | static  | wire-prefix-resolving Avro key deserializer     |
|  [02]   | `SetAvroValueDeserializer(…, tombstoneBehavior)`                 | static  | value deserializer with tombstone policy        |
|  [03]   | `await SetAvroKeyDeserializer(…, int id)`                        | static  | binds the key deserializer to a fixed schema id |
|  [04]   | `await SetAvroValueDeserializer(…, string subject)`              | static  | binds the reader schema to a subject (latest)   |
|  [05]   | `await SetAvroValueDeserializer(…, string subject, int version)` | static  | binds to an exact subject+version               |
|  [06]   | overloads taking `IEnumerable<KeyValuePair<string,string>>`      | static  | raw-config mirror of each overload              |
|  [07]   | overloads taking `ISchemaRegistryDeserializerBuilder`            | static  | shared-builder mirror of each overload          |

[ENTRYPOINT_SCOPE]: standalone serializer construction (`SchemaRegistrySerializerBuilder`)
- Ctor composes `ISchemaRegistryClient` with the optional sub-builders; every `await Build<T>(…, TombstoneBehavior) -> Task<ISerializer<T>>`.

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `new SchemaRegistrySerializerBuilder(ISchemaRegistryClient, …)`    | ctor     | composes the four sub-builders explicitly         |
|  [02]   | `await Build<T>(string subject, AutomaticRegistrationBehavior)`    | instance | derives `T`'s schema, registers, bound serializer |
|  [03]   | `await Build<T>(int id)`                                           | instance | binds to an existing registry schema id           |
|  [04]   | `await Build<T>(string subject, int version)`                      | instance | binds to an exact subject+version                 |
|  [05]   | `new AsyncSchemaRegistrySerializer<T>(...)` / `.AsSyncOverAsync()` | instance | async serde on Kafka's sync slot (`api-kafka`)    |

[ENTRYPOINT_SCOPE]: standalone deserializer construction (`SchemaRegistryDeserializerBuilder`)
- Ctor composes `ISchemaRegistryClient` with the optional sub-builders; every `await Build<T>(…, TombstoneBehavior) -> Task<IDeserializer<T>>`.

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :---------------------------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `new SchemaRegistryDeserializerBuilder(ISchemaRegistryClient, …)` | ctor     | composes the deserializer sub-builders                |
|  [02]   | `await Build<T>(int id)`                                          | instance | binds the reader schema to an existing registry id    |
|  [03]   | `await Build<T>(string subject)`                                  | instance | binds the reader schema to a subject's latest version |
|  [04]   | `await Build<T>(string subject, int version)`                     | instance | binds the reader schema to an exact subject+version   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Confluent wire-format-1 frames each body: a `0x00` magic byte, a big-endian 4-byte schema id, then the `Chr.Avro.Binary` Avro binary body. `WireFormatBytesSerializerRewriter` wraps the body-codec output; `WireFormatBytesDeserializerRewriter` reads the id, the deserializer fetches the writer schema from the registry, resolves it against the reader schema, and decodes through the compiled body delegate.
- Schema resolution is four-way per overload: `(subject, registerAutomatically)` derives and registers/looks-up `T`'s latest schema, `(id)` binds an existing id, `(subject)`/`(subject, version)` binds a registry subject's latest or exact version. Registration and lookup are build-time REST round-trips, so the `id`/`subject`/`subject+version` overloads are async while the sync extension late-binds on the first `Serialize`.
- `SchemaRegistrySerializerBuilder.Build<T>` is the composition root: `SchemaBuilder.BuildSchema<T>()` (`api-chr-avro`), then `SchemaWriter.Write` to registry JSON (`Chr.Avro.Json`), then register/resolve the subject on `RegistryClient`, then `SerializerBuilder.BuildDelegateExpression<T>(schema)` compiles the body codec (`api-chr-avro-binary`).
- `AutomaticRegistrationBehavior.Never` fails closed on an unregistered subject; `Always` registers the derived schema under the registry's compatibility policy.
- `TombstoneBehavior.Strict` maps a null value to a Kafka tombstone and requires a nullable-union schema; `None` serializes null as an Avro value — the CDC delete-marker seam.

[STACKING]:
- Owning pages: the wire-format-1 body codec lands on the `Element/codec` interchange serde and the off-Kafka registry-governed wire rides `Version/egress`.
- `Chr.Avro` (schema from CLR type) + `Chr.Avro.Json` (schema → registry JSON) + `Chr.Avro.Binary` (compiled body codec) + `Confluent.SchemaRegistry` (subject governance) + `Confluent.Kafka` (transport) collapse into one `producerBuilder.SetAvroValueSerializer(registryClient, subject, AutomaticRegistrationBehavior.Never, TombstoneBehavior.Strict)`; the builder owns the schema lifecycle and the magic-byte prefix.
- `Confluent.Kafka`(`api-kafka`): `SetAvroValueSerializer` returns the same `ProducerBuilder<TKey, TValue>` whose `Build()` yields `IProducer<TKey, TValue>`, the resulting `ISerializer<T>` occupying the value-serializer slot.
- `Confluent.Kafka`(`api-kafka`) sync slot: `AsyncSchemaRegistrySerializer<T>` is an `IAsyncSerializer<T>`, and `SyncOverAsync.AsSyncOverAsync()` mounts it on the producer's synchronous `Serialize` slot so the build-time registry round-trip rides the async codec.
- `Confluent.SchemaRegistry` subject naming: `Func<SerializationContext, string> subjectNameBuilder` binds the subject to the Kafka `SerializationContext` (topic + component, `api-kafka`), making topic-name vs record-name vs topic-record-name a policy function; `ConstructValueSubjectName`/`ConstructKeySubjectName` are the defaults.
- `Chr.Avro`(`api-chr-avro`) evolution: the registry enforces the subject's compatibility level on `RegisterSchemaAsync`, and the deserializer's reader-schema resolution (`RecordField.Default`/`NamedSchema.Aliases`) lets a consumer pinned to an older reader schema decode a newer writer-schema message.

[LOCAL_ADMISSION]:
- Admitted for the off-Kafka registry-governed Avro path only — a wire-format-1 body on an object-store or non-Kafka transport, framed by the `Chr.Avro.Binary` expression-compiled codec. On that path the serde is a codec-plus-governance leg, not public Persistence vocabulary; subject, registration behavior, and tombstone policy are profile data.
- `AutomaticRegistrationBehavior.Never` is the production default: registration is an explicit governance act, never an implicit side effect of the first produce.
- A serde is built once per `(type, subject)` and cached with its compiled body delegate; the registry REST round-trip is a build-time cost, never per-message.
- `TombstoneBehavior.Strict` on a compacted/CDC topic makes a delete a real tombstone; the choice is recorded in the egress sink receipt.

[RAIL_LAW]:
- Package: `Chr.Avro.Confluent`
- Owns: the off-Kafka Schema-Registry-governed Avro serde — wire-format-1 framing over the `Chr.Avro.Binary` expression-compiled body codec, four-way schema resolution, and subject-keyed evolution.
- Defers: the Kafka value/key serde slot to `Confluent.SchemaRegistry.Serdes.Avro` (`api-schemaregistry-serdes-avro`), which owns `IAsyncSerializer<T>`/`IAsyncDeserializer<T>` on the `Confluent.Kafka` slot, the shared `CachedSchemaRegistryClient`, and the `RuleRegistry` CSFLE pass; both legs emit identical wire-format-1, so one payload is framed by exactly one and the `SetAvro*` builder extensions here are the rejected duplicate on the Kafka backbone.
- Accept: `Build<T>` facade construction on the off-Kafka path, `Never`-default registration, `Strict`-tombstone CDC nulls.
- Reject: hand-assembled magic-byte/schema-id prefixes, per-message registry round-trips, implicit auto-registration on the durable path, and mounting the `SetAvro*` extensions on a Kafka producer/consumer where the first-party serde owns the slot.
