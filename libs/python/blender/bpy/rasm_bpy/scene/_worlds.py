"""World environments, graphs, swaps, sun-following nodes, and studio worlds."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from enum import Enum
from functools import partial, reduce
from pathlib import Path
from typing import Annotated, ClassVar, Final

import bpy
from expression import Option, Some
from expression.extra.result import catch
from mathutils import Color
import msgspec
import numpy as np

from rasm_bpy.commands.operators import Answer, Call, Done, Empty, Operator, Unchanged
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import Acquisitions
from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._ids.ownership import kept_block, Ownership, Saved
from rasm_bpy.data._ids.store import Handle
from rasm_bpy.data._rna.descriptors import Descriptor, Value, written
from rasm_bpy.data._rna.paths import RnaPath, rooted
from rasm_bpy.data._rna.properties import choices, reconciling, redeclared
from rasm_bpy.data._rna.snapshots import assigned
from rasm_bpy.nodes import functions
from rasm_bpy.nodes._addressing import owned
from rasm_bpy.nodes._content import Loaded
from rasm_bpy.nodes._desired import Graph, item_key, Members
from rasm_bpy.nodes._graphs import Expr, mixed, Term
from rasm_bpy.nodes._trees import emptied, tree_of
from rasm_bpy.nodes.changes import reconciled
from rasm_bpy.settings._attachments import Attached, Attachment
from rasm_bpy.ui._layout.members import Member
from rasm_bpy.ui._layout.rows import OperatorButton, Property
from rasm_bpy.ui._layout.sections import Section
from rasm_bpy.ui._layout.values import Store
from rasm_records import Fault, Nothing, Ok, Outcome, Retention
from rasm_rendering import SunPosition

# --- [TYPES] ----------------------------------------------------------------------------


class BackdropSource(Enum):
    """World source of a ray slot beside the Sky Texture types, valued by its label and description."""

    HDRI = ("HDRI", "Environment image with projection and rotation")
    COLOR = ("Color", "Scene-linear world color")
    GRADIENT = ("Gradient", "Vertical camera frame gradient, the background slot alone")
    IMAGE = ("Image", "Image filling camera frame, the background slot alone")


class _RaySlot(Enum):
    """Environment slot, valued by the Light Path output selecting its rays."""

    LIGHTING = ""
    GLOSSY = "Is Glossy Ray"
    CAMERA = "Is Camera Ray"

    @property
    def source(self) -> Key:
        """Key of the slot's source node."""
        return item_key(self.name.lower(), "source")


# --- [CONSTANTS] ------------------------------------------------------------------------

_SKY: Final = tuple(
    prop
    for prop in bpy.types.ShaderNodeTexSky.bl_rna.properties
    if prop.identifier not in bpy.types.ShaderNode.bl_rna.properties and isinstance(prop, bpy.types.FloatProperty | bpy.types.BoolProperty) and not prop.is_array
)
_FOLLOWED: Final = frozenset(("sun_elevation", "sun_rotation"))
_DISC: Final = "sun_disc"
_MODES: Final = choices((*Descriptor.of(bpy.types.ShaderNodeTexSky, ("sky_type",), bpy.types.EnumProperty).prop.enum_items, *((source.name, *source.value) for source in BackdropSource)))

# --- [MODELS] ---------------------------------------------------------------------------


class Sky(bpy.types.PropertyGroup):
    """Scalar members of the Sky Texture node, each declared from the node's own property."""

    __annotations__ = {prop.identifier: redeclared(prop) for prop in _SKY}


