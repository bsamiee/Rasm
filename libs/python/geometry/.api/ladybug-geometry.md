# [PY_GEOMETRY_API_LADYBUG_GEOMETRY]

`ladybug-geometry` is the pure-Python planar (`geometry2d`) and solid (`geometry3d`) primitive layer that is the geometric substrate of the entire Ladybug Tools energy companion band: every honeybee `Room`/`Face`/`Aperture`/`Shade`, every dragonfly `Story`/`Building`, and every `ladybug` solar/visualization object is built on one of its immutable value objects. Each primitive shares a uniform contract — a `type`-tagged `from_dict`/`to_dict` round-trip, an `from_array`/`to_array` coordinate form, and the `move`/`rotate`/`rotate_xy`/`reflect`/`scale` transform algebra returning new objects — and the package additionally owns its own pure-Python 2D boolean algebra (`boolean`, `Polygon2D.boolean_*`), line/plane/sphere/arc intersection (`intersection2d`/`intersection3d`), broad-phase bounding (`bounding`), ear-clipping triangulation (`triangulation.earcut`), method-level planar projection (`Plane.project_point`/`Face3D.project_point`), a directed-graph network, OBJ/STL mesh interop, and tolerance-bucketed coordinate hashing (`network.coordinates_hash`). The energy-geometry owner composes these primitives, the `dictutil.geometry_dict_to_object` polymorphic decoder, the `Face3D` aperture/shade generators, and the OBJ/STL interop into one substrate owner; it never re-implements the value-object algebra, the 2D boolean, the earcut triangulation, or the intersection primitives this package already owns, and routes the heavy watertight-CSG, mesh repair, and registration concerns to `manifold3d`/`trimesh` rather than growing them here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ladybug-geometry`
- package: `ladybug-geometry`
- import: `import ladybug_geometry`
- owner: `geometry`
- rail: energy / geometry-substrate
- consumer: `.planning/energy/model.md` (`Face3D`/`Polyface3D` BIM-to-BEM lifting) + `.planning/energy/district.md` (footprint rings)
- installed: `1.33.11`
- license: AGPL-3.0 (strong copyleft; the energy band runs as an out-of-process companion rail graduating HBJSON/result evidence across the wire, never linked into a distributed host binary)
- abi: pure-Python (`py2.py3-none-any`, purelib; no native extension)
- entry points: none (library only; no console script)
- capability: immutable 2D/3D point/vector/ray/line/arc/polyline/polygon/mesh and 3D plane/face/polyface/mesh/sphere/cone/cylinder value objects; uniform `type`-tagged dict and array serialization; full transform algebra; pure-Python 2D boolean union/intersect/difference/xor/split (n-ary); line/plane/sphere/arc intersection; axis-aligned and oriented bounding plus overlap broad-phase; ear-clipping triangulation; planar projection; directed-graph network traversal; OBJ/STL mesh read/write; and tolerance-bucketed coordinate hashing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: planar primitives (`ladybug_geometry.geometry2d`)
- rail: energy / geometry-substrate

All share the value-object contract: `from_dict`/`to_dict` (carrying a `type` discriminator), `from_array`/`to_array`, `duplicate`, and the `move`/`rotate`/`reflect`/`scale` transform algebra. Construction is positional value objects, never a parallel reader family.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                           |
| :-----: | :------------------------ | :------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `Point2D` / `Vector2D`    | primitive     | planar point/direction; `dot`/`cross`/`determinant`/`angle`/`normalize`                |
|  [02]   | `Ray2D` / `LineSegment2D` | 1d            | bounded/half-line with `intersect_line_ray`, `closest_point`                           |
|  [03]   | `Arc2D`                   | 1d curve      | circular arc; `subdivide`, `point_at`, `intersect_line_ray`                            |
|  [04]   | `Polyline2D`              | 1d chain      | open/closed chain; `offset`, `to_polygon`, `join_segments`, `remove_colinear_vertices` |
|  [05]   | `Polygon2D`               | 2d region     | boolean algebra, `offset`, point/region relationship, `pole_of_inaccessibility`        |
|  [06]   | `Mesh2D`                  | 2d mesh       | face/vertex mesh with grid/triangulated constructors and color algebra                 |

[PUBLIC_TYPE_SCOPE]: solid primitives (`ladybug_geometry.geometry3d`)
- rail: energy / geometry-substrate

`Face3D` is the workhorse honeybee `Face`/`Aperture` wrap; `Polyface3D` is the lightweight closed-solid honeybee `Room` wrap; `Mesh3D` is the analysis/visualization grid.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]   | [CAPABILITY]                                                                     |
| :-----: | :----------------------------- | :-------------- | :------------------------------------------------------------------------------- |
|  [01]   | `Point3D` / `Vector3D`         | primitive       | 3D point/direction; `cross`/`dot`/`angle`/`project`/`distance_to_plane`          |
|  [02]   | `Ray3D` / `LineSegment3D`      | 1d              | bounded/half-line; `intersect_plane`, `closest_point`                            |
|  [03]   | `Arc3D`                        | 1d curve        | planar arc in 3D; `from_start_mid_end`, `to_polyline`, `subdivide`               |
|  [04]   | `Plane`                        | frame           | `xy_to_xyz`/`xyz_to_xy`, `intersect_plane`, `altitude`/`azimuth`/`tilt`          |
|  [05]   | `Polyline3D`                   | 1d chain        | 3D chain; `from_polyline2d`, `to_polyline2d`, `remove_colinear_vertices`         |
|  [06]   | `Face3D`                       | 2d region in 3D | aperture/shade/split/coplanar-boolean, `mesh_grid`, `triangulated_mesh3d`        |
|  [07]   | `Polyface3D`                   | closed solid    | `is_solid`, `volume`, `naked_edges`, `from_box`/`from_faces`/`from_offset_face`  |
|  [08]   | `Mesh3D`                       | 3d mesh         | `from_obj`/`from_stl`/`from_mesh2d`, normals, `height_field_mesh`, `offset_mesh` |
|  [09]   | `Sphere` / `Cone` / `Cylinder` | analytic solid  | `area`/`volume`; `Sphere.intersect_line_ray`/`intersect_plane` (Sphere only)     |

[PUBLIC_TYPE_SCOPE]: network and interop (`network`, `interop`)
- rail: energy / geometry-substrate

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                                                         |
| :-----: | :---------------------------------------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `network.DirectedGraphNetwork` / `network.Node` | graph         | directed adjacency over geometry nodes for street/skeleton traversal |
|  [02]   | `interop.obj.OBJ`                               | mesh codec    | vertex texture/normal/color + material structure                     |
|  [03]   | `interop.stl.STL`                               | mesh codec    | `face_vertices`/`face_normals`, binary/ASCII STL                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: polymorphic deserialization and shared value-object contract
- rail: energy / geometry-substrate

`geometry_dict_to_object` is the one decode dispatcher: it reads the `type` key of any geometry dict and reconstructs the matching object, so the boundary never branches on a `from_<type>` ladder. Every primitive carries the symmetric `to_dict` (emitting the `type` tag), the `from_array`/`to_array` coordinate form, and the transform algebra.

| [INDEX] | [SURFACE]                                                                   | [CALL_SHAPE]       | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------------- | :----------------- | :----------------------------------- |
|  [01]   | `dictutil.geometry_dict_to_object(ladybug_geom_dict, raise_exception=True)` | `type`-tagged dict | reconstruct any primitive by `type`  |
|  [02]   | `<Primitive>.to_dict()` / `<Primitive>.from_dict(data)`                     | object / dict      | round-trip; `to_dict` tags `type`    |
|  [03]   | `<Primitive>.to_array()` / `<Primitive>.from_array(arr)`                    | object / tuple     | coordinate-only numeric form         |
|  [04]   | `<Primitive>.move(v)` / `.rotate(axis, angle, origin)`                      | transform args     | translate / axis-rotate (new object) |
|  [05]   | `<Primitive>.rotate_xy(angle, origin)` / `.reflect(normal, origin)`         | transform args     | xy-rotate / reflect (new object)     |
|  [06]   | `<Primitive>.scale(factor, origin)`                                         | transform args     | scale about origin (new object)      |
|  [07]   | `<Primitive>.duplicate()` / `.is_equivalent(other, tolerance)`              | none / peer + tol  | copy; tolerance-aware equivalence    |

[ENTRYPOINT_SCOPE]: `Face3D` aperture, shade, contour, and split operations
- rail: energy / geometry-substrate

`Face3D` carries the energy-model geometry generators honeybee composes: ratio/dimension subdivision mints apertures, the contour-fin family mints shading louvers, and the split family carves a face against lines/polylines/planes/holes. Operations return new `Face3D` lists, never mutate. Surfaces elide the `Face3D.` prefix.

| [INDEX] | [SURFACE]                                                                | [CALL_SHAPE]    | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------------------------- | :-------------- | :-------------------------------------- |
|  [01]   | `sub_faces_by_ratio(ratio)` / `sub_faces_by_ratio_rectangle(ratio, tol)` | window-to-wall  | aperture sub-faces by area ratio        |
|  [02]   | `sub_faces_by_ratio_gridded(...)`                                        | window-to-wall  | gridded ratio apertures                 |
|  [03]   | `sub_faces_by_ratio_sub_rectangle(...)`                                  | window-to-wall  | sub-rectangle ratio apertures           |
|  [04]   | `sub_faces_by_dimension_rectangle(sub_rect_height, sub_rect_width, ...)` | physical dims   | apertures by explicit dimension         |
|  [05]   | `contour_by_distance_between(...)` / `contour_by_number(...)`            | spacing/count   | contour lines by spacing / count        |
|  [06]   | `contour_fins_by_distance_between(...)` / `contour_fins_by_number(...)`  | spacing/count   | shading fins (louvers) by spacing/count |
|  [07]   | `split_with_line(line, tol)` / `split_with_lines(...)`                   | cutter geometry | carve a face by line list               |
|  [08]   | `split_with_polyline(...)`                                               | cutter geometry | carve a face by polyline                |
|  [09]   | `split_through_holes()` / `intersect_plane(plane)`                       | cutter geometry | carve through holes / plane-intersect   |
|  [10]   | `coplanar_difference(faces, tol, ...)`                                   | coplanar faces  | coplanar difference                     |
|  [11]   | `coplanar_split(face1, face2, ...)`                                      | coplanar faces  | coplanar split                          |
|  [12]   | `coplanar_union(...)` / `coplanar_union_all(...)`                        | coplanar faces  | coplanar union / union-all              |
|  [13]   | `join_coplanar_faces(...)`                                               | coplanar faces  | join coplanar faces                     |
|  [14]   | `mesh_grid(x_dim, y_dim=None, offset=None, ...)`                         | grid dims       | analysis-grid `Mesh3D` over the face    |
|  [15]   | `from_extrusion(line_segment, extrusion_vector)` / `from_rectangle(...)` | seed geometry   | extrusion / rectangle `Face3D` seed     |
|  [16]   | `from_punched_geometry(base, holes)` / `from_regular_polygon(...)`       | seed geometry   | punched / regular-polygon seed          |

[ENTRYPOINT_SCOPE]: 2D boolean, intersection, bounding, triangulation, projection
- rail: energy / geometry-substrate

The pure-Python computational kernels. `boolean.*` and the `Polygon2D.boolean_*` mirrors are the package's own watertight 2D boolean (n-ary `_all` forms fold a sequence); `intersection2d`/`intersection3d` are free functions over raw primitives; `bounding` is the broad-phase; `triangulation.earcut` is the mapbox ear-clipping algorithm feeding every `triangulated_mesh*` property.

| [INDEX] | [SURFACE]                                                              | [CALL_SHAPE]        | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------- | :------------------ | :------------------------------------ |
|  [01]   | `boolean.union(poly1, poly2, tolerance)` / `intersect(...)`            | polygon pair        | low-level 2D boolean on point lists   |
|  [02]   | `boolean.difference(...)` / `split(...)` / `xor(...)`                  | polygon pair        | low-level 2D difference / split / xor |
|  [03]   | `boolean.union_all(polygons, tolerance)` / `intersect_all(...)`        | polygon sequence    | n-ary fold of the boolean             |
|  [04]   | `Polygon2D.boolean_union(polygon, tolerance)` / `boolean_intersect`    | peer polygon        | object boolean -> `Polygon2D`         |
|  [05]   | `boolean_difference` / `boolean_xor`                                   | peer polygon        | object boolean -> `Polygon2D`         |
|  [06]   | `Polygon2D.boolean_union_all(polygons, tolerance)`                     | sequence            | n-ary static boolean (static)         |
|  [07]   | `boolean_intersect_all(...)` / `boolean_split(...)` (static)           | sequence            | n-ary static boolean over `Polygon2D` |
|  [08]   | `intersection3d.intersect_line3d_plane(line, plane)`                   | primitives          | 3D line-plane intersection            |
|  [09]   | `intersect_plane_plane(...)` / `intersect_line3d_sphere(...)`          | primitives          | 3D plane-plane / line-sphere          |
|  [10]   | `intersect_plane_sphere(...)`                                          | primitives          | 3D plane-sphere intersection          |
|  [11]   | `intersection2d.intersect_line2d(...)` / `intersect_line2d_arc2d(...)` | primitives          | 2D line / line-arc intersection       |
|  [12]   | `closest_point2d_on_line2d(...)`                                       | primitives          | 2D closest-point on a line            |
|  [13]   | `bounding.bounding_box(geometries, axis_angle=0)`                      | geometry seq        | axis-aligned bounding box             |
|  [14]   | `bounding.bounding_rectangle(...)`                                     | geometry seq        | oriented bounding rectangle           |
|  [15]   | `bounding.bounding_domain_x/y/z(...)`                                  | geometry seq        | per-axis extent domains               |
|  [16]   | `Polyface3D.overlapping_bounding_boxes(pf1, pf2, tol)`                 | geometry pair       | AABB overlap broad-phase (3D)         |
|  [17]   | `Polygon2D.overlapping_bounding_rect(p1, p2, tol)`                     | geometry pair       | AABB overlap broad-phase (2D)         |
|  [18]   | `triangulation.earcut(data, hole_indices=None, dim=2)`                 | flat coords + holes | ear-clipping triangulation (earcut)   |
|  [19]   | `Plane.project_point(point, projection_direction=None)`                | point + plane       | project point onto a plane            |
|  [20]   | `Face3D.project_point(point)` / `Point3D.project(normal, origin)`      | point/vec + plane   | project point onto face / plane       |
|  [21]   | `Vector3D.project(normal)`                                             | vector + plane      | project vector onto a plane           |

[ENTRYPOINT_SCOPE]: mesh interop and content hashing (`interop`, `util`)
- rail: energy / geometry-substrate

`Mesh3D.to_obj(folder, name, include_colors=True, include_normals=False, triangulate_quads=False, include_mtl=False)` and the `interop.obj.OBJ(vertices, faces, vertex_texture_map=None, vertex_normals=None, vertex_colors=None, material_structure=None)` constructor carry the full flag/field roster.

| [INDEX] | [SURFACE]                                                         | [CALL_SHAPE] | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------- | :----------- | :----------------------------------------- |
|  [01]   | `Mesh3D.from_obj(file_path)` / `Mesh3D.to_obj(folder, name, ...)` | path         | OBJ read/write off a `Mesh3D`              |
|  [02]   | `Mesh3D.from_stl(file_path)` / `Mesh3D.to_stl(folder, name)`      | path         | STL read/write off a `Mesh3D`              |
|  [03]   | `interop.obj.OBJ(...)` / `OBJ.from_mesh3ds(meshes, ...)`          | mesh(es)     | multi-mesh OBJ (materials/textures/colors) |
|  [04]   | `OBJ.to_file(folder, name, ...)`                                  | mesh(es)     | write multi-mesh OBJ + MTL                 |
|  [05]   | `interop.stl.STL(face_vertices, face_normals, name='polyhedron')` | mesh         | STL value object                           |
|  [06]   | `STL.from_mesh3d(mesh)` / `STL.to_file(folder, name)`             | mesh         | STL build / write                          |
|  [07]   | `network.coordinates_hash(point, tolerance)`                      | point + tol  | coordinate hash for vertex dedup / node id |

## [04]-[INTEGRATION_PATTERNS]

[STACK_BOUNDARY_DISCRIMINATED]: `type`-tagged dict <-> `msgspec`/`pydantic` discriminated boundary
- Every primitive's `to_dict()` emits a `type` field (`"Point3D"`, `"Face3D"`, `"Polyface3D"`, ...) and `dictutil.geometry_dict_to_object` is the symmetric decode dispatcher keyed on that field. The energy-geometry owner maps this onto a `msgspec.Struct` tagged union (`tag_field="type"`) or a `pydantic` discriminated union so geometry crosses the gRPC/HBJSON boundary as one polymorphic payload — the decode is `geometry_dict_to_object(payload)`, never a `from_<type>` branch ladder. This is the identical pattern honeybee/dragonfly use to nest geometry inside their model dicts, so the substrate decoder and the model decoder share one discriminator.

[STACK_NUMPY_MESH]: `Mesh3D`/`Face3D` <-> `numpy` arrays and the mesh owner
- `Mesh3D.vertices`/`faces`, `Face3D.triangulated_mesh3d`, and every `to_array()` return nested coordinate tuples that lift directly to `numpy` arrays for the analysis-grid and mesh owners. `Face3D.mesh_grid(x_dim, y_dim)` produces the sensor `Mesh3D` whose `face_centroids` become the radiance/comfort sensor points; the owner stacks `numpy` vectorization on top of the centroid array rather than iterating Python objects. The lightweight `Mesh3D` is the energy-side grid; the heavy conditioning (repair, decimation, proximity) routes to `trimesh` via `Mesh3D.to_obj` -> `trimesh.load`.

[STACK_HONEYBEE_SUBSTRATE]: `Face3D`/`Polyface3D` <-> honeybee model graph
- This package is the geometric base the energy model is authored on: honeybee `Room.from_polyface3d`, `Face`, `Aperture`, `Shade`, and dragonfly `Room2D` all wrap these primitives. The dragonfly urban constructors consume them directly — `Room2D.from_polygon(identifier, polygon, ...)`/`from_vertices(...)` take a `Polygon2D`/`Point2D`+`Face3D`, and `Building.from_footprint(identifier, footprint, ...)` extrudes a `Face3D`/`Polygon2D` footprint into stories (the reciprocal seam in `dragonfly-core.md`). `Face3D.sub_faces_by_ratio*` is exactly how a window-to-wall-ratio aperture set is minted; `contour_fins_by_*` mints louver `Shade`s; `Polyface3D.from_box`/`from_offset_face` seeds a `Room` solid; `is_solid`/`naked_edges` is the closed-volume validity gate honeybee checks before simulation. The energy-geometry owner builds the model by composing these generators, never by re-deriving aperture/shade geometry.

[STACK_HEAVY_CSG_HANDOFF]: pure-Python 2D boolean <-> `manifold3d`/`trimesh`/`shapely`
- `boolean.*` and `Polygon2D.boolean_*` are the package's own pure-Python planar boolean, sufficient for coplanar `Face3D` operations and aperture carving; the owner uses them in-place for energy-model geometry. Robust watertight 3D mesh CSG, mesh repair, and large planar-set operations route out to the heavier siblings — `Polyface3D`/`Mesh3D` graduate via `to_obj` into `trimesh`/`manifold3d` for exact boolean, and `Polygon2D` graduates into `shapely` for industrial-scale 2D ops. The boundary is deliberate: ladybug-geometry stays pure-Python and dependency-free; the heavy kernels stay in their owners.

[STACK_SOLAR_CONSUMER]: `ladybug` Sunpath <-> this substrate
- `ladybug.sunpath.Sun.sun_vector` returns a `ladybug_geometry.geometry3d.pointvector.Vector3D`, `Sun.position_3d()` a `Point3D`, and `Sunpath.day_arc3d`/`hourly_analemma_polyline3d` return `Arc3D`/`Polyline3D`. The climate owner (`ladybug-core`) is therefore a downstream consumer of this substrate — solar geometry, compass diagrams, and view spheres are all expressed in these primitives — so the energy band shares one geometry vocabulary from weather through model to simulation.

[STACK_CONTENT_IDENTITY]: `coordinates_hash` <-> content-keyed memoization
- `network.coordinates_hash(point, tolerance)` buckets coordinates to a tolerance before hashing, giving a geometry-stable key for vertex dedup and the runtime `ContentIdentity` seed (the same content-key pattern the C# `XxHash128` rail uses): tolerance-equal geometry shares a cache key so a memoized boolean/triangulation/projection result is reused without re-running the op.

## [05]-[IMPLEMENTATION_LAW]

[ENERGY_GEOMETRY_SUBSTRATE]:
- import: `import ladybug_geometry` (and the `geometry2d`/`geometry3d` submodules) at boundary scope only; module-level import is banned by the manifest import policy.
- value-object axis: every primitive is an immutable value object with a uniform `from_dict`/`to_dict` (`type`-tagged), `from_array`/`to_array`, `duplicate`, `is_equivalent(other, tolerance)`, and the `move`/`rotate`/`rotate_xy`/`reflect`/`scale` transform algebra returning a new object. Transforms never mutate; the operation kind is the named method, never a parallel transformed-subclass.
- serialization axis: `dictutil.geometry_dict_to_object` is the single polymorphic decoder keyed on the dict `type` field; the boundary decodes through it, never a `from_<type>` ladder. The owner maps the `type` field onto a `msgspec`/`pydantic` discriminated union so the substrate and the honeybee model share one discriminator.
- geometry axis: `Face3D` owns aperture (`sub_faces_by_ratio*`/`sub_faces_by_dimension_rectangle`), shade (`contour_fins_by_*`), split (`split_with_*`/`split_through_holes`), and coplanar-boolean (`coplanar_difference`/`coplanar_union`) generation; `Polyface3D` owns the closed-solid model with `is_solid`/`volume`/`naked_edges` validity; `Mesh3D` owns the analysis grid with `mesh_grid`-sourced centroids. These are the named generators honeybee composes, never re-derived.
- kernel axis: 2D boolean (`boolean.*`, `Polygon2D.boolean_*`, n-ary `_all` folds), intersection (`intersection2d`/`intersection3d` free functions), bounding broad-phase (`bounding.*` extents, `Polyface3D.overlapping_bounding_boxes`/`Polygon2D.overlapping_bounding_rect` overlap), and `triangulation.earcut` are the package's own pure-Python kernels feeding the `triangulated_mesh*` properties; the owner uses them in place and routes only the heavy watertight-CSG/repair concerns to `manifold3d`/`trimesh`.
- interop axis: `Mesh3D.to_obj`/`from_obj`/`to_stl`/`from_stl` and the `interop.OBJ`/`STL` value objects are the lightweight pure-Python mesh codec; file-level mesh exchange at scale and the GLB encode route to the data `MeshPayload` codec and `trimesh`/`meshio`, never grown here.
- evidence: each geometry op captures the primitive `type`, vertex/face count, and (for solids) `is_solid`/`volume`/`naked_edges` validity; each boolean/split captures the operation, operand counts, and result count; each triangulation captures the input vertex count and triangle count as a geometry receipt.
- boundary: ladybug-geometry owns the pure-Python planar/solid primitive algebra, its own 2D boolean/intersection/bounding/triangulation kernels and point-projection methods, the directed-graph network, OBJ/STL interop, and coordinate hashing. Robust watertight mesh CSG and repair route to `manifold3d`/`trimesh`; industrial planar operations to `shapely`; NURBS/BREP to `cadquery-ocp`/`rhino3dm`; mesh-file encode to the data `MeshPayload` codec; the honeybee/dragonfly/ladybug model semantics to those packages, which consume this substrate.

## [06]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ladybug-geometry`
- Owns: immutable 2D/3D primitive value objects with uniform `type`-tagged dict/array serialization and transform algebra; the `geometry_dict_to_object` polymorphic decoder; `Face3D` aperture/shade/split/coplanar-boolean generation; `Polyface3D` closed-solid modeling; `Mesh3D` analysis grids; pure-Python 2D boolean, intersection, bounding, ear-clipping triangulation, and projection; the directed-graph network; OBJ/STL interop; and tolerance-bucketed coordinate hashing
- Accept: the geometric substrate of the honeybee/dragonfly/ladybug energy band feeding the energy-geometry owner; the `type` discriminator shared with the honeybee model decoder; the analysis-grid centroids feeding the radiance/comfort sensor rails; the solar geometry `ladybug-core` produces in these primitives
- Reject: wrapper-renames of the value objects or `geometry_dict_to_object`; a hand-rolled vector/boolean/intersection/triangulation/bounding kernel where this package already owns the pure-Python version; a `from_<type>` decode ladder over the `type` discriminator; a robust watertight-CSG/repair re-implementation where `manifold3d`/`trimesh` are admitted; a parallel transformed-geometry subclass over the `move`/`rotate`/`scale` methods; identity minting the runtime owns

[CAPTURE_GAP]:
- AGPL-3.0: strong copyleft is the binding admission flag — the geometry substrate rides the out-of-process energy companion rail and graduates HBJSON/result evidence across the wire, never linked into a distributed host binary.
- heavy kernels: robust watertight 3D mesh CSG, mesh repair, decimation, and registration are out of scope and route to `manifold3d`/`trimesh`; industrial-scale planar boolean routes to `shapely`; NURBS/BREP to `cadquery-ocp`/`rhino3dm`; mesh-file encode/GLB to the data `MeshPayload` codec. This catalog is the pure-Python primitive-algebra surface; the honeybee/dragonfly/ladybug model semantics are owned by those packages, which consume this substrate.
