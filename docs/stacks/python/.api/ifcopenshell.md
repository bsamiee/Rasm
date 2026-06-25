# [PY_GEOMETRY_API_IFCOPENSHELL]

`ifcopenshell` supplies the IFC model and tessellation surface for the geometry ifc rail and is the spine the whole IfcOpenShell-ecosystem companion lane (`ifcpatch`, `ifcdiff`, `ifcclash`, `ifc4d`, `ifc5d`, `ifctester`, `bcf-client`) composes against: an in-memory `file` model, an `entity_instance` wrapper, the `open` polymorphic reader across SPF/sqlite/streamed backends, the `api.run(module.action, file, **kwargs)` verb dispatcher for high-level authoring, the `util` analysis namespace, and the `geom` tessellation daemon that drives IFC parse, query, mutation, and IFC-to-mesh/GLB conversion through OpenCASCADE or CGAL kernels with property, quantity, and relationship analysis. The package owner composes `ifcopenshell.open`, `file.by_type`, `api.run`, and `geom.iterate` into the ifc owner; it never re-implements STEP parsing, the authoring verb table, or BREP tessellation ifcopenshell already owns.

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
|  [03]   | `ifcopenshell.create_entity`  | `(type, schema="IFC4", *args, **kwargs)` | construct a standalone entity (schema defaults to `IFC4`) |
|  [04]   | `ifcopenshell.schema_by_name` | schema name or version         | resolve a schema definition          |
|  [05]   | `ifcopenshell.guess_format`   | path                           | detect IFC backend format            |
|  [05b]  | `ifcopenshell.register_schema`| `SchemaClass`                  | register a custom EXPRESS schema      |
|  [06]   | `file.by_type`                | type string                    | all instances of an entity type      |
|  [07]   | `file.by_id`                  | step id                        | one instance by step id              |
|  [08]   | `file.by_guid`                | GlobalId GUID                  | one instance by IFC GUID             |
|  [09]   | `file.create_entity`          | type plus attributes           | add a new entity to the model        |
|  [10]   | `file.add`                    | entity plus copy policy        | insert an entity (cross-model copy)  |
|  [11]   | `file.remove`                 | entity                         | delete an entity                     |
|  [12]   | `file.traverse`               | `(inst, max_levels=None, breadth_first=False)` | dependent-entity graph walk; `max_levels` is the depth bound (`None`/`-1` = infinite), NOT a `depth` kwarg |
|  [13]   | `file.get_inverse`            | entity (overloaded)            | inverse-referencing instances        |
|  [14]   | `file.begin_transaction` / `end_transaction` | none              | open/close an undoable edit batch; `end_transaction()` takes NO `commit=` arg |
|  [15]   | `file.undo` / `redo` / `discard_transaction` | none              | step the transaction stack; rollback is `discard_transaction()`/`undo()`, not a commit flag |
|  [16]   | `file.write`                  | `(path, format=None, zipped=False)` | serialize the model           |
|  [17]   | `file.from_string`            | SPF string                     | parse a model from an in-memory string (static) |

[ENTRYPOINT_SCOPE]: tessellation and analysis
- rail: ifc

Tessellation rows consume a `geom.settings` knob bag and a `geom.GEOMETRY_LIBRARY` kernel selector (`opencascade`/`cgal`/`cgal-simple`/`hybrid-cgal-simple-opencascade`).

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                       | [CAPABILITY]                     |
| :-----: | :--------------------------- | :--------------------------------- | :------------------------------- |
|  [01]   | `geom.create_shape`          | settings plus instance plus kernel | per-element tessellation         |
|  [02]   | `geom.iterate`               | settings plus model plus threads   | lazy whole-model mesh generator  |
|  [03]   | `geom.iterator`              | settings plus model plus threads   | reusable mesh iterator object    |
|  [04]   | `geom.serialise`             | schema plus shape string           | serialize geometry to a format   |
|  [05]   | `geom.tree`                  | model plus settings                | build a spatial/clash query tree; `geometry_library` defaults to `"opencascade"` |
|  [06]   | `guid.new` / `compress` / `expand` / `split` | none or (un)expanded GUID | IFC GUID mint and encode/decode; `compress(uuid, /)`, `expand(guid, /)`, `split(uuid, /)` are positional-only |
|  [07]   | `validate.validate`          | model plus logger                  | schema-conformance validation    |

[ENTRYPOINT_SCOPE]: `api.run` authoring verb dispatch
- rail: ifc

`ifcopenshell.api.run(usecase_path, ifc_file=None, should_run_listeners=True, **settings)` is the legacy high-level authoring entry: `usecase_path` is a dotted `module.action` name that routes into the `ifcopenshell.api.<module>.<action>` usecase package, `ifc_file` is the target `ifcopenshell.file`, and `**settings` carry the action's typed arguments. It is one polymorphic dispatcher over a closed module/action vocabulary, never a per-verb authoring function family; adding an authoring operation is one dotted usecase name. In 0.8.5 `api.run(usecase_path, ifc_file=None, should_run_listeners=True, **settings)` is deprecated ("This is deprecated and will be removed in a future version. Do not use this function.") and delegates to `ifcopenshell.api.<module>.<action>(ifc_file, should_run_listeners=…, **settings)` directly — the same closed module/action vocabulary, now the canonical contract. The surrounding surface is `api.extract_docs(module, usecase)` (a TWO-arg introspection over a module name plus a usecase name, NOT a single dotted recipe), `api.wrap_usecase(usecase_path, usecase)`, and the `add_pre_listener`/`add_post_listener`/`remove_pre_listener`/`remove_post_listener`/`remove_all_listeners` registration. The ifc owner composes the direct `module.action` callables; the `api.run` row below documents the equivalent dotted vocabulary. The per-usecase keyword names below are decompile-verified against the `0.8.5` source: each usecase takes the `ifc_file` first-positional then its named arguments, and the `products`/`relating_structure`/`relating_object`/`related_objects`/`relating_type` keyword spellings DIFFER per usecase — they are table-driven per row, never a single generic relating keyword.

