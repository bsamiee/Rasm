---
name: use-rhino
description: "Use when a task drives a Rhino document, a .3dm file, or a Grasshopper 2 canvas, covering slots, orientation, layers, selection, commands, files, views, materials, settings, and definitions."
---

# [RHINO_MCP]

`rhino-mcp-platform` routes each call to a slot, one document of one Rhino process every session and the user share. The project hook runs each `run_python` script through `document.run` on its slot's `__rhino_doc__`, and scripts print records and `Faults` as results.

[REFERENCES]:
- [01]-[SETUP](references/setup.md): Listener recovery, direct listener calls, prompts and dialogs, and exits
- [02]-[FILES](references/files.md): `.3dm` reads on disk, saves, exports, conversions, imports, and headless documents
- [03]-[OBJECTS](references/objects.md): Layers, objects, and selection
- [04]-[VIEWS](references/views.md): Captures, display modes, saved states, and window captures
- [05]-[RENDERING](references/rendering.md): Materials, environments, render settings, and renders
- [06]-[DRAFTING](references/drafting.md): Drawings, sheets, dimensions, sections, and PDFs
- [07]-[SETTINGS](references/settings.md): Application settings every document and session shares
- [08]-[PLUGINS](references/plugins.md): Yak packages and repository builds loaded into Rhino
- [09]-[GRASSHOPPER](references/grasshopper.md): Grasshopper 2 definitions

[SCRIPTS]:
- [01]-[HOOK](scripts/hook.py): Project hook running each `run_python` script through `document.run`
- [02]-[RECORDS](scripts/records.py): Records and faults every script returns, a `Fault` naming the type that refused a value and values it accepts
- [03]-[DOCUMENT](scripts/document.py): Entry points over documents, objects, files, views, renders, drawings, and plugin loads
- [04]-[CANVAS](scripts/canvas.py): Grasshopper 2 task documents, builds, values, solves, bakes, and pictures
- [05]-[FILE3DM](scripts/file3dm.py): `.3dm` file records read on disk without Rhino

Records print with a class naming their case and rejected inputs as `Faults`, here for a massing of model lengths `width`, `depth`, and `height`:

```python
# Add massing on <A::B>
from Rhino.Geometry import Box, Interval, Plane
import document
from records import Properties

doc = __rhino_doc__
massing = Box(Plane.WorldXY, Interval(0, width), Interval(0, depth), Interval(0, height)).ToBrep()
print(document.layer(doc, "<A::B>", Properties(color="#<RRGGBB>")))
print(document.add(doc, massing, "<A::B>", name="<name>"))
print(document.capture(doc, "<name>"))
```

## [01]-[SHARED_PROCESS]

Each call leaves screen, pointer, keys, front application, other sessions' documents, and the user's Grasshopper 2 canvas as it found them:
- `RhinoDoc.ActiveDoc` is the document any session last activated, scripts take `__rhino_doc__` or `RhinoDoc.FromRuntimeSerialNumber(<serial>)`
- Documents activate only while the user holds Rhino in front, a file `open -g` opens arriving active with Rhino behind
- Prompt releases, saves, and closes run with Rhino behind
- `capture`, `pdf`, `material`, `save`, `close`, and `Properties(material=)` return a `RhinoDoc` fault while no document is active
- Trial changes (color, setting, selection, `ViewportInfo(viewport)` camera, display mode) revert in their call's `finally`
- Prompts and modal dialogs hold Rhino's UI thread for every call until the setup reference's release frees them
- Trial table rows (hatch patterns, linetypes) go in a `RhinoDoc.CreateHeadless(None)` the same call disposes, user documents taking kept rows alone
- Untitled documents of other sessions stay open and inactive, closing one destroys its session's work
- Tasks close documents and canvases they opened, their working document last, and remove packages and files they added apart from their products
- Host facts come from `docs/research/applications/rhino/` listings, a named fact or decompile, the installed assembly, then a `spawn_slot` document

## [02]-[START]

