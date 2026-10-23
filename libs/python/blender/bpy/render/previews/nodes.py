"""Per-node preview targets, isolated renders, change keys, and node editor thumbnails."""

from collections.abc import Callable, Generator, Hashable, Iterable
from contextlib import contextmanager, ExitStack
from enum import auto, Enum
import functools
from functools import reduce
import itertools
import math
import os
from pathlib import Path
from typing import Annotated, ClassVar, Self

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
from blender.bpy.data.ids.images import loaded, Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.ids.store import Handle, resolved
from blender.bpy.data.rna.descriptors import Descriptor, IdName, rooted, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import Change, PathHandler, SceneHandler, UpdateHandler
from blender.bpy.extension.identity import Identity, Name
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderFaults, BlenderImagingFault, BlenderReadFault, BlenderResult, BlenderWriteFault
from blender.bpy.nodes.changes import reconciled
from blender.bpy.nodes.graphs import End, Graph, Link, Node
from blender.bpy.nodes.trees import active_output, created, fingerprinted, GroupNode, Holder, joined, OutputNode, socket_at, tree_of
from blender.bpy.render.color.luts import Enabled
from blender.bpy.render.effects.stack import EffectStack, viewed
from blender.bpy.render.previews.queue import Job, PreviewQueue, Rank
from blender.bpy.render.previews.scenes import Chain, Framed, Interior, Lamp, Lighting, Mirrored, Panorama, PreviewEngine, PreviewScene, Readback, Shape, Staging
from blender.bpy.render.previews.tiles import fitted, scaled, Tiles
from blender.bpy.scene.materials import Clay, shaded
from blender.bpy.scene.worlds import Environment
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.presets import removed
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.drawing.canvas import Align, Canvas, Rect, Shadow
from blender.bpy.ui.drawing.handlers import Drawing, Overlay, Signal
from blender.bpy.ui.layout.rows import alerted, Line, Member, OperatorButton, Row, RowDraw
from blender.bpy.ui.layout.sections import Hosted, Single, Store
from blender.bpy.ui.resources.previews import Drawn, Previews
from blender.bpy.ui.resources.textures import TextureCache
from blender.bpy.ui.resources.theme import color, Metrics
from blender.bpy.ui.screens.areas import AreaQuery
from blender.bpy.ui.surfaces.appends import Append
from blender.imaging.color.processors import DisplayView, unassociated
from blender.imaging.effects.quantization import codes
from blender.imaging.faults import accumulated, ImagingReadFault, paired
from blender.imaging.files.images import Depth, Format, ImageFile, read_file, Storage
from blender.imaging.hashing import content_hash, ContentHash
from blender.imaging.parameters import Choice, Gated
from blender.rendering.history import Retention

# --- [TYPES] ----------------------------------------------------------------------------


class TreeKind(Choice):
    """Trees that take thumbnails."""

    MATERIAL = 1, "Material"
    LIGHT = 2, "Light"
    WORLD = 3, "World"
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

    MATERIAL_SWATCH = 1, "Material Swatch"
    LIGHT_SWATCH = 2, "Light Swatch"
    FIELD_SWATCH = 3, "Field Swatch"
    WORLD_PANORAMA = 4, "World Panorama"
    WORLD_VOLUME = 5, "World Volume"
    GEOMETRY_CLAY = 6, "Geometry Clay"
    EFFECT_SWATCH = 7, "Effect Swatch"


class Status(Enum):
    """Cell thumbnail state."""

    READY = auto()
    QUEUED = auto()
    FAILED = auto()


type Source = MaterialSource | LightSource | WorldSource | GeometrySource | CompositorSource


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
    trees: Annotated[frozenset[TreeKind], msgspec.Meta(description="Trees that take thumbnails")] = frozenset({TreeKind.MATERIAL, TreeKind.LIGHT, TreeKind.WORLD})
    lighting: Annotated[Lighting, msgspec.Meta(description="Shader preview lighting")] = Lighting()
    display: Annotated[Display, msgspec.Meta(description="Thumbnail drawing")] = Display()


class NodeSetting(msgspec.Struct, frozen=True, kw_only=True):
    """Node thumbnail settings."""

    show: Annotated[bool, msgspec.Meta(title="Show Preview", description="Thumbnail under Marked scope")] = True
    output: Annotated[Gated[Annotated[str, outputs]], msgspec.Meta(description="Shown output, first linked when off")] = Gated(enabled=False, value="")


class Export(msgspec.Struct, frozen=True, kw_only=True):
    """Exported thumbnail size and image load after write."""

    size: Annotated[int, msgspec.Meta(ge=256, le=2048, description="Image side in pixels")] = 512
    opened: Annotated[bool, msgspec.Meta(title="Open Image", description="Load written image")] = True


class MaterialSource(msgspec.Struct, frozen=True, kw_only=True):
    """Material whose tree an editor shows."""

    kind: ClassVar[TreeKind] = TreeKind.MATERIAL
    material: Handle[bpy.types.Material]


class LightSource(msgspec.Struct, frozen=True, kw_only=True):
    """Light whose tree an editor shows."""

    kind: ClassVar[TreeKind] = TreeKind.LIGHT
    light: Handle[bpy.types.Light]


class WorldSource(msgspec.Struct, frozen=True, kw_only=True):
    """World whose tree an editor shows."""

    kind: ClassVar[TreeKind] = TreeKind.WORLD
    world: Handle[bpy.types.World]


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

    source: Source
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

    source: Source
    groups: tuple[str, ...]
    node: str
    socket: str


class Routed[H: Holder](msgspec.Struct, frozen=True, kw_only=True):
    """Copied holder and routed socket, or kept output node's input."""

    holder: H
    socket: bpy.types.NodeSocket


class Render(msgspec.Struct, frozen=True, kw_only=True):
    """Isolated target render with kind, value read, and swatch shape."""

    target: Target
    kind: RenderKind
    valued: bool
    shape: Option[Shape] = Nothing


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


