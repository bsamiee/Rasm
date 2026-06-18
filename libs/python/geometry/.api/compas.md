# [PY_GEOMETRY_API_COMPAS]

`compas` supplies the computational-geometry and datastructure surface for the geometry algebra rail: a pure-Python primitive/shape/transform library in `compas.geometry`, mesh/graph/volmesh datastructures in `compas.datastructures`, the COMPAS JSON serialization functions, file readers/writers in `compas.files`, and host probes that detect Rhino/Grasshopper/Blender modality. The package owner composes `compas.geometry` primitives, `datastructures.Mesh`, and the `json_dump`/`json_load` serializers into the algebra owner; it never re-implements vector algebra, best-fit, or boolean operations compas already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `compas`
- package: `compas`
- import: `import compas`
- owner: `geometry`
- rail: algebra
- installed: `2.15.1` reflected via `python -c "import compas"` on cp312
- pin: `compas>=2.15.1; python_version<'3.15'` (gated by the scipy-family CPython 3.15 lag)
- entry points: none (library only)
- capability: geometric primitives, shapes, NURBS curves/surfaces, BREP, transformations, vector and best-fit algebra, mesh/graph/network/volmesh datastructures, boolean operations, OBJ/PLY/STL/OFF/GLTF/DXF read/write, COMPAS JSON serialization, RPC compute bridge, and host-modality detection

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry primitives and shapes (`compas.geometry`)
- rail: algebra

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :------------------------------------- | :------------ | :--------------------------------------------- |
|   [1]   | `Point` / `Vector`                     | primitive     | point and direction value objects              |
|   [2]   | `Line` / `Polyline` / `Polygon`        | primitive     | linear and polygonal geometry                  |
|   [3]   | `Plane` / `Frame`                      | primitive     | infinite plane and oriented coordinate frame   |
|   [4]   | `Circle` / `Ellipse` / `Arc`           | primitive     | conic curve primitives                         |
|   [5]   | `Bezier`                               | primitive     | parametric Bezier curve                        |
|   [6]   | `Quaternion`                           | primitive     | rotation quaternion                            |
|   [7]   | `Pointcloud` / `KDTree`                | primitive     | point set and nearest-neighbor index           |
|   [8]   | `Box` / `Sphere` / `Cylinder`          | shape         | solid `Cone`/`Capsule`/`Torus`/`Polyhedron`    |
|   [9]   | `Curve` / `NurbsCurve`                 | curve         | generic and NURBS curves                       |
|  [10]   | `Surface` / `NurbsSurface`             | surface       | generic and NURBS surfaces                     |
|  [11]   | `PlanarSurface` / `CylindricalSurface` | surface       | analytic `Conical`/`Spherical`/`Toroidal`      |
|  [12]   | `Brep` / `BrepFace` / `BrepEdge`       | brep          | boundary-representation `Loop`/`Vertex`/`Trim` |
|  [13]   | `Transformation` / `Translation`       | transform     | affine transform matrices                      |
|  [14]   | `Rotation` / `Scale` / `Shear`         | transform     | `Reflection`/`Projection` transforms           |

[PUBLIC_TYPE_SCOPE]: datastructures (`compas.datastructures`)
- rail: algebra

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                                 |
| :-----: | :---------------------- | :-------------- | :------------------------------------------- |
|   [1]   | `Mesh`                  | mesh            | half-edge mesh with topology and geometry    |
|   [2]   | `Graph`                 | graph           | node/edge graph datastructure                |
|   [3]   | `Network`               | graph           | spatial node/edge network                    |
|   [4]   | `VolMesh`               | volumetric mesh | volumetric half-face mesh                    |
|   [5]   | `CellNetwork`           | cell complex    | cell/face/edge network                       |
|   [6]   | `Tree` / `TreeNode`     | tree            | hierarchical tree datastructure              |
|   [7]   | `Assembly` / `Part`     | assembly        | part assembly with features                  |
|   [8]   | `HashTree` / `HashNode` | hash tree       | content-addressed hierarchy                  |
|   [9]   | `Datastructure`         | base            | shared datastructure data/serialization base |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: algebra and best-fit functions (`compas.geometry`)
- rail: algebra

