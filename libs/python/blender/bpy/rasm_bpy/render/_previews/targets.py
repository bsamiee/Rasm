"""Node preview settings, target selection, and isolated renders: the node outputs a geometry node editor previews, the routed copies exposing them, and the clay and swatch renders each takes in the preview scene."""

from collections import defaultdict
from contextlib import ExitStack
from enum import auto, Enum
from functools import partial, reduce
import itertools
from typing import Annotated, Final, Self

import bpy
from expression import Option, Result, Some, try_downcast
from mathutils import Vector
import msgspec
import PyOpenColorIO

from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._ids.ownership import Ownership, Saved
from rasm_bpy.data._ids.store import Handle
from rasm_bpy.data._rna.descriptors import Descriptor, written
from rasm_bpy.data._rna.paths import struct_at
from rasm_bpy.data._rna.properties import case_choices, enum_choices
from rasm_bpy.nodes import functions
from rasm_bpy.nodes._desired import Graph, Members
from rasm_bpy.nodes._graphs import Expr
from rasm_bpy.nodes._trees import active_output, created, endpoints, group_tree, Holder, linked_sockets, socket_at, tree_of
from rasm_bpy.nodes.changes import reconciled
from rasm_bpy.render._color.spaces import viewed
from rasm_bpy.render._outputs.results import intermediate
from rasm_bpy.render._previews.scenes import Ambient, PreviewLighting, PreviewMesh, PreviewScene, Readback, Staging, Studio, SURFACES
from rasm_bpy.scene._materials import Clay, shaded
from rasm_bpy.settings._attachments import Attachment
from rasm_bpy.ui._elements.panels import Preference
from rasm_records import accumulated, Error, Fault, Nothing, Ok, Outcome, paired, Reason

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

    def lighting(self, requested: PreviewLighting) -> PreviewLighting:
        """Returns field lighting or the clay's fixed ambient strength and key light."""
        return msgspec.structs.replace(requested, strength=0.25, key=4.0, aim=(_CLAY_AIM.x, _CLAY_AIM.y, _CLAY_AIM.z)) if self is RenderKind.GEOMETRY_CLAY else requested


# --- [CONSTANTS] ------------------------------------------------------------------------

_CLAY_AIM: Final = Vector((0.3, -0.6, 0.9)).normalized().to_track_quat("Z", "Y").to_euler()
_VALUED: Final = frozenset({"VALUE", "INT", "BOOLEAN"})
_LIGHTING: Final = PreviewLighting()

# --- [MODELS] ---------------------------------------------------------------------------


class NodePreviews(bpy.types.PropertyGroup):
    """Node preview settings of extension preferences, the previews rendering under their lighting and drawing by their thumbnail members."""

    enabled: Annotated[bool, bpy.props.BoolProperty(name="Show Previews", description="Thumbnails over node editor nodes", default=True)]
    automatic: Annotated[bool, bpy.props.BoolProperty(name="Auto Update", description="Changed nodes render again", default=True)]
    scope: Annotated[str, bpy.props.EnumProperty(name="Scope", description="Nodes that take a thumbnail", items=enum_choices(PreviewScope))]
    every: Annotated[bool, bpy.props.BoolProperty(name="All Linked Outputs", description="Every linked output side by side")]
    frames: Annotated[bool, bpy.props.BoolProperty(name="Follow Frame", description="Render again on frame change and during playback")]
    fields: Annotated[bool, bpy.props.BoolProperty(name="Field Nodes", description="Shader nodes of geometry trees as swatches", default=True)]
    surface: Annotated[str, bpy.props.EnumProperty(name="Surface", description="Shader preview surface", items=SURFACES, default=_LIGHTING.surface.name)]
    ambient: Annotated[str, bpy.props.EnumProperty(name="Ambient", description="Shader preview ambient light", items=case_choices(Ambient), default=Studio.__name__)]
    world: Annotated[str, bpy.props.StringProperty(name="World", description="Studio light environment", default=Studio().name)]
    strength: Annotated[float, bpy.props.FloatProperty(name="World Light", description="Ambient light strength", min=0.0, max=10.0, default=_LIGHTING.strength)]
    key: Annotated[float, bpy.props.FloatProperty(name="Key Light", description="Key sun strength", min=0.0, max=20.0, default=_LIGHTING.key)]
    scale: Annotated[float, bpy.props.FloatProperty(name="Thumbnail Size", description="Share of node width", subtype="FACTOR", min=0.25, max=3.0, default=1.0)]
    position: Annotated[str, bpy.props.EnumProperty(name="Position", description="Node side holding thumbnail", items=enum_choices(Position))]
    use_enlarge: Annotated[bool, bpy.props.BoolProperty(name="Enlarge Active Node", description="Active node thumbnail larger on top")]
    enlarge: Annotated[float, bpy.props.FloatProperty(name="Enlarge", description="Active node scale while enlarged", min=1.25, max=6.0, default=2.5)]
    checker: Annotated[bool, bpy.props.BoolProperty(name="Checkerboard", description="Theme checker behind thumbnails")]
    show_values: Annotated[bool, bpy.props.BoolProperty(name="Show Values", description="Number on uniform swatches", default=True)]
    limit: Annotated[int, bpy.props.IntProperty(name="Cached Thumbnails", description="Thumbnails kept in GPU memory and on disk", min=16, max=4096, default=256)]

    @property
    def lighting(self) -> PreviewLighting:
        """Shader preview lighting."""
        ambient = msgspec.convert({"kind": self.ambient, "name": self.world}, Ambient.__value__)
        return PreviewLighting(surface=PreviewMesh[self.surface], ambient=ambient, strength=self.strength, key=self.key)


