---
name: blender-coding-extensions
description: "Use when writing or reviewing a Blender extension's registration, preferences, or manifest, covering lifecycle and packaging"
---

# [BLENDER_CODING_EXTENSIONS]

Covers what an extension acquires and releases across its lifecycle, with its manifest and wheels.

- Use `blender-coding` for callback bodies, properties, and operator classes
- Use `blender-coding-interface` for panels, menus, lists, and interface text
- Use `use-blender` for installing, validating, and running extensions
- Use `manage-repo` for extension assembly and imports

[REFERENCES]:
- [01]-[KEYMAPS](references/keymaps.md): Addon keymap items, event order, and user edits a preferences page draws
- [02]-[FILES](references/files.md): User folders, packaged files, network access, and file handlers for drops and collection exports
- [03]-[PACKAGING](references/packaging.md): Manifest rules validation leaves unchecked, shared wheels, and subpackages

## [01]-[REGISTRATION]

Registration follows host rules:
- Module import and `register()` run under `RestrictBlend`, `bpy.context` holding `window_manager` and `preferences` alone and `bpy.data` reads raising `AttributeError`
- When `register()` raises, Blender calls no `unregister()`, and classes, handlers, and timers acquired before the raise stay live
- `bpy.utils.register_classes_factory` stops at a raise with earlier classes registered
- Gizmo, gizmo group, and macro classes hold no `is_registered`

Acquisitions follow host dependency order, and releases run in reverse, a group unregistered while an attachment names it crashing Blender at the attachment's next read:

| [INDEX] | [FIRST]         | [THEN]                                                   | [OTHERWISE]                                   |
| :-----: | :-------------- | :------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `PropertyGroup` | Class or attachment naming it in a pointer or collection | `ValueError`, class registered without it     |
|  [02]   | Parent panel    | Panel naming it in `bl_parent_id`                        | `RuntimeError` at registration                |
|  [03]   | Operator        | Keymap item calling it                                   | Item property writes raising `AttributeError` |

`ordered` sorts classes by pointer and collection targets and `bl_parent_id`. Class releases go on the stack before `register_class`, a refused property raising with the class registered:

```python
from contextlib import ExitStack
import graphlib
from sys import platform
from typing import get_type_hints, TYPE_CHECKING, TypedDict

from bl_keymap_utils.io import keymap_init_from_data
from bl_keymap_utils.platform_helpers import keyconfig_data_oskey_from_ctrl_for_macos
import bpy

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

type Registrable = type[bpy.types.PropertyGroup | bpy.types.Operator | bpy.types.Panel]


class Location(TypedDict, total=False):
    space_type: "rna_enums.SpaceTypeItems"
    region_type: "rna_enums.RegionTypeItems"


type Keymaps = list[tuple[str, Location, dict[str, list[tuple[str, dict[str, object], dict[str, object]]]]]]


def ordered[C: Registrable](classes: tuple[C, ...]) -> tuple[C, ...]:
    panels = {vars(cls).get("bl_idname", cls.__name__): cls for cls in classes if issubclass(cls, bpy.types.Panel)}
    return tuple(
        graphlib.TopologicalSorter({
            cls: {
                *(
                    hint.keywords["type"]
                    for hint in get_type_hints(cls).values()
                    if isinstance(hint, bpy.props._PropertyDeferred) and hint.function in {bpy.props.PointerProperty, bpy.props.CollectionProperty}
                ),
                *(panels[parent] for parent in panels.keys() & {vars(cls).get("bl_parent_id")}),
            }
            & set(classes)
            for cls in classes
        }).static_order()
    )


CLASSES: tuple[Registrable, ...] = (Detail, Holder, Run, Main, Settings)
KEYMAPS: Keymaps = [("Object Mode", {}, {"items": [(Run.bl_idname, {"type": "J", "value": "PRESS", "ctrl": 1}, {"properties": [("amount", 2)]})]})]
held = ExitStack()


def released(cls: Registrable) -> None:
    if "bl_rna" in vars(cls):
        bpy.utils.unregister_class(cls)


def register() -> None:
    keyconfigs: bpy.types.KeyConfigurations = bpy.context.window_manager.keyconfigs  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    addon: bpy.types.KeyConfig = keyconfigs.addon  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    with ExitStack() as stack:
        stack.callback(keyconfigs.update, keep_properties=True)
        for cls in ordered(CLASSES):
            stack.callback(released, cls)
            bpy.utils.register_class(cls)
        bpy.types.Scene.item_holder = bpy.props.PointerProperty(type=Holder)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        stack.callback(delattr, bpy.types.Scene, "item_holder")
        rows: Keymaps = keyconfig_data_oskey_from_ctrl_for_macos(KEYMAPS) if platform == "darwin" else KEYMAPS  # type: ignore[func-returns-value, assignment]  # ty: ignore[invalid-assignment]
        for name, params, content in rows:
            keymap = addon.keymaps.new(name, **params)
            start = len(keymap.keymap_items)
            keymap_init_from_data(keymap, content["items"])
            for item in keymap.keymap_items[start:]:
                stack.callback(keymap.keymap_items.remove, item)
        held.push(stack.pop_all())


unregister = held.close
```

