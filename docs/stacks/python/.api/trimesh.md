# [PY_GEOMETRY_API_TRIMESH]

`trimesh` is the triangle-mesh modeling, conditioning, and exchange surface for the geometry rail: a `Trimesh` body with a lazily-cached topology/mass/spatial-index algebra, a `Scene` transform graph of named geometries, a `PointCloud`, the polymorphic `load` reader dispatching on resolved `file_type` across every `available_formats()` entry, primitive `creation` constructors over `shapely` polygons, `manifold3d`-backed boolean CSG and VHACD convex decomposition, ICP/Procrustes/non-rigid `registration`, Laplacian/Taubin/Humphrey `smoothing` over a `scipy.sparse` Laplacian, quadric decimation and subdivision `remesh`, hole/normal/winding `repair`, signed-distance/closest-point `proximity`, area-weighted `sample`, ray casting, FCL collision, and voxelization. The owner composes `load`, `Trimesh.export`, and the operation modules into the mesh owner; it never re-implements the IO codecs, the `manifold3d` CSG kernel, the `scipy` sparse-Laplacian smoothing solve, the `rtree` triangle index, the convex-hull (`scipy.spatial.ConvexHull`), or the `fcl` collision engine that trimesh already binds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `trimesh`
- package: `trimesh`
- import: `import trimesh`
- owner: `geometry`
- rail: mesh
- version: `4.12.2`
- license: MIT (`Requires-Python: >=3.8`)
- wheel-floor: `trimesh` itself is unmarked pure-Python (`py3-none-any`) and resolves on the `>=3.15` project venv; its compiled optional backends are the gate. The manifest marks `manifold3d; python_version<'3.15'` (boolean CSG / VHACD decomposition, C++ pybind), and `rtree` (the triangle R-tree index over libspatialindex) likewise lacks a cp315 wheel. The full boolean/decomposition/spatial-index surface therefore enumerates only on a cp313/cp314 companion where those backends and `scipy`/`shapely`/`networkx` resolve; the bare `>=3.15` venv exposes the pure-Python mesh/IO/topology/sampling surface but raises on the boolean and R-tree paths.
- ABI: `manifold3d`, `rtree` (libspatialindex), `python-fcl` (collision), optional `embreex` (Embree ray accel) are native; `scipy`/`shapely`/`networkx`/`pillow` are pure-consumer dependencies trimesh dispatches into. No native code in `trimesh` itself.
- entry points: none (library only)
- capability: mesh/scene/path IO across `obj`/`ply`/`stl`/`off`/`glb`/`gltf`/`dae`/`3mf`/`xyz`/`dxf`/`svg`; primitive creation from extents and `shapely` polygons; convex hull, minimum bounding sphere/cylinder/box, and VHACD approximate convex decomposition; manifold boolean CSG; rigid ICP/Procrustes and non-rigid Amberg/Sumner registration; Laplacian/Taubin/Humphrey/mutable-diffusion smoothing; quadric decimation and Loop/midpoint subdivision remesh; hole fill, normal/winding/inversion fix, and vertex stitch repair; signed-distance, closest-point, thickness, and max-tangent-sphere proximity; area-weighted and blue-noise surface sampling plus volume sampling; ray casting; FCL collision and minimum-distance; and voxelization with morphology

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry roots (`trimesh`)
- rail: mesh

All four share the `parent.Geometry` transform/bounds/identifier contract: `apply_transform`/`apply_obb`/`apply_scale`/`apply_translation`, `bounds`/`extents`/`centroid`, `bounding_box`/`bounding_box_oriented`/`bounding_sphere`/`bounding_cylinder`/`bounding_primitive`, `convex_hull`, `identifier_hash`, `copy`, `export`. Dispatch on the runtime type, never a parallel reader family.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :----------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `Trimesh`    | triangle mesh | `vertices`/`faces` plus cached topology, mass, boolean, repair, proximity |
|  [02]   | `Scene`      | scene graph   | named geometries on a transform tree; `dump`/`to_mesh`/`export`/`subscene` |
|  [03]   | `PointCloud` | point cloud   | `vertices`/`colors`/`weights` with `kdtree`, `query`, `convex_hull`      |
|  [04]   | `Geometry`   | geometry root | abstract base unifying transform/bounds/identifier across the three      |

