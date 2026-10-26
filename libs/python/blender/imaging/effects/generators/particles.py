"""Soft points in depth layers covering frame, emitting light, or both."""

from collections.abc import Callable
import functools
import math
from typing import Annotated

from expression import Error, Nothing, Ok, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO

from blender.imaging.chain import Effect, Stage
from blender.imaging.effects.generators.noise import Basis, density_range, Emission, Incandescence, Octaves, Perlin, sample, Spectrum, Tint
from blender.imaging.effects.optics.defocus import Aperture
from blender.imaging.faults import accumulated, guarded, ImagingBufferFault, ImagingNumericFault, ImagingResult, paired
from blender.imaging.hashing import hashed
from blender.imaging.parameters import AtLeastOne, Finite, FrameExtent, FrameLength, Gated, GraphForm, Motion, NonNegative, Positive, Presentation, Quantity, Seed, Unit
from blender.imaging.sampling import Estimate, Precision, smoothstep

# --- [MODELS] ---------------------------------------------------------------------------


class Mark(msgspec.Struct, frozen=True, kw_only=True):
    """Stable population member with global center and physical radius."""

    cell: tuple[int, ...]
    center: tuple[float, float]
    radius: float
    gain: float
    tone: float


class Population(msgspec.Struct, frozen=True, kw_only=True):
    """Soft discs in Voronoi cells with presence, size, fade, and edge."""

    spacing: Annotated[FrameExtent, msgspec.Meta(description="Cell width at nearest layer over greater frame side"), Presentation(soft=Some((0.002, 0.1)), precision=Some(4))] = 0.01
    density: Annotated[Unit, msgspec.Meta(description="Share of cells holding a particle")] = 0.5
    size: Annotated[float, msgspec.Meta(gt=0.0, le=0.5, description="Particle diameter over cell width")] = 0.2
    variation: Annotated[Unit, msgspec.Meta(description="Diameter shrink share below size")] = 0.5
    spread: Annotated[NonNegative, msgspec.Meta(description="Per-particle coverage drop in stops"), Presentation(soft=Some((0.0, 10.0)), step=Some(0.01), precision=Some(3))] = 1.0
    softness: Annotated[Unit, msgspec.Meta(description="Edge fade over radius before pixel filtering")] = 0.5

    def covering(self, bounds: tuple[float, float, float, float]) -> npt.NDArray[np.int64]:
        """Returns cells with discs able to intersect global bounds, rows bottom first."""
        left, bottom, right, top = bounds
        radius = self.size / 2.0
        columns = np.arange(math.floor(left / self.spacing - radius) - 1, math.floor(right / self.spacing + radius) + 1)
        rows = np.arange(math.floor(bottom / self.spacing - radius) - 1, math.floor(top / self.spacing + radius) + 1)
        return np.stack(np.meshgrid(columns, rows), axis=-1).reshape(-1, 2)

    def marks(self, bounds: tuple[float, float, float, float], *, seed: Seed, stream: int = 0) -> tuple[Mark, ...]:
        """Returns occupied members of cells covering global bounds."""
        return self.sites(self.covering(bounds), seed=seed, stream=stream)

    def sites(self, cells: npt.NDArray[np.int64], *, seed: Seed, stream: int = 0) -> tuple[Mark, ...]:
        """Returns occupied members of integer cells, in cell order."""
        draws = hashed(cells, seed=seed, stream=stream, count=6)
        kept = draws[:, 0] < self.density
        return self.marked(cells[kept], draws[kept], self.spacing)

    def placed(
        self, cells: npt.NDArray[np.int64], draws: npt.NDArray[np.float64], spacing: float, scale: float | npt.NDArray[np.float64] = 1.0
    ) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64], npt.NDArray[np.float64]]:
        """Returns centers, radii, and peak gains of occupied cells from their draws at lattice spacing and depth scale."""
        return (cells[:, :2] + 0.25 + 0.5 * draws[:, 4:6]) * spacing, self.spacing * scale * self.size * (1.0 - self.variation * draws[:, 1]) / 2.0, np.exp2(-self.spread * draws[:, 2])

    def marked(self, cells: npt.NDArray[np.int64], draws: npt.NDArray[np.float64], spacing: float, scale: float | npt.NDArray[np.float64] = 1.0) -> tuple[Mark, ...]:
        """Constructs occupied cells from their draws at lattice spacing and depth scale."""
        centers, radii, gains = self.placed(cells, draws, spacing, scale)
        return tuple(
            Mark(cell=tuple(cell), center=(center[0], center[1]), radius=radius, gain=gain, tone=tone)
            for cell, center, radius, gain, tone in zip(cells.tolist(), centers.tolist(), radii.tolist(), gains.tolist(), draws[:, 3].tolist())
        )

    def integrated(
        self, bounds: tuple[float, float, float, float], shape: tuple[int, int] = (1, 1), *, seed: Seed, precision: Precision, stream: int = 0, paint: Callable[[float], float] = float
    ) -> ImagingResult[Estimate[npt.NDArray[np.float64]]]:
        """Estimates mean disc coverage and covered tone over each pixel of a positive rectangle.

        Args:
            bounds: Global rectangle as left, bottom, right, top.
            shape: Pixel rows and columns splitting the rectangle, rows top first.
            seed: Number selecting the population.
            precision: Sobol resolution and independent error replicates.
            stream: Independent population under one seed.
            paint: Tone weight of covered ink.

        Returns:
            `(rows, columns, 2)` coverage and covered paint, or numeric refusal.
        """

        def computed() -> ImagingResult[Estimate[npt.NDArray[np.float64]]]:
            left, bottom, right, top = bounds
            height, width = shape
            across, down = (right - left) / width, (top - bottom) / height
            cells = self.covering(bounds)
            draws = hashed(cells, seed=seed, stream=stream, count=6)
            kept = draws[:, 0] < self.density
            cells, draws = cells[kept], draws[kept]
            centers, radii, gains = self.placed(cells, draws, self.spacing)
            paints = np.column_stack((np.ones(len(draws)), np.array([paint(tone) for tone in draws[:, 3].tolist()], dtype=np.float64).reshape(-1)))

            def replicated(replicate: int) -> npt.NDArray[np.float64]:
                net = (precision.points(2, replicate) + hashed(cells, seed=precision.seed, stream=replicate, count=2)[:, None]) % 1.0
                radial, angle = np.sqrt(net[..., 0]), math.tau * net[..., 1]
                columns = np.floor((centers[:, :1] + radii[:, None] * radial * np.cos(angle) - left) / across).astype(np.intp)
                rows = np.ceil((top - centers[:, 1:] - radii[:, None] * radial * np.sin(angle)) / down).astype(np.intp) - 1
                inside = (columns >= 0) & (columns < width) & (rows >= 0) & (rows < height)
                profile = np.ones_like(radial) if self.softness == 0.0 else smoothstep(np.minimum((1.0 - radial) / self.softness, 1.0))
                mass = profile * (math.pi * radii**2 * gains / (across * down * net.shape[1]))[:, None]
                pixels = np.zeros((height * width, 2), dtype=np.float64)
                np.add.at(pixels, (rows * width + columns)[inside], (mass[..., None] * paints[:, None])[inside])
                return pixels.reshape(height, width, 2)

            return Ok(Estimate.of(np.stack([replicated(replicate) for replicate in range(precision.replicates)]), precision))

        return guarded(ImagingNumericFault, self, computed)


