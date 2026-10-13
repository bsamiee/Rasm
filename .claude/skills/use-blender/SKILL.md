---
name: use-blender
description: "Use when driving live or background Blender, its extensions, or bpy member reads, covering sessions, background runs, site, drafting, BIM, parametric, and CAD exchange."
---

# [USE_BLENDER]

Covers MCP-driven GUI sessions, background runs, extension installs, logs, and evaluation runs of Blender 5.4 at `BLENDER_PATH`, with workflows installed extensions serve. Use `blender-coding` for code a session or run executes.

[REFERENCES]:
- [01]-[GIS_SITE](references/gis-site.md): Georeferenced scenes, terrain, imagery, OSM, point clouds, and sun placement
- [02]-[DRAFTING](references/drafting.md): Sketches, profiles, sheet cameras, linework, dimensions, and drawing output
- [03]-[BIM](references/bim.md): IFC projects, elements, properties, queries, drawings, and exchange through Bonsai
- [04]-[PARAMETRIC](references/parametric.md): Sverchok trees, formula nodes, tree code, VI-Suite analyses, topology optimization, and pin solves
- [05]-[INTERCHANGE](references/interchange.md): Rhino, STEP, DXF, 3ds Max, mesh, and SVG imports, Rhino exports, and attribute tables

## [01]-[TOOL_INDEX]

Server `blender` reaches the GUI through extension `bl_ext.blender_lab.mcp` on port 9877, and server `mcp-for-blender` through add-on `blender_mcp` on port 9876. Table lists every tool a session calls:

| [INDEX] | [TOOL]                                                    | [USE_WHEN]                                                    |
| :-----: | :-------------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `mcp__blender__get_blendfile_summary_path_info`           | "Which file is open?" / "Does it hold unsaved changes?"       |
|  [02]   | `mcp__blender__get_blendfile_summary_datablocks`          | "How many data-blocks of each type does the file hold?"       |
|  [03]   | `mcp__blender__get_blendfile_summary_missing_files`       | "Which external files fail to resolve?"                       |
|  [04]   | `mcp__blender__get_blendfile_summary_of_linked_libraries` | "Which libraries does the file link?"                         |
|  [05]   | `mcp__blender__get_objects_summary`                       | "What does the collection tree hold?"                         |
|  [06]   | `mcp__blender__get_object_detail_summary`                 | "What modifiers, constraints, and materials does it hold?"    |
|  [07]   | `mcp__blender__get_screenshot_of_window_as_json`          | "Which areas, mode, and selection does the main window show?" |
|  [08]   | `mcp__blender__get_screenshot_of_window_as_image`         | "Is a popup, prompt, or splash screen open?"                  |
|  [09]   | `mcp__blender__get_screenshot_of_area_as_image`           | "What does this one editor show?"                             |
|  [10]   | `mcp__mcp-for-blender__look`                              | "What do these objects look like now?"                        |
|  [11]   | `mcp__blender__jump_to_tab_by_name`                       | "Show the user this workspace"                                |
|  [12]   | `mcp__blender__jump_to_tab_by_space_type`                 | "Show the user this editor"                                   |
|  [13]   | `mcp__blender__jump_to_view3d_object_by_name`             | "Frame this object in the viewport"                           |
|  [14]   | `mcp__blender__search_manual_docs`                        | "How does this feature or workflow work?"                     |
|  [15]   | `mcp__blender__execute_blender_code`                      | "Read or edit live state no other tool covers"                |
|  [16]   | `mcp__mcp-for-blender__get_addon_status`                  | "Which asset and model sources are on?"                       |
|  [17]   | `mcp__mcp-for-blender__search_assets`                     | "Find an HDRI, texture, or model in an online library"        |
|  [18]   | `mcp__mcp-for-blender__import_asset`                      | "Bring a found asset into the scene"                          |
|  [19]   | `mcp__mcp-for-blender__generate_3d`                       | "Generate a model from a prompt or image"                     |

