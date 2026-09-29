# [INTERFACE]

Workspaces, areas, regions, panels, and views of a GUI window's screen, and pictures of the window.

## [01]-[TIMING]

- Workspace, area sizes, and region flags apply one timer `yield 0.1` after a write
- `window.workspace = <workspace>` applies after the call returns, `while window.workspace.name != name: yield 0.1` waits for it in a timer
- Screen operators but `area_close` finish in a background process
- Workspace switches, deletes, and area sizes apply in a GUI event loop alone
- Screen operators (`workspace.duplicate`, `workspace.delete`, `area_move`, `area_split`, `area_close`) poll under `temp_override(window=, screen=)`
- `temp_override(window=window)` derives no screen, `screen=window.screen` joins it
- `temp_override` refuses an area outside the window's current screen, operators on another workspace's area run after the workspace activates
- `ui_scale` moves no area vertex and changes the header size after the next draw, sizes follow in a later tick

## [02]-[WORKSPACES]

- Activation puts the active object in the workspace's `object_mode`, a mode it lacks (`POSE` on a mesh) keeps the current workspace with no error
- Workspace switches alone set `bpy.data.is_dirty`
- `bpy.ops.workspace.duplicate()` switches to the copy on a later pass
- Copies are the workspace absent from the set taken before the call, renamed after the switch
- `bpy.ops.workspace.delete()` deletes the active workspace on a later pass and keeps the last one
- `bpy.data.workspaces` iterates by name
- `bpy.ops.workspace.reorder_to_front()` moves the active workspace to the first tab, activations in reverse order restore an order
- Tab order sits in DNA `WorkSpace.order` outside RNA, and `screen.workspace_cycle(direction="NEXT")` visits workspaces in it
- Region flags and `SpaceProperties.context` written on a workspace off screen revert when it shows, writes follow its activation
- On-screen areas read through `workspace.screens[0].areas`
- File loads keep the window's screens while `filepaths.use_load_ui` is False, `wm.read_homefile(load_ui=True)` restores the startup screens
- `SpaceProperties.context` lists `OBJECT`, `MODIFIER`, and `MATERIAL` only with an active object
- `WorkSpace.owner_ids` with `use_filter_by_owner` filter add-on panels, menus, gizmo groups, and own-named keymaps
- Operators and tools of every add-on run in every workspace
- Toolbars read their tools through each `ToolSelectPanelHelper` subclass's `tools_from_context`
- Types with an empty owner id pass every owner filter
- `workspace.tools.from_space_view3d_mode("<MODE>", create=True).idname` sets the active tool per mode

## [03]-[AREAS]

- `Area.x`, `y`, `width`, and `height` are read-only device pixels screen operators (`area_move`, `area_split`, `area_join`, `area_close`) change
- `Area.type` is the editor family and `Area.ui_type` the sub-editor (`TIMELINE`, `ShaderNodeTree`, `FILES`), size tables key on `ui_type`
- `screen.area_split(direction=, factor=)` under an `area` override cuts an exact size from the bottom or left with no snap
- Split areas keep the larger part as the original area, and the new one is the area absent from the set taken before the call
- `screen.area_move(x=, y=, delta=)` polls true under a window and screen override, `x` and `y` name the edge it moves
- `--enable-event-simulate` launches drop OS input
- `window.event_simulate(type="MOUSEMOVE", value="NOTHING", x=, y=)` rests the pointer and raises `RuntimeError` without the flag
- `area_move` snaps a horizontal edge above a Timeline or Dope Sheet to header height plus 23 × scale, the area resized as an Outliner takes the exact size
- Window growth collapses a bottom Dope Sheet area at or under 1.5 times the minimum height to header height
- Editors trade places by swapping `area.type` values, each area keeps its rectangle and restores its stored space of that type
- `screen.area_close` closes the override's `area` in a GUI and hangs a background process
- `screen.area_swap(cursor=(x, y))` swaps the editors sharing the edge under the point

## [04]-[REGIONS]

