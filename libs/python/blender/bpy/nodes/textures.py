"""Stock node settings records as expressions and group sockets, and textures evolving over time by their dimensions."""

from collections.abc import Callable, Mapping
import math
from types import MemberDescriptorType
from typing import Annotated, ClassVar

import bpy
from expression import Option
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.nodes.desired import Members
from blender.bpy.nodes.graphs import Expr, Scalar, SocketType, Term, xyz
from blender.bpy.nodes.interfaces import Socket, StockInput
from blender.records.faults import Error, Fault, Nothing, Ok, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Linked = Callable[[MemberDescriptorType], Option[Term]] | Mapping[object, Term]
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
        return {field: getattr(self, field) for field in self.__struct_fields__ if field in rna}  # ty: ignore[unsound-return-statement]

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
            linked: Operand mapping or reader per input field member descriptor, unlisted fields keeping their record values.

        Returns:
            Output expression.
        """
        read: Callable[[MemberDescriptorType, Term], Term]
        match linked.to_optional():
            case Mapping() as values:
                read = values.get  # ty: ignore[unsound-assignment]
            case None:
                read = lambda _member, value: value
            case reader:
                read = lambda member, value: reader(member).default_value(value)
        inputs = tuple((identifier, read(getattr(type(self), field), getattr(self, field))) for field, identifier in self.inputs.items())
        return Expr(kind=self.kind, inputs=(*zip(self.coordinates, operands), *inputs), members=self.members, socket=socket, socket_type=socket_type)


class Noise(NodeSettings, frozen=True, kw_only=True):
    """Noise Texture node: distorted fractal Perlin noise."""

    kind = bpy.types.ShaderNodeTexNoise
    noise_dimensions: Annotated[str, Descriptor(root=bpy.types.ShaderNodeTexNoise, prop=bpy.types.ShaderNodeTexNoise.bl_rna.properties["noise_dimensions"])] = "3D"
    noise_type: Annotated[str, Descriptor(root=bpy.types.ShaderNodeTexNoise, prop=bpy.types.ShaderNodeTexNoise.bl_rna.properties["noise_type"])] = "FBM"
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
    voronoi_dimensions: Annotated[str, Descriptor(root=bpy.types.ShaderNodeTexVoronoi, prop=bpy.types.ShaderNodeTexVoronoi.bl_rna.properties["voronoi_dimensions"])] = "3D"
    feature: Annotated[str, Descriptor(root=bpy.types.ShaderNodeTexVoronoi, prop=bpy.types.ShaderNodeTexVoronoi.bl_rna.properties["feature"])] = "F1"
    distance: Annotated[str, Descriptor(root=bpy.types.ShaderNodeTexVoronoi, prop=bpy.types.ShaderNodeTexVoronoi.bl_rna.properties["distance"])] = "EUCLIDEAN"
    normalize: bool = False
    scale: float = 5.0
    detail: float = 0.0
    roughness: float = 0.5
    lacunarity: float = 2.0
    smoothness: float = 1.0
    exponent: float = 0.5
    randomness: float = 1.0

    def color_bounds(self) -> Outcome[tuple[float, float]]:
        """Returns [0, 1] for finite CPU Geometry Nodes colors in 1D with zero detail or roughness in F1, F2, or Smooth F1 with nonpositive smoothing; other settings return a fault."""
        bounded = self.voronoi_dimensions == "1D" and (self.detail == 0.0 or self.roughness == 0.0) and (self.feature in {"F1", "F2"} or (self.feature == "SMOOTH_F1" and self.smoothness <= 0.0))
        return Ok((0.0, 1.0)) if bounded else Error(Fault.of(Reason.OUTSIDE, type(self).__name__, self, member="finite output bounds"))


class Wave(NodeSettings, frozen=True, kw_only=True):
    """Wave Texture node: bands or rings of a periodic profile, distorted by fractal noise."""

    kind = bpy.types.ShaderNodeTexWave
    coordinates = ("Vector",)
    wave_type: Annotated[str, Descriptor(root=bpy.types.ShaderNodeTexWave, prop=bpy.types.ShaderNodeTexWave.bl_rna.properties["wave_type"])] = "BANDS"
    bands_direction: Annotated[str, Descriptor(root=bpy.types.ShaderNodeTexWave, prop=bpy.types.ShaderNodeTexWave.bl_rna.properties["bands_direction"])] = "X"
    rings_direction: Annotated[str, Descriptor(root=bpy.types.ShaderNodeTexWave, prop=bpy.types.ShaderNodeTexWave.bl_rna.properties["rings_direction"])] = "X"
    wave_profile: Annotated[str, Descriptor(root=bpy.types.ShaderNodeTexWave, prop=bpy.types.ShaderNodeTexWave.bl_rna.properties["wave_profile"])] = "SIN"
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
    gabor_type: Annotated[str, Descriptor(root=bpy.types.ShaderNodeTexGabor, prop=bpy.types.ShaderNodeTexGabor.bl_rna.properties["gabor_type"])] = "2D"
    scale: float = 5.0
    frequency: float = 2.0
    anisotropy: float = 1.0
    orientation_2d: float = math.pi / 4.0
    orientation_3d: tuple[float, float, float] = (math.sqrt(2.0), math.sqrt(2.0), 0.0)


class WhiteNoise(NodeSettings, frozen=True, kw_only=True):
    """White Noise Texture node: a uniform value and color hashed from the coordinates themselves."""

    kind = bpy.types.ShaderNodeTexWhiteNoise
    noise_dimensions: Annotated[str, Descriptor(root=bpy.types.ShaderNodeTexWhiteNoise, prop=bpy.types.ShaderNodeTexWhiteNoise.bl_rna.properties["noise_dimensions"])] = "3D"


class MapRange(NodeSettings, frozen=True, kw_only=True):
    """Map Range node over floats, read at `Result`, the first enabled output."""

    kind = bpy.types.ShaderNodeMapRange
    coordinates = ("Value",)
    interpolation_type: Annotated[str, Descriptor(root=bpy.types.ShaderNodeMapRange, prop=bpy.types.ShaderNodeMapRange.bl_rna.properties["interpolation_type"])] = "LINEAR"
    clamp: bool = True
    from_min: float = 0.0
    from_max: float = 1.0
    to_min: float = 0.0
    to_max: float = 1.0
    steps: float = 4.0

    @np.errstate(over="raise", invalid="raise", under="ignore")
    def bounds(self) -> Outcome[tuple[float, float]]:
        """Encloses finite CPU Geometry Nodes outputs of record settings under separate round-to-nearest binary32 arithmetic, including flushed subnormals; unproved settings return a fault."""
        limits = np.finfo(np.float32)

        def rounded(values: npt.ArrayLike) -> npt.NDArray[np.float32]:
            native = np.asarray(values, dtype=np.float32)
            ends = np.asarray((native.min(), native.max()), dtype=np.float32)
            return np.where(np.abs(ends) <= limits.tiny, np.asarray((-limits.tiny, limits.tiny), dtype=np.float32), ends)

        try:
            targets = np.asarray((self.to_min, self.to_max), dtype=np.float32)
            if not np.isfinite(targets).all():
                return Error(Fault.of(Reason.OUTSIDE, type(self).__name__, self, member="finite binary32 targets"))
            a, b = (rounded(value) for value in targets)
            match self.interpolation_type:
                case "LINEAR" | "STEPPED" if self.clamp:
                    span = rounded(np.concatenate((a, b)))
                case "SMOOTHSTEP" | "SMOOTHERSTEP" as mode:
                    factor = 1.0 + (2.0 if mode == "SMOOTHSTEP" else 12.0) * limits.eps
                    delta = rounded(b[:, None] - a)
                    product = rounded(delta[:, None] * np.asarray((0.0, factor), dtype=np.float32))
                    span = rounded(a[:, None] + product)
                case _:
                    return Error(Fault.of(Reason.OUTSIDE, type(self).__name__, self, member="finite output bounds"))
            low, high = span
            return Ok((float(low), float(high)))
        except FloatingPointError as error:
            return Error(Fault.of(Reason.FAILED, repr(self), error))


# --- [OPERATIONS] -----------------------------------------------------------------------


def dimensions(texture: Texture, /) -> int:
    """Coordinate axes a texture reads by its dimensions member, Wave and Magic reading three."""
    match texture:
        case Noise(noise_dimensions=held) | WhiteNoise(noise_dimensions=held) | Voronoi(voronoi_dimensions=held) | Gabor(gabor_type=held):
            return int(held.removesuffix("D"))
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
