"""Owned surface setups, texture classification, shader AOVs, and color matrices."""

from collections.abc import Mapping
from contextlib import ExitStack
from enum import IntEnum, StrEnum
from functools import partial
from pathlib import Path, PurePath
import re
from types import MappingProxyType
from typing import ClassVar, Final, get_args, Literal

import bpy
from expression import Option, Some
from expression.extra.result import catch
import msgspec
import PyOpenColorIO

from rasm_bpy.commands.invocations import Open
from rasm_bpy.commands.operators import Answer, Call, Done, Empty, Operator, Unchanged
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import Acquisitions
from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._ids.images import Tag
from rasm_bpy.data._ids.ownership import _kept, _retained, Ownership, Saved
from rasm_bpy.data._rna.conditions import Present, Scenes
from rasm_bpy.data._rna.descriptors import Descriptor
from rasm_bpy.data._rna.snapshots import Applied, Snapshot
from rasm_bpy.nodes._content import Loaded
from rasm_bpy.nodes._desired import Graph, Members
from rasm_bpy.nodes._graphs import Expr, Term, vector, xyz
from rasm_bpy.nodes._trees import _emptied, _tree_of
from rasm_bpy.nodes.changes import reconciled
from rasm_bpy.nodes.functions import ColorExpr, grouped, lowered, VectorExpr
from rasm_bpy.scene._objects import _fanned, _ObjectState
from rasm_bpy.settings._attachments import Stored
from rasm_records import accumulated, Case, Fault, Finite, NonNegative, Nothing, Ok, Outcome, paired, Reason, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type _ShadingSetup = Textured | Clay | Emissive | Utility


class _MapKind(IntEnum):
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


class _TextureProjection(StrEnum):
    """Image projection valued by the Texture Coordinate output it reads."""

    FLAT = "UV"
    BOX = "Object"


class _DataPass(IntEnum):
    """Shader AOV purpose, its AOV named after the member."""

    DEPTH = 1
    POSITION = 2
    REFERENCE = 3

    @property
    def kind(self) -> Literal["VALUE", "COLOR"]:
        """AOV type the source output's socket shape takes."""
        return "VALUE" if _AOVS[self].socket_type is bpy.types.NodeSocketFloat else "COLOR"


# --- [CONSTANTS] ------------------------------------------------------------------------

_ACTIVE: Final = Descriptor[bpy.types.PointerProperty](root=bpy.types.ViewLayer, owner=("objects",), prop=bpy.types.LayerObjects.bl_rna.properties["active"])  # ty: ignore[invalid-argument-type]
_ACTIVE_MATERIAL: Final = Descriptor(root=bpy.types.Object, prop=bpy.types.Object.bl_rna.properties["active_material"])
_DISPLACEMENT_METHOD: Final = Descriptor[bpy.types.EnumProperty](root=bpy.types.Material, prop=bpy.types.Material.bl_rna.properties["displacement_method"])  # ty: ignore[invalid-argument-type]
_IMAGES: Final[tuple[str, ...]] = tuple(sorted(bpy.path.extensions_image))  # ty: ignore[unresolved-attribute]

# --- [MODELS] ---------------------------------------------------------------------------


class _MapRole(msgspec.Struct, frozen=True, kw_only=True):
    """Filename words, colorspace role or name `Tag` resolves, and Principled BSDF input of one map kind."""

    words: tuple[str, ...]
    space: Option[str] = Some(PyOpenColorIO.ROLE_DATA)
    socket: Option[str] = Nothing


class _DisplacementMap(msgspec.Struct, frozen=True, kw_only=True):
    """Height map scale and map value at undisplaced surface."""

    scale: Finite = 1.0
    midlevel: Finite = 0.5


class Textured(Case, frozen=True, kw_only=True):
    """Principled surface from texture set."""

    ownership: ClassVar[Ownership[bpy.types.Material]] = Ownership(kind=bpy.types.Material, key="textured", saved=Saved.USED, single=False)
    maps: Mapping[_MapKind, str] = msgspec.field(default_factory=dict)
    projection: _TextureProjection = _TextureProjection.FLAT
    projection_blend: Unit = 0.0
    repeat: tuple[Finite, Finite] = (1.0, 1.0)
    displacement: _DisplacementMap | None = None


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
    passes: frozenset[_DataPass] = frozenset(_DataPass)