- Region widths follow a drag of the inner edge, posted in an event-simulate Blender as MOUSEMOVE, LEFTMOUSE PRESS, MOUSEMOVE, LEFTMOUSE RELEASE
- Drags start 3 logical pixels inside the edge after `view2d.reset`, and hidden regions report 1x1
- Stored region widths read back as `int(scale * (logical + 0.5))` device pixels
- Toolbar glyphs scale with the region's view2d zoom alone, `scale_y` sizes the cell and never the glyph
- `view2d.zoom_out(zoomfacx=(z - 1) / 2, zoomfacy=(z - 1) / 2)` after `view2d.reset` sets zoom z, a drag snaps to z
- Region zoom reads as `width / ((width - 1) * span)`, `span` the x distance `view2d.region_to_view` gives between pixels 0 and 1
- `screen.region_toggle(region_type=)` under an area override shows or hides a region
- `show_region_asset_shelf` is writable where a registered `AssetShelf` polls true, `space.is_property_readonly(<name>)` reads writability on screen
- One region shows one shelf, the first registered type with a passing poll, and `filter_<id>` class booleans pre-filter its assets by ID type
- `bpy.ops.view3d.view_center_camera()` under a camera view's `WINDOW` override fits the frame to the region, a rerun at one size keeps the zoom
- `show_region_header` False hides the tool header with it, a hidden 3D tool header loses nothing, mode buttons and tool settings draw elsewhere
- `Area.show_menus` False keeps a narrow 3D Viewport header whole by folding every `Menu.draw_collapsible` menu into one icon
- Draw functions appended to the header itself stay beside the folded menus
- Top bar and status bar are `window.global_areas` (`TOPBAR`, `STATUSBAR`) outside `screen.areas`, `temp_override(area=)` takes them
- `Region.search_filter` holds the Sidebar search text
- `Region.active_panel_category` selects the Sidebar tab and reads `UNSUPPORTED` until the region draws
- Sidebar tabs draw while one of their panels polls true
- `system.show_panel_tabs_compact` draws Sidebar tabs as an icon or the first letters of the category

## [05]-[PANELS]

- Panel open state and order store per region per screen as `Panel` records in `startup.blend`, reset by rebuilding it from a factory layout
- Re-registering a class with `DEFAULT_CLOSED` closes it only in regions that never drew it, and stored records keep their `sortorder`
- `HIDE_HEADER` panels have no header to collapse, their parent tab panel takes the option
- Unregistering removes a type from its region list and registering appends it after every panel of equal `bl_order`
- Properties tabs hide per area through `show_properties_<tab>`, Bone, Bone Constraints, and Texture appear only in context

## [06]-[VIEWS]

- User's view is `region_3d` of the largest `VIEW_3D` area
- `view_perspective`, `view_rotation`, `view_location`, `view_distance`, and `use_view_flip_x` restore a view
- `view_camera_offset`, `view_camera_zoom`, and `view_camera_roll` restore a camera view
- `region_3d.pause_render` pauses a Rendered viewport where `support_pause_render` reads true
- `region_3d.window_matrix` and `view_matrix` keep the earlier view until `region_3d.update()` or the next call
- `region_3d.update()` segfaults a background process
- Orthographic device pixels per meter are `region.width * window_matrix[0][0] / 2`, `view_distance * measured / target` scales to a target
- `is_orthographic_side_view` reads true only after an axis operator, `view_perspective` names the projection
- `space.local_view` set marks local view, with a `view_distance` apart from the global view's
- In camera view, `view3d.move` pans the camera frame, `view3d.rotate` leaves the camera, wheel zoom changes `view_camera_zoom`
- `region_3d.lock_rotation` holds a view's orientation, view writes set it `False` first
- `view3d.view_axis`, `view_persportho`, `view_camera`, `view_orbit`, and `rotate` poll false under `lock_rotation`
- `inputs.use_auto_perspective` switches an axis view to orthographic and an orbit back to perspective
- Viewport `lens` acts on a 72 mm sensor, `space.lens` at twice a camera's `lens` shows that camera's field of view

## [07]-[PICTURES]

```bash
# Layer-0 windows of the GUI with their ids, then a capture of one without activating it
uv run --with pyobjc-framework-Quartz python -c 'import Quartz as q; [print(w[q.kCGWindowNumber], w.get(q.kCGWindowName)) for w in q.CGWindowListCopyWindowInfo(q.kCGWindowListOptionOnScreenOnly, q.kCGNullWindowID) if w[q.kCGWindowOwnerName] == "Blender" and w[q.kCGWindowLayer] == 0]'
screencapture -x -o -l <id> <dir>/<name>.png
```

- Main window is titled `<file> - Blender <version>` with a leading `*` while dirty, window ids change at every launch
- Region writes that rebuild no region (`active_panel_category`) show in a later frame, a capture follows a returned call
- `area.tag_redraw()` on every area, then a return from the call, precedes a window capture of a theme or view write
- Captures hold an ICC profile, byte reads convert through one tool every time (`magick -profile "sRGB Profile.icc"`)
- `mcp-for-blender` `get_viewport_screenshot` draws offscreen without overlays
- `wm.window_new()` under a `VIEW_3D` override opens a second window titled `3D Viewport`, `wm.window_close()` under it removes it
- Menus, panels, and pies open at the pointer from a timer through `wm.call_menu`, `wm.call_panel`, and `wm.call_menu_pie` under an area override
- `wm.call_panel` of a Properties panel polls under a `PROPERTIES` area override alone
- Pies close when `window.workspace` is reassigned
- `wm.call_panel(auto_keymap=True)`, `invoke_popup(auto_keymap=True)`, and `popover(auto_keymap=True)` underline accelerator keys
- `window_manager.try_activate_rna_button(region=, data=, property=)` activates the button drawing that property
