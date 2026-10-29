"""Covered samples and their readings: exposure metering, frame white, absolute PQ peak, histograms, waveforms, vectorscopes, code clipping, view bands, and chromaticity."""

import math
from typing import ClassVar, Self

import colour
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.imaging.color.colorimetry import chromaticities, coordinates
from blender.imaging.color.processors import converted, DisplayView, GREY, luma, xyz
from blender.imaging.files.buffers import unassociated
from blender.imaging.geometry.frames import Region
from blender.records.faults import Outcome, paired
from blender.records.fields import Frozen, Percent, Unit

# --- [MODELS] ---------------------------------------------------------------------------


class Extent(msgspec.Struct, frozen=True, kw_only=True):
    """Lowest covered luminance and percentile frame white."""

    percentile: ClassVar[float] = 99.995
    low: float
    white: float


class Samples(Frozen, frozen=True, kw_only=True, eq=False):
    """Straight RGB, float64 working luminance, and pixel coverage."""

    floor: ClassVar[float] = 1e-5
    color: npt.NDArray[np.float32]
    luminance: npt.NDArray[np.float64]
    covered: npt.NDArray[np.bool]

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32]) -> Outcome[Self]:
        """Reads associated RGB or RGBA as straight samples."""
        straight, covered = unassociated(plane)[..., :3], np.asarray((plane[..., 3:] > 0.0).all(axis=-1), dtype=np.bool)
        return luma(config).map(lambda coefficients: cls(color=straight, luminance=np.where(covered, np.matmul(straight, coefficients, dtype=np.float64), 0.0), covered=covered))

    @classmethod
    def stops(cls, values: npt.NDArray[np.float64], weights: npt.NDArray[np.float64] | None = None) -> float:
        """Returns stops from the weighted mean log2 of luminance `values`, clamped to `floor`, to scene gray."""
        return math.log2(GREY) - float(np.average(np.log2(np.maximum(values, cls.floor)), weights=weights))

    @property
    def finite(self) -> npt.NDArray[np.bool]:
        """Covered pixels with finite luminance."""
        return np.asarray(self.covered & np.isfinite(self.luminance), dtype=np.bool)

    def peak(self) -> Outcome[Option[float]]:
        """Returns covered percentile PQ peak in cd/m², decoded through the OCIO ST-2084 curve, absent without covered samples."""
        if not (found := (greatest := self.color.max(axis=-1))[self.covered & np.isfinite(greatest)]).size:
            return Ok(Nothing)
        code = np.full((1, 3), np.clip(np.percentile(found, Extent.percentile, method="inverted_cdf"), 0.0, 1.0), dtype=np.float32)
        return converted(PyOpenColorIO.Config(), PyOpenColorIO.BuiltinTransform(style="CURVE - ST-2084_to_LINEAR"), code).map(lambda linear: Some(100.0 * float(linear[0, 0])))

    def percentile(self, rank: float) -> Option[float]:
        """Returns covered luminance at `rank` percentile."""
        return Some(self.luminance[self.finite]).filter(lambda found: found.size > 0).map(lambda found: float(np.percentile(found, rank, method="inverted_cdf")))

    def extent(self) -> Option[Extent]:
        """Returns covered luminance extent."""
        return self.percentile(Extent.percentile).map(lambda white: Extent(low=float(self.luminance[self.finite].min()), white=white))

    def log_statistics(self) -> Option[tuple[float, float, float]]:
        """Returns mean, maximum, and minimum covered log luminance."""
        return (
            Some(np.log2(np.maximum(self.luminance[self.finite], self.floor)))
            .filter(lambda found: found.size > 0)
            .map(lambda found: (float(found.mean(dtype=np.float64)), float(found.max()), float(found.min())))
        )

    def waveform(self, cells: tuple[int, int]) -> npt.NDArray[np.float64]:
        """Returns covered R, G, B, and luminance density planes over `cells` column groups and value rows."""
        height, width = self.covered.shape
        rows = cells[1]
        columns = np.broadcast_to(np.arange(width), (height, width))[self.covered]
        bins = (np.r_[-np.inf, np.arange(1, rows) / rows, np.inf], np.linspace(0.0, width, min(cells[0], width) + 1))
        return np.stack(tuple(np.histogram2d(channel, columns, bins=bins)[0] for channel in np.column_stack((self.color[self.covered], self.luminance[self.covered])).T))


