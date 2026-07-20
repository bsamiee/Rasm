# [PY_ARTIFACTS_API_CLOUDEVENTS]

`cloudevents` is the CNCF reference Python SDK for the CloudEvents envelope: one `cloudevents.core.v1.event.CloudEvent` carries a required/optional/extension attribute mapping and an arbitrary data payload, and protocol bindings lower it onto HTTP headers-or-body and Kafka message parts. `cloudevents.core.*` is the authoritative package family; its owner composes `cloudevents.core.formats.json.JSONFormat` with `cloudevents.core.bindings.http.to_structured_event`/`to_binary_event`, never re-implementing the spec attribute algebra, JSON format, or binding header maps.

Artifacts `delivery/notice` composes it into `TransmittalNotice`: an ISO 19650 issue close mints one `cloudevents.core.v1.event.CloudEvent` keyed by the transmittal content key, carries container digests and register-row references as extension attributes, injects the active trace context as the `traceparent`/`tracestate` extension, and serializes to structured-JSON bytes an app transports over any carrier — so a downstream system ingests an issue-for-construction event without opening the archive. This SDK holds no broker client; its Kafka binding returns a transport-neutral `KafkaMessage` the composing app hands to its own producer, matching the ruled python asymmetry that transport ends at envelope bytes.

`cloudevents.v1.*` is a separately packaged API family with dict-backed and pydantic events, conversion helpers, and Kafka helpers. Notice composition uses `cloudevents.core.*` exclusively, so one event never crosses family-specific types or conversion contracts.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cloudevents`
- package: `cloudevents`
- import: `cloudevents.core.v1.event`, `cloudevents.core.formats`, `cloudevents.core.bindings`, and the separate `cloudevents.v1.http`, `cloudevents.v1.kafka`, `cloudevents.v1.pydantic`, `cloudevents.v1.conversion`, `cloudevents.v1.abstract`, and `cloudevents.v1.exceptions` family
- owner: `artifacts`
- rail: delivery-notice
- license: Apache-2.0
- target: pure-Python `py3-none-any` wheel (no native asset, no RID burden)
- runtime deps: `python-dateutil` (RFC 3339 time parse), `deprecation` (deprecation markers), both pure-Python; the `pydantic` binding composes the already-admitted `pydantic`
- entry points: none (library only)
- capability: CloudEvents event envelope with required/optional/extension attribute algebra, structured-versus-binary content-mode conversion, JSON format codec, HTTP and Kafka protocol bindings, an optional `pydantic` `BaseModel` event, and extension-attribute carriage for the distributed-tracing (`traceparent`/`tracestate`), partitioning, and sampling spec extensions

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: separately packaged `cloudevents.v1.*` envelope and faults
- rail: delivery-notice

`cloudevents.v1.http.CloudEvent` and `cloudevents.v1.pydantic.CloudEvent` back the shared `cloudevents.v1.abstract.CloudEvent` contract — a dict-backed event and a validating `pydantic.BaseModel` event. Attributes are a `Mapping[str, Any]` of spec-reserved and arbitrary extension keys, and `data` is any payload. Distributed tracing uses the `traceparent`/`tracestate` extension attributes in that mapping. `SpecVersion` selects `v1_0` or `v0_3`, and every fault subclasses `GenericException`.

| [INDEX] | [SYMBOL]                                          | [PACKAGE_ROLE]           | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------ | :----------------------- | :--------------------------------------------- |
|  [01]   | `cloudevents.v1.http.CloudEvent`                  | dict-backed envelope     | `create`/`get_attributes`/`get_data` event     |
|  [02]   | `cloudevents.v1.abstract.CloudEvent`              | envelope contract        | shared `create`/`get_attributes`/`get` base    |
|  [03]   | `cloudevents.v1.pydantic.CloudEvent`              | validated envelope       | `BaseModel` event with field validation        |
|  [04]   | `cloudevents.v1.kafka.KafkaMessage`               | transport value          | `NamedTuple(headers, key, value)`, broker-free |
|  [05]   | `cloudevents.v1.kafka.KeyMapper`                  | key-derivation callable  | `Callable[[CloudEvent], str]` partition mapper |
|  [06]   | `SpecVersion`                                     | spec-version axis (enum) | `v1_0` / `v0_3` spec selector                  |
|  [07]   | `GenericException`                                | envelope fault base      | `Exception` subclass; every fault descends     |
|  [08]   | `MissingRequiredFields` / `InvalidRequiredFields` | attribute fault          | required absence / malformed required value    |
|  [09]   | `InvalidStructuredJSON` / `InvalidHeadersFormat`  | wire fault               | structured-body / binary-header decode failure |
|  [10]   | `DataMarshallerError` / `DataUnmarshallerError`   | payload fault            | data marshaller raised on encode/decode        |
|  [11]   | `PydanticFeatureNotInstalled`                     | optional-feature fault   | `pydantic` binding used without the extra      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: event construction
- rail: separately packaged `cloudevents.v1.*` family — `CloudEvent.create` is the checked factory; a bare `CloudEvent(attributes, data)` constructor also exists

`create(attributes, data)` builds the event from an attributes mapping and an optional payload; missing `id`/`time` derive through `default_id_selection_algorithm` and `default_time_selection_algorithm`. `get_attributes`/`get_data` read the mapping and payload back; `__getitem__`/`get` read one attribute by reserved-or-extension key.

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]                             | [CAPABILITY]                       |
| :-----: | :-------------------------- | :--------------------------------------- | :--------------------------------- |
|  [01]   | `CloudEvent.create`         | `create(attributes, data) -> CloudEvent` | build an event; id/time default    |
|  [02]   | `CloudEvent.get_attributes` | `get_attributes() -> Mapping[str, Any]`  | read the reserved + extension map  |
|  [03]   | `CloudEvent.get_data`       | `get_data() -> Any \| None`              | read the payload                   |
|  [04]   | `CloudEvent.get`            | `get(key, default=None) -> Any \| None`  | read one attribute with a fallback |

[ENTRYPOINT_SCOPE]: content-mode conversion (`cloudevents.v1.conversion`)
- rail: delivery-notice — structured places every attribute and data inside one JSON body; binary splits attributes to headers and data to the raw body

`to_structured` returns one JSON-object body carrying all attributes and data with headers `{"content-type": "application/cloudevents+json"}`. `to_binary` splits attributes into `ce-`-prefixed headers with the data as the raw body; `to_json` is the structured body bytes alone. `from_json`/`from_http`/`from_dict` reverse each, parameterized by the target `event_type` so both the dict and pydantic backings round-trip. Every pair takes an optional `data_marshaller`/`data_unmarshaller` for non-JSON payloads.

| [INDEX] | [CALL]                                                                                     | [RESULT]             |
| :-----: | :----------------------------------------------------------------------------------------- | :------------------- |
|  [01]   | `cloudevents.v1.conversion.to_structured(event, data_marshaller=None)`                     | header/body tuple    |
|  [02]   | `cloudevents.v1.conversion.to_binary(event, data_marshaller=None)`                         | binary header/body   |
|  [03]   | `cloudevents.v1.conversion.to_json(event, data_marshaller=None)`                           | structured bytes     |
|  [04]   | `cloudevents.v1.conversion.to_dict(event)`                                                | event dictionary     |
|  [05]   | `cloudevents.v1.conversion.from_json(event_type, data, data_unmarshaller=None)`            | event from JSON      |
|  [06]   | `cloudevents.v1.conversion.from_http(event_type, headers, data, data_unmarshaller=None)`   | event from HTTP      |
|  [07]   | `cloudevents.v1.conversion.from_dict(event_type, event)`                                  | event from mapping   |
|  [08]   | `cloudevents.v1.conversion.is_binary(headers)`                                            | content-mode verdict |

[ENTRYPOINT_SCOPE]: Kafka protocol binding (`cloudevents.v1.kafka`)
- rail: delivery-notice — lowers one event to a broker-neutral `KafkaMessage`, holding no producer/consumer client

`to_binary`/`to_structured` return a `KafkaMessage(headers, key, value)` — binary maps attributes to `ce_`-prefixed headers with data as the value, structured packs the whole event as the JSON value. `key_mapper` derives the partition key from the event, defaulting to the `partitionkey` extension attribute; `from_binary`/`from_structured` reverse a received message. Composing apps own the Kafka client — this binding only shapes the message parts.

| [INDEX] | [CALL]                                                                                                               | [RESULT]   |
| :-----: | :------------------------------------------------------------------------------------------------------------------- | :--------- |
|  [01]   | `cloudevents.v1.kafka.to_binary(event, data_marshaller=None, key_mapper=None)`                                       | binary     |
|  [02]   | `cloudevents.v1.kafka.to_structured(event, data_marshaller=None, envelope_marshaller=None, key_mapper=None)`         | structured |
|  [03]   | `cloudevents.v1.kafka.from_binary(message, event_type=None, data_unmarshaller=None)`                                 | decoded    |
|  [04]   | `cloudevents.v1.kafka.from_structured(message, event_type=None, data_unmarshaller=None, envelope_unmarshaller=None)` | decoded    |

## [04]-[CORE_SURFACE]

[CORE_SCOPE]: `cloudevents.core.*` typed-binding architecture
- rail: delivery-notice — authoritative event, format, and binding family

`cloudevents.core.v1.event.CloudEvent` extends `BaseCloudEvent` with per-attribute getters (`get_id`/`get_source`/`get_type`/`get_time`/`get_subject`/`get_datacontenttype`/`get_dataschema`/`get_extension`) and a validating constructor raising the closed `CloudEventValidationError`/`MissingRequiredAttributeError`/`InvalidAttributeTypeError`/`InvalidAttributeValueError`/`CustomExtensionAttributeError` family over `REQUIRED_ATTRIBUTES`/`OPTIONAL_ATTRIBUTES`. `cloudevents.core.formats` supplies the pluggable `Format`/`EventFactory` contract with a `JSONFormat`; `cloudevents.core.bindings` owns the HTTP, AMQP, Kafka, RabbitMQ, and common protocol adapters over that format.

| [INDEX] | [SYMBOL]                                               | [PACKAGE_ROLE]     | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------- | :----------------- | :-------------------------------------- |
|  [01]   | `cloudevents.core.v1.event.CloudEvent`                 | typed envelope     | `BaseCloudEvent` + getters + validation |
|  [02]   | `cloudevents.core.v1.event.CloudEventValidationError`  | typed fault family | required/type/value/extension faults    |
|  [03]   | `cloudevents.core.formats.base.Format`                 | format contract    | `read`/`write` pluggable serialization  |
|  [04]   | `cloudevents.core.formats.json.JSONFormat`             | JSON format        | `cloudevents+json` read/write           |
|  [05]   | `cloudevents.core.bindings.{http,amqp,kafka,rabbitmq}` | protocol bindings  | per-protocol structured/binary lowering |

## [05]-[STACKING]

[STACKING]: the notice owner composes CloudEvents beside the runtime trace context and the delivery close
- `opentelemetry-api` owns the trace context — `propagate.inject(carrier, context, setter)` writes `traceparent`/`tracestate` into a carrier dict, which the notice owner folds into the `cloudevents.core.v1.event.CloudEvent(attributes, data)` attributes mapping; the SDK holds no OpenTelemetry dependency, so the wiring lives in the notice owner.
- `msgspec`/`pydantic` own the payload shape — the notice `data` is a settled JSON-compatible value that `JSONFormat` and `cloudevents.core.bindings.http.to_structured_event` serialize into the event body.
- `delivery/transmittal` composes the notice as a terminal issue-closure member — container digests and issued-register row references from `TransmittalEvidence` become notice extension attributes keyed by the transmittal `ContentKey`.
- `xxhash` owns the content key the notice `subject` references — the CloudEvents `id` stays the SDK-defaulted uuid4 and the transmittal content key rides a reserved extension attribute, so replay identity and event identity never collide.
- Kafka `KafkaMessage` and `cloudevents.core.bindings` never pull a broker client — every binding returns a transport-neutral value, so the ruled python asymmetry (no broker clients; transport belongs to the composing app) holds at the SDK boundary.
