"""Projected film damage: speckles, scratches, and hairs integrated over each pixel's footprint, gate weave, and a stationary rough projector aperture."""

import functools
from typing import Annotated

import bpy
from expression import Ok, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End
from blender.bpy.nodes.textures import Noise, stream, uniforms, Voronoi
from blender.bpy.render.compositor.coordinates import Clock
from blender.bpy.render.compositor.effects import Alpha, CameraFrame, CompositorEffect, Inspects, precise, Prepared, Prepares, Requests, Shell
from blender.imaging.color.processors import Domain
from blender.imaging.faults import ImagingResult
from blender.imaging.files import images
from blender.imaging.geometry.regions import FrameExtent, FrameLength
from blender.imaging.sampling.integration import Estimate, footprint, Precision, Raster, smoothstep
from blender.imaging.sampling.populations import Population
from blender.records.faults import accumulated, guarded, NumericFault, paired
from blender.records.fields import AtLeastOne, Finite, Gated, NonNegative, Presentation, Quantity, Rgb, Seed, Unit

from ...chain.stages import Effect, Stage

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
    rate: Annotated[NonNegative, msgspec.Meta(description="Presence evolution in cells per clock frame"), Presentation(soft=Some((0.0, 1.0)))] = 0.05

    def integrated(self, interval: tuple[float, float], extent: tuple[float, float], clock: float, *, seed: Seed) -> npt.NDArray[np.float64]:
        """Integrates every scratch stripe over its nearest-cell ownership at the held clock height across a positive horizontal pixel interval.

        Args:
            interval: Displayed horizontal pixel interval.
            extent: Displayed frame width and height.
            clock: Held film clock.
            seed: Record seed selecting the scratch sites.

        Returns:
            Mean coverage and covered white ink.
        """
        scale = self.count / extent[0]
        left, right = (value * scale for value in interval)
        height, radius = clock * self.rate, self.width * scale / 2.0

        def column(index: int) -> npt.NDArray[np.float64]:
            cells = np.stack(np.meshgrid(np.arange(index - 1, index + 2), np.arange(np.floor(height) - 1, np.floor(height) + 2), indexing="ij"), axis=-1).reshape(-1, 2).astype(np.int64)
            draws = uniforms(cells, stream(Scratches, seed), 2)
            sites = cells + draws
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

        return sum((column(index) for index in range(int(np.floor(left)), int(np.floor(right)) + 1)), np.zeros(2, dtype=np.float64))


class Hairs(msgspec.Struct, frozen=True, kw_only=True):
    """Curled, trimmed Voronoi cell edges drawn as film hairs."""

    count: Annotated[NonNegative, msgspec.Meta(description="Nominal strand density"), Presentation(soft=Some((0.0, 20.0)))] = 1.0
    width: Annotated[FrameLength, msgspec.Meta(description="Hair width"), Presentation(soft=Some((0.0, 0.005)), precision=Some(4))] = 0.0012
    white: Annotated[Unit, msgspec.Meta(description="Share of white hairs")] = 0.0
    length: Annotated[FrameExtent, msgspec.Meta(description="Strand cell scale"), Presentation(soft=Some((0.001, 0.2)), precision=Some(3))] = 0.08
    curl: Annotated[NonNegative, msgspec.Meta(description="Curl displacement in strand lengths"), Presentation(soft=Some((0.0, 5.0)))] = 1.0
    rate: Annotated[NonNegative, msgspec.Meta(description="Strand evolution in cells per clock frame"), Presentation(soft=Some((0.0, 1.0)))] = 0.1

    def sampled(self, points: npt.NDArray[np.float64], extent: tuple[float, float], clock: float, *, seed: Seed) -> npt.NDArray[np.float64]:
        """Returns curled, density-selected, trimmed hair coverage and covered white ink at displayed points, cells evolving along the clock.

        Args:
            points: Displayed greater-side XY coordinates.
            extent: Displayed frame width and height in the same units.
            clock: Held film clock.
            seed: Record seed selecting the strands.

        Returns:
            Coverage and covered ink columns.
        """
        cells = points / self.length
        bend = Noise(noise_dimensions="3D", scale=1.0, detail=0.0)
        curled = cells + self.curl * np.column_stack(tuple(2.0 * bend.evaluated(np.column_stack((cells, np.full(len(cells), stream(Hairs, "curl", axis, seed)))))["Fac"] - 1.0 for axis in range(2)))
        at = np.column_stack((curled, np.full(len(cells), clock * self.rate)))
        plane = float(stream(Hairs, "edge", seed))
        distance = Voronoi(voronoi_dimensions="4D", feature="DISTANCE_TO_EDGE", scale=1.0).evaluated(at, plane)["Distance"] * self.length
        color = Voronoi(voronoi_dimensions="4D", scale=1.0).evaluated(at, plane)["Color"]
        trim = bend.evaluated(np.column_stack((cells, np.full(len(cells), stream(Hairs, "trim", seed)))))["Fac"]
        coverage = (distance < self.width / 2.0) * (color[:, 0] < self.count * self.length**2 / (extent[0] * extent[1])) * smoothstep(np.clip((trim - 0.45) / 0.1, 0.0, 1.0))
        return np.column_stack((coverage, coverage * (color[:, 1] < self.white))).astype(np.float64)


