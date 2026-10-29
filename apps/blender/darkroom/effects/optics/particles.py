"""Soft points in depth layers covering the frame or emitting light, integrated over pixels, the iris, and the scene shutter with ordered occlusion."""

from collections.abc import Callable, Mapping
from math import cos, exp2, floor, pi, sin, tau
from typing import Annotated, Final

import bpy
from expression import Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes import textures
from blender.bpy.nodes.desired import Interface, InterfacePanel, Socket
from blender.bpy.nodes.graphs import Expr
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, precise
from blender.bpy.render.compositor.inputs import CameraFrame, Shell
from blender.bpy.render.compositor.motion import Clock, Shutter
from blender.imaging.files import buffers
from blender.imaging.geometry.frames import Region
from blender.imaging.geometry.lenses import Aperture
from blender.imaging.sampling import tiles
from blender.imaging.sampling.integration import Estimate, Population, Precision, transmitted
from blender.records.faults import accumulated, Outcome
from blender.records.fields import AtLeastOne, Finite, NonNegative, Positive, Seed, Unit

from ...chain.stages import Effect, prepared, Stage
from .procedural import (
    bases,
    Basis,
    clock,
    decoded,
    density_range,
    Emission,
    emission,
    evaluated,
    evaluated_nodes,
    fields,
    Incandescence,
    incandescence,
    Perlin,
    POSITION,
    spectra,
    Spectrum,
    spectrum,
    Tint,
    W,
)

# --- [CONSTANTS] ------------------------------------------------------------------------

_CLOCK: Final = Clock(speed=2.496)
_WIND: Final = Perlin(texture=textures.Noise(noise_dimensions="4D", scale=3.2))
_FACE: Final = Interface(
    inputs=(
        Socket(key="incandescence", kind=bpy.types.NodeSocketFloatFactor, description="Blackbody share of emitted color"),
        Socket(key="direction", kind=bpy.types.NodeSocketFloatAngle, description="Particle travel direction", default_value=-pi / 2.0),
        Socket(key="turbulence", kind=bpy.types.NodeSocketFloat, description="Nearest layer sway over greater frame side", min_value=0.0),
        Socket(key="evolution", kind=bpy.types.NodeSocketFloat, description="Signed wind field evolution per scene frame"),
        Socket(key="motion_blur", kind=bpy.types.NodeSocketBool, description="Streaks each particle across scene shutter", default_value=True),
        Socket(key="layers", kind=bpy.types.NodeSocketInt, description="Depth layers drawn from far to near", min_value=1, default_value=3),
        Socket(key="depth", kind=bpy.types.NodeSocketFloat, description="Farthest layer distance over nearest", min_value=1.0, default_value=4.0),
        Socket(key="depth_variation", kind=bpy.types.NodeSocketFloatFactor, description="Random depth within each layer interval"),
        Socket(key="focus", kind=bpy.types.NodeSocketFloatFactor, description="Focused depth between nearest and farthest layers", default_value=1.0),
        Socket(key="defocus", kind=bpy.types.NodeSocketFloat, description="Blur radius per reciprocal depth difference", min_value=0.0),
        Socket(key="squeeze", kind=bpy.types.NodeSocketFloat, description="Horizontal compression of particle outline", min_value=0.0, default_value=1.0),
        Socket(key="fringe", kind=bpy.types.NodeSocketFloat, description="Channel radius shift, toward blue above zero", min_value=-1.0, max_value=1.0),
        Socket(key="fade", kind=bpy.types.NodeSocketFloatFactor, description="Trail share fading toward tail"),
        Socket(key="twinkle", kind=bpy.types.NodeSocketFloatFactor, description="Light share flickering over time"),
        Socket(key="seed", kind=bpy.types.NodeSocketInt, description="Number selecting pattern", min_value=0, max_value=2**24),
    ),
    panels=(
        InterfacePanel(
            key="population",
            name="Population",
            description="Particle layout, size, and edge",
            inputs=fields(
                "population",
                Socket(key="spacing", kind=bpy.types.NodeSocketFloat, description="Cell width at nearest layer over greater frame side", min_value=0.0, default_value=0.01),
                Socket(key="density", kind=bpy.types.NodeSocketFloatFactor, description="Share of cells holding a particle", default_value=0.5),
                Socket(key="size", kind=bpy.types.NodeSocketFloat, description="Particle diameter over cell width", min_value=0.0, max_value=0.5, default_value=0.2),
                Socket(key="variation", kind=bpy.types.NodeSocketFloatFactor, description="Diameter shrink share below size", default_value=0.5),
                Socket(key="spread", kind=bpy.types.NodeSocketFloat, description="Per-particle coverage drop in stops", min_value=0.0, default_value=1.0),
                Socket(key="softness", kind=bpy.types.NodeSocketFloatFactor, description="Edge fade over radius before pixel filtering", default_value=0.5),
            ),
        ),
        spectra("coverage", Tint(), "Scene-linear cover color by particle tone", Some(True)),
        emission("emission", Emission(), "Light a particle adds", Some(False)),
        incandescence("temperature", Incandescence(), "Blackbody band multiplying emitted spectrum"),
        clock("clock", speed=_CLOCK.speed),
        bases("wind", _WIND, "Texture displacing each axis independently", "Wind"),
        InterfacePanel(
            key="iris",
            name="Iris",
            description="Projected particle outline",
            inputs=fields(
                "iris",
                Socket(key="blades", kind=bpy.types.NodeSocketInt, description="Blades outlining bokeh", min_value=3, max_value=24, default_value=5),
                Socket(key="rotation", kind=bpy.types.NodeSocketFloatAngle, description="Clockwise blade vertex turn from vertical", min_value=-pi, max_value=pi),
                Socket(key="roundness", kind=bpy.types.NodeSocketFloatFactor, description="Blade curvature, circular at 1", default_value=1.0),
                Socket(key="obstruction", kind=bpy.types.NodeSocketFloatFactor, description="Central mirror share of iris"),
            ),
        ),
        clock("scintillation", speed=0.0),
    ),
)

