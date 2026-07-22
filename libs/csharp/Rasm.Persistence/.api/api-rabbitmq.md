# [RASM_PERSISTENCE_API_RABBITMQ]

`RabbitMQ.Client` owns the AMQP 0-9-1 routing-rich egress lane backing `EgressSink.RabbitMq`: async connection and channel lifecycle, publisher-confirm publish, ack-based consume, and exchange/queue/binding topology, every op `Task`/`ValueTask`-returning and `CancellationToken`-aware. Exchange routing across topic, direct, fanout, and headers types, per-message TTL and priority, and ack-based work-queue dispatch are its owned capability; the `CloudNative.CloudEvents` envelope rides the message body, owned here for publish, consume, and ack, never for its shape.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RabbitMQ.Client`
- package: `RabbitMQ.Client`
- license: `Apache-2.0 OR MPL-2.0` (dual-licensed)
- assembly: `RabbitMQ.Client`
- namespace: `RabbitMQ.Client`, `RabbitMQ.Client.Events`, `RabbitMQ.Client.Exceptions`
- target: multi-target (`net8.0`, `netstandard2.0`); the `net10.0` consumer binds `lib/net8.0` — pure-managed AnyCPU, no native runtime
- rail: amqp-egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: connection and channel roots

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]     | [CAPABILITY]                                           |
| :-----: | :---------------------------------------------- | :---------------- | :----------------------------------------------------- |
|  [01]   | `IConnectionFactory`                            | factory contract  | builds connections, recovery policy                    |
|  [02]   | `ConnectionFactory`                             | factory           | concrete factory + recovery defaults                   |
|  [03]   | `IConnection`                                   | connection root   | channel creation, recovery events                      |
|  [04]   | `IChannel`                                      | channel root      | publish/consume/ack/topology surface                   |
|  [05]   | `CreateChannelOptions`                          | channel policy    | publisher-confirm + dispatch-concurrency policy        |
|  [06]   | `AmqpTcpEndpoint`                               | endpoint          | host/port/TLS endpoint descriptor                      |
|  [07]   | `IEndpointResolver`                             | endpoint resolver | multi-endpoint connection ordering                     |
|  [08]   | `SslOption`                                     | TLS option        | server cert validation + client cert                   |
|  [09]   | `ICredentialsProvider`                          | credential source | rotatable credential provider                          |
|  [10]   | `.OutstandingPublisherConfirmationsRateLimiter` | confirm limiter   | default `ThrottlingRateLimiter(128, 50)` back-pressure |

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
|  [09]   | `IChannelExtensions` / `IConnectionExtensions` | extensions       | reduced-arity convenience overloads      |

## [03]-[ENTRYPOINTS]

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
|  [05]   | `RabbitMQActivitySource.ContextInjector` / `ContextExtractor`        | telemetry  | W3C trace-context via headers        |
|  [06]   | `RabbitMQActivitySource.{PublisherSourceName, SubscriberSourceName}` | telemetry  | `ActivitySource` names for OTel      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every connection, channel, publish, consume, and topology op returns `Task`/`ValueTask` and takes a `CancellationToken`; `BasicAckAsync`/`BasicNackAsync`/`BasicRejectAsync`/`GetNextPublishSequenceNumberAsync`/`BasicPublishAsync` return `ValueTask` on the hot path, topology and consume return `Task`.
- `IChannel` multiplexes one AMQP session over a single TCP `IConnection` and is single-writer for publishes; concurrent publishes need separate channels or external serialization, and `consumerDispatchConcurrency` on `CreateChannelOptions` bounds parallel consumer-callback dispatch.
- Publisher confirms are the durable-publish mechanism: `CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true)` makes `BasicPublishAsync` await the broker ack and throw on nack, the `OutstandingPublisherConfirmationsRateLimiter` bounding in-flight unconfirmed publishes; AMQP `Tx*` is the alternative transaction mechanism, rejected on the durable egress path where confirms apply.
- Automatic and topology recovery, both on by default, reconnect and replay declared exchanges, queues, bindings, and consumers after a connection drop; `RecoverySucceededAsync`/`ConnectionRecoveryErrorAsync` observe it.
- `ReadOnlyMemory<byte>` carries the message body end to end across publish and `BasicDeliverEventArgs.Body`, no per-message `byte[]` allocation.

[STACKING]:
- `CloudNative.CloudEvents` (`api-cloudevents`) frames the body and rides its attributes (`traceparent`, `redacted`, `sequence`) on `BasicProperties.Headers`, so a headers-exchange binding filters on attributes without parsing the body; `RabbitMQ.Client` owns only the publish and ack.
- `RabbitMQActivitySource.ContextInjector`/`ContextExtractor` propagate W3C trace context through `BasicProperties.Headers` and the publisher/subscriber `ActivitySource`s register with the AppHost `telemetry` OpenTelemetry pipeline; the redacted op payload is framed by the redaction codec (`api-redaction`) before publish.
- `IConnection.UpdateSecretAsync` rotates an OAuth2 token on the live connection and `ICredentialsProvider` is the periodic-refresh form; the runtime token authority (`OpenIddict.Client`) is the shared seam binding broker auth to the token provider.
- `OutstandingPublisherConfirmationsRateLimiter` (a `System.Threading.RateLimiting.RateLimiter`) bounds in-flight publishes and transient connect/publish faults retry through the `Polly`/`stamina` engine rail; `ConnectionBlockedAsync` feeds the broker-resource-alarm back-pressure shed.
- Dead-lettered messages and shovel/backup snapshots share the object-store residence (`api-objectstore`/`Minio`) with the other egress sinks through the `Store/blobstore` lane.

[LOCAL_ADMISSION]:
- `EgressSink.RabbitMq` builds one `IConnection` per broker and one `IChannel` per publishing path via `CreateChannelAsync` with confirm tracking enabled; the channel confirm policy is fixed at open, never per-publish.
- At-least-once egress: `BasicPublishAsync` awaits the confirm and a nack triggers the retry rail; `mandatory: true` with a `BasicReturnEventArgs` handler routes an unroutable message to dead-letter rather than dropping it.
- `BasicQosAsync` prefetch and manual `BasicAckAsync`/`BasicNackAsync(requeue)` keep the ack from outrunning durable downstream apply; `autoAck` is rejected on the durable work-queue path.
- Durable topology declares queues `durable: true` with `x-queue-type=quorum` and a dead-letter exchange in `arguments`, per-message TTL and priority riding `BasicProperties.Expiration`/`Priority`; the declaration is idempotent and replayed by topology recovery.
- RabbitMQ owns routing-rich egress — topic and headers exchange, RPC `ReplyTo`, priority — and the partitioned append-log changefeed stays on Kafka (`api-kafka`); the two are distinct `EgressSink` rows, never collapsed.

[RAIL_LAW]:
- Package: `RabbitMQ.Client`
- Owns: AMQP 0-9-1 routing-rich egress — connection and channel lifecycle, exchange/queue/binding topology, publisher-confirm publish, ack-based consume, and built-in trace-context propagation
- Accept: the async `IConnection`/`IChannel` surface, `CreateChannelOptions` publisher confirms with the rate limiter, generic `BasicPublishAsync<T>`, manual `BasicQosAsync` with `BasicAckAsync`/`BasicNackAsync`, and `RabbitMQActivitySource` propagation
- Reject: a hand-rolled AMQP framing or confirm-tracking loop, AMQP `Tx*` on the durable path where confirms apply, `autoAck` on the durable work queue, and a hand-rolled trace-context or retry implementation where the client owns it