Tasks orient in order before any change:
1. `list_slots` rows and `pgrep -x Rhinoceros` pids show a running Rhino, a pid no row lists taking the setup reference's listener recovery first
2. `documents()` in any row's slot lists every open document by `serial`, `path` (`None` untitled), `modified`, `active`, and listener `port`
3. Titled documents match a named file by `path`, and `open -g` of their file changes nothing
4. `uv run --script <skill>/scripts/file3dm.py <file>...` reads named files no document holds, a file it answers with `Faults` staying unopened
5. `open -g -b com.mcneel.rhinoceros.9 <file>...` opens each other named file as its own active document and slot, launching Rhino when none runs
6. Tasks naming no document open a copy of `Template Files/<template>.3dm` the same way, `Default` in feet and `Metric` in millimeters
7. Record the working document's `serial`, `pid`, and `port`
8. `describe(doc)` fields decide the next step:

| [INDEX] | [FIELD]                           | [DECIDES]                                                                          |
| :-----: | :-------------------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `units`, `tolerance`              | Every number in a script or macro is in model units                                |
|  [02]   | `active`                          | `command` and `render` refuse an inactive document                                 |
|  [03]   | `prompt`                          | Command waiting in the process, released before any command                        |
|  [04]   | `layers`, `current_layer`         | Hierarchy new objects join                                                         |
|  [05]   | `selected`                        | Objects the user means by "this" or "these"                                        |
|  [06]   | `types`, `hidden`, `locked`       | Model-space counts, hidden objects answering `find(hidden=True)` alone             |
|  [07]   | `current_view`, `views`           | View the user looks at, names and cameras `capture` and `show` take, layout scales |
|  [08]   | `page_units`, `style`             | Paper units, and the annotation style new dimensions take                          |
|  [09]   | `named_views`, `named_cplanes`    | Views and construction planes a script restores by name                            |
|  [10]   | `named_positions`, `layer_states` | Placements `position` and layers `doc.NamedLayerStates.Restore` bring back         |
|  [11]   | `materials`                       | Physically based materials a `Properties(material=)` names                         |
|  [12]   | `path`, `modified`                | File `save(doc)` writes, `None` takes a path, `True` marks unsaved edits           |

Calls pass `slot`, a call without one running in the session's last-used slot, else in the user's first document:
- `list_slots` before each writing step names the row on recorded `pid` and `port`, slot names and `adopted` changing over time
- Ports a closed document frees pass to the next document while its row stays, `documents()` naming each port's listening serial
- Scripts bind `RhinoDoc.FromRuntimeSerialNumber(<serial>)` in any slot, reaching documents with `port` `None` and reading `None` once one closed
- `spawn_slot` gives an untitled document in place of a template copy, launching Rhino 9 when none runs
- `spawn_slot` documents close unsaved through a call that edits nothing setting `doc.Modified = False`, then `close_slot`
- `Template Files/` sits in `~/Library/Application Support/McNeel/Rhinoceros/`
- Template copies close unsaved from another slot through `doc.Modified = False` then `close(doc)` in one call

## [03]-[ROUTING]