class Materials(msgspec.Struct, frozen=True, kw_only=True):
    """Setup ownerships, color matrix group, and texture set operator."""

    @staticmethod
    def acquisitions(_identity: Identity, /) -> Acquisitions:
        """Returns the ownership of each setup case, matrix group, and texture set operator."""
        cases: tuple[type[_ShadingSetup], ...] = get_args(_ShadingSetup.__value__)  # ty: ignore[unsound-assignment]
        return (*(case.ownership for case in cases), _color_matrix.group, texture_setup)


# --- [TABLES] ---------------------------------------------------------------------------

_MAPS: Final[Mapping[_MapKind, _MapRole]] = MappingProxyType({
    _MapKind.BASE_COLOR: _MapRole(words=("basecolor", "base_color", "albedo", "diffuse", "diff", "colour", "color", "col"), space=Nothing, socket=Some("Base Color")),
    _MapKind.METALLIC: _MapRole(words=("metallic", "metalness", "metal"), socket=Some("Metallic")),
    _MapKind.SPECULAR: _MapRole(words=("specular", "spec"), socket=Some("Specular IOR Level")),
    _MapKind.GLOSS: _MapRole(words=("glossiness", "gloss", "smoothness")),
    _MapKind.ROUGHNESS: _MapRole(words=("roughness", "rough"), socket=Some("Roughness")),
    _MapKind.NORMAL_DX: _MapRole(words=("normal_dx", "normaldx", "nor_dx", "nrm_dx")),
    _MapKind.NORMAL: _MapRole(words=("normal_gl", "normal", "nor", "nrm", "norm")),
    _MapKind.BUMP: _MapRole(words=("bump",)),
    _MapKind.DISPLACEMENT: _MapRole(words=("displacement", "disp", "height")),
    _MapKind.TRANSMISSION: _MapRole(words=("transmission", "transmissive"), socket=Some("Transmission Weight")),
    _MapKind.EMISSION: _MapRole(words=("emission", "emissive", "emit"), space=Nothing, socket=Some("Emission Color")),
    _MapKind.ALPHA: _MapRole(words=("alpha", "opacity"), socket=Some("Alpha")),
})
_AOVS: Final[Mapping[_DataPass, Expr]] = MappingProxyType({
    _DataPass.DEPTH: Expr(kind=bpy.types.ShaderNodeCameraData, socket="View Z Depth"),
    _DataPass.POSITION: Expr(kind=bpy.types.ShaderNodeNewGeometry, socket="Position", socket_type=bpy.types.NodeSocketVector),
    _DataPass.REFERENCE: Expr(kind=bpy.types.ShaderNodeAttribute, members={"attribute_name": "rest_position"}, socket="Vector", socket_type=bpy.types.NodeSocketVector),
})

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CLASSIFICATION]


def _map_kind(path: str) -> Option[_MapKind]:
    """Returns kind of longest filename word match, each case change a word boundary, `Nothing` on no match or tie."""
    stem = re.sub(r"(?<=[a-z])(?=[A-Z])", "_", PurePath(path).stem)
    found = tuple((len(word), member) for member, role in _MAPS.items() for word in role.words if re.search(rf"(?<![a-z]){re.escape(word)}(?![a-z])", stem, re.IGNORECASE))
    longest = max((size for size, _ in found), default=0)
    match tuple(frozenset(member for size, member in found if size == longest)):
        case (only,):
            return Some(only)
        case _:
            return Nothing


def _classified(paths: tuple[str, ...]) -> Outcome[dict[_MapKind, str]]:
    """Classifies files by map kind, accumulating each later file of a kind as a collision with its first."""
    first: dict[_MapKind, str] = {}
    return accumulated(
        Ok((member, path)).filter_with(lambda pair: first.setdefault(*pair) == pair[1], lambda pair: Fault.of(Refusal.TAKEN, pair[1], member=first[pair[0]]))
        for path in paths
        for member in _map_kind(path).to_list()
    ).map(dict)


# --- [GRAPHS]


