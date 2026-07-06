# [PY_GEOMETRY_API_COMPAS]

`compas` supplies the computational-geometry and datastructure surface for the geometry algebra rail: a pure-Python primitive/shape/transform library in `compas.geometry`, mesh/graph/volmesh datastructures in `compas.datastructures`, the COMPAS JSON serialization functions, file readers/writers in `compas.files`, host probes that detect Rhino/Grasshopper/Blender modality, and the out-of-process `compas.rpc.Proxy` solver bridge. It is the spine of the COMPAS form-finding band — `compas_dr` (dynamic relaxation) and `compas_tna` (thrust-network analysis) ride it and serialize through the same `json_dumps`/`json_loads` `Data` round-trip, so `graph/algebra.md#ALGEBRA` composes `compas.geometry` primitives, `datastructures.Mesh`/`Network`/`VolMesh`/`Assembly`/`NurbsSurface`, the `_numpy`-accelerated best-fit/hull primitives, the transform-matrix constructors, and the `rpc.Proxy` heavy-band offload into one `ComputationalGeometry` tagged-union owner; it never re-implements vector algebra, best-fit, boolean operations, or the form-finding solvers compas and its companions already own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `compas`
- package: `compas`
- import: `import compas`
- owner: `geometry`
- rail: algebra
- installed: `2.15.1`
- license: MIT
- entry points: none (library only)
- capability: geometric primitives, shapes, NURBS curves/surfaces, BREP, transformations, vector and `_numpy`-accelerated best-fit/hull algebra, mesh/graph/network/volmesh datastructures, boolean operations, OBJ/PLY/STL/OFF/GLTF/DXF read/write, COMPAS JSON `Data` serialization, the `rpc.Proxy` out-of-process CPython solver bridge, and host-modality detection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry primitives and shapes (`compas.geometry`)
- rail: algebra

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
- rail: algebra

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
- rail: algebra

The `compas.geometry.__all__` carries 342 entries: 56 classes and 286 free functions; `_numpy`-suffixed variants are NumPy/SciPy-accelerated (the heavy-band rows `graph/algebra.md` offloads through `rpc.Proxy`). Functions take and return plain coordinate sequences.

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]        | [CAPABILITY]                       |
| :-----: | :---------------------------------- | :------------------ | :--------------------------------- |
|  [01]   | `add_vectors` / `cross_vectors`     | vector pairs        | vector addition and cross product  |
|  [02]   | `dot_vectors` / `normalize_vector`  | vectors             | dot product and unit normalization |
|  [03]   | `angle_vectors`                     | two vectors         | angle between vectors              |
|  [04]   | `area_polygon` / `area_triangle`    | polygon/points      | planar area                        |
|  [05]   | `barycentric_coordinates`           | point plus triangle | barycentric coordinates            |
|  [06]   | `bestfit_plane`                     | points              | least-squares plane fit (`PROJECTIVE` transform source) |
|  [07]   | `centroid_points`                   | points              | point-set centroid (`AFFINE` translation source) |
|  [08]   | `bestfit_frame_numpy`               | points              | accelerated best-fit frame; returns the `(origin, xaxis, yaxis)` float triple (NOT a `Frame`) the `RIGID`/`SIMILARITY` rows wrap as `Frame(*triple)`; SciPy-backed, `rpc`-offloaded |
|  [09]   | `bestfit_circle_numpy`              | points              | accelerated best-fit circle (SciPy-backed) |
|  [10]   | `bestfit_sphere_numpy`              | points              | accelerated best-fit sphere (SciPy-backed) |
|  [11]   | `bbox` / `bbox_numpy`               | points              | axis-aligned bounding box          |
|  [12]   | `oriented_bounding_box_numpy`       | points              | minimal oriented bounding box; returns `list[[float, float, float]]` (8 corner points); SciPy-backed, `rpc`-offloaded |
|  [13]   | `convex_hull` / `convex_hull_numpy` | points              | convex hull; `_numpy` returns `(ndarray[int](N,), ndarray[int](M, 3))` (hull vertex indices, faces); SciPy-backed, `rpc`-offloaded |
|  [14]   | `boolean_union_mesh_mesh`           | two meshes          | mesh boolean union                 |
|  [15]   | `boolean_difference_mesh_mesh`      | two meshes          | mesh boolean difference            |
|  [16]   | `boolean_intersection_mesh_mesh`    | two meshes          | mesh boolean intersection          |
|  [17]   | `intersection_line_line`            | two lines           | line-line intersection point       |
|  [18]   | `closest_point_on_line`             | point plus line     | nearest point projection           |

