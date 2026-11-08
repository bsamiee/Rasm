---
name: blender-coding-interface
description: "Use when writing or reviewing Python that builds Blender's interface or draws in editors, covering interface elements and redraw"
---

# [BLENDER_CODING_INTERFACE]

Covers `bpy` code that builds Blender's interface and draws in its editors.

[REFERENCES]:
- [01]-[DRAWING](references/drawing.md): Pixels code draws itself, for what layout elements cannot show

## [01]-[REDRAW]

Blender calls interface methods on region redraw, none for a hidden region:

| [INDEX] | [METHOD]                   | [CALLED]                                                                     |
| :-----: | :------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | Panel `poll`               | Each redraw of its region, panels of other tabs included                     |
|  [02]   | Panel `draw_header`        | Each redraw while its tab shows, closed panels included                      |
|  [03]   | Panel `draw`               | Each redraw of an open panel, closed ones under a search filter              |
|  [04]   | Child panel methods        | Under an open parent alone                                                   |
|  [05]   | Menu `draw`                | Each opening and each `tag_refresh_ui()` of its popup                        |
|  [06]   | Operator button `poll`     | Each rebuild of its layout, with the button's context and `operator_context` |
|  [07]   | List `filter_items`        | Each list redraw over every item, skipped by `COMPACT` lists                 |
|  [08]   | List `draw_item`           | Each list redraw, once per row in the scroll window                          |
|  [09]   | Gizmo group `poll`         | Each redraw of its region                                                    |
|  [10]   | Gizmo group `setup`        | Once per region it links into, kept across editor switches                   |
|  [11]   | Gizmo group `refresh`      | After `setup` and each notifier its region receives, never on a plain redraw |
|  [12]   | Gizmo group `draw_prepare` | Each redraw of its region                                                    |
|  [13]   | Draw handler               | Each redraw of its region type in every editor of its space                  |

Redraws follow the notifiers each kind of write sends:
- Built-in RNA writes (`frame_current`, `location`) from any caller send their member's notifier, redrawing every listening region and gizmo
- Operators add no redraw of their own, `UNDO` on or off
- `bpy.props` writes run a declared `update=` callback and redraw every region of every window
- `bpy.props` writes without `update=` and `id["<key>"]` writes redraw nothing
- `region.tag_redraw()` redraws that region, `area.tag_redraw()` every region of the area
- Blender ignores tags from inside a region's own draw, and continuous redraws run from a timer or modal step
- `tag_refresh_ui()` reruns popup `draw` (`context.region_popup`) and raises `RuntimeError` elsewhere, `tag_redraw()` repainting built layout alone
- `ID.rename()` and `remove` on a registered `CollectionProperty` send no notifier, and code tags regions drawing that data
- `ID.name` writes, `bpy.data.<collection>.remove`, and `batch_remove` redraw every area
- Regions no built-in notifier reaches redraw through a `bpy.msgbus` subscription calling `tag_redraw()`
- Built-in RNA and `update=` writes from a draw handler notify on each draw, equal values included, and redraw without end

Callback bodies run at each redraw over state other code stored:
- `poll` matches a class pattern (`case SpaceNodeEditor(edit_tree=CompositorNodeTree()):`), rejecting `None`, other editors, and editors with no tree
- `poll` tests every precondition gated bodies read, and those bodies read each as present through a typed local
- `draw`, `poll`, and `filter_items` read stored state, Blender refusing writes there to any ID but window manager, screen, and workspace data
- Values a `draw` shows derive from stored state at the read, `property_unset` calling no `update` callback
- Failing operator `poll` grays its button without hiding it, and `draw` hides a button by skipping it

Raises in `draw`, `draw_header`, `draw_prepare`, or a draw handler print a traceback on every redraw and leave the class or handler running. Draw bodies read a fallible value once and draw rows from its success, and the read logs a fault once.

## [02]-[PANELS]