class NodeSetting(bpy.types.PropertyGroup):
    """Node thumbnail settings."""

    use_output: Annotated[bool, bpy.props.BoolProperty(name="Pick Output", description="Show the named output, first linked when off")]
    output: Annotated[str, bpy.props.StringProperty(name="Output", description="Name of the shown output")]


class Editor(msgspec.Struct, frozen=True, kw_only=True):
    """Source scene, object, modifier, and group path one geometry node editor previews."""

    item: Handle[bpy.types.Object]
    scene: Handle[bpy.types.Scene]
    modifier: str
    groups: tuple[str, ...]

    @classmethod
    def of(cls, space: bpy.types.SpaceNodeEditor, scene: bpy.types.Scene) -> Outcome[Option[Self]]:
        """Reads a unique native group path and its active or sole modifier, refusing ambiguous editor paths."""
        match space.id_from, space.node_tree:
            case bpy.types.Object() as item, bpy.types.GeometryNodeTree() as root:
                using = tuple(modifier for modifier in item.modifiers if isinstance(modifier, bpy.types.NodesModifier) and modifier.node_group == root)
                match item.modifiers.active, using:
                    case bpy.types.NodesModifier(node_group=group) as modifier, _ if group == root:
                        chosen = Some(modifier)
                    case _, (modifier,):
                        chosen = Some(modifier)
                    case _, ():
                        return Ok(Nothing)
                    case _:
                        return Error(Fault.of(Refusal.REQUIRED, repr(space), tuple(modifier.name for modifier in using), member="explicit preview modifier"))
                return enclosing(space).map(
                    lambda groups: chosen.map(lambda modifier: cls(item=Handle.of(item), scene=Handle.of(scene), modifier=modifier.name, groups=tuple(node.name for node in groups)))
                )
            case _:
                return Ok(Nothing)

    def tree(self, root: bpy.types.NodeTree) -> Option[bpy.types.NodeTree]:
        """Returns tree group path reaches from root."""
        return reduce(lambda held, name: held.bind(lambda tree: Option.of_optional(tree.nodes.get(name))).bind(group_tree), self.groups, Some(root))

    def held(self) -> Outcome[bpy.types.NodeTree]:
        """Resolves the group of the editor's modifier."""
        return self.item.resolve().bind(
            lambda item: (
                Option
                .of_optional(try_downcast(bpy.types.NodesModifier, item.modifiers.get(self.modifier)))
                .bind(lambda found: Option.of_optional(found.node_group))
                .to_result_with(partial(Fault.of, Reason.ABSENT, repr(item.modifiers), member=self.modifier))
            )
        )


class PreviewTarget(msgspec.Struct, frozen=True, kw_only=True):
    """Node output a job renders and a cell draws, with the isolated render it takes and whether a uniform value reads back."""

    editor: Editor
    node: str
    socket: str
    kind: RenderKind
    valued: bool


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [TARGETS]


