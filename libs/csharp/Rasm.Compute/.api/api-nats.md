# [RASM_COMPUTE_API_NATS]

`NATS.Net` owns the NATS Core subscription ingest seam Compute binds: `INatsClient.SubscribeAsync<T>` surfaces one `NatsMsg<byte[]>` per sensor sample, its `Data` carries the structured-mode CloudEvents body, and `NatsHeaders` on `NatsMsg.Headers` reads the W3C trace pair beside the decode. This overlay holds the Core ingest seam alone; the JetStream durable-stream, KV, and Object-Store surfaces and the publish-ack ceremony are the Persistence `api-nats` overlay. Each decoded `SensorEnvelope<T>` admits onto the `WorkLane.CaptureIngest` DropOldest row driving `TwinLoop.Ingest`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NATS.Net` (meta-package)
- package: `NATS.Net` (`Apache-2.0`, `nats-io/nats.net`)
- assembly: `NATS.Net` facade over `NATS.Net.NatsClient`; Compute binds the `NATS.Client.Core` subscription surface alone
- namespace: `NATS.Client.Core` (client, connection, message, headers), `NATS.Net` (`NatsClient` entry)
- asset: pure-managed AnyCPU, no native asset, no RID burden (TCP/WebSocket protocol)
- rail: capture-ingest

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: NATS Core subscription types — the client, the message, and the header carrier

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]    | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------------ | :--------------- | :-------------------------------------------------------------- |
|  [01]   | `NATS.Net.NatsClient : INatsClient`               | simplified entry | one-line subscriber; `.Connection` is `INatsConnection`         |
|  [02]   | `INatsClient : IAsyncDisposable`                  | client           | subscribe/publish/ping surface Compute binds against            |
|  [03]   | `INatsConnection : INatsClient`                   | connection       | request/reply, `SubscribeCoreAsync`, lifecycle, `Opts`          |
|  [04]   | `NatsConnection : INatsConnection`                | connection       | concrete per-instance handle (`new NatsConnection(NatsOpts)`)   |
|  [05]   | `NatsOpts` (sealed record)                        | options          | url, name, serializer registry, subscription buffer             |
|  [06]   | `NatsMsg<T>` (readonly record struct)             | message          | `Subject`/`Data`/`Headers`/`ReplyTo`/`Connection`; `ReplyAsync` |
|  [07]   | `NatsHeaders : IDictionary<string, StringValues>` | headers          | `traceparent`/`tracestate` W3C carrier; non-throwing indexer    |
|  [08]   | `NatsSubOpts` (record)                            | sub options      | subscription channel/serializer options for `SubscribeAsync`    |
|  [09]   | `NatsMsgFlags` (byte enum)                        | message bit      | `None`/`Empty`/`NoResponders` — `IsEmpty`/`HasNoResponders`     |
|  [10]   | `INatsDeserialize<T>` / `NatsRawSerializer<T>`    | codec            | per-type deserialize; `NatsRawSerializer<byte[]>` passthrough   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `INatsClient` subscribe, `NatsMsg<T>` read, `NatsHeaders` lookup

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `client.SubscribeAsync<byte[]>(subject, queueGroup?, serializer?, opts?, ct)` | instance | `IAsyncEnumerable<NatsMsg<byte[]>>`   |
|  [02]   | `connection.SubscribeCoreAsync<T>(subject, queueGroup?, …)`                   | instance | `ValueTask<INatsSub<T>>` manual drain |
|  [03]   | `msg.Data`                                                                    | property | `byte[]?` CloudEvents body            |
|  [04]   | `msg.Subject` / `msg.ReplyTo`                                                 | property | routing token / reply subject         |
|  [05]   | `msg.Headers`                                                                 | property | `NatsHeaders?`, null when unset       |
|  [06]   | `msg.Headers?.TryGetValue(name, out StringValues v)`                          | instance | non-throwing W3C extract              |
|  [07]   | `msg.Headers?[name]`                                                          | property | `StringValues`, `.Empty` if absent    |
|  [08]   | `msg.ReplyAsync<TReply>(data, headers?, …)`                                   | instance | reply to a request-subject message    |
|  [09]   | `connection.RequestAsync<TReq, TReply>(subject, data, …)`                     | instance | `ValueTask<NatsMsg<TReply>>`          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `NatsConnection`/`NatsClient` is the long-lived per-instance `IAsyncDisposable` handle, thread-safe, multiplexing every subject over one connection — constructed once and shared across the capture lane, never per-subscription and never a process-global static; Compute owns its subscriber independent of the Persistence egress connection.
- `SubscribeAsync<byte[]>(subject, queueGroup, ct)` drains through `await foreach` until `ct` trips or the connection drops, one `NatsMsg<byte[]>` per iteration; `queueGroup` load-balances the sensor subject across N capture subscribers, null for one.
- Back-pressure rides the `NatsSubOpts` channel bound and `INatsConnection.SlowConsumerDetected`; admission onto the `WorkLane.CaptureIngest` DropOldest row sheds oldest geometry state at the lane rather than blocking the drain.

[STACKING]:
- `api-cloudevents-mqtt`(`.api/api-cloudevents-mqtt.md`): `NatsMsg<byte[]>.Data` (the structured-mode `application/cloudevents+json` body) decodes through the once-constructed shared `JsonEventFormatter<T>` with the pre-declared extension attributes into the typed `SensorEnvelope<T>`, the W3C pair over `NatsMsg.Headers` symmetric to the MQTT `UserProperties` carrier.
- `api-nats`(`Rasm.Persistence/.api/api-nats.md`): the one `NATS.Net` meta-package fans to both overlays — Compute subscribes the Core seam, Persistence owns the JetStream durable-stream, publish-ack, KV, and Object-Store egress and store rails.
- twin loop: a `NatsClient` subscription surfaces one `NatsMsg<byte[]>` per sample → `msg.Data` decodes through the shared formatter → `SensorEnvelope<T>` admits onto `WorkLane.CaptureIngest` DropOldest → `Stats/signal` folds the measured end (`Transform.Modal`) → `TwinLoop.Ingest`/`Update` closes into anomaly verdicts, the identical pipeline the MQTT adapter drives from `MqttApplicationMessage`; `BrokerChannels.Nats<T>` and `BrokerChannels.Mqtt<T>` are two carriers of one `SensorEnvelope<T>`.
- RPC leg: `INatsConnection.RequestAsync<TReq, TReply>` dispatches a compute call awaiting `NatsMsg<TReply>` and `msg.ReplyAsync<TReply>` answers a request-subject message — the request/reply half beside the gRPC `CallSpine`, distinct from the fire-and-forget sensor subscription.

[LOCAL_ADMISSION]:
- ingest decodes `NatsMsg<byte[]>` through the once-constructed shared `JsonEventFormatter<T>` with the pre-declared extension attributes, `NatsRawSerializer<byte[]>` framing the encoded bytes untouched while the formatter owns the shape; `msg.IsEmpty`/`msg.HasNoResponders` (the `NatsMsgFlags` bits) skip an empty control frame before the formatter runs.
- W3C trace continuity is a manual composite carrier read from `NatsMsg.Headers` via `msg.Headers?.TryGetValue("traceparent", out var tp)` beside the `Data` decode — a missing key yields a new root span through the non-throwing indexer, and estate transport law owns this read with no OTel broker instrumentation.

[RAIL_LAW]:
- Package: `NATS.Net` (Compute binds the `NATS.Client.Core` subscription namespace)
- Owns: the NATS Core subscription ingest seam and the `RequestAsync`/`ReplyAsync` remote-compute RPC leg — `INatsClient.SubscribeAsync<byte[]>`, `NatsMsg<byte[]>.Data`, `NatsMsg.Headers`, and `NatsHeaders` W3C lookup for the twin sensor wire
- Accept: one long-lived per-instance `NatsClient`/`NatsConnection` subscriber, `SubscribeAsync<byte[]>` drained with a `CancellationToken`, `NatsMsg.Data` decoded through the shared `JsonEventFormatter<T>`, the W3C pair from `NatsMsg.Headers`, admission onto `WorkLane.CaptureIngest`
- Reject: per-subscription connection construction, a hand-rolled NATS protocol loop `NATS.Client.Core` owns, a per-message formatter instance, JSON hand-spelled at the subject, trace read anywhere but `NatsMsg.Headers`, a per-transport envelope shape parallel to `SensorEnvelope<T>`, or any JetStream/KV/Object surface the Persistence overlay owns
