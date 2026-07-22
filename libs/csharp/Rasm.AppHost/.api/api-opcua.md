# [RASM_APPHOST_API_OPCUA]

`OPCFoundation.NetStandard.Opc.Ua` owns the AppHost OPC UA client stack: managed `Session`/`Subscription`/`MonitoredItem` streaming above the low-level RPC `SessionClient`, application-configuration loading, and certificate-store PKI. `OPCFoundation.NetStandard.Opc.Ua.PubSub` owns the publish-subscribe dataset transport over MQTT and UDP.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OPCFoundation.NetStandard.Opc.Ua`
- package: `OPCFoundation.NetStandard.Opc.Ua`
- assembly: `Opc.Ua.Core` primary, aggregating `Opc.Ua.Client`, `Opc.Ua.Server`, `Opc.Ua.Configuration`, `Opc.Ua.Security.Certificates`, `Opc.Ua.Types` — no DLL of its own
- namespace: `Opc.Ua`, `Opc.Ua.Bindings`, `Opc.Ua.Configuration`, `Opc.Ua.Security`, `Opc.Ua.Security.Certificates`
- asset: runtime library
- rail: opcua-core

[PACKAGE_SURFACE]: `OPCFoundation.NetStandard.Opc.Ua.PubSub`
- package: `OPCFoundation.NetStandard.Opc.Ua.PubSub`
- assembly: `Opc.Ua.PubSub`
- namespace: `Opc.Ua.PubSub`, `Opc.Ua.PubSub.Configuration`, `Opc.Ua.PubSub.Encoding`, `Opc.Ua.PubSub.PublishedData`, `Opc.Ua.PubSub.Transport`
- asset: runtime library
- rail: opcua-pubsub

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: application and configuration — `Opc.Ua.Configuration`

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :----------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `ApplicationConfiguration`     | configuration | root configuration for UA applications      |
|  [02]   | `SecurityConfiguration`        | configuration | PKI trust lists, cert stores, policies      |
|  [03]   | `ServerConfiguration`          | configuration | server endpoint and session configuration   |
|  [04]   | `ClientConfiguration`          | configuration | client session and connection configuration |
|  [05]   | `TransportQuotas`              | configuration | message size, timeout, and channel limits   |
|  [06]   | `TraceConfiguration`           | configuration | diagnostic trace settings                   |
|  [07]   | `ConfiguredEndpointCollection` | collection    | persisted endpoint registry                 |
|  [08]   | `ConfiguredEndpoint`           | record class  | one persisted server endpoint               |
|  [09]   | `ServerSecurityPolicy`         | record class  | one security policy, mode plus URI          |

[PUBLIC_TYPE_SCOPE]: session and client — `Opc.Ua`

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `SessionClient`      | class         | full OPC UA session client, extends `ClientBase` |
|  [02]   | `ClientBase`         | class         | channel-backed RPC client base                   |
|  [03]   | `DiscoveryClient`    | class         | discovery endpoint client                        |
|  [04]   | `ISessionClient`     | interface     | session operation contract                       |
|  [05]   | `IUserIdentity`      | interface     | user identity token contract                     |
|  [06]   | `UserIdentity`       | class         | anonymous, username, certificate identity        |
|  [07]   | `ReverseConnectHost` | class         | reverse-connect listener                         |
|  [08]   | `SessionChannel`     | class         | session-scoped transport channel                 |

[PUBLIC_TYPE_SCOPE]: managed client — `Opc.Ua.Client`

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :----------------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `Session`                            | class         | managed session over `SessionClient`; publish + keep-alive  |
|  [02]   | `ISession`                           | interface     | managed session contract                                    |
|  [03]   | `Subscription`                       | class         | one subscription; publishing interval, keep-alive, items    |
|  [04]   | `MonitoredItem`                      | class         | one monitored node; start node, attribute, sampling, mode   |
|  [05]   | `MonitoredItemNotificationEventArgs` | event args    | per-item notification carrying `NotificationValue`          |
|  [06]   | `MonitoredItemNotification`          | class         | notification body; `DataValue Value`                        |
|  [07]   | `ReverseConnectManager`              | class         | reverse-connect endpoint manager passed to `CreateAsync`    |
|  [08]   | `ITelemetryContext`                  | interface     | telemetry context threaded into managed client constructors |
|  [09]   | `MonitoringMode`                     | enum          | `Disabled` / `Sampling` / `Reporting`                       |

[PUBLIC_TYPE_SCOPE]: address-space and value primitives — `Opc.Ua`

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :---------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `NodeId`                | class         | address-space node identifier; `Parse(string)`             |
|  [02]   | `DataValue`             | class         | value + `StatusCode` + `SourceTimestamp`/`ServerTimestamp` |
|  [03]   | `Variant`               | struct        | wrapped UA scalar/array value; `new Variant(double)`       |
|  [04]   | `StatusCode`            | struct        | UA status; `StatusCode.IsGood(code)`                       |
|  [05]   | `Attributes`            | static class  | attribute id constants; `Attributes.Value`                 |
|  [06]   | `WriteValue`            | class         | one node write request (node id, attribute, value)         |
|  [07]   | `ReadValueId`           | class         | one node read request                                      |
|  [08]   | `WriteValueCollection`  | collection    | batch of `WriteValue` for `WriteAsync`                     |
|  [09]   | `ReadValueIdCollection` | collection    | batch of `ReadValueId` for `ReadAsync`                     |
|  [10]   | `StatusCodeCollection`  | collection    | `WriteResponse.Results` status batch                       |
|  [11]   | `TimestampsToReturn`    | enum          | `Source` / `Server` / `Both` / `Neither`                   |

[PUBLIC_TYPE_SCOPE]: certificate and PKI — `Opc.Ua.Security.Certificates`

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :--------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `CertificateFactory`         | static class  | creation, revocation, and signing-request ops   |
|  [02]   | `CertificateValidator`       | class         | chain validation and trust-list evaluation      |
|  [03]   | `CertificateIdentifier`      | class         | thumbprint/subject/URI certificate selector     |
|  [04]   | `CertificateStoreIdentifier` | class         | directory or Windows store path + type          |
|  [05]   | `CertificateTrustList`       | class         | trust list backed by a certificate store        |
|  [06]   | `ICertificateStore`          | interface     | certificate store open/find/add/remove contract |
|  [07]   | `ICertificateValidator`      | interface     | validation contract                             |
|  [08]   | `DirectoryCertificateStore`  | class         | PEM/DER file-system certificate store           |
|  [09]   | `X509CertificateStore`       | class         | Windows X.509 store adapter                     |

[PUBLIC_TYPE_SCOPE]: PubSub application — `Opc.Ua.PubSub`

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]  | [CAPABILITY]                              |
| :-----: | :------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `UaPubSubApplication`            | class          | PubSub application root; owns connections |
|  [02]   | `IUaPubSubConnection`            | interface      | one publish/subscribe connection contract |
|  [03]   | `IUaPublisher`                   | interface      | publisher contract                        |
|  [04]   | `IUaPubSubDataStore`             | interface      | dataset storage for published values      |
|  [05]   | `UaDataSetMessage`               | abstract class | base for UADP and JSON dataset messages   |
|  [06]   | `UaNetworkMessage`               | abstract class | base for UADP and JSON network messages   |
|  [07]   | `TransportProtocol`              | enum           | MQTT, UDP, AMQP transport selector        |
|  [08]   | `MessageMapping`                 | enum           | UADP or JSON encoding selector            |
|  [09]   | `DataSetFlags1EncodingMask`      | enum (byte)    | dataset message field-inclusion flags     |
|  [10]   | `UADPFlagsEncodingMask`          | enum (byte)    | UADP network message flags                |
|  [11]   | `EnumMqttProtocolVersion`        | enum           | MQTT 3.1 / 5.0 version selector           |
|  [12]   | `IntervalRunner`                 | class          | periodic publish interval runner          |
|  [13]   | `ConfigurationUpdatingEventArgs` | event args     | configuration update notification         |
|  [14]   | `SubscribedDataEventArgs`        | event args     | received dataset notification             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: application configuration construction

Configuration loads pivot by source; application type, system type, logger, telemetry, and trace policy stay call-specific parameters.

| [INDEX] | [SURFACE]                                     | [SHAPE] | [CAPABILITY]                     |
| :-----: | :-------------------------------------------- | :------ | :------------------------------- |
|  [01]   | `ApplicationConfiguration.LoadAsync(section)` | factory | loads an app.config section      |
|  [02]   | `ApplicationConfiguration.LoadAsync(file)`    | factory | loads an explicit file path      |
|  [03]   | `ApplicationConfiguration.LoadAsync(stream)`  | factory | loads a stream payload           |
|  [04]   | `ConfiguredEndpointCollection.Load(...)`      | factory | loads the persisted endpoint set |

[ENTRYPOINT_SCOPE]: session client lifecycle

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :---------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `SessionClient(ITransportChannel, ITelemetryContext)` | ctor     | creates a client over a channel      |
|  [02]   | `SessionClient.CreateSessionAsync(...)`               | instance | creates an OPC UA session            |
|  [03]   | `SessionClient.ActivateSessionAsync(...)`             | instance | activates a session with an identity |
|  [04]   | `SessionClient.CloseSessionAsync(...)`                | instance | closes a session                     |
|  [05]   | `SessionClient.ReadAsync(...)`                        | instance | reads node values                    |
|  [06]   | `SessionClient.WriteAsync(...)`                       | instance | writes node values                   |
|  [07]   | `SessionClient.BrowseAsync(...)`                      | instance | browses the address space            |
|  [08]   | `SessionClient.CreateSubscriptionAsync(...)`          | instance | creates a subscription               |

[ENTRYPOINT_SCOPE]: managed session, subscription, and monitored-item lifecycle

Managed lifecycle pivots by stage; configuration, reverse-connect, endpoint, identity, locales, and timeout stay call-specific parameters to `Session.CreateAsync`.

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------ | :------- | :----------------------------------------------- |
|  [01]   | `Session.CreateAsync(...)`                        | factory  | mints + activates a managed session              |
|  [02]   | `Session.AddSubscription(Subscription)`           | instance | attaches a subscription; returns `bool`          |
|  [03]   | `Subscription(ITelemetryContext)`                 | ctor     | creates a subscription with telemetry context    |
|  [04]   | `Subscription.AddItem(MonitoredItem)`             | instance | adds a monitored item before create              |
|  [05]   | `Subscription.CreateAsync(CancellationToken)`     | instance | arms the subscription on the server              |
|  [06]   | `Subscription.CurrentPublishingInterval`          | property | server-negotiated `double` publishing interval   |
|  [07]   | `MonitoredItem(ITelemetryContext)`                | ctor     | creates a monitored item with telemetry context  |
|  [08]   | `MonitoredItem.Notification`                      | event    | per-value notification fan                       |
|  [09]   | `MonitoredItem.DetachNotificationEventHandlers()` | instance | detaches notification handlers on teardown       |
|  [10]   | `Session.ReadAsync(...)`                          | instance | inherited managed read                           |
|  [11]   | `Session.WriteAsync(...)`                         | instance | inherited managed write; `WriteResponse.Results` |

- `MonitoredItem.Notification`: `MonitoredItemNotificationEventArgs.NotificationValue` casts to `MonitoredItemNotification`, whose `DataValue Value` carries the value, `StatusCode`, and `SourceTimestamp`.

[ENTRYPOINT_SCOPE]: managed arming and write-request members

Arming members set on the object initializer before `Subscription.AddItem` and `Subscription.CreateAsync`; publishing and sampling intervals are milliseconds.

| [INDEX] | [SURFACE]                         | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :-------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Subscription.PublishingInterval` | property | `int` publish cadence, ms          |
|  [02]   | `Subscription.KeepAliveCount`     | property | `uint` empty-publish threshold     |
|  [03]   | `Subscription.LifetimeCount`      | property | `uint` server-side expiry          |
|  [04]   | `MonitoredItem.StartNodeId`       | property | `NodeId` monitored node            |
|  [05]   | `MonitoredItem.AttributeId`       | property | `uint` monitored attribute         |
|  [06]   | `MonitoredItem.SamplingInterval`  | property | `int` sample cadence, ms           |
|  [07]   | `MonitoredItem.MonitoringMode`    | property | `MonitoringMode` notification mode |
|  [08]   | `WriteValue.NodeId`               | property | `NodeId` write node                |
|  [09]   | `WriteValue.AttributeId`          | property | `uint` write attribute             |
|  [10]   | `WriteValue.Value`                | property | `DataValue` write payload          |

