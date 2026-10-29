"""Published chart data, perspective chart sampling, and color correction fits."""

from collections.abc import Callable
from enum import IntEnum
import functools
import math
import sys
from typing import Annotated, Final, Self

import colour
from colour.characterisation import ColourChecker
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.imaging.color.colorimetry import adaptation
from blender.imaging.color.processors import basis, converted, matrix, Stops, xyz
from blender.imaging.geometry.frames import Homography, Quad, StMap
from blender.imaging.geometry.panoramas import PanoramaView
from blender.records.faults import Fault, Outcome, paired, Reason
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------


class PublishedChart(IntEnum):
    """Published color charts, each holding its reference data in printed grid order."""

    COLORCHECKER_CLASSIC = 1
    SPYDERCHECKR_24 = 2

    @functools.cached_property
    def checker(self) -> ColourChecker:
        """Published patch chromaticities, illuminant, and grid."""
        match self:
            case PublishedChart.COLORCHECKER_CLASSIC:
                published: ColourChecker = colour.CCS_COLOURCHECKERS["ColorChecker24 - After November 2014"]
                return published
            case PublishedChart.SPYDERCHECKR_24:
                patches = {
                    "H1 (Bluish Green)": (0.3076822512, 0.4234772396, 0.3374520529),
                    "H2 (Blue Flower)": (0.2452835592, 0.2348654227, 0.3416765010),
                    "H3 (Foliage)": (0.1037198781, 0.1279977883, 0.0523456534),
                    "H4 (Blue Sky)": (0.1692686551, 0.1858660205, 0.2666809013),
                    "H5 (Light Skin)": (0.3952982368, 0.3553099128, 0.1928659515),
                    "H6 (Dark Skin)": (0.1113998964, 0.0966807861, 0.0490883941),
                    "G1 (Orange)": (0.3888763433, 0.2946518384, 0.0431458486),
                    "G2 (Purplish Blue)": (0.1172593609, 0.1098191865, 0.2771796138),
                    "G3 (Moderate Red)": (0.2996723782, 0.1951990129, 0.1039804505),
                    "G4 (Purple)": (0.0917247784, 0.0693130081, 0.1219016829),
                    "G5 (Yellow Green)": (0.3562702838, 0.4446769162, 0.0880911399),
                    "G6 (Orange Yellow)": (0.5038320114, 0.4391253963, 0.0603927928),
                    "F1 (Cyan)": (0.1208589631, 0.1795378021, 0.2854043683),
                    "F2 (Magenta)": (0.2990686680, 0.1930314394, 0.2271268085),
                    "F3 (Yellow)": (0.6242483660, 0.6326192430, 0.0712084288),
                    "F4 (Red)": (0.2194131115, 0.1230703602, 0.0384412857),
                    "F5 (Green)": (0.1438849866, 0.2259240844, 0.0741006071),
                    "F6 (Blue)": (0.0669023563, 0.0544993983, 0.2116807502),
                    "E1 (White 9.5)": (0.8886310787, 0.9226861820, 0.7490419119),
                    "E2 (Neutral 8)": (0.5669540573, 0.5894253744, 0.4798162851),
                    "E3 (Neutral 6.5)": (0.3432548507, 0.3572595422, 0.2897947815),
                    "E4 (Neutral 5)": (0.1838367508, 0.1913948585, 0.1556906989),
                    "E5 (Neutral 3.5)": (0.0807065939, 0.0838392766, 0.0677486804),
                    "E6 (Black 2)": (0.0248551698, 0.0258980980, 0.0222499522),
                }
                values = np.array(tuple(patches.values()), dtype=np.float64)
                return ColourChecker("SpyderCheckr 24", dict(zip(patches, np.unstack(colour.XYZ_to_xyY(values)))), np.array((0.3457, 0.3585), dtype=np.float64), 4, 6)

    @property
    def neutral(self) -> int:
        """Index of the middle gray patch."""
        return 21

    @property
    def shape(self) -> npt.NDArray[np.int64]:
        """Patch columns and rows of chart grid."""
        return np.array((self.checker.columns, self.checker.rows), dtype=np.int64)

    def values(self, config: PyOpenColorIO.Config, colorspace: str = PyOpenColorIO.ROLE_SCENE_LINEAR) -> Outcome[tuple[tuple[float, float, float], ...]]:
        """Returns white-adapted patch RGB in `colorspace`, or color faults."""
        patches = colour.xyY_to_XYZ(np.array(tuple(self.checker.data.values()), dtype=np.float64))
        white = np.asarray(colour.xy_to_XYZ(self.checker.illuminant), dtype=np.float64)
        return (
            basis(config, PyOpenColorIO.ROLE_SCENE_LINEAR)
            .bind(
                lambda working: converted(
                    config,
                    PyOpenColorIO.GroupTransform([
                        matrix(adaptation(white, working.sum(axis=1))),
                        PyOpenColorIO.GroupTransform([xyz(config, colorspace)], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE),
                    ]),
                    patches.astype(np.float32),
                )
            )
            .map(lambda rgb: msgspec.convert(rgb.tolist(), tuple[tuple[float, float, float], ...]))
        )


