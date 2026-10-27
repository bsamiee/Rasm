---
name: blender-coding-interface
description: "Use when writing or reviewing Python that builds Blender's interface or draws in editors, covering interface elements and redraw"
---

# [BLENDER_CODING_INTERFACE]

Covers `bpy` code that builds Blender's interface and draws in its editors.

- Use `blender-coding` for callback bodies, operator classes, and operator dialogs
- Use `blender-coding-extensions` for acquiring and releasing classes, draw handlers, menu extensions, previews, and translation tables

## [01]-[REDRAW]

Blender calls interface methods when a region redraws, and none for a hidden region:

| [INDEX] | [METHOD]                           | [CALLED]                                                                           |
| :-----: | :--------------------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | Panel `poll`                       | Each redraw of its region, panels of other tabs included                           |
|  [02]   | Panel `draw_header`                | Each redraw while its tab shows, closed panels included                            |
|  [03]   | Panel `draw`                       | Each redraw of an open panel, closed ones under a search filter                    |
|  [04]   | Child panel methods                | Under an open parent alone                                                         |
|  [05]   | Menu `draw`                        | Each opening of the menu and each `tag_refresh_ui()` of its popup                  |
|  [06]   | Operator button `poll`             | Each rebuild of its layout, with the button's context and `operator_context`       |
|  [07]   | List `filter_items`                | Each redraw of the list over the whole collection, skipped by `COMPACT` lists      |
|  [08]   | List `draw_item`                   | Each redraw of the list, once per row in the scroll window                         |
|  [09]   | Gizmo group `setup`                | Once per region it links into, kept across editor switches                         |
|  [10]   | Gizmo group `refresh`              | After `setup` and after each notifier its region receives, never on a plain redraw |
|  [11]   | Gizmo group `poll`, `draw_prepare` | Each redraw of its region                                                          |
|  [12]   | Draw handler                       | Each redraw of its region type in every editor of its space                        |

Redraws follow notifiers, and each kind of write reaches them differently:
- Stock RNA writes (`frame_current`, `location`) send their member's notifier from any caller, redrawing every listening region in every area, gizmos included
- Operators add no redraw of their own, and an `execute` writing a registered property redraws nothing, `UNDO` on or off
- Writes to a `bpy.props` property declared with `update=` run the callback and redraw every region of every window
- Writes to a property declared without `update=` and `id["<key>"]` writes redraw nothing
- `region.tag_redraw()` redraws that region, `area.tag_redraw()` every region of the area
- Blender ignores tags from inside a region's own draw, and continuous redraws run from a timer or modal step
- Popup regions (`context.region_popup`) rerun their `draw` on `tag_refresh_ui()`, `tag_redraw()` repainting the built layout alone
- Regions other than popups raise `RuntimeError` on `tag_refresh_ui()`
- `ID.rename()` and `remove` on a registered `CollectionProperty` send no notifier, and code tags the regions drawing that data
- `ID.name` writes, `bpy.data.<collection>.remove`, and `batch_remove` redraw every area themselves
- Stock RNA changes no notifier carries to the code's region reach it through a `bpy.msgbus` subscription with a notify calling `tag_redraw()`
- Draw handlers name the drawing editor in `context.area` and run with no write guard
- Stock RNA writes and `update=` property writes from a draw handler send a notifier on each draw, an equal value included, and redraw without end

Callback bodies follow from the call table:
- `poll` answers for every context it meets, a class pattern on `context.space_data` (`bpy.types.SpaceNodeEditor(tree_type="CompositorNodeTree")`) rejecting `None` and other editors alike
- `poll` owns every precondition `draw` reads, and `draw` reads them as present, a missing one raising on every redraw
- Raises in `draw`, `draw_header`, `draw_prepare`, or a draw handler print a traceback on every redraw and leave the class or handler running
- `draw`, `poll`, and `filter_items` read stored state, Blender refusing writes there to any ID but window manager, screen, and workspace data
- Summaries derive in the operator or `update` callback that changed their data
- Failing operator `poll` grays a button and never hides it, and `draw` skips a button the layout must hide

## [02]-[PANELS]

