# [PROBE]-[OPENCAMLIB_BINDING_HINGE]

VERDICT: **FEASIBLE — admit OpenCAMLib as the `[V5]` drop-cutter/push-cutter/waterline sampling engine via a thin in-house C-shim + P/Invoke, exactly as ruled.** The probe found NO missing RID, NO missing entry point, NO license conflict, and a strictly-heavier fallback. Every residual is the already-priced Manifold-class native-asset burden the brief admits, each with a ruled close. The author-kernel fallback stands only if admission is abandoned, which the evidence does not warrant.

Primary source throughout: `github.com/aewallin/opencamlib`, master HEAD `95b036fe` (2025-02-12). Every claim carries two corroborating anchors.

## [01]-[RELEASE_ARCHIVES_AND_SYMBOLS]

The brief's RID claim holds on disk. Latest release `2023.01.11` ships five CXX library archives as first-class release assets (`get_latest_release`; README.rst: "The C++ library is called `libocl` and is hosted on our Github Releases page"):

| Archive | Size | Downloads | Brief RID |
|---|---|---|---|
| `macos-cxx-arm64.zip` | 1.72 MB | 63 | osx-arm64 PRESENT |
| `windows-cxx-x64.zip` | 226 KB | 894 | win-x64 PRESENT |
| `linux-cxx-x86_64.zip` | 2.07 MB | 395 | linux-x64 PRESENT |
| `macos-cxx-x86_64.zip` | 1.72 MB | 59 | (osx-x64 bonus) |
| `windows-cxx-ia32.zip` | 217 KB | 115 | (win-x86 bonus) |

All three brief-named RIDs ship; osx-arm64 and linux-aarch64 are both built (`pyproject.toml [tool.cibuildwheel]` archs `arm64`/`aarch64`; cd.yml CXX matrix builds win{ia32,x64}, macos{x86_64,arm64}, linux{x86_64,aarch64}).

SHARED, not static (`src/cxxlib/cxxlib.cmake:1` `add_library(ocl SHARED ...)`). Artifact per platform, from the `install()` rules (`cxxlib.cmake` + `GNUInstallDirs`):
- macOS: `libocl.dylib` (LTO, `strip -Sl`, `.dSYM` sidecar) **+ bundled `libomp.dylib`** (copied into `lib/opencamlib`, `INSTALL_RPATH "/opt/homebrew/opt/libomp/lib;/usr/local/opt/libomp/lib;@loader_path"`, `install_name_tool -change ... @rpath/libomp.dylib`).
- Windows: `libocl.dll` + `libocl.lib` import lib (`PREFIX "lib"`, `WINDOWS_EXPORT_ALL_SYMBOLS ON`); OpenMP via MSVC `vcomp140.dll` redist, not bundled.
- Linux: `libocl.so`; OpenMP via system `libgomp`.

Export model: full-visibility C++ ABI. Windows exports every symbol (`WINDOWS_EXPORT_ALL_SYMBOLS ON`); macOS/Linux use default Clang/GCC visibility (no `-fvisibility=hidden` in `src/CMakeLists.txt` — only `-Wall -Wno-deprecated -pedantic-errors`). The exported names are C++-MANGLED and the methods take C++ objects by reference/value (`STLSurf&`, `CLPoint&`, `std::vector<CLPoint>`). This is the load-bearing fact: **a direct P/Invoke is impossible** — mangling is toolchain-specific and the arguments are not blittable. A C-shim exporting `extern "C"` is mandatory, precisely as the brief ruled. Not a defect; the ruled path.

## [02]-[THE_CPP_SURFACE_THE_SHIM_WRAPS]

The wrap target is small and uniform because a single base class unifies the engines.

`ocl::Operation` (`src/algo/operation.hpp`) — the base every sampling engine derives from: `setSTL(const STLSurf&)`, `setCutter(const MillingCutter*)`, `setSampling(double)`, `setThreads(unsigned)`, `setBucketSize(unsigned)` (KD-tree), `run()`, `getCLPoints() -> vector<CLPoint>`, plus push-cutter carriers (`appendFiber(Fiber&)`, `getFibers()`, `setXDirection`/`setYDirection`). The shim wraps ONE lifecycle across all four engines, not four bespoke surfaces.

