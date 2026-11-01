# [DATA]

## [01]-[STORAGE]

Value lifetime picks the owner:

| [INDEX] | [LIFETIME]                            | [OWNER]                                                                                   |
| :-----: | :------------------------------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | One operator call                     | Operator properties, `options={'SKIP_SAVE'}` on values the next call must not reuse       |
|  [02]   | Saved with an ID, node, or view layer | `PointerProperty(type=<group>)` attached to the holding type                              |
|  [03]   | Saved record `bpy.props` cannot state | `msgspec.json` bytes on its ID under the record's class name                              |
|  [04]   | Saved with a window's screen          | `bpy.props` on `Screen`, `Window` and `Space` attachments refusing writes                 |
|  [05]   | User setting across files             | `AddonPreferences` on the extension package                                               |
|  [06]   | Named preset files                    | `bpy.utils.register_preset_path` folder                                                   |
|  [07]   | Session cache                         | Scope-owned dict keyed by `session_uid`, cleared on `load_post`, `undo_post`, `redo_post` |

Records holding unions, 64-bit integers, or nested tuples store as JSON, whose escaped text holds no NUL byte for an overwrite to cut at, and decode against their class at each read:

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

`held(Grid, obj)` answers `ABSENT` before the first `kept`, and `INVALID` with the refused key path once `Grid` changes shape. Record class names stay unique among the records one ID holds, the 63-byte key cap leaving no room for a module prefix.

## [02]-[CREATED_IDS]

Created IDs take their saved lifetime from their users and links:
- `use_fake_user = True` keeps an ID saved with no other user
- `<old>.user_remap(<new>)` moves every user onto a replacement before the old ID's removal
- Writes to data with `users > 1` reach every user, and per-data work runs once per data
- `objects.link` and `children.link` of an item already linked raise `RuntimeError`, and code tests membership before the link
- `bpy.data.scenes.new()` takes factory settings (metric units, EEVEE, no world, no camera), and `bpy.ops.scene.new(type="EMPTY")` copies the window scene's units and engine
- Appends and links fire `blend_import_pre` and `blend_import_post`, where code merges imported duplicates of data it owns

## [03]-[LIBRARY_LOADS]

`libraries.load` takes source names inside its context and holds IDs after it exits, both sides naming the `bpy.data` collection whose `fixed_type` is the nearest class in the requested class's MRO:

```python
@catch(exception=OSError)
def loaded(kind: type[bpy.types.ID], path: Path, chosen: Callable[[str], bool], /, *, link: bool = False) -> tuple[bpy.types.ID, ...]:
    data: bpy.types.Struct = bpy.types.BlendData.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    member = next(prop.identifier for cls in kind.__mro__ for prop in data.properties if isinstance(prop, bpy.types.CollectionProperty) and type(prop.fixed_type) is cls)
    opened: AbstractContextManager[tuple[object, object]] = bpy.data.libraries.load(str(path), link=link, reuse_local_id=not link)  # type: ignore[func-returns-value, assignment]  # ty: ignore[invalid-assignment]
    with opened as (source, target):
        setattr(target, member, list(filter(chosen, getattr(source, member))))
    return tuple(getattr(target, member))
```

`loaded(bpy.types.Material, path, {"Brick", "Wood"}.__contains__)` appends what the source holds of both names, a repeat call returns the same copies, and `bpy.types.SunLight` reads `lights`:
- Appends with `reuse_local_id=True` return the local copy an earlier append made, links return the linked ID already present, and both flags together raise `ValueError`
- Unreadable paths raise `OSError` at `__enter__`, and a name the source lacks loads as `None` with a `UserWarning`
- Loads bring every ID a requested ID references
- `bpy.data.temp_data()` holds another file's loads in a store freed at its exit, session data untouched
- `bpy.data.libraries.write(path, blocks, path_remap=, fake_user=, compress=)` writes a subset with every ID it references, a `path_remap` other than `NONE` keeping `//` paths valid in another folder
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
- Default `flags` skip packed and weak-reference paths, and `EXPAND_TOKENS` and `EXPAND_SEQUENCES` visit each UDIM tile and frame as an `is_expanded` path no return rewrites
- `//` paths resolve through `bpy.path.abspath(path, library=block.library)`, linked data relative to its library
- Unsaved files resolve `//` against the process working directory in `bpy.path.abspath` (`tex.png` for `//tex.png`) and in render writes
- Script writes, path rewrites included, leave `bpy.data.is_dirty` false in GUI and background runs
- `wm.save_as_mainfile` remaps relative paths and `wm.save_mainfile` keeps them
- Saves with `compress` unset keep a saved file's compression and read `use_file_compression` for a file never saved
- Portable files pack external data through `file.pack_all`, and images from a temporary source pack before the source's removal
- Scripted `wm.save_mainfile` under `save_modified_images` `ASK` saves no modified image, and images save through `image.save()`
- `bpy.app.tempdir` names a session folder Blender deletes at quit
- `bpy.utils.system_resource(kind, path=)` resolves folders and returns `""` for a file, and the file name joins onto its resolved folder

## [05]-[CUSTOM_PROPERTIES]

Custom properties hold `int`, `float`, `str`, `bytes`, `bool`, `None`, IDs, string-keyed dictionaries, and single-typed lists:
- Integers are signed 32-bit, a larger value raising `OverflowError`
- Floats are doubles while coordinates are `float32`, and georeferenced geometry stores a local offset with its double-precision origin in an ID property
- Lists mixing strings with numbers raise `TypeError`, lists mixing integers with floats store floats, and integer keys raise `KeyError`
- Keys hold at most 63 UTF-8 bytes, a longer key raising `KeyError`, and keys built from data paths go in values
- Lists of dictionaries read back as a `list` of `IDPropertyGroup` items, `to_dict()` returning each item's plain form
- `id[key] = []` stores an empty integer `IDPropertyArray`
- ID values, alone or in a list, each count as a user and follow renames, and hold the IDs a stored record names
- Bytes assigned to a key already holding bytes keep the bytes before the first NUL, while a new key, `update()`, a whole-group write, and `ID.copy()` keep every byte
- `id.id_properties_ui(key).update(min=, max=, default=)` sets the UI range and the Reset to Default value
