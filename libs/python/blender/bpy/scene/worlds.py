"""World environments, native graphs, owned swaps, solar followers, and studio worlds."""

from collections.abc import Callable
from contextlib import ExitStack
from itertools import starmap
import math
import sys
from typing import Annotated, ClassVar

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np

from blender.bpy.commands.operators import Call, Done, Empty, Operator, Unchanged
from blender.bpy.data.ids.ownership import earliest, Mark, Ownership, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.descriptors import Value, written
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderResult
from blender.bpy.nodes.changes import reconciled
from blender.bpy.nodes.graphs import End, ended, Fragment, Graph, Loaded, mixed, node, Operand
from blender.bpy.nodes.trees import tree_of
from blender.bpy.settings.attachments import Attachment, reconciling
from blender.bpy.settings.properties import Choices, Definition
from blender.bpy.ui.layout.rows import Member, OperatorButton, Property
from blender.bpy.ui.layout.sections import Hosted, Section, Single, Store
from blender.bpy.ui.surfaces.panels import enumerated
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Case, Finite, Gated, Presentation, Quantity, Rgb
from blender.rendering.history import Retention
from blender.rendering.lighting.solar import SunPosition

# --- [TYPES] ----------------------------------------------------------------------------

type SkySun = Still | Solar
type ImageTurn = FixedTurn | SolarTurn
type WorldMode = Hdri | Scattering | Preetham | HosekWilkie | FlatColor
type Scenery = WorldMode | FrameGradient | Plate
type Follower = bpy.types.ShaderNodeTexSky | EnvironmentNode

# --- [METADATA] -------------------------------------------------------------------------

sky_properties = {prop.identifier: prop for prop in bpy.types.ShaderNodeTexSky.bl_rna.properties if isinstance(prop, bpy.types.FloatProperty)}

# --- [MODELS] ---------------------------------------------------------------------------


