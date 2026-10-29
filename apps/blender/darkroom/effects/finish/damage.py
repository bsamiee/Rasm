"""Projected film damage: speckles, scratches, and hairs integrated over each pixel's footprint, gate weave, and a stationary rough projector aperture."""

from collections.abc import Mapping
import functools
import inspect
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.evaluation import column
from blender.bpy.nodes.graphs import Expr
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, Socket
from blender.bpy.nodes.textures import Noise, Voronoi
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, precise
from blender.bpy.render.compositor.inputs import CameraFrame
from blender.bpy.render.compositor.motion import Clock
from blender.bpy.render.compositor.shells import Shell
from blender.imaging.color.processors import Domain
from blender.imaging.files import buffers
from blender.imaging.geometry.arithmetic import smoothstep
from blender.imaging.geometry.frames import Raster
from blender.imaging.sampling.integration import Estimate, footprint, Precision
from blender.imaging.sampling.populations import Population
from blender.imaging.sampling.tiles import stream, uniforms
from blender.records.faults import accumulated, Fault, Outcome, paired, RecordReason

from ...chain.stages import Effect, prepared, Stage
from ..optics.procedural import CLOCK, evaluated_nodes, POSITION, W

# --- [MODELS] ---------------------------------------------------------------------------


class Speckles(msgspec.Struct, frozen=True, kw_only=True):
    """Independent opaque or translucent black and white film spots."""

    population: Population
    white: float


class Scratches(msgspec.Struct, frozen=True, kw_only=True):
    """Vertical film scratches with fixed horizontal sites and evolving presence."""

    count: float
    width: float
    white: float
    rate: float

    def integrated(self, interval: tuple[float, float], extent: tuple[float, float], clock: float, *, seed: int) -> npt.NDArray[np.float64]:
        """Integrates every scratch stripe over its nearest-cell ownership at the held clock height across a positive horizontal pixel interval.

        Args:
            interval: Displayed horizontal pixel interval.
            extent: Displayed frame width and height.
            clock: Held film clock.
            seed: Seed selecting the scratch sites.

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

    count: float
    width: float
    white: float
    length: float
    curl: float
    rate: float

    def sampled(self, identity: Identity, points: npt.NDArray[np.float64], extent: tuple[float, float], clock: float, *, seed: int) -> npt.NDArray[np.float64]:
        """Returns curled, density-selected, trimmed hair coverage and covered white ink at displayed points, cells evolving along the clock.

        Args:
            identity: Extension owning the scratch trees evaluating the noise.
            points: Displayed greater-side XY coordinates.
            extent: Displayed frame width and height in the same units.
            clock: Held film clock.
            seed: Seed selecting the strands.

        Returns:
            Coverage and covered ink columns.
        """
        cells = points / self.length
        bend = Noise(scale=1.0, detail=0.0)
        planes = {name: np.column_stack((cells, np.full(len(cells), stream(Hairs, *family, seed)))) for name, family in (("curl_x", ("curl", 0)), ("curl_y", ("curl", 1)), ("trim", ("trim",)))}
        curl_x, curl_y, trim = evaluated_nodes(identity, planes, *(bend.node(column(name, bpy.types.NodeSocketVector)) for name in planes))
        curled = cells + self.curl * np.column_stack((2.0 * curl_x - 1.0, 2.0 * curl_y - 1.0))
        at = {"position": np.column_stack((curled, np.full(len(cells), clock * self.rate))), "w": np.full(len(cells), float(stream(Hairs, "edge", seed)))}
        edges, color = evaluated_nodes(
            identity,
            at,
            Voronoi(voronoi_dimensions="4D", feature="DISTANCE_TO_EDGE", scale=1.0).node(POSITION, W),
            Voronoi(voronoi_dimensions="4D", scale=1.0).node(POSITION, W, socket="Color", shape=bpy.types.NodeSocketColor),
        )
        distance = edges * self.length
        coverage = (distance < self.width / 2.0) * (color[:, 0] < self.count * self.length**2 / (extent[0] * extent[1])) * smoothstep(np.clip((trim - 0.45) / 0.1, 0.0, 1.0))
        return np.column_stack((coverage, coverage * (color[:, 1] < self.white))).astype(np.float64)


class Weave(msgspec.Struct, frozen=True, kw_only=True):
    """Film displacement through projector gate."""

    amplitude: tuple[float, float]
    rate: float

    def sampled(self, identity: Identity, clock: float, *, seed: int) -> npt.NDArray[np.float64]:
        """Returns independently drawn horizontal and vertical displayed displacement of the film at the held clock."""
        at = np.array([(clock * self.rate, stream(Weave, axis, seed), 0.0) for axis in range(2)], dtype=np.float64)
        (fac,) = evaluated_nodes(identity, {"position": at}, Noise(noise_dimensions="2D", scale=1.0, detail=0.0).node(POSITION))
        return np.asarray(self.amplitude) * (2.0 * fac.astype(np.float64) - 1.0)


class Gate(msgspec.Struct, frozen=True, kw_only=True):
    """Stationary rounded projector aperture with a rough, feathered edge."""

    inset: float
    radius: float
    feather: float
    roughness: float
    edge: Noise
    color: tuple[float, float, float, float]

    def sampled(self, identity: Identity, points: npt.NDArray[np.float64], extent: tuple[float, float], *, seed: int) -> npt.NDArray[np.float64]:
        """Returns rough rounded-aperture coverage at displayed points before pixel integration.

        Args:
            identity: Extension owning the scratch tree evaluating the edge noise.
            points: Displayed greater-side XY coordinates.
            extent: Displayed frame width and height in the same units.
            seed: Seed selecting the edge.

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
        rough: npt.NDArray[np.float64] = distance + self.roughness * (
            2.0 * evaluated_nodes(identity, {"position": np.column_stack((points, np.full(len(points), stream(Gate, seed))))}, self.edge.node(POSITION))[0] - 1.0
        )
        coverage = np.clip(0.5 - rough / self.feather, 0.0, 1.0) if self.feather > 0.0 else (rough < 0.0).astype(np.float64)
        return coverage[:, None]


