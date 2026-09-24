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
- [05]-[RENDERING](references/rendering.md): Engines, render settings, the render command, compositor, and output media
- [06]-[LOOK_DEVELOPMENT](references/look-development.md): Materials and color spaces, worlds, lights, downloaded assets, and asset libraries
- [07]-[ANIMATION](references/animation.md): Layered actions, slots, and keyframes
- [08]-[DRAFTING](references/drafting.md): Sheet scale, Line Art, sections, dimensions, and SVG and PDF sheets
- [09]-[GEOMETRY_NODES](references/geometry-nodes.md): Node groups from code, modifier inputs, and evaluated results
- [10]-[INTERCHANGE](references/interchange.md): CAD and city model import and authoring, unit and axis checks, Rhino materials, and batch conversion
- [11]-[BIM](references/bim.md): IFC authoring, Bonsai load behavior, and IFC drawings and sheets
- [12]-[GEOSPATIAL](references/geospatial.md): Georeferencing, GIS imports, sun position, and climate studies

[SCRIPTS]:
- [01]-[HEADLESS](scripts/headless.py): `run`, `start`, `call`, `stop`, and `render` for every process outside the live session, one JSON outcome each
- [02]-[WRAPPER](scripts/wrapper.py): Hook on both `execute_blender_code` tools, undo steps live and operator return sets on every host
- [03]-[DISCOVER](scripts/discover.py): Operators, RNA types, and add-on settings matching words, each hit naming the tool for the next question
- [04]-[CAPTURE](scripts/capture.py): Framed view of named or visible objects to `.artifacts/blender/<name>.png` with a diff against an earlier one
- [05]-[SNAPSHOT](scripts/snapshot.py): Evaluated scene state to `.artifacts/blender/<name>.json` with the changes since an earlier snapshot
- [06]-[NODES](scripts/nodes.py): One node tree as interface, nodes with non-default values, and links by socket identifier
- [07]-[DRAWING](scripts/drawing.py): Line Art strokes through an orthographic camera as an SVG and PDF sheet at scale
- [08]-[CONVERT](scripts/convert.py): Interchange files through Blender's importers and one exporter, one empty scene per file

`headless.py` and the hook run under `uv run --script`, every other script runs inside Blender on any host through `runpy.run_path("<skill>/scripts/<name>.py")`, `<skill>` this skill's directory, and returns an `attrs` case record that `as_result` turns into the `result` dict with its name under `kind`.

## [01]-[CONDUCT]

Both servers answer the GUI the user watches, and a task leaves it as found:
- Reads and writes go through the API with Blender behind the frontmost application, and nothing activates it or posts a pointer or key event to it
- Probes that set a value (a sentinel color, a trial setting) restore it in the same call, before the call returns
- Pictures come from an offscreen draw or a window capture by id, neither needs Blender in front
- Every session and GUI a task opens ends with the task, `stop` for a session and one quit call for a GUI
- Preference and add-on writes in the GUI persist at quit, a trial sets `preferences.use_preferences_save = False` first

## [02]-[FILES]

Work runs live and on named files, a task starts by saving the file the live session holds and names every file it creates:

```bash
# Every running Blender, the GUI both servers answer is the line without --background
pgrep -lf Blender.app/Contents/MacOS/Blender
```

```python
# [EXECUTE_BLENDER_CODE] Live file saved before a headless process reads it
import bpy

if bpy.data.filepath:
    bpy.ops.wm.save_mainfile()
result = {"file": bpy.data.filepath}
```

- No GUI line means the task opens one with `open -n -g -a Blender <file> --args --no-window-focus`, without the flag Blender activates itself
- Server calls after a launch fail until both add-ons listen
- New work goes into a file the agent names with `bpy.ops.wm.save_as_mainfile(filepath="<dir>/<name>.blend")`
- Long jobs, files the GUI does not hold, and batches run through `headless.py` on the saved file, every other job runs live
- Quits run through `mcp-for-blender`, the `blender` sandbox refuses `wm.quit_blender`, `sys.exit`, and `wm.read_userpref` and its factory forms
- `wm.quit_blender()` from code quits with no prompt, and the server answers before Blender exits
- `caffeinate -t 30 -w <pid>` returns once Blender exits or 30 seconds pass, `kill -KILL <pid>` ends a Blender that `ps -p <pid>` still lists

