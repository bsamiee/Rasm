# [PY_GEOMETRY_API_TRIMESH]

`trimesh` owns the geometry branch's triangle-mesh modeling, conditioning, and exchange rail: a `Trimesh` body with a content-hash-keyed lazy property algebra, a `Scene` transform graph, a `PointCloud`, polymorphic `load`/`export` IO, `creation` primitives from `shapely` profiles, and operation modules spanning CSG, registration, conditioning, remesh, proximity, sampling, and collision. Mesh owners compose these surfaces and never re-implement trimesh's own bindings — the IO codecs, the `manifold3d` CSG kernel, the `scipy` sparse-Laplacian solve, the `rtree` index, and `fcl` collision.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `trimesh`
- package: `trimesh`
- import: `import trimesh`
- owner: `geometry`
- rail: mesh
- capability: mesh/scene/path IO across `obj`/`ply`/`stl`/`off`/`glb`/`gltf`/`dae`/`3mf`/`xyz`/`dxf`/`svg`; primitive creation from extents and `shapely` polygons, convex hull and VHACD decomposition, minimum bounds, manifold boolean CSG, rigid and non-rigid registration, Laplacian/Taubin/Humphrey smoothing, quadric decimation and subdivision remesh, hole/normal/winding repair, signed-distance/closest-point/thickness proximity, surface/volume sampling, ray casting, FCL collision, and voxelization

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry roots (`trimesh`)

Every geometry root shares the `parent.Geometry` transform/bounds/identifier contract (`apply_transform`, `apply_obb`, `convex_hull`, `identifier_hash`, `copy`, `export`); dispatch on the runtime type, never a parallel reader family.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                               |
| :-----: | :----------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `Trimesh`    | triangle mesh | `vertices`/`faces` with cached topology, mass, boolean, repair, proximity  |
|  [02]   | `Scene`      | scene graph   | named geometries on a transform tree; `dump`/`to_mesh`/`export`/`subscene` |
|  [03]   | `PointCloud` | point cloud   | `vertices`/`colors`/`weights` with `kdtree`, `query`, `convex_hull`        |
|  [04]   | `Geometry`   | geometry root | abstract base unifying transform/bounds/identifier across the three        |

[PUBLIC_TYPE_SCOPE]: cached mesh property algebra (`Trimesh`)

`Trimesh` derives geometry as `caching`-backed lazy properties keyed on a `vertices`/`faces` content hash; `update_vertices`/`update_faces`/`process` invalidate the dependent cache. These are properties, never methods or parallel subclasses, and each row folds one property family into its `[CAPABILITY]` cell.

| [INDEX] | [PROPERTY]                                                                        | [CAPABILITY]                                         |
| :-----: | :-------------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `volume` / `area` / `area_faces` / `center_mass` / `mass` / `density`             | mass: volume, per-face area, centroid, `density`     |
|  [02]   | `mass_properties` / `moment_inertia` / `moment_inertia_frame`                     | inertia: `MassProperties`, tensor at origin/frame    |
|  [03]   | `principal_inertia_components` / `principal_inertia_vectors` / `_transform`       | inertia axes: moments, axes, diagonalizing transform |
|  [04]   | `is_watertight` / `is_winding_consistent` / `euler_number`                        | validity: closure, orientation, Euler number         |
|  [05]   | `is_convex` / `is_volume` / `is_empty` / `body_count`                             | solidity: convex, solid, empty, body count           |
|  [06]   | `convex_hull` / `bounding_box_oriented` / `bounding_sphere` / `_cylinder`         | convex hull; oriented/sphere/cylinder min bounds     |
|  [07]   | `face_normals` / `vertex_normals` / `face_angles` / `face_angles_sparse`          | per-face/per-vertex normals; interior face angles    |
|  [08]   | `edges` / `edges_unique` / `edges_sorted` / `edges_face` / `faces_unique_edges`   | directed/unique/sorted edges; owning face per edge   |
|  [09]   | `face_adjacency` / `face_adjacency_angles` / `face_adjacency_edges` / `_convex`   | dihedral face-pair graph: angles, edges, convex      |
|  [10]   | `facets` / `facets_boundary` / `facets_area` / `facets_normal` / `facets_on_hull` | coplanar facets: boundary, area, normal, hull flag   |
|  [11]   | `vertex_adjacency_graph` / `vertex_neighbors` / `vertex_faces` / `vertex_degree`  | per-vertex `networkx` adjacency, neighbors, degree   |
|  [12]   | `vertex_defects` / `integral_mean_curvature` / `symmetry` / `symmetry_axis`       | Gaussian angle defect, mean curvature, symmetry      |
|  [13]   | `kdtree` / `triangles_tree` / `edges_sorted_tree` / `face_adjacency_tree`         | `scipy` kdtree + `rtree` triangle/edge R-trees       |
|  [14]   | `identifier` / `identifier_hash`                                                  | rotation/scale-invariant identifier + hash for dedup |

