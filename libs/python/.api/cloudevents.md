# [PY_BRANCH_API_CLOUDEVENTS]

`cloudevents` is the CNCF Python distribution carrying TWO disjoint trees that never import each other. `cloudevents.core` is the validating tree: a typed `CloudEvent` per spec version validating required, optional, and extension attributes into one aggregating `CloudEventValidationError`, a `Format` protocol whose `write_data`/`read_data` pair is the binary-mode payload seam, and four protocol bindings lowering one event onto transport parts. `cloudevents.v1` is the frozen legacy tree: a mutable dict-backed event whose constructor checks a required-NAME subset and nothing else, a converter/marshaller stack over it, and a pydantic mirror. Every binding returns a transport-neutral value and the distribution holds no broker or HTTP client, so its reach ends at message envelope bytes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cloudevents`
- package: `cloudevents` (Apache-2.0)
- module: `cloudevents`
- namespaces: `cloudevents.core.{spec,base,exceptions}`, `cloudevents.core.{v1,v03}.event`, `cloudevents.core.formats.{base,json}`, `cloudevents.core.bindings.{common,http,kafka,amqp,rabbitmq}`, `cloudevents.v1.{abstract,conversion,exceptions}`, `cloudevents.v1.{http,kafka,pydantic}`, `cloudevents.v1.sdk.{marshaller,types,exceptions}`, `cloudevents.v1.sdk.{converters,event}`
- target: pure-Python wheel, no native asset; `py.typed` at `cloudevents/` and `cloudevents/v1/`
- runtime deps: `deprecation`, `python-dateutil` — `dateutil.parser.isoparse` is the RFC-3339 reader every binding's `time` decode runs through
- rail: event-envelope

`cloudevents/__init__.py` exports `__version__` alone — no symbol, no `__all__`, no re-export — so `assay api --key cloudevents` resolves empty and every member claim reads the module source. `cloudevents/v1/__init__.py` carries its own frozen `__version__` for the legacy tree.

## [02]-[PUBLIC_TYPES]

[CORE_EVENT_SCOPE]: `cloudevents.core` — the validating family

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :----------------------------------- | :------------ | :------------------------------------------------------------------------ |
|  [01]   | `core.base.BaseCloudEvent`           | protocol      | the read contract both versions subclass explicitly                       |
|  [02]   | `core.base.EventFactory`             | type alias    | `Callable[[dict, dict \| str \| bytes \| None], BaseCloudEvent]`          |
|  [03]   | `core.v1.event.CloudEvent`           | class         | v1.0 message envelope validating required, optional, and extension names  |
|  [04]   | `core.v03.event.CloudEvent`          | class         | v0.3 message envelope; `schemaurl`+`datacontentencoding` for `dataschema` |
|  [05]   | `core.v1.event.REQUIRED_ATTRIBUTES`  | `list[str]`   | `["id", "source", "type", "specversion"]`; `core.v03` twins it            |
|  [06]   | `core.v1.event.OPTIONAL_ATTRIBUTES`  | `list[str]`   | `["datacontenttype", "dataschema", "subject", "time"]`                    |
|  [07]   | `core.v03.event.OPTIONAL_ATTRIBUTES` | `list[str]`   | swaps `dataschema` for `schemaurl`, adds `datacontentencoding`            |
|  [08]   | `core.spec.SpecVersion`              | type alias    | `Literal["1.0", "0.3"]`                                                   |
|  [09]   | `core.spec.SPECVERSION_V1_0` `_V0_3` | `str`         | the two version literals the validators compare against                   |

[CORE_FORMAT_SCOPE]: `cloudevents.core.formats` — the pluggable codec

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]  | [CAPABILITY]                                                         |
| :-----: | :------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `core.formats.base.Format`             | protocol       | `read` `write` `write_data` `read_data` `get_content_type`           |
|  [02]   | `core.formats.json.JSONFormat`         | class          | the one shipped `Format`; `application/cloudevents+json`             |
|  [03]   | `JSONFormat.CONTENT_TYPE`              | `str`          | `"application/cloudevents+json"`, the structured-mode media type     |
|  [04]   | `JSONFormat.DEFAULT_CONTENT_TYPE`      | `str`          | `"application/json"`, the payload default when none is declared      |
|  [05]   | `JSONFormat.JSON_CONTENT_TYPE_PATTERN` | `Pattern[str]` | `^(application\|text)/([a-zA-Z0-9\-\.]+\+)?json(;.*)?$` payload gate |

[CORE_BINDING_SCOPE]: `cloudevents.core.bindings` — one frozen message dataclass per protocol

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY] | [FIELDS]                                                             |
| :-----: | :--------------------------------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `core.bindings.http.HTTPMessage`         | dataclass     | `headers: dict[str, str]`, `body: bytes`                             |
|  [02]   | `core.bindings.kafka.KafkaMessage`       | dataclass     | `headers: dict[str, bytes]`, `key: str\|bytes\|None`, `value: bytes` |
|  [03]   | `core.bindings.amqp.AMQPMessage`         | dataclass     | `properties`, `application_properties: dict[str, Any]`               |
|  [04]   | `core.bindings.rabbitmq.RabbitMQMessage` | dataclass     | `headers: dict[str, str]`, `content_type: str\|None`, `body: bytes`  |
|  [05]   | `core.bindings.kafka.KeyMapper`          | type alias    | `Callable[[BaseCloudEvent], str \| bytes \| None]`                   |

Every message dataclass is `@dataclass(frozen=True)`; its container fields are plain mutable `dict`, so freezing binds the reference alone. `AMQPMessage` carries its payload in `application_data: bytes`.

[BINDING_PREFIX]: distinct prefix families across the bindings, each a module-level `Final[str]`

| [INDEX] | [MODULE]                 | [CONSTANT]                                 | [LITERAL]                       | [CONTENT_TYPE_CARRIER]   |
| :-----: | :----------------------- | :----------------------------------------- | :------------------------------ | :----------------------- |
|  [01]   | `core.bindings.http`     | `CE_PREFIX`                                | `ce-`                           | `content-type` header    |
|  [02]   | `core.bindings.rabbitmq` | `CE_PREFIX`                                | `ce-`                           | the `content_type` field |
|  [03]   | `core.bindings.kafka`    | `CE_PREFIX`                                | `ce_`                           | `content-type` header    |
|  [04]   | `core.bindings.amqp`     | `CE_PREFIX_UNDERSCORE` / `CE_PREFIX_COLON` | `cloudEvents_` / `cloudEvents:` | `CONTENT_TYPE_PROPERTY`  |

`rabbitmq` seats its content type in the message's own `content_type` field and never in a header, `kafka` writes every header value as UTF-8 bytes, and `amqp` seats `CONTENT_TYPE_PROPERTY` inside `properties`. `core.bindings.kafka.PARTITIONKEY_ATTR` is `"partitionkey"`. `core.bindings.http._CE_SAFE_CHARS` is the printable-ASCII set less space, `"`, and `%` — HTTP's own percent-encoding safe set, WIDER than `common.encode_header_value`'s `safe=""`, so the shared encoder escapes strictly more than the HTTP binding does and the two spellings of one attribute value diverge.