[ENTRYPOINT_SCOPE]: transform constructors (`compas.geometry`)
- rail: algebra

The transform matrices are pure-Python value objects (no SciPy core); `graph/algebra.md` folds the four rows below into one `NUMERICAL` table keyed by `NumericalOp`, never a dispatch branch per transform. They compose from a fitted frame/plane so a transform is a numerical op on a coordinate set, not a second concern.

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]              | [CAPABILITY]                                       |
| :-----: | :----------------------------------------- | :----------------------- | :------------------------------------------------- |
|  [01]   | `Frame.worldXY()`                          | none                     | the world origin frame (transform source frame)    |
|  [02]   | `Transformation.from_frame_to_frame(a, b)` | two `Frame`              | rigid frame-to-frame map (`RIGID` row)             |
|  [03]   | `Translation.from_vector(vector)`          | a 3-vector / centroid    | pure translation (`AFFINE` row)                    |
|  [04]   | `Scale.from_factors(factors, frame=None)`  | xyz factors plus frame   | anisotropic scaling about a frame (`SIMILARITY` row) |
|  [05]   | `Projection.from_plane(plane)`             | a `Plane`/best-fit plane | orthographic projection onto a plane (`PROJECTIVE` row) |

[ENTRYPOINT_SCOPE]: datastructure construction and accessors (`compas.datastructures`)
- rail: algebra

Construction routes through `from_*` classmethods, never positional constructors; the datastructure algebra verbs the `DATASTRUCTURE` table folds read through these accessors.

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]            | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------- | :---------------------- | :--------------------------------------------- |
|  [01]   | `Network.from_nodes_and_edges(nodes, edges)`    | node list plus edge list | build a `Network` from adjacency               |
|  [02]   | `Mesh.from_json(string)` / `from_jsonstring`    | COMPAS-JSON string      | reconstruct a `Mesh` from a serialized handle  |
|  [03]   | `Mesh.dual()` / `Mesh.subdivide(scheme=...)`    | none / subdivision tag  | dual mesh, subdivided mesh                     |
|  [04]   | `Mesh.vertices_attributes(names=None, values=None, keys=None)` | names / row values / keys | polymorphic get-or-set: `names` alone reads one Nx-len row per vertex (`'xyz'` Nx3); `values=` writes the SAME `values` object to EVERY vertex (its SET leg `for key in keys: vertex_attributes(key, names, values)` zips `names` against the outer rows of `values`, so it does NOT distribute one row per vertex) — per-vertex rows need a `for key, row in zip(keys, rows): vertex_attributes(key, names, row)` loop |
|  [05]   | `Mesh.vertex_attributes(key, names=None, values=None)` | one key / names / one row | single-vertex get-or-set: as setter (`names` and `values` both given) it zips `names` against `values` and writes them onto `key`, returning `None` — THE genuine row-setter the `vertices_attributes` per-vertex write-back loop calls per vertex |
|  [06]   | `Mesh.number_of_edges()` / `number_of_vertices` | none                    | topology counts (edge/vertex census)           |
|  [07]   | `VolMesh.dual()`                                | none                    | volumetric dual cell complex                   |
|  [08]   | `Assembly.graph`                                | property                | the part-adjacency `Graph` of an assembly      |
|  [09]   | `NurbsSurface.to_mesh(nu=..., nv=...)`          | tessellation density    | tessellate a NURBS surface to a `Mesh`         |

