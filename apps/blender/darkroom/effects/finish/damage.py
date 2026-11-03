"""Projected film damage: speckles, scratches, and hairs integrated over each pixel's footprint, gate weave, and a stationary rough projector aperture."""

from functools import partial, reduce
from typing import Annotated, Final

import bpy
from expression import Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO

from blender.bpy.extension.identity import Identity
from blender.bpy.nodes.functions import column, POSITION, sampled, W
from blender.bpy.nodes.interfaces import decoded, Interface, InterfacePanel
from blender.bpy.nodes.textures import Noise, Voronoi
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, precise, Prepared, Stage
from blender.bpy.render.compositor.inputs import passed, toggled, value
from blender.bpy.render.compositor.motion import CLOCK, Clock, SEED
from blender.imaging.files import buffers
from blender.imaging.geometry.arithmetic import smoothstep
from blender.imaging.geometry.frames import Raster
from blender.imaging.sampling.integration import Estimate, footprint, Population, Precision
from blender.imaging.sampling.tiles import stream, uniforms
from blender.records.faults import accumulated, Outcome, paired
from blender.records.fields import AtLeastOne, Case, NonNegative, Positive, Seed, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type _Parts = tuple[Option[Speckles], Option[Scratches], Option[Hairs], Option[Weave], Option[Gate]]

# --- [MODELS] ---------------------------------------------------------------------------


class Speckles(msgspec.Struct, frozen=True, kw_only=True):
    """Independent opaque or translucent black and white film spots."""

    population: Population = Population(spacing=0.02, density=0.02, variation=1.0, spread=0.0, softness=0.3)
    white: Annotated[Unit, msgspec.Meta(description="Share of white spots")] = 0.5

    def covered(self, raster: Raster, precision: Precision, clock: float, *, seed: int) -> Outcome[npt.NDArray[np.float64]]:
        """Returns spot and white spot coverage replicates over each pixel, the spots redrawn per whole clock unit."""
        return self.population.integrated(raster, precision=precision, stream=stream(Speckles, int(np.floor(clock)), seed), paint=lambda tone: float(tone < self.white)).map(
            lambda estimate: estimate.replicates
        )


class Scratches(msgspec.Struct, frozen=True, kw_only=True):
    """Vertical film scratches with fixed horizontal sites and evolving presence."""

    count: Annotated[AtLeastOne, msgspec.Meta(description="Scratch cells across frame width")] = 3.0
    width: Annotated[NonNegative, msgspec.Meta(description="Scratch width over greater frame side")] = 0.0008
    white: Annotated[Unit, msgspec.Meta(description="Share of white scratches")] = 0.7
    rate: Annotated[NonNegative, msgspec.Meta(description="Presence evolution in cells per clock unit")] = 0.05

    def covered(self, raster: Raster, extent: tuple[float, float], clock: float, *, seed: int) -> npt.NDArray[np.float64]:
        """Integrates every scratch stripe over its nearest-cell ownership at the held clock height across each displayed pixel column.

        Args:
            raster: Displayed pixel grid.
            extent: Displayed frame width and height.
            clock: Held film clock.
            seed: Seed selecting the scratch sites.

        Returns:
            `(columns, 2)` mean coverage and white coverage, alike down each column.
        """
        scale = self.count / extent[0]
        height, radius = clock * self.rate, self.width * scale / 2.0

        def strip(index: int, left: float, right: float) -> npt.NDArray[np.float64]:
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

        def interval(left: float, right: float) -> npt.NDArray[np.float64]:
            return sum((strip(index, left, right) for index in range(int(np.floor(left)), int(np.floor(right)) + 1)), np.zeros(2, dtype=np.float64))

        start, step = raster.bounds.left * scale, raster.step[0] * scale
        return np.stack([interval(start + index * step, start + (index + 1) * step) for index in range(raster.shape[1])])


