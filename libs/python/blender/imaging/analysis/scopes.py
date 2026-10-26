"""Scene-linear histograms, waveforms, vectorscopes, written-code clipping, view bands, and chromaticity."""

from typing import Annotated, ClassVar, Self

import colour
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.imaging.analysis.metering import Samples
from blender.imaging.color import observer
from blender.imaging.color.processors import chromaticities, converted, DisplayView, luma, unassociated, xyz
from blender.imaging.faults import ImagingResult, paired
from blender.imaging.parameters import markers, Stops
from blender.imaging.sampling import Frozen

# --- [MODELS] ---------------------------------------------------------------------------


class Histogram(Frozen, frozen=True, kw_only=True, eq=False):
    """Covered luminance and channel counts, linear to one, then four log stops."""

    resolution: ClassVar[int] = 256
    headroom: ClassVar[int] = 4
    counts: npt.NDArray[np.intp]
    channels: npt.NDArray[np.intp]
    clipped: Option[npt.NDArray[np.intp]]

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32], clip: Option[tuple[float, float]] = Nothing) -> ImagingResult[Self]:
        """Counts covered RGB and luminance, `clip` counting channels outside [low, high)."""

        def counted(samples: Samples) -> ImagingResult[Self]:
            color, bins = samples.color[samples.covered], np.r_[-np.inf, cls.edges(), np.inf]
            return Ok(
                cls(
                    counts=np.histogram(samples.luminance[samples.covered], bins=bins)[0],
                    channels=np.stack(tuple(np.histogram(channel, bins=bins)[0] for channel in color.T)),
                    clipped=clip.map(lambda levels: np.stack(((color < levels[0]).sum(axis=0), (color >= levels[1]).sum(axis=0)))),
                )
            )

        return Samples.of(config, plane).bind(counted)

    @classmethod
    def edges(cls) -> npt.NDArray[np.float64]:
        """Returns inner luminance bin boundaries."""
        return np.concatenate((np.arange(1, cls.resolution) / cls.resolution, np.exp2(np.arange(cls.resolution * cls.headroom + 1) / cls.resolution)))

    @classmethod
    def mask(cls, values: npt.NDArray[np.float32] | npt.NDArray[np.float64], first: int, last: int) -> npt.NDArray[np.bool]:
        """Selects non-NaN values in inclusive bin interval."""
        return np.isin(np.digitize(values, cls.edges()), np.arange(first, last + 1)) & ~np.isnan(values)


class Waveform(Frozen, frozen=True, kw_only=True, eq=False):
    """Covered RGB and luminance density planes, value rows by column groups."""

    traces: npt.NDArray[np.float64]

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32], cells: tuple[int, int]) -> ImagingResult[Self]:
        """Counts covered RGB and luminance over `cells` columns and value rows."""

        def traced(samples: Samples) -> ImagingResult[Self]:
            width, rows = min(cells[0], plane.shape[1]), cells[1]
            columns = np.broadcast_to(np.arange(plane.shape[1]), samples.covered.shape)[samples.covered]
            bins = (np.r_[-np.inf, np.arange(1, rows) / rows, np.inf], np.linspace(0.0, plane.shape[1], width + 1))
            values = np.column_stack((samples.color[samples.covered], samples.luminance[samples.covered]))
            return Ok(cls(traces=np.stack(tuple(np.histogram2d(channel, columns, bins=bins)[0] for channel in values.T))))

        return Samples.of(config, plane).bind(traced)


