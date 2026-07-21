# [TS_CORE_API_MQTT]

`mqtt` is the MQTT.js client — where `interchange/carrier` speaks the MQTT v5 dialect of the closed per-transport propagation table, mapping the W3C tracing value onto `properties.userProperties` rather than a header list. Carrier owns the typed `{TraceContext, Baggage}` value and its parse/print folds; this package owns the live client, the connect/publish/subscribe surface carrying `userProperties`, and the pub/sub event streams. Dialect split mirrors `cloudevents` (`.api/cloudevents.md`): core owns the `userProperties` shape the carrier folds print into and read back off, while the runtime folder composes the live client. Boundary internalizes resource, emitter, and callback hazards: `connect()` returns a live `MqttClient` holding sockets, keepalive timers, and QoS stores, so it composes only through `Effect.acquireRelease` closing on `endAsync()`; each subscription lifts its `message` frames to an owned `Stream`/`Queue`; node-callback verbs cross through `Effect.tryPromise` or `Effect.async`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `mqtt`
- package: `mqtt`
- license: `MIT`
- effect-peer: none — every `MqttClient` construction, publish, and event frame crosses into `effect` at the `interchange/carrier` seam; `connectAsync`/`publishAsync` are consumed under `Effect.tryPromise` and the `EventEmitter` frames lift to `Stream` (`.api/effect.md`); no bundled peer
- catalog-verdict: KEEP — the MQTT-dialect arm of the `interchange/carrier` propagation table: `cloudevents` owns the CloudEvents envelope and its own MQTT binary binding (`.api/cloudevents.md`), `@connectrpc/connect` owns the Connect metadata dialect (`.api/connectrpc-connect.md`), this owns the live MQTT v5 broker client and its `userProperties` transport map — one carrier value, many dialect clients, no overlap
- runtime: mixed — `connect` resolves a transport by URL scheme across `mqtt`/`mqtts`/`ws`/`wss`/`tcp`/`ssl` and the WeChat/Alipay lanes; the node/bun lane carries the TCP/TLS builders and node stream deps, the browser lane ships the WebSocket-only ESM/min bundles (`dist/mqtt.esm.js`, `dist/mqtt.min.js`); broker connection is a runtime-lane surface, the dialect shape is isomorphic
- transitive deps: `mqtt-packet` (the v5 packet codec owning `UserProperties` and every property block), `readable-stream`, `ws`, `socks`, `lru-cache`, `worker-timers` (background-throttle-immune browser keepalive), `commist`/`help-me` (the bundled CLI only)
- modules / subpaths: single barrel — `main` `build/index.js` (CJS), `types` `build/index.d.ts`; conditional `exports` route `react-native`/`browser` to the ESM/min bundles, `default` to the node build; the client, shared, and validation surfaces re-export flat from the barrel, no deep subpath imports

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the live client, its per-operation option frames, and the MQTT v5 property map
- rail: interchange/carrier
- `MqttClient` is the stateful broker client over a `TypedEventEmitter` of the typed event map. `IClientOptions`/`IClientPublishOptions`/`IClientSubscribeOptions` each carry a `properties` slot whose `userProperties` is the carrier's dialect target. `UserProperties` (from `mqtt-packet`) is the `{[key]: string | string[]}` map the carrier folds print into; `QoS` is the `0 | 1 | 2` delivery grade.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                                |
| :-----: | :---------------------------------------------- | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `MqttClient` (`Client`)                         | live client     | `TypedEventEmitter<MqttClientEventCallbacks>`; resource not value  |
|  [02]   | `IClientOptions`                                | connect frame   | `protocolVersion` 5, CONNECT `userProperties`, `will`, `keepalive` |
|  [03]   | `IClientPublishOptions`                         | publish frame   | `qos`/`retain`/`dup`; PUBLISH `userProperties`, `contentType`      |
|  [04]   | `IClientSubscribeOptions`                       | subscribe frame | `qos`/`nl`/`rap`/`rh`; SUBSCRIBE `userProperties`                  |
|  [05]   | `IClientUnsubscribeProperties`                  | unsub frame     | UNSUBSCRIBE `userProperties`                                       |
|  [06]   | `ISubscriptionMap` / `ISubscriptionGrant`       | topic map       | `{[topic]: IClientSubscribeOptions}`; `qos: QoS \| 128` refuses    |
|  [07]   | `MqttClientEventCallbacks`                      | event map       | typed `connect`/`message`/`disconnect`/`error`/`close`/`offline`   |
|  [08]   | `UserProperties` (`mqtt-packet`)                | v5 prop map     | `{[key]: string \| string[]}` — carrier `userProperties` target    |
|  [09]   | `QoS` (`mqtt-packet`)                           | delivery grade  | `0 \| 1 \| 2`; QoS 1/2 route through the per-client `IStore`       |
|  [10]   | `MqttProtocol` / `BaseMqttProtocol`             | scheme brand    | `mqtt`/`mqtts`/`ws`/`wss`/`tcp`/`ssl` (+`+unix`) selector          |
|  [11]   | `ErrorWithReasonCode` / `ErrorWithSubackPacket` | typed fault     | `code: number` / `packet: ISubackPacket` → `FaultClass`            |
|  [12]   | `IStore` / `Store`                              | QoS store       | per-client `put`/`get`/`del`/`createStream`; never global          |
|  [13]   | `TimerVariant` (`auto`/`worker`/`native`)       | keepalive clock | `worker` = `worker-timers` throttle-immune browser keepalive       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client acquisition, publish/subscribe with `userProperties`, event-frame consumption, and release
- rail: interchange/carrier
- `connectAsync` acquires a connected client; `publishAsync` sets the carrier-printed `userProperties`; `subscribeAsync` yields `ISubscriptionGrant[]`; `client.on('message', ...)` frames lift to a `Stream`; `endAsync` releases the resource. Callback overloads lift through `Effect.async`.

