# [PY_GEOMETRY_API_MANIFOLD3D]

`manifold3d` supplies a GPU-accelerated watertight mesh kernel through `Manifold` (3D solid CSG), `CrossSection` (2D polygon CSG), `Mesh`/`Mesh64` (raw triangle soup carrier), `ExecutionContext` (async cancel/progress), and `RayHit` (ray-cast result) for geometry processing, boolean operations, extrusion, SDF level-set construction, and convex-hull rails.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `manifold3d`
- package: `manifold3d`
- module: `manifold3d`
- asset: compiled nanobind extension, cp313 wheel available
- rail: geometry-csg

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: solid geometry family
- rail: geometry-csg

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]       | [CAPABILITY]                                        |
| :-----: | :----------------- | :------------------ | :-------------------------------------------------- |
|   [1]   | `Manifold`         | watertight solid    | CSG, extrude, revolve, refine, topology queries     |
|   [2]   | `CrossSection`     | 2D polygon          | offset, extrude, revolve, hull, boolean on polygons |
|   [3]   | `Mesh`             | triangle soup       | `tri_verts`, `vert_properties`, run/merge index     |
|   [4]   | `Mesh64`           | large triangle soup | identical surface to `Mesh` with 64-bit indices     |
|   [5]   | `ExecutionContext` | async context       | cancel/progress tracking for deferred manifold ops  |
|   [6]   | `RayHit`           | ray-cast result     | `distance`, `face_id`, `normal`, `position`         |

[PUBLIC_TYPE_SCOPE]: enumeration family
- rail: geometry-csg

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]  | [CAPABILITY]                                     |
| :-----: | :--------- | :------------- | :----------------------------------------------- |
|   [1]   | `OpType`   | boolean opcode | `Add`, `Subtract`, `Intersect`                   |
|   [2]   | `FillRule` | polygon fill   | `EvenOdd`, `NonZero`, `Positive`, `Negative`     |
|   [3]   | `JoinType` | offset join    | `Square`, `Round`, `Miter`, `Bevel`              |
|   [4]   | `Error`    | status codes   | `NoError`, `NotManifold`, `NonFiniteVertex`, ... |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Manifold constructors
- rail: geometry-csg

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :---------------------------- |
|   [1]   | `Manifold.cube(size=(1,1,1), center=False) -> Manifold`                                    | primitive      | axis-aligned box              |
|   [2]   | `Manifold.sphere(radius, circular_segments=0) -> Manifold`                                 | primitive      | geodesic sphere               |
|   [3]   | `Manifold.cylinder(height, radius_low, radius_high=-1, circular_segments=0, center=False)` | primitive      | cylinder or cone              |
|   [4]   | `Manifold.tetrahedron() -> Manifold`                                                       | primitive      | unit tetrahedron              |
|   [5]   | `Manifold.extrude(crossSection, height, n_divisions=0, twist_degrees=0, scale_top=(1,1))`  | constructor    | extrude CrossSection along Z  |
|   [6]   | `Manifold.revolve(crossSection, circular_segments=0, revolve_degrees=360)`                 | constructor    | revolve CrossSection around Y |
|   [7]   | `Manifold.smooth(mesh, sharpened_edges=[], edge_smoothness=[]) -> Manifold`                | constructor    | smooth manifold from Mesh     |
|   [8]   | `Manifold.level_set(f, bounds, edgeLength, level=0, tolerance=-1) -> Manifold`             | constructor    | SDF marching-tetrahedra       |
|   [9]   | `Manifold.batch_boolean(manifolds, op) -> Manifold`                                        | set operation  | n-ary CSG from sequence       |
|  [10]   | `Manifold.compose(manifolds) -> Manifold`                                                  | set operation  | topological union (no CSG)    |
|  [11]   | `Manifold.hull_points(pts) -> Manifold`                                                    | constructor    | convex hull of point cloud    |

[ENTRYPOINT_SCOPE]: Manifold instance operations
- rail: geometry-csg

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------------------------------------------------- | :------------- | :------------------------------ |
|   [1]   | `m.hull() -> Manifold`                                                | geometry       | convex hull of solid            |
|   [2]   | `m.decompose() -> list[Manifold]`                                     | topology       | split disconnected components   |
|   [3]   | `m.split(cutter) -> tuple[Manifold, Manifold]`                        | geometry       | cut by another manifold         |
|   [4]   | `m.split_by_plane(normal, offset) -> tuple[Manifold, Manifold]`       | geometry       | cut by half-space               |
|   [5]   | `m.trim_by_plane(normal, offset) -> Manifold`                         | geometry       | discard one half-space          |
|   [6]   | `m.project() -> CrossSection`                                         | projection     | XY outline of solid             |
|   [7]   | `m.refine(n) -> Manifold`                                             | mesh           | subdivide each edge n times     |
|   [8]   | `m.refine_to_length(length) -> Manifold`                              | mesh           | subdivide to max edge length    |
|   [9]   | `m.refine_to_tolerance(tolerance) -> Manifold`                        | mesh           | smooth subdivision to tolerance |
|  [10]   | `m.simplify(tolerance) -> Manifold`                                   | mesh           | coarsen within tolerance        |
|  [11]   | `m.smooth_out(min_sharp_angle, min_super_normal_angle) -> Manifold`   | mesh           | smooth sharp edges out          |
|  [12]   | `m.smooth_by_normals(normal_idx) -> Manifold`                         | mesh           | smooth using stored normals     |
|  [13]   | `m.calculate_normals(normal_idx=0, min_sharp_angle=52.5) -> Manifold` | properties     | fill normal vertex properties   |
|  [14]   | `m.calculate_curvature(gaussian_idx, mean_idx) -> Manifold`           | properties     | fill curvature properties       |
|  [15]   | `m.set_properties(new_num_prop, f) -> Manifold`                       | properties     | recompute per-vertex properties |
|  [16]   | `m.mirror(normal) -> Manifold`                                        | transform      | reflect across plane            |
|  [17]   | `m.rotate(v) -> Manifold`                                             | transform      | Euler-angle rotation            |
|  [18]   | `m.scale(v) -> Manifold`                                              | transform      | per-axis scale                  |

