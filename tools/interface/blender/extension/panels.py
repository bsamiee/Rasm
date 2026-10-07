# ty: ignore[invalid-argument-type, invalid-assignment, redundant-condition, unresolved-attribute, unresolved-import]
# mypy: disable-error-code="arg-type, assignment, attr-defined, func-returns-value, import-not-found, no-any-return, union-attr, unreachable"
# ruff: file-ignore[invalid-class-name, mutable-class-default, private-member-access, unnecessary-dunder-call]
"""Add-on panels collapsed under their owners, icon sidebar tabs, stock header draws, packed toolbar columns of the workspace's owners for a scope, and one asset shelf per asset kind."""

from collections.abc import Callable, Generator, Iterator
from contextlib import contextmanager
from functools import cache
from importlib.metadata import packages_distributions
from itertools import chain, groupby
from pathlib import Path
import sys
import tomllib
from types import MappingProxyType
from typing import Final, override, TYPE_CHECKING

import addon_utils
from bl_pkg.bl_extension_utils import PKG_MANIFEST_FILENAME_TOML
import bl_ui
from bl_ui.space_toolsystem_common import ToolSelectPanelHelper
import bpy
from packaging.utils import canonicalize_name, parse_wheel_filename

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import IconItems

# --- [CONSTANTS] ------------------------------------------------------------------------

ICONS: Final = MappingProxyType[str, "IconItems"]({
    "Item": "OBJECT_DATA",
    "Tool": "TOOL_SETTINGS",
    "View": "HIDE_OFF",
    "Animation": "ANIM",
    "Node": "NODE",
    "Group": "NODETREE",
    "Options": "OPTIONS",
    "Mask": "MOD_MASK",
    "Scopes": "SEQ_HISTOGRAM",
    "Text": "TEXT",
    "Footage": "FILE_MOVIE",
    "Track": "TRACKER",
    "Stabilization": "TRACKING",
    "Action": "ACTION",
    "Shape Key": "SHAPEKEY_DATA",
    "Strip": "SEQUENCE",
    "Cache": "FILE_CACHE",
    "Proxy": "SEQ_PREVIEW",
    "Annotation": "GREASEPENCIL",
    "Attributes": "FILE_FOLDER",
    "Bookmarks": "BOOKMARKS",
    "Filter": "FILTER",
    "Navigation": "VIEW_PAN",
    "Solve": "CON_CAMERASOLVER",
    "AmbientCG Fetcher": "MATERIAL",
    "Arrange": "ALIGN_JUSTIFY",
    "Attrio": "SPREADSHEET",
    "BlenDiff": "SELECT_DIFFERENCE",
    "Blosm": "WORLD",
    "blosm ape": "ASSET_MANAGER",
    "Boolean": "MOD_BOOLEAN",
    "CAD Helper": "SNAP_ON",
    "Dimensions": "DRIVER_DISTANCE",
    "Formula": "DRIVER",
    "Jupyter": "CONSOLE",
    "MCP for Blender": "INTERNET",
    "MeasureIt_ARCH": "ARROW_LEFTRIGHT",
    "MPFB": "USER",
    "MPR": "MESH_CUBE",
    "MTree": "CURVES",
    "Node Wrangler": "NODE_SEL",
    "NodeToPython": "SCRIPT",
    "osm": "NODE_MATERIAL",
    "PinSolver": "PINNED",
    "Pohlke": "VIEW_ORTHO",
    "Point Cloud": "POINTCLOUD_DATA",
    "Profile": "CURVE_BEZCIRCLE",
    "Sketcher": "LINE_DATA",
    "STEP": "IMPORT",
    "Struct Topo": "MOD_REMESH",
    "SV": "SCRIPTPLUGINS",
    "Sverchok": "SCRIPTPLUGINS",
    "UniV": "UV",
    "VI-Suite": "LIGHT_SUN",
})

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [OWNERS]
def lineage[T](base: type[T]) -> list[type[T]]:
    """Every subclass of the type, each before its own subclasses."""
    return [cls for child in base.__subclasses__() for cls in (child, *lineage(child))]


@cache
def owning(addons: frozenset[str]) -> Callable[[str], str | None]:
    """Resolver from a module name to its owning add-on, read from the enabled add-ons' folders and the wheels their extension manifests list, cached per add-on set."""
    modules = [module for name, module in sys.modules.items() if name in addons and name not in addon_utils._addons_hidden_core]
    folders = {Path(module.__file__).parent if module.__spec__.submodule_search_locations else Path(module.__file__): module.__name__ for module in modules}
    wheels = {
        parse_wheel_filename(Path(wheel).name)[0]: module
        for folder, module in folders.items()
        if addon_utils.check_extension(module)
        for wheel in tomllib.loads((folder / PKG_MANIFEST_FILENAME_TOML).read_text(encoding="utf-8")).get("wheels", ())
    }
    distributions = packages_distributions()

    @cache
    def owner(name: str) -> str | None:
        file, top = Path(sys.modules[name].__file__), name.partition(".")[0]
        bundled = (module for folder, module in folders.items() if file.is_relative_to(folder))
        installed = (wheels[wheel] for wheel in map(canonicalize_name, distributions.get(top, ())) if wheel in wheels)
        return next(chain(bundled, installed), None)

    return owner


