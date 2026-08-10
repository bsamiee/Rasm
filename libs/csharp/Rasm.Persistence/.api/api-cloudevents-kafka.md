# [RASM_PERSISTENCE_API_CLOUDEVENTS_KAFKA]

`CloudNative.CloudEvents.Kafka` binds the CloudEvents envelope to a `Confluent.Kafka` `Message<string?, byte[]>` — one `KafkaExtensions` static class carrying the `ce_` header binding for the op-log changefeed egress. `Version/egress` `Egress.Envelope` mints the `CloudEvent` through the branch owner per `OpLogEntry` and this binding encodes it binary-mode, so external brokers and the Python `runtime/transport` leg route on headers without parsing the body. Envelope and codec member truth is the branch catalogue (`libs/csharp/.api/api-cloudevents.md`); native transport rides `api-kafka.md`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.Kafka`
- package: `CloudNative.CloudEvents.Kafka` (Apache-2.0)
- assembly: `CloudNative.CloudEvents.Kafka` (`net10.0` bound asset, pure-managed)
- namespace: `CloudNative.CloudEvents.Kafka`
- depends: `CloudNative.CloudEvents` (branch substrate, `libs/csharp/.api/api-cloudevents.md`); `Confluent.Kafka` (native `librdkafka.redist` rides that package, `api-kafka.md`)
- rail: sync-egress

[REGISTRATION]: `CloudNative.CloudEvents` + `CloudNative.CloudEvents.SystemTextJson` — branch substrate at `libs/csharp/.api/api-cloudevents.md`; the `CloudEvent` algebra, `JsonEventFormatter`, `Partitioning`, and every attribute member resolve there.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Kafka protocol binding (`CloudNative.CloudEvents.Kafka`)

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :---------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `KafkaExtensions` | static class  | `CloudEvent` ⇄ `Message<string?, byte[]>`; `ce_` header binding |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Kafka binding maps

| [INDEX] | [SURFACE]                                                           | [SHAPE] | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------------------ | :------ | :-------------------------------------------------- |
|  [01]   | `cloudEvent.ToKafkaMessage(ContentMode, CloudEventFormatter)`       | static  | builds `Message<string?, byte[]>`, key=partitionkey |
|  [02]   | `message.ToCloudEvent(formatter, params CloudEventAttribute[])`     | static  | decodes `Message<string?, byte[]>` to `CloudEvent`  |
|  [03]   | `message.ToCloudEvent(formatter, IEnumerable<CloudEventAttribute>)` | static  | decode with an attribute enumerable                 |
|  [04]   | `message.IsCloudEvent()`                                            | static  | detects the `ce_`/content-type headers              |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ContentMode.Binary` writes attributes to Kafka `ce_*` headers and only `Data` to the body, so a header-filtering broker routes on `ce_type`/`ce_source`/`partitionkey` without deserializing the op payload; `ToKafkaMessage` accepts the base `CloudEventFormatter`, so the shared `JsonEventFormatter` instance binds directly.
- `Partitioning.SetPartitionKey` from the entity key feeds `Message.Key`, preserving per-key ordering on one partition through `librdkafka`'s default partitioner — the one binding row in the roster exposing a real routing member, which is why `RoutesOn` is a column and never a law the other rows inherit falsely.

[STACKING]:
- `CloudNative.CloudEvents`(`libs/csharp/.api/api-cloudevents.md`): the mapped envelope, the shared formatter identity, and the extension-attribute declarations are that catalogue's; this binding adds the `Message<string?, byte[]>` carrier alone.
- `Confluent.Kafka`(`.api/api-kafka.md`): `Version/egress` composes `cloudEvent.ToKafkaMessage(ContentMode.Binary, EventFormat.Json.Formatter)` → `ProduceAsync`, whose `DeliveryResult.Status == Persisted` advances the `Store/coordination` `OutboxAdvance` cursor past the contiguous `Persisted` prefix.
- `api-schemaregistry-serdes-json`(`.api/api-schemaregistry-serdes-json.md`): envelope and body codecs own disjoint BYTES on one `Message<string?, byte[]>` — the CloudEvents formatter owns the header bag (`ce_specversion`, one `ce_<name>` per populated attribute, the `content-type` header, `partitionkey` → `Message.Key`) and the registry serde owns the `Data` bytes with its `__value_schema_id` framing — yet the two are JOINED by two attributes, never unrelated: `datacontenttype` carries the serde's own media type and `dataschema` the registry subject and version that serde framed under, so a consumer selects its deserializer from the envelope instead of guessing from the topic. Reading the two stacks as merely disjoint is what leaves `datacontenttype` an unconditional `application/octet-stream` over a registry-framed body. Two unrelated `JsonSerializerOptions` instances, one declared join.

[LOCAL_ADMISSION]:
- Egress composes `cloudEvent.ToKafkaMessage(ContentMode.Binary, EventFormat.Json.Formatter)` at the `kafka` binding row's one leg, the partition key arriving as the row's `RoutesOn` extension rather than a per-leg call; ingress and replay decode via `message.ToCloudEvent(EventFormat.Json.Formatter, EventRoster.Declared)` — the SAME declared roster both directions take, since a roster spelled at one end alone decodes every typed extension as an untyped string.

[RAIL_LAW]:
- Package: `CloudNative.CloudEvents.Kafka`
- Owns: the CloudEvents Kafka protocol binding — binary-mode `CloudEvent` ⇄ `Message<string?, byte[]>` with the `ce_` header contract for the changefeed egress wire
- Accept: `ToKafkaMessage`/`ToCloudEvent` at explicit `ContentMode.Binary` through the shared formatter, partition key via `Partitioning`
- Reject: manual `ce_` Kafka header construction, raw `Message<string?, byte[]>` assembly bypassing `KafkaExtensions`, a `datacontenttype` literal beside a registry-framed body, or an envelope member re-tabled here instead of the branch catalogue
