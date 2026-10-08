# [EXTENSIONS]

Extensions and `bl_info` add-ons from registration code to installed package, proven in their own tree before the user's Blender takes them.

## [01]-[REGISTRATION]

`register()` runs before any file loads, with `bpy.context` narrowed to `window_manager` and `preferences` and `bpy.data` empty:
1. Register classes through `bpy.utils.register_classes_factory(<classes>)`, draw functions through `prepend` or `append`, and `@persistent` handlers
2. Register the tick as `bpy.app.timers.register(<first_tick>, first_interval=0.0, persistent=True)`, which a launch naming a `.blend` keeps
3. In the tick, add keymap items from `keyconfigs.default`, re-register panels, and read file data, every add-on registered by then
4. Pair each step with its reversal in one `contextlib.ExitStack`, `unregister` closing it, as `registration()` in the extension's `__init__.py` does

Keymap items join the add-on keyconfig and leave through the keymap that added them:
- `keyconfigs.addon.keymaps.new(name=, space_type=, region_type=)` returns the one add-on keymap of that name every add-on shares
- `keymap_items.new(<idname>, <type>, <value>, alt=True)` adds an item, `new_from_item(<stock item>)` copying one with its properties for field writes
- `register` keeps each `(keymap, item)` pair, and `unregister` calls `keymap.keymap_items.remove(item)` on each pair and keeps the keymap
- Modal bindings join an existing modal keymap
- Items return one tick after a GUI disable and enable

Panels, draw handlers, timers, and status text each take one form:
- Panel classes declare `bl_owner_id`, which keeps the owner through a re-registration from the tick, where the owner otherwise reads empty
- Panel options read through `getattr(cls, "bl_options", ())`, most add-on panels declaring none
- Registration appends a panel type after every panel of equal `bl_order`
- Re-registering a class with `DEFAULT_CLOSED` closes it in regions that never drew it, stored records keeping their flag and `sortorder`
- `HIDE_HEADER` panels collapse once `HIDE_HEADER` leaves their `bl_options`
- Panel trees leave deepest-first and return parent-first, `panels.collapsed` in the extension restoring each class's prior attributes at exit
- `Space<Type>.draw_handler_add(<fn>, <args>, "<REGION>", "POST_PIXEL")` draws in every region of that type in every window
- Draw handlers leave through `draw_handler_remove(<handle>, "<REGION>")` at unregister
- RNA writes inside a draw handler go on difference, an equal-value write tagging a redraw every frame
- Timers match by callable identity, and `unregister` removes one while `bpy.app.timers.is_registered(<fn>)` reads true
- Modal operators pass `workspace.status_text_set` a `(header, context)` function in `invoke` and `None` at every exit and `cancel()`
- `INTERFACE_OT_alias` in the extension's `commands.py` does so
- Status bar and pie draws run on every redraw, a pie on every pointer move, and `invoke` resolves what they show once

## [02]-[PACKAGE]

Use `nx run <project>:pack` for repository extension projects.

Project manifests declare `platforms`. Packing adds `wheels` to staged manifests and writes platform archives under `.artifacts/blender/<project>/`.

Prepared extensions build from a folder containing `blender_manifest.toml`. Default file discovery skips symbolic links:

```bash
cp -RL <source> <copy>
mkdir -p <dist>
blender --factory-startup -c extension build --source-dir <copy> --output-dir <dist>
unzip -l <dist>/<id>-<version>.zip
```

