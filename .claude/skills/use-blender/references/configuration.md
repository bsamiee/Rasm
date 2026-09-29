# [CONFIGURATION]

Preferences, add-on records, repositories, keymaps, themes, fonts, and units, each written through the API in one process.

## [01]-[STORES]

- `bpy.utils.user_resource` takes `CONFIG`, `SCRIPTS`, `EXTENSIONS`, and `DATAFILES` under a `<major>.<minor>` folder
- `userpref.blend` in `CONFIG` holds preferences, themes, text styles, add-on records, repositories, keymap diffs, and keyconfig preferences
- `startup.blend` beside `userpref.blend` holds screens, workspaces, and the startup scene
- Window Manager ID properties save in no file
- `bpy.ops.wm.save_userpref()` writes at once, `--factory-startup` processes included
- `use_preferences_save` True rewrites `userpref.blend` at a GUI quit while `is_dirty` is set
- Background quits and `os._exit` write no preferences
- Theme, text-style, and add-on record changes tag dirty, `view.show_addons_enabled_only` and `extensions.active_repo` do not
- Last writer wins across processes, preference writes run in one process with no other Blender open
- `bpy.ops.wm.save_homefile()` saves the current screens, a background save keeps workspace data edits under the stored active workspace
- Workspaces active at `save_homefile` open the next session
- Add-on Scene properties save into `startup.blend` and every file made from it
- Disabled add-ons keep their Scene values in `scene.bl_system_properties_get()` until deleted there

## [02]-[PROPERTIES]

- Factory values come from a `--factory-startup` process, `prop.default` misreports `audio_device` and `font_directory`
- Dynamic enums (`display_device`, `view_transform`, `look`, `length_unit`, `temperature_unit`) list `NONE` or `DEFAULT` in `bl_rna`
- `render.engine` lists `BLENDER_EEVEE` alone in `bl_rna`
- `Property.is_deprecated` marks members due for removal (`Scene.compositing_node_group`), each read or write of one prints a `DeprecationWarning`
- `show_statusbar_vram`, `support_emulation`, and `use_remote_asset_libraries` raise `AttributeError` on write
- `system.dpi`, `ui_scale`, `pixel_size`, and `ui_line_width` are runtime results of `view.ui_scale` and `view.ui_line_width`
- `system.gpu_preferred_device` reads METAL in the GUI and AUTO headless
- `FileSelectParams.directory` is a byte string that reads back with a trailing separator, writes take `f"{folder}/".encode()`
- `PASSWORD` subtype properties (`access_token`, `auth_token`, API keys) store plain text and leave every report
- `bpy.data.project_init(name=, project_root=)` makes a `.blender_project` root
- `bpy.data.project` holds `name`, `root_path`, `variables`, and `asset_libraries`, `None` outside a project

## [03]-[ADDONS]

- `preferences.addons` holds enabled add-ons alone, `addon_utils.modules()` lists every installed module
- `preferences.addons[<module>].preferences` holds an add-on's preference group, `None` for an add-on that failed to register
- Module names are `bl_ext.<repository>.<id>` for an extension, the module for a core add-on, and the zip's top folder or stem for a `bl_info` add-on
- `bl_info` module names hold the zip's branch or version suffix, `{m.bl_info["name"]: m.__name__ for m in addon_utils.modules()}` resolves one
- `__name__.rpartition(".")[2]` is an extension's id
- `preferences.addon_disable` unregisters, removes the record, and frees its preference group, a later enable starts from the add-on's defaults
- `addon_disable` on a record with a missing module removes the record
- Records without a module log `Add-on not loaded` at every start
- `bl_pkg`, `io_anim_bvh`, `io_curve_svg`, `io_mesh_uv_layout`, and `io_scene_fbx` load at every start whatever their record says
- `bl_pkg` holds the extension system
- Add-ons that fail in `--background` keep their record with `loaded` False
- Dimension add-ons take their imperial precision as a denominator string, Bonsai `doc.imperial_precision` as `1/<n>`

## [04]-[REPOSITORIES]

- `preferences.extensions.repos` hold `blender_org`, `user_default`, `system`, and added remotes, each `module` naming its `bl_ext.<module>` package
- `repos[i].module` stays unwritten, a write moves the repository's folder to `extensions/<module>` and drops its add-on records on a failed re-enable
- `system.use_online_access` gates `repo_sync_all`, `--offline-mode` locks it for one launch
- GUI writes to `remote_url`, `use_access_token`, or `access_token` start a network sync at once when online, written on difference alone
- `use_cache` True keeps downloaded archives under `.blender_ext/cache/`, False deletes them after install and clears the cache before an upgrade
- GUI starts under `view.show_extensions_updates` sync a `use_sync_on_startup` repository with packages and an `index.json` missing or over 24 h old
- `show_extensions_updates` off hides the blocked-extension alert and the offline icon with the update count