class Weave(msgspec.Struct, frozen=True, kw_only=True):
    """Film displacement through projector gate."""

    amplitude: Annotated[
        tuple[NonNegative, NonNegative], msgspec.Meta(description="Largest displacement per axis"), Presentation(soft=Some((0.0, 0.005)), precision=Some(4), quantity=Some(Quantity.FRAME))
    ] = (0.0006, 0.0006)
    rate: Annotated[NonNegative, msgspec.Meta(description="Displacement evolution in cells per clock frame"), Presentation(soft=Some((0.0, 1.0)))] = 0.125

    def sampled(self, clock: float, *, seed: Seed) -> npt.NDArray[np.float64]:
        """Returns independently drawn horizontal and vertical displayed displacement of the film at the held clock."""
        at = np.array([(clock * self.rate, stream(Weave, axis, seed), 0.0) for axis in range(2)], dtype=np.float64)
        return np.asarray(self.amplitude) * (2.0 * Noise(noise_dimensions="2D", scale=1.0, detail=0.0).evaluated(at)["Fac"].astype(np.float64) - 1.0)


class Gate(msgspec.Struct, frozen=True, kw_only=True):
    """Stationary rounded projector aperture with a rough, feathered edge."""

    inset: Annotated[FrameLength, msgspec.Meta(description="Inset from each frame edge"), Presentation(soft=Some((0.0, 0.1)), precision=Some(3))] = 0.02
    radius: Annotated[FrameLength, msgspec.Meta(description="Corner radius inside inset"), Presentation(soft=Some((0.0, 0.1)), precision=Some(3))] = 0.02
    feather: Annotated[FrameLength, msgspec.Meta(description="Edge transition width"), Presentation(soft=Some((0.0, 0.02)), precision=Some(4))] = 0.005
    roughness: Annotated[FrameLength, msgspec.Meta(description="Largest edge displacement"), Presentation(soft=Some((0.0, 0.02)), precision=Some(4))] = 0.003
    edge: Annotated[Noise, msgspec.Meta(description="Ridged noise roughening the edge")] = Noise(
        noise_type="RIDGED_MULTIFRACTAL", scale=2.5, detail=15.0, roughness=0.9, gain=74.5, offset=1.0, distortion=1.9
    )
    color: Annotated[Rgb[Finite], msgspec.Meta(description="Display color outside aperture")] = Rgb(red=0.0, green=0.0, blue=0.0)

    def sampled(self, points: npt.NDArray[np.float64], extent: tuple[float, float], *, seed: Seed) -> npt.NDArray[np.float64]:
        """Returns rough rounded-aperture coverage at displayed points before pixel integration.

        Args:
            points: Displayed greater-side XY coordinates.
            extent: Displayed frame width and height in the same units.
            seed: Record seed selecting the edge.

        Returns:
            Aperture coverage, zero once the inset leaves no aperture.
        """
        center = np.asarray(extent) / 2.0
        remaining = center - self.inset
        if (shorter := float(np.min(remaining))) <= 0.0:
            return np.zeros((len(points), 1), dtype=np.float64)
        radius = min(self.radius, shorter)
        excess = np.abs(points - center) - (remaining - radius)
        distance = np.linalg.norm(np.maximum(excess, 0.0), axis=1) + np.minimum(np.max(excess, axis=1), 0.0) - radius
        rough = distance + self.roughness * (2.0 * self.edge.evaluated(np.column_stack((points, np.full(len(points), stream(Gate, seed)))))["Fac"] - 1.0)
        coverage = np.clip(0.5 - rough / self.feather, 0.0, 1.0) if self.feather > 0.0 else (rough < 0.0).astype(np.float64)
        return coverage[:, None]


