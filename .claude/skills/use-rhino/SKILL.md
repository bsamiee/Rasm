---
name: use-rhino
description: "Use when a task drives a Rhino document, a .3dm file, or a Grasshopper 2 canvas, covering slots, orientation, layers, selection, commands, files, views, materials, settings, and definitions."
---

# [RHINO_MCP]

`rhino-mcp-platform` routes each call to one Rhino document, a slot. One Rhino process holds every slot on the machine, the user's documents and other sessions' slots included, and one Grasshopper 2 canvas every slot shares.

[REFERENCES]:
- [01]-[SETUP](references/setup.md): Process, router, listener, Rhino in front, dialogs, crashes, quit and relaunch, and Grasshopper 2 start
- [02]-[SETTINGS](references/settings.md): Stores, writes and read-backs, aliases, shortcuts, panels, template, colors, fonts, Grasshopper 2 settings
- [03]-[FILES](references/files.md): Reading `.3dm` files on disk, opening, exporting, converting, importing, and headless documents
- [04]-[GRASSHOPPER](references/grasshopper.md): Building, solving, checking, baking, saving, and picturing a Grasshopper 2 definition
- [05]-[PRESENTATION](references/presentation.md): Materials, sun and ground, display modes, saved states, and window captures
- [06]-[DRAFTING](references/drafting.md): Make2D, layout sheets with scaled details, dimensions, sections and hatches, and PDF sheets
- [07]-[PLUGINS](references/plugins.md): Yak packages, and loading and proving a compiled plugin or library

[SCRIPTS]:
- [01]-[HOOK](scripts/hook.py): Project hook over `run_python` and `run_command`, nothing calls it by hand
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

## [01]-[SHARED_PROCESS]

One Rhino process serves the user and every session, and each call leaves the screen, the front application, and other documents as it found them:
- `RhinoDoc.ActiveDoc` is the document any session last activated, scripts take `__rhino_doc__` or `RhinoDoc.FromRuntimeSerialNumber(<serial>)`
- Calls send no pointer or key event, settings, probes, and captures run with Rhino behind the user's application
- Rhino comes in front for a document switch, a prompt release, and a close alone, through the setup reference's `osascript` pair
- Values a probe sets (a color, a setting, a selection) return in the `finally` of the call that set them
- Views return from `ViewportInfo(viewport)` and `viewport.DisplayMode` taken before the change, in the same call's `finally`
- Modals hold the UI thread for every session, code calls nothing the setup reference lists as raising one
- Table probes (hatch patterns, linetypes) run in a `RhinoDoc.CreateHeadless(None)` disposed in the same call, user documents take kept rows alone
- Untitled documents stay open as their session left them, closing one destroys that session's work
- Another session's document is never activated, and its window tab never comes forward
- Tasks push their own Grasshopper 2 canvas at their start and pop it at their end
- Documents, canvases, packages, and files a task opened close or leave at its end

## [02]-[START]

Every task opens with these steps before any change:
1. `list_slots` with no row means no Rhino runs, `spawn_slot {"version": "9"}` launches one, a Rhino the user started lists every document
2. `documents()` in any row's slot lists every open document by `serial`, `path` (`None` untitled), `modified`, `active`, and listener `port`
3. Files the task names that no document holds read first with `uv run --script <skill>/scripts/file3dm.py <file>...`
4. `Fault` lines name files Rhino answers with a modal alert that holds every close in the process, and stay unopened
5. `open -g -b com.mcneel.rhinoceros.9 <file>...` opens the rest behind the user's application, each as its own slot, an open file adds no document
6. Work in the document whose file or contents the task names, `spawn_slot {"version": "9"}` when none fits
7. Record the working document's `pid` and `port`, the pair names it for the task, rows with another pair belong to other sessions
8. `describe(doc)` fields decide the next step:

| [INDEX] | [FIELD]                   | [DECIDES]                                                                             |
| :-----: | :------------------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `units`, `tolerance`      | Every number in a script or macro is in model units                                   |
|  [02]   | `active`                  | `command`, `save`, and `close` on an inactive document run in a call held in front    |
|  [03]   | `prompt`                  | Command waiting for input, its release in the setup reference precedes any other call |
|  [04]   | `layers`, `current_layer` | Hierarchy new objects join, an add without attributes lands on layer index 0          |
|  [05]   | `selected`                | Objects the user means by "this" or "these"                                           |
|  [06]   | `current_view`, `views`   | View the user looks at, names and cameras `capture`, `show`, and `save_view` take     |
|  [07]   | `materials`               | Physically based materials a `Properties(material=)` names for a layer or object      |
|  [08]   | `path`, `modified`        | File `save(doc)` writes, `None` takes a path, `True` marks unsaved edits              |

