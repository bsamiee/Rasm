# [RASM_COMPUTE_API_CLOUDEVENTS_MQTT]

`CloudNative.CloudEvents.Mqtt` is the CNCF MQTT protocol binding: one static
`MqttExtensions` class mapping a `CloudEvent` onto an `MQTTnet` `MqttApplicationMessage`
and back, structured-mode only. It closes the held CloudEvents binding family for Compute — the core
`CloudEvent`/`CloudEventFormatter` envelope algebra and the `JsonEventFormatter`
System.Text.Json codec are the Persistence `api-cloudevents` overlay and are NOT restated
here; this overlay owns only the MQTT wire seam. It is the transport half of the
`Rasm.Compute/IDEAS#TWIN_SENSOR_INGEST` sensor loop: an MQTT v5 subscription decodes each
structured-mode envelope into a typed measured signal, admits it onto the `capture-ingest`
`WorkLane.CaptureIngest` DropOldest row, and drives `DigitalTwin.Score`/`Update` so site
telemetry yields live anomaly verdicts. W3C trace continuity rides `MqttApplicationMessage.UserProperties`
as a manual composite carrier by estate transport law — the binding never reads or writes
UserProperties, so trace extract-and-continue is Compute code beside the envelope decode,
not a binding feature. This page is HOST-LOCAL and carries no TS_PROJECTION.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.Mqtt`
- package: `CloudNative.CloudEvents.Mqtt`
- license: `Apache-2.0` (`cloudevents/sdk-csharp`)
- assembly: `CloudNative.CloudEvents.Mqtt`
- namespace: `CloudNative.CloudEvents.Mqtt`
- asset: pure-managed library; no native asset, no RID burden
- depends: `CloudNative.CloudEvents` (core envelope/formatter, `api-cloudevents`) and `MQTTnet` (the estate `MqttApplicationMessage` transport)
- abi: the binding reads and writes `MqttApplicationMessage.PayloadSegment`; resolve carrier compatibility against the restored `MQTTnet` public surface
- rail: capture-ingest

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: MQTT protocol binding (`CloudNative.CloudEvents.Mqtt`)
- rail: capture-ingest
- note: the whole assembly is one static class; the core `CloudEvent`, `CloudEventFormatter`, `ContentMode`, and `CloudEventAttribute` types it consumes are owned by `api-cloudevents`.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [CAPABILITY]                                                             |
| :-----: | :--------------- | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `MqttExtensions` | extension static | `CloudEvent` ⇄ `MqttApplicationMessage`; structured-mode encode + decode |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `MqttExtensions` egress and ingress maps
- rail: capture-ingest
- note: three members, all structured-mode. `ToMqttApplicationMessage` throws `ArgumentOutOfRangeException` on any `ContentMode` other than `Structured` — the MQTT binding has no binary-mode surface, unlike the Kafka binding. `ToCloudEvent` always decodes through `CloudEventFormatter.DecodeStructuredModeMessage` over `message.PayloadSegment` with a null `ContentType`. Both `ToCloudEvent` overloads validate non-null message and formatter through `Validation.CheckNotNull`; `ToMqttApplicationMessage` validates a complete `CloudEvent` through `Validation.CheckCloudEventArgument`.
- returns: `ToMqttApplicationMessage` returns `MQTTnet`'s `MqttApplicationMessage`; both `ToCloudEvent` overloads return a validated `CloudEvent`.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [CAPABILITY]                                      |
| :-----: | :----------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `ce.ToMqttApplicationMessage(contentMode, formatter, topic)` | egress map     | structured-mode encode into payload; sets `Topic` |
|  [02]   | `message.ToCloudEvent(formatter, params extensions)`         | ingress map    | structured-mode decode; `params` extension attrs  |
|  [03]   | `message.ToCloudEvent(formatter, IEnumerable<extensions>)`   | ingress map    | decode; `IEnumerable<CloudEventAttribute>` attrs  |

## [04]-[IMPLEMENTATION_LAW]

[MQTT_TOPOLOGY]:
- namespace `CloudNative.CloudEvents.Mqtt` carries the single `MqttExtensions` static class; the carrier type `MqttApplicationMessage` is `MQTTnet`, `MqttUserProperty` is `MQTTnet.Packets`, and `MqttQualityOfServiceLevel` is `MQTTnet.Protocol`.
- `ToMqttApplicationMessage(ce, ContentMode.Structured, formatter, topic)` packs the entire event — attributes and `Data` — into the message payload under `application/cloudevents+json` via `formatter.EncodeStructuredModeMessage`, writes it through `BinaryDataUtilities.GetArraySegment` onto `MqttApplicationMessage.PayloadSegment`, and sets `Topic`. Any `contentMode` other than `Structured` throws `ArgumentOutOfRangeException` — binary-mode CloudEvents placement is not an MQTT-binding capability.
- `ToCloudEvent(message, formatter, extensions)` decodes `message.PayloadSegment` (as `ReadOnlyMemory<byte>`) through `formatter.DecodeStructuredModeMessage` with the pre-declared extension-attribute set and a null `ContentType`, returning a validated `CloudEvent`; the `params` overload forwards to the `IEnumerable<CloudEventAttribute>?` overload.
- Formatter is the injected `CloudEventFormatter` — a shared `JsonEventFormatter`/`JsonEventFormatter<T>` from `CloudNative.CloudEvents.SystemTextJson` (`api-cloudevents`), constructed once and reused; a per-message formatter instance is the rejected form.

[STACKING]:
- Twin ingest is one rail: an `MQTTnet` `IMqttClient` subscription surfaces an `MqttApplicationMessage` per sensor sample → `message.ToCloudEvent(formatter, extensions)` decodes the structured-mode envelope with the pre-declared `traceparent`/`redacted` extension attributes → the typed `Data` (a `JsonEventFormatter<T>` measured-signal record, or raw bytes under `JsonEventFormatter`) admits onto the `WorkLane.CaptureIngest` DropOldest row → `Stats/signal` folds the measured end (`Transform.Modal`) → `DigitalTwin.Score`/`Update` closes the loop into anomaly verdicts and receipted control suggestions.
- W3C trace continuity is a manual composite carrier, not a binding member: MQTT v5 `MqttApplicationMessage.UserProperties` (`List<MqttUserProperty>`) carry the `traceparent`/`tracestate` pair, read beside the `ToCloudEvent` decode to extract-and-continue the originating span; the binding touches only the payload body, so the estate transport law (no OTel broker instrumentation — manual carriers are design, not gap) owns the UserProperties read and the NATS-header counterpart symmetrically.
- envelope-vs-carrier ownership splits and never overlaps: this binding owns the structured-mode CloudEvents body over `PayloadSegment`; the MQTT topic string (`Topic`), QoS (`MqttQualityOfServiceLevel`), and `UserProperties` trace pair are Compute subscription policy. A structured-mode CloudEvents payload and its MQTT delivery metadata are one message, never two layers contending for the body.
- CloudEvents envelope is the single cross-transport, cross-language ingest vocabulary: the Kafka egress (`api-cloudevents`, Persistence) and this MQTT ingest project the same `CloudEvent` shape, so a measured signal crosses MQTT into the twin under the identical envelope the changefeed egress emits — a per-transport re-pack is the drift defect.

[LOCAL_ADMISSION]:
- ingest decodes through `message.ToCloudEvent(formatter, extensions)` with the same pre-declared extension-attribute set the egress declared once via `CloudEventAttribute.CreateExtension`, so the consumer reads typed attributes rather than re-parsing envelope strings.
- Shared `JsonEventFormatter`/`JsonEventFormatter<T>` instance decodes every message; the serializer options are fixed at formatter construction, never per message.
- structured mode is the only admitted MQTT content mode — a `ContentMode.Binary` call is a compile-time-legal, run-time-throwing defect, so Compute ingest pins `ContentMode.Structured` at the single call site.

[RAIL_LAW]:
- Package: `CloudNative.CloudEvents.Mqtt`
- Owns: the CloudEvents MQTT protocol binding — structured-mode `CloudEvent` ⇄ `MqttApplicationMessage` for the twin sensor-ingest wire
- Accept: `ToMqttApplicationMessage`/`ToCloudEvent` with an injected shared `CloudEventFormatter`, `ContentMode.Structured`, pre-declared extension attributes, and W3C trace read from `MqttApplicationMessage.UserProperties` beside the decode
- Reject: `ContentMode.Binary` on the MQTT binding, hand-rolled CloudEvents JSON over a raw `MqttApplicationMessage` payload, a per-message formatter instance, trace context smuggled into the envelope body instead of UserProperties, or a per-transport envelope shape parallel to the shared `CloudEvent` projection
