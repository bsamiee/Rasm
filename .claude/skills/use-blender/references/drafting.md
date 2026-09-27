# [DRAFTING]

Scaled technical drawings come from orthographic cameras, where paper size and scale fix every number, Line Art draws visible edges with hidden lines removed, and `scripts/drawing.py` writes the sheet. Sheets draw in a headless session or a factory run on the saved file. Use `references/bim.md` for drawings of an IFC model.

## [01]-[SCALE]

Orthographic cameras with the default `sensor_fit` of `AUTO` span `ortho_scale` meters across the longer render side:
- `ortho_scale` = long paper side in meters times the scale denominator, ARCH D at 1/4"=1'-0" is `36 * 0.0254 * 48`, A1 at 1:50 `0.841 * 50`
- `resolution_x` and `resolution_y` hold the paper aspect, `round(inches * dpi)` per side for a raster sheet
- Plan cameras sit above the model with rotation `(0, 0, 0)`, elevation cameras take `(pi / 2 + 1e-4, 0, <heading> + 1e-4)`
- Axis-aligned cameras drop every edge with faces that mirror each other exactly, a cylinder's silhouette included, the `1e-4` tilt keeps it
- `clip_start` places a section's cut plane, `use_clip_plane_boundaries` on the modifier draws the cut outline where the plane meets geometry
- `bim.add_section_plane()` adds a `Section` empty and culls every material below its local Z through a material override
- Walls 16 ft long draw 4 in at 1/4"=1'-0", a sphere of radius `r` cut at height `h` draws a circle of radius `sqrt(r^2 - h^2)`
- MeasureIt_ARCH views on an orthographic camera at `res_type` `PAPER` scale text by `model_scale / paper_scale`, other views by `default_scale`

## [02]-[LINEWORK]

```python
# [HEADLESS_CALL] Plan sheet at 1/4"=1'-0" through <camera>, an ORTHO camera above the model, <skill> is this skill's directory
import runpy

import bpy

drawing = runpy.run_path("<skill>/scripts/drawing.py")
scene = bpy.context.scene
scene.camera = bpy.data.objects["<camera>"]
bpy.ops.object.grease_pencil_add(type="LINEART_SCENE")
strokes = bpy.context.view_layer.objects.active
strokes.modifiers[0].use_custom_camera = True
strokes.modifiers[0].source_camera = scene.camera
strokes.update_tag()
bpy.context.view_layer.update()
result = drawing["as_result"](drawing["sheet"]("<name>", 48))
```

- `sheet("<name>", <scale>)` takes every visible Grease Pencil object and the scene camera, `camera=` and `objects=` narrow both
- One Line Art object per camera, `source_camera` binds it, `LINEART_COLLECTION` and `LINEART_OBJECT` limit the source
- `update_tag()` with a depsgraph update recomputes line art after scene edits, and a result without it misses objects added since its last evaluation
- Hand-built Line Art needs a layer frame at the current frame, a layer without one evaluates to no strokes
- `sheet` answers `NotOrthographic` for a perspective camera and `NoStrokes` when Line Art found nothing in view
- Line weight, color, and opacity come from the strokes, a pen of `w` mm at 1:N sets the Line Art `radius` to `w * N / 1000`
- Screen-ink white strokes write as document black on the sheet
- `sheet` writes `<name>.pdf` beside the SVG on a page of the paper size through `typst`, `NoPdf` holds its diagnostics
- `resvg --background white <file>.svg <file>.png` rasterizes a sheet for `Read`

## [03]-[DIMENSIONS]

Persistent dimensions bake to Grease Pencil, headless included, `scene.dimensions_settings` holds style and units:
1. In Edit Mode with one edge selected, `bpy.ops.dimensions.dimension_selected_edge()` adds a linear dimension
2. Set `scene.camera` to the sheet camera and `output_sizing_mode` `WORLD`, `output_world_text_height` the paper text height times the scale
3. `bpy.ops.dimensions.generate_output()` adds one Grease Pencil object, `sheet` writes it with the linework

- Angle and area dimensions come from `angle_selected_edges` and `area_selected_faces`
- `unit_style` `AUTO` follows scene units (a 2 m edge reads `6' 6 3/4"` under FEET), a metric sheet sets `MILLIMETERS` or `METERS`
- Labels face `scene.camera`, with none set they lie flat in XY
- `CAMERA` sizing takes `ortho_scale / resolution_y` per pixel, so a landscape sheet draws labels `resolution_x / resolution_y` too large
- `output_world_text_height` is a cap height, a text object's `size` is an em size, the cap height divided by the face's cap ratio
- Dimensions and guides join the collection whose `dimensions_collection_role` reads `DIMENSIONS` or `GUIDES`
- Without one they join a scene collection named `Dimensions` or `Construction Guides` and stamp its role, else a new root collection
- Text objects join the active collection, MeasureIt_ARCH items sit on their host, Bonsai annotations join their drawing camera's collection

## [04]-[PARALLEL_VIEWS]

- Axonometric and oblique cameras come from `bpy.pohlke.add_preset_camera("<preset>", ortho_scale=<m>)`, `bpy.pohlke.names` lists the presets
- Calls run under a `VIEW_3D` and `WINDOW` override, in background from the stored screen, without it the operator raises past a half-built camera

## [05]-[SKETCHES]

CAD Sketcher draws constrained 2D profiles as curve objects:
- Sketches are `CURVES` objects parented to a `WP_XY`, `WP_XZ`, or `WP_YZ` empty, a `CAD Sketcher Convert` node modifier meshing them
- `Fill` on the convert modifier fills closed loops
- Constraints sit in `<sketch>.data.sketch_constraints` by kind with curves named by `curve_id`, `solver_state` and `dof` hold the last solve
- `bpy.ops.view3d.slvs_add_sketch_on_plane(plane=)` under a `VIEW_3D` and `WINDOW` override adds and activates an empty sketch on an origin plane
- Drawing tools (`slvs_add_*`) run from invoke under the pointer, and code builds exact geometry through the data API from a sketch's curves