[ENTRYPOINT_SCOPE]: certificate PKI operations

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `CertificateFactory.CreateCertificate(subjectName)`                       | factory  | starts an `ICertificateBuilder` chain    |
|  [02]   | `CertificateFactory.CreateCertificate(appUri, appName, subject, domains)` | factory  | creates an application certificate       |
|  [03]   | `CertificateFactory.RevokeCertificate(issuer, crls, revoked)`             | static   | produces an updated CRL                  |
|  [04]   | `CertificateFactory.CreateSigningRequest(certificate, domainNames)`       | static   | generates CSR bytes                      |
|  [05]   | `CertificateValidator.Validate(certificate)`                              | instance | validates against configured trust lists |
|  [06]   | `CertificateStoreIdentifier.CreateStore(storeTypeName, telemetry)`        | factory  | opens a certificate store by type name   |

[ENTRYPOINT_SCOPE]: PubSub application lifecycle

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------ | :------- | :---------------------------------------- |
|  [01]   | `UaPubSubApplication.Create(ITelemetryContext)`                     | factory  | creates an application with no data store |
|  [02]   | `UaPubSubApplication.Create(IUaPubSubDataStore, ITelemetryContext)` | factory  | creates an application with a data store  |
|  [03]   | `UaPubSubApplication.Create(configFilePath, telemetry, dataStore?)` | factory  | creates an application from a config file |
|  [04]   | `UaPubSubApplication.SupportedTransportProfiles`                    | property | returns supported transport profile URIs  |
|  [05]   | `UaPubSubApplication.Start()`                                       | instance | starts all configured connections         |
|  [06]   | `UaPubSubApplication.Stop()`                                        | instance | stops all connections                     |
|  [07]   | `UaPubSubApplication.Dispose()`                                     | instance | releases connections and data store       |
|  [08]   | `UaPubSubApplication.DataReceived`                                  | event    | received dataset fan                      |

