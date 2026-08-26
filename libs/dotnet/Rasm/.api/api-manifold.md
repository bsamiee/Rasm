# [RASM_API_MANIFOLD]

`manifoldc` binds the Manifold C++ engine as the arrangement's tier-3 scale companion behind the exact boolean owner: `ArrangementPolicy.ScaleCeiling` routes over-ceiling operands to guaranteed-manifold throughput while the managed exact arrangement holds the correctness guarantee.

## [01]-[MEMORY_LAW]

Every constructor takes a leading `void* mem` sized by its `manifold_*_size()` twin; `manifold_alloc_*()` mints a malloc-backed object. `manifold_destruct_*()` runs the destructor over caller-owned storage, `manifold_delete_*()` also frees allocated storage, and array accessors write caller-owned buffers sized from the paired `*_length` read.

| [INDEX] | [SYMBOL]                    | [ROLE]     | [MEMORY_EFFECT]      |
| :-----: | :-------------------------- | :--------- | :------------------- |
|  [01]   | `manifold_manifold_size()`  | sizing     | object-buffer bytes  |
|  [02]   | `manifold_alloc_manifold()` | allocation | malloc-backed object |
|  [03]   | `manifold_destruct_*()`     | teardown   | destructor only      |
|  [04]   | `manifold_delete_*()`       | teardown   | destructor plus free |

## [02]-[CORE_SURFACE]

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

[MANIFOLD_VECTOR]:

`ManifoldManifoldVec` is the only N-ary shape across the boundary — the operand carrier `manifold_batch_boolean` folds and the result carrier `manifold_decompose` and `manifold_compose` exchange. It obeys `[02]-[MEMORY_LAW]` whole, and element reads MINT: `manifold_manifold_vec_get` takes its own leading `void* mem`, so every read handle carries its own release independent of the vector's.

| [INDEX] | [SURFACE]                                 | [CAPABILITY]         |
| :-----: | :---------------------------------------- | :------------------- |
|  [01]   | `manifold_manifold_empty_vec(mem)`        | empty vector         |
|  [02]   | `manifold_manifold_vec(mem, sz)`          | sized vector         |
|  [03]   | `manifold_manifold_vec_reserve(ms, sz)`   | capacity reserve     |
|  [04]   | `manifold_manifold_vec_length(ms)`        | element census       |
|  [05]   | `manifold_manifold_vec_get(mem, ms, idx)` | element read, minted |
|  [06]   | `manifold_manifold_vec_set(ms, idx, m)`   | indexed fill         |
|  [07]   | `manifold_manifold_vec_push_back(ms, m)`  | growth fill          |
|  [08]   | `manifold_manifold_vec_size()`            | sizing               |
|  [09]   | `manifold_alloc_manifold_vec()`           | allocation           |
|  [10]   | `manifold_destruct_manifold_vec(ms)`      | destructor only      |
|  [11]   | `manifold_delete_manifold_vec(ms)`        | destructor plus free |

- A `manifold_manifold_vec` sized at construction fills by `set` at a known arity; `manifold_manifold_vec_reserve` plus `push_back` is the streaming form for an unknown one, and mixing the two over one vector appends past the sized prefix rather than overwriting it.
- `manifold_decompose` returns the vector, never an array — the shell census is `manifold_manifold_vec_length` and each shell is one `get`, so a connected input yields a one-element vector rather than a null or a bare handle.

[EXTRACTION]:

Extraction lowers a manifold into the float or double `MeshGL`; array reads copy into caller-sized buffers, and merge reads expose the topological re-weld map for an open `MeshGL`. EVERY entry point is LANE-SUFFIXED: the `64` infix binds `ManifoldMeshGL64` and its absence binds `ManifoldMeshGL`, so a row copied across lanes without the infix names a symbol the shared object does not export and fails at first call rather than at compile. Kernel bindings take the double lane alone.

