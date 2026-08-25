# [RASM_PERSISTENCE_API_RABBITMQ]

`RabbitMQ.Client` owns the AMQP 0-9-1 routing-rich egress lane backing the `rabbitmq` binding row: async connection and channel lifecycle, publisher-confirm publish, ack-based consume, and exchange/queue/binding topology, every op `Task`/`ValueTask`-returning and `CancellationToken`-aware. Exchange routing across topic, direct, fanout, and headers types, per-message TTL and priority, and ack-based work-queue dispatch are its owned capability; the `CloudNative.CloudEvents` message envelope rides the body, owned here for publish, consume, and ack, never for its shape.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: connection and channel roots

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]     | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------- | :---------------- | :------------------------------------------------- |
|  [01]   | `IConnectionFactory`                            | factory contract  | builds connections, recovery policy                |
|  [02]   | `ConnectionFactory`                             | factory           | concrete factory + recovery defaults               |
|  [03]   | `IConnection`                                   | connection root   | channel creation, recovery events                  |
|  [04]   | `IChannel`                                      | channel root      | publish/consume/ack/topology surface               |
|  [05]   | `CreateChannelOptions`                          | channel policy    | publisher-confirm + dispatch-concurrency policy    |
|  [06]   | `AmqpTcpEndpoint`                               | endpoint          | host/port/TLS endpoint descriptor                  |
|  [07]   | `IEndpointResolver`                             | endpoint resolver | multi-endpoint connection ordering                 |
|  [08]   | `SslOption`                                     | TLS option        | server cert validation + client cert               |
|  [09]   | `ICredentialsProvider`                          | credential source | rotatable credential provider                      |
|  [10]   | `.OutstandingPublisherConfirmationsRateLimiter` | confirm limiter   | `RateLimiter?`; NULL on every constructed instance |

[PUBLIC_TYPE_SCOPE]: message, properties, and consumer family

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]     | [CAPABILITY]                          |
| :-----: | :----------------------------- | :---------------- | :------------------------------------ |
|  [01]   | `IReadOnlyBasicProperties`     | properties read   | message metadata accessor             |
|  [02]   | `IBasicProperties`             | properties write  | mutable message metadata              |
|  [03]   | `BasicProperties`              | properties value  | concrete mutable properties           |
|  [04]   | `DeliveryModes`                | durability enum   | `Transient` / `Persistent`            |
|  [05]   | `IAsyncBasicConsumer`          | consumer contract | async delivery callback surface       |
|  [06]   | `AsyncEventingBasicConsumer`   | consumer          | event-based async consumer            |
|  [07]   | `AsyncDefaultBasicConsumer`    | consumer base     | override-based async consumer base    |
|  [08]   | `Events.BasicDeliverEventArgs` | delivery event    | tag, body, properties of one delivery |
|  [09]   | `Events.BasicAckEventArgs`     | confirm event     | publisher-confirm ack                 |
|  [10]   | `Events.BasicNackEventArgs`    | confirm event     | publisher-confirm nack                |
|  [11]   | `Events.BasicReturnEventArgs`  | return event      | unroutable message return             |
|  [12]   | `BasicGetResult`               | poll result       | one polled message (`BasicGetAsync`)  |
|  [13]   | `QueueDeclareOk`               | declare result    | queue name + message/consumer counts  |
|  [14]   | `CachedString`                 | interned string   | pre-encoded exchange/routing-key key  |