class Particles(Effect, frozen=True, kw_only=True, tag=33):
    """Soft points in depth layers covering frame or emitting light (rain, snow, sparks, dust, stars)."""

    stage = Stage.GENERATE
    pointwise = False
    population: Annotated[Population, msgspec.Meta(description="Particle layout, size, and edge")] = Population()
    coverage: Annotated[Gated[Spectrum], msgspec.Meta(description="Scene-linear cover color by particle tone")] = Gated(enabled=True, value=Tint())
    emission: Annotated[Gated[Emission], msgspec.Meta(description="Light a particle adds")] = Gated(enabled=False, value=Emission())
    incandescence: Annotated[Unit, msgspec.Meta(description="Blackbody share of emitted color")] = 0.0
    temperature: Annotated[Incandescence, msgspec.Meta(description="Blackbody band multiplying emitted spectrum")] = Incandescence()
    motion: Annotated[Motion, msgspec.Meta(description="Nearest layer travel in greater frame sides per scene frame")] = Motion(speed=2.5 / 24.0)
    direction: Annotated[Finite, msgspec.Meta(description="Particle travel direction"), Presentation(quantity=Some(Quantity.ANGLE))] = -math.pi / 2.0
    turbulence: Annotated[FrameLength, msgspec.Meta(description="Nearest layer sway over greater frame side"), Presentation(soft=Some((0.0, 0.1)), precision=Some(4))] = 0.0
    wind: Annotated[Basis, msgspec.Meta(description="Texture displacing each axis independently")] = Perlin(size=1.0 / 3.2)
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
    scintillation: Annotated[Motion, msgspec.Meta(title="Twinkle Clock", description="Independent twinkle field rate and hold")] = Motion(speed=0.0)
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting pattern")] = 0

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
        """Integrates particle supports, shutter paths, and ordered intrinsic occlusion."""

        def computed() -> ImagingResult[Estimate[OpenImageIO.ImageBuf]]:
            if (source := (plane.get_pixels(OpenImageIO.DOUBLE))) is None:
                return Error((ImagingBufferFault(request=plane, error=plane.geterror()),))
            spec = plane.spec()
            left, bottom, right, top = bounds
            dx, dy = (right - left) / spec.width, (top - bottom) / spec.height
            heading = np.array((math.cos(self.direction), math.sin(self.direction)))
            held = self.motion.held(reference_frame)
            opening, closing = shutter if self.motion_blur else (0.0, 0.0)
            focus = self.depth ** (-self.focus)
            wind_low, wind_high = density_range(self.wind)
            wind_reach = max(abs(2.0 * wind_low - 1.0), abs(2.0 * wind_high - 1.0))
            channel = np.array((1.0 - max(self.fringe, 0.0), 1.0 - abs(self.fringe) / 2.0, 1.0 + min(self.fringe, 0.0)))
            drawn = self.population.density > 0.0 and self.iris.obstruction < 1.0 and (self.coverage.enabled or self.emission.enabled)

            def layered(layer: int) -> tuple[npt.NDArray[np.float64], ...]:
                midpoint = (layer + 0.5) / self.layers
                near = self.depth ** (-(midpoint - self.depth_variation / (2.0 * self.layers)))
                far = self.depth ** (-(midpoint + self.depth_variation / (2.0 * self.layers)))
                spacing = self.population.spacing * self.depth ** (-midpoint)
                reach = self.population.spacing * near * self.population.size / 2.0 + self.defocus * max(abs(near - focus), abs(far - focus))
                support = np.array((reach / self.squeeze, reach))
                travel = np.array([heading * self.motion.speed * scale * time for scale in (near, far) for time in (held + opening, held + closing)])
                low = travel.min(axis=0) - self.turbulence * near * wind_reach - support
                high = travel.max(axis=0) + self.turbulence * near * wind_reach + support
                columns = np.arange(math.floor((left - high[0]) / spacing) - 1, math.floor((right - low[0]) / spacing) + 1)
                rows = np.arange(math.floor((bottom - high[1]) / spacing) - 1, math.floor((top - low[1]) / spacing) + 1)
                cells = np.stack(np.meshgrid(columns, rows), axis=-1).reshape(-1, 2)
                keys = np.column_stack((cells, np.full(len(cells), layer)))
                draws = hashed(keys, seed=self.seed, stream=0, count=7)
                kept = (draws[:, 0] < self.population.density) & drawn
                depths = midpoint + self.depth_variation * (draws[kept, 6] - 0.5) / self.layers
                scales = self.depth ** (-depths)
                return (keys[kept], depths, scales, *self.population.placed(cells[kept], draws[kept], spacing, scales), draws[kept, 3])

            parts = tuple(np.concatenate(columns) for columns in zip(*(layered(layer) for layer in range(self.layers))))
            order = np.lexsort((parts[0][:, 0], parts[0][:, 1], parts[0][:, 2], parts[1]))
            keys, _, scales, centers, radii, gains, tones = (part[order] for part in parts)
            optical = radii + self.defocus * np.abs(scales - focus)
            extent = optical * channel.max() * max(1.0, 1.0 / self.squeeze) + scales * (self.motion.speed * max(abs(held + opening), abs(held + closing)) + self.turbulence * wind_reach)
            nets = tuple(precision.points(3, replicate) for replicate in range(precision.replicates))
            plain = Perlin(octaves=Octaves(detail=0.0))

            def moved(selected: npt.NDArray[np.intp], times: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
                rest = centers[selected, None, :] + times[None, :, None] * heading * self.motion.speed * scales[selected, None, None]
                if self.turbulence == 0.0:
                    return Ok(rest - (left, bottom))
                evolved = np.broadcast_to(self.evolution * times[None, :, None], (*rest.shape[:2], 1))
                coordinates = np.concatenate((rest / scales[selected, None, None], evolved, np.zeros_like(evolved)), axis=-1).reshape(-1, 4)
                return accumulated(sample(self.wind, coordinates, seed=self.seed, stream=axis + 1) for axis in range(2)).map(
                    lambda fields: rest - (left, bottom) + self.turbulence * scales[selected, None, None] * (2.0 * np.stack([field[:, 0] for field in fields], axis=-1).reshape(rest.shape) - 1.0)
                )

            def profiles(selected: npt.NDArray[np.intp], offsets: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
                normalized = np.divide(
                    (offsets * (self.squeeze, 1.0))[..., None, :],
                    optical[selected, None, None, None] * channel[:, None],
                    out=np.zeros((*offsets.shape[:-1], 3, 2), dtype=np.float64),
                    where=channel[:, None] > 0.0,
                )
                return self.iris.profile(normalized, self.population.softness) * (channel > 0.0)

            def modulated(index: int, times: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
                if not (self.emission.enabled and self.twinkle > 0.0):
                    return Ok(np.ones(len(times), dtype=np.float64))
                coordinates = np.column_stack((np.broadcast_to(keys[index], (len(times), 3)), self.scintillation.clock(reference_frame) + self.scintillation.speed * (times - held)))
                return sample(plain, coordinates, seed=self.seed, stream=3).map(lambda twinkle: 1.0 - self.twinkle * twinkle[:, 0])

            def rendered(planes: tuple[npt.NDArray[np.float64], ...]) -> ImagingResult[Estimate[OpenImageIO.ImageBuf]]:
                coating, light, thermal = planes
                illuminant = light * (1.0 - self.incandescence + self.incandescence * thermal)
                radiance = math.exp2(self.emission.value.intensity) if self.emission.enabled else 0.0
                unit_area, area_error = self.iris.integral(self.population.softness, epsabs=precision.absolute, epsrel=precision.relative) if len(keys) and self.emission.enabled else (1.0, 0.0)

                def deposited(observation: npt.NDArray[np.float64], replicate: int, index: int) -> ImagingResult[None]:
                    net = (nets[replicate] + hashed(keys[index], seed=precision.seed, stream=replicate, count=3)) % 1.0
                    times = held + opening + (closing - opening) * net[:, 2]
                    radius = optical[index] * np.sqrt(net[:, 0])
                    angle = math.tau * net[:, 1]
                    offsets = np.stack((radius * np.cos(angle) / self.squeeze, radius * np.sin(angle)), axis=-1)

                    def landed(center: npt.NDArray[np.float64]) -> ImagingResult[None]:
                        positions = center[0] + offsets
                        selected = (positions[:, 0] >= 0.0) & (positions[:, 0] < spec.width * dx) & (positions[:, 1] >= 0.0) & (positions[:, 1] < spec.height * dy)
                        if not np.any(selected):
                            return Ok(None)
                        kept, arrived = times[selected], positions[selected]
                        columns = np.floor(arrived[:, 0] / dx).astype(np.intp)
                        rows = spec.height - 1 - np.floor(arrived[:, 1] / dy).astype(np.intp)
                        profile = profiles(np.array((index,)), offsets[None, selected])[0]
                        opacity = gains[index] * np.mean(profile, axis=-1)
                        pixel = source[rows, columns]
                        temporal = (1.0 - self.fade + self.fade * net[selected, 2]) / (1.0 - self.fade / 2.0) if opening != closing else np.ones(len(kept), dtype=np.float64)
                        weight = math.pi * optical[index] ** 2 / self.squeeze / (dx * dy) / len(net)
                        nearer = np.flatnonzero(np.linalg.norm(centers[:index] - centers[index], axis=-1) <= extent[:index] + extent[index])

                        def added(factors: tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]) -> None:
                            transmittance, modulation = factors
                            contribution = np.zeros((len(kept), 4), dtype=np.float64)
                            if self.coverage.enabled:
                                contribution[:, :3] = opacity[:, None] * (coating[index] - pixel[:, :3])
                                contribution[:, 3] = opacity * (1.0 - pixel[:, 3])
                            if self.emission.enabled:
                                energy = radiance * gains[index] * math.pi * radii[index] ** 2
                                area = unit_area * np.square(optical[index] * channel) / self.squeeze
                                peak = np.divide(energy * illuminant[index], area, out=np.zeros(3, dtype=np.float64), where=channel > 0.0)
                                contribution[:, :3] += profile * peak * modulation[:, None]
                            np.add.at(observation, (rows, columns), contribution * (weight * temporal * transmittance)[:, None])

                        occlusion = moved(nearer, kept).map(lambda found: np.prod(1.0 - gains[nearer, None] * np.mean(profiles(nearer, arrived[None] - found), axis=-1), axis=0))
                        return paired(occlusion, modulated(index, kept)).map(added)

                    return moved(np.array((index,)), times).bind(landed)

                def published(observations: npt.NDArray[np.float64]) -> ImagingResult[Estimate[OpenImageIO.ImageBuf]]:
                    estimate = msgspec.structs.replace(Estimate.of(observations, precision), normalization=Some((unit_area, area_error)) if len(keys) and self.emission.enabled else Nothing)
                    if not (output := (plane.copy(OpenImageIO.DOUBLE))).set_pixels(output.roi, estimate.value):
                        return Error((ImagingBufferFault(request=output, error=output.geterror()),))
                    return Ok(estimate.holding(output))

                observations = np.broadcast_to(source, (precision.replicates, *source.shape)).copy()
                steps = ((replicate, index) for replicate in range(precision.replicates) for index in range(len(keys)))
                deposits: ImagingResult[None] = functools.reduce(lambda done, step: done.bind(lambda _: deposited(observations[step[0]], *step)), steps, Ok(None))
                return deposits.bind(lambda _: published(observations))

            return accumulated((
                spectrum(self.coverage.value, tones) if self.coverage.enabled else Ok(np.zeros((len(keys), 3), dtype=np.float64)),
                spectrum(self.emission.value.spectrum, tones) if self.emission.enabled else Ok(np.zeros((len(keys), 3), dtype=np.float64)),
                spectrum(self.temperature, tones) if self.emission.enabled and self.incandescence > 0.0 else Ok(np.ones((len(keys), 3), dtype=np.float64)),
            )).bind(rendered)

        return guarded(ImagingNumericFault, self, computed)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Mark", "Particles", "Population"]
