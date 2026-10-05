# [BIM]

IFC files hold the model and Blender objects its entities, `ifcopenshell.api` authoring it and Bonsai sessions loading, editing, and drawing it.

## [01]-[AUTHORING]

New projects start from `ifcopenshell.api` through `run` or `call`, lengths in meters with the file unit converting on write:

```python
# [HEADLESS_CALL] New IFC4 project in feet with a wall on its storey, written to <file>.ifc
import ifcopenshell.api as api
import ifcopenshell.util.unit

feet = {"is_metric": False, "raw": "FEET"}
f = api.run("project.create_file", version="IFC4")
project = api.run("root.create_entity", f, ifc_class="IfcProject", name="<project>")
api.run("unit.assign_unit", f, length=feet, area=feet, volume=feet)
model = api.run("context.add_context", f, context_type="Model")
body = api.run("context.add_context", f, context_type="Model", context_identifier="Body", target_view="MODEL_VIEW", parent=model)
site, building, storey = (api.run("root.create_entity", f, ifc_class=c, name=n) for c, n in (("IfcSite", "<site>"), ("IfcBuilding", "<building>"), ("IfcBuildingStorey", "<storey>")))
for parent, child in ((project, site), (site, building), (building, storey)):
    api.run("aggregate.assign_object", f, relating_object=parent, products=[child])
wall = api.run("root.create_entity", f, ifc_class="IfcWall", name="<wall>")
api.run("geometry.assign_representation", f, product=wall, representation=api.run("geometry.add_wall_representation", f, context=body, length=<length>, height=<height>, thickness=<thickness>))
api.run("spatial.assign_container", f, relating_structure=storey, products=[wall])
api.run("geometry.edit_object_placement", f, product=wall)
f.write("<file>.ifc")
result = {"unit_scale": ifcopenshell.util.unit.calculate_unit_scale(f)}
```

- `unit.assign_unit` with no arguments sets millimeters, `feet` as `length`, `area`, and `volume` sets feet, square feet, and cubic feet
- `calculate_unit_scale` answers `0.3048` for feet and `0.001` for millimeters, representation arguments staying meters under either
- `bim.new_project(preset="imperial_ft")` (`metric_m`, `metric_mm` metric) starts a project in a scene other than the startup set
- `bim.new_project` and `bim.create_project` delete every object, mesh, and material in `bpy.data` while the context scene holds one mesh, light, and camera alone (the startup set)

## [02]-[PROJECT]

IFC files load, edit, and save in a session, `should_start_fresh_session` deciding what the load keeps:
1. `headless.py start <file>.blend <name>` opens the session
2. `bim.load_project(filepath=)` loads the IFC as the table reads
3. Object transforms and mesh edits change the model, `bim.update_representation` writing each edited mesh
4. `bim.save_project(filepath=<copy>.ifc)` writes the IFC
5. `headless.py stop <name>`

| [INDEX] | [SHOULD_START_FRESH_SESSION] | [LOAD]                                                                                       |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `True` (default)             | Startup file read first, the file untitled with the IFC objects alone, `stop` saving nothing |
|  [02]   | `False`                      | Into the open file, keeping its objects, path, and Sun Position's `sun_object`               |

```python
# [HEADLESS_CALL] <file>.ifc loaded, <Object> moved and reshaped, saved to <copy>.ifc
import bpy
import bonsai.tool as tool
import ifcopenshell.util.element as element

bpy.ops.bim.load_project(filepath="<file>.ifc")
obj = bpy.data.objects["<Object>"]
obj.location.x += <x>
for vertex in obj.data.vertices:
    vertex.co.z *= <factor>
bpy.ops.bim.update_representation(obj=obj.name, ifc_representation_class="IfcTessellatedFaceSet")
bpy.ops.bim.save_project(filepath="<copy>.ifc")
entity = tool.Ifc.get_entity(obj)
result = {"ifc": bpy.context.scene.BIMProperties.ifc_file, "container": element.get_container(entity).Name, "body": [r.RepresentationType for r in entity.Representation.Representations]}
```