[PUBLIC_TYPE_SCOPE]: spatial-query owners (`ProximityQuery`, `RayMeshIntersector`, `CollisionManager`, `VoxelGrid`)

Persistent query objects amortize index construction across many queries; batch against one fixed mesh through them rather than the one-shot module functions. Each owner's method roster is the keyed list below.

| [INDEX] | [SYMBOL]                              | [CONSTRUCTOR]                                                 |
| :-----: | :------------------------------------ | :------------------------------------------------------------ |
|  [01]   | `proximity.ProximityQuery`            | `ProximityQuery(mesh)`                                        |
|  [02]   | `ray.ray_triangle.RayMeshIntersector` | `RayMeshIntersector(mesh)` (or `mesh.ray`)                    |
|  [03]   | `ray.ray_pyembree.RayMeshIntersector` | auto-selected when `ray.has_embree`                           |
|  [04]   | `collision.CollisionManager`          | `CollisionManager()` then `add_object(name, mesh, transform)` |
|  [05]   | `voxel.VoxelGrid`                     | `mesh.voxelized(pitch, method='subdivide')`                   |

- [01]-[PROXIMITY_QUERY]: `signed_distance(points)`, `on_surface(points) -> (pts, dist, tid)`, `vertex(points)`.
- [02]-[RAY]: `intersects_location`, `intersects_id`, `intersects_first`, `intersects_any`, `contains_points`.
- [03]-[RAY_EMBREE]: Embree-accelerated mirror of the same intersector contract.
- [04]-[COLLISION]: `in_collision_internal/single/other`, `min_distance_internal/single/other`, `set_transform`.
- [05]-[VOXEL]: `matrix`, `points`, `as_boxes()`, `fill`/`hollow`, `marching_cubes`, `revoxelized`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: polymorphic IO (`load`, `export`, `available_formats`)

`load` resolves `file_type` from the argument or path extension over a `file_obj` path/bytes/stream, dispatches across every registered codec, and returns the runtime `Geometry` (`Trimesh`, `Scene`, or `Path`); `force=` collapses ambiguous results to one kind. `export` is the symmetric writer over the same format set, returning `bytes`/`str`/`dict` when `file_obj=None`. Format is an argument, never a `load_<fmt>`/`export_<fmt>` family. Carry: `load`/`load_scene` share `(file_obj, file_type=None, resolver=None, allow_remote=False)`; `load` adds `force=None`, `load_scene` adds `metadata=None`.

| [INDEX] | [SURFACE]                                     | [CAPABILITY]                                            |
| :-----: | :-------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `load(..., force=None)`                       | polymorphic mesh/scene/path read returning `Geometry`   |
|  [02]   | `load_mesh(*args, **kwargs)`                  | force a single `Trimesh` result (concatenates a scene)  |
|  [03]   | `load_scene(..., metadata=None)`              | force a `Scene` result preserving the transform graph   |
|  [04]   | `load_path(file_obj, file_type=None)`         | force a `Path2D`/`Path3D` (dxf/svg) result              |
|  [05]   | `load_remote(url, **kwargs)`                  | fetch + load a remote asset (requires `allow_remote`)   |
|  [06]   | `available_formats()`                         | enumerate every registered load/export extension        |
|  [07]   | `mesh.export(file_obj=None, file_type=None)`  | write a mesh to any registered format; bytes if no sink |
|  [08]   | `scene.export(file_obj=None, file_type=None)` | write a scene with transforms (glb/gltf/3mf/dae)        |

[ENTRYPOINT_SCOPE]: primitive creation (`creation`)

