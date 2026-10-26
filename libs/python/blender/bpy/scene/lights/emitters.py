"""Emitter discovery, active emitter, emitter selection, and owned mesh lights."""

from collections import deque
from collections.abc import Callable
from typing import Annotated, ClassVar, Final

from bl_ui.space_view3d import VIEW3D_MT_light_add
import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np

from blender.bpy.commands.operators import Done, Operator
from blender.bpy.data.ids.ownership import kept, Ownership, Saved
from blender.bpy.data.ids.store import Handle, IdName, vacant
from blender.bpy.data.rna.descriptors import Descriptor, typed, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderEditFault, BlenderResult
from blender.bpy.nodes import changes
from blender.bpy.nodes.graphs import Expr, Graph, mixed
from blender.bpy.nodes.trees import emptied, socket_at, tree_of
from blender.bpy.scene.objects import ObjectState, reconciled, shaped, spawned, VERTEX
from blender.bpy.settings.attachments import Attachment, reconciling
from blender.bpy.settings.history import Retention
from blender.bpy.ui.layout.rows import acquired, Member, OperatorButton
from blender.bpy.ui.layout.sections import Section, Single
from blender.bpy.ui.layout.values import Hosted, Store
from blender.bpy.ui.surfaces.appends import Append
from blender.records.faults import accumulated, paired
from blender.records.fields import Finite, Gated, NonNegative, Presentation, Quantity, Rgb

# --- [TYPES] ----------------------------------------------------------------------------

type Kelvin = Annotated[Gated[Annotated[float, TEMPERATURE.meta]], TEMPERATURE.presentation]

# --- [CONSTANTS] ------------------------------------------------------------------------