Registration checks panel placement:
- Sidebar (`UI`) panels name an existing tab exactly (`Tool`, `View`, `Item`) or a new one in `bl_category`, an empty one filing them under `Misc`
- `bl_category` on a region without tabs fails registration in 3D View, Image, Node, and Sequencer editors, other spaces ignoring it
- Child panels set `bl_parent_id` and repeat parent `bl_space_type` and `bl_region_type`, a mismatch raising `parent '<id>' for '<id>' not found`
- Child panels draw under their parent whatever their `bl_category`
- `region.active_panel_category = <category>` opens a tab after the region draws its tabs, raising `AttributeError` before
- Panels registered outside `register()` store an empty `bl_owner_id` and pass every workspace add-on filter

`bl_options` flags answer one need each:

| [INDEX] | [NEED]                                   | [FORM]                                                                                 |
| :-----: | :--------------------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | Secondary section starting collapsed     | `DEFAULT_CLOSED`, applied when a region first stores the panel, stored state after     |
|  [02]   | Lead row or region filler with no header | `HIDE_HEADER`, sorting ahead of every headed panel                                     |
|  [03]   | Header row stretching across the panel   | `HEADER_LAYOUT_EXPAND`                                                                 |
|  [04]   | Section per item of an extension's stack | `layout.panel_prop(<item>, <bool property>)`, `INSTANCED` serving Blender stacks alone |

Panels sort by `bl_order`, then registration:
- New types insert after the last type of their region with a `bl_order` at most their own
- Custom Properties panels take `bl_order = 1000` and Animation panels 999, and panels sorting below both take a `bl_order` above 1000

Panel headers hold switches and presets:
- Boolean-gated panels draw their boolean in `draw_header` as `prop(<data>, <property>, text="")` and set `layout.active` from it in `draw`
- `use_property_split` starts `False` in region headers and inherits parent body's value in popover child panel headers
- Header checkboxes in popover child panels set `use_property_split` to `False`
- Preset buttons are a `(bl_ui.utils.PresetPanel, Panel)` class, a `HEADER` panel with its class name as `bl_idname`
- Preset classes set `preset_subdir`, `preset_operator` (`script.execute_preset`), and `preset_add_operator`
- `draw_header_preset` draws preset buttons through `<preset class>.draw_panel_header(self.layout)`

Layout panels fold sections inside a draw:
- `header, body = layout.panel(<idname>, default_closed=<bool>)` returns `body` as `None` while closed, and costly rows go under `body`
- `layout.panel_prop(<data>, <bool property>)` stores open state in that property, and a write to it opens the section
- `layout.panel` calls sit in a panel's own column outside a box or split, layout panels spanning full panel width for backdrop and header clicks
- Region panels, file browser sidebars, redo popups, `invoke_popup`, `invoke_props_dialog`, and button or `keep_open=True` popovers take layout panels
- Menus, `popup_menu`, pies, `window_manager.popover`, and `call_panel(keep_open=False)` raise `RuntimeError` on a layout panel
- Popups keep layout panel open state per operator or panel idname

`wm.call_panel(name=<idname>)` opens any registered panel type as a popup over the current editor, `self.is_popover` reading `True` in its `draw`:
- `keep_open=False` opens a menu-style popover that closes after one button use and draws child panels open
- Child panels of a popover draw as layout panels inside it
- `layout.popover(panel=<idname>)` draws the panel's `draw_header` inline beside its button with `is_popover` `True`
- Popover-only panels register in a `HEADER` region that draws no panels
- `bl_ui_units_x` sets popover width

## [03]-[LAYOUT]

Settings draw a struct's own properties in declaration order with the label, tooltip, unit, and range each `bpy.props` call states. `use_<name>` booleans beside `<name>` draw in the value's row under its heading, dimming the value while off. Collections draw as a list over `<name>_index`, nested groups as a layout panel:

```python
def drawn(layout: bpy.types.UILayout, struct: Struct, /, *, switch: str = "use_{}", index: str = "{}_index", listtype_name: str = UI_UL_list.__name__) -> None:
    rna: bpy.types.Struct = struct.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    inherited: Elements[bpy.types.Property] = rna.base.properties  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    declared = tuple(prop.identifier for prop in rna.properties if prop.identifier not in inherited and not prop.is_hidden)
    companions = frozenset((*map(switch.format, declared), *map(index.format, declared)))
    layout.use_property_split = True
    for name in (name for name in declared if name not in companions):
        match getattr(struct, name):
            case bpy.types.PropertyGroup() as group:
                header, body = layout.panel(name)
                header.label(text=rna.properties[name].name)
                if body:
                    drawn(body, group, switch=switch, index=index, listtype_name=listtype_name)
            case bpy.types.bpy_prop_collection():
                layout.template_list(listtype_name, name, struct, name, struct, index.format(name))
            case _ if switch.format(name) in declared:
                row = layout.row(heading=rna.properties[name].name)
                row.prop(struct, switch.format(name), text="")
                value = row.row()
                value.active = getattr(struct, switch.format(name))
                value.prop(struct, name, text="")
            case _:
                layout.prop(struct, name)
```

`drawn(self.layout, <group>)` draws a panel, `drawn(self.layout, self.properties)` an operator dialog, and `drawn(layout, window_manager.operator_properties_last(<idname>))` the settings an operator's next run takes:
- `bl_rna.base.properties` holds base members (`rna_type`, `name`, `bl_idname`), and each `PropertyGroup` subclass lists its own runtime `name` last
- `options={'HIDDEN'}` takes a property out of the draw and out of Blender's redo panel
- Lone `use_` booleans draw as checkboxes, and `listtype_name` names a custom list class
- Operator properties draw with no keyframe decorator, and group properties on an ID draw one under `use_property_split`
- `layout.prop` on a collection draws a read-only item count, and on a nested group an empty pointer field

Layout needs beyond the declaration take one form each:

| [INDEX] | [NEED]                               | [FORM]                                                                                            |
| :-----: | :----------------------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | Label column at another width        | `layout.split(factor=<f>)`, a label, then a `text=""` field, `property_split_factor` fixed at 0.4 |
|  [02]   | One label over a checkbox group      | `layout.column(heading=<text>)`, drawn in the first unlabeled row's label column                  |
|  [03]   | Borderless field showing keyed state | `prop(<data>, <property>, emboss=False)`, drawn as `NONE_OR_STATUS`                               |
|  [04]   | Formatted text                       | `label_markdown(text=<text>)` with headers, lists, emphasis, code, links, quotes, and rules       |
|  [05]   | Plain text wrapped to the region     | `label_multiline(text=<text>, max_lines=<n>)`, an ellipsis ending a cut last line                 |
|  [06]   | Progress inside a layout             | `progress(factor=<f>, text=<text>)` as a bar, `type='RING'` as a ring                             |
|  [07]   | Row dimmed with its fields editable  | `layout.active = False`, `enabled = False` refusing edits                                         |
|  [08]   | ID pointer with a New button         | `template_ID(<data>, <pointer>, new=<idname>)`, its operator assigning the ID it creates          |
|  [09]   | Curve, ramp, or scope widget         | Template over data an ID owns (node mapping, brush, `SpaceImageEditor.scopes`)                    |
|  [10]   | ID preview                           | `template_preview` for Material, Texture, World, Light, and LineStyle                             |
|  [11]   | Preview of any other ID              | `template_icon(icon_value=layout.icon(<id>), scale=<n>)` after `<id>.preview_ensure()`            |
|  [12]   | Enum of preview icons                | `template_icon_view(<data>, <enum property>, show_labels=True)`                                   |

