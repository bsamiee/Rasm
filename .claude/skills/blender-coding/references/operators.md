# [OPERATORS]

## [01]-[DECLARATION]

Operator classes declare their inputs as `bpy.props` annotations, a set several operators share sitting on a plain base class whose annotations register on each operator deriving it. `execute` hands them to one pure function as `**self.as_keywords()`, each property the keyword parameter of its name with its default, and `ignore=(<identifier>, ...)` takes a tuple of the properties the function never reads:
- Redo, Repeat Last, and script calls run `execute` alone, and a background `'INVOKE_DEFAULT'` call runs `execute` with no `invoke` and `self.options.is_invoke` reading `False`
- `execute` reads its properties and context alone, event and preference values entering through properties `invoke` writes
- `invoke` writes only properties `self.properties.is_property_set(<name>)` reports unset, a value a button, keymap, or script passed skipping its gesture or form
- Script calls with `undo` off neither read nor store last-run values, and `undo=True` calls and interface runs read and store them in `window_manager.operator_properties_last(<idname>)`, the struct a panel draws to set the next run
- `__init__` overrides take `(self, *args, **kwargs)` and forward both to `super().__init__`, Blender passing its own argument at each run
- `bl_options` takes `BLOCKING` and `GRAB_CURSOR` for a gesture owning the cursor, `MODAL_PRIORITY` for events ahead of other modal operators, and `INTERNAL` for no search entry
- `DEPENDS_ON_CURSOR` with `bl_cursor_pending` serves an `invoke` reading the cursor from a menu or button

## [02]-[INVOKE_FORMS]

`invoke` returns one form `WindowManager` holds as a class-level function over the operator (`bpy.types.WindowManager.invoke_props_dialog(self)`), each taking the options its `bl_rna.functions[<name>].parameters` list:

| [INDEX] | [FORM]                | [BEHAVIOR]                                                                       |
| :-----: | :-------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `invoke_confirm`      | Confirmation with `title`, `message`, `confirm_text`, and `icon`, then `execute` |
|  [02]   | `invoke_props_dialog` | Editable dialog running `execute` on confirm, `cancel_default` for data loss     |
|  [03]   | `invoke_props_popup`  | Popup rerunning `execute` on each edit                                           |
|  [04]   | `invoke_popup`        | Property popup that never runs `execute`                                         |
|  [05]   | `invoke_search_popup` | Search over the enum `bl_property` names                                         |

Forms read operator members:
- `check(self, context)` runs after each property edit, `True` redrawing the layout
- `ExportHelper` operators declare `filename_ext`, its `check` correcting the `filepath` extension

## [03]-[MODAL]

Modal gestures write their value into a property and apply it through `execute` for Redo and Repeat Last to replay, and `cancel` restores what `invoke` captured:

```python
class Shrink(bpy.types.Operator):
    """Shrink selected objects by cursor position across the region."""

    bl_idname = "object.shrink"
    bl_label = "Shrink"
    bl_options = frozenset({"REGISTER", "UNDO", "BLOCKING"})  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

    factor: bpy.props.FloatProperty(name="Factor", subtype="FACTOR", min=0.0, max=1.0, default=1.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @override
    def invoke(self, context: bpy.types.Context, event: bpy.types.Event) -> Answer:
        if self.properties.is_property_set("factor"):
            return self.execute(context)
        self.before = {obj.session_uid: obj.delta_scale.copy() for obj in context.selected_editable_objects}
        bpy.types.WindowManager.modal_handler_add(self)
        return {"RUNNING_MODAL"}

    @override
    def modal(self, context: bpy.types.Context, event: bpy.types.Event) -> Answer:
        match event.type, event.value:
            case "MOUSEMOVE", _ if region := context.region:
                self.factor = event.mouse_region_x / region.width
                self.execute(context)
                return {"RUNNING_MODAL"}
            case "LEFTMOUSE", "PRESS":
                return self.execute(context)
            case (("RIGHTMOUSE" | "ESC"), "PRESS"):
                self.cancel(context)
                return {"CANCELLED"}
            case _:
                return {"RUNNING_MODAL"}

    @override
    def cancel(self, context: bpy.types.Context) -> None:
        for obj in context.selected_editable_objects:
            obj.delta_scale = self.before[obj.session_uid]

    @override
    def execute(self, context: bpy.types.Context) -> Answer:
        for obj in context.selected_editable_objects:
            obj.delta_scale = mathutils.Vector.Fill(3, self.factor)
        return {"FINISHED"}
```

Buttons passing `factor` skip the gesture, and Redo after a drag reruns `execute` at the dragged `factor`:
- Instance attributes persist from `invoke` through every `modal` step, and each Redo builds a fresh instance holding properties alone
- `modal` runs under the area and region `invoke` ran in, under the region of that type beneath the cursor inside that area (a quad view quarter) once `invoke` sets `self.options.use_cursor_region`, and Blender calls `cancel` only when a file load or window close ends the gesture
- Every window timer delivers `TIMER` to each modal step with no timer on `Event`, and timer steps derive progress from their own `timer.time_duration`
- `context.window_manager.event_timer_add(<seconds>, window=context.window)` timers, cursor, status text, and header writes reset at finish and in `cancel`, timers through `event_timer_remove`
- Steps returning `PASS_THROUGH` let undo and other operators run between steps, and state held across them keys by `session_uid`

## [04]-[PLACEMENTS]

Buttons, menus, keymaps, and macros name an operator by its class's `bl_idname`, calls reach it through `bpy.ops.<category>.<name>`, and the registered class sits at `bpy.types.<CATEGORY>_OT_<name>` in place of its Python name:
- `layout.operator_enum(<Class>.bl_idname, <enum property>)` draws one button per item and `operator_menu_enum` one menu, and a `description(cls, context, properties)` classmethod returns each button's tooltip from the properties it passes
- `bpy.types.Macro` subclasses declaring `REGISTER` and `UNDO` run each `define(<Class>.bl_idname)` step in order as one undo step and one redo entry, the returned `OperatorMacro.properties` holding that step's values
- `layout.context_pointer_set("<member>", <struct>)` hands a struct to the operators a layout places, each reading it as `context.<member>`
