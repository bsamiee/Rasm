# [RASM_APPHOST_API_OPCUA]

`OPCFoundation.NetStandard.Opc.Ua` owns the AppHost OPC UA client stack: managed `Session`/`Subscription`/`MonitoredItem` streaming above the low-level RPC `SessionClient`, application-configuration loading, and certificate-store PKI. `OPCFoundation.NetStandard.Opc.Ua.PubSub` owns the publish-subscribe dataset transport over MQTT and UDP.

## [01]-[PUBLIC_TYPES]

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

[PUBLIC_TYPE_SCOPE]: write refusal and status — `Opc.Ua`

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `ISessionClientMethods`    | interface     | declares every session RPC, `Write` included     |
|  [02]   | `SessionClientBatched`     | class         | operation-limit batching over `SessionClient`    |
|  [03]   | `WriteResponse`            | class         | `ResponseHeader` + `Results` + `DiagnosticInfos` |
|  [04]   | `ResponseHeader`           | class         | service-level verdict on every response          |
|  [05]   | `ServiceResult`            | class         | lifted status, symbolic id, and inner cause      |
|  [06]   | `ServiceResultException`   | exception     | thrown form of a `ServiceResult`                 |
|  [07]   | `StatusCodes`              | static class  | `public const uint` status roster                |
|  [08]   | `DiagnosticInfoCollection` | collection    | per-element diagnostics beside `Results`         |
|  [09]   | `Profiles`                 | static class  | transport profile URI constants                  |

[PUBLIC_TYPE_SCOPE]: managed client — `Opc.Ua.Client`

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :----------------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `Session`                            | class         | managed session over `SessionClient`; publish + keep-alive  |
|  [02]   | `ISession`                           | interface     | managed session contract                                    |
|  [03]   | `Subscription`                       | class         | one subscription; publishing interval, keep-alive, items    |
|  [04]   | `MonitoredItem`                      | class         | one monitored node; start node, attribute, sampling, mode   |
|  [05]   | `MonitoredItemNotificationEventArgs` | event args    | per-item notification carrying `NotificationValue`          |
|  [06]   | `MonitoredItemNotification`          | class         | notification body; `DataValue Value`, `uint ClientHandle`   |
|  [07]   | `ReverseConnectManager`              | class         | reverse-connect endpoint manager passed to `CreateAsync`    |
|  [08]   | `ITelemetryContext`                  | interface     | telemetry context threaded into managed client constructors |
|  [09]   | `MonitoringMode`                     | enum          | `Disabled` / `Sampling` / `Reporting`                       |
|  [10]   | `NotificationMessage`                | class         | publish batch; `uint SequenceNumber` orders redeliveries    |
|  [11]   | `MonitoredItemOptions`               | record        | serializable item request; `QueueSize` defaults to `0`      |
|  [12]   | `MonitoredItemStatus`                | record        | the server's REVISED item parameters and per-item `Error`   |

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
|  [12]   | `MonitoringFilter`      | class         | filter base every monitored-item filter derives from       |
|  [13]   | `DataChangeFilter`      | class         | data-item filter; trigger, deadband type, deadband value   |
|  [14]   | `DataChangeTrigger`     | enum          | `Status` / `StatusValue` / `StatusValueTimestamp`          |
|  [15]   | `DeadbandType`          | enum          | `None` / `Absolute` / `Percent`                            |

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

