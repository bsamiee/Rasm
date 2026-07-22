# [RASM_PERSISTENCE_API_SPECKLE]

`Speckle.Sdk` owns the `Base` object-graph model, its dynamic detach/chunk serialisation, the DI-resolved `IOperations` send/receive surface, the transport family, and the GraphQL `IClient`; `Speckle.Objects` layers the geometry roster and the `Speckle.Objects.Data` host-object family onto `Base`. This surface feeds the Persistence sync rail's `SyncTransport.SpeckleLikeDiff` case: the send `rootObjId` is the content hash mapping to `UInt128 ContentKey`, dispatched through `SyncPump.Offer`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Speckle.Sdk`
- package: `Speckle.Sdk`
- assembly: `Speckle.Sdk`
- companion: `Speckle.Sdk.Dependencies` (transitive; ILRepacks `Polly`, `Open.ChannelExtensions`, `Microsoft.Extensions.ObjectPool`, and the serialisation-V2 send/receive channel pipeline into one assembly)
- transitive: `GraphQL.Client`, `Microsoft.Data.Sqlite` (native `e_sqlite3` via `SQLitePCLRaw`), `System.Text.Json`, `Speckle.Newtonsoft.Json`, `Speckle.DoubleNumerics`
- namespace: `Speckle.Sdk`, `Speckle.Sdk.Api`, `Speckle.Sdk.Models`, `Speckle.Sdk.Transports`, `Speckle.Sdk.Serialisation`, `Speckle.Sdk.Credentials`
- target frameworks: `net10.0`, `net8.0`, `netstandard2.0`
- asset: runtime library
- rail: sync

[PACKAGE_SURFACE]: `Speckle.Objects`
- package: `Speckle.Objects`
- assembly: `Speckle.Objects`
- companion: `Speckle.Sdk` (supplies the `Base`/`ISpeckleObject` base graph)
- namespace: `Speckle.Objects`, `Speckle.Objects.Geometry`, `Speckle.Objects.Data`, `Speckle.Objects.Primitive`, `Speckle.Objects.Other`, `Speckle.Objects.Annotation`
- target frameworks: `net10.0`, `net8.0`, `netstandard2.0`
- asset: runtime library
- rail: sync

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `Speckle.Sdk` object-graph model and attributes

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------------------ | :------------ | :-------------------------------------------------------- |
|  [01]   | `Base : DynamicBase, ISpeckleObject`  | model root    | dynamic object graph; `id`/`applicationId`/`speckle_type` |
|  [02]   | `ObjectReference : Base`              | reference     | detached-child placeholder in a serialized graph          |
|  [03]   | `DetachPropertyAttribute : Attribute` | attribute     | sealed; `Detachable` (default `true`)                     |
|  [04]   | `ChunkableAttribute : Attribute`      | attribute     | sealed; `MaxObjCountPerChunk` (default `1000`)            |

- `Base`: `id` is null until the graph is deserialized from a transport; `GetTotalChildrenCount()` counts the detachable children and itself.

[PUBLIC_TYPE_SCOPE]: `Speckle.Sdk` operations, client, transports

Each concrete transport is `sealed : ITransport, IBlobCapableTransport, ICloneable`; the `[SYMBOL]` cell carries only its distinguishing base.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]       | [CAPABILITY]                                                      |
| :-----: | :---------------------------------------- | :------------------ | :---------------------------------------------------------------- |
|  [01]   | `IOperations`                             | operations contract | INTERFACE; DI-resolved `Send`/`Receive`/`Serialize`               |
|  [02]   | `Operations : IOperations`                | operations impl     | INSTANCE class; DI primary-ctor; no static `Send`/`Receive`       |
|  [03]   | `IClient : IDisposable`                   | client contract     | GraphQL resources, `Account`, `ServerUrl`, `GQLClient`            |
|  [04]   | `Client : ISpeckleGraphQLClient, IClient` | client impl         | sealed; DI-constructed GraphQL client                             |
|  [05]   | `ITransport`                              | transport contract  | `SaveObject`/`GetObject`/`CopyObjectAndChildren`/`HasObjects`     |
|  [06]   | `IServerTransport : ITransport`           | server contract     | server-bound transport marker                                     |
|  [07]   | `ServerTransport : IServerTransport`      | transport (server)  | sealed; remote server object store                                |
|  [08]   | `SQLiteTransport`                         | transport (local)   | sealed; default local SQLite cache                                |
|  [09]   | `MemoryTransport`                         | transport (memory)  | sealed; in-process object store                                   |
|  [10]   | `ProgressArgs`                            | progress value      | `readonly record struct (ProgressEvent, long Count, long? Total)` |

