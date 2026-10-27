# [DATA]

Covers storage owners per value lifetime, created IDs, library loads, paths, and custom property values.

## [01]-[STORAGE]

Value lifetime picks the owner:

| [INDEX] | [LIFETIME]                            | [OWNER]                                                                                   |
| :-----: | :------------------------------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | One operator call                     | Operator properties, `options={'SKIP_SAVE'}` on values next call must not reuse           |
|  [02]   | Saved with an ID, node, or view layer | `PointerProperty(type=<group>)` attached to holding type                                  |
|  [03]   | Saved with file, read for session     | `bpy.data.texts` block with `use_fake_user`, copied to window manager                     |
|  [04]   | Typed record holding ID references    | MessagePack bytes beside a group of ID pointers in one ID property                        |
|  [05]   | Saved with a screen, one per window   | `bpy.props` on `Screen`, `Window` and `Space` types registering none                      |
|  [06]   | User setting across files             | `AddonPreferences` on extension package                                                   |
|  [07]   | Named preset files                    | `bpy.utils.register_preset_path` folder                                                   |
|  [08]   | Session cache                         | Scope-owned dict keyed by `session_uid`, cleared on `load_post`, `undo_post`, `redo_post` |

## [02]-[CREATED_IDS]

Created IDs take their saved lifetime from their users:
- `use_fake_user = True` keeps an ID saved with no other user
- `<old>.user_remap(<new>)` moves every user onto a replacement before old ID's removal
- Appends and links fire `bpy.app.handlers.blend_import_post`, where code merges imported duplicates of data it owns

Created and shared data follow host links:
- Writes to data with `users > 1` reach every user, and per-data work runs once per data
- `objects.link` and `children.link` of an item already linked raise `RuntimeError`, and reconcilers test membership before the link
- `bpy.data.scenes.new()` takes factory settings (metric units, EEVEE, no world, no camera), and `bpy.ops.scene.new(type="EMPTY")` copies the window scene's units and engine
- Linked libraries repeat local names, and created IDs map from their source key in place of a name lookup

Creation registers new ID's removal on scope rollback, kept once every later step succeeds

## [03]-[LIBRARY_LOADS]

`libraries.load` takes names and returns IDs after its context exits:
- `data_from` lists source names inside the context, and `data_to` holds loaded IDs after it exits
- `bpy.data.temp_data()` opens another file's data in a store closing with its scope, session data untouched
- Loads take `link`, `pack`, and `reuse_local_id` keywords, `reuse_local_id=True` taking a local copy in place of a `.001` duplicate
- Loads and appends bring every ID a requested ID references, and the created set is the `bpy.data` difference before and after
- Invalid flag sets and the current file raise `ValueError` before loading
- Names the source lacks load as `None` with a `UserWarning`, and `data_from.libraries` lists nested libraries
- `bpy.data.libraries.write(path, blocks, path_remap=, fake_user=, compress=)` writes a subset of IDs with every ID they reference, a `path_remap` other than `NONE` keeping `//` paths valid in another folder
- Undo skips library data and crashes where a library ID points at local data
- Edits to linked data take an override from `ID.override_create(remap_local_usages=)` or `ID.override_hierarchy_create(scene, view_layer, reference=, do_fully_editable=)`
- `IDOverrideLibrary.reset`, `destroy`, and `resync` take their host keywords
- Values read from a file (custom properties, texts) stay data, and untrusted files open with `use_scripts=False`

## [04]-[PATHS]

Path properties hold blend-relative and byte paths:
- `//` paths resolve through `bpy.path.abspath(path, library=block.library)`, linked data relative to its library
- Unsaved files resolve `//` against the process working directory in `bpy.path.abspath` (`tex.png` for `//tex.png`) and in render writes
- `bpy.app.tempdir` names a session folder Blender deletes at quit
- `bpy.data.file_path_map()` maps IDs to paths
- `bpy.data.file_path_foreach(fn)` calls `fn(owner, path, meta)` and rewrites each path to the string it returns
- `bpy.data.user_map(subset=, key_types=, value_types=)` answers the users of each ID in one call
- `wm.save_as_mainfile` remaps relative paths and `wm.save_mainfile` keeps them
- Saves with `compress` unset keep a saved file's compression and read `use_file_compression` for a file never saved
- Portable files pack external data through `file.pack_all`
- Importers reference images at absolute paths, and temporary sources pack before removal
- Script writes leave `bpy.data.is_dirty` false in GUI and background runs
- Scripted `wm.save_mainfile` under `save_modified_images` `ASK` saves no modified image, and images save through `image.save()`
- `bpy.utils.system_resource(kind, path=...)` resolves folders and returns `""` for a file, and the file name joins onto its resolved folder

## [05]-[CUSTOM_PROPERTIES]

Custom properties hold `int`, `float`, `str`, `bool`, `None`, ID pointers, string-keyed dictionaries, and single-typed lists:
- Integers are signed 32-bit, a larger value raising `OverflowError`
- Floats are doubles while coordinates are `float32`, and georeferenced geometry stores a local offset with its double-precision origin in an ID property
- Lists mixing strings with numbers raise `TypeError`, and integer-keyed dictionaries raise `KeyError`
- Names hold at most 63 bytes, a longer key raising `KeyError`
- Lists of dictionaries read back as a `list` of `IDPropertyGroup` through `ID.copy()`, undo, and reload, holding keys built from paths as values
- `id[key] = []` stores an empty `IDPropertyArray` in place of a list
- Bytes written to a key holding a byte string keep the bytes before the first NUL, while a new key, a whole-group write, and `ID.copy()` keep every byte
- `id.id_properties_ui(key).update(min=, max=, default=)` sets UI range and the Reset to Default value