[PUBLIC_TYPE_SCOPE]: PubSub configuration and decode — `Opc.Ua.PubSub`, `Opc.Ua.PubSub.PublishedData`, `Opc.Ua`

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :------------------------------------ | :------------ | :---------------------------------------------------------- |
|  [01]   | `UaPubSubConfigurator`                | class         | configuration mutation, every member returning `StatusCode` |
|  [02]   | `ValidateBrokerCertificateHandler`    | delegate      | `bool(X509Certificate2)` broker-certificate gate            |
|  [03]   | `PublishedData.DataSet`               | class         | `Name`, `Fields`, `IsDeltaFrame`, metadata                  |
|  [04]   | `PublishedData.Field`                 | class         | `Value`, `TargetNodeId`, `TargetAttribute`, metadata        |
|  [05]   | `PublishedData.FieldMetaData`         | class         | per-field declared type and name                            |
|  [06]   | `DataSetDecodeErrorEventArgs`         | event args    | decode failure carrying message and reader                  |
|  [07]   | `DataSetDecodeErrorReason`            | enum          | `NoError` / `MetadataMajorVersion`                          |
|  [08]   | `RawDataReceivedEventArgs`            | event args    | undecoded network-message bytes                             |
|  [09]   | `PublisherEndpointsEventArgs`         | event args    | publisher endpoint announcement                             |
|  [10]   | `DataSetWriterConfigurationEventArgs` | event args    | writer-configuration announcement                           |
|  [11]   | `PubSubConfigurationDataType`         | data type     | whole PubSub configuration document                         |
|  [12]   | `PubSubConnectionDataType`            | data type     | one connection, its groups, and transport                   |
|  [13]   | `WriterGroupDataType`                 | data type     | publish cadence and its dataset writers                     |
|  [14]   | `DataSetReaderDataType`               | data type     | one subscribed dataset and its target set                   |
|  [15]   | `PublishedDataSetDataType`            | data type     | one published dataset and its source                        |

## [02]-[ENTRYPOINTS]

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

| [INDEX] | [SURFACE]                         | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :-------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `Subscription.PublishingInterval` | property | `int` publish cadence, ms                       |
|  [02]   | `Subscription.KeepAliveCount`     | property | `uint` empty-publish threshold                  |
|  [03]   | `Subscription.LifetimeCount`      | property | `uint` server-side expiry                       |
|  [04]   | `MonitoredItem.StartNodeId`       | property | `NodeId` monitored node                         |
|  [05]   | `MonitoredItem.AttributeId`       | property | `uint` monitored attribute                      |
|  [06]   | `MonitoredItem.SamplingInterval`  | property | `int` sample cadence, ms                        |
|  [07]   | `MonitoredItem.MonitoringMode`    | property | `MonitoringMode` notification mode              |
|  [08]   | `MonitoredItem.QueueSize`         | property | `uint` SERVER-side notification queue depth     |
|  [09]   | `MonitoredItem.DiscardOldest`     | property | `bool` server discard end, default `true`       |
|  [10]   | `MonitoredItem.Filter`            | property | `MonitoringFilter?` server-side sampling filter |
|  [11]   | `WriteValue.NodeId`               | property | `NodeId` write node                             |
|  [12]   | `WriteValue.AttributeId`          | property | `uint` write attribute                          |
|  [13]   | `WriteValue.Value`                | property | `DataValue` write payload                       |

[ENTRYPOINT_SCOPE]: monitored-item sampling policy and the server's verdict on it

`QueueSize`, `DiscardOldest`, `SamplingInterval`, and `Filter` cross together as one `MonitoredItemCreateRequest.RequestedParameters`, and the server answers each with a revised value on `MonitoredItem.Status`.

| [INDEX] | [SURFACE]                              | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `MonitoredItem.Status`                 | property | `MonitoredItemStatus`, the server's own answer  |
|  [02]   | `MonitoredItemStatus.Error`            | property | `ServiceResult?` per-item create/modify refusal |
|  [03]   | `MonitoredItemStatus.QueueSize`        | property | `uint` queue depth the server GRANTED           |
|  [04]   | `MonitoredItemStatus.SamplingInterval` | property | `double` interval the server GRANTED            |
|  [05]   | `MonitoredItemStatus.FilterResult`     | property | `MonitoringFilterResult?` filter verdict        |
|  [06]   | `MonitoredItemStatus.Created`          | property | `bool`, true once the server assigned an id     |
|  [07]   | `DataChangeFilter.Trigger`             | property | `DataChangeTrigger` reporting trigger           |
|  [08]   | `DataChangeFilter.DeadbandType`        | property | `uint` slot the `DeadbandType` roster fills     |
|  [09]   | `DataChangeFilter.DeadbandValue`       | property | `double` absolute or percent band               |
|  [10]   | `DataChangeFilter.Validate()`          | instance | `ServiceResult` pre-wire refusal                |
|  [11]   | `Subscription.CreateItemsAsync(ct)`    | instance | `Task<IList<MonitoredItem>>`, per-item results  |