Panels follow host rules at registration:
- Classes registered without `bl_idname` (panels, menus, lists) hold no `bl_idname` attribute, and code reads their name from `cls.bl_rna.identifier`
- Sidebar (`UI`) panels name a tab in `bl_category`, an existing tab spelled exactly (`Tool`, `View`, `Item`) or their own, an empty category filing them under `Misc`
- `bl_category` on a region without tabs fails registration in the 3D View, Image, Node, and Sequencer editors, other spaces ignoring it
- Child panels name their parent in `bl_parent_id` and repeat its `bl_space_type` and `bl_region_type`, a mismatch raising `parent '<id>' for '<id>' not found`
- Child panels draw under their parent whatever their `bl_category`
- `region.active_panel_category = <category>` opens a tab after the region draws its tabs, raising `AttributeError` before
- Panels registered outside `register()` store an empty `bl_owner_id` and pass every workspace add-on filter
- Render engines show stock Properties panels by adding their `bl_idname` to the `COMPAT_ENGINES` set of each stock panel holding `BLENDER_RENDER`

`bl_options` flags answer one need each:

| [INDEX] | [NEED]                                   | [FORM]                                                                                 |
| :-----: | :--------------------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | Secondary section starting collapsed     | `DEFAULT_CLOSED`, applied when a region first stores the panel, stored state after     |
|  [02]   | Lead row or region filler with no header | `HIDE_HEADER`, sorting ahead of every headed panel                                     |
|  [03]   | Header row stretching across the panel   | `HEADER_LAYOUT_EXPAND`                                                                 |
|  [04]   | Section per item of an extension's stack | `layout.panel_prop(<item>, <bool property>)`, `INSTANCED` serving Blender stacks alone |

Panels sort by host order:
- `bl_order` sorts the panels of one region ahead of registration order, a new type inserting after the last type of its region with a `bl_order` at most its own
- Custom Properties panels take `bl_order = 1000` and Animation panels 999, and a panel sorting below them takes a `bl_order` above 1000

Panel headers hold switches and presets:
- Boolean-gated panels draw their boolean in `draw_header` as `prop(<data>, <property>, text="")` and set `layout.active` from it in `draw`
- Child panel headers in a popover inherit the parent body's `use_property_split`, and a header checkbox there sets it `False`, region headers starting `False`
- Preset buttons are a `(bl_ui.utils.PresetPanel, bpy.types.Panel)` class setting `preset_subdir`, `preset_operator` (`script.execute_preset`), and `preset_add_operator`, a `HEADER` panel with its class name as `bl_idname`
- `draw_header_preset` draws preset buttons through `<preset class>.draw_panel_header(self.layout)`

Layout panels fold sections inside a draw:
- `header, body = layout.panel(<idname>, default_closed=<bool>)` returns `body` as `None` while closed, and costly rows go under `body`
- `layout.panel_prop(<data>, <bool property>)` stores open state in that property, and a write to it opens the section
- `layout.panel` calls sit in the panel's own column outside a box or split, layout panels spanning the whole panel width for backdrop and header clicks
- Region panels, panel popovers opened from a button or `call_panel(keep_open=True)`, `invoke_props_dialog`, `invoke_popup`, the redo popup, and the file browser sidebar take layout panels
- Menus, `popup_menu`, pies, `window_manager.popover`, and `call_panel(keep_open=False)` raise `RuntimeError` on a layout panel, and popups keep open state per operator or panel idname

`wm.call_panel(name=<idname>)` opens any registered panel type as a popup over the current editor, `self.is_popover` reading `True` in its `draw`:
- `keep_open=False` opens a menu-style popover that closes after one button use and draws child panels open
- Child panels of a popover draw as layout panels inside it
- `layout.popover(panel=<idname>)` draws the panel's `draw_header` inline beside its button with `is_popover` `True`
- Popover-only panels register in a `HEADER` region that draws no panels
- `bl_ui_units_x` sets popover width

## [03]-[LAYOUT]

Layout needs take one form each:

| [INDEX] | [NEED]                               | [FORM]                                                                                            |
| :-----: | :----------------------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | Label column at another width        | `layout.split(factor=<f>)`, a label, then a `text=""` field, `property_split_factor` fixed at 0.4 |
|  [02]   | One label over a checkbox group      | `layout.column(heading=<text>)`, landing in the label column of the first unlabeled row           |
|  [03]   | Borderless field showing keyed state | `prop(<data>, <property>, emboss=False)`, drawn as `NONE_OR_STATUS`                               |
|  [04]   | Formatted text                       | `label_markdown(text=<text>)` with headers, lists, emphasis, code, links, quotes, and rules       |
|  [05]   | ID pointer with a New button         | `template_ID(<data>, <pointer>, new=<idname>)`, the operator assigning the ID it creates          |
|  [06]   | Enum item label                      | `bpy.types.UILayout.enum_item_name(<data>, <property>, <identifier>)` in interface language       |
|  [07]   | Enum item label as source text       | `enum_items[<identifier>].name`                                                                   |
|  [08]   | Curve, ramp, or scope widget         | Template over data an ID owns (node mapping, brush, `SpaceImageEditor.scopes`)                    |
|  [09]   | ID preview                           | `template_preview` for Material, Texture, World, Light, and LineStyle                             |
|  [10]   | Preview of any other ID              | `template_icon(icon_value=layout.icon(<id>), scale=<n>)` after `<id>.preview_ensure()`            |
|  [11]   | Enum of preview icons                | `template_icon_view(<data>, <enum property>, show_labels=True)`                                   |

Icons and previews follow host rules:
- Built-in icons go by name (`icon=<name>`), their `icon_value` numbers moving between builds
- `icon_value` comes from a preview collection, `bpy.app.icons`, or `layout.icon(<id>)`
- Previews load from files through `<previews>.load(<name>, <path>, 'IMAGE')` or from pixels through `<previews>.new(<name>)`, `image_size`, and `image_pixels_float`
- `load` and `new` raise `KeyError` for a name the collection holds
- Preview pixels load on first draw or first read of `icon_size`, `image_size`, or a pixel array, image loads capping the image at 256 pixels and the icon at 32
- `<id>.preview_ensure()` renders no pixels in background runs, and `image_size` reads `(0, 0)`
- `Material.preview_render_type` sets the swatch shape (`FLAT`, `SPHERE`, `CUBE`, `HAIR`, `SHADERBALL`, `CLOTH`, `FLUID`)
- Full-resolution icons set `icon_size = image_size` and copy the image pixels into the icon buffer
- Preview collection items hold no ID, and draw through `icon_value=<previews>[<name>].icon_id` or as the fourth member of an enum item, outside ID templates
- `icon_id` reads 0 in a background run

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

Popups follow host rules:
- Draw functions of code-built popups, popovers, and pies take `(popup, context)`
- Code-built popups open from `invoke` or a button
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

Menus follow host rules:
- Menus with entries that need `invoke` set `layout.operator_context = 'INVOKE_REGION_WIN'` in `draw`
- Menus name no space or region, and `bl_options = {'SEARCH_ON_KEY_PRESS'}` opens menu search on a typed key
- Header menus draw through `<Menu>.draw_collapsible(context, layout)`, inline or folded by `area.show_menus`
- `layout.menu_contents(<idname>)` draws a menu's entries inline

Functions appended to a menu, panel, or header draw by host rules:
- Appended functions start from the layout's opening context, restored after each one, and a raise prints its traceback while the others draw
- Appended functions store the extension as owner and hide while `workspace.use_filter_by_owner` excludes it, unless the class sets `bl_owner_use_filter = False`
- Prepended functions run ahead of the class's own `draw` and its `use_property_split`, setting the split they need
- `<Menu>.is_extended()` reports whether a class holds appended functions

Right-click entries append to the menu the click opens:
- Editors open their mode's context menu (`VIEW3D_MT_object_context_menu`, `VIEW3D_MT_edit_mesh_context_menu`, `NODE_MT_context_menu`, `OUTLINER_MT_context_menu`)
- Paint and sculpt modes open `VIEW3D_PT_*_context_menu` panels
- `UI_MT_button_context_menu` opens on any button and `UI_MT_list_item_context_menu` on a list row
- `context.button_pointer` and `context.button_prop` name the clicked button's data, and raise `AttributeError` outside a button menu

