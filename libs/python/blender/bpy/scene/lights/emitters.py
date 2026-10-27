"""Emitter discovery, active emitter, emitter selection, and owned mesh lights."""

from collections import deque
from collections.abc import Callable
from typing import ClassVar, Final

from bl_ui.space_view3d import VIEW3D_MT_light_add
import bpy
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np

from blender.bpy.commands.operators import Call, Done, Operator
from blender.bpy.data.ids.ownership import kept, Ownership, Saved
from blender.bpy.data.ids.store import Handle, IdName
from blender.bpy.data.rna.descriptors import Descriptor, typed, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes import changes
from blender.bpy.nodes.graphs import Expr, Graph, mixed
from blender.bpy.nodes.trees import emptied, socket_at, tree_of
from blender.bpy.scene.objects import ObjectState, reconciled, shaped, spawned, VERTEX
from blender.bpy.settings.attachments import Attachment, reconciling
from blender.bpy.ui.layout.rows import acquired, Member, OperatorButton
from blender.bpy.ui.layout.sections import Section, Single
from blender.bpy.ui.layout.values import Hosted, Store
from blender.bpy.ui.surfaces.appends import Append
from blender.records.faults import accumulated, Fault, Outcome, paired
from blender.records.trees import Retention

# --- [CONSTANTS] ------------------------------------------------------------------------