def targets(identity: Identity, editor: Editor, tree: bpy.types.NodeTree, settings: NodePreviews) -> Outcome[tuple[tuple[bpy.types.Node, bpy.types.NodeSocket, PreviewTarget], ...]]:
    """Returns in-scope tree nodes outside the zones valid unmuted links reach from a zone input before its output, with the targets their outputs and setting choose."""
    scope = PreviewScope[settings.scope]
    following: defaultdict[str, set[str]] = defaultdict(set)
    for source, _, sink, _ in (endpoints(link) for link in tree.links if link.is_valid and not link.is_muted):
        following[source.name].add(sink.name)
    zones: set[str] = set()
    for start, end in ((node.name, end.name) for node in tree.nodes for end in struct_at(node, ("paired_output",), bpy.types.Node).to_list()):
        frontier, seen = {start}, {start}
        while frontier:
            frontier = {sink for name in frontier - {end} for sink in following.get(name, ())} - seen
            seen.update(frontier)
        zones |= seen

    def chosen(node: bpy.types.Node, setting: NodeSetting) -> tuple[bpy.types.NodeSocket, ...]:
        eligible = (
            tuple(
                socket
                for socket in node.outputs
                if socket.enabled and not socket.hide and (socket.type == "GEOMETRY" or (isinstance(node, bpy.types.ShaderNode) and socket.type in {"RGBA", "VECTOR", "VALUE"}))
            )
            if isinstance(node.id_data, bpy.types.GeometryNodeTree)
            else ()
        )
        used = tuple(socket for socket in eligible if socket.is_linked)
        picked = tuple(socket for socket in eligible if setting.use_output and socket.name == setting.output)
        return used if settings.every else (picked or used or eligible)[:1]

    def shown(node: bpy.types.Node) -> bool:
        match scope:
            case PreviewScope.ALL:
                return True
            case PreviewScope.SELECTED:
                return node.select
            case PreviewScope.MARKED:
                return node.show_preview

    def isolated(node: bpy.types.Node, socket: bpy.types.NodeSocket) -> Option[PreviewTarget]:
        kind = Some(RenderKind.GEOMETRY_CLAY) if socket.type == "GEOMETRY" else Some(RenderKind.FIELD_SWATCH) if settings.fields and isinstance(node, bpy.types.ShaderNode) else Nothing
        return kind.map(lambda found: PreviewTarget(editor=editor, node=node.name, socket=socket.identifier, kind=found, valued=socket.type in _VALUED))

    candidates = tuple(
        node for node in tree.nodes if not (isinstance(node, bpy.types.NodeFrame | bpy.types.NodeReroute | bpy.types.NodeGroupInput | bpy.types.NodeGroupOutput) or node.mute or node.name in zones)
    )
    return SETTING.bound(identity).map(
        lambda attached: tuple((node, socket, target) for node in candidates if shown(node) for socket in chosen(node, attached.struct(node)) for target in isolated(node, socket).to_list())
    )


def enclosing(space: bpy.types.SpaceNodeEditor) -> Outcome[tuple[bpy.types.Node, ...]]:
    """Resolves unique group instances between native adjacent trees; repeated instances require an explicit `Editor.groups` path."""
    trees = tuple(item.node_tree for item in space.path)

    def entered(parent: bpy.types.NodeTree | None, child: bpy.types.NodeTree | None) -> Outcome[bpy.types.Node]:
        match parent, child:
            case bpy.types.NodeTree(), bpy.types.NodeTree():
                match tuple(node for node in parent.nodes if group_tree(node) == Some(child)):
                    case (node,):
                        return Ok(node)
                    case matches:
                        return Error(Fault.of(Reason.OUTSIDE, repr(space), tuple(node.name for node in matches), member="explicit Editor.groups for an ambiguous native path"))
            case _:
                return Error(Fault.of(Reason.ABSENT, repr(space), member="native node-tree path"))

    return accumulated(itertools.starmap(entered, itertools.pairwise(trees)))


# --- [ROUTES]


def _node(tree: bpy.types.NodeTree, name: str) -> Outcome[bpy.types.Node]:
    """Reads one native node, retaining its missing-key cause."""
    try:
        return Ok(tree.nodes[name])
    except KeyError as error:
        return Error(Fault.of(Reason.ABSENT, repr(tree.nodes), error, member=name))