- `UaPubSubApplication.DataReceived`: `EventHandler<SubscribedDataEventArgs>` carrying `NetworkMessage` and `Source`; `RawDataReceived` and `MetaDataReceived` are the raw and metadata siblings.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Opc.Ua` holds over 1000 types; configuration, certificate, channel, and address-space types coexist in this one namespace.
- Managed `Opc.Ua.Client` (`Session`/`Subscription`/`MonitoredItem`) sits above `SessionClient` and owns the publish loop, keep-alive, and notification fan; the `SessionClient`/`ClientBase` RPC surface is the inherited read/write base, never the direct subscription owner.
- `Subscription.PublishingInterval` is the `int` policy the row sets; `CurrentPublishingInterval` reads back the server-negotiated `double`, never cast to a `TimeSpan`.
- PubSub transports UDP-UADP, MQTT-JSON, and MQTT-UADP, declared in `UaPubSubApplication.SupportedTransportProfiles`.
- Certificate stores are directory (PEM/DER), Windows X.509, and `CertificateIdentifierCollectionStore`.
- Session and PKI operations use the `*Async` variants, each taking a `CancellationToken`.

[STACKING]:
- `Wire/livewire.md` `OpcUaLane`: composes the managed `Session`/`Subscription`/`MonitoredItem` surface; each `MonitoredItem.Notification` projects one `DataValue` into `ExternalValue` and writes it to one bounded `Channel<ExternalValue>`, never running the interior on the OPC UA publish thread; `PubSubLane` composes `UaPubSubApplication.DataReceived`, fanning each dataset field into the same lane.
- `api-mqtt`(`.api/api-mqtt.md`): the peer `MQTTnet` transport row; its `IMqttClient` fan and this surface's `UaPubSubApplication.DataReceived` fan drain into the one bounded `Channel<ExternalValue>` the live-wire studio owns.

[LOCAL_ADMISSION]:
- Application configuration loads through `ApplicationConfiguration.LoadAsync` from a file, stream, or .NET configuration section.
- Low-level session construction requires a transport channel; `ClientChannelManager` owns channel lifecycle, while managed `Session.CreateAsync` builds its own channel from the configuration-loaded endpoint.
- Certificate validation runs through `CertificateValidator` initialized from `SecurityConfiguration`; trust lists persist as directory stores.
- PubSub admits one `UaPubSubApplication` per process; connections register through configuration.

[RAIL_LAW]:
- Package: `OPCFoundation.NetStandard.Opc.Ua` + `OPCFoundation.NetStandard.Opc.Ua.PubSub`
- Owns: OPC UA application lifecycle, low-level session client, managed `Opc.Ua.Client` session/subscription/monitored-item, certificate PKI, publish-subscribe transport
- Accept: configuration-loaded endpoints, certificate-store-backed PKI, async managed session/subscription operations, event-driven monitored-item notification
- Reject: hand-rolled binary encoding, inline security policy strings, direct low-level channel construction outside `ApplicationConfiguration`, a hand-rolled subscription/publish loop beside the managed `Subscription`