The `compas.geometry.__all__` carries 342 entries: 56 classes and 286 free functions; `_numpy`-suffixed variants are NumPy-accelerated. Functions take and return plain coordinate sequences.

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]        | [CAPABILITY]                       |
| :-----: | :---------------------------------- | :------------------ | :--------------------------------- |
|   [1]   | `add_vectors` / `cross_vectors`     | vector pairs        | vector addition and cross product  |
|   [2]   | `dot_vectors` / `normalize_vector`  | vectors             | dot product and unit normalization |
|   [3]   | `angle_vectors`                     | two vectors         | angle between vectors              |
|   [4]   | `area_polygon` / `area_triangle`    | polygon/points      | planar area                        |
|   [5]   | `barycentric_coordinates`           | point plus triangle | barycentric coordinates            |
|   [6]   | `bestfit_plane`                     | points              | least-squares plane fit            |
|   [7]   | `bestfit_frame_numpy`               | points              | accelerated best-fit frame         |
|   [8]   | `bestfit_circle_numpy`              | points              | accelerated best-fit circle        |
|   [9]   | `bestfit_sphere_numpy`              | points              | accelerated best-fit sphere        |
|  [10]   | `bbox` / `bbox_numpy`               | points              | axis-aligned bounding box          |
|  [11]   | `oriented_bounding_box_numpy`       | points              | minimal oriented bounding box      |
|  [12]   | `convex_hull` / `convex_hull_numpy` | points              | convex hull                        |
|  [13]   | `boolean_union_mesh_mesh`           | two meshes          | mesh boolean union                 |
|  [14]   | `boolean_difference_mesh_mesh`      | two meshes          | mesh boolean difference            |
|  [15]   | `boolean_intersection_mesh_mesh`    | two meshes          | mesh boolean intersection          |
|  [16]   | `intersection_line_line`            | two lines           | line-line intersection point       |
|  [17]   | `closest_point_on_line`             | point plus line     | nearest point projection           |

[ENTRYPOINT_SCOPE]: serialization, construction, and host probes
- rail: algebra

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]        | [CAPABILITY]                           |
| :-----: | :----------------------------- | :------------------ | :------------------------------------- |
|   [1]   | `compas.json_dump`             | data plus filepath  | write COMPAS JSON to a file            |
|   [2]   | `compas.json_dumps`            | data                | serialize COMPAS JSON to a string      |
|   [3]   | `compas.json_dumpz`            | data plus filepath  | write compressed COMPAS JSON           |
|   [4]   | `compas.json_load`             | filepath            | read COMPAS JSON from a file           |
|   [5]   | `compas.json_loads`            | string              | parse COMPAS JSON from a string        |
|   [6]   | `Mesh.from_vertices_and_faces` | vertices plus faces | construct a mesh from arrays           |
|   [7]   | `Mesh.from_obj` / `from_ply`   | filepath            | construct from `OBJ`/`PLY`/`STL`/`OFF` |
|   [8]   | `Mesh.from_polyhedron`         | face count          | construct a platonic mesh              |
|   [9]   | `compas.is_rhino`              | none                | detect Rhino host                      |
|  [10]   | `compas.is_grasshopper`        | none                | detect Grasshopper host                |
|  [11]   | `compas.is_blender`            | none                | detect Blender host                    |
|  [12]   | `compas.is_ironpython`         | none                | detect IronPython runtime              |

## [4]-[IMPLEMENTATION_LAW]

[GEOMETRY_ALGEBRA]:
- import: `import compas` at boundary scope only; module-level import is banned by the manifest import policy.
- primitive axis: `compas.geometry` primitives, shapes, curves, surfaces, and BREP are pure-Python value objects; `_numpy`-suffixed functions (`bestfit_frame_numpy`, `bbox_numpy`, `convex_hull_numpy`, `oriented_bounding_box_numpy`) provide accelerated variants over the same coordinate inputs, never a parallel class hierarchy.
- datastructure axis: `Mesh` is a half-edge structure; `Graph`/`Network` share node/edge topology; `VolMesh`/`CellNetwork` carry volumetric and cell complexes. Construction routes through `from_*` classmethods (`from_vertices_and_faces`, `from_obj`, `from_polyhedron`), never positional constructors.
- serialization axis: `json_dump`/`json_dumps`/`json_dumpz` and the `json_load*` pair are the COMPAS data serializers; every `Data` subclass round-trips through them, so persistence is one serializer family, never per-type encoders.
- subpackages: `geometry`, `datastructures`, `colors`, `files` (OBJ/PLY/STL/OFF/GLTF/XML readers-writers), `topology`, `scene`, `rpc` (out-of-process CPython solver bridge), `data`, `tolerance`.
- evidence: each geometry op captures input vertex/point count and result type; each datastructure op captures vertex/face/edge counts as an algebra receipt.
- boundary: compas owns the pure-Python algebra and datastructures; native scientific acceleration routes through `numpy`/`scipy` companions, mesh exchange to `meshio`/`trimesh`, dynamic relaxation to `compas_dr`, thrust-network analysis to `compas_tna`; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `compas`
- Owns: geometric primitives/shapes/transforms, NURBS and BREP, vector and best-fit algebra, mesh/graph/volmesh datastructures, boolean operations, file exchange, COMPAS JSON serialization, and host detection
- Accept: AEC computational geometry, networks, and form-finding feeding the algebra and geometry owners
- Reject: wrapper-renames of primitives or `json_dump`; a hand-rolled vector, best-fit, or boolean kernel where compas is admitted; positional datastructure constructors over `from_*`; identity minting the runtime owns

[CAPTURE_GAP]:
- floor: companion interpreter cp312; the `>=3.15` project venv carries no cp315 wheel for the scientific companions (`numpy`/`scipy`), so the project-venv `assay api query` resolves no source there
- members: verified by introspection against the installed cp312 distribution; the `compas.geometry.__all__` count of 342 (56 classes, 286 functions) is the verified size, and every documented primitive, datastructure, free function, serializer, host probe, and subpackage resolves — no phantom
