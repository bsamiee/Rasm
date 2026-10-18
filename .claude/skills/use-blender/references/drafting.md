# [DRAFTING]

Covers scaled drawings of a model or sketch.

## [01]-[ROUTES]

Lengths in code are meters and angles radians, imperial values entering as literals over `ft, inch = 0.3048, 0.0254`, and labels display through the scene's unit settings. Interactive tools of every drafting extension (sketch drawing, dimension placement, part placement, the scale picker) read mouse events and stay with the user:

| [INDEX] | [TASK]                                | [ROUTE]                                                            | [SESSION]         |
| :-----: | :------------------------------------ | :----------------------------------------------------------------- | :---------------- |
|  [01]   | Constrained 2D sketch to geometry     | CAD Sketcher model API, solver, body modifiers                     | Background or GUI |
|  [02]   | Profile swept along a path            | Curve Profile Creator `geometry` functions, `cpc.*` exec operators | Background or GUI |
|  [03]   | Model scaled from a known length      | `object.scale_confirm_dialog` exec form                            | Background or GUI |
|  [04]   | Imported CAD assembly made drawable   | CAD-Helper `object.cleanup_selected_meshes` with no window         | Background or GUI |
|  [05]   | Plan, elevation, section camera       | Camera data API, Per-Camera Resolution paper                       | Background or GUI |
|  [06]   | Axonometric or oblique camera         | `bpy.pohlke` under a `VIEW_3D` override                            | Background or GUI |
|  [07]   | Linework                              | Line Art Grease Pencil object over a source collection             | Background or GUI |
|  [08]   | Dimensions, angles, areas             | Dimensions package functions, Edit Mode selection operators        | Background or GUI |
|  [09]   | Pen-true vector sheet                 | Grease Pencil PDF or SVG export through the drawn camera view      | GUI               |
|  [10]   | Annotation sheet with title block     | `dimensions.export_pdf`, `dimensions.export_svg`                   | Background or GUI |
|  [11]   | Raster sheets of cameras              | `render.render` per camera, Batch Render Cameras in a GUI          | Background or GUI |
|  [12]   | Sheet labels as font text, DXF output | MeasureIt_ARCH generators and output operators                     | GUI               |

## [02]-[SKETCHES]

CAD Sketcher holds a sketch as a hidden Curves object under a `Body` mesh with a Geometry Nodes modifier filling closed loops as faces. Model API builds and solves sketches with no viewport:

```python
from importlib import import_module

import bpy

add_sketch, workplane, curves, solver, curve_data, body_of = (import_module(f"bl_ext.user_default.CAD_Sketcher.{name}") for name in ("operators.add_sketch", "utilities.workplane", "model.curve_ref", "curve_solver", "utilities.curve_data", "utilities.body"))
context = bpy.context
workplane.ensure_origin_workplane_empties(context)
sketch = add_sketch.build_sketch_on_workplane(context, context.scene.sketcher.wp_xy)
origin = next(ref for ref in (curves.curve_ref(sketch, cid) for cid in curve_data.read_uuid_list(sketch.target_object.data, "curve_id")) if isinstance(ref, curves.PointRef) and ref.is_origin)
w, d = <width_m>, <depth_m>
corners = [curves.PointRef.create(sketch, co) for co in ((0, 0), (w, 0), (w, d), (0, d))]
sides = [curves.LineRef.create(sketch, a, b) for a, b in zip(corners, corners[1:] + corners[:1])]
constraints = sketch.constraints
constraints.add_coincident(corners[0].curve_id, origin.curve_id)
for side in sides[0::2]:
    constraints.add_horizontal(side.curve_id)
for side in sides[1::2]:
    constraints.add_vertical(side.curve_id)
for side, length in zip(sides, (w, d)):
    constraints.add_distance(curve_id_1=side.p1.curve_id, curve_id_2=side.p2.curve_id, value=length)
solved = solver.solve_system(context, sketch=sketch)
result = {"solved": solved, "dof": sketch.dof, "state": sketch.solver_state, "body": body_of.body_of(sketch.target_object).name}
```

