# [TYPING]

- Use `manage-repo` for ignore policy

## [01]-[ANNOTATIONS]

Blender's Python 3.13 evaluates module, class, and signature annotations as their statement runs, and `register_class` resolves every annotation on a registrable class and its bases through `typing.get_type_hints` in the class's module:
- `bpy_struct` and `bpy_prop_collection` raise `TypeError` on a runtime subscript while stubs declare them generic, and stub-only names (`bpy.stub_internal`, `TYPE_CHECKING` imports) stay unbound at runtime, so both reach annotations through a `type` alias, whose value evaluates on first read alone
- Signatures name earlier declarations and aliases, a quote holding a later declaration
- An unresolvable annotation on a `PropertyGroup` or `AddonPreferences` class or its base raises at `register_class`, and an `Operator` registers with none of its `bpy.props` properties, so registrable classes annotate `bpy.props` calls and runtime-bound names alone
- Python 3.13 lacks `annotationlib`, `lazy` imports, and `typing.TypeForm`: `inspect.get_annotations(owner, eval_str=True)` reads annotations, imports stay eager, and schemas take `typing_extensions.TypeForm`
- Blender's main thread flushes subnormal floats to zero while worker threads keep them, `msgspec.Meta(gt=0.0)` accepting `0.0` on the main thread, and positive bounds take `ge=sys.float_info.min` for one answer on every thread

```python
if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import OperatorReturnItems

type Answer = set[OperatorReturnItems]
type Members = bpy.types.bpy_prop_collection[bpy.types.Object]


def smoothed(context: bpy.types.Context, members: Members, /) -> Answer:
    with context.temp_override(selected_editable_objects=members[:]):
        return bpy.ops.object.shade_smooth()
```

`smoothed(context, collection.objects)` imports and runs in Blender, and `Answer` types every `execute`, `invoke`, and `modal` return.

## [02]-[STUB_DEFECTS]

Stub declarations runtime contradicts take one form each, a suppression holding `# type: ignore[<mypy code>]  # ty: ignore[<ty rule>]` with only the codes each checker reports on its line:

| [INDEX] | [STUB]                                           | [RUNTIME]                                  | [FORM]                                                                 |
| :-----: | :----------------------------------------------- | :----------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `bl_rna` as `BlenderRNA`                         | `bpy.types.Struct` with `identifier`       | `bpy.types.Struct` local, `assignment`, `invalid-assignment`           |
|  [02]   | `poll`, `idname_py`, `get_rna_type` on a class   | Members of the `bpy.ops` operator function | `call-arg`, `missing-argument` at the call                             |
|  [03]   | `bpy_prop_array`, `Color` without iteration      | Iterates, indexes, takes `len`             | `np.asarray(<array>)` and unpack                                       |
|  [04]   | `bpy_struct` without `__contains__`              | `key in struct` tests ID properties        | `operator` on the test                                                 |
|  [05]   | `fixed_type` as `Struct`                         | Instance of the described class            | `ID()` class pattern, `unreachable` in its arm                         |
|  [06]   | `Property` read as its subclass                  | Subclass members after a `type` test       | Class pattern on the subclass                                          |
|  [07]   | Never-null members typed optional                | Present                                    | One typed local, `assignment`, `invalid-assignment`                    |
|  [08]   | Engine, color management, `socket_type` literals | Live dynamic enum items                    | `str` local, the write or call under the codes its checkers report     |
|  [09]   | Data and node `new` returning the base class     | Subclass the call names                    | Class pattern, or typed local under `assignment`, `invalid-assignment` |
|  [10]   | Collection `get` without default as `None`       | Item or `None`                             | `<item type> \| None` local                                            |
|  [11]   | `bpy_prop_collection` outside `Sequence`         | Indexes, iterates, takes `len`             | `Iterable[T]` parameter, else the collection's `type` alias            |
|  [12]   | `Menu.append` draw as `draw(context)`            | Blender calls `draw(self, context)`        | Two-parameter draw, `arg-type`, `invalid-argument-type`                |
|  [13]   | `bl_ui` classes on `_bpy_types` bases            | `bpy.types` class subclasses               | `type[bpy.types.Panel]` local, `assignment`, `invalid-assignment`      |
|  [14]   | `gpu` stack contexts as plain classes            | Context managers                           | `ExitStack.enter_context`, `arg-type`, `invalid-argument-type`         |
|  [15]   | `gpu.state.blend_get` as `str`                   | Mode `blend_set` takes                     | `arg-type`, `invalid-argument-type` at the setter                      |
|  [16]   | Array `foreach_set`, batch, `FromPolygons`       | Take numpy arrays                          | `arg-type`, `dict-item`, `invalid-argument-type`                       |
|  [17]   | `PropertyGroup` without `is_registered`          | Flag on every registrable class            | `bool` local, `attr-defined`, `unresolved-attribute`                   |
|  [18]   | `bpy.props` call as class annotation             | Registers the property                     | `valid-type`, `invalid-type-form`                                      |
|  [19]   | `libraries.load` returning `None`                | Yields `(data_from, data_to)`              | Typed local, `func-returns-value`, `assignment`, `invalid-assignment`  |
|  [20]   | `app.handlers.persistent` as `Any`               | Returns the marked function                | `untyped-decorator`, `dynamic-function-decorator-return`               |
|  [21]   | `Operator.bl_options` as `set`                   | Takes a `frozenset`                        | `frozenset`, `assignment`, `invalid-assignment`                        |

Never-null members the stubs type optional:
- `Context.preferences` and `window_manager`, `Scene.view_settings` and `display_settings`, `ViewLayer.active_layer_collection`
- `NodeTree.interface`, node `inputs` and `outputs`, `NodeLink` endpoints, `NodeSocket.links`
- `id_data` on structs an ID owns, `ID.original`, `DepsgraphUpdate.id`
- `KeyMapItem.active`, `keyconfigs.user` and `addon` in background runs included
- `Panel`, `Menu`, `Header`, and `Operator` `layout` inside the `draw` Blender calls, and a popup's `layout` inside its draw function

## [03]-[TRUE_OPTIONALS]

Optionals stubs declare and runtime returns narrow once at read with no suppression:
- `Panel.layout`, `Operator.layout`, and popup `layout` outside a draw
- `Context.area` and `region` in every background run and every GUI timer, msgbus, and `load_post` body, the GUI bodies holding `windows[0]` as `context.window`
- `Context.area` in GUI `depsgraph_update_post` handlers, each holding the last window of `window_manager.windows`
- `Context.window`, `screen`, and `view_layer` in contexts that lack them, `screen` reading `None` at window close
- `Context.active_object` with no active object
- `Depsgraph.scene` and `view_layer` outside evaluation
- `ViewLayer.depsgraph` until that layer evaluates, `evaluated_depsgraph_get()` under `temp_override(view_layer=<layer>)` evaluating another layer
- `id_data` on `Preferences` and its member structs, outside every ID
- `ID.animation_data` before `animation_data_create()`
- `KeyMapItem.properties` and `AssetRepresentation.metadata`
- `ID.copy()` on an uncopyable ID
- `modifiers.new` on an object type its modifier kind rejects
- Unset `PointerProperty` values, `FCurve.driver` on a keyed curve, and `DepsgraphObjectInstance.parent` outside an instance
- `links.new` result, modifier and compositor effect `properties` before a node group
