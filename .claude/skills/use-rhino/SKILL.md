---
name: use-rhino
description: "Use when a task drives a Rhino document, a .3dm file, or a Grasshopper 2 canvas, covering slots, orientation, layers, selection, commands, files, views, materials, and definitions."
hooks:
  PreToolUse:
    - matcher: "mcp__rhino-mcp-platform__run_python"
      hooks:
        - type: command
          command: uv
          args: ["run", "--script", "${CLAUDE_PROJECT_DIR}/.claude/skills/use-rhino/scripts/hook.py"]
---

# [RHINO_MCP]

`rhino-mcp-platform` routes each call to one Rhino document, a slot. One Rhino process holds every slot on the machine, the user's documents and other sessions' slots included, and one Grasshopper 2 canvas every slot shares.

[REFERENCES]:
- [01]-[SETUP](references/setup.md): Machine, router, listener, Rhino in front, dialogs, hung or crashed process, and Grasshopper 2 start
- [02]-[FILES](references/files.md): Reading `.3dm` files on disk, opening, exporting, converting, importing, and headless documents
- [03]-[GRASSHOPPER](references/grasshopper.md): Building, solving, checking, baking, saving, and picturing a Grasshopper 2 definition
- [04]-[PRESENTATION](references/presentation.md): Materials, sun and ground, display styles, named views, named positions, and layer states
- [05]-[DRAFTING](references/drafting.md): Make2D, layout sheets with scaled details, dimensions, and PDF sheets
- [06]-[PLUGINS](references/plugins.md): Yak packages, and loading and proving a compiled plugin or library from the repository

[SCRIPTS]:
- [01]-[HOOK](scripts/hook.py): Frontmatter hook over `run_python`, nothing calls it by hand
- [02]-[RECORDS](scripts/records.py): `Record`, `Fault`, `File`, `Properties`, and the layer and material records every script shares
- [03]-[DOCUMENT](scripts/document.py): Entry points over the open documents and one document, each writes and reads back into a record
- [04]-[CANVAS](scripts/canvas.py): Grasshopper 2 values, assignments, bakes, script components, groups, files, plugins, and canvas images
- [05]-[ASSEMBLY](scripts/assembly.py): `load(path)` loads a `.rhp` or library and proves the held build is the file on disk
- [06]-[FILE3DM](scripts/file3dm.py): `uv run --script` over `.3dm` files or folders prints each file's record, no Rhino involved

Scripts import the modules by name inside `run_python`, results print as records whose class names the case. Rejected inputs return a tuple of `Fault(source, value, accepted)`, `source` the Rhino type that refused `value` and `accepted` its alternatives:

```python
# Add tower massing on Site::Buildings
from Rhino.Geometry import Box, Interval, Plane
import document
from records import Properties
doc = __rhino_doc__
massing = Box(Plane.WorldXY, Interval(0, 10), Interval(0, 10), Interval(0, 30)).ToBrep()
print(document.layer(doc, "Site::Buildings", Properties(color="#C83C3C")))
print(document.add(doc, massing, "Site::Buildings", name="<name>"))
print(document.capture(doc, "<name>"))
```

## [01]-[START]

Every task opens with these steps before any change:
1. `list_slots` with no row means no Rhino runs, `spawn_slot {"version": "9"}` launches one, a Rhino the user started lists every document
2. `documents()` in any row's slot lists every open document by `serial`, `path` (`None` untitled), `modified`, `active`, and listener `port`
3. Files the task names that no document holds read first with `uv run --script <skill>/scripts/file3dm.py <file>...`
4. `Fault` lines name files Rhino answers with a modal alert that holds every close in the process, and stay unopened
5. `open -g -b com.mcneel.rhinoceros.9 <file>...` opens the rest behind the user's application, each as its own slot, an open file adds no document
6. Work in the document whose file or contents the task names, `spawn_slot {"version": "9"}` when none fits
7. Titled documents the task does not need take `save(RhinoDoc.FromRuntimeSerialNumber(<serial>))` when `modified`, in a call held in front
8. `close(...)` closes them on the next call held in front and makes another document active
9. Untitled documents stay open as their owner left them
10. Record the working document's `pid` and `port`, the pair names it for the task, rows with another pair belong to other sessions
11. `describe(doc)` fields decide the next step:

| [INDEX] | [FIELD]                   | [DECIDES]                                                                             |
| :-----: | :------------------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `units`, `tolerance`      | Every number in a script or macro is in model units                                   |
|  [02]   | `active`                  | `command`, `save`, and `close` make an inactive document active through its window    |
|  [03]   | `prompt`                  | Command waiting for input, its release in the setup reference precedes any other call |
|  [04]   | `layers`, `current_layer` | Hierarchy new objects join, an add without attributes lands on layer index 0          |
|  [05]   | `selected`                | Objects the user means by "this" or "these"                                           |
|  [06]   | `current_view`, `views`   | View the user looks at, names and cameras `capture`, `show`, and `save_view` take     |
|  [07]   | `materials`               | Physically based materials a `Properties(material=)` names for a layer or object      |
|  [08]   | `path`, `modified`        | File `save(doc)` writes, `None` takes a path, `True` marks unsaved edits              |