Icons draw by name or `icon_value`, and previews load on demand:
- Built-in icons go by name (`icon=<name>`), their `icon_value` numbers changing between builds
- `icon_value` takes `<previews>[<name>].icon_id`, a `bpy.app.icons` id, or `layout.icon(<id>)`
- Previews load files through `<previews>.load(<name>, <path>, 'IMAGE')` or pixels through `new(<name>)`, `image_size`, and `image_pixels_float`
- `load` and `new` raise `KeyError` for a name the collection holds
- Preview pixels load on first draw or read of `icon_size`, `image_size`, or a pixel array, file loads capping images at 256 pixels and icons at 32
- Full-resolution icons set `icon_size = image_size` and copy image pixels into the icon buffer
- Preview collection items hold no ID and draw outside ID templates, as `icon_value` or the fourth member of an enum item
- Background runs render no preview pixels, `<id>.preview_ensure()` leaving `image_size` at `(0, 0)` and collection items reading `icon_id` 0
- `Material.preview_render_type` sets the swatch shape (`FLAT`, `SPHERE`, `CUBE`, `HAIR`, `SHADERBALL`, `CLOTH`, `FLUID`)

## [04]-[MENUS_AND_POPUPS]

Interface elements answer one need each:

| [INDEX] | [NEED]                                 | [ELEMENT]                                                                   |
| :-----: | :------------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | Settings shown while an editor is open | Panel in the editor's region                                                |
|  [02]   | Settings a shortcut raises             | Panel through `wm.call_panel(name=<idname>)`                                |
|  [03]   | Up to eight actions around the cursor  | Pie through `wm.call_menu_pie(name=<idname>)`                               |
|  [04]   | Action list                            | Menu through `layout.menu(<idname>)` or `wm.call_menu(name=<idname>)`       |
|  [05]   | Popup built from code                  | `window_manager.popup_menu(<draw>, title=<text>, icon=<icon>)`              |
|  [06]   | Popover built from code                | `window_manager.popover(<draw>, ui_units_x=<n>, keymap=<keymap>)`           |
|  [07]   | Pie built from code                    | `window_manager.popup_menu_pie(<event>, <draw>, title=<text>, icon=<icon>)` |

Code-built popups, popovers, and pies take `(popup, context)` draw functions and open from `invoke` or a button:
- Timer calls of `popup_menu`, `popover`, or `wm.call_panel` draw and show nothing, and a timer's `wm.call_menu` opens
- `popup_menu` crashes a background run
- Dialogs taller than the window clamp to its height and scroll through arrow buttons

Layouts start in an operator context their opener sets, and entries run `invoke` under an `INVOKE_*` context and `execute` alone under `EXEC_*`:

| [INDEX] | [OPENER]                                                              | [CONTEXT]           |
| :-----: | :-------------------------------------------------------------------- | :------------------ |
|  [01]   | Panel                                                                 | `INVOKE_REGION_WIN` |
|  [02]   | Button drawn by `layout.menu`                                         | `INVOKE_REGION_WIN` |
|  [03]   | `wm.call_menu`, `popup_menu`, or `popover` from code or a keymap item | `EXEC_REGION_WIN`   |
|  [04]   | `wm.call_panel`                                                       | `INVOKE_REGION_WIN` |

Menu classes set their own operator context and search:
- Menus with entries that need `invoke` set `layout.operator_context = 'INVOKE_REGION_WIN'` in `draw`
- `_bl_ui_utils.layout.operator_context(layout, <context>)` scopes another context to a block of entries and restores the opener's
- Menus name no space or region, and `bl_options = {'SEARCH_ON_KEY_PRESS'}` opens menu search on a typed key
- Header menus draw through `<Menu>.draw_collapsible(context, layout)`, inline or folded by `area.show_menus`
- `layout.menu_contents(<idname>)` draws a menu's entries inline

Functions appended to a menu, panel, or header draw in their own context:
- Appended functions start from a layout's opening context, restored after each one, and a raise prints its traceback while other functions draw
- Appended functions store their extension as owner
- `workspace.use_filter_by_owner` hides functions of an excluded owner unless the extended class sets `bl_owner_use_filter = False`
- Prepended functions run ahead of their class's `draw` and its `use_property_split`, setting the split they need
- `<Menu>.is_extended()` reports whether a class holds appended functions
- `append` replaces a class's own `draw`, and `vars(cls)` reads that `draw` where `cls.draw` resolves a base's (`Header` declares none)

