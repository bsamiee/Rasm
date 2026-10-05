# [INTERFACE]

Windows, views, workspaces, regions, and popups of the user's screen driven live, and the stored screens of a closed file.

## [01]-[WINDOWS]

Screen work kept off the user's window runs in a second window, the steps of one deferred call:
1. `bpy.ops.wm.window_new()` under `temp_override(window=manager.windows[0])` opens it, the window absent from the set held before the call
2. One `yield None` passes before the first read of its areas
3. Work runs under `temp_override(window=<window>, area=, region=)` of its areas
4. `window.workspace` takes back its opening workspace, and one pass later `wm.window_close()` under its override closes it, both in the `finally`

- New windows take the source screen's largest editor at 95 by 90 percent of the source window's point size over the native pixel size
- New 3D views start at lens 50 looking straight down
- Windows open on a one-area layout of the source workspace, which a close while the window shows it removes
- `screen.delete()` under a window's override deletes the layout it shows on the next pass and shows a layout of that workspace no other window shows

Use SKILL.md for pictures of a window or an area.

## [02]-[VIEWS]

Views save and restore by value through the view `viewport()` from `scene.py` finds:
1. Read the held view before the first change
2. Frame the work through `view_location` and `view_distance` with the selection kept
3. Write the held view back when the work ends or the user asks for their view again
4. Read `view_matrix` and `window_matrix` in the next call, the writing call holding the earlier matrices

```python
# [EXECUTE_BLENDER_CODE] User's view by value, the dict a later call writes back in its order
from scene import viewport

found = viewport()
view = found.space.region_3d
result = {
    "lens": found.space.lens,
    "lock_rotation": view.lock_rotation,
    "view_perspective": view.view_perspective,
    "view_rotation": view.view_rotation[:],
    "view_location": view.view_location[:],
    "view_distance": view.view_distance,
    "use_view_flip_x": view.use_view_flip_x,
    "view_camera_zoom": view.view_camera_zoom,
    "view_camera_offset": view.view_camera_offset[:],
    "view_camera_roll": view.view_camera_roll,
}
```

```python
# [EXECUTE_BLENDER_CODE] <Object> framed in the user's view, selection kept
import bpy
from scene import bounds, viewport

found = viewport()
view = found.space.region_3d
low, high = bounds(bpy.context.evaluated_depsgraph_get(), drawn=True)["<Object>"]
view.view_perspective = "PERSP" if view.view_perspective == "CAMERA" else view.view_perspective
view.view_location = (low + high) / 2
view.view_distance = max(high - low) * found.space.lens / 72
result = {"location": view.view_location[:], "distance": view.view_distance}
```

```python
# [EXECUTE_BLENDER_CODE] View written back from <held>, the earlier call's result
from scene import viewport

found, held = viewport(), <held>
found.space.lens = held.pop("lens")
for name, value in held.items():
    setattr(found.space.region_3d, name, value)
```

- Views frame `view_distance * 72 / lens` across the region's wider side, through `view_location`
- Value writes take effect under `lock_rotation`, which fails the polls of `rotate` and the axis, projection, orbit, roll, and camera view operators
- `view3d.view_axis(type=)` under the `WINDOW` region override turns orthographic under `inputs.use_auto_perspective`
- Axis views read `is_orthographic_side_view` true until a `view_rotation` write of another value, `view_perspective` naming the projection
- `space.local_view` set marks local view, with a `view_distance` apart from the global view's
- Viewport `space.lens` at twice an AUTO-fit camera's `lens` draws that camera's field of view
- `region_3d.pause_render` pauses a Rendered viewport where `support_pause_render` reads true
- Orthographic device pixels per meter are `region.width * window_matrix[0][0] / 2`, `view_distance * measured / target` scaling to a target

Camera views fit and restore through their zoom and offset:
1. `view_perspective = "CAMERA"`
2. `view3d.view_center_camera()` under the `WINDOW` region override fits the frame to the region, a rerun at one region size keeping the zoom
3. `view_camera_zoom` with `view_camera_offset` restore the camera view by value

## [03]-[WORKSPACES]

Workspaces copy through `WorkSpace.copy()` and take their areas in a second window, each `yield None` waiting for the pass applying the last step:

```python
# [EXECUTE_BLENDER_CODE] Copy <name> of <Workspace>, its main area beside a Properties column over an Outliner, laid out in a second window
import functools

import bpy

manager = bpy.context.window_manager


def split(window, area, direction, factor):
    before = {each.as_pointer() for each in window.screen.areas}
    with bpy.context.temp_override(window=window, area=area):
        bpy.ops.screen.area_split(direction=direction, factor=factor)
    yield None
    return next(each for each in window.screen.areas if each.as_pointer() not in before)


def steps():
    copy = bpy.data.workspaces["<Workspace>"].copy()
    copy.name = "<name>"
    source, opened = manager.windows[0], set(manager.windows)
    with bpy.context.temp_override(window=source):
        bpy.ops.wm.window_new()
    window = next(each for each in manager.windows if each not in opened)
    held = window.workspace
    try:
        yield None
        window.workspace = copy
        yield None
        main = max(window.screen.areas, key=lambda area: area.width * area.height)
        for area in [area for area in window.screen.areas if area != main]:
            with bpy.context.temp_override(window=window, area=area):
                bpy.ops.screen.area_close()
            yield None
        column = yield from split(window, main, "VERTICAL", 0.75)
        column.ui_type = "PROPERTIES"
        (yield from split(window, column, "HORIZONTAL", 0.3)).ui_type = "OUTLINER"
        yield None
        areas = [(area.ui_type, area.width, area.height) for area in window.screen.areas]
    finally:
        window.workspace = held
        yield None
        with bpy.context.temp_override(window=window):
            bpy.ops.wm.window_close()
    yield {"workspace": copy.name, "areas": areas}


check_is_finished = functools.partial(next, steps(), None)
```

- `WorkSpace.copy()` copies layouts, owners, filter, mode, and tools, and switches no window
- `window.workspace` reads the old workspace in the assigning call, and the next pass reads the new screen at its sizes
- Activation puts the active object into the workspace's `object_mode`, `workspace.object_mode = "OBJECT"` beforehand keeping the object's mode
- Switches set `bpy.data.is_dirty`, and a mode the object lacks (POSE on a mesh) leaves the object as it is
- `area_close` drops the area inside the call, and the neighbor joining over it takes its size on the next pass
- `area_split` cuts `factor` of the height from the bottom or the width from the left, the new area taking the upper or right part past 0.5
- New areas read a zero rectangle until the next pass, and a refused split returns `FINISHED` with the area set unchanged
- `area.ui_type` takes a sub-editor (`TIMELINE`, `ShaderNodeTree`, `FILES`), and space values written in the same call hold
- `screen.area_swap(cursor=(x, y))` swaps the editors of the areas sharing the edge under the point
- Window growth collapses a bottom Dope Sheet area at or under 1.5 times its minimum height to its header
- `tools/interface/blender/script/screens.py` holds each workspace's split tree and extents, and `cut` the factor of an exact extent
- `jump_to_tab_by_name` shows the user a workspace on the next pass
- `bpy.ops.workspace.delete()` under `temp_override(window=)` deletes the shown workspace on the next pass and shows the previous tab
- Copies no window shows leave through `bpy.data.batch_remove([<copy>])` and a second `batch_remove` of the screens `<copy>.screens` held before it
- Tab order follows activating each workspace in turn and calling `workspace.reorder_to_back()` one pass apart, names order `bpy.data`
- `temp_override(window=, screen=<another workspace's screen>, area=)` shows that workspace for the block and switches back at exit
- File loads keep the window's screens while `filepaths.use_load_ui` is False, `wm.read_homefile(load_ui=True)` restores the startup screens
- `owner_ids` under `use_filter_by_owner` keep an add-on's panels, menus, gizmo groups, and own-named keymaps, its operators and tools staying
- Types with an empty owner id pass every workspace filter
- `workspace.tools.from_space_view3d_mode("<MODE>", create=True).idname` sets a mode's active tool with no switch

## [04]-[REGIONS]

Regions show and hide through their space's `show_region_<name>` flags, each write rebuilding the area inside the call, on screen or off:

```python
# [EXECUTE_BLENDER_CODE] Sidebar of the user's view opened on tab <tab>
import functools

from scene import viewport


def steps():
    found = viewport()
    region = next(each for each in found.area.regions if each.type == "UI")
    found.space.show_region_ui = True
    while region.active_panel_category == "UNSUPPORTED":
        yield None
    region.active_panel_category = "<tab>"
    yield {"tab": region.active_panel_category}


check_is_finished = functools.partial(next, steps(), None)
```

