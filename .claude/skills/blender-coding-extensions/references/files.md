# [FILES]

## [01]-[USER_FILES]

Extensions reach files through host folders:
- Files packaged with the extension read through `importlib.resources.files(<package>)`, and upgrades replace that folder, deleting files written beside its modules
- Writes go under `bpy.utils.extension_path_user(<package>, path=<folder>)`, `<extensions>/.user/<repo>/<id>/<folder>`
- `extension_path_user` raises `ValueError` for any name but the three-part `bl_ext.<repo>.<id>`, a check of shape alone

`create=True` prints a failed folder creation and returns `""`, and folders come from `Path.mkdir` under one `catch` keeping the `OSError`:

```python
from pathlib import Path

import bpy
from expression.extra.result import catch


@catch(exception=OSError)
def folder(package: str, name: str) -> Path:
    path = Path(bpy.utils.extension_path_user(package, path=name))
    path.mkdir(parents=True, exist_ok=True)
    return path
```

## [02]-[NETWORK]

Network calls run from an explicit user action and read `bpy.app.online_access` at each call:
- `--online-mode` and `--offline-mode` force `online_access` over `preferences.system.use_online_access`, setting `bpy.app.online_access_override`
- Refusals name `--offline-mode` while `online_access_override` reads `True`, else the System preference

## [03]-[FILE_HANDLERS]

`bpy.types.FileHandler` classes route file drops and collection exports to operators:
- `bl_import_operator` and `bl_export_operator` name the operators, and `bl_file_extensions` lists extensions as `".a;.b"`, matched without case
- Handlers take a drop their `poll_drop` classmethod answers `True` for, and drops matching more than one handler open a menu
- Drops invoke the import operator with first matching path in `filepath` and every matching path in `directory` and `files`
- `collection.exporters.new(<handler bl_idname>, name=<name>)` adds a collection exporter holding the export operator's properties
- Collection exports run `EXEC_DEFAULT` over stored values, an empty `filepath` becoming `//<collection name><first extension>`

Drop operators declare `directory` and `files` with `SKIP_SAVE` and `HIDDEN`, read every path from them, and invoke through `ImportHelper.invoke_popup`, a confirmation for a dropped path and the browser otherwise:

```python
from pathlib import Path
from typing import override, TYPE_CHECKING

import bpy
from bpy_extras.io_utils import ImportHelper, poll_file_object_drop

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums


class Import(bpy.types.Operator, ImportHelper):
    bl_idname = "item.import_item"
    bl_label = "Import Item"
    bl_options = frozenset({"REGISTER", "UNDO"})  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

    filter_glob: bpy.props.StringProperty(default="*.item", options={"HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    directory: bpy.props.StringProperty(subtype="DIR_PATH", options={"SKIP_SAVE", "HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    files: bpy.props.CollectionProperty(type=bpy.types.OperatorFileListElement, options={"SKIP_SAVE", "HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @override
    def invoke(self, context: bpy.types.Context, event: bpy.types.Event) -> set[str]:  # type: ignore[override]  # ty: ignore[invalid-method-override]
        return self.invoke_popup(context)

    @override
    def execute(self, context: bpy.types.Context) -> set["rna_enums.OperatorReturnItems"]:
        paths = tuple(Path(self.directory, file.name) for file in self.files)
        self.report({"INFO"}, ", ".join(path.name for path in paths))
        return {"FINISHED"}


class Handler(bpy.types.FileHandler):
    bl_idname = "ITEM_FH_item"
    bl_label = "Item"
    bl_import_operator = Import.bl_idname
    bl_file_extensions = ".item"

    @override
    @classmethod
    def poll_drop(cls, context: bpy.types.Context | None) -> bool:
        return poll_file_object_drop(context)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
```

`poll_file_object_drop` accepts drops on 3D Viewport and on Outliner in View Layer mode.