Right-click entries append to the context menu a click opens:
- Each mode opens its menu (`VIEW3D_MT_object_context_menu`, `VIEW3D_MT_edit_mesh_context_menu`, `NODE_MT_context_menu`, `OUTLINER_MT_context_menu`)
- Paint and sculpt modes open `VIEW3D_PT_*_context_menu` panels
- `UI_MT_button_context_menu` opens on any button and `UI_MT_list_item_context_menu` on a list row
- `context.button_pointer` and `context.button_prop` name the clicked button's data, and raise `AttributeError` outside a button menu

Pie menus place `layout.menu_pie()` items west, east, south, north, northwest, northeast, southwest, then southeast in call order:
- Labels take no slot, `separator()` takes a slot and leaves it empty, and a sub-layout takes one slot
- Keymap items opening a pie on a key with a tap action of its own take `value='CLICK_DRAG'`

## [05]-[LISTS]

`template_list(<list idname>, <list id>, <data>, <collection>, <active data>, <active index property>, rows=<n>)` draws a list:
- Filter text, sort flags, and size persist per region under `<list idname>_<list id>`, and a list id per owner keeps state per owner
- `sort_reverse` and `sort_lock` arguments seed the stored state when Blender creates it
- `UI_UL_list` with an empty list id draws nothing
- Active items are an int index, a pointer selection reaching the list through an `IntProperty` with `get` and `set` over that pointer
- `item_dyntip_propname=<string property>` gives each row the tooltip its item's string property holds
- `draw_item` takes 7 to 10 positional parameters counting `self`, `active_property`, `index`, and `flt_flag` optional
- `draw_item` opens its row with `prop(item, "name", text="", emboss=False)`, a click selecting the row and a double-click or Ctrl-click renaming

Lists without `filter_items` filter by name and sort by name under `use_filter_sort_alpha`, and lists adding a criterion return `(flags, order)` from `filter_items`:
- `flags` holds one Python `int` per item, numpy scalars raising `TypeError`, with `self.bitflag_filter_item` on each shown item
- `self.bitflag_item_never_show` hides an item whatever its filter or invert state, and lower 16 bits hold custom marks
- `order[i]` holds item `i`'s display position, below item count, and Blender applies `use_filter_sort_reverse` after it
- `use_filter_invert` and `use_filter_sort_alpha` act in Blender's built-in filter alone, and `filter_items` reads them
- Empty `flags` or `order` leave items unfiltered or unsorted, and either one at another length prints an error and acts as empty
- `order` beside non-empty `flags` remaps in quadratic time over shown items
- Name sorts return `UI_UL_list.sort_items_by_name(<items>)` under `use_filter_sort_alpha` and an empty `order` otherwise
- `UI_UL_list.filter_items_by_name` matches a case-insensitive glob with `*` implied at both ends and returns an empty list for an empty filter

List classes take a CapWords name and a `_UL_` `bl_idname`, and `filter_items` reads every item through `foreach_get` and returns plain lists where the stubs declare arrays:

```python
class WeightedList(bpy.types.UIList):
    bl_idname = "ITEM_UL_weighted"

    @override
    def draw_item(
        self,
        context: bpy.types.Context | None,
        layout: bpy.types.UILayout,
        data: Struct | None,
        item: Struct | None,
        icon: int | None,
        active_data: Struct,
        active_property: str | None,
        index: int | None = 0,
        flt_flag: int | None = 0,
    ) -> None:
        layout.prop(item, "name", text="", emboss=False)
        layout.prop(item, "weight", text="")

    @override
    def filter_items(self, context: bpy.types.Context | None, data: Struct | None, property: str) -> tuple[list[int], list[int]]:  # type: ignore[override]
        items: Elements[bpy.types.PropertyGroup] = getattr(data, property)
        items.foreach_get("hidden", hidden := np.empty(len(items), np.bool_))
        items.foreach_get("weight", weights := np.empty(len(items), np.float32))
        named = UI_UL_list.filter_items_by_name(self.filter_name, self.bitflag_filter_item, items, reverse=self.use_filter_invert)
        ranks = np.argsort(np.argsort(weights, kind="stable"), kind="stable")
        return np.where(hidden, self.bitflag_item_never_show, named or self.bitflag_filter_item).tolist(), ranks.tolist()
```

