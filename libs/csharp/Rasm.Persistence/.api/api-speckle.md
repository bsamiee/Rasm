# [RASM_PERSISTENCE_API_SPECKLE]

`Speckle.Sdk` supplies the object-graph `Base` model, the dynamic detach/chunk serialisation, the DI-resolved `IOperations` send/receive surface, the transport family, and the GraphQL `IClient`. `Speckle.Objects` supplies the geometry roster and the `Speckle.Objects.Data` host-object family layered on `Base`. Both bind the Persistence `SyncTransport.SpeckleLikeDiff` case (owned at `Sync/collaboration`, dispatched through `SyncPump.Offer`): the INSTANCE `IOperations.Send`/`Receive` resolve from DI (never `static Operations.Send`, which does not exist), and the `Send` tuple's `rootObjId` is the content hash that maps to the existing `UInt128 ContentKey`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Speckle.Sdk`
- package: `Speckle.Sdk`
- version: `2026.6.0`
- assembly: `Speckle.Sdk`
- companion: `Speckle.Sdk.Dependencies` (`2026.6.0`; transitive; ILRepacks `Polly`, `Open.ChannelExtensions`, `Microsoft.Extensions.ObjectPool`, and the serialisation-V2 send/receive channel pipeline into one assembly)
- transitive: `GraphQL.Client`, `Microsoft.Data.Sqlite` (carries native `e_sqlite3` via `SQLitePCLRaw`), `System.Text.Json`, `Speckle.Newtonsoft.Json`, `Speckle.DoubleNumerics`
- namespace: `Speckle.Sdk`, `Speckle.Sdk.Api`, `Speckle.Sdk.Models`, `Speckle.Sdk.Transports`, `Speckle.Sdk.Serialisation`, `Speckle.Sdk.Credentials`
- target frameworks: `net10.0`, `net8.0`, `netstandard2.0`
- asset: runtime library
- rail: sync

[PACKAGE_SURFACE]: `Speckle.Objects`
- package: `Speckle.Objects`
- version: `2026.6.0`
- assembly: `Speckle.Objects`
- companion: `Speckle.Sdk` (`2026.6.0`; supplies the `Base`/`ISpeckleObject` base graph)
- namespace: `Speckle.Objects`, `Speckle.Objects.Geometry`, `Speckle.Objects.Data`, `Speckle.Objects.Primitive`, `Speckle.Objects.Other`, `Speckle.Objects.Annotation`
- target frameworks: `net10.0`, `net8.0`, `netstandard2.0`
- asset: runtime library
- rail: sync

The in-Rhino plugin assembly never references `Speckle.Sdk` or `Speckle.Objects`; the Speckle surface lives OUTSIDE-RHINO on the companion/sidecar target, where `Speckle.Sdk.Dependencies` repacks the Polly + channel + object-pool + serialisation-V2 closure into one assembly so the SDK's own dependency graph stays isolated from the host. The in-Rhino assembly composes only the canonical `SyncTransport.SpeckleLikeDiff` case and never loads the Speckle assemblies.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `Speckle.Sdk` object-graph model and attributes
- rail: sync

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------------------ | :------------ | :-------------------------------------------------------- |
|  [01]   | `Base : DynamicBase, ISpeckleObject`  | model root    | dynamic object graph; `id`/`applicationId`/`speckle_type` |
|  [02]   | `ObjectReference : Base`              | reference     | detached-child placeholder in a serialized graph          |
|  [03]   | `DetachPropertyAttribute : Attribute` | attribute     | sealed; `Detachable` (default `true`)                     |
|  [04]   | `ChunkableAttribute : Attribute`      | attribute     | sealed; `MaxObjCountPerChunk` (default `1000`)            |

[PUBLIC_TYPE_SCOPE]: `Speckle.Sdk` operations, client, transports
- rail: sync

