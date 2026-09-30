# [CONFIGURATION]

Preferences, add-on records, repositories, keymaps, themes, text styles, interface scale, and scene units, read live and written through their owner.

## [01]-[OWNERS]

`bpy.utils.user_resource("CONFIG")` holds `userpref.blend` (preferences, themes, text styles, add-on records with their preference groups, repositories, keymap diffs, keyconfig preferences) and `startup.blend` (startup scenes, their units, add-on Scene properties). `nx run rasm:interface -- blender` writes every value `tools/interface/blender/` declares, and saves both files once at its end:
- `rg -n '<member>' tools/interface/blender` finds a member's row, and a change to a declared value edits that row for the apply
- Values no row declares join the interface module of their category as a new row
- Live writes of a declared value hold until the next apply writes the row again
- Every GUI quit rewrites `userpref.blend` from memory while `preferences.is_dirty` is set, so a process writing the file runs with no GUI open

Use `manage-repo` for interface rows and the apply.

## [02]-[READS]

Preferences read live in the GUI, the one process holding window-bound values (`system.dpi`, `system.ui_scale`, `system.pixel_size`, filled keymaps). `plain(<struct>)` from `rna.py` reads a section as JSON with passwords and class registration members left out. Factory values come from a factory `headless.py run` of the same read, `bl_rna.properties[<name>].default` misreports members (`view.header_align`, `system.audio_device`, `system.network_timeout`, `filepaths.font_directory`):

```bash
# Factory values of one preference section as JSON
python .claude/skills/use-blender/scripts/headless.py run <dir>/configuration-factory.blend > <dir>/factory.json <<'PY'
import bpy
from rna import plain
result = {"section": plain(bpy.context.preferences.view)}
PY
```

```python
# [EXECUTE_BLENDER_CODE] Live members of the section that differ from factory, each as [factory, live]
import json
from pathlib import Path

import bpy
from rna import plain

factory = json.loads(Path("<dir>/factory.json").read_text(encoding="utf-8"))["result"]["section"]
result = {key: [factory.get(key), value] for key, value in plain(bpy.context.preferences.view).items() if factory.get(key) != value}
```

- Each differing member leads to its interface row through `rg`
- Dynamic enums (`view_transform`, `display_device`, `length_unit`, `temperature_unit`, `audio_device`) list `NONE` or `DEFAULT` in `enum_items`
- `bpy.types.UILayout.enum_item_name(<struct>, "<member>", "<identifier>")` returns `""` for an identifier outside a dynamic enum's current set
- `render.engine` lists `BLENDER_EEVEE` alone in `enum_items`, the current value names a valid engine

## [03]-[WRITES]

Writes take one process by what they persist:
- Declared or new persistent values take their interface row and the apply
- Trial writes run in `headless.py start <file> configuration-<purpose>`, a session on a copy of the user's config under `bpy.app.tempdir`
- `bpy.ops.wm.save_userpref()` writes the current process's config folder at once, the session's copy included, and `stop` deletes the copy
- GUI writes persist at the GUI quit when they tag `is_dirty`, as RNA preference, theme, text-style, and record writes do
- Add-on preference groups, `view.show_addons_enabled_only`, and `extensions.active_repo` leave `is_dirty` unset, `save_userpref()` in the GUI stores them
- `preferences.is_dirty` is writable, background quits and `os._exit` write no preferences
- `PASSWORD` subtype members (`access_token`, `auth_token`, API keys) store plain text, and reports leave them out

## [04]-[ADD_ONS]

Records and installed modules answer apart, a record names an add-on enabled at startup:

```python
# [EXECUTE_BLENDER_CODE] Installed modules with name and load state, records without a module, and one add-on's preference group
import addon_utils
import bpy
from rna import plain

preferences = bpy.context.preferences
installed = {module.__name__: (module.bl_info["name"], *addon_utils.check(module.__name__)) for module in addon_utils.modules(refresh=False)}
result = {
    "disabled": sorted(name for name, (_, _, loaded) in installed.items() if not loaded),
    "missing": sorted(addon.module for addon in preferences.addons if addon.module not in installed),
    "settings": plain(preferences.addons["<module>"].preferences),
}
```