class Planned(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Editor target of one feed pass."""

    editor: Editor
    region: bpy.types.Region
    tree: bpy.types.NodeTree
    scene: bpy.types.Scene
    node: bpy.types.Node
    render: Render
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

    signal: Signal
    keys: dict[Hashable, Demand] = msgspec.field(default_factory=dict)
    values: dict[Hashable, float] = msgspec.field(default_factory=dict)
    held: dict[Handle[bpy.types.ID], ContentHash] = msgspec.field(default_factory=dict)
    cells: dict[Editor, tuple[Cell, ...]] = msgspec.field(default_factory=dict)
    stages: dict[Target, float] = msgspec.field(default_factory=dict)


class Counts(Line, frozen=True):
    """Queued, failed, and cached thumbnail counts with cache memory."""

    @staticmethod
    def drawn(identity: Identity, /) -> BlenderResult[RowDraw]:
        """Returns label reading backlog and cache per draw."""

        def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", _context: bpy.types.Context) -> None:
            backlog = PreviewQueue.held[identity.package]
            queued, failed = len(backlog.jobs), len(backlog.failed)
            cache = TextureCache(identity=identity)
            layout.label(text=f"{queued} queued, {failed} failed, {len(TextureCache.held[identity.package])} cached, {cache.memory / 2**20:.1f} MiB pixels")

        return Ok(shown)


class NodeThumbnails(msgspec.Struct, frozen=True, kw_only=True):
    """Drawn part placing cells beside nodes."""

    identity: Identity
    settings: Callable[[Identity], BlenderResult[NodePreviews]]

    def read(self, _metrics: Metrics, /) -> BlenderResult[Option[Shown]]:
        """Reads display settings and editor cells while previews show."""
        return self.settings(self.identity).map(lambda settings: Some(Shown(display=settings.display, cells=NodePreview.state[self.identity.package].cells)) if settings.enabled else Nothing)

    def drawn(self, canvas: Canvas, state: Shown) -> None:
        """Draws cells beside nodes, active node grid last."""
        metrics, display, theme, style = canvas.metrics, state.display, bpy.context.preferences.themes[0], bpy.context.preferences.ui_styles[0].widget  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        cache, points, pad, gap, checker = TextureCache(identity=self.identity), metrics.text(style), metrics.spacing.button_y, metrics.spacing.template, metrics.checker()
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


class NodePreview:
    """Row of per-node previews over every node editor."""

    setting: ClassVar[Attachment[NodeSetting]] = Attachment(key="node_preview", form=NodeSetting, owner=bpy.types.Node)
    clay: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="preview_clay", saved=Saved.NEVER)
    swatch: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="preview_swatch", saved=Saved.NEVER)
    state: ClassVar[dict[str, NodeState]] = {}

    def __init__(
        self,
        *,
        stack: EffectStack,
        tiles: Tiles,
        settings: Callable[[Identity], BlenderResult[NodePreviews]],
        stored: Callable[[Identity, NodePreviews], BlenderResult[None]],
        retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing,
    ) -> None:
        """Builds operators once for every registration placement."""
        self.stack, self.tiles, self.settings, self.stored, self.retention = stack, tiles, settings, stored, retention

        self.toggle = Operator(
            key="toggle_node_previews",
            title="Toggle Node Previews",
            description="Toggle node editor thumbnails",
            form=Empty,
            undo=False,
            run=lambda call: (
                self
                .settings(call.identity)
                .bind(lambda held: self.stored(call.identity, msgspec.structs.replace(held, enabled=not held.enabled)))
                .bind(lambda _: NodePreview.state[call.identity.package].signal.fired())
                .map(lambda _: Done())
            ),
        )

        self.enlarge = Operator(
            key="enlarge_node_preview",
            title="Enlarge Active Node Preview",
            description="Toggle larger active node thumbnails on top",
            form=Empty,
            undo=False,
            run=lambda call: (
                self
                .settings(call.identity)
                .bind(lambda held: self.stored(call.identity, msgspec.structs.replace(held, display=msgspec.structs.replace(held.display, enlarged=not held.display.enlarged))))
                .bind(lambda _: NodePreview.state[call.identity.package].signal.fired())
                .map(lambda _: Done())
            ),
        )

        self.refresh = Operator(
            key="refresh_node_previews",
            title="Refresh Node Previews",
            description="Render every node thumbnail again",
            form=Empty,
            undo=False,
            run=lambda call: refreshed(call.identity).map(lambda _: Done()),
        )

        self.clear = Operator(
            key="clear_node_previews",
            title="Clear Node Previews",
            description="Remove cached and stored thumbnails and render again",
            form=Empty,
            undo=False,
            run=lambda call: cleared(call.identity).map(lambda count: Done(notes=(("INFO", f"Removed {count} stored thumbnails"),))),
        )

        def marking(call: Call[Empty]) -> BlenderResult[Outcome]:
            if not (nodes := tuple(call.context.selected_nodes)):
                return Ok(Unchanged(reason="No node selected"))

            def flipping(attached: Attached[NodeSetting]) -> BlenderResult[Done]:
                edits = tuple(attached.read(node).bind(lambda held: attached.write(node, msgspec.structs.replace(held, show=not held.show))) for node in nodes)
                return tallied(edits, (("INFO", f"Toggled {sum(edit.is_ok() for edit in edits)} node marks"),))

            return NodePreview.setting.bound(call.identity).bind(flipping)

        self.mark = Operator(key="mark_node_previews", title="Mark Node Previews", description="Toggle selected node thumbnails under Marked scope", form=Empty, run=marking)

        writable = Format.writable()
        suffixes = tuple(f".{extension}" for row in writable for extension in row.extensions)

        def written_file(path: Path, plane: npt.NDArray[np.float32], config: PyOpenColorIO.Config, space: str) -> BlenderResult[str]:
            suffix = path.suffix.lower()
            chosen = Option.of_optional(next((row for row in writable if suffix.removeprefix(".") in row.extensions), None)).to_result_with(
                lambda: (BlenderChoiceFault(path="filepath", identifier=suffix, accepted=suffixes),)
            )

            def write(row: Format) -> BlenderResult[str]:
                depth = row.depths[-1]
                match depth:
                    case Depth.UINT8 | Depth.UINT16:
                        image = ImageFile(pixels=codes(unassociated(plane), 8) if depth is Depth.UINT8 else codes(unassociated(plane), 16), colorspace=space).write(str(path), Storage(), {}, config)
                    case Depth.HALF | Depth.FLOAT:
                        image = ImageFile(pixels=plane, colorspace=space).write(str(path), Storage(depth=16 if depth is Depth.HALF else 32), {}, config)
                return image.map_error(BlenderImagingFault.lifted)

            return Result[Format, BlenderFaults].bind(chosen, write)

        def exported(call: Call[Export]) -> BlenderResult[Done]:
            space = call.context.space_data

            def chosen(editor: Editor, tree: bpy.types.NodeTree, node: bpy.types.Node, settings: NodePreviews) -> BlenderResult[Render]:
                return targets(call.identity, editor, tree, settings).bind(
                    lambda found: Option.of_optional(next((render for held, target in found if held == node for render in isolated(held, target, settings).to_list()), None)).to_result_with(
                        lambda: (BlenderAbsentFault(owner=repr(tree.nodes), key=node.name),)
                    )
                )

            def wrote(scene: bpy.types.Scene, render: Render, settings: NodePreviews, config: PyOpenColorIO.Config) -> BlenderResult[str]:
                formed = viewed(scene)
                return paired(
                    self.rendered(call.identity, render, call.record.size, settings, scene=scene),
                    formed.colorspace(config).to_result_with(lambda: (BlenderAbsentFault(owner=repr(formed), key=formed.display),)),
                ).bind(lambda pair: written_file(call.paths[0], pair[0].plane, config, pair[1].getName()))

            viewing = (
                Option
                .of_optional(space if isinstance(space, bpy.types.SpaceNodeEditor) else None)
                .bind(lambda found: Editor.of(found).bind(lambda editor: Option.of_optional(found.edit_tree).map(lambda tree: (editor, tree))))
                .to_result_with(lambda: (BlenderAbsentFault(owner="bpy.context", key="space_data"),))
            )
            active = (
                Option
                .of_optional(space.edit_tree if isinstance(space, bpy.types.SpaceNodeEditor) else None)
                .bind(lambda tree: Option.of_optional(tree.nodes.active))
                .to_result_with(lambda: (BlenderAbsentFault(owner="SpaceNodeEditor.edit_tree.nodes", key="active"),))
            )
            return (
                paired(paired(viewing, active), paired(self.settings(call.identity), self.stack.config()))
                .bind(lambda parts: chosen(*parts[0][0], parts[0][1], parts[1][0]).bind(lambda render: call.root(bpy.types.Scene).bind(lambda scene: wrote(scene, render, *parts[1]))))
                .bind(lambda name: loaded(name, Tag()).map(lambda _: name) if call.record.opened else Ok(name))
                .map(lambda name: Done.wrote((name,)))
            )

        self.export = Operator(
            key="export_node_preview",
            title="Export Node Preview",
            description="Write active node thumbnail to an image file",
            form=Export,
            undo=False,
            invoke=Some(Save(extensions=lambda _: (".exr", *(suffix for suffix in suffixes if suffix != ".exr")))),
            run=exported,
        )

    def rendered(self, identity: Identity, render: Render, size: int, settings: NodePreviews, *, scene: bpy.types.Scene) -> BlenderResult[Readback]:
        """Renders target through routed copies in preview scene, staged per source kind."""
        lighting, target = settings.lighting, render.target
        aov: BlenderResult[Option[str]] = identity.name(Name.ATTACHMENT, PreviewScene.value).map(Some) if render.valued else Ok(Nothing)
        environment = lighting.environment

        def wired[H: Holder](source: BlenderResult[H], output: type[bpy.types.Node]) -> BlenderResult[Routed[H]]:
            return paired(source.bind(lambda found: resources.enter_context(routed(identity, found, target))), aov).bind(
                lambda pair: (
                    tree_of(pair[0].holder)
                    .bind(
                        lambda tree: (
                            written(pair[0].socket.node, "is_active_output", True).map(lambda _: None) if isinstance(pair[0].socket.node, OutputNode) else outlet(tree, pair[0].socket, output, pair[1])
                        )
                    )
                    .map(lambda _: pair[0])
                )
            )

        def clay() -> BlenderResult[bpy.types.Material]:
            return paired(NodePreview.clay.made(identity, lambda: Ok(bpy.data.materials.new("Clay"))), self.stack.config()).bind(lambda pair: shaded(pair[0], Clay(), pair[1], identity))

        def staged(world: Environment | bpy.types.World, lamp: Lamp | bpy.types.Light) -> Staging:
            return Staging(source=scene, size=(size, size), engine=PreviewEngine.of(scene), world=world, lamp=lamp, value=render.valued, chain=Chain.of(scene, Nothing, (), ()))

        def swatched() -> BlenderResult[bpy.types.Material]:
            def defaulted(socket: bpy.types.NodeSocket) -> Option[Value]:
                found: bpy.types.Property | None = socket.bl_rna.properties.get("default_value")
                return Option.of_optional(found).map(lambda prop: Descriptor(root=type(socket), owner="", prop=prop).read(socket))

            def graph(source: bpy.types.Node, name: Option[str]) -> Graph:
                socket, kind = target.socket, type(source)
                own = {
                    descriptor: descriptor.read(source)
                    for prop in source.bl_rna.properties
                    if prop.identifier not in bpy.types.Node.bl_rna.properties and not prop.is_readonly
                    for descriptor in (Descriptor(root=kind, owner="", prop=prop),)
                }
                inputs = {item.identifier: held for item in source.inputs if not item.is_linked for held in defaulted(item).to_list()}
                sink, aovs = bpy.types.ShaderNodeOutputMaterial, bpy.types.ShaderNodeOutputAOV
                return Graph(
                    kind=bpy.types.ShaderNodeTree,
                    nodes={
                        "field": Node(kind=kind, members=own, inputs=inputs),
                        "coordinates": Node(kind=bpy.types.ShaderNodeTexCoord),
                        "emission": Node(kind=bpy.types.ShaderNodeEmission),
                        "output": Node(kind=sink, members={Descriptor(root=sink, owner="", prop=sink.bl_rna.properties["target"]): "ALL"}),
                        **name.map(lambda held: {"aov": Node(kind=aovs, members={Descriptor(root=aovs, owner="", prop=aovs.bl_rna.properties["aov_name"]): held})}).default_value({}),
                    },
                    links=(
                        *(
                            Link(source=End(node="coordinates", socket="Generated"), target=End(node="field", socket=item.identifier))
                            for item in source.inputs
                            if item.type == "VECTOR" and not item.is_linked
                        ),
                        Link(source=End(node="field", socket=socket), target=End(node="emission", socket="Color")),
                        Link(source=End(node="emission", socket="Emission"), target=End(node="output", socket="Surface")),
                        *(Link(source=End(node="field", socket=socket), target=End(node="aov", socket="Value")) for _ in name.to_list()),
                    ),
                )

            found = (
                held(target)
                .map(lambda root: Editor(source=target.source, groups=target.groups).tree(root) if isinstance(root, bpy.types.NodeTree) else Nothing)
                .bind(lambda tree: tree.bind(lambda inner: Option.of_optional(inner.nodes.get(target.node))).to_result_with(lambda: (BlenderAbsentFault(owner=repr(target.source), key=target.node),)))
            )
            return paired(paired(found, aov), NodePreview.swatch.made(identity, lambda: Ok(bpy.data.materials.new("Swatch")))).bind(
                lambda parts: tree_of(parts[1]).bind(lambda tree: reconciled(graph(*parts[0]), tree, identity)).map(lambda _: parts[1])
            )

        def clayed() -> BlenderResult[bpy.types.Object]:
            def clothed(group: bpy.types.NodeTree, carried: Routed[Holder], material: bpy.types.Material) -> BlenderResult[None]:
                return active_output(group).bind(
                    lambda sink: created(group, bpy.types.GeometryNodeSetMaterial).bind(
                        lambda painted: accumulated((
                            socket_at(painted.inputs, "Geometry").bind(lambda target: joined(group, carried.socket, target)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                            socket_at(painted.outputs, "Geometry").bind(lambda source: joined(group, source, sink.inputs[0])),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                            socket_at(painted.inputs, "Material").bind(lambda socket: written(socket, "default_value", material)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                        )).map(lambda _: None)
                    )
                )

            def copied(item: bpy.types.Object, name: str, carried: Routed[Holder], material: bpy.types.Material) -> BlenderResult[bpy.types.Object]:
                def acquired(copy: bpy.types.Object) -> bpy.types.Object:
                    resources.callback(bpy.data.objects.remove, copy)
                    return copy

                return paired(
                    tree_of(carried.holder),
                    Ownership(kind=bpy.types.Object, key=PreviewScene.routes.key, saved=Saved.NEVER, single=False)
                    .made(identity, lambda: Option.of_optional(item.copy()).to_result_with(lambda: (BlenderAbsentFault(owner=repr(item), key="copy"),)))
                    .map(acquired),
                ).bind(
                    lambda pair: accumulated((
                        written(pair[1], "parent", None),
                        written(pair[1], "location", (0.0, 0.0, 0.0)),
                        written(pair[1].modifiers[name], "node_group", pair[0]),
                        *(written(later, "show_render", value=False) for later in itertools.dropwhile(lambda modifier: modifier.name != name, pair[1].modifiers) if later.name != name),
                        clothed(pair[0], carried, material),
                    )).map(lambda _: pair[1])
                )

            match target.source:
                case GeometrySource(item=handle, modifier=name):
                    return paired(paired(handle.resolve(), held(target).bind(lambda group: resources.enter_context(routed(identity, group, target)))), clay()).bind(
                        lambda parts: copied(parts[0][0], name, parts[0][1], parts[1])
                    )
                case MaterialSource() | LightSource() | WorldSource() | CompositorSource():
                    return Error((BlenderAbsentFault(owner=repr(target.source), key=target.node),))

        with ExitStack() as resources:
            match target.source:
                case MaterialSource(material=handle):
                    staging = paired(wired(handle.resolve(), bpy.types.ShaderNodeOutputMaterial), environment).map(
                        lambda pair: msgspec.structs.replace(staged(pair[1], lighting.lamp), material=Some(pair[0].holder), shape=render.shape)
                    )
                case LightSource(light=handle):
                    staging = paired(paired(wired(handle.resolve(), bpy.types.ShaderNodeOutputLight), clay()), environment).map(
                        lambda parts: msgspec.structs.replace(staged(parts[1], parts[0][0].holder), engine=PreviewEngine.CYCLES, material=Some(parts[0][1]), shape=Some(Shape.SPHERE))
                    )
                case WorldSource(world=handle):
                    world = wired(handle.resolve(), bpy.types.ShaderNodeOutputWorld).map(lambda carried: carried.holder)
                    staging = (
                        paired(world, clay()).map(
                            lambda pair: msgspec.structs.replace(
                                staged(pair[0], Lamp(energy=1500.0, kind="POINT", location=(2.0, -2.0, 2.0))), transparent=False, view=Interior(), material=Some(pair[1]), shape=Some(Shape.SPHERE)
                            )
                        )
                        if render.kind is RenderKind.WORLD_VOLUME
                        else world.map(lambda held: msgspec.structs.replace(staged(held, lighting.lamp), transparent=False, view=Panorama()))
                    )
                case GeometrySource():
                    aim = Vector((0.3, -0.6, 0.9)).normalized().to_track_quat("Z", "Y").to_euler()
                    staging = (
                        paired(clayed(), msgspec.structs.replace(lighting, strength=0.25).environment).map(
                            lambda pair: msgspec.structs.replace(staged(pair[1], Lamp(energy=4.0, rotation=(aim.x, aim.y, aim.z))), subject=Some(pair[0]), view=Framed())
                        )
                        if render.kind is RenderKind.GEOMETRY_CLAY
                        else paired(swatched(), environment).map(lambda pair: msgspec.structs.replace(staged(pair[1], lighting.lamp), material=Some(pair[0]), shape=Some(Shape.PLANE)))
                    )
                case CompositorSource():
                    staging = paired(self.effected(identity, scene, target, size, resources), environment).map(
                        lambda pair: msgspec.structs.replace(
                            staged(pair[1], lighting.lamp),
                            size=pair[0][1],
                            chain=pair[0][0],
                            shape=Nothing if pair[0][0].head.is_some() else Some(lighting.shape),
                            context=Tiles.held[identity.package].proxy.bind(
                                lambda proxy: pair[0][0].head.map(lambda pixels: ((pixels.shape[0], pixels.shape[1]), scaled(proxy.framing, proxy.plane.shape[:2], pixels.shape[:2])))
                            ),
                        )
                    )
            return staging.bind(lambda desired: PreviewScene(stack=self.stack).rendered(identity, desired))

    def effected(self, identity: Identity, scene: bpy.types.Scene, target: Target, size: int, resources: ExitStack) -> BlenderResult[tuple[Chain, tuple[int, int]]]:
        """Chains routed effect group copy after earlier preview-enabled entries, over tile proxy."""
        preview = PreviewScene(stack=self.stack)

        def chained(entry: bpy.types.SceneCompositorEffect, carried: Routed[bpy.types.NodeTree], head: Option[npt.NDArray[np.float32]]) -> BlenderResult[tuple[Chain, tuple[int, int]]]:
            ahead = tuple(itertools.takewhile(lambda held: held != entry, scene.compositor_effects))
            mirrored = tuple(mirror for mirror in preview.held(identity, scene, Enabled.PREVIEW) if mirror.entry in ahead)
            group = carried.holder
            return (
                active_output(group)
                .bind(lambda sink: joined(group, carried.socket, sink.inputs[0]))
                .map(lambda _: (Chain.of(scene, head, (*mirrored, Mirrored(entry=entry, group=group)), ()), head.map(lambda plane: (plane.shape[1], plane.shape[0])).default_value((size, size))))
            )

        match target.source:
            case CompositorSource(entry=name):
                entry: BlenderResult[bpy.types.SceneCompositorEffect] = Option.of_optional(scene.compositor_effects.get(name)).to_result_with(
                    lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=name),)
                )
                head = Tiles.held[identity.package].proxy.map(lambda held: fitted(held.plane, size).map(Some)).default_value(Ok(Nothing))
                return Result[bpy.types.SceneCompositorEffect, BlenderFaults].bind(
                    entry,
                    lambda entry: paired(
                        Result[bpy.types.NodeTree, BlenderFaults].bind(
                            Option.of_optional(entry.node_group).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key="node_group"),)),
                            lambda group: resources.enter_context(routed(identity, group, target)),
                        ),
                        head,
                    ).bind(lambda pair: chained(entry, pair[0], pair[1])),
                )
            case MaterialSource() | LightSource() | WorldSource() | GeometrySource():
                return Error((BlenderAbsentFault(owner=repr(target.source), key=target.node),))

    def thumbnailed(self, identity: Identity, render: Render, demand: Demand, size: int, kept: frozenset[Hashable], settings: NodePreviews, *, scene: bpy.types.Scene) -> BlenderResult[None]:
        """Renders a target into texture cache and previews folder, holding uniform value."""
        state, folder, limit = NodePreview.state[identity.package], identity.folder / "previews", settings.display.limit
        key = demand.content.default_value(demand)

        def pruned() -> BlenderResult[None]:
            files: BlenderResult[tuple[Path, ...]] = catch(lambda: tuple(sorted(folder.glob("*.exr"), key=lambda path: path.stat().st_mtime)), exception=OSError)().map_error(
                lambda error: (BlenderReadFault(request=folder, error=error),)
            )
            return Result[tuple[Path, ...], BlenderFaults].bind(files, lambda files: accumulated(removed(path) for path in files[:-limit])).map(lambda _: None)

        def numbered(readback: Readback) -> Option[float]:
            plane = readback.plane
            match render.kind:
                case RenderKind.EFFECT_SWATCH:
                    return value(plane[..., 0][plane[..., 3] > Readback.covered]) if render.valued else Nothing
                case _:
                    return readback.values.bind(value)

        def stored(readback: Readback, config: PyOpenColorIO.Config, name: str, space: str) -> BlenderResult[None]:
            if state.keys.get(render.target) is not demand:
                return Ok(None)
            number = numbered(readback)

            def saved(_texture: gpu.types.GPUTexture) -> BlenderResult[None]:
                for held_value in number.to_list():
                    state.values[key] = held_value

                def written_file(content: ContentHash) -> BlenderResult[None]:
                    directory: BlenderResult[None] = catch(lambda: folder.mkdir(parents=True, exist_ok=True), exception=OSError)().map_error(
                        lambda error: (BlenderWriteFault(request=folder, error=error),)
                    )
                    return (
                        Result[None, BlenderFaults]
                        .bind(
                            directory,
                            lambda _: (
                                ImageFile(pixels=readback.plane, colorspace=space)
                                .write(
                                    str(folder / f"{content.hex()}.exr"), Storage(depth=16), number.map(lambda held_value: {name: msgspec.json.encode(held_value).decode()}).default_value({}), config
                                )
                                .map_error(BlenderImagingFault.lifted)
                            ),
                        )
                        .bind(lambda _: pruned())
                    )

                return demand.content.map(written_file).default_value(Ok(None)).bind(lambda _: state.signal.fired())

            return TextureCache(identity=identity).stored(key, readback.plane, limit, kept).bind(saved)

        formed = viewed(scene)
        return paired(self.rendered(identity, render, size, settings, scene=scene), paired(self.stack.config(), identity.name(Name.ATTACHMENT, PreviewScene.value))).bind(
            lambda parts: Result[PyOpenColorIO.ColorSpace, BlenderFaults].bind(
                formed.colorspace(parts[1][0]).to_result_with(lambda: (BlenderAbsentFault(owner=repr(formed), key=formed.display),)),
                lambda space: stored(parts[0], parts[1][0], parts[1][1], space.getName()),
            )
        )

    def jobs(self, identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[Job, ...]]:
        """Writes editor cells and returns a job per target key neither cache nor folder holds."""
        state, metrics = NodePreview.state[identity.package], Metrics.read()
        prior_keys = state.keys.copy()
        state.keys.clear()
        failed = PreviewQueue.held[identity.package].failed
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

        def keyed(node: bpy.types.Node, render: Render, staged: Staged, config: PyOpenColorIO.Config, enclosing: tuple[bpy.types.Node, ...]) -> BlenderResult[Option[ContentHash]]:
            def held[B: bpy.types.ID](block: B, fresh: Callable[[B], BlenderResult[ContentHash]]) -> BlenderResult[ContentHash]:
                handle: Handle[bpy.types.ID] = Handle.of(block)
                return Option.of_optional(state.held.get(handle)).map(Ok).default_with(lambda: fresh(block).map(lambda key: state.held.setdefault(handle, key)))

            def data_key(block: bpy.types.ID, *, weighted: bool) -> BlenderResult[ContentHash]:
                def read[T](values: "bpy.types.bpy_prop_collection[T]", properties: Iterable[bpy.types.Property]) -> tuple[npt.NDArray[np.bool | np.int32 | np.float32], ...]:
                    buffers: tuple[tuple[str, npt.NDArray[np.bool | np.int32 | np.float32]], ...] = tuple(
                        (
                            prop.identifier,
                            np.empty(
                                len(values) * (1 if isinstance(prop, bpy.types.EnumProperty) else max(prop.array_length, 1)),
                                np.float32 if isinstance(prop, bpy.types.FloatProperty) else np.bool if isinstance(prop, bpy.types.BoolProperty) else np.int32,
                            ),
                        )
                        for prop in properties
                        if isinstance(prop, bpy.types.BoolProperty | bpy.types.IntProperty | bpy.types.FloatProperty | bpy.types.EnumProperty)
                    )
                    for member, buffer in buffers:
                        values.foreach_get(member, buffer)
                    return tuple(buffer for _, buffer in buffers)

                def captured(struct: "bpy.types.bpy_struct[object]") -> BlenderResult[Snapshot]:
                    definition: bpy.types.Struct = struct.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                    return Snapshot.capture(
                        struct,
                        Some(
                            tuple(
                                Descriptor(root=type(struct), owner="", prop=prop)
                                for prop in Snapshot.members(definition)
                                if not (isinstance(prop, bpy.types.CollectionProperty) or (isinstance(prop, bpy.types.PointerProperty) and prop.is_readonly))
                            )
                        ),
                    )

                shapes = tuple(Option.of_optional(block.shape_keys).to_list()) if isinstance(block, bpy.types.Mesh | bpy.types.Curve) else ()
                keys = tuple(key for shape in shapes for key in shape.key_blocks)
                materials = tuple(block.materials) if isinstance(block, bpy.types.Mesh | bpy.types.Curve | bpy.types.Curves | bpy.types.PointCloud | bpy.types.GreasePencil) else ()
                attributes: tuple[bpy.types.Attribute, ...] = ()
                structs: tuple[bpy.types.bpy_struct[object], ...] = (block, *shapes, *keys)
                arrays: tuple[npt.NDArray[np.bool | np.int32 | np.float32 | np.float64], ...] = ()
                labels: tuple[tuple[str, str | None], ...] = ()
                match block:
                    case bpy.types.Mesh():
                        attributes = tuple(block.attributes)
                        arrays = (
                            *read(block.polygons, (bpy.types.MeshPolygon.bl_rna.properties["loop_start"],)),
                            *(array for key in keys for array in read(key.points, bpy.types.ShapeKeyPoint.bl_rna.properties)),
                            *(array for layer in block.skin_vertices for array in read(layer.data, bpy.types.MeshSkinVertex.bl_rna.properties)),
                            *(
                                (np.fromiter((value for index, vertex in enumerate(block.vertices) for element in vertex.groups for value in (index, element.group, element.weight)), np.float64),)
                                if weighted
                                else ()
                            ),
                        )
                    case bpy.types.Curve():
                        profiles = tuple(Option.of_optional(block.bevel_profile).to_list())
                        structs = (*structs, *profiles)
                        arrays = (
                            *read(block.splines, bpy.types.Spline.bl_rna.properties),
                            *(array for spline in block.splines for array in read(spline.points, bpy.types.SplinePoint.bl_rna.properties)),
                            *(array for spline in block.splines for array in read(spline.bezier_points, bpy.types.BezierSplinePoint.bl_rna.properties)),
                            *(array for key in keys for array in read(key.data, (bpy.types.ShapeKeyPoint.bl_rna.properties["co"],))),
                            *(array for profile in profiles for array in read(profile.points, bpy.types.CurveProfilePoint.bl_rna.properties)),
                            *(
                                (*read(block.body_format, bpy.types.TextCharacterFormat.bl_rna.properties), *read(block.text_boxes, bpy.types.TextBox.bl_rna.properties))
                                if isinstance(block, bpy.types.TextCurve)
                                else ()
                            ),
                        )
                    case bpy.types.Curves():
                        attributes, arrays = tuple(block.attributes), read(block.curve_offset_data, bpy.types.IntAttributeValue.bl_rna.properties)
                    case bpy.types.PointCloud():
                        attributes = tuple(block.attributes)
                    case bpy.types.GreasePencil():
                        drawings = tuple(drawing for layer in block.layers for frame in layer.frames for drawing in Option.of_optional(frame.drawing).to_list())
                        nodes = (*block.layers, *block.layer_groups)
                        attributes, structs = (*block.attributes, *(attribute for drawing in drawings for attribute in drawing.attributes)), (block, *nodes)
                        arrays = (
                            *(array for layer in block.layers for array in read(layer.frames, bpy.types.GreasePencilFrame.bl_rna.properties)),
                            *(array for drawing in drawings for array in read(drawing.curve_offsets, bpy.types.IntAttributeValue.bl_rna.properties)),
                        )
                        labels = tuple((node.name, Option.of_optional(node.parent_group).map(lambda group: group.name).to_optional()) for node in nodes)
                    case _:
                        pass
                columns = tuple(
                    array
                    for attribute in attributes
                    for data in (attribute.bl_rna.properties["data"],)
                    if isinstance(data, bpy.types.CollectionProperty)
                    for item in Option.of_optional(data.fixed_type).to_list()
                    for array in read(getattr(attribute, data.identifier), item.properties)
                )
                named = msgspec.msgpack.encode((
                    tuple((attribute.name, attribute.domain, attribute.data_type) for attribute in attributes),
                    tuple(Option.of_optional(material).map(IdName.of).to_optional() for material in materials),
                    tuple(key.relative_key.name for key in keys),
                    labels,
                ))
                return accumulated(captured(struct) for struct in structs).map(lambda snapshots: content_hash(type(block), *snapshots, named, arrays=(*columns, *arrays)))

            def geometry(item: bpy.types.Object, name: str) -> BlenderResult[Option[tuple[msgspec.Struct | ContentHash, ...]]]:
                earlier = tuple(itertools.takewhile(lambda modifier: modifier.name != name, item.modifiers))
                snapshots = accumulated(Snapshot.capture(modifier) for modifier in (*earlier, *Option.of_optional(item.modifiers.get(name)).to_list()))
                trees = accumulated(fingerprinted(group) for modifier in earlier if isinstance(modifier, bpy.types.NodesModifier) for group in Option.of_optional(modifier.node_group).to_list())
                data = accumulated(held(block, lambda found: data_key(found, weighted=bool(item.vertex_groups))) for block in Option.of_optional(item.data).to_list())
                return paired(snapshots, paired(trees, data)).map(
                    lambda parts: reduce(
                        lambda values, key: values.map2(lambda rows, key: (*rows, key), key),
                        parts[1][0],
                        Option[tuple[msgspec.Struct | ContentHash, ...]].Some((
                            *parts[0],
                            Snapshot.of(bpy.types.Object, {"vertex_groups": tuple(group.name for group in item.vertex_groups)}),
                            *parts[1][1],
                        )),
                    )
                )

            def sourced() -> BlenderResult[Option[tuple[msgspec.Struct | ContentHash, ...]]]:
                match render.target.source:
                    case GeometrySource(item=handle, modifier=name):
                        return handle.resolve().bind(lambda item: geometry(item, name))
                    case MaterialSource() | LightSource() | WorldSource() | CompositorSource():
                        return Ok(Some(()))

            context = accumulated(fingerprinted(group) for group in enclosing)
            return paired(fingerprinted(node), paired(context, sourced())).map(
                lambda parts: reduce(
                    lambda own, key: own.map2(lambda prior, current: content_hash(Render, prior, current), key),
                    parts[1][0],
                    parts[0].map2(lambda own, source: content_hash(Render, staged, own, *source, config=Some(config)), parts[1][1]),
                )
            )

        def restored(key: ContentHash, config: PyOpenColorIO.Config, name: str, limit: int, kept: frozenset[Hashable]) -> BlenderResult[bool]:
            path = identity.folder / "previews" / f"{key.hex()}.exr"

            def cached(image: ImageFile[np.float32]) -> BlenderResult[bool]:
                decoded = catch(
                    lambda: dict[Hashable, float]((key, msgspec.json.decode(str(field.value), type=float)) for field in image.header if field.name == name), exception=msgspec.DecodeError
                )()
                match decoded:
                    case Result(tag="error"):
                        return Ok(False)
                    case Result(ok=numbers):
                        pass
                return (
                    TextureCache(identity=identity)
                    .stored(key, image.pixels, limit, kept)
                    .bind(lambda _: catch(lambda: os.utime(path), exception=OSError)().map_error(lambda error: (BlenderWriteFault(request=path, error=error),)))
                    .map(lambda _: state.values.update(numbers))
                    .map(lambda _: True)
                )

            match read_file(str(path), config):
                case Result(tag="error", error=faults) if all(isinstance(fault, ImagingReadFault) for fault in faults):
                    return Ok(False)
                case Result(tag="error", error=faults):
                    return Error(BlenderImagingFault.lifted(faults))
                case Result(ok=image):
                    return cached(image)

        def rank(planned: Planned) -> Rank:
            node, region = planned.node, planned.region
            position: tuple[int, int] = region.view2d.view_to_region(node.location_absolute[0] * metrics.scale, node.location_absolute[1] * metrics.scale, clip=False)  # type: ignore[assignment]
            x, y = position
            return Rank.ACTIVE if node.select or node == planned.tree.nodes.active else Rank.SHOWN if 0 <= x <= region.width and 0 <= y <= region.height else Rank.REST

        def planned(scene: bpy.types.Scene, settings: NodePreviews, config: PyOpenColorIO.Config, name: str) -> BlenderResult[tuple[Job, ...]]:
            def staging(render: Render, size: int, source: bpy.types.Scene) -> Staged:
                return Staged(
                    kind=render.kind,
                    socket=render.target.socket,
                    engine=PreviewEngine.CYCLES if render.kind is RenderKind.LIGHT_SWATCH else PreviewEngine.of(source),
                    size=size,
                    lighting=settings.lighting,
                    view=viewed(source),
                    frame=(source.frame_current,) if settings.frames else (),
                    host=(bpy.app.version, bpy.app.build_hash),
                    config=config.getCacheID(),
                )

            def key_for(node: bpy.types.Node, render: Render, size: int, nested: tuple[bpy.types.Node, ...], source: bpy.types.Scene, place: Hashable) -> BlenderResult[Demand]:
                staged = staging(render, size, source)
                match render.target.source:
                    case CompositorSource(entry=name):
                        ahead = tuple(itertools.takewhile(lambda entry: entry.name != name, source.compositor_effects))
                        context = self.tiles.parts(identity, source, mirrored=Some(tuple(mirror for mirror in self.tiles.mirrors(identity, source) if mirror.entry in ahead)))
                    case MaterialSource() | LightSource() | WorldSource() | GeometrySource():
                        context = Ok(Some(()))

                def remembered(content: Option[ContentHash]) -> Demand:
                    demand = (
                        Option.of_optional(prior_keys.get(place)).filter(lambda prior: prior.staged == staged and prior.content == content).default_with(lambda: Demand(staged=staged, content=content))
                    )
                    state.keys[place] = demand
                    return demand

                return (
                    Option
                    .of_optional(prior_keys.get(place))
                    .filter(lambda prior: prior.staged == staged and render.kind is not RenderKind.EFFECT_SWATCH)
                    .map(lambda prior: Ok(remembered(prior.content)))
                    .default_with(
                        lambda: (
                            paired(keyed(node, render, staged, config, nested), context)
                            .map(lambda parts: parts[0].map2(lambda key, context: content_hash(Render, key, *context, config=Some(config)), parts[1]))
                            .map(remembered)
                        )
                    )
                )

            def viewed_in(editor: Editor, space: bpy.types.SpaceNodeEditor, region: bpy.types.Region, tree: bpy.types.NodeTree) -> BlenderResult[tuple[Planned, ...]]:
                nested = tuple(node for found in enclosing(space) for node in found.to_list())

                def each(node: bpy.types.Node, render: Render, size: int, sources: dict[Handle[bpy.types.Scene], bpy.types.Scene]) -> BlenderResult[Planned]:
                    source = sources[render.target.source.scene] if isinstance(render.target.source, CompositorSource) else scene
                    return key_for(node, render, size, nested, source, render.target).map(
                        lambda demand: Planned(editor=editor, region=region, tree=tree, scene=source, node=node, render=render, size=size, demand=demand)
                    )

                def planned(rows: tuple[tuple[bpy.types.Node, Render, int], ...]) -> BlenderResult[tuple[Planned, ...]]:
                    handles = tuple(dict.fromkeys(render.target.source.scene for _, render, _ in rows if isinstance(render.target.source, CompositorSource)))
                    return resolved(handles).map(lambda scenes: dict(zip(handles, scenes))).bind(lambda sources: accumulated(each(*row, sources) for row in rows))

                return targets(identity, editor, tree, settings).bind(
                    lambda found: planned(
                        tuple(
                            (node, render, round(node.width * metrics.scale * settings.display.scale / (2 if sum(1 for held, _ in found if held == node) > 1 else 1)))
                            for node, target in found
                            for render in isolated(node, target, settings).to_list()
                        )
                    )
                )

            def scheduled(plans: tuple[Planned, ...]) -> BlenderResult[tuple[Job, ...]]:
                ranked = tuple((plan, rank(plan), plan.demand.content.default_value(plan.demand)) for plan in plans)
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
                    return (state.signal.fired() if changed else Ok(None)).map(
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
                        if key in TextureCache.held[identity.package]
                        else content.map(lambda digest: restored(digest, config, name, settings.display.limit, kept).map(lambda held: Some(key) if held else Nothing)).default_value(Ok(Nothing))
                        for key, content in {key: plan.demand.content for plan, _rank, key in ranked}.items()
                    )
                    .map(lambda parts: frozenset(key for part in parts for key in part.to_list()))
                    .bind(shown)
                )

            def stage_job(target: Target, scale: float) -> BlenderResult[tuple[Job, ...]]:
                previews, slot = Previews(identity=identity), repr(target)
                source: BlenderResult[bpy.types.Node] = (
                    held(target).bind(tree_of).bind(lambda tree: Option.of_optional(tree.nodes.get(target.node)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(tree), key=target.node),)))
                )

                def planned_stage(node: bpy.types.Node) -> BlenderResult[tuple[Job, ...]]:
                    render = Render(target=target, kind=RenderKind.EFFECT_SWATCH, valued=False)

                    def requested(demand: Demand) -> tuple[Job, ...]:
                        key = demand.content.default_value(demand)

                        def published() -> BlenderResult[None]:
                            return previews.tile(
                                slot,
                                key,
                                lambda side: self.rendered(identity, render, side, settings, scene=scene).map(
                                    lambda readback: Some(Some(readback.plane)) if state.keys.get((StageThumbnail, target)) is demand else Nothing
                                ),
                                scale,
                            ).map(lambda _: None)

                        return () if previews.current(slot, key, scale) else (Job(place=(StageThumbnail, target), key=Drawn.read(key, scale), rank=Rank.SHOWN, run=published),)

                    return key_for(node, render, int(scale * metrics.unit), (), scene, (StageThumbnail, target)).map(requested)

                return source.bind(planned_stage)

            shown = tuple(
                view for view in views if settings.enabled and view[0].source.kind in settings.trees and not (isinstance(view[3], bpy.types.CompositorNodeTree) and view[1].overlay.show_previews)
            )
            editors = accumulated(itertools.starmap(viewed_in, shown)).map(lambda parts: tuple(itertools.chain.from_iterable(parts))).bind(scheduled)
            stages = accumulated(
                stage_job(target, scale)
                for target, scale in state.stages.items()
                if isinstance(target.source, CompositorSource) and target.source.scene == Handle.of(scene) and target.source.entry in scene.compositor_effects
            )
            return paired(editors, stages).map(lambda parts: (*parts[0], *itertools.chain.from_iterable(parts[1])))

        scene = rooted(bpy.types.Scene, context)
        return paired(paired(scene, self.settings(identity)), paired(self.stack.config(), identity.name(Name.ATTACHMENT, PreviewScene.value))).bind(lambda parts: planned(*parts[0], *parts[1]))

    def section(self, header: tuple[Row, ...] = ()) -> Single:
        """Returns node previews section."""
        return Single(
            key="node_previews",
            title="Node Previews",
            header=header,
            rows=(
                *(OperatorButton(target=row) for row in (self.toggle, self.refresh, self.clear)),
                OperatorButton(target=self.export),
                Counts(),
                Single(
                    key="node_previews_active",
                    title="Active Node",
                    store=Some(Hosted(host=Member(root=bpy.types.SpaceNodeEditor, path="edit_tree.nodes.active"), store=Store(attachment=NodePreview.setting, retention=self.retention))),
                ),
            ),
        )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns preview declarations, resources, overlay, and handlers."""
        signal = Signal()
        stores = tuple(
            kind
            for prop in bpy.types.BlendData.bl_rna.properties
            if isinstance(prop, bpy.types.CollectionProperty)
            for kind in (type(prop.fixed_type),)
            if issubclass(kind, bpy.types.ID) and kind not in {bpy.types.ID, bpy.types.Object}  # type: ignore[unreachable]
        )

        @contextmanager
        def scoped() -> Generator[BlenderResult[None]]:
            NodePreview.state[identity.package] = NodeState(signal=signal)
            try:
                yield Result[None, BlenderFaults].Ok(None)
            finally:
                del NodePreview.state[identity.package]

        def forgotten(switch: Callable[[NodePreviews], bool]) -> BlenderResult[None]:
            state = NodePreview.state[identity.package]
            read = self.settings(identity)
            for _ in read.to_option().filter(switch).to_list():
                state.keys.clear()
            return read.map(lambda _: None)

        def dropped(_scene: bpy.types.Scene, updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> BlenderResult[None]:
            for block in (held for update in updates for held in Option.of_optional(update.id).to_list()):
                NodePreview.state[identity.package].held.pop(Handle.of(block), None)
            return forgotten(lambda settings: settings.automatic)

        def cleared(_path: Option[Path]) -> BlenderResult[None]:
            NodePreview.state[identity.package] = NodeState(signal=signal)
            return signal.fired()

        overlay = Overlay(
            drawing=Drawing(area=AreaQuery(space="NODE_EDITOR"), part=NodeThumbnails(identity=identity, settings=self.settings), refreshes=(signal,)),
            space=bpy.types.SpaceNodeEditor,
            draw_type="POST_VIEW",
            scissor_enabled=False,
        )
        return Ok((
            self.tiles,
            self.setting,
            Keymap(keymap="Node Editor", bindings={Chord(type="P", ctrl=1, alt=1): self.toggle, Chord(type="Z", ctrl=1, alt=1): self.enlarge, Chord(type="R", ctrl=1, alt=1): self.refresh}),
            self.clay,
            self.swatch,
            (Band.RESOURCES, scoped()),
            (Band.RESOURCES, TextureCache(identity=identity)),
            (Band.RESOURCES, PreviewQueue.fed(identity, self.jobs)),
            self.toggle,
            self.enlarge,
            self.refresh,
            self.clear,
            self.mark,
            self.export,
            Append(target=NODE_MT_context_menu, rows=(OperatorButton(target=self.mark), OperatorButton(target=self.export), OperatorButton(target=self.refresh))),
            overlay,
            UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=stores, body=dropped),  # type: ignore[arg-type]
            UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Object,), body=dropped, changes=Some(Change.GEOMETRY | Change.SHADING)),
            SceneHandler(handlers=bpy.app.handlers.frame_change_post, body=lambda _: forgotten(lambda settings: settings.frames)),
            PathHandler(handlers=bpy.app.handlers.load_post, body=cleared),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        ))


