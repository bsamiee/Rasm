# [DRAFTING]

Scaled sheets of a model or sketch through orthographic cameras, Line Art, and Dimensions output, written by `sheet` from `scripts/drawing.py`.

## [01]-[SKETCHES]

CAD Sketcher solves a constrained 2D profile through its model API in one call, and its `Body` mesh takes the profile into Line Art:

```python
# [HEADLESS_CALL] Rectangle <width> by <depth> m on the XY plane, one corner fixed at the origin, solved to zero freedom, its Body in new collection <collection>
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
body, profile = sketch.target_object.parent.parent, bpy.data.collections.new("<collection>")
bpy.context.scene.collection.children.link(profile)
for owner in body.users_collection:
    owner.objects.unlink(body)
profile.objects.link(body)
result = {"body": body.name, "dof": sketch.target_object["dof"], "state": sketch.target_object["solver_state"]}
```

- `slvs_add_sketch_on_plane` builds and activates a sketch with `Body`, its `CAD Sketcher Convert` modifier, `Body Workplane`, and `Body Sketch` curves
- Lines sharing a `PointRef` join, a distance between a line's `p1` and `p2` drives its length, `fixed=True` pins a point
- `dof` 0 with `solver_state` `OKAY` marks a fully constrained sketch, and moving `Body` moves the sketch with it

## [02]-[CAMERAS]

Each sheet is an orthographic camera holding its paper, the scene camera while its Line Art and sheet evaluate:

```python
# [HEADLESS_CALL] Sheet camera <Sheet> on <width> by <height> m paper at 1:<N>, the scene camera
from math import pi

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
- Plans take rotation `(0, 0, 0)` above the model and `clip_start` at the camera height less the cut height, and Line Art outlines the cut
- Elevations take `(pi / 2, 0, <heading> + 1e-4)`, heading 0 facing +Y, and a section is an elevation with `clip_start` at its cut plane
- Headings turned 1e-4 keep the Line Art silhouette of a face edge-on to an axis view (a cylinder side)

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

- Presets (isometric, dimetric, trimetric, cavalier, military) place an `AUTO` fit scene camera 25 m from the origin, aimed at it
- Scale holds on the picture plane, model axes foreshorten by the preset (an isometric axis draws 0.8165 of its length)

## [03]-[LINEWORK]

Line Art strokes the scene camera's view into a Grease Pencil object that `sheet` projects onto paper in document ink:

```python
# [HEADLESS_CALL] Line Art of <collection> through the scene camera in a <pen> m pen, written as the camera's sheet at 1:<N>
import bpy
from drawing import sheet
from results import as_result

scale, pen = <N>, <pen>
camera = bpy.context.scene.camera
bpy.ops.object.grease_pencil_add(type="LINEART_COLLECTION")
lines = bpy.context.view_layer.objects.active
lines.name = f"{camera.name} Line Art"
art = lines.modifiers[0]
art.source_collection, art.use_custom_camera, art.source_camera = bpy.data.collections["<collection>"], True, camera
art.radius, art.use_image_boundary_trimming = pen * scale, True
result = as_result(sheet(camera.name, scale, (lines.name,)))
```

- Line Art `radius` is the stroke width, one object per pen and source
- Pens come from `Pen` in `tools/interface/units.py` (0.35 mm cut, 0.25 mm projection, 0.18 mm fine)
- Boolean cutters take `lineart.usage = "EXCLUDE"`, Line Art occluding with every scene object whatever its source
- Ground planes under the model take `lineart.usage = "NO_INTERSECTION"`, as `tools/interface/blender/script/startup.py` writes on `Ground`
- `sheet(<name>, <N>, <objects>)` recomputes each named Line Art and writes `.artifacts/blender/sheets/<name>.svg`, `.pdf` at paper size, and `.png`
- Sheets take their camera's name, and each sheet object opens with the camera's name and a space, the prefix gathering its Line Art and dimensions
- `Sheet.paper` holds the paper size in meters
- `Sheet.window` holds the left, top, width, and height in meters of the paper the PNG shows
- `Sheet.strokes` counts strokes per pen width in millimeters of each object
- Strokes draw black at material alpha times layer opacity times mean point opacity
- `Rejected` names a missing, non-camera, or perspective scene camera, a non-Grease Pencil object, or no strokes
- `Uncompiled` names the `typst` diagnostics
- PNGs show the inked extent with 2.5% of its long side added on every side, clipped to the paper
- PNGs span 2000 px across the window's long side S, where a length L at 1:N measures L / N × 2000 / S px

```bash
# Region of a sheet at print resolution, pixel offsets and size at <dpi>
pdftoppm -png -r <dpi> -x <x> -y <y> -W <w> -H <h> -singlefile .artifacts/blender/sheets/<name>.pdf .artifacts/blender/sheets/<name>-detail
```

## [04]-[DIMENSIONS]

Dimensions output bakes each dimension to a Grease Pencil object with its label facing the scene camera:

```python
# [HEADLESS_CALL] Dimension <label> of the <object> edge farthest along <direction>, <offset> m off on paper, drawn on the camera's sheet
from math import copysign

import bmesh
import bpy
from mathutils import Vector
from drawing import sheet
from results import as_result

scale, text, pen, offset, direction = <N>, <cap>, <pen>, <offset>, Vector(<direction>)
scene, target = bpy.context.scene, bpy.data.objects["<object>"]
settings, normal = scene.dimensions_settings, scene.camera.matrix_world.col[2].xyz
settings.output_world_text_height = settings.output_world_arrow_size = text * scale
settings.output_world_line_width = pen * scale
bpy.context.view_layer.objects.active = target
bpy.ops.object.mode_set(mode="EDIT")
mesh = bmesh.from_edit_mesh(target.data)
for element in (*mesh.verts, *mesh.edges, *mesh.faces):
    element.select = False
edge = max(mesh.edges, key=lambda e: min((target.matrix_world @ v.co).dot(direction) for v in e.verts))
edge.select_set(True)
start, end = (target.matrix_world @ v.co for v in edge.verts)
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
- `CAMERA` sizing reads `ortho_scale / resolution_y` meters per pixel, a cap `h` taking `output_text_height = h * N * resolution_y / ortho_scale`
- `generate_output` under `output_scope` `ALL` rebuilds every visible dimension facing the scene camera, and each sheet regenerates before `sheet`
- Labels follow scene units through `imperial_unit_style` `FEET_INCHES` and `metric_unit_style` `MILLIMETERS`, which `extension/unit_system.py` writes
- `angle_selected_edges` and `area_selected_faces` add angle and area dimensions from the same Edit Mode selection
- `bpy.ops.interface.units(system="<METRIC or IMPERIAL>")` switches the file's units and label sizes at the system's sheet scale
- Unit switches rewrite the scene camera's clip range and `ortho_scale`, and run while the render camera is the scene camera
- MeasureIt_ARCH draws in a GUI alone, sheets take Dimensions output

## [05]-[EXISTING_FILES]

Files holding sheet cameras rewrite every sheet from each camera's stored paper and Line Art binding:

```python
# [HEADLESS_CALL] Every sheet of the open file rewritten at its stored scale, labels at <cap> and dimension lines at <pen>, <camera> restored
import bpy
from drawing import sheet
from results import as_result

text, pen = <cap>, <pen>
scene = bpy.context.scene
settings = scene.dimensions_settings
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
scene.camera = bpy.data.objects["<camera>"]
result = sheets
```

- `headless.py start <file> <name>` opens the file with the Dimensions and Per-Camera Resolution add-ons the snippets call
- Edits to objects a Line Art reads show at the next `sheet`