| [INDEX] | [SYMBOL]                                                                            | [TYPE_FAMILY]       | [CAPABILITY]                                                            |
| :-----: | :---------------------------------------------------------------------------------- | :------------------ | :---------------------------------------------------------------------- |
|  [01]   | `IOperations`                                                                       | operations contract | INTERFACE; DI-resolved `Send`/`Receive`/`Serialize` (`Speckle.Sdk.Api`) |
|  [02]   | `Operations : IOperations`                                                          | operations impl     | INSTANCE class; DI primary-ctor; no static `Send`/`Receive`             |
|  [03]   | `IClient : IDisposable`                                                             | client contract     | GraphQL resources, `Account`, `ServerUrl`, `GQLClient`                  |
|  [04]   | `Client : ISpeckleGraphQLClient, IClient`                                           | client impl         | sealed; DI-constructed GraphQL client                                   |
|  [05]   | `ITransport`                                                                        | transport contract  | `SaveObject`/`GetObject`/`CopyObjectAndChildren`/`HasObjects`           |
|  [06]   | `IServerTransport : ITransport`                                                     | server contract     | server-bound transport marker                                           |
|  [07]   | `ServerTransport : IServerTransport, ITransport, IBlobCapableTransport, ICloneable` | transport (server)  | sealed; remote server object store                                      |
|  [08]   | `SQLiteTransport : ITransport, IBlobCapableTransport, ICloneable`                   | transport (local)   | sealed; default local SQLite cache                                      |
|  [09]   | `MemoryTransport : ITransport, IBlobCapableTransport, ICloneable`                   | transport (memory)  | sealed; in-process object store                                         |
|  [10]   | `ProgressArgs`                                                                      | progress value      | `readonly record struct (ProgressEvent, long Count, long? Total)`       |

[PUBLIC_TYPE_SCOPE]: `Speckle.Sdk` serialisation, credentials, DI
- rail: sync

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :-------------------------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `SpeckleObjectSerializer`                           | serializer    | `Serialize(Base)` to JSON over write transports                   |
|  [02]   | `SpeckleObjectDeserializer`                         | deserializer  | sealed; `DeserializeAsync(string?)` to `Base`                     |
|  [03]   | `Account : IEquatable<Account>`                     | credential    | `token`/`refreshToken`/`serverInfo`/`userInfo`/`id`               |
|  [04]   | `ServiceRegistration`                               | DI extensions | static; hosts `AddSpeckleSdk` on `IServiceCollection`             |
|  [05]   | `Application` (`record (string Name, string Slug)`) | DI input      | host-application identity for registration                        |
|  [06]   | `SpeckleSdkOptions` (`record`)                      | DI input      | `(Application, ApplicationVersion, SpeckleVersion?, Assemblies?)` |

[PUBLIC_TYPE_SCOPE]: `Speckle.Objects.Geometry` roster
- rail: sync

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                                                     |
| :-----: | :------------------------- | :------------ | :----------------------------------------------------------------------------------------------- |
|  [01]   | `Point : Base`             | geometry      | `ITransformable<Point>`, `IEquatable<Point>`                                                     |
|  [02]   | `Vector : Base`            | geometry      | `IHasBoundingBox`, `ITransformable<Vector>`                                                      |
|  [03]   | `Plane : Base`             | geometry      | `ITransformable<Plane>`                                                                          |
|  [04]   | `Line : Base`              | curve         | `ICurve`, `IHasBoundingBox`, `ITransformable<Line>`                                              |
|  [05]   | `Polyline : Base`          | curve         | `ICurve`, `IHasArea`, `IHasBoundingBox`, `ITransformable`                                        |
|  [06]   | `Arc : Base`               | curve         | `ICurve`, `ITransformable<Arc>`                                                                  |
|  [07]   | `Circle : Base`            | curve         | `ICurve`, `IHasArea`, `IHasBoundingBox`                                                          |
|  [08]   | `Ellipse : Base`           | curve         | `ICurve`, `IHasArea`                                                                             |
|  [09]   | `Curve : Base`             | curve         | `ICurve`, `ITransformable<Curve>`, `IDisplayValue<Polyline>`                                     |
|  [10]   | `Polycurve : Base`         | curve         | `ICurve`, `IHasArea`, `IHasBoundingBox`, `ITransformable`                                        |
|  [11]   | `Spiral : Base`            | curve         | `ICurve`, `IDisplayValue<Polyline>`                                                              |
|  [12]   | `Mesh : Base`              | geometry      | `IHasBoundingBox`, `IHasVolume`, `IHasArea`, `ITransformable<Mesh>`                              |
|  [13]   | `Brep : Base`              | geometry      | `IHasArea`, `IHasVolume`, `IHasBoundingBox`, `ITransformable<Brep>`, `IDisplayValue<List<Mesh>>` |
|  [14]   | `BrepX : RawEncodedObject` | geometry      | raw-encoded brep payload                                                                         |
|  [15]   | `Surface : Base`           | geometry      | `IHasArea`, `ITransformable<Surface>`                                                            |
|  [16]   | `Box : Base`               | geometry      | `IHasVolume`, `IHasArea`, `IHasBoundingBox`                                                      |
|  [17]   | `Pointcloud : Base`        | geometry      | `IHasBoundingBox`, `ITransformable<Pointcloud>`                                                  |

