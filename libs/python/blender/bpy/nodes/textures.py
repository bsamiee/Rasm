"""Texture and Map Range nodes as records that lower to node expressions and evaluate on the CPU through Blender's own hashes."""

from collections.abc import Callable
import functools
import itertools
import math
import typing
from typing import Annotated, ClassVar, Final, Literal, override

import bpy
from expression import Nothing, Option
import msgspec
import msgspec.structs
import numpy as np
import numpy.typing as npt

from blender.bpy.nodes.graphs import DataType, Expr, Scalar, Term
from blender.imaging.geometry.arithmetic import Field, Namespace
from blender.records.fields import Finite, NonNegative, Unit
from blender.records.identity import content_hash

# --- [TYPES] ----------------------------------------------------------------------------

type Floats = npt.NDArray[np.float32]
type Cells = npt.NDArray[np.int32]
type Linked = Callable[[msgspec.structs.FieldInfo], Option[Scalar]]
type Dimensions = Literal["1D", "2D", "3D", "4D"]
type WaveProfile = Literal["SIN", "SAW", "TRI"]

# --- [CONSTANTS] ------------------------------------------------------------------------

_PERLIN_SCALES: Final = (np.float32(0.25), np.float32(0.6616), np.float32(0.9820), np.float32(0.8344))
_EPSILON: Final = float(np.finfo(np.float32).eps)
_TAU: Final = 2.0 * np.pi

# --- [MODELS] ---------------------------------------------------------------------------


class NodeSettings(msgspec.Struct, frozen=True, kw_only=True):
    """One node's settings: fields its RNA declares are its members, every other field the input socket its display name names."""

    kind: ClassVar[type[bpy.types.ShaderNode]]
    outputs: ClassVar[tuple[tuple[str, DataType], ...]]

    @property
    def primary(self) -> str:
        """Output a node form reads unless the caller names another."""
        return self.outputs[0][0]

    @property
    def unfed(self) -> frozenset[object]:
        """Member descriptors of input fields whose sockets the current settings leave unavailable."""
        return frozenset()

    def lowered(self, coordinates: tuple[tuple[str, Term], ...], socket: Option[str], linked: Option[Linked]) -> Expr:
        """Builds the node over coordinate sockets and every available input field.

        Args:
            coordinates: Coordinate socket operands ahead of the input fields.
            socket: Output identifier read, the primary output when absent.
            linked: Operand per input field in place of its record value, such as a group input animating it.

        Returns:
            Output expression in the output's shape.
        """
        rna: bpy.types.Struct = self.kind.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        fields = msgspec.structs.fields(self)
        settings = tuple(field for field in fields if field.name in rna.properties)
        sockets = tuple(field for field in fields if field not in settings and getattr(type(self), field.name) not in self.unfed)
        chosen = socket.default_value(self.primary)
        return Expr(
            kind=self.kind,
            inputs=(*coordinates, *((bpy.path.display_name(field.name), linked.bind(lambda read: read(field)).default_value(getattr(self, field.name))) for field in sockets)),
            members=tuple((field.name, getattr(self, field.name)) for field in settings),
            socket=chosen,
            shape=dict(self.outputs)[chosen],
        )


class Texture(NodeSettings, frozen=True, kw_only=True):
    """Texture node reading `Vector` and `W` coordinates by its rank."""

    @property
    def rank(self) -> int:
        """Coordinate axes the texture reads."""
        return 3

    def node(self, vector: Term, w: Scalar = 0.0, *, socket: Option[str] = Nothing, linked: Option[Linked] = Nothing) -> Expr:
        """Builds the texture node, `Vector` read above rank 1 and `W` at ranks 1 and 4.

        Args:
            vector: Coordinate the `Vector` socket reads.
            w: Coordinate the `W` socket reads.
            socket: Output identifier read, the primary output when absent.
            linked: Operand per input field in place of its record value.

        Returns:
            Output expression in the output's shape.
        """
        return self.lowered((*((("Vector", vector),) if self.rank != 1 else ()), *((("W", w),) if self.rank in {1, 4} else ())), socket, linked)

    def coordinates(self, vector: npt.ArrayLike, w: npt.ArrayLike) -> Floats:
        """Stacks the `Vector` and `W` columns the rank reads, one row per point."""
        held = np.asarray(vector, dtype=np.float32).reshape(-1, 3)
        fourth = np.broadcast_to(np.asarray(w, dtype=np.float32), len(held))[:, None]
        return np.ascontiguousarray(fourth if self.rank == 1 else held[:, : self.rank] if self.rank < 4 else np.concatenate((held, fourth), axis=1))


