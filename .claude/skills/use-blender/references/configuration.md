# [CONFIGURATION]

Preferences, add-on records, repositories, keymaps, themes, text styles, interface scale, and scene units, read live and written through their owner.

## [01]-[OWNERS]

Lasting values are `tools/interface/blender/` rows that `nx run rasm:interface -- blender` writes into `bpy.utils.user_resource("CONFIG")` files:
1. `rg -n '<member>' tools/interface/blender` names the row declaring a member, an undeclared value joining its category's module as a new row
2. Edit the row

- Scripted runs end in `save_userpref` and `save_homefile`, `stores.py` then rewriting stored shelves and region widths
- `userpref.blend` holds preferences, themes, text styles, add-on records with their groups, repositories, keymap diffs, and keyconfig preferences
- `startup.blend` holds startup scenes with their units and add-on Scene properties
- Applies discard an untitled GUI file and refuse a titled one holding unsaved edits
- Live writes of a declared value hold until the next apply writes its row again

Use `manage-repo` for interface rows and the apply.

## [02]-[READS]

Preferences read live in the GUI, the one process holding window-bound values (`system.dpi`, `system.ui_scale`, `system.pixel_size`, filled keymaps):
- Dynamic enum placeholders are `NONE` (`view_transform`, `display_device`), `DEFAULT` (`length_unit`, `temperature_unit`), `None` (`audio_device`)
- `bpy.types.UILayout.enum_item_name(<struct>, "<member>", "<identifier>")` returns `""` for an identifier outside a dynamic enum's current set

## [03]-[TRIALS]

Trials hold the value and `preferences.is_dirty`, write, read, and restore both in the same call's `finally`:

```python
# [EXECUTE_BLENDER_CODE] Trial of one preference, read and restored with the modified flag in one call
import bpy

preferences = bpy.context.preferences
held, dirty = preferences.<section>.<member>, preferences.is_dirty
try:
    preferences.<section>.<member> = <value>
    result = {"held": held, "trial": preferences.<section>.<member>}
finally:
    preferences.<section>.<member> = held
    preferences.is_dirty = dirty
```

Each write takes the process of what it persists:

| [INDEX] | [WRITE]                        | [PROCESS]                                          | [PERSISTS]                                        |
| :-----: | :----------------------------- | :------------------------------------------------- | :------------------------------------------------ |
|  [01]   | Declared or new lasting value  | Interface row and the apply                        | Every start                                       |
|  [02]   | Value drawn in the GUI         | `execute_blender_code` in the trial form           | Nothing, `finally` restoring value and flag       |
|  [03]   | Value read under user settings | `headless.py start <file> configuration-<purpose>` | Nothing, `save_userpref` writing the session copy |

- Sessions run on a copy of the user's config under `bpy.app.tempdir`, which `bpy.utils.user_resource("CONFIG")` and `wm.save_userpref()` name
- GUI quits at exit code 0 rewrite `userpref.blend` while `is_dirty` is set, and background quits and `os._exit` write nothing
- RNA preference, theme, text-style, and record writes set `is_dirty`
- Add-on preference groups, `view.show_addons_enabled_only`, and `extensions.active_repo` leave `is_dirty` unset, a GUI `save_userpref()` storing them
- `PASSWORD` subtype members (`access_token`, `auth_token`, API keys) store plain text, and reports leave them out

## [04]-[ADD_ONS]

Records and installed modules answer apart, a record naming an add-on enabled at startup and `preferences.addons.get` reading `None` without one:

```python
# [EXECUTE_BLENDER_CODE] Disabled modules, records without a module, and one add-on's preference group
import addon_utils
import bpy
from rna import plain

preferences = bpy.context.preferences
installed = {module.__name__: (module.bl_info["name"], *addon_utils.check(module.__name__)) for module in addon_utils.modules(refresh=False)}
record = preferences.addons.get("<module>")
result = {
    "disabled": sorted(name for name, (_, _, loaded) in installed.items() if not loaded),
    "missing": sorted(addon.module for addon in preferences.addons if addon.module not in installed),
    "settings": record and record.preferences and plain(record.preferences),
}
```

- `addon_utils.check(<module>)` returns `(loaded_default, loaded_state)`
- Modules are `bl_ext.<repository>.<id>` for an extension and the folder or `.py` stem of a `bl_info` add-on, beside its name in `installed`
- `missing` records log `Add-on not loaded` at every start, `preferences.addon_disable(module="<module>")` removes one
- `addon_disable` frees the preference group, a later enable starting from the add-on's defaults
- `bl_pkg`, `io_anim_bvh`, `io_curve_svg`, `io_mesh_uv_layout`, and `io_scene_fbx` load at every start whatever their record says
- Add-on Scene properties save into `startup.blend` and every file made from it
- Disabled add-ons keep their Scene values in `scene.bl_system_properties_get()` until deleted there

Use extensions.md for installs and registration.

