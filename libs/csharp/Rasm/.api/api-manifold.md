# [RASM_API_MANIFOLD]

`manifoldc` binds the Manifold C++ engine as the arrangement's tier-3 scale companion behind the exact boolean owner: `ArrangementPolicy.ScaleCeiling` routes over-ceiling operands to guaranteed-manifold throughput while the managed exact arrangement holds the correctness rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `manifoldc`
- package: `manifoldc` (`Apache-2.0`, `elalish/manifold`)
- role: in-repo P/Invoke binding over the C FFI, no NuGet package
- abi: C headers `manifoldc.h` and `types.h`; `ManifoldVec3` carries `double` components
- handles: `ManifoldManifold` `ManifoldManifoldVec` `ManifoldMeshGL` `ManifoldMeshGL64` `ManifoldBox` `ManifoldSimplePolygon` `ManifoldPolygons` `ManifoldExecutionContext`
- asset: per-RID native library, `osx-arm64` the primary RID
- rail: guaranteed-manifold boolean throughput above the arrangement scale ceiling

## [02]-[MEMORY_LAW]

Every constructor takes a leading `void* mem` sized by its `manifold_*_size()` twin; `manifold_alloc_*()` mints a malloc-backed object. `manifold_destruct_*()` runs the destructor over caller-owned storage, `manifold_delete_*()` also frees allocated storage, and array accessors write caller-owned buffers sized from the paired `*_length` read.

| [INDEX] | [SYMBOL]                    | [ROLE]     | [MEMORY_EFFECT]      |
| :-----: | :-------------------------- | :--------- | :------------------- |
|  [01]   | `manifold_manifold_size()`  | sizing     | object-buffer bytes  |
|  [02]   | `manifold_alloc_manifold()` | allocation | malloc-backed object |
|  [03]   | `manifold_destruct_*()`     | teardown   | destructor only      |
|  [04]   | `manifold_delete_*()`       | teardown   | destructor plus free |

## [03]-[CORE_SURFACE]

[CONSTRUCTION_INGEST]:

`manifold_meshgl` ingests `n_verts × n_props` interleaved floats, positions first with `n_props >= 3`, and `3 × n_tris` triangle indices; `manifold_meshgl64` mirrors the layout with `double` properties and `uint64_t` indices, taking the kernel's SoA `double` lane. Invalid input raises nonzero `manifold_status`.

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

`manifold_boolean` is the routed binary entry, `ManifoldOpType` mapping the kernel's `BooleanOp` rows. Booleans build a lazy CSG tree that `manifold_status`, extraction, or refinement forces to evaluate, propagating error status; planar section and projection return `ManifoldPolygons`.

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

Extraction lowers a manifold into the float or double `MeshGL`; array reads copy into caller-sized buffers, and merge reads expose the topological re-weld map for an open `MeshGL`.

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

`ManifoldError` is the native error vocabulary the binding folds into `GeometryFault`; `manifold_status` is the first eager read after each boolean. Execution contexts thread cooperative cancellation and progress through the same rail, cancellation recording `MANIFOLD_CANCELLED`.

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

Guarantee reads populate `BooleanReceipt` and `ManifoldStatus` without a second correctness rail.

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

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every op folds through the `void* mem` sizing ABI with deterministic release; Manifold guarantees manifold output at float precision, the managed exact arrangement retaining exact signs, implicit-point crossings, and cell classification.
- `manifold_status` forces eagerly onto the single `BooleanReceipt`/`ManifoldStatus` evidence rail.

[STACKING]:
- Arrangement engine split: the managed arrangement owns exact signs, implicit-point crossings, and cell welds; Manifold owns throughput above `ArrangementPolicy.ScaleCeiling`; `ArrangementOp.MeshBoolean` discriminates engine from policy so consumers compose one operation.
- Mesh edit: ingest lowers the published `MeshSpace` or `MeshEdit` through the `meshgl64` interleaved layout; extraction re-enters predicate-gated geometry once through `MeshEdit.Of`.
- Fabrication split: PicoGK owns the voxel and implicit lane; Manifold owns the kernel boolean scale gate.
- Native auxiliary: `manifold_slice`, `manifold_project`, and `manifold_hull` stay native-scale surfaces outside kernel routing — the kernel slice stack owns slicing, the drawing view projection, and the hull tiers hull operations.

[LOCAL_ADMISSION]:
- Arrangement tier-3 routing owns every `manifoldc` call site, activating when combined operands exceed `ArrangementPolicy.ScaleCeiling`, the per-RID native asset resolves, and the golden-boolean fixture matches the managed exact rail.
- Kind mismatch and nonzero `manifold_status` fold to a typed `Fin` failure; a missing per-RID asset over the ceiling folds to `NativeAssetMissing`; the `Fin` boundary rail contains both without exceptions.

[RAIL_LAW]:
- Package: `manifoldc`
- Owns: guaranteed-manifold boolean throughput above `ArrangementPolicy.ScaleCeiling`, its lazy CSG evaluation, float and double `MeshGL` ingest and extraction, the native `manifold_hull`/`manifold_slice`/`manifold_project` surfaces, the genus/area/volume/bounds guarantee reads, and the deterministic-release `void* mem` ABI with `ManifoldExecutionContext` cancellation and progress.
- Accept: high-scale booleans routed above `ArrangementPolicy.ScaleCeiling`, the kernel SoA `double` lane lowered through `meshgl64`, `ManifoldError` folded into `GeometryFault`, and cancellation through the execution-context rail.
- Reject: a NuGet reference, the in-repo binding owning no package and the `Manifold`/`ManifoldNET` NuGet IDs naming unrelated projects; the unrouted `manifold_union`/`manifold_difference`/`manifold_intersection` twins in place of `manifold_boolean`; a second correctness rail beside the managed exact arrangement; an exception in place of the `Fin` boundary fold.
