"""Chart references, perspective chart sampling, and color correction fits."""

from collections.abc import Callable
import functools
import math
from typing import Annotated, Self

import colour
from colour.characterisation import ColourChecker
from colour.characterisation.datasets.colour_checkers.chromaticity_coordinates import CCS_COLORCHECKER24_AFTER_NOV2014
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.imaging.color.processors import basis, converted, matrix, xyz
from blender.imaging.color.white import adaptation
from blender.imaging.faults import ImagingResult
from blender.imaging.geometry.homography import Homography, Quad
from blender.imaging.geometry.panoramas import PanoramaView
from blender.imaging.geometry.stmaps import StMap
from blender.records.faults import DomainFault, NumericFault, paired
from blender.records.fields import Choice, Gated, Rgb

# --- [TYPES] ----------------------------------------------------------------------------


class Chart(Choice):
    """Published color charts."""

    COLORCHECKER_CLASSIC = 1, "ColorChecker Classic"
    SPYDERCHECKR_24 = 2, "SpyderCheckr 24"


type Finding = Mirrored | Skewed | Wide | Clipped

# --- [MODELS] ---------------------------------------------------------------------------

# --- [REFERENCES]


class ChartReference(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Chart data with neutral patch index."""

    checker: ColourChecker
    neutral: int

    @classmethod
    @functools.cache
    def of(cls, chart: Chart) -> Self:
        """Returns published reference data in printed grid order."""
        match chart:
            case Chart.COLORCHECKER_CLASSIC:
                return cls(checker=CCS_COLORCHECKER24_AFTER_NOV2014, neutral=21)
            case Chart.SPYDERCHECKR_24:
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
                return cls(
                    checker=ColourChecker(
                        chart.label,
                        dict(zip(patches, np.unstack(np.column_stack((values[:, :2] / values.sum(axis=-1, keepdims=True), values[:, 1]))))),
                        np.array((0.3457, 0.3585), dtype=np.float64),
                        4,
                        6,
                    ),
                    neutral=21,
                )

    @property
    def shape(self) -> npt.NDArray[np.int64]:
        """Patch columns and rows of chart grid."""
        return np.array((self.checker.columns, self.checker.rows), dtype=np.int64)

    def values(self, config: PyOpenColorIO.Config, colorspace: str = PyOpenColorIO.ROLE_SCENE_LINEAR) -> ImagingResult[tuple[Rgb[float], ...]]:
        """Returns white-adapted patch RGB in `colorspace`, or color faults."""
        coordinates = np.array(tuple(self.checker.data.values()), dtype=np.float64)
        illuminant = np.asarray(self.checker.illuminant, dtype=np.float64)
        white = np.append(illuminant, 1.0 - illuminant.sum()) / illuminant[1]
        patches = np.stack((coordinates[:, 0], coordinates[:, 1], 1.0 - coordinates[:, 0] - coordinates[:, 1]), axis=-1) * (coordinates[:, 2:] / coordinates[:, 1:2])
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
            .map(lambda rgb: msgspec.convert(rgb.tolist(), tuple[Rgb, ...]))
        )


# --- [PLACEMENTS]


class Mirrored(msgspec.Struct, frozen=True, kw_only=True):
    """Corner order reversing printed grid."""

    area: float


class Skewed(msgspec.Struct, frozen=True, kw_only=True):
    """Corner angles outside sampling limits."""

    narrowest: float
    widest: float


class Wide(msgspec.Struct, frozen=True, kw_only=True):
    """Panorama view wider than field-of-view limit."""

    field_of_view: float


class Clipped(msgspec.Struct, frozen=True, kw_only=True):
    """Patch indices with footprints past clipped source edge."""

    patches: tuple[int, ...]


class Limits(msgspec.Struct, frozen=True, kw_only=True):
    """Corner angle and panorama field-of-view limits."""

    narrowest: float = math.radians(60.0)
    widest: float = math.radians(120.0)
    field_of_view: float = math.radians(120.0)


class ChartPlacement(msgspec.Struct, frozen=True, kw_only=True):
    """Chart corners, patch size, and panorama alignment view."""

    chart: Chart = Chart.COLORCHECKER_CLASSIC
    quad: Annotated[Quad, msgspec.Meta(title="Corners")] = Quad()
    footprint: Annotated[float, msgspec.Meta(gt=0.0, le=1.0, title="Patch Size")] = 0.8
    panorama: Annotated[Gated[PanoramaView], msgspec.Meta(title="Panorama View")] = Gated(enabled=False, value=PanoramaView())

    @property
    def footprints(self) -> npt.NDArray[np.float64]:
        """Clockwise footprint corners in chart coordinates, top-left patch first."""
        reference = ChartReference.of(self.chart)
        rows, columns = np.divmod(np.arange(len(reference.checker.data)), reference.checker.columns)
        centers = (np.stack((columns, rows), axis=-1) + 0.5) / reference.shape * (1.0, -1.0) + (0.0, 1.0)
        planar = centers[:, np.newaxis] + (Quad().samples[:, :2] - 0.5) * self.footprint / reference.shape
        return np.concatenate((planar, np.ones_like(planar[..., :1])), axis=-1)

    @property
    def extension(self) -> tuple[OpenImageIO.Wrap, OpenImageIO.Wrap]:
        """Source edge wraps for direct or panorama sampling."""
        return self.panorama.value.source.extension if self.panorama.enabled else (OpenImageIO.Wrap.Black, OpenImageIO.Wrap.Black)

    @property
    def aligned(self) -> ImagingResult[Option[Homography]]:
        """Convex chart map in alignment frame, absent for undefined or nonconvex corners."""
        return (
            Some(self.panorama.value.to_view(self.quad.samples)).filter(lambda viewed: bool(np.all(viewed[:, 2] > 0.0))).map(lambda viewed: Homography.of(Quad.of(viewed))).default_value(Ok(Nothing))
            if self.panorama.enabled
            else Homography.of(self.quad)
        )

    def mapping(self, homography: Homography) -> Callable[[npt.NDArray[np.float64]], npt.NDArray[np.float64]]:
        """Returns chart-to-source map for aligned homography."""
        return (lambda samples: self.panorama.value.to_panorama(homography.to_image(samples))) if self.panorama.enabled else homography.to_image

    def findings(self, aspect: float, limits: Limits) -> ImagingResult[Option[tuple[Finding, ...]]]:
        """Returns placement findings, or absence when chart cannot align."""
        view = self.panorama.value.view
        clip = np.array([axis == OpenImageIO.Wrap.Black for axis in self.extension])

        def listed(homography: Homography) -> tuple[Finding, ...]:
            corners = homography.quad.samples[:, :2] * (view.aspect if self.panorama.enabled else aspect, 1.0)
            following = np.roll(corners, -1, axis=0)
            before, after = np.roll(corners, 1, axis=0) - corners, following - corners
            angles = np.arctan2(np.abs(before[:, 0] * after[:, 1] - before[:, 1] * after[:, 0]), np.vecdot(before, after))
            narrowest, widest = float(angles.min()), float(angles.max())
            area = float((corners[:, 0] * following[:, 1] - corners[:, 1] * following[:, 0]).sum() / 2.0)
            positions = self.mapping(homography)(self.footprints)
            clipped = tuple(np.flatnonzero(np.any((((positions[..., :2] < 0.0) | (positions[..., :2] > 1.0)) & clip), axis=(1, 2)) | np.any(positions[..., 2] <= 0.0, axis=1)).tolist())
            return (
                *((Mirrored(area=area),) if area > 0.0 else ()),
                *((Skewed(narrowest=narrowest, widest=widest),) if narrowest < limits.narrowest or widest > limits.widest else ()),
                *((Wide(field_of_view=view.field_of_view),) if self.panorama.enabled and view.field_of_view > limits.field_of_view else ()),
                *((Clipped(patches=clipped),) if clipped else ()),
            )

        return self.aligned.map(lambda aligned: aligned.map(listed))

    def means(self, plane: npt.NDArray[np.float32]) -> ImagingResult[Option[tuple[Rgb[float], ...]]]:
        """Returns alpha-weighted patch means, or absence when chart cannot align."""
        shape, channels = ChartReference.of(self.chart).shape, plane.shape[2]
        repeat = np.array([axis == OpenImageIO.Wrap.Periodic for axis in self.extension])

        def cells(samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            grid = samples[..., :2] * shape
            return np.concatenate(((np.floor(grid) + 0.5 + (grid % 1.0 - 0.5) * self.footprint) / shape, samples[..., 2:]), axis=-1)

        def sampled(homography: Homography) -> ImagingResult[tuple[Rgb[float], ...]]:
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
                .map(lambda means: msgspec.convert(means.tolist(), tuple[Rgb, ...]))
            )

        return self.aligned.bind(lambda aligned: aligned.map(lambda homography: sampled(homography).map(Some)).default_value(Ok(Nothing)))

    def framed(self, share: float) -> Option[Self]:
        """Returns placement with view framing its corners, or absence for undefined view."""
        return self.panorama.value.framing(self.quad, share).map(lambda view: msgspec.structs.replace(self, panorama=Gated(enabled=True, value=view)))

    def realigned(self, quad: Quad) -> Self:
        """Stores edited alignment-frame corners back in source coordinates."""
        return msgspec.structs.replace(self, quad=Quad.of(self.panorama.value.to_panorama(quad.samples)) if self.panorama.enabled else quad)


# --- [FITS]


class Calibration(msgspec.Struct, frozen=True, kw_only=True):
    """Output-channel RGB rows with factored exposure in stops."""

    gained: tuple[Rgb[float], Rgb[float], Rgb[float]] = (Rgb(red=1.0, green=0.0, blue=0.0), Rgb(red=0.0, green=1.0, blue=0.0), Rgb(red=0.0, green=0.0, blue=1.0))
    exposure: float = 0.0

    @classmethod
    def fit(cls, samples: tuple[tuple[Rgb[float], Rgb[float]], ...]) -> ImagingResult["CalibrationFit"]:
        """Returns the Cheung 2004 linear fit of measured onto reference colors with its inverse, or numeric and ill-conditioned fit faults."""
        source, target = np.unstack(np.array(samples, dtype=np.float64).reshape(-1, 2, 3), axis=1)
        try:
            fitted = np.asarray(colour.matrix_colour_correction(source, target, method="Cheung 2004"), dtype=np.float64)
        except np.linalg.LinAlgError as error:
            return Error((NumericFault(request=source, error=error),))
        condition, ceiling = float(np.linalg.cond(fitted)), 1.0 / (len(fitted) * float(np.finfo(np.float64).eps))
        if not condition < ceiling:
            return Error((DomainFault(subject="Calibration fit", value=condition, accepted=f"matrix condition below {ceiling:.3g}"),))
        forward, inverse = (msgspec.convert({"gained": rows.tolist()}, cls) for rows in (fitted, np.linalg.inv(fitted)))
        return Ok(CalibrationFit(forward=forward, inverse=inverse, condition=condition))

    def normalized(
        self, source: Rgb[float], target: Rgb[float], config: PyOpenColorIO.Config, *, source_space: str = PyOpenColorIO.ROLE_SCENE_LINEAR, target_space: str = PyOpenColorIO.ROLE_SCENE_LINEAR
    ) -> ImagingResult[Option[Self]]:
        """Returns calibration with neutral luminance ratio as exposure, or absence for nonpositive luminance."""

        def factored(matrices: tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]) -> Option[Self]:
            source_luma, target_luma = (float(held[1] @ np.asarray(sample)) for held, sample in zip(matrices, (source, target)))
            return Some(msgspec.structs.replace(self, exposure=math.log2(target_luma) - math.log2(source_luma))) if source_luma > 0.0 and target_luma > 0.0 else Nothing

        return paired(basis(config, source_space), basis(config, target_space)).map(factored)

    @property
    def matrix(self) -> tuple[Rgb[float], Rgb[float], Rgb[float]]:
        """RGB rows without factored exposure."""
        gain = math.exp2(-self.exposure)
        rows = tuple(Rgb(red=row.red * gain, green=row.green * gain, blue=row.blue * gain) for row in self.gained)
        return rows[0], rows[1], rows[2]

    @property
    def transform(self) -> PyOpenColorIO.MatrixTransform:
        """OCIO matrix transform with exposure."""
        return matrix(np.array(self.gained, dtype=np.float64))


class CalibrationFit(msgspec.Struct, frozen=True, kw_only=True):
    """Forward and inverse calibrations with the fitted matrix's condition number."""

    forward: Calibration
    inverse: Calibration
    condition: float


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Calibration", "CalibrationFit", "Chart", "ChartPlacement", "ChartReference", "Clipped", "Finding", "Limits", "Mirrored", "Skewed", "Wide"]
