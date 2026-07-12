# [RASM_PERSISTENCE_API_RABBITMQ]

`RabbitMQ.Client` is the official AMQP 0-9-1 protocol client in its fully-async v7 form, backing `EgressSink.RabbitMq` (`Version/egress#EGRESS_SINK`). The v7 surface is a clean break from v6: connection and channel creation, publish, consume, ack/nack, and all topology (`exchange`/`queue`/`binding`) admin are `Task`/`ValueTask`-returning and `CancellationToken`-aware; publisher confirms are a first-class `CreateChannelOptions` policy with a built-in `RateLimiter` (no manual `WaitForConfirms`); `BasicPublishAsync<TProperties>` is generic over `IReadOnlyBasicProperties, IAmqpHeader` so properties carry no boxing; and W3C trace-context propagation is built into `RabbitMQActivitySource`. This is the routing-rich messaging lane distinct from the Kafka log/changefeed (`api-kafka`): RabbitMQ owns topic/direct/fanout/headers exchange routing, per-message TTL/priority, and ack-based work-queue semantics where Kafka owns the partitioned append log. The `CloudNative.CloudEvents` envelope rides on top; `RabbitMQ.Client` owns only the publish/consume/ack, never the envelope shape.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RabbitMQ.Client`
- package: `RabbitMQ.Client`
- license: `Apache-2.0 OR MPL-2.0` (dual-licensed)
- assembly: `RabbitMQ.Client`
- namespace: `RabbitMQ.Client`, `RabbitMQ.Client.Events`, `RabbitMQ.Client.Exceptions`
- target: multi-target (`net8.0`, `netstandard2.0`); the `net10.0` consumer binds `lib/net8.0` — pure-managed AnyCPU, no native runtime
- xml docs: `RabbitMQ.Client.xml` ships beside the assembly; member intent is doc-comment-sourced
- rail: amqp-egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: connection and channel roots
- rail: amqp-egress

`IConnectionFactory`/`ConnectionFactory` build connections from a `Uri` or host/port + credentials; `AutomaticRecoveryEnabled` (default `true`), `TopologyRecoveryEnabled` (default `true`), and `NetworkRecoveryInterval` (default 5s) drive transparent reconnection and topology replay. `IConnection.CreateChannelAsync(CreateChannelOptions?)` opens a channel (the AMQP multiplexed session); `IConnection.UpdateSecretAsync` rotates the OAuth2 token on a live connection. `IChannel` is the unit of all publish/consume/topology work and is not thread-safe for concurrent publishes on one instance.

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY]     | [RAIL]                                                 |
| :-----: | :------------------------------------------------------------------ | :---------------- | :----------------------------------------------------- |
|  [01]   | `IConnectionFactory`                                                | factory contract  | builds connections, recovery policy                    |
|  [02]   | `ConnectionFactory`                                                 | factory           | concrete factory + recovery defaults                   |
|  [03]   | `IConnection`                                                       | connection root   | channel creation, recovery events                      |
|  [04]   | `IChannel`                                                          | channel root      | publish/consume/ack/topology surface                   |
|  [05]   | `CreateChannelOptions`                                              | channel policy    | publisher-confirm + dispatch-concurrency policy        |
|  [06]   | `AmqpTcpEndpoint`                                                   | endpoint          | host/port/TLS endpoint descriptor                      |
|  [07]   | `IEndpointResolver`                                                 | endpoint resolver | multi-endpoint connection ordering                     |
|  [08]   | `SslOption`                                                         | TLS option        | server cert validation + client cert                   |
|  [09]   | `ICredentialsProvider`                                              | credential source | rotatable credential provider                          |
|  [10]   | `CreateChannelOptions.OutstandingPublisherConfirmationsRateLimiter` | confirm limiter   | default `ThrottlingRateLimiter(128, 50)` back-pressure |

[PUBLIC_TYPE_SCOPE]: message, properties, and consumer family
- rail: amqp-egress

`IReadOnlyBasicProperties`/`IBasicProperties`/`BasicProperties` carry the AMQP message metadata: `Persistent`/`DeliveryMode` (durable message), `Headers` (the header-exchange + CloudEvents attribute carrier), `CorrelationId`, `ReplyTo`/`ReplyToAddress` (RPC), `Expiration` (per-message TTL), `Priority`, `ContentType`/`ContentEncoding`, `MessageId`, `Timestamp`. `IAsyncBasicConsumer`/`AsyncEventingBasicConsumer` is the v7 consumer; its `ReceivedAsync` event delivers `BasicDeliverEventArgs` with the body as `ReadOnlyMemory<byte>`.

| [INDEX] | [SYMBOL]                                                                   | [TYPE_FAMILY]        | [RAIL]                                |
| :-----: | :------------------------------------------------------------------------- | :------------------- | :------------------------------------ |
|  [01]   | `IReadOnlyBasicProperties`                                                 | properties read      | message metadata accessor             |
|  [02]   | `IBasicProperties`                                                         | properties write     | mutable message metadata              |
|  [03]   | `BasicProperties`                                                          | properties value     | concrete mutable properties           |
|  [04]   | `DeliveryModes`                                                            | durability enum      | `Transient` / `Persistent`            |
|  [05]   | `IAsyncBasicConsumer`                                                      | consumer contract    | async delivery callback surface       |
|  [06]   | `AsyncEventingBasicConsumer`                                               | consumer             | event-based async consumer            |
|  [07]   | `AsyncDefaultBasicConsumer`                                                | consumer base        | override-based async consumer base    |
|  [08]   | `Events.BasicDeliverEventArgs`                                             | delivery event       | tag, body, properties of one delivery |
|  [09]   | `Events.BasicAckEventArgs` / `BasicNackEventArgs` / `BasicReturnEventArgs` | confirm/return event | publisher-confirm / unroutable return |
|  [10]   | `BasicGetResult`                                                           | poll result          | one polled message (`BasicGetAsync`)  |
|  [11]   | `QueueDeclareOk`                                                           | declare result       | queue name + message/consumer counts  |
|  [12]   | `CachedString`                                                             | interned string      | pre-encoded exchange/routing-key key  |

[PUBLIC_TYPE_SCOPE]: routing, observability, and exception family
- rail: amqp-egress

`ExchangeType` names the exchange kinds (`Direct`/`Fanout`/`Topic`/`Headers`); `Headers` carries the well-known header keys. `RabbitMQActivitySource` is the built-in OpenTelemetry seam: `PublisherSourceName`/`SubscriberSourceName` are the `ActivitySource` names, and `ContextInjector`/`ContextExtractor` propagate W3C trace context through message headers.

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]    | [RAIL]                                   |
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
- rail: amqp-egress

`CreateChannelOptions(publisherConfirmationsEnabled, publisherConfirmationTrackingEnabled, outstandingPublisherConfirmationsRateLimiter?, consumerDispatchConcurrency?)` is the v7 confirms policy: with tracking enabled, `BasicPublishAsync` itself awaits the broker ack and throws on nack, replacing the v6 manual `WaitForConfirmsOrDie`. The `OutstandingPublisherConfirmationsRateLimiter` (default `ThrottlingRateLimiter(128, 50)`) bounds in-flight unconfirmed publishes.

| [INDEX] | [SURFACE]                                                                                                                                 | [CALL_SHAPE]  | [CAPABILITY]                              |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `new ConnectionFactory { Uri = … }`                                                                                                       | factory init  | configures endpoint + credentials         |
|  [02]   | `ConnectionFactory.CreateConnectionAsync(ct)`                                                                                             | async connect | opens a recovering connection             |
|  [03]   | `CreateConnectionAsync(IEnumerable<AmqpTcpEndpoint>, clientProvidedName, ct)`                                                             | async connect | connects across an endpoint list          |
|  [04]   | `IConnection.CreateChannelAsync(CreateChannelOptions?, ct)`                                                                               | async open    | opens a channel (confirm policy bound)    |
|  [05]   | `new CreateChannelOptions(publisherConfirmationsEnabled, publisherConfirmationTrackingEnabled, rateLimiter, consumerDispatchConcurrency)` | ctor          | publisher-confirm + dispatch policy       |
|  [06]   | `IConnection.UpdateSecretAsync(newSecret, reason, ct)`                                                                                    | async runtime | rotates OAuth2 token on a live connection |
|  [07]   | `IConnection.CloseAsync(reasonCode, reasonText, timeout, abort, ct)`                                                                      | async close   | graceful connection close                 |

[ENTRYPOINT_SCOPE]: topology declaration
- rail: amqp-egress

Every topology method is idempotent server-side and carries `noWait`/`passive`/`arguments` so quorum-queue and dead-letter arguments (`x-queue-type`, `x-dead-letter-exchange`, `x-message-ttl`, `x-max-priority`) pass through the `arguments` dictionary.

| [INDEX] | [SURFACE]                                                                                            | [CALL_SHAPE]   | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `IChannel.ExchangeDeclareAsync(exchange, type, durable, autoDelete, arguments, passive, noWait, ct)` | async topology | declares an exchange                |
|  [02]   | `IChannel.QueueDeclareAsync(queue, durable, exclusive, autoDelete, arguments, …)`                    | async topology | declares a queue → `QueueDeclareOk` |
|  [03]   | `IChannel.QueueBindAsync(queue, exchange, routingKey, arguments, noWait, ct)`                        | async topology | binds a queue to an exchange        |
|  [04]   | `IChannel.ExchangeBindAsync` / `ExchangeUnbindAsync`                                                 | async topology | exchange-to-exchange binding        |
|  [05]   | `IChannel.QueueDeleteAsync(queue, ifUnused, ifEmpty, …)` / `QueuePurgeAsync(queue, ct)`              | async topology | drop / purge a queue                |
|  [06]   | `IChannel.MessageCountAsync(queue, ct)` / `ConsumerCountAsync(queue, ct)`                            | async probe    | queue depth / consumer count        |

[ENTRYPOINT_SCOPE]: publish, consume, and acknowledge
- rail: amqp-egress

`BasicPublishAsync<TProperties>` has a `(string exchange, string routingKey, …)` overload and a `(CachedString exchange, CachedString routingKey, …)` overload for pre-interned hot-path keys; both take `mandatory`, the generic `basicProperties`, and a `ReadOnlyMemory<byte>` body. `BasicConsumeAsync` binds an `IAsyncBasicConsumer`; `BasicQosAsync` sets the prefetch window for fair work-queue dispatch.

| [INDEX] | [SURFACE]                                                                                              | [CALL_SHAPE]  | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------------------------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `IChannel.BasicPublishAsync<T>(exchange, routingKey, mandatory, basicProperties, body, ct)`            | async publish | publishes (awaits confirm if tracking on)   |
|  [02]   | `IChannel.BasicPublishAsync<T>(CachedString exchange, CachedString routingKey, …)`                     | async publish | hot-path publish with interned keys         |
|  [03]   | `IChannel.GetNextPublishSequenceNumberAsync(ct)`                                                       | async confirm | next publisher-confirm sequence number      |
|  [04]   | `IChannel.BasicConsumeAsync(queue, autoAck, consumerTag, noLocal, exclusive, arguments, consumer, ct)` | async consume | starts an async consumer                    |
|  [05]   | `IChannel.BasicGetAsync(queue, autoAck, ct)`                                                           | async poll    | pulls one message (`BasicGetResult?`)       |
|  [06]   | `IChannel.BasicQosAsync(prefetchSize, prefetchCount, global, ct)`                                      | async flow    | sets the consumer prefetch window           |
|  [07]   | `IChannel.BasicAckAsync(deliveryTag, multiple, ct)`                                                    | async ack     | acknowledges one/all up-to delivery         |
|  [08]   | `IChannel.BasicNackAsync(deliveryTag, multiple, requeue, ct)`                                          | async nack    | negative-ack with requeue/dead-letter       |
|  [09]   | `IChannel.BasicRejectAsync(deliveryTag, requeue, ct)`                                                  | async reject  | rejects one delivery                        |
|  [10]   | `IChannel.BasicCancelAsync(consumerTag, noWait, ct)`                                                   | async cancel  | cancels a consumer                          |
|  [11]   | `AsyncEventingBasicConsumer.ReceivedAsync += handler`                                                  | event wire    | `BasicDeliverEventArgs` delivery callback   |
|  [12]   | `IChannel.TxSelectAsync` / `TxCommitAsync` / `TxRollbackAsync`                                         | async tx      | AMQP transaction (rarely used vs. confirms) |

[ENTRYPOINT_SCOPE]: recovery and observability wiring
- rail: amqp-egress

| [INDEX] | [SURFACE]                                                                              | [CALL_SHAPE] | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------------------------------- | :----------- | :------------------------------------------- |
|  [01]   | `IConnection.RecoverySucceededAsync += handler`                                        | event wire   | fires after automatic recovery completes     |
|  [02]   | `IConnection.ConnectionRecoveryErrorAsync += handler`                                  | event wire   | fires on a recovery attempt failure          |
|  [03]   | `IConnection.ConnectionBlockedAsync += handler`                                        | event wire   | broker flow-control / resource-alarm block   |
|  [04]   | `IConnection.ConnectionShutdownAsync += handler` / `CallbackExceptionAsync += handler` | event wire   | shutdown / callback-fault hook               |
|  [05]   | `RabbitMQActivitySource.ContextInjector` / `ContextExtractor`                          | telemetry    | W3C trace-context propagation via headers    |
|  [06]   | `RabbitMQActivitySource.{PublisherSourceName, SubscriberSourceName}`                   | telemetry    | `ActivitySource` names for OTel registration |

## [04]-[IMPLEMENTATION_LAW]

[RABBITMQ_TOPOLOGY]:
- v7 is fully async: there is NO synchronous `IModel`/`CreateModel`/`BasicPublish`. `IConnection`→`IChannel` replaces `IConnection`→`IModel`; every operation is `Task`/`ValueTask`. `BasicAckAsync`/`BasicNackAsync`/`BasicRejectAsync`/`GetNextPublishSequenceNumberAsync`/`BasicPublishAsync` are `ValueTask` (hot path); topology and consume are `Task`.
- `IChannel` is the multiplexed AMQP session over one TCP `IConnection`; it is single-writer for publishes — concurrent publishes need separate channels or external serialization. `consumerDispatchConcurrency` on `CreateChannelOptions` controls parallel consumer-callback dispatch.
- publisher confirms are the durable-publish mechanism, not transactions: `CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true)` makes `BasicPublishAsync` await the broker ack and throw on nack; the `OutstandingPublisherConfirmationsRateLimiter` bounds in-flight unconfirmed publishes. AMQP `Tx*` is the legacy alternative and is rejected on the durable egress path.
- automatic + topology recovery (both on by default) transparently reconnect and replay declared exchanges/queues/bindings/consumers after a connection drop; `RecoverySucceededAsync`/`ConnectionRecoveryErrorAsync` observe it.
- the message body is `ReadOnlyMemory<byte>` end to end (publish and `BasicDeliverEventArgs.Body`) — zero-copy, no `byte[]` allocation per message.

[LOCAL_ADMISSION]:
- `EgressSink.RabbitMq` builds one `IConnection` per broker and one `IChannel` per publishing path through `CreateChannelAsync` with confirms tracking enabled; the channel's confirm policy is fixed at open, never per-publish.
- at-least-once egress: `BasicPublishAsync` awaits the confirm; a nack (surfaced as a throw under tracking, or via `BasicNackEventArgs` under manual tracking) triggers the retry rail. `mandatory: true` plus a `BasicReturnEventArgs` handler routes an unroutable message to dead-letter rather than silently dropping it.
- the consumer side uses `BasicQosAsync` prefetch + manual `BasicAckAsync`/`BasicNackAsync(requeue)` so the ack never outruns durable downstream apply; `autoAck` is rejected on the durable work-queue path.
- durable topology: queues are declared `durable: true` with `x-queue-type=quorum` and a dead-letter exchange in `arguments`; per-message TTL/priority ride `BasicProperties.Expiration`/`Priority`. The declaration is idempotent and replayed by topology recovery.
- routing vs. log: RabbitMQ owns routing-rich egress (topic/headers exchange, RPC `ReplyTo`, priority); the partitioned append-log changefeed stays on Kafka (`api-kafka`). The two are distinct `EgressSink` rows, never collapsed.

[STACKING]:
- CloudEvents envelope: the message body is the CloudEvents-framed payload and the CloudEvents attributes (`traceparent`, `redacted`, `sequence`) ride `BasicProperties.Headers`, so a headers-exchange binding filters on attributes without parsing the body — built via `CloudNative.CloudEvents` (`api-cloudevents`); `RabbitMQ.Client` owns only the publish/ack, never the envelope. This mirrors the Kafka binding's header-carried-attributes pattern exactly.
- telemetry: `RabbitMQActivitySource.ContextInjector`/`ContextExtractor` propagate W3C trace context through `BasicProperties.Headers`, and the publisher/subscriber `ActivitySource`s register with the AppHost `telemetry` OpenTelemetry pipeline — the publish/consume span is first-class, not a bespoke logger. The redacted op payload is framed by the redaction codec (`api-redaction`) before publish.
- credential rotation: `IConnection.UpdateSecretAsync` rotates an OAuth2 token on the live connection, the AMQP counterpart to Kafka's `SetSaslCredentials`/`SetOAuthBearerTokenRefreshHandler` — the runtime token authority (`OpenIddict.Client`) is the shared seam binding broker auth to the token provider; `ICredentialsProvider` is the periodic-refresh form.
- retry/back-pressure: the `OutstandingPublisherConfirmationsRateLimiter` (a `System.Threading.RateLimiting.RateLimiter`) is the in-flight bound; transient connect/publish faults retry through the `Polly`/`stamina`-shaped engine rail, never a hand-rolled loop. `ConnectionBlockedAsync` (broker resource alarm) feeds the same back-pressure shed the Kafka consumer-lag probe drives.
- snapshot/DLQ residence: dead-lettered messages and shovel/backup snapshots share the object-store residence (`api-objectstore`/`Minio`) with the other egress sinks via the `Store/blobstore` lane.

[RAIL_LAW]:
- Package: `RabbitMQ.Client`
- Owns: AMQP 0-9-1 routing-rich egress — connection/channel lifecycle, exchange/queue/binding topology, publisher-confirm publish, ack-based consume, and built-in trace-context propagation
- Accept: the async `IConnection`/`IChannel` v7 surface, `CreateChannelOptions` publisher confirms with the rate limiter, generic `BasicPublishAsync<T>`, manual `BasicQosAsync`+`BasicAckAsync`/`BasicNackAsync`, and `RabbitMQActivitySource` propagation
- Reject: the removed synchronous `IModel`/`BasicPublish` surface, AMQP `Tx*` on the durable path where confirms exist, `autoAck` on the durable work queue, and a hand-rolled trace-context/retry implementation where the client owns it
