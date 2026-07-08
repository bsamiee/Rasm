# [RASM_FABRICATION_API_LIB3MF]

`lib3mf` is the official 3MF Consortium reference implementation — the 3MF core/production/beam-lattice reader and WRITER driving the `Additive/production` build-hand-off egress. The managed surface is the ACT-generated C# binding (interface version `2.5.0`): a `Lib3MF.Wrapper` static factory mints a `CModel`, resources attach through `AddMeshObject`/`AddComponentsObject`/`AddBaseMaterialGroup`/`AddSliceStack`, a mesh grows beam-lattice geometry through `CMeshObject.BeamLattice()`, the build tree assembles through `AddBuildItem`, and `QueryWriter("3mf")`/`QueryReader("3mf")` serialize the OPC package. The binding P/Invokes the native `lib3mf` shared library through the `Internal.Lib3MFWrapper` layer, so every managed call is a thin marshalling shell over the native core; there is no NuGet package (feed-verified; `IxMilia.ThreeMf` dominated) — the binding source + the RID-keyed native asset vendor into the folder.

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

| [INDEX] | [SYMBOL]                                                                 | [TYPE_FAMILY]   | [CAPABILITY]                                                  |
| :-----: | :----------------------------------------------------------------------- | :-------------- | :------------------------------------------------------------ |
|  [01]   | `CWriter`                                                                | writer          | serialize the model to a `3mf`/`stl` package                  |
|  [02]   | `CReader`                                                                | reader          | parse a `3mf`/`stl` package into the model                    |
|  [03]   | `sPosition`/`sTriangle`                                                  | mesh struct     | vertex `float[3]` / triangle `uint[3]` marshalling            |
|  [04]   | `sBeam`/`sBall`                                                          | lattice struct  | beam (index pair + radii + cap modes) / ball (index + radius) |
|  [05]   | `sTransform`/`sBox`                                                      | geometry struct | 4×3 build/component transform / axis-aligned outbox           |
|  [06]   | `eModelUnit`/`eObjectType`/`eBeamLatticeCapMode`/`eSlicesMeshResolution` | enum            | unit, object-kind, beam-cap, slice-resolution discriminants   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory + model lifecycle — `Lib3MF.Wrapper` / `CModel`
- rail: fabrication

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                           |
| :-----: | :-------------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `Wrapper.CreateModel()`                                         | factory        | mint an empty `CModel`                                 |
|  [02]   | `Wrapper.GetLibraryVersion(out maj, out min, out mic)`          | factory        | native version probe                                   |
|  [03]   | `Wrapper.GetSpecificationVersion(url, out ok, …)`               | factory        | extension-support probe (production/beamlattice/slice) |
|  [04]   | `Wrapper.RGBAToColor` / `ColorToRGBA`                           | factory        | `sColor` ⇄ RGBA marshalling                            |
|  [05]   | `model.SetUnit(eModelUnit)` / `GetUnit()`                       | policy         | model unit (mm the fabrication default)                |
|  [06]   | `model.AddMeshObject()`                                         | resource       | add a mesh object                                      |
|  [07]   | `model.AddComponentsObject()`                                   | resource       | add an assembly object                                 |
|  [08]   | `model.AddBaseMaterialGroup()`                                  | resource       | add a base-material group                              |
|  [09]   | `model.AddSliceStack(bottomZ)`                                  | resource       | add a resin/powder slice stack                         |
|  [10]   | `model.AddBuildItem(object, sTransform)`                        | build          | place an object instance in the build                  |
|  [11]   | `model.GetBuildItems()` / `GetResources()` / `GetMeshObjects()` | query          | resource + build-tree iterators                        |
|  [12]   | `model.GetMetaDataGroup()` / `AddAttachment(uri, relType)`      | metadata       | model metadata + package attachments                   |
|  [13]   | `model.QueryWriter(keyword)` / `QueryReader(keyword)`           | serialize      | mint a `3mf`/`stl` `CWriter`/`CReader`                 |

[ENTRYPOINT_SCOPE]: mesh + beam-lattice authoring — `CMeshObject` / `CBeamLattice`
- rail: fabrication

