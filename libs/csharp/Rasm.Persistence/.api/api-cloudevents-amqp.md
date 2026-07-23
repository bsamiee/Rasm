# [RASM_PERSISTENCE_API_CLOUDEVENTS_AMQP]

`CloudNative.CloudEvents.Amqp` is the CNCF `AMQP 1.0` protocol binding — one static `AmqpExtensions` class mapping a `CloudEvent` onto an `AMQPNetLite.Core` `Amqp.Message` and back in structured and binary content modes. Its `AMQP 1.0` message model is disjoint from the `AMQP 0-9-1` `RabbitMQ.Client` surface (`api-rabbitmq`): the two never share a message type. This binding is the AMQP-native half of the CloudEvents egress projection — the same `CloudEvent` the Kafka sink emits crosses an `AMQP 1.0` broker under its native binding with zero envelope fork.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.Amqp`
- package: `CloudNative.CloudEvents.Amqp` (Apache-2.0)
- assembly: `CloudNative.CloudEvents.Amqp`; namespace `CloudNative.CloudEvents.Amqp`
- asset: pure-managed, no native asset or RID burden
- depends: `CloudNative.CloudEvents` (`api-cloudevents`) and `AMQPNetLite.Core` (`Amqp.Message` transport — assembly `Amqp.Net.dll`, namespaces `Amqp`/`Amqp.Framing`/`Amqp.Types`)
- rail: sync-egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `AMQP 1.0` protocol binding (`CloudNative.CloudEvents.Amqp`)

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [CAPABILITY]                                                         |
| :-----: | :--------------- | :--------------- | :------------------------------------------------------------------- |
|  [01]   | `AmqpExtensions` | extension static | `CloudEvent` ⇄ `Amqp.Message`; structured + binary encode and decode |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `AmqpExtensions` egress and ingress maps over `Amqp.Message`

| [INDEX] | [SURFACE]                                                      | [SHAPE] | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------------------- | :------ | :-------------------------------------------------------- |
|  [01]   | `ce.ToAmqpMessage(contentMode, formatter)`                     | static  | egress; both content modes, default `cloudEvents:` prefix |
|  [02]   | `ce.ToAmqpMessageWithUnderscorePrefix(contentMode, formatter)` | static  | egress; `cloudEvents_` prefix                             |
|  [03]   | `ce.ToAmqpMessageWithColonPrefix(contentMode, formatter)`      | static  | egress; `cloudEvents:` prefix, JMS-incompatible           |
|  [04]   | `message.ToCloudEvent(formatter, params extensions)`           | static  | ingress; decode `Amqp.Message`, `params` attrs            |
|  [05]   | `message.ToCloudEvent(formatter, IEnumerable<extensions>)`     | static  | ingress; `IEnumerable<CloudEventAttribute>` attrs         |
|  [06]   | `message.IsCloudEvent()`                                       | static  | predicate; content type or `cloudEvents_specversion`      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- namespace `CloudNative.CloudEvents.Amqp` carries the single `AmqpExtensions` static class; the `Amqp.Message` carrier, its `ApplicationProperties`/`Properties`/`Data` sections, and the `Symbol`/`Map` value types are `AMQPNetLite.Core` (`Amqp`/`Amqp.Framing`/`Amqp.Types`), disjoint from the `AMQP 0-9-1` `IChannel`/`BasicProperties` surface `api-rabbitmq` owns.
- `ToAmqp*` selects placement by `ContentMode`: `Binary` writes each populated attribute as a `cloudEvents_<name>` entry in `ApplicationProperties`, the `Data` bytes in the body, and the inferred data content type in `Properties.ContentType`; `Structured` packs the whole event through `EncodeStructuredModeMessage` under `application/cloudevents+json`. `specversion` is always written and `datacontenttype` is excluded from the property map; a `Uri` serialises through `ToString()` and a `DateTimeOffset` through its `UtcDateTime`; any other `ContentMode` throws `ArgumentOutOfRangeException`.
- `ToCloudEvent` reads the content type from `Properties.ContentType`, decodes the body through the formatter, and re-hydrates each `cloudEvents_`- or `cloudEvents:`-prefixed application property onto the event with the pre-declared extension-attribute set, so the consumer reads typed attributes rather than re-parsing property strings. A header-filtering broker routes on the `cloudEvents_type`/`cloudEvents_source` properties without parsing the body.

[STACKING]:
- `Version/egress` projects the `Version/ledger` `OpLogEntry` → `CloudEvent` via the `Egress.Envelope` projector → `ce.ToAmqpMessageWithUnderscorePrefix(ContentMode.Binary, formatter)` → an `AMQPNetLite.Core` `SenderLink.Send` over the broker connection, the same one-rail projection as the Kafka sink.
- `ContentMode.Binary` is the load-bearing choice: the CloudEvents attributes stay in AMQP application properties so a broker routes on `cloudEvents_type`/`cloudEvents_source` without deserialising the op payload, and the redaction flag and `traceparent` ride as extension attributes — the one envelope crosses masked and traced.
- `RabbitMQ.Client` (`api-rabbitmq`) is the peer `AMQP 0-9-1` sink over its own `BasicProperties.Headers` carrier; both are separate `EgressSink` rows over the one `CloudEvent`, this binding riding the distinct `AMQPNetLite.Core` `AMQP 1.0` transport.
- this binding projects the identical `CloudEvent` as the other CloudEvents egress sinks, so an `AMQP 1.0` consumer joins the CDC fan under the same envelope the changefeed emits.

[LOCAL_ADMISSION]:
- egress composes `ce.ToAmqpMessageWithUnderscorePrefix(ContentMode.Binary, formatter)`; the extension attributes (`traceparent`, `redacted`, `sequence`) are declared once via `CloudEventAttribute.CreateExtension` and read back on ingress through the `ToCloudEvent` overload with the identical attribute enumerable.
- a single shared `JsonEventFormatter`/`JsonEventFormatter<T>` instance encodes and decodes every message; the serializer options are fixed at formatter construction, never per message.
- `ToAmqpMessageWithUnderscorePrefix` is the admitted egress; `ToAmqpMessage` and `ToAmqpMessageWithColonPrefix` write the JMS-incompatible `cloudEvents:` form.

[RAIL_LAW]:
- Package: `CloudNative.CloudEvents.Amqp`
- Owns: the CloudEvents `AMQP 1.0` protocol binding — `CloudEvent` ⇄ `Amqp.Message` for an `AMQP 1.0` `EgressSink` over the `AMQPNetLite.Core` transport
- Accept: `ToAmqpMessageWithUnderscorePrefix`/`ToCloudEvent` with an injected shared `CloudEventFormatter`, `ContentMode.Binary`, and pre-declared extension attributes carrying trace and redaction
- Reject: colon-prefix egress, a per-message formatter instance, hand-rolled `cloudEvents_` application-property construction over a raw `Amqp.Message`, conflation with the `AMQP 0-9-1` `RabbitMQ.Client` `Deliver` leg, or a per-transport envelope shape parallel to the shared `CloudEvent` projection