class Damage(Effect, frozen=True, kw_only=True):
    """Film marks and weave behind a stationary projector aperture, the image the CPU render prepared in place of the entering color held while it renders."""

    stage = Stage.DAMAGE
    pointwise = False
    domain = Domain.DISPLAY
    staged: IdName | None = None


# --- [OPERATIONS] -----------------------------------------------------------------------


@prepared.register
def _(
    _record: Damage, identity: Identity, values: Mapping[Key, Value], _config: PyOpenColorIO.Config, source: OpenImageIO.ImageBuf, precision: Precision, frame: CameraFrame, scene: bpy.types.Scene, /
) -> Outcome[Option[OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf] | Mapping[Key, Value]]]:
    """Marks captured film in original camera units, weaves it through the gate, and estimates the result with its numerical error."""
    spec = source.spec()
    bounds, extent = frame.window(spec)
    raster = Raster(bounds=bounds, shape=(spec.height, spec.width))
    step_x, step_y = raster.step

    def part[T: msgspec.Struct](kind: type[T], prefix: Key, **given: object) -> Outcome[Option[T]]:
        enabled = values.get(prefix) is not False
        held = {**{field.name: values.get(item_key(prefix, field.name)) for field in msgspec.structs.fields(kind) if field.name not in given}, **given}
        return (
            catch(msgspec.convert, exception=msgspec.ValidationError)(held, kind).map(Some).map_error(functools.partial(Fault.of, RecordReason.UNREADABLE, kind.__name__)) if enabled else Ok(Nothing)
        )

    def composed(
        plane: npt.NDArray[np.float32], seed: int, clock: float, speckles: Option[Speckles], scratches: Option[Scratches], hairs: Option[Hairs], weave: Option[Weave], gate: Option[Gate]
    ) -> Outcome[Option[OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf] | Mapping[Key, Value]]]:
        height, width = raster.shape
        empty = np.zeros((precision.replicates, height, width, 2), dtype=np.float64)
        left, step = raster.bounds.left, raster.step[0]
        spots = speckles.map(
            lambda held: held.population.integrated(raster, uniforms, precision=precision, stream=stream(Speckles, int(np.floor(clock)), seed), paint=lambda tone: float(tone < held.white)).map(
                lambda estimate: estimate.replicates
            )
        ).default_value(Ok(empty))
        lines = scratches.map(
            lambda held: np.broadcast_to(np.stack([held.integrated((left + column * step, left + (column + 1) * step), extent, clock, seed=seed) for column in range(width)]), empty.shape)
        )
        strands = hairs.map(
            lambda held: footprint(lambda points: Ok(held.sampled(identity, points, extent, clock, seed=seed)), raster, precision).map(lambda estimate: estimate.replicates)
        ).default_value(Ok(empty))
        aperture = gate.map(lambda held: footprint(lambda points: Ok(held.sampled(identity, points, extent, seed=seed)), raster, precision).map(lambda estimate: estimate.replicates)).default_value(
            Ok(np.ones((*empty.shape[:-1], 1), dtype=np.float64))
        )

        def marked(fields: npt.NDArray[np.float64]) -> Outcome[Option[OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf] | Mapping[Key, Value]]]:
            copies = np.broadcast_to(plane, (precision.replicates, *plane.shape)).astype(np.float64)

            def section(held: tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]], index: int) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
                color, mask = held
                coverage, ink = fields[..., 2 * index : 2 * index + 1], fields[..., 2 * index + 1 : 2 * index + 2]
                return np.concatenate((color[..., :3] * (1.0 - coverage) + color[..., 3:] * ink, color[..., 3:]), axis=-1), 1.0 - (1.0 - mask) * (1.0 - coverage)

            color, mask = functools.reduce(section, range(3), (copies, np.zeros((*copies.shape[:-1], 1), dtype=np.float64)))
            replicates = tuple(replicate.astype(np.float32) for replicate in np.concatenate((color, mask), axis=-1))

            def woven(held: Weave) -> Outcome[tuple[npt.NDArray[np.float32], ...]]:
                shift = held.sampled(identity, clock, seed=seed)
                transform = (1.0, 0.0, 0.0, 0.0, 1.0, 0.0, float(shift[0] / step_x), float(-shift[1] / step_y), 1.0)
                return accumulated(
                    buffers.located(replicate, source).bind(lambda image: buffers.checked(OpenImageIO.ImageBufAlgo.warp(image, transform, "triangle", 2.0, False, "clamp"))).bind(buffers.pixels)
                    for replicate in replicates
                )

            def gated(warped: tuple[npt.NDArray[np.float32], ...]) -> Outcome[Option[OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf] | Mapping[Key, Value]]]:
                result = np.stack(warped).astype(np.float64)
                opening = fields[..., 6:]
                outside = gate.map(lambda held: np.asarray(held.color[:3])).default_value(np.zeros(3))
                result[..., :3] = result[..., :3] * opening + outside * result[..., 3:4] * (1.0 - opening)
                estimate = Estimate.of(result[..., :4], precision)
                return buffers.located(estimate.value.astype(np.float32), source).map(estimate.holding).map(Some)

            return weave.map(woven).default_value(Ok(replicates)).bind(gated)

        return accumulated((spots, Ok(lines.default_value(empty)), strands, aperture)).map(lambda found: np.concatenate(found, axis=-1)).bind(marked)

    seed = Option.of_optional(try_downcast(int, values.get("seed"))).to_result_with(functools.partial(Fault.of, RecordReason.UNREADABLE, "seed", repr(values.get("seed"))))
    clock = part(Clock, "clock").bind(lambda found: found.to_result_with(functools.partial(Fault.of, RecordReason.UNREADABLE, Clock.__name__)))
    speckles = part(Population, "speckles_population").bind(lambda population: population.map(lambda found: part(Speckles, "speckles", population=found)).default_value(Ok(Nothing)))
    gate = part(Noise, "gate_edge", noise_dimensions="3D", noise_type="RIDGED_MULTIFRACTAL", normalize=True).bind(
        lambda edge: edge.map(lambda found: part(Gate, "gate", edge=found)).default_value(Ok(Nothing))
    )
    timed = clock.map(lambda held: held.clock(scene.frame_current + scene.frame_subframe, scene.render.fps / scene.render.fps_base))
    return paired(paired(buffers.pixels(source), seed, timed, speckles), paired(part(Scratches, "scratches"), part(Hairs, "hairs"), part(Weave, "weave"), gate)).bind(
        lambda found: composed(*found[0], *found[1])
    )


