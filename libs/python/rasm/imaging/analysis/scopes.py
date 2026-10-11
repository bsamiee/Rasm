"""Scene-linear histograms, written-code clipping, view bands, and chromaticity."""

from typing import Annotated, ClassVar, Self

import colour
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm.imaging.analysis.metering import Samples
from rasm.imaging.color import observer
from rasm.imaging.color.processors import applied, chromaticities, DisplayView, luma, processor, unassociated, xyz
from rasm.imaging.faults import ImagingResult, paired
from rasm.imaging.parameters import STOP_RANGE

# --- [MODELS] ---------------------------------------------------------------------------


class Histogram(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Covered luminance counts, linear to one and logarithmic through four stops."""

    resolution: ClassVar[int] = 256
    headroom: ClassVar[int] = 4
    counts: npt.NDArray[np.intp]
    channels: npt.NDArray[np.intp]
    clipped: Option[npt.NDArray[np.intp]]

    @classmethod
    def edges(cls) -> npt.NDArray[np.float64]:
        """Returns interior boundaries of the 1281-bin luminance axis."""
        edges = np.concatenate((np.arange(1, cls.resolution) / cls.resolution, np.exp2(np.arange(cls.resolution * cls.headroom + 1) / cls.resolution)))
        edges.setflags(write=False)
        return edges

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32], clip: Option[tuple[float, float]] = Nothing) -> "ImagingResult[Histogram]":
        """Counts covered RGB and luminance, with optional low/high channel clip counts."""

        def counted(samples: Samples) -> "Histogram":
            color, bins = samples.color[samples.covered], np.r_[-np.inf, cls.edges(), np.inf]
            counts = np.histogram(samples.luminance[samples.covered], bins=bins)[0]
            channels = np.stack(tuple(np.histogram(channel, bins=bins)[0] for channel in color.T))
            clipped = clip.map(lambda levels: np.stack(((color < levels[0]).sum(axis=0), (color >= levels[1]).sum(axis=0))))
            for array in (counts, channels, *clipped.to_list()):
                array.setflags(write=False)
            return cls(counts=counts, channels=channels, clipped=clipped)

        return Samples.of(config, plane).map(counted)

    @classmethod
    def mask(cls, values: npt.NDArray[np.float32] | npt.NDArray[np.float64], first: int, last: int) -> npt.NDArray[np.bool]:
        """Selects values in an inclusive bin interval, omitting NaN."""
        selected = np.isin(np.digitize(values, cls.edges()), np.arange(first, last + 1)) & ~np.isnan(values)
        selected.setflags(write=False)
        return selected


class Vectorscope(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Centered Cb/Cr density, with Cr rows and Cb columns."""

    trace: npt.NDArray[np.float64]
    inverse: npt.NDArray[np.float64]

    @staticmethod
    def of(config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32], cells: tuple[int, int], zoom: Annotated[float, msgspec.Meta(gt=0.0)] = 1.0) -> "ImagingResult[Vectorscope]":
        """Bins covered chroma and returns its native YCbCr-to-RGB color matrix."""

        def scoped(coefficients: npt.NDArray[np.float32]) -> "Vectorscope":
            inverse = np.asarray(colour.matrix_YCbCr(K=coefficients[[0, 2]].astype(np.float64), is_legal=False, is_int=False), dtype=np.float64)
            color = unassociated(plane)[..., :3][(plane[..., 3:] > 0.0).all(axis=-1)]
            values = np.linalg.solve(inverse, color.T).T
            reach = 0.5 / zoom
            held = ((values[:, 1:] >= -reach) & (values[:, 1:] < reach)).all(axis=-1)
            trace = np.histogram2d(values[held, 2], values[held, 1], bins=(cells[1], cells[0]), range=((-reach, reach), (-reach, reach)))[0]
            for array in (trace, inverse):
                array.setflags(write=False)
            return Vectorscope(trace=trace, inverse=inverse)

        return luma(config).map(scoped)


