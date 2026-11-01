# [TYPING]

- Use `manage-repo` for ignore policy

## [01]-[ANNOTATIONS]

Blender's Python 3.13 evaluates module, class, and signature annotations as their statement runs, and `register_class` resolves every annotation on a registrable class and its bases through `typing.get_type_hints` in the class's module:
- Stub-generic `bpy_struct` and `bpy_prop_collection` raise `TypeError` on a runtime subscript, and annotations reach them through a `type` alias
- Stub-only names (`bpy.stub_internal`, `TYPE_CHECKING` imports) stay unbound at runtime, and a `type` alias value evaluates on first read alone
- Signatures name earlier declarations and aliases, a quote holding a later declaration
- Unresolvable annotations raise at `register_class` on a `PropertyGroup` or `AddonPreferences` class or base, and leave an `Operator` with no property
- Registrable classes annotate `bpy.props` calls and runtime-bound names alone
- Python 3.13 reads annotations through `inspect.get_annotations(owner, eval_str=True)`, keeps imports eager, and takes `typing_extensions.TypeForm`
- Blender's main thread flushes subnormal floats to zero where worker threads keep them, and `msgspec.Meta(gt=0.0)` accepts `0.0` on the main thread
- Positive bounds take `ge=sys.float_info.min` for one result on every thread

```python
if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import OperatorReturnItems

type Answer = set[OperatorReturnItems]
type Elements[T] = bpy.types.bpy_prop_collection[T]
```

Signatures over both aliases import and run in Blender, `Answer` typing every `execute`, `invoke`, and `modal` return and `Elements[bpy.types.Object]` a parameter taking `collection.objects`.

## [02]-[STUB_DEFECTS]

Stub declarations runtime contradicts take one form each, a suppression holding `# type: ignore[<mypy code>]  # ty: ignore[<ty rule>]` with only the codes each checker reports on its line. Typed locals and assignments take `assignment` and `invalid-assignment`, arguments `arg-type` and `invalid-argument-type`:

| [INDEX] | [STUB]                                      | [RUNTIME]                      | [FORM]                                                   |
| :-----: | :------------------------------------------ | :----------------------------- | :------------------------------------------------------- |
|  [01]   | `bl_rna` as `BlenderRNA`                    | `bpy.types.Struct`             | `bpy.types.Struct` local                                 |
|  [02]   | `poll`, `idname_py`, `get_rna_type` methods | `bpy.ops` function members     | `call-arg`, `missing-argument` at the call               |
|  [03]   | `bpy_prop_array`, `Color` as non-iterable   | Iterates, indexes, takes `len` | `np.asarray(<array>)` and unpack                         |
|  [04]   | `bpy_struct` without `__contains__`         | `in` tests ID properties       | `operator` on the test                                   |
|  [05]   | `fixed_type` as `Struct`                    | Instance of described class    | `ID()` class pattern, `unreachable` in its arm           |
|  [06]   | `Property` read as its subclass             | Subclass after a `type` test   | Class pattern on the subclass                            |
|  [07]   | Never-null members typed optional           | Present                        | One typed local                                          |
|  [08]   | Dynamic enum values as literals             | Live items                     | `str` local, suppression at the write or call            |
|  [09]   | Data and node `new` as base class           | Subclass the call names        | Class pattern or typed local                             |
|  [10]   | Collection `get(key)` as `None`             | Item or `None`                 | `<item type> \| None` local                              |
|  [11]   | `bpy_prop_collection` outside `Sequence`    | Indexes, iterates, takes `len` | `Iterable[T]` parameter, else `Elements[T]`              |
|  [12]   | `Menu.append` draw as `draw(context)`       | `draw(self, context)`          | Two-parameter draw                                       |
|  [13]   | `bl_ui` classes on `_bpy_types` bases       | `bpy.types` class subclasses   | `type[bpy.types.Panel]` local                            |
|  [14]   | `gpu` stack contexts as plain classes       | Context managers               | `ExitStack.enter_context`                                |
|  [15]   | `gpu.state.blend_get` as `str`              | Mode `blend_set` takes         | Suppression at the setter                                |
|  [16]   | `foreach_set`, batch, `FromPolygons` arrays | Take numpy arrays              | `dict-item` beside argument codes                        |
|  [17]   | `PropertyGroup` without `is_registered`     | Flag on registrable classes    | `bool` local, `attr-defined`, `unresolved-attribute`     |
|  [18]   | `bpy.props` call as class annotation        | Registers the property         | `valid-type`, `invalid-type-form`                        |
|  [19]   | `libraries.load` returning `None`           | Yields `(data_from, data_to)`  | Typed local, `func-returns-value`                        |
|  [20]   | `app.handlers.persistent` as `Any`          | Returns the marked function    | `untyped-decorator`, `dynamic-function-decorator-return` |
|  [21]   | `Operator.bl_options` as `set`              | Takes a `frozenset`            | `frozenset`                                              |

Never-null members the stubs type optional:
- `Context.preferences` and `window_manager`, `Scene.view_settings` and `display_settings`, `ViewLayer.active_layer_collection`
- `Depsgraph.scene` and `view_layer`, `AssetRepresentation.metadata`
- `NodeTree.interface`, concrete node classes' `inputs` and `outputs`, `NodeLink` endpoints, `NodeSocket.links` as an empty tuple on an unlinked socket
- `id_data` on structs an ID owns, `ID.original`, `DepsgraphUpdate.id`
- `KeyMapItem.active`, `keyconfigs.user` and `addon` in background runs included
- `Panel`, `Menu`, `Header`, and `Operator` `layout` inside the `draw` Blender calls, a draw function appended to a `Header` or `Menu` class, and a popup's draw function

## [03]-[TRUE_OPTIONALS]

Optionals stubs declare and runtime returns narrow once at read with no suppression:
- `Operator.layout` in `invoke` and `execute`, and `Panel.layout` once its region draw returns
- `Context.area`, `region`, `space_data`, and `region_data` in every background run and every GUI timer, msgbus, and `load_post` body, the GUI bodies holding `windows[0]` as `context.window`
- Every `Context` window member in `depsgraph_update_post`, whose body reads its `scene` and `depsgraph` parameters: an event loop call holds the last window and any area its editor refresh left, and a window close's call holds no window or screen
- `Context.window` and `screen` under `temp_override(window=<window>)` once `wm.window_close` closes that window
- `ViewLayer.depsgraph` until that layer evaluates, `evaluated_depsgraph_get()` under `temp_override(view_layer=<layer>)` evaluating another layer
- `id_data` on `Preferences` and its member structs, outside every ID
- `KeyMapItem.properties` on a modal keymap's items
- `ID.copy()` on a `WindowManager` and `modifiers.new` on an object type its modifier kind rejects, each returning `None` with no raise
- Unset `PointerProperty` values, `FCurve.driver` on a keyed curve, and `DepsgraphObjectInstance.parent` outside an instance
- `links.new` on a socket of another tree or a link a dynamic-socket node refuses, and `NodesModifier` and `SceneCompositorEffect` `properties` before a node group
