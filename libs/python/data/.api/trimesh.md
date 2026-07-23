# [PY_DATA_API_TRIMESH]

`trimesh` is the data-tier mesh and scene interchange edge: it reads mesh files into the geometry-owned `Trimesh` root and encodes mesh/scene exchange bytes back out. Data composes only the loaders and `export`, never re-cataloguing or mutating the geometry modeling, repair, boolean, proximity, sampling, or registration surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `trimesh`
- package: `trimesh`
- module: `trimesh`
- rail: data-tier mesh/scene interchange over the geometry canonical `Trimesh`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry-owned load return kinds the data edge receives
- `load_scene` returns a `Scene`; `load_mesh` returns one `Trimesh`. Scene content spans `Trimesh` `Path2D` `Path3D` `PointCloud`.

## [03]-[ENTRYPOINTS]

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

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `load_scene` is the polymorphic intake discriminating return content by source; `load_mesh` forces one `Trimesh`. Format rides `file_type` as a policy value, never a `load_<format>`/`export_<format>` family.
- Data reads interchange over the geometry-conditioned `Trimesh` and reads `is_watertight`/`volume`/`area`/`identifier_hash` for exchange evidence, never mutating raw vertex/face arrays.

[STACKING]:
- geometry canonical `trimesh` (`geometry/.api/trimesh.md`): owns mesh modeling, repair, boolean, proximity, sampling, and registration; the data edge composes only the loaders and `export`, returning the conditioned `Trimesh`.
- `rhino3dm` (`.api/rhino3dm.md`): OpenNURBS `.3dm` exchange routes here, never through the trimesh loaders.

[LOCAL_ADMISSION]:
- Unstructured solver mesh routes to `meshio`, point-cloud registration to `open3d`, IFC to `ifcopenshell`; `mesh.units or "m"` carries the interchange unit hint.

[RAIL_LAW]:
- Package: `trimesh`
- Owns: data-tier mesh/scene load and export interchange over the geometry canonical mesh
- Accept: polymorphic scene load, forced mesh load, scene collapse, export, exchange-evidence read, and the unit hint
- Reject: geometry modeling/repair/boolean/proximity/sampling re-catalogues, raw vertex-array mutation, loader/export wrapper-renames, and a `load_<format>`/`export_<format>` family
