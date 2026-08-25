# [PY_BRANCH_API_PAHO_MQTT]

`paho-mqtt` is the Eclipse MQTT client covering `MQTTv31`, `MQTTv311`, and `MQTTv5` over TCP, WebSockets, and Unix sockets. It carries no coroutine surface at all: the client is a synchronous state machine driven either by its own daemon thread (`loop_start`) or, socket-first, by a foreign event loop through `socket()`/`want_write()` and the `loop_read`/`loop_write`/`loop_misc` triple paired with the `on_socket_*` registration callbacks. `MQTTv5` User Properties ride `Properties.UserProperty` as an append-on-repeat list of UTF-8 string pairs, which is the unprefixed carrier the CloudEvents MQTT binding lowers attributes onto. Reason codes, subscribe options, packet types, and the property table are each their own module, so the wire vocabulary is data rather than integers at a call site.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `paho-mqtt`
- package: `paho-mqtt` (EPL-2.0 / EDL-1.0)
- module: `paho.mqtt`
- namespaces: `paho.mqtt.{client,enums,properties,reasoncodes,packettypes,subscribeoptions,matcher,publish,subscribe}`
- target: pure-Python wheel, no native asset; `py.typed` with inline annotations and no stub files
- rail: broker-transport

## [02]-[PUBLIC_TYPES]

[CLIENT_SCOPE]: `paho.mqtt.client`

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `Client`                   | class         | the whole protocol state machine; one instance, one connection   |
|  [02]   | `MQTTMessage`              | class         | `__slots__` record: payload, qos, retain, dup, mid, `properties` |
|  [03]   | `MQTTMessageInfo`          | class         | publish handle: `mid`, `rc`, `wait_for_publish`, `is_published`  |
|  [04]   | `ConnectFlags`             | `NamedTuple`  | `session_present: bool` — the VERSION2 connect flag carrier      |
|  [05]   | `DisconnectFlags`          | `NamedTuple`  | `is_disconnect_packet_from_server: bool`                         |
|  [06]   | `WebsocketConnectionError` | exception     | `ConnectionError` subclass for the WebSocket handshake           |
|  [07]   | `PayloadType`              | type alias    | `str \| bytes \| bytearray \| int \| float \| None`              |
|  [08]   | `CleanStartOption`         | type alias    | `bool \| Literal[3]`; `MQTT_CLEAN_START_FIRST_ONLY` is that `3`  |
|  [09]   | `WebSocketHeaders`         | type alias    | a header dict or a callable rewriting one                        |

[ENUM_SCOPE]: `paho.mqtt.enums` — every wire vocabulary as a closed member set

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [MEMBERS]                                                                                |
| :-----: | :-------------------- | :------------ | :--------------------------------------------------------------------------------------- |
|  [01]   | `CallbackAPIVersion`  | `Enum`        | `VERSION1` (deprecated, slated for removal), `VERSION2`                                  |
|  [02]   | `MQTTProtocolVersion` | `IntEnum`     | `MQTTv31=3`, `MQTTv311=4`, `MQTTv5=5`                                                    |
|  [03]   | `MQTTErrorCode`       | `IntEnum`     | every member prefixes `MQTT_ERR_`: `AGAIN=-1` … `KEEPALIVE=16`, `QUEUE_SIZE=15` shedding |
|  [04]   | `ConnackCode`         | `IntEnum`     | `CONNACK_ACCEPTED=0` and five refusals to `CONNACK_REFUSED_NOT_AUTHORIZED=5`             |
|  [05]   | `MessageState`        | `IntEnum`     | ten states `MQTT_MS_INVALID=0` … `MQTT_MS_QUEUED=9`, the last pre-send                   |
|  [06]   | `MessageType`         | `IntEnum`     | the wire nibbles `CONNECT=0x10` … `AUTH=0xF0`                                            |
|  [07]   | `LogLevel`            | `IntEnum`     | prefixed likewise: `MQTT_LOG_INFO=1` … `MQTT_LOG_DEBUG=16`                               |
|  [08]   | `PahoClientMode`      | `IntEnum`     | `MQTT_CLIENT=0`, `MQTT_BRIDGE=1`                                                         |