| [INDEX] | [FLOAT_LANE]                                   | [DOUBLE_LANE]                                    | [CAPABILITY]         |
| :-----: | :--------------------------------------------- | :----------------------------------------------- | :------------------- |
|  [01]   | `manifold_get_meshgl(mem, manifold)`           | `manifold_get_meshgl64(mem, manifold)`           | mesh lowering        |
|  [02]   | `manifold_meshgl_num_vert(mesh)`               | `manifold_meshgl64_num_vert(mesh)`               | vertex count         |
|  [03]   | `manifold_meshgl_num_tri(mesh)`                | `manifold_meshgl64_num_tri(mesh)`                | triangle count       |
|  [04]   | `manifold_meshgl_num_prop(mesh)`               | `manifold_meshgl64_num_prop(mesh)`               | property count       |
|  [05]   | `manifold_meshgl_vert_properties_length(mesh)` | `manifold_meshgl64_vert_properties_length(mesh)` | property-buffer size |
|  [06]   | `manifold_meshgl_tri_length(mesh)`             | `manifold_meshgl64_tri_length(mesh)`             | index-buffer size    |
|  [07]   | `manifold_meshgl_merge_length(mesh)`           | `manifold_meshgl64_merge_length(mesh)`           | merge-map size       |
|  [08]   | `manifold_meshgl_vert_properties(mem, mesh)`   | `manifold_meshgl64_vert_properties(mem, mesh)`   | vertex-property copy |
|  [09]   | `manifold_meshgl_tri_verts(mem, mesh)`         | `manifold_meshgl64_tri_verts(mem, mesh)`         | triangle-index copy  |
|  [10]   | `manifold_meshgl_merge(mem, mesh)`             | `manifold_meshgl64_merge(mem, mesh)`             | topological re-weld  |
|  [11]   | `manifold_meshgl_merge_from_vert(mem, mesh)`   | `manifold_meshgl64_merge_from_vert(mem, mesh)`   | source-vertex map    |
|  [12]   | `manifold_meshgl_merge_to_vert(mem, mesh)`     | `manifold_meshgl64_merge_to_vert(mem, mesh)`     | target-vertex map    |
|  [13]   | `manifold_meshgl_tolerance(mesh)`              | `manifold_meshgl64_tolerance(mesh)`              | output tolerance     |

- Sizing and allocation twins in `[02]-[MEMORY_LAW]` carry the same infix — `manifold_meshgl64_size()`/`manifold_alloc_meshgl64()`/`manifold_destruct_meshgl64()`/`manifold_delete_meshgl64()` against their unsuffixed peers — so a lane crossing is one infix across the construct, extract, and release triple rather than three independent lookups.
- Lanes differ in element WIDTH as well as in name: the double lane reads `double` properties, `uint64_t` triangle and merge indices, and a `double` tolerance, while the float lane reads `float` properties, `uint32_t` indices, and a `float` tolerance — so a buffer sized from a `*_length` read is sized in the lane's own element type, never in bytes shared across the two.

[RUN_PROVENANCE]:

MeshGL carries its output triangles as RUNS — maximal contiguous index ranges sharing one original-mesh id and one instancing transform — so a boolean output attributes back to the operand that produced each face. This is the only source-identity channel across the boundary; without it a `BooleanCensus` can report counts and volumes and nothing about provenance. Every row is lane-suffixed on the same law as `[EXTRACTION]`.