- `DataChangeFilter.Validate()` answers `ServiceResult.Good` on admission — never null, unlike `WriteValue.Validate` — and refuses an unrostered deadband type or trigger, a negative `DeadbandValue`, and a percent band past `100`.
- `MonitoredItemStatus.DiscardOldest` initializes `true`; every other granted column starts at its type default until the server answers.

[ENTRYPOINT_SCOPE]: node write and its refusal path

`Write` declares on `Opc.Ua.ISessionClientMethods`, NOT on `ISessionClient`, and `Opc.Ua.SessionClient` implements it `virtual`. `Opc.Ua.Client.Session` declares no `Write` member of its own — it inherits through `SessionClientBatched : SessionClient`, which overrides `WriteAsync` to split by `OperationLimits.MaxNodesPerWrite` and concatenate each batch's `Results`, `DiagnosticInfos`, and `StringTable`.

| [INDEX] | [SURFACE]                                               | [SHAPE]      | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------ | :----------- | :----------------------------------------- |
|  [01]   | `ISessionClientMethods.WriteAsync(header, nodes, ct)`   | interface    | `Task<WriteResponse>`, the live write      |
|  [02]   | `SessionClient.WriteAsync(...)`                         | virtual      | the implementing override                  |
|  [03]   | `SessionClientBatched.WriteAsync(...)`                  | override     | splits on `MaxNodesPerWrite`, concatenates |
|  [04]   | `Write(header, nodes, out results, out diagnostics)`    | `[Obsolete]` | synchronous write                          |
|  [05]   | `BeginWrite(header, nodes, callback, state)`            | `[Obsolete]` | APM begin half                             |
|  [06]   | `EndWrite(result, out results, out diagnostics)`        | `[Obsolete]` | APM end half                               |
|  [07]   | `WriteResponse.ResponseHeader`                          | property     | service-level verdict                      |
|  [08]   | `WriteResponse.Results -> StatusCodeCollection`         | property     | one `StatusCode` per written node          |
|  [09]   | `WriteResponse.DiagnosticInfos`                         | property     | `DiagnosticInfoCollection`, optional       |
|  [10]   | `ClientBase.GetResult(...)`                             | static       | lift one element to `ServiceResult`        |
|  [11]   | `ClientBase.ValidateDataValue(...)`                     | static       | lift one read element to `ServiceResult`   |
|  [12]   | `WriteValue.Validate(WriteValue) -> ServiceResult`      | static       | pre-wire refusal, null admits              |
|  [13]   | `ClientBase.ValidateResponse(ResponseHeader)`           | static       | throws on a null or bad service header     |
|  [14]   | `ClientBase.ValidateResponse<TRequest, TResponse>(...)` | static       | arity guard, reads no status value         |
|  [15]   | `ClientBase.ValidateDiagnosticInfos<TRequest>(...)`     | static       | arity guard, reads no status value         |

- `ClientBase.GetResult(StatusCode, int, DiagnosticInfoCollection?, ResponseHeader?) -> ServiceResult` spells the per-element lift in full, and `ClientBase.ValidateDataValue(DataValue, Type, int, DiagnosticInfoCollection, ResponseHeader) -> ServiceResult` its read-side twin.
- `ClientBase.ValidateResponse<TRequest, TResponse>(IReadOnlyList<TResponse>, IReadOnlyList<TRequest>)` and `ValidateDiagnosticInfos<TRequest>(...)` compare element counts alone and read no status value.
- `WriteValue.Validate` answers null where the value admits, else `BadStructureMissing`, `BadNodeIdInvalid`, `BadAttributeIdInvalid`, `BadIndexRangeInvalid`, `BadIndexRangeNoData`, or `BadTypeMismatch` — the one client-side refusal landing before the wire.

