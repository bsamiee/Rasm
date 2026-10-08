---
name: use-blender
description: "Use when a task drives a live or headless Blender or a .blend file, covering sessions, execution, captures, nodes, configuration, interface, extensions, modeling, materials, animation, rendering, drafting, interchange, BIM, and geospatial work."
---

# [BLENDER]

`.mcp.json` servers `blender` and `mcp-for-blender` share one GUI Blender process. `headless.py` runs background processes. Project hook runs `execute_blender_code` through `wrapper.run` with script modules imported fresh. Snippets assign a `result` dict with `as_result` adding each case record's class name under `kind`:
- `[EXECUTE_BLENDER_CODE]` snippets run through `blender` `execute_blender_code`, `[MCP_FOR_BLENDER]` through `mcp-for-blender`
- `[HEADLESS_CALL]` snippets run through `headless.py call`, or through `run` when they call no extension operator

[REFERENCES]:
- [01]-[SESSIONS](references/sessions.md): Servers, file routes, headless commands, background context, outcomes, quits, and failures
- [02]-[EXECUTION](references/execution.md): Undo steps, operator context and readings, deferred passes, and faults
- [03]-[NODES](references/nodes.md): Node trees of every kind, sockets, new and existing trees, modifier inputs, evaluated results, and Sverchok
- [04]-[CONFIGURATION](references/configuration.md): Preferences, add-on records, repositories, keymaps, themes, scale and fonts, and scene units
- [05]-[INTERFACE](references/interface.md): Second windows, views, workspaces, regions, panels, popups, and stored screens
- [06]-[EXTENSIONS](references/extensions.md): Packages, isolated trees, installs, upgrades, removals, and registration with its reads
- [07]-[MODELING](references/modeling.md): Parts built from dimensions with cutters and arrays, edits of existing parts, and joins
- [08]-[LOOK_DEVELOPMENT](references/look-development.md): Materials, library and generated assets, site sky and sun, lights, and the asset library
- [09]-[ANIMATION](references/animation.md): Layered actions, slots, channelbags, and keyframes of new and existing motion
- [10]-[RENDERING](references/rendering.md): Render settings, headless renders, look renders, passes, and renders of the live scene
- [11]-[DRAFTING](references/drafting.md): Sketches, sheet cameras, Line Art, dimensions, and sheets at scale
- [12]-[INTERCHANGE](references/interchange.md): Imports per format, Rhino round trips, CAD file authoring, and batch conversion
- [13]-[BIM](references/bim.md): IFC authoring, Bonsai projects, and IFC drawings and sheets
- [14]-[GEOSPATIAL](references/geospatial.md): Georeference, sun position, Bonsai solar, GIS imports, and climate files

[SCRIPTS]:
- [01]-[HEADLESS](scripts/headless.py): Background Blender runs, sessions, and frame renders with a JPEG sheet of a named file
- [02]-[WRAPPER](scripts/wrapper.py): PreToolUse hook closing each `execute_blender_code` call with an undo step
- [03]-[BRIDGE](scripts/bridge.py): Host client of the MCP extension's execute protocol on a session's loopback port
- [04]-[RESULTS](scripts/results.py): Faults every script returns, `result` dict conversion of case records, and `.artifacts/blender/` folders
- [05]-[DISCOVER](scripts/discover.py): Operators, RNA types, and add-on settings matching words across stock Blender and every enabled add-on
- [06]-[RNA](scripts/rna.py): Stored RNA values of a struct as JSON and the function of every registered operator
- [07]-[SCENE](scripts/scene.py): Evaluated points and world bounds of scene objects and the largest 3D Viewport of open or stored windows
- [08]-[CAPTURE](scripts/capture.py): One scene view as `.artifacts/blender/<name>.png` with the user's view kept, diffed against an earlier one
- [09]-[SNAPSHOT](scripts/snapshot.py): Evaluated scene state written to `.artifacts/blender/<name>.json`, compared against an earlier snapshot
- [10]-[NODES](scripts/nodes.py): Digest of one node tree by socket identifier, and its layout through Node Arrange
- [11]-[DRAWING](scripts/drawing.py): Grease Pencil strokes an orthographic camera sees as SVG, PDF, and PNG sheets at scale
- [12]-[CONVERT](scripts/convert.py): Headless batch of files through importers and one exporter into `.artifacts/blender/convert/<name>/`

