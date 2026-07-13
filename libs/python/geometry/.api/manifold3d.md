# [PY_GEOMETRY_API_MANIFOLD3D]

`manifold3d` supplies the geometrically-robust watertight mesh kernel for the geometry CSG/boolean rail: `Manifold` (3D solid, guaranteed-manifold boolean/extrude/revolve/refine/SDF/hull/Minkowski), `CrossSection` (2D polygon CSG and offset over Clipper2), `Mesh`/`Mesh64` (interleaved triangle-soup carriers with merge/run/normal/tangent channels for lossless round-trip), `ExecutionContext` (cancel/progress on long-running ingest/level-set/smooth), and `RayHit` (one hit on a ray-segment cast). The geometry owner composes lazy-transform chains, `batch_boolean` n-ary CSG, and the SDF `level_set` constructor into the spatial-operation union; it never re-implements Clipper2 offsetting, the manifold boolean kernel, marching-tetrahedra, or convex-hull. Compiled nanobind/scikit-build extension — the in-memory `Mesh`/`Mesh64` arrays are the wire to the data mesh codec, never a file.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `manifold3d`
- package: `manifold3d`
- import: `import manifold3d`
- owner: `geometry`
- rail: geometry-csg / spatial-operations
- installed: `3.5.1`
- capability: guaranteed-manifold 3D boolean CSG, 2D polygon boolean/offset over Clipper2, lazy chained affine/warp transforms, SDF marching-tetrahedra level sets, convex hull (single + batch), Minkowski sum/difference, edge refinement and tolerance simplification, normal/curvature/arbitrary vertex-property computation, ray-segment casting, and async cancel/progress on long ops

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry carriers
- rail: geometry-csg

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]     | [CAPABILITY]                                                                             |
| :-----: | :----------------- | :---------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `Manifold`         | watertight solid  | boolean CSG, transform, extrude/revolve, refine, simplify, hull, Minkowski, SDF, queries |
|  [02]   | `CrossSection`     | 2D polygon set    | boolean, offset, hull, decompose, extrude/revolve, transform over Clipper2               |
|  [03]   | `Mesh`             | f32 triangle soup | `vert_properties`/`tri_verts` plus merge/run/transform/flags/face-id/halfedge-tangent    |
|  [04]   | `Mesh64`           | f64 triangle soup | identical channels to `Mesh` with 64-bit positions/indices for large meshes              |
|  [05]   | `ExecutionContext` | async op context  | `cancel`/`cancelled`/`progress` plus context-bound `from_mesh`/`level_set`/`smooth`      |
|  [06]   | `RayHit`           | ray-segment hit   | `face_id`, `distance`, `position`, `normal` for a single intersection                    |

[PUBLIC_TYPE_SCOPE]: bounded vocabularies
- rail: geometry-csg

`FillRule` resolves self-intersecting contours into a clean section.

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]  | [CAPABILITY]                                                                            |
| :-----: | :--------- | :------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `OpType`   | boolean opcode | `Add` (=union), `Subtract` (=difference, tail differenced from head), `Intersect`       |
|  [02]   | `FillRule` | polygon fill   | `EvenOdd`, `NonZero`, `Positive` (default), `Negative`                                  |
|  [03]   | `JoinType` | offset join    | `Square`, `Round` (default), `Miter`, `Bevel`                                           |
|  [04]   | `Error`    | status code    | `NoError`..`Cancelled` (15 cases: `NonFiniteVertex`/`NotManifold`/`ResultTooLarge`/...) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Manifold construction
- rail: geometry-csg

`Manifold(mesh)` ingests a `Mesh`/`Mesh64` (merging by the merge vectors, setting an `Error` status if not an oriented 2-manifold); the empty `Manifold()` is the identity element of boolean folds. Method surfaces elide the `Manifold.` prefix; primitive constructors default `circular_segments=0` and `center=False`; the `extrude`/`revolve` tail args are spelled on the `CrossSection` rows.

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `Manifold()` / `Manifold(mesh: Mesh\|Mesh64)`             | constructor    | empty solid, or lossless ingest of a triangle soup    |
|  [02]   | `cube(size=(1,1,1))`                                      | primitive      | axis-aligned box                                      |
|  [03]   | `sphere(radius)`                                          | primitive      | geodesic sphere (octahedron refinement)               |
|  [04]   | `cylinder(height, radius_low, radius_high=-1)`            | primitive      | cylinder or cone                                      |
|  [05]   | `tetrahedron()`                                           | primitive      | unit tetrahedron                                      |
|  [06]   | `extrude(crossSection, height, ...)`                      | constructor    | extrude `CrossSection` along Z (twist/taper)          |
|  [07]   | `revolve(crossSection, ...)`                              | constructor    | revolve `CrossSection` about Y                        |
|  [08]   | `smooth(mesh, sharpened_edges=[], edge_smoothness=[])`    | constructor    | tangent-creating smooth (interpolate via refine)      |
|  [09]   | `level_set(f, bounds, edgeLength, level=0, tolerance=-1)` | constructor    | SDF marching-tetrahedra over `f(x,y,z)->float`        |
|  [10]   | `batch_boolean(manifolds, op: OpType)`                    | set operation  | n-ary CSG; empty=>identity, single=>no-op             |
|  [11]   | `batch_hull(manifolds)` / `hull_points(pts: (N,3) f64)`   | constructor    | convex hull of a set of solids or a point cloud       |
|  [12]   | `compose(manifolds)`                                      | set operation  | [DEPRECATED]; use `batch_boolean(.., OpType.Add)`     |
|  [13]   | `reserve_ids(n) -> int`                                   | identity       | reserve n sequential mesh IDs for multi-material runs |

