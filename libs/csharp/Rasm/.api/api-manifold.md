# [RASM_API_MANIFOLD]

Manifold (`elalish/manifold`) is the tier-3 scale companion to the kernel's exact boolean owner: an in-kernel P/Invoke binding reaches the C++ engine through `manifoldc`. `ArrangementPolicy.ScaleCeiling` routes admitted high-scale operations to Manifold, while the managed exact arrangement retains the correctness rail.

## [01]-[PACKAGE_SURFACE]

- Package: `manifoldc`
- Source: `elalish/manifold`
- Headers: `bindings/c/include/manifold/manifoldc.h` and `bindings/c/include/manifold/types.h`
- License: `Apache-2.0`
- Binding: In-house P/Invoke over the C FFI
- Build: CMake with `-DMANIFOLD_CBIND=ON` and `-DBUILD_SHARED_LIBS=ON`
- Distribution: Source tarball; upstream publishes no prebuilt C binaries
- Channels: PyPI wheels and npm WASM artifacts do not carry the RID C asset
- Asset: Per-RID native library with `osx-arm64` as the primary RID
- Handles: Opaque `typedef struct` pointers named `ManifoldManifold`, `ManifoldManifoldVec`, `ManifoldMeshGL`, `ManifoldMeshGL64`, `ManifoldBox`, `ManifoldSimplePolygon`, `ManifoldPolygons`, and `ManifoldExecutionContext`
- Scalar: `ManifoldVec3` stores `double` components
- Cross-section fill: The admitted ABI omits `ManifoldFillRule` from ingest
- Cross-section compose: The admitted ABI omits `manifold_cross_section_compose`
- Simplify: Parameter names derive from the selected header
- Rail: Guaranteed-manifold boolean throughput above the arrangement scale ceiling

The binding targets the C headers directly. The NuGet ID `Manifold` denotes the `Garume/Manifold` CLI and MCP operation source generator, and `ManifoldNET` does not participate in the binding.

## [02]-[MEMORY_LAW]

Every constructor receives a leading `void* mem` sized by its `manifold_*_size()` twin, while `manifold_alloc_*()` mints a malloc-backed object. `manifold_destruct_*()` runs only the destructor for caller-owned storage, and `manifold_delete_*()` also frees storage minted by `manifold_alloc_*()`. Array accessors write into caller-owned output buffers sized from the corresponding `*_length` read. The binding capsules every handle with deterministic release.

| [INDEX] | [SYMBOL]                    | [ROLE]     | [MEMORY_EFFECT]      |
| :-----: | :-------------------------- | :--------- | :------------------- |
|  [01]   | `manifold_manifold_size()`  | sizing     | object-buffer bytes  |
|  [02]   | `manifold_alloc_manifold()` | allocation | malloc-backed object |
|  [03]   | `manifold_destruct_*()`     | teardown   | destructor only      |
|  [04]   | `manifold_delete_*()`       | teardown   | destructor plus free |

## [03]-[CORE_SURFACE]

[CONSTRUCTION_INGEST]:

`manifold_meshgl` ingests `n_verts × n_props` interleaved floats with position properties first and `n_props >= 3`; the triangle buffer carries `3 × n_tris` indices. `manifold_meshgl64` mirrors that layout with `double` properties and `uint64_t` indices, receiving the kernel's SoA `double` lane at the boundary. Mesh raising records invalid input as nonzero `manifold_status`.

| [INDEX] | [SURFACE]                                                                                | [CAPABILITY]         |
| :-----: | :--------------------------------------------------------------------------------------- | :------------------- |
|  [01]   | `manifold_meshgl(mem, float* vert_props, n_verts, n_props, uint32_t* tri_verts, n_tris)` | float mesh ingest    |
|  [02]   | `manifold_meshgl64(mem, double*, n_verts, n_props, uint64_t*, n_tris)`                   | double mesh ingest   |
|  [03]   | `manifold_of_meshgl(mem, mesh)`                                                          | float mesh raising   |
|  [04]   | `manifold_of_meshgl64(mem, mesh)`                                                        | double mesh raising  |
|  [05]   | `manifold_empty(mem)`                                                                    | empty identity       |
|  [06]   | `manifold_copy(mem, m)`                                                                  | manifold copy        |
|  [07]   | `manifold_compose(mem, vec)`                                                             | disjoint union       |
|  [08]   | `manifold_decompose(mem, m)`                                                             | connected components |