[CORE_FAULTS]: `cloudevents.core.exceptions`, rooted at `BaseCloudEventException(Exception)`

The four leaf findings expose `attribute_name` but no stable code, tag, enum, or other discriminant. Their exception class is therefore the only non-message finding identity available to a boundary fault; `str(finding)` is mutable diagnostic prose rather than an identity.

| [INDEX] | [SYMBOL]                        | [BASES]                               | [CARRIES]                                           |
| :-----: | :------------------------------ | :------------------------------------ | :-------------------------------------------------- |
|  [01]   | `BaseCloudEventException`       | `Exception`                           | the tree root                                       |
|  [02]   | `CloudEventValidationError`     | `BaseCloudEventException`             | `.errors: dict[str, list[BaseCloudEventException]]` |
|  [03]   | `MissingRequiredAttributeError` | `BaseCloudEventException, ValueError` | `.attribute_name`                                   |
|  [04]   | `CustomExtensionAttributeError` | `BaseCloudEventException, ValueError` | `.attribute_name` beside the grammar message        |
|  [05]   | `InvalidAttributeTypeError`     | `BaseCloudEventException, TypeError`  | `.attribute_name` beside the expected type          |
|  [06]   | `InvalidAttributeValueError`    | `BaseCloudEventException, ValueError` | `.attribute_name` beside the value message          |