- `addon_utils.check(<module>)` returns `(loaded_default, loaded_state)`
- Modules are `bl_ext.<repository>.<id>` for an extension and the folder or `.py` stem of a `bl_info` add-on, beside its name in `installed`
- `missing` records log `Add-on not loaded` at every start, `preferences.addon_disable(module="<module>")` removes one
- `addon_disable` frees the preference group, a later enable starts from the add-on's defaults
- `bl_pkg`, `io_anim_bvh`, `io_curve_svg`, `io_mesh_uv_layout`, and `io_scene_fbx` load at every start whatever their record says
- Add-on Scene properties save into `startup.blend` and every file made from it
- Disabled add-ons keep their Scene values in `scene.bl_system_properties_get()` until deleted there
- Stored records read without importing an add-on under `BLENDER_USER_CONFIG` on a scratch copy and every other `BLENDER_USER_*` on empty folders

Use `extensions.md` for installs and registration.

## [05]-[REPOSITORIES]

`plain(bpy.context.preferences.extensions.repos)` reads every repository with its `module`, `remote_url`, `source`, and flags. Repositories and packages are `packages.toml` rows beside the apply:
- `module` names the `bl_ext.<module>` package and keys every record of the repository, a write re-keys them, so it keeps its created value
- `use_cache` True keeps downloaded archives under `.blender_ext/cache/` whatever its label says, False deletes them after install
- Writes to `remote_url`, `use_access_token`, or `access_token` start a sync at once while online, so each is written on difference
- `repos.new` and `blender -c extension repo-add` add a row on every call
- `system.use_online_access` gates every sync, `--offline-mode` and `--online-mode` override it for one launch
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
- Stock Ctrl items have Cmd copies beside them on macOS, add-on items have none
- `preferences.keymap.active_keyconfig` names the preset, `keyconfigs.active.preferences` holds its options (`spacebar_action`, `use_pie_click_drag`)

Stock item edits are rows in `tools/interface/blender/script/preferences.py`, new bindings register with their extension in `keyconfigs.addon`. A trial edit runs in a session, where `keyconfig_set` fills the keymaps and `keyconfigs.update()` applies the stored diffs:

```python
# [HEADLESS_CALL] Trial edit of one stock item recorded as a diff, then its held value written back
import bpy

keyconfigs = bpy.context.window_manager.keyconfigs
bpy.utils.keyconfig_set(bpy.utils.preset_find("Blender", "keyconfig"))


def found() -> bpy.types.KeyMapItem:
    return next(item for item in keyconfigs.user.keymaps["<Keymap>"].keymap_items if item.idname == "<operator>" and item.type == "<KEY>")


held = found().value
found().value = "<VALUE>"
keyconfigs.update()
edited = (found().value, keyconfigs.user.keymaps["<Keymap>"].is_user_modified)
found().value = held
keyconfigs.update()
result = {"edited": edited, "restored": (found().value, keyconfigs.user.keymaps["<Keymap>"].is_user_modified)}
```

- `keyconfigs.update()` records edits as diffs and rebuilds the user items, `found()` reads each item again after it
- Held values written back drop their diff, `restore_to_default()` drops every edit of the keymap
- `KeyMapItem.idname` writes reset its properties, a copy writes `idname` first and then each property `is_property_set` on the source
- Operator-property edits enter the diff once `item.active` is written after them
- Add-on items added in the same event-loop pass as a user keymap edit enter the diff as removed, one timer tick between them keeps them
- Keyconfig preference writes regenerate the whole keyconfig, so each is written on difference

## [07]-[THEME]

`plain(bpy.context.preferences.themes[0].<space>)` reads theme colors as byte / 255 floats, and `themes[0].filepath` names the preset the apply wrote. `tools/interface/blender/script/theme.xml` holds each member as a role placeholder over `tools/interface/roles.py`, and `theme.py` solves members Blender derives before it draws. A trial write runs live or in a session, then the preset restores theme and text styles:

```python
# [EXECUTE_BLENDER_CODE] Theme and text styles reset, then the preset the apply wrote run over them
import bpy

preferences = bpy.context.preferences
status = bpy.ops.script.execute_preset(filepath=preferences.themes[0].filepath, menu_idname="USERPREF_MT_interface_theme_presets")
result = {"status": sorted(status), "widget_points": preferences.ui_styles[0].widget.points}
```

