---
name: use-blender
description: "Use when a task drives a Blender session, a headless Blender, or a .blend file through its MCP servers and scripts, covering conduct, files, routing, execution, headless work, nodes, captures, configuration, interface, and extensions."
---

# [BLENDER]

`.mcp.json` runs `blender` (Blender Lab) and `mcp-for-blender` against the user's Blender GUI. `scripts/headless.py` runs code, sessions, and renders in background Blender processes.

[REFERENCES]:
- [01]-[SETUP](references/setup.md): Servers, machine prerequisites, startup state, processes, and failures
- [02]-[CONFIGURATION](references/configuration.md): Preference stores, properties, add-on records, repositories, keymaps, themes, fonts, and units
- [03]-[INTERFACE](references/interface.md): Event-loop timing, workspaces, areas, regions, panels, views, and pictures of the window
- [04]-[EXTENSIONS](references/extensions.md): Package layout, manifests, installs, wheels, and registration
- [05]-[MODELING](references/modeling.md): Meshes from dimensions, modifier stacks, cutters, Edit Mode, joins, collections, and parenting
- [06]-[RENDERING](references/rendering.md): Engines, render settings, the render command, compositor, and output media
- [07]-[LOOK_DEVELOPMENT](references/look-development.md): Materials and color spaces, worlds, lights, downloaded assets, and asset libraries
- [08]-[ANIMATION](references/animation.md): Layered actions, slots, and keyframes
- [09]-[DRAFTING](references/drafting.md): Sheet scale, Line Art, sections, dimensions, sketches, and SVG and PDF sheets
- [10]-[GEOMETRY_NODES](references/geometry-nodes.md): Node groups from code, modifier inputs, evaluated results, and Sverchok trees
- [11]-[INTERCHANGE](references/interchange.md): CAD and city model import and authoring, Rhino materials, and batch conversion
- [12]-[BIM](references/bim.md): IFC authoring, Bonsai load behavior, and IFC drawings and sheets
- [13]-[GEOSPATIAL](references/geospatial.md): Georeferencing, GIS imports, sun position, and climate studies

[SCRIPTS]:
- [01]-[HEADLESS](scripts/headless.py): Blender processes outside the live session through `run`, `start`, `call`, `stop`, and `render`
- [02]-[WRAPPER](scripts/wrapper.py): Hook importing `scripts/` fresh into every live and headless call
- [03]-[DISCOVER](scripts/discover.py): Operators, RNA types, and add-on settings matching words, operators and types naming the next tool
- [04]-[CAPTURE](scripts/capture.py): Framed view of named or visible objects, or the user's view, to `.artifacts/blender/<name>.png` with a diff against an earlier one
- [05]-[SNAPSHOT](scripts/snapshot.py): Evaluated scene state to `.artifacts/blender/<name>.json` with the changes since an earlier snapshot
- [06]-[NODES](scripts/nodes.py): One node tree as interface, nodes with non-default values, and links by socket identifier
- [07]-[DRAWING](scripts/drawing.py): Grease Pencil strokes through an orthographic camera as an SVG and PDF sheet at scale
- [08]-[CONVERT](scripts/convert.py): Interchange files through Blender's importers and one exporter, one empty scene per file in a headless process
- [09]-[RESULTS](scripts/results.py): Case records as `result` dicts and JSON, `.artifacts/blender/` folders, and the unknown-object case
- [10]-[RNA](scripts/rna.py): RNA values as JSON and the function of every registered operator
- [11]-[SCENE](scripts/scene.py): Evaluated world bounds per object and the largest 3D Viewport

`headless.py` and hook `wrapper.py` run on the host under `python`, every other script runs inside Blender imported by module name. `as_result` converts an entry point's `attrs` case record to a `result` dict with class name under `kind` at every level.

## [01]-[CONDUCT]

Work leaves the user's GUI as found:
- Reads and writes go through `bpy` with Blender behind the frontmost application
- Summary and screenshot tools of both servers leave undo history and `bpy.data.is_dirty` as found
- Each opened session ends through `stop`, each opened GUI through one quit call

## [02]-[FILES]

Work runs live or headless on named files:

```bash
# Every running Blender, a line without --background is the GUI both servers answer
pgrep -lf Blender.app/Contents/MacOS/Blender

# GUI on <file> behind the frontmost application, with login session environment in place of shell environment
env -i /usr/bin/open -g -a Blender <file> --args --no-window-focus
```

```python
# [EXECUTE_BLENDER_CODE] Live file saved before a headless process reads it
import bpy

if bpy.data.filepath:
    bpy.ops.wm.save_mainfile()
result = {"file": bpy.data.filepath}
```

