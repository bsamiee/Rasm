# [PY_DATA_API_TRIMESH]

`trimesh` supplies the triangular-mesh surface for the data aec rail: a `Trimesh` mesh root, a `Scene` graph of named geometries, a `PointCloud`, the `load`/`load_scene` polymorphic readers, and the processing submodules that drive mesh repair, boolean operations, sampling, smoothing, ICP/non-rigid registration, and exchange across `STL`/`OBJ`/`PLY`/`GLTF`/`GLB`/`OFF`/`3MF`. The package owner composes `trimesh.load`, `Trimesh`, and `Trimesh.export` into the aec owner; it never re-implements mesh booleans, mass properties, smoothing operators, or proximity queries trimesh already owns. Geometry hands an in-memory `Trimesh` (vertices/faces arrays) across the wire to this owner, which owns the mesh-file/GLB encode the geometry kernel never touches.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `trimesh`
- package: `trimesh`
- import: `import trimesh`
- owner: `data`
- rail: aec
- installed: `4.12.2` resolved by `assay api resolve trimesh` on the `>=3.15` project venv (`.venv/lib/python3.15/site-packages/trimesh-4.12.2.dist-info`); unmarked pure-Python (`py3-none-any`), so the mesh/IO/topology/visual surface reflects on the main cp315 band
- entry points: none (library only)
- capability: mesh load/export, mass properties, watertightness and manifold checks, repair, boolean union/difference/intersection, surface and volume sampling, Laplacian/Taubin/Humphrey smoothing, ICP and non-rigid registration, primitive and parametric construction, voxelization, ray and proximity queries

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: mesh, scene, and point roots
- rail: aec

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]  | [CAPABILITY]                                               |
| :-----: | :-------------------- | :-------------- | :--------------------------------------------------------- |
|  [01]   | `Trimesh`             | mesh root       | vertices/faces mesh with mass, topology, and edit methods  |
|  [02]   | `Scene`               | scene graph     | named geometries under a transform tree with camera/lights |
|  [03]   | `PointCloud`          | point set       | vertices with colors, `kdtree`, and convex hull            |
|  [04]   | `parent.Geometry`     | geometry base   | shared bounds/transform/export contract (`Trimesh` return discriminant) |
|  [05]   | `path.Path3D`         | path geometry   | 3D entity/vertex curve network from `load_path`/`section`  |
|  [06]   | `primitives.Box`      | primitive solid | `Box`/`Sphere`/`Cylinder`/`Capsule`/`Extrusion`/`Primitive` primitives |
|  [07]   | `visual.ColorVisuals` / `visual.TextureVisuals` | visual layer | `Trimesh.visual` union: `ColorVisuals.vertex_colors`/`face_colors` are `(N,4)` `uint8` RGBA properties (defaults synthesized when undefined); `TextureVisuals` carries UV/material and exposes neither, gated behind `visual.kind in {'vertex','face','texture',None}` |
|  [08]   | `voxel.VoxelGrid`     | voxel volume    | sparse occupancy grid from `Trimesh.voxelized`             |

[PUBLIC_TYPE_SCOPE]: `Trimesh` topology and mass accessors
- rail: aec

Cached topology accessors expose the connectivity downstream owners read directly instead of recomputing; they recompute on `process()` after vertex/face edits. Mass and validity accessors carry the receipt fields.

| [INDEX] | [SYMBOL]                                         | [PACKAGE_ROLE]  | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------- | :-------------- | :------------------------------------------------------ |
|  [01]   | `Trimesh.vertices` / `faces` / `face_normals` / `vertex_normals` | array | core geometry arrays                          |
|  [02]   | `Trimesh.edges` / `edges_unique` / `edges_sorted` | array          | directed / deduplicated / sorted edge arrays           |
|  [03]   | `Trimesh.edges_face`                             | array           | per-edge owning-face index (open-edge / boundary scan) |
|  [04]   | `Trimesh.face_adjacency` / `face_adjacency_edges` | array          | adjacent face pairs and their shared edges             |
|  [05]   | `Trimesh.facets` / `facets_boundary`             | list            | coplanar facet groups and their boundary edge loops    |
|  [06]   | `Trimesh.vertex_faces` / `edges_sparse`          | array / sparse  | vertex->face incidence; sparse edge adjacency          |
|  [07]   | `Trimesh.mass_properties` / `moment_inertia` / `principal_inertia_components` | dict / array | volume/COM/inertia receipt and inertia tensor |
|  [08]   | `Trimesh.is_watertight` / `is_winding_consistent` / `is_volume` / `euler_number` | bool / int | manifold/validity flags and Euler characteristic |
|  [09]   | `Trimesh.identifier_hash`                        | bytes           | order-invariant geometric hash for content identity    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: load, construct, and export
- rail: aec

