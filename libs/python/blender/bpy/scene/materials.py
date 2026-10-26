"""Owned surface setups, texture classification, shader AOVs, and color matrices."""

from contextlib import ExitStack
from pathlib import PurePath
import re
from typing import Annotated, ClassVar, Final, get_args, Literal

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import msgspec.structs
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Empty, Open, Operator
from blender.bpy.data.ids.images import Tag
from blender.bpy.data.ids.ownership import committed, kept, Ownership, Saved
from blender.bpy.data.ids.store import Handle, vacant
from blender.bpy.data.rna.conditions import Present
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.snapshots import Scenes, Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderEditFault, BlenderResult
from blender.bpy.nodes.changes import reconciled
from blender.bpy.nodes.graphs import Expr, Graph, INPUTS, Loaded, NodeGroup, SOCKETS, Term, vector, xyz
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.nodes.trees import emptied, tree_of
from blender.bpy.render.color.spaces import config_of
from blender.bpy.scene.objects import fanned, ObjectState, Reconciled
from blender.bpy.settings.attachments import Attachment, reconciling
from blender.bpy.settings.properties import Definition
from blender.records.faults import accumulated, paired
from blender.records.fields import Case, Choice, Finite, Gated, heading, NonNegative, Presentation, Quantity, Rgb, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type ShadingSetup = Textured | Clay | Emissive | Utility
type AovKind = Literal["VALUE", "COLOR"]


class MapKind(Choice):
    """Texture map kind matched by filename words."""

    words: tuple[str, ...]
    space: Option[str]
    socket: Option[str]

    def __init__(self, value: int, words: tuple[str, ...], space: Option[str], socket: Option[str] = Nothing, label: str | None = None) -> None:
        """Initializes filename words, colorspace role, and Principled BSDF input."""
        super().__init__(value, label)
        self.words, self.space, self.socket = words, space, socket

    BASE_COLOR = 1, ("basecolor", "base_color", "albedo", "diffuse", "diff", "colour", "color", "col"), Nothing, Some("Base Color")
    METALLIC = 2, ("metallic", "metalness", "metal"), Some(PyOpenColorIO.ROLE_DATA), Some("Metallic")
    SPECULAR = 3, ("specular", "spec"), Some(PyOpenColorIO.ROLE_DATA), Some("Specular IOR Level")
    GLOSS = 4, ("glossiness", "gloss", "smoothness"), Some(PyOpenColorIO.ROLE_DATA)
    ROUGHNESS = 5, ("roughness", "rough"), Some(PyOpenColorIO.ROLE_DATA), Some("Roughness")
    NORMAL_DX = 6, ("normal_dx", "normaldx", "nor_dx", "nrm_dx"), Some(PyOpenColorIO.ROLE_DATA), Nothing, "Normal (DirectX)"
    NORMAL = 7, ("normal_gl", "normal", "nor", "nrm", "norm"), Some(PyOpenColorIO.ROLE_DATA)
    BUMP = 8, ("bump",), Some(PyOpenColorIO.ROLE_DATA)
    DISPLACEMENT = 9, ("displacement", "disp", "height"), Some(PyOpenColorIO.ROLE_DATA)
    TRANSMISSION = 10, ("transmission", "transmissive"), Some(PyOpenColorIO.ROLE_DATA), Some("Transmission Weight")
    EMISSION = 11, ("emission", "emissive", "emit"), Nothing, Some("Emission Color")
    ALPHA = 12, ("alpha", "opacity"), Some(PyOpenColorIO.ROLE_DATA), Some("Alpha")


class TextureProjection(Choice):
    """Image projection with Texture Coordinate output it reads."""

    coordinates: str

    def __init__(self, value: int, coordinates: str, label: str | None = None) -> None:
        """Initializes Texture Coordinate output of projection."""
        super().__init__(value, label)
        self.coordinates = coordinates

    FLAT = 1, "UV", "UV"
    BOX = 2, "Object"


class DataPass(Choice):
    """Shader AOV purpose with AOV type and source socket."""

    kind: AovKind
    source: type[bpy.types.Node]
    output: str
    attribute: Option[str]

    def __init__(self, value: int, kind: AovKind, source: type[bpy.types.Node], output: str, attribute: Option[str] = Nothing, label: str | None = None) -> None:
        """Initializes AOV type, source node, output socket, and attribute."""
        super().__init__(value, label)
        self.kind, self.source, self.output, self.attribute = kind, source, output, attribute

    DEPTH = 1, "VALUE", bpy.types.ShaderNodeCameraData, "View Z Depth"
    POSITION = 2, "COLOR", bpy.types.ShaderNodeNewGeometry, "Position"
    REFERENCE = 3, "COLOR", bpy.types.ShaderNodeAttribute, "Vector", Some("rest_position"), "Reference Position"