[BOOLEAN]:

`manifold_boolean` is the routed binary boolean entry, and `ManifoldOpType` maps the kernel's `BooleanOp` rows. Boolean operations build a lazy CSG tree; `manifold_status`, extraction, or refinement forces evaluation and propagates error status. Planar section and projection operations return `ManifoldPolygons`.

| [INDEX] | [SURFACE]                              | [CAPABILITY]           |
| :-----: | :------------------------------------- | :--------------------- |
|  [01]   | `ManifoldOpType`                       | boolean vocabulary     |
|  [02]   | `MANIFOLD_ADD`                         | union operation        |
|  [03]   | `MANIFOLD_SUBTRACT`                    | difference operation   |
|  [04]   | `MANIFOLD_INTERSECT`                   | intersection operation |
|  [05]   | `manifold_boolean(mem, a, b, op)`      | binary dispatch        |
|  [06]   | `manifold_batch_boolean(mem, vec, op)` | manifold-vector fold   |
|  [07]   | `manifold_union`                       | unrouted named twin    |
|  [08]   | `manifold_difference`                  | unrouted named twin    |
|  [09]   | `manifold_intersection`                | unrouted named twin    |
|  [10]   | `manifold_split(mem1, mem2, a, b)`     | two-sided split        |
|  [11]   | `manifold_split_by_plane`              | plane split            |
|  [12]   | `manifold_trim_by_plane`               | plane trim             |
|  [13]   | `manifold_hull(mem, m)`                | native convex hull     |
|  [14]   | `manifold_slice(mem, m, height)`       | planar section         |
|  [15]   | `manifold_project(mem, m)`             | silhouette projection  |

[EXTRACTION]:

Mesh extraction lowers a manifold into the float or double `MeshGL` representation. Array reads copy into the caller-sized output buffers, and merge reads expose the topological re-weld mapping for an open `MeshGL`.

| [INDEX] | [SURFACE]                                      | [CAPABILITY]         |
| :-----: | :--------------------------------------------- | :------------------- |
|  [01]   | `manifold_get_meshgl(mem, manifold)`           | float mesh lowering  |
|  [02]   | `manifold_get_meshgl64(mem, manifold)`         | double mesh lowering |
|  [03]   | `manifold_meshgl_num_vert(mesh)`               | vertex count         |
|  [04]   | `manifold_meshgl_num_tri(mesh)`                | triangle count       |
|  [05]   | `manifold_meshgl_num_prop(mesh)`               | property count       |
|  [06]   | `manifold_meshgl_vert_properties_length(mesh)` | property-buffer size |
|  [07]   | `manifold_meshgl_tri_length(mesh)`             | index-buffer size    |
|  [08]   | `manifold_meshgl_vert_properties(mem, mesh)`   | vertex-property copy |
|  [09]   | `manifold_meshgl_tri_verts(mem, mesh)`         | triangle-index copy  |
|  [10]   | `manifold_meshgl_merge(mem, mesh)`             | topological re-weld  |
|  [11]   | `manifold_meshgl_merge_from_vert`              | source-vertex map    |
|  [12]   | `manifold_meshgl_merge_to_vert`                | target-vertex map    |
|  [13]   | `manifold_meshgl_tolerance(mesh)`              | receipt tolerance    |

[STATUS]:

`ManifoldError` is the native error vocabulary, and the binding folds every member into `GeometryFault`. `manifold_status` is the first eager read after each boolean. Execution contexts carry cooperative cancellation and progress through the same error rail, with cancellation recording `MANIFOLD_CANCELLED`.