class Damage(Effect, frozen=True, kw_only=True):
    """Film marks and weave behind a stationary projector aperture."""

    stage = Stage.DAMAGE
    pointwise = False
    clock: Clock = Clock()
    seed: Seed = 0
    speckles: Gated[Speckles] = Gated(enabled=True, value=Speckles())
    scratches: Gated[Scratches] = Gated(enabled=True, value=Scratches())
    hairs: Gated[Hairs] = Gated(enabled=False, value=Hairs())
    weave: Gated[Weave] = Gated(enabled=True, value=Weave())
    gate: Gated[Gate] = Gated(enabled=False, value=Gate())

    def filtered(self, raster: Raster, extent: tuple[float, float], clock: float, *, precision: Precision) -> ImagingResult[Estimate[npt.NDArray[np.float64]]]:
        """Returns pixel-footprint means of speck, scratch, and hair coverage and ink pairs beside gate coverage, with joint replicates.

        Args:
            raster: Displayed data window in original camera units and its pixel grid.
            extent: Original displayed camera-frame width and height.
            clock: Held film clock.
            precision: Sobol resolution and independent error replicates.

        Returns:
            `(rows, columns, 7)` estimates, or numeric refusal.
        """
        height, width = raster.shape
        empty = np.zeros((precision.replicates, height, width, 2), dtype=np.float64)
        left, step = raster.bounds[0], raster.step[0]
        speckles, scratches, hairs, gate = self.speckles.value, self.scratches.value, self.hairs.value, self.gate.value

        def computed() -> ImagingResult[Estimate[npt.NDArray[np.float64]]]:
            spots = (
                speckles.population.integrated(raster, uniforms, precision=precision, stream=stream(Speckles, int(np.floor(clock)), self.seed), paint=lambda tone: float(tone < speckles.white)).map(
                    lambda estimate: estimate.replicates
                )
                if self.speckles.enabled
                else Ok(empty)
            )
            lines = (
                np.broadcast_to(np.stack([scratches.integrated((left + column * step, left + (column + 1) * step), extent, clock, seed=self.seed) for column in range(width)]), empty.shape)
                if self.scratches.enabled
                else empty
            )
            strands = (
                footprint(lambda points: Ok(hairs.sampled(points, extent, clock, seed=self.seed)), raster, precision).map(lambda estimate: estimate.replicates) if self.hairs.enabled else Ok(empty)
            )
            aperture = (
                footprint(lambda points: Ok(gate.sampled(points, extent, seed=self.seed)), raster, precision).map(lambda estimate: estimate.replicates)
                if self.gate.enabled
                else Ok(np.ones((*empty.shape[:-1], 1), dtype=np.float64))
            )
            return accumulated((spots, Ok(lines), strands, aperture)).map(lambda values: Estimate.of(np.concatenate(values, axis=-1), precision))

        return guarded(NumericFault, self, computed, frozenset())

    def applied(
        self, source: OpenImageIO.ImageBuf, raster: Raster, extent: tuple[float, float], clock: float, *, precision: Precision
    ) -> ImagingResult[Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]]:
        """Marks associated film, weaves it through the gate, and keys the damage, with joint numerical error.

        Args:
            source: Associated RGBA, rows top first, with retained windows.
            raster: Displayed data window in original camera units and its pixel grid.
            extent: Original displayed camera-frame width and height.
            clock: Held film clock.
            precision: Integration resolution and independent replicates.

        Returns:
            Result RGBA and scalar damage key with joint replicate error, or image and numeric faults.
        """
        step_x, step_y = raster.step

        def composed(plane: npt.NDArray[np.float32], fields: npt.NDArray[np.float64]) -> ImagingResult[Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]]:
            copies = np.broadcast_to(plane, (precision.replicates, *plane.shape)).astype(np.float64)

            def marked(held: tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]], section: int) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
                color, mask = held
                coverage, ink = fields[..., 2 * section : 2 * section + 1], fields[..., 2 * section + 1 : 2 * section + 2]
                return np.concatenate((color[..., :3] * (1.0 - coverage) + color[..., 3:] * ink, color[..., 3:]), axis=-1), 1.0 - (1.0 - mask) * (1.0 - coverage)

            color, mask = functools.reduce(marked, range(3), (copies, np.zeros((*copies.shape[:-1], 1), dtype=np.float64)))
            shift = self.weave.value.sampled(clock, seed=self.seed)
            transform = (1.0, 0.0, 0.0, 0.0, 1.0, 0.0, float(shift[0] / step_x), float(-shift[1] / step_y), 1.0)
            replicates = tuple(replicate.astype(np.float32) for replicate in np.concatenate((color, mask), axis=-1))
            moved = (
                accumulated(
                    images.located(replicate, source).bind(lambda image: images.checked(OpenImageIO.ImageBufAlgo.warp(image, transform, "triangle", 2.0, False, "clamp"))).bind(images.pixels)
                    for replicate in replicates
                )
                if self.weave.enabled
                else Ok(replicates)
            )

            def gated(warped: tuple[npt.NDArray[np.float32], ...]) -> ImagingResult[Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]]:
                result = np.stack(warped).astype(np.float64)
                aperture = fields[..., 6:]
                result[..., :3] = result[..., :3] * aperture + np.asarray(self.gate.value.color) * result[..., 3:4] * (1.0 - aperture)
                result[..., 4:] = 1.0 - (1.0 - result[..., 4:]) * aperture
                estimate = Estimate.of(result, precision)
                return paired(images.located(estimate.value[..., :4].astype(np.float32), source), images.located(estimate.value[..., 4:].astype(np.float32), source)).map(estimate.holding)

            return moved.bind(gated)

        return paired(images.pixels(source), self.filtered(raster, extent, clock, precision=precision).map(lambda estimate: estimate.replicates)).bind(lambda pair: composed(*pair))


