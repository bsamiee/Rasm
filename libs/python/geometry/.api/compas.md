# [PY_GEOMETRY_API_COMPAS]

`compas` supplies the computational-geometry and datastructure surface for the geometry algebra rail: a pure-Python primitive/shape/transform library in `compas.geometry`, mesh/graph/volmesh datastructures in `compas.datastructures`, the COMPAS JSON serialization functions, file readers/writers in `compas.files`, host probes that detect Rhino/Grasshopper/Blender modality, and the out-of-process `compas.rpc.Proxy` solver bridge. It is the spine of the COMPAS form-finding band — `compas_dr` (dynamic relaxation) and `compas_tna` (thrust-network analysis) ride it and serialize through the same `json_dumps`/`json_loads` `Data` round-trip, so `graph/algebra.md#ALGEBRA` composes `compas.geometry` primitives, `datastructures.Mesh`/`Network`/`VolMesh`/`Assembly`/`NurbsSurface`, the `_numpy`-accelerated best-fit/hull primitives, the transform-matrix constructors, and the `rpc.Proxy` heavy-band offload into one `ComputationalGeometry` tagged-union owner; it never re-implements vector algebra, best-fit, boolean operations, or the form-finding solvers compas and its companions already own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `compas`
- package: `compas`
- import: `import compas`
- owner: `geometry`
- rail: algebra
- installed: `2.15.1` reflected via `python -c "import compas"` on the cp312 companion (`assay api resolve compas` resolves no source on the cp315 `3.15.0b2` core — the documented companion-band gap, not a catalog fault)
- license: MIT
- wheel-floor: pure-Python `py3-none-any` wheel for `compas` itself, but the scientific companions it pulls (`numpy`/`scipy` for the `_numpy` primitives) carry no cp315 wheel; ABI: none for compas core, scipy-family native ABI for the accelerated rows
- pin: `compas>=2.15.1; python_version<'3.15'` (gated by the scipy-family CPython 3.15 lag — `gast`/`pythran` lack cp315 support), alongside the `compas_dr`/`compas_tna` form-finding band on the same marker
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
|  [08]   | `bestfit_frame_numpy`               | points              | accelerated best-fit frame (`RIGID`/`SIMILARITY` source; SciPy-backed, `rpc`-offloaded) |
|  [09]   | `bestfit_circle_numpy`              | points              | accelerated best-fit circle (SciPy-backed) |
|  [10]   | `bestfit_sphere_numpy`              | points              | accelerated best-fit sphere (SciPy-backed) |
|  [11]   | `bbox` / `bbox_numpy`               | points              | axis-aligned bounding box          |
|  [12]   | `oriented_bounding_box_numpy`       | points              | minimal oriented bounding box (SciPy-backed, `rpc`-offloaded) |
|  [13]   | `convex_hull` / `convex_hull_numpy` | points              | convex hull (`_numpy` SciPy-backed, `rpc`-offloaded) |
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
|  [04]   | `Mesh.vertices_attributes(names)`               | attribute name(s)       | per-vertex attribute rows (e.g. `'xyz'` Nx3)   |
|  [05]   | `Mesh.number_of_edges()` / `number_of_vertices` | none                    | topology counts (edge/vertex census)           |
|  [06]   | `VolMesh.dual()`                                | none                    | volumetric dual cell complex                   |
|  [07]   | `Assembly.graph`                                | property                | the part-adjacency `Graph` of an assembly      |
|  [08]   | `NurbsSurface.to_mesh(nu=..., nv=...)`          | tessellation density    | tessellate a NURBS surface to a `Mesh`         |

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

`Proxy` runs a CPython server in a separate process and resolves dotted callables across the wire; `graph/algebra.md#bridged` routes the SciPy-backed heavy band (`_numpy` primitives, `compas_dr`/`compas_tna` numpy solvers) through `Proxy` so the gated companion solver never blocks in-process, crossing the blocking RPC call to the event loop through `anyio.to_thread.run_sync` (the awaitable thunk `async_boundary` requires). Pure-Python transform rows, `Network` adjacency builds, and `from_json` datastructure algebra carry no SciPy core and stay in-thread — the proxy is a per-row capability, not a blanket re-entry.

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]            | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------- | :---------------------- | :----------------------------------------------- |
|  [01]   | `Proxy(package=None, python=None, ...)`            | optional module/interp  | construct the proxy bound to a CPython worker    |
|  [02]   | `Proxy.function(dotted_name)`                      | a dotted callable path  | resolve a remote callable (e.g. `"compas.geometry.bestfit_frame_numpy"`, `"compas_dr.solvers.dr_numpy"`) |
|  [03]   | `Proxy.restart_server()` / `stop_server()`         | none                    | lifecycle control of the worker process          |

## [04]-[IMPLEMENTATION_LAW]

