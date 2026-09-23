# [DRAFTING]

Scaled technical drawings come from orthographic cameras, where paper size and scale fix every number, Line Art draws visible edges with hidden lines removed, and `scripts/drawing.py` writes the sheet. Sheets draw in a headless session or a factory run on the saved file. Models with IFC data take their drawings from `references/bim.md`.

## [01]-[SCALE]

Orthographic cameras with the default `sensor_fit` of `AUTO` span `ortho_scale` meters across the longer render side:
- `ortho_scale` = long paper side in meters times the scale denominator, A4 landscape at 1:50 is `0.297 * 50`
- `resolution_x` and `resolution_y` hold the paper aspect, `round(mm / 25.4 * dpi)` per side for a raster sheet
- Plan cameras sit above the model with rotation `(0, 0, 0)`, elevation cameras take `(pi / 2 + 1e-4, 0, <heading> + 1e-4)`
- Axis-aligned cameras drop every edge with faces that mirror each other exactly, a cylinder's silhouette included, the `1e-4` tilt keeps it
- `clip_start` places a section's cut plane, `use_clip_plane_boundaries` on the modifier draws the cut outline where the plane meets geometry
- Objects 2 m long draw 40 mm at 1:50, a sphere of radius `r` cut at height `h` draws a circle of radius `sqrt(r^2 - h^2)`

## [02]-[LINEWORK]

```python
# [HEADLESS_CALL] Plan sheet at 1:50 through <camera>, an ORTHO camera above the model, <root> is the repository root
import runpy

import bpy

drawing = runpy.run_path("<root>/.claude/skills/use-blender/scripts/drawing.py")
scene = bpy.context.scene
scene.camera = bpy.data.objects["<camera>"]
bpy.ops.object.grease_pencil_add(type="LINEART_SCENE")
strokes = bpy.context.view_layer.objects.active
strokes.modifiers[0].use_custom_camera = True
strokes.modifiers[0].source_camera = scene.camera
strokes.update_tag()
bpy.context.view_layer.update()
result = drawing["as_result"](drawing["sheet"]("<name>", 50))
```

- `sheet("<name>", <scale>)` takes every Grease Pencil object with a Line Art modifier and the scene camera, `camera=` and `objects=` narrow both
- One Line Art object per camera, `source_camera` binds it, `LINEART_COLLECTION` and `LINEART_OBJECT` limit the source
- `update_tag()` with a depsgraph update recomputes line art after scene edits, a stale result misses objects added since its last evaluation
- Hand-built Line Art needs a layer frame at the current frame, a layer without one evaluates to no strokes
- `sheet` answers `NotOrthographic` for a perspective camera and `NoStrokes` when Line Art found nothing in view
- Line weight, color, and opacity come from the strokes, a pen of `w` mm at 1:N sets the Line Art `radius` to `w * N / 1000`
- `sheet` writes `<name>.pdf` beside the SVG on a page of the paper size through `typst`, `NoPdf` holds its diagnostics
- `resvg --background white <file>.svg <file>.png` rasterizes a sheet for `Read`

## [03]-[SECTIONS]

Section toolbox operators cut meshes with a box and export the cut and projection as SVG or DXF, headless included, each reading `scene.sbx_settings`:
1. Select target meshes, `bpy.ops.sbx.create_from_selection(padding=<m>)` makes the active `SectionBox` bound to them
2. Set `sbx_settings.width`, `depth`, `height`, `move_*`, and `rot_*` to place the cut faces, `face_choice` (`PX` to `NZ`) picks the cutting face
3. `bpy.ops.sbx.section_lines()` generates the section of that face into a new iteration collection
4. Set `sbx_settings.export_path_box`, then `bpy.ops.sbx.export_box_svg()` or `export_box_dxf()` writes the last iteration

- Export before a `section_lines` run answers `CANCELLED` with `No box iteration found`
- SVG coordinates are meters as unitless user units, a sheet at 1:N sets `width` and `height` to `<meters * 1000 / N>mm`

## [04]-[DIMENSIONS]

Persistent dimensions bake to Grease Pencil, headless included, `scene.dimensions_settings` holds style and units:
1. In Edit Mode with one edge selected, `bpy.ops.dimensions.dimension_selected_edge()` adds a linear dimension
2. Set `scene.camera` to the sheet camera, `output_sizing_mode` `CAMERA` lays text in that camera's plane
3. `bpy.ops.dimensions.generate_output()` adds one Grease Pencil object, `sheet` writes it with the linework

- Angle and area dimensions come from `angle_selected_edges` and `area_selected_faces`
- `unit_style` `AUTO` follows scene units (a 2 m edge reads `6' 6 3/4"` here), a metric sheet sets `MILLIMETERS` or `METERS`
- Dimensions generated before the camera is set bake garbled text

## [05]-[PARALLEL_VIEWS]

- Axonometric and oblique cameras come from `bpy.pohlke.add_preset_camera("<preset>", ortho_scale=<m>)`, `bpy.pohlke.names` lists the presets
- Calls run under `temp_override` with a `VIEW_3D` area and its `WINDOW` region, in background from the file's stored screen
- Without the override the operator raises after leaving a half-built camera that is not the scene camera
