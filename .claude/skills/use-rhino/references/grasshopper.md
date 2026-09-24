# [GRASSHOPPER]

Definitions build through the `g2_*` tools on the current canvas of the Rhino process, `canvas` reads values, assigns inputs, bakes, adds script components and groups, opens files, and pictures the canvas.

## [01]-[BUILD]

1. `Editor.Instance.Documents.Push(Document.NewActiveDocument(), None)` after `g2_start` opens an empty canvas, the earlier one waits inactive
2. `g2_search_components {"query": "<concept words>"}`, every word must appear in name, info, chapter, or section, its `guid` is the selector
3. `g2_describe_component {"name": "<Name>"}` names each input's selector, `typeName`, and default, an ambiguous name fails
4. `g2_apply_graph` with keyed `sliders`, `components`, and `wires` and `"solve": false` while the graph grows, `placed[]` maps key to id
5. `g2_apply_graph {"sliders": [], "components": [], "wires": []}` solves and returns `diagnostics[]` with id, level, and message
6. `graph()` proves each wired output's values against the intent
7. `Editor.Instance.Documents.Pop(<document>, True)` closes the task's canvas unprompted, unsaved edits included, and restores the earlier one

- `Document` imports from `Grasshopper2.Doc` and `Editor` from `Grasshopper2.UI`, every session's calls target the current canvas
- Search by operation (`Number Sequence`, `Loft From Curves`), an exact name can match a parameter first (`Circle`, not `Circle Radius`)
- Port selectors take `""` for the first port, an index, a name, or a user name, a slider's one port takes `""`
- `wires` in one call join keys placed in that call, a wire to an existing object goes through `g2_connect` with its id
- Solves past 60 seconds, an `Expire()` during a solve, a newer solve, Escape in the editor, and `-GH2 _Solver` off end in phase `Canceled`
- Solves run on a background thread, `Editor.Instance.Canvas.Document.Solution.Start()` starts one and a later call reads it
- Slider specs keep `min <= value <= max` and `decimals` from 0 to 12, a clamped spec or a replaced wire drops `placed[]` for guidance
- Unwired inputs solve with their defaults (World XY plane, unit Z span, capped extrusion)
- `field` inputs take numbers through a wire from a `Number` parameter alone
- Panels, value lists, and toggles are value sources, wiring into them fails
- `Grasshopper2.Parameters.Connections.DisconnectAllInputs(parameter, None)` in a script removes an input's wires

## [02]-[VALUES]

`assign(canvas_id, values, input_name=None)` sets a slider, toggle, value list, Value text, text input, typed parameter, or a component's named input, expires it for the next solve, and returns its `Data`:
- `input_name` and `bake`'s `output` take a port's name or user name, `graph()` lists ports by user name, the name a script's signature gave
- Slider values land at the slider's accuracy and range, `Data.values` shows what the definition sees
- Value lists take the index of the item to select and read back the selected indexes
- Value text that fails to parse returns a `ValueObject` fault with the parse error
- Wired inputs return an `IParameter` fault, a solve reads persistent values only from an input with no wire
- Typed parameters take their own type, another type raises a `TypeError` naming the type held, a `field` input refuses persistent numbers
- Rhino geometry enters as a copy of `doc.Objects.FindId(id).Geometry`

```python
import canvas
print(canvas.assign("<number id>", (2.0, 4.0, 6.0)))
print(canvas.assign("<slider id>", (7.5,)))
print(canvas.graph(3))
```

## [03]-[SCRIPT_COMPONENTS]

`script(Python3Component, title, source, at, outputs)` places a Python 3 or C# (`CSharpComponent`) script component named `title` on the canvas, whose inputs come from the `RunScript` signature, returns its node with input and output names, and a later empty `g2_apply_graph` solves it:
- Sources take the editor's instance form, `RunScript` a method of `Script_Instance`, an unbound `def RunScript` collects inputs and never runs
- Python type hints set input types (`count: int` gives an integer input), outputs come from `return a, b` in the order `outputs` names
- C# outputs are `ref object`, a typed `ref List<T>` fails to compile
- Formulas belong in Evaluate Expression, a script component holds logic no component has
- Canvas search finds script components under `#python` and `#C#`

