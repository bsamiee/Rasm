# [RASM_APPHOST_API_OPCUA]

`OPCFoundation.NetStandard.Opc.Ua` is a meta-package that aggregates `Opc.Ua.Types`, `Opc.Ua.Core`, `Opc.Ua.Security.Certificates`, `Opc.Ua.Configuration`, `Opc.Ua.Client`, and `Opc.Ua.Server` into a single dependency; `OPCFoundation.NetStandard.Opc.Ua.PubSub` adds the publish-subscribe application, connection, dataset encoding, and transport surfaces for MQTT and UDP delivery.

## [1]-[PACKAGE_SURFACE]

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

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: application and configuration — `Opc.Ua.Configuration`
- rail: opcua-core

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :----------------------------- | :------------ | :------------------------------------------ |
|   [1]   | `ApplicationConfiguration`     | configuration | root configuration for UA applications      |
|   [2]   | `SecurityConfiguration`        | configuration | PKI trust lists, cert stores, policies      |
|   [3]   | `ServerConfiguration`          | configuration | server endpoint and session configuration   |
|   [4]   | `ClientConfiguration`          | configuration | client session and connection configuration |
|   [5]   | `TransportQuotas`              | configuration | message size, timeout, and channel limits   |
|   [6]   | `TraceConfiguration`           | configuration | diagnostic trace settings                   |
|   [7]   | `ConfiguredEndpointCollection` | collection    | persisted endpoint registry                 |
|   [8]   | `ConfiguredEndpoint`           | record class  | one persisted server endpoint               |
|   [9]   | `ServerSecurityPolicy`         | record class  | one security policy (mode + URI)            |

[PUBLIC_TYPE_SCOPE]: session and client — `Opc.Ua`
- rail: opcua-core

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------------- | :------------ | :----------------------------------------------- |
|   [1]   | `SessionClient`      | class         | full OPC UA session client, extends `ClientBase` |
|   [2]   | `ClientBase`         | class         | channel-backed RPC client base                   |
|   [3]   | `DiscoveryClient`    | class         | discovery endpoint client                        |
|   [4]   | `ISessionClient`     | interface     | session operation contract                       |
|   [5]   | `IUserIdentity`      | interface     | user identity token contract                     |
|   [6]   | `UserIdentity`       | class         | anonymous, username, certificate identity        |
|   [7]   | `ReverseConnectHost` | class         | reverse-connect listener                         |
|   [8]   | `SessionChannel`     | class         | session-scoped transport channel                 |

[PUBLIC_TYPE_SCOPE]: certificate and PKI — `Opc.Ua.Security.Certificates`
- rail: opcua-core

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :--------------------------- | :------------ | :---------------------------------------------- |
|   [1]   | `CertificateFactory`         | static class  | creation, revocation, and signing-request ops   |
|   [2]   | `CertificateValidator`       | class         | chain validation and trust-list evaluation      |
|   [3]   | `CertificateIdentifier`      | class         | thumbprint/subject/URI certificate selector     |
|   [4]   | `CertificateStoreIdentifier` | class         | directory or Windows store path + type          |
|   [5]   | `CertificateTrustList`       | class         | trust list backed by a certificate store        |
|   [6]   | `ICertificateStore`          | interface     | certificate store open/find/add/remove contract |
|   [7]   | `ICertificateValidator`      | interface     | validation contract                             |
|   [8]   | `DirectoryCertificateStore`  | class         | PEM/DER file-system certificate store           |
|   [9]   | `X509CertificateStore`       | class         | Windows X.509 store adapter                     |

[PUBLIC_TYPE_SCOPE]: PubSub application — `Opc.Ua.PubSub`
- rail: opcua-pubsub

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]  | [CAPABILITY]                              |
| :-----: | :------------------------------- | :------------- | :---------------------------------------- |
|   [1]   | `UaPubSubApplication`            | class          | PubSub application root; owns connections |
|   [2]   | `IUaPubSubConnection`            | interface      | one publish/subscribe connection contract |
|   [3]   | `IUaPublisher`                   | interface      | publisher contract                        |
|   [4]   | `IUaPubSubDataStore`             | interface      | dataset storage for published values      |
|   [5]   | `UaDataSetMessage`               | abstract class | base for UADP and JSON dataset messages   |
|   [6]   | `UaNetworkMessage`               | abstract class | base for UADP and JSON network messages   |
|   [7]   | `TransportProtocol`              | enum           | MQTT, UDP, AMQP transport selector        |
|   [8]   | `MessageMapping`                 | enum           | UADP or JSON encoding selector            |
|   [9]   | `DataSetFlags1EncodingMask`      | enum (byte)    | dataset message field-inclusion flags     |
|  [10]   | `UADPFlagsEncodingMask`          | enum (byte)    | UADP network message flags                |
|  [11]   | `EnumMqttProtocolVersion`        | enum           | MQTT 3.1 / 5.0 version selector           |
|  [12]   | `IntervalRunner`                 | class          | periodic publish interval runner          |
|  [13]   | `ConfigurationUpdatingEventArgs` | event args     | configuration update notification         |
|  [14]   | `SubscribedDataEventArgs`        | event args     | received dataset notification             |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: application configuration construction
- rail: opcua-core