class Hairs(msgspec.Struct, frozen=True, kw_only=True):
    """Curled, trimmed Voronoi cell edges drawn as film hairs."""

    count: Annotated[NonNegative, msgspec.Meta(description="Nominal strand density")] = 1.0
    width: Annotated[NonNegative, msgspec.Meta(description="Hair width over greater frame side")] = 0.0012
    white: Annotated[Unit, msgspec.Meta(description="Share of white hairs")] = 0.0
    length: Annotated[Positive, msgspec.Meta(description="Strand cell scale over greater frame side")] = 0.08
    curl: Annotated[NonNegative, msgspec.Meta(description="Curl displacement in strand lengths")] = 1.0
    rate: Annotated[NonNegative, msgspec.Meta(description="Strand evolution in cells per clock unit")] = 0.1

    def sampled(self, identity: Identity, points: npt.NDArray[np.float64], extent: tuple[float, float], clock: float, *, seed: int) -> Outcome[npt.NDArray[np.float64]]:
        """Returns curled, density-selected, trimmed hair coverage and white coverage at displayed points, cells evolving along the clock.

        Args:
            identity: Extension owning the scratch trees evaluating the noise.
            points: Displayed greater-side XY coordinates.
            extent: Displayed frame width and height in the same units.
            clock: Held film clock.
            seed: Seed selecting the strands.

        Returns:
            Coverage and white coverage columns, or the faults of the noise evaluation.
        """
        cells = points / self.length
        planes = {name: np.column_stack((cells, np.full(len(cells), stream(*parts, seed)))) for name, parts in (("curl_x", (Hairs.curl, 0)), ("curl_y", (Hairs.curl, 1)), ("trim", (Hairs.length,)))}
        bend = Noise(scale=1.0, detail=0.0)

        def edged(fields: tuple[npt.NDArray[np.float32], ...]) -> Outcome[npt.NDArray[np.float64]]:
            curl_x, curl_y, trim = fields
            curled = cells + self.curl * np.column_stack((2.0 * curl_x - 1.0, 2.0 * curl_y - 1.0))
            at = {"position": np.column_stack((curled, np.full(len(cells), clock * self.rate))), "w": np.full(len(cells), float(stream(Hairs, seed)))}
            kept = smoothstep(np.clip((trim - 0.45) / 0.1, 0.0, 1.0))

            def covered(found: tuple[npt.NDArray[np.float32], ...]) -> npt.NDArray[np.float64]:
                edges, color = found
                coverage = (edges * self.length < self.width / 2.0) * (color[:, 0] < self.count * self.length**2 / (extent[0] * extent[1])) * kept
                return np.column_stack((coverage, coverage * (color[:, 1] < self.white))).astype(np.float64)

            return sampled(
                identity,
                at,
                Voronoi(voronoi_dimensions="4D", feature="DISTANCE_TO_EDGE", scale=1.0).node(POSITION, W),
                Voronoi(voronoi_dimensions="4D", scale=1.0).node(POSITION, W, socket="Color", socket_type=bpy.types.NodeSocketColor),
            ).map(covered)

        return sampled(identity, planes, *(bend.node(column(name, bpy.types.NodeSocketVector)) for name in planes)).bind(edged)


class Weave(msgspec.Struct, frozen=True, kw_only=True):
    """Film displacement through the projector gate."""

    amplitude: Annotated[tuple[NonNegative, NonNegative], msgspec.Meta(description="Largest displacement per axis as a frame share")] = (0.0006, 0.0006)
    rate: Annotated[NonNegative, msgspec.Meta(description="Displacement evolution in cells per clock unit")] = 0.125

    def shifted(self, identity: Identity, clock: float, *, seed: int) -> Outcome[npt.NDArray[np.float64]]:
        """Returns independently drawn horizontal and vertical displayed displacement of the film at the held clock."""
        at = np.array([(clock * self.rate, stream(Weave.amplitude, axis, seed), 0.0) for axis in range(2)], dtype=np.float64)
        return sampled(identity, {"position": at}, Noise(noise_dimensions="2D", scale=1.0, detail=0.0).node(POSITION)).map(
            lambda found: np.asarray(self.amplitude) * (2.0 * found[0].astype(np.float64) - 1.0)
        )