| [INDEX] | [JOB]                         | [CALL]                                                                                                  |
| :-----: | :---------------------------- | :------------------------------------------------------------------------------------------------------ |
|  [01]   | Create geometry or annotation | `add(doc, geometry, layer_path=, properties=, name=, page=)` places it, annotation without `layer_path` |
|  [02]   | Find objects                  | `find(doc, layer_path=, object_type=, name=, test=, hidden=)`, the user's selection unchanged           |
|  [03]   | Select for the user           | `set_selection {"ids": [...]}` with ids from `find`                                                     |
|  [04]   | Object properties             | `change(doc, ids, Properties(...), layer_path=, name=)`                                                 |
|  [05]   | Move or replace geometry      | `doc.Objects.Transform(Guid(id), xform, True)`, `doc.Objects.Replace(Guid(id), geometry)`               |
|  [06]   | Layer settings                | `layer(doc, "<A::B>", Properties(...))`                                                                 |
|  [07]   | Render material               | `material(doc, "<name>", "#<RRGGBB>", roughness=, metallic=, opacity=, textures=)`                      |
|  [08]   | Environment or render         | `environment(doc, "<name>", "<image>.exr")`, `render(doc, "<name>", view=, size=, samples=)`            |
|  [09]   | Rhino or plugin command       | `command(doc, "<macro>", "<A::B>", ids)`                                                                |
|  [10]   | See the model                 | `capture(doc, "<name>", zoom=, view=, mode=, named=, size=, since=)`, then `Read` of its PNG            |
|  [11]   | Move the user's view          | `show(doc, view=, named=, zoom=, mode=)`, with `save_view` and `position` recording states              |
|  [12]   | Save, close, export, import   | `save(doc, path)`, `close(doc)`, `export(doc, path, ids)`, `load(doc, path, "<A>")`                     |
|  [13]   | Convert files                 | `convert(doc, sources, "<.suffix>", folder)` through headless documents                                 |
|  [14]   | Drawings and sheets           | `make2d`, `sheet`, and `pdf` from the drafting reference                                                |
|  [15]   | Geometry away from documents  | `RhinoDoc.CreateHeadless(None)` disposed in its creating call, `command` refusing it                    |
|  [16]   | Application setting           | Owning `Rhino.ApplicationSettings` class or `PersistentSettings` key from the settings reference        |
|  [17]   | Display mode                  | `DisplayModeDescription` from the views reference, `capture(mode=)` drawing one                         |
|  [18]   | Interface picture             | `screencapture -x -o -l <window id>` from the views reference, Rhino left behind                        |
|  [19]   | Grasshopper 2 definition      | `canvas` builds and bakes, `g2_search_components` finds parts, the grasshopper reference                |
|  [20]   | Plugin package or build       | `yak` installs packages, `document.assembly` loads builds, the plugins reference                        |

## [04]-[SCRIPTS]

`run_python` runs CPython on Rhino's UI thread, the hook wrapping each script in `document.run` under a `# env:` line for the skill's `scripts/` folder:
- Scripts `import document`, `records`, and `canvas`, each call importing them fresh
- Directive comments (`# r:`, `# env:`, `# flag:`) in a script's first 30 lines reach RhinoCode under the hook's line
- `# r: <package>` installs a package a skill module's `dependencies` names once its import raises `ModuleNotFoundError`
- Edits of one call form one undo step named after the script's first comment line and mark the document modified
- `scriptcontext.doc` and `rhinoscriptsyntax` see the slot's document during each call
- One CPython serves every run in the process, listener calls and Grasshopper 2 components included, its isolated config ignoring `PYTHON*` variables
- `PYTHONPATH` reaches `sys.path` as one unsplit entry RhinoCode prepends on each run
- `# env: <folder>` puts a folder on `sys.path` for one run, and its modules stay in `sys.modules` until a run deletes them
- `_NoEcho _-ScriptEditor _Run "<file>"` runs a `.py` file on the shared CPython with no history line

Rules the hook cannot hold:
- Globals reset per call, later calls reach objects through printed ids or `find`
- Record ids are strings, RhinoCommon tables take `Guid("<id>")`
- Working directory is read-only `/`, file paths are absolute
- Lengths are model units, `RhinoMath.UnitScale(UnitSystem.<from>, doc.ModelUnitSystem)` scaling one given in another unit
- Tolerances come from `doc.ModelAbsoluteTolerance` and `doc.ModelAngleToleranceRadians`
- `Rhino.UI.Localization.FormatNumber(<length>, doc.ModelUnitSystem, Rhino.UI.DistanceDisplayMode.FeetInches, <p>, False)` prints to 1/2^p in
- `doc.AdjustModelUnitSystem(UnitSystem.<unit>, False)` sets the model unit alone, tolerance, distance display, page units, style, and grid staying
- Solves started in one call read in the next, a call waiting on UI-thread work (`Task.Wait`, a Grasshopper 2 solve) deadlocking Rhino
- Python callables reach .NET as event handlers the same call removes, library delegates running in the plugins reference's C# script
- Out parameters take no argument and follow the return value in a tuple, `TryGetBool(key)` reading `(found, value)` and `PlugInExists(id)` a triple
- .NET arrays reach a collection overload through `method.Overloads[IEnumerable[T]](array)`, pythonnet binding the single-item overload otherwise
- Enum parameters take a member or `<Enum>(<int>)`, pythonnet converting no integer
- .NET members named `None` read as `NONE` (`ShowContentChooserFlags.NONE`, `Option[T].NONE`), `X.None` being a syntax error