| [INDEX] | [USECASE]                                      | [CALL_SHAPE]                            | [CAPABILITY]                                         |
| :-----: | :--------------------------------------------- | :-------------------------------------- | :--------------------------------------------------- |
|  [01]   | `root.create_entity(file, ifc_class="IfcBuildingElementProxy", predefined_type=None, name=None)` | `ifc_class`/`predefined_type`/`name` | mint a typed root entity (the canonical create verb) |
|  [02]   | `root.remove_product(file, product)`           | `product`                               | remove a product and its dependents                  |
|  [03]   | `root.copy_class(file, product)`               | `product`                               | duplicate an entity within its class                 |
|  [04]   | `attribute.edit_attributes(file, product, attributes)` | `product` plus `attributes`     | set direct attribute values                          |
|  [05]   | `geometry.add_representation(file, context, …)`| `context` plus geometry source          | attach a shape representation to a product           |
|  [06]   | `geometry.edit_object_placement(file, product, matrix)` | `product` plus `matrix`        | set a product's object placement                     |
|  [07]   | `context.add_context(file, context_type, …)`   | `context_type`/`context_identifier`     | add a geometric representation context               |
|  [08]   | `unit.add_si_unit(file, unit_type, prefix=None)` | `unit_type` plus `prefix`             | add an SI unit to the project unit assignment        |
|  [09]   | `unit.assign_unit(file, units=None)`           | `units`                                 | assign units to the `IfcProject`                     |
|  [10]   | `pset.add_pset(file, product, name)`           | `product` plus `name`                   | attach a property set, then `pset.edit_pset`         |
|  [11]   | `spatial.assign_container(file, products, relating_structure)` | `products` (list) plus `relating_structure` | place products in a spatial container |
|  [12]   | `aggregate.assign_object(file, products, relating_object)` | `products` (list) plus `relating_object` | aggregate products under a parent       |
|  [13]   | `material.add_material(file, name=None, category=None)` | `name` plus `category`         | create and assign materials                          |
|  [14]   | `type.assign_type(file, related_objects, relating_type)` | `related_objects` (list) plus `relating_type` | assign occurrences to a type        |
|  [15]   | `cost.calculate_cost_item_resource_value(file, cost_item)` | `cost_item`                   | roll construction-resource base costs into the item value |

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
- mutation axis: edits batch under `begin_transaction`/`end_transaction()` (no `commit=` arg) with `undo`/`redo`/`discard_transaction` stepping the stack — a rollback is `discard_transaction()`/`undo()`, never an `end_transaction(commit=False)`. The direct `ifcopenshell.api.<module>.<action>(ifc_file, **kwargs)` callable (with `api.run("module.action", file, **kwargs)` the deprecated dispatcher) is the one high-level authoring surface over the closed `module.action` usecase vocabulary (`root.create_entity`, `attribute.edit_attributes`, `geometry.add_representation`, `unit.add_si_unit`, `pset.add_pset`, `spatial.assign_container`, `aggregate.assign_object`, `material.add_material`, `type.assign_type`, `cost.calculate_cost_item_resource_value`, …), never a per-verb authoring function family; the per-usecase relating keyword is table-driven and DIFFERS per row (decompile-verified: `spatial.assign_container(products, relating_structure)`, `aggregate.assign_object(products, relating_object)`, `type.assign_type(related_objects, relating_type)` — each takes `ifc_file` first-positional), so a caller threading a single generic relating keyword is the deleted form. `file.create_entity`/`add`/`remove` are the primitive low-level verbs underneath it.
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
- floor: `ifcopenshell==0.8.5` ships prebuilt cp312/cp313/cp314 wheels and no cp315 wheel, so it does not import on the cp315 (`3.15.0b2`) core today; `assay api resolve ifcopenshell` returns no source because the workspace lock is unsatisfiable under the `opentelemetry-proto` `protobuf>=5,<7` ceiling against the `protobuf>=7.35` floor — a workspace conflict, not an ifcopenshell fault. The campaign nonetheless designates it the cp315-core IFC spine; a later phase reconciles the wheel/lock gap and does not downgrade the spine role.
- members: the model roots, the `geom` tessellation types, the `open`/`by_id`/`by_guid`/`by_type`/`create_entity`/`add`/`remove`/`traverse` verbs, the `api.<module>.<action>(ifc_file, **kwargs)` direct callables (and the deprecated `api.run` dispatcher), and the `util.element`/`placement`/`selector`/`unit`/`shape` analysis namespace are decompile-confirmed against the cached `ifcopenshell==0.8.5` cp313 wheel source (`__init__.py`/`file.py`/`entity_instance.py`/`geom/main.py`/`guid.py`/`api/`/`util/`). Confirmed there: `file.traverse(inst, max_levels=None, breadth_first=False)`, `end_transaction()` (no `commit=`), `create_entity(type, schema="IFC4", …)`, `guid.{new,compress,expand,split}`, `geom.GEOMETRY_LIBRARY` literal with `geometry_library="opencascade"` default, and the per-usecase keyword spellings (`spatial.assign_container(products, relating_structure)` / `aggregate.assign_object(products, relating_object)` / `type.assign_type(related_objects, relating_type)`). `geom.has_occ` is evaluated at import and is `False` on a wheel-only install where OpenCASCADE is unlinked; the CGAL/`cgal-simple` kernels remain available.
