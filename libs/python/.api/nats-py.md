# [PY_BRANCH_API_NATS_PY]

`nats-py` is the NATS core and JetStream client: subject-addressed publish and subscribe, request-reply over an inbox, optional message headers negotiated off the server's own INFO advertisement, and a JetStream layer carrying streams, push and pull consumers, key-value buckets, and object stores. It is natively asynchronous and asyncio-locked — every internal task, queue, and future is an asyncio primitive and no `anyio` or `trio` surface exists — so it composes on the asyncio backend alone and its reader, ping, and flusher tasks live at the loop rather than as children of a caller's task group. Its distribution declares zero required dependencies.

## [01]-[PUBLIC_TYPES]

[CLIENT_SCOPE]: `nats.aio.client`

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :------------------ | :------------ | :------------------------------------------------------------------------ |
|  [01]   | `Client`            | class         | the connection, its reader/ping/flusher tasks, and the subscription table |
|  [02]   | `Callback`          | type alias    | `Callable[[], Awaitable[None]]` — every lifecycle hook                    |
|  [03]   | `ErrorCallback`     | type alias    | `Callable[[Exception], Awaitable[None]]`                                  |
|  [04]   | `SignatureCallback` | type alias    | `Callable[[str], bytes]` — the NKEYS challenge signer                     |
|  [05]   | `JWTCallback`       | type alias    | `Callable[[], bytearray \| bytes]`                                        |
|  [06]   | `TokenCallback`     | type alias    | `Callable[[], str]`                                                       |
|  [07]   | `Credentials`       | type alias    | a path, a `(jwt, seed)` pair, raw credentials, or a `Path`                |

`Client` carries the connection-state constants `DISCONNECTED` `CONNECTED` `CLOSED` `RECONNECTING` `CONNECTING` `DRAINING_SUBS` `DRAINING_PUBS`, and the module fixes the wire defaults — a 2-second connect timeout and reconnect wait, sixty reconnect attempts, a 120-second ping interval over two outstanding pings, a 2 MiB pending buffer, a 1 MiB default max payload, a 30-second drain timeout, and the `_INBOX` prefix.

[MESSAGE_SCOPE]: `nats.aio.msg` and `nats.aio.subscription`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]    | [FIELDS_OR_MEMBERS]                                                                     |
| :-----: | :-------------------------- | :--------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `Msg`                       | dataclass        | `subject`, `reply`, `data`, `headers`, and the private client, sid, and metadata        |
|  [02]   | `Msg.Metadata`              | frozen dataclass | `sequence`, `num_pending`, `num_delivered`, `timestamp`, `stream`, `consumer`, `domain` |
|  [03]   | `Msg.Metadata.SequencePair` | frozen dataclass | `consumer`, `stream`                                                                    |
|  [04]   | `Msg.Ack`                   | class            | the settlement tokens `+ACK`, `-NAK`, `+WPI`, `+TERM`                                   |
|  [05]   | `Subscription`              | class            | `subject`, `queue`, `delivered`, `pending_msgs`, `pending_bytes`, `messages`            |

[JETSTREAM_SCOPE]: `nats.js`

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                                         |
| :-----: | :---------------------------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `JetStreamManager`                  | class         | stream, consumer, and message administration over `$JS.API`          |
|  [02]   | `JetStreamContext`                  | class         | SUBCLASSES the manager, so every admin method rides the context too  |
|  [03]   | `JetStreamContext.PullSubscription` | class         | `fetch`, `consumer_info`, `unsubscribe`, and the pending counters    |
|  [04]   | `JetStreamContext.PushSubscription` | class         | a `Subscription` widened with `consumer_info`                        |
|  [05]   | `nats.js.kv.KeyValue`               | class         | the bucket, its `Entry`, `BucketStatus`, and `KeyWatcher`            |
|  [06]   | `nats.js.object_store.ObjectStore`  | class         | the bucket, its `ObjectResult`, `ObjectStoreStatus`, `ObjectWatcher` |