class Noise(Texture, frozen=True, kw_only=True):
    """Noise Texture node: distorted fractal Perlin noise."""

    kind = bpy.types.ShaderNodeTexNoise
    outputs = (("Fac", "FLOAT"), ("Color", "RGBA"))
    noise_dimensions: Dimensions = "3D"
    noise_type: Literal["MULTIFRACTAL", "RIDGED_MULTIFRACTAL", "HYBRID_MULTIFRACTAL", "FBM", "HETERO_TERRAIN"] = "FBM"
    normalize: bool = True
    scale: Annotated[float, msgspec.Meta(ge=-1000.0, le=1000.0)] = 5.0
    detail: Annotated[float, msgspec.Meta(ge=0.0, le=15.0)] = 2.0
    roughness: Unit = 0.5
    lacunarity: Annotated[float, msgspec.Meta(ge=0.0, le=1000.0)] = 2.0
    offset: Annotated[float, msgspec.Meta(ge=-1000.0, le=1000.0)] = 0.0
    gain: Annotated[float, msgspec.Meta(ge=0.0, le=1000.0)] = 1.0
    distortion: Annotated[float, msgspec.Meta(ge=-1000.0, le=1000.0)] = 0.0

    @property
    @override
    def rank(self) -> int:
        """Axes of `noise_dimensions`."""
        return _rank(self.noise_dimensions)

    @property
    @override
    def unfed(self) -> frozenset[object]:
        """`Offset` outside the offset sums, `Gain` outside hybrid and ridged sums."""
        return frozenset((*((Noise.offset,) if self.noise_type in {"MULTIFRACTAL", "FBM"} else ()), *((Noise.gain,) if self.noise_type not in {"HYBRID_MULTIFRACTAL", "RIDGED_MULTIFRACTAL"} else ())))

    def evaluated(self, vector: npt.ArrayLike, w: npt.ArrayLike = 0.0) -> dict[str, Floats]:
        """Evaluates `Fac` and RGB `Color` as the node's CPU function does, seed offsets in [100, 200] from the float hash."""
        position = self.coordinates(vector, w) * np.float32(self.scale)
        rank, strength = position.shape[1], np.float32(self.distortion)

        def offsets(seed: int) -> Floats:
            draws = _hash_float(np.float32(seed)) if rank == 1 else _hash_float(np.float32(seed), np.arange(rank, dtype=np.float32))
            return np.float32(100.0) + draws * np.float32(100.0)

        warped = position + np.stack(tuple(_perlin(position + offsets(axis)) * strength for axis in range(rank)), axis=-1) if strength != 0.0 else position
        color = np.stack(tuple(_fractal(self, warped + shift) for shift in (np.float32(0.0), offsets(rank), offsets(rank + 1))), axis=-1)
        return {"Fac": color[:, 0], "Color": color}


