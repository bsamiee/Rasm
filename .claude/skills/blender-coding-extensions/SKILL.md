---
name: blender-coding-extensions
description: "Use when writing or reviewing a Blender extension's registration, preferences, or manifest, covering lifecycle and packaging"
---

# [BLENDER_CODING_EXTENSIONS]

Covers what a Blender extension acquires in `register()` and releases in `unregister()`, the state it keeps across lifecycle events, and its manifest and wheels.

- Use `blender-coding` for callback bodies, properties, and operator classes
- Use `blender-coding-interface` for panels, menus, lists, and interface text
- Use `use-blender` for installing, validating, and running extensions
- Use `manage-repo` for extension assembly and imports

## [01]-[REGISTRATION]

Registration runs under host conditions a release stack answers:
- Module import and `register()` run under `RestrictBlend`
- `RestrictBlend` leaves `bpy.context` holding `window_manager` and `preferences` alone, and `bpy.data` reads raise `AttributeError`
- When `register()` raises, Blender calls no `unregister()`, and classes, handlers, and timers acquired before the raise stay live
- Submodules a raising `register()` imported stay in `sys.modules`, the next enable running their failing code until they leave
- Next enables append handlers and timers again and refuse a class a kept submodule still registers with `ValueError`
- `bpy.utils.register_classes_factory` stops at a raise with earlier classes registered, and an extension registers each class on its own stack
- Pointers or collections naming an unregistered group raise `ValueError` after the print with the class registered, and releases test `is_registered`

`acquired` enters every acquisition into one `ExitStack` and yields once the last succeeds, `register()` entering it into the module release stack and `unregister()` closing that stack:

```python
from collections.abc import Generator
from contextlib import ExitStack, contextmanager
from sys import platform

from bl_keymap_utils.io import keymap_init_from_data
from bl_keymap_utils.platform_helpers import keyconfig_data_oskey_from_ctrl_for_macos
import bpy
from expression import identity

type Keymaps = list[tuple[str, dict[str, str], dict[str, list[tuple[str, dict[str, object], dict[str, list[tuple[str, object]]]]]]]]


@contextmanager
def acquired() -> Generator[None]:
    def unregistered(cls: type[bpy.types.PropertyGroup | bpy.types.Operator | bpy.types.Panel]) -> None:
        if cls.is_registered:  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            bpy.utils.unregister_class(cls)

    keyconfigs: bpy.types.KeyConfigurations = bpy.context.window_manager.keyconfigs  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    keymaps: Keymaps = (keyconfig_data_oskey_from_ctrl_for_macos if platform == "darwin" else identity)(  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        [("Object Mode", {}, {"items": [(ITEM_OT_run.bl_idname, {"type": "J", "value": "PRESS", "ctrl": 1}, {"properties": [("amount", 2)]})]})]
    )
    with ExitStack() as stack:
        stack.callback(keyconfigs.update, keep_properties=True)
        for cls in (Settings, ITEM_OT_run, ITEM_PT_settings):
            stack.callback(unregistered, cls)
            bpy.utils.register_class(cls)
        bpy.types.Scene.item_settings = bpy.props.PointerProperty(type=Settings)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        stack.callback(delattr, bpy.types.Scene, "item_settings")
        bpy.app.handlers.load_post.append(loaded)
        stack.callback(bpy.app.handlers.load_post.remove, loaded)
        for keymap, content in ((keyconfigs.addon.keymaps.new(name, **params), content) for name, params, content in keymaps):  # type: ignore[union-attr, arg-type]  # ty: ignore[unresolved-attribute, invalid-argument-type]
            present = len(keymap.keymap_items)
            keymap_init_from_data(keymap, content["items"])
            for item in keymap.keymap_items[present:]:
                stack.callback(keymap.keymap_items.remove, item)
        yield


releases = ExitStack()


def register() -> None:
    releases.enter_context(acquired())


unregister = releases.close
```

Snippet placeholders stand for extension declarations and values:
- `Settings` for a `PropertyGroup`, `ITEM_OT_run` for an operator with an `amount` property, and `ITEM_PT_settings` for a panel
- `item` for the manifest `id`, and `loaded` for a `@bpy.app.handlers.persistent` function

