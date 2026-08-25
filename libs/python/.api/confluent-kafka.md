# [PY_BRANCH_API_CONFLUENT_KAFKA]

`confluent-kafka` binds librdkafka: the producer, consumer, share consumer, message, partition, and error types are one C extension, and everything above them — the admin client, the model carriers, the serialization contracts, and the Schema Registry stack — is pure Python over that base. Every blocking C call releases the GIL, so the synchronous clients compose under a thread lane while their delivery, rebalance, and settlement callbacks fire on whichever thread drove `poll`, `consume`, or `flush`. Schema Registry rides a genuine async client over `httpx.AsyncClient` beside its synchronous twin, and its serializers frame every payload with a magic byte and a schema coordinate.

## [01]-[PUBLIC_TYPES]

[CLIENT_SCOPE]: `confluent_kafka.cimpl`, re-exported at the package root

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :-------------------------------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `Producer`                                                | C class       | the produce queue, its delivery reports, transactions        |
|  [02]   | `Consumer`                                                | C class       | group membership, assignment, poll, offset settlement        |
|  [03]   | `ShareConsumer`                                           | C class       | the KIP-932 share-group consumer over `Messages`             |
|  [04]   | `Message`                                                 | C class       | one record; every accessor is a METHOD, never a field        |
|  [05]   | `TopicPartition`                                          | C class       | the partition coordinate; hashable and ordered               |
|  [06]   | `KafkaError`                                              | C class       | the error code, name, severity, and abort verdict            |
|  [07]   | `KafkaException`                                          | exception     | `args[0]` is the `KafkaError`                                |
|  [08]   | `IllegalStateException` `ConcurrentModificationException` | exception     | `RuntimeError` subclasses; `str` is a message, not an error  |
|  [09]   | `Uuid`                                                    | C class       | `get_most_significant_bits()`/`get_least_significant_bits()` |
|  [10]   | `ThrottleEvent`                                           | class         | `broker_name`, `broker_id`, `throttle_time` in seconds       |

`TopicPartition` carries `topic`, `partition`, `offset`, `leader_epoch`, `metadata`, and `error`; `KafkaError` answers `code()`, `name()`, `str()`, `fatal()`, `retriable()`, and `txn_requires_abort()`.

[MODEL_SCOPE]: `confluent_kafka._model`, re-exported at the package root

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [MEMBERS_OR_FIELDS]                                                      |
| :-----: | :------------------------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `Node`                                 | class         | `id`, `id_string`, `host`, `port`, `rack`                                |
|  [02]   | `ConsumerGroupTopicPartitions`         | class         | `group_id` beside a `TopicPartition` list                                |
|  [03]   | `ConsumerGroupState`                   | `Enum`        | the group-state vocabulary beside a deprecated alias                     |
|  [04]   | `ConsumerGroupType`                    | `Enum`        | `UNKNOWN`, `CONSUMER`, `CLASSIC`                                         |
|  [05]   | `IsolationLevel`                       | `Enum`        | `READ_UNCOMMITTED`, `READ_COMMITTED`                                     |
|  [06]   | `ElectionType`                         | `Enum`        | `PREFERRED`, `UNCLEAN`                                                   |
|  [07]   | `AcknowledgeType`                      | `IntEnum`     | `ACCEPT`, `RELEASE`, `REJECT`                                            |
|  [08]   | `TopicCollection` `TopicPartitionInfo` | class         | the describe-topics request and its partition record                     |
|  [09]   | `Messages`                             | class         | the read-only share-consumer batch: `records()`, `count()`, `is_empty()` |

`ConsumerGroupState` names `UNKNOWN`, `PREPARING_REBALANCING`, `COMPLETING_REBALANCING`, `STABLE`, `DEAD`, `EMPTY`, and the deprecated `UNKOWN` alias.

