# [PY_GEOMETRY_API_IFCOPENSHELL]

`ifcopenshell` supplies the IFC model and tessellation surface for the geometry ifc rail and is the spine the whole IfcOpenShell-ecosystem companion lane (`ifcpatch`, `ifcdiff`, `ifcclash`, `ifc4d`, `ifc5d`, `ifctester`, `bcf-client`, `ifc5d`) composes against: an in-memory `file` model, an `entity_instance` wrapper, the `open` polymorphic reader across SPF/sqlite/streamed backends, the `api.run(module.action, file, **kwargs)` verb dispatcher for high-level authoring, the `util` analysis namespace, and the `geom` tessellation daemon that drives IFC parse, query, mutation, and IFC-to-mesh/GLB conversion through OpenCASCADE or CGAL kernels with property, quantity, and relationship analysis. The package owner composes `ifcopenshell.open`, `file.by_type`, `api.run`, and `geom.iterate` into the ifc owner; it never re-implements STEP parsing, the authoring verb table, or BREP tessellation ifcopenshell already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifcopenshell`
- package: `ifcopenshell`
- import: `import ifcopenshell`
- owner: `geometry`
- rail: ifc
- spine: the geometry ifc/* pages and the IfcOpenShell companion lane treat `ifcopenshell` as the cp315-core IFC spine per campaign decision; the companion packages ride it and resolve where it resolves
- installed: `0.8.5` authored from ledger ([04]-sourced; `assay api` resolution blocked by the `opentelemetry-proto` `protobuf>=5,<7` ceiling against the workspace `protobuf>=7.35` floor — a workspace lock conflict, not an interpreter or wheel fault for ifcopenshell); license LGPL-3.0-or-later; ships a prebuilt `py313` (cp313) wheel and carries no cp315 wheel, so it does not import on the cp315 (`3.15.0b2`) core today => designated cp315-core spine, gap reconciled in a later phase
- entry points: none (library only)
- capability: IFC2X3/IFC4/IFC4X3 read/write, `api.run` high-level authoring verb dispatch, entity creation and mutation, transactional undo/redo, GUID encode/decode, placement and unit math, schema introspection, OpenCASCADE/CGAL tessellation to verts/faces/materials, parallel whole-model meshing, GLB/OBJ/XML serialization, and selector-grammar element queries

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: model and entity roots
- rail: ifc

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE] | [CAPABILITY]                                           |
| :-----: | :---------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `file`                  | model root     | in-memory IFC model with query, mutation, transactions |
|  [02]   | `entity_instance`       | entity wrapper | attribute/inverse access for one IFC instance          |
|  [03]   | `sqlite`                | model backend  | sqlite-backed IFC model for large files                |
|  [04]   | `stream`                | model backend  | streamed SPF model with lazy instance access           |
|  [05]   | `Error` / `SchemaError` | fault          | parse and schema-resolution failures                   |

[PUBLIC_TYPE_SCOPE]: tessellation types (`ifcopenshell.geom`)
- rail: ifc

| [INDEX] | [SYMBOL]                                                                        | [PACKAGE_ROLE]     | [CAPABILITY]                                                                                                                                             |
| :-----: | :------------------------------------------------------------------------------ | :----------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `geom.settings`                                                                 | tessellation knobs | deflection/units/UV/output mode knob bag                                                                                                                 |
|  [02]   | `geom.serializer_settings`                                                      | serializer knobs   | GLB/OBJ/XML serializer configuration                                                                                                                     |
|  [03]   | `geom.iterator`                                                                 | mesh daemon        | multi-threaded whole-model tessellation iterator                                                                                                         |
|  [04]   | `geom.tree`                                                                     | spatial index      | bounding-box/clash spatial query tree                                                                                                                    |
|  [05]   | `geom.create_shape` result (`Element` / `BRepElement` / `TriangulationElement`) | shape result       | per-element shape: `BRepElement` carries an OCC BRep representation, `TriangulationElement` carries a `Triangulation` with verts/faces/normals/materials |
|  [06]   | `geom.serializers`                                                              | serializer set     | GLB/OBJ/XML/SVG mesh serializers                                                                                                                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model open, query, and mutate
- rail: ifc

`open` returns a `file`, `sqlite`, or `stream` discriminated by `format`/`should_stream`; query rows accept an id, GUID, or type string and return one or many `entity_instance` values.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                   | [CAPABILITY]                         |
| :-----: | :---------------------------- | :----------------------------- | :----------------------------------- |
|  [01]   | `ifcopenshell.open`           | path plus format/stream policy | open SPF/sqlite/streamed model       |
|  [02]   | `ifcopenshell.file`           | optional schema/path           | construct or wrap an in-memory model |
|  [03]   | `ifcopenshell.create_entity`  | type plus schema plus attrs    | construct a standalone entity        |
|  [04]   | `ifcopenshell.schema_by_name` | schema name or version         | resolve a schema definition          |
|  [05]   | `ifcopenshell.guess_format`   | path                           | detect IFC backend format            |
|  [06]   | `file.by_type`                | type string                    | all instances of an entity type      |
|  [07]   | `file.by_id`                  | step id                        | one instance by step id              |
|  [08]   | `file.by_guid`                | GlobalId GUID                  | one instance by IFC GUID             |
|  [09]   | `file.create_entity`          | type plus attributes           | add a new entity to the model        |
|  [10]   | `file.add`                    | entity plus copy policy        | insert an entity (cross-model copy)  |
|  [11]   | `file.remove`                 | entity                         | delete an entity                     |
|  [12]   | `file.traverse`               | entity plus depth              | dependent-entity graph walk          |
|  [13]   | `file.get_inverse`            | entity                         | inverse-referencing instances        |
|  [14]   | `file.begin_transaction`      | none                           | start an undoable edit batch         |
|  [15]   | `file.write`                  | path                           | serialize the model                  |

[ENTRYPOINT_SCOPE]: tessellation and analysis
- rail: ifc

Tessellation rows consume a `geom.settings` knob bag and a `geom.GEOMETRY_LIBRARY` kernel selector (`opencascade`/`cgal`/`cgal-simple`/`hybrid-cgal-simple-opencascade`).

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                       | [CAPABILITY]                     |
| :-----: | :--------------------------- | :--------------------------------- | :------------------------------- |
|  [01]   | `geom.create_shape`          | settings plus instance plus kernel | per-element tessellation         |
|  [02]   | `geom.iterate`               | settings plus model plus threads   | lazy whole-model mesh generator  |
|  [03]   | `geom.iterator`              | settings plus model plus threads   | reusable mesh iterator object    |
|  [04]   | `geom.serialise`             | schema plus shape string           | serialize geometry to a format   |
|  [05]   | `geom.tree`                  | model plus settings                | build a spatial/clash query tree |
|  [06]   | `guid.new` / `guid.compress` | none or expanded GUID              | IFC GUID mint and encode/decode  |
|  [07]   | `validate.validate`          | model plus logger                  | schema-conformance validation    |

[ENTRYPOINT_SCOPE]: `api.run` authoring verb dispatch
- rail: ifc

`ifcopenshell.api.run(usecase_path, ifc_file=None, should_run_listeners=True, **settings)` is the legacy high-level authoring entry: `usecase_path` is a dotted `module.action` name that routes into the `ifcopenshell.api.<module>.<action>` usecase package, `ifc_file` is the target `ifcopenshell.file`, and `**settings` carry the action's typed arguments. It is one polymorphic dispatcher over a closed module/action vocabulary, never a per-verb authoring function family; adding an authoring operation is one dotted usecase name. In 0.8.5 `api.run` is deprecated ("Do not use this function") in favor of calling `ifcopenshell.api.<module>.<action>(file, **kwargs)` directly — the same closed module/action vocabulary, now the canonical contract, with `api.extract_docs`/`wrap_usecase` and the pre/post listener registration the surrounding surface. The ifc owner composes the direct `module.action` callables; the `api.run` row below documents the equivalent dotted vocabulary.

| [INDEX] | [USECASE]                                      | [CALL_SHAPE]                            | [CAPABILITY]                                         |
| :-----: | :--------------------------------------------- | :-------------------------------------- | :--------------------------------------------------- |
|  [01]   | `api.run("root.create_entity", …)`             | `ifc_class` plus `predefined_type`/name | mint a typed root entity (the canonical create verb) |
|  [02]   | `api.run("root.remove_product", …)`            | `product`                               | remove a product and its dependents                  |
|  [03]   | `api.run("root.copy_class", …)`                | `product`                               | duplicate an entity within its class                 |
|  [04]   | `api.run("attribute.edit_attributes", …)`      | `product` plus `attributes`             | set direct attribute values                          |
|  [05]   | `api.run("geometry.add_representation", …)`    | `context` plus geometry source          | attach a shape representation to a product           |
|  [06]   | `api.run("geometry.edit_object_placement", …)` | `product` plus `matrix`                 | set a product's object placement                     |
|  [07]   | `api.run("context.add_context", …)`            | `context_type`/`context_identifier`     | add a geometric representation context               |
|  [08]   | `api.run("unit.add_si_unit", …)`               | `unit_type` plus `prefix`               | add an SI unit to the project unit assignment        |
|  [09]   | `api.run("unit.assign_unit", …)`               | `units`                                 | assign units to the `IfcProject`                     |
|  [10]   | `api.run("pset.add_pset", …)`                  | `product` plus `name`                   | attach a property set, then `pset.edit_pset`         |
|  [11]   | `api.run("spatial.assign_container", …)`       | `products` plus `relating_structure`    | place products in a spatial container                |
|  [12]   | `api.run("aggregate.assign_object", …)`        | `products` plus `relating_object`       | aggregate products under a parent                    |
|  [13]   | `api.run("material.add_material", …)`          | `name` plus `category`                  | create and assign materials                          |
|  [14]   | `api.run("type.assign_type", …)`               | `related_objects` plus `relating_type`  | assign an occurrence to a type                       |

[ENTRYPOINT_SCOPE]: `util` analysis namespace
- rail: ifc

`ifcopenshell.util` is the read-side analysis namespace over a `file`/`entity_instance`; each submodule owns one query concern and returns `entity_instance` values, dicts, or numpy matrices, never a parallel model.

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]              | [CAPABILITY]                           |
| :-----: | :----------------------------------- | :------------------------ | :------------------------------------- |
|  [01]   | `util.element.get_psets`             | element                   | property and quantity sets as a dict   |
|  [02]   | `util.element.get_type`              | element                   | the element's type object              |
|  [03]   | `util.element.get_container`         | element                   | spatial container of an element        |
|  [04]   | `util.element.get_decomposition`     | element                   | aggregated and contained parts         |
|  [05]   | `util.placement.get_local_placement` | placement                 | local placement as a 4x4 numpy matrix  |
|  [06]   | `util.selector.filter_elements`      | model plus query string   | selector-grammar element filter        |
|  [07]   | `util.selector.get_element_value`    | element plus query string | a queried attribute/pset value         |
|  [08]   | `util.unit.calculate_unit_scale`     | model                     | project-to-SI length unit scale factor |
|  [09]   | `util.unit.get_project_unit`         | model plus unit type      | the assigned project unit for a type   |
|  [10]   | `util.shape.get_vertices`            | geometry shape            | shape vertices as a numpy array        |

## [04]-[IMPLEMENTATION_LAW]

[IFC_TESSELLATION]:
- import: `import ifcopenshell` at boundary scope only; module-level import is banned by the manifest import policy.
- model axis: `ifcopenshell.open` is the polymorphic intake; the backend (`file`, `sqlite`, `stream`) discriminates on `format`/`should_stream`, never a parallel open function per backend. Query routes through `by_id`/`by_guid`/`by_type` on one `file`, never per-key getter families.
- mutation axis: edits batch under `begin_transaction`/`end_transaction` with `undo`/`redo`. `api.run(module.action, file, **kwargs)` is the one high-level authoring dispatcher over the closed `module.action` usecase vocabulary (`root.create_entity`, `attribute.edit_attributes`, `geometry.add_representation`, `unit.add_si_unit`, `pset.add_pset`, `spatial.assign_container`, `aggregate.assign_object`, `material.add_material`, `type.assign_type`, …), never a per-verb authoring function family; `file.create_entity`/`add`/`remove` are the primitive low-level verbs underneath it.
- tessellation axis: one `geom.settings` knob bag (deflection, `iterator-output`, `use-world-coords`, `generate-uvs`, `length-unit`) plus a `geometry_library` kernel feeds `geom.iterate`/`create_shape`; `geom.has_occ` flags OpenCASCADE availability and falls back to CGAL. `geom.create_shape` returns an `Element` whose representation discriminates on output mode: a `TriangulationElement` exposes a `Triangulation` carrying verts/faces/normals/materials, while a `BRepElement` exposes an OCC BRep; the IFC-to-mesh/GLB seam reads the `TriangulationElement` verts/faces/materials.
- analysis axis: `util.element` resolves property sets, containment, and decomposition; `util.selector.filter_elements` runs the selector grammar; results stay `entity_instance` values.
- evidence: each model op captures schema version, instance count, edited-entity count, and each tessellation captures element count, vertex count, face count, and kernel as an ifc receipt.
- boundary: ifcopenshell owns IFC parse and tessellation; mesh post-processing routes to `trimesh`, point clouds to `open3d`, glTF authoring to the artifacts owner; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ifcopenshell`
- Owns: IFC read/write, the `api.run` authoring verb dispatch, entity mutation and transactions, schema introspection, GUID coding, placement/unit math, OpenCASCADE/CGAL tessellation to mesh/GLB, and selector-grammar queries — the cp315-core IFC spine the companion lane composes against
- Accept: IFC ingestion, `api.run` authoring, mutation, and IFC-to-mesh conversion feeding the ifc and geometry owners and the IfcOpenShell companion lane
- Reject: wrapper-renames of `open`/`by_type`/`create_shape`; a per-verb authoring function family over the `api.run` `module.action` row; a hand-rolled STEP parser or BREP tessellator where ifcopenshell is admitted; per-key getter families over `by_id`/`by_guid`/`by_type`; identity minting the runtime owns

