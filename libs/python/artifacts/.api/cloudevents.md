# [PY_ARTIFACTS_API_CLOUDEVENTS]

`cloudevents` is the CNCF Python SDK for the CloudEvents envelope: a required/optional/extension attribute algebra over arbitrary `data`, structured-versus-binary conversion, a JSON codec, and HTTP and Kafka bindings that lower one event onto transport parts. `cloudevents.v1.*` backs a dict event and a validating `pydantic` event with conversion and Kafka helpers; `cloudevents.core.*` owns a typed validating event over a pluggable format and per-protocol bindings. Every binding returns a transport-neutral value and the SDK holds no broker or HTTP client, so transport ends at envelope bytes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cloudevents`
- package: `cloudevents` (Apache-2.0, CNCF)
- module: `cloudevents.v1`, `cloudevents.core`
- namespaces: `cloudevents.v1.{http,kafka,pydantic,conversion,abstract,exceptions}`, `cloudevents.core.{spec,v1.event,formats,bindings}`
- target: pure-Python wheel, no native asset
- rail: delivery-notice — the trace-continuous envelope sealing the transmittal close

## [02]-[PUBLIC_TYPES]

[V1_TYPE_SCOPE]: `cloudevents.v1.*` envelope, transport value, and faults
- `cloudevents.v1.http.CloudEvent` and `cloudevents.v1.pydantic.CloudEvent` realize the `cloudevents.v1.abstract.CloudEvent` contract; attributes are a `Mapping[str, Any]` of spec-reserved and arbitrary extension keys, `data` is any payload, and distributed tracing rides the `traceparent`/`tracestate` extension keys.

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :----------------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `cloudevents.v1.http.CloudEvent`     | class         | dict-backed event: `create`/`get_attributes`   |
|  [02]   | `cloudevents.v1.abstract.CloudEvent` | class         | shared `create`/`get`/`get_data` base          |
|  [03]   | `cloudevents.v1.pydantic.CloudEvent` | class         | `BaseModel` event with field validation        |
|  [04]   | `cloudevents.v1.kafka.KafkaMessage`  | struct        | `NamedTuple(headers, key, value)`, broker-free |
|  [05]   | `cloudevents.v1.kafka.KeyMapper`     | delegate      | `Callable[[CloudEvent], str]` partition mapper |

[V1_FAULTS]: `GenericException` (base) `MissingRequiredFields` `InvalidRequiredFields` `InvalidStructuredJSON` `InvalidHeadersFormat` `DataMarshallerError` `DataUnmarshallerError` `PydanticFeatureNotInstalled`

[CORE_TYPE_SCOPE]: `cloudevents.core.*` typed event, pluggable format, and bindings
- `cloudevents.core.v1.event.CloudEvent` validates against `REQUIRED_ATTRIBUTES`/`OPTIONAL_ATTRIBUTES` on construction and reads back through a getter per spec attribute and `get_extension`. `SpecVersion` is `Literal["1.0", "0.3"]`; `SPECVERSION_V1_0`/`SPECVERSION_V0_3` carry those string constants.

| [INDEX] | [SYMBOL]                                                      | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------------------ | :------------ | :------------------------------------------------------ |
|  [01]   | `cloudevents.core.v1.event.CloudEvent`                        | class         | typed validating envelope with per-attribute getters    |
|  [02]   | `cloudevents.core.spec.SpecVersion`                           | type alias    | `Literal["1.0", "0.3"]` spec-version discriminant       |
|  [03]   | `cloudevents.core.formats.base.Format`                        | protocol      | pluggable `read(event_factory)`/`write` codec           |
|  [04]   | `cloudevents.core.formats.json.JSONFormat`                    | class         | `application/cloudevents+json` read/write               |
|  [05]   | `cloudevents.core.bindings.{http,amqp,kafka,rabbitmq,common}` | module        | per-protocol structured/binary lowering over a `Format` |

[CORE_FAULTS]: `CloudEventValidationError` (aggregates) `MissingRequiredAttributeError` `InvalidAttributeTypeError` `InvalidAttributeValueError` `CustomExtensionAttributeError`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: event construction (`cloudevents.v1`)
- `create(attributes, data)` builds the event; a missing `id` defaults to a UUID and a missing `time` to the current UTC instant. `get_attributes`/`get_data` read the mapping and payload; `get`/`__getitem__` read one attribute by reserved-or-extension key.

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :-------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `CloudEvent.create(attributes, data) -> CloudEvent` | factory  | build an event; id/time default    |
|  [02]   | `CloudEvent.get_attributes() -> Mapping`            | instance | read the reserved + extension map  |
|  [03]   | `CloudEvent.get_data() -> Any \| None`              | instance | read the payload                   |
|  [04]   | `CloudEvent.get(key, default=None)`                 | instance | read one attribute with a fallback |

[ENTRYPOINT_SCOPE]: content-mode conversion (`cloudevents.v1.conversion`)
- Structured places every attribute and `data` inside one JSON body under `content-type: application/cloudevents+json`; binary splits attributes into `ce-`-prefixed headers with `data` as the raw body. `from_*` reverse each, parameterized by `event_type` so both the dict and pydantic backings round-trip, and every pair takes an optional `data_marshaller`/`data_unmarshaller` for non-JSON payloads.

| [INDEX] | [SURFACE]                                                      | [SHAPE] | [CAPABILITY]         |
| :-----: | :------------------------------------------------------------- | :------ | :------------------- |
|  [01]   | `to_structured(event, data_marshaller=None)`                   | static  | header/body tuple    |
|  [02]   | `to_binary(event, data_marshaller=None)`                       | static  | binary header/body   |
|  [03]   | `to_json(event, data_marshaller=None)`                         | static  | structured bytes     |
|  [04]   | `to_dict(event)`                                               | static  | event dictionary     |
|  [05]   | `from_json(event_type, data, data_unmarshaller=None)`          | static  | event from JSON      |
|  [06]   | `from_http(event_type, headers, data, data_unmarshaller=None)` | static  | event from HTTP      |
|  [07]   | `from_dict(event_type, event)`                                 | static  | event from mapping   |
|  [08]   | `is_binary(headers) -> bool`                                   | static  | content-mode verdict |

[ENTRYPOINT_SCOPE]: Kafka binding (`cloudevents.v1.kafka`)
- Both directions carry the broker-neutral `KafkaMessage(headers, key, value)`: binary maps attributes to `ce_`-prefixed headers with `data` as the value, structured packs the whole event as the JSON value. `key_mapper` derives the partition key, defaulting to the `partitionkey` extension attribute.

| [INDEX] | [SURFACE]                                                                                       | [SHAPE] | [CAPABILITY] |
| :-----: | :---------------------------------------------------------------------------------------------- | :------ | :----------- |
|  [01]   | `to_binary(event, data_marshaller=None, key_mapper=None)`                                       | static  | binary       |
|  [02]   | `to_structured(event, data_marshaller=None, envelope_marshaller=None, key_mapper=None)`         | static  | structured   |
|  [03]   | `from_binary(message, event_type=None, data_unmarshaller=None)`                                 | static  | decoded      |
|  [04]   | `from_structured(message, event_type=None, data_unmarshaller=None, envelope_unmarshaller=None)` | static  | decoded      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One event carries a `Mapping[str, Any]` attribute algebra over arbitrary `data`; every binding lowers that one shape to transport parts and reverses it, never re-implementing the spec attribute contract or JSON codec.

[STACKING]:
- `opentelemetry-api`(`.api/opentelemetry-api.md`): `propagate.inject(carrier, context, setter)` writes the active W3C trace and baggage into a carrier dict the notice owner folds into the `create(attributes, data)` extension keys; the SDK carries no OpenTelemetry dependency, so the wiring lives in the notice owner.
- `msgspec`(`.api/msgspec.md`)/`pydantic`(`.api/pydantic.md`): own the notice `data` shape — a settled JSON-compatible value `to_structured`/`to_binary` serialize into the event body.
- `xxhash`(`.api/xxhash.md`): owns the transmittal `ContentKey` the notice `subject` references; the event `id` mints as a distinct time-ordered `uuid7`, so replay identity and content identity never collide.
- `delivery/transmittal`: composes the notice as the terminal issue-closure member — `TransmittalEvidence` container digests and issued-register row references become event extension attributes keyed by the `ContentKey`.

[LOCAL_ADMISSION]:
- Notice composition stays within `cloudevents.v1.*`, so one event never crosses family-specific types or conversion contracts.

[RAIL_LAW]:
- Package: `cloudevents`
- Owns: the CloudEvents envelope — attribute algebra, content-mode conversion, JSON codec, HTTP and Kafka bindings
- Accept: a `cloudevents.v1.http.CloudEvent` from `create(attributes, data)` lowered by `to_structured`/`to_binary`, extension attributes carrying trace, register, and evidence fields
- Reject: a hand-rolled event dict, a bespoke `ce-` header map, or a broker client embedded here — transport ends at envelope bytes
