# [GRASSHOPPER]

Grasshopper 2 definitions build, solve, check, and bake through `canvas` in task documents `scriptcontext.sticky` holds outside the editor.

## [01]-[SEQUENCE]

Each step is one `run_python` call on the task's slot unless it names a router tool:
1. `definition = canvas.definition("</abs/definition.ghz>")` opens the file or creates it, and later calls get the same document
2. `definition.Solution.Start()` solves an opened file, `canvas.graph(definition)` and `canvas.image(definition, "<name>")` in the next call show it
3. `g2_search_components` and `canvas.plugins()` list the components the change can use
4. `build`, `wire`, `assign`, `arrange`, `cluster`, and `delete` change the definition, and the call ends with `definition.Solution.Start()`
5. `graph(definition)` in the next call reads `phase` and every output, `image` shows the layout, then `Read` the PNG
6. `bake(__rhino_doc__, definition, "<id>", "<A::B>")` writes the result, `document.capture` shows it
7. `DocumentIO(definition, trackFiles=False, reportErrors=False).Save(definition.File.Path, FileContents.Small)` saves the file
8. `canvas.close(definition)` drops the document with its autosave at task end, unsaved edits discarded

- `definition()` with no path returns the user's current canvas, a path the editor holds returns that editor document
- Task documents record no Grasshopper undo step, stay out of the editor's recent files and session, and preview nothing in Rhino

## [02]-[COMPONENTS]

`g2_search_components` keeps components holding every query word in name, info, chapter, or section, a lone shape word ranking its parameter first:
1. `{"query": "<Chapter>", "category": "<Chapter>", "limit": 300}` lists one chapter
2. `{"query": "<operation> <object>"}` (`sphere radius`, `item index`) narrows to one operation
3. `g2_describe_component {"name": "<Name>"}` reads each port's name, user name, type, access, and requirement
4. Chosen `guid` is the part's `selector`

Grasshopper 2 names operations by pattern:
- Chapters are Curve, Maths, Vector, Data, Surface, Mesh, Params, Display, Transform, Intersect, and Text
- Breps are surfaces (Solid Union, Split With Cutters, Explode Surface, Surface-Surface, Mesh From Surface)
- Constructors read `<Thing> From <Input>` (Box From Corner, Polyline From Points), deconstructors `Dissect <Thing>`
- Lists and trees take Item From Index, Cycle, Range From Steps, Explode Tree, and List From Path
- Points and vectors take Point XYZ and World X, Y, and Z, planes World XY, World XZ, and World YZ
- Spelling is British (colour, centre), `color` matches nothing
- Random values come from a distribution component through a Random engine a seed slider drives
- Preview colour, width, dashes, symbol, and size come from item meta Assign Display·Colour and its family set
- Trig applies a selected trigonometric function and Mesh From Surface meshes any Brep, the single-function components are hidden
- Unit conversions are Evaluate Expression over the unit ratio

## [03]-[BUILD]

`build(definition, doc, groups, wires)` adds groups of parts right of the existing objects and returns each key's canvas id, or every fault with the definition unchanged, here with `sizes` as `(name, value text)` pairs and a height slider from `lower` to `upper`:

```python
# Columns on existing grid points, in feet
import canvas
from canvas import Group, Part, Slider, Wire

definition = canvas.definition("</abs/columns.ghz>")
height = Slider("height", lower, lower, upper, 1, "Height", "{0} ft")
inputs = Group("Inputs", "Gray", (height, Part("size", "<Value List guid>", "Column size", items=sizes)))
columns = Group("Columns", "Teal", (Part("boxes", "<Box From Corner guid>", "Columns"),))
wires = (Wire("<points id>", "Point", "boxes", "Corner Plane"), Wire("size", "", "boxes", "Width"), Wire("size", "", "boxes", "Depth"), Wire("height", "", "boxes", "Height"))
print(canvas.build(definition, __rhino_doc__, (inputs, columns), wires))
definition.Solution.Start()
```

- Groups take a name and an Open Color family, a `GroupObject` fault lists the families
- Parts take a user name stating what their lone output holds, drawn as the port's label, and script parts name outputs by variable
- Wires name a port by name, user name, or index, a repeated port name takes its index, a lone parameter's port takes `""`
- Wire ends name part keys or canvas ids of existing objects, `build` adds a second source to a wired input
- `wire(definition, wires)` replaces each target input's sources, `replace=False` adds one
- Groups holding a component pin `doc`'s unit system and tolerance in place of `RhinoDoc.ActiveDoc`'s
- Slider values are model units of `doc`, `grip` writes the unit on the grip (`"{0} ft"` draws `30.0 ft` and holds 30.0)
- Value list parts take `items` as `(name, value text)` pairs and `selected` as an index
- `values` maps an input to persistent values through Grasshopper 2's conversions
- `modifiers` maps an input to `With<Name>` modifiers (`"Grafting"`, `"Flatten"`, `("Trimming", 1)`) in place of tree components
- `build` and `arrange(definition)` lay groups left to right by flow, parts in columns by depth, `arrange(definition, ids)` a set right of the rest
- Part columns fed past the previous column start below it, so their wires pass clear of the parts they skip
- `cluster(definition, ids)` collapses a convex set into one cluster in its members' groups, embedded in the file
- Clusters take an output per output a part outside the set reads and an input per outside source, named `Input 1` onward
- `delete(definition, ids)` removes objects with their wires

## [04]-[SCRIPT_PARTS]

