# [RASM_FABRICATION_API_LIB3MF]

`lib3mf` supplies the 3MF Consortium reference reader and writer for core, production, beam-lattice, and slice documents. ACT-generated C# bindings expose `Lib3MF.Wrapper`, `CModel`, mesh/component/material/slice resources, `CMeshObject.BeamLattice()`, `AddBuildItem`, and `QueryWriter("3mf")`/`QueryReader("3mf")`. `Internal.Lib3MFWrapper` P/Invokes the native core; vendored binding source and RID-keyed native assets own the package because no official NuGet artifact exists.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lib3mf`
- package: `lib3mf` (vendored — no NuGet artifact; the ACT binding source stages compile-excluded at `vendor/sdk/Lib3MF.cs` and compiles into `Rasm.Fabrication` at realization, the RID-keyed natives ride `vendor/runtimes` through the folder `.csproj`'s `Exists`-gated `Content` group, LFS-carried, outside NuGet restore)
- license: `BSD-2-Clause`
- assembly: the vendored ACT-generated `Lib3MF.cs` compiled into `Rasm.Fabrication`; the native core is a RID-keyed `lib3mf` shared library
- namespace: `Lib3MF` — the `C…`-prefixed handle wrappers (`CModel`/`CMeshObject`/`CBeamLattice`/`CWriter`/`CReader`/`CSliceStack`) over `IntPtr` handles, the static `Wrapper` factory, and the `s…`/`e…` struct/enum value types
- asset: RID-keyed native `lib3mf.dll`/`.so`/`.dylib` (ALC-firebreak/sidecar posture, content-keyed at the wire); dependency-light, no submodule closure
- rail: fabrication — the additive build-hand-off writer, golden-fixture gated

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: model + resource handles
- rail: fabrication

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]   | [CAPABILITY]                                                  |
| :-----: | :----------------------------- | :-------------- | :------------------------------------------------------------ |
|  [01]   | `CModel`                       | document root   | the in-memory 3MF model; resource + build-item owner          |
|  [02]   | `CObject : CResource`          | object base     | shared object contract (`GetType`/`SetType`/`GetName`/outbox) |
|  [03]   | `CMeshObject`                  | mesh object     | triangle mesh + optional `CBeamLattice`                       |
|  [04]   | `CComponentsObject`            | assembly object | component tree referencing other objects with transforms      |
|  [05]   | `CLevelSet`                    | implicit object | function-backed level-set (volumetric) object                 |
|  [06]   | `CVolumeData`                  | volume data     | field-backed volumetric property attachment                   |
|  [07]   | `CBuildItem`                   | build item      | one placed object instance with a build transform + UUID      |
|  [08]   | `CMetaData` / `CMetaDataGroup` | metadata        | typed key/namespace metadata                                  |

[PUBLIC_TYPE_SCOPE]: beam-lattice + additive property groups
- rail: fabrication

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]    | [CAPABILITY]                                              |
| :-----: | :------------------------ | :--------------- | :-------------------------------------------------------- |
|  [01]   | `CBeamLattice`            | lattice geometry | beams + balls on a mesh object (the PicoGK `Lattice` map) |
|  [02]   | `CBeamSet`                | beam group       | named beam-index subset                                   |
|  [03]   | `CBaseMaterialGroup`      | material group   | base-material rows (name + display color)                 |
|  [04]   | `CMultiPropertyGroup`     | property group   | layered/blended property mapping                          |
|  [05]   | `CSliceStack : CResource` | slice stack      | resin/powder planar layer stack (`AddSlice`)              |
|  [06]   | `CSlice`                  | slice layer      | one Z-layer polygon set + top-Z                           |

