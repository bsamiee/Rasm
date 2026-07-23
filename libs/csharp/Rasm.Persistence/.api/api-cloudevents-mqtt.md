# [RASM_PERSISTENCE_API_CLOUDEVENTS_MQTT]

`CloudNative.CloudEvents.Mqtt` binds the CNCF `MqttExtensions` static class, mapping a `CloudEvent` onto an `MQTTnet` `MqttApplicationMessage` and back in structured mode alone — the MQTT leg of the one CloudEvents egress projection feeding the `Version/egress#EGRESS_SINK` `EgressSink.Mqtt` sync-egress row. MQTT carries no binary content mode, so the whole event always rides the payload body under `application/cloudevents+json` while `Topic`, QoS, and `UserProperties` stay subscription metadata the binding never touches.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.Mqtt`
- package: `CloudNative.CloudEvents.Mqtt` (Apache-2.0)
- assembly: `CloudNative.CloudEvents.Mqtt`
- namespace: `CloudNative.CloudEvents.Mqtt`
- asset: pure-managed library; no native asset, no RID burden
- depends: `CloudNative.CloudEvents` (core envelope/formatter, `api-cloudevents`), `MQTTnet` (the `MqttApplicationMessage` transport, `api-mqtt`)
- rail: sync-egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: MQTT protocol binding (`CloudNative.CloudEvents.Mqtt`)

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [CAPABILITY]                                                             |
| :-----: | :--------------- | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `MqttExtensions` | extension static | `CloudEvent` ⇄ `MqttApplicationMessage`; structured-mode encode + decode |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `MqttExtensions` structured-mode egress and ingress maps

| [INDEX] | [SURFACE]                                                    | [SHAPE]     | [CAPABILITY]                                      |
| :-----: | :----------------------------------------------------------- | :---------- | :------------------------------------------------ |
|  [01]   | `ce.ToMqttApplicationMessage(contentMode, formatter, topic)` | egress map  | structured-mode encode into payload; sets `Topic` |
|  [02]   | `message.ToCloudEvent(formatter, params extensions)`         | ingress map | structured-mode decode; `params` extension attrs  |
|  [03]   | `message.ToCloudEvent(formatter, IEnumerable<extensions>)`   | ingress map | decode; `IEnumerable<CloudEventAttribute>` attrs  |

- `ce.ToMqttApplicationMessage`: throws `ArgumentOutOfRangeException("contentMode", …)` on any `ContentMode` but `Structured`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `MqttExtensions` is the whole binding; its carrier `MqttApplicationMessage`, `MqttUserProperty`, and `MqttQualityOfServiceLevel` are `MQTTnet`'s transport model (`api-mqtt`), and the consumed `CloudEvent`, `CloudEventFormatter`, `ContentMode`, and `CloudEventAttribute` are `api-cloudevents`', never the binding's.
- `ce.ToMqttApplicationMessage(Structured, formatter, topic)` packs the entire event into `PayloadSegment` under `application/cloudevents+json` and returns the exact `MqttApplicationMessage` an `IMqttClient.PublishAsync` publishes with no re-map; structured mode carries no `ce_type`/`ce_source` header-route form, so a broker filters on topic alone.

[STACKING]:
- `api-cloudevents`(`.api/api-cloudevents.md`): the injected shared `CloudEventFormatter`/`JsonEventFormatter<T>` encodes and decodes every event through `EncodeStructuredModeMessage`/`DecodeStructuredModeMessage`, and the mapped `CloudEvent` is that overlay's envelope algebra.
- `api-mqtt`(`.api/api-mqtt.md`): `ToMqttApplicationMessage`'s result is the exact `MqttApplicationMessage` an `IMqttClient.PublishAsync` sends at QoS-1 whose PUBACK is the `DeliveryAck`; the W3C `traceparent`/`tracestate` pair rides `MqttApplicationMessage.UserProperties` (`List<MqttUserProperty>`), stamped beside the encode and read beside the decode by the AppHost `TraceContext` adapter, symmetric with the NATS-header and AMQP application-property carriers.
- `Version/egress` rail: the `Version/ledger#CHANGEFEED` `OpLogEntry` → `CloudEvent` via the `Egress.Envelope` projector → `ce.ToMqttApplicationMessage(ContentMode.Structured, formatter, topic)` → the QoS-1 publish; the CloudEvents `id` is the only dedup handle, so receiver-side id-dedup is the MQTT sink's whole dedup story.
- ownership splits at the message: this binding owns the structured-mode CloudEvents body over `PayloadSegment`, while `Topic`, `MqttQualityOfServiceLevel`, and the `UserProperties` trace pair are `EgressSink.Mqtt` subscription policy.

[LOCAL_ADMISSION]:
- egress pins `cloudEvent.ToMqttApplicationMessage(ContentMode.Structured, formatter, topic)` at the single `EgressSink.Mqtt` call site; a `ContentMode.Binary` call is compile-legal and run-time-throwing.
- one shared `JsonEventFormatter`/`JsonEventFormatter<T>` instance encodes and decodes every message, its serializer options fixed at construction, never per message.
- extension attributes (`traceparent`, `redacted`, `sequence`) are declared once via `CloudEventAttribute.CreateExtension` and read back on ingress through the `ToCloudEvent` overload with the identical attribute enumerable.

[RAIL_LAW]:
- Package: `CloudNative.CloudEvents.Mqtt`
- Owns: the CloudEvents MQTT protocol binding — structured-mode `CloudEvent` ⇄ `MqttApplicationMessage` for the `EgressSink.Mqtt` sync-egress leg
- Accept: `ToMqttApplicationMessage`/`ToCloudEvent` with an injected shared `CloudEventFormatter`, `ContentMode.Structured`, pre-declared extension attributes, and W3C trace on `MqttApplicationMessage.UserProperties`
- Reject: `ContentMode.Binary`, hand-rolled CloudEvents JSON over a raw `MqttApplicationMessage` payload, a per-message formatter instance, trace context in the envelope body instead of `UserProperties`, or a per-transport envelope shape parallel to the shared `CloudEvent` projection
