---
name: use-blender
description: "Use when a task drives a Blender session, a headless Blender, or a .blend file through its MCP servers and scripts, covering conduct, files, routing, execution, headless work, API forms, verification, configuration, interface, and extensions."
---

# [BLENDER]

`.mcp.json` runs `blender` (Blender Lab) and `mcp-for-blender` against the user's Blender GUI, and `scripts/headless.py` runs every Blender process outside it. Routing rows decide the tool for a job over any tool the servers' connect instructions name.

[REFERENCES]:
- [01]-[SETUP](references/setup.md): Servers, machine prerequisites, startup state, processes, and failures
- [02]-[CONFIGURATION](references/configuration.md): Preference stores, read-back, add-on records, repositories, keymaps, themes, fonts, and units
- [03]-[INTERFACE](references/interface.md): Event-loop timing, workspaces, areas, regions, panels, views, and pictures of the window
- [04]-[EXTENSIONS](references/extensions.md): Package layout, manifests, installs, wheels, and registration
- [05]-[MODELING](references/modeling.md): Meshes from dimensions, modifier stacks, cutters, Edit Mode, joins, collections, and parenting
- [06]-[RENDERING](references/rendering.md): Engines, render settings, the render command, compositor, and output media
- [07]-[LOOK_DEVELOPMENT](references/look-development.md): Materials and color spaces, worlds, lights, downloaded assets, and asset libraries
- [08]-[ANIMATION](references/animation.md): Layered actions, slots, and keyframes
- [09]-[DRAFTING](references/drafting.md): Sheet scale, Line Art, sections, dimensions, sketches, and SVG and PDF sheets
- [10]-[GEOMETRY_NODES](references/geometry-nodes.md): Node groups from code, modifier inputs, evaluated results, and Sverchok trees
- [11]-[INTERCHANGE](references/interchange.md): CAD and city model import and authoring, unit and axis checks, Rhino materials, and batch conversion
- [12]-[BIM](references/bim.md): IFC authoring, Bonsai load behavior, and IFC drawings and sheets
- [13]-[GEOSPATIAL](references/geospatial.md): Georeferencing, GIS imports, sun position, and climate studies

[SCRIPTS]:
- [01]-[HEADLESS](scripts/headless.py): `run`, `start`, `call`, `stop`, and `render` for every process outside the live session, one JSON outcome each
- [02]-[WRAPPER](scripts/wrapper.py): Hook on `execute_blender_code` and headless calls, crash refusals, undo steps live, and operator return sets
- [03]-[DISCOVER](scripts/discover.py): Operators, RNA types, and add-on settings matching words, each hit naming the tool for the next question
- [04]-[CAPTURE](scripts/capture.py): Framed view of named or visible objects to `.artifacts/blender/<name>.png` with a diff against an earlier one
- [05]-[SNAPSHOT](scripts/snapshot.py): Evaluated scene state to `.artifacts/blender/<name>.json` with the changes since an earlier snapshot
- [06]-[NODES](scripts/nodes.py): One node tree as interface, nodes with non-default values, and links by socket identifier
- [07]-[DRAWING](scripts/drawing.py): Line Art strokes through an orthographic camera as an SVG and PDF sheet at scale
- [08]-[CONVERT](scripts/convert.py): Interchange files through Blender's importers and one exporter, one empty scene per file

`headless.py` and the hook run under `uv run --script`, every other script runs inside Blender on any host through `runpy.run_path("<skill>/scripts/<name>.py")`, `<skill>` this skill's directory, and returns an `attrs` case record that `as_result` turns into the `result` dict with its name under `kind`.

## [01]-[CONDUCT]

Both servers answer the GUI the user watches, and a task leaves it as found:
- Reads and writes go through the API while Blender stays behind the frontmost application
- Reads through the servers' summary and screenshot tools leave undo history and the modified flag as found
- Calls that set a trial value (a sentinel color, a setting) restore it before they return
- Pictures come from an offscreen draw, an area's framebuffer, or a window capture by id, each with Blender behind other windows
- Every session and GUI a task opens ends with the task, `stop` for a session and one quit call for a GUI
- Preference and add-on writes in the GUI persist at quit, a trial sets `preferences.use_preferences_save = False` first

## [02]-[FILES]

Work runs live and on named files, a task starts by saving the file the live session holds and names every file it creates:

```bash
# Every running Blender, the GUI both servers answer is the line without --background
pgrep -lf Blender.app/Contents/MacOS/Blender

# GUI on <file> behind the frontmost application, with the login session's environment in place of the shell's
env -i /usr/bin/open -n -g -a Blender <file> --args --no-window-focus
```

```python
# [EXECUTE_BLENDER_CODE] Live file saved before a headless process reads it
import bpy

if bpy.data.filepath:
    bpy.ops.wm.save_mainfile()
result = {"file": bpy.data.filepath}
```

- `pgrep` lines without `--background` name the running GUI the task uses, the second command launches one when every line holds `--background`
- Server calls answer once both add-ons listen on their ports
- New work the task keeps goes into a file the agent names with `bpy.ops.wm.save_as_mainfile(filepath="<dir>/<name>.blend")`
- Temporary work stays in an untitled file that closes unsaved, and disk keeps only the files the task produces
- Long jobs, files the GUI does not hold, and batches run through `headless.py` on the saved file, every other job runs live
- Quits and preference reloads run through `mcp-for-blender`, the `blender` sandbox blocks `wm.quit_blender`, `sys.exit`, and `wm.read_userpref` forms
- `mcp-for-blender` `execute_blender_code` answers with printed output and drops `result`, a raise answers with the traceback alone
- `wm.quit_blender()` from code quits with no prompt, and the server answers before Blender exits
- Exit reads from `ps -p <pid>` in a later call, `kill -KILL <pid>` ends a Blender it still lists after the quit

```python
# [MCP_FOR_BLENDER] Quit, saving a titled file with unsaved changes and discarding an untitled one
import bpy

status = bpy.ops.wm.save_mainfile(exit=True) if bpy.data.filepath and bpy.data.is_dirty else bpy.ops.wm.quit_blender()
print(sorted(status))
```

## [03]-[ROUTING]

Each job has one tool across both servers and the scripts:

| [INDEX] | [JOB]                         | [TOOL]                             | [DECIDING_FACT]                                                     |
| :-----: | :---------------------------- | :--------------------------------- | :------------------------------------------------------------------ |
|  [01]   | Scene inventory               | `get_objects_summary`              | Collection tree with parent, selection, and visibility per object   |
|  [02]   | One object's stack            | `get_object_detail_summary`        | Modifiers, constraints, slots, collections, rotation read as Euler  |
|  [03]   | Live file inventory           | `get_blendfile_summary_datablocks` | Datablock counts, siblings for missing files, libraries, and paths  |
|  [04]   | Change the user's scene       | `blender` `execute_blender_code`   | `result` dict, stdout, stderr, and traceback per call               |
|  [05]   | Long job or build on a file   | `headless.py start`, `call`        | State kept between calls, every extension, no client timeout        |
|  [06]   | Closed or downloaded `.blend` | `headless.py run <file>`           | Factory start, embedded scripts off, `snapshot` and `capture` whole |
|  [07]   | Stock export or CAD output    | `headless.py run`                  | Stock exporters and the CAD modules in a half-second factory start  |
|  [08]   | Still, frame range, video     | `headless.py render`               | User's Cycles device, frames resumed after a stop                   |
|  [09]   | Batch of interchange files    | `scripts/convert.py`               | Session of its own, importer per suffix, one outcome per file       |
|  [10]   | GLB or FBX of the live scene  | `export_scene`                     | Both exporters in one call, selects and activates what it exports   |
|  [11]   | Stock class or module page    | `get_python_api_docs`              | Bundled reference with `examples`, `X.*` lists a namespace          |
|  [12]   | Stock member or phrase        | `search_api_docs`                  | Ranked hits across the bundled reference, `index=` widens one hit   |
|  [13]   | Extension member, enum items  | `bpy_api_lookup`                   | Live RNA of every registered type, add-ons included                 |
|  [14]   | Node sockets per mode         | `describe_node_type`               | Socket index, identifier, type, default per `property_overrides`    |
|  [15]   | Concept or workflow           | `search_manual_docs`               | Bundled manual of stock Blender                                     |
|  [16]   | Capability by task words      | `scripts/discover.py`              | Operators with owner, params, polls, reads, RNA types, settings     |
|  [17]   | Extension on the platform     | `blender -c extension list`        | `[installed]` per repository, `--factory-startup` loads none        |
|  [18]   | Geometry from a chosen view   | `scripts/capture.py`               | Framed on objects or the user's view, overlays off, frame reused    |
|  [19]   | Existing node tree            | `scripts/nodes.py`                 | Values that differ from a fresh node, links by socket identifier    |
|  [20]   | Editor, panel, node canvas    | `get_screenshot_of_area_as_image`  | One area from Blender's framebuffer whatever window is in front     |
|  [21]   | Whole window with chrome      | `screencapture -x -o -l <id>`      | Drawn window at 1:1 device pixels, top bar and status bar included  |
|  [22]   | Layout, mode, selection       | `get_screenshot_of_window_as_json` | Areas, shading, view, active object with mode                       |
|  [23]   | Library or generated asset    | `mcp-for-blender` asset tools      | Real-world size and packed images in one call per asset             |
|  [24]   | Show the user an object       | `jump_to_view3d_object_by_name`    | Object Mode, deselects all, frames, unhides on `allow_edits`        |