[PUBLIC_TYPE_SCOPE]: `Speckle.Sdk` serialisation, credentials, DI

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :-------------------------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `SpeckleObjectSerializer`                           | serializer    | `Serialize(Base)` to JSON over write transports                   |
|  [02]   | `SpeckleObjectDeserializer`                         | deserializer  | sealed; `DeserializeAsync(string?)` to `Base`                     |
|  [03]   | `Account : IEquatable<Account>`                     | credential    | `token`/`refreshToken`/`serverInfo`/`userInfo`/`id`               |
|  [04]   | `ServiceRegistration`                               | DI extensions | static; hosts `AddSpeckleSdk` on `IServiceCollection`             |
|  [05]   | `Application` (`record (string Name, string Slug)`) | DI input      | host-application identity for registration                        |
|  [06]   | `SpeckleSdkOptions` (`record`)                      | DI input      | `(Application, ApplicationVersion, SpeckleVersion?, Assemblies?)` |

- `Account`: `id` is a lazy MD5 of `email + url`; `serverInfo` (`ServerInfo`), `userInfo` (`UserInfo`), and `isDefault` ride the record, and `GetHashedEmail()`/`GetHashedServer()` hash the credentials.

[PUBLIC_TYPE_SCOPE]: `Speckle.Sdk` boundary fault types

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :----------------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `SpeckleException : Exception` | exception     | serialize/send failure base fault             |
|  [02]   | `TransportException`           | exception     | `: SpeckleException`; save/copy/retrieve      |
|  [03]   | `SpeckleDeserializeException`  | exception     | `: SpeckleException`; requested-object decode |

[PUBLIC_TYPE_SCOPE]: `Speckle.Objects.Geometry` roster

Every roster type derives `Base` except `BrepX : RawEncodedObject`; the `[SYMBOL]` cell drops the shared `: Base`.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                                                     |
| :-----: | :----------- | :------------ | :----------------------------------------------------------------------------------------------- |
|  [01]   | `Point`      | geometry      | `ITransformable<Point>`, `IEquatable<Point>`                                                     |
|  [02]   | `Vector`     | geometry      | `IHasBoundingBox`, `ITransformable<Vector>`                                                      |
|  [03]   | `Plane`      | geometry      | `ITransformable<Plane>`                                                                          |
|  [04]   | `Line`       | curve         | `ICurve`, `IHasBoundingBox`, `ITransformable<Line>`                                              |
|  [05]   | `Polyline`   | curve         | `ICurve`, `IHasArea`, `IHasBoundingBox`, `ITransformable`                                        |
|  [06]   | `Arc`        | curve         | `ICurve`, `ITransformable<Arc>`                                                                  |
|  [07]   | `Circle`     | curve         | `ICurve`, `IHasArea`, `IHasBoundingBox`                                                          |
|  [08]   | `Ellipse`    | curve         | `ICurve`, `IHasArea`                                                                             |
|  [09]   | `Curve`      | curve         | `ICurve`, `ITransformable<Curve>`, `IDisplayValue<Polyline>`                                     |
|  [10]   | `Polycurve`  | curve         | `ICurve`, `IHasArea`, `IHasBoundingBox`, `ITransformable`                                        |
|  [11]   | `Spiral`     | curve         | `ICurve`, `IDisplayValue<Polyline>`                                                              |
|  [12]   | `Mesh`       | geometry      | `IHasBoundingBox`, `IHasVolume`, `IHasArea`, `ITransformable<Mesh>`                              |
|  [13]   | `Brep`       | geometry      | `IHasArea`, `IHasVolume`, `IHasBoundingBox`, `ITransformable<Brep>`, `IDisplayValue<List<Mesh>>` |
|  [14]   | `BrepX`      | geometry      | `: RawEncodedObject`; raw-encoded brep payload                                                   |
|  [15]   | `Surface`    | geometry      | `IHasArea`, `ITransformable<Surface>`                                                            |
|  [16]   | `Box`        | geometry      | `IHasVolume`, `IHasArea`, `IHasBoundingBox`                                                      |
|  [17]   | `Pointcloud` | geometry      | `IHasBoundingBox`, `ITransformable<Pointcloud>`                                                  |

- `Mesh`: required `vertices` (`List<double>`), `faces` (`List<int>`), `units` (`string`); optional `colors` (`List<int>` ARGB) and `textureCoordinates` (`List<double>`); `VerticesCount` is `vertices.Count / 3`.