| [INDEX] | [SURFACE]                                                                               | [ENTRY_FAMILY] | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------------------------------------- | :------------- | :--------------------------------------- |
|   [1]   | `ApplicationConfiguration.Load(sectionName, applicationType)`                           | factory async  | loads config from app.config section     |
|   [2]   | `ApplicationConfiguration.LoadAsync(sectionName, type, logger, telemetry, ct)`          | factory async  | telemetry-enabled async load             |
|   [3]   | `ApplicationConfiguration.Load(FileInfo, applicationType, systemType)`                  | factory async  | loads from explicit file path            |
|   [4]   | `ApplicationConfiguration.LoadAsync(FileInfo, type, systemType, applyTrace, telemetry)` | factory async  | async file-path load with trace settings |
|   [5]   | `ApplicationConfiguration.Load(Stream, applicationType, systemType, applyTrace)`        | factory async  | loads from stream                        |
|   [6]   | `ConfiguredEndpointCollection.Load(ApplicationConfiguration, filePath, telemetry)`      | factory static | loads persisted endpoint registry        |

[ENTRYPOINT_SCOPE]: session client lifecycle
- rail: opcua-core

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY] | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------------- | :------------- | :------------------------------------ |
|   [1]   | `SessionClient(ITransportChannel, ITelemetryContext)`                        | constructor    | creates client over existing channel  |
|   [2]   | `SessionClient.CreateSessionAsync(requestHeader, ..., ct)`                   | async call     | creates OPC UA session                |
|   [3]   | `SessionClient.ActivateSessionAsync(requestHeader, ..., ct)`                 | async call     | activates session with identity token |
|   [4]   | `SessionClient.CloseSessionAsync(requestHeader, delete, ct)`                 | async call     | closes session                        |
|   [5]   | `SessionClient.ReadAsync(requestHeader, maxAge, timestamps, nodes, ct)`      | async call     | reads node values                     |
|   [6]   | `SessionClient.WriteAsync(requestHeader, nodesToWrite, ct)`                  | async call     | writes node values                    |
|   [7]   | `SessionClient.BrowseAsync(requestHeader, view, maxRefs, nodesToBrowse, ct)` | async call     | browses address space                 |
|   [8]   | `SessionClient.CreateSubscriptionAsync(requestHeader, ..., ct)`              | async call     | creates subscription                  |

[ENTRYPOINT_SCOPE]: certificate PKI operations
- rail: opcua-core

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------ | :-------------- | :--------------------------------------- |
|   [1]   | `CertificateFactory.CreateCertificate(subjectName)`                       | builder factory | starts `ICertificateBuilder` chain       |
|   [2]   | `CertificateFactory.CreateCertificate(appUri, appName, subject, domains)` | builder factory | creates application certificate          |
|   [3]   | `CertificateFactory.RevokeCertificate(issuer, crls, revoked)`             | static call     | produces updated CRL                     |
|   [4]   | `CertificateFactory.CreateSigningRequest(certificate, domainNames)`       | static call     | generates CSR bytes                      |
|   [5]   | `CertificateValidator.Validate(certificate)`                              | validation call | validates against configured trust lists |
|   [6]   | `CertificateStoreIdentifier.CreateStore(storeTypeName, telemetry)`        | static factory  | opens a certificate store by type name   |

[ENTRYPOINT_SCOPE]: PubSub application lifecycle
- rail: opcua-pubsub

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------ | :-------------- | :--------------------------------------- |
|   [1]   | `UaPubSubApplication.Create(ITelemetryContext)`                     | static factory  | creates application with no data store   |
|   [2]   | `UaPubSubApplication.Create(IUaPubSubDataStore, ITelemetryContext)` | static factory  | creates application with data store      |
|   [3]   | `UaPubSubApplication.Create(configFilePath, telemetry, dataStore?)` | static factory  | creates application from config file     |
|   [4]   | `UaPubSubApplication.SupportedTransportProfiles`                    | static property | returns supported transport profile URIs |
|   [5]   | `UaPubSubApplication.Start()`                                       | lifecycle call  | starts all configured connections        |
|   [6]   | `UaPubSubApplication.Stop()`                                        | lifecycle call  | stops all connections                    |
|   [7]   | `UaPubSubApplication.Dispose()`                                     | lifetime call   | releases connections and data store      |

## [4]-[IMPLEMENTATION_LAW]

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

[RAIL_LAW]:
- Package: `OPCFoundation.NetStandard.Opc.Ua` + `OPCFoundation.NetStandard.Opc.Ua.PubSub`
- Owns: OPC UA application lifecycle, session client, certificate PKI, publish-subscribe transport
- Accept: configuration-loaded endpoints, certificate-store-backed PKI, async session operations
- Reject: hand-rolled binary encoding, inline security policy strings, direct low-level channel construction outside `ApplicationConfiguration`