class StageThumbnail(Line, frozen=True, kw_only=True):
    """Inline preview of effect entry stage output."""

    preview: NodePreview

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns preview producer owning inline request queue."""
        return Ok((self.preview,))

    def drawn(self, identity: Identity, /) -> BlenderResult[RowDraw]:
        """Records Group Output target and draws its published icon."""

        def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
            if isinstance(holder, bpy.types.SceneCompositorEffect) and isinstance(holder.id_data, bpy.types.Scene) and isinstance(holder.node_group, bpy.types.NodeTree):
                source = CompositorSource(scene=Handle.of(holder.id_data), entry=holder.name)

                def requested(node: bpy.types.Node) -> None:
                    stages = {
                        Target(source=source, groups=(), node=node.name, socket=socket.identifier): region.width / Metrics.read().unit
                        for socket, region in itertools.product(tuple(node.inputs)[:1], Option.of_optional(context.region).to_list())
                    }
                    NodePreview.state[identity.package].stages.update(stages)
                    for icon, scale in ((icon, scale) for target, scale in stages.items() for icon in Previews(identity=identity).icon_id(repr(target)).to_list()):
                        layout.template_icon(icon_value=icon, scale=scale)

                alerted(layout, active_output(holder.node_group).map(requested))

        return Ok(shown)


# --- [OPERATIONS] -----------------------------------------------------------------------


def targets(identity: Identity, editor: Editor, tree: bpy.types.NodeTree, settings: NodePreviews) -> BlenderResult[tuple[tuple[bpy.types.Node, Target], ...]]:
    """Returns in-scope tree nodes with targets their outputs and setting choose."""
    zones = zoned(tree)

    def chosen(node: bpy.types.Node, setting: NodeSetting) -> tuple[str, ...]:
        eligible, inputs = outputs_of(node), node.inputs
        linked = tuple(socket.identifier for socket in eligible if socket.is_linked)
        picked = tuple(socket.identifier for socket in eligible if setting.output.enabled and socket.identifier == setting.output.value)
        match node:
            case bpy.types.ShaderNodeOutputMaterial() | bpy.types.ShaderNodeOutputWorld() | bpy.types.ShaderNodeOutputLight() | bpy.types.NodeGroupOutput():
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


def isolated(node: bpy.types.Node, target: Target, settings: NodePreviews) -> Option[Render]:
    """Returns isolated render of a target socket."""
    inputs, outputs = node.inputs, node.outputs

    def kind(socket: bpy.types.NodeSocket) -> Option[RenderKind]:
        match target.source:
            case MaterialSource():
                return Some(RenderKind.MATERIAL_SWATCH)
            case LightSource():
                return Some(RenderKind.LIGHT_SWATCH)
            case WorldSource():
                return Some(RenderKind.WORLD_VOLUME if socket.identifier == "Volume" else RenderKind.WORLD_PANORAMA)
            case GeometrySource():
                return Some(RenderKind.GEOMETRY_CLAY) if socket.type == "GEOMETRY" else Some(RenderKind.FIELD_SWATCH) if settings.fields and isinstance(node, bpy.types.ShaderNode) else Nothing
            case CompositorSource():
                return Some(RenderKind.EFFECT_SWATCH)

    return (
        socket_at(inputs if isinstance(node, OutputNode) else outputs, target.socket)
        .to_option()
        .bind(lambda socket: kind(socket).map(lambda found: (socket, found)))
        .map(
            lambda pair: Render(
                target=target, kind=pair[1], valued=pair[0].type in {"VALUE", "INT", "BOOLEAN"}, shape=Some(settings.lighting.shape) if pair[1] is RenderKind.MATERIAL_SWATCH else Nothing
            )
        )
    )


def sourced(space: bpy.types.SpaceNodeEditor, root: bpy.types.NodeTree) -> Option[Source]:
    """Returns data-block a node editor shows, with modifier for geometry groups and entry for effect groups."""
    match space.id, space.id_from, root:
        case bpy.types.Material() as material, _, bpy.types.ShaderNodeTree():
            return Some(MaterialSource(material=Handle.of(material)))
        case bpy.types.Light() as light, _, bpy.types.ShaderNodeTree():
            return Some(LightSource(light=Handle.of(light)))
        case bpy.types.World() as world, _, bpy.types.ShaderNodeTree():
            return Some(WorldSource(world=Handle.of(world)))
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
        case MaterialSource(material=handle):
            return handle.resolve()
        case LightSource(light=handle):
            return handle.resolve()
        case WorldSource(world=handle):
            return handle.resolve()
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
    """Scopes preview copies, exposing target output or keeping a shader output node whole.

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
            case bpy.types.ShaderNodeOutputMaterial() | bpy.types.ShaderNodeOutputWorld() | bpy.types.ShaderNodeOutputLight():
                return socket_at(inputs, target.socket)
            case bpy.types.NodeGroupOutput():
                return socket_at(inputs, target.socket).bind(
                    lambda socket: Option.of_optional(next((link.from_socket for link in socket.links), None)).to_result_with(  # type: ignore[union-attr]  # ty: ignore[not-iterable]
                        lambda: (BlenderAbsentFault(owner=repr(socket.links), key=target.socket),)
                    )
                )
            case _:
                return socket_at(node.outputs, target.socket)

    with ExitStack() as rollback:

        def copied[T: Holder](block: T) -> BlenderResult[T]:
            def acquired(copy: T) -> T:
                rollback.callback(bpy.data.batch_remove, ids=(copy,))
                return copy

            return Ownership(kind=type(block), key=PreviewScene.routes.key, saved=Saved.NEVER, single=False).made(identity, lambda: Ok(block.copy())).map(acquired)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

        def exposed(inner: bpy.types.NodeTree, source: bpy.types.NodeSocket) -> BlenderResult[str]:
            return (
                active_output(inner)
                .bind(lambda output: socket_at(output.inputs, "__extend__"))
                .bind(lambda extend: joined(inner, source, extend, handle_dynamic_sockets=True))
                .map(lambda link: link.to_socket.identifier)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            )

        def swapped(node: bpy.types.Node) -> BlenderResult[bpy.types.NodeTree]:
            group: BlenderResult[bpy.types.NodeTree] = (Option.of_optional(node.node_tree) if isinstance(node, GroupNode) else Nothing).to_result_with(
                lambda: (BlenderAbsentFault(owner=repr(node), key=bpy.types.NodeTree.__name__),)
            )
            return Result[bpy.types.NodeTree, BlenderFaults].bind(group, lambda held: copied(held).bind(lambda inner: written(node, "node_tree", inner).map(lambda _: inner)))

        def outward(tree: bpy.types.NodeTree, groups: tuple[str, ...]) -> BlenderResult[bpy.types.NodeSocket]:
            match groups:
                case (head, *rest):
                    return found(tree, head).bind(
                        lambda node: (
                            swapped(node).bind(lambda inner: outward(inner, tuple(rest)).bind(lambda source: exposed(inner, source))).bind(lambda identifier: socket_at(node.outputs, identifier))
                        )
                    )
                case _:
                    return found(tree, target.node).bind(endpoint)

        yield copied(holder).bind(lambda copy: tree_of(copy).bind(lambda tree: outward(tree, target.groups)).map(lambda socket: Routed(holder=copy, socket=socket)))


