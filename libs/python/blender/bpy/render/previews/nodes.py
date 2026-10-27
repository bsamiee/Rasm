"""Per-node preview targets, isolated renders, change keys, and node editor thumbnails."""

from collections.abc import Callable, Generator, Hashable, Mapping
from contextlib import contextmanager, ExitStack, nullcontext
from enum import auto, Enum, IntEnum
import functools
from functools import partial, reduce
import inspect
import itertools
import math
import os
from pathlib import Path
import typing
from typing import Annotated, ClassVar, Final, Self

from bl_ui.space_node import NODE_MT_context_menu
import blf
import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block
from expression.extra.result import catch
import gpu
from mathutils import Vector
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.keymaps import Chord, Keymap
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, Save, tallied, Unchanged
from blender.bpy.data.ids import content
from blender.bpy.data.ids.images import loaded, Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.ids.store import Handle, STORES
from blender.bpy.data.rna.descriptors import choices, Descriptor, RnaPath, rooted, Struct, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events import Change, Handler, updated
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.changes import reconciled
from blender.bpy.nodes.graphs import Expr, Graph
from blender.bpy.nodes.trees import active_output, created, GroupNode, Holder, joined, OutputNode, socket_at, tree_of
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.outputs.results import intermediate, INTERMEDIATES
from blender.bpy.render.previews.queue import BACKLOG, Job, PreviewQueue, Rank
from blender.bpy.render.previews.scenes import Ambient, Fitted, Lamp, PreviewLighting, PreviewMesh, PreviewScene, Readback, Staging, Studio, White
from blender.bpy.scene.materials import Clay, shaded
from blender.bpy.settings.attachments import Attachment
from blender.bpy.settings.presets import removed
from blender.bpy.ui.drawing.canvas import Align, Canvas, Rect, Shadow
from blender.bpy.ui.drawing.handlers import Drawing, Overlay, Signal
from blender.bpy.ui.layout.rows import Line, Member, OperatorButton, Row, RowDraw, SearchField
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.values import Hosted, Store
from blender.bpy.ui.resources.textures import TEXTURES
from blender.bpy.ui.resources.theme import color, Metrics
from blender.bpy.ui.screens.areas import AreaQuery
from blender.bpy.ui.surfaces.appends import Append
from blender.imaging.color.processors import DisplayView
from blender.imaging.files.buffers import unassociated
from blender.imaging.files.coding import Depth, Rounded
from blender.imaging.files.images import Format, ImageFile, read_file, Storage
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.identity import content_hash, ContentHash
from blender.records.trees import Retention

# --- [TYPES] ----------------------------------------------------------------------------


class PreviewScope(IntEnum):
    """Nodes that take a thumbnail."""

    ALL = 1
    SELECTED = 2
    MARKED = 3


class Position(IntEnum):
    """Side of a node its thumbnails sit on."""

    ABOVE = 1
    BELOW = 2
    LEFT = 3
    RIGHT = 4


class RenderKind(IntEnum):
    """Isolated render a node output takes."""

    FIELD_SWATCH = 1
    GEOMETRY_CLAY = 2


class Status(Enum):
    """Cell thumbnail state."""

    READY = auto()
    QUEUED = auto()
    FAILED = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------


WRITABLE: Final = Format.writable()
SUFFIXES: Final = tuple(f".{extension}" for row in WRITABLE for extension in row.extensions)
STORED: Final = tuple(kind for kind in STORES if issubclass(kind, bpy.types.ID) and kind not in {bpy.types.ID, bpy.types.Object})
SCOPES, SURFACES, POSITIONS = (choices((member.name, bpy.path.display_name(member.name.lower()), "") for member in kind) for kind in (PreviewScope, PreviewMesh, Position))
AMBIENTS: Final = choices((kind.__name__, kind.__name__, inspect.getdoc(kind) or "") for kind in typing.get_args(Ambient.__value__))
CLAY_AIM: Final = Vector((0.3, -0.6, 0.9)).normalized().to_track_quat("Z", "Y").to_euler()

# --- [MODELS] ---------------------------------------------------------------------------


class Display(msgspec.Struct, frozen=True, kw_only=True):
    """Node editor thumbnail drawing."""

    scale: Annotated[float, msgspec.Meta(ge=0.25, le=3.0, title="Thumbnail Size", description="Share of node width")] = 1.0
    position: Annotated[Position, msgspec.Meta(description="Node side holding thumbnail")] = Position.ABOVE
    enlarged: Annotated[bool, msgspec.Meta(title="Enlarge Active Node", description="Active node thumbnail larger on top")] = False
    enlarge: Annotated[float, msgspec.Meta(ge=1.25, le=6.0, description="Active node scale while enlarged")] = 2.5
    checker: Annotated[bool, msgspec.Meta(title="Checkerboard", description="Theme checker behind thumbnails")] = False
    status: Annotated[bool, msgspec.Meta(title="Status Marks", description="Queued and failed marks on thumbnails")] = True
    values: Annotated[bool, msgspec.Meta(title="Show Values", description="Number on uniform swatches")] = True
    limit: Annotated[int, msgspec.Meta(ge=16, le=4096, title="Cached Thumbnails", description="Thumbnails kept in GPU memory and on disk")] = 256


