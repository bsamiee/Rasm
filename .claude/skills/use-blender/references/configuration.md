# [CONFIGURATION]

Preferences, add-on records, repositories, keymaps, themes, fonts, and units, each written through the API in one process and read back from the store that holds it.

## [01]-[STORES]

- `userpref.blend` under `user_resource("CONFIG")` holds preferences, themes, text styles, add-on records with preference groups, and repositories
- `userpref.blend` also holds keymap diffs and keyconfig preferences, the config folder name is major.minor
- `startup.blend` beside it holds screens, workspaces, the startup scene, and Window Manager ID properties
- `bpy.ops.wm.save_userpref()` writes at once, `use_preferences_save` True rewrites the file at quit whenever `is_dirty` is set, `os._exit` skips it
- Theme, text-style, and add-on record changes tag dirty, `view.show_addons_enabled_only` and `extensions.active_repo` do not
- Last writer wins across processes, preference writes run in one process with no other Blender open
- `--factory-startup` background processes never save preferences
- `bpy.ops.wm.save_homefile()` saves the current screens from a window context, a background save writes the file with no workspace edit before it
- `filepaths.use_load_ui` False keeps the startup screens when a file opens, the workspace active at `save_homefile` opens the next session
- Add-on Scene properties save into `startup.blend` and every file made from it, a disabled add-on's keys stay until deleted

## [02]-[READBACK]

- Writes read back by path, and one walk before and one after a step names every change the step caused
- Walks follow `bl_rna.properties`, skip `rna_type` and `bl_rna`, record ID pointers by name, and key collection items by `name_property` when unique
- Nested RNA structs share their owner's address, a cycle guard keys on `(as_pointer(), type)`, the address alone hides whole sections
- Factory values come from a `--factory-startup` process, `prop.default` misreports `keyframe_new_handle_type`, `audio_device`, and `font_directory`
- Dynamic enums list `NONE` or `DEFAULT` in `bl_rna` (`display_device`, `view_transform`, `look`, `render.engine`, `length_unit`, `temperature_unit`)
- Valid items of a dynamic enum come from the `TypeError` of an unknown identifier, `enum "X" not found in (...)`, the value restored after the read
- `show_statusbar_vram` on Metal, `support_emulation`, and `use_remote_asset_libraries` raise `AttributeError` on write
- `system.dpi`, `ui_scale`, `pixel_size`, and `ui_line_width` are runtime results of `view.ui_scale` and `view.ui_line_width`
- `system.gpu_preferred_device` reads METAL in the GUI and AUTO headless
- Theme structs mix `COLOR_GAMMA` arrays with scalars (`grid_levels`, `dash_alpha`, `noodle_curving`), a walk branches on `subtype` or `array_length`
- `SpaceNodeEditor.cursor_location`, `SpaceConsole.select_start`, `select_end`, and `Preferences.is_dirty` change with use and leave the comparison
- `FileSelectParams.directory` is a byte string that reads back with a trailing separator, writes take `f"{folder}/".encode()`
- `PASSWORD` subtype properties (`access_token`, API keys) store plain text and leave every report

## [03]-[ADDONS]

- `preferences.addons` holds enabled add-ons alone, indexing an absent one raises `KeyError`, `addon_utils.modules()` lists every installed module
- `scene.<group>` of a disabled add-on raises `AttributeError`, presence resolves before the read
- Module names are `bl_ext.<repository>.<id>` for an extension, the module for a core add-on, and the zip's top folder or stem for a legacy add-on
- Legacy folder names carry a branch or version suffix (`BlenderGIS-2215`), code keys them on `bl_info["name"]` and resolves the module at run time
- `{m.bl_info["name"]: m.__name__ for m in addon_utils.modules()}` resolves a legacy module, `__name__.rpartition(".")[2]` names an extension
- `preferences.addon_disable` unregisters, removes the record, and frees its preference group, a later enable starts from the add-on's defaults
- `addon_disable` on a record whose module is gone still removes the record, a record without a module logs `Add-on not loaded` at every start
- `bl_pkg`, `io_anim_bvh`, `io_curve_svg`, `io_mesh_uv_layout`, and `io_scene_fbx` load at every start whatever their record says
- `bl_pkg` holds the extension system, and add-ons that fail in `--background` keep their record with `loaded` False
- Bonsai prints `KeyError: 'materials'` at registration without an IFC file and Sverchok logs one `registration error`, both load
- Sverchok `log_to_buffer` writes a `sverchok.log` text block into every file, off with `bpy.data.texts["sverchok.log"]` removed before a save
- BlenderGIS `logLevel` DEBUG floods the console, `overpassServer` default fails, `cacheFolder` and `core/settings.json` sit inside the add-on tree
- Dimension add-ons each hold one imperial precision (`imperial_denominator`, `imperial_precision`, `imperial_fraction_denominator`)
- One denominator feeds every precision field as a string, Bonsai `doc.imperial_precision` as `1/<n>`

## [04]-[REPOSITORIES]

- `preferences.extensions.repos` hold `blender_org`, `user_default`, `system`, and added remotes, each `module` naming its `bl_ext.<module>` package
- `repos[i].module` has a setter that re-keys every add-on record and preference group of that repository, no code writes it
- `system.use_online_access` gates `repo_sync_all` and the Lab add-on's autostart, `--offline-mode` locks it for one launch
- Writes to `remote_url`, `use_access_token`, or `access_token` start a network sync at once when online, written on difference alone
- `use_cache` True keeps downloaded archives under `.blender_ext/cache/`, False deletes them after install and clears the cache before an upgrade
- Startup sync runs online when a `use_sync_on_startup` repository's `index.json` is older than 24 h, the flag off everywhere keeps starts offline
- Installing into one repository reloads add-ons of the others, install output shows their unregister and register lines