[PROTOCOL_SCOPE]: the wire-vocabulary modules

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :-------------------------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `paho.mqtt.properties.Properties`             | class         | the MQTT 5.0 property bag; `__setattr__` validates per packet type |
|  [02]   | `paho.mqtt.properties.VariableByteIntegers`   | class         | `encode(x)` / `decode(buffer) -> (value, used)`                    |
|  [03]   | `paho.mqtt.properties.MalformedPacket`        | exception     | `MQTTException` subclass raised by the wire readers                |
|  [04]   | `paho.mqtt.reasoncodes.ReasonCode`            | class         | `@total_ordering`; `.value`, `.is_failure`, `getName()`, `pack()`  |
|  [05]   | `paho.mqtt.packettypes.PacketTypes`           | class         | `CONNECT=1` … `AUTH=15`, plus the `WILLMESSAGE=99` pseudo type     |
|  [06]   | `paho.mqtt.subscribeoptions.SubscribeOptions` | class         | `QoS`, `noLocal`, `retainAsPublished`, `retainHandling`            |
|  [07]   | `paho.mqtt.matcher.MQTTMatcher`               | class         | topic-filter trie: `__setitem__`/`__getitem__`/`iter_match`        |
|  [08]   | `paho.mqtt.MQTTException`                     | exception     | the distribution root                                              |

[CALLBACK_SCOPE]: the VERSION2 signatures, protocol-independent by construction

| [INDEX] | [CALLBACK]                                     | [SIGNATURE]                                                                      |
| :-----: | :--------------------------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `on_connect`                                   | `(client, userdata, connect_flags: ConnectFlags, reason_code, properties)`       |
|  [02]   | `on_connect_fail`                              | `(client, userdata)`                                                             |
|  [03]   | `on_disconnect`                                | `(client, userdata, disconnect_flags: DisconnectFlags, reason_code, properties)` |
|  [04]   | `on_message`                                   | `(client, userdata, message: MQTTMessage)`                                       |
|  [05]   | `on_publish`                                   | `(client, userdata, mid, reason_code, properties)`                               |
|  [06]   | `on_subscribe`                                 | `(client, userdata, mid, reason_code_list, properties)`                          |
|  [07]   | `on_unsubscribe`                               | `(client, userdata, mid, reason_code_list, properties)`                          |
|  [08]   | `on_log`                                       | `(client, userdata, level: int, buf: str)`                                       |
|  [09]   | `on_pre_connect`                               | `(client, userdata)`                                                             |
|  [10]   | `on_socket_open` `_close`                      | `(client, userdata, sock)`                                                       |
|  [11]   | `on_socket_register_write` `_unregister_write` | `(client, userdata, sock)`                                                       |

Each callback also has a decorator factory on the instance — `connect_callback()`, `message_callback()`, and the eleven siblings — and `message_callback_add(sub, callback)` installs a per-filter handler that bypasses `on_message` entirely.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction

| [INDEX] | [SURFACE]                                                                          | [SHAPE] | [CAPABILITY]     |
| :-----: | :--------------------------------------------------------------------------------- | :------ | :--------------- |
|  [01]   | `Client(callback_api_version, client_id="", clean_session=None, userdata=None, …)` | ctor    | the whole client |

`Client` continues `protocol=MQTTv311`, `transport="tcp"`, `reconnect_on_failure=True`, `manual_ack=False`.

[ENTRYPOINT_SCOPE]: connection and session