class NodePreviews(bpy.types.PropertyGroup):
    """Node preview settings of extension preferences, read as the lighting and drawing records the previews render and draw with."""

    enabled: bpy.props.BoolProperty(name="Show Previews", description="Thumbnails over node editor nodes", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    automatic: bpy.props.BoolProperty(name="Auto Update", description="Changed nodes render again", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    scope: bpy.props.EnumProperty(name="Scope", description="Nodes that take a thumbnail", items=SCOPES)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    every: bpy.props.BoolProperty(name="All Linked Outputs", description="Every linked output side by side")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    frames: bpy.props.BoolProperty(name="Follow Frame", description="Render again on frame change and during playback")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    fields: bpy.props.BoolProperty(name="Field Nodes", description="Shader nodes of geometry trees as flat swatches", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    surface: bpy.props.EnumProperty(name="Surface", description="Shader preview surface", items=SURFACES, default=PreviewLighting().surface.name)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    ambient: bpy.props.EnumProperty(name="Ambient", description="Shader preview ambient light", items=AMBIENTS, default=type(PreviewLighting().ambient).__name__)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    world: bpy.props.StringProperty(name="World", description="Studio light environment", default=Studio().name)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    strength: bpy.props.FloatProperty(name="World Light", description="Ambient light strength", min=0.0, max=10.0, default=PreviewLighting().strength)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    key: bpy.props.FloatProperty(name="Key Light", description="Key sun strength", min=0.0, max=20.0, default=PreviewLighting().key)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    scale: bpy.props.FloatProperty(name="Thumbnail Size", description="Share of node width", subtype="FACTOR", min=0.25, max=3.0, default=Display().scale)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    position: bpy.props.EnumProperty(name="Position", description="Node side holding thumbnail", items=POSITIONS, default=Display().position.name)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    enlarged: bpy.props.BoolProperty(name="Enlarge Active Node", description="Active node thumbnail larger on top", default=Display().enlarged)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    enlarge: bpy.props.FloatProperty(name="Enlarge", description="Active node scale while enlarged", min=1.25, max=6.0, default=Display().enlarge)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    checker: bpy.props.BoolProperty(name="Checkerboard", description="Theme checker behind thumbnails", default=Display().checker)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    status: bpy.props.BoolProperty(name="Status Marks", description="Queued and failed marks on thumbnails", default=Display().status)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    values: bpy.props.BoolProperty(name="Show Values", description="Number on uniform swatches", default=Display().values)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    limit: bpy.props.IntProperty(name="Cached Thumbnails", description="Thumbnails kept in GPU memory and on disk", min=16, max=4096, default=Display().limit)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @property
    def lighting(self) -> PreviewLighting:
        """Shader preview lighting."""
        return PreviewLighting(surface=PreviewMesh[self.surface], ambient=Studio(name=self.world) if self.ambient == Studio.__name__ else White(), strength=self.strength, key=self.key)

    @property
    def display(self) -> Display:
        """Thumbnail drawing."""
        return Display(scale=self.scale, position=Position[self.position], enlarged=self.enlarged, enlarge=self.enlarge, checker=self.checker, status=self.status, values=self.values, limit=self.limit)


class NodeSetting(bpy.types.PropertyGroup):
    """Node thumbnail settings."""

    show: bpy.props.BoolProperty(name="Show Preview", description="Thumbnail under Marked scope", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_output: bpy.props.BoolProperty(name="Pick Output", description="Show the named output, first linked when off")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    output: bpy.props.StringProperty(name="Output", description="Name of the shown output")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class ThumbnailExport(bpy.types.PropertyGroup):
    """Exported thumbnail size and image load after write."""

    size: bpy.props.IntProperty(name="Size", description="Image side in pixels", subtype="PIXEL", min=256, max=2048, default=512)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    opened: bpy.props.BoolProperty(name="Open Image", description="Load written image", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class GeometrySource(msgspec.Struct, frozen=True, kw_only=True):
    """Object and modifier whose group an editor shows, shader and compositor trees drawing their native node previews."""

    item: Handle[bpy.types.Object]
    modifier: str

    @classmethod
    def of(cls, space: bpy.types.SpaceNodeEditor, root: bpy.types.NodeTree) -> Option[Self]:
        """Returns the object and the active modifier running the geometry group a node editor shows."""
        match space.id_from, root:
            case bpy.types.Object() as item, bpy.types.GeometryNodeTree():
                using = tuple(modifier for modifier in item.modifiers if isinstance(modifier, bpy.types.NodesModifier) and modifier.node_group == root)
                return (
                    Block
                    .of_seq(using)
                    .filter(lambda modifier: modifier.is_active)
                    .try_head()
                    .or_else_with(lambda: Block.of_seq(using).try_head())
                    .map(lambda modifier: cls(item=Handle.of(item), modifier=modifier.name))
                )
            case _:
                return Nothing


class Editor(msgspec.Struct, frozen=True, kw_only=True):
    """Source and group path one node editor shows."""

    source: GeometrySource
    groups: tuple[str, ...]

    @classmethod
    def of(cls, space: bpy.types.SpaceNodeEditor) -> Option[Self]:
        """Reads shown source and group path, `Nothing` outside previewed data-blocks."""
        nested = enclosing(space)
        groups = tuple(node.name for found in nested for node in found.to_list())
        return Option.of_optional(space.node_tree).bind(lambda root: GeometrySource.of(space, root)).filter(lambda _: len(groups) == len(nested)).map(lambda found: cls(source=found, groups=groups))

    def tree(self, root: bpy.types.NodeTree) -> Option[bpy.types.NodeTree]:
        """Returns tree group path reaches from root."""
        return reduce(
            lambda held, name: held.bind(lambda tree: Option.of_optional(tree.nodes.get(name))).bind(lambda node: Option.of_optional(node.node_tree) if isinstance(node, GroupNode) else Nothing),
            self.groups,
            Some(root),
        )


class Target(msgspec.Struct, frozen=True, kw_only=True):
    """Node output a job renders and a cell draws."""

    source: GeometrySource
    groups: tuple[str, ...]
    node: str
    socket: str


class Routed[H: Holder](msgspec.Struct, frozen=True, kw_only=True):
    """Copied holder and routed socket, or kept output node's input."""

    holder: H
    socket: bpy.types.NodeSocket


class TargetRender(msgspec.Struct, frozen=True, kw_only=True):
    """Isolated target render with kind and value read."""

    target: Target
    kind: RenderKind
    valued: bool


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


class Planned(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Editor target of one feed pass."""

    editor: Editor
    region: bpy.types.Region
    tree: bpy.types.NodeTree
    scene: bpy.types.Scene
    node: bpy.types.Node
    render: TargetRender
    size: int
    demand: Demand


class Cell(msgspec.Struct, frozen=True, kw_only=True):
    """Node thumbnail cell."""

    node: str
    key: Hashable
    label: str
    status: Status
    value: Option[float] = Nothing


class Shown(msgspec.Struct, frozen=True, kw_only=True):
    """Display settings and cells each editor draws."""

    display: Display
    cells: Mapping[Editor, tuple[Cell, ...]]


class NodeState:
    """Per-package preview state."""

    __slots__ = ("cells", "held", "keys", "values")

    def __init__(self) -> None:
        """Holds no demand, value, held hash, or cell."""
        self.keys: dict[Hashable, Demand] = {}
        self.values: dict[Hashable, float] = {}
        self.held: dict[Handle[bpy.types.ID], Option[ContentHash]] = {}
        self.cells: dict[Editor, tuple[Cell, ...]] = {}

    def forgotten(self) -> tuple[Hashable, ...]:
        """Drops every target key, returning the keys dropped."""
        dropped = tuple(self.keys)
        self.keys.clear()
        return dropped

    def released(self, block: bpy.types.ID) -> None:
        """Drops the content hash held for a changed block."""
        self.held.pop(Handle.of(block), None)


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

    identity: Identity
    settings: Callable[[Identity], Outcome[NodePreviews]]

    def read(self, _metrics: Metrics, /) -> Outcome[Option[Shown]]:
        """Reads display settings and editor cells while previews show."""
        return self.settings(self.identity).map(lambda settings: Some(Shown(display=settings.display, cells=self.identity.held[NODE_STATES].cells)) if settings.enabled else Nothing)

    def drawn(self, canvas: Canvas, state: Shown) -> None:
        """Draws cells beside nodes, active node grid last."""
        metrics, display, theme, style = canvas.metrics, state.display, canvas.metrics.preferences.themes[0], canvas.metrics.preferences.ui_styles[0].widget
        cache, points, pad, gap, checker = self.identity.held[TEXTURES], metrics.points(), metrics.spacing.button_y, metrics.spacing.template, metrics.checker()
        shader = gpu.shader.from_builtin("IMAGE_COLOR")
        marks = {Status.QUEUED: ("…", color(theme.info.info_warning_text)), Status.FAILED: ("!", color(theme.info.info_error_text))} if display.status else {}

        def cell(item: Cell, rect: Rect, *, several: bool, view: bpy.types.View2D) -> None:
            mark = Option.of_optional(marks.get(item.status))
            inner = Rect(x=rect.x + pad, y=rect.y + pad, width=rect.width - 2 * pad, height=rect.height - 2 * pad)
            centre = (rect.x + rect.width / 2, rect.y + rect.height / 2)
            canvas.frame(rect, fill=color(theme.node_editor.node_backdrop), edge=next((edge for _, edge in mark.to_list()), color(theme.node_editor.wire)), radius=0.0)
            steps = range(math.ceil(inner.width / checker.cell)) if display.checker else range(0)
            for column, row in itertools.product(steps, steps):
                ground = checker.primary if (column + row) % 2 else checker.secondary
                width, height = min(checker.cell, inner.width - column * checker.cell), min(checker.cell, inner.height - row * checker.cell)
                canvas.frame(Rect(x=inner.x + column * checker.cell, y=inner.y + row * checker.cell, width=width, height=height), fill=ground, edge=ground, radius=0.0)
            for texture in cache.shown(item.key).to_list():
                canvas.picture(texture, inner, shader, blend="ALPHA")
            for text, _ in mark.to_list():
                canvas.label(text, at=centre, color=canvas.theme.text_hi, points=points, align=Align.CENTER)
            blf.size(0, points)
            for number in (f"{held:.4g}" for held in item.value.to_list() if display.values and mark.is_none()):
                width, _height = blf.dimensions(0, number)
                if width <= inner.width:
                    canvas.label(number, at=centre, color=canvas.theme.text_hi, points=points, align=Align.CENTER, shadow=Some(Shadow.of(style)))
            if several:
                with Canvas.clipped(rect, view=Some(view)):
                    canvas.label(item.label, at=(inner.x, inner.y), color=canvas.theme.text, points=points)

        def grid(node: bpy.types.Node, cells: tuple[Cell, ...], grown: float, view: bpy.types.View2D) -> None:
            columns, scale = 2 if len(cells) > 1 else 1, metrics.scale
            left, top = node.location_absolute[0] * scale, node.location_absolute[1] * scale
            side = node.width * scale * display.scale * grown / columns
            width, height = side * columns, side * math.ceil(len(cells) / columns)
            match display.position:
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
            (tree, cells, tuple(Option.of_optional(tree.nodes.active).filter(lambda _: display.enlarged).to_list()))
            for space in canvas.space.to_list()
            if isinstance(space, bpy.types.SpaceNodeEditor)
            for tree in Option.of_optional(space.edit_tree).to_list()
            for editor in Editor.of(space).to_list()
            for cells in Option.of_optional(state.cells.get(editor)).to_list()
        )
        for node, drawn, grown, region in (
            (node, drawn, display.enlarge if node in active else 1.0, region)
            for region in Option.of_optional(bpy.context.region).to_list()
            for tree, cells, active in views
            for node in (*tuple(node for node in tree.nodes if node not in active), *active)
            for drawn in (tuple(item for item in cells if item.node == node.name),)
            if drawn and node.width * metrics.scale >= metrics.unit / 2
        ):
            grid(node, drawn, grown, region.view2d)


# --- [SERVICES] -------------------------------------------------------------------------

NODE_STATES: Final = Resource[NodeState](made=lambda _identity: nullcontext(NodeState()), renewed=Some(lambda _held: NodeState()))


class NodePreview(msgspec.Struct, frozen=True, kw_only=True):
    """Row of per-node previews over every node editor, its commands equal across reads."""

    settings: Callable[[Identity], Outcome[NodePreviews]]
    retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing
    preview: PreviewScene = PreviewScene()
    signal: Signal = msgspec.field(default_factory=Signal)
    key: ClassVar[Key] = "node_previews"
    setting: ClassVar[Attachment[NodeSetting]] = Attachment(key="node_preview", form=NodeSetting, owner=bpy.types.Node)
    clay: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="preview_clay", saved=Saved.NEVER)
    swatch: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="preview_swatch", saved=Saved.NEVER)

    # --- [COMMANDS]

    @property
    def toggle(self) -> Operator[Empty]:
        """Command toggling node editor thumbnails."""
        return Operator(form=Empty, undo=False, run=self.toggle_node_previews)

    @property
    def enlarge(self) -> Operator[Empty]:
        """Command toggling the larger active node thumbnail."""
        return Operator(form=Empty, undo=False, run=self.enlarge_node_preview)

    @property
    def refresh(self) -> Operator[Empty]:
        """Command rendering every thumbnail again."""
        return Operator(form=Empty, undo=False, run=self.refresh_node_previews)

    @property
    def clear(self) -> Operator[Empty]:
        """Command removing cached and stored thumbnails."""
        return Operator(form=Empty, undo=False, run=self.clear_node_previews)

    @property
    def mark(self) -> Operator[Empty]:
        """Command toggling selected node marks."""
        return Operator(form=Empty, run=self.mark_node_previews)

    @property
    def export(self) -> Operator[ThumbnailExport]:
        """Command writing the active node thumbnail to an image file."""
        return Operator(form=ThumbnailExport, undo=False, invoke=Some(Save(extensions=extensions)), run=self.export_node_preview)

    def flipped(self, call: Call[Empty], change: Callable[[NodePreviews], Outcome[object]]) -> Outcome[Answer]:
        """Writes the change into the settings and refreshes thumbnails."""
        return self.settings(call.identity).bind(change).bind(lambda _: self.signal.fired()).map(lambda _: Done())

    def toggle_node_previews(self, call: Call[Empty]) -> Outcome[Answer]:
        """Toggles node editor thumbnails."""
        return self.flipped(call, lambda held: written(held, "enabled", not held.enabled))

    def enlarge_node_preview(self, call: Call[Empty]) -> Outcome[Answer]:
        """Toggles the larger active node thumbnail on top."""
        return self.flipped(call, lambda held: written(held, "enlarged", not held.enlarged))

    def refresh_node_previews(self, call: Call[Empty]) -> Outcome[Answer]:
        """Renders every thumbnail again."""
        return self.rerendered(call.identity).map(lambda _: Done())

    def clear_node_previews(self, call: Call[Empty]) -> Outcome[Answer]:
        """Empties texture cache, held values, and previews folder, then renders every thumbnail again."""
        files = tuple((call.identity.folder / NodePreview.key).glob("*.exr"))
        call.identity.held[TEXTURES].held.clear()
        call.identity.held[NODE_STATES].values.clear()
        return accumulated(removed(path) for path in files).bind(lambda _: self.rerendered(call.identity)).map(lambda _: Done(notes=(("INFO", f"Removed {len(files)} stored thumbnails"),)))

    @classmethod
    def mark_node_previews(cls, call: Call[Empty]) -> Outcome[Answer]:
        """Toggles the Marked scope thumbnail of every selected node."""
        if not (nodes := tuple(call.context.selected_nodes)):
            return Ok(Unchanged(reason="No node selected"))
        edits = tuple(cls.setting.read(call.identity, node).bind(lambda held: written(held, "show", not held.show)) for node in nodes)
        return tallied(edits, (("INFO", f"Toggled {sum(edit.is_ok() for edit in edits)} node marks"),))

    def export_node_preview(self, call: Call[ThumbnailExport]) -> Outcome[Answer]:
        """Writes the active node thumbnail to an image file in the scene display space."""
        space = call.context.space_data
        shown: Outcome[tuple[Editor, bpy.types.NodeTree, bpy.types.Node]] = (
            Option
            .of_optional(space if isinstance(space, bpy.types.SpaceNodeEditor) else None)
            .bind(
                lambda found: Editor.of(found).bind(lambda editor: Option.of_optional(found.edit_tree).bind(lambda tree: Option.of_optional(tree.nodes.active).map(lambda node: (editor, tree, node))))
            )
            .to_result_with(partial(Fault.of, BlenderReason.ABSENT, "SpaceNodeEditor.edit_tree.nodes", member="active"))
        )

        def chosen(editor: Editor, tree: bpy.types.NodeTree, node: bpy.types.Node, settings: NodePreviews) -> Outcome[TargetRender]:
            return targets(call.identity, editor, tree, settings).bind(
                lambda found: (
                    Block
                    .of_seq(found)
                    .filter(lambda pair: pair[0] == node)
                    .choose(lambda pair: isolated(pair[0], pair[1], settings))
                    .try_head()
                    .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(tree.nodes), member=node.name))
                )
            )

        def wrote(scene: bpy.types.Scene, render: TargetRender, settings: NodePreviews, config: PyOpenColorIO.Config) -> Outcome[str]:
            formed = viewed(scene)
            return paired(
                self.rendered(call.identity, render, call.properties.size, settings, scene=scene),
                formed.colorspace(config).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(formed), member=formed.display)),
            ).bind(lambda pair: imaged(call.paths[0], pair[0].plane, config, pair[1].getName()))

        return (
            paired(shown, self.settings(call.identity), call.root(bpy.types.Scene))
            .bind(lambda parts: chosen(*parts[0], parts[1]).bind(lambda render: wrote(parts[2], render, parts[1], PyOpenColorIO.GetCurrentConfig())))
            .bind(lambda name: loaded(name, Tag()).map(lambda _: name) if call.properties.opened else Ok(name))
            .map(lambda name: Done.wrote((name,)))
        )

    # --- [RENDERING]

    def rendered(self, identity: Identity, render: TargetRender, size: int, settings: NodePreviews, *, scene: bpy.types.Scene) -> Outcome[Readback]:
        """Renders target through routed copies in preview scene, staged per source kind and formed through the scene view, the source's updates during the render marked as its own."""
        lighting, target = settings.lighting, render.target
        aov: Outcome[Option[str]] = identity.identifier(PreviewScene.value).map(Some) if render.valued else Ok(Nothing)

        def staged(world: Mapping[RnaPath, Value] | bpy.types.World, lamp: Lamp | bpy.types.Light, *, valued: bool) -> Staging:
            return Staging(source=scene, size=(size, size), world=world, lamp=lamp, value=valued and render.valued)

        def clay() -> Outcome[bpy.types.Material]:
            made = NodePreview.clay.made(identity, lambda: catch(bpy.data.materials.new, exception=RuntimeError)("Clay").map_error(partial(Fault.of, BlenderReason.REFUSED, NodePreview.clay.key)))
            return made.bind(lambda material: shaded(material, Clay(), PyOpenColorIO.GetCurrentConfig(), identity))

        def swatched() -> Outcome[bpy.types.Material]:
            def defaulted(socket: bpy.types.NodeSocket) -> Option[Value]:
                found: bpy.types.Property | None = socket.bl_rna.properties.get("default_value")
                return Option.of_optional(found).map(lambda prop: Descriptor(root=type(socket), prop=prop).read(socket))

            def graph(source: bpy.types.Node, name: Option[str]) -> Outcome[Graph]:
                own = tuple(
                    (member.prop.identifier, member.read(source)) for member in Descriptor.saved(source, lambda prop: not prop.is_readonly and prop.identifier not in bpy.types.Node.bl_rna.properties)
                )
                coordinates = Expr(kind=bpy.types.ShaderNodeTexCoord, socket="Generated", shape="VECTOR", label=Some("coordinates"))
                inputs = tuple((item.identifier, coordinates if item.type == "VECTOR" else held) for item in source.inputs if not item.is_linked for held in defaulted(item).to_list())
                field = Expr(kind=type(source), inputs=inputs, members=own, socket=target.socket, label=Some("field"))
                emission = Expr(kind=bpy.types.ShaderNodeEmission, inputs=(("Color", field),), socket="Emission", label=Some("emission"))
                roots = (
                    Expr(kind=bpy.types.ShaderNodeOutputMaterial, inputs=(("PreviewMesh", emission),), members=(("target", "ALL"),), label=Some("output")),
                    *(Expr(kind=bpy.types.ShaderNodeOutputAOV, inputs=(("Value", field),), members=(("aov_name", held),), label=Some("aov")) for held in name.to_list()),
                )
                return Graph.lowered(bpy.types.ShaderNodeTree, *roots)

            found = (
                held(target)
                .map(lambda root: Editor(source=target.source, groups=target.groups).tree(root) if isinstance(root, bpy.types.NodeTree) else Nothing)
                .bind(
                    lambda tree: tree.bind(lambda inner: Option.of_optional(inner.nodes.get(target.node))).to_result_with(
                        lambda: Fault.of(BlenderReason.ABSENT, repr(target.source), member=target.node)
                    )
                )
            )
            material = NodePreview.swatch.made(
                identity, lambda: catch(bpy.data.materials.new, exception=RuntimeError)("Swatch").map_error(partial(Fault.of, BlenderReason.REFUSED, NodePreview.swatch.key))
            )
            return paired(found, aov, material).bind(lambda parts: paired(graph(parts[0], parts[1]), tree_of(parts[2])).bind(lambda pair: reconciled(*pair, identity)).map(lambda _: parts[2]))

        def clayed(item: Handle[bpy.types.Object], name: str, copies: ExitStack) -> Outcome[bpy.types.Object]:
            def clothed(group: bpy.types.NodeTree, carried: Routed[Holder], material: bpy.types.Material) -> Outcome[None]:
                return paired(active_output(group), created(group, bpy.types.GeometryNodeSetMaterial)).bind(
                    lambda pair: accumulated((
                        socket_at(pair[1].inputs, "Geometry").bind(lambda socket: joined(group, carried.socket, socket)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                        socket_at(pair[1].outputs, "Geometry").bind(lambda socket: joined(group, socket, pair[0].inputs[0])),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                        socket_at(pair[1].inputs, "Material").bind(lambda socket: written(socket, "default_value", material)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                    )).map(lambda _: None)
                )

            def copied(source: bpy.types.Object, carried: Routed[Holder], material: bpy.types.Material) -> Outcome[bpy.types.Object]:
                copy = Ownership(kind=bpy.types.Object, key=PreviewScene.routes.key, saved=Saved.NEVER, single=False).made(
                    identity, lambda: Option.of_optional(source.copy()).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(source), member="copy")), scope=Some(copies)
                )
                return paired(tree_of(carried.holder), copy).bind(
                    lambda pair: accumulated((
                        written(pair[1], "parent", None),
                        written(pair[1], "location", (0.0, 0.0, 0.0)),
                        written(pair[1].modifiers[name], "node_group", pair[0]),
                        *(written(later, "show_render", value=False) for later in itertools.dropwhile(lambda modifier: modifier.name != name, pair[1].modifiers) if later.name != name),
                        clothed(pair[0], carried, material),
                    )).map(lambda _: pair[1])
                )

            return paired(item.resolve(), held(target).bind(lambda group: copies.enter_context(routed(identity, group, target))), clay()).bind(lambda parts: copied(*parts))

        with intermediate(identity, scene), ExitStack() as copies:
            staging = (
                paired(clayed(target.source.item, target.source.modifier, copies), msgspec.structs.replace(lighting, strength=0.25).environment).map(
                    lambda pair: msgspec.structs.replace(staged(pair[1], Lamp(energy=4.0, rotation=(CLAY_AIM.x, CLAY_AIM.y, CLAY_AIM.z)), valued=False), viewpoint=Fitted(subject=pair[0]))
                )
                if render.kind is RenderKind.GEOMETRY_CLAY
                else paired(swatched(), lighting.environment).map(
                    lambda pair: msgspec.structs.replace(staged(pair[1], lighting.lamp, valued=True), material=Some(pair[0]), surface=Some(PreviewMesh.PLANE))
                )
            )
            return staging.bind(lambda desired: self.preview.rendered(identity, desired)).bind(lambda readback: readback.formed(viewed(scene), PyOpenColorIO.GetCurrentConfig()))

    def thumbnailed(self, identity: Identity, render: TargetRender, demand: Demand, size: int, kept: frozenset[Hashable], settings: NodePreviews, *, scene: bpy.types.Scene) -> Outcome[None]:
        """Renders a target into texture cache and previews folder, holding uniform value."""
        state, folder, limit = identity.held[NODE_STATES], identity.folder / NodePreview.key, settings.display.limit

        def pruned() -> Outcome[None]:
            files: Outcome[list[Path]] = catch(sorted, exception=OSError)(folder.glob("*.exr"), key=lambda path: path.stat().st_mtime).map_error(
                partial(Fault.of, RecordReason.UNREADABLE, str(folder))
            )
            return Result[list[Path], Faults].bind(files, lambda found: accumulated(removed(path) for path in found[:-limit])).map(lambda _: None)

        def stored(readback: Readback, config: PyOpenColorIO.Config, name: str, space: str) -> Outcome[None]:
            number = readback.values.bind(value)

            def written_file(digest: ContentHash) -> Outcome[None]:
                fields = number.map(lambda held_value: {name: msgspec.json.encode(held_value).decode()}).default_value({})
                return ImageFile(pixels=readback.plane, colorspace=space).write(str(folder / f"{digest.hex()}.exr"), Storage(depth=Depth.HALF), fields, config).bind(lambda _: pruned())

            def saved(_texture: gpu.types.GPUTexture) -> Outcome[None]:
                for held_value in number.to_list():
                    state.values[demand.key] = held_value
                return demand.content.map(written_file).default_value(Ok(None)).bind(lambda _: self.signal.fired())

            return identity.held[TEXTURES].stored(demand.key, readback.plane, limit, kept).bind(saved) if state.keys.get(render.target) is demand else Ok(None)

        formed = viewed(scene)

        def spaced(config: PyOpenColorIO.Config) -> Outcome[PyOpenColorIO.ColorSpace]:
            return formed.colorspace(config).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(formed), member=formed.display))

        config = PyOpenColorIO.GetCurrentConfig()
        return paired(self.rendered(identity, render, size, settings, scene=scene), identity.identifier(PreviewScene.value), spaced(config)).bind(
            lambda parts: stored(parts[0], config, parts[1], parts[2].getName())
        )

    # --- [FEED]

    def jobs(self, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[Job, ...]]:
        """Writes editor cells and returns a job per target key neither cache nor folder holds."""
        state, metrics, failed = identity.held[NODE_STATES], Metrics.read(), identity.held[BACKLOG].failed
        prior, state.keys = state.keys, {}
        manager: bpy.types.WindowManager = context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        views = tuple(
            (editor, space, region, tree)
            for window in manager.windows
            for area in window.screen.areas
            for space in (area.spaces.active,)
            if isinstance(space, bpy.types.SpaceNodeEditor)
            for region in area.regions
            if region.type == "WINDOW"
            for editor in Editor.of(space).to_list()
            for tree in Option.of_optional(space.edit_tree).to_list()
        )

        def keyed(node: bpy.types.Node, render: TargetRender, staged: Staged, config: PyOpenColorIO.Config, enclosing: tuple[bpy.types.Node, ...]) -> Outcome[Option[ContentHash]]:
            def cached(block: bpy.types.ID, *, weighted: bool) -> Outcome[Option[ContentHash]]:
                handle: Handle[bpy.types.ID] = Handle.of(block)
                return Option.of_optional(state.held.get(handle)).map(Ok).default_with(lambda: content.fingerprinted(block, weighted=weighted).map(lambda key: state.held.setdefault(handle, key)))

            def geometry(item: bpy.types.Object, name: str) -> Outcome[Option[tuple[msgspec.Struct | ContentHash, ...]]]:
                earlier = tuple(itertools.takewhile(lambda modifier: modifier.name != name, item.modifiers))
                snapshots = accumulated(Snapshot.capture(modifier) for modifier in (*earlier, *Option.of_optional(item.modifiers.get(name)).to_list()))
                trees = accumulated(
                    content.fingerprinted(group) for modifier in earlier if isinstance(modifier, bpy.types.NodesModifier) for group in Option.of_optional(modifier.node_group).to_list()
                )
                data = accumulated(cached(block, weighted=bool(item.vertex_groups)) for block in Option.of_optional(item.data).to_list())
                return paired(snapshots, trees, data).map(
                    lambda parts: reduce(
                        lambda values, key: values.map2(lambda rows, key: (*rows, key), key),
                        (*parts[1], *parts[2]),
                        Option[tuple[msgspec.Struct | ContentHash, ...]].Some((*parts[0], Snapshot.of(bpy.types.Object, {("vertex_groups",): tuple(group.name for group in item.vertex_groups)}))),
                    )
                )

            source = render.target.source
            return paired(content.fingerprinted(node), accumulated(content.fingerprinted(group) for group in enclosing), source.item.resolve().bind(lambda item: geometry(item, source.modifier))).map(
                lambda parts: reduce(
                    lambda own, key: own.map2(lambda prior_key, current: content_hash(TargetRender, prior_key, current), key),
                    parts[1],
                    parts[0].map2(lambda own, source: content_hash(TargetRender, staged, own, *source, config=Some(config.getCacheID())), parts[2]),
                )
            )

        def restored(key: ContentHash, config: PyOpenColorIO.Config, name: str, limit: int, kept: frozenset[Hashable]) -> Outcome[bool]:
            path = identity.folder / NodePreview.key / f"{key.hex()}.exr"

            def cached(image: ImageFile[np.float32]) -> Outcome[bool]:
                match catch(lambda: tuple(msgspec.json.decode(str(field.value), type=float) for field in image.header if field.name == name), exception=msgspec.DecodeError)():
                    case Result(tag="ok", ok=numbers):

                        def recorded(_: object) -> bool:
                            for number in numbers[-1:]:
                                state.values[key] = number
                            return True

                        return (
                            identity
                            .held[TEXTURES]
                            .stored(key, image.pixels, limit, kept)
                            .bind(lambda _: catch(os.utime, exception=OSError)(path).map_error(partial(Fault.of, RecordReason.UNWRITABLE, str(path))))
                            .map(recorded)
                        )
                    case _:
                        return Ok(False)

            match read_file(str(path), config):
                case Result(tag="error", error=faults) if all(fault.reason in {RecordReason.UNREADABLE, RecordReason.UNWRITABLE} for fault in faults):
                    return Ok(False)
                case Result(tag="error", error=faults):
                    return Error(faults)
                case Result(ok=image):
                    return cached(image)

        def rank(planned: Planned) -> Rank:
            node, region = planned.node, planned.region
            position: tuple[int, int] = region.view2d.view_to_region(node.location_absolute[0] * metrics.scale, node.location_absolute[1] * metrics.scale, clip=False)  # type: ignore[assignment]
            x, y = position
            return Rank.ACTIVE if node.select or node == planned.tree.nodes.active else Rank.SHOWN if 0 <= x <= region.width and 0 <= y <= region.height else Rank.REST

        def planned(scene: bpy.types.Scene, settings: NodePreviews, config: PyOpenColorIO.Config, name: str) -> Outcome[tuple[Job, ...]]:
            def demanded(node: bpy.types.Node, render: TargetRender, size: int, nested: tuple[bpy.types.Node, ...], place: Hashable) -> Outcome[Demand]:
                staged = Staged(
                    kind=render.kind,
                    socket=render.target.socket,
                    engine=scene.render.engine,
                    size=size,
                    lighting=settings.lighting,
                    view=viewed(scene),
                    frame=(scene.frame_current,) if settings.frames else (),
                    host=(bpy.app.version, bpy.app.build_hash),
                    config=config.getCacheID(),
                )

                def remembered(digest: Option[ContentHash]) -> Demand:
                    state.keys[place] = demand = (
                        Option.of_optional(prior.get(place)).filter(lambda held: held.staged == staged and held.content == digest).default_with(lambda: Demand(staged=staged, content=digest))
                    )
                    return demand

                return (
                    Option
                    .of_optional(prior.get(place))
                    .filter(lambda held: held.staged == staged)
                    .map(lambda held: Ok(remembered(held.content)))
                    .default_with(lambda: keyed(node, render, staged, config, nested).map(remembered))
                )

            def viewed_in(editor: Editor, space: bpy.types.SpaceNodeEditor, region: bpy.types.Region, tree: bpy.types.NodeTree) -> Outcome[tuple[Planned, ...]]:
                nested = tuple(node for found in enclosing(space) for node in found.to_list())

                def each(node: bpy.types.Node, render: TargetRender, size: int) -> Outcome[Planned]:
                    return demanded(node, render, size, nested, render.target).map(
                        lambda demand: Planned(editor=editor, region=region, tree=tree, scene=scene, node=node, render=render, size=size, demand=demand)
                    )

                def rows(found: tuple[tuple[bpy.types.Node, Target], ...]) -> Outcome[tuple[Planned, ...]]:
                    listed = tuple(
                        (node, render, round(node.width * metrics.scale * settings.display.scale / (2 if sum(1 for held, _ in found if held == node) > 1 else 1)))
                        for node, target in found
                        for render in isolated(node, target, settings).to_list()
                    )
                    return accumulated(itertools.starmap(each, listed))

                return targets(identity, editor, tree, settings).bind(rows)

            def scheduled(plans: tuple[Planned, ...]) -> Outcome[tuple[Job, ...]]:
                ranked = tuple((plan, rank(plan), plan.demand.key) for plan in plans)
                kept = frozenset(key for _plan, held, key in ranked if held is not Rank.REST)

                def shown(ready: frozenset[Hashable]) -> Outcome[tuple[Job, ...]]:
                    cells = {
                        editor: tuple(
                            Cell(
                                node=plan.node.name,
                                key=key,
                                label=socket_at(plan.node.outputs, plan.render.target.socket).map(lambda socket: socket.name).default_value(plan.node.name),
                                status=Status.READY if key in ready else Status.FAILED if ((Some(self.jobs), plan.render.target), key) in failed.items() else Status.QUEUED,
                                value=Option.of_optional(state.values.get(key)),
                            )
                            for plan, _rank, key in ranked
                            if plan.editor == editor
                        )
                        for editor in dict.fromkeys(plan.editor for plan in plans)
                    }
                    changed, state.cells = cells != state.cells, cells
                    return (self.signal.fired() if changed else Ok(None)).map(
                        lambda _: tuple(
                            Job(
                                place=plan.render.target,
                                key=key,
                                rank=held,
                                run=functools.partial(self.thumbnailed, identity, plan.render, plan.demand, plan.size, kept, settings, scene=plan.scene),
                                playback=settings.frames,
                            )
                            for plan, held, key in ranked
                            if key not in ready
                        )
                    )

                return (
                    accumulated(
                        Ok(Some(key))
                        if key in identity.held[TEXTURES].held
                        else digest.map(lambda found: restored(found, config, name, settings.display.limit, kept).map(lambda held: Some(key) if held else Nothing)).default_value(Ok(Nothing))
                        for key, digest in {key: plan.demand.content for plan, _rank, key in ranked}.items()
                    )
                    .map(lambda parts: frozenset(key for part in parts for key in part.to_list()))
                    .bind(shown)
                )

            return accumulated(itertools.starmap(viewed_in, views if settings.enabled else ())).map(lambda parts: tuple(itertools.chain.from_iterable(parts))).bind(scheduled)

        return paired(rooted(bpy.types.Scene, context), self.settings(identity), identity.identifier(PreviewScene.value)).bind(
            lambda parts: planned(parts[0], parts[1], PyOpenColorIO.GetCurrentConfig(), parts[2])
        )

    # --- [REGISTRATION]

    def section(self, header: tuple[Row, ...] = ()) -> Single:
        """Returns node previews section."""
        return Single(
            key=self.key,
            title=bpy.path.display_name(self.key),
            header=header,
            rows=(
                *(OperatorButton(target=row) for row in (self.toggle, self.refresh, self.clear, self.export)),
                Counts(),
                Single(
                    key="node_previews_active",
                    title="Active Node",
                    store=Some(
                        Hosted(
                            host=Some(Member(root=bpy.types.SpaceNodeEditor, path=("edit_tree", "nodes", "active"))),
                            store=Store(
                                attachment=NodePreview.setting,
                                retention=self.retention,
                                fields={"output": lambda member: SearchField(target=member, collection=Member(root=bpy.types.SpaceNodeEditor, path=("edit_tree", "nodes", "active", "outputs")))},
                            ),
                        )
                    ),
                ),
            ),
        )

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns preview declarations, state, resources, overlay, and handlers, updates of a scene under a preview render left to the render."""

        def forgotten(switch: Callable[[NodePreviews], bool]) -> Outcome[None]:
            read = self.settings(identity)
            for _ in read.to_option().filter(switch).to_list():
                identity.held[NODE_STATES].forgotten()
            return read.map(lambda _: None)

        def dropped(scene: bpy.types.Scene, updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> Outcome[None]:
            if scene.session_uid in identity.held[INTERMEDIATES]:
                return Ok(None)
            for block in (held for update in updates for held in Option.of_optional(update.id).to_list()):
                identity.held[NODE_STATES].released(block)
            return forgotten(lambda settings: settings.automatic)

        overlay = Overlay(
            drawing=Drawing(area=AreaQuery(space="NODE_EDITOR"), part=NodeThumbnails(identity=identity, settings=self.settings), refreshes=(self.signal,)),
            space=bpy.types.SpaceNodeEditor,
            draw_type="POST_VIEW",
        )
        return Ok((
            self.preview,
            self.setting,
            NODE_STATES,
            BACKLOG,
            Keymap(keymap="Node Editor", bindings={Chord(type="P", ctrl=1, alt=1): self.toggle, Chord(type="Z", ctrl=1, alt=1): self.enlarge, Chord(type="R", ctrl=1, alt=1): self.refresh}),
            self.clay,
            self.swatch,
            TEXTURES,
            (Band.RESOURCES, PreviewQueue.fed(identity, self.jobs)),
            self.toggle,
            self.enlarge,
            self.refresh,
            self.clear,
            self.mark,
            self.export,
            Append(target=NODE_MT_context_menu, rows=(OperatorButton(target=self.mark), OperatorButton(target=self.export), OperatorButton(target=self.refresh))),
            overlay,
            Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, STORED, dropped)),
            Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.Object,), dropped, changes=Some(Change.GEOMETRY | Change.SHADING))),
            Handler(handlers=bpy.app.handlers.frame_change_post, body=lambda: forgotten(lambda settings: settings.frames)),
        ))

    def rerendered(self, identity: Identity) -> Outcome[None]:
        """Drops target keys and held failures for a full rerender."""
        PreviewQueue.forced(identity, identity.held[NODE_STATES].forgotten())
        return self.signal.fired()


# --- [OPERATIONS] -----------------------------------------------------------------------


def targets(identity: Identity, editor: Editor, tree: bpy.types.NodeTree, settings: NodePreviews) -> Outcome[tuple[tuple[bpy.types.Node, Target], ...]]:
    """Returns in-scope tree nodes with targets their outputs and setting choose."""
    zones = zoned(tree)

    def chosen(node: bpy.types.Node, setting: NodeSetting) -> tuple[str, ...]:
        eligible = outputs_of(node)
        linked = tuple(socket.identifier for socket in eligible if socket.is_linked)
        picked = tuple(socket.identifier for socket in eligible if setting.use_output and socket.name == setting.output)
        return linked if settings.every else (picked or linked or tuple(socket.identifier for socket in eligible))[:1]

    def shown(node: bpy.types.Node, setting: NodeSetting) -> bool:
        match PreviewScope[settings.scope]:
            case PreviewScope.ALL:
                return True
            case PreviewScope.SELECTED:
                return node.select
            case PreviewScope.MARKED:
                return bool(setting.show)

    candidates = tuple(
        node for node in tree.nodes if not (isinstance(node, bpy.types.NodeFrame | bpy.types.NodeReroute | bpy.types.NodeGroupInput | bpy.types.NodeGroupOutput) or node.mute or node.name in zones)
    )
    return (
        NodePreview.setting
        .bound(identity)
        .map(lambda attached: tuple((node, attached.struct(node)) for node in candidates))
        .map(
            lambda pairs: tuple(
                (node, Target(source=editor.source, groups=editor.groups, node=node.name, socket=socket)) for node, setting in pairs if shown(node, setting) for socket in chosen(node, setting)
            )
        )
    )


def isolated(node: bpy.types.Node, target: Target, settings: NodePreviews) -> Option[TargetRender]:
    """Returns isolated render of a target socket."""
    inputs, outputs = node.inputs, node.outputs

    def kind(socket: bpy.types.NodeSocket) -> Option[RenderKind]:
        return Some(RenderKind.GEOMETRY_CLAY) if socket.type == "GEOMETRY" else Some(RenderKind.FIELD_SWATCH) if settings.fields and isinstance(node, bpy.types.ShaderNode) else Nothing

    return (
        socket_at(inputs if isinstance(node, OutputNode) else outputs, target.socket)
        .to_option()
        .bind(lambda socket: kind(socket).map(lambda found: (socket, found)))
        .map(lambda pair: TargetRender(target=target, kind=pair[1], valued=pair[0].type in {"VALUE", "INT", "BOOLEAN"}))
    )


def enclosing(space: bpy.types.SpaceNodeEditor) -> tuple[Option[bpy.types.Node], ...]:
    """Returns group node entering each tree down editor path, parent active node first."""
    trees = tuple(item.node_tree for item in space.path)
    return tuple(
        Block.of_seq((parent.nodes.active, *parent.nodes)).choose(Option.of_optional).filter(lambda node: isinstance(node, GroupNode) and node.node_tree == child).try_head()
        if parent is not None
        else Nothing
        for parent, child in itertools.pairwise(trees)
    )


def held(target: Target) -> Outcome[Holder]:
    """Resolves the group of the target source's modifier."""
    name = target.source.modifier
    return target.source.item.resolve().bind(
        lambda item: (
            Option
            .of_optional(item.modifiers.get(name))
            .bind(lambda found: Option.of_optional(found.node_group) if isinstance(found, bpy.types.NodesModifier) else Nothing)
            .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(item.modifiers), member=name))
        )
    )


@contextmanager
def routed[H: Holder](identity: Identity, holder: H, target: Target) -> Generator[Outcome[Routed[H]]]:
    """Scopes preview copies, exposing target output.

    Args:
        identity: Extension whose marks the copies hold.
        holder: Tree holder nesting target node.
        target: Route of copies.

    Yields:
        Copied holder and output, copies removed at scope exit.
    """

    def found(tree: bpy.types.NodeTree, name: str) -> Outcome[bpy.types.Node]:
        return Option.of_optional(tree.nodes.get(name)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(tree.nodes), member=name))

    with ExitStack() as copies:

        def copied[T: Holder](block: T) -> Outcome[T]:
            duplicate: Callable[[], T] = block.copy  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return Ownership(kind=type(block), key=PreviewScene.routes.key, saved=Saved.NEVER, single=False).made(
                identity, lambda: catch(duplicate, exception=RuntimeError)().map_error(partial(Fault.of, BlenderReason.REFUSED, repr(block))), scope=Some(copies)
            )

        def exposed(inner: bpy.types.NodeTree, source: bpy.types.NodeSocket) -> Outcome[str]:
            return (
                active_output(inner)
                .bind(lambda output: socket_at(output.inputs, "__extend__"))
                .bind(lambda extend: joined(inner, source, extend, handle_dynamic_sockets=True))
                .map(lambda link: link.to_socket.identifier)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            )

        def entered(node: bpy.types.Node) -> Outcome[bpy.types.NodeTree]:
            group = (Option.of_optional(node.node_tree) if isinstance(node, GroupNode) else Nothing).to_result_with(
                lambda: Fault.of(BlenderReason.ABSENT, repr(node), member=bpy.types.NodeTree.__name__)
            )
            return Result[bpy.types.NodeTree, Faults].bind(group, copied).bind(lambda inner: written(node, "node_tree", inner).map(lambda _: inner))

        def outward(tree: bpy.types.NodeTree, groups: tuple[str, ...]) -> Outcome[bpy.types.NodeSocket]:
            match groups:
                case (head, *rest):
                    return found(tree, head).bind(
                        lambda node: (
                            entered(node).bind(lambda inner: outward(inner, tuple(rest)).bind(lambda source: exposed(inner, source))).bind(lambda identifier: socket_at(node.outputs, identifier))
                        )
                    )
                case _:
                    return found(tree, target.node).bind(lambda node: socket_at(node.outputs, target.socket))

        yield copied(holder).bind(lambda copy: tree_of(copy).bind(lambda tree: outward(tree, target.groups)).map(lambda socket: Routed(holder=copy, socket=socket)))


def outputs_of(node: bpy.types.Node) -> tuple[bpy.types.NodeSocket, ...]:
    """Returns shown outputs of a geometry tree node with a socket type the tree previews."""
    shown = tuple(socket for socket in node.outputs if socket.enabled and not socket.hide and not socket.is_unavailable)
    return (
        tuple(socket for socket in shown if socket.type == "GEOMETRY" or (isinstance(node, bpy.types.ShaderNode) and socket.type in {"RGBA", "VECTOR", "VALUE"}))
        if isinstance(node.id_data, bpy.types.GeometryNodeTree)
        else ()
    )


def zoned(tree: bpy.types.NodeTree) -> frozenset[str]:
    """Returns names of nodes inside paired zones."""
    pairs = sorted((link.from_node.name, link.to_node.name) for link in tree.links)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    following = {name: frozenset(target for _, target in group) for name, group in itertools.groupby(pairs, key=lambda pair: pair[0])}

    def inside(start: str, end: str) -> frozenset[str]:
        def walked(frontier: frozenset[str], seen: frozenset[str]) -> frozenset[str]:
            fresh = frozenset(target for name in frontier - {end} for target in following.get(name, frozenset())) - seen
            return walked(fresh, seen | fresh) if fresh else seen

        return walked(frozenset({start}), frozenset({start}))

    return frozenset(
        name
        for node in tree.nodes
        if "paired_output" in node.bl_rna.properties
        for end in (node.paired_output,)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        if isinstance(end, bpy.types.Node)
        for name in inside(node.name, end.name)
    )


def imaged(path: Path, plane: npt.NDArray[np.float32], config: PyOpenColorIO.Config, space: str) -> Outcome[str]:
    """Writes a display-space thumbnail at the deepest depth the suffix's writer stores, integer depths coded from unassociated values."""

    def write(depth: Depth) -> Outcome[str]:
        match depth:
            case Depth.HALF | Depth.FLOAT as floated:
                return ImageFile(pixels=plane, colorspace=space).write(str(path), Storage(depth=floated), {}, config)
            case Depth.UINT8 | Depth.UINT16:
                return ImageFile(pixels=depth.coded(unassociated(plane), Rounded()), colorspace=space).write(str(path), Storage(), {}, config)

    suffix = path.suffix.lower()
    chosen = (
        Block
        .of_seq(WRITABLE)
        .filter(lambda row: suffix.removeprefix(".") in row.extensions)
        .try_head()
        .to_result_with(lambda: Fault.of(RecordReason.OUTSIDE, "filepath", suffix, member=", ".join(SUFFIXES)))
    )
    return Result[Format, Faults].bind(chosen, lambda row: write(row.depths[-1]))


def extensions(_record: ThumbnailExport) -> tuple[str, *tuple[str, ...]]:
    """Save suffixes of the export, OpenEXR first."""
    return (".exr", *(suffix for suffix in SUFFIXES if suffix != ".exr"))


def value(values: npt.NDArray[np.float32]) -> Option[float]:
    """Returns mean of a field within a thousandth of its magnitude, `Nothing` when varying or empty."""
    return Option.of_optional(values if values.size else None).filter(lambda held: float(np.ptp(held)) <= 1e-3 * max(1.0, float(np.abs(held).max()))).map(lambda held: float(held.mean()))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "NODE_STATES",
    "Cell",
    "Counts",
    "Display",
    "Editor",
    "GeometrySource",
    "NodePreview",
    "NodePreviews",
    "NodeSetting",
    "NodeState",
    "NodeThumbnails",
    "Planned",
    "Position",
    "PreviewScope",
    "RenderKind",
    "Shown",
    "Staged",
    "Status",
    "Target",
    "TargetRender",
    "ThumbnailExport",
    "extensions",
    "imaged",
    "isolated",
    "outputs_of",
    "targets",
    "value",
    "zoned",
]