```ts signature
declare function connectAsync(brokerUrl: string, opts?: IClientOptions): Promise<MqttClient>;
type OnMessageCallback = (topic: string, payload: Buffer, packet: IPublishPacket) => void;
interface MqttClient {
  subscribeAsync(
    topicObject: string | string[] | ISubscriptionMap,
    opts?: IClientSubscribeOptions | IClientSubscribeProperties,
  ): Promise<ISubscriptionGrant[]>;
  publishAsync(topic: string, message: string | Buffer, opts?: IClientPublishOptions): Promise<Packet | undefined>;
  endAsync(force?: boolean, opts?: Partial<IDisconnectPacket>): Promise<void>;
  on(event: "message", callback: OnMessageCallback): this;
}
interface IClientPublishOptions {
  qos?: QoS;
  retain?: boolean;
  dup?: boolean;
  properties?: IPublishPacket["properties"]; // userProperties?: UserProperties rides here — the v5 carrier slot
  cbStorePut?: StorePutCallback;
}
```

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY]  | [CONSUMER_BOUNDARY]                                        |
| :-----: | :----------------------------------------------- | :-------------- | :--------------------------------------------------------- |
|  [01]   | `connect(brokerUrl \| opts): MqttClient`         | acquire         | live client; `protocolVersion: 5` selects the v5 surface   |
|  [02]   | `connectAsync`                                   | acquire         | first CONNACK; scoped acquire arm                          |
|  [03]   | `client.publishAsync(topic, message, opts?)`     | publish         | carrier-printed `userProperties` on `opts.properties`      |
|  [04]   | `client.subscribeAsync`                          | subscribe       | `Promise<ISubscriptionGrant[]>`; `qos: 128` grant refuses  |
|  [05]   | `client.unsubscribeAsync(topic, opts?)`          | unsubscribe     | `userProperties` on the UNSUBSCRIBE frame                  |
|  [06]   | `client.on(event, cb)` (`TypedEventEmitter`)     | subscribe frame | `message`/`connect`/`disconnect`/`error`/`close`/`offline` |
|  [07]   | `client.handleMessage(...)` / `customHandleAcks` | manual ack      | QoS 1/2 app-controlled ack with a v5 reason code           |
|  [08]   | `client.handleAuth(...)` / `authPacket`          | v5 auth         | enhanced-authentication AUTH (`authenticationMethod`)      |
|  [09]   | `client.endAsync(force?, opts?)`                 | release         | `opts` DISCONNECT carries `userProperties`; release arm    |
|  [10]   | `client.reconnect(opts?)`                        | resume          | manual reconnect reusing `incomingStore`/`outgoingStore`   |
|  [11]   | `ReasonCodes`                                    | v5 code map     | `{ [code: number]: string }` — the v5 reason-code map      |
|  [12]   | `client.connected` / `reconnecting`              | state flags     | lifecycle read; the `Stream` scope reads these on close    |