[JETSTREAM_API]: `nats.js.api` — mutable dataclasses over `as_dict()`/`from_response()` beside closed enums

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY] | [CARRIES]                                              |
| :-----: | :------------------------------------------------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `PubAck`                                                       | dataclass     | `stream`, `seq`, `domain`, `duplicate`                 |
|  [02]   | `StreamConfig` `StreamInfo` `StreamState` `StreamSource`       | dataclass     | stream declaration and its observed state              |
|  [03]   | `ConsumerConfig` `ConsumerInfo` `ConsumerPause`                | dataclass     | consumer declaration, its state, and its pause verdict |
|  [04]   | `KeyValueConfig` `ObjectStoreConfig` `ObjectInfo` `ObjectMeta` | dataclass     | bucket families                                        |
|  [05]   | `AccountInfo` `AccountLimits` `APIStats` `Tier`                | dataclass     | account quota and usage                                |
|  [06]   | `Placement` `PeerInfo` `ClusterInfo` `ExternalStream`          | dataclass     | cluster topology                                       |
|  [07]   | `AckPolicy`                                                    | `str` enum    | settlement policy                                      |
|  [08]   | `DeliverPolicy`                                                | `str` enum    | consumer start point                                   |
|  [09]   | `RetentionPolicy` `DiscardPolicy` `ReplayPolicy`               | `str` enum    | retention, overflow disposition, replay pacing         |
|  [10]   | `StorageType` `StoreCompression` `PersistMode`                 | `str` enum    | backing store, compression, persist mode               |
|  [11]   | `StatusCode`                                                   | `str` enum    | inbound status-line codes                              |
|  [12]   | `Header`                                                       | `str` enum    | reserved header keys                                   |

`AckPolicy` spells `none`, `all`, `explicit`; `DeliverPolicy` spells `all`, `last`, `new`, `by_start_sequence`, `by_start_time`, `last_per_subject`; `RetentionPolicy` `limits`/`interest`/`workqueue`, `DiscardPolicy` `old`/`new`, `ReplayPolicy` `instant`/`original`; `StorageType` `file`/`memory`, `StoreCompression` `none`/`s2`, `PersistMode` `default`/`async`; `StatusCode` `503`, `404`, `408`, `409`, `100`; `Header` `Nats-Msg-Id`, the `Nats-Expected-*` keys, `Nats-TTL`, `Nats-Rollup`, `Status`, `Description`.

[FAULT_SCOPE]: `nats.errors` rooted at `Error(Exception)`

| [INDEX] | [GROUP]       | [MEMBERS]                                                                                               |
| :-----: | :------------ | :------------------------------------------------------------------------------------------------------ |
|  [01]   | connection    | `NoServersError` `ConnectionClosedError` `ConnectionDrainingError` `ConnectionReconnectingError`        |
|  [02]   | authorization | `AuthorizationError` `InvalidUserCredentialsError` `SecureConnFailedError` `SecureConnRequiredError`    |
|  [03]   | protocol      | `ProtocolError` `JsonParseError` `BadSubjectError` `BadSubscriptionError` `BadTimeoutError`             |
|  [04]   | flow          | `MaxPayloadError` `OutboundBufferLimitError` `SlowConsumerError` `NoRespondersError`                    |
|  [05]   | settlement    | `MsgAlreadyAckdError` `NotJSMessageError`                                                               |
|  [06]   | deadline      | `TimeoutError` extending both `Error` and the builtin, with `DrainTimeoutError` and `FlushTimeoutError` |

`StaleConnectionError`, `UnexpectedEOF`, and `ServerNotInPoolError` join the connection group; `SecureConnWantedError` joins authorization; `InvalidCallbackTypeError` joins protocol.

