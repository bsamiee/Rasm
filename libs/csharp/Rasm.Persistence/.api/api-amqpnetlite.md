# [RASM_PERSISTENCE_API_AMQPNETLITE]

`AMQPNetLite.Core` owns the `AMQP 1.0` protocol itself: the container connection, its session multiplex, the sender and receiver links, the full framing model every performative and terminus lowers onto, the SASL negotiation, transaction coordination, and a broker-side listener. It carries the `AMQP 1.0` leg of the op-log changefeed egress — `CloudNative.CloudEvents.Amqp` (`api-cloudevents-amqp`) maps the envelope onto this package's `Amqp.Message`, and this package owns everything from that message to the wire. Its message model is disjoint from the `AMQP 0-9-1` `RabbitMQ.Client` surface (`api-rabbitmq`); the two protocols share no type.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AMQPNetLite.Core`
- package: `AMQPNetLite.Core` (Apache-2.0)
- assembly: `Amqp.Net` — package id and assembly name DIVERGE, so an assembly-name probe against the package id resolves nothing
- namespace: `Amqp` (connection, session, links, settings, exception), `Amqp.Framing` (performatives, terminus, outcomes), `Amqp.Types` (`Symbol`, `Map`, `Fields`, described types), `Amqp.Sasl` (mechanism profiles), `Amqp.Transactions` (coordinator and declared state), `Amqp.Handler` (protocol interception), `Amqp.Listener` (broker-side accept)
- asset: pure-managed, `netstandard2.0` only — no native payload and no RID burden
- rail: cdc-egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: connection topology — `AmqpObject` is the shared base carrying `Error` and the close ladder, so a fault on ANY node reaches the same two members

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]  | [CAPABILITY]                                       |
| :-----: | :------------------ | :------------- | :------------------------------------------------- |
|  [01]   | `AmqpObject`        | abstract class | `Error`, `Closed`, `AddClosedCallback`, close pair |
|  [02]   | `Connection`        | class          | container connection over an `Address`             |
|  [03]   | `Session`           | class          | channel multiplex on one connection                |
|  [04]   | `Link`              | abstract class | name, handle, role, link state, detach             |
|  [05]   | `SenderLink`        | class          | outbound link — send, cancel, no credit member     |
|  [06]   | `ReceiverLink`      | class          | inbound link — receive and the credit surface      |
|  [07]   | `Address`           | class          | broker endpoint with scheme and credentials        |
|  [08]   | `ConnectionFactory` | class          | configured connection mint                         |
|  [09]   | `AmqpSettings`      | class          | frame size, container id, session and link caps    |
|  [10]   | `CreditMode`        | enum           | receiver-side credit replenishment policy          |
|  [11]   | `AmqpException`     | sealed class   | fault carrying its `Error`                         |