[PUBLIC_TYPE_SCOPE]: `Speckle.Objects.Data` host-object family

Family base `DataObject : Base, IDataObject, IProperties, IDisplayValue<IReadOnlyList<Base>>` seats each host row, which derives it and adds its host marker (`[SYMBOL]` = `: DataObject, I<Host>Object`).

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]    | [CAPABILITY]                               |
| :-----: | :------------------- | :--------------- | :----------------------------------------- |
|  [01]   | `DataObject`         | host-object base | `name`/`displayValue`/`properties` carrier |
|  [02]   | `RevitObject`        | host-object      | Revit-sourced data object                  |
|  [03]   | `RhinoObject`        | host-object      | Rhino-sourced data object                  |
|  [04]   | `ArchicadObject`     | host-object      | Archicad-sourced data object               |
|  [05]   | `TeklaObject`        | host-object      | Tekla-sourced data object                  |
|  [06]   | `Civil3dObject`      | host-object      | Civil3D-sourced data object                |
|  [07]   | `AutocadObject`      | host-object      | AutoCAD-sourced data object                |
|  [08]   | `EtabsObject`        | host-object      | ETABS/CSI-sourced (`ICsiObject`)           |
|  [09]   | `ArcgisObject`       | host-object      | ArcGIS-sourced (`IGisObject`)              |
|  [10]   | `NavisworksObject`   | host-object      | Navisworks-sourced data object             |
|  [11]   | `MicrostationObject` | host-object      | MicroStation-sourced data object           |
|  [12]   | `TsdObject`          | host-object      | TSD-sourced data object                    |

`Speckle.Objects.Data` is the sole host-object roster; built-element geometry rides `DataObject.displayValue` as `List<Base>` (`IDisplayValue<IReadOnlyList<Base>>`), distinct from `Brep.displayValue` (`List<Mesh>`). `IDisplayValue<out T>` is the generic display-value contract.

## [03]-[ENTRYPOINTS]

[SPECKLE_SYNC]: `IOperations` send/receive over the DI-resolved surface

Every member is instance, resolved from DI off the wired provider, and trails `IProgress<ProgressArgs>?` progress and a `CancellationToken`.

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `IOperations.Send(Base, IServerTransport, bool) -> (rootObjId, refs)`      | server-store send with cache flag     |
|  [02]   | `IOperations.Send(Base, ITransport, bool) -> (rootObjId, refs)`            | single-transport send with cache flag |
|  [03]   | `IOperations.Send(Base, IReadOnlyCollection<ITransport>)`                  | multi-transport send, no local cache  |
|  [04]   | `IOperations.Receive(string, ITransport?, ITransport?) -> Base`            | local-then-remote receive             |
|  [05]   | `IOperations.Send2(Uri, string, string?, Base) -> SerializeProcessResults` | URL-bound V2 send pipeline            |
|  [06]   | `IOperations.Receive2(Uri, string, string, string?) -> Base`               | URL-bound V2 receive pipeline         |
|  [07]   | `IOperations.Serialize(Base) -> string`                                    | object graph to JSON                  |
|  [08]   | `IOperations.SerializeNew(Base) -> string`                                 | V2 System.Text.Json serialize         |
|  [09]   | `IOperations.DeserializeAsync(string) -> Base`                             | JSON to `Base`                        |

- `Send2`/`Receive2` gate the V2 pipeline through `SerializeProcessOptions(SkipCacheRead, SkipCacheWrite, SkipServer, SkipFindTotalObjects)` with settable `MaxHttpSendBatchSize`/`MaxCacheBatchSize`/`MaxParallelism`, and `DeserializeProcessOptions(SkipCache, ThrowOnMissingReferences, SkipInvalidConverts, MaxParallelism, SkipServer)`.
- `SerializeProcessResults` is a `readonly record struct (string RootId, IReadOnlyDictionary<Id, ObjectReference> ConvertedReferences)`: `RootId` is the send content hash, and the reference map keys on the Speckle `Id` value type, never `string`.

[SPECKLE_TRANSPORT]: transport and serializer construction

