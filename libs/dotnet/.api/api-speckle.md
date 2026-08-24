# [RASM_API_SPECKLE]

`Speckle.Sdk` owns the `Base` object-graph model, its dynamic detach/chunk serialisation, the DI-resolved `IOperations` send/receive surface, the transport family, and the GraphQL `IClient`; `Speckle.Objects` layers the geometry roster and the `Speckle.Objects.Data` host-object family onto `Base`. Two folders split one graph: `Rasm.Persistence` owns the SEND half — the serialiser, transports, and client feeding the sync rail's `SyncTransport.SpeckleLikeDiff` case, the send `rootObjId` mapping to `UInt128 ContentKey` through `SyncPump.Offer` — and `Rasm.Bim` owns the RECEIVE half: the deduplicating `Flatten` traversal, the display-mesh geometry, the metre-conversion surface, and the `DataObject` typed-parameter family folding onto the canonical Bim carriers at the exchange import seam. A non-display `Brep`/`Surface`/`Curve` with no `displayValue` hands off to the Compute tessellation companion.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Speckle.Sdk`
- package: `Speckle.Sdk` (Apache-2.0)
- assembly: `Speckle.Sdk`
- companion: `Speckle.Sdk.Dependencies` (transitive; ILRepacks `Polly`, `Open.ChannelExtensions`, `Microsoft.Extensions.ObjectPool`, and the serialisation-V2 send/receive channel pipeline into one assembly)
- transitive: `GraphQL.Client`, `Microsoft.Data.Sqlite` (native `e_sqlite3` via `SQLitePCLRaw`), `System.Text.Json`, `Speckle.Newtonsoft.Json`, `Speckle.DoubleNumerics`
- namespace: `Speckle.Sdk`, `Speckle.Sdk.Api`, `Speckle.Sdk.Models`(`.Extensions`, `.GraphTraversal`), `Speckle.Sdk.Transports`, `Speckle.Sdk.Serialisation`, `Speckle.Sdk.Credentials`, `Speckle.Sdk.Common`
- asset: `net10.0`, `net8.0`, `netstandard2.0`; the net10.0 consumer binds `lib/net10.0` — the host-neutral exchange assembly binds it, never the in-Rhino plugin ALC
- rail: interchange and sync

[PACKAGE_SURFACE]: `Speckle.Objects`
- package: `Speckle.Objects` (Apache-2.0)
- assembly: `Speckle.Objects`
- companion: `Speckle.Sdk` (supplies the `Base`/`ISpeckleObject` base graph)
- namespace: `Speckle.Objects`, `Speckle.Objects.Geometry`, `Speckle.Objects.Data`, `Speckle.Objects.Primitive`, `Speckle.Objects.Other`, `Speckle.Objects.Annotation`
- asset: `net10.0`, `net8.0`, `netstandard2.0`; the net10.0 consumer binds `lib/net10.0`
- rail: interchange and sync

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `Speckle.Sdk` object-graph model and attributes

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------------------ | :------------ | :-------------------------------------------------------- |
|  [01]   | `Base : DynamicBase, ISpeckleObject`  | model root    | dynamic object graph; `id`/`applicationId`/`speckle_type` |
|  [02]   | `ObjectReference : Base`              | reference     | detached-child placeholder in a serialized graph          |
|  [03]   | `DetachPropertyAttribute : Attribute` | attribute     | sealed; `Detachable` (default `true`)                     |
|  [04]   | `ChunkableAttribute : Attribute`      | attribute     | sealed; `MaxObjCountPerChunk` (default `1000`)            |

- `Base`: `id` is null until the graph is deserialized from a transport; `GetTotalChildrenCount()` counts the detachable children and itself.

[PUBLIC_TYPE_SCOPE]: traversal and unit surface (`Speckle.Sdk.Models.Extensions`, `.GraphTraversal`, `.Common`)

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :--------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `BaseExtensions`       | static class  | dedup traversal, display-value, and parameter accessors   |
|  [02]   | `BaseRecursionBreaker` | delegate      | `bool(Base)` descent predicate nested in `BaseExtensions` |
|  [03]   | `TraversalContext`     | class         | walk node reconstructing spatial containment              |
|  [04]   | `Units`                | static class  | unit-string constants and the metre-scaling factor        |
|  [05]   | `IDisplayValue<T>`     | interface     | covariant `displayValue` contract over the payload type   |

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

- `Mesh`: required `vertices` (`List<double>` flat `x,y,z` triples), `faces` (`List<int>` length-prefixed `[n, i0…]` runs), `units` (`string`); optional `colors` (`List<int>` ARGB), `textureCoordinates` (`List<double>`), and `vertexNormals` (`List<double>`, a live normal channel only when `vertexNormals.Count == vertices.Count`); `VerticesCount` is `vertices.Count / 3` and `GetPoint(int)` reads one vertex.