class Voronoi(Texture, frozen=True, kw_only=True):
    """Voronoi Texture node: fractal cellular distances, colors, and feature points."""

    kind = bpy.types.ShaderNodeTexVoronoi
    outputs = (("Distance", "FLOAT"), ("Color", "RGBA"), ("Position", "VECTOR"), ("W", "FLOAT"), ("Radius", "FLOAT"))
    voronoi_dimensions: Dimensions = "3D"
    feature: Literal["F1", "F2", "SMOOTH_F1", "DISTANCE_TO_EDGE", "N_SPHERE_RADIUS"] = "F1"
    distance: Literal["EUCLIDEAN", "MANHATTAN", "CHEBYCHEV", "MINKOWSKI"] = "EUCLIDEAN"
    normalize: bool = False
    scale: Annotated[float, msgspec.Meta(ge=-1000.0, le=1000.0)] = 5.0
    detail: Annotated[float, msgspec.Meta(ge=0.0, le=15.0)] = 0.0
    roughness: Unit = 0.5
    lacunarity: Annotated[float, msgspec.Meta(ge=0.0, le=1000.0)] = 2.0
    smoothness: Unit = 1.0
    exponent: Annotated[float, msgspec.Meta(ge=0.0, le=32.0)] = 0.5
    randomness: Unit = 1.0

    @property
    @override
    def rank(self) -> int:
        """Axes of `voronoi_dimensions`."""
        return _rank(self.voronoi_dimensions)

    @property
    @override
    def primary(self) -> str:
        """`Radius` for the n-sphere feature, else `Distance`."""
        return "Radius" if self.feature == "N_SPHERE_RADIUS" else "Distance"

    @property
    @override
    def unfed(self) -> frozenset[object]:
        """`Smoothness` outside smooth F1, `Exponent` outside ranked Minkowski distances, octave inputs at the n-sphere feature."""
        return frozenset((
            *((Voronoi.smoothness,) if self.feature != "SMOOTH_F1" else ()),
            *((Voronoi.exponent,) if self.distance != "MINKOWSKI" or self.rank == 1 or self.feature in {"DISTANCE_TO_EDGE", "N_SPHERE_RADIUS"} else ()),
            *((Voronoi.detail, Voronoi.roughness, Voronoi.lacunarity) if self.feature == "N_SPHERE_RADIUS" else ()),
        ))

    def evaluated(self, vector: npt.ArrayLike, w: npt.ArrayLike = 0.0) -> dict[str, Floats]:
        """Evaluates the outputs the feature offers as the node's CPU function does, `Color` as RGB.

        Cell points jitter by Blender's PCG integer hash above rank 1 and its float hash at rank 1, octaves summing as fBM does with
        lerps in place of some sums.

        Args:
            vector: `Vector` coordinates, one row of three per point.
            w: `W` coordinate per point.

        Returns:
            Output arrays by output identifier.
        """
        rank, coordinates = self.rank, self.coordinates(vector, w) * np.float32(self.scale)
        randomness = np.float32(min(max(self.randomness, 0.0), 1.0))
        smooth = np.float32(min(max(self.smoothness / 2.0, 0.0), 0.5))
        detail, roughness, lacunarity = np.float32(self.detail), np.float32(self.roughness), np.float32(self.lacunarity)
        flat, remainder, columns = detail == 0.0 or roughness == 0.0, detail - np.floor(detail), np.arange(len(coordinates))
        half = np.float32(0.5) + np.float32(0.5) * randomness

        def pcg(cells: Cells) -> Floats:
            def rounds(axes: tuple[Cells, ...]) -> tuple[Cells, ...]:
                match axes:
                    case (x, y):
                        first = x + y * 1664525
                        return first, y + first * 1664525
                    case (x, y, z):
                        first = x + y * z
                        second = y + z * first
                        return first, second, z + first * second
                    case (x, y, z, w):
                        first = x + y * w
                        second = y + z * first
                        third = z + first * second
                        return first, second, third, w + second * third
                    case _:
                        return axes

            started = rounds(tuple(np.moveaxis(cells * 1664525 + 1013904223, -1, 0)))
            return np.stack(tuple((axis & 0x7FFFFFFF).astype(np.float32) * np.float32(2.0**-31) for axis in rounds(tuple(axis ^ (axis >> 16) for axis in started))), axis=-1)

        def jitter(cells: Cells) -> Floats:
            return _hash_float(cells[..., 0].astype(np.float32))[..., None] if rank == 1 else pcg(cells)

        def tint(cells: Cells) -> Floats:
            match rank:
                case 1:
                    return _hash_float3(cells.astype(np.float32))
                case 2:
                    return pcg(np.concatenate((cells, np.zeros_like(cells[..., :1])), axis=-1))
                case _:
                    return pcg(cells)[..., :3]

        def metric(delta: Floats, *, exact: bool) -> Floats:
            match self.distance:
                case _ if rank == 1:
                    return np.abs(delta[..., 0])
                case "EUCLIDEAN":
                    squared = np.sum(delta * delta, axis=-1)
                    return np.sqrt(squared) if exact else squared
                case "MANHATTAN":
                    return np.sum(np.abs(delta), axis=-1)
                case "CHEBYCHEV":
                    return np.max(np.abs(delta), axis=-1)
                case "MINKOWSKI":
                    exponent = np.float32(self.exponent)
                    return np.sum(np.abs(delta) ** exponent, axis=-1) ** (np.float32(1.0) / exponent)

        def scanned(at: Floats, reach: int) -> tuple[Floats, Cells, Cells, Floats]:
            floored = np.floor(at)
            cells, offsets = floored.astype(np.int32), _offsets_around(rank, reach)
            return at - floored, cells, offsets, offsets[:, None, :].astype(np.float32) + jitter(cells[None] + offsets[:, None]) * randomness

        def positioned(points: Floats) -> Floats:
            placed = np.zeros((len(points), 4), dtype=np.float32)
            placed[:, (3,) if rank == 1 else tuple(range(rank))] = points
            return placed

        def octave(at: Floats) -> tuple[Floats, Floats, Floats]:
            local, cells, offsets, points = scanned(at, 2 if self.feature == "SMOOTH_F1" and smooth != 0.0 else 1)
            match self.feature:
                case "SMOOTH_F1" if smooth != 0.0:
                    distances, tints = metric(points - local, exact=True), tint(cells[None] + offsets[:, None])
                    found, color, point = distances[0], tints[0], points[0]
                    for index in range(1, len(offsets)):
                        blend = _smoothstep(np.float32(0.5) + np.float32(0.5) * (found - distances[index]) / smooth)
                        correction = smooth * blend * (np.float32(1.0) - blend)
                        shift = (correction / (np.float32(1.0) + np.float32(3.0) * smooth))[:, None]
                        found = _lerp(found, distances[index], blend) - correction
                        color, point = _lerp(color, tints[index], blend[:, None]) - shift, _lerp(point, points[index], blend[:, None]) - shift
                    return found, color, positioned(cells.astype(np.float32) + point)
                case "F2":
                    selected = np.argsort(metric(points - local, exact=True), axis=0, kind="stable")[1]
                case _:
                    selected = np.argmin(metric(points - local, exact=False), axis=0)
            target = points[selected, columns]
            return metric(target - local, exact=True), tint(cells + offsets[selected]), positioned(target + cells.astype(np.float32))

        def edge(at: Floats) -> Floats:
            local, cells, _, vectors = scanned(at, 1)
            if rank == 1:
                middle, left, right = (np.float32(side) + jitter(cells + side)[:, 0] * randomness for side in (0, -1, 1))
                return np.minimum(np.abs((middle + left) / np.float32(2.0) - local[:, 0]), np.abs((middle + right) / np.float32(2.0) - local[:, 0]))
            toward = vectors - local
            closest = toward[np.argmin(np.sum(toward * toward, axis=-1), axis=0), columns]
            across = toward - closest
            length = np.sqrt(np.sum(across * across, axis=-1))
            edges = np.sum((closest + toward) / np.float32(2.0) * (across / np.where(length > 0.0, length, np.float32(1.0))[..., None]), axis=-1)
            return np.min(edges, axis=0, where=length * length > np.float32(0.0001), initial=np.finfo(np.float32).max)

        amplitude, scale = np.float32(1.0), np.float32(1.0)
        match self.feature:
            case "N_SPHERE_RADIUS":
                local, cells, offsets, points = scanned(coordinates, 1)
                nearest = np.argmin(np.sum(np.square(points - local), axis=-1), axis=0)
                closest, around = points[nearest, columns], offsets[np.any(offsets != 0, axis=1)][:, None, :] + offsets[nearest]
                neighbors = around.astype(np.float32) + jitter(cells[None] + around) * randomness
                other = neighbors[np.argmin(np.sum(np.square(closest - neighbors), axis=-1), axis=0), columns]
                return {"Radius": np.sqrt(np.sum(np.square(other - closest), axis=-1)) * np.float32(0.5)}
            case "DISTANCE_TO_EDGE":
                total, distance = np.asarray(half, dtype=np.float32), np.full(len(coordinates), np.float32(8.0), dtype=np.float32)
                for index in range(int(np.ceil(detail)) + 1):
                    found = edge(coordinates * scale)
                    if flat:
                        distance = found
                        break
                    if index <= detail:
                        total, distance = _lerp(total, np.asarray(half / scale), amplitude), _lerp(distance, np.minimum(distance, found / scale), amplitude)
                        scale, amplitude = scale * lacunarity, amplitude * roughness
                    elif remainder != 0.0:
                        total = _lerp(total, _lerp(total, np.asarray(half / scale), amplitude), remainder)
                        distance = _lerp(distance, np.minimum(distance, _lerp(distance, np.minimum(distance, found / scale), amplitude)), remainder)
                return {"Distance": distance / total if self.normalize else distance}
            case _:
                bound = np.full((1, rank), half, dtype=np.float32)
                limit = np.float32((half if rank == 1 else metric(bound, exact=True)[0]) * np.float32(2.0 if self.feature == "F2" else 1.0))
                total = np.zeros((), dtype=np.float32)
                distance, color, position = np.zeros(len(coordinates), dtype=np.float32), np.zeros((len(coordinates), 3), dtype=np.float32), np.zeros((len(coordinates), 4), dtype=np.float32)
                for index in range(int(np.ceil(detail)) + 1):
                    found, tinted, at = octave(coordinates * scale)
                    if flat:
                        total, distance, color, position = np.ones((), dtype=np.float32), found, tinted, at
                        break
                    if index <= detail:
                        total, distance, color = total + amplitude, distance + found * amplitude, color + tinted * amplitude
                        position, scale, amplitude = _lerp(position, at / scale, amplitude), scale * lacunarity, amplitude * roughness
                    elif remainder != 0.0:
                        total = _lerp(total, total + amplitude, remainder)
                        distance, color = _lerp(distance, distance + found * amplitude, remainder), _lerp(color, color + tinted * amplitude, remainder)
                        position = _lerp(position, _lerp(position, at / scale, amplitude), remainder)
                placed = position / np.float32(self.scale) if self.scale != 0.0 else np.zeros_like(position)
                return {
                    "Distance": distance / (total * limit) if self.normalize else distance,
                    "Color": color / total if self.normalize else color,
                    **({"Position": placed[:, :3]} if rank != 1 else {}),
                    **({"W": placed[:, 3]} if rank in {1, 4} else {}),
                }


