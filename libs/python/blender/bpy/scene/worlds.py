"""World environments, graphs, swaps, sun-following nodes, and studio worlds."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from functools import reduce
import math
from types import MappingProxyType
from typing import Annotated, ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np

from blender.bpy.commands.operators import Call, Done, Empty, Operator, Unchanged
from blender.bpy.data.ids.ownership import kept, Mark, Ownership, Saved
from blender.bpy.data.ids.store import Handle, vacant
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderResult
from blender.bpy.nodes.changes import reconciled
from blender.bpy.nodes.graphs import Expr, Graph, Loaded, mixed, Term
from blender.bpy.nodes.trees import emptied, tree_of
from blender.bpy.settings.attachments import Attached, Attachment, reconciling
from blender.bpy.settings.history import Retention
from blender.bpy.settings.properties import Choices, Definition
from blender.bpy.ui.layout.rows import Member, OperatorButton, Property
from blender.bpy.ui.layout.sections import Section, Single
from blender.bpy.ui.layout.values import Hosted, Store
from blender.imaging.parameters import Case, Finite, Gated, heading, NonNegative, Presentation, Quantity, Rgb
from blender.rendering.lighting.solar import SunPosition

# --- [TYPES] ----------------------------------------------------------------------------

type SkySun = Still | Solar
type ImageTurn = FixedTurn | SolarTurn
type WorldMode = Hdri | Scattering | Preetham | HosekWilkie | FlatColor
type Scenery = WorldMode | FrameGradient | Plate
type Follower = bpy.types.ShaderNodeTexSky | EnvironmentNode

# --- [CONSTANTS] ------------------------------------------------------------------------

_SKY: Final = MappingProxyType({
    name: Descriptor.declared(bpy.types.ShaderNodeTexSky, name, bpy.types.FloatProperty)
    for name in ("sun_size", "sun_intensity", "altitude", "air_density", "aerosol_density", "ozone_density", "turbidity", "ground_albedo")
})
_SKY_TYPE: Final = Descriptor.declared(bpy.types.ShaderNodeTexSky, "sky_type", bpy.types.EnumProperty)
_PROJECTION: Final = Descriptor.declared(bpy.types.ShaderNodeTexEnvironment, "projection", bpy.types.EnumProperty)
_RAYS: Final[Mapping[str, str]] = MappingProxyType({"glossy": "Is Glossy Ray", "camera": "Is Camera Ray"})

# --- [MODELS] ---------------------------------------------------------------------------


class Still(Case, frozen=True, kw_only=True, tag=1):
    """Sun at fixed scene angles."""

    sun_elevation: Annotated[Finite, msgspec.Meta(title="Elevation"), Presentation(quantity=Some(Quantity.ANGLE))] = math.radians(15.0)
    sun_rotation: Annotated[Finite, msgspec.Meta(title="Rotation"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0


class Solar(Case, frozen=True, kw_only=True, tag=2):
    """Sky following scene sun light."""

    title = "Sun Light"


class Disc(msgspec.Struct, frozen=True, kw_only=True):
    """Scattering sky sun disc."""

    sun_size: Annotated[float, _SKY["sun_size"].meta, _SKY["sun_size"].presentation] = _SKY["sun_size"].prop.default
    sun_intensity: Annotated[float, _SKY["sun_intensity"].meta, _SKY["sun_intensity"].presentation] = _SKY["sun_intensity"].prop.default


class Scattering(Case, frozen=True, kw_only=True, tag=2):
    """Scattering sky."""

    title = "Sky"
    sky_type: Annotated[str, msgspec.Meta(title="Model"), Choices.mirrored(_SKY_TYPE.prop, lambda identifier: identifier in {"SINGLE_SCATTERING", "MULTIPLE_SCATTERING"})] = "MULTIPLE_SCATTERING"
    sun: SkySun = Still()
    disc: Annotated[Gated[Disc], msgspec.Meta(title="Sun Disc")] = Gated(enabled=True, value=Disc())
    altitude: Annotated[float, _SKY["altitude"].meta, _SKY["altitude"].presentation] = _SKY["altitude"].prop.default
    air_density: Annotated[float, _SKY["air_density"].meta, _SKY["air_density"].presentation] = _SKY["air_density"].prop.default
    aerosol_density: Annotated[float, _SKY["aerosol_density"].meta, _SKY["aerosol_density"].presentation] = _SKY["aerosol_density"].prop.default
    ozone_density: Annotated[float, _SKY["ozone_density"].meta, _SKY["ozone_density"].presentation] = _SKY["ozone_density"].prop.default


class Analytic(Case, frozen=True, kw_only=True):
    """Analytic sky model over sun and turbidity."""

    sky_type: ClassVar[str]
    sun: SkySun = Still()
    turbidity: Annotated[float, _SKY["turbidity"].meta, _SKY["turbidity"].presentation] = _SKY["turbidity"].prop.default


class Preetham(Analytic, frozen=True, kw_only=True, tag=3):
    """Preetham analytic sky."""

    sky_type = "PREETHAM"


class HosekWilkie(Analytic, frozen=True, kw_only=True, tag=4):
    """Hosek-Wilkie analytic sky."""

    title = "Hosek / Wilkie"
    sky_type = "HOSEK_WILKIE"
    ground_albedo: Annotated[float, _SKY["ground_albedo"].meta, _SKY["ground_albedo"].presentation] = _SKY["ground_albedo"].prop.default


class FixedTurn(Case, frozen=True, kw_only=True, tag=1):
    """Fixed image rotation."""

    title = "Fixed"
    rotation: Annotated[Finite, Presentation(quantity=Some(Quantity.ANGLE))] = 0.0


class SolarTurn(Case, frozen=True, kw_only=True, tag=2):
    """Image turned to align its sun with scene sun."""

    title = "Sun Light"
    heading: Annotated[Finite, msgspec.Meta(title="Sun Heading"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0


class Hdri(Case, frozen=True, kw_only=True, tag=1):
    """Environment image with projection and rotation."""

    title = "HDRI"
    path: Annotated[str, msgspec.Meta(title="Image"), Definition(text="FILE_PATH")] = ""
    projection: Annotated[str, Choices.mirrored(_PROJECTION.prop)] = _PROJECTION.prop.default
    turn: ImageTurn = FixedTurn()


class FlatColor(Case, frozen=True, kw_only=True, tag=5):
    """Scene-linear world color."""

    title = "Color"
    color: Rgb[Finite] = Rgb(red=0.0508761, green=0.0508761, blue=0.0508761)


class FrameGradient(Case, frozen=True, kw_only=True, tag=6):
    """Vertical camera frame gradient."""

    title = "Gradient"
    top: Rgb[Finite] = Rgb(red=0.2, green=0.2, blue=0.2)
    bottom: Rgb[Finite] = Rgb(red=0.02, green=0.02, blue=0.02)


class Plate(Case, frozen=True, kw_only=True, tag=7):
    """Image filling camera frame."""

    title = "Image"
    path: Annotated[str, msgspec.Meta(title="Image"), Definition(text="FILE_PATH")] = ""


class Backdrop[T](msgspec.Struct, frozen=True, kw_only=True):
    """Ray slot source, strength, and tint."""

    mode: T
    strength: Annotated[NonNegative, Presentation(soft=Some((0.0, 10.0)))] = 1.0
    tint: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)


class Environment(msgspec.Struct, frozen=True, kw_only=True):
    """World lighting with gated camera and glossy overrides."""

    lighting: Backdrop[WorldMode] = Backdrop(mode=FlatColor())
    camera: Annotated[Gated[Backdrop[Scenery]], msgspec.Meta(title="Background")] = Gated(enabled=False, value=Backdrop(mode=FlatColor()))
    glossy: Annotated[Gated[Backdrop[WorldMode]], msgspec.Meta(title="Reflection")] = Gated(enabled=False, value=Backdrop(mode=FlatColor()))


class EnvironmentNode(msgspec.Struct, frozen=True, kw_only=True):
    """Environment Texture node with image sun heading."""

    node: bpy.types.ShaderNodeTexEnvironment
    heading: float


class WorldEnvironment(msgspec.Struct, frozen=True, kw_only=True):
    """Environment attachment, owned worlds, operators, and section."""

    retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing

    @staticmethod
    def refreshed(world: Handle[bpy.types.World], identity: Identity) -> BlenderResult[Option[str]]:
        """Reconciles world tree to stored environment, naming no undo step."""
        return (
            world
            .resolve()
            .bind(tree_of)
            .bind(lambda tree: WorldEnvironment.attachment.read(identity, tree).bind(world_graph).bind(lambda graph: reconciled(graph, tree, identity)))
            .map(lambda _: Nothing)
        )

    @staticmethod
    def placed(call: Call[Empty], scene: bpy.types.Scene) -> BlenderResult[Done]:
        """Shows owned environment world on scene, defaults when none is stored."""
        identity, row = call.identity, WorldEnvironment.ownership
        return (
            _shown(row, scene, identity)
            .bind(lambda world: Option.of_optional(world.node_tree))
            .map(lambda tree: WorldEnvironment.attachment.read(identity, tree))
            .default_with(lambda: Ok(Environment()))
            .bind(lambda environment: world_graph(environment).map(lambda graph: (environment, graph)))
            .bind(lambda prepared: swapped(row, prepared, scene, identity))
            .map(lambda _: Done())
        )

    attachment: ClassVar[Attachment[Environment]] = Attachment(key="environment", form=Environment, owner=bpy.types.NodeTree, changed=Some(reconciling(bpy.types.World, refreshed)))
    ownership: ClassVar[Ownership[bpy.types.World]] = Ownership(kind=bpy.types.World, key="environment", saved=Saved.USED)
    place: ClassVar[Operator[Empty]] = Operator(
        key="environment_place",
        title="Show Environment",
        description="Show owned environment world on scene",
        form=Empty,
        run=lambda call: call.root(bpy.types.Scene).bind(lambda scene: WorldEnvironment.placed(call, scene)),
    )
    restore: ClassVar[Operator[Empty]] = Operator(
        key="environment_restore",
        title="Restore World",
        description="Show world the owned environment replaced",
        form=Empty,
        run=lambda call: (
            call
            .root(bpy.types.Scene)
            .bind(lambda scene: reverted(WorldEnvironment.ownership, scene, call.identity))
            .map(lambda change: change.map(lambda _: Done()).default_value(Unchanged(reason="Scene shows no owned environment")))
        ),
    )

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns attachment, ownership, and operators."""
        return Ok((self.attachment, self.ownership, self.place, self.restore))

    def section(self) -> Section:
        """Returns environment, world settings, and place and restore buttons."""
        return Single(
            key="environment",
            title="Environment",
            rows=tuple(Property(target=Member(root=bpy.types.Context, path=f"scene.world.{member}")) for member in ("sun_threshold", "lightgroup")),
            header=(OperatorButton(target=self.place), OperatorButton(target=self.restore)),
            store=Some(Hosted(host=Some(Member(root=bpy.types.Context, path="scene.world.node_tree")), store=Store(attachment=self.attachment, retention=self.retention))),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [GRAPH]


def _slots(environment: Environment) -> tuple[tuple[str, Key, Backdrop[WorldMode] | Backdrop[Scenery]], ...]:
    """Returns lighting, then enabled glossy and camera slots, with source keys."""
    slots: tuple[tuple[str, bool, Backdrop[WorldMode] | Backdrop[Scenery]], ...] = (
        ("lighting", True, environment.lighting),
        ("glossy", environment.glossy.enabled, environment.glossy.value),
        ("camera", environment.camera.enabled, environment.camera.value),
    )
    return tuple((slot, f"{slot}_source", backdrop) for slot, enabled, backdrop in slots if enabled)


def world_graph(environment: Environment) -> BlenderResult[Graph]:
    """Builds world graph from enabled ray slots, each source node under its slot key."""

    def image(kind: type[bpy.types.Node], key: Key, path: str, inputs: tuple[tuple[str, Term], ...], members: tuple[tuple[str, Value], ...]) -> Expr:
        return Expr(
            kind=kind,
            inputs=inputs,
            members=(*members, *(() if path else (("image", None),))),
            content=Option.of_optional(path or None).map(lambda chosen: Loaded(path=chosen)),
            socket="Color",
            shape="RGBA",
            label=Some(key),
        )

    def sourced(key: Key, mode: Scenery) -> Term:
        match mode:
            case FlatColor(color=color):
                return (*msgspec.structs.astuple(color), 1.0)
            case Scattering() | Analytic():
                members: dict[str, Value] = {
                    **{field.name: value for field in msgspec.structs.fields(mode) if isinstance(value := getattr(mode, field.name), float)},
                    "sky_type": mode.sky_type,
                    "texture_mapping.rotation": (0.0, 0.0, 0.0),
                }
                match mode.sun:
                    case Still(sun_elevation=elevation, sun_rotation=rotation):
                        toward = SunPosition.toward(np.asarray(rotation, dtype=np.float64), np.asarray(elevation, dtype=np.float64))
                        members |= {"sun_elevation": elevation, "sun_rotation": rotation, "sun_direction": tuple(float(value) for value in toward)}
                    case Solar():
                        pass
                if isinstance(mode, Scattering):
                    members |= {"sun_disc": mode.disc.enabled and isinstance(mode.sun, Still)} | msgspec.structs.asdict(mode.disc.value)
                return Expr(kind=bpy.types.ShaderNodeTexSky, members=tuple(members.items()), socket="Color", shape="RGBA", label=Some(key))
            case Hdri(path=path, projection=projection, turn=turn):
                turned = (("texture_mapping.rotation", (0.0, 0.0, turn.rotation)),) if isinstance(turn, FixedTurn) else ()
                return image(bpy.types.ShaderNodeTexEnvironment, key, path, (), (("projection", projection), *turned))
            case FrameGradient(top=top, bottom=bottom):
                window = Expr(kind=bpy.types.ShaderNodeTexCoord, socket="Window", shape="VECTOR")
                return mixed(window["Y"], (*msgspec.structs.astuple(bottom), 1.0), (*msgspec.structs.astuple(top), 1.0)).named(key)
            case Plate(path=path):
                return image(bpy.types.ShaderNodeTexImage, key, path, (("Vector", Expr(kind=bpy.types.ShaderNodeTexCoord, socket="Window", shape="VECTOR")),), ())

    def background(source: Key, backdrop: Backdrop[WorldMode] | Backdrop[Scenery]) -> Expr:
        tinted = mixed(1.0, sourced(source, backdrop.mode), (*msgspec.structs.astuple(backdrop.tint), 1.0), blend="MULTIPLY")
        return Expr(kind=bpy.types.ShaderNodeBackground, inputs=(("Color", tinted), ("Strength", backdrop.strength)), socket="Background")

    def joined(shader: Expr, slot: tuple[str, Key, Backdrop[WorldMode] | Backdrop[Scenery]]) -> Expr:
        ray = Expr(kind=bpy.types.ShaderNodeLightPath, socket=_RAYS[slot[0]])
        return Expr(kind=bpy.types.ShaderNodeMixShader, inputs=(("Fac", ray), ("Shader", shader), ("Shader_001", background(*slot[1:]))), socket="Shader")

    (_, key, lighting), *overrides = _slots(environment)
    surface = reduce(joined, overrides, background(key, lighting))
    return Graph.lowered(bpy.types.ShaderNodeTree, Expr(kind=bpy.types.ShaderNodeOutputWorld, inputs=(("Surface", surface),), members=(("is_active_output", True),)))


# --- [HOLDERS]


def swapped(row: Ownership[bpy.types.World], prepared: tuple[Environment, Graph], scene: bpy.types.Scene, identity: Identity, scope: Option[ExitStack] = Nothing) -> BlenderResult[bpy.types.World]:
    """Shows owned environment world on scene and marks replaced world, restored on scope unwind."""
    environment, graph = prepared
    predecessor = Option.of_optional(scene.world)
    for outer in scope.to_list():
        outer.callback(written, scene, "world", predecessor.to_optional())

    def displayed(attached: Attached[Environment], world: bpy.types.World) -> BlenderResult[bpy.types.World]:
        return tree_of(world).bind(lambda tree: attached.write(tree, environment).bind(lambda _: reconciled(graph, tree, identity))).bind(lambda _: written(scene, "world", world)).map(lambda _: world)

    return WorldEnvironment.attachment.bound(identity).bind(
        lambda attached: (
            _shown(row, scene, identity)
            .map(lambda world: displayed(attached, world))
            .default_with(
                lambda: kept(BlenderEditFault.attempted(row.key, lambda: bpy.data.worlds.new(vacant(f"{scene.name} {heading(row.key)}", bpy.data.worlds.keys()))), emptied).bind(
                    lambda world: row.owned(identity, world, of=predecessor, filled=lambda world: displayed(attached, world), scope=scope)
                )
            )
        )
    )


def reverted(row: Ownership[bpy.types.World], scene: bpy.types.Scene, identity: Identity) -> BlenderResult[Option[int]]:
    """Restores replaced world, returning count of removed unused owned worlds, `Nothing` without shown owned world."""
    return (
        Option
        .of_optional(scene.world)
        .bind(lambda world: row.marked(identity, world))
        .map(lambda mark: written(scene, "world", mark.of.to_optional()).bind(lambda _: row.removed(identity)).map(Some))
        .default_value(Ok(Nothing))
    )


def _shown(row: Ownership[bpy.types.World], scene: bpy.types.Scene, identity: Identity) -> Option[bpy.types.World]:
    """Returns shown owned world, else earliest owned world replacing shown world."""
    shown = Option.of_optional(scene.world)
    return shown.filter(lambda world: row.marked(identity, world).is_some()).or_else_with(lambda: row.found(identity, of=shown) if row.single else Nothing)


# --- [SUN]


def followers(tree: bpy.types.NodeTree, identity: Identity) -> BlenderResult[tuple[Follower, ...]]:
    """Returns owned sun-following sky and environment nodes, none without stored environment."""

    def followed(mode: Scenery, source: bpy.types.Node) -> tuple[Follower, ...]:
        match mode, source:
            case Scattering(sun=Solar()) | Analytic(sun=Solar()), bpy.types.ShaderNodeTexSky() as sky:
                return (sky,)
            case Hdri(turn=SolarTurn(heading=heading)), bpy.types.ShaderNodeTexEnvironment() as image:
                return (EnvironmentNode(node=image, heading=heading),)
            case _:
                return ()

    def found(environment: Environment) -> tuple[Follower, ...]:
        marked = {mark.key: source for source in tree.nodes for mark in Mark.read(identity, source).to_list()}
        return tuple(follower for _, key, backdrop in _slots(environment) for source in Option.of_optional(marked.get(key)).to_list() for follower in followed(backdrop.mode, source))

    return WorldEnvironment.attachment.bound(identity).bind(lambda attached: attached.read(tree).map(found) if attached.stored(tree) else Ok(()))


# --- [STUDIO]


def studio(name: str, preferences: bpy.types.Preferences) -> BlenderResult[str]:
    """Returns path of WORLD studio light by name."""
    lights = preferences.studio_lights
    return Option.of_optional(lights.get(name)).filter(lambda light: light.type == "WORLD").map(lambda light: light.path).to_result_with(lambda: (BlenderAbsentFault(owner=repr(lights), key=name),))


# --- [CHOICES] --------------------------------------------------------------------------

studios = Choices.named(
    lambda _identity, _owner, context: (
        light.name
        for context in Option.of_optional(context).to_list()
        for light in context.preferences.studio_lights  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        if light.type == "WORLD"
    )
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Analytic",
    "Backdrop",
    "Disc",
    "Environment",
    "EnvironmentNode",
    "FixedTurn",
    "FlatColor",
    "Follower",
    "FrameGradient",
    "Hdri",
    "HosekWilkie",
    "ImageTurn",
    "Plate",
    "Preetham",
    "Scattering",
    "Scenery",
    "SkySun",
    "Solar",
    "SolarTurn",
    "Still",
    "WorldEnvironment",
    "WorldMode",
    "followers",
    "reverted",
    "studio",
    "studios",
    "swapped",
    "world_graph",
]
