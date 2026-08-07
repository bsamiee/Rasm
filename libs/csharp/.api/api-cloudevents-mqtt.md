# [RASM_API_CLOUDEVENTS_MQTT]

`CloudNative.CloudEvents.Mqtt` binds the CNCF `MqttExtensions` static class, mapping a `CloudEvent` onto an `MQTTnet` `MqttApplicationMessage` and back in structured mode alone. Two folders drive the two directions of one binding: `Rasm.Persistence` the sync-egress leg feeding the `Version/egress#EGRESS_SINK` `EgressSink.Mqtt` row, and `Rasm.Compute` the twin capture-INGEST direction — the sensor subscription decoding one sample per message onto the `WorkLane.CaptureIngest` row. MQTT carries no binary content mode, so the whole event always rides the payload body under `application/cloudevents+json` while `Topic`, QoS, and `UserProperties` stay subscription metadata the binding never touches.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.Mqtt`
- package: `CloudNative.CloudEvents.Mqtt` (Apache-2.0)
- assembly: `CloudNative.CloudEvents.Mqtt`
- namespace: `CloudNative.CloudEvents.Mqtt`
- asset: pure-managed library; no native asset, no RID burden
- depends: `CloudNative.CloudEvents` (core envelope/formatter, `api-cloudevents.md`; transitive at Compute, which holds no direct core reference), `MQTTnet` (the `MqttApplicationMessage` transport, `api-mqtt.md`)
- abi: compiled against the MQTTnet v4 message shape — the egress `PayloadSegment` WRITE survives on the pinned v5 carrier because v5 keeps that member as a set-only `ArraySegment<byte>` shim folding into `Payload`, while the ingress `PayloadSegment` READ has no v5 getter and faults `MissingMethodException`, so a decode against the restored carrier reads `MqttApplicationMessage.Payload` (`ReadOnlySequence<byte>`) into `formatter.DecodeStructuredModeMessage` directly
- rail: sync-egress and capture-ingest

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

- `ce.ToMqttApplicationMessage`: throws `ArgumentOutOfRangeException("contentMode", …)` on any `ContentMode` but `Structured`; it assembles the message through an object initializer over the `PayloadSegment` setter rather than `MqttApplicationMessageBuilder`, and the v5 shim folds those bytes into `Payload`, so the returned message exposes its body on `Payload` alone.
- `message.ToCloudEvent`: reads the retired v4 `PayloadSegment` getter into `formatter.DecodeStructuredModeMessage` under a null `ContentType`; the `params` overload forwards to the `IEnumerable` one, and both gate message and formatter through `Validation.CheckNotNull`, so the null gates pass and the member faults on the missing getter instead.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `MqttExtensions` is the whole binding; its carrier `MqttApplicationMessage`, `MqttUserProperty`, and `MqttQualityOfServiceLevel` are `MQTTnet`'s transport model (`api-mqtt.md`), and the consumed `CloudEvent`, `CloudEventFormatter`, `ContentMode`, and `CloudEventAttribute` are `api-cloudevents.md`'s, never the binding's.
- `ce.ToMqttApplicationMessage(Structured, formatter, topic)` packs the entire event into the message body under `application/cloudevents+json` and returns the exact `MqttApplicationMessage` an `IMqttClient.PublishAsync` publishes with no re-map, its body readable on `Payload`; structured mode carries no `ce_type`/`ce_source` header-route form, so a broker filters on topic alone.
- The binding touches the payload body alone, so `Topic`, QoS, and `UserProperties` are per-leg policy — egress-sink policy on the publish side, Compute subscription policy on the ingest side — one message, never two layers contending for the body, symmetric to the NATS `NatsHeaders` carrier.

[STACKING]:
- `CloudNative.CloudEvents`(`api-cloudevents.md`): the injected shared `CloudEventFormatter`/`JsonEventFormatter<T>` encodes and decodes every event through `EncodeStructuredModeMessage`/`DecodeStructuredModeMessage`, and the mapped `CloudEvent` is that catalogue's envelope algebra.
- `MQTTnet`(`api-mqtt.md`): `ToMqttApplicationMessage`'s result is the exact `MqttApplicationMessage` an `IMqttClient.PublishAsync` sends at QoS-1 whose PUBACK is the `DeliveryAck`; the W3C `traceparent`/`tracestate` pair rides `MqttApplicationMessage.UserProperties` (`List<MqttUserProperty>`), stamped beside the encode and read beside the decode by the AppHost `TraceContext` adapter, symmetric with the NATS-header and AMQP application-property carriers.
- Persistence consumer anchor: the `Version/ledger#CHANGEFEED` `OpLogEntry` → `CloudEvent` via the `Egress.Envelope` projector → `ce.ToMqttApplicationMessage(ContentMode.Structured, formatter, topic)` → the QoS-1 publish; the CloudEvents `id` is the only dedup handle, so receiver-side id-dedup is the MQTT sink's whole dedup story. Ownership splits at the message: the binding owns the structured-mode body over `Payload`, while `Topic`, `MqttQualityOfServiceLevel`, and the `UserProperties` trace pair are `EgressSink.Mqtt` subscription policy.
- Compute consumer anchor: an `MQTTnet` `IMqttClient` subscription surfaces one `MqttApplicationMessage` per sensor sample → `formatter.DecodeStructuredModeMessage(message.Payload, null, extensions)` decodes the structured envelope off the v5 body → the typed `Data` admits onto the `WorkLane.CaptureIngest` DropOldest row → `Stats/signal` folds the measured end (`Transform.Modal`) → `DigitalTwin.Score`/`Update` closes the loop into anomaly verdicts. CloudEvents is the single cross-transport ingest vocabulary: the Kafka egress and this MQTT ingest project the same `CloudEvent` shape, so a measured signal crosses into the twin under the identical envelope the changefeed egress emits.

[LOCAL_ADMISSION]:
- Egress pins `cloudEvent.ToMqttApplicationMessage(ContentMode.Structured, formatter, topic)` at the single `EgressSink.Mqtt` call site; a `ContentMode.Binary` call is compile-legal and run-time-throwing.
- Any leg reading a message back decodes `MqttApplicationMessage.Payload` through the shared formatter at its single subscription call site, reading the pre-declared extension attributes as typed values; `message.ToCloudEvent` is compile-legal and run-time-throwing on the pinned carrier, so the ingress overloads never enter a call site.
- One shared `JsonEventFormatter`/`JsonEventFormatter<T>` instance encodes and decodes every message, its serializer options fixed at construction, never per message.
- Extension attributes (`traceparent`, `redacted`, `sequence`) are declared once via `CloudEventAttribute.CreateExtension` and read back on ingress with the identical attribute enumerable.

[RAIL_LAW]:
- Package: `CloudNative.CloudEvents.Mqtt`
- Owns: the CloudEvents MQTT protocol binding — structured-mode `CloudEvent` ⇄ `MqttApplicationMessage` for the `EgressSink.Mqtt` sync-egress leg and the twin capture-ingest subscription decode
- Accept: `ToMqttApplicationMessage` with an injected shared `CloudEventFormatter`, `ContentMode.Structured`, a structured-mode decode off `MqttApplicationMessage.Payload`, pre-declared extension attributes, and W3C trace on `MqttApplicationMessage.UserProperties`
- Reject: `ContentMode.Binary`, `message.ToCloudEvent` against the pinned v5 carrier, hand-rolled CloudEvents JSON over a raw `MqttApplicationMessage` payload, a per-message formatter instance, trace context in the envelope body instead of `UserProperties`, or a per-transport envelope shape parallel to the shared `CloudEvent` projection
