# [TYPING]

## [01]-[ANNOTATIONS]

Blender's Python 3.13 evaluates module, class, and signature annotations as their statement runs. `register_class` resolves every annotation on a registrable class and its bases through `typing.get_type_hints` in its module:
- Stub-generic `bpy_struct` and `bpy_prop_collection` raise `TypeError` on a runtime subscript, and annotations reach them through a `type` alias
- Stub-only names (`bpy.stub_internal`, `TYPE_CHECKING` imports) stay unbound at runtime, and a `type` alias value evaluates on first read alone
- Signatures name earlier declarations and aliases, a quote holding a later declaration
- Unresolvable annotations raise at `register_class` on `PropertyGroup` and `AddonPreferences` classes or bases, and leave `Operator` with no property
- Registrable classes annotate properties and runtime-bound names alone
- Classes `rasm_bpy.data.Registration` registers take `Annotated[<value type>, <bpy.props definition>]`, which `annotated` hands Blender
- Classes a script registers through `bpy.utils.register_class` (MCP runs, `use-blender` scripts) take the bare `bpy.props` call
- `register_class` registers nothing from an `Annotated` hint and prints no error, and a bare call takes `# ty: ignore[invalid-type-form]`
- Blender's main thread flushes subnormal floats to zero where worker threads keep them, and `msgspec.Meta(gt=0.0)` accepts `0.0` on the main thread
- Positive bounds take `ge=sys.float_info.min` for one result on every thread

```python
if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import OperatorReturnItems

type Returns = set[OperatorReturnItems]
type Elements[T] = bpy.types.bpy_prop_collection[T]
```

Both aliases import and run in Blender, `Returns` typing `execute`, `invoke`, and `modal` returns and `Elements[bpy.types.Object]` a `collection.objects` parameter. `rna_enums` literals type every static host enum value (`OperatorReturnItems`, `EventTypeItems`, `OperatorTypeFlagItems`). mypy narrowing compares each member of a declared union with each narrowed member at every read and class pattern, a cost a large `rna_enums` literal multiplies, and a `match` over such a parameter reads through a local typed as the union its arms test (`Path | bpy.types.ID | str`).

## [02]-[STUB_DEFECTS]

Stub defects take their row's form, and suppressions stay only where a live run proves a stub narrower than runtime with no value satisfying both:

| [INDEX] | [STUB]                                       | [RUNTIME]                      | [FORM]                                                |
| :-----: | :------------------------------------------- | :----------------------------- | :---------------------------------------------------- |
|  [01]   | `bl_rna` as `BlenderRNA`                     | `bpy.types.Struct`             | `.properties` read, `try_downcast` for `base`         |
|  [02]   | `poll`, `idname_py`, `get_rna_type` methods  | `bpy.ops` function members     | `missing-argument` at the call                        |
|  [03]   | `bpy_prop_array`, `Color` as non-iterable    | Iterates, indexes, takes `len` | `np.asarray(<array>)` and unpack                      |
|  [04]   | `fixed_type` as `Struct`                     | Instance of described class    | `ID()` class pattern, `unreachable` in its arm        |
|  [05]   | `Property` read as its subclass              | Subclass after a `type` test   | Class pattern, `Descriptor.of` for a class member     |
|  [06]   | Never-null members typed optional            | Present                        | `root_in` on `Context`, `id_data`, else class pattern |
|  [07]   | Dynamic enum values as literals              | Live items                     | `written` or `assigned` member write                  |
|  [08]   | Data and node `new` as base class            | Subclass the call names        | `typed` in a chain, class pattern, or `isinstance`    |
|  [09]   | Collection `get(key)` as `None`              | Item or `None`                 | `<item type> \| None` local                           |
|  [10]   | `bpy_prop_collection` outside `Sequence`     | Indexes, iterates, takes `len` | `Iterable[T]` parameter, else `Elements[T]`           |
|  [11]   | `Menu.append` draw as `draw(context)`        | `draw(self, context)`          | Two-parameter draw, `invalid-argument-type`           |
|  [12]   | `bl_ui` classes on `_bpy_types` bases        | `bpy.types` class subclasses   | `type[bpy.types.Panel]` local, `invalid-assignment`   |
|  [13]   | `gpu.matrix` stack contexts as plain classes | Context managers               | `push` with a `pop` callback                          |
|  [14]   | `GPUFrameBuffer.bind` returning `None`       | `enter_context` takes it       | `func-returns-value`, `invalid-argument-type`         |
|  [15]   | `gpu.state.blend_get` as `str`               | Mode `blend_set` takes         | `rasm_bpy.ui.Canvas.blended`                          |
|  [16]   | `foreach_set`, batch, `FromPolygons` arrays  | Take numpy arrays              | Arrays as is, `tolist()` into `FromPolygons`          |
|  [17]   | `PropertyGroup` without `is_registered`      | Flag on registrable classes    | `"bl_rna" in vars(<class>)`                           |
|  [18]   | `bpy.props` call as class annotation         | Registers the property         | `Annotated[<value type>, <definition>]`               |
|  [19]   | `libraries.load` returning `None`            | Yields `(data_from, data_to)`  | Local, `func-returns-value`, `invalid-assignment`     |
|  [20]   | `app.handlers.persistent` as `Any`           | Returns the marked function    | `persistent(<function>)` call, function used as is    |
|  [21]   | `bl_options` as instance `set`               | Class member, any set          | `type(<name>, (<class>,), {"bl_options": <set>})`     |
|  [22]   | `ImportHelper.invoke` as `set[str]`          | Either return set              | `incompatible-method-override` on `invoke`            |
|  [23]   | `extensions_image`, `system_resource` absent | Present                        | `missing-attribute` at the read                       |

Never-null members the stubs type optional:
- `Context.preferences` and `window_manager`, `Scene.view_settings` and `display_settings`, `ViewLayer.active_layer_collection`
- `Depsgraph.scene` and `view_layer`, `AssetRepresentation.metadata`
- `Context.evaluated_depsgraph_get()` after successful native evaluation
- `NodeTree.interface`, concrete node classes' `inputs` and `outputs`, `NodeLink` endpoints, `NodeSocket.links` (empty tuple when unlinked)
- `id_data` on structs an ID owns, `ID.original`, `DepsgraphUpdate.id`
- `KeyMapItem.active`, `keyconfigs.user` and `addon` in background runs included
- `Panel`, `Menu`, `Header`, and `Operator` `layout` in Blender-called `draw`, appended `Header` or `Menu` draw functions, or popup draw functions

## [03]-[OPTIONAL_VALUES]

Optionals stubs declare and runtime returns narrow once at read with no suppression:
- `Operator.layout` in `invoke` and `execute`, and `Panel.layout` once its region draw returns
- `Context.area`, `region`, `space_data`, and `region_data` in background runs and GUI timer, msgbus, and `load_post` bodies
- GUI timer, msgbus, and `load_post` bodies hold `windows[0]` as `context.window`
- Every `Context` window member in `depsgraph_update_post`, whose body reads its `scene` and `depsgraph` parameters
- `depsgraph_update_post` context holding last window and any area an editor refresh left in event loop calls, and no window or screen on window close
- `Context.window` and `screen` under `temp_override(window=<window>)` once `wm.window_close` closes that window
- `ViewLayer.depsgraph` before native graph creation
- `id_data` on `Preferences` and its member structs, outside every ID
- `KeyMapItem.properties` on a modal keymap's items
- `ID.copy()` on a `WindowManager` and `modifiers.new` on an object type its modifier kind rejects, each returning `None` with no raise
- Unset `PointerProperty` values, `FCurve.driver` on a keyed curve, and `DepsgraphObjectInstance.parent` outside an instance
- `links.new` across trees or into a refusing dynamic-socket node, and `NodesModifier` and `SceneCompositorEffect` `properties` before a node group
