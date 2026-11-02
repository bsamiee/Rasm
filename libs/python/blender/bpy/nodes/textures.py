"""Stock node settings records as expressions and group sockets, and textures evolving over time by their dimensions."""

from collections.abc import Callable
import math
from types import MemberDescriptorType
from typing import Annotated, ClassVar

import bpy
from expression import Nothing, Option
import msgspec

from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.nodes.desired import Members
from blender.bpy.nodes.graphs import Expr, Scalar, SocketType, Term, xyz
from blender.bpy.nodes.interfaces import Socket, StockInput

# --- [TYPES] ----------------------------------------------------------------------------

type Linked = Callable[[MemberDescriptorType], Option[Term]]
type Texture = Noise | Voronoi | Wave | Magic | Gabor | WhiteNoise

# --- [MODELS] ---------------------------------------------------------------------------


class NodeSettings(msgspec.Struct, frozen=True, kw_only=True):
    """One stock node's stated values: fields its RNA declares are its members holding host identifiers, every other field the input socket its display name names."""

    kind: ClassVar[type[bpy.types.Node]]
    coordinates: ClassVar[tuple[str, ...]] = ("Vector", "W")

    @property
    def members(self) -> Members:
        """Values of the fields the node's RNA declares, by member name."""
        rna = self.kind.bl_rna.properties
        return {field: getattr(self, field) for field in self.__struct_fields__ if field in rna}

    @property
    def inputs(self) -> dict[str, str]:
        """Input socket identifier by field name, for every field the node's RNA declares no member of."""
        rna = self.kind.bl_rna.properties
        return {field: bpy.path.display_name(field) for field in self.__struct_fields__ if field not in rna}

    @property
    def sockets(self) -> tuple[Socket, ...]:
        """Group sockets of every input field at its value by field name, each declared as the stock node declares its input."""
        return tuple(Socket(key=field, kind=StockInput(node=self.kind, input=identifier), default_value=getattr(self, field)) for field, identifier in self.inputs.items())

    def node(self, *operands: Term, socket: str | int = 0, socket_type: SocketType = bpy.types.NodeSocketFloat, linked: Option[Linked] = Nothing) -> Expr:
        """Builds the node over its coordinate sockets and every field, the reconciler leaving sockets the members disable unlinked.

        Args:
            *operands: Coordinate operands in `coordinates` order (`Vector`, `W`), later coordinates keeping the node's own values.
            socket: Output identifier read, else position among the enabled outputs, the first enabled output by default.
            socket_type: Socket class of the output read.
            linked: Operand per input field member descriptor in place of its record value (the group input of its socket).

        Returns:
            Output expression.
        """
        inputs = tuple((identifier, linked.bind(lambda read: read(getattr(type(self), field))).default_value(getattr(self, field))) for field, identifier in self.inputs.items())
        return Expr(kind=self.kind, inputs=(*zip(self.coordinates, operands), *inputs), members=self.members, socket=socket, socket_type=socket_type)


class Noise(NodeSettings, frozen=True, kw_only=True):
    """Noise Texture node: distorted fractal Perlin noise."""

    kind = bpy.types.ShaderNodeTexNoise
    noise_dimensions: Annotated[str, Descriptor.declared(bpy.types.ShaderNodeTexNoise, ("noise_dimensions",), bpy.types.EnumProperty)] = "3D"
    noise_type: Annotated[str, Descriptor.declared(bpy.types.ShaderNodeTexNoise, ("noise_type",), bpy.types.EnumProperty)] = "FBM"
    normalize: bool = True
    scale: float = 5.0
    detail: float = 2.0
    roughness: float = 0.5
    lacunarity: float = 2.0
    offset: float = 0.0
    gain: float = 1.0
    distortion: float = 0.0


class Voronoi(NodeSettings, frozen=True, kw_only=True):
    """Voronoi Texture node: fractal cellular distances, colors, and feature points."""

    kind = bpy.types.ShaderNodeTexVoronoi
    voronoi_dimensions: Annotated[str, Descriptor.declared(bpy.types.ShaderNodeTexVoronoi, ("voronoi_dimensions",), bpy.types.EnumProperty)] = "3D"
    feature: Annotated[str, Descriptor.declared(bpy.types.ShaderNodeTexVoronoi, ("feature",), bpy.types.EnumProperty)] = "F1"
    distance: Annotated[str, Descriptor.declared(bpy.types.ShaderNodeTexVoronoi, ("distance",), bpy.types.EnumProperty)] = "EUCLIDEAN"
    normalize: bool = False
    scale: float = 5.0
    detail: float = 0.0
    roughness: float = 0.5
    lacunarity: float = 2.0
    smoothness: float = 1.0
    exponent: float = 0.5
    randomness: float = 1.0


