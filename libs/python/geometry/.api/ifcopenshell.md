# [PY_GEOMETRY_API_IFCOPENSHELL]

`ifcopenshell` owns the IFC model and tessellation surface the geometry `ifc` rail binds: an in-memory `file` model over SPF/sqlite/streamed backends, entity authoring through the `ifcopenshell.api.<module>.<action>` usecase namespace, `util` read-side analysis, and the OpenCASCADE/CGAL `geom` tessellation daemon. It is the spine every IfcOpenShell-ecosystem worker composes against. STEP parsing, the authoring usecase vocabulary, and BREP tessellation stay here; no consumer re-implements them.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifcopenshell`
- package: `ifcopenshell` (LGPL-3.0)
- import: `import ifcopenshell`
- owner: `geometry`
- rail: ifc
- entry points: none (library only)
- capability: IFC2X3/IFC4/IFC4X3 read/write, `ifcopenshell.api.<module>.<action>` authoring dispatch, entity mutation, transactional undo/redo, GUID codec, placement and unit math, schema introspection, OpenCASCADE/CGAL tessellation to verts/faces/materials, parallel whole-model meshing, GLB/OBJ/XML serialization, and selector-grammar element queries

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: model and entity roots

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [CAPABILITY]                                           |
| :-----: | :---------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `file`                  | model root     | in-memory IFC model with query, mutation, transactions |
|  [02]   | `entity_instance`       | entity wrapper | attribute/inverse access for one IFC instance          |
|  [03]   | `sqlite`                | model backend  | sqlite-backed IFC model for large files                |
|  [04]   | `stream`                | model backend  | streamed SPF model with lazy instance access           |
|  [05]   | `Error` / `SchemaError` | exception      | parse and schema-resolution failures                   |

[PUBLIC_TYPE_SCOPE]: tessellation types (`ifcopenshell.geom`)

`geom.create_shape` returns an `Element` whose representation discriminates on output mode into one of the two shape carriers below.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]      | [CAPABILITY]                                                                       |
| :-----: | :------------------------- | :----------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `geom.settings`            | tessellation knobs | deflection, precision, dimensionality, placement, material, UV, and opening policy |
|  [02]   | `geom.serializer_settings` | serializer knobs   | GLB/OBJ/XML serializer configuration                                               |
|  [03]   | `geom.iterator`            | mesh daemon        | multi-threaded whole-model tessellation iterator                                   |
|  [04]   | `geom.tree`                | spatial index      | bounding-box/clash spatial query tree                                              |
|  [05]   | `BRepElement`              | shape result       | `Element` carrying an OCC BRep representation                                      |
|  [06]   | `TriangulationElement`     | shape result       | `Element` with a `Triangulation` (verts/faces/normals/materials)                   |
|  [07]   | `geom.serializers`         | serializer set     | GLB/OBJ/XML/SVG mesh serializers                                                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model open, query, and mutate

`open` returns a `file`, `sqlite`, or `stream` discriminated by `format`/`should_stream`; query rows accept an id, GUID, or type string and return one or many `entity_instance` values.

| [INDEX] | [SURFACE]                                                   | [CAPABILITY]                                                |
| :-----: | :---------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `ifcopenshell.open`                                         | open SPF/sqlite/streamed model under a format/stream policy |
|  [02]   | `ifcopenshell.file`                                         | construct or wrap an in-memory model (optional schema/path) |
|  [03]   | `ifcopenshell.create_entity(type, schema="IFC4", ...)`      | construct a standalone entity (schema defaults `IFC4`)      |
|  [04]   | `ifcopenshell.schema_by_name`                               | resolve a schema definition by name or version              |
|  [05]   | `ifcopenshell.guess_format`                                 | detect IFC backend format from a path                       |
|  [06]   | `ifcopenshell.register_schema(SchemaClass)`                 | register a custom EXPRESS schema                            |
|  [07]   | `file.by_type`                                              | all instances of an entity type                             |
|  [08]   | `file.by_id`                                                | one instance by step id                                     |
|  [09]   | `file.by_guid`                                              | one instance by IFC GlobalId GUID                           |
|  [10]   | `file.create_entity`                                        | add a new entity (type plus attributes)                     |
|  [11]   | `file.add`                                                  | insert an entity, cross-model copy policy                   |
|  [12]   | `file.remove`                                               | delete an entity                                            |
|  [13]   | `file.traverse(inst, max_levels=None, breadth_first=False)` | dependent-entity graph walk; `max_levels` bounds depth      |
|  [14]   | `file.get_inverse`                                          | inverse-referencing instances (overloaded on entity)        |
|  [15]   | `file.begin_transaction` / `end_transaction`                | open/close an undoable edit batch                           |
|  [16]   | `file.undo` / `redo` / `discard_transaction`                | step the transaction stack                                  |
|  [17]   | `file.write(path, format=None, zipped=False)`               | serialize the model                                         |
|  [18]   | `file.from_string`                                          | parse a model from an in-memory SPF string (static)         |
|  [19]   | `entity_instance.is_a(type=None)`                           | answer the class name, or test the SUBTYPE relation         |
|  [20]   | `entity_instance.id()`                                      | the instance's own step id — the stable partition key       |

[ENTRYPOINT_SCOPE]: tessellation and analysis

Tessellation rows consume a `geom.settings` knob bag and a `geom.GEOMETRY_LIBRARY` kernel selector (`opencascade`/`cgal`/`cgal-simple`/`hybrid-cgal-simple-opencascade`), defaulting to `opencascade`; `geom.iterator` takes it as `geometry_library=`.

| [INDEX] | [SURFACE]                                                         | [CAPABILITY]                                               |
| :-----: | :---------------------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `geom.create_shape`                                               | per-element tessellation from settings/instance/kernel     |
|  [02]   | `geom.iterate`                                                    | lazy whole-model mesh generator (model plus threads)       |
|  [03]   | `geom.iterator(settings, model, num_threads, include=, exclude=)` | reusable filtered mesh iterator object                     |
|  [04]   | `geom.serialise`                                                  | serialize geometry to a format (schema plus shape string)  |
|  [05]   | `geom.tree`                                                       | build a spatial/clash query tree                           |
|  [06]   | `guid.new` / `compress` / `expand` / `split`                      | IFC GUID mint and codec; the codec trio is positional-only |
|  [07]   | `validate.validate`                                               | schema-conformance validation (model plus logger)          |

[ENTRYPOINT_SCOPE]: authoring usecase dispatch

`ifcopenshell.api.<module>.<action>(ifc_file, should_run_listeners=True, **settings)` is the high-level authoring surface: `ifc_file` is the target `ifcopenshell.file` and `**settings` carry the action's typed arguments over a closed `module.action` usecase vocabulary. `api.extract_docs(module, usecase)` reads a legacy `Usecase.__init__`/`Usecase.execute` pair most 0.8 usecase modules no longer define — the live argument contract is `inspect.signature` over the wrapped callable, whose `__signature__` the `wrap_usecases` listener shim assigns from the unwrapped usecase — and `add_pre_listener`/`add_post_listener`/`remove_pre_listener`/`remove_post_listener`/`remove_all_listeners` register mutation hooks. Each usecase takes `ifc_file` first-positional then its named arguments. `geometry.add_representation` is the Blender-coupled dispatcher (`bpy.types.Object` operands behind a `try/except ImportError` guard) and never binds host-free; `owner.update_owner_history(file, element)` keys its operand `element`, never `owner_history`.

| [INDEX] | [USECASE]                                                                                        | [CAPABILITY]                          |
| :-----: | :----------------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `root.create_entity(file, ifc_class="IfcBuildingElementProxy", predefined_type=None, name=None)` | mint a typed root entity              |
|  [02]   | `root.remove_product(file, product)`                                                             | remove a product and its dependents   |
|  [03]   | `root.copy_class(file, product)`                                                                 | duplicate an entity in its class      |
|  [04]   | `attribute.edit_attributes(file, product, attributes)`                                           | set direct attribute values           |
|  [05]   | `geometry.add_mesh_representation(file, context, vertices, edges=None, faces=None, …)`           | attach a mesh shape representation    |
|  [06]   | `geometry.add_profile_representation(file, context, profile, depth=1.0, cardinal_point=5, …)`    | attach an extruded-profile shape      |
|  [07]   | `geometry.assign_representation(file, product, representation) -> None`                          | bind a representation to a product    |
|  [08]   | `geometry.edit_object_placement(file, product, matrix)`                                          | set a product's object placement      |
|  [09]   | `context.add_context(file, context_type, …)`                                                     | add a representation context          |
|  [10]   | `unit.add_si_unit(file, unit_type, prefix=None)`                                                 | add an SI unit to the project         |
|  [11]   | `unit.assign_unit(file, units=None)`                                                             | assign units to the `IfcProject`      |
|  [12]   | `pset.add_pset(file, product, name)`                                                             | attach a property set                 |
|  [13]   | `spatial.assign_container(file, products, relating_structure)`                                   | place products (list) in a container  |
|  [14]   | `aggregate.assign_object(file, products, relating_object)`                                       | aggregate products (list) to a parent |
|  [15]   | `material.add_material(file, name=None, category=None)`                                          | create and assign materials           |
|  [16]   | `type.assign_type(file, related_objects, relating_type)`                                         | assign occurrences (list) to a type   |
|  [17]   | `cost.calculate_cost_item_resource_value(file, cost_item)`                                       | roll resource base costs              |
|  [18]   | `georeference.add_georeferencing(file, ifc_class="IfcMapConversion", name="EPSG:3857")`          | mint the map-conversion/CRS pair      |
|  [19]   | `georeference.edit_georeferencing(file, coordinate_operation=None, projected_crs=None)`          | edit both georeference entity dicts   |
|  [20]   | `georeference.remove_georeferencing(file)`                                                       | delete the georeference pair          |
|  [21]   | `georeference.edit_wcs(file, x=0.0, y=0.0, z=0.0, rotation=0.0, is_si=True)`                     | edit the context's WCS false origin   |
|  [22]   | `georeference.edit_true_north(file, true_north=0.0)`                                             | set the coordinate operation's X axis |

[ENTRYPOINT_SCOPE]: `util` analysis namespace

`ifcopenshell.util` is the read-side analysis namespace over a `file`/`entity_instance`; each submodule owns one query concern and returns `entity_instance` values, dicts, or numpy matrices, never a parallel model. `util.geolocation` is the one exception that also writes nothing yet spans both directions of one transform, and it is listed separately below because its entries pair an `auto_*` model-resolved leg with a manual parameter-taking twin.

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

[ENTRYPOINT_SCOPE]: `util.geolocation` map-conversion namespace

The georeference band is pure Python over `IfcMapConversion`/`IfcMapConversionScaled`/`IfcRigidOperation` and the IFC2X3 `ePSet_MapConversion` fallback, so it reads without the native wrapper. `get_helmert_transformation_parameters` is the ONE extraction entry and returns a `HelmertTransformation` `NamedTuple` — `(e, n, h, xaa, xao, scale, factor_x, factor_y, factor_z)` — or `None` where no coordinate operation exists; every `auto_*` entry resolves it internally and returns its input unchanged on `None`, so absence is the identity transform rather than a raise. The manual `xyz2enh`/`enh2xyz`/`local2global`/`global2local` entries take those nine parameters as keyword-defaulted floats, which is exactly the `*parameters` splat the `auto_*` legs perform.

| [INDEX] | [SURFACE]                                                         | [CALL_SHAPE]           | [CAPABILITY]                                |
| :-----: | :---------------------------------------------------------------- | :--------------------- | :------------------------------------------ |
|  [01]   | `get_helmert_transformation_parameters(ifc_file)`                 | model                  | `HelmertTransformation \| None`             |
|  [02]   | `get_crs(ifc_file)`                                               | model                  | `IfcProjectedCRS` attribute dict            |
|  [03]   | `get_wcs(ifc_file)`                                               | model                  | world coordinate system as a 4x4, or `None` |
|  [04]   | `auto_xyz2enh(ifc_file, x, y, z, should_return_in_map_units)`     | model plus local xyz   | local to easting/northing/height            |
|  [05]   | `auto_enh2xyz(ifc_file, e, n, h, is_specified_in_map_units)`      | model plus map coords  | map to local xyz                            |
|  [06]   | `auto_local2global(ifc_file, matrix, should_return_in_map_units)` | model plus 4x4         | local placement matrix to map frame         |
|  [07]   | `auto_global2local(ifc_file, matrix, is_specified_in_map_units)`  | model plus 4x4         | map matrix to local frame                   |
|  [08]   | `auto_z2e(ifc_file, z, should_return_in_map_units)`               | model plus z           | Z coordinate to elevation                   |
|  [09]   | `get_grid_north(ifc_file)` / `get_true_north(ifc_file)`           | model                  | grid and true north angles in degrees       |
|  [10]   | `xaxis2angle(x, y)` / `angle2xaxis(angle)`                        | abscissa/ordinate pair | X-axis direction to angle and back          |
|  [11]   | `yaxis2angle(x, y)` / `angle2yaxis(angle)`                        | abscissa/ordinate pair | Y-axis direction to angle and back          |
|  [12]   | `dms2dd(degrees, minutes, seconds, us)` / `dd2dms(dd, use_us)`    | angle scalars          | sexagesimal and decimal degrees round-trip  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: boundary scope only; module-level import is banned by the manifest import policy.
- model axis: `ifcopenshell.open` is the polymorphic intake — the backend (`file`/`sqlite`/`stream`) discriminates on `format`/`should_stream`, never a per-backend open function. Query routes through `by_id`/`by_guid`/`by_type` on one `file`, never per-key getter families.
- defined-type axis: LIST-of-SELECT members surface as `entity_instance` values — `IfcIndexedPolyCurve.Segments` yields segments answering `is_a("IfcLineIndex")`/`is_a("IfcArcIndex")` with the raw 1-based index run into `Points.CoordList` on `wrappedValue`; an absent OPTIONAL list attribute reads `None`, never `()`.
- mutation axis: edits batch under `begin_transaction`/`end_transaction()` (no `commit=` arg), with `undo`/`redo`/`discard_transaction` stepping the stack. High-level authoring is the direct `ifcopenshell.api.<module>.<action>(ifc_file, **settings)` callable over the closed usecase vocabulary; the per-usecase relating keyword differs per row, so a single generic relating keyword is the deleted form. `file.create_entity`/`add`/`remove` are the primitive verbs underneath.
- georeference axis: `get_helmert_transformation_parameters` is the single extraction seam every conversion reads, so an `IfcMapConversion`, an `IfcMapConversionScaled` with its three scale factors, an `IfcRigidOperation`, and the IFC2X3 `ePSet_MapConversion` all resolve to ONE nine-field `HelmertTransformation` and no consumer branches on schema or coordinate-operation subtype. A `None` return means the model carries no georeference, and every `auto_*` entry answers its input unchanged rather than raising, so the ungeoreferenced case is the identity transform. `should_return_in_map_units`/`is_specified_in_map_units` select whether the scale factor is applied on the way out, so a project-unit and a map-unit consumer share one entry — a caller re-dividing by `scale` outside is the deleted form.
- tessellation axis: one `geom.settings` knob bag (`mesher-linear-deflection`, `mesher-angular-deflection`, `precision`, `dimensionality`, `weld-vertices`, `use-world-coords`, `apply-default-materials`, `generate-uvs`, `disable-opening-subtractions`) and a `geometry_library` kernel feed `geom.iterator`/`iterate`/`create_shape`; `serializer_settings.set("use-element-guids", bool)` owns serialized node identity. Iterator `include`/`exclude` are mutually exclusive lists of entity instances or GlobalId strings, so element/type scope binds at construction rather than a post-mesh filter. `geom.has_occ` flags OpenCASCADE and falls back to CGAL. `TriangulationElement` verts/faces/materials feed the mesh/GLB seam, never the `BRepElement`.
- analysis axis: `util.element` resolves property sets, containment, and decomposition; `util.selector.filter_elements` runs the selector grammar; results stay `entity_instance` values.
- evidence: each model result carries schema version, instance count, and edited-entity count; each tessellation result carries element/vertex/face counts and kernel.
- boundary: ifcopenshell owns IFC parse and tessellation; mesh post-processing routes to `trimesh`, point clouds to `open3d`, glTF authoring to the artifacts owner; live UI stays outside.

[STACKING]:
- `ifcpatch`(`.api/ifcpatch.md`): `ifcpatch.execute` reads and returns the `ifcopenshell.file` model root, applying a named recipe transformation over it.
- `ifcdiff`(`.api/ifcdiff.md`): `IfcDiff` diffs an `old`/`new` `file` pair across the relationship axis.
- `ifcclash`(`.api/ifcclash.md`): loads models into `geom.tree` for intersection and clearance clash.
- `ifc4d`(`.api/ifc4d.md`): builds an `IfcWorkSchedule`/`IfcTask` tree over the model through the authoring usecases.
- `ifc5d`(`.api/ifc5d.md`): `qto.quantify` writes `IfcElementQuantity` base quantities over the model.
- `ifctester`(`.api/ifctester.md`): validates a `file` against IDS facets.
- `ifccsv`(`.api/ifccsv.md`): `util.selector.filter_elements` selects the element set and `util.element` reads its attribute/pset values for tabular export.
- `bcf-client`(`.api/bcf-client.md`): `file.by_type`/`by_guid` `entity_instance` GUIDs drive BCF viewpoint selection and visibility.
- `geometry:ifc` owner: composes `ifcopenshell.open`, the `by_id`/`by_guid`/`by_type` query, the direct `ifcopenshell.api.<module>.<action>` authoring callables, and `geom.iterate`/`create_shape` into typed IFC results.

[LOCAL_ADMISSION]:
- `geometry:ifc` owner admits `ifcopenshell.open`, the query/authoring/tessellation surface, and `util` analysis as the ifc rail: a path opens the model, the usecase vocabulary authors it, and `geom` meshes it under a `geometry_library` kernel.

[CAPTURE_GAP]:
- members: every row above verifies by published-surface read, never runtime import — the `ifcopenshell` C extension does not build on darwin/python3.15, the same constraint the `ifccsv` sibling records.
- ABSENT: the EXPRESS reflection under `schema_by_name` — a schema's `declaration_by_name`, an entity declaration's `all_attributes`, and an attribute's `name`/`optional` — carries no row here, so a fence proving a materialized attribute roster against the running schema has no catalogued producer to compose and none is authored from memory.

[RAIL_LAW]:
- Package: `ifcopenshell`
- Owns: IFC2X3/IFC4/IFC4X3 parse and serialization, the `ifcopenshell.api` authoring usecase vocabulary, transactional mutation, and OpenCASCADE/CGAL tessellation to verts/faces/materials.
- Accept: a path or SPF string for `open`; a `geom.settings` knob bag and `geometry_library` kernel for tessellation; the direct usecase callables for authoring, feeding the ifc rail owner.
- Reject: wrapper-renames of `open`/`by_type`/`create_shape`; a per-verb authoring function family over the usecase vocabulary; a hand-rolled STEP parser or BREP tessellator where ifcopenshell is admitted; per-key getter families over `by_id`/`by_guid`/`by_type`; identity minting the runtime GUID codec owns.
