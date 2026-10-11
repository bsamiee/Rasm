"""Soft points in depth layers covering the frame, emitting light, or both."""

from collections.abc import Callable, Iterable
from fractions import Fraction
import math
import sys
from typing import Annotated

from expression import Error, Nothing, Ok, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO

from rasm.imaging.chain import Effect, Stage
from rasm.imaging.effects.generators.noise import Basis, Cut, density_range, Emission, Estimate, Incandescence, Interpolation, Octaves, Perlin, Precision, sample, Spectrum, Tint, uniforms
from rasm.imaging.effects.optics.defocus import Aperture
from rasm.imaging.faults import accumulated, ImagingBufferFault, ImagingGeneratorFault, ImagingResult
from rasm.imaging.parameters import Finite, Gated, GraphForm, Motion, Presentation, Quantity, Seed

# --- [MODELS] -----------------------------------------------------------------------------


class Mark(msgspec.Struct, frozen=True, kw_only=True):
    """One stable population member with exact global center and physical radius."""

    cell: tuple[int, int]
    center: tuple[Fraction, Fraction]
    radius: float
    gain: float
    tone: float


class Particle(msgspec.Struct, frozen=True, kw_only=True):
    """Population member at its own depth with core and optical support."""

    mark: Mark
    layer: int
    depth: float
    scale: float
    optical: float