[PUBLIC_TYPE_SCOPE]: serialization + value types
- rail: fabrication

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                                 |
| :-----: | :---------------------- | :-------------- | :------------------------------------------- |
|  [01]   | `CWriter`               | writer          | serialize the model to a `3mf`/`stl` package |
|  [02]   | `CReader`               | reader          | parse a `3mf`/`stl` package into the model   |
|  [03]   | `sPosition`             | mesh struct     | vertex `float[3]` marshalling                |
|  [04]   | `sTriangle`             | mesh struct     | triangle `uint[3]` marshalling               |
|  [05]   | `sBeam`                 | lattice struct  | index pair, radii, and cap modes             |
|  [06]   | `sBall`                 | lattice struct  | index and radius                             |
|  [07]   | `sTransform`            | geometry struct | `Fields : Single[][]` 4×3 affine transform   |
|  [08]   | `sBox`                  | geometry struct | axis-aligned outbox                          |
|  [09]   | `eModelUnit`            | enum            | unit discriminant                            |
|  [10]   | `eObjectType`           | enum            | object-kind discriminant                     |
|  [11]   | `eBeamLatticeCapMode`   | enum            | beam-cap discriminant                        |
|  [12]   | `eSlicesMeshResolution` | enum            | slice-resolution discriminant                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory + model lifecycle — `Lib3MF.Wrapper` / `CModel`
- rail: fabrication

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [CAPABILITY]                                           |
| :-----: | :--------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `Wrapper.CreateModel()`                                    | factory        | mint an empty `CModel`                                 |
|  [02]   | `Wrapper.GetLibraryVersion(out maj, out min, out mic)`     | factory        | native version probe                                   |
|  [03]   | `Wrapper.GetSpecificationVersion(url, out ok, …)`          | factory        | extension-support probe (production/beamlattice/slice) |
|  [04]   | `Wrapper.RGBAToColor` / `ColorToRGBA`                      | factory        | `sColor` ⇄ RGBA marshalling                            |
|  [05]   | `model.SetUnit(eModelUnit)` / `GetUnit()`                  | policy         | model unit (mm the fabrication default)                |
|  [06]   | `model.AddMeshObject()`                                    | resource       | add a mesh object                                      |
|  [07]   | `model.AddComponentsObject()`                              | resource       | add an assembly object                                 |
|  [08]   | `model.AddBaseMaterialGroup()`                             | resource       | add a base-material group                              |
|  [09]   | `model.AddSliceStack(bottomZ)`                             | resource       | add a resin/powder slice stack                         |
|  [10]   | `model.AddBuildItem(object, sTransform)`                   | build          | place an object instance in the build                  |
|  [11]   | `model.GetBuildItems()`                                    | query          | build-tree iterator                                    |
|  [12]   | `model.GetResources()`                                     | query          | resource iterator                                      |
|  [13]   | `model.GetMeshObjects()`                                   | query          | mesh-object iterator                                   |
|  [14]   | `model.GetMetaDataGroup()` / `AddAttachment(uri, relType)` | metadata       | model metadata + package attachments                   |
|  [15]   | `model.QueryWriter(keyword)` / `QueryReader(keyword)`      | serialize      | mint a `3mf`/`stl` `CWriter`/`CReader`                 |
|  [16]   | `model.SetBuildUUID(uuid)` / `GetBuildUUID(out hasUuid)`   | production     | production-extension build identity                    |
|  [17]   | `object.SetUUID(uuid)` / `GetUUID(out hasUuid)`            | production     | production-extension object identity                   |
|  [18]   | `buildItem.SetUUID(uuid)` / `GetUUID(out hasUuid)`         | production     | production-extension placement identity                |
|  [19]   | `buildItem.GetObjectResource()` / `GetObjectTransform()`   | build query    | placed object plus affine transform                    |
|  [20]   | `component.GetObjectResource()` / `GetTransform()`         | assembly query | nested object plus affine transform                    |
|  [21]   | `components.GetComponentCount()` / `GetComponent(index)`   | assembly query | component-tree traversal                               |
|  [22]   | `object.GetUniqueResourceID()`                             | identity       | component-cycle guard key                              |
|  [23]   | `resource.GetResourceID()` / `GetUniqueResourceID()`       | identity       | resource and property attribution keys                 |
|  [24]   | `components.AddComponent(object, transform)`               | assembly       | add a transformed child resource                       |
|  [25]   | `materialGroup.AddMaterial(name, color)`                   | property       | add a base-material property row                       |
|  [26]   | `model.AddMultiPropertyGroup()`                            | property       | add a layered property group                           |
|  [27]   | `multiProperty.AddMultiProperty(ids)`                      | property       | compose layered property identifiers                   |
|  [28]   | `attachment.ReadFromBuffer(byte[])`                        | attachment     | populate a package relationship payload                |
|  [29]   | `multiProperty.AddLayer(sMultiPropertyLayer)`              | property       | declare a blended property-resource layer              |
|  [30]   | `model.AddLevelSet()` / `AddVolumeData()`                  | resource       | add implicit and field-backed volume resources         |
|  [31]   | `levelSet.SetMinFeatureSize(double)`                       | policy         | declare the minimum realizable implicit feature        |

[ENTRYPOINT_SCOPE]: mesh + beam-lattice authoring — `CMeshObject` / `CBeamLattice`
- rail: fabrication

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------ | :------------- | :---------------------------------------------- |
|  [01]   | `mesh.AddVertex(sPosition)` / `SetVertex`               | input          | append/set a vertex                             |
|  [02]   | `mesh.AddTriangle(sTriangle)` / `SetTriangle`           | input          | append/set a triangle                           |
|  [03]   | `mesh.SetGeometry(sPosition[], sTriangle[])`            | input          | bulk mesh assignment (the fast path)            |
|  [04]   | `mesh.IsManifoldAndOriented()`                          | validate       | watertight/orientation gate before write        |
|  [05]   | `mesh.SetObjectLevelProperty` / `SetTriangleProperties` | property       | material/color property mapping                 |
|  [06]   | `mesh.BeamLattice()`                                    | lattice        | the mesh's `CBeamLattice` handle                |
|  [07]   | `lattice.SetMinLength(d)`                               | policy         | minimum beam length                             |
|  [08]   | `lattice.SetRepresentation`                             | policy         | lattice representation                          |
|  [09]   | `lattice.SetClipping`                                   | policy         | lattice clipping                                |
|  [10]   | `lattice.SetBallOptions`                                | policy         | lattice ball parameters                         |
|  [11]   | `lattice.AddBeam(sBeam)` / `SetBeams(sBeam[])`          | input          | beam geometry (PicoGK `Lattice` beams map here) |
|  [12]   | `lattice.AddBall(sBall)` / `SetBalls(sBall[])`          | input          | ball geometry (node spheres)                    |
|  [13]   | `lattice.AddBeamSet()` / `GetBeamSet(i)`                | grouping       | named beam subsets                              |
|  [14]   | `mesh.GetVertices(out sPosition[])`                     | read           | bulk vertex detachment                          |
|  [15]   | `mesh.GetTriangleIndices(out sTriangle[])`              | read           | bulk triangle detachment                        |
|  [16]   | `beamSet.SetReferences(uint[])` / `SetBallReferences`   | grouping       | bind beam and ball indices to a named subset    |

