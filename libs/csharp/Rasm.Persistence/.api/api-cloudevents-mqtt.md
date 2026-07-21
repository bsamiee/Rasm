# [RASM_PERSISTENCE_API_CLOUDEVENTS_MQTT]

`CloudNative.CloudEvents.Mqtt` is the CNCF MQTT protocol binding: one static
`MqttExtensions` class mapping a `CloudEvent` onto an `MQTTnet` `MqttApplicationMessage` and
back, structured-mode ONLY. It closes the held CloudEvents binding family for Persistence — the
core `CloudEvent`/`CloudEventFormatter` envelope algebra and the `JsonEventFormatter`
System.Text.Json codec are the `api-cloudevents` overlay and are NOT restated here; this overlay
owns only the MQTT wire seam. It is the MQTT half of the one CloudEvents egress projection: the
same `CloudEvent` the Kafka and `AMQP 1.0` sinks emit crosses an MQTT v5 broker under its native
binding with zero envelope fork, backing the `Version/egress#EGRESS_SINK` `EgressSink.Mqtt`
delivery row. Unlike the Kafka and `AMQP 1.0` bindings, MQTT carries NO binary content mode — the
whole event always rides the payload body under `application/cloudevents+json`, so the header-route
optimisation the binary-mode legs use has no MQTT counterpart and topic/QoS/UserProperties are the
subscription's own metadata, not the binding's.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.Mqtt`
- package: `CloudNative.CloudEvents.Mqtt`
- license: `Apache-2.0` (`cloudevents/sdk-csharp`)
- assembly: `CloudNative.CloudEvents.Mqtt`
- namespace: `CloudNative.CloudEvents.Mqtt`
- frameworks: `net10.0`, `net8.0`, `netstandard2.0`; Persistence binds the `net10.0` asset
- asset: pure-managed library; no native asset, no RID burden
- depends: `CloudNative.CloudEvents` (core envelope/formatter, `api-cloudevents`) and `MQTTnet` (the estate `MqttApplicationMessage` transport, `api-mqtt`)
- state: whole assembly is one stateless static extension class; every member is a pure projection over caller-supplied arguments, so the surface is thread-safe and needs no single-owner admission — the only shared object is the injected `CloudEventFormatter`, whose concurrency is the formatter's own
- rail: sync-egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: MQTT protocol binding (`CloudNative.CloudEvents.Mqtt`)
- rail: sync-egress
- note: the whole assembly is one static class; the core `CloudEvent`, `CloudEventFormatter`, `ContentMode`, and `CloudEventAttribute` types it consumes are owned by `api-cloudevents`, and the `MqttApplicationMessage` carrier is owned by the `MQTTnet` transport dependency (`api-mqtt`).

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [CAPABILITY]                                                             |
| :-----: | :--------------- | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `MqttExtensions` | extension static | `CloudEvent` ⇄ `MqttApplicationMessage`; structured-mode encode + decode |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `MqttExtensions` egress and ingress maps
- rail: sync-egress
- note: three public members, all structured-mode. `ToMqttApplicationMessage` throws `ArgumentOutOfRangeException("contentMode", ...)` on any `ContentMode` other than `Structured` — the MQTT binding has NO binary-mode surface, unlike the Kafka and AMQP bindings. It validates a complete `CloudEvent` through `Validation.CheckCloudEventArgument` and the formatter through `Validation.CheckNotNull`. Both `ToCloudEvent` overloads validate non-null message and formatter through `Validation.CheckNotNull`; the `params` overload forwards to the `IEnumerable<CloudEventAttribute>?` overload, which decodes `message.PayloadSegment` through `CloudEventFormatter.DecodeStructuredModeMessage` with a null `ContentType`.
- returns: `ToMqttApplicationMessage` returns `MQTTnet`'s `MqttApplicationMessage`, setting `Topic` and `PayloadSegment`; both `ToCloudEvent` overloads return a validated `CloudEvent`.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [CAPABILITY]                                      |
| :-----: | :----------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `ce.ToMqttApplicationMessage(contentMode, formatter, topic)` | egress map     | structured-mode encode into payload; sets `Topic` |
|  [02]   | `message.ToCloudEvent(formatter, params extensions)`         | ingress map    | structured-mode decode; `params` extension attrs  |
|  [03]   | `message.ToCloudEvent(formatter, IEnumerable<extensions>)`   | ingress map    | decode; `IEnumerable<CloudEventAttribute>` attrs  |

## [04]-[IMPLEMENTATION_LAW]

[CLOUDEVENTS_MQTT_TOPOLOGY]:
- namespace `CloudNative.CloudEvents.Mqtt` carries the single `MqttExtensions` static class; the carrier type `MqttApplicationMessage` is `MQTTnet`, `MqttUserProperty` is `MQTTnet.Packets`, and `MqttQualityOfServiceLevel` is `MQTTnet.Protocol` — the transport model is `api-mqtt`'s, never the binding's.
- `ToMqttApplicationMessage(ce, ContentMode.Structured, formatter, topic)` packs the entire event — attributes and `Data` — into the message payload under `application/cloudevents+json` via `formatter.EncodeStructuredModeMessage`, writes it through `BinaryDataUtilities.GetArraySegment` onto `MqttApplicationMessage.PayloadSegment`, and sets `Topic`. It targets the MQTTnet v5 message type directly — the returned `MqttApplicationMessage` is the same object the `IMqttClient.PublishAsync` transport consumes, so the egress leg publishes it without a re-map. Any `contentMode` other than `Structured` throws `ArgumentOutOfRangeException` — binary-mode CloudEvents placement is not an MQTT-binding capability, so the MQTT sink has no `ce_type`/`ce_source` header-route form and a broker filters on topic alone.
- `ToCloudEvent(message, formatter, extensions)` decodes `message.PayloadSegment` (as `ReadOnlyMemory<byte>`) through `formatter.DecodeStructuredModeMessage` with the pre-declared extension-attribute set and a null `ContentType`, returning a validated `CloudEvent`; the `params` overload forwards to the `IEnumerable<CloudEventAttribute>?` overload.
- Formatter is the injected `CloudEventFormatter` — the shared `JsonEventFormatter`/`JsonEventFormatter<T>` from `CloudNative.CloudEvents.SystemTextJson` (`api-cloudevents`) whose `JsonSerializerOptions` compose `ConfigureForNodaTime` + `ThinktectureJsonConverterFactory`, constructed once and reused across every sink; a per-message formatter instance is the rejected form.

[STACKING]:
- MQTT egress is the same one-rail projection as the Kafka and `AMQP 1.0` sinks: the `Version/ledger#CHANGEFEED` `OpLogEntry` → `CloudEvent` via the `Version/egress#EGRESS_SINK` `Egress.Envelope` projector → `ce.ToMqttApplicationMessage(ContentMode.Structured, formatter, topic)` → an `MQTTnet` `IMqttClient.PublishAsync` at QoS-1 whose PUBACK is the `DeliveryAck`. One shared `JsonEventFormatter` encodes every event; there is no second envelope shape and no hand-built CloudEvents JSON over a raw payload.
- `Structured` is the FORCED content mode, not a choice: the binding throws on `Binary`, so unlike the Kafka `ce_*` and AMQP `cloudEvents_` header legs the whole envelope always rides the payload body and the CloudEvents `id` is the only dedup handle a receiver keys on — receiver-side id-dedup is the MQTT sink's whole dedup story, exactly the stance the envelope law demands.
- W3C trace continuity is a manual composite carrier, not a binding member: MQTT v5 `MqttApplicationMessage.UserProperties` (`List<MqttUserProperty>`) carry the `traceparent`/`tracestate` pair, stamped beside the `ToMqttApplicationMessage` encode by the AppHost `TraceContext` carrier adapter and read beside the `ToCloudEvent` decode to extract-and-continue the originating span; the binding touches only the payload body, so the estate transport law (no OTel broker instrumentation — manual carriers are design, not gap) owns the UserProperties write and read, symmetric with the NATS-header and AMQP application-property counterparts.
- envelope-vs-carrier ownership splits and never overlaps: this binding owns the structured-mode CloudEvents body over `PayloadSegment`; the MQTT topic string (`Topic`), QoS (`MqttQualityOfServiceLevel`), and `UserProperties` trace pair are `EgressSink.Mqtt` subscription policy. A structured-mode CloudEvents payload and its MQTT delivery metadata are one message, never two layers contending for the body.
- CloudEvents envelope is the single cross-transport, cross-language egress vocabulary: the Kafka egress (`api-cloudevents`), the `AMQP 1.0` binding (`api-cloudevents-amqp`), and this MQTT binding project the identical `CloudEvent` shape, so an MQTT consumer joins the CDC fan under the same envelope the changefeed emits — a per-transport re-pack is the drift defect.

