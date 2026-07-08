# [PY_DATA_API_TRIMESH]

Full surface and stacking: `libs/python/geometry/.api/trimesh.md` (geometry-tier canonical owner).

`trimesh` enters data as the load/export interchange edge around the geometry-owned triangular mesh. Data accepts an in-memory `Trimesh` crossing from geometry, reads mesh files into that canonical root, and emits mesh/scene exchange bytes without re-cataloguing the modeling, repair, boolean, proximity, sampling, or registration surface.

## [01]-[DATA_LOAD_EXPORT]

[OVERLAY_SCOPE]:
- `trimesh.load(file_obj, file_type=None, resolver=None, force=None, allow_remote=False, **kwargs)` is the polymorphic intake; its return kind (`Trimesh`, `Scene`, or `Path3D`) discriminates on source content.
- `load_mesh` and `load_scene` force the return family when a data owner already knows the expected shape.
- `Trimesh.export(file_obj=None, file_type=None, **kwargs)` and `Scene.export(...)` are the mesh/scene egress edges for `STL`, `OBJ`, `PLY`, `GLTF`, `GLB`, `OFF`, and `3MF`.
- `Scene.to_mesh()` / `Scene.dump(concatenate=True)` collapse a named scene graph to one `Trimesh` before data hands it to mesh-algebra or mesh-file egress.
- `file_type` is a policy value for extensionless sources, never a `load_<format>` or `export_<format>` function family.

[MESH_AEC]:
- Geometry hands data a `Trimesh` with vertices/faces arrays and unit metadata already conditioned by the geometry tier.
- Data reads `mesh.units or "m"` as the unit hint on interchange, and the owning payload records vertex count, face count, watertight flag, volume, surface area, `identifier_hash`, and export byte length.
- Data never mutates raw vertex/face arrays; conditioning uses the geometry canonical surface before interchange.
- OpenNURBS `.3dm` routes to `rhino3dm`, unstructured solver mesh formats route to `meshio`, point-cloud registration routes to `open3d`, and IFC routes to `ifcopenshell`.

## [02]-[CAPTURE_GAP]

[CAPTURE_GAP]:
- members: verified by introspection against an installed `trimesh==4.12.2` companion distribution with `manifold3d`/`rtree`/`scipy`/`shapely`/`networkx` resolved; every data-used load/export member resolves with the signatures named here.
- query tuple shapes: `closest_point`/`ProximityQuery.on_surface` produce `(points, distances, triangle_id)`; `icp`/`procrustes` produce `(matrix, transformed, cost)`; `mesh_other` produces `(matrix, cost)`; `sample_surface` and `sample_surface_even` produce `(points, face_index)`.
- mutation return shapes: `fill_holes`/`fix_*` return `bool`; `smoothing.filter_*` returns the mutated `Trimesh`.
- boolean dispatch: `boolean.*` carries `engine=Literal["manifold", "blender", None]` and `check_volume`; the in-process default is the manifold engine.
- boundary accessors: open-edge accessors `edges_face` and `facets_boundary` are cached properties and are read directly by downstream boundary scans.

[RAIL_LAW]:
- Package: `trimesh` (data overlay)
- Owns: data-tier mesh/scene/path load and export interchange over the geometry canonical mesh surface
- Accept: polymorphic load, forced load kind, export, scene collapse, unit hint read, and interchange evidence extraction
- Reject: package-surface duplication, mesh modeling re-catalogues, raw vertex-array mutation, hand-rolled face KD-trees, wrapper-renames of load/export, and identity minting outside the runtime owner
