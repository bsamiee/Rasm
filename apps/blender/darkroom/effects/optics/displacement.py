"""Displacement fields composed into one warp, integrated over each pixel: twirl, waves, turbulence, sphere, luminance, bubbles, image maps, glass, and raindrops."""

from collections.abc import Mapping
from enum import auto, Enum
import functools
from math import cos, pi, sin, tau
from typing import Annotated, Literal

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO
from scipy.ndimage import gaussian_filter, map_coordinates, sobel

from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Key
from blender.bpy.nodes import textures
from blender.bpy.nodes.graphs import Expr
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.render.compositor.coordinates import Border, BORDERS, Clock
from blender.bpy.render.compositor.effects import Alpha, Animates, CameraFrame, CompositorEffect, precise, Requests, Shell
from blender.imaging.color.processors import converted, GREY, luma
from blender.imaging.files import buffers
from blender.imaging.files.images import ImageFile, read_file
from blender.imaging.geometry.stmaps import displaced, slope, softened, tapped
from blender.imaging.sampling.integration import Estimate, Precision, Raster, smoothstep
from blender.imaging.sampling.populations import Population
from blender.records.faults import accumulated, Fault, Outcome, paired, RecordReason
from blender.records.fields import Finite, NonNegative, Positive, Seed, Unit

from ...chain.stages import Effect, prepared, Stage
from ..grade.exposure import LATER, later
from .procedural import ANGLE, bases, Basis, clock, decoded, evaluated, FACTOR, Perlin, PLANAR, profiled, WaveProfile

# --- [TYPES] ----------------------------------------------------------------------------


class Front(Enum):
    """Traveling bands or circular waves with radial and tangential motion."""

    LINEAR = auto()
    RADIAL = auto()
    ZIGZAG = auto()
    POND = auto()


class MapReading(Enum):
    """Source image reading as a coordinate field."""

    GRADIENT = auto()
    LUMA = auto()
    OFFSETS = auto()
    COORDINATES = auto()


class Surface(Enum):
    """Periodic glass heights and a stochastic frosted height."""

    BLOCKS = auto()
    CANVAS = auto()
    FROSTED = auto()
    TINY_LENS = auto()


# --- [MODELS] ---------------------------------------------------------------------------

# --- [FIELDS]


