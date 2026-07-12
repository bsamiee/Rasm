# [RASM_BIM_API_GEOMETRY3SHARP]

`geometry3Sharp` supplies pure-managed triangle-mesh-text decode: the `StandardMeshReader` format-dispatching reader over the registered `OBJFormatReader`/`STLFormatReader`/`OFFFormatReader` handlers, the `DMesh3` indexed triangle-mesh carrier with per-vertex normal/color/UV channels, and the `IMeshBuilder`/`DMesh3Builder` accumulation surface, grounding the `Exchange/format#FORMAT_AXIS` `MeshText` codec OBJ/STL/OFF managed-import leg. PLY and 3MF carry no reader in this package.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geometry3Sharp`
- package: `geometry3Sharp`
- assembly: `geometry3Sharp`
- namespace: `g3`
- asset: netstandard2.0, net45 — `net10.0` consumer binds `lib/netstandard2.0`; pure-managed IL-only AnyCPU, ALC-safe
- asset: both TFM dependency groups are empty (zero package dependencies)
- license: gradientspace Boost-style permissive (`licenseUrl` form, no SPDX expression; `requireLicenseAcceptance=false`)
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reader facade and dispatch
- rail: geometry

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [ROLE]                                                                            |
| :-----: | :--------------------- | :--------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `StandardMeshReader`   | reader facade    | extension-dispatched read into an `IMeshBuilder`; default OBJ/STL/OFF/G3 handlers |
|  [02]   | `MeshFormatReader`     | reader interface | per-format handler: `SupportedExtensions` plus `ReadFile` over file or stream     |
|  [03]   | `OBJFormatReader`      | format handler   | OBJ reader; `SupportedExtensions = ["obj"]`                                       |
|  [04]   | `STLFormatReader`      | format handler   | binary and ASCII STL reader; `SupportedExtensions = ["stl"]`                      |
|  [05]   | `OFFFormatReader`      | format handler   | OFF reader; `SupportedExtensions = ["off"]`                                       |
|  [06]   | `BinaryG3FormatReader` | format handler   | native binary-G3 reader; `SupportedExtensions = ["g3mesh"]`                       |

[PUBLIC_TYPE_SCOPE]: mesh model and builder
- rail: geometry

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]    | [ROLE]                                                                                        |
| :-----: | :---------------- | :--------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `DMesh3`          | mutable class    | indexed triangle mesh; refcounted vertex/triangle/edge pools, vtx normals                     |
|  [02]   | `DMesh3Builder`   | builder class    | `IMeshBuilder` accumulator owning `List<DMesh3> Meshes` and material rows                     |
|  [03]   | `IMeshBuilder`    | builder contract | append-mesh/vertex/triangle/material surface the format readers drive                         |
|  [04]   | `NewVertexInfo`   | mutable struct   | per-vertex bundle: position, normal, color, UV with presence flags                            |
|  [05]   | `GenericMaterial` | abstract class   | `name`/`id` base material; the OBJ MTL parse builds `OBJMaterial` when `ReadMaterials` is set |

[PUBLIC_TYPE_SCOPE]: vector and index value types
- rail: geometry

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]  | [FIELDS]                                                                                                                                                                                           |
| :-----: | :--------- | :------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Vector3d` | mutable struct | `double x`, `y`, `z`; the canonical position type                                                                                                                                                  |
|  [02]   | `Vector3f` | mutable struct | `float x`, `y`, `z`; the normal and color type; static anchors `AxisX`/`AxisY`/`AxisZ`/`Zero`/`One` (`AxisZ` the default up-normal a projection substitutes when a mesh carries no normal channel) |
|  [03]   | `Vector2f` | mutable struct | `float x`, `y`; the UV type                                                                                                                                                                        |
|  [04]   | `Index3i`  | mutable struct | `int a`, `b`, `c`; a triangle's three vertex indices                                                                                                                                               |

[PUBLIC_TYPE_SCOPE]: read result and options
- rail: geometry

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [ROLE]                                                               |
| :-----: | :------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `IOReadResult` | struct        | `IOCode code`, `string message`; `IOReadResult.Ok` the success value |
|  [02]   | `IOCode`       | enum          | result status (`Ok=0`, parse/format/access error rows)               |
|  [03]   | `ReadOptions`  | mutable class | `ReadMaterials` flag plus `CustomFlags`; `ReadOptions.Defaults`      |