[LOCAL_ADMISSION]:
- egress composes `cloudEvent.ToMqttApplicationMessage(ContentMode.Structured, formatter, topic)` at the `EgressSink.Mqtt` leg; the extension attributes (`traceparent`, `redacted`, `sequence`) are the same set declared once via `CloudEventAttribute.CreateExtension` and read back on ingress through the `ToCloudEvent` overload with the identical attribute enumerable.
- Shared `JsonEventFormatter`/`JsonEventFormatter<T>` instance encodes and decodes every message; the serializer options are fixed at formatter construction, never per message.
- structured mode is the only admitted MQTT content mode — a `ContentMode.Binary` call is a compile-time-legal, run-time-throwing defect, so the sink pins `ContentMode.Structured` at the single call site.

[RAIL_LAW]:
- Package: `CloudNative.CloudEvents.Mqtt`
- Owns: the CloudEvents MQTT protocol binding — structured-mode `CloudEvent` ⇄ `MqttApplicationMessage` for the `EgressSink.Mqtt` sync-egress leg
- Accept: `ToMqttApplicationMessage`/`ToCloudEvent` with an injected shared `CloudEventFormatter`, `ContentMode.Structured`, pre-declared extension attributes, and W3C trace written to and read from `MqttApplicationMessage.UserProperties` beside the projection
- Reject: `ContentMode.Binary` on the MQTT binding, hand-rolled CloudEvents JSON over a raw `MqttApplicationMessage` payload, a per-message formatter instance, trace context smuggled into the envelope body instead of UserProperties, or a per-transport envelope shape parallel to the shared `CloudEvent` projection
