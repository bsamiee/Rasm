"""Stock nodes as expressions: settings records split into the node's RNA members and input sockets, and texture and Map Range records built through one stock builder."""

from collections.abc import Callable, Mapping
import math
from types import MemberDescriptorType
from typing import ClassVar

import bpy
from expression import Nothing, Option
import msgspec
import msgspec.structs

from blender.bpy.nodes.desired import Members
from blender.bpy.nodes.graphs import Expr, Shape, Term

# --- [TYPES] ----------------------------------------------------------------------------

type Linked = Callable[[MemberDescriptorType], Option[Term]]

# --- [MODELS] ---------------------------------------------------------------------------


class NodeSettings(msgspec.Struct, frozen=True, kw_only=True):
    """One stock node's stated values: fields its RNA declares are its members holding host identifiers, every other field the input socket its display name names."""

    kind: ClassVar[type[bpy.types.Node]]

    def stocked(self, coordinates: Mapping[str, Term], *, socket: str | int = 0, shape: Shape = bpy.types.NodeSocketFloat, linked: Option[Linked] = Nothing) -> Expr:
        """Builds the node over coordinate sockets and every field, the reconciler leaving sockets the members disable unlinked.

        Args:
            coordinates: Coordinate socket operands by identifier, such as `Vector` and `W`.
            socket: Output identifier read, else position among the enabled outputs, the first enabled output by default.
            shape: Socket class of the output read.
            linked: Operand per input field member descriptor in place of its record value, such as a group input animating it.

        Returns:
            Output expression.
        """
        rna = self.kind.bl_rna.properties
        fields = msgspec.structs.fields(self)
        members: Members = {field.name: getattr(self, field.name) for field in fields if field.name in rna}
        inputs = {
            bpy.path.display_name(field.name): linked.bind(lambda read: read(getattr(type(self), field.name))).default_value(getattr(self, field.name)) for field in fields if field.name not in rna
        }
        return stock(self.kind, members, {**coordinates, **inputs}, socket=socket, shape=shape)


class Texture(NodeSettings, frozen=True, kw_only=True):
    """Texture node reading `Vector` and, where it declares one, `W` coordinates, the node's dimensions enabling the ones it reads."""

    coordinates: ClassVar[tuple[str, ...]] = ("Vector", "W")

    def node(self, vector: Term, w: Term = 0.0, *, socket: str | int = 0, shape: Shape = bpy.types.NodeSocketFloat, linked: Option[Linked] = Nothing) -> Expr:
        """Builds the texture node over the coordinate sockets it declares, `w` unread by a node without `W`."""
        return self.stocked(dict(zip(self.coordinates, (vector, w))), socket=socket, shape=shape, linked=linked)


class Noise(Texture, frozen=True, kw_only=True):
    """Noise Texture node: distorted fractal Perlin noise."""

    kind = bpy.types.ShaderNodeTexNoise
    noise_dimensions: str = "3D"
    noise_type: str = "FBM"
    normalize: bool = True
    scale: float = 5.0
    detail: float = 2.0
    roughness: float = 0.5
    lacunarity: float = 2.0
    offset: float = 0.0
    gain: float = 1.0
    distortion: float = 0.0


class Voronoi(Texture, frozen=True, kw_only=True):
    """Voronoi Texture node: fractal cellular distances, colors, and feature points."""

    kind = bpy.types.ShaderNodeTexVoronoi
    voronoi_dimensions: str = "3D"
    feature: str = "F1"
    distance: str = "EUCLIDEAN"
    normalize: bool = False
    scale: float = 5.0
    detail: float = 0.0
    roughness: float = 0.5
    lacunarity: float = 2.0
    smoothness: float = 1.0
    exponent: float = 0.5
    randomness: float = 1.0


class Wave(Texture, frozen=True, kw_only=True):
    """Wave Texture node: bands or rings of a periodic profile, distorted by fractal noise."""

    kind = bpy.types.ShaderNodeTexWave
    coordinates = ("Vector",)
    wave_type: str = "BANDS"
    bands_direction: str = "X"
    rings_direction: str = "X"
    wave_profile: str = "SIN"
    scale: float = 5.0
    distortion: float = 0.0
    detail: float = 2.0
    detail_scale: float = 1.0
    detail_roughness: float = 0.5
    phase_offset: float = 0.0


class Magic(Texture, frozen=True, kw_only=True):
    """Magic Texture node: nested trigonometric turbulence."""

    kind = bpy.types.ShaderNodeTexMagic
    coordinates = ("Vector",)
    turbulence_depth: int = 2
    scale: float = 5.0
    distortion: float = 1.0


class Gabor(Texture, frozen=True, kw_only=True):
    """Gabor Texture node: sparse windowed oscillations of one frequency and orientation."""

    kind = bpy.types.ShaderNodeTexGabor
    coordinates = ("Vector",)
    gabor_type: str = "2D"
    scale: float = 5.0
    frequency: float = 2.0
    anisotropy: float = 1.0
    orientation_2d: float = math.pi / 4.0
    orientation_3d: tuple[float, float, float] = (math.sqrt(2.0), math.sqrt(2.0), 0.0)


class White(Texture, frozen=True, kw_only=True):
    """White Noise Texture node: a uniform value and color hashed from the coordinates themselves."""

    kind = bpy.types.ShaderNodeTexWhiteNoise
    noise_dimensions: str = "3D"


class MapRange(NodeSettings, frozen=True, kw_only=True):
    """Map Range node over floats."""

    kind = bpy.types.ShaderNodeMapRange
    interpolation_type: str = "LINEAR"
    clamp: bool = True
    from_min: float = 0.0
    from_max: float = 1.0
    to_min: float = 0.0
    to_max: float = 1.0
    steps: float = 4.0

    def node(self, value: Term, *, linked: Option[Linked] = Nothing) -> Expr:
        """Builds the Map Range node over `Value`, read at `Result`."""
        return self.stocked({"Value": value}, socket="Result", linked=linked)


# --- [OPERATIONS] -----------------------------------------------------------------------


def stock(kind: type[bpy.types.Node], members: Members, inputs: Mapping[str, Term], /, *, socket: str | int = 0, shape: Shape = bpy.types.NodeSocketFloat) -> Expr:
    """Builds a stock node over its RNA members and input operands.

    Args:
        kind: Stock node class.
        members: Member values by member identifier.
        inputs: Input operands by socket identifier.
        socket: Output identifier read, else position among the enabled outputs.
        shape: Socket class of the output read.

    Returns:
        Output expression.
    """
    return Expr(kind=kind, inputs=tuple(inputs.items()), members=members, socket=socket, shape=shape)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Gabor", "Linked", "Magic", "MapRange", "NodeSettings", "Noise", "Texture", "Voronoi", "Wave", "White", "stock"]