- Finished sketches solve to `dof` 0 with `solver_state` `OKAY`
- Sketch coordinates are local 2D meters on the workplane (`wp_xy`, `wp_xz`, `wp_yz`)
- Lines sharing a `PointRef` join
- Constraints take `curve_id` strings
- Circles take `curves.CircleRef.create(sketch, center, radius)` and their size through `add_diameter`
- Point-to-line distances are signed by line direction
- `add_distance(init=True, curve_id_1=, curve_id_2=)` with `.value` written after keeps the point's side
- `add_angle(init=True, value=<v>)` takes degrees and `init=False` radians
- With constraint values reading and writing through `bpy.context.scene`, sketch work runs with the sketch's scene as context scene
- Extrusion assigns `import_module("bl_ext.user_default.CAD_Sketcher.utilities.extrude_nodes").build_extrude_node_group()` to a new `NODES` modifier on the body
- Extrusion writes the modifier's `Size` input through `operators.modifiers.set_modifier_input`, then `body.update_tag()`
- Sketches become sweep profiles with convert modifier's `Fill` input off, then `bpy.data.meshes.new_from_object` of the evaluated body
- `object.convert(target="CURVE")` converts the new mesh's object to the profile curve

## [03]-[PROFILES]

Curve Profile Creator sweeps a 2D profile curve along a path as its bevel object, both built from point lists:

```python
from importlib import import_module

import bpy
from mathutils import Vector

geometry = import_module("bl_ext.user_default.curve_profile_creator.geometry")
collection = bpy.data.collections.new("<Trim>")
bpy.context.scene.collection.children.link(collection)
profile = geometry.create_profile_curve("<Profile>", [[Vector(co) for co in <outline>]], Vector((0, 0, 0)), collection)
sweep = geometry.create_sweep_path("<Sweep>", [[Vector(co) for co in <path>]], profile, collection)
```

- Profile local X is offset from the path and local Y the rise, and anchor argument names the profile point the path runs through
- With closed 2D paths pointing profile +X inside the loop whatever the winding, outside profiles draw at negative X
- Open paths point profile +X to the right of travel
- Paths with matching first and last points become cyclic
- Paths sharing one Z stay 2D and keep corners at full profile width
- `geometry.create_profile_curve_bezier` takes chains of `co`, `handle_left`, `handle_right`, and handle type dicts for curved profiles
- Existing curves take `scene.cpc_settings.active_profile` through `cpc.apply_profile_to_curve()`
- `cpc.apply_profile_to_curve()` runs under `temp_override(object=<path>, active_object=<path>)`
- `cpc.convert_sweep_to_mesh()` converts active sweep in place under the same override, `selected_objects` and `selected_editable_objects` naming it
- `fill_caps`, `smooth_by_angle`, and `smooth_angle` writes act on `context.object` and run with the sweep as context object

## [04]-[SCALE_FROM_MEASUREMENT]

Measure and Scale's confirm step scales a selection from a measured world distance:

```python
import bpy

objects = [bpy.data.objects[name] for name in ("<Object>", "<Other>")]
a, b = (objects[0].matrix_world @ objects[0].data.vertices[index].co for index in (<i>, <j>))
for obj in bpy.context.view_layer.objects:
    obj.select_set(obj in objects)
bpy.context.view_layer.objects.active = objects[0]
bpy.ops.object.scale_confirm_dialog(measured_distance=(b - a).length, selected_object_names=",".join(o.name for o in objects), target_dimension=<target>)
```

- `target_dimension` is in `scene.scale_interactive_settings.system_unit` units (`IMPERIAL_IN` inches, `IMPERIAL_FT` feet, `METRIC` its `metric_unit`)
- Named objects resize as one set about `tool_settings.transform_pivot_point`, then scale applies to their mesh data
- Extension unit factors round at parts per million
- Exact lengths multiply `Matrix.Translation(pivot) @ Matrix.Scale(<target_m> / measured, 4) @ Matrix.Translation(-pivot)` into each `matrix_world`
- `object.transform_apply(scale=True)` follows the exact-length writes

## [05]-[CAD_CLEANUP]

Imported meshes with separate vertices per face draw every face seam in Line Art. CAD-Helper merges by distance, clears split normals, recalculates normals, and smooths by angle per mesh, from `scene.cad_cleanup_*` settings:

```python
import bpy

scene = bpy.context.scene
scene.cad_cleanup_use_merge_by_distance = True
meshes = [obj for obj in bpy.data.collections["<Import>"].all_objects if obj.type == "MESH"]
with bpy.context.temp_override(window=None, selected_objects=meshes, active_object=meshes[0], object=meshes[0], view_layer=bpy.context.view_layer, scene=scene):
    bpy.ops.object.cleanup_selected_meshes()
```

- `window=None` runs cleanup inside the call, a window in context making it a timer-driven modal that background runs never advance
- Merging is off by default, and `cad_cleanup_merge_distance` sets the weld distance
- Hierarchy flattening runs in exec form on the selection through `object.flatten_hierarchy` and `object.flatten_and_join_hierarchy`
- Hierarchy deletes run in exec form on the selection through `object.delete_child_empties_without_children` and `object.delete_and_reparent_children`

## [06]-[SHEET_CAMERAS]

Sheets are orthographic cameras with a frame spanning paper at 1:N, Per-Camera Resolution holding paper at 300 px per inch, the density Grease Pencil PDF export maps to points:

```python
import bpy

paper_in, scale = (<long_in>, <short_in>), <N>
scene = bpy.context.scene
camera = bpy.data.objects.new("<Sheet>", bpy.data.cameras.new("<Sheet>"))
scene.collection.objects.link(camera)
scene.camera = camera
camera.data.type = "ORTHO"
camera.data.ortho_scale = paper_in[0] * 0.0254 * scale * (1 - 1e-6)
camera.location, camera.rotation_euler = <location>, <rotation>
paper = camera.data.per_camera_resolution
paper.use_custom_resolution = True
paper.resolution_x, paper.resolution_y = (round(side * 300) for side in paper_in)
bpy.context.view_layer.update()
```

- `ortho_scale` spans the long side under `AUTO` sensor fit
- Factor (1 − 1e-6) keeps the frame inside page against float32 error in Dimensions fit check
- Imperial scales take N = 12 per inch to the foot (1/4" = 1'-0" is 48, 1/8" = 1'-0" is 96)
- With `use_custom_resolution` reading the scene camera in its setter, a scene camera exists before first paper write
- Per-Camera Resolution copies the scene camera's paper from a `depsgraph_update_pre` handler at next `view_layer.update()` or render
- Paper edits on the current scene camera apply after `camera.data.update_tag()` and an update

| [INDEX] | [DRAWING] | [ROTATION_EULER]                | [CLIP]                                      |
| :-----: | :-------- | :------------------------------ | :------------------------------------------ |
|  [01]   | Plan      | `(0, 0, 1e-4)` above the model  | `clip_start` = camera height − cut height   |
|  [02]   | Elevation | `(pi / 2, 0, <heading> + 1e-4)` | `clip_start` short of the model's near face |
|  [03]   | Section   | `(pi / 2, 0, <heading> + 1e-4)` | `clip_start` at the cut plane               |

With Line Art dropping silhouettes of curved faces seen exactly edge-on, axis-aligned views turn 1e-4 rad about the view axis.

## [07]-[AXONOMETRIC]

Pohlke adds isometric, dimetric, trimetric, and oblique cameras aimed at the 3D cursor through `bpy.pohlke`, under a `VIEW_3D` override:

```python
import bpy

window = bpy.context.window_manager.windows[0]
area = next(a for a in window.screen.areas if a.type == "VIEW_3D")
region = next(r for r in area.regions if r.type == "WINDOW")
bpy.context.scene.cursor.location = <model_center>
with bpy.context.temp_override(window=window, area=area, region=region):
    camera = bpy.pohlke.add_preset_camera("<preset>", ortho_scale=<long_in> * 0.0254 * <N>)
paper = camera.data.per_camera_resolution
paper.use_custom_resolution = True
paper.resolution_x, paper.resolution_y, paper.pixel_aspect_x = <x_px>, <y_px>, camera["pixel_ratio"]
camera.data.update_tag()
bpy.context.view_layer.update()
```