TEMPERATURE: Final = Descriptor.declared(bpy.types.Light, "temperature", bpy.types.FloatProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class MeshLight(bpy.types.PropertyGroup):
    """Emissive square a person edits as one light."""

    size: bpy.props.FloatProperty(name="Size", description="Square edge length", subtype="DISTANCE", min=0.0, soft_min=0.0254, soft_max=3.048, default=0.9144)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    strength: bpy.props.FloatProperty(name="Strength", description="Emission strength", min=0.0, soft_max=100.0, default=1.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    color: bpy.props.FloatVectorProperty(name="Color", description="Emission filter", subtype="COLOR", size=3, default=(1.0, 1.0, 1.0))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_temperature: bpy.props.BoolProperty(name="Blackbody", description="Filter a blackbody color", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    temperature: bpy.props.FloatProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name=TEMPERATURE.prop.name,
        description="Blackbody color the color filters",
        subtype="COLOR_TEMPERATURE",
        min=TEMPERATURE.prop.hard_min,
        max=TEMPERATURE.prop.hard_max,
        soft_min=TEMPERATURE.prop.soft_min,
        soft_max=TEMPERATURE.prop.soft_max,
        default=5500.0,
    )
    single_sided: bpy.props.BoolProperty(name="Single Sided", description="Front face alone emits", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    constant: bpy.props.BoolProperty(name="Constant Falloff", description="Strength equal at every distance")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    camera: bpy.props.BoolProperty(name="Camera", description="Visible to camera")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class MeshLights(msgspec.Struct, frozen=True, kw_only=True):
    """Mesh light attachment, add operator, and section."""

    retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing

    @staticmethod
    def reshaped(light: Handle[bpy.types.Object], identity: Identity) -> Outcome[Option[str]]:
        """Reconciles mesh light to its stored group in deferred write.

        Args:
            light: Handle of mesh light object.
            identity: Identity binding attachment and marking owned data.

        Returns:
            `Nothing` naming no undo step, or `absent`, `register`, `read`, and `lit` faults.
        """
        return light.resolve().bind(lambda item: MeshLights.attachment.read(identity, item).bind(lambda group: lit(group, item, identity))).map(lambda _: Nothing)

    attachment: ClassVar[Attachment[MeshLight]] = Attachment(key="mesh_light", form=MeshLight, owner=bpy.types.Object, changed=Some(reconciling(bpy.types.Object, reshaped)))
    light: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key="mesh_light", saved=Saved.USED, single=False)
    material: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="mesh_light", saved=Saved.USED)

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns attachment, ownerships, add operator, its Add Light menu entry, and section."""
        return Ok((
            MeshLights.attachment,
            MeshLights.light,
            MeshLights.material,
            Append(target=VIEW3D_MT_light_add, rows=(OperatorButton(target=mesh_light_add_command),)),
            *acquired((self.section(),)),
        ))

    def section(self) -> Section:
        """Returns mesh light section of active object."""
        return Single(
            key="mesh_light",
            title="Mesh Light",
            store=Some(Hosted(host=Some(Member(root=bpy.types.LayerObjects, path="active")), store=Store(attachment=MeshLights.attachment, retention=self.retention))),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def feeding(socket: bpy.types.NodeSocket) -> tuple[tuple[bpy.types.Node, bpy.types.NodeSocket], ...]:
    """Returns source node and socket of each valid unmuted link into socket."""
    links: tuple[bpy.types.NodeLink, ...] = socket.links  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    sources: tuple[tuple[bpy.types.Node, bpy.types.NodeSocket], ...] = tuple((link.from_node, link.from_socket) for link in links if link.is_valid and not link.is_muted)  # type: ignore[misc]  # ty: ignore[invalid-assignment]
    return sources


def strengths(material: bpy.types.Material) -> tuple[bpy.types.NodeSocketFloat, ...]:
    """Returns linked or positive Emission and Principled BSDF strength sockets reaching active Cycles output."""
    type State = tuple[bpy.types.Node, bpy.types.NodeSocket, tuple[bpy.types.ShaderNodeGroup, ...]]
    emitted: dict[type[bpy.types.Node], str] = {bpy.types.ShaderNodeEmission: "Strength", bpy.types.ShaderNodeBsdfPrincipled: "Emission Strength"}
    pending: deque[State] = deque(
        (output, socket, ()) for tree in Option.of_optional(material.node_tree).to_list() for output in Option.of_optional(tree.get_output_node("CYCLES")).to_list() for socket in output.inputs
    )
    visited: set[State] = set()
    found: dict[bpy.types.NodeSocketFloat, None] = {}
    while pending:
        node, socket, path = state = pending.popleft()
        if state in visited:
            continue
        visited.add(state)
        match node:
            case _ if not socket.is_output:
                pending.extend((source, output, path) for source, output in feeding(socket))
            case _ if node.mute:
                routed: tuple[bpy.types.NodeSocket, ...] = tuple(link.from_socket for link in node.internal_links if link.to_socket == socket)  # type: ignore[misc]  # ty: ignore[invalid-assignment]
                pending.extend((node, inner, path) for inner in routed)
            case bpy.types.ShaderNodeGroup(node_tree=tree) if tree is not None:
                pending.extend(
                    (output, inner, (*path, node))
                    for output in tree.nodes
                    if isinstance(output, bpy.types.NodeGroupOutput) and output.is_active_output
                    for inner in output.inputs
                    if inner.identifier == socket.identifier
                )
            case bpy.types.NodeGroupInput() if path:
                pending.extend((path[-1], outer, path[:-1]) for outer in path[-1].inputs if outer.identifier == socket.identifier)
            case _:
                found.update(
                    (strength, None)
                    for identifier in Option.of_optional(emitted.get(type(node))).to_list()
                    for strength in socket_at(node.inputs, identifier).to_option().to_list()
                    if isinstance(strength, bpy.types.NodeSocketFloat) and (feeding(strength) or strength.default_value > 0.0)
                )
                pending.extend((node, inner, path) for inner in node.inputs)
    return tuple(found)


def emitting(item: bpy.types.Object) -> bool:
    """Returns whether object is a light or has an emitting material."""
    return isinstance(item.data, bpy.types.Light) or any(strengths(slot.material) for slot in item.material_slots if slot.material is not None)


def emitters(owner: bpy.types.Scene | bpy.types.ViewLayer | bpy.types.Object) -> tuple[bpy.types.Object, ...]:
    """Returns emitting objects of scene, view layer, or one object in owner order."""
    return tuple(item for item in (owner.objects if isinstance(owner, bpy.types.Scene | bpy.types.ViewLayer) else (owner,)) if emitting(item))


def picked(chosen: frozenset[bpy.types.Object], view_layer: bpy.types.ViewLayer) -> Outcome[None]:
    """Selects chosen objects of view layer alone."""
    return accumulated(reconciled(ObjectState(selected=Some(item in chosen)), item, (view_layer,)) for item in view_layer.objects).map(lambda _: None)


def emission_graph(record: MeshLight) -> Outcome[Graph]:
    """Returns shader graph of mesh light values, `absent` and `choice` faults for missing node members."""
    filtered = (*record.color, 1.0)
    blackbody = Expr(kind=bpy.types.ShaderNodeBlackbody, inputs=(("Temperature", record.temperature),), socket="Color", shape="RGBA")
    falloff = Expr(kind=bpy.types.ShaderNodeLightFalloff, inputs=(("Strength", record.strength),), socket="Constant")
    emission = Expr(
        kind=bpy.types.ShaderNodeEmission,
        inputs=(("Color", mixed(1.0, blackbody, filtered, blend="MULTIPLY") if record.use_temperature else filtered), ("Strength", falloff if record.constant else record.strength)),
        socket="Emission",
    )
    sides = Expr(
        kind=bpy.types.ShaderNodeMixShader,
        inputs=(("Fac", Expr(kind=bpy.types.ShaderNodeNewGeometry, socket="Backfacing")), ("Shader", emission), ("Shader_001", Expr(kind=bpy.types.ShaderNodeBsdfTransparent, socket="BSDF"))),
        socket="Shader",
    )
    return Graph.lowered(
        bpy.types.ShaderNodeTree, Expr(kind=bpy.types.ShaderNodeOutputMaterial, inputs=(("Surface", sides if record.single_sided else emission),), members=(("is_active_output", True),))
    )


def lit(record: MeshLight, item: bpy.types.Object, identity: Identity) -> Outcome[None]:
    """Writes mesh light quad, material graph, slot, and ray visibility from mesh light values.

    Args:
        record: Mesh light values of the stored group or the add operator.
        item: Owned mesh light object.
        identity: Identity marking material and nodes.

    Returns:
        `Ok` once written, or graph, sampling, and object faults.
    """
    half = record.size / 2.0
    visibility = Snapshot.of(bpy.types.Object, {"visible_camera": record.camera, "visible_shadow": False, "show_wire": True, "show_all_edges": True})
    vertices = Descriptor.quantized(VERTEX.prop, np.array(((-half, -half, 0.0), (-half, half, 0.0), (half, half, 0.0), (half, -half, 0.0)), dtype=np.float64))

    def built() -> Outcome[bpy.types.Material]:
        return kept(
            catch(bpy.data.materials.new, exception=RuntimeError)(item.name).map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=f"{item.name} material", cause=error),)), emptied
        )

    return (
        typed(item.data, bpy.types.Mesh, f"{item!r}.data")
        .bind(lambda mesh: paired(emission_graph(record), vertices).bind(lambda prepared: shaped(mesh, prepared[1], np.arange(4).reshape(1, 4)).map(lambda _: prepared[0])))
        .bind(
            lambda wanted: MeshLights.material.made(
                identity,
                built,
                of=Some(item),
                filled=lambda material: (
                    paired(tree_of(material).bind(lambda tree: changes.reconciled(wanted, tree, identity)), written(material, "cycles.emission_sampling", "FRONT" if record.single_sided else "AUTO"))
                    .bind(lambda _: reconciled(ObjectState(material_slots=Some((Snapshot.of(bpy.types.MaterialSlot, {"material": IdName.of(material)}),)), values=Some(visibility)), item, ()))
                    .map(lambda _: material)
                ),
            )
        )
        .map(lambda _: None)
    )


def made(record: MeshLight, scene: bpy.types.Scene, collection: bpy.types.Collection, identity: Identity) -> Outcome[bpy.types.Object]:
    """Creates owned mesh light at 3D cursor, storing and applying the add operator's values.

    Args:
        record: Mesh light values of the add operator.
        scene: Scene with cursor placing light.
        collection: Collection receiving light.
        identity: Identity marking object, material, and nodes.

    Returns:
        Mesh light, or `register`, `edit`, and `lit` faults.
    """

    def configured(item: bpy.types.Object) -> Outcome[bpy.types.Object]:
        return (
            MeshLights.attachment
            .bound(identity)
            .bind(lambda attached: Snapshot.capture(record).bind(lambda held: msgspec.structs.replace(held, struct=MeshLight.__name__).apply(attached.struct(item)).outcome))
            .bind(lambda _: reconciled(ObjectState(placement=Some(scene.cursor.matrix)), item, ()))
            .bind(lambda _: lit(record, item, identity))
            .map(lambda _: item)
        )

    return kept(
        catch(bpy.data.meshes.new, exception=RuntimeError)("Mesh Light").map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject="Mesh Light mesh", cause=error),)),
        lambda mesh: MeshLights.light.made(identity, lambda: spawned("Mesh Light", Some(mesh), collection), filled=configured),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def mesh_light_add(call: Call[MeshLight]) -> Outcome[Done]:
    """Adds an emissive square at the 3D cursor."""
    return (
        paired(call.root(bpy.types.Scene), call.root(bpy.types.Collection))
        .bind(lambda pair: made(call.properties, *pair, call.identity))
        .map(lambda item: Done(notes=(("INFO", f"Added {item.name}"),)))
    )


mesh_light_add_command: Operator[MeshLight] = Operator(form=MeshLight, run=mesh_light_add)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TEMPERATURE", "MeshLight", "MeshLights", "emission_graph", "emitters", "emitting", "feeding", "lit", "made", "mesh_light_add_command", "picked", "strengths"]
