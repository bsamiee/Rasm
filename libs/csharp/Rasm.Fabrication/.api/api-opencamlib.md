# [RASM_FABRICATION_API_OPENCAMLIB]

`OpenCAMLib` (`ocl`) is the analytic 3-axis toolpath-geometry engine driving the `Toolpath/surface` 3D-finishing family: exact cutter-location computation for arbitrary `MillingCutter` forms against a triangle-mesh surface, spanning drop-cutter Z-sampling (`BatchDropCutter`/`PathDropCutter`/`AdaptivePathDropCutter`), push-cutter fiber intersection (`BatchPushCutter`), and Z-level waterline extraction (`Waterline`/`AdaptiveWaterline`). Every operation shares one lifecycle contract — `setSTL(STLSurf)` · `setCutter(MillingCutter)` · `setSampling`/`setThreads` · `run` · `getCLPoints`/`getLoops` — so a single `ocl::Operation`-shaped wrapper unifies the whole surface. The exported ABI is C++-mangled with by-reference `STLSurf&`/`CLPoint&`/`std::vector<CLPoint>` arguments, so a direct managed P/Invoke is impossible: consumption is a thin `extern "C"` C-shim (opaque `void*` handles + flat `double[]` triangle/point buffers) over the shipped SHARED `libocl`, source-generated `[LibraryImport]` on the managed side. The Boost.Python binding (`src/pythonlib/ocl_*.cpp`, release `2023.01.11`) is the member-by-member shim blueprint; no C ABI, no NuGet, and no C# binding exists (feed-verified).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenCAMLib`
- package: `OpenCAMLib` (vendored — no NuGet artifact; the `extern "C"` shim + `[LibraryImport]` bindings compile into `Rasm.Fabrication`, the RID-keyed SHARED `libocl` rides `vendor/runtimes` through the folder `.csproj`'s `Exists`-gated `Content` group, LFS-carried, outside NuGet restore)
- license: `LGPL-2.1` — dynamic-link P/Invoke through the separate replaceable `libocl.dylib`/`.so`/`.dll` satisfies §6; `libocl` stays dynamically linked, never statically folded into the shim
- assembly: managed `Rasm.Fabrication` P/Invoke shim (source-generated `[LibraryImport]`); no managed assembly ships upstream
- namespace: `ocl` (C++ engine); the C-shim exports flat `extern "C"` functions, the managed side owns the `OpenCamLib`-shaped local surface
- asset: RID-keyed SHARED native — `macos-cxx-arm64`/`windows-cxx-x64`/`linux-cxx-x86_64` `libocl` (all three present in `2023.01.11`; `cxxlib.cmake` `add_library(ocl SHARED …)`); per-RID OpenMP carriage (osx-arm64 bundles rpath'd `libomp.dylib`, linux resolves system `libgomp`, win the MSVC `vcomp` redist; `USE_OPENMP=OFF` the build-policy fallback); dependency closure is header-only Boost (`Boost::boost`) + OpenMP, zero submodules, no CGAL/Eigen/TBB
- rail: fabrication — the ALC-firebreak/sidecar surface engine, content-keyed at the wire, golden-fixture gated

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: milling-cutter hierarchy — the `CutterForm` axis map
- rail: fabrication

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]  | [CAPABILITY]                                                        |
| :-----: | :--------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `MillingCutter`  | abstract base  | shared drop/push/offset contract; maps to the `CutterForm` axis     |
|  [02]   | `CylCutter`      | flat endmill   | `CylCutter(diameter, length)` → `CutterForm.Flat`                    |
|  [03]   | `BallCutter`     | ball nose      | `BallCutter(diameter, length)` → `CutterForm.Ball`                   |
|  [04]   | `BullCutter`     | bull/toroid    | `BullCutter(diameter, cornerRadius, length)` → `CutterForm.Bull`     |
|  [05]   | `ConeCutter`     | taper/vee      | `ConeCutter(diameter, angle, length)` → `CutterForm.Taper`           |
|  [06]   | `CompCylCutter`  | composite flat | `CompCylCutter(diameter, length)` → `CutterForm.Compound`            |
|  [07]   | `CompBallCutter` | composite ball | `CompBallCutter(diameter, length)` → `CutterForm.Compound`           |
|  [08]   | `CylConeCutter`  | composite      | `CylConeCutter(diameter, majorLength, angle)` → `CutterForm.Compound`|
|  [09]   | `BallConeCutter` | composite      | `BallConeCutter(diameter, majorLength, angle)` → `CutterForm.Compound`|
|  [10]   | `BullConeCutter` | composite      | `BullConeCutter(diameter, r, majorLength, angle)` → `CutterForm.Compound`|
|  [11]   | `ConeConeCutter` | composite      | `ConeConeCutter(diameter, angle1, majorLength, angle2)` → `CutterForm.Compound`|

[PUBLIC_TYPE_SCOPE]: geometry primitives — flat-buffer marshalling
- rail: fabrication

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [CAPABILITY]                                                       |
| :-----: | :----------------- | :-------------- | :---------------------------------------------------------------- |
|  [01]   | `Point`            | 3D point        | `Point(x, y, z)`; `x`/`y`/`z` readwrite; norm/dot/cross/rotate ops |
|  [02]   | `CLPoint`          | cutter location | drop-cutter Z result; `getCC()` the contact point                 |
|  [03]   | `CCPoint`          | contact point   | surface-contact locus; `type` (`CCType`)                          |
|  [04]   | `CCType`           | contact enum    | `VERTEX`/`EDGE`/`FACET`/… contact classification                  |
|  [05]   | `Triangle`         | mesh facet      | `Triangle(p1, p2, p3)`; `getPoints()`; readonly `p`/`n`           |
|  [06]   | `STLSurf`          | triangle mesh   | `addTriangle`/`size`/`rotate`/`getBounds`/`getTriangles`; the surface input |
|  [07]   | `STLReader`        | mesh loader     | STL-file → `STLSurf`                                              |
|  [08]   | `Bbox`             | bounding box    | `isInside`; `maxpt`/`minpt`                                       |
|  [09]   | `Line` / `Arc`     | path spans      | line and arc span primitives                                     |
|  [10]   | `Path`             | drive curve     | `append(Line)`/`append(Arc)`; `getSpans`; the `PathDropCutter` input |
|  [11]   | `SpanType`         | span enum       | `LineSpanType`/`ArcSpanType`                                      |
|  [12]   | `Interval`/`Fiber` | push-cutter     | fiber-intersection interval + fiber (`BatchPushCutter`/`Waterline`) |

[PUBLIC_TYPE_SCOPE]: operations — the shared `ocl::Operation` lifecycle
- rail: fabrication

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]     | [CAPABILITY]                                                    |
| :-----: | :------------------------ | :---------------- | :------------------------------------------------------------- |
|  [01]   | `BatchDropCutter`         | drop-cutter batch | Z-sample an unordered `CLPoint` cloud against the surface       |
|  [02]   | `PathDropCutter`          | drop-cutter path  | Z-project a `Path` drive curve to cutter-location points        |
|  [03]   | `AdaptivePathDropCutter`  | adaptive path     | error-bounded adaptive-sampled path drop (`setCosLimit`)        |
|  [04]   | `BatchPushCutter`         | push-cutter batch | fiber-interval intersection for waterline/slice fibers          |
|  [05]   | `Waterline`               | Z-level extract   | Z-level closed-loop extraction (`getLoops`)                     |
|  [06]   | `AdaptiveWaterline`       | adaptive Z-level  | error-bounded adaptive waterline (`setMinSampling`)             |
|  [07]   | `CutterLocationSurface`   | CL surface        | dense cutter-location surface (`getVertices`/`getEdges`)        |
|  [08]   | `ZigZag`                  | direction fill    | direction/stepover raster fill of a region                     |
|  [09]   | `LineCLFilter`            | CL simplify       | tolerance-based CL-point line simplification                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: shared operation lifecycle — every `Operation` subclass
- rail: fabrication

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------------------ | :------------- | :---------------------------------------------------- |
|  [01]   | `setSTL(STLSurf& surf)`         | input          | bind the triangle-mesh surface                        |
|  [02]   | `setCutter(MillingCutter& c)`   | input          | bind the cutter form (drives `CutterForm` selection)  |
|  [03]   | `setSampling(double s)`         | policy         | sampling step for path/waterline operations           |
|  [04]   | `setThreads(int n)` / `getThreads()` | policy    | OpenMP thread count for batch operations              |
|  [05]   | `run()`                         | operation      | execute the geometry computation                      |
|  [06]   | `getCLPoints()`                 | output         | cutter-location result cloud (`std::vector<CLPoint>`) |