class Wave(NodeSettings, frozen=True, kw_only=True):
    """Wave Texture node: bands or rings of a periodic profile, distorted by fractal noise."""

    kind = bpy.types.ShaderNodeTexWave
    coordinates = ("Vector",)
    wave_type: Annotated[str, Descriptor.declared(bpy.types.ShaderNodeTexWave, ("wave_type",), bpy.types.EnumProperty)] = "BANDS"
    bands_direction: Annotated[str, Descriptor.declared(bpy.types.ShaderNodeTexWave, ("bands_direction",), bpy.types.EnumProperty)] = "X"
    rings_direction: Annotated[str, Descriptor.declared(bpy.types.ShaderNodeTexWave, ("rings_direction",), bpy.types.EnumProperty)] = "X"
    wave_profile: Annotated[str, Descriptor.declared(bpy.types.ShaderNodeTexWave, ("wave_profile",), bpy.types.EnumProperty)] = "SIN"
    scale: float = 5.0
    distortion: float = 0.0
    detail: float = 2.0
    detail_scale: float = 1.0
    detail_roughness: float = 0.5
    phase_offset: float = 0.0


class Magic(NodeSettings, frozen=True, kw_only=True):
    """Magic Texture node: nested trigonometric turbulence."""

    kind = bpy.types.ShaderNodeTexMagic
    coordinates = ("Vector",)
    turbulence_depth: int = 2
    scale: float = 5.0
    distortion: float = 1.0


class Gabor(NodeSettings, frozen=True, kw_only=True):
    """Gabor Texture node: sparse windowed oscillations of one frequency and orientation."""

    kind = bpy.types.ShaderNodeTexGabor
    coordinates = ("Vector",)
    gabor_type: Annotated[str, Descriptor.declared(bpy.types.ShaderNodeTexGabor, ("gabor_type",), bpy.types.EnumProperty)] = "2D"
    scale: float = 5.0
    frequency: float = 2.0
    anisotropy: float = 1.0
    orientation_2d: float = math.pi / 4.0
    orientation_3d: tuple[float, float, float] = (math.sqrt(2.0), math.sqrt(2.0), 0.0)


class WhiteNoise(NodeSettings, frozen=True, kw_only=True):
    """White Noise Texture node: a uniform value and color hashed from the coordinates themselves."""

    kind = bpy.types.ShaderNodeTexWhiteNoise
    noise_dimensions: Annotated[str, Descriptor.declared(bpy.types.ShaderNodeTexWhiteNoise, ("noise_dimensions",), bpy.types.EnumProperty)] = "3D"


class MapRange(NodeSettings, frozen=True, kw_only=True):
    """Map Range node over floats, read at `Result`, the first enabled output."""

    kind = bpy.types.ShaderNodeMapRange
    coordinates = ("Value",)
    interpolation_type: Annotated[str, Descriptor.declared(bpy.types.ShaderNodeMapRange, ("interpolation_type",), bpy.types.EnumProperty)] = "LINEAR"
    clamp: bool = True
    from_min: float = 0.0
    from_max: float = 1.0
    to_min: float = 0.0
    to_max: float = 1.0
    steps: float = 4.0


# --- [OPERATIONS] -----------------------------------------------------------------------


def dimensions(texture: Texture, /) -> int:
    """Coordinate axes a texture reads by its dimensions member, Wave and Magic reading three."""
    match texture:
        case Noise(noise_dimensions=held) | WhiteNoise(noise_dimensions=held) | Voronoi(voronoi_dimensions=held) | Gabor(gabor_type=held):
            return int(held[0])
        case _:
            return 3


def evolving(texture: Texture, point: Expr, time: Scalar, /, *, socket: str | int = 0, socket_type: SocketType = bpy.types.NodeSocketFloat, linked: Option[Linked] = Nothing) -> Expr:
    """Builds a texture node at a point evolving over time by its dimensions: one reading the point's X as W, three taking time as Z, four as W, and two staying still.

    Args:
        texture: Texture settings.
        point: Coordinate the texture reads.
        time: Evolution coordinate.
        socket: Output identifier read, else position among the enabled outputs.
        socket_type: Socket class of the output read.
        linked: Operand per input field member descriptor in place of its record value.

    Returns:
        Output expression.
    """
    match dimensions(texture):
        case 1:
            coordinates: tuple[Term, ...] = (point, point["X"])
        case 3:
            coordinates = (point + xyz(z=time), time)
        case _:
            coordinates = (point, time)
    return texture.node(*coordinates, socket=socket, socket_type=socket_type, linked=linked)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Gabor", "Linked", "Magic", "MapRange", "NodeSettings", "Noise", "Texture", "Voronoi", "Wave", "WhiteNoise", "dimensions", "evolving"]