Pictures come from the source that draws current state:
- `look` draws offscreen at call time with `target` naming objects, else startup file's 1828.8 m `Ground` fills the frame
- `look` `mode` `frames` frames target at its current-frame position, and `view` `camera` on a camera covering the path reads motion
- Window and area images show the last drawn frame, for popups and layout
- State reads come from data or `look`
- Node trees read as data (`nodes`, `links`, `interface.items_tree`)
- `jump_to_view3d_object_by_name` sets Object Mode, replaces selection and active object, and leaves camera view
- Writing `region_3d.view_location` and `view_distance` on a 3D view frames an object with selection and mode kept

## [02]-[SESSION]

- Inspect UI, logs and crash reports; resolve dialogs and errors, discard untitled and scratch work, save named files in place, decline submission

Sessions open in order:
1. `lsof -a -nP -iTCP:9876-9877 -sTCP:LISTEN` names the GUI pid
2. With no listener, `/usr/bin/open -n -g -a "${BLENDER_PATH%/Contents/MacOS/Blender}" --env PYTHONUNBUFFERED=1 --stdout <log> --stderr <log> --args --no-window-focus --python-use-system-env` starts a task GUI
3. `rg -m1 -q 'BlenderMCP server started' < <(tail -F -n +1 <log> 2>/dev/null)` with `run_in_background: true` completes once port 9876 listens
4. One `ToolSearch` select line loads every `mcp__blender__*` and `mcp__mcp-for-blender__*` schema
5. `mcp__blender__get_blendfile_summary_path_info` names the open file and its `is_dirty` state
6. `mcp__mcp-for-blender__get_addon_status` answers `"source": "native"`, or `"error"` while replacing a quit GUI's socket, `native` on its next call

- Files outside the task close through `bpy.ops.wm.read_homefile()`, after `bpy.ops.wm.save_mainfile()` for a named file, untitled files with no save

Quitting saves preferences, the last GUI to quit overwriting what others saved, and preferences a task GUI changed take their prior values first. One `mcp__blender__execute_blender_code` call quits a task GUI with no save prompt by reloading the startup file before closing its last window:

```python
import bpy

bpy.ops.wm.read_homefile()
with bpy.context.temp_override(window=bpy.context.window_manager.windows[0]):
    closed = sorted(bpy.ops.wm.window_close())
result = {"closed": closed}
```

Script-driven task GUIs run a runner script:
- `python .claude/skills/use-blender/scripts/evaluate.py run <name> -- --no-window-focus --python <runner.py>` starts one through `open` and returns once it quits
- Runners register a persistent `bpy.app.timers` function that runs one step per call, modal jobs and drawing advancing between steps
- Runners end with the quit call

Configured startup files open with:
- Data in meters under an imperial feet display
- Unselectable `Ground` plane
- Working directory `<repo>/.artifacts/blender/`
- AgX exposure of -5.3, under which `render.opengl` and Workbench render near black and Cycles and EEVEE render daylight

## [03]-[CALLS]

`mcp__blender__execute_blender_code` runs Python inside the GUI process:
- Calls on either server run one at a time on the GUI main thread
- With each call's namespace holding `result = {}` alone, code imports `bpy` and every module it uses
- `result` returns a dict of names, numbers, and lists, IDs and `mathutils` values returning as their `repr`
- Each call sets `window` and `screen` and leaves `area` and `region` None
- Operators reading an area, space, node, or material run under a `temp_override` naming them
- Calls push no undo step and leave `is_dirty` as it was
- Answers hold prints and every operator report line in `stdout` and an `ERROR` report's traceback in `message`
- Python warnings reach an answer through `warnings.catch_warnings(record=True)` with `simplefilter("always")`
- File paths are absolute, relative paths resolving under the working directory
- Operators run in default exec form
- Operators returning `RUNNING_MODAL` (invoked tools, timer-driven jobs) run on after the call, and later calls read progress from their data
- Extension operators answer FINISHED whatever their body did, and keep data made before a raise

## [04]-[MODAL_STATE]