[ENTRYPOINT_SCOPE]: serialization, construction, and host probes
- rail: algebra

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]        | [CAPABILITY]                           |
| :-----: | :----------------------------- | :------------------ | :------------------------------------- |
|  [01]   | `compas.json_dump`             | data plus filepath  | write COMPAS JSON to a file            |
|  [02]   | `compas.json_dumps`            | data                | serialize any `Data` subclass to a COMPAS-JSON string (the one graduation handle) |
|  [03]   | `compas.json_dumpz`            | data plus filepath  | write compressed COMPAS JSON           |
|  [04]   | `compas.json_load`             | filepath            | read COMPAS JSON from a file           |
|  [05]   | `compas.json_loads`            | string              | parse a COMPAS-JSON string back to its `Data` instance (the decode leg the form-finding constraint fold reads) |
|  [06]   | `Mesh.from_vertices_and_faces` | vertices plus faces | construct a mesh from arrays           |
|  [07]   | `Mesh.from_obj` / `from_ply`   | filepath            | construct from `OBJ`/`PLY`/`STL`/`OFF` |
|  [08]   | `Mesh.from_polyhedron`         | face count          | construct a platonic mesh              |
|  [09]   | `compas.is_rhino`              | none                | detect Rhino host                      |
|  [10]   | `compas.is_grasshopper`        | none                | detect Grasshopper host                |
|  [11]   | `compas.is_blender`            | none                | detect Blender host                    |
|  [12]   | `compas.is_ironpython`         | none                | detect IronPython runtime              |

[ENTRYPOINT_SCOPE]: out-of-process solver bridge (`compas.rpc`)
- rail: algebra

`Proxy` runs a CPython server in a separate process and resolves dotted callables across the wire; `graph/algebra.md#bridged` routes the SciPy-backed heavy band (`_numpy` primitives, `compas_dr`/`compas_tna` numpy solvers) through `Proxy` so the gated companion solver never blocks in-process, crossing the blocking RPC call to the event loop through the runtime lane THREAD band (`lane.offload(..., modality=Modality.THREAD)`, the lane's own `async_boundary` the fence). Construction is EAGER and lifecycle-bearing: `Proxy.__init__` first `_try_reconnect`s to an already-running localhost server (default `url='http://127.0.0.1'`, `port=1753`) and only when no server answers `ping()` does it spawn one through the blocking `start_server()`, recording ownership in `_implicitely_started_server`; `function(dotted_name)` is pure dispatch over an already-running server and never starts one. `Proxy` is therefore a sync context manager whose `__exit__` runs the ownership-aware teardown, so the `graph/algebra.md#ALGEBRA` `solver_proxy` async-resource scope drives its `__enter__`/`__exit__` through the runtime lane THREAD band (`lane.offload`) — never a per-`function` construction on the caller's event-loop thread. Pure-Python transform rows, `Network` adjacency builds, and `from_json` datastructure algebra carry no SciPy core and stay in-thread — the proxy is a per-row capability, not a blanket re-entry.

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]            | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------- | :---------------------- | :----------------------------------------------- |
|  [01]   | `Proxy(package=None, python=None, url='http://127.0.0.1', port=1753, max_conn_attempts=100, autoreload=True, capture_output=True, ...)` | optional module/interp  | eager construction: `_try_reconnect`s to the running localhost server on `url`/`port` or spawns one via the blocking `start_server()` (`Popen` + `time.sleep(0.1)` ping loop up to `max_conn_attempts`), recording ownership in `_implicitely_started_server` for teardown |
|  [02]   | `Proxy.function(dotted_name)`                      | a dotted callable path  | resolve a remote callable (e.g. `"compas.geometry.bestfit_frame_numpy"`, `"compas_dr.solvers.dr_numpy"`) over an already-running server; never starts one |
|  [03]   | `Proxy.__enter__()` / `Proxy.__exit__(*args)`      | none                    | sync context-manager protocol; `__exit__` runs ownership-aware teardown — `stop_server()` only when `_implicitely_started_server` (this proxy spawned it), otherwise it disconnects from a reused server |
|  [04]   | `Proxy.restart_server()` / `stop_server()`         | none                    | recovery control: `restart_server()` is `stop_server()`+`start_server()`, NOT the bring-up path (construction is the bring-up); reserved for the transient-recovery growth axis |
|  [05]   | `compas.rpc.RPCServerError` / `RPCClientError`     | exception               | the bring-up/remote-dispatch fault surface (exported from `compas.rpc.__all__`); `start_server()` raises `RPCServerError("The server is not available.")` on a cold-start timeout, `_proxy` raises on a remote fault |

## [04]-[IMPLEMENTATION_LAW]