## [05]-[REPOSITORIES]

Repositories are `packages.toml` rows, `plain(bpy.context.preferences.extensions.repos)` reading their `module`, `remote_url`, `source`, and flags:
- `module` names the `bl_ext.<module>` package and keys every record of the repository, which a write re-keys
- `use_cache` True keeps downloaded archives under `.blender_ext/cache/` whatever its label says, False deletes them after install
- `remote_url`, `use_access_token`, and `access_token` take writes on difference, each write starting a sync while online
- `repos.new` and `blender -c extension repo-add` add a row on every call
- GUI starts under `view.show_extensions_updates` sync each `use_sync_on_startup` repository holding a package once an `index.json` is 24 h old

## [06]-[KEYMAPS]

Keymaps read live, where the GUI holds `keyconfigs.user` as the default keyconfig and add-on items with the stored diffs applied:

```python
# [EXECUTE_BLENDER_CODE] Every user keymap item on one chord, with its keymap, position, and diff flag
import bpy

keyconfigs = bpy.context.window_manager.keyconfigs
chord = ("<KEY>", False, False, False, True)  # type, shift, ctrl, alt, oskey
result = {
    "items": [
        (keymap.name, index, item.idname, item.value, item.active, keymap.is_user_modified)
        for keymap in keyconfigs.user.keymaps
        for index, item in enumerate(keymap.keymap_items)
        if (item.type, item.shift_ui, item.ctrl_ui, item.alt_ui, item.oskey_ui) == chord
    ]
}
```

- First item with an operator polling true consumes an event, 3D Viewport regions run the tool keymap, mode keymaps, `3D View Generic`, then `3D View`
- Add-on items sit at the head of their user keymap, the last added first
- Stock Ctrl items have a Cmd copy just ahead of them on macOS, add-on items gaining none
- Plain Ctrl on Tab, H, M, W, Space, Period, and Accent Grave and Ctrl+Alt on Q gain no Cmd copy
- `preferences.keymap.active_keyconfig` names the preset, `keyconfigs.active.preferences` holds its options (`spacebar_action`, `use_pie_click_drag`)

Stock item edits are rows in `tools/interface/blender/script/preferences.py`, and new bindings register with their extension.

Trial edits run in a session, where `keyconfig_set` fills the keymaps and `keyconfigs.update()` applies the stored diffs:

```python
# [HEADLESS_CALL] Trial edit of one stock item, read back as a diff of its keymap
import bpy

keyconfigs = bpy.context.window_manager.keyconfigs
bpy.utils.keyconfig_set(bpy.utils.preset_find("Blender", "keyconfig"))


def found() -> bpy.types.KeyMapItem:
    return next(item for item in keyconfigs.user.keymaps["<Keymap>"].keymap_items if item.idname == "<operator>" and item.type == "<KEY>")


found().value = "<VALUE>"
keyconfigs.update()
result = {"edited": (found().value, keyconfigs.user.keymaps["<Keymap>"].is_user_modified)}
```

- `keyconfigs.update()` records edits as diffs and rebuilds the user items at new addresses, `found()` reading each item again after it
- Items written back to their default value drop their diff, `restore_to_default()` drops every edit of the keymap
- `KeyMapItem.idname` writes reset its properties, a copy writing `idname` first and then each property `is_property_set` on the source
- Operator-property edits enter the diff once `item.active` is written after them
- Add-on items added in the same event-loop pass as a user keymap edit enter the diff as removed, one timer tick between them keeping them
- Keyconfig preferences take writes on difference, each write regenerating the whole keyconfig

## [07]-[THEME]

`tools/interface/blender/script/theme.xml` holds each member as a `tools/interface/roles.py` placeholder, trials drawing in a capture in the call:

```python
# [EXECUTE_BLENDER_CODE] Trial of one theme color drawn in a top capture, restored with the modified flag
import bpy
from capture import capture
from results import as_result

preferences = bpy.context.preferences
space = preferences.themes[0].view_3d.space
held, dirty = tuple(space.gradients.high_gradient), preferences.is_dirty
try:
    space.gradients.high_gradient = (<r>, <g>, <b>)
    result = {"held": held, "capture": as_result(capture("<name>", view="top"))}
finally:
    space.gradients.high_gradient = held
    preferences.is_dirty = dirty
```

- `theme.py` solves the members Blender derives before it draws
- `plain(bpy.context.preferences.themes[0].<space>)` reads theme colors as byte / 255 floats, and a held tuple restores a member byte for byte
- Theme writes reach a capture in the same call under the viewport's `THEME` background
- Theme writes read back as `round(value * 255) / 255` at float32 (alpha 0.5 reads 0.502)
- `View3DShading.single_color` and `object_outline_color` are `COLOR` members in scene linear, theme members `COLOR_GAMMA` bytes
- `Color` conversions of an RNA color member convert zeros unless `.copy()` precedes them (`<member>.copy().from_srgb_to_scene_linear()`)