Dimensioned examples use `scale_length = 1`. Massing dimensions `width`, `depth`, and `height` are Blender units:

```python
# [EXECUTE_BLENDER_CODE] Massing <Object> in new collection <Collection>, with its evaluated dimensions and an iso capture
import bmesh
import bpy
from capture import capture
from mathutils import Matrix
from results import as_result

mesh, bm = bpy.data.meshes.new("<Object>"), bmesh.new()
bmesh.ops.create_cube(bm, size=1.0, matrix=Matrix.Translation((width / 2, depth / 2, height / 2)) @ Matrix.Diagonal((width, depth, height, 1.0)))
bm.to_mesh(mesh)
bm.free()
part = bpy.data.collections.new("<Collection>")
bpy.context.scene.collection.children.link(part)
massing = bpy.data.objects.new("<Object>", mesh)
part.objects.link(massing)
evaluated = massing.evaluated_get(bpy.context.evaluated_depsgraph_get())
result = {"dimensions": list(evaluated.dimensions), "iso": as_result(capture("<name>", objects=(massing.name,)))}
```

## [01]-[SHARED_PROCESS]

Preserve scene, selection, mode, view, workspace, preferences, and file outside requested changes:
- Calls run on the GUI's main thread between event-loop passes while the window and other clients wait
- Other agents' calls run between a task's calls, each call reading the state it depends on
- Live calls mark the file modified and delete the user's redo steps
- Trial writes revert in the call's `finally`, longer trials running in a `headless.py start <copy> <name>` session on a copy
- Sessions take a name no other agent holds and stop at task end
- Scripted GUI launches under `tools/interface` wait while another agent drives Blender, one apply at a time
- Quits from code end a GUI the task launched alone, through `mcp-for-blender`
- Tasks remove windows, data-blocks, handlers, timers, packages, and files they added apart from their products

## [02]-[START]

Tasks orient in order before any change:
1. `pgrep -lf "^$BLENDER_PATH"` lists every Blender with its arguments, the GUI line holding none of `-b`, `--background`, `-c`, or `--command`
2. Tasks finding no GUI line launch one through the sessions reference's launch command
3. `lsof -a -nP -iTCP -sTCP:LISTEN -c Blender` lists ports 9877 and 9876 on the GUI pid and one loopback port per session
4. Sessions reference's route table takes each target: the live file, a copy in a session, a closed file, or a new file
5. One live call reads the fields that decide the next step:

```python
# [EXECUTE_BLENDER_CODE] Live file, scene, selection, and view the next step depends on
import bpy
from scene import viewport

scene, view, layer = bpy.context.scene, viewport(), bpy.context.view_layer
units = scene.unit_settings
result = {
    "file": bpy.data.filepath or None,
    "dirty": bpy.data.is_dirty,
    "scene": scene.name,
    "scenes": [s.name for s in bpy.data.scenes],
    "units": [units.system, units.length_unit, units.scale_length],
    "mode": bpy.context.mode,
    "active": layer.objects.active and layer.objects.active.name,
    "selected": [o.name for o in layer.objects if o.select_get()],
    "collections": [c.name for c in scene.collection.children_recursive],
    "camera": scene.camera and scene.camera.name,
    "engine": scene.render.engine,
    "look": [scene.view_settings.view_transform, scene.view_settings.exposure],
    "frame": scene.frame_current,
    "workspace": bpy.context.window.workspace.name,
    "view": view and [view.space.region_3d.view_perspective, view.space.shading.type, view.space.local_view is not None],
}
```