- Preset names come from `bpy.pohlke.names`, short docstring names raising `ValueError: '<name>' not a preset.`
- `add_axonometric_camera(alpha, beta, ...)` and `add_oblique_camera(alpha, beta, shortening, ...)` take degrees
- `position` and `orientation` pick the view side
- Oblique presets stretch through `pixel_aspect_x`
- With Per-Camera Resolution writing its own `pixel_aspect_x`, paper takes it from `camera["pixel_ratio"]`
- Cameras sit 25 m from the cursor, larger models moving the camera back along `camera.matrix_world.col[2].xyz` with `clip_end` covering them
- Calls without the override raise after adding a camera that stays in the scene

## [08]-[LINEWORK]

Line Art draws a collection as one camera sees it into a Grease Pencil object, re-evaluated on each depsgraph update:

```python
import bpy

bpy.ops.object.grease_pencil_add(type="LINEART_COLLECTION")
art = bpy.context.view_layer.objects.active.modifiers[0]
art.source_collection = bpy.data.collections["<Model>"]
art.use_custom_camera, art.source_camera = True, bpy.context.scene.camera
art.radius = <pen_mm> / 1000 * <N>
art.use_clip_plane_boundaries = True
```

- With new Line Art objects taking whole Scene Collection as source, `source_collection` names the drawn collection
- `radius` is the line width in model meters, a pen width on paper times N
- `use_custom_camera` binds the linework to its sheet camera across scene camera changes
- `use_clip_plane_boundaries` draws the cut outline where `clip_start` slices the model
- Each pen weight and source takes its own Line Art object
- With every visible object occluding whatever the source, boolean cutters take `obj.lineart.usage = "EXCLUDE"`
- `object.lineart_bake_strokes` under an `object` override freezes strokes, and a sheet that follows the model again takes a new Line Art object

## [09]-[DIMENSIONS]

Dimensions annotations are empties holding `dimension_props`, anchored to mesh vertices by stable ids and baked to Grease Pencil for sheets:

```python
from importlib import import_module

import bpy
from mathutils import Vector

collections, anchors, geometry, sync = (import_module(f"bl_ext.user_default.dimensions.{name}") for name in ("collections", "anchors", "dimension_geometry", "scene_sync"))
target = bpy.data.objects["<Object>"]
dimension = collections.create_dimension_object(bpy.context, "<Label>")
props = dimension.dimension_props
props.annotation_kind = "LINEAR"
anchors.set_anchor(props.start, target, <vertex_a>)
anchors.set_anchor(props.end, target, <vertex_b>)
props.dimension_type, props.offset_distance, props.offset_plane_normal = "<X|Y|Z>", <offset_m>, <toward_camera>
line = geometry.get_dimension_world_geometry(props.dimension_type, Vector(props.start.world_co), Vector(props.end.world_co), Vector(props.offset_plane_normal), props.offset_distance)
dimension.location = line["line_mid_world"]
sync.sync_scene_objects(bpy.context.scene)
```

- `dimension_type` names the axis the line offsets along, and `offset_distance` sign picks the side
- `measurement_mode` (`TRUE`, `DELTA_X`, `DELTA_Y`, `DELTA_Z`) picks what is measured
- `set_world_anchor(anchor, co)` fixes a point in space and `set_object_anchor(anchor, obj, co)` follows an object's transform
- Edit Mode selections annotate through `bpy.ops.dimensions` exec operators, an edge through `dimension_selected_edge()`
- Edge pairs annotate through `angle_selected_edges()`, and faces through `area_selected_faces()`
- With `dimension_selected_edge` offsetting in the override 3D view's plane or the XY plane, elevations rewrite `offset_plane_normal` after it

Sheet output bakes every visible annotation facing the scene camera into `Dimensions Output` collection, replacing it on each run:

```python
import bpy

settings = bpy.context.scene.dimensions_settings
settings.dimension_color = (0, 0, 0, 1)
settings.output_sizing_mode = "WORLD"
settings.output_world_text_height = settings.output_world_arrow_size = 3 / 32 * 0.0254 * <N>
settings.output_world_line_width = 0.25 / 1000 * <N>
bpy.ops.dimensions.generate_output()
```

- With `dimension_color` pink in the startup file, sheets set it before `generate_output`
- `WORLD` sizing states sizes in model meters (paper size times N), and `CAMERA` sizing in output pixels
- `units.format_length(context, meters, precision)` returns the label a length takes

## [10]-[SHEET_OUTPUT]

