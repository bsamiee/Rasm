"""Per-node preview targets, isolated renders, change keys, and node editor thumbnails."""

from collections.abc import Callable, Generator, Hashable
from contextlib import contextmanager, ExitStack, nullcontext
from enum import auto, Enum
import functools
from functools import reduce
import itertools
import math
import operator
import os
from pathlib import Path
from typing import Annotated, ClassVar, Final, Self

from bl_ui.space_node import NODE_MT_context_menu
import blf
import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import gpu
from mathutils import Vector
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.keymaps import Chord, Keymap
from blender.bpy.commands.operators import Call, Done, Empty, Operator, Outcome, Save, tallied, Unchanged
from blender.bpy.data.ids import content
from blender.bpy.data.ids.images import loaded, Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.ids.store import Handle, resolved, STORES, vacant
from blender.bpy.data.rna.descriptors import Descriptor, rooted, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import Change, SceneHandler, UpdateHandler
from blender.bpy.extension.identity import Identity, Name
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.changes import reconciled
from blender.bpy.nodes.graphs import Expr, Graph
from blender.bpy.nodes.trees import active_output, created, fingerprinted, GroupNode, Holder, joined, OutputNode, socket_at, tree_of
from blender.bpy.render.color.spaces import config_of, viewed
from blender.bpy.render.compositor.effects import Context, Enabled
from blender.bpy.render.compositor.inputs import inputs_of, KeyPath, updated
from blender.bpy.render.outputs.results import intermediate, INTERMEDIATES
from blender.bpy.render.previews.queue import BACKLOG, Job, PreviewQueue, Rank
from blender.bpy.render.previews.scenes import Fitted, Lamp, Lighting, Mirrored, PreviewEngine, PreviewMesh, PreviewScene, PreviewStack, Readback, Staging
from blender.bpy.scene.materials import Clay, shaded
from blender.bpy.scene.worlds import Environment
from blender.bpy.settings.attachments import Attachment
from blender.bpy.settings.history import Retention
from blender.bpy.settings.presets import removed
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.drawing.canvas import Align, Canvas, Rect, Shadow
from blender.bpy.ui.drawing.handlers import Drawing, Overlay, Signal
from blender.bpy.ui.layout.rows import Line, Member, OperatorButton, Row, RowDraw
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.values import Hosted, Store
from blender.bpy.ui.resources.textures import TEXTURES
from blender.bpy.ui.resources.theme import color, Metrics
from blender.bpy.ui.screens.areas import AreaQuery
from blender.bpy.ui.surfaces.appends import Append
from blender.imaging.color.processors import DisplayView
from blender.imaging.files.images import Depth, Format, ImageFile, read_file, Rounded, Storage, unassociated
from blender.records.faults import accumulated, DomainFault, FileFault, paired
from blender.records.fields import Choice, Gated
from blender.records.identity import content_hash, ContentHash

# --- [TYPES] ----------------------------------------------------------------------------


class TreeKind(Choice):
    """Trees that take thumbnails, shader trees drawing their native node previews."""

    GEOMETRY = 4, "Geometry Nodes"
    COMPOSITOR = 5, "Compositor"


class PreviewScope(Choice):
    """Nodes that take a thumbnail."""

    ALL = 1, "All"
    SELECTED = 2, "Selected"
    MARKED = 3, "Marked"


class Position(Choice):
    """Side of a node its thumbnails sit on."""

    ABOVE = 1, "Above"
    BELOW = 2, "Below"
    LEFT = 3, "Left"
    RIGHT = 4, "Right"


class RenderKind(Choice):
    """Isolated render a node output takes."""

    FIELD_SWATCH = 1, "Field Swatch"
    GEOMETRY_CLAY = 2, "Geometry Clay"
    EFFECT_SWATCH = 3, "Effect Swatch"


class Status(Enum):
    """Cell thumbnail state."""

    READY = auto()
    QUEUED = auto()
    FAILED = auto()


type TreeSource = GeometrySource | CompositorSource

# --- [CONSTANTS] ------------------------------------------------------------------------

WRITABLE: Final = Format.writable()
SUFFIXES: Final = tuple(f".{extension}" for row in WRITABLE for extension in row.extensions)
STORED: Final = tuple(kind for kind in STORES if issubclass(kind, bpy.types.ID) and kind not in {bpy.types.ID, bpy.types.Object})
CLAY_AIM: Final = Vector((0.3, -0.6, 0.9)).normalized().to_track_quat("Z", "Y").to_euler()

# --- [CHOICES] --------------------------------------------------------------------------

outputs = Choices(
    lambda identity, owner, _: identity.reports.reported(
        lambda: NodePreview.setting.bound(identity).map(
            lambda attached: tuple(
                (socket.identifier, socket.name, "")
                for group in (owner,)
                if isinstance(group, bpy.types.PropertyGroup)
                for node in (attached.host(group),)
                if isinstance(node, bpy.types.Node)
                for socket in outputs_of(node)
            )
        )
    )().default_value(())
)

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


class NodePreviews(msgspec.Struct, frozen=True, kw_only=True):
    """Node preview settings of extension preferences."""

    enabled: Annotated[bool, msgspec.Meta(title="Show Previews", description="Thumbnails over node editor nodes")] = True
    automatic: Annotated[bool, msgspec.Meta(title="Auto Update", description="Changed nodes render again")] = True
    scope: Annotated[PreviewScope, msgspec.Meta(description="Nodes that take a thumbnail")] = PreviewScope.ALL
    every: Annotated[bool, msgspec.Meta(title="All Linked Outputs", description="Every linked output side by side")] = False
    frames: Annotated[bool, msgspec.Meta(title="Follow Frame", description="Render again on frame change and during playback")] = False
    fields: Annotated[bool, msgspec.Meta(title="Field Nodes", description="Shader nodes of geometry trees as flat swatches")] = True
    trees: Annotated[frozenset[TreeKind], msgspec.Meta(description="Trees that take thumbnails")] = frozenset(TreeKind)
    lighting: Annotated[Lighting, msgspec.Meta(description="Shader preview lighting")] = Lighting()
    display: Annotated[Display, msgspec.Meta(description="Thumbnail drawing")] = Display()