[PUBLIC_TYPE_SCOPE]: routing, observability, and exception family

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]    | [CAPABILITY]                             |
| :-----: | :--------------------------------------------- | :--------------- | :--------------------------------------- |
|  [01]   | `ExchangeType`                                 | routing enum     | `Direct`/`Fanout`/`Topic`/`Headers`      |
|  [02]   | `PublicationAddress`                           | address value    | exchange + routing-key address           |
|  [03]   | `RabbitMQActivitySource`                       | telemetry seam   | OTel source + trace-context propagation  |
|  [04]   | `RabbitMQTracingOptions`                       | telemetry policy | tracing enable/baggage policy            |
|  [05]   | `Events.ShutdownEventArgs`                     | shutdown event   | reply code/text + initiator              |
|  [06]   | `Exceptions.OperationInterruptedException`     | failure          | broker-initiated operation interrupt     |
|  [07]   | `Exceptions.AlreadyClosedException`            | failure          | operation on a closed channel/connection |
|  [08]   | `Exceptions.BrokerUnreachableException`        | failure          | all endpoints unreachable at connect     |
|  [09]   | `Exceptions.PublishException`                  | failure          | `IsReturn` splits return from nack       |
|  [10]   | `Events.FlowControlEventArgs`                  | flow event       | channel-level broker flow control        |
|  [11]   | `IChannelExtensions` / `IConnectionExtensions` | extensions       | reduced-arity convenience overloads      |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connect and open channel

| [INDEX] | [SURFACE]                                                                     | [SHAPE]       | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `new ConnectionFactory { Uri = … }`                                           | factory init  | configures endpoint + credentials       |
|  [02]   | `ConnectionFactory.CreateConnectionAsync(ct)`                                 | async connect | opens a recovering connection           |
|  [03]   | `CreateConnectionAsync(IEnumerable<AmqpTcpEndpoint>, clientProvidedName, ct)` | async connect | connects across an endpoint list        |
|  [04]   | `IConnection.CreateChannelAsync(CreateChannelOptions?, ct)`                   | async open    | opens a channel (confirm policy bound)  |
|  [05]   | `new CreateChannelOptions(…)`                                                 | ctor          | publisher-confirm + dispatch policy     |
|  [06]   | `IConnection.UpdateSecretAsync(newSecret, reason, ct)`                        | async runtime | rotates OAuth2 token on live connection |
|  [07]   | `IConnection.CloseAsync(reasonCode, reasonText, timeout, abort, ct)`          | async close   | graceful connection close               |

[ENTRYPOINT_SCOPE]: topology declaration

| [INDEX] | [SURFACE]                                                              | [SHAPE]        | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `ExchangeDeclareAsync(exchange, type, durable, autoDelete, …)`         | async topology | declares an exchange                |
|  [02]   | `QueueDeclareAsync(queue, durable, exclusive, autoDelete, …)`          | async topology | declares a queue → `QueueDeclareOk` |
|  [03]   | `QueueBindAsync(queue, exchange, routingKey, …)`                       | async topology | binds a queue to an exchange        |
|  [04]   | `ExchangeBindAsync` / `ExchangeUnbindAsync`                            | async topology | exchange-to-exchange binding        |
|  [05]   | `QueueDeleteAsync(queue, ifUnused, ifEmpty, …)` / `QueuePurgeAsync(…)` | async topology | drop / purge a queue                |
|  [06]   | `MessageCountAsync(queue, ct)` / `ConsumerCountAsync(queue, ct)`       | async probe    | queue depth / consumer count        |

[ENTRYPOINT_SCOPE]: publish, consume, and acknowledge

