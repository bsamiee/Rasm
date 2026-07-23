# [PY_GEOMETRY_API_COMPAS]

`compas` owns the computational-geometry and datastructure surface of the geometry algebra rail: the `compas.geometry` primitive/shape/NURBS/BREP algebra, the `compas.datastructures` half-edge mesh/graph/volmesh family, COMPAS-JSON `Data` serialization, file exchange, and the out-of-process `compas.rpc.Proxy` solver bridge. It is the spine of the COMPAS form-finding band — `compas_dr` and `compas_tna` ride its `Mesh` and its `json_dumps`/`json_loads` round-trip. It never re-implements vector algebra, best-fit, boolean operations, or the form-finding solvers its companions own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `compas`
- package: `compas` (MIT)
- module: `compas`; namespaces `compas.geometry`, `compas.datastructures`, `compas.files`, `compas.rpc`
- owner: `geometry`
- rail: algebra
- entry points: none (library only)
- capability: geometric primitives, shapes, NURBS curves/surfaces, BREP, transformations, vector and `_numpy`-accelerated best-fit/hull algebra, mesh/graph/network/volmesh datastructures, boolean operations, OBJ/PLY/STL/OFF/GLTF/DXF read/write, COMPAS-JSON `Data` serialization, the `rpc.Proxy` out-of-process CPython solver bridge, and host-modality detection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry primitives and shapes (`compas.geometry`)

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :------------------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `Point` / `Vector`                     | primitive     | point and direction value objects              |
|  [02]   | `Line` / `Polyline` / `Polygon`        | primitive     | linear and polygonal geometry                  |
|  [03]   | `Plane` / `Frame`                      | primitive     | infinite plane and oriented coordinate frame   |
|  [04]   | `Circle` / `Ellipse` / `Arc`           | primitive     | conic curve primitives                         |
|  [05]   | `Bezier`                               | primitive     | parametric Bezier curve                        |
|  [06]   | `Quaternion`                           | primitive     | rotation quaternion                            |
|  [07]   | `Pointcloud` / `KDTree`                | primitive     | point set and nearest-neighbor index           |
|  [08]   | `Box` / `Sphere` / `Cylinder`          | shape         | solid `Cone`/`Capsule`/`Torus`/`Polyhedron`    |
|  [09]   | `Curve` / `NurbsCurve`                 | curve         | generic and NURBS curves                       |
|  [10]   | `Surface` / `NurbsSurface`             | surface       | generic and NURBS surfaces                     |
|  [11]   | `PlanarSurface` / `CylindricalSurface` | surface       | analytic `Conical`/`Spherical`/`Toroidal`      |
|  [12]   | `Brep` / `BrepFace` / `BrepEdge`       | brep          | boundary-representation `Loop`/`Vertex`/`Trim` |
|  [13]   | `Transformation` / `Translation`       | transform     | affine transform matrices                      |
|  [14]   | `Rotation` / `Scale` / `Shear`         | transform     | `Reflection`/`Projection` transforms           |

[PUBLIC_TYPE_SCOPE]: datastructures (`compas.datastructures`)

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                                 |
| :-----: | :---------------------- | :-------------- | :------------------------------------------- |
|  [01]   | `Mesh`                  | mesh            | half-edge mesh with topology and geometry    |
|  [02]   | `Graph`                 | graph           | node/edge graph datastructure                |
|  [03]   | `Network`               | graph           | spatial node/edge network                    |
|  [04]   | `VolMesh`               | volumetric mesh | volumetric half-face mesh                    |
|  [05]   | `CellNetwork`           | cell complex    | cell/face/edge network                       |
|  [06]   | `Tree` / `TreeNode`     | tree            | hierarchical tree datastructure              |
|  [07]   | `Assembly` / `Part`     | assembly        | part assembly with features                  |
|  [08]   | `HashTree` / `HashNode` | hash tree       | content-addressed hierarchy                  |
|  [09]   | `Datastructure`         | base            | shared datastructure data/serialization base |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: algebra and best-fit functions (`compas.geometry`)

`_numpy`-suffixed variants are the SciPy-accelerated heavy band; every function takes and returns plain coordinate sequences.

