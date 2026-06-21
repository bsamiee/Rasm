# [PY_GEOMETRY_API_TRIMESH]

`trimesh` supplies the triangle-mesh modeling and exchange surface for the geometry rail: a `Trimesh` body with cached geometric/topological properties, a `Scene` graph of named geometries, a `PointCloud`, the polymorphic `load` reader across `obj`/`ply`/`stl`/`glb`/`off`/`dxf`, primitive `creation` constructors, manifold-backed `boolean` CSG, ICP and non-rigid `registration`, mesh `repair`/`smoothing`/`remesh`, and `proximity`/`sample` queries. The package owner composes `load`, `Trimesh.export`, and the `creation`/`boolean`/`registration` modules into the mesh owner; it never re-implements mesh IO codecs, the manifold boolean kernel, or convex hull.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `trimesh`
- package: `trimesh`
- import: `import trimesh`
- owner: `geometry`
- rail: mesh
- installed: `4.12.2` reflected via `python -c "import trimesh"` on cp313
- entry points: none (library only)
- capability: mesh and scene IO across `obj`/`ply`/`stl`/`glb`/`gltf`/`off`/`dxf`/`xyz`, primitive creation, convex hull and decomposition, manifold boolean CSG, ICP and non-rigid registration, Laplacian/Taubin/Humphrey smoothing, quadric decimation and subdivision remeshing, hole repair and normal fixing, signed-distance and closest-point proximity, and surface sampling

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry roots (`trimesh`)
- rail: mesh

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------- | :------------ | :------------------------------------------------------ |
|  [01]   | `Trimesh`    | triangle mesh | vertices/faces with topology, mass, boolean, repair     |
|  [02]   | `Scene`      | scene graph   | named geometries, transforms, dump/export/to_mesh       |
|  [03]   | `PointCloud` | point cloud   | vertices/colors with kdtree query and convex hull       |
|  [04]   | `Geometry`   | geometry root | abstract base for `Trimesh`/`Scene`/`PointCloud`/`Path` |

[PUBLIC_TYPE_SCOPE]: cached mesh property axes (`Trimesh`)
- rail: mesh

`Trimesh` exposes derived geometry as lazily cached properties keyed off `vertices`/`faces`; each property recomputes on geometry change.

| [INDEX] | [PROPERTY]                                | [PROPERTY_FAMILY] | [CAPABILITY]                            |
| :-----: | :---------------------------------------- | :---------------- | :-------------------------------------- |
|  [01]   | `volume` / `area` / `center_mass`         | mass property     | enclosed volume, surface area, centroid |
|  [02]   | `mass_properties` / `moment_inertia`      | inertia           | full inertia tensor and principal axes  |
|  [03]   | `is_watertight` / `is_winding_consistent` | validity          | manifold and orientation status         |
|  [04]   | `is_convex` / `is_volume` / `is_empty`    | validity          | convexity and solidity flags            |
|  [05]   | `convex_hull` / `bounding_box_oriented`   | derived solid     | convex hull and oriented bound          |
|  [06]   | `face_normals` / `vertex_normals`         | normals           | per-face and per-vertex normals         |
|  [07]   | `edges_unique` / `face_adjacency`         | topology          | unique edges and face adjacency graph   |
|  [08]   | `facets` / `vertex_defects`               | topology          | coplanar facet groups, angle defects    |
|  [09]   | `kdtree` / `triangles_tree`               | spatial index     | vertex kdtree and triangle R-tree       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: load, export, and primitive creation
- rail: mesh

`load` dispatches on `file_type` (or file extension) across every registered format; `creation.*` are static constructors returning a `Trimesh`.