[ENTRYPOINT_SCOPE]: Manifold operators, transforms, and lazy chaining
- rail: geometry-csg

Boolean operators `__add__`/`__sub__`/`__xor__` are union/difference/intersection; transforms are combined and applied lazily, so chained `.translate().rotate().scale()` collapses to one matrix.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `a + b` / `a - b` / `a ^ b`                                        | boolean        | union / difference / intersection shorthand  |
|  [02]   | `m.transform(m3x4)`                                                | transform      | affine 3x3+translation, lazy                 |
|  [03]   | `m.translate(v3)` / `m.rotate(v3_degrees)` / `m.scale(v3\|float)`  | transform      | lazy chained translate/Euler-rotate/scale    |
|  [04]   | `m.mirror(normal3)`                                                | transform      | reflect across plane (zero normal => empty)  |
|  [05]   | `m.warp(f: vec3->vec3)` / `m.warp_batch(f: (N,3)->(N,3))`          | transform      | arbitrary per-vertex/vectorized displacement |
|  [06]   | `m.hull()`                                                         | geometry       | convex hull of this solid                    |
|  [07]   | `m.decompose() -> list[Manifold]`                                  | topology       | split topologically-disconnected components  |
|  [08]   | `m.split(cutter) -> (intersection, difference)`                    | geometry       | one-pass cut by another manifold             |
|  [09]   | `m.split_by_plane(normal3, origin_offset) -> (Manifold, Manifold)` | geometry       | half-space cut                               |
|  [10]   | `m.trim_by_plane(normal3, origin_offset)`                          | geometry       | keep only the normal-side half               |
|  [11]   | `m.minkowski_sum(other)` / `m.minkowski_difference(other)`         | morphology     | morphological dilation / erosion             |

[ENTRYPOINT_SCOPE]: Manifold mesh refinement and vertex properties
- rail: geometry-csg

`set_tolerance` increasing the value performs simplification; `simplify(tolerance)` returns a coarsened copy without changing the stored tolerance.

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `m.refine(n)`                                                        | mesh           | split each edge n-ways (interpolate tangents)   |
|  [02]   | `m.refine_to_length(length)`                                         | mesh           | subdivide to roughly max edge length            |
|  [03]   | `m.refine_to_tolerance(tolerance)`                                   | mesh           | curvature-adaptive subdivision to tolerance     |
|  [04]   | `m.simplify(tolerance)` / `m.set_tolerance(t)` / `m.get_tolerance()` | mesh           | coarsen / set+simplify / read tolerance         |
|  [05]   | `m.smooth_out(min_sharp_angle=52.5, min_smoothness=0)`               | mesh           | fill tangents from geometry (G1, sharp-safe)    |
|  [06]   | `m.smooth_by_normals(normal_idx=0)`                                  | mesh           | fill tangents from stored vertex normals        |
|  [07]   | `m.calculate_normals(normal_idx=0, min_sharp_angle=52.5)`            | properties     | compute+record vertex normals (slot 0)          |
|  [08]   | `m.calculate_curvature(gaussian_idx, mean_idx)`                      | properties     | store Gaussian/mean curvature on channels       |
|  [09]   | `m.set_properties(new_num_prop, f: (pos, props)->props)`             | properties     | recompute/grow/shrink property channels         |
|  [10]   | `m.as_original()`                                                    | identity       | drop ancestor relations, mark as a new original |

