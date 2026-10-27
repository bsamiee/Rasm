"""World environments, graphs, swaps, sun-following nodes, and studio worlds."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from functools import partial, reduce
from types import MappingProxyType
from typing import ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np

from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, Unchanged
from blender.bpy.data.ids.ownership import kept, Mark, Ownership, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.descriptors import assigned, choices, Descriptor, mirrored, RnaPath, Value, written
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.changes import reconciled
from blender.bpy.nodes.graphs import Expr, Graph, Loaded, mixed, Term
from blender.bpy.nodes.trees import emptied, tree_of
from blender.bpy.settings.attachments import Attached, Attachment, reconciling
from blender.bpy.ui.layout.rows import Member, OperatorButton, Property
from blender.bpy.ui.layout.sections import Section, Single
from blender.bpy.ui.layout.values import Hosted, Store
from blender.records.faults import Fault, Outcome
from blender.records.trees import Retention
from blender.rendering.lighting.solar import SunPosition

# --- [TYPES] ----------------------------------------------------------------------------

type Follower = bpy.types.ShaderNodeTexSky | EnvironmentNode

# --- [CONSTANTS] ------------------------------------------------------------------------

_SKY_TYPE: Final = Descriptor.declared(bpy.types.ShaderNodeTexSky, "sky_type", bpy.types.EnumProperty)
_PROJECTION: Final = Descriptor.declared(bpy.types.ShaderNodeTexEnvironment, "projection", bpy.types.EnumProperty)
_RAYS: Final[Mapping[str, str]] = MappingProxyType({"glossy": "Is Glossy Ray", "camera": "Is Camera Ray"})
_SKY: Final = MappingProxyType({
    name: Descriptor.declared(bpy.types.ShaderNodeTexSky, name, bpy.types.FloatProperty).prop
    for name in ("sun_elevation", "sun_rotation", "sun_size", "sun_intensity", "altitude", "air_density", "aerosol_density", "ozone_density", "turbidity", "ground_albedo")
})
_MODES: Final = choices((
    *_SKY_TYPE.prop.enum_items,
    ("HDRI", "HDRI", "Environment image with projection and rotation"),
    ("COLOR", "Color", "Scene-linear world color"),
    ("GRADIENT", "Gradient", "Vertical camera frame gradient, the background slot alone"),
    ("IMAGE", "Image", "Image filling camera frame, the background slot alone"),
))
_TURNS: Final = choices((("FIXED", "Fixed", "Fixed angles"), ("SUN", "Sun Light", "Follow scene sun light")))

# --- [MODELS] ---------------------------------------------------------------------------


class Backdrop(bpy.types.PropertyGroup):
    """Ray slot source, strength, and tint, the source's sky, image, color, and gradient members."""

    mode: bpy.props.EnumProperty(name="Source", description="World source of the slot", items=_MODES, default="COLOR")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    strength: bpy.props.FloatProperty(name="Strength", description="Background strength", min=0.0, soft_max=10.0, default=1.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    tint: bpy.props.FloatVectorProperty(name="Tint", description="Color the source multiplies", subtype="COLOR", size=3, default=(1.0, 1.0, 1.0))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    color: bpy.props.FloatVectorProperty(name="Color", description="Scene-linear world color", subtype="COLOR", size=3, default=(0.0508761, 0.0508761, 0.0508761))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    top: bpy.props.FloatVectorProperty(name="Top", description="Gradient color at frame top", subtype="COLOR", size=3, default=(0.2, 0.2, 0.2))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    bottom: bpy.props.FloatVectorProperty(name="Bottom", description="Gradient color at frame bottom", subtype="COLOR", size=3, default=(0.02, 0.02, 0.02))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    path: bpy.props.StringProperty(name="Image", description="Image file", subtype="FILE_PATH")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    projection: bpy.props.EnumProperty(**mirrored(_PROJECTION.prop))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    turn: bpy.props.EnumProperty(name="Turn", description="Image rotation source", items=_TURNS, default="FIXED")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    rotation: bpy.props.FloatProperty(name="Rotation", description="Fixed image rotation", subtype="ANGLE")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    heading: bpy.props.FloatProperty(name="Sun Heading", description="Heading of the image sun turned onto scene sun", subtype="ANGLE")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    sun: bpy.props.EnumProperty(name="Sun", description="Sky sun source", items=_TURNS, default="FIXED")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    sun_disc: bpy.props.BoolProperty(name="Sun Disc", description="Draw the scattering sky sun disc", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    sun_elevation: bpy.props.FloatProperty(**mirrored(_SKY["sun_elevation"]))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    sun_rotation: bpy.props.FloatProperty(**mirrored(_SKY["sun_rotation"]))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    sun_size: bpy.props.FloatProperty(**mirrored(_SKY["sun_size"]))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    sun_intensity: bpy.props.FloatProperty(**mirrored(_SKY["sun_intensity"]))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    altitude: bpy.props.FloatProperty(**mirrored(_SKY["altitude"]))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    air_density: bpy.props.FloatProperty(**mirrored(_SKY["air_density"]))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    aerosol_density: bpy.props.FloatProperty(**mirrored(_SKY["aerosol_density"]))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    ozone_density: bpy.props.FloatProperty(**mirrored(_SKY["ozone_density"]))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    turbidity: bpy.props.FloatProperty(**mirrored(_SKY["turbidity"]))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    ground_albedo: bpy.props.FloatProperty(**mirrored(_SKY["ground_albedo"]))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Environment(bpy.types.PropertyGroup):
    """World lighting with switched background and reflection overrides."""

    lighting: bpy.props.PointerProperty(type=Backdrop, name="Lighting")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_camera: bpy.props.BoolProperty(name="Background", description="Show another source to the camera")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    camera: bpy.props.PointerProperty(type=Backdrop, name="Background")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_glossy: bpy.props.BoolProperty(name="Reflection", description="Show another source in glossy reflections")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    glossy: bpy.props.PointerProperty(type=Backdrop, name="Reflection")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class EnvironmentNode(msgspec.Struct, frozen=True, kw_only=True):
    """Environment Texture node with image sun heading."""

    node: bpy.types.ShaderNodeTexEnvironment
    heading: float


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
            .bind(lambda tree: WorldEnvironment.attachment.read(identity, tree).bind(world_graph).bind(lambda graph: reconciled(graph, tree, identity)))
            .map(lambda _: Nothing)
        )

    attachment: ClassVar[Attachment[Environment]] = Attachment(key="environment", form=Environment, owner=bpy.types.NodeTree, changed=Some(reconciling(bpy.types.World, refreshed)))
    ownership: ClassVar[Ownership[bpy.types.World]] = Ownership(kind=bpy.types.World, key="environment", saved=Saved.USED)

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns attachment, ownership, and operators."""
        return Ok((self.attachment, self.ownership, environment_place_command, environment_restore_command))

    def section(self) -> Section:
        """Returns environment, world settings, and place and restore buttons."""
        return Single(
            key="environment",
            title="Environment",
            rows=tuple(Property(target=Member(root=bpy.types.Context, path=("scene", "world", member))) for member in ("sun_threshold", "lightgroup")),
            header=(OperatorButton(target=environment_place_command), OperatorButton(target=environment_restore_command)),
            store=Some(Hosted(host=Some(Member(root=bpy.types.Context, path=("scene", "world", "node_tree"))), store=Store(attachment=self.attachment, retention=self.retention))),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [GRAPH]


def _slots(environment: Environment) -> tuple[tuple[str, Key, Backdrop], ...]:
    """Returns lighting, then switched-on glossy and camera slots, with source keys."""
    slots = (("lighting", True, environment.lighting), ("glossy", environment.use_glossy, environment.glossy), ("camera", environment.use_camera, environment.camera))
    return tuple((slot, f"{slot}_source", backdrop) for slot, shown, backdrop in slots if shown)


def world_graph(environment: Environment) -> Outcome[Graph]:
    """Builds world graph from switched-on ray slots, each source node under its slot key."""

    def image(kind: type[bpy.types.Node], key: Key, path: str, inputs: tuple[tuple[str, Term], ...], members: tuple[tuple[str | RnaPath, Value], ...]) -> Expr:
        return Expr(
            kind=kind,
            inputs=inputs,
            members=(*members, *(() if path else (("image", None),))),
            content=Option.of_optional(path or None).map(lambda chosen: Loaded(path=chosen)),
            socket="Color",
            shape="RGBA",
            label=Some(key),
        )

    def sourced(key: Key, backdrop: Backdrop) -> Term:
        match backdrop.mode:
            case "COLOR":
                return (*backdrop.color, 1.0)
            case "HDRI":
                turned = ((("texture_mapping", "rotation"), (0.0, 0.0, backdrop.rotation)),) if backdrop.turn == "FIXED" else ()
                return image(bpy.types.ShaderNodeTexEnvironment, key, backdrop.path, (), (("projection", backdrop.projection), *turned))
            case "GRADIENT":
                window = Expr(kind=bpy.types.ShaderNodeTexCoord, socket="Window", shape="VECTOR")
                return mixed(window["Y"], (*backdrop.bottom, 1.0), (*backdrop.top, 1.0)).named(key)
            case "IMAGE":
                return image(bpy.types.ShaderNodeTexImage, key, backdrop.path, (("Vector", Expr(kind=bpy.types.ShaderNodeTexCoord, socket="Window", shape="VECTOR")),), ())
            case sky_type:
                still = backdrop.sun == "FIXED"
                toward = SunPosition.toward(np.asarray(backdrop.sun_rotation, dtype=np.float64), np.asarray(backdrop.sun_elevation, dtype=np.float64))
                members: dict[str | RnaPath, Value] = {
                    "sky_type": sky_type,
                    ("texture_mapping", "rotation"): (0.0, 0.0, 0.0),
                    "sun_disc": backdrop.sun_disc and still,
                    "sun_size": backdrop.sun_size,
                    "sun_intensity": backdrop.sun_intensity,
                    "altitude": backdrop.altitude,
                    "air_density": backdrop.air_density,
                    "aerosol_density": backdrop.aerosol_density,
                    "ozone_density": backdrop.ozone_density,
                    "turbidity": backdrop.turbidity,
                    "ground_albedo": backdrop.ground_albedo,
                    **({"sun_elevation": backdrop.sun_elevation, "sun_rotation": backdrop.sun_rotation, "sun_direction": tuple(float(value) for value in toward)} if still else {}),
                }
                return Expr(kind=bpy.types.ShaderNodeTexSky, members=tuple(members.items()), socket="Color", shape="RGBA", label=Some(key))

    def background(source: Key, backdrop: Backdrop) -> Expr:
        tinted = mixed(1.0, sourced(source, backdrop), (*backdrop.tint, 1.0), blend="MULTIPLY")
        return Expr(kind=bpy.types.ShaderNodeBackground, inputs=(("Color", tinted), ("Strength", backdrop.strength)), socket="Background")

    def joined(shader: Expr, slot: tuple[str, Key, Backdrop]) -> Expr:
        ray = Expr(kind=bpy.types.ShaderNodeLightPath, socket=_RAYS[slot[0]])
        return Expr(kind=bpy.types.ShaderNodeMixShader, inputs=(("Fac", ray), ("Shader", shader), ("Shader_001", background(*slot[1:]))), socket="Shader")

    (_, key, lighting), *overrides = _slots(environment)
    surface = reduce(joined, overrides, background(key, lighting))
    return Graph.lowered(bpy.types.ShaderNodeTree, Expr(kind=bpy.types.ShaderNodeOutputWorld, inputs=(("Surface", surface),), members=(("is_active_output", True),)))


# --- [HOLDERS]


def swapped(row: Ownership[bpy.types.World], values: Mapping[RnaPath, Value], scene: bpy.types.Scene, identity: Identity, scope: Option[ExitStack] = Nothing) -> Outcome[bpy.types.World]:
    """Writes environment members by path to an owned world and shows it on scene.

    The shown owned world serves first, then the earliest owned world, then a new one, each marking the world it replaces, which scope unwind shows again.

    Returns:
        Shown world, or the faults of binding, writing, and reconciling it.
    """
    predecessor = Option.of_optional(scene.world)
    for outer in scope.to_list():
        outer.callback(written, scene, "world", predecessor.to_optional())

    def displayed(attached: Attached[Environment], world: bpy.types.World) -> Outcome[bpy.types.World]:
        return (
            tree_of(world)
            .bind(lambda tree: assigned(attached.struct(tree), values).bind(lambda _: world_graph(attached.struct(tree))).bind(lambda graph: reconciled(graph, tree, identity)))
            .bind(lambda _: written(scene, "world", world))
            .map(lambda _: world)
        )

    return WorldEnvironment.attachment.bound(identity).bind(
        lambda attached: (
            predecessor
            .filter(lambda world: row.marked(identity, world).is_some())
            .or_else_with(lambda: row.found(identity, of=predecessor) if row.single else Nothing)
            .map(lambda world: displayed(attached, world))
            .default_with(
                lambda: kept(
                    catch(bpy.data.worlds.new, exception=RuntimeError)(f"{scene.name} {bpy.path.display_name(row.key)}").map_error(partial(Fault.of, BlenderReason.REFUSED, row.key)), emptied
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
        .map(lambda mark: written(scene, "world", mark.of.to_optional()).bind(lambda _: row.removed(identity)).map(Some))
        .default_value(Ok(Nothing))
    )


# --- [SUN]


def followers(tree: bpy.types.NodeTree, identity: Identity) -> Outcome[tuple[Follower, ...]]:
    """Returns owned sun-following sky and environment nodes, none without stored environment."""

    def followed(backdrop: Backdrop, source: bpy.types.Node) -> tuple[Follower, ...]:
        match source:
            case bpy.types.ShaderNodeTexSky() as sky if backdrop.mode == sky.sky_type and backdrop.sun == "SUN":
                return (sky,)
            case bpy.types.ShaderNodeTexEnvironment() as picture if backdrop.mode == "HDRI" and backdrop.turn == "SUN":
                return (EnvironmentNode(node=picture, heading=backdrop.heading),)
            case _:
                return ()

    def found(environment: Environment) -> tuple[Follower, ...]:
        marked = {mark.key: source for source in tree.nodes for mark in Mark.read(identity, source).to_list()}
        return tuple(follower for _, key, backdrop in _slots(environment) for source in Option.of_optional(marked.get(key)).to_list() for follower in followed(backdrop, source))

    return WorldEnvironment.attachment.bound(identity).map(lambda attached: found(attached.struct(tree)) if attached.stored(tree) else ())


# --- [STUDIO]


def studio(name: str, preferences: bpy.types.Preferences) -> Outcome[str]:
    """Returns path of WORLD studio light by name."""
    lights = preferences.studio_lights
    return (
        Option.of_optional(lights.get(name)).filter(lambda light: light.type == "WORLD").map(lambda light: light.path).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(lights), member=name))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def environment_place(call: Call[Empty]) -> Outcome[Done]:
    """Shows the owned environment world on the scene."""
    return call.root(bpy.types.Scene).bind(lambda scene: swapped(WorldEnvironment.ownership, {}, scene, call.identity)).map(lambda _: Done())


def environment_restore(call: Call[Empty]) -> Outcome[Answer]:
    """Shows the world the owned environment replaced."""
    return (
        call
        .root(bpy.types.Scene)
        .bind(lambda scene: reverted(WorldEnvironment.ownership, scene, call.identity))
        .map(lambda change: change.map(lambda _: Done()).default_value(Unchanged(reason="Scene shows no owned environment")))
    )


environment_place_command: Operator[Empty] = Operator(form=Empty, run=environment_place)
environment_restore_command: Operator[Empty] = Operator(form=Empty, run=environment_restore)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Backdrop",
    "Environment",
    "EnvironmentNode",
    "Follower",
    "WorldEnvironment",
    "environment_place_command",
    "environment_restore_command",
    "followers",
    "reverted",
    "studio",
    "swapped",
    "world_graph",
]