| [INDEX] | [FIELD]                    | [DECIDES]                                                                          |
| :-----: | :------------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `file`, `dirty`            | File a save writes, `None` for an untitled file, `True` marking edits a copy keeps |
|  [02]   | `scene`, `scenes`          | Scene context reads and operators act on                                           |
|  [03]   | `units`                    | Units the user reads and types                                                     |
|  [04]   | `mode`                     | Operators polling by mode, and the form of a mesh edit                             |
|  [05]   | `active`, `selected`       | Objects the user means by "this" or "these"                                        |
|  [06]   | `collections`              | Hierarchy new objects join                                                         |
|  [07]   | `camera`, `engine`, `look` | Camera, engine, view transform, and exposure renders take                          |
|  [08]   | `frame`                    | Frame evaluated reads use and `frame_set` returns to                               |
|  [09]   | `workspace`, `view`        | Screen and 3D view the user looks at                                               |

## [03]-[ROUTING]

Each job takes one call across both servers and `scripts/`:

| [INDEX] | [JOB]                        | [CALL]                                                                                              |
| :-----: | :--------------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | Live file path and state     | `get_blendfile_summary_path_info` with path, saved and dirty flags, age, and backups                |
|  [02]   | Live file contents           | `get_blendfile_summary_datablocks` with counts per ID kind, active workspace, and render engine     |
|  [03]   | Missing external files       | `get_blendfile_summary_missing_files` with every external file reference absent from disk           |
|  [04]   | Linked libraries             | `get_blendfile_summary_of_linked_libraries` with direct and indirect library files                  |
|  [05]   | Closed file state            | `snapshot("<name>")` through `headless.py run <file>`                                               |
|  [06]   | Scene inventory              | `get_objects_summary` with the collection tree and parent, selection, and visibility per object     |
|  [07]   | One object's stacks          | `get_object_detail_summary(name=)` with transforms, stacks, materials, visibility, and collections  |
|  [08]   | Evaluated state              | `snapshot("<name>", objects=, since=)`                                                              |
|  [09]   | Stored values of a struct    | `plain(<struct>)` as JSON, floats at `Object.location` precision and IDs by name                    |
|  [10]   | Capability by words          | `discover("<word>", ...)`                                                                           |
|  [11]   | Operator or type members     | `bpy_api_lookup(query=)` with enum items, defaults, and ranges of operators and exposed types       |
|  [12]   | Stock class or module page   | `get_python_api_docs(identifier=)` with prose and `examples`, `X.*` listing a namespace             |
|  [13]   | Stock member or phrase       | `search_api_docs(query=, context=)` ranked over bundled pages, members outside `bl_rna` included    |
|  [14]   | Concept or workflow          | `search_manual_docs(query=, context=)` over the bundled manual of stock Blender                     |
|  [15]   | Node sockets per mode        | `describe_node_type(bl_idname=, property_overrides=)` of an exposed type                            |
|  [16]   | Existing node tree           | `digest("<name>")`                                                                                  |
|  [17]   | Node tree layout             | `arrange("<tree>")` through Node Arrange, node selection kept                                       |
|  [18]   | Extension on the platform    | `blender -c extension list` with every repository's local index and `[installed]` per package       |
|  [19]   | Batch of interchange files   | `convert("<name>", "<exporter>", sources, options=)`                                                |
|  [20]   | GLB or FBX of the live scene | `export_scene(filepath=, format=, selection_only=)` with modifiers baked under `apply_modifiers`    |
|  [21]   | Geometry from a chosen view  | `capture("<name>", objects=, view=, size=, since=)` with the user's view kept                       |
|  [22]   | Editor, panel, node tree     | `get_screenshot_of_area_as_image(area_ui_type=)` at device pixels, another application in front     |
|  [23]   | Whole window with chrome     | `screencapture -x -o -l <id>` with top bar and status bar                                           |
|  [24]   | Layout, mode, selection      | `get_screenshot_of_window_as_json` with areas, shading, view, and active object with mode           |
|  [25]   | Camera render                | `headless.py render <file> --frames <frames>`                                                       |
|  [26]   | Library or generated asset   | `mcp-for-blender` search, preview, download, and generate tools, one call importing an asset        |
|  [27]   | Show the user an object      | `viewport()` framed on `bounds(depsgraph, drawn=True)`                                              |
|  [28]   | Show the user an editor      | `jump_to_tab_by_space_type(space_type=)` showing the workspace whose main area holds the space type |
|  [29]   | Show the user a workspace    | `jump_to_tab_by_name(name=)`                                                                        |

