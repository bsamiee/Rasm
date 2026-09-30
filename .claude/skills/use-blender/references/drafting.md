# [DRAFTING]

Scaled drawings from an orthographic camera and Line Art, written by `sheet` in a headless session or factory run on the saved file.

## [01]-[SCALE]

Orthographic cameras with the default `sensor_fit` of `AUTO` span `ortho_scale` meters across the longer render side:
- `ortho_scale` = long paper side in meters times the scale denominator, ARCH D at 1/4"=1'-0" is `36 * 0.0254 * 48`, A1 at 1:50 `0.841 * 50`
- Plan cameras sit above the model with rotation `(0, 0, 0)`, elevation cameras take `(pi / 2 + 1e-4, 0, <heading> + 1e-4)`
- Axis-aligned cameras drop every edge with faces that mirror each other exactly, a cylinder's silhouette included, the `1e-4` tilt keeps it
- `clip_start` places a section's cut plane, `use_clip_plane_boundaries` on the modifier draws the cut outline where the plane meets geometry
- `bim.add_section_plane()` adds a `Section` empty at the cursor in a `Sections` collection and culls below its local Z through a material override
- MeasureIt_ARCH views on an orthographic camera at `res_type` `PAPER` scale text by `model_scale / paper_scale`, other views by `default_scale`

## [02]-[LINEWORK]

```python
# [HEADLESS_CALL] Plan sheet at 1/4"=1'-0" through <camera>, an ORTHO camera above the model
import bpy
from drawing import sheet
from results import as_result

scene = bpy.context.scene
scene.camera = bpy.data.objects["<camera>"]
bpy.ops.object.grease_pencil_add(type="LINEART_SCENE")
strokes = bpy.context.view_layer.objects.active
strokes.modifiers[0].use_custom_camera = True
strokes.modifiers[0].source_camera = scene.camera
strokes.update_tag()
bpy.context.view_layer.update()
result = as_result(sheet("<name>", 48))
```

- `sheet("<name>", <scale>)` takes every visible Grease Pencil object and the scene camera, `camera=` and `objects=` narrow both
- One Line Art object per camera, `source_camera` binds it, `LINEART_COLLECTION` and `LINEART_OBJECT` limit the source
- `update_tag()` with a depsgraph update recomputes line art after scene edits, and a result without it misses objects added since its last evaluation
- Grease Pencil data sits in `bpy.data.grease_pencils` on `GREASEPENCIL` objects, and Line Art is the `LINEART` modifier
- `LINEART` modifiers write into their target layer at the current frame, a layer with no frame included
- `sheet` answers `UnknownObjects` for a missing name, `Rejected` with `not_orthographic` for a perspective camera, `no_strokes` for empty Line Art
- Line weight, color, and opacity come from the strokes, a pen of `w` mm at 1:N sets the Line Art `radius` to `w * N / 1000`
- Screen-ink white strokes write as document black on the sheet
- `sheet` writes `<name>.svg` in paper inches and `<name>.pdf` on a page of the paper size through `typst`, `NoPdf` holds its diagnostics
- `resvg --background white <file>.svg <file>.png` rasterizes a sheet for `Read`

## [03]-[DIMENSIONS]

Dimensions add-on output bakes to Grease Pencil, headless included, `scene.dimensions_settings` holds style and units:
1. In Edit Mode with one edge selected, `bpy.ops.dimensions.dimension_selected_edge()` adds a linear dimension
2. Set `scene.camera` to the sheet camera and `output_sizing_mode` `WORLD`, `output_world_text_height` the paper text height times the scale
3. `bpy.ops.dimensions.generate_output()` replaces one Grease Pencil object per dimension in the `Dimensions Output` collection

- Angle and area dimensions come from `angle_selected_edges` and `area_selected_faces`
- `imperial_unit_style` and `metric_unit_style` set the label form under each unit system
- `AUTO` style follows `length_unit`, a 2 m edge reads `6' 6 3/4"` under FEET
- `unit_style` applies under system `NONE` alone
- Labels face `scene.camera`, with none set a label lies in its dimension's offset plane
- `CAMERA` sizing takes `ortho_scale / resolution_y` per pixel and draws labels on a landscape sheet `resolution_x / resolution_y` too large
- `output_world_text_height` is a cap height, a text object's `size` is an em size, the cap height divided by the face's cap ratio
- Dimensions and guides join the collection with `dimensions_collection_role` `DIMENSIONS` or `GUIDES`
- Without a role collection, dimensions and guides join a scene collection named `Dimensions` or `Construction Guides` and set its role, else a new root collection
- MeasureIt_ARCH items sit on their host, Bonsai annotations join their drawing camera's collection
- MeasureIt_ARCH raises at import in a `--background` process, headless dimensions come from the Dimensions add-on

## [04]-[PARALLEL_VIEWS]

- Axonometric and oblique cameras come from `bpy.pohlke.add_preset_camera("<preset>", ortho_scale=<m>)`, `bpy.pohlke.names` lists the presets
- Calls run under a `VIEW_3D` and `WINDOW` override, in background from the stored screen, without the override the operator raises past a half-built camera

## [05]-[SKETCHES]

CAD Sketcher draws constrained 2D profiles as curve objects:
- Sketches are `CURVES` objects parented to a workplane empty under a `Body` mesh with a `CAD Sketcher Convert` node modifier meshing them
- `Fill` on the convert modifier fills closed loops
- Constraints sit in `<sketch>.data.sketch_constraints` by kind with curves named by `curve_id`
- `<sketch>["solver_state"]` and `<sketch>["dof"]` hold the last solve
- `bpy.ops.view3d.slvs_add_sketch_on_plane(plane=)` under a `VIEW_3D` and `WINDOW` override adds and activates an empty sketch on an origin plane
- Drawing tools (`slvs_add_*`) run from invoke under the pointer and leave curves when they raise
- Code builds exact sketch geometry through the data API