`creation.*` are static constructors returning a `Trimesh`, each taking an optional 4x4 `transform` and forwarding `**kwargs` to the constructor. `extrude_polygon`/`revolve`/`sweep_polygon`/`triangulate_polygon` consume a `shapely` `Polygon`/`LineString`, bridging the planar `shapely` rail into solids.

| [INDEX] | [SURFACE]                                                                 | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------------------ | :-------------------------------------------- |
|  [01]   | `box(extents=None, transform=None, bounds=None)`                          | axis-aligned box                              |
|  [02]   | `cylinder(radius, height=None, sections=None, segment=None)`              | cylinder (or segment-defined)                 |
|  [03]   | `cone(radius, height, sections=None)` / `capsule(height, radius)`         | cone / capsule                                |
|  [04]   | `annulus(r_min, r_max, height=None, sections=None, segment=None)`         | annular tube                                  |
|  [05]   | `torus(major_radius, minor_radius, major_sections=32, minor_sections=32)` | torus                                         |
|  [06]   | `icosphere(subdivisions=3, radius=1.0)` / `uv_sphere(radius, count=None)` | geodesic / UV sphere                          |
|  [07]   | `icosahedron()`                                                           | unit icosahedron seed for subdivision         |
|  [08]   | `extrude_polygon(polygon, height, transform=None, mid_plane=False)`       | extrude a planar `shapely` polygon to a solid |
|  [09]   | `extrude_triangulation(vertices, faces, height)`                          | extrude a precomputed triangulation           |
|  [10]   | `revolve(linestring, angle=None, cap=False, sections=None)`               | revolve a 2D profile (full/partial)           |
|  [11]   | `sweep_polygon(polygon, path, angles=None, cap=True, connect=True)`       | sweep a profile along a polyline path         |
|  [12]   | `triangulate_polygon(polygon, triangle_args=None, engine=None) -> (V,F)`  | triangulate via `triangle`/`earcut` engine    |
|  [13]   | `axis(...)` / `camera_marker(camera, ...)`                                | debug axis tripod / camera frustum marker     |

[ENTRYPOINT_SCOPE]: boolean CSG, decomposition, repair, smoothing, remesh

Boolean rows route to `manifold3d` (`engine='manifold'` default, `'blender'` fallback) and return a new `Trimesh`; `check_volume=True` rejects non-positive-volume operands, and `reduce_cascade` folds n-ary input pairwise to balance the manifold tree. Smoothing rows mutate `mesh.vertices` in place and return the same `Trimesh`; `repair.fill_holes`/`fix_*` return `bool` and mutate in place; `remesh.*`/`subdivide_to_size` return new vertex/face arrays or a new `Trimesh`. `filter_laplacian` carries `implicit_time_integration=False, volume_constraint=True`.

| [INDEX] | [SURFACE]                                                                          | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `boolean.union(meshes, engine=None, check_volume=True)`                            | n-ary watertight union via manifold               |
|  [02]   | `boolean.difference(meshes, engine=None, check_volume=True)`                       | n-ary difference (first minus rest)               |
|  [03]   | `boolean.intersection(meshes, engine=None, check_volume=True)`                     | n-ary intersection                                |
|  [04]   | `boolean.boolean_manifold(meshes, operation, check_volume=True)`                   | low-level manifold op                             |
|  [05]   | `boolean.reduce_cascade(op, items)`                                                | pairwise cascade fold for n-ary input             |
|  [06]   | `mesh.union/difference/intersection(other, engine=None)`                           | method mirror of the module booleans              |
|  [07]   | `decomposition.convex_decomposition(mesh, **kwargs) -> list[dict]`                 | approximate convex decomposition (manifold VHACD) |
|  [08]   | `repair.fill_holes(mesh, use_fan=False) -> bool`                                   | fill boundary holes in place                      |
|  [09]   | `repair.fix_normals(mesh, multibody=False)`                                        | consistent outward normals                        |
|  [10]   | `repair.fix_winding(mesh)`                                                         | consistent winding                                |
|  [11]   | `repair.fix_inversion(mesh)`                                                       | fix inverted faces                                |
|  [12]   | `repair.stitch(mesh, faces=None, insert_vertices=False)`                           | stitch open boundaries                            |
|  [13]   | `repair.broken_faces(mesh)`                                                        | enumerate broken faces                            |
|  [14]   | `smoothing.filter_taubin(mesh, lamb=0.5, nu=0.5, iterations=10)`                   | Taubin shrink-free smoothing                      |
|  [15]   | `smoothing.filter_laplacian(mesh, lamb=0.5, iterations=10, ...)`                   | Laplacian smoothing (explicit/implicit)           |
|  [16]   | `smoothing.filter_humphrey(mesh, alpha=0.1, beta=0.5, iterations=10)`              | Humphrey Laplacian                                |
|  [17]   | `smoothing.filter_mut_dif_laplacian(...)`                                          | mutable-diffusion Laplacian                       |
|  [18]   | `smoothing.laplacian_calculation(mesh, equal_weight=True, pinned_vertices=None)`   | reusable `scipy.sparse` Laplacian operator        |
|  [19]   | `mesh.simplify_quadric_decimation(percent=None, face_count=None, aggression=None)` | quadric decimation (manifold backend)             |
|  [20]   | `remesh.subdivide_to_size(vertices, faces, max_edge, max_iter=10, ...)`            | edge-length subdivision                           |
|  [21]   | `remesh.subdivide(vertices, faces, ...)`                                           | midpoint subdivision                              |
|  [22]   | `remesh.subdivide_loop(vertices, faces, iterations=None)`                          | Loop subdivision                                  |

