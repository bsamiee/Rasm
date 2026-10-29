"""Isolated node preview renders: routed copies exposing a target output, and the clay and swatch renders each target takes in the preview scene."""

from collections.abc import Generator, Mapping
from contextlib import contextmanager, ExitStack
from functools import partial
import itertools
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Result, Some, try_downcast
from expression.extra.result import catch
from mathutils import Vector
import msgspec
import PyOpenColorIO

from blender.bpy.data.ids import content
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.data.rna.paths import RnaPath, struct_at
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes import functions
from blender.bpy.nodes.changes import reconciled
from blender.bpy.nodes.desired import Graph, Members
from blender.bpy.nodes.graphs import Expr
from blender.bpy.nodes.trees import active_output, created, GROUP_NODES, Holder, joined, socket_at, tree_of
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.outputs.results import intermediate
from blender.bpy.render.previews.fixtures import Fitted, lit
from blender.bpy.render.previews.scenes import PreviewScene, Readback, Staging
from blender.bpy.render.previews.targets import NodePreviews, RenderKind, Target
from blender.bpy.scene.materials import Clay, shaded
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired

# --- [CONSTANTS] ------------------------------------------------------------------------

CLAY_AIM: Final = Vector((0.3, -0.6, 0.9)).normalized().to_track_quat("Z", "Y").to_euler()

# --- [MODELS] ---------------------------------------------------------------------------


class Routed[H: Holder](msgspec.Struct, frozen=True, kw_only=True):
    """Copied holder and routed socket, or kept output node's input."""

    holder: H
    socket: bpy.types.NodeSocket


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [ROUTES]


def copied[T: bpy.types.ID](identity: Identity, block: T, copies: ExitStack) -> Outcome[T]:
    """Copies a block under the preview route ownership for the copies' scope."""
    refused = partial(Fault.of, BlenderReason.REFUSED, repr(block))
    made = PreviewScene.routes.made(
        identity, lambda: catch(block.copy, exception=RuntimeError)().map_error(refused).bind(lambda copy: Option.of_optional(copy).to_result_with(refused)), scope=Some(copies)
    )
    return made.bind(lambda copy: Option.of_optional(try_downcast(type(block), copy)).to_result_with(refused))


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
        return Option.of_optional(tree.nodes.get(name)).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(tree.nodes), member=name))

    with ExitStack() as copies:

        def exposed(inner: bpy.types.NodeTree, source: bpy.types.NodeSocket) -> Outcome[str]:
            return (
                active_output(inner)
                .bind(lambda output: socket_at(output.inputs, "__extend__"))
                .bind(lambda extend: joined(inner, source, extend, handle_dynamic_sockets=True))
                .map(lambda link: content.endpoints(link)[3].identifier)
            )

        def entered(node: bpy.types.Node) -> Outcome[bpy.types.NodeTree]:
            group = (struct_at(node, ("node_tree",), bpy.types.NodeTree) if isinstance(node, GROUP_NODES) else Nothing).to_result_with(
                partial(Fault.of, BlenderReason.ABSENT, repr(node), member=bpy.types.NodeTree.__name__)
            )
            return Result[bpy.types.NodeTree, Faults].bind(group, lambda inner: copied(identity, inner, copies)).bind(lambda inner: written(node, ("node_tree",), inner).map(lambda _: inner))

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

        yield copied(identity, holder, copies).bind(lambda copy: tree_of(copy).bind(lambda tree: outward(tree, target.editor.groups)).map(lambda socket: Routed(holder=copy, socket=socket)))


# --- [RENDERS]


