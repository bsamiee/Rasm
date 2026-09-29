# [GRASSHOPPER]

Grasshopper 2 definitions build, solve, check, and bake through `canvas` in documents the task owns.

## [01]-[DOCUMENTS]

Each task works in its own document behind the current canvas:
1. `definition = canvas.definition("</abs/definition.ghz>")` opens or creates the file's document with its Rhino preview off
2. `canvas.build(definition, __rhino_doc__, groups, wires)` adds the definition's parts
3. `definition.Solution.Start()` starts a solve, `canvas.graph(definition)` in a later call reads each output
4. `canvas.bake(__rhino_doc__, definition, "<id>")` writes the result into `__rhino_doc__`
5. `Editor.Instance.Documents.Pop(definition, True)` closes `definition` at the task's end, unsaved edits included

- `Editor` imports from `Grasshopper2.UI`
- Task documents opened into an editor holding only its empty start document replace it and become the current canvas
- `canvas.definition()` with no path returns the user's current canvas
- `canvas.show(definition)` makes a task document the current canvas with its Rhino preview on
- `g2_solve_canvas` solves the current canvas alone, task documents solve through `Solution.Start()`
- Solves run on a background thread, an `Expire()` during a solve, a newer solve, Escape in the editor, and `-GH2 _Solver` off cancel one
- Object state writes through `Activity`, `Display`, and `Selection` as `ObjectActivity`, `ObjectDisplay`, and `ObjectSelection` members
- `Selected` on an object is read-only
- `Editor.Instance.Canvas.Navigate(RectangleF(x, y, w, h), ValueTuple[Single, Single](Single(<min>), Single(<max>)), Duration.Abrupt)` frames a region
- `RecentFiles.PurgeFile(path)` removes a task file from the recent files list

## [02]-[COMPONENTS]

`g2_search_components` orders each ranking tier alphabetically and cuts it at `limit`:
1. Query the operation and its object (`sphere radius`, `split cutters`, `item index`), a lone shape word returns its parameter first
2. `{"query": "<Chapter>", "category": "<Chapter>", "limit": 300}` lists a whole chapter when no match fits the operation
3. `g2_describe_component {"name": "<Name>"}` reads each port's access and requirement, `typeName` empty on generic geometry ports
4. Chosen match's `guid` is the part's `selector`

Grasshopper 2 names operations by pattern:
- Breps are surfaces (Solid Union, Split With Cutters, Explode Surface, Surface-Surface, Mesh From Surface)
- Constructors read `<Thing> From <Input>` (Sphere From Radius, Polyline From Points), deconstructors read `Dissect <Thing>`
- Lists and trees take Item From Index, Cycle, Range From Steps, Explode Tree, and List From Path
- Points and vectors take Point XYZ and World X, Y, and Z, planes World XY, World XZ, and World YZ
- Spelling is British (colour, centre)
- Random values come from a distribution component through a Random engine a seed slider drives
- Preview colour, width, dashes, symbol, and size come from meta the data holds, set by Assign Display·Colour and its family
- Trig and Mesh From Surface cover single trigonometric functions and Brep meshing, the library hides their own components
- Unit conversions are Evaluate Expression over the unit ratio
- Groups, clusters, value lists, and script components come from `canvas`

## [03]-[BUILD]

`build(definition, doc, groups, wires)` adds a definition as groups of parts and returns each key's canvas id, or every fault with the document unchanged:

```python
# Bay grid in model units
import canvas
from canvas import Group, Part, Slider, Wire

definition = canvas.definition("</abs/definition.ghz>")
groups = (
    Group("Inputs", "Gray", (Slider("bay", 10.0, 30.0, 60.0, 1, "Bay", "{0} ft"),)),
    Group("Grid", "Blue", (Part("grid", "00000000-6c37-45ac-9068-c7a38be9f9ca", "Grid", values={"Y Size": (30.0,)}),)),
)
print(canvas.build(definition, __rhino_doc__, groups, (Wire("bay", "", "grid", "X Size"),)))
```