- `mcp-for-blender` calls leave `user_prompt` empty, a field for the data collection `telemetry_consent` False turns off
- `export_scene(object_names=)` leaves the named objects selected and active, and exports no object with `hide_select` set

```bash
# Packages of every repository's local index matching <word>, under the user's preferences
blender -c extension list | rg -i '^  \S+( \[installed\])?: .*<word>'
```

## [04]-[DISCOVERY]

`discover("<word>", ...)` lists stock Blender and enabled add-on members holding every word, case-folded, in their name, label, or description:

```python
# [EXECUTE_BLENDER_CODE] Operators, types, and add-on settings holding every word
from discover import discover
from results import as_result

result = as_result(discover("<word>", "<word>"))
```

```python
# [HEADLESS_CALL] Add-on setting values of a closed file, the session returned to the file on disk in the same call
import bpy
from discover import discover
from results import as_result

result = as_result(discover("<word>", "<word>"))
bpy.ops.wm.revert_mainfile()
```

1. Pass words naming a capability, including add-on prefixes (`bim`)
2. Run it live for what exists, the GUI holding every add-on a session loads
3. Run the session form for a closed file's setting values, add-on polls and group reads creating group storage the revert drops
4. Call an operator with `poll` true as is, one with `poll_in_view` true alone under the largest 3D Viewport override
5. Read `owner` (enabled add-on module, `None` for Blender) and `source` (file defining a Python class) before calling an extension operator
6. Read `params` for the keywords, then `bpy_api_lookup("bpy.ops.<category>.<name>")` for the values each keyword takes
7. Read a type with `exposed` true through `bpy_api_lookup("<identifier>")` and a node type's sockets through `describe_node_type`
8. Read a type with `exposed` false through `bpy.types.<base>.bl_rna_get_subclass_py("<identifier>").bl_rna` in code
9. Read a compiled stock type (`owner` and `source` `None`) through `get_python_api_docs("bpy.types.<identifier>")`, a Python class from `source`
10. Read a setting's `values` by ID name for each context member of its ID type, by module for add-on preferences, `PASSWORD` strings left out
11. Write a setting through `bpy.context.<member>.<property>` or `bpy.context.preferences.addons["<module>"].preferences.<property>`

- `poll_in_view` reads `None` in a background process and in a window with no 3D Viewport
- Add-on poll and enum callback tracebacks stay out of the answer, a raising poll reading `False`

## [05]-[SCRIPTS]

`execute_blender_code` calls and `headless.py` `run` and `call` code compile under `<agent>` in a fresh namespace. Snippets import `results`, `scene`, `rna`, `discover`, `capture`, `snapshot`, `nodes`, `drawing`, and `convert` by module name.

Snippets hold each rule:
- Globals reset per call, later calls reaching data by name through `bpy.data.<collection>["<name>"]`
- New data-blocks take a `.001` suffix on a name another holds, later code reading the `name` the new ID took
- Use [configuration.md](references/configuration.md) for scene units
- Use data-API writes where available, with target scene overrides for context-reading property callbacks
- Calls hold the first window with no area or region
- State an event-loop pass applies (workspace switches, area sizes, view matrices) reads in a deferred step or the next call
- Values after modifiers read through `obj.evaluated_get(bpy.context.evaluated_depsgraph_get())`
- `matrix_world` holds the earlier placement until `bpy.context.view_layer.update()` evaluates a transform written in the call
- Static enum identifiers come from `bl_rna.properties["<member>"].enum_items`, a rejected write's `TypeError` naming a dynamic enum's set
- Dynamic enums (`render.engine`, `view_transform`, `look`, `length_unit`) answer their static placeholder through `enum_items`
- GUI and session calls run in `.artifacts/blender/`, the parent of `filepaths.render_output_directory` the interface extension enters

