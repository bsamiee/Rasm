# [RASM_PERSISTENCE_API_CLOUDEVENTS]

`CloudNative.CloudEvents` is the CNCF reference SDK for the CloudEvents `CloudEventsSpecVersion.V1_0` envelope: the
mutable sealed `CloudEvent` in-memory event, the `CloudEventAttribute`/`CloudEventAttributeType`
typed attribute algebra, the `CloudEventsSpecVersion` schema, and the abstract
`CloudEventFormatter` encode/decode contract that every protocol/format binding extends.
`CloudNative.CloudEvents.Kafka` adds the Kafka protocol binding (`KafkaExtensions`) over a
`Confluent.Kafka` `Message<string?, byte[]>`, and `CloudNative.CloudEvents.SystemTextJson`
adds the `JsonEventFormatter`/`JsonEventFormatter<T>` System.Text.Json codec. The trio is the
single wire vocabulary the Persistence `Version/egress` rail projects the redacted op-log
changefeed onto — one `CdcEnvelope` becomes one CloudEvent encoded structured-or-binary,
bound to Kafka headers, partitioned by entity key, and decoded identically by external brokers
and the Python `runtime/transport` leg. The whole surface is pure-managed; only the Kafka
binding pulls a native transport, and that lives in the `Confluent.Kafka`/`librdkafka.redist`
asset documented by `api-kafka`, never in these three assemblies.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CloudNative.CloudEvents`
- package: `CloudNative.CloudEvents`
- license: `Apache-2.0`
- assembly: `CloudNative.CloudEvents` (`lib/net8.0` binds for the `net10.0` consumer; `netstandard2.0`/`netstandard2.1` are fallback assets)
- namespace: `CloudNative.CloudEvents`, `CloudNative.CloudEvents.Extensions`, `CloudNative.CloudEvents.Core`, `CloudNative.CloudEvents.Http`
- asset: pure-managed library (no native asset, no RID burden)
- rail: sync-egress

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.Kafka`
- package: `CloudNative.CloudEvents.Kafka`
- license: `Apache-2.0`
- assembly: `CloudNative.CloudEvents.Kafka` (`lib/net10.0` first-class asset at; `net8.0`/`netstandard2.0` are fallback assets)
- namespace: `CloudNative.CloudEvents.Kafka`
- asset: pure-managed library; depends on `Confluent.Kafka` (native `librdkafka.redist` rides that package, see `api-kafka`)
- rail: sync-egress

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.SystemTextJson`
- package: `CloudNative.CloudEvents.SystemTextJson`
- license: `Apache-2.0`
- assembly: `CloudNative.CloudEvents.SystemTextJson` (`lib/net8.0` binds for the `net10.0` consumer)
- namespace: `CloudNative.CloudEvents.SystemTextJson`
- asset: pure-managed library over `System.Text.Json` (BCL-shipped on `net10.0`)
- rail: sync-egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core envelope and attribute algebra (`CloudNative.CloudEvents`)
- rail: sync-egress

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                                                         |
| :-----: | :----------------------------- | :----------------- | :------------------------------------------------------------- |
|  [01]   | `CloudEvent`                   | event envelope     | mutable sealed v1.0 event; attributes + `Data`                 |
|  [02]   | `CloudEventAttribute`          | attribute value    | name, type, required/optional/extension, parse/format/validate |
|  [03]   | `CloudEventAttributeType`      | attribute type     | abstract; seven `CloudEvents` value-type singletons            |
|  [04]   | `CloudEventsSpecVersion`       | spec-version value | required/optional attribute schema, version-id resolution      |
|  [05]   | `ContentMode`                  | content-mode enum  | `Structured` vs. `Binary` placement                            |
|  [06]   | `CloudEventFormatter`          | formatter base     | abstract encode/decode contract every binding extends          |
|  [07]   | `CloudEventFormatterAttribute` | data-type marker   | `[CloudEventFormatter(typeof(T))]` on a payload CLR type       |
|  [08]   | `Partitioning`                 | extension static   | `partitionkey` attribute + `Set`/`GetPartitionKey`             |
|  [09]   | `Sampling`                     | extension static   | `sampledrate` (Integer) attribute + `Set`/`GetSampledRate`     |
|  [10]   | `Sequence`                     | extension static   | `sequence`/`sequencetype` (String) attributes + accessors      |
|  [11]   | `Validation`                   | guard static       | `CheckNotNull`/`CheckArgument` boundary guards                 |
|  [12]   | `MimeUtilities`                | mime static        | `application/cloudevents` media types, content-type bridge     |
|  [13]   | `BinaryDataUtilities`          | byte static        | `ReadOnlyMemory<byte>` ⇄ `Stream`/array/`ArraySegment` glue    |

[PUBLIC_TYPE_SCOPE]: Kafka protocol binding (`CloudNative.CloudEvents.Kafka`)
- rail: sync-egress

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]    | [RAIL]                                                          |
| :-----: | :---------------- | :--------------- | :-------------------------------------------------------------- |
|  [01]   | `KafkaExtensions` | extension static | `CloudEvent` ⇄ `Message<string?, byte[]>`; `ce_` header binding |

[PUBLIC_TYPE_SCOPE]: System.Text.Json codec (`CloudNative.CloudEvents.SystemTextJson`)
- rail: sync-egress

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [RAIL]                                                                          |
| :-----: | :---------------------- | :-------------- | :------------------------------------------------------------------------------ |
|  [01]   | `JsonEventFormatter`    | formatter       | `CloudEventFormatter` over `System.Text.Json`; `Data` stays `JsonElement`/bytes |
|  [02]   | `JsonEventFormatter<T>` | typed formatter | binary-mode `Data` serialised/deserialised as CLR type `T`                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `CloudEvent` construction and attribute access
- rail: sync-egress

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [RAIL]                                                   |
| :-----: | :------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `new CloudEvent()`                                 | ctor           | default v1.0 event (`CloudEventsSpecVersion.Default`)    |
|  [02]   | `new CloudEvent(specVersion)`                      | ctor           | explicit spec-version event                              |
|  [03]   | `new CloudEvent(extensionAttributes)`              | ctor           | v1.0 event pre-declaring extension attributes            |
|  [04]   | `new CloudEvent(specVersion, extensionAttributes)` | ctor           | spec version plus extension declarations                 |
|  [05]   | `Id` / `Source` / `Type` / `Time`                  | property       | required (`id`/`source`/`type`) + optional `time`        |
|  [06]   | `Subject` / `DataSchema` / `DataContentType`       | property       | optional context attributes                              |
|  [07]   | `Data`                                             | property       | `object?` payload; format-decoded, set raw bytes or POCO |
|  [08]   | `this[CloudEventAttribute]` / `this[string]`       | indexer        | typed get/set by attribute or by name (auto-extension)   |
|  [09]   | `SetAttributeFromString(name, value)`              | parse setter   | parses + sets from canonical string form                 |
|  [10]   | `GetAttribute(name)`                               | lookup         | resolves the `CloudEventAttribute` if populated/known    |
|  [11]   | `GetPopulatedAttributes()`                         | projection     | `KeyValuePair<CloudEventAttribute, object>` non-null set |
|  [12]   | `ExtensionAttributes`                              | projection     | declared extension attributes on this event              |
|  [13]   | `SpecVersion` / `IsValid` / `Validate()`           | validation     | version, required-attribute completeness, throw-on-fail  |

[ENTRYPOINT_SCOPE]: attribute algebra, spec-version, and standard extensions
- rail: sync-egress

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]     | [RAIL]                                           |
| :-----: | :--------------------------------------------------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `CloudEventAttribute.CreateExtension(name, type[, validator])`   | factory            | declares an extension attribute (+ validator)    |
|  [02]   | `CloudEventAttribute.CreateRequired`                             | factory            | required context attribute (custom spec version) |
|  [03]   | `CloudEventAttribute.CreateOptional`                             | factory            | optional context attribute (custom spec version) |
|  [04]   | `attribute.Parse(text)` / `Format(value)` / `Validate(value)`    | value op           | typed string round-trip and constraint check     |
|  [05]   | `attribute.Type` / `Name` / `IsRequired` / `IsExtension`         | property           | attribute identity and role flags                |
|  [06]   | `CloudEventAttributeType.Boolean`..`Timestamp`                   | static singleton   | the seven closed value-type singletons           |
|  [07]   | `attributeType.Parse` / `Format` / `Validate` / `ClrType`        | type op            | per-type parse/format and `Type ClrType` mapping |
|  [08]   | `CloudEventsSpecVersion.V1_0` / `.Default` / `FromVersionId(id)` | static/parse       | v1.0 schema, default (`V1_0`), id resolution     |
|  [09]   | `specVersion.RequiredAttributes`                                 | projection         | required context-attribute schema                |
|  [10]   | `specVersion.OptionalAttributes`                                 | projection         | optional context-attribute schema                |
|  [11]   | `specVersion.AllAttributes`                                      | projection         | full attribute schema for round-trip             |
|  [12]   | `Partitioning.SetPartitionKey(ce, key)` / `GetPartitionKey(ce)`  | extension accessor | `partitionkey` → Kafka message key               |
|  [13]   | `Sequence.SetSequence(ce, value)`                                | extension accessor | set total event ordering (String-typed)          |
|  [14]   | `Sequence.GetSequenceValue(ce)` / `GetSequenceString(ce)`        | extension accessor | read the sequence value / string                 |
|  [15]   | `Sampling.SetSampledRate(ce, rate)` / `GetSampledRate(ce)`       | extension accessor | sampling-rate hint (Integer, positive)           |
|  [16]   | `Partitioning.PartitionKeyAttribute`                             | static attribute   | `partitionkey` attribute to pre-register         |
|  [17]   | `Sampling.SampledRateAttribute`                                  | static attribute   | `sampledrate` attribute to pre-register          |
|  [18]   | `Sequence.SequenceAttribute`                                     | static attribute   | `sequence` attribute to pre-register             |

[ENTRYPOINT_SCOPE]: `CloudEventFormatter` codec contract and `JsonEventFormatter`
- rail: sync-egress

Every `Encode*` takes `(cloudEvent[s][, out ContentType])` and every `Decode*` takes
`(body|stream, contentType, extensions)`, in a sync form and a `...Async` mirror.

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `new JsonEventFormatter()`                                           | ctor           | default System.Text.Json formatter              |
|  [02]   | `new JsonEventFormatter(serializerOptions, documentOptions)`         | ctor           | `JsonSerializerOptions` + `JsonDocumentOptions` |
|  [03]   | `new JsonEventFormatter<T>([serializerOptions, documentOptions])`    | ctor           | typed-`Data` formatter binding `Data` to `T`    |
|  [04]   | `EncodeStructuredModeMessage`                                        | encode         | full event as structured-mode JSON body         |
|  [05]   | `EncodeBinaryModeEventData`                                          | encode         | binary-mode `Data` body (attributes → headers)  |
|  [06]   | `EncodeBatchModeMessage`                                             | encode         | batch-mode JSON array body                      |
|  [07]   | `DecodeStructuredModeMessage`                                        | decode         | `ReadOnlyMemory<byte>`/`Stream` → `CloudEvent`  |
|  [08]   | `DecodeStructuredModeMessageAsync`                                   | async decode   | awaitable stream structured-mode decode         |
|  [09]   | `DecodeBatchModeMessage` / `DecodeBatchModeMessageAsync`             | decode         | array body → `IReadOnlyList<CloudEvent>`        |
|  [10]   | `DecodeBinaryModeEventData`                                          | decode         | binary-mode `Data` body into an event           |
|  [11]   | `ConvertToJsonElement(ce)` / `ConvertFromJsonElement(element, exts)` | element bridge | `JsonElement` round-trip of the full event      |
|  [12]   | `GetOrInferDataContentType(cloudEvent)`                              | content infer  | resolves the effective `datacontenttype`        |

[ENTRYPOINT_SCOPE]: Kafka binding and byte/mime glue
- rail: sync-egress

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                                 |
| :-----: | :--------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `cloudEvent.ToKafkaMessage(contentMode, formatter)`        | egress map     | builds `Message<string?, byte[]>` (key = partitionkey) |
|  [02]   | `message.ToCloudEvent(formatter, params extensions)`       | ingress map    | decodes a `Message<string?, byte[]>` to `CloudEvent`   |
|  [03]   | `message.ToCloudEvent(formatter, IEnumerable<extensions>)` | ingress map    | decode with an attribute enumerable                    |
|  [04]   | `message.IsCloudEvent()`                                   | predicate      | detects the CloudEvents `ce_`/content-type headers     |
|  [05]   | `MimeUtilities.MediaType`                                  | mime           | `application/cloudevents` media type                   |
|  [06]   | `MimeUtilities.BatchMediaType`                             | mime           | `application/cloudevents-batch` media type             |
|  [07]   | `MimeUtilities.IsCloudEventsContentType`                   | mime           | probes a CloudEvents content type                      |
|  [08]   | `BinaryDataUtilities.AsArray`                              | byte glue      | `Data` payload → `byte[]`                              |
|  [09]   | `BinaryDataUtilities.AsStream`                             | byte glue      | `Data` payload → `Stream`                              |
|  [10]   | `BinaryDataUtilities.ToReadOnlyMemory[Async]`              | byte glue      | `Data` payload → `ReadOnlyMemory<byte>`                |
|  [11]   | `CloudEventFormatterAttribute.CreateFormatter(targetType)` | reflection     | resolves a `[CloudEventFormatter]`-annotated formatter |

## [04]-[IMPLEMENTATION_LAW]

[CLOUDEVENTS_TOPOLOGY]:
- core namespace `CloudNative.CloudEvents` carries `CloudEvent`, `CloudEventAttribute`, `CloudEventAttributeType`, `CloudEventsSpecVersion`, `ContentMode`, `CloudEventFormatter`, and `CloudEventFormatterAttribute`; extension accessors (`Partitioning`/`Sampling`/`Sequence`) live in `CloudNative.CloudEvents.Extensions`; byte/mime/validation glue lives in `CloudNative.CloudEvents.Core` (`BinaryDataUtilities`, `MimeUtilities`, `Validation`); an HTTP binding lives in `CloudNative.CloudEvents.Http` and is unused by Persistence (Kafka is the transport).
- `CloudEvent` is mutable and sealed; `Data` is `object?` and round-trips through whatever `CloudEventFormatter` encodes it — a raw `byte[]`/`ReadOnlyMemory<byte>` with `DataContentType = application/octet-stream`, or a POCO under `JsonEventFormatter<T>`. The `CloudEventsSpecVersion.V1_0` required context attributes are `id`, `source`, `type`; optional are `subject`, `datacontenttype`, `dataschema`, `time`. `IsValid` checks required completeness; `Validate` throws on the gap.
- `CloudEventAttributeType` is the closed value-type algebra: `Boolean`, `Integer`, `String`, `Binary`, `Uri`, `UriReference`, `Timestamp` — each a static singleton exposing `Parse`/`Format`/`Validate`/`ClrType`. `CloudEventAttribute.CreateExtension(name, type[, validator])` declares an extension; `CreateRequired`/`CreateOptional` are the spec-version context-attribute factories.
- `ContentMode` is two cases: `Structured` packs the entire event (attributes + data) into the body under `application/cloudevents+json`; `Binary` places attributes in transport metadata (Kafka `ce_*` headers) and only `Data` in the body. A header-filtering broker reads attributes without parsing the body only in `Binary` mode.
- `CloudEventFormatter` is the abstract codec every binding extends: `EncodeStructuredModeMessage`/`EncodeBinaryModeEventData`/`EncodeBatchModeMessage` and the `Decode*`/`Decode*Async` mirror, plus `GetOrInferDataContentType`. `JsonEventFormatter` is the System.Text.Json implementation taking `JsonSerializerOptions`/`JsonDocumentOptions`; `JsonEventFormatter<T>` overrides only the binary-mode `Data` codec so the payload deserialises to `T`. `ToKafkaMessage` accepts the `CloudEventFormatter` base, so a `JsonEventFormatter` instance binds directly.
- the standard extensions are real `CloudEventAttribute`s, not free strings: `Partitioning.PartitionKeyAttribute` (`partitionkey`, String) feeds the Kafka message key; `Sampling.SampledRateAttribute` (`sampledrate`, Integer, positive-validated); `Sequence.SequenceAttribute`/`SequenceTypeAttribute` (String). `SetPartitionKey`/`SetSampledRate`/`SetSequence` are extension methods on `CloudEvent`, callable as `Partitioning.SetPartitionKey(ce, key)` or `ce.SetPartitionKey(key)`.
- `KafkaExtensions` reads/writes the `ce_`-prefixed headers and the `content-type` header on `Message<string?, byte[]>`; `ToKafkaMessage` selects placement by `ContentMode` and projects the `partitionkey` extension onto `Message.Key`. The four members (`ToKafkaMessage`, two `ToCloudEvent`, `IsCloudEvent`) are the whole binding.

[STACKING]:
- the changefeed wire is a single rail: the `Version/ledger#CHANGEFEED` `OpLogEntry` → `CloudEvent` via the `Version/egress#EGRESS_SINK` `Egress.Envelope` projector → `Confluent.Kafka` `Message<string?, byte[]>` via `ToKafkaMessage(ContentMode.Binary, formatter)` → awaited `ProduceAsync` whose `DeliveryResult.Status == Persisted` (see `api-kafka`) advances the durable `SyncCursor`. One shared `JsonEventFormatter` instance encodes every event; `JsonEventFormatter<T>` is selected only when `Data` is a typed change record. There is no second envelope shape and no hand-built `ce_` header.
- `Binary` content mode is the load-bearing choice for the stack: it keeps the CloudEvents attributes in Kafka headers, so a broker or consumer filters and routes on `ce_type`/`ce_source`/`partitionkey` without deserialising the op payload, and `Partitioning.SetPartitionKey` from the entity key preserves per-key ordering on one partition through `librdkafka`'s default partitioner.
- distributed-trace continuity stacks through a CloudEvents extension attribute, not a side channel: the egress sets `traceparent` (and `redacted`) as extension attributes on the event, so the Python `runtime/transport` ingress `ToCloudEvent` decode recovers the W3C trace context and extract-and-continues the originating span — the one envelope carries the masking flag and the trace, so an out-of-authority payload crosses masked and traced rather than raw.
- the three keyed AppHost `OutboundHop` consumers of the `[ONE_OUTBOX_EGRESS_SPINE]` (this Persistence egress, the AppHost outbox relay, the durable-orchestration dispatch) all drain the same `CdcEnvelopeWire` CloudEvents projection as the hop payload; the CloudEvents envelope is the single cross-consumer, cross-language vocabulary, so a per-consumer re-pack is the drift defect.
- envelope-vs-body codec ownership is split and never overlapped: this binding (`CloudNative.CloudEvents.SystemTextJson` `JsonEventFormatter` over `ToKafkaMessage`) owns the CloudEvents envelope — `MapHeaders` writes `ce_specversion` plus one `ce_<name>` header per populated attribute (`datacontenttype` and `partitionkey` excluded), the `content-type` header, and the `partitionkey` projection onto `Message.Key` — while the registry-governed payload-body codec (`Confluent.SchemaRegistry.Serdes.Json` `JsonSerializer<T>`, see `api-schemaregistry-serdes-json`) owns the `Data` bytes and its own schema-id framing. The envelope formatter is `System.Text.Json`; the body serde is `Newtonsoft.Json` under `NJsonSchema` validation — two unrelated JSON stacks, never a shared `JsonSerializerOptions`.
- the two header families co-exist on one `Message<string?, byte[]>.Headers` with zero key collision: when the body serde frames the schema id in a header (`SchemaIdSerializerStrategy.Header`) it writes the Confluent `__value_schema_id` / `__key_schema_id` keys (`SchemaId.VALUE_SCHEMA_ID_HEADER` / `KEY_SCHEMA_ID_HEADER` via `HeaderSchemaIdEncoder`), disjoint from the `ce_`-prefixed CloudEvents attribute headers and the `content-type` header `ToKafkaMessage` writes. A header-filtering broker routes on `ce_type`/`ce_source`/`partitionkey` without parsing the body, and the registry-id header travels alongside untouched — so a `Binary`-mode CloudEvents envelope and a registry-validated `Data` body are one message, not two layers fighting for the header bag.

