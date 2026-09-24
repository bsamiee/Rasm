# ty: ignore[invalid-argument-type, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined, union-attr, unreachable"
# ruff: file-ignore[private-member-access, unnecessary-dunder-call]
"""Add-on panels re-registered closed and owned by the add-on that ships them, and stock panels another add-on reordered put back in Blender's order."""

from importlib.metadata import packages_distributions
from itertools import chain, groupby
from pathlib import Path
import sys
import tomllib

import addon_utils
import bl_ui
import bpy
from packaging.utils import canonicalize_name, parse_wheel_filename

# --- [OPERATIONS] -----------------------------------------------------------------------


def subclasses[T](base: type[T]) -> list[type[T]]:
    """Every registered subclass of the type, each before its own subclasses."""
    return [cls for child in base.__subclasses__() for cls in (child, *subclasses(child)) if cls.is_registered]


def collapse(preferences: bpy.types.Preferences) -> None:
    """Re-register the stock panels of each placement another add-on reordered in Blender's order, then each add-on panel subtree outside a region header and a render engine's add-on, a top-level one closed with its header shown, each class declaring the owner any later re-registration keeps."""
    modules = [module for name, module in sys.modules.items() if name in preferences.addons and name not in addon_utils._addons_hidden_core]
    folders = {Path(module.__file__).parent if module.__spec__.submodule_search_locations else Path(module.__file__): module.__name__ for module in modules}
    wheels = {
        parse_wheel_filename(Path(wheel).name)[0]: module
        for folder, module in folders.items()
        for manifest in folder.glob("blender_manifest.toml")
        for wheel in tomllib.loads(manifest.read_text(encoding="utf-8")).get("wheels", ())
    }
    distributions = packages_distributions()

    def owner(cls: type) -> str:
        file, top = Path(sys.modules[cls.__module__].__file__), cls.__module__.partition(".")[0]
        shipped = (module for folder, module in folders.items() if file.is_relative_to(folder))
        installed = (wheels[name] for name in map(canonicalize_name, distributions.get(top, ())) if name in wheels)
        return next(chain(shipped, installed), "")

    def place(cls: type[bpy.types.Panel]) -> tuple[str, str, str]:
        return cls.bl_space_type, cls.bl_region_type, contexts[cls]

    def subtree(parent: type[bpy.types.Panel]) -> list[type[bpy.types.Panel]]:
        return [parent, *(member for child, name in parents.items() if name == parent.bl_rna.identifier and child not in roots for member in subtree(child))]

    startup = Path(bpy.utils.system_resource("SCRIPTS", path="startup"))
    engines = {owner(engine) for engine in subclasses(bpy.types.RenderEngine)} - {""}
    registered = [cls for name in bpy.types.__dir__() if isinstance(cls := getattr(bpy.types, name), type) and issubclass(cls, bpy.types.Panel) and cls is not bpy.types.Panel]
    blender = [cls for cls in (*bl_ui.classes, *chain.from_iterable(module.classes for module in bl_ui._modules_loaded)) if isinstance(cls, type) and issubclass(cls, bpy.types.Panel)]
    loaded, parents, contexts = list(dict.fromkeys((*blender, *registered))), dict[type[bpy.types.Panel], str](), dict[type[bpy.types.Panel], str]()
    for cls in loaded:
        match cls:
            case type(bl_parent_id=name):
                parents[cls] = name
            case type(bl_context=context):
                contexts[cls] = context
            case _:
                contexts[cls] = ""
    stock, position = {cls.bl_rna.identifier for cls in loaded if Path(sys.modules[cls.__module__].__file__).is_relative_to(startup)}, {cls: index for index, cls in enumerate(loaded)}
    tops = sorted((cls for cls in registered if position[cls] < len(blender) and cls not in parents), key=place)
    shuffled = {key for key, group in groupby(tops, key=place) if (indices := [position[cls] for cls in group]) != sorted(indices)}
    roots = [
        *(cls for cls in loaded if position[cls] < len(blender) and cls not in parents and place(cls) in shuffled),
        *(cls for cls in loaded if cls.bl_rna.identifier not in stock and (cls not in parents or parents[cls] in stock) and cls.bl_region_type != "HEADER" and owner(cls) not in engines),
    ]
    for parent in [cls for cls in roots if cls not in parents and cls.bl_rna.identifier not in stock]:
        match parent:
            case type(bl_options=declared):
                parent.bl_options = {"DEFAULT_CLOSED", *(option for option in declared if option != "HIDE_HEADER" or parent.bl_rna.identifier == "BIM_PT_tabs")}
            case _:
                parent.bl_options = {"DEFAULT_CLOSED"}
    for members in [subtree(parent) for parent in roots]:
        for cls in reversed(members):
            bpy.utils.unregister_class(cls)
        for cls in members:
            cls.bl_owner_id = owner(cls)
            bpy.utils.register_class(cls)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["collapse"]
