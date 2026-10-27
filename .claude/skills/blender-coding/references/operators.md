# [OPERATORS]

## [01]-[DECLARATION]

Operator classes declare properties as `bpy.props` annotations and do their work in `execute`:
- Redo, Repeat Last, and scripts rerun `execute` over its properties, and `execute` reads them alone, preferences and scene settings seeding them in `invoke`
- `invoke` gathers event input into properties, then opens its invoke form or calls `execute`
- `invoke` writes only properties `is_property_set` reports unset, keeping keymap, menu, and script values
- Script `bpy.ops` calls with `undo` off neither read nor store last-run values, while calls with `undo` on and interface runs read and store them
- `bl_property` names the property a search popup or enum operator edits
- `INTERNAL` in `bl_options` hides the operator from search
- Class docstrings supply the description a class without `bl_description` shows, and a `description(cls, context, properties)` classmethod returns one per property values

`execute` translates its work's result once into reports and an answer:

```python
class EngineSet(bpy.types.Operator):
    """Set the render engine."""

    bl_idname = "item.engine_set"
    bl_label = "Set Engine"
    bl_options = frozenset({"REGISTER", "UNDO"})  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

    engine: bpy.props.StringProperty(name="Engine")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @override
    def execute(self, context: bpy.types.Context) -> set["rna_enums.OperatorReturnItems"]:
        render: bpy.types.RenderSettings = context.scene.render  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        if render.engine == self.engine:
            self.report({"INFO"}, f"{self.engine} already set")
            return {"CANCELLED"}
        outcome = catch(setattr, exception=TypeError)(render, "engine", self.engine)
        match outcome.tag:
            case "ok":
                return {"FINISHED"}
            case "error":
                self.report({"ERROR"}, str(outcome.error))
                return {"CANCELLED"}
```

`rna_enums` imports from `bpy.stub_internal` under `TYPE_CHECKING`.

## [02]-[CONDITIONS]

`poll` answers whether the operator runs in a context:
- Each requirement reads one context member, an absent member failing it
- `poll` bodies that raise answer `False` and print their traceback, a raised member read showing as an unavailable operator

## [03]-[INVOKE_FORMS]

`invoke` takes one `WindowManager.invoke_*` call, file browser, or modal form, and window-manager calls take options their `bl_rna.functions[<name>].parameters` state:

| [INDEX] | [FORM]                             | [BEHAVIOR]                                                                  |
| :-----: | :--------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `invoke_confirm`                   | Confirmation with an optional computed message, then the body               |
|  [02]   | `invoke_props_dialog`              | Editable dialog running the body on confirm, `cancel_default` for data loss |
|  [03]   | `invoke_popup`                     | Property popup that never runs the body                                     |
|  [04]   | `invoke_props_popup`               | Redo popup over last call                                                   |
|  [05]   | `invoke_search_popup`              | Enum search over `bl_property`                                              |
|  [06]   | `fileselect_add` on `ImportHelper` | Import browser, chosen files in `filepath`, `directory`, and `files`        |
|  [07]   | `fileselect_add` on `ExportHelper` | Export browser, `check_existing` confirming replacements                    |
|  [08]   | `modal_handler_add`                | Interactive gesture with preview, rollback, timer, and status               |

Dialogs and file browsers read operator members:
- `draw(self, context)` lays out the dialog, the default layout listing every property without `HIDDEN`
- `check(self, context)` runs after property edits, `True` redrawing the layout
- `ExportHelper` operators declare `filename_ext`, its `check` correcting the `filepath` extension, and a declared `filter_glob` property filters either browser

## [04]-[MODAL]

`modal` steps a gesture over the operator's state:
- `invoke` captures values restored on cancel and on a finish that changed nothing
- Timer steps come from `window_manager.event_timer_add(<seconds>, window=<window>)`, removed through `event_timer_remove` at finish
- `Event` holds no timer member, and every window timer delivers `TIMER` to each modal step
- Timer steps advance only when their own timer's `time_duration` advances
- `MODAL_PRIORITY` in `bl_options` handles events ahead of other modal operators in `Window.modal_operators`
- Finish and `cancel` release cursor, status text, header, timer, and grab
- `cancel` restores under a `temp_override` of the invoking window, area, and region
- Undo can run while the operator is modal, and modal state holds `session_uid` keys and values in place of host references

## [05]-[PLACEMENTS]

Buttons, menus, keymaps, and macros place an operator as a value:
- Placements hold `(<operator idname>, ((<property>, <value>), ...))`, the keymap-data form keymaps, tools, gizmos, and buttons share
- `bpy.types.Macro` subclasses run steps added through `define(<operator idname>)` as one undo step and one redo entry
- Layouts hand a struct to a placed operator through `layout.context_pointer_set("<member>", struct)`, and the operator reads that context member