Preset clicks and `preferences.reset_default_theme()` reset every member and override, and the apply's preset restores theme and text styles whole:

```python
# [EXECUTE_BLENDER_CODE] Theme and text styles reset, then the preset the apply wrote run over them
import bpy

preferences = bpy.context.preferences
status = bpy.ops.script.execute_preset(filepath=preferences.themes[0].filepath, menu_idname="USERPREF_MT_interface_theme_presets")
result = {"status": sorted(status), "widget_points": preferences.ui_styles[0].widget.points}
```

- Resets rebuild `ui_styles`, reads after one taking `preferences.ui_styles[0]` again
- `ui_styles[0]` holds `panel_title`, `widget`, and `tooltip`, and shadow offsets, alpha, and value draw only while `shadow` is above 0
- Theme XML naming a member the build lacks prints `<Type>.<attr> not found` per member and applies the rest

## [08]-[SCALE_AND_FONTS]

Device pixel sizes follow from the GUI's scale values through Blender's own rounding:

```python
# [EXECUTE_BLENDER_CODE] Scale values and the device pixel sizes they give
from math import floor

import bpy

system = bpy.context.preferences.system
factor, pixel = system.dpi / 72, int(system.pixel_size)
widget = floor(18 * factor + 0.5) + 2 * pixel
result = {"dpi": system.dpi, "pixel_size": pixel, "widget_unit": widget, "area_header": widget + int(6 * factor), "device": {size: floor(size * factor + 0.5) for size in (<logical>,)}}
```

- `system.dpi` is `int(96 * native_pixel_size * view.ui_scale * 0.75)`
- `pixel_size` is `max(1, max(1, dpi // 64) + offset)` with THIN -1, AUTO 0, THICK +1
- `view.ui_scale` is the one multiplier, THIN keeping one-device-pixel lines at every `ui_scale` up to 1.33 on a 2x display
- `view.font_path_ui` and `font_path_ui_mono` take font files, empty meaning the bundled Inter and DejaVu Sans Mono
- Variable fonts take their `wght` from the text style's `character_weight`
- Text draws at style points times `view.ui_scale`, and `theme.py` solves the points the interface text size needs

## [09]-[UNITS]

Multiplying geometry coordinates in Blender units by `unit_settings.scale_length` gives meters. Divide meter inputs by `scale_length` before geometry writes. Unit systems change display and typed input alone. `bpy.ops.interface.units(system="<IMPERIAL|METRIC>")` switches every scene and dependent setting:
- `tools/interface/blender/extension/unit_system.py` declares those settings: unit tokens, merge tolerance, clip ranges, sheet scale, add-on lengths
- Scenes set outside the operator take `unit_settings.system` first, each system write resetting `length_unit` and `mass_unit`, then each unit token
- Unit tokens take the system's items alone (`temperature_unit = "CELSIUS"` raises under IMPERIAL), a raising tuple assignment keeping earlier targets
- `view_distance` stores Blender units
- Camera `lens` and `sensor_width` read millimeters
- Render pixel sizes have no imperial form
- `bpy.data.scenes.new()` starts from factory metric units, `bpy.ops.scene.new(type="EMPTY")` copying the current scene's settings

Typed dimensions convert to meters at 1/64 inch and back to display text:

```python
# [EXECUTE_BLENDER_CODE] Typed imperial dimensions to meters at 1/64 inch, and back to display text
import bpy

texts = ("<feet>' <inches>\"", '<feet>\' <inches>" <numerator>/<denominator>"')
meters = {text: round(bpy.utils.units.to_value("IMPERIAL", "LENGTH", text) / 0.0254 * 64) / 64 * 0.0254 for text in texts}
result = {"meters": meters, "display": {text: bpy.utils.units.to_string("IMPERIAL", "LENGTH", value, precision=3, split_unit=True) for text, value in meters.items()}}
```

- Imperial text parses as a sum of unit-marked terms, `-` and `+` being arithmetic and a number with no unit feet
- `5' 3.5"`, `5' 3" 1/2"`, and `5' 3"+1/2"` read 63.5 in, `5' 3-1/2"` reading 95.5 in
- Fractions take their own `"`, a spaced fraction without one (`5'-3 1/2"`, `3 1/16"`) raising `ValueError`
- `to_value` holds float error (`10' 6"` reads 3.2004000382814404 m) that the 1/64 inch rounding removes
- `to_string` picks its unit by magnitude and reads no `unit_settings`, parts below an inch reading in thou (1/16" as 62.5 thou)
- `precision` in `to_string` counts significant digits
- `divmod(round(m / 0.0254 * 64) / 64, 12)` gives feet and inches to 1/64"
- Reports state ft² and ft³ as meters over `0.3048` per dimension, lb as kg over `0.45359237`
- Viewport grids step through the system's length table, `grid_subdivisions` inactive under a unit system
