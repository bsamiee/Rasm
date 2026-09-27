---
name: use-rhino
description: "Use when a task drives a Rhino document, a .3dm file, or a Grasshopper 2 canvas, covering slots, orientation, layers, selection, commands, files, views, materials, settings, and definitions."
---

# [RHINO_MCP]

`rhino-mcp-platform` routes each call to one Rhino document, a slot. One Rhino process holds every slot on a machine, the user's documents and other sessions' slots included.

[REFERENCES]:
- [01]-[SETUP](references/setup.md): Process, router, listener, prompts and documents, dialogs, crashes, quit and relaunch, and Grasshopper 2 start
- [02]-[SETTINGS](references/settings.md): Stores, writes, aliases, shortcuts, panels, template, colors, fonts, Grasshopper 2 settings
- [03]-[FILES](references/files.md): Reading `.3dm` files on disk, opening, exporting, converting, importing, and headless documents
- [04]-[GRASSHOPPER](references/grasshopper.md): Grasshopper 2 task documents, components, builds, values, checks, bakes, files, and libraries
- [05]-[PRESENTATION](references/presentation.md): Materials, sun and ground, display modes, saved states, and window captures
- [06]-[DRAFTING](references/drafting.md): Make2D, layout sheets with scaled details, dimensions, sections and hatches, and PDF sheets
- [07]-[PLUGINS](references/plugins.md): Yak packages, and loading a compiled plugin or library and calling its commands and functions

[SCRIPTS]:
- [01]-[HOOK](scripts/hook.py): Project hook that wraps `run_python` and refuses router tools a script entry point replaces, nothing calls it by hand
- [02]-[RECORDS](scripts/records.py): `Record`, `Fault`, `File`, `Properties`, and the layer and material records every script shares
- [03]-[DOCUMENT](scripts/document.py): Entry points over the open documents, one document, and a `.rhp` or library load, each returning a record
- [04]-[CANVAS](scripts/canvas.py): Grasshopper 2 task documents, builds, layout, values, bakes, clusters, plugins, and pictures
- [05]-[FILE3DM](scripts/file3dm.py): `uv run --script` over `.3dm` files or folders prints each file's record, no Rhino involved

Results print as records with a class naming their case. Rejected inputs return a tuple of `Fault(source, value, accepted)`, `source` the Rhino type that refused `value` and `accepted` its alternatives:

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

Each call leaves screen, front application, and other documents as it found them:
- `RhinoDoc.ActiveDoc` is the document any session last activated, scripts take `__rhino_doc__` or `RhinoDoc.FromRuntimeSerialNumber(<serial>)`
- Calls send no pointer or key event and run with Rhino behind the user's application
- Documents become active only while the user holds Rhino in front, prompt releases, saves, and closes run behind
- Temporary values (a color, a setting, a selection) return in the `finally` of their setting call
- Views return from `ViewportInfo(viewport)` and `viewport.DisplayMode` taken before a change, in the same call's `finally`
- Modals hold Rhino's shared UI thread and can bring Rhino front as they end, code calls nothing the setup reference lists as raising one
- Trial table rows (hatch patterns, linetypes) go in a `RhinoDoc.CreateHeadless(None)` disposed in the same call, user documents take kept rows alone
- Untitled documents stay open as their session left them, closing one destroys its session's work
- Other sessions' documents stay inactive with their window tabs behind
- Tasks build Grasshopper 2 definitions in their own documents and leave the user's current canvas unchanged
- Documents a task opened close at its end, a titled one through `close(doc)`, a spawned untitled one through `close_slot`
- Canvases, packages, and files a task opened leave at its end, and disk keeps only the files the task produces

## [02]-[START]

Tasks run steps in order before any change:
1. `list_slots` rows and `pgrep -x Rhinoceros` pids show a running Rhino the task uses, `spawn_slot {"version": "9"}` launches one when neither shows it
2. `pgrep` pids with no `list_slots` row take the setup reference's listener check before any other call
3. `spawn_slot` answering `startup_timeout` names a slot that can arrive later, `list_slots` precedes any retry
4. `documents()` in any row's slot lists every open document by `serial`, `path` (`None` untitled), `modified`, `active`, and listener `port`
5. Files the task names that no document holds read first with `uv run --script <skill>/scripts/file3dm.py <file>...`
6. `Fault` lines name files Rhino answers with a modal alert that holds every close in the process, and stay unopened
7. `open -g -b com.mcneel.rhinoceros.9 <file>...` opens other files behind the user's application, each as its own slot, an open file adds no document
8. Work in the task's named document, else in a scratchpad copy of its unit system's `Template Files/` template, closed unsaved and deleted at the end
9. Record the working document's `serial`, `pid`, and `port`
10. `describe(doc)` fields decide the next step:

| [INDEX] | [FIELD]                   | [DECIDES]                                                                                              |
| :-----: | :------------------------ | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `units`, `tolerance`      | Every number in a script or macro is in model units                                                    |
|  [02]   | `active`                  | `command` refuses an inactive document, `close(doc)` then `open -g` on its file reopen it active       |
|  [03]   | `prompt`                  | Command waiting in Rhino, setup release frees a task's own, another session's holds every command      |
|  [04]   | `layers`, `current_layer` | Hierarchy new objects join, adds without attributes take the active document's current layer index     |
|  [05]   | `selected`                | Objects the user means by "this" or "these"                                                            |
|  [06]   | `current_view`, `views`   | View the user looks at, names and cameras `capture`, `show`, and `save_view` take, each layout's scale |
|  [07]   | `page_units`, `style`     | Paper size units, and the annotation style new dimensions take                                         |
|  [08]   | `named_cplanes`           | Construction planes a script restores by name                                                          |
|  [09]   | `materials`               | Physically based materials a `Properties(material=)` names for a layer or object                       |
|  [10]   | `path`, `modified`        | File `save(doc)` writes, `None` takes a path, `True` marks unsaved edits                               |

Every call passes `slot`, a call without one runs in its session's last-used slot, else in the user's first document:
- Slot names and `adopted` change while a document stays open, a spawned or opened document can return as `adopted: true` under another name
- Ports a closed document frees pass to the next document while its slot row stays, a row's `pid` and `port` can name another document
- Scripts reach the working document through `RhinoDoc.FromRuntimeSerialNumber(<serial>)` from any slot, `None` means it closed
- `list_slots` runs before each writing step, its row on a recorded `serial`'s `documents()` port names the step's `slot`
- `documents()` rows with `port` `None` answer no slot, a call in another slot reaches them by `serial`

## [03]-[ROUTING]

| [INDEX] | [JOB]                         | [CALL]                                                                                                  |
| :-----: | :---------------------------- | :------------------------------------------------------------------------------------------------------ |
|  [01]   | Create geometry or annotation | RhinoCommon builds it, `add(doc, geometry, "<A::B>", Properties(...), name=, page=)`                    |
|  [02]   | Find objects                  | `find(doc, layer_path=, object_type=, name=, test=, hidden=)`, the user's selection stays               |
|  [03]   | Select for the user           | `set_selection {"ids": [...]}` with ids from `find`                                                     |
|  [04]   | Object properties             | `change(doc, ids, Properties(...), layer_path=, name=)`, records show the overrides held                |
|  [05]   | Move or replace geometry      | `doc.Objects.Transform(Guid(id), xform, True)` and `doc.Objects.Replace(Guid(id), geometry)`            |
|  [06]   | Layer settings                | `layer(doc, "<A::B>", Properties(...))`, a visible layer turns its parents visible                      |
|  [07]   | Render material               | `material(doc, "<name>", "#RRGGBB", roughness=, metallic=, opacity=)` adds or updates, `None` keeps     |
|  [08]   | Rhino or plugin command       | `command(doc, "<macro>", "<A::B>", ids)`                                                                |
|  [09]   | See the model                 | `capture(doc, "<name>", zoom=, view=, mode=, named=, size=, since=)`, then `Read` its PNG               |
|  [10]   | Move the user's view          | `show(doc, view=, named=, zoom=, mode=)`                                                                |
|  [11]   | Read `.3dm` files on disk     | `uv run --script <skill>/scripts/file3dm.py <file or folder>...` in Bash, Rhino untouched               |
|  [12]   | Open a `.3dm` for work        | `open -g -b com.mcneel.rhinoceros.9 <file>...` in Bash after `file3dm.py`, `documents()` gives its port |
|  [13]   | Save, close, export, import   | `save(doc, path)`, `close(doc)`, `export(doc, path, ids)`, `load(doc, path, "<A>")`                     |
|  [14]   | Convert files                 | `convert(doc, sources, "<.suffix>", folder)` through headless documents                                 |
|  [15]   | Drawings and sheets           | Use the drafting reference, `make2d`, `sheet`, and `pdf` draw them                                      |
|  [16]   | Geometry away from documents  | `RhinoDoc.CreateHeadless(None)` in a script, `Dispose()` at the end, `command` refuses it               |
|  [17]   | Application setting           | Use the settings reference, owning `Rhino.ApplicationSettings` class or `PersistentSettings`            |
|  [18]   | Display mode                  | Use the presentation reference, `DisplayModeDescription` owns modes and `capture(mode=)` draws one      |
|  [19]   | Interface picture             | `screencapture -x -o -l <window id> <file>.png` from the presentation reference, no activation          |
|  [20]   | Grasshopper 2 definition      | Use the grasshopper reference, `g2_search_components` finds parts and `canvas` builds and bakes         |
|  [21]   | Plugin package or build       | Use the plugins reference, `yak` installs packages and `document.assembly` loads a build                |

