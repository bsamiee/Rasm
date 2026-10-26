"""Gated displacement fields composed into one warp, integrated over each pixel: twirl, waves, turbulence, sphere, luminance, bubbles, image maps, glass, and raindrops."""

from collections.abc import Mapping
import functools
from math import cos, pi, sin, tau
from typing import Annotated, Final, Literal

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO
from scipy.ndimage import gaussian_filter, map_coordinates, sobel

from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes import textures
from blender.bpy.nodes.graphs import End
from blender.bpy.render.compositor.coordinates import Border, Clock
from blender.bpy.render.compositor.effects import Alpha, CameraFrame, CompositorEffect, Inspects, precise, Prepared, Prepares, Requests, Shell
from blender.bpy.render.compositor.inputs import entry_input
from blender.bpy.settings.shapes import GraphForm
from blender.imaging.color.processors import converted, GREY, luma
from blender.imaging.faults import ImagingResult
from blender.imaging.files import images
from blender.imaging.files.images import ImageFile, read_file
from blender.imaging.geometry.regions import FrameExtent, FrameLength, Position
from blender.imaging.geometry.stmaps import displaced, slope, softened, tapped
from blender.imaging.sampling.integration import Estimate, Precision, Raster, smoothstep
from blender.imaging.sampling.populations import Population
from blender.records.faults import accumulated, paired
from blender.records.fields import Choice, Finite, Gated, NonNegative, Positive, Presentation, Quantity, Seed, Unit

from ...chain.stages import Effect, Stage
from ..grade.exposure import LATER, later
from .procedural import Basis, evaluated, Perlin, profiled, WaveProfile

# --- [TYPES] ----------------------------------------------------------------------------


class Front(Choice):
    """Traveling bands or circular waves with radial and tangential motion."""

    LINEAR = 1
    RADIAL = 2
    ZIGZAG = 3
    POND = 4


class MapReading(Choice):
    """Source image reading as a coordinate field."""

    GRADIENT = 1
    LUMA = 2, "Luminance"
    OFFSETS = 3
    COORDINATES = 4


class Surface(Choice):
    """Periodic glass heights and a stochastic frosted height."""

    BLOCKS = 1
    CANVAS = 2
    FROSTED = 3
    TINY_LENS = 4


# --- [CONSTANTS] ------------------------------------------------------------------------

_DISPLACEMENT: Final[Key] = "displacement"
_DROPS: Final[Key] = "drops"

# --- [MODELS] ---------------------------------------------------------------------------

# --- [FIELDS]