Every call passes `slot`, a call without one lands in the slot this session used last, else in the user's first document:
- Slot names and `adopted` change while a document stays open, a spawned or opened document can return as `adopted: true` under another name
- `list_slots` before each step that writes, the row with the recorded `pid` and `port` gives that step's `slot`
- `slot_not_found` on the recorded name reads `list_slots` again, no row with the pair means the document is gone
- `documents()` rows with `port` `None` answer no slot, a call in another slot reaches them by `serial`

## [03]-[ROUTING]

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
|  [17]   | Application setting          | Use the settings reference, the owning `Rhino.ApplicationSettings` class or `PersistentSettings`        |
|  [18]   | Display mode                 | Use the presentation reference, `DisplayModeDescription` owns modes and `capture(mode=)` draws one      |
|  [19]   | Interface picture            | `screencapture -x -o -l <window id> <file>.png` from the presentation reference, no activation          |
|  [20]   | Grasshopper 2 definition     | Use the grasshopper reference, `g2_*` tools build and `canvas` reads, assigns, and bakes                |
|  [21]   | Plugin package or build      | Use the plugins reference, `yak` installs packages and `assembly.load` loads a build                    |

## [04]-[SCRIPTS]

`run_python` runs CPython 3.13 inside Rhino on the UI thread with `__rhino_doc__` as the slot's document, and the project hook wraps every call:
- Scripts `import document`, `records`, `canvas`, and `assembly` fresh from the skill's directory, packages they name install on first use
- Stdout, stderr, and a raise's traceback with the script's own line numbers return as the call's output, the call itself succeeds
- Edits of one call form one undo step named after the script's first comment line, which marks the document modified
- `scriptcontext.doc` and `rhinoscriptsyntax` see the slot's document during the call
- One CPython serves every run in the process, listener calls and Grasshopper 2 components included, and ignores `PYTHON*` variables
- `_-ScriptEditor _Run "<file>"` runs a `.py` file on that CPython
- `# env: <folder>` puts a folder on `sys.path` for one run, its modules stay in `sys.modules` until a run deletes them or Rhino quits

Rules the hook cannot hold:
- Globals reset per call, objects from an earlier call are found again through `find`
- Record ids are strings, RhinoCommon tables take `Guid("<id>")`
- Working directory is read-only `/`, file paths are absolute
- Lengths are model units, tolerances come from `doc.ModelAbsoluteTolerance` and `doc.ModelAngleToleranceRadians`
- `Rhino.UI.Localization.FormatNumber(<length>, doc.ModelUnitSystem, Rhino.UI.DistanceDisplayMode.FeetInches, <p>, False)` states a length to 1/2^p in
- `doc.AdjustModelUnitSystem(<UnitSystem>, False)` changes model units without scaling objects
- `Properties(color=, linetype=, print_color=, print_width=, material=, visible=, locked=, strings=)` sets a layer or overrides objects, `None` keeps
- `doc.Objects.Add*` stores invalid geometry, `add` refuses it with the `IsValidWithLog` reason
- Objects on a locked layer lock, `add`, `change`, `command`, `load`, `make2d`, and `bake` refuse a locked `layer_path`
- Boxes and extrusion commands make `Extrusion` objects, `object_type=ObjectType.Brep | ObjectType.Extrusion` finds every solid
- Scripts that wait on other UI-thread work (a Grasshopper 2 solve, `Task.Wait`) deadlock Rhino, an empty `g2_apply_graph` in a later call solves
- Roll a call back by deleting the ids it returned, `doc.Undo()` from a script leaves an undo record open that absorbs later edits
- `PushViewProjection` in one call and `PopViewProjection` in a later one return `False` and keep the changed camera
- Out parameters take no argument and follow the return value in a tuple, `TryGetBool(key)` reads `(found, value)`, `PlugInExists(id)` a triple
- Enum parameters take a member or `<Enum>(<int>)`, pythonnet converts no integer
- Enum members named `None` take another spelling, `ShowContentChooserFlags.NONE` and `FontStyle(0)`, `FontStyle.None` is a syntax error
- `RenderContent.GetParameter(name)` returns a `Variant`, `.ToDouble()`, `.ToColor4f()`, or the content's `Xml` holds the value
- Titled documents autosave into their own file when Rhino leaves the front and every 5 idle minutes, edits reach disk without `save`
- `save` runs `_-Save` for the document's own file and `_-SaveAs` for a new one, `_-SaveAs` onto the open file raises a lock alert
- RhinoCommon `Save`, `SaveAs`, and `WriteFile` on a GUI document make macOS reopen the file and drop later edits
- `describe(doc).modified` clears on the call after `save`, `close` saves a titled document's unsaved edits and discards an untitled document's

