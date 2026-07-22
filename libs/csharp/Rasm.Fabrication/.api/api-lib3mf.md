# [RASM_FABRICATION_API_LIB3MF]

`lib3mf` owns 3MF read and write across the core, production, beam-lattice, and slice extensions — the 3MF Consortium reference codec. It carries the additive build hand-off document: an oriented mesh with its beam-lattice and slice geometry, machine metadata, and placed build items serialize to one content-keyed package. ACT-generated C# bindings marshal every `C…` handle over the native core through `Internal.Lib3MFWrapper`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lib3mf`
- package: `lib3mf` (`BSD-2-Clause`, 3MF Consortium)
- assembly: vendored ACT-generated `Lib3MF.cs` compiled into `Rasm.Fabrication`; the native core is a RID-keyed `lib3mf` shared library
- namespace: `Lib3MF` — `C…` handle wrappers over `IntPtr`, the static `Wrapper` factory, and the `s…`/`e…` struct and enum value types
- abi: RID-keyed native `lib3mf.dll`/`.so`/`.dylib`, ALC-firebreaked sidecar-only, content-keyed at the wire
- rail: fabrication — the additive build hand-off writer, golden-fixture gated

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: model + resource handles

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :----------------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `CModel`                       | class         | the in-memory 3MF model; resource + build-item owner          |
|  [02]   | `CObject : CResource`          | class         | shared object contract (`GetType`/`SetType`/`GetName`/outbox) |
|  [03]   | `CMeshObject`                  | class         | triangle mesh + optional `CBeamLattice`                       |
|  [04]   | `CComponentsObject`            | class         | component tree referencing other objects with transforms      |
|  [05]   | `CLevelSet`                    | class         | function-backed level-set (volumetric) object                 |
|  [06]   | `CVolumeData`                  | class         | field-backed volumetric property attachment                   |
|  [07]   | `CBuildItem`                   | class         | one placed object instance with a build transform + UUID      |
|  [08]   | `CMetaData` / `CMetaDataGroup` | class         | typed key/namespace metadata                                  |

[PUBLIC_TYPE_SCOPE]: beam-lattice + additive property groups

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------ | :------------ | :-------------------------------------------------------- |
|  [01]   | `CBeamLattice`            | class         | beams + balls on a mesh object (the PicoGK `Lattice` map) |
|  [02]   | `CBeamSet`                | class         | named beam-index subset                                   |
|  [03]   | `CBaseMaterialGroup`      | class         | base-material rows (name + display color)                 |
|  [04]   | `CMultiPropertyGroup`     | class         | layered/blended property mapping                          |
|  [05]   | `CSliceStack : CResource` | class         | resin/powder planar layer stack (`AddSlice`)              |
|  [06]   | `CSlice`                  | class         | one Z-layer polygon set + top-Z                           |