```python
# [EXECUTE_BLENDER_CODE] Quit, saving a titled file with unsaved changes and discarding an untitled one
import bpy

status = bpy.ops.wm.save_mainfile(exit=True) if bpy.data.filepath and bpy.data.is_dirty else bpy.ops.wm.quit_blender()
result = {"status": sorted(status)}
```

## [03]-[ROUTING]

Each job has one tool across both servers and the scripts:

| [INDEX] | [JOB]                         | [TOOL]                             | [DECIDING_FACT]                                                     |
| :-----: | :---------------------------- | :--------------------------------- | :------------------------------------------------------------------ |
|  [01]   | Scene inventory               | `get_objects_summary`              | Collection tree with parent, selection, and visibility per object   |
|  [02]   | One object's stack            | `get_object_detail_summary`        | Modifiers, constraints, slots, collections, rotation read as Euler  |
|  [03]   | Change the user's scene       | `execute_blender_code`             | `result` dict, stdout, stderr, and traceback per call               |
|  [04]   | Long job or build on a file   | `headless.py start`, `call`        | State kept between calls, every extension, no client timeout        |
|  [05]   | Closed or downloaded `.blend` | `headless.py run <file>`           | Factory start, embedded scripts off, `snapshot` and `capture` whole |
|  [06]   | Stock export or CAD output    | `headless.py run`                  | Stock exporters and the CAD modules in a half-second factory start  |
|  [07]   | Still, frame range, video     | `headless.py render`               | User's Cycles device, every frame proven, resumed after a stop      |
|  [08]   | Batch of interchange files    | `scripts/convert.py` in a session  | Importer per suffix, options per operator, one outcome per file     |
|  [09]   | GLB or FBX of the live scene  | `export_scene`                     | One call over both exporters, written where asked                   |
|  [10]   | Stock or Python-defined API   | `get_python_api_docs`              | Bundled reference with `examples`, `X.*` lists a namespace          |
|  [11]   | Extension member, enum items  | `bpy_api_lookup`                   | Live RNA of every registered type, add-ons included                 |
|  [12]   | Node sockets per mode         | `describe_node_type`               | Socket index, identifier, type, default per `property_overrides`    |
|  [13]   | Concept or workflow           | `search_manual_docs`               | Bundled manual of stock Blender                                     |
|  [14]   | Capability by task words      | `scripts/discover.py`              | Operators with owner, params, polls, reads, RNA types, settings     |
|  [15]   | Extension on the platform     | `blender -c extension list`        | `[installed]` per repository, `--factory-startup` loads none        |
|  [16]   | Geometry from a chosen view   | `scripts/capture.py`               | Framed on objects or the user's view, overlays off, frame reused    |
|  [17]   | Existing node tree            | `scripts/nodes.py`                 | Values that differ from a fresh node, links by socket identifier    |
|  [18]   | Editor, panel, node canvas    | `screencapture -x -o -l <id>`      | Drawn window at 1:1 device pixels, no activation, chrome included   |
|  [19]   | Layout, mode, selection       | `get_screenshot_of_window_as_json` | Areas, shading, view, active object with mode                       |
|  [20]   | Library or generated asset    | `mcp-for-blender` asset tools      | Real-world size and packed images in one call per asset             |
|  [21]   | Show the user an object       | `jump_to_view3d_object_by_name`    | Object Mode, deselects all, frames, unhides on `allow_edits`        |