`hidden` and `weight` stand for a `BoolProperty` and a `FloatProperty` on the item's group, and sort permutation ranks are display positions. `drawn(layout, <owner>, listtype_name=WeightedList.bl_idname)` draws every owner collection through it.

## [06]-[MODAL_FEEDBACK]

Modal operators show state through their window:
- `context.area.header_text_set(<text>)` writes into an area's visible tool header, else its header, the other header keeping its layout
- `context.workspace.status_text_set(<text>)` writes the status bar's left side, and `None` on either call restores its text
- `status_text_set(<draw>)` with a `(header, context)` function replaces `STATUSBAR_HT_header.draw` for every window until a call with text or `None`
- Status draw functions show key hints through `template_event_from_keymap_item(<item>, text=<label>)` over the operator's modal keymap items
- `window_manager.progress_begin(<min>, <max>)`, `progress_update(<value>)`, and `progress_end()` drive the active window's progress cursor
- Background runs draw no progress cursor
- `context.window.cursor_modal_set(<cursor>)` holds a cursor until `cursor_modal_restore()` or `progress_end()`

## [07]-[GIZMOS]

Gizmo group lifetime follows its options and tools:
- Groups without `PERSISTENT` stay unpolled and undrawn until an active tool names them in `bl_widget` or code ensures them
- `window_manager.gizmo_group_type_ensure(<idname>)` links a group into every region of its type
- `gizmo_group_type_unlink_delayed(<idname>)` unlinks an ensured group, and both calls raise `RuntimeError` for a `PERSISTENT` or unknown group
- `poll` and `setup_keymap(keyconfig)` are classmethods, and `setup`, `refresh`, `draw_prepare`, and `invoke_prepare(context, gizmo)` take `self`
- Targets bound in `setup` stay on the data current at setup
- `use_undo`, off by default, pushes one undo step per gizmo use

Gizmo group `bl_options` flags answer one need each:

| [INDEX] | [NEED]                                     | [OPTION]                                         |
| :-----: | :----------------------------------------- | :----------------------------------------------- |
|  [01]   | Gizmos placed in world space               | `3D`, without it `matrix_basis` in region pixels |
|  [02]   | Size following view zoom                   | `SCALE`, without it a fixed display size         |
|  [03]   | Occlusion behind geometry                  | `DEPTH_3D`                                       |
|  [04]   | Selectable gizmos                          | `SELECT`                                         |
|  [05]   | Group hidden while one of its gizmos drags | `EXCLUDE_MODAL`                                  |
|  [06]   | Every group shown during a drag            | `SHOW_MODAL_ALL`                                 |
|  [07]   | Setup held until the tool operator runs    | `TOOL_INIT`                                      |
|  [08]   | Fallback tool keymap on the group          | `TOOL_FALLBACK_KEYMAP`                           |

Built-in gizmo types take one target or none:

| [INDEX] | [TYPE]                                                                                 | [TARGET]                    |
| :-----: | :------------------------------------------------------------------------------------- | :-------------------------- |
|  [01]   | `GIZMO_GT_arrow_3d`, `GIZMO_GT_dial_3d`                                                | `offset`, `FLOAT` length 1  |
|  [02]   | `GIZMO_GT_move_3d`                                                                     | `offset`, `FLOAT` length 3  |
|  [03]   | `GIZMO_GT_cage_2d`, `GIZMO_GT_cage_3d`                                                 | `matrix`, `FLOAT` length 16 |
|  [04]   | `GIZMO_GT_button_2d`, `GIZMO_GT_primitive_3d`, `GIZMO_GT_blank_3d`, `GIZMO_GT_snap_3d` | None, `target_set_operator` |