| [INDEX] | [SURFACE]                                       | [CAPABILITY]            |
| :-----: | :---------------------------------------------- | :---------------------- |
|  [01]   | `ManifoldError`                                 | error vocabulary        |
|  [02]   | `MANIFOLD_NO_ERROR`                             | successful status       |
|  [03]   | `MANIFOLD_NOT_MANIFOLD`                         | manifoldness fault      |
|  [04]   | `MANIFOLD_NON_FINITE_VERTEX`                    | finite-coordinate fault |
|  [05]   | `MANIFOLD_INVALID_CONSTRUCTION`                 | construction fault      |
|  [06]   | `MANIFOLD_RESULT_TOO_LARGE`                     | result-size fault       |
|  [07]   | `MANIFOLD_CANCELLED`                            | cancellation fault      |
|  [08]   | `manifold_status(manifold)`                     | eager status read       |
|  [09]   | `manifold_execution_context(mem)`               | execution context       |
|  [10]   | `manifold_execution_context_cancel(context)`    | cancellation request    |
|  [11]   | `manifold_execution_context_cancelled(context)` | cancellation read       |
|  [12]   | `manifold_execution_context_progress(context)`  | progress read           |
|  [13]   | `manifold_with_context(mem, manifold, context)` | context binding         |

[GUARANTEE_EVIDENCE]:

Guarantee reads populate `BooleanReceipt` and `ManifoldStatus` without creating a second correctness rail.

| [INDEX] | [SURFACE]                              | [CAPABILITY]          |
| :-----: | :------------------------------------- | :-------------------- |
|  [01]   | `manifold_is_empty(manifold)`          | emptiness fact        |
|  [02]   | `manifold_genus(manifold)`             | genus fact            |
|  [03]   | `manifold_num_vert(manifold)`          | vertex count          |
|  [04]   | `manifold_num_edge(manifold)`          | edge count            |
|  [05]   | `manifold_num_tri(manifold)`           | triangle count        |
|  [06]   | `manifold_epsilon(manifold)`           | epsilon fact          |
|  [07]   | `manifold_get_tolerance(manifold)`     | tolerance fact        |
|  [08]   | `manifold_surface_area(manifold)`      | surface-area evidence |
|  [09]   | `manifold_volume(manifold)`            | volume evidence       |
|  [10]   | `manifold_bounding_box(mem, manifold)` | bounds evidence       |

## [04]-[LOCAL_ADMISSION]

[BOUNDARY]:
- The arrangement tier-3 route owns every `manifoldc` call site.
- Kind mismatch and nonzero `manifold_status` return a typed `Fin` failure.
- A missing RID asset returns `NativeAssetMissing` `2423`.
- The `Fin` boundary rail contains these failures without exceptions.

[ACTIVATION]:
- Face count: Combined operands exceed `ArrangementPolicy.ScaleCeiling` at `1_000_000` faces
- Native asset: The RID library resolves at runtime
- Fixture: The golden-boolean suite matches the managed exact rail

[PRECISION]:
- Manifold guarantees manifold output at float precision.
- The managed exact arrangement retains exact signs, implicit-point crossings, and cell classification.
- `BooleanReceipt` and `ManifoldStatus` carry the selected engine's evidence.

## [05]-[STACKING_LAW]

[ARRANGEMENT]:
- The managed arrangement owns exact signs, implicit-point crossings, and cell welds.
- Manifold owns throughput above `ArrangementPolicy.ScaleCeiling`.
- `ArrangementOp.MeshBoolean` discriminates the engine from policy, so consumers compose one operation.

[MESH_EDIT]:
- Ingest lowers only the published `MeshSpace` or `MeshEdit` through the `meshgl64` interleaved layout.
- Extraction re-enters predicate-gated geometry once through `MeshEdit.Of`.

[PICO_GK]:
- PicoGK owns Fabrication's voxel and implicit lane.
- Manifold owns the kernel boolean scale gate.

[NATIVE_AUXILIARY]:
- `manifold_slice`, `manifold_project`, and `manifold_hull` remain native-scale surfaces outside kernel routing.
- The kernel slice stack owns slicing, the drawing view owns projection, and the hull tiers own hull operations.