| [INDEX] | [SURFACE]                                   | [CALL_SHAPE]             | [CAPABILITY]                        |
| :-----: | :------------------------------------------ | :----------------------- | :---------------------------------- |
|  [01]   | `trimesh.load(file_obj, file_type=None)`    | path/bytes plus type     | polymorphic mesh/scene/path read    |
|  [02]   | `trimesh.load_mesh(file_obj, file_type=None)` | path plus type         | force a `Trimesh` result            |
|  [03]   | `trimesh.load_scene(file_obj)`              | path plus type           | force a `Scene` result              |
|  [04]   | `trimesh.available_formats()`               | none                     | enumerate supported extensions      |
|  [05]   | `mesh.export(file_obj, file_type)`          | path plus type           | write mesh to any registered format |
|  [06]   | `scene.export(file_type)`                   | type                     | write scene (e.g. `glb`)            |
|  [07]   | `creation.box(extents, transform)`          | extents plus transform   | axis-aligned box mesh               |
|  [08]   | `creation.cylinder(radius, height)`         | radius plus height       | cylinder mesh                       |
|  [09]   | `creation.icosphere(subdivisions, radius)`  | subdivisions plus radius | geodesic sphere mesh                |
|  [10]   | `creation.extrude_polygon(polygon, height)` | polygon plus height      | extrude a Shapely polygon           |
|  [11]   | `creation.revolve(linestring, angle)`       | profile plus angle       | revolve a 2D profile                |
|  [12]   | `creation.sweep_polygon(polygon, path)`     | polygon plus path        | sweep a profile along a path        |

[ENTRYPOINT_SCOPE]: boolean CSG, repair, smoothing, and remesh
- rail: mesh

Boolean rows route to the `manifold3d` backend and return a new `Trimesh`; smoothing rows mutate in place; remesh rows return new vertices/faces.

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]         | [CAPABILITY]                     |
| :-----: | :------------------------------------------------- | :------------------- | :------------------------------- |
|  [01]   | `boolean.union(meshes)`                            | mesh sequence        | n-ary watertight union           |
|  [02]   | `boolean.difference(meshes)`                       | mesh sequence        | n-ary difference                 |
|  [03]   | `boolean.intersection(meshes)`                     | mesh sequence        | n-ary intersection               |
|  [04]   | `mesh.convex_decomposition()`                      | none                 | approximate convex decomposition |
|  [05]   | `repair.fill_holes(mesh)`                          | mesh                 | fill boundary holes              |
|  [06]   | `repair.fix_normals(mesh)`                         | mesh                 | consistent outward normals       |
|  [07]   | `repair.fix_winding(mesh)`                         | mesh                 | consistent face winding          |
|  [08]   | `smoothing.filter_taubin(mesh)`                    | mesh plus lambda/nu  | Taubin shrink-free smoothing     |
|  [09]   | `smoothing.filter_laplacian(mesh)`                 | mesh plus iterations | Laplacian smoothing              |
|  [10]   | `smoothing.filter_humphrey(mesh)`                  | mesh plus alpha/beta | Humphrey classes smoothing       |
|  [11]   | `mesh.simplify_quadric_decimation(face_count)`     | target faces         | quadric decimation               |
|  [12]   | `remesh.subdivide_to_size(verts, faces, max_edge)` | vertices/faces       | edge-length subdivision          |

[ENTRYPOINT_SCOPE]: registration, proximity, sampling, and section
- rail: mesh

Registration rows return a transform plus cost; proximity rows return distances/points; section rows return a `Path3D`/`Path2D`.

