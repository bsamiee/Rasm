# [PY_BRANCH_API_PIKA]

`pika` is the reference AMQP 0-9-1 client. Its admitted arm is `BlockingConnection`/`BlockingChannel` — a synchronous facade over the `SelectConnection` ioloop where nearly every method blocks on its own `*-Ok` method frame, exactly one method is thread-safe, and every consumer callback, timer, and heartbeat fires only inside `process_data_events` or `start_consuming`. `spec.BasicProperties` is the AMQP content header the CloudEvents RabbitMQ binding lowers onto, carrying `headers` as an AMQP field table and `content_type` as its own slot. `pika`'s package root imports `asyncio` transitively and unconditionally through its adapter roster — an import-graph fact, never a claim about the blocking arm's runtime.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pika`
- package: `pika` (BSD-3-Clause)
- module: `pika`
- namespaces: `pika.{spec,connection,channel,exceptions,credentials,frame,delivery_mode,exchange_type,data}`, `pika.adapters.{blocking_connection,select_connection,asyncio_connection,base_connection}`, `pika.adapters.utils`
- target: pure-Python wheel, no native asset
- rail: broker-transport

`pika.__all__` is `adapters` `AMQPConnectionWorkflow` `BaseConnection` `BasicProperties` `BlockingConnection` `ConnectionParameters` `DeliveryMode` `PlainCredentials` `SelectConnection` `SSLOptions` `URLParameters`.

## [02]-[PUBLIC_TYPES]

[BLOCKING_SCOPE]: `pika.adapters.blocking_connection` — the admitted arm

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `BlockingConnection` | class         | synchronous facade over `SelectConnection`; owns the ioloop and thread |
|  [02]   | `BlockingChannel`    | class         | one AMQP channel; nearly every method awaits its own `*-Ok`            |
|  [03]   | `ReturnedMessage`    | class         | `(method, properties, body)` triple filling `UnroutableError.messages` |

[SPEC_SCOPE]: `pika.spec` — the AMQP 0-9-1 wire vocabulary

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :--------------------------------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `BasicProperties`                                    | class         | AMQP content header, each field defaulting `None`     |
|  [02]   | `Basic.Deliver`                                      | class         | the delivery envelope                                 |
|  [03]   | `Basic.GetOk`                                        | class         | that envelope beside `message_count`                  |
|  [04]   | `Basic.Return`                                       | class         | `reply_code`, `reply_text`, `exchange`, `routing_key` |
|  [05]   | `Basic.Ack` `.Nack` `.Reject` `.Qos`                 | class         | the settlement and prefetch frames                    |
|  [06]   | `Queue.DeclareOk` `Exchange.DeclareOk` `Tx.SelectOk` | class         | the topology and transaction confirmations            |

`Basic.Deliver` carries `consumer_tag`, `delivery_tag`, `redelivered`, `exchange`, and `routing_key`.

[PARAMETER_SCOPE]: `pika.connection` and `pika.credentials`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :--------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `Parameters`           | class         | the default roster every concrete parameter set inherits         |
|  [02]   | `ConnectionParameters` | class         | sixteen named axes over a `_DEFAULT` sentinel                    |
|  [03]   | `URLParameters`        | class         | the same axes decoded from an `amqp://` URL and its query string |
|  [04]   | `SSLOptions`           | class         | `context: ssl.SSLContext`, `server_hostname: str \| None`        |
|  [05]   | `PlainCredentials`     | class         | `username`, `password`, `erase_on_connect`                       |
|  [06]   | `ExternalCredentials`  | class         | the `EXTERNAL` SASL mechanism, no material                       |
|  [07]   | `DeliveryMode`         | `Enum`        | `Transient=1`, `Persistent=2`                                    |
|  [08]   | `ExchangeType`         | `str` enum    | `direct`, `fanout`, `headers`, `topic`                           |

[BASIC_PROPERTIES]: every slot, in encode order

