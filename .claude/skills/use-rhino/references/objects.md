# [OBJECTS]

Layers, objects, and selection go through `document.py` entry points inside `run_python`, RhinoCommon for an operation no entry point holds.

## [01]-[LAYERS]

`Properties(color=, linetype=, print_color=, print_width=, material=, visible=, locked=, strings=)` sets a layer or overrides an object's layer, `None` keeps each attribute:
1. `material(doc, "<name>", "#<RRGGBB>", roughness=, metallic=, opacity=)` adds each material a layer names
2. `layer(doc, "<A::B>", Properties(...))` per layer creates it with its parents or edits it in place

- Linetypes name a document linetype or a Rhino default (Hidden, Dashed, Center), a default the document lacks copies in on first use
- `visible=True` turns the layer and every parent on, `locked=True` locks the layer and every sublayer
- Locked layers stay editable through `layer`, and `add`, `change`, `command`, `load`, `make2d`, and `bake` fault on one, listing unlocked layers
- New layers print in the default pen (`print_width` 0) with `print_color` following `color`
- Refused colors, linetypes, and materials fault together before any write, linetype and material faults listing accepted names

## [02]-[ADDING]

RhinoCommon constructors build geometry and `add` places it on a layer, here parallel paths of model length `length` at offsets `spacing` apart:

```python
# Locked paths on <A::B>
from Rhino.Geometry import LineCurve, Point3d
import document
from records import Properties

doc = __rhino_doc__
paths = [LineCurve(Point3d(0, y, 0), Point3d(length, y, 0)) for y in (0, spacing)]
print(document.add(doc, paths, "<A::B>", Properties(color="#<RRGGBB>", locked=True), name="<name>"))
```

- `add(doc, geometry, "<A::B>", Properties(...), name=, page=)` takes one geometry or a list, `page` naming a layout in page units
- Annotation without `"<A::B>"` goes on the dimension layer, and other geometry or an unset dimension layer returns a `Layer` fault listing layers
- Scripts calling `doc.Objects.Add*` directly set `ObjectAttributes().LayerIndex` from `document.layer_index(doc, "<A::B>")`

## [03]-[FINDING_AND_SELECTING]

Finds leave the user's selection as it is, and a selection for the user replaces it:
1. `find(doc, layer_path=, object_type=, name=, test=, hidden=)` returns the matching records
2. `set_selection {"ids": [...], "slot": "<slot>"}` replaces the user's selection with found ids, locked and hidden ids staying unselected and counted

- `layer_path` matches the layer with its sublayers, and an unknown path returns a `Layer` fault listing every layer
- `object_type` takes `ObjectType` flags joined by `|`, `ObjectType.Brep | ObjectType.Extrusion` for solids with the ones extrusion commands add
- `name` takes `*` and `?` wildcards, `test` takes a function from `RhinoObject` to `bool`
- Locked objects match, `hidden=True` adds hidden objects and objects on hidden layers

## [04]-[CHANGING_AND_MOVING]

`change` edits attributes, and RhinoCommon moves and replaces geometry under the same ids:
1. `change(doc, ids, Properties(...), layer_path=, name=)` sets overrides, layer, and name, `visible=True` and `locked=False` show and unlock objects
2. `doc.Objects.Transform(Guid("<id>"), Transform.<kind>(...), True)` moves an object, locked objects included
3. `doc.Objects.Replace(Guid("<id>"), <geometry>)` swaps geometry and keeps attributes on an unlocked object
4. `read_objects(doc, ids)` returns records after a RhinoCommon edit

- Ids naming no object return a `RhinoObject` fault each
- Calls roll back in a later call through `doc.Objects.Delete(doc.Objects.FindId(Guid("<id>")), True, True)` on the ids they returned, locked and hidden ones included
- `change` restores overrides an earlier record lists, and the user's `_Undo` steps back one whole call
