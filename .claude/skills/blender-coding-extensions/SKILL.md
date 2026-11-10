---
name: blender-coding-extensions
description: "Use when writing or reviewing a Blender extension's registration, preferences, or manifest, covering lifecycle and packaging"
---

# [BLENDER_CODING_EXTENSIONS]

Covers what an extension acquires and releases across its lifecycle, and what its package holds.

[REFERENCES]:
- [01]-[KEYMAPS](references/keymaps.md): How an extension's key bindings resolve against built-in items and user edits
- [02]-[FILES](references/files.md): How an extension reaches data outside Blender, on disk or over a network
- [03]-[PACKAGING](references/packaging.md): What an extension's package declares and how its code loads beside other extensions

## [01]-[REGISTRATION]

Registration runs:
- Import and `register()` run under `RestrictBlend`: `bpy.context` holds `window_manager` and `preferences` alone, `bpy.data` raises `AttributeError`
- Module-level `from bpy import context, data` binds the restricted stand-ins permanently, and code reads `bpy.context` and `bpy.data` per call
- Raising `register()` gets no `unregister()`, its classes, handlers, and timers staying registered
- Classes register once, a second `register_class` raising `ValueError` and a second `unregister_class` `RuntimeError`
- Classes whose `register` classmethod raises stay registered with `bl_rna` in their own namespace

Acquisitions follow Blender dependency order and releases reverse it, an attachment naming an unregistered group crashing Blender at its next read:

| [INDEX] | [FIRST]         | [THEN]                                                   | [OTHERWISE]                                   |
| :-----: | :-------------- | :------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `PropertyGroup` | Class or attachment naming it in a pointer or collection | `ValueError`, class registered without it     |
|  [02]   | Parent panel    | Panel naming it in `bl_parent_id`                        | `RuntimeError` at registration                |
|  [03]   | Operator        | Keymap item calling it                                   | Item property writes raising `AttributeError` |

`rasm_bpy.data.Registration` acquires an extension's rows in that order whatever order they hold: groups before the attachments and operators naming them, operators before the keymap items calling them, each panel after its `bl_parent_id` panel. Each release joins one stack, `unregister` closing it in reverse. Rows sit in an extension module, the package `__init__.py` re-exporting `register` and `unregister`, and a class registered outside `Registration` takes the bare `bpy.props` annotation `register_class` reads:

```python
class Settings(bpy.types.PropertyGroup):
    """Amount the item runs by."""

    amount: Annotated[int, bpy.props.IntProperty(name="Amount", min=1, default=1)]


@Operator.of(Settings, options=frozenset({"UNDO", "INTERNAL"}))
def run_item(call: Call[Settings]) -> Outcome[Answer]:
    """Run the item by its amount."""
    return Ok(Done(notes=(("INFO", f"Ran {call.properties.amount} times"),)))


_REGISTRATION = Registration(
    __name__.rpartition(".")[0],
    (
        Keymap(keymap="Object Mode", bindings=((Chord(type="J", ctrl=1), Placement(run_item, {"amount": 2})),)),
        Attachment(key="item", form=Settings, owner=bpy.types.Scene),
        Handler(handlers=bpy.app.handlers.load_post, body=loaded),
    ),
)
register, unregister = _REGISTRATION.register, _REGISTRATION.unregister
```

`Placement` registers `run_item` before the `J` item calling it, and `Attachment` registers `Settings` before pointing `Scene.<id>_item` at it. `loaded` is a `load_post` body returning `Outcome[None]`. A refused row releases every earlier acquisition, and `register` raises one `RuntimeError` listing the faults.

Attachments:
- `Attachment(key=<key>, form=<group>, owner=<host type>)` points `<owner>.<id>_<key>` at its form group, `delattr` releasing it
- Group storage on an ID starts at its first read, saves with files, and outlives a release, reading back once the group attaches again
- Static RNA members (`render`, `frame_current`) refuse an attachment with `ValueError`, and `name`, RNA functions, and `bl_rna` take a shadowing one
- Second native attachments under one name replace the first with no raise, and `Attachment` refuses a held name with a `TAKEN` fault

## [02]-[CLASSES]

Registered classes take the forms Blender checks at `register_class` and at each call:
- Operator `bl_idname` is `<category>.<name>` within 60 lowercase letters, digits, and underscores, else registration raises `RuntimeError`
- Panel, menu, header, list, asset shelf, file handler idnames hold `_PT_`, `_MT_`, `_HT_`, `_UL_`, `_AST_`, `_FH_` mid-name, else registration warns
- Panel, menu, list, and render engine idnames register cut to 63 bytes; scene engine names store at most 31 UTF-8 bytes
- Classes under a taken idname replace the earlier class with an `Info` line
- Callbacks with a positional parameter count other than the host's raise `ValueError` at registration, `*args` counting none (Panel `draw` takes 2)
- Panels and menus build a new instance per draw, and state between draws goes in a property
- Classes without `bl_idname` register under the `__name__` they hold at `register_class`
- Panels shared across editors register one `type(f"{base.__name__}_{space.lower()}", (base,), {"bl_space_type": space})` per space
- Shared panel bases hold `draw` and other `bl_*` members

## [03]-[CALLBACKS]

Callback kinds pair one acquisition with one release on the same object, pushed onto the registration stack after it, members named under `bpy`:

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

Acquisitions follow Blender identity rules:
- Handler lists, timers, UI extensions, manual maps, and message bus keys hold one entry per acquisition call
- Timers store one callable and match it by identity, each read of a bound method building a new object
- `bpy.app.timers.unregister` raises `ValueError` for a function no longer registered, a timer that stopped itself included
- `persistent(fn)` marks and returns plain functions and lambdas, and raises `ValueError` on a bound method, `functools.partial`, or callable instance
- Handler lists keep bound methods of a `persistent` function across file loads
- `<Menu>.remove` drops the first equal entry, a fresh bound method of one object included, and ignores an absent one
- Message bus subscriptions take one private `object()` owner per extension

Presets resolve through Blenders paths:
- Typed presets are JSON records decoded by record type, in place of the Python files `script.execute_preset` runs
- Typed presets resolve under `presets/<extension id>/<key>` of each `utils.preset_paths` folder
- User presets save under `utils.user_resource('SCRIPTS', path="presets/<extension id>/<key>")`, which uninstall leaves
- Stock presets are records in code or JSON files of the package folder the extension registers as a preset path
- Operators with `PRESET` in `bl_options` read presets from `presets/operator/<category>.<name>`, each holding every property without `SKIP_PRESET`
- Typed saves replace a held user file and refuse a stock name with a `TAKEN` fault
- `AddPresetBase` adds under a name any preset path holds cancel with a warning

## [04]-[COMMANDS]

`blender --command <id> <args>` calls the registered `fn(argv)`:
- `argv` holds every argument after the id unparsed, `--`, `--python-expr`, and `-c` included, and Blender acts on none of them
- Returned integers exit modulo 256, `sys.exit(<n>)` and `argparse` exit `<n>`, and raises and non-integer returns print their error and exit 1
- Commands run in background with full `bpy.data`, the startup file's window and scene, and no area
- Arguments before `--command` (`.blend`, `--python`) run first, a `.blend` leaving `context.window` `None` beside one `window_manager.windows` entry
- Commands reach extensions enabled in preferences or through `--addons` placed before `--command`, and `--factory-startup` alone drops them
- Every `unregister()` runs after `fn` returns, `sys.exit` included
- Second registrations of a registered id, built-in `extension` included, abort that command with exit 1 until either handle releases

## [05]-[TRANSLATIONS]

Tables take the form `{<locale>: {(<context>, <msgid>): <text>}}`, one per package:
- Contexts `None`, `""`, and `"*"` name the default context `pgettext` reads
- Tables keying each msgid under both `contexts.default` and `contexts.operator_default` translate one text as a button label and an operator label
- Locale keys take a `bpy.app.translations.locales` code or a shorter form
- Lookups read one dict per table, trying the full locale, `language_country`, `language@variant`, then `language`
- Tables key each language once, a full code beside a shorter form (`pt_BR` beside `pt`) answering from the shorter form after an earlier table misses
- Tables translate only msgids the built-in catalog leaves untranslated, and `en_US` entries relabel built-in English text
- Shared keys answer from the first registered table, and second registrations of one package raise `ValueError`
- `_bl_i18n_utils.bl_extract_messages.dump_addon_messages(<package>, False, <settings>)` returns a `.pot` keyed by `(context, msgid)`
- Extraction settings set `SOURCE_DIR` to `bpy.utils.resource_path("LOCAL")`
- Extraction returns extension RNA strings, literal `text=` and report strings, manifest `name` and `tagline`, and built-in definition strings
- Extraction leaves out panel `bl_label` and `bl_category`

## [06]-[MANUAL_MAPS]

Online Manual lookups call manual map functions returning `(<url prefix>, ((<pattern>, <url suffix>), ...))`:
- Lookups call functions newest first and built-in map last, first matching pattern answering, and specific patterns precede wildcards
- Lookup ids lowercase before `fnmatchcase`, and patterns are lowercase
- URLs join prefix and suffix with no separator
- Operator buttons and their property buttons resolve to `bpy.ops.<category>.<name>`
- Registered group and `AddonPreferences` property buttons resolve to `bpy.types.<class name>.<property>`, a pattern on the attachment missing them
- Other property buttons resolve to `bpy.types.<declaring struct>.<property>`

Patterns derive from the registered classes, an operator's `bl_idname` and a group's class name each keying one pattern and one anchor:

```python
def manual(root: str, classes: Iterable[type[Struct]], /) -> tuple[str, tuple[tuple[str, str], ...]]:
    return root, tuple(
        (f"bpy.ops.{cls.bl_idname}", f"#{cls.bl_idname}") if issubclass(cls, bpy.types.Operator) else (f"bpy.types.{cls.__name__.lower()}.*", f"#{cls.__name__.lower()}")
        for cls in classes
        if issubclass(cls, (bpy.types.Operator, bpy.types.PropertyGroup, bpy.types.AddonPreferences))
    )
```

`register_manual_map` and `unregister_manual_map` take one held `partial(manual, "<url>", <classes>)`, called at each lookup. `Registration` registers its own map over the manifest `website`.

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
- `importlib.reload` of an enabled package rebinds its module-level `held` stack, and the next `unregister()` releases nothing acquired before
- Blender writes bytecode beside extension sources and checks it by source size and whole-second mtime, and an edit keeping both loads the old code
- Extensions with a manifest range excluding the running Blender raise `RuntimeError` at each enable without importing and keep their preference row

Code changes reach a session through an upgrade install or a `disable` and `enable` cycle:
- Enables after a `disable` or a raising `register()` load changed submodules once every `sys.modules` entry at or under the package leaves
- `handle_error` collects release and registration faults
- `enable` returns `None` when the extension stays disabled

## [09]-[PREFERENCES]

`AddonPreferences` values sit on the package's `preferences.addons` row, and reads derive the package from the class's `bl_idname`:

```python
def preferred[P: bpy.types.AddonPreferences](kind: type[P], preferences: bpy.types.Preferences, /) -> Option[P]:
    addon: bpy.types.Addon | None = preferences.addons.get(kind.bl_idname)
    stored = addon and addon.preferences
    return Some(stored) if isinstance(stored, kind) else Nothing
```

`preferred` returns `Nothing` while the class is unregistered and where `--addons` or a first `default_set=False` enable created no row. Stored values follow host rules:
- Rows store written properties alone, unwritten ones reading the running version's default
- Values outlive their declaration by identifier and read back once a version declares the identifier again
- Stored values a property's type refuses delete at read and return the default, `bool`, `int`, and enum values reading as each other
- Renames and type changes keep the old identifier under `options={'HIDDEN'}` beside a new one
- `register()` copies an old value `is_property_set` reports to the new identifier, then calls `property_unset(<old>)`
- Core preference writes, page edits, and row-changing enables set `preferences.is_dirty`, and script writes leave it false
- Code sets `is_dirty` after a write it keeps
- GUI quits run `exit_pre` handlers, then a save of dirty preferences, then every `unregister()`
- Background exits skip the save, and background runs save through `bpy.ops.wm.save_userpref()` alone
- `bpy.ops.preferences.addon_show(module=<package>)` opens Preferences at the extension's expanded row
- `exit_pre` writes to `filepaths.asset_libraries` hang Blender while a remote listing job runs
- `filepaths.asset_libraries.new()` rows set `is_dirty` and save at quit, next registration taking the saved row and `unregister()` removing it