Window reads name what Blender shows before a call that depends on it:
- `mcp__blender__get_screenshot_of_window_as_json` describes the main window's areas, mode, and selection, popups excluded
- `mcp__blender__get_screenshot_of_window_as_image` shows popups, prompts, and splash screen over the main window
- `bpy.context.window_manager.windows` lists each window's areas and `modal_operators`

| [INDEX] | [STATE]         | [READS_AS]                                        | [RESOLUTION]                                       |
| :-----: | :-------------- | :------------------------------------------------ | :------------------------------------------------- |
|  [01]   | File browser    | Window with one `FILE_BROWSER` area, opener modal | `file.cancel()` under its window, area, and region |
|  [02]   | Running tool    | `modal_operators` non-empty                       | Complete or cancel through UI                      |
|  [03]   | Prompt or popup | Window image                                      | Read dialog and select action                      |
|  [04]   | Running job     | `window_manager.is_interface_locked` true         | Data writes after it reads false                   |

## [05]-[BACKGROUND_RUNS]

Arguments apply left to right, `--python-exit-code` and the `.blend` before `--python`, and `-o`, `-F`, `-E` before `-f`:

```bash
"$BLENDER_PATH" --background <file.blend> -o <absolute path> -E <engine> --python-exit-code 1 --python <absolute script.py> -- <args>
```

Background runs follow host rules:
- Runs using installed extensions launch `"$BLENDER_PATH"` on the user's profile, every other run passes arguments to `evaluate.py run <name> --`
- Tracebacks exit 0 without `--python-exit-code`, and `sys.exit(<n>)` exits `<n>`
- Background runs hold no event loop, and `bpy.app.timers` and msgbus callbacks never run
- Modal operators return `RUNNING_MODAL` without stepping, and invoke-only operators return `PASS_THROUGH`
- Background runs hold no undo stack until `ed.undo_push()`, and Python writes leave `is_dirty` False
- Handlers (render and frame handlers included) and update callbacks run on the main thread and accept data writes
- `load_post` reaches an extension at startup when preferences enable it, or when a `.blend` argument follows `--addons`
- GPU drawing (`GPUOffScreen`, `batch_for_shader`, `gpu.texture.from_image`, `draw_view3d`) runs after `gpu.init()`
- Brush strokes and `render.opengl` need a GUI session, and sculpt operators crash a background run in their undo push
- `wm.read_homefile(use_empty=True)` empties the scene and keeps loaded extensions

Background renders use named files saved in place; untitled and scratch scenes render in GUI without saving:
1. `bpy.ops.wm.save_mainfile()` in the GUI saves named work before rendering
2. `"$BLENDER_PATH" --background <file.blend> -o <absolute dir>/<stem>_#### -f <frame>` with `run_in_background: true` logs `Saved: '<path>'`

## [06]-[MEMBERS]

Member, signature, enum, and operator property facts come from the installed build, each printed by `python .claude/skills/use-blender/scripts/evaluate.py run members -- --background --python-expr "import bpy; print(<expr>)"`, extension members by `"$BLENDER_PATH"` with the same arguments:
- `<Type>.bl_rna.properties["<name>"]` reads `type`, `fixed_type`, `is_readonly`, `enum_items`, and `array_dimensions`
- `<Type>.bl_rna.functions["<name>"].parameters` lists a method's parameters, `bpy.ops.<module>.<op>.get_rna_type().properties` an operator's
- `<function>.__doc__` prints the signature of a Python-level function (`bpy.app.timers.register`, `bpy.utils.*`)

Searches find code using a member:
- `rg <member> "${BLENDER_PATH%/MacOS/Blender}/Resources/5.4/scripts"` finds stock code
- `rg` over `~/Library/Application Support/Blender/5.4/extensions` and `scripts/addons` finds extension code

## [07]-[EXTENSIONS]

Use `manage-repo` for extension projects and their `pack` and `install` targets.