# --- [CONSTANTS] ------------------------------------------------------------------------

_ACTIVE: Final = Descriptor.declared(bpy.types.ViewLayer, "objects.active", bpy.types.PointerProperty)
_ACTIVE_MATERIAL: Final = Descriptor.declared(bpy.types.Object, "active_material", bpy.types.PointerProperty)
_DISPLACEMENT_METHOD: Final = Descriptor.declared(bpy.types.Material, "displacement_method", bpy.types.EnumProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class DisplacementMap(msgspec.Struct, frozen=True, kw_only=True):
    """Height map scale and map value at undisplaced surface."""

    scale: Annotated[Finite, Presentation(quantity=Some(Quantity.LENGTH))] = 1.0
    midlevel: Finite = 0.5


class Textured(Case, frozen=True, kw_only=True):
    """Principled surface from texture set."""

    title = "Texture Set"
    ownership: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="textured", saved=Saved.USED, single=False)
    maps: dict[MapKind, Annotated[str, msgspec.Meta(title="File"), Definition(text="FILE_PATH")]] = msgspec.field(default_factory=dict)
    projection: TextureProjection = TextureProjection.FLAT
    projection_blend: Annotated[Unit, msgspec.Meta(title="Blend")] = 0.0
    repeat: tuple[Finite, Finite] = (1.0, 1.0)
    displacement: Gated[DisplacementMap] = Gated(enabled=False, value=DisplacementMap())


class Clay(Case, frozen=True, kw_only=True):
    """Plain Principled surface shared by layer overrides."""

    ownership: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="clay", saved=Saved.USED)
    color: Rgb[Unit] = Rgb(red=0.8, green=0.8, blue=0.8)
    roughness: Unit = 0.5


class Emissive(Case, frozen=True, kw_only=True):
    """Emission surface from color or enabled image."""

    title = "Emission"
    ownership: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="emissive", saved=Saved.USED, single=False)
    color: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    strength: Annotated[NonNegative, Presentation(soft=Some((0.0, 100.0)))] = 1.0
    image: Annotated[Gated[str], Definition(text="FILE_PATH")] = Gated(enabled=False, value="")


class Utility(Case, frozen=True, kw_only=True):
    """Data passes written through shader AOVs beside black surface."""

    title = "Data Passes"
    ownership: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="data", saved=Saved.USED)
    passes: frozenset[DataPass] = frozenset(DataPass)


class Shading(msgspec.Struct, frozen=True, kw_only=True):
    """Surface setup stored on owned material node tree."""

    setup: ShadingSetup = Clay()


