"""Owned surface setups, texture classification, shader AOVs, and color matrices."""

from contextlib import ExitStack
import math
from pathlib import PurePath
import re
from typing import Annotated, ClassVar, get_args, Literal

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import msgspec.structs
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Empty, Open, Operator
from blender.bpy.data.ids.images import Tag
from blender.bpy.data.ids.ownership import committed, kept, Ownership, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Present
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.snapshots import Scenes, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderEditFault, BlenderResult
from blender.bpy.nodes.changes import reconciled
from blender.bpy.nodes.graphs import End, ended, Fragment, Graph, INPUTS, Loaded, node, NodeGroup, Operand, vector
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.nodes.trees import emptied, tree_of
from blender.bpy.render.color.spaces import config_of
from blender.bpy.scene.objects import fanned, ObjectState, Reconciled
from blender.bpy.settings.attachments import Attachment, reconciling
from blender.bpy.settings.properties import Definition
from blender.imaging.calibration import Calibration
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Case, Choice, Finite, Gated, heading, NonNegative, Presentation, Quantity, Rgb, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type ShadingSetup = Textured | Clay | Emissive | Utility
type AovKind = Literal["VALUE", "COLOR"]


class MapKind(Choice):
    """Texture purpose with vendor filename words, color reading, and direct surface route."""

    words: tuple[str, ...]
    space: Option[str]
    socket: Option[str]

    def __init__(self, value: int, words: tuple[str, ...], space: Option[str], socket: Option[str] = Nothing, label: str | None = None) -> None:
        """Sets the classification and routing facts of one persistent map purpose."""
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
    """Surface projection paired with the coordinate socket it requires."""

    coordinates: str

    def __init__(self, value: int, coordinates: str, label: str | None = None) -> None:
        """Sets the native coordinate source of a persistent mapping choice."""
        super().__init__(value, label)
        self.coordinates = coordinates

    FLAT = 1, "UV", "UV"
    BOX = 2, "Object"


class DataPass(Choice):
    """Shader AOV purpose with its native type and source socket."""

    kind: AovKind
    source: type[bpy.types.Node]
    output: str
    attribute: Option[str]

    def __init__(self, value: int, kind: AovKind, source: type[bpy.types.Node], output: str, attribute: Option[str] = Nothing, label: str | None = None) -> None:
        """Sets the native graph source of one persistent pass."""
        super().__init__(value, label)
        self.kind, self.source, self.output, self.attribute = kind, source, output, attribute

    DEPTH = 1, "VALUE", bpy.types.ShaderNodeCameraData, "View Z Depth"
    POSITION = 2, "COLOR", bpy.types.ShaderNodeNewGeometry, "Position"
    REFERENCE = 3, "COLOR", bpy.types.ShaderNodeAttribute, "Vector", Some("rest_position"), "Reference Position"


# --- [MODELS] ---------------------------------------------------------------------------


class DisplacementMap(msgspec.Struct, frozen=True, kw_only=True):
    """Height-map scale and the map value lying on the undisplaced surface."""

    scale: Annotated[Finite, Presentation(quantity=Some(Quantity.LENGTH))] = 1.0
    midlevel: Finite = 0.5


class Textured(Case, frozen=True, kw_only=True, tag=1):
    """Principled surface fed by a classified texture set."""

    title = "Texture Set"
    ownership: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="textured", saved=Saved.USED, single=False)
    maps: dict[MapKind, Annotated[str, msgspec.Meta(title="File"), Definition(text="FILE_PATH")]] = msgspec.field(default_factory=dict)
    projection: TextureProjection = TextureProjection.FLAT
    projection_blend: Annotated[Unit, msgspec.Meta(title="Blend")] = 0.0
    repeat: tuple[Finite, Finite] = (1.0, 1.0)
    displacement: Gated[DisplacementMap] = Gated(enabled=False, value=DisplacementMap())


class Clay(Case, frozen=True, kw_only=True, tag=2):
    """Plain Principled surface shared by layer overrides."""

    ownership: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="clay", saved=Saved.USED)
    color: Rgb[Unit] = Rgb(red=0.8, green=0.8, blue=0.8)
    roughness: Unit = 0.5


class Emissive(Case, frozen=True, kw_only=True, tag=3):
    """Emission surface driven by a color or an enabled image."""

    title = "Emission"
    ownership: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="emissive", saved=Saved.USED, single=False)
    color: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    strength: Annotated[NonNegative, Presentation(soft=Some((0.0, 100.0)))] = 1.0
    image: Annotated[Gated[str], Definition(text="FILE_PATH")] = Gated(enabled=False, value="")