def _damage(shell: Shell[Damage]) -> Outcome[Expr]:
    """Reads the CPU-damaged film image while a render stages it, the entering color otherwise."""
    return Ok(
        Option
        .of_optional(shell.record.staged)
        .map(lambda image: Expr(kind=bpy.types.CompositorNodeImage, members={"image": image}, socket="Image", shape=bpy.types.NodeSocketColor))
        .default_value(shell.color)
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

damage: Final = CompositorEffect(
    graph=_damage,
    record=Damage,
    interface=Interface(
        inputs=(Socket(key="seed", kind=bpy.types.NodeSocketInt, description="Number selecting every mark", min_value=0, max_value=2**24),),
        panels=(
            CLOCK,
            *(
                InterfacePanel(
                    key=key,
                    name=bpy.path.display_name(key),
                    description=str(inspect.getdoc(kind)),
                    toggle=Some(enabled),
                    inputs=(
                        *(
                            Socket(key=item_key(key, field), name=bpy.path.display_name(field), kind=shape, description=description, min_value=low, max_value=high, default_value=default)
                            for field, shape, description, low, high, default in rows
                        ),
                        *extra,
                    ),
                    panels=panels,
                )
                for key, kind, enabled, rows, extra, panels in (
                    (
                        "speckles",
                        Speckles,
                        True,
                        (
                            ("population_spacing", bpy.types.NodeSocketFloat, "Cell width over greater frame side", 1e-6, None, 0.02),
                            ("population_density", bpy.types.NodeSocketFloatFactor, "Share of cells holding a spot", 0.0, 1.0, 0.02),
                            ("population_size", bpy.types.NodeSocketFloat, "Spot diameter over cell width", 1e-6, 0.5, 0.2),
                            ("population_variation", bpy.types.NodeSocketFloatFactor, "Diameter shrink share below size", 0.0, 1.0, 1.0),
                            ("population_spread", bpy.types.NodeSocketFloat, "Per-spot coverage drop in stops", 0.0, None, None),
                            ("population_softness", bpy.types.NodeSocketFloatFactor, "Edge fade over radius before pixel filtering", 0.0, 1.0, 0.3),
                            ("white", bpy.types.NodeSocketFloatFactor, "Share of white spots", 0.0, 1.0, 0.5),
                        ),
                        (),
                        (),
                    ),
                    (
                        "scratches",
                        Scratches,
                        True,
                        (
                            ("count", bpy.types.NodeSocketFloat, "Scratch cells across frame width", 1.0, None, 3.0),
                            ("width", bpy.types.NodeSocketFloat, "Scratch width over greater frame side", 0.0, None, 0.0008),
                            ("white", bpy.types.NodeSocketFloatFactor, "Share of white scratches", 0.0, 1.0, 0.7),
                            ("rate", bpy.types.NodeSocketFloat, "Presence evolution in cells per clock frame", 0.0, None, 0.05),
                        ),
                        (),
                        (),
                    ),
                    (
                        "hairs",
                        Hairs,
                        False,
                        (
                            ("count", bpy.types.NodeSocketFloat, "Nominal strand density", 0.0, None, 1.0),
                            ("width", bpy.types.NodeSocketFloat, "Hair width over greater frame side", 0.0, None, 0.0012),
                            ("white", bpy.types.NodeSocketFloatFactor, "Share of white hairs", 0.0, 1.0, None),
                            ("length", bpy.types.NodeSocketFloat, "Strand cell scale over greater frame side", 1e-6, None, 0.08),
                            ("curl", bpy.types.NodeSocketFloat, "Curl displacement in strand lengths", 0.0, None, 1.0),
                            ("rate", bpy.types.NodeSocketFloat, "Strand evolution in cells per clock frame", 0.0, None, 0.1),
                        ),
                        (),
                        (),
                    ),
                    (
                        "weave",
                        Weave,
                        True,
                        (("rate", bpy.types.NodeSocketFloat, "Displacement evolution in cells per clock frame", 0.0, None, 0.125),),
                        (
                            Socket(
                                key="weave_amplitude",
                                name="Amplitude",
                                kind=bpy.types.NodeSocketVectorXYZ2D,
                                description="Largest displacement per axis as a frame share",
                                min_value=0.0,
                                default_value=(0.0006, 0.0006),
                            ),
                        ),
                        (),
                    ),
                    (
                        "gate",
                        Gate,
                        False,
                        (
                            ("inset", bpy.types.NodeSocketFloat, "Inset from each frame edge over greater frame side", 0.0, None, 0.02),
                            ("radius", bpy.types.NodeSocketFloat, "Corner radius inside inset", 0.0, None, 0.02),
                            ("feather", bpy.types.NodeSocketFloat, "Edge transition width", 0.0, None, 0.005),
                            ("roughness", bpy.types.NodeSocketFloat, "Largest edge displacement", 0.0, None, 0.003),
                        ),
                        (Socket(key="gate_color", name="Color", kind=bpy.types.NodeSocketColor, description="Display color outside aperture", default_value=(0.0, 0.0, 0.0, 1.0)),),
                        (
                            InterfacePanel(
                                key="gate_edge",
                                name="Edge",
                                description="Ridged noise roughening the edge",
                                inputs=tuple(
                                    Socket(key=item_key("gate_edge", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, min_value=low, max_value=high, default_value=default)
                                    for field, low, high, default in (
                                        ("scale", -1000.0, 1000.0, 2.5),
                                        ("detail", 0.0, 15.0, 15.0),
                                        ("roughness", 0.0, 1.0, 0.9),
                                        ("lacunarity", 0.0, 1000.0, 2.0),
                                        ("offset", -1000.0, 1000.0, 1.0),
                                        ("gain", 0.0, 1000.0, 74.5),
                                        ("distortion", -1000.0, 1000.0, 1.9),
                                    )
                                ),
                            ),
                        ),
                    ),
                )
            ),
        ),
    ),
    mix=Blended(alpha=Alpha.RESULT),
    request=Some(precise),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Damage", "Gate", "Hairs", "Scratches", "Speckles", "Weave", "damage"]
