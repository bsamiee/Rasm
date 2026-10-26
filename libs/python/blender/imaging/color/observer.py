"""CIE 1931 2° matching functions, spectral samples, blackbody emission, spectral locus, and XYZ chromaticity."""

import functools

import colour
import numpy as np
import numpy.typing as npt

# --- [OPERATIONS] -----------------------------------------------------------------------


@functools.cache
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
    shape = colour.SpectralShape(rows[0, 0], rows[-1, 0], 1.0)
    emitted = np.stack([colour.sd_blackbody(float(temperature), shape).values for temperature in temperatures.reshape(-1)]).reshape(*temperatures.shape, len(rows))
    tristimulus = emitted @ rows[:, 1:]
    return tristimulus / tristimulus[..., 1:2]


def locus() -> npt.NDArray[np.float64]:
    """Returns spectral xy coordinates in wavelength order as an open curve."""
    return coordinates(table()[:, 1:])


@functools.cache
def frame() -> tuple[npt.NDArray[np.float64], npt.NDArray[np.intp]]:
    """Returns the diagram frame's far xy corner from the origin, the spectral locus maxima rounded up to tenths, and its tenths per axis."""
    tenths = np.ceil(locus().max(axis=0) * 10.0).astype(np.intp)
    extent = tenths / 10.0
    for array in (extent, tenths):
        array.setflags(write=False)
    return extent, tenths


def coordinates(xyz: npt.NDArray[np.float32 | np.float64]) -> npt.NDArray[np.float64]:
    """Returns float64 xy over last axis, NaN where X + Y + Z is nonpositive."""
    xy = xyz[..., :2]
    total = xyz.sum(axis=-1, keepdims=True, dtype=np.float64)
    return np.divide(xy, total, out=np.full_like(xy, np.nan, dtype=np.float64), where=total > 0.0)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["blackbody", "coordinates", "frame", "locus", "sampled", "table"]