```python
from ScriptComponents.Components import Python3Component
import canvas
source = """import Grasshopper2

class Script_Instance(Grasshopper2.Components.GH_ScriptInstance):
    def RunScript(self, count: int, step: float):
        values = [index * step for index in range(count)]
        return values, sum(values)
"""
print(canvas.script(Python3Component, "Series", source, (160.0, 300.0), ("values", "total")))
```

## [04]-[LAYOUT]

- `g2_apply_graph` `x` and `y` are an object's center, sliders measure 240 by 22
- Stages sit in columns about 250 apart from `(160, 40)` with inputs stacked on the left, every object right of and below the origin
- `group(name, "<Open Color family>", ids)` wraps one stage in a named, colored group, `graph()` lists its members
- `group(...)` refuses a family Open Color lacks, listing the families, and each id a group cannot hold
- Groups draw stale after a move until `group.Attributes.InvalidateLayout()`, `GroupObject.Expire()` does nothing
- `image(name)` draws groups, wires, and messages to `.artifacts/rhino/<name>.png`, `detail` lists objects left of or above the origin it leaves out

## [05]-[LIBRARIES]

Yak libraries installed before the editor started load with it, and their components answer `g2_search_components` by component name, a library name matches nothing. `plugins()` loads packages installed since the editor started and registered development builds, and lists each plugin's `id` and components by chapter and section. Use the plugins reference for finding and installing packages.

## [06]-[BAKE]

`bake(doc, canvas_id, layer_path, output=None)` runs Grasshopper 2's own bake for a component or one output as one undo step:
- Objects land on `layer_path`, created with its parents when absent, color and render material come from the layer
- Metadata the data carries (`Rhino.Layer`, `Rhino.Name`, `Rhino.Colour`) overrides none of the layer's attributes
- Repeat bakes delete the source's earlier bake, objects the user moved or edited included, and list its ids in `deleted`, copies the user made stay
- Sources that hold no bakeable data or have not solved return an `IBakeAware` fault
- Circles bake as `ArcCurve`
- Baked objects carry `G2ObjGuid`, `G2ProcGuid`, and sibling user keys, `GH2WipeBakeData` removes them and `SelBaked` selects by them

## [07]-[FILES]

`DocumentIO.Save` writes the canvas to a `.ghz`:

```python
# Save the current canvas as a small .ghz, which becomes the canvas file
from Grasshopper2.Doc import DocumentIO, FileContents
from Grasshopper2.UI import Editor
print(DocumentIO(Editor.Instance.Canvas.Document, False, False, False, False).Save("</abs/definition.ghz>", FileContents.Small))
```

- `FileContents.Small` is the form Save Small and autosave write, `Minimal` reopens with the Rhino preview off
- `FileContents.All` adds thumbnails, undo history, and a picture of `RhinoDoc.ActiveDoc`'s view, which can be another session's document
- Saved canvases join the session file `g2_start` reopens until `Documents.Pop` closes them
- `SaveCopy(path, contents, BackupMethod.NONE)` writes a copy and keeps the canvas's own file
- Named documents autosave as `<name>.ghautosave` beside the `.ghz` on canvas edits, a save deletes the autosave
- `open_document("</abs/definition.ghz>")` makes a file the current canvas, ids kept, and returns its nodes unsolved
- `open_document` returns a `DocumentIO` fault when the canvas afterwards holds another file, a path already open shows the canvas in memory
- Files that need a plugin no loaded build supplies return a `PluginRequirement` fault per plugin, `value` its id and `accepted` the version saved
- `yak search guid:<value>` names the package, `yak install` and `plugins()` load it, and `open_document` runs again
- Rhino holding another build of that plugin relaunches through the setup reference before `open_document`
- Clusters embed in the file that holds them, Reference Cluster has an empty handler and links nothing
- Display, throttling, backup, and rule-set changes mark no document modified and reach the `.ghz` with the next edit that does
- `g2_clear_canvas {"confirm": true}` empties the current canvas for every session, a task clears only a canvas it pushed