| [INDEX] | [SURFACE]                                                                              | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `connect(host, port=1883, keepalive=60, bind_address="", bind_port=0, …)`              | instance | dial then reconnect              |
|  [02]   | `connect_async(...)` / `connect_srv(domain=None, ...)`                                 | instance | deferred dial, SRV discovery     |
|  [03]   | `reconnect()`                                                                          | instance | re-dial the last parameters      |
|  [04]   | `disconnect(reasoncode=None, properties=None)`                                         | instance | clean DISCONNECT                 |
|  [05]   | `is_connected()`                                                                       | instance | live CONNACK state               |
|  [06]   | `username_pw_set(username, password=None)` / `user_data_set(userdata)`                 | instance | credentials, opaque caller state |
|  [07]   | `tls_set(...)` / `tls_set_context(context=None)` / `tls_insecure_set(value)`           | instance | TLS material                     |
|  [08]   | `will_set(topic, payload=None, qos=0, retain=False, properties=None)` / `will_clear()` | instance | last-will registration           |
|  [09]   | `ws_set_options(path="/mqtt", headers=None)` / `proxy_set(**proxy_args)`               | instance | WebSocket path, proxy hop        |
|  [10]   | `reconnect_delay_set(min_delay=1, max_delay=120)`                                      | instance | exponential backoff bounds       |
|  [11]   | `max_inflight_messages_set(inflight)` / `max_queued_messages_set(queue_size)`          | instance | in-flight and queue bounds       |

`connect` continues `clean_start=MQTT_CLEAN_START_FIRST_ONLY`, `properties=None`.

[ENTRYPOINT_SCOPE]: publish and subscribe

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `publish(topic, payload=None, qos=0, retain=False, properties=None)` | instance | returns `MQTTMessageInfo`, never blocks        |
|  [02]   | `subscribe(topic, qos=0, options=None, properties=None)`             | instance | one filter, one pair, or a list of either      |
|  [03]   | `unsubscribe(topic, properties=None)`                                | instance | one filter or a list                           |
|  [04]   | `ack(mid, qos)` / `manual_ack_set(on)`                               | instance | deferred PUBACK/PUBCOMP under manual ack       |
|  [05]   | `MQTTMessageInfo.wait_for_publish(timeout=None)` / `.is_published()` | instance | blocking confirmation; `None` blocks unbounded |
|  [06]   | `topic_matches_sub(sub, topic)`                                      | static   | filter-match predicate                         |

`subscribe` admits one filter, a `(filter, qos)` pair, a `(filter, SubscribeOptions)` pair, or a list of either.

[ENTRYPOINT_SCOPE]: the two loop shapes

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :---------------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `loop_start()` / `loop_stop()`                              | instance | spawn and join the daemon network thread        |
|  [02]   | `loop_forever(timeout=1.0, retry_first_connection=False)`   | instance | run the machine on the calling thread           |
|  [03]   | `loop(timeout=1.0)`                                         | instance | one bounded select-and-service pass             |
|  [04]   | `socket()` / `want_write()`                                 | instance | the live socket and its pending-output verdict  |
|  [05]   | `loop_read(max_packets=1)` / `loop_write()` / `loop_misc()` | instance | the foreign-loop triple: read, flush, keepalive |
|  [06]   | `enable_logger(logger=None)` / `disable_logger()`           | instance | route the client's own log line                 |