The four ruled engines, all `Operation` subclasses:
- `BatchDropCutter` (`src/dropcutter/batchdropcutter.hpp`) — OpenMP-parallel, KD-tree-accelerated point drop; the production drop-cutter.
- `PathDropCutter` (`src/dropcutter/pathdropcutter.hpp`) — drop along a sampled path; `PointDropCutter`/`AdaptivePathDropCutter` siblings.
- `Waterline` (`src/algo/waterline.hpp`) — constant-z contour via push-cutter fibers + weave/half-edge linking.
- `AdaptiveWaterline` (`src/algo/adaptivewaterline.hpp`) — adaptively-sampled waterline.

The cutter hierarchy (`src/cutters/`) maps 1:1 onto the `[V5]` `CutterForm` axis. `MillingCutter` base (`millingcutter.hpp`: `getDiameter/getRadius/getLength`, `offsetCutter(d)`, the template-method `dropCutter`/`pushCutter`) with concrete forms:

| OCL cutter | Ctor shape | `[V5]` CutterForm row |
|---|---|---|
| `CylCutter` | (diameter, length) | flat |
| `BallCutter` | (diameter, length) | ball |
| `BullCutter` | (diameter, corner_radius, length) | bull |
| `ConeCutter` | (diameter, angle, length) | taper |
| `CompositeCutter` | composed radial segments | compound |

`offsetCutter(d)` (returns a larger cutter) is the cutter-radius-compensation primitive the `guard` swept-disc consumes. STL surface loading: `STLSurf` (`src/geo/stlsurf.hpp`) via `addTriangle(Triangle)`, or `STLReader` (`src/geo/stlreader.hpp`) from file. Output atoms: `CLPoint` (cutter-location, x/y/z) carrying a `CCPoint` (cutter-contact) — flat doubles at the wire.

Thread model: OpenMP. `ocl::max_threads()` returns `omp_get_max_threads()` under `_OPENMP`, else 1 (`src/ocl.cpp`); every `Operation` carries `setThreads(n)` fanned to sub-operations. `USE_OPENMP` defaults ON (`src/CMakeLists.txt`), so the shipped drop-cutter is parallel. `USE_OPENMP=OFF` is a valid single-threaded build that erases the libomp dependency at a throughput cost — a DECISION build-policy row, not a blocker.

## [03]-[SHIM_SURFACE_MAP]

A single `extern "C"` translation unit (`opencamlib_c.cpp`) exporting opaque `void*` handles and flat `double*` buffers. The Boost.Python binding (`src/pythonlib/ocl_cutters.cpp`, `ocl_dropcutter.cpp`, `ocl_algo.cpp`, `ocl_geometry.cpp`) is the exact member-by-member blueprint — the shim mirrors its split minus Python, plus flat-array marshalling. Estimated ~18-24 functions:

- Surface: `ocl_stlsurf_new()`, `ocl_stlsurf_add_triangle(h, double[9])`, `ocl_stlsurf_free(h)`.
- Cutters: `ocl_cutter_new_cyl(d,len)`, `_ball(d,len)`, `_bull(d,r,len)`, `_cone(d,angle,len)`, `_offset(h,d)`, `_free(h)`.
- Engines (one quintet per engine, uniform via `Operation`): `ocl_bdc_new()`, `ocl_bdc_set_stl(h,surf)`, `ocl_bdc_set_cutter(h,cut)`, `ocl_bdc_set_sampling(h,s)`, `ocl_bdc_set_threads(h,n)`, `ocl_bdc_append_point(h,x,y)`, `ocl_bdc_run(h)`, `ocl_bdc_get_count(h)`, `ocl_bdc_get_points(h,double* out)`, `ocl_bdc_free(h)`; the same shape re-instanced for `pdc`/`waterline`/`awl` (waterline adds a `set_z(h,z)` and returns loops).
- Diagnostics: `ocl_max_threads()`, `ocl_version()`.

Marshalling is blittable throughout: inputs are `double[9]` triangles and `(x,y)` seeds; outputs are `double[]` of `(x,y,z,cc)` quadruples sized by a prior `get_count`. No STL container, cutter object, or geometry type crosses the boundary. C# side is `[LibraryImport]` source-generated P/Invoke behind the `Toolpath/surface` owner, under the ALC-firebreak/sidecar posture the brief already mandates for net9-native lanes.

## [04]-[LICENSING_POSTURE]

