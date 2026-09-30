# [EXTENSIONS]

Extensions and `bl_info` add-ons from source folder to registered package, proven in their own tree before the user's Blender takes them.

## [01]-[PACKAGE]

New extensions are one folder named after the manifest id, checked and zipped by Blender's own commands:
- Folders hold `blender_manifest.toml` beside `__init__.py`, siblings import relatively, and installs import as `bl_ext.<repository>.<id>`
- Required keys are `schema_version`, `id`, `name`, `tagline`, `version`, `type`, `maintainer`, `license`, and `blender_version_min`
- `platforms` stays absent on a pure-Python package, a listed platform hides the package on every platform the list omits
- `wheels` lists `.whl` paths of unmodified PyPI wheels, one per dependency and platform tag, and Blender reads no wheel metadata
- `validate` runs before every build, failing a `tagline` over 64 characters the loader accepts
- `build` skips linked files without a message, and a folder holding links builds from a copy that resolves them
- `validate` and `build` run on the user's tree and leave its shared wheels as they are
- `--split-platforms` writes one `<id>-<version>-<platform>.zip` per manifest platform

```bash
# Manifest check, exit 1 naming each invalid key
blender --factory-startup -c extension validate <source>

# Copy with links resolved, <id>-<version>.zip under <dist>, then its members
cp -RL <source> <copy>
mkdir -p <dist>
blender --factory-startup -c extension build --source-dir <copy> --output-dir <dist>
unzip -l <dist>/<id>-<version>.zip
```

`packaged` in `tools/interface/blender/packages.py` builds the repository's extension from a resolved copy of `extension/` through `--output-filepath`.

## [02]-[ISOLATED_TREE]

Processes that enable packages resync `<EXTENSIONS>/.local` to their enabled set, and installs and registration runs take their own tree:
- `headless.py run` is that tree headless, a factory process with extensions, config, and scripts under `.artifacts/blender/`
- Runs that load saved records (a startup enable, a reinstall, a GUI) take `BLENDER_USER_RESOURCES` on an existing `<tree>` folder
- GUI runs add `TMPDIR` on an existing `<tree>/tmp`, where `wm.quit_blender` writes `quit.blend` while the temporary directory preference is empty
- GUI scripts step through one timer generator and end in `wm.quit_blender()`, the quit saving preferences into `<tree>/config`
- Headless enables register classes and draw functions with no keymap item from the timer tick, and keymaps read in a GUI

```bash
# Built zip installed, enabled, read, and uninstalled in the run tree
python .claude/skills/use-blender/scripts/headless.py run <file> <<'PY'
import addon_utils
import bpy

repo = bpy.context.preferences.extensions.repos["User Default"]
bpy.ops.extensions.package_install_files(filepath="<zip>", repo="user_default", enable_on_install=True)
state = {"check": addon_utils.check("bl_ext.user_default.<id>"), "operators": dir(bpy.ops.<prefix>)}
bpy.ops.extensions.package_uninstall(repo_directory=repo.directory, pkg_id="<id>")
result = state
PY

# Background process on the tree's saved records
env BLENDER_USER_RESOURCES=<tree> blender --background --python <script>

# GUI on the tree running <script> on <file>, open returning at its quit
env BLENDER_USER_RESOURCES=<tree> TMPDIR=<tree>/tmp /usr/bin/open -n -g -W -a Blender --stdout <tree>/gui.log --stderr <tree>/gui.err --args --no-window-focus <file> --python <script>
```

## [03]-[INSTALLS]

Each package kind has one command line and one call inside a running Blender:

```bash
# Built zip into user_default, enabled, preferences saved
env BLENDER_USER_RESOURCES=<tree> blender -c extension install-file -r user_default -e <zip>

# Platform package synced, installed, enabled, preferences saved
env BLENDER_USER_RESOURCES=<tree> blender --online-mode -c extension install --sync --enable blender_org.<id>
```

```python
# [HEADLESS_CALL] Built zip, platform package, and bl_info zip installed and enabled, the records saved
import addon_utils
import bpy

preferences = bpy.context.preferences
preferences.system.use_online_access = True
repo = preferences.extensions.repos["extensions.blender.org"]
bpy.ops.extensions.package_install_files(filepath="<zip>", repo="user_default", enable_on_install=True)
bpy.ops.extensions.repo_sync_all()
bpy.ops.extensions.package_install(repo_directory=repo.directory, pkg_id="<id>", enable_on_install=True)
bpy.ops.preferences.addon_install(filepath="<bl_info zip>")
bpy.ops.preferences.addon_enable(module="<top folder>")
bpy.ops.wm.save_userpref()
result = {name: addon_utils.check(name) for name in ("bl_ext.user_default.<id>", "bl_ext.blender_org.<id>", "<top folder>")}
```

- Background calls leave the records unsaved until `wm.save_userpref()`, the `-c extension` commands save their own
- Platform installs need online access, `--online-mode` on a launch or `system.use_online_access = True` in a running process
- `bl_info` modules take the name of the zip's top folder
- `bl_info` add-ons that build a GPU shader at import enable in a GUI run alone
- User-tree packages are `packages.toml` rows in `tools/interface/blender/`, which `nx run rasm:interface -- blender` stages and converges
- `nx run rasm:interface -- upgrade blender` stages the newest build of every row for the next apply

## [04]-[UPGRADES]

Reinstalls over an enabled copy load the new modules in the running process:
- Extension installs over an enabled copy disable it, drop the package and its submodules from `sys.modules`, and enable it again, preferences kept
- `enable_on_install` decides nothing for an enabled copy, which returns enabled
- `-c extension update --sync` upgrades every package
- `bl_info` reinstalls take the snippet, `addon_install` keeping loaded submodules and `package_install_files` enabling nothing
- Renamed packages take `preferences.addon_disable(module=<old module>)`, the install under the new id, an enable, and `wm.save_userpref()`

```python
# [HEADLESS_CALL] bl_info add-on reinstalled over its enabled copy with the new submodules loaded
import sys

import addon_utils
import bpy

module = "<module>"
addon_utils.disable(module)
for name in [name for name in sys.modules if name == module or name.startswith(f"{module}.")]:
    del sys.modules[name]
bpy.ops.preferences.addon_install(filepath="<zip>")
addon_utils.enable(module, default_set=True)
result = {"check": addon_utils.check(module)}
```

## [05]-[REMOVAL]

Removals take the call of their package kind:
- Extensions take `extensions.package_uninstall(repo_directory=<repo>.directory, pkg_id="<id>")`, which removes folder, cached archive, and user data
- Uninstalls resync the shared wheels, the last package listing a wheel removing its module from every process on the tree
- Core add-ons take `addon_utils.disable(<module>, default_set=True)`
- `bl_info` add-ons take `preferences.addon_remove(module=)` under an area override, the loaded modules staying in `sys.modules` until a restart
- User-tree packages leave with their `packages.toml` row at the next apply

```python
# [EXECUTE_BLENDER_CODE] bl_info add-on removed, the call ending in a redraw of the override's area
import bpy

window = bpy.context.window_manager.windows[0]
with bpy.context.temp_override(window=window, screen=window.screen, area=window.screen.areas[0]):
    status = bpy.ops.preferences.addon_remove(module="<module>")
result = {"status": sorted(status)}
```

## [06]-[REGISTRATION]

`register()` runs before any file loads, with `bpy.context` narrowed to `window_manager` and `preferences` and `bpy.data` empty:
1. Register classes through `bpy.utils.register_classes_factory(<classes>)`, draw functions through `prepend` or `append`, and `@persistent` handlers
2. Register the tick as `bpy.app.timers.register(<first_tick>, first_interval=0.0, persistent=True)`, which a launch naming a `.blend` keeps
3. In the tick, add keymap items from `keyconfigs.default`, re-register panels, and read file data, every add-on registered by then
4. Pair each step with its reversal in one `contextlib.ExitStack`, `unregister` closing it, as `registration()` in the extension's `__init__.py` does

Keymap items join the add-on keyconfig and leave through the keymap that added them:
- `keyconfigs.addon.keymaps.new(name=, space_type=, region_type=)` returns the one add-on keymap of that name every add-on shares
- `keymap_items.new(<idname>, <type>, <value>, alt=True)` adds an item, `new_from_item(<stock item>)` copies one with its properties for field writes
- `register` keeps each `(keymap, item)` pair, and `unregister` calls `keymap.keymap_items.remove(item)` on each pair and keeps the keymap
- Items sit at the head of the user keymap in reverse order of addition, ahead of every stock item, with no macOS Cmd copy
- Modal bindings join an existing modal keymap, the add-on keyconfig creates none
- Items return one tick after a GUI disable and enable

Panels, draw handlers, timers, and status text each take one form:
- Panel classes declare `bl_owner_id`, which keeps the owner through a re-registration from the tick, where the owner otherwise reads empty
- Panel options read through `getattr(cls, "bl_options", ())`, most add-on panels declaring none
- Panel trees leave deepest-first and return parent-first, `panels.collapsed` in the extension restoring each class's prior attributes at exit
- `Space<Type>.draw_handler_add(<fn>, <args>, "<REGION>", "POST_PIXEL")` draws in every region of that type in every window
- Draw handlers leave through `draw_handler_remove(<handle>, "<REGION>")` at unregister
- RNA writes inside a draw handler go on difference, an equal-value write tags a redraw every frame
- Timers match by callable identity, and `unregister` removes one while `bpy.app.timers.is_registered(<fn>)` reads true
- Modal operators pass `workspace.status_text_set` a `(header, context)` function in `invoke` and `None` at every exit and in `cancel()`
- Status bar and pie draws run on every redraw, a pie on every pointer move, and `invoke` resolves what they show once
- `INTERFACE_OT_alias` in the extension's `commands.py` is the working modal status form

## [07]-[READING]

Registration reads run in the process that registered, a tree's GUI through its script and the user's GUI through `execute_blender_code`:

```python
# [EXECUTE_BLENDER_CODE] Add-on keymap items, the user keymap head, owned panels, and draw functions of a menu
import bpy

keyconfigs = bpy.context.window_manager.keyconfigs
panels = [cls for name in bpy.types.__dir__() if isinstance(cls := getattr(bpy.types, name), type) and issubclass(cls, bpy.types.Panel) and cls is not bpy.types.Panel]
result = {
    "items": [(keymap.name, item.idname, item.type, item.value, item.alt) for keymap in keyconfigs.addon.keymaps for item in keymap.keymap_items if item.idname.startswith("<prefix>.")],
    "head": [(index, item.idname, item.type) for index, item in enumerate(keyconfigs.user.keymaps["3D View"].keymap_items[:5])],
    "owned": sorted(cls.bl_rna.identifier for cls in panels if vars(cls).get("bl_owner_id") == "<module>"),
    "draws": [function.__module__ for function in bpy.types.<UIClass>._dyn_ui_initialize()],
}
```

- `bl_owner_id` reads on classes that declare it or that a re-registration wrote, other panels keep their owner outside Python
- `_dyn_ui_initialize()` lists a UI class's draw functions in draw order, the class's own draw among them