Acquisitions follow host dependency order, and releases run in reverse, a group unregistered while an attachment names it crashing Blender at the attachment's next read:

| [INDEX] | [FIRST]         | [THEN]                                                   | [OTHERWISE]                                   |
| :-----: | :-------------- | :------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `PropertyGroup` | Class or attachment naming it in a pointer or collection | `ValueError` at registration                  |
|  [02]   | Parent panel    | Panel naming it in `bl_parent_id`                        | `RuntimeError` at registration                |
|  [03]   | Operator        | Keymap item calling it                                   | Item property writes raising `AttributeError` |

## [02]-[CLASSES]

Registered classes take the forms Blender checks at `register_class` and at each call:
- Operator `bl_idname` takes `<category>.<name>` in lowercase letters, digits, and underscores, one dot, at most 60 characters, else `RuntimeError`
- Callbacks declaring more or fewer positional parameters than the host passes raise `ValueError` at `register_class`, `*args` counting none (Panel `draw` takes 2)
- `__init__` overrides take `*args, **kwargs` and pass them to `super().__init__` first, a narrower signature reaching callers as `RuntimeError`
- Blender builds a new operator, panel, and menu instance per call, and instance attributes reach no later call
- Panel and menu `bl_idname` values register cut to 63 bytes, and a class under a held idname replaces the earlier class with an `Info` line
- Second `register_class` calls on one class raise `ValueError`
- Unregistering an operator class while it runs modal leaves its `window.modal_operators` entry reading `None` until the next event removes it

`bpy.types.<Type>.<name> = bpy.props.PointerProperty(type=<group>)` attaches a group, and `delattr(bpy.types.<Type>, "<name>")` releases it:
- Releases remove the attachment alone, stored values staying on IDs, saving with the file, and reading back when the group attaches again
- Files saved after a first read hold group storage whether the attachment stays or goes
- Static RNA member names (`render`, `frame_current`) refuse an attachment with `ValueError`
- `name`, RNA functions, and `bl_rna` take an attachment that shadows the member
- Second attachments under one name replace the first with no raise

## [03]-[CALLBACKS]

Each callback kind pairs one acquisition with one release on the same object, members named under `bpy`:

| [INDEX] | [KIND]       | [ACQUIRE]                                                          | [RELEASE]                                            |
| :-----: | :----------- | :----------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | App handler  | `app.handlers.<list>.append(fn)`, `fn` decorated `@persistent`     | `app.handlers.<list>.remove(fn)`                     |
|  [02]   | Timer        | `app.timers.register(fn, first_interval=<delay>, persistent=True)` | `app.timers.unregister(fn)` when `is_registered(fn)` |
|  [03]   | Message bus  | `msgbus.subscribe_rna(key=<key>, owner=owner, args=(), notify=fn)` | `msgbus.clear_by_owner(owner)`                       |
|  [04]   | Draw handler | `handle = <Space>.draw_handler_add(fn, args, <region>, <type>)`    | `<Space>.draw_handler_remove(handle, <region>)`      |
|  [05]   | UI extension | `<Menu>.append(fn)` or `prepend(fn)`, panels and headers alike     | `<Menu>.remove(fn)`                                  |
|  [06]   | Previews     | `previews = utils.previews.new()`                                  | `utils.previews.remove(previews)`                    |
|  [07]   | Translations | `app.translations.register(<package>, <table>)`                    | `app.translations.unregister(<package>)`             |
|  [08]   | Command      | `handle = utils.register_cli_command(<id>, fn)`                    | `utils.unregister_cli_command(handle)`               |
|  [09]   | Manual links | `utils.register_manual_map(fn)`                                    | `utils.unregister_manual_map(fn)`                    |
|  [10]   | Presets      | `utils.register_preset_path(<folder holding presets/>)`            | `utils.unregister_preset_path(<same path>)`          |
|  [11]   | Tool         | `utils.register_tool(<tool>)`, its keymap built from `bl_keymap`   | `utils.unregister_tool(<tool>)`, keymap included     |