class Twirl(msgspec.Struct, frozen=True, kw_only=True):
    """Turn about a center with a smooth falloff to a radius."""

    center: Annotated[Position, msgspec.Meta(description="Point twirl turns about")] = (0.5, 0.5)
    radius: Annotated[FrameExtent, msgspec.Meta(description="Distance where turn reaches zero"), Presentation(soft=Some((0.001, 1.0)))] = 0.5
    angle: Annotated[Finite, msgspec.Meta(description="Counterclockwise turn at center"), Presentation(soft=Some((-2.0 * tau, 2.0 * tau)), quantity=Some(Quantity.ANGLE))] = 6.1
    falloff: Annotated[Positive, msgspec.Meta(description="Radial falloff power")] = 1.0
    arms: Annotated[int, msgspec.Meta(ge=0, description="Angular turn lobes, uniform at zero"), GraphForm.SETTING] = 0
    arm_offset: Annotated[Finite, msgspec.Meta(description="Angular lobe turn"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0

    def displaced(self, points: npt.NDArray[np.float64], extent: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Offsets turning points about the center, the turn falling smoothly to zero at the radius."""
        arm = points - np.array(self.center) * extent
        weight = smoothstep(np.clip(1.0 - (np.linalg.norm(arm, axis=-1) / self.radius) ** (1.0 / self.falloff), 0.0, 1.0)) * (
            0.5 + 0.5 * np.cos(self.arms * (np.arctan2(arm[..., 1], arm[..., 0]) - self.arm_offset))
        )
        turn = -self.angle * weight
        return arm - np.stack((arm[..., 0] * np.cos(turn) - arm[..., 1] * np.sin(turn), arm[..., 0] * np.sin(turn) + arm[..., 1] * np.cos(turn)), axis=-1)


class Wave(msgspec.Struct, frozen=True, kw_only=True):
    """Ripple across traveling periodic wave fronts."""

    amplitude: Annotated[FrameLength, msgspec.Meta(description="Farthest wave offset"), Presentation(soft=Some((0.0, 0.1)))] = 0.02
    wavelength: Annotated[FrameExtent, msgspec.Meta(description="Distance between crests"), Presentation(soft=Some((0.01, 1.0)))] = tau / (20.0 * 2.0**0.5)
    direction: Annotated[Finite, msgspec.Meta(description="Wave travel direction"), Presentation(soft=Some((-pi, pi)), quantity=Some(Quantity.ANGLE))] = 0.0
    phase: Annotated[Finite, msgspec.Meta(description="Wave phase offset"), Presentation(soft=Some((-pi, pi)), quantity=Some(Quantity.ANGLE))] = 0.0
    clock: Annotated[Clock, msgspec.Meta(description="Crest clock, one crest per clock frame")] = Clock(speed=0.5 * 10.0 / tau / 24.0)
    profile: Annotated[WaveProfile, msgspec.Meta(description="Periodic wave profile"), GraphForm.SETTING] = "SIN"
    front: Annotated[Front, msgspec.Meta(description="Wave front shape and displacement direction"), GraphForm.SETTING] = Front.LINEAR
    count: Annotated[int, msgspec.Meta(ge=1, description="Waves superposed with equal total weight"), GraphForm.SETTING] = 1
    wavelength_variation: Annotated[float, msgspec.Meta(ge=0.0, lt=1.0, description="Share each wavelength shrinks below maximum")] = 0.0
    amplitude_variation: Annotated[Unit, msgspec.Meta(description="Share each amplitude shrinks below maximum")] = 0.0
    spread: Annotated[Finite, msgspec.Meta(description="Angular span of wave directions"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    axes: Annotated[tuple[Finite, Finite], msgspec.Meta(description="Signed displacement multiplier per axis")] = (1.0, 1.0)
    center: Annotated[Position, msgspec.Meta(description="Circular front center")] = (0.5, 0.5)
    radius: Annotated[FrameExtent, msgspec.Meta(description="Circular front reach")] = 0.5
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting each wave's variation")] = 0

    def displaced(self, points: npt.NDArray[np.float64], extent: npt.NDArray[np.float64], frame: float) -> npt.NDArray[np.float64]:
        """Offsets of every superposed crest along its front's displacement axis."""
        draws = textures.uniforms(np.arange(self.count, dtype=np.int64)[:, None], textures.stream(Wave, self.seed), 3)

        def crest(index: int) -> npt.NDArray[np.float64]:
            wavelength = self.wavelength * (1.0 - self.wavelength_variation * draws[index, 0])
            amplitude = self.amplitude * (1.0 - self.amplitude_variation * draws[index, 1]) / self.count
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
            cycles = travel / wavelength - self.clock.clock(frame) + self.phase / tau + (draws[index, 2] if self.count > 1 else 0.0)
            return amplitude * (2.0 * profiled(self.profile, cycles, np) - 1.0)[..., None] * envelope[..., None] * axis * self.axes

        return sum((crest(index) for index in range(self.count)), np.zeros_like(points))


class Turbulence(msgspec.Struct, frozen=True, kw_only=True):
    """Movement along evolving procedural noise."""

    amplitude: Annotated[FrameLength, msgspec.Meta(description="Farthest noise offset"), Presentation(soft=Some((0.0, 0.2)))] = 0.1
    basis: Annotated[Basis, msgspec.Meta(description="Noise sampled per displacement axis")] = Perlin(texture=textures.Noise(noise_dimensions="4D", scale=2.0))
    axes: Annotated[tuple[Finite, Finite], msgspec.Meta(description="Signed displacement multiplier per axis")] = (1.0, 1.0)
    clock: Annotated[Clock, msgspec.Meta(description="Noise clock, one noise unit per clock frame")] = Clock(speed=0.5 / 24.0)
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting pattern")] = 0

    def displaced(self, points: npt.NDArray[np.float64], frame: float) -> npt.NDArray[np.float64]:
        """Offsets of one noise per axis at the held clock."""
        flat = points.reshape(-1, 2)
        moment = np.full(len(flat), self.clock.clock(frame))
        fields = np.stack([evaluated(self.basis, flat, moment, seed=self.seed, stream=textures.stream(Turbulence, axis))[:, 0] for axis in range(2)], axis=-1)
        return self.amplitude * (2.0 * fields.reshape(points.shape) - 1.0) * self.axes


class Sphere(msgspec.Struct, frozen=True, kw_only=True):
    """Bulge or pinch inside a circle with a hemisphere mapping."""

    center: Annotated[Position, msgspec.Meta(description="Sphere center")] = (0.5, 0.5)
    radius: Annotated[FrameExtent, msgspec.Meta(description="Sphere radius"), Presentation(soft=Some((0.001, 1.0)))] = 0.15
    amount: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0, description="Bulge above zero, pinch below zero")] = 0.1
    axes: Annotated[tuple[Unit, Unit], msgspec.Meta(description="Bulge share per axis")] = (1.0, 1.0)

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

    amplitude: Annotated[FrameLength, msgspec.Meta(description="Offset approached by bright content, black moving against it"), Presentation(soft=Some((0.0, 0.1)))] = 0.02
    direction: Annotated[Finite, msgspec.Meta(description="Direction bright content moves"), Presentation(soft=Some((-pi, pi)), quantity=Some(Quantity.ANGLE))] = 0.0
    softness: Annotated[FrameLength, msgspec.Meta(description="Luminance height blur"), Presentation(soft=Some((0.0, 0.2)))] = 0.015


class Bubbles(msgspec.Struct, frozen=True, kw_only=True):
    """Refracts through frame luminance height."""

    amplitude: Annotated[FrameLength, msgspec.Meta(description="Refraction strength"), Presentation(soft=Some((0.0, 0.1)))] = 0.02
    softness: Annotated[FrameLength, msgspec.Meta(description="Height blur and finite-difference span, no displacement at zero"), Presentation(soft=Some((0.0, 0.2)))] = 0.015


class Raindrops(msgspec.Struct, frozen=True, kw_only=True):
    """Drops showing an inverted, blurred view around their centers."""

    drops: Annotated[Population, msgspec.Meta(description="Drops placed one per cell at most")] = Population(spacing=1.0 / (2.0 * 3.8), size=0.5, spread=0.0, softness=0.05)
    refraction: Annotated[NonNegative, msgspec.Meta(description="Scene radii each drop shows inverted"), Presentation(soft=Some((0.0, 10.0)))] = 2.0
    blur: Annotated[FrameLength, msgspec.Meta(description="View blur inside each drop"), Presentation(soft=Some((0.0, 0.02)))] = 14.6 / 1920.0
    direction: Annotated[Finite, msgspec.Meta(description="Drop fall slant from straight down"), Presentation(soft=Some((-pi / 3.0, pi / 3.0)), quantity=Some(Quantity.ANGLE))] = 0.0
    clock: Annotated[Clock, msgspec.Meta(description="Fall clock, one drop spacing per clock frame")] = Clock(speed=0.01 / (24.0 * drops.spacing))
    trails: Annotated[Unit, msgspec.Meta(description="Trail height behind each drop")] = 0.0
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting drop population")] = 0
    strength: Annotated[Finite, msgspec.Meta(description="Signed refraction through trail height"), Presentation(quantity=Some(Quantity.FRAME))] = 0.0
    softness: Annotated[FrameLength, msgspec.Meta(description="Trail height blur radius"), Presentation(soft=Some((0.0, 0.2)))] = 0.015

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
    """Image read as luminance, slope, offsets, or absolute source coordinates."""

    source: Annotated[str | None, msgspec.Meta(description="Image file path, input image when absent"), GraphForm.SETTING] = None
    reading: Annotated[MapReading, msgspec.Meta(description="Image channel meaning"), GraphForm.SETTING] = MapReading.OFFSETS
    axes: Annotated[Position, msgspec.Meta(description="Signed offset per axis")] = (0.02, 0.02)
    tile: Annotated[Gated[tuple[FrameExtent, FrameExtent]], msgspec.Meta(description="Map tile size")] = Gated(enabled=False, value=(1.0, 1.0))
    rotation: Annotated[Finite, msgspec.Meta(description="Map and displacement axes turn"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    softness: Annotated[FrameLength, msgspec.Meta(description="Luminance height blur radius"), Presentation(soft=Some((0.0, 0.2)))] = 0.015

    @property
    def data(self) -> bool:
        """Whether the image holds data channels read untransformed."""
        return self.reading in {MapReading.OFFSETS, MapReading.COORDINATES}


class Refraction(msgspec.Struct, frozen=True, kw_only=True):
    """Refraction and blur through a periodic or frosted glass height."""

    surface: Annotated[Surface, msgspec.Meta(description="Glass height function"), GraphForm.SETTING] = Surface.FROSTED
    size: Annotated[FrameExtent, msgspec.Meta(description="Glass cell width")] = 0.1
    strength: Annotated[Finite, msgspec.Meta(description="Signed displacement per unit slope in glass-cell coordinates"), Presentation(quantity=Some(Quantity.FRAME))] = 0.02
    softness: Annotated[FrameLength, msgspec.Meta(description="Glass height blur radius"), Presentation(soft=Some((0.0, 0.2)))] = 0.015
    blur: Annotated[FrameLength, msgspec.Meta(description="Refracted image blur radius"), Presentation(soft=Some((0.0, 0.02)))] = 0.0
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting frosted height")] = 0

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
    """Composes geometric, image-driven, glass, and falling-drop displacement fields."""

    stage = Stage.WARP
    pointwise = False
    twirl: Annotated[Gated[Twirl], msgspec.Meta(description="Turn about a center with a smooth falloff")] = Gated(enabled=False, value=Twirl())
    wave: Annotated[Gated[Wave], msgspec.Meta(description="Ripple across traveling periodic wave fronts")] = Gated(enabled=False, value=Wave())
    turbulence: Annotated[Gated[Turbulence], msgspec.Meta(description="Movement along evolving procedural noise")] = Gated(enabled=False, value=Turbulence())
    sphere: Annotated[Gated[Sphere], msgspec.Meta(description="Bulge or pinch inside a circle")] = Gated(enabled=False, value=Sphere())
    luma: Annotated[Gated[Luma], msgspec.Meta(description="Bright and dark content moving apart")] = Gated(enabled=False, value=Luma())
    bubbles: Annotated[Gated[Bubbles], msgspec.Meta(description="Refraction through frame luminance height")] = Gated(enabled=False, value=Bubbles())
    raindrops: Annotated[Gated[Raindrops], msgspec.Meta(description="Drops on lens showing scene inverted")] = Gated(enabled=False, value=Raindrops())
    map: Annotated[Gated[Map], msgspec.Meta(description="Image-driven offsets or absolute coordinates")] = Gated(enabled=False, value=Map())
    refraction: Annotated[Gated[Refraction], msgspec.Meta(description="Refraction through glass height")] = Gated(enabled=False, value=Refraction())
    extension: Annotated[tuple[Border, Border], msgspec.Meta(description="Source sampling past frame edges per axis")] = (Border.CLAMP, Border.CLAMP)
    filtername: Annotated[str, msgspec.Meta(description="OpenImageIO reconstruction filter name"), GraphForm.SETTING] = "triangle"
    filterwidth: Annotated[Positive, msgspec.Meta(description="Filter support width in source pixels")] = 2.0

    @property
    def active(self) -> bool:
        """Whether any field displaces."""
        return any(value.enabled for value in msgspec.structs.astuple(self) if isinstance(value, Gated))

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
    ) -> ImagingResult[Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]]:
        """Integrates refracted color, displacement, and drop coverage over each pixel.

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
            Estimated image, displacement, and drop coverage with joint replicate error, or image faults.
        """
        left, bottom, right, top = raster.bounds
        wrap = (self.extension[0].wrap, self.extension[1].wrap)

        def computed(source: npt.NDArray[np.float32]) -> ImagingResult[Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]]:
            height, width, channels = source.shape
            centers = raster.centers

            def lifted(plane: npt.NDArray[np.float32]) -> npt.NDArray[np.float64]:
                positive = np.maximum(images.unassociated(plane)[..., :3] @ luminance, 0.0)
                return positive / (positive + GREY / gain)

            base = lifted(source)
            luma_height = softened(base, raster, self.luma.value.softness) if self.luma.enabled else base
            bubble_height = softened(base, raster, self.bubbles.value.softness) if self.bubbles.enabled else base
            rain = self.raindrops.value
            rain_gradient = (
                rain.strength * rain.drops.spacing * slope(softened(rain.sampled(centers.reshape(-1, 2), frame)[:, 3].reshape(height, width), raster, rain.softness), raster)
                if self.raindrops.enabled and rain.strength != 0.0
                else np.zeros((height, width, 2))
            )
            glass = self.refraction.value
            glass_gradient = glass.strength * glass.size * slope(softened(glass.height(centers), raster, glass.softness), raster) if self.refraction.enabled else np.zeros((height, width, 2))
            request = self.map.value
            plane = mapping.default_value(source)
            map_origin, map_span = ((0.0, 0.0), tuple(extent)) if mapping.is_some() else ((left, bottom), (right - left, top - bottom))
            map_step = np.array(request.tile.value if request.tile.enabled else map_span) / plane.shape[1::-1]
            mode: Literal["wrap", "nearest"] = "wrap" if request.tile.enabled else "nearest"
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
            glass_source = softened(source.astype(np.float64), raster, glass.blur).astype(np.float32) if self.refraction.enabled else source
            drop_source = softened(source.astype(np.float64), raster, rain.blur).astype(np.float32) if self.raindrops.enabled else source

            def rotated(points: npt.NDArray[np.float64], angle: float) -> npt.NDArray[np.float64]:
                return points @ np.array(((cos(angle), sin(angle)), (-sin(angle), cos(angle))), dtype=np.float64)

            def composed(points: npt.NDArray[np.float64]) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
                coverage = np.ones(points.shape[:-1], dtype=np.float64)
                terms = (
                    *((self.twirl.value.displaced(points, extent),) if self.twirl.enabled else ()),
                    *((self.wave.value.displaced(points, extent, frame),) if self.wave.enabled else ()),
                    *((self.turbulence.value.displaced(points, frame),) if self.turbulence.enabled else ()),
                    *((self.sphere.value.displaced(points, extent),) if self.sphere.enabled else ()),
                    *(
                        (self.luma.value.amplitude * (2.0 * tapped(luma_height, raster, points) - 1.0)[..., None] * np.array((cos(self.luma.value.direction), sin(self.luma.value.direction))),)
                        if self.luma.enabled
                        else ()
                    ),
                    *(
                        (
                            self.bubbles.value.amplitude
                            * np.stack([tapped(bubble_height, raster, points - axis) - tapped(bubble_height, raster, points + axis) for axis in np.eye(2) * self.bubbles.value.softness], axis=-1),
                        )
                        if self.bubbles.enabled
                        else ()
                    ),
                    *((np.stack([tapped(glass_gradient[..., axis], raster, points) for axis in range(2)], axis=-1),) if self.refraction.enabled else ()),
                )
                displacement = sum(terms, np.zeros_like(points))
                if self.map.enabled:
                    local = rotated(points, -request.rotation)
                    fraction = local / request.tile.value % 1.0 if request.tile.enabled else (local - map_origin) / map_span
                    at = np.stack(((1.0 - fraction[..., 1]) * map_field.shape[0] - 0.5, fraction[..., 0] * map_field.shape[1] - 0.5))
                    field = np.stack([map_coordinates(map_field[..., axis], at, order=1, mode="grid-wrap" if request.tile.enabled else "nearest", prefilter=False) for axis in range(3)], axis=-1)
                    if request.reading is MapReading.COORDINATES:
                        return displacement + points - rotated(field[..., :2] * extent, request.rotation), field[..., 2]
                    return displacement + rotated(field[..., :2] * request.axes * extent, request.rotation), coverage
                return displacement, coverage

            def dropped(points: npt.NDArray[np.float64]) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
                if not self.raindrops.enabled:
                    return np.zeros(points.shape[:-1]), np.zeros_like(points)
                field = rain.sampled(points.reshape(-1, 2), frame)
                return field[:, 2].reshape(points.shape[:-1]), field[:, :2].reshape(points.shape) + np.stack([tapped(rain_gradient[..., axis], raster, points) for axis in range(2)], axis=-1)

            def observed(points: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
                displacement, coverage = composed(points)
                drops, refraction = dropped(points)
                return paired(
                    displaced(glass_source, raster, points - displacement, coverage, wrap, filtername=self.filtername, filterwidth=self.filterwidth),
                    displaced(drop_source, raster, points - displacement - refraction, coverage, wrap, filtername=self.filtername, filterwidth=self.filterwidth),
                ).map(lambda colors: np.concatenate((colors[0] * (1.0 - drops[..., None]) + colors[1] * drops[..., None], displacement + drops[..., None] * refraction, drops[..., None]), axis=-1))

            def replicated(replicate: int) -> ImagingResult[npt.NDArray[np.float64]]:
                net = precision.points(2, replicate)
                return functools.reduce(
                    lambda total, offset: total.bind(lambda held: observed(centers + (offset - 0.5) * raster.step * (1.0, -1.0)).map(lambda term: held + term / len(net))),
                    net,
                    Ok(np.zeros((height, width, channels + 3), dtype=np.float64)),
                )

            def published(observations: tuple[npt.NDArray[np.float64], ...]) -> ImagingResult[Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]]:
                estimate = Estimate.of(np.stack(observations), precision)
                return accumulated(
                    images.located(np.ascontiguousarray(part, dtype=np.float32), image)
                    for part in (estimate.value[..., :channels], estimate.value[..., channels : channels + 2], estimate.value[..., channels + 2 :])
                ).map(lambda buffers: estimate.holding((buffers[0], buffers[1], buffers[2])))

            return accumulated(map(replicated, range(precision.replicates))).bind(published)

        return images.pixels(image).bind(computed)