- When every `pgrep` line holds `--background`, launch a GUI with `open`
- Live calls answer once their server's add-on listens, `_for_cli` tools answer without one
- `_for_cli` tools run the user's add-ons with no sandbox, stop at 120 s, and save then delete `<stem>_mcp_<n>.blend` beside a dirty GUI file
- Kept work saves to a named file through `bpy.ops.wm.save_as_mainfile(filepath="<dir>/<name>.blend")`
- Quits and preference reloads run through `mcp-for-blender`
- `blender` raises on `sys.exit`, `wm.quit_blender`, `read_userpref`, `read_factory_settings`, and `read_factory_userpref`, `read_homefile` runs
- `mcp-for-blender` `execute_blender_code` answers with printed output and drops `result`, a raise answers with the traceback alone
- `wm.quit_blender()` from code quits with no prompt, and the server answers before Blender exits
- Exit reads from `ps -p <pid>` in a later call, `kill -KILL <pid>` ends a Blender `ps` lists after the quit

```python
# [MCP_FOR_BLENDER] Quit, saving a titled file with unsaved changes and discarding an untitled one
import bpy

status = bpy.ops.wm.save_mainfile(exit=True) if bpy.data.filepath and bpy.data.is_dirty else bpy.ops.wm.quit_blender()
print(sorted(status))
```

## [03]-[ROUTING]

Each job has one tool across both servers and `scripts/`, over any tool server instructions name:

| [INDEX] | [JOB]                        | [TOOL]                             | [DECIDING_FACT]                                                     |
| :-----: | :--------------------------- | :--------------------------------- | :------------------------------------------------------------------ |
|  [01]   | Scene inventory              | `get_objects_summary`              | Collection tree with parent, selection, and visibility per object   |
|  [02]   | One object's stack           | `get_object_detail_summary`        | Modifiers, constraints, slots, collections, rotation read as Euler  |
|  [03]   | Live file inventory          | `get_blendfile_summary_*`          | Datablock counts, missing files, linked libraries, paths, and usage |
|  [04]   | Change the user's scene      | `blender` `execute_blender_code`   | `result` dict, stdout, stderr, and traceback per call               |
|  [05]   | Long job or build on a file  | `headless.py start`, `call`        | State kept between calls, every extension, no client timeout        |
|  [06]   | Code on a closed `.blend`    | `headless.py run <file>`           | Factory start, `-Y` on files outside the repository                 |
|  [07]   | Stock export or CAD output   | `headless.py run`                  | Stock exporters and CAD wheels in a factory start                   |
|  [08]   | Still, frame range, video    | `headless.py render`               | User's Cycles device, frames resumed after a stop                   |
|  [09]   | Batch of interchange files   | `scripts/convert.py`               | Headless process, importer per suffix, one outcome per file         |
|  [10]   | GLB or FBX of the live scene | `export_scene`                     | Whole scene, selection, or named objects selected with children     |
|  [11]   | Stock class or module page   | `get_python_api_docs`              | Bundled reference with `examples`, `X.*` lists a namespace          |
|  [12]   | Stock member or phrase       | `search_api_docs`                  | Ranked hits across the bundled reference, `index=` widens one hit   |
|  [13]   | Extension member, enum items | `bpy_api_lookup`                   | Live RNA of every registered type, add-ons included                 |
|  [14]   | Node sockets per mode        | `describe_node_type`               | Socket index, identifier, type, default per `property_overrides`    |
|  [15]   | Concept or workflow          | `search_manual_docs`               | Bundled manual of stock Blender                                     |
|  [16]   | Capability by words          | `scripts/discover.py`              | Operators with owner, params, polls, reads, RNA types, settings     |
|  [17]   | Extension on the platform    | `blender -c extension list`        | Remote index with `[installed]` per package, every repository       |
|  [18]   | Geometry from a chosen view  | `scripts/capture.py`               | Framed on objects or the user's view, overlays off, frame reused    |
|  [19]   | Existing node tree           | `scripts/nodes.py`                 | Values that differ from a fresh node, links by socket identifier    |
|  [20]   | Editor, panel, node canvas   | `get_screenshot_of_area_as_image`  | One area from Blender's framebuffer whatever window is in front     |
|  [21]   | Whole window with chrome     | `screencapture -x -o -l <id>`      | Drawn window at 1:1 device pixels, top bar and status bar included  |
|  [22]   | Layout, mode, selection      | `get_screenshot_of_window_as_json` | Areas, shading, view, active object with mode                       |
|  [23]   | Library or generated asset   | `mcp-for-blender` asset tools      | One call downloads and imports an asset                             |
|  [24]   | Show the user an object      | `jump_to_view3d_object_by_name`    | Object Mode, deselects all, frames, unhides on `allow_edits`        |
|  [25]   | Summary of a closed file     | `get_blendfile_summary_*_for_cli`  | Background Blender on the named file                                |

