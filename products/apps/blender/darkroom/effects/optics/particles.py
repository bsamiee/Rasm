"""Soft points in depth layers covering the frame or emitting light, integrated over pixels, the iris, and the scene shutter with ordered occlusion."""

from collections.abc import Callable
from functools import cache, partial
from math import cos, exp2, floor, pi, sin, tau
from typing import Annotated, Final, get_args, override

import bpy
from expression import Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
from scipy.spatial import cKDTree

from rasm_bpy.data import Identity
from rasm_bpy.nodes import decoded, fielded, Interface, InterfacePanel, item_key, POSITION, sampled, textures, W
from rasm_bpy.render import chosen, Clock, CompositorEffect, CompositorRecord, lenses, Mix, origin, precise, Prepared, rate, Shutter, Stage, toggled
from rasm_imaging import Aperture, Estimate, fringed, located, pixels, Population, Precision, Region, Seed, stream, transmitted, uniforms
from rasm_records import accumulated, AtLeastOne, Error, Fault, Finite, NonNegative, Nothing, Ok, Outcome, paired, Positive, Reason, Unit

from . import bases, emission
from .bases import Basis, drawn, Perlin, TEXTURE, Voronoi
from .emission import colored, Emission, Incandescence, spectra, SPECTRUM, Spectrum, spectrum, Tint

# --- [MODELS] ---------------------------------------------------------------------------


