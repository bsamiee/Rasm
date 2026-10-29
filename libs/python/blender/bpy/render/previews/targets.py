"""Node preview settings, target selection, and isolated renders: the node outputs a geometry node editor previews, the routed copies exposing them, and the clay and swatch renders each takes in the preview scene."""

from contextlib import ExitStack
from enum import auto, Enum
from functools import partial, reduce
import itertools
from typing import Final, Self

import bpy
from expression import Nothing, Ok, Option, Result, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
from mathutils import Vector
import msgspec
import PyOpenColorIO

from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.data.rna.mirrors import case_choices, enum_choices
from blender.bpy.data.rna.paths import struct_at
from blender.bpy.extension.identity import Identity
from blender.bpy.nodes import functions
from blender.bpy.nodes.changes import reconciled
from blender.bpy.nodes.desired import Graph, Members
from blender.bpy.nodes.graphs import Expr
from blender.bpy.nodes.trees import active_output, created, endpoints, group_tree, Holder, joined, OUTPUT_NODES, socket_at, tree_of
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.outputs.results import intermediate
from blender.bpy.render.previews.scenes import Ambient, Fitted, PreviewLighting, PreviewMesh, PreviewScene, Readback, Staging, Studio, SURFACES
from blender.bpy.scene.materials import Clay, shaded
from blender.bpy.settings.attachments import Attachment
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason

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

CLAY_AIM: Final = Vector((0.3, -0.6, 0.9)).normalized().to_track_quat("Z", "Y").to_euler()
_VALUED: Final = frozenset({"VALUE", "INT", "BOOLEAN"})
_LIGHTING: Final = PreviewLighting()

# --- [MODELS] ---------------------------------------------------------------------------


