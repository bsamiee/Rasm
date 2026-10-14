"""Gated geometric fields and integrated refracting source samples."""

from fractions import Fraction
import functools
import math
import sys
from typing import Annotated, Literal

from expression import Error, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
from scipy.ndimage import gaussian_filter, map_coordinates, sobel

from blender.imaging.chain import Effect, GREY, Stage
from blender.imaging.color.processors import unassociated
from blender.imaging.effects.generators import noise
from blender.imaging.effects.generators.noise import Basis, Cut, Estimate, estimated, Interpolation, Octaves, Perlin, Precision, Profile, sample, uniforms
from blender.imaging.effects.generators.particles import Population
from blender.imaging.faults import accumulated, ImagingBufferFault, ImagingGeneratorFault, ImagingResult, paired
from blender.imaging.geometry.stmaps import Extension, StMap
from blender.imaging.parameters import Choice, Finite, Gated, GraphForm, Motion, Position, Presentation, Quantity, Seed

# --- [TYPES] ----------------------------------------------------------------------------

type _Length = Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max), Presentation(quantity=Some(Quantity.FRAME_GREATER))]
type _Radius = Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max), Presentation(quantity=Some(Quantity.FRAME_GREATER))]
type _Fraction = Annotated[float, msgspec.Meta(ge=0.0, le=1.0)]


class Front(Choice):
    """Travelling bands or circular waves with radial and tangential motion."""

    LINEAR = 1, "Linear"
    RADIAL = 2, "Radial"
    ZIGZAG = 3, "Zigzag"
    POND = 4, "Pond"


class MapReading(Choice):
    """Interpretation of a source image as a coordinate field."""

    GRADIENT = 1, "Gradient"
    LUMA = 2, "Luminance"
    OFFSETS = 3, "Offsets"
    COORDINATES = 4, "Coordinates"


class Surface(Choice):
    """Periodic glass heights and a stochastic frosted height."""

    BLOCKS = 1, "Blocks"
    CANVAS = 2, "Canvas"
    FROSTED = 3, "Frosted"
    TINY_LENS = 4, "Tiny Lens"


# --- [MODELS] ---------------------------------------------------------------------------


