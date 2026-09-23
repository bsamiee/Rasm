---
name: use-blender
description: "Use when a task drives a Blender session, a headless Blender, or a .blend file through its MCP servers and scripts, covering files, routing, execution, headless work, API forms, and verification."
hooks:
  PreToolUse:
    - matcher: "^mcp__(blender|mcp-for-blender)__execute_blender_code$"
      hooks:
        - type: command
          command: "uv run --script \"${CLAUDE_PROJECT_DIR}\"/.claude/skills/use-blender/scripts/wrapper.py"
          timeout: 10
---

# [BLENDER]

`.mcp.json` runs `blender` (Blender Lab, port 9877) and `mcp-for-blender` (port 9876) against the user's Blender GUI, and `scripts/headless.py` runs every Blender process outside it. Routing rows decide the tool for a job over any tool the servers' connect instructions name.

[REFERENCES]:
- [01]-[SETUP](references/setup.md): Servers, connection checks, session facts a task inherits, and extensions
- [02]-[RENDERING](references/rendering.md): Engines, render settings, the render command, compositor, and output media
- [03]-[LOOK_DEVELOPMENT](references/look-development.md): Materials, worlds, lights, downloaded assets and their traps, and asset libraries
- [04]-[ANIMATION](references/animation.md): Layered actions, slots, and keyframes
- [05]-[DRAFTING](references/drafting.md): Sheet scale, Line Art, sections, dimensions, and SVG and PDF sheets
- [06]-[GEOMETRY_NODES](references/geometry-nodes.md): Node groups from code, modifier inputs, and evaluated results
- [07]-[INTERCHANGE](references/interchange.md): CAD and city model import and authoring with unit and axis checks, and batch conversion
- [08]-[BIM](references/bim.md): IFC authoring, Bonsai sync, and IFC drawings and sheets
- [09]-[GEOSPATIAL](references/geospatial.md): Georeferencing, GIS imports, sun position, and climate studies

[SCRIPTS]:
- [01]-[HEADLESS](scripts/headless.py): `run`, `start`, `call`, `stop`, and `render` for every process outside the live session, one JSON outcome each
- [02]-[WRAPPER](scripts/wrapper.py): Hook and wrapper agent code runs in on every host, undo steps live and operator return sets everywhere
- [03]-[DISCOVER](scripts/discover.py): Operators, RNA types, and add-on settings matching words, each hit naming the tool for the next question
- [04]-[CAPTURE](scripts/capture.py): Framed view of named or visible objects to `.artifacts/blender/<name>.png` with a diff against an earlier one
- [05]-[SNAPSHOT](scripts/snapshot.py): Evaluated scene state to `.artifacts/blender/<name>.json` with the changes since an earlier snapshot
- [06]-[NODES](scripts/nodes.py): One node tree as interface, nodes with non-default values, and links by socket identifier
- [07]-[DRAWING](scripts/drawing.py): Line Art strokes through an orthographic camera as an SVG and PDF sheet at scale
- [08]-[CONVERT](scripts/convert.py): Interchange files through Blender's importers and one exporter, one empty scene per file

`headless.py` and the hook run under `uv run --script`, every other script runs inside Blender on any host through `runpy.run_path("<root>/.claude/skills/use-blender/scripts/<name>.py")`, `<root>` the repository root, and returns a case dataclass that `as_result` turns into the `result` dict with its name under `kind`.

## [01]-[FILES]

Work runs live and on named files, a task starts by saving the file the live session holds and names every file it creates:

```bash
# Every running Blender, the one both servers answer reports its file
pgrep -lf Blender.app/Contents/MacOS/Blender
```

```python
# [EXECUTE_BLENDER_CODE] Live file saved before a headless process reads it
import bpy

if bpy.data.filepath:
    bpy.ops.wm.save_mainfile()
result = {"file": bpy.data.filepath}
```

- No running Blender means the task opens one with `open -a Blender <file>`, and the first server call answers once both add-ons listen
- New work goes into a file the agent names with `bpy.ops.wm.save_as_mainfile(filepath="<dir>/<name>.blend")`
- Long jobs, files the GUI does not hold, and batches run through `headless.py` on the saved file, every other job runs live

