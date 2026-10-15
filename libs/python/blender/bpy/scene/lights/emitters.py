"""Emitters by one walk, the active emitter, the selection write over emitters, and owned mesh lights."""

from collections import deque
from collections.abc import Callable
from contextlib import ExitStack
import sys
from typing import Annotated, ClassVar

from bl_ui.space_view3d import VIEW3D_MT_light_add
import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np

from blender.bpy.commands.operators import Done, Operator
from blender.bpy.data.ids.ownership import earliest, Mark, Ownership, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.descriptors import Descriptor, IdName, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderEditFault, BlenderResult
from blender.bpy.nodes import changes
from blender.bpy.nodes.graphs import End, Graph, Link, Node
from blender.bpy.nodes.trees import emptied, socket_at, tree_of
from blender.bpy.scene.objects import ObjectState, reconciled, shaped, spawned
from blender.bpy.settings.attachments import Attachment, reconciling
from blender.bpy.ui.layout.rows import acquired, Member, OperatorButton
from blender.bpy.ui.layout.sections import Hosted, Section, Single, Store
from blender.bpy.ui.surfaces.appends import Append
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Finite, Gated, Presentation, Quantity, Rgb
from blender.rendering.history import Retention

# --- [MODELS] ---------------------------------------------------------------------------


class MeshLight(msgspec.Struct, frozen=True, kw_only=True):
    """Emissive square a person edits as one light."""

    size: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Size", description="Edge length of the emitting square"), Presentation(soft=Some((0.0254, 3.048)), quantity=Some(Quantity.LENGTH))
    ] = 0.9144
    strength: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Strength", description="Emission strength of the front face"), Presentation(soft=Some((0.0, 100.0)), decimals=Some(2))
    ] = 1.0
    color: Annotated[Rgb[Finite], msgspec.Meta(title="Color", description="Filter on the emission")] = Rgb(red=1.0, green=1.0, blue=1.0)
    temperature: Annotated[
        Gated[Annotated[float, msgspec.Meta(ge=800.0, le=20000.0)]],
        msgspec.Meta(title="Temperature", description="Blackbody color the color filters"),
        Presentation(step=Some(1.0), decimals=Some(0), quantity=Some(Quantity.COLOR_TEMPERATURE)),
    ] = Gated(enabled=True, value=5500.0)
    single_sided: Annotated[bool, msgspec.Meta(title="Single Sided", description="Emit from the front face alone")] = True
    constant: Annotated[bool, msgspec.Meta(title="Constant Falloff", description="Strength equal at every distance")] = False
    camera: Annotated[bool, msgspec.Meta(title="Camera", description="Show the emitting face to the camera")] = False


class MeshLights(msgspec.Struct, frozen=True, kw_only=True):
    """Row attaching `MeshLight` to objects with its add operator, reconcile, and section."""

    retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing

    @staticmethod
    def reshaped(light: Handle[bpy.types.Object], identity: Identity) -> BlenderResult[Option[str]]:
        """Reconciles a mesh light to the record it stores, the write a record edit defers.

        Args:
            light: Handle of the mesh light object.
            identity: Identity binding the attachment and marking owned data.

        Returns:
            `Ok` of `Nothing`, a reconcile pushing no undo step; `absent` for an object the file no longer holds, `register`, `read`, and the faults of `lit`.
        """
        return paired(light.resolve(), MeshLights.attachment.bound(identity)).bind(lambda pair: pair[1].read(pair[0]).bind(lambda record: lit(record, pair[0], identity))).map(lambda _: Nothing)

    attachment: ClassVar[Attachment[MeshLight]] = Attachment(key="mesh_light", form=MeshLight, owner=bpy.types.Object, changed=Some(reconciling(bpy.types.Object, reshaped)))
    light: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key="mesh_light", saved=Saved.USED, single=False)
    material: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="mesh_light", saved=Saved.USED)

    add: ClassVar[Operator[MeshLight]] = Operator(
        key="mesh_light_add",
        title="Mesh Light",
        description="Add an emissive square at the 3D cursor",
        form=MeshLight,
        run=lambda call: (
            paired(call.root(bpy.types.Scene), call.root(bpy.types.Collection))
            .bind(lambda pair: made(call.record, *pair, call.identity))
            .map(lambda item: Done(notes=(("INFO", f"Added {item.name}"),)))
        ),
    )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Answers the attachment, the ownership merges, the add operator, and its Add Light menu entry.

        Args:
            identity: Extension placing the row.

        Returns:
            `Ok` of every band and acquisition pair, or every construction fault.
        """
        return Ok((
            MeshLights.attachment,
            MeshLights.light,
            MeshLights.material,
            Append(target=VIEW3D_MT_light_add, rows=(OperatorButton(target=self.add),)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            *acquired((self.section(),)),
        ))

    def section(self) -> Section:
        """Section drawing the active object's mesh light record."""
        return Single(
            key="mesh_light", title="Mesh Light", store=Some(Hosted(host=Member(root=bpy.types.LayerObjects, path="active"), store=Store(attachment=MeshLights.attachment, retention=self.retention)))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def strengths(material: bpy.types.Material) -> tuple[bpy.types.NodeSocketFloat, ...]:
    """Answers the emitting strength sockets a material's active output reaches.

    Args:
        material: Material whose tree is walked.

    Returns:
        Each linked or positive strength socket of an Emission or Principled node, in walk order.
    """
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
                    (source, link.from_socket, path)
                    for link in socket.links  # type: ignore[union-attr]  # ty: ignore[not-iterable]
                    if link.is_valid and not link.is_muted and (source := link.from_node) is not None and link.from_socket is not None
                )
            case _ if node.mute:
                pending.extend((node, link.from_socket, path) for link in node.internal_links if link.to_socket == socket and link.from_socket is not None)
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
    """Whether an object is a light or holds an emitting slot material."""
    return isinstance(item.data, bpy.types.Light) or any(strengths(slot.material) for slot in item.material_slots if slot.material is not None)