[LOCAL_ADMISSION]:
- the changefeed enters through a `CloudEvent` populated with required `Id` (the content key), `Source` (a stable `rasm:persistence/oplog` URI), `Type` (`rasm.oplog.{entityKind}.{kind}`), and `Time`, with the redacted `CdcEnvelope.Payload` in `Data` and `DataContentType = application/octet-stream`.
- a single shared `JsonEventFormatter` instance encodes the egress; `JsonEventFormatter<T>` only when `Data` is a typed change record rather than raw bytes. The serializer options are fixed at formatter construction, never per event.
- Kafka egress composes `cloudEvent.ToKafkaMessage(ContentMode.Binary, formatter)`; the partition key is `Partitioning.SetPartitionKey(ce, entityKey)`; extension attributes (`traceparent`, `redacted`, `sequence`) are declared once via `CloudEventAttribute.CreateExtension` and set with the typed indexer.
- ingress and replay decode through `message.ToCloudEvent(formatter, extensions)` with the same pre-declared extension attribute set, so the consumer reads the typed attributes rather than re-parsing header strings.

[RAIL_LAW]:
- Packages: `CloudNative.CloudEvents`, `CloudNative.CloudEvents.Kafka`, `CloudNative.CloudEvents.SystemTextJson`
- Owns: the CloudEvents `CloudEventsSpecVersion.V1_0` envelope, the Kafka protocol binding, and the System.Text.Json formatter for the redacted-changefeed egress wire
- Accept: `CloudEvent` construction with typed `CloudEventAttribute`s, one shared `JsonEventFormatter`/`JsonEventFormatter<T>`, `ToKafkaMessage`/`ToCloudEvent` with explicit `ContentMode.Binary`, partition key via `Partitioning`, trace/redaction as extension attributes
- Reject: hand-rolled CloudEvents JSON layout, manual `ce_` Kafka header construction, raw `Message<string?, byte[]>` assembly bypassing `KafkaExtensions`, a per-event formatter instance, or a second envelope shape parallel to the one CloudEvents projection