[PUBLIC_TYPE_SCOPE]: message and disposition — `Message` exposes its sections as public FIELDS, so a null-section read is the caller's guard and no property setter validates one

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]  | [CAPABILITY]                                     |
| :-----: | :----------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `Message`                            | class          | seven section fields, `Body`, `DeliveryTag`      |
|  [02]   | `Amqp.Framing.Header`                | sealed class   | durability, priority, ttl, delivery counters     |
|  [03]   | `Amqp.Framing.Properties`            | sealed class   | standard properties including `ContentType`      |
|  [04]   | `Amqp.Framing.ApplicationProperties` | sealed class   | the map a header-filtering broker routes on      |
|  [05]   | `Amqp.Framing.Source`                | sealed class   | receiving terminus with filter and outcome set   |
|  [06]   | `Amqp.Framing.Target`                | sealed class   | sending terminus with address and expiry policy  |
|  [07]   | `Amqp.Framing.Outcome`               | abstract class | `Accepted`, `Released`, `Rejected`, `Modified`   |
|  [08]   | `Amqp.Framing.Error`                 | sealed class   | condition, description, info carried on a fault  |
|  [09]   | `Amqp.Framing.Begin`                 | sealed class   | session performative — LOCAL receive window only |
|  [10]   | `Amqp.Framing.Flow`                  | sealed class   | credit performative the PEER sends               |
|  [11]   | `Amqp.Types.Symbol`                  | class          | AMQP symbol key                                  |
|  [12]   | `Amqp.Types.Map` / `Fields`          | class          | described map value spaces                       |
|  [13]   | `OutcomeCallback`                    | delegate       | `(ILink, Message, Outcome, object)` async ack    |
|  [14]   | `MessageCallback`                    | delegate       | receiver-side per-message dispatch               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `SenderLink` — the complete send surface; every send lowers through one internal path, and the awaited pair is the only form whose return states an outcome

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :---------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `new SenderLink(Session, string, string)`             | ctor     | link over a plain target address          |
|  [02]   | `new SenderLink(Session, string, Target, OnAttached)` | ctor     | link over a configured terminus           |
|  [03]   | `new SenderLink(Session, string, Attach, OnAttached)` | ctor     | link over a whole attach performative     |
|  [04]   | `Send(Message)`                                       | instance | BLOCKS on the ack under a fixed timeout   |
|  [05]   | `Send(Message, TimeSpan)`                             | instance | blocks; cancels then throws on expiry     |
|  [06]   | `Send(Message, OutcomeCallback, object)`              | instance | queues and returns; null callback un-acks |
|  [07]   | `Send(Message, DeliveryState, OutcomeCallback, obj)`  | instance | queues under a state; not transactional   |
|  [08]   | `SendAsync(Message)`                                  | instance | awaited ack under the fixed timeout       |
|  [09]   | `SendAsync(Message, TimeSpan)`                        | instance | awaited ack under a caller timeout        |
|  [10]   | `Cancel(Message)`                                     | instance | drops a queued delivery                   |

[ENTRYPOINT_SCOPE]: `ReceiverLink` — the credit surface, which has NO sender-side counterpart

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :---------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `Start(int, MessageCallback?)`                  | instance | grants credit and begins push dispatch |
|  [02]   | `SetCredit(int, bool autoRestore = true)`       | instance | grants credit with auto-replenish      |
|  [03]   | `SetCredit(int, CreditMode, int flowThreshold)` | instance | grants credit under an explicit mode   |
|  [04]   | `Receive()` / `Receive(TimeSpan)`               | instance | pull one message, blocking             |
|  [05]   | `ReceiveAsync()` / `ReceiveAsync(TimeSpan)`     | instance | pull one message, awaited              |
|  [06]   | `Accept` / `Release` / `Reject` / `Modify`      | instance | terminal dispositions on a delivery    |
|  [07]   | `Complete(Message, DeliveryState)`              | instance | disposition under an explicit state    |

[ENTRYPOINT_SCOPE]: lifecycle, fault, and settings — every node inherits the `AmqpObject` half, so one subscription shape covers connection, session, and link alike

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :--------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `new Connection(Address)`                            | ctor     | connects and opens the container               |
|  [02]   | `new Connection(Address, SaslProfile, Open, cb)`     | ctor     | connects under SASL and a tuned `Open`         |
|  [03]   | `new Session(Connection)`                            | ctor     | begins a session; NO factory method            |
|  [04]   | `new Session(Connection, Begin, OnBegin)`            | ctor     | begins under a supplied performative           |
|  [05]   | `AmqpObject.Closed` / `AddClosedCallback(cb)`        | event    | out-of-band fault on ANY node                  |
|  [06]   | `AmqpObject.Error` / `IsClosed`                      | property | last fault and terminal state                  |
|  [07]   | `Close()` / `Close(TimeSpan, Error?)`                | instance | synchronous close with an optional fault       |
|  [08]   | `CloseAsync()` / `CloseAsync(TimeSpan, Error?)`      | instance | awaited close                                  |
|  [09]   | `Link.Detach(Error?)` / `DetachAsync(Error?)`        | instance | link-scoped teardown                           |
|  [10]   | `Connection.Factory` / `DisableServerCertValidation` | static   | shared factory; a public FIELD, not a property |