LGPL-2.1, confirmed by four independent primary sources:
1. `COPYING` header verbatim: "GNU LESSER GENERAL PUBLIC LICENSE / Version 2.1, February 1999".
2. Every source header (`ocl.cpp`, `operation.hpp`, `millingcutter.hpp`): "GNU Lesser General Public License ... either version 2.1 of the License, or (at your option) any later version".
3. README.rst: "LGPL v2.1" badge + "From August 2018 OpenCAMLib is released under LGPL license".
4. Relicense history in the release tags: `2018.07 GPL` ("last version of opencamlib under GPL license. A change to LGPL will follow") → `2018.08 LGPL` ("The first release under LGPL license"). Same-author `aewallin/openvoronoi` ("C++ with python bindings. Licensed under LGPL2.1") corroborates the house posture.

Dynamic-link P/Invoke posture is clean and already-ratified policy (brief `[ROSTER_RECONCILIATION]`: "LGPL native engines are admissible as dynamic-link P/Invoke assets under the golden-fixture gate the kernel's Manifold admission normalized"). `libocl` ships and is consumed as a separate, user-replaceable `.dylib`/`.so`/`.dll`; the `extern "C"` shim is "a work that uses the Library" that links it dynamically; the C# layer calls the shim's C ABI at a further remove. LGPL-2.1 §6's relink obligation is satisfied trivially because the library remains an independent replaceable shared object. Keep `libocl` dynamically linked (never statically folded into the shim) to avoid the §6 object-file-provisioning obligation — the natural posture regardless. `("or any later version")` also permits treating the asset as LGPL-3 if ever convenient; no need.

## [05]-[BUILD_BURDEN_AND_DEPENDENCY_CLOSURE]

Minimal and modern. `find_package(Boost REQUIRED)` + `target_link_libraries(ocl PUBLIC Boost::boost)` — **HEADER-ONLY Boost** for the core `libocl` (`Boost::boost` is the header target: `boost::graph` for the weave half-edge diagram, `boost::foreach`, `boost::math`). Compiled Boost.Python is required ONLY for `BUILD_PY_LIB`, which the shim path never enables. `OpenMP::OpenMP_CXX` (PRIVATE, when `USE_OPENMP`). CMake 3.15–3.25, C++14 (`CMAKE_CXX_STANDARD 14`). `.gitmodules` is empty — zero submodules; no CGAL, Eigen, TBB, or vendored third-party. That is the entire closure: Boost headers + OpenMP.

Two independent admission routes, so the RID matrix is doubly covered:
- Consume the shipped `2023.01.11` SHARED `libocl` archives (osx-arm64/win-x64/linux-x64) and link the shim against them.
- Build `libocl` + shim together from the pinned source per RID (`BUILD_CXX_LIB=ON`) — trivial given the tiny closure, and immune to any staleness in the 2023 prebuilt binaries. `forge-scientific-env` already owns compiler/`pkg-config`/native-lib/libomp provisioning, so the per-RID build is an existing Forge capability, mirroring the kernel Z3 native-asset precedent.