## [06]-[RESULTS]

`blender` `execute_blender_code` answers `status` `ok` with `result`, `stdout`, and `stderr`, or `error` with `message` and `stdout`:

| [INDEX] | [PART]                         | [HOLDS]                                                 | [NEXT]                             |
| :-----: | :----------------------------- | :------------------------------------------------------ | :--------------------------------- |
|  [01]   | `result`                       | Assigned dict, `repr` text of a value JSON cannot hold  | Convert the value in code          |
|  [02]   | `stdout`                       | Prints and operator report lines                        | Operator readings                  |
|  [03]   | `stderr`                       | `<file>:<line>: <Category>: <text>`, handler tracebacks | Replace the member a line names    |
|  [04]   | `message`                      | Traceback, `stdout` before the raise beside it          | Read the state, resume at the line |
|  [05]   | `Blender connection timed out` | Server's 300 s wait ended                               | Read state                         |

- Warnings print once per line per call, add-on files included
- Tracebacks and warnings count `<agent>` lines as sent, a traceback's last `File "<agent>", line <n>` frame naming the sent line
- `mcp-for-blender` `execute_blender_code` answers printed output alone and drops `result`, a raise answering with its traceback

Use [execution.md](references/execution.md) for script faults and deferred passes.

## [07]-[CAPTURE]

`capture("<name>")` draws `.artifacts/blender/<name>.png` and returns `Capture(path, view, camera, comparison)`, then `Read` of `path` shows it:
1. `capture("<name>")` frames every object the largest 3D Viewport shows from `iso`, one frame in every shading and process
2. `capture("<name>", view="user")` draws what the user sees
3. Table arguments narrow the picture

| [INDEX] | [ARGUMENT]                 | [DRAWS]                                                                                |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `objects=("<Object>",)`    | Objects' evaluated boxes, each side bordered by 2.5% of projected extent's longer side |
|  [02]   | `view="iso"`               | Perspective at default lens's fit distance, sensor widened for border, centering shift |
|  [03]   | `view="<axis>"`            | `top`, `bottom`, `front`, `back`, `right`, or `left` orthographic over extent          |
|  [04]   | `view="user"`              | Largest 3D Viewport's view at its region's aspect                                      |
|  [05]   | `size=(<width>, <height>)` | Exact size, default 300 thousand pixels at view's aspect                               |
|  [06]   | `since="<before>"`         | View, camera, and size `<before>.png` stores, with pixels changed against it           |

- Live calls draw offscreen through the viewport's shading, Solid, Material Preview, and EEVEE Rendered alike, with overlays off
- Rendered shading under Cycles draws black offscreen, and Cycles looks take `headless.py render`
- Background calls render Workbench under the stored viewport's Solid shading, surfaces matching a live draw byte for byte
- Background processes hold the startup file's screens under `filepaths.use_load_ui` off, its shading and user view in place of the live ones
- Background fill takes the preferences' theme as a Custom background color, lighter under `run`'s factory preferences than in a `start` session
- Workbench renders draw Theme and World backgrounds in the world's viewport color, black in a scene with no world
- `WIRE` and `BOUNDS` display types, curves with no bevel or extrusion, loose edges, and empties draw nothing
- `Read` downscales an image past 2000 px and re-encodes one past 500 KB as JPEG, default sizes staying under both up to a 13:1 frame
- Pixel reads (levels, line widths, band edges, dither) pass `size=(region.width, region.height)` for the viewport's device pixels
- `magick <png> -crop <width>x<height>+<x>+<y> +repage <crop>.png` cuts a detail of a larger `size` that `Read` shows whole
- PNGs hold no ICC profile, `magick <png> -format "%[pixel:p{<x>,<y>}]" info:` reading a byte as drawn

## [08]-[COMPARISONS]

