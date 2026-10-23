# [BIM]

Covers IFC work through Bonsai and its bundled ifcopenshell.

## [01]-[ROUTES]

Code reaches the open model through `from bonsai import tool`, with `tool.Ifc.get()` returning the `ifcopenshell.file`, `tool.Ifc.get_entity(obj)` an object's element, and `tool.Ifc.get_object(element)` an element's object. With Bonsai keeping IFC file and Blender scene in step through its operators alone, scene-changing work goes through them and IFC-only data through `ifcopenshell.api` on the open file:

| [INDEX] | [TASK]                                                    | [ROUTE]                                                 | [SESSION]         |
| :-----: | :-------------------------------------------------------- | :------------------------------------------------------ | :---------------- |
|  [01]   | New model generated from data, no scene                   | `ifcopenshell.api` on its own file, then `f.write`      | Background        |
|  [02]   | Element creation, deletion, placement, containment, class | Bonsai operators on a loaded model                      | Background or GUI |
|  [03]   | Geometry, types, openings, fills, spaces, joins           | Bonsai operators on a loaded model                      | Background or GUI |
|  [04]   | Property sets, quantities, classification, materials      | `ifcopenshell.api` on `tool.Ifc.get()`                  | Background or GUI |
|  [05]   | Reads and queries                                         | `ifcopenshell.util` on `tool.Ifc.get()` or an open file | Background or GUI |
|  [06]   | Drawings and sheets                                       | Bonsai drawing operators on a saved IFC                 | Background or GUI |
|  [07]   | Schedules, IDS checks, diffs, patches, geometry export    | Bundled libraries on an open file                       | Background        |

Operator and API results follow host rules:
- Operator bodies that raise print `Operation partially completed`
- API edits on the open file reach saved IFC and leave scene as found, new elements getting no object and placements moving none
- Blender coordinates and Bonsai operator arguments are meters
- Attribute and layer values take file units, a meter value divided by `ifcopenshell.util.unit.calculate_unit_scale(f)`
- Bonsai panels hold cached data until `bonsai.bim.handler.refresh_ui_data()` runs

## [02]-[PROJECT]

`bim.new_project(preset=)` reads the home file, then creates the project, `metric_m`, `metric_mm`, and `imperial_ft` making IFC4 projects and `wizard` stopping for settings before `bim.create_project()`:

```python
from bonsai import tool
import bpy

bpy.ops.bim.new_project(preset="wizard")
with bpy.context.temp_override(window=bpy.context.window_manager.windows[0]):
    tool.Project.get_project_props().export_schema = "IFC4X3_ADD2"
    bpy.context.scene.unit_settings.system, bpy.context.scene.unit_settings.length_unit = "IMPERIAL", "FEET"
    bim = tool.Blender.get_bim_props()
    bim.area_unit, bim.volume_unit = "square foot", "cubic foot"
    bpy.ops.bim.create_project()
    building = tool.Ifc.get().by_type("IfcBuilding")[0]
    bpy.ops.bim.add_part_to_object(part_class="IfcBuildingStorey", part_name="<Storey>", element=building.id())
    storey = next(s for s in tool.Ifc.get().by_type("IfcBuildingStorey") if s.Name == "<Storey>")
    tool.Ifc.get_object(storey).location.z = <elevation_m>
    tool.Spatial.set_default_container(storey)
```

- `bim.new_project` and a fresh-session `bim.load_project` replace the open file with no prompt
- Projects hold `My Site`, `My Building`, and `My Storey`, the storey as default container
- `tool.Spatial.set_default_container(storey)` sets the container new elements join
- Storey height sits in the placement, and consumers reading `Elevation` take an `edit_attributes` write in file units
- API-authored files for later Bonsai edits hold the Model Body `MODEL_VIEW`, Model Profile `ELEVATION_VIEW`, and Plan Axis `GRAPH_VIEW` contexts
- `unit.assign_unit` with no arguments sets millimeters, and `{"is_metric": False, "raw": "FEET"}` as `length`, `area`, and `volume` sets feet

## [03]-[OPEN]

`bim.load_project(filepath=)` loads an IFC into the scene:
- `should_start_fresh_session=True` (default) reads the home file first, and `False` loads into the open file
- Objects take the name `<IfcClass>/<Name>`, spatial elements load as empties, and each element sits in its container's collection
- Openings load with no object, and selection is empty after a load
- Non-startup scenes set `tool.Project.get_project_props().geometry_library = "opencascade"` before a load, the factory kernel failing every load
- `bim.reload_ifc_file(filepath=)` brings in edits another tool made on disk, matching by `GlobalId`
- `bim.link_ifc(filepath=, directory=)` references another model read-only through a cache `.blend` beside it

