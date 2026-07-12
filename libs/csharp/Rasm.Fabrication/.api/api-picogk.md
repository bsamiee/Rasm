# [RASM_FABRICATION_API_PICOGK]

`PicoGK` (LEAP71) is the implicit/SDF/voxel geometry kernel for additive manufacturing — a thin managed front over an OpenVDB/boost/TBB native core (`picogk.26.2.dylib`): the `IImplicit.fSignedDistance` field contract feeds `Voxels(IImplicit, BBox3)` to rasterize TPMS/gyroid/cellular infill the rectilinear/honeycomb quartet cannot express; `Lattice.AddBeam`/`AddSphere` rasterizes overhang-conditioned support scaffolds into `Voxels(Lattice)`; the `Voxels` field owns 3D SDF Boolean (`BoolAdd`/`BoolSubtract`/`BoolIntersect` + operators), distance morphology (`Offset`/`DoubleOffset`/`TripleOffset`/`OverOffset`/`Shell`/`Fillet`/`Smoothen`), and dual-contoured mesh extraction (`mshAsMesh`); `GetVoxelSlice`/`GetInterpolatedVoxelSlice` rasterize the grayscale SLA/DLP/MSLA layer stack and `oVectorize`/`CliIo`/`Vdb2Cli` emit the `.cli` vector slice program (the resin/powder path the planar-only FFF `Section` never reaches); `OpenVdbFile` round-trips fields to disk; and `ScalarField`/`VectorField` carry sampled scalar/vector data with `FieldMetadata`. The package is OWNED by `Rasm.Fabrication` (`Rasm.Fabrication.csproj`). It is a COMPANION / OUTSIDE-RHINO owner: `lib/net9.0`-only plus the bundled `runtimes/<rid>/native/picogk.26.2.*` native core firebreaks it out of any net48 in-Rhino plugin ALC — it runs in the sidecar/AppHost host, never inside the RhinoCommon plugin.