## [05]-[COMMANDS]

RhinoCommon comes first where it has an operation, and `command(doc, "<macro>", "<A::B>", ids)` runs the rest:
- Macros run in the active document with visible unlocked `ids` selected and `<A::B>` current
- `get_commands {"filter": "<part>"}` finds a command's English name, registered plugins Rhino has not loaded included
- Hidden plugin commands stay out of `get_commands` and run from macros, `document.assembly` listing them and `Command.IsCommand` reading True
- Listener resource `rhino://commands/<Name>/help` read through the setup reference's resource call returns a built-in command's docs URL
- Macros answer every prompt in order, `_Enter` accepting a default, `!` cancelling a command, `'` nesting one, and `-` taking the scripted form
- Points and numbers answer as typed values in model units (`0,0,0`, `5`), where `_Pause` waits for a viewport pick that holds every later call
- Options take their full `_<Name>=_<Choice>` form, abbreviations depending on the command-line locale
- Selection commands (`_SelDup`, `_SelLast`) add to the `ids` selection, `_SelNone` first giving them an empty one
- `rows` hold objects the macro created or rewrote under their ids, `deleted` ids it removed, `results` each command's English name and `Result`
- `output` holds each `Prompt` the macro met and each line its commands printed in order, another session's call during a prompt adding its own
- Prompts a macro leaves unanswered cancel at its end, `results` reading `Result.Cancel` and `output` ending with that `Prompt`
- Tokens past a macro's last prompt run as commands of their own, `results` listing each
- New objects go on `<A::B>`, dimensions and leaders on the document's dimension layer while `UseDimensionLayer` holds (`Default.3dm`)
- Objects a command derives from inputs (`_Copy` and `_Move` results, boolean results) keep their input's layer

## [06]-[INSPECTION]

Records, saved files, and captures show a document's state:
- Records hold each object's layer and bounding box, and RhinoCommon on its id gives `IsValid`, `IsSolid`, `IsClosed`, length, area, and volume
- `file3dm.py` on a saved or exported `.3dm` shows layers, counts, and views its file holds, `edited` naming the last save
- Captures draw clipping planes as the window does and leave out grid, axes, selection highlight, Gumball, and Grasshopper 2 preview
- Every view keeps its camera, display mode, and overlays through a capture, and the selection keeps its objects

## [07]-[GRASSHOPPER]

Grasshopper 2 starts once per process, and its editor and canvases are state every session shares:
- `g2_start` starts the editor with every library once per process, before any `import canvas`
- `g2_search_components` and `g2_describe_component` start the editor themselves while `Editor.Instance` is `None`
- `PlugIn.LoadPlugIn(PlugIn.IdFromName("Grasshopper2"))` enables `import Grasshopper2` without the editor or its libraries
- Starts reopen every canvas `~/Library/Application Support/Grasshopper2/Session.ghsession` names while `ReinstateSession` holds True
- `Editor.Instance.Visible = False` hides the editor with preview and solves running, `Editor.Instance.Close()` hides it with its canvas inactive

## [08]-[RESULTS]

`run_python` returns one `payload` with `stdout` holding printed records, stderr, and tracebacks on `<run_python>` lines, the call succeeding:
- Other router tools keep their first content block alone, and the setup reference's listener call returns every block
- `payload` of `guidance` alone means the call ran and its result dropped, fix each argument its note names and read state back
- `error` with `message` drops detail blocks, a read-only call repeated through the listener returning each one
- `unexpected` naming `TaskCanceledException` is the router's 300 s limit, its call running on and holding Rhino's UI thread for later calls
- `rhino_crashed` and `rhino_closed` mean Rhino ended mid-call, the setup reference's exit read telling a crash from another session's quit
- `close_failed` from `close_slot` with its slot gone from `list_slots` means the close ran
- Main-conversation calls past 120 s move to a background task, the result in its notification
- Subagent calls wait to the router's limit
