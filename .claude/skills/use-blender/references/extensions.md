# [EXTENSIONS]

Extensions are packages Blender installs from a zip and imports as `bl_ext.<repository>.<id>`.

## [01]-[PACKAGE]

- `platforms` stays absent on a pure-Python package, listing one hides the package elsewhere
- Loader checks run non-strict and load a manifest failing `validate`, `build` refuses it with the same errors
- Folders linked into a repository import from the source, packages reach Blender as the zip `build` writes
- Local repositories take the folder name as the package id, a linked folder is named after the manifest id
- Reinstalling over an enabled copy pops the package and every submodule from `sys.modules`, the new modules load with no restart
- Reinstalls keep the preference group's values
- Renamed packages take `addon_disable` on their earlier module, a relink, `extensions.repo_refresh_all()`, an enable, and a save

```bash
# Manifest check, exit 1 naming each invalid key or the missing blender_manifest.toml
blender --factory-startup -c extension validate <source>

# Zip named <id>-<version>.zip under an existing <dist>
blender --factory-startup -c extension build --source-dir <source> --output-dir <dist>
```

## [02]-[INSTALLS]

- Zip packages install live through `bpy.ops.extensions.package_install_files(filepath=<zip>, repo="user_default", enable_on_install=True)`
- Platform packages take `extensions.repo_sync_all()`, then `package_install(repo_index=<blender_org>, pkg_id="<id>", enable_on_install=True)`
- `package_install` on an installed id upgrades it to the synced index's newest compatible version and keeps its enabled state
- `bpy.ops.extensions.package_uninstall(repo_index=<i>, pkg_id="<id>")` removes a package before returning
- Installs enable with no restart
- Uninstalls remove the record, the preference group, the package folder, the cached archive, and `extensions/.user/<repo>/<id>`
- `https://extensions.blender.org/api/v1/extensions/?blender_version=<version>&platform=<platform>` lists one newest compatible row per id
- `blender -c extension install-file -r user_default -e <zip>` installs a package file with the manifest at its root
- `install-file` on a directory prints `ERROR ... Is a directory` at exit 0
- `blender --online-mode -c extension install --sync --enable blender_org.<id>` installs a platform package, the sync fails without `--online-mode`
- `-c extension` without `--factory-startup` loads the user's preferences and prints every add-on's register output
- `bl_info` add-on zips install through `preferences.addon_install(filepath=)`, then `addon_enable(module=)` and `wm.save_userpref()`
- `preferences.addon_remove(module=)` redraws `context.area`, a call from a timer runs it under a `temp_override` holding an area
- Add-ons that draw through `gpu` at import enable in a GUI run alone, ended by `wm.quit_blender()`
- Every process on the user's tree but `-c extension validate` and `build` resyncs the shared wheels to the extensions it enables
- `--factory-startup -c extension install-file -e` saves `userpref.blend` with factory records and the installed package alone
- Shared wheels sit under `<EXTENSIONS>/.local/lib/python<version>/site-packages` of Blender's Python

## [03]-[REGISTRATION]

- Keymap items, panel re-registrations, and file data reads run in a timer `register()` adds with `first_interval=0.0`, after every add-on registered
- Startup registration sees `bpy.data` as `_RestrictData`, a read of `bpy.data.objects` inside `register()` raises `AttributeError`
- Add-on keymap items go in `keyconfigs.addon`, one keymap per name for every add-on, and `unregister` removes the exact `(keymap, item)` pairs
- Add-on items go to the head of the user keymap in reverse order of addition, a table written in reverse ends in the order written
- Chords are checked against every modifier combination of the stock items first, an add-on PRESS item ahead of a stock CLICK_DRAG item shadows it
- Wrappers of stock operators declare the stock properties (`get_rna_type().properties` less `OperatorProperties`)
- Wrapper items copy each stock item with `new_from_item`, then write `idname` and the set properties
- Draw handlers (`draw_handler_add(fn, (), "WINDOW", "POST_PIXEL")`) write a value only where it differs, an unguarded write redraws forever
- `cls.bl_options` raises `AttributeError` on a panel that declares none, `getattr(cls, "bl_options", set())` reads it
- Registration outside `register()` stores an empty `owner_id`, and a class declaring `bl_owner_id` keeps its owner through every re-registration
- Owner ids name the add-on module that registered the class in its `register()`, a wheel module's classes included
- Hidden core add-ons register with an empty owner id
- `bpy.types.__dir__()` lists registered types in registration order, `dir()` sorts it, the Cycles `RenderEngine` is registered and absent from it
- Pie draws pad every direction, a slice with nothing to draw takes a separator, and a raise inside `draw` leaves the pie empty
- `layout.label_multiline(text=, max_lines=, alignment=)` wraps a label to the region width, `label_markdown(text=)` draws markdown
- Modal operators reading the next key pass `workspace.status_text_set()` a text or `draw(header, context)`, and `None` at every exit and `cancel()`
- Status bar draws lay out one row across the window, labels past the window edge clip unseen
- Pie and status-bar draw functions run on every redraw, a pie on every MOUSEMOVE
- Operators resolve what a pie or status bar draws once in `invoke`