`nats.js.errors` roots its own `Error` on `nats.errors.Error` and layers `APIError` with the code-keyed `ServerError` `NotFoundError` `BadRequestError` `ServiceUnavailableError`, the key-value family `KeyValueError` `KeyNotFoundError` `KeyDeletedError` `KeyWrongLastSequenceError` `NoKeysError` `KeyHistoryTooLargeError` `BucketNotFoundError` `BadBucketError`, the object-store family `ObjectNotFoundError` `ObjectDeletedError` `ObjectAlreadyExists` `BadObjectMetaError` `DigestMismatchError`, and the flow family `NoStreamResponseError` `ConsumerSequenceMismatchError` `FetchTimeoutError` `TooManyStalledMsgsError`.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `nats.connect(servers, **options)`                                 | static   | construct and connect in one call            |
|  [02]   | `Client.connect(servers, error_cb, disconnected_cb, closed_cb, …)` | instance | the whole option roster                      |
|  [03]   | `flush(timeout=10)` / `rtt(timeout=10)`                            | instance | round-trip barrier and its measured latency  |
|  [04]   | `drain()` / `close()` / `force_reconnect()`                        | instance | orderly shed, hard close, deliberate re-dial |
|  [05]   | `new_inbox()` / `set_server_pool(servers)`                         | instance | a fresh reply subject; the dial roster       |

`Client.connect(servers, error_cb, disconnected_cb, closed_cb, discovered_server_cb, reconnected_cb, name, pedantic, verbose, allow_reconnect, connect_timeout, reconnect_time_wait, max_reconnect_attempts, ping_interval, max_outstanding_pings, dont_randomize, flusher_queue_size, no_echo, tls, tls_hostname, tls_handshake_first, user, password, token, drain_timeout, signature_cb, user_jwt_cb, user_credentials, nkeys_seed, nkeys_seed_str, inbox_prefix, pending_size, flush_timeout, ws_connection_headers, reconnect_to_server_handler, lame_duck_mode_cb)` spells that roster whole.

Every lifecycle callback must be a coroutine function — a plain callable raises `InvalidCallbackTypeError` at connect.

[ENTRYPOINT_SCOPE]: core messaging

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :-------------------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `publish(subject, payload=b'', reply='', headers=None)`                     | instance | fire onto a subject                         |
|  [02]   | `subscribe(subject, queue='', cb=None, future=None, max_msgs=0, …)`         | instance | callback or iterator subscription           |
|  [03]   | `request(subject, payload=b'', timeout=0.5, old_style=False, headers=None)` | instance | one reply over the shared inbox             |
|  [04]   | `Subscription.messages`                                                     | property | async iterator, callback-free only          |
|  [05]   | `Subscription.next_msg(timeout=1.0)`                                        | instance | one pull; raises `nats.errors.TimeoutError` |
|  [06]   | `Subscription.unsubscribe(limit=0)` / `Subscription.drain()`                | instance | immediate or auto-unsubscribe, orderly shed |
|  [07]   | `Msg.respond(data)`                                                         | instance | reply on `msg.reply`                        |
|  [08]   | `Msg.ack()` / `ack_sync()` / `nak()` / `in_progress()` / `term()`           | instance | the JetStream settlement verbs              |

`subscribe` bounds each subscription with `pending_msgs_limit=524288` and `pending_bytes_limit=134217728`; `ack_sync(timeout=1.0)` and `nak(delay=None)` carry the settlement defaults.

[ENTRYPOINT_SCOPE]: JetStream