| [INDEX] | [SURFACE]                           | [SHAPE]             | [CAPABILITY]                                                            |
| :-----: | :---------------------------------- | :------------------ | :---------------------------------------------------------------------- |
|  [01]   | `add_vectors` / `cross_vectors`     | vector pairs        | vector addition and cross product                                       |
|  [02]   | `dot_vectors` / `normalize_vector`  | vectors             | dot product and unit normalization                                      |
|  [03]   | `angle_vectors`                     | two vectors         | angle between vectors                                                   |
|  [04]   | `area_polygon` / `area_triangle`    | polygon/points      | planar area                                                             |
|  [05]   | `barycentric_coordinates`           | point plus triangle | barycentric coordinates                                                 |
|  [06]   | `bestfit_plane`                     | points              | least-squares plane fit; returns `(point, normal)`                      |
|  [07]   | `centroid_points`                   | points              | point-set centroid; returns a plain 3-list                              |
|  [08]   | `bestfit_frame_numpy`               | points              | best-fit frame; returns `(origin, xaxis, yaxis)` triple (not a `Frame`) |
|  [09]   | `bestfit_circle_numpy`              | points              | best-fit circle                                                         |
|  [10]   | `bestfit_sphere_numpy`              | points              | best-fit sphere                                                         |
|  [11]   | `bbox` / `bbox_numpy`               | points              | axis-aligned bounding box                                               |
|  [12]   | `oriented_bounding_box_numpy`       | points              | minimal OBB; returns `list[[float, float, float]]` 8 corner points      |
|  [13]   | `convex_hull` / `convex_hull_numpy` | points              | convex hull; `_numpy` returns `(vertex-index, face-index)` int arrays   |
|  [14]   | `boolean_union_mesh_mesh`           | two meshes          | mesh boolean union                                                      |
|  [15]   | `boolean_difference_mesh_mesh`      | two meshes          | mesh boolean difference                                                 |
|  [16]   | `boolean_intersection_mesh_mesh`    | two meshes          | mesh boolean intersection                                               |
|  [17]   | `intersection_line_line`            | two lines           | line-line intersection point                                            |
|  [18]   | `closest_point_on_line`             | point plus line     | nearest point projection                                                |

[ENTRYPOINT_SCOPE]: transform constructors (`compas.geometry`)

Transform matrices are pure-Python value objects composed off a fitted frame or plane, so a transform is a numerical op on a coordinate set.

| [INDEX] | [SURFACE]                                  | [SHAPE] | [CAPABILITY]                                        |
| :-----: | :----------------------------------------- | :------ | :-------------------------------------------------- |
|  [01]   | `Frame.worldXY()`                          | factory | the world origin frame (transform source frame)     |
|  [02]   | `Transformation.from_frame_to_frame(a, b)` | factory | rigid frame-to-frame map                            |
|  [03]   | `Translation.from_vector(vector)`          | factory | pure translation off a 3-vector/centroid            |
|  [04]   | `Scale.from_factors(factors, frame=None)`  | factory | anisotropic scaling about a frame                   |
|  [05]   | `Projection.from_plane(plane)`             | factory | orthographic projection onto a plane/best-fit plane |

[ENTRYPOINT_SCOPE]: datastructure construction and accessors (`compas.datastructures`)

Construction routes through `from_*` classmethods, never positional constructors; the datastructure algebra verbs read through these accessors.

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :---------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `Network.from_nodes_and_edges(nodes, edges)`    | factory  | build a `Network` from adjacency                       |
|  [02]   | `Mesh.from_json(string)` / `from_jsonstring`    | factory  | reconstruct a `Mesh` from a serialized handle          |
|  [03]   | `Mesh.dual()` / `Mesh.subdivide(scheme=...)`    | instance | dual mesh, subdivided mesh                             |
|  [04]   | `Mesh.vertices_attributes(...)`                 | instance | get-or-set across keys; SET leg is asymmetric          |
|  [05]   | `Mesh.vertex_attributes(...)`                   | instance | single-vertex get-or-set; genuine per-`key` row-setter |
|  [06]   | `Mesh.number_of_edges()` / `number_of_vertices` | instance | topology counts (edge/vertex census)                   |
|  [07]   | `VolMesh.dual()`                                | instance | volumetric dual cell complex                           |
|  [08]   | `Assembly.graph`                                | property | the part-adjacency `Graph` of an assembly              |
|  [09]   | `NurbsSurface.to_mesh(nu=..., nv=...)`          | instance | tessellate a NURBS surface to a `Mesh`                 |

- `Mesh.vertices_attributes(names, values, keys)`: its SET leg hands the whole `values` object to every key, zipping `names` against its outer rows, so a per-vertex row-write drops every row past the first — write per-vertex rows through `for key, row in zip(keys, rows): vertex_attributes(key, names, row)` over the read's `keys` ordering.
- `bestfit_frame_numpy`: a `Frame`-consuming row wraps the returned coordinate triple as `Frame(*triple)`; `Frame.__init__(point, xaxis, yaxis)` is positional over auto-orthonormalized plain lists.

[ENTRYPOINT_SCOPE]: serialization, construction, and host probes

| [INDEX] | [SURFACE]                      | [SHAPE] | [CAPABILITY]                                                              |
| :-----: | :----------------------------- | :------ | :------------------------------------------------------------------------ |
|  [01]   | `compas.json_dump`             | static  | write COMPAS JSON to a file                                               |
|  [02]   | `compas.json_dumps`            | static  | serialize any `Data` subclass to a COMPAS-JSON string (graduation handle) |
|  [03]   | `compas.json_dumpz`            | static  | write compressed COMPAS JSON                                              |
|  [04]   | `compas.json_load`             | static  | read COMPAS JSON from a file                                              |
|  [05]   | `compas.json_loads`            | static  | parse a COMPAS-JSON string back to its `Data` instance                    |
|  [06]   | `Mesh.from_vertices_and_faces` | factory | construct a mesh from arrays                                              |
|  [07]   | `Mesh.from_obj` / `from_ply`   | factory | construct from `OBJ`/`PLY`/`STL`/`OFF`                                    |
|  [08]   | `Mesh.from_polyhedron`         | factory | construct a platonic mesh                                                 |
|  [09]   | `compas.is_rhino`              | static  | detect Rhino host                                                         |
|  [10]   | `compas.is_grasshopper`        | static  | detect Grasshopper host                                                   |
|  [11]   | `compas.is_blender`            | static  | detect Blender host                                                       |
|  [12]   | `compas.is_ironpython`         | static  | detect IronPython runtime                                                 |