[PUBLIC_TYPE_SCOPE]: serialization + value types

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                 |
| :-----: | :---------------------- | :------------ | :------------------------------------------- |
|  [01]   | `CWriter`               | class         | serialize the model to a `3mf`/`stl` package |
|  [02]   | `CReader`               | class         | parse a `3mf`/`stl` package into the model   |
|  [03]   | `sPosition`             | struct        | vertex `float[3]` marshalling                |
|  [04]   | `sTriangle`             | struct        | triangle `uint[3]` marshalling               |
|  [05]   | `sBeam`                 | struct        | index pair, radii, and cap modes             |
|  [06]   | `sBall`                 | struct        | index and radius                             |
|  [07]   | `sTransform`            | struct        | `Fields : Single[][]` 4×3 affine transform   |
|  [08]   | `sBox`                  | struct        | axis-aligned outbox                          |
|  [09]   | `eModelUnit`            | enum          | unit discriminant                            |
|  [10]   | `eObjectType`           | enum          | object-kind discriminant                     |
|  [11]   | `eBeamLatticeCapMode`   | enum          | beam-cap discriminant                        |
|  [12]   | `eSlicesMeshResolution` | enum          | slice-resolution discriminant                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory + model lifecycle — `Lib3MF.Wrapper` / `CModel`

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :--------------------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `Wrapper.CreateModel()`                                    | factory  | mint an empty `CModel`                                 |
|  [02]   | `Wrapper.GetLibraryVersion(out maj, out min, out mic)`     | static   | native version probe                                   |
|  [03]   | `Wrapper.GetSpecificationVersion(url, out ok, …)`          | static   | extension-support probe (production/beamlattice/slice) |
|  [04]   | `Wrapper.RGBAToColor` / `ColorToRGBA`                      | static   | `sColor` ⇄ RGBA marshalling                            |
|  [05]   | `model.SetUnit(eModelUnit)` / `GetUnit()`                  | instance | model unit (mm the fabrication default)                |
|  [06]   | `model.AddMeshObject()`                                    | factory  | add a mesh object                                      |
|  [07]   | `model.AddComponentsObject()`                              | factory  | add an assembly object                                 |
|  [08]   | `model.AddBaseMaterialGroup()`                             | factory  | add a base-material group                              |
|  [09]   | `model.AddSliceStack(bottomZ)`                             | factory  | add a resin/powder slice stack                         |
|  [10]   | `model.AddBuildItem(object, sTransform)`                   | factory  | place an object instance in the build                  |
|  [11]   | `model.GetBuildItems()`                                    | instance | build-tree iterator                                    |
|  [12]   | `model.GetResources()`                                     | instance | resource iterator                                      |
|  [13]   | `model.GetMeshObjects()`                                   | instance | mesh-object iterator                                   |
|  [14]   | `model.GetMetaDataGroup()` / `AddAttachment(uri, relType)` | factory  | model metadata + package attachments                   |
|  [15]   | `model.QueryWriter(keyword)` / `QueryReader(keyword)`      | factory  | mint a `3mf`/`stl` `CWriter`/`CReader`                 |
|  [16]   | `model.SetBuildUUID(uuid)` / `GetBuildUUID(out hasUuid)`   | instance | production-extension build identity                    |
|  [17]   | `object.SetUUID(uuid)` / `GetUUID(out hasUuid)`            | instance | production-extension object identity                   |
|  [18]   | `buildItem.SetUUID(uuid)` / `GetUUID(out hasUuid)`         | instance | production-extension placement identity                |
|  [19]   | `buildItem.GetObjectResource()` / `GetObjectTransform()`   | instance | placed object plus affine transform                    |
|  [20]   | `component.GetObjectResource()` / `GetTransform()`         | instance | nested object plus affine transform                    |
|  [21]   | `components.GetComponentCount()` / `GetComponent(index)`   | instance | component-tree traversal                               |
|  [22]   | `object.GetUniqueResourceID()`                             | instance | component-cycle guard key                              |
|  [23]   | `resource.GetResourceID()` / `GetUniqueResourceID()`       | instance | resource and property attribution keys                 |
|  [24]   | `components.AddComponent(object, transform)`               | factory  | add a transformed child resource                       |
|  [25]   | `materialGroup.AddMaterial(name, color)`                   | factory  | add a base-material property row                       |
|  [26]   | `model.AddMultiPropertyGroup()`                            | factory  | add a layered property group                           |
|  [27]   | `multiProperty.AddMultiProperty(ids)`                      | factory  | compose layered property identifiers                   |
|  [28]   | `attachment.ReadFromBuffer(byte[])`                        | instance | populate a package relationship payload                |
|  [29]   | `multiProperty.AddLayer(sMultiPropertyLayer)`              | factory  | declare a blended property-resource layer              |
|  [30]   | `model.AddLevelSet()` / `AddVolumeData()`                  | factory  | add implicit and field-backed volume resources         |
|  [31]   | `levelSet.SetMinFeatureSize(double)`                       | instance | declare the minimum realizable implicit feature        |

[ENTRYPOINT_SCOPE]: mesh + beam-lattice authoring — `CMeshObject` / `CBeamLattice`

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------ | :------- | :---------------------------------------------- |
|  [01]   | `mesh.AddVertex(sPosition)` / `SetVertex`               | instance | append/set a vertex                             |
|  [02]   | `mesh.AddTriangle(sTriangle)` / `SetTriangle`           | instance | append/set a triangle                           |
|  [03]   | `mesh.SetGeometry(sPosition[], sTriangle[])`            | instance | bulk mesh assignment (the fast path)            |
|  [04]   | `mesh.IsManifoldAndOriented()`                          | instance | watertight/orientation gate before write        |
|  [05]   | `mesh.SetObjectLevelProperty` / `SetTriangleProperties` | instance | material/color property mapping                 |
|  [06]   | `mesh.BeamLattice()`                                    | factory  | the mesh's `CBeamLattice` handle                |
|  [07]   | `lattice.SetMinLength(d)`                               | instance | minimum beam length                             |
|  [08]   | `lattice.SetRepresentation`                             | instance | lattice representation                          |
|  [09]   | `lattice.SetClipping`                                   | instance | lattice clipping                                |
|  [10]   | `lattice.SetBallOptions`                                | instance | lattice ball parameters                         |
|  [11]   | `lattice.AddBeam(sBeam)` / `SetBeams(sBeam[])`          | instance | beam geometry (PicoGK `Lattice` beams map here) |
|  [12]   | `lattice.AddBall(sBall)` / `SetBalls(sBall[])`          | instance | ball geometry (node spheres)                    |
|  [13]   | `lattice.AddBeamSet()` / `GetBeamSet(i)`                | factory  | named beam subsets                              |
|  [14]   | `mesh.GetVertices(out sPosition[])`                     | instance | bulk vertex detachment                          |
|  [15]   | `mesh.GetTriangleIndices(out sTriangle[])`              | instance | bulk triangle detachment                        |
|  [16]   | `beamSet.SetReferences(uint[])` / `SetBallReferences`   | instance | bind beam and ball indices to a named subset    |