class Twirl(msgspec.Struct, frozen=True, kw_only=True):
    """Turn about a center with a smooth falloff to a radius."""

    center: Annotated[Position, msgspec.Meta(description="Point the twirl turns about")] = (0.5, 0.5)
    radius: Annotated[
        float, msgspec.Meta(gt=0.0, le=sys.float_info.max, description="Distance where the turn reaches zero"), Presentation(soft=Some((0.001, 1.0)), quantity=Some(Quantity.FRAME_GREATER))
    ] = 0.5
    angle: Annotated[Finite, msgspec.Meta(description="Turn at the center, counterclockwise above zero"), Presentation(soft=Some((-2.0 * math.tau, 2.0 * math.tau)), quantity=Some(Quantity.ANGLE))] = (
        6.1
    )
    falloff: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max, description="Radial falloff power")] = 1.0
    arms: Annotated[int, msgspec.Meta(ge=0, description="Angular lobes of the turn, zero giving a uniform twirl"), GraphForm.SETTING] = 0
    arm_offset: Annotated[Finite, msgspec.Meta(description="Rotation of the angular lobes"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0


class Wave(msgspec.Struct, frozen=True, kw_only=True):
    """Ripple across travelling periodic wave fronts."""

    amplitude: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Farthest offset across the wave"), Presentation(soft=Some((0.0, 0.1)), quantity=Some(Quantity.FRAME_GREATER))
    ] = 0.02
    wavelength: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max, description="Distance between crests"), Presentation(soft=Some((0.01, 1.0)), quantity=Some(Quantity.FRAME_GREATER))] = (
        math.tau / (20.0 * math.sqrt(2.0))
    )
    direction: Annotated[Finite, msgspec.Meta(description="Direction the wave travels"), Presentation(soft=Some((-math.pi, math.pi)), quantity=Some(Quantity.ANGLE))] = 0.0
    phase: Annotated[Finite, msgspec.Meta(description="Phase added to the travelling wave in radians"), Presentation(soft=Some((-math.pi, math.pi)), quantity=Some(Quantity.ANGLE))] = 0.0
    motion: Annotated[Motion, msgspec.Meta(description="Clock of the crests, one crest per clock frame")] = Motion(speed=0.5 * 10.0 / math.tau / 24.0)
    profile: Annotated[Profile, msgspec.Meta(description="Periodic wave profile"), GraphForm.SETTING] = Profile.SIN
    front: Annotated[Front, msgspec.Meta(description="Shape and displacement direction of the wave fronts"), GraphForm.SETTING] = Front.LINEAR
    count: Annotated[int, msgspec.Meta(ge=1, description="Waves superposed with equal total weight"), GraphForm.SETTING] = 1
    wavelength_variation: Annotated[float, msgspec.Meta(ge=0.0, lt=1.0, description="Share each wavelength shrinks below its maximum")] = 0.0
    amplitude_variation: Annotated[_Fraction, msgspec.Meta(description="Share each wave's amplitude shrinks below its maximum")] = 0.0
    spread: Annotated[Finite, msgspec.Meta(description="Angular span of the wave directions"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    axes: Annotated[tuple[Finite, Finite], msgspec.Meta(description="Signed displacement multiplier on each frame axis")] = (1.0, 1.0)
    center: Annotated[Position, msgspec.Meta(description="Center of circular wave fronts")] = (0.5, 0.5)
    radius: Annotated[_Length, msgspec.Meta(description="Reach of circular wave fronts")] = 0.5
    seed: Annotated[Seed, msgspec.Meta(description="Independent wavelength, amplitude, and phase per wave")] = 0


class Turbulence(msgspec.Struct, frozen=True, kw_only=True):
    """Movement along evolving fractal noise."""

    amplitude: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Farthest offset the noise reaches"), Presentation(soft=Some((0.0, 0.2)), quantity=Some(Quantity.FRAME_GREATER))
    ] = 0.1
    basis: Annotated[Basis, msgspec.Meta(description="Texture sampled for each displacement axis")] = Perlin(
        size=0.5, stretch=1.0, octaves=Octaves(), distortion=0.0, cut=Cut(interpolation=Interpolation.LINEAR)
    )
    axes: Annotated[tuple[Finite, Finite], msgspec.Meta(description="Signed displacement multiplier on each frame axis")] = (1.0, 1.0)
    motion: Annotated[Motion, msgspec.Meta(description="Clock of the noise, one noise unit per clock frame")] = Motion(speed=0.5 / 24.0)
    seed: Annotated[Seed, msgspec.Meta(description="Noise variant")] = 0


class Sphere(msgspec.Struct, frozen=True, kw_only=True):
    """Bulge or pinch inside a circle with a hemisphere mapping."""

    center: Annotated[Position, msgspec.Meta(description="Center of the sphere")] = (0.5, 0.5)
    radius: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max, description="Radius of the sphere"), Presentation(soft=Some((0.001, 1.0)), quantity=Some(Quantity.FRAME_GREATER))] = 0.15
    amount: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0, description="Bulge above zero, pinch below zero")] = 0.1
    axes: Annotated[tuple[_Fraction, _Fraction], msgspec.Meta(description="Share of the bulge along each axis, (1,0) horizontal and (0,1) vertical")] = (1.0, 1.0)


class Luma(msgspec.Struct, frozen=True, kw_only=True):
    """Move bright and dark content in opposite directions."""

    amplitude: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Offset approached by bright content, black moving against it"),
        Presentation(soft=Some((0.0, 0.1)), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 0.02
    direction: Annotated[Finite, msgspec.Meta(description="Direction bright content moves"), Presentation(soft=Some((-math.pi, math.pi)), quantity=Some(Quantity.ANGLE))] = 0.0
    softness: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Blur of the luminance height"), Presentation(soft=Some((0.0, 0.2)), quantity=Some(Quantity.FRAME_GREATER))] = (
        0.015
    )