- `search_api_docs` finds Python-defined members outside `bl_rna` (`Object.evaluated_geometry`)
- Precision work (architecture, CAD, BIM) builds geometry from dimensions in code and extension operators, library assets serve props and context

```python
# [EXECUTE_BLENDER_CODE] Operators, types, and settings matching words
from discover import discover
from results import as_result

result = as_result(discover("<word>", "<word>"))
```

- `poll` answers under calling context and `poll_in_view` under the largest 3D Viewport, `None` without one
- Raising polls answer false with the traceback on `stderr`
- `reads` names context chains a Python operator's source reads, `settings` names add-on runtime properties on ID types
- Settings hold `values` by ID name from each context member of their ID type

## [04]-[EXECUTION]

`execute_blender_code` runs each call in a fresh namespace on Blender's main thread from a timer, the UI waits until call returns:
- Snippets tagged `[EXECUTE_BLENDER_CODE]` run through `blender` `execute_blender_code`, `[MCP_FOR_BLENDER]` through `mcp-for-blender`
- `"<name>" in dir(bpy.ops.<category>)` is true for a registered operator alone
- `scripts/wrapper.py` closes every live call with one `Agent (<mode>)` undo step, background processes hold no undo history
- Consecutive calls in one mode share a step until the user acts, one Ctrl-Z reverts the step
- Edits before a raise stay inside the step
- Every call marks the file modified and deletes the user's redo steps
- Read-only calls after a user action add one empty step
- Edit Mode steps record data-API changes to other objects in that call through a hidden global step, one Ctrl-Z reverts both
- `window_manager.undo_stack.steps` lists each step's `name` and `is_substep`, `active` is the current step, background processes read `None`
- `bpy.context.area` and `bpy.context.region` are `None`, a view operator or extension code reading `context.area` runs under `temp_override`
- Overrides pass the `VIEW_3D` area with its `WINDOW` region, an `area` alone fails the poll
- Override keywords naming no context member do nothing
- Steps that wait on a redraw run as `bpy.app.timers.register(functools.partial(next, <generator>, None))`, one step per `yield 0.1`
- `persistent=True` keeps a timer across a file load
- Raises inside a timer end it with the traceback on stderr and no report
- `object.mode_set`, primitives, `view3d.localview`, and exporters' selected-only options act on `view_layer.objects.active` and `select_set`
- Error reports raise `RuntimeError`, warning and info reports print a `Warning:` or `Info:` line in `stdout`, a no-op returns `FINISHED`
- `blender` stops waiting at 300 s while Blender keeps working, longer work runs through `headless.py`
- `bpy` lengths are meters under the imperial display, `bpy.utils.units.to_value("IMPERIAL", "LENGTH", "10' 6\"")` converts a user's dimension
- Reports state ft, ft², and ft³ as meters over `0.3048` per dimension, lb as kg over `0.45359237`, and feet and inches by `divmod(m / 0.0254, 12)`
- Camera `lens` and `sensor_width` take the `CAMERA` unit, millimeters under every system

```python
# [EXECUTE_BLENDER_CODE] Context-reading operator on a named object through a selection override
import bpy

target = bpy.data.objects["<Object>"]
with bpy.context.temp_override(active_object=target, selected_objects=[target], selected_editable_objects=[target]):
    status = bpy.ops.object.shade_auto_smooth()
result = {"status": sorted(status)}
```

## [05]-[HEADLESS]

Every `headless.py` command runs as one Bash call, prints one JSON outcome with its case under `kind`, and exits 1 for every case but success:

```bash
# Code on stdin in a fresh factory process on <file>
python .claude/skills/use-blender/scripts/headless.py run <file> <<'PY'
import bpy
result = {"objects": sorted(bpy.data.objects.keys())}
PY

# Session <name> on <file>, answered once it listens
python .claude/skills/use-blender/scripts/headless.py start <file> <name>

# Code on stdin in session <name>, a traceback exits 1
python .claude/skills/use-blender/scripts/headless.py call <name> <<'PY'
import bpy
result = {"objects": sorted(bpy.data.objects.keys())}
PY

# Session's changes saved to its titled file (`saved`) once a running call returns, then a quit
python .claude/skills/use-blender/scripts/headless.py stop <name>
```