[ENTRYPOINT_SCOPE]: serialization — `CWriter` / `CReader` / `CSliceStack`

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :-------------------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `writer.WriteToFile(path)`                                      | instance | serialize to a `.3mf`/`.stl` file                   |
|  [02]   | `writer.WriteToBuffer(out byte[])` / `GetStreamSize()`          | instance | serialize to an in-memory buffer (content-key seed) |
|  [03]   | `writer.SetDecimalPrecision(n)` / `SetStrictModeActive(b)`      | instance | precision + strict-mode toggles                     |
|  [04]   | `writer.GetWarning(i, …)` / `GetWarningCount()`                 | instance | non-fatal write warnings                            |
|  [05]   | `reader.ReadFromFile(path)` / `ReadFromBuffer(byte[])`          | instance | parse a package into the model                      |
|  [06]   | `reader.AddRelationToRead(relType)` / `SetStrictModeActive`     | instance | selective-relation + strict parse                   |
|  [07]   | `sliceStack.AddSlice(topZ)` / `GetSlice(i)` / `GetSliceCount()` | factory  | build/read planar layer stacks                      |
|  [08]   | `reader.GetWarning(i, out code)` / `GetWarningCount()`          | instance | non-fatal read warning evidence                     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- every `C…` handle wraps an `IntPtr` over the native core; `Internal.Lib3MFWrapper` marshals the call and `CheckError` lifts a native error code into `Lib3MFException`, which the boundary owner catches once and lowers to the `Additive` `ThreeMfWriteRejected` 2747 `(EgressKind target, string native)` arm on the `Fin` rail — a CLR defect propagates
- RID-keyed native libraries stay sidecar/host-side, never referenced from a type loading in an in-Rhino plugin ALC
- `Wrapper.GetSpecificationVersion` capability-probes each extension (production, beam-lattice, slice); a required-but-unsupported extension raises the `Additive` `Unsupported3mfExtension` 2725 `(ThreeMfExtension extension, EgressKind target)` arm at the boundary

[STACKING]:
- `api-picogk`(`.api/api-picogk.md`): a PicoGK `Lattice` (`AddBeam`/`AddSphere`) lowers its beam and node sets onto the build mesh's `BeamLattice()` through `lattice.SetBeams(sBeam[])` + `lattice.SetBalls(sBall[])`, and a `Voxels.mshAsMesh()` extraction marshals to `mesh.SetGeometry(sPosition[], sTriangle[])` — the beam-lattice extension is the native carrier over any STL tessellation
- `Additive/production#Production.Plan`: marshals the oriented `MeshSpace` result to `mesh.SetGeometry`, drives `model.SetUnit` and base-material rows from the machine profile, places the oriented part through `model.AddBuildItem`, and produces the content-keyed package through `model.QueryWriter("3mf").WriteToBuffer`

[LOCAL_ADMISSION]:
- solid ingress traverses `model.GetBuildItems`, composes nested component transforms, and detaches resources through `mesh.GetVertices`/`GetTriangleIndices` before canonical mesh admission
- resin/powder planar layer stacks route through `CSliceStack.AddSlice` only when the 3MF slice extension is the hand-off target; the PicoGK `.cli`/grayscale vector path stays `Additive/implicit`
- `ContentKey.Of(EgressKind.ThreeMf, bytes)` over `writer.WriteToBuffer` is the sole 3MF content-key mint site
- `CWriter` admits only against a committed per-RID round-trip golden fixture — write, `CReader.ReadFromFile`, structural compare

[RAIL_LAW]:
- Package: `lib3mf` (vendored ACT-generated binding, RID-keyed native, BSD-2)
- Owns: 3MF core, production, beam-lattice, and slice read and write — the additive build hand-off document
- Accept: an oriented `MeshSpace`, a machine profile, and an optional PicoGK `Lattice` from `Additive/production`
- Reject: hand-rolled 3MF XML/OPC synthesis, an STL fallback where the 3MF beam-lattice or slice extension carries the geometry, and a second content hash over the native buffer