Load rows share file object or path, `file_type`, `resolver`, and `allow_remote` policy; `load` returns a `parent.Geometry` (`Trimesh`, `Scene`, or `Path3D`) discriminated by source content, while `load_mesh`/`load_scene` force the return kind. `file_type` forces a format only when extension is absent.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                                                                   | [CAPABILITY]                          |
| :-----: | :-------------------- | :--------------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `trimesh.load`        | `load(file_obj, file_type=None, resolver=None, force=None, allow_remote=False, **kwargs) -> Geometry` | polymorphic load to mesh/scene/path   |
|  [02]   | `trimesh.load_mesh`   | `load_mesh(*args, **kwargs) -> Trimesh`                                                         | load forced to a `Trimesh`            |
|  [03]   | `trimesh.load_scene`  | `load_scene(file_obj, file_type=None, resolver=None, allow_remote=False, metadata=None, **kwargs) -> Scene` | load forced to a `Scene` |
|  [04]   | `trimesh.load_path`   | `load_path(file_obj, file_type=None, **kwargs) -> Path3D`                                       | load a `Path3D`/`Path2D`              |
|  [05]   | `trimesh.load_remote` | `load_remote(url, **kwargs) -> Scene`                                                           | fetch and load a remote asset         |
|  [06]   | `Trimesh`             | `Trimesh(vertices=None, faces=None, process=True, validate=False, **kwargs)`                    | construct a mesh                      |
|  [07]   | `Trimesh.export`      | `export(file_obj=None, file_type=None, **kwargs)`                                               | serialize to `STL`/`OBJ`/`PLY`/`GLB`/`3MF`/`OFF` |
|  [08]   | `available_formats`   | `available_formats() -> set[str]`                                                               | enumerate supported exchange types    |

[ENTRYPOINT_SCOPE]: process, query, and combine
- rail: aec

Method rows operate on a `Trimesh`; boolean rows accept a second mesh or a sequence and an optional `engine`. Booleans dispatch to the in-process `manifold` engine by default (`engine=None`); the only optional external engine is `blender` (`trimesh.boolean.engines_available == {None, 'blender'}`), selected by `engine='blender'`.

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]                                                                                  | [CAPABILITY]                          |
| :-----: | :------------------------------------ | :-------------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `Trimesh.mass_properties`             | property -> `dict`                                                                            | volume/center-of-mass/inertia receipt |
|  [02]   | `Trimesh.union`                       | `union(other, engine=None, check_volume=True, **kwargs) -> Trimesh`                           | boolean union (manifold default)      |
|  [03]   | `Trimesh.difference`                  | `difference(other, engine=None, check_volume=True, **kwargs) -> Trimesh`                      | boolean difference                    |
|  [04]   | `Trimesh.intersection`                | `intersection(other, engine=None, check_volume=True, **kwargs) -> Trimesh`                    | boolean intersection                  |
|  [05]   | `Trimesh.slice_plane`                 | `slice_plane(plane_origin, plane_normal, cap=False, face_index=None, **kwargs) -> Trimesh`    | planar cut, optionally capped         |
|  [06]   | `Trimesh.section`                     | `section(plane_normal, plane_origin, **kwargs) -> Path3D \| None`                             | planar cross-section curve            |
|  [07]   | `Trimesh.simplify_quadric_decimation` | `simplify_quadric_decimation(percent=None, face_count=None, aggression=None) -> Trimesh`      | quadric decimation by percent/count   |
|  [08]   | `Trimesh.subdivide_loop`              | `subdivide_loop(iterations=None) -> Trimesh`                                                  | Loop subdivision refinement           |
|  [09]   | `Trimesh.sample`                      | `sample(count, return_index=False, face_weight=None)`                                         | area-weighted surface point sampling  |
|  [10]   | `Trimesh.contains`                    | `contains(points) -> ndarray[bool]`                                                           | inside/outside test                   |
|  [11]   | `Trimesh.fill_holes` / `fix_normals`  | `fill_holes(use_fan=False)` / `fix_normals(multibody=False)`                                  | close boundary loops / consistent winding |
|  [12]   | `Trimesh.convex_hull`                 | property -> `Trimesh`                                                                         | convex hull mesh                      |
|  [13]   | `Trimesh.bounding_box_oriented` / `bounding_cylinder` | property -> `Trimesh` / primitive                                            | minimum-volume oriented bound          |
|  [14]   | `Trimesh.voxelized`                   | `voxelized(pitch, method='subdivide', **kwargs) -> VoxelGrid`                                 | voxel-grid occupancy                  |

