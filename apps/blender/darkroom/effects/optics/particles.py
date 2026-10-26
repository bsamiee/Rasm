"""Soft points in depth layers covering the frame or emitting light, integrated over pixels, the iris, and the scene shutter with ordered occlusion."""

from collections.abc import Callable
from math import cos, exp2, floor, pi, sin, tau
from typing import Annotated

import bpy
from expression import Nothing, Ok, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes import textures
from blender.bpy.render.compositor.coordinates import Clock, Shutter
from blender.bpy.render.compositor.effects import Alpha, CameraFrame, CompositorEffect, precise, Prepared, Prepares, Requests, Shell
from blender.bpy.settings.shapes import GraphForm
from blender.imaging.files import images
from blender.imaging.geometry.lenses import Aperture
from blender.imaging.geometry.regions import FrameLength
from blender.imaging.sampling.integration import Estimate, Precision
from blender.imaging.sampling.populations import Population, transmitted
from blender.records.faults import accumulated
from blender.records.fields import AtLeastOne, Finite, Gated, Positive, Presentation, Quantity, Seed, Unit

from ...chain.stages import Effect, Stage
from .procedural import Basis, density_range, Emission, evaluated, Incandescence, Perlin, Spectrum, spectrum, Tint

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
    """Soft points in depth layers covering frame or emitting light: rain, snow, sparks, dust, and stars."""

    stage = Stage.GENERATE
    pointwise = False
    population: Annotated[Population, msgspec.Meta(description="Particle layout, size, and edge")] = Population()
    coverage: Annotated[Gated[Spectrum], msgspec.Meta(description="Scene-linear cover color by particle tone")] = Gated(enabled=True, value=Tint())
    emission: Annotated[Gated[Emission], msgspec.Meta(description="Light a particle adds")] = Gated(enabled=False, value=Emission())
    incandescence: Annotated[Unit, msgspec.Meta(description="Blackbody share of emitted color")] = 0.0
    temperature: Annotated[Incandescence, msgspec.Meta(description="Blackbody band multiplying emitted spectrum")] = Incandescence()
    clock: Annotated[Clock, msgspec.Meta(description="Nearest layer travel in greater frame sides per scene frame")] = Clock(speed=2.5 / 24.0)
    direction: Annotated[Finite, msgspec.Meta(description="Particle travel direction"), Presentation(quantity=Some(Quantity.ANGLE))] = -pi / 2.0
    turbulence: Annotated[FrameLength, msgspec.Meta(description="Nearest layer sway over greater frame side"), Presentation(soft=Some((0.0, 0.1)), precision=Some(4))] = 0.0
    wind: Annotated[Basis, msgspec.Meta(description="Texture displacing each axis independently")] = Perlin(texture=textures.Noise(noise_dimensions="4D", scale=3.2))
    evolution: Annotated[Finite, msgspec.Meta(description="Signed wind field evolution per scene frame")] = 0.0
    motion_blur: Annotated[bool, msgspec.Meta(description="Streaks each particle across scene shutter")] = True
    layers: Annotated[int, msgspec.Meta(ge=1, description="Depth layers drawn from far to near"), GraphForm.SETTING] = 3
    depth: Annotated[AtLeastOne, msgspec.Meta(description="Farthest layer distance over nearest"), Presentation(soft=Some((1.0, 16.0)), precision=Some(2))] = 4.0
    depth_variation: Annotated[Unit, msgspec.Meta(description="Random depth within each layer interval")] = 0.0
    focus: Annotated[Unit, msgspec.Meta(description="Focused depth between nearest and farthest layers")] = 1.0
    defocus: Annotated[FrameLength, msgspec.Meta(description="Blur radius per reciprocal depth difference"), Presentation(soft=Some((0.0, 0.05)))] = 0.0
    iris: Annotated[Aperture, msgspec.Meta(description="Projected particle outline")] = Aperture()
    squeeze: Annotated[Positive, msgspec.Meta(description="Horizontal compression of particle outline")] = 1.0
    fringe: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0, description="Channel radius shift, toward blue above zero")] = 0.0
    fade: Annotated[Unit, msgspec.Meta(description="Trail share fading toward tail")] = 0.0
    twinkle: Annotated[Unit, msgspec.Meta(description="Light share flickering over time")] = 0.0
    scintillation: Annotated[Clock, msgspec.Meta(title="Twinkle Clock", description="Independent twinkle field rate and hold")] = Clock(speed=0.0)
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting pattern")] = 0

    @property
    def channels(self) -> npt.NDArray[np.float64]:
        """Red, green, and blue outline radius factors of the fringe."""
        return np.array((1.0 - max(self.fringe, 0.0), 1.0 - abs(self.fringe) / 2.0, 1.0 + min(self.fringe, 0.0)))

    def layered(self, bounds: tuple[float, float, float, float], held: float, shutter: tuple[float, float], reach: float) -> Layered:
        """Draws every layer's occupied cells able to reach the bounds across the shutter, sorted far to near by depth then lattice key."""
        left, bottom, right, top = bounds
        heading = np.array((cos(self.direction), sin(self.direction)))
        focus = self.depth ** (-self.focus)
        drawn = self.population.density > 0.0 and self.iris.obstruction < 1.0 and (self.coverage.enabled or self.emission.enabled)

        def layer(index: int) -> tuple[npt.NDArray[np.float64] | npt.NDArray[np.int64], ...]:
            midpoint = (index + 0.5) / self.layers
            near, far = (self.depth ** (-(midpoint + side * self.depth_variation / (2.0 * self.layers))) for side in (-1.0, 1.0))
            spacing = self.population.spacing * self.depth ** (-midpoint)
            extent = self.population.spacing * near * self.population.size / 2.0 + self.defocus * max(abs(near - focus), abs(far - focus))
            support = np.array((extent / self.squeeze, extent))
            travel = np.array([heading * self.clock.speed * scale * time for scale in (near, far) for time in (held + shutter[0], held + shutter[1])])
            low, high = travel.min(axis=0) - self.turbulence * near * reach - support, travel.max(axis=0) + self.turbulence * near * reach + support
            columns = np.arange(floor((left - high[0]) / spacing) - 1, floor((right - low[0]) / spacing) + 1)
            rows = np.arange(floor((bottom - high[1]) / spacing) - 1, floor((top - low[1]) / spacing) + 1)
            cells = np.stack(np.meshgrid(columns, rows), axis=-1).reshape(-1, 2)
            keys = np.column_stack((cells, np.full(len(cells), index)))
            draws = textures.uniforms(keys, textures.stream(Particles, self.seed), 7)
            kept = (draws[:, 0] < self.population.density) & drawn
            depths = midpoint + self.depth_variation * (draws[kept, 6] - 0.5) / self.layers
            scales = self.depth ** (-depths)
            return (keys[kept], depths, scales, *self.population.placed(cells[kept], draws[kept], spacing, scales), draws[kept, 3])

        keys, depths, scales, centers, radii, gains, tones = (np.concatenate(columns) for columns in zip(*map(layer, range(self.layers))))
        order = np.lexsort((keys[:, 0], keys[:, 1], keys[:, 2], depths))
        return Layered(keys=keys[order], scales=scales[order], centers=centers[order], radii=radii[order], gains=gains[order], tones=tones[order])

    def applied(
        self,
        plane: OpenImageIO.ImageBuf,
        bounds: tuple[float, float, float, float],
        *,
        frame: float,
        shutter: tuple[float, float],
        precision: Precision,
        colored: Callable[[Spectrum, npt.NDArray[np.float64]], BlenderResult[npt.NDArray[np.float64]]],
    ) -> BlenderResult[Estimate[OpenImageIO.ImageBuf]]:
        """Integrates particle supports along their shutter paths into the associated plane, nearer particles occluding farther ones.

        Args:
            plane: Associated RGBA, rows top first.
            bounds: Data window in original camera units as left, bottom, right, top.
            frame: Scene frame the clocks hold.
            shutter: Shutter opening and closing in frames from the held frame.
            precision: Sobol resolution and independent error replicates.
            colored: Scene-linear RGB of a spectrum at tones.

        Returns:
            Estimated plane with replicate error and emitted area normalization, or image and color faults.
        """
        left, bottom, right, top = bounds
        held = self.clock.held(frame)
        opening, closing = shutter if self.motion_blur else (0.0, 0.0)
        heading = np.array((cos(self.direction), sin(self.direction)))
        focus = self.depth ** (-self.focus)
        low, high = density_range(self.wind)
        reach = max(abs(2.0 * low - 1.0), abs(2.0 * high - 1.0))
        channel = self.channels
        drawn = self.layered(bounds, held, (opening, closing), reach)
        optical = drawn.radii + self.defocus * np.abs(drawn.scales - focus)
        extent = optical * channel.max() * max(1.0, 1.0 / self.squeeze) + drawn.scales * (self.clock.speed * max(abs(held + opening), abs(held + closing)) + self.turbulence * reach)
        emitting = len(drawn.keys) > 0 and self.emission.enabled

        def moved(selected: npt.NDArray[np.intp], times: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            rest = drawn.centers[selected, None, :] + times[None, :, None] * heading * self.clock.speed * drawn.scales[selected, None, None]
            if self.turbulence == 0.0:
                return rest - (left, bottom)
            points = (rest / drawn.scales[selected, None, None]).reshape(-1, 2)
            evolved = np.broadcast_to(self.evolution * times[None, :], rest.shape[:2]).reshape(-1)
            sway = np.stack([evaluated(self.wind, points, evolved, seed=self.seed, stream=textures.stream(Particles, "wind", axis))[:, 0] for axis in range(2)], axis=-1).reshape(rest.shape)
            return rest - (left, bottom) + self.turbulence * drawn.scales[selected, None, None] * (2.0 * sway - 1.0)

        def profiles(selected: npt.NDArray[np.intp], offsets: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            scaled = (offsets * (self.squeeze, 1.0))[..., None, :] / (optical[selected, None, None, None] * np.where(channel > 0.0, channel, 1.0)[:, None])
            return self.iris.profile(scaled[..., 0], scaled[..., 1], self.population.softness, np) * (channel > 0.0)

        def modulated(index: int, times: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            if not (self.emission.enabled and self.twinkle > 0.0):
                return np.ones(len(times), dtype=np.float64)
            moments = self.scintillation.clock(frame) + self.scintillation.speed * (times - held)
            field = textures.Noise(noise_dimensions="4D", scale=1.0, detail=0.0).evaluated(np.broadcast_to(drawn.keys[index], (len(times), 3)), moments)["Fac"]
            return 1.0 - self.twinkle * field.astype(np.float64)

        def rendered(
            source: npt.NDArray[np.float64], coating: npt.NDArray[np.float64], light: npt.NDArray[np.float64], thermal: npt.NDArray[np.float64]
        ) -> BlenderResult[Estimate[OpenImageIO.ImageBuf]]:
            height, width = source.shape[:2]
            dx, dy = (right - left) / width, (top - bottom) / height
            illuminant = light * (1.0 - self.incandescence + self.incandescence * thermal)
            radiance = exp2(self.emission.value.intensity) if self.emission.enabled else 0.0
            unit_area, area_error = self.iris.integral(self.population.softness, epsabs=precision.absolute, epsrel=precision.relative) if emitting else (1.0, 0.0)
            nets = tuple(precision.points(3, replicate) for replicate in range(precision.replicates))

            def deposited(observation: npt.NDArray[np.float64], replicate: int, index: int) -> None:
                net = (nets[replicate] + textures.uniforms(drawn.keys[index][None], textures.stream(Precision, precision.seed, replicate), 3)[0]) % 1.0
                times = held + opening + (closing - opening) * net[:, 2]
                radius, angle = optical[index] * np.sqrt(net[:, 0]), tau * net[:, 1]
                offsets = np.stack((radius * np.cos(angle) / self.squeeze, radius * np.sin(angle)), axis=-1)
                positions = moved(np.array((index,)), times)[0] + offsets
                selected = (positions[:, 0] >= 0.0) & (positions[:, 0] < width * dx) & (positions[:, 1] >= 0.0) & (positions[:, 1] < height * dy)
                if not np.any(selected):
                    return
                kept, arrived = times[selected], positions[selected]
                columns = np.floor(arrived[:, 0] / dx).astype(np.intp)
                rows = height - 1 - np.floor(arrived[:, 1] / dy).astype(np.intp)
                profile = profiles(np.array((index,)), offsets[None, selected])[0]
                opacity = drawn.gains[index] * np.mean(profile, axis=-1)
                pixel = source[rows, columns]
                temporal = (1.0 - self.fade + self.fade * net[selected, 2]) / (1.0 - self.fade / 2.0) if opening != closing else np.ones(len(kept), dtype=np.float64)
                weight = pi * optical[index] ** 2 / self.squeeze / (dx * dy) / len(net)
                nearer = np.flatnonzero(np.linalg.norm(drawn.centers[:index] - drawn.centers[index], axis=-1) <= extent[:index] + extent[index])
                transmittance = transmitted(drawn.gains[nearer], np.mean(profiles(nearer, arrived[None] - moved(nearer, kept)), axis=-1))
                contribution = np.zeros((len(kept), 4), dtype=np.float64)
                if self.coverage.enabled:
                    contribution[:, :3] = opacity[:, None] * (coating[index] - pixel[:, :3])
                    contribution[:, 3] = opacity * (1.0 - pixel[:, 3])
                if self.emission.enabled:
                    energy = radiance * drawn.gains[index] * pi * drawn.radii[index] ** 2
                    area = unit_area * np.square(optical[index] * channel) / self.squeeze
                    peak = np.divide(energy * illuminant[index], area, out=np.zeros(3, dtype=np.float64), where=channel > 0.0)
                    contribution[:, :3] += profile * peak * modulated(index, kept)[:, None]
                np.add.at(observation, (rows, columns), contribution * (weight * temporal * transmittance)[:, None])

            observations = np.broadcast_to(source, (precision.replicates, *source.shape)).copy()
            for replicate in range(precision.replicates):
                for index in range(len(drawn.keys)):
                    deposited(observations[replicate], replicate, index)
            estimate = msgspec.structs.replace(Estimate.of(observations, precision), normalization=Some((unit_area, area_error)) if emitting else Nothing)
            return images.located(estimate.value.astype(np.float32), plane).map(estimate.holding)

        zeros, ones = np.zeros((len(drawn.keys), 3), dtype=np.float64), np.ones((len(drawn.keys), 3), dtype=np.float64)
        return accumulated((
            images.pixels(plane).map(lambda values: values.astype(np.float64)),
            colored(self.coverage.value, drawn.tones) if self.coverage.enabled else Ok(zeros),
            colored(self.emission.value.spectrum, drawn.tones) if self.emission.enabled else Ok(zeros),
            colored(self.temperature, drawn.tones) if self.emission.enabled and self.incandescence > 0.0 else Ok(ones),
        )).bind(lambda parts: rendered(*parts))


# --- [OPERATIONS] -----------------------------------------------------------------------


def prepared(
    _identity: Identity,
    scene: bpy.types.Scene,
    record: Particles,
    config: PyOpenColorIO.Config,
    image: OpenImageIO.ImageBuf,
    precision: Precision,
    frame: CameraFrame,
    _entry: bpy.types.SceneCompositorEffect,
) -> BlenderResult[Prepared[Particles]]:
    """Integrates particle light over the captured window and the scene shutter, the record unchanged."""
    timing = Shutter.of(scene)
    return record.applied(
        image,
        frame.window(image.spec())[0],
        frame=scene.frame_current_final,
        shutter=(timing.opening * timing.length, (timing.opening + 1.0) * timing.length),
        precision=precision,
        colored=lambda request, tones: spectrum(request, tones, config),
    ).map(lambda estimate: (record, {Shell.image: estimate}))


# --- [COMPOSITION] ----------------------------------------------------------------------

particles = CompositorEffect(record=Particles, graph=Shell.passed, alpha=Alpha.RESULT, parts=(Requests[Particles](request=precise), Prepares(prepare=prepared)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Layered", "Particles", "particles"]