- `Ran` holds `result`, stdout, and stderr, `Raised` the traceback with `<agent>` lines, `Failed` the exit code and the log
- `run` and `start` open the file itself, and code that saves writes it
- `run` and `start` on a path with no file open the user's startup file saved under that path
- `bpy.ops.wm.revert_mainfile()` in a call returns a session to the file on disk
- `stop` saves a titled file when the session's data differ from a copy taken at its last save or after the call that last loaded it
- Factory starts hold stock add-ons and the wheels of installed extensions, and no extension operator
- Calls keep `bpy.data`, see the file's active object and selection, and wait as long as the code runs
- One session runs per name until `stop`, concurrent work takes distinct names
- Background processes fire no `bpy.app.timers` callback, code meant for a timer runs inside the call
- Background calls hold the file's first stored window and its screen in context, `bpy.data.window_managers[0].windows` holds every stored screen
- Stored 3D views read through `SpaceView3D.region_3d` in the background with the matrices of their last GUI draw, `Region.data` reads `None` there
- Background processes read `system.dpi` 72, `ui_scale` 0.0, and `pixel_size` 1 whatever the preferences say
- `stop` answers `Raised` and leaves the session running when the save raises, `Alive` when the process outlives its termination
- `stop` answers `Diverged` with the session running when both its data and the file on disk changed, a call saves elsewhere or reopens the file
- Sessions write preferences into a copy of the user's config folder under `bpy.app.tempdir`, and Blender deletes `bpy.app.tempdir` at quit
- Sessions reject `check_is_finished`
- `Lost` names a session process that ended during a `call` or `stop`, the session log names its crash report
- `start` answers `NoBridge` with the first package Blender failed to import when the user's extensions lack the Blender Lab MCP extension
- Popup operators crash background Blender
- Sun Position prints an `AttributeError` traceback on `stderr` when a load or `scene.new` leaves the scene without a world, and the call finishes
- `scripts_blocked` names the first driver or script Blender skipped in a file from outside the repository
- `bpy.ops.wm.open_mainfile(filepath=bpy.data.filepath, use_scripts=True)` in a call runs blocked drivers and scripts
- Simple and math driver expressions evaluate under `-Y` and factory starts, an expression reaching past the restricted names reads 0
- Snippets tagged `[HEADLESS_CALL]` run through `call`, or through `run` when they call no extension operator

## [06]-[NODES]

Node trees of every kind take their sockets by name and identifier:
- `digest("<tree>")` from `nodes.py` reads one tree by its node group name or the name of the ID holding it
- Mode properties take their value first, `inputs["<Name>"]` then returns the enabled socket of that name, an integer index counts disabled sockets
- Repeated socket names resolve by identifier (`Value_001`), `describe_node_type` prints identifiers per mode
- `links.new` on a type mismatch returns a link with `is_valid` false and raises nothing

## [07]-[CAPTURES]

Each change ends with one picture from a chosen view, `snapshot` names what changed and `capture` shows it:
- Counts and dimensions after modifiers and instances come from `obj.evaluated_get(depsgraph)` or `snapshot`
- `snapshot("<name>")` writes each object's evaluated transform, bounds, geometry hash, stacks, and animation
- Snapshots hold unit and color settings, materials, datablock counts, library files, missing paths, and node trees
- `comparison` lists objects added and removed since the earlier snapshot, and each changed value's before and after under its keys and list indexes
- Curve bound boxes, and the snapshot bounds and capture frames read from them, grow by the point radius on every side
- `capture("<name>", ("<Object>",), "<view>")` frames named objects or every visible one, `iso` in perspective and each axis view orthographic
- `capture("<name>", view="user")` draws the user's own view at its aspect with no frame, live or from the file's stored view
- `capture("<after>", since="<before>")` redraws the earlier frame, counts the pixels that differ, and writes `<after>-diff.png` with them in red
- `outside` true in a diff means geometry left the earlier frame, a capture without `since` frames it whole
- Live captures draw offscreen through the largest 3D Viewport's shading and local view with overlays and gizmos off
- Headless captures render Workbench under Solid mode's color management and leave every render setting as found
- Captures under a repeated `<name>` overwrite `.artifacts/blender/<name>.png`
- `Read` shows the PNG whole, 1280x720 or the user's view fit inside it

```python
# [EXECUTE_BLENDER_CODE] Snapshot and front capture of the changed object against earlier ones
from capture import capture
from results import as_result
from snapshot import snapshot

result = {"changes": as_result(snapshot("<after>", since="<before>")), "front": as_result(capture("<after>-front", ("<Object>",), "front", since="<before>-front"))}
```