| [INDEX] | [FIELD]            | [ADMITS]                                      | [WIRE]                              |
| :-----: | :----------------- | :-------------------------------------------- | :---------------------------------- |
|  [01]   | `content_type`     | `str \| bytes`                                | short string, 255 bytes             |
|  [02]   | `content_encoding` | `str \| bytes`                                | short string                        |
|  [03]   | `headers`          | `dict` — an AMQP field table                  | `data.encode_table`                 |
|  [04]   | `delivery_mode`    | `int \| DeliveryMode` — the enum is unwrapped | one byte; 1 transient, 2 persistent |
|  [05]   | `priority`         | `int` 0-9                                     | one byte                            |
|  [06]   | `correlation_id`   | `str \| bytes`                                | short string                        |
|  [07]   | `reply_to`         | `str \| bytes`                                | short string                        |
|  [08]   | `expiration`       | `str \| bytes` — MILLISECONDS as text         | short string                        |
|  [09]   | `message_id`       | `str \| bytes`                                | short string                        |
|  [10]   | `timestamp`        | `int` — unix seconds                          | big-endian unsigned 64-bit          |
|  [11]   | `type`             | `str \| bytes` — shadows the builtin name     | short string                        |
|  [12]   | `user_id`          | `str \| bytes` — the broker verifies it       | short string                        |
|  [13]   | `app_id`           | `str \| bytes`                                | short string                        |
|  [14]   | `cluster_id`       | `str \| bytes`                                | short string                        |

AMQP field-table values admit `str`, `bytes`, `int`, `float`, `Decimal`, `datetime`, `dict`, `list`, `bool`, and `None`.

[FAULT_SCOPE]: `pika.exceptions`, two disjoint roots

| [INDEX] | [ROOT]                | [LEAVES]                                                                                    |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `AMQPConnectionError` | `ConnectionOpenAborted` `StreamLostError` `IncompatibleProtocolError` `AuthenticationError` |
|  [02]   | `AMQPChannelError`    | `ChannelWrongStateError` `DuplicateConsumerTag` `ConsumerCancelled`                         |
|  [03]   | `ProtocolSyntaxError` | `UnexpectedFrameError` `ProtocolVersionMismatch` `BodyTooLongError` `InvalidFrameError`     |
|  [04]   | `AMQPError` direct    | `InvalidChannelNumber` `MethodNotImplemented` `ShortStringTooLong`                          |
|  [05]   | `Exception` direct    | `ChannelError` `DuplicateGetOkCallback` `ReentrancyError`                                   |

`AMQPConnectionError` also seats `ProbableAuthenticationError`, `ProbableAccessDeniedError`, `NoFreeChannels`, `ConnectionWrongStateError`, `ConnectionClosed` with its `ByBroker`/`ByClient` pair, `ConnectionBlockedTimeout`, and `AMQPHeartbeatTimeout`; `AMQPChannelError` also seats `ChannelClosed` with the same pair, `UnroutableError`, and `NackError`; `ProtocolSyntaxError` also seats `InvalidFieldTypeException` and `UnsupportedAMQPFieldException`; `DuplicateGetOkCallback` derives from `ChannelError`, and neither it nor `ReentrancyError` joins `AMQPError`.

`ConnectionClosed` and `ChannelClosed` each expose `.reply_code` and `.reply_text`; `UnroutableError` and `NackError` each carry `.messages` as a `ReturnedMessage` sequence.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection lifecycle

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [BLOCKS] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------- | :------- | :------- | :----------------------------------------------- |
|  [01]   | `BlockingConnection(parameters=None)`                    | ctor     | yes      | dial and open; a `Sequence` is a failover chain  |
|  [02]   | `channel(channel_number=None)`                           | instance | yes      | Channel.Open-ok                                  |
|  [03]   | `close(reply_code=200, reply_text='Normal shutdown')`    | instance | yes      | closes every open channel first                  |
|  [04]   | `process_data_events(time_limit=0)`                      | instance | bounded  | the ONE dispatch pump; `None` blocks on I/O      |
|  [05]   | `sleep(duration)`                                        | instance | bounded  | a `process_data_events` loop, never `time.sleep` |
|  [06]   | `add_callback_threadsafe(callback)`                      | instance | no       | the ONE thread-safe method on the whole class    |
|  [07]   | `call_later(delay, callback)` / `remove_timeout(id)`     | instance | no       | timers dispatched by the pump alone              |
|  [08]   | `add_on_connection_blocked_callback(cb)` / `_unblocked_` | instance | no       | broker flow-control notifications                |
|  [09]   | `update_secret(new_secret, reason)`                      | instance | yes      | credential rotation on a live connection         |

