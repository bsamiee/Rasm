# [RASM_FABRICATION_API_OPENCAMLIB]

`OpenCAMLib` (`ocl`) owns analytic 3-axis cutter-location geometry: exact drop-cutter Z-sampling, push-cutter fiber intersection, and waterline Z-level loop extraction for arbitrary `MillingCutter` forms against a triangle mesh, every operation folding through one `ocl::Operation` lifecycle. Path layout stays the kernel on-mesh machinery; `ocl` owns only the Z-height and contact sampling at the laid-out points. Its C++-mangled by-reference ABI binds through a source-generated `[LibraryImport]` over an `extern "C"` shim against the vendored SHARED `libocl`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenCAMLib`
- package: `OpenCAMLib` (LGPL-2.1)
- assembly: managed `Rasm.Fabrication` P/Invoke shim (source-generated `[LibraryImport]`); no managed assembly ships upstream
- namespace: `ocl` (C++ engine); the C-shim exports flat `extern "C"` functions, the managed side owns the `OpenCamLib`-shaped local surface
- asset: RID-keyed SHARED native `libocl` (`macos-cxx-arm64`/`windows-cxx-x64`/`linux-cxx-x86_64`, all three present), riding `vendor/runtimes` through the folder `.csproj` `Exists`-gated `Content` group, LFS-carried, outside NuGet restore; per-RID OpenMP carriage (osx-arm64 rpath'd `libomp.dylib`, linux system `libgomp`, win MSVC `vcomp`); dependency closure is header-only Boost + OpenMP
- rail: fabrication — the ALC-firebreak/sidecar surface engine, content-keyed at the wire, golden-fixture gated

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: milling-cutter hierarchy — the `CutterForm` axis map

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]  | [CAPABILITY]                                                                    |
| :-----: | :--------------- | :------------- | :------------------------------------------------------------------------------ |
|  [01]   | `MillingCutter`  | abstract base  | shared drop/push/offset contract; maps to the `CutterForm` axis                 |
|  [02]   | `CylCutter`      | flat endmill   | `CylCutter(diameter, length)` → `CutterForm.Flat`                               |
|  [03]   | `BallCutter`     | ball nose      | `BallCutter(diameter, length)` → `CutterForm.Ball`                              |
|  [04]   | `BullCutter`     | bull/toroid    | `BullCutter(diameter, cornerRadius, length)` → `CutterForm.Bull`                |
|  [05]   | `ConeCutter`     | taper/vee      | `ConeCutter(diameter, angle, length)` → `CutterForm.Taper`                      |
|  [06]   | `CompCylCutter`  | composite flat | `CompCylCutter(diameter, length)` → `CutterForm.Compound`                       |
|  [07]   | `CompBallCutter` | composite ball | `CompBallCutter(diameter, length)` → `CutterForm.Compound`                      |
|  [08]   | `CylConeCutter`  | composite      | `CylConeCutter(diameter, majorLength, angle)` → `CutterForm.Compound`           |
|  [09]   | `BallConeCutter` | composite      | `BallConeCutter(diameter, majorLength, angle)` → `CutterForm.Compound`          |
|  [10]   | `BullConeCutter` | composite      | `BullConeCutter(diameter, r, majorLength, angle)` → `CutterForm.Compound`       |
|  [11]   | `ConeConeCutter` | composite      | `ConeConeCutter(diameter, angle1, majorLength, angle2)` → `CutterForm.Compound` |

[PUBLIC_TYPE_SCOPE]: geometry primitives — flat-buffer marshalling

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [CAPABILITY]                                                                |
| :-----: | :----------------- | :-------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Point`            | 3D point        | `Point(x, y, z)`; `x`/`y`/`z` readwrite; norm/dot/cross/rotate ops          |
|  [02]   | `CLPoint`          | cutter location | drop-cutter Z result; `getCC()` the contact point                           |
|  [03]   | `CCPoint`          | contact point   | surface-contact locus; `type` (`CCType`)                                    |
|  [04]   | `CCType`           | contact enum    | `VERTEX`/`EDGE`/`FACET`/… contact classification                            |
|  [05]   | `Triangle`         | mesh facet      | `Triangle(p1, p2, p3)`; `getPoints()`; readonly `p`/`n`                     |
|  [06]   | `STLSurf`          | triangle mesh   | `addTriangle`/`size`/`rotate`/`getBounds`/`getTriangles`; the surface input |
|  [07]   | `STLReader`        | mesh loader     | STL-file → `STLSurf`                                                        |
|  [08]   | `Bbox`             | bounding box    | `isInside`; `maxpt`/`minpt`                                                 |
|  [09]   | `Line` / `Arc`     | path spans      | line and arc span primitives                                                |
|  [10]   | `Path`             | drive curve     | `append(Line)`/`append(Arc)`; `getSpans`; the `PathDropCutter` input        |
|  [11]   | `SpanType`         | span enum       | `LineSpanType`/`ArcSpanType`                                                |
|  [12]   | `Interval`/`Fiber` | push-cutter     | fiber-intersection interval + fiber (`BatchPushCutter`/`Waterline`)         |