Acquisitions follow host identity rules:
- Handler lists, timers, UI extensions, manual maps, and message bus keys hold one entry per acquisition call
- Timers match callables by identity, each read of a bound method building a new object
- Timer calls take one stored callable
- `bpy.app.timers.unregister` raises `ValueError` for a function no longer registered, a timer that stopped itself included
- `persistent(fn)` marks and returns `fn` for plain functions and lambdas, raising `ValueError` on a bound method, `functools.partial`, or callable instance
- File loads keep persistent functions and their bound methods in handler lists and drop every other callable
- `<Menu>.remove` drops the first entry equal to its argument, a fresh bound method of the same object included, and ignores an absent one
- Message bus subscriptions take one private `object()` owner per extension

Presets resolve through host paths:
- Preset menus reach an extension's presets through its registered path alone, user presets saving under `<user scripts>/presets/<subdir>`
- Operators with `PRESET` in `bl_options` read presets from `presets/operator/<category>.<name>`
- Operator presets hold every property without `SKIP_PRESET`
- Preset adds under a name any preset path holds cancel with a warning

## [04]-[COMMANDS]

`blender --command <id> <args>` calls the registered `fn(argv)`:
- `argv` holds every argument after the id unparsed, `--`, `--python-expr`, and `-c` included, and Blender acts on none of them
- Returned integers exit modulo 256, and `sys.exit(<n>)` exits `<n>`, `argparse` exits included
- Raises and non-integer returns print their error and exit 1
- Commands run in background with full `bpy.data`, the startup file's window and scene, and no area
- `.blend` and `--python` arguments before `--command` run first
- `.blend` arguments leave `context.window` `None` beside one window in `window_manager.windows`
- Commands reach extensions enabled in preferences or through `--addons` placed before `--command`, and `--factory-startup` alone drops them
- Every `unregister()` runs after `fn` returns, `sys.exit` included
- Second registrations of a held id, stock `extension` included, abort that command with exit 1 until either handle releases

## [05]-[TRANSLATIONS]

Tables take the form `{<locale>: {(<context>, <msgid>): <text>}}`, one per package:
- Contexts `None`, `""`, and `"*"` name the default context `pgettext` reads, and operator labels key under `contexts.operator_default`
- Locale keys take a `bpy.app.translations.locales` code or a shorter form
- Lookups read one dict per table, trying the full locale, `language_country`, `language@variant`, then `language`
- Tables key each language once, a full code beside a shorter form (`pt_BR` beside `pt`) answering from the shorter form after an earlier table misses
- Stock catalogs answer first, and tables translate only msgids the stock catalog leaves untranslated
- `en_US` entries relabel stock English text
- Shared keys answer from the first registered table
- Second registrations of one package raise `ValueError`
- `_bl_i18n_utils.bl_extract_messages.dump_addon_messages(<package>, False, <settings>)` returns a `.pot` keyed by `(context, msgid)`
- Extraction reads extension RNA strings, literal `text=` and report strings, and manifest `name` and `tagline`
- Extraction skips panel `bl_label` and `bl_category` and returns stock definition strings beside extension keys
- Extraction settings set `SOURCE_DIR` to `bpy.utils.resource_path("LOCAL")`

## [06]-[MANUAL_MAPS]

Online Manual lookups call manual map functions returning `(<url prefix>, ((<pattern>, <url suffix>), ...))`:
- Lookups call functions newest first with the stock map last, and the first matching mapping answers, extension patterns overriding stock pages
- Lookup ids lowercase before `fnmatchcase`, and patterns are lowercase
- Mappings answer with their first matching pattern, specific patterns preceding wildcards
- URLs join prefix and suffix with no separator
- Operator buttons and their property buttons resolve to `bpy.ops.<category>.<name>`
- Registered group and `AddonPreferences` property buttons resolve to `bpy.types.<class name>.<property>`, a pattern on the attachment missing them
- Other property buttons resolve to `bpy.types.<declaring struct>.<property>`

## [07]-[KEYMAPS]

