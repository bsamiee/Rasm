# [RASM_API_MANIFOLD]

Manifold (`elalish/manifold`) is the tier-3 SCALE companion for the kernel's exact boolean owner: a guaranteed-manifold C++ boolean engine reached through a thin in-house P/Invoke over its C FFI (`manifoldc`), routed ONLY when `ArrangementPolicy.ScaleCeiling` (1_000_000 combined operand faces) trips AND the per-RID native asset resolves — else the call fails typed as `NativeAssetMissing` 2423. The managed exact arrangement (`Meshing/arrangement`) stays the ONE correctness rail; Manifold is never a second correctness path, and activation is gated behind the arrangement page's golden-boolean fixture law. There is NO NuGet pin: the `Manifold` NuGet ID is a REJECTED HOMONYM — a CLI/MCP operation source-generator framework (`github.com/Garume/Manifold`), zero geometry — and `ManifoldNET` (`weianweigan/manifold-csharp`) is REJECTED-STALLED at (2024-08, no stable release, ~22 months behind upstream). The binding is authored in-kernel against the header below.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `manifoldc` (in-house P/Invoke charter — no package)
- source: `github.com/elalish/manifold`, tag; header `bindings/c/include/manifold/manifoldc.h`, enums `bindings/c/include/manifold/types.h`
- license: Apache-2.0 (repo SPDX + per-file headers)
- release cadence: active — 2026-05, 2026-06, 2026-06; upstream ships NO prebuilt C binaries (source tarball only; PyPI/npm channels are wheel/WASM only)
- native asset: built per-RID via CMake `-DMANIFOLD_CBIND=ON -DBUILD_SHARED_LIBS=ON`; the RID asset set (osx-arm64 first) is recorded at admission and its resolution is the runtime gate
- object model: opaque `typedef struct` pointers — `ManifoldManifold`, `ManifoldManifoldVec`, `ManifoldMeshGL`, `ManifoldMeshGL64`, `ManifoldBox`, `ManifoldSimplePolygon`, `ManifoldPolygons`, `ManifoldExecutionContext`; vec structs are double-precision (`ManifoldVec3 {double}`)
- drift guard: this catalog pins; master already drifts (`ManifoldFillRule` removed from cross-section ingest, `manifold_cross_section_compose` removed, `simplify` param rename) — a version bump re-verifies the cross-section rows before any binding edit

## [02]-[MEMORY_LAW]

Every constructor takes a leading `void* mem` — a caller-provided buffer sized by the matching `manifold_*_size()` — or `manifold_alloc_*()` mints a malloc-backed object. Teardown is split by ownership: `manifold_destruct_*` runs the destructor only (caller-owned buffers); `manifold_delete_*` destructs AND frees (`manifold_alloc_*` pointers). Array extraction accessors also take `void* mem` as the OUTPUT buffer, sized from the `*_length` twin queried first. The binding wraps every handle in a capsule with deterministic release; a handle with no deterministic close is the named leak defect.

| [INDEX] | [SYMBOL] | [ROLE] | [CAPABILITY] |
|:-----: |:------------------------------------------------ |:--------------- |:---------------------------------------------------- |
| [01] | `manifold_manifold_size()` (per-type family) | sizing | byte size for a caller-provided object buffer |
| [02] | `manifold_alloc_manifold()` (per-type family) | allocation | malloc-backed object; pair with `manifold_delete_*` |
| [03] | `manifold_destruct_*(p)` / `manifold_delete_*(p)` | teardown | destructor-only vs destruct+free, split by ownership |

## [03]-[CORE_SURFACE]

[CONSTRUCTION_INGEST]:

| [INDEX] | [SYMBOL] | [CAPABILITY] |
|:-----: |:----------------------------------------------------------------------- |:--------------------------------------------------------------------- |
| [01] | `manifold_meshgl(mem, float* vert_props, n_verts, n_props, uint32_t* tri_verts, n_tris)` | mesh ingest — interleaved `n_verts×n_props` floats (positions first, `n_props >= 3`) + `3×n_tris` indices |
| [02] | `manifold_meshgl64(mem, double*, n_verts, n_props, uint64_t*, n_tris)` | double/`uint64` ingest mirror — the kernel's SoA `double` lane feeds this arity |
| [03] | `manifold_of_meshgl(mem, mesh)` / `manifold_of_meshgl64(mem, mesh)` | raise MeshGL → Manifold; NEVER aborts on bad input — the result carries non-zero `manifold_status` |
| [04] | `manifold_empty(mem)` / `manifold_copy(mem, m)` | identity/copy constructors |
| [05] | `manifold_compose(mem, vec)` / `manifold_decompose(mem, m)` | disjoint-union compose and connected-component split |

[BOOLEAN]:

| [INDEX] | [SYMBOL] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------------------ |:--------------------------------------------------------------------- |
| [01] | `ManifoldOpType` — `MANIFOLD_ADD` / `MANIFOLD_SUBTRACT` / `MANIFOLD_INTERSECT` | the closed op vocabulary the kernel `BooleanOp` rows map onto |
| [02] | `manifold_boolean(mem, a, b, op)` | the ONE binary boolean entry — union/difference/intersection by op |
| [03] | `manifold_batch_boolean(mem, vec, op)` | n-ary boolean over a manifold vector |
| [04] | `manifold_union` / `manifold_difference` / `manifold_intersection` | named twins of [02]; the binding routes [02] only |
| [05] | `manifold_split(mem1, mem2, a, b)` / `manifold_split_by_plane(...)` / `manifold_trim_by_plane(...)` | two-sided split and plane trim |
| [06] | `manifold_hull(mem, m)` | convex hull at native scale |
| [07] | `manifold_slice(mem, m, height)` / `manifold_project(mem, m)` | planar section / silhouette projection → `ManifoldPolygons` |

Booleans are LAZY (CSG tree): evaluation forces on the first eager read (`manifold_status`, extraction, refine); error status propagates through composed operations.

[EXTRACTION]:

| [INDEX] | [SYMBOL] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------------------ |:--------------------------------------------------------------------- |
| [01] | `manifold_get_meshgl(mem, m)` / `manifold_get_meshgl64(mem, m)` | lower Manifold → MeshGL (float / double mirror) |
| [02] | `manifold_meshgl_num_vert/num_tri/num_prop(m)` | census reads |
| [03] | `manifold_meshgl_vert_properties_length/tri_length(m)` | buffer sizing for the array reads |
| [04] | `manifold_meshgl_vert_properties(mem, m)` / `manifold_meshgl_tri_verts(mem, m)` | copy-out into the caller's sized buffer |
| [05] | `manifold_meshgl_merge(mem, m)` + `merge_from_vert`/`merge_to_vert` reads | topological re-weld of an open MeshGL |
| [06] | `manifold_meshgl_tolerance(m)` | the result's tolerance fact for the receipt |

[STATUS_AND_GUARANTEE]:

| [INDEX] | [SYMBOL] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------------------ |:--------------------------------------------------------------------- |
| [01] | `ManifoldError` — `MANIFOLD_NO_ERROR`, `MANIFOLD_NOT_MANIFOLD`, `MANIFOLD_NON_FINITE_VERTEX`, `MANIFOLD_INVALID_CONSTRUCTION`, `MANIFOLD_RESULT_TOO_LARGE`, `MANIFOLD_CANCELLED`, … | the closed error vocabulary the binding folds into `GeometryFault` |
| [02] | `manifold_status(m)` | the eager status read — the FIRST read after every boolean |
| [03] | `manifold_is_empty/genus/num_vert/num_edge/num_tri(m)` | guarantee properties for the `BooleanReceipt` |
| [04] | `manifold_epsilon(m)` / `manifold_get_tolerance(m)` | numeric-conditioning facts |
| [05] | `manifold_surface_area(m)` / `manifold_volume(m)` / `manifold_bounding_box(mem, m)` | mass/bounds evidence |
| [06] | `manifold_execution_context(mem)` + `_cancel/_cancelled/_progress` + `manifold_with_context(mem, m, ctx)` | cooperative cancellation; surfaces as `MANIFOLD_CANCELLED` |

## [04]-[LOCAL_ADMISSION]

- The binding is a BOUNDARY KERNEL: every `manifoldc` call site lives inside the arrangement page's tier-3 route; no other page reaches the FFI. Kind mismatch, non-zero `manifold_status`, and a missing RID asset all route `Fin` fail with the typed fault — `NativeAssetMissing` 2423 for the asset gate, never a throw.
- Activation gate, three conditions AND-ed: `ArrangementPolicy.ScaleCeiling` (1_000_000 combined operand faces) exceeded; the RID native asset resolves at runtime; the golden-boolean fixture suite passes against the managed exact rail before the route is admitted for a build.
- Manifold guarantees OUTPUT manifoldness at float precision — it is the scale companion, NOT an exactness tier: signs and cell classification at correctness grade stay with the managed exact arrangement; a Manifold result is evidence-carried (`BooleanReceipt` + `ManifoldStatus`) and never silently substitutes the exact path below the ceiling.
- Rejected bindings, never re-proposed: the `Manifold` NuGet ID homonym (CLI/MCP source-generator framework); `ManifoldNET` (stalled, 2024-08).

## [05]-[STACKING_LAW]

- vs `Meshing/arrangement`: the managed exact arrangement owns correctness (exact signs, implicit-point crossings, cell welds); Manifold owns throughput above the ceiling. One `ArrangementOp.MeshBoolean` entry discriminates the route on the policy row — the consumer never selects an engine.
- vs `Meshing/edit`: ingest lowers from the frozen `MeshSpace`/`MeshEdit` publish (never a mid-build arena) into the `meshgl64` interleaved layout; extraction re-admits through `MeshEdit.Of` so the result re-enters the predicate-gated world exactly once.
- vs PicoGK (Fabrication stratum): PicoGK is Fabrication's voxel/implicit lane; Manifold is the kernel's boolean scale gate — distinct strata, no shared concern, never a second SDF owner.
- vs `manifold_slice`/`manifold_project`/`manifold_hull`: available at native scale but NOT the kernel owners — `Meshing/slice` (kernel slice stack), `Drawing/view` (projection), and the `[V11]` hull tiers own those concerns; the native twins stay unrouted absent a ruled scale demand.
