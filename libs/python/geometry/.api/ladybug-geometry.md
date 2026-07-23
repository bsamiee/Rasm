# [PY_GEOMETRY_API_LADYBUG_GEOMETRY]

`ladybug-geometry` mints the immutable pure-Python planar (`geometry2d`) and solid (`geometry3d`) value-object algebra every Ladybug Tools object is authored on — honeybee `Room`/`Face`/`Aperture`/`Shade`, dragonfly `Story`/`Building`, and `ladybug` solar geometry. Each primitive folds through one contract: a `type`-tagged dict round-trip, an array coordinate form, and a `move`/`rotate`/`rotate_xy`/`reflect`/`scale` transform algebra returning new objects.

Beyond the primitives it owns its own pure-Python 2D boolean, intersection, bounding, ear-clipping triangulation, projection, a directed-graph network, and OBJ/STL interop; robust watertight-CSG, mesh repair, and registration route out to `manifold3d`/`trimesh`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ladybug-geometry`
- package: `ladybug-geometry` (AGPL-3.0)
- module: `import ladybug_geometry`
- namespaces: `geometry2d`, `geometry3d`, `boolean`, `intersection2d`, `intersection3d`, `bounding`, `triangulation`, `network`, `interop`, `dictutil`
- owner: `geometry`
- rail: energy / geometry-substrate
- consumer: `.planning/energy/model.md` (`Face3D`/`Polyface3D` BIM-to-BEM lift), `.planning/energy/district.md` (footprint rings)
- abi: pure-Python (`py2.py3-none-any`, purelib)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: planar primitives (`ladybug_geometry.geometry2d`)

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                           |
| :-----: | :------------------------ | :------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `Point2D` / `Vector2D`    | primitive     | planar point/direction; `dot`/`cross`/`determinant`/`angle`/`normalize`                |
|  [02]   | `Ray2D` / `LineSegment2D` | 1d            | bounded/half-line with `intersect_line_ray`, `closest_point`                           |
|  [03]   | `Arc2D`                   | 1d curve      | circular arc; `subdivide`, `point_at`, `intersect_line_ray`                            |
|  [04]   | `Polyline2D`              | 1d chain      | open/closed chain; `offset`, `to_polygon`, `join_segments`, `remove_colinear_vertices` |
|  [05]   | `Polygon2D`               | 2d region     | boolean algebra, `offset`, point/region relationship, `pole_of_inaccessibility`        |
|  [06]   | `Mesh2D`                  | 2d mesh       | face/vertex mesh with grid/triangulated constructors and color algebra                 |

[PUBLIC_TYPE_SCOPE]: solid primitives (`ladybug_geometry.geometry3d`)

`Face3D` is the honeybee `Face`/`Aperture` wrap; `Polyface3D` the closed-solid `Room` wrap; `Mesh3D` the analysis/visualization grid.

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

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                                                         |
| :-----: | :---------------------------------------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `network.DirectedGraphNetwork` / `network.Node` | graph         | directed adjacency over geometry nodes for street/skeleton traversal |
|  [02]   | `interop.obj.OBJ`                               | mesh codec    | vertex texture/normal/color + material structure                     |
|  [03]   | `interop.stl.STL`                               | mesh codec    | `face_vertices`/`face_normals`, binary/ASCII STL                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: polymorphic deserialization and the shared value-object contract

`geometry_dict_to_object` reads the `type` key of any geometry dict and reconstructs the matching object; every primitive carries the symmetric `to_dict` (tagging `type`), the `from_array`/`to_array` coordinate form, and the transform algebra.

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `dictutil.geometry_dict_to_object(ladybug_geom_dict, raise_exception=True)` | static   | reconstruct any primitive by `type`  |
|  [02]   | `<Primitive>.to_dict()` / `<Primitive>.from_dict(data)`                     | instance | round-trip; `to_dict` tags `type`    |
|  [03]   | `<Primitive>.to_array()` / `<Primitive>.from_array(arr)`                    | instance | coordinate-only numeric form         |
|  [04]   | `<Primitive>.move(v)` / `.rotate(axis, angle, origin)`                      | instance | translate / axis-rotate (new object) |
|  [05]   | `<Primitive>.rotate_xy(angle, origin)` / `.reflect(normal, origin)`         | instance | xy-rotate / reflect (new object)     |
|  [06]   | `<Primitive>.scale(factor, origin)`                                         | instance | scale about origin (new object)      |
|  [07]   | `<Primitive>.duplicate()` / `.is_equivalent(other, tolerance)`              | instance | copy; tolerance-aware equivalence    |

[ENTRYPOINT_SCOPE]: `Face3D` aperture, shade, contour, and split generators (prefix elided)

`Face3D` carries the energy-model geometry generators honeybee composes — ratio/dimension subdivision mints apertures, the contour-fin family mints shading louvers, and the split family carves against lines/polylines/planes/holes. Every op returns new `Face3D` lists, never mutates.

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `sub_faces_by_ratio(ratio)` / `sub_faces_by_ratio_rectangle(ratio, tol)` | instance | aperture sub-faces by area ratio        |
|  [02]   | `sub_faces_by_ratio_gridded(...)`                                        | instance | gridded ratio apertures                 |
|  [03]   | `sub_faces_by_ratio_sub_rectangle(...)`                                  | instance | sub-rectangle ratio apertures           |
|  [04]   | `sub_faces_by_dimension_rectangle(sub_rect_height, sub_rect_width, ...)` | instance | apertures by explicit dimension         |
|  [05]   | `contour_by_distance_between(...)` / `contour_by_number(...)`            | instance | contour lines by spacing / count        |
|  [06]   | `contour_fins_by_distance_between(...)` / `contour_fins_by_number(...)`  | instance | shading fins (louvers) by spacing/count |
|  [07]   | `split_with_line(line, tol)` / `split_with_lines(...)`                   | instance | carve a face by line list               |
|  [08]   | `split_with_polyline(...)`                                               | instance | carve a face by polyline                |
|  [09]   | `split_through_holes()` / `intersect_plane(plane)`                       | instance | carve through holes / plane-intersect   |
|  [10]   | `coplanar_difference(faces, tol, ...)`                                   | instance | coplanar difference                     |
|  [11]   | `coplanar_split(face1, face2, ...)`                                      | static   | coplanar split                          |
|  [12]   | `coplanar_union(...)` / `coplanar_union_all(...)`                        | static   | coplanar union / union-all              |
|  [13]   | `join_coplanar_faces(...)`                                               | static   | join coplanar faces                     |
|  [14]   | `mesh_grid(x_dim, y_dim=None, offset=None, ...)`                         | instance | analysis-grid `Mesh3D` over the face    |
|  [15]   | `from_extrusion(line_segment, extrusion_vector)` / `from_rectangle(...)` | factory  | extrusion / rectangle `Face3D` seed     |
|  [16]   | `from_punched_geometry(base, holes)` / `from_regular_polygon(...)`       | factory  | punched / regular-polygon seed          |

[ENTRYPOINT_SCOPE]: 2D boolean, intersection, bounding, triangulation, projection

`boolean.*` and the `Polygon2D.boolean_*` mirrors are the package's own watertight 2D boolean (`_all` forms fold a sequence); `intersection2d`/`intersection3d` are free functions over raw primitives; `triangulation.earcut` is the mapbox ear-clipping feeding every `triangulated_mesh*` property.

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `boolean.union(poly1, poly2, tolerance)` / `intersect(...)`            | static   | low-level 2D boolean on point lists   |
|  [02]   | `boolean.difference(...)` / `split(...)` / `xor(...)`                  | static   | low-level 2D difference / split / xor |
|  [03]   | `boolean.union_all(polygons, tolerance)` / `intersect_all(...)`        | static   | n-ary fold of the boolean             |
|  [04]   | `Polygon2D.boolean_union(polygon, tolerance)` / `boolean_intersect`    | instance | object boolean -> `Polygon2D`         |
|  [05]   | `boolean_difference` / `boolean_xor`                                   | instance | object boolean -> `Polygon2D`         |
|  [06]   | `Polygon2D.boolean_union_all(polygons, tolerance)`                     | static   | n-ary boolean over `Polygon2D`        |
|  [07]   | `boolean_intersect_all(...)` / `boolean_split(...)`                    | static   | n-ary boolean over `Polygon2D`        |
|  [08]   | `intersection3d.intersect_line3d_plane(line, plane)`                   | static   | 3D line-plane intersection            |
|  [09]   | `intersect_plane_plane(...)` / `intersect_line3d_sphere(...)`          | static   | 3D plane-plane / line-sphere          |
|  [10]   | `intersect_plane_sphere(...)`                                          | static   | 3D plane-sphere intersection          |
|  [11]   | `intersection2d.intersect_line2d(...)` / `intersect_line2d_arc2d(...)` | static   | 2D line / line-arc intersection       |
|  [12]   | `closest_point2d_on_line2d(...)`                                       | static   | 2D closest-point on a line            |
|  [13]   | `bounding.bounding_box(geometries, axis_angle=0)`                      | static   | axis-aligned bounding box             |
|  [14]   | `bounding.bounding_rectangle(...)`                                     | static   | oriented bounding rectangle           |
|  [15]   | `bounding.bounding_domain_x/y/z(...)`                                  | static   | per-axis extent domains               |
|  [16]   | `Polyface3D.overlapping_bounding_boxes(pf1, pf2, tol)`                 | static   | AABB overlap broad-phase (3D)         |
|  [17]   | `Polygon2D.overlapping_bounding_rect(p1, p2, tol)`                     | static   | AABB overlap broad-phase (2D)         |
|  [18]   | `triangulation.earcut(data, hole_indices=None, dim=2)`                 | static   | ear-clipping triangulation (earcut)   |
|  [19]   | `Plane.project_point(point, projection_direction=None)`                | instance | project point onto a plane            |
|  [20]   | `Face3D.project_point(point)` / `Point3D.project(normal, origin)`      | instance | project point onto face / plane       |
|  [21]   | `Vector3D.project(normal)`                                             | instance | project vector onto a plane           |

[ENTRYPOINT_SCOPE]: mesh interop and content hashing (`interop`, `network`)

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `Mesh3D.from_obj(file_path)` / `Mesh3D.to_obj(folder, name, ...)` | factory  | OBJ read/write off a `Mesh3D`              |
|  [02]   | `Mesh3D.from_stl(file_path)` / `Mesh3D.to_stl(folder, name)`      | factory  | STL read/write off a `Mesh3D`              |
|  [03]   | `interop.obj.OBJ(...)` / `OBJ.from_mesh3ds(meshes, ...)`          | ctor     | multi-mesh OBJ (materials/textures/colors) |
|  [04]   | `OBJ.to_file(folder, name, ...)`                                  | instance | write multi-mesh OBJ + MTL                 |
|  [05]   | `interop.stl.STL(face_vertices, face_normals, name='polyhedron')` | ctor     | STL value object                           |
|  [06]   | `STL.from_mesh3d(mesh)` / `STL.to_file(folder, name)`             | factory  | STL build / write                          |
|  [07]   | `network.coordinates_hash(point, tolerance)`                      | static   | coordinate hash for vertex dedup / node id |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every primitive is an immutable value object folding through one contract: `type`-tagged `from_dict`/`to_dict`, `from_array`/`to_array`, `duplicate`, `is_equivalent(other, tolerance)`, and the `move`/`rotate`/`rotate_xy`/`reflect`/`scale` transform algebra returning a new object — the operation kind is the named method, never a transformed subclass.
- `dictutil.geometry_dict_to_object` is the one decoder keyed on the dict `type` field; every boundary crossing decodes through it, never a `from_<type>` ladder.
- `Face3D` owns aperture/shade/split/coplanar-boolean generation, `Polyface3D` the closed-solid model gated by `is_solid`/`volume`/`naked_edges`, and `Mesh3D` the `mesh_grid`-sourced analysis grid — the named generators honeybee composes.

[STACKING]:
- `honeybee-core`(`geometry/.api/honeybee-core.md`): `Room.from_polyface3d`, `Face`, `Aperture`, and `Shade` wrap these primitives — `Face3D.sub_faces_by_ratio*` mints the aperture set, `contour_fins_by_*` mints louver `Shade`s, and `Polyface3D.is_solid`/`naked_edges` is the closed-volume gate honeybee checks before simulation.
- `dragonfly-core`(`geometry/.api/dragonfly-core.md`): `Room2D.from_polygon`/`from_vertices` take a `Polygon2D`/`Point2D`+`Face3D` and `Building.from_footprint` extrudes a `Face3D`/`Polygon2D` footprint into stories.
- `ladybug-core`(`geometry/.api/ladybug-core.md`): `Sun.sun_vector`/`position_3d` return `Vector3D`/`Point3D` and `Sunpath.day_arc3d`/`hourly_analemma_polyline3d` return `Arc3D`/`Polyline3D` — solar geometry is expressed in these primitives.
- `msgspec`/`pydantic`(`.api/msgspec.md`, `.api/pydantic.md`): the `type` field maps onto a `msgspec.Struct` tagged union (`tag_field="type"`) or a pydantic discriminated union, so geometry crosses the gRPC/HBJSON boundary as one polymorphic payload the substrate and honeybee model decoders share.
- `numpy`(`.api/numpy.md`): every `to_array()`, `Mesh3D.vertices`/`faces`, and `Face3D.triangulated_mesh3d` lifts to `numpy` arrays; `Face3D.mesh_grid` centroids become the radiance/comfort sensor points the owner vectorizes rather than iterating Python objects.
- `trimesh`/`manifold3d`(`.api/trimesh.md`, `geometry/.api/manifold3d.md`): `Polyface3D`/`Mesh3D` graduate via `to_obj` into `trimesh`/`manifold3d` for robust watertight CSG and repair, and `Polygon2D` graduates into `shapely` for industrial-scale planar ops.
- `geometry` owner: `network.coordinates_hash(point, tolerance)` buckets coordinates to a tolerance-stable key seeding the runtime `ContentIdentity` and memoizing boolean/triangulation/projection results; the energy-geometry owner composes the `Face3D` generators, the `geometry_dict_to_object` decoder, and OBJ/STL interop, never re-deriving the value-object algebra.

[LOCAL_ADMISSION]:
- AGPL-3.0 strong copyleft binds admission: consume the honeybee/ladybug/dragonfly band out-of-process as a companion rail, exchanging HBJSON/result evidence across the wire, its code never linked into a distributed host binary.

[RAIL_LAW]:
- Package: `ladybug-geometry`
- Owns: immutable 2D/3D primitive value objects with uniform `type`-tagged serialization and transform algebra; `geometry_dict_to_object`; `Face3D` aperture/shade/split/coplanar-boolean generation; `Polyface3D` closed-solid modeling; `Mesh3D` analysis grids; pure-Python 2D boolean, intersection, bounding, ear-clipping triangulation, and projection; the directed-graph network; OBJ/STL interop; and tolerance-bucketed coordinate hashing
- Accept: the geometric substrate feeding the energy-geometry owner; the `type` discriminator shared with the honeybee model decoder; the analysis-grid centroids feeding the radiance/comfort sensor rails; the solar geometry `ladybug-core` mints in these primitives
- Reject: wrapper-renames of the value objects or `geometry_dict_to_object`; a hand-rolled vector/boolean/intersection/triangulation/bounding kernel this package already owns; a `from_<type>` decode ladder over the `type` discriminator; a watertight-CSG/repair re-implementation where `manifold3d`/`trimesh` are admitted; a parallel transformed-geometry subclass over the transform methods; identity minting the runtime owns
