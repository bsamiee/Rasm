# [RASM_PERSISTENCE_API_CLOUDEVENTS]

CloudEvents projects the Persistence redacted op-log changefeed onto one CNCF-standard wire: `CloudNative.CloudEvents` owns the `CloudEventsSpecVersion.V1_0` envelope and typed attribute algebra, `CloudNative.CloudEvents.Kafka` binds it to a `Confluent.Kafka` `Message<string?, byte[]>`, and `CloudNative.CloudEvents.SystemTextJson` supplies the `System.Text.Json` codec. Every `CdcEnvelope` becomes one `CloudEvent` encoded binary-mode, decoded by external brokers and the Python `runtime/transport` leg. Native Kafka transport rides `Confluent.Kafka`/`librdkafka.redist` under `api-kafka`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CloudNative.CloudEvents`
- package: `CloudNative.CloudEvents` (Apache-2.0)
- assembly: `CloudNative.CloudEvents` (`net10.0` bound asset, pure-managed, no RID burden)
- namespace: `CloudNative.CloudEvents`, `CloudNative.CloudEvents.Extensions`, `CloudNative.CloudEvents.Core`
- rail: sync-egress

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.Kafka`
- package: `CloudNative.CloudEvents.Kafka` (Apache-2.0)
- assembly: `CloudNative.CloudEvents.Kafka` (`net10.0` bound asset, pure-managed)
- namespace: `CloudNative.CloudEvents.Kafka`
- depends: `Confluent.Kafka` (native `librdkafka.redist` rides that package, `api-kafka`)
- rail: sync-egress

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.SystemTextJson`
- package: `CloudNative.CloudEvents.SystemTextJson` (Apache-2.0)
- assembly: `CloudNative.CloudEvents.SystemTextJson` (`net10.0` bound asset over `System.Text.Json`, BCL-shipped on `net10.0`)
- namespace: `CloudNative.CloudEvents.SystemTextJson`
- rail: sync-egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core envelope and attribute algebra (`CloudNative.CloudEvents`)

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [CAPABILITY]                                                        |
| :-----: | :----------------------------- | :------------- | :------------------------------------------------------------------ |
|  [01]   | `CloudEvent`                   | sealed class   | mutable v1.0 event; typed attributes + `Data`                       |
|  [02]   | `CloudEventAttribute`          | class          | name, type, required/optional/extension role; parse/format/validate |
|  [03]   | `CloudEventAttributeType`      | abstract class | seven value-type singletons; `Parse`/`Format`/`Validate`/`ClrType`  |
|  [04]   | `CloudEventsSpecVersion`       | class          | required/optional attribute schema; `V1_0`, version-id resolution   |
|  [05]   | `ContentMode`                  | enum           | `Structured` vs. `Binary` placement                                 |
|  [06]   | `CloudEventFormatter`          | abstract class | encode/decode contract every binding extends                        |
|  [07]   | `CloudEventFormatterAttribute` | attribute      | `[CloudEventFormatter(typeof(T))]` on a payload CLR type            |
|  [08]   | `Partitioning`                 | static class   | `partitionkey` attribute + `Set`/`GetPartitionKey`                  |
|  [09]   | `Sampling`                     | static class   | `sampledrate` (Integer) attribute + `Set`/`GetSampledRate`          |
|  [10]   | `Sequence`                     | static class   | `sequence`/`sequencetype` (String) attributes + accessors           |
|  [11]   | `Validation`                   | static class   | `CheckNotNull`/`CheckArgument` boundary guards                      |
|  [12]   | `MimeUtilities`                | static class   | `application/cloudevents` media types, content-type bridge          |
|  [13]   | `BinaryDataUtilities`          | static class   | `ReadOnlyMemory<byte>` ⇄ `Stream`/array/`ArraySegment` glue         |

[PUBLIC_TYPE_SCOPE]: Kafka protocol binding (`CloudNative.CloudEvents.Kafka`)

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :---------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `KafkaExtensions` | static class  | `CloudEvent` ⇄ `Message<string?, byte[]>`; `ce_` header binding |

[PUBLIC_TYPE_SCOPE]: System.Text.Json codec (`CloudNative.CloudEvents.SystemTextJson`)

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :---------------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `JsonEventFormatter`    | class         | `CloudEventFormatter` over `System.Text.Json`; `Data` stays `JsonElement`/bytes |
|  [02]   | `JsonEventFormatter<T>` | class         | binary-mode `Data` serialized/deserialized as CLR type `T`                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `CloudEvent` construction and attribute access

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                                |
| :-----: | :----------------------------------------------------- | :------- | :---------------------------------------------------------- |
|  [01]   | `new CloudEvent([specVersion][, extensionAttributes])` | ctor     | v1.0 event; optional spec-version + pre-declared extensions |
|  [02]   | `Id` / `Source` / `Type` / `Time`                      | property | required `id`/`source`/`type` + optional `time`             |
|  [03]   | `Subject` / `DataSchema` / `DataContentType`           | property | optional context attributes                                 |
|  [04]   | `Data`                                                 | property | `object?` payload; format-decoded, set raw bytes or POCO    |
|  [05]   | `this[CloudEventAttribute]` / `this[string]`           | property | typed get/set by attribute or name (auto-extension)         |
|  [06]   | `SetAttributeFromString(string, string)`               | instance | parses + sets from canonical string form                    |
|  [07]   | `GetAttribute(string)`                                 | instance | resolves the `CloudEventAttribute` if populated/known       |
|  [08]   | `GetPopulatedAttributes()`                             | instance | `KeyValuePair<CloudEventAttribute, object>` non-null set    |
|  [09]   | `ExtensionAttributes`                                  | property | declared extension attributes on this event                 |
|  [10]   | `SpecVersion` / `IsValid` / `Validate()`               | instance | version; `IsValid` completeness, `Validate` throws          |

[ENTRYPOINT_SCOPE]: attribute algebra, spec-version, and standard extensions
- `CloudEventAttributeType` value-type singletons: `Boolean` `Integer` `String` `Binary` `Uri` `UriReference` `Timestamp`

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------------ | :------- | :----------------------------------------- |
|  [01]   | `CloudEventAttribute.CreateExtension(string, type[, validator])`          | factory  | declares an extension attribute            |
|  [02]   | `attribute.Parse(string)` / `Format(object)` / `Validate(object)`         | instance | typed string round-trip + validation       |
|  [03]   | `attribute.Type` / `Name` / `IsRequired` / `IsExtension`                  | property | attribute identity and role flags          |
|  [04]   | `attributeType.Parse` / `Format` / `Validate` / `ClrType`                 | instance | per-type parse/format; `ClrType` mapping   |
|  [05]   | `CloudEventsSpecVersion.V1_0` / `.Default` / `FromVersionId(string)`      | static   | v1.0 schema, `V1_0` default, id resolution |
|  [06]   | `specVersion.RequiredAttributes` / `OptionalAttributes` / `AllAttributes` | property | required/optional/full attribute schema    |
|  [07]   | `Partitioning.SetPartitionKey(ce, string)` / `GetPartitionKey(ce)`        | static   | `partitionkey` → Kafka message key         |
|  [08]   | `Sequence.SetSequence(ce, object)` / `GetSequence{Value,String}(ce)`      | static   | total event ordering (String-typed)        |
|  [09]   | `Sampling.SetSampledRate(ce, int)` / `GetSampledRate(ce)`                 | static   | sampling-rate hint (Integer, positive)     |
|  [10]   | `Partitioning`/`Sampling`/`Sequence` `.<name>Attribute`                   | static   | extension attributes to pre-register       |

[ENTRYPOINT_SCOPE]: `CloudEventFormatter` codec contract and `JsonEventFormatter`

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------------ | :------- | :--------------------------------------------- |
|  [01]   | `new JsonEventFormatter([JsonSerializerOptions, JsonDocumentOptions])`    | ctor     | default or options-bound STJ formatter         |
|  [02]   | `new JsonEventFormatter<T>([JsonSerializerOptions, JsonDocumentOptions])` | ctor     | typed-`Data` formatter binding `Data` to `T`   |
|  [03]   | `EncodeStructuredModeMessage(ce, out ContentType)`                        | instance | full event as structured-mode JSON body        |
|  [04]   | `EncodeBinaryModeEventData(ce)`                                           | instance | binary-mode `Data` body                        |
|  [05]   | `EncodeBatchModeMessage(ces, out ContentType)`                            | instance | batch-mode JSON array body                     |
|  [06]   | `DecodeStructuredModeMessage(body, contentType, extensions)` / `...Async` | instance | `ReadOnlyMemory<byte>`/`Stream` → `CloudEvent` |
|  [07]   | `DecodeBatchModeMessage` / `...Async`                                     | instance | array body → `IReadOnlyList<CloudEvent>`       |
|  [08]   | `DecodeBinaryModeEventData(body, ce)`                                     | instance | binary-mode `Data` body into an event          |
|  [09]   | `ConvertToJsonElement(ce)` / `ConvertFromJsonElement(JsonElement, exts)`  | instance | `JsonElement` round-trip of the full event     |
|  [10]   | `GetOrInferDataContentType(ce)`                                           | instance | resolves the effective `datacontenttype`       |

[ENTRYPOINT_SCOPE]: Kafka binding and byte/mime glue

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------------------ | :------- | :-------------------------------------------------- |
|  [01]   | `cloudEvent.ToKafkaMessage(ContentMode, CloudEventFormatter)`       | static   | builds `Message<string?, byte[]>`, key=partitionkey |
|  [02]   | `message.ToCloudEvent(formatter, params CloudEventAttribute[])`     | static   | decodes `Message<string?, byte[]>` to `CloudEvent`  |
|  [03]   | `message.ToCloudEvent(formatter, IEnumerable<CloudEventAttribute>)` | static   | decode with an attribute enumerable                 |
|  [04]   | `message.IsCloudEvent()`                                            | static   | detects the `ce_`/content-type headers              |
|  [05]   | `MimeUtilities.MediaType` / `BatchMediaType`                        | property | `application/cloudevents`[`-batch`] media types     |
|  [06]   | `MimeUtilities.IsCloudEventsContentType(string)`                    | static   | probes a CloudEvents content type                   |
|  [07]   | `BinaryDataUtilities.AsArray`/`AsStream`/`ToReadOnlyMemory[Async]`  | static   | `Data` ⇄ `byte[]`/`Stream`/`ReadOnlyMemory<byte>`   |
|  [08]   | `CloudEventFormatterAttribute.CreateFormatter(Type)`                | factory  | resolves a `[CloudEventFormatter]` formatter        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `CloudEvent` is mutable and sealed; `Data` is `object?` round-tripping through whatever `CloudEventFormatter` encodes it — raw `byte[]`/`ReadOnlyMemory<byte>` under `application/octet-stream`, or a POCO under `JsonEventFormatter<T>`. `CloudEventsSpecVersion.V1_0` requires `id`/`source`/`type` and admits optional `subject`/`datacontenttype`/`dataschema`/`time`; `IsValid` checks required completeness, `Validate` throws on the gap.
- `CloudEventAttributeType` is the closed value-type algebra, each case a static singleton exposing `Parse`/`Format`/`Validate`/`ClrType`; `CloudEventAttribute.CreateExtension` declares every extension attribute.
- `ContentMode` places the event two ways: `Structured` packs attributes + data into the body under `application/cloudevents+json`; `Binary` writes attributes to transport metadata (Kafka `ce_*` headers) and only `Data` to the body, so a header-filtering broker routes without parsing the body.
- `CloudEventFormatter` is the abstract codec every binding extends; `JsonEventFormatter` implements it over `System.Text.Json`, and `JsonEventFormatter<T>` overrides only the binary-mode `Data` codec so the payload deserializes to `T`. `ToKafkaMessage` accepts the base, so a `JsonEventFormatter` instance binds directly.
- Standard extensions are real `CloudEventAttribute`s: `Partitioning.PartitionKeyAttribute` (String) feeds the Kafka message key, `Sampling.SampledRateAttribute` (Integer, positive-validated), `Sequence.SequenceAttribute` (String); each `Set*` is an extension method on `CloudEvent`, callable as `Partitioning.SetPartitionKey(ce, key)` or `ce.SetPartitionKey(key)`.

[STACKING]:
- `Version/egress` projects the `Version/ledger` `OpLogEntry` → `CloudEvent` via the egress-sink projector → `cloudEvent.ToKafkaMessage(ContentMode.Binary, formatter)` → `Confluent.Kafka` `ProduceAsync` (`api-kafka`), whose `DeliveryResult.Status == Persisted` advances the `Store/coordination` `OutboxAdvance` cursor past the contiguous `Persisted` prefix. One shared `JsonEventFormatter` encodes every event; `JsonEventFormatter<T>` is selected only when `Data` is a typed change record.
- `ContentMode.Binary` is load-bearing: the CloudEvents attributes stay in Kafka headers so a broker filters and routes on `ce_type`/`ce_source`/`partitionkey` without deserializing the op payload, and `Partitioning.SetPartitionKey` from the entity key preserves per-key ordering on one partition through `librdkafka`'s default partitioner.
- `api-schemaregistry-serdes-json` (`JsonSerializer<T>`): envelope-vs-body codec ownership is split on one `Message<string?, byte[]>.Headers` bag — this binding's `JsonEventFormatter` owns the CloudEvents envelope (`ce_specversion` and one `ce_<name>` per populated attribute, the `content-type` header, and `partitionkey` → `Message.Key`), while the registry serde owns the `Data` bytes and its `__value_schema_id`/`__key_schema_id` framing, disjoint from the `ce_` keys. Two unrelated JSON stacks, never a shared `JsonSerializerOptions`.
- trace continuity rides a CloudEvents extension attribute: egress sets `traceparent` and `redacted` on the event, and the Python `runtime/transport` `ToCloudEvent` decode recovers the W3C context and continues the originating span. One `CloudEvent` is the single cross-consumer, cross-language vocabulary every `OutboundHop` consumer of the outbox spine drains, so a per-consumer re-pack is the drift defect and an out-of-authority payload crosses masked and traced.

[LOCAL_ADMISSION]:
- Changefeed rows enter as a `CloudEvent` with required `Id` (the content key), `Source` (`rasm:persistence/oplog`), `Type` (`rasm.oplog.{entityKind}.{kind}`), and `Time`, the redacted `CdcEnvelope.Payload` in `Data` under `application/octet-stream`.
- one shared `JsonEventFormatter` (or `JsonEventFormatter<T>` for a typed change record) encodes the egress; serializer options are fixed at formatter construction, never per event.
- egress composes `cloudEvent.ToKafkaMessage(ContentMode.Binary, formatter)` with the partition key via `Partitioning.SetPartitionKey(ce, entityKey)`; `traceparent`/`redacted`/`sequence` are declared once via `CloudEventAttribute.CreateExtension` and set through the typed indexer; ingress and replay decode via `message.ToCloudEvent(formatter, extensions)` with the same pre-declared attribute set.

[RAIL_LAW]:
- Packages: `CloudNative.CloudEvents`, `CloudNative.CloudEvents.Kafka`, `CloudNative.CloudEvents.SystemTextJson`
- Owns: the `CloudEventsSpecVersion.V1_0` envelope, the Kafka protocol binding, and the `System.Text.Json` formatter for the redacted-changefeed egress wire
- Accept: `CloudEvent` construction with typed `CloudEventAttribute`s, one shared `JsonEventFormatter`/`JsonEventFormatter<T>`, `ToKafkaMessage`/`ToCloudEvent` at explicit `ContentMode.Binary`, partition key via `Partitioning`, trace and redaction as extension attributes
- Reject: hand-rolled CloudEvents JSON layout, manual `ce_` Kafka header construction, raw `Message<string?, byte[]>` assembly bypassing `KafkaExtensions`, a per-event formatter instance, or a second envelope shape parallel to the one CloudEvents projection
