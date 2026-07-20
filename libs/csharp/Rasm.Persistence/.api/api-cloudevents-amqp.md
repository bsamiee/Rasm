# [RASM_PERSISTENCE_API_CLOUDEVENTS_AMQP]

`CloudNative.CloudEvents.Amqp` is the CNCF `AMQP 1.0` protocol binding: one static
`AmqpExtensions` class mapping a `CloudEvent` onto an `AMQPNetLite.Core` `Amqp.Message` and
back, in both structured and binary content modes. It closes the held CloudEvents binding
family for Persistence — the core `CloudEvent`/`CloudEventFormatter` envelope algebra and the
`JsonEventFormatter` System.Text.Json codec are the `api-cloudevents` overlay and are NOT
restated here; this overlay owns only the `AMQP 1.0` wire seam. Its carrier is AMQPNetLite's
`Amqp.Message` over brokers such as Azure Service Bus, ActiveMQ Artemis, and RabbitMQ's
native `AMQP 1.0` endpoint, a different protocol from `RabbitMQ.Client`'s `AMQP 0-9-1` client
(`api-rabbitmq`): the two never share a message type, so this binding rides the `AMQP 1.0`
transport, while the 0-9-1 `EgressSink.RabbitMq` `Deliver` leg keeps its own
`BasicProperties.Headers` carrier. This binding is the AMQP-native half of the one CloudEvents
egress projection — the same `CloudEvent` the Kafka sink emits crosses an `AMQP 1.0` broker under
its native binding with zero envelope fork.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.Amqp`
- package: `CloudNative.CloudEvents.Amqp`
- license: `Apache-2.0` (`cloudevents/sdk-csharp`)
- assembly: `CloudNative.CloudEvents.Amqp`
- namespace: `CloudNative.CloudEvents.Amqp`
- asset: pure-managed library; no native asset, no RID burden
- depends: `CloudNative.CloudEvents` (core envelope/formatter, `api-cloudevents`) and `AMQPNetLite.Core` (the `Amqp.Message` `AMQP 1.0` transport — assembly `Amqp.Net.dll`, namespaces `Amqp`/`Amqp.Framing`/`Amqp.Types`)
- rail: sync-egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `AMQP 1.0` protocol binding (`CloudNative.CloudEvents.Amqp`)
- rail: sync-egress
- note: the whole assembly is one static class; the core `CloudEvent`, `CloudEventFormatter`, `ContentMode`, and `CloudEventAttribute` types it consumes are owned by `api-cloudevents`, and the `Amqp.Message` carrier is owned by the `AMQPNetLite.Core` transport dependency.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [CAPABILITY]                                                         |
| :-----: | :--------------- | :--------------- | :------------------------------------------------------------------- |
|  [01]   | `AmqpExtensions` | extension static | `CloudEvent` ⇄ `Amqp.Message`; structured + binary encode and decode |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `AmqpExtensions` egress and ingress maps
- rail: sync-egress
- note: six public members over `Amqp.Message`. `ToAmqpMessage` selects placement by `ContentMode` — `Structured` packs the whole event into the body under `application/cloudevents+json` via `EncodeStructuredModeMessage`; `Binary` places `Data` in the body under the inferred data content type via `EncodeBinaryModeEventData`. `ToAmqpMessage` writes `cloudEvents:` application properties, while `ToAmqpMessageWithUnderscorePrefix` writes the admitted `cloudEvents_` form. `specversion` is always mapped; the `datacontenttype` attribute is excluded from the application-property map.
- returns: each `ToAmqpMessage*` returns AMQPNetLite's `Amqp.Message`; both `ToCloudEvent` overloads return a validated `CloudEvent`.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `ce.ToAmqpMessage(contentMode, formatter)`                     | egress map     | both content modes; `cloudEvents:` prefix         |
|  [02]   | `ce.ToAmqpMessageWithUnderscorePrefix(contentMode, formatter)` | egress map     | explicit `cloudEvents_` prefix                    |
|  [03]   | `ce.ToAmqpMessageWithColonPrefix(contentMode, formatter)`      | egress map     | colon `cloudEvents:` prefix; JMS-incompatible     |
|  [04]   | `message.ToCloudEvent(formatter, params extensions)`           | ingress map    | decode an `Amqp.Message`; `params` attrs          |
|  [05]   | `message.ToCloudEvent(formatter, IEnumerable<extensions>)`     | ingress map    | decode; `IEnumerable<CloudEventAttribute>` attrs  |
|  [06]   | `message.IsCloudEvent()`                                       | predicate      | detects content type or `cloudEvents_specversion` |

## [04]-[IMPLEMENTATION_LAW]

[CLOUDEVENTS_AMQP_TOPOLOGY]:
- namespace `CloudNative.CloudEvents.Amqp` carries the single `AmqpExtensions` static class; the carrier `Amqp.Message`, its `ApplicationProperties`/`Properties`/`Data` sections, and the `Symbol`/`Map` value types are all `AMQPNetLite.Core` (`Amqp`/`Amqp.Framing`/`Amqp.Types`), never `RabbitMQ.Client` — the `AMQP 1.0` message model is disjoint from the `AMQP 0-9-1` `IChannel`/`BasicProperties` surface `api-rabbitmq` owns.
- `ToAmqpMessageWithUnderscorePrefix(ce, ContentMode.Binary, formatter)` writes each populated attribute as a `cloudEvents_<name>` entry in the message `ApplicationProperties`, the redacted `Data` bytes in the body, and the inferred data content type in `Properties.ContentType`; a `Uri` attribute serialises through `ToString()` and a `DateTimeOffset` through its `UtcDateTime`. A header-filtering `AMQP 1.0` broker routes on the `cloudEvents_type`/`cloudEvents_source` application properties without parsing the body — the binary-mode counterpart of the Kafka `ce_*` header binding.
- `ToAmqpMessageWithUnderscorePrefix(ce, ContentMode.Structured, formatter)` packs the whole event through `EncodeStructuredModeMessage` into the body under `application/cloudevents+json`; the `specversion` application property is still written, so `IsCloudEvent` detects the message by content type or by the `cloudEvents_specversion` property either way. Any `ContentMode` outside `Structured`/`Binary` throws `ArgumentOutOfRangeException`.
- `ToCloudEvent(message, formatter, extensions)` reads the content type from `Properties.ContentType`, decodes the body through the formatter, and re-hydrates each `cloudEvents_`- or `cloudEvents:`-prefixed application property back onto the event with the pre-declared extension-attribute set — so the consumer reads typed attributes rather than re-parsing property strings.
- Formatter is the injected `CloudEventFormatter` — a shared `JsonEventFormatter`/`JsonEventFormatter<T>` from `CloudNative.CloudEvents.SystemTextJson` (`api-cloudevents`), constructed once and reused; a per-message formatter instance is the rejected form.

[STACKING]:
- `AMQP 1.0` egress is the same one-rail projection as the Kafka sink: the `Version/ledger#CHANGEFEED` `OpLogEntry` → `CloudEvent` via the `Version/egress#EGRESS_SINK` `Egress.Envelope` projector → `ce.ToAmqpMessageWithUnderscorePrefix(ContentMode.Binary, formatter)` → an AMQPNetLite `SenderLink.Send` over the broker connection. One shared `JsonEventFormatter` encodes every event; there is no second envelope shape and no hand-built `cloudEvents_` property.
- `Binary` content mode is the load-bearing choice, exactly as on the Kafka leg: the CloudEvents attributes stay in AMQP application properties so a broker filters and routes on `cloudEvents_type`/`cloudEvents_source` without deserialising the op payload, and the redaction flag and `traceparent` ride as extension attributes rather than a side channel — the one envelope crosses masked and traced.
- protocol boundary vs the `AMQP 0-9-1` leg: this binding does NOT re-bind the `RabbitMQ.Client` `EgressSink.RabbitMq` `Deliver` leg — `RabbitMQ.Client` speaks `AMQP 0-9-1`, and its `BasicPublishAsync<TProperties>` carries no `Amqp.Message`. This `AMQP 1.0` binding is a distinct transport over `AMQPNetLite.Core`, while the `AMQP 0-9-1` sink keeps its `BasicProperties.Headers` attribute carrier under `api-rabbitmq`. Both sinks are separate `EgressSink` rows over the one CloudEvents envelope, never collapsed.
- CloudEvents envelope is the single cross-transport, cross-language egress vocabulary: the Kafka egress (`api-cloudevents`), the NATS/Pulsar sinks, and this `AMQP 1.0` binding project the identical `CloudEvent` shape, so an `AMQP 1.0` consumer joins the CDC fan under the same envelope the changefeed emits — a per-transport re-pack is the drift defect.