class Utility(Case, frozen=True, kw_only=True, tag=4):
    """Data passes written through shader AOVs beside an opaque black surface."""

    title = "Data Passes"
    ownership: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="data", saved=Saved.USED)
    passes: frozenset[DataPass] = frozenset(DataPass)


class Shading(msgspec.Struct, frozen=True, kw_only=True):
    """Surface setup stored on an owned material's embedded tree."""

    setup: ShadingSetup = Clay()


class Materials(msgspec.Struct, frozen=True, kw_only=True):
    """Material storage, ownership, shared matrix, and texture-browser registration."""

    # --- [STORAGE]

    @staticmethod
    def reshaded(held: Handle[bpy.types.Material], identity: Identity) -> BlenderResult[Option[str]]:
        """Reconciles the latest attached setup in a deferred write without an undo step."""
        return (
            paired(held.resolve(), Materials.attachment.bound(identity))
            .bind(lambda pair: paired(tree_of(pair[0]).bind(pair[1].read), config_of()).bind(lambda read: shaded(pair[0], read[0].setup, read[1], identity)))
            .map(lambda _: Nothing)
        )

    attachment: ClassVar[Attachment[Shading]] = Attachment(key="shading", form=Shading, owner=bpy.types.NodeTree, changed=Some(reconciling(bpy.types.Material, reshaded)))

    # --- [OPERATORS]

    @property
    def texture_setup(self) -> BlenderResult[Operator[Empty]]:
        """Native file-browser operator assigning a texture set to the active slot."""

        def run(call: Call[Empty]) -> BlenderResult[Done]:
            paths = tuple(map(str, call.paths))

            def assigned(maps: dict[MapKind, str], config: PyOpenColorIO.Config) -> BlenderResult[dict[MapKind, str]]:
                return (
                    paired(
                        Option.of_optional(call.context.active_object).to_result_with(lambda: (BlenderAbsentFault(owner=repr(call.context), key="active_object"),)),
                        Descriptor.resolve(bpy.types.Object, "active_material", bpy.types.PointerProperty),
                    )
                    .bind(lambda pair: material(Textured(maps=maps), config, call.identity, Some(rollback)).bind(lambda held: pair[1].write(pair[0], held)))
                    .map(lambda _: maps)
                )

            def published(maps: dict[MapKind, str]) -> Done:
                matched = frozenset(maps.values())
                return Done(notes=tuple(("INFO", f"{PurePath(path).name} matches no map kind") for path in paths if path not in matched))

            with ExitStack() as rollback:
                return committed(paired(classified(paths), config_of()).bind(lambda pair: assigned(*pair)), rollback).map(published)

        return Descriptor.resolve(bpy.types.ViewLayer, "objects.active", bpy.types.PointerProperty).map(
            lambda active: Operator(
                key="texture_setup",
                title="Add Texture Set",
                description="Build a material from texture files and assign it to the active object's active slot",
                form=Empty,
                run=run,
                condition=Some(Present(descriptor=active)),
                invoke=Some(Open(extensions=lambda _: tuple(sorted(bpy.path.extensions_image)))),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            )
        )

    # --- [REGISTRATION]

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns nested declarations for registration to scope once under this identity."""
        return paired(matrix(), self.texture_setup).map(lambda pair: (Materials.attachment, *(case.ownership for case in get_args(ShadingSetup.__value__)), *pair))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CLASSIFY]


def kind(path: str) -> Option[MapKind]:
    """Returns the unique longest filename-word match, or absence for no match or a tie."""
    stem = PurePath(path).stem
    found = tuple((len(word), member) for member in MapKind for word in member.words if re.search(rf"(?<![a-z]){re.escape(word)}(?![a-z])", stem, re.IGNORECASE))
    longest = max((size for size, _ in found), default=0)
    match tuple(frozenset(member for size, member in found if size == longest)):
        case (only,):
            return Some(only)
        case _:
            return Nothing


def classified(paths: tuple[str, ...]) -> BlenderResult[dict[MapKind, str]]:
    """Classifies each selected file once, accumulating every repeated-purpose collision."""
    held: dict[MapKind, str] = {}
    maps: list[BlenderResult[tuple[MapKind, str]]] = []
    for path in paths:
        for found in kind(path).to_list():
            maps.append(Error((BlenderCollisionFault(request=path, error=held[found]),)) if found in held else Ok((found, path)))
            held.setdefault(found, path)
    return accumulated(maps).map(dict)


# --- [GRAPH]


def material_graph(setup: ShadingSetup, config: PyOpenColorIO.Config) -> BlenderResult[Graph]:
    """Builds a shader graph, reading data-map colorspace from an admitted OCIO config."""
    parts: tuple[BlenderResult[Fragment], ...]
    output: dict[str, Operand] = {}
    match setup:
        case Textured(maps=maps, projection=projection, projection_blend=blend, repeat=repeat, displacement=displacement):
            textures = {member: End(node=member.name.lower(), socket="Color") for member in maps}
            normal = Option.of_optional(next((member for member in (MapKind.NORMAL, MapKind.NORMAL_DX) if member in maps), None))
            flipped = normal == Some(MapKind.NORMAL_DX)
            glossed = MapKind.GLOSS in maps and MapKind.ROUGHNESS not in maps
            bumped = MapKind.BUMP in maps
            displaced = displacement.enabled and MapKind.DISPLACEMENT in maps
            surface: dict[str, Operand] = {socket: textures[member] for member in maps for socket in member.socket.to_list()}
            surface |= {"Emission Strength": 1.0} if MapKind.EMISSION in maps else {}
            surface |= {"Roughness": End(node="invert", socket="Color")} if glossed else {}
            surface |= {"Normal": End(node="relief", socket="Normal")} if bumped else normal.map(lambda _: {"Normal": End(node="normal_map", socket="Normal")}).default_value({})
            output = {"Displacement": End(node="displacer", socket="Displacement")} if displaced else {}
            parts = (
                node("surface", bpy.types.ShaderNodeBsdfPrincipled, surface, {}),
                *(
                    (
                        node("coordinates", bpy.types.ShaderNodeTexCoord, {}, {}),
                        node("mapping", bpy.types.ShaderNodeMapping, {"Vector": End(node="coordinates", socket=projection.coordinates), "Scale": (*repeat, 1.0)}, {}),
                    )
                    if maps
                    else ()
                ),
                *(
                    node(
                        member.name.lower(),
                        bpy.types.ShaderNodeTexImage,
                        {"Vector": End(node="mapping", socket="Vector")},
                        {"projection": projection.name, "projection_blend": blend},
                        Some(Loaded(path=path, tag=Tag(space=member.space.map(lambda role: config.getColorSpace(role).getName())))),
                    )
                    for member, path in maps.items()
                ),
                *((node("invert", bpy.types.ShaderNodeInvert, {"Fac": 1.0, "Color": textures[MapKind.GLOSS]}, {}),) if glossed else ()),
                *(node("normal_map", bpy.types.ShaderNodeNormalMap, {"Color": End(node="flip_join", socket="Color") if flipped else textures[member]}, {}) for member in normal.to_list()),
                *(
                    (
                        node("flip_split", bpy.types.ShaderNodeSeparateColor, {"Color": textures[MapKind.NORMAL_DX]}, {}),
                        node("flip_green", bpy.types.ShaderNodeMath, {"Value": 1.0, "Value_001": End(node="flip_split", socket="Green")}, {"operation": "SUBTRACT"}),
                        node(
                            "flip_join",
                            bpy.types.ShaderNodeCombineColor,
                            {"Red": End(node="flip_split", socket="Red"), "Green": End(node="flip_green"), "Blue": End(node="flip_split", socket="Blue")},
                            {},
                        ),
                    )
                    if flipped
                    else ()
                ),
                *(
                    (node("relief", bpy.types.ShaderNodeBump, {"Height": textures[MapKind.BUMP], **normal.map(lambda _: {"Normal": End(node="normal_map", socket="Normal")}).default_value({})}, {}),)
                    if bumped
                    else ()
                ),
                *(
                    (node("displacer", bpy.types.ShaderNodeDisplacement, {"Height": textures[MapKind.DISPLACEMENT], "Scale": displacement.value.scale, "Midlevel": displacement.value.midlevel}, {}),)
                    if displaced
                    else ()
                ),
            )
            shader = "BSDF"
        case Clay(color=color, roughness=roughness):
            parts = (node("surface", bpy.types.ShaderNodeBsdfPrincipled, {"Base Color": (*msgspec.structs.astuple(color), 1.0), "Roughness": roughness}, {}),)
            shader = "BSDF"
        case Emissive(color=color, strength=strength, image=image):
            parts = (
                node("surface", bpy.types.ShaderNodeEmission, {"Color": End(node="image", socket="Color") if image.enabled else (*msgspec.structs.astuple(color), 1.0), "Strength": strength}, {}),
                *((node("image", bpy.types.ShaderNodeTexImage, {}, {}, Some(Loaded(path=image.value))),) if image.enabled else ()),
            )
            shader = "Emission"
        case Utility(passes=passes):
            selected = {member.name.lower(): member for member in sorted(passes)}
            parts = (
                node("surface", bpy.types.ShaderNodeEmission, {"Color": (0.0, 0.0, 0.0, 1.0), "Strength": 0.0}, {}),
                *(node(f"{key}_source", member.source, {}, member.attribute.map(lambda name: {"attribute_name": name}).default_value({})) for key, member in selected.items()),
                *(node(key, bpy.types.ShaderNodeOutputAOV, {heading(member.kind): End(node=f"{key}_source", socket=member.output)}, {"aov_name": key}) for key, member in selected.items()),
            )
            shader = "Emission"
    output["Surface"] = End(node="surface", socket=shader)
    return Fragment.joined(*parts, node("output", bpy.types.ShaderNodeOutputMaterial, output, {"is_active_output": True})).map(
        lambda body: Graph(kind=bpy.types.ShaderNodeTree, nodes=body.nodes, links=body.links)
    )


# --- [HOLDERS]


def shaded(held: bpy.types.Material, setup: ShadingSetup, config: PyOpenColorIO.Config, identity: Identity) -> BlenderResult[bpy.types.Material]:
    """Reconciles an owned material and restores native displacement defaults on case switches."""
    return (
        paired(paired(material_graph(setup, config), tree_of(held)), Descriptor.resolve(bpy.types.Material, "displacement_method", bpy.types.EnumProperty))
        .bind(
            lambda found: reconciled(found[0][0], found[0][1], identity).bind(
                lambda _: found[1].write(held, "BOTH" if isinstance(setup, Textured) and setup.displacement.enabled else found[1].prop.default)
            )
        )
        .map(lambda _: held)
    )


def material(setup: ShadingSetup, config: PyOpenColorIO.Config, identity: Identity, scope: Option[ExitStack] = Nothing) -> BlenderResult[bpy.types.Material]:
    """Reuses or creates the owned material of a setup, stores its record, and reconciles its tree, a built material removed on refusal or when a given scope unwinds."""

    def built() -> BlenderResult[bpy.types.Material]:
        return kept(BlenderEditFault.attempted(setup.title, lambda: bpy.data.materials.new(setup.title)), emptied)

    return Materials.attachment.bound(identity).bind(
        lambda attached: setup.ownership.made(
            identity, built, filled=lambda held: tree_of(held).bind(lambda tree: attached.write(tree, Shading(setup=setup))).bind(lambda _: shaded(held, setup, config, identity)), scope=scope
        )
    )


def referenced(scene: bpy.types.Scene, context: bpy.types.Context) -> BlenderResult[tuple[Reconciled, ...]]:
    """Enables Blender's pre-modifier rest-position attribute on every object in a scene."""
    return fanned(ObjectState(values=Some(Snapshot.of(bpy.types.Object, {"add_rest_position_attribute": True}))), Scenes(names=(scene.name,)), context)