Parts with `script` build a Python 3 or C# script component, `selector` the Python 3 Script or C# Script guid, and hold logic no component has:
- Sources take the editor's instance form, `RunScript` a method of `Script_Instance`
- Inputs come from the `RunScript` signature and wire by variable name, Python outputs from `return a, b` in `outputs` order
- Python hints convert each value at the call, `list` gives list access, a failed conversion errors its iteration and empties every output
- Unwired inputs arrive as `None`, a `list` input included
- C# outputs come from `ref object` parameters in signature order with `outputs` empty
- `print()` and C# `Print` write the `Console` output one branch per iteration, a raise adds an Error message with `[line:col]`

```python
from canvas import Part

offsets = """import Grasshopper2

class Script_Instance(Grasshopper2.Components.GH_ScriptInstance):
    def RunScript(self, count: int, bay: float):
        return [index * bay for index in range(count + 1)], count * bay
"""
area = """using Grasshopper2.Components;

public class Script_Instance : GH_ScriptInstance
{
    private void RunScript(double span, ref object area)
    {
        area = span * span;
    }
}
"""
python = Part("offsets", "00000000-33f7-4221-9706-35b3266bbc0c", script=offsets, outputs=("offsets", "span"))
csharp = Part("area", "00000000-09d7-48c4-819a-63d618da60c8", script=area)
```

## [05]-[VALUES]

`assign(definition, canvas_id, values, input_name, modifiers)` sets a value source or an unwired input, expires it for the next solve, and returns what it holds:
- `input_name` and `bake`'s `output` take a port's name or user name, `graph` lists ports by name and script ports by variable name
- `None` keeps values or modifiers, `()` clears persistent values, `modifiers` replaces the input's modifiers
- Sliders round to their accuracy and range and return the held number, value lists select and return an item index
- Rhino geometry enters as a copy of `doc.Objects.FindId(id).Geometry`
- Faults name the refusal: `ValueObject` a parse error, `IParameter` a wired input, `ConversionServer` a value no conversion reaches
- `field` inputs (Box From Corner sizes) hold numbers as constant fields, `Data.values` printing each as `ConstantScalarField`
- Unwired inputs solve persistent defaults `g2_describe_component` omits (Plane From XZ gives World XY), `graph` after a solve reads them

## [06]-[CHECKS]

`graph(definition, sample)` reads the solve `phase`, and per object its messages, `disabled`, `bounds`, solve `seconds`, input `sources`, and output trees.

Solves with zero errors pass value faults in silence:
- Typed inputs turn unparsable text into null, `nulls` counts them
- Generic operators join numbers and text as text (Addition of 1 and `"4"` gives `"14"`), values print quoted
- Division by zero gives `inf`, `nonfinite` counts it
- Negative radii give geometry, unwired point inputs give the origin
- Unwired inputs with cleared values empty every output downstream, `items` reads 0
- Disabled objects and everything fed only by them output no tree, `disabled` names the source
- `seconds` names the objects a slow solve spends its time in
- Output `labels` show the first paths a graft or flatten produced
- `strings -n 8` of the newest `~/Library/Application Support/Grasshopper2/Diagnostics/*.ghlog` shows solves, saves, load failures, and faults

`image(definition, "<name>", ids)` draws objects with their groups and wires into `.artifacts/rhino/<name>.png` at up to 2 pixels per canvas unit within `Read`'s 2000 pixels, `detail` the scale:
- Pictures draw in the editor's skin at 100% zoom detail, without the canvas grid or +/− parameter buttons
- Wires of an unsolved definition draw dashed whole, and wires carrying more than one path draw a dashed core
- Scales below 1 shrink text, an `image` of one group's id draws that group at full size

## [07]-[BAKE]

`bake(doc, definition, canvas_id, layer_path, output)` runs Grasshopper 2's own bake for a component or one output as one undo step:
- `layer_path` names every object's layer, created with its parents when absent, color and render material from the layer
- `layer_path=None` bakes as the editor's Bake Default, item meta `Rhino.Layer`, `Rhino.Name`, and `Rhino.Colour` setting layer, name, and color
- Repeat bakes delete the source's earlier bake, from a reopened file too, and list the ids in `deleted`
- Sources with no bakeable data or no solve return an `IBakeAware` fault
- Text dots are the annotation that bakes (Dot From Text, Dot From Meta), Text 3D draws in the preview alone
- Boxes bake as `Extrusion` and circles as `ArcCurve`
- Baked objects hold `G2ObjGuid`, `G2ProcGuid`, and sibling user keys, `GH2WipeBakeData` removes them and `SelBaked` selects by them

## [08]-[FILES]

- `FileContents.Small` is the form Save Small and autosave write, `All` adds undo records, the canvas thumbnail and projection, and a picture of `RhinoDoc.ActiveDoc`'s view
- `SaveCopy(path, FileContents.Small, BackupMethod.NONE)` on the save step's `DocumentIO` writes a copy and keeps the document's own file
- Edits autosave `<name>.ghautosave` beside the `.ghz`, a save or `close` deletes it
- `definition(path)` returns a `PluginRequirement` fault per plugin no loaded library supplies, `value` its id, `accepted` its name and version
- `definition(path)` opens the file once `plugins()` loads each missing library, a library Rhino holds in another build after a quit and relaunch

## [09]-[LIBRARIES]

- Libraries installed before Rhino launched load with the editor, their components answer `g2_search_components` by name
- `plugins()` loads libraries installed since the editor started, listing third-party components by `chapter/section` and failed ones by `reason`
- Package removals run between quit and relaunch, `plugins()` scanning every package folder the running Rhino registered
- Rhino packages add components when they hold a Grasshopper 2 library

Use `plugins.md` for finding and installing packages.