class Gate(msgspec.Struct, frozen=True, kw_only=True):
    """Stationary rounded projector aperture with a rough, feathered edge."""

    inset: Annotated[NonNegative, msgspec.Meta(description="Inset from each frame edge over greater frame side")] = 0.02
    radius: Annotated[NonNegative, msgspec.Meta(description="Corner radius inside inset")] = 0.02
    feather: Annotated[NonNegative, msgspec.Meta(description="Edge transition width")] = 0.005
    roughness: Annotated[NonNegative, msgspec.Meta(description="Largest edge displacement")] = 0.003
    color: Annotated[tuple[float, float, float, float], bpy.types.NodeSocketColor, msgspec.Meta(description="Display color outside aperture")] = (0.0, 0.0, 0.0, 1.0)
    edge: Noise = Noise(noise_type="RIDGED_MULTIFRACTAL", scale=2.5, detail=15.0, roughness=0.9, offset=1.0, gain=74.5, distortion=1.9)

    def sampled(self, identity: Identity, points: npt.NDArray[np.float64], extent: tuple[float, float], *, seed: int) -> Outcome[npt.NDArray[np.float64]]:
        """Returns rough rounded-aperture coverage at displayed points before pixel integration.

        Args:
            identity: Extension owning the scratch tree evaluating the edge noise.
            points: Displayed greater-side XY coordinates.
            extent: Displayed frame width and height in the same units.
            seed: Seed selecting the edge.

        Returns:
            Aperture coverage, zero once the inset leaves no aperture, or the faults of the noise evaluation.
        """
        center = np.asarray(extent) / 2.0
        remaining = center - self.inset
        if (shorter := float(np.min(remaining))) <= 0.0:
            return Ok(np.zeros((len(points), 1), dtype=np.float64))
        radius = min(self.radius, shorter)
        excess = np.abs(points - center) - (remaining - radius)
        distance = np.linalg.norm(np.maximum(excess, 0.0), axis=1) + np.minimum(np.max(excess, axis=1), 0.0) - radius

        def covered(found: tuple[npt.NDArray[np.float32], ...]) -> npt.NDArray[np.float64]:
            rough = distance + self.roughness * (2.0 * found[0].astype(np.float64) - 1.0)
            coverage: npt.NDArray[np.float64] = np.clip(0.5 - rough / self.feather, 0.0, 1.0) if self.feather > 0.0 else (rough < 0.0).astype(np.float64)
            return coverage[:, None]

        return sampled(identity, {"position": np.column_stack((points, np.full(len(points), stream(Gate, seed))))}, self.edge.node(POSITION)).map(covered)


class Damage(Case, frozen=True, kw_only=True):
    """Film marks and weave behind a stationary projector aperture, rendered on the CPU before the graph reads them."""


# --- [OPERATIONS] -----------------------------------------------------------------------