[SERDE_SCOPE]: `confluent_kafka.serialization` and `confluent_kafka._types`

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :---------------------------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `SerializationContext`                    | class         | `topic`, `field: MessageField`, `headers`              |
|  [02]   | `MessageField`                            | `str` enum    | `NONE`, `KEY`, `VALUE`                                 |
|  [03]   | `SerializationError`                      | exception     | a `KafkaException` subclass                            |
|  [04]   | `Serializer` `Deserializer`               | ABC           | `__call__(obj, ctx=None)`; `None` maps to a Kafka null |
|  [05]   | `StringSerializer` `StringDeserializer`   | class         | one `codec` parameter defaulting `utf_8`               |
|  [06]   | `IntegerSerializer` `IntegerDeserializer` | class         | big-endian int32                                       |
|  [07]   | `DoubleSerializer` `DoubleDeserializer`   | class         | big-endian binary64                                    |
|  [08]   | `_types.HeadersType`                      | type alias    | a `dict[str, str \| bytes \| None]` or its pair list   |

[REGISTRY_SCOPE]: `confluent_kafka.schema_registry` — attrs models, sync and async twins

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :-------------------------------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `SchemaRegistryClient` `AsyncSchemaRegistryClient`  | class         | the same method roster over `httpx.Client` and `httpx.AsyncClient` |
|  [02]   | `Schema`                                            | attrs, frozen | `schema_str`, `schema_type`, `references`, `metadata`, `rule_set`  |
|  [03]   | `RegisteredSchema`                                  | attrs, frozen | `subject`, `version`, `schema_id`, `guid`, `schema`                |
|  [04]   | `SchemaReference`                                   | attrs, frozen | `name`, `subject`, `version`                                       |
|  [05]   | `ServerConfig`                                      | attrs         | compatibility level, group, and the default and override rule sets |
|  [06]   | `Metadata` `MetadataTags` `MetadataProperties`      | attrs         | the schema's own tag and property bands                            |
|  [07]   | `Rule` `RuleSet` `RuleParams` `RuleKind` `RuleMode` | attrs, enum   | the data-contract rule vocabulary                                  |
|  [08]   | `ConfigCompatibilityLevel`                          | `str` enum    | the compatibility-level vocabulary                                 |
|  [09]   | `SchemaRegistryError`                               | exception     | `http_status_code`, `error_code`, `error_message`                  |
|  [10]   | `schema_registry.error.OAuthTokenError`             | exception     | bearer-credential failure; unexported at the package root          |

`Schema.schema_type` defaults `AVRO`, `RegisteredSchema` takes its five fields positionally with no defaults, and `ConfigCompatibilityLevel` names `BACKWARD`, `BACKWARD_TRANSITIVE`, `FORWARD`, `FORWARD_TRANSITIVE`, `FULL`, `FULL_TRANSITIVE`, and `NONE`.

[FAULT_SCOPE]: `confluent_kafka.error`

| [INDEX] | [SYMBOL]                                              | [BASES]                            | [CARRIES]                                  |
| :-----: | :---------------------------------------------------- | :--------------------------------- | :----------------------------------------- |
|  [01]   | `_KafkaClientError`                                   | `KafkaException`                   | `code`, `name`, wrapped exception, message |
|  [02]   | `ConsumeError`                                        | `_KafkaClientError`                | the consume-side failure                   |
|  [03]   | `ProduceError`                                        | `_KafkaClientError`                | the produce-side failure                   |
|  [04]   | `KeyDeserializationError` `ValueDeserializationError` | `ConsumeError, SerializationError` | the matching `_*_DESERIALIZATION` code     |
|  [05]   | `KeySerializationError` `ValueSerializationError`     | `ProduceError, SerializationError` | the matching `_*_SERIALIZATION` code       |