class Vectorscope(Frozen, frozen=True, kw_only=True, eq=False):
    """Centered Cb/Cr density, with Cr rows and Cb columns."""

    trace: npt.NDArray[np.float64]
    inverse: npt.NDArray[np.float64]

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32], cells: tuple[int, int], zoom: Annotated[float, msgspec.Meta(gt=0.0)] = 1.0) -> ImagingResult[Self]:
        """Returns covered Cb/Cr counts with full-range YCbCr-to-RGB matrix of working luminance weights."""

        def scoped(coefficients: npt.NDArray[np.float64]) -> ImagingResult[Self]:
            weights = coefficients[[0, 2]]
            values = colour.RGB_to_YCbCr(unassociated(plane)[..., :3][(plane[..., 3:] > 0.0).all(axis=-1)].astype(np.float64), K=weights, in_legal=False, out_legal=False, out_int=False)
            reach = 0.5 / zoom
            held = ((values[:, 1:] >= -reach) & (values[:, 1:] < reach)).all(axis=-1)
            return Ok(
                cls(
                    trace=np.histogram2d(values[held, 2], values[held, 1], bins=(cells[1], cells[0]), range=((-reach, reach), (-reach, reach)))[0],
                    inverse=np.asarray(colour.matrix_YCbCr(K=weights, is_legal=False, is_int=False), dtype=np.float64),
                )
            )

        return luma(config).bind(scoped)


class Clipping(Frozen, frozen=True, kw_only=True, eq=False):
    """Covered pixels with a color channel at either written-code endpoint."""

    low_channels: npt.NDArray[np.bool]
    high_channels: npt.NDArray[np.bool]

    @property
    def low(self) -> npt.NDArray[np.bool]:
        """Pixels with any channel at lowest code."""
        return self.low_channels.any(axis=-1)

    @property
    def high(self) -> npt.NDArray[np.bool]:
        """Pixels with any channel at highest code."""
        return self.high_channels.any(axis=-1)

    @classmethod
    def of(cls, codes: npt.NDArray[np.uint8] | npt.NDArray[np.uint16]) -> Self:
        """Returns channel masks at code endpoints of array dtype."""
        color, covered = codes[..., :3], (codes[..., 3:] > 0).all(axis=-1)
        return cls(low_channels=(color == 0) & covered[..., None], high_channels=(color == np.iinfo(codes.dtype).max) & covered[..., None])


class Bands(Frozen, frozen=True, kw_only=True, eq=False):
    """Constant view colors and their scene-linear boundaries in ramp order."""

    resolution: ClassVar[int] = 256
    colors: npt.NDArray[np.float32]
    edges: npt.NDArray[np.float64]

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, view: DisplayView) -> ImagingResult[Option[Self]]:
        """Reads a stepped view's constant levels across the stop range, absent unless each level the range holds whole outlasts every step between levels."""
        least, most = (float(bound) for meta in markers(Stops, msgspec.Meta) for bound in (meta.ge, meta.le) if bound is not None)
        exposures = np.arange(round(least * cls.resolution), round(most * cls.resolution) + 1) / cls.resolution

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
    def of(cls, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32], view: DisplayView, cells: tuple[int, int]) -> ImagingResult[Self]:
        """Returns xy counts over `cells` spanning the diagram frame, or every color fault."""
        extent, _ = observer.frame()

        def built(parts: tuple[npt.NDArray[np.float32], npt.NDArray[np.float64], Option[npt.NDArray[np.float64]]]) -> ImagingResult[Self]:
            tristimulus, working, display = parts
            points = observer.coordinates(tristimulus[..., :3][(plane[..., 3:] > 0.0).all(axis=-1)])
            return Ok(cls(trace=np.histogram2d(points[:, 0], points[:, 1], bins=cells, range=np.stack((np.zeros(2), extent), axis=-1))[0], working=working, display=display))

        shown = view.colorspace(config).filter(lambda space: not space.isData()).map(lambda space: chromaticities(config, space.getName()).map(Some)).default_value(Ok(Nothing))
        return paired(converted(config, xyz(config, PyOpenColorIO.ROLE_SCENE_LINEAR), plane), chromaticities(config, PyOpenColorIO.ROLE_SCENE_LINEAR), shown).bind(built)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bands", "Chromaticity", "Clipping", "Histogram", "Vectorscope", "Waveform"]
