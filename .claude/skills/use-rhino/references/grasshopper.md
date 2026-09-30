# [GRASSHOPPER]

Grasshopper 2 definitions build, solve, check, and bake through `canvas` in task documents `scriptcontext.sticky` holds outside the editor.

## [01]-[SEQUENCE]

Each step is one `run_python` call on the task's slot unless it names a router tool:
1. `g2_start` once per Rhino process, before the first `import canvas`
2. `definition = canvas.definition("</abs/definition.ghz>")` opens the file or creates it, and later calls get the same document
3. `canvas.graph(definition)` and `canvas.image(definition, "<name>")` show an existing definition, then `Read` the PNG
4. `g2_search_components` and `canvas.plugins()` list the components the change can use
5. `build`, `wire`, `assign`, `arrange`, `cluster`, and `delete` change the definition, `image` shows each layout
6. `definition.Solution.Start()` ends its call, `graph(definition)` in the next call reads `phase` and every output
7. `bake(__rhino_doc__, definition, "<id>", "<A::B>")` writes the result, `document.capture` shows it
8. `DocumentIO(definition, trackFiles=False, reportErrors=False).Save(definition.File.Path, FileContents.Small)` saves the file
9. `canvas.close(definition)` drops the document at task end

- `definition()` with no path returns the user's current canvas, a path the editor holds returns that editor document
- `g2_*` router tools edit the current canvas every session shares, task documents take `canvas` alone
- Task documents record no Grasshopper undo step, stay out of the editor's recent files and session, and preview nothing in Rhino

## [02]-[COMPONENTS]

`g2_search_components` matches every query word inside name, info, chapter, or section, and orders exact names, then names holding every word, then the rest, each tier alphabetical and cut at `limit`:
1. `{"query": "<Chapter>", "category": "<Chapter>", "limit": 300}` lists one chapter: Curve, Maths, Vector, Data, Surface, Mesh, Params, Display, Transform, Intersect, Text
2. `{"query": "<operation> <object>"}` narrows to one operation (`sphere radius`, `split cutters`, `item index`), a lone shape word ranks its parameter first
3. `g2_describe_component {"name": "<Name>"}` reads each port's name, access, and requirement, `typeName` empty on generic geometry ports
4. Chosen `guid` is the part's `selector`
5. `canvas.plugins()` loads libraries installed since Rhino started and lists each third-party library's components by `chapter/section`

Grasshopper 2 names operations by pattern:
- Breps are surfaces (Solid Union, Split With Cutters, Explode Surface, Surface-Surface, Mesh From Surface)
- Constructors read `<Thing> From <Input>` (Sphere From Radius, Polyline From Points), deconstructors `Dissect <Thing>`
- Lists and trees take Item From Index, Cycle, Range From Steps, Explode Tree, and List From Path
- Points and vectors take Point XYZ and World X, Y, and Z, planes World XY, World XZ, and World YZ
- Spelling is British (colour, centre), `color` matches nothing
- Random values come from a distribution component through a Random engine a seed slider drives
- Preview colour, width, dashes, symbol, and size come from item meta Assign Display·Colour and its family set
- Trig applies a selected trigonometric function and Mesh From Surface meshes any Brep, the single-function components are hidden
- Unit conversions are Evaluate Expression over the unit ratio

## [03]-[BUILD]

`build(definition, doc, groups, wires)` adds groups of parts right of the existing objects and returns each key's canvas id, or every fault with the definition unchanged:

```python
# Column grid in model units
import canvas
from canvas import Group, Part, Slider, Wire

definition = canvas.definition("</abs/definition.ghz>")
groups = (
    Group("Inputs", "Gray", (Slider("bay", 10.0, 30.0, 60.0, 1, "Bay", "{0} ft"),)),
    Group("Grid", "Blue", (Part("grid", "<Rectangle Grid guid>", "Grid", values={"Y Size": (30.0,)}),)),
)
print(canvas.build(definition, __rhino_doc__, groups, (Wire("bay", "", "grid", "X Size"),)))
```

- Groups take a name and an Open Color family (Gray, Red, Pink, Grape, Violet, Indigo, Blue, Cyan, Teal, Green, Lime, Yellow, Orange)
- Parts take a user name that says what their output holds
- Groups lay out left to right by flow, parts in columns by wire depth, a picture shows every group label, part, and wire apart
- Wires name a port by name, user name, or index, a repeated port name takes its index, a lone parameter's port takes `""`
- Wire ends name part keys or canvas ids of existing objects, `build` adds a second source to a wired input
- `wire(definition, wires)` replaces each target input's sources, `replace=False` adds one
- Build groups pin `doc`'s unit system and tolerance, unpinned components read `RhinoDoc.ActiveDoc`, another session's document included
- Slider values are model units of `doc`, `grip` writes the unit on the grip (`"{0} ft"` draws `30.0 ft` and holds 30.0)
- Value list parts take `items` as `(name, value text)` pairs and `selected` as an index
- `values` maps an input to persistent values through Grasshopper 2's conversions
- `modifiers` maps an input to `With<Name>` modifiers (`"Grafting"`, `"Flatten"`, `("Trimming", 1)`) in place of tree components
- `arrange(definition)` lays out a whole definition by flow, `arrange(definition, ids)` a set right of the rest
- `cluster(definition, ids, "<name>")` collapses a convex set into one cluster in its members' groups, and embeds it in the file
- `delete(definition, ids)` removes objects with their wires