def _prepared(stage: Stage[Damage]) -> Outcome[Option[Prepared]]:
    """Marks captured film in original camera units, weaves it through the gate, and estimates the result with its numerical error.

    Args:
        stage: Captured film, entry values, camera frame, scene, and precision.

    Returns:
        Damaged film estimate, or the faults of the entry values, mark integrals, and warp.
    """
    values, identity, source, precision = stage.values, stage.identity, stage.image, stage.precision
    spec = source.spec()
    bounds, extent = stage.frame.window(spec)
    raster = Raster(bounds=bounds, shape=(spec.height, spec.width))
    empty = np.zeros((precision.replicates, spec.height, spec.width, 2), dtype=np.float64)

    def part[T: msgspec.Struct](panel: InterfacePanel, record: T) -> Outcome[Option[T]]:
        return toggled(values, partial(decoded, record, values, panel.key), panel)

    def replicated(estimate: Estimate[npt.NDArray[np.float64]]) -> npt.NDArray[np.float64]:
        return estimate.replicates

    def marks(seed: int, clock: float, found: _Parts) -> Outcome[npt.NDArray[np.float64]]:
        speckles, scratches, hairs, _, gate = found
        return accumulated((
            speckles.map(lambda held: held.covered(raster, precision, clock, seed=seed)).default_value(Ok(empty)),
            Ok(scratches.map(lambda held: np.broadcast_to(held.covered(raster, extent, clock, seed=seed), empty.shape)).default_value(empty)),
            hairs.map(lambda held: footprint(lambda points: held.sampled(identity, points, extent, clock, seed=seed), raster, precision).map(replicated)).default_value(Ok(empty)),
            gate.map(lambda held: footprint(lambda points: held.sampled(identity, points, extent, seed=seed), raster, precision).map(replicated)).default_value(
                Ok(np.ones((*empty.shape[:-1], 1), dtype=np.float64))
            ),
        )).map(lambda planes: np.concatenate(planes, axis=-1))

    def damaged(plane: npt.NDArray[np.float32], seed: int, clock: float, found: _Parts) -> Outcome[Option[Prepared]]:
        _, _, _, weave, gate = found

        def section(held: tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]], index: int, fields: npt.NDArray[np.float64]) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
            color, mask = held
            coverage, white = fields[..., 2 * index : 2 * index + 1], fields[..., 2 * index + 1 : 2 * index + 2]
            return np.concatenate((color[..., :3] * (1.0 - coverage) + color[..., 3:] * white, color[..., 3:]), axis=-1), 1.0 - (1.0 - mask) * (1.0 - coverage)

        def woven(fields: npt.NDArray[np.float64]) -> Outcome[tuple[npt.NDArray[np.float64], tuple[npt.NDArray[np.float32], ...]]]:
            copies = np.broadcast_to(plane, (precision.replicates, *plane.shape)).astype(np.float64)
            color, mask = reduce(lambda held, index: section(held, index, fields), range(3), (copies, np.zeros((*copies.shape[:-1], 1), dtype=np.float64)))
            replicates = tuple(replicate.astype(np.float32) for replicate in np.concatenate((color, mask), axis=-1))

            def warped(shift: npt.NDArray[np.float64]) -> Outcome[tuple[npt.NDArray[np.float32], ...]]:
                transform = (1.0, 0.0, 0.0, 0.0, 1.0, 0.0, float(shift[0] / raster.step[0]), float(-shift[1] / raster.step[1]), 1.0)
                return accumulated(
                    buffers.located(replicate, source).bind(lambda image: buffers.checked(OpenImageIO.ImageBufAlgo.warp(image, transform, "triangle", 2.0, False, "clamp"))).bind(buffers.pixels)
                    for replicate in replicates
                )

            return weave.map(lambda held: held.shifted(identity, clock, seed=seed).bind(warped)).default_value(Ok(replicates)).map(lambda shown: (fields, shown))

        def gated(held: tuple[npt.NDArray[np.float64], tuple[npt.NDArray[np.float32], ...]]) -> Outcome[Option[Prepared]]:
            fields, shown = held
            result = np.stack(shown).astype(np.float64)
            opening = fields[..., 6:]
            outside = gate.map(lambda held: np.asarray(held.color[:3])).default_value(np.zeros(3))
            result[..., :3] = result[..., :3] * opening + outside * result[..., 3:4] * (1.0 - opening)
            estimate = Estimate.of(result[..., :4], precision)
            return buffers.located(estimate.value.astype(np.float32), source).map(estimate.holding).map(Some)

        return marks(seed, clock, found).bind(woven).bind(gated)

    settings = paired(
        value(values, Seed, SEED),
        decoded(Clock(), values, _CLOCK.key).map(lambda held: held.timed(stage.scene)),
        paired(part(_SPECKLES, Speckles()), part(_SCRATCHES, Scratches()), part(_HAIRS, Hairs()), part(_WEAVE, Weave()), part(_GATE, Gate())),
    )
    return paired(buffers.pixels(source), settings).bind(lambda held: damaged(held[0], *held[1]))


# --- [COMPOSITION] ----------------------------------------------------------------------

_CLOCK: Final = InterfacePanel.of(CLOCK.key, Clock(speed=24.0))
_SPECKLES: Final = InterfacePanel.of("speckles", Speckles(), toggle=Some(True))
_SCRATCHES: Final = InterfacePanel.of("scratches", Scratches(), toggle=Some(True))
_HAIRS: Final = InterfacePanel.of("hairs", Hairs(), toggle=Some(False))
_WEAVE: Final = InterfacePanel.of("weave", Weave(), toggle=Some(True))
_GATE: Final = InterfacePanel.of("gate", Gate(), toggle=Some(False))
damage: Final = CompositorEffect(
    passed,
    record=Damage,
    interface=Interface(inputs=(SEED,), panels=(_CLOCK, _SPECKLES, _SCRATCHES, _HAIRS, _WEAVE, _GATE)),
    mix=Blended(alpha=Alpha.RESULT),
    prepared=Some(_prepared),
    request=Some(precise),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Damage", "damage"]