class Twirl(msgspec.Struct, frozen=True, kw_only=True):
    """Turn about a center with a smooth falloff to a radius."""

    center: tuple[Finite, Finite] = (0.5, 0.5)
    radius: Positive = 0.5
    angle: Finite = 6.1
    falloff: Positive = 1.0
    arms: Annotated[int, msgspec.Meta(ge=0)] = 0
    arm_offset: Finite = 0.0

    def displaced(self, points: npt.NDArray[np.float64], extent: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Offsets turning points about the center, the turn falling smoothly to zero at the radius."""
        arm: npt.NDArray[np.float64] = points - np.array(self.center) * extent
        weight = smoothstep(np.clip(1.0 - (np.linalg.norm(arm, axis=-1) / self.radius) ** (1.0 / self.falloff), 0.0, 1.0)) * (
            0.5 + 0.5 * np.cos(self.arms * (np.arctan2(arm[..., 1], arm[..., 0]) - self.arm_offset))
        )
        turn = -self.angle * weight
        return arm - np.stack((arm[..., 0] * np.cos(turn) - arm[..., 1] * np.sin(turn), arm[..., 0] * np.sin(turn) + arm[..., 1] * np.cos(turn)), axis=-1)


class Wave(msgspec.Struct, frozen=True, kw_only=True):
    """Ripple across traveling periodic wave fronts."""

    amplitude: NonNegative = 0.02
    wavelength: Positive = tau / (20.0 * 2.0**0.5)
    direction: Finite = 0.0
    phase: Finite = 0.0
    clock: Clock = Clock(speed=0.5 * 10.0 / tau / 24.0)
    profile: WaveProfile = "SIN"
    front: Front = Front.LINEAR
    count: Annotated[int, msgspec.Meta(ge=1)] = 1
    wavelength_variation: Annotated[float, msgspec.Meta(ge=0.0, lt=1.0)] = 0.0
    amplitude_variation: Unit = 0.0
    spread: Finite = 0.0
    axes: tuple[Finite, Finite] = (1.0, 1.0)
    center: tuple[Finite, Finite] = (0.5, 0.5)
    radius: Positive = 0.5
    seed: Seed = 0

    def displaced(self, points: npt.NDArray[np.float64], extent: npt.NDArray[np.float64], frame: float) -> npt.NDArray[np.float64]:
        """Offsets of every superposed crest along its front's displacement axis."""
        draws = textures.uniforms(np.arange(self.count, dtype=np.int64)[:, None], textures.stream(Wave, self.seed), 3)

        def crest(index: int) -> npt.NDArray[np.float64]:
            wavelength = self.wavelength * (1.0 - self.wavelength_variation * float(draws[index, 0]))
            amplitude = self.amplitude * (1.0 - self.amplitude_variation * float(draws[index, 1])) / self.count
            heading = self.direction + self.spread * (index - (self.count - 1.0) / 2.0) / max(self.count - 1, 1)
            along = np.array((cos(heading), sin(heading)))
            if self.front is Front.LINEAR:
                travel, axis, envelope = (points - extent / 2.0) @ along, np.broadcast_to((-along[1], along[0]), points.shape), np.ones(points.shape[:-1])
            else:
                arm = points - np.array(self.center) * extent
                travel = np.linalg.norm(arm, axis=-1)
                spoke = np.divide(arm, travel[..., None], out=np.zeros_like(arm), where=travel[..., None] > 0.0)
                tangent = np.stack((-spoke[..., 1], spoke[..., 0]), axis=-1)
                axis = {Front.RADIAL: spoke, Front.ZIGZAG: tangent, Front.POND: (spoke + tangent) * 0.5**0.5}[self.front]
                envelope = np.maximum(0.0, 1.0 - travel / self.radius)
            cycles = travel / wavelength - self.clock.clock(frame) + self.phase / tau + (float(draws[index, 2]) if self.count > 1 else 0.0)
            return np.asarray(amplitude * (2.0 * profiled(self.profile, cycles, np) - 1.0)[..., None] * envelope[..., None] * axis * self.axes, dtype=np.float64)

        return sum((crest(index) for index in range(self.count)), np.zeros_like(points))


class Turbulence(msgspec.Struct, frozen=True, kw_only=True):
    """Movement along evolving procedural noise."""

    amplitude: NonNegative = 0.1
    basis: Basis = Perlin(texture=textures.Noise(noise_dimensions="4D", scale=2.0))
    axes: tuple[Finite, Finite] = (1.0, 1.0)
    clock: Clock = Clock(speed=0.5 / 24.0)
    seed: Seed = 0

    def displaced(self, points: npt.NDArray[np.float64], frame: float) -> npt.NDArray[np.float64]:
        """Offsets of one noise per axis at the held clock."""
        flat = points.reshape(-1, 2)
        moment = np.full(len(flat), self.clock.clock(frame))
        fields = np.stack([evaluated(self.basis, flat, moment, seed=self.seed, stream=textures.stream(Turbulence, axis))[:, 0] for axis in range(2)], axis=-1)
        return self.amplitude * (2.0 * fields.reshape(points.shape) - 1.0) * self.axes


class Sphere(msgspec.Struct, frozen=True, kw_only=True):
    """Bulge or pinch inside a circle with a hemisphere mapping."""

    center: tuple[Finite, Finite] = (0.5, 0.5)
    radius: Positive = 0.15
    amount: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0)] = 0.1
    axes: tuple[Unit, Unit] = (1.0, 1.0)

    def displaced(self, points: npt.NDArray[np.float64], extent: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Offsets moving each point inside the circle along the hemisphere mapping."""
        arm = (points - np.array(self.center) * extent) * self.axes
        radial = np.linalg.norm(arm, axis=-1) / self.radius
        bounded = np.minimum(radial, 1.0)
        target = 2.0 * np.arcsin(bounded) / pi if self.amount >= 0.0 else np.sin(pi * bounded / 2.0)
        change = np.divide(target - bounded, bounded, out=np.zeros_like(bounded), where=(bounded > 0.0) & (radial < 1.0))
        return -abs(self.amount) * change[..., None] * arm


class Luma(msgspec.Struct, frozen=True, kw_only=True):
    """Moves bright and dark content in opposite directions."""

    amplitude: NonNegative = 0.02
    direction: Finite = 0.0
    softness: NonNegative = 0.015


class Bubbles(msgspec.Struct, frozen=True, kw_only=True):
    """Refracts through frame luminance height."""

    amplitude: NonNegative = 0.02
    softness: NonNegative = 0.015


class Raindrops(msgspec.Struct, frozen=True, kw_only=True):
    """Drops showing an inverted, blurred view around their centers."""

    drops: Population = Population(spacing=1.0 / (2.0 * 3.8), size=0.5, spread=0.0, softness=0.05)
    refraction: NonNegative = 2.0
    blur: NonNegative = 14.6 / 1920.0
    direction: Finite = 0.0
    clock: Clock = Clock(speed=0.01 / (24.0 * drops.spacing))
    trails: Unit = 0.0
    seed: Seed = 0
    strength: Finite = 0.0
    softness: NonNegative = 0.015

    def sampled(self, points: npt.NDArray[np.float64], frame: float) -> npt.NDArray[np.float64]:
        """Returns inversion offsets, drop coverage, and receding trail height at `(n, 2)` points."""
        sine, cosine = sin(self.direction), cos(self.direction)
        turn = np.array(((cosine, sine), (-sine, cosine)))
        spacing = self.drops.spacing
        local = points @ turn + (0.0, self.clock.clock(frame) * spacing)
        cells, ownership = np.unique(np.floor(local / spacing).astype(np.int64), axis=0, return_inverse=True)
        draws = textures.uniforms(cells, textures.stream(Raindrops, self.seed), 6)
        centers, radii, gains = (values[ownership] for values in self.drops.placed(cells, draws, spacing))
        present = (draws[:, 0] < self.drops.density)[ownership]
        arm = local - centers

        def edged(distance: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            return smoothstep((distance < 1.0).astype(np.float64) if self.drops.softness == 0.0 else np.clip((1.0 - distance) / self.drops.softness, 0.0, 1.0))

        drop = edged(np.linalg.norm(arm, axis=-1) / radii)
        lag = self.trails * np.where(arm[:, 1] <= 0.0, np.maximum(0.0, 1.0 + arm[:, 1] / (centers[:, 1] - cells[ownership, 1] * spacing)), 0.0)
        cross = np.abs(arm[:, 0]) / (0.5 * radii)
        return (
            np.column_stack((
                (arm @ turn.T) * ((1.0 + self.refraction) * (drop > 0.0))[:, None],
                np.maximum(drop, lag * edged(cross)) * gains,
                lag * (1.0 - drop) * np.sqrt(np.maximum(0.0, 1.0 - cross * cross)),
            ))
            * present[:, None]
        )


class Map(msgspec.Struct, frozen=True, kw_only=True):
    """Image read as luminance, slope, offsets, or absolute source coordinates, the input image without a file path."""

    source: str | None = None
    reading: MapReading = MapReading.OFFSETS
    axes: tuple[Finite, Finite] = (0.02, 0.02)
    tile: tuple[Positive, Positive] | None = None
    rotation: Finite = 0.0
    softness: NonNegative = 0.015

    @property
    def data(self) -> bool:
        """Whether the image holds data channels read untransformed."""
        return self.reading in {MapReading.OFFSETS, MapReading.COORDINATES}


class Refraction(msgspec.Struct, frozen=True, kw_only=True):
    """Refraction and blur through a periodic or frosted glass height."""

    surface: Surface = Surface.FROSTED
    size: Positive = 0.1
    strength: Finite = 0.02
    softness: NonNegative = 0.015
    blur: NonNegative = 0.0
    seed: Seed = 0

    def height(self, centers: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Glass height at `(rows, columns, 2)` pixel centers."""
        cell = centers / self.size
        fraction = cell % 1.0
        match self.surface:
            case Surface.BLOCKS:
                return np.prod(1.0 - np.abs(2.0 * fraction - 1.0) ** 4, axis=-1)
            case Surface.CANVAS:
                return 0.5 + 0.5 * np.prod(np.sin(tau * cell), axis=-1)
            case Surface.TINY_LENS:
                return np.sqrt(np.maximum(0.0, 1.0 - 4.0 * np.sum((fraction - 0.5) ** 2, axis=-1)))
            case Surface.FROSTED:
                frosted = Perlin(texture=textures.Noise(noise_dimensions="2D", scale=1.0 / self.size))
                flat = centers.reshape(-1, 2)
                return evaluated(frosted, flat, np.zeros(len(flat)), seed=self.seed, stream=textures.stream(Refraction))[:, 0].reshape(centers.shape[:-1])


class Displacement(Effect, frozen=True, kw_only=True):
    """Composes geometric, image-driven, glass, and falling-drop displacement fields, decoded from the entry inputs for the CPU render, the image it prepares in `staged`."""

    stage = Stage.WARP
    pointwise = False
    twirl: Twirl | None = None
    wave: Wave | None = None
    turbulence: Turbulence | None = None
    sphere: Sphere | None = None
    luma: Luma | None = None
    bubbles: Bubbles | None = None
    raindrops: Raindrops | None = None
    map: Map | None = None
    refraction: Refraction | None = None
    extension_x: Border = Border.CLAMP
    extension_y: Border = Border.CLAMP
    filtername: str = "triangle"
    filterwidth: Positive = 2.0
    staged: IdName | None = None

    @property
    def active(self) -> bool:
        """Whether any field displaces."""
        return any(field is not None for field in (self.twirl, self.wave, self.turbulence, self.sphere, self.luma, self.bubbles, self.raindrops, self.map, self.refraction))

    def applied(
        self,
        image: OpenImageIO.ImageBuf,
        raster: Raster,
        extent: npt.NDArray[np.float64],
        *,
        frame: float,
        luminance: npt.NDArray[np.float64],
        gain: float,
        mapping: Option[npt.NDArray[np.float32]],
        precision: Precision,
    ) -> Outcome[Estimate[OpenImageIO.ImageBuf]]:
        """Integrates refracted color over each pixel.

        Args:
            image: Associated stage input, rows top first.
            raster: Data window in original camera units and its pixel grid.
            extent: Displayed camera frame width and height.
            frame: Scene frame the clocks hold.
            luminance: Scene-linear luminance coefficients.
            gain: Keyed gain of later exposure entries, scaling the luminance height's middle gray.
            mapping: Linear map image, the input image when absent.
            precision: Pixel integration and replicate error policy.

        Returns:
            Estimated image with replicate error, or image faults.
        """
        left, bottom, right, top = raster.bounds
        wrap = (self.extension_x.wrap, self.extension_y.wrap)

        def computed(source: npt.NDArray[np.float32]) -> Outcome[Estimate[OpenImageIO.ImageBuf]]:
            height, width, channels = source.shape
            centers = raster.centers

            def lifted(plane: npt.NDArray[np.float32]) -> npt.NDArray[np.float64]:
                positive = np.maximum(buffers.unassociated(plane)[..., :3] @ luminance, 0.0)
                return positive / (positive + GREY / gain)

            base = lifted(source)
            luma_height = softened(base, raster, self.luma.softness) if self.luma is not None else base
            bubble_height = softened(base, raster, self.bubbles.softness) if self.bubbles is not None else base
            rain = self.raindrops
            rain_gradient = (
                rain.strength * rain.drops.spacing * slope(softened(rain.sampled(centers.reshape(-1, 2), frame)[:, 3].reshape(height, width), raster, rain.softness), raster)
                if rain is not None and rain.strength != 0.0
                else np.zeros((height, width, 2))
            )
            glass = self.refraction
            glass_gradient = glass.strength * glass.size * slope(softened(glass.height(centers), raster, glass.softness), raster) if glass is not None else np.zeros((height, width, 2))
            request = Map() if self.map is None else self.map
            plane = mapping.default_value(source)
            map_origin, map_span = ((0.0, 0.0), tuple(extent)) if mapping.is_some() else ((left, bottom), (right - left, top - bottom))
            map_step = np.array(map_span if request.tile is None else request.tile) / plane.shape[1::-1]
            mode: Literal["wrap", "nearest"] = "nearest" if request.tile is None else "wrap"
            map_field = np.ones((*plane.shape[:2], 3), dtype=np.float64)
            match request.reading:
                case MapReading.GRADIENT:
                    level = gaussian_filter(lifted(plane), (request.softness / map_step[1], request.softness / map_step[0]), mode=mode)
                    map_field[..., :2] = np.stack((sobel(level, axis=1, mode=mode) / (8.0 * map_step[0]), -sobel(level, axis=0, mode=mode) / (8.0 * map_step[1])), axis=-1)
                case MapReading.LUMA:
                    map_field[..., :2] = (2.0 * gaussian_filter(lifted(plane), (request.softness / map_step[1], request.softness / map_step[0]), mode=mode) - 1.0)[..., None]
                case MapReading.OFFSETS:
                    map_field[..., :2] = 2.0 * plane[..., :2] - 1.0
                case MapReading.COORDINATES:
                    map_field[..., :2] = plane[..., :2]
                    map_field[..., 2] = plane[..., 2]
            glass_source = source if glass is None else softened(source.astype(np.float64), raster, glass.blur).astype(np.float32)
            drop_source = source if rain is None else softened(source.astype(np.float64), raster, rain.blur).astype(np.float32)

            def rotated(points: npt.NDArray[np.float64], angle: float) -> npt.NDArray[np.float64]:
                return points @ np.array(((cos(angle), sin(angle)), (-sin(angle), cos(angle))), dtype=np.float64)

            def composed(points: npt.NDArray[np.float64]) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
                coverage = np.ones(points.shape[:-1], dtype=np.float64)
                terms = (
                    *(() if self.twirl is None else (self.twirl.displaced(points, extent),)),
                    *(() if self.wave is None else (self.wave.displaced(points, extent, frame),)),
                    *(() if self.turbulence is None else (self.turbulence.displaced(points, frame),)),
                    *(() if self.sphere is None else (self.sphere.displaced(points, extent),)),
                    *(
                        (self.luma.amplitude * (2.0 * tapped(luma_height, raster, points) - 1.0)[..., None] * np.array((cos(self.luma.direction), sin(self.luma.direction))),)
                        if self.luma is not None
                        else ()
                    ),
                    *(
                        (
                            self.bubbles.amplitude
                            * np.stack([tapped(bubble_height, raster, points - axis) - tapped(bubble_height, raster, points + axis) for axis in np.eye(2) * self.bubbles.softness], axis=-1),
                        )
                        if self.bubbles is not None
                        else ()
                    ),
                    *((np.stack([tapped(glass_gradient[..., axis], raster, points) for axis in range(2)], axis=-1),) if glass is not None else ()),
                )
                displacement = sum(terms, np.zeros_like(points))
                if self.map is not None:
                    local = rotated(points, -request.rotation)
                    fraction = (local - map_origin) / map_span if request.tile is None else local / request.tile % 1.0
                    at = np.stack(((1.0 - fraction[..., 1]) * map_field.shape[0] - 0.5, fraction[..., 0] * map_field.shape[1] - 0.5))
                    field = np.stack([map_coordinates(map_field[..., axis], at, order=1, mode="nearest" if request.tile is None else "grid-wrap", prefilter=False) for axis in range(3)], axis=-1)
                    if request.reading is MapReading.COORDINATES:
                        return displacement + points - rotated(field[..., :2] * extent, request.rotation), field[..., 2]
                    return displacement + rotated(field[..., :2] * request.axes * extent, request.rotation), coverage
                return displacement, coverage

            def dropped(points: npt.NDArray[np.float64]) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
                if rain is None:
                    return np.zeros(points.shape[:-1]), np.zeros_like(points)
                field = rain.sampled(points.reshape(-1, 2), frame)
                return field[:, 2].reshape(points.shape[:-1]), field[:, :2].reshape(points.shape) + np.stack([tapped(rain_gradient[..., axis], raster, points) for axis in range(2)], axis=-1)

            def observed(points: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float64]]:
                displacement, coverage = composed(points)
                drops, refraction = dropped(points)
                return paired(
                    displaced(glass_source, raster, points - displacement, coverage, wrap, filtername=self.filtername, filterwidth=self.filterwidth),
                    displaced(drop_source, raster, points - displacement - refraction, coverage, wrap, filtername=self.filtername, filterwidth=self.filterwidth),
                ).map(lambda colors: colors[0] * (1.0 - drops[..., None]) + colors[1] * drops[..., None])

            def replicated(replicate: int) -> Outcome[npt.NDArray[np.float64]]:
                net = precision.points(2, replicate)
                return functools.reduce(
                    lambda total, offset: total.bind(lambda held: observed(centers + (offset - 0.5) * raster.step * (1.0, -1.0)).map(lambda term: held + term / len(net))),
                    net,
                    Ok(np.zeros((height, width, channels), dtype=np.float64)),
                )

            def published(observations: tuple[npt.NDArray[np.float64], ...]) -> Outcome[Estimate[OpenImageIO.ImageBuf]]:
                estimate = Estimate.of(np.stack(observations), precision)
                return buffers.located(np.ascontiguousarray(estimate.value, dtype=np.float32), image).map(estimate.holding)

            return accumulated(map(replicated, range(precision.replicates))).bind(published)

        return buffers.pixels(image).bind(computed)