[LEGACY_SCOPE]: `cloudevents.v1` — the frozen non-validating family

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :------------------------------------ | :------------ | :-------------------------------------------------------------- |
|  [01]   | `v1.abstract.CloudEvent`              | class         | mapping contract: `get`/`__getitem__`/`__iter__`/`__contains__` |
|  [02]   | `v1.http.CloudEvent`                  | class         | dict-backed MUTABLE event; `__setitem__`/`__delitem__`          |
|  [03]   | `v1.pydantic.CloudEvent`              | class         | pydantic mirror; the `__init__` dispatches on installed major   |
|  [04]   | `v1.kafka.conversion.KafkaMessage`    | `NamedTuple`  | `headers` `key` `value` — the legacy twin of the core dataclass |
|  [05]   | `v1.kafka.conversion.KeyMapper`       | type alias    | `Callable[[AnyCloudEvent], AnyStr]` — the legacy twin           |
|  [06]   | `v1.sdk.types.MarshallerType`         | type alias    | `Callable[[Any], AnyStr]`                                       |
|  [07]   | `v1.sdk.types.UnmarshallerType`       | type alias    | `Callable[[AnyStr], Any]`                                       |
|  [08]   | `v1.sdk.types.SupportsDuplicateItems` | protocol      | `items()` widening for duplicate-key header mappings            |
|  [09]   | `v1.sdk.marshaller.HTTPMarshaller`    | class         | converter registry over `FromRequest`/`ToRequest`               |
|  [10]   | `v1.sdk.converters.base.Converter`    | class         | `TYPE` beside `read`/`write`/`can_read`/`event_supported`       |
|  [11]   | `v1.sdk.event.attribute.SpecVersion`  | `str` enum    | `v0_3` / `v1_0` — a THIRD version vocabulary beside `core.spec` |
|  [12]   | `v1.sdk.event.{v1,v03}.Event`         | class         | `Option`-slotted fluent builder behind the marshaller           |

[LEGACY_FAULTS]: `v1.exceptions.GenericException` roots `MissingRequiredFields` `InvalidRequiredFields` `InvalidStructuredJSON` `InvalidHeadersFormat` `DataMarshallerError` `DataUnmarshallerError` `IncompatibleArgumentsError` `PydanticFeatureNotInstalled`; `v1.kafka.exceptions.KeyMapperError` extends it. `v1.sdk.exceptions` mints five BARE `Exception` subclasses joined to no root — `UnsupportedEvent` `InvalidDataUnmarshaller` `InvalidDataMarshaller` `NoSuchConverter` `UnsupportedEventConverter`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: message envelope construction and read (`cloudevents.core`)

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `CloudEvent(attributes, data=None)`                                     | ctor     | validate-or-raise; seeds `specversion`/`id`/`time` |
|  [02]   | `get_id()` `get_source()` `get_type()`                                  | instance | the three required string reads, non-optional      |
|  [03]   | `get_specversion()`                                                     | instance | the version literal the format dispatches on       |
|  [04]   | `get_datacontenttype()` `get_dataschema()`                              | instance | `str \| None`                                      |
|  [05]   | `get_subject()`                                                         | instance | `str \| None`                                      |
|  [06]   | `get_time()`                                                            | instance | `datetime \| None` — a real datetime, never text   |
|  [07]   | `get_extension(extension_name)`                                         | instance | `Any`; an unknown name reads `None`                |
|  [08]   | `get_data()`                                                            | instance | `dict[str, Any] \| str \| bytes \| None`           |
|  [09]   | `get_attributes()`                                                      | instance | the LIVE internal dict, uncopied and unproxied     |
|  [10]   | `core.v03.event.CloudEvent.get_datacontentencoding()` `get_schemaurl()` | instance | the two v0.3-only reads                            |

[ENTRYPOINT_SCOPE]: format codec (`cloudevents.core.formats`)

| [INDEX] | [SURFACE]                                  | [SHAPE]  | [CAPABILITY]                                                             |
| :-----: | :----------------------------------------- | :------- | :----------------------------------------------------------------------- |
|  [01]   | `Format.read(event_factory, data)`         | protocol | structured message envelope to event; `None` factory detects the version |
|  [02]   | `Format.write(event)`                      | protocol | structured message envelope to bytes                                     |
|  [03]   | `Format.write_data(data, datacontenttype)` | protocol | PAYLOAD alone to bytes — the binary-mode seam                            |
|  [04]   | `Format.read_data(body, datacontenttype)`  | protocol | PAYLOAD alone from bytes — the binary-mode seam                          |
|  [05]   | `Format.get_content_type()`                | protocol | the structured-mode media type                                           |