[ENTRYPOINT_SCOPE]: `StatusCode` and `ServiceResult`

`StatusCode` is a struct whose SIX predicates are all `static bool(StatusCode)`; no instance predicate exists, so a check spells `StatusCode.IsBad(code)`.

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `StatusCode.Code -> uint`                                                        | property | get and set                             |
|  [02]   | `StatusCode.SymbolicId -> string`                                                | property | resolved symbolic name                  |
|  [03]   | `CodeBits` / `FlagBits` / `SubCode`                                              | property | code-space partitions                   |
|  [04]   | `StructureChanged` / `SemanticsChanged`                                          | property | structure and semantics flag bits       |
|  [05]   | `HasDataValueInfo` / `LimitBits` / `Overflow` / `AggregateBits`                  | property | data-value flag bits                    |
|  [06]   | `SetCodeBits(uint)` / `SetFlagBits(uint)`                                        | instance | fluent bit writes returning a new value |
|  [07]   | `implicit operator StatusCode(uint)`                                             | operator | widen a raw code                        |
|  [08]   | `explicit operator uint(StatusCode)`                                             | operator | narrow to the raw code                  |
|  [09]   | `IsGood` / `IsNotGood` / `IsUncertain` / `IsNotUncertain` / `IsBad` / `IsNotBad` | static   | `bool(StatusCode)`, the only predicates |

`ServiceResult` lifts one `StatusCode` with its symbolic identity, diagnostics, and inner cause.

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `ServiceResult.Good` / `.Bad`                                                    | static   | canonical results                       |
|  [02]   | `Code -> uint` / `StatusCode -> StatusCode`                                      | property | numeric and struct forms                |
|  [03]   | `NamespaceUri` / `SymbolicId`                                                    | property | symbolic identity of the fault          |
|  [04]   | `LocalizedText` / `AdditionalInfo`                                               | property | human-readable diagnostics              |
|  [05]   | `InnerResult -> ServiceResult`                                                   | property | nested cause                            |
|  [06]   | `GetServiceResultException()`                                                    | instance | lift to `ServiceResultException`        |
|  [07]   | `ToString()`                                                                     | instance | formatted text                          |
|  [08]   | `IsGood` / `IsNotGood` / `IsUncertain` / `IsNotUncertain` / `IsBad` / `IsNotBad` | static   | null-tolerant predicates                |
|  [09]   | `IsGoodOrUncertain`                                                              | static   | seventh predicate, no `StatusCode` twin |

[ENTRYPOINT_SCOPE]: write-relevant `StatusCodes` constants

`Opc.Ua.StatusCodes` declares each entry `public const uint`; `GetBrowseName(uint)`, `GetIdentifier(string)`, and the `GetSymbolicId(this StatusCode)` extension resolve names both ways.