[IO_CODE_CASES]: `IOCode`
- `Ok = 0`, `FileAccessError = 1`, `UnknownFormatError = 2`, `FormatNotSupportedError = 3`, `InvalidFilenameError = 4`
- `FileParsingError = 100`, `GarbageDataError = 101`, `GenericReaderError = 102`, `GenericReaderWarning = 103`
- `WriterError = 200`, `ComputingInWorkerThread = 1000`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `StandardMeshReader` — read and dispatch
- rail: geometry

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY] | [RAIL]                                           |
| :-----: | :----------------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `StandardMeshReader(bool bIncludeDefaultReaders = true)`                 | constructor    | registers OBJ/STL/OFF/G3 default handlers        |
|  [02]   | `MeshBuilder` property                                                   | builder slot   | the `IMeshBuilder` sink; default `DMesh3Builder` |
|  [03]   | `Read(Stream, string sExtension, ReadOptions)`                           | stream read    | returns `IOReadResult`; in-memory, no file path  |
|  [04]   | `Read(string sFilename, ReadOptions)`                                    | file read      | path-dispatched read; returns `IOReadResult`     |
|  [05]   | `SupportsFormat(string sExtension)`                                      | capability     | `true` when a registered handler claims the ext  |
|  [06]   | `AddFormatHandler(MeshFormatReader)`                                     | registration   | adds a handler; throws on duplicate extension    |
|  [07]   | `StandardMeshReader.ReadMesh(Stream, string sExtension)`                 | static read    | returns first `DMesh3` or `null` on non-`Ok`     |
|  [08]   | `StandardMeshReader.ReadMesh(string sFilename)`                          | static read    | path-overload of [07]; first `DMesh3` or `null`  |
|  [09]   | `StandardMeshReader.ReadFile(Stream, string, ReadOptions, IMeshBuilder)` | static read    | reads into a caller-supplied builder             |
|  [10]   | `StandardMeshReader.ReadFile(string, ReadOptions, IMeshBuilder)`         | static read    | path-overload reading into a supplied builder    |
|  [11]   | `warningEvent` (`ParsingMessagesHandler`)                                | diagnostics    | per-parse warning callback                       |

[ENTRYPOINT_SCOPE]: `MeshFormatReader` — per-format handler
- rail: geometry

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :-------------------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `SupportedExtensions` property                                        | capability     | `List<string>` of bare extensions, no dot |
|  [02]   | `ReadFile(Stream, IMeshBuilder, ReadOptions, ParsingMessagesHandler)` | stream parse   | parses the stream into the builder        |
|  [03]   | `ReadFile(string, IMeshBuilder, ReadOptions, ParsingMessagesHandler)` | file parse     | parses the file into the builder          |

[ENTRYPOINT_SCOPE]: `DMesh3` — mesh read accessors
- rail: geometry

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY]   | [RAIL]                                      |
| :-----: | :-------------------------- | :--------------- | :------------------------------------------ |
|  [01]   | `VertexCount` property      | count            | live (refcounted) vertex count              |
|  [02]   | `TriangleCount` property    | count            | live triangle count                         |
|  [03]   | `HasVertexNormals` property | channel presence | `true` when the normal channel is allocated |
|  [04]   | `GetVertex(int vID)`        | accessor         | returns `Vector3d` position                 |
|  [05]   | `GetVertexf(int vID)`       | accessor         | returns `Vector3f` position                 |
|  [06]   | `GetVertexNormal(int vID)`  | accessor         | returns `Vector3f` normal                   |
|  [07]   | `GetTriangle(int tID)`      | accessor         | returns `Index3i` of three vertex indices   |
|  [08]   | `GetTriNormal(int tID)`     | accessor         | returns `Vector3d` face normal              |
|  [09]   | `VertexIndices()`           | enumerator       | `IEnumerable<int>` over live vertex ids     |
|  [10]   | `TriangleIndices()`         | enumerator       | `IEnumerable<int>` over live triangle ids   |
|  [11]   | `IsVertex(int vID)`         | predicate        | `true` when the slot is a live vertex       |
|  [12]   | `IsTriangle(int tID)`       | predicate        | `true` when the slot is a live triangle     |

[ENTRYPOINT_SCOPE]: `DMesh3Builder` — accumulation
- rail: geometry

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `Meshes` property                            | mesh list      | `List<DMesh3>` the readers populate               |
|  [02]   | `AppendNewMesh(bool, bool, bool, bool)`      | mesh add       | normals/colors/UVs/face-group presence flags      |
|  [03]   | `AppendVertex(double x, double y, double z)` | vertex add     | appends to the active mesh; returns vertex id     |
|  [04]   | `AppendTriangle(int i, int j, int k, int g)` | triangle add   | appends a grouped triangle; returns triangle id   |
|  [05]   | `Materials` property                         | material list  | `List<GenericMaterial>` from the MTL parse        |
|  [06]   | `SupportsMetaData` property                  | capability     | `true`; `AppendMetaData` stores per-mesh metadata |