def outputs_of(node: bpy.types.Node) -> tuple[bpy.types.NodeSocket, ...]:
    """Returns shown outputs with a socket type the tree previews."""
    shown = tuple(socket for socket in node.outputs if socket.enabled and not socket.hide and not socket.is_unavailable)
    fields = frozenset({"RGBA", "VECTOR", "VALUE"})
    match node.id_data:
        case bpy.types.ShaderNodeTree():
            return tuple(socket for socket in shown if socket.type in fields | {"SHADER"})
        case bpy.types.GeometryNodeTree():
            return tuple(socket for socket in shown if socket.type == "GEOMETRY" or (isinstance(node, bpy.types.ShaderNode) and socket.type in fields))
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
        for end in (node.path_resolve("paired_output"),)
        if isinstance(end, bpy.types.Node)
        for name in inside(node.name, end.name)
    )


def outlet(tree: bpy.types.NodeTree, socket: bpy.types.NodeSocket, output: type[bpy.types.Node], aov: Option[str]) -> BlenderResult[None]:
    """Adds output, Emission or Background, and value AOV nodes a routed socket drives."""
    emitter, emitted = (bpy.types.ShaderNodeBackground, "Background") if output is bpy.types.ShaderNodeOutputWorld else (bpy.types.ShaderNodeEmission, "Emission")

    def surfaced(sink: bpy.types.Node) -> BlenderResult[None]:
        match socket.type:
            case "SHADER":
                return socket_at(sink.inputs, socket.identifier).or_else(socket_at(sink.inputs, "Surface")).bind(lambda target: joined(tree, socket, target)).map(lambda _: None)
            case _:
                return (
                    created(tree, emitter)
                    .bind(
                        lambda node: accumulated((
                            socket_at(node.inputs, "Color").bind(lambda target: joined(tree, socket, target)),  # ty: ignore[invalid-argument-type]
                            paired(socket_at(node.outputs, emitted), socket_at(sink.inputs, "Surface")).bind(lambda pair: joined(tree, *pair)),  # ty: ignore[invalid-argument-type]
                        ))
                    )
                    .map(lambda _: None)
                )

    return paired(
        created(tree, output).bind(lambda sink: accumulated((surfaced(sink), written(sink, "target", "ALL"), written(sink, "is_active_output", value=True)))),
        accumulated(
            created(tree, bpy.types.ShaderNodeOutputAOV).bind(
                lambda node: accumulated((socket_at(node.inputs, "Value").bind(lambda target: joined(tree, socket, target)), written(node, "aov_name", name)))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            )
            for name in aov.to_list()
        ),
    ).map(lambda _: None)