`Settings` stands for a `PropertyGroup`, `Holder` for a group collecting `Settings`, `Main` and `Detail` for a panel and its child, `Run` for an operator with an `amount` property, and `item` for the manifest `id`.

## [02]-[CLASSES]

Registered classes take the forms Blender checks at `register_class` and at each call:
- Operator `bl_idname` takes `<category>.<name>` in lowercase letters, digits, and underscores, one dot, at most 60 characters, else `RuntimeError`
- Panel, menu, header, list, asset shelf, and file handler idnames hold `_PT_`, `_MT_`, `_HT_`, `_UL_`, `_AST_`, or `_FH_` mid-name, else registration warns
- Panel, menu, list, and render engine `bl_idname` values register cut to 63 bytes, and a class under a taken idname replaces the earlier class with an `Info` line
- Callbacks declaring more or fewer positional parameters than the host passes raise `ValueError` at `register_class`, `*args` counting none (Panel `draw` takes 2)
- `__init__` overrides take `*args, **kwargs` and pass them to `super().__init__` first, a narrower signature reaching callers as `RuntimeError`
- Blender builds a new operator, panel, and menu instance per call, and instance attributes reach no later call
- `type(<name>, (<base>,), <namespace>)` registers as a class statement does, a `__annotations__` entry holding its `bpy.props` definitions
- Unregistering an operator class while it runs modal leaves its `window.modal_operators` entry reading `None` until the next event removes it
- Classes register once, a second `register_class` raising `ValueError` and a second `unregister_class` `RuntimeError`

Panels shared across editors derive one subclass per `bl_space_type` from a base holding `draw`, a class without `bl_idname` registering under its class name:

```python
from typing import override

import bpy


class Shared(bpy.types.Panel):
    bl_label = "Item"
    bl_region_type = "UI"
    bl_category = "Item"

    @override
    def draw(self, context: bpy.types.Context) -> None:
        layout: bpy.types.UILayout = self.layout  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        layout.label(text=self.bl_space_type)


PANELS: tuple[type[bpy.types.Panel], ...] = tuple(type(f"ITEM_PT_{space.lower()}", (Shared,), {"bl_space_type": space}) for space in ("VIEW_3D", "IMAGE_EDITOR", "NODE_EDITOR"))
```

`bpy.types.<Type>.<name> = bpy.props.PointerProperty(type=<group>)` attaches a group, and `delattr(bpy.types.<Type>, "<name>")` releases it:
- Group storage on an ID starts at its first read, saves with files, and outlives a release, reading back once the group attaches again
- Static RNA members (`render`, `frame_current`) refuse an attachment with `ValueError`, and `name`, RNA functions, and `bl_rna` take a shadowing one
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