## [04]-[SCRIPT_PARTS]

Parts with `script` build a Python 3 or C# script component, `selector` the Python 3 Script or C# Script guid, and hold logic no component has:
- Sources take the editor's instance form, `RunScript` a method of `Script_Instance`
- Inputs come from the `RunScript` signature and wire by variable name, Python outputs from `return a, b` in `outputs` order
- Python hints convert each value at the call, `list` gives list access, a failed conversion errors its iteration and empties every output
- Unwired inputs arrive as `None`, a `list` input included
- C# outputs come from `ref object` parameters in signature order with `outputs` empty
- `print()` and C# `Print` write the `out` output one branch per iteration, a raise adds an Error message with `[line:col]`

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

`assign(definition, canvas_id, values, input_name, modifiers)` sets a value source or an unwired input, expires it for the next solve, and returns what it holds:
- `input_name` and `bake`'s `output` take a port's name or user name, `graph` lists ports by name and script ports by variable name
- `None` keeps values or modifiers, `()` clears persistent values, `modifiers` replaces the input's modifiers
- Sliders round to their accuracy and range, value lists take the item index to select, `Data.values` shows what the definition sees
- Rhino geometry enters as a copy of `doc.Objects.FindId(id).Geometry`
- Faults name the refusal: `ValueObject` a parse error, `IParameter` a wired input, `ConversionServer` a value no conversion reaches
- `field` inputs take numbers from a wired number output alone
- Unwired inputs solve persistent defaults `g2_describe_component` omits (Plane From XZ gives World XY), `graph` after a solve reads them

## [06]-[CHECKS]

`graph(definition, sample)` reads the solve `phase`, and per object its messages, `disabled`, `bounds`, solve `seconds`, input `sources`, and output trees.

Solves with zero errors pass value faults in silence:
- Typed inputs turn unparsable text into null, `nulls` counts them
- Generic operators join numbers and text as text (Addition of 1 and `"4"` gives `"14"`), values print quoted
- Division by zero gives `inf`, `nonfinite` counts it
- Negative radii give geometry, unwired point inputs give the origin
- Disabled objects and everything fed only by them output no tree, `disabled` names the source
- `seconds` names the objects a slow solve spends its time in
- Output `labels` show the first paths a graft or flatten produced
- `strings -n 8 "$(ls -t ~/Library/Application\ Support/Grasshopper2/Diagnostics/*.ghlog | head -1)" | tail` shows solve and load faults

`image(definition, "<name>", ids)` draws objects with their groups and wires into `.artifacts/rhino/<name>.png` at up to 2 pixels per canvas unit within `Read`'s 2000 pixels, `detail` the scale:
- Scales below 1 shrink text, an `image` of one group's id draws that group at full size
- Pictures draw in the editor's skin without the canvas grid

## [07]-[BAKE]

`bake(doc, definition, canvas_id, layer_path, output)` runs Grasshopper 2's own bake for a component or one output as one undo step:
- `layer_path` names every object's layer, created with its parents when absent, color and render material from the layer
- `layer_path=None` bakes as the editor's Bake Default, item meta `Rhino.Layer`, `Rhino.Name`, and `Rhino.Colour` setting layer, name, and color
- Repeat bakes delete the source's earlier bake, user-moved or edited objects included, and list their ids in `deleted`
- Sources with no bakeable data or no solve return an `IBakeAware` fault
- Text dots are the annotation that bakes (Dot From Text, Dot From Meta), Text 3D draws in the preview alone
- Circles bake as `ArcCurve`
- Baked objects hold `G2ObjGuid`, `G2ProcGuid`, and sibling user keys, `GH2WipeBakeData` removes them and `SelBaked` selects by them
- Captures leave out the Grasshopper 2 preview, baked objects show in them

## [08]-[FILES]

- `FileContents.Small` is the form Save Small and autosave write, `All` adds thumbnails, undo history, and a picture of `RhinoDoc.ActiveDoc`'s view
- `SaveCopy(path, FileContents.Small, BackupMethod.NONE)` writes a copy and keeps the document's own file
- Edits autosave `<name>.ghautosave` beside the `.ghz`, a save or `close` deletes it
- `definition(path)` returns a `PluginRequirement` fault per plugin no loaded library supplies, `value` its id, `accepted` its name and version
- `yak search guid:<id>` names the package, `yak install` and `canvas.plugins()` load it, then `definition(path)` opens the file
- Rhino holding another build of a library relaunches through the setup reference before `definition(path)`

## [09]-[LIBRARIES]

- Libraries installed before Rhino launched load with the editor, their components answer `g2_search_components` by name
- `plugins()` rows hold each library's `id`, `location`, and components, a failed library its `FailureKind` in `failure` and message in `reason`
- Rhino packages add components when they hold a Grasshopper 2 library
- Use the plugins reference for finding and installing packages