class Wave(Texture, frozen=True, kw_only=True):
    """Wave Texture node: bands or rings of a periodic profile, distorted by fractal noise."""

    kind = bpy.types.ShaderNodeTexWave
    outputs = (("Fac", "FLOAT"), ("Color", "RGBA"))
    wave_type: Literal["BANDS", "RINGS"] = "BANDS"
    bands_direction: Literal["X", "Y", "Z", "DIAGONAL"] = "X"
    rings_direction: Literal["X", "Y", "Z", "SPHERICAL"] = "X"
    wave_profile: WaveProfile = "SIN"
    scale: Annotated[float, msgspec.Meta(ge=-1000.0, le=1000.0)] = 5.0
    distortion: Annotated[float, msgspec.Meta(ge=-1000.0, le=1000.0)] = 0.0
    detail: Annotated[float, msgspec.Meta(ge=0.0, le=15.0)] = 2.0
    detail_scale: Annotated[float, msgspec.Meta(ge=-1000.0, le=1000.0)] = 1.0
    detail_roughness: Unit = 0.5
    phase_offset: Annotated[float, msgspec.Meta(ge=-1000.0, le=1000.0)] = 0.0

    def evaluated(self, vector: npt.ArrayLike, w: npt.ArrayLike = 0.0) -> dict[str, Floats]:
        """Evaluates `Fac` and RGB `Color` as the node's CPU function does, the distortion an fBM `Noise` of the detail inputs."""
        point = (self.coordinates(vector, w) * np.float32(self.scale) + np.float32(0.000001)) * np.float32(0.999999)
        x, y, z = point.T
        match self.wave_type:
            case "BANDS":
                match self.bands_direction:
                    case "X":
                        phase = x * np.float32(20.0)
                    case "Y":
                        phase = y * np.float32(20.0)
                    case "Z":
                        phase = z * np.float32(20.0)
                    case "DIAGONAL":
                        phase = (x + y + z) * np.float32(10.0)
            case "RINGS":
                match self.rings_direction:
                    case "X":
                        phase = np.sqrt(y * y + z * z) * np.float32(20.0)
                    case "Y":
                        phase = np.sqrt(x * x + z * z) * np.float32(20.0)
                    case "Z":
                        phase = np.sqrt(x * x + y * y) * np.float32(20.0)
                    case "SPHERICAL":
                        phase = np.sqrt(x * x + y * y + z * z) * np.float32(20.0)
        detailed = Noise(detail=self.detail, roughness=self.detail_roughness)
        distorted = np.float32(self.distortion) * (_fractal(detailed, point * np.float32(self.detail_scale)) * np.float32(2.0) - np.float32(1.0)) if self.distortion != 0.0 else np.float32(0.0)
        value = periodic(self.wave_profile, (phase + np.float32(self.phase_offset) + distorted) / np.float32(_TAU), np)
        return {"Fac": value, "Color": np.repeat(value[:, None], 3, axis=1)}


