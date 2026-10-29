"""Node editor thumbnails: the cells each editor shows, the change keys and per-package preview state behind them, and the overlay drawing them beside their nodes."""

from collections.abc import Callable, Hashable, Mapping
from contextlib import nullcontext
from enum import auto, Enum
from functools import partial
import math
from pathlib import Path
from typing import ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.collections import Block
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.paths import Struct
from blender.bpy.extension.identity import Identity, Resource
from blender.bpy.render.previews.fixtures import PreviewLighting
from blender.bpy.render.previews.queue import BACKLOG
from blender.bpy.render.previews.targets import Editor, NodePreviews, Position, RenderKind
from blender.bpy.ui.drawing.canvas import Align, Canvas, DrawType, Rect, Shadow
from blender.bpy.ui.layout.rows import Line, RowDraw
from blender.bpy.ui.resources.textures import TEXTURES
from blender.bpy.ui.resources.theme import Checker, color, Metrics, Spacing
from blender.imaging.color.processors import DisplayView
from blender.imaging.files.buffers import Depth, Rounded, unassociated
from blender.imaging.files.images import Format, ImageFile, Storage
from blender.records.faults import Fault, Faults, Outcome, RecordReason
from blender.records.identity import ContentHash

# --- [TYPES] ----------------------------------------------------------------------------


class Status(Enum):
    """Cell thumbnail state."""

    READY = auto()
    QUEUED = auto()
    FAILED = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

WRITABLE: Final = Format.writable()
SUFFIXES: Final = tuple(f".{extension}" for row in WRITABLE for extension in row.extensions)

# --- [MODELS] ---------------------------------------------------------------------------


class Cell(msgspec.Struct, frozen=True, kw_only=True):
    """Node thumbnail cell."""

    node: str
    key: Hashable
    label: str
    status: Status
    value: Option[float] = Nothing


class Staged(msgspec.Struct, frozen=True, kw_only=True):
    """Settings a change key hashes."""

    kind: RenderKind
    socket: str
    engine: str
    size: int
    lighting: PreviewLighting
    view: DisplayView
    frame: tuple[int, ...]
    host: tuple[tuple[int, ...], bytes]
    config: str