[ENTRYPOINT_SCOPE]: serialization — `CWriter` / `CReader` / `CSliceStack`
- rail: fabrication

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                        |
| :-----: | :-------------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `writer.WriteToFile(path)`                                      | write          | serialize to a `.3mf`/`.stl` file                   |
|  [02]   | `writer.WriteToBuffer(out byte[])` / `GetStreamSize()`          | write          | serialize to an in-memory buffer (content-key seed) |
|  [03]   | `writer.SetDecimalPrecision(n)` / `SetStrictModeActive(b)`      | policy         | precision + strict-mode toggles                     |
|  [04]   | `writer.GetWarning(i, …)` / `GetWarningCount()`                 | diagnostic     | non-fatal write warnings                            |
|  [05]   | `reader.ReadFromFile(path)` / `ReadFromBuffer(byte[])`          | read           | parse a package into the model                      |
|  [06]   | `reader.AddRelationToRead(relType)` / `SetStrictModeActive`     | policy         | selective-relation + strict parse                   |
|  [07]   | `sliceStack.AddSlice(topZ)` / `GetSlice(i)` / `GetSliceCount()` | slice          | build/read planar layer stacks                      |
|  [08]   | `reader.GetWarning(i, out code)` / `GetWarningCount()`          | diagnostic     | non-fatal read warning evidence                     |

## [04]-[IMPLEMENTATION_LAW]

[NATIVE_TOPOLOGY]:
- every `C…` handle wraps an `IntPtr` over the native `lib3mf` core; the binding marshals through `Internal.Lib3MFWrapper` and lifts native error codes via `CheckError` into `Lib3MFException` — the boundary owner catches that binding exception ONCE and lowers it to the `Additive` `ThreeMfWriteRejected` 2747 `(EgressKind target, string native)` arm on the `Fin` rail; a CLR defect propagates
- Native shared libraries are RID-keyed and ALC-firebreaked: keep them sidecar/host-side, never referenced from a type that loads in an in-Rhino plugin ALC
- extension support is capability-probed through `Wrapper.GetSpecificationVersion` (production `.../production/2015/06`, beam-lattice `.../beamlattice/2017/02`, slice `.../slice/2015/07`); a required-but-unsupported extension raises `Additive` `Unsupported3mfExtension` 2725 `(ThreeMfExtension extension, EgressKind target)` at the boundary

[LOCAL_ADMISSION]:
- Solid ingress traverses `GetBuildItems`, composes nested component transforms, and detaches resources through `GetVertices`/`GetTriangleIndices` before canonical mesh admission.
- `Additive/production#Production.Plan` owns the 3MF egress: the oriented `MeshSpace` result marshals to `mesh.SetGeometry`, machine profiles set `model.SetUnit`/base-material rows, `AddBuildItem` places the oriented part with its build transform, and `QueryWriter("3mf").WriteToBuffer` produces the content-keyed package — the STL-implied hand-off dies for the typed 3MF writer
- PicoGK `Lattice` beam/node sets map directly onto `CBeamLattice`: `Additive/implicit` beam-support and scaffold geometry lowers to `lattice.SetBeams(sBeam[])` + `lattice.SetBalls(sBall[])` on the build mesh's `BeamLattice()`, never an STL tessellation of the lattice — the beam-lattice extension is the native carrier
- resin/powder planar layer stacks route through `CSliceStack.AddSlice` when the 3MF slice extension is the hand-off target; the PicoGK `.cli`/grayscale vector path stays `Additive/implicit`'s and is orthogonal to this writer
- 3MF content keys mint through `ContentKey.Of(EgressKind.ThreeMf, bytes)` over `writer.WriteToBuffer` (the ONE mint site, K9); NEVER a second hasher over the native stream
- golden-fixture gated: the writer admits only against a committed round-trip 3MF golden fixture (write → `CReader.ReadFromFile` → structural compare) per RID

[RAIL_LAW]:
- Package: `lib3mf` (vendored ACT-generated binding + RID-keyed native, BSD-2)
- Owns: 3MF core/production/beam-lattice/slice READ + WRITE — the additive build-hand-off document
- Accept: an oriented `MeshSpace` + machine profile + optional PicoGK `Lattice` from `Additive/production`
- Reject: hand-rolled 3MF XML/OPC synthesis, an STL fallback where the 3MF beam-lattice/slice extension carries the geometry, and re-minting a second content hash over the native buffer