class Magic(Texture, frozen=True, kw_only=True):
    """Magic Texture node: nested trigonometric turbulence."""

    kind = bpy.types.ShaderNodeTexMagic
    outputs = (("Fac", "FLOAT"), ("Color", "RGBA"))
    turbulence_depth: Annotated[int, msgspec.Meta(ge=0, le=10)] = 2
    scale: Annotated[float, msgspec.Meta(ge=-1000.0, le=1000.0)] = 5.0
    distortion: Annotated[float, msgspec.Meta(ge=-1000.0, le=1000.0)] = 1.0

    def evaluated(self, vector: npt.ArrayLike, w: npt.ArrayLike = 0.0) -> dict[str, Floats]:
        """Evaluates `Fac` and RGB `Color` as the node's CPU function does, one trigonometric step per depth."""
        a, b, c = (self.coordinates(vector, w) * np.float32(self.scale)).T
        distort = np.float32(self.distortion)
        steps: tuple[Callable[[Floats, Floats, Floats], tuple[Floats, Floats, Floats]], ...] = (
            lambda x, y, z: (x * distort, -np.cos(x * distort - y * distort + z * distort) * distort, z * distort),
            lambda x, y, z: (np.cos(x - y - z) * distort, y, z),
            lambda x, y, z: (x, y, np.sin(-x - y - z) * distort),
            lambda x, y, z: (-np.cos(-x + y - z) * distort, y, z),
            lambda x, y, z: (x, -np.sin(-x + y + z) * distort, z),
            lambda x, y, z: (x, -np.cos(-x + y + z) * distort, z),
            lambda x, y, z: (np.cos(x + y + z) * distort, y, z),
            lambda x, y, z: (x, y, np.sin(x + y - z) * distort),
            lambda x, y, z: (-np.cos(-x - y + z) * distort, y, z),
            lambda x, y, z: (x, -np.sin(x - y + z) * distort, z),
        )
        turbulence = functools.reduce(
            lambda held, step: step(*held), steps[: self.turbulence_depth], (np.sin((a + b + c) * np.float32(5.0)), np.cos((-a + b - c) * np.float32(5.0)), -np.cos((-a - b + c) * np.float32(5.0)))
        )
        stacked: Floats = np.stack(turbulence, axis=-1)
        color = np.float32(0.5) - (stacked / (distort * np.float32(2.0)) if distort != 0.0 else stacked)
        return {"Fac": np.sum(color, axis=-1) * np.float32(1.0 / 3.0), "Color": color}


class Gabor(Texture, frozen=True, kw_only=True):
    """Gabor Texture node: sparse windowed oscillations of one frequency and orientation."""

    kind = bpy.types.ShaderNodeTexGabor
    outputs = (("Value", "FLOAT"), ("Phase", "FLOAT"), ("Intensity", "FLOAT"))
    gabor_type: Literal["2D", "3D"] = "2D"
    scale: Finite = 5.0
    frequency: NonNegative = 2.0
    anisotropy: Unit = 1.0
    orientation_2d: Finite = math.pi / 4.0
    orientation_3d: tuple[Finite, Finite, Finite] = (math.sqrt(2.0), math.sqrt(2.0), 0.0)

    @property
    @override
    def rank(self) -> int:
        """Axes of `gabor_type`."""
        return _rank(self.gabor_type)

    @property
    @override
    def unfed(self) -> frozenset[object]:
        """The orientation socket of the other type."""
        return frozenset((Gabor.orientation_3d,) if self.gabor_type == "2D" else (Gabor.orientation_2d,))

    def evaluated(self, vector: npt.ArrayLike, w: npt.ArrayLike = 0.0) -> dict[str, Floats]:
        """Evaluates `Value`, `Phase`, and `Intensity` as the node's CPU function does, eight float-hashed impulses per cell."""
        coordinates = self.coordinates(vector, w) * np.float32(self.scale)
        isotropy = np.float32(1.0) - np.float32(min(max(self.anisotropy, 0.0), 1.0))
        frequency = np.float32(max(0.001, self.frequency))
        direction = np.asarray(self.orientation_3d, dtype=np.float32)
        base = direction / np.float32(np.linalg.norm(direction)) if self.rank == 3 else direction
        floored = np.floor(coordinates)
        local = coordinates - floored

        def pair(seeds: Floats) -> Floats:
            x, y, z, *rest = np.moveaxis(seeds, -1, 0)
            return np.stack((_hash_float(x, y, z, *rest), _hash_float(z, x, *rest, y) if rest else _hash_float(z, x, y)), axis=-1)

        def oriented(seeds: Floats) -> Floats:
            match self.rank:
                case 2:
                    turn = np.float32(self.orientation_2d) + (_hash_float(*seeds.T) - np.float32(0.5)) * np.float32(np.pi) * isotropy
                    return np.stack((np.cos(turn), np.sin(turn)), axis=-1)
                case _ if isotropy == 0.0:
                    return np.broadcast_to(base, (len(seeds), 3))
                case _:
                    angles = pair(seeds) * np.float32(np.pi) * isotropy
                    inclination = np.arccos(base[2]) + angles[:, 0]
                    azimuth = np.sign(base[1]) * np.arccos(base[0] / np.sqrt(base[0] * base[0] + base[1] * base[1])) + angles[:, 1]
                    return np.stack((np.sin(inclination) * np.cos(azimuth), np.sin(inclination) * np.sin(azimuth), np.cos(inclination)), axis=-1)

        def impulse(offset: Floats, index: int) -> Floats:
            cell = floored + offset
            orientation, center, weight = (np.concatenate((cell, np.full((len(cell), 1), np.float32(index * 3 + part))), axis=1) for part in range(3))
            inside = local - offset - (pair(center) if self.rank == 2 else _hash_float3(center))
            squared = np.sum(inside * inside, axis=-1)
            envelope = np.exp(-np.float32(np.pi) * squared) * (np.float32(0.5) + np.float32(0.5) * np.cos(np.float32(np.pi) * squared))
            angle = np.float32(_TAU) * np.sum(inside * (frequency * oriented(orientation)), axis=-1)
            signed = np.where(_hash_float(*weight.T) < np.float32(0.5), np.float32(-1.0), np.float32(1.0))
            return np.asarray(np.where(squared < np.float32(1.0), signed * envelope, np.float32(0.0))[:, None] * np.stack((np.cos(angle), np.sin(angle)), axis=-1), dtype=np.float32)

        phasor = sum((impulse(offset, index) for offset in _offsets_around(self.rank, 1).astype(np.float32) for index in range(8)), np.zeros((len(coordinates), 2), dtype=np.float32))
        normalization = np.float32(6.0) * np.sqrt(np.float32(8.0 * 0.5) * np.float32(0.25 if self.rank == 2 else 1.0 / (4.0 * np.sqrt(2.0))))
        return {
            "Value": phasor[:, 1] / normalization * np.float32(0.5) + np.float32(0.5),
            "Phase": (np.arctan2(phasor[:, 1], phasor[:, 0]) + np.float32(np.pi)) / np.float32(_TAU),
            "Intensity": np.sqrt(np.sum(phasor * phasor, axis=-1)) / normalization,
        }