[CAPTURE_GAP]:
- floor: `ifcopenshell==0.8.5` ships a prebuilt `py313` (cp313) wheel and no cp315 wheel, so it does not import on the cp315 (`3.15.0b2`) core today; `assay api` additionally resolves no source because the workspace lock is unsatisfiable under the `opentelemetry-proto` `protobuf>=5,<7` ceiling against the `protobuf>=7.35` floor — a workspace conflict, not an ifcopenshell fault. The campaign nonetheless designates it the cp315-core IFC spine; a later phase reconciles the wheel/lock gap and does not downgrade the spine role.
- members: the `file`/`entity_instance`/`sqlite`/`stream` model roots, the `geom.settings`/`iterator`/`tree` tessellation types and the `geom.create_shape` result (`Element`/`BRepElement`/`TriangulationElement`), the `open`/`by_id`/`by_guid`/`by_type`/`create_entity`/`add`/`remove`/`traverse` model verbs, the `api.run(module.action, file, **kwargs)` dispatch and its `root.create_entity`/`geometry`/`unit`/`pset`/`spatial` usecase names, and the `util.element`/`placement`/`selector`/`unit`/`shape` analysis namespace are stated from the IfcOpenShell 0.8.5 surface and confirm by introspection once the spine resolves on a supported interpreter. `geom.has_occ` is `False` on a wheel-only install; OpenCASCADE links when the full build is present.
