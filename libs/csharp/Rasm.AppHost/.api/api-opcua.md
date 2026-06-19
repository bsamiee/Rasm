# [RASM_APPHOST_API_OPCUA]

`OPCFoundation.NetStandard.Opc.Ua` is a meta-package that aggregates `Opc.Ua.Types`, `Opc.Ua.Core`, `Opc.Ua.Security.Certificates`, `Opc.Ua.Configuration`, `Opc.Ua.Client`, and `Opc.Ua.Server` into a single dependency; `OPCFoundation.NetStandard.Opc.Ua.PubSub` adds the publish-subscribe application, connection, dataset encoding, and transport surfaces for MQTT and UDP delivery.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OPCFoundation.NetStandard.Opc.Ua`
- package: `OPCFoundation.NetStandard.Opc.Ua`
- assembly: `Opc.Ua.Core` (primary); also `Opc.Ua.Client`, `Opc.Ua.Server`, `Opc.Ua.Configuration`, `Opc.Ua.Security.Certificates`, `Opc.Ua.Types`
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
- rail: opcua-core

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
|  [09]   | `ServerSecurityPolicy`         | record class  | one security policy (mode + URI)            |

[PUBLIC_TYPE_SCOPE]: session and client — `Opc.Ua`
- rail: opcua-core

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
- rail: opcua-client

The high-level managed session/subscription/monitored-item surface the live-wire `OpcUaLane` composes; it sits above `SessionClient` and owns the publish loop, keep-alive, and notification fan the low-level RPC client does not.

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
- rail: opcua-core

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
- rail: opcua-core

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
- rail: opcua-pubsub

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
- rail: opcua-core

Configuration loads pivot by source type; application type, system type, logger, telemetry, and trace policy stay call-specific parameters.

| [INDEX] | [SOURCE]          | [SURFACE]                                | [ENTRY_FAMILY] | [CAPABILITY]                      |
| :-----: | :---------------- | :--------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | section           | `Load(section)`                          | sync load      | loads app.config section          |
|  [02]   | section           | `LoadAsync(section)`                     | async load     | loads section with telemetry      |
|  [03]   | file              | `Load(file)`                             | sync load      | loads explicit file path          |
|  [04]   | file              | `LoadAsync(file)`                        | async load     | loads file with trace policy      |
|  [05]   | stream            | `Load(stream)`                           | sync load      | loads stream payload              |
|  [06]   | endpoint registry | `ConfiguredEndpointCollection.Load(...)` | static load    | loads persisted endpoint registry |

[CONFIG_LOAD_PARAMETERS]:
- Section forms carry section name plus application type and may add logger, telemetry, and cancellation.
- File and stream forms carry application type, system type, and optional trace application policy.
- Endpoint-registry loading carries the resolved `ApplicationConfiguration`, file path, and telemetry context.

[ENTRYPOINT_SCOPE]: session client lifecycle
- rail: opcua-core

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY] | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `SessionClient(ITransportChannel, ITelemetryContext)`                        | constructor    | creates client over existing channel  |
|  [02]   | `SessionClient.CreateSessionAsync(requestHeader, ..., ct)`                   | async call     | creates OPC UA session                |
|  [03]   | `SessionClient.ActivateSessionAsync(requestHeader, ..., ct)`                 | async call     | activates session with identity token |
|  [04]   | `SessionClient.CloseSessionAsync(requestHeader, delete, ct)`                 | async call     | closes session                        |
|  [05]   | `SessionClient.ReadAsync(requestHeader, maxAge, timestamps, nodes, ct)`      | async call     | reads node values                     |
|  [06]   | `SessionClient.WriteAsync(requestHeader, nodesToWrite, ct)`                  | async call     | writes node values                    |
|  [07]   | `SessionClient.BrowseAsync(requestHeader, view, maxRefs, nodesToBrowse, ct)` | async call     | browses address space                 |
|  [08]   | `SessionClient.CreateSubscriptionAsync(requestHeader, ..., ct)`              | async call     | creates subscription                  |

[ENTRYPOINT_SCOPE]: managed session, subscription, and monitored-item lifecycle
- rail: opcua-client

The managed client pivots by lifecycle stage; configuration, reverse-connect, endpoint, identity, locales, and timeout stay call-specific parameters to `Session.CreateAsync`. Subscription publishing-interval (`int PublishingInterval`) and keep-alive/lifetime counts (`uint`) set the publish cadence; the negotiated `double CurrentPublishingInterval` reads back the server-resolved interval (never a `TimeSpan`). Monitored items arm on a node id, attribute, sampling interval, and monitoring mode and raise `Notification`.

| [INDEX] | [SURFACE]                                                                                                                                                             | [ENTRY_FAMILY] | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `Session.CreateAsync(ApplicationConfiguration, ReverseConnectManager, ConfiguredEndpoint, bool, bool, string, uint, IUserIdentity, IList<string>, CancellationToken)` | static factory | mints + activates a managed session              |
|  [02]   | `Session.AddSubscription(Subscription)`                                                                                                                               | bool call      | attaches a subscription to the session           |
|  [03]   | `Subscription(ITelemetryContext)`                                                                                                                                     | constructor    | creates a subscription with telemetry context    |
|  [04]   | `Subscription.AddItem(MonitoredItem)`                                                                                                                                 | void call      | adds a monitored item before create              |
|  [05]   | `Subscription.CreateAsync(CancellationToken)`                                                                                                                         | async call     | arms the subscription on the server              |
|  [06]   | `Subscription.CurrentPublishingInterval`                                                                                                                              | `double` prop  | server-negotiated publishing interval            |
|  [07]   | `MonitoredItem(ITelemetryContext)`                                                                                                                                    | constructor    | creates a monitored item with telemetry context  |
|  [08]   | `MonitoredItem.Notification` (`event MonitoredItemNotificationEventHandler`)                                                                                          | event          | per-value notification fan                       |
|  [09]   | `MonitoredItem.DetachNotificationEventHandlers()`                                                                                                                     | void call      | detaches notification handlers on teardown       |
|  [10]   | `Session.ReadAsync(RequestHeader, double, TimestampsToReturn, ReadValueIdCollection, CancellationToken)`                                                              | async call     | inherited managed read                           |
|  [11]   | `Session.WriteAsync(RequestHeader, WriteValueCollection, CancellationToken)`                                                                                          | async call     | inherited managed write; `WriteResponse.Results` |

[MANAGED_VALUE_PROJECTION]:
- `MonitoredItemNotificationEventArgs.NotificationValue` casts to `MonitoredItemNotification` whose `DataValue Value` carries `object Value`, `StatusCode StatusCode`, and `DateTime SourceTimestamp`.
- `NodeId.Parse(string)` resolves a node-id string; `new DataValue(new Variant(double))` wraps an outbound scalar; `StatusCode.IsGood(code)` grades quality; `Attributes.Value` selects the value attribute.

[ENTRYPOINT_SCOPE]: certificate PKI operations
- rail: opcua-core

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------ | :-------------- | :--------------------------------------- |
|  [01]   | `CertificateFactory.CreateCertificate(subjectName)`                       | builder factory | starts `ICertificateBuilder` chain       |
|  [02]   | `CertificateFactory.CreateCertificate(appUri, appName, subject, domains)` | builder factory | creates application certificate          |
|  [03]   | `CertificateFactory.RevokeCertificate(issuer, crls, revoked)`             | static call     | produces updated CRL                     |
|  [04]   | `CertificateFactory.CreateSigningRequest(certificate, domainNames)`       | static call     | generates CSR bytes                      |
|  [05]   | `CertificateValidator.Validate(certificate)`                              | validation call | validates against configured trust lists |
|  [06]   | `CertificateStoreIdentifier.CreateStore(storeTypeName, telemetry)`        | static factory  | opens a certificate store by type name   |

[ENTRYPOINT_SCOPE]: PubSub application lifecycle
- rail: opcua-pubsub

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------ | :-------------- | :--------------------------------------- |
|  [01]   | `UaPubSubApplication.Create(ITelemetryContext)`                     | static factory  | creates application with no data store   |
|  [02]   | `UaPubSubApplication.Create(IUaPubSubDataStore, ITelemetryContext)` | static factory  | creates application with data store      |
|  [03]   | `UaPubSubApplication.Create(configFilePath, telemetry, dataStore?)` | static factory  | creates application from config file     |
|  [04]   | `UaPubSubApplication.SupportedTransportProfiles`                    | static property | returns supported transport profile URIs |
|  [05]   | `UaPubSubApplication.Start()`                                       | lifecycle call  | starts all configured connections        |
|  [06]   | `UaPubSubApplication.Stop()`                                        | lifecycle call  | stops all connections                    |
|  [07]   | `UaPubSubApplication.Dispose()`                                     | lifetime call   | releases connections and data store      |

## [04]-[IMPLEMENTATION_LAW]

[OPCUA_TOPOLOGY]:
- meta-package: `OPCFoundation.NetStandard.Opc.Ua` aggregates `Opc.Ua.Types`, `Opc.Ua.Core`, `Opc.Ua.Security.Certificates`, `Opc.Ua.Configuration`, `Opc.Ua.Client`, `Opc.Ua.Server` — no library DLL of its own
- core namespace: `Opc.Ua` contains 1000+ types; configuration, certificate, channel, and address-space types coexist in this namespace
- supported transports (PubSub): UDP-UADP, MQTT-JSON, MQTT-UADP — declared in `UaPubSubApplication.SupportedTransportProfiles`
- certificate store types: directory (PEM/DER), Windows X.509, and `CertificateIdentifierCollectionStore`
- session call pattern: async variants (`*Async`) accept `CancellationToken`; sync and `IAsyncResult` variants coexist

[LOCAL_ADMISSION]:
- Application configuration loads from file, stream, or .NET configuration section; `ApplicationConfiguration.LoadAsync` is preferred for telemetry
- Session construction requires a transport channel; `ClientChannelManager` owns channel lifecycle
- Certificate validation uses `CertificateValidator` initialized from `SecurityConfiguration`; trust lists persist as directory stores
- PubSub entry point is `UaPubSubApplication.Create(...)` — one application per process; connections are registered through configuration

[MANAGED_CLIENT_LAW]:
- The `Opc.Ua.Client` managed cluster (`Session`/`Subscription`/`MonitoredItem`) is the live-wire `OpcUaLane` owner — it sits above `SessionClient` and owns the publish loop, keep-alive, and notification fan; the low-level `SessionClient`/`ClientBase` RPC surface is the inherited read/write base, never the direct subscription owner.
- The notification fan crosses on the `MonitoredItem.Notification` event thread; the host projects each `DataValue` into the suite carrier and hands it to a bounded channel, never running the interior on the OPC-UA publish thread.
- Subscription publishing interval is an `int` policy column the row sets; `CurrentPublishingInterval` is a `double` read-back of the server-negotiated interval and is never cast to a `TimeSpan`.

[RAIL_LAW]:
- Package: `OPCFoundation.NetStandard.Opc.Ua` + `OPCFoundation.NetStandard.Opc.Ua.PubSub`
- Owns: OPC UA application lifecycle, low-level session client, managed `Opc.Ua.Client` session/subscription/monitored-item, certificate PKI, publish-subscribe transport
- Accept: configuration-loaded endpoints, certificate-store-backed PKI, async managed session/subscription operations, event-driven monitored-item notification
- Reject: hand-rolled binary encoding, inline security policy strings, direct low-level channel construction outside `ApplicationConfiguration`, a hand-rolled subscription/publish loop beside the managed `Subscription`