| [INDEX] | [SURFACE]                                                                           | [SHAPE]  | [CAPABILITY]               |
| :-----: | :---------------------------------------------------------------------------------- | :------- | :------------------------- |
|  [01]   | `ServerTransport(ISpeckleHttp, ISdkActivityFactory, Account, string, int, string?)` | ctor     | remote server object store |
|  [02]   | `SQLiteTransport(string?, string?, string?)`                                        | ctor     | default local SQLite cache |
|  [03]   | `MemoryTransport(ConcurrentDictionary?, bool, string?, string?)`                    | ctor     | in-process object store    |
|  [04]   | `SpeckleObjectSerializer(IReadOnlyCollection<ITransport>, IProgress?, bool)`        | ctor     | write-transport serializer |
|  [05]   | `SpeckleObjectSerializer.Serialize(Base) -> string`                                 | instance | serialize graph to JSON    |
|  [06]   | `SpeckleObjectDeserializer.DeserializeAsync(string?) -> ValueTask<Base>`            | instance | deserialize JSON to `Base` |

[SPECKLE_DI]: `AddSpeckleSdk` registration on `IServiceCollection` (namespace `Speckle.Sdk`)

Every overload is `IServiceCollection AddSpeckleSdk(this IServiceCollection, …)`; the application overloads lead with `Application application, string applicationVersion`, and the `[SURFACE]` cell carries only the distinguishing trailing parameters.

| [INDEX] | [SURFACE]                                                                 | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------------ | :--------------------------- |
|  [01]   | `SpeckleSdkOptions speckleSdkOptions`                                     | options-driven register      |
|  [02]   | `string? speckleVersion = null, IEnumerable<Assembly>? assemblies = null` | application register         |
|  [03]   | `string? speckleVersion, params Assembly[] assemblies`                    | params register              |
|  [04]   | `params Assembly[] assemblies`                                            | params register (no version) |

- `AddSpeckleSdk` registers `IOperations`, `IClient`, the transport factories, and the serialisation pipeline; the `SpeckleLikeDiff` rail resolves `IOperations` from the wired provider.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `IOperations` resolves from DI; `Operations` declares no static `Send`/`Receive`, so the `SpeckleLikeDiff` rail binds the instance surface alone.
- Transport-bound `Send`/`Receive` drive the explicit transport stack; `Send2`/`Receive2` run the URL-bound serialisation-V2 pipeline that bypasses it.
- `Send`/`Receive` lift `ArgumentException`/`ArgumentNullException` on a null graph, missing `objectId`, or empty transport set, `OperationCanceledException` on the token, and `HttpRequestException` at the HTTP layer.

[STACKING]:
- `api-thinktecture-serialization`/`api-messagepack`: parallel codec rails, never composed inline. Speckle owns its own `Base`-graph serialiser (`SpeckleObjectSerializer`, the V2 pipeline, content hashing) and never routes through the snapshot codecs; a Rasm owner marshals to a Speckle `Base`/`DataObject` (or `displayValue` geometry) at the `Version/ledger#SYNC_TRANSPORTS` `SpeckleSend` seam, then Speckle's serialiser hashes and stores it — no double-encoding.
- within-lib: the rail composes `Send` over a `ServerTransport` + `SQLiteTransport` pair for remote-plus-local-cache, or `Send2` for the URL pipeline; `SyncPump.Offer` maps the resulting `rootObjId` to the existing `UInt128 ContentKey`.

[LOCAL_ADMISSION]:
- `Speckle.Sdk`/`Speckle.Objects` run OUTSIDE-RHINO on the companion target; the in-Rhino assembly composes only the `SyncTransport.SpeckleLikeDiff` case and never references the Speckle assemblies. `Speckle.Sdk.Dependencies` repacks the Polly + channel + object-pool + serialisation-V2 closure so the SDK dependency graph stays isolated from the host.
- `ServerInfo`/`UserInfo` acquisition and the `Account` token lifecycle are connection input from app roots, not a Persistence fence member.
- `Version/ledger#SYNC_TRANSPORTS` owns the `rootObjId`→`ContentKey` projection, gated by the `SyncFault.SpeckleMarshal` drift fault; this catalog records only that the send tuple's first element and `SerializeProcessResults.RootId` are the content hash.

[RAIL_LAW]:
- Packages: `Speckle.Sdk`, `Speckle.Objects`
- Owns: the `Base` object-graph model, its detach/chunk serialisation, and the DI-resolved send/receive/transport surface feeding the `SpeckleLikeDiff` sync case
- Accept: instance `IOperations.Send`/`Receive`/`Send2` off the wired provider over a declared transport set; a Rasm owner marshalled to `Base`/`DataObject` at the sync seam
- Reject: `static Operations.Send`, a hand-rolled `Base`-graph serialiser beside `SpeckleObjectSerializer`, a snapshot-codec payload double-encoded through Speckle