| [INDEX] | [SURFACE]                                   | [CALL_SHAPE]         | [CAPABILITY]                         |
| :-----: | :------------------------------------------ | :------------------- | :----------------------------------- |
|  [01]   | `registration.mesh_other(mesh, other)`      | mesh plus target     | align mesh to mesh by ICP            |
|  [02]   | `registration.icp(a, b)`                    | point sets plus init | iterative closest point              |
|  [03]   | `registration.procrustes(a, b)`             | point sets           | rigid Procrustes fit                 |
|  [04]   | `registration.nricp_amberg(source, target)` | source plus target   | non-rigid Amberg deformation         |
|  [05]   | `proximity.closest_point(mesh, points)`     | mesh plus points     | closest surface points, distances, and triangle ids (3-tuple) |
|  [06]   | `proximity.signed_distance(mesh, points)`   | mesh plus points     | signed distance field samples        |
|  [07]   | `sample.sample_surface(mesh, count)`        | mesh plus count      | area-weighted surface samples        |
|  [08]   | `sample.sample_surface_even(mesh, count)`   | mesh plus count      | blue-noise even surface samples      |
|  [09]   | `mesh.contains(points)`                     | points               | inside/outside test                  |
|  [10]   | `mesh.section(plane_normal, plane_origin)`  | plane                | planar cross-section path            |
|  [11]   | `mesh.slice_plane(plane_origin, normal)`    | plane                | half-space clip                      |
|  [12]   | `mesh.split(only_watertight)`               | flag                 | disconnected-component split         |

## [04]-[IMPLEMENTATION_LAW]

[MESH_TOPOLOGY]:
- import: `import trimesh` at boundary scope only; module-level import is banned by the manifest import policy.
- mesh axis: one `Trimesh` owns `vertices`/`faces` plus a cache of derived geometry; mass properties, topology graphs, normals, and spatial indices are lazily cached properties, never parallel mesh subclasses. Mutating `vertices`/`faces` invalidates the cache through `process`.
- IO axis: `load` dispatches on the resolved `file_type` across every entry in `available_formats()`; the format is an argument, never a `load_obj`/`load_ply` function family. `export` is the symmetric writer over the same format set.
- boolean axis: `boolean.union`/`difference`/`intersection` and `Trimesh.union`/`difference`/`intersection` route to the `manifold3d` engine; watertight input is required, and the operation kind is the named function, distinct from the CSG owner that `manifold3d` holds.
- registration axis: `registration.icp`/`mesh_other`/`procrustes` perform rigid alignment and `nricp_amberg`/`nricp_sumner` perform non-rigid deformation; the estimation method is the named function, and small-cloud fine GICP routes to `small_gicp`.
- repair axis: `repair.fill_holes`/`fix_normals`/`fix_winding` and `smoothing.filter_taubin`/`filter_laplacian`/`filter_humphrey` are in-place mesh-conditioning rows; `process=True` on construction runs the default merge-and-validate pass.
- evidence: each load captures format, vertex/face count, and `is_watertight`; each boolean and registration captures the operation, input counts, and result validity as a mesh receipt; `mass_properties` carries volume, area, center of mass, and inertia.
- boundary: trimesh owns triangle-mesh modeling, exchange, and mesh-mesh registration; `.3dm`/OpenNURBS exchange routes to `rhino3dm`, point-cloud scan registration and reconstruction to `open3d`, fine GICP to `small_gicp`, and the watertight CSG kernel to `manifold3d`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `trimesh`
- Owns: triangle-mesh and scene IO, primitive creation, convex hull and decomposition, manifold boolean CSG, ICP and non-rigid registration, smoothing and remeshing, hole repair, and proximity/sampling queries
- Accept: triangle-mesh modeling and exchange feeding the geometry and mesh owners
- Reject: wrapper-renames of `load`/`export`; a hand-rolled mesh IO codec or boolean kernel where trimesh is admitted; a `load_<format>` or `Add<Op>` family over the format/operation argument row; identity minting the runtime owns

[CAPTURE_GAP]:
- floor: `trimesh` is admitted pure-Python under `>=3.15`; reflection runs on a cp313 companion interpreter where its compiled optional backends (`manifold3d`, `rtree`) resolve, so the full IO/boolean/registration surface enumerates there while the `>=3.15` project venv lacks those compiled backends
- members: verified by introspection against the installed cp313 distribution; every documented type, cached property, and module entrypoint resolves — no phantom