def _material_graph(setup: _ShadingSetup) -> Outcome[Graph]:
    """Builds shader graph of setup, its output and every AOV output lowered as roots, map colorspaces resolved by `Tag` on load."""

    def image(path: str, tag: Tag, inputs: tuple[tuple[str, Term], ...] = (), members: Members = MappingProxyType({})) -> Expr:
        return Expr(kind=bpy.types.ShaderNodeTexImage, inputs=inputs, members=members, content=Some(Loaded(path=Path(path), tag=tag)), socket="Color", socket_type=bpy.types.NodeSocketColor)

    def output(surface: Expr, displacement: Option[Expr] = Nothing) -> Expr:
        return Expr(kind=bpy.types.ShaderNodeOutputMaterial, inputs=(("Surface", surface), *(("Displacement", held) for held in displacement.to_list())), members={"is_active_output": True})

    def textured(setup: Textured) -> Expr:
        maps, displacement = setup.maps, Option.of_optional(setup.displacement)
        mapping = Expr(
            kind=bpy.types.ShaderNodeMapping,
            inputs=(("Vector", Expr(kind=bpy.types.ShaderNodeTexCoord, socket=setup.projection.value, socket_type=bpy.types.NodeSocketVector)), ("Scale", (*setup.repeat, 1.0))),
            socket="Vector",
            socket_type=bpy.types.NodeSocketVector,
        )
        textures = {
            member: image(path, Tag(space=_MAPS[member].space), (("Vector", mapping),), {"projection": setup.projection.name, "projection_blend": setup.projection_blend})
            for member, path in maps.items()
        }
        normal = Option.of_optional(textures.get(_MapKind.NORMAL)).or_else_with(
            lambda: Option.of_optional(textures.get(_MapKind.NORMAL_DX)).map(lambda texture: vector("MULTIPLY_ADD", texture, (1.0, -1.0, 1.0), (0.0, 1.0, 0.0)))
        )
        mapped = normal.map(lambda color: Expr(kind=bpy.types.ShaderNodeNormalMap, inputs=(("Color", color),), socket="Normal", socket_type=bpy.types.NodeSocketVector))
        relief = Option.of_optional(textures.get(_MapKind.BUMP)).map(
            lambda height: Expr(kind=bpy.types.ShaderNodeBump, inputs=(("Height", height), *(("Normal", held) for held in mapped.to_list())), socket="Normal", socket_type=bpy.types.NodeSocketVector)
        )
        glossed = Option.of_optional(textures.get(_MapKind.GLOSS)).filter(lambda _: _MapKind.ROUGHNESS not in maps)
        surface = Expr(
            kind=bpy.types.ShaderNodeBsdfPrincipled,
            inputs=(
                *((socket, textures[member]) for member in maps for socket in _MAPS[member].socket.to_list()),
                *((("Emission Strength", 1.0),) if _MapKind.EMISSION in maps else ()),
                *(("Roughness", 1.0 - gloss) for gloss in glossed.to_list()),
                *(("Normal", held) for held in relief.or_else(mapped).to_list()),
            ),
            socket="BSDF",
        )
        displaced = Option.of_optional(textures.get(_MapKind.DISPLACEMENT)).bind(lambda height: displacement.map(lambda held: (height, held)))
        return output(
            surface,
            displaced.map(
                lambda pair: Expr(
                    kind=bpy.types.ShaderNodeDisplacement,
                    inputs=(("Height", pair[0]), ("Scale", pair[1].scale), ("Midlevel", pair[1].midlevel)),
                    socket="Displacement",
                    socket_type=bpy.types.NodeSocketVector,
                )
            ),
        )

    def emission(color: Term, strength: float) -> Expr:
        return Expr(kind=bpy.types.ShaderNodeEmission, inputs=(("Color", color), ("Strength", strength)), socket="Emission")

    def roots() -> tuple[Expr, ...]:
        match setup:
            case Textured():
                return (textured(setup),)
            case Clay(color=color, roughness=roughness):
                return (output(Expr(kind=bpy.types.ShaderNodeBsdfPrincipled, inputs=(("Base Color", (*color, 1.0)), ("Roughness", roughness)), socket="BSDF")),)
            case Emissive(color=color, strength=strength, image=path):
                return (output(emission(Option.of_optional(path).map(lambda chosen: image(chosen, Tag())).default_value((*color, 1.0)), strength)),)
            case Utility(passes=passes):
                return (
                    output(emission((0.0, 0.0, 0.0, 1.0), 0.0)),
                    *(Expr(kind=bpy.types.ShaderNodeOutputAOV, inputs=((member.kind.capitalize(), _AOVS[member]),), members={"aov_name": member.name.lower()}) for member in sorted(passes)),
                )

    return lowered(bpy.types.ShaderNodeTree, *roots())