Pie menus place `layout.menu_pie()` items west, east, south, north, northwest, northeast, southwest, then southeast in call order:
- Labels take no slot, `separator()` takes a slot and leaves it empty, and a sub-layout takes one slot
- Keymap items opening a pie on a key with a tap action of its own take `value='CLICK_DRAG'`

## [05]-[LISTS]

`template_list(<list class>, <list id>, <data>, <collection>, <active data>, <active index property>, rows=<n>)` draws a list:
- Filter text, sort flags, and size persist per region under `<list class>_<list id>`, and one class keyed per owner keeps state per owner
- `sort_reverse` and `sort_lock` arguments seed the stored state when Blender creates it
- `UI_UL_list` with an empty list id draws nothing
- `type` takes `DEFAULT` or `COMPACT`
- Active items are an int index, and a pointer selection reaches the list through an `IntProperty` with `get` and `set` over the pointer
- `item_dyntip_propname=<string property>` gives each row the tooltip a string property of its item holds
- `draw_item` declares 7 to 10 positional parameters counting `self`, `active_property`, `index`, and `flt_flag` optional, another arity raising `ValueError` at `register_class`
- `draw_item` opens its row with `prop(item, "name", text="", emboss=False)`, a click selecting the row and a double-click or Ctrl-click renaming

Lists without `filter_items` filter by name and sort by name under `use_filter_sort_alpha` in Blender's own code, and a list adding a criterion of its own returns `(flags, order)` from `filter_items`:
- `flags` holds one Python `int` per item, numpy scalars raising `TypeError`, with `self.bitflag_filter_item` on each shown item
- `self.bitflag_item_never_show` hides an item whatever the filter or invert, and the lower 16 bits hold marks of the code's own
- `order[i]` holds the display position of item `i`, below the item count, and Blender applies `use_filter_sort_reverse` after it
- `use_filter_invert` and `use_filter_sort_alpha` act in Blender's own filter alone, and `filter_items` reads them
- Empty `flags` or `order` leave items unfiltered or unsorted, and either one at another length prints an error and acts as empty
- `order` beside non-empty `flags` remaps in quadratic time over shown items, and `filter_items` returns an empty `order` while sorting is off
- `UI_UL_list.filter_items_by_name` matches a case-insensitive glob with `*` implied at both ends and returns an empty list for an empty filter
- `UI_UL_list.sort_items_by_name` and `sort_items_helper(<(index, key) rows>, <key>)` build `order`

```python
import bpy
import numpy as np

type Struct = bpy.types.bpy_struct[object]


def draw_item(
    _ui: bpy.types.UIList,
    _context: bpy.types.Context,
    layout: bpy.types.UILayout,
    _data: Struct,
    item: Struct,
    _icon: int,
    _active_data: Struct,
    _active_property: str,
) -> None:
    layout.prop(item, "name", text="", emboss=False)


def filter_items(ui: bpy.types.UIList, _context: bpy.types.Context, data: Struct, propname: str) -> tuple[list[int], list[int]]:
    items: bpy.types.bpy_prop_collection[bpy.types.PropertyGroup] = getattr(data, propname)
    items.foreach_get("hidden", hidden := np.empty(len(items), np.bool_))
    named: list[int] = bpy.types.UI_UL_list.filter_items_by_name(ui.filter_name, ui.bitflag_filter_item, items, reverse=ui.use_filter_invert)  # type: ignore[arg-type]  # ty: ignore[invalid-assignment]
    order: list[int] = bpy.types.UI_UL_list.sort_items_by_name(items) if ui.use_filter_sort_alpha else []  # type: ignore[arg-type]  # ty: ignore[invalid-assignment]
    return np.where(hidden, ui.bitflag_item_never_show, named or ui.bitflag_filter_item).tolist(), order


ITEM_UL_items = type("ITEM_UL_items", (bpy.types.UIList,), {"draw_item": draw_item, "filter_items": filter_items})
```

`hidden` stands for a `BoolProperty` on the item's `PropertyGroup`.

## [06]-[MODAL_FEEDBACK]