class White(Texture, frozen=True, kw_only=True):
    """White Noise Texture node: a uniform value and color hashed from the coordinates themselves."""

    kind = bpy.types.ShaderNodeTexWhiteNoise
    outputs = (("Value", "FLOAT"), ("Color", "RGBA"))
    noise_dimensions: Dimensions = "3D"

    @property
    @override
    def rank(self) -> int:
        """Axes of `noise_dimensions`."""
        return _rank(self.noise_dimensions)

    def evaluated(self, vector: npt.ArrayLike, w: npt.ArrayLike = 0.0) -> dict[str, Floats]:
        """Evaluates `Value` and RGB `Color` as the node's CPU function does."""
        coordinates = self.coordinates(vector, w)
        return {"Value": _hash_float(*coordinates.T), "Color": _hash_float3(coordinates)}


class MapRange(NodeSettings, frozen=True, kw_only=True):
    """Map Range node over floats."""

    kind = bpy.types.ShaderNodeMapRange
    outputs = (("Result", "FLOAT"),)
    interpolation_type: Literal["LINEAR", "STEPPED", "SMOOTHSTEP", "SMOOTHERSTEP"] = "LINEAR"
    clamp: bool = True
    from_min: float = 0.0
    from_max: float = 1.0
    to_min: float = 0.0
    to_max: float = 1.0
    steps: float = 4.0

    @property
    @override
    def unfed(self) -> frozenset[object]:
        """`Steps` outside stepped interpolation."""
        return frozenset((MapRange.steps,) if self.interpolation_type != "STEPPED" else ())

    def node(self, value: Scalar, *, linked: Option[Linked] = Nothing) -> Expr:
        """Builds the Map Range node over `Value`, read at `Result`."""
        return self.lowered((("Value", value),), Nothing, linked)

    def evaluated(self, values: npt.ArrayLike) -> Floats:
        """Remaps values as the node's CPU function does, clamping only linear and stepped interpolation."""
        held = np.asarray(values, dtype=np.float32)
        low, high, start, end, steps = (np.float32(bound) for bound in (self.from_min, self.from_max, self.to_min, self.to_max, self.steps))
        factor = (held - low) / (high - low) if high != low else np.zeros_like(held)
        match self.interpolation_type:
            case "STEPPED":
                shaped = np.floor(factor * (steps + np.float32(1.0))) / steps if steps != 0.0 else np.zeros_like(factor)
            case "SMOOTHSTEP":
                shaped = _smoothstep(factor)
            case "SMOOTHERSTEP":
                unit = np.clip(factor, np.float32(0.0), np.float32(1.0))
                shaped = unit * unit * unit * (unit * (unit * np.float32(6.0) - np.float32(15.0)) + np.float32(10.0))
            case "LINEAR":
                shaped = factor
        result = start + shaped * (end - start)
        return np.asarray(np.clip(result, min(start, end), max(start, end)) if self.clamp and self.interpolation_type in {"LINEAR", "STEPPED"} else result, dtype=np.float32)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PROFILES]


def periodic[V: Field, B](profile: WaveProfile, cycles: V, ns: Namespace[V, B]) -> V:
    """Returns the Wave Texture profile in [0, 1] at a cycle coordinate, one period per unit."""
    match profile:
        case "SIN":
            return 0.5 - ns.cos(cycles * _TAU) * 0.5
        case "SAW":
            return cycles - ns.floor(cycles)
        case "TRI":
            return ns.abs(cycles - ns.floor(cycles + 0.5)) * 2.0


