# [PY_DATA_API_TRIMESH]

`trimesh` supplies the triangular-mesh surface for the data aec rail: a `Trimesh` mesh root, a `Scene` graph of named geometries, a `PointCloud`, the `load` polymorphic reader, and the processing submodules that drive mesh repair, boolean operations, sampling, smoothing, ICP registration, and exchange across `STL`/`OBJ`/`PLY`/`GLTF`/`OFF`/`3MF`. The package owner composes `trimesh.load`, `Trimesh`, and `Trimesh.export` into the aec owner; it never re-implements mesh booleans, mass properties, or proximity queries trimesh already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `trimesh`
- package: `trimesh`
- import: `import trimesh`
- owner: `data`
- rail: aec
- installed: `4.12.2` reflected via `python -c "import trimesh"` on cp313
- entry points: none (library only)
- capability: mesh load/export, mass properties, watertightness and manifold checks, repair, boolean union/difference/intersection, surface and volume sampling, Laplacian/Taubin/Humphrey smoothing, ICP and non-rigid registration, primitive and parametric construction, voxelization, ray and proximity queries

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: mesh, scene, and point roots
- rail: aec

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]  | [CAPABILITY]                                               |
| :-----: | :-------------------- | :-------------- | :--------------------------------------------------------- |
|   [1]   | `Trimesh`             | mesh root       | vertices/faces mesh with mass, topology, and edit methods  |
|   [2]   | `Scene`               | scene graph     | named geometries under a transform tree with camera/lights |
|   [3]   | `PointCloud`          | point set       | vertices with colors, KD-tree, and convex hull             |
|   [4]   | `parent.Geometry`     | geometry base   | shared bounds/transform/export contract                    |
|   [5]   | `path.Path3D`         | path geometry   | 3D entity/vertex curve network from `load_path`            |
|   [6]   | `primitives.Box`      | primitive solid | `Box`/`Sphere`/`Cylinder`/`Capsule`/`Extrusion` primitives |
|   [7]   | `visual.ColorVisuals` | visual layer    | per-vertex/face color and `TextureVisuals` material        |
|   [8]   | `voxel.VoxelGrid`     | voxel volume    | sparse occupancy grid from `Trimesh.voxelized`             |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: load, construct, and export
- rail: aec

Load rows share file object or path, `file_type`, resolver, and process-on-load policy; `load` returns a `Trimesh`, `Scene`, or `Path` discriminated by source content.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]               | [CAPABILITY]                        |
| :-----: | :-------------------- | :------------------------- | :---------------------------------- |
|   [1]   | `trimesh.load`        | file/path plus type policy | polymorphic load to mesh/scene/path |
|   [2]   | `trimesh.load_mesh`   | file/path plus type policy | load forced to a `Trimesh`          |
|   [3]   | `trimesh.load_scene`  | file/path plus type policy | load forced to a `Scene`            |
|   [4]   | `trimesh.load_path`   | file/path plus type policy | load a `Path3D`/`Path2D`            |
|   [5]   | `trimesh.load_remote` | URL plus load policy       | fetch and load a remote asset       |
|   [6]   | `Trimesh`             | vertices plus faces policy | construct a mesh                    |
|   [7]   | `Trimesh.export`      | path or type plus options  | serialize to `STL`/`OBJ`/`GLB`      |
|   [8]   | `available_formats`   | none                       | enumerate supported exchange types  |

[ENTRYPOINT_SCOPE]: process, query, and combine
- rail: aec

Method rows operate on a `Trimesh`; boolean and registration rows accept a second mesh or point set and return new geometry or a transform.

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]           | [CAPABILITY]                          |
| :-----: | :------------------------------------ | :--------------------- | :------------------------------------ |
|   [1]   | `Trimesh.mass_properties`             | density property       | volume/center-of-mass/inertia receipt |
|   [2]   | `Trimesh.is_watertight`               | property               | watertightness check                  |
|   [3]   | `Trimesh.fill_holes`                  | in-place               | close boundary loops                  |
|   [4]   | `Trimesh.fix_normals`                 | in-place               | consistent winding and normals        |
|   [5]   | `Trimesh.union`                       | other mesh plus engine | boolean union                         |
|   [6]   | `Trimesh.difference`                  | other mesh plus engine | boolean difference                    |
|   [7]   | `Trimesh.intersection`                | other mesh plus engine | boolean intersection                  |
|   [8]   | `Trimesh.slice_plane`                 | origin plus normal     | planar cap/section                    |
|   [9]   | `Trimesh.simplify_quadric_decimation` | target face count      | quadric mesh decimation               |
|  [10]   | `Trimesh.subdivide_loop`              | iteration count        | Loop subdivision refinement           |
|  [11]   | `Trimesh.sample`                      | sample count           | uniform surface point sampling        |
|  [12]   | `Trimesh.contains`                    | query points           | inside/outside test                   |
|  [13]   | `Trimesh.convex_hull`                 | property               | convex hull mesh                      |
|  [14]   | `Trimesh.voxelized`                   | pitch                  | voxel-grid occupancy                  |