def rendered(identity: Identity, preview: PreviewScene, target: Target, size: int, settings: NodePreviews, *, scene: bpy.types.Scene) -> Outcome[Readback]:
    """Renders a target through routed copies in the preview scene, staged per render kind and formed through the scene view, the source's updates during the render marked as its own."""
    lighting, editor = settings.lighting, target.editor
    aov: Outcome[Option[str]] = identity.identifier(PreviewScene.value).map(Some) if target.valued else Ok(Nothing)

    def staged(world: Mapping[RnaPath, Value] | bpy.types.World, lamp: Mapping[RnaPath, Value] | bpy.types.Light, *, valued: bool) -> Staging:
        return Staging(source=scene, size=(size, size), world=world, lamp=lamp, value=valued and target.valued)

    def clay() -> Outcome[bpy.types.Material]:
        made = CLAY.made(identity, lambda: catch(bpy.data.materials.new, exception=RuntimeError)("Clay").map_error(partial(Fault.of, BlenderReason.REFUSED, CLAY.key)))
        return made.bind(lambda material: shaded(material, Clay(), PyOpenColorIO.GetCurrentConfig(), identity))

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
            .bind(lambda tree: tree.bind(lambda inner: Option.of_optional(inner.nodes.get(target.node))).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(editor), member=target.node)))
        )
        material = SWATCH.made(identity, lambda: catch(bpy.data.materials.new, exception=RuntimeError)("Swatch").map_error(partial(Fault.of, BlenderReason.REFUSED, SWATCH.key)))
        return paired(found, aov, material).bind(lambda parts: paired(graph(parts[0], parts[1]), tree_of(parts[2])).bind(lambda pair: reconciled(*pair, identity)).map(lambda _: parts[2]))

    def clayed(copies: ExitStack) -> Outcome[bpy.types.Object]:
        def clothed(group: bpy.types.NodeTree, carried: Routed[Holder], material: bpy.types.Material) -> Outcome[None]:
            def set_material(output: bpy.types.NodeGroupOutput, node: bpy.types.Node) -> Outcome[None]:
                return accumulated((
                    socket_at(node.inputs, "Geometry").bind(lambda socket: joined(group, carried.socket, socket)),
                    socket_at(node.outputs, "Geometry").bind(lambda socket: joined(group, socket, output.inputs[0])),
                    socket_at(node.inputs, "Material").bind(lambda socket: written(socket, ("default_value",), material)),
                )).map(lambda _: None)

            return paired(active_output(group), created(group, bpy.types.GeometryNodeSetMaterial)).bind(lambda pair: set_material(*pair))

        def dressed(source: bpy.types.Object, carried: Routed[Holder], material: bpy.types.Material) -> Outcome[bpy.types.Object]:
            return paired(tree_of(carried.holder), copied(identity, source, copies)).bind(
                lambda pair: accumulated((
                    written(pair[1], ("parent",), None),
                    written(pair[1], ("location",), (0.0, 0.0, 0.0)),
                    written(pair[1].modifiers[editor.modifier], ("node_group",), pair[0]),
                    *(
                        written(later, ("show_render",), value=False)
                        for later in itertools.dropwhile(lambda modifier: modifier.name != editor.modifier, pair[1].modifiers)
                        if later.name != editor.modifier
                    ),
                    clothed(pair[0], carried, material),
                )).map(lambda _: pair[1])
            )

        return paired(editor.item.resolve(), editor.held().bind(lambda group: copies.enter_context(routed(identity, group, target))), clay()).bind(lambda parts: dressed(*parts))

    with intermediate(identity, scene), ExitStack() as copies:
        match target.kind:
            case RenderKind.GEOMETRY_CLAY:
                staging = paired(clayed(copies), msgspec.structs.replace(lighting, strength=0.25).environment).map(
                    lambda pair: msgspec.structs.replace(staged(pair[1], lit(4.0, (CLAY_AIM.x, CLAY_AIM.y, CLAY_AIM.z)), valued=False), viewpoint=Fitted(subject=pair[0]))
                )
            case RenderKind.FIELD_SWATCH:
                staging = paired(swatched(), lighting.environment).map(
                    lambda pair: msgspec.structs.replace(staged(pair[1], lighting.lamp, valued=True), material=Some(pair[0]), surface=Some(lighting.surface))
                )
        return staging.bind(lambda desired: preview.rendered(identity, desired)).bind(lambda readback: readback.formed(viewed(scene), PyOpenColorIO.GetCurrentConfig()))


# --- [COMPOSITION] ----------------------------------------------------------------------

CLAY: Final = Ownership(kind=bpy.types.Material, key="preview_clay", saved=Saved.NEVER)
SWATCH: Final = Ownership(kind=bpy.types.Material, key="preview_swatch", saved=Saved.NEVER)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CLAY", "CLAY_AIM", "SWATCH", "Routed", "copied", "rendered", "routed"]