Every call passes `slot`, a call without one lands in the slot this session used last, else in the user's oldest document:
- Slot names and `adopted` change while a document stays open, a spawned or opened document can return as `adopted: true` under another name
- `list_slots` before each step that writes, the row with the recorded `pid` and `port` gives that step's `slot`
- `slot_not_found` on the recorded name reads `list_slots` again, no row with the pair means the document is gone
- `documents()` rows with `port` `None` answer no slot, a call in another slot reaches them by `serial`

## [02]-[ROUTING]

| [INDEX] | [JOB]                        | [SURFACE]                                                                                               |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------------------------------ |
|  [01]   | Create geometry or a light   | RhinoCommon builds it, `add(doc, geometry, "<A::B>", Properties(...), name=)`                           |
|  [02]   | Find objects                 | `find(doc, layer_path=, object_type=, name=, test=, hidden=)`, the user's selection stays               |
|  [03]   | Select for the user          | `set_selection {"ids": [...]}` with ids from `find`                                                     |
|  [04]   | Object properties            | `change(doc, ids, Properties(...), layer_path=, name=)`, records show the overrides held                |
|  [05]   | Move or replace geometry     | `doc.Objects.Transform(Guid(id), xform, True)` and `doc.Objects.Replace(Guid(id), geometry)`            |
|  [06]   | Layer settings               | `layer(doc, "<A::B>", Properties(...))`, a visible layer turns its parents visible                      |
|  [07]   | Render material              | `material(doc, "<name>", "#RRGGBB", roughness=, metallic=, opacity=)` adds or updates, `None` keeps     |
|  [08]   | Rhino or plugin command      | `command(doc, "<macro>", "<A::B>", ids)`                                                                |
|  [09]   | See the model                | `capture(doc, "<name>", zoom=, view=, mode=, named=, size=, since=)`, then `Read` the PNG               |
|  [10]   | Move the user's view         | `show(doc, view=, named=, zoom=, mode=)`                                                                |
|  [11]   | Read `.3dm` files on disk    | `uv run --script <skill>/scripts/file3dm.py <file or folder>...` in Bash, Rhino untouched               |
|  [12]   | Open a `.3dm` for work       | `open -g -b com.mcneel.rhinoceros.9 <file>...` in Bash after `file3dm.py`, `documents()` gives its port |
|  [13]   | Save, close, export, import  | `save(doc, path)`, `close(doc)`, `export(doc, path, ids)`, `load(doc, path, "<A>")`                     |
|  [14]   | Convert files                | `convert(doc, sources, "<.suffix>", folder)` through headless documents                                 |
|  [15]   | Drawings and sheets          | Use the drafting reference, `make2d`, `sheet`, and `pdf` draw them                                      |
|  [16]   | Geometry away from documents | `RhinoDoc.CreateHeadless(None)` in a script, `Dispose()` at the end, `command` refuses it               |
|  [17]   | Grasshopper 2 definition     | Use the grasshopper reference, `g2_*` tools build and `canvas` reads, assigns, and bakes                |
|  [18]   | Plugin package or build      | Use the plugins reference, `yak` installs packages and `assembly.load` loads a build                    |

## [03]-[SCRIPTS]

`run_python` runs CPython 3.13 inside Rhino on the UI thread with `__rhino_doc__` as the slot's document, and the hook wraps each call of the session that invoked this skill:
- Scripts `import document`, `records`, `canvas`, and `assembly` fresh from the skill's directory, packages they name install on first use
- Stdout, stderr, and a raise's traceback with the script's own line numbers return as the call's output, the call itself succeeds
- Edits of one call form one undo step named after the script's first comment line, which marks the document modified
- `scriptcontext.doc` and `rhinoscriptsyntax` see the slot's document during the call, `RhinoDoc.ActiveDoc` stays Rhino's active document

Subagent calls run unwrapped:
- Scripts open with `# env: <root>/.claude/skills/use-rhino/scripts`, the modules stay cached for the process
- `ModuleNotFoundError` for `msgspec` or `pillow` takes `# r: <package>` in one call, each `# r:` line runs pip on the UI thread
- Edits go inside `doc.BeginUndoRecord("<step>")` and `doc.EndUndoRecord(record)`
- Raises return only their lines shaped like `Type: message`, a body inside `try` that prints `traceback.format_exc()` keeps every line