# --- [PANELS]
@contextmanager
def collapsed(preferences: bpy.types.Preferences) -> Generator[None]:
    """Re-register reordered stock panels in Blender's order and each add-on panel tree closed with a header, owned by its add-on, with its category icon, for the scope, then re-register each class still registered with the attributes it held before."""
    owner = owning(frozenset(preferences.addons.keys()))

    def place(cls: type[bpy.types.Panel]) -> tuple[str, str, str]:
        return cls.bl_space_type, cls.bl_region_type, contexts[cls]

    def subtree(parent: type[bpy.types.Panel]) -> list[type[bpy.types.Panel]]:
        return [parent, *(member for child, name in parents.items() if name == parent.bl_rna.identifier and child not in roots for member in subtree(child))]

    engines = {owner(engine.__module__) for engine in lineage(bpy.types.RenderEngine) if engine.is_registered} - {None}
    registered = [member for name in bpy.types.__dir__() if isinstance(member := getattr(bpy.types, name), type) and issubclass(member, bpy.types.Panel) and member is not bpy.types.Panel]
    blender = [cls for classes in (bl_ui.classes, *(module.classes for module in bl_ui._modules_loaded)) for cls in classes if isinstance(cls, type) and issubclass(cls, bpy.types.Panel)]
    loaded, parents, contexts, icons = list(dict.fromkeys((*blender, *registered))), dict[type[bpy.types.Panel], str](), dict[type[bpy.types.Panel], str](), dict[type[bpy.types.Panel], "IconItems"]()
    for cls in loaded:
        match cls:
            case type(bl_parent_id=name):
                parents[cls] = name
            case type(bl_context=context):
                contexts[cls] = context
            case _:
                contexts[cls] = ""
        match cls:
            case type(bl_category=category) if category in ICONS:
                icons[cls] = ICONS[category]
            case _:
                pass
    stock, position = {cls.bl_rna.identifier for cls in blender}, {cls: index for index, cls in enumerate(loaded)}
    tops = sorted((cls for cls in registered if position[cls] < len(blender) and cls not in parents), key=place)
    shuffled = {key for key, group in groupby(tops, key=place) if (indices := [position[cls] for cls in group]) != sorted(indices)}
    placements = shuffled | {place(cls) for cls in tops if cls in icons}
    roots = [
        *(cls for cls in loaded if position[cls] < len(blender) and cls not in parents and place(cls) in placements),
        *(cls for cls in loaded if cls.bl_rna.identifier not in stock and (cls not in parents or parents[cls] in stock) and cls.bl_region_type != "HEADER" and owner(cls.__module__) not in engines),
    ]
    subtrees = [subtree(parent) for parent in roots]
    prior = tuple((cls, {key: vars(cls).get(key) for key in ("bl_options", "bl_owner_id", "bl_icon", "bl_icon_value")}) for members in subtrees for cls in members)
    for parent in [cls for cls in roots if cls not in parents and cls.bl_rna.identifier not in stock]:
        match parent:
            case type(bl_options=declared):
                parent.bl_options = {"DEFAULT_CLOSED", *declared} - {"HIDE_HEADER"}
            case _:
                parent.bl_options = {"DEFAULT_CLOSED"}
    for cls in [cls for cls, held in prior if cls in icons and held["bl_icon_value"] is not None]:
        del cls.bl_icon_value
    for members in subtrees:
        for cls in reversed(members):
            bpy.utils.unregister_class(cls)
        for cls in members:
            if (module := owner(cls.__module__)) is not None:
                cls.bl_owner_id = module
            if cls in icons:
                cls.bl_icon = icons[cls]
            bpy.utils.register_class(cls)
    try:
        yield
    finally:
        registered = [cls for cls, _ in prior if cls.is_registered]
        for cls in reversed(registered):
            bpy.utils.unregister_class(cls)
        for cls, key, value in [(cls, key, value) for cls, held in prior for key, value in held.items()]:
            if value is not None:
                setattr(cls, key, value)
            elif key in vars(cls):
                delattr(cls, key)
        for cls in registered:
            bpy.utils.register_class(cls)