## [04]-[IMPLEMENTATION_LAW]

[GEOMETRY3SHARP_TOPOLOGY]:
- namespace: `g3`; the import-relevant surface is the `StandardMeshReader` facade, the `MeshFormatReader` handler family, the `DMesh3` carrier, and the `IMeshBuilder`/`DMesh3Builder` accumulator
- `StandardMeshReader` registers `OBJFormatReader`, `STLFormatReader`, `OFFFormatReader`, and `BinaryG3FormatReader` by default; format selection is by case-insensitive bare extension through `SupportedExtensions`
- `STLFormatReader` reads both binary and ASCII STL; `OBJFormatReader` reads OBJ with optional MTL material parse gated on `ReadOptions.ReadMaterials` (the MTL parse materializes `OBJMaterial: GenericMaterial` rows into `DMesh3Builder.Materials`)
- `DMesh3` is a refcounted-pool mesh: `VertexCount`/`TriangleCount` are live counts and the index space is sparse, so projection iterates `VertexIndices()`/`TriangleIndices()` rather than a dense `0..Count` loop; `HasVertexNormals` is `normals != null` on the live carrier (the `IMesh` view returns a constant `false`)
- the carrier is double-precision: `GetVertex` returns `Vector3d`, `GetVertexf` the `Vector3f` position, `GetVertexNormal` returns `Vector3f`, `GetTriangle` returns `Index3i`, `GetTriNormal` returns `Vector3d`
- the package ALSO ships a writer family (`StandardMeshWriter: IDisposable`, `IMeshWriter`, `OBJWriter`/`STLWriter`/`OFFWriter`/`SVGWriter`), but export is OUT of this rail's scope: the canonical mesh-export owner is the Compute glTF `EXPORT_RAIL` (`SharpGLTF` + `Openize.Drako`/meshopt), so this catalog admits only the decode leg
- there is no PLY reader and no 3MF reader in this package; the `MeshText` codec PLY/3MF rows stay codec-pending (no pure-managed RID-safe NuGet reader admits)

[INTEGRATION_STACK]:
- `geometry3Sharp` grounds the managed OBJ/STL/OFF leg of the `MeshText` `InterchangeCodec` at `Exchange/format#FORMAT_AXIS`; it is the import counterpart to the Compute glTF `EXPORT_RAIL`. The dispatch is one codec row keyed by bare extension: `StandardMeshReader.Read(stream, ext, ReadOptions.Defaults)` decodes into a `DMesh3Builder`, then the boundary projects `DMesh3` into the canonical triangle-soup (positions/normals/UVs + index buffer).
- The projected canonical buffer is the SAME shape `Openize.Drako` (`PointAttribute.Wrap`) and `Alimer.Bindings.MeshOptimizer` (`ReadOnlySpan<TVertex>`) consume — so an imported mesh round-trips to a compressed glTF export with no second projection.
- A Compute import rail wraps the decode in the project `Fin`/`Eff` rail: `IOReadResult.code != IOCode.Ok` becomes a typed import failure, and the `warningEvent`/`ParsingMessagesHandler` callbacks fold into the same telemetry span as warning rows (the reader signals via `IOReadResult` + the warning event, never a `Fin`).

[LOCAL_ADMISSION]:
- Stream read: construct `StandardMeshReader` (default handlers), set `MeshBuilder` to a `DMesh3Builder`, call `Read(stream, extension, ReadOptions.Defaults)`, and check `IOReadResult.code == IOCode.Ok`.
- Direct mesh: `StandardMeshReader.ReadMesh(stream, extension)` returns the first `DMesh3` or `null` on a non-`Ok` result.
- Projection: read `mesh.VertexIndices()` for `GetVertex`/`GetVertexNormal` and `mesh.TriangleIndices()` for `GetTriangle`, materializing the canonical triangle-soup vertex/normal/index triple at the boundary.
- Culture: `StandardMeshReader.ReadInvariantCulture` is `true` by default so the float parse is locale-independent.

[RAIL_LAW]:
- Package: `geometry3Sharp`
- Owns: pure-managed OBJ/STL/OFF triangle-mesh-text decode into the `DMesh3` carrier
- Accept: `Stream` input through `StandardMeshReader.Read`/`ReadMesh` with a bare extension discriminant; `IMeshBuilder` sink
- Reject: a hand-rolled STL/OBJ tokenizer, a per-format reader family beside the `MeshFormatReader` dispatch, and any PLY/3MF claim against this package (no such reader exists here)