| [INDEX] | [SURFACE]                                                           | [SHAPE]       | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------ | :------------ | :---------------------------------------- |
|  [01]   | `IChannel.BasicPublishAsync<T>(exchange, routingKey, mandatory, …)` | async publish | publishes; confirms if tracking on        |
|  [02]   | `IChannel.BasicPublishAsync<T>(CachedString exchange, …)`           | async publish | hot-path publish with interned keys       |
|  [03]   | `IChannel.GetNextPublishSequenceNumberAsync(ct)`                    | async confirm | next publisher-confirm sequence #         |
|  [04]   | `IChannel.BasicConsumeAsync(queue, autoAck, consumer, …)`           | async consume | starts an async consumer                  |
|  [05]   | `IChannel.BasicGetAsync(queue, autoAck, ct)`                        | async poll    | pulls one message (`BasicGetResult?`)     |
|  [06]   | `IChannel.BasicQosAsync(prefetchSize, prefetchCount, global, ct)`   | async flow    | sets the consumer prefetch window         |
|  [07]   | `IChannel.BasicAckAsync(deliveryTag, multiple, ct)`                 | async ack     | acknowledges one/all up-to delivery       |
|  [08]   | `IChannel.BasicNackAsync(deliveryTag, multiple, requeue, ct)`       | async nack    | negative-ack with requeue/dead-letter     |
|  [09]   | `IChannel.BasicRejectAsync(deliveryTag, requeue, ct)`               | async reject  | rejects one delivery                      |
|  [10]   | `IChannel.BasicCancelAsync(consumerTag, noWait, ct)`                | async cancel  | cancels a consumer                        |
|  [11]   | `AsyncEventingBasicConsumer.ReceivedAsync += handler`               | event wire    | `BasicDeliverEventArgs` delivery callback |
|  [12]   | `IChannel.TxSelectAsync` / `TxCommitAsync` / `TxRollbackAsync`      | async tx      | AMQP transaction alternative to confirms  |

[ENTRYPOINT_SCOPE]: recovery and observability wiring

| [INDEX] | [SURFACE]                                                            | [SHAPE]    | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------------- | :--------- | :----------------------------------- |
|  [01]   | `IConnection.RecoverySucceededAsync`                                 | event wire | fires after automatic recovery       |
|  [02]   | `IConnection.ConnectionRecoveryErrorAsync`                           | event wire | fires on a recovery attempt failure  |
|  [03]   | `IConnection.ConnectionBlockedAsync`                                 | event wire | broker flow-control / resource alarm |
|  [04]   | `IConnection.ConnectionShutdownAsync` / `CallbackExceptionAsync`     | event wire | shutdown / callback-fault hook       |
|  [05]   | `IChannel.BasicAcksAsync` / `BasicNacksAsync` / `BasicReturnAsync`   | event wire | confirm and return, channel-scoped   |
|  [06]   | `IChannel.FlowControlAsync` / `ChannelShutdownAsync`                 | event wire | channel flow control / shutdown      |
|  [07]   | `RabbitMQActivitySource.ContextInjector` / `ContextExtractor`        | telemetry  | W3C trace-context via headers        |
|  [08]   | `RabbitMQActivitySource.{PublisherSourceName, SubscriberSourceName}` | telemetry  | `ActivitySource` names for OTel      |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every connection, channel, publish, consume, and topology op returns `Task`/`ValueTask` and takes a `CancellationToken`; `BasicAckAsync`/`BasicNackAsync`/`BasicRejectAsync`/`GetNextPublishSequenceNumberAsync`/`BasicPublishAsync` return `ValueTask` on the hot path, topology and consume return `Task`.
- `IChannel` multiplexes one AMQP session over a single TCP `IConnection` and is single-writer for publishes; concurrent publishes need separate channels or external serialization, and `consumerDispatchConcurrency` on `CreateChannelOptions` bounds parallel consumer-callback dispatch.
- Publisher confirms carry durable publish: `CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true)` makes `BasicPublishAsync` await the broker ack and throw on nack. Confirm mode elects THERE alone — `ConfirmSelectAsync`, `WaitForConfirmsAsync`, and a `NextPublishSeqNo` property are absent, `GetNextPublishSequenceNumberAsync` being the only sequence read.
- AMQP `Tx*` is the alternative transaction mechanism, rejected on the durable egress path where confirms apply.
- TRAP: that exact two-argument call leaves in-flight publishes UNBOUNDED. `OutstandingPublisherConfirmationsRateLimiter` is a `public readonly` field whose initializer is `new ThrottlingRateLimiter(128, 50)`, but the public constructor's own parameter for it defaults to `null` and the constructor assigns it, so a caller-built instance carries `null` — which its own doc defines as rate limiting disabled. Only a PASSED third argument mints a limiter at all, and the members stay readonly FIELDS, so no post-construction assignment repairs it.
- `CreateChannelOptions` publishes no static `Default`, and its `consumerDispatchConcurrency` parameter defaults to `1` while the field's own doc claims `null`; the parameter wins, so a channel never inherits `IConnectionFactory.ConsumerDispatchConcurrency` unless `null` is passed explicitly.
- `mandatory` carries NO default on either `BasicPublishAsync` overload, so every call site states it; the two overloads differ only in `string` versus `CachedString` for exchange and routing key.
- Confirm-tracking publish reports both broker refusals through one `PublishException`, whose `IsReturn` is the sole discriminant: `true` is a `basic.return` — an unroutable address no retry reaches — and `false` a `basic.nack` the broker may take on a later attempt, so a fold collapsing the pair either quarantines a re-drivable row or re-offers an address that cannot resolve. `PublishSequenceNumber` pairs the refusal with its publish, and both constructors reject a zero sequence number.
- Automatic and topology recovery, both on by default, reconnect and replay declared exchanges, queues, bindings, and consumers after a connection drop; `RecoverySucceededAsync`/`ConnectionRecoveryErrorAsync` observe it.
- `ReadOnlyMemory<byte>` carries the message body end to end across publish and `BasicDeliverEventArgs.Body`, no per-message `byte[]` allocation.