## [05]-[COMMANDS]

RhinoCommon comes first where it has the operation, `command(doc, macro, layer_path, ids)` runs the rest:
- Macros answer every prompt in order, `_Enter` accepts a default, `!` cancels a running command, `'` runs inside one, `-` takes the scripted form
- Options take their full `_Name=Value` form, abbreviated option text depends on the command-line locale
- Prompts the macro leaves unanswered cancel the command, its `results` read `Result.Cancel` and `output` ends with that prompt
- Tokens past the last prompt run as commands of their own after the command ends, `output` shows each
- `_Pause` in a macro waits for a pick in the viewport and holds the call and every later call in the process
- Selection commands (`_SelDup`, `_SelLast`) add to the selection `ids` set, `_SelNone` first gives them an empty one
- Objects a command creates land on `layer_path`, a boolean result keeps its first input's layer
- `Objects.rows` are the objects created, `deleted` the ids gone, `results` each command's `Result`, `output` the history
- Command history is one text for the whole process, another session's lines can appear in `output`
- `get_commands {"filter": "<part>"}` finds a command's English name, commands of registered plugins Rhino has not loaded included
- `Command.IsCommand("<name>")` decides whether a macro token is a command, hidden commands (`_OptionsPage`) run from macros while it reads `False`
- Listener resource `rhino://commands/<Name>/help` returns Rhino's help page for a built-in command, `<endpoint>` from `list_slots`:

```bash
curl -s -X POST <endpoint>/ -H 'Content-Type: application/json' \
    -d '{"jsonrpc":"2.0","id":1,"method":"resources/read","params":{"uri":"rhino://commands/<Name>/help"}}' | jq -r '.result.contents[0].text'
```

## [06]-[EVIDENCE]

Work is done when a document read and a capture show it:
- Records carry each object's layer and bounding box, RhinoCommon on the ids gives `IsValid`, `IsSolid`, `IsClosed`, length, area, and volume
- `file3dm.py` on a saved or exported `.3dm` shows the layers, counts, and views the file on disk holds, `edited` names its last save
- `capture` draws `.artifacts/rhino/<name>.png` through the view's pipeline without grid, axes, highlight, or Gumball
- Clipping planes stay out of `capture`, a section cut shows in a window capture from the presentation reference
- `viewport.DisplayMode` set in a call draws in captures from the next call, `capture(mode=)` draws a mode at once
- Every view keeps its camera, display mode, and overlays through a capture, and the selection keeps its objects
- `zoom` takes ids, a `BoundingBox`, or `()` for every visible object, `None` draws the view as the user sees it
- Captures draw at the view's device pixel size unless `size` names one
- `viewport.WorldToClient(point)` maps a point to those pixels, `describe(doc).current_view` names the view the user sees
- `Bitmap.GetPixel` reads the frame buffer and `magick` reads the PNG through its profile, compare values from one path alone
- PNGs store their settings and camera, `capture(doc, "<after>", since="<before>")` redraws them and counts changed pixels
- `Capture.changed` is that count, `<after>-diff.png` marks the pixels red, an unchanged scene counts 0
- Captures draw the current Grasshopper 2 canvas's preview in every document of the process while the editor holds that canvas active

## [07]-[GRASSHOPPER]

`g2_*` tools edit the current canvas every slot and session shares:
- `g2_start` precedes the first other `g2_*` call and the first `canvas` import in a process, those fail while Grasshopper 2 is unloaded
- `replace` defaults to true on every wiring tool and drops an input's earlier sources, `"replace": false` adds a second source
- Wrong-type wires solve with zero diagnostics, `canvas.graph()` shows the value each output holds
- Canvas edits record no Grasshopper undo step and leave the definition unmodified, a `.ghz` save keeps them

## [08]-[RESULTS]

Router results hold the plugin's first content block alone:
- `payload` of `guidance` alone means the call ran and its result was dropped, fix the argument the note names and read the state back
- `payload` of `error` and `message` drops detail blocks, the same call through the listener returns every block
- `rhino_crashed` or `rhino_closed` pruned the slot, `list_slots` shows what remains
