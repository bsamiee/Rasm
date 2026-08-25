# [RASM_PERSISTENCE_API_CLOUDEVENTS_KAFKA]

`CloudNative.CloudEvents.Kafka` binds the CloudEvents envelope to a `Confluent.Kafka` `Message<string?, byte[]>` — one `KafkaExtensions` static class carrying the `ce_` header binding for the op-log changefeed egress. `Version/egress` `Egress.Envelope` mints the `CloudEvent` through the branch owner per `OpLogEntry` and this binding encodes it binary-mode, so external brokers and the Python `runtime/transport` leg route on headers without parsing the body. Envelope and codec member truth is the branch catalogue (`libs/dotnet/.api/api-cloudevents.md`); native transport rides `api-kafka.md`.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Kafka protocol binding (`CloudNative.CloudEvents.Kafka`)

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :---------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `KafkaExtensions` | static class  | `CloudEvent` ⇄ `Message<string?, byte[]>`; `ce_` header binding |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Kafka binding maps

| [INDEX] | [SURFACE]                                                           | [SHAPE] | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------------------ | :------ | :-------------------------------------------------- |
|  [01]   | `cloudEvent.ToKafkaMessage(ContentMode, CloudEventFormatter)`       | static  | builds `Message<string?, byte[]>`, key=partitionkey |
|  [02]   | `message.ToCloudEvent(formatter, params CloudEventAttribute[])`     | static  | decodes `Message<string?, byte[]>` to `CloudEvent`  |
|  [03]   | `message.ToCloudEvent(formatter, IEnumerable<CloudEventAttribute>)` | static  | decode with an attribute enumerable                 |
|  [04]   | `message.IsCloudEvent()`                                            | static  | detects the `ce_`/content-type headers              |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ContentMode.Binary` writes attributes to Kafka `ce_*` headers and only `Data` to the body, so a header-filtering broker routes on `ce_type`/`ce_source` without deserializing the op payload; `ToKafkaMessage` accepts the base `CloudEventFormatter`, so the shared `JsonEventFormatter` instance binds directly. Content type rides a BARE `content-type` header carrying no `ce_` prefix, written only where the resolved type is non-null.
- Partition keys ride NO header: `ToKafkaMessage` reads `Partitioning.PartitionKeyAttribute` into `Message.Key` and EXCLUDES it, beside `datacontenttype`, from the `ce_`-prefixed map, so a broker header-filter selects on it never and only the partitioner sees it.
- That key preserves per-entity ordering on one partition through `librdkafka`'s default partitioner — the one binding row in the roster exposing a real routing member, which is why `RoutesOn` stays a column and never a law the other rows inherit falsely. `ToCloudEvent` runs the inverse, copying a non-empty `Message.Key` back onto that attribute.
- `CloudEvent`'s attribute indexer resolves by attribute NAME against one value map, so a generated extension declaring `partitionkey` as a string satisfies `Partitioning.PartitionKeyAttribute` without the producer ever naming the SDK's own instance — which is what makes a generated `partitionkey` field reach `Message.Key` at all, and what makes a non-string declaration of that name fail validation at the read rather than at the mint.
- Binary decode lowercases every stripped attribute name and SILENTLY SKIPS a header whose value bytes are null, so an attribute a peer wrote empty is absent rather than empty on the decoded event.
- Batch is ABSENT: no `ToKafkaMessages`, no batch decode, no batch probe; `IsCloudEvent` answers false for a batch body by its own contract, and `IsCloudEvent` itself tests the `ce_specversion` header first and falls back to the content type.

[STACKING]:
- `CloudNative.CloudEvents`(`libs/dotnet/.api/api-cloudevents.md`): the mapped envelope, the shared formatter identity, and the extension-attribute declarations are that catalogue's; this binding adds the `Message<string?, byte[]>` carrier alone.
- `Confluent.Kafka`(`.api/api-kafka.md`): `Version/egress` composes `cloudEvent.ToKafkaMessage(ContentMode.Binary, EventFormat.Json.Formatter)` → `ProduceAsync`, whose `DeliveryResult.Status == Persisted` folds through `KafkaAck.FromResult` and advances the `Store/coordination` `OutboxAdvance` cursor past the contiguous `Persisted` prefix. Message shape fixes at `Message<string?, byte[]>` with no generic form, so the key stays a string and the value stays framed bytes.
- `api-schemaregistry-serdes-json`(`.api/api-schemaregistry-serdes-json.md`): envelope and body codecs own disjoint bytes on one `Message<string?, byte[]>`. The CloudEvents formatter owns the `ce_*` headers and the bare `content-type` one; the registry serde owns `Data` plus `__value_schema_id` framing, and `Message.Key` stays the partitioner's.
- `datacontenttype` names the body serde's media type. `dataschema`, when present, is an absolute schema URI supplied by the payload owner; it is not the registry subject/version or a protobuf `Any` type URL.
- Registry subject/version stays serializer configuration and the registered schema id stays in Confluent framing. CloudEvents exposes neither as a surrogate envelope attribute.

[LOCAL_ADMISSION]:
- Egress composes `cloudEvent.ToKafkaMessage(ContentMode.Binary, EventFormat.Json.Formatter)` at the Kafka binding row.
- Ingress and egress share the kernel `EventExtensionContract<Extensions>` descriptor bridge.