## [02]-[ROUTING]

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
|  [08]   | Batch of interchange files    | `scripts/convert.py` in a session  | Importer per suffix from its file filter, one outcome per file      |
|  [09]   | GLB or FBX of the live scene  | `export_scene`                     | One call over both exporters, written where asked                   |
|  [10]   | Stock or Python-defined API   | `get_python_api_docs`              | Bundled reference with `examples`, `X.*` lists a namespace          |
|  [11]   | Extension member, enum items  | `bpy_api_lookup`                   | Live RNA of every registered type, add-ons included                 |
|  [12]   | Node sockets per mode         | `describe_node_type`               | Socket index, identifier, type, default per `property_overrides`    |
|  [13]   | Concept or workflow           | `search_manual_docs`               | Bundled manual of stock Blender                                     |
|  [14]   | Capability by task words      | `scripts/discover.py`              | Operators with owner, params, polls, reads, RNA types, settings     |
|  [15]   | Extension on the platform     | `blender -c extension list`        | `[installed]` per repository, `--factory-startup` loads none        |
|  [16]   | Geometry from a chosen view   | `scripts/capture.py`               | Framed on objects or the user's view, overlays off, frame reused    |
|  [17]   | Existing node tree            | `scripts/nodes.py`                 | Values that differ from a fresh node, links by socket identifier    |
|  [18]   | Editor, panel, node canvas    | `get_screenshot_of_area_as_image`  | Largest area of a `ui_type` on the active workspace, with chrome    |
|  [19]   | Layout, mode, selection       | `get_screenshot_of_window_as_json` | Areas, shading, view, active object with mode                       |
|  [20]   | Library or generated asset    | `mcp-for-blender` asset tools      | Real-world size and packed images in one call per asset             |
|  [21]   | Show the user an object       | `jump_to_view3d_object_by_name`    | Object Mode, deselects all, frames, unhides on `allow_edits`        |

- `bpy_api_lookup` reads `bl_rna`, a Python-defined member (`Object.evaluated_geometry`) answers `no property or function`, the bundled docs hold it
- Screenshots read the window's front buffer, `osascript -e 'tell application "Blender" to activate'` first draws the current frame
- Precision work (architecture, CAD, BIM) builds geometry from dimensions in code and extension operators, library assets serve props and context

```python
# [EXECUTE_BLENDER_CODE] Operators, types, and settings matching the words, <root> is the repository root
import runpy

discover = runpy.run_path("<root>/.claude/skills/use-blender/scripts/discover.py")
result = discover["as_result"](discover["discover"]("<word>", "<word>"))
```

- `poll` answers under the timer context and `poll_in_view` under the largest 3D Viewport, a raising poll answers false with its traceback on `stderr`
- `reads` and `settings` name the context chains and add-on property groups with live values that a parameterless operator reads

## [03]-[EXECUTION]

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
- Context-reading operators act on named objects through `temp_override(active_object=, selected_objects=, selected_editable_objects=)`
- Operators reading view-layer flags (`view3d.localview`, primitives) ignore that override and act on objects `select_set` selects
- Error reports raise `RuntimeError`, warning and info reports print a `Warning:` or `Info:` line in `stdout`, a no-op returns `FINISHED`
- `blender` stops waiting at 300 s while Blender keeps working, longer work runs in the headless session
- `bpy` lengths are meters under the imperial display, `bpy.utils.units.to_value("IMPERIAL", "LENGTH", "10' 6\"")` converts a user's dimension
- `bpy.utils.units.to_string("IMPERIAL", "LENGTH", <meters>, precision=3, split_unit=True)` states a length back in the scene's units

```python
# [EXECUTE_BLENDER_CODE] Selection-dependent operator on a named object, return set and post-condition in one result
import bpy

target = bpy.data.objects["<Object>"]
with bpy.context.temp_override(active_object=target, selected_objects=[target], selected_editable_objects=[target]):
    status = bpy.ops.object.shade_auto_smooth()
result = {"status": sorted(status), "modifiers": [(m.name, m.type) for m in target.modifiers]}
```

## [04]-[HEADLESS]

Every `headless.py` command is one Bash call that prints one JSON outcome with its case under `kind` and exits 1 for every case but success:

```bash
# Code on stdin in a fresh factory process on <file>
uv run --script <root>/.claude/skills/use-blender/scripts/headless.py run <file> <<'PY'
import bpy
result = {"objects": sorted(bpy.data.objects.keys())}
PY

# Session on <file>, answered once it listens
uv run --script <root>/.claude/skills/use-blender/scripts/headless.py start <file>

# Code on stdin in the session, a traceback exits 1
uv run --script <root>/.claude/skills/use-blender/scripts/headless.py call <<'PY'
import bpy
result = {"objects": sorted(bpy.data.objects.keys())}
PY

# Current frame, one frame, a range, or `all` of <file>
uv run --script <root>/.claude/skills/use-blender/scripts/headless.py render <file> --frames <start>..<end>

# Server stop and a clean quit that removes Blender's temporary folder, a session busy past the deadline is killed
uv run --script <root>/.claude/skills/use-blender/scripts/headless.py stop
```

- `Ran` holds `result`, stdout, and stderr, `Raised` the traceback with `<agent>` lines, `Failed` the exit code and Blender's error lines
- `run` and `start` open the file itself, and code that saves writes it
- Factory starts hold stock add-ons and the extension wheel folder (`OCP`, `ezdxf`, `rhino3dm`, `ifcopenshell`) and no extension operator
- Calls keep `bpy.data`, see the file's window, active object, and selection, and wait as long as the code runs
- One session runs per repository until `stop`, and a render inside a call runs on the user's GPU
- Sessions and renders run on a copy of the user's config folder, and preference and add-on writes stay in the copy
- Sessions reject `check_is_finished`, and `Lost` names a process that ended during a call
- Popup operators crash background Blender, `errors` of a `Lost` call names the crash report
- `scripts_blocked` names the first driver or script Blender skipped in a file from outside the repository
- `bpy.ops.wm.open_mainfile(filepath=bpy.data.filepath, use_scripts=True)` in a call runs them
- Simple and math driver expressions evaluate under `-Y` and factory starts, an expression reaching past the restricted names reads 0
- `Rendered` holds every frame on disk with pixels varying beyond one 8-bit level, a video counted by its frames
- `resumed` counts image frames an earlier run of the unchanged file wrote, a stopped or failed run included, a changed file renders every frame again
- `Incomplete` lists missing and blank frames of a run that exited 0 and deletes the blank ones for the next run
- Snippets tagged `[HEADLESS_CALL]` run through `call`, or through `run` when they call no extension operator
- Enabling an extension subset (`--addons bl_ext.<id>`, `addon_utils.enable`, factory `-c extension install -e`) rewrites shared packages to it
- Imports fail after a subset run until a run under the user's preferences restores the full package set
- Hand-built commands exit 0 on a raising script unless `--python-exit-code 1` precedes `--python`, and a `-f` range on a video writes one frame

## [05]-[API]

Blender 5.2 spells each concern one way, a form outside the table comes from `bpy_api_lookup` before it runs:

| [INDEX] | [CONCERN]                  | [FORM]                                                                                       |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | Engine identifiers         | `BLENDER_EEVEE`, `CYCLES`, `BLENDER_WORKBENCH`                                               |
|  [02]   | F-curves of an animated ID | `bpy_extras.anim_utils.action_get_channelbag_for_slot(action, slot).fcurves`                 |
|  [03]   | Action on a second ID      | `animation_data.action_slot = action.slots[<id>]`, an unset slot leaves the ID still         |
|  [04]   | Geometry Nodes input       | `modifier.properties.inputs.<identifier>.value`, identifier from `tree.interface.items_tree` |
|  [05]   | Compositor                 | `scene.compositing_node_group`, a `CompositorNodeTree` ending in `NodeGroupOutput`           |
|  [06]   | Video output               | `image_settings.media_type = "VIDEO"` before `file_format` and `render.ffmpeg`               |
|  [07]   | Smooth shading by angle    | `bpy.ops.object.shade_auto_smooth(angle=<radians>)`, a `Smooth by Angle` modifier            |
|  [08]   | Grease Pencil              | `bpy.data.grease_pencils`, `GREASEPENCIL` objects, line art is the `LINEART` modifier        |
|  [09]   | Principled BSDF inputs     | `Subsurface Weight`, `Specular IOR Level`, `Transmission Weight`, `Coat Weight`              |
|  [10]   | Sequencer strips           | `scene.sequence_editor.strips`                                                               |