type Finding = Reversed | Skewed | Wide | Clipped

# --- [CONSTANTS] ------------------------------------------------------------------------

_CEILING: Final = 1.0 / (3 * float(np.finfo(np.float64).eps))

# --- [MODELS] ---------------------------------------------------------------------------

# --- [PLACEMENTS]


class Reversed(Case, frozen=True, kw_only=True):
    """Corner order reversing printed grid."""

    area: float


class Skewed(Case, frozen=True, kw_only=True):
    """Corner angles outside sampling limits."""

    narrowest: float
    widest: float


class Wide(Case, frozen=True, kw_only=True):
    """Panorama view wider than field-of-view limit."""

    field_of_view: float


class Clipped(Case, frozen=True, kw_only=True):
    """Patch indices with footprints past clipped source edge."""

    patches: tuple[int, ...]


class ChartPlacement(msgspec.Struct, frozen=True, kw_only=True):
    """Chart corners, patch size, and panorama alignment view, direct sampling while absent."""

    chart: PublishedChart = PublishedChart.COLORCHECKER_CLASSIC
    quad: Quad = Quad()
    footprint: Annotated[float, msgspec.Meta(ge=sys.float_info.min, le=1.0)] = 0.8
    panorama: PanoramaView | None = None

    @property
    def footprints(self) -> npt.NDArray[np.float64]:
        """Clockwise footprint corners in chart coordinates, top-left patch first."""
        chart = self.chart
        rows, columns = np.divmod(np.arange(len(chart.checker.data)), chart.checker.columns)
        centers = (np.stack((columns, rows), axis=-1) + 0.5) / chart.shape * (1.0, -1.0) + (0.0, 1.0)
        planar = centers[:, np.newaxis] + (Quad().samples[:, :2] - 0.5) * self.footprint / chart.shape
        return np.concatenate((planar, np.ones_like(planar[..., :1])), axis=-1)

    @property
    def extension(self) -> tuple[OpenImageIO.Wrap, OpenImageIO.Wrap]:
        """Source edge wraps for direct or panorama sampling."""
        return Option.of_optional(self.panorama).map(lambda panorama: panorama.source.extension).default_value((OpenImageIO.Wrap.Black, OpenImageIO.Wrap.Black))

    @property
    def aligned(self) -> Outcome[Option[Homography]]:
        """Convex chart map in alignment frame, absent for undefined or nonconvex corners."""
        return (
            Option
            .of_optional(self.panorama)
            .map(
                lambda panorama: (
                    Some(panorama.to_view(self.quad.samples)).filter(lambda viewed: bool(np.all(viewed[:, 2] > 0.0))).map(lambda viewed: Homography.of(Quad.of(viewed))).default_value(Ok(Nothing))
                )
            )
            .default_with(lambda: Homography.of(self.quad))
        )

    def mapping(self, homography: Homography) -> Callable[[npt.NDArray[np.float64]], npt.NDArray[np.float64]]:
        """Returns chart-to-source map for aligned homography."""
        return Option.of_optional(self.panorama).map(lambda panorama: lambda samples: panorama.to_panorama(homography.to_image(samples))).default_value(homography.to_image)

    def findings(
        self, aspect: float, /, *, narrowest: float = math.radians(60.0), widest: float = math.radians(120.0), field_of_view: float = math.radians(120.0)
    ) -> Outcome[Option[tuple[Finding, ...]]]:
        """Returns findings against corner angle and panorama field-of-view limits, or absence when chart cannot align."""
        view = Option.of_optional(self.panorama).map(lambda panorama: panorama.view)
        clip = np.array([axis == OpenImageIO.Wrap.Black for axis in self.extension])

        def listed(homography: Homography) -> tuple[Finding, ...]:
            corners = homography.quad.samples[:, :2] * (view.map(lambda held: held.aspect).default_value(aspect), 1.0)
            following = np.roll(corners, -1, axis=0)
            before, after = np.roll(corners, 1, axis=0) - corners, following - corners
            angles = np.arctan2(np.abs(before[:, 0] * after[:, 1] - before[:, 1] * after[:, 0]), np.vecdot(before, after))
            least, most = float(angles.min()), float(angles.max())
            area = float((corners[:, 0] * following[:, 1] - corners[:, 1] * following[:, 0]).sum() / 2.0)
            positions = self.mapping(homography)(self.footprints)
            clipped = tuple(np.flatnonzero(np.any((((positions[..., :2] < 0.0) | (positions[..., :2] > 1.0)) & clip), axis=(1, 2)) | np.any(positions[..., 2] <= 0.0, axis=1)).tolist())
            return (
                *((Reversed(area=area),) if area > 0.0 else ()),
                *((Skewed(narrowest=least, widest=most),) if least < narrowest or most > widest else ()),
                *(Wide(field_of_view=held.field_of_view) for held in view.to_list() if held.field_of_view > field_of_view),
                *((Clipped(patches=clipped),) if clipped else ()),
            )

        return self.aligned.map(lambda aligned: aligned.map(listed))

    def means(self, plane: npt.NDArray[np.float32]) -> Outcome[Option[tuple[tuple[float, float, float], ...]]]:
        """Returns alpha-weighted patch means, or absence when chart cannot align."""
        shape, channels = self.chart.shape, plane.shape[2]
        repeat = np.array([axis == OpenImageIO.Wrap.Periodic for axis in self.extension])

        def cells(samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            grid = samples[..., :2] * shape
            return np.concatenate(((np.floor(grid) + 0.5 + (grid % 1.0 - 0.5) * self.footprint) / shape, samples[..., 2:]), axis=-1)

        def sampled(homography: Homography) -> Outcome[tuple[tuple[float, float, float], ...]]:
            mapping = self.mapping(homography)
            corners = mapping(Quad().samples)[:, :2]
            edges = np.roll(corners, -1, axis=0) - corners
            lengths = np.hypot.reduce(np.where(repeat, (edges + 0.5) % 1.0 - 0.5, edges) * plane.shape[1::-1], axis=1)
            side = max(1, math.ceil(self.footprint * float(np.min((lengths[:2] + lengths[2:]) / (2.0 * shape)))))
            width, height = (int(size) * side for size in shape)

            return (
                StMap
                .bake(width, height, lambda samples: mapping(cells(samples)), self.extension, source_size=(plane.shape[1], plane.shape[0]))
                .warp(plane)
                .map(lambda warped: warped.reshape(shape[1], side, shape[0], side, channels).sum(axis=(1, 3), dtype=np.float64).reshape(-1, channels))
                .map(lambda sums: np.divide(sums[:, :3], sums[:, 3:], out=np.zeros_like(sums[:, :3]), where=sums[:, 3:] > 0.0))
                .map(lambda means: msgspec.convert(means.tolist(), tuple[tuple[float, float, float], ...]))
            )

        return self.aligned.bind(lambda aligned: aligned.map(lambda homography: sampled(homography).map(Some)).default_value(Ok(Nothing)))

    def framed(self, share: float) -> Option[Self]:
        """Returns placement with view framing its corners, or absence for undefined view."""
        return Option.of_optional(self.panorama).default_with(PanoramaView).framing(self.quad, share).map(lambda view: msgspec.structs.replace(self, panorama=view))

    def realigned(self, quad: Quad) -> Self:
        """Stores edited alignment-frame corners back in source coordinates."""
        return msgspec.structs.replace(self, quad=Option.of_optional(self.panorama).map(lambda panorama: Quad.of(panorama.to_panorama(quad.samples))).default_value(quad))


# --- [FITS]


class Calibration(msgspec.Struct, frozen=True, kw_only=True):
    """Invertible output-channel RGB rows with factored exposure in stops."""

    gained: tuple[tuple[float, float, float], tuple[float, float, float], tuple[float, float, float]] = ((1.0, 0.0, 0.0), (0.0, 1.0, 0.0), (0.0, 0.0, 1.0))
    exposure: Stops = 0.0

    def __post_init__(self) -> None:
        """Refuses rows whose condition number reaches the float64 inversion ceiling."""
        if not self.condition < _CEILING:
            raise ValueError(f"matrix condition {self.condition:.3g} reaches {_CEILING:.3g}")

    @classmethod
    def fit(cls, samples: tuple[tuple[tuple[float, float, float], tuple[float, float, float]], ...]) -> Outcome[Self]:
        """Returns the Cheung 2004 linear fit of measured onto reference colors, or numeric and ill-conditioned fit faults."""
        source, target = np.unstack(np.array(samples, dtype=np.float64).reshape(-1, 2, 3), axis=1)
        return (
            catch(colour.matrix_colour_correction, exception=np.linalg.LinAlgError)(source, target, method="Cheung 2004")
            .map_error(functools.partial(Fault.of, Reason.FAILED, str(source)))
            .bind(
                lambda found: catch(msgspec.convert, exception=msgspec.ValidationError)({"gained": np.asarray(found, dtype=np.float64).tolist()}, cls).map_error(
                    functools.partial(Fault.of, Reason.OUTSIDE, cls.__name__, member=f"matrix condition below {_CEILING:.3g}")
                )
            )
        )

    @property
    def condition(self) -> float:
        """Condition number of the RGB rows."""
        return float(np.linalg.cond(np.array(self.gained, dtype=np.float64)))

    @property
    def inverted(self) -> Self:
        """Calibration undoing this one, its exposure negated."""
        return msgspec.convert({"gained": np.linalg.inv(np.array(self.gained, dtype=np.float64)).tolist(), "exposure": -self.exposure}, type(self))

    def normalized(
        self,
        source: tuple[float, float, float],
        target: tuple[float, float, float],
        config: PyOpenColorIO.Config,
        *,
        source_space: str = PyOpenColorIO.ROLE_SCENE_LINEAR,
        target_space: str = PyOpenColorIO.ROLE_SCENE_LINEAR,
    ) -> Outcome[Option[Self]]:
        """Returns calibration with neutral luminance ratio as exposure, or absence for nonpositive luminance or a ratio past the stop range."""

        def factored(matrices: tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]) -> Option[Self]:
            source_luma, target_luma = (float(held[1] @ np.asarray(sample)) for held, sample in zip(matrices, (source, target)))
            return (
                catch(msgspec.convert, exception=msgspec.ValidationError)(math.log2(target_luma) - math.log2(source_luma), Stops).to_option() if source_luma > 0.0 and target_luma > 0.0 else Nothing
            ).map(lambda exposure: msgspec.structs.replace(self, exposure=exposure))

        return paired(basis(config, source_space), basis(config, target_space)).map(factored)

    @property
    def matrix(self) -> npt.NDArray[np.float64]:
        """RGB rows without factored exposure."""
        return np.array(self.gained, dtype=np.float64) * math.exp2(-self.exposure)

    @property
    def transform(self) -> PyOpenColorIO.MatrixTransform:
        """OCIO matrix transform with exposure."""
        return matrix(np.array(self.gained, dtype=np.float64))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Calibration", "ChartPlacement", "Clipped", "Finding", "PublishedChart", "Reversed", "Skewed", "Wide"]