[PUBLIC_TYPE_SCOPE]: `Speckle.Objects.Data` host-object family
- rail: sync

| [INDEX] | [SYMBOL]                                                                          | [TYPE_FAMILY]    | [CAPABILITY]                                         |
| :-----: | :-------------------------------------------------------------------------------- | :--------------- | :--------------------------------------------------- |
|  [01]   | `DataObject : Base, IDataObject, IProperties, IDisplayValue<IReadOnlyList<Base>>` | host-object base | `name`/`displayValue`/`properties` carrier           |
|  [02]   | `RevitObject : DataObject, IRevitObject`                                          | host-object      | Revit-sourced data object                            |
|  [03]   | `RhinoObject : DataObject, IRhinoObject`                                          | host-object      | Rhino-sourced data object                            |
|  [04]   | `ArchicadObject : DataObject, IArchicadObject`                                    | host-object      | Archicad-sourced data object                         |
|  [05]   | `TeklaObject : DataObject, ITeklaObject`                                          | host-object      | Tekla-sourced data object                            |
|  [06]   | `Civil3dObject : DataObject, ICivilObject`                                        | host-object      | Civil3D-sourced data object                          |
|  [07]   | `AutocadObject : DataObject, IAutocadObject`                                      | host-object      | AutoCAD-sourced data object                          |
|  [08]   | `EtabsObject : DataObject, ICsiObject`                                            | host-object      | ETABS/CSI-sourced data object                        |
|  [09]   | `ArcgisObject : DataObject, IGisObject`                                           | host-object      | ArcGIS-sourced data object                           |
|  [10]   | `NavisworksObject` / `MicrostationObject` / `TsdObject`                           | host-object      | Navisworks / MicroStation / TSD-sourced data objects |

The Speckle 2026.6 model replaces the v2 `Objects.BuiltElements` typed roster (`Wall`/`Beam`/`Column`/`Duct`/`Level`) with the unified `Speckle.Objects.Data` family; no `BuiltElements` namespace and no typed built-element classes exist in 2026.6.0. Built-element geometry rides `DataObject.displayValue` as `List<Base>` (`IDisplayValue<IReadOnlyList<Base>>`), distinct from `Brep.displayValue` (`List<Mesh>`); `IDisplayValue<out T>` is the generic display-value contract.

## [03]-[ENTRYPOINTS]

[SPECKLE_SYNC]: INSTANCE `IOperations` send/receive over the DI-resolved surface
- rail: sync

| [INDEX] | [SURFACE]          | [CALL_SHAPE]                                                                                                                                                                                                                                                     | [CAPABILITY]                                       |
| :-----: | :----------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `Send`             | `Task<(string rootObjId, IReadOnlyDictionary<string, ObjectReference> convertedReferences)> Send(Base value, IServerTransport transport, bool useDefaultCache, IProgress<ProgressArgs>? onProgressAction = null, CancellationToken cancellationToken = default)` | server-transport send; `rootObjId` is content hash |
|  [02]   | `Send`             | `Task<(string rootObjId, IReadOnlyDictionary<string, ObjectReference> convertedReferences)> Send(Base value, ITransport transport, bool useDefaultCache, IProgress<ProgressArgs>? onProgressAction = null, CancellationToken cancellationToken = default)`       | single-transport send                              |
|  [03]   | `Send`             | `Task<(string rootObjId, IReadOnlyDictionary<string, ObjectReference> convertedReferences)> Send(Base value, IReadOnlyCollection<ITransport> transports, IProgress<ProgressArgs>? onProgressAction = null, CancellationToken cancellationToken = default)`       | multi-transport send (no implicit local cache)     |
|  [04]   | `Send2`            | `Task<SerializeProcessResults> Send2(Uri url, string streamId, string? authorizationToken, Base value, IProgress<ProgressArgs>? onProgressAction, CancellationToken cancellationToken, SerializeProcessOptions? options = null)`                                 | direct-to-URL send pipeline                        |
|  [05]   | `Receive`          | `Task<Base> Receive(string objectId, ITransport? remoteTransport = null, ITransport? localTransport = null, IProgress<ProgressArgs>? onProgressAction = null, CancellationToken cancellationToken = default)`                                                    | local-then-remote receive                          |
|  [06]   | `Receive2`         | `Task<Base> Receive2(Uri url, string streamId, string objectId, string? authorizationToken, IProgress<ProgressArgs>? onProgressAction, CancellationToken cancellationToken, DeserializeProcessOptions? options = null)`                                          | direct-from-URL receive pipeline                   |
|  [07]   | `Serialize`        | `string Serialize(Base value, CancellationToken cancellationToken = default)`                                                                                                                                                                                    | object-to-JSON                                     |
|  [08]   | `SerializeNew`     | `string SerializeNew(Base value)`                                                                                                                                                                                                                                | object-to-JSON over the System.Text.Json pipeline  |
|  [09]   | `DeserializeAsync` | `Task<Base> DeserializeAsync(string value, CancellationToken cancellationToken = default)`                                                                                                                                                                       | JSON-to-`Base`                                     |

