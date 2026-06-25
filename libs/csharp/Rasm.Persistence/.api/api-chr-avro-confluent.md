# [RASM_PERSISTENCE_API_CHR_AVRO_CONFLUENT]

`Chr.Avro.Confluent` is the Confluent Schema Registry serde leg of `Chr.Avro` — the package that stacks the Avro schema model (`api-chr-avro`) and binary codec (`api-chr-avro-binary`) directly onto the `Confluent.Kafka` client (`api-kafka`) and the `Confluent.SchemaRegistry` REST client. Its load-bearing surface is the `ProducerBuilderExtensions`/`ConsumerBuilderExtensions` fluent extensions on `Confluent.Kafka`'s `ProducerBuilder<TKey, TValue>`/`ConsumerBuilder<TKey, TValue>`, and the `SchemaRegistrySerializerBuilder`/`SchemaRegistryDeserializerBuilder` facades producing the `ISerializer<T>`/`IDeserializer<T>` (and async `IAsyncSerializer<T>`/`IAsyncDeserializer<T>`) codecs Kafka's builder slots accept. Every serde emits Confluent wire-format-1 (a `0x00` magic byte + big-endian 4-byte schema id + Avro binary body), resolves the schema four ways (latest-with-auto-register, by id, by subject, by subject+version), and governs subject-keyed schema evolution. This is the second Avro-over-Schema-Registry serde leg the workspace admits, and it is the NON-CANONICAL one: the canonical Kafka-wire Avro serde is `Confluent.SchemaRegistry.Serdes.Avro` (`api-schemaregistry-serdes-avro`), which mounts directly on the first-party `Confluent.Kafka` `IAsyncSerializer<T>`/`IAsyncDeserializer<T>` slot, shares the single `CachedSchemaRegistryClient` with the Protobuf/Json serdes, and carries the `RuleRegistry` CSFLE/migration executors. Both legs emit identical wire-format-1 and govern subject evolution, so per `one canonical semantic name per bounded concept` they never both frame the same payload — the Kafka slot belongs to the Confluent serde, and this package is admitted ONLY for the path that serde cannot reach: the `Chr.Avro`-derived expression-compiled body codec (`api-chr-avro-binary`) over a CLR-derived schema model (`api-chr-avro`), against the `0x00`-prefixed wire-format-1 envelope, when an Avro body must be produced/consumed OUTSIDE the Kafka serde slot (a registry-governed Avro blob on an object-store or a non-Kafka transport). On the Kafka backbone itself this package is the rejected alternative.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Chr.Avro.Confluent`
- package: `Chr.Avro.Confluent`
- version: `10.13.1`
- license: MIT
- assembly: `Chr.Avro.Confluent`
- namespace: `Chr.Avro.Confluent`
- bound asset: `lib/net8.0` (consumer-bound; package multi-targets `net6.0`/`net8.0`/`netstandard2.0` — there is NO `net10.0` asset, so the `net10.0` consumer binds the `net8.0` surface)
- dependencies: `Chr.Avro.Binary` `10.13.1` (the body codec), `Chr.Avro.Json` `10.13.1` (the schema JSON read/write for registration), `Confluent.Kafka` `[2.14.0, 2.15.0)` (`api-kafka`), `Confluent.SchemaRegistry` `[2.14.0, 2.15.0)` (the REST client); the workspace co-pins the Confluent line at `2.14.2`. Pure-managed AnyCPU
- rail: avro-registry-serde

## [02]-[PUBLIC_TYPES]

[SERDE_FACADE_TYPES]: registry serde builders and async serializers
- rail: avro-registry-serde

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]        | [CAPABILITY]                                       |
| :-----: | :-------------------------------- | :-------------------- | :------------------------------------------------- |
|  [01]   | `SchemaRegistrySerializerBuilder` | serializer factory    | builds `ISerializer<T>` bound to a registry subject/id |
|  [02]   | `SchemaRegistryDeserializerBuilder` | deserializer factory | builds `IDeserializer<T>` resolving by id/subject/version |
|  [03]   | `ISchemaRegistrySerializerBuilder` | factory contract     | `Build<T>(...)` → `Task<ISerializer<T>>`           |
|  [04]   | `ISchemaRegistryDeserializerBuilder` | factory contract   | `Build<T>(...)` → `Task<IDeserializer<T>>`         |
|  [05]   | `AsyncSchemaRegistrySerializer<T>` | async serializer     | `IAsyncSerializer<T>` — late-binds/registers per `Serialize` |
|  [06]   | `AsyncSchemaRegistryDeserializer<T>` | async deserializer | `IAsyncDeserializer<T>` — resolves schema-id from the wire prefix |

`SchemaRegistrySerializerBuilder` composes (constructor-injectable): `ISchemaRegistryClient RegistryClient`, `ISchemaBuilder SchemaBuilder` (`api-chr-avro`), `IJsonSchemaReader SchemaReader` + `IJsonSchemaWriter SchemaWriter` (`Chr.Avro.Json`), `IBinarySerializerBuilder SerializerBuilder` (`api-chr-avro-binary`). Constructed from an `ISchemaRegistryClient` or a raw `IEnumerable<KeyValuePair<string,string>>` registry config; `IDisposable`.
`SchemaRegistryDeserializerBuilder` composes `ISchemaRegistryClient RegistryClient`, `IJsonSchemaReader SchemaReader`, `IBinaryDeserializerBuilder DeserializerBuilder`; `IDisposable`.
`AsyncSchemaRegistrySerializer<T>` (`: IAsyncSerializer<T>, IDisposable`) properties: `RegistryClient`, `RegisterAutomatically` (`AutomaticRegistrationBehavior`), `SchemaBuilder`, `SchemaReader`, `SchemaWriter`, `SerializerBuilder` (`IBinarySerializerBuilder`), `Func<SerializationContext, string> SubjectNameBuilder`, `TombstoneBehavior`; constructed from an `ISchemaRegistryClient` or raw `IEnumerable<KeyValuePair<string,string>>` config (both ctor forms default `registerAutomatically = Never`, `tombstoneBehavior = None`); `Task<byte[]> SerializeAsync(T data, SerializationContext context)` late-binds and per-`subject`-caches the compiled `Func<T, byte[]>` on first `Serialize`.
`AsyncSchemaRegistryDeserializer<T>` (`: IAsyncDeserializer<T>, IDisposable`) properties: `RegistryClient`, `DeserializerBuilder` (`IBinaryDeserializerBuilder`), `SchemaReader`, `TombstoneBehavior`; `Task<T> DeserializeAsync(ReadOnlyMemory<byte> data, bool isNull, SerializationContext context)` reads the wire-prefix schema id, per-id-caches the compiled `Func<ReadOnlyMemory<byte>, T>`, and (with `TombstoneBehavior.Strict`) maps a `null`/empty payload to the default `T` tombstone.

[SERDE_POLICY_TYPES]: registration and tombstone behavior
- rail: avro-registry-serde

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]       | [CAPABILITY]                                       |
| :-----: | :------------------------------ | :------------------- | :------------------------------------------------- |
|  [01]   | `AutomaticRegistrationBehavior` | registration policy  | `Never` (fail if unregistered) / `Always` (register the derived schema) |
|  [02]   | `TombstoneBehavior`             | null-value policy    | `None` (serialize null as Avro) / `Strict` (null → Kafka tombstone, reject non-null-union) |

[CODEC_INTERNALS_TYPES]: the per-id codec adapters and wire-format rewriters (NOT public composition surface)
- rail: avro-registry-serde

| [INDEX] | [SYMBOL]                              | [PACKAGE_ROLE]      | [CAPABILITY]                                       |
| :-----: | :------------------------------------ | :------------------ | :------------------------------------------------- |
|  [01]   | `DelegateSerializer<T>` (`internal`)  | per-id `ISerializer<T>` | wraps a `delegate void Implementation(T value, Stream stream)` + `int SchemaId` + `TombstoneBehavior` — what `Build<T>` returns behind the `ISerializer<T>` interface |
|  [02]   | `DelegateDeserializer<T>` (`internal`) | per-id `IDeserializer<T>` | wraps a `delegate T Implementation(ReadOnlySpan<byte> data)` + `int SchemaId` + `TombstoneBehavior` |
|  [03]   | `WireFormatBytesSerializerRewriter` (`internal`) | wire-format-1 framer | rewrites the body-codec output into the `0x00` + big-endian schema-id envelope |
|  [04]   | `WireFormatBytesDeserializerRewriter` (`internal`) | wire-format-1 unframer | strips/reads the `0x00` + schema-id prefix before the body decode |

These four are `internal` to `Chr.Avro.Confluent` — a consumer never names them. `Build<T>` / the `SetAvro*` extensions hand back the `ISerializer<T>`/`IDeserializer<T>` / `IAsyncSerializer<T>`/`IAsyncDeserializer<T>` interface; the nested `DelegateSerializer<T>.Implementation`/`DelegateDeserializer<T>.Implementation` delegate signatures and the wire-format rewriters are documented only to show where the `Chr.Avro.Binary` body codec (`api-chr-avro-binary`) and the wire-format-1 prefix machinery actually live, not as a composition seam.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Kafka producer builder integration (`ProducerBuilderExtensions`)
- rail: avro-registry-serde

| [INDEX] | [SURFACE]                                                                              | [CALL_SHAPE] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------------------------------- | :----------- | :------------------------------------------------- |
|  [01]   | `producerBuilder.SetAvroKeySerializer(ISchemaRegistryClient, registerAutomatically, subjectNameBuilder)` | sync ext | wires a late-binding Avro key serializer (registers on first `Serialize`) |
|  [02]   | `producerBuilder.SetAvroValueSerializer(ISchemaRegistryClient, registerAutomatically, subjectNameBuilder, tombstoneBehavior)` | sync ext | wires a late-binding Avro value serializer with tombstone policy |
|  [03]   | `await producerBuilder.SetAvroKeySerializer(ISchemaRegistryClient, int id)`            | async ext    | binds the key serializer to a fixed schema id      |
|  [04]   | `await producerBuilder.SetAvroValueSerializer(ISchemaRegistryClient, string subject, AutomaticRegistrationBehavior, TombstoneBehavior)` | async ext | binds the value serializer to a subject (latest, optionally registering) |
|  [05]   | `await producerBuilder.SetAvroValueSerializer(ISchemaRegistryClient, string subject, int version)` | async ext | binds to an exact subject+version |
|  [06]   | overloads taking `IEnumerable<KeyValuePair<string,string>> registryConfiguration`      | ext          | every overload mirrored to take raw registry config instead of a client |
|  [07]   | overloads taking `ISchemaRegistrySerializerBuilder serializerBuilder`                  | async ext    | binds against a pre-built/shared serializer-builder |
|  [08]   | every overload also defined on `DependentProducerBuilder<TKey, TValue>`                | ext          | the `Confluent.Kafka` handle-sharing dependent-builder mirror |

[ENTRYPOINT_SCOPE]: Kafka consumer builder integration (`ConsumerBuilderExtensions`)
- rail: avro-registry-serde

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------------------- | :----------- | :------------------------------------------------- |
|  [01]   | `consumerBuilder.SetAvroKeyDeserializer(ISchemaRegistryClient)`            | sync ext     | wires a wire-prefix-resolving Avro key deserializer |
|  [02]   | `consumerBuilder.SetAvroValueDeserializer(ISchemaRegistryClient, tombstoneBehavior)` | sync ext | wires the value deserializer with tombstone policy |
|  [03]   | `await consumerBuilder.SetAvroKeyDeserializer(ISchemaRegistryClient, int id)` | async ext  | binds the key deserializer to a fixed reader schema id |
|  [04]   | `await consumerBuilder.SetAvroValueDeserializer(ISchemaRegistryClient, string subject, TombstoneBehavior)` | async ext | binds the reader schema to a subject (latest) |
|  [05]   | `await consumerBuilder.SetAvroValueDeserializer(ISchemaRegistryClient, string subject, int version, TombstoneBehavior)` | async ext | binds the reader schema to an exact subject+version |
|  [06]   | overloads taking `IEnumerable<KeyValuePair<string,string>>` / `ISchemaRegistryDeserializerBuilder` | ext | raw-config and shared-builder mirrors of each overload |

[ENTRYPOINT_SCOPE]: standalone serde construction
- rail: avro-registry-serde

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------------------- | :----------- | :------------------------------------------------- |
|  [01]   | `new SchemaRegistrySerializerBuilder(ISchemaRegistryClient, schemaBuilder?, schemaReader?, schemaWriter?, serializerBuilder?)` | ctor | composes the four sub-builders explicitly |
|  [02]   | `await SchemaRegistrySerializerBuilder.Build<T>(string subject, AutomaticRegistrationBehavior, TombstoneBehavior)` → `ISerializer<T>` | build call | derives `T`'s schema, registers, returns a bound serializer |
|  [03]   | `await SchemaRegistrySerializerBuilder.Build<T>(int id, TombstoneBehavior)` → `ISerializer<T>` | build call | binds to an existing registry schema id |
|  [04]   | `await SchemaRegistrySerializerBuilder.Build<T>(string subject, int version, TombstoneBehavior)` → `ISerializer<T>` | build call | binds to an exact subject+version |
|  [05]   | `new SchemaRegistryDeserializerBuilder(ISchemaRegistryClient, deserializerBuilder?, schemaReader?)` | ctor | composes the deserializer sub-builders |
|  [06]   | `await SchemaRegistryDeserializerBuilder.Build<T>(int id, TombstoneBehavior)` → `IDeserializer<T>` | build call | binds the reader schema to an existing registry id |
|  [07]   | `await SchemaRegistryDeserializerBuilder.Build<T>(string subject, TombstoneBehavior)` → `IDeserializer<T>` | build call | binds the reader schema to a subject's latest version |
|  [08]   | `await SchemaRegistryDeserializerBuilder.Build<T>(string subject, int version, TombstoneBehavior)` → `IDeserializer<T>` | build call | binds the reader schema to an exact subject+version |
|  [09]   | `new AsyncSchemaRegistrySerializer<T>(...)` / `.AsSyncOverAsync()`         | adapter      | mount the async serde on Kafka's sync codec slot (`api-kafka`) |

## [04]-[IMPLEMENTATION_LAW]

[AVRO_REGISTRY_TOPOLOGY]:
- namespace: `Chr.Avro.Confluent` — the extensions, the two facades, the two async serde classes, the two policy enums
- wire format: Confluent wire-format-1 — `0x00` magic byte, then a big-endian 4-byte schema id, then the Avro binary body produced by `Chr.Avro.Binary`. The internal `WireFormatBytesSerializerRewriter` wraps the body-codec output in the prefix; `WireFormatBytesDeserializerRewriter` strips/reads the id, the deserializer fetches the corresponding writer schema from the registry, resolves it against the configured reader schema, and decodes through the `Chr.Avro.Binary` compiled body delegate
- schema resolution is four-way and explicit per overload: `(subject, registerAutomatically)` derives `T`'s schema and registers/looks-up latest; `(id)` binds an existing id; `(subject)` / `(subject, version)` binds a registry subject's latest or exact version. The async overloads return `Task<…Builder>` / `Task<ISerializer<T>>` because registration and lookup are REST round-trips at build time
- `SchemaRegistrySerializerBuilder.Build<T>` is the composition root: it calls `SchemaBuilder.BuildSchema<T>()` (`api-chr-avro`), serializes that model to JSON via `SchemaWriter.Write(...)` (`Chr.Avro.Json`), registers/resolves the subject on `RegistryClient`, and compiles the body codec via `SerializerBuilder.BuildDelegateExpression<T>(schema)` (`api-chr-avro-binary`) — four packages folded into one `ISerializer<T>`
- `AutomaticRegistrationBehavior.Never` fails closed when a subject is unregistered (the production-safe default, registration is a governance step); `Always` registers the derived schema, subject to the registry's compatibility policy
- `TombstoneBehavior.Strict` maps a null value to a Kafka tombstone (null payload) and requires the schema to be a nullable union; `None` serializes null as an Avro value — the CDC delete-marker seam
- the sync extensions wire a LATE-binding serializer (schema derived/registered on the first `Serialize` call), while the `int id` / `subject` / `subject, version` overloads are async because they round-trip the registry at build time

[INTEGRATION_LAW]:
- This is the four-package fold made concrete: `Chr.Avro` (schema from CLR type) + `Chr.Avro.Json` (schema → registry JSON) + `Chr.Avro.Binary` (compiled body codec) + `Confluent.SchemaRegistry` (subject governance) + `Confluent.Kafka` (transport) collapse into `producerBuilder.SetAvroValueSerializer(registryClient, subject, AutomaticRegistrationBehavior.Never, TombstoneBehavior.Strict)`. The builder owns the schema lifecycle; the catalog consumer never hand-assembles the magic-byte prefix.
- It CAN plug into the exact Kafka builder slots the transport catalog documents — `SetAvroValueSerializer` returns the same `ProducerBuilder<TKey, TValue>` whose `Build()` yields the `IProducer<TKey, TValue>` (`api-kafka`), the resulting `ISerializer<T>` occupying the value-serializer slot — but on the Kafka backbone that slot is owned by `Confluent.SchemaRegistry.Serdes.Avro` (`api-schemaregistry-serdes-avro`), so these `SetAvro{Key,Value}{Serializer,Deserializer}` extensions are NOT the chosen Kafka egress codec. The canonical Kafka CDC egress chooses, per topic, between the first-party Confluent Avro serde (the schema-governed typed body) and the schema-free `Serializers.ByteArray` + CloudEvents envelope of the existing changefeed path; this package's builder extensions are the rejected duplicate of the former, retained only for an off-Kafka registry-governed Avro body. All are mutually exclusive codec choices, never layered.
- Async-to-sync adapter stacking: `AsyncSchemaRegistrySerializer<T>` is an `IAsyncSerializer<T>`; mounting it on Kafka's synchronous codec slot uses `Confluent.Kafka.SyncOverAsync.AsSyncOverAsync()` (`api-kafka`) — the same adapter the transport catalog documents, so the registry round-trip rides the async codec while the producer's sync `Serialize` path stays satisfied.
- Subject-naming strategy stacks at the edge: the `Func<SerializationContext, string> subjectNameBuilder` parameter binds the registry subject to the Kafka `SerializationContext` (topic + key/value component, `api-kafka`), so a topic-name vs record-name vs topic-record-name subject strategy is a policy function, not a parallel API — and `Confluent.SchemaRegistry`'s `ConstructValueSubjectName`/`ConstructKeySubjectName` are the canonical defaults.
- Schema evolution is governed downstream of this seam: the registry enforces the subject's compatibility level on `RegisterSchemaAsync`, and the deserializer's reader-schema resolution (`RecordField.Default`/`NamedSchema.Aliases`, `api-chr-avro`) is what lets a consumer pinned to an older reader schema decode a newer writer-schema message — Avro's forward/backward compatibility, realized across the registry boundary.

[LOCAL_ADMISSION]:
- The Kafka serde slot is NOT this package's: `Confluent.SchemaRegistry.Serdes.Avro` (`api-schemaregistry-serdes-avro`) is the canonical Kafka-wire Avro owner, so the `SetAvro{Key,Value}{Serializer,Deserializer}` builder extensions here are the rejected alternative on the Kafka backbone and never the chosen changefeed codec. This package is admitted for the OFF-Kafka registry-governed Avro path only — a wire-format-1 Avro body on an object-store or a non-Kafka transport, framed by the `Chr.Avro.Binary` expression-compiled codec the Confluent serde does not use.
- On that off-Kafka path the serde is a codec-plus-governance leg, not public Persistence vocabulary; the subject, registration behavior, and tombstone policy are profile data.
- `AutomaticRegistrationBehavior.Never` is the production default — schema registration is an explicit governance act, never an implicit side effect of the first produce on the durable path.
- A serde is built once per `(type, subject)` and cached with its compiled body delegate; the registry REST round-trip is a build-time cost, never per-message.
- Null-handling is `TombstoneBehavior.Strict` on a compacted/CDC topic so a delete is a real tombstone, never a serialized Avro null; the choice is recorded in the egress sink receipt.

[RAIL_LAW]:
- Package: `Chr.Avro.Confluent`
- Owns: the OFF-Kafka Schema-Registry-governed Avro serde — wire-format-1 framing over the `Chr.Avro.Binary` expression-compiled body codec, four-way schema resolution, subject-keyed evolution, and the Kafka builder extensions ONLY as the explicitly-rejected duplicate of the first-party serde
- Defers: the Kafka-slot Avro serde to `Confluent.SchemaRegistry.Serdes.Avro` (`api-schemaregistry-serdes-avro`) — that package owns `IAsyncSerializer<T>`/`IAsyncDeserializer<T>` on the `Confluent.Kafka` value/key slot, the shared `CachedSchemaRegistryClient`, and the `RuleRegistry` CSFLE pass
- Accept: `Build<T>` facade construction on the off-Kafka path, `Never`-default registration, `Strict`-tombstone CDC nulls
- Reject: hand-assembled magic-byte/schema-id prefixes, per-message registry round-trips, implicit auto-registration on the durable path, a serde that bypasses subject governance, and mounting these `SetAvro*` extensions on a Kafka producer/consumer where the first-party serde owns the slot