- Groups lay out left to right in flow order in their Open Color family, their parts in columns by wire depth
- Parts take a user name that says what their output holds, every part another group reads takes one
- `values` maps an input to persistent values, `modifiers` maps an input to `With<Name>` modifiers (`"Grafting"`, `("Trimming", 1)`)
- Modifiers graft, flatten, simplify, and reverse data on the input itself in place of Graft Tree and Flatten Tree components
- Slider values are model units of `doc`, `grip` shows their unit on the grip (`"{0} ft"` draws `30.0 ft` and holds 30.0)
- Value list parts take `items` as `(name, value text)` pairs and `selected` as an index
- Parts reading a group beyond the adjacent column take its data through a Relay part in their own group
- Wires name a port by name, user name, or index, a repeated port name takes its index, a lone parameter's port takes `""`
- Components read units and tolerance from `RhinoDoc.ActiveDoc` unless a pin sets them, build groups pin `doc`'s
- `wire(definition, wires)` joins existing objects by canvas id, `replace=False` adds a second source to an input
- `arrange(definition)` lays out an existing definition by flow, each group one block
- `delete(definition, ids)` removes objects with their wires
- `cluster(definition, ids, "<name>")` collapses a finished group into one cluster with its boundary wires
- `cluster` returns a fault naming the `GraphTopology` of an empty set and of a set a wire path leaves and reenters

## [04]-[SCRIPT_PARTS]

Parts with `script` build a Python 3 or C# script component, `selector` the Python 3 Script or C# Script guid:
- Sources take the editor's instance form, `RunScript` a method of `Script_Instance`, a module-level `RunScript` outputs null with no message
- Inputs come from the `RunScript` signature, Python outputs from `return a, b` in `outputs` order
- Python hints convert each value at the call, `list` gives list access, a failed conversion errors its iteration and empties every output
- Unwired inputs arrive as `None`, a `list` input included
- C# outputs come from `ref object` parameters in signature order with `outputs` empty, a typed `ref List<T>` fails to compile
- `print()` and C# `Print` write the Console output one branch per iteration, a raise adds an Error with `[line:col]`
- Formulas belong in Evaluate Expression, a script component holds logic no component has

```python
from canvas import Part

source = """import Grasshopper2

class Script_Instance(Grasshopper2.Components.GH_ScriptInstance):
    def RunScript(self, count: int, step: float):
        values = [index * step for index in range(count)]
        return values, sum(values)
"""
series = Part("series", "00000000-33f7-4221-9706-35b3266bbc0c", "Series", script=source, outputs=("values", "total"))
```

## [05]-[VALUES]

`assign(definition, canvas_id, values, input_name, modifiers)` sets a value source or a component's named input, expires it for the next solve, and returns its `Data`:
- `input_name` and `bake`'s `output` take a port's name or user name, `graph` lists ports by name and script ports by variable name
- `None` keeps values or modifiers, `()` clears an input's persistent values, `modifiers` replaces an input's modifiers
- Slider values round to their slider's accuracy and range, `Data.values` shows what the definition sees
- Value lists take the item index to select and read back selected indexes
- Value text that fails to parse returns a `ValueObject` fault with the parse error
- Wired inputs return an `IParameter` fault, a solve reads persistent values only from an input with no wire
- Typed ports take values through Grasshopper 2's conversions, text to a meta name or a number included
- Values no conversion reaches return a `ConversionServer` fault naming the port's type
- `field` inputs take numbers from any wired number output and refuse persistent numbers
- Rhino geometry enters as a copy of `doc.Objects.FindId(id).Geometry`
- Unwired inputs solve persistent defaults `g2_describe_component` omits (Plane From XZ gives World XY), `graph` after a solve reads them

```python
import canvas

definition = canvas.definition("</abs/definition.ghz>")
print(canvas.assign(definition, "<number id>", (2.0, 4.0, 6.0)))
print(canvas.assign(definition, "<move id>", None, "Shape", ("Grafting",)))
print(canvas.graph(definition, 3))
```