| [INDEX] | [SURFACE]                                                                             | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------------ | :------- | :------------------------------------ |
|  [01]   | `Client.jetstream(**opts)` / `Client.jsm(**opts)`                                     | instance | the context and the manager           |
|  [02]   | `JetStreamContext.publish(subject, payload=b'', timeout=None, …)`                     | instance | answers a `PubAck`                    |
|  [03]   | `publish_async(...)` / `publish_async_completed()` / `publish_async_pending()`        | instance | the pipelined publish window          |
|  [04]   | `subscribe(subject, queue=None, cb=None, durable=None, stream=None, …)`               | instance | a push consumer                       |
|  [05]   | `pull_subscribe(subject, durable=None, …)` / `pull_subscribe_bind(...)`               | instance | a pull consumer                       |
|  [06]   | `PullSubscription.fetch(batch=1, timeout=5, heartbeat=None)`                          | instance | one batch; raises `FetchTimeoutError` |
|  [07]   | `add_stream(...)` / `update_stream(...)` / `delete_stream(name)` / `stream_info(...)` | instance | stream administration                 |
|  [08]   | `add_consumer(stream, config=None, …)` / `delete_consumer(stream, consumer)`          | instance | consumer administration               |
|  [09]   | `get_msg(...)` / `get_last_msg(...)` / `delete_msg(...)` / `purge_stream(...)`        | instance | direct stream access                  |
|  [10]   | `create_key_value(...)` / `key_value(bucket)` / `create_object_store(...)`            | instance | the two bucket families               |

