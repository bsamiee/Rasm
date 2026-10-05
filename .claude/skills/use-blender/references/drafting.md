# [DRAFTING]

Scaled sheets of a model or sketch come from orthographic cameras, Line Art, and Dimensions output, written by `sheet` from `scripts/drawing.py`.

## [01]-[FILE]

Sheets build in one session on the drawing file, under the unit system the sheets print in:
1. `headless.py start <file> <name>` holds the work, every snippet of this file running through `call`
2. `bpy.ops.interface.units(system="<METRIC|IMPERIAL>")` sets the sheet system before any sheet camera exists
3. Cameras, Line Art, and dimensions follow per sheet, then `sheet` writes it
4. `headless.py stop <name>` saves the file with its cameras, Line Art, and dimension output

- Unit switches write Dimensions label sizes at the system's sheet, `Units.text` cap height and `Pen.THIN` line width times the scale
- Unit switches rewrite the scene camera's `clip_start`, `clip_end`, and `ortho_scale` to the system's sheet (A1 at 1:50, ARCH D at 1/4"=1'-0")
- Per-Camera Resolution keeps each camera's stored paper through a switch, the paper `sheet` reads the scale from
- Dimensions output gives sheets their dimensions, MeasureIt_ARCH drawing in a GUI alone

## [02]-[SKETCHES]

CAD Sketcher solves a constrained 2D profile through its model API in one call, and its `Body` mesh takes the profile into Line Art:

```python
# [HEADLESS_CALL] Rectangle <width> by <depth> m on plane XY, one corner fixed at the origin, fully constrained, Body in new collection <Collection>
from importlib import import_module

import bpy
from scene import viewport

width, depth = <width>, <depth>
found = viewport()
cad = next(module for module in bpy.context.preferences.addons.keys() if module.endswith("CAD_Sketcher"))
curves, sketches, solver = (import_module(f"{cad}.{name}") for name in ("model.curve_ref", "model.sketch_ref", "curve_solver"))
with bpy.context.temp_override(window=found.window, area=found.area, region=found.region):
    bpy.ops.view3d.slvs_add_sketch_on_plane(plane="XY")
sketch = sketches.get_active_sketch(bpy.context)
corners = [curves.PointRef.create(sketch, co, fixed=co == (0, 0)) for co in ((0, 0), (width, 0), (width, depth), (0, depth))]
sides = [curves.LineRef.create(sketch, a, b) for a, b in zip(corners, corners[1:] + corners[:1])]
for side in sides[0::2]:
    sketch.constraints.add_horizontal(side.curve_id)
for side in sides[1::2]:
    sketch.constraints.add_vertical(side.curve_id)
for side, length in zip(sides, (width, depth)):
    sketch.constraints.add_distance(curve_id_1=side.p1.curve_id, curve_id_2=side.p2.curve_id, value=length)
solver.solve_system(bpy.context, sketch)
body, profile = sketch.target_object.parent.parent, bpy.data.collections.new("<Collection>")
bpy.context.scene.collection.children.link(profile)
for owner in body.users_collection:
    owner.objects.unlink(body)
profile.objects.link(body)
result = {"body": body.name, "dof": sketch.target_object["dof"], "state": sketch.target_object["solver_state"]}
```

- `slvs_add_sketch_on_plane` adds and activates a sketch under mesh `Body`, child `Body Workplane`, and grandchild `Body Sketch`
- `Body` meshes the sketch through its `CAD Sketcher Convert` modifier
- Lines sharing a `PointRef` join, a distance between a line's `p1` and `p2` drives its length, and `fixed=True` pins a point
- `dof` 0 with `solver_state` `OKAY` marks a fully constrained sketch, and moving `Body` moves the sketch with it

## [03]-[CAMERAS]

Each sheet is an orthographic camera holding its paper, the scene camera while its Line Art and sheet evaluate:

```python
# [HEADLESS_CALL] Sheet camera <Sheet> on <width> by <height> m paper at 1:<N>, the scene camera
import bpy

scale, paper = <N>, (<width>, <height>)
scene = bpy.context.scene
camera = bpy.data.objects.new("<Sheet>", bpy.data.cameras.new("<Sheet>"))
scene.collection.objects.link(camera)
camera.data.type, camera.data.ortho_scale = "ORTHO", max(paper) * scale
camera.location, camera.rotation_euler = <location>, <rotation>
camera.data.clip_start, camera.data.clip_end = <near>, <far>
stored = camera.data.per_camera_resolution
stored.use_custom_resolution = True
stored.resolution_x, stored.resolution_y = (round(side * 1e4) for side in paper)
scene.camera = camera
result = {"camera": camera.name}
```