class NodeSetting(msgspec.Struct, frozen=True, kw_only=True):
    """Node thumbnail settings."""

    show: Annotated[bool, msgspec.Meta(title="Show Preview", description="Thumbnail under Marked scope")] = True
    output: Annotated[Gated[Annotated[str, outputs]], msgspec.Meta(description="Shown output, first linked when off")] = Gated(enabled=False, value="")


class ThumbnailExport(msgspec.Struct, frozen=True, kw_only=True):
    """Exported thumbnail size and image load after write."""

    size: Annotated[int, msgspec.Meta(ge=256, le=2048, description="Image side in pixels")] = 512
    opened: Annotated[bool, msgspec.Meta(title="Open Image", description="Load written image")] = True


class GeometrySource(msgspec.Struct, frozen=True, kw_only=True):
    """Object and modifier whose group an editor shows."""

    kind: ClassVar[TreeKind] = TreeKind.GEOMETRY
    item: Handle[bpy.types.Object]
    modifier: str


class CompositorSource(msgspec.Struct, frozen=True, kw_only=True):
    """Scene and effect entry whose group an editor shows."""

    kind: ClassVar[TreeKind] = TreeKind.COMPOSITOR
    scene: Handle[bpy.types.Scene]
    entry: str


class Editor(msgspec.Struct, frozen=True, kw_only=True):
    """Source and group path one node editor shows."""

    source: TreeSource
    groups: tuple[str, ...]

    @classmethod
    def of(cls, space: bpy.types.SpaceNodeEditor) -> Option[Self]:
        """Reads shown source and group path, `Nothing` outside previewed data-blocks."""
        nested = enclosing(space)
        groups = tuple(node.name for found in nested for node in found.to_list())
        return Option.of_optional(space.node_tree).bind(lambda root: sourced(space, root)).filter(lambda _: len(groups) == len(nested)).map(lambda found: cls(source=found, groups=groups))

    def tree(self, root: bpy.types.NodeTree) -> Option[bpy.types.NodeTree]:
        """Returns tree group path reaches from root."""
        return reduce(
            lambda held, name: held.bind(lambda tree: Option.of_optional(tree.nodes.get(name))).bind(lambda node: Option.of_optional(node.node_tree) if isinstance(node, GroupNode) else Nothing),
            self.groups,
            Some(root),
        )


class Target(msgspec.Struct, frozen=True, kw_only=True):
    """Node output a job renders and a cell draws."""

    source: TreeSource
    groups: tuple[str, ...]
    node: str
    socket: str


class Routed[H: Holder](msgspec.Struct, frozen=True, kw_only=True):
    """Copied holder and routed socket, or kept output node's input."""

    holder: H
    socket: bpy.types.NodeSocket