`IOperations` is resolved from DI (`Operations(ILogger<Operations>, ISdkActivityFactory, ISdkMetricsFactory, ISerializeProcessFactory, IDeserializeProcessFactory) : IOperations`); the `Operations` type carries no static `Send`/`Receive`, so the `SpeckleLikeDiff` rail binds the instance members only. That rail uses the transport-bound `Send`/`Receive` overloads (entries `[1]`-`[3]`, `[5]`); `Send2`/`Receive2` are the URL-bound serialisation-V2 pipeline overloads that bypass the explicit transport stack. The `Send` tuple's `rootObjId` (first element) is the content hash of the sent graph and maps directly to the Persistence `UInt128 ContentKey`; `convertedReferences` carries the detached-child `ObjectReference` map. `Receive` returns the root `Base` for the requested `objectId`.

V2 pipeline tuning (`Send2`/`Receive2`): `SerializeProcessOptions(bool SkipCacheRead = false, bool SkipCacheWrite = false, bool SkipServer = false, bool SkipFindTotalObjects = false)` plus settable `MaxHttpSendBatchSize`/`MaxCacheBatchSize`/`MaxParallelism` knobs gate the send pipeline's cache and HTTP batching; `DeserializeProcessOptions(bool SkipCache = false, bool ThrowOnMissingReferences = true, bool SkipInvalidConverts = false, int? MaxParallelism = null, bool SkipServer = false)` gates the receive pipeline. `Send2` returns `SerializeProcessResults(string RootId, IReadOnlyDictionary<Id, ObjectReference> ConvertedReferences)` — the result dictionary keys on the Speckle `Id` value type (NOT `string`), where `RootId` is the same content hash as the transport-bound `Send` tuple's `rootObjId`.

[SPECKLE_TRANSPORT]: transport and serializer construction
- rail: sync

| [INDEX] | [SURFACE]                                    | [CALL_SHAPE]                                                                                                                                                                                                    | [CAPABILITY]               |
| :-----: | :------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------- |
|  [01]   | `ServerTransport`                            | `ServerTransport(ISpeckleHttp http, ISdkActivityFactory activityFactory, Account account, string streamId, int timeoutSeconds = 60, string? blobStorageFolder = null)`                                          | remote server store ctor   |
|  [02]   | `SQLiteTransport`                            | `SQLiteTransport(string? basePath = null, string? applicationName = null, string? scope = null)`                                                                                                                | local SQLite cache ctor    |
|  [03]   | `MemoryTransport`                            | `MemoryTransport(ConcurrentDictionary<string, string>? objects = null, bool blobStorageEnabled = false, string? basePath = null, string? applicationName = null)`                                               | in-process store ctor      |
|  [04]   | `SpeckleObjectSerializer`                    | `SpeckleObjectSerializer(IReadOnlyCollection<ITransport> writeTransports, IProgress<ProgressArgs>? onProgressAction = null, bool trackDetachedChildren = false, CancellationToken cancellationToken = default)` | write-transport serializer |
|  [05]   | `SpeckleObjectSerializer.Serialize`          | `string Serialize(Base baseObj)`                                                                                                                                                                                | serialize to JSON          |
|  [06]   | `SpeckleObjectDeserializer.DeserializeAsync` | `ValueTask<Base> DeserializeAsync(string? rootObjectJson)`                                                                                                                                                      | deserialize from JSON      |

[SPECKLE_DI]: `AddSpeckleSdk` registration on `IServiceCollection` (namespace `Speckle.Sdk`)
- rail: sync

