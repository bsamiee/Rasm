# [OPERATORS]

## [01]-[DECLARATION]

Operator classes declare inputs as `bpy.props` annotations, shared sets sitting on a plain base class that registers them on each subclass. `execute` passes them to one pure function as `**self.as_keywords()`, each property a same-named keyword parameter with its default. `ignore=(<identifier>, ...)` drops properties the function never reads:
- Redo, Repeat Last, script calls, and background `'INVOKE_DEFAULT'` calls run `execute` alone, the last with `self.options.is_invoke` `False`
- `execute` reads its properties and context alone, event and preference values entering through properties `invoke` writes
- `invoke` writes only properties `self.properties.is_property_set(<name>)` reports unset
- Values from a button, keymap, or script skip their gesture or popup
- Script calls with `undo` off neither read nor store last-run values
- `undo=True` calls and interface runs read and store last-run values in `window_manager.operator_properties_last(<idname>)`
- `__init__` overrides take `(self, *args, **kwargs)` and forward both to `super().__init__`, Blender passing its own argument at each run
- `bl_options` takes `BLOCKING` and `GRAB_CURSOR` for cursor-owning gestures and `INTERNAL` to hide from search
- `MODAL_PRIORITY` in `bl_options` runs a modal operator before other modal operators
- `DEPENDS_ON_CURSOR` with `bl_cursor_pending` serves an `invoke` reading the cursor from a menu or button

## [02]-[INVOKE_POPUPS]

`invoke` returns a `WindowManager` popup called on the class (`bpy.types.WindowManager.invoke_props_dialog(self)`). Each takes the options its `bl_rna.functions[<name>].parameters` list:

| [INDEX] | [FUNCTION]            | [BEHAVIOR]                                                                       |
| :-----: | :-------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `invoke_confirm`      | Confirmation with `title`, `message`, `confirm_text`, and `icon`, then `execute` |
|  [02]   | `invoke_props_dialog` | Editable dialog running `execute` on confirm, `cancel_default` for data loss     |
|  [03]   | `invoke_props_popup`  | Popup rerunning `execute` on each edit                                           |
|  [04]   | `invoke_popup`        | Property popup that never runs `execute`                                         |
|  [05]   | `invoke_search_popup` | Search over the enum `bl_property` names                                         |

Popups read operator members:
- `check(self, context)` runs after each property edit, `True` redrawing the layout
- `ExportHelper` operators declare `filename_ext`, its `check` correcting the `filepath` extension

## [03]-[MODAL]

Modal gestures capture targets and prior values in `invoke`, update properties for Redo, and roll back through native modal or cancel callbacks. Context-free resource disposal has its own lifetime:

```python
class Shrink(bpy.types.Operator):
    """Shrink selected objects by cursor position across the region."""

    bl_idname = "object.shrink"
    bl_label = "Shrink"
    bl_options = frozenset({"REGISTER", "UNDO", "BLOCKING"})  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

    factor: bpy.props.FloatProperty(name="Factor", subtype="FACTOR", min=0.0, max=1.0, default=1.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @override
    def invoke(self, context: bpy.types.Context, event: bpy.types.Event) -> Returns:
        if self.properties.is_property_set("factor"):
            return self.execute(context)
        self.before = {obj.session_uid: obj.delta_scale.copy() for obj in context.selected_editable_objects}
        bpy.types.WindowManager.modal_handler_add(self)
        return {"RUNNING_MODAL"}

    @override
    def modal(self, context: bpy.types.Context, event: bpy.types.Event) -> Returns:
        reply: Returns
        match context.area, context.region, event.type, event.value:
            case (None, _, _, _) | (_, None, _, _):
                self.cancel(context)
                return {"CANCELLED"}
            case _, region, "MOUSEMOVE", _:
                self.factor = event.mouse_region_x / region.width
                reply = {"RUNNING_MODAL"}
            case _, _, "LEFTMOUSE", "PRESS":
                reply = {"FINISHED"}
            case _, _, ("RIGHTMOUSE" | "ESC"), "PRESS":
                self.cancel(context)
                return {"CANCELLED"}
            case _:
                return {"RUNNING_MODAL"}
        selected = [obj for obj in bpy.data.objects if obj.session_uid in self.before]
        with context.temp_override(selected_editable_objects=selected):
            self.execute(context)
        return reply

    @override
    def cancel(self, _context: bpy.types.Context) -> None:
        for obj in (obj for obj in bpy.data.objects if obj.session_uid in self.before):
            obj.delta_scale = self.before[obj.session_uid]

    @override
    def execute(self, context: bpy.types.Context) -> Returns:
        for obj in context.selected_editable_objects:
            obj.delta_scale = mathutils.Vector.Fill(3, self.factor)
        return {"FINISHED"}
```

Buttons passing `factor` skip the gesture, and Redo after a drag reruns `execute` at the dragged `factor`:
- Instance attributes persist from `invoke` through every `modal` step, and each Redo builds a fresh instance holding properties alone
- `modal` runs in `invoke`'s area and region, or with `self.options.use_cursor_region` the same-type region under the cursor (a quad view quarter)
- File load and window close call `cancel`; class unregistration removes modal handlers without it
- Area or region teardown clears modal UI context, and a step with absent area or region ends that lifetime
- Every window timer sends `TIMER` to each modal step with no timer on `Event`, and steps read progress from their own `timer.time_duration`
- `event_timer_add(<seconds>, window=context.window)` timers release through `event_timer_remove`
- Steps returning `PASS_THROUGH` let undo and other operators run between steps, and state held across them keys by `session_uid`

Context-free scopes release cursor, timer, progress, and workspace status at finish, native cancellation, and unregistration. Registration holds their `close` methods after acquisition so releases precede class removal. Rollback uses native callback context; saved Contexts and manual calls from unregistration cannot supply it.

## [04]-[CALL_SITES]

Buttons, menus, keymaps, and macros name an operator by `bl_idname`, and calls reach it through `bpy.ops.<category>.<name>`. The registered class sits at `bpy.types.<CATEGORY>_OT_<name>` in place of its Python name:
- `layout.operator_enum(<Class>.bl_idname, <enum property>)` draws a button per item and `operator_menu_enum` a menu
- `description(cls, context, properties)` classmethods return tooltips from button properties
- `bpy.types.Macro` subclasses with `REGISTER` and `UNDO` run `define(<Class>.bl_idname)` steps in order as one undo and redo entry
- `OperatorMacro.properties` from each `define` call holds that step's values
- `layout.context_pointer_set("<member>", <struct>)` hands a struct to the operators a layout places, each reading it as `context.<member>`
