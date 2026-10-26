"""Projected film marks, gate motion, aperture, and scene-linear lamp flicker."""

from collections.abc import Callable
from enum import IntEnum
import math
from typing import Annotated

from expression import Ok, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO

from blender.imaging.chain import Effect, Stage
from blender.imaging.effects.generators.noise import cellular, Feature, Perlin, RIDGED, sample, Voronoi
from blender.imaging.effects.generators.particles import Population
from blender.imaging.faults import accumulated, guarded, ImagingBufferFault, ImagingNumericFault, ImagingResult, paired
from blender.imaging.hashing import hashed
from blender.imaging.parameters import AtLeastOne, Finite, FrameExtent, FrameLength, Gated, Motion, NonNegative, Presentation, Quantity, Rgb, Seed, Stops, Unit
from blender.imaging.sampling import Estimate, Precision, smoothstep

# --- [TYPES] ----------------------------------------------------------------------------


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

    population: Annotated[Population, msgspec.Meta(description="Spot placement and size")] = Population(spacing=0.02, density=0.02, variation=1.0, spread=0.0, softness=0.3)
    white: Annotated[Unit, msgspec.Meta(description="Share of white spots")] = 0.5


class Scratches(msgspec.Struct, frozen=True, kw_only=True):
    """Vertical film scratches with fixed horizontal sites and evolving presence."""

    count: Annotated[AtLeastOne, msgspec.Meta(description="Scratch cells across frame width"), Presentation(soft=Some((1.0, 20.0)))] = 3.0
    width: Annotated[FrameLength, msgspec.Meta(description="Scratch width"), Presentation(soft=Some((0.0, 0.005)), precision=Some(4))] = 0.0008
    white: Annotated[Unit, msgspec.Meta(description="Share of white scratches")] = 0.7
    rate: Annotated[NonNegative, msgspec.Meta(description="Presence evolution in cells per motion frame"), Presentation(soft=Some((0.0, 1.0)))] = 0.05

    def sampled(self, points: npt.NDArray[np.float64], extent: tuple[float, float], clock: float, *, seed: Seed) -> ImagingResult[npt.NDArray[np.float64]]:
        """Returns continuous nearest-owned scratch coverage and covered white ink.

        Args:
            points: Displayed greater-side XY coordinates.
            extent: Displayed frame width and height in the same units.
            clock: Held film clock.
            seed: Shot's explicit random origin.

        Returns:
            Coverage and covered ink columns, or numerical provider refusal.
        """
        scale = self.count / extent[0]
        at = np.zeros((len(points), 4), dtype=np.float64)
        at[:, 0], at[:, 1] = points[:, 0] * scale, clock * self.rate

        def composed(delta: npt.NDArray[np.float64], color: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            coverage = (np.abs(delta[:, 0] / scale) < self.width / 2.0).astype(np.float64)
            return np.column_stack((coverage, coverage * (color[:, 1] < self.white)))

        return cellular(Voronoi(dimensions=2, spacing=1.0), at, seed=seed, stream=Noise.SCRATCHES).map(lambda field: composed(field.delta, field.color))

    def integrated(self, interval: tuple[float, float], extent: tuple[float, float], clock: float, *, seed: Seed) -> ImagingResult[npt.NDArray[np.float64]]:
        """Integrates every scratch stripe over its nearest-cell ownership at the held clock height.

        Args:
            interval: Positive displayed horizontal pixel interval.
            extent: Displayed frame width and height.
            clock: Held film clock.
            seed: Shot's explicit random origin.

        Returns:
            Mean coverage and covered white ink, or a numeric refusal.
        """

        def computed() -> ImagingResult[npt.NDArray[np.float64]]:
            scale = self.count / extent[0]
            left, right = (value * scale for value in interval)
            height, radius = clock * self.rate, self.width * scale / 2.0

            def column(index: int) -> npt.NDArray[np.float64]:
                cells = np.stack(np.meshgrid(np.arange(index - 1, index + 2), np.arange(math.floor(height) - 1, math.floor(height) + 2), indexing="ij"), axis=-1).reshape(-1, 2)
                draws = hashed(cells, seed=seed, stream=Noise.SCRATCHES, count=4)
                sites = cells + draws[:, :2]
                (x, y), (other_x, other_y) = sites.T[:, :, None], sites.T[:, None, :]
                coefficient = 2.0 * (other_x - x)
                bound = other_x**2 + other_y**2 - x**2 - y**2 - 2.0 * height * (other_y - y)
                cut = bound / np.where(coefficient == 0.0, 1.0, coefficient)
                upper = np.minimum.reduce((np.full(len(cells), min(right, index + 1.0)), sites[:, 0] + radius, np.where(coefficient > 0.0, cut, np.inf).min(axis=1)))
                lower = np.maximum.reduce((np.full(len(cells), max(left, float(index))), sites[:, 0] - radius, np.where(coefficient < 0.0, cut, -np.inf).max(axis=1)))
                earlier = (cells[None, :, 0] < cells[:, None, 0]) | ((cells[None, :, 0] == cells[:, None, 0]) & (cells[None, :, 1] < cells[:, None, 1]))
                excluded = ((coefficient == 0.0) & ((bound < 0.0) | ((bound == 0.0) & earlier))).any(axis=1)
                owned = np.where(excluded, 0.0, np.maximum(0.0, upper - lower)) / (right - left)
                return owned @ np.column_stack((np.ones(len(cells)), draws[:, 1] < self.white))

            return Ok(sum((column(index) for index in range(math.floor(left), math.floor(right) + 1)), np.zeros(2, dtype=np.float64)))

        return guarded(ImagingNumericFault, self, computed, frozenset())


class Hairs(msgspec.Struct, frozen=True, kw_only=True):
    """Curled, trimmed cell edges drawn as film hairs."""

    count: Annotated[NonNegative, msgspec.Meta(description="Nominal strand density"), Presentation(soft=Some((0.0, 20.0)))] = 1.0
    width: Annotated[FrameLength, msgspec.Meta(description="Hair width"), Presentation(soft=Some((0.0, 0.005)), precision=Some(4))] = 0.0012
    white: Annotated[Unit, msgspec.Meta(description="Share of white hairs")] = 0.0
    length: Annotated[FrameExtent, msgspec.Meta(description="Strand cell scale"), Presentation(soft=Some((0.001, 0.2)), precision=Some(3))] = 0.08
    curl: Annotated[NonNegative, msgspec.Meta(description="Curl displacement in strand lengths"), Presentation(soft=Some((0.0, 5.0)))] = 1.0
    rate: Annotated[NonNegative, msgspec.Meta(description="Strand evolution in cells per motion frame"), Presentation(soft=Some((0.0, 1.0)))] = 0.1

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
        basis = Perlin(dimensions=2, size=self.length)
        bend = accumulated(sample(basis, at, seed=seed, stream=Noise.BEND + axis * len(Noise)) for axis in range(2))
        strands = sample(basis, at, seed=seed, stream=Noise.STRANDS)

        def bent(fields: tuple[npt.NDArray[np.float64], ...]) -> ImagingResult[npt.NDArray[np.float64]]:
            warped = at.copy()
            warped[:, :2] += self.curl * self.length * (2.0 * np.column_stack(tuple(field[:, 0] for field in fields)) - 1.0)
            warped[:, 3] = clock * self.rate
            return cellular(Voronoi(dimensions=3, spacing=self.length, feature=Feature.DISTANCE_TO_EDGE), warped, seed=seed, stream=Noise.HAIRS).map2(
                strands, lambda edge, trim: composed(edge.value, edge.color, trim[:, 0])
            )

        def composed(distance: npt.NDArray[np.float64], color: npt.NDArray[np.float64], trim: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            opening = np.clip((trim - 0.45) / (0.55 - 0.45), 0.0, 1.0)
            coverage = (distance * self.length < self.width / 2.0) * (color[:, 0] < self.count * self.length**2 / (extent[0] * extent[1])) * smoothstep(opening)
            return np.column_stack((coverage, coverage * (color[:, 1] < self.white)))

        return bend.bind(bent)


class Weave(msgspec.Struct, frozen=True, kw_only=True):
    """Film displacement through projector gate."""

    amplitude: Annotated[
        tuple[NonNegative, NonNegative], msgspec.Meta(description="Largest displacement per axis"), Presentation(soft=Some((0.0, 0.005)), precision=Some(4), quantity=Some(Quantity.FRAME))
    ] = (0.0006, 0.0006)
    rate: Annotated[NonNegative, msgspec.Meta(description="Displacement evolution in cells per motion frame"), Presentation(soft=Some((0.0, 1.0)))] = 0.125

    def sampled(self, clock: float, *, seed: Seed) -> ImagingResult[npt.NDArray[np.float64]]:
        """Returns independently seeded displayed displacement of the film.

        Args:
            clock: Held film clock.
            seed: Shot's explicit random origin.

        Returns:
            Horizontal and vertical offsets, or accumulated native field refusals.
        """
        at = np.array(((clock * self.rate, 0.0, 0.0, 0.0),), dtype=np.float64)
        basis = Perlin(dimensions=1)
        return accumulated(sample(basis, at, seed=seed, stream=stream) for stream in (Noise.SHIFT_X, Noise.SHIFT_Y)).map(
            lambda fields: np.asarray(self.amplitude) * (2.0 * np.array(tuple(field[0, 0] for field in fields)) - 1.0)
        )


class Gate(msgspec.Struct, frozen=True, kw_only=True):
    """Stationary rounded projector aperture with a rough, feathered edge."""

    inset: Annotated[FrameLength, msgspec.Meta(description="Inset from each frame edge"), Presentation(soft=Some((0.0, 0.1)), precision=Some(3))] = 0.02
    radius: Annotated[FrameLength, msgspec.Meta(description="Corner radius inside inset"), Presentation(soft=Some((0.0, 0.1)), precision=Some(3))] = 0.02
    feather: Annotated[FrameLength, msgspec.Meta(description="Edge transition width"), Presentation(soft=Some((0.0, 0.02)), precision=Some(4))] = 0.005
    roughness: Annotated[FrameLength, msgspec.Meta(description="Largest edge displacement"), Presentation(soft=Some((0.0, 0.02)), precision=Some(4))] = 0.003
    color: Annotated[Rgb[Finite], msgspec.Meta(description="Display color outside aperture")] = Rgb(red=0.0, green=0.0, blue=0.0)

    def sampled(self, points: npt.NDArray[np.float64], extent: tuple[float, float], *, seed: Seed) -> ImagingResult[npt.NDArray[np.float64]]:
        """Returns continuous rough rounded-aperture coverage before pixel integration.

        Args:
            points: Displayed greater-side XY coordinates.
            extent: Displayed frame width and height in the same units.
            seed: Shot's explicit random origin.

        Returns:
            Aperture coverage, or native edge field refusal.
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
        edge = msgspec.structs.replace(RIDGED, offset=1.0)

        def coverage(distance: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            return np.clip(0.5 - distance / self.feather, 0.0, 1.0) if self.feather > 0.0 else (distance < 0.0).astype(np.float64)

        return sample(edge, at, seed=seed, stream=Noise.EDGE).map(lambda field: coverage(distance + self.roughness * (2.0 * field[:, 0] - 1.0)))


class Damage(Effect, frozen=True, kw_only=True, tag=44):
    """Film marks and weave behind a stationary projector aperture."""

    stage = Stage.DAMAGE
    pointwise = False
    motion: Motion = Motion()
    seed: Seed = 0
    speckles: Gated[Speckles] = Gated(enabled=True, value=Speckles())
    scratches: Gated[Scratches] = Gated(enabled=True, value=Scratches())
    hairs: Gated[Hairs] = Gated(enabled=False, value=Hairs())
    weave: Gated[Weave] = Gated(enabled=True, value=Weave())
    gate: Gated[Gate] = Gated(enabled=False, value=Gate())

    def filtered(
        self, bounds: tuple[float, float, float, float], shape: tuple[int, int], extent: tuple[float, float], clock: float, *, precision: Precision
    ) -> ImagingResult[Estimate[npt.NDArray[np.float64]]]:
        """Returns pixel-footprint-filtered mark pairs and stationary gate with joint replicates.

        Args:
            bounds: Positive global window as left, bottom, right, top.
            shape: Pixel rows and columns splitting the window, rows top first.
            extent: Original displayed camera-frame width and height.
            clock: Held film clock.
            precision: Caller-selected Sobol resolution and independent error replicates.

        Returns:
            `(rows, columns, 7)` speck, scratch, and hair coverage and ink pairs with gate coverage and numerical error.
        """
        left, bottom, right, top = bounds
        height, width = shape
        step = np.array(((right - left) / width, (top - bottom) / height))
        corners = np.stack(np.meshgrid(left + np.arange(width) * step[0], top - (np.arange(height) + 1) * step[1]), axis=-1)
        nets = tuple(precision.points(2, replicate) for replicate in range(precision.replicates))
        empty = np.zeros((precision.replicates, height, width, 2), dtype=np.float64)

        def footprints(field: Callable[[npt.NDArray[np.float64]], ImagingResult[npt.NDArray[np.float64]]]) -> ImagingResult[npt.NDArray[np.float64]]:
            return accumulated(
                accumulated(field((row[:, None] + net * step).reshape(-1, 2)).map(lambda values: values.reshape(width, len(net), -1).mean(axis=1)) for row in corners).map(np.stack) for net in nets
            ).map(lambda replicates: np.stack(replicates, dtype=np.float64))

        def computed() -> ImagingResult[Estimate[npt.NDArray[np.float64]]]:
            spots = (
                self.speckles.value.population.integrated(
                    bounds, shape, seed=self.seed, precision=precision, stream=math.floor(clock) * len(Noise) + Noise.SPECKLES, paint=lambda tone: float(tone < self.speckles.value.white)
                ).map(lambda estimate: estimate.replicates)
                if self.speckles.enabled
                else Ok(empty)
            )
            scratches = (
                accumulated(self.scratches.value.integrated((left + column * step[0], left + (column + 1) * step[0]), extent, clock, seed=self.seed) for column in range(width)).map(
                    lambda columns: np.broadcast_to(np.stack(columns), empty.shape)
                )
                if self.scratches.enabled
                else Ok(empty)
            )
            hairs = footprints(lambda points: self.hairs.value.sampled(points, extent, clock, seed=self.seed)) if self.hairs.enabled else Ok(empty)
            gate = footprints(lambda points: self.gate.value.sampled(points, extent, seed=self.seed)) if self.gate.enabled else Ok(np.ones((*empty.shape[:-1], 1), dtype=np.float64))
            return accumulated((spots, scratches, hairs, gate)).map(lambda values: Estimate.of(np.concatenate(values, axis=-1), precision))

        return guarded(ImagingNumericFault, self, computed)

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

        def processed(plane: npt.NDArray[np.float32]) -> ImagingResult[Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]]:
            left, bottom, right, top = bounds
            height, width = plane.shape[:2]
            step_x, step_y = (right - left) / width, (top - bottom) / height
            fields = self.filtered(bounds, (height, width), extent, clock, precision=precision).map(lambda estimate: estimate.replicates)
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
                        ImagingBufferFault
                        .located(replicate.astype(np.float32), source)
                        .bind(lambda image: ImagingBufferFault.checked(OpenImageIO.ImageBufAlgo.warp(image, transform, "triangle", 2.0, False, "clamp")))
                        .bind(ImagingBufferFault.pixels)
                        for replicate in marked
                    )
                    if self.weave.enabled
                    else Ok(tuple(replicate.astype(np.float32) for replicate in marked))
                )

                def gated(replicates: tuple[npt.NDArray[np.float32], ...]) -> ImagingResult[Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]]:
                    result = np.stack(replicates).astype(np.float64)
                    gate = fields[..., 6:]
                    color = np.array(self.gate.value.color)
                    result[..., :3] = result[..., :3] * gate + color * result[..., 3:4] * (1.0 - gate)
                    result[..., 4:] = 1.0 - (1.0 - result[..., 4:]) * gate
                    estimate = Estimate.of(result, precision)
                    return paired(ImagingBufferFault.located(estimate.value[..., :4].astype(np.float32), source), ImagingBufferFault.located(estimate.value[..., 4:].astype(np.float32), source)).map(
                        estimate.holding
                    )

                return moved.bind(gated)

            return paired(fields, displacement).bind(composed)

        return guarded(ImagingNumericFault, self, lambda: ImagingBufferFault.pixels(source).bind(processed))


class Flicker(Effect, frozen=True, kw_only=True, tag=47):
    """Uniform scene-linear lamp exposure variation in stops."""

    stage = Stage.EXPOSURE
    pointwise = True
    motion: Motion = Motion()
    seed: Seed = 0
    amplitude: Annotated[Stops, msgspec.Meta(description="Largest signed variation in stops")] = 0.05
    rate: Annotated[NonNegative, msgspec.Meta(description="Lamp evolution in cells per motion frame"), Presentation(soft=Some((0.0, 10.0)))] = 2.5


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Damage", "Flicker", "Gate", "Hairs", "Noise", "Scratches", "Speckles", "Weave"]