- `AmqpSettings` carries exactly `MaxFrameSize` `ContainerId` `HostName` `MaxSessionsPerConnection` `MaxLinksPerSession` `IdleTimeout` — no member bounds credit or in-flight deliveries.
- `Session` publishes only `Connection`, `SessionState`, and its two constructors; `Connection` publishes no `CreateSession`, so a session is constructed, never requested.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one `Connection` owns many `Session`s and one `Session` owns many `Link`s, each constructed against its parent rather than minted by it, and every one of the three derives `AmqpObject`, so `Error`, `IsClosed`, `Closed`, and the close pair are the single fault-and-teardown vocabulary across the whole tree.
- `Message` holds `Header`, `DeliveryAnnotations`, `MessageAnnotations`, `Properties`, `ApplicationProperties`, `BodySection`, and `Footer` as public fields; `Body` and `DeliveryTag` are get-only projections, and `Encode`/`Decode` cross to and from a `ByteBuffer`.
- SENDER-SIDE CREDIT DOES NOT EXIST. `SenderLink` publishes no credit member, `Link` publishes none, and `AmqpSettings` bounds none; the link's internal credit is assigned only while handling a peer `Flow`, so the PEER decides how much a sender may have in flight. `ReceiverLink.SetCredit`/`Start` are the credit surface and they govern the inbound direction alone — that asymmetry is the protocol's, not an omission.
- async sends run UNBOUNDED by construction: with credit absent or a write in progress, a send appends to an internal outgoing list carrying no cap, no rejection, and no block, so a peer withholding credit grows managed memory a caller can neither read nor limit. Only the blocking and awaited forms bound anything, and they bound it at one delivery per awaiting caller.
- `Begin.IncomingWindow`/`OutgoingWindow` set the LOCAL receive window; the session's own outgoing window is overwritten from the peer's `Begin` and `Flow`, so writing it locally states nothing the peer honors. Session windows are private fields with no accessor.
- `Open` carries frame-size and channel ceilings only, so no connection-level knob caps outbound deliveries either.

[STACKING]:
- `CloudNative.CloudEvents.Amqp`(`.api/api-cloudevents-amqp.md`): `ce.ToAmqpMessageWithUnderscorePrefix(ContentMode.Binary, formatter)` mints the exact `Amqp.Message` a send takes and `message.ToCloudEvent(formatter, extensions)` inverts it, so envelope attributes ride `cloudEvents_` application properties and this package never reads the body.
- `System.Threading.Channels`(`libs/csharp/.api/api-bcl-channels.md`): a caller wanting overlapped sends supplies the in-flight bound this package refuses to — a bounded channel under `BoundedChannelFullMode.Wait` offers deliveries and settles their awaited acks in offer order, which is the only shape that pipelines without inheriting the unbounded outgoing list.
- `RabbitMQ.Client`(`.api/api-rabbitmq.md`): the `AMQP 0-9-1` peer sink; protocols are disjoint, so no message, channel, or property type crosses between the two and a shared delivery leg is structurally impossible.
- `Version/egress#EGRESS_SINK`: its `EgressSink.Amqp` row composes the awaited send inside its own bounded window and reads `AmqpObject.Closed` into the family's `Watch` cell, so a link the peer detached reports even when a send already returned.

[LOCAL_ADMISSION]:
- egress sends through `SendAsync(Message, TimeSpan)` alone; its awaited return IS the settlement, and an `AmqpException` over a `Released` or `Rejected` outcome is the refusal.
- callback send forms refuse on a durable rail because their queue has no ceiling and their null-callback variant acknowledges nothing.
- in-flight breadth is the composing fence's declared row value realized over a bounded channel, never a client setting, because this client publishes none.
- connection, session, and link each register one closed-callback subscription at composition, folding `Error` into the delivery leg's out-of-band cell.

[RAIL_LAW]:
- Package: `AMQPNetLite.Core`
- Owns: the `AMQP 1.0` protocol — container connection, session multiplex, sender and receiver links, the framing and type model, SASL, transactions, and the broker-side listener
- Accept: awaited `SendAsync` with a caller timeout, receiver credit through `SetCredit`/`Start`, `AmqpObject.Closed` and `Error` as the out-of-band fault surface, and terminus configuration through `Source`/`Target`
- Reject: the callback send forms on a durable rail, any claim of a sender-side credit or in-flight setting, a locally-written outgoing session window read as a bound, hand-built `cloudEvents_` application properties over a raw message, and conflation with the `AMQP 0-9-1` `RabbitMQ.Client` surface
