# [RASM_BIM_API_GEOMETRY3SHARP]

`geometry3Sharp` owns pure-managed triangle-mesh-text decode: the `StandardMeshReader` extension-dispatching facade over its per-format `MeshFormatReader` handlers, the `DMesh3` refcounted indexed-mesh carrier with per-vertex normal/color/UV channels, and the `IMeshBuilder`/`DMesh3Builder` accumulator the readers drive. It grounds the OBJ/STL/OFF managed-import leg of the `MeshText` interchange codec; PLY and 3MF carry no reader here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geometry3Sharp`
- package: `geometry3Sharp` (Boost-1.0)
- assembly: `geometry3Sharp`
- namespace: `g3`
- asset: `netstandard2.0`/`net45`; `net10.0` binds `lib/netstandard2.0`; pure-managed AnyCPU IL, ALC-safe, zero package dependencies
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reader facade and dispatch

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :--------------------- | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `StandardMeshReader`   | class         | extension-dispatched read into an `IMeshBuilder`; OBJ/STL/OFF/G3 by default   |
|  [02]   | `MeshFormatReader`     | interface     | per-format handler: `SupportedExtensions` plus `ReadFile` over file or stream |
|  [03]   | `OBJFormatReader`      | class         | OBJ reader; `SupportedExtensions = ["obj"]`                                   |
|  [04]   | `STLFormatReader`      | class         | binary and ASCII STL reader; `["stl"]`                                        |
|  [05]   | `OFFFormatReader`      | class         | OFF reader; `["off"]`                                                         |
|  [06]   | `BinaryG3FormatReader` | class         | native binary-G3 reader; `["g3mesh"]`                                         |

[PUBLIC_TYPE_SCOPE]: mesh model and builder

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]  | [CAPABILITY]                                                                   |
| :-----: | :---------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `DMesh3`          | class          | indexed triangle mesh; refcounted vertex/triangle/edge pools, vtx normals      |
|  [02]   | `DMesh3Builder`   | class          | `IMeshBuilder` accumulator owning `List<DMesh3> Meshes` and material rows      |
|  [03]   | `IMeshBuilder`    | interface      | append-mesh/vertex/triangle/material surface the format readers drive          |
|  [04]   | `NewVertexInfo`   | mutable struct | per-vertex bundle: position, normal, color, UV with presence flags             |
|  [05]   | `GenericMaterial` | abstract class | `name`/`id` base; the OBJ MTL parse builds `OBJMaterial` under `ReadMaterials` |

[PUBLIC_TYPE_SCOPE]: vector and index value types

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]  | [CAPABILITY]                                                                         |
| :-----: | :--------- | :------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `Vector3d` | mutable struct | `double x`, `y`, `z`; the canonical position type                                    |
|  [02]   | `Vector3f` | mutable struct | `float x`, `y`, `z`; normal/color type; `AxisX`/`AxisY`/`AxisZ`/`Zero`/`One` anchors |
|  [03]   | `Vector2f` | mutable struct | `float x`, `y`; the UV type                                                          |
|  [04]   | `Index3i`  | mutable struct | `int a`, `b`, `c`; a triangle's three vertex indices                                 |

- `Vector3f.AxisZ`: default up-normal a projection substitutes when a mesh carries no normal channel.

[PUBLIC_TYPE_SCOPE]: read result and options

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `IOReadResult` | struct        | `IOCode code`, `string message`; `IOReadResult.Ok` the success value            |
|  [02]   | `IOCode`       | enum          | result status; `Ok=0` plus parse/format/access error rows                       |
|  [03]   | `ReadOptions`  | class         | `ReadMaterials` flag, `CustomFlags`; `ReadOptions.Defaults` reads geometry only |

[IO_CODE_CASES]: `IOCode`
- `Ok=0` `FileAccessError=1` `UnknownFormatError=2` `FormatNotSupportedError=3` `InvalidFilenameError=4`
- `FileParsingError=100` `GarbageDataError=101` `GenericReaderError=102` `GenericReaderWarning=103` `WriterError=200` `ComputingInWorkerThread=1000`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `StandardMeshReader` — read and dispatch

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `StandardMeshReader(bool)`                            | ctor     | registers OBJ/STL/OFF/G3 handlers by default         |
|  [02]   | `MeshBuilder`                                         | property | the `IMeshBuilder` sink; defaults to `DMesh3Builder` |
|  [03]   | `Read(Stream, string, ReadOptions)`                   | instance | in-memory read; returns `IOReadResult`               |
|  [04]   | `Read(string, ReadOptions)`                           | instance | path-dispatched read; returns `IOReadResult`         |
|  [05]   | `SupportsFormat(string)`                              | instance | `true` when a registered handler claims the ext      |
|  [06]   | `AddFormatHandler(MeshFormatReader)`                  | instance | adds a handler; throws on duplicate extension        |
|  [07]   | `ReadMesh(Stream, string)`                            | static   | first `DMesh3` or `null` on non-`Ok`                 |
|  [08]   | `ReadMesh(string)`                                    | static   | path overload; first `DMesh3` or `null`              |
|  [09]   | `ReadFile(Stream, string, ReadOptions, IMeshBuilder)` | static   | reads into a caller-supplied builder                 |
|  [10]   | `ReadFile(string, ReadOptions, IMeshBuilder)`         | static   | path overload into a supplied builder                |
|  [11]   | `warningEvent` (`ParsingMessagesHandler`)             | event    | per-parse warning callback                           |

[ENTRYPOINT_SCOPE]: `MeshFormatReader` — per-format handler

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `SupportedExtensions`                                                 | property | `List<string>` of bare extensions, no dot |
|  [02]   | `ReadFile(Stream, IMeshBuilder, ReadOptions, ParsingMessagesHandler)` | instance | parses the stream into the builder        |
|  [03]   | `ReadFile(string, IMeshBuilder, ReadOptions, ParsingMessagesHandler)` | instance | parses the file into the builder          |

[ENTRYPOINT_SCOPE]: `DMesh3` — mesh read accessors

| [INDEX] | [SURFACE]              | [SHAPE]  | [CAPABILITY]                                                    |
| :-----: | :--------------------- | :------- | :-------------------------------------------------------------- |
|  [01]   | `VertexCount`          | property | live refcounted vertex count                                    |
|  [02]   | `TriangleCount`        | property | live triangle count                                             |
|  [03]   | `HasVertexNormals`     | property | `true` when the normal channel is allocated (`normals != null`) |
|  [04]   | `GetVertex(int)`       | instance | `Vector3d` position                                             |
|  [05]   | `GetVertexf(int)`      | instance | `Vector3f` position                                             |
|  [06]   | `GetVertexNormal(int)` | instance | `Vector3f` normal                                               |
|  [07]   | `GetTriangle(int)`     | instance | `Index3i` of three vertex indices                               |
|  [08]   | `GetTriNormal(int)`    | instance | `Vector3d` face normal                                          |
|  [09]   | `VertexIndices()`      | instance | `IEnumerable<int>` over live vertex ids                         |
|  [10]   | `TriangleIndices()`    | instance | `IEnumerable<int>` over live triangle ids                       |
|  [11]   | `IsVertex(int)`        | instance | `true` when the slot is a live vertex                           |
|  [12]   | `IsTriangle(int)`      | instance | `true` when the slot is a live triangle                         |

[ENTRYPOINT_SCOPE]: `DMesh3Builder` — accumulation

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                                                      |
| :-----: | :-------------------------------------- | :------- | :---------------------------------------------------------------- |
|  [01]   | `Meshes`                                | property | `List<DMesh3>` the readers populate                               |
|  [02]   | `AppendNewMesh(bool, bool, bool, bool)` | instance | normals/colors/UVs/face-group presence flags                      |
|  [03]   | `AppendVertex(double, double, double)`  | instance | appends to the active mesh; returns vertex id                     |
|  [04]   | `AppendTriangle(int, int, int, int)`    | instance | appends a grouped triangle; returns triangle id                   |
|  [05]   | `Materials`                             | property | `List<GenericMaterial>` from the MTL parse                        |
|  [06]   | `SupportsMetaData`                      | property | `true`; `AppendMetaData(string, object)` stores per-mesh metadata |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `StandardMeshReader` registers `OBJFormatReader`, `STLFormatReader`, `OFFFormatReader`, and `BinaryG3FormatReader` by default, dispatching on case-insensitive bare extension through each handler's `SupportedExtensions`; `AddFormatHandler` throws on a duplicate extension.
- `STLFormatReader` reads binary and ASCII STL; `OBJFormatReader` materializes MTL rows as `OBJMaterial : GenericMaterial` into `DMesh3Builder.Materials` only under `ReadOptions.ReadMaterials`.
- `DMesh3` pools are refcounted and sparse, so `VertexCount`/`TriangleCount` are live counts and projection iterates `VertexIndices()`/`TriangleIndices()`, never a dense `0..Count` loop; `HasVertexNormals` reads `normals != null` on the live carrier while the `IMesh` view returns constant `false`.

[STACKING]:
- `Openize.Drako`(`.api/api-openize-drako.md`): the boundary's projected vertex/normal/index triple loads `PointAttribute.Wrap(AttributeType, Span<Vector3>)` as the `KHR_draco_mesh_compression` encode intake, with no second projection.
- `Alimer.Bindings.MeshOptimizer`(`.api/api-alimer-meshoptimizer.md`): the same triple passes as `ReadOnlySpan<TVertex>` into the meshopt remap/optimize and `EncodeVertexBuffer<TVertex>` compression leg.
- `SharpGLTF`(`.api/api-sharpgltf.md`): a decoded `DMesh3` re-emits through the glTF schema and toolkit builders as the export counterpart to this decode leg.
- within-lib: `StandardMeshReader.Read(Stream, string, ReadOptions.Defaults)` folds one `MeshText` `InterchangeCodec` row keyed by bare extension into a `DMesh3Builder`, then the import boundary projects `DMesh3` to the canonical triangle-soup and wraps `IOReadResult.code != IOCode.Ok` and the `warningEvent` callbacks onto the project `Fin`/`Eff` telemetry rail.

[LOCAL_ADMISSION]:
- Read through `StandardMeshReader.Read(stream, extension, ReadOptions.Defaults)` into a `DMesh3Builder`, gating on `IOReadResult.code == IOCode.Ok`; `ReadMesh(stream, extension)` returns the first `DMesh3` or `null` for a one-shot, and projection iterates `VertexIndices()`/`TriangleIndices()`.
- `ReadInvariantCulture` defaults `true`, so the float parse stays locale-independent.

[RAIL_LAW]:
- Package: `geometry3Sharp`
- Owns: pure-managed OBJ/STL/OFF/G3 triangle-mesh-text decode into the `DMesh3` carrier
- Accept: `Stream` or file input through `StandardMeshReader.Read`/`ReadMesh` keyed by a bare-extension discriminant, driving an `IMeshBuilder` sink
- Reject: a hand-rolled STL/OBJ tokenizer, a per-format reader family beside the `MeshFormatReader` dispatch, and the in-package writer family, whose export leg the glTF rail owns