[ENTRYPOINT_SCOPE]: processing submodules
- rail: aec

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                                                                              | [CAPABILITY]                                  |
| :-----: | :--------------------------- | :-------------------------------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `creation.box`               | `box(extents=None, transform=None, bounds=None, **kwargs)`                                                | `box`/`cylinder`/`cone`/`icosphere`/`uv_sphere`/`capsule`/`annulus`/`torus` primitive meshes |
|  [02]   | `creation.extrude_polygon`   | `extrude_polygon(polygon, height, transform=None, mid_plane=False, **kwargs)`                             | extrude/`revolve(linestring,angle,...)`/`sweep_polygon(polygon,path,...)` |
|  [03]   | `creation.triangulate_polygon` | `triangulate_polygon(polygon, triangle_args=None, engine=None, ...) -> (vertices, faces)`               | 2D polygon triangulation                      |
|  [04]   | `boolean.union`              | `union(meshes, engine=None, check_volume=True, **kwargs)` (also `difference`/`intersection`)             | module-level boolean over a sequence; `boolean_manifold(meshes, operation, ...)` is the in-process kernel |
|  [05]   | `repair.fix_winding`         | `fix_winding(mesh)` / `fix_normals(mesh, multibody=False)` / `fix_inversion(mesh)` / `stitch(mesh)`        | winding/normal/inversion/stitch repair        |
|  [06]   | `repair.fill_holes`          | `fill_holes(mesh, use_fan=False)` / `broken_faces(mesh)` / `connected_components(...)`                    | hole filling and degenerate/broken-face detection |
|  [07]   | `smoothing.filter_taubin`    | `filter_taubin(mesh, lamb=0.5, nu=0.5, iterations=10, laplacian_operator=None)`                           | Taubin (shrink-free) smoothing                |
|  [08]   | `smoothing.filter_laplacian` | `filter_laplacian(mesh, lamb=0.5, iterations=10, implicit_time_integration=False, volume_constraint=True, laplacian_operator=None)` | Laplacian smoothing (implicit/volume-preserving options) |
|  [09]   | `smoothing.filter_humphrey`  | `filter_humphrey(mesh, alpha=0.1, beta=0.5, iterations=10, laplacian_operator=None)`                      | Humphrey shrink-compensated smoothing         |
|  [10]   | `registration.icp`           | `icp(a, b, initial=None, threshold=1e-05, max_iterations=20, **kwargs)`                                   | iterative closest point fit -> 4x4 transform  |
|  [11]   | `registration.procrustes`    | `procrustes(a, b, weights=None, reflection=True, translation=True, scale=True, return_cost=True)`         | rigid/similarity least-squares alignment      |
|  [12]   | `registration.mesh_other`    | `mesh_other(mesh, other, samples=500, scale=False, icp_first=10, icp_final=50, **kwargs)`                 | mesh-to-mesh alignment via sampled ICP        |
|  [13]   | `registration.nricp_amberg`  | `nricp_amberg(source_mesh, target_geometry, source_landmarks=None, target_positions=None, steps=None, eps=1e-4, gamma=1, distance_threshold=0.1, ...)` | non-rigid ICP (Amberg) deformed vertices |
|  [14]   | `registration.nricp_sumner`  | `nricp_sumner(source_mesh, target_geometry, ..., face_pairs_type='vertex')`                               | non-rigid ICP (Sumner) deformation            |
|  [15]   | `sample.sample_surface`      | `sample_surface(mesh, count, face_weight=None, sample_color=False, seed=None)`                            | area-weighted surface sampling (deterministic via `seed`) |
|  [16]   | `sample.sample_surface_even` | `sample_surface_even(mesh, count, radius=None, seed=None)`                                                | Poisson-disc even surface sampling            |
|  [17]   | `sample.volume_mesh`         | `volume_mesh(mesh, count) -> ndarray`                                                                     | interior volume point sampling                |
|  [18]   | `remesh.subdivide_to_size`   | `subdivide_to_size(vertices, faces, max_edge, max_iter=10, return_index=False)`                           | edge-length-bounded subdivision               |
|  [19]   | `proximity.closest_point`    | `closest_point(mesh, points) -> (closest[m,3], distance[m], triangle_id[m])`                              | nearest-surface point, distance, and face id  |
|  [20]   | `proximity.signed_distance`  | `signed_distance(mesh, points) -> ndarray`                                                                | signed distance (negative inside watertight mesh) |