| [INDEX] | [FLOAT_LANE]                                   | [DOUBLE_LANE]                                    | [CAPABILITY]           |
| :-----: | :--------------------------------------------- | :----------------------------------------------- | :--------------------- |
|  [01]   | `manifold_meshgl_num_run(mesh)`                | `manifold_meshgl64_num_run(mesh)`                | run count              |
|  [02]   | `manifold_meshgl_run_index_length(mesh)`       | `manifold_meshgl64_run_index_length(mesh)`       | run-boundary size      |
|  [03]   | `manifold_meshgl_run_index(mem, mesh)`         | `manifold_meshgl64_run_index(mem, mesh)`         | run start offsets      |
|  [04]   | `manifold_meshgl_run_original_id_length(mesh)` | `manifold_meshgl64_run_original_id_length(mesh)` | id-buffer size         |
|  [05]   | `manifold_meshgl_run_original_id(mem, mesh)`   | `manifold_meshgl64_run_original_id(mem, mesh)`   | per-run source id      |
|  [06]   | `manifold_meshgl_run_transform_length(mesh)`   | `manifold_meshgl64_run_transform_length(mesh)`   | transform-buffer size  |
|  [07]   | `manifold_meshgl_run_transform(mem, mesh)`     | `manifold_meshgl64_run_transform(mem, mesh)`     | per-run instance pose  |
|  [08]   | `manifold_meshgl_run_flags_length(mesh)`       | `manifold_meshgl64_run_flags_length(mesh)`       | flag-buffer size       |
|  [09]   | `manifold_meshgl_run_flags(mem, mesh)`         | `manifold_meshgl64_run_flags(mem, mesh)`         | per-run flag bytes     |
|  [10]   | `manifold_meshgl_backside(mesh, run)`          | `manifold_meshgl64_backside(mesh, run)`          | run-orientation fact   |
|  [11]   | `manifold_meshgl_has_normals(mesh, run)`       | `manifold_meshgl64_has_normals(mesh, run)`       | run-normal presence    |
|  [12]   | `manifold_meshgl_face_id_length(mesh)`         | `manifold_meshgl64_face_id_length(mesh)`         | face-id-buffer size    |
|  [13]   | `manifold_meshgl_face_id(mem, mesh)`           | `manifold_meshgl64_face_id(mem, mesh)`           | per-triangle source id |
|  [14]   | `manifold_meshgl_tangent_length(mesh)`         | `manifold_meshgl64_tangent_length(mesh)`         | tangent-buffer size    |
|  [15]   | `manifold_meshgl_halfedge_tangent(mem, mesh)`  | `manifold_meshgl64_halfedge_tangent(mem, mesh)`  | halfedge tangent copy  |

- `num_run` reads the ORIGINAL-ID count and `run_index` is one longer, so run `i` spans the FLAT `tri_verts` window `[run_index[i], run_index[i+1])` — a triangle-index reading of those boundaries is off by the factor of three every `run_index` value is divisible by. Runs sort by original id and cover `tri_verts` whole.
- Element widths cross the lanes UNEVENLY: `run_index` and `face_id` widen `uint32_t`→`uint64_t` and `run_transform` and `halfedge_tangent` widen `float`→`double`, while `run_original_id` stays `uint32_t` and `run_flags` stays `uint8_t` in both. Copying a lane's buffer types wholesale therefore mis-sizes exactly the two that do not move.
- `face_id` is `num_tri` long and survives simplification as the edge-preserving boundary; absent input face ids fill from Manifold's own coplanar-face pass against the mesh tolerance.
- `run_transform` is `12 × num_run` components — a column-major 3×4 affine per run — and `manifold_get_meshgl_w_normals`/`manifold_get_meshgl64_w_normals` are the extraction forms populating the normal property channel that `has_normals` then reports per run.
- Provenance is EARNED on the input side: `manifold_reserve_ids(uint32_t n)` mints a unique original-id block, `manifold_as_original(mem, m)` re-seats a manifold as its own original, and `manifold_original_id(m)` reads the seated id back, so a boolean's `run_original_id` attributes to operands the caller can name. Without that seating, the output ids are Manifold's own and attribute to nothing the kernel declared.
- The reserved block reaches an ingested mesh through no path this ABI exposes: `manifold_meshgl` and `manifold_meshgl64` take vertex properties and triangle indices ALONE, and every run accessor is a read. A `manifold_reserve_ids` block therefore serves a caller that authors `runOriginalID` through the C++ surface, while a C-FFI ingest earns attribution from `manifold_as_original` and reads it back through `manifold_original_id` — that read-back is the only attribution key this boundary yields.
- `manifold_original_id` returns `int` and reports `-1` for a manifold that is not an original, while every `run_original_id` buffer is `uint32_t`, so the seated read and the run buffer meet across an explicit width crossing and a blind reinterpretation turns the not-an-original sentinel into the largest valid id.

