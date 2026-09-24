# [INTERFACE]

Workspaces, areas, regions, panels, and views change on the screen the window shows, one event-loop pass after the write, and pictures of the window come from a draw the agent owns.

## [01]-[TIMING]

- `bpy.app.timers` generators run between event-loop passes, and workspace, area sizes, and region flags settle one `yield 0.1` after a write
- `window.workspace = <workspace>` applies after the call returns, `while window.workspace.name != name: yield 0.1` waits for it in a timer
- Workspace and screen edits need a GUI instance
- Screen operators (`workspace.duplicate`, `workspace.delete`, `area_move`, `area_split`, `area_close`) poll under `temp_override(window=, screen=)`
- `temp_override(window=window)` derives no screen, `screen=window.screen` joins it
- `temp_override` refuses an area outside the window's current screen, an operator on another workspace's area runs after that workspace's activation
- `ui_scale` moves no area vertex and changes the header size after the next draw, sizes follow in a later tick

## [02]-[WORKSPACES]

- Activation puts the active object in the workspace's `object_mode`, a mode the object lacks (`POSE` on a mesh) keeps the old workspace with no error
- `workspace.object_mode = "OBJECT"` on every workspace first lets each activation succeed, stock Modeling and UV Editing hold `EDIT`
- Workspace switches alone set `bpy.data.is_dirty`
- `bpy.ops.workspace.duplicate()` switches to the copy on a later pass, another duplicate, activation, or delete before the switch crashes Blender
- Copies are the workspace absent from the set taken before the call, renamed after the switch, chained copies run one at a time
- `bpy.ops.workspace.delete()` deletes the active workspace on a later pass and refuses the last one, `bpy.data.workspaces` iterates in tab order
- `bpy.ops.workspace.reorder_to_front()` moves the active workspace to the first tab, activations in reverse order restore an order
- Renames are plain data writes, `bpy.data.workspaces["Layout"].name = "Model"`
- Region flags and `SpaceProperties.context` written on a workspace off screen revert when it shows, writes follow its activation
- On-screen areas read through `workspace.screens[0].areas`
- `SpaceProperties.context` lists `OBJECT`, `MODIFIER`, and `MATERIAL` only with an active object
- `WorkSpace.owner_ids` with `use_filter_by_owner` filter add-on panels, menus, gizmo groups, and own-named keymaps, operators carry no owner check
- Types with an empty owner id pass every owner filter
- `workspace.tools.from_space_view3d_mode("<MODE>", create=True).idname` sets the active tool per mode

## [03]-[AREAS]

- `Area.x`, `y`, `width`, and `height` are read-only device pixels, `area_move` and `area_split` are the only writers
- `Area.type` is the editor family and `Area.ui_type` the sub-editor (`TIMELINE`, `ShaderNodeTree`, `FILES`), size tables key on `ui_type`
- `screen.area_split(direction=, factor=)` under an `area` override cuts an exact size from the bottom or left with no snap
- Split areas keep the larger part as the old area, and the new one is the area absent from the set taken before the call
- `screen.area_move` polls true only with the pointer resting on the edge, so scripted sizes come from `area_split`
- Window growth collapses a bottom Dope Sheet area at or under 1.5 times the minimum height to header height
- Editors trade places by swapping `area.type` values, each area keeps its rectangle and restores its stored space of that type
- `screen.area_close` closes the override's `area`, `screen.area_swap(cursor=(x, y))` swaps the editors sharing the edge under the point

## [04]-[REGIONS]