- Hidden regions read 1x1 and shown ones their stored width in the writing call, and `active_panel_category` reads a tab one pass later
- Sidebar tabs draw while one of their panels polls, `UILayout.enum_item_name(region, "active_panel_category", "<tab>")` reading `""` for none
- `Region.search_filter` holds the Sidebar search text, and `system.show_panel_tabs_compact` draws tabs as icons or leading letters
- Regions with no flag (Properties `NAVIGATION_BAR`) toggle through `screen.region_toggle(region_type=)` under the area override
- `screen.region_flip()` with `region=` in the override moves a header, tool header, or tab column to the opposite side
- `show_region_header` False hides the area's tool header with it
- Hidden tool headers lose nothing, mode buttons drawing in the header and tool settings in Sidebar and Properties Tool
- `Area.show_menus` False folds every `Menu.draw_collapsible` menu into one icon, draw functions appended to the header staying
- Top bar and status bar are `window.global_areas` outside `screen.areas`, and `temp_override(area=)` takes them
- Sidebars and toolbars open at the width `startup.blend` stores, read back as `int(scale * (logical + 0.5))`
- `tools/interface/blender/stores.py` writes those widths and zooms into `startup.blend` after a quit, `screens.py` measuring them

Properties tabs, asset shelves, and panels hold state per area and region:
- `space.context` takes an object tab (`OBJECT`, `MODIFIER`, `MATERIAL`, `DATA`) once the area drew with an active object, one pass later
- `show_properties_<tab>` hides a tab per area, and Bone, Effects, Particles, Texture, and Strip appear once their context resolves
- `show_region_asset_shelf` takes a write on screen where `space.is_property_readonly("show_region_asset_shelf")` reads False
- Shelves show the first registered `AssetShelf` type whose poll passes, `filter_<id>` class booleans narrowing its assets by ID type
- Panel open state and order store per region per screen as `Panel` records in `startup.blend`, each record made at a region's first draw
- `tools/interface/blender/extension/panels.py` collapses add-on panels under owners, sets tab icons, packs toolbar columns, and adds shelves

## [05]-[POPUPS]

Menus, panels, and pies open inside the call under a window, area, and region override, at the window's last pointer position:

```python
# [EXECUTE_BLENDER_CODE] Menu <Menu> over the user's 3D Viewport
import bpy
from scene import viewport

found = viewport()
with bpy.context.temp_override(window=found.window, area=found.area, region=found.region):
    status = bpy.ops.wm.call_menu(name="<Menu>")
result = {"status": sorted(status)}
```

- `wm.call_menu`, `wm.call_panel(name=, keep_open=)`, and `wm.call_menu_pie` return `INTERFACE`, the popup clamped inside the window
- `wm.call_panel` of a Properties panel polls under a `PROPERTIES` area override, a pie of a menu the owner filter drops cancelling
- `auto_keymap=True` on `wm.call_panel`, `invoke_popup`, and `popover` underlines accelerator keys
- `try_activate_rna_button(region=, data=, property=, state=)` activates the property's button and returns its center in window pixels
- `state="TEXT_EDITING"` opens the button's text field, and `warp_cursor_at_button` False keeps the user's pointer
- `window.workspace = window.workspace` in a later call closes every popup of that window

## [06]-[STORED_SCREENS]

Headless calls write space values on the stored screens of a copy, and the save keeps them:

```python
# [HEADLESS_CALL] Sidebar closed and Properties on <CONTEXT> in every workspace, each 3D view's projection and distance read
import bpy

held = {}
for workspace in bpy.data.workspaces:
    for index, area in enumerate(workspace.screens[0].areas):
        match area.spaces.active:
            case bpy.types.SpaceView3D() as space:
                space.show_region_ui = False
                held[f"{workspace.name}/{index}"] = (space.region_3d.view_perspective, space.region_3d.view_distance)
            case bpy.types.SpaceProperties() as space:
                space.context = "<CONTEXT>"
bpy.ops.wm.save_mainfile()
result = {"views": held}
```

- Workspaces reach their screen through `bpy.data.workspaces["<name>"].screens[0]`, a background process switching no workspace
- Region flags, Properties tabs, and view values written there read back at the next load of the file
- Area splits and closes run in the GUI, a background process holding no window to lay them out
