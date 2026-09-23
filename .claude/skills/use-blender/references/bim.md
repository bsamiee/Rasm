# [BIM]

IFC work runs on Bonsai with its bundled `ifcopenshell`: the IFC file is the model, Blender objects are views of its entities that Bonsai writes back on save.

## [01]-[MODEL]

- `bim.load_project(filepath=, should_start_fresh_session=False)` loads an IFC into the open file, spatial elements as empties
- `should_start_fresh_session` defaults true and runs `wm.read_homefile()`, the open file's objects and path gone with no prompt
- Loaded objects take the name `<IfcClass>/<Name>`
- `bonsai.tool.Ifc.get()` returns the open `ifcopenshell.file`, `bonsai.tool.Ifc.get_entity(<object>)` the entity behind an object
- `ifcopenshell.util.element` reads psets (`get_psets`), container (`get_container`), and type (`get_type`) from an entity
- `bim.save_project(filepath=)` writes the file, object transforms reach `ObjectPlacement` on save
- Mesh edits stay in Blender until `bim.update_representation()` runs on the active and selected object, a save without it writes the old shape

## [02]-[AUTHORING]

`ifcopenshell.api` builds a valid project outside the UI, with lengths in meters and the file unit converting on write:

```python
# [HEADLESS_CALL] New IFC4 project with a wall on its storey, written to <path>
import ifcopenshell.api as api
import ifcopenshell.util.unit

f = api.run("project.create_file", version="IFC4")
project = api.run("root.create_entity", f, ifc_class="IfcProject", name="<project>")
api.run("unit.assign_unit", f)
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

- `unit.assign_unit` with no arguments sets millimeters, `calculate_unit_scale` answers `0.001` and representation arguments stay meters
- `bim.load_project` of the written file then gives the Blender view, a 5 m wall measures `(5.0, 0.2, 3.0)` as `IfcWall/<wall>`

## [03]-[DRAWINGS]

Bonsai drawings are scaled SVG views cut from the IFC model and sheets place them, in a headless session:
1. `scene.DocProperties.target_view` takes `PLAN_VIEW`, `ELEVATION_VIEW`, `SECTION_VIEW`, `REFLECTED_PLAN_VIEW`, or `MODEL_VIEW`
2. `bpy.ops.bim.add_drawing()` adds an `IfcAnnotation/<VIEW>` camera at the origin, section and elevation cameras take their place in code
3. `bpy.ops.bim.activate_drawing(drawing=<camera>.BIMObjectProperties.ifc_definition_id, should_view_from_camera=False)`
4. `bpy.ops.bim.create_drawing(open_viewer=False)` writes `drawings/<VIEW>.svg` beside the IFC file
5. `bpy.ops.bim.save_project(filepath=)`, sheet operators poll false until the IFC is saved
6. `bpy.ops.bim.load_drawings()`, then `bpy.ops.bim.toggle_target_view(toggle_all=True, option="EXPAND")` fills the list past its group headers
7. `bpy.ops.bim.add_sheet()` and `bpy.ops.bim.load_sheets()`, then `active_sheet_index` and `active_drawing_index` pick an `is_drawing` entry
8. `bpy.ops.bim.add_drawing_to_sheet()`, then `bpy.ops.bim.create_sheets(open_viewer=False)` writes `sheets/<sheet>.svg` at the sheet size

- Drawings carry `data-scale="1:100"` and millimeter `width` and `height`, a 5 m wall draws 50 mm long
- `drawings/cache/` holds linework and annotation layers, `drawings/assets/` holds symbols, markers, and patterns
- `create_sheets` converts to PDF through the add-on's `svg2pdf_command` preference, Inkscape by default and absent here
- `typst compile --root / <sheet>.typ <pdf>` writes a sheet PDF with its stylesheet line weights
- `<sheet>.typ` sets `page(width: <w>mm, height: <h>mm, margin: 0pt)` and holds `#image("<sheet>.svg", width: 100%, height: 100%)`
