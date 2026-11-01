# [OPERATORS]

## [01]-[DECLARATION]

Operator classes declare their inputs as `bpy.props` annotations, and `execute` is the one body every run reaches:
- Redo, Repeat Last, and script calls run `execute` alone, and a background `'INVOKE_DEFAULT'` call runs `execute` with no `invoke`
- `execute` reads its properties and context alone, event and preference values entering through properties `invoke` writes
- `invoke` writes only properties `self.properties.is_property_set(<name>)` reports unset, a value a button, keymap, or script passed skipping its gesture or form
- Script calls with `undo` off neither read nor store last-run values, `undo=True` calls and interface runs read and store them, and `SKIP_SAVE` keeps one property at its default per run
- `poll` reads the context members `execute` acts on, and a raising `poll` prints its traceback and fails the call with `RuntimeError("Operator bpy.ops.<op>.poll() failed, context is incorrect")`
- Class docstrings supply the tooltip, and a `description(cls, context, properties)` classmethod returns one per property values
- `bl_options` takes `BLOCKING` and `GRAB_CURSOR` for a gesture owning the cursor, `DEPENDS_ON_CURSOR` with `bl_cursor_pending` for an `invoke` reading the cursor from a menu or button, `MODAL_PRIORITY` for events ahead of other modal operators, and `INTERNAL` for no search entry

## [02]-[INVOKE_FORMS]

`invoke` returns one `WindowManager` form, each taking the options its `bl_rna.functions[<name>].parameters` list:

| [INDEX] | [FORM]                | [BEHAVIOR]                                                                       |
| :-----: | :-------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `invoke_confirm`      | Confirmation with `title`, `message`, `confirm_text`, and `icon`, then `execute` |
|  [02]   | `invoke_props_dialog` | Editable dialog running `execute` on confirm, `cancel_default` for data loss     |
|  [03]   | `invoke_props_popup`  | Popup rerunning `execute` on each edit                                           |
|  [04]   | `invoke_popup`        | Property popup that never runs `execute`                                         |
|  [05]   | `invoke_search_popup` | Search over the enum `bl_property` names                                         |
|  [06]   | `fileselect_add`      | File browser filling `filepath`, `directory`, and `files`                        |
|  [07]   | `modal_handler_add`   | Gesture stepping `modal` until it answers `FINISHED` or `CANCELLED`              |

Forms read operator members:
- `draw(self, context)` lays out the form, the default layout listing every property without `HIDDEN`
- `check(self, context)` runs after each property edit, `True` redrawing the layout
- `ExportHelper` operators declare `filename_ext`, its `check` correcting the `filepath` extension, and a `filter_glob` property filters either browser

## [03]-[MODAL]

Modal gestures write their value into a property and apply it through `execute`, so Redo and Repeat Last replay the result. Assignment clamps the value to its hard bounds, and `cancel` restores what `invoke` captured:

```python
class Shrink(bpy.types.Operator):
    """Shrink the selected objects by the cursor's place across the region."""

    bl_idname = "object.shrink"
    bl_label = "Shrink"
    bl_options = frozenset({"REGISTER", "UNDO", "BLOCKING"})  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

    factor: bpy.props.FloatProperty(name="Factor", subtype="FACTOR", min=0.0, max=1.0, default=1.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @override
    @classmethod
    def poll(cls, context: bpy.types.Context) -> bool:
        return bool(context.selected_editable_objects)

    @override
    def invoke(self, context: bpy.types.Context, event: bpy.types.Event) -> set["rna_enums.OperatorReturnItems"]:
        if self.properties.is_property_set("factor"):
            return self.execute(context)
        self.before = {obj.session_uid: obj.delta_scale.copy() for obj in context.selected_editable_objects}
        bpy.types.WindowManager.modal_handler_add(self)
        return {"RUNNING_MODAL"}

    @override
    def modal(self, context: bpy.types.Context, event: bpy.types.Event) -> set["rna_enums.OperatorReturnItems"]:
        match event.type, event.value:
            case "MOUSEMOVE", _ if region := context.region:
                self.factor = event.mouse_region_x / region.width
                self.execute(context)
                return {"RUNNING_MODAL"}
            case "LEFTMOUSE", "PRESS":
                return self.execute(context)
            case ("RIGHTMOUSE" | "ESC"), "PRESS":
                self.cancel(context)
                return {"CANCELLED"}
            case _:
                return {"RUNNING_MODAL"}

    @override
    def cancel(self, context: bpy.types.Context) -> None:
        for obj in context.selected_editable_objects:
            obj.delta_scale = self.before[obj.session_uid]

    @override
    def execute(self, context: bpy.types.Context) -> set["rna_enums.OperatorReturnItems"]:
        for obj in context.selected_editable_objects:
            obj.delta_scale = mathutils.Vector.Fill(3, self.factor)
        return {"FINISHED"}
```

A button passing `factor` skips the gesture, and Redo after a drag reruns `execute` at the dragged `factor`:
- Instance attributes persist from `invoke` through every `modal` step, and each Redo builds a fresh instance holding properties alone
- `modal` runs under the area and region `invoke` ran in, and Blender calls `cancel` only when a file load or window close ends the gesture
- Timer steps derive progress from their own `timer.time_duration`, since `Event` names no timer and every window timer delivers `TIMER` to each modal step
- `event_timer_add(<seconds>, window=context.window)` timers leave through `event_timer_remove` at finish and in `cancel`, beside cursor, status text, and header writes
- Steps answering `PASS_THROUGH` let undo and other operators run between steps, and state held across them keys by `session_uid`

## [04]-[PLACEMENTS]

Buttons, menus, keymaps, and macros name an operator by its class's `bl_idname`, and calls reach it through `bpy.ops.<category>.<name>`:
- `layout.operator(<Class>.bl_idname)` returns the `OperatorProperties` a button passes, each write marking that property set
- `bpy.types.Macro` subclasses declaring `REGISTER` and `UNDO` run each `define(<Class>.bl_idname)` step in order as one undo step and one redo entry, the returned `OperatorMacro.properties` holding that step's values
- `layout.context_pointer_set("<member>", <struct>)` hands a struct to the operators a layout places, each reading it as `context.<member>`
