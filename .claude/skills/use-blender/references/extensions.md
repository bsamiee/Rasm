# [EXTENSIONS]

Extensions are packages Blender installs from a zip and imports as `bl_ext.<repository>.<id>`.

## [01]-[PACKAGE]

- Packages hold `blender_manifest.toml` beside an `__init__.py` whose `register` and `unregister` mirror each other
- Sibling modules import relatively (`from . import <module>`), each owning one concern, and `__init__.py` composes their classes
- Manifest keys `schema_version`, `id`, `name`, `tagline`, `version`, `type`, `maintainer`, `license`, and `blender_version_min` are required
- `id` is a Python identifier with no `__` and no leading or trailing `_`, `tagline` is at most 64 characters ending in an alphanumeric or bracket
- `platforms` stays absent on a pure-Python package, listing one hides the package elsewhere, `wheels` names `.whl` paths
- Loader checks run non-strict and load a manifest that fails `validate`, each manifest change runs `validate` before a build
- Folders linked into a repository import from the source and write bytecode into it, packages reach Blender as the zip `build` writes
- Local repositories take the folder name as the package id, a linked folder is named after the manifest id
- Reinstalling over an enabled copy pops the package and every submodule from `sys.modules`, and the new modules load with no restart
- Renamed packages leave a stale record and a dangling link, `addon_disable` on the old record, relink, `extensions.repo_refresh_all()`, enable, save

```bash
# Manifest check, exit 1 naming each invalid key or the missing blender_manifest.toml
blender --factory-startup -c extension validate <source>

# Zip named <id>-<version>.zip under an existing <dist>
blender --factory-startup -c extension build --source-dir <source> --output-dir <dist>
```

## [02]-[INSTALLS]

- Zip packages install live through `bpy.ops.extensions.package_install_files(filepath=<zip>, repo="user_default", enable_on_install=True)`
- Platform packages take `bpy.ops.extensions.repo_sync_all()`, then `package_install(repo_index=<i>, pkg_id="<id>", enable_on_install=True)`
- `<i>` is the index of `blender_org` in `preferences.extensions.repos`
- `bpy.ops.extensions.package_uninstall(repo_index=<i>, pkg_id="<id>")` removes one, installs enable in the call with no restart
- `blender -c extension install-file -r user_default -e <zip>` installs a package file with the manifest at its root, a directory is refused
- `blender --online-mode -c extension install --sync --enable blender_org.<id>` installs a platform package, empty resources start offline
- Legacy zips install through `preferences.addon_install(filepath=)` then `addon_enable(module=)` and `wm.save_userpref()`
- Add-ons that draw through `gpu` at import enable in a GUI run alone, ended by `wm.quit_blender()`
- Development runs under `BLENDER_USER_RESOURCES=<dir>`, apart from the user's packages, and background runs there call the installed operators
- Enabling a subset (`--factory-startup` with `addon_utils.enable`, `--addons bl_ext.<id>`, factory `-c extension install -e`) rewrites the wheels
- Shared wheels sit under `<EXTENSIONS>/.local/lib/python3.13/site-packages`, a run under the user's preferences restores the full set

## [03]-[REGISTRATION]

- Keymap items and panel re-registrations run in a timer `register()` adds with `first_interval=0.0`, after every add-on registered
- Add-on keymap items go in `keyconfigs.addon`, one keymap per name for every add-on, and `unregister` removes the exact `(keymap, item)` pairs
- Add-on items land at the head of the user keymap in reverse order of addition, a table written in reverse lands in the order written
- Chords are checked against every modifier combination of the stock items first, an add-on PRESS item ahead of a stock CLICK_DRAG item shadows it
- Wrappers of stock operators declare the stock properties (`get_rna_type().properties` less `OperatorProperties`)
- Wrapper items copy each stock item with `new_from_item`, then write `idname` and the set properties
- Draw handlers (`draw_handler_add(fn, (), "WINDOW", "POST_PIXEL")`) write a value only where it differs, an unguarded write redraws forever
- Most add-on panels declare no `bl_options`, `cls.bl_options` then raises `AttributeError`, and `getattr(cls, "bl_options", set())` reads it
- Registration outside `register()` stores an empty `owner_id`, and a class declaring `bl_owner_id` keeps its owner through every re-registration
- Owner of a class is the enabled add-on whose folder holds its module file, else the extension whose manifest `wheels` list installs the module
- Hidden core add-ons register with an empty owner id
- `bpy.types.__dir__()` lists registered types in registration order, `dir()` sorts it, the Cycles `RenderEngine` is registered and absent from it
- Pie draws pad every direction, a slice with nothing to draw takes a separator, and a raise inside `draw` leaves the pie empty
- Modal operators that read the next key set `workspace.status_text_set(<text>)` and clear it with `None`
