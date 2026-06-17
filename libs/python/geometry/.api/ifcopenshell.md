# [PY_GEOMETRY_API_IFCOPENSHELL]

`ifcopenshell` API capture placeholder for `geometry`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifcopenshell`
- package: `ifcopenshell`
- import: `ifcopenshell`
- owner: `geometry`
- rail: ifc-companion
- capability: IFC->mesh/GLB tessellation daemon plus IFC property/quantity/relationship analysis

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- `ifcopenshell.file` — in-memory IFC model: `by_id`, `by_guid`, `by_type`, `create_entity`, `add`, `remove`, `traverse`, `get_inverse`, `get_total_inverses`, `schema`, `schema_identifier`, `schema_version`, `header`, `units`, `to_string`, `from_string`, `write`, `begin_transaction`/`end_transaction`/`discard_transaction`/`undo`/`redo`, `batch`/`unbatch`
- `ifcopenshell.entity_instance` — single IFC entity wrapper (attribute/inverse access)
- `ifcopenshell.geom.settings` — tessellation knob bag; key knobs `mesher-linear-deflection`, `mesher-angular-deflection`, `weld-vertices`, `use-world-coords`, `apply-default-materials`, `generate-uvs`, `triangulation-type`, `dimensionality`, `length-unit`, `precision`, `iterator-output`, `disable-opening-subtractions`, `unify-shapes` (full set via `settings().setting_names()`)
- `ifcopenshell.geom.serializer_settings`, `ifcopenshell.geom.iterator`, `ifcopenshell.geom.tree`, `ifcopenshell.geom.serializers`
- `ifcopenshell.sqlite`, `ifcopenshell.stream` — alternate model backends (rocksdb/sqlite/streamed SPF)

[ENTRYPOINTS]:
- `ifcopenshell.open(path, format=None, should_stream=False, readonly=False, mmap=False, bypass_types=None) -> file | sqlite | stream`
- `ifcopenshell.geom.create_shape(settings, inst, repr=None, geometry_library='opencascade')` — per-element tessellation
- `ifcopenshell.geom.iterator(settings, file_or_filename, num_threads=1, include=None, exclude=None, geometry_library='opencascade')` — parallel whole-model mesh daemon
- `ifcopenshell.geom.iterate(settings, file_or_filename, num_threads=1, include=None, exclude=None, *, with_progress=False, cache=None, serializer_settings=None, geometry_library='opencascade') -> Generator[IteratorOutput, None, None]`
- `ifcopenshell.geom.serialise(schema, string, *args)`, `ifcopenshell.geom.tesselate(...)`, `ifcopenshell.geom.map_shape(...)`
- `ifcopenshell.create_entity(...)`, `ifcopenshell.schema_by_name(...)`, `ifcopenshell.register_schema(...)`, `ifcopenshell.guess_format(...)`, `ifcopenshell.get_log()`

[IMPLEMENTATION_LAW]:
- Submodules: `geom` (tessellation), `util` (placement/element/unit/selector/representation analysis), `api` (model mutation verbs), `express`, `validate`, `mvd`, `sql`, `draw`, `guid`.
- `geom.has_occ` flags OpenCASCADE availability; `geometry_library='opencascade'|'cgal'|'cgal-simple'|'hybrid-cgal-simple-opencascade'` selects the kernel.
- Tessellation rail: `open` -> `geom.settings()` knob set -> `geom.iterator` over `num_threads` -> per-element `ShapeElementType` (verts/faces/materials) feeds the IFC->mesh/GLB two-hop seam.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ifcopenshell`
- Owns: IFC->mesh/GLB tessellation daemon plus IFC property/quantity/relationship analysis
- Accept: companion-floor capture on a `python_version<'3.13'` interpreter
- Reject: wrapper-renames and weaker local reimplementation

[CAPTURE_GAP]:
- floor: companion interpreter `python_version<'3.13'`, divorced from the `>=3.15` runtime floor
- state: `ifcopenshell==0.8.5` installs and reflects on a cp312 companion interpreter; the `>=3.15` project venv carries no cp315 wheel, so the project-venv `assay api query` resolves no source there
- members: verified by introspection against the installed cp312 distribution; every documented type, entrypoint, submodule, and `settings()` knob resolves — no phantom
