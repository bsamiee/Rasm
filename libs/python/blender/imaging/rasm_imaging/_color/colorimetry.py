"""CIE 1931 2° observer, blackbody emission, xy chromaticity, spectral RGB matching, Robertson white points, and Bradford adaptation."""

from functools import cache, partial
from typing import Annotated, ClassVar, Self

import colour
from colour.hints import NDArrayFloat
from colour.temperature.robertson1968 import DATA_ISOTEMPERATURE_LINES_ROBERTSON1968
from expression import Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm_imaging._color.processors import basis, converted, xyz
from rasm_records import Error, Fault, Ok, Outcome, Reason

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
    def of(cls, tristimulus: npt.NDArray[np.float64]) -> Option[Self]:
        """Solves the white point of a neutral XYZ pick of shape `(3,)`, absent off the uv plane, at or past the hottest isotherm, or outside field bounds."""

        def solved(ucs: NDArrayFloat) -> Option[Self]:
            temperature, duv = colour.temperature.uv_to_CCT_Robertson1968(colour.UCS_to_uv(ucs)).tolist()
            return catch(lambda: msgspec.convert({"temperature": temperature, "duv": duv}, type=cls), exception=msgspec.ValidationError)().to_option()

        return Some(colour.XYZ_to_UCS(tristimulus)).filter(lambda ucs: float(ucs.sum()) > 0.0).bind(solved).filter(lambda white: white.temperature < cls.hottest)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [OBSERVER]


@cache
def observer() -> npt.NDArray[np.float64]:
    """Returns read-only float64 rows (λ, x̄, ȳ, z̄) at every nanometer from 360 to 830 nm."""
    matching = colour.MSDS_CMFS["CIE 1931 2 Degree Standard Observer"]
    rows = np.column_stack((matching.wavelengths, matching.values)).astype(np.float64, copy=False)
    rows.setflags(write=False)
    return rows


def spectral_samples(count: int) -> npt.NDArray[np.float64]:
    """Returns rows (λ, x̄, ȳ, z̄) at `count` midpoint wavelengths spanning the table, long wavelengths first."""
    rows = observer()
    wavelengths = rows[-1, 0] - (np.arange(count, dtype=np.float64) + 0.5) * (rows[-1, 0] - rows[0, 0]) / count
    return np.column_stack((wavelengths, *(np.interp(wavelengths, rows[:, 0], column) for column in rows[:, 1:].T)))


def blackbody(temperatures: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
    """Returns float64 XYZ at Y = 1 of Planckian emission at each kelvin temperature, integrated against the observer."""
    rows = observer()
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
    return coordinates(observer()[:, 1:])


@cache
def diagram_extent() -> npt.NDArray[np.float64]:
    """Returns the read-only diagram frame's far xy corner from the origin, the spectral locus maxima rounded up to tenths."""
    extent: npt.NDArray[np.float64] = np.ceil(locus().max(axis=0) * 10.0) / 10.0
    extent.setflags(write=False)
    return extent


def chromaticities(config: PyOpenColorIO.Config, space: str) -> Outcome[npt.NDArray[np.float64]]:
    """Returns float64 xy rows of red, green, blue, and white from unit codes every display encoding fixes, or `FAILED` fault."""
    return converted(config, xyz(config, space), np.vstack((np.eye(3, dtype=np.float32), np.ones((1, 3), dtype=np.float32)))).map(coordinates)


# --- [MATCHING]


def rgb(config: PyOpenColorIO.Config, space: str, tristimulus: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float64]]:
    """Returns linear RGB rows in `space` of XYZ `tristimulus` rows, or `FAILED` fault for a singular basis."""

    @catch(exception=np.linalg.LinAlgError)
    def solved(values: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        return np.asarray(np.linalg.solve(values, tristimulus.T).T, dtype=np.float64)

    return basis(config, space).bind(lambda values: solved(values).map_error(partial(Fault.of, Reason.FAILED, space)))


def matching(config: PyOpenColorIO.Config, space: str, tristimulus: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float64]]:
    """Returns signed RGB matching weights of observer `tristimulus` rows in `space`, each channel integrating to one."""

    @np.errstate(all="ignore")
    def normalized(weights: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float64]]:
        integral = weights.sum(axis=0)
        held = np.isfinite(weights).all() and np.isfinite(integral).all() and (integral != 0.0).all()
        return Ok(weights / integral) if held else Error(Fault.of(Reason.INVALID, space, integral, member="finite spectral weights with nonzero finite RGB integrals"))

    return rgb(config, space, tristimulus).bind(normalized)


def adaptation(source: npt.NDArray[np.float64], target: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
    """Returns float64 Bradford matrix between whites normalized to Y = 1."""
    return np.asarray(colour.adaptation.matrix_chromatic_adaptation_VonKries(source / source[1], target / target[1], transform="Bradford"), dtype=np.float64)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["WhitePoint", "adaptation", "blackbody", "chromaticities", "coordinates", "diagram_extent", "locus", "matching", "observer", "rgb", "spectral_samples"]