# --- [MODELS] ---------------------------------------------------------------------------


class Layered(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Occupied particles of every depth layer far to near: lattice keys, depth scales, centers, radii, gains, and tones."""

    keys: npt.NDArray[np.int64]
    scales: npt.NDArray[np.float64]
    centers: npt.NDArray[np.float64]
    radii: npt.NDArray[np.float64]
    gains: npt.NDArray[np.float64]
    tones: npt.NDArray[np.float64]


class Particles(Effect, frozen=True, kw_only=True):
    """Soft points in depth layers covering frame or emitting light: rain, snow, sparks, dust, and stars, decoded from the entry inputs for the CPU render, the image it prepares in `staged`."""

    stage = Stage.GENERATE
    pointwise = False
    population: Population = Population()
    coverage: Spectrum | None = Tint()
    emission: Emission | None = None
    incandescence: Unit = 0.0
    temperature: Incandescence = Incandescence()
    clock: Clock = _CLOCK
    direction: Finite = -pi / 2.0
    turbulence: NonNegative = 0.0
    wind: Basis = _WIND
    evolution: Finite = 0.0
    motion_blur: bool = True
    layers: Annotated[int, msgspec.Meta(ge=1)] = 3
    depth: AtLeastOne = 4.0
    depth_variation: Unit = 0.0
    focus: Unit = 1.0
    defocus: NonNegative = 0.0
    iris: Aperture = Aperture()
    squeeze: Positive = 1.0
    fringe: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0)] = 0.0
    fade: Unit = 0.0
    twinkle: Unit = 0.0
    scintillation: Clock = Clock(speed=0.0)
    seed: Seed = 0
    staged: IdName | None = None

    @property
    def channels(self) -> npt.NDArray[np.float64]:
        """Red, green, and blue outline radius factors of the fringe."""
        return np.array((1.0 - max(self.fringe, 0.0), 1.0 - abs(self.fringe) / 2.0, 1.0 + min(self.fringe, 0.0)))

    def layered(self, bounds: Region, held: float, shutter: tuple[float, float], reach: float, rate: float) -> Layered:
        """Draws every layer's occupied cells able to reach the bounds across the shutter, sorted far to near by depth then lattice key, the clock at `rate` scene frames per second."""
        heading = np.array((cos(self.direction), sin(self.direction)))
        focus = self.depth ** (-self.focus)
        drawn = self.population.density > 0.0 and self.iris.obstruction < 1.0 and (self.coverage is not None or self.emission is not None)

        def layer(index: int) -> tuple[npt.NDArray[np.float64] | npt.NDArray[np.int64], ...]:
            midpoint = (index + 0.5) / self.layers
            near, far = (self.depth ** (-(midpoint + side * self.depth_variation / (2.0 * self.layers))) for side in (-1.0, 1.0))
            spacing = self.population.spacing * self.depth ** (-midpoint)
            extent = self.population.spacing * near * self.population.size / 2.0 + self.defocus * max(abs(near - focus), abs(far - focus))
            support = np.array((extent / self.squeeze, extent))
            travel = np.array([heading * self.clock.speed / rate * scale * time for scale in (near, far) for time in (held + shutter[0], held + shutter[1])])
            low, high = travel.min(axis=0) - self.turbulence * near * reach - support, travel.max(axis=0) + self.turbulence * near * reach + support
            columns = np.arange(floor((bounds.left - high[0]) / spacing) - 1, floor((bounds.right - low[0]) / spacing) + 1)
            rows = np.arange(floor((bounds.bottom - high[1]) / spacing) - 1, floor((bounds.top - low[1]) / spacing) + 1)
            cells = np.stack(np.meshgrid(columns, rows), axis=-1).reshape(-1, 2)
            keys = np.column_stack((cells, np.full(len(cells), index)))
            draws = tiles.uniforms(keys, tiles.stream(Particles, self.seed), 7)
            kept = (draws[:, 0] < self.population.density) & drawn
            depths = midpoint + self.depth_variation * (draws[kept, 6] - 0.5) / self.layers
            scales = self.depth ** (-depths)
            return (keys[kept], depths, scales, *self.population.placed(cells[kept], draws[kept], spacing, scales), draws[kept, 3])

        keys, depths, scales, centers, radii, gains, tones = (np.concatenate(columns) for columns in zip(*map(layer, range(self.layers))))
        order = np.lexsort((keys[:, 0], keys[:, 1], keys[:, 2], depths))
        return Layered(keys=keys[order], scales=scales[order], centers=centers[order], radii=radii[order], gains=gains[order], tones=tones[order])

    def applied(
        self,
        identity: Identity,
        plane: OpenImageIO.ImageBuf,
        bounds: Region,
        *,
        frame: float,
        rate: float,
        shutter: tuple[float, float],
        precision: Precision,
        linear_rgb: Callable[[Spectrum, npt.NDArray[np.float64]], Outcome[npt.NDArray[np.float64]]],
    ) -> Outcome[Estimate[OpenImageIO.ImageBuf]]:
        """Integrates particle supports along their shutter paths into the associated plane, nearer particles occluding farther ones.

        Args:
            identity: Extension owning the scratch trees evaluating wind, twinkle, and ramps.
            plane: Associated RGBA, rows top first.
            bounds: Data window in original camera units.
            frame: Scene frame the clocks hold.
            rate: Scene frames per second the clocks count in.
            shutter: Shutter opening and closing in frames from the held frame.
            precision: Sobol resolution and independent error replicates.
            linear_rgb: Scene-linear RGB of a spectrum at tones.

        Returns:
            Estimated plane with replicate error, or image and color faults.
        """
        held = self.clock.held(frame)
        opening, closing = shutter if self.motion_blur else (0.0, 0.0)
        heading = np.array((cos(self.direction), sin(self.direction)))
        focus = self.depth ** (-self.focus)
        low, high = density_range(self.wind)
        reach = max(abs(2.0 * low - 1.0), abs(2.0 * high - 1.0))
        channel = self.channels
        drawn = self.layered(bounds, held, (opening, closing), reach, rate)
        optical = drawn.radii + self.defocus * np.abs(drawn.scales - focus)
        extent = optical * channel.max() * max(1.0, 1.0 / self.squeeze) + drawn.scales * (self.clock.speed / rate * max(abs(held + opening), abs(held + closing)) + self.turbulence * reach)
        emitting = len(drawn.keys) > 0 and self.emission is not None

        def moved(selected: npt.NDArray[np.intp], times: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            rest: npt.NDArray[np.float64] = drawn.centers[selected] + times[:, None] * heading * (self.clock.speed / rate) * drawn.scales[selected, None]
            if self.turbulence == 0.0 or len(selected) == 0:
                return rest - (bounds.left, bounds.bottom)
            points, evolved = rest / drawn.scales[selected, None], self.evolution * times
            sway = np.stack([evaluated(identity, self.wind, points, evolved, seed=self.seed, stream=tiles.stream(Particles, "wind", axis))[:, 0] for axis in range(2)], axis=-1)
            return rest - (bounds.left, bounds.bottom) + self.turbulence * drawn.scales[selected, None] * (2.0 * sway - 1.0)

        def profiles(selected: npt.NDArray[np.intp], offsets: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            scaled = (offsets * (self.squeeze, 1.0))[..., None, :] / (optical[selected, None, None, None] * np.where(channel > 0.0, channel, 1.0)[:, None])
            profile: npt.NDArray[np.float64] = self.iris.profile(scaled[..., 0], scaled[..., 1], self.population.softness, np)
            return np.where(channel > 0.0, profile, 0.0)

        def modulated(selected: npt.NDArray[np.intp], times: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            if not (self.emission is not None and self.twinkle > 0.0) or len(selected) == 0:
                return np.ones(len(times), dtype=np.float64)
            moments = self.scintillation.clock(frame, rate) + self.scintillation.speed / rate * (times - held)
            twinkle = textures.Noise(noise_dimensions="4D", scale=1.0, detail=0.0).node(POSITION, W)
            (field,) = evaluated_nodes(identity, {"position": drawn.keys[selected].astype(np.float64), "w": moments}, twinkle)
            return 1.0 - self.twinkle * field.astype(np.float64)

        def split(values: npt.NDArray[np.float64], sizes: list[int]) -> list[npt.NDArray[np.float64]]:
            return np.split(values, np.cumsum(sizes)[:-1]) if sizes else []

        def rendered(source: npt.NDArray[np.float64], coating: npt.NDArray[np.float64], light: npt.NDArray[np.float64], thermal: npt.NDArray[np.float64]) -> Outcome[Estimate[OpenImageIO.ImageBuf]]:
            height, width = source.shape[:2]
            dx, dy = (bounds.right - bounds.left) / width, (bounds.top - bounds.bottom) / height
            illuminant = light * (1.0 - self.incandescence + self.incandescence * thermal)
            radiance = exp2(self.emission.intensity) if self.emission is not None else 0.0
            unit_area, _ = self.iris.integral(self.population.softness, epsabs=precision.absolute, epsrel=precision.relative) if emitting else (1.0, 0.0)
            count = len(drawn.keys)

            def deposited(observation: npt.NDArray[np.float64], replicate: int) -> None:
                net = (precision.points(3, replicate)[None] + tiles.uniforms(drawn.keys, tiles.stream(Precision, precision.seed, replicate), 3)[:, None, :]) % 1.0
                samples = net.shape[1]
                times = held + opening + (closing - opening) * net[..., 2]
                radius, angle = optical[:, None] * np.sqrt(net[..., 0]), tau * net[..., 1]
                offsets = np.stack((radius * np.cos(angle) / self.squeeze, radius * np.sin(angle)), axis=-1)
                positions = moved(np.repeat(np.arange(count), samples), times.reshape(-1)).reshape(count, samples, 2) + offsets
                inside = (positions[..., 0] >= 0.0) & (positions[..., 0] < width * dx) & (positions[..., 1] >= 0.0) & (positions[..., 1] < height * dy)
                active = np.flatnonzero(inside.any(axis=1))
                kept = [times[index][inside[index]] for index in active]
                nearer = [np.flatnonzero(np.linalg.norm(drawn.centers[:index] - drawn.centers[index], axis=-1) <= extent[:index] + extent[index]) for index in active]
                pairs = [len(near) * len(held_times) for near, held_times in zip(nearer, kept)]
                occluders = split(
                    moved(
                        np.concatenate([np.repeat(near, len(held_times)) for near, held_times in zip(nearer, kept)] or [np.zeros(0, dtype=np.intp)]),
                        np.concatenate([np.tile(held_times, len(near)) for near, held_times in zip(nearer, kept)] or [np.zeros(0)]),
                    ),
                    pairs,
                )
                twinkles = split(modulated(np.repeat(active, [len(held_times) for held_times in kept]), np.concatenate(kept or [np.zeros(0)])), [len(held_times) for held_times in kept])
                weight = pi * optical**2 / self.squeeze / (dx * dy) / samples
                for slot, index in enumerate(active):
                    selected, arrived, near = inside[index], positions[index][inside[index]], nearer[slot]
                    columns = np.floor(arrived[:, 0] / dx).astype(np.intp)
                    rows = height - 1 - np.floor(arrived[:, 1] / dy).astype(np.intp)
                    profile = profiles(np.array((index,)), offsets[index][None, selected])[0]
                    opacity = drawn.gains[index] * np.mean(profile, axis=-1)
                    pixel = source[rows, columns]
                    temporal = (1.0 - self.fade + self.fade * net[index, selected, 2]) / (1.0 - self.fade / 2.0) if opening != closing else np.ones(len(arrived), dtype=np.float64)
                    occluded = occluders[slot].reshape(len(near), len(kept[slot]), 2)
                    transmittance = transmitted(drawn.gains[near], np.mean(profiles(near, arrived[None] - occluded), axis=-1))
                    contribution = np.zeros((len(arrived), 4), dtype=np.float64)
                    if self.coverage is not None:
                        contribution[:, :3] = opacity[:, None] * (coating[index] - pixel[:, :3])
                        contribution[:, 3] = opacity * (1.0 - pixel[:, 3])
                    if self.emission is not None:
                        energy = radiance * drawn.gains[index] * pi * drawn.radii[index] ** 2
                        area = unit_area * np.square(optical[index] * channel) / self.squeeze
                        peak = np.divide(energy * illuminant[index], area, out=np.zeros(3, dtype=np.float64), where=channel > 0.0)
                        contribution[:, :3] += profile * peak * twinkles[slot][:, None]
                    np.add.at(observation, (rows, columns), contribution * (weight[index] * temporal * transmittance)[:, None])

            observations = np.broadcast_to(source, (precision.replicates, *source.shape)).copy()
            for replicate in range(precision.replicates):
                deposited(observations[replicate], replicate)
            estimate = Estimate.of(observations, precision)
            return buffers.located(estimate.value.astype(np.float32), plane).map(estimate.holding)

        zeros, ones = np.zeros((len(drawn.keys), 3), dtype=np.float64), np.ones((len(drawn.keys), 3), dtype=np.float64)
        return accumulated((
            buffers.pixels(plane).map(lambda values: values.astype(np.float64)),
            linear_rgb(self.coverage, drawn.tones) if self.coverage is not None else Ok(zeros),
            linear_rgb(self.emission.spectrum, drawn.tones) if self.emission is not None else Ok(zeros),
            linear_rgb(self.temperature, drawn.tones) if self.emission is not None and self.incandescence > 0.0 else Ok(ones),
        )).bind(lambda parts: rendered(*parts))


# --- [OPERATIONS] -----------------------------------------------------------------------


@prepared.register
def _(
    record: Particles, identity: Identity, values: Mapping[Key, Value], config: PyOpenColorIO.Config, image: OpenImageIO.ImageBuf, precision: Precision, frame: CameraFrame, scene: bpy.types.Scene, /
) -> Outcome[Option[OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]:
    """Integrates particle light over the captured window and the scene shutter."""
    timing = Shutter.of(scene)
    return (
        decoded(record, values)
        .bind(
            lambda particles: particles.applied(
                identity,
                image,
                frame.window(image.spec())[0],
                frame=scene.frame_current_final,
                rate=scene.render.fps / scene.render.fps_base,
                shutter=(timing.opening * timing.length, (timing.opening + 1.0) * timing.length),
                precision=precision,
                linear_rgb=lambda request, tones: spectrum(identity, request, tones, config),
            )
        )
        .map(Some)
    )


def strewn(shell: Shell[Particles]) -> Outcome[Expr]:
    """Reads the image the CPU render prepared, the stage input outside one."""
    return Ok(shell.color if shell.record.staged is None else Expr(kind=bpy.types.CompositorNodeImage, members={"image": shell.record.staged}, socket="Image", shape=bpy.types.NodeSocketColor))


# --- [COMPOSITION] ----------------------------------------------------------------------

particles: Final = CompositorEffect(strewn, record=Particles, interface=_FACE, mix=Blended(alpha=Alpha.RESULT), request=Some(precise))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Layered", "Particles", "particles"]