[ENTRYPOINT_SCOPE]: processing submodules
- rail: aec

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]                 | [CAPABILITY]                        |
| :-----: | :-------------------------- | :--------------------------- | :---------------------------------- |
|   [1]   | `creation.box`              | extents/transform            | `box`/`cylinder`/`cone`/`icosphere` |
|   [2]   | `creation.extrude_polygon`  | polygon plus height          | extrude/`revolve`/`sweep_polygon`   |
|   [3]   | `repair.fix_winding`        | mesh                         | winding/inversion/`stitch` repair   |
|   [4]   | `repair.broken_faces`       | mesh                         | degenerate-face detection           |
|   [5]   | `smoothing.filter_taubin`   | mesh plus lambda/nu          | Taubin/Laplacian/Humphrey smoothing |
|   [6]   | `registration.icp`          | source plus target points    | iterative closest point fit         |
|   [7]   | `registration.procrustes`   | point pairs                  | rigid least-squares alignment       |
|   [8]   | `registration.nricp_amberg` | source plus target mesh      | non-rigid ICP deformation           |
|   [9]   | `sample.sample_surface`     | mesh plus count              | area-weighted surface sampling      |
|  [10]   | `sample.volume_mesh`        | mesh plus count              | interior volume point sampling      |
|  [11]   | `remesh.subdivide_to_size`  | vertices/faces plus max edge | edge-length-bounded subdivision     |
|  [12]   | `proximity.closest_point`   | mesh plus query points       | nearest-surface point and distance  |

## [4]-[IMPLEMENTATION_LAW]

[MESH_AEC]:
- import: `import trimesh` at boundary scope only; module-level import is banned by the manifest import policy.
- load axis: `trimesh.load` is the polymorphic intake; the return kind (`Trimesh`, `Scene`, `Path3D`) discriminates on source content, not a parallel reader family. `file_type` forces a format only when extension is absent.
- mesh axis: one `Trimesh` owns geometry, topology, mass properties, repair, and export; cached derived properties (`edges_unique`, `face_adjacency`, `facets`) recompute on `process`. Vertex/face edits route through `update_vertices`/`update_faces`/`merge_vertices`, never raw array mutation.
- boolean axis: `union`/`difference`/`intersection` dispatch to the `manifold` engine (`boolean.engines_available`); the operation kind is a `BooleanOperationType` row, never separate boolean classes.
- registration axis: `registration.icp`/`procrustes` return a 4x4 transform; `nricp_amberg`/`nricp_sumner` return deformed vertices for the non-rigid case.
- evidence: each mesh op captures vertex count, face count, watertight flag, volume, surface area, and export byte length as an aec receipt.
- boundary: trimesh owns triangular meshes and exchange; `STEP`/`BREP` route to the OCC owners, IFC to `ifcopenshell`, point-cloud registration to `open3d`; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `trimesh`
- Owns: mesh load/export, mass properties, repair, booleans, sampling, smoothing, registration, primitive construction, voxelization, proximity and ray queries
- Accept: triangular-mesh inspection, processing, and exchange feeding the aec and geometry owners
- Reject: wrapper-renames of `load`/`export`; a hand-rolled boolean or mass-property kernel where trimesh is admitted; raw vertex-array mutation that bypasses `update_vertices`; identity minting the runtime owns

[CAPTURE_GAP]:
- floor: companion interpreter cp313; `trimesh==4.12.2` ships pure-Python wheels but rides the `shapely`/`numpy` native floor, and the `>=3.15` project venv carries no scientific companions, so the project-venv `assay api query` resolves no source there
- members: verified by introspection against the installed cp313 distribution; every documented type, method, entrypoint, and submodule resolves — no phantom