[ENTRYPOINT_SCOPE]: protocol bindings (`cloudevents.core.bindings`) — one uniform shape across all four modules

| [INDEX] | [SURFACE]                                                           | [SHAPE] | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------------------ | :------ | :----------------------------------------------------- |
|  [01]   | `to_binary(event, event_format)`                                    | module  | attributes to prefixed headers, payload to body        |
|  [02]   | `to_structured(event, event_format)`                                | module  | whole message envelope into the body, one media header |
|  [03]   | `from_binary(message, event_format, event_factory=None)`            | module  | prefixed headers back to attributes                    |
|  [04]   | `from_structured(message, event_format, event_factory=None)`        | module  | body back through `Format.read`                        |
|  [05]   | `from_<protocol>(message, event_format, event_factory=None)`        | module  | content-mode detection then the matching leg           |
|  [06]   | `to_binary_event` `to_structured_event`                             | module  | `event_format` defaulting a fresh `JSONFormat()`       |
|  [07]   | `from_binary_event` `from_structured_event` `from_<protocol>_event` | module  | `http`/`kafka` pass none; `amqp`/`rabbitmq` bind v1    |

Kafka alone widens the two `to_*` legs with `key_mapper: KeyMapper | None = None`; `_default_key_mapper` reads the `partitionkey` extension and coerces a non-`str`/`bytes` value through `str`. Content-mode detection splits by family: `http` and `kafka` read the prefix off the header names, `amqp` and `rabbitmq` read `content-type` for the `application/cloudevents` stem.

[ENTRYPOINT_SCOPE]: version dispatch (`cloudevents.core.bindings.common`)

| [INDEX] | [SURFACE]                                                | [SHAPE]      | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------- | :----------- | :--------------------------------------------------- |
|  [01]   | `get_event_factory_for_version(specversion)`             | static       | `"0.3"` to the v0.3 class, EVERY other value to v1.0 |
|  [02]   | `encode_header_value(value)`                             | static       | `quote(..., safe="")` with the datetime `Z` rewrite  |
|  [03]   | `decode_header_value(attr_name, value)`                  | static       | `unquote`, then `isoparse` for `time` alone          |
|  [04]   | `TIME_ATTR` `CONTENT_TYPE_HEADER` `DATACONTENTTYPE_ATTR` | `Final[str]` | `"time"` `"content-type"` `"datacontenttype"`        |

[ENTRYPOINT_SCOPE]: legacy marshaller and conversion (`cloudevents.v1`)

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `v1.sdk.marshaller.NewDefaultHTTPMarshaller()`                               | factory  | the structured-then-binary converter pair    |
|  [02]   | `v1.sdk.marshaller.NewHTTPMarshaller(converters)`                            | factory  | a caller-ordered converter sequence          |
|  [03]   | `HTTPMarshaller.FromRequest(event, headers, body, data_unmarshaller=None)`   | instance | first converter whose `can_read` admits      |
|  [04]   | `HTTPMarshaller.ToRequest(event, converter_type=None, data_marshaller=None)` | instance | `converter_type` defaults to slot zero       |
|  [05]   | `v1.conversion.to_structured(event, data_marshaller=None)`                   | static   | `(headers, body)` tuple                      |
|  [06]   | `v1.conversion.to_binary(event, data_marshaller=None)`                       | static   | `(headers, body)` tuple                      |
|  [07]   | `v1.conversion.to_json(event, data_marshaller=None)`                         | static   | structured bytes                             |
|  [08]   | `v1.conversion.from_json(event_type, data, data_unmarshaller=None)`          | static   | event from JSON                              |
|  [09]   | `v1.conversion.from_http(event_type, headers, data, data_unmarshaller=None)` | static   | event from HTTP parts                        |
|  [10]   | `v1.conversion.from_dict(event_type, event)` / `to_dict(event)`              | static   | mapping round trip                           |
|  [11]   | `v1.sdk.converters.util.has_binary_headers(headers)`                         | static   | ALL `ce-specversion`/`-source`/`-type`/`-id` |