## [04]-[IMPLEMENTATION_LAW]

[MESH_AEC]:
- import: `import trimesh` at boundary scope only; module-level import is banned by the manifest import policy.
- load axis: `trimesh.load` is the polymorphic intake; the return kind (`Trimesh`, `Scene`, `Path3D`) discriminates on source content, not a parallel reader family. `load_mesh`/`load_scene` force the return kind; `file_type` forces a format only when extension is absent.
- mesh axis: one `Trimesh` owns geometry, topology, mass properties, repair, and export; cached derived properties (`edges_unique`, `edges_face`, `face_adjacency`, `facets`, `facets_boundary`) recompute on `process()`. Open-edge / boundary work reads `edges_face`/`facets_boundary` directly, never a hand-rolled edge-incidence scan. Vertex/face edits route through `update_vertices`/`update_faces`/`merge_vertices`, never raw array mutation.
- boolean axis: `union`/`difference`/`intersection` (method and `trimesh.boolean` module forms over a sequence) default to the in-process `manifold` engine (`engine=None`); the only optional external engine is `blender` — `trimesh.boolean.engines_available == {None, 'blender'}` (`manifold` is always present as the default and is not enumerated). `boolean_manifold(meshes, operation, ...)` is the kernel; the operation is the function name, never a separate boolean class.
- registration axis: `registration.icp`/`procrustes`/`mesh_other` return a 4x4 transform (and cost); `nricp_amberg`/`nricp_sumner` return deformed vertices for the non-rigid case.
- scene axis: a `Scene` holds named geometries under a transform tree; `Scene.to_mesh()` / `Scene.dump(concatenate=True)` collapse it to a single `Trimesh`, `add_geometry`/`subscene`/`convert_units` edit the graph, and `Scene.export` serializes to GLB. The owner concatenates a scene before mesh-algebra ops, never iterates raw node transforms by hand.
- proximity axis: `proximity.closest_point` returns the `(closest, distance, triangle_id)` 3-tuple and `signed_distance` returns the signed field; surface-proximity and inside/outside queries route here, never a hand-rolled KD-tree over faces.
- evidence: each mesh op captures vertex count, face count, watertight flag, volume, surface area, `identifier_hash`, and export byte length as an aec receipt.
- boundary: trimesh owns triangular meshes, smoothing, booleans, mass properties, proximity, and exchange (the in-memory `Trimesh` -> file/GLB encode); `STEP`/`BREP` route to the OCC owners, IFC to `ifcopenshell`, point-cloud registration to `open3d`; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `trimesh`
- Owns: mesh load/export, mass properties, repair, booleans, sampling, smoothing, registration, primitive construction, voxelization, proximity and ray queries
- Accept: triangular-mesh inspection, processing, and exchange feeding the aec and geometry owners
- Reject: wrapper-renames of `load`/`export`; a hand-rolled boolean, smoothing, or mass-property kernel where trimesh is admitted; raw vertex-array mutation that bypasses `update_vertices`; a hand-rolled face KD-tree where `proximity.closest_point`/`signed_distance` apply; a `blender`-engine assumption where `manifold` is the in-process default; identity minting the runtime owns

[CAPTURE_GAP]:
- floor: `trimesh==4.12.2` is MIT, unmarked pure-Python (`py3-none-any`, hard `numpy` dependency only), and `assay api resolve trimesh` finds the distribution on the `>=3.15` project venv — so the mesh/IO/topology/visual surface this `data` rail consumes reflects directly on the main cp315 band, no companion interpreter. The optional compiled backends the manifest pins `<'3.15'` (`manifold3d` boolean CSG, `rtree` libspatialindex) gate only the boolean/R-tree paths, which this file-exchange owner never drives; `shapely` is optional, gating `extrude_polygon`/`triangulate_polygon` alone
- members: every documented type, method, accessor, entrypoint, and submodule resolves by reflection against the locked cp315 distribution; `ColorVisuals.vertex_colors` and `ColorVisuals.face_colors` reflect as `(len(vertices), 4)` / `(len(faces), 4)` `uint8` RGBA `property` accessors that synthesize defaults when no color is defined, `ColorVisuals.kind` reports `'vertex'`/`'face'`/`None`, and `TextureVisuals` exposes neither `vertex_colors` nor `face_colors` (`hasattr` is `False`) with `kind == 'texture'`; `engines_available` reflects `{None, 'blender'}` (manifold is the always-present default and is not listed), `proximity.closest_point` reflects the `(closest, distance, triangle_id)` 3-tuple, and `simplify_quadric_decimation` reflects `percent`/`face_count`/`aggression` — no phantom