Modal operators show state through the window they run in, and each write outlives the operator until code clears it:
- `context.area.header_text_set(<text>)` writes the text into the area's visible tool header, else its header, the other header keeping its layout
- `context.workspace.status_text_set(<text>)` writes the left of the status bar, and `None` on either call restores its text
- `status_text_set(<draw>)` with a `(header, context)` function replaces `bpy.types.STATUSBAR_HT_header.draw` for every window until the next `status_text_set` call with text or `None`
- Status draw functions show key hints through `template_event_from_keymap_item(<item>, text=<label>)` over the operator's modal keymap items
- `window_manager.progress_begin(<min>, <max>)`, `progress_update(<value>)`, and `progress_end()` drive a progress cursor in the active window, background runs drawing nothing
- `context.window.cursor_modal_set(<cursor>)` holds a cursor until `cursor_modal_restore()` or `progress_end()`

## [07]-[GIZMOS]

Gizmo group lifetime follows its options and tools:
- Groups without `PERSISTENT` stay unpolled and undrawn until a tool naming them in `bl_widget` is active
- `window_manager.gizmo_group_type_ensure(<idname>)` links a group into every region of its type until `gizmo_group_type_unlink_delayed(<idname>)`, both raising `RuntimeError` for a `PERSISTENT` or unknown group
- `poll` and `setup_keymap(keyconfig)` are classmethods, and `setup`, `refresh`, `draw_prepare`, and `invoke_prepare(context, gizmo)` take `self`
- Targets bound in `setup` stay on the first object, `refresh` binding targets to current data and `draw_prepare` placing gizmos
- `use_undo`, off by default, pushes one undo step per use of the gizmo

Gizmo group `bl_options` flags answer one need each:

| [INDEX] | [NEED]                                     | [OPTION]                                         |
| :-----: | :----------------------------------------- | :----------------------------------------------- |
|  [01]   | Gizmos placed in world space               | `3D`, without it `matrix_basis` in region pixels |
|  [02]   | Size following view zoom                   | `SCALE`, without it a fixed display size         |
|  [03]   | Occlusion behind geometry                  | `DEPTH_3D`                                       |
|  [04]   | Selectable gizmos                          | `SELECT`                                         |
|  [05]   | Group shown with no tool naming it         | `PERSISTENT`                                     |
|  [06]   | Group hidden while one of its gizmos drags | `EXCLUDE_MODAL`                                  |
|  [07]   | Every group shown during a drag            | `SHOW_MODAL_ALL`                                 |
|  [08]   | Setup held until the tool operator runs    | `TOOL_INIT`                                      |
|  [09]   | Fallback tool keymap on the group          | `TOOL_FALLBACK_KEYMAP`                           |

Built-in gizmo types take one target each:

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

Gizmo members follow host rules:
- `target_set_prop` raises `RuntimeError` for an index past the target's length or a property the target cannot take
- Reads of an unbound target raise `ValueError`
- Style members (`draw_style`, `draw_options`, `transform`) forward to the gizmo's `properties`

Custom gizmos declare `bl_target_properties = ({"id": <name>, "type": <type>, "array_length": <n>},)` and draw their own shape:
- `setup` builds the shape through `self.new_custom_shape('TRIS', <vertices>)`
- `draw(context)` and `draw_select(context, select_id)` call `self.draw_custom_shape(<shape>, matrix=<matrix>, select_id=<id>)`
- `modal(context, event, tweak)` reads `SNAP` and `PRECISE` in `tweak`
- `exit(context, cancel)` writes the invoke value back to the target when `cancel` is true, Blender restoring nothing for a custom gizmo

## [08]-[TOOLS]

`bpy.utils.register_tool(<tool>, after={<tool idname>}, separator=<bool>, group=<bool>)` registers a `WorkSpaceTool` subclass, never `register_class`:
- Tools declare `bl_space_type`, `bl_context_mode`, `bl_idname`, and `bl_label`, and take `bl_description`, `bl_icon`, `bl_cursor`, `bl_widget`, `bl_widget_properties`, `bl_keymap`, `draw_settings`, and `draw_cursor`
- `bl_context_mode` names a key of the space's toolbar (`EDIT_MESH` in the 3D View, `UV` in the Image Editor, `SEQUENCER` in the Sequencer, `None` in the Node Editor), another key raising `KeyError`
- `group=True` starts a flyout group
- `register_tool` raises `Exception` for a taken `bl_idname` or a space with no toolbar, and an `after` matching no tool appends at the end
- `unregister_tool` raises `AttributeError` for a class never registered or already released