| [INDEX] | [MEMBER]                      |  [VALUE]   | [MEANING]                             |
| :-----: | :---------------------------- | :--------: | :------------------------------------ |
|  [01]   | `Good`                        | 0x00000000 | write accepted                        |
|  [02]   | `GoodCompletesAsynchronously` | 0x002E0000 | accepted, completion deferred         |
|  [03]   | `GoodClamped`                 | 0x00300000 | accepted after clamping to range      |
|  [04]   | `UncertainLastUsableValue`    | 0x40900000 | value stale but usable                |
|  [05]   | `BadNotWritable`              | 0x803B0000 | attribute refuses writes              |
|  [06]   | `BadWriteNotSupported`        | 0x80730000 | server implements no write            |
|  [07]   | `BadUserAccessDenied`         | 0x801F0000 | identity refuses the write            |
|  [08]   | `BadNodeIdUnknown`            | 0x80340000 | node id resolves to nothing           |
|  [09]   | `BadNodeIdInvalid`            | 0x80330000 | node id is malformed                  |
|  [10]   | `BadAttributeIdInvalid`       | 0x80350000 | attribute id inadmissible on the node |
|  [11]   | `BadIndexRangeInvalid`        | 0x80360000 | index range is malformed              |
|  [12]   | `BadIndexRangeNoData`         | 0x80370000 | index range selects nothing           |
|  [13]   | `BadTypeMismatch`             | 0x80740000 | value type mismatches the node        |
|  [14]   | `BadOutOfRange`               | 0x803C0000 | value outside the node range          |
|  [15]   | `BadOutOfService`             | 0x808D0000 | node taken out of service             |
|  [16]   | `BadNoCommunication`          | 0x80310000 | underlying device unreachable         |
|  [17]   | `BadDeviceFailure`            | 0x808B0000 | device-level fault                    |
|  [18]   | `BadSensorFailure`            | 0x808C0000 | sensor-level fault                    |
|  [19]   | `BadTimeout`                  | 0x800A0000 | operation exceeded its bound          |
|  [20]   | `BadLocked`                   | 0x80E90000 | node locked by another owner          |
|  [21]   | `BadRequestNotAllowed`        | 0x80E40000 | server refuses the request outright   |
|  [22]   | `BadTooManyOperations`        | 0x80100000 | request exceeds the operation limit   |
|  [23]   | `BadServerNotConnected`       | 0x800D0000 | no channel to the server              |
|  [24]   | `BadSessionIdInvalid`         | 0x80250000 | session no longer valid               |
|  [25]   | `BadSecureChannelClosed`      | 0x80860000 | channel closed under the session      |
|  [26]   | `BadUnknownResponse`          | 0x80090000 | service answered with nothing         |

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

`UaPubSubApplication` exposes NO public constructor: five `Create` factories are the only construction path, the fifth taking `(PubSubConfigurationDataType, IUaPubSubDataStore, ITelemetryContext)`. Every member below declares on `UaPubSubApplication`.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `Create(ITelemetryContext)`                              | factory  | application with no data store                      |
|  [02]   | `Create(IUaPubSubDataStore, ITelemetryContext)`          | factory  | application over a supplied store                   |
|  [03]   | `Create(configFilePath, telemetry, dataStore?)`          | factory  | application from a configuration file               |
|  [04]   | `Create(PubSubConfigurationDataType, ITelemetryContext)` | factory  | application from an in-memory document              |
|  [05]   | `Create(config, dataStore, telemetry)`                   | factory  | document and store together                         |
|  [06]   | `SupportedTransportProfiles`                             | static   | `string[]` of three profile URIs                    |
|  [07]   | `UaPubSubConfigurator`                                   | property | the live configuration mutator                      |
|  [08]   | `DataStore -> IUaPubSubDataStore`                        | property | published-value storage                             |
|  [09]   | `PubSubConnections`                                      | property | `ReadOnlyList<IUaPubSubConnection>`                 |
|  [10]   | `ApplicationId -> string`                                | property | get and set                                         |
|  [11]   | `OnValidateBrokerCertificate`                            | field    | public `ValidateBrokerCertificateHandler`           |
|  [12]   | `Start()` / `Stop()`                                     | instance | `void`, signalling nothing                          |
|  [13]   | `Dispose()`                                              | instance | releases connections and data store                 |
|  [14]   | `DataReceived`                                           | event    | `EventHandler<SubscribedDataEventArgs>`             |
|  [15]   | `MetaDataReceived`                                       | event    | `EventHandler<SubscribedDataEventArgs>`             |
|  [16]   | `RawDataReceived`                                        | event    | `EventHandler<RawDataReceivedEventArgs>`            |
|  [17]   | `PublisherEndpointsReceived`                             | event    | `EventHandler<PublisherEndpointsEventArgs>`         |
|  [18]   | `ConfigurationUpdating`                                  | event    | `EventHandler<ConfigurationUpdatingEventArgs>`      |
|  [19]   | `DataSetWriterConfigurationReceived`                     | event    | `EventHandler<DataSetWriterConfigurationEventArgs>` |