# --- [OPERATIONS] -----------------------------------------------------------------------


def prepared(
    _identity: Identity,
    scene: bpy.types.Scene,
    record: Damage,
    _config: PyOpenColorIO.Config,
    source: OpenImageIO.ImageBuf,
    precision: Precision,
    frame: CameraFrame,
    _entry: bpy.types.SceneCompositorEffect,
) -> BlenderResult[Prepared[Damage]]:
    """Damages captured film in original camera units and publishes the damage key beside it.

    Args:
        _identity: Stage job owner.
        scene: Evaluated scene supplying the film clock.
        record: Evaluated film marks, weave, and stationary gate.
        _config: Stage color configuration, film reading associated scene-linear RGBA.
        source: Top-first data image with retained full window.
        precision: Integration resolution and error replicates.
        frame: Original camera pixel aspect and retained overscan scale.
        _entry: Evaluated stage entry.

    Returns:
        Unchanged record with jointly estimated image and damage key, or imaging faults.
    """
    spec = source.spec()
    bounds, extent = frame.window(spec)

    def published(estimate: Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]) -> Prepared[Damage]:
        return record, {
            key: Estimate(value=plane, standard_error=estimate.standard_error[..., part], precision=estimate.precision, replicates=estimate.replicates[..., part], normalization=estimate.normalization)
            for key, plane, part in zip((Shell.image, "damage"), estimate.value, (slice(4), slice(4, None)))
        }

    raster = Raster(bounds=bounds, shape=(spec.height, spec.width))
    return record.applied(source, raster, extent, record.clock.clock(scene.frame_current + scene.frame_subframe), precision=precision).map(published)


# --- [COMPOSITION] ----------------------------------------------------------------------

damage = CompositorEffect(
    record=Damage,
    graph=Shell.passed,
    domain=Domain.DISPLAY,
    alpha=Alpha.RESULT,
    parts=(Requests[Damage](request=precise), Prepares(prepare=prepared), Inspects(key="damage_key", end=End(node="damage", socket="Image"), description="Integrated film marks and stationary gate")),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Damage", "Gate", "Hairs", "Scratches", "Speckles", "Weave", "damage"]
