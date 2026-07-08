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

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]      | [CAPABILITY]                                                                                  |
| :-----: | :-------------------- | :----------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `IImplicit`           | SDF contract       | the single member `float fSignedDistance(in Vector3 vec)` — the signed-distance field a consumer implements to define a gyroid/TPMS/cellular/arbitrary implicit solid; negative inside, positive outside |
|  [02]   | `IBoundedImplicit`    | bounded SDF        | `: IImplicit` + `BBox3 oBounds { get; }` — a self-bounding implicit (the `Voxels(IBoundedImplicit)` ctor reads its own extent; the unbounded `IImplicit` needs an explicit `BBox3`) |
|  [03]   | `ITraverseScalarField`/`ITraverseVectorField` | traversal visitor | the active-voxel visitor callbacks `ScalarField.TraverseActive`/`VectorField.TraverseActive` invoke per populated voxel |

[PUBLIC_TYPE_SCOPE]: the field owners — voxels and sampled fields
- rail: fabrication

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [CAPABILITY]                                                                                  |
| :-----: | :------------------ | :----------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `Voxels`            | SDF/voxel field (`IDisposable`) | THE core owner — a narrow-band signed-distance voxel grid: SDF Boolean, distance morphology, slice rasterization, mesh extraction, ray-cast, property/bbox query (see entrypoints); `IFieldWithMetadata` so it carries `FieldMetadata` |
|  [02]   | `ScalarField`       | scalar field (`IImplicit`, `IDisposable`) | a sparse sampled scalar field; itself an `IImplicit` (`fSignedDistance` reads the sampled value), built from `Voxels`; `SetValue`/`bGetValue`/`RemoveValue` per `Vector3`, `TraverseActive`, `GetVoxelSlice` to an `ImageGrayScale` |
|  [03]   | `VectorField`       | vector field (`IDisposable`) | a sparse sampled `Vector3` field (e.g. a thermal-gradient or fibre-orientation field driving anisotropic infill); `SetValue`/`bGetValue`/`RemoveValue`, `TraverseActive` |
|  [04]   | `FieldMetadata`     | field metadata (`IDisposable`) | a typed key->value bag (`EType` String/Float/Vector) on any field — `SetValue`/`bGetValueAt` over `string`/`float`/`Vector3`; the per-field provenance/parameter carrier round-tripped through VDB |
|  [05]   | `IFieldWithMetadata`| metadata contract  | `FieldMetadata oMetaData()` — implemented by `Voxels`/`ScalarField`/`VectorField` |