`JetStreamContext.publish` continues `stream=None`, `headers=None`, `msg_ttl=None`; `subscribe` continues `config=None`, `manual_ack=False`, `ordered_consumer=False`, `idle_heartbeat=None`, `flow_control=False`; `pull_subscribe` continues `stream=None`, `config=None`; `add_stream` and `create_key_value` each take `config=None` beside `**params`, `add_consumer` a `timeout=None` beside its own, and `delete_msg` takes `stream_name` and `seq`.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Client` is asyncio-locked: its reader, ping, and flusher legs are `asyncio.Task`s, its pending queues `asyncio.Queue`, and its pong and request slots `asyncio.Future`. It runs under the anyio asyncio backend alone and is not trio-compatible; its internal tasks are loop-level rather than children of the caller's group, so a cancel scope does not reach them and `drain()`/`close()` is the orderly shed.
- Headers ride the HPUB command with a `NATS/1.0` header line, and the capability is negotiated off the SERVER's own INFO advertisement — the client sets its header and no-responders options from `_server_info["headers"]` alone, so a server not advertising it carries no header surface and an unprefixed attribute lowering is unspellable there.
- Inbound status lines arrive as headers: a `503` on a request raises `NoRespondersError`, and the status keys resolve through `nats.js.api.Header.STATUS`.
- `publish` refuses past the SERVER-advertised `max_payload` with `MaxPayloadError`, past the pending buffer with `OutboundBufferLimitError`, and on an empty subject with `BadSubjectError` — the payload ceiling is read off the connection rather than a constant.
- `Subscription.messages` is available only on a callback-free subscription and raises otherwise; a callback subscription and an iterator subscription are two shapes over one constructor.
- Each subscription is backed by a bounded `asyncio.Queue`, and overflow reaches `error_cb` as a `SlowConsumerError` carrying the subject, reply, sid, and the subscription. Two ceilings shed there independently: a `pending_bytes_limit` above zero drops before the enqueue, a `QueueFull` on `put_nowait` drops at `pending_msgs_limit`. Both drop and continue — no raise reaches the reader and `stats` counts no shed.
- `error_cb` unbound resolves the package's own `_default_error_callback`, which writes one `logger.error` and returns. Every asynchronous shed the client owns — `SlowConsumerError`, `DrainTimeoutError`, `FlushTimeoutError`, `ServerNotInPoolError`, and each reader- and reconnect-leg raise — reaches that callback alone, so an unbound composition reduces permanent message loss to one stdlib log line.
- `Client` runs a RECONNECT schedule of its own beside any caller's: `allow_reconnect` defaults on, `max_reconnect_attempts` to sixty tries, and `reconnect_time_wait` to a two-second gap, so a dropped connection re-dials for roughly two minutes underneath whatever the caller wrapped the call in. `force_reconnect()` drives the same path deliberately.
- `drain()` bounds the whole subscription drain with `drain_timeout` and CANCELS the gather on expiry — the remaining pending queues are discarded, `error_cb` receives one `DrainTimeoutError`, and the connection then flushes PUBLISHES and closes. Inbound loss at teardown is therefore silent past that one callback, while the outbound half genuinely flushes.
- `publish` bounds the outbound side LOUDLY by contrast: `pending_size` caps the command buffer and an overflowing publish raises `OutboundBufferLimitError` at the caller rather than dropping.
- `Msg` is a MUTABLE dataclass while its `Metadata` and `SequencePair` are frozen; `metadata` parses lazily off the `$JS.ACK` reply subject and raises `NotJSMessageError` on a core message.
- `in_progress()` may be called repeatedly and does not mark the message acknowledged; `ack()` twice raises `MsgAlreadyAckdError`.
- `JetStreamContext` SUBCLASSES `JetStreamManager`, so the administration roster is reachable from the context and a separate `jsm()` handle is needed only where the context is not held.
- `stats` is a plain dict attribute rather than a property — `in_msgs`, `out_msgs`, `in_bytes`, `out_bytes`, `reconnects`, `errors_received`.
- `drain()` sheds subscriptions, then flushes publishes, then closes; it raises `ConnectionClosedError` or `ConnectionReconnectingError` by state rather than settling.
- `nats-py` declares ZERO required dependencies; the WebSocket transport, NKEYS authentication, and the accelerated header parse are each an optional extra whose absence disables that leg alone.
- `nats.aio.errors` keeps the v1 aliases (`ErrTimeout`, `ErrNoServers`, `NatsError`, and siblings), each subclassing its modern counterpart under a docstring naming the `nats.errors` replacement.

[STACKING]:
- `anyio`(`.api/anyio.md`): the client composes under the asyncio backend inside the caller's task group — the group owns the subscription pump and the `drain()`/`close()` teardown, and the forfeit is trio, which the descriptor row states rather than hides. No thread lane is needed, since every call is already a coroutine.
- `cloudevents`(`.api/cloudevents.md`): NATS carries no binding module in the distribution, so attribute lowering onto `ce-` headers is branch-owned; the subject is the routing key and header support gates the binary content mode.
- `opentelemetry-api`(`.api/opentelemetry-api.md`): the hop's W3C carrier rides the same header map the attributes do, so injection and extraction are one mapping crossing rather than two.

[LOCAL_ADMISSION]:
- Every lifecycle callback is a coroutine function, since a plain callable refuses at connect.
- Every composition BINDS `error_cb` onto the branch fault channel, because the package default converts a permanently lost message into one log line — a `SlowConsumerError` is the sole witness a delivery the server already made never reached a handler.
- Every subscription STATES `pending_msgs_limit` and `pending_bytes_limit` against the lane's own limiter, since an inherited ceiling sizes the shed threshold against nothing the branch measured.
- `reconnect_time_wait`, `max_reconnect_attempts`, and `allow_reconnect` state their values at the dial. `reliability/resilience#RESILIENCE` holds every schedule the branch runs and `RetryClass.BROKER` routes its re-offer through a RESTART, so an inherited reconnect curve underneath that route makes effective attempts the product of two schedules — the fork the branch RULINGS foreclose for binding rows and that inheritance re-opens beneath them.
- `drain_timeout` states its value against the lane's in-flight window, since expiry discards every queued delivery rather than extending the drain.
- Header support probes the live connection rather than a version literal, so a server without it refuses the binary content mode at admission instead of publishing a headerless message that decodes as nothing.
- Admission reads `max_payload` off the connection before any encode, so an oversized event routes to the reference-carrying leg rather than raising at publish.
- Trio's forfeit rides a declared row on the arm's own descriptor, never an undocumented assumption.
- Every raise crosses one `boundary` fence into `BoundaryFault`; `nats.js.errors.APIError` and its code-keyed leaves are caught at their own grain, and the deprecated `nats.aio.errors` aliases are refused.