[ENTRYPOINT_SCOPE]: Manifold queries, export, and async
- rail: geometry-csg

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [CAPABILITY]                                              |
| :-----: | :-------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `m.to_mesh(normal_idx=-1)` / `m.to_mesh64(normal_idx=-1)` | export         | interleaved `Mesh`/`Mesh64` (recorded normal slot)        |
|  [02]   | `m.status() -> Error`                                     | validation     | construction error code (carries through ops, incl. NaN)  |
|  [03]   | `m.is_empty()`                                            | query          | no triangles present                                      |
|  [04]   | `m.genus()`                                               | topology       | topological genus (call `decompose` first)                |
|  [05]   | `m.volume()` / `m.surface_area()`                         | measurement    | enclosed volume / surface area (epsilon-clamped)          |
|  [06]   | `m.bounding_box() -> (xmin,ymin,zmin,xmax,ymax,zmax)`     | query          | axis-aligned bounds tuple                                 |
|  [07]   | `m.num_vert()` / `m.num_edge()` / `m.num_tri()`           | query          | vertex / edge / triangle counts                           |
|  [08]   | `m.num_prop()` / `m.num_prop_vert()` / `m.original_id()`  | query          | per-vertex property count / property-vert count / mesh ID |
|  [09]   | `m.ray_cast(origin3, endpoint3) -> list[RayHit]`          | query          | cast a ray segment -> all hits sorted by distance         |
|  [10]   | `m.min_gap(other, search_length) -> float`                | query          | minimum gap to another solid (0..search_length)           |
|  [11]   | `m.slice(height) -> CrossSection`                         | projection     | X-Y cross-section at a Z height                           |
|  [12]   | `m.project() -> CrossSection`                             | projection     | X-Y projected outline (run through Positive fill)         |
|  [13]   | `m.with_context(ctx: ExecutionContext)`                   | async          | attach context; consumed by next eager op (status/refine) |

[ENTRYPOINT_SCOPE]: CrossSection operations
- rail: geometry-csg

`CrossSection(contours, fillrule=Positive)` unions the contours into an intersection-free section; operators and transforms mirror `Manifold`. Static factory surfaces elide the `CrossSection.` prefix.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `CrossSection(contours: list[(N,2)], fillrule=FillRule.Positive)`          | constructor    | union contours into a clean section       |
|  [02]   | `square(size2, center=False)` / `.circle(radius, circular_segments=0)`     | primitive      | rectangle / circle polygon                |
|  [03]   | `batch_boolean(sections, op)` / `.compose(sections)`                       | set operation  | n-ary boolean / union                     |
|  [04]   | `batch_hull(sections)` / `.hull_points(pts: (N,2))`                        | constructor    | convex hull of sections / 2D points       |
|  [05]   | `cs + cs2` / `cs - cs2` / `cs ^ cs2`                                       | boolean        | union / difference / intersection         |
|  [06]   | `cs.offset(delta, join_type=Round, miter_limit=2.0, circular_segments=0)`  | offset         | grow/shrink contours (Clipper2)           |
|  [07]   | `cs.hull()` / `cs.decompose() -> list[CrossSection]`                       | geometry       | convex hull / split disconnected outlines |
|  [08]   | `cs.simplify(epsilon=1e-6)`                                                | geometry       | drop near-collinear verts (post-offset)   |
|  [09]   | `cs.translate(v2)` / `.rotate(deg)` / `.scale(v2\|float)` / `.mirror(ax2)` | transform      | lazy chained 2D affine                    |
|  [10]   | `cs.transform(m2x3)` / `.warp(f)` / `.warp_batch(f)`                       | transform      | matrix / per-vertex warp                  |
|  [11]   | `cs.extrude(height, n_divisions=0, twist_degrees=0, scale_top=(1,1))`      | constructor    | extrude to `Manifold` (twist/taper)       |
|  [12]   | `cs.revolve(circular_segments=0, revolve_degrees=360)`                     | constructor    | revolve to `Manifold`                     |
|  [13]   | `cs.to_polygons() -> list[(N,2)]`                                          | export         | contour vertex arrays                     |
|  [14]   | `cs.area()` / `cs.num_contour()` / `cs.num_vert()`                         | query          | signed area / contour + vertex counts     |
|  [15]   | `cs.bounds()` / `cs.is_empty()`                                            | query          | bounds / emptiness                        |

[ENTRYPOINT_SCOPE]: module-level functions
- rail: geometry-csg

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------------ | :------------- | :----------------------------------------------- |
|  [01]   | `triangulate(polygons, epsilon=-1, allow_convex=True) -> (N,3) int` | utility        | epsilon-valid polygons to index triples          |
|  [02]   | `get_circular_segments(radius) -> int`                              | config         | resolve segment count for a radius               |
|  [03]   | `set_circular_segments(n)`                                          | config         | exact count, overrides angle/length              |
|  [04]   | `set_min_circular_angle(degrees)`                                   | config         | min angle per segment (rounds up to factor of 4) |
|  [05]   | `set_min_circular_edge_length(length)`                              | config         | min segment edge length                          |

## [04]-[IMPLEMENTATION_LAW]