[ENTRYPOINT_SCOPE]: MQTT 5.0 properties

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------ | :------- | :--------------------------------------------------- |
|  [01]   | `Properties(packetType)`                          | ctor     | a bag scoped to one `PacketTypes` member             |
|  [02]   | `props.UserProperty = (name, value)`              | instance | APPENDS a `(str, str)` pair; the slot is a list      |
|  [03]   | `props.UserProperty`                              | instance | `list[tuple[str, str]]`; absent when never set       |
|  [04]   | `props.isEmpty()` / `props.clear()`               | instance | any-property probe, whole reset                      |
|  [05]   | `props.json()`                                    | instance | a dict of set properties, hex-encoding binary values |
|  [06]   | `props.pack()` / `props.unpack(buffer)`           | instance | wire round trip                                      |
|  [07]   | `getIdentFromName(name)` / `getNameFromIdent(id)` | instance | the identifier/name bridge                           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- There is no coroutine surface — the package contains no `async def`, no `await`, and no `asyncio` import anywhere. Two integration shapes exist and only the second keeps a cancel scope honest: `loop_start()` runs the machine on a daemon thread named `paho-mqtt-client-<id>` and fires every callback there, while `socket()` with `loop_read`/`loop_write`/`loop_misc` under the `on_socket_open`/`on_socket_close`/`on_socket_register_write`/`on_socket_unregister_write` registration pair drives the same machine from a foreign loop with no thread at all. `loop_write` calls the register/unregister pair inside its own `finally`, so the write-readiness state machine is driven for the caller.
- Callback raises PROPAGATE out of the network loop unless `client.suppress_exceptions` is set, so a re-entry failure kills `loop_forever` and arms the reconnect path rather than being swallowed.
- `loop_read` ignores its `max_packets` argument — the body overwrites it with the live in-flight count, floored at one.
- `loop_misc` answers `MQTT_ERR_CONN_LOST` and fires `on_disconnect` when no PINGRESP arrives within `keepalive` seconds, so keepalive liveness is that call's alone.
- `publish` never blocks and never raises on queue overflow — it answers an `MQTTMessageInfo` whose `rc` is `MQTT_ERR_QUEUE_SIZE`, and `wait_for_publish`/`is_published` on that handle raise `ValueError` rather than reporting the shed. It raises `ValueError` for an empty topic under `MQTTv311`, an out-of-range QoS, and a payload past 268435455 bytes.
- Two distinct causes wear that one `rc`: the outbound queue standing at its ceiling, and a message-id collision against a live in-flight entry. Neither is recoverable from the code alone.
- Outbound queue depth defaults to ZERO meaning UNBOUNDED, so an unset ceiling buffers a stalled broker's whole backlog in process memory rather than shedding, and `max_queued_messages` refuses on an established connection — the value binds before `connect` or never. In-flight width defaults to twenty messages.
- `reconnect_on_failure` defaults on and re-dials from inside CONNACK handling, not just from the network loop: an `MQTTv311` session refused for protocol version silently DOWNGRADES itself to `MQTTv31` and reconnects, and one refused for identifier with an empty client id MINTS a random client id and reconnects. Both reach the socket-first shape, both change a session property a subscriber's durability depends on, and neither raises.
- `MQTTMessageInfo.wait_for_publish` waits on a `threading.Condition`, waking every `timeout/10` seconds and blocking unbounded when `timeout is None`.
- `MQTTMessage.topic` decodes its bytes as UTF-8 on every read, so a non-UTF-8 topic raises on property access.
- `Client.__init__` refuses a `str` first argument with the v1-to-v2 migration message, refuses `clean_session` under MQTTv5, and refuses an empty `client_id` with `clean_session=False`. `VERSION1` emits a `DeprecationWarning` at construction.
- `reinitialise(client_id, clean_session, userdata)` is BROKEN in this release: it re-enters `__init__` positionally, binding `client_id` onto `callback_api_version`. It is never composed.
- `on_publish` under VERSION2 for QoS 0 SYNTHESIZES its reason code and properties — MQTT 5.0 carries no PUBLISH-level reason code, so the value is fabricated at PUBACK time.
- `on_unsubscribe` under VERSION1 for MQTTv5 takes `(client, userdata, mid, properties, reason_codes)` — properties BEFORE reason codes, the reverse of every sibling — which is one of the reasons VERSION2 is the only admitted shape.
- `Properties.UserProperty` and `SubscriptionIdentifier` are the two identifiers `allowsMultiple` admits: assignment APPENDS rather than replaces, the stored value is always a `list[tuple[str, str]]`, and duplicate keys are legal and order-preserved. Assigning a single tuple normalizes to a one-element list. Unpacking a non-multiple property twice raises `MQTTException`.
- User Property applies to every packet type except PINGREQ and PINGRESP, and to the `WILLMESSAGE` pseudo type. Setting a property a packet type does not admit raises `MQTTException` at assignment.
- `MQTTMessage.properties` is populated only under MQTTv5 and is `None` otherwise, so an `MQTTv311` session carries no property surface at all and an unprefixed-attribute binding is unspellable there.
- `Properties.__setattr__` enforces the specification's value ranges — `ReceiveMaximum`/`TopicAlias` 1–65535, `TopicAliasMaximum` 0–65535, `MaximumPacketSize`/`SubscriptionIdentifier` 1–268435455, the three flag properties 0 or 1 — raising `MQTTException` rather than truncating.
- `SubscribeOptions` refuses an invalid `qos` or `retainHandling` by an explicit `raise AssertionError(...)` inside an `if`, not an `assert` statement, so `python -O` leaves the refusal armed and the constructor IS the gate.
- Deprecations are TWO, each carrying a `DeprecationWarning`: `CallbackAPIVersion.VERSION1` and the plural `ReasonCodes` alias whose metaclass makes `isinstance` true for any `ReasonCode`.