- `bpy_api_lookup` reads `bl_rna` of registered types, `search_api_docs` finds Python-defined members (`Object.evaluated_geometry`)
- Precision work (architecture, CAD, BIM) builds geometry from dimensions in code and extension operators, library assets serve props and context

```python
# [EXECUTE_BLENDER_CODE] Operators, types, and settings matching the words, <skill> is this skill's directory
import runpy

discover = runpy.run_path("<skill>/scripts/discover.py")
result = discover["as_result"](discover["discover"]("<word>", "<word>"))
```

- `poll` answers under the timer context and `poll_in_view` under the largest 3D Viewport, a raising poll answers false with its traceback on `stderr`
- `reads` and `settings` name the context chains and add-on property groups with live values that a parameterless operator reads

## [04]-[EXECUTION]

`execute_blender_code` runs each call in a fresh namespace on Blender's main thread from a timer, the UI waits until the call returns:
- `result` holds the post-condition read of the same call, the proof of the call
- Snippets tagged `[EXECUTE_BLENDER_CODE]` run through `blender` `execute_blender_code`, `[MCP_FOR_BLENDER]` through `mcp-for-blender`
- `"<name>" in dir(bpy.ops.<category>)` is true for a registered operator alone
- `scripts/wrapper.py` closes every live call with one `Agent (<mode>)` undo step
- Operator calls that do not finish print `bpy.ops.<op> returned [...]` in `stdout` on every host
- Consecutive calls in one mode share a step until the user acts, one Ctrl-Z then reverts them together
- Edits before a raise stay inside the step, and every call marks the file modified and deletes the user's redo steps
- Read-only calls after a user action add one empty step
- Edit Mode steps hold the edited mesh alone, a data-API change to another object in that call survives Ctrl-Z
- `bpy.context.area` and `bpy.context.region` are `None`, a view operator or extension code reading `context.area` runs under `temp_override`
- Overrides pass the `VIEW_3D` area with its `WINDOW` region, an `area` alone fails the poll, and a keyword naming no context member does nothing
- Steps that wait on a redraw inside one call run as a `bpy.app.timers` generator yielding 0.1 per pass
- `persistent=True` keeps a timer across a file load, and a raise inside it ends the timer with its traceback on stderr and no report
- Context-reading operators act on named objects through `temp_override(active_object=, selected_objects=, selected_editable_objects=)`
- `object.mode_set`, primitives, `view3d.localview`, and exporters' selected-only options act on `view_layer.objects.active` and `select_set`
- Error reports raise `RuntimeError`, warning and info reports print a `Warning:` or `Info:` line in `stdout`, a no-op returns `FINISHED`
- `blender` stops waiting at 300 s while Blender keeps working, longer work runs in the headless session
- `bpy` lengths are meters under the imperial display, `bpy.utils.units.to_value("IMPERIAL", "LENGTH", "10' 6\"")` converts a user's dimension
- Reports state ft, ft², and ft³ as meters over `0.3048` per dimension, lb as kg over `0.45359237`, and feet and inches by `divmod(m / 0.0254, 12)`
- Camera `lens` and `sensor_width` take the `CAMERA` unit, millimeters under every system
- Use `references/interface.md` for the user's view, screen operators, workspace switches, area and region edits, and window pictures
- Use `references/configuration.md` for preference, add-on, keymap, theme, and unit writes and their read-back

