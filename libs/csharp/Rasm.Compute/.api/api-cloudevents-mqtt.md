# [RASM_COMPUTE_API_CLOUDEVENTS_MQTT]

`CloudNative.CloudEvents.Mqtt` binds the CNCF CloudEvents envelope onto the MQTT wire: the static `MqttExtensions` class maps a `CloudEvent` onto an MQTTnet `MqttApplicationMessage` and back, structured mode only. `CloudEvent`/`CloudEventFormatter` envelope algebra and the System.Text.Json codec live in the Persistence `api-cloudevents` overlay; this overlay owns only the MQTT protocol-binding seam feeding the twin sensor-ingest capture rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CloudNative.CloudEvents.Mqtt`
- package: `CloudNative.CloudEvents.Mqtt` (Apache-2.0)
- assembly/namespace: `CloudNative.CloudEvents.Mqtt`
- asset: pure-managed library, no native asset, no RID burden
- depends: `CloudNative.CloudEvents` (core envelope/formatter) and `MQTTnet` (the estate `MqttApplicationMessage` transport)
- abi: the binding reads and writes `MqttApplicationMessage.PayloadSegment`; resolve carrier compatibility against the restored `MQTTnet` public surface
- rail: capture-ingest

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: MQTT protocol binding — one static class consuming the `api-cloudevents` core types

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :--------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `MqttExtensions` | static class  | `CloudEvent` ⇄ `MqttApplicationMessage`; structured-mode encode + decode |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `MqttExtensions` structured-mode egress and ingress maps, all static extension methods

| [INDEX] | [SURFACE]                                                                     | [SHAPE] | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------------------- | :------ | :----------------------------------------- |
|  [01]   | `ce.ToMqttApplicationMessage(ContentMode, CloudEventFormatter, string)`       | static  | encode onto `PayloadSegment`; sets `Topic` |
|  [02]   | `message.ToCloudEvent(CloudEventFormatter, params CloudEventAttribute[])`     | static  | decode; params extension attrs             |
|  [03]   | `message.ToCloudEvent(CloudEventFormatter, IEnumerable<CloudEventAttribute>)` | static  | decode; enumerable extension attrs         |

- `ToCloudEvent`: decodes `PayloadSegment` through `formatter.DecodeStructuredModeMessage` with a null `ContentType`; the `params` overload forwards to the `IEnumerable` one, and both validate non-null message and formatter through `Validation.CheckNotNull`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `MqttExtensions` maps every op through the injected shared `CloudEventFormatter` (`JsonEventFormatter`/`JsonEventFormatter<T>`, `api-cloudevents`), structured mode only — `PayloadSegment` carries the whole `application/cloudevents+json` envelope both directions, any non-`Structured` `ContentMode` throws `ArgumentOutOfRangeException`, and a per-message formatter instance is the rejected form.

[STACKING]:
- twin ingest is one rail: an `MQTTnet` `IMqttClient` subscription surfaces one `MqttApplicationMessage` per sensor sample → `message.ToCloudEvent(formatter, extensions)` decodes the structured envelope → the typed `Data` admits onto the `WorkLane.CaptureIngest` DropOldest row → `Stats/signal` folds the measured end (`Transform.Modal`) → `DigitalTwin.Score`/`Update` closes the loop into anomaly verdicts.
- W3C trace continuity rides `MqttApplicationMessage.UserProperties` (`List<MqttUserProperty>`, MQTT v5) as a manual composite carrier by estate transport law, read beside the `ToCloudEvent` decode to extract-and-continue the originating span; the binding touches only the payload body, so `Topic`, QoS (`MqttQualityOfServiceLevel`), and `UserProperties` are Compute subscription policy — one message, never two layers contending for the body, symmetric to the `api-nats` `NatsHeaders` carrier.
- CloudEvents is the single cross-transport ingest vocabulary: the Kafka egress (`api-cloudevents`) and this MQTT ingest project the same `CloudEvent` shape, so a measured signal crosses into the twin under the identical envelope the changefeed egress emits, and a per-transport re-pack is the drift defect.

[LOCAL_ADMISSION]:
- Compute pins `ContentMode.Structured` at the single call site and reads the pre-declared extension attributes as typed values rather than re-parsing envelope strings; a `ContentMode.Binary` call is compile-legal and run-time-throwing.

[RAIL_LAW]:
- Package: `CloudNative.CloudEvents.Mqtt`
- Owns: the CloudEvents MQTT protocol binding — structured-mode `CloudEvent` ⇄ `MqttApplicationMessage` for the twin sensor-ingest wire
- Accept: `ToMqttApplicationMessage`/`ToCloudEvent` with an injected shared `CloudEventFormatter`, `ContentMode.Structured`, pre-declared extension attributes, and W3C trace read from `MqttApplicationMessage.UserProperties` beside the decode
- Reject: `ContentMode.Binary` on the MQTT binding, hand-rolled CloudEvents JSON over a raw `MqttApplicationMessage` payload, a per-message formatter instance, trace context smuggled into the envelope body instead of `UserProperties`, or a per-transport envelope shape parallel to the shared `CloudEvent` projection