Maintenance status, stated honestly: the core CAM algorithms are frozen/mature (`cutters`/`dropcutter`/`algo` substantially unchanged since the 2019.07 release). Repository maintenance is low-velocity but alive — master HEAD is 2025-02-12, a Jan–Feb 2025 cluster of build/packaging commits (typo sweep, CMake/Boost-URL refresh, PR #167 "switch build to link with dynamic boost library", Debian path fixes). Last binary release is Jan 2023. "Active" is defensible as *maintained*, not *evolving*; for a sampling kernel we pin and wrap, frozen maturity is an asset, and we control the shim + pin `libocl` regardless.

## [06]-[BINDING_AND_FFI_PRECEDENT]

No C ABI, no C# binding, no NuGet — confirmed. Upstream bindings are all object-oriented language wraps of the same C++ classes: Boost.Python (`src/pythonlib/`, `BOOST_PYTHON_MODULE(ocl)` in `ocl.cpp`, per-class `*_py.hpp` `boost::python::class_<>` exports), a Node.js native module (`src/nodejslib/`), and an Emscripten/WASM binding (`src/emscriptenlib/`). None expose `extern "C"`. `search_code opencamlib DllImport language:C#` → 0 results; the 75-repo `opencamlib` sweep returns only the C++ canonical (520 stars), conda-forge/AUR packaging, dead Google-Code mirrors, and Python/C++ downstream CAM consumers (cq-cam, HeeksCNC, stl2ngc) — no managed binding anywhere. The shim is genuinely net-new in-house work, but the Boost.Python binding is a complete, precise implementation map, so authoring risk is low. (Brief `[04]` phrasing "pybind11/emscripten" is imprecise: the canonical upstream Python binding is Boost.Python, not pybind11 — immaterial to feasibility; both are OO wraps and neither is a reusable C ABI.)

## [07]-[FALLBACK_COST_MAP]

Author-kernel drop-cutter over the kernel `[V8]` `SignedDistanceFromMesh` index/SDF lane — strictly heavier on both axes, stated honestly:

- **Materially more interior code.** OCL computes exact per-cutter-form contact analytically (`vertexDrop`/`facetDrop`/`edgeDrop`, `CC_CLZ_Pair singleEdgeDropCanonical` per subclass). A mesh SDF gives a clean drop only for the BALL form (iso-offset = ball radius); flat/bull/cone drops are the lower envelope of the cutter solid swept against the surface, which the mesh SDF alone does not provide — each demands a cutter-profile-aware vertical root-find over the field. Re-deriving flat/bull/cone/composite contact is the bulk of `libocl`'s 15-year value, rebuilt from scratch.
- **Resolution-bound approximation.** Sampled-field drops are grid-accuracy-limited where OCL is analytically exact; production scallop/pencil/finish tolerances force fine sampling, forfeiting OCL's exact CC points and its OpenMP+KD-tree performance.
- **Waterline is worse still.** OCL's waterline is an exact push-cutter fiber/interval computation linked by the weave graph. From an SDF it degrades to marching-squares on z-slices — approximate, resolution-bound, and losing exact fiber-interval CC data.
- **Calendar hostage to an UNLANDED plane.** The kernel `[V8]` SDF lane is a geometry-campaign deliverable. Until it lands with the index acceleration, the Fabrication surface plane has no substrate — it then either blocks on the geometry calendar or re-implements the index/SDF itself, the `[V2]` re-implementation defect. The fallback codes the Fabrication surface schedule to the geometry SDF lane's completion; the shim does not.

The fallback's only advantage is operational: no native asset, no RID matrix, no ABI surface — pure managed C# over the kernel field. It trades a bounded, ruled native-asset burden for a large, approximate, calendar-coupled interior. Worse trade.

## [08]-[VERDICT_AND_ADMISSION_CLOSE_ITEMS]

**FEASIBLE — admit as ruled.** The `[V5]` surface engine sites OpenCAMLib behind a thin in-house `extern "C"` C-shim + source-generated P/Invoke on `Toolpath/surface`; the author-kernel drop-cutter is retired to a recorded fallback that stands only on admission abandonment. Path LAYOUT stays kernel-composed (geodesic/extract isolines, flow streamlines, segment direction fields per `[V5]`); OpenCAMLib owns cutter POSITIONING (drop/push/waterline height sampling) only — the two do not overlap.

The residuals are the pre-priced Manifold-class burden the brief already admits, not verdict-changing gaps (no missing RID, no missing entry point, no license conflict):
- **A1 — Per-RID native asset.** The shim is built + bundled with `libocl` for osx-arm64/win-x64/linux-x64 (either against the shipped SHARED archives or from-source per RID). Forge-provisioned exactly like the kernel Z3 precedent; RID assets recorded at admission; content-keyed at the wire.
- **A2 — libomp carriage.** osx-arm64 bundle carries `libomp.dylib` (upstream already rpaths it); linux relies on system `libgomp`; win on MSVC `vcomp`. Or set `USE_OPENMP=OFF` to erase the dependency at a throughput cost — a DECISION build-policy row.
- **A3 — Golden-fixture gate.** Pin drop-cutter/waterline outputs on reference STL fixtures per cutter form; the gate the LGPL native-asset admission requires, normalized by the kernel's Manifold precedent.
- **A4 — Byte-level archive confirmation.** The macOS libomp bundling and release `USE_OPENMP` state are proven from `cxxlib.cmake` install rules and defaults; confirm by unzipping `macos-cxx-arm64.zip` (or building from source, which moots it) at admission. Low risk, mechanism proven.
- **A5 — `.api` catalog.** Author `api-opencamlib.md` with the admission: the `Operation` lifecycle, the cutter-form ctors, the shim ABI, the RID/libomp asset map, the LGPL-2.1 dynamic-link note.
