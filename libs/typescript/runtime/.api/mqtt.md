# [TS_RUNTIME_API_MQTT]

`mqtt` owns the live MQTT.js client realizing core's MQTT v5 carrier dialect. Runtime scopes connections, subscriptions, packet properties, acknowledgements, and per-client QoS stores; core owns carrier injection and extraction.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `mqtt`
- package: `mqtt` (MIT)
- module: one barrel; browser and React Native exports select WebSocket bundles
- runtime: node/bun TCP, TLS, and WebSocket; browser WebSocket only
- rail: runtime/net/channel

## [02]-[PUBLIC_TYPES]

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

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: scoped acquisition, packet verbs, event consumption, acknowledgement, and release

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `connect(brokerUrl \| opts): MqttClient`     | factory  | live client selected by URL scheme         |
|  [02]   | `connectAsync`                               | factory  | resolve after CONNACK                      |
|  [03]   | `client.publishAsync(topic, message, opts?)` | instance | publish payload and v5 properties          |
|  [04]   | `client.subscribeAsync`                      | instance | grants with `128` refusal                  |
|  [05]   | `client.unsubscribeAsync(topic, opts?)`      | instance | unsubscribe with v5 properties             |
|  [06]   | `client.on(event, cb)`                       | instance | typed packet and lifecycle events          |
|  [07]   | `client.handleMessage` + `IClientOptions.customHandleAcks` | instance + option | application-controlled QoS acknowledgement — the flag seats on the connect options record, never the client instance |
|  [08]   | `client.handleAuth` + `IClientOptions.authPacket`          | instance + option | MQTT v5 enhanced authentication — the packet seats on the connect options record, never the client instance          |
|  [09]   | `client.endAsync(force?, opts?)`             | instance | scoped release and DISCONNECT properties   |
|  [10]   | `client.reconnect(opts?)`                    | instance | reconnect with the client's stores         |
|  [11]   | `ReasonCodes`                                | static   | MQTT v5 reason-code vocabulary             |

## [04]-[IMPLEMENTATION_LAW]

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
- Keep every incoming/outgoing `Store` private to its client; QoS 1 and 2 delivery state never crosses clients.
- Treat subscription grant `128` as refusal and map it through the typed fault rail.

[RAIL_LAW]:
- Owns: live client resources, MQTT verbs, packet and option frames, lifecycle events, acknowledgements, and per-client stores.
- Accept: scoped acquisition, carrier-driven user properties, typed packet reads, scoped streams, and typed reason-code folding.
- Reject: invalid root imports, global clients/listeners/stores, unscoped connections, raw carrier reads, and swallowed grant refusal.