- `view_settings.view_transform` and `look` list `NONE` in `bl_rna` like `render.engine`, an unknown identifier's `TypeError` lists every value
- Mode properties take their value first, `inputs["<Name>"]` then returns the enabled socket of that name, an integer index counts disabled sockets
- Repeated socket names resolve by identifier (`Value_001`), `describe_node_type` prints identifiers per mode
- `links.new` on a type mismatch returns a link with `is_valid` false and raises nothing, builds end with `all(link.is_valid for link in tree.links)`

## [06]-[VERIFICATION]

Each change ends with a data read in `result` and one picture from a chosen view, `snapshot` names what changed and `capture` shows it:
- `snapshot("<name>", since="<earlier>")` writes transforms, evaluated bounds with instances, geometry counts, and a shape hash per object
- Snapshots hold materials, modifiers, animation, datablock counts, linked library files, missing file paths, and one digest per node tree
- `comparison` lists objects added and removed since the earlier snapshot, changed fields with before and after, and trees whose digest changed
- Curve bound boxes, and the snapshot bounds and capture frames read from them, grow by the point radius on every side
- `digest("<tree>")` from `nodes.py` reads one tree by its material, node group, world, or scene name before code edits it
- `capture("<name>", ("<Object>",), "<view>")` frames named objects or every visible one, `iso` in perspective and each axis view orthographic
- `capture("<name>", view="user")` draws the user's own view at its aspect
- `capture("<after>", since="<before>")` redraws the earlier frame, counts the pixels that differ, and writes `<after>-diff.png` with them in red
- Live sessions draw offscreen through the largest 3D Viewport's shading with overlays and gizmos off, inside its local view
- Headless sessions and one-shot runs render Workbench through a temporary camera
- `<name>` is a short label reused through a task, a re-capture overwrites the file, `Read` shows the 1280x720 PNG whole

```python
# [EXECUTE_BLENDER_CODE] Snapshot against the earlier one, then a capture of the changed object against its earlier picture, <root> is the repository root
import runpy

snapshot = runpy.run_path("<root>/.claude/skills/use-blender/scripts/snapshot.py")
capture = runpy.run_path("<root>/.claude/skills/use-blender/scripts/capture.py")
result = {"changes": snapshot["as_result"](snapshot["snapshot"]("<after>", since="<before>")), "front": capture["as_result"](capture["capture"]("<after>-front", ("<Object>",), "front", since="<before>-front"))}
```

## [07]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                                               | [CORRECT_FORM]                                             |
| :-----: | :------------------------------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `FINISHED` as proof of a change                                            | Post-condition read in `result`                            |
|  [02]   | `bpy.ops.ed.undo_push` in agent code                                       | Step the hook pushes                                       |
|  [03]   | `select_all` and `select_set` before a context-reading operator            | `temp_override` of the selection members                   |
|  [04]   | `hasattr(bpy.ops.<category>, "<name>")`, true for any name                 | `"<name>" in dir(bpy.ops.<category>)`                      |
|  [05]   | Enum identifier from memory or `bl_rna` of a dynamic enum                  | Current value, or the `TypeError` of an unknown identifier |
|  [06]   | Feet typed as `bpy` lengths                                                | `bpy.utils.units.to_value` of the dimension string         |
|  [07]   | `obj.data` counts or `dimensions` as the result of modifiers               | `evaluated_get(depsgraph)`, or `snapshot`                  |
|  [08]   | `scene.frame_current = <frame>` before an animated read                    | `scene.frame_set(<frame>)`, which evaluates the frame      |
|  [09]   | Member spelled from memory of an older release                             | `bpy_api_lookup`, then the bundled docs                    |
|  [10]   | Bundled docs for an extension operator                                     | `scripts/discover.py`, then `bpy_api_lookup`               |
|  [11]   | Render or bake inside `execute_blender_code`                               | `headless.py render`, or a session call                    |
|  [12]   | `execute_blender_code_for_cli` or a `get_blendfile_summary_*_for_cli`      | `headless.py run`, or a session call                       |
|  [13]   | Hand-built `Blender --background` command                                  | `headless.py run`, `call`, or `render`                     |
|  [14]   | Downloaded `.blend` opened in the GUI or through a summary tool            | `headless.py run <file>`, embedded scripts off             |
|  [15]   | `--factory-startup` beside `--addons bl_ext.<id>` or `-c extension ... -e` | User's preferences, or `BLENDER_USER_RESOURCES=<dir>`      |
|  [16]   | PyPI `bpy` module for a background run                                     | `headless.py run`                                          |