class NodePreviews(bpy.types.PropertyGroup):
    """Node preview settings of extension preferences, the previews rendering under their lighting and drawing by their thumbnail members."""

    enabled: bpy.props.BoolProperty(name="Show Previews", description="Thumbnails over node editor nodes", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    automatic: bpy.props.BoolProperty(name="Auto Update", description="Changed nodes render again", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    scope: bpy.props.EnumProperty(name="Scope", description="Nodes that take a thumbnail", items=enum_choices(PreviewScope))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    every: bpy.props.BoolProperty(name="All Linked Outputs", description="Every linked output side by side")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    frames: bpy.props.BoolProperty(name="Follow Frame", description="Render again on frame change and during playback")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    fields: bpy.props.BoolProperty(name="Field Nodes", description="Shader nodes of geometry trees as swatches", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    surface: bpy.props.EnumProperty(name="Surface", description="Shader preview surface", items=SURFACES, default=_LIGHTING.surface.name)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    ambient: bpy.props.EnumProperty(name="Ambient", description="Shader preview ambient light", items=case_choices(Ambient), default=Studio.__name__)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    world: bpy.props.StringProperty(name="World", description="Studio light environment", default=Studio().name)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    strength: bpy.props.FloatProperty(name="World Light", description="Ambient light strength", min=0.0, max=10.0, default=_LIGHTING.strength)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    key: bpy.props.FloatProperty(name="Key Light", description="Key sun strength", min=0.0, max=20.0, default=_LIGHTING.key)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    scale: bpy.props.FloatProperty(name="Thumbnail Size", description="Share of node width", subtype="FACTOR", min=0.25, max=3.0, default=1.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    position: bpy.props.EnumProperty(name="Position", description="Node side holding thumbnail", items=enum_choices(Position))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    enlarged: bpy.props.BoolProperty(name="Enlarge Active Node", description="Active node thumbnail larger on top")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    enlarge: bpy.props.FloatProperty(name="Enlarge", description="Active node scale while enlarged", min=1.25, max=6.0, default=2.5)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    checker: bpy.props.BoolProperty(name="Checkerboard", description="Theme checker behind thumbnails")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    status: bpy.props.BoolProperty(name="Status Marks", description="Queued and failed marks on thumbnails", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    values: bpy.props.BoolProperty(name="Show Values", description="Number on uniform swatches", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    limit: bpy.props.IntProperty(name="Cached Thumbnails", description="Thumbnails kept in GPU memory and on disk", min=16, max=4096, default=256)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @property
    def lighting(self) -> PreviewLighting:
        """Shader preview lighting."""
        ambient = msgspec.convert({"kind": self.ambient, "name": self.world}, Ambient.__value__)
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
        return reduce(lambda held, name: held.bind(lambda tree: Option.of_optional(tree.nodes.get(name))).bind(group_tree), self.groups, Some(root))

    def held(self) -> Outcome[Holder]:
        """Resolves the group of the editor's modifier."""
        return self.item.resolve().bind(
            lambda item: (
                Option
                .of_optional(item.modifiers.get(self.modifier))
                .bind(lambda found: Option.of_optional(found.node_group) if isinstance(found, bpy.types.NodesModifier) else Nothing)
                .to_result_with(partial(Fault.of, Reason.ABSENT, repr(item.modifiers), member=self.modifier))
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
            socket_at(node.inputs if isinstance(node, OUTPUT_NODES) else node.outputs, identifier)
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
        Block.of_seq((parent.nodes.active, *parent.nodes)).choose(Option.of_optional).filter(lambda node: group_tree(node) == Some(child)).try_head() if parent is not None else Nothing
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
    links = tuple((source.name, target.name) for source, _, target, _ in (endpoints(link) for link in tree.links if link.is_valid and not link.is_muted))
    following = {name: frozenset(target for held, target in links if held == name) for name, _ in links}

    def inside(start: str, end: str) -> frozenset[str]:
        def walked(frontier: frozenset[str], seen: frozenset[str]) -> frozenset[str]:
            fresh = frozenset(target for name in frontier - {end} for target in following.get(name, ())) - seen
            return walked(fresh, seen | fresh) if fresh else seen

        return walked(frozenset({start}), frozenset({start}))

    return frozenset(name for node in tree.nodes for end in struct_at(node, ("paired_output",), bpy.types.Node).to_list() for name in inside(node.name, end.name))


# --- [ROUTES]


def copied[T: bpy.types.ID](identity: Identity, block: T, copies: ExitStack) -> Outcome[T]:
    """Copies a block under the preview route ownership for the copies' scope."""
    refused = partial(Fault.of, Reason.REFUSED, repr(block))
    made = PreviewScene.routes.made(
        identity, lambda: catch(block.copy, exception=RuntimeError)().map_error(refused).bind(lambda copy: Option.of_optional(copy).to_result_with(refused)), scope=Some(copies)
    )
    return made.bind(lambda copy: Option.of_optional(try_downcast(type(block), copy)).to_result_with(refused))


def routed[H: Holder](identity: Identity, holder: H, target: Target, copies: ExitStack) -> Outcome[tuple[H, bpy.types.NodeSocket]]:
    """Copies the holder and every group down the target's editor path for the copies' scope, exposing the target output on the copied holder's tree.

    Args:
        identity: Extension whose marks the copies hold.
        holder: Tree holder nesting target node.
        target: Route of copies.
        copies: Scope removing the copies.

    Returns:
        Copied holder and the socket carrying the target output.
    """

    def found(tree: bpy.types.NodeTree, name: str) -> Outcome[bpy.types.Node]:
        return Option.of_optional(tree.nodes.get(name)).to_result_with(partial(Fault.of, Reason.ABSENT, repr(tree.nodes), member=name))

    def exposed(inner: bpy.types.NodeTree, source: bpy.types.NodeSocket) -> Outcome[str]:
        return (
            active_output(inner)
            .bind(lambda output: socket_at(output.inputs, "__extend__"))
            .bind(lambda extend: joined(inner, source, extend, handle_dynamic_sockets=True))
            .map(lambda link: endpoints(link)[3].identifier)
        )

    def entered(node: bpy.types.Node) -> Outcome[bpy.types.NodeTree]:
        group = group_tree(node).to_result_with(partial(Fault.of, Reason.ABSENT, repr(node), member=bpy.types.NodeTree.__name__))
        return Result[bpy.types.NodeTree, Faults].bind(group, lambda inner: copied(identity, inner, copies)).bind(lambda inner: written(node, ("node_tree",), inner).map(lambda _: inner))

    def outward(tree: bpy.types.NodeTree, groups: tuple[str, ...]) -> Outcome[bpy.types.NodeSocket]:
        match groups:
            case (head, *rest):
                return found(tree, head).bind(
                    lambda node: entered(node).bind(lambda inner: outward(inner, tuple(rest)).bind(lambda source: exposed(inner, source))).bind(lambda identifier: socket_at(node.outputs, identifier))
                )
            case _:
                return found(tree, target.node).bind(lambda node: socket_at(node.outputs, target.socket))

    return copied(identity, holder, copies).bind(lambda copy: tree_of(copy).bind(lambda tree: outward(tree, target.editor.groups)).map(lambda socket: (copy, socket)))


# --- [RENDERS]


def rendered(identity: Identity, preview: PreviewScene, target: Target, size: int, settings: NodePreviews, *, scene: bpy.types.Scene) -> Outcome[Readback]:
    """Renders a target through routed copies in the preview scene, staged per render kind and formed through the scene view, the source's updates during the render marked as its own."""
    lighting, editor = settings.lighting, target.editor
    aov: Outcome[Option[str]] = identity.identifier(PreviewScene.value).map(Some) if target.valued else Ok(Nothing)

    def staged(lit: PreviewLighting, **changes: object) -> Outcome[Staging]:
        return lit.environment.map(lambda world: msgspec.structs.replace(Staging(source=scene, size=(size, size), world=world, lamp=lit.lamp), **changes))

    def swatched() -> Outcome[bpy.types.Material]:
        def defaulted(socket: bpy.types.NodeSocket) -> Option[Value]:
            found: bpy.types.Property | None = socket.bl_rna.properties.get("default_value")
            return Option.of_optional(found).map(lambda prop: Descriptor(root=type(socket), prop=prop).read(socket))

        def graph(source: bpy.types.Node, name: Option[str]) -> Outcome[Graph]:
            own: Members = {member.path: member.read(source) for member in Descriptor.saved(source, lambda prop: not prop.is_readonly and prop.identifier not in bpy.types.Node.bl_rna.properties)}
            coordinates = Expr(kind=bpy.types.ShaderNodeTexCoord, socket="Generated", shape=bpy.types.NodeSocketVector, label=Some("coordinates"))
            inputs = tuple((item.identifier, coordinates if item.type == "VECTOR" else held) for item in source.inputs if not item.is_linked for held in defaulted(item).to_list())
            field = Expr(kind=type(source), inputs=inputs, members=own, socket=target.socket, label=Some("field"))
            emission = Expr(kind=bpy.types.ShaderNodeEmission, inputs=(("Color", field),), socket="Emission", label=Some("emission"))
            roots = (
                Expr(kind=bpy.types.ShaderNodeOutputMaterial, inputs=(("Surface", emission),), members={"target": "ALL"}, label=Some("output")),
                *(Expr(kind=bpy.types.ShaderNodeOutputAOV, inputs=(("Value", field),), members={"aov_name": held}, label=Some("aov")) for held in name.to_list()),
            )
            return functions.lowered(bpy.types.ShaderNodeTree, *roots)

        found = (
            editor
            .held()
            .map(lambda root: editor.tree(root) if isinstance(root, bpy.types.NodeTree) else Nothing)
            .bind(lambda tree: tree.bind(lambda inner: Option.of_optional(inner.nodes.get(target.node))).to_result_with(partial(Fault.of, Reason.ABSENT, repr(editor), member=target.node)))
        )
        material = SWATCH.made(identity, lambda: catch(bpy.data.materials.new, exception=RuntimeError)("Swatch").map_error(partial(Fault.of, Reason.REFUSED, SWATCH.key)))
        return paired(found, aov, material).bind(lambda parts: paired(graph(parts[0], parts[1]), tree_of(parts[2])).bind(lambda pair: reconciled(*pair, identity)).map(lambda _: parts[2]))

    def clayed(copies: ExitStack) -> Outcome[bpy.types.Object]:
        def clothed(group: bpy.types.NodeTree, carried: bpy.types.NodeSocket, material: bpy.types.Material) -> Outcome[None]:
            def set_material(output: bpy.types.NodeGroupOutput, node: bpy.types.Node) -> Outcome[None]:
                return accumulated((
                    socket_at(node.inputs, "Geometry").bind(lambda socket: joined(group, carried, socket)),
                    socket_at(node.outputs, "Geometry").bind(lambda socket: joined(group, socket, output.inputs[0])),
                    socket_at(node.inputs, "Material").bind(lambda socket: written(socket, ("default_value",), material)),
                )).map(lambda _: None)

            return paired(active_output(group), created(group, bpy.types.GeometryNodeSetMaterial)).bind(lambda pair: set_material(*pair))

        def dressed(source: bpy.types.Object, carried: tuple[Holder, bpy.types.NodeSocket], material: bpy.types.Material) -> Outcome[bpy.types.Object]:
            holder, socket = carried
            return paired(tree_of(holder), copied(identity, source, copies)).bind(
                lambda pair: accumulated((
                    written(pair[1], ("parent",), None),
                    written(pair[1], ("location",), (0.0, 0.0, 0.0)),
                    written(pair[1].modifiers[editor.modifier], ("node_group",), pair[0]),
                    *(
                        written(later, ("show_render",), value=False)
                        for later in itertools.dropwhile(lambda modifier: modifier.name != editor.modifier, pair[1].modifiers)
                        if later.name != editor.modifier
                    ),
                    clothed(pair[0], socket, material),
                )).map(lambda _: pair[1])
            )

        clay = CLAY.made(identity, lambda: catch(bpy.data.materials.new, exception=RuntimeError)("Clay").map_error(partial(Fault.of, Reason.REFUSED, CLAY.key))).bind(
            lambda material: shaded(material, Clay(), identity)
        )
        return paired(editor.item.resolve(), editor.held().bind(lambda group: routed(identity, group, target, copies)), clay).bind(lambda parts: dressed(*parts))

    with intermediate(identity, scene), ExitStack() as copies:
        match target.kind:
            case RenderKind.GEOMETRY_CLAY:
                lit = msgspec.structs.replace(lighting, strength=0.25, key=4.0, aim=(CLAY_AIM.x, CLAY_AIM.y, CLAY_AIM.z))
                staging = clayed(copies).bind(lambda subject: staged(lit, viewpoint=Fitted(subject=subject)))
            case RenderKind.FIELD_SWATCH:
                staging = swatched().bind(lambda material: staged(lighting, material=Some(material), surface=Some(lighting.surface), value=target.valued))
        return staging.bind(lambda desired: preview.rendered(identity, desired)).bind(lambda readback: readback.formed(viewed(scene), PyOpenColorIO.GetCurrentConfig()))


# --- [COMPOSITION] ----------------------------------------------------------------------

SETTING: Final = Attachment(key="node_preview", form=NodeSetting, owner=bpy.types.Node)
CLAY: Final = Ownership(kind=bpy.types.Material, key="preview_clay", saved=Saved.NEVER)
SWATCH: Final = Ownership(kind=bpy.types.Material, key="preview_swatch", saved=Saved.NEVER)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CLAY",
    "CLAY_AIM",
    "SETTING",
    "SWATCH",
    "Editor",
    "NodePreviews",
    "NodeSetting",
    "Position",
    "PreviewScope",
    "RenderKind",
    "Target",
    "copied",
    "enclosing",
    "outputs_of",
    "rendered",
    "routed",
    "targets",
    "zoned",
]