- `bpy_api_lookup` reads `bl_rna`, a Python-defined member (`Object.evaluated_geometry`) answers `no property or function`, the bundled docs hold it
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
- `result` holds the post-condition read of the same call, a call without an error proves nothing
- `scripts/wrapper.py` closes every live call with one `Agent (<mode>)` undo step
- Operator calls that do not finish print `bpy.ops.<op> returned [...]` in `stdout` on every host
- Consecutive calls in one mode share a step until the user acts, one Ctrl-Z then reverts them together
- Edits before a raise stay inside the step, and every call marks the file modified and deletes the user's redo steps
- Read-only calls after a user action add one empty step
- Edit Mode steps hold the edited mesh alone, a data-API change to another object in that call survives Ctrl-Z
- `bpy.context.area` and `bpy.context.region` are `None`, a view operator or extension code reading `context.area` runs under `temp_override`
- Overrides pass the `VIEW_3D` area with its `WINDOW` region, `area` alone fails the poll, an unknown keyword changes nothing
- Steps that wait on a redraw inside one call run as a `bpy.app.timers` generator yielding 0.1 per pass
- `persistent=True` keeps a timer across a file load, and a raise inside it ends the timer with its traceback on stderr and no report
- Context-reading operators act on named objects through `temp_override(active_object=, selected_objects=, selected_editable_objects=)`
- Operators reading view-layer flags (`view3d.localview`, primitives) ignore that override and act on objects `select_set` selects
- Error reports raise `RuntimeError`, warning and info reports print a `Warning:` or `Info:` line in `stdout`, a no-op returns `FINISHED`
- `blender` stops waiting at 300 s while Blender keeps working, longer work runs in the headless session
- `bpy` lengths are meters under the imperial display, `bpy.utils.units.to_value("IMPERIAL", "LENGTH", "10' 6\"")` converts a user's dimension
- `bpy.utils.units.to_string("IMPERIAL", "LENGTH", <meters>, precision=3, split_unit=True)` states a length back in the scene's units
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

# Titled file saved when the session changed data, then a clean quit that removes Blender's temporary folder, a session busy past the deadline is killed
uv run --script <skill>/scripts/headless.py stop <name>
```

- `Ran` holds `result`, stdout, and stderr, `Raised` the traceback with `<agent>` lines, `Failed` the exit code and Blender's error lines
- `run` and `start` open the file itself, and code that saves writes it
- Factory starts hold stock add-ons and the extension wheel folder (`OCP`, `ezdxf`, `rhino3dm`, `ifcopenshell`) and no extension operator
- Calls keep `bpy.data`, see the file's active object and selection, and wait as long as the code runs
- One session runs per name until `stop`, parallel agents each name their own, and a render inside a call runs on the user's GPU
- Background processes fire no `bpy.app.timers` callback, code a timer would defer runs inside the call
- Background processes leave `bpy.context.window` at `None`, and `bpy.data.window_managers[0].windows` holds the file's stored screens for an override
- Background processes read `system.dpi` 72, `ui_scale` 0.0, and `pixel_size` 1 whatever the preferences say
- `stop` answers `Raised` and leaves the session running when the save raises
- `stop` answers `Diverged` with the session running when both its data and the file on disk changed, a call saves elsewhere or reopens the file
- Sessions and renders run on a copy of the user's config folder, and preference and add-on writes stay in the copy
- Sessions reject `check_is_finished`, and `Lost` names a process that ended during a call
- Popup operators crash background Blender, `errors` of a `Lost` call names the crash report
- MeasureIt_ARCH raises at import in a `--background` session (`measureit_arch_geometry.py:103`), dimensions bake through `dimensions`
- `scripts_blocked` names the first driver or script Blender skipped in a file from outside the repository
- `bpy.ops.wm.open_mainfile(filepath=bpy.data.filepath, use_scripts=True)` in a call runs them
- Simple and math driver expressions evaluate under `-Y` and factory starts, an expression reaching past the restricted names reads 0
- Snippets tagged `[HEADLESS_CALL]` run through `call`, or through `run` when they call no extension operator
- `blender -c <command>` implies `--background`, a windowed run takes `open -n -g -a Blender --args --no-window-focus --python-expr "<code>"`

## [06]-[API]

Blender 5.2 spells each concern one way, a form outside the table comes from `bpy_api_lookup` before it runs:

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
- `snapshot("<name>", since="<earlier>")` writes transforms, evaluated bounds with instances, geometry counts, and a shape hash per object
- Snapshots hold materials, modifiers, animation, color management, datablock counts, library files, missing paths, and one digest per node tree
- `comparison` lists objects added and removed since the earlier snapshot, changed fields with before and after, and trees whose digest changed
- Curve bound boxes, and the snapshot bounds and capture frames read from them, grow by the point radius on every side
- `digest("<tree>")` from `nodes.py` reads one tree by its material, node group, world, or scene name before code edits it
- `capture("<name>", ("<Object>",), "<view>")` frames named objects or every visible one, `iso` in perspective and each axis view orthographic
- `capture("<name>", view="user")` draws the user's own view at its aspect
- `capture("<after>", since="<before>")` redraws the earlier frame, counts the pixels that differ, and writes `<after>-diff.png` with them in red
- Live sessions draw offscreen through the largest 3D Viewport's shading with overlays and gizmos off, inside its local view
- Headless sessions and one-shot runs render Workbench through a temporary camera under the `Standard` view at exposure 0, as Solid mode draws
- `<name>` is a short label reused through a task, a re-capture overwrites the file, `Read` shows the 1280x720 PNG whole

```python
# [EXECUTE_BLENDER_CODE] Snapshot against the earlier one, then a capture of the changed object against its earlier picture, <skill> is this skill's directory
import runpy

