"""Owned surface setups, texture classification, shader AOVs, and color matrices."""

from collections.abc import Mapping
from contextlib import ExitStack
from enum import IntEnum, StrEnum
from functools import partial
from pathlib import PurePath
import re
from types import MappingProxyType
from typing import ClassVar, Final, get_args, Literal

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Empty, Open, Operator
from blender.bpy.data.ids.images import Tag
from blender.bpy.data.ids.ownership import kept, Ownership, retained, Saved
from blender.bpy.data.rna.conditions import Present
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.snapshots import Scenes, Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.changes import reconciled
from blender.bpy.nodes.graphs import Expr, Graph, INPUTS, Loaded, NodeGroup, SOCKETS, Term, vector, xyz
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.nodes.trees import emptied, tree_of
from blender.bpy.scene.objects import fanned, ObjectState, Reconciled
from blender.bpy.settings.attachments import Stored
from blender.records.faults import accumulated, Fault, Outcome, paired
from blender.records.fields import Case, Finite, NonNegative, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type ShadingSetup = Textured | Clay | Emissive | Utility
type AovKind = Literal["VALUE", "COLOR"]


class MapKind(IntEnum):
    """Texture map kind matched by filename words."""

    BASE_COLOR = 1
    METALLIC = 2
    SPECULAR = 3
    GLOSS = 4
    ROUGHNESS = 5
    NORMAL_DX = 6
    NORMAL = 7
    BUMP = 8
    DISPLACEMENT = 9
    TRANSMISSION = 10
    EMISSION = 11
    ALPHA = 12


class TextureProjection(StrEnum):
    """Image projection valued by the Texture Coordinate output it reads."""

    FLAT = "UV"
    BOX = "Object"


class DataPass(IntEnum):
    """Shader AOV purpose."""

    DEPTH = 1
    POSITION = 2
    REFERENCE = 3


# --- [CONSTANTS] ------------------------------------------------------------------------

_ACTIVE: Final = Descriptor.declared(bpy.types.ViewLayer, ("objects", "active"), bpy.types.PointerProperty)
_ACTIVE_MATERIAL: Final = Descriptor.declared(bpy.types.Object, "active_material", bpy.types.PointerProperty)
_DISPLACEMENT_METHOD: Final = Descriptor.declared(bpy.types.Material, "displacement_method", bpy.types.EnumProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class MapRole(msgspec.Struct, frozen=True, kw_only=True):
    """Filename words, colorspace role, and Principled BSDF input of one map kind."""

    words: tuple[str, ...]
    space: Option[str] = Some(PyOpenColorIO.ROLE_DATA)
    socket: Option[str] = Nothing


class Aov(msgspec.Struct, frozen=True, kw_only=True):
    """AOV type, source node, source output, and attribute of one data pass."""

    kind: AovKind
    source: type[bpy.types.Node]
    output: str
    attribute: Option[str] = Nothing


class DisplacementMap(msgspec.Struct, frozen=True, kw_only=True):
    """Height map scale and map value at undisplaced surface."""

    scale: Finite = 1.0
    midlevel: Finite = 0.5


class Textured(Case, frozen=True, kw_only=True):
    """Principled surface from texture set."""

    ownership: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="textured", saved=Saved.USED, single=False)
    maps: Mapping[MapKind, str] = msgspec.field(default_factory=dict)
    projection: TextureProjection = TextureProjection.FLAT
    projection_blend: Unit = 0.0
    repeat: tuple[Finite, Finite] = (1.0, 1.0)
    displacement: DisplacementMap | None = None


class Clay(Case, frozen=True, kw_only=True):
    """Plain Principled surface shared by layer overrides."""

    ownership: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="clay", saved=Saved.USED)
    color: tuple[float, float, float] = (0.8, 0.8, 0.8)
    roughness: Unit = 0.5


class Emissive(Case, frozen=True, kw_only=True):
    """Emission surface from color or image."""

    ownership: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="emissive", saved=Saved.USED, single=False)
    color: tuple[float, float, float] = (1.0, 1.0, 1.0)
    strength: NonNegative = 1.0
    image: str | None = None


class Utility(Case, frozen=True, kw_only=True):
    """Data passes written through shader AOVs beside black surface."""

    ownership: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="data", saved=Saved.USED)
    passes: frozenset[DataPass] = frozenset(DataPass)


class Shading(msgspec.Struct, frozen=True, kw_only=True):
    """Surface setup stored on owned material node tree."""

    setup: ShadingSetup = Clay()