| [INDEX] | [SURFACE]       | [CALL_SHAPE]                                                                                                                                                                                               | [CAPABILITY]                 |
| :-----: | :-------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------------- |
|  [01]   | `AddSpeckleSdk` | `IServiceCollection AddSpeckleSdk(this IServiceCollection serviceCollection, SpeckleSdkOptions speckleSdkOptions)`                                                                                         | options-driven register      |
|  [02]   | `AddSpeckleSdk` | `IServiceCollection AddSpeckleSdk(this IServiceCollection serviceCollection, Application application, string applicationVersion, string? speckleVersion = null, IEnumerable<Assembly>? assemblies = null)` | application register         |
|  [03]   | `AddSpeckleSdk` | `IServiceCollection AddSpeckleSdk(this IServiceCollection serviceCollection, Application application, string applicationVersion, string? speckleVersion, params Assembly[] assemblies)`                    | params register              |
|  [04]   | `AddSpeckleSdk` | `IServiceCollection AddSpeckleSdk(this IServiceCollection serviceCollection, Application application, string applicationVersion, params Assembly[] assemblies)`                                            | params register (no version) |

`AddSpeckleSdk` registers `IOperations`, `IClient`, the transport factories, and the serialisation pipeline into the container; the `SpeckleLikeDiff` rail resolves `IOperations` from the wired provider. `Base` member surface: `id` (`string?`, null until the graph is deserialized from a transport), `applicationId` (`string?`), `speckle_type` (`string`, derived type discriminator), `GetId(bool decompose = false)` (`[Obsolete]`; full-serialize content hash matching the `Operations` send path), `GetTotalChildrenCount()`. `Account`: `token`, `refreshToken`, `serverInfo` (`ServerInfo`), `userInfo` (`UserInfo`), `id` (lazy MD5 of `email + url`), `isDefault`, `isOnline` (`[Obsolete]`), `GetHashedEmail()`, `GetHashedServer()`. `Mesh`: `vertices` (`List<double>`, required), `faces` (`List<int>`, required), `colors` (`List<int>`, ARGB), `textureCoordinates` (`List<double>`), `units` (`string`, required); `VerticesCount` is `vertices.Count / 3`.

## [04]-[ERROR_TAXONOMY]

[BOUNDARY_FAULTS]: SDK exception surfaces lifted at the Speckle sync edge
- rail: sync

| [INDEX] | [THROWN]                                                    | [DISCRIMINANT]                             |
| :-----: | :---------------------------------------------------------- | :----------------------------------------- |
|  [01]   | `Speckle.Sdk.SpeckleException`                              | serialization/send failure base fault      |
|  [02]   | `Speckle.Sdk.Transports.TransportException`                 | transport save/copy/retrieve failure       |
|  [03]   | `Speckle.Sdk.Serialisation.SpeckleDeserializeException`     | deserialization of requested object failed |
|  [04]   | `System.Net.Http.HttpRequestException`                      | HTTP-layer server fault                    |
|  [05]   | `System.OperationCanceledException`                         | `cancellationToken` requested cancellation |
|  [06]   | `System.ArgumentNullException` / `System.ArgumentException` | null `value`/`objectId` or no transports   |

## [05]-[CATALOGUE_LAW]

[PACKAGE_SCOPE]:
- Package pages carry external package API facts; the `SyncTransport.SpeckleLikeDiff` case, the `SyncPump.Offer` dispatch, and the `ContentKey` mapping are owned at `Sync/collaboration`.
- The `rootObjId`-to-`ContentKey` projection is owned at `Sync/collaboration`; this catalogue records only that the `Send` tuple's first element (and `SerializeProcessResults.RootId`) is the content hash.
- Codec lane separation: Speckle owns its OWN `Base`-graph serialiser (`SpeckleObjectSerializer`/V2 pipeline, MD5/SHA content hashing) — it does NOT route through the `api-thinktecture-serialization`/`api-messagepack` snapshot codecs, which own the in-process `[ValueObject]`/`[SmartEnum]` snapshot lane. The two are parallel sync/codec rails that meet only at the `SyncTransport` case algebra: a Rasm domain owner is projected to a Speckle `Base`/`DataObject` (or `displayValue` geometry) at the `Sync/collaboration` boundary, then Speckle's own serialiser hashes and stores it. No double-encoding.
- `ServerInfo`/`UserInfo` credential acquisition and the `Account` token lifecycle are connection input handed over by app roots, not a Persistence fence member.
- `Speckle.Sdk` runs OUTSIDE-RHINO; `Speckle.Sdk.Dependencies` repacks the SDK's Polly + channel + serialisation-V2 closure into one assembly, and the in-Rhino assembly composes only the canonical `SyncTransport.SpeckleLikeDiff` case and never references the Speckle assemblies.