[ENTRYPOINT_SCOPE]: PubSub decode chain

`DataReceived` hands one `SubscribedDataEventArgs`, and the value path runs `NetworkMessage` to `DataSetMessages` to `DataSet` to `Fields` to `Value`. `DataSet`, `Field`, and `FieldMetaData` live in `Opc.Ua.PubSub.PublishedData`; every other member below in `Opc.Ua.PubSub`.

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :----------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `SubscribedDataEventArgs.NetworkMessage -> UaNetworkMessage` | property | internal setter, decode entry                  |
|  [02]   | `SubscribedDataEventArgs.Source -> string`                   | property | originating connection                         |
|  [03]   | `UaNetworkMessage.DataSetMessages -> List<UaDataSetMessage>` | property | decoded dataset messages                       |
|  [04]   | `UaNetworkMessage.DataSetMetaData -> DataSetMetaDataType`    | property | metadata payload                               |
|  [05]   | `UaNetworkMessage.IsMetaDataMessage -> bool`                 | property | true where metadata rides alone                |
|  [06]   | `UaNetworkMessage.WriterGroupId -> ushort`                   | property | producing writer group                         |
|  [07]   | `UaNetworkMessage.DataSetWriterId -> ushort?`                | property | null unless one writer produced it             |
|  [08]   | `UaNetworkMessage.DataSetDecodeErrorOccurred`                | event    | `EventHandler<DataSetDecodeErrorEventArgs>`    |
|  [09]   | `UaDataSetMessage.DataSet -> DataSet`                        | property | internal setter, null on decode failure        |
|  [10]   | `UaDataSetMessage.DataSetWriterId -> ushort`                 | property | producing writer                               |
|  [11]   | `UaDataSetMessage.SequenceNumber -> uint`                    | property | publisher-assigned ordering                    |
|  [12]   | `UaDataSetMessage.Timestamp -> DateTime`                     | property | publish instant                                |
|  [13]   | `UaDataSetMessage.Status -> StatusCode`                      | property | message-level verdict                          |
|  [14]   | `UaDataSetMessage.FieldContentMask`                          | property | `DataSetFieldContentMask` inclusion flags      |
|  [15]   | `UaDataSetMessage.MetaDataVersion`                           | property | `ConfigurationVersionDataType` of the payload  |
|  [16]   | `UaDataSetMessage.DecodeErrorReason`                         | property | `DataSetDecodeErrorReason` on the failing path |
|  [17]   | `DataSet.Fields -> Field[]`                                  | property | the field array, one entry per metadata field  |
|  [18]   | `DataSet.Name -> string`                                     | property | dataset name off the metadata                  |
|  [19]   | `DataSet.DataSetWriterId -> int`                             | property | producing writer, widened                      |
|  [20]   | `DataSet.SequenceNumber -> uint`                             | property | internal setter, publisher ordering            |
|  [21]   | `DataSet.IsDeltaFrame -> bool`                               | property | delta versus key frame                         |
|  [22]   | `DataSet.DataSetMetaData -> DataSetMetaDataType`             | property | describing metadata                            |
|  [23]   | `Field.Value -> DataValue`                                   | property | value, `StatusCode`, and timestamps            |
|  [24]   | `Field.TargetNodeId -> NodeId`                               | property | subscriber-side target node                    |
|  [25]   | `Field.TargetAttribute -> uint`                              | property | subscriber-side target attribute               |
|  [26]   | `Field.FieldMetaData -> FieldMetaData`                       | property | internal setter, declared field shape          |

[ENTRYPOINT_SCOPE]: `UaPubSubConfigurator` mutation API