TEMPERATURE: Final = Descriptor.declared(bpy.types.Light, "temperature", bpy.types.FloatProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class MeshLight(msgspec.Struct, frozen=True, kw_only=True):
    """Emissive square a person edits as one light."""

    size: Annotated[NonNegative, msgspec.Meta(description="Square edge length"), Presentation(soft=Some((0.0254, 3.048)), quantity=Some(Quantity.LENGTH))] = 0.9144
    strength: Annotated[NonNegative, msgspec.Meta(description="Emission strength"), Presentation(soft=Some((0.0, 100.0)), precision=Some(2))] = 1.0
    color: Annotated[Rgb[Finite], msgspec.Meta(description="Emission filter")] = Rgb(red=1.0, green=1.0, blue=1.0)
    temperature: Annotated[Kelvin, msgspec.Meta(description="Blackbody color the color filters")] = Gated(enabled=True, value=5500.0)
    single_sided: Annotated[bool, msgspec.Meta(description="Front face alone emits")] = True
    constant: Annotated[bool, msgspec.Meta(title="Constant Falloff", description="Strength equal at every distance")] = False
    camera: Annotated[bool, msgspec.Meta(description="Visible to camera")] = False


class MeshLights(msgspec.Struct, frozen=True, kw_only=True):
    """Mesh light attachment, add operator, and section."""

    retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing

    @staticmethod
    def reshaped(light: Handle[bpy.types.Object], identity: Identity) -> BlenderResult[Option[str]]:
        """Reconciles mesh light to stored record in deferred write.

        Args:
            light: Handle of mesh light object.
            identity: Identity binding attachment and marking owned data.

        Returns:
            `Nothing` naming no undo step, or `absent`, `register`, `read`, and `lit` faults.
        """
        return light.resolve().bind(lambda item: MeshLights.attachment.read(identity, item).bind(lambda record: lit(record, item, identity))).map(lambda _: Nothing)

    attachment: ClassVar[Attachment[MeshLight]] = Attachment(key="mesh_light", form=MeshLight, owner=bpy.types.Object, changed=Some(reconciling(bpy.types.Object, reshaped)))
    light: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key="mesh_light", saved=Saved.USED, single=False)
    material: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="mesh_light", saved=Saved.USED)

    add: ClassVar[Operator[MeshLight]] = Operator(
        key="mesh_light_add",
        title="Mesh Light",
        description="Add emissive square at 3D cursor",
        form=MeshLight,
        run=lambda call: (
            paired(call.root(bpy.types.Scene), call.root(bpy.types.Collection))
            .bind(lambda pair: made(call.record, *pair, call.identity))
            .map(lambda item: Done(notes=(("INFO", f"Added {item.name}"),)))
        ),
    )

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns attachment, ownerships, add operator, its Add Light menu entry, and section."""
        return Ok((MeshLights.attachment, MeshLights.light, MeshLights.material, Append(target=VIEW3D_MT_light_add, rows=(OperatorButton(target=self.add),)), *acquired((self.section(),))))

    def section(self) -> Section:
        """Returns mesh light section of active object."""
        return Single(
            key="mesh_light",
            title="Mesh Light",
            store=Some(Hosted(host=Some(Member(root=bpy.types.LayerObjects, path="active")), store=Store(attachment=MeshLights.attachment, retention=self.retention))),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


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
                pending.extend(
                    (link.from_node, link.from_socket, path)  # type: ignore[misc]  # ty: ignore[invalid-argument-type]
                    for link in socket.links  # type: ignore[union-attr]  # ty: ignore[not-iterable]
                    if link.is_valid and not link.is_muted
                )
            case _ if node.mute:
                pending.extend((node, link.from_socket, path) for link in node.internal_links if link.to_socket == socket)  # type: ignore[misc]  # ty: ignore[invalid-argument-type]
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
                    if isinstance(strength, bpy.types.NodeSocketFloat) and (any(link.is_valid and not link.is_muted for link in strength.links) or strength.default_value > 0.0)  # type: ignore[union-attr]  # ty: ignore[not-iterable]
                )
                pending.extend((node, inner, path) for inner in node.inputs)
    return tuple(found)


def emitting(item: bpy.types.Object) -> bool:
    """Returns whether object is a light or has an emitting material."""
    return isinstance(item.data, bpy.types.Light) or any(strengths(slot.material) for slot in item.material_slots if slot.material is not None)


def emitters(owner: bpy.types.Scene | bpy.types.ViewLayer | bpy.types.Object) -> tuple[bpy.types.Object, ...]:
    """Returns emitting objects of scene, view layer, or one object in owner order."""
    return tuple(item for item in (owner.objects if isinstance(owner, bpy.types.Scene | bpy.types.ViewLayer) else (owner,)) if emitting(item))


def picked(chosen: frozenset[bpy.types.Object], view_layer: bpy.types.ViewLayer) -> BlenderResult[None]:
    """Selects chosen objects of view layer alone."""
    return accumulated(reconciled(ObjectState(selected=Some(item in chosen)), item, (view_layer,)) for item in view_layer.objects).map(lambda _: None)


def emission_graph(record: MeshLight) -> BlenderResult[Graph]:
    """Returns shader graph of mesh light record, `absent` and `choice` faults for missing node members."""
    filtered = (*msgspec.structs.astuple(record.color), 1.0)
    blackbody = Expr(kind=bpy.types.ShaderNodeBlackbody, inputs=(("Temperature", record.temperature.value),), socket="Color", shape="RGBA")
    falloff = Expr(kind=bpy.types.ShaderNodeLightFalloff, inputs=(("Strength", record.strength),), socket="Constant")
    emission = Expr(
        kind=bpy.types.ShaderNodeEmission,
        inputs=(("Color", mixed(1.0, blackbody, filtered, blend="MULTIPLY") if record.temperature.enabled else filtered), ("Strength", falloff if record.constant else record.strength)),
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


def lit(record: MeshLight, item: bpy.types.Object, identity: Identity) -> BlenderResult[None]:
    """Writes mesh light quad, material graph, slot, and ray visibility from record.

    Args:
        record: Mesh light values.
        item: Owned mesh light object.
        identity: Identity marking material and nodes.

    Returns:
        `Ok` once written, or graph, sampling, and object faults.
    """
    half = record.size / 2.0
    visibility = Snapshot.of(bpy.types.Object, {"visible_camera": record.camera, "visible_shadow": False, "show_wire": True, "show_all_edges": True})
    vertices = Descriptor.quantized(VERTEX.prop, np.array(((-half, -half, 0.0), (-half, half, 0.0), (half, half, 0.0), (half, -half, 0.0)), dtype=np.float64))

    def built() -> BlenderResult[bpy.types.Material]:
        return kept(BlenderEditFault.attempted(f"{item.name} material", lambda: bpy.data.materials.new(vacant(item.name, bpy.data.materials.keys()))), emptied)

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


def made(record: MeshLight, scene: bpy.types.Scene, collection: bpy.types.Collection, identity: Identity) -> BlenderResult[bpy.types.Object]:
    """Creates owned mesh light at 3D cursor, storing and applying record.

    Args:
        record: Mesh light values.
        scene: Scene with cursor placing light.
        collection: Collection receiving light.
        identity: Identity marking object, material, and nodes.

    Returns:
        Mesh light, or `register`, `edit`, and `lit` faults.
    """

    def configured(item: bpy.types.Object) -> BlenderResult[bpy.types.Object]:
        return (
            MeshLights.attachment
            .bound(identity)
            .bind(lambda attached: attached.write(item, record))
            .bind(lambda _: reconciled(ObjectState(placement=Some(scene.cursor.matrix)), item, ()))
            .bind(lambda _: lit(record, item, identity))
            .map(lambda _: item)
        )

    return kept(
        BlenderEditFault.attempted("Mesh Light mesh", lambda: bpy.data.meshes.new(vacant("Mesh Light", bpy.data.meshes.keys()))),
        lambda mesh: MeshLights.light.made(identity, lambda: spawned("Mesh Light", Some(mesh), collection), filled=configured),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TEMPERATURE", "Kelvin", "MeshLight", "MeshLights", "emission_graph", "emitters", "emitting", "lit", "made", "picked", "strengths"]