## [05]-[KEYMAPS]

- `preferences.keymap.active_keyconfig` names the preset, `keyconfigs.active.preferences` holds `spacebar_action` and `use_pie_click_drag`
- `keyconfigs.user` rebuilds at load as the default keyconfig and add-on items with the stored diffs applied, `keymap.is_user_modified` flags a diff
- First keymap item with an operator polling true consumes an event
- 3D Viewport regions run the tool keymap, mode keymaps, `Object Non-modal`, `Frames`, `3D View Generic`, and `3D View`, then area and window handlers
- Add-on item added and a user keymap edited in the same event-loop pass records the item as removed in the diff, one tick between them keeps it
- User keymap edits enter the diff at the next `keyconfigs.update()`, `is_user_modified` reads False until then
- `restore_to_default()` then `keyconfigs.update()` merges dropped add-on items back and drops every user edit of that keymap
- Writing `KeyMapItem.idname` resets its properties, a copy writes `idname` first and then each property `is_property_set` on the source
- Removing an operator type (add-on rename or reload) frees `properties` of every keymap item pointing at it, a later read crashes Blender
- Context menus sit on RIGHTMOUSE PRESS as `wm.call_menu` and `wm.call_panel` items, `CLICK` keeps a still click opening them under a CLICK_DRAG item
- Cmd copies of Ctrl items exist on macOS beside the Ctrl items, and no held modifier leaves every letter free
- Ctrl H, M, W, Space, Accent Grave, Period, and Tab without Shift or Alt, and Ctrl Alt Q, have no Cmd copy
- Background keyconfigs hold the C keymaps alone, `bpy.utils.keyconfig_set(bpy.utils.preset_find("Blender", "keyconfig"))` fills the default one

## [06]-[THEME]

- Theme colors store bytes exposed as `COLOR_GAMMA` floats, a write of `byte / 255` reads back quantized
- `View3DShading.single_color` and `object_outline_color` are `COLOR` properties holding scene linear values
- Add-on colors drawn through the `gpu` module read as sRGB whatever their subtype
- Preset clicks run `reset_default_theme` then the preset XML, `Blender_Dark.xml` is a pure reset, and every override written before it goes
- `reset_default_theme` rebuilds `ui_styles` to 11 points and weight 400, shadow 3 on `panel_title` and 1 on `widget` and `tooltip`
- Write text styles after the theme
- Shadow offsets, alpha, and value of a text style do nothing while its `shadow` is 0
- 3D grid lines draw the theme color shaded +10 for `grid` and +20 for `grid_major` while the major line stays brighter than the canvas
- Hovered menu rows draw `int(0.8 * inner + 0.2 * text)`, a selected list item draws `0.5 * inner_sel + 0.5 * outline`
- Outliner hover is white at alpha 0.13 over any row, with no theme member
- `wcol_state.blend` mixes the state color into a field, only the `_sel` members of the `inner_*` state colors draw
- `user_interface.gizmo_hi` is the hover color of every viewport gizmo, live with no control on the Themes page
- `themes[0].filepath` names a preset file, `bl_pkg` reapplies a changed file and resets the theme for a vanished one after extension operations
- Theme XML naming a member the build lacks loads the rest and prints one `<Type>.<attr> not found` line per member
- `ThemeSpaceGeneric` holds `back`, `header`, `text`, and `text_hi`

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

## [08]-[UNITS]

- Assigning `unit_settings.system` resets `length_unit` and `mass_unit` to that system's defaults, `system` writes before the other fields
- Unit fields take the system's items alone, `temperature_unit = "CELSIUS"` raises under `IMPERIAL`
- Unit fields take one assignment each, a tuple assignment that raises leaves its earlier targets written
- `view_distance` stores meters with unit NONE, and render pixel sizes have no imperial form
- Grid steps follow the unit table, perspective views from the base unit up and axis views with every smaller unit, `overlay.grid_scale` scales them
- `grid_subdivisions` is inactive under a unit system, and `grid_scale_unit` reads 0.3048 under FEET
- Imperial text parses as a sum of unit-marked terms with `-` and `+` as arithmetic and a bare number as feet, `5' 3-1/2"` reads 95.5 in
- `5' 3.5"`, `5' 3" 1/2"`, and `5' 3"+1/2"` read 63.5 in
- Imperial display and `bpy.utils.units.to_string` pick the unit by magnitude (thou, mi, sq yd, cu yd, st, tn), 1/16" reading 62.5 thou
- `precision` in `bpy.utils.units.to_string` counts significant digits
