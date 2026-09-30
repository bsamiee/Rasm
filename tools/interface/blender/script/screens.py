# ty: ignore[unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined"
"""Blender's workspace layouts as split trees, the screen each one builds at its extents, and the region widths the host stores."""

from collections.abc import Iterator, Sequence
from enum import auto, Enum, StrEnum
from inspect import signature
from itertools import chain, repeat
from math import floor
from types import ModuleType
from typing import Final, Literal

from attrs import frozen
from bl_ui.space_toolsystem_common import ToolSelectPanelHelper
import blf
import bpy

from interface.blender.rows import JSON, Width
from interface.blender.script.rna import converge
from interface.frame import LOWER_EDITOR, RIGHT_COLUMN, Task, TREE_ROWS
from interface.report import changes, Kind, line, subscript

# --- [TYPES] ----------------------------------------------------------------------------


class Side(StrEnum):
    """Side of a split's sized part along its cut: the top of a lower part, the left of a right part, or the right of a left part."""

    TOP = "top"
    LEFT = "left"
    RIGHT = "right"

    @property
    def direction(self) -> Literal["HORIZONTAL", "VERTICAL"]:
        """Direction `screen.area_split` cuts along this edge."""
        return "HORIZONTAL" if self is Side.TOP else "VERTICAL"

    def bounds(self, areas: Sequence[bpy.types.Area]) -> tuple[int, int]:
        """Lowest start and highest end the areas reach across a cut along this edge, in window device pixels."""
        return (min(area.y for area in areas), max(area.y + area.height for area in areas)) if self is Side.TOP else (min(area.x for area in areas), max(area.x + area.width for area in areas))

    def order(self, area: bpy.types.Area) -> int:
        """Key ordering areas top to bottom across a horizontal cut and left to right across a vertical one."""
        return -area.y if self is Side.TOP else area.x

    def clear(self, first: Sequence[bpy.types.Area], second: Sequence[bpy.types.Area]) -> bool:
        """Whether a cut along this edge runs clear between the first part's areas and the second's."""
        (first_low, first_high), (second_low, second_high) = self.bounds(first), self.bounds(second)
        return first_low >= second_high if self is Side.TOP else second_low >= first_high


class View(StrEnum):
    """View a layout's canvas opens on, an axis view by its `view3d.view_axis` type."""

    PERSPECTIVE = "PERSP"
    TOP = "TOP"
    CAMERA = "CAMERA"