[ENTRYPOINT_SCOPE]: drop-cutter Z-sampling — `BatchDropCutter` / `PathDropCutter` / `AdaptivePathDropCutter`
- rail: fabrication

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :--------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `appendPoint(CLPoint& p)`          | input          | seed one CL-point to Z-sample (`BatchDropCutter`)      |
|  [02]   | `setPath(Path& p)`                 | input          | drive curve for path drop (`PathDropCutter`)          |
|  [03]   | `setZ(double z)` / `getZ()`        | policy         | minimum-Z clearance floor                             |
|  [04]   | `setMinSampling(double s)`         | policy         | adaptive lower sampling bound (`Adaptive…`)           |
|  [05]   | `setCosLimit(double c)`            | policy         | adaptive angular error threshold (`Adaptive…`)        |
|  [06]   | `getTrianglesUnderCutter(CLPoint&, MillingCutter&)` | diagnostic | triangles overlapped at a location (visualization)    |
|  [07]   | `getCalls()` / `setBucketSize`/`getBucketSize` | tuning | KD-tree bucket + call-count instrumentation           |

[ENTRYPOINT_SCOPE]: waterline extraction — `Waterline` / `AdaptiveWaterline`
- rail: fabrication

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY] | [CAPABILITY]                                       |
| :-----: | :---------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `setZ(double z)`              | input          | the Z-level to extract at                          |
|  [02]   | `run()` / `run2()`            | operation      | extract loops (`run2` the weave-graph variant)     |
|  [03]   | `getLoops()`                  | output         | closed Z-level loops (`std::vector<std::vector<Point>>`) |
|  [04]   | `setMinSampling(double s)`    | policy         | adaptive fiber refinement bound (`Adaptive…`)      |
|  [05]   | `getXFibers()`/`getYFibers()` | diagnostic     | the X/Y push-cutter fiber sets                     |
|  [06]   | `reset()`                     | reset          | clear fibers/loops for re-run                      |