def refreshed(identity: Identity) -> BlenderResult[None]:
    """Drops target keys and held failures for a full rerender."""
    state = NodePreview.state[identity.package]
    PreviewQueue.forced(identity, tuple(state.keys))
    state.keys.clear()
    return state.signal.fired()


def cleared(identity: Identity) -> BlenderResult[int]:
    """Empties texture cache, held values, and previews folder, then refreshes every target."""
    files = tuple((identity.folder / "previews").glob("*.exr"))
    TextureCache.held[identity.package].clear()
    NodePreview.state[identity.package].values.clear()
    return accumulated(removed(path) for path in files).bind(lambda _: refreshed(identity)).map(lambda _: len(files))


def value(values: npt.NDArray[np.float32]) -> Option[float]:
    """Returns mean of a field within a thousandth of its magnitude, `Nothing` when varying or empty."""
    return Option.of_optional(values if values.size else None).filter(lambda held: float(np.ptp(held)) <= 1e-3 * max(1.0, float(np.abs(held).max()))).map(lambda held: float(held.mean()))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Cell",
    "CompositorSource",
    "Counts",
    "Display",
    "Editor",
    "Export",
    "GeometrySource",
    "LightSource",
    "MaterialSource",
    "NodePreview",
    "NodePreviews",
    "NodeSetting",
    "NodeState",
    "NodeThumbnails",
    "Planned",
    "Position",
    "PreviewScope",
    "Render",
    "RenderKind",
    "Shown",
    "Source",
    "StageThumbnail",
    "Staged",
    "Status",
    "Target",
    "TreeKind",
    "WorldSource",
    "outlet",
    "outputs",
    "outputs_of",
    "value",
    "zoned",
]