Every mutator answers `StatusCode` and throws nothing, so a configuration change reports its refusal as a returned value.

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `AddConnection(PubSubConnectionDataType)`             | instance | seat one connection                     |
|  [02]   | `AddWriterGroup(uint parentConnectionId, ...)`        | instance | seat a writer group under a connection  |
|  [03]   | `AddDataSetWriter(uint parentWriterGroupId, ...)`     | instance | seat a dataset writer under a group     |
|  [04]   | `AddReaderGroup(...)` / `AddDataSetReader(...)`       | instance | seat the subscribe half                 |
|  [05]   | `AddPublishedDataSet(...)` / `AddExtensionField(...)` | instance | seat a published dataset and its fields |
|  [06]   | `Remove*(...)`                                        | instance | retire any seated element               |
|  [07]   | `Enable(...)` / `Disable(...)`                        | instance | arm or disarm a seated element          |

[ENTRYPOINT_SCOPE]: PubSub configuration data types

- `PubSubConnectionDataType`: `Name`, `Enabled`, `Variant PublisherId`, `string TransportProfileUri`, `ExtensionObject Address`, `KeyValuePairCollection ConnectionProperties`, `ExtensionObject TransportSettings`, `WriterGroupDataTypeCollection WriterGroups`, `ReaderGroupDataTypeCollection ReaderGroups`.
- `PubSubGroupDataType` is the group base: `Name`, `Enabled`, `MessageSecurityMode SecurityMode`, `SecurityGroupId`, `EndpointDescriptionCollection SecurityKeyServices`, `uint MaxNetworkMessageSize`, `GroupProperties`.
- `WriterGroupDataType : PubSubGroupDataType` adds `ushort WriterGroupId`, `double PublishingInterval`, `double KeepAliveTime`, `byte Priority`, `StringCollection LocaleIds`, `HeaderLayoutUri`, `TransportSettings`, `MessageSettings`, `DataSetWriterDataTypeCollection DataSetWriters`.
- `DataSetReaderDataType`: `Name`, `Enabled`, `Variant PublisherId`, `ushort WriterGroupId`, `ushort DataSetWriterId`, `DataSetMetaDataType DataSetMetaData`, `uint DataSetFieldContentMask`, `double MessageReceiveTimeout`, `uint KeyFrameCount`, `HeaderLayoutUri`, `SecurityMode`, `SecurityGroupId`, `SecurityKeyServices`, `DataSetReaderProperties`, `TransportSettings`, `MessageSettings`, `ExtensionObject SubscribedDataSet`.
- `PublishedDataSetDataType`: `Name`, `StringCollection DataSetFolder`, `DataSetMetaDataType DataSetMetaData`, `KeyValuePairCollection ExtensionFields`, `ExtensionObject DataSetSource`.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Opc.Ua` holds over 1000 types; configuration, certificate, channel, and address-space types coexist in this one namespace.
- Managed `Opc.Ua.Client` (`Session`/`Subscription`/`MonitoredItem`) sits above `SessionClient` and owns the publish loop, keep-alive, and notification fan; the `SessionClient`/`ClientBase` RPC surface is the inherited read/write base, never the direct subscription owner.
- `Subscription.PublishingInterval` is the `int` policy the row sets; `CurrentPublishingInterval` reads back the server-negotiated `double`, never cast to a `TimeSpan`.
- `Opc.Ua.Profiles` in `Opc.Ua.Core.dll` declares the transport URIs — `PubSubUdpUadpTransport` is `http://opcfoundation.org/UA-Profile/Transport/pubsub-udp-uadp`, with `PubSubMqttUadpTransport` and `PubSubMqttJsonTransport` its `pubsub-mqtt-uadp` and `pubsub-mqtt-json` siblings; the static `UaPubSubApplication.SupportedTransportProfiles` re-inlines the same three, ordered udp-uadp, mqtt-json, mqtt-uadp.
- `SessionClient.WriteAsync` throws at two sites alone: a null service response raising `BadUnknownResponse`, and `ClientBase.ValidateResponse(ResponseHeader)` raising on a null or bad SERVICE-level result. `Results` is never inspected on the way out, so a per-node refusal arrives as one element of `Results` and throws nothing.
- `MonitoredItemOptions.QueueSize` initializes to `0` and `DiscardOldest` to `true`; a data item forwards that zero verbatim (the setter rewrites only `int.MaxValue`, which it maps to `1`), so an item left unset arms a server queue holding ONE value and the server discards every change between two publishes.
- Part 4 sets the `Overflow` InfoBit on the first `DataValue` a client receives after such a discard, and `StatusCode.Overflow` reads it only where `HasDataValueInfo` (`0x400`) is set, so the getter already gates its own precondition.
- TRAP: `MonitoredItem.Filter`'s SETTER runs `ValidateFilter(NodeClass, value)`, which THROWS `ServiceResultException` for a node class admitting no filter (`Object`, `Method`, `ObjectType`, and the type-node classes) and silently REWRITES `State.NodeClass` when the filter kind disagrees with it; `MonitoredItemOptions.NodeClass` initializes to `Variable`, where a `DataChangeFilter` admits and neither branch fires.
- TRAP: `Subscription.CreateAsync` runs `CreateItemsAsync` internally, and that member lands each per-item refusal on `MonitoredItem.Status.Error` through `SetCreateResult` and THROWS NOTHING — only a service-level fault raises — so an item the server declined leaves a created subscription that will never notify.
- TRAP: `ServiceResult`'s statics are NULL-TOLERANT with asymmetric defaults — `IsGood(null)` is true, `IsBad(null)` false, `IsNotBad(null)` true, `IsUncertain(null)` false — so an unassigned result reads as good on the good check and as not-bad on the bad check.
- TRAP: `ServiceResult.ToLongString` DOES NOT EXIST at this pin; `ToString()` is the only formatted read, and a fence spelling the long form fails to compile.
- TRAP: `UaDataSetMessage.DataSet` is null wherever decode failed — the UADP decoder logs and answers null — and a DELTA frame yields a full-length `Fields` array whose untransmitted entries carry a null `Value` beside live `FieldMetaData`. Both nulls guard before a field projects into a value.
- Certificate stores are directory (PEM/DER), Windows X.509, and `CertificateIdentifierCollectionStore`.
- Session and PKI operations use the `*Async` variants, each taking a `CancellationToken`.

