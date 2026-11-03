# [FILES]

## [01]-[USER_FILES]

Extensions read packaged files through `importlib.resources.files(__package__)` and write under `bpy.utils.extension_path_user(<package>, path=<folder>)`, `<extensions>/.user/<repo>/<id>/<folder>`:
- Upgrades replace the extension folder, deleting files written beside its modules
- `extension_path_user` takes `bl_ext.<repo>.<id>` alone, raising `ValueError` on a subpackage or add-on module name
- `extension_path_user` checks name shape without an install lookup
- Paths return uncreated, and `create=True` prints a traceback and returns `""` when a file blocks the path

Folders come from `Path.mkdir` under one `catch`, the `OSError` kept as the fault's cause:

```python
def folder(package: str, /, *, name: str = "") -> Outcome[Path]:
    path = Path(bpy.utils.extension_path_user(package, path=name))
    return catch(Path.mkdir, exception=OSError)(path, parents=True, exist_ok=True).map(lambda _: path).map_error(partial(Fault.of, Reason.UNWRITABLE, str(path)))
```

## [02]-[NETWORK]

Network calls run from an explicit user action and read `bpy.app.online_access` at each call:
- `preferences.system.use_online_access` writes set `online_access` at once and clear `bpy.app.online_access_override`
- `--online-mode` and `--offline-mode` force `online_access` and set `online_access_override`
- `--offline-mode` makes preference writes raise `AttributeError`
- Refusals name `--offline-mode` while `online_access_override` reads `True`, else the System preference

## [03]-[FILE_HANDLERS]

`bpy.types.FileHandler` classes route file drops and collection imports to `bl_import_operator` and collection exports to `bl_export_operator`:
- `bl_file_extensions` (`".a;.b"`, case-insensitive) filters drops alone, and the file browser filters by the operator's `filter_glob`
- Handlers take drops their `poll_drop` classmethod returns `True` for, none without one, and drops matching more than one handler open a menu
- Drops call the import operator with the first matching path in `filepath` and every matching path in `directory` and `files`, skipping other files
- Script calls and collection importers set `filepath` alone
- Collection importers run the operator in an empty `context.blend_data` with `scene` `None`, and `collection` or view layer reads crash Blender
- IDs created in the importer's `blend_data` join its collection, and an import creating none returns `CANCELLED`

One import operator serves drops, the file browser, scripts, and collection importers, creating IDs in `context.blend_data` and linking objects only under a scene:

```python
class Import(bpy.types.Operator, ImportHelper):
    bl_idname = "item.import_item"
    bl_label = "Import Item"
    bl_options = frozenset({"REGISTER", "UNDO"})  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

    filter_glob: bpy.props.StringProperty(default="*.item;*.itm", options={"HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    directory: bpy.props.StringProperty(subtype="DIR_PATH", options={"SKIP_SAVE", "HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    files: bpy.props.CollectionProperty(type=bpy.types.OperatorFileListElement, options={"SKIP_SAVE", "HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @override
    def invoke(self, context: bpy.types.Context, event: bpy.types.Event) -> set[str]:  # type: ignore[override]  # ty: ignore[invalid-method-override]
        return self.invoke_popup(context)

    @override
    def execute(self, context: bpy.types.Context) -> Returns:
        data: bpy.types.BlendData = context.blend_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        paths = tuple(Path(self.directory, file.name) for file in self.files) or (Path(self.filepath),)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        created = tuple(data.objects.new(path.stem, None) for path in paths)
        if context.scene and (collection := context.collection):
            for obj in created:
                collection.objects.link(obj)
        return {"FINISHED"}


class Handler(bpy.types.FileHandler):
    bl_idname = "ITEM_FH_item"
    bl_label = "Item"
    bl_import_operator = Import.bl_idname
    bl_file_extensions = Import.__annotations__["filter_glob"].keywords["default"].replace("*", "")

    @override
    @classmethod
    def poll_drop(cls, context: bpy.types.Context | None) -> bool:
        return poll_file_object_drop(context)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
```

`ImportHelper.invoke_popup` confirms a set `filepath`, else opens the file browser. `bpy_extras.io_utils.poll_file_object_drop` accepts drops on the 3D Viewport and on the Outliner in View Layer mode.

## [04]-[COLLECTION_IO]

Collections hold one importer and any number of exporters, each storing its operator's properties in the file:
- `bpy.ops.collection.importer_add(name=<handler bl_idname>, filepath=<path>)` sets the context collection's importer
- `importer_import` imports, and `importer_reload` clears and reimports
- Importer collections refuse links and unlinks with `RuntimeError`
- `importer_make_local` keeps imported objects and drops the importer, and `importer_remove` deletes them
- `collection.exporters.new(<handler bl_idname>, name=<name>)` raises `TypeError` for a handler without a registered export operator
- Exporter names default to the handler's `bl_label`
- `exporter.export_properties` is the export operator's property struct, and each export reads values written there
- Exports run in exec form over stored values with `//` expanded in `filepath`, an empty one becoming `//<collection name><first handler extension>`
- Export operators with a `collection` `StringProperty` receive the collection's name, `context.collection` holding it and window selection unchanged
- `collection.export_all` runs every exporter of the context collection