[PUBLIC_TYPE_SCOPE]: operations — the shared `ocl::Operation` lifecycle

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [CAPABILITY]                                              |
| :-----: | :----------------------- | :---------------- | :-------------------------------------------------------- |
|  [01]   | `BatchDropCutter`        | drop-cutter batch | Z-sample an unordered `CLPoint` cloud against the surface |
|  [02]   | `PathDropCutter`         | drop-cutter path  | Z-project a `Path` drive curve to cutter-location points  |
|  [03]   | `AdaptivePathDropCutter` | adaptive path     | error-bounded adaptive-sampled path drop (`setCosLimit`)  |
|  [04]   | `BatchPushCutter`        | push-cutter batch | fiber-interval intersection for waterline/slice fibers    |
|  [05]   | `Waterline`              | Z-level extract   | Z-level closed-loop extraction (`getLoops`)               |
|  [06]   | `AdaptiveWaterline`      | adaptive Z-level  | error-bounded adaptive waterline (`setMinSampling`)       |
|  [07]   | `CutterLocationSurface`  | CL surface        | dense cutter-location surface (`getVertices`/`getEdges`)  |
|  [08]   | `ZigZag`                 | direction fill    | direction/stepover raster fill of a region                |
|  [09]   | `LineCLFilter`           | CL simplify       | tolerance-based CL-point line simplification              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: shared operation lifecycle — every `Operation` subclass

| [INDEX] | [SURFACE]                            | [SHAPE]   | [CAPABILITY]                                          |
| :-----: | :----------------------------------- | :-------- | :---------------------------------------------------- |
|  [01]   | `setSTL(STLSurf& surf)`              | input     | bind the triangle-mesh surface                        |
|  [02]   | `setCutter(MillingCutter& c)`        | input     | bind the cutter form (drives `CutterForm` selection)  |
|  [03]   | `setSampling(double s)`              | policy    | sampling step for path/waterline operations           |
|  [04]   | `setThreads(int n)` / `getThreads()` | policy    | OpenMP thread count for batch operations              |
|  [05]   | `run()`                              | operation | execute the geometry computation                      |
|  [06]   | `getCLPoints()`                      | output    | cutter-location result cloud (`std::vector<CLPoint>`) |

[ENTRYPOINT_SCOPE]: drop-cutter Z-sampling — `BatchDropCutter` / `PathDropCutter` / `AdaptivePathDropCutter`

| [INDEX] | [SURFACE]                                           | [SHAPE]    | [CAPABILITY]                                       |
| :-----: | :-------------------------------------------------- | :--------- | :------------------------------------------------- |
|  [01]   | `appendPoint(CLPoint& p)`                           | input      | seed one CL-point to Z-sample (`BatchDropCutter`)  |
|  [02]   | `setPath(Path& p)`                                  | input      | drive curve for path drop (`PathDropCutter`)       |
|  [03]   | `setZ(double z)` / `getZ()`                         | policy     | minimum-Z clearance floor                          |
|  [04]   | `setMinSampling(double s)`                          | policy     | adaptive lower sampling bound (`Adaptive…`)        |
|  [05]   | `setCosLimit(double c)`                             | policy     | adaptive angular error threshold (`Adaptive…`)     |
|  [06]   | `getTrianglesUnderCutter(CLPoint&, MillingCutter&)` | diagnostic | triangles overlapped at a location (visualization) |
|  [07]   | `getCalls()` / `setBucketSize`/`getBucketSize`      | tuning     | KD-tree bucket + call-count instrumentation        |

[ENTRYPOINT_SCOPE]: waterline extraction — `Waterline` / `AdaptiveWaterline`

| [INDEX] | [SURFACE]                     | [SHAPE]    | [CAPABILITY]                                             |
| :-----: | :---------------------------- | :--------- | :------------------------------------------------------- |
|  [01]   | `setZ(double z)`              | input      | the Z-level to extract at                                |
|  [02]   | `run()` / `run2()`            | operation  | extract loops (`run2` the weave-graph variant)           |
|  [03]   | `getLoops()`                  | output     | closed Z-level loops (`std::vector<std::vector<Point>>`) |
|  [04]   | `setMinSampling(double s)`    | policy     | adaptive fiber refinement bound (`Adaptive…`)            |
|  [05]   | `getXFibers()`/`getYFibers()` | diagnostic | the X/Y push-cutter fiber sets                           |
|  [06]   | `reset()`                     | reset      | clear fibers/loops for re-run                            |

[ENTRYPOINT_SCOPE]: push-cutter fibers — `BatchPushCutter`