[ENTRYPOINT_SCOPE]: out-of-process solver bridge (`compas.rpc`)

`Proxy` runs a CPython server in a separate process and resolves remote callables by attribute access. Construction is eager and lifecycle-bearing: `Proxy.__init__` reconnects to a running localhost server on `url`/`port` or spawns one through the blocking `start_server()` (Popen and a ping loop up to `max_conn_attempts`), owning teardown through `__exit__`. Attribute access resolves a remote callable and never starts a server; `package` namespaces the dotted name.

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `Proxy(package=None, python=None, port=1753, ...)` | ctor     | eager reconnect-or-spawn; owns teardown              |
|  [02]   | `proxy.<name>(*args, **kwargs)`                    | dynamic  | `__getattr__` resolves a remote callable; no spawn   |
|  [03]   | `Proxy.__enter__()` / `Proxy.__exit__(*args)`      | instance | sync context-manager; ownership-aware exit teardown  |
|  [04]   | `Proxy.restart_server()` / `stop_server()`         | instance | `restart_server` = `stop_server` then `start_server` |
|  [05]   | `compas.rpc.RPCServerError` / `RPCClientError`     | class    | fault surface; a timeout raises `RPCServerError`     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- primitives, shapes, curves, surfaces, and BREP are pure-Python value objects; the `_numpy`-suffixed functions (`bestfit_frame_numpy`, `bbox_numpy`, `convex_hull_numpy`, `oriented_bounding_box_numpy`) are SciPy-accelerated variants over the same coordinate inputs, never a parallel hierarchy.
- transform constructors compose a matrix off a fitted frame or plane, so a transform is a numerical op on a coordinate set rather than a second concern.
- `json_dump`/`json_dumps`/`json_dumpz` and the `json_load*` pair are one `Data` serializer family backed by `DataEncoder`/`DataDecoder`; every `Data` subclass round-trips through them, and `DataEncoder.default` coerces raw numpy — `ndarray -> tolist`, int/float/bool scalars to builtins — so a non-`Data` `_numpy` return serializes with no caller `.tolist()` and no output projector.
- `Proxy` lifecycle is eager: construction reconnects-or-spawns the localhost server, so the whole proxy scope — construction, callable resolution, and teardown — is the blocking surface, never a per-call lazy bring-up.

[STACKING]:
- `compas-dr`(`.api/compas-dr.md`): the compas `Mesh` feeds `InputData.from_mesh`; `InputData` and every `Constraint` are `compas.data.Data` subclasses round-tripping through `json_dumps`/`json_loads`, and `dr_numpy` resolves out of process through `Proxy`.
- `compas-tna`(`.api/compas-tna.md`): `FormDiagram` extends `compas.datastructures.Mesh` and builds through `FormDiagram.from_mesh`; diagrams round-trip the same `Data` handle, and `horizontal_numpy`/`vertical_from_zmax` resolve through `Proxy`.
- `trimesh`(`.api/trimesh.md`): mesh exchange over vertices/faces arrays carries the `manifold3d`-backed boolean and repair paths past compas's `boolean_*_mesh_mesh` rows.
- `topologicpy`(`.api/topologicpy.md`): the non-manifold-topology sibling composed beside the compas datastructures, never folded into their rows.
- `meshio`(`.api/meshio.md`): the raw mesh-file decode/encode seam — compas returns COMPAS-JSON handles across the wire rather than writing a mesh file.
- `graph/algebra.md`: `ComputationalGeometry` folds geometry primitives, `_numpy` best-fit/hull, transform constructors, and datastructure verbs into `NumericalOp`/`DatastructureOp` tables, and offloads the whole `Proxy` scope on the runtime lane thread band under a `RetryClass.RPC` cold-start retry.

[LOCAL_ADMISSION]:
- compas is the admitted computational-geometry and datastructure spine for the geometry branch; the `compas_dr`/`compas_tna` form-finding companions and the `_numpy` heavy band compose against it rather than a parallel surface.

[RAIL_LAW]:
- Package: `compas`
- Owns: geometric primitives/shapes/transforms, NURBS and BREP, vector and best-fit algebra, mesh/graph/volmesh datastructures, boolean operations, file exchange, COMPAS-JSON serialization, the `rpc.Proxy` solver bridge, and host detection
- Accept: AEC computational geometry, networks, and form-finding feeding the algebra and geometry owners
- Reject: wrapper-renames of primitives or `json_dump`; a hand-rolled vector, best-fit, boolean, or form-finding solver where compas and its companions are admitted; positional datastructure constructors over `from_*`; a parallel transform page where the transform rows are `NumericalOp` table entries; an in-process re-entry where the `rpc.Proxy` offload is the heavy-band path