class Still(Case, frozen=True, kw_only=True, tag=1):
    """Sun at fixed scene angles."""

    title = "Still"
    sun_elevation: Annotated[Finite, msgspec.Meta(title="Elevation"), Presentation(quantity=Some(Quantity.ANGLE))] = math.radians(15.0)
    sun_rotation: Annotated[Finite, msgspec.Meta(title="Rotation"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0


class Solar(Case, frozen=True, kw_only=True, tag=2):
    """Sky driven by the scene's sun lamp."""

    title = "Sun Lamp"


class Disc(msgspec.Struct, frozen=True, kw_only=True):
    """Native scattering sun size and intensity."""

    sun_size: Annotated[float, msgspec.Meta(ge=sky_properties["sun_size"].hard_min, le=sky_properties["sun_size"].hard_max, title="Size"), Presentation(quantity=Some(Quantity.ANGLE))] = (
        sky_properties["sun_size"].default
    )
    sun_intensity: Annotated[float, msgspec.Meta(ge=sky_properties["sun_intensity"].hard_min, le=sky_properties["sun_intensity"].hard_max, title="Intensity")] = sky_properties["sun_intensity"].default


class Scattering(Case, frozen=True, kw_only=True, tag=2):
    """Native scattering sky and atmosphere."""

    title = "Sky"
    sky_type: Annotated[
        str,
        msgspec.Meta(title="Model"),
        Choices(lambda *_: (row for row in enumerated(bpy.types.ShaderNodeTexSky.bl_rna.properties["sky_type"]) if row[0] in {"SINGLE_SCATTERING", "MULTIPLE_SCATTERING"})),
    ] = "MULTIPLE_SCATTERING"
    sun: SkySun = Still()
    disc: Annotated[Gated[Disc], msgspec.Meta(title="Sun Disc")] = Gated(enabled=True, value=Disc())
    altitude: Annotated[float, msgspec.Meta(ge=sky_properties["altitude"].hard_min, le=sky_properties["altitude"].hard_max), Presentation(quantity=Some(Quantity.LENGTH))] = sky_properties[
        "altitude"
    ].default
    air_density: Annotated[float, msgspec.Meta(ge=sky_properties["air_density"].hard_min, le=sky_properties["air_density"].hard_max, title="Air")] = sky_properties["air_density"].default
    aerosol_density: Annotated[float, msgspec.Meta(ge=sky_properties["aerosol_density"].hard_min, le=sky_properties["aerosol_density"].hard_max, title="Aerosols")] = sky_properties[
        "aerosol_density"
    ].default
    ozone_density: Annotated[float, msgspec.Meta(ge=sky_properties["ozone_density"].hard_min, le=sky_properties["ozone_density"].hard_max, title="Ozone")] = sky_properties["ozone_density"].default


class Preetham(Case, frozen=True, kw_only=True, tag=3):
    """Preetham's analytic sky."""

    title = "Preetham"
    sun: SkySun = Still()
    turbidity: Annotated[float, msgspec.Meta(ge=sky_properties["turbidity"].hard_min, le=sky_properties["turbidity"].hard_max)] = 2.2


class HosekWilkie(Case, frozen=True, kw_only=True, tag=4):
    """Hosek and Wilkie's analytic sky with ground light."""

    title = "Hosek / Wilkie"
    sun: SkySun = Still()
    turbidity: Annotated[float, msgspec.Meta(ge=sky_properties["turbidity"].hard_min, le=sky_properties["turbidity"].hard_max)] = 2.2
    ground_albedo: Annotated[float, msgspec.Meta(ge=sky_properties["ground_albedo"].hard_min, le=sky_properties["ground_albedo"].hard_max)] = 0.3


class FixedTurn(Case, frozen=True, kw_only=True, tag=1):
    """Fixed environment-image rotation."""

    title = "Fixed"
    rotation: Annotated[Finite, Presentation(quantity=Some(Quantity.ANGLE))] = 0.0


class SolarTurn(Case, frozen=True, kw_only=True, tag=2):
    """Image sun aligned with the sun lamp."""

    title = "Sun Lamp"
    heading: Annotated[Finite, msgspec.Meta(title="Sun Heading"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0


class Hdri(Case, frozen=True, kw_only=True, tag=1):
    """Environment image with native projection and rotation."""

    title = "HDRI"
    path: Annotated[str, msgspec.Meta(title="Image"), Definition(text="FILE_PATH")] = ""
    projection: Annotated[str, Choices(lambda *_: enumerated(bpy.types.ShaderNodeTexEnvironment.bl_rna.properties["projection"]))] = "EQUIRECTANGULAR"
    turn: ImageTurn = FixedTurn()


class FlatColor(Case, frozen=True, kw_only=True, tag=5):
    """One scene-linear world color."""

    title = "Color"
    color: Rgb[Finite] = Rgb(red=0.0508761, green=0.0508761, blue=0.0508761)


class FrameGradient(Case, frozen=True, kw_only=True, tag=6):
    """Vertical camera-frame gradient."""

    title = "Gradient"
    top: Rgb[Finite] = Rgb(red=0.2, green=0.2, blue=0.2)
    bottom: Rgb[Finite] = Rgb(red=0.02, green=0.02, blue=0.02)


class Plate(Case, frozen=True, kw_only=True, tag=7):
    """Image filling the camera frame."""

    title = "Image"
    path: Annotated[str, msgspec.Meta(title="Image"), Definition(text="FILE_PATH")] = ""


class Backdrop[T](msgspec.Struct, frozen=True, kw_only=True):
    """One ray slot's source, strength, and tint."""

    mode: T
    strength: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max), Presentation(soft=Some((0.0, 10.0)))] = 1.0
    tint: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)


class Environment(msgspec.Struct, frozen=True, kw_only=True):
    """World lighting and retained camera and glossy overrides."""

    lighting: Backdrop[WorldMode] = Backdrop(mode=FlatColor())
    camera: Annotated[Gated[Backdrop[Scenery]], msgspec.Meta(title="Background")] = Gated(enabled=False, value=Backdrop(mode=FlatColor()))
    glossy: Annotated[Gated[Backdrop[WorldMode]], msgspec.Meta(title="Reflection")] = Gated(enabled=False, value=Backdrop(mode=FlatColor()))


class EnvironmentNode(msgspec.Struct, frozen=True, kw_only=True):
    """Environment node and the heading of the sun in its image."""

    node: bpy.types.ShaderNodeTexEnvironment
    heading: float


class WorldEnvironment(msgspec.Struct, frozen=True, kw_only=True):
    """Environment attachment, owned worlds, commands, and section."""

    retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing

    @staticmethod
    def refreshed(world: Handle[bpy.types.World], identity: Identity) -> BlenderResult[Option[str]]:
        """Reconciles the latest stored record without adding an undo step."""
        return (
            paired(world.resolve().bind(tree_of), WorldEnvironment.attachment.bound(identity))
            .bind(lambda pair: pair[1].read(pair[0]).bind(world_graph).bind(lambda graph: reconciled(graph, pair[0], identity)))
            .map(lambda _: Nothing)
        )

    @staticmethod
    def placed(call: Call[Empty], scene: bpy.types.Scene) -> BlenderResult[Done]:
        """Shows the retained owned environment or a new default environment."""
        identity, row = call.identity, WorldEnvironment.ownership
        held = row.held(identity)
        prior = Option.of_optional(scene.world)
        owned = prior.filter(lambda world: any(block == world for block, _ in held)).or_else(Option.of_optional(earliest(held).get(Mark(key=row.key, of=prior))))
        return (
            WorldEnvironment.attachment
            .bound(identity)
            .bind(lambda attached: owned.bind(lambda world: Option.of_optional(world.node_tree)).map(attached.read).default_with(lambda: Ok(Environment())))
            .bind(lambda environment: world_graph(environment).map(lambda graph: (environment, graph)))
            .bind(lambda prepared: swapped(row, prepared, scene, identity))
            .map(lambda _: Done())
        )

    attachment: ClassVar[Attachment[Environment]] = Attachment(key="environment", form=Environment, owner=bpy.types.NodeTree, changed=Some(reconciling(bpy.types.World, refreshed)))
    ownership: ClassVar[Ownership[bpy.types.World]] = Ownership(kind=bpy.types.World, key="environment", saved=Saved.USED)
    place: ClassVar[Operator[Empty]] = Operator(
        key="environment_place",
        title="Show Environment",
        description="Show an owned world holding an environment on the scene",
        form=Empty,
        run=lambda call: call.root(bpy.types.Scene).bind(lambda scene: WorldEnvironment.placed(call, scene)),
    )
    restore: ClassVar[Operator[Empty]] = Operator(
        key="environment_restore",
        title="Restore World",
        description="Show the world the owned environment replaced",
        form=Empty,
        run=lambda call: (
            call
            .root(bpy.types.Scene)
            .bind(lambda scene: reverted(WorldEnvironment.ownership, scene, call.identity))
            .map(lambda change: change.map(lambda _: Done()).default_value(Unchanged(reason="The scene shows no owned environment")))
        ),
    )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns the same declarations consumed by the section."""
        return Ok((self.attachment, self.ownership, self.place, self.restore))

    def section(self) -> Section:
        """Draws the environment, native world controls, and retained commands."""
        return Single(
            key="environment",
            title="Environment",
            rows=tuple(Property(target=Member(root=bpy.types.Context, path=f"scene.world.{member}")) for member in ("sun_threshold", "lightgroup")),
            header=(OperatorButton(target=self.place), OperatorButton(target=self.restore)),
            store=Some(Hosted(host=Member(root=bpy.types.Context, path="scene.world.node_tree"), store=Store(attachment=self.attachment, retention=self.retention))),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [GRAPH]


def _slots(environment: Environment) -> tuple[tuple[str, str, Backdrop[WorldMode] | Backdrop[Scenery]], ...]:
    """Returns lighting, then enabled glossy and camera slots with their source keys."""
    slots: tuple[tuple[str, bool, Backdrop[WorldMode] | Backdrop[Scenery]], ...] = (
        ("lighting", True, environment.lighting),
        ("glossy", environment.glossy.enabled, environment.glossy.value),
        ("camera", environment.camera.enabled, environment.camera.value),
    )
    return tuple((slot, f"{slot}_source", backdrop) for slot, enabled, backdrop in slots if enabled)


def world_graph(environment: Environment) -> BlenderResult[Graph]:
    """Derives the enabled ray slots and returns every independent construction fault."""

    def sourced(key: str, mode: Scenery) -> BlenderResult[Operand]:
        match mode:
            case FlatColor(color=color):
                return Ok((color.red, color.green, color.blue, 1.0))
            case Scattering() | Preetham() | HosekWilkie():
                members: dict[str, Value] = {field.name: value for field in msgspec.structs.fields(type(mode)) if isinstance(value := getattr(mode, field.name), float)}
                members.update({
                    "sky_type": mode.sky_type if isinstance(mode, Scattering) else "PREETHAM" if isinstance(mode, Preetham) else "HOSEK_WILKIE",
                    "texture_mapping.rotation": (0.0, 0.0, 0.0),
                })
                match mode.sun:
                    case Still(sun_elevation=elevation, sun_rotation=rotation):
                        members.update({
                            "sun_elevation": elevation,
                            "sun_rotation": rotation,
                            "sun_direction": tuple(
                                float(value) for value in SunPosition(azimuth=np.asarray(rotation, dtype=np.float64), elevation=np.asarray(elevation, dtype=np.float64), north=0.0).vectors
                            ),
                        })
                    case Solar():
                        pass
                if isinstance(mode, Scattering):
                    members.update({"sun_disc": mode.disc.enabled if isinstance(mode.sun, Still) else False, **msgspec.structs.asdict(mode.disc.value)})
                return node(key, bpy.types.ShaderNodeTexSky, {}, members).map(ended(key, "Color"))
            case Hdri(path=path, projection=projection, turn=turn):
                return node(
                    key,
                    bpy.types.ShaderNodeTexEnvironment,
                    {},
                    {"projection": projection, **({"texture_mapping.rotation": (0.0, 0.0, turn.rotation)} if isinstance(turn, FixedTurn) else {}), **({} if path else {"image": None})},
                    Option.of_optional(path or None).map(lambda chosen: Loaded(path=chosen)),
                ).map(ended(key, "Color"))
            case FrameGradient(top=top, bottom=bottom):
                window, split = f"{key}_window", f"{key}_split"
                return (
                    node(window, bpy.types.ShaderNodeTexCoord, {}, {})
                    .map(ended(window, "Window"))
                    .bind(lambda coordinate: node(split, bpy.types.ShaderNodeSeparateXYZ, {"Vector": coordinate}, {}).map(ended(split, "Y")))
                    .bind(lambda factor: mixed(key, "RGBA", factor, (bottom.red, bottom.green, bottom.blue, 1.0), (top.red, top.green, top.blue, 1.0)))
                )
            case Plate(path=path):
                window = f"{key}_window"
                return (
                    node(window, bpy.types.ShaderNodeTexCoord, {}, {})
                    .map(ended(window, "Window"))
                    .bind(
                        lambda coordinate: node(
                            key, bpy.types.ShaderNodeTexImage, {"Vector": coordinate}, {} if path else {"image": None}, Option.of_optional(path or None).map(lambda chosen: Loaded(path=chosen))
                        ).map(ended(key, "Color"))
                    )
                )

    def background(slot: str, source: str, backdrop: Backdrop[WorldMode] | Backdrop[Scenery]) -> BlenderResult[tuple[Fragment, End]]:
        tint, output = f"{slot}_tint", f"{slot}_background"
        return (
            sourced(source, backdrop.mode)
            .bind(lambda color: mixed(tint, "RGBA", 1.0, color, (backdrop.tint.red, backdrop.tint.green, backdrop.tint.blue, 1.0), "MULTIPLY"))
            .bind(lambda color: node(output, bpy.types.ShaderNodeBackground, {"Color": color, "Strength": backdrop.strength}, {}).map(ended(output, "Background")))
        )

    def joined(sources: tuple[tuple[Fragment, End], ...]) -> BlenderResult[Graph]:
        path = "rays"
        rays = node(path, bpy.types.ShaderNodeLightPath, {}, {})
        result: BlenderResult[tuple[Fragment, End]] = Ok(sources[0])
        for (slot, _, _), source in zip(slots[1:], sources[1:]):
            key = f"{slot}_join"
            result = paired(result, rays.map(ended(path, "Is Glossy Ray" if slot == "glossy" else "Is Camera Ray"))).bind(
                lambda pair: node(key, bpy.types.ShaderNodeMixShader, {"Fac": pair[1], "Shader": pair[0], "Shader_001": source}, {}).map(ended(key, "Shader"))
            )
        return result.bind(lambda shader: node("output", bpy.types.ShaderNodeOutputWorld, {"Surface": shader}, {"is_active_output": True})).map(
            lambda fragment: Graph(kind=bpy.types.ShaderNodeTree, nodes=fragment.nodes, links=fragment.links)
        )

    slots = _slots(environment)
    return accumulated(starmap(background, slots)).bind(joined)


# --- [HOLDERS]


def swapped(row: Ownership[bpy.types.World], prepared: tuple[Environment, Graph], scene: bpy.types.Scene, identity: Identity) -> BlenderResult[bpy.types.World]:
    """Publishes a prepared environment graph, retaining its predecessor in the mark."""
    environment, graph = prepared
    previous = Option.of_optional(scene.world)
    mark = Mark(key=row.key, of=previous)
    held = previous.filter(lambda world: Mark.read(identity, world).filter(lambda mark: mark.key == row.key).is_some()).or_else_with(
        lambda: Option.of_optional(earliest(row.held(identity)).get(mark)) if row.single else Nothing
    )
    with ExitStack() as rollback:

        def built() -> BlenderResult[bpy.types.World]:
            try:
                made = bpy.data.worlds.new(row.key)
            except RuntimeError as error:
                return Error((BlenderEditFault(request=row.key, error=error),))
            rollback.callback(bpy.data.batch_remove, ids=(made,))
            return row.owned(identity, made, mark).bind(lambda world: tree_of(world).map(lambda tree: tree.nodes.clear()).map(lambda _: world))

        def published(world: bpy.types.World) -> bpy.types.World:
            rollback.pop_all()
            return world

        return (
            WorldEnvironment.attachment
            .bound(identity)
            .bind(
                lambda attached: (
                    held
                    .map(Ok)
                    .default_with(built)
                    .bind(
                        lambda world: (
                            tree_of(world)
                            .bind(lambda tree: attached.write(tree, environment).bind(lambda _: reconciled(graph, tree, identity)))
                            .bind(lambda _: written(scene, "world", world))
                            .map(lambda _: world)
                        )
                    )
                )
            )
            .map(published)
        )


def reverted(row: Ownership[bpy.types.World], scene: bpy.types.Scene, identity: Identity) -> BlenderResult[Option[int]]:
    """Restores the marked predecessor and removes unused worlds of the caller's row, answering their count or `Nothing` where the scene shows no world of the row."""
    return (
        Option
        .of_optional(scene.world)
        .bind(lambda world: Mark.read(identity, world))
        .filter(lambda mark: mark.key == row.key)
        .map(lambda mark: written(scene, "world", mark.of.to_optional()).map(lambda _: Some(row.removed(identity))))
        .default_with(lambda: Ok(Nothing))
    )


# --- [SUN]


def followers(tree: bpy.types.NodeTree, identity: Identity) -> BlenderResult[tuple[Follower, ...]]:
    """Returns owned solar sky and image nodes without creating unstored records."""

    def followed(mode: Scenery, source: bpy.types.Node) -> tuple[Follower, ...]:
        match mode, source:
            case Scattering(sun=Solar()) | Preetham(sun=Solar()) | HosekWilkie(sun=Solar()), bpy.types.ShaderNodeTexSky() as sky:
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
    """Returns a native WORLD studio light's path or a typed absent fault."""
    lights = preferences.studio_lights
    return Option.of_optional(lights.get(name)).filter(lambda light: light.type == "WORLD").map(lambda light: light.path).to_result_with(lambda: (BlenderAbsentFault(owner=repr(lights), key=name),))


# --- [CHOICES] --------------------------------------------------------------------------

studios = Choices(
    lambda _identity, _owner, context: (
        (light.name, light.name, "")
        for context in Option.of_optional(context).to_list()
        for light in context.preferences.studio_lights  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        if light.type == "WORLD"
    )
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
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
