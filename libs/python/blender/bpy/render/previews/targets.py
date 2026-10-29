"""Node preview settings and target selection: the node outputs a geometry node editor previews and the isolated render each takes."""

from enum import auto, Enum
from functools import partial, reduce
import itertools
from typing import Final, Self

import bpy
from expression import Nothing, Option, Some
from expression.collections import Block
import msgspec

from blender.bpy.data.ids import content
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.mirrors import case_choices, choices
from blender.bpy.data.rna.paths import struct_at
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.trees import GROUP_NODES, Holder, OutputNode, socket_at
from blender.bpy.render.previews.fixtures import Ambient, KEY_LIGHT, PreviewLighting, PreviewMesh, STUDIO, Studio, SURFACE, SURFACES, WORLD_LIGHT
from blender.bpy.settings.attachments import Attachment
from blender.records.faults import Fault, Outcome

# --- [TYPES] ----------------------------------------------------------------------------


class PreviewScope(Enum):
    """Nodes that take a thumbnail."""

    ALL = auto()
    SELECTED = auto()
    MARKED = auto()


class Position(Enum):
    """Side of a node its thumbnails sit on."""

    ABOVE = auto()
    BELOW = auto()
    LEFT = auto()
    RIGHT = auto()


class RenderKind(Enum):
    """Isolated render a node output takes."""

    FIELD_SWATCH = auto()
    GEOMETRY_CLAY = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

SCOPES, POSITIONS = (choices((member.name, bpy.path.display_name(member.name.lower()), "") for member in kind) for kind in (PreviewScope, Position))
_VALUED: Final = frozenset({"VALUE", "INT", "BOOLEAN"})
_AMBIENTS: Final = Ambient.__value__

# --- [MODELS] ---------------------------------------------------------------------------