class Backdrop(bpy.types.PropertyGroup):
    """Ray slot source, strength, and tint, the source's sky, image, color, and gradient members."""

    source: Annotated[str, bpy.props.EnumProperty(name="Source", description="World source of the slot", items=_MODES, default=BackdropSource.COLOR.name)]
    strength: Annotated[float, bpy.props.FloatProperty(name="Strength", description="Background strength", min=0.0, soft_max=10.0, default=1.0)]
    tint: Annotated[Color, bpy.props.FloatVectorProperty(name="Tint", description="Color the source multiplies", subtype="COLOR", size=3, default=(1.0, 1.0, 1.0))]
    color: Annotated[Color, bpy.props.FloatVectorProperty(name="Color", description="Scene-linear world color", subtype="COLOR", size=3, default=(0.0508761, 0.0508761, 0.0508761))]
    top: Annotated[Color, bpy.props.FloatVectorProperty(name="Top", description="Gradient color at frame top", subtype="COLOR", size=3, default=(0.2, 0.2, 0.2))]
    bottom: Annotated[Color, bpy.props.FloatVectorProperty(name="Bottom", description="Gradient color at frame bottom", subtype="COLOR", size=3, default=(0.02, 0.02, 0.02))]
    path: Annotated[str, bpy.props.StringProperty(name="Image", description="Image file", subtype="FILE_PATH")]
    projection: Annotated[str, redeclared(Descriptor.of(bpy.types.ShaderNodeTexEnvironment, ("projection",), bpy.types.EnumProperty).prop)]
    rotation: Annotated[float, bpy.props.FloatProperty(name="Rotation", description="Fixed image rotation", subtype="ANGLE")]
    heading: Annotated[float, bpy.props.FloatProperty(name="Sun Heading", description="Heading of the image sun turned onto scene sun", subtype="ANGLE")]
    use_sun: Annotated[bool, bpy.props.BoolProperty(name="Follow Sun", description="Sky sun and image rotation follow the scene sun light")]
    sky: Annotated[Sky, bpy.props.PointerProperty(type=Sky, name="Sky")]


class Environment(bpy.types.PropertyGroup):
    """World lighting with switched background and reflection overrides."""

    lighting: Annotated[Backdrop, bpy.props.PointerProperty(type=Backdrop, name="Lighting")]
    use_camera: Annotated[bool, bpy.props.BoolProperty(name="Background", description="Show another source to the camera")]
    camera: Annotated[Backdrop, bpy.props.PointerProperty(type=Backdrop, name="Background")]
    use_glossy: Annotated[bool, bpy.props.BoolProperty(name="Reflection", description="Show another source in glossy reflections")]
    glossy: Annotated[Backdrop, bpy.props.PointerProperty(type=Backdrop, name="Reflection")]