[ENTRYPOINT_SCOPE]: Manifold query and misc
- rail: geometry-csg

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :------------------------------------------ | :------------- | :--------------------------- |
|   [1]   | `m.to_mesh() -> Mesh`                       | export         | extract triangle soup        |
|   [2]   | `m.to_mesh64() -> Mesh64`                   | export         | extract 64-bit index soup    |
|   [3]   | `m.status() -> Error`                       | validation     | construction error code      |
|   [4]   | `m.is_empty() -> bool`                      | query          | no triangles present         |
|   [5]   | `m.genus() -> int`                          | topology       | topological genus            |
|   [6]   | `m.volume() -> float`                       | measurement    | enclosed volume              |
|   [7]   | `m.surface_area() -> float`                 | measurement    | total surface area           |
|   [8]   | `m.bounding_box() -> tuple`                 | query          | axis-aligned bounds          |
|   [9]   | `m.num_vert() -> int`                       | query          | vertex count                 |
|  [10]   | `m.num_tri() -> int`                        | query          | triangle count               |
|  [11]   | `m.num_edge() -> int`                       | query          | edge count                   |
|  [12]   | `m.ray_cast(origin, direction) -> RayHit`   | query          | ray intersection             |
|  [13]   | `m.min_gap(other, search_length) -> float`  | query          | minimum gap to another solid |
|  [14]   | `m.minkowski_sum(other) -> Manifold`        | morphology     | morphological dilation       |
|  [15]   | `m.minkowski_difference(other) -> Manifold` | morphology     | morphological erosion        |
|  [16]   | `m.with_context(ctx) -> Manifold`           | async          | attach ExecutionContext      |

[ENTRYPOINT_SCOPE]: CrossSection operations
- rail: geometry-csg

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :-------------------------------------------------- | :------------- | :------------------------------- |
|   [1]   | `CrossSection.square(dims, center=False)`           | primitive      | rectangle                        |
|   [2]   | `CrossSection.circle(radius, circular_segments=0)`  | primitive      | circle polygon                   |
|   [3]   | `CrossSection.compose(sections)`                    | set operation  | topological union                |
|   [4]   | `CrossSection.batch_boolean(sections, op)`          | set operation  | n-ary boolean                    |
|   [5]   | `CrossSection.hull_points(pts)`                     | constructor    | convex hull of 2D points         |
|   [6]   | `cs.offset(delta, join_type, miter_limit, arc_tol)` | offset         | grow or shrink polygon           |
|   [7]   | `cs.hull() -> CrossSection`                         | geometry       | convex hull                      |
|   [8]   | `cs.simplify(epsilon) -> CrossSection`              | geometry       | simplify polygon                 |
|   [9]   | `cs.to_polygons() -> list`                          | export         | list of vertex coordinate arrays |
|  [10]   | `cs.area() -> float`                                | measurement    | signed area                      |
|  [11]   | `cs.num_contour() -> int`                           | query          | contour count                    |
|  [12]   | `cs.num_vert() -> int`                              | query          | vertex count                     |

[ENTRYPOINT_SCOPE]: module-level functions
- rail: geometry-csg

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------- | :------------- | :---------------------------------- |
|   [1]   | `triangulate(polygons) -> list`        | utility        | polygon triangulation               |
|   [2]   | `get_circular_segments() -> int`       | config         | global circular-segment default     |
|   [3]   | `set_circular_segments(n)`             | config         | set global circular-segment default |
|   [4]   | `set_min_circular_angle(degrees)`      | config         | set minimum angle per segment       |
|   [5]   | `set_min_circular_edge_length(length)` | config         | set minimum edge length             |

## [4]-[IMPLEMENTATION_LAW]

[CSG_TOPOLOGY]:
- All solids must be manifold (watertight, orientable) before boolean ops; `m.status()` returns `Error.NoError` for valid input.
- `OpType.Add` is union, `OpType.Subtract` is difference, `OpType.Intersect` is intersection; `batch_boolean` applies these to a sequence.
- `Mesh`/`Mesh64` are triangle-soup carriers with `tri_verts` (Nx3 index array), `vert_properties` (Mx P float array where columns 0-2 are XYZ).
- `RayHit.face_id` is `-1` when no intersection; check before using `distance`/`normal`/`position`.
- `ExecutionContext` attaches to a manifold via `with_context`; the context is consumed on the next eager op.
- Global circular-segment policy applies to all `sphere`/`cylinder`/`circle` calls after `set_circular_segments`.

[LOCAL_ADMISSION]:
- Solids enter as `Manifold` instances from primitives, `Mesh` round-trips, or SDF level-sets; raw polygon soup enters via `Mesh` constructor.
- Boolean CSG pipelines operate on `Manifold` values immutably; each operation returns a new `Manifold`.
- Level-set construction requires a Python callable `f(x, y, z) -> float` and explicit bounds/edge-length parameters.

[RAIL_LAW]:
- Package: `manifold3d`
- Owns: watertight mesh CSG, 2D polygon offset and extrusion, SDF level-set meshes, convex hull, ray-cast
- Accept: `Manifold` and `CrossSection` values from primitives or `Mesh` round-trips
- Reject: hand-rolled boolean mesh operations, non-manifold triangle soup as CSG input