def gaussian[V: Field, B](radius: V, angle: V, ns: Namespace[V, B]) -> tuple[V, V]:
    """Returns the cosine and sine Box-Muller normals of two distinct uniform draws, such as two White Noise values.

    Args:
        radius: Uniform draw setting the shared radius, kept off zero by float32 epsilon.
        angle: Uniform draw setting the shared phase.
        ns: Namespace evaluating the draws, `numpy` or the node namespace.

    Returns:
        Normals over one radius and phase.
    """
    spread = ns.sqrt(ns.log((radius + _EPSILON) / (1.0 + 2.0 * _EPSILON)) * -2.0)
    phase = angle * _TAU
    return spread * ns.cos(phase), spread * ns.sin(phase)


def stream(kind: type[object], *key: str | int) -> int:
    """Returns the draw family of a record kind and field key, exact in float32 so node and CPU hashes read one coordinate."""
    return int.from_bytes(content_hash(kind, msgspec.msgpack.encode(key))[:3], "little")


def uniforms(keys: npt.NDArray[np.int64], family: int, count: int) -> npt.NDArray[np.float64]:
    """Returns `count` stable uniforms per integer key row from the White Noise node's 4D hash, key columns folded one at a time ahead of the draw family and index."""
    white = White(noise_dimensions="4D")
    rows = len(keys)
    folded = functools.reduce(lambda held, column: white.evaluated(np.column_stack((held, column, np.full(rows, family))), 0.0)["Value"], keys.T.astype(np.float32), np.zeros(rows, dtype=np.float32))
    return np.column_stack([white.evaluated(np.column_stack((folded, np.full(rows, family), np.full(rows, index))), 0.0)["Value"] for index in range(count)]).astype(np.float64)


# --- [HASHES]


def _hash_uint(*keys: npt.NDArray[np.uint32]) -> npt.NDArray[np.uint32]:
    start = np.full(np.broadcast_shapes(*(key.shape for key in keys)), 0xDEADBEEF + (len(keys) << 2) + 13, dtype=np.uint32)
    state = (*(start + key for key in keys[:3]), *(start for _ in range(3 - len(keys[:3]))))

    def rotated(value: npt.NDArray[np.uint32], bits: int) -> npt.NDArray[np.uint32]:
        return (value << np.uint32(bits)) | (value >> np.uint32(32 - bits))

    def replaced(held: tuple[npt.NDArray[np.uint32], ...], index: int, value: npt.NDArray[np.uint32]) -> tuple[npt.NDArray[np.uint32], ...]:
        return (*held[:index], value, *held[index + 1 :])

    def mixed(held: tuple[npt.NDArray[np.uint32], ...], step: tuple[int, int, int, int]) -> tuple[npt.NDArray[np.uint32], ...]:
        target, source, bits, added = step
        moved = replaced(held, target, (held[target] - held[source]) ^ rotated(held[source], bits))
        return replaced(moved, source, moved[source] + moved[added])

    def finalized(held: tuple[npt.NDArray[np.uint32], ...], step: tuple[int, int, int]) -> tuple[npt.NDArray[np.uint32], ...]:
        target, source, bits = step
        return replaced(held, target, (held[target] ^ held[source]) - rotated(held[source], bits))

    match keys:
        case (_, _, _, w):
            first, *rest = functools.reduce(mixed, ((0, 2, 4, 1), (1, 0, 6, 2), (2, 1, 8, 0), (0, 2, 16, 1), (1, 0, 19, 2), (2, 1, 4, 0)), state)
            entered = (first + w, *rest)
        case _:
            entered = state
    return functools.reduce(finalized, ((2, 1, 14), (0, 2, 11), (1, 0, 25), (2, 1, 16), (0, 2, 4), (1, 0, 14), (2, 1, 24)), entered)[2]


def _hash_float(*keys: npt.ArrayLike) -> Floats:
    return _hash_uint(*(np.ascontiguousarray(key, dtype=np.float32).view(np.uint32) for key in keys)).astype(np.float32) / np.float32(0xFFFFFFFF)


def _hash_float3(keys: Floats) -> Floats:
    match tuple(np.moveaxis(keys, -1, 0)):
        case (x, y, z, w):
            return np.stack((_hash_float(x, y, z, w), _hash_float(z, x, w, y), _hash_float(w, z, y, x)), axis=-1)
        case axes:
            return np.stack(tuple(_hash_float(*axes, *((np.float32(seed),) if seed else ())) for seed in range(3)), axis=-1)


# --- [PERLIN]