`PicoGK` COMPOSES — does NOT replace — the planar fabrication owners: it leaves the `Clipper2` 2D perimeter Boolean/offset substrate untouched, and the kernel planar mesh-section path (`Meshing/slice`) intact. Its lane is the implicit/voxel/SDF concern those line/mesh owners cannot reach — TPMS infill, conformal lattices, distance-field morphology (true 3D shelling/filleting/over-offset), and the resin/powder grayscale-and-vector layer stack. The boundary is sharp: planar FFF perimeters and 2D toolpaths stay on `Clipper2`/`CavalierContours`; the `Voxels`/`IImplicit` SDF concern stays here. The extracted `Mesh` is the wire-meeting point — a `Voxels.mshAsMesh()` result crosses to the kernel `MeshSpace` vocabulary for downstream consumption.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PicoGK`

- package: `PicoGK`
- license: `Apache-2.0` (LEAP71; nuspec `<license type="expression">Apache-2.0</license>`)
- assembly: `PicoGK`
- namespace: `PicoGK` (single namespace; rich XML doc ships as `lib/net9.0/PicoGK.xml`)
- asset: `lib/net9.0/PicoGK.dll` ONLY — NO net10 asset; the `net10.0` consumer binds the `net9.0` fallback (forward-compatible). Plus the native core under `runtimes/<rid>/native/`: on `osx-arm64` `picogk.26.2.dylib` (the OpenVDB/boost/TBB engine) + its `libboost_*`/`libicu*`/`liblzma`/`libzstd` companions; on `win-x64` `picogk.26.2.dll` + `tbb12`/`blosc`/`lz4`/`z`/`zstd` — this is the RID-bearing, ALC-firebreaking native dependency
- native lib id: `Config.strPicoGKLib = "picogk.26.2"` (the P/Invoke target name; the loaded library version is `26.2`, NOT a bare `picogk.dylib`); a host/native-version mismatch throws `PicoGKLibraryMismatchException`
- dependencies: `SkiaSharp 3.119.0` (the image/raster dependency, `exclude="Build,Analyzers"`) — rides the App-UI SkiaSharp row already pinned
- runtime model: `IDisposable` field/mesh/lattice handles wrapping native objects — every `Voxels`/`ScalarField`/`VectorField`/`Mesh`/`Lattice`/`OpenVdbFile`/`Library` MUST be disposed (the native core leaks otherwise); allocation failure throws `PicoGKAllocException`
- owner: `Rasm.Fabrication.csproj`
- rail: fabrication (companion / outside-Rhino, AM)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the implicit (SDF) contract — the field-function entry

- rail: fabrication

`IImplicit` defines `float fSignedDistance(in Vector3 vec)` for arbitrary implicit solids; negative values lie inside and positive values lie outside. `IBoundedImplicit : IImplicit` adds `BBox3 oBounds { get; }`, so `Voxels(IBoundedImplicit)` reads its extent while an unbounded `IImplicit` requires an explicit `BBox3`. `ScalarField.TraverseActive` and `VectorField.TraverseActive` invoke their traversal contracts once per populated voxel.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]  | [CAPABILITY]          |
| :-----: | :--------------------- | :------------- | :-------------------- |
|  [01]   | `IImplicit`            | SDF contract   | signed-distance field |
|  [02]   | `IBoundedImplicit`     | bounded SDF    | self-bounding field   |
|  [03]   | `ITraverseScalarField` | scalar visitor | active-voxel callback |
|  [04]   | `ITraverseVectorField` | vector visitor | active-voxel callback |

[PUBLIC_TYPE_SCOPE]: the field owners — voxels and sampled fields

- rail: fabrication

`Voxels` owns the narrow-band signed-distance grid, including SDF Boolean, distance morphology, slice rasterization, mesh extraction, ray-cast, property, and bounding-box queries. `ScalarField` is an `IImplicit` sampled field built from `Voxels`; `fSignedDistance` reads its sampled value, `SetValue`/`bGetValue`/`RemoveValue` address `Vector3` positions, `TraverseActive` visits populated voxels, and `GetVoxelSlice` writes `ImageGrayScale`. `VectorField` carries sparse sampled `Vector3` values for drivers such as thermal gradients or fibre orientation through `SetValue`, `bGetValue`, `RemoveValue`, and `TraverseActive`. `FieldMetadata` stores `EType` String/Float/Vector values through `SetValue` and `bGetValueAt` overloads for `string`, `float`, and `Vector3`, and VDB preserves this provenance and parameter carrier. `IFieldWithMetadata` exposes `FieldMetadata oMetaData()` on `Voxels`, `ScalarField`, and `VectorField`.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]                             | [CAPABILITY]          |
| :-----: | :------------------- | :---------------------------------------- | :-------------------- |
|  [01]   | `Voxels`             | SDF field (`IDisposable`)                 | voxel geometry owner  |
|  [02]   | `ScalarField`        | scalar field (`IImplicit`, `IDisposable`) | sampled scalar values |
|  [03]   | `VectorField`        | vector field (`IDisposable`)              | sampled vector values |
|  [04]   | `FieldMetadata`      | metadata (`IDisposable`)                  | typed field values    |
|  [05]   | `IFieldWithMetadata` | metadata contract                         | metadata access       |

[PUBLIC_TYPE_SCOPE]: scaffolds, meshes, lines — the geometry carriers

- rail: fabrication

`Lattice.AddBeam(vecA, fRadA, vecB, fRadB, bRoundCap)` and `AddSphere(vecCenter, fRadius)` build capsule-and-sphere CSG that `Voxels(Lattice)` or `RenderLattice` rasterizes for conformal support scaffolds and strut lattices. `Mesh` is the extraction target and STL bridge: `nAddVertex`/`AddVertices`/`nAddTriangle`/`AddQuad` construct it; `vecVertexAt`/`oTriangleAt`/`GetTriangle` and `nVertexCount`/`nTriangleCount` expose indexed geometry; `mshCreateTransformed(Matrix4x4)`, `mshCreateMirrored`, `Append`, `oBoundingBox`, and `mshFromStlFile(path, EStlUnit, fPostScale, ...)` transform, combine, bound, and import it. `Mesh(in Voxels)` and `Voxels.mshAsMesh()` construct a mesh from a field. `PolyLine` owns `nAddVertex`/`Add(IEnumerable<Vector3>)`, `vecVertexAt`/`nVertexCount`, `AddArrow`/`AddCross`, and `oBoundingBox` for colored viewer geometry carried by `ColorFloat`. `Coord(int x,y,z)` carries a voxel index, while `Triangle(int a,b,c)` carries a mesh face by vertex index and remains distinct from the kernel `Triangle.NET` mesher.

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]                 | [CAPABILITY]        |
| :-----: | :--------- | :---------------------------- | :------------------ |
|  [01]   | `Lattice`  | scaffold (`IDisposable`)      | beam-and-sphere CSG |
|  [02]   | `Mesh`     | triangle mesh (`IDisposable`) | extraction target   |
|  [03]   | `PolyLine` | colored line (`IDisposable`)  | viewer geometry     |
|  [04]   | `Coord`    | voxel index                   | grid coordinate     |
|  [05]   | `Triangle` | face index                    | mesh triangle       |

[PUBLIC_TYPE_SCOPE]: VDB I/O, slice vectorization, CLI emission — the AM file/layer surface

- rail: fabrication

`OpenVdbFile` round-trips multiple `Voxels`, `ScalarField`, and `VectorField` values through `nAdd`, `voxGet`, `oGetScalarField`, and `oGetVectorField`; `nFieldCount`, `strFieldName`, `eFieldType`, `xField`, and `SaveToFile` expose and persist them. `bIsPicoGKCompatible`, `fPicoGKVoxelSizeMM`, and `libCreateCompatibleLibraryFor(path)` open foreign VDB data at its native voxel size. `Voxels.oVectorize(fLayerHeight)` returns `PolySliceStack`, whose `nCount`, `oSliceAt`, `oBBox`, `AddSlices`, and `AddToViewer` operations carry the layered resin or powder contour program. Each `PolySlice` owns one Z layer of closed `PolyContour` loops through `fZPos`, `AddContour`, `Close`, `bIsEmpty`, `SaveToSvgFile(path, bSolid)`, and `oFromSdf(Image, fZPos, vecOffset, fScale)`. Each `PolyContour` owns `oVertices`, `vecVertex`, `nCount`, `eDetectWinding`/`DetectWinding`/`eWinding`, `AsSvgPolyline`/`AsSvgPath`, and `oBBox`. `CliIo` writes and reads Common Layer Interface programs through `WriteSlicesToCliFile(PolySliceStack, path, EFormat, strDate, fUnitsInMM, IProgress)` and `oSlicesFromCliFile(path)`, whose `Result` carries slices, bounds, and header. `Vdb2Cli.Convert(strVdbFilePath, fCliLayerHeight, strCliFilePath, strVoxelFieldName, IProgress)` bridges a VDB field directly to `.cli`.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]                 | [CAPABILITY]         |
| :-----: | :--------------- | :---------------------------- | :------------------- |
|  [01]   | `OpenVdbFile`    | VDB container (`IDisposable`) | multi-field VDB I/O  |
|  [02]   | `PolySliceStack` | vector slice stack            | layered contours     |
|  [03]   | `PolySlice`      | vector layer                  | closed contour layer |
|  [04]   | `PolyContour`    | 2D contour                    | closed loop          |
|  [05]   | `CliIo`          | `.cli` I/O                    | slice-program I/O    |
|  [06]   | `Vdb2Cli`        | VDB-to-CLI bridge             | direct conversion    |
|  [07]   | `TgaIo`          | image I/O                     | TGA slice raster     |

[PUBLIC_TYPE_SCOPE]: runtime, geometry primitives, faults

- rail: fabrication

`Library` owns the native-core lifecycle at one `fVoxelSizeMM`; every field and mesh allocation belongs to that library, `Library.GlobalInstance(fVoxelSizeMM, logPath, viewerTitle, env)` sets the ambient instance, and `Library.oLibrary()` reads it. Its `n*MemUsage()` and `n*Allocated()` members report native resources, and `vecVoxelsToMm` converts coordinates. `Config.strPicoGKLib = "picogk.26.2"` names the P/Invoke library. `Viewer` carries the interactive OpenGL camera, arcball, key and mouse callbacks, sidebar, and animation for sidecar visual debugging outside headless posting. `BBox3` owns `vecMin`/`vecMax`, `vecSize`/`vecCenter`, `Include`/`Grow`/`bContains`/`bIsEmpty`, and `oFitInto(bounds, out fScale, out vecOffset)`. The color family and `Image*` carry slice-raster color and image buffers, with `ImageGrayScale` as the slice target. `PicoGKAllocException` reports native allocation failure, while `PicoGKLibraryMismatchException` reports a native-library version mismatch.

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

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: implicit -> voxels rasterization

- rail: fabrication
- note: an unbounded `IImplicit` needs an explicit `BBox3`; an `IBoundedImplicit` carries its own. The voxel resolution is the `Library` `fVoxelSizeMM` — finer voxels = sharper SDF surface at higher native memory.

`Voxels.RenderImplicit(in IImplicit, in BBox3)` adds an implicit to an existing field, while `IntersectImplicit(in IImplicit)` intersects it and `voxIntersectImplicit` returns the copy form. `Voxels.voxSphere`, `voxLatticeBeam`, and `voxMeshShell(msh, fRadius)` each expose ambient-library and explicit-`Library` overloads.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]         |
| :-----: | :----------------------------------------------------- | :------------- | :------------------- |
|  [01]   | `new Voxels(in IImplicit xImplicit, in BBox3 oBounds)` | SDF raster     | boxed implicit       |
|  [02]   | `new Voxels(in IBoundedImplicit xImplicit)`            | SDF raster     | bounded implicit     |
|  [03]   | `Voxels.RenderImplicit`                                | SDF compose    | field addition       |
|  [04]   | `IntersectImplicit`                                    | SDF compose    | field intersection   |
|  [05]   | `voxIntersectImplicit`                                 | SDF compose    | copied intersection  |
|  [06]   | `new Voxels(in Lattice lat)`                           | lattice raster | scaffold field       |
|  [07]   | `Voxels.RenderLattice`                                 | lattice raster | scaffold composition |
|  [08]   | `new Voxels(in Mesh msh)`                              | mesh raster    | mesh field           |
|  [09]   | `Voxels.RenderMesh`                                    | mesh raster    | mesh composition     |
|  [10]   | `Voxels.voxSphere`                                     | factory        | sphere field         |
|  [11]   | `voxLatticeBeam`                                       | factory        | beam field           |
|  [12]   | `voxMeshShell`                                         | factory        | mesh-shell field     |

[ENTRYPOINT_SCOPE]: SDF Boolean and distance morphology — `Voxels`

- rail: fabrication
- note: each operation has a MUTATING form (`BoolAdd`, `Offset`, ...) and a COPY form (`voxBoolAdd`, `voxOffset`, ...) returning a new `Voxels`; operators `+`/`-`/`&` are the copy Boolean. Distances are in MM (converted through the `Library` voxel size).

`BoolAdd`, `BoolSubtract`, and `BoolIntersect` consume `in Voxels`; `BoolAddAll` and `BoolSubtractAll(IEnumerable<Voxels>)` apply batch Boolean operations, while static `voxCombine` and `voxCombineAll` return batch unions. `Offset(float fDistMM)` performs uniform SDF dilation or erosion, and a negative distance shrinks the field. `DoubleOffset(fDist1MM, fDist2MM)` and `TripleOffset(fDistMM)` perform open-or-close conditioning across two or three stages. `OverOffset(fFirstOffsetMM, fFinalSurfaceDistInMM)` offsets outward and returns to a controlled surface distance. `Shell` accepts either `fOffset` or `(fNegMM, fPosMM, fSmoothInnerMM)` for asymmetric walls and inner smoothing. `Fillet(fRoundingMM)` rounds the distance field, `Smoothen(fDistMM)` applies mean-curvature smoothing, `Trim(BBox3)` crops the field, and `ProjectZSlice(fStartZMM, fEndZMM)` projects a Z band into a 2D-extruded slab.

| [INDEX] | [SURFACE]          | [ENTRY_FAMILY] | [CAPABILITY]          |
| :-----: | :----------------- | :------------- | :-------------------- |
|  [01]   | `BoolAdd`          | SDF Boolean    | in-place union        |
|  [02]   | `voxBoolAdd`       | SDF Boolean    | copied union          |
|  [03]   | `operator +`       | SDF Boolean    | copied union          |
|  [04]   | `BoolSubtract`     | SDF Boolean    | in-place subtraction  |
|  [05]   | `voxBoolSubtract`  | SDF Boolean    | copied subtraction    |
|  [06]   | `operator -`       | SDF Boolean    | copied subtraction    |
|  [07]   | `BoolIntersect`    | SDF Boolean    | in-place intersection |
|  [08]   | `voxBoolIntersect` | SDF Boolean    | copied intersection   |
|  [09]   | `operator &`       | SDF Boolean    | copied intersection   |
|  [10]   | `BoolAddAll`       | SDF Boolean    | batch union           |
|  [11]   | `BoolSubtractAll`  | SDF Boolean    | batch subtraction     |
|  [12]   | `voxCombine`       | SDF Boolean    | copied combination    |
|  [13]   | `voxCombineAll`    | SDF Boolean    | copied batch union    |
|  [14]   | `Offset`           | distance       | signed field offset   |
|  [15]   | `voxOffset`        | distance       | copied field offset   |
|  [16]   | `DoubleOffset`     | morphology     | two-stage offset      |
|  [17]   | `TripleOffset`     | morphology     | three-stage offset    |
|  [18]   | `OverOffset`       | morphology     | surface conditioning  |
|  [19]   | `Shell`            | shelling       | wall construction     |
|  [20]   | `Fillet`           | smoothing      | distance-field fillet |
|  [21]   | `Smoothen`         | smoothing      | curvature smoothing   |
|  [22]   | `Trim`             | crop           | bounding-box crop     |
|  [23]   | `ProjectZSlice`    | project        | Z-band projection     |

[ENTRYPOINT_SCOPE]: mesh extraction, ray-cast, properties — `Voxels`

- rail: fabrication

`Voxels.mshAsMesh()` and `new Mesh(in Voxels)` dual-contour an SDF into a watertight triangle `Mesh` for the kernel `MeshSpace` vocabulary. `CalculateProperties(out float fVolumeCubicMM, out BBox3)` returns solid volume and bounds for AM mass or cost estimates, and `oCalculateBoundingBox()` returns the bounds alone. `bIsInside(Vector3)` tests membership. `bRayCastToSurface(vecSearch, vecDir, out vecSurfacePoint)` and `vecRayCastToSurface` locate the SDF surface. `bIsEmpty()`, `bIsEqual(in Voxels)`, `nMemUsage()`, and `voxDuplicate()` expose emptiness, structural equality, native memory, and deep copying.

| [INDEX] | [SURFACE]               | [ENTRY_FAMILY] | [CAPABILITY]        |
| :-----: | :---------------------- | :------------- | :------------------ |
|  [01]   | `Voxels.mshAsMesh`      | extraction     | watertight mesh     |
|  [02]   | `new Mesh(in Voxels)`   | extraction     | watertight mesh     |
|  [03]   | `CalculateProperties`   | metrics        | solid metrics       |
|  [04]   | `oCalculateBoundingBox` | metrics        | field bounds        |
|  [05]   | `bIsInside`             | query          | point membership    |
|  [06]   | `bRayCastToSurface`     | query          | ray-hit status      |
|  [07]   | `vecRayCastToSurface`   | query          | ray-hit point       |
|  [08]   | `bIsEmpty`              | state          | emptiness           |
|  [09]   | `bIsEqual`              | state          | structural equality |
|  [10]   | `nMemUsage`             | state          | native footprint    |
|  [11]   | `voxDuplicate`          | state          | deep copy           |

[ENTRYPOINT_SCOPE]: AM layer stack — grayscale slices and vector `.cli` program

- rail: fabrication
- note: TWO output paths from a finished `Voxels` solid: (a) GRAYSCALE slice images for SLA/DLP/MSLA mask exposure (`GetVoxelSlice` -> `ImageGrayScale`); (b) VECTOR contour slices for `.cli`/SVG (`oVectorize` -> `PolySliceStack` -> `CliIo`). This is the resin/powder posting path the planar-only FFF `Section` owner never reaches.

`imgAllocateSlice(out int nSliceCount, ESliceAxis eAxis = Z)` allocates the grayscale buffer, and `nSliceCount()` reports the layer count. `GetVoxelSlice(in int nSlice, ref ImageGrayScale img, ESliceMode = SignedDistance, ESliceAxis = Z)` rasterizes a voxel-grid layer for SLA or DLP exposure. `GetInterpolatedVoxelSlice(in float fZSlice, ref ImageGrayScale img, ESliceMode = SignedDistance)` rasterizes an arbitrary sub-voxel Z layer. `oVectorize(float fLayerHeight = 0f, bool bUseAbsXYOrigin = false, IProgress?)` extracts the solid-wide closed-contour stack. `CliIo.WriteSlicesToCliFile(PolySliceStack, path, EFormat, strDate, fUnitsInMM, IProgress?)` admits `EFormat.FirstLayerWithContent` and `UseEmptyFirstLayer`. `Vdb2Cli.Convert(vdbPath, fCliLayerHeight, cliPath, voxelFieldName, IProgress?)` posts a VDB field directly to `.cli`. `PolySlice.SaveToSvgFile(path, bSolid)` emits a complete layer, while `PolyContour.AsSvgPath(out str)` emits one contour path for pen plotting or mask preview.

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY] | [CAPABILITY]         |
| :-----: | :--------------------------- | :------------- | :------------------- |
|  [01]   | `imgAllocateSlice`           | slice alloc    | grayscale buffer     |
|  [02]   | `nSliceCount`                | slice alloc    | layer count          |
|  [03]   | `GetVoxelSlice`              | grayscale      | voxel-grid mask      |
|  [04]   | `GetInterpolatedVoxelSlice`  | grayscale      | interpolated mask    |
|  [05]   | `oVectorize`                 | vectorize      | closed-contour stack |
|  [06]   | `CliIo.WriteSlicesToCliFile` | CLI emit       | vector slice program |
|  [07]   | `Vdb2Cli.Convert`            | VDB-to-CLI     | direct posting       |
|  [08]   | `PolySlice.SaveToSvgFile`    | SVG emit       | layer SVG            |
|  [09]   | `PolyContour.AsSvgPath`      | SVG emit       | contour path         |

[ENTRYPOINT_SCOPE]: VDB field I/O and the runtime library

- rail: fabrication

`new Library(float fVoxelSizeMM)` binds the native core to one voxel resolution, while `Library.GlobalInstance(fVoxelSizeMM, ...)` sets the ambient library that parameterless field constructors resolve through `Library.oLibrary()`. `OpenVdbFile` reads and writes multiple named fields through `nAdd(field, name)`, `voxGet`, `oGetScalarField`, `oGetVectorField`, and `SaveToFile(path)`. `OpenVdbFile.libCreateCompatibleLibraryFor(vdbPath)`, `bIsPicoGKCompatible`, and `fPicoGKVoxelSizeMM` preserve a foreign VDB's native voxel size. `Voxels.voxFromVdbFile(path)` loads one field directly. `FieldMetadata.SetValue` and `bGetValueAt` carry `string`, `float`, and `Vector3` values through VDB.

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]        |
| :-----: | :------------------------------------------ | :------------- | :------------------ |
|  [01]   | `new Library(float fVoxelSizeMM)`           | runtime init   | explicit library    |
|  [02]   | `Library.GlobalInstance`                    | runtime init   | ambient library     |
|  [03]   | `new OpenVdbFile(path)`                     | VDB I/O        | VDB container       |
|  [04]   | `nAdd`                                      | VDB I/O        | named field append  |
|  [05]   | `voxGet`                                    | VDB I/O        | voxel field read    |
|  [06]   | `oGetScalarField`                           | VDB I/O        | scalar field read   |
|  [07]   | `oGetVectorField`                           | VDB I/O        | vector field read   |
|  [08]   | `SaveToFile`                                | VDB I/O        | VDB write           |
|  [09]   | `OpenVdbFile.libCreateCompatibleLibraryFor` | VDB compat     | native-size library |
|  [10]   | `bIsPicoGKCompatible`                       | VDB compat     | compatibility flag  |
|  [11]   | `fPicoGKVoxelSizeMM`                        | VDB compat     | native voxel size   |
|  [12]   | `Voxels.voxFromVdbFile`                     | VDB load       | direct field load   |
|  [13]   | `FieldMetadata.SetValue`                    | metadata       | typed value write   |
|  [14]   | `bGetValueAt`                               | metadata       | typed value read    |

## [04]-[IMPLEMENTATION_LAW]

[RUNTIME_TOPOLOGY]:

- ALC FIREBREAK: `PicoGK` is `net9.0`-only with a RID-bearing native core (`picogk.26.2.dylib` + boost/TBB/icu/zstd on osx-arm64) — it CANNOT load inside a net48 in-Rhino plugin ALC. The Fabrication AM rail runs it in the sidecar / `Rasm.AppHost` host process, marshalling results (extracted `Mesh`, slice programs) back across the wire. Never reference `PicoGK` from a type that must load in the RhinoCommon plugin domain.
- LIBRARY LIFECYCLE: every `Voxels`/`ScalarField`/`VectorField`/`Mesh`/`Lattice`/`OpenVdbFile` is owned by a `Library` bound to ONE `fVoxelSizeMM` (the global voxel resolution). Set it once via `Library.GlobalInstance(fVoxelSizeMM, ...)` (which the parameterless ctors resolve through `Library.oLibrary()`), or pass an explicit `Library` to the `(Library, ...)` ctor/factory overloads. Changing voxel size means a new `Library`. Mixing fields from two libraries of different voxel sizes is invalid.
- DISPOSAL: every field/mesh/lattice/file/library handle is `IDisposable` over a NATIVE object — wrap in `using` or a resource rail; an undisposed handle leaks native memory the GC cannot reclaim. The `Library.n*MemUsage()`/`n*Allocated()` census is the native-resource receipt for verifying clean teardown. Allocation failure throws `PicoGKAllocException`; a native-version mismatch throws `PicoGKLibraryMismatchException`.
- MUTATE-VS-COPY: the `Voxels` operations come in pairs — the verb form (`BoolAdd`, `Offset`, `Shell`) mutates in place and returns `void`; the `vox`-prefixed form (`voxBoolAdd`, `voxOffset`, `voxShell`) returns a fresh `Voxels`. Choose the mutating form inside an owned pipeline (avoids per-step native allocation) and the copy form when the input must survive. All distances are MM.

[STACKING_LAW]:

- IMPLICIT INFILL the planar quartet cannot express: a consumer's gyroid/TPMS/cellular `IImplicit.fSignedDistance` -> `Voxels(IImplicit, BBox3)`, intersected (`BoolIntersect`/`IntersectImplicit`) with the part shell, is the conformal-infill primitive the rectilinear/honeycomb FFF infill owner has no expression for. A `VectorField` can drive anisotropic/graded infill (the field is the orientation/density driver sampled by the implicit).
- CONFORMAL SUPPORT: `Lattice.AddBeam`/`AddSphere` -> `Voxels(Lattice)` is the overhang-conditioned strut-scaffold builder; `BoolSubtract` the part to get the support-minus-part solid.
- THE WIRE MEETS AT `Mesh`: `Voxels.mshAsMesh()` extracts a watertight mesh that crosses to the kernel `MeshSpace` vocabulary for downstream slicing, posting, or display; line-sourced section chains route to the owned biarc fold (`Posting/program`) and the kernel mesh vocabulary, never a package mesh owner. PicoGK does NOT own the planar mesh-section (the kernel `Meshing/slice` slice-stack owns it) — it extracts the mesh and hands off.
- THE AM POSTING PATH stays HERE, disjoint from `Clipper2`/`CavalierContours`: the resin/powder grayscale (`GetVoxelSlice`) and vector (`oVectorize` -> `CliIo`/`Vdb2Cli` `.cli`) layer stack is the SLA/DLP/MSLA posting the planar-only FFF `Section`/`Clipper2` 2D-perimeter path never reaches. The `Clipper2` 2D Boolean/offset substrate and the `CavalierContours` arc-native offsetting are UNTOUCHED — they own planar FFF toolpaths; PicoGK owns the voxel/SDF/layer-stack AM lane.

[RAIL_LAW]:

- Package: `PicoGK` (assembly `PicoGK`, native core `picogk.26.2`)
- Owns: the implicit/SDF/voxel AM kernel — `IImplicit` field rasterization to `Voxels`, 3D SDF Boolean + distance morphology (offset/double-offset/over-offset/shell/fillet/smoothen), `Lattice` scaffold rasterization, dual-contour `Mesh` extraction, SDF ray-cast + volume/bbox metrics, the grayscale (`GetVoxelSlice`) and vector (`oVectorize`/`CliIo`/`Vdb2Cli` `.cli`) SLA/DLP/MSLA layer stack, `ScalarField`/`VectorField` sampled fields with `FieldMetadata`, and `OpenVdbFile` round-trip — all in the sidecar/host process over a native core
- Accept: the additive-manufacturing implicit/voxel concern — TPMS/gyroid/cellular conformal infill, overhang lattices, true-3D shelling/filleting/over-offset, and the resin/powder grayscale-plus-vector layer-stack posting; results handed back as an extracted `Mesh` or a slice program
- Reject: plugin-domain references; calls into the owned biarc fold, kernel mesh vocabulary, artifact-index `Query/cache#ARTIFACT_BLOB_INDEX` posture via `ContentHash.Of`, or 2D-perimeter substrate (`Clipper2`/`CavalierContours`); undisposed `Voxels`/field/mesh/library/VDB handles; mixed-library voxel sizes; bare `picogk.dylib` names.