[STACKING]:
- `Wire/livewire.md` `OpcUaLane`: composes the managed `Session`/`Subscription`/`MonitoredItem` surface; each `MonitoredItem.Notification` projects one `DataValue` into `ExternalValue` and writes it to one bounded `Channel<ExternalValue>`, never running the interior on the OPC UA publish thread; its `SamplePolicy` seat declares `QueueSize`, `DiscardOldest`, and the `DataChangeFilter` so the server-side queue carries a stated depth, the opener proves `Status.Error` before handing a lane back, and the callback fans `StatusCode.Overflow` as the server's own discard evidence; `PubSubLane` composes `UaPubSubApplication.DataReceived`, fanning each dataset field into the same lane.
- `api-mqtt`(`libs/dotnet/.api/api-mqtt.md`): the peer `MQTTnet` transport row; its `IMqttClient` fan and this surface's `UaPubSubApplication.DataReceived` fan drain into the one bounded `Channel<ExternalValue>` the live-wire studio owns.

[LOCAL_ADMISSION]:
- Application configuration loads through `ApplicationConfiguration.LoadAsync` from a file, stream, or .NET configuration section.
- Low-level session construction requires a transport channel; `ClientChannelManager` owns channel lifecycle, while managed `Session.CreateAsync` builds its own channel from the configuration-loaded endpoint.
- Certificate validation runs through `CertificateValidator` initialized from `SecurityConfiguration`; trust lists persist as directory stores.
- PubSub admits one `UaPubSubApplication` per process; connections register through configuration.
- Monitored items declare `QueueSize`, `DiscardOldest`, and their `DataChangeFilter` at the consuming seat; an unset queue is a server-side discard policy no consumer chose.
- Per-item create verdicts read off `MonitoredItem.Status.Error`, never off an exception the create call does not raise.
