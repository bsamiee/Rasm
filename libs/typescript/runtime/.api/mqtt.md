# [TS_RUNTIME_API_MQTT]

`mqtt` owns the live MQTT.js client realizing core's MQTT v5 carrier dialect. Runtime scopes connections, subscriptions, packet properties, acknowledgements, and per-client QoS stores; core owns carrier injection and extraction.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the live client, operation options, delivered packets, lifecycle events, and scoped stores

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]   | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `MqttClient` (`Client`)                         | live client     | typed event emitter; resource, never plain value   |
|  [02]   | `IClientOptions`                                | connect frame   | protocol v5, will, keepalive, packet properties    |
|  [03]   | `IClientPublishOptions`                         | publish frame   | QoS, retain, duplicate, and PUBLISH properties     |
|  [04]   | `IClientSubscribeOptions`                       | subscribe frame | QoS and MQTT v5 subscription properties            |
|  [05]   | `IClientUnsubscribeProperties`                  | unsubscribe     | MQTT v5 UNSUBSCRIBE properties                     |
|  [06]   | `IPublishPacket`                                | delivered frame | topic, payload, QoS, retain, and packet properties |
|  [07]   | `ISubscriptionMap` / `ISubscriptionGrant`       | topic map       | requested options and granted QoS/refusal          |
|  [08]   | `MqttClientEventCallbacks`                      | event map       | connect, message, disconnect, error, and close     |
|  [09]   | `MqttProtocol` / `BaseMqttProtocol`             | scheme brand    | MQTT URL transport selector                        |
|  [10]   | `ErrorWithReasonCode` / `ErrorWithSubackPacket` | typed fault     | reason code or SUBACK refusal packet               |
|  [11]   | `IStore` / `Store`                              | QoS store       | per-client incoming/outgoing packet store          |
|  [12]   | `TimerVariant`                                  | clock policy    | automatic, worker, or native keepalive timer       |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: scoped acquisition, packet verbs, event consumption, acknowledgement, and release

| [INDEX] | [SURFACE]                                                  | [SHAPE]           | [CAPABILITY]                               |
| :-----: | :--------------------------------------------------------- | :---------------- | :----------------------------------------- |
|  [01]   | `connect(brokerUrl \| opts): MqttClient`                   | factory           | live client selected by URL scheme         |
|  [02]   | `connectAsync`                                             | factory           | resolve after CONNACK                      |
|  [03]   | `client.publishAsync(topic, message, opts?)`               | instance          | publish payload and v5 properties          |
|  [04]   | `client.subscribeAsync`                                    | instance          | grants with `128` refusal                  |
|  [05]   | `client.unsubscribeAsync(topic, opts?)`                    | instance          | unsubscribe with v5 properties             |
|  [06]   | `client.on(event, cb)`                                     | instance          | typed packet and lifecycle events          |
|  [07]   | `client.handleMessage` + `IClientOptions.customHandleAcks` | instance + option | application-controlled QoS acknowledgement |
|  [08]   | `client.handleAuth` + `IClientOptions.authPacket`          | instance + option | MQTT v5 enhanced authentication            |
|  [09]   | `client.endAsync(force?, opts?)`                           | instance          | scoped release and DISCONNECT properties   |
|  [10]   | `client.reconnect(opts?)`                                  | instance          | reconnect with the client's stores         |
|  [11]   | `ReasonCodes`                                              | static            | MQTT v5 reason-code vocabulary             |

- `IClientOptions` seats both override hooks: `customHandleAcks` and `authPacket` ride the connect options record, while `handleMessage` and `handleAuth` are the client members they drive.
- `IClientOptions` also seats the CONNECT credential (`username`, `password`), the session pair (`clean`, `properties.sessionExpiryInterval`), the inbound flow-control window (`properties.receiveMaximum`), and the re-dial posture (`reconnectPeriod`, `reconnectOnConnackError`, `connectTimeout`, `resubscribe`).
- `customHandleAcks` is honored at `protocolVersion: 5` alone; every lower version replaces it with an immediate acknowledgement.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `PublishQoS` derives as `NonNullable<IClientPublishOptions["qos"]>` from the public publish-options frame.
- `UserProperties` derives as `NonNullable<NonNullable<IPublishPacket["properties"]>["userProperties"]>` from delivered packets.
- Carrier injection writes `opts.properties.userProperties`; extraction reads the delivered `IPublishPacket` property block.
- A client is scoped state containing sockets, timers, subscriptions, packet identifiers, and incoming/outgoing stores.
- Each subscription owns one message listener and removes it on scope release; no application-global event pump exists.
- Promise verbs lift through `Effect.tryPromise`; callback-only acknowledgement and authentication paths lift through `Effect.async`.

[STACKING]:
- `core/interchange/carrier`: injects and extracts the typed trace carrier; runtime only maps it onto MQTT v5 properties.
- `cloudevents`: owns the MQTT CloudEvent envelope; this package transports the encoded packet.
- `effect`: `Effect.acquireRelease` owns connection lifetime, and `Stream.asyncScoped` owns subscription events.
- `value/fault`: reason-code and SUBACK faults fold into the runtime fault vocabulary at the boundary.

[LOCAL_ADMISSION]:
- Acquire with `connectAsync` and release with `endAsync`; set `protocolVersion: 5` where the carrier dialect is required.
- Gate inbound acknowledgement on the handler through `handleMessage`: the package emits `message` and then calls that member, whose callback releases the PUBACK at QoS 1 and the PUBCOMP at QoS 2. Event listeners alone read frames the broker already counts acknowledged, which is the named at-most-once defect wearing a QoS number.
- Pair a withheld acknowledgement with `clean: false` and a `sessionExpiryInterval`: a refused message re-offers on SESSION RESUME and never inside the live connection, and a clean session discards it outright.
- Declare `properties.receiveMaximum` — unset, the broker decides how many QoS>0 publications ride unacknowledged toward this client.
- Bound the release: `end(false)` parks on `outgoingEmpty` with NO deadline, so an unresponsive broker holds a closing scope forever; run the graceful arm under a window and fall through to `end(true)`. Neither arm drains the offline queue holding QoS-0 publishes and control packets.
- Every dial rebuilds the CONNECT packet from `client.options`, so that record is the credential rotation path; `reconnect()` replaces both packet stores and discards queued QoS>0 state, so a rotation supervisor trades freshness for publish loss.
- Keep every incoming/outgoing `Store` private to its client; QoS 1 and 2 delivery state never crosses clients.
- Treat subscription grant `128` as refusal and map it through the typed fault channel.
