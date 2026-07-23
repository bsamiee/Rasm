# [RASM_FABRICATION_API_PICOGK]

`PicoGK` (LEAP71) owns the implicit/SDF/voxel geometry kernel for additive manufacturing over an OpenVDB/boost/TBB native core. An `IImplicit` signed-distance field rasterizes to `Voxels`, the field owner of 3D SDF Boolean, distance morphology, dual-contour mesh extraction, and the grayscale-and-vector SLA/DLP/MSLA layer stack no planar FFF path reaches. Planar perimeters and 2D toolpaths stay on `Clipper2`/`CavalierContours`; the voxel/SDF/layer-stack concern stays here. `Voxels.mshAsMesh` is the wire seam handing an extracted `Mesh` to the kernel `MeshSpace` vocabulary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PicoGK`
- package: `PicoGK` (Apache-2.0)
- assembly: `PicoGK`
- namespace: `PicoGK`
- asset: `lib/net9.0/PicoGK.dll`; a `net10.0` consumer binds the net9.0 fallback. A RID-bearing native core `runtimes/<rid>/native/picogk.26.2.*` (OpenVDB/boost/TBB) firebreaks it out of any in-Rhino plugin ALC
- abi: `Config.strPicoGKLib = "picogk.26.2"` names the P/Invoke target
- depends: `SkiaSharp`, riding the App-UI SkiaSharp row
- rail: fabrication companion, outside-Rhino AM

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the implicit (SDF) contract — the field-function entry

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]  | [CAPABILITY]          |
| :-----: | :--------------------- | :------------- | :-------------------- |
|  [01]   | `IImplicit`            | SDF contract   | signed-distance field |
|  [02]   | `IBoundedImplicit`     | bounded SDF    | self-bounding field   |
|  [03]   | `ITraverseScalarField` | scalar visitor | active-voxel callback |
|  [04]   | `ITraverseVectorField` | vector visitor | active-voxel callback |

- `IImplicit.fSignedDistance(in Vector3) -> float`: negative inside, positive outside; an unbounded `IImplicit` needs an explicit `BBox3`, `IBoundedImplicit` supplies `oBounds`.

[PUBLIC_TYPE_SCOPE]: the field owners — voxels and sampled fields

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]                             | [CAPABILITY]          |
| :-----: | :------------------- | :---------------------------------------- | :-------------------- |
|  [01]   | `Voxels`             | SDF field (`IDisposable`)                 | voxel geometry owner  |
|  [02]   | `ScalarField`        | scalar field (`IImplicit`, `IDisposable`) | sampled scalar values |
|  [03]   | `VectorField`        | vector field (`IDisposable`)              | sampled vector values |
|  [04]   | `FieldMetadata`      | metadata (`IDisposable`)                  | typed field values    |
|  [05]   | `IFieldWithMetadata` | metadata contract                         | metadata access       |

- `[ScalarField]`: `SetValue(Vector3, float)` `bGetValue(Vector3, out float)` `RemoveValue(Vector3)` `TraverseActive(ITraverseScalarField)` `fSignedDistance(in Vector3)`
- `[VectorField]` (thermal/fibre drivers): `SetValue(Vector3, Vector3)` `bGetValue(Vector3, out Vector3)` `RemoveValue(Vector3)` `TraverseActive(ITraverseVectorField)`
- `IFieldWithMetadata.oMetaData() -> FieldMetadata` on `Voxels`, `ScalarField`, and `VectorField`.

[PUBLIC_TYPE_SCOPE]: scaffolds, meshes, lines — the geometry carriers

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]                 | [CAPABILITY]        |
| :-----: | :--------- | :---------------------------- | :------------------ |
|  [01]   | `Lattice`  | scaffold (`IDisposable`)      | beam-and-sphere CSG |
|  [02]   | `Mesh`     | triangle mesh (`IDisposable`) | extraction target   |
|  [03]   | `PolyLine` | colored line (`IDisposable`)  | viewer geometry     |
|  [04]   | `Coord`    | voxel index                   | grid coordinate     |
|  [05]   | `Triangle` | face index                    | mesh triangle       |

- `[Lattice]`: `AddBeam(Vector3, float, Vector3, float, bool)` `AddSphere(Vector3, float)` — capsule-and-sphere CSG `Voxels(Lattice)` rasterizes.
- `[Mesh]`: `nAddVertex(Vector3)` `AddVertices` `nAddTriangle(int, int, int)` `AddQuad(int, int, int, int, bool)` `vecVertexAt(int)` `oTriangleAt(int)` `GetTriangle` `nVertexCount` `nTriangleCount` `mshCreateTransformed(Matrix4x4)` `mshCreateMirrored` `Append(Mesh)` `oBoundingBox` `mshFromStlFile(string, EStlUnit, float, ...)` — the extraction target and STL bridge.
- `[PolyLine]`: `nAddVertex(Vector3)` `Add(IEnumerable<Vector3>)` `vecVertexAt(int)` `nVertexCount` `AddArrow` `AddCross` `oBoundingBox` — colored viewer geometry carried by `ColorFloat`.
- `Triangle(int, int, int)` carries a mesh face by vertex index, distinct from the kernel `Triangle.NET` mesher; `Coord(int, int, int)` carries a voxel index.

[PUBLIC_TYPE_SCOPE]: VDB I/O, slice vectorization, CLI emission — the AM file/layer surface

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]                 | [CAPABILITY]         |
| :-----: | :--------------- | :---------------------------- | :------------------- |
|  [01]   | `OpenVdbFile`    | VDB container (`IDisposable`) | multi-field VDB I/O  |
|  [02]   | `PolySliceStack` | vector slice stack            | layered contours     |
|  [03]   | `PolySlice`      | vector layer                  | closed contour layer |
|  [04]   | `PolyContour`    | 2D contour                    | closed loop          |
|  [05]   | `CliIo`          | `.cli` I/O                    | slice-program I/O    |
|  [06]   | `Vdb2Cli`        | VDB-to-CLI bridge             | direct conversion    |
|  [07]   | `TgaIo`          | image I/O                     | TGA slice raster     |

- `[OpenVdbFile]` field enumeration: `nFieldCount` `strFieldName` `eFieldType` `xField` beside the `nAdd`/`voxGet`/`oGet*` I/O.
- `[PolySliceStack]`: `nCount` `oSliceAt(int)` `oBBox` `AddSlices` `AddToViewer` — the layered contour program from `Voxels.oVectorize`.
- `[PolySlice]`: `fZPos` `AddContour(PolyContour)` `Close` `bIsEmpty` `nContours` `oContourAt(int)` `SaveToSvgFile(string, bool)` `oFromSdf(Image, float, Vector2, float)` — one Z layer of closed loops.
- `[PolyContour]`: `oVertices` `vecVertex(int)` `nCount` `eDetectWinding` `DetectWinding` `eWinding` `AsSvgPolyline(out string)` `AsSvgPath(out string)` `oBBox` — one closed loop with winding.
- `[CliIo.Result]`: `oSlices` `nLayers` `oBBoxFile` `strHeaderDate` `strWarnings` — the imported CLI payload and header receipt.

[PUBLIC_TYPE_SCOPE]: runtime, geometry primitives, faults

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]                 | [CAPABILITY]         |
| :-----: | :------------------------------- | :---------------------------- | :------------------- |
|  [01]   | `Library`                        | runtime (`IDisposable`)       | native lifecycle     |
|  [02]   | `Config`                         | native binding                | P/Invoke library id  |
|  [03]   | `Viewer`                         | OpenGL viewer (`IDisposable`) | interactive debug UI |
|  [04]   | `BBox3`                          | bounds struct                 | 3D field extent      |
|  [05]   | `BBox2`                          | bounds struct                 | 2D field extent      |
|  [06]   | `ColorFloat`                     | color                         | floating-point color |
|  [07]   | `ColorRgba32`                    | color                         | packed RGBA color    |
|  [08]   | `ColorHSV`                       | color                         | HSV color            |
|  [09]   | `ColorHLS`                       | color                         | HLS color            |
|  [10]   | `Image*`                         | image buffer                  | slice raster         |
|  [11]   | `PicoGKAllocException`           | fault                         | allocation failure   |
|  [12]   | `PicoGKLibraryMismatchException` | fault                         | version mismatch     |

- `Library`: `oLibrary()` `vecVoxelsToMm` `MmToVoxels` and the `n*MemUsage()`/`n*Allocated()` native-resource census.
- `BBox3`: `vecMin`/`vecMax` `vecSize`/`vecCenter` `Include`/`Grow`/`bContains`/`bIsEmpty` `oFitInto(bounds, out float, out Vector3)`.
- `ImageGrayScale` is the slice target; its `nWidth`/`nHeight` and public `m_afValues` float raster are the canonical-byte payload a mask content key digests.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: implicit -> voxels rasterization

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]         |
| :-----: | :---------------------------------------------------------------- | :------- | :------------------- |
|  [01]   | `Voxels(in IImplicit, in BBox3)`                                  | ctor     | boxed implicit       |
|  [02]   | `Voxels(in IBoundedImplicit)`                                     | ctor     | bounded implicit     |
|  [03]   | `Voxels.RenderImplicit(in IImplicit, in BBox3)`                   | instance | field addition       |
|  [04]   | `Voxels.IntersectImplicit(in IImplicit)`                          | instance | field intersection   |
|  [05]   | `Voxels.voxIntersectImplicit(in IImplicit) -> Voxels`             | instance | copied intersection  |
|  [06]   | `Voxels(in Lattice)`                                              | ctor     | scaffold field       |
|  [07]   | `Voxels.RenderLattice(in Lattice)`                                | instance | scaffold composition |
|  [08]   | `Voxels(in Mesh)`                                                 | ctor     | mesh field           |
|  [09]   | `Voxels.RenderMesh(in Mesh)`                                      | instance | mesh composition     |
|  [10]   | `Voxels.voxSphere(Vector3, float) -> Voxels`                      | factory  | sphere field         |
|  [11]   | `Voxels.voxLatticeBeam(Vector3, float, Vector3, float) -> Voxels` | factory  | beam field           |
|  [12]   | `Voxels.voxMeshShell(Mesh, float) -> Voxels`                      | factory  | mesh-shell field     |

- Voxel resolution is the `Library` `fVoxelSizeMM`: finer voxels sharpen the SDF surface at higher native memory.

[ENTRYPOINT_SCOPE]: SDF Boolean and distance morphology — `Voxels`

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]          |
| :-----: | :------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `Voxels.BoolAdd(in Voxels)`                              | instance | in-place union        |
|  [02]   | `Voxels.voxBoolAdd(in Voxels) -> Voxels`                 | instance | copied union          |
|  [03]   | `operator +`                                             | operator | copied union          |
|  [04]   | `Voxels.BoolSubtract(in Voxels)`                         | instance | in-place subtraction  |
|  [05]   | `Voxels.voxBoolSubtract(in Voxels) -> Voxels`            | instance | copied subtraction    |
|  [06]   | `operator -`                                             | operator | copied subtraction    |
|  [07]   | `Voxels.BoolIntersect(in Voxels)`                        | instance | in-place intersection |
|  [08]   | `Voxels.voxBoolIntersect(in Voxels) -> Voxels`           | instance | copied intersection   |
|  [09]   | `operator &`                                             | operator | copied intersection   |
|  [10]   | `Voxels.BoolAddAll(in IEnumerable<Voxels>)`              | instance | batch union           |
|  [11]   | `Voxels.BoolSubtractAll(in IEnumerable<Voxels>)`         | instance | batch subtraction     |
|  [12]   | `Voxels.voxCombine(in Voxels, in Voxels) -> Voxels`      | static   | copied combination    |
|  [13]   | `Voxels.voxCombineAll(in IEnumerable<Voxels>) -> Voxels` | static   | copied batch union    |
|  [14]   | `Voxels.Offset(float)`                                   | instance | signed field offset   |
|  [15]   | `Voxels.voxOffset(float) -> Voxels`                      | instance | copied field offset   |
|  [16]   | `Voxels.DoubleOffset(float, float)`                      | instance | two-stage open/close  |
|  [17]   | `Voxels.TripleOffset(float)`                             | instance | three-stage offset    |
|  [18]   | `Voxels.OverOffset(float, float)`                        | instance | surface conditioning  |
|  [19]   | `Voxels.voxShell(float) -> Voxels`                       | instance | copied wall field     |
|  [20]   | `Voxels.Fillet(float)`                                   | instance | distance-field fillet |
|  [21]   | `Voxels.Smoothen(float)`                                 | instance | curvature smoothing   |
|  [22]   | `Voxels.Trim(BBox3)`                                     | instance | bounding-box crop     |
|  [23]   | `Voxels.ProjectZSlice(float, float)`                     | instance | Z-band projection     |

- `Voxels.voxShell`: also `(float, float, float)` for asymmetric walls with inner smoothing.

[ENTRYPOINT_SCOPE]: mesh extraction, ray-cast, properties — `Voxels`

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]        |
| :-----: | :---------------------------------------------------------------------- | :------- | :------------------ |
|  [01]   | `Voxels.mshAsMesh() -> Mesh`                                            | instance | watertight mesh     |
|  [02]   | `Mesh(in Voxels)`                                                       | ctor     | watertight mesh     |
|  [03]   | `Voxels.CalculateProperties(out float, out BBox3)`                      | instance | solid metrics       |
|  [04]   | `Voxels.oCalculateBoundingBox() -> BBox3`                               | instance | field bounds        |
|  [05]   | `Voxels.bIsInside(Vector3) -> bool`                                     | instance | point membership    |
|  [06]   | `Voxels.bRayCastToSurface(in Vector3, in Vector3, out Vector3) -> bool` | instance | ray-hit status      |
|  [07]   | `Voxels.vecRayCastToSurface(in Vector3, in Vector3) -> Vector3`         | instance | ray-hit point       |
|  [08]   | `Voxels.bIsEmpty() -> bool`                                             | instance | emptiness           |
|  [09]   | `Voxels.bIsEqual(in Voxels) -> bool`                                    | instance | structural equality |
|  [10]   | `Voxels.nMemUsage() -> long`                                            | instance | native footprint    |
|  [11]   | `Voxels.voxDuplicate() -> Voxels`                                       | instance | deep copy           |

[ENTRYPOINT_SCOPE]: AM layer stack — grayscale slices and vector `.cli` program

| [INDEX] | [SURFACE]                                                                               | [SHAPE]  | [CAPABILITY]          |
| :-----: | :-------------------------------------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `Voxels.imgAllocateSlice(out int, ESliceAxis) -> ImageGrayScale`                        | instance | grayscale buffer      |
|  [02]   | `Voxels.nSliceCount() -> int`                                                           | instance | layer count           |
|  [03]   | `Voxels.GetVoxelSlice(in int, ref ImageGrayScale, ESliceMode, ESliceAxis)`              | instance | voxel-grid mask       |
|  [04]   | `Voxels.GetInterpolatedVoxelSlice(in float, ref ImageGrayScale, ESliceMode)`            | instance | interpolated mask     |
|  [05]   | `Voxels.oVectorize(float, bool, IProgress) -> PolySliceStack`                           | instance | closed-contour stack  |
|  [06]   | `CliIo.WriteSlicesToCliFile(PolySliceStack, string, EFormat, string, float, IProgress)` | static   | vector `.cli` program |
|  [07]   | `Vdb2Cli.Convert(string, float, string, string, IProgress)`                             | static   | direct `.cli` posting |
|  [08]   | `PolySlice.SaveToSvgFile(string, bool)`                                                 | instance | layer SVG             |
|  [09]   | `PolyContour.AsSvgPath(out string)`                                                     | instance | contour SVG path      |

- TWO output paths from a finished `Voxels`: grayscale mask slices (`GetVoxelSlice` -> `ImageGrayScale`) for SLA/DLP/MSLA exposure, and vector contour slices (`oVectorize` -> `PolySliceStack` -> `CliIo`/`Vdb2Cli`) for `.cli`/SVG.

[ENTRYPOINT_SCOPE]: VDB field I/O and the runtime library

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]        |
| :-----: | :------------------------------------------------------------- | :------- | :------------------ |
|  [01]   | `Library(float)`                                               | ctor     | explicit library    |
|  [02]   | `Library.GlobalInstance(float, string, string, string)`        | ctor     | ambient library     |
|  [03]   | `Library.oLibrary() -> Library`                                | static   | ambient resolve     |
|  [04]   | `OpenVdbFile(string)`                                          | ctor     | VDB read container  |
|  [05]   | `OpenVdbFile()`                                                | ctor     | VDB write container |
|  [06]   | `OpenVdbFile.nAdd(Voxels, string) -> int`                      | instance | named field append  |
|  [07]   | `OpenVdbFile.voxGet(int) -> Voxels`                            | instance | voxel field read    |
|  [08]   | `OpenVdbFile.oGetScalarField(int) -> ScalarField`              | instance | scalar field read   |
|  [09]   | `OpenVdbFile.oGetVectorField(int) -> VectorField`              | instance | vector field read   |
|  [10]   | `OpenVdbFile.SaveToFile(string)`                               | instance | VDB write           |
|  [11]   | `OpenVdbFile.libCreateCompatibleLibraryFor(string) -> Library` | static   | native-size library |
|  [12]   | `OpenVdbFile.bIsPicoGKCompatible() -> bool`                    | instance | compatibility flag  |
|  [13]   | `OpenVdbFile.fPicoGKVoxelSizeMM() -> float`                    | instance | native voxel size   |
|  [14]   | `Voxels.voxFromVdbFile(string) -> Voxels`                      | static   | direct field load   |
|  [15]   | `FieldMetadata.SetValue(...)`                                  | instance | typed value write   |
|  [16]   | `FieldMetadata.bGetValueAt(...) -> bool`                       | instance | typed value read    |

- `OpenVdbFile.nAdd` and `oGetScalarField`/`oGetVectorField` overload on `Voxels`, `ScalarField`, and `VectorField`; `FieldMetadata.SetValue`/`bGetValueAt` carry `string`, `float`, and `Vector3` values through VDB.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- ALC FIREBREAK: `PicoGK` binds a RID-bearing native core (`picogk.26.2.*` over boost/TBB/icu/zstd) and never loads inside an in-Rhino plugin ALC; the AM rail runs it in the `Rasm.AppHost` sidecar and marshals the extracted `Mesh` and slice programs back across the wire.
- LIBRARY LIFECYCLE: one `Library` bound to one `fVoxelSizeMM` owns every field, mesh, and lattice allocation. `new Library.GlobalInstance(fVoxelSizeMM, ...)` binds the ambient instance the parameterless ctors resolve through `Library.oLibrary()`; an explicit `Library` passes to the `(Library, ...)` overloads. A voxel-size change is a new `Library`, and fields from two libraries never mix.
- DISPOSAL: every field/mesh/lattice/file/library handle is `IDisposable` over a native object and disposes through `using` or a resource rail; `Library.n*MemUsage()`/`n*Allocated()` is the native-resource census. Allocation failure throws `PicoGKAllocException`, a native-version mismatch `PicoGKLibraryMismatchException`.
- MUTATE-VS-COPY: each Boolean or morphology op pairs a mutating verb (`BoolAdd`, `Offset`) with a copy verb (`voxBoolAdd`, `voxOffset`) returning a fresh `Voxels`; shelling ships copy-only as `voxShell`, so an owned pipeline swaps the lease and disposes the source. Distances are MM.

[STACKING]:
- kernel `MeshSpace` (within-lib): `Voxels.mshAsMesh()` extracts a watertight mesh that crosses to the kernel mesh vocabulary for downstream slicing, posting, or display; `PicoGK` extracts and hands off, leaving the planar mesh-section to the kernel `Meshing/slice` stack.
- `Clipper2`(`../../.api/api-clipper2.md`)/`CavalierContours`(`.api/api-cavaliercontours.md`): disjoint by boundary — those own planar FFF 2D-perimeter Boolean/offset and arc-native toolpaths, `PicoGK` owns the voxel/SDF/layer-stack AM lane, neither surface touched.
- IMPLICIT INFILL: a consumer's gyroid/TPMS/cellular `IImplicit.fSignedDistance` rasterizes through `Voxels(IImplicit, BBox3)` and intersects (`BoolIntersect`/`IntersectImplicit`) the part shell — the conformal-infill primitive the rectilinear/honeycomb FFF infill owner cannot express; a `VectorField` drives anisotropic or graded density.
- CONFORMAL SUPPORT: `Lattice.AddBeam`/`AddSphere` rasterizes through `Voxels(Lattice)`, and `BoolSubtract` of the part yields the overhang support-minus-part solid.

[LOCAL_ADMISSION]:
- `PicoGK` admits the implicit/voxel/SDF concern — TPMS/gyroid/cellular conformal infill, overhang lattices, true-3D shell/fillet/over-offset, and the resin/powder grayscale-plus-vector layer stack, results handed back as an extracted `Mesh` or a slice program. Planar FFF perimeters and 2D toolpaths admit `Clipper2`/`CavalierContours` instead.

[RAIL_LAW]:
- Package: `PicoGK` (assembly `PicoGK`, native core `picogk.26.2`)
- Owns: the implicit/SDF/voxel AM kernel — `IImplicit` rasterization to `Voxels`, 3D SDF Boolean and distance morphology, `Lattice` scaffold rasterization, dual-contour `Mesh` extraction, SDF ray-cast and volume/bbox metrics, the grayscale (`GetVoxelSlice`) and vector (`oVectorize`/`CliIo`/`Vdb2Cli`) SLA/DLP/MSLA layer stack, `ScalarField`/`VectorField` sampled fields with `FieldMetadata`, and `OpenVdbFile` round-trip, over a native core in the sidecar host.
- Accept: the AM implicit/voxel concern handed back as an extracted `Mesh` or a slice program.
- Reject: plugin-domain references; undisposed `Voxels`/field/mesh/`Library`/VDB handles; mixed-library voxel sizes; a bare `picogk.dylib` name; the planar 2D-perimeter concern `Clipper2`/`CavalierContours` own; and a hand-rolled mesh or slice hand-off the kernel `MeshSpace` and the `Query/cache` artifact index (`ContentHash.Of`) already own.