Keymap items join `bpy.context.window_manager.keyconfigs.addon`, present in every process mode, and release through the keymap that made them:
- Keymap names with their space and region come from `keyconfigs.default.keymaps`, and addon keymaps matching none never fire
- `keymaps.new` and `keymaps.find` default to space `EMPTY` and region `WINDOW`, and editor keymaps pass `space_type`
- Extensions share one addon keymap per name, space, and region, and `keymaps.remove` on it drops other extensions' items
- `keyconfigs.addon` refuses modal keymaps with `RuntimeError`, stock modal maps included, and modal keys stay in the operator's `modal`
- `keymap_items.new` leaves `repeat` off, held-key items passing `repeat=True`
- Modifier keywords take `1`, `0`, or `-1` for either state, and `any=True` matches every modifier state
- Keymap rows take the stock keyconfig data form `(<operator>, <event>, {"properties": [(<name>, <value>), ...]})`, events naming pressed modifiers alone
- `ctrl=1` items ignore Cmd on macOS
- `darwin` rows take the Cmd copies `keyconfig_data_oskey_from_ctrl_for_macos` derives for stock keymaps and for `ctrl: 0` events
- Background runs hold the C-defined default items until `bpy.utils.keyconfig_init()` loads the stock preset
- Addon items reach `keyconfigs.user` through `keyconfigs.update()`, and GUI event loops run the update themselves

Events reach items in user keyconfig order:
- Addon items precede every stock item of their keymap and win an event a stock item there binds
- Addon items join in reverse add order, the last added winning a shared event and `head=True` losing to earlier addon items
- Items overriding a stock shortcut join the stock item's keymap, a stock Object Mode item beating addon items in 3D View or Window
- Items with an operator failing `poll` hand the event to the next matching item, and an operator returning `PASS_THROUGH` runs and hands it on

User edits sit on `keyconfigs.user` as differences from addon items:
- `unregister()` ends with `keyconfigs.update(keep_properties=True)` after classes unregister, else a disable or update drops edits to extension items
- Edits follow an item through a changed key or modifier, matched by operator and property values
- Edits skip an item with changed property values and stay stored, applying again once values match
- `keyconfigs.user` holds no keymap while startup `register()` runs
- User items resolve at each draw through `keymaps.find_match(<addon keymap>)` and `keymap_items.find_match(<addon keymap>, <addon item>)`
- `KeyMap.restore_item_to_default(<user item>)` resets an item to the extension's row and deletes its edit
- `KeyMap.restore_to_default()` resets every edited item of a user keymap, stock edits included
- Preference pages draw each user item through `rna_keymap_ui.draw_kmi([], <user config>, <user keymap>, <user item>, layout, 0)`
- `layout.context_pointer_set("keymap", <user keymap>)` precedes each `draw_kmi` call
- Operators under an extension's own category lack right-click Assign Shortcut, and its preference page draws their items

## [08]-[IDENTITY]

`__package__` of the top-level `__init__.py` names the extension package that host APIs key on:
- `__package__` of a subpackage module names the subpackage
- Subpackage modules read the extension package through a relative import (`from .. import __package__ as package` one level down)
- `AddonPreferences.bl_idname` takes that package
- Names sharing one namespace across extensions (operator and class `bl_idname`, `bpy.types` attachments, commands) open with the manifest `id`
- Stored preference values key on the full `bl_ext.<repo>.<id>`
- Another repository or a changed manifest `id` installs a separate extension at defaults

## [09]-[LIFECYCLE]

Lifecycle events treat extension state by host rule, `disable` and `enable` naming `addon_utils` calls:

| [INDEX] | [EVENT]                  | [VALUES]                   | [CODE]                               | [WHEELS]                   | [USER_FILES] |
| :-----: | :----------------------- | :------------------------- | :----------------------------------- | :------------------------- | :----------- |
|  [01]   | Upgrade install          | Kept                       | Package and submodules reimported    | Synced                     | Kept         |
|  [02]   | Reload Scripts           | Kept                       | Top module rerun, submodules stale   | Unchanged                  | Kept         |
|  [03]   | Checkbox off, then on    | Deleted                    | Modules kept, top rerun on new mtime | Removed, restored          | Kept         |
|  [04]   | `disable`, then `enable` | Kept, `None` when disabled | Modules kept, top rerun on new mtime | Unchanged                  | Kept         |
|  [05]   | `register()` raises      | Deleted on checkbox enable | Top module dropped, submodules kept  | Removed on checkbox enable | Kept         |
|  [06]   | Uninstall                | Deleted                    | Package and submodules dropped       | Removed                    | Deleted      |