Acquisitions follow host identity rules:
- Handler lists, timers, UI extensions, manual maps, and message bus keys hold one entry per acquisition call
- Timers store one callable and match it by identity, each read of a bound method building a new object
- `bpy.app.timers.unregister` raises `ValueError` for a function no longer registered, a timer that stopped itself included
- `persistent(fn)` marks and returns `fn` for plain functions and lambdas, raising `ValueError` on a bound method, `functools.partial`, or callable instance
- Handler lists keep bound methods of a `persistent` function across file loads
- `<Menu>.remove` drops the first equal entry, a fresh bound method of one object included, and ignores an absent one
- Message bus subscriptions take one private `object()` owner per extension

Presets resolve through host paths:
- Preset menus reach an extension's presets through its registered path alone, user presets saving under `<user scripts>/presets/<subdir>`
- Operators with `PRESET` in `bl_options` read presets from `presets/operator/<category>.<name>`, each holding every property without `SKIP_PRESET`
- Preset adds under a name any preset path holds cancel with a warning

## [04]-[COMMANDS]

`blender --command <id> <args>` calls the registered `fn(argv)`:
- `argv` holds every argument after the id unparsed, `--`, `--python-expr`, and `-c` included, and Blender acts on none of them
- Returned integers exit modulo 256, `sys.exit(<n>)` and `argparse` exit `<n>`, and raises and non-integer returns print their error and exit 1
- Commands run in background with full `bpy.data`, the startup file's window and scene, and no area
- Arguments before `--command` (`.blend`, `--python`) run first, a `.blend` leaving `context.window` `None` beside one `window_manager.windows` entry
- Commands reach extensions enabled in preferences or through `--addons` placed before `--command`, and `--factory-startup` alone drops them
- Every `unregister()` runs after `fn` returns, `sys.exit` included
- Second registrations of a registered id, stock `extension` included, abort that command with exit 1 until either handle releases

## [05]-[TRANSLATIONS]

Tables take the form `{<locale>: {(<context>, <msgid>): <text>}}`, one per package:
- Contexts `None`, `""`, and `"*"` name the default context `pgettext` reads
- Tables keying each msgid under both `contexts.default` and `contexts.operator_default` translate one text as a button label and an operator label
- Locale keys take a `bpy.app.translations.locales` code or a shorter form
- Lookups read one dict per table, trying the full locale, `language_country`, `language@variant`, then `language`
- Tables key each language once, a full code beside a shorter form (`pt_BR` beside `pt`) answering from the shorter form after an earlier table misses
- Tables translate only msgids the stock catalog leaves untranslated
- `en_US` entries relabel stock English text
- Shared keys answer from the first registered table, and second registrations of one package raise `ValueError`
- `_bl_i18n_utils.bl_extract_messages.dump_addon_messages(<package>, False, <settings>)` returns a `.pot` keyed by `(context, msgid)`
- Extraction reads extension RNA strings, literal `text=` and report strings, and manifest `name` and `tagline`
- Extraction skips panel `bl_label` and `bl_category` and returns stock definition strings beside extension keys
- Extraction settings set `SOURCE_DIR` to `bpy.utils.resource_path("LOCAL")`

## [06]-[MANUAL_MAPS]

Online Manual lookups call manual map functions returning `(<url prefix>, ((<pattern>, <url suffix>), ...))`:
- Lookups call functions newest first and stock map last, first matching pattern answering, and specific patterns precede wildcards
- Lookup ids lowercase before `fnmatchcase`, and patterns are lowercase
- URLs join prefix and suffix with no separator
- Operator buttons and their property buttons resolve to `bpy.ops.<category>.<name>`
- Registered group and `AddonPreferences` property buttons resolve to `bpy.types.<class name>.<property>`, a pattern on the attachment missing them
- Other property buttons resolve to `bpy.types.<declaring struct>.<property>`

Maps run at each lookup, and patterns derived from `dir(bpy.ops.<category>)` and class names cover each operator and property class the extension registers:

```python
import bpy


def manual() -> tuple[str, tuple[tuple[str, str], ...]]:
    category: object = bpy.ops.item  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    return "https://<docs>/", (
        *((f"bpy.ops.item.{name}", f"operators.html#{name}") for name in dir(category)),
        *((f"bpy.types.{key}.*", f"settings.html#{key}") for key in (cls.__name__.lower() for cls in (Settings, ItemPreferences))),
    )
```

## [07]-[IDENTITY]

`__package__` of top-level `__init__.py` names the extension package host APIs key on:
- `__package__` of a subpackage module names the subpackage
- Subpackage modules read the extension package through a relative import (`from .. import __package__ as package` one level down)
- `AddonPreferences.bl_idname` takes that package
- Names sharing one namespace across extensions (operator and class `bl_idname`, `bpy.types` attachments, commands) open with the manifest `id`
- Preference values key on the full `bl_ext.<repo>.<id>`, another repository or changed manifest `id` installing a separate extension at defaults

## [08]-[LIFECYCLE]

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
- Reload Scripts reruns an edited top `__init__.py` twice, and module bodies hold declarations alone
- `importlib.reload` of an enabled package rebinds its module-level `ExitStack`, and the next `unregister()` releases nothing acquired before
- Blender writes bytecode beside extension sources and checks it by source size and whole-second mtime, and an edit keeping both loads the old code
- Extensions with a manifest range excluding the running Blender raise `RuntimeError` at each enable without importing and keep their preference row

Code changes reach a session through an upgrade install or a `disable` and `enable` cycle:
- Enables after a `disable` or a raising `register()` load changed submodules once every `sys.modules` entry at or under the package leaves
- `handle_error` collects release and registration faults
- `enable` returns `None` when the extension stays disabled

## [09]-[PREFERENCES]

`AddonPreferences` values sit on the package's `preferences.addons` row:
- `--addons` and a first `default_set=False` enable create no row, and `preferences` reads `None` while the class is unregistered
- Reads match `preferences.addons.get(<package>)` against `bpy.types.Addon(preferences=<class>() as prefs)`, any other case taking the record default
- Rows store written properties alone, unwritten ones reading the running version's default
- Values of renamed or removed properties stay in `bl_system_properties_get()` by name and return when a version declares the name again
- Stored values a property's type refuses delete at read and return the default, `bool`, `int`, and enum values reading as each other
- Script writes leave `preferences.is_dirty` false while core preference writes, page edits, and row-changing enables set it, and code sets `is_dirty = True` after a write it keeps
- GUI quits run `exit_pre` handlers, then a save of dirty preferences, then every `unregister()`
- Background exits skip the save, and background runs save through `bpy.ops.wm.save_userpref()` alone
- `bpy.ops.preferences.addon_show(module=<package>)` opens Preferences at the extension's expanded row
- `exit_pre` writes to `filepaths.asset_libraries` hang Blender while a remote listing job runs
- `filepaths.asset_libraries.new()` rows set `is_dirty` and save at quit, next registration taking the saved row and `unregister()` removing it

Properties keep identifier and type across versions, and a rename or type change takes a new identifier that `register()` fills after the preferences class registers:

```python
import bpy
from expression import Nothing, Option, Result, Some
from expression.extra.result import catch
import msgspec


class Previous(msgspec.Struct, frozen=True, kw_only=True):
    count: int = 1


def migrated(preferences: bpy.types.Preferences, package: str) -> Option[Result[Previous, msgspec.ValidationError]]:
    addon: bpy.types.Addon | None = preferences.addons.get(package)
    match addon:
        case bpy.types.Addon(preferences=ItemPreferences() as prefs) if (stored := prefs.bl_system_properties_get()) is not None and (
            names := tuple(field.name for field in msgspec.structs.fields(Previous) if field.name in stored)  # type: ignore[operator]
        ):
            previous: Result[Previous, msgspec.ValidationError] = catch(msgspec.convert, exception=msgspec.ValidationError)({name: stored.pop(name) for name in names}, type=Previous)
            for record in previous.to_option().to_list():
                prefs.amount = record.count
            preferences.is_dirty = True
            return Some(previous)
        case _:
            return Nothing
```

`Previous` holds an earlier version's moved properties at that version's defaults, and `ItemPreferences` declares `amount`, an `Error` leaving its default.