`data_marshaller` and `data_unmarshaller` are the two callable seams the legacy tree threads: `MarshallerType` rides every `to_*` leg positionally after the event and defaults `None`; `UnmarshallerType` rides every `from_*` leg last and defaults `None` at the public surface while `Converter.read`, `BaseEvent.UnmarshalJSON`, and `BaseEvent.UnmarshalBinary` each REQUIRE it. `HTTPMarshaller.FromRequest` defaults it to `json.loads` and `BaseEvent.MarshalBinary` defaults its marshaller to `json.dumps`; `v1.conversion.from_http` defaults to the module-private `_json_or_string`. Either slot rejects a non-callable, raising `InvalidDataMarshaller`/`InvalidDataUnmarshaller`. `v1.kafka.conversion` alone adds `envelope_marshaller`/`envelope_unmarshaller` for the structured leg.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `cloudevents.core` imports nothing from `cloudevents.v1` and the reverse holds, so the two trees are disjoint and a family choice is total — a `core.v1.event.CloudEvent` never crosses a `v1.conversion` leg and a `v1.http.CloudEvent` never satisfies `BaseCloudEvent`.
- Validation seats in `core` alone. `core.v1.event.CloudEvent.__init__` runs three static validators accumulating into one `defaultdict(list)` keyed by attribute name and raises a single `CloudEventValidationError` carrying every finding — presence, emptiness, and `isinstance` per required attribute; `datetime`-ness and timezone-awareness of `time`; `str`-ness and non-emptiness of `subject`/`datacontenttype`/`dataschema`; and the extension grammar over every unrecognized key.
- `v1.http.CloudEvent.__init__` checks exactly two things: that `specversion` names a known version, and that `{"id", "source", "type", "specversion"} <= attributes.keys()`. It validates no type, no emptiness, no URI shape, no RFC-3339 parse, and no extension name; it lowercases every key, stores `time` as an ISO STRING rather than a `datetime`, and exposes `__setitem__`/`__delitem__` so a constructed event mutates past every check it did run.
- Extension names admit through `re.match(r"^[a-z0-9]+$", name)`, a one-character floor, and refusal of the reserved `"data"` name — a CHARSET rule with no length ceiling, so a name past the specification's twenty-character bound passes here.
- `CloudEvent.__init__` MUTATES the `attributes` mapping it is handed, writing the `specversion`, `id`, and `time` defaults into the caller's own dict before validating it; `get_attributes()` then returns that same live dict uncopied, and `core.bindings.kafka.from_structured` relies on the aliasing to inject `partitionkey` before rebuilding the event through a second full validation pass.
- `BaseCloudEvent.get_data`, `EventFactory`, and `CloudEvent.__init__` annotate data as `dict | str | bytes | None`, but the constructor performs no data type or value validation and stores the object untouched. A generated protobuf `Any` therefore works at runtime while the public annotation rejects it; the protobuf format must widen that one proven constructor call explicitly and bind the v1 factory, never invent a second event carrier.
- Batch code is absent whole: no batch media type, no batch encoder, no batch branch in `JSONFormat`, and no occurrence of the token anywhere in the tree — batch framing is branch-owned, never a package gap to route around.
- `JSONFormat` is the one concrete `Format` and is never a singleton — every `*_event` convenience wrapper constructs a fresh `JSONFormat()` per call.
- `write_data`/`read_data` carry the binary content mode and `write`/`read` the structured one, so a `Format` implementation missing the payload pair breaks every binding's binary leg while its structured leg still runs.
- Value handling diverges per binding: `http` percent-encodes over its own WIDER safe set and skips `None`-valued attributes, `kafka` UTF-8-encodes every header value to bytes and skips `None`, `rabbitmq` writes plain unencoded strings and skips `None`, while `amqp` preserves native `bool`/`int`, writes `time` as a MILLISECOND epoch integer, and skips no `None` at all.
- Auto-detection splits at the `*_event` wrappers: `http` and `kafka` pass a `None` factory so a `SPECVERSION_V0_3` payload decodes to `core.v03.event.CloudEvent`, while `amqp` and `rabbitmq` hard-bind `core.v1.event.CloudEvent`, whose `_validate_required_attributes` REFUSES any `specversion` but `1.0` — a `SPECVERSION_V0_3` payload raises `CloudEventValidationError` there rather than crossing as a v1 event.
- `get_event_factory_for_version` falls through to `core.v1.event.CloudEvent` for every unknown version string, so an unrecognized `specversion` decodes rather than refusing.
- Ten legacy functions across `v1.http.{http_methods,json_methods,event_type,util}` carry `@deprecated`; the `deprecation` runtime dependency exists for them alone. `core.bindings.common.encode_header_value`/`decode_header_value` are public with zero in-package callers.
- Three version vocabularies coexist — `core.spec.SpecVersion` (`Literal`), `v1.sdk.event.attribute.SpecVersion` (`str` enum), and `v1.conversion._obj_by_version` (a dict) — and only the first is the `core` tree's.