Code reloads follow host rules:
- Reload Scripts runs from a timer after the calling script returns and never in background runs
- Reload Scripts reruns each enabled top `__init__.py`, an edited one twice, and module bodies hold declarations alone
- `importlib.reload` of an enabled package rebinds its module-level release stack, and the next `unregister()` releases nothing acquired before
- Blender writes bytecode beside extension sources and checks it by source size and whole-second mtime, and an edit keeping both loads the old code
- Extensions with a manifest range excluding the running Blender raise `RuntimeError` at each enable without importing and keep their preference row

Code changes reach a session through an upgrade install or a `disable` and `enable` cycle:
- Cycles delete every `sys.modules` entry at or under the package between `disable` and `enable`
- `handle_error` collects release and registration faults
- `enable` returns `None` when the extension stays disabled

## [10]-[PREFERENCES]

`AddonPreferences` values sit on the package's `preferences.addons` row:
- `--addons` and a first `default_set=False` enable create no row, and `preferences` reads `None` while the class is unregistered
- Reads match `preferences.addons.get(<package>)` against `bpy.types.Addon(preferences=<class>() as prefs)`, any other case taking the record default
- Rows store written properties alone, unwritten ones reading the running version's default
- Values of renamed or removed properties stay stored and return when a version declares the name again
- `bl_system_properties_get()` holds stored values by name, orphaned keys included
- Stored values the property's type refuses delete at read and return the default, `bool`, `int`, and enum values reading as each other
- Script writes leave `preferences.is_dirty` false
- Core preference writes, preferences page edits, and row-changing enables and disables set `is_dirty`
- GUI quits run `exit_pre` handlers, then a save of dirty preferences, then every `unregister()`
- Code sets `is_dirty = True` after a write it keeps
- Background exits skip the save, and background runs save through `bpy.ops.wm.save_userpref()` alone
- `exit_pre` writes to `filepaths.asset_libraries` hang Blender while a remote listing job runs
- `filepaths.asset_libraries.new()` rows set `is_dirty` and save at quit, the next registration taking the saved row and `unregister()` removing it

Properties keep identifier and type across versions, and a rename or type change takes a new identifier that `register()` fills after the preferences class registers:

```python
import bpy
from expression import Nothing, Option, Result, Some
from expression.extra.result import catch
import msgspec


class Previous(msgspec.Struct, frozen=True, kw_only=True):
    count: int = 1


def migrated(preferences: bpy.types.Preferences, package: str) -> Option[Result[Previous, msgspec.ValidationError]]:
    held: bpy.types.Addon | None = preferences.addons.get(package)
    match held:
        case bpy.types.Addon(preferences=ItemPreferences() as prefs) if (stored := prefs.bl_system_properties_get()) is not None and any(field.name in stored for field in msgspec.structs.fields(Previous)):  # type: ignore[operator]
            previous: Result[Previous, msgspec.ValidationError] = catch(msgspec.convert, exception=msgspec.ValidationError)(
                {field.name: stored.pop(field.name) for field in msgspec.structs.fields(Previous) if field.name in stored}, type=Previous  # type: ignore[operator]
            )
            for record in previous.to_option().to_list():
                prefs.amount = record.count
            preferences.is_dirty = True
            return Some(previous)
        case _:
            return Nothing
```

`Previous` holds the earlier version's moved properties with that version's defaults for unwritten values, and `ItemPreferences` declares `amount`. An `Error` keeps the `amount` default, as the host does for an incompatible stored value.

## [11]-[FILES_AND_NETWORK]

Extensions reach files and the network through host state:
- Files go under `bpy.utils.extension_path_user(<package>, path=<folder>)`, `<extensions>/.user/<repo>/<id>/<folder>`
- Upgrades replace the extension folder and delete files written beside its modules
- `extension_path_user` raises `ValueError` for any name but the three-part `bl_ext.<repo>.<id>`, a check of shape alone
- `create=True` prints a failed folder creation and returns `""`, and writes call `Path.mkdir` to keep the `OSError`
- Network calls run from an explicit user action and read `bpy.app.online_access` at each call
- `--online-mode` and `--offline-mode` force `online_access` over `preferences.system.use_online_access`, setting `bpy.app.online_access_override`
- Refusals name `--offline-mode` while `online_access_override` reads `True`, else the System preference

User files hold frozen records through typed codecs, a missing file reading as the record default:

```python
from pathlib import Path

import bpy
from expression import Error, Ok, Result
from expression.extra.result import catch
import msgspec


class Recent(msgspec.Struct, frozen=True, kw_only=True):
    paths: tuple[str, ...] = ()


def placed(package: str) -> Path:
    return Path(bpy.utils.extension_path_user(package, path="state"), "recent.json")


def recalled(package: str) -> Result[Recent, OSError | msgspec.DecodeError]:
    match catch(placed(package).read_bytes, exception=OSError)():
        case Result(tag="ok", ok=data):
            return catch(msgspec.json.decode, exception=msgspec.DecodeError)(data, type=Recent)
        case Result(error=FileNotFoundError()):
            return Ok(Recent())
        case Result(error=error):
            return Error(error)


def kept(package: str, recent: Recent) -> Result[Recent, OSError]:
    @catch(exception=OSError)
    def written(path: Path) -> Recent:
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(msgspec.json.encode(recent))
        return recent

    return written(placed(package))
```

`bpy.types.FileHandler` classes route file drops and collection exports to operators:
- `bl_import_operator` and `bl_export_operator` name the operators, and `bl_file_extensions` lists extensions as `".a;.b"`, matched without case
- Handlers receive drops through a `poll_drop` classmethod answering `True` alone, a drop matching more than one handler opening a menu
- Drops invoke the import operator with the first matching path in `filepath` and every matching path in `directory` and `files`
- Drop operators declare those properties `SKIP_SAVE` and `HIDDEN`, and `invoke` returns `ImportHelper.invoke_popup(context)` in place of the browser
- `collection.exporters.new(<handler bl_idname>, name=<name>)` adds a collection exporter holding the export operator's properties
- Collection exports run `EXEC_DEFAULT` over stored values, an empty `filepath` becoming `//<collection name><first extension>`

## [12]-[MANIFEST]

`blender_manifest.toml` takes rules `extension validate` leaves unchecked:
- Unknown keys and tables (a misspelled `[permission]`) pass validation with no effect
- `[permissions]` names each capability code uses with a reason, validation checking key names and reason text alone
- Permissions grant and enforce nothing, Blender listing title-cased keys without reasons and showing no permission an upgrade adds
- `blender_version_max` excludes its own version
- `license` entries take `SPDX:<identifier>`, validation passing a missing prefix and unknown identifiers

## [13]-[WHEELS]

Wheels of enabled extensions alone install into one shared `<extensions>/.local/lib/python3.13/site-packages`:
- Shared folders present at startup sit ahead of Blender's `site-packages`, and a session's first wheel install appends the folder last until restart
- Wheels ahead of Blender's `site-packages` replace bundled copies process-wide, and extensions omit one Blender bundles at a version they accept
- Distributions, keyed by wheel file name before its first `-`, resolve to the highest version any enabled extension bundles
- Versions rank by major, minor, and patch, then the tail as text, `1.10` reading as `1.0.10` below `1.9.0` and `1.0.0rc1` outranking `1.0.0`
- First imports of a session hold until restart, a later install or disable rewriting files while `sys.modules` keeps the imported version
- Enabled-set changes (checkbox, `default_set=True` calls, uninstall, failed enables) resync the folder at once, distributions falling back or leaving
- Syncs extract a wheel whose `<name>-<version>.dist-info` folder is absent, a wheel rebuilt at that version keeping the first extraction
- First extractions stay until an enabled-set change leaves no enabled extension listing the wheel, an upgrade install keeping them
- Wheels for another platform skip at enable, and the import in `register()` raises `ModuleNotFoundError`
- Wheels for another CPython minor version mark the extension incompatible, and `abi3` wheels of an older CPython load
- Wheels for macOS match by architecture or `universal2`, whatever OS version their tag names