def emitters(owner: bpy.types.Scene | bpy.types.ViewLayer | bpy.types.Object) -> tuple[bpy.types.Object, ...]:
    """Answers the emitters of a scene, a view layer, or one object.

    Args:
        owner: Scene, view layer, or object a scope reaches.

    Returns:
        The emitting objects in the owner's object order.
    """
    return tuple(item for item in (owner.objects if isinstance(owner, bpy.types.Scene | bpy.types.ViewLayer) else (owner,)) if emitting(item))


def focused(view_layer: bpy.types.ViewLayer) -> BlenderResult[bpy.types.Object]:
    """Answers the active object of a view layer, the emitter a row edits.

    Args:
        view_layer: View layer whose active object is read.

    Returns:
        `Ok` of the active object; `absent` holding the layer's objects where none is active.
    """
    objects = view_layer.objects
    return Option.of_optional(objects.active).to_result_with(lambda: (BlenderAbsentFault(owner=repr(objects), key="active"),))


def picked(chosen: frozenset[bpy.types.Object], view_layer: bpy.types.ViewLayer) -> BlenderResult[None]:
    """Selects the chosen objects of a view layer and deselects every other.

    Args:
        chosen: Objects left selected.
        view_layer: View layer whose base flags change.

    Returns:
        `Ok` once written; the `edit` faults `scene.objects` `reconciled` answers.
    """
    return accumulated(reconciled(ObjectState(selected=Some(item in chosen)), item, (view_layer,)) for item in view_layer.objects).map(lambda _: None)


def emission_graph(record: MeshLight) -> BlenderResult[Graph]:
    """Answers the shader tree a mesh light record derives.

    Args:
        record: Mesh light values.

    Returns:
        `Ok` of the graph; `absent` or `choice` for a node member the node classes lack.
    """
    rgba, tinted, constant, sided = (*msgspec.structs.astuple(record.color), 1.0), record.temperature.enabled, record.constant, record.single_sided
    surface = (
        (
            Link(source=End(node="facing", socket="Backfacing"), target=End(node="sides", socket="Fac")),
            Link(source=End(node="emission", socket="Emission"), target=End(node="sides", socket="Shader")),
            Link(source=End(node="back", socket="BSDF"), target=End(node="sides", socket="Shader_001")),
            Link(source=End(node="sides", socket="Shader"), target=End(node="output", socket="Surface")),
        )
        if sided
        else (Link(source=End(node="emission", socket="Emission"), target=End(node="output", socket="Surface")),)
    )
    return paired(
        paired(Descriptor.resolve(bpy.types.ShaderNodeOutputMaterial, "is_active_output", bpy.types.BoolProperty), Descriptor.resolve(bpy.types.ShaderNodeMix, "data_type", bpy.types.EnumProperty)),
        Descriptor.resolve(bpy.types.ShaderNodeMix, "blend_type", bpy.types.EnumProperty),
    ).map(
        lambda found: Graph(
            kind=bpy.types.ShaderNodeTree,
            nodes={
                "output": Node(kind=bpy.types.ShaderNodeOutputMaterial, members={found[0][0]: True}),
                "emission": Node(kind=bpy.types.ShaderNodeEmission, inputs={**({} if tinted else {"Color": rgba}), **({} if constant else {"Strength": record.strength})}),
                **(
                    {
                        "blackbody": Node(kind=bpy.types.ShaderNodeBlackbody, inputs={"Temperature": record.temperature.value}),
                        "tint": Node(kind=bpy.types.ShaderNodeMix, members={found[0][1]: "RGBA", found[1]: "MULTIPLY"}, inputs={"Factor_Float": 1.0, "B_Color": rgba}),
                    }
                    if tinted
                    else {}
                ),
                **({"falloff": Node(kind=bpy.types.ShaderNodeLightFalloff, inputs={"Strength": record.strength})} if constant else {}),
                **({"facing": Node(kind=bpy.types.ShaderNodeNewGeometry), "back": Node(kind=bpy.types.ShaderNodeBsdfTransparent), "sides": Node(kind=bpy.types.ShaderNodeMixShader)} if sided else {}),
            },
            links=(
                *(
                    (
                        Link(source=End(node="blackbody", socket="Color"), target=End(node="tint", socket="A_Color")),
                        Link(source=End(node="tint", socket="Result_Color"), target=End(node="emission", socket="Color")),
                    )
                    if tinted
                    else ()
                ),
                *((Link(source=End(node="falloff", socket="Constant"), target=End(node="emission", socket="Strength")),) if constant else ()),
                *surface,
            ),
        )
    )