## [06]-[CHECKS]

`graph(definition, sample)` reads each object's messages, `disabled`, `bounds`, solve `seconds`, input `sources`, and output trees.

Solves with zero errors pass value faults in silence:
- Typed inputs turn unparsable text into null, `nulls` counts them
- Generic operators join numbers and text as text (Addition of 1 and `"4"` gives `"14"`), values print quoted
- Division by zero gives `inf`, `nonfinite` counts it
- Negative radii give geometry, unwired point inputs give the origin
- Disabled objects and everything fed only by them output no tree, `disabled` names the source
- `seconds` names the objects a slow solve spends its time in
- Output `labels` show the first paths a graft or flatten produced

`image(definition, "<name>")` draws every group, wire, and object at 1:1 into `<name>.png` beside `capture`'s pictures, `detail` the count drawn.

## [07]-[BAKE]

`bake(doc, definition, canvas_id, layer_path, output)` runs Grasshopper 2's own bake for a component or one output as one undo step:
- `layer_path` names every object's layer, created with its parents when absent, color and render material from the layer
- `layer_path=None` bakes as the editor's Bake Default, item meta `Rhino.Layer`, `Rhino.Name`, and `Rhino.Colour` setting layer, name, and color
- `Rhino.Layer` names a layer path, created when absent, items without one go on the current layer
- Text dots are the annotation that bakes (Dot From Text, Dot From Meta), dimensions and leaders come from Rhino's drafting on baked geometry
- Text 3D draws in the preview alone, its `Size` a cap height in model units
- Repeat bakes delete the source's earlier bake, user-moved or edited objects included, and list its ids in `deleted`, user-made copies stay
- Sources that hold no bakeable data or have not solved return an `IBakeAware` fault
- Circles bake as `ArcCurve`
- Baked objects hold `G2ObjGuid`, `G2ProcGuid`, and sibling user keys, `GH2WipeBakeData` removes them and `SelBaked` selects by them

## [08]-[FILES]

`DocumentIO.Save` writes a document to its `.ghz`:

```python
# Save a task document to its own .ghz
from Grasshopper2.Doc import DocumentIO, FileContents
import canvas

definition = canvas.definition("</abs/definition.ghz>")
print(DocumentIO(definition, trackFiles=False, reportErrors=False).Save(definition.File.Path, FileContents.Small))
```

- `FileContents.Small` is the form Save Small and autosave write, `Minimal` reopens with Rhino preview off
- `FileContents.All` adds thumbnails, undo history, and a picture of `RhinoDoc.ActiveDoc`'s view, `ActiveDoc` can be another session's document
- Held editor documents join the session file `g2_start` reopens until `Documents.Pop` closes them
- `SaveCopy(path, contents, BackupMethod.NONE)` writes a copy and keeps the document's own file
- Named documents autosave as `<name>.ghautosave` beside their `.ghz` on edits, a save deletes the autosave
- `definition(path)` returns a `PluginRequirement` fault per plugin no loaded build supplies, `value` its id, `accepted` the version saved
- `yak search guid:<value>` names the package, `yak install` and `plugins()` load it, and `definition(path)` runs again
- Rhino holding another plugin build relaunches through the setup reference before `definition(path)`
- Clusters embed in the file that holds them
- Display, throttling, backup, and rule-set changes mark no document modified and reach the `.ghz` with a later edit that does

## [09]-[LIBRARIES]

Libraries come from Yak packages and registered development builds:
- Libraries installed before Rhino launched load with the editor, their components answer `g2_search_components` by component name
- `plugins()` loads libraries installed while Rhino runs
- Failed libraries hold a `FailureKind` in `failure` and Grasshopper 2's message in `reason`
- Rhino packages add components when they hold a Grasshopper 2 library
- Use the plugins reference for finding and installing packages