class Materials(msgspec.Struct, frozen=True, kw_only=True):
    """Shading store with ownerships, color matrix group, and texture set operator."""

    ownerships: tuple[Ownership[bpy.types.Material], ...] = tuple(case.ownership for case in get_args(ShadingSetup.__value__))

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns ownerships, matrix group, and texture set operator."""
        return matrix().map(lambda group: (*self.ownerships, group, texture_setup_command))


# --- [TABLES] ---------------------------------------------------------------------------

MAPS: Final[Mapping[MapKind, MapRole]] = MappingProxyType({
    MapKind.BASE_COLOR: MapRole(words=("basecolor", "base_color", "albedo", "diffuse", "diff", "colour", "color", "col"), space=Nothing, socket=Some("Base Color")),
    MapKind.METALLIC: MapRole(words=("metallic", "metalness", "metal"), socket=Some("Metallic")),
    MapKind.SPECULAR: MapRole(words=("specular", "spec"), socket=Some("Specular IOR Level")),
    MapKind.GLOSS: MapRole(words=("glossiness", "gloss", "smoothness")),
    MapKind.ROUGHNESS: MapRole(words=("roughness", "rough"), socket=Some("Roughness")),
    MapKind.NORMAL_DX: MapRole(words=("normal_dx", "normaldx", "nor_dx", "nrm_dx")),
    MapKind.NORMAL: MapRole(words=("normal_gl", "normal", "nor", "nrm", "norm")),
    MapKind.BUMP: MapRole(words=("bump",)),
    MapKind.DISPLACEMENT: MapRole(words=("displacement", "disp", "height")),
    MapKind.TRANSMISSION: MapRole(words=("transmission", "transmissive"), socket=Some("Transmission Weight")),
    MapKind.EMISSION: MapRole(words=("emission", "emissive", "emit"), space=Nothing, socket=Some("Emission Color")),
    MapKind.ALPHA: MapRole(words=("alpha", "opacity"), socket=Some("Alpha")),
})
AOVS: Final[Mapping[DataPass, Aov]] = MappingProxyType({
    DataPass.DEPTH: Aov(kind="VALUE", source=bpy.types.ShaderNodeCameraData, output="View Z Depth"),
    DataPass.POSITION: Aov(kind="COLOR", source=bpy.types.ShaderNodeNewGeometry, output="Position"),
    DataPass.REFERENCE: Aov(kind="COLOR", source=bpy.types.ShaderNodeAttribute, output="Vector", attribute=Some("rest_position")),
})

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CLASSIFY]


def kind(path: str) -> Option[MapKind]:
    """Returns kind of longest filename word match, `Nothing` on no match or tie."""
    stem = PurePath(path).stem
    found = tuple((len(word), member) for member, role in MAPS.items() for word in role.words if re.search(rf"(?<![a-z]){re.escape(word)}(?![a-z])", stem, re.IGNORECASE))
    longest = max((size for size, _ in found), default=0)
    match tuple(frozenset(member for size, member in found if size == longest)):
        case (only,):
            return Some(only)
        case _:
            return Nothing


def classified(paths: tuple[str, ...]) -> Outcome[dict[MapKind, str]]:
    """Classifies files by map kind, accumulating repeated kinds as collisions."""
    held: dict[MapKind, str] = {}
    maps: list[Outcome[tuple[MapKind, str]]] = []
    for path in paths:
        for found in kind(path).to_list():
            maps.append(Error(Fault.of(BlenderReason.TAKEN, path, member=held[found])) if found in held else Ok((found, path)))
            held.setdefault(found, path)
    return accumulated(maps).map(dict)


# --- [GRAPH]


def material_graph(setup: ShadingSetup, config: PyOpenColorIO.Config) -> Outcome[Graph]:
    """Builds shader graph of setup with config colorspaces, its output and every AOV output lowered as roots."""

    def image(path: str, tag: Tag, inputs: tuple[tuple[str, Term], ...] = (), members: tuple[tuple[str, Value], ...] = ()) -> Expr:
        return Expr(kind=bpy.types.ShaderNodeTexImage, inputs=inputs, members=members, content=Some(Loaded(path=path, tag=tag)), socket="Color", shape="RGBA")

    def output(surface: Expr, displacement: Option[Expr] = Nothing) -> Expr:
        return Expr(kind=bpy.types.ShaderNodeOutputMaterial, inputs=(("Surface", surface), *(("Displacement", held) for held in displacement.to_list())), members=(("is_active_output", True),))

    def textured(setup: Textured) -> tuple[Expr, ...]:
        maps, displacement = setup.maps, Option.of_optional(setup.displacement)
        mapping = Expr(
            kind=bpy.types.ShaderNodeMapping,
            inputs=(("Vector", Expr(kind=bpy.types.ShaderNodeTexCoord, socket=setup.projection.value, shape="VECTOR")), ("Scale", (*setup.repeat, 1.0))),
            socket="Vector",
            shape="VECTOR",
        )
        textures = {
            member: image(
                path,
                Tag(space=MAPS[member].space.map(lambda role: config.getColorSpace(role).getName())),
                (("Vector", mapping),),
                (("projection", setup.projection.name), ("projection_blend", setup.projection_blend)),
            )
            for member, path in maps.items()
        }

        def flipped(texture: Expr) -> Expr:
            red, green, blue = (Expr(kind=bpy.types.ShaderNodeSeparateColor, inputs=(("Color", texture),), socket=channel) for channel in ("Red", "Green", "Blue"))
            return Expr(kind=bpy.types.ShaderNodeCombineColor, inputs=(("Red", red), ("Green", 1.0 - green), ("Blue", blue)), socket="Color", shape="RGBA")

        normal = Option.of_optional(textures.get(MapKind.NORMAL)).or_else_with(lambda: Option.of_optional(textures.get(MapKind.NORMAL_DX)).map(flipped))
        mapped = normal.map(lambda color: Expr(kind=bpy.types.ShaderNodeNormalMap, inputs=(("Color", color),), socket="Normal", shape="VECTOR"))
        relief = Option.of_optional(textures.get(MapKind.BUMP)).map(
            lambda height: Expr(kind=bpy.types.ShaderNodeBump, inputs=(("Height", height), *(("Normal", held) for held in mapped.to_list())), socket="Normal", shape="VECTOR")
        )
        glossed = Option.of_optional(textures.get(MapKind.GLOSS)).filter(lambda _: MapKind.ROUGHNESS not in maps)
        surface = Expr(
            kind=bpy.types.ShaderNodeBsdfPrincipled,
            inputs=(
                *((socket, textures[member]) for member in maps for socket in MAPS[member].socket.to_list()),
                *((("Emission Strength", 1.0),) if MapKind.EMISSION in maps else ()),
                *(("Roughness", Expr(kind=bpy.types.ShaderNodeInvert, inputs=(("Fac", 1.0), ("Color", gloss)), socket="Color", shape="RGBA")) for gloss in glossed.to_list()),
                *(("Normal", held) for held in relief.or_else(mapped).to_list()),
            ),
            socket="BSDF",
        )
        displaced = Option.of_optional(textures.get(MapKind.DISPLACEMENT)).bind(lambda height: displacement.map(lambda held: (height, held)))
        return (
            output(
                surface,
                displaced.map(
                    lambda pair: Expr(
                        kind=bpy.types.ShaderNodeDisplacement, inputs=(("Height", pair[0]), ("Scale", pair[1].scale), ("Midlevel", pair[1].midlevel)), socket="Displacement", shape="VECTOR"
                    )
                ),
            ),
        )

    def emission(color: Term, strength: float) -> Expr:
        return Expr(kind=bpy.types.ShaderNodeEmission, inputs=(("Color", color), ("Strength", strength)), socket="Emission")

    def roots() -> tuple[Expr, ...]:
        match setup:
            case Textured():
                return textured(setup)
            case Clay(color=color, roughness=roughness):
                return (output(Expr(kind=bpy.types.ShaderNodeBsdfPrincipled, inputs=(("Base Color", (*color, 1.0)), ("Roughness", roughness)), socket="BSDF")),)
            case Emissive(color=color, strength=strength, image=path):
                return (output(emission(Option.of_optional(path).map(lambda chosen: image(chosen, Tag())).default_value((*color, 1.0)), strength)),)
            case Utility(passes=passes):
                return (
                    output(emission((0.0, 0.0, 0.0, 1.0), 0.0)),
                    *(
                        Expr(
                            kind=bpy.types.ShaderNodeOutputAOV,
                            inputs=((aov.kind.capitalize(), Expr(kind=aov.source, members=tuple(("attribute_name", name) for name in aov.attribute.to_list()), socket=aov.output)),),
                            members=(("aov_name", member.name.lower()),),
                        )
                        for member in sorted(passes)
                        for aov in (AOVS[member],)
                    ),
                )

    return Graph.lowered(bpy.types.ShaderNodeTree, *roots())


# --- [HOLDERS]


def shaded(held: bpy.types.Material, setup: ShadingSetup, config: PyOpenColorIO.Config, identity: Identity) -> Outcome[bpy.types.Material]:
    """Reconciles owned material tree, `BOTH` displacement method when enabled."""
    method = "BOTH" if isinstance(setup, Textured) and setup.displacement is not None else _DISPLACEMENT_METHOD.prop.default
    return paired(material_graph(setup, config), tree_of(held)).bind(lambda found: reconciled(*found, identity)).bind(lambda _: _DISPLACEMENT_METHOD.write(held, method)).map(lambda _: held)


def material(setup: ShadingSetup, config: PyOpenColorIO.Config, identity: Identity, scope: Option[ExitStack] = Nothing) -> Outcome[bpy.types.Material]:
    """Reuses or creates owned setup material, removing a created one on fault or scope unwind."""

    def built() -> Outcome[bpy.types.Material]:
        return kept(catch(bpy.data.materials.new, exception=RuntimeError)(bpy.path.display_name(setup.ownership.key)).map_error(partial(Fault.of, BlenderReason.REFUSED, setup.ownership.key)), emptied)

    return stored(identity).bind(
        lambda shading: setup.ownership.made(
            identity, built, filled=lambda held: tree_of(held).bind(lambda tree: shading.write(tree, Shading(setup=setup))).bind(lambda _: shaded(held, setup, config, identity)), scope=scope
        )
    )


def stored(identity: Identity) -> Outcome[Stored[Shading]]:
    """Returns the store of the shading setup an owned material tree holds."""
    return identity.identifier("shading").map(lambda name: Stored(name=name, decoder=msgspec.msgpack.Decoder(Shading)))


def referenced(scene: bpy.types.Scene, context: bpy.types.Context) -> Outcome[tuple[Reconciled, ...]]:
    """Enables rest position attribute on every scene object."""
    return fanned(ObjectState(values=Some(Snapshot.of(bpy.types.Object, {"add_rest_position_attribute": True}))), Scenes(names=(scene.name,)), context)


def texture_setup(call: Call[Empty]) -> Outcome[Done]:
    """Builds texture set material from chosen files into active object's active slot, noting unmatched files."""
    paths = tuple(map(str, call.paths))

    def assigned(maps: dict[MapKind, str], config: PyOpenColorIO.Config, rollback: ExitStack) -> Outcome[dict[MapKind, str]]:
        return (
            paired(
                Option.of_optional(call.context.active_object).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(call.context), member="active_object")),
                material(Textured(maps=maps), config, call.identity, Some(rollback)),
            )
            .bind(lambda pair: _ACTIVE_MATERIAL.write(*pair))
            .map(lambda _: maps)
        )

    def published(maps: dict[MapKind, str]) -> Done:
        return Done(notes=tuple(("INFO", f"{PurePath(path).name} matches no map kind") for path in paths if path not in frozenset(maps.values())))

    with ExitStack() as rollback:
        return retained(classified(paths).bind(lambda maps: assigned(maps, PyOpenColorIO.GetCurrentConfig(), rollback)), rollback).map(published)


