# ty: ignore[invalid-context-manager, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined, union-attr"
"""Blender's workspaces with each screen's columns, area edges, and region sizes."""

from collections import Counter
from collections.abc import Callable, Iterator
from enum import Enum, StrEnum
from inspect import signature
from itertools import chain, zip_longest
from math import ceil, floor, isclose
from typing import Final, Literal

from attrs import frozen
from bl_ui.space_toolsystem_common import ToolSelectPanelHelper
import blf
import bpy
from mathutils import Vector

from interface.frame import LOWER_EDITOR, RIGHT_COLUMN, TREE_ROWS
from interface.report import Kind, line
from interface.units import Units

# --- [TYPES] ----------------------------------------------------------------------------


class Side(StrEnum):
    """Area edge a resize moves, or the side column an area sits in."""

    TOP = "top"
    LEFT = "left"
    RIGHT = "right"

    def extent(self, area: bpy.types.Area) -> int:
        """Area size along the axis the edge moves."""
        return area.height if self is Side.TOP else area.width

    def grip(self, area: bpy.types.Area, pixel: int) -> tuple[int, int]:
        """Window position one pixel across the edge at its midpoint."""
        match self:
            case Side.TOP:
                return (area.x + area.width // 2, area.y + area.height - 1 + pixel)
            case Side.LEFT:
                return (area.x - pixel, area.y + area.height // 2)
            case Side.RIGHT:
                return (area.x + area.width - 1 + pixel, area.y + area.height // 2)


class Below(StrEnum):
    """Editor under a column's top editor by the ui_type it shows."""

    OUTLINER = "OUTLINER"
    INFO = "INFO"
    CONSOLE = "CONSOLE"
    GEOMETRY_NODES = "GeometryNodeTree"
    SHADER_NODES = "ShaderNodeTree"
    COMPOSITOR = "CompositorNodeTree"
    SHEET = "VIEW_3D"

    @property
    def height(self) -> int:
        """Device pixels of the editor, its header over whole rows at their pitch and margin or the lower editor height."""
        system, pitch = bpy.context.preferences.system, widget_unit()
        header = pitch + int(6 * system.ui_scale)
        match self:
            case Below.OUTLINER:
                return header + 2 + TREE_ROWS * pitch
            case Below.INFO:
                report = int(17 * system.ui_scale)
                return header + int(0.45 * pitch) + 5 * (report + 2 * int(0.4 * report))
            case Below.CONSOLE:
                return header + int(4 * system.ui_scale) + 10 * int(CONSOLE_SIZE * system.ui_scale)
            case Below.GEOMETRY_NODES | Below.SHADER_NODES | Below.COMPOSITOR | Below.SHEET:
                return device_pixels(LOWER_EDITOR)


class Beside(StrEnum):
    """Editor left of a row's main editor or atop the left column by the ui_type it shows."""

    SPREADSHEET = "SPREADSHEET"
    UV = "UV"
    RENDER_RESULT = "IMAGE_EDITOR"
    TEXT = "TEXT_EDITOR"

    @property
    def width(self) -> int:
        """Device pixels of the editor, its tier width or the Text Editor's margin columns beside a four-digit gutter and the scroll bar."""
        match self:
            case Beside.SPREADSHEET:
                return device_pixels(480)
            case Beside.UV | Beside.RENDER_RESULT:
                return device_pixels(LOWER_EDITOR)
            case Beside.TEXT:
                face = bpy.context.preferences.view.font_path_ui_mono
                font = blf.load(face)
                blf.size(font, widget_unit() * TEXT_SIZE // 20)
                advance = int(blf.dimensions(font, "0")[0])
                blf.unload(face)
                return advance * (4 + 3 + MARGIN_COLUMN) + widget_unit()


# --- [CONSTANTS] ------------------------------------------------------------------------

TICK: Final = 0.1
ZOOM_TOLERANCE: Final = 1e-3
CONSOLE_SIZE: Final = 14
TEXT_SIZE: Final = 12
MARGIN_COLUMN: Final = 80

# --- [MODELS] ---------------------------------------------------------------------------


@frozen(kw_only=True)
class Layout:
    """Workspace layout with its left column editors from the top, center rows, Properties context, Bonsai tab, top viewport shading and view, shelf editors, and sidebar."""

    left: tuple[str, ...] = ()
    rows: tuple[tuple[str, ...], ...] = (("VIEW_3D",),)
    context: str
    tab: str = "BLENDER"
    shading: str = "SOLID"
    color_type: str = "SINGLE"
    view: str | None = None
    reach: float | None = None
    shelf: tuple[str, ...] = ()
    sidebar: tuple[str, str] | None = None

    @property
    def columns(self) -> tuple[tuple[str, ...], ...]:
        """Editor types by column from the left, each column from the top."""
        center = (tuple(editor for editor in rank if editor is not None) for rank in zip_longest(*self.rows))
        return (*((self.left,) if self.left else ()), *center, ("PROPERTIES", Below.OUTLINER))


class Workspace(Enum):
    """Workspace layouts by tab name in tab order."""

    Modeling = Layout(context="OBJECT", shelf=("VIEW_3D",))
    Site = Layout(context="WORLD", color_type="TEXTURE", view="TOP", reach=Units.IMPERIAL.extent, shelf=("VIEW_3D",), sidebar=("VIEW_3D", "View"))
    Nodes = Layout(rows=((Beside.SPREADSHEET, "VIEW_3D"), (Below.GEOMETRY_NODES,)), context="MODIFIER")
    BIM = Layout(context="OBJECT", tab="PROJECT", color_type="MATERIAL")
    Shading = Layout(rows=(("VIEW_3D",), (Beside.UV, Below.SHADER_NODES)), context="MATERIAL", shading="MATERIAL", shelf=(Below.SHADER_NODES,))
    Drafting = Layout(rows=(("VIEW_3D",), (Below.SHEET,)), context="SCENE", view="TOP")
    Rendering = Layout(rows=(("VIEW_3D",), (Beside.RENDER_RESULT, Below.COMPOSITOR)), context="RENDER", view="CAMERA", shelf=(Beside.RENDER_RESULT, Below.COMPOSITOR))
    Scripting = Layout(left=(Beside.TEXT, Below.CONSOLE), rows=(("VIEW_3D",), (Below.INFO,)), context="OBJECT")


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [UNITS]
def device_pixels(size: int) -> int:
    """Device pixels of a size in points."""
    preferences = bpy.context.preferences
    return size * round(preferences.system.ui_scale / preferences.view.ui_scale)


def widget_unit() -> int:
    """Device pixels of Blender's widget unit, 18 logical pixels at the interface scale with two line widths."""
    system = bpy.context.preferences.system
    return floor(18 * system.ui_scale + 0.5) + 2 * int(system.pixel_size)


def tool_zoom() -> float:
    """Toolbar zoom that draws a tool row 24 points tall."""
    return device_pixels(24) / (float(signature(ToolSelectPanelHelper.draw_cls).parameters["scale_y"].default) * widget_unit())


# --- [CONTEXT]
def override(window: bpy.types.Window, area: bpy.types.Area | None = None, region: bpy.types.Region | None = None) -> bpy.types.ContextTempOverride:
    """Context override on the window and its screen."""
    return bpy.context.temp_override(window=window, screen=window.screen, area=area, region=region)


# --- [WAITS]
def until(done: Callable[[], bool], step: str, interval: float = TICK) -> Iterator[float]:
    """Yield the interval until the condition holds, raising after fifty passes."""
    budget = 50
    for _ in range(budget):
        if done():
            return
        yield interval
    if not done():
        raise TimeoutError(f"{step} is unfinished after {budget} passes")


# --- [WORKSPACES]
def activate(window: bpy.types.Window, name: str) -> Iterator[float | str]:
    """Show the workspace in the window and yield until its screen refreshed."""
    window.workspace = bpy.data.workspaces[name]
    yield from until(lambda: window.workspace.name == name, f"{name}.activate")
    yield TICK


def duplicate(window: bpy.types.Window, name: str) -> Iterator[float | str]:
    """Duplicate the window's workspace under the name."""
    before = set(bpy.data.workspaces)
    with override(window):
        bpy.ops.workspace.duplicate()
    copy = next(workspace for workspace in bpy.data.workspaces if workspace not in before)
    yield from until(lambda: window.workspace == copy, f"{name}.duplicate")
    copy.name = name


def delete(window: bpy.types.Window, name: str) -> Iterator[float | str]:
    """Delete the workspace and yield until the file drops it."""
    yield from activate(window, name)
    with override(window):
        bpy.ops.workspace.delete()
    yield from until(lambda: name not in bpy.data.workspaces, f"{name}.delete")


def shape_workspaces(window: bpy.types.Window) -> Iterator[float | str]:
    """Set every workspace to Object Mode, add each missing declared workspace, and delete every undeclared one."""
    before = tuple(workspace.name for workspace in bpy.data.workspaces)
    for workspace, held in [(workspace, workspace.object_mode) for workspace in bpy.data.workspaces if workspace.object_mode != "OBJECT"]:
        workspace.object_mode = "OBJECT"
        yield line(Kind.CHANGE, f"{workspace.name}.object_mode", held, "OBJECT")
    yield from chain.from_iterable(duplicate(window, name) for name in [declared.name for declared in Workspace if declared.name not in bpy.data.workspaces])
    yield from chain.from_iterable(delete(window, name) for name in [workspace.name for workspace in bpy.data.workspaces if workspace.name not in Workspace.__members__])
    if set(before) != set(Workspace.__members__):
        yield line(Kind.CHANGE, "workspaces", repr(before), repr(tuple(Workspace.__members__)))


# --- [COLUMNS]
def columns(screen: bpy.types.Screen) -> tuple[tuple[str, ...], ...]:
    """Screen's editor types by column from the left, each column from the top."""
    return tuple(tuple(area.ui_type for area in sorted((area for area in screen.areas if area.x == x), key=lambda area: -area.y)) for x in sorted({area.x for area in screen.areas}))


def names(screen: bpy.types.Screen) -> dict[int, str]:
    """Report name of each area by its pointer, suffixed with its side where two areas share one."""
    top, left = max(area.y + area.height for area in screen.areas), min(area.x for area in screen.areas)
    kinds = [(area, Below(area.ui_type).name if area.ui_type in Below and area.y + area.height < top else area.ui_type) for area in screen.areas]
    counts = Counter(kind for _, kind in kinds)
    return {area.as_pointer(): kind if counts[kind] == 1 else f"{kind}.{Side.LEFT if area.x == left else Side.RIGHT}" for area, kind in kinds}


def split(window: bpy.types.Window, area: bpy.types.Area, direction: Literal["HORIZONTAL", "VERTICAL"], factor: float) -> frozenset[int]:
    """Split the area at the factor and return the screen's areas before the split."""
    before = frozenset(area.as_pointer() for area in window.screen.areas)
    with override(window, area):
        bpy.ops.screen.area_split(direction=direction, factor=factor)
    return before


def parts(window: bpy.types.Window, area: bpy.types.Area, before: frozenset[int], direction: Literal["HORIZONTAL", "VERTICAL"]) -> tuple[bpy.types.Area, bpy.types.Area]:
    """Two parts of a split area, left then right or top then bottom."""
    first, second = sorted((area, next(area for area in window.screen.areas if area.as_pointer() not in before)), key=lambda part: part.x if direction == "VERTICAL" else -part.y)
    return first, second


def stack(window: bpy.types.Window, column: bpy.types.Area, rows: tuple[str, ...]) -> Iterator[float]:
    """Split the column into its top editor and the editors below it at their heights."""
    area = column
    for index, kind in enumerate(rows[:-1]):
        before = split(window, area, "HORIZONTAL", sum(Below(below).height for below in rows[index + 1 :]) / area.height)
        yield TICK
        top, area = parts(window, area, before, "HORIZONTAL")
        top.ui_type = kind
    area.ui_type = rows[-1]
    yield TICK


def build_screen(window: bpy.types.Window, layout: Layout) -> Iterator[float | str]:
    """Rebuild the screen from its largest area when its columns differ from the layout."""
    if (before := columns(window.screen)) == layout.columns:
        return
    keep = max(window.screen.areas, key=lambda area: area.width * area.height)
    for area in [area for area in window.screen.areas if area.as_pointer() != keep.as_pointer()]:
        with override(window, area):
            bpy.ops.screen.area_close()
        yield TICK
    divide = split(window, keep, "VERTICAL", 1 - device_pixels(RIGHT_COLUMN) / keep.width)
    yield TICK
    center, right = parts(window, keep, divide, "VERTICAL")
    if layout.left:
        divide = split(window, center, "VERTICAL", Beside(layout.left[0]).width / center.width)
        yield TICK
        left, center = parts(window, center, divide, "VERTICAL")
        yield from stack(window, left, layout.left)
    yield from stack(window, center, tuple(row[-1] for row in layout.rows))
    yield from stack(window, right, ("PROPERTIES", Below.OUTLINER))
    for row in [row for row in layout.rows if len(row) > 1]:
        area = next(area for area in window.screen.areas if area.x == center.x and area.ui_type == row[-1])
        for editor in row[:-1]:
            divide = split(window, area, "VERTICAL", Beside(editor).width / area.width)
            yield TICK
            side, area = parts(window, area, divide, "VERTICAL")
            side.ui_type = editor
            yield TICK
    yield line(Kind.CHANGE, f"{window.workspace.name}.columns", repr(before), repr(layout.columns))


# --- [EDGES]
def edges(workspace: bpy.types.WorkSpace) -> Iterator[tuple[bpy.types.Area, Side, int]]:
    """Every area edge the layout sizes with its target in device pixels, columns first, lower editors from the bottom up, then side editors."""
    areas, layout = workspace.screens[0].areas, Workspace[workspace.name].value
    right, left, top = max(area.x + area.width for area in areas), min(area.x for area in areas), max(area.y + area.height for area in areas)
    yield next(area for area in areas if area.type == "PROPERTIES" and area.x + area.width == right), Side.LEFT, device_pixels(RIGHT_COLUMN)
    if layout.left:
        yield next(area for area in areas if area.x == left and area.y + area.height == top), Side.RIGHT, Beside(layout.left[0]).width
    yield from ((area, Side.TOP, Below(area.ui_type).height) for area in sorted(areas, key=lambda area: area.y) if area.ui_type in Below and area.y + area.height < top)
    yield from ((next(area for area in areas if area.ui_type == editor), Side.RIGHT, Beside(editor).width) for row in layout.rows for editor in row[:-1])


def resize(window: bpy.types.Window, area: bpy.types.Area, side: Side, target: int, pixel: int) -> Iterator[float | str]:
    """Move one edge of the area so its size along that axis is `target`."""
    if (size := side.extent(area)) == target:
        return
    x, y = side.grip(area, pixel)
    window.event_simulate(type="MOUSEMOVE", value="NOTHING", x=x, y=y)
    yield TICK
    yield TICK
    with override(window):
        bpy.ops.screen.area_move(x=x, y=y, delta=size - target if side is Side.LEFT else target - size)
    yield TICK
    yield line(Kind.CHANGE, f"{window.workspace.name}.{names(window.screen)[area.as_pointer()]}.{side}", str(size), str(target))


# --- [REGIONS]
def stored(width: int, scale: float) -> int | None:
    """Logical width a drag stored for a region `width` device pixels wide, None for a region at its default."""
    held = ceil(width / scale - 0.5)
    return held if int(scale * (held + 0.5)) == width else None


def stroke(window: bpy.types.Window, region: bpy.types.Region, travel: int) -> Iterator[float]:
    """Drag the region's inner edge `travel` device pixels outward from three logical pixels inside it."""
    outward, inset = 1 if region.alignment == "LEFT" else -1, int(3 * bpy.context.preferences.system.ui_scale)
    x, y = region.x + region.width - 1 - inset if outward == 1 else region.x + inset, region.y + region.height // 2
    for kind, value, at in (
        ("MOUSEMOVE", "NOTHING", x),
        ("MOUSEMOVE", "NOTHING", x),
        ("LEFTMOUSE", "PRESS", x),
        ("MOUSEMOVE", "NOTHING", x + outward * travel),
        ("LEFTMOUSE", "RELEASE", x + outward * travel),
    ):
        window.event_simulate(type=kind, value=value, x=at, y=y)
        yield TICK
        yield TICK


def zoomed(region: bpy.types.Region) -> float:
    """Region's view2d zoom, its width in device pixels over its view width."""
    return region.width / ((region.width - 1) * (Vector(region.view2d.region_to_view(1.0, 0.0)) - Vector(region.view2d.region_to_view(0.0, 0.0))).x)


def drag(window: bpy.types.Window, area: bpy.types.Area, region: bpy.types.Region, target: int, zoom: float) -> Iterator[float]:
    """Drag the region's edge to a stored width of `target` logical pixels at the zoom, from a reset view zoomed out to it."""
    scale = bpy.context.preferences.system.ui_scale
    with override(window, area, region):
        bpy.ops.view2d.reset()
        bpy.ops.view2d.zoom_out(zoomfacx=(zoom - 1) / 2, zoomfacy=(zoom - 1) / 2)
    yield TICK
    if stored(region.width, scale) is None:
        yield from stroke(window, region, ceil(target * scale) - region.width)
    if (held := stored(region.width, scale)) is not None and held != target:
        yield from stroke(window, region, (1 if target > held else -1) * ceil(abs(target - held) * scale))


def toggle(window: bpy.types.Window, area: bpy.types.Area, region: bpy.types.Region, label: str) -> Iterator[float | str]:
    """Show a hidden region or hide a shown one and yield until the screen redraws."""
    shown = min(region.width, region.height) > 1
    with override(window, area):
        bpy.ops.screen.region_toggle(region_type=region.type)
    yield from until(lambda: (min(region.width, region.height) > 1) is not shown, f"{label}.toggle", 0.0)


def size_regions(window: bpy.types.Window) -> Iterator[float | str]:
    """Drag every toolbar and sidebar on screen to its logical width and zoom, a hidden one shown for the drag and hidden again."""
    scale, sidebar, zoom, labels = bpy.context.preferences.system.ui_scale, 260, tool_zoom(), names(window.screen)
    shelf, strip = (int((16 + count * 40) * zoom) for count in (2, 1))
    roles = (
        ("VIEW_3D", "TOOLS", shelf, zoom),
        ("VIEW_3D", "UI", sidebar, 1.0),
        ("NODE_EDITOR", "TOOLS", strip, zoom),
        ("NODE_EDITOR", "UI", sidebar, 1.0),
        ("IMAGE_EDITOR", "TOOLS", strip, zoom),
        ("IMAGE_EDITOR", "UI", sidebar, 1.0),
        ("TEXT_EDITOR", "UI", sidebar, 1.0),
        ("SPREADSHEET", "TOOLS", 155, 1.0),
        ("SPREADSHEET", "UI", sidebar, 1.0),
    )
    for area, kind, target, factor in [(area, kind, size, factor) for area in window.screen.areas for editor, kind, size, factor in roles if editor == area.type]:
        region, label = next(region for region in area.regions if region.type == kind), f"{window.workspace.name}.{labels[area.as_pointer()]}.{kind}"
        if hidden := min(region.width, region.height) <= 1:
            yield from toggle(window, area, region, label)
        before, held = region.width, zoomed(region)
        if stored(before, scale) != target or not isclose(held, factor, rel_tol=1e-3):
            yield from drag(window, area, region, target, factor)
            yield line(Kind.CHANGE, label, f"{before} at zoom {held:.4f}", f"{int(scale * (target + 0.5))} at zoom {factor:.4f}")
        if hidden:
            yield from toggle(window, area, region, label)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CONSOLE_SIZE",
    "MARGIN_COLUMN",
    "TEXT_SIZE",
    "TICK",
    "Below",
    "Layout",
    "Workspace",
    "activate",
    "build_screen",
    "edges",
    "names",
    "override",
    "resize",
    "shape_workspaces",
    "size_regions",
    "toggle",
    "until",
]