Gizmos bind a target in one form per source:

| [INDEX] | [SOURCE]         | [BINDING]                                                                  |
| :-----: | :--------------- | :------------------------------------------------------------------------- |
|  [01]   | RNA property     | `target_set_prop(<target>, <data>, <property>, index=<n>)`                 |
|  [02]   | Computed value   | `target_set_handler(<target>, get=<fn>, set=<fn>, range=<fn>)`             |
|  [03]   | Operator on drag | `target_set_operator(<idname>)`, returning the operator properties to fill |

Gizmo members refuse or forward by kind:
- `target_set_prop` raises `RuntimeError` for an index past target length or a property the target cannot take
- Reads of an unbound target raise `ValueError`
- Style members (`draw_style`, `draw_options`, `transform`) forward to the gizmo's `properties`

Groups take theme colors in `setup`, rebind in `refresh`, and place in `draw_prepare`, each called only while `poll` passes. `bl_idname = __qualname__` states the group's identifier once:

```python
class LightEnergy(bpy.types.GizmoGroup):
    bl_idname = __qualname__
    bl_label = "Light Energy"
    bl_space_type = "VIEW_3D"
    bl_region_type = "WINDOW"
    bl_options = frozenset({"3D", "PERSISTENT"})  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

    @override
    @classmethod
    def poll(cls, context: bpy.types.Context) -> bool:
        match context.object:
            case bpy.types.Object(data=bpy.types.Light()):
                return True
            case _:
                return False

    @override
    def setup(self, context: bpy.types.Context) -> None:
        preferences: bpy.types.Preferences = context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        ui = preferences.themes[0].user_interface
        arrow = self.gizmos.new("GIZMO_GT_arrow_3d")
        arrow.color, arrow.color_highlight = ui.gizmo_primary, ui.gizmo_hi
        arrow.use_draw_value = True

    @override
    def refresh(self, context: bpy.types.Context) -> None:
        light: bpy.types.Object = context.object  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        self.gizmos[0].target_set_prop("offset", light.data, "energy")

    @override
    def draw_prepare(self, context: bpy.types.Context) -> None:
        light: bpy.types.Object = context.object  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        self.gizmos[0].matrix_basis = light.matrix_world.normalized()
```

Custom gizmos declare `bl_target_properties = ({"id": <name>, "type": <type>, "array_length": <n>},)` and draw their own shape:
- `setup` builds the shape through `self.new_custom_shape('TRIS', <vertices>)`
- `draw(context)` and `draw_select(context, select_id)` call `self.draw_custom_shape(<shape>, matrix=<matrix>, select_id=<id>)`
- `modal(context, event, tweak)` reads `SNAP` and `PRECISE` in `tweak`
- `exit(context, cancel)` writes the invoke value back to its target when `cancel` is true, Blender restoring nothing for custom gizmos

## [08]-[TOOLS]

`bpy.utils.register_tool(<tool>, after={<tool idname>}, separator=<bool>, group=<bool>)` registers a `WorkSpaceTool` subclass in place of `register_class`:
- Tools declare `bl_space_type`, `bl_context_mode`, `bl_idname`, and `bl_label`
- Tools take optional `bl_description`, `bl_icon`, `bl_cursor`, `bl_widget`, `bl_widget_properties`, `bl_keymap`, `draw_settings`, and `draw_cursor`
- `bl_context_mode` names a toolbar key (3D View `EDIT_MESH`, Image Editor `UV`, Sequencer `SEQUENCER`, Node Editor `None`) or raises `KeyError`
- `register_tool` raises `Exception` for a taken `bl_idname` or a space with no toolbar, and an `after` matching no tool appends at the end
- `unregister_tool` raises `AttributeError` for a class never registered or already released