# --- [MATRIX]


def matrix() -> Outcome[NodeGroup]:
    """Builds shared shader group multiplying color by matrix rows and gain."""
    rows = ("red", "green", "blue")
    interface = Interface(
        outputs=(Socket(key="transformed", name="Color", kind=bpy.types.NodeSocketColor),),
        inputs=(
            Socket(key="color", name="Color", kind=bpy.types.NodeSocketColor),
            *(Socket(key=row, name=bpy.path.display_name(row), kind=bpy.types.NodeSocketVector) for row in rows),
            Socket(key="gain", name="Gain", kind=bpy.types.NodeSocketFloat),
        ),
    )
    entered = {
        item.key: Expr(kind=bpy.types.NodeGroupInput, socket=item.key, shape=next(shape for shape, kind in SOCKETS.items() if kind is item.kind), label=Some(INPUTS)) for item in interface.inputs
    }
    transformed = vector("SCALE", xyz(*(vector("DOT_PRODUCT", entered["color"], entered[row]) for row in rows)), entered["gain"])
    return NodeGroup.built("color_matrix", "Color Matrix", interface, {"transformed": transformed}, bpy.types.ShaderNodeTree)


# --- [COMPOSITION] ----------------------------------------------------------------------


texture_setup_command: Operator[Empty] = Operator(
    form=Empty,
    run=texture_setup,
    condition=Some(Present(descriptor=_ACTIVE)),
    invoke=Some(Open(extensions=lambda _: tuple(sorted(bpy.path.extensions_image)))),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "AOVS",
    "MAPS",
    "Aov",
    "Clay",
    "DataPass",
    "DisplacementMap",
    "Emissive",
    "MapKind",
    "MapRole",
    "Materials",
    "Shading",
    "ShadingSetup",
    "TextureProjection",
    "Textured",
    "Utility",
    "classified",
    "kind",
    "material",
    "material_graph",
    "matrix",
    "referenced",
    "shaded",
    "stored",
    "texture_setup_command",
]