def _copied[T: bpy.types.ID](identity: Identity, block: T, copies: ExitStack) -> Outcome[T]:
    """Copies a block under the preview route ownership for the copies' scope."""
    refused = partial(Fault.of, Refusal.REFUSED, repr(block))

    def built() -> Outcome[T]:
        try:
            copied = block.copy()
        except RuntimeError as error:
            return Error(refused(error))
        return Option.of_optional(copied).to_result_with(refused)

    return Ownership(kind=type(block), key="preview_route", saved=Saved.NEVER, single=False).made(identity, built, scope=Some(copies))


def _routed[H: Holder](identity: Identity, holder: H, target: PreviewTarget, copies: ExitStack) -> Outcome[tuple[H, bpy.types.NodeSocket]]:
    """Copies the holder and every group down the target's editor path for the copies' scope, exposing the target output on the copied holder's tree.

    Args:
        identity: Extension marking the copies.
        holder: Tree holder nesting target node.
        target: Route of copies.
        copies: Scope removing the copies.

    Returns:
        Copied holder and the socket holding the target output.
    """

    def exposed(inner: bpy.types.NodeTree, source: bpy.types.NodeSocket) -> Outcome[str]:
        return (
            active_output(inner)
            .bind(lambda output: socket_at(output.inputs, "__extend__"))
            .bind(lambda extend: linked_sockets(inner, source, extend, handle_dynamic_sockets=True))
            .map(lambda link: endpoints(link)[3].identifier)
        )

    def entered(node: bpy.types.Node) -> Outcome[bpy.types.NodeTree]:
        group = group_tree(node).to_result_with(partial(Fault.of, Reason.ABSENT, repr(node), member=bpy.types.NodeTree.__name__))
        return group.bind(lambda inner: _copied(identity, inner, copies)).bind(lambda inner: written(node, ("node_tree",), inner).map(lambda _: inner))

    def outward(tree: bpy.types.NodeTree) -> Outcome[bpy.types.NodeSocket]:
        parents: list[tuple[bpy.types.Node, bpy.types.NodeTree]] = []
        for name in target.editor.groups:
            match _node(tree, name).bind(lambda node: entered(node).map(lambda inner: (node, inner))):
                case Result(tag="ok", ok=(node, inner)):
                    parents.append((node, inner))
                    tree = inner
                case Result(error=faults):
                    return Error(faults)
        source = _node(tree, target.node).bind(lambda node: socket_at(node.outputs, target.socket))
        for node, inner in reversed(parents):
            source = source.bind(lambda socket: exposed(inner, socket).bind(lambda identifier: socket_at(node.outputs, identifier)))
        return source

    return _copied(identity, holder, copies).bind(lambda copy: tree_of(copy).bind(outward).map(lambda socket: (copy, socket)))


# --- [RENDERING]