[ENTRYPOINT_SCOPE]: registration, proximity, sampling, section, split

Registration rows return a transform with diagnostics; `mesh_other` returns `(matrix, cost)`, `icp`/`procrustes` return `(matrix, transformed, cost)`. Proximity rows return distances/points; `closest_point` and `ProximityQuery.on_surface` return the `(points, distances, triangle_id)` 3-tuple. `sample_surface`/`_even` return `(points, face_index)`. `section`/`section_multiplane` return a `Path3D`/`Path2D`; `slice_plane` returns a clipped `Trimesh`. Full registration signatures are the keyed list below the table.

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `registration.mesh_other(mesh, other, ...)`                                | align mesh to mesh by sampled ICP; sig in [01]  |
|  [02]   | `registration.icp(a, b, ...)`                                              | iterative closest point; sig in [02]            |
|  [03]   | `registration.procrustes(a, b, ...)`                                       | weighted rigid/similarity fit; sig in [03]      |
|  [04]   | `registration.nricp_amberg(source_mesh, target_geometry, ...)`             | non-rigid Amberg deformation; sig in [04]       |
|  [05]   | `registration.nricp_sumner(source_mesh, target_geometry, ...)`             | non-rigid Sumner deformation; sig in [05]       |
|  [06]   | `proximity.closest_point(mesh, points)`                                    | closest surface points, distances, triangle ids |
|  [07]   | `proximity.signed_distance(mesh, points)`                                  | signed distance (negative outside)              |
|  [08]   | `proximity.thickness(mesh, points, exterior=False, method='max_sphere')`   | local wall thickness via inscribed sphere       |
|  [09]   | `proximity.max_tangent_sphere(...)`                                        | max inscribed tangent sphere per point          |
|  [10]   | `sample.sample_surface(mesh, count, face_weight=None, ...)`                | area-weighted surface samples                   |
|  [11]   | `sample.sample_surface_even(mesh, count, radius=None, seed=None)`          | blue-noise / Poisson-disk even samples          |
|  [12]   | `sample.volume_mesh(mesh, count)`                                          | interior volume samples                         |
|  [13]   | `mesh.sample(count)`                                                       | surface-sample convenience                      |
|  [14]   | `mesh.contains(points) -> bool[]`                                          | inside/outside test                             |
|  [15]   | `mesh.section(plane_normal, plane_origin)`                                 | planar cross-section path                       |
|  [16]   | `mesh.section_multiplane(plane_origin, plane_normal, heights)`             | multi-plane cross-section paths                 |
|  [17]   | `mesh.slice_plane(plane_origin, plane_normal, cap=False, face_index=None)` | half-space clip (optionally capped)             |
|  [18]   | `intersections.slice_mesh_plane(...)`                                      | module-form half-space clip                     |
|  [19]   | `mesh.split(only_watertight=True, ...) -> list[Trimesh]`                   | disconnected-component split                    |
|  [20]   | `mesh.submesh(faces_sequence)`                                             | face-group submeshes                            |

