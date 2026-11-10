"""Emitter discovery, active emitter, emitter selection, and owned mesh lights."""

from collections.abc import Callable, Iterable, Mapping
from functools import partial
from types import MappingProxyType
from typing import Annotated, ClassVar, Final

from bl_ui.space_view3d import VIEW3D_MT_light_add
import bpy
from expression import Option, Some
from expression.extra.result import catch
from mathutils import Color
import msgspec
import numpy as np

from rasm_bpy.commands.operators import Call, Done, Operator
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import acquired_rows, Acquisitions
from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._ids.ownership import kept_block, Ownership, Saved
from rasm_bpy.data._ids.store import Handle, IdName
from rasm_bpy.data._rna.descriptors import Descriptor, written
from rasm_bpy.data._rna.paths import rooted
from rasm_bpy.data._rna.properties import reconciling, redeclared
from rasm_bpy.data._rna.snapshots import Snapshot
from rasm_bpy.nodes import changes, functions
from rasm_bpy.nodes._graphs import Expr, mixed
from rasm_bpy.nodes._trees import active_output, emptied, feeds, nested, socket_at, tree_of
from rasm_bpy.scene._objects import ObjectState, reconciled, shaped, spawned, VERTEX
from rasm_bpy.settings._attachments import Attachment
from rasm_bpy.ui._elements.menus import Append
from rasm_bpy.ui._layout.members import Member
from rasm_bpy.ui._layout.rows import OperatorButton
from rasm_bpy.ui._layout.sections import Section
from rasm_bpy.ui._layout.values import Store
from rasm_records import accumulated, Fault, Nothing, Outcome, paired, reached, Retention, typed

# --- [CONSTANTS] ------------------------------------------------------------------------

TEMPERATURE: Final = redeclared(Descriptor.of(bpy.types.Light, ("temperature",), bpy.types.FloatProperty).prop)

# --- [MODELS] ---------------------------------------------------------------------------


class MeshLight(bpy.types.PropertyGroup):
    """Emissive square a person edits as one light."""

    size: Annotated[float, bpy.props.FloatProperty(name="Size", description="Square edge length", subtype="DISTANCE", min=0.0, soft_min=0.0254, soft_max=3.048, default=0.9144)]
    strength: Annotated[float, bpy.props.FloatProperty(name="Strength", description="Emission strength", min=0.0, soft_max=100.0, default=1.0)]
    color: Annotated[Color, bpy.props.FloatVectorProperty(name="Color", description="Emission filter", subtype="COLOR", size=3, default=(1.0, 1.0, 1.0))]
    use_temperature: Annotated[bool, bpy.props.BoolProperty(name="Blackbody", description="Filter a blackbody color", default=True)]
    temperature: Annotated[float, TEMPERATURE]
    single_sided: Annotated[bool, bpy.props.BoolProperty(name="Single Sided", description="Front face alone emits", default=True)]
    constant: Annotated[bool, bpy.props.BoolProperty(name="Constant Falloff", description="Strength equal at every distance")]
    camera: Annotated[bool, bpy.props.BoolProperty(name="Camera", description="Visible to camera")]