def rendered(identity: Identity, preview: PreviewScene, target: PreviewTarget, size: int, lighting: PreviewLighting, world: Graph) -> Outcome[Readback]:
    """Renders a target through routed copies in the preview scene, staged per render kind and formed through the scene view, the source's updates during the render marked as its own."""
    editor = target.editor
    aov = Some(identity.identifier(PreviewScene.value)) if target.valued else Nothing

    def staged(scene: bpy.types.Scene, **changes: object) -> Staging:
        return msgspec.structs.replace(Staging(source=scene, input=(size, size), world=world, sun=lighting.sun), **changes)

    def swatched() -> Outcome[bpy.types.Material]:
        def graph(source: bpy.types.Node, name: Option[str]) -> Outcome[Graph]:
            own: Members = {member.path: member.read(source) for member in Descriptor.saved(source, lambda prop: not prop.is_readonly and prop.identifier not in bpy.types.Node.bl_rna.properties)}
            coordinates = Expr(kind=bpy.types.ShaderNodeTexCoord, socket="Generated", socket_type=bpy.types.NodeSocketVector, key=Some("coordinates"))
            inputs = tuple(
                (item.identifier, coordinates if item.type == "VECTOR" else found.read(item))
                for item in source.inputs
                if not item.is_linked
                for found in Descriptor.resolve(item, ("default_value",), bpy.types.Property).to_option().to_list()
            )
            field = Expr(kind=type(source), inputs=inputs, members=own, socket=target.socket, key=Some("field"))
            emission = Expr(kind=bpy.types.ShaderNodeEmission, inputs=(("Color", field),), socket="Emission", key=Some("emission"))
            roots = (
                Expr(kind=bpy.types.ShaderNodeOutputMaterial, inputs=(("Surface", emission),), members={"target": "ALL"}, key=Some("output")),
                *(Expr(kind=bpy.types.ShaderNodeOutputAOV, inputs=(("Value", field),), members={"aov_name": held}, key=Some("aov")) for held in name.to_list()),
            )
            return functions.lowered(bpy.types.ShaderNodeTree, *roots)

        found: Outcome[bpy.types.Node] = (
            editor.held().bind(lambda root: editor.tree(root).to_result_with(partial(Fault.of, Reason.ABSENT, repr(editor), member=target.node))).bind(lambda tree: _node(tree, target.node))
        )
        material = SWATCH.created(identity, bpy.data.materials.new, "Swatch")
        return paired(found, material).bind(lambda parts: paired(graph(parts[0], aov), tree_of(parts[1])).bind(lambda pair: reconciled(*pair, identity)).map(lambda _: parts[1]))

    def clayed(copies: ExitStack) -> Outcome[bpy.types.Object]:
        def material_set(group: bpy.types.NodeTree, upstream: bpy.types.NodeSocket, material: bpy.types.Material) -> Outcome[None]:
            def connected(output: bpy.types.NodeGroupOutput, node: bpy.types.Node) -> Outcome[None]:
                return accumulated((
                    socket_at(node.inputs, "Geometry").bind(lambda socket: linked_sockets(group, upstream, socket)),
                    socket_at(node.outputs, "Geometry").bind(lambda socket: linked_sockets(group, socket, output.inputs[0])),
                    socket_at(node.inputs, "Material").bind(lambda socket: written(socket, ("default_value",), material)),
                )).map(lambda _: None)

            return paired(active_output(group), created(group, bpy.types.GeometryNodeSetMaterial)).bind(lambda pair: connected(*pair))

        def prepared(source: bpy.types.Object, held: tuple[Holder, bpy.types.NodeSocket], material: bpy.types.Material) -> Outcome[bpy.types.Object]:
            holder, socket = held
            return paired(tree_of(holder), _copied(identity, source, copies)).bind(
                lambda pair: accumulated((
                    written(pair[1], ("hide_render",), value=False),
                    written(pair[1], ("hide_viewport",), value=False),
                    written(modifier := pair[1].modifiers[editor.modifier], ("node_group",), pair[0]),
                    written(modifier, ("show_viewport",), value=True),
                    written(modifier, ("show_render",), value=True),
                    *(
                        written(later, (member,), value=False)
                        for later in itertools.dropwhile(lambda modifier: modifier.name != editor.modifier, pair[1].modifiers)
                        if later.name != editor.modifier
                        for member in ("show_viewport", "show_render")
                    ),
                    material_set(pair[0], socket, material),
                )).map(lambda _: pair[1])
            )

        clay = CLAY.created(identity, bpy.data.materials.new, "Clay").bind(lambda material: shaded(material, Clay(), identity))
        return paired(editor.item.resolve(), editor.held().bind(lambda group: _routed(identity, group, target, copies)), clay).bind(lambda parts: prepared(*parts))

    def sourced(scene: bpy.types.Scene) -> Outcome[Readback]:
        with intermediate(identity, scene), ExitStack() as copies:
            match target.kind:
                case RenderKind.GEOMETRY_CLAY:
                    staging = clayed(copies).map(lambda subject: staged(scene, viewpoint=subject))
                case RenderKind.FIELD_SWATCH:
                    staging = swatched().map(lambda material: staged(scene, material=Some(material), surface=Some(lighting.surface), value=target.valued))
            return staging.bind(lambda desired: preview.rendered(identity, desired)).bind(lambda readback: readback.formed(viewed(scene), PyOpenColorIO.GetCurrentConfig()))

    return editor.scene.resolve().bind(sourced)


# --- [COMPOSITION] ----------------------------------------------------------------------

NODE_PREVIEWS: Final = Preference(key="node_previews", form=NodePreviews)
SETTING: Final = Attachment(key="node_preview", form=NodeSetting, owner=bpy.types.Node)
CLAY: Final = Ownership(kind=bpy.types.Material, key="preview_clay", saved=Saved.NEVER)
SWATCH: Final = Ownership(kind=bpy.types.Material, key="preview_swatch", saved=Saved.NEVER)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CLAY", "NODE_PREVIEWS", "SETTING", "SWATCH", "Editor", "NodePreviews", "NodeSetting", "Position", "PreviewScope", "PreviewTarget", "RenderKind", "enclosing", "rendered", "targets"]
