# [PY_GEOMETRY_API_COMPAS]

`compas` API capture placeholder for `geometry`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `compas`
- package: `compas`
- import: `compas`
- owner: `geometry`
- rail: geometry-algebra
- capability: AEC computational geometry, networks, form-finding under GeometryAlgebra

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- `compas.geometry` primitives: `Point`, `Vector`, `Line`, `Plane`, `Frame`, `Quaternion`, `Polyline`, `Polygon`, `Circle`, `Ellipse`, `Arc`, `Bezier`, `Pointcloud`, `KDTree`
- `compas.geometry` shapes: `Box`, `Sphere`, `Cylinder`, `Cone`, `Capsule`, `Torus`, `Polyhedron`
- `compas.geometry` curves/surfaces/Brep: `Curve`, `NurbsCurve`, `Surface`, `NurbsSurface`, `PlanarSurface`/`CylindricalSurface`/`ConicalSurface`/`SphericalSurface`/`ToroidalSurface`, `Brep`/`BrepFace`/`BrepEdge`/`BrepLoop`/`BrepVertex`/`BrepTrim`
- `compas.geometry` transforms: `Transformation`, `Translation`, `Rotation`, `Scale`, `Shear`, `Reflection`, `Projection`
- `compas.datastructures`: `Mesh`, `Graph`, `Network`, `VolMesh`, `CellNetwork`, `Tree`/`TreeNode`, `Assembly`/`Part`, `HashTree`/`HashNode`, `Datastructure`

[ENTRYPOINTS]:
- `compas.geometry` free functions (286 functions across a 342-entry `__all__`): vector algebra (`add_vectors`, `cross_vectors`, `dot_vectors`, `angle_vectors`, `normalize_vector`), best-fit (`bestfit_plane`, `bestfit_frame_numpy`, `bestfit_circle_numpy`, `bestfit_sphere_numpy`), boolean (`boolean_difference_mesh_mesh`, `boolean_intersection_mesh_mesh`, `boolean_difference_polygon_polygon`), `bbox`/`bbox_numpy`, `barycentric_coordinates`, `area_polygon`/`area_triangle`
- `compas.json_dump`/`json_dumps`/`json_dumpz`/`json_load`/`json_loads`/`json_loadz` — COMPAS data serialization
- `compas.datastructures.Mesh` constructors `from_vertices_and_faces`, `from_obj`, `from_ply`, `from_off`, `from_stl`, `from_polyhedron`; topology/geometry methods on each datastructure
- host probes: `compas.is_rhino()`, `is_grasshopper()`, `is_blender()`, `is_ironpython()` — host-modality detection

[IMPLEMENTATION_LAW]:
- Subpackages: `geometry`, `datastructures`, `colors`, `files` (OBJ/PLY/STL/OFF/DXF/GLTF readers-writers), `topology`, `scene`, `rpc` (out-of-process compute bridge), `data`, `utilities`.
- GeometryAlgebra rail: pure-Python primitives/shapes with `_numpy`-suffixed accelerated variants for best-fit and bbox; `compas.files` owns the import/export surface; `compas.rpc` brides to CPython-only solvers.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `compas`
- Owns: AEC computational geometry, networks, form-finding under GeometryAlgebra
- Accept: companion-floor capture on a `python_version<'3.13'` interpreter
- Reject: wrapper-renames and weaker local reimplementation

[CAPTURE_GAP]:
- floor: companion interpreter `python_version<'3.13'`; rides the native geometry stack floor (numpy/scipy companions)
- state: `compas==2.15.1` installs and reflects on a cp312 companion interpreter; the `>=3.15` project venv carries no cp315 wheel for its scientific companions, so the project-venv `assay api query` resolves no source there
- members: verified by introspection against the installed cp312 distribution; every documented primitive, datastructure, free function, serializer, host probe, and subpackage resolves — the function count was corrected to the verified `__all__` size, no phantom