[STACKING]:
- `anyio`(`.api/anyio.md`): the socket-first shape is the admitted one — `socket()` registers on the caller's own readiness primitive and `loop_read`/`loop_write`/`loop_misc` run as bounded steps inside the caller's task group, so cancellation reaches the machine at a checkpoint rather than orphaning a daemon thread. Where the thread shape is unavoidable, `loop_start` runs under a `CapacityLimiter`-bounded `to_thread` lane and every callback re-enters through one `BlockingPortalProvider`.
- `cloudevents`(`.api/cloudevents.md`): MQTT is the UNPREFIXED binding — the distribution ships no MQTT binding module, so attribute lowering onto `Properties.UserProperty` pairs is branch-owned and the three `CE_PREFIX` families do not apply.

[LOCAL_ADMISSION]:
- `CallbackAPIVersion.VERSION2` is the one admitted shape, so every callback has one protocol-independent arity and MQTTv3 return codes arrive already lifted to `ReasonCode`.
- `suppress_exceptions` stays false and the crossing rails its own faults, so a re-entry failure is a typed fault rather than a killed loop.
- `MQTTv5` is the admitted protocol for the binary content mode; an `MQTTv311` session carries no property surface and lowers structured-only.
- `reinitialise` is refused; a re-armed composition constructs a fresh client.
- Every client STATES `max_queued_messages` and `max_inflight_messages_set` before connecting, since the shipped queue ceiling is unbounded and a bound landing after connect refuses.
- Every publish READS the answered `MQTTMessageInfo.rc`, because the shed is that value alone and both its causes — a full queue and a message-id collision — return normally.
- `reconnect_on_failure` and `reconnect_delay_set` state their values at construction. `reliability/resilience#RESILIENCE` holds every schedule the branch runs and `RetryClass.BROKER` routes its re-offer through a RESTART, so an inherited reconnect curve underneath that route makes effective attempts the product of two schedules.
- `MQTTv5` carries the admitted content mode, so its CONNACK path reaches neither silent respelling; a composition falling back to `MQTTv311` binds a non-empty client id and refuses `reconnect_on_failure`, since a downgraded protocol version and a minted session identity each change what a durable subscription resumes.

[RAIL_LAW]:
- Package: `paho-mqtt`
- Owns: the MQTT protocol state machine, its 5.0 property vocabulary, reason codes, subscribe options, and topic-filter matching
- Accept: `Client` under `CallbackAPIVersion.VERSION2`, the socket-first loop triple, `Properties` scoped to a `PacketTypes` member, `ReasonCode`, `SubscribeOptions`
- Reject: an unbounded outbound queue; a publish whose `MQTTMessageInfo.rc` nothing reads; an inherited reconnect curve beside the `RetryClass` owner; an empty client id on an `MQTTv311` session; `reinitialise`; `CallbackAPIVersion.VERSION1`; the plural `ReasonCodes` alias; a bare integer where a `ReasonCode` or `PacketTypes` member states the value; `suppress_exceptions = True`