[ENTRYPOINT_SCOPE]: push-cutter fibers — `BatchPushCutter`
- rail: fabrication

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------------------------ | :------------- | :----------------------------------------------- |
|  [01]   | `appendFiber(Fiber& f)`         | input          | seed one fiber to intersect                      |
|  [02]   | `setXDirection`/`setYDirection` | policy         | fiber orientation for the push pass              |
|  [03]   | `getFibers()`                   | output         | the intersected fibers with their `Interval`s    |
|  [04]   | `getOverlapTriangles(Fiber&)`   | diagnostic     | triangles overlapped by a fiber                  |

## [04]-[IMPLEMENTATION_LAW]

[SHIM_ABI]:
- The exported C++ ABI is mangled and by-reference (`STLSurf&`/`CLPoint&`/`std::vector<CLPoint>`); a managed `[LibraryImport]` binds ONLY through the `extern "C"` C-shim — ~18-24 flat functions, all blittable: opaque `void*` handles for `Operation`/`MillingCutter`/`STLSurf`, and flat `double[]` buffers for triangle vertices and CL-points
- The shim mirrors the `ocl::Operation` lifecycle: `ocl_op_create(kind)` → `ocl_op_set_stl(op, double* tris, int nTris)` → `ocl_op_set_cutter(op, cutterHandle)` → `ocl_op_set_sampling`/`ocl_op_set_threads` → `ocl_op_run(op)` → `ocl_op_get_clpoints(op, double* out, int* n)` → `ocl_op_destroy(op)`; the `MillingCutter` ctor map is `ocl_cutter_cyl(d, l)`/`ocl_cutter_ball(d, l)`/`ocl_cutter_bull(d, r, l)`/`ocl_cutter_cone(d, angle, l)`/composite variants — one shim ctor per `CutterForm` row
- `libocl` stays a separate dynamically-linked SHARED archive (LGPL-2.1 §6); NEVER static-fold it into the shim; two admission routes doubly cover the RID matrix — consume the shipped SHARED archives OR build `libocl`+shim from source per RID (`BUILD_CXX_LIB=ON`), Forge-provisioned like the kernel Z3 precedent
- The Boost.Python binding files (`ocl_cutters.cpp`/`ocl_dropcutter.cpp`/`ocl_algo.cpp`/`ocl_geometry.cpp`) are the exact member-by-member shim blueprint — Boost.Python, NOT pybind11/emscripten; the shim re-exports exactly the members those files expose

