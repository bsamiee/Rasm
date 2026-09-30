# [BIM]

IFC files hold the model and Blender objects show its entities, authored through `ifcopenshell.api` and edited, drawn, and saved through Bonsai in a headless session on a copy.

## [01]-[AUTHORING]

`ifcopenshell.api` builds a valid project outside the UI through `run` or `call`:

```python
# [HEADLESS_CALL] New IFC4 project in feet with a wall on its storey, written to <path>
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
api.run("geometry.assign_representation", f, product=wall, representation=api.run("geometry.add_wall_representation", f, context=body, length=5, height=3, thickness=0.2))
api.run("spatial.assign_container", f, relating_structure=storey, products=[wall])
api.run("geometry.edit_object_placement", f, product=wall)
f.write("<path>")
result = {"unit_scale": ifcopenshell.util.unit.calculate_unit_scale(f)}
```

- `bim.new_project(preset="imperial_ft")` starts a feet project (`metric_m`, `metric_mm` metric) on the startup file, `bim.save_project` writing it

## [02]-[PROJECT]

IFC files load, edit, and save in a session `headless.py start <scratch>.blend <name>` opened, saved to a copy and ended by `stop`:

```python
# [HEADLESS_CALL] <file>.ifc loaded, <object> moved and reshaped, saved to <copy>.ifc
import bpy
import bonsai.tool as tool
import ifcopenshell.util.element as element

bpy.ops.bim.load_project(filepath="<file>.ifc")
obj = bpy.data.objects["<object>"]
obj.location.x += <meters>
for vertex in obj.data.vertices:
    vertex.co.z *= <factor>
bpy.ops.bim.update_representation(obj=obj.name, ifc_representation_class="IfcTessellatedFaceSet")
bpy.ops.bim.save_project(filepath="<copy>.ifc")
entity = tool.Ifc.get_entity(obj)
result = {"ifc": bpy.context.scene.BIMProperties.ifc_file, "container": element.get_container(entity).Name, "body": [r.RepresentationType for r in entity.Representation.Representations]}
```

- `bim.load_project` replaces the open file with the startup file, `should_start_fresh_session=False` loading into the open file with its objects
- Loads mesh geometry through the scene's `BIMProjectProperties.geometry_library`, `opencascade` loading IFC geometry
- Scenes from outside the startup file take `geometry_library = "opencascade"` before a load into them
- Objects take the name `<IfcClass>/<Name>`, spatial elements as empties, each in its container's collection
- Mesh edits reach the IFC through `bim.update_representation` on the object
- `IfcTessellatedFaceSet` writes an edited mesh as it stands, the default class re-detects an extrusion and raises on a mesh that breaks its profile
- `bim.save_project` repoints `scene.BIMProperties.ifc_file` at the written file and saves a titled `.blend` holding unsaved changes

## [03]-[DRAWINGS]

Drawings are scaled SVG views cut from the IFC model and placed on sheets, one call activating the drawing and a later one writing it:

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
# [HEADLESS_CALL] Active drawing placed, scaled, and written, then set on a new sheet
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

- New cameras sit at the origin
- Camera settings written in the call that activated the drawing are lost, a later call writes them into the drawing's `EPset_Drawing`
- `diagram_scale` items follow the scene's unit system (`1:100|1/100` under metric), and `width` and `height` in meters set `ortho_scale`
- `create_drawing` writes `drawings/<drawing>.svg` beside the IFC, `max(width, height) * 1000 / N` millimeters on its long side at 1:N
- Drawings and Sheets panels draw only with a saved IFC project
- `create_sheets` writes `sheets/<sheet>.svg` at the A1 title block size and `sheets/<sheet>.pdf` through the `svg2pdf_command` preference
- Sheets draw from `drawings/assets/default.css` (OpenGost, cut 0.35, projection 0.25, fine 0.18 mm), `doc.drawing_font` the viewport font alone