class Meter(msgspec.Struct, frozen=True, kw_only=True):
    """Exposure percentile trims, weight taken from samples outside the middle region, and metered region, the whole frame by default."""

    low: Percent = 0.0
    high: Percent = 100.0
    centre: Unit = 0.0
    middle: Region = Region(left=0.25, bottom=0.25, right=0.75, top=0.75)
    region: Region = Region()

    def stops(self, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32]) -> Outcome[Option[float]]:
        """Returns stops to scene gray, or absence without weighted samples."""

        def metered(samples: Samples) -> Option[float]:
            covered = samples.finite
            found, weights = samples.luminance[covered], np.where(self.middle.mask(*samples.covered.shape), 1.0, 1.0 - self.centre)[covered]
            start = min(math.floor(found.size * min(self.low, self.high) / 100.0), found.size - 1)
            kept = np.argsort(found, stable=True)[start : max(math.ceil(found.size * max(self.low, self.high) / 100.0), start + 1)]
            return Some(weights[kept]).filter(lambda selected: bool(selected.sum(dtype=np.float64) > 0.0)).map(lambda selected: Samples.stops(found[kept], selected))

        return Samples.of(config, plane[self.region.window(*plane.shape[:2])]).map(metered)


class Histogram(Frozen, frozen=True, kw_only=True, eq=False):
    """Covered luminance and channel counts, linear to one, then four log stops."""

    resolution: ClassVar[int] = 256
    headroom: ClassVar[int] = 4
    counts: npt.NDArray[np.intp]
    channels: npt.NDArray[np.intp]
    clipped: Option[npt.NDArray[np.intp]]

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32], clip: Option[tuple[float, float]] = Nothing) -> Outcome["Histogram"]:
        """Counts covered RGB and luminance, `clip` counting channels outside [low, high)."""

        def counted(samples: Samples) -> Histogram:
            color, bins = samples.color[samples.covered], np.r_[-np.inf, cls.edges(), np.inf]
            return cls(
                counts=np.histogram(samples.luminance[samples.covered], bins=bins)[0],
                channels=np.stack(tuple(np.histogram(channel, bins=bins)[0] for channel in color.T)),
                clipped=clip.map(lambda levels: np.stack(((color < levels[0]).sum(axis=0), (color >= levels[1]).sum(axis=0)))),
            )

        return Samples.of(config, plane).map(counted)

    @classmethod
    def edges(cls) -> npt.NDArray[np.float64]:
        """Returns inner luminance bin boundaries."""
        return np.concatenate((np.arange(1, cls.resolution) / cls.resolution, np.exp2(np.arange(cls.resolution * cls.headroom + 1) / cls.resolution)))

    @classmethod
    def mask(cls, values: npt.NDArray[np.float32] | npt.NDArray[np.float64], first: int, last: int) -> npt.NDArray[np.bool]:
        """Selects non-NaN values in inclusive bin interval."""
        return np.asarray(np.isin(np.digitize(values, cls.edges()), np.arange(first, last + 1)) & ~np.isnan(values), dtype=np.bool)


class Vectorscope(Frozen, frozen=True, kw_only=True, eq=False):
    """Centered Cb/Cr density, with Cr rows and Cb columns."""

    trace: npt.NDArray[np.float64]
    inverse: npt.NDArray[np.float64]

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32], cells: tuple[int, int], zoom: float = 1.0) -> Outcome["Vectorscope"]:
        """Returns covered Cb/Cr counts with full-range YCbCr-to-RGB matrix of working luminance weights."""

        def traced(pair: tuple[Samples, npt.NDArray[np.float64]]) -> Vectorscope:
            samples, coefficients = pair
            weights = coefficients[[0, 2]]
            values = np.asarray(colour.RGB_to_YCbCr(samples.color[samples.covered].astype(np.float64), K=weights, in_legal=False, out_legal=False, out_int=False), dtype=np.float64)
            reach = 0.5 / zoom
            held = ((values[:, 1:] >= -reach) & (values[:, 1:] < reach)).all(axis=-1)
            return cls(
                trace=np.histogram2d(values[held, 2], values[held, 1], bins=(cells[1], cells[0]), range=((-reach, reach), (-reach, reach)))[0],
                inverse=np.asarray(colour.matrix_YCbCr(K=weights, is_legal=False, is_int=False), dtype=np.float64),
            )

        return paired(Samples.of(config, plane), luma(config)).map(traced)