# --- [OPERATIONS] -----------------------------------------------------------------------


def prepared(
    identity: Identity,
    scene: bpy.types.Scene,
    record: Displacement,
    config: PyOpenColorIO.Config,
    source: OpenImageIO.ImageBuf,
    precision: Precision,
    frame: CameraFrame,
    entry: bpy.types.SceneCompositorEffect,
) -> BlenderResult[Prepared[Displacement]]:
    """Integrates the displaced stage input under original camera coordinates and the keyed gain of later exposure entries, the record unchanged."""
    spec = source.spec()
    bounds, extent = frame.window(spec)
    request = record.map.value

    def linear(file: ImageFile[np.float32]) -> ImagingResult[npt.NDArray[np.float32]]:
        return Ok(file.pixels) if request.data else converted(config, PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR), file.pixels)

    mapping: ImagingResult[Option[npt.NDArray[np.float32]]] = (
        Option
        .of_optional(request.source)
        .filter(lambda _: record.map.enabled)
        .map(lambda path: read_file(path, config, colorspace=Some(PyOpenColorIO.ROLE_DATA) if request.data else Nothing).bind(linear).map(Some))
        .default_value(Ok(Nothing))
    )
    gain = entry_input(identity, entry, LATER.key).bind(lambda socket: Descriptor.resolve(socket, "value", bpy.types.FloatProperty).map(lambda held: msgspec.convert(held.read(socket), float)))

    def published(estimate: Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]) -> Prepared[Displacement]:
        channels = spec.nchannels
        parts: Mapping[Key, tuple[OpenImageIO.ImageBuf, slice]] = {
            Shell.image: (estimate.value[0], slice(0, channels)),
            _DISPLACEMENT: (estimate.value[1], slice(channels, channels + 2)),
            _DROPS: (estimate.value[2], slice(channels + 2, channels + 3)),
        }
        return record, {
            key: Estimate(value=plane, standard_error=estimate.standard_error[..., part], precision=precision, replicates=estimate.replicates[..., part]) for key, (plane, part) in parts.items()
        }

    raster = Raster(bounds=bounds, shape=(spec.height, spec.width))
    return paired(luma(config), mapping, gain).bind(
        lambda values: record.applied(source, raster, np.asarray(extent), frame=scene.frame_current_final, luminance=values[0], gain=values[2], mapping=values[1], precision=precision).map(published)
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

displacement = CompositorEffect(
    record=Displacement,
    graph=Shell.passed,
    alpha=Alpha.RESULT,
    parts=(
        Requests[Displacement](request=precise),
        Prepares(prepare=prepared, when=lambda record: record.active),
        *later,
        Inspects(key="displacement_field", end=End(node=_DISPLACEMENT, socket="Image"), description="Integrated displacement in displayed frame units"),
        Inspects(key="raindrop_coverage", end=End(node=_DROPS, socket="Image"), description="Integrated refracting drop and trail coverage"),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bubbles", "Displacement", "Front", "Luma", "Map", "MapReading", "Raindrops", "Refraction", "Sphere", "Surface", "Turbulence", "Twirl", "Wave", "displacement"]