## [05]-[KEYMAPS]

- `preferences.keymap.active_keyconfig` names the preset, `keyconfigs.active.preferences` holds `spacebar_action` and `use_pie_click_drag`
- `keyconfigs.user` rebuilds at load as the default keyconfig plus add-on items with the stored diffs applied, `keymap.is_user_modified` flags a diff
- First keymap item whose operator polls true consumes an event, `view3d.rotate` polls false under `region_data.lock_rotation`
- Add-on item added and a user keymap edited in the same event-loop pass records the item as removed in the diff, one tick between them keeps it
- `restore_to_default()` then `keyconfigs.update()` merges dropped add-on items back and drops every user edit of that keymap
- Writing `KeyMapItem.idname` resets its properties, a copy writes `idname` first and then each property `is_property_set` on the source
- Removing an operator type (an add-on renamed or reloaded) frees the properties of every item pointing at it
- Context menus sit on RIGHTMOUSE PRESS as `wm.call_menu` and `wm.call_panel` items, `CLICK` keeps a still click opening them under a CLICK_DRAG item
- Cmd copies of every Ctrl item exist on macOS, the Ctrl items stay, and no held modifier leaves every letter free
- Background keyconfigs hold the C keymaps alone, `bpy.utils.keyconfig_set(bpy.utils.preset_find("Blender", "keyconfig"))` fills the default one

## [06]-[THEME]

- Theme colors store bytes exposed as `COLOR_GAMMA` floats, a write of `byte / 255` reads back quantized, comparisons run on bytes
- `View3DShading.single_color` and `object_outline_color` are `COLOR` properties holding scene linear values
- Add-on colors drawn through the `gpu` module read as sRGB whatever their subtype
- Preset clicks run `reset_default_theme` then the preset XML, `Blender_Dark.xml` is a pure reset, and every override written before it goes
- `reset_default_theme` rebuilds `ui_styles` to 11 points, weight 400, shadow 3, text styles are written after the theme
- Text style shadow 0 makes the shadow offsets, alpha, and value inert
- 3D grid lines draw the theme color shaded +10 for `grid` and +20 for `grid_major` while the major line stays brighter than the canvas
- Hovered menu rows draw `int(0.8 * inner + 0.2 * text)`, a selected list item draws `0.5 * inner_sel + 0.5 * outline`
- Outliner hover is white at alpha 0.13 over any row, with no theme member
- `wcol_state.blend` mixes the state color into a field, only the `_sel` members of the `inner_*` state colors draw
- `user_interface.gizmo_hi` is the hover color of every viewport gizmo, live with no control on the Themes page
- `themes[0].filepath` names a preset file, `bl_pkg` reapplies a changed file and resets the theme for a vanished one after extension operations

## [07]-[FONTS_AND_SCALE]

- `view.font_path_ui` and `font_path_ui_mono` take file paths, empty means the bundled Inter and DejaVu Sans Mono
- Variable fonts take their `wght` from `character_weight`
- `view.text_hinting` NONE keeps glyph shapes at 2 device px per point, `use_text_render_subpixelaa` is subpixel placement up to 16 pt
- `dpi = 96 * native_pixel_size * view.ui_scale * 0.75` as an integer, `pixelsize = max(1, dpi // 64 + offset)`, THIN -1, AUTO 0, THICK +1
- `scale_factor = dpi / 72`, `widget_unit = round(18 * scale_factor) + 2 * pixelsize`, area header `widget_unit + int(6 * scale_factor)`
- Device pixels of a logical size are `round(logical * preferences.system.ui_scale)`
- THIN keeps one-device-pixel lines at every `ui_scale` up to 1.33 on a 2x display
- `view.border_width` 1 draws 1 device px per area side, `view.use_reduce_motion` turns off panel, region, smooth view, and pie animation
- `view.header_align` TOP or BOTTOM resets every screen's header side at load, NONE keeps each file's own
- `system.use_region_overlap` False makes every region opaque beside the main region, header alpha and region fade then draw nothing
- `render_display_type` NONE keeps the layout on render, `filebrowser_display_type` and `preferences_display_type` SCREEN open a maximized area
- `show_extensions_updates` off hides the blocked-extension alert and the offline icon with the update count

## [08]-[UNITS]

- Assigning `unit_settings.system` resets `length_unit` and `mass_unit` to that system's defaults, `system` writes before the other fields
- Unit fields take the system's items alone, `temperature_unit = "CELSIUS"` raises under `IMPERIAL`
- Tuple assignments that raise leave their earlier targets written, unit fields take one assignment each
- Lengths are meters built from exact literals, `INCH = 0.0254`, `FOOT = 12 * INCH`, a bare 1.7 displays as an odd imperial value
- `view_distance` stores meters with unit NONE, and render pixel sizes have no imperial form
- `bpy.utils.units` systems are `IMPERIAL`, `METRIC`, `NONE`, categories read through `dir(bpy.utils.units.categories)`
- Viewport grid follows the unit ladder under a unit system, `grid_subdivisions` is inactive and `grid_scale_unit` reads 0.3048 under FEET
- Typed input parses `6'3"` under IMPERIAL, `inputs.use_numeric_input_advanced` enables expressions
