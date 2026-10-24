"""CIE 1931 2° matching functions, spectral locus, and XYZ chromaticity."""

import functools

from colour.colorimetry.datasets import cmfs
import numpy as np
import numpy.typing as npt

# --- [OPERATIONS] -----------------------------------------------------------------------


@functools.cache
def table() -> npt.NDArray[np.float64]:
    """Returns read-only float64 rows (λ, x̄, ȳ, z̄) at every nanometer from 360 to 830 nm."""
    matching = cmfs.DATA_CMFS_STANDARD_OBSERVER["CIE 1931 2 Degree Standard Observer"]
    rows = np.array([(wavelength, *values) for wavelength, values in matching.items()], dtype=np.float64)
    rows.setflags(write=False)
    return rows


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

__all__ = ["coordinates", "frame", "locus", "table"]