[PUBLIC_TYPE_SCOPE]: scaffolds, meshes, lines — the geometry carriers
- rail: fabrication

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [CAPABILITY]                                                                                  |
| :-----: | :------------------ | :----------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `Lattice`           | beam/sphere scaffold (`IDisposable`) | a CSG of capsules/spheres: `AddBeam(vecA, fRadA, vecB, fRadB, bRoundCap)` / `AddSphere(vecCenter, fRadius)` — rasterized to a solid by `Voxels(Lattice)` or `RenderLattice`; the conformal-support / strut-lattice builder |
|  [02]   | `Mesh`              | triangle mesh (`IDisposable`) | the extraction target and STL bridge: `nAddVertex`/`AddVertices`/`nAddTriangle`/`AddQuad`, `vecVertexAt`/`oTriangleAt`/`GetTriangle`, `nVertexCount`/`nTriangleCount`, `mshCreateTransformed(Matrix4x4)` / `mshCreateMirrored`, `Append`, `oBoundingBox`, `mshFromStlFile(path, EStlUnit, fPostScale, ...)`; built from `Voxels` via ctor `Mesh(in Voxels)` or `Voxels.mshAsMesh()` |
|  [03]   | `PolyLine`          | colored polyline (`IDisposable`) | `nAddVertex`/`Add(IEnumerable<Vector3>)`, `vecVertexAt`/`nVertexCount`, `AddArrow`/`AddCross`, `oBoundingBox` — a viewer/debug polyline with a `ColorFloat` |
|  [04]   | `Coord` / `Triangle`| voxel/index structs| `Coord(int x,y,z)` (a voxel index) and `Triangle(int a,b,c)` (a mesh face by vertex index) — the value carriers (NOTE: PicoGK's `Triangle` is index-based, distinct from the kernel `Triangle.NET` mesher) |

[PUBLIC_TYPE_SCOPE]: VDB I/O, slice vectorization, CLI emission — the AM file/layer surface
- rail: fabrication

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [CAPABILITY]                                                                                  |
| :-----: | :------------------ | :----------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `OpenVdbFile`       | VDB container (`IDisposable`) | the multi-field `.vdb` round-trip: `nAdd(Voxels/ScalarField/VectorField, name)`, `voxGet`/`oGetScalarField`/`oGetVectorField` by index or name, `nFieldCount`/`strFieldName`/`eFieldType`/`xField`, `SaveToFile`; `bIsPicoGKCompatible`/`fPicoGKVoxelSizeMM` + static `libCreateCompatibleLibraryFor(path)` to open a foreign VDB at its native voxel size |
|  [02]   | `PolySliceStack`    | vector slice stack | the output of `Voxels.oVectorize(fLayerHeight)`: `nCount`/`oSliceAt(n)`/`oBBox`, `AddSlices`, `AddToViewer` — the layered vector-contour program for resin/powder posting |
|  [03]   | `PolySlice`         | one vector layer    | one Z layer of closed `PolyContour`s: `fZPos`, `AddContour`/`Close`/`bIsEmpty`, `SaveToSvgFile(path, bSolid)`, static `oFromSdf(Image, fZPos, vecOffset, fScale)` |
|  [04]   | `PolyContour`       | 2D contour          | a closed 2D loop: `oVertices()`/`vecVertex(n)`/`nCount`, `EWinding` detection (`eDetectWinding`/`DetectWinding`/`eWinding`), `AsSvgPolyline`/`AsSvgPath`, `oBBox` |
|  [05]   | `CliIo`             | `.cli` reader/writer| `WriteSlicesToCliFile(PolySliceStack, path, EFormat, strDate, fUnitsInMM, IProgress)` + `oSlicesFromCliFile(path)` returning a `Result` (slices + bbox + header) — the Common Layer Interface slice-program I/O for SLA/DLP machines |
|  [06]   | `Vdb2Cli`           | VDB->CLI bridge     | static `Convert(strVdbFilePath, fCliLayerHeight, strCliFilePath, strVoxelFieldName, IProgress)` — one-call VDB-field to `.cli` slice program |
|  [07]   | `TgaIo`             | TGA image I/O       | the slice-image raster file format |

[PUBLIC_TYPE_SCOPE]: runtime, geometry primitives, faults
- rail: fabrication

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]      | [CAPABILITY]                                                                |
| :-----: | :-------------------------------- | :----------------- | :------------------------------------------------------------------------- |
|  [01]   | `Library`                         | runtime (`IDisposable`) | the native-core lifecycle bound to ONE `fVoxelSizeMM`: every field/mesh allocation belongs to a `Library`; `Library.GlobalInstance(fVoxelSizeMM, logPath, viewerTitle, env)` sets the ambient instance the parameterless ctors use, `Library.oLibrary()` reads it; exposes the `n*MemUsage()`/`n*Allocated()` native-resource census and `vecVoxelsToMm` coordinate conversion |
|  [02]   | `Config`                          | native binding     | `const string strPicoGKLib = "picogk.26.2"` — the P/Invoke library id |
|  [03]   | `Viewer`                          | OpenGL viewer (`IDisposable`) | the interactive 3D viewer (camera/arcball, key/mouse callbacks, sidebar, animation) — for the sidecar visual debug surface, NOT a headless-posting dependency |
|  [04]   | `BBox3` / `BBox2`                 | bounds struct      | `BBox3`: `vecMin`/`vecMax`, `vecSize`/`vecCenter`, `Include`/`Grow`/`bContains`/`bIsEmpty`, `oFitInto(bounds, out fScale, out vecOffset)` — the implicit-rasterization extent and the fit transform |
|  [05]   | `ColorFloat` / `ColorRgba32` / `ColorHSV` / `ColorHLS` / `Image*` | color/image | the slice-raster color and image-buffer family (`ImageGrayScale` is the slice target) |
|  [06]   | `PicoGKAllocException` / `PicoGKLibraryMismatchException` | fault | native allocation failure / native-library version mismatch — the two typed native faults |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: implicit -> voxels rasterization
- rail: fabrication
- note: an unbounded `IImplicit` needs an explicit `BBox3`; an `IBoundedImplicit` carries its own. The voxel resolution is the `Library` `fVoxelSizeMM` — finer voxels = sharper SDF surface at higher native memory.

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :-------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `new Voxels(in IImplicit xImplicit, in BBox3 oBounds)`          | SDF raster     | rasterize an unbounded implicit (gyroid/TPMS/cellular) within a box          |
|  [02]   | `new Voxels(in IBoundedImplicit xImplicit)`                     | SDF raster     | rasterize a self-bounding implicit                                           |
|  [03]   | `Voxels.RenderImplicit(in IImplicit, in BBox3)` / `IntersectImplicit(in IImplicit)` | SDF compose | add / intersect an implicit into an existing voxel field (`voxIntersectImplicit` returns a copy) |
|  [04]   | `new Voxels(in Lattice lat)` / `Voxels.RenderLattice(in Lattice)` | lattice raster | rasterize a beam/sphere lattice scaffold                                     |
|  [05]   | `new Voxels(in Mesh msh)` / `Voxels.RenderMesh(in Mesh)`        | mesh raster    | voxelize a triangle mesh into an SDF field                                   |
|  [06]   | `Voxels.voxSphere` / `voxLatticeBeam` / `voxMeshShell(msh, fRadius)` | factory   | static one-call primitives (a `(Library, ...)` overload pair: ambient vs explicit library) |