class Extent(Enum):
    """Size a split gives its sized part, by the editor that sets it."""

    COLUMN = auto()
    LOWER = auto()
    TABLE = auto()
    TREE = auto()
    REPORTS = auto()
    CONSOLE = auto()
    CODE = auto()

    def pixels(self, preferences: bpy.types.Preferences) -> int:
        """Device pixels of the part: frame points, an editor's header over whole rows, or the Text Editor's margin columns beside a four-digit gutter and its scroll bar."""
        system = preferences.system
        header, unit, scale = header_size(system), widget_unit(system), system.ui_scale
        match self:
            case Extent.COLUMN:
                return device_pixels(preferences, RIGHT_COLUMN)
            case Extent.LOWER:
                return device_pixels(preferences, LOWER_EDITOR)
            case Extent.TABLE:
                table_points = 480
                return device_pixels(preferences, table_points)
            case Extent.TREE:
                ol_y_offset = 2
                return header + ol_y_offset + TREE_ROWS * unit
            case Extent.REPORTS:
                info_line, info_padding, info_margin, report_rows = 17, 0.4, 0.45, 5
                report = int(info_line * scale)
                return header + int(info_margin * unit) + report_rows * (report + 2 * int(info_padding * report))
            case Extent.CONSOLE:
                console_margin, console_rows = 4, 10
                return header + int(console_margin * scale) + console_rows * int(CONSOLE_SIZE * scale)
            case Extent.CODE:
                face, unit_points = preferences.view.font_path_ui_mono, 20
                font = blf.load(face)
                try:
                    blf.size(font, unit * TEXT_SIZE // unit_points)
                    advance = int(blf.dimensions(font, "0")[0])
                finally:
                    blf.unload(face)
                gutter_digits, txt_numcol_pad, txt_body_lpad = 4, 1, 1
                return advance * (gutter_digits + 2 * txt_numcol_pad + txt_body_lpad + MARGIN_COLUMN) + unit


# --- [CONSTANTS] ------------------------------------------------------------------------

TICK: Final = 0.1
CONSOLE_SIZE: Final = 14
TEXT_SIZE: Final = 12
MARGIN_COLUMN: Final = 80

# --- [MODELS] ---------------------------------------------------------------------------


@frozen
class Split:
    """Area cut along the side's edge into a first part, top or left, and a second part, bottom or right, its sized part at the extent, each part a split or a leaf area's editor by its `ui_type`."""

    side: Side
    first: "str | Split"
    second: "str | Split"
    extent: Extent


@frozen(kw_only=True)
class Layout:
    """Workspace layout of a task: the split tree left of the right column, Properties context and Bonsai tab, canvas shading, view, and whether its plan frames the modeling extent, asset shelf editors, and canvas sidebar tab."""

    task: Task
    body: str | Split = "VIEW_3D"
    context: str
    tab: str = "BLENDER"
    shading: str = "SOLID"
    color_type: str = "SINGLE"
    view: View = View.PERSPECTIVE
    reach: bool = False
    shelf: tuple[str, ...] = ()
    sidebar: str | None = None

    @property
    def screen(self) -> Split:
        """Split tree of the screen, the body beside the right column of Properties over the Outliner."""
        return Split(Side.LEFT, self.body, Split(Side.TOP, "PROPERTIES", "OUTLINER", Extent.TREE), Extent.COLUMN)


# --- [LAYOUTS] --------------------------------------------------------------------------

LAYOUTS: Final = (
    Layout(task=Task.MODELING, context="OBJECT", shelf=("VIEW_3D",)),
    Layout(task=Task.SITE, context="WORLD", color_type="TEXTURE", view=View.TOP, reach=True, shelf=("VIEW_3D",), sidebar="View"),
    Layout(task=Task.NODES, body=Split(Side.TOP, Split(Side.RIGHT, "SPREADSHEET", "VIEW_3D", Extent.TABLE), "GeometryNodeTree", Extent.LOWER), context="MODIFIER"),
    Layout(task=Task.BIM, context="OBJECT", tab="PROJECT", color_type="MATERIAL"),
    Layout(
        task=Task.SHADING, body=Split(Side.TOP, "VIEW_3D", Split(Side.RIGHT, "UV", "ShaderNodeTree", Extent.LOWER), Extent.LOWER), context="MATERIAL", shading="MATERIAL", shelf=("ShaderNodeTree",)
    ),
    Layout(task=Task.DRAFTING, body=Split(Side.TOP, "VIEW_3D", "VIEW_3D", Extent.LOWER), context="SCENE", view=View.TOP),
    Layout(
        task=Task.RENDERING,
        body=Split(Side.TOP, "VIEW_3D", Split(Side.RIGHT, "IMAGE_EDITOR", "CompositorNodeTree", Extent.LOWER), Extent.LOWER),
        context="RENDER",
        view=View.CAMERA,
        shelf=("IMAGE_EDITOR", "CompositorNodeTree"),
    ),
    Layout(task=Task.SCRIPTING, body=Split(Side.RIGHT, Split(Side.TOP, "TEXT_EDITOR", "CONSOLE", Extent.CONSOLE), Split(Side.TOP, "VIEW_3D", "INFO", Extent.REPORTS), Extent.CODE), context="OBJECT"),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [UNITS]
def widget_unit(system: bpy.types.PreferencesSystem) -> int:
    """Device pixels of Blender's widget unit, 18 units at the scale factor between two line widths."""
    widget_body = 18
    return floor(widget_body * system.ui_scale + 0.5) + 2 * int(system.pixel_size)


def header_size(system: bpy.types.PreferencesSystem) -> int:
    """Device pixels of an area header, `ED_area_headersize`: a widget unit over the scaled `HEADER_PADDING_Y`."""
    header_padding_y = 6
    return widget_unit(system) + int(system.ui_scale * header_padding_y)


def device_pixels(preferences: bpy.types.Preferences, points: int) -> int:
    """Device pixels of a length in window points at the display's native pixel size, the scale factor over the interface scale."""
    return points * round(preferences.system.ui_scale / preferences.view.ui_scale)


def tool_zoom(preferences: bpy.types.Preferences) -> float:
    """Toolbar zoom that draws a tool row 24 points tall, a row `draw_cls` draws as its scaled widget unit truncated to whole device pixels."""
    tool_row = 24
    return device_pixels(preferences, tool_row) / int(signature(ToolSelectPanelHelper.draw_cls).parameters["scale_y"].default * widget_unit(preferences.system))


# --- [LABELS]
def workspace_label(workspace: bpy.types.WorkSpace) -> str:
    """Report label of the workspace by its name."""
    return subscript("workspaces", workspace.name)


def area_label(workspace: bpy.types.WorkSpace, area: bpy.types.Area) -> str:
    """Report label of an area of the workspace's screen by its path from the screen."""
    return f"{workspace_label(workspace)}.screens[0].{area.path_from_id()}"


def region_label(workspace: bpy.types.WorkSpace, area: bpy.types.Area, region: bpy.types.Region) -> str:
    """Report label of a region of an area by its index."""
    return f"{area_label(workspace, area)}.regions[{area.regions[:].index(region)}]"


# --- [CONTEXT]
def override(window: bpy.types.Window, area: bpy.types.Area | None = None, region: bpy.types.Region | None = None) -> bpy.types.ContextTempOverride:
    """Context override on the window and its screen."""
    return bpy.context.temp_override(window=window, screen=window.screen, area=area, region=region)


# --- [WORKSPACES]
def activate(window: bpy.types.Window, name: str) -> Iterator[float]:
    """Show the workspace in the window, one pass for the switch and one for its screen's draw."""
    window.workspace = bpy.data.workspaces[name]
    yield from repeat(TICK, 2)


def duplicate(window: bpy.types.Window, name: str) -> Iterator[float]:
    """Duplicate the window's workspace and rename the copy once the pass after the call shows it."""
    before = set(bpy.data.workspaces)
    with override(window):
        bpy.ops.workspace.duplicate()
    copy = next(workspace for workspace in bpy.data.workspaces if workspace not in before)
    yield TICK
    copy.name = name


def delete(window: bpy.types.Window, name: str) -> Iterator[float]:
    """Show the workspace and delete it, the file dropping it one pass later."""
    yield from activate(window, name)
    with override(window):
        bpy.ops.workspace.delete()
    yield TICK


def shaped_workspaces(window: bpy.types.Window, unit_system: ModuleType) -> Iterator[float | str]:
    """Set every workspace to Object Mode, add each missing layout's workspace, and delete every workspace no layout names."""
    names, before = tuple(str(layout.task) for layout in LAYOUTS), tuple(workspace.name for workspace in bpy.data.workspaces)
    yield from chain.from_iterable(converge(unit_system, workspace_label(workspace), workspace, {"object_mode": "OBJECT"}) for workspace in bpy.data.workspaces)
    yield from chain.from_iterable(duplicate(window, name) for name in names if name not in bpy.data.workspaces)
    yield from chain.from_iterable(delete(window, name) for name in [workspace.name for workspace in bpy.data.workspaces if workspace.name not in names])
    yield from changes("workspaces", before, tuple(sorted(names)))


# --- [TREES]
def leaves(node: str | Split) -> tuple[str, ...]:
    """Editors of the tree's leaves in tree order, each first part before its second."""
    match node:
        case str():
            return (node,)
        case Split(first=first, second=second):
            return (*leaves(first), *leaves(second))


def parted(split: Split, areas: Sequence[bpy.types.Area]) -> tuple[tuple[bpy.types.Area, ...], tuple[bpy.types.Area, ...]]:
    """Areas of the split's first and second parts, ordered across its cut, the first part holding as many areas as its leaves."""
    ordered, count = sorted(areas, key=split.side.order), len(leaves(split.first))
    return tuple(ordered[:count]), tuple(ordered[count:])


def arranged(node: str | Split, areas: Sequence[bpy.types.Area]) -> bool:
    """Whether areas as many as the tree's leaves tile it, each split's cut running clear between its parts."""
    match node:
        case str():
            return True
        case Split(side=side, first=first, second=second):
            upper, lower = parted(node, areas)
            return side.clear(upper, lower) and arranged(first, upper) and arranged(second, lower)


def paired(node: str | Split, areas: Sequence[bpy.types.Area]) -> tuple[tuple[str, bpy.types.Area], ...]:
    """Each leaf's editor of the tree with the area it holds on a screen the tree tiles, in tree order."""
    match node:
        case str():
            return ((node, areas[0]),)
        case Split(first=first, second=second):
            upper, lower = parted(node, areas)
            return (*paired(first, upper), *paired(second, lower))


def extents(node: str | Split, areas: Sequence[bpy.types.Area]) -> tuple[int, ...]:
    """Extent each split's sized part spans across its cut on a screen the tree tiles, splits before their parts."""
    match node:
        case str():
            return ()
        case Split(side=side, first=first, second=second):
            upper, lower = parted(node, areas)
            low, high = side.bounds(upper if side is Side.RIGHT else lower)
            return (high - low, *extents(first, upper), *extents(second, lower))


def targets(preferences: bpy.types.Preferences, node: str | Split) -> tuple[int, ...]:
    """Declared extent of each split's sized part, splits before their parts."""
    match node:
        case str():
            return ()
        case Split(first=first, second=second, extent=extent):
            return (extent.pixels(preferences), *targets(preferences, first), *targets(preferences, second))


# --- [SCREEN]
def cut(preferences: bpy.types.Preferences, areas: Sequence[bpy.types.Area], area: bpy.types.Area, split: Split) -> float:
    """Factor `screen.area_split` takes to leave the split's sized part at its extent: the cut's offset from the area's low vertex over its vertex span, each side padded as `area_calc_totrct` pads it and a single area unpadded."""
    system, side, border = preferences.system, split.side, preferences.view.border_width
    inner, edge = int(max(border * system.ui_scale, system.ui_scale)), int(min(2 * system.ui_scale, border * system.ui_scale))
    (low, high), (window_low, window_high) = side.bounds((area,)), side.bounds(areas)
    near, far = inner if low > window_low else edge, inner if high < window_high else int(system.pixel_size) if side is Side.TOP else edge
    span, target = high - low + (near + far if len(areas) > 1 else 0), split.extent.pixels(preferences)
    return (span - target - far - inner if side is Side.LEFT else target + inner + near - 1) / span


def divided(window: bpy.types.Window, preferences: bpy.types.Preferences, area: bpy.types.Area, node: str | Split) -> Iterator[float | str]:
    """Cut the area at each split's sized extent, the two parts ordered across the cut, splits before their parts, and an error line for a split the area refused as too small."""
    match node:
        case str():
            return
        case Split(side=side, first=first, second=second):
            before, factor = frozenset(each.as_pointer() for each in window.screen.areas), cut(preferences, window.screen.areas[:], area, node)
            with override(window, area):
                bpy.ops.screen.area_split(direction=side.direction, factor=factor)
            yield TICK
            match [each for each in window.screen.areas if each.as_pointer() not in before]:
                case [added]:
                    upper, lower = sorted((area, added), key=side.order)
                    yield from divided(window, preferences, upper, first)
                    yield from divided(window, preferences, lower, second)
                case _:
                    yield line(Kind.ERROR, f"{area_label(window.workspace, area)} refused its {side} split at factor {factor:.4f}")


def build_screen(window: bpy.types.Window, preferences: bpy.types.Preferences, unit_system: ModuleType, layout: Layout) -> Iterator[float | str]:
    """Rebuild the screen from its largest area when its areas do not tile the layout's tree at its extents, each split cut at its sized extent, then give each area its leaf's editor."""
    screen, tree, workspace = window.screen, layout.screen, window.workspace
    if (held := extents(tree, screen.areas[:]) if len(screen.areas) == len(leaves(tree)) and arranged(tree, screen.areas[:]) else ()) != (wanted := targets(preferences, tree)):
        before, keep = (tuple(area.ui_type for area in screen.areas), held), max(screen.areas, key=lambda area: area.width * area.height)
        for area in [area for area in screen.areas if area != keep]:
            with override(window, area):
                bpy.ops.screen.area_close()
            yield TICK
        yield from divided(window, preferences, keep, tree)
        yield from changes(f"{workspace_label(workspace)}.screens[0].areas", before, (leaves(tree), wanted))
    yield from chain.from_iterable(converge(unit_system, area_label(workspace, area), area, {"ui_type": editor}) for editor, area in paired(tree, screen.areas[:]))
    yield TICK


# --- [REGIONS]
def region_widths(preferences: bpy.types.Preferences) -> str:
    """Measurement line of the logical width and zoom of the sidebar of every editor a screen places and of each toolbar by editor, types by name and RNA enum value, which the host writes into the saved startup file once Blender quit."""
    zoom, sidebar = tool_zoom(preferences), 260
    toolbar_margin, toolbar_column = 16, 40
    shelf, strip = (int((toolbar_margin + columns * toolbar_column) * zoom) for columns in (2, 1))
    widths = {
        **{
            (area.type, region.type): (sidebar, 1.0) if region.type == "UI" else (strip, zoom)
            for screen in bpy.data.screens
            for area in screen.areas
            for region in area.regions
            if region.type in {"UI", "TOOLS"}
        },
        ("VIEW_3D", "TOOLS"): (shelf, zoom),
        ("SPREADSHEET", "TOOLS"): (155, 1.0),
    }
    spaces, kinds = (owner.bl_rna.properties["type"].enum_items for owner in (bpy.types.Area, bpy.types.Region))
    return line(Kind.MEASUREMENT, JSON.dumps(tuple(Width(editor, spaces[editor].value, kind, kinds[kind].value, width, factor) for (editor, kind), (width, factor) in widths.items())))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CONSOLE_SIZE",
    "LAYOUTS",
    "MARGIN_COLUMN",
    "TEXT_SIZE",
    "TICK",
    "Extent",
    "Layout",
    "Side",
    "Split",
    "View",
    "activate",
    "area_label",
    "build_screen",
    "device_pixels",
    "leaves",
    "override",
    "paired",
    "region_label",
    "region_widths",
    "shaped_workspaces",
    "workspace_label",
]