def _perlin(position: Floats) -> Floats:
    rank = position.shape[1]
    corrected = np.fmod(position, np.float32(100000.0)) + np.float32(0.5) * (np.abs(position) >= np.float32(1000000.0))
    floored = np.floor(corrected)
    local, cells = corrected - floored, floored.astype(np.int32)
    corners = np.array(tuple(corner[::-1] for corner in itertools.product((0, 1), repeat=rank)), dtype=np.int32)
    hashed = _hash_uint(*np.moveaxis((cells[None] + corners[:, None, :]).astype(np.uint32), -1, 0))

    def signed(value: Floats, bit: int) -> Floats:
        return np.where((hashed & np.uint32(bit)) != 0, -value, value)

    def below(mask: int, bound: int) -> npt.NDArray[np.bool]:
        return (hashed & np.uint32(mask)) < np.uint32(bound)

    axes: tuple[Floats, ...] = tuple(np.moveaxis(local[None] - corners[:, None, :].astype(np.float32), -1, 0))
    match axes:
        case (x,):
            gradients = np.multiply(signed((np.uint32(1) + (hashed & np.uint32(7))).astype(np.float32), 8), x)
        case (x, y):
            gradients = np.add(signed(np.where(below(7, 4), x, y), 1), signed(np.float32(2.0) * np.where(below(7, 4), y, x), 2))
        case (x, y, z):
            ends = np.isin(hashed & np.uint32(15), (12, 14))
            gradients = np.add(signed(np.where(below(15, 8), x, y), 1), signed(np.where(below(15, 4), y, np.where(ends, x, z)), 2))
        case (x, y, z, w):
            gradients = np.add(np.add(signed(np.where(below(31, 24), x, y), 1), signed(np.where(below(31, 16), y, z), 2)), signed(np.where(below(31, 8), z, w), 4))
        case _:
            raise ValueError(f"Perlin gradients span ranks 1 to 4, not {rank}")
    fades = local * local * local * (local * (local * np.float32(6.0) - np.float32(15.0)) + np.float32(10.0))
    blended = functools.reduce(
        lambda held, axis: held[..., 0, :] * (np.float32(1.0) - fades[:, axis]) + held[..., 1, :] * fades[:, axis], range(rank), gradients.reshape((2,) * rank + (len(position),))
    )
    return blended * _PERLIN_SCALES[rank - 1]


def _fractal(noise: Noise, position: Floats) -> Floats:
    detail = np.float32(min(max(noise.detail, 0.0), 15.0))
    roughness, lacunarity, offset, gain = np.float32(max(noise.roughness, 0.0)), np.float32(noise.lacunarity), np.float32(noise.offset), np.float32(noise.gain)
    octaves, remainder = int(detail), detail - np.floor(detail)
    match noise.noise_type:
        case "FBM":
            scale, amplitude, total = np.float32(1.0), np.float32(1.0), np.float32(0.0)
            value = np.zeros(len(position), dtype=np.float32)
            for _ in range(octaves + 1):
                value, total = value + _perlin(scale * position) * amplitude, total + amplitude
                amplitude, scale = amplitude * roughness, scale * lacunarity
            if remainder == 0.0:
                return np.float32(0.5) * value / total + np.float32(0.5) if noise.normalize else value
            extended = value + _perlin(scale * position) * amplitude
            return (
                _lerp(np.float32(0.5) * value / total + np.float32(0.5), np.float32(0.5) * extended / (total + amplitude) + np.float32(0.5), remainder)
                if noise.normalize
                else _lerp(value, extended, remainder)
            )
        case "MULTIFRACTAL":
            value, power, at = np.ones(len(position), dtype=np.float32), np.float32(1.0), position
            for _ in range(octaves + 1):
                value, power, at = value * (power * _perlin(at) + np.float32(1.0)), power * roughness, at * lacunarity
            return value * (remainder * power * _perlin(at) + np.float32(1.0)) if remainder != 0.0 else value
        case "HETERO_TERRAIN":
            power, at = roughness, position * lacunarity
            value = offset + _perlin(position)
            for _ in range(1, octaves + 1):
                value, power, at = value + (_perlin(at) + offset) * power * value, power * roughness, at * lacunarity
            return value + remainder * ((_perlin(at) + offset) * power * value) if remainder != 0.0 else value
        case "HYBRID_MULTIFRACTAL":
            power, at = np.float32(1.0), position
            value, weight = np.zeros(len(position), dtype=np.float32), np.ones(len(position), dtype=np.float32)
            for _ in range(octaves + 1):
                alive, held, signal = weight > np.float32(0.001), np.minimum(weight, np.float32(1.0)), (_perlin(at) + offset) * power
                value, weight = np.where(alive, value + held * signal, value), np.where(alive, held * gain * signal, weight)
                power, at = power * roughness, at * lacunarity
            last = value + remainder * np.minimum(weight, np.float32(1.0)) * ((_perlin(at) + offset) * power)
            return np.where(weight > np.float32(0.001), last, value) if remainder != 0.0 else value
        case "RIDGED_MULTIFRACTAL":
            power, at = roughness, position
            signal = np.square(offset - np.abs(_perlin(position)))
            value = signal
            for _ in range(1, octaves + 1):
                at, weight = at * lacunarity, np.clip(signal * gain, np.float32(0.0), np.float32(1.0))
                signal = np.square(offset - np.abs(_perlin(at))) * weight
                value, power = value + signal * power, power * roughness
            return value


# --- [GEOMETRY]


def _offsets_around(rank: int, reach: int) -> Cells:
    return np.array(tuple(offset[::-1] for offset in itertools.product(range(-reach, reach + 1), repeat=rank)), dtype=np.int32)


def _smoothstep(values: Floats) -> Floats:
    held = np.clip(values, np.float32(0.0), np.float32(1.0))
    return (np.float32(3.0) - np.float32(2.0) * held) * (held * held)


def _lerp(start: Floats, end: Floats, amount: Floats | np.float32) -> Floats:
    return np.asarray((np.float32(1.0) - amount) * start + amount * end, dtype=np.float32)


def _rank(dimensions: str) -> int:
    return typing.get_args(Dimensions.__value__).index(dimensions) + 1


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Dimensions", "Gabor", "Linked", "Magic", "MapRange", "NodeSettings", "Noise", "Texture", "Voronoi", "Wave", "WaveProfile", "White", "gaussian", "periodic", "stream", "uniforms"]
