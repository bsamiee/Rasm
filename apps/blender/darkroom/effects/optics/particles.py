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
from scipy.spatial import cKDTree

from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes import textures
from blender.bpy.nodes.desired import item_key
from blender.bpy.nodes.functions import POSITION, sampled, W
from blender.bpy.nodes.interfaces import decoded, fielded, Interface, InterfacePanel
from blender.bpy.render.compositor import lenses, motion
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, precise, Prepared, Stage
from blender.bpy.render.compositor.inputs import passed, toggled
from blender.bpy.render.compositor.motion import Clock, Shutter
from blender.imaging.files import buffers
from blender.imaging.geometry.frames import Region
from blender.imaging.geometry.lenses import Aperture, fringed
from blender.imaging.sampling import tiles
from blender.imaging.sampling.integration import Estimate, Population, Precision, transmitted
from blender.records.faults import accumulated, Outcome, paired
from blender.records.fields import AtLeastOne, Case, Finite, NonNegative, Positive, Seed, Unit

from . import bases, emission
from .bases import Basis, density_range, evaluated, Perlin
from .emission import colored, Emission, Incandescence, spectra, Spectrum, spectrum, Tint

# --- [TYPES] ----------------------------------------------------------------------------

type _Floats = npt.NDArray[np.float64]
type _Drawn = tuple[npt.NDArray[np.int64], _Floats, _Floats, _Floats, _Floats, _Floats]

# --- [MODELS] ---------------------------------------------------------------------------