class Bubbles(msgspec.Struct, frozen=True, kw_only=True):
    """Refract through the frame's luminance height."""

    amplitude: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Refraction through the luminance height"), Presentation(soft=Some((0.0, 0.1)), quantity=Some(Quantity.FRAME_GREATER))
    ] = 0.02
    softness: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Blur and finite-difference span of the luminance height, zero giving no displacement"),
        Presentation(soft=Some((0.0, 0.2)), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 0.015


class Raindrops(msgspec.Struct, frozen=True, kw_only=True):
    """Drops showing an inverted, blurred view around their centers."""

    drops: Annotated[Population, msgspec.Meta(description="Drops placed one per cell at most")] = Population(spacing=1.0 / (2.0 * 3.8), size=0.5, spread=0.0, softness=0.05)
    refraction: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Radii of the scene each drop shows inverted"), Presentation(soft=Some((0.0, 10.0)))] = 2.0
    blur: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Blur of the view inside each drop"), Presentation(soft=Some((0.0, 0.02)), quantity=Some(Quantity.FRAME_GREATER))
    ] = 14.6 / 1920.0
    direction: Annotated[Finite, msgspec.Meta(description="Slant of the drops' fall from straight down"), Presentation(soft=Some((-math.pi / 3.0, math.pi / 3.0)), quantity=Some(Quantity.ANGLE))] = 0.0
    motion: Annotated[Motion, msgspec.Meta(description="Clock of the fall, one drop spacing per clock frame")] = Motion(speed=0.01 / (24.0 * drops.spacing))
    trails: Annotated[_Fraction, msgspec.Meta(description="Height of the receding trail behind each falling drop")] = 0.0
    seed: Annotated[Seed, msgspec.Meta(description="Stable drop population variant")] = 0
    strength: Annotated[Finite, msgspec.Meta(description="Signed refraction through the receding trail height"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = 0.0
    softness: Annotated[_Radius, msgspec.Meta(description="Blur radius of the receding trail height")] = 0.015

    def sampled(self, points: npt.NDArray[np.float64], reference_frame: float) -> ImagingResult[npt.NDArray[np.float64]]:
        """Returns continuous inversion offsets, coverage, and receding trail height."""
        try:
            with np.errstate(over="raise", divide="raise", invalid="raise"):
                sine, cosine = math.sin(self.direction), math.cos(self.direction)
                turn = np.array(((cosine, sine), (-sine, cosine)))
                local = points @ turn
                local[:, 1] += self.motion.speed * self.motion.hold * math.floor(reference_frame / self.motion.hold) * self.drops.spacing
                cells = [(math.floor(point[0] / self.drops.spacing), math.floor(point[1] / self.drops.spacing)) for point in local]
                unique = tuple(dict.fromkeys(cells))
                indices = {cell: index for index, cell in enumerate(unique)}
                ownership = np.array([indices[cell] for cell in cells])
                result = np.zeros((len(points), 4), dtype=np.float64)
                for mark in self.drops.sites(unique, seed=self.seed):
                    selected = ownership == indices[mark.cell]
                    arm = local[selected] - tuple(map(float, mark.center))
                    radial = np.linalg.norm(arm, axis=-1) / mark.radius
                    drop = (radial < 1.0).astype(np.float64) if self.drops.softness == 0.0 else np.clip((1.0 - radial) / self.drops.softness, 0.0, 1.0)
                    drop = drop * drop * (3.0 - 2.0 * drop)
                    feature = float(mark.center[1] - Fraction(mark.cell[1]) * Fraction(self.drops.spacing))
                    lag = self.trails * np.where(arm[:, 1] <= 0.0, np.maximum(0.0, 1.0 + arm[:, 1] / feature), 0.0)
                    cross = np.abs(arm[:, 0]) / (0.5 * mark.radius)
                    trail = (cross < 1.0).astype(np.float64) if self.drops.softness == 0.0 else np.clip((1.0 - cross) / self.drops.softness, 0.0, 1.0)
                    trail = trail * trail * (3.0 - 2.0 * trail)
                    result[selected, :2] = (arm @ turn.T) * ((1.0 + self.refraction) * (drop > 0.0))[:, None]
                    result[selected, 2] = np.maximum(drop, lag * trail) * mark.gain
                    result[selected, 3] = lag * (1.0 - drop) * np.sqrt(np.maximum(0.0, 1.0 - cross * cross))
                return Ok(result)
        except (FloatingPointError, MemoryError, OverflowError, ValueError) as error:
            return Error((ImagingGeneratorFault(request=self, error=error),))


class Map(msgspec.Struct, frozen=True, kw_only=True):
    """Image read as luminance, slope, offsets, or absolute source coordinates."""

    source: Annotated[str | None, msgspec.Meta(description="Image file path, absent to read the entering image"), GraphForm.SETTING] = None
    reading: Annotated[MapReading, msgspec.Meta(description="Meaning of the image channels"), GraphForm.SETTING] = MapReading.OFFSETS
    axes: Annotated[Position, msgspec.Meta(description="Signed offset on each frame axis")] = (0.02, 0.02)
    tile: Annotated[Gated[tuple[_Length, _Length]], msgspec.Meta(description="Repeated map tile dimensions")] = Gated(enabled=False, value=(1.0, 1.0))
    rotation: Annotated[Finite, msgspec.Meta(description="Turn of the map and its displacement axes"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    softness: Annotated[_Radius, msgspec.Meta(description="Blur radius of the luminance height")] = 0.015


class Refraction(msgspec.Struct, frozen=True, kw_only=True):
    """Refraction and blur through a periodic or frosted glass height."""

    surface: Annotated[Surface, msgspec.Meta(description="Glass height function"), GraphForm.SETTING] = Surface.FROSTED
    size: Annotated[_Length, msgspec.Meta(description="Width of the glass cells")] = 0.1
    strength: Annotated[Finite, msgspec.Meta(description="Signed displacement per unit slope in glass-cell coordinates"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = 0.02
    softness: Annotated[_Radius, msgspec.Meta(description="Blur radius of the glass height")] = 0.015
    blur: Annotated[_Radius, msgspec.Meta(description="Blur radius of the refracted image")] = 0.0
    seed: Annotated[Seed, msgspec.Meta(description="Variant of the frosted height")] = 0


class Displacement(Effect, frozen=True, kw_only=True, tag=37):
    """Compose geometric, image-driven, glass, and falling-drop displacement fields."""

    stage = Stage.WARP
    title = "Displacement"
    pointwise = False
    twirl: Annotated[Gated[Twirl], msgspec.Meta(description="Turn about a center with a smooth falloff")] = Gated(enabled=False, value=Twirl())
    wave: Annotated[Gated[Wave], msgspec.Meta(description="Ripple across travelling periodic wave fronts")] = Gated(enabled=False, value=Wave())
    turbulence: Annotated[Gated[Turbulence], msgspec.Meta(description="Movement along evolving fractal noise")] = Gated(enabled=False, value=Turbulence())
    sphere: Annotated[Gated[Sphere], msgspec.Meta(description="Bulge or pinch inside a circle")] = Gated(enabled=False, value=Sphere())
    luma: Annotated[Gated[Luma], msgspec.Meta(description="Movement of bright and dark content in opposite directions")] = Gated(enabled=False, value=Luma())
    bubbles: Annotated[Gated[Bubbles], msgspec.Meta(description="Refraction through the frame's luminance height")] = Gated(enabled=False, value=Bubbles())
    raindrops: Annotated[Gated[Raindrops], msgspec.Meta(description="Drops on the lens showing the scene inverted")] = Gated(enabled=False, value=Raindrops())
    map: Annotated[Gated[Map], msgspec.Meta(description="Image-driven offsets or absolute coordinates")] = Gated(enabled=False, value=Map())
    refraction: Annotated[Gated[Refraction], msgspec.Meta(description="Refract through a glass height")] = Gated(enabled=False, value=Refraction())
    extension: Annotated[tuple[Extension, Extension], msgspec.Meta(description="Sampling rule past each source axis")] = (Extension.EXTEND, Extension.EXTEND)
    filtername: Annotated[str, msgspec.Meta(description="Reconstruction filter name supplied by the native sampler"), GraphForm.SETTING] = "triangle"
    filterwidth: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max, description="Full filter support width in source pixels")] = 2.0

    def applied(
        self,
        image: OpenImageIO.ImageBuf,
        bounds: tuple[float, float, float, float],
        extent: tuple[float, float],
        *,
        reference_frame: float,
        luminance: npt.NDArray[np.float64],
        gain: float,
        mapping: Option[npt.NDArray[np.float32]],
        precision: Precision,
    ) -> ImagingResult[Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]]:
        """Integrates refracted color, displacement, and drop coverage over each pixel."""
        try:
            if (source := image.get_pixels(OpenImageIO.FLOAT)) is None:
                return Error((ImagingBufferFault(request=image, error=image.geterror()),))
            with np.errstate(over="raise", divide="raise", invalid="raise"):
                height, width, channels = source.shape
                left, bottom, right, top = bounds
                step = np.array(((right - left) / width, (top - bottom) / height))
                s, t = np.meshgrid(np.arange(width), np.arange(height))
                centers = np.stack((left + (s + 0.5) * step[0], top - (t + 0.5) * step[1]), axis=-1)
                shape = centers.shape

                def clock(motion: Motion) -> float:
                    return motion.speed * motion.hold * math.floor(reference_frame / motion.hold)

                def rotated(points: npt.NDArray[np.float64], angle: float) -> npt.NDArray[np.float64]:
                    sine, cosine = math.sin(angle), math.cos(angle)
                    return points @ np.array(((cosine, sine), (-sine, cosine)), dtype=np.float64)

                def softened(plane: npt.NDArray[np.float64], radius: float) -> npt.NDArray[np.float64]:
                    return gaussian_filter(plane, (radius / step[1], radius / step[0]), axes=(0, 1), mode="nearest")

                def height_of(plane: npt.NDArray[np.float32]) -> npt.NDArray[np.float64]:
                    positive = np.maximum(unassociated(plane)[..., :3] @ luminance, 0.0)
                    return positive / (positive + GREY / gain)

                def tapped(plane: npt.NDArray[np.float64], points: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
                    coordinates = np.stack(((top - points[..., 1]) / step[1] - 0.5, (points[..., 0] - left) / step[0] - 0.5))
                    return map_coordinates(plane, coordinates, order=1, mode="nearest", prefilter=False)

                def slope(plane: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
                    return np.stack((sobel(plane, axis=1, mode="nearest") / (8.0 * step[0]), -sobel(plane, axis=0, mode="nearest") / (8.0 * step[1])), axis=-1)

                base_height = height_of(source)
                luma_height = softened(base_height, self.luma.value.softness) if self.luma.enabled else base_height
                bubble_height = softened(base_height, self.bubbles.value.softness) if self.bubbles.enabled else base_height

                def trailed(rain: Raindrops) -> ImagingResult[npt.NDArray[np.float64]]:
                    return rain.sampled(centers.reshape(-1, 2), reference_frame).map(
                        lambda field: rain.strength * rain.drops.spacing * slope(softened(field[:, 3].reshape(height, width), rain.softness))
                    )

                def glazed(glass: Refraction) -> ImagingResult[npt.NDArray[np.float64]]:
                    cell = centers / glass.size
                    fraction = cell % 1.0
                    level: ImagingResult[npt.NDArray[np.float64]]
                    match glass.surface:
                        case Surface.BLOCKS:
                            level = Ok(np.prod(1.0 - np.abs(2.0 * fraction - 1.0) ** 4, axis=-1))
                        case Surface.CANVAS:
                            level = Ok(0.5 + 0.5 * np.prod(np.sin(math.tau * cell), axis=-1))
                        case Surface.TINY_LENS:
                            level = Ok(np.sqrt(np.maximum(0.0, 1.0 - 4.0 * np.sum((fraction - 0.5) ** 2, axis=-1))))
                        case Surface.FROSTED:
                            level = sample(
                                Perlin(dimensions=2, size=glass.size, stretch=1.0, octaves=Octaves(), distortion=0.0, cut=Cut(interpolation=Interpolation.LINEAR)),
                                np.column_stack((centers.reshape(-1, 2), np.zeros((width * height, 2)))),
                                seed=glass.seed,
                            ).map(lambda field: field[:, 0].reshape(height, width))
                    return level.map(lambda held: glass.strength * glass.size * slope(softened(held, glass.softness)))

                rain_gradient = trailed(self.raindrops.value) if self.raindrops.enabled and self.raindrops.value.strength != 0.0 else Ok(np.zeros(shape, dtype=np.float64))
                glass_gradient = glazed(self.refraction.value) if self.refraction.enabled else Ok(np.zeros(shape, dtype=np.float64))

                map_field = np.zeros((*shape[:2], 3), dtype=np.float64)
                map_origin, map_span = (left, bottom), (right - left, top - bottom)
                if self.map.enabled:
                    request = self.map.value
                    plane = mapping.default_value(source)
                    map_height, map_width = plane.shape[:2]
                    map_origin, map_span = ((0.0, 0.0), extent) if mapping.is_some() else (map_origin, map_span)
                    map_step = np.array(request.tile.value if request.tile.enabled else map_span) / (map_width, map_height)
                    mode: Literal["wrap", "nearest"] = "wrap" if request.tile.enabled else "nearest"
                    map_field = np.ones((map_height, map_width, 3), dtype=np.float64)
                    match request.reading:
                        case MapReading.GRADIENT:
                            level = gaussian_filter(height_of(plane), (request.softness / map_step[1], request.softness / map_step[0]), mode=mode)
                            map_field[..., :2] = np.stack((sobel(level, axis=1, mode=mode) / (8.0 * map_step[0]), -sobel(level, axis=0, mode=mode) / (8.0 * map_step[1])), axis=-1)
                        case MapReading.LUMA:
                            level = 2.0 * gaussian_filter(height_of(plane), (request.softness / map_step[1], request.softness / map_step[0]), mode=mode) - 1.0
                            map_field[..., :2] = level[..., None]
                        case MapReading.OFFSETS:
                            map_field[..., :2] = 2.0 * plane[..., :2] - 1.0
                        case MapReading.COORDINATES:
                            map_field[..., :2] = plane[..., :2]
                    map_field[..., 2:] = plane[..., 2:3] if request.reading is MapReading.COORDINATES else 1.0

                glass_source = softened(source.astype(np.float64), self.refraction.value.blur).astype(np.float32) if self.refraction.enabled else source
                drop_source = softened(source.astype(np.float64), self.raindrops.value.blur).astype(np.float32) if self.raindrops.enabled else source

                def fronted(wave: Wave, along: npt.NDArray[np.float64], points: npt.NDArray[np.float64]) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64], npt.NDArray[np.float64]]:
                    if wave.front is Front.LINEAR:
                        travel = (points - np.array(extent) / 2.0) @ along
                        return travel, np.broadcast_to((-along[1], along[0]), points.shape), np.ones_like(travel)
                    arm = points - np.array(wave.center) * extent
                    travel = np.linalg.norm(arm, axis=-1)
                    spoke = np.divide(arm, travel[..., None], out=np.zeros_like(arm), where=travel[..., None] > 0.0)
                    tangent = np.stack((-spoke[..., 1], spoke[..., 0]), axis=-1)
                    axis = spoke if wave.front is Front.RADIAL else tangent if wave.front is Front.ZIGZAG else (spoke + tangent) * math.sqrt(0.5)
                    return travel, axis, np.maximum(0.0, 1.0 - travel / wave.radius)

                def crest(wave: Wave, index: int, points: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
                    draw = uniforms((index,), seed=wave.seed, count=3)
                    wavelength = wave.wavelength * (1.0 - wave.wavelength_variation * draw[0])
                    amplitude = wave.amplitude * (1.0 - wave.amplitude_variation * draw[1]) / wave.count
                    direction = wave.direction + wave.spread * (index - (wave.count - 1.0) / 2.0) / max(wave.count - 1, 1)
                    travel, axis, envelope = fronted(wave, np.array((math.cos(direction), math.sin(direction))), points)
                    cycles = travel / wavelength - clock(wave.motion) + wave.phase / math.tau + (draw[2] if wave.count > 1 else 0.0)
                    return sample(
                        noise.Wave(wavelength=1.0, profile=wave.profile, cut=Cut(interpolation=Interpolation.LINEAR)), np.column_stack((cycles.ravel(), np.zeros((cycles.size, 3)))), seed=wave.seed
                    ).map(lambda profile: amplitude * (2.0 * profile[:, 0].reshape(travel.shape) - 1.0)[..., None] * envelope[..., None] * axis * wave.axes)

                def stirred(turbulence: Turbulence, points: npt.NDArray[np.float64]) -> ImagingResult[tuple[npt.NDArray[np.float64], ...]]:
                    coordinates = np.column_stack((points.reshape(-1, 2), np.zeros(points[..., 0].size), np.full(points[..., 0].size, clock(turbulence.motion))))
                    return accumulated(sample(turbulence.basis, coordinates, seed=turbulence.seed, stream=axis) for axis in range(2)).map(
                        lambda axes: (turbulence.amplitude * (2.0 * np.stack([axis[:, 0] for axis in axes], axis=-1).reshape(points.shape) - 1.0) * turbulence.axes,)
                    )

                def dropped(points: npt.NDArray[np.float64], rain_gradient: npt.NDArray[np.float64]) -> ImagingResult[tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]]:
                    return (
                        self.raindrops.value.sampled(points.reshape(-1, 2), reference_frame).map(
                            lambda field: (
                                field[:, 2].reshape(points.shape[:-1]),
                                field[:, :2].reshape(points.shape) + np.stack([tapped(rain_gradient[..., axis], points) for axis in range(2)], axis=-1),
                            )
                        )
                        if self.raindrops.enabled
                        else Ok((np.zeros(points.shape[:-1], dtype=np.float64), np.zeros_like(points)))
                    )

                def composed(
                    points: npt.NDArray[np.float64], terms: tuple[npt.NDArray[np.float64], ...], glass_gradient: npt.NDArray[np.float64]
                ) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
                    displacement = np.zeros_like(points)
                    coverage = np.ones(points.shape[:-1], dtype=np.float64)
                    if self.twirl.enabled:
                        twirl = self.twirl.value
                        arm = points - np.array(twirl.center) * extent
                        reach = np.linalg.norm(arm, axis=-1) / twirl.radius
                        weight = np.clip(1.0 - reach ** (1.0 / twirl.falloff), 0.0, 1.0)
                        weight = weight * weight * (3.0 - 2.0 * weight)
                        weight *= 0.5 + 0.5 * np.cos(twirl.arms * (np.arctan2(arm[..., 1], arm[..., 0]) - twirl.arm_offset))
                        turn = -twirl.angle * weight
                        sine, cosine = np.sin(turn), np.cos(turn)
                        displacement += arm - np.stack((arm[..., 0] * cosine - arm[..., 1] * sine, arm[..., 0] * sine + arm[..., 1] * cosine), axis=-1)
                    if self.sphere.enabled:
                        sphere = self.sphere.value
                        arm = (points - np.array(sphere.center) * extent) * sphere.axes
                        radial = np.linalg.norm(arm, axis=-1) / sphere.radius
                        inside = radial < 1.0
                        bounded = radial[inside]
                        target = 2.0 * np.arcsin(bounded) / math.pi if sphere.amount >= 0.0 else np.sin(math.pi * bounded / 2.0)
                        change = np.divide(target - bounded, bounded, out=np.zeros_like(bounded), where=bounded > 0.0)
                        displacement[inside] -= abs(sphere.amount) * change[..., None] * arm[inside]
                    for term in terms:
                        displacement += term
                    if self.luma.enabled:
                        luma = self.luma.value
                        displacement += luma.amplitude * (2.0 * tapped(luma_height, points) - 1.0)[..., None] * np.array((math.cos(luma.direction), math.sin(luma.direction)))
                    if self.bubbles.enabled:
                        bubbles = self.bubbles.value
                        displacement += bubbles.amplitude * np.stack([tapped(bubble_height, points - axis) - tapped(bubble_height, points + axis) for axis in np.eye(2) * bubbles.softness], axis=-1)
                    if self.refraction.enabled:
                        displacement += np.stack([tapped(glass_gradient[..., axis], points) for axis in range(2)], axis=-1)
                    if self.map.enabled:
                        request = self.map.value
                        local = rotated(points, -request.rotation)
                        fraction = local / request.tile.value % 1.0 if request.tile.enabled else (local - map_origin) / map_span
                        at = np.stack(((1.0 - fraction[..., 1]) * map_field.shape[0] - 0.5, fraction[..., 0] * map_field.shape[1] - 0.5))
                        field = np.stack([map_coordinates(map_field[..., axis], at, order=1, mode="grid-wrap" if request.tile.enabled else "nearest", prefilter=False) for axis in range(3)], axis=-1)
                        if request.reading is MapReading.COORDINATES:
                            displacement += points - rotated(field[..., :2] * extent, request.rotation)
                            coverage *= field[..., 2]
                        else:
                            displacement += rotated(field[..., :2] * request.axes * extent, request.rotation)
                    return displacement, coverage

                def refracted(plane: npt.NDArray[np.float32], position: npt.NDArray[np.float64], coverage: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float32]]:
                    coordinates = np.stack(((position[..., 0] - left) / (right - left), (position[..., 1] - bottom) / (top - bottom), coverage), axis=-1)
                    return StMap.bake(width, height, lambda _fraction: coordinates, self.extension, source_size=(width, height)).warp(plane, filtername=self.filtername, filterwidth=self.filterwidth)

                def observed(points: npt.NDArray[np.float64], gradients: tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]) -> ImagingResult[npt.NDArray[np.float64]]:
                    rain_gradient, glass_gradient = gradients
                    waves = accumulated(crest(self.wave.value, index, points) for index in range(self.wave.value.count)) if self.wave.enabled else Ok(())
                    turbulence = stirred(self.turbulence.value, points) if self.turbulence.enabled else Ok(())

                    def colored(
                        fields: tuple[tuple[tuple[npt.NDArray[np.float64], ...], tuple[npt.NDArray[np.float64], ...]], tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]],
                    ) -> ImagingResult[npt.NDArray[np.float64]]:
                        (crests, stirs), (drops, refraction) = fields
                        displacement, coverage = composed(points, (*crests, *stirs), glass_gradient)
                        return paired(refracted(glass_source, points - displacement, coverage), refracted(drop_source, points - displacement - refraction, coverage)).map(
                            lambda colors: np.concatenate(
                                (colors[0] * (1.0 - drops[..., None]) + colors[1] * drops[..., None], displacement + drops[..., None] * refraction, drops[..., None]), axis=-1
                            )
                        )

                    return paired(paired(waves, turbulence), dropped(points, rain_gradient)).bind(colored)

                def replicated(observation: npt.NDArray[np.float64], replicate: int, gradients: tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]) -> ImagingResult[npt.NDArray[np.float64]]:
                    net = precision.points(2, replicate)
                    return functools.reduce(
                        lambda total, offset: total.bind(lambda held: observed(centers + (offset - 0.5) * step * (1.0, -1.0), gradients).map(lambda term: np.add(held, term / len(net), out=held))),
                        net,
                        Ok(observation),
                    )

                def located(plane: npt.NDArray[np.float64]) -> ImagingResult[OpenImageIO.ImageBuf]:
                    answer = OpenImageIO.ImageBuf(np.ascontiguousarray(plane, dtype=np.float32))
                    answer.copy_metadata(image)
                    answer.set_origin(image.xbegin, image.ybegin, image.zbegin)
                    answer.roi_full = image.roi_full
                    return Error((ImagingBufferFault(request=image, error=answer.geterror()),)) if answer.has_error else Ok(answer)

                def published(observations: npt.NDArray[np.float64]) -> ImagingResult[Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]]:
                    estimate = estimated(observations, precision)
                    return accumulated(located(plane) for plane in (estimate.value[..., :channels], estimate.value[..., channels : channels + 2], estimate.value[..., channels + 2 :])).map(
                        lambda buffers: Estimate(value=(buffers[0], buffers[1], buffers[2]), standard_error=estimate.standard_error, precision=precision, replicates=estimate.replicates)
                    )

                observations = np.zeros((precision.replicates, height, width, channels + 3), dtype=np.float64)
                return (
                    paired(rain_gradient, glass_gradient)
                    .bind(lambda gradients: accumulated(replicated(observation, replicate, gradients) for replicate, observation in enumerate(observations)))
                    .bind(lambda _: published(observations))
                )
        except (FloatingPointError, MemoryError, OverflowError, ValueError) as error:
            return Error((ImagingGeneratorFault(request=self, error=error),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bubbles", "Displacement", "Front", "Luma", "Map", "MapReading", "Raindrops", "Refraction", "Sphere", "Surface", "Turbulence", "Twirl", "Wave"]