class Clipping(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Covered pixels with a color channel at either written-code endpoint."""

    low_channels: npt.NDArray[np.bool]
    high_channels: npt.NDArray[np.bool]

    @property
    def low(self) -> npt.NDArray[np.bool]:
        """Covered pixels with any channel at the lowest code."""
        mask = self.low_channels.any(axis=-1)
        mask.setflags(write=False)
        return mask

    @property
    def high(self) -> npt.NDArray[np.bool]:
        """Covered pixels with any channel at the highest code."""
        mask = self.high_channels.any(axis=-1)
        mask.setflags(write=False)
        return mask

    @classmethod
    def of(cls, codes: npt.NDArray[np.uint8] | npt.NDArray[np.uint16]) -> Self:
        """Returns per-pixel low and high clipping masks at the array's native depth."""
        color, covered = codes[..., :3], (codes[..., 3:] > 0).all(axis=-1)
        low, high = (color == 0) & covered[..., None], (color == np.iinfo(codes.dtype).max) & covered[..., None]
        for array in (low, high):
            array.setflags(write=False)
        return cls(low_channels=low, high_channels=high)


class Bands(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Constant view colors and their scene-linear boundaries in ramp order."""

    resolution: ClassVar[int] = 256
    colors: npt.NDArray[np.float32]
    edges: npt.NDArray[np.float64]

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, view: DisplayView) -> "ImagingResult[Option[Bands]]":
        """Reads neutral-response plateaus, or absence for a view without constant runs."""
        least, most = STOP_RANGE
        exposures = np.arange(round(least * cls.resolution), round(most * cls.resolution) + 1) / cls.resolution

        def banded(formed: npt.NDArray[np.float32]) -> "Option[Bands]":
            colors = formed[0]
            same = (colors[1:] == colors[:-1]).all(axis=-1)
            starts = np.flatnonzero(same & ~np.r_[False, same[:-1]])
            ends = np.flatnonzero(same & ~np.r_[same[1:], False]) + 1
            selected, edges = colors[starts], np.exp2((exposures[ends[:-1]] + exposures[starts[1:]]) / 2.0)
            for array in (selected, edges):
                array.setflags(write=False)
            return Some(cls(colors=selected, edges=edges)).filter(lambda bands: bands.colors.shape[0] > 0)

        return view.form(config, np.repeat(np.exp2(exposures).astype(np.float32)[None, :, None], 3, axis=-1)).map(banded)


class Chromaticity(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """XY density with working and display primaries and white points."""

    trace: npt.NDArray[np.float64]
    working: npt.NDArray[np.float64]
    display: Option[npt.NDArray[np.float64]]

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32], view: DisplayView, cells: tuple[int, int]) -> "ImagingResult[Chromaticity]":
        """Returns XY counts at the requested x/y resolution and every independent build fault."""
        locus = observer.locus()

        def built(pair: tuple[tuple[PyOpenColorIO.Processor, npt.NDArray[np.float64]], Option[npt.NDArray[np.float64]]]) -> Chromaticity:
            (trace, working), display = pair
            points = observer.coordinates(applied(trace.getDefaultCPUProcessor(), plane)[..., :3][(plane[..., 3:] > 0.0).all(axis=-1)])
            counts = np.histogram2d(points[:, 0], points[:, 1], bins=cells, range=np.stack((locus.min(axis=0), locus.max(axis=0)), axis=-1))[0]
            for array in (counts, working, *display.to_list()):
                array.setflags(write=False)
            return cls(trace=counts, working=working, display=display)

        shown = view.colorspace(config).filter(lambda space: not space.isData()).map(lambda space: chromaticities(config, space.getName()).map(Some))
        return paired(paired(processor(config, xyz(config, PyOpenColorIO.ROLE_SCENE_LINEAR)), chromaticities(config, PyOpenColorIO.ROLE_SCENE_LINEAR)), shown.default_value(Ok(Nothing))).map(built)


# --- [OPERATIONS] -----------------------------------------------------------------------


def waveform(config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32], cells: tuple[int, int]) -> ImagingResult[npt.NDArray[np.float64]]:
    """Returns covered RGB/Y density planes with value rows and source-column groups."""

    def traced(samples: Samples) -> npt.NDArray[np.float64]:
        width, rows = min(cells[0], plane.shape[1]), cells[1]
        columns = np.broadcast_to(np.arange(plane.shape[1]), samples.covered.shape)[samples.covered]
        bins = (np.r_[-np.inf, np.arange(1, rows) / rows, np.inf], np.linspace(0.0, plane.shape[1], width + 1))
        values = np.column_stack((samples.color[samples.covered], samples.luminance[samples.covered]))
        traces = np.stack(tuple(np.histogram2d(channel, columns, bins=bins)[0] for channel in values.T))
        traces.setflags(write=False)
        return traces

    return Samples.of(config, plane).map(traced)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bands", "Chromaticity", "Clipping", "Histogram", "Vectorscope", "waveform"]
