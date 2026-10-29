"""Node editor thumbnails: the cells each editor shows, the change keys and per-package preview state behind them, and the overlay drawing them beside their nodes."""

from collections.abc import Callable, Hashable, Mapping
from contextlib import nullcontext
from enum import auto, Enum
import math
from typing import ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.paths import Struct
from blender.bpy.extension.identity import Identity, Resource
from blender.bpy.render.previews.queue import BACKLOG
from blender.bpy.render.previews.scenes import PreviewLighting
from blender.bpy.render.previews.targets import Editor, NodePreviews, Position, RenderKind
from blender.bpy.ui.drawing.canvas import Align, Canvas, DrawType, Rect, Shadow
from blender.bpy.ui.layout.rows import Line, RowDraw
from blender.bpy.ui.resources.previews import TEXTURES
from blender.bpy.ui.resources.theme import color, Metrics, Spacing
from blender.imaging.color.processors import DisplayView
from blender.records.faults import Outcome
from blender.records.stores import ContentHash

# --- [TYPES] ----------------------------------------------------------------------------


class Status(Enum):
    """Cell thumbnail state."""

    READY = auto()
    QUEUED = auto()
    FAILED = auto()


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

    def read(self, _metrics: Metrics, /) -> Outcome[Option[tuple[NodePreviews, Mapping[Editor, tuple[Cell, ...]]]]]:
        """Reads settings and editor cells while previews show."""
        return self.settings(self.identity).map(lambda settings: Some((settings, self.identity.held[NODE_STATES].cells)) if settings.enabled else Nothing)

    def drawn(self, canvas: Canvas, state: tuple[NodePreviews, Mapping[Editor, tuple[Cell, ...]]]) -> None:
        """Draws cells beside nodes, active node grid last."""
        (settings, shown), metrics = state, canvas.metrics
        theme, style = metrics.preferences.themes[0], metrics.preferences.ui_styles[0].widget
        cache, points, pad, gap, checker = self.identity.held[TEXTURES], metrics.font(style), metrics.spaced(Spacing.BUTTON_Y), metrics.spaced(Spacing.TEMPLATE), metrics.checker()
        marks = {Status.QUEUED: ("…", color(theme.info.info_warning_text)), Status.FAILED: ("!", color(theme.info.info_error_text))} if settings.status else {}
        position = Position[settings.position]

        def cell(item: Cell, rect: Rect, *, several: bool, view: bpy.types.View2D) -> None:
            mark = Option.of_optional(marks.get(item.status))
            inner = Rect(x=rect.x + pad, y=rect.y + pad, width=rect.width - 2 * pad, height=rect.height - 2 * pad)
            centre = (rect.x + rect.width / 2, rect.y + rect.height / 2)
            canvas.frame(rect, fill=color(theme.node_editor.node_backdrop), edge=next((edge for _, edge in mark.to_list()), color(theme.node_editor.wire)), radius=0.0)
            if settings.checker:
                canvas.checkered(inner, checker)
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
            for cells in Option.of_optional(shown.get(editor)).to_list()
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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["NODE_STATES", "Cell", "Counts", "Demand", "NodeState", "NodeThumbnails", "Staged", "Status"]
