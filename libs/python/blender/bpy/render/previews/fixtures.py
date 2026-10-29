"""Preview scene fixtures: the owned surfaces, camera, and key light every preview render reuses, their poses, and the lighting records staging them."""

from collections.abc import Mapping
from contextlib import ExitStack
from enum import auto, Enum
from functools import partial
import math
from types import MappingProxyType
from typing import Final, Literal, overload

import bmesh
import bpy
from expression import Ok, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.data.ids.ownership import kept, Ownership, Saved
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.mirrors import choices
from blender.bpy.data.rna.paths import RnaPath
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderReason
from blender.bpy.scene.objects import spawned
from blender.bpy.scene.worlds import studio
from blender.records.faults import Fault, Outcome
from blender.records.fields import Case, NonNegative

# --- [TYPES] ----------------------------------------------------------------------------


class PreviewMesh(Enum):
    """Shader preview surface, named after the material preview type the host offers."""

    SPHERE = auto()
    CUBE = auto()
    FLAT = auto()


class Fixture(Enum):
    """Shared camera and key light fixtures."""

    CAMERA = auto()
    LAMP = auto()


class CameraPose(Enum):
    """Fixed preview camera: orthographic above the surface, panoramic at the world origin, or perspective into a world volume."""

    TOP = auto()
    PANORAMA = auto()
    INTERIOR = auto()


type Piece = PreviewMesh | Fixture
type Ambient = White | Studio
type Viewpoint = CameraPose | Fitted

# --- [CONSTANTS] ------------------------------------------------------------------------

CAMERAS: Final[Mapping[tuple[CameraPose, str | None], tuple[Mapping[RnaPath, Value], Mapping[RnaPath, Value]]]] = MappingProxyType({
    (CameraPose.TOP, None): ({("type",): "ORTHO", ("ortho_scale",): 2.0}, {("location",): (0.0, 0.0, 2.0), ("rotation_euler",): (0.0, 0.0, 0.0)}),
    (CameraPose.PANORAMA, "CYCLES"): ({("type",): "PANO", ("panorama_type",): "EQUIRECTANGULAR"}, {("location",): (0.0, 0.0, 0.0), ("rotation_euler",): (math.pi / 2.0, 0.0, 0.0)}),
    (CameraPose.PANORAMA, None): ({("type",): "PERSP", ("lens",): 12.0}, {("location",): (0.0, 0.0, 0.0), ("rotation_euler",): (1.3, 0.0, 0.0)}),
    (CameraPose.INTERIOR, None): ({("type",): "PERSP", ("lens",): 45.0}, {("location",): (0.0, -4.0, 0.0), ("rotation_euler",): (math.pi / 2.0, 0.0, 0.0)}),
})
SURFACE: Final = PreviewMesh.SPHERE
STUDIO: Final = "forest.exr"
WORLD_LIGHT: Final = 1.0
KEY_LIGHT: Final = 2.0
SURFACES: Final = choices(item for item in Descriptor.declared(bpy.types.Material, ("preview_render_type",), bpy.types.EnumProperty).prop.enum_items if item.identifier in PreviewMesh.__members__)

# --- [MODELS] ---------------------------------------------------------------------------


class White(Case, frozen=True):
    """Uniform white ambient light."""


class Studio(Case, frozen=True, kw_only=True):
    """Blender studio light environment."""

    name: str = STUDIO


class PreviewLighting(msgspec.Struct, frozen=True, kw_only=True):
    """Preview surface, ambient, and key strength."""

    surface: PreviewMesh = SURFACE
    ambient: Ambient = Studio()
    strength: NonNegative = WORLD_LIGHT
    key: NonNegative = KEY_LIGHT

    @property
    def environment(self) -> Outcome[Mapping[RnaPath, Value]]:
        """Environment members lighting a studio light or a flat white backdrop."""

        def lit(path: str) -> Mapping[RnaPath, Value]:
            return {("lighting", "mode"): "HDRI", ("lighting", "path"): path, ("lighting", "strength"): self.strength}

        match self.ambient:
            case White():
                return Ok({("lighting", "mode"): "COLOR", ("lighting", "color"): (1.0, 1.0, 1.0), ("lighting", "strength"): self.strength})
            case Studio(name=name):
                preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                return studio(name, preferences).map(lit)

    @property
    def lamp(self) -> Mapping[RnaPath, Value]:
        """Key sun at key strength as light object members, its type a `Light.type` item the host checks at the write."""
        return lit(self.key, (0.9, 0.15, 0.5))