class NodePreviews(bpy.types.PropertyGroup):
    """Node preview settings of extension preferences, the previews rendering under their lighting and drawing by their thumbnail members."""

    enabled: bpy.props.BoolProperty(name="Show Previews", description="Thumbnails over node editor nodes", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    automatic: bpy.props.BoolProperty(name="Auto Update", description="Changed nodes render again", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    scope: bpy.props.EnumProperty(name="Scope", description="Nodes that take a thumbnail", items=SCOPES)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    every: bpy.props.BoolProperty(name="All Linked Outputs", description="Every linked output side by side")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    frames: bpy.props.BoolProperty(name="Follow Frame", description="Render again on frame change and during playback")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    fields: bpy.props.BoolProperty(name="Field Nodes", description="Shader nodes of geometry trees as swatches", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    surface: bpy.props.EnumProperty(name="Surface", description="Shader preview surface", items=SURFACES, default=SURFACE.name)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    ambient: bpy.props.EnumProperty(name="Ambient", description="Shader preview ambient light", items=case_choices(Ambient), default=Studio.__name__)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    world: bpy.props.StringProperty(name="World", description="Studio light environment", default=STUDIO)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    strength: bpy.props.FloatProperty(name="World Light", description="Ambient light strength", min=0.0, max=10.0, default=WORLD_LIGHT)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    key: bpy.props.FloatProperty(name="Key Light", description="Key sun strength", min=0.0, max=20.0, default=KEY_LIGHT)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    scale: bpy.props.FloatProperty(name="Thumbnail Size", description="Share of node width", subtype="FACTOR", min=0.25, max=3.0, default=1.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    position: bpy.props.EnumProperty(name="Position", description="Node side holding thumbnail", items=POSITIONS)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    enlarged: bpy.props.BoolProperty(name="Enlarge Active Node", description="Active node thumbnail larger on top")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    enlarge: bpy.props.FloatProperty(name="Enlarge", description="Active node scale while enlarged", min=1.25, max=6.0, default=2.5)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    checker: bpy.props.BoolProperty(name="Checkerboard", description="Theme checker behind thumbnails")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    status: bpy.props.BoolProperty(name="Status Marks", description="Queued and failed marks on thumbnails", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    values: bpy.props.BoolProperty(name="Show Values", description="Number on uniform swatches", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    limit: bpy.props.IntProperty(name="Cached Thumbnails", description="Thumbnails kept in GPU memory and on disk", min=16, max=4096, default=256)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @property
    def lighting(self) -> PreviewLighting:
        """Shader preview lighting."""
        ambient = msgspec.convert({"kind": self.ambient, "name": self.world}, _AMBIENTS)
        return PreviewLighting(surface=PreviewMesh[self.surface], ambient=ambient, strength=self.strength, key=self.key)


class NodeSetting(bpy.types.PropertyGroup):
    """Node thumbnail settings."""

    use_output: bpy.props.BoolProperty(name="Pick Output", description="Show the named output, first linked when off")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    output: bpy.props.StringProperty(name="Output", description="Name of the shown output")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Editor(msgspec.Struct, frozen=True, kw_only=True):
    """Object and modifier running the geometry group one node editor shows, and the group path entered from it, shader and compositor trees drawing their host node previews."""

    item: Handle[bpy.types.Object]
    modifier: str
    groups: tuple[str, ...]

    @classmethod
    def of(cls, space: bpy.types.SpaceNodeEditor) -> Option[Self]:
        """Reads the active modifier running the shown geometry group and the group path, `Nothing` outside previewed geometry groups."""
        nested = enclosing(space)
        groups = tuple(node.name for found in nested for node in found.to_list())
        match space.id_from, space.node_tree:
            case bpy.types.Object() as item, bpy.types.GeometryNodeTree() as root if len(groups) == len(nested):
                using = Block.of_seq(item.modifiers).filter(lambda modifier: isinstance(modifier, bpy.types.NodesModifier) and modifier.node_group == root)
                chosen = using.filter(lambda modifier: modifier.is_active).try_head().or_else_with(using.try_head)
                return chosen.map(lambda modifier: cls(item=Handle.of(item), modifier=modifier.name, groups=groups))
            case _:
                return Nothing

    def tree(self, root: bpy.types.NodeTree) -> Option[bpy.types.NodeTree]:
        """Returns tree group path reaches from root."""
        return reduce(
            lambda held, name: held.bind(lambda tree: Option.of_optional(tree.nodes.get(name))).bind(
                lambda node: struct_at(node, ("node_tree",), bpy.types.NodeTree) if isinstance(node, GROUP_NODES) else Nothing
            ),
            self.groups,
            Some(root),
        )

    def held(self) -> Outcome[Holder]:
        """Resolves the group of the editor's modifier."""
        return self.item.resolve().bind(
            lambda item: (
                Option
                .of_optional(item.modifiers.get(self.modifier))
                .bind(lambda found: Option.of_optional(found.node_group) if isinstance(found, bpy.types.NodesModifier) else Nothing)
                .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(item.modifiers), member=self.modifier))
            )
        )


class Target(msgspec.Struct, frozen=True, kw_only=True):
    """Node output a job renders and a cell draws, with the isolated render it takes and whether a uniform value reads back."""

    editor: Editor
    node: str
    socket: str
    kind: RenderKind
    valued: bool


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [TARGETS]


def targets(identity: Identity, editor: Editor, tree: bpy.types.NodeTree, settings: NodePreviews) -> Outcome[tuple[tuple[bpy.types.Node, Target], ...]]:
    """Returns in-scope tree nodes with the targets their outputs and setting choose, each output an isolated render takes."""
    zones = zoned(tree)

    def chosen(node: bpy.types.Node, setting: NodeSetting) -> tuple[str, ...]:
        eligible = outputs_of(node)
        linked = tuple(socket.identifier for socket in eligible if socket.is_linked)
        picked = tuple(socket.identifier for socket in eligible if setting.use_output and socket.name == setting.output)
        return linked if settings.every else (picked or linked or tuple(socket.identifier for socket in eligible))[:1]

    def shown(node: bpy.types.Node) -> bool:
        match PreviewScope[settings.scope]:
            case PreviewScope.ALL:
                return True
            case PreviewScope.SELECTED:
                return node.select
            case PreviewScope.MARKED:
                return node.show_preview

    def isolated(node: bpy.types.Node, identifier: str) -> Option[Target]:
        def kind(socket: bpy.types.NodeSocket) -> Option[RenderKind]:
            return Some(RenderKind.GEOMETRY_CLAY) if socket.type == "GEOMETRY" else Some(RenderKind.FIELD_SWATCH) if settings.fields and isinstance(node, bpy.types.ShaderNode) else Nothing

        return (
            socket_at(node.inputs if isinstance(node, OutputNode.__value__) else node.outputs, identifier)
            .to_option()
            .bind(lambda socket: kind(socket).map(lambda found: Target(editor=editor, node=node.name, socket=identifier, kind=found, valued=socket.type in _VALUED)))
        )

    candidates = tuple(
        node for node in tree.nodes if not (isinstance(node, bpy.types.NodeFrame | bpy.types.NodeReroute | bpy.types.NodeGroupInput | bpy.types.NodeGroupOutput) or node.mute or node.name in zones)
    )
    return SETTING.bound(identity).map(
        lambda attached: tuple(
            (node, target) for node in candidates for setting in (attached.struct(node),) if shown(node) for identifier in chosen(node, setting) for target in isolated(node, identifier).to_list()
        )
    )


def enclosing(space: bpy.types.SpaceNodeEditor) -> tuple[Option[bpy.types.Node], ...]:
    """Returns group node entering each tree down editor path, parent active node first."""
    trees = tuple(item.node_tree for item in space.path)
    return tuple(
        Block
        .of_seq((parent.nodes.active, *parent.nodes))
        .choose(Option.of_optional)
        .filter(lambda node: isinstance(node, GROUP_NODES) and struct_at(node, ("node_tree",), bpy.types.NodeTree) == Some(child))
        .try_head()
        if parent is not None
        else Nothing
        for parent, child in itertools.pairwise(trees)
    )


def outputs_of(node: bpy.types.Node) -> tuple[bpy.types.NodeSocket, ...]:
    """Returns shown outputs of a geometry tree node with a socket type the tree previews."""
    shown = tuple(socket for socket in node.outputs if socket.enabled and not socket.hide)
    return (
        tuple(socket for socket in shown if socket.type == "GEOMETRY" or (isinstance(node, bpy.types.ShaderNode) and socket.type in {"RGBA", "VECTOR", "VALUE"}))
        if isinstance(node.id_data, bpy.types.GeometryNodeTree)
        else ()
    )


def zoned(tree: bpy.types.NodeTree) -> frozenset[str]:
    """Returns names of nodes inside paired zones, reached forward from a zone input before its output over valid unmuted links."""
    following: dict[str, set[str]] = {}
    for link in tree.links:
        if link.is_valid and not link.is_muted:
            source, _, target, _ = content.endpoints(link)
            following.setdefault(source.name, set()).add(target.name)

    def inside(start: str, end: str) -> frozenset[str]:
        def walked(frontier: frozenset[str], seen: frozenset[str]) -> frozenset[str]:
            fresh = frozenset(target for name in frontier - {end} for target in following.get(name, ())) - seen
            return walked(fresh, seen | fresh) if fresh else seen

        return walked(frozenset({start}), frozenset({start}))

    return frozenset(name for node in tree.nodes for end in struct_at(node, ("paired_output",), bpy.types.Node).to_list() for name in inside(node.name, end.name))


# --- [COMPOSITION] ----------------------------------------------------------------------

SETTING: Final = Attachment(key="node_preview", form=NodeSetting, owner=bpy.types.Node)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["SETTING", "Editor", "NodePreviews", "NodeSetting", "Position", "PreviewScope", "RenderKind", "Target", "enclosing", "outputs_of", "targets", "zoned"]
