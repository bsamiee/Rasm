# [RASM_PERSISTENCE_API_CLOUDEVENTS_AMQP]

`CloudNative.CloudEvents.Amqp` is the CNCF `AMQP 1.0` protocol binding — one static `AmqpExtensions` class mapping a `CloudEvent` onto an `AMQPNetLite.Core` `Amqp.Message` and back in structured and binary content modes. Its `AMQP 1.0` message model is disjoint from the `AMQP 0-9-1` `RabbitMQ.Client` surface (`api-rabbitmq`): the two never share a message type. This binding is the AMQP-native half of the CloudEvents egress projection — the same `CloudEvent` the Kafka sink emits crosses an `AMQP 1.0` broker under its native binding with zero envelope fork.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `AMQP 1.0` protocol binding (`CloudNative.CloudEvents.Amqp`)

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [CAPABILITY]                                                         |
| :-----: | :--------------- | :--------------- | :------------------------------------------------------------------- |
|  [01]   | `AmqpExtensions` | extension static | `CloudEvent` ⇄ `Amqp.Message`; structured + binary encode and decode |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `AmqpExtensions` egress and ingress maps over `Amqp.Message`

| [INDEX] | [SURFACE]                                                      | [SHAPE] | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------------------- | :------ | :-------------------------------------------------------- |
|  [01]   | `ce.ToAmqpMessage(contentMode, formatter)`                     | static  | egress; both content modes, default `cloudEvents:` prefix |
|  [02]   | `ce.ToAmqpMessageWithUnderscorePrefix(contentMode, formatter)` | static  | egress; `cloudEvents_` prefix                             |
|  [03]   | `ce.ToAmqpMessageWithColonPrefix(contentMode, formatter)`      | static  | egress; `cloudEvents:` prefix, JMS-incompatible           |
|  [04]   | `message.ToCloudEvent(formatter, params extensions)`           | static  | ingress; decode `Amqp.Message`, `params` attrs            |
|  [05]   | `message.ToCloudEvent(formatter, IEnumerable<extensions>)`     | static  | ingress; `IEnumerable<CloudEventAttribute>` attrs         |
|  [06]   | `message.IsCloudEvent()`                                       | static  | predicate; content type or `cloudEvents_specversion`      |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- namespace `CloudNative.CloudEvents.Amqp` carries the single `AmqpExtensions` static class; the `Amqp.Message` carrier, its `ApplicationProperties`/`Properties`/`Data` sections, and the `Symbol`/`Map` value types are `AMQPNetLite.Core` (`Amqp`/`Amqp.Framing`/`Amqp.Types`), disjoint from the `AMQP 0-9-1` `IChannel`/`BasicProperties` surface `api-rabbitmq` owns.
- `ToAmqp*` selects placement by `ContentMode`: `Binary` writes each populated attribute as a `cloudEvents_<name>` entry in `ApplicationProperties`, the `Data` bytes in the body, and the inferred data content type in `Properties.ContentType`; `Structured` packs the whole event through `EncodeStructuredModeMessage` under the format row's own media type. `specversion` is always written and `datacontenttype` is excluded from the property map — so a broker header-filter selects on type and source and never on content type — while a `Uri` serialises through `ToString()` and a `DateTimeOffset` through its `UtcDateTime`, which is why `time` crosses UTC with its offset dropped; any other `ContentMode` throws `ArgumentOutOfRangeException`.
- `ToCloudEvent` reads the content type from `Properties.ContentType`, decodes the body through the formatter, and re-hydrates each `cloudEvents_`- or `cloudEvents:`-prefixed application property onto the event with the pre-declared extension-attribute set, so the consumer reads typed attributes rather than re-parsing property strings. Header-filtering brokers route on the `cloudEvents_type`/`cloudEvents_source` properties without parsing the body.
- Decode is prefix-AGNOSTIC on both entrypoints: `IsCloudEvent` probes the content type, then `cloudEvents_specversion`, then `cloudEvents:specversion`, and `ToCloudEvent` strips either form — the two prefixes are the same length, so one substring width serves both. Encode is not: the prefix is fixed by which of the three egress methods is called.
- TRAP: `ToAmqpMessage`'s own doc states that releases from March 2023 onward write `cloudEvents_`, while its body on the installed distribution routes to the private `cloudEvents:` overload. Doc and body disagree, the colon form is the JMS-incompatible one, and a caller trusting the doc ships it — which is why the underscore method is named explicitly rather than relied on as a default.
- Batch is ABSENT on this binding, and the prefix constants are `internal const`, so no fence can name them and each spelling is carried by the method it selects.

[STACKING]:
- `Version/egress` mints the `Version/ledger` `OpLogEntry` → `CloudEvent` through the branch owner at `Egress.Envelope` → `ce.ToAmqpMessageWithUnderscorePrefix(ContentMode.Binary, EventFormat.Json.Formatter)` → an awaited `AMQPNetLite.Core` `SenderLink.SendAsync(Message, TimeSpan)` inside the `amqp` binding row's own bounded in-flight window, the same single-path projection the `kafka` row takes; the callback send forms stay refused there because their outgoing queue carries no ceiling (`.api/api-amqpnetlite.md`).
- `ContentMode.Binary` is the load-bearing choice: the CloudEvents attributes stay in AMQP application properties so a broker routes on `cloudEvents_type`/`cloudEvents_source` without deserialising the op payload, and `dataclassification` beside the creation-time `traceparent` ride as rostered extension attributes — the one envelope crosses graded and traced, and the handling class a broker refuses is readable without opening the body.
- `RabbitMQ.Client` (`api-rabbitmq`) is the peer `AMQP 0-9-1` sink over its own `BasicProperties.Headers` carrier; both are separate binding rows over the one `CloudEvent`, this binding riding the distinct `AMQPNetLite.Core` `AMQP 1.0` transport.
- this binding projects the identical `CloudEvent` as the other CloudEvents egress sinks, so an `AMQP 1.0` consumer joins the CDC fan under the same envelope the changefeed emits.

[LOCAL_ADMISSION]:
- Egress composes `ce.ToAmqpMessageWithUnderscorePrefix(ContentMode.Binary, EventFormat.Json.Formatter)`; extension declarations derive from generated `event.Extensions` descriptors at the producer/consumer projection, never a kernel roster or hand-spelled catalog.
- `EventFormat` supplies the formatter instance from the branch owner's one row; serializer and document options fix at that construction, never per message and never per transport.
- `ToAmqpMessageWithUnderscorePrefix` is the admitted egress; `ToAmqpMessage` and `ToAmqpMessageWithColonPrefix` write the JMS-incompatible `cloudEvents:` form.