class Particles(CompositorRecord, frozen=True, kw_only=True):
    """Soft points in depth layers covering frame or emitting light: rain, snow, sparks, dust, and stars, every field an entry input the CPU render decodes, the wind basis and coverage ramp holding their node settings."""

    # --- [SETTINGS]

    incandescence: Annotated[Unit, msgspec.Meta(description="Blackbody share of emitted color")] = 0.0
    direction: Annotated[Finite, bpy.types.NodeSocketFloatAngle, msgspec.Meta(description="Particle travel direction")] = -pi / 2.0
    turbulence: Annotated[NonNegative, msgspec.Meta(description="Nearest layer sway over greater frame side")] = 0.0
    evolution: Annotated[Finite, msgspec.Meta(description="Signed wind field evolution per second")] = 0.0
    motion_blur: Annotated[bool, msgspec.Meta(description="Streaks each particle across scene shutter")] = True
    layers: Annotated[int, msgspec.Meta(ge=1, description="Depth layers ordered near to far")] = 3
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

    # --- [POPULATION]

    @property
    def shown(self) -> bool:
        """Whether the population can cover or light the frame through the iris."""
        return self.population.density > 0.0 and self.iris.obstruction < 1.0 and (self.coverage is not None or self.emission is not None)

    @property
    def heading(self) -> npt.NDArray[np.float64]:
        """Unit travel direction."""
        return np.array((cos(self.direction), sin(self.direction)))

    @property
    def focal(self) -> float:
        """Depth scale of the focused depth."""
        return float(self.depth ** (-self.focus))

    def layered(
        self, bounds: Region, held: float, shutter: tuple[float, float], reach: float, rate: float
    ) -> tuple[npt.NDArray[np.int64], npt.NDArray[np.float64], npt.NDArray[np.float64], npt.NDArray[np.float64], npt.NDArray[np.float64], npt.NDArray[np.float64]]:
        """Draws occupied cells that can reach bounds across shutter, ordered near to far by depth then lattice key.

        Returns:
            Lattice keys, depth scales, centers, radii, gains, and tones of the occupied cells.
        """
        if not self.shown:
            empty = np.empty(0, dtype=np.float64)
            return np.empty((0, 3), dtype=np.int64), empty, np.empty((0, 2), dtype=np.float64), empty, empty, empty
        velocity, focal = self.heading * (self.clock.speed / rate), self.focal

        def layer(index: int) -> tuple[npt.NDArray[np.int64] | npt.NDArray[np.float64], ...]:
            midpoint = (index + 0.5) / self.layers
            near, far = (self.depth ** (-(midpoint + side * self.depth_variation / (2.0 * self.layers))) for side in (-1.0, 1.0))
            spacing = self.population.spacing * self.depth ** (-midpoint)
            extent = self.population.spacing * near * self.population.size / 2.0 + self.defocus * max(abs(near - focal), abs(far - focal))
            support = np.array((extent / self.squeeze, extent))
            travel = np.outer((near, far), held + np.asarray(shutter)).reshape(-1, 1) * velocity
            low, high = travel.min(axis=0) - self.turbulence * near * reach - support, travel.max(axis=0) + self.turbulence * near * reach + support
            columns = np.arange(floor((bounds.left - high[0]) / spacing) - 1, floor((bounds.right - low[0]) / spacing) + 1, dtype=np.int64)
            rows = np.arange(floor((bounds.bottom - high[1]) / spacing) - 1, floor((bounds.top - low[1]) / spacing) + 1, dtype=np.int64)
            cells = np.stack(np.meshgrid(columns, rows), axis=-1).reshape(-1, 2)
            keys = np.column_stack((cells, np.full(len(cells), index)))
            draws = uniforms(keys, stream(Particles), 7, seed=self.seed)
            kept = draws[:, 0] < self.population.density
            depths = midpoint + self.depth_variation * (draws[kept, 6] - 0.5) / self.layers
            scales = self.depth ** (-depths)
            x, y, radii, gains = self.population.placed((cells[kept, 0].astype(np.float64), cells[kept, 1].astype(np.float64)), tuple(draws[kept, :6].T), np.exp2, spacing=spacing, scale=scales)
            return keys[kept], depths, scales, np.column_stack((x, y)), radii, gains, draws[kept, 3]

        keys, depths, scales, centers, radii, gains, tones = (np.concatenate(columns) for columns in zip(*map(layer, range(self.layers))))
        order = np.lexsort((keys[:, 0], keys[:, 1], keys[:, 2], depths))
        return keys[order], scales[order], centers[order], radii[order], gains[order], tones[order]

    # --- [INTEGRATION]

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
            Estimated plane with replicate error, or image, color, and node evaluation faults. The image aliases the unchanged input when the population cannot contribute.
        """
        if not self.shown:
            return pixels(plane).map(lambda values: Estimate.of(np.broadcast_to(values.astype(np.float64), (precision.replicates, *values.shape)), precision).holding(plane))
        held = self.clock.held(frame)
        opening, closing = shutter if self.motion_blur else (0.0, 0.0)
        match (
            Ok(0.0)
            if self.turbulence == 0.0
            else (
                paired(self.wind.cut.bounds(), self.wind.feature_cells.color_bounds()).map(
                    lambda spans: (float((products := np.multiply.outer(*spans, dtype=np.float32)).min()), float(products.max()))
                )
                if isinstance(self.wind, Voronoi) and self.wind.cell_color
                else self.wind.cut.bounds()
            ).map(lambda span: max(abs(2.0 * value - 1.0) for value in span))
        ):
            case Result(tag="ok", ok=reach):
                channel = np.array(fringed(np.asarray(self.fringe), np))
            case Result(error=faults):
                return Error(faults)
        keys, scales, rest_centers, radii, gains, tones = self.layered(bounds, held, (opening, closing), reach, rate)
        count = len(keys)
        optical = radii + self.defocus * np.abs(scales - self.focal)
        speed, heading = self.clock.speed / rate, self.heading
        centers = rest_centers + held * heading * speed * scales[:, None]
        sway = self.turbulence * scales * reach
        extent = optical * max(1.0, 1.0 / self.squeeze) + scales * (speed * max(abs(opening), abs(closing))) + 2.0**0.5 * sway
        tree = cKDTree(centers)
        largest_extent = extent.max(initial=0.0)

        @cache
        def neighbors(index: np.intp) -> npt.NDArray[np.intp]:
            near = np.asarray(tree.query_ball_point(centers[index], extent[index] + largest_extent, return_sorted=True), dtype=np.intp)
            return np.extract((near < index) & (np.linalg.norm(centers[near] - centers[index], axis=-1) <= extent[near] + extent[index]), near)

        def moved(selected: npt.NDArray[np.intp], times: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float64]]:
            rest: npt.NDArray[np.float64] = rest_centers[selected] + times[:, None] * heading * speed * scales[selected, None]
            if self.turbulence == 0.0 or len(selected) == 0:
                return Ok(rest - (bounds.left, bounds.bottom))
            points, evolved = rest / scales[selected, None], self.evolution * times / rate
            wind = sampled(
                identity,
                {"position": np.column_stack((points, np.zeros(len(points)))), "w": evolved},
                *(drawn(self.wind, POSITION, W, origin(float(self.seed), float(stream(Particles.wind, axis))), Nothing)[0] for axis in range(2)),
            )
            return (
                wind
                .map(lambda fields: np.stack(fields, axis=-1, dtype=np.float64))
                .filter_with(lambda fields: bool(np.isfinite(fields).all()), partial(Fault.of, Reason.OUTSIDE, item_key(type(self), Particles.wind), member="finite density"))
                .map(lambda fields: rest - (bounds.left, bounds.bottom) + self.turbulence * scales[selected, None] * (2.0 * fields - 1.0))
            )

        def profiles(selected: npt.NDArray[np.intp], offsets: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            scaled = (offsets * (self.squeeze, 1.0))[..., None, :] / (optical[selected, None, None, None] * np.where(channel > 0.0, channel, 1.0)[:, None])
            profile: npt.NDArray[np.float64] = self.iris.profile(scaled[..., 0], scaled[..., 1], self.population.softness, np)
            return np.where(channel > 0.0, profile, 0.0)

        def modulated(selected: npt.NDArray[np.intp], times: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float64]]:
            if not (self.emission is not None and self.twinkle > 0.0):
                return Ok(np.ones(len(times), dtype=np.float64))
            moments = self.scintillation.clock(frame, rate) + self.scintillation.speed / rate * (times - held)
            twinkle = textures.Noise(noise_dimensions="4D", scale=1.0, detail=0.0).node(POSITION, W)
            return sampled(identity, {"position": keys[selected].astype(np.float64), "w": moments}, twinkle).map(lambda fields: 1.0 - self.twinkle * fields[0].astype(np.float64))

        def rendered(source: npt.NDArray[np.float64], coating: npt.NDArray[np.float64], light: npt.NDArray[np.float64], thermal: npt.NDArray[np.float64]) -> Outcome[Estimate[OpenImageIO.ImageBuf]]:
            height, width = source.shape[:2]
            dx, dy = (bounds.right - bounds.left) / width, (bounds.top - bounds.bottom) / height
            illuminant = light * (1.0 - self.incandescence + self.incandescence * thermal)
            radiance = exp2(self.emission.intensity) if self.emission is not None else 0.0
            unit_area, _ = self.iris.integral(self.population.softness, epsabs=precision.absolute, epsrel=precision.relative) if count and self.emission is not None else (1.0, 0.0)
            weight = pi * optical**2 / self.squeeze / (dx * dy)

            def deposited(observation: npt.NDArray[np.float64], replicate: int) -> Outcome[None]:
                net = (precision.points(3, replicate)[None] + uniforms(keys, stream(Precision, precision.seed, replicate), 3)[:, None, :]) % 1.0
                samples = net.shape[1]
                times = held + opening + (closing - opening) * net[..., 2]
                radius, angle = optical[:, None] * np.sqrt(net[..., 0]), tau * net[..., 1]
                offsets = np.stack((radius * np.cos(angle) / self.squeeze, radius * np.sin(angle)), axis=-1)

                def placed(rest: npt.NDArray[np.float64]) -> Outcome[None]:
                    positions = rest.reshape(count, samples, 2) + offsets
                    inside = (positions[..., 0] >= 0.0) & (positions[..., 0] < width * dx) & (positions[..., 1] >= 0.0) & (positions[..., 1] < height * dy)
                    active = np.flatnonzero(inside.any(axis=1))
                    if len(active) == 0:
                        return Ok(None)
                    kept = [times[index][inside[index]] for index in active]
                    sizes = [len(held_times) for held_times in kept]
                    nearer = [neighbors(index) for index in active]
                    pairs = [len(near) * size for near, size in zip(nearer, sizes)]
                    occluding = moved(np.concatenate(list(map(np.repeat, nearer, sizes))), np.concatenate([np.tile(held_times, len(near)) for near, held_times in zip(nearer, kept)]))
                    flickering = modulated(np.repeat(active, sizes), np.concatenate(kept))
                    match paired(occluding, flickering):
                        case Result(tag="ok", ok=(blocked, flicker)):
                            occluders, twinkles = np.split(blocked, np.cumsum(pairs)[:-1]), np.split(flicker, np.cumsum(sizes)[:-1])
                        case Result(error=faults):
                            return Error(faults)
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
                    return Ok(None)

                return moved(np.repeat(np.arange(count), samples), times.reshape(-1)).bind(placed)

            observations = np.broadcast_to(source, (precision.replicates, *source.shape)).copy()
            return (
                accumulated(deposited(observations[replicate], replicate) for replicate in range(precision.replicates))
                .map(lambda _: Estimate.of(observations, precision))
                .bind(lambda estimate: located(estimate.value.astype(np.float32), plane).map(estimate.holding))
            )

        zeros, ones = np.zeros((count, 3), dtype=np.float64), np.ones((count, 3), dtype=np.float64)
        return accumulated((
            pixels(plane).map(lambda values: values.astype(np.float64)),
            linear_rgb(self.coverage, tones) if self.coverage is not None else Ok(zeros),
            linear_rgb(self.emission.spectrum, tones) if self.emission is not None else Ok(zeros),
            linear_rgb(self.temperature, tones) if self.emission is not None and self.incandescence > 0.0 else Ok(ones),
        )).bind(lambda parts: rendered(*parts))

    # --- [RENDERING]

    requested = staticmethod(precise)

    @property
    @override
    def prepared(self) -> Callable[[Stage], Outcome[Option[Prepared]]]:
        """Integrates particle light over the captured window and the scene shutter."""

        def prepared(stage: Stage) -> Outcome[Option[Prepared]]:
            timing = Shutter.of(stage.scene)
            covered = toggled(stage.values, lambda: colored(stage.values, self.coverage or Tint(), _COVERAGE), _COVERAGE)
            light = self.emission or Emission()
            lit = toggled(
                stage.values,
                lambda: colored(stage.values, light.spectrum, _EMISSION, SPECTRUM).bind(lambda held: decoded(msgspec.structs.replace(light, spectrum=held), stage.values, _EMISSION.key)),
                _EMISSION,
            )
            wind = (
                chosen(stage.values, get_args(Basis.__value__), _WIND, TEXTURE)
                .map(lambda kind: self.wind if isinstance(self.wind, kind) else kind())
                .bind(
                    lambda case: paired(
                        decoded(case.texture, stage.values, item_key(_WIND.key, type(case), type(case).texture)), decoded(case.cut, stage.values, item_key(_WIND.key, type(case), type(case).cut))
                    ).bind(lambda parts: decoded(msgspec.structs.replace(case, texture=parts[0], cut=parts[1]), stage.values, item_key(_WIND.key, type(case))))
                )
            )
            return (
                paired(decoded(self, stage.values), covered, lit, wind)
                .map(lambda parts: msgspec.structs.replace(parts[0], coverage=parts[1].to_optional(), emission=parts[2].to_optional(), wind=parts[3]))
                .bind(
                    lambda particles: particles.applied(
                        stage.identity,
                        stage.image,
                        stage.rasters[0].bounds,
                        frame=stage.scene.frame_current_final,
                        rate=rate(stage.scene),
                        shutter=(timing.opening * timing.length, (timing.opening + 1.0) * timing.length),
                        precision=stage.precision,
                        linear_rgb=partial(spectrum, stage.identity, config=stage.config),
                    )
                )
                .map(Some)
            )

        return prepared


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

# --- [COMPOSITION] ----------------------------------------------------------------------

particles: Final = CompositorEffect(record=Particles, interface=_INTERFACE, mix=Mix.RESULT)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Particles", "particles"]