# --- [MATRIX]


def matrix() -> BlenderResult[NodeGroup]:
    """Builds the shared shader group applying output-row dot products and factored gain."""
    interface = Interface(
        outputs=(Socket(key="transformed", name="Color", kind=bpy.types.NodeSocketColor),),
        inputs=(
            Socket(key="color", name="Color", kind=bpy.types.NodeSocketColor),
            *(Socket(key=row, name=heading(row), kind=bpy.types.NodeSocketVector) for row in Rgb.__struct_fields__),
            Socket(key="gain", name="Gain", kind=bpy.types.NodeSocketFloat),
        ),
    )
    return (
        accumulated(vector(f"{row}_dot", "DOT_PRODUCT", End(node=INPUTS, socket="color"), End(node=INPUTS, socket=row)) for row in Rgb.__struct_fields__)
        .bind(lambda rows: node("combined", bpy.types.ShaderNodeCombineXYZ, dict(zip("XYZ", rows)), {}))
        .bind(lambda combined: vector("scaled", "SCALE", ended("combined", "Vector")(combined), End(node=INPUTS, socket="gain")))
        .bind(lambda scaled: NodeGroup.built("color_matrix", "Color Matrix", interface, {"transformed": scaled}, bpy.types.ShaderNodeTree))
    )


def matrix_inputs(calibration: Calibration) -> dict[Key, Value]:
    """Returns matrix rows and exposure gain for an owned color-matrix group."""
    return {**{name: msgspec.structs.astuple(row) for name, row in zip(Rgb.__struct_fields__, calibration.matrix)}, "gain": math.exp2(calibration.exposure)}


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
    "matrix_inputs",
    "referenced",
    "shaded",
]