[PUBLIC_TYPE_SCOPE]: `Speckle.Objects.Data` host-object family

Family base `DataObject : Base, IDataObject, IProperties, IDisplayValue<IReadOnlyList<Base>>` seats each host row, which derives it and adds its host marker (`[SYMBOL]` = `: DataObject, I<Host>Object`).

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]    | [CAPABILITY]                                                  |
| :-----: | :------------------- | :--------------- | :------------------------------------------------------------ |
|  [01]   | `DataObject`         | host-object base | `name`/`displayValue`/`properties` carrier                    |
|  [02]   | `RevitObject`        | host-object      | adds `type`/`family`/`category`/`level`/`location`/`elements` |
|  [03]   | `RhinoObject`        | host-object      | Rhino-sourced data object                                     |
|  [04]   | `ArchicadObject`     | host-object      | Archicad-sourced data object                                  |
|  [05]   | `TeklaObject`        | host-object      | adds `type`/`elements`                                        |
|  [06]   | `Civil3dObject`      | host-object      | Civil3D-sourced data object                                   |
|  [07]   | `AutocadObject`      | host-object      | AutoCAD-sourced data object                                   |
|  [08]   | `EtabsObject`        | host-object      | ETABS/CSI-sourced (`ICsiObject`)                              |
|  [09]   | `ArcgisObject`       | host-object      | ArcGIS-sourced (`IGisObject`)                                 |
|  [10]   | `NavisworksObject`   | host-object      | Navisworks-sourced data object                                |
|  [11]   | `MicrostationObject` | host-object      | MicroStation-sourced data object                              |
|  [12]   | `TsdObject`          | host-object      | Tekla Structural Designer host element                        |

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

[ENTRYPOINT_SCOPE]: `BaseExtensions` — traversal and display (statics extending `Base`)

| [INDEX] | [SURFACE]                                      | [SHAPE] | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------- | :------ | :-------------------------------------------------- |
|  [01]   | `Flatten(Base, BaseRecursionBreaker?)`         | static  | `IEnumerable<Base>` dedup walk caching on `Base.id` |
|  [02]   | `Traverse(Base, BaseRecursionBreaker)`         | static  | `IEnumerable<Base>` breaker-gated depth-first walk  |
|  [03]   | `TraverseWithPath(Base, BaseRecursionBreaker)` | static  | `IEnumerable<(string[], Base)>` path-carrying walk  |
|  [04]   | `TryGetDisplayValue(Base)`                     | static  | `IReadOnlyList<Base>?` display-node list            |
|  [05]   | `TryGetDisplayValue<T>(Base)`                  | static  | `IReadOnlyList<T>?` typed list, `T : Base`          |
|  [06]   | `IsDisplayableObject(Base)`                    | static  | `bool`, true when a display value exists            |
|  [07]   | `TryGetName(Base)`                             | static  | `string?` node display name                         |

[ENTRYPOINT_SCOPE]: `Units` — metre conversion

| [INDEX] | [SURFACE]                                | [SHAPE] | [CAPABILITY]                                |
| :-----: | :--------------------------------------- | :------ | :------------------------------------------ |
|  [01]   | `Meters`                                 | const   | `"m"` canonical kernel-frame target unit    |
|  [02]   | `Millimeters`/`Centimeters`/`Kilometers` | const   | `"mm"`/`"cm"`/`"km"` metric tokens          |
|  [03]   | `Inches`/`Feet`/`Yards`/`Miles`          | const   | `"in"`/`"ft"`/`"yd"`/`"mi"` imperial tokens |
|  [04]   | `GetConversionFactor(string?, string?)`  | static  | `double` source-to-target metre scale       |
|  [05]   | `IsUnitSupported(string)`                | static  | `bool` recognized-unit gate                 |

[ENTRYPOINT_SCOPE]: `TraversalContext` — walk node