| [INDEX] | [SURFACE]                                                                      | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `mesh.AddVertex(sPosition)` / `SetVertex`                                      | input          | append/set a vertex                             |
|  [02]   | `mesh.AddTriangle(sTriangle)` / `SetTriangle`                                  | input          | append/set a triangle                           |
|  [03]   | `mesh.SetGeometry(sPosition[], sTriangle[])`                                   | input          | bulk mesh assignment (the fast path)            |
|  [04]   | `mesh.IsManifoldAndOriented()`                                                 | validate       | watertight/orientation gate before write        |
|  [05]   | `mesh.SetObjectLevelProperty` / `SetTriangleProperties`                        | property       | material/color property mapping                 |
|  [06]   | `mesh.BeamLattice()`                                                           | lattice        | the mesh's `CBeamLattice` handle                |
|  [07]   | `lattice.SetMinLength(d)` / `SetRepresentation`/`SetClipping`/`SetBallOptions` | policy         | lattice global parameters                       |
|  [08]   | `lattice.AddBeam(sBeam)` / `SetBeams(sBeam[])`                                 | input          | beam geometry (PicoGK `Lattice` beams map here) |
|  [09]   | `lattice.AddBall(sBall)` / `SetBalls(sBall[])`                                 | input          | ball geometry (node spheres)                    |
|  [10]   | `lattice.AddBeamSet()` / `GetBeamSet(i)`                                       | grouping       | named beam subsets                              |

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

## [04]-[IMPLEMENTATION_LAW]

[NATIVE_TOPOLOGY]:
- every `C…` handle wraps an `IntPtr` over the native `lib3mf` core; the binding marshals through `Internal.Lib3MFWrapper` and lifts native error codes via `CheckError` into `Lib3MFException` — the boundary owner catches that binding exception ONCE and lowers it to the `Additive` `ThreeMfWriteRejected` 2747 `(EgressKind target, string native)` arm on the `Fin` rail; a CLR defect propagates
- the native shared library is RID-keyed and ALC-firebreaks: keep it sidecar/host-side, never referenced from a type that must load in an in-Rhino plugin ALC
- extension support is capability-probed through `Wrapper.GetSpecificationVersion` (production `.../production/2015/06`, beam-lattice `.../beamlattice/2017/02`, slice `.../slice/2015/07`); a required-but-unsupported extension raises `Additive` `Unsupported3mfExtension` 2725 `(ThreeMfExtension extension, EgressKind target)` at the boundary

[LOCAL_ADMISSION]:
- `Additive/production#Production.Plan` owns the 3MF egress: the oriented `MeshSpace` result marshals to `mesh.SetGeometry`, machine profiles set `model.SetUnit`/base-material rows, `AddBuildItem` places the oriented part with its build transform, and `QueryWriter("3mf").WriteToBuffer` produces the content-keyed package — the STL-implied hand-off dies for the typed 3MF writer
- the PicoGK `Lattice` beam/node set maps DIRECTLY onto `CBeamLattice`: `Additive/implicit` beam supports/scaffolds lower to `lattice.SetBeams(sBeam[])` + `lattice.SetBalls(sBall[])` on the build mesh's `BeamLattice()`, never an STL tessellation of the lattice — the beam-lattice extension is the native carrier
- resin/powder planar layer stacks route through `CSliceStack.AddSlice` when the 3MF slice extension is the hand-off target; the PicoGK `.cli`/grayscale vector path stays `Additive/implicit`'s and is orthogonal to this writer
- the 3MF content key mints through `ContentHash.Of` over `writer.WriteToBuffer` (the ONE mint site, K9); NEVER a second hasher over the native stream
- golden-fixture gated: the writer admits only against a committed round-trip 3MF golden fixture (write → `CReader.ReadFromFile` → structural compare) per RID

[RAIL_LAW]:
- Package: `lib3mf` (vendored ACT-generated binding + RID-keyed native, BSD-2)
- Owns: 3MF core/production/beam-lattice/slice READ + WRITE — the additive build-hand-off document
- Accept: an oriented `MeshSpace` + machine profile + optional PicoGK `Lattice` from `Additive/production`
- Reject: hand-rolled 3MF XML/OPC synthesis, an STL fallback where the 3MF beam-lattice/slice extension carries the geometry, and re-minting a second content hash over the native buffer