Reopening a `.blend` saved beside a loaded IFC reloads the IFC from `BIMProperties.ifc_file` and relinks the saved objects.

## [04]-[WALLS_AND_SLABS]

Layered walls and slabs come from a type and `bim.add_occurrence`, each body built from the type's layers, model props, and 3D cursor:

```python
import math

from bonsai import tool
import bpy
import ifcopenshell.api.material
import ifcopenshell.util.element
import ifcopenshell.util.unit

f = tool.Ifc.get()
bpy.ops.bim.add_default_type(ifc_element_type="IfcWallType")
wall_type = f.by_type("IfcWallType")[-1]
layer = ifcopenshell.util.element.get_material(wall_type).MaterialLayers[0]
ifcopenshell.api.material.edit_layer(f, layer=layer, attributes={"LayerThickness": <thickness_m> / ifcopenshell.util.unit.calculate_unit_scale(f)})
model = tool.Model.get_model_props()
model.length, model.extrusion_depth = <length_m>, <height_m>
for obj in bpy.context.selected_objects:
    obj.select_set(False)
bpy.context.scene.cursor.location = (<x>, <y>, 0.0)
bpy.ops.bim.add_occurrence(relating_type_id=wall_type.id())
wall = bpy.context.view_layer.objects.active
wall.rotation_euler.z = math.radians(<degrees>)
bpy.ops.bim.edit_object_placement(obj=wall.name)
```

- Layer thickness goes on the type before any occurrence exists, and `IfcSlabType` follows the same form
- Occurrences with nothing selected join the default container, walls starting at the cursor along +X
- Elements selected before `add_occurrence` lend their container to the new occurrence
- `bim.edit_object_placement(obj=)` writes an object transform to the IFC at once
- Names go through `ifcopenshell.api.attribute.edit_attributes`, then `tool.Root.set_object_name(obj, element)`
- `bim.change_layer_length(length=)` and `bim.change_extrusion_depth(depth=)` resize the selected walls
- Wall pairs, the second active, take `bim.join_walls_intersection()` for an L corner, the join held in `ConnectedTo` and `ConnectedFrom`
- Closed loops of joined walls take `bim.draw_slab_from_wall()` with `model.relating_type_id` set to the slab type id string
- `bim.draw_slab_from_wall()` runs under an override holding `window`, `area`, `region`, and `space_data` of a `VIEW_3D` area
- With `bim.assign_type` of a layered type replacing a custom body with the type's extrusion, custom slab outlines go through the profile editor

Slab outlines change through the profile editor in object space:

```python
import bmesh
import bpy

bpy.context.view_layer.objects.active = <slab object>
bpy.ops.bim.enable_editing_extrusion_profile()
bm = bmesh.from_edit_mesh(<slab object>.data)
bmesh.ops.delete(bm, geom=list(bm.verts), context="VERTS")
ring = [bm.verts.new((x, y, 0.0)) for x, y in <outline>]
for a, b in zip(ring, ring[1:] + ring[:1]):
    bm.edges.new((a, b))
bmesh.update_edit_mesh(<slab object>.data)
bpy.ops.bim.edit_extrusion_profile()
```

## [05]-[OPENINGS_AND_FILLS]

Doors and windows come from a type with representation maps and fill a wall that is the only selected object:

```python
from pathlib import Path

from bonsai import tool
from bonsai.bim.ifc import IfcStore
import bpy

library = Path(tool.__file__).parents[1] / "bim" / "data" / "libraries" / f"{tool.Ifc.get().schema} Demo Library.ifc"
bpy.ops.bim.select_library_file(filepath=str(library))
source = next(t for t in IfcStore.library_file.by_type("IfcDoorType") if t.RepresentationMaps)
bpy.ops.bim.append_library_element(definition=source.id())
door_type = next(t for t in tool.Ifc.get().by_type("IfcDoorType") if t.Name == source.Name)
for obj in bpy.context.selected_objects:
    obj.select_set(False)
host = tool.Ifc.get_object(<wall element>)
host.select_set(True)
bpy.context.view_layer.objects.active = host
bpy.context.scene.cursor.location = (<x on wall axis>, <y on wall axis>, host.matrix_world.translation.z)
bpy.ops.bim.add_occurrence(relating_type_id=door_type.id())
```