## [04]-[SCRIPTS]

`run_python` runs CPython on Rhino's UI thread with `__rhino_doc__` as its slot's document, and the project hook wraps every call:
- Scripts `import document`, `records`, and `canvas` fresh from the skill's directory, packages they name install on first use
- Stdout, stderr, and a raise's traceback with script line numbers return as call output, and the call itself succeeds
- Edits of one call form one undo step named after its script's first comment line and mark the document modified
- `scriptcontext.doc` and `rhinoscriptsyntax` see the slot's document during each call
- One CPython serves every run in the process, listener calls and Grasshopper 2 components included, and ignores `PYTHON*` variables
- `_-ScriptEditor _Run "<file>"` runs a `.py` file on the shared CPython, `_NoEcho` ahead of it keeps its prompts and path out of the history
- `# env: <folder>` puts a folder on `sys.path` for one run, its modules stay in `sys.modules` until a run deletes them or Rhino quits

Rules the hook cannot hold:
- Globals reset per call, objects from an earlier call are found again through `find`
- Record ids are strings, RhinoCommon tables take `Guid("<id>")`
- Working directory is read-only `/`, file paths are absolute
- Lengths are model units, tolerances come from `doc.ModelAbsoluteTolerance` and `doc.ModelAngleToleranceRadians`
- `Rhino.UI.Localization.FormatNumber(<length>, doc.ModelUnitSystem, Rhino.UI.DistanceDisplayMode.FeetInches, <p>, False)` states a length to 1/2^p in
- `doc.AdjustModelUnitSystem(<UnitSystem>, False)` sets the model unit alone, tolerance, distance display, page units, style, and grid numbers stay
- `Properties(color=, linetype=, print_color=, print_width=, material=, visible=, locked=, strings=)` sets a layer or overrides objects, `None` keeps
- `ObjectAttributes()` holds layer index 0, a script's add sets `LayerIndex` from `document.layer_index(doc, "<A::B>")`
- Objects on a locked layer lock, `add`, `change`, `command`, `load`, `make2d`, and `bake` refuse a locked `layer_path`
- Boxes and extrusion commands make `Extrusion` objects, `object_type=ObjectType.Brep | ObjectType.Extrusion` finds every solid
- Scripts that wait on other UI-thread work (a Grasshopper 2 solve, `Task.Wait`) deadlock Rhino, a solve started in one call reads in the next
- Roll a call back by deleting the ids it returned, `doc.Undo()` from a script leaves an undo record open that absorbs later edits
- `PushViewProjection` in one call and `PopViewProjection` in a later one return `False` and keep the changed camera
- Out parameters take no argument and follow the return value in a tuple, `TryGetBool(key)` reads `(found, value)`, `PlugInExists(id)` a triple
- .NET arrays reach a collection overload through `method.Overloads[IEnumerable[T]](array)`, pythonnet binds the single-item overload otherwise
- Enum parameters take a member or `<Enum>(<int>)`, pythonnet converts no integer
- Enum members named `None` take another spelling, `ShowContentChooserFlags.NONE` and `FontStyle(0)`, `FontStyle.None` is a syntax error
- `RenderContent.GetParameter(name)` returns a `Variant`, `.ToDouble()`, `.ToColor4f()`, or the content's `Xml` holds its value
- Titled documents autosave into their own file when Rhino leaves the front and every 5 idle minutes, edits reach disk without `save`
- `save` writes through the window's `NSDocument`, a new `path` becomes the document's file
- `doc.Save`, `SaveAs`, and `WriteFile` onto a GUI document's file make macOS reopen it and drop later edits, `WriteFile` elsewhere changes nothing
- Writes drawing a preview image (`IncludePreviewImage` True, `close_slot`) spin Rhino's UI thread while the process holds no active document
- `describe(doc).modified` clears inside the `save` call, `close` saves a titled document's unsaved edits and discards an untitled document's

