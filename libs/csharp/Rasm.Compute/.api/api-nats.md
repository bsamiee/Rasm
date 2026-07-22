# [RASM_COMPUTE_API_NATS]

`NATS.Net` is the NATS Core subscription seam Compute consumes as the broker counterpart to
the MQTT twin-ingest adapter: `INatsClient.SubscribeAsync<T>` surfaces one `NatsMsg<T>` per
sensor sample, `NatsMsg<byte[]>.Data` carries the structured-mode CloudEvents body, and the
`NatsHeaders` lookup on `NatsMsg.Headers` reads the W3C `traceparent`/`tracestate` pair beside
the decode. This overlay owns ONLY that Core ingest seam — the JetStream durable-stream, KV,
and Object-Store surfaces plus the full publish-ack ceremony are the Persistence `api-nats`
overlay and are NOT restated here. It arms `Runtime/transport#BROKER_NATS`: a NATS subscription
decodes each `NatsMsg<byte[]>` payload into a typed `SensorEnvelope<T>` through the shared
`JsonEventFormatter<T>` (`api-cloudevents-mqtt`), admits it onto the `capture-ingest`
`WorkLane.CaptureIngest` DropOldest row, and drives `TwinLoop.Ingest` so broker telemetry yields
live anomaly verdicts — the NATS adapter beside `BrokerChannels.Mqtt<T>`. W3C trace continuity
rides `NatsHeaders` as a manual composite carrier by estate transport law (no broker OTel
instrumentation), symmetric to the MQTT `UserProperties` carrier. Connection is a per-instance
`IAsyncDisposable` handle, one long-lived subscriber shared across subjects, never a
process-global static. This page is HOST-LOCAL and carries no TS_PROJECTION.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NATS.Net` (meta-package)
- package: `NATS.Net`
- license: `Apache-2.0` (`nats-io/nats.net`)
- assembly: `NATS.Net` (facade exposing `NATS.Net.NatsClient`; the subscription surface lives in `NATS.Client.Core`)
- namespace: `NATS.Client.Core` (client, connection, message, headers), `NATS.Net` (`NatsClient` entry)
- asset: pure-managed AnyCPU library; no native asset, no RID burden (TCP/WebSocket NATS protocol)
- depends: transitively pins `NATS.Client.Core`, `NATS.Client.Abstractions`, and the JetStream/KV/Object/Services/Hosting/Simplified/Serializers sub-clients — Compute consumes only the `NATS.Client.Core` subscription namespace
- abi: `NatsMsg<T>` is a `readonly record struct`; resolve `Data`/`Headers`/`Subject` against the restored `NATS.Client.Core` public surface
- rail: capture-ingest

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: NATS Core subscription types (`NATS.Client.Core` / `NATS.Net`) — the client, the message, and the header carrier
- rail: capture-ingest
- note: the JetStream/KV/Object types (`INatsJSContext`, `INatsKVStore`, `INatsObjStore`, `PubAckResponse`) are the Persistence overlay's and are absent here; the connection is per-instance (`new NatsConnection(NatsOpts)`, `IAsyncDisposable`), one shared subscriber, never a static.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]    | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------ | :--------------- | :------------------------------------------------------------- |
|  [01]   | `NATS.Net.NatsClient : INatsClient`               | simplified entry | one-line subscriber; `.Connection` is `INatsConnection`        |
|  [02]   | `INatsClient : IAsyncDisposable`                  | client           | subscribe/publish/ping surface Compute binds against          |
|  [03]   | `INatsConnection : INatsClient`                   | connection       | request/reply, `SubscribeCoreAsync`, lifecycle, `Opts`         |
|  [04]   | `NatsConnection : INatsConnection`                | connection       | concrete per-instance handle (`new NatsConnection(NatsOpts)`)  |
|  [05]   | `NatsOpts` (sealed record)                        | options          | url, name, serializer registry, subscription buffer           |
|  [06]   | `NatsMsg<T>` (readonly record struct)             | message          | `Subject`/`Data`/`Headers`/`ReplyTo`/`Connection`; `ReplyAsync` |
|  [07]   | `NatsHeaders : IDictionary<string, StringValues>` | headers          | `traceparent`/`tracestate` W3C carrier; non-throwing indexer   |
|  [08]   | `NatsSubOpts` (readonly record struct)            | sub options      | subscription channel/serializer options for `SubscribeAsync`   |
|  [09]   | `NatsMsgFlags` (enum)                             | message bit      | `None`/`Empty`/`NoResponders` — `IsEmpty`/`HasNoResponders`    |
|  [10]   | `INatsDeserialize<T>` / `NatsRawSerializer<T>`    | codec            | per-type deserialize; `NatsRawSerializer<byte[]>` passthrough  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `INatsClient` subscribe, `NatsMsg<T>` read, `NatsHeaders` lookup
- rail: capture-ingest
- note: `SubscribeAsync<T>` yields `IAsyncEnumerable<NatsMsg<T>>` — an `await foreach` drain that runs until the `CancellationToken` trips or the connection drops; `queueGroup` load-balances the same subject across N subscribers. `NatsMsg<byte[]>.Data` is the structured-mode CloudEvents body, `Headers` is `NatsHeaders?` (null when the publisher sent none), and the `NatsHeaders` string indexer returns `StringValues.Empty` for an absent key rather than throwing — W3C trace lookup is `msg.Headers?.TryGetValue("traceparent", out var tp)`.
- returns: `SubscribeAsync<T>` → `IAsyncEnumerable<NatsMsg<T>>`; each `NatsMsg<T>.Data` → `T?`; `NatsHeaders[key]` → `StringValues`.

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY] | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `client.SubscribeAsync<byte[]>(subject, queueGroup?, serializer?, opts?, ct)` | subscribe      | → `IAsyncEnumerable<NatsMsg<byte[]>>` |
|  [02]   | `connection.SubscribeCoreAsync<T>(subject, queueGroup?, …)`                   | subscribe      | → `ValueTask<INatsSub<T>>` drain      |
|  [03]   | `msg.Data`                                                                    | message read   | `byte[]?` CloudEvents body            |
|  [04]   | `msg.Subject` / `msg.ReplyTo`                                                 | message read   | routing token / RPC reply subject     |
|  [05]   | `msg.Headers`                                                                 | header carrier | `NatsHeaders?`; null when none        |
|  [06]   | `msg.Headers?.TryGetValue(name, out StringValues v)`                          | header lookup  | non-throwing W3C extract              |
|  [07]   | `msg.Headers?[name]`                                                          | header lookup  | `StringValues`; `.Empty` if absent    |
|  [08]   | `msg.ReplyAsync<TReply>(data, headers?, …)`                                   | rpc reply      | reply to a request-subject message    |
|  [09]   | `connection.RequestAsync<TReq, TReply>(subject, data, …)`                     | rpc request    | → `ValueTask<NatsMsg<TReply>>`        |

## [04]-[IMPLEMENTATION_LAW]

[SUBSCRIPTION_TOPOLOGY]:
- `NatsConnection`/`NatsClient` is the long-lived per-instance handle (`IAsyncDisposable`), thread-safe, multiplexing every subject over one TCP/WebSocket connection; a subscriber is `new NatsClient(url)` or `new NatsConnection(NatsOpts)` once, shared across the capture lane, never one connection per subscription. The handle is app-scoped and honestly per-instance — the constructor is the per-instance factory evidence — so Compute owns its own subscriber independent of the Persistence egress connection.
- `SubscribeAsync<byte[]>(subject, queueGroup, cancellationToken: ct)` is the pump: an `await foreach (var msg in client.SubscribeAsync<byte[]>(subject, ct: ct))` drain runs until `ct` trips, each iteration surfacing one `NatsMsg<byte[]>`. The `queueGroup` argument load-balances the sensor subject across N capture subscribers for horizontal drain; a single subscriber leaves it null.
- back-pressure is the `NatsSubOpts` channel bound and the `INatsConnection.SlowConsumerDetected` event — the broker-side trip the estate reachability probe reads (Persistence overlay owns the event roster); Compute admits onto the `WorkLane.CaptureIngest` DropOldest row, so a slow twin score sheds oldest geometry state at the lane, never blocks the NATS drain.

[MESSAGE_DECODE]:
- `NatsMsg<byte[]>.Data` is the structured-mode CloudEvents body — the same `application/cloudevents+json` payload the MQTT `PayloadSegment` carries — decoded through the shared `JsonEventFormatter<T>` (`api-cloudevents-mqtt`, constructed once and reused) with the pre-declared extension-attribute set, yielding the typed `SensorEnvelope<T>` the twin loop ingests. A per-message formatter instance is the rejected form.
- `NatsMsg<byte[]>` subscribes with the default deserializer or `NatsRawSerializer<byte[]>` — the raw passthrough that hands the encoded bytes through untouched; NATS frames the bytes, the CloudEvents formatter owns the shape, no JSON hand-spelled at the subject boundary.
- `msg.IsEmpty`/`msg.HasNoResponders` (the `NatsMsgFlags` bits) guard the decode: an empty-payload control frame is skipped before the formatter runs, never fed a zero-length body.

[HEADER_CARRIER]:
- W3C trace continuity is a manual composite carrier, not a broker instrumentation member: `NatsMsg.Headers` (`NatsHeaders?`) carries the `traceparent`/`tracestate` pair the originating span wrote, read via `msg.Headers?.TryGetValue("traceparent", out var tp)` beside the `Data` decode to extract-and-continue the span. The estate transport law (no OTel broker instrumentation — manual carriers are design, not gap) owns this read, symmetric to the MQTT `UserProperties` carrier the `api-cloudevents-mqtt` overlay names.
- `NatsHeaders` is an `IDictionary<string, StringValues>`; the string indexer returns `StringValues.Empty` for an absent key (non-throwing) and `TryGetValue` is the guarded form — a missing `traceparent` yields a new root span, never an exception. `Keys` enumerates the delivered header names for the composite-carrier extract.
- envelope-vs-carrier ownership splits and never overlaps: this overlay owns the structured-mode CloudEvents body over `NatsMsg.Data` and the W3C pair over `NatsMsg.Headers`; the NATS `Subject` string and `queueGroup` are Compute subscription policy. One NATS message is one envelope plus its delivery metadata, never two layers contending for the body.

[RPC_REPLY]:
- the remote-compute RPC leg reaches through `INatsConnection` (`client.Connection`): `RequestAsync<TReq, TReply>(subject, data, ct)` dispatches a compute call and awaits `NatsMsg<TReply>`, and `msg.ReplyAsync<TReply>(result)` answers a received request-subject message — the request/reply half of `Runtime/transport` beside the gRPC `CallSpine`, distinct from the fire-and-forget sensor subscription. A subscription-only capture lane never touches this leg.

## [05]-[STACKING_AND_RAIL]

[STACKING]:
- broker ingest is one rail: a `NatsClient` subscription surfaces a `NatsMsg<byte[]>` per sensor sample → `msg.Headers?.TryGetValue("traceparent", …)` extracts the W3C pair → `msg.Data` decodes through the shared `JsonEventFormatter<T>` with the pre-declared extension attributes → the typed `SensorEnvelope<T>` admits onto the `WorkLane.CaptureIngest` DropOldest row → `Stats/signal` folds the measured end (`Transform.Modal`) → `TwinLoop.Ingest`/`Update` closes the loop into anomaly verdicts, the identical pipeline the MQTT adapter (`api-cloudevents-mqtt`) drives from `MqttApplicationMessage`.
- CloudEvents envelope is the single cross-transport ingest vocabulary: the NATS sensor subscription and the MQTT sensor subscription project the same `CloudEvent`/`SensorEnvelope<T>` shape into the twin, so `BrokerChannels.Nats<T>` and `BrokerChannels.Mqtt<T>` are two carriers of one envelope, never two envelope shapes — a per-transport re-pack is the drift defect.
- transport ownership splits from the Persistence egress: this overlay owns the Core subscription ingest seam; JetStream durable streams, publish-ack, KV, and Object Store are the Persistence `api-nats` overlay's egress and store rails. The same `NATS.Net` meta-package fans to both overlays at different consumption angles — Compute subscribes, Persistence publishes-and-persists — one package, two folder-scoped seams.

[RAIL_LAW]:
- Package: `NATS.Net` (Compute consumes the `NATS.Client.Core` subscription namespace)
- Owns: the NATS Core subscription ingest seam — `INatsClient.SubscribeAsync<byte[]>`, `NatsMsg<byte[]>.Data`, `NatsMsg.Headers`, and `NatsHeaders` W3C lookup for the twin sensor-ingest wire, plus the `RequestAsync`/`ReplyAsync` remote-compute RPC leg
- Accept: one long-lived per-instance `NatsClient`/`NatsConnection` subscriber, `SubscribeAsync<byte[]>` drained with a `CancellationToken`, `NatsMsg.Data` decoded through the shared `JsonEventFormatter<T>`, the W3C pair read from `NatsMsg.Headers` beside the decode, and admission onto `WorkLane.CaptureIngest`
- Reject: per-subscription connection construction, a hand-rolled NATS protocol/framing loop `NATS.Client.Core` owns, a per-message formatter instance, JSON hand-spelled at the subject instead of the CloudEvents formatter, trace context read from anywhere but `NatsMsg.Headers`, a per-transport envelope shape parallel to the shared `SensorEnvelope<T>`, or any JetStream/KV/Object surface (owned by the Persistence overlay)