def remove_appended(header: type[bpy.types.Header]) -> list[Callable[[bpy.types.Header, bpy.types.Context], None]]:
    """Remove and return every draw function an add-on appended to the header while it registered, which carries its owner."""
    appended = [draw for draw in header._dyn_ui_initialize() if "_owner" in vars(draw)]
    for draw in appended:
        header.remove(draw)
    return appended


# --- [TOOLBAR]
def packed(layout: bpy.types.UILayout, column_count: int, scale_y: float) -> Generator[bpy.types.UILayout | None, bool | None]:
    """Toolbar column layout Blender's tool draw sends each tool and group end to, filling rows across groups, a run of group ends opening one block, each row's cells sharing its width past the 2 widget units an icon cell stays fixed at, and blank cells padding the last row."""
    block, row, filled, fixed_units = layout.column(align=True), None, column_count, 2
    signal = yield None
    while signal is not None:
        match signal:
            case True if row is not None and filled == column_count:
                block, row = layout.column(align=True), None
                signal = yield None
            case True:
                signal = yield None
            case False if filled == column_count:
                row, filled = block.row(align=True), 1
                row.scale_x, row.scale_y = fixed_units + 1, scale_y
                signal = yield row
            case False:
                filled += 1
                signal = yield row
    for _ in range(column_count - filled):
        row.label(text="", icon="BLANK1")
    yield None


def placed(stock: Callable[[type, bpy.types.Context, str | None], Iterator[object]]) -> Callable[[type, bpy.types.Context, str | None], Iterator[object]]:
    """Toolbar tool read that drops each tool of an add-on the workspace's owner filter excludes, reading the preferences from the process, since callers pass any context carrying a workspace and a mode."""

    def tools_from_context(cls: type, context: bpy.types.Context, mode: str | None = None) -> Iterator[object]:
        workspace = context.workspace
        owner, passed = owning(frozenset(bpy.context.preferences.addons.keys())), {None, *(entry.name for entry in workspace.owner_ids)}
        owners = {id(tool._bl_tool): owner(tool.__module__) for tool in lineage(bpy.types.WorkSpaceTool) if workspace.use_filter_by_owner and "_bl_tool" in vars(tool)}
        items = (tuple(tool for tool in item if owners.get(id(tool)) in passed) if type(item) is tuple else item for item in stock(cls, context, mode))
        return (item for item in items if item != () and owners.get(id(item)) in passed)

    return tools_from_context


@contextmanager
def toolbars() -> Generator[None]:
    """Toolbar columns packed and each helper's tool read filtered by the workspace's owners for the scope, the stock layout and reads put back after it."""
    columns = vars(ToolSelectPanelHelper)["_layout_generator_multi_columns"]
    readers = {helper: members["tools_from_context"] for helper in ToolSelectPanelHelper.__subclasses__() if "tools_from_context" in (members := vars(helper))}
    ToolSelectPanelHelper._layout_generator_multi_columns = staticmethod(packed)
    for helper, reader in readers.items():
        helper.tools_from_context = classmethod(placed(reader.__func__))
    try:
        yield
    finally:
        ToolSelectPanelHelper._layout_generator_multi_columns = columns
        for helper, reader in readers.items():
            helper.tools_from_context = reader


# --- [SHELF]
class Shelf(bpy.types.AssetShelf):
    """Asset shelf shown by default, its catalog tabs stored in the preferences."""

    bl_options = {"DEFAULT_VISIBLE", "STORE_ENABLED_CATALOGS_IN_PREFERENCES"}


class VIEW3D_AST_objects(Shelf):
    """Object Mode asset shelf of the object and collection assets in every library."""

    bl_space_type = "VIEW_3D"
    filter_object = True
    filter_group = True

    @classmethod
    @override
    def poll(cls, context: bpy.types.Context | None) -> bool:
        return context.mode == "OBJECT"


class NODE_AST_materials(Shelf):
    """Shader editor asset shelf of the material assets in every library."""

    bl_space_type = "NODE_EDITOR"
    filter_material = True

    @classmethod
    @override
    def poll(cls, context: bpy.types.Context | None) -> bool:
        return context.space_data.tree_type == "ShaderNodeTree"


class IMAGE_AST_worlds(Shelf):
    """Image Editor View mode asset shelf of the world assets in every library."""

    bl_space_type = "IMAGE_EDITOR"
    filter_world = True

    @classmethod
    @override
    def poll(cls, context: bpy.types.Context | None) -> bool:
        return context.space_data.mode == "VIEW"


# --- [COMPOSITION] ----------------------------------------------------------------------

CLASSES: Final = (VIEW3D_AST_objects, NODE_AST_materials, IMAGE_AST_worlds)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CLASSES", "collapsed", "remove_appended", "toolbars"]