class Clipping(Frozen, frozen=True, kw_only=True, eq=False):
    """Covered pixels with a color channel at either written-code endpoint."""

    low_channels: npt.NDArray[np.bool]
    high_channels: npt.NDArray[np.bool]

    @property
    def low(self) -> npt.NDArray[np.bool]:
        """Pixels with any channel at lowest code."""
        return np.asarray(self.low_channels.any(axis=-1), dtype=np.bool)

    @property
    def high(self) -> npt.NDArray[np.bool]:
        """Pixels with any channel at highest code."""
        return np.asarray(self.high_channels.any(axis=-1), dtype=np.bool)

    @classmethod
    def of(cls, codes: npt.NDArray[np.uint8 | np.uint16]) -> Self:
        """Returns channel masks at code endpoints of array dtype."""
        color, covered = codes[..., :3], np.asarray((codes[..., 3:] > 0).all(axis=-1), dtype=np.bool)
        return cls(low_channels=(color == 0) & covered[..., None], high_channels=(color == np.iinfo(codes.dtype).max) & covered[..., None])


class Bands(Frozen, frozen=True, kw_only=True, eq=False):
    """Constant view colors and their scene-linear boundaries in ramp order, the ramp spanning a caller's exposure stop range."""

    resolution: ClassVar[int] = 256
    colors: npt.NDArray[np.float32]
    edges: npt.NDArray[np.float64]

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, view: DisplayView, stops: tuple[float, float]) -> Outcome[Option[Self]]:
        """Reads a stepped view's constant levels across `stops`, the lowest and highest exposure, absent unless each level the range holds whole outlasts every step between levels."""
        least, most = stops
        exposures = np.arange(math.ceil(least * cls.resolution), math.floor(most * cls.resolution) + 1) / cls.resolution

        def banded(formed: npt.NDArray[np.float32]) -> Option[Self]:
            colors = formed[0]
            steps = np.diff(np.pad((colors[1:] == colors[:-1]).all(axis=-1), 1).astype(np.int8))
            starts, ends = np.flatnonzero(steps == 1), np.flatnonzero(steps == -1)
            stepped = starts.size > 2 and (ends - starts)[1:-1].min() > (starts[1:] - ends[:-1]).max()
            return Some(cls(colors=colors[starts], edges=np.exp2((exposures[ends[:-1]] + exposures[starts[1:]]) / 2.0))) if stepped else Nothing

        return view.form(config, np.repeat(np.exp2(exposures).astype(np.float32)[None, :, None], 3, axis=-1)).map(banded)


class Chromaticity(Frozen, frozen=True, kw_only=True, eq=False):
    """CIE xy density over the diagram frame with working and display chromaticities."""

    trace: npt.NDArray[np.float64]
    working: npt.NDArray[np.float64]
    display: Option[npt.NDArray[np.float64]]

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32], view: DisplayView, cells: tuple[int, int], extent: npt.NDArray[np.float64]) -> Outcome["Chromaticity"]:
        """Returns xy counts over `cells` spanning the diagram frame from the origin to the `extent` corner, or every color fault."""

        def built(parts: tuple[npt.NDArray[np.float32], npt.NDArray[np.float64], Option[npt.NDArray[np.float64]]]) -> Chromaticity:
            tristimulus, working, display = parts
            points = coordinates(tristimulus[..., :3][(plane[..., 3:] > 0.0).all(axis=-1)])
            return cls(trace=np.histogram2d(points[:, 0], points[:, 1], bins=cells, range=np.stack((np.zeros(2), extent), axis=-1))[0], working=working, display=display)

        shown = view.colorspace(config).filter(lambda space: not space.isData()).map(lambda space: chromaticities(config, space.getName()).map(Some)).default_value(Ok(Nothing))
        return paired(converted(config, xyz(config, PyOpenColorIO.ROLE_SCENE_LINEAR), plane), chromaticities(config, PyOpenColorIO.ROLE_SCENE_LINEAR), shown).map(built)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bands", "Chromaticity", "Clipping", "Extent", "Histogram", "Meter", "Samples", "Vectorscope"]