[LOCAL_ADMISSION]:
- `Toolpath/surface#SurfacePath.Sample` owns cutter POSITIONING through this engine: the `CutterForm` axis picks the `MillingCutter` ctor at the P/Invoke edge (`CylCutter`/`BallCutter`/`BullCutter`/`ConeCutter`/`Comp…`), and the `MeshSpace` triangle buffer marshals to `setSTL`
- The strategy row picks the operation: waterline roughing → `Waterline`/`AdaptiveWaterline` `getLoops`, drop-cutter finishing → `BatchDropCutter`/`PathDropCutter` `getCLPoints`, adaptive-error → `AdaptivePathDropCutter` `setCosLimit`
- Path LAYOUT is NEVER this engine's: geodesic-parallel/constant-stepover isolines, flowline/morph streamlines, and flank/swarf cross-field orientation compose the kernel on-mesh machinery (`geodesics.md`/`extract.md`/`flow.md`/`segment.md`); OCL owns only the Z-height/contact sampling AT the laid-out sample points — a Fabrication-side on-mesh re-implementation is the forbidden `[V2]` defect
- Drop-cutter non-convergence and empty CL-clouds raise `Toolpath` `SampleStalled` 2713 `(SurfaceStrategy strategy, int iteration)` at the shim boundary, distinct from the `Toolpath/partition` Voronoi `PartitionDegenerate` 2723
- The author-kernel SDF drop-cutter over the kernel `[V8]` distance-field lane (`Spatial/fields.md#SignedDistanceFromMesh`, K8) is the admission-abandonment FALLBACK: it re-derives per-cutter-form contact (flat/bull/cone are the cutter-solid lower envelope, NOT a mesh-SDF iso-offset), is resolution-bound where OCL is analytically exact, degrades waterline to marching-squares, and is calendar-hostage to the unlanded kernel SDF lane — the shim is the ruled path
- Golden-fixture gated: the shim + native asset admit only against a committed cutter-location golden fixture per RID (the LGPL native-engine admission gate)

[RAIL_LAW]:
- Package: `OpenCAMLib` (vendored SHARED native, LGPL-2.1 dynamic-link)
- Owns: exact 3-axis cutter-location geometry — drop-cutter Z-sampling, push-cutter fibers, waterline Z-level loops — for arbitrary `MillingCutter` forms against a triangle mesh
- Accept: a `MeshSpace` triangle buffer + a `CutterForm` row + a `SurfaceStrategy` from `Toolpath/surface`, marshalled through the `extern "C"` shim
- Reject: path layout (kernel on-mesh owned), any 2D polygon concern (`Geometry2D`), a second SDF owner (PicoGK stays Fabrication's VOXEL lane, the kernel `[V8]` lane the sole distance field), and static-linking `libocl` into the shim