Grease Pencil export writes every visible Grease Pencil object, Line Art and Dimensions output alike, through scene camera once the export's 3D view shows it:

```python
import bpy

area = next(a for a in bpy.context.window_manager.windows[0].screen.areas if a.type == "VIEW_3D")
area.spaces.active.region_3d.view_perspective = "CAMERA"
bpy.ops.wm.grease_pencil_export_pdf(filepath="<sheet>.pdf", selected_object_type="VISIBLE")
bpy.ops.wm.grease_pencil_export_svg(filepath="<sheet>.svg", selected_object_type="VISIBLE", use_clip_camera=True)
```

- PDF pages measure render resolution at 300 px per inch, sheet cameras printing at true scale
- SVG pages hold render resolution in `px` with a matching `viewBox` and a `blender_object.<name>` group per object
- Pen-true sheets export from a GUI with camera view drawn, strokes as filled outlines matching the Line Art `radius`
- `use_uniform_width=True` writes stroke centerlines, the form CAD exchange takes in a background run or GUI session
- Exports outside the camera view frame object bounds and return FINISHED over an empty page

Annotation sheets with a border and title block come from Dimensions' export:

```python
import bpy

settings = bpy.context.scene.dimensions_settings
settings.vector_paper_size, settings.vector_orientation = "LETTER", "LANDSCAPE"
settings.sheet_border_enabled = settings.sheet_title_block_enabled = True
settings.sheet_drawing_title, settings.sheet_drawing_number = "<title>", "<number>"
bpy.ops.dimensions.sheet_sync_scale()
bpy.ops.dimensions.sheet_populate_date()
bpy.ops.dimensions.export_pdf(filepath="<annotations>.pdf")
```

- Paper sizes are `A4`, `A3`, and `LETTER`, and only annotations draw
- Border and title block shrink the printable area, and a sheet camera spanning the paper at 1:N exports only at the larger denominator `sheet_sync_scale()` writes
- `export_pdf` and `export_svg` answer CANCELLED with a warning naming the cause

Raster sheets render per camera, each camera's paper applying:

```python
import bpy

scene = bpy.context.scene
for camera in <cameras>:
    scene.camera = camera
    scene.render.filepath = f"<folder>/{camera.name}.png"
    bpy.ops.render.render(write_still=True)
```

Batch Render Cameras queues the same renders as one GUI job:
- `render.refresh_cameras()` fills `scene.camera_items`
- `render.batch_render_cameras("INVOKE_DEFAULT")` under a window override renders each enabled camera
- Later calls read `scene.batch_render_running` and `scene.batch_render_status`

## [11]-[MEASUREIT_ARCH]

MeasureIt_ARCH loads in a GUI alone and serves sheets with labels as font text or DXF output. Dimensions write through its generators, each dimension with its wrapper row:

```python
import bpy

target = bpy.data.objects["<Object>"]
generator = target.DimensionGenerator
dimension = generator.alignedDimensions.add()
dimension.dimObjectA = dimension.dimObjectB = target
dimension.dimPointA, dimension.dimPointB = <vertex_a>, <vertex_b>
dimension.name, dimension.itemType, dimension.textAlignment, dimension.dimViewPlane = "<Label>", "alignedDimensions", "C", "<XY|XZ|YZ>"
style = bpy.context.scene.MeasureItArchProps.default_dimension_style
dimension.style, dimension.uses_style = style, bool(style)
wrapper = generator.wrapper.add()
wrapper.itemType, wrapper.itemIndex = "alignedDimensions", len(generator.alignedDimensions) - 1
generator.measureit_arch_num += 1
```

- Output runs from `scene.ViewGenerator.views[scene.ViewGenerator.active_index]` under a `VIEW_3D` override
- Output views take the scene camera and an `output_path` folder
- `measureit_arch.rendervectorbutton()`, `renderimagebutton()`, and `renderdxfbutton()` write `<output_path>_<view name>_0001.svg`, `.png`, and `.dxf`
- View property writes raise in an update callback before its camera sync
- Sheets set camera `ortho_scale`, paper, and the view's `width` and `height` directly
- Vertex indices past the mesh's count delete the dimension at the next draw
- Image output draws labels from text a viewport draw or earlier render filled
