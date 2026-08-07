# [RASM_COMPUTE_API_CLOUDEVENTS_MQTT]

`Rasm.Persistence` owns the `CloudNative.CloudEvents.Mqtt` binding for this branch at `libs/csharp/Rasm.Persistence/.api/api-cloudevents-mqtt.md` — one `MqttExtensions` static class mapping a `CloudEvent` onto an MQTTnet `MqttApplicationMessage` and back, structured mode only — so Compute registers that surface rather than re-tabling it. This partition holds the twin capture-INGEST direction alone: the sensor subscription decoding one sample per message onto the `WorkLane.CaptureIngest` row, against the sync-egress direction the owner drives.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: Compute ingest partition of `CloudNative.CloudEvents.Mqtt`
- package: `CloudNative.CloudEvents.Mqtt` (Apache-2.0, direct `PackageReference`)
- assembly/namespace: `CloudNative.CloudEvents.Mqtt`, as catalogued at the Persistence owner
- asset: pure-managed library, no native asset, no RID burden
- depends: `CloudNative.CloudEvents` (the envelope and formatter core — transitive here, so this folder holds no direct reference and no catalogue for it) and `MQTTnet` (`libs/csharp/.api/api-mqtt.md`, the estate `MqttApplicationMessage` transport)
- abi: the binding is compiled against the MQTTnet v4 message shape — its egress write survives because v5 keeps `PayloadSegment` as a set-only `ArraySegment<byte>` shim folding into `Payload`, but its ingress read wants the v4 `PayloadSegment` getter the restored v5 carrier dropped, so `message.ToCloudEvent` faults `MissingMethodException` and this direction decodes `MqttApplicationMessage.Payload` (`ReadOnlySequence<byte>`) through the shared formatter instead
- rail: capture-ingest

- Registers the MQTT protocol binding(`libs/csharp/Rasm.Persistence/.api/api-cloudevents-mqtt.md`): `MqttExtensions` with its `ToMqttApplicationMessage` egress map and both `ToCloudEvent` ingress overloads, the `ContentMode.Structured`-only contract and its `ArgumentOutOfRangeException`, and the null-`ContentType` decode path all resolve there, over the `CloudEvent`/`CloudEventFormatter`/`CloudEventAttribute` algebra `libs/csharp/Rasm.Persistence/.api/api-cloudevents.md` owns.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- The binding touches the payload body alone, so `Topic`, QoS (`MqttQualityOfServiceLevel`), and `UserProperties` are Compute subscription policy on the ingest side — one message, never two layers contending for the body, symmetric to the `api-nats` `NatsHeaders` carrier.

[STACKING]:
- twin ingest is one rail: an `MQTTnet` `IMqttClient` subscription surfaces one `MqttApplicationMessage` per sensor sample → `formatter.DecodeStructuredModeMessage(message.Payload, null, extensions)` decodes the structured envelope off the v5 body → the typed `Data` admits onto the `WorkLane.CaptureIngest` DropOldest row → `Stats/signal` folds the measured end (`Transform.Modal`) → `DigitalTwin.Score`/`Update` closes the loop into anomaly verdicts.
- W3C trace continuity rides `MqttApplicationMessage.UserProperties` (`List<MqttUserProperty>`, MQTT v5) as a manual composite carrier by estate transport law, read beside the `ToCloudEvent` decode to extract and continue the originating span.
- CloudEvents is the single cross-transport ingest vocabulary: the Kafka egress and this MQTT ingest project the same `CloudEvent` shape, so a measured signal crosses into the twin under the identical envelope the changefeed egress emits, and a per-transport re-pack is the drift defect.

[LOCAL_ADMISSION]:
- Compute decodes the structured body off `MqttApplicationMessage.Payload` at the single subscription call site and reads the pre-declared extension attributes as typed values rather than re-parsing envelope strings; `message.ToCloudEvent` is compile-legal and run-time-throwing on the pinned carrier, so it never enters a call site.
- One shared `JsonEventFormatter`/`JsonEventFormatter<T>` instance decodes every message, its serializer options fixed at construction.

[RAIL_LAW]:
- Package: `CloudNative.CloudEvents.Mqtt`
- Owns: the twin sensor capture-ingest direction over the registered binding — the subscription decode and its admission onto `WorkLane.CaptureIngest`
- Accept: a structured-mode decode off `MqttApplicationMessage.Payload` through an injected shared `CloudEventFormatter`, pre-declared extension attributes, and W3C trace read from `MqttApplicationMessage.UserProperties` beside the decode
- Reject: a member roster for the binding here, `message.ToCloudEvent` against the pinned v5 carrier, hand-rolled CloudEvents JSON over a raw `MqttApplicationMessage` payload, a per-message formatter instance, trace context smuggled into the envelope body instead of `UserProperties`, and a per-transport envelope shape parallel to the shared `CloudEvent` projection