[GEOMETRY_ALGEBRA]:
- import: `import compas` at boundary scope only; module-level import is banned by the manifest import policy.
- primitive axis: `compas.geometry` primitives, shapes, curves, surfaces, and BREP are pure-Python value objects; `_numpy`-suffixed functions (`bestfit_frame_numpy`, `bbox_numpy`, `convex_hull_numpy`, `oriented_bounding_box_numpy`) provide SciPy-accelerated variants over the same coordinate inputs, never a parallel class hierarchy. The transform constructors (`Transformation.from_frame_to_frame`, `Translation.from_vector`, `Scale.from_factors`, `Projection.from_plane`) are pure-Python matrix composition fitted off a frame/plane, so `graph/algebra.md` folds best-fit/hull primitives AND transform rows into one `NumericalOp`-keyed `NUMERICAL` table — a transform is a numerical op on a coordinate set, not a second `graph/transform` concern.
- datastructure axis: `Mesh` is a half-edge structure; `Graph`/`Network` share node/edge topology; `VolMesh`/`CellNetwork` carry volumetric and cell complexes. Construction routes through `from_*` classmethods (`from_vertices_and_faces`, `from_obj`, `from_polyhedron`, `from_nodes_and_edges`, `from_json`), never positional constructors; the datastructure algebra verbs (`Mesh.dual`/`subdivide`, `VolMesh.dual`, `Assembly.graph`, `NurbsSurface.to_mesh`) read through the accessors, folded into the `DatastructureOp`-keyed `DATASTRUCTURE` table.
- serialization axis: `json_dump`/`json_dumps`/`json_dumpz` and the `json_load*` pair are the COMPAS `Data` serializers; every `Data` subclass — including the `compas_dr` `InputData`/`ResultData`/`Constraint` carriers and the `compas_tna` `FormDiagram`/`ForceDiagram` — round-trips through them, so persistence and the graduation handle are one serializer family, never per-type encoders. The algebra owner serializes every result through `json_dumps` for graduation and decodes constraint geometry through `json_loads` so `Constraint.get_constraint_cls` dispatches on the real decoded type.
- form-finding band axis: `compas_dr` and `compas_tna` are the COMPAS solver companions — `compas_dr` builds `InputData.from_mesh` then runs `dr_numpy`/`dr_constrained_numpy` and writes back through `ResultData.update_mesh`; `compas_tna` runs `FormDiagram.from_mesh` → `relax_boundary_openings` → `LoadUpdater` selfweight → `horizontal_numpy` → `vertical_from_zmax`. Both ride `compas` as the spine, share its `Mesh`/`json_*` surface, and are selected by the `FormEngine` sub-enum on the one form-finding case, never two parallel pages.
- rpc bridge axis: `compas.rpc.Proxy.function(dotted_name)` resolves the SciPy-backed heavy band out of process; the blocking RPC call crosses to the event loop through `anyio.to_thread.run_sync`, so the in-process `evaluate` and out-of-process `bridged` paths share one fault rail rather than a parallel async surface. The bridge band is exactly the `_numpy` primitives plus the `dr`/`tna` numpy solvers, keyed per row (`op in _NUMERICAL_RPC`), never a matrix-multiply marshalled across the process wall.
- subpackages: `geometry`, `datastructures`, `colors`, `files` (OBJ/PLY/STL/OFF/GLTF/XML readers-writers), `topology`, `scene`, `rpc` (out-of-process CPython solver bridge), `data`, `tolerance`.
- evidence: each geometry op captures input vertex/point count and result type; each datastructure op captures vertex/face/edge counts; each form-finding pass captures the `numpy`-reduced max-abs residual against the `FormParams.tol` ceiling as an algebra receipt.
- boundary: compas owns the pure-Python algebra and datastructures; SciPy-backed acceleration routes through the `_numpy` rows offloaded via `rpc.Proxy`+`anyio`, mesh exchange to `meshio`/`trimesh`, dynamic relaxation to `compas_dr`, thrust-network analysis to `compas_tna`, non-manifold topology to `topologicpy` (the `nonmanifold` sibling, never folded here), raw mesh-file decode/encode to the data `MeshPayload` seam (`evaluate` returns COMPAS-JSON handles across the wire, never writes a mesh file); live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `compas`
- Owns: geometric primitives/shapes/transforms, NURBS and BREP, vector and best-fit algebra, mesh/graph/volmesh datastructures, boolean operations, file exchange, COMPAS JSON serialization, the `rpc.Proxy` solver bridge, and host detection
- Accept: AEC computational geometry, networks, and form-finding feeding the algebra and geometry owners; the `compas_dr`/`compas_tna` companions composing against the `compas` spine
- Reject: wrapper-renames of primitives or `json_dump`; a hand-rolled vector, best-fit, boolean, or form-finding solver where compas/`compas_dr`/`compas_tna` are admitted; positional datastructure constructors over `from_*`; a parallel `graph/transform` page where the transform rows are `NumericalOp` table entries; an in-process re-entry where `rpc.Proxy`+`anyio.to_thread.run_sync` is the heavy-band offload; identity minting the runtime owns

[CAPTURE_GAP]:
- floor: companion interpreter cp312; the `>=3.15` project venv carries no cp315 wheel for the scientific companions (`numpy`/`scipy`), so `assay api resolve compas` resolves no source on the cp315 (`3.15.0b2`) core — the documented marker gap, not a catalog fault. The `compas.geometry.__all__` count of 342 (56 classes, 286 functions) is verified against the installed cp312 distribution.
- members: every documented primitive, datastructure, free function, serializer, host probe, and subpackage resolves on the cp312 companion — no phantom. The construction/accessor classmethods (`Network.from_nodes_and_edges`, `Mesh.from_json`/`dual`/`subdivide`/`vertices_attributes`/`number_of_edges`, `VolMesh.dual`, `Assembly.graph`, `NurbsSurface.to_mesh`), the transform constructors (`Frame.worldXY`, `Transformation.from_frame_to_frame`, `Translation.from_vector`, `Scale.from_factors`, `Projection.from_plane`), the `centroid_points` free function, and the `rpc.Proxy.function` dotted-resolution + server lifecycle confirm by introspection against the cp312 distribution before the algebra fence is final — resolve under the verified 342-entry `__all__` and the `rpc` subpackage, gated on the companion interpreter, never asserted as a SciPy-core member on cp315.