```python
# [EXECUTE_BLENDER_CODE] Selection-dependent operator on a named object, return set and post-condition in one result
import bpy

target = bpy.data.objects["<Object>"]
with bpy.context.temp_override(active_object=target, selected_objects=[target], selected_editable_objects=[target]):
    status = bpy.ops.object.shade_auto_smooth()
result = {"status": sorted(status), "modifiers": [(m.name, m.type) for m in target.modifiers]}
```

## [05]-[HEADLESS]

Every `headless.py` command is one Bash call that prints one JSON outcome with its case under `kind` and exits 1 for every case but success:

```bash
# Code on stdin in a fresh factory process on <file>
uv run --script <skill>/scripts/headless.py run <file> <<'PY'
import bpy
result = {"objects": sorted(bpy.data.objects.keys())}
PY

# Session <name> on <file>, answered once it listens
uv run --script <skill>/scripts/headless.py start <file> <name>

# Code on stdin in session <name>, a traceback exits 1
uv run --script <skill>/scripts/headless.py call <name> <<'PY'
import bpy
result = {"objects": sorted(bpy.data.objects.keys())}
PY

# Current frame, one frame, a range, or `all` of <file>
uv run --script <skill>/scripts/headless.py render <file> --frames <start>..<end>

# Titled file saved when the session changed data (`saved`), then a quit, a busy session ended unsaved at the deadline with its call answering `Lost`
uv run --script <skill>/scripts/headless.py stop <name>
```

- `Ran` holds `result`, stdout, and stderr, `Raised` the traceback with `<agent>` lines, `Failed` the exit code and Blender's error lines
- `Refused` names each `<agent>` line that crashes Blender with the form that runs, and Blender receives no code
- `run` and `start` open the file itself, and code that saves writes it
- `run` and `start` on a path with no file open the user's startup file saved under that path
- `bpy.ops.wm.revert_mainfile()` in a call returns a session to the file on disk, and `stop` then saves nothing
- Factory starts hold stock add-ons and the wheels of installed extensions, and no extension operator
- Calls keep `bpy.data`, see the file's active object and selection, and wait as long as the code runs
- One session runs per name until `stop`, parallel agents each name their own, and a render inside a call runs on the user's GPU
- Background processes fire no `bpy.app.timers` callback, code a timer would defer runs inside the call
- Background processes leave `bpy.context.window` at `None`, and `bpy.data.window_managers[0].windows` holds the file's stored screens for an override
- Stored 3D views read through `SpaceView3D.region_3d` in the background with the matrices of their last GUI draw, `Region.data` reads `None` there
- Background processes read `system.dpi` 72, `ui_scale` 0.0, and `pixel_size` 1 whatever the preferences say
- `stop` answers `Raised` and leaves the session running when the save raises
- `stop` answers `Diverged` with the session running when both its data and the file on disk changed, a call saves elsewhere or reopens the file
- Sessions and renders run on a copy of the user's config folder, and preference and add-on writes stay in the copy `stop` and `render` remove
- Sessions reject `check_is_finished`, and `Lost` names a process that ended during a call, `stop` answering `Stopped` beside it
- Popup operators crash background Blender, `errors` of a `Lost` call names the crash report
- MeasureIt_ARCH raises at import in a `--background` session, headless dimensions come from the Dimensions add-on
- Sun Position prints an `AttributeError` traceback on `stderr` when a load or `scene.new` leaves the scene without a world, and the call finishes
- `scripts_blocked` names the first driver or script Blender skipped in a file from outside the repository
- `bpy.ops.wm.open_mainfile(filepath=bpy.data.filepath, use_scripts=True)` in a call runs them
- Simple and math driver expressions evaluate under `-Y` and factory starts, an expression reaching past the restricted names reads 0
- Snippets tagged `[HEADLESS_CALL]` run through `call`, or through `run` when they call no extension operator
- `blender -c <command>` implies `--background`, a windowed run adds `--python-expr "<code>"` after `--args` in the GUI launch

## [06]-[API]

Blender spells each concern one way, a form outside the table comes from `bpy_api_lookup` before it runs:

| [INDEX] | [CONCERN]                  | [FORM]                                                                                            |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | Engine identifiers         | `BLENDER_EEVEE`, `CYCLES`, `BLENDER_WORKBENCH`                                                    |
|  [02]   | F-curves of an animated ID | `bpy_extras.anim_utils.action_get_channelbag_for_slot(action, slot).fcurves`                      |
|  [03]   | Action on a second ID      | `animation_data.action_slot = action.slots[<id>]`, an unset slot leaves the ID still              |
|  [04]   | Geometry Nodes input       | `modifier.properties.inputs.<identifier>.value`, identifier from `tree.interface.items_tree`      |
|  [05]   | Compositor                 | `scene.compositing_node_group`, a `CompositorNodeTree` ending in `NodeGroupOutput`                |
|  [06]   | Video output               | `image_settings.media_type = "VIDEO"` before `file_format` and `render.ffmpeg`                    |
|  [07]   | Smooth shading by angle    | `bpy.ops.object.shade_auto_smooth(angle=<radians>)`, a `Smooth by Angle` modifier                 |
|  [08]   | Grease Pencil              | `bpy.data.grease_pencils`, `GREASEPENCIL` objects, line art is the `LINEART` modifier             |
|  [09]   | Principled BSDF inputs     | `Subsurface Weight`, `Specular IOR Level`, `Transmission Weight`, `Coat Weight`, `Emission Color` |
|  [10]   | Sequencer strips           | `scene.sequence_editor.strips`                                                                    |
|  [11]   | Sky Texture model          | `MULTIPLE_SCATTERING`, `SINGLE_SCATTERING`                                                        |
|  [12]   | Material transparency      | `material.surface_render_method` (`DITHERED`, `BLENDED`)                                          |
|  [13]   | User resource folders      | `bpy.utils.user_resource("CONFIG" \| "SCRIPTS" \| "EXTENSIONS" \| "DATAFILES")`                   |
|  [14]   | Add-on preference group    | `preferences.addons[<module>].preferences`, `None` for an add-on that failed to register          |

- `display_settings.display_device` writes before `view_transform`, a display lacking the current view resets it to `Standard` with no error
- Mode properties take their value first, `inputs["<Name>"]` then returns the enabled socket of that name, an integer index counts disabled sockets
- Repeated socket names resolve by identifier (`Value_001`), `describe_node_type` prints identifiers per mode
- `links.new` on a type mismatch returns a link with `is_valid` false and raises nothing, builds end with `all(link.is_valid for link in tree.links)`

## [07]-[VERIFICATION]

Each change ends with a data read in `result` and one picture from a chosen view, `snapshot` names what changed and `capture` shows it:
- Counts and dimensions after modifiers and instances come from `obj.evaluated_get(depsgraph)` or `snapshot`
- `snapshot("<name>", since="<earlier>")` writes transforms, evaluated bounds with instances, geometry counts, and a shape hash per object
- Snapshots hold materials, modifier settings, animation, color management, datablock counts, library files, missing paths, and tree digests
- `comparison` lists objects added and removed since the earlier snapshot, changed fields with before and after, and trees whose digest changed
- Curve bound boxes, and the snapshot bounds and capture frames read from them, grow by the point radius on every side
- `digest("<tree>")` from `nodes.py` reads one tree by its material, node group, world, or scene name before code edits it
- `capture("<name>", ("<Object>",), "<view>")` frames named objects or every visible one, `iso` in perspective and each axis view orthographic
- `capture("<name>", view="user")` draws the user's own view at its aspect with no frame, live or from the file's stored view
- `capture("<after>", since="<before>")` redraws the earlier frame, counts the pixels that differ, and writes `<after>-diff.png` with them in red
- `outside` true in a diff means geometry left the earlier frame, a capture without `since` frames it whole
- Live sessions draw offscreen through the largest 3D Viewport's shading with overlays and gizmos off, inside its local view
- Headless sessions and one-shot runs render Workbench through a temporary camera under the `Standard` view at exposure 0, as Solid mode draws
- `<name>` is a short label reused through a task, a re-capture overwrites the file, `Read` shows the 1280x720 PNG whole

```python
# [EXECUTE_BLENDER_CODE] Snapshot and front capture of the changed object against earlier ones, <skill> is this skill's directory
import runpy

snapshot = runpy.run_path("<skill>/scripts/snapshot.py")
capture = runpy.run_path("<skill>/scripts/capture.py")
result = {"changes": snapshot["as_result"](snapshot["snapshot"]("<after>", since="<before>")), "front": capture["as_result"](capture["capture"]("<after>-front", ("<Object>",), "front", since="<before>-front"))}
```