class Materials(msgspec.Struct, frozen=True, kw_only=True):
    """Shading attachment with ownerships, color matrix group, and texture set operator."""

    @staticmethod
    def reshaded(held: Handle[bpy.types.Material], identity: Identity) -> BlenderResult[Option[str]]:
        """Reconciles material to stored setup, naming no undo step."""
        return (
            held
            .resolve()
            .bind(
                lambda material: paired(tree_of(material).bind(lambda tree: Materials.attachment.read(identity, tree)), config_of()).bind(
                    lambda read: shaded(material, read[0].setup, read[1], identity)
                )
            )
            .map(lambda _: Nothing)
        )

    attachment: ClassVar[Attachment[Shading]] = Attachment(key="shading", form=Shading, owner=bpy.types.NodeTree, changed=Some(reconciling(bpy.types.Material, reshaded)))

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns attachment, ownerships, matrix group, and texture set operator."""
        return matrix().map(lambda group: (Materials.attachment, *(case.ownership for case in get_args(ShadingSetup.__value__)), group, texture_setup))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CLASSIFY]


def kind(path: str) -> Option[MapKind]:
    """Returns kind of longest filename word match, `Nothing` on no match or tie."""
    stem = PurePath(path).stem
    found = tuple((len(word), member) for member in MapKind for word in member.words if re.search(rf"(?<![a-z]){re.escape(word)}(?![a-z])", stem, re.IGNORECASE))
    longest = max((size for size, _ in found), default=0)
    match tuple(frozenset(member for size, member in found if size == longest)):
        case (only,):
            return Some(only)
        case _:
            return Nothing


def classified(paths: tuple[str, ...]) -> BlenderResult[dict[MapKind, str]]:
    """Classifies files by map kind, accumulating repeated kinds as collisions."""
    held: dict[MapKind, str] = {}
    maps: list[BlenderResult[tuple[MapKind, str]]] = []
    for path in paths:
        for found in kind(path).to_list():
            maps.append(Error((BlenderCollisionFault(request=path, error=held[found]),)) if found in held else Ok((found, path)))
            held.setdefault(found, path)
    return accumulated(maps).map(dict)


# --- [GRAPH]


def material_graph(setup: ShadingSetup, config: PyOpenColorIO.Config) -> BlenderResult[Graph]:
    """Builds shader graph of setup with config colorspaces, its output and every AOV output lowered as roots."""

    def image(path: str, tag: Tag, inputs: tuple[tuple[str, Term], ...] = (), members: tuple[tuple[str, Value], ...] = ()) -> Expr:
        return Expr(kind=bpy.types.ShaderNodeTexImage, inputs=inputs, members=members, content=Some(Loaded(path=path, tag=tag)), socket="Color", shape="RGBA")

    def output(surface: Expr, displacement: Option[Expr] = Nothing) -> Expr:
        return Expr(kind=bpy.types.ShaderNodeOutputMaterial, inputs=(("Surface", surface), *(("Displacement", held) for held in displacement.to_list())), members=(("is_active_output", True),))

    def textured(setup: Textured) -> tuple[Expr, ...]:
        maps, displacement = setup.maps, setup.displacement
        mapping = Expr(
            kind=bpy.types.ShaderNodeMapping,
            inputs=(("Vector", Expr(kind=bpy.types.ShaderNodeTexCoord, socket=setup.projection.coordinates, shape="VECTOR")), ("Scale", (*setup.repeat, 1.0))),
            socket="Vector",
            shape="VECTOR",
        )
        textures = {
            member: image(
                path,
                Tag(space=member.space.map(lambda role: config.getColorSpace(role).getName())),
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
                *((socket, textures[member]) for member in maps for socket in member.socket.to_list()),
                *((("Emission Strength", 1.0),) if MapKind.EMISSION in maps else ()),
                *(("Roughness", Expr(kind=bpy.types.ShaderNodeInvert, inputs=(("Fac", 1.0), ("Color", gloss)), socket="Color", shape="RGBA")) for gloss in glossed.to_list()),
                *(("Normal", held) for held in relief.or_else(mapped).to_list()),
            ),
            socket="BSDF",
        )
        displaced = Option.of_optional(textures.get(MapKind.DISPLACEMENT)).filter(lambda _: displacement.enabled)
        return (
            output(
                surface,
                displaced.map(
                    lambda height: Expr(
                        kind=bpy.types.ShaderNodeDisplacement,
                        inputs=(("Height", height), ("Scale", displacement.value.scale), ("Midlevel", displacement.value.midlevel)),
                        socket="Displacement",
                        shape="VECTOR",
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
                return (output(Expr(kind=bpy.types.ShaderNodeBsdfPrincipled, inputs=(("Base Color", (*msgspec.structs.astuple(color), 1.0)), ("Roughness", roughness)), socket="BSDF")),)
            case Emissive(color=color, strength=strength, image=gated):
                return (output(emission(image(gated.value, Tag()) if gated.enabled else (*msgspec.structs.astuple(color), 1.0), strength)),)
            case Utility(passes=passes):
                return (
                    output(emission((0.0, 0.0, 0.0, 1.0), 0.0)),
                    *(
                        Expr(
                            kind=bpy.types.ShaderNodeOutputAOV,
                            inputs=((heading(member.kind), Expr(kind=member.source, members=tuple(("attribute_name", name) for name in member.attribute.to_list()), socket=member.output)),),
                            members=(("aov_name", member.name.lower()),),
                        )
                        for member in sorted(passes)
                    ),
                )

    return Graph.lowered(bpy.types.ShaderNodeTree, *roots())


# --- [HOLDERS]


def shaded(held: bpy.types.Material, setup: ShadingSetup, config: PyOpenColorIO.Config, identity: Identity) -> BlenderResult[bpy.types.Material]:
    """Reconciles owned material tree, `BOTH` displacement method when enabled."""
    method = "BOTH" if isinstance(setup, Textured) and setup.displacement.enabled else _DISPLACEMENT_METHOD.prop.default
    return paired(material_graph(setup, config), tree_of(held)).bind(lambda found: reconciled(*found, identity)).bind(lambda _: _DISPLACEMENT_METHOD.write(held, method)).map(lambda _: held)


def material(setup: ShadingSetup, config: PyOpenColorIO.Config, identity: Identity, scope: Option[ExitStack] = Nothing) -> BlenderResult[bpy.types.Material]:
    """Reuses or creates owned setup material, removing a created one on fault or scope unwind."""

    def built() -> BlenderResult[bpy.types.Material]:
        return kept(BlenderEditFault.attempted(setup.title, lambda: bpy.data.materials.new(vacant(setup.title, bpy.data.materials.keys()))), emptied)

    return Materials.attachment.bound(identity).bind(
        lambda attached: setup.ownership.made(
            identity, built, filled=lambda held: tree_of(held).bind(lambda tree: attached.write(tree, Shading(setup=setup))).bind(lambda _: shaded(held, setup, config, identity)), scope=scope
        )
    )


def referenced(scene: bpy.types.Scene, context: bpy.types.Context) -> BlenderResult[tuple[Reconciled, ...]]:
    """Enables rest position attribute on every scene object."""
    return fanned(ObjectState(values=Some(Snapshot.of(bpy.types.Object, {"add_rest_position_attribute": True}))), Scenes(names=(scene.name,)), context)


def _textured(call: Call[Empty]) -> BlenderResult[Done]:
    """Builds texture set material from chosen files into active object's active slot, noting unmatched files."""
    paths = tuple(map(str, call.paths))

    def assigned(maps: dict[MapKind, str], config: PyOpenColorIO.Config, rollback: ExitStack) -> BlenderResult[dict[MapKind, str]]:
        return (
            paired(
                Option.of_optional(call.context.active_object).to_result_with(lambda: (BlenderAbsentFault(owner=repr(call.context), key="active_object"),)),
                material(Textured(maps=maps), config, call.identity, Some(rollback)),
            )
            .bind(lambda pair: _ACTIVE_MATERIAL.write(*pair))
            .map(lambda _: maps)
        )

    def published(maps: dict[MapKind, str]) -> Done:
        return Done(notes=tuple(("INFO", f"{PurePath(path).name} matches no map kind") for path in paths if path not in frozenset(maps.values())))

    with ExitStack() as rollback:
        return committed(paired(classified(paths), config_of()).bind(lambda pair: assigned(*pair, rollback)), rollback).map(published)


