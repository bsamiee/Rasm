"""Projected film marks, gate motion, aperture, and scene-linear lamp flicker."""

from enum import IntEnum
from fractions import Fraction
from math import floor
from sys import float_info
from typing import Annotated

from expression import Error, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO

from blender.imaging.chain import Effect, Stage
from blender.imaging.effects.generators.noise import (
    cellular,
    cellular_sites,
    CellularSite,
    Cut,
    Estimate,
    estimated,
    Feature,
    Fractal,
    integrated,
    Interpolation,
    Octaves,
    Perlin,
    Precision,
    sample,
    Voronoi,
)
from blender.imaging.effects.generators.particles import Population
from blender.imaging.faults import accumulated, ImagingBufferFault, ImagingGeneratorFault, ImagingRepresentationFault, ImagingResult, paired
from blender.imaging.parameters import Finite, Gated, Motion, Presentation, Quantity, Rgb, Seed, Stops

# --- [TYPES] ----------------------------------------------------------------------------

type _Nonnegative = Annotated[float, msgspec.Meta(ge=0.0, le=float_info.max)]


class Noise(IntEnum):
    """Persistent independent origins of projected film fields."""

    SPECKLES = 0
    SCRATCHES = 1
    BEND = 2
    HAIRS = 3
    STRANDS = 4
    SHIFT_X = 5
    SHIFT_Y = 6
    EDGE = 7


# --- [MODELS] ---------------------------------------------------------------------------


class Speckles(msgspec.Struct, frozen=True, kw_only=True):
    """Independent opaque or translucent black and white film spots."""

    population: Annotated[Population, msgspec.Meta(title="Population", description="Spot cells, diameter, spread, and radial softness")] = Population(
        spacing=0.02, density=0.02, size=0.2, variation=1.0, spread=0.0, softness=0.3
    )
    white: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="White", description="Fraction of spots that are white")] = 0.5