class _MeshLights(msgspec.Struct, frozen=True, kw_only=True):
    """Mesh light attachment, add operator, and section."""

    retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing

    @staticmethod
    def reshaped(light: Handle[bpy.types.Object], identity: Identity) -> Outcome[Option[str]]:
        """Reconciles mesh light to its stored group in deferred write.

        Args:
            light: Handle of mesh light object.
            identity: Identity binding attachment and marking owned data.

        Returns:
            `Nothing` naming no undo step, or `absent`, `unregistered`, `unreadable`, and `lit` faults.
        """
        return light.resolve().bind(lambda item: _MeshLights.attachment.read(identity, item).bind(lambda group: _lit(group, item, identity))).map(lambda _: Nothing)

    attachment: ClassVar[Attachment[MeshLight]] = Attachment(key="mesh_light", form=MeshLight, owner=bpy.types.Object, changed=Some(partial(reconciling, bpy.types.Object, reshaped)))
    light: ClassVar[Ownership[bpy.types.Object]] = Ownership(kind=bpy.types.Object, key="mesh_light", saved=Saved.USED, single=False)
    material: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="mesh_light", saved=Saved.USED)

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns attachment, ownerships, add operator, its Add Light menu entry, and section."""
        return _MeshLights.attachment, _MeshLights.light, _MeshLights.material, Append(target=VIEW3D_MT_light_add, rows=(OperatorButton(target=mesh_light_add),)), *acquired_rows((self.section(),))

    def section(self) -> Section[MeshLight]:
        """Returns mesh light section of active object."""
        return Section(key="mesh_light", store=Some(Store(attachment=_MeshLights.attachment, host=Some(Member(root=bpy.types.LayerObjects, path=("active",))), retention=self.retention)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def strengths(material: bpy.types.Material) -> tuple[tuple[bpy.types.NodeSocketFloat, bool], ...]:
    """Returns linked or positive Emission and Principled BSDF strength sockets reaching active Cycles output, each with whether a valid unmuted link feeds it."""
    emitted: Mapping[type[bpy.types.Node], str] = MappingProxyType({bpy.types.ShaderNodeEmission: "Strength", bpy.types.ShaderNodeBsdfPrincipled: "Emission Strength"})
    roots = Option.of_optional(material.node_tree).to_list()
    fed = feeds(nested(roots))

    def expanded(
        state: tuple[bpy.types.Node, bpy.types.NodeSocket, tuple[bpy.types.ShaderNodeGroup, ...]],
    ) -> Iterable[tuple[bpy.types.Node, bpy.types.NodeSocket, tuple[bpy.types.ShaderNodeGroup, ...]]]:
        node, socket, path = state
        match node:
            case _ if not socket.is_output:
                return ((source, output, path) for source, output in fed.get(socket, ()))
            case _ if node.mute:
                return ((node, output, path) for link in node.internal_links if link.to_socket == socket for output in Option.of_optional(link.from_socket).to_list())
            case bpy.types.ShaderNodeGroup(node_tree=tree) if tree is not None:
                return ((output, inner, (*path, node)) for output in active_output(tree).to_option().to_list() for inner in output.inputs if inner.identifier == socket.identifier)
            case bpy.types.NodeGroupInput() if path:
                return ((path[-1], outer, path[:-1]) for outer in path[-1].inputs if outer.identifier == socket.identifier)
            case _:
                return ((node, inner, path) for inner in node.inputs)

    states = reached(((output, socket, ()) for tree in roots for output in Option.of_optional(tree.get_output_node("CYCLES")).to_list() for socket in output.inputs), expanded)
    found = dict.fromkeys(
        strength
        for node, socket, _ in states
        if socket.is_output and not node.mute
        for identifier in Option.of_optional(emitted.get(type(node))).to_list()
        for strength in socket_at(node.inputs, identifier).to_option().to_list()
        if isinstance(strength, bpy.types.NodeSocketFloat) and (strength in fed or strength.default_value > 0.0)
    )
    return tuple((strength, strength in fed) for strength in found)


def emitting(item: bpy.types.Object) -> bool:
    """Returns whether object is a light or has an emitting material."""
    return isinstance(item.data, bpy.types.Light) or any(strengths(slot.material) for slot in item.material_slots if slot.material is not None)


def emitters(owner: bpy.types.Scene | bpy.types.ViewLayer | bpy.types.Object) -> tuple[bpy.types.Object, ...]:
    """Returns emitting objects of scene, view layer, or one object in owner order."""
    return tuple(item for item in (owner.objects if isinstance(owner, bpy.types.Scene | bpy.types.ViewLayer) else (owner,)) if emitting(item))


def picked(chosen: frozenset[bpy.types.Object], view_layer: bpy.types.ViewLayer) -> Outcome[None]:
    """Selects chosen objects of view layer alone."""
    return accumulated(reconciled(ObjectState(selected=Some(item in chosen)), item, (view_layer,)) for item in view_layer.objects).map(lambda _: None)


def _lit(record: MeshLight, item: bpy.types.Object, identity: Identity) -> Outcome[None]:
    """Writes mesh light quad, material graph, slot, and ray visibility from mesh light values.

    Args:
        record: Mesh light values of the stored group or the add operator.
        item: Owned mesh light object.
        identity: Identity marking material and nodes.

    Returns:
        `Ok` once written, or graph, sampling, and object faults.
    """
    half, filtered = record.size / 2.0, (*record.color[:], 1.0)
    blackbody = Expr(kind=bpy.types.ShaderNodeBlackbody, inputs=(("Temperature", record.temperature),), socket="Color", socket_type=bpy.types.NodeSocketColor)
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
    visibility = Snapshot.of(bpy.types.Object, {("visible_camera",): record.camera, ("visible_shadow",): False, ("show_wire",): True, ("show_all_edges",): True})
    vertices = Descriptor.quantized(VERTEX, np.array(((-half, -half, 0.0), (-half, half, 0.0), (half, half, 0.0), (half, -half, 0.0)), dtype=np.float64))

    def built() -> Outcome[bpy.types.Material]:
        return kept_block(catch(bpy.data.materials.new, exception=RuntimeError)(item.name).map_error(partial(Fault.of, Refusal.REFUSED, f"{item.name} material")), emptied)

    return (
        typed(bpy.types.Mesh, f"{item!r}.data", item.data)
        .bind(
            lambda mesh: paired(
                functions.lowered(
                    bpy.types.ShaderNodeTree, Expr(kind=bpy.types.ShaderNodeOutputMaterial, inputs=(("Surface", sides if record.single_sided else emission),), members={"is_active_output": True})
                ),
                vertices,
            ).bind(lambda prepared: shaped(mesh, prepared[1], np.arange(4).reshape(1, 4)).map(lambda _: prepared[0]))
        )
        .bind(
            lambda wanted: _MeshLights.material.made(
                identity,
                built,
                of=Some(item),
                filled=lambda material: (
                    paired(
                        tree_of(material).bind(lambda tree: changes.reconciled(wanted, tree, identity)), written(material, ("cycles", "emission_sampling"), "FRONT" if record.single_sided else "AUTO")
                    )
                    .bind(lambda _: reconciled(ObjectState(material_slots=Some((Snapshot.of(bpy.types.MaterialSlot, {("material",): IdName.of(material)}),)), values=Some(visibility)), item, ()))
                    .map(lambda _: material)
                ),
            )
        )
        .map(lambda _: None)
    )


def _made(record: MeshLight, scene: bpy.types.Scene, collection: bpy.types.Collection, identity: Identity) -> Outcome[bpy.types.Object]:
    """Creates owned mesh light at 3D cursor, storing and applying the add operator's values.

    Args:
        record: Mesh light values of the add operator.
        scene: Scene with cursor placing light.
        collection: Collection receiving light.
        identity: Identity marking object, material, and nodes.

    Returns:
        Mesh light, or `unregistered`, `refused`, and `lit` faults.
    """

    def configured(item: bpy.types.Object) -> Outcome[bpy.types.Object]:
        return (
            _MeshLights.attachment
            .bound(identity)
            .bind(lambda attached: Snapshot.capture(record).bind(lambda held: msgspec.structs.replace(held, struct=MeshLight.__name__).apply(attached.struct(item)).outcome))
            .bind(lambda _: reconciled(ObjectState(placement=Some(scene.cursor.matrix)), item, ()))
            .bind(lambda _: _lit(record, item, identity))
            .map(lambda _: item)
        )

    return kept_block(
        catch(bpy.data.meshes.new, exception=RuntimeError)("Mesh Light").map_error(partial(Fault.of, Refusal.REFUSED, "Mesh Light mesh")),
        lambda mesh: _MeshLights.light.made(identity, lambda: spawned("Mesh Light", Some(mesh), collection), filled=configured),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(MeshLight)
def mesh_light_add(call: Call[MeshLight]) -> Outcome[Done]:
    """Adds an emissive square at the 3D cursor."""
    return (
        paired(rooted(bpy.types.Scene, call.context), rooted(bpy.types.Collection, call.context))
        .bind(lambda pair: _made(call.properties, *pair, call.identity))
        .map(lambda item: Done(notes=(("INFO", f"Added {item.name}"),)))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TEMPERATURE", "MeshLight", "emitters", "emitting", "mesh_light_add", "picked", "strengths"]
