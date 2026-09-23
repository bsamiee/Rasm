# [GRASSHOPPER]

Definitions build through the `g2_*` tools on the one canvas a Rhino process holds, `canvas` reads values, assigns inputs, bakes, adds script components and groups, opens files, and pictures the canvas.

## [01]-[BUILD]

1. `graph()` reads what the canvas holds, another session or an autosave restore can have work there
2. `g2_search_components {"query": "<concept words>"}`, every word must appear in name, info, chapter, or section, its `guid` is the selector
3. `g2_describe_component {"name": "<Name>"}` names each input's selector, `typeName`, and default, an ambiguous name fails
4. `g2_apply_graph` with keyed `sliders`, `components`, and `wires` and `"solve": false` while the graph grows, `placed[]` maps key to id
5. `g2_apply_graph {"sliders": [], "components": [], "wires": []}` solves and returns `diagnostics[]` with id, level, and message
6. `graph()` proves each wired output's values against the intent

- Search by operation (`Number Sequence`, `Loft From Curves`), an exact name can match a parameter first (`Circle`, not `Circle Radius`)
- Port selectors take `""` for the first port, an index, a name, or a user name, a slider's one port takes `""`
- `wires` in one call join keys placed in that call, a wire to an existing object goes through `g2_connect` with its id
- Solves past 60 seconds end in phase `Canceled`
- Slider specs keep `min <= value <= max` and `decimals` from 0 to 12, a clamped spec or a replaced wire drops `placed[]` for guidance
- Unwired inputs solve with their defaults (World XY plane, unit Z span, capped extrusion)
- `field` inputs take numbers through a wire from a `Number` parameter alone
- Panels, value lists, and toggles are value sources, wiring into them fails
- `Grasshopper2.Parameters.Connections.DisconnectAllInputs(parameter, None)` in a script removes an input's wires

## [02]-[VALUES]

`assign(canvas_id, values, input_name=None)` sets persistent values on a slider, toggle, value list, text input, typed parameter, or a component's named input, expires it for the next solve, and returns its `Data`:
- `input_name` and `bake`'s `output` take a port's name or user name, `graph()` lists ports by user name, the name a script's signature gave
- Slider values land at the slider's accuracy and range, `Data.values` shows what the definition sees
- Typed parameters take their own type, another type raises a `TypeError` naming the type held, a `field` input refuses persistent numbers
- Rhino geometry enters as a copy of `doc.Objects.FindId(id).Geometry`, live references come from the editor's picker alone

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
- `group(name, "<Open Color family>", ids)` wraps one stage in a named, coloured group, `graph()` lists its members
- Groups draw stale after a move until `group.Expire()`
- `image(name)` draws groups, wires, and messages to `.artifacts/rhino/<name>.png`, `detail` lists objects left of or above the origin it leaves out

## [05]-[LIBRARIES]

Yak libraries installed before the editor started load with it, and their components answer `g2_search_components` by component name, a library name matches nothing. `plugins()` loads a package installed since the editor started and lists each plugin's `id` and components by chapter and section. Use the plugins reference for finding and installing packages.

## [06]-[BAKE]

`bake(doc, canvas_id, layer_path, output=None)` runs Grasshopper 2's own bake for a component or one output as one undo step:
- Objects land on `layer_path`, created with its parents when absent, color and render material come from the layer
- Repeat bakes delete the source's earlier bake and list its ids in `deleted`, copies the user made of a bake stay
- Sources that hold no bakeable data return an `IBakeAware` fault
- Circles bake as `ArcCurve`

## [07]-[FILES]

Closing the editor discards unsaved canvas edits without a prompt, a `.ghz` save keeps them:

```python
# Save the current canvas as a small .ghz, which becomes the canvas file
from Grasshopper2.Doc import DocumentIO, FileContents
from Grasshopper2.UI import Editor
print(DocumentIO(Editor.Instance.Canvas.Document, False, False, False, False).Save("</abs/definition.ghz>", FileContents.Minimal))
```

- `FileContents.All` adds thumbnails and undo history
- `open_document("</abs/definition.ghz>")` makes a file the current canvas, ids kept, and returns its nodes unsolved
- Files that need a plugin no loaded library supplies return a `PluginRequirement` fault per plugin, `value` its id and `accepted` the version saved
- `yak search guid:<value>` names the package, `yak install` and `plugins()` load it, and `open_document` runs again
- `g2_clear_canvas {"confirm": true}` empties the canvas every session shares, a canvas of the task's own leaves through `Documents.Pop`