## [04]-[IMPLEMENTATION_LAW]

[CARRIER_TOPOLOGY]:
- `userProperties` is the v5 dialect target, never a header list: MQTT v5 carries application key/value pairs as `UserProperties` (`{[key]: string | string[]}`) on the `properties` block of every CONNECT/PUBLISH/SUBSCRIBE/UNSUBSCRIBE/DISCONNECT packet. `interchange/carrier` owns the typed `{TraceContext, Baggage}` value and its total parse/print folds; it prints the folded strings into `opts.properties.userProperties` on publish and reads them back off the delivered `IPublishPacket['properties'].userProperties` through the same folds — a raw `userProperties` entry never reaches domain code untyped.
- client is a resource, not a value: `connect`/`connectAsync` return a live `MqttClient` holding sockets, keepalive timers, and per-client QoS stores. Boundary composes it through `Effect.acquireRelease(Effect.tryPromise(() => connectAsync(url, opts)), (c) => Effect.promise(() => c.endAsync()))`; each app owns its connection, subscription set, and message-id space.
- events lift per subscription, never a shared listener: `MqttClient` extends a `TypedEventEmitter<MqttClientEventCallbacks>`. Each `subscribe` call scopes its `message` frames into an owned `Stream`/`Queue` — the `error`/`close`/`offline` events terminate that scope. Two apps never register competing listeners on one emitter; the `Stream` is the owned surface, the `on` handler its internal pump.
- node-callback surface crosses at the seam: every verb ships a callback overload and an `*Async` overload. Boundary consumes `connectAsync`/`publishAsync`/`subscribeAsync`/`unsubscribeAsync`/`endAsync` under `Effect.tryPromise`, mapping the rejection onto `FaultClass`; the emitter frames and any callback-only path (`customHandleAcks`, `handleAuth`) lift through `Effect.async`. A raw callback into a fold is the defect.