[PUBLIC_TYPE_SCOPE]: cached mesh property algebra (`Trimesh`)
- rail: mesh

`Trimesh` exposes derived geometry as `caching`-backed lazy properties keyed off a `vertices`/`faces` content hash; any `vertices`/`faces` mutation through `update_vertices`/`update_faces`/`process` invalidates the dependent cache. These are properties, never methods and never parallel subclasses.

| [INDEX] | [PROPERTY]                                                              | [PROPERTY_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :--------------------------------------------------------------------- | :---------------- | :------------------------------------------------------------------- |
|  [01]   | `volume` / `area` / `area_faces` / `center_mass` / `mass` / `density`  | mass property     | enclosed volume, surface/per-face area, centroid, mass at `density`  |
|  [02]   | `mass_properties` / `moment_inertia` / `moment_inertia_frame`          | inertia tensor    | `MassProperties` record; inertia tensor at origin or arbitrary frame |
|  [03]   | `principal_inertia_components` / `principal_inertia_vectors` / `_transform` | inertia axes  | principal moments, axes, and the diagonalizing transform             |
|  [04]   | `is_watertight` / `is_winding_consistent` / `euler_number`             | manifold validity | manifold closure, consistent orientation, Euler characteristic       |
|  [05]   | `is_convex` / `is_volume` / `is_empty` / `body_count`                  | solidity validity | convexity, solid (closed+oriented), emptiness, connected-body count  |
|  [06]   | `convex_hull` / `bounding_box_oriented` / `bounding_sphere` / `_cylinder` | derived solid  | convex hull and minimum oriented/sphere/cylinder bounds              |
|  [07]   | `face_normals` / `vertex_normals` / `face_angles` / `face_angles_sparse` | normals & angles | per-face/per-vertex normals; interior face angles                    |
|  [08]   | `edges` / `edges_unique` / `edges_sorted` / `edges_face` / `faces_unique_edges` | edge topology | directed/unique/sorted edges and the face each edge belongs to       |
|  [09]   | `face_adjacency` / `face_adjacency_angles` / `face_adjacency_edges` / `_convex` | adjacency  | dihedral face-pair graph with angles, shared edges, convex flags     |
|  [10]   | `facets` / `facets_boundary` / `facets_area` / `facets_normal` / `facets_on_hull` | coplanar facets | coplanar face groups, their boundary edges, area, normal, hull flag |
|  [11]   | `vertex_adjacency_graph` / `vertex_neighbors` / `vertex_faces` / `vertex_degree` | vertex graph | per-vertex `networkx` adjacency, neighbor lists, incident faces      |
|  [12]   | `vertex_defects` / `integral_mean_curvature` / `symmetry` / `symmetry_axis` | curvature/symmetry | angle defect (Gaussian), integral mean curvature, symmetry class |
|  [13]   | `kdtree` / `triangles_tree` / `edges_sorted_tree` / `face_adjacency_tree` | spatial index   | `scipy` vertex kdtree and `rtree` R-trees over triangles/edges       |
|  [14]   | `identifier` / `identifier_hash`                                       | content identity  | rotation/scale-invariant identifier and its hash for dedup           |

[PUBLIC_TYPE_SCOPE]: spatial-query owners (`ProximityQuery`, `RayMeshIntersector`, `CollisionManager`, `VoxelGrid`)
- rail: mesh

Persistent query objects amortize index construction across many queries; prefer them over the one-shot module functions when batching against one fixed mesh.

| [INDEX] | [SYMBOL]                          | [CONSTRUCTOR]                          | [METHODS]                                                                        |
| :-----: | :-------------------------------- | :------------------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `proximity.ProximityQuery`        | `ProximityQuery(mesh)`                 | `signed_distance(points)`, `on_surface(points)->(pts,dist,tid)`, `vertex(points)` |
|  [02]   | `ray.ray_triangle.RayMeshIntersector` | `RayMeshIntersector(mesh)` (or `mesh.ray`) | `intersects_location`, `intersects_id`, `intersects_first`, `intersects_any`, `contains_points` |
|  [03]   | `ray.ray_pyembree.RayMeshIntersector` | auto-selected when `ray.has_embree`    | Embree-accelerated mirror of the same intersector contract                       |
|  [04]   | `collision.CollisionManager`      | `CollisionManager()` then `add_object(name, mesh, transform)` | `in_collision_internal/single/other`, `min_distance_internal/single/other`, `set_transform` |
|  [05]   | `voxel.VoxelGrid`                 | `mesh.voxelized(pitch, method='subdivide')` | `matrix`, `points`, `as_boxes()`, `fill`/`hollow`, `marching_cubes`, `revoxelized` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: polymorphic IO (`load`, `export`, `available_formats`)
- rail: mesh

`load` resolves `file_type` from the explicit argument or the path/extension, dispatches across every registered codec, and returns the runtime-appropriate `Geometry` (a `Trimesh`, `Scene`, or `Path`); `force=` collapses ambiguous results to one kind. `export` is the symmetric writer over the same format set, returning `bytes`/`str`/`dict` when `file_obj=None`. The format is always an argument, never a `load_obj`/`load_ply`/`export_stl` function family.

| [INDEX] | [SURFACE]                                                                    | [CALL_SHAPE]                | [CAPABILITY]                                              |
| :-----: | :--------------------------------------------------------------------------- | :-------------------------- | :------------------------------------------------------- |
|  [01]   | `trimesh.load(file_obj, file_type=None, resolver=None, force=None, allow_remote=False)` | path/bytes/stream + type | polymorphic mesh/scene/path read returning `Geometry`    |
|  [02]   | `trimesh.load_mesh(*args, **kwargs) -> Trimesh`                              | path + type                 | force a single `Trimesh` result (concatenates a scene)   |
|  [03]   | `trimesh.load_scene(file_obj, file_type=None, resolver=None, allow_remote=False, metadata=None) -> Scene` | path + type | force a `Scene` result preserving the transform graph    |
|  [04]   | `trimesh.load_path(file_obj, file_type=None)`                                | path + type                 | force a `Path2D`/`Path3D` (dxf/svg) result               |
|  [05]   | `trimesh.load_remote(url, **kwargs) -> Scene`                                | URL                         | fetch + load a remote asset (requires `allow_remote`)    |
|  [06]   | `trimesh.available_formats() -> set[str]`                                    | none                        | enumerate every registered load/export extension         |
|  [07]   | `mesh.export(file_obj=None, file_type=None) -> dict\|bytes\|str`             | path/stream + type          | write a mesh to any registered format; bytes if no sink  |
|  [08]   | `scene.export(file_obj=None, file_type=None)`                                | type (e.g. `'glb'`)         | write a scene with transforms (glb/gltf/3mf/dae)         |

[ENTRYPOINT_SCOPE]: primitive creation (`creation`)
- rail: mesh

`creation.*` are static constructors returning a `Trimesh`; all accept an optional `transform` 4x4 and forward `**kwargs` to the `Trimesh` constructor. `extrude_polygon`/`revolve`/`sweep_polygon`/`triangulate_polygon` consume a `shapely` `Polygon`/`LineString`, making `creation` the bridge from the planar `shapely` rail into solids.

| [INDEX] | [SURFACE]                                                                       | [CALL_SHAPE]              | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------------------ | :------------------------ | :---------------------------------------------------- |
|  [01]   | `creation.box(extents=None, transform=None, bounds=None)`                       | extents or bounds         | axis-aligned box                                      |
|  [02]   | `creation.cylinder(radius, height=None, sections=None, segment=None)`           | radius + height/segment   | cylinder (or segment-defined)                         |
|  [03]   | `creation.cone(radius, height, sections=None)` / `creation.capsule(height, radius)` | radius + height       | cone / capsule                                        |
|  [04]   | `creation.annulus(r_min, r_max, height=None, sections=None, segment=None)`       | inner/outer radii         | annular tube                                          |
|  [05]   | `creation.torus(major_radius, minor_radius, major_sections=32, minor_sections=32)` | radii + sections        | torus                                                 |
|  [06]   | `creation.icosphere(subdivisions=3, radius=1.0)` / `creation.uv_sphere(radius, count=None)` | subdivisions/count | geodesic / UV sphere                                  |
|  [07]   | `creation.icosahedron()`                                                        | none                      | unit icosahedron seed for subdivision                 |
|  [08]   | `creation.extrude_polygon(polygon, height, transform=None, mid_plane=False)`    | shapely polygon + height  | extrude a planar `shapely` polygon to a solid         |
|  [09]   | `creation.extrude_triangulation(vertices, faces, height)`                       | 2D mesh + height          | extrude a precomputed triangulation                   |
|  [10]   | `creation.revolve(linestring, angle=None, cap=False, sections=None)`            | profile + angle           | revolve a 2D profile (full/partial)                   |
|  [11]   | `creation.sweep_polygon(polygon, path, angles=None, cap=True, connect=True)`    | shapely polygon + 3D path | sweep a profile along a polyline path                 |
|  [12]   | `creation.triangulate_polygon(polygon, triangle_args=None, engine=None) -> (V,F)` | shapely polygon          | triangulate via `triangle`/`earcut` engine            |
|  [13]   | `creation.axis(...)` / `creation.camera_marker(camera, ...)`                    | transform / camera        | debug axis tripod / camera frustum marker             |

[ENTRYPOINT_SCOPE]: boolean CSG, decomposition, repair, smoothing, remesh
- rail: mesh

Boolean rows route to the `manifold3d` engine (`engine='manifold'` default, `'blender'` fallback) and return a new `Trimesh`; `check_volume=True` rejects non-positive-volume operands. `boolean.reduce_cascade` folds an n-ary operation pairwise to balance the manifold tree. Smoothing rows mutate `mesh.vertices` in place and return the same `Trimesh`; `repair.fill_holes`/`fix_*` return `bool` success and mutate in place. `remesh.*` and `subdivide_to_size` return new vertex/face arrays or a new `Trimesh`.

| [INDEX] | [SURFACE]                                                                                 | [CALL_SHAPE]               | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------------------------------------------------- | :------------------------- | :------------------------------------------------- |
|  [01]   | `boolean.union(meshes, engine=None, check_volume=True)`                                   | mesh sequence              | n-ary watertight union via manifold                |
|  [02]   | `boolean.difference(meshes, engine=None, check_volume=True)`                              | mesh sequence              | n-ary difference (first minus rest)                |
|  [03]   | `boolean.intersection(meshes, engine=None, check_volume=True)`                            | mesh sequence              | n-ary intersection                                 |
|  [04]   | `boolean.boolean_manifold(meshes, operation, check_volume=True)` / `boolean.reduce_cascade(op, items)` | sequence + op       | low-level manifold op; pairwise cascade fold       |
|  [05]   | `mesh.union/difference/intersection(other, engine=None)`                                  | other mesh                 | method mirror of the module booleans               |
|  [06]   | `decomposition.convex_decomposition(mesh, **kwargs) -> list[dict]` / `mesh.convex_decomposition()` | mesh + VHACD params | approximate convex decomposition (manifold VHACD)  |
|  [07]   | `repair.fill_holes(mesh, use_fan=False) -> bool`                                          | mesh                       | fill boundary holes in place                       |
|  [08]   | `repair.fix_normals(mesh, multibody=False)` / `repair.fix_winding(mesh)` / `repair.fix_inversion(mesh)` | mesh           | consistent outward normals / winding / inversion   |
|  [09]   | `repair.stitch(mesh, faces=None, insert_vertices=False)` / `repair.broken_faces(mesh)`    | mesh                       | stitch open boundaries; enumerate broken faces     |
|  [10]   | `smoothing.filter_taubin(mesh, lamb=0.5, nu=0.5, iterations=10)`                           | mesh + lambda/nu           | Taubin shrink-free smoothing                       |
|  [11]   | `smoothing.filter_laplacian(mesh, lamb=0.5, iterations=10, implicit_time_integration=False, volume_constraint=True)` | mesh + iters | Laplacian smoothing (explicit/implicit)       |
|  [12]   | `smoothing.filter_humphrey(mesh, alpha=0.1, beta=0.5, iterations=10)` / `filter_mut_dif_laplacian(...)` | mesh + alpha/beta | Humphrey / mutable-diffusion Laplacian        |
|  [13]   | `smoothing.laplacian_calculation(mesh, equal_weight=True, pinned_vertices=None)`          | mesh                       | reusable `scipy.sparse` Laplacian operator         |
|  [14]   | `mesh.simplify_quadric_decimation(percent=None, face_count=None, aggression=None)`        | target % or face count     | quadric decimation (manifold backend)              |
|  [15]   | `remesh.subdivide_to_size(vertices, faces, max_edge, max_iter=10, return_index=False)` / `mesh.subdivide_to_size(...)` | V/F + max_edge | edge-length subdivision                       |
|  [16]   | `remesh.subdivide(vertices, faces, ...)` / `remesh.subdivide_loop(vertices, faces, iterations=None)` / `mesh.subdivide_loop(...)` | V/F + iters | midpoint / Loop subdivision               |

[ENTRYPOINT_SCOPE]: registration, proximity, sampling, section, split
- rail: mesh

Registration rows return a transform plus diagnostics; `mesh_other` returns `(matrix, cost)`, `icp`/`procrustes` return `(matrix, transformed, cost)`. Proximity rows return distances/points; `closest_point` and `ProximityQuery.on_surface` return the `(points, distances, triangle_id)` 3-tuple. `sample_surface`/`_even` return `(points, face_index)`. `section`/`section_multiplane` return a `Path3D`/`Path2D`; `slice_plane` returns a clipped `Trimesh`.

| [INDEX] | [SURFACE]                                                                                   | [CALL_SHAPE]              | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------------------------------------------ | :------------------------ | :-------------------------------------------------- |
|  [01]   | `registration.mesh_other(mesh, other, samples=500, scale=False, icp_first=10, icp_final=50) -> (matrix, cost)` | mesh + target | align mesh to mesh by sampled ICP             |
|  [02]   | `registration.icp(a, b, initial=None, threshold=1e-5, max_iterations=20) -> (matrix, transformed, cost)` | point sets + init | iterative closest point                     |
|  [03]   | `registration.procrustes(a, b, weights=None, reflection=True, translation=True, scale=True, return_cost=True) -> (matrix, transformed, cost)` | correspondence sets | weighted rigid/similarity Procrustes fit |
|  [04]   | `registration.nricp_amberg(source_mesh, target_geometry, source_landmarks=None, target_positions=None, steps=None, eps=1e-4, gamma=1, distance_threshold=0.1, ...)` | source + target | non-rigid Amberg deformation        |
|  [05]   | `registration.nricp_sumner(source_mesh, target_geometry, ..., face_pairs_type='vertex')`    | source + target           | non-rigid Sumner deformation                        |
|  [06]   | `proximity.closest_point(mesh, points) -> (points, distances, triangle_id)`                 | mesh + points             | closest surface points, distances, triangle ids     |
|  [07]   | `proximity.signed_distance(mesh, points)` / `ProximityQuery(mesh).signed_distance(points)`  | mesh + points             | signed distance (negative outside)                  |
|  [08]   | `proximity.thickness(mesh, points, exterior=False, method='max_sphere')` / `proximity.max_tangent_sphere(...)` | mesh + points | local wall thickness via inscribed sphere    |
|  [09]   | `sample.sample_surface(mesh, count, face_weight=None, sample_color=False, seed=None) -> (points, face_index)` | mesh + count | area-weighted surface samples                |
|  [10]   | `sample.sample_surface_even(mesh, count, radius=None, seed=None) -> (points, face_index)`   | mesh + count              | blue-noise / Poisson-disk even surface samples      |
|  [11]   | `sample.volume_mesh(mesh, count)` / `mesh.sample(count)` / `mesh.contains(points) -> bool[]` | mesh + count/points       | interior volume samples; inside/outside test        |
|  [12]   | `mesh.section(plane_normal, plane_origin) -> Path3D` / `mesh.section_multiplane(plane_origin, plane_normal, heights)` | plane(s)     | planar cross-section path(s)                        |
|  [13]   | `mesh.slice_plane(plane_origin, plane_normal, cap=False, face_index=None) -> Trimesh` / `intersections.slice_mesh_plane(...)` | plane | half-space clip (optionally capped)         |
|  [14]   | `mesh.split(only_watertight=True, ...) -> list[Trimesh]` / `mesh.submesh(faces_sequence)`   | flags / face groups       | disconnected-component split; face-group submeshes  |

[ENTRYPOINT_SCOPE]: bounds, curvature, collision, ray, voxel, poses
- rail: mesh

The analysis and spatial surfaces the dense owner composes for measurement, interference, and visibility.

| [INDEX] | [SURFACE]                                                                                | [CALL_SHAPE]              | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------------------------------------- | :------------------------ | :------------------------------------------------- |
|  [01]   | `bounds.oriented_bounds(obj, ...) -> (transform, extents)` / `bounds.minimum_cylinder(obj)` | mesh/points            | minimum-volume OBB and bounding cylinder           |
|  [02]   | `nsphere.minimum_nsphere(obj) -> (center, radius)` / `nsphere.fit_nsphere(points)`       | mesh/points               | minimum bounding sphere; least-squares sphere fit  |
|  [03]   | `curvature.discrete_gaussian_curvature_measure(mesh, points, radius)` / `discrete_mean_curvature_measure(...)` | mesh + points + radius | ball-integrated Gaussian/mean curvature   |
|  [04]   | `mesh.ray.intersects_location(ray_origins, ray_directions) -> (locations, ray_idx, tri_idx)` | rays                  | ray-surface hit points (Embree-accelerated if present) |
|  [05]   | `mesh.ray.intersects_first(...)` / `intersects_id(...)` / `intersects_any(...)`          | rays                      | first-hit triangle, all hit ids, any-hit boolean   |
|  [06]   | `collision.CollisionManager().add_object(name, mesh, transform); .in_collision_internal()` | named meshes            | FCL pairwise collision detection                   |
|  [07]   | `CollisionManager.min_distance_internal()` / `min_distance_single(mesh)` / `min_distance_other(mgr)` | manager(s)        | FCL minimum separation distance + contact          |
|  [08]   | `mesh.voxelized(pitch, method='subdivide') -> VoxelGrid` then `.matrix` / `.as_boxes()` / `.fill()` | pitch + method     | dense/sparse voxelization, fill, box mesh           |
|  [09]   | `mesh.compute_stable_poses(center_mass=None, sigma=0.0, n_samples=1, threshold=0.0) -> (transforms, probs)` | mesh            | resting-pose probabilities for placement           |
|  [10]   | `mesh.apply_obb()` / `mesh.apply_transform(matrix)` / `mesh.apply_scale(factor)`         | mesh + transform          | reorient to OBB frame; rigid/affine transform       |

## [04]-[INTEGRATION_PATTERNS]

[STACK_IO_BOUNDARY]: `load`/`export` <-> data mesh codec
- The geometry owner conditions an in-memory `Trimesh` (`vertices`/`faces` `numpy` arrays) and hands it across the wire to the data tier; `mesh.export(file_type='glb')` returning `bytes` is the only encode path, owned by `data` `MeshPayload` (`rasm.data.spatial.mesh`), not by geometry. The geometry kernel never opens a file handle: it returns the conditioned triangulation, and the data codec owns GLB/3MF/PLY serialization. `available_formats()` is the discriminator a `DatasetKind`-style dispatch reads to route a path to `load` versus a sibling codec.

[STACK_SHAPELY_TO_SOLID]: `shapely` planar -> `creation` solid
- `creation.extrude_polygon`/`revolve`/`sweep_polygon`/`triangulate_polygon` consume a `shapely` `Polygon`/`LineString` directly. The dense rail composes the `shapely` planar-operation owner (offset, buffer, boolean) to produce a clean profile, then lifts it to a solid through one `creation` call, with `triangulate_polygon(engine='triangle')` selecting the meshing backend. `Trimesh.section(...)` closes the loop back to a `Path3D` whose `.polygons_full` are `shapely` polygons, so section -> planar-op -> re-extrude is a single rail.

[STACK_MANIFOLD_CSG]: `boolean` <-> `manifold3d` algebra
- `boolean.union`/`difference`/`intersection` are the trimesh-side facade over the `manifold3d` CSG kernel; the dense owner that needs the raw `Manifold` algebra (batched booleans, `+`/`-`/`^` operators, warp/refine) drops to `manifold3d` directly and re-wraps via `trimesh.Trimesh(vertices=..., faces=...)`. Boolean operands must be watertight (`is_watertight`); the owner gates on that property before the call and surfaces a typed precondition rather than letting `check_volume` raise inside the kernel. `boolean.reduce_cascade` is the fold for n-ary input, balancing the manifold tree instead of left-folding.

[STACK_SPARSE_SMOOTHING]: `smoothing` <-> `scipy.sparse`
- `smoothing.laplacian_calculation` returns a reusable `scipy.sparse` cotangent/uniform Laplacian operator; the implicit-integration path of `filter_laplacian` calls `scipy.sparse.linalg.spsolve`. A dense conditioning pipeline computes the operator once with `pinned_vertices=` for feature-preserving boundary constraints, then reuses it across `filter_taubin`/`filter_laplacian`/`filter_humphrey` via the `laplacian_operator=` parameter rather than rebuilding the sparse system per filter.

[STACK_REGISTRATION_HANDOFF]: trimesh `registration` vs `open3d`/`small_gicp`
- trimesh owns mesh-to-mesh rigid alignment (`mesh_other`, `icp`, `procrustes`) and non-rigid deformation (`nricp_amberg`/`nricp_sumner`); it returns the 4x4 transform that feeds `apply_transform`. Point-cloud global registration (FPFH/FGR/RANSAC) and fine GICP route to `open3d`/`small_gicp`/`kiss-matcher`; the transform those engines return is applied through the same trimesh `apply_transform`, so the registration result is a shared 4x4 matrix the whole rail composes regardless of which backend estimated it.

[STACK_PROXIMITY_RECEIPT]: `proximity`/`sample` <-> deviation evidence
- The deviation/quality consumers compose `ProximityQuery(reference).signed_distance(sample.sample_surface(target, n)[0])` into one rail: sample the target surface, query signed distance against a watertight reference, fold the distance distribution into deviation receipt facts. `ProximityQuery` amortizes the `rtree` triangle-index build across the sample batch; the one-shot `proximity.closest_point` is the single-query form. The owner gates `reference.is_watertight` before `signed_distance` (the kernel raises otherwise) and lifts the precondition once at the boundary.

[STACK_IDENTITY]: `identifier_hash` <-> content-keyed caching
- `Trimesh.identifier_hash` (and `Scene.identifier_hash`) is a rotation/translation/scale-invariant content hash the runtime composes as a `ContentIdentity` seed for memoized boolean/decomposition/registration results, so geometry-equal meshes share a cache key without re-running the operation.

## [05]-[IMPLEMENTATION_LAW]

[MESH_TOPOLOGY]:
- import: `import trimesh` at boundary scope only; module-level import is banned by the manifest import policy.
- mesh axis: one `Trimesh` owns `vertices`/`faces` plus a `caching` cache of derived geometry keyed on a content hash; mass properties, topology graphs, normals, curvature, symmetry, and `scipy`/`rtree` spatial indices are lazily cached properties, never parallel mesh subclasses. `update_vertices`/`update_faces`/`process` invalidate the dependent cache.
- IO axis: `load` dispatches on the resolved `file_type` across every entry in `available_formats()`; the format is an argument, never a `load_<fmt>` function family. `export(file_type=...)` is the symmetric writer over the same set, and `force=`/`load_mesh`/`load_scene`/`load_path` collapse ambiguous results to one kind. File encode belongs to the data codec; geometry returns in-memory triangulations.
- boolean axis: `boolean.union`/`difference`/`intersection` and the `Trimesh` method mirrors route to `manifold3d` (`engine='manifold'`), require watertight operands, and fold n-ary input through `reduce_cascade`; the operation kind is the named function, distinct from the raw `manifold3d` algebra the CSG owner holds.
- decomposition axis: `convex_decomposition` runs manifold-backed VHACD returning a `list[dict]` of convex parts; convex hull is `scipy.spatial.ConvexHull`, minimum bounds are `bounds`/`nsphere` — none are hand-rolled.
- registration axis: `icp`/`mesh_other`/`procrustes` perform rigid/similarity alignment returning a 4x4 transform plus cost; `nricp_amberg`/`nricp_sumner` perform non-rigid deformation; the estimation method is the named function. Point-cloud global registration and fine GICP route to `open3d`/`small_gicp`/`kiss-matcher`, sharing the 4x4 transform contract.
- conditioning axis: `repair.fill_holes`/`fix_normals`/`fix_winding`/`fix_inversion`/`stitch` return `bool` success and mutate in place; `smoothing.filter_*` mutate `vertices` over a shared `scipy.sparse` Laplacian and return the same `Trimesh`; `simplify_quadric_decimation`/`subdivide_*`/`remesh.*` are the resolution operators; `process=True` on construction runs the default merge-and-validate pass.
- query axis: `ProximityQuery`, `RayMeshIntersector` (Embree-accelerated when `ray.has_embree`), `CollisionManager` (FCL), and `VoxelGrid` are persistent query owners that amortize index construction; prefer them over the one-shot module functions when batching against one fixed mesh.
- evidence: each load captures format, vertex/face count, and `is_watertight`/`is_winding_consistent`; each boolean/decomposition/registration captures the operation, input counts, result validity, and (registration) the transform plus cost as a mesh receipt; `mass_properties` carries `MassProperties` (volume, area, center of mass, inertia tensor).
- boundary: trimesh owns triangle-mesh modeling, conditioning, exchange, and mesh-mesh registration. `.3dm`/OpenNURBS exchange routes to `rhino3dm`; STEP/AP242 BREP to `cadquery-ocp`; point-cloud scan registration/reconstruction to `open3d`; fine GICP to `small_gicp`/`kiss-matcher`; the watertight CSG/VHACD kernel to `manifold3d`; planar operations to `shapely`; mesh-file encode to the data `MeshPayload` codec.

## [06]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `trimesh`
- Owns: triangle-mesh/scene/path IO, primitive creation from `shapely` profiles, convex hull and VHACD decomposition, minimum bounds, manifold boolean CSG, ICP/Procrustes/non-rigid registration, Laplacian/Taubin/Humphrey smoothing, quadric decimation and subdivision remesh, hole/normal/winding repair, signed-distance/closest-point/thickness proximity, surface/volume sampling, ray casting, FCL collision, and voxelization
- Accept: triangle-mesh modeling, conditioning, and exchange feeding the geometry and mesh owners; the 4x4 registration transform shared with the point-cloud registration backends
- Reject: wrapper-renames of `load`/`export`; a hand-rolled mesh IO codec, boolean kernel, sparse-Laplacian solve, convex hull, R-tree, or FCL binding where trimesh already binds them; a `load_<format>`/`export_<format>` or `Add<Op>` family over the format/operation argument row; a mesh-file encode path that bypasses the data `MeshPayload` codec; identity minting the runtime owns

[CAPTURE_GAP]:
- floor: `trimesh==4.12.2` is MIT, unmarked pure-Python (`Requires-Python: >=3.8`, `py3-none-any`) and installs on the `>=3.15` project venv. Its operation surface is gated by the optional compiled backends the manifest pins `<'3.15'`: `manifold3d` (boolean CSG and VHACD decomposition) and the unmarked `rtree` (libspatialindex triangle R-tree) lack cp315 wheels, and `python-fcl` (collision) plus `embreex` (ray accel) are native. Reflection therefore runs on a cp313/cp314 companion where those backends plus `scipy`/`shapely`/`networkx`/`pillow` resolve, enumerating the full IO/boolean/decomposition/registration/proximity/collision surface; the bare `>=3.15` venv exposes the pure-Python mesh/IO/topology/smoothing/sampling surface and raises on the boolean and R-tree-backed paths.
- members: verified by introspection against an installed `trimesh==4.12.2` companion distribution with `manifold3d`/`rtree`/`scipy`/`shapely`/`networkx` resolved; every documented type, cached property, query owner, and module entrypoint resolves with the signatures shown — no phantom. `closest_point`/`ProximityQuery.on_surface` confirmed 3-tuple `(points, distances, triangle_id)`; `icp`/`procrustes` 3-tuple `(matrix, transformed, cost)`; `mesh_other` `(matrix, cost)`; `sample_surface`/`_even` `(points, face_index)`; `fill_holes`/`fix_*` return `bool`; `smoothing.filter_*` return the mutated `Trimesh`; `boolean.*` carry `engine=Literal['manifold','blender',None]` and `check_volume`. Open-edge accessors `edges_face` and `facets_boundary` are confirmed cached properties.