[STACKING]:
- `CloudNative.CloudEvents` (`api-cloudevents`) frames the body and rides its attributes (`traceparent`, `redacted`, `sequence`) on `BasicProperties.Headers`, so a headers-exchange binding filters on attributes without parsing the body; `RabbitMQ.Client` owns only the publish and ack.
- `RabbitMQActivitySource.ContextInjector`/`ContextExtractor` propagate W3C trace context through `BasicProperties.Headers` and the publisher/subscriber `ActivitySource`s register with the AppHost `telemetry` OpenTelemetry pipeline; the redacted op payload is framed by the redaction codec (`api-redaction`) before publish.
- `IConnection.UpdateSecretAsync` rotates an OAuth2 token on the live connection and `ICredentialsProvider` is the periodic-refresh form; the runtime token authority (`OpenIddict.Client`) is the shared seam binding broker auth to the token provider.
- `OutstandingPublisherConfirmationsRateLimiter` (a `System.Threading.RateLimiting.RateLimiter`) bounds in-flight publishes only where the composing root PASSES one, since the constructor defaults it to `null`; transient connect/publish faults retry through the `Polly`/`stamina` engine rail, `ConnectionBlockedAsync` feeds the connection-scoped broker-resource-alarm shed, and `IChannel.FlowControlAsync` is its channel-scoped peer.
- Dead-lettered messages and shovel/backup snapshots share the object-store residence (`api-objectstore`/`Minio`) with the other egress sinks through the `Store/blobstore` lane.

[LOCAL_ADMISSION]:
- `rabbitmq` binds one `IConnection` per broker and one `IChannel` per publishing path via `CreateChannelAsync` with confirm tracking enabled AND an explicit rate limiter, because the constructor's own default leaves unconfirmed publishes unbounded; the channel confirm policy is fixed at open, never per-publish.
- At-least-once egress: `BasicPublishAsync` awaits the confirm under `mandatory: true` and both broker refusals arrive as one `PublishException`, whose `IsReturn` splits the terminal unroutable address from the re-drivable nack; the `BasicReturnAsync` event carries the returned body for a leg that needs the payload beside that verdict.
- `BasicQosAsync` prefetch and manual `BasicAckAsync`/`BasicNackAsync(requeue)` keep the ack from outrunning durable downstream apply; `autoAck` is rejected on the durable work-queue path.
- Durable topology declares queues `durable: true` with `x-queue-type=quorum` and a dead-letter exchange in `arguments`, per-message TTL and priority riding `BasicProperties.Expiration`/`Priority`; the declaration is idempotent and replayed by topology recovery.
- RabbitMQ owns routing-rich egress — topic and headers exchange, RPC `ReplyTo`, priority — and the partitioned append-log changefeed stays on Kafka (`api-kafka`); the two are distinct binding roster rows, never collapsed.