[GEOMETRY_ALGEBRA]:
- import: `import compas` at boundary scope only; module-level import is banned by the manifest import policy.
- primitive axis: `compas.geometry` primitives, shapes, curves, surfaces, and BREP are pure-Python value objects; `_numpy`-suffixed functions (`bestfit_frame_numpy`, `bbox_numpy`, `convex_hull_numpy`, `oriented_bounding_box_numpy`) provide SciPy-accelerated variants over the same coordinate inputs, never a parallel class hierarchy. The transform constructors (`Transformation.from_frame_to_frame`, `Translation.from_vector`, `Scale.from_factors`, `Projection.from_plane`) are pure-Python matrix composition fitted off a frame/plane, so `graph/algebra.md` folds best-fit/hull primitives AND transform rows into one `NumericalOp`-keyed `NUMERICAL` table — a transform is a numerical op on a coordinate set, not a second `graph/transform` concern.
- datastructure axis: `Mesh` is a half-edge structure; `Graph`/`Network` share node/edge topology; `VolMesh`/`CellNetwork` carry volumetric and cell complexes. Construction routes through `from_*` classmethods (`from_vertices_and_faces`, `from_obj`, `from_polyhedron`, `from_nodes_and_edges`, `from_json`), never positional constructors; the datastructure algebra verbs (`Mesh.dual`/`subdivide`, `VolMesh.dual`, `Assembly.graph`, `NurbsSurface.to_mesh`) read through the accessors, folded into the `DatastructureOp`-keyed `DATASTRUCTURE` table.
- serialization axis: `json_dump`/`json_dumps`/`json_dumpz` and the `json_load*` pair are the COMPAS `Data` serializers backed by `compas.data.DataEncoder`/`DataDecoder`; every `Data` subclass — including the `compas_dr` `InputData`/`ResultData`/`Constraint` carriers and the `compas_tna` `FormDiagram`/`ForceDiagram` — round-trips through them, so persistence and the graduation handle are one serializer family, never per-type encoders. `DataEncoder.default` ALSO coerces raw NumPy out of the box — `numpy.ndarray -> o.tolist()` and every numpy int/float/bool scalar to its Python builtin — so a non-`Data` `_numpy` return (the `bestfit_frame_numpy` float triple, the `oriented_bounding_box_numpy` 8-corner list, the `convex_hull_numpy` `(indices, faces)` integer-ndarray pair) serializes through the same `json_dumps` with no caller-side `.tolist()` and no output projector. The algebra owner serializes every result through `json_dumps` for graduation and decodes constraint geometry through `json_loads` so `Constraint.get_constraint_cls` dispatches on the real decoded type.
- form-finding band axis: `compas_dr` and `compas_tna` are the COMPAS solver companions — `compas_dr` builds `InputData.from_mesh` then runs `dr_numpy`/`dr_constrained_numpy` and writes back through `ResultData.update_mesh`; `compas_tna` runs `FormDiagram.from_mesh` → `relax_boundary_openings` → `LoadUpdater` selfweight → `horizontal_numpy` → `vertical_from_zmax`. Both ride `compas` as the spine, share its `Mesh`/`json_*` surface, and are selected by the `FormEngine` sub-enum on the one form-finding case, never two parallel pages.
- rpc bridge axis: `compas.rpc.Proxy.function(dotted_name)` resolves the SciPy-backed heavy band out of process. The blocking surface is the WHOLE proxy lifecycle, not only the per-`function` solve: `Proxy(...)` construction eagerly `_try_reconnect`s to the running localhost server or spawns one through the blocking `start_server()` (Popen + ping loop), so the `graph/algebra.md#ALGEBRA` `solver_proxy` async-resource scope offloads both the eager `__enter__` construction AND each `function` solve across the runtime lane THREAD band (`lane.offload(..., modality=Modality.THREAD)`), the ownership-aware `__exit__` teardown riding the same offload — never a per-`function` lazy first-call bring-up on the caller's event-loop thread. The in-process `run` and out-of-process `bridged` paths share one fault rail rather than a parallel async surface; the cold-start bring-up transient (`RPCServerError`/`RPCClientError`) retries under the runtime `RetryClass.RPC` row on the scope-entry offload leg. The bridge band is exactly the `_numpy` primitives plus the `dr`/`tna` numpy solvers, keyed per row off the `NumericalSpec.rpc` row field (`spec.rpc is not None`), never a parallel `_NUMERICAL_RPC` map and never a matrix-multiply marshalled across the process wall.
- subpackages: `geometry`, `datastructures`, `colors`, `files` (OBJ/PLY/STL/OFF/GLTF/XML readers-writers), `topology`, `scene`, `rpc` (out-of-process CPython solver bridge), `data`, `tolerance`.
- evidence: each geometry op captures input vertex/point count and result type; each datastructure op captures vertex/face/edge counts; each form-finding pass captures the `numpy`-reduced max-abs residual against the `FormParams.tol` ceiling as an algebra receipt.
- boundary: compas owns the pure-Python algebra and datastructures; SciPy-backed acceleration routes through the `_numpy` rows offloaded via `rpc.Proxy`+`anyio`, mesh exchange to `meshio`/`trimesh`, dynamic relaxation to `compas_dr`, thrust-network analysis to `compas_tna`, non-manifold topology to `topologicpy` (the `nonmanifold` sibling, never folded here), raw mesh-file decode/encode to the data `MeshPayload` seam (`run` returns COMPAS-JSON handles across the wire, never writes a mesh file); live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `compas`
- Owns: geometric primitives/shapes/transforms, NURBS and BREP, vector and best-fit algebra, mesh/graph/volmesh datastructures, boolean operations, file exchange, COMPAS JSON serialization, the `rpc.Proxy` solver bridge, and host detection
- Accept: AEC computational geometry, networks, and form-finding feeding the algebra and geometry owners; the `compas_dr`/`compas_tna` companions composing against the `compas` spine
- Reject: wrapper-renames of primitives or `json_dump`; a hand-rolled vector, best-fit, boolean, or form-finding solver where compas/`compas_dr`/`compas_tna` are admitted; positional datastructure constructors over `from_*`; a parallel `graph/transform` page where the transform rows are `NumericalOp` table entries; an in-process re-entry where `rpc.Proxy`+`anyio.to_thread.run_sync` is the heavy-band offload; identity minting the runtime owns