class Fitted(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Orthographic camera fitted to its subject's bounds, the subject linked into the preview scene for the render."""

    subject: bpy.types.Object


# --- [OPERATIONS] -----------------------------------------------------------------------


def lit(energy: float, rotation: tuple[float, float, float]) -> Mapping[RnaPath, Value]:
    """Light object members of a sun at an energy and rotation."""
    return {("data", "type"): "SUN", ("data", "energy"): energy, ("location",): (0.0, 0.0, 0.0), ("rotation_euler",): rotation}


def piece_ownership(kind: Piece) -> tuple[Ownership[bpy.types.Object], Ownership[bpy.types.Mesh | bpy.types.Camera | bpy.types.Light]]:
    """Returns object and data ownership of a preview piece."""
    key = f"preview_{kind.name.lower()}"
    data = bpy.types.Mesh if isinstance(kind, PreviewMesh) else bpy.types.Camera if kind is Fixture.CAMERA else bpy.types.Light
    return Ownership(kind=bpy.types.Object, key=key, saved=Saved.NEVER), Ownership(kind=data, key=key, saved=Saved.NEVER)


@overload
def piece(identity: Identity, kind: PreviewMesh, scene: bpy.types.Scene) -> Outcome[tuple[bpy.types.Object, bpy.types.Mesh]]: ...


@overload
def piece(identity: Identity, kind: Literal[Fixture.CAMERA], scene: bpy.types.Scene) -> Outcome[tuple[bpy.types.Object, bpy.types.Camera]]: ...


@overload
def piece(identity: Identity, kind: Literal[Fixture.LAMP], scene: bpy.types.Scene) -> Outcome[tuple[bpy.types.Object, bpy.types.Light]]: ...


def piece(identity: Identity, kind: Piece, scene: bpy.types.Scene) -> Outcome[tuple[bpy.types.Object, bpy.types.Mesh | bpy.types.Camera | bpy.types.Light]]:
    """Returns owned piece data with its object linked once, placed at the piece's rest transform."""
    objects, data = piece_ownership(kind)
    name = bpy.path.display_name(data.key)

    def surfaced(shape: PreviewMesh, mesh: bpy.types.Mesh) -> Outcome[bpy.types.Mesh]:
        with ExitStack() as release:
            surface = bmesh.new()
            release.callback(surface.free)
            surface.loops.layers.uv.new("UVMap")
            match shape:
                case PreviewMesh.FLAT:
                    bmesh.ops.create_grid(surface, x_segments=1, y_segments=1, size=1.0, calc_uvs=True)
                case PreviewMesh.SPHERE:
                    bmesh.ops.create_uvsphere(surface, u_segments=48, v_segments=24, radius=0.92, calc_uvs=True)
                case PreviewMesh.CUBE:
                    bmesh.ops.create_cube(surface, size=1.0, calc_uvs=True)
            surface.to_mesh(mesh)
        (mesh.shade_smooth if shape is PreviewMesh.SPHERE else mesh.shade_flat)()
        return Ok(mesh)

    def built() -> Outcome[bpy.types.Mesh | bpy.types.Camera | bpy.types.Light]:
        match kind:
            case Fixture.CAMERA:
                return catch(bpy.data.cameras.new, exception=RuntimeError)(name).map_error(partial(Fault.of, BlenderReason.REFUSED, data.key))
            case Fixture.LAMP:
                return catch(bpy.data.lights.new, exception=RuntimeError)(name, type="SUN").map_error(partial(Fault.of, BlenderReason.REFUSED, data.key))
            case PreviewMesh() as shape:
                return kept(catch(bpy.data.meshes.new, exception=RuntimeError)(name).map_error(partial(Fault.of, BlenderReason.REFUSED, data.key)), lambda mesh: surfaced(shape, mesh))

    rest: Mapping[RnaPath, Value] = {
        ("location",): (0.0, 0.0, 0.0),
        ("rotation_euler",): (math.pi / 4.0, math.atan(math.sqrt(0.5)), 0.0) if kind is PreviewMesh.CUBE else (0.0, 0.0, 0.0),
        ("scale",): (1.04, 1.04, 1.0) if kind is PreviewMesh.FLAT else (0.98, 0.98, 0.98) if kind is PreviewMesh.CUBE else (1.0, 1.0, 1.0),
        ("hide_render",): isinstance(kind, PreviewMesh),
    }
    return data.made(identity, built).bind(
        lambda block: objects.made(identity, lambda: spawned(name, Some(block), scene.collection)).bind(lambda item: assigned(item, rest).map(lambda _: (item, block)))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CAMERAS",
    "KEY_LIGHT",
    "STUDIO",
    "SURFACE",
    "SURFACES",
    "WORLD_LIGHT",
    "Ambient",
    "CameraPose",
    "Fitted",
    "Fixture",
    "Piece",
    "PreviewLighting",
    "PreviewMesh",
    "Studio",
    "Viewpoint",
    "White",
    "lit",
    "piece",
    "piece_ownership",
]