## [05]-[COMMANDS]

RhinoCommon comes first where it has an operation, `command(doc, macro, layer_path, ids)` runs the rest:
- Macros answer every prompt in order, `_Enter` accepts a default, `!` cancels a running command, `'` runs inside one, `-` takes the scripted form
- Options take their full `_Name=Value` form, abbreviated option text depends on the command-line locale
- Prompts a macro leaves unanswered cancel its command, `results` read `Result.Cancel`, and `output` ends with the unanswered prompt
- Tokens past a macro's last prompt run as commands of their own after the command ends, `output` shows each
- `_Pause` in a macro waits for a viewport pick and holds its call and every later call in the process
- Selection commands (`_SelDup`, `_SelLast`) add to the selection `ids` set, `_SelNone` first gives them an empty one
- Objects a command creates go on `layer_path` apart from dimensions and leaders, a boolean result keeps its first input's layer
- `Objects.rows` are created objects, `deleted` removed ids, `results` each command's `Result`, `output` the history
- Command history is one text for the whole process, another session's lines can appear in `output`
- `get_commands {"filter": "<part>"}` finds a command's English name, registered plugins Rhino has not loaded included
- Hidden commands stay out of `get_commands` and run from macros, `document.assembly` lists a plugin's hidden commands
- `Command.IsCommand("<name>")` decides whether a macro token is a command and reads `False` for hidden commands (`_OptionsPage`)
- Listener resource `rhino://commands/<Name>/help` returns Rhino's help page for a built-in command, `<endpoint>` from `list_slots`:

```bash
curl -s -X POST <endpoint>/ -H 'Content-Type: application/json' \
    -d '{"jsonrpc":"2.0","id":1,"method":"resources/read","params":{"uri":"rhino://commands/<Name>/help"}}' | jq -r '.result.contents[0].text'
```

## [06]-[EVIDENCE]

Work is done when a document read and a capture show it:
- Records hold each object's layer and bounding box, RhinoCommon on the ids gives `IsValid`, `IsSolid`, `IsClosed`, length, area, and volume
- `file3dm.py` on a saved or exported `.3dm` shows layers, counts, and views the file on disk holds, `edited` names its last save
- `capture` draws `.artifacts/rhino/<name>.png` through the view's pipeline without grid, axes, highlight, or Gumball
- Clipping planes stay out of `capture`, a section cut shows in a window capture from the presentation reference
- `viewport.DisplayMode` set in a call draws in captures from the next call, `capture(mode=)` draws a mode at once
- Every view keeps its camera, display mode, and overlays through a capture, and the selection keeps its objects
- `zoom` takes ids, a `BoundingBox`, or `()` for every visible object, `None` draws the view unchanged
- Captures draw at the view's device pixel size unless `size` names one
- `viewport.WorldToClient(point)` maps a point to capture pixels, `describe(doc).current_view` names the user's view
- `Bitmap.GetPixel` reads the frame buffer and `magick` reads a PNG through its profile, compare values from one path alone
- PNGs store their settings and camera, `capture(doc, "<after>", since="<before>")` redraws them and counts changed pixels
- `Capture.changed` holds the count, `<after>-diff.png` marks changed pixels red, an unchanged scene counts 0
- `capture`, `pdf`, and `material` return `Fault(RhinoDoc, None)` with no active document, a file `open -g` opens becomes active
- Captures leave out the Grasshopper 2 preview, and baked objects show in them

## [07]-[GRASSHOPPER]

`g2_*` tools edit the current canvas every slot and session shares:
- `g2_start` precedes the first `canvas` import in a process, `g2_*` tools start Grasshopper 2 themselves
- Builds record no Grasshopper undo step and leave the definition unmodified, a `.ghz` save keeps them

## [08]-[RESULTS]

Router results hold the plugin's first content block alone:
- `payload` of `guidance` alone means the call ran and its result dropped, fix each argument its note names and read state back
- `payload` of `error` and `message` drops detail blocks, the same call through a listener returns every block
- `rhino_crashed` and `rhino_closed` mean Rhino ended mid-call, the setup reference's exit read tells a crash from another session's quit
- `close_failed` from `close_slot` with its slot gone from `list_slots` means the close ran
- `unexpected` naming `TaskCanceledException` is a router timeout, the setup reference's command log shows whether its command started