- Theme colors store bytes, a write reads back as `round(value * 255) / 255` at float32 (alpha 0.5 reads 0.502)
- Preset clicks and `preferences.reset_default_theme()` reset every member first, so overrides written before them go
- Resets rebuild `ui_styles`, reads after one take `preferences.ui_styles[0]` again
- `ui_styles[0]` holds `panel_title`, `widget`, and `tooltip`, shadow offsets, alpha, and value draw nothing while `shadow` is 0
- Theme XML naming a member the build lacks prints `<Type>.<attr> not found` per member and applies the rest
- `View3DShading.single_color` and `object_outline_color` are `COLOR` members in scene linear, theme members `COLOR_GAMMA` bytes

Use `interface.md` for captures of a theme write.

## [08]-[SCALE_AND_FONTS]

Scale reads in the GUI, background processes read factory values or the ones the last windowed run stored:

```python
# [EXECUTE_BLENDER_CODE] Scale values and the device pixel sizes they give
from math import floor

import bpy

system = bpy.context.preferences.system
factor, pixel = system.dpi / 72, int(system.pixel_size)
widget = floor(18 * factor + 0.5) + 2 * pixel
result = {"dpi": system.dpi, "pixel_size": pixel, "widget_unit": widget, "area_header": widget + int(6 * factor), "device": {size: floor(size * factor + 0.5) for size in (<logical>,)}}
```

- `system.dpi` is `int(96 * native_pixel_size * view.ui_scale * 0.75)`, `pixel_size` is `max(1, dpi // 64 + offset)` with THIN -1, AUTO 0, THICK +1
- `view.ui_scale` is the one multiplier, THIN keeps one-device-pixel lines at every `ui_scale` up to 1.33 on a 2x display
- `view.font_path_ui` and `font_path_ui_mono` take font files, empty means the bundled Inter and DejaVu Sans Mono
- Variable fonts take their `wght` from the text style's `character_weight`
- Text draws at style points times `view.ui_scale`, and `theme.py` solves the points the interface text size needs

## [09]-[UNITS]

`bpy` lengths are meters under every unit system, and a system changes display and typed input alone. `bpy.ops.interface.units(system="<IMPERIAL|METRIC>")` switches every scene and the application-wide lengths, `tools/interface/blender/extension/unit_system.py` declaring each setting that follows the system:
- Scenes without the operator take `unit_settings.system` first, then each unit token, a system write resets `length_unit` and `mass_unit` even at the same value
- Unit tokens take the system's items alone (`temperature_unit = "CELSIUS"` raises under IMPERIAL), a raising tuple assignment leaves earlier targets written
- `view_distance` stores meters with unit NONE, camera `lens` and `sensor_width` read millimeters, and render pixel sizes have no imperial form

```python
# [EXECUTE_BLENDER_CODE] Typed imperial dimensions to meters at 1/64 inch, and back to display text
import bpy

texts = ("<feet>' <inches>\"", '<feet>\' <inches>" <numerator>/<denominator>"')
meters = {text: round(bpy.utils.units.to_value("IMPERIAL", "LENGTH", text) / 0.0254 * 64) / 64 * 0.0254 for text in texts}
result = {"meters": meters, "display": {text: bpy.utils.units.to_string("IMPERIAL", "LENGTH", value, precision=3, split_unit=True) for text, value in meters.items()}}
```

- Imperial text parses as a sum of unit-marked terms, `-` and `+` are arithmetic, and a number with no unit is feet
- `5' 3.5"`, `5' 3" 1/2"`, and `5' 3"+1/2"` read 63.5 in, and `5' 3-1/2"` reads 95.5 in
- Spaced fractions without their own `"` (`5'-3 1/2"`, `3 1/16"`) raise `ValueError`
- `to_value` carries float error (`10' 6"` reads 3.2004000382814404 m) that the 1/64 inch rounding removes
- `to_string` picks its unit by magnitude and reads no `unit_settings`, parts below an inch read in thou (1/16" as 62.5 thou)
- `precision` in `to_string` counts significant digits
- Viewport grids step through the system's length table, `grid_subdivisions` is inactive under a unit system
