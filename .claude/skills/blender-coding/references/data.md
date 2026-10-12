# [DATA]

Covers where data is stored and how long it persists.

## [01]-[STORAGE]

Value lifetime picks the owner:

| [INDEX] | [LIFETIME]                                | [OWNER]                                                                              |
| :-----: | :---------------------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | One operator call                         | `Operator.form` record, `SKIP_SAVE` in a field's `Definition` options                |
|  [02]   | Saved with an ID, node, or view layer     | `settings.attachments.Attachment`                                                    |
|  [03]   | Saved with the file, read for the session | `settings.texts.TextRecord`, a fake-user text mirrored on the window manager         |
|  [04]   | Typed record holding ID references        | `settings.attachments.Stored`, MessagePack beside the referenced IDs                 |
|  [05]   | Saved with a screen, one per window       | `bpy.props` on `Screen`, `Window` and `Space` types registering none                 |
|  [06]   | User setting across files                 | `ui.surfaces.preferences.PreferencesPage`, written through `preferred`               |
|  [07]   | Named preset files                        | `settings.presets.Preset` under `Identity.folder`                                    |
|  [08]   | Session cache                             | Scope-owned dict keyed by `Handle`, cleared on `load_post`, `undo_post`, `redo_post` |

## [02]-[CREATED_IDS]

Each created kind takes one `Ownership` row:
- `Saved.NEVER` marks runtime data the file never saves, `Saved.USED` saves while used, `Saved.ALWAYS` sets a fake user
- `made` returns the earliest holder of an equal `Mark` (key, source ID, content hash) before it builds, `single=False` builds per call
- `rebuilt` remaps users onto the replacement, removes the predecessor, and restores its name
- `removed` drops unused `Saved.USED` holders
- Saved single rows merge imported duplicates on `blend_import_post`

Created and shared data follow host links:
- Writes to data with `users > 1` reach every user, and per-data work runs once per data
- `objects.link` and `children.link` of an item already linked raise `RuntimeError`, and reconcilers test membership before the link
- `bpy.data.scenes.new()` takes factory settings (metric units, EEVEE, no world, no camera), and `bpy.ops.scene.new(type="EMPTY")` copies the window scene's units and engine
- Linked libraries repeat local names, and created IDs map from their source key in place of a name lookup

Creation outside `Ownership` takes a rollback that removes the ID on any later refusal:

```python
def founded(parent: Option[bpy.types.Collection], name: str) -> BlenderResult[bpy.types.Collection]:
    with ExitStack() as rollback:
        made: bpy.types.Collection = bpy.data.collections.new(name)
        rollback.callback(bpy.data.batch_remove, ids=(made,))
        linked: BlenderResult[bpy.types.Collection] = named(made, name).bind(lambda block: parent.map(lambda held: joined(held, block)).default_value(Ok(None)).map(lambda _: block))
        for _ in linked.to_option().to_list():
            rollback.pop_all()
        return linked
```

`joined` stands for the `(Collection, Collection) -> BlenderResult[None]` parent link step.

## [03]-[LIBRARY_LOADS]

`libraries.load` takes names and hands IDs back after its context exits:
- `files.blends.sourced(data, path, take, **keywords)` runs `take` over the source listing and returns its value with the loaded IDs per store
- `files.blends.inspected(path)` yields another file's data in a `bpy.data.temp_data()` store closing with the scope, session data untouched
- `files.libraries.blocks_loaded(path, kind, names, link)` takes `Linked`, `LocalCopy`, or `Overrides` and accumulates every absent name
- Loads and appends bring every ID a requested ID references, and the created set is the `bpy.data` difference before and after
- Invalid flag sets and the current file raise `ValueError` before loading
- Names the source lacks load as `None` with a `UserWarning`, and `data_from.libraries` lists nested libraries
- `files.libraries.library_written(path, blocks, remap, fake_user, compress)` writes a subset of IDs with every ID they reference, a remap other than `NONE` keeping `//` paths valid in another folder
- Undo skips library data and crashes where a library ID points at local data
- Edits to linked data go through `files.libraries.overridden`
- Values read from a file (custom properties, texts) stay data, and untrusted files open with `use_scripts=False`

## [04]-[PATHS]

Path properties hold blend-relative and byte paths:
- `//` paths resolve through `bpy.path.abspath(path, library=block.library)`, linked data relative to its library
- `bpy.path.abspath("//tex.png")` in an unsaved file returns `tex.png`, a path file access resolves against the process working directory
- `files.blends.dependencies` reads every external path with its presence
- `bpy.data.file_path_map()` maps IDs to paths
- `bpy.data.file_path_foreach(fn)` calls `fn(owner, path, meta)` and rewrites each path to the string it returns
- `bpy.data.user_map(subset=, key_types=, value_types=)` answers the users of each ID in one call
- `wm.save_as_mainfile` remaps relative paths and `wm.save_mainfile` keeps them
- Saves with `compress` unset keep a saved file's compression and read `use_file_compression` for a file never saved
- Portable files pack images (`file.pack_all`) or go through `files.blends.packed`, importers referencing images at absolute paths and temporary sources packing before removal
- Script writes leave `bpy.data.is_dirty` false in GUI and background runs
- Scripted `wm.save_mainfile` under `save_modified_images` `ASK` saves no modified image, and images save through `image.save()`
- `bpy.utils.system_resource(kind, path=...)` resolves folders and returns `""` for a file, and the file name joins onto its resolved folder

## [05]-[CUSTOM_PROPERTIES]

Custom properties hold `int`, `float`, `str`, `bool`, `None`, ID pointers, string-keyed dictionaries, and single-typed lists:
- Integers are signed 32-bit, a larger value raising `OverflowError`
- Floats are doubles while coordinates are `float32`, and georeferenced geometry stores a local offset with its double-precision origin in an ID property
- Lists mixing strings with numbers raise `TypeError`, and integer-keyed dictionaries raise `KeyError`
- Names hold at most 63 characters
- `id.id_properties_ui(key).update(min=, max=, default=)` sets UI range and the Reset to Default value