class Entering(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Frame compositor thumbnails read in place of a preview surface: its content key, and its pixels fitted to a side with their camera framing."""

    key: ContentHash
    fitted: Callable[[int], BlenderResult[tuple[npt.NDArray[np.float32], Context]]]


class TargetRender(msgspec.Struct, frozen=True, kw_only=True):
    """Isolated target render with kind and value read."""

    target: Target
    kind: RenderKind
    valued: bool


class Staged(msgspec.Struct, frozen=True, kw_only=True):
    """Settings a change key hashes."""

    kind: RenderKind
    socket: str
    engine: PreviewEngine
    size: int
    lighting: Lighting
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
    cells: dict[Editor, tuple[Cell, ...]]


class NodeState(msgspec.Struct, kw_only=True):
    """Per-package preview state."""

    keys: dict[Hashable, Demand] = msgspec.field(default_factory=dict)
    values: dict[Hashable, float] = msgspec.field(default_factory=dict)
    held: dict[Handle[bpy.types.ID], ContentHash] = msgspec.field(default_factory=dict)
    cells: dict[Editor, tuple[Cell, ...]] = msgspec.field(default_factory=dict)


class Counts(Line, frozen=True):
    """Queued, failed, and cached thumbnail counts with cache memory."""

    @staticmethod
    def drawn(identity: Identity, /) -> BlenderResult[RowDraw]:
        """Returns label reading backlog and cache per draw."""

        def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", _context: bpy.types.Context) -> None:
            backlog, cache = BACKLOG.of(identity), TEXTURES.of(identity)
            layout.label(text=f"{len(backlog.jobs)} queued, {len(backlog.failed)} failed, {len(cache.held)} cached, {cache.memory / 2**20:.1f} MiB pixels")

        return Ok(shown)


class NodeThumbnails(msgspec.Struct, frozen=True, kw_only=True):
    """Drawn part placing cells beside nodes."""

    identity: Identity
    settings: Callable[[Identity], BlenderResult[NodePreviews]]

    def read(self, _metrics: Metrics, /) -> BlenderResult[Option[Shown]]:
        """Reads display settings and editor cells while previews show."""
        return self.settings(self.identity).map(lambda settings: Some(Shown(display=settings.display, cells=NODE_STATES.of(self.identity).cells)) if settings.enabled else Nothing)

    def drawn(self, canvas: Canvas, state: Shown) -> None:
        """Draws cells beside nodes, active node grid last."""
        metrics, display, theme, style = canvas.metrics, state.display, canvas.metrics.preferences.themes[0], canvas.metrics.preferences.ui_styles[0].widget
        cache, points, pad, gap, checker = TEXTURES.of(self.identity), metrics.points(), metrics.spacing.button_y, metrics.spacing.template, metrics.checker()
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

NODE_STATES: Final = Scoped[NodeState](made=lambda _identity: nullcontext(NodeState()), renewed=Some(lambda _held: NodeState()))


class NodePreview(msgspec.Struct, frozen=True, kw_only=True):
    """Row of per-node previews over every node editor, its commands equal across reads."""

    settings: Callable[[Identity], BlenderResult[NodePreviews]]
    stored: Callable[[Identity, NodePreviews], BlenderResult[None]]
    retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing
    preview: PreviewScene = PreviewScene()
    entering: Callable[[Identity, bpy.types.Scene], Option[Entering]] = lambda _identity, _scene: Nothing
    signal: Signal = msgspec.field(default_factory=Signal)
    setting: ClassVar[Attachment[NodeSetting]] = Attachment(key="node_preview", form=NodeSetting, owner=bpy.types.Node)
    clay: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="preview_clay", saved=Saved.NEVER)
    swatch: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="preview_swatch", saved=Saved.NEVER)

    # --- [COMMANDS]

    @property
    def toggle(self) -> Operator[Empty]:
        """Command toggling node editor thumbnails."""
        return Operator(key="toggle_node_previews", title="Toggle Node Previews", description="Toggle node editor thumbnails", form=Empty, undo=False, run=self.toggled)

    @property
    def enlarge(self) -> Operator[Empty]:
        """Command toggling the larger active node thumbnail."""
        return Operator(key="enlarge_node_preview", title="Enlarge Active Node Preview", description="Toggle larger active node thumbnails on top", form=Empty, undo=False, run=self.enlarged)

    @property
    def refresh(self) -> Operator[Empty]:
        """Command rendering every thumbnail again."""
        return Operator(key="refresh_node_previews", title="Refresh Node Previews", description="Render every node thumbnail again", form=Empty, undo=False, run=self.refreshed)

    @property
    def clear(self) -> Operator[Empty]:
        """Command removing cached and stored thumbnails."""
        return Operator(key="clear_node_previews", title="Clear Node Previews", description="Remove cached and stored thumbnails and render again", form=Empty, undo=False, run=self.cleared)

    @property
    def mark(self) -> Operator[Empty]:
        """Command toggling selected node marks."""
        return Operator(key="mark_node_previews", title="Mark Node Previews", description="Toggle selected node thumbnails under Marked scope", form=Empty, run=self.marked)

    @property
    def export(self) -> Operator[ThumbnailExport]:
        """Command writing the active node thumbnail to an image file."""
        return Operator(
            key="export_node_preview",
            title="Export Node Preview",
            description="Write active node thumbnail to an image file",
            form=ThumbnailExport,
            undo=False,
            invoke=Some(Save(extensions=extensions)),
            run=self.exported,
        )

    def flipped(self, call: Call[Empty], path: KeyPath) -> BlenderResult[Outcome]:
        """Negates one boolean setting at a field path and refreshes thumbnails."""
        return self.settings(call.identity).bind(lambda held: self.stored(call.identity, updated(held, path, operator.not_))).bind(lambda _: self.signal.fired()).map(lambda _: Done())

    def toggled(self, call: Call[Empty]) -> BlenderResult[Outcome]:
        """Flips thumbnail drawing."""
        return self.flipped(call, ("enabled",))

    def enlarged(self, call: Call[Empty]) -> BlenderResult[Outcome]:
        """Flips the larger active node thumbnail."""
        return self.flipped(call, ("display", "enlarged"))

    def refreshed(self, call: Call[Empty]) -> BlenderResult[Outcome]:
        """Renders every thumbnail again."""
        return self.rerendered(call.identity).map(lambda _: Done())

    def cleared(self, call: Call[Empty]) -> BlenderResult[Outcome]:
        """Empties texture cache, held values, and previews folder, then renders every thumbnail again."""
        files = tuple((call.identity.folder / "previews").glob("*.exr"))
        TEXTURES.of(call.identity).held.clear()
        NODE_STATES.of(call.identity).values.clear()
        return accumulated(removed(path) for path in files).bind(lambda _: self.rerendered(call.identity)).map(lambda _: Done(notes=(("INFO", f"Removed {len(files)} stored thumbnails"),)))

    def marked(self, call: Call[Empty]) -> BlenderResult[Outcome]:
        """Flips the Marked scope mark of every selected node."""
        if not (nodes := tuple(call.context.selected_nodes)):
            return Ok(Unchanged(reason="No node selected"))
        edits = tuple(NodePreview.setting.revised(call.identity, node, lambda held: Ok(msgspec.structs.replace(held, show=not held.show))) for node in nodes)
        return tallied(edits, (("INFO", f"Toggled {sum(edit.is_ok() for edit in edits)} node marks"),))

    def exported(self, call: Call[ThumbnailExport]) -> BlenderResult[Outcome]:
        """Renders the active node's isolated target at the requested size and writes it in the scene's display space."""
        space = call.context.space_data
        shown: BlenderResult[tuple[Editor, bpy.types.NodeTree, bpy.types.Node]] = (
            Option
            .of_optional(space if isinstance(space, bpy.types.SpaceNodeEditor) else None)
            .bind(
                lambda found: Editor.of(found).bind(lambda editor: Option.of_optional(found.edit_tree).bind(lambda tree: Option.of_optional(tree.nodes.active).map(lambda node: (editor, tree, node))))
            )
            .to_result_with(lambda: (BlenderAbsentFault(owner="SpaceNodeEditor.edit_tree.nodes", key="active"),))
        )

        def chosen(editor: Editor, tree: bpy.types.NodeTree, node: bpy.types.Node, settings: NodePreviews) -> BlenderResult[TargetRender]:
            return targets(call.identity, editor, tree, settings).bind(
                lambda found: Option.of_optional(next((render for held, target in found if held == node for render in isolated(held, target, settings).to_list()), None)).to_result_with(
                    lambda: (BlenderAbsentFault(owner=repr(tree.nodes), key=node.name),)
                )
            )

        def wrote(scene: bpy.types.Scene, render: TargetRender, settings: NodePreviews, config: PyOpenColorIO.Config) -> BlenderResult[str]:
            formed = viewed(scene)
            return paired(
                self.rendered(call.identity, render, call.record.size, settings, scene=scene),
                formed.colorspace(config).to_result_with(lambda: (BlenderAbsentFault(owner=repr(formed), key=formed.display),)),
            ).bind(lambda pair: imaged(call.paths[0], pair[0].plane, config, pair[1].getName()))

        return (
            paired(shown, self.settings(call.identity), config_of(), call.root(bpy.types.Scene))
            .bind(lambda parts: chosen(*parts[0], parts[1]).bind(lambda render: wrote(parts[3], render, parts[1], parts[2])))
            .bind(lambda name: loaded(name, Tag()).map(lambda _: name) if call.record.opened else Ok(name))
            .map(lambda name: Done.wrote((name,)))
        )

    # --- [RENDERING]

    def rendered(self, identity: Identity, render: TargetRender, size: int, settings: NodePreviews, *, scene: bpy.types.Scene) -> BlenderResult[Readback]:
        """Renders target through routed copies in preview scene, staged per source kind and formed through the scene view, the source's updates during the render marked as its own."""
        lighting, target = settings.lighting, render.target
        aov: BlenderResult[Option[str]] = identity.name(Name.ATTACHMENT, PreviewScene.value).map(Some) if render.valued else Ok(Nothing)

        def staged(world: Environment | bpy.types.World, lamp: Lamp | bpy.types.Light, *, valued: bool) -> Staging:
            return Staging(source=scene, size=(size, size), engine=PreviewEngine.of(scene), world=world, lamp=lamp, value=valued and render.valued)

        def clay() -> BlenderResult[bpy.types.Material]:
            made = NodePreview.clay.made(identity, lambda: BlenderEditFault.attempted(NodePreview.clay.key, lambda: bpy.data.materials.new(vacant("Clay", bpy.data.materials.keys()))))
            return paired(made, config_of()).bind(lambda pair: shaded(pair[0], Clay(), pair[1], identity))

        def swatched() -> BlenderResult[bpy.types.Material]:
            def defaulted(socket: bpy.types.NodeSocket) -> Option[Value]:
                found: bpy.types.Property | None = socket.bl_rna.properties.get("default_value")
                return Option.of_optional(found).map(lambda prop: Descriptor(root=type(socket), owner="", prop=prop).read(socket))

            def graph(source: bpy.types.Node, name: Option[str]) -> BlenderResult[Graph]:
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
                .bind(lambda tree: tree.bind(lambda inner: Option.of_optional(inner.nodes.get(target.node))).to_result_with(lambda: (BlenderAbsentFault(owner=repr(target.source), key=target.node),)))
            )
            material = NodePreview.swatch.made(identity, lambda: BlenderEditFault.attempted(NodePreview.swatch.key, lambda: bpy.data.materials.new(vacant("Swatch", bpy.data.materials.keys()))))
            return paired(found, aov, material).bind(lambda parts: paired(graph(parts[0], parts[1]), tree_of(parts[2])).bind(lambda pair: reconciled(*pair, identity)).map(lambda _: parts[2]))

        def clayed(item: Handle[bpy.types.Object], name: str, copies: ExitStack) -> BlenderResult[bpy.types.Object]:
            def clothed(group: bpy.types.NodeTree, carried: Routed[Holder], material: bpy.types.Material) -> BlenderResult[None]:
                return paired(active_output(group), created(group, bpy.types.GeometryNodeSetMaterial)).bind(
                    lambda pair: accumulated((
                        socket_at(pair[1].inputs, "Geometry").bind(lambda socket: joined(group, carried.socket, socket)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                        socket_at(pair[1].outputs, "Geometry").bind(lambda socket: joined(group, socket, pair[0].inputs[0])),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                        socket_at(pair[1].inputs, "Material").bind(lambda socket: written(socket, "default_value", material)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                    )).map(lambda _: None)
                )

            def copied(source: bpy.types.Object, carried: Routed[Holder], material: bpy.types.Material) -> BlenderResult[bpy.types.Object]:
                copy = Ownership(kind=bpy.types.Object, key=PreviewScene.routes.key, saved=Saved.NEVER, single=False).made(
                    identity, lambda: Option.of_optional(source.copy()).to_result_with(lambda: (BlenderAbsentFault(owner=repr(source), key="copy"),)), scope=Some(copies)
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

        def chained(name: str, copies: ExitStack) -> BlenderResult[Staging]:
            frame = self.entering(identity, scene).map(lambda held: held.fitted(size).map(Some)).default_value(Ok(Nothing))

            def mirrored(entry: bpy.types.SceneCompositorEffect, carried: Routed[bpy.types.NodeTree], head: Option[npt.NDArray[np.float32]]) -> BlenderResult[PreviewStack]:
                ahead = tuple(itertools.takewhile(lambda held: held != entry, scene.compositor_effects))
                return (
                    active_output(carried.holder)
                    .bind(lambda sink: joined(carried.holder, carried.socket, sink.inputs[0]))
                    .map(
                        lambda _: PreviewStack(head=head, mirrored=(*(mirror for mirror in Mirrored.of(scene, Enabled.PREVIEW) if mirror.entry in ahead), Mirrored(entry=entry, group=carried.holder)))
                    )
                )

            def grouped(entry: bpy.types.SceneCompositorEffect, entered: Option[tuple[npt.NDArray[np.float32], Context]]) -> BlenderResult[Staging]:
                group = Option.of_optional(entry.node_group).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key="node_group"),))
                head = entered.map(lambda held: held[0])
                return paired(
                    Result[bpy.types.NodeTree, BlenderFaults].bind(group, lambda tree: copies.enter_context(routed(identity, tree, target))).bind(lambda carried: mirrored(entry, carried, head)),
                    lighting.environment,
                ).map(
                    lambda pair: msgspec.structs.replace(
                        staged(pair[1], lighting.lamp, valued=False),
                        size=head.map(lambda plane: (plane.shape[1], plane.shape[0])).default_value((size, size)),
                        stack=pair[0],
                        surface=Nothing if head.is_some() else Some(lighting.surface),
                        context=entered.bind(lambda held: held[1]),
                    )
                )

            entry: BlenderResult[bpy.types.SceneCompositorEffect] = Option.of_optional(scene.compositor_effects.get(name)).to_result_with(
                lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=name),)
            )
            return paired(entry, frame).bind(lambda pair: grouped(*pair))

        with intermediate(identity, scene), ExitStack() as copies:
            match target.source:
                case GeometrySource(item=item, modifier=name) if render.kind is RenderKind.GEOMETRY_CLAY:
                    staging = paired(clayed(item, name, copies), msgspec.structs.replace(lighting, strength=0.25).environment).map(
                        lambda pair: msgspec.structs.replace(staged(pair[1], Lamp(energy=4.0, rotation=(CLAY_AIM.x, CLAY_AIM.y, CLAY_AIM.z)), valued=False), viewpoint=Fitted(subject=pair[0]))
                    )
                case GeometrySource():
                    staging = paired(swatched(), lighting.environment).map(
                        lambda pair: msgspec.structs.replace(staged(pair[1], lighting.lamp, valued=True), material=Some(pair[0]), surface=Some(PreviewMesh.PLANE))
                    )
                case CompositorSource(entry=name):
                    staging = chained(name, copies)
            return paired(staging.bind(lambda desired: self.preview.rendered(identity, desired)), config_of()).bind(lambda pair: pair[0].formed(viewed(scene), pair[1]))

    def thumbnailed(self, identity: Identity, render: TargetRender, demand: Demand, size: int, kept: frozenset[Hashable], settings: NodePreviews, *, scene: bpy.types.Scene) -> BlenderResult[None]:
        """Renders a target into texture cache and previews folder, holding uniform value."""
        state, folder, limit = NODE_STATES.of(identity), identity.folder / "previews", settings.display.limit

        def pruned() -> BlenderResult[None]:
            files: BlenderResult[tuple[Path, ...]] = catch(lambda: tuple(sorted(folder.glob("*.exr"), key=lambda path: path.stat().st_mtime)), exception=OSError)().map_error(
                lambda error: (FileFault(access="read", request=folder, error=error),)
            )
            return Result[tuple[Path, ...], BlenderFaults].bind(files, lambda found: accumulated(removed(path) for path in found[:-limit])).map(lambda _: None)

        def numbered(readback: Readback) -> Option[float]:
            match render.kind:
                case RenderKind.EFFECT_SWATCH:
                    return value(readback.plane[..., 0][readback.plane[..., 3] > Readback.covered]) if render.valued else Nothing
                case _:
                    return readback.values.bind(value)

        def stored(readback: Readback, config: PyOpenColorIO.Config, name: str, space: str) -> BlenderResult[None]:
            number = numbered(readback)

            def written_file(digest: ContentHash) -> BlenderResult[None]:
                directory = catch(lambda: folder.mkdir(parents=True, exist_ok=True), exception=OSError)().map_error(lambda error: (FileFault(access="write", request=folder, error=error),))
                fields = number.map(lambda held_value: {name: msgspec.json.encode(held_value).decode()}).default_value({})
                return (
                    Result[None, BlenderFaults]
                    .bind(directory, lambda _: ImageFile(pixels=readback.plane, colorspace=space).write(str(folder / f"{digest.hex()}.exr"), Storage(depth=16), fields, config))
                    .bind(lambda _: pruned())
                )

            def saved(_texture: gpu.types.GPUTexture) -> BlenderResult[None]:
                for held_value in number.to_list():
                    state.values[demand.key] = held_value
                return demand.content.map(written_file).default_value(Ok(None)).bind(lambda _: self.signal.fired())

            return TEXTURES.of(identity).stored(demand.key, readback.plane, limit, kept).bind(saved) if state.keys.get(render.target) is demand else Ok(None)

        formed = viewed(scene)

        def spaced(config: PyOpenColorIO.Config) -> BlenderResult[PyOpenColorIO.ColorSpace]:
            return formed.colorspace(config).to_result_with(lambda: (BlenderAbsentFault(owner=repr(formed), key=formed.display),))

        return paired(self.rendered(identity, render, size, settings, scene=scene), config_of(), identity.name(Name.ATTACHMENT, PreviewScene.value)).bind(
            lambda parts: spaced(parts[1]).bind(lambda space: stored(parts[0], parts[1], parts[2], space.getName()))
        )

    # --- [FEED]

    def jobs(self, identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[Job, ...]]:
        """Writes editor cells and returns a job per target key neither cache nor folder holds."""
        state, metrics, failed = NODE_STATES.of(identity), Metrics.read(), BACKLOG.of(identity).failed
        prior, state.keys = state.keys, {}
        views = tuple(
            (editor, space, region, tree)
            for manager in Option.of_optional(context.window_manager).to_list()
            for window in manager.windows
            for area in window.screen.areas
            for space in (area.spaces.active,)
            if isinstance(space, bpy.types.SpaceNodeEditor)
            for region in area.regions
            if region.type == "WINDOW"
            for editor in Editor.of(space).to_list()
            for tree in Option.of_optional(space.edit_tree).to_list()
        )

        def keyed(node: bpy.types.Node, render: TargetRender, staged: Staged, config: PyOpenColorIO.Config, enclosing: tuple[bpy.types.Node, ...]) -> BlenderResult[Option[ContentHash]]:
            def cached(block: bpy.types.ID, *, weighted: bool) -> BlenderResult[ContentHash]:
                handle: Handle[bpy.types.ID] = Handle.of(block)
                return Option.of_optional(state.held.get(handle)).map(Ok).default_with(lambda: content.fingerprinted(block, weighted=weighted).map(lambda key: state.held.setdefault(handle, key)))

            def geometry(item: bpy.types.Object, name: str) -> BlenderResult[Option[tuple[msgspec.Struct | ContentHash, ...]]]:
                earlier = tuple(itertools.takewhile(lambda modifier: modifier.name != name, item.modifiers))
                snapshots = accumulated(Snapshot.capture(modifier) for modifier in (*earlier, *Option.of_optional(item.modifiers.get(name)).to_list()))
                trees = accumulated(fingerprinted(group) for modifier in earlier if isinstance(modifier, bpy.types.NodesModifier) for group in Option.of_optional(modifier.node_group).to_list())
                data = accumulated(cached(block, weighted=bool(item.vertex_groups)) for block in Option.of_optional(item.data).to_list())
                return paired(snapshots, trees, data).map(
                    lambda parts: reduce(
                        lambda values, key: values.map2(lambda rows, key: (*rows, key), key),
                        parts[1],
                        Option[tuple[msgspec.Struct | ContentHash, ...]].Some((
                            *parts[0],
                            Snapshot.of(bpy.types.Object, {"vertex_groups": tuple(group.name for group in item.vertex_groups)}),
                            *parts[2],
                        )),
                    )
                )

            def sourced() -> BlenderResult[Option[tuple[msgspec.Struct | ContentHash, ...]]]:
                match render.target.source:
                    case GeometrySource(item=handle, modifier=name):
                        return handle.resolve().bind(lambda item: geometry(item, name))
                    case CompositorSource():
                        return Ok(Some(()))

            return paired(fingerprinted(node), accumulated(fingerprinted(group) for group in enclosing), sourced()).map(
                lambda parts: reduce(
                    lambda own, key: own.map2(lambda prior_key, current: content_hash(TargetRender, prior_key, current), key),
                    parts[1],
                    parts[0].map2(lambda own, source: content_hash(TargetRender, staged, own, *source, config=Some(config.getCacheID())), parts[2]),
                )
            )

        def restored(key: ContentHash, config: PyOpenColorIO.Config, name: str, limit: int, kept: frozenset[Hashable]) -> BlenderResult[bool]:
            path = identity.folder / "previews" / f"{key.hex()}.exr"

            def cached(image: ImageFile[np.float32]) -> BlenderResult[bool]:
                match catch(lambda: tuple(msgspec.json.decode(str(field.value), type=float) for field in image.header if field.name == name), exception=msgspec.DecodeError)():
                    case Result(tag="ok", ok=numbers):

                        def recorded(_: object) -> bool:
                            for number in numbers[-1:]:
                                state.values[key] = number
                            return True

                        return (
                            TEXTURES
                            .of(identity)
                            .stored(key, image.pixels, limit, kept)
                            .bind(lambda _: catch(lambda: os.utime(path), exception=OSError)().map_error(lambda error: (FileFault(access="write", request=path, error=error),)))
                            .map(recorded)
                        )
                    case _:
                        return Ok(False)

            match read_file(str(path), config):
                case Result(tag="error", error=faults) if all(isinstance(fault, FileFault) for fault in faults):
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

        def planned(scene: bpy.types.Scene, settings: NodePreviews, config: PyOpenColorIO.Config, name: str) -> BlenderResult[tuple[Job, ...]]:
            def demanded(node: bpy.types.Node, render: TargetRender, size: int, nested: tuple[bpy.types.Node, ...], source: bpy.types.Scene, place: Hashable) -> BlenderResult[Demand]:
                staged = Staged(
                    kind=render.kind,
                    socket=render.target.socket,
                    engine=PreviewEngine.of(source),
                    size=size,
                    lighting=settings.lighting,
                    view=viewed(source),
                    frame=(source.frame_current,) if settings.frames else (),
                    host=(bpy.app.version, bpy.app.build_hash),
                    config=config.getCacheID(),
                )
                match render.target.source:
                    case CompositorSource(entry=entry):
                        ahead = tuple(itertools.takewhile(lambda held: held.name != entry, source.compositor_effects))
                        context = accumulated(
                            paired(inputs_of(mirror.entry).bind(Snapshot.capture), fingerprinted(mirror.group)).map(lambda parts: parts[1].map(lambda key: (parts[0], key)))
                            for mirror in Mirrored.of(source, Enabled.PREVIEW)
                            if mirror.entry in ahead
                        ).map(
                            lambda found: (
                                Some((*(part for held in found for pair in held.to_list() for part in pair), *(held.key for held in self.entering(identity, source).to_list())))
                                if all(held.is_some() for held in found)
                                else Nothing
                            )
                        )
                    case GeometrySource():
                        context = Ok(Some(()))

                def remembered(digest: Option[ContentHash]) -> Demand:
                    state.keys[place] = demand = (
                        Option.of_optional(prior.get(place)).filter(lambda held: held.staged == staged and held.content == digest).default_with(lambda: Demand(staged=staged, content=digest))
                    )
                    return demand

                return (
                    Option
                    .of_optional(prior.get(place))
                    .filter(lambda held: held.staged == staged and render.kind is not RenderKind.EFFECT_SWATCH)
                    .map(lambda held: Ok(remembered(held.content)))
                    .default_with(
                        lambda: (
                            paired(keyed(node, render, staged, config, nested), context)
                            .map(lambda parts: parts[0].map2(lambda key, values: content_hash(TargetRender, key, *values, config=Some(config.getCacheID())), parts[1]))
                            .map(remembered)
                        )
                    )
                )

            def viewed_in(editor: Editor, space: bpy.types.SpaceNodeEditor, region: bpy.types.Region, tree: bpy.types.NodeTree) -> BlenderResult[tuple[Planned, ...]]:
                nested = tuple(node for found in enclosing(space) for node in found.to_list())

                def each(node: bpy.types.Node, render: TargetRender, size: int, sources: dict[Handle[bpy.types.Scene], bpy.types.Scene]) -> BlenderResult[Planned]:
                    source = sources[render.target.source.scene] if isinstance(render.target.source, CompositorSource) else scene
                    return demanded(node, render, size, nested, source, render.target).map(
                        lambda demand: Planned(editor=editor, region=region, tree=tree, scene=source, node=node, render=render, size=size, demand=demand)
                    )

                def rows(found: tuple[tuple[bpy.types.Node, Target], ...]) -> BlenderResult[tuple[Planned, ...]]:
                    listed = tuple(
                        (node, render, round(node.width * metrics.scale * settings.display.scale / (2 if sum(1 for held, _ in found if held == node) > 1 else 1)))
                        for node, target in found
                        for render in isolated(node, target, settings).to_list()
                    )
                    handles = tuple(dict.fromkeys(render.target.source.scene for _, render, _ in listed if isinstance(render.target.source, CompositorSource)))
                    return resolved(handles).map(lambda scenes: dict(zip(handles, scenes))).bind(lambda sources: accumulated(each(*row, sources) for row in listed))

                return targets(identity, editor, tree, settings).bind(rows)

            def scheduled(plans: tuple[Planned, ...]) -> BlenderResult[tuple[Job, ...]]:
                ranked = tuple((plan, rank(plan), plan.demand.key) for plan in plans)
                kept = frozenset(key for _plan, held, key in ranked if held is not Rank.REST)

                def shown(ready: frozenset[Hashable]) -> BlenderResult[tuple[Job, ...]]:
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
                        if key in TEXTURES.of(identity).held
                        else digest.map(lambda found: restored(found, config, name, settings.display.limit, kept).map(lambda held: Some(key) if held else Nothing)).default_value(Ok(Nothing))
                        for key, digest in {key: plan.demand.content for plan, _rank, key in ranked}.items()
                    )
                    .map(lambda parts: frozenset(key for part in parts for key in part.to_list()))
                    .bind(shown)
                )

            shown = tuple(
                view for view in views if settings.enabled and view[0].source.kind in settings.trees and not (isinstance(view[3], bpy.types.CompositorNodeTree) and view[1].overlay.show_previews)
            )
            return accumulated(itertools.starmap(viewed_in, shown)).map(lambda parts: tuple(itertools.chain.from_iterable(parts))).bind(scheduled)

        return paired(rooted(bpy.types.Scene, context), self.settings(identity), config_of(), identity.name(Name.ATTACHMENT, PreviewScene.value)).bind(lambda parts: planned(*parts))

    # --- [REGISTRATION]

    def section(self, header: tuple[Row, ...] = ()) -> Single:
        """Returns node previews section."""
        return Single(
            key="node_previews",
            title="Node Previews",
            header=header,
            rows=(
                *(OperatorButton(target=row) for row in (self.toggle, self.refresh, self.clear, self.export)),
                Counts(),
                Single(
                    key="node_previews_active",
                    title="Active Node",
                    store=Some(Hosted(host=Some(Member(root=bpy.types.SpaceNodeEditor, path="edit_tree.nodes.active")), store=Store(attachment=NodePreview.setting, retention=self.retention))),
                ),
            ),
        )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns preview declarations, state, resources, overlay, and handlers, updates of a scene under a preview render left to the render."""

        def forgotten(switch: Callable[[NodePreviews], bool]) -> BlenderResult[None]:
            read = self.settings(identity)
            for _ in read.to_option().filter(switch).to_list():
                NODE_STATES.of(identity).keys.clear()
            return read.map(lambda _: None)

        def dropped(scene: bpy.types.Scene, updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> BlenderResult[None]:
            if scene.session_uid in INTERMEDIATES.of(identity):
                return Ok(None)
            for block in (held for update in updates for held in Option.of_optional(update.id).to_list()):
                NODE_STATES.of(identity).held.pop(Handle.of(block), None)
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
            UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=STORED, body=dropped),
            UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Object,), body=dropped, changes=Some(Change.GEOMETRY | Change.SHADING)),
            SceneHandler(handlers=bpy.app.handlers.frame_change_post, body=lambda _: forgotten(lambda settings: settings.frames)),
        ))

    def rerendered(self, identity: Identity) -> BlenderResult[None]:
        """Drops target keys and held failures for a full rerender."""
        state = NODE_STATES.of(identity)
        PreviewQueue.forced(identity, tuple(state.keys))
        state.keys.clear()
        return self.signal.fired()


# --- [OPERATIONS] -----------------------------------------------------------------------


def targets(identity: Identity, editor: Editor, tree: bpy.types.NodeTree, settings: NodePreviews) -> BlenderResult[tuple[tuple[bpy.types.Node, Target], ...]]:
    """Returns in-scope tree nodes with targets their outputs and setting choose."""
    zones = zoned(tree)

    def chosen(node: bpy.types.Node, setting: NodeSetting) -> tuple[str, ...]:
        eligible, inputs = outputs_of(node), node.inputs
        linked = tuple(socket.identifier for socket in eligible if socket.is_linked)
        picked = tuple(socket.identifier for socket in eligible if setting.output.enabled and socket.identifier == setting.output.value)
        match node:
            case bpy.types.NodeGroupOutput():
                return tuple(socket.identifier for socket in inputs if socket.is_linked)[:1]
            case _:
                return linked if settings.every else (picked or linked or tuple(socket.identifier for socket in eligible))[:1]

    def shown(node: bpy.types.Node, setting: NodeSetting) -> bool:
        match settings.scope:
            case PreviewScope.ALL:
                return True
            case PreviewScope.SELECTED:
                return node.select
            case PreviewScope.MARKED:
                return setting.show

    candidates = tuple(
        node
        for node in tree.nodes
        if not (
            isinstance(node, bpy.types.NodeFrame | bpy.types.NodeReroute | bpy.types.NodeGroupInput)
            or node.mute
            or node.name in zones
            or (isinstance(node, bpy.types.NodeGroupOutput) and not isinstance(tree, bpy.types.CompositorNodeTree))
        )
    )
    return (
        NodePreview.setting
        .bound(identity)
        .bind(lambda attached: accumulated(attached.read(node).map(lambda setting: (node, setting)) for node in candidates))
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
        match target.source.kind:
            case TreeKind.GEOMETRY:
                return Some(RenderKind.GEOMETRY_CLAY) if socket.type == "GEOMETRY" else Some(RenderKind.FIELD_SWATCH) if settings.fields and isinstance(node, bpy.types.ShaderNode) else Nothing
            case TreeKind.COMPOSITOR:
                return Some(RenderKind.EFFECT_SWATCH)

    return (
        socket_at(inputs if isinstance(node, OutputNode) else outputs, target.socket)
        .to_option()
        .bind(lambda socket: kind(socket).map(lambda found: (socket, found)))
        .map(lambda pair: TargetRender(target=target, kind=pair[1], valued=pair[0].type in {"VALUE", "INT", "BOOLEAN"}))
    )


def sourced(space: bpy.types.SpaceNodeEditor, root: bpy.types.NodeTree) -> Option[TreeSource]:
    """Returns data-block a node editor shows, with modifier for geometry groups and entry for effect groups."""
    match space.id, space.id_from, root:
        case _, bpy.types.Object() as item, bpy.types.GeometryNodeTree():
            using = tuple(modifier for modifier in item.modifiers if isinstance(modifier, bpy.types.NodesModifier) and modifier.node_group == root)
            return Option.of_optional(next((modifier for modifier in using if modifier.is_active), next(iter(using), None))).map(
                lambda modifier: GeometrySource(item=Handle.of(item), modifier=modifier.name)
            )
        case bpy.types.Scene() as scene, _, bpy.types.CompositorNodeTree():
            return Option.of_optional(next((entry.name for entry in scene.compositor_effects if entry.node_group == root), None)).map(
                lambda entry: CompositorSource(scene=Handle.of(scene), entry=entry)
            )
        case _:
            return Nothing


def enclosing(space: bpy.types.SpaceNodeEditor) -> tuple[Option[bpy.types.Node], ...]:
    """Returns group node entering each tree down editor path, parent active node first."""
    trees = tuple(item.node_tree for item in space.path)
    return tuple(
        Option.of_optional(next((node for node in (parent.nodes.active, *parent.nodes) if isinstance(node, GroupNode) and node.node_tree == child), None)) if parent is not None else Nothing
        for parent, child in itertools.pairwise(trees)
    )


def held(target: Target) -> BlenderResult[Holder]:
    """Resolves target source's data-block or group."""
    match target.source:
        case GeometrySource(item=handle, modifier=name):
            return handle.resolve().bind(
                lambda item: (
                    Option
                    .of_optional(item.modifiers.get(name))
                    .bind(lambda found: Option.of_optional(found.node_group) if isinstance(found, bpy.types.NodesModifier) else Nothing)
                    .to_result_with(lambda: (BlenderAbsentFault(owner=repr(item.modifiers), key=name),))
                )
            )
        case CompositorSource(scene=handle, entry=name):
            return handle.resolve().bind(
                lambda scene: (
                    Option
                    .of_optional(scene.compositor_effects.get(name))
                    .bind(lambda entry: Option.of_optional(entry.node_group))
                    .to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=name),))
                )
            )