- [01]-[MESH_OTHER]: `registration.mesh_other(mesh, other, samples=500, scale=False, icp_first=10, icp_final=50) -> (matrix, cost)`.
- [02]-[ICP]: `registration.icp(a, b, initial=None, threshold=1e-5, max_iterations=20) -> (matrix, transformed, cost)`.
- [03]-[PROCRUSTES]: `registration.procrustes(a, b, weights=None, reflection=True, translation=True, scale=True, return_cost=True) -> (matrix, transformed, cost)`.
- [04]-[NRICP_AMBERG]: `registration.nricp_amberg(source_mesh, target_geometry, source_landmarks=None, target_positions=None, steps=None, eps=1e-4, gamma=1, distance_threshold=0.1, ...)`.
- [05]-[NRICP_SUMNER]: `registration.nricp_sumner(source_mesh, target_geometry, ..., face_pairs_type='vertex')`.

[ENTRYPOINT_SCOPE]: bounds, curvature, collision, ray, voxel, poses

Analysis and spatial surfaces for measurement, interference, and visibility; the input shape reads off each signature.

| [INDEX] | [SURFACE]                                                                            | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `bounds.oriented_bounds(obj, ...) -> (transform, extents)`                           | minimum-volume OBB                          |
|  [02]   | `bounds.minimum_cylinder(obj)`                                                       | minimum bounding cylinder                   |
|  [03]   | `nsphere.minimum_nsphere(obj) -> (center, radius)`                                   | minimum bounding sphere                     |
|  [04]   | `nsphere.fit_nsphere(points)`                                                        | least-squares sphere fit                    |
|  [05]   | `curvature.discrete_gaussian_curvature_measure(mesh, points, radius)`                | ball-integrated Gaussian curvature          |
|  [06]   | `curvature.discrete_mean_curvature_measure(...)`                                     | ball-integrated mean curvature              |
|  [07]   | `mesh.ray.intersects_location(ray_origins, ray_directions)`                          | ray-surface hit points (Embree-accelerated) |
|  [08]   | `mesh.ray.intersects_first(...)`                                                     | first-hit triangle per ray                  |
|  [09]   | `mesh.ray.intersects_id(...)`                                                        | all hit triangle ids                        |
|  [10]   | `mesh.ray.intersects_any(...)`                                                       | any-hit boolean                             |
|  [11]   | `collision.CollisionManager().add_object(name, mesh, transform)`                     | register a named mesh for collision         |
|  [12]   | `collision.CollisionManager().in_collision_internal()`                               | FCL pairwise collision detection            |
|  [13]   | `CollisionManager.min_distance_internal()`                                           | FCL minimum internal separation             |
|  [14]   | `CollisionManager.min_distance_single(mesh)`                                         | FCL distance to one mesh                    |
|  [15]   | `CollisionManager.min_distance_other(mgr)`                                           | FCL distance to another manager             |
|  [16]   | `mesh.voxelized(pitch, method='subdivide')`                                          | dense/sparse voxelization; fill, boxes      |
|  [17]   | `mesh.compute_stable_poses(center_mass=None, sigma=0.0, n_samples=1, threshold=0.0)` | resting-pose probabilities for placement    |
|  [18]   | `mesh.apply_obb()`                                                                   | reorient to OBB frame                       |
|  [19]   | `mesh.apply_transform(matrix)`                                                       | rigid/affine transform                      |
|  [20]   | `mesh.apply_scale(factor)`                                                           | scale in place                              |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `import trimesh` at boundary scope only; a module-level import violates the manifest import policy.
- One `Trimesh` owns `vertices`/`faces` with a `caching` cache of derived geometry keyed on a content hash; mass, topology graphs, normals, curvature, symmetry, and `scipy`/`rtree` spatial indices are lazily cached properties, never parallel subclasses, and `update_vertices`/`update_faces`/`process` invalidate them.
- `load` dispatches on the resolved `file_type` across `available_formats()` and `export(file_type=...)` writes the same set; the format is an argument, never a `load_<fmt>` family, and geometry returns in-memory triangulations while file encode belongs to the data codec.
- `boolean.*` and `convex_decomposition` route to `manifold3d`, require watertight operands, and fold n-ary input through `reduce_cascade`; convex hull is `scipy.spatial.ConvexHull` and minimum bounds are `bounds`/`nsphere`.
- `repair.*` return `bool` success and mutate in place; `smoothing.filter_*` mutate `vertices` over a shared `scipy.sparse` Laplacian and return the same `Trimesh`; `process=True` on construction runs the merge-and-validate pass.
- `ProximityQuery`, `RayMeshIntersector` (Embree when `ray.has_embree`), `CollisionManager` (FCL), and `VoxelGrid` are persistent query owners amortizing index construction over one fixed mesh.
- Each op captures a mesh receipt: `load` carries format and vertex/face count with `is_watertight`/`is_winding_consistent`; boolean/decomposition/registration carry the operation, input counts, result validity, and the transform and cost; `mass_properties` carries `MassProperties`.

