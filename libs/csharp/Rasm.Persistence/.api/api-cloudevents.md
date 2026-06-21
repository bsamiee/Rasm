# [RASM_PERSISTENCE_API_CLOUDEVENTS]

`CloudNative.CloudEvents` supplies the CNCF CloudEvents v1.0 envelope: the `CloudEvent`
in-memory event, its typed attribute model, the `CloudEventsSpecVersion` schema, and the
`CloudEventFormatter` encode/decode contract with `ContentMode` selection. `CloudNative.CloudEvents.Kafka`
supplies the Kafka protocol binding through `KafkaExtensions`, mapping a `CloudEvent` to and
from a `Confluent.Kafka` `Message<string?, byte[]>`. `CloudNative.CloudEvents.SystemTextJson`
supplies the `JsonEventFormatter` System.Text.Json encoder, the formatter that serialises the
changefeed as CloudEvents over Kafka for Persistence sync/egress CDC.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CloudNative.CloudEvents`
- package: `CloudNative.CloudEvents`
- assembly: `CloudNative.CloudEvents`
- namespace: `CloudNative.CloudEvents`, `CloudNative.CloudEvents.Extensions`
- asset: runtime library
- rail: sync-egress

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.Kafka`
- package: `CloudNative.CloudEvents.Kafka`
- assembly: `CloudNative.CloudEvents.Kafka`
- namespace: `CloudNative.CloudEvents.Kafka`
- asset: runtime library
- rail: sync-egress

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.SystemTextJson`
- package: `CloudNative.CloudEvents.SystemTextJson`
- assembly: `CloudNative.CloudEvents.SystemTextJson`
- namespace: `CloudNative.CloudEvents.SystemTextJson`
- asset: runtime library
- rail: sync-egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core envelope and attribute family
- rail: sync-egress

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [RAIL]                              |
| :-----: | :------------------------ | :----------------- | :---------------------------------- |
|  [01]   | `CloudEvent`              | event envelope     | mutable v1.0 event with attributes  |
|  [02]   | `CloudEventAttribute`     | attribute value    | name, type, required/extension flag |
|  [03]   | `CloudEventAttributeType` | attribute type     | seven CloudEvents value types       |
|  [04]   | `CloudEventsSpecVersion`  | spec-version value | required/optional attribute schema  |
|  [05]   | `ContentMode`             | content-mode enum  | structured vs. binary placement     |
|  [06]   | `CloudEventFormatter`     | formatter base     | encode/decode contract              |
|  [07]   | `Partitioning`            | extension static   | `partitionkey` attribute accessors  |
|  [08]   | `Sampling`                | extension static   | `sampledrate` attribute accessors   |
|  [09]   | `Sequence`                | extension static   | `sequence` attribute accessors      |

[PUBLIC_TYPE_SCOPE]: Kafka binding family
- rail: sync-egress

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]    | [RAIL]                                |
| :-----: | :---------------- | :--------------- | :------------------------------------ |
|  [01]   | `KafkaExtensions` | extension static | `CloudEvent` <-> Kafka `Message` maps |

[PUBLIC_TYPE_SCOPE]: System.Text.Json formatter family
- rail: sync-egress

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [RAIL]                              |
| :-----: | :---------------------- | :-------------- | :---------------------------------- |
|  [01]   | `JsonEventFormatter`    | formatter       | System.Text.Json CloudEvents codec  |
|  [02]   | `JsonEventFormatter<T>` | typed formatter | data deserialised to a CLR type `T` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: CloudEvent construction and attribute access
- rail: sync-egress

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `new CloudEvent()`                           | ctor           | default v1.0 event              |
|  [02]   | `new CloudEvent(specVersion)`                | ctor           | explicit spec-version event     |
|  [03]   | `new CloudEvent(extensionAttributes)`        | ctor           | event with extension attributes |
|  [04]   | `Id` / `Source` / `Type` / `Time`            | property       | required context attributes     |
|  [05]   | `Subject` / `DataSchema` / `DataContentType` | property       | optional context attributes     |
|  [06]   | `Data`                                       | property       | event payload, format-decoded   |
|  [07]   | `this[CloudEventAttribute]`                  | indexer        | typed attribute get/set         |
|  [08]   | `this[string]`                               | indexer        | attribute get/set by name       |
|  [09]   | `SetAttributeFromString(name, value)`        | parse setter   | sets attribute from string form |
|  [10]   | `GetAttribute(name)`                         | lookup         | resolves `CloudEventAttribute`  |
|  [11]   | `GetPopulatedAttributes()`                   | projection     | non-null attribute pairs        |
|  [12]   | `IsValid` / `Validate()`                     | validation     | required-attribute completeness |

[ENTRYPOINT_SCOPE]: attribute, spec-version, and extension surfaces
- rail: sync-egress

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY]     | [RAIL]                          |
| :-----: | :------------------------------------------------------------ | :----------------- | :------------------------------ |
|  [01]   | `CloudEventAttribute.CreateExtension(name, type)`             | factory            | declares an extension attribute |
|  [02]   | `CloudEventAttribute.CreateExtension(name, type, validator)`  | factory            | extension with validator        |
|  [03]   | `CloudEventAttributeType.Boolean`..`Timestamp`                | static singleton   | seven CloudEvents value types   |
|  [04]   | `CloudEventAttribute.Parse` / `Format` / `Validate`           | value op           | string round-trip and check     |
|  [05]   | `CloudEventsSpecVersion.V1_0`                                 | static value       | v1.0 attribute schema           |
|  [06]   | `CloudEventsSpecVersion.Default`                              | static value       | default spec version (`V1_0`)   |
|  [07]   | `CloudEventsSpecVersion.FromVersionId(id)`                    | parse              | resolves spec version by id     |
|  [08]   | `RequiredAttributes` / `OptionalAttributes` / `AllAttributes` | projection         | attribute schema enumeration    |
|  [09]   | `Partitioning.SetPartitionKey` / `GetPartitionKey`            | extension accessor | Kafka partition routing key     |
|  [10]   | `Sequence.SetSequence` / `GetSequenceValue`                   | extension accessor | total event ordering            |
|  [11]   | `Sampling.SetSampledRate` / `GetSampledRate`                  | extension accessor | sampling rate hint              |

[ENTRYPOINT_SCOPE]: Kafka binding and JSON formatter
- rail: sync-egress

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :----------------------------------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `cloudEvent.ToKafkaMessage(contentMode, formatter)`          | egress map     | builds `Message<string?, byte[]>`  |
|  [02]   | `message.ToCloudEvent(formatter, params extensions)`         | ingress map    | decodes message to `CloudEvent`    |
|  [03]   | `message.ToCloudEvent(formatter, extensions)`                | ingress map    | decodes with attribute enumerable  |
|  [04]   | `message.IsCloudEvent()`                                     | predicate      | detects CloudEvents Kafka headers  |
|  [05]   | `new JsonEventFormatter()`                                   | ctor           | default System.Text.Json formatter |
|  [06]   | `new JsonEventFormatter(serializerOptions, documentOptions)` | ctor           | formatter with JSON options        |
|  [07]   | `EncodeStructuredModeMessage(cloudEvent, out contentType)`   | encode         | structured-mode JSON body          |
|  [08]   | `EncodeBinaryModeEventData(cloudEvent)`                      | encode         | binary-mode data body              |
|  [09]   | `EncodeBatchModeMessage(cloudEvents, out contentType)`       | encode         | batch-mode JSON array body         |
|  [10]   | `DecodeStructuredModeMessage(body, contentType, extensions)` | decode         | structured-mode body to event      |
|  [11]   | `DecodeBinaryModeEventData(body, cloudEvent)`                | decode         | binary-mode data into `Data`       |
|  [12]   | `ConvertToJsonElement` / `ConvertFromJsonElement`            | element bridge | `JsonElement` round-trip           |

## [04]-[IMPLEMENTATION_LAW]

[CLOUDEVENTS_TOPOLOGY]:
- core namespace: `CloudNative.CloudEvents` — `CloudEvent`, `CloudEventAttribute`, `CloudEventAttributeType`, `CloudEventsSpecVersion`, `ContentMode`, `CloudEventFormatter`
- extension namespace: `CloudNative.CloudEvents.Extensions` — `Partitioning`, `Sampling`, `Sequence` static accessors over standard extension attributes
- Kafka namespace: `CloudNative.CloudEvents.Kafka` — `KafkaExtensions` over `Confluent.Kafka` `Message<string?, byte[]>`
- JSON namespace: `CloudNative.CloudEvents.SystemTextJson` — `JsonEventFormatter` and `JsonEventFormatter<T>`
- `CloudEvent` is a mutable, sealed v1.0 envelope; required attributes are `id`, `source`, `type`, and optional context attributes are `subject`, `datacontenttype`, `dataschema`, `time`
- `CloudEventAttributeType` exposes seven value types: `Boolean`, `Integer`, `String`, `Binary`, `Uri`, `UriReference`, `Timestamp`
- `ContentMode` has two cases: `Structured` packs the full event in the body; `Binary` places attributes in transport metadata and data in the body
- `CloudEventFormatter` is the abstract codec contract; `JsonEventFormatter` is the System.Text.Json implementation, with `JsonEventFormatter<T>` deserialising `Data` to a CLR type
- `KafkaExtensions` reads and writes `ce_`-prefixed Kafka headers and the `content-type` header; `ToKafkaMessage` selects placement by `ContentMode`
- `Partitioning.GetPartitionKey` supplies the Kafka message key; the binding sets it from the `partitionkey` extension attribute

[LOCAL_ADMISSION]:
- The changefeed enters through a `CloudEvent` populated with required `Id`, `Source`, `Type`, and `Time`, plus the change payload in `Data`.
- A single shared `JsonEventFormatter` instance encodes the egress; `JsonEventFormatter<T>` is used when `Data` is a typed change record rather than raw JSON.
- Kafka egress composes `cloudEvent.ToKafkaMessage(ContentMode.Binary, formatter)`; binary mode keeps attributes in headers so brokers and consumers filter without parsing the body.
- The Kafka partition key is set through `Partitioning.SetPartitionKey` so co-keyed changes preserve per-key ordering on one partition.
- Ingress and replay decode through `message.ToCloudEvent(formatter, extensions)`; extension attributes are declared via `CloudEventAttribute.CreateExtension` at the boundary, not re-parsed inline.

[RAIL_LAW]:
- Packages: `CloudNative.CloudEvents`, `CloudNative.CloudEvents.Kafka`, `CloudNative.CloudEvents.SystemTextJson`
- Owns: the CloudEvents v1.0 envelope, the Kafka protocol binding, and the System.Text.Json formatter for changefeed egress
- Accept: `CloudEvent` construction with typed attributes, `JsonEventFormatter` encode/decode, `ToKafkaMessage`/`ToCloudEvent` with explicit `ContentMode`, partition key via `Partitioning`
- Reject: hand-rolled CloudEvents JSON layout, manual `ce_` Kafka header construction, or raw `Message<string?, byte[]>` assembly bypassing the binding