class Population(msgspec.Struct, frozen=True, kw_only=True):
    """Soft discs in Voronoi cells: their presence, size, fade, and edge."""

    spacing: Annotated[
        float,
        msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Spacing", description="Cell width at the nearest layer as a share of the frame's greater side"),
        Presentation(soft=Some((0.002, 0.1)), decimals=Some(4), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 0.01
    density: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Density", description="Share of cells holding a particle")] = 0.5
    size: Annotated[float, msgspec.Meta(gt=0.0, le=0.5, title="Size", description="Particle diameter as a share of its cell")] = 0.2
    variation: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Variation", description="Share each particle's diameter shrinks below the size")] = 0.5
    spread: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Spread", description="Stops each particle's coverage falls below full"),
        Presentation(soft=Some((0.0, 10.0)), step=Some(0.01), decimals=Some(3)),
    ] = 1.0
    softness: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Softness", description="Share of the radius the edge fades over before pixel filtering")] = 0.5

    def marks(self, bounds: tuple[Fraction, Fraction, Fraction, Fraction], *, seed: Seed, stream: int = 0) -> tuple[Mark, ...]:
        """Enumerates every cell whose disc can intersect positive global bounds."""
        left, bottom, right, top = bounds
        spacing = Fraction(self.spacing)
        radius = Fraction(self.size) / 2
        columns = range(math.floor(left / spacing - radius) - 1, math.floor(right / spacing + radius) + 1)
        rows = range(math.floor(bottom / spacing - radius) - 1, math.floor(top / spacing + radius) + 1)
        return self.sites(((column, line) for line in rows for column in columns), seed=seed, stream=stream)

    def sites(self, cells: Iterable[tuple[int, int]], *, seed: Seed, stream: int = 0) -> tuple[Mark, ...]:
        """Constructs occupied cell identities, exact centers, radii, and peak gains."""
        spacing = Fraction(self.spacing)
        return tuple(
            Mark(
                cell=cell,
                center=((Fraction(cell[0]) + Fraction(0.25 + 0.5 * float(draw[4]))) * spacing, (Fraction(cell[1]) + Fraction(0.25 + 0.5 * float(draw[5]))) * spacing),
                radius=self.spacing * self.size * (1.0 - self.variation * float(draw[1])) / 2.0,
                gain=math.exp2(-self.spread * float(draw[2])),
                tone=float(draw[3]),
            )
            for cell in cells
            for draw in (uniforms(cell, seed=seed, stream=stream, count=6),)
            if draw[0] < self.density
        )

    def integrated(
        self, bounds: tuple[float, float, float, float], *, seed: Seed, precision: Precision, stream: int = 0, paint: Callable[[float], float] = float
    ) -> ImagingResult[Estimate[npt.NDArray[np.float64]]]:
        """Estimates mean disc coverage and covered tone over a positive rectangle."""
        try:
            with np.errstate(over="raise", divide="raise", invalid="raise"):
                left, bottom, right, top = map(Fraction, bounds)
                area = float((right - left) * (top - bottom))
                marks = self.marks((left, bottom, right, top), seed=seed, stream=stream)
                estimates = np.zeros((precision.replicates, 2), dtype=np.float64)
                for replicate in range(precision.replicates):
                    for mark in marks:
                        points = precision.points(2, replicate, key=(*mark.cell, seed, stream))
                        radial = np.sqrt(points[:, 0])
                        angle = math.tau * points[:, 1]
                        x = mark.radius * radial * np.cos(angle)
                        y = mark.radius * radial * np.sin(angle)
                        inside = (x >= float(left - mark.center[0])) & (x < float(right - mark.center[0])) & (y >= float(bottom - mark.center[1])) & (y < float(top - mark.center[1]))
                        if self.softness == 0.0:
                            profile = np.ones_like(radial)
                        else:
                            rim = np.minimum((1.0 - radial) / self.softness, 1.0)
                            profile = rim * rim * (3.0 - 2.0 * rim)
                        mass = float(np.mean(inside * profile)) * math.pi * mark.radius**2 * mark.gain / area
                        estimates[replicate] += mass * np.array((1.0, paint(mark.tone)))
                return Ok(Estimate.of(estimates, precision))
        except (FloatingPointError, MemoryError, OverflowError, ValueError) as error:
            return Error((ImagingGeneratorFault(request=self, error=error),))


class Particles(Effect, frozen=True, kw_only=True, tag=33):
    """Soft points in depth layers covering the frame or emitting light: rain, snow, sparks, dust, and stars."""

    stage = Stage.GENERATE
    title = "Particles"
    pointwise = False
    population: Annotated[Population, msgspec.Meta(title="Population", description="Spacing, share, size, brightness, and edge of the particles")] = Population()
    coverage: Annotated[Gated[Spectrum], msgspec.Meta(title="Coverage", description="Scene-linear color and ramp coverage graded by particle tone")] = Gated(enabled=True, value=Tint())
    emission: Annotated[Gated[Emission], msgspec.Meta(title="Emission", description="Light a particle adds")] = Gated(enabled=False, value=Emission())
    incandescence: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Incandescence", description="Share of the emitted spectrum modulated by blackbody color")] = 0.0
    temperature: Annotated[Incandescence, msgspec.Meta(title="Temperature", description="Blackbody band multiplying the emitted spectrum")] = Incandescence()
    motion: Annotated[Motion, msgspec.Meta(title="Motion", description="Nearest layer's travel in shares of the frame's greater side per scene frame")] = Motion(speed=2.5 / 24.0)
    direction: Annotated[Finite, msgspec.Meta(title="Direction", description="Travel direction of the particles"), Presentation(quantity=Some(Quantity.ANGLE))] = -math.pi / 2.0
    turbulence: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Turbulence", description="Sway of the nearest layer crossing the wind field as a share of the frame's greater side"),
        Presentation(soft=Some((0.0, 0.1)), decimals=Some(4), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 0.0
    wind: Annotated[Basis, msgspec.Meta(title="Wind", description="Texture sampled independently along both displacement axes")] = Perlin(
        size=1.0 / 3.2, stretch=1.0, octaves=Octaves(), distortion=0.0, cut=Cut(interpolation=Interpolation.LINEAR)
    )
    evolution: Annotated[Finite, msgspec.Meta(title="Evolution", description="Wind field's signed evolution per scene frame")] = 0.0
    motion_blur: Annotated[bool, msgspec.Meta(title="Motion Blur", description="Streak each particle across the scene's shutter")] = True
    layers: Annotated[int, msgspec.Meta(ge=1, title="Layers", description="Depth layers drawn from far to near"), GraphForm.SETTING] = 3
    depth: Annotated[
        float, msgspec.Meta(ge=1.0, le=sys.float_info.max, title="Depth", description="Distance of the farthest layer over the nearest"), Presentation(soft=Some((1.0, 16.0)), decimals=Some(2))
    ] = 4.0
    depth_variation: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Depth Variation", description="Random depth within each layer's interval")] = 0.0
    focus: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Focus", description="Focused depth between the nearest and farthest layers")] = 1.0
    defocus: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Defocus", description="Radius added per difference in reciprocal depth"), Presentation(quantity=Some(Quantity.FRAME_GREATER))
    ] = 0.0
    iris: Annotated[Aperture, msgspec.Meta(title="Iris", description="Projected particle outline")] = Aperture()
    squeeze: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Squeeze", description="Horizontal compression of the particle outline")] = 1.0
    fringe: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0, title="Fringe", description="Channel radius shift, toward blue above zero")] = 0.0
    fade: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Fade", description="Share of the trail fading from its head toward its tail")] = 0.0
    twinkle: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Twinkle", description="Share of each particle's light modulated over time")] = 0.0
    scintillation: Annotated[Motion, msgspec.Meta(title="Twinkle Clock", description="Independent rate and hold of the twinkle field")] = Motion(speed=0.0)
    seed: Annotated[Seed, msgspec.Meta(title="Seed", description="Field of equal statistics the entry samples")] = 0

    def applied(
        self,
        plane: OpenImageIO.ImageBuf,
        bounds: tuple[float, float, float, float],
        *,
        reference_frame: float,
        shutter: tuple[float, float],
        precision: Precision,
        spectrum: Callable[[Spectrum, npt.NDArray[np.float64]], ImagingResult[npt.NDArray[np.float64]]],
    ) -> ImagingResult[Estimate[OpenImageIO.ImageBuf]]:
        """Integrates all particle supports, shutter paths, and ordered intrinsic occlusion."""
        try:
            if (source := (plane.get_pixels(OpenImageIO.DOUBLE))) is None:
                return Error((ImagingBufferFault(request=plane, error=plane.geterror()),))
            with np.errstate(over="raise", divide="raise", invalid="raise"):
                spec = plane.spec()
                left, bottom, right, top = map(Fraction, bounds)
                dx, dy = (right - left) / spec.width, (top - bottom) / spec.height
                origin = left, bottom
                heading = np.array((math.cos(self.direction), math.sin(self.direction)))
                held = self.motion.hold * math.floor(reference_frame / self.motion.hold)
                opening, closing = shutter if self.motion_blur else (0.0, 0.0)
                focus = self.depth ** (-self.focus)
                particles: list[Particle] = []
                wind_low, wind_high = density_range(self.wind)
                wind_reach = max(abs(2.0 * wind_low - 1.0), abs(2.0 * wind_high - 1.0))
                if self.population.density > 0.0 and self.iris.obstruction < 1.0 and (self.coverage.enabled or self.emission.enabled):
                    for layer in range(self.layers):
                        midpoint = (layer + 0.5) / self.layers
                        reference = self.depth ** (-midpoint)
                        near = self.depth ** (-(midpoint - self.depth_variation / (2.0 * self.layers)))
                        far = self.depth ** (-(midpoint + self.depth_variation / (2.0 * self.layers)))
                        spacing = Fraction(self.population.spacing) * Fraction(reference)
                        reach = self.population.spacing * near * self.population.size / 2.0 + self.defocus * max(abs(near - focus), abs(far - focus))
                        support = np.array((reach / self.squeeze, reach))
                        travel = np.array([heading * self.motion.speed * scale * time for scale in (near, far) for time in (held + opening, held + closing)])
                        low = travel.min(axis=0) - self.turbulence * near * wind_reach - support
                        high = travel.max(axis=0) + self.turbulence * near * wind_reach + support
                        for row in range(math.floor((bottom - Fraction(float(high[1]))) / spacing) - 1, math.floor((top - Fraction(float(low[1]))) / spacing) + 1):
                            for column in range(math.floor((left - Fraction(float(high[0]))) / spacing) - 1, math.floor((right - Fraction(float(low[0]))) / spacing) + 1):
                                if (draw := (uniforms((column, row, layer), seed=self.seed, count=7)))[0] < self.population.density:
                                    depth = midpoint + self.depth_variation * (float(draw[6]) - 0.5) / self.layers
                                    scale = self.depth ** (-depth)
                                    core = self.population.spacing * scale * self.population.size * (1.0 - self.population.variation * float(draw[1])) / 2.0
                                    optical = core + self.defocus * abs(scale - focus)
                                    mark = Mark(
                                        cell=(column, row),
                                        center=((Fraction(column) + Fraction(0.25 + 0.5 * float(draw[4]))) * spacing, (Fraction(row) + Fraction(0.25 + 0.5 * float(draw[5]))) * spacing),
                                        radius=core,
                                        gain=math.exp2(-self.population.spread * float(draw[2])),
                                        tone=float(draw[3]),
                                    )
                                    particles.append(Particle(mark=mark, layer=layer, depth=depth, scale=scale, optical=optical))
                particles.sort(key=lambda particle: (particle.depth, particle.layer, particle.mark.cell[1], particle.mark.cell[0]))
                tones = np.array([particle.mark.tone for particle in particles], dtype=np.float64)
                colors = accumulated((
                    spectrum(self.coverage.value, tones) if self.coverage.enabled else Ok(np.zeros((len(particles), 3), dtype=np.float64)),
                    spectrum(self.emission.value.spectrum, tones) if self.emission.enabled else Ok(np.zeros((len(particles), 3), dtype=np.float64)),
                    spectrum(self.temperature, tones) if self.emission.enabled and self.incandescence > 0.0 else Ok(np.ones((len(particles), 3), dtype=np.float64)),
                ))
                if colors.tag == "error":
                    return Error(colors.error)
                coating, light, thermal = colors.ok
                illuminant = light * (1.0 - self.incandescence + self.incandescence * thermal)
                radiance = math.exp2(self.emission.value.intensity) if self.emission.enabled else 0.0
                unit_area, area_error = self.iris.integral(self.population.softness, epsabs=precision.absolute, epsrel=precision.relative) if particles and self.emission.enabled else (1.0, 0.0)
                channel = np.array((1.0 - max(self.fringe, 0.0), 1.0 - abs(self.fringe) / 2.0, 1.0 + min(self.fringe, 0.0)))
                plain = Perlin(dimensions=4, size=1.0, stretch=1.0, distortion=0.0, octaves=Octaves(detail=0.0), cut=Cut(interpolation=Interpolation.LINEAR))

                def moved(particle: Particle, times: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
                    drift = times[:, None] * heading * self.motion.speed * particle.scale
                    local = np.array((float(particle.mark.center[0] - origin[0]), float(particle.mark.center[1] - origin[1]))) + drift
                    if self.turbulence == 0.0:
                        return Ok(local)
                    rest = np.array(tuple(float(value) for value in particle.mark.center)) + drift
                    coordinates = np.column_stack((rest / particle.scale, self.evolution * times, np.zeros(len(times))))
                    return accumulated(sample(self.wind, coordinates, seed=self.seed, stream=axis + 1) for axis in range(2)).map(
                        lambda fields: local + self.turbulence * particle.scale * (2.0 * np.stack([field[:, 0] for field in fields], axis=-1) - 1.0)
                    )

                def profiles(particle: Particle, offsets: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
                    coordinates = offsets * np.array((self.squeeze, 1.0))
                    normalized = np.divide(coordinates[:, None, :], particle.optical * channel[None, :, None], out=np.zeros((len(offsets), 3, 2), dtype=np.float64), where=channel[None, :, None] > 0.0)
                    return self.iris.profile(normalized, self.population.softness) * (channel > 0.0)

                observations = np.broadcast_to(source, (precision.replicates, *source.shape)).copy()
                for replicate in range(precision.replicates):
                    for index, particle in enumerate(particles):
                        net = precision.points(3, replicate, key=(*particle.mark.cell, particle.layer, self.seed))
                        times = held + opening + (closing - opening) * net[:, 2]
                        if (center := (moved(particle, times))).tag == "error":
                            return Error(center.error)
                        radius = particle.optical * np.sqrt(net[:, 0])
                        angle = math.tau * net[:, 1]
                        offsets = np.stack((radius * np.cos(angle) / self.squeeze, radius * np.sin(angle)), axis=-1)
                        positions = center.ok + offsets
                        selected = (positions[:, 0] >= 0.0) & (positions[:, 0] < float(spec.width * dx)) & (positions[:, 1] >= 0.0) & (positions[:, 1] < float(spec.height * dy))
                        if not np.any(selected):
                            continue
                        positions, offsets, times = positions[selected], offsets[selected], times[selected]
                        columns = np.floor(positions[:, 0] / float(dx)).astype(np.intp)
                        rows = spec.height - 1 - np.floor(positions[:, 1] / float(dy)).astype(np.intp)
                        profile = profiles(particle, offsets)
                        opacity = particle.mark.gain * np.mean(profile, axis=-1)
                        transmittance = np.ones(len(times), dtype=np.float64)
                        for nearer in particles[:index]:
                            if (near_center := (moved(nearer, times))).tag == "error":
                                return Error(near_center.error)
                            transmittance *= 1.0 - nearer.mark.gain * np.mean(profiles(nearer, positions - near_center.ok), axis=-1)
                        pixel = source[rows, columns]
                        contribution = np.zeros((len(times), 4), dtype=np.float64)
                        if self.coverage.enabled:
                            contribution[:, :3] = opacity[:, None] * (coating[index] - pixel[:, :3])
                            contribution[:, 3] = opacity * (1.0 - pixel[:, 3])
                        if self.emission.enabled:
                            energy = radiance * particle.mark.gain * math.pi * particle.mark.radius**2
                            area = unit_area * np.square(particle.optical * channel) / self.squeeze
                            peak = np.divide(energy * illuminant[index], area, out=np.zeros(3, dtype=np.float64), where=channel > 0.0)
                            modulation = np.ones(len(times), dtype=np.float64)
                            if self.twinkle > 0.0:
                                twinkle_held = self.scintillation.hold * math.floor(reference_frame / self.scintillation.hold)
                                twinkle_time = self.scintillation.speed * (twinkle_held + times - held)
                                coordinates = np.column_stack((
                                    np.full(len(times), particle.mark.cell[0]),
                                    np.full(len(times), particle.mark.cell[1]),
                                    np.full(len(times), particle.layer),
                                    twinkle_time,
                                ))
                                if (twinkle := (sample(plain, coordinates, seed=self.seed, stream=3))).tag == "error":
                                    return Error(twinkle.error)
                                modulation -= self.twinkle * twinkle.ok[:, 0]
                            contribution[:, :3] += profile * peak * modulation[:, None]
                        temporal = (1.0 - self.fade + self.fade * net[selected, 2]) / (1.0 - self.fade / 2.0) if opening != closing else np.ones(len(times), dtype=np.float64)
                        weight = math.pi * particle.optical**2 / self.squeeze / float(dx * dy) / len(net)
                        np.add.at(observations[replicate], (rows, columns), contribution * (weight * temporal * transmittance)[:, None])
                estimate = Estimate.of(observations, precision)
                if not (output := (plane.copy(OpenImageIO.DOUBLE))).set_pixels(output.roi, estimate.value):
                    return Error((ImagingBufferFault(request=output, error=output.geterror()),))
                return Ok(
                    Estimate(
                        value=output,
                        standard_error=estimate.standard_error,
                        precision=precision,
                        replicates=observations,
                        normalization=Some((unit_area, area_error)) if particles and self.emission.enabled else Nothing,
                    )
                )
        except (FloatingPointError, MemoryError, OverflowError, ValueError) as error:
            return Error((ImagingGeneratorFault(request=self, error=error),))


# --- [EXPORTS] ----------------------------------------------------------------------------

__all__ = ["Mark", "Particle", "Particles", "Population"]