Repository `user_default` holds every installed extension:
- `nx run <id>:install` installs and enables an extension inside a running GUI session, else through `--command extension install-file`
- Running GUI sessions are those of the executable `BLENDER_PATH` resolves, by file identity, under the installing process's `BLENDER_USER_*` profile
- `"$BLENDER_PATH" --command extension remove user_default.<id>` uninstalls with no GUI running
- `install-file` exits 0 on install and on a platform or version refusal, and 1 on a failed `--enable`
- `install-file` output's `STATUS Installed "<id>"` or `ERROR <id>: <reason>` line states the result
- `install` and `install-file` save preferences an enable or asset library changed, and `remove`, `repo-add`, and `repo-remove` always save
- `--no-prefs` skips preference saves for a command that changes extension files and leaves the enabled set as found
- Calls reach extension modules through `importlib.import_module("bl_ext.<repository>.<id>.<module>")`
- Calls reach legacy add-ons (`sverchok`, `vi_suite`, `BlenderGIS`) by package name

## [08]-[LOGS]

Each source writes its log to one place:

| [INDEX] | [SOURCE]          | [OUTPUT]                                                                           |
| :-----: | :---------------- | :--------------------------------------------------------------------------------- |
|  [01]   | Background run    | Calling shell                                                                      |
|  [02]   | `pack`, `install` | Nx task output, native install output forwarded from the GUI                       |
|  [03]   | Task GUI          | `--stdout` log with prints, add-on requests, and extension registration tracebacks |
|  [04]   | GUI, no log file  | `execute_blender_code` answers, Info editor lines in `window_manager.reports`      |
|  [05]   | Crash, Blender    | `blender.crash.txt` in `filepaths.temporary_directory`, else process `TMPDIR`      |
|  [06]   | Crash, macOS      | `~/Library/Logs/DiagnosticReports/Blender-<time>.ips`                              |

Log reads follow host rules:
- Blender Python reads `PYTHONUNBUFFERED` under `--python-use-system-env` alone, and prints reach the log per line
- `--log "<categories>"` selects log categories, and `--log-level trace` prints every level
- `--log-level trace --log "context"` prints each context member an operator read, tagged with the calling script line
- Crash reports read before the next run starts, concurrent Blenders sharing one file
- `Cannot connect to Blender` answers mean no GUI listens on the answering server's port

## [09]-[EVALUATION]

Blender resolves a `BLENDER_USER_*` variable naming a missing folder through `BLENDER_USER_RESOURCES` to the user's profile. `scripts/evaluate.py` runs from the repository root, each command launching Blender with factory settings in a fresh `.artifacts/blender/evaluate/<name>/` tree, every `BLENDER_USER_*` folder and `TMPDIR` created before launch:
- `python .claude/skills/use-blender/scripts/evaluate.py run <name> -- <arguments>` runs Blender with the arguments and prints its output
- `python .claude/skills/use-blender/scripts/evaluate.py checks <file.py>` runs each `@check` the file declares
- `checks --isolate` runs one process per check, and `checks --falsify` runs each check again with its `flip` input
- `run` and `checks` install every package `nx run <id>:pack` bundles, with all of `rasm`, into tree folder `scripts/modules` before launch
- Blender lists `scripts/modules` first on `sys.path` and keeps its packages through factory resets
- `python .claude/skills/use-blender/scripts/evaluate.py extension <zip>` validates and installs an archive
- `extension` then cycles disable and enable twice and unregisters the extension
- Checks files call `check` with no import, and each check runs from factory settings:

```python
import bpy


@check("<id>", expect=<value or exception class>, flip=<input>)
def <name>(flip=<default>):
    return <value read from bpy>
```

Results read in order:
1. Stdout summary line `<name>: <n> passed, <n> failed, <n> crashed, <rows path>`, exit code 1 on any failure or crash
2. `jq -c 'select(.verdict == false or .kind == "crash")' .artifacts/blender/evaluate/<name>/rows.jsonl` lists failed and crashed rows
3. Crash report a crash row's `report` names, then `blender.log` in the run folder