Tool members take keymap, gizmo, and header forms the toolbar reads:
- Tuple `bl_keymap` rows `(<operator>, <event keywords>, {"properties": [...]})` create keymap `<prefix> <Mode Title>, <label>` in `keyconfigs.addon`
- Keymap prefixes come from `keymap_prefix` on the space's `ToolSelectPanelHelper` subclass (`VIEW3D_PT_tools_active.keymap_prefix`)
- Registrations finding a tool keymap present keep its items, and `unregister_tool` removes the keymap
- `bl_widget_properties` sets `bl_widget` group settings as a `list` of `(name, value)` pairs, another sequence failing `wm.tool_set_by_id`
- `bl_icon` names a `.dat` geometry icon without suffix, a built-in name (`ops.generic.select_circle`) or an absolute path to the extension's file
- Missing `bl_icon` files draw a blank icon
- `draw_settings(context, layout, tool)` takes no `self` and runs on each tool header redraw
- `tool.operator_properties(<operator idname>)` returns operator values the tool header edits
- `tool.gizmo_group_properties(<group idname>)` returns the group's settings for `draw_settings`
- `draw_cursor(context, tool, xy)` draws in window coordinates
- `wm.tool_set_by_id(name=<idname>, space_type=<space>)` activates a tool, `space_type` naming the space for code outside it

## [09]-[ASSET_SHELVES]

`AssetShelf` subclasses show assets in a region of 3D View, Image Editor, or node editors, and `context.asset` reads the asset an operator acts on:
- `DEFAULT_VISIBLE` in `bl_options` shows a shelf region the first time a shelf polls, regions otherwise starting hidden
- `bl_activate_operator` and `bl_drag_operator` name operators a click and a drag run
- `poll`, `asset_poll(asset)`, `get_active_asset()`, and `draw_context_menu(context, asset, layout)` are classmethods
- `filter_<id type>` booleans drop other ID types before `asset_poll` tests each asset
- `get_active_asset` returns the highlighted asset's reference, and `draw_context_menu` draws an asset's right-click menu
- `wm.call_asset_shelf_popover(name=<idname>)` opens a shelf as a popover

Regions show one shelf each:
- Regions keep their active shelf while its poll passes, else their first remembered shelf, else the first registered type that polls
- `show_region_asset_shelf` turns read-only where no shelf polls, an assignment raising `AttributeError`
- `space.is_property_readonly("show_region_asset_shelf")` reads the read-only state
- `context.asset_shelf` names the shelf under a `temp_override` of an `ASSET_SHELF` region

## [10]-[TRANSLATION]

Blender translates interface text at draw time in the interface language:
- `bl_label`, `bl_description`, property names, and literal `text=` and `report` messages translate with no code
- Operators, panels, and menus set their context in `bl_translation_context`, lists and headers holding none
- `bl_translation_context` defaults to `bpy.app.translations.contexts.operator_default` on operators and `contexts.default` on panels and menus
- Properties take their context through `translation_context=`
- `text_ctxt=<context>` on a layout call selects another context
- Built text translates its template, then formats it, then draws with `translate=False` (`iface_("{} items").format(count)`)
- `pgettext_iface`, imported as `iface_`, follows `use_translate_interface`
- `pgettext_tip`, imported as `tip_`, follows `use_translate_tooltips`
- `pgettext_rpt`, imported as `rpt_`, follows `use_translate_reports`
- `pgettext_data`, imported as `data_`, follows `use_translate_new_dataname`
- `data_` names new data-blocks and group sockets
- `pgettext` ignores every flag
- `pgettext_n(<text>, <context>)`, imported as `n_`, marks table strings for extraction untranslated, and `iface_` translates them at draw
- `UILayout.enum_item_name` and `enum_item_description` return translated item text, and `enum_items[<identifier>].name` holds source text
- `bpy.app.handlers.translation_update_post` runs after each language or translation flag change, and translated text caches rebuild there
- `bpy.props` names default to the identifier as written (`ev_bias`), and every property a draw shows declares its `name`
- `bpy.path.display_name(<identifier>)` misreads acronyms and `use_` booleans as `Ev Bias` and `Use Region`