- `Units` in `tools/interface/units.py` pairs ARCH D (36" by 24") with 1/4"=1'-0" (48) and A1 (841 by 594 mm) with 1:50
- `ortho_scale` spans the paper's long side times the scale under the `AUTO` fit of a new camera
- Per-Camera Resolution copies the scene camera's stored paper into the render resolution that Line Art and `sheet` frame by
- Stored paper at 1e4 px per meter holds ARCH D, A1, and letter exactly

Each drawing kind takes its camera placement:

| [INDEX] | [DRAWING]   | [ROTATION]                      | [CLIP_START]                                    |
| :-----: | :---------- | :------------------------------ | :---------------------------------------------- |
|  [01]   | Plan        | `(0, 0, 0)` above the model     | Camera height less the cut height, cut outlined |
|  [02]   | Elevation   | `(pi / 2, 0, <heading> + 1e-4)` | In front of the model                           |
|  [03]   | Section     | `(pi / 2, 0, <heading> + 1e-4)` | At the cut plane                                |
|  [04]   | Axonometric | Pohlke preset, Z turned 1e-4    | Preset camera's own                             |

- Headings read 0 facing +Y, and the 1e-4 turn keeps the Line Art silhouette of a face edge-on to an axis view (a cylinder side)

```python
# [HEADLESS_CALL] Pohlke <preset> camera <Sheet> on <width> by <height> m paper at 1:<N>, the scene camera
import bpy
from scene import viewport

scale, paper = <N>, (<width>, <height>)
found = viewport()
with bpy.context.temp_override(window=found.window, area=found.area, region=found.region):
    camera = bpy.pohlke.add_preset_camera("<preset>", ortho_scale=max(paper) * scale)
camera.name = "<Sheet>"
camera.rotation_euler.z += 1e-4
stored = camera.data.per_camera_resolution
stored.use_custom_resolution = True
stored.resolution_x, stored.resolution_y = (round(side * 1e4) for side in paper)
result = {"camera": camera.name, "presets": list(bpy.pohlke.names)}
```

- `bpy.pohlke.names` lists the isometric, dimetric, trimetric, Hejduk, cavalier, and military presets `<preset>` takes
- Presets place an `AUTO` fit scene camera 25 m from the origin, aimed at it
- Scale holds on the picture plane, model axes foreshorten by the preset (an isometric axis draws 0.8165 of its length)

## [04]-[LINEWORK]

Line Art strokes the scene camera's view into a Grease Pencil object that `sheet` projects onto paper in document ink:

```python
# [HEADLESS_CALL] Line Art of <Collection> through the scene camera in a <pen> m pen, written as the camera's sheet at 1:<N>
import bpy
from drawing import sheet
from results import as_result

scale, pen = <N>, <pen>
camera = bpy.context.scene.camera
bpy.ops.object.grease_pencil_add(type="LINEART_COLLECTION")
lines = bpy.context.view_layer.objects.active
lines.name = f"{camera.name} Line Art"
art = lines.modifiers[0]
art.source_collection, art.use_custom_camera, art.source_camera = bpy.data.collections["<Collection>"], True, camera
art.radius, art.use_image_boundary_trimming = pen * scale, True
result = as_result(sheet(camera.name, scale, (lines.name,)))
```

- Line Art `radius` is the stroke width, one object per pen and source
- Pens come from `Pen` in `tools/interface/units.py` (0.35 mm cut, 0.25 mm projection, 0.18 mm fine)
- Line Art occludes with every scene object whatever its source, Boolean cutters taking `lineart.usage = "EXCLUDE"`
- Ground planes under the model take `lineart.usage = "NO_INTERSECTION"`, as `tools/interface/blender/script/startup.py` writes on `Ground`
- Edits to objects a Line Art reads show at the next `sheet`

## [05]-[DIMENSIONS]

Dimensions output bakes each dimension to a Grease Pencil object with its label facing the scene camera:

```python
# [HEADLESS_CALL] Dimension <label> of the <Object> edge in the picture plane farthest along <direction>, <offset> m off on paper, on the camera sheet
from math import copysign

import bmesh
import bpy
from mathutils import Vector
from drawing import sheet
from results import as_result

scale, text, pen, offset, direction = <N>, <cap>, <pen>, <offset>, Vector(<direction>)
scene, target = bpy.context.scene, bpy.data.objects["<Object>"]
settings, normal, world = scene.dimensions_settings, scene.camera.matrix_world.col[2].xyz, target.matrix_world
settings.output_sizing_mode = "WORLD"
settings.output_world_text_height = settings.output_world_arrow_size = text * scale
settings.output_world_line_width = pen * scale
bpy.context.view_layer.objects.active = target
bpy.ops.object.mode_set(mode="EDIT")
mesh = bmesh.from_edit_mesh(target.data)
for element in (*mesh.verts, *mesh.edges, *mesh.faces):
    element.select = False
drawn = [e for e in mesh.edges if abs((world @ e.verts[1].co - world @ e.verts[0].co).normalized().dot(normal)) < 1e-6]
edge = max(drawn, key=lambda e: min((world @ v.co).dot(direction) for v in e.verts))
edge.select_set(True)
start, end = (world @ v.co for v in edge.verts)
bmesh.update_edit_mesh(target.data)
before = set(scene.objects)
bpy.ops.dimensions.dimension_selected_edge()
bpy.ops.object.mode_set(mode="OBJECT")
(dimension,) = set(scene.objects) - before
dimension.name = f"{scene.camera.name} <label>"
props = dimension.dimension_props
props.offset_plane_normal, props.offset_distance = normal, copysign(offset * scale, normal.cross(end - start).dot(direction))
bpy.ops.dimensions.generate_output()
result = as_result(sheet(scene.camera.name, scale, tuple(o.name for o in scene.objects if o.type == "GREASEPENCIL" and o.name.startswith(f"{scene.camera.name} "))))
```

- `dimension_selected_edge` attaches to the edge's vertices in order in a plan plane 0.25 m off, moved by `offset_plane_normal` and `offset_distance`
- Offsets run along the plane normal crossed with the edge, sign picking the side, and `Units.first_offset` sets the first string at 6 cap heights
- `WORLD` sizing states the label cap height, arrow, and line width in model meters, paper value times N, `Units.text` giving 3/32" or 2.5 mm caps
- `generate_output` under `output_scope` `ALL` rebuilds every visible dimension facing the scene camera, and each sheet regenerates before `sheet`
- Labels follow scene units through `imperial_unit_style` `FEET_INCHES` and `metric_unit_style` `MILLIMETERS`, which `extension/unit_system.py` writes
- `angle_selected_edges` and `area_selected_faces` add angle and area dimensions from the same Edit Mode selection

## [06]-[SHEETS]

`sheet(<name>, <N>, <objects>)` recomputes each named Line Art and writes `.artifacts/blender/sheets/<name>.svg`, `.pdf` at paper size, and `.png`:
- Sheets take their camera's name, and each sheet object opens with the camera's name and a space, the prefix gathering its Line Art and dimensions
- Strokes draw black at material alpha times layer opacity times mean point opacity
- PNGs show the inked extent with 2.5% of its long side added on every side, clipped to the paper
- PNGs span 2000 px across the long side S of `Sheet.window`, where a length L at 1:N measures L / N × 2000 / S px

Sheets read in order:
1. `Read` of the `Sheet` PNG shows the inked window
2. `pdftoppm` cuts a region of the PDF at print resolution for detail
3. `GreasePencilDrawing` faults take the Line Art source collection checked against the sheet camera's clip range

```bash
# Region of a sheet at print resolution, pixel offsets and size at <dpi>
pdftoppm -png -r <dpi> -x <x> -y <y> -W <w> -H <h> -singlefile .artifacts/blender/sheets/<name>.pdf .artifacts/blender/sheets/<name>-detail
```

## [07]-[EXISTING_FILES]

Files holding sheet cameras rewrite every sheet from each camera's stored paper and Line Art binding, the scene camera restored:
1. `headless.py start <file> <name>`
2. `call` of the snippet, `sheets` naming each case
3. `headless.py stop <name>`

```python
# [HEADLESS_CALL] Every sheet of the open file rewritten at its stored scale, labels at <cap> and dimension lines at <pen>
import bpy
from drawing import sheet
from results import as_result

text, pen = <cap>, <pen>
scene = bpy.context.scene
settings, held = scene.dimensions_settings, scene.camera
settings.output_sizing_mode = "WORLD"
cameras = sorted({m.source_camera for o in scene.objects if o.type == "GREASEPENCIL" for m in o.modifiers if m.type == "LINEART" and m.use_custom_camera}, key=lambda c: c.name)
sheets = {}
for camera in cameras:
    stored = camera.data.per_camera_resolution
    scale = round(camera.data.ortho_scale * 1e4 / max(stored.resolution_x, stored.resolution_y))
    scene.camera = camera
    settings.output_world_text_height = settings.output_world_arrow_size = text * scale
    settings.output_world_line_width = pen * scale
    bpy.ops.dimensions.generate_output()
    sheets[camera.name] = as_result(sheet(camera.name, scale, tuple(o.name for o in scene.objects if o.type == "GREASEPENCIL" and o.name.startswith(f"{camera.name} "))))
scene.camera = held
result = {"sheets": sheets, "camera": scene.camera.name}
```