class Scratches(msgspec.Struct, frozen=True, kw_only=True):
    """Vertical film scratches with fixed horizontal sites and evolving presence."""

    count: Annotated[float, msgspec.Meta(ge=1.0, le=float_info.max, title="Count", description="Scratch cells across the frame width"), Presentation(soft=Some((1.0, 20.0)))] = 3.0
    width: Annotated[_Nonnegative, msgspec.Meta(title="Width", description="Width of a scratch"), Presentation(soft=Some((0.0, 0.005)), decimals=Some(4), quantity=Some(Quantity.FRAME_GREATER))] = (
        0.0008
    )
    white: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="White", description="Fraction of scratches that are white")] = 0.7
    rate: Annotated[_Nonnegative, msgspec.Meta(title="Rate", description="Presence evolution in cells per motion frame"), Presentation(soft=Some((0.0, 1.0)))] = 0.05

    def sampled(self, points: npt.NDArray[np.float64], extent: tuple[float, float], clock: float, *, seed: Seed) -> ImagingResult[npt.NDArray[np.float64]]:
        """Returns continuous nearest-owned scratch coverage and covered white ink.

        Args:
            points: Displayed greater-side XY coordinates.
            extent: Displayed frame width and height in the same units.
            clock: Held film clock.
            seed: Shot's explicit random origin.

        Returns:
            Coverage and covered ink columns, or the numerical provider's refusal.
        """
        scale = self.count / extent[0]
        at = np.zeros((len(points), 4), dtype=np.float64)
        at[:, 0], at[:, 1] = points[:, 0] * scale, clock * self.rate

        def composed(delta: npt.NDArray[np.float64], color: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            coverage = (np.abs(delta[:, 0] / scale) < self.width / 2.0).astype(np.float64)
            return np.column_stack((coverage, coverage * (color[:, 1] < self.white)))

        return cellular(Voronoi(dimensions=2, spacing=1.0), at, seed=seed, stream=Noise.SCRATCHES).map(lambda field: composed(field.delta, field.color))

    def integrated(self, interval: tuple[float, float], extent: tuple[float, float], clock: float, *, seed: Seed) -> ImagingResult[npt.NDArray[np.float64]]:
        """Integrates every scratch stripe and nearest-cell ownership crossing exactly.

        Args:
            interval: Positive displayed horizontal pixel interval.
            extent: Displayed frame width and height.
            clock: Held film clock.
            seed: Shot's explicit random origin.

        Returns:
            Mean coverage and covered white ink, or a numerical representation refusal.
        """
        try:
            scale = Fraction(self.count) / Fraction(extent[0])
            left, right = (Fraction(value) * scale for value in interval)
            y = Fraction(clock) * Fraction(self.rate)
            radius = Fraction(self.width) * scale / 2

            def owned(site: CellularSite, candidates: tuple[CellularSite, ...], first: Fraction, last: Fraction) -> Fraction:
                x, height = site.point
                lower, upper = max(first, x - radius), min(last, x + radius)
                for other in candidates:
                    ox, oy = other.point
                    coefficient = 2 * (ox - x)
                    bound = ox**2 + oy**2 - x**2 - height**2 - 2 * y * (oy - height)
                    if coefficient > 0:
                        upper = min(upper, bound / coefficient)
                    elif coefficient < 0:
                        lower = max(lower, bound / coefficient)
                    elif bound < 0 or (bound == 0 and other.cell < site.cell):
                        return Fraction(0)
                return max(Fraction(0), upper - lower)

            def composed(sites: tuple[CellularSite, ...]) -> npt.NDArray[np.float64]:
                def column(index: int) -> npt.NDArray[np.float64]:
                    candidates = tuple(site for site in sites if abs(site.cell[0] - index) <= 1 and abs(site.cell[1] - floor(y)) <= 1)
                    first, last = max(left, Fraction(index)), min(right, Fraction(index + 1))
                    return sum(
                        (float(owned(site, candidates, first, last) / (right - left)) * np.array((1.0, float(site.color[1] < self.white))) for site in candidates), np.zeros(2, dtype=np.float64)
                    )

                return sum((column(index) for index in range(floor(left), floor(right) + 1)), np.zeros(2, dtype=np.float64))

            return cellular_sites(Voronoi(dimensions=2, spacing=1.0), ((left, y), (right, y)), seed=seed, stream=Noise.SCRATCHES).map(composed)
        except (FloatingPointError, MemoryError, OverflowError, ValueError) as error:
            return Error((ImagingGeneratorFault(request=self, error=error),))


class Hairs(msgspec.Struct, frozen=True, kw_only=True):
    """Curled, trimmed cell edges selected as hair strands on the film."""

    count: Annotated[_Nonnegative, msgspec.Meta(title="Count", description="Nominal strand density through cell selection"), Presentation(soft=Some((0.0, 20.0)))] = 1.0
    width: Annotated[_Nonnegative, msgspec.Meta(title="Width", description="Width of a hair"), Presentation(soft=Some((0.0, 0.005)), decimals=Some(4), quantity=Some(Quantity.FRAME_GREATER))] = 0.0012
    white: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="White", description="Fraction of hairs that are white")] = 0.0
    length: Annotated[
        float,
        msgspec.Meta(gt=0.0, le=float_info.max, title="Length", description="Scale of the cells that form strands"),
        Presentation(soft=Some((0.001, 0.2)), decimals=Some(3), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 0.08
    curl: Annotated[_Nonnegative, msgspec.Meta(title="Curl", description="Coordinate displacement as a multiple of strand length"), Presentation(soft=Some((0.0, 5.0)))] = 1.0
    rate: Annotated[_Nonnegative, msgspec.Meta(title="Rate", description="Strand evolution in cells per motion frame"), Presentation(soft=Some((0.0, 1.0)))] = 0.1

    def sampled(self, points: npt.NDArray[np.float64], extent: tuple[float, float], clock: float, *, seed: Seed) -> ImagingResult[npt.NDArray[np.float64]]:
        """Returns warped, selected, trimmed hair coverage and covered white ink.

        Args:
            points: Displayed greater-side XY coordinates.
            extent: Displayed frame width and height in the same units.
            clock: Held film clock.
            seed: Shot's explicit random origin.

        Returns:
            Coverage and covered ink columns, or accumulated native field refusals.
        """
        at = np.zeros((len(points), 4), dtype=np.float64)
        at[:, :2] = points
        basis = Perlin(dimensions=2, size=self.length, stretch=1.0, octaves=Octaves(), distortion=0.0, cut=Cut(interpolation=Interpolation.LINEAR))
        bend = accumulated(sample(basis, at, seed=seed, stream=Noise.BEND + axis * len(Noise)) for axis in range(2))
        strands = sample(basis, at, seed=seed, stream=Noise.STRANDS)

        def bent(fields: tuple[npt.NDArray[np.float64], ...]) -> ImagingResult[npt.NDArray[np.float64]]:
            warped = at.copy()
            warped[:, :2] += self.curl * self.length * (2.0 * np.column_stack(tuple(field[:, 0] for field in fields)) - 1.0)
            warped[:, 3] = clock * self.rate
            return cellular(Voronoi(dimensions=3, spacing=self.length, feature=Feature.DISTANCE_TO_EDGE), warped, seed=seed, stream=Noise.HAIRS).bind(
                lambda edge: strands.map(lambda trim: composed(edge.value, edge.color, trim[:, 0]))
            )

        def composed(distance: npt.NDArray[np.float64], color: npt.NDArray[np.float64], trim: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            opening = np.clip((trim - 0.45) / (0.55 - 0.45), 0.0, 1.0)
            coverage = (distance * self.length < self.width / 2.0) * (color[:, 0] < self.count * self.length**2 / (extent[0] * extent[1])) * opening**2 * (3.0 - 2.0 * opening)
            return np.column_stack((coverage, coverage * (color[:, 1] < self.white)))

        return bend.bind(bent)


class Weave(msgspec.Struct, frozen=True, kw_only=True):
    """Horizontal and vertical film displacement through the projector gate."""

    amplitude: Annotated[
        tuple[_Nonnegative, _Nonnegative],
        msgspec.Meta(title="Amplitude", description="Maximum horizontal and vertical displacement"),
        Presentation(soft=Some((0.0, 0.005)), decimals=Some(4), quantity=Some(Quantity.FRAME_GREATER)),
    ] = (0.0006, 0.0006)
    rate: Annotated[_Nonnegative, msgspec.Meta(title="Rate", description="Displacement evolution in cells per motion frame"), Presentation(soft=Some((0.0, 1.0)))] = 0.125

    def sampled(self, clock: float, *, seed: Seed) -> ImagingResult[npt.NDArray[np.float64]]:
        """Returns independently seeded displayed displacement of the film.

        Args:
            clock: Held film clock.
            seed: Shot's explicit random origin.

        Returns:
            Horizontal and vertical offsets, or accumulated native field refusals.
        """
        at = np.array(((clock * self.rate, 0.0, 0.0, 0.0),), dtype=np.float64)
        basis = Perlin(dimensions=1, size=1.0, stretch=1.0, octaves=Octaves(), distortion=0.0, cut=Cut(interpolation=Interpolation.LINEAR))
        return accumulated(sample(basis, at, seed=seed, stream=stream) for stream in (Noise.SHIFT_X, Noise.SHIFT_Y)).map(
            lambda fields: np.asarray(self.amplitude) * (2.0 * np.array(tuple(field[0, 0] for field in fields)) - 1.0)
        )


class Gate(msgspec.Struct, frozen=True, kw_only=True):
    """Stationary rounded projector aperture with a rough, feathered edge."""

    inset: Annotated[
        _Nonnegative, msgspec.Meta(title="Inset", description="Aperture inset from each frame edge"), Presentation(soft=Some((0.0, 0.1)), decimals=Some(3), quantity=Some(Quantity.FRAME_GREATER))
    ] = 0.02
    radius: Annotated[
        _Nonnegative,
        msgspec.Meta(title="Radius", description="Corner radius within the remaining aperture"),
        Presentation(soft=Some((0.0, 0.1)), decimals=Some(3), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 0.02
    feather: Annotated[
        _Nonnegative, msgspec.Meta(title="Feather", description="Transition width at the aperture edge"), Presentation(soft=Some((0.0, 0.02)), decimals=Some(4), quantity=Some(Quantity.FRAME_GREATER))
    ] = 0.005
    roughness: Annotated[
        _Nonnegative,
        msgspec.Meta(title="Roughness", description="Maximum displacement of the aperture edge"),
        Presentation(soft=Some((0.0, 0.02)), decimals=Some(4), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 0.003
    color: Annotated[Rgb[Finite], msgspec.Meta(title="Color", description="Display color outside the aperture")] = Rgb(red=0.0, green=0.0, blue=0.0)

    def sampled(self, points: npt.NDArray[np.float64], extent: tuple[float, float], *, seed: Seed) -> ImagingResult[npt.NDArray[np.float64]]:
        """Returns continuous rough rounded-aperture coverage before pixel integration.

        Args:
            points: Displayed greater-side XY coordinates.
            extent: Displayed frame width and height in the same units.
            seed: Shot's explicit random origin.

        Returns:
            Aperture coverage, or the native edge field's numerical refusal.
        """
        center = np.asarray(extent) / 2.0
        remaining = center - self.inset
        if (shorter := float(np.min(remaining))) <= 0.0:
            return Ok(np.zeros(len(points), dtype=np.float64))
        radius = min(self.radius, shorter)
        excess = np.abs(points - center) - (remaining - radius)
        distance = np.linalg.norm(np.maximum(excess, 0.0), axis=1) + np.minimum(np.max(excess, axis=1), 0.0) - radius
        at = np.zeros((len(points), 4), dtype=np.float64)
        at[:, :2] = points
        edge = Perlin(
            dimensions=4,
            size=1.0 / 2.5,
            stretch=1.0,
            fractal=Fractal.RIDGED_MULTIFRACTAL,
            octaves=Octaves(detail=15.0, roughness=0.9),
            offset=1.0,
            gain=74.5,
            distortion=1.9,
            cut=Cut(interpolation=Interpolation.LINEAR),
        )

        def coverage(distance: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            return np.clip(0.5 - distance / self.feather, 0.0, 1.0) if self.feather > 0.0 else (distance < 0.0).astype(np.float64)

        return sample(edge, at, seed=seed, stream=Noise.EDGE).map(lambda field: coverage(distance + self.roughness * (2.0 * field[:, 0] - 1.0)))


class Damage(Effect, frozen=True, kw_only=True, tag=44):
    """Film marks and weave behind a stationary projector aperture."""

    stage = Stage.DAMAGE
    title = "Damage"
    pointwise = False
    motion: Motion = Motion()
    seed: Seed = 0
    speckles: Gated[Speckles] = Gated(enabled=True, value=Speckles())
    scratches: Gated[Scratches] = Gated(enabled=True, value=Scratches())
    hairs: Gated[Hairs] = Gated(enabled=False, value=Hairs())
    weave: Gated[Weave] = Gated(enabled=True, value=Weave())
    gate: Gated[Gate] = Gated(enabled=False, value=Gate())

    def filtered(self, bounds: tuple[float, float, float, float], extent: tuple[float, float], clock: float, *, precision: Precision) -> ImagingResult[Estimate[npt.NDArray[np.float64]]]:
        """Returns footprint-filtered mark pairs and stationary gate with joint replicates.

        Args:
            bounds: Positive global pixel footprint as left, bottom, right, top.
            extent: Original displayed camera-frame width and height.
            clock: Held film clock.
            precision: Caller-selected Sobol resolution and independent error replicates.

        Returns:
            Speck, scratch, hair coverage/ink pairs and gate coverage, with numerical error.
        """
        try:
            with np.errstate(over="raise", divide="raise", invalid="raise"):
                left, bottom, right, top = bounds
                area = (right - left) * (top - bottom)
                rectangle = ((left, bottom), (right, top))
                empty = np.zeros((precision.replicates, 2), dtype=np.float64)
                spots = (
                    self.speckles.value.population.integrated(
                        bounds, seed=self.seed, precision=precision, stream=floor(clock) * len(Noise) + Noise.SPECKLES, paint=lambda tone: float(tone < self.speckles.value.white)
                    ).map(lambda estimate: estimate.replicates)
                    if self.speckles.enabled
                    else Ok(empty)
                )
                scratches = (
                    self.scratches.value.integrated((left, right), extent, clock, seed=self.seed).map(lambda value: np.broadcast_to(value, empty.shape)) if self.scratches.enabled else Ok(empty)
                )
                hairs = (
                    integrated(self.hairs.value, lambda points: self.hairs.value.sampled(points, extent, clock, seed=self.seed), rectangle, precision=precision).map(
                        lambda estimate: estimate.replicates / area
                    )
                    if self.hairs.enabled
                    else Ok(empty)
                )
                gate = (
                    integrated(self.gate.value, lambda points: self.gate.value.sampled(points, extent, seed=self.seed), rectangle, precision=precision).map(
                        lambda estimate: estimate.replicates[:, None] / area
                    )
                    if self.gate.enabled
                    else Ok(np.ones((precision.replicates, 1), dtype=np.float64))
                )
                return accumulated((spots, scratches, hairs, gate)).map(lambda values: estimated(np.concatenate(values, axis=1), precision))
        except (FloatingPointError, MemoryError, OverflowError, ValueError) as error:
            return Error((ImagingGeneratorFault(request=self, error=error),))

    def applied(
        self, source: OpenImageIO.ImageBuf, bounds: tuple[float, float, float, float], extent: tuple[float, float], clock: float, *, precision: Precision
    ) -> ImagingResult[Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]]:
        """Processes associated film and its damage key with joint numerical error.

        Args:
            source: Native associated RGBA, with top-first pixel rows and retained windows.
            bounds: Displayed bottom-left DATA window in original camera units.
            extent: Original displayed camera-frame width and height.
            clock: Held film clock.
            precision: Caller-selected integration resolution and independent replicates.

        Returns:
            Native result RGBA and scalar damage key, with joint replicate error or provider faults.
        """

        def buffered(image: OpenImageIO.ImageBuf) -> ImagingResult[OpenImageIO.ImageBuf]:
            return Error((ImagingBufferFault(request=image, error=image.geterror()),)) if image.has_error else Ok(image)

        def located(values: npt.NDArray[np.float32]) -> ImagingResult[OpenImageIO.ImageBuf]:
            image = OpenImageIO.ImageBuf(values)
            image.copy_metadata(source)
            image.set_origin(source.xbegin, source.ybegin, source.zbegin)
            image.roi_full = source.roi_full
            return buffered(image)

        def pixels(image: OpenImageIO.ImageBuf) -> ImagingResult[npt.NDArray[np.float32]]:
            return Option.of_optional(image.get_pixels(OpenImageIO.FLOAT)).to_result_with(lambda: (ImagingBufferFault(request=image, error=image.geterror()),))

        def processed(plane: npt.NDArray[np.float32]) -> ImagingResult[Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]]:
            left, bottom, right, top = bounds
            height, width = plane.shape[:2]
            step_x, step_y = (right - left) / width, (top - bottom) / height
            fields = accumulated(
                self.filtered((left + column * step_x, top - (row + 1) * step_y, left + (column + 1) * step_x, top - row * step_y), extent, clock, precision=precision)
                for row in range(height)
                for column in range(width)
            ).map(lambda estimates: np.stack(tuple(estimate.replicates for estimate in estimates), axis=1).reshape((precision.replicates, height, width, 7)))
            displacement = self.weave.value.sampled(clock, seed=self.seed) if self.weave.enabled else Ok(np.zeros(2, dtype=np.float64))

            def composed(values: tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]) -> ImagingResult[Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]]:
                fields, shift = values
                copies = np.broadcast_to(plane, (precision.replicates, *plane.shape)).astype(np.float64)
                mask = np.zeros((*copies.shape[:-1], 1), dtype=np.float64)
                for section in range(3):
                    coverage, ink = fields[..., 2 * section : 2 * section + 1], fields[..., 2 * section + 1 : 2 * section + 2]
                    copies[..., :3] = copies[..., :3] * (1.0 - coverage) + copies[..., 3:] * ink
                    mask = 1.0 - (1.0 - mask) * (1.0 - coverage)
                marked = np.concatenate((copies, mask), axis=-1)
                transform = (1.0, 0.0, 0.0, 0.0, 1.0, 0.0, float(shift[0] / step_x), float(-shift[1] / step_y), 1.0)
                moved = (
                    accumulated(
                        located(replicate.astype(np.float32)).bind(lambda image: buffered(OpenImageIO.ImageBufAlgo.warp(image, transform, "triangle", 2.0, False, "clamp"))).bind(pixels)
                        for replicate in marked
                    )
                    if self.weave.enabled
                    else Ok(tuple(replicate.astype(np.float32) for replicate in marked))
                )

                def gated(replicates: tuple[npt.NDArray[np.float32], ...]) -> ImagingResult[Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]]:
                    result = np.stack(replicates).astype(np.float64)
                    gate = fields[..., 6:]
                    color = np.array((self.gate.value.color.red, self.gate.value.color.green, self.gate.value.color.blue))
                    result[..., :3] = result[..., :3] * gate + color * result[..., 3:4] * (1.0 - gate)
                    result[..., 4:] = 1.0 - (1.0 - result[..., 4:]) * gate
                    estimate = estimated(result, precision)
                    return paired(located(estimate.value[..., :4].astype(np.float32)), located(estimate.value[..., 4:].astype(np.float32))).map(
                        lambda images: Estimate(value=images, standard_error=estimate.standard_error, precision=precision, replicates=result)
                    )

                return moved.bind(gated)

            return paired(fields, displacement).bind(composed)

        try:
            with np.errstate(over="raise", divide="raise", invalid="raise"):
                return pixels(source).bind(processed)
        except (FloatingPointError, ValueError) as error:
            return Error((ImagingGeneratorFault(request=self, error=error),))
        except (TypeError, OverflowError, MemoryError) as error:
            return Error((ImagingRepresentationFault(request=self, error=error),))


class Flicker(Effect, frozen=True, kw_only=True, tag=47):
    """Lamp variation as a spatially uniform scene-linear exposure in stops."""

    stage = Stage.EXPOSURE
    title = "Flicker"
    pointwise = True
    motion: Motion = Motion()
    seed: Seed = 0
    amplitude: Annotated[Stops, msgspec.Meta(title="Amplitude", description="Signed maximum exposure variation in stops")] = 0.05
    rate: Annotated[_Nonnegative, msgspec.Meta(title="Rate", description="Lamp evolution in cells per motion frame"), Presentation(soft=Some((0.0, 10.0)))] = 2.5


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Damage", "Flicker", "Gate", "Hairs", "Noise", "Scratches", "Speckles", "Weave"]