- Door and window types come from `bim.append_library_element` or `bim.add_default_type`
- `bim.add_default_type` needs the Model Profile `ELEVATION_VIEW` context in API-authored files
- Library ids belong to `IfcStore.library_file` after `bim.select_library_file`, which refuses a library of another schema
- Fills placed with one wall selected cut an `IfcOpeningElement` through it, and fills placed otherwise stand free with no opening
- Fills sit at the cursor height, and walls at their container's elevation, a fill missing the wall cutting nothing
- Free openings come from a wall (active) and a cutter mesh selected together and `bim.add_opening()`

## [06]-[SPACES]

Spaces come from walls or the cursor:
- `bim.generate_spaces_from_walls()` over the bounding walls, one active, makes a space from their inner faces at the active wall's height
- `bim.generate_space()` makes a space from the walls around the 3D cursor
- Spaces aggregate under the default container and sit in the `IfcSpace` collection, `Name` and `LongName` written through `edit_attributes`

## [07]-[CLASSES_AND_TYPES]

`bim.assign_class(obj=, ifc_class=, predefined_type=)` turns an Object Mode object into an element in the default container, applying its modifiers and scale:
- Meshes with faces become `IfcPolygonalFaceSet` bodies
- `bim.update_representation(obj=, ifc_representation_class="IfcExtrudedAreaSolid/IfcArbitraryClosedProfileDef")` turns a prism into an extrusion
- `bim.assign_class` skips objects holding an element
- `bim.assign_type(relating_type=<type id>, related_object=<object name>)` maps a type's representation onto the occurrence
- `ifcopenshell.util.element.get_type(element)` and `get_types(type)` read both directions
- `bim.override_object_delete(confirm=False)` deletes selected objects with their elements
- `bpy.data.objects.remove` leaves the element in the IFC

## [08]-[PROPERTIES]

Property sets, quantities, classification, attributes, and materials go through `ifcopenshell.api` on the open file:

```python
from bonsai import tool
import ifcopenshell.api.classification
import ifcopenshell.api.pset

f = tool.Ifc.get()
pset = ifcopenshell.api.pset.add_pset(f, product=<element>, name="Pset_WallCommon")
ifcopenshell.api.pset.edit_pset(f, pset=pset, properties={"IsExternal": True, "FireRating": "<rating>"})
system = ifcopenshell.api.classification.add_classification(f, classification="<system>")
ifcopenshell.api.classification.add_reference(f, products=[<element>], identification="<code>", name="<title>", classification=system)
```

- Standard sets type values from their template and drop a value of the wrong Python type with no raise
- Custom sets type Python values as `IfcLabel`, `IfcInteger`, `IfcReal`, and `IfcBoolean`
- `bim.edit_pset(obj=, obj_type="Object", pset_id=, properties=<json>)` writes the same values as one Bonsai undo step
- `bim.perform_quantity_take_off()` writes `IFC4QtoBaseQuantities` sets for the selected objects
- `bim.edit_attributes`, `bim.add_classification`, `bim.assign_material`, and `bim.export_ifccsv` read panel properties in place of arguments
- Scripts write attributes, classifications, materials, and CSV exports through the API and bundled libraries

## [09]-[QUERIES]

Reads use `ifcopenshell.util` on the open file:
- `ifcopenshell.util.selector.filter_elements(f, "<query>")` filters by class and property (`Pset_WallCommon.IsExternal=TRUE`)
- Queries filter by `type=`, `material=`, `classification=`, and `location="<storey>"`
- `ifcopenshell.util.selector.get_element_value(element, "<path>")` reads one value (`Pset_WallCommon.FireRating`, `type.Name`, `container.Name`)
- `ifcopenshell.util.element` reads `get_container`, `get_aggregate`, `get_decomposition`, `get_material`, and `get_filled_void`
- `ifcopenshell.util.element.get_psets(element)` merges type sets under occurrence values
- `bim.select_query_elements(query=)` adds matches to the selection
- `obj.BIMObjectProperties.ifc_definition_id` holds an object's element step id

## [10]-[SAVE]

`bim.save_project` syncs every moved object's transform, then writes the IFC:
- `filepath=` saves as that path and makes it the open file's path, `.ifczip` and `.ifcjson` exporting beside the `.ifc` path
- Paired `.blend` files take `bpy.ops.wm.save_mainfile()` after the IFC save
- Placements in a session go on the object, an object moved after an API placement edit winning on save

## [11]-[DRAWINGS]

Drawings are SVG views cut from the model through a Bonsai camera:

```python
from bonsai import tool
import bpy

docs = bpy.context.scene.DocProperties
docs.target_view = "PLAN_VIEW"
docs.location_hint = str(<storey>.id())
bpy.context.scene.cursor.location = (<x>, <y>, 0.0)
cameras = {obj for obj in bpy.data.objects if obj.type == "CAMERA"}
bpy.ops.bim.add_drawing()
camera = next(obj for obj in bpy.data.objects if obj.type == "CAMERA" and obj not in cameras)
bpy.ops.bim.activate_drawing(drawing=tool.Ifc.get_entity(camera).id(), should_view_from_camera=False)
settings = camera.data.BIMCameraProperties
settings.diagram_scale = '1/4"=1\'-0"|1/48'
settings.width, settings.height = <width_m>, <height_m>
bpy.ops.bim.create_drawing()
```

- `target_view` takes `PLAN_VIEW`, `ELEVATION_VIEW`, `SECTION_VIEW`, `REFLECTED_PLAN_VIEW`, or `MODEL_VIEW`
- Plans take their storey id as `location_hint`, placing the camera 1.6 m above the storey
- Elevations and sections take `NORTH`, `SOUTH`, `EAST`, or `WEST` at the cursor
- `bim.activate_drawing` swaps the camera's data-block and hides IFC objects outside its view volume
- Camera settings go through `camera.data` read after activation
- `diagram_scale` items follow the scene's unit system (`1/4"=1'-0"|1/48`, `1:100|1/100`)
- `bim.create_drawing()` writes `drawings/<name>.svg` beside the IFC, pages measuring `width / N` by `height / N`
- Plan SVGs omit doors with an `IfcGeometricCurveSet` Plan Body, Bonsai parametric and library doors both
- SVG elements name their IFC element in `ifc:name`, and `class` values holding `cut` mark cut linework

Sheets follow saved drawings:

```python
import bpy

docs = bpy.context.scene.DocProperties
bpy.ops.bim.save_project()
bpy.ops.bim.load_drawings()
bpy.ops.bim.toggle_target_view(toggle_all=True, option="EXPAND")
bpy.ops.bim.add_sheet()
bpy.ops.bim.load_sheets()
docs.active_sheet_index = next(i for i, item in enumerate(docs.sheets) if item.is_sheet)
for index in [i for i, item in enumerate(docs.drawings) if item.is_drawing]:
    docs.active_drawing_index = index
    bpy.ops.bim.add_drawing_to_sheet()
bpy.ops.bim.create_sheets()
```

`bim.create_sheets` writes `sheets/<id> - <name>.svg` and a PDF through the `svg2pdf_command` preference running Inkscape. Sheet operators poll False until the IFC is saved.

## [12]-[EXCHANGE]

IFC is the file other BIM tools open, and other forms come from the bundled libraries:
- CSV schedules: `ifccsv.IfcCsv().export(f, elements, ["Name", "type.Name", "<Set>.<Property>"], output=<csv>, format="csv")`
- IDS checks: `ifctester.ids.open(<ids>)`, then `.validate(f)` and `ifctester.reporter.Json(ids).report()`
- Schema validation: `ifcopenshell.validate.validate(f, ifcopenshell.validate.json_logger())`
- Model diffs: `ifcdiff.IfcDiff(old, new).diff()`, then `added_elements`, `deleted_elements`, and `change_register`
- Patches: `ifcpatch.write(ifcpatch.execute({"input": <path>, "file": f, "recipe": "<Recipe>", "arguments": [...]}), <out>)`
- Patch recipes: `ifcpatch/recipes/`
- Geometry as glTF, OBJ, or STEP: `ifcopenshell.geom.serializers.gltf`, `obj`, or `step`
- Serializer input: `ifcopenshell.geom.iterator(settings, f, geometry_library="opencascade")`
- Serializer writes: each `iterator.get()` between `writeHeader()` and `finalize()`, the file complete once the serializer is released
- IFC from other formats: import into Blender, then `bim.assign_class` per object

## [13]-[GUI_SESSION]

GUI sessions run the same sequences and add window state:
- Bonsai operators push an undo step called as `bpy.ops.bim.<op>("EXEC_DEFAULT", True, ...)`, and undoing it reverts IFC and scene together
- Code reverts a Bonsai edit through the inverse operator (`bim.override_object_delete`, `bim.edit_pset` with old values)
- With `ed.undo` from code crashing the GUI, Ctrl+Z stays with the user
- `bim.activate_drawing` at its default `should_view_from_camera=True` turns the first 3D view into the drawing camera's view
- `bim.set_tab(tab="<TAB>")` runs under a Properties area override
- `draw_*` tools (`bim.draw_polyline_wall`, `bim.draw_polyline_slab`) wait for mouse input
- Invoked `draw_*` tools end at a file load or by Esc