@contextmanager
def routed[H: Holder](identity: Identity, holder: H, target: Target) -> Generator[BlenderResult[Routed[H]]]:
    """Scopes preview copies, exposing target output.

    Args:
        identity: Extension whose marks the copies hold.
        holder: Tree holder nesting target node.
        target: Route of copies, an output node naming its linked input.

    Yields:
        Copied holder and output, copies removed at scope exit.
    """

    def found(tree: bpy.types.NodeTree, name: str) -> BlenderResult[bpy.types.Node]:
        return Option.of_optional(tree.nodes.get(name)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(tree.nodes), key=name),))

    def endpoint(node: bpy.types.Node) -> BlenderResult[bpy.types.NodeSocket]:
        inputs = node.inputs
        match node:
            case bpy.types.NodeGroupOutput():
                return socket_at(inputs, target.socket).bind(
                    lambda socket: Option.of_optional(next((link.from_socket for link in socket.links), None)).to_result_with(  # type: ignore[union-attr]  # ty: ignore[not-iterable]
                        lambda: (BlenderAbsentFault(owner=repr(socket.links), key=target.socket),)
                    )
                )
            case _:
                return socket_at(node.outputs, target.socket)

    with ExitStack() as copies:

        def copied[T: Holder](block: T) -> BlenderResult[T]:
            duplicate: Callable[[], T] = block.copy  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return Ownership(kind=type(block), key=PreviewScene.routes.key, saved=Saved.NEVER, single=False).made(
                identity, lambda: BlenderEditFault.attempted(repr(block), duplicate), scope=Some(copies)
            )

        def exposed(inner: bpy.types.NodeTree, source: bpy.types.NodeSocket) -> BlenderResult[str]:
            return (
                active_output(inner)
                .bind(lambda output: socket_at(output.inputs, "__extend__"))
                .bind(lambda extend: joined(inner, source, extend, handle_dynamic_sockets=True))
                .map(lambda link: link.to_socket.identifier)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            )

        def entered(node: bpy.types.Node) -> BlenderResult[bpy.types.NodeTree]:
            group = (Option.of_optional(node.node_tree) if isinstance(node, GroupNode) else Nothing).to_result_with(lambda: (BlenderAbsentFault(owner=repr(node), key=bpy.types.NodeTree.__name__),))
            return Result[bpy.types.NodeTree, BlenderFaults].bind(group, copied).bind(lambda inner: written(node, "node_tree", inner).map(lambda _: inner))

        def outward(tree: bpy.types.NodeTree, groups: tuple[str, ...]) -> BlenderResult[bpy.types.NodeSocket]:
            match groups:
                case (head, *rest):
                    return found(tree, head).bind(
                        lambda node: (
                            entered(node).bind(lambda inner: outward(inner, tuple(rest)).bind(lambda source: exposed(inner, source))).bind(lambda identifier: socket_at(node.outputs, identifier))
                        )
                    )
                case _:
                    return found(tree, target.node).bind(endpoint)

        yield copied(holder).bind(lambda copy: tree_of(copy).bind(lambda tree: outward(tree, target.groups)).map(lambda socket: Routed(holder=copy, socket=socket)))


