# [TYPING]

Covers annotation forms under Blender's eager evaluation and stub declarations Blender's runtime contradicts, with the code form each takes.

- Use `manage-repo` for ignore policy

## [01]-[ANNOTATIONS]

Blender's Python 3.13 evaluates module, class, and signature annotations when their statement runs, an unbound name raising `NameError`:
- Repository code holds no `from __future__ import annotations`
- Methods naming their own class annotate with `typing.Self`
- Declarations follow dependency order, every annotation naming an earlier declaration
- `type` alias values and type parameter bounds and defaults evaluate on first read, naming later declarations unquoted
- Quotes hold later declarations and `TYPE_CHECKING` names, and a `bpy.types` subscript sits behind one `type` alias (`type Struct = bpy.types.bpy_struct[object]`)
- `register_class` evaluates every annotation on an ID-property class and each base, quoted `bpy.types` subscripts and `TYPE_CHECKING` names raising
- `msgspec.structs.fields`, `msgspec.inspect`, `convert`, and decoders evaluate quoted record annotations, a `TYPE_CHECKING` name raising `NameError`
- Ruff under the 3.13 declaration flags quotes on local variable annotations (UP037) and `type` alias values (TC008)
- `bpy.types` classes raise `TypeError` on a runtime subscript, while stubs declare them generic

## [02]-[STUB_DEFECTS]

Stub declarations runtime contradicts take the form in `[FORM]`, each checker code there going in the line's one `# type: ignore[<code>]  # ty: ignore[<rule>]` suppression:

| [INDEX] | [STUB]                                      | [RUNTIME]                             | [FORM]                                            |
| :-----: | :------------------------------------------ | :------------------------------------ | :------------------------------------------------ |
|  [01]   | `bl_rna` as `BlenderRNA`                    | `bpy.types.Struct` with `identifier`  | `bpy.types.Struct` local, `assignment`            |
|  [02]   | `bpy.ops.<module>.<name>` as a class        | Operator function with `idname_py()`  | `Operation` from `commands.calls`                 |
|  [03]   | Handler lists as list unions                | Path callbacks for `load_*`, `save_*` | `arg-type` at a `list[Callable[..., None]]` field |
|  [04]   | `bpy_prop_array`, `Color` without iteration | Iterates, indexes, takes `len`        | `np.asarray(image.size)` and unpack               |
|  [05]   | `bpy_struct` without `__contains__`         | `key in struct` tests ID properties   | `operator` on the test                            |
|  [06]   | `PointerProperty.fixed_type` as `Struct`    | Instance of the described class       | `fixed_type=ID()` pattern, `unreachable`          |
|  [07]   | `Menu.append` draw as `draw(context)`       | Blender calls `draw(self, context)`   | Two-parameter draw, `arg-type` at the call        |
|  [08]   | Never-null members typed optional           | Present                               | Read as present, `union-attr`                     |
|  [09]   | Engine and color space enums as literals    | Live engine and OCIO enums            | `str`, written through `written`                  |
|  [10]   | `Vector`, `Matrix` declaring `__set__`      | No descriptor protocol                | `arg-type` where a struct field takes one         |
|  [11]   | `gpu` stack contexts as plain classes       | Context managers                      | `with`, `attr-defined`, `invalid-context-manager` |
|  [12]   | `bpy_prop_array.foreach_set`, batch content | Take flat numpy buffers               | `arg-type`, `dict-item`, `invalid-argument-type`  |
|  [13]   | `msgspec.structs.fields` concrete type      | Accepts a parameterized generic       | `arg-type` on the call                            |
|  [14]   | `bl_ui` classes on `_bpy_types` bases       | Subclasses of the `bpy.types` class   | `type[bpy.types.Panel]`, `arg-type`               |
|  [15]   | Python helpers stubbed `-> None` or absent  | Return values                         | Return from installed `__doc__` or source         |
|  [16]   | Data and node `new` returning base classes  | Subclass the call names               | Class pattern or annotated local, `assignment`    |
|  [17]   | Collections without `new` and `remove`      | Functions its `srna` declares         | `attr-defined`, `unresolved-attribute`            |
|  [18]   | `Property` read as its subclass             | Subclass members after a `type` test  | `attr-defined`, `unresolved-attribute`            |
|  [19]   | Collection `get` without default as `None`  | Item or `None`                        | Annotated `<item type> \| None` local             |
|  [20]   | `gpu.state.blend_get` as `str`              | Mode `blend_set` takes                | `arg-type`, `invalid-argument-type` at the setter |
|  [21]   | `PropertyGroup` without `is_registered`     | Flag on every registrable class       | `union-attr`, `unresolved-attribute`              |
|  [22]   | `catch` over an exception tuple             | Catches each listed class             | `call-overload`, `invalid-argument-type`          |
|  [23]   | `bpy_prop_collection` outside `Sequence`    | Indexes, iterates, takes `len`        | `arg-type` at a `Sequence` parameter              |
|  [24]   | `gpu` stack contexts in `enter_context`     | Context managers                      | `arg-type`, `invalid-argument-type`               |

Never-null members the stubs type optional, `Property.is_never_none` reading False on them:
- `Context.preferences` and `window_manager`, `Scene.view_settings` and `display_settings`, `ViewLayer.active_layer_collection`
- `NodeTree.interface`, node `inputs` and `outputs`, `NodeLink` endpoints, `NodeSocket.links`
- `id_data` on structs an ID owns, `ID.original`, `DepsgraphUpdate.id`
- `KeyMapItem.active`, `keyconfigs.user` and `addon` in background runs included
- `Panel`, `Menu`, `Header`, and `Operator` `layout` inside the `draw` Blender calls, and a popup's `layout` inside its draw function

Class-level msgspec fields take their runtime form:
- `Record.field` checks as the field type while runtime holds a member descriptor
- Field handle parameters annotate `object` and narrow with `case MemberDescriptorType(__name__=<name>)`
- Iteration over every field reads `msgspec.structs.fields(<Record>)`

## [03]-[TRUE_OPTIONALS]

Optionals the stub declares and runtime returns narrow once at the read through `Option.of_optional`, with no suppression:
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
- Unset `PointerProperty` values, `FCurve.driver` on a keyed curve, and `DepsgraphObjectInstance.parent` outside an instance
- `links.new` result, modifier and compositor effect `properties` before a node group