snapshot = runpy.run_path("<skill>/scripts/snapshot.py")
capture = runpy.run_path("<skill>/scripts/capture.py")
result = {"changes": snapshot["as_result"](snapshot["snapshot"]("<after>", since="<before>")), "front": capture["as_result"](capture["capture"]("<after>-front", ("<Object>",), "front", since="<before>-front"))}
```

## [08]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                                           | [CORRECT_FORM]                                             |
| :-----: | :--------------------------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `FINISHED` as proof of a change                                        | Post-condition read in `result`                            |
|  [02]   | `bpy.ops.ed.undo_push` in agent code                                   | Step the hook pushes                                       |
|  [03]   | `select_all` and `select_set` before a context-reading operator        | `temp_override` of the selection members                   |
|  [04]   | `hasattr(bpy.ops.<category>, "<name>")`, true for any name             | `"<name>" in dir(bpy.ops.<category>)`                      |
|  [05]   | Enum identifier from memory or `bl_rna` of a dynamic enum              | Current value, or the `TypeError` of an unknown identifier |
|  [06]   | `obj.data` counts or `dimensions` as the result of modifiers           | `evaluated_get(depsgraph)`, or `snapshot`                  |
|  [07]   | `scene.frame_current = <frame>` before an animated read                | `scene.frame_set(<frame>)`, which evaluates the frame      |
|  [08]   | Member spelled from memory of an older release                         | `bpy_api_lookup`, then the bundled docs                    |
|  [09]   | Bundled docs for an extension operator                                 | `scripts/discover.py`, then `bpy_api_lookup`               |
|  [10]   | Render or bake inside `execute_blender_code`                           | `headless.py render`, or a session call                    |
|  [11]   | `execute_blender_code_for_cli` or a `get_blendfile_summary_*_for_cli`  | `headless.py run`, or a session call                       |
|  [12]   | Hand-built `Blender --background` command                              | `headless.py run`, `call`, or `render`                     |
|  [13]   | Downloaded `.blend` opened in the GUI or through a summary tool        | `headless.py run <file>`, embedded scripts off             |
|  [14]   | `--factory-startup` beside `--addons bl_ext.<id>` or `-c extension -e` | User's preferences, or `BLENDER_USER_RESOURCES=<dir>`      |
|  [15]   | PyPI `bpy` module for a background run                                 | `headless.py run`                                          |
|  [16]   | `screen.screenshot` or a server screenshot tool behind another window  | `screencapture -x -o -l <id>`, or `capture.py`             |
|  [17]   | `nodes["Principled BSDF"]` or another localized node name              | `next(n for n in nodes if n.type == "BSDF_PRINCIPLED")`    |