Tool members follow host rules:
- Tuple `bl_keymap` rows of `(<operator>, <event keywords>, {"properties": [...]})` create keymap `<prefix> <Mode Title>, <label>` in `keyconfigs.addon`
- Keymap prefixes come from `keymap_prefix` on the space's `ToolSelectPanelHelper` subclass (`VIEW3D_PT_tools_active.keymap_prefix`)
- Registrations finding the tool keymap present keep its items, and `unregister_tool` removes the keymap
- `bl_widget = <gizmo group idname>` shows that group while the tool is active, and `bl_widget_properties` takes a `list` of `(name, value)` pairs, another sequence failing `wm.tool_set_by_id` at activation
- `tool.gizmo_group_properties(<group idname>)` returns the group's settings for `draw_settings`
- `bl_icon` names a `.dat` geometry icon without its suffix, a stock name (`ops.generic.select_circle`) or an absolute path to the extension's file, a missing file drawing a blank icon
- `draw_settings(context, layout, tool)` takes no `self` and runs on each tool header redraw
- `tool.operator_properties(<operator idname>)` returns the operator values the tool header edits
- `draw_cursor(context, tool, xy)` draws in window coordinates
- `wm.tool_set_by_id(name=<idname>, space_type=<space>)` activates a tool, `space_type` naming the space for code outside it

## [09]-[ASSET_SHELVES]

`AssetShelf` subclasses show assets in a region of the 3D View, Image Editor, or a node editor, and `context.asset` reads the asset an operator acts on:
- `DEFAULT_VISIBLE` in `bl_options` shows the shelf region the first time a shelf polls, the region otherwise starting hidden
- `bl_activate_operator` and `bl_drag_operator` name the operators a click and a drag run
- `filter_<id type>` booleans drop other ID types before `asset_poll`
- `asset_poll`, `get_active_asset`, and `draw_context_menu(context, asset, layout)` are the callbacks
- `wm.call_asset_shelf_popover(name=<idname>)` opens a shelf as a popover

Regions show one shelf each:
- Regions keep the active shelf while its poll passes, else the first remembered one, else the first registered type that polls
- `show_region_asset_shelf` turns read-only where no shelf polls, an assignment raising `AttributeError`, and `space.is_property_readonly("show_region_asset_shelf")` reads it
- `context.asset_shelf` names the shelf under a `temp_override` naming the `ASSET_SHELF` region


## [11]-[TEXT]

Blender translates interface text at draw time in the interface language:
- `bl_label`, `bl_description`, property names, and literal `text=` and `report` messages translate with no code
- Operators, panels, and menus set their context in `bl_translation_context`, lists and headers holding none
- `bl_translation_context` defaults to `bpy.app.translations.contexts.operator_default` on operators and `contexts.default` on panels and menus
- Properties take their context through `translation_context=`
- `text_ctxt=<context>` on a layout call selects another context
- Built text translates its template, then formats it, then draws with `translate=False` (`iface_("{} items").format(count)`)
- `pgettext_iface`, `pgettext_tip`, `pgettext_rpt`, and `pgettext_data`, imported as `iface_`, `tip_`, `rpt_`, and `data_`, follow `use_translate_interface`, `use_translate_tooltips`, `use_translate_reports`, and `use_translate_new_dataname`
- `data_` names new data-blocks and group sockets, and `pgettext` ignores every flag
- `pgettext_n(<text>, <context>)`, imported as `n_`, marks table strings for extraction untranslated, and `iface_` translates them at draw
- `bpy.app.handlers.translation_update_post` runs after each language or translation flag change, and caches of translated text rebuild there
- `bpy.props` names default to the identifier as written (`ev_bias`), and `bpy.path.display_name(<identifier>)` derives labels, misreading acronyms and `use_` booleans as `Ev Bias` and `Use Region`
- Labels take title case and end in `...` where the action asks for more input, and boolean labels take no `Use`, `Enable`, or `Is`
- Tooltips open with an imperative verb, describe a toggle's on state, and end with no period