[STACKING]:
- data mesh codec (`rasm.data.spatial.mesh`): `mesh.export(file_type='glb') -> bytes` is the only encode path, owned by `MeshPayload`; geometry returns the conditioned triangulation and the data codec owns GLB/3MF/PLY serialization, so the kernel never opens a file handle.
- `manifold3d`(`.api/manifold3d.md`): `boolean.union`/`difference`/`intersection` are the facade over the `Manifold` CSG kernel; an owner needing batched booleans or `+`/`-`/`^` drops to `manifold3d` directly and re-wraps via `Trimesh(vertices=..., faces=...)`, gating on `is_watertight` before the call.
- `open3d`(`.api/open3d.md`)/`small_gicp`(`.api/small-gicp.md`)/`kiss-matcher`(`.api/kiss-matcher.md`): trimesh owns mesh-mesh rigid (`mesh_other`/`icp`/`procrustes`) and non-rigid (`nricp_amberg`/`nricp_sumner`) alignment; point-cloud global registration and fine GICP route to those engines, and every backend's 4x4 transform feeds the same `apply_transform`.
- `shapely` planar -> `creation` solid: `creation.extrude_polygon`/`revolve`/`sweep_polygon`/`triangulate_polygon` consume a `shapely` `Polygon`/`LineString`, and `Trimesh.section(...)` closes the loop to a `Path3D` whose `.polygons_full` are `shapely` polygons, so section -> planar-op -> re-extrude is one rail.
- `scipy.sparse`: `smoothing.laplacian_calculation(pinned_vertices=...)` returns a reusable cotangent/uniform Laplacian; the implicit `filter_laplacian` path solves through `scipy.sparse.linalg.spsolve`, and the operator reuses across `filter_taubin`/`filter_laplacian`/`filter_humphrey` via `laplacian_operator=`.
- within-lib deviation rail: `ProximityQuery(reference).signed_distance(sample.sample_surface(target, n)[0])` folds a watertight-gated signed-distance distribution into deviation receipt facts, amortizing the `rtree` triangle index across the sample batch.
- within-lib identity: `Trimesh.identifier_hash`/`Scene.identifier_hash` is a rotation/translation/scale-invariant content hash seeding `ContentIdentity` for memoized boolean/decomposition/registration results.

[LOCAL_ADMISSION]:
- `trimesh` is the admitted triangle-mesh modeling, conditioning, and exchange backend for the geometry branch; the mesh and geometry owners compose its IO, operation modules, and query owners rather than a parallel codec, boolean, or spatial-index surface.

[RAIL_LAW]:
- Package: `trimesh`
- Owns: triangle-mesh/scene/path IO, primitive creation from `shapely` profiles, convex hull and VHACD decomposition, minimum bounds, manifold boolean CSG, ICP/Procrustes/non-rigid registration, Laplacian/Taubin/Humphrey smoothing, quadric decimation and subdivision remesh, hole/normal/winding repair, signed-distance/closest-point/thickness proximity, surface/volume sampling, ray casting, FCL collision, and voxelization
- Accept: triangle-mesh modeling, conditioning, and exchange feeding the geometry and mesh owners; the shared 4x4 registration transform
- Reject: wrapper-renames of `load`/`export`; a hand-rolled mesh IO codec, boolean kernel, sparse-Laplacian solve, convex hull, R-tree, or FCL binding trimesh already binds; a `load_<format>`/`export_<format>` or `Add<Op>` family over the format/operation argument; a mesh-file encode bypassing the data `MeshPayload` codec; `.3dm`/OpenNURBS (routes to `rhino3dm`), STEP/AP242 BREP (`cadquery-ocp`), or point-cloud scan reconstruction (`open3d`) surfaces trimesh does not own