- Region widths change by a drag of the region edge alone, no RNA member writes them, hidden regions report 1x1
- Stored region widths read back as `int(scale * (logical + 0.5))` device pixels
- `screen.region_toggle(region_type=)` under an area override shows or hides a region
- `Region.alignment` is read-only, `screen.region_flip` on the region flips it
- `SpaceNodeEditor.show_region_asset_shelf` is writable on a `CompositorNodeTree` editor alone, `space.is_property_readonly(<name>)` on screen tells
- Writing `show_region_hud` crashes Blender
- `show_region_header` False hides the tool header too, a hidden 3D tool header loses nothing, mode buttons and tool settings draw elsewhere
- `Area.show_menus` False folds every menu drawn through `Menu.draw_collapsible` into one icon, which keeps a narrow 3D Viewport header whole
- Draw functions appended to the header itself stay beside the folded menus
- Top bar and status bar are window areas absent from `screen.areas`, right-click on the top bar is their only path
- `Region.active_panel_category` selects the Sidebar tab, reads `UNSUPPORTED` until the region draws, and no RNA hides a tab
- `system.show_panel_tabs_compact` draws Sidebar tabs as an icon or the first letters of the category

## [05]-[PANELS]

- Panel open state and order store per region per screen as `Panel` records in `startup.blend`, and no Python collection changes a stored record
- Re-registering a class with `DEFAULT_CLOSED` closes it only in regions that never drew it, stored records keep their `sortorder` too
- Stored panel order and state reset only by rebuilding `startup.blend`, from a factory layout with Blender quit
- `HIDE_HEADER` panels have no header to collapse, their parent tab panel takes the option
- Unregistering removes a type from its region list and registering appends it after every panel of equal `bl_order`
- Properties tabs hide per area through `show_properties_<tab>`, Bone, Bone Constraints, and Texture appear only in context

## [06]-[VIEWS]

- User's view is `region_3d` of the largest `VIEW_3D` area, view operators run under its `WINDOW` region override
- `view_perspective`, `view_rotation`, `view_location`, `view_distance` restore a view, `view_camera_offset` and `view_camera_zoom` in camera view
- `region_3d.window_matrix` and `view_matrix` keep the earlier view until `region_3d.update()` or the next call
- Orthographic device pixels per meter are `region.width * window_matrix[0][0] / 2`, `view_distance * measured / target` scales to a target
- `is_orthographic_side_view` reads true only after an axis operator, `view_perspective` names the projection
- `space.local_view` set marks local view, with a `view_distance` apart from the global view's
- In camera view `view3d.move` pans the camera frame, `view3d.rotate` leaves the camera, wheel zoom changes `view_camera_zoom`
- Parallel and camera views of the user's GUI hold `region_3d.lock_rotation`, view writes set it `False` first, and the next draw relocks them
- `view3d.view_axis`, `view_persportho`, `view_camera`, and `view_orbit` poll false under the lock
- `inputs.use_auto_perspective` on switches to orthographic on an axis view and back to perspective on orbit, off leaves axis views unlocked

## [07]-[PICTURES]

```bash
# Layer-0 windows of the GUI with their ids, then one window at 1:1 device pixels with no activation
uv run --with pyobjc-framework-Quartz python -c 'import Quartz as q; [print(w[q.kCGWindowNumber], w.get(q.kCGWindowName)) for w in q.CGWindowListCopyWindowInfo(q.kCGWindowListOptionOnScreenOnly, q.kCGNullWindowID) if w[q.kCGWindowOwnerName] == "Blender" and w[q.kCGWindowLayer] == 0]'
screencapture -x -o -l <id> <dir>/<name>.png
```

- Main window is titled `<file> - Blender <version>` with a leading `*` while dirty, ids change at every launch
- Region writes that rebuild no region (`active_panel_category`) show in a later frame, the capture follows a call that returned
- `area.tag_redraw()` on every area, then a return from the call, precedes a window capture of a theme or view write
- Captures hold an ICC profile, byte reads convert through one tool every time (`magick -profile "sRGB Profile.icc"`)
- Community `get_viewport_screenshot` draws offscreen without overlays
- `wm.window_new()` under a `VIEW_3D` override opens a second window titled `3D Viewport` for a probe view, `wm.window_close()` under it removes it
- Pies open from a timer through `wm.call_menu_pie` under an area and region override and close by reassigning `window.workspace`, with no key event
