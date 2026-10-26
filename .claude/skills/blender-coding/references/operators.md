# [OPERATORS]

Covers operator classes declared through `blender.bpy.commands.operators.Operator`.

## [01]-[DECLARATION]

`Operator[T, S]` builds the native class from a record type and a body, and its row registers it:
- `form` is the frozen record the properties bind to, `Empty` for none
- `run` takes `Call[T]`
- Redo, Repeat Last, and scripts rerun `execute` over the record alone, and `execute` reads its own properties, preferences and scene settings seeding `invoke` alone
- `invoke` gathers event input through `seed` and the invoke form
- `seed` fills only properties `is_property_set` reports unset, keeping keymap, menu, and script values
- Script `bpy.ops` calls with `undo` off, the `called` default, neither read nor store last-run values, while `undo=True` calls and interface runs read and store them
- `undo=True` pushes one step per `FINISHED` call, and `undo="<label>"` declares `UNDO_GROUPED` without `UNDO`, collapsing consecutive calls under that label into one step
- `primary` names the field a search popup or enum operator edits
- `internal=True` hides the operator from search
- Bodies that act (write data, save a file, open a view) return `Done`, with report entries in `notes`
- Bodies return `Unchanged(reason=...)` when the work set tested before writing is empty or a write helper wrote nothing (`Nothing` from `written`, empty `Applied.written`)

```python
class Scaled(msgspec.Struct, frozen=True, kw_only=True):
    percentage: Annotated[int, msgspec.Meta(ge=1, le=100)] = 100


def rescaled(call: Call[Scaled]) -> BlenderResult[Done | Unchanged]:
    return (
        call.root(bpy.types.Scene)
        .bind(lambda scene: written(scene, "render.resolution_percentage", call.record.percentage))
        .map(lambda prior: Done() if prior.is_some() else Unchanged(reason="Resolution holds requested scale"))
    )


scale = Operator[Scaled](key="scale", title="Scale", description="Set the render resolution scale", form=Scaled, run=rescaled, undo="Scale")
```

## [02]-[CONDITIONS]

`condition` becomes `poll`:
- Leaf `Condition` cases read one requirement each, and absent owners fail it
- `poll` bodies that raise answer `False` and print their traceback, a raised member read showing as an unavailable operator
- Shared preconditions compose as `AllOf`, `AnyOf`, and `Not` values in place of `poll` mixins
- `check` returns every unmet member of an `AllOf`, and `poll_message_set` receives their rendered messages

## [03]-[INVOKE_FORMS]

`invoke` takes one `WindowManager.invoke_*` call, file browser, pick, or modal form, and window-manager calls take options their `bl_rna.functions[<name>].parameters` state:

| [INDEX] | [FORM]                | [BEHAVIOR]                                                                  |
| :-----: | :-------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `invoke_confirm`      | Confirmation with an optional computed message, then the body               |
|  [02]   | `invoke_props_dialog` | Editable dialog running the body on confirm, `cancel_default` for data loss |
|  [03]   | `Suppressible`        | Dialog with "Do not show again" clearing its stored confirmation preference |
|  [04]   | `invoke_popup`        | Property popup that never runs the body                                     |
|  [05]   | `invoke_props_popup`  | Redo popup over the last call                                               |
|  [06]   | `invoke_search_popup` | Enum search over `primary`                                                  |
|  [07]   | `Open`                | Import browser on `ImportHelper`, chosen files in `Call.paths`              |
|  [08]   | `Save`                | Export browser on `ExportHelper`, `destinations` confirming replacements    |
|  [09]   | `Pick`                | Pending cursor until the user picks a location                              |
|  [10]   | `Modal`               | Interactive gesture with preview, rollback, timer, and status               |

`Open` and `Save` derive file extensions from the record:
- Record fields stand in for an exporter's keyword arguments
- `files.interchange.Interchange` registers a file handler over `Open` and `Save` rows with `poll_file_object_drop` as its drop poll
- Dropped paths open a confirmation in place of the browser

## [04]-[MODAL]

`Modal[S]` drives a gesture over a state value:
- `begin` returns the snapshot `restore` rolls back to on cancel or a finishing body's `Unchanged`
- `preview` returns header text for each `Travel`, and `Nothing` clears it
- `tick` steps the state over elapsed seconds on its own window timer, and `Nothing` finishes the run
- `Event` holds no timer member, and every window timer delivers `TIMER` to each modal step
- `tick` steps only when its own timer's `time_duration` advances
- `event` takes raw events in place of the standard modal keymap
- `priority=True` declares `MODAL_PRIORITY`, handling events ahead of other modal operators in `Window.modal_operators`
- `status` draws the status bar from the state
- Cursor, status text, header, timer, and grab release on finish, cancel, and unregister
- `restore` runs under an override of the invoking window, area, and region
- Undo can run while the operator is modal, and the state holds handles and values in place of host references

## [05]-[PLACEMENTS]

Buttons, menus, keymaps, and macros place an operator as a value:
- `Own(command=..., values=record)` places a library operator with a complete record
- `Command` (`Operator` or `Macro`) places itself and sets no properties, and `Stock(operator=bpy.ops..., values=...)` places a stock operator with RNA values
- `Macro(steps=...)` runs placed steps as one undo step and one redo entry
- Layouts hand a struct to a placed operator through `layout.context_pointer_set("<member>", struct)`, and the operator reads that context member