[LOCAL_ADMISSION]:
- egress composes `cloudEvent.ToAmqpMessageWithUnderscorePrefix(ContentMode.Binary, formatter)` at the `AMQP 1.0` sink; the extension attributes (`traceparent`, `redacted`, `sequence`) are the same set declared once via `CloudEventAttribute.CreateExtension` and read back on ingress through the `ToCloudEvent` overload with the identical attribute enumerable.
- Shared `JsonEventFormatter`/`JsonEventFormatter<T>` instance encodes and decodes every message; the serializer options are fixed at formatter construction, never per message.
- Underscore prefix is the admitted form — call sites use `ToAmqpMessageWithUnderscorePrefix`; `ToAmqpMessage` and `ToAmqpMessageWithColonPrefix` write the JMS-incompatible `cloudEvents:` form.

[RAIL_LAW]:
- Package: `CloudNative.CloudEvents.Amqp`
- Owns: the CloudEvents `AMQP 1.0` protocol binding — `CloudEvent` ⇄ `Amqp.Message` for an `AMQP 1.0` `EgressSink`, over the `AMQPNetLite.Core` transport
- Accept: `ToAmqpMessageWithUnderscorePrefix`/`ToCloudEvent` with an injected shared `CloudEventFormatter`, `ContentMode.Binary`, pre-declared extension attributes, and trace/redaction as extension attributes
- Reject: colon-prefix egress, a per-message formatter instance, hand-rolled CloudEvents application-property construction over a raw `Amqp.Message`, conflation with the `AMQP 0-9-1` `RabbitMQ.Client` `Deliver` leg, or a per-transport envelope shape parallel to the shared `CloudEvent` projection
