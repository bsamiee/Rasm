# [PY_GEOMETRY_API_IFCOPENSHELL]

`ifcopenshell` supplies the IFC model and tessellation surface for the geometry ifc rail: an in-memory `file` model, an `entity_instance` wrapper, the `open` polymorphic reader across SPF/sqlite/streamed backends, and the `geom` tessellation daemon that drives IFC parse, query, mutation, and IFC-to-mesh/GLB conversion through OpenCASCADE or CGAL kernels with property, quantity, and relationship analysis. The package owner composes `ifcopenshell.open`, `file.by_type`, and `geom.iterate` into the ifc owner; it never re-implements STEP parsing or BREP tessellation ifcopenshell already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifcopenshell`
- package: `ifcopenshell`
- import: `import ifcopenshell`
- owner: `geometry`
- rail: ifc
- installed: `0.8.5` reflected via `python -c "import ifcopenshell"` on cp313
- entry points: none (library only)
- capability: IFC2X3/IFC4/IFC4X3 read/write, entity creation and mutation, transactional undo/redo, GUID encode/decode, schema introspection, OpenCASCADE/CGAL tessellation to verts/faces/materials, parallel whole-model meshing, GLB/OBJ/XML serialization, and selector-grammar element queries

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: model and entity roots
- rail: ifc

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE] | [CAPABILITY]                                           |
| :-----: | :---------------------- | :------------- | :----------------------------------------------------- |
|   [1]   | `file`                  | model root     | in-memory IFC model with query, mutation, transactions |
|   [2]   | `entity_instance`       | entity wrapper | attribute/inverse access for one IFC instance          |
|   [3]   | `sqlite`                | model backend  | sqlite-backed IFC model for large files                |
|   [4]   | `stream`                | model backend  | streamed SPF model with lazy instance access           |
|   [5]   | `Error` / `SchemaError` | fault          | parse and schema-resolution failures                   |

[PUBLIC_TYPE_SCOPE]: tessellation types (`ifcopenshell.geom`)
- rail: ifc

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]     | [CAPABILITY]                                     |
| :-----: | :------------------------- | :----------------- | :----------------------------------------------- |
|   [1]   | `geom.settings`            | tessellation knobs | deflection/units/UV/output mode knob bag         |
|   [2]   | `geom.serializer_settings` | serializer knobs   | GLB/OBJ/XML serializer configuration             |
|   [3]   | `geom.iterator`            | mesh daemon        | multi-threaded whole-model tessellation iterator |
|   [4]   | `geom.tree`                | spatial index      | bounding-box/clash spatial query tree            |
|   [5]   | `geom.ShapeElementType`    | shape result       | per-element verts/faces/normals/materials        |
|   [6]   | `geom.serializers`         | serializer set     | GLB/OBJ/XML/SVG mesh serializers                 |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model open, query, and mutate
- rail: ifc

`open` returns a `file`, `sqlite`, or `stream` discriminated by `format`/`should_stream`; query rows accept an id, GUID, or type string and return one or many `entity_instance` values.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                   | [CAPABILITY]                        |
| :-----: | :------------------------------ | :----------------------------- | :---------------------------------- |
|   [1]   | `ifcopenshell.open`             | path plus format/stream policy | open SPF/sqlite/streamed model      |
|   [2]   | `ifcopenshell.create_entity`    | type plus schema plus attrs    | construct a standalone entity       |
|   [3]   | `ifcopenshell.schema_by_name`   | schema name or version         | resolve a schema definition         |
|   [4]   | `ifcopenshell.guess_format`     | path                           | detect IFC backend format           |
|   [5]   | `file.by_type`                  | type string                    | all instances of an entity type     |
|   [6]   | `file.by_id`                    | step id or GUID                | one instance by id or GUID          |
|   [7]   | `file.create_entity`            | type plus attributes           | add a new entity to the model       |
|   [8]   | `file.add`                      | entity plus copy policy        | insert an entity (cross-model copy) |
|   [9]   | `file.remove`                   | entity                         | delete an entity                    |
|  [10]   | `file.traverse`                 | entity plus depth              | dependent-entity graph walk         |
|  [11]   | `file.get_inverse`              | entity                         | inverse-referencing instances       |
|  [12]   | `file.begin_transaction`        | none                           | start an undoable edit batch        |
|  [13]   | `file.write`                    | path                           | serialize the model                 |
|  [14]   | `util.selector.filter_elements` | model plus query string        | selector-grammar element filter     |

[ENTRYPOINT_SCOPE]: tessellation and analysis
- rail: ifc

Tessellation rows consume a `geom.settings` knob bag and a `geom.GEOMETRY_LIBRARY` kernel selector (`opencascade`/`cgal`/`cgal-simple`/`hybrid-cgal-simple-opencascade`).

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                       | [CAPABILITY]                     |
| :-----: | :------------------------------- | :--------------------------------- | :------------------------------- |
|   [1]   | `geom.create_shape`              | settings plus instance plus kernel | per-element tessellation         |
|   [2]   | `geom.iterate`                   | settings plus model plus threads   | lazy whole-model mesh generator  |
|   [3]   | `geom.iterator`                  | settings plus model plus threads   | reusable mesh iterator object    |
|   [4]   | `geom.serialise`                 | schema plus shape string           | serialize geometry to a format   |
|   [5]   | `geom.tree`                      | model plus settings                | build a spatial/clash query tree |
|   [6]   | `util.element.get_psets`         | element                            | property and quantity sets       |
|   [7]   | `util.element.get_container`     | element                            | spatial container of an element  |
|   [8]   | `util.element.get_decomposition` | element                            | aggregated and contained parts   |
|   [9]   | `api.run`                        | usecase plus model plus arguments  | high-level model mutation verb   |
|  [10]   | `guid.new` / `guid.compress`     | none or expanded GUID              | IFC GUID mint and encode/decode  |
|  [11]   | `validate.validate`              | model plus logger                  | schema-conformance validation    |

## [4]-[IMPLEMENTATION_LAW]

[IFC_TESSELLATION]:
- import: `import ifcopenshell` at boundary scope only; module-level import is banned by the manifest import policy.
- model axis: `ifcopenshell.open` is the polymorphic intake; the backend (`file`, `sqlite`, `stream`) discriminates on `format`/`should_stream`, never a parallel open function per backend. Query routes through `by_id`/`by_guid`/`by_type` on one `file`, never per-key getter families.
- mutation axis: edits batch under `begin_transaction`/`end_transaction` with `undo`/`redo`; `api.run` carries high-level usecases, while `create_entity`/`add`/`remove` are the primitive verbs.
- tessellation axis: one `geom.settings` knob bag (deflection, `iterator-output`, `use-world-coords`, `generate-uvs`, `length-unit`) plus a `geometry_library` kernel feeds `geom.iterate`/`create_shape`; `geom.has_occ` flags OpenCASCADE availability and falls back to CGAL. The IFC-to-mesh/GLB seam reads `ShapeElementType` verts/faces/materials.
- analysis axis: `util.element` resolves property sets, containment, and decomposition; `util.selector.filter_elements` runs the selector grammar; results stay `entity_instance` values.
- evidence: each model op captures schema version, instance count, edited-entity count, and each tessellation captures element count, vertex count, face count, and kernel as an ifc receipt.
- boundary: ifcopenshell owns IFC parse and tessellation; mesh post-processing routes to `trimesh`, point clouds to `open3d`, glTF authoring to the artifacts owner; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ifcopenshell`
- Owns: IFC read/write, entity mutation and transactions, schema introspection, GUID coding, OpenCASCADE/CGAL tessellation to mesh/GLB, and selector-grammar queries
- Accept: IFC ingestion, mutation, and IFC-to-mesh conversion feeding the ifc and geometry owners
- Reject: wrapper-renames of `open`/`by_type`/`create_shape`; a hand-rolled STEP parser or BREP tessellator where ifcopenshell is admitted; per-key getter families over `by_id`/`by_guid`/`by_type`; identity minting the runtime owns

[CAPTURE_GAP]:
- floor: companion interpreter cp313; `ifcopenshell==0.8.5` ships a prebuilt cp313 wheel, while the `>=3.15` project venv carries no cp315 wheel, so the project-venv `assay api query` resolves no source there
- members: verified by introspection against the installed cp313 distribution; every documented type, entrypoint, submodule, and `settings()` knob resolves — no phantom. `geom.has_occ` is `False` on a wheel-only install; OpenCASCADE links when the full build is present
