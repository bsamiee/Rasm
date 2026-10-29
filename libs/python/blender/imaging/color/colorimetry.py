"""CIE 1931 2° observer, blackbody emission, xy chromaticity, spectral RGB matching, Robertson white points, and Bradford adaptation."""

from functools import cache, partial
from typing import Annotated, ClassVar, Self

import colour
from colour.hints import NDArrayFloat
from colour.temperature.robertson1968 import DATA_ISOTEMPERATURE_LINES_ROBERTSON1968
from expression import Error, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.imaging.color.processors import basis, converted, xyz
from blender.imaging.faults import ImagingReason
from blender.records.faults import Fault, Outcome, RecordReason

# --- [MODELS] ---------------------------------------------------------------------------


class WhitePoint(msgspec.Struct, frozen=True, kw_only=True):
    """Color temperature and signed Duv from the Planckian locus, bounded by the isotherms Robertson 1968 inverts."""

    coolest: ClassVar[float] = float(colour.temperature.mired_to_CCT(DATA_ISOTEMPERATURE_LINES_ROBERTSON1968[-1][0]))
    hottest: ClassVar[float] = float(colour.temperature.mired_to_CCT(DATA_ISOTEMPERATURE_LINES_ROBERTSON1968[1][0]))
    temperature: Annotated[float, msgspec.Meta(ge=coolest, le=hottest)] = 6500.0
    duv: Annotated[float, msgspec.Meta(ge=-0.05, le=0.05)] = 0.0

    @property
    def xyz(self) -> npt.NDArray[np.float64]:
        """Float64 XYZ at Y = 1."""
        return np.asarray(colour.xy_to_XYZ(colour.UCS_uv_to_xy(colour.temperature.CCT_to_uv_Robertson1968(np.array((self.temperature, self.duv))))), dtype=np.float64)

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, color: npt.NDArray[np.float64]) -> Outcome[Option[Self]]:
        """Solves the white point of a scene-linear neutral pick of shape `(3,)`, absent off the uv plane, at or past the hottest isotherm, or outside field bounds."""

        def solved(ucs: NDArrayFloat) -> Option[Self]:
            temperature, duv = colour.temperature.uv_to_CCT_Robertson1968(colour.UCS_to_uv(ucs)).tolist()
            return catch(msgspec.convert, exception=msgspec.ValidationError)({"temperature": temperature, "duv": duv}, type=cls).to_option()

        return basis(config, PyOpenColorIO.ROLE_SCENE_LINEAR).map(
            lambda matrix: Some(colour.XYZ_to_UCS(matrix @ color)).filter(lambda ucs: float(ucs.sum()) > 0.0).bind(solved).filter(lambda white: white.temperature < cls.hottest)
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [OBSERVER]


@cache
def table() -> npt.NDArray[np.float64]:
    """Returns read-only float64 rows (λ, x̄, ȳ, z̄) at every nanometer from 360 to 830 nm."""
    matching = colour.MSDS_CMFS["CIE 1931 2 Degree Standard Observer"]
    rows = np.column_stack((matching.wavelengths, matching.values)).astype(np.float64)
    rows.setflags(write=False)
    return rows


def sampled(count: int) -> npt.NDArray[np.float64]:
    """Returns rows (λ, x̄, ȳ, z̄) at `count` midpoint wavelengths spanning the table, long wavelengths first."""
    rows = table()
    wavelengths = rows[-1, 0] - (np.arange(count, dtype=np.float64) + 0.5) * (rows[-1, 0] - rows[0, 0]) / count
    return np.column_stack((wavelengths, *(np.interp(wavelengths, rows[:, 0], column) for column in rows[:, 1:].T)))


def blackbody(temperatures: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
    """Returns float64 XYZ at Y = 1 of Planckian emission at each kelvin temperature, integrated against the observer."""
    rows = table()
    tristimulus = colour.colorimetry.planck_law(rows[:, 0] * 1e-9, temperatures).reshape(len(rows), -1).T.reshape(*temperatures.shape, len(rows)) @ rows[:, 1:]
    return tristimulus / tristimulus[..., 1:2]


# --- [CHROMATICITY]


def coordinates(tristimulus: npt.NDArray[np.float32 | np.float64]) -> npt.NDArray[np.float64]:
    """Returns float64 xy over last axis, NaN where X + Y + Z is nonpositive."""
    xy = tristimulus[..., :2]
    total = tristimulus.sum(axis=-1, keepdims=True, dtype=np.float64)
    return np.asarray(np.divide(xy, total, out=np.full_like(xy, np.nan, dtype=np.float64), where=total > 0.0), dtype=np.float64)


def locus() -> npt.NDArray[np.float64]:
    """Returns spectral xy coordinates in wavelength order as an open curve."""
    return coordinates(table()[:, 1:])


@cache
def frame() -> tuple[npt.NDArray[np.float64], npt.NDArray[np.intp]]:
    """Returns the diagram frame's far xy corner from the origin, the spectral locus maxima rounded up to tenths, and its tenths per axis."""
    tenths = np.ceil(locus().max(axis=0) * 10.0).astype(np.intp)
    extent = tenths / 10.0
    for array in (extent, tenths):
        array.setflags(write=False)
    return extent, tenths


def chromaticities(config: PyOpenColorIO.Config, space: str) -> Outcome[npt.NDArray[np.float64]]:
    """Returns float64 xy rows of red, green, blue, and white from unit codes every display encoding fixes, or color fault."""
    return converted(config, xyz(config, space), np.vstack((np.eye(3, dtype=np.float32), np.ones((1, 3), dtype=np.float32)))).map(coordinates)


# --- [MATCHING]


def matching(config: PyOpenColorIO.Config, space: str, tristimulus: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float64]]:
    """Returns signed RGB matching weights of observer `tristimulus` rows in `space`, each channel integrating to one."""

    @catch(exception=np.linalg.LinAlgError)
    def solved(values: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        return np.asarray(np.linalg.solve(values, tristimulus.T).T, dtype=np.float64)

    @np.errstate(all="ignore")
    def normalized(weights: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float64]]:
        integral = weights.sum(axis=0)
        held = np.isfinite(weights).all() and np.isfinite(integral).all() and (integral != 0.0).all()
        return Ok(weights / integral) if held else Error(Fault.of(ImagingReason.SPECTRAL, space, member=str(integral)))

    return basis(config, space).bind(lambda values: solved(values).map_error(partial(Fault.of, RecordReason.NUMERIC, str(values)))).bind(normalized)


def adaptation(source: npt.NDArray[np.float64], target: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
    """Returns float64 Bradford matrix between whites normalized to Y = 1."""
    return np.asarray(colour.adaptation.matrix_chromatic_adaptation_VonKries(source / source[1], target / target[1], transform="Bradford"), dtype=np.float64)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["WhitePoint", "adaptation", "blackbody", "chromaticities", "coordinates", "frame", "locus", "matching", "sampled", "table"]