Each change ends with one call recording evaluated state and drawing changed objects against earlier records:
1. `snapshot("<before>")` and `capture("<before>-<view>", objects=)` before the first edit, or after a new target's build
2. Edits under comparison, one capture per view the work judges
3. `snapshot("<after>", since="<before>")` and `capture("<after>-<view>", objects=, since="<before>-<view>")`, `<after>` naming the next baseline
4. `Read` of `<after>-<view>-diff.png`, the earlier capture at half brightness with changed pixels mixed half with red

```python
# [EXECUTE_BLENDER_CODE] Snapshot and front capture of the changed object against earlier ones
from capture import capture
from results import as_result
from snapshot import snapshot

result = {"changes": as_result(snapshot("<after>", since="<before>")), "front": as_result(capture("<after>-front", objects=("<Object>",), since="<before>-front"))}
```

- Snapshots record transforms, evaluated bounds, stacks, drivers, actions, and geometry counts
- Geometry hashes cover rounded evaluated positions and instance transforms
- Snapshots record scene settings, orphans, missing paths, material settings, datablock counts per kind, and node tree digests
- Capture PNGs store framing in a `Capture` text chunk for `since` comparisons after bounds or views move
- Live draws and background renders frame alike and differ on antialiased edges, a comparison taking both captures from one kind of process

Readings of each comparison decide the next step:

| [INDEX] | [READING]                                  | [NEXT_STEP]                                                   |
| :-----: | :----------------------------------------- | :------------------------------------------------------------ |
|  [01]   | Snapshot `changed` holds edited values     | Read diff PNG for where edit shows                            |
|  [02]   | Snapshot `changed` holds unedited value    | Trace its owner (modifier input, driver, node link) first     |
|  [03]   | Snapshot `added` or `removed` lists names  | Check count against edit, `datablocks` counting each ID kind  |
|  [04]   | Hash `shape` alone changed                 | Read counts beside it or F-curve keys                         |
|  [05]   | Capture `outside` true on iso or axis view | Capture again without `since`, framing new extent whole       |
|  [06]   | Capture `changed` 0 after material edit    | Read snapshot, Solid drawing `color_type` `MATERIAL` colors   |
|  [07]   | Capture `changed` 0 after visible edit     | Capture another view, or `view="user"` under viewport shading |

- `outside` on a user view reads true while objects extend past the user's view

## [09]-[WINDOW]

Window pictures start from the GUI pid's window id:

```bash
# Layer-0 windows of the GUI pid pgrep names, with id, on-screen flag, and title `<file> — Blender <version>` (`* ` while dirty)
uv run --with pyobjc-framework-Quartz python -c 'import sys, Quartz as q; [print(w[q.kCGWindowNumber], w.get(q.kCGWindowIsOnscreen), w.get(q.kCGWindowName)) for w in q.CGWindowListCopyWindowInfo(q.kCGWindowListOptionAll | q.kCGWindowListExcludeDesktopElements, q.kCGNullWindowID) if w[q.kCGWindowOwnerPID] == int(sys.argv[1]) and w[q.kCGWindowLayer] == 0]' <pid>

# Window at 1:1 device pixels under the display's ICC profile, without activating it
screencapture -x -o -l <id> .artifacts/blender/<name>.png

# Readable copy in sRGB at logical pixels as an undithered palette
magick .artifacts/blender/<name>.png -profile "/System/Library/ColorSync/Profiles/sRGB Profile.icc" -resize 50% -strip +dither -colors 256 PNG8:.artifacts/blender/<name>-read.png

# One area at device pixels from its `x`, `y`, `width`, and `height`, `y` counted from the window's bottom edge
magick .artifacts/blender/<name>.png -profile "/System/Library/ColorSync/Profiles/sRGB Profile.icc" -gravity SouthWest -crop <width>x<height>+<x>+<y> +repage .artifacts/blender/<name>-<area>.png
```

- Window ids change at every launch
- `could not create image from window` answers a stale id or a capture refused at that moment alike
- `blender` screenshots past 785 KB shrink bilinearly to logical pixels, then until they fit, with no note in the answer
- Window screenshots show a theme or view write after `area.tag_redraw()` on every area and a return from the call
- Region writes that rebuild no region (`active_panel_category`) show a frame later