# --- [HOLDERS]


def _shaded(held: bpy.types.Material, setup: _ShadingSetup, identity: Identity) -> Outcome[bpy.types.Material]:
    """Reconciles owned material tree, `BOTH` displacement method when enabled."""
    method = "BOTH" if isinstance(setup, Textured) and setup.displacement is not None else _DISPLACEMENT_METHOD.prop.default
    return paired(_material_graph(setup), _tree_of(held)).bind(lambda found: reconciled(*found, identity)).bind(lambda _: _DISPLACEMENT_METHOD.write(held, method)).map(lambda _: held)


def _material(setup: _ShadingSetup, identity: Identity, scope: Option[ExitStack] = Nothing) -> Outcome[bpy.types.Material]:
    """Reuses or creates owned setup material, storing the setup on its tree, removing a created one on fault or scope unwind."""

    def built() -> Outcome[bpy.types.Material]:
        return _kept(catch(bpy.data.materials.new, exception=RuntimeError)(bpy.path.display_name(setup.ownership.key)).map_error(partial(Fault.of, Refusal.REFUSED, setup.ownership.key)), _emptied)

    return setup.ownership.made(identity, built, filled=lambda held: _tree_of(held).bind(lambda tree: stored(identity).write(tree, setup)).bind(lambda _: _shaded(held, setup, identity)), scope=scope)


def stored(identity: Identity) -> Stored[_ShadingSetup]:
    """Returns the store of the shading setup an owned material tree holds."""
    return Stored.keyed(identity, "shading", msgspec.msgpack.Decoder(_ShadingSetup))


def referenced(scene: bpy.types.Scene, context: bpy.types.Context) -> Outcome[tuple[Applied, ...]]:
    """Enables rest position attribute on every scene object."""
    return _fanned(_ObjectState(values=Some(Snapshot.of(bpy.types.Object, {("add_rest_position_attribute",): True}))), Scenes(names=(scene.name,)), context)


# --- [COMPOSITION] ----------------------------------------------------------------------


@grouped(kind=bpy.types.ShaderNodeTree)
def _color_matrix(color: ColorExpr, red: VectorExpr, green: VectorExpr, blue: VectorExpr, gain: Expr) -> ColorExpr:
    """Shared shader group multiplying color by matrix rows and gain."""
    return vector("SCALE", xyz(*(vector("DOT_PRODUCT", color, row) for row in (red, green, blue))), gain)


@Operator.of(Empty, condition=Some(Present(descriptor=_ACTIVE)), invoke=Some(Open[Empty](extensions=lambda _: _IMAGES)))
def texture_setup(call: Call[Empty]) -> Outcome[Answer]:
    """Builds texture set material from chosen files into active object's active slot, noting unmatched files."""
    paths = tuple(map(str, call.paths))
    answer: Outcome[Answer] = Ok(Unchanged(reason="No chosen file matches a map kind"))

    def assigned(maps: dict[_MapKind, str], rollback: ExitStack) -> Outcome[dict[_MapKind, str]]:
        return (
            paired(
                Option.of_optional(call.context.active_object).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(call.context), member="active_object")),
                _material(Textured(maps=maps), call.identity, Some(rollback)),
            )
            .bind(lambda pair: _ACTIVE_MATERIAL.write(*pair))
            .map(lambda _: maps)
        )

    def published(maps: dict[_MapKind, str]) -> Done:
        matched = frozenset(maps.values())
        return Done(notes=tuple(("INFO", f"{PurePath(path).name} matches no map kind") for path in paths if path not in matched))

    with ExitStack() as rollback:
        return _retained(_classified(paths).bind(lambda maps: assigned(maps, rollback).map(published) if maps else answer), rollback)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Clay", "Emissive", "Materials", "Textured", "Utility", "referenced", "stored", "texture_setup"]