class Particles(Case, frozen=True, kw_only=True):
    """Soft points in depth layers covering frame or emitting light: rain, snow, sparks, dust, and stars, every field an entry input the CPU render decodes, the wind basis and coverage ramp holding their node settings."""

    incandescence: Annotated[Unit, msgspec.Meta(description="Blackbody share of emitted color")] = 0.0
    direction: Annotated[Finite, bpy.types.NodeSocketFloatAngle, msgspec.Meta(description="Particle travel direction")] = -pi / 2.0
    turbulence: Annotated[NonNegative, msgspec.Meta(description="Nearest layer sway over greater frame side")] = 0.0
    evolution: Annotated[Finite, msgspec.Meta(description="Signed wind field evolution per second")] = 0.0
    motion_blur: Annotated[bool, msgspec.Meta(description="Streaks each particle across scene shutter")] = True
    layers: Annotated[int, msgspec.Meta(ge=1, description="Depth layers drawn from far to near")] = 3
    depth: Annotated[AtLeastOne, msgspec.Meta(description="Farthest layer distance over nearest")] = 4.0
    depth_variation: Annotated[Unit, msgspec.Meta(description="Random depth within each layer interval")] = 0.0
    focus: Annotated[Unit, msgspec.Meta(description="Focused depth between nearest and farthest layers")] = 1.0
    defocus: Annotated[NonNegative, msgspec.Meta(description="Blur radius per reciprocal depth difference")] = 0.0
    squeeze: Annotated[Positive, msgspec.Meta(description="Horizontal compression of particle outline")] = 1.0
    fringe: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0, description="Channel radius shift, toward blue above zero")] = 0.0
    fade: Annotated[Unit, msgspec.Meta(description="Trail share fading toward tail")] = 0.0
    twinkle: Annotated[Unit, msgspec.Meta(description="Light share flickering over time")] = 0.0
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting pattern")] = 0
    population: Population = Population()
    coverage: Spectrum | None = Tint()
    emission: Emission | None = None
    temperature: Incandescence = Incandescence()
    clock: Clock = Clock(speed=2.5)
    wind: Basis = Perlin(texture=textures.Noise(noise_dimensions="4D", scale=3.2))
    iris: Aperture = Aperture()
    scintillation: Clock = Clock(speed=0.0)

    @property
    def heading(self) -> _Floats:
        """Unit travel direction."""
        return np.array((cos(self.direction), sin(self.direction)))

    @property
    def focal(self) -> float:
        """Depth scale of the focused depth."""
        return float(self.depth ** (-self.focus))

    def layered(self, bounds: Region, held: float, shutter: tuple[float, float], reach: float, rate: float) -> _Drawn:
        """Draws every layer's occupied cells able to reach the bounds across the shutter, sorted far to near by depth then lattice key, the clock at `rate` scene frames per second.

        Returns:
            Lattice keys, depth scales, centers, radii, gains, and tones of the occupied cells.
        """
        shown = self.population.density > 0.0 and self.iris.obstruction < 1.0 and (self.coverage is not None or self.emission is not None)

        def layer(index: int) -> tuple[npt.NDArray[np.int64] | _Floats, ...]:
            midpoint = (index + 0.5) / self.layers
            near, far = (self.depth ** (-(midpoint + side * self.depth_variation / (2.0 * self.layers))) for side in (-1.0, 1.0))
            spacing = self.population.spacing * self.depth ** (-midpoint)
            extent = self.population.spacing * near * self.population.size / 2.0 + self.defocus * max(abs(near - self.focal), abs(far - self.focal))
            support = np.array((extent / self.squeeze, extent))
            travel = np.array([self.heading * self.clock.speed / rate * scale * time for scale in (near, far) for time in (held + shutter[0], held + shutter[1])])
            low, high = travel.min(axis=0) - self.turbulence * near * reach - support, travel.max(axis=0) + self.turbulence * near * reach + support
            columns = np.arange(floor((bounds.left - high[0]) / spacing) - 1, floor((bounds.right - low[0]) / spacing) + 1)
            rows = np.arange(floor((bounds.bottom - high[1]) / spacing) - 1, floor((bounds.top - low[1]) / spacing) + 1)
            cells = np.stack(np.meshgrid(columns, rows), axis=-1).reshape(-1, 2)
            keys = np.column_stack((cells, np.full(len(cells), index)))
            draws = tiles.uniforms(keys, tiles.stream(Particles), 7, seed=self.seed)
            kept = (draws[:, 0] < self.population.density) & shown
            depths = midpoint + self.depth_variation * (draws[kept, 6] - 0.5) / self.layers
            scales = self.depth ** (-depths)
            x, y, radii, gains = self.population.placed((cells[kept, 0], cells[kept, 1]), tuple(draws[kept, :6].T), np, spacing=spacing, scale=scales)
            return keys[kept], depths, scales, np.column_stack((x, y)), radii, gains, draws[kept, 3]

        keys, depths, scales, centers, radii, gains, tones = (np.concatenate(columns) for columns in zip(*map(layer, range(self.layers))))
        order = np.lexsort((keys[:, 0], keys[:, 1], keys[:, 2], depths))
        return keys[order], scales[order], centers[order], radii[order], gains[order], tones[order]

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
        linear_rgb: Callable[[Spectrum, _Floats], Outcome[_Floats]],
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
            Estimated plane with replicate error, or image, color, and node evaluation faults.
        """
        held = self.clock.held(frame)
        opening, closing = shutter if self.motion_blur else (0.0, 0.0)
        low, high = density_range(self.wind)
        reach = max(abs(2.0 * low - 1.0), abs(2.0 * high - 1.0))
        channel = np.array(fringed(np.asarray(self.fringe), np))
        keys, scales, centers, radii, gains, tones = self.layered(bounds, held, (opening, closing), reach, rate)
        count = len(keys)
        optical = radii + self.defocus * np.abs(scales - self.focal)
        extent = optical * channel.max() * max(1.0, 1.0 / self.squeeze) + scales * (self.clock.speed / rate * max(abs(held + opening), abs(held + closing)) + self.turbulence * reach)
        tree = cKDTree(centers) if count else None

        def moved(selected: npt.NDArray[np.intp], times: _Floats) -> Outcome[_Floats]:
            rest: _Floats = centers[selected] + times[:, None] * self.heading * (self.clock.speed / rate) * scales[selected, None]
            if self.turbulence == 0.0 or len(selected) == 0:
                return Ok(rest - (bounds.left, bounds.bottom))
            points, evolved = rest / scales[selected, None], self.evolution * times / rate
            sway = accumulated(evaluated(identity, self.wind, points, evolved, seed=self.seed, stream=tiles.stream(Particles.wind, axis)) for axis in range(2))
            return sway.map(lambda axes: rest - (bounds.left, bounds.bottom) + self.turbulence * scales[selected, None] * (2.0 * np.stack([held_axis[:, 0] for held_axis in axes], axis=-1) - 1.0))

        def profiles(selected: npt.NDArray[np.intp], offsets: _Floats) -> _Floats:
            scaled = (offsets * (self.squeeze, 1.0))[..., None, :] / (optical[selected, None, None, None] * np.where(channel > 0.0, channel, 1.0)[:, None])
            profile: _Floats = self.iris.profile(scaled[..., 0], scaled[..., 1], self.population.softness, np)
            return np.where(channel > 0.0, profile, 0.0)

        def modulated(selected: npt.NDArray[np.intp], times: _Floats) -> Outcome[_Floats]:
            if not (self.emission is not None and self.twinkle > 0.0) or len(selected) == 0:
                return Ok(np.ones(len(times), dtype=np.float64))
            moments = self.scintillation.clock(frame, rate) + self.scintillation.speed / rate * (times - held)
            twinkle = textures.Noise(noise_dimensions="4D", scale=1.0, detail=0.0).node(POSITION, W)
            return sampled(identity, {"position": keys[selected].astype(np.float64), "w": moments}, twinkle).map(lambda fields: 1.0 - self.twinkle * fields[0].astype(np.float64))

        def split(values: _Floats, sizes: list[int]) -> list[_Floats]:
            return np.split(values, np.cumsum(sizes)[:-1]) if sizes else []

        def rendered(source: _Floats, coating: _Floats, light: _Floats, thermal: _Floats) -> Outcome[Estimate[OpenImageIO.ImageBuf]]:
            height, width = source.shape[:2]
            dx, dy = (bounds.right - bounds.left) / width, (bounds.top - bounds.bottom) / height
            illuminant = light * (1.0 - self.incandescence + self.incandescence * thermal)
            radiance = exp2(self.emission.intensity) if self.emission is not None else 0.0
            unit_area, _ = self.iris.integral(self.population.softness, epsabs=precision.absolute, epsrel=precision.relative) if count and self.emission is not None else (1.0, 0.0)
            weight = pi * optical**2 / self.squeeze / (dx * dy)

            def deposited(observation: _Floats, replicate: int) -> Outcome[None]:
                net = (precision.points(3, replicate)[None] + tiles.uniforms(keys, tiles.stream(Precision, precision.seed, replicate), 3)[:, None, :]) % 1.0
                samples = net.shape[1]
                times = held + opening + (closing - opening) * net[..., 2]
                radius, angle = optical[:, None] * np.sqrt(net[..., 0]), tau * net[..., 1]
                offsets = np.stack((radius * np.cos(angle) / self.squeeze, radius * np.sin(angle)), axis=-1)

                def placed(rest: _Floats) -> Outcome[None]:
                    positions = rest.reshape(count, samples, 2) + offsets
                    inside = (positions[..., 0] >= 0.0) & (positions[..., 0] < width * dx) & (positions[..., 1] >= 0.0) & (positions[..., 1] < height * dy)
                    active = np.flatnonzero(inside.any(axis=1))
                    kept = [times[index][inside[index]] for index in active]
                    found = tree.query_ball_point(centers[active], extent[active] + extent.max(), return_sorted=True) if tree is not None and len(active) else []
                    nearer = [
                        held_near[np.linalg.norm(centers[held_near] - centers[index], axis=-1) <= extent[held_near] + extent[index]]
                        for index, near in zip(active, found, strict=True)
                        for held_near in (np.asarray([candidate for candidate in near if candidate < index], dtype=np.intp),)
                    ]
                    pairs = [len(near) * len(held_times) for near, held_times in zip(nearer, kept, strict=True)]
                    occluding = moved(
                        np.concatenate([np.repeat(near, len(held_times)) for near, held_times in zip(nearer, kept, strict=True)] or [np.zeros(0, dtype=np.intp)]),
                        np.concatenate([np.tile(held_times, len(near)) for near, held_times in zip(nearer, kept, strict=True)] or [np.zeros(0)]),
                    )
                    flickering = modulated(np.repeat(active, [len(held_times) for held_times in kept]), np.concatenate(kept or [np.zeros(0)]))
                    return paired(occluding, flickering).map(
                        lambda parts: added(positions, inside, active, kept, nearer, split(parts[0], pairs), split(parts[1], [len(held_times) for held_times in kept]))
                    )

                def added(
                    positions: _Floats,
                    inside: npt.NDArray[np.bool_],
                    active: npt.NDArray[np.intp],
                    kept: list[_Floats],
                    nearer: list[npt.NDArray[np.intp]],
                    occluders: list[_Floats],
                    twinkles: list[_Floats],
                ) -> None:
                    for slot, index in enumerate(active):
                        selected, arrived, near = inside[index], positions[index][inside[index]], nearer[slot]
                        columns = np.floor(arrived[:, 0] / dx).astype(np.intp)
                        rows = height - 1 - np.floor(arrived[:, 1] / dy).astype(np.intp)
                        profile = profiles(np.array((index,)), offsets[index][None, selected])[0]
                        opacity = gains[index] * np.mean(profile, axis=-1)
                        pixel = source[rows, columns]
                        temporal = (1.0 - self.fade + self.fade * net[index, selected, 2]) / (1.0 - self.fade / 2.0) if opening != closing else np.ones(len(arrived), dtype=np.float64)
                        occluded = occluders[slot].reshape(len(near), len(kept[slot]), 2)
                        transmittance = transmitted(gains[near], np.mean(profiles(near, arrived[None] - occluded), axis=-1))
                        contribution = np.zeros((len(arrived), 4), dtype=np.float64)
                        if self.coverage is not None:
                            contribution[:, :3] = opacity[:, None] * (coating[index] - pixel[:, :3])
                            contribution[:, 3] = opacity * (1.0 - pixel[:, 3])
                        if self.emission is not None:
                            energy = radiance * gains[index] * pi * radii[index] ** 2
                            area = unit_area * np.square(optical[index] * channel) / self.squeeze
                            peak = np.divide(energy * illuminant[index], area, out=np.zeros(3, dtype=np.float64), where=channel > 0.0)
                            contribution[:, :3] += profile * peak * twinkles[slot][:, None]
                        np.add.at(observation, (rows, columns), contribution * (weight[index] / samples * temporal * transmittance)[:, None])

                return moved(np.repeat(np.arange(count), samples), times.reshape(-1)).bind(placed)

            observations = np.broadcast_to(source, (precision.replicates, *source.shape)).copy()

            def estimated(_: tuple[None, ...]) -> Outcome[Estimate[OpenImageIO.ImageBuf]]:
                estimate = Estimate.of(observations, precision)
                return buffers.located(estimate.value.astype(np.float32), plane).map(estimate.holding)

            return accumulated(deposited(observations[replicate], replicate) for replicate in range(precision.replicates)).bind(estimated)

        zeros, ones = np.zeros((count, 3), dtype=np.float64), np.ones((count, 3), dtype=np.float64)
        return accumulated((
            buffers.pixels(plane).map(lambda values: values.astype(np.float64)),
            linear_rgb(self.coverage, tones) if self.coverage is not None else Ok(zeros),
            linear_rgb(self.emission.spectrum, tones) if self.emission is not None else Ok(zeros),
            linear_rgb(self.temperature, tones) if self.emission is not None and self.incandescence > 0.0 else Ok(ones),
        )).bind(lambda parts: rendered(*parts))


# --- [INTERFACES] -----------------------------------------------------------------------

_DEFAULTS: Final = Particles()
_COVERAGE: Final = spectra(item_key(Particles.coverage), Tint(), "Scene-linear cover color by particle tone", Some(True))
_EMISSION: Final = emission.emission(item_key(Particles.emission), Emission(), "Light a particle adds", Some(False))
_WIND: Final = bases.bases(item_key(Particles.wind), _DEFAULTS.wind, "Texture displacing each axis independently")
_INTERFACE: Final = Interface(
    inputs=fielded(_DEFAULTS).inputs,
    panels=(
        InterfacePanel.of(item_key(Particles.population), _DEFAULTS.population, description="Particle layout, size, and edge"),
        _COVERAGE,
        _EMISSION,
        InterfacePanel.of(item_key(Particles.temperature), _DEFAULTS.temperature, description="Blackbody band multiplying emitted spectrum"),
        InterfacePanel.of(item_key(Particles.clock), _DEFAULTS.clock),
        _WIND,
        lenses.IRIS,
        InterfacePanel.of(item_key(Particles.scintillation), _DEFAULTS.scintillation),
    ),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _held(record: Particles, values: Mapping[Key, Value]) -> Outcome[Particles]:
    """Particles the entry inputs hold, coverage and emission absent while their panels are off, the wind basis and coverage ramp keeping the record's node settings."""
    covered = toggled(values, lambda: colored(values, record.coverage or Tint(), _COVERAGE), _COVERAGE)
    lit = toggled(values, lambda: emission.selected(_EMISSION, record.emission or Emission(), values), _EMISSION)
    wind = bases.selected(_WIND, record.wind, values)
    return paired(decoded(record, values), covered, lit, wind).map(lambda parts: msgspec.structs.replace(parts[0], coverage=parts[1].to_optional(), emission=parts[2].to_optional(), wind=parts[3]))


def _drawn(stage: Stage[Particles]) -> Outcome[Option[Prepared]]:
    """Integrates particle light over the captured window and the scene shutter."""
    timing = Shutter.of(stage.scene)
    return (
        _held(stage.record, stage.values)
        .bind(
            lambda particles: particles.applied(
                stage.identity,
                stage.image,
                stage.frame.window(stage.image.spec())[0],
                frame=stage.scene.frame_current_final,
                rate=motion.rate(stage.scene),
                shutter=(timing.opening * timing.length, (timing.opening + 1.0) * timing.length),
                precision=stage.precision,
                linear_rgb=lambda request, tones: spectrum(stage.identity, request, tones, stage.config),
            )
        )
        .map(Some)
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

particles: Final = CompositorEffect(passed, record=Particles, interface=_INTERFACE, mix=Blended(alpha=Alpha.RESULT), prepared=Some(_drawn), request=Some(precise))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Particles", "particles"]