`KafkaError` carries its codes as CLASS attributes in two families — broker codes such as `UNKNOWN_TOPIC_OR_PART` and `TRANSACTION_ABORTABLE`, and underscore-led local codes such as `_MSG_TIMED_OUT`, `_PARTITION_EOF`, `_QUEUE_FULL`, `_PURGE_QUEUE`, `_PURGE_INFLIGHT`, `_ASSIGNMENT_LOST`, `_MAX_POLL_EXCEEDED`, and `_TRANSPORT`.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Producer`

| [INDEX] | [SURFACE]                                                                                                        | [SHAPE]  | [BLOCKS] |
| :-----: | :--------------------------------------------------------------------------------------------------------------- | :------- | :------- |
|  [01]   | `produce(topic, value=None, key=None, partition=-1, callback=None, on_delivery=None, timestamp=0, headers=None)` | instance | no       |
|  [02]   | `produce_batch(topic, messages, partition=-1, callback=None, on_delivery=None)`                                  | instance | no       |
|  [03]   | `poll(timeout=-1)` / `flush(timeout=-1)`                                                                         | instance | yes      |
|  [04]   | `purge(in_queue=True, in_flight=True, blocking=True)`                                                            | instance | optional |
|  [05]   | `init_transactions(timeout=-1)` / `begin_transaction()`                                                          | instance | yes / no |
|  [06]   | `send_offsets_to_transaction(positions, group_metadata, timeout=-1)`                                             | instance | yes      |
|  [07]   | `commit_transaction(timeout=-1)` / `abort_transaction(timeout=-1)`                                               | instance | yes      |
|  [08]   | `list_topics(topic=None, timeout=-1)` / `set_sasl_credentials(username, password)` / `close()`                   | instance | yes / no |
|  [09]   | `__len__()`                                                                                                      | instance | no       |

Delivery reports fire `on_delivery(err, msg)` over `Optional[KafkaError]` and `Message`; `callback` and `on_delivery` are aliases.

[ENTRYPOINT_SCOPE]: `Consumer`

| [INDEX] | [SURFACE]                                                                                                  | [SHAPE]  | [BLOCKS]         |
| :-----: | :--------------------------------------------------------------------------------------------------------- | :------- | :--------------- |
|  [01]   | `subscribe(topics, on_assign=None, on_revoke=None, on_lost=None)` / `unsubscribe()`                        | instance | no               |
|  [02]   | `poll(timeout=-1)` / `consume(num_messages=1, timeout=-1)`                                                 | instance | yes              |
|  [03]   | `commit(message=…, offsets=…, asynchronous=True)`                                                          | instance | when synchronous |
|  [04]   | `store_offsets(message=None, offsets=None)`                                                                | instance | no               |
|  [05]   | `assign(partitions)` / `unassign()` / `assignment()`                                                       | instance | no               |
|  [06]   | `incremental_assign(partitions)` / `incremental_unassign(partitions)`                                      | instance | no               |
|  [07]   | `pause(partitions)` / `resume(partitions)` / `seek(partition)`                                             | instance | no               |
|  [08]   | `position(partitions)` / `committed(partitions, timeout=-1)`                                               | instance | no / yes         |
|  [09]   | `get_watermark_offsets(partition, timeout=-1, cached=False)` / `offsets_for_times(partitions, timeout=-1)` | instance | yes              |
|  [10]   | `consumer_group_metadata()` / `memberid()`                                                                 | instance | no               |
|  [11]   | `close()` / `list_topics(topic=None, timeout=-1)` / `set_sasl_credentials(...)`                            | instance | yes              |

Each rebalance callback takes `(consumer, partitions)` with the ABSOLUTE partition list; `on_lost` defaults to `on_revoke`.

[ENTRYPOINT_SCOPE]: `ShareConsumer` — the KIP-932 settlement surface `AcknowledgeType` keys

| [INDEX] | [SURFACE]                                                            | [BLOCKS] | [SETTLES]                                |
| :-----: | :------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `acknowledge(message, acknowledge_type)` / `acknowledge_offset(...)` | no       | one delivery, by `AcknowledgeType`       |
|  [02]   | `commit_sync(...)` / `commit_async(...)`                             | sync/no  | the acknowledged batch                   |
|  [03]   | `set_acknowledgement_commit_callback(callback)` / `subscription()`   | no       | the commit report and the live topic set |

[ENTRYPOINT_SCOPE]: `Message` — every accessor is a method

| [INDEX] | [SURFACE]                                                   | [ANSWERS]                                                               |
| :-----: | :---------------------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `topic()` `partition()` `offset()`                          | `str \| None`, `int \| None`, `int \| None`                             |
|  [02]   | `key()` `value()`                                           | `bytes \| None`                                                         |
|  [03]   | `headers()`                                                 | the pair-list form of `HeadersType`, or `None`                          |
|  [04]   | `error()`                                                   | `KafkaError \| None` — a returned message may be an event carrier       |
|  [05]   | `timestamp()`                                               | `(timestamp_type, milliseconds)` over the three `TIMESTAMP_*` constants |
|  [06]   | `latency()`                                                 | producer-side seconds from `produce` to delivery                        |
|  [07]   | `leader_epoch()` `delivery_count()`                         | the fencing epoch and the share-group redelivery count                  |
|  [08]   | `set_key` `set_value` `set_headers` `set_error` `set_topic` | the mutators                                                            |

[ENTRYPOINT_SCOPE]: `admin.AdminClient` — every operation answers a stdlib `concurrent.futures.Future`

| [INDEX] | [SURFACE]                                                                                       | [ANSWERS]                          |
| :-----: | :---------------------------------------------------------------------------------------------- | :--------------------------------- |
|  [01]   | `create_topics(new_topics, **kwargs)` / `delete_topics(topics, **kwargs)`                       | `dict[str, Future]`                |
|  [02]   | `create_partitions(new_partitions, **kwargs)`                                                   | `dict[str, Future]`                |
|  [03]   | `describe_topics(topics, **kwargs)` / `describe_cluster(**kwargs)`                              | `dict[str, Future]` / one `Future` |
|  [04]   | `describe_configs(resources, **kwargs)` / `incremental_alter_configs(...)`                      | `dict[ConfigResource, Future]`     |
|  [05]   | `list_consumer_groups(**kwargs)` / `describe_consumer_groups(group_ids, ...)`                   | one `Future` / `dict[str, Future]` |
|  [06]   | `list_consumer_group_offsets(request, **kwargs)` / `alter_consumer_group_offsets(...)`          | `dict[str, Future]`                |
|  [07]   | `list_offsets(topic_partition_offsets, **kwargs)` / `delete_records(...)`                       | `dict[TopicPartition, Future]`     |
|  [08]   | `create_acls` / `describe_acls` / `delete_acls`                                                 | per-binding futures                |
|  [09]   | `list_topics(topic=None, timeout=-1)` / `list_groups(...)`                                      | SYNCHRONOUS metadata               |
|  [10]   | `delete_consumer_groups(group_ids, **kwargs)` / `elect_leaders(election_type, partitions=None)` | `dict[str, Future]` / one `Future` |
|  [11]   | `describe_user_scram_credentials(...)` / `alter_user_scram_credentials(...)`                    | `dict[str, Future]`                |

[ENTRYPOINT_SCOPE]: Schema Registry — the sync roster; the async twin adds `Async` and awaits

| [INDEX] | [SURFACE]                                                                                         | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------------------------------------------ | :----------------------------------- |
|  [01]   | `SchemaRegistryClient(conf)`                                                                      | one dict; `ValueError` on stray keys |
|  [02]   | `register_schema(subject_name, schema, normalize_schemas=False)`                                  | the schema id                        |
|  [03]   | `register_schema_full_response(subject_name, schema, normalize_schemas=False)`                    | the whole `RegisteredSchema`         |
|  [04]   | `get_schema(schema_id, subject_name=None, fmt=None, reference_format=None)`                       | the `Schema` by id                   |
|  [05]   | `get_schema_by_guid(guid, fmt=None)`                                                              | the `Schema` by guid                 |
|  [06]   | `lookup_schema(subject_name, schema, normalize_schemas=False, fmt=None, deleted=False)`           | the registered coordinate for a body |
|  [07]   | `get_latest_version(subject_name, fmt=None)` / `get_version(subject_name, version='latest', ...)` | one version                          |
|  [08]   | `get_latest_with_metadata(subject_name, metadata, deleted=False, fmt=None)`                       | metadata-selected version            |
|  [09]   | `test_compatibility(subject_name, schema, version='latest', normalize=False, verbose=False)`      | the compatibility verdict            |
|  [10]   | `set_config(subject_name=None, config=None)` / `get_config(...)` / `delete_config(...)`           | the `ServerConfig` triple            |
|  [11]   | `get_mode` / `update_mode` / `get_global_mode` / `update_global_mode`                             | registry mode                        |
|  [12]   | `clear_caches()` / `clear_latest_caches()` / `new_client(conf)`                                   | cache reset and the static factory   |
|  [13]   | `get_subjects(subject_prefix=None, deleted=False, deleted_only=False, offset=0, limit=-1)`        | the subject roster                   |
|  [14]   | `get_versions(subject_name, ...)` / `delete_subject(subject_name, permanent=False)`               | subject lifecycle                    |

[ENTRYPOINT_SCOPE]: registry serdes — the argument order diverges per format

| [INDEX] | [SURFACE]                                                                                                                       |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `AvroSerializer(schema_registry_client, schema_str=None, to_dict=None, conf=None)`                                              |
|  [02]   | `AvroDeserializer(schema_registry_client, schema_str=None, from_dict=None, return_record_name=False, conf=None)`                |
|  [03]   | `JSONSerializer(schema_str, schema_registry_client, to_dict=None, conf=None, json_encode=None)`                                 |
|  [04]   | `JSONDeserializer(schema_str, from_dict=None, schema_registry_client=None, conf=None, json_decode=None)`                        |
|  [05]   | `ProtobufSerializer(msg_type, schema_registry_client, conf=None)`                                                               |
|  [06]   | `ProtobufDeserializer(message_type, conf=None, schema_registry_client=None)`                                                    |
|  [07]   | `topic_subject_name_strategy` / `topic_record_subject_name_strategy` / `record_subject_name_strategy`                           |
|  [08]   | `prefix_schema_id_serializer` / `header_schema_id_serializer` / `dual_schema_id_deserializer` / `prefix_schema_id_deserializer` |

Every serde answers `__call__(obj, ctx: SerializationContext | None = None)`. Avro and Protobuf constructors tail `rule_conf=None` beside `rule_registry=None`; the JSON pair tails its own encode hook past them — `JSONSerializer(..., rule_conf, rule_registry, json_encode=None)` and `JSONDeserializer(..., rule_conf, rule_registry, json_decode=None)`.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every blocking C call RELEASES the GIL — `poll`, `consume`, `flush`, a synchronous `commit`, the transaction verbs, `list_topics`, and the admin operations — so a thread hop is sound and does not stall a sibling task.
- Callbacks fire on whichever thread drove `poll`, `consume`, `flush`, or `close`: the delivery report, the three rebalance legs, `on_commit`, and the error, stats, and throttle callables all run there and never on a background thread of the client's own.
- `produce` is asynchronous and enqueues only; it raises `BufferError` when the internal queue is full and `NotImplementedError` where the broker cannot carry a timestamp. Headers are NOT propagated onto the `Message` handed to the delivery callback, while `produce_batch` DOES carry a per-message `headers` slot and validates it — a malformed one raises `ValueError: Invalid headers at index N` where an unknown key is silently ignored.
- `flush` calls `poll` until the queue empties or the timeout elapses, so every pending delivery report drains through it; `commit_transaction` and `abort_transaction` each flush first, which means delivery callbacks fire from inside a transaction verb.
- `purge` surfaces purged messages as `_PURGE_QUEUE`/`_PURGE_INFLIGHT` delivery reports on the next poll rather than dropping them silently.
- Transactional failures read off `KafkaException.args[0]` through `retriable()` and `txn_requires_abort()`; anything answering neither is fatal.
- `poll` may answer a `Message` that is an EVENT rather than a record — `error()` must be read before `value()`, and `_PARTITION_EOF` arrives that way when `enable.partition.eof` is set.
- Committed offsets sit ONE PAST the message offset. `store_offsets` requires `enable.auto.offset.store=False`, and a synchronous `commit` answers a `TopicPartition` list whose per-partition `.error` carries the verdict.
- Under a cooperative assignor, `on_assign` calls `incremental_assign` and `on_revoke`/`on_lost` call `incremental_unassign`; the client does it for a callback that does not.
- Every method raises `RuntimeError` on a closed consumer.
- This release SHIPS an asyncio layer — `confluent_kafka.aio.AIOProducer` and `AIOConsumer` — hard-wired to `asyncio.get_running_loop`, `asyncio.Future`, and `run_coroutine_threadsafe`. `AIOProducer.produce` answers a Future resolving to the delivered message rather than enqueuing, and its `headers` keyword raises `NotImplementedError`.
- Schema Registry framing is the codec seam: version 0 writes a one-byte magic `0x00` beside a big-endian unsigned schema id, followed for Protobuf by a zigzag varint message-index array; version 1 writes magic `0x01` beside a sixteen-byte guid. `prefix_schema_id_serializer` puts that header on the payload while `header_schema_id_serializer` puts the guid form into the `__key_schema_id`/`__value_schema_id` message header instead; `dual_schema_id_deserializer` reads the header first and falls back to the prefix. `AvroDeserializer` refuses a payload of five bytes or fewer.
- `use.latest.version` and `auto.register.schemas` are mutually exclusive and raise `ValueError` together. `use.deprecated.format` is accepted only as false.
- `SchemaRegistryError` and `OAuthTokenError` derive from `Exception` and NOT from `KafkaException`, so a registry failure escapes a Kafka catch-all.
- `AdminClient` answers stdlib futures, never awaitables.
- Async registry serdes are `@asyncinit`, so construction itself is awaited.
- Type stubs omit four runtime members — `Producer.close`, `Message.set_topic`, and the two `Uuid` bit accessors.
- Deprecations carry both a `.. deprecated::` marker and a `DeprecationWarning`: the whole `confluent_kafka.avro` legacy tree, `AdminClient.alter_configs` in favour of `incremental_alter_configs`, and the `ConsumerGroupState.UNKOWN` typo alias. `SerializingProducer`/`DeserializingConsumer`/`DeserializingShareConsumer` carry NEITHER — their docstrings read "experimental and likely to be removed", which is an instability claim rather than a deprecation.

[STACKING]:
- `anyio`(`.api/anyio.md`): the synchronous clients ride a `CapacityLimiter`-bounded `to_thread` lane, sound because every blocking call releases the GIL. Poll-loop lifetime is the caller's task group and never a daemon thread; callbacks re-enter through one `BlockingPortalProvider`, which is exactly what the shipped `aio` layer does with asyncio primitives.
- `cloudevents`(`.api/cloudevents.md`): `core.bindings.kafka.KafkaMessage` maps field for field — `headers` onto `produce(headers=)` and back off `Message.headers()`, `key` onto `produce(key=)` off the `partitionkey` extension, `value` onto `produce(value=)` — and the `ce_` prefix is the binding's alone.
- `fastavro`(`.api/fastavro.md`) and `protobuf`(`.api/protobuf.md`): the registry serializers compose them beneath the framing, so a payload codec never sees the magic byte and the framing never sees a schema document.
- `httpx`(`runtime/.api/httpx.md`): the async registry client is one `httpx.AsyncClient`, so a registry lookup composes in the caller's task group with no thread hop.

[LOCAL_ADMISSION]:
- Synchronous `Producer`/`Consumer` under a thread lane is the admitted arm; `confluent_kafka.aio` is refused because its loop ownership and its `asyncio.Future` contract bind the composition to one backend, which is the loop-ownership defect.
- `AsyncSchemaRegistryClient` and the `Async*` serdes are the admitted registry arm, since they are genuinely non-blocking and need no lane.
- `enable.auto.commit` is false and offsets settle explicitly, so a commit never outruns the durable write it stands for.
- `SchemaRegistryError` and `OAuthTokenError` are caught by name beside `KafkaException`, never through it.
- Legacy `confluent_kafka.avro` and the three wrapper clients are refused whole; serializers compose directly.