def lit(record: MeshLight, item: bpy.types.Object, identity: Identity) -> BlenderResult[None]:
    """Rewrites a mesh light's quad, material graph, slot, and ray visibility from its record.

    Args:
        record: Mesh light values.
        item: Owned mesh light object.
        identity: Identity marking the material and its nodes.

    Returns:
        `Ok` once written; the faults of the graph, its reconcile, the sampling write, and the object reconcile.
    """
    half = record.size / 2.0
    visibility = Snapshot.of(bpy.types.Object, {"visible_camera": record.camera, "visible_shadow": False, "show_wire": True, "show_all_edges": True})
    if not isinstance(mesh := item.data, bpy.types.Mesh):
        return Error((BlenderChoiceFault(path=f"{item!r}.data", identifier=type(mesh).__name__, accepted=(bpy.types.Mesh.__name__,)),))

    vertices = Descriptor.resolve(bpy.types.MeshVertex, "co", bpy.types.FloatProperty).bind(
        lambda co: Descriptor.quantized(co.prop, np.array(((-half, -half, 0.0), (-half, half, 0.0), (half, half, 0.0), (half, -half, 0.0)), dtype=np.float64))
    )

    with ExitStack() as rollback:
        mark = Mark(key=MeshLights.material.key, of=Some(item))

        def retained(material: bpy.types.Material) -> BlenderResult[bpy.types.Material]:
            rollback.callback(bpy.data.batch_remove, ids=(material,))
            return emptied(material)

        def built() -> BlenderResult[bpy.types.Material]:
            try:
                material = bpy.data.materials.new("Mesh Light")
            except (ReferenceError, RuntimeError, TypeError, ValueError) as error:
                return Error((BlenderEditFault(request="Mesh Light material", error=error),))
            return MeshLights.material.owned(identity, material, mark).bind(retained)

        def published(_: object) -> None:
            rollback.pop_all()

        return (
            paired(emission_graph(record), vertices)
            .bind(lambda prepared: shaped(mesh, prepared[1], np.arange(4).reshape(1, 4)).map(lambda _: prepared[0]))
            .bind(
                lambda wanted: (
                    Option
                    .of_optional(earliest(MeshLights.material.held(identity)).get(mark))
                    .map(Ok)
                    .default_with(built)
                    .bind(
                        lambda material: paired(
                            tree_of(material).bind(lambda tree: changes.reconciled(wanted, tree, identity)),
                            written(material.cycles, "emission_sampling", "FRONT" if record.single_sided else "AUTO"),  # type: ignore[arg-type]
                        ).bind(lambda _: reconciled(ObjectState(material_slots=Some((Snapshot.of(bpy.types.MaterialSlot, {"material": IdName.of(material)}),)), values=Some(visibility)), item, ()))
                    )
                )
            )
            .map(published)
        )


def made(record: MeshLight, scene: bpy.types.Scene, collection: bpy.types.Collection, identity: Identity) -> BlenderResult[bpy.types.Object]:
    """Builds an owned mesh light at the 3D cursor and stores and draws its record.

    Args:
        record: Mesh light values.
        scene: Scene whose cursor places the light.
        collection: Collection receiving the light.
        identity: Identity marking the object, material, and nodes.

    Returns:
        `Ok` of the mesh light; `register`, `edit`, and the faults of `lit`.
    """
    with ExitStack() as rollback:

        def published(item: bpy.types.Object) -> bpy.types.Object:
            rollback.pop_all()
            return item

        def configured(item: bpy.types.Object) -> BlenderResult[bpy.types.Object]:
            rollback.callback(bpy.data.objects.remove, item, do_unlink=True)
            return (
                MeshLights.attachment
                .bound(identity)
                .bind(lambda attached: attached.write(item, record))
                .bind(lambda _: reconciled(ObjectState(placement=Some(scene.cursor.matrix)), item, ()))
                .bind(lambda _: lit(record, item, identity))
                .map(lambda _: item)
            )

        try:
            mesh = bpy.data.meshes.new("Mesh Light")
            rollback.callback(bpy.data.meshes.remove, mesh)
        except RuntimeError as error:
            return Error((BlenderEditFault(request="Mesh Light mesh", error=error),))
        return MeshLights.light.made(identity, lambda: spawned("Mesh Light", Some(mesh), collection)).bind(configured).map(published)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["MeshLight", "MeshLights", "emission_graph", "emitters", "emitting", "focused", "lit", "made", "picked", "strengths"]