[STACKING]:
- `msgspec`(`.api/msgspec.md`): opaque payload bytes cross binary mode unchanged; JSON batch decoding uses `list[msgspec.Raw]` only to preserve each complete JSON event object's bytes before `JSONFormat.read` admits it.
- `protobuf-py`(`.api/protobuf-py.md`): the sealed payload union also admits a generated `Message`; protobuf structured mode packs it into `Any`, while decode retains the generated `Any` and its `type_url` for registry resolution.
- Rasm-profile payload support is explicit per event-format row: JSON/Avro admit opaque bytes, while protobuf also admits generated `Message`; an unsupported arm refuses before codec invocation.
- The generic Avro adapter retains the publisher AVSC's complete recursive JSON-value union; its wire-only record wrappers disappear before package `CloudEvent` construction, and profile admission narrows later.
- Runtime `EventFormat.write`/`decode` expose strict generic v1 single and batch capability over the package event protocol; `encode`/`admit` compose the generated Rasm profile without replacing those entries.
- The CloudEvents `ce_integer` abstract type is signed 32-bit in both publisher protobuf and Avro schemas, so a wider generated scalar re-enters that ceiling before mint.
- Binary `write_data`/`read_data` are an SDK binding mechanism, not a structured event-format choice. Compression applies after the complete carrier body at the binding/residence owner; no identity frame or marshaller twin sits inside a format row.
- `opentelemetry-api`(`.api/opentelemetry-api.md`): `propagate.inject`/`propagate.extract` over the attribute mapping carry the CREATION-time W3C context as extension attributes; the distribution declares no OpenTelemetry dependency, so both directions are branch-wired.
- `confluent-kafka`(`.api/confluent-kafka.md`): `KafkaMessage.headers`/`key`/`value` map onto `Producer.produce(headers=, key=, value=)` and back off `Message.headers()`/`key()`/`value()`; the SDK opens no connection, so the client leg is the branch's.
- `pika`(`.api/pika.md`): `RabbitMQMessage.headers`/`content_type`/`body` map onto `BasicProperties(headers=, content_type=)` and `basic_publish(body=)`.
- `python-dateutil`: `isoparse` is the only RFC-3339 reader in the tree, reached through `common.decode_header_value` and `JSONFormat.read`.

[LOCAL_ADMISSION]:
- `cloudevents.core` is the admitted validating family at every seam; the `v1` legacy tree, its converters, its marshaller stack, and its pydantic mirror are refused whole — a second event type, a second `KafkaMessage`, a second `KeyMapper`, and a third version vocabulary each fork a concept the branch owns once.
- Extension names admit through the branch roster before construction, since the package's charset check carries no length ceiling and no roster.
- Every `attributes` mapping handed to the constructor is freshly built per mint and never a value the caller retains, because construction writes defaults into it.
- Every raise crosses one `boundary` fence into `BoundaryFault`; `CloudEventValidationError.errors` spreads into the aggregate rather than collapsing to its `__str__`.

[RAIL_LAW]:
- Package: `cloudevents`
- Owns: the spec attribute algebra, its validation, the structured/binary content-mode split, the JSON format, and the four protocol bindings' header and property lowering
- Accept: `core.v1.event.CloudEvent`, `core.formats.base.Format` with `core.formats.json.JSONFormat`, the four `core.bindings` modules, `core.exceptions`
- Reject: the whole `cloudevents.v1` tree; a hand-rolled `ce-`/`ce_`/`cloudEvents_` header map beside a binding that owns it; a broker or HTTP client here — reach ends at message envelope bytes