def outputs_of(node: bpy.types.Node) -> tuple[bpy.types.NodeSocket, ...]:
    """Returns shown outputs with a socket type the tree previews."""
    shown = tuple(socket for socket in node.outputs if socket.enabled and not socket.hide and not socket.is_unavailable)
    match node.id_data:
        case bpy.types.GeometryNodeTree():
            return tuple(socket for socket in shown if socket.type == "GEOMETRY" or (isinstance(node, bpy.types.ShaderNode) and socket.type in {"RGBA", "VECTOR", "VALUE"}))
        case bpy.types.CompositorNodeTree():
            return shown
        case _:
            return ()


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


def imaged(path: Path, plane: npt.NDArray[np.float32], config: PyOpenColorIO.Config, space: str) -> BlenderResult[str]:
    """Writes a display-space thumbnail at the deepest depth the suffix's writer stores, integer depths coded from unassociated values."""

    def write(depth: Depth) -> BlenderResult[str]:
        match depth:
            case Depth.HALF | Depth.FLOAT:
                return ImageFile(pixels=plane, colorspace=space).write(str(path), Storage(depth=16 if depth is Depth.HALF else 32), {}, config)
            case Depth.UINT8 | Depth.UINT16:
                return ImageFile(pixels=depth.coded(unassociated(plane), Rounded()), colorspace=space).write(str(path), Storage(), {}, config)

    suffix = path.suffix.lower()
    chosen = Option.of_optional(next((row for row in WRITABLE if suffix.removeprefix(".") in row.extensions), None)).to_result_with(
        lambda: (DomainFault(subject="filepath", value=suffix, accepted=", ".join(SUFFIXES)),)
    )
    return Result[Format, BlenderFaults].bind(chosen, lambda row: write(row.depths[-1]))


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
    "CompositorSource",
    "Counts",
    "Display",
    "Editor",
    "Entering",
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
    "TreeKind",
    "TreeSource",
    "extensions",
    "imaged",
    "isolated",
    "outputs",
    "outputs_of",
    "targets",
    "value",
    "zoned",
]