class WorldEnvironment(msgspec.Struct, frozen=True, kw_only=True):
    """Environment attachment, owned worlds, operators, and section."""

    retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing

    @staticmethod
    def refreshed(world: Handle[bpy.types.World], identity: Identity) -> Outcome[Option[str]]:
        """Reconciles world tree to its stored environment, naming no undo step."""
        return (
            world
            .resolve()
            .bind(tree_of)
            .bind(lambda tree: WorldEnvironment.attachment.read(identity, tree).bind(_world_graph).bind(lambda graph: reconciled(graph, tree, identity)))
            .map(lambda _: Nothing)
        )

    attachment: ClassVar[Attachment[Environment]] = Attachment(key="environment", form=Environment, owner=bpy.types.NodeTree, changed=Some(partial(reconciling, bpy.types.World, refreshed)))
    ownership: ClassVar[Ownership[bpy.types.World]] = Ownership(kind=bpy.types.World, key="environment", saved=Saved.USED)

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns attachment, ownership, and operators."""
        return self.attachment, self.ownership, environment_place, environment_restore

    def section(self) -> Section[Environment]:
        """Returns environment, world settings, and place and restore buttons."""
        return Section(
            key="environment",
            rows=tuple(Property(target=Member(root=bpy.types.Context, path=("scene", "world", member))) for member in ("sun_threshold", "lightgroup")),
            header=(OperatorButton(target=environment_place), OperatorButton(target=environment_restore)),
            store=Some(Store(attachment=self.attachment, host=Some(Member(root=bpy.types.Context, path=("scene", "world", "node_tree"))), retention=self.retention)),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GRAPHS]


def _slots(environment: Environment) -> tuple[tuple[_RaySlot, Backdrop], ...]:
    """Returns lighting, then switched-on glossy and camera slots."""
    slots = ((_RaySlot.LIGHTING, True, environment.lighting), (_RaySlot.GLOSSY, environment.use_glossy, environment.glossy), (_RaySlot.CAMERA, environment.use_camera, environment.camera))
    return tuple((slot, backdrop) for slot, shown, backdrop in slots if shown)


def shaders(lighting: Term, overrides: tuple[tuple[_RaySlot, Term], ...] = ()) -> Outcome[Graph]:
    """Builds an active world output from a lighting shader and ordered ray-specific shader overrides."""

    def joined(shader: Term, slot: tuple[_RaySlot, Term]) -> Expr:
        ray = Expr(kind=bpy.types.ShaderNodeLightPath, socket=slot[0].value)
        return Expr(kind=bpy.types.ShaderNodeMixShader, inputs=(("Fac", ray), ("Shader", shader), ("Shader_001", slot[1])), socket="Shader")

    surface = reduce(joined, overrides, lighting)
    return functions.lowered(bpy.types.ShaderNodeTree, Expr(kind=bpy.types.ShaderNodeOutputWorld, inputs=(("Surface", surface),), members={"is_active_output": True}))


def _world_graph(environment: Environment) -> Outcome[Graph]:
    """Builds world graph from switched-on ray slots, each source node under its slot key."""

    def image(kind: type[bpy.types.Node], key: Key, path: str, inputs: tuple[tuple[str, Term], ...], members: Members) -> Expr:
        return Expr(
            kind=kind,
            inputs=inputs,
            members={**members, **({} if path else {"image": None})},
            content=Option.of_optional(path or None).map(lambda chosen: Loaded(path=Path(chosen))),
            socket="Color",
            socket_type=bpy.types.NodeSocketColor,
            key=Some(key),
        )

    def sourced(key: Key, backdrop: Backdrop) -> Term:
        match BackdropSource.__members__.get(backdrop.source):
            case BackdropSource.COLOR:
                return (*backdrop.color[:], 1.0)
            case BackdropSource.HDRI:
                rotated: dict[str | RnaPath, Value] = dict[str | RnaPath, Value]() if backdrop.use_sun else {("texture_mapping", "rotation"): (0.0, 0.0, backdrop.rotation)}
                return image(bpy.types.ShaderNodeTexEnvironment, key, backdrop.path, (), {"projection": backdrop.projection, **rotated})
            case BackdropSource.GRADIENT:
                window = Expr(kind=bpy.types.ShaderNodeTexCoord, socket="Window", socket_type=bpy.types.NodeSocketVector)
                return mixed(window["Y"], (*backdrop.bottom[:], 1.0), (*backdrop.top[:], 1.0)).keyed(key)
            case BackdropSource.IMAGE:
                return image(bpy.types.ShaderNodeTexImage, key, backdrop.path, (("Vector", Expr(kind=bpy.types.ShaderNodeTexCoord, socket="Window", socket_type=bpy.types.NodeSocketVector)),), {})
            case None:
                still, values = not backdrop.use_sun, {prop.identifier: Descriptor(root=Sky, prop=prop).read(backdrop.sky) for prop in _SKY}
                toward = SunPosition.toward(np.asarray(values["sun_rotation"], dtype=np.float64), np.asarray(values["sun_elevation"], dtype=np.float64))
                scattering = dict[str | RnaPath, Value]((name, value) for name, value in values.items() if still or name not in _FOLLOWED)
                direction: dict[str | RnaPath, Value] = {"sun_direction": tuple(float(value) for value in toward)} if still else dict[str | RnaPath, Value]()
                members: Members = {"sky_type": backdrop.source, ("texture_mapping", "rotation"): (0.0, 0.0, 0.0), **scattering, _DISC: scattering[_DISC] and still, **direction}
                return Expr(kind=bpy.types.ShaderNodeTexSky, members=members, socket="Color", socket_type=bpy.types.NodeSocketColor, key=Some(key))

    def background(source: Key, backdrop: Backdrop) -> Expr:
        tinted = mixed(1.0, sourced(source, backdrop), (*backdrop.tint[:], 1.0), blend="MULTIPLY")
        return Expr(kind=bpy.types.ShaderNodeBackground, inputs=(("Color", tinted), ("Strength", backdrop.strength)), socket="Background")

    (first, lighting), *overrides = _slots(environment)
    return shaders(background(first.source, lighting), tuple((slot, background(slot.source, backdrop)) for slot, backdrop in overrides))


# --- [HOLDERS]


def swapped(row: Ownership[bpy.types.World], values: Mapping[RnaPath, Value], scene: bpy.types.Scene, identity: Identity, scope: Option[ExitStack] = Nothing) -> Outcome[bpy.types.World]:
    """Writes environment members by path to an owned world and shows it on scene.

    Shown owned world serves first, then the earliest owned world, then a new one, each marking the world it replaces, which scope unwind shows again.

    Returns:
        Shown world, or the faults of binding, writing, and reconciling it.
    """
    predecessor = Option.of_optional(scene.world)
    for outer in scope.to_list():
        outer.callback(written, scene, ("world",), predecessor.to_optional())

    def displayed(attached: Attached[Environment], world: bpy.types.World) -> Outcome[bpy.types.World]:
        return (
            tree_of(world)
            .bind(lambda tree: assigned(attached.struct(tree), values).bind(lambda _: _world_graph(attached.struct(tree))).bind(lambda graph: reconciled(graph, tree, identity)))
            .bind(lambda _: written(scene, ("world",), world))
            .map(lambda _: world)
        )

    return WorldEnvironment.attachment.bound(identity).bind(
        lambda attached: (
            predecessor
            .filter(lambda world: row.marked(identity, world).is_some())
            .or_else_with(lambda: row.found(identity, of=predecessor) if row.single else Nothing)
            .map(lambda world: displayed(attached, world))
            .default_with(
                lambda: kept_block(
                    catch(bpy.data.worlds.new, exception=RuntimeError)(f"{scene.name} {bpy.path.display_name(row.key)}").map_error(partial(Fault.of, Refusal.REFUSED, row.key)), emptied
                ).bind(lambda world: row.owned(identity, world, of=predecessor, filled=lambda world: displayed(attached, world), scope=scope))
            )
        )
    )


def reverted(row: Ownership[bpy.types.World], scene: bpy.types.Scene, identity: Identity) -> Outcome[Option[int]]:
    """Restores replaced world, returning count of removed unused owned worlds, `Nothing` without shown owned world."""
    return (
        Option
        .of_optional(scene.world)
        .bind(lambda world: row.marked(identity, world))
        .map(lambda mark: written(scene, ("world",), mark.of.to_optional()).bind(lambda _: row.removed(identity)).map(Some))
        .default_value(Ok(Nothing))
    )


# --- [SUN]


def followers(tree: bpy.types.NodeTree, identity: Identity) -> Outcome[tuple[tuple[bpy.types.ShaderNodeTexSky | bpy.types.ShaderNodeTexEnvironment, float], ...]]:
    """Returns owned sun-following sky and environment nodes with the heading of the image sun, none without stored environment."""

    def followed(backdrop: Backdrop, source: bpy.types.Node) -> tuple[tuple[bpy.types.ShaderNodeTexSky | bpy.types.ShaderNodeTexEnvironment, float], ...]:
        match source:
            case bpy.types.ShaderNodeTexSky() as sky if backdrop.source == sky.sky_type and backdrop.use_sun:
                return ((sky, 0.0),)
            case bpy.types.ShaderNodeTexEnvironment() as picture if backdrop.source == BackdropSource.HDRI.name and backdrop.use_sun:
                return ((picture, backdrop.heading),)
            case _:
                return ()

    def found(environment: Environment) -> tuple[tuple[bpy.types.ShaderNodeTexSky | bpy.types.ShaderNodeTexEnvironment, float], ...]:
        marked = owned(tree, identity)
        return tuple(follower for slot, backdrop in _slots(environment) for source in Option.of_optional(marked.get(slot.source)).to_list() for follower in followed(backdrop, source))

    return WorldEnvironment.attachment.bound(identity).map(lambda attached: found(attached.struct(tree)) if attached.stored(tree) else ())


# --- [STUDIO]


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(Empty)
def environment_place(call: Call[Empty]) -> Outcome[Done]:
    """Shows the owned environment world on the scene."""
    return rooted(bpy.types.Scene, call.context).bind(lambda scene: swapped(WorldEnvironment.ownership, {}, scene, call.identity)).map(lambda _: Done())


@Operator.of(Empty)
def environment_restore(call: Call[Empty]) -> Outcome[Answer]:
    """Shows the world the owned environment replaced."""
    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: reverted(WorldEnvironment.ownership, scene, call.identity))
        .map(lambda change: change.map(lambda _: Done()).default_value(Unchanged(reason="Scene shows no owned environment")))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Backdrop", "BackdropSource", "Environment", "WorldEnvironment", "environment_place", "environment_restore", "followers", "reverted", "shaders", "swapped"]