[INTEGRATION_LAW]:
- Stack with `interchange/carrier` (`core/.planning/interchange/carrier.md`): the carrier owns the closed per-transport dialect table; this package IS the MQTT row's realization — `carrier` folds `{TraceContext, Baggage}` to strings and sets them on `opts.properties.userProperties`, and reads them back off the delivered packet's `properties.userProperties`. Connect ASCII/`-bin` metadata is `@connectrpc/connect`'s row, CloudEvents `ce-` headers are `cloudevents`'s row (`.api/cloudevents.md`) — one carrier value, many dialect clients.
- Stack with `cloudevents` (`core/.api/cloudevents.md`): `cloudevents` ships its OWN `MQTT.binary`/`MQTT.toEvent` binding mapping envelope extensions onto an MQTT PUBLISH — the CloudEvents-over-MQTT ENVELOPE codec, not a broker client. This package is the live client the envelope rides: a CloudEvents-over-MQTT lane serializes through the `cloudevents` MQTT binding, then `publishAsync`es the frame through this client. Binding never opens a connection; client never mints an envelope.
- Stack with `effect` `Stream` (`.api/effect.md`): the `message` event frames lift to a `Stream<IPublishPacket>` scoped to the subscription — `Stream.asyncScoped` registers the `on('message')` pump and tears it down on scope close, so backpressure and interruption are owned, never a leaked listener.
- Stack with `effect` `Schema` (`.api/effect.md`): a delivered `IPublishPacket['payload']` is `string | Buffer` and its `properties.userProperties` is untyped; `Schema.decodeUnknown` decodes the payload (by the C#-mint codec) and the carrier folds decode the tracing attributes once into owned vocabulary. A raw packet never leaks past the seam.
- Stack with `effect` `Match`/`Data` (`.api/effect.md`): the `MqttProtocol` scheme, the `QoS` grade, and the `ReasonCodes` verdict dispatch through `Match.exhaustive` — a transport scheme or reason code is a table row, never an `if`/`switch` ladder over broker-URL strings.
- Stack with `value/fault` (`core/.planning/value/fault.md`): `ErrorWithReasonCode.code` and `ErrorWithSubackPacket.packet` project onto a `FaultClass` diagnostic row at the `Effect.tryPromise` boundary through the `ReasonCodes` map; the carrier surfaces a typed connect/publish fault, never a bare `Error`.
- Stack with `value/identity` (`core/.planning/value/identity.md`): the `rasm.tenant` baggage promotion rides one `userProperties` key branded by the identity projection, so tenant context survives every broker hop the MQTT client crosses — the cost-attribution boundary the carrier law preserves.

[LOCAL_ADMISSION]:
- acquire through `Effect.acquireRelease(Effect.tryPromise(() => connectAsync(url, opts)), (c) => Effect.promise(() => c.endAsync()))` with `protocolVersion: 5`; each client stays inside its scope.
- set tracing/baggage on `opts.properties.userProperties` from the carrier's print fold; never read a raw `userProperties` entry off a delivered packet into domain logic — the branded value is the only typed form.
- scope each subscription's `message` frames into a `Stream.asyncScoped` registering `on('message')` and tearing down on close; never a bare `client.on('message', ...)` handler mutating shared state.
- consume `connectAsync`/`publishAsync`/`subscribeAsync`/`endAsync` under `Effect.tryPromise`; lift `customHandleAcks`/`handleAuth`/emitter frames through `Effect.async`; map `ErrorWithReasonCode`/`ErrorWithSubackPacket` onto `FaultClass` through the `ReasonCodes` map.
- select QoS 1/2 for at-least/exactly-once delivery routed through the per-client `Store`; the default in-memory store is per-client and app-neutral, and a shared store across clients is banned.
- dispatch the `MqttProtocol` scheme and `ReasonCodes` verdict as `Match` rows; a `qos: 128` grant is a subscription refusal the boundary lifts onto `FaultClass`, never a silent success.

[RAIL_LAW]:
- Package: `mqtt`
- Owns: the live `MqttClient` (`connect`/`connectAsync`, `publish`/`publishAsync`, `subscribe`/`subscribeAsync`, `unsubscribe`/`unsubscribeAsync`, `end`/`endAsync`, `reconnect`), the `IClientOptions`/`IClientPublishOptions`/`IClientSubscribeOptions` frames whose `properties.userProperties` realizes the carrier's MQTT dialect row, the typed `MqttClientEventCallbacks` emitter, the per-client `IStore`/`Store` QoS surface, the `MqttProtocol` scheme brand, the `ReasonCodes` v5 vocabulary, and the `ErrorWithReasonCode`/`ErrorWithSubackPacket` faults
- Accept: `Effect.acquireRelease` acquisition on `connectAsync`/`endAsync`, tracing/baggage printed onto `opts.properties.userProperties` from the carrier folds, `message` frames scoped into a `Stream.asyncScoped`, `*Async` overloads under `Effect.tryPromise` and callback paths under `Effect.async`, delivered payload and `userProperties` crossing `Schema.decodeUnknown` and the carrier folds, `ErrorWithReasonCode`/`ErrorWithSubackPacket` mapped onto `FaultClass`, `MqttProtocol`/`ReasonCodes` dispatched as `Match` rows
- Reject: a module-scoped or process-global shared client (app-neutrality collision), a bare `connect()` client outliving its scope (resource leak), a raw `client.on('message')` listener two apps register on, a raw `userProperties` entry read into domain code bypassing the carrier folds, a callback into a fold where the `*Async` overload exists, a shared QoS `Store` across clients, a `qos: 128` refusal swallowed as success, the bundled `commist`/`help-me` CLI surface