[ENTRYPOINT_SCOPE]: SDF Boolean and distance morphology — `Voxels`
- rail: fabrication
- note: each operation has a MUTATING form (`BoolAdd`, `Offset`, ...) and a COPY form (`voxBoolAdd`, `voxOffset`, ...) returning a new `Voxels`; operators `+`/`-`/`&` are the copy Boolean. Distances are in MM (converted through the `Library` voxel size).

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :-------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `BoolAdd`/`BoolSubtract`/`BoolIntersect(in Voxels)` (+ `voxBool*` copies, `+`/`-`/`&` operators) | SDF Boolean | the three 3D SDF Boolean ops; `BoolAddAll`/`BoolSubtractAll(IEnumerable<Voxels>)` + static `voxCombine`/`voxCombineAll` for batch union |
|  [02]   | `Offset(float fDistMM)` / `voxOffset`                          | distance offset | uniform SDF dilation/erosion (negative shrinks)                              |
|  [03]   | `DoubleOffset(fDist1MM, fDist2MM)` / `TripleOffset(fDistMM)`    | morphology     | open/close-style two- and three-stage offsets (de-feature / gap-bridge)      |
|  [04]   | `OverOffset(fFirstOffsetMM, fFinalSurfaceDistInMM)`            | morphology     | offset-out-then-back for controlled surface conditioning                     |
|  [05]   | `Shell(fOffset)` / `Shell(fNegMM, fPosMM, fSmoothInnerMM)`     | shelling       | hollow-shell the solid to a wall thickness — the lightweighting primitive    |
|  [06]   | `Fillet(fRoundingMM)` / `Smoothen(fDistMM)`                    | smoothing      | distance-field fillet / mean-curvature smoothing of the surface              |
|  [07]   | `Trim(BBox3)` / `ProjectZSlice(fStartZMM, fEndZMM)`           | crop/project   | crop to a box / project a Z-band down into a 2D-extruded slab                 |