# --- [MATRIX]


def matrix() -> BlenderResult[NodeGroup]:
    """Builds shared shader group multiplying color by matrix rows and gain."""
    interface = Interface(
        outputs=(Socket(key="transformed", name="Color", kind=bpy.types.NodeSocketColor),),
        inputs=(
            Socket(key="color", name="Color", kind=bpy.types.NodeSocketColor),
            *(Socket(key=row, name=heading(row), kind=bpy.types.NodeSocketVector) for row in Rgb.__struct_fields__),
            Socket(key="gain", name="Gain", kind=bpy.types.NodeSocketFloat),
        ),
    )
    entered = {
        item.key: Expr(kind=bpy.types.NodeGroupInput, socket=item.key, shape=next(shape for shape, kind in SOCKETS.items() if kind is item.kind), label=Some(INPUTS)) for item in interface.inputs
    }
    transformed = vector("SCALE", xyz(*(vector("DOT_PRODUCT", entered["color"], entered[row]) for row in Rgb.__struct_fields__)), entered["gain"])
    return NodeGroup.built("color_matrix", "Color Matrix", interface, {"transformed": transformed}, bpy.types.ShaderNodeTree)


# --- [COMPOSITION] ----------------------------------------------------------------------


texture_setup: Operator[Empty] = Operator(
    key="texture_setup",
    title="Add Texture Set",
    description="Build material from texture files into active material slot",
    form=Empty,
    run=_textured,
    condition=Some(Present(descriptor=_ACTIVE)),
    invoke=Some(Open(extensions=lambda _: tuple(sorted(bpy.path.extensions_image)))),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Clay",
    "DataPass",
    "DisplacementMap",
    "Emissive",
    "MapKind",
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
    "texture_setup",
]
