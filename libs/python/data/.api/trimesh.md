# [PY_DATA_API_TRIMESH]

`trimesh` is the data-tier mesh and scene interchange edge: it reads mesh files into the geometry-owned `Trimesh` root and encodes mesh/scene exchange bytes back out. Data composes only the loaders and `export`, never re-cataloguing or mutating the geometry modeling, repair, boolean, proximity, sampling, or registration surface.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry-owned load return kinds the data edge receives
- `load_scene` returns a `Scene`; `load_mesh` returns one `Trimesh`. Scene content spans `Trimesh` `Path2D` `Path3D` `PointCloud`.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: mesh/scene load and export over STL/OBJ/PLY/OFF/GLB/GLTF/3MF
- Every surface carries `file_obj, file_type`; `export` returns bytes only when `file_obj` is None, and `file_type` selects the codec for an extensionless sink.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :--------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `load_scene(file_obj, *, allow_remote, metadata) -> Scene` | static   | read any source into a `Scene` container   |
|  [02]   | `load_mesh(file_obj) -> Trimesh`                           | static   | force a single `Trimesh`, collapsing scene |
|  [03]   | `Scene.to_mesh() -> Trimesh`                               | instance | collapse the scene graph to one mesh       |
|  [04]   | `Scene.dump(concatenate) -> list[Geometry]`                | instance | flatten geometries; `True` yields one mesh |
|  [05]   | `Trimesh.export(file_type) -> bytes \| str`                | instance | encode a mesh to exchange bytes            |
|  [06]   | `Scene.export(file_type) -> bytes`                         | instance | encode a scene with transforms             |
|  [07]   | `load(file_obj, file_type, *, force) -> Geometry`          | static   | deprecated shim over `load_scene`          |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `load_scene` is the polymorphic intake discriminating return content by source; `load_mesh` forces one `Trimesh`. Format rides `file_type` as a policy value, never a `load_<format>`/`export_<format>` family.
- `load` carries its own deprecation notice and is a backwards-compatibility shim whose body is `load_scene(...)` followed by one arm: `force="mesh"` returns `.to_mesh()` and `force="scene"` returns the scene untouched, both logging that the typed function owns the call. Every reachable behaviour is therefore rows [01]–[03] under a name that erases the return type — `load(..., force="mesh")` IS `load_mesh` and `load(..., force="scene")` IS `load_scene`, so the shim spelling buys nothing and costs the static return.
- `load` with `force` UNSET is the one shape no typed function reproduces, and it is the shape to refuse: it re-derives the pre-5.0 heuristic, returning a bare geometry where the scene holds exactly one entry and the source extension sits outside `{glb, gltf, zip, 3dxml, tar.gz}`, and a `Scene` otherwise. The return kind then tracks file content rather than the call, so one code path receives `Trimesh`, `Path2D`, `PointCloud`, or `Scene` from the same read and every consumer downstream branches on a runtime type probe.
- Data reads interchange over the geometry-conditioned `Trimesh` and reads `is_watertight`/`volume`/`area`/`identifier_hash` for exchange evidence, never mutating raw vertex/face arrays.
- The intake raise surface splits across TWO builtins and `trimesh` publishes no exception class of its own (`trimesh.exceptions` holds only `ExceptionWrapper`, a lazy-import placeholder), so a consumer catch set names both: an unregistered or undetectable `file_type` raises `NotImplementedError`, while a malformed source, an unset `file_type` on a file object, an undeterminable kwarg set, and a remote refused under `allow_remote=False` raise `ValueError`. `Trimesh.export`/`Scene.export` raise `ValueError` alone. A set spelling `ValueError` without its sibling lets an unsupported format escape the rail as an unclassified raise.

[STACKING]:
- geometry canonical `trimesh` (`geometry/.api/trimesh.md`): owns mesh modeling, repair, boolean, proximity, sampling, and registration; the data edge composes only the loaders and `export`, returning the conditioned `Trimesh`.
- `rhino3dm` (`.api/rhino3dm.md`): OpenNURBS `.3dm` exchange routes here, never through the trimesh loaders.

[LOCAL_ADMISSION]:
- Unstructured solver mesh routes to `meshio`, point-cloud registration to `open3d`, IFC to `ifcopenshell`; `Trimesh.units` is the nullable interchange unit hint.
- `Trimesh.units` reads `None` on a mesh whose source declared no unit, so it lifts to an absence posture at the read site and never coalesces to a default — `mesh.units or "m"` publishes a metre declaration this package never produced, and no consumer can tell it from a file that stated metres.