# --- [OPERATIONS] -----------------------------------------------------------------------


@prepared.register
def _(
    record: Displacement, values: Mapping[Key, Value], config: PyOpenColorIO.Config, image: OpenImageIO.ImageBuf, precision: Precision, frame: CameraFrame, scene: bpy.types.Scene, /
) -> Outcome[Option[OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]:
    """Integrates the displaced stage input under original camera coordinates and the keyed gain of later exposure entries, unstaged while no field displaces."""
    spec = image.spec()
    bounds, extent = frame.window(spec)
    raster = Raster(bounds=bounds, shape=(spec.height, spec.width))

    def linear(request: Map, file: ImageFile[np.float32]) -> Outcome[npt.NDArray[np.float32]]:
        return Ok(file.pixels) if request.data else converted(config, PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR), file.pixels)

    def mapped(request: Map | None) -> Outcome[Option[npt.NDArray[np.float32]]]:
        match request:
            case Map(source=str(path)):
                return read_file(path, config, colorspace=Some(PyOpenColorIO.ROLE_DATA) if request.data else Nothing).bind(functools.partial(linear, request)).map(Some)
            case _:
                return Ok(Nothing)

    def staged(field: Displacement) -> Outcome[Option[OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]:
        match values.get(LATER.key):
            case float(gain) if field.active:
                return paired(luma(config), mapped(field.map)).bind(
                    lambda parts: field.applied(image, raster, np.asarray(extent), frame=scene.frame_current_final, luminance=parts[0], gain=gain, mapping=parts[1], precision=precision).map(Some)
                )
            case float():
                return Ok(Nothing)
            case found:
                return Error((Fault(reason=RecordReason.UNREADABLE, subject=LATER.key, cause=repr(found)),))

    return decoded(record, values).bind(staged)


def warped(shell: Shell[Displacement]) -> Outcome[Expr]:
    """Reads the image the CPU render prepared, the stage input outside one."""
    return Ok(shell.color if shell.record.staged is None else Expr(kind=bpy.types.CompositorNodeImage, members=(("image", shell.record.staged),), socket="Image", shape="RGBA"))


# --- [COMPOSITION] ----------------------------------------------------------------------

displacement = CompositorEffect[Displacement](
    record=Displacement,
    graph=warped,
    interface=Interface(
        inputs=(
            LATER,
            *(
                Socket(
                    key=f"extension_{axis}",
                    name=bpy.path.display_name(f"extension_{axis}"),
                    kind=bpy.types.NodeSocketMenu,
                    description=f"Source sampling past frame edges along {axis.upper()}",
                    members={"default_value": Border.CLAMP.item.name},
                    items=Some(BORDERS),
                )
                for axis in ("x", "y")
            ),
            Socket(key="filterwidth", name="Filter Width", kind=bpy.types.NodeSocketFloat, description="Filter support width in source pixels", members={"min_value": 0.0, "default_value": 2.0}),
        ),
        panels=(
            InterfacePanel(
                key="twirl",
                name="Twirl",
                description="Turn about a center with a smooth falloff",
                toggle=Some(False),
                inputs=tuple(
                    Socket(key=item_key("twirl", field), name=bpy.path.display_name(field), kind=kind, description=text, members=members)
                    for field, kind, text, members in (
                        ("center", bpy.types.NodeSocketVector, "Point twirl turns about", {**PLANAR, "default_value": (0.5, 0.5)}),
                        ("radius", bpy.types.NodeSocketFloat, "Distance where turn reaches zero", {"min_value": 0.0, "default_value": 0.5}),
                        ("angle", bpy.types.NodeSocketFloat, "Counterclockwise turn at center", {**ANGLE, "default_value": 6.1}),
                        ("falloff", bpy.types.NodeSocketFloat, "Radial falloff power", {"min_value": 0.0, "default_value": 1.0}),
                        ("arms", bpy.types.NodeSocketInt, "Angular turn lobes, uniform at zero", {"min_value": 0}),
                        ("arm_offset", bpy.types.NodeSocketFloat, "Angular lobe turn", ANGLE),
                    )
                ),
            ),
            InterfacePanel(
                key="wave",
                name="Wave",
                description="Ripple across traveling periodic wave fronts",
                toggle=Some(False),
                inputs=(
                    Socket(
                        key="wave_front",
                        name="Front",
                        kind=bpy.types.NodeSocketMenu,
                        description="Wave front shape and displacement direction",
                        members={"default_value": bpy.path.display_name(Wave().front.name.lower())},
                        items=Some(MenuItems(items=tuple(MenuItem(name=bpy.path.display_name(front.name.lower())) for front in Front))),
                    ),
                    *(
                        Socket(key=item_key("wave", field), name=bpy.path.display_name(field), kind=kind, description=text, members=members)
                        for field, kind, text, members in (
                            ("amplitude", bpy.types.NodeSocketFloat, "Farthest wave offset", {"min_value": 0.0, "default_value": 0.02}),
                            ("wavelength", bpy.types.NodeSocketFloat, "Distance between crests", {"min_value": 0.0, "default_value": tau / (20.0 * 2.0**0.5)}),
                            ("direction", bpy.types.NodeSocketFloat, "Wave travel direction", ANGLE),
                            ("phase", bpy.types.NodeSocketFloat, "Wave phase offset", ANGLE),
                            ("count", bpy.types.NodeSocketInt, "Waves superposed with equal total weight", {"min_value": 1, "default_value": 1}),
                            ("wavelength_variation", bpy.types.NodeSocketFloat, "Share each wavelength shrinks below maximum", {"min_value": 0.0, "max_value": 0.999}),
                            ("amplitude_variation", bpy.types.NodeSocketFloat, "Share each amplitude shrinks below maximum", FACTOR),
                            ("spread", bpy.types.NodeSocketFloat, "Angular span of wave directions", ANGLE),
                            ("axes", bpy.types.NodeSocketVector, "Signed displacement multiplier per axis", {**PLANAR, "default_value": (1.0, 1.0)}),
                            ("center", bpy.types.NodeSocketVector, "Circular front center", {**PLANAR, "default_value": (0.5, 0.5)}),
                            ("radius", bpy.types.NodeSocketFloat, "Circular front reach", {"min_value": 0.0, "default_value": 0.5}),
                            ("seed", bpy.types.NodeSocketInt, "Number selecting each wave's variation", {"min_value": 0, "max_value": 2**24}),
                        )
                    ),
                ),
                panels=(clock("wave_clock", speed=0.5 * 10.0 / tau / 24.0),),
            ),
            InterfacePanel(
                key="turbulence",
                name="Turbulence",
                description="Movement along evolving procedural noise",
                toggle=Some(False),
                inputs=tuple(
                    Socket(key=item_key("turbulence", field), name=bpy.path.display_name(field), kind=kind, description=text, members=members)
                    for field, kind, text, members in (
                        ("amplitude", bpy.types.NodeSocketFloat, "Farthest noise offset", {"min_value": 0.0, "default_value": 0.1}),
                        ("axes", bpy.types.NodeSocketVector, "Signed displacement multiplier per axis", {**PLANAR, "default_value": (1.0, 1.0)}),
                        ("seed", bpy.types.NodeSocketInt, "Number selecting pattern", {"min_value": 0, "max_value": 2**24}),
                    )
                ),
                panels=(bases("turbulence_basis", Turbulence().basis, "Noise sampled per displacement axis"), clock("turbulence_clock", speed=0.5 / 24.0)),
            ),
            InterfacePanel(
                key="sphere",
                name="Sphere",
                description="Bulge or pinch inside a circle",
                toggle=Some(False),
                inputs=tuple(
                    Socket(key=item_key("sphere", field), name=bpy.path.display_name(field), kind=kind, description=text, members=members)
                    for field, kind, text, members in (
                        ("center", bpy.types.NodeSocketVector, "Sphere center", {**PLANAR, "default_value": (0.5, 0.5)}),
                        ("radius", bpy.types.NodeSocketFloat, "Sphere radius", {"min_value": 0.0, "default_value": 0.15}),
                        ("amount", bpy.types.NodeSocketFloat, "Bulge above zero, pinch below zero", {"min_value": -1.0, "max_value": 1.0, "default_value": 0.1}),
                        ("axes", bpy.types.NodeSocketVector, "Bulge share per axis", {**PLANAR, "min_value": 0.0, "max_value": 1.0, "default_value": (1.0, 1.0)}),
                    )
                ),
            ),
            InterfacePanel(
                key="luma",
                name="Luma",
                description="Bright and dark content moving apart",
                toggle=Some(False),
                inputs=tuple(
                    Socket(key=item_key("luma", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members=members)
                    for field, text, members in (
                        ("amplitude", "Offset approached by bright content, black moving against it", {"min_value": 0.0, "default_value": 0.02}),
                        ("direction", "Direction bright content moves", ANGLE),
                        ("softness", "Luminance height blur", {"min_value": 0.0, "default_value": 0.015}),
                    )
                ),
            ),
            InterfacePanel(
                key="bubbles",
                name="Bubbles",
                description="Refraction through frame luminance height",
                toggle=Some(False),
                inputs=tuple(
                    Socket(key=item_key("bubbles", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members={"min_value": 0.0, "default_value": default})
                    for field, text, default in (("amplitude", "Refraction strength", 0.02), ("softness", "Height blur and finite-difference span, no displacement at zero", 0.015))
                ),
            ),
            InterfacePanel(
                key="raindrops",
                name="Raindrops",
                description="Drops on lens showing scene inverted",
                toggle=Some(False),
                inputs=tuple(
                    Socket(key=item_key("raindrops", field), name=bpy.path.display_name(field), kind=kind, description=text, members=members)
                    for field, kind, text, members in (
                        ("refraction", bpy.types.NodeSocketFloat, "Scene radii each drop shows inverted", {"min_value": 0.0, "default_value": 2.0}),
                        ("blur", bpy.types.NodeSocketFloat, "View blur inside each drop", {"min_value": 0.0, "default_value": 14.6 / 1920.0}),
                        ("direction", bpy.types.NodeSocketFloat, "Drop fall slant from straight down", ANGLE),
                        ("trails", bpy.types.NodeSocketFloat, "Trail height behind each drop", FACTOR),
                        ("seed", bpy.types.NodeSocketInt, "Number selecting drop population", {"min_value": 0, "max_value": 2**24}),
                        ("strength", bpy.types.NodeSocketFloat, "Signed refraction through trail height", {}),
                        ("softness", bpy.types.NodeSocketFloat, "Trail height blur radius", {"min_value": 0.0, "default_value": 0.015}),
                    )
                ),
                panels=(
                    InterfacePanel(
                        key="raindrops_drops",
                        name="Drops",
                        description="Drops placed one per cell at most",
                        inputs=tuple(
                            Socket(key=item_key("raindrops_drops", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members=members)
                            for field, text, members in (
                                ("spacing", "Cell width over greater frame side", {"min_value": 0.0, "default_value": 1.0 / (2.0 * 3.8)}),
                                ("density", "Share of cells holding a drop", {**FACTOR, "default_value": 0.5}),
                                ("size", "Drop diameter over cell width", {"min_value": 0.0, "max_value": 0.5, "default_value": 0.5}),
                                ("variation", "Diameter shrink share below size", {**FACTOR, "default_value": 0.5}),
                                ("spread", "Per-drop coverage drop in stops", {"min_value": 0.0}),
                                ("softness", "Edge fade over radius", {**FACTOR, "default_value": 0.05}),
                            )
                        ),
                    ),
                    clock("raindrops_clock", speed=0.01 / (24.0 * Raindrops().drops.spacing)),
                ),
            ),
            InterfacePanel(
                key="map",
                name="Map",
                description="Image-driven offsets or absolute coordinates",
                toggle=Some(False),
                inputs=(
                    Socket(
                        key="map_reading",
                        name="Reading",
                        kind=bpy.types.NodeSocketMenu,
                        description="Image channel meaning",
                        members={"default_value": bpy.path.display_name(Map().reading.name.lower())},
                        items=Some(MenuItems(items=tuple(MenuItem(name=bpy.path.display_name(reading.name.lower())) for reading in MapReading))),
                    ),
                    *(
                        Socket(key=item_key("map", field), name=bpy.path.display_name(field), kind=kind, description=text, members=members)
                        for field, kind, text, members in (
                            ("axes", bpy.types.NodeSocketVector, "Signed offset per axis", {**PLANAR, "default_value": (0.02, 0.02)}),
                            ("rotation", bpy.types.NodeSocketFloat, "Map and displacement axes turn", ANGLE),
                            ("softness", bpy.types.NodeSocketFloat, "Luminance height blur radius", {"min_value": 0.0, "default_value": 0.015}),
                        )
                    ),
                ),
                panels=(
                    InterfacePanel(
                        key="map_tile",
                        name="Tile",
                        description="Map tile size",
                        toggle=Some(False),
                        inputs=(
                            Socket(
                                key="map_tile_value",
                                name="Value",
                                kind=bpy.types.NodeSocketVector,
                                description="Map tile size over greater frame side",
                                members={**PLANAR, "default_value": (1.0, 1.0)},
                            ),
                        ),
                    ),
                ),
            ),
            InterfacePanel(
                key="refraction",
                name="Refraction",
                description="Refraction through glass height",
                toggle=Some(False),
                inputs=(
                    Socket(
                        key="refraction_surface",
                        name="Surface",
                        kind=bpy.types.NodeSocketMenu,
                        description="Glass height function",
                        members={"default_value": bpy.path.display_name(Refraction().surface.name.lower())},
                        items=Some(MenuItems(items=tuple(MenuItem(name=bpy.path.display_name(surface.name.lower())) for surface in Surface))),
                    ),
                    *(
                        Socket(key=item_key("refraction", field), name=bpy.path.display_name(field), kind=kind, description=text, members=members)
                        for field, kind, text, members in (
                            ("size", bpy.types.NodeSocketFloat, "Glass cell width", {"min_value": 0.0, "default_value": 0.1}),
                            ("strength", bpy.types.NodeSocketFloat, "Signed displacement per unit slope in glass-cell coordinates", {"default_value": 0.02}),
                            ("softness", bpy.types.NodeSocketFloat, "Glass height blur radius", {"min_value": 0.0, "default_value": 0.015}),
                            ("blur", bpy.types.NodeSocketFloat, "Refracted image blur radius", {"min_value": 0.0}),
                            ("seed", bpy.types.NodeSocketInt, "Number selecting frosted height", {"min_value": 0, "max_value": 2**24}),
                        )
                    ),
                ),
            ),
        ),
    ),
    alpha=Alpha.RESULT,
    parts=(Requests[Displacement](request=precise), Animates[Displacement](keys=later)),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bubbles", "Displacement", "Front", "Luma", "Map", "MapReading", "Raindrops", "Refraction", "Sphere", "Surface", "Turbulence", "Twirl", "Wave", "displacement"]