[ENTRYPOINT_SCOPE]: mesh extraction, ray-cast, properties — `Voxels`
- rail: fabrication

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :-------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Voxels.mshAsMesh()` / `new Mesh(in Voxels)`                   | extraction     | dual-contour the SDF to a watertight triangle `Mesh` — the wire back to the kernel `MeshSpace` vocabulary |
|  [02]   | `CalculateProperties(out float fVolumeCubicMM, out BBox3)` / `oCalculateBoundingBox()` | metrics | the solid volume + bbox (AM mass/cost estimate) |
|  [03]   | `bIsInside(Vector3)` / `bRayCastToSurface(vecSearch, vecDir, out vecSurfacePoint)` / `vecRayCastToSurface` | query | point-membership and SDF ray-cast to the surface |
|  [04]   | `bIsEmpty()` / `bIsEqual(in Voxels)` / `nMemUsage()` / `voxDuplicate()` | state    | emptiness, structural equality, native footprint, deep copy                  |

[ENTRYPOINT_SCOPE]: AM layer stack — grayscale slices and vector `.cli` program
- rail: fabrication
- note: TWO output paths from a finished `Voxels` solid: (a) GRAYSCALE slice images for SLA/DLP/MSLA mask exposure (`GetVoxelSlice` -> `ImageGrayScale`); (b) VECTOR contour slices for `.cli`/SVG (`oVectorize` -> `PolySliceStack` -> `CliIo`). This is the resin/powder posting path the planar-only FFF `Section` owner never reaches.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `imgAllocateSlice(out int nSliceCount, ESliceAxis eAxis = Z)` / `nSliceCount()`                    | slice alloc    | allocate the grayscale slice buffer + report the layer count                 |
|  [02]   | `GetVoxelSlice(in int nSlice, ref ImageGrayScale img, ESliceMode = SignedDistance, ESliceAxis = Z)` | grayscale slice | rasterize one layer to a grayscale mask (the SLA/DLP exposure image)         |
|  [03]   | `GetInterpolatedVoxelSlice(in float fZSlice, ref ImageGrayScale img, ESliceMode = SignedDistance)` | grayscale slice | a sub-voxel-Z interpolated layer (arbitrary layer height, not voxel-grid-aligned) |
|  [04]   | `oVectorize(float fLayerHeight = 0f, bool bUseAbsXYOrigin = false, IProgress?)`                    | vectorize      | extract the closed-contour `PolySliceStack` over the whole solid at a layer height |
|  [05]   | `CliIo.WriteSlicesToCliFile(PolySliceStack, path, EFormat, strDate, fUnitsInMM, IProgress?)`        | CLI emit       | write the vector slice stack as a `.cli` program (`EFormat.FirstLayerWithContent`/`UseEmptyFirstLayer`) |
|  [06]   | `Vdb2Cli.Convert(vdbPath, fCliLayerHeight, cliPath, voxelFieldName, IProgress?)`                    | VDB->CLI       | one-call VDB-field-to-`.cli` posting                                         |
|  [07]   | `PolySlice.SaveToSvgFile(path, bSolid)` / `PolyContour.AsSvgPath(out str)`                          | SVG emit       | per-layer SVG export (pen-plot / mask-preview)                               |

[ENTRYPOINT_SCOPE]: VDB field I/O and the runtime library
- rail: fabrication

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :-------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `new Library(float fVoxelSizeMM)` / `Library.GlobalInstance(fVoxelSizeMM, ...)` | runtime init | bind the native core to a voxel resolution; `GlobalInstance` sets the ambient library the parameterless field ctors resolve via `Library.oLibrary()` |
|  [02]   | `new OpenVdbFile(path)` / `nAdd(field, name)` / `voxGet`/`oGetScalarField`/`oGetVectorField` / `SaveToFile(path)` | VDB I/O | multi-field VDB read/write |
|  [03]   | `OpenVdbFile.libCreateCompatibleLibraryFor(vdbPath)` + `bIsPicoGKCompatible`/`fPicoGKVoxelSizeMM` | VDB compat | open a foreign VDB at ITS native voxel size (cross-tool interop) |
|  [04]   | `Voxels.voxFromVdbFile(path)`                                  | VDB load       | the one-call field load from a `.vdb`                                        |
|  [05]   | `FieldMetadata.SetValue`/`bGetValueAt` (`string`/`float`/`Vector3`) | metadata  | per-field provenance/parameter key-values round-tripped through VDB          |

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