- Loads mesh geometry through the scene's `BIMProjectProperties.geometry_library`, `opencascade` in the startup file loading IFC geometry
- Scenes from outside the startup file take `geometry_library = "opencascade"` before a load into them
- Objects take the name `<IfcClass>/<Name>`, spatial elements as empties, each in its container's collection
- `tool.Ifc.get()` returns the open `ifcopenshell.file`, and `ifcopenshell.util.element` reads an entity's psets (`get_psets`), container, and type
- Object transforms reach `ObjectPlacement` on save
- `IfcTessellatedFaceSet` writes an edited mesh as it stands, where the default class re-detects an extrusion from the profile the mesh keeps
- `bim.save_project` repoints `scene.BIMProperties.ifc_file` at the written file and saves a titled `.blend` holding unsaved changes

## [03]-[DRAWINGS]

Drawings are scaled SVG views cut from the saved IFC model and placed on sheets, one call activating the drawing and a later one writing it:
1. One call adds and activates the drawing, a camera at the origin
2. One later call places and scales the camera, writes the drawing, and sets it on a new sheet

```python
# [HEADLESS_CALL] <VIEW> drawing added and activated
import bpy

bpy.context.scene.DocProperties.target_view = "<VIEW>"
cameras = {obj for obj in bpy.data.objects if obj.type == "CAMERA"}
bpy.ops.bim.add_drawing()
camera = next(obj for obj in bpy.data.objects if obj.type == "CAMERA" and obj not in cameras)
bpy.ops.bim.activate_drawing(drawing=camera.BIMObjectProperties.ifc_definition_id, should_view_from_camera=False)
result = {"camera": camera.name}
```

```python
# [HEADLESS_CALL] Active drawing placed at <x>, <y>, <z>, scaled 1/4"=1'-0" over <width> by <height> m, written, then set on a new sheet
import bpy

scene = bpy.context.scene
camera, docs = scene.camera, scene.DocProperties
settings = camera.data.BIMCameraProperties
camera.location = (<x>, <y>, <z>)
settings.diagram_scale = '1/4"=1\'-0"|1/48'
settings.width, settings.height = <width>, <height>
bpy.ops.bim.create_drawing(open_viewer=False)
bpy.ops.bim.save_project(filepath=scene.BIMProperties.ifc_file)
bpy.ops.bim.load_drawings()
bpy.ops.bim.toggle_target_view(toggle_all=True, option="EXPAND")
bpy.ops.bim.add_sheet()
bpy.ops.bim.load_sheets()
docs.active_sheet_index = len(docs.sheets) - 1
docs.active_drawing_index = next(i for i, entry in enumerate(docs.drawings) if entry.ifc_definition_id == camera.BIMObjectProperties.ifc_definition_id)
bpy.ops.bim.add_drawing_to_sheet()
bpy.ops.bim.create_sheets(open_viewer=False)
result = {"sheet": docs.sheets[docs.active_sheet_index].name}
```

- `target_view` takes `PLAN_VIEW`, `ELEVATION_VIEW`, `SECTION_VIEW`, `REFLECTED_PLAN_VIEW`, or `MODEL_VIEW`, a plan camera cutting at its Z
- Camera settings reach the drawing's `EPset_Drawing` from the call after the activation
- `diagram_scale` items follow the scene's unit system (`1:100|1/100` under metric), and `width` and `height` in meters set `ortho_scale`
- `create_drawing` writes `drawings/<drawing>.svg` beside the IFC, `max(width, height) * 1000 / N` millimeters on its long side at 1:N
- Sheet operators poll true once the IFC is saved, and `toggle_target_view` fills the drawing list past its group headers
- `create_sheets` writes `sheets/<sheet>.svg` at the A1 title block size and `sheets/<sheet>.pdf` through the `svg2pdf_command` preference
- Sheets draw from `drawings/assets/default.css` (OpenGost, cut 0.35, projection 0.25, fine 0.18 mm), `doc.drawing_font` the viewport font alone