[ENTRYPOINT_SCOPE]: publish and consume

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [BLOCKS]     | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------- | :------- | :----------- | :----------------------------------------- |
|  [01]   | `basic_publish(exchange, routing_key, body, …)`                | instance | on confirms  | round-trips once `confirm_delivery` ran    |
|  [02]   | `basic_consume(queue, on_message_callback, auto_ack=False, …)` | instance | yes          | Consume-ok; answers the tag                |
|  [03]   | `consume(queue, auto_ack=False, inactivity_timeout=None, …)`   | instance | per `next()` | generator over deliveries                  |
|  [04]   | `basic_get(queue, auto_ack=False)`                             | instance | yes          | one pull; GetOk or GetEmpty                |
|  [05]   | `basic_ack(delivery_tag=0, multiple=False)`                    | instance | no           | settle                                     |
|  [06]   | `basic_nack(delivery_tag=0, multiple=False, requeue=True)`     | instance | no           | settle with requeue disposition            |
|  [07]   | `basic_reject(delivery_tag=0, requeue=True)`                   | instance | no           | single-message refusal                     |
|  [08]   | `basic_qos(prefetch_size=0, prefetch_count=0, …)`              | instance | yes          | the prefetch bound                         |
|  [09]   | `basic_cancel(consumer_tag)` / `cancel()`                      | instance | yes          | Cancel-ok; auto-Nacks undispatched work    |
|  [10]   | `start_consuming()` / `stop_consuming(consumer_tag=None)`      | instance | indefinitely | the pump loop while any consumer lives     |
|  [11]   | `confirm_delivery()`                                           | instance | yes          | Confirm.Select-ok; arms publisher confirms |
|  [12]   | `get_waiting_message_count()`                                  | instance | no           | undispatched deliveries on this channel    |
|  [13]   | `add_on_return_callback(cb)` / `add_on_cancel_callback(cb)`    | instance | no           | mandatory-return and broker-cancel taps    |

`basic_publish` continues `properties=None`, `mandatory=False`; `basic_consume` continues `exclusive=False`, `consumer_tag=None`, `arguments=None`; `consume` continues `exclusive=False`, `arguments=None`, `consumer_tag=None`, and yields `(None, None, None)` on inactivity expiry; `basic_qos` continues `global_qos=False`.

[ENTRYPOINT_SCOPE]: topology and transactions — every one blocks on its own `*-Ok`

| [INDEX] | [SURFACE]                                                                                                | [SHAPE]  |
| :-----: | :------------------------------------------------------------------------------------------------------- | :------- |
|  [01]   | `exchange_declare(exchange, exchange_type=ExchangeType.direct, passive=False, …)`                        | instance |
|  [02]   | `exchange_delete(exchange=None, if_unused=False)` / `exchange_bind(...)` / `exchange_unbind(...)`        | instance |
|  [03]   | `queue_declare(queue, passive=False, durable=False, exclusive=False, auto_delete=False, arguments=None)` | instance |
|  [04]   | `queue_delete(queue, if_unused=False, if_empty=False)` / `queue_purge(queue)`                            | instance |
|  [05]   | `queue_bind(queue, exchange, routing_key=None, arguments=None)` / `queue_unbind(...)`                    | instance |
|  [06]   | `tx_select()` / `tx_commit()` / `tx_rollback()`                                                          | instance |
|  [07]   | `flow(active)` / `close(reply_code=0, reply_text='Normal shutdown')`                                     | instance |

`exchange_declare` continues `durable=False`, `auto_delete=False`, `internal=False`, `arguments=None`.

[ENTRYPOINT_SCOPE]: parameters

| [INDEX] | [SURFACE]                                                                                           | [SHAPE] |
| :-----: | :-------------------------------------------------------------------------------------------------- | :------ |
|  [01]   | `ConnectionParameters(host, port, virtual_host, credentials, channel_max, frame_max, heartbeat, …)` | ctor    |
|  [02]   | `URLParameters(url)`                                                                                | ctor    |
|  [03]   | `PlainCredentials(username, password, erase_on_connect=False)` / `ExternalCredentials()`            | ctor    |
|  [04]   | `SSLOptions(context, server_hostname=None)`                                                         | ctor    |

`ConnectionParameters` continues `ssl_options`, `connection_attempts`, `retry_delay`, `socket_timeout`, `stack_timeout`, `locale`, `blocked_connection_timeout`, `client_properties`, and `tcp_options`.