| [INDEX] | [SURFACE]  | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :--------- | :------- | :-------------------------------------------------- |
|  [01]   | `Current`  | property | `Base` node at this walk position                   |
|  [02]   | `Parent`   | property | `TraversalContext?` containment-reconstruction link |
|  [03]   | `PropName` | property | `string?` member name the parent reached through    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Base` is a `DynamicBase`; a seam reads typed members through the package surface and never reflects the dynamic bag; host subtypes add typed columns over the one inherited `properties` dictionary.
- `Flatten` is the sole deduplicating traversal, caching on `Base.id`; a hand-rolled `DynamicBase` recursion is the rejected form. `TryGetDisplayValue`/`IsDisplayableObject` own the displayable-node vocabulary — a per-type `is Mesh`/`is Brep` ladder is the rejected form.
- `Mesh.faces` fans each length-prefixed n-gon to a triangle fan at the boundary; `Brep` ships no managed NURBS evaluator, so a non-mesh `Brep`/`Surface`/`Curve` lacking `displayValue` routes to the tessellation companion.
- `IOperations` resolves from DI; `Operations` declares no static `Send`/`Receive`, so the `SpeckleLikeDiff` rail binds the instance surface alone. Transport-bound `Send`/`Receive` drive the explicit transport stack; `Send2`/`Receive2` run the URL-bound serialisation-V2 pipeline that bypasses it.
- `Send`/`Receive` lift `ArgumentException`/`ArgumentNullException` on a null graph, missing `objectId`, or empty transport set, `OperationCanceledException` on the token, and `HttpRequestException` at the HTTP layer.

[STACKING]:
- `dotbim`(`Rasm.Bim/.api/api-dotbim.md`), `SharpGLTF`(`api-sharpgltf.md`), `AssimpNetter`(`Rasm.Bim/.api/api-assimpnetter.md`): every display `Mesh` fans to the shared canonical triangle carrier these codecs decode into, so a received Speckle model re-exports through any of them.
- `Thinktecture.Json`/`MessagePack`(`api-thinktecture-json.md`, `api-thinktecture-messagepack.md`, `api-messagepack.md`): parallel codec rails, never composed inline. Speckle owns its own `Base`-graph serialiser (`SpeckleObjectSerializer`, the V2 pipeline, content hashing) and never routes through the snapshot codecs; a Rasm owner marshals to a Speckle `Base`/`DataObject` (or `displayValue` geometry) at the `Version/ledger#SYNC_TRANSPORTS` `SpeckleSend` seam, then Speckle's serialiser hashes and stores it — no double-encoding.
- Persistence consumer anchor: the sync rail composes `Send` over a `ServerTransport` + `SQLiteTransport` pair for remote-plus-local-cache, or `Send2` for the URL pipeline; `SyncPump.Offer` maps the resulting `rootObjId` to the existing `UInt128 ContentKey`. This half never re-projects a received graph.
- Bim consumer anchor: the `Exchange/import` seam runs `root.Flatten()` split to `OfType<Mesh>` geometry and `OfType<DataObject>` semantics onto the canonical carriers, `Units.GetConversionFactor(mesh.units, Units.Meters)` scaling every mesh to the kernel metre frame; a `displayValue`-less `Brep`/`Surface`/`Curve` hands to the Compute tessellation companion; containment reconstructs by walking the `TraversalContext.Parent` chain. This half mints no transport.

[LOCAL_ADMISSION]:
- `Speckle.Sdk`/`Speckle.Objects` run OUTSIDE-RHINO on the companion target; the in-Rhino assembly composes only the `SyncTransport.SpeckleLikeDiff` case and never references the Speckle assemblies. `Speckle.Sdk.Dependencies` repacks the Polly + channel + object-pool + serialisation-V2 closure so the SDK dependency graph stays isolated from the host.
- `ServerInfo`/`UserInfo` acquisition and the `Account` token lifecycle are connection input from app roots, not a fence member.
- `Version/ledger#SYNC_TRANSPORTS` owns the `rootObjId`→`ContentKey` projection, gated by the `SyncFault.SpeckleMarshal` drift fault; this catalog records only that the send tuple's first element and `SerializeProcessResults.RootId` are the content hash.
- Bim display fold: `root.Flatten` → per-node `TryGetDisplayValue?.OfType<Mesh>` → fan `faces` n-gons to triangles → scale by `Units.GetConversionFactor(mesh.units, Units.Meters)`; semantic fold: `root.Flatten().OfType<DataObject>()` → project `name`/`speckle_type`/`applicationId`, flatten `properties` to typed parameter rows.

[RAIL_LAW]:
- Packages: `Speckle.Sdk`, `Speckle.Objects`
- Owns: the `Base` object-graph model, its detach/chunk serialisation, the DI-resolved send/receive/transport surface feeding the `SpeckleLikeDiff` sync case, the dedup traversal, the display-mesh geometry, and the host-object semantic family
- Accept: instance `IOperations.Send`/`Receive`/`Send2` off the wired provider over a declared transport set; a Rasm owner marshalled to `Base`/`DataObject` at the sync seam; an already-deserialized `Base` root read through the package `Flatten`/`TryGetDisplayValue` surface
- Reject: `static Operations.Send`, a hand-rolled `Base`-graph serialiser beside `SpeckleObjectSerializer`, a snapshot-codec payload double-encoded through Speckle, a hand-rolled `Base`-graph recursion, a per-type display ladder, a managed Speckle BRep tessellator, and a second `IOperations.Receive` in the import seam