Rules the hook cannot hold:
- `Properties(color=, linetype=, print_color=, print_width=, material=, visible=, locked=, strings=)` sets a layer or overrides it on objects
- `None` fields in `Properties` stay as they are
- Globals reset per call, objects from an earlier call are found again through `find`
- Record ids are strings, RhinoCommon tables take `Guid("<id>")`
- Working directory is read-only `/`, file paths are absolute
- Lengths are model units, tolerances come from `doc.ModelAbsoluteTolerance` and `doc.ModelAngleToleranceRadians`
- `doc.Objects.Add*` stores invalid geometry, `add` refuses it with the `IsValidWithLog` reason
- Objects on a locked layer lock, `add`, `change`, `command`, `load`, `make2d`, and `bake` refuse a locked `layer_path`
- Boxes and extrusion commands make `Extrusion` objects, `object_type=ObjectType.Brep | ObjectType.Extrusion` finds every solid
- Scripts that wait on other UI-thread work (a Grasshopper 2 solve, `Task.Wait`) deadlock Rhino, an empty `g2_apply_graph` in a later call solves
- Roll a call back by deleting the ids it returned, `doc.Undo()` from a script leaves an undo record open that absorbs later edits
- Titled documents autosave into their own file after an edit, every edit to a user's file reaches disk without `save`
- `save` runs `_-Save` for the document's own file and `_-SaveAs` for a new one, `_-SaveAs` onto the open file raises a lock alert
- RhinoCommon `Save`, `SaveAs`, and `WriteFile` on a GUI document make macOS reopen the file and drop later edits
- `describe(doc).modified` clears on the call after `save`, `close` refuses a document with unsaved edits
- Drawing entry points (`material`, `Properties(material=)`, `capture`, `pdf`) refuse a Rhino with no active document, `Fault(RhinoDoc, None)`

## [04]-[COMMANDS]

RhinoCommon comes first where it has the operation, `command(doc, macro, layer_path, ids)` runs the rest:
- Macros answer every prompt in order, `_Enter` accepts a default, dash forms (`_-Loft`) skip dialogs, `_Name=Value` sets options
- Prompts the macro leaves unanswered cancel the command, its `results` read `Result.Cancel` and `output` ends with that prompt
- Tokens past the last prompt run as commands of their own after the command ends, `output` shows each
- Objects a command creates land on `layer_path`, a boolean result keeps its first input's layer
- `Objects.rows` are the objects created, `deleted` the ids gone, `results` each command's `Result`, `output` the history
- Commands run in the active document, `command` makes the document's window main while Rhino is in front, else an `NSApplication` fault
- Command history is one text for the whole process, another document's lines can appear in `output`
- `get_commands {"filter": "<part>"}` finds a command's English name, commands of plugins Rhino has not loaded included
- Listener resource `rhino://commands/<Name>/help` returns Rhino's help page for a built-in command, `<endpoint>` from `list_slots`:

```bash
curl -s -X POST <endpoint>/ -H 'Content-Type: application/json' \
    -d '{"jsonrpc":"2.0","id":1,"method":"resources/read","params":{"uri":"rhino://commands/<Name>/help"}}' | jq -r '.result.contents[0].text'
```

## [05]-[EVIDENCE]

Work is done when a document read and a capture show it:
- Records carry each object's layer and bounding box, RhinoCommon on the ids gives `IsValid`, `IsSolid`, `IsClosed`, length, area, and volume
- `file3dm.py` on a saved or exported `.3dm` shows the layers, counts, and views the file on disk holds, `edited` names its last save
- `capture` draws `.artifacts/rhino/<name>.png` through the view's pipeline without grid, axes, highlight, or Gumball
- Every view keeps its camera, display mode, and overlays through a capture, and the selection keeps its objects
- `zoom` takes ids, a `BoundingBox`, or `()` for every visible object, `None` draws the view as the user sees it
- Captures draw at the view's pixel size unless `size` names one, `describe(doc).current_view` names the view the user sees
- PNGs store their settings and camera, `capture(doc, "<after>", since="<before>")` redraws them and counts changed pixels
- `Capture.changed` is that count, `<after>-diff.png` marks the pixels red, an unchanged scene counts 0
- Captures draw the shared Grasshopper 2 preview in every document of the process

## [06]-[GRASSHOPPER]

One canvas per Rhino process, shared by every slot and session, `g2_*` tools edit it:
- `g2_start` precedes the first other `g2_*` call and the first `canvas` import in a process, those fail while Grasshopper 2 is unloaded
- `replace` defaults to true on every wiring tool and drops an input's earlier sources, `"replace": false` adds a second source
- Wrong-type wires solve with zero diagnostics, `canvas.graph()` shows the value each output holds
- Canvas edits record no Grasshopper undo step and leave the definition unmodified, a `.ghz` save keeps them

## [07]-[RESULTS]

Router results hold the plugin's first content block alone:
- `payload` of `guidance` alone means the call ran and its result was dropped, fix the argument the note names and read the state back
- `payload` of `error` and `message` drops detail blocks, the same call through the listener returns every block
- `rhino_crashed` or `rhino_closed` pruned the slot, `list_slots` shows what remains