`ConnectionParameters` defaults: host `localhost`, port 5672 (5671 when `ssl_options` is truthy), virtual host `/`, credentials `PlainCredentials('guest', 'guest')`, frame max 131072, connection attempts 1, retry delay 2.0, socket timeout 10.0, stack timeout 15.0, locale `en_US`, heartbeat `None` meaning the broker's proposal is accepted. Any unrecognized keyword raises `TypeError`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `import pika` imports `asyncio` UNCONDITIONALLY and transitively — `pika/__init__.py` imports `pika.adapters`, whose `__init__` eagerly imports `AsyncioConnection`, whose module imports `asyncio` — and there is no lazy path, no extra, and no escape, because `blocking_connection` itself reaches `pika.connection` and so the package root. That import stays inert: the adapter class is merely defined, no loop is created, no loop policy is touched, and nothing runs until that class is instantiated.
- `add_callback_threadsafe` is the ONE thread-safe method on `BlockingConnection`; every other manipulation runs on the connection's own thread. It raises `ConnectionWrongStateError` on a closed or closing connection.
- Consumer callbacks, timers, and connection-blocked notifications dispatch ONLY inside `process_data_events()` and `start_consuming()`. Workers that stop calling one of them stop delivering messages and stop answering heartbeats.
- `basic_publish` blocks only under publisher confirms: outside confirm mode it drains output and returns, and after `confirm_delivery()` it awaits a per-message round trip, raising `NackError` on Basic.Nack and `UnroutableError` on a mandatory return.
- `start_consuming` raises `ReentrancyError` when called from inside any `BlockingConnection` or `BlockingChannel` callback.
- `basic_cancel` diverges by ack mode: on an `auto_ack=False` consumer it auto-Nacks undispatched deliveries and answers an empty sequence; on `auto_ack=True` it answers the pending triples instead of dispatching them, which is a deliberate reentrancy avoidance rather than a loss.
- `consume()` raises `ValueError` when re-entered with a different `(queue, auto_ack, exclusive)` than the live generator holds.
- `BasicProperties` gates its string fields by `assert isinstance(...)` at encode time, which `python -O` removes — a non-string reaching a short-string slot then corrupts the frame rather than refusing.
- `expiration` is a STRING of milliseconds and `timestamp` an integer of unix seconds; neither takes a `timedelta` or a `datetime`, and `delivery_mode` accepts the `DeliveryMode` enum which the constructor unwraps to its value.
- `SSLOptions` refuses a non-`ssl.SSLContext` with `TypeError`; `ConnectionParameters` resolves `port` only after `ssl_options`, so the TLS port default depends on evaluation order the caller does not control.
- Reentrant events arriving inside a nested dispatch are queued and drained once the nesting unwinds, so a callback that itself pumps does not lose deliveries.
- `ChannelError` and `ReentrancyError` derive from bare `Exception` rather than `AMQPError`, so an `except AMQPError` catch-all misses both.
- `pika` carries ZERO runtime deprecations — no `warnings.warn`, no `@deprecated`, no `DeprecationWarning` anywhere.

[STACKING]:
- `anyio`(`.api/anyio.md`): the whole arm rides one `CapacityLimiter`-bounded `to_thread` lane per connection, since the connection is single-threaded by contract. Async callers reach it exclusively through `add_callback_threadsafe(partial(...))`, and every consumer callback re-enters the loop through one `BlockingPortalProvider`. Whichever worker holds the lane calls `process_data_events` on its own cadence so heartbeats and deliveries keep flowing while the scope stays cancellable at that granularity.
- `cloudevents`(`.api/cloudevents.md`): `core.bindings.rabbitmq.RabbitMQMessage` maps field for field — `headers` onto `BasicProperties(headers=)`, `content_type` onto `BasicProperties(content_type=)`, `body` onto `basic_publish(body=)` — and the `ce-` prefix is the binding's, never spelled here.

[LOCAL_ADMISSION]:
- `BlockingConnection`/`BlockingChannel` is the admitted arm whole; `AsyncioConnection`, `TornadoConnection`, `TwistedProtocolConnection`, and `GeventConnection` are refused, because a second loop owner beside the caller's task group is the loop-ownership defect.
- `pika`'s transitive `asyncio` import admits as an import-graph fact: the branch ban governs the branch's own module-scope imports and a dependency's inert class definition creates no loop.
- Publisher confirms arm at composition, so a publish either round-trips or the composition declared at-most-once.
- Every AMQP raise crosses one `boundary` fence into `BoundaryFault`; `ChannelError` and `ReentrancyError` are caught by name rather than through the `AMQPError` root that excludes them.

[RAIL_LAW]:
- Package: `pika`
- Owns: the AMQP 0-9-1 protocol, its content-header vocabulary, topology and transaction verbs, and publisher confirms
- Accept: `BlockingConnection`, `BlockingChannel`, `spec.BasicProperties`, `ConnectionParameters`/`URLParameters`, `PlainCredentials`/`ExternalCredentials`, `SSLOptions`, `DeliveryMode`, `ExchangeType`
- Reject: every non-blocking adapter; a manipulation off the connection's own thread that is not `add_callback_threadsafe`; a bare integer where `DeliveryMode` or `ExchangeType` states the value; `except AMQPError` standing in for the two roots outside it