[STATUS]:

`ManifoldError` is the native error vocabulary the binding folds into `GeometryFault`; `manifold_status` is the first eager read after each boolean. An execution context reaches an evaluation ONLY through the eager op that consumes its attachment: `manifold_with_context` returns a COPY carrying the context, and `manifold_status` or a `manifold_refine*` call consumes it. DEFERRED ops — boolean operators, transforms, and the batch fold — IGNORE an attached context and return a result carrying none, so a context bound to the operands governs nothing and its progress read reports an evaluation that never ran under it. The attachment belongs on the RESULT, immediately before the terminal force, and cancellation there records `MANIFOLD_CANCELLED`.

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
|  [13]   | `manifold_with_context(mem, manifold, context)` | result-side attachment  |
|  [14]   | `manifold_execution_context_size()`             | sizing                  |
|  [15]   | `manifold_alloc_execution_context()`            | allocation              |
|  [16]   | `manifold_destruct_execution_context(context)`  | destructor only         |
|  [17]   | `manifold_delete_execution_context(context)`    | destructor plus free    |

- Cancellation is sticky and granular per boolean, and the context is safe to read and write from any thread, so the cancel request may cross from a caller thread while the evaluation runs.
- The ctx-aware static factories — `manifold_execution_context_level_set`, `_level_set_seq`, `_of_meshgl`, `_of_meshgl64`, `_smooth`, `_smooth64` — take the context as an argument because they have no source manifold to attach one to; every other op reaches a context only through `manifold_with_context`, so a construction that must report progress spells the ctx-aware form rather than binding after the fact.

[GUARANTEE_EVIDENCE]:

Guarantee reads populate `BooleanCensus` and `ManifoldStatus` without a second correctness owner.

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

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every op folds through the `void* mem` sizing ABI with deterministic release; Manifold guarantees manifold output at float precision, the managed exact arrangement retaining exact signs, implicit-point crossings, and cell classification.
- `manifold_status` forces eagerly onto the single `BooleanCensus`/`ManifoldStatus` evidence pair, and it is also the op that consumes a context attachment — so the binding attaches at the result immediately before that read, never at the operands a deferred fold discards the context from.
- Lane infix rides the SYMBOL, never the handle type the .NET side declares: `nint` erases `ManifoldMeshGL` and `ManifoldMeshGL64` to one shape, so nothing but the entry-point spelling keeps the two lanes apart and a mis-suffixed `LibraryImport` fails at first call rather than at compile. Kernel bindings declare the `meshgl64` lane only.

[STACKING]:
- Arrangement engine split: the managed arrangement owns exact signs, implicit-point crossings, and cell welds; Manifold owns throughput above `ArrangementPolicy.ScaleCeiling`; `ArrangementOp.MeshBoolean` discriminates engine from policy so consumers compose one operation.
- Mesh edit: ingest lowers the published `MeshSpace` or `MeshEdit` through the `meshgl64` interleaved layout; extraction re-enters predicate-gated geometry once through `MeshEdit.Of`.
- Fabrication split: PicoGK owns the voxel and implicit lane; Manifold owns the kernel boolean scale gate.
- Native auxiliary: `manifold_slice`, `manifold_project`, and `manifold_hull` stay native-scale surfaces outside kernel routing — the kernel slice stack owns slicing, the drawing view projection, and the hull tiers hull operations.

[LOCAL_ADMISSION]:
- Arrangement tier-3 routing owns every `manifoldc` call site, activating when combined operands exceed `ArrangementPolicy.ScaleCeiling`, the per-RID native asset resolves, and the native engine's genus/area/volume/bounds guarantee reads agree with the managed exact owner.
- Kind mismatch and nonzero `manifold_status` fold to a typed `Fin` failure; a missing per-RID asset over the ceiling folds to `NativeAssetMissing`; the `Fin` boundary result contains both without exceptions.