[CAPTURE_GAP]:
- accessor and `_numpy` return shapes (verified against the COMPAS `2.15.1` source): `Mesh.vertices_attributes(names=None, values=None, keys=None)` is one polymorphic get-or-set, but its two legs are NOT symmetric. The GET leg `[vertex_attributes(key, names) for key in keys]` correctly returns one row per vertex. The SET leg is `for key in keys: vertex_attributes(key, names, values)` — it hands the WHOLE `values` object to every vertex, and `vertex_attributes` zips `names` against the OUTER rows of `values`, so a `values=loads.tolist()` Nx3 row-write against the `('px','py','pz')` names writes `px=loads[0]`/`py=loads[1]`/`pz=loads[2]` onto EVERY vertex (dropping rows `3..N`) rather than distributing one row per vertex. Writing per-vertex rows therefore REQUIRES a `for key, row in zip(keys, rows): vertex_attributes(key, names, row)` loop — `vertex_attributes(key, names, values)` is the genuine row-setter (it zips `names` against `values` for the single `key`). `graph/algebra.md#_tna` uses exactly this per-vertex loop, over the SAME `keys = list(form.vertices())` ordering its `vertices_attributes('xyz', keys=keys)` read used, so the load rows stay vertex-aligned. `bestfit_frame_numpy(points)` returns the `(origin, xaxis, yaxis)` coordinate triple — three `[float, float, float]` rows, NOT a `Frame` — so a `Frame`-consuming transform row wraps it as `Frame(*triple)`; `Frame.__init__(point, xaxis=None, yaxis=None, name=None)` is positional and accepts plain lists (auto-orthonormalized). `centroid_points` returns a plain 3-`list` and `bestfit_plane` a `(point, normal)` tuple, both fed straight to `Translation.from_vector`/`Projection.from_plane`. The three `_numpy` primitive returns — `bestfit_frame_numpy -> (origin, xaxis, yaxis)` float triple, `oriented_bounding_box_numpy -> list[[float, float, float]]` (8 corner points), `convex_hull_numpy -> (ndarray[int](N,), ndarray[int](M, 3))` (vertex indices, faces) — all serialize directly through one `json_dumps` with NO caller `.tolist()` and NO output projector: COMPAS `DataEncoder.default` coerces `numpy.ndarray -> o.tolist()` and numpy int/float/bool scalars to their Python builtins, so the frame/OBB plain-list returns are already JSON-native and the `convex_hull_numpy` integer arrays coerce inside the encoder.