[CSG_TOPOLOGY]:
- import: `import manifold3d` at boundary scope only; module-level import is banned by the manifest import policy.
- boolean axis: `OpType.Add`=union, `OpType.Subtract`=difference (tail differenced from head), `OpType.Intersect`=intersection. Three-or-more-operand CSG folds through `batch_boolean(sequence, op)` (empty=>identity `Manifold`, single=>no-op), never a manual `reduce` over `+`/`-`/`^` — `batch_boolean` is the single n-ary owner. `compose` is deprecated; route topological union through `batch_boolean(.., OpType.Add)`.
- transform axis: `transform`/`translate`/`rotate`/`scale`/`mirror`/`warp` are lazy — combined and applied on the next eager op, so a transform chain is one fused matrix, not n passes. Euler `rotate` is in z-y'-x" (yaw/pitch/roll) order, exact for multiples of 90 degrees.
- mesh-carrier axis: `Mesh`/`Mesh64` carry `vert_properties` ((N, numProp) where cols 0..2 are XYZ, optional normals/curvature beyond), `tri_verts` ((N,3) index), plus `merge_from_vert`/`merge_to_vert` (manifold-stitching), `run_index`/`run_original_id`/`run_transform`/`run_flags` (multi-material runs), `face_id`, `halfedge_tangent`, and `tolerance`. `Mesh.merge()` best-effort re-stitches lost merge vectors after a file round-trip. `Manifold(mesh)` ingest sets a non-`NoError` `status()` rather than raising when the soup is not an oriented 2-manifold — gate on `status()`, not exceptions.
- validation axis: every constructor that can fail sets `status() -> Error` (carrying through subsequent ops including NaN propagation); the boundary checks `status() == Error.NoError` and `is_empty()` rather than trusting a returned non-empty object. `level_set` requires a Python `f(x,y,z)->float` and explicit `bounds`/`edgeLength`; the SDF need not be a true distance or continuous.
- ray axis: `ray_cast(origin, endpoint)` casts a finite segment and returns `list[RayHit]` sorted by distance (empty list = miss) — it is per-ray with no vectorized batch entry, so a ray-batch is a Python comprehension over the rays (the one CPU-bound spot handed to the geometry offload lane for large batches; a future release exposing a batch ray entry is a one-line arm change). `RayHit.face_id == -1` flags an invalid hit.
- async axis: `ExecutionContext` (`cancel`/`cancelled`/`progress`) attaches via `with_context` and is consumed by the next eager op (status/refine family); long ingest/`level_set`/`smooth` also run context-bound directly via `ctx.from_mesh`/`ctx.level_set`/`ctx.smooth`, so a cancellable long CSG/SDF run is the context method, not a watchdog thread.

[STACKING_LAW]:
- `Mesh`/`Mesh64` are the in-memory wire to the data mesh codec: the geometry owner hands `to_mesh()`/`to_mesh64()` arrays (`vert_properties`/`tri_verts`) to `data` `MeshPayload`/GLB encode and ingests them back via the `Mesh` constructor; this kernel never opens a file handle. A `trimesh.Trimesh` round-trips through the `Mesh(vert_properties, tri_verts)` constructor (vertices padded to the property layout) and `to_mesh().vert_properties[:, :3]` / `tri_verts`, so trimesh conditioning and manifold3d CSG compose without a serialized blob.
- the `level_set` SDF constructor is the bridge from a `compute`/`numerics` signed-distance field to a watertight solid: the field callable feeds `f`, and the resulting `Manifold` enters the same boolean/refine pipeline as primitive-built solids.
- convex-hull and Minkowski (`hull`/`batch_hull`/`hull_points`, `minkowski_sum`/`minkowski_difference`) own the morphology/offset-in-3D concern the spatial-operation union dispatches to; the 2D analogue is `CrossSection.offset` over Clipper2 — neither is hand-rolled where this kernel is admitted.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `manifold3d`
- Owns: guaranteed-manifold 3D boolean CSG, 2D polygon boolean/offset over Clipper2, lazy affine/warp transforms, SDF marching-tetrahedra level sets, convex hull and Minkowski morphology, edge refinement and tolerance simplification, vertex normal/curvature/property computation, and ray-segment casting
- Accept: `Manifold`/`CrossSection` values from primitives, `Mesh`/`Mesh64` round-trips, SDF level sets, or hull/Minkowski; in-memory triangle arrays from the data mesh codec
- Reject: hand-rolled boolean mesh ops, hand-rolled Clipper2 offsetting, a manual `reduce` over `+`/`-`/`^` where `batch_boolean` owns n-ary CSG, non-manifold soup trusted without a `status()` gate, a mesh-file/GLB encode that belongs to the data codec, and the deprecated `compose` where `batch_boolean(OpType.Add)` is the union owner

[CAPTURE_GAP]:
