# [DATA]

## [01]-[STORAGE]

Value lifetime picks the owner:

| [INDEX] | [LIFETIME]                            | [OWNER]                                                                                         |
| :-----: | :------------------------------------ | :---------------------------------------------------------------------------------------------- |
|  [01]   | One operator call                     | Operator properties, `options={'SKIP_SAVE'}` on values the next call must not reuse             |
|  [02]   | Saved with an ID, node, or view layer | `PointerProperty(type=<group>)` attached to the holding type                                    |
|  [03]   | Saved record `bpy.props` cannot state | `msgspec.json` bytes on its ID under the record's class name                                    |
|  [04]   | Saved with a window's screen          | `bpy.props` on `Screen`, `Window` and `Space` attachments refusing writes                       |
|  [05]   | User setting across files             | `AddonPreferences` on the extension package                                                     |
|  [06]   | Named preset files                    | `presets/<id>/<key>`: stock under `register_preset_path`, user under `user_resource('SCRIPTS')` |
|  [07]   | Session cache                         | Scope-owned dict keyed by `session_uid`, cleared on `load_post`, `undo_post`, `redo_post`       |

Records with unions, 64-bit integers, or nested tuples store as NUL-free JSON that overwrites cannot cut, decoding against their class at each read:

```python
def kept(owner: bpy.types.ID, record: msgspec.Struct, /) -> None:
    owner[type(record).__name__] = msgspec.json.encode(record)


def held[S: msgspec.Struct](form: type[S], owner: bpy.types.ID, /) -> Outcome[S]:
    return (
        Option.of_optional(owner.get(form.__name__))
        .to_result_with(partial(Fault.of, Reason.ABSENT, owner.name_full, member=form.__name__))
        .bind(lambda raw: catch(msgspec.json.decode, exception=msgspec.DecodeError)(raw, type=form).map_error(partial(Fault.of, Reason.INVALID, owner.name_full)))
    )
```

`held(Grid, obj)` returns `ABSENT` before the first `kept`, and `INVALID` with the failing key path once `Grid` changes shape. Record class names stay unique among records one ID holds, keys leaving no room for a module prefix.

## [02]-[CREATED_IDS]

Created IDs take their saved lifetime from their users and links:
- `use_fake_user = True` keeps an ID saved as one counted user
- `<old>.user_remap(<new>)` moves every user, fake user included, before removing `<old>`
- `objects.link` and `children.link` of a linked item raise `RuntimeError`, and `in` tests on them take the item's `name`, an ID raising `TypeError`
- `children.link` of a parent or ancestor raises already-in-collection `RuntimeError`, a fault at the call
- `bpy.data.scenes.new()` takes factory settings (metric units, EEVEE, no world, no camera)
- `bpy.ops.scene.new(type="EMPTY")` copies the window scene's units and engine and makes the new scene the window's scene
- Appends and links fire `blend_import_pre` and `blend_import_post` with a `BlendImportContext`
- `BlendImportContext.import_items` entries hold the local `id` (`Brick.001`), the source `name` (`Brick`), `source_library`, and `append_action`
- `blend_import_post` handlers remap imported duplicates of data their extension owns onto the original the source `name` holds
- `asset_mark()` sets `use_fake_user` and an `asset_data` filed under the nil catalog, an object staying outside every collection
- `asset_clear()` clears `use_fake_user` and `asset_data`, and the next save drops an ID with no other user
- Catalogs are `<uuid>:<path>:<simple name>` lines of `blender_assets.cats.txt` at library root
- `asset_data.catalog_id = <uuid>` files with no check and ignores malformed strings
- `asset_generate_preview()` renders the 128 by 128 `preview` at the call, an empty object keeping `preview` `None`

## [03]-[LIBRARY_LOADS]

`libraries.load` takes source names inside its context and holds IDs after exit. Both sides name the `bpy.data` collection with `fixed_type` nearest the requested class in its MRO:

```python
@catch(exception=OSError)
def loaded(kind: type[bpy.types.ID], path: Path, chosen: Callable[[str], bool], /, *, link: bool = False) -> tuple[bpy.types.ID, ...]:
    member = next(prop.identifier for cls in kind.__mro__ for prop in bpy.types.BlendData.bl_rna.properties if isinstance(prop, bpy.types.CollectionProperty) and type(prop.fixed_type) is cls)
    opened: AbstractContextManager[tuple[object, object]] = bpy.data.libraries.load(str(path), link=link, reuse_local_id=not link)  # type: ignore[func-returns-value]  # ty: ignore[invalid-assignment]
    with opened as (source, target):
        setattr(target, member, list(filter(chosen, getattr(source, member))))
    return tuple(getattr(target, member))
```

`loaded(bpy.types.Material, path, {"Brick", "Wood"}.__contains__)` appends both names the source holds, returning the same copies on repeat. `bpy.types.SunLight` reads `lights`:
- `reuse_local_id=True` appends return an earlier append's local copy, links return the present linked ID, and both flags together raise `ValueError`
- Unreadable paths raise `OSError` at `__enter__`, and a name the source lacks loads as `None` with a `UserWarning`
- Loads bring every ID a requested ID references
- `bpy.data.temp_data()` holds another file's loads in a store freed at its exit, session data untouched
- `bpy.data.libraries.write(path, blocks, path_remap=, fake_user=, compress=)` writes a subset with its references
- Non-`NONE` `path_remap` on `libraries.write` keeps `//` paths valid elsewhere
- Edits to linked data take an override from `ID.override_create` or `ID.override_hierarchy_create`
- Values read from a file (custom properties, texts) stay data, and untrusted files open through `wm.open_mainfile(use_scripts=False)`

## [04]-[PATHS]

`bpy.data.file_path_foreach` replaces each path its visitor returns a string for, and takes a `def` or `lambda`, a `partial` raising `TypeError`:

```python
def rebased(old: Path, new: Path, /, *, subset: set[bpy.types.ID] | None = None) -> None:
    def moved(owner: bpy.types.ID, path: str, meta: bpy.types.BlendDataPathMeta) -> str | None:
        held = Path(bpy.path.abspath(path, library=owner.library))
        return None if meta.is_readonly or not held.is_relative_to(old) else str(new / held.relative_to(old))

    bpy.data.file_path_foreach(moved, subset=subset)
```

Path properties hold blend-relative paths:
- Default `flags` skip packed and weak-reference paths
- `EXPAND_TOKENS` and `EXPAND_SEQUENCES` visit UDIM tiles and frames as fixed `is_expanded` paths
- `//` paths resolve through `bpy.path.abspath(path, library=block.library)`, linked data relative to its library
- Unsaved files resolve `//` against the process working directory in `bpy.path.abspath` (`tex.png` for `//tex.png`) and in render writes
- Script writes, path rewrites included, leave `bpy.data.is_dirty` false in GUI and background runs
- `wm.save_as_mainfile` rewrites `//` paths for the new folder (`//tex.png` from `a/` into `b/deep/` reads `//../../a/tex.png`)
- `relative_remap=False` keeps each `//` path's text, retargeting it against the new folder
- `copy=True` remaps the written copy and leaves session paths and `bpy.data.filepath` unchanged
- Saves with `compress` unset keep a saved file's compression and read `use_file_compression` for a file never saved
- Portable files pack external data through `file.pack_all`, and images from a temporary source pack before the source's removal
- Script saves write modified images under `save_modified_images` `ALWAYS_SAVE` alone, ignoring dialog setting, code calling `image.save()` otherwise
- `bpy.app.tempdir` names a session folder Blender deletes at quit
- `bpy.utils.system_resource(kind, path=)` resolves folders and returns `""` for a file, and the file name joins onto its resolved folder

## [05]-[CUSTOM_PROPERTIES]

Custom properties hold `int`, `float`, `str`, `bytes`, `bool`, `None`, IDs, string-keyed dictionaries, and single-typed lists:
- Integers are signed 32-bit, a larger value raising `OverflowError`
- Floats are doubles and coordinates `float32`, georeferenced geometry storing a local offset with its double origin in an ID property
- Lists mixing strings with numbers raise `TypeError`, lists mixing integers with floats store floats, and integer keys raise `KeyError`
- Keys hold at most 63 UTF-8 bytes, a longer key raising `KeyError`, and keys built from data paths go in values
- Lists of dictionaries read back as a `list` of `IDPropertyGroup` items, `to_dict()` returning each item's plain form
- `id[key] = []` stores an empty integer `IDPropertyArray`
- ID values, alone or in a list, each count as a user and follow renames, and hold the IDs a stored record names
- Bytes written to a key holding bytes stop at the first NUL, and a new key, `update()`, a whole-group write, and `ID.copy()` keep every byte
- `id.id_properties_ui(key).update(min=, max=, default=)` sets the UI range and the Reset to Default value