- `blender_manifest.toml` sits beside add-on `__init__.py` or theme XML
- Installed add-ons import as `bl_ext.<repository>.<id>` and import sibling modules relatively
- Use [Blender's manifest documentation](https://docs.blender.org/manual/en/latest/advanced/extensions/getting_started.html) for required keys
- `platforms` restricts package availability regardless of implementation language, with omission allowing every platform
- `wheels` lists `./wheels/<file>.whl` paths for bundled dependencies, including transitive dependencies Blender does not supply
- Bundled wheels must cover every supported platform
- Blender selects wheels by filename tags and resolves no dependencies from wheel metadata
- `build` validates manifests, tags, and add-on root `__init__.*` before writing
- `build` limits `tagline` to 64 characters where loading accepts more
- Build output folders must exist before the command runs
- `validate <source-or-zip>` checks a folder or archive without building and exits 1 on invalid metadata
- `validate` and `build` run on the user's tree and leave its shared wheels as they are
- `--split-platforms` requires `platforms` and writes `<id>-<version>-<platform>.zip` per entry, replacing platform hyphens with underscores
- Split archives hold compatible wheels and `[build.generated]` metadata restricting installation to their platform

Blender's [wheel requirements](https://docs.blender.org/manual/en/latest/advanced/extensions/python_wheels.html) require unmodified PyPI wheels.

Use `tools/interface/blender/packages.py` for interface extension packaging.

## [03]-[ISOLATED_TREE]

Processes enabling packages resync `<EXTENSIONS>/.local` to their enabled set, installs and registration runs taking a tree by what they load:

| [INDEX] | [RUN]                                             | [TREE]                                                              |
| :-----: | :------------------------------------------------ | :------------------------------------------------------------------ |
|  [01]   | Install and registration read, headless           | `headless.py run` on any file                                       |
|  [02]   | Saved records loaded at start (enable, reinstall) | `BLENDER_USER_RESOURCES` on an existing `<tree>` folder, background |
|  [03]   | Keymap items, GPU shaders, panels drawn           | GUI on `<tree>`                                                     |

```bash
# Built zip installed, enabled, called, and uninstalled in the run tree
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
env BLENDER_USER_RESOURCES=<tree> /usr/bin/open -n -g -W -a Blender --stdout <tree>/gui.log --stderr <tree>/gui.err --args --no-window-focus <file> --python <script>
```

- Installs and uninstalls print `STATUS Installed "<id>"` and `STATUS Removed "<id>"` on `stdout`
- GUI scripts step through one timer generator and end in `wm.quit_blender()`, which saves preferences into `<tree>/config` under `is_dirty`
- Headless enables register classes and draw functions, and the timer tick adding keymap items runs in a GUI

## [04]-[INSTALLS]

Use `nx run <project>:install` for repository extension projects.

Install through Blender running on `<tree>`. Background installs resync shared wheels. GUI exit can overwrite preferences saved by another process.

With no Blender running on `<tree>`, use the command for each package kind:

```bash
# Built zip into user_default, enabled, preferences saved
env BLENDER_USER_RESOURCES=<tree> blender -c extension install-file -r user_default -e <zip>

# Platform package synced, installed, enabled, preferences saved
env BLENDER_USER_RESOURCES=<tree> blender --online-mode -c extension install --sync --enable blender_org.<id>
```

```python
# [EXECUTE_BLENDER_CODE] Built zip, platform package, and bl_info zip installed and enabled
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
result = {name: addon_utils.check(name) for name in ("bl_ext.user_default.<id>", "bl_ext.blender_org.<id>", "<top folder>")}
```

- `system.use_online_access` gates every sync and platform install, a write in a running process setting `bpy.app.online_access`
- `--online-mode` and `--offline-mode` set online access for one launch, `--offline-mode` locking the preference
- Background calls leave the records unsaved until `wm.save_userpref()`, the `-c extension` commands saving their own
- `bl_info` modules take the name of the zip's top folder
- `bl_info` add-ons that build a GPU shader at import enable in a GUI run
- User-tree packages are `packages.toml` rows in `tools/interface/blender/`, which `nx run rasm:interface -- blender` stages and converges

## [05]-[UPGRADES]

Reinstalls over an enabled copy load the new modules in the running process:
- Extension installs over an enabled copy disable it, drop the package and its submodules from `sys.modules`, and enable it again, preferences kept
- Enabled copies return enabled whatever `enable_on_install` holds
- `package_install` on an installed id takes the newest compatible version of the synced index
- `-c extension update --sync` upgrades every package
- `nx run rasm:interface -- upgrade blender` stages the newest build of every `packages.toml` row for the next apply
- Renamed packages take `preferences.addon_disable(module=<old module>)`, the install under the new id, and an enable
- `bl_info` reinstalls take the snippet, the disable and `sys.modules` pass loading the new submodules that `addon_install` alone keeps old

```python
# [EXECUTE_BLENDER_CODE] bl_info add-on reinstalled over its enabled copy with new submodules loaded
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

## [06]-[READING]

Registration reads run in the process that registered, a tree's GUI through its script and the user's GUI through `execute_blender_code`:

```python
# [EXECUTE_BLENDER_CODE] Add-on keymap items, the user keymap head, owned panels, and draw functions of a UI class
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

- `bl_owner_id` reads on classes that declare it or that a re-registration wrote, other panels keeping their owner outside Python
- `_dyn_ui_initialize()` lists a UI class's draw functions in draw order, the class's own draw among them

## [07]-[REMOVAL]

Removals take the call of their package kind:
- Extensions take `extensions.package_uninstall(repo_directory=<repo>.directory, pkg_id="<id>")`, which removes folder, cached archive, and user data
- Uninstalls resync the shared wheels, the last package listing a wheel removing its module from every process on the tree
- Core add-ons take `addon_utils.disable(<module>, default_set=True)`
- `bl_info` add-ons take `preferences.addon_remove(module=)` under an area override, which deletes the module file or folder alone
- `addon_remove` leaves loaded modules in `sys.modules` until a restart
- User-tree packages leave with their `packages.toml` row at the next apply

```python
# [EXECUTE_BLENDER_CODE] bl_info add-on removed, the call ending in a redraw of the override's area
import bpy

window = bpy.context.window_manager.windows[0]
with bpy.context.temp_override(window=window, screen=window.screen, area=window.screen.areas[0]):
    status = bpy.ops.preferences.addon_remove(module="<module>")
result = {"status": sorted(status)}
```