class Demand(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Held render request and its content hash."""

    staged: Staged
    content: Option[ContentHash]

    @property
    def key(self) -> Hashable:
        """Content hash, else the demand itself."""
        return self.content.default_value(self)


class Shown(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Settings and cells each editor draws."""

    settings: NodePreviews
    cells: Mapping[Editor, tuple[Cell, ...]]


# --- [SERVICES] -------------------------------------------------------------------------


class NodeState:
    """Per-package preview state: the demand each target holds, uniform values by key, content hashes of geometry sources, and editor cells."""

    __slots__ = ("cells", "held", "keys", "values")

    def __init__(self) -> None:
        """Holds no demand, value, held hash, or cell."""
        self.keys: dict[Hashable, Demand] = {}
        self.values: dict[Hashable, float] = {}
        self.held: dict[tuple[Handle[bpy.types.Object], str], Option[ContentHash]] = {}
        self.cells: dict[Editor, tuple[Cell, ...]] = {}

    def forgotten(self) -> tuple[Hashable, ...]:
        """Drops every target key, returning the keys dropped."""
        dropped = tuple(self.keys)
        self.keys.clear()
        return dropped

    def cleared(self) -> int:
        """Drops every held uniform value, answering how many it held."""
        count = len(self.values)
        self.values.clear()
        return count

    def released(self, block: bpy.types.ID) -> None:
        """Drops the content hashes held for a changed object."""
        changed = Handle.of(block)
        self.held = {key: value for key, value in self.held.items() if key[0] != changed}


NODE_STATES: Final = Resource[NodeState](made=lambda _identity: nullcontext(NodeState()), renewed=Some(lambda _held: NodeState()))


class Counts(Line, frozen=True):
    """Queued, failed, and cached thumbnail counts with cache memory."""

    @staticmethod
    def drawn(identity: Identity, /) -> Outcome[RowDraw]:
        """Returns label reading backlog and cache per draw."""

        def shown(layout: bpy.types.UILayout, _holder: Struct, _context: bpy.types.Context) -> None:
            backlog, cache = identity.held[BACKLOG], identity.held[TEXTURES]
            layout.label(text=f"{len(backlog.jobs)} queued, {len(backlog.failed)} failed, {len(cache.held)} cached, {cache.memory / 2**20:.1f} MiB pixels")

        return Ok(shown)


class NodeThumbnails(msgspec.Struct, frozen=True, kw_only=True):
    """Drawn part placing cells beside nodes."""

    draw_type: ClassVar[DrawType] = "POST_VIEW"
    identity: Identity
    settings: Callable[[Identity], Outcome[NodePreviews]]

    def read(self, _metrics: Metrics, /) -> Outcome[Option[Shown]]:
        """Reads settings and editor cells while previews show."""
        return self.settings(self.identity).map(lambda settings: Some(Shown(settings=settings, cells=self.identity.held[NODE_STATES].cells)) if settings.enabled else Nothing)

    def drawn(self, canvas: Canvas, state: Shown) -> None:
        """Draws cells beside nodes, active node grid last."""
        metrics, settings, theme, style = canvas.metrics, state.settings, canvas.metrics.preferences.themes[0], canvas.metrics.preferences.ui_styles[0].widget
        cache, points, pad, gap, checker = self.identity.held[TEXTURES], metrics.font(style), metrics.spaced(Spacing.BUTTON_Y), metrics.spaced(Spacing.TEMPLATE), metrics.checker()
        marks = {Status.QUEUED: ("…", color(theme.info.info_warning_text)), Status.FAILED: ("!", color(theme.info.info_error_text))} if settings.status else {}
        position = Position[settings.position]

        def cell(item: Cell, rect: Rect, *, several: bool, view: bpy.types.View2D) -> None:
            mark = Option.of_optional(marks.get(item.status))
            inner = Rect(x=rect.x + pad, y=rect.y + pad, width=rect.width - 2 * pad, height=rect.height - 2 * pad)
            centre = (rect.x + rect.width / 2, rect.y + rect.height / 2)
            canvas.frame(rect, fill=color(theme.node_editor.node_backdrop), edge=next((edge for _, edge in mark.to_list()), color(theme.node_editor.wire)), radius=0.0)
            if settings.checker:
                checkered(canvas, inner, checker)
            for texture in cache.shown(item.key).to_list():
                canvas.picture(texture, inner, blend="ALPHA_PREMULT")
            for text, _ in mark.to_list():
                canvas.label(text, at=centre, color=canvas.theme.text_hi, size=points, align=Align.CENTER)
            for number in (f"{held:.4g}" for held in item.value.to_list() if settings.values and mark.is_none()):
                if canvas.measured(number, points)[0] <= inner.width:
                    canvas.label(number, at=centre, color=canvas.theme.text_hi, size=points, align=Align.CENTER, shadow=Some(Shadow.of(style)))
            if several:
                with Canvas.clipped(rect, view=Some(view)):
                    canvas.label(item.label, at=(inner.x, inner.y), color=canvas.theme.text, size=points)

        def grid(node: bpy.types.Node, cells: tuple[Cell, ...], grown: float, view: bpy.types.View2D) -> None:
            columns, scale = 2 if len(cells) > 1 else 1, metrics.scale
            left, top = node.location_absolute[0] * scale, node.location_absolute[1] * scale
            side = node.width * scale * settings.scale * grown / columns
            width, height = side * columns, side * math.ceil(len(cells) / columns)
            match position:
                case Position.ABOVE:
                    x, y = left, top + gap
                case Position.BELOW:
                    x, y = left, top - node.dimensions[1] - gap - height
                case Position.LEFT:
                    x, y = left - gap - width, top - height
                case Position.RIGHT:
                    x, y = left + node.dimensions[0] + gap, top - height
            for index, item in enumerate(cells):
                cell(item, Rect(x=x + (index % columns) * side, y=y + height - (index // columns + 1) * side, width=side, height=side), several=columns > 1, view=view)

        views = tuple(
            (tree, cells, tuple(Option.of_optional(tree.nodes.active).filter(lambda _: settings.enlarged).to_list()))
            for space in (canvas.space,)
            if isinstance(space, bpy.types.SpaceNodeEditor)
            for tree in Option.of_optional(space.edit_tree).to_list()
            for editor in Editor.of(space).to_list()
            for cells in Option.of_optional(state.cells.get(editor)).to_list()
        )
        for node, drawn, grown, region in (
            (node, drawn, settings.enlarge if node in active else 1.0, region)
            for region in Option.of_optional(bpy.context.region).to_list()
            for tree, cells, active in views
            for node in (*tuple(node for node in tree.nodes if node not in active), *active)
            for drawn in (tuple(item for item in cells if item.node == node.name),)
            if drawn and node.width * metrics.scale >= metrics.unit / 2
        ):
            grid(node, drawn, grown, region.view2d)


# --- [OPERATIONS] -----------------------------------------------------------------------


def checkered(canvas: Canvas, rect: Rect, checker: Checker) -> None:
    """Draws the transparency checker over a rect as one flat-colored triangle batch, edge squares clipped to the rect."""
    size = float(checker.cell)
    left, bottom = np.meshgrid(rect.x + np.arange(math.ceil(rect.width / size)) * size, rect.y + np.arange(math.ceil(rect.height / size)) * size)
    right, top = np.minimum(left + size, rect.x + rect.width), np.minimum(bottom + size, rect.y + rect.height)
    corners = np.stack((left, bottom, right, bottom, right, top, left, bottom, right, top, left, top), axis=-1).reshape(-1, 2).astype(np.float32)
    odd = (np.indices(left.shape).sum(axis=0) % 2).reshape(-1, 1).astype(bool)
    tints = np.repeat(np.where(odd, np.asarray(checker.primary, np.float32), np.asarray(checker.secondary, np.float32)), 6, axis=0)
    with canvas.shader("FLAT_COLOR") as shader:
        Canvas.batched("FLAT_COLOR", "TRIS", {"pos": corners, "color": tints}).draw(shader)


def imaged(path: Path, plane: npt.NDArray[np.float32], config: PyOpenColorIO.Config, space: str) -> Outcome[Path]:
    """Writes a display-space thumbnail at the deepest depth the suffix writer stores, integer depths coded from unassociated values."""

    def write(depth: Depth) -> Outcome[Path]:
        match depth:
            case Depth.HALF | Depth.FLOAT as floated:
                return ImageFile(pixels=plane, colorspace=space).written(path, Storage(depth=floated), {}, config)
            case Depth.UINT8 | Depth.UINT16 as integer:
                return Rounded.coded(unassociated(plane), integer).bind(lambda codes: ImageFile(pixels=codes, colorspace=space).written(path, Storage(), {}, config))

    suffix = path.suffix.lower()
    chosen = (
        Block
        .of_seq(WRITABLE)
        .filter(lambda row: suffix.removeprefix(".") in row.extensions)
        .try_head()
        .to_result_with(partial(Fault.of, RecordReason.OUTSIDE, "filepath", suffix, member=", ".join(SUFFIXES)))
    )
    return Result[Format, Faults].bind(chosen, lambda row: write(row.depths[-1]))


def value(values: npt.NDArray[np.float32]) -> Option[float]:
    """Returns mean of a field within a thousandth of its magnitude, `Nothing` when varying or empty."""
    return Option.of_optional(values if values.size else None).filter(lambda held: float(np.ptp(held)) <= 1e-3 * max(1.0, float(np.abs(held).max()))).map(lambda held: float(held.mean()))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["NODE_STATES", "SUFFIXES", "WRITABLE", "Cell", "Counts", "Demand", "NodeState", "NodeThumbnails", "Shown", "Staged", "Status", "checkered", "imaged", "value"]