| [INDEX] | [SURFACE]                       | [SHAPE]    | [CAPABILITY]                                  |
| :-----: | :------------------------------ | :--------- | :-------------------------------------------- |
|  [01]   | `appendFiber(Fiber& f)`         | input      | seed one fiber to intersect                   |
|  [02]   | `setXDirection`/`setYDirection` | policy     | fiber orientation for the push pass           |
|  [03]   | `getFibers()`                   | output     | the intersected fibers with their `Interval`s |
|  [04]   | `getOverlapTriangles(Fiber&)`   | diagnostic | triangles overlapped by a fiber               |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every operation folds through one `ocl::Operation` lifecycle — `setSTL(STLSurf&)` · `setCutter(MillingCutter&)` · `setSampling`/`setThreads` · `run()` · `getCLPoints()`/`getLoops()` — so one `Operation`-shaped wrapper unifies the whole surface.
- A managed `[LibraryImport]` binds only through the `extern "C"` shim, never the mangled by-reference C++ ABI (`STLSurf&`/`CLPoint&`/`std::vector<CLPoint>`) directly; every shim function is blittable — opaque `void*` handles for `Operation`/`MillingCutter`/`STLSurf`, flat `double[]` buffers for triangle vertices and CL-points.
- Shim functions mirror the lifecycle: `ocl_op_create(kind)` → `ocl_op_set_surface`/`ocl_op_set_cutter` → `ocl_op_set_sampling`/`ocl_op_set_threads` → `ocl_op_run` → `ocl_op_get_clpoints(op, double* out, int* n)` → `ocl_op_destroy`, with one ctor per `CutterForm` row (`ocl_cutter_cyl`/`ocl_cutter_ball`/`ocl_cutter_bull`/`ocl_cutter_cone` and composites).
- `STLSurf` is its own handle family (`ocl_stl_create(tris, n)`/`ocl_stl_destroy`), not operation-owned state: `setSTL` binds by reference, so one triangle upload serves every capsule a run creates.
- `Waterline.reset()` clears fibers and loops in place, so one waterline capsule sweeps a whole Z-level set through `setZ`/`run`/`getLoops`/`reset`.
- `ocl_cutters.cpp`/`ocl_dropcutter.cpp`/`ocl_algo.cpp`/`ocl_geometry.cpp` are the Boost.Python member-by-member shim blueprint; the shim re-exports exactly the members they expose.

[STACKING]:
- `Toolpath/surface` kernel (within-lib): `SurfacePath.Sample` marshals the `MeshSpace` triangle buffer to `setSTL`, picks the `MillingCutter` ctor from the `CutterForm` axis and the operation from the `SurfaceStrategy` row — waterline roughing → `Waterline`/`AdaptiveWaterline` `getLoops`, drop-cutter finishing → `BatchDropCutter`/`PathDropCutter` `getCLPoints`, adaptive-error → `AdaptivePathDropCutter` `setCosLimit` — threads `setSampling`/`setThreads`, and decodes the CL cloud or Z-level loops at the managed boundary; `LineCLFilter` (`setTolerance`/`addCLPoint`/`run`/`getCLPoints`) simplifies the drop-cutter output before the decode.
- `api-picogk`(`.api/api-picogk.md`): the voxel lane's `Voxels.mshAsMesh()` dual-contour mesh crosses the kernel `MeshSpace` vocabulary and Z-samples through `setSTL`; `ocl` owns the analytic mesh-CL lane and never the `IImplicit`/`Voxels` SDF concern, and the kernel distance-field lane is the sole SDF owner.
- `api-cavaliercontours`(`.api/api-cavaliercontours.md`): waterline `getLoops()` Z-level closed loops hand off to the arc-native 2D `Polyline<double>` offset for stepover and clearing; `ocl` emits the loops and owns no 2D polygon offset.

[LOCAL_ADMISSION]:
- Path layout is the kernel's — geodesic-parallel/constant-stepover isolines, flowline/morph streamlines, and flank/swarf cross-field orientation compose the on-mesh machinery (`geodesics.md`/`extract.md`/`flow.md`/`segment.md`); `ocl` Z-samples at the laid-out points, and the kernel owns every on-mesh re-layout.
- `libocl` stays a separate dynamically-linked SHARED archive (LGPL-2.1 §6), never static-folded into the shim. Two admission routes cover the RID matrix — consume the shipped SHARED archives, or build `libocl` and the shim from source per RID (`BUILD_CXX_LIB=ON`), Forge-provisioned like the kernel Z3 precedent.
- Shim and native asset admit only against a committed per-RID cutter-location golden fixture, the LGPL native-engine admission gate.
- Drop-cutter non-convergence and an empty CL-cloud raise `Toolpath` `SampleStalled` 2713 `(SurfaceStrategy strategy, int iteration)` at the shim boundary, distinct from the `Toolpath/partition` Voronoi `PartitionDegenerate` 2723.

[RAIL_LAW]:
- Package: `OpenCAMLib` (LGPL-2.1)
- Owns: exact analytic 3-axis cutter-location geometry — drop-cutter Z-sampling, push-cutter fibers, waterline Z-level loops — for arbitrary `MillingCutter` forms against a triangle mesh
- Accept: a `MeshSpace` triangle buffer, a `CutterForm` row, and a `SurfaceStrategy` from `Toolpath/surface`, marshalled through the `extern "C"` shim
- Reject: path layout (kernel on-mesh owned); any 2D polygon concern (`Geometry2D`, `api-cavaliercontours`); a second SDF owner (the kernel distance-field lane is sole, PicoGK owns the voxel lane); static-linking `libocl` into the shim
