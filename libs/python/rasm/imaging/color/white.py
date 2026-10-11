"""Robertson temperature and tint, scene-linear white picks, and Bradford adaptation."""

import functools
from typing import Annotated, Self

from colour.adaptation import CAT_BRADFORD
from colour.temperature import robertson1968
from expression import Nothing, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm.imaging.color.processors import applied, processor, xyz
from rasm.imaging.faults import ImagingResult
from rasm.imaging.parameters import Presentation, Quantity, Rgb

# --- [MODELS] ---------------------------------------------------------------------------


class White(msgspec.Struct, frozen=True, kw_only=True):
    """Correlated color temperature and signed tint from the Planckian locus."""

    temperature: Annotated[
        float,
        msgspec.Meta(ge=1800.0, le=100000.0, description="Correlated color temperature of the white"),
        Presentation(soft=Some((2000.0, 11000.0)), step=Some(1.0), decimals=Some(0), quantity=Some(Quantity.COLOR_TEMPERATURE)),
    ] = 6500.0
    tint: Annotated[float, msgspec.Meta(ge=-150.0, le=150.0, description="Offset from the Planckian locus, positive toward green"), Presentation(step=Some(0.01), decimals=Some(1))] = 10.0

    @staticmethod
    @functools.cache
    def locus() -> npt.NDArray[np.float64]:
        """Returns read-only published Robertson rows of mired, u, v, and isotherm slope."""
        rows = np.array(robertson1968.DATA_ISOTEMPERATURE_LINES_ROBERTSON1968, dtype=np.float64)
        rows.setflags(write=False)
        return rows

    @property
    def xyz(self) -> npt.NDArray[np.float64]:
        """Float64 XYZ at Y = 1 with Blender's normalized-isotherm tint."""
        mired, u, v, slope = self.locus().T
        reciprocal, normal = 1e6 / self.temperature, np.hypot(1.0, slope)
        isotherm = np.array([np.interp(reciprocal, mired, row) for row in (1.0 / normal, slope / normal)], dtype=np.float64)
        uv = np.array([np.interp(reciprocal, mired, u), np.interp(reciprocal, mired, v)], dtype=np.float64) - isotherm / np.linalg.norm(isotherm) * (self.tint / 3000.0)
        return np.array((3.0 * uv[0], 2.0 * uv[1], 4.0 - uv[0] - 10.0 * uv[1]), dtype=np.float64) / float(2.0 * uv[1])

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, color: Rgb) -> ImagingResult[Option[Self]]:
        """Solves a neutral pick, distinguishing processor faults from colors outside the white domain."""

        def solved(built: PyOpenColorIO.Processor) -> Option[Self]:
            tristimulus = applied(built.getDefaultCPUProcessor(), np.array(msgspec.structs.astuple(color), dtype=np.float32)).astype(np.float64)
            if (total := (float(tristimulus @ (1.0, 15.0, 3.0)))) <= 0.0:
                return Nothing
            rows, uv = cls.locus(), tristimulus[:2] * (4.0, 6.0) / total
            distance = (uv[1] - rows[:, 2] - rows[:, 3] * (uv[0] - rows[:, 1])) / np.hypot(1.0, rows[:, 3])
            if (high := (int(np.count_nonzero(distance >= 0.0)))) == 0 or high == len(rows):
                return Nothing
            low = high - 1
            mired, u, v, _ = rows[low] + (rows[high] - rows[low]) * (distance[low] / (distance[low] - distance[high]))
            values = {"temperature": float(1e6 / mired), "tint": float(np.copysign(3000.0 * np.hypot(uv[0] - u, uv[1] - v), u - uv[0]))}
            return catch(lambda: msgspec.convert(values, type=cls), exception=msgspec.ValidationError)().to_option()

        return processor(config, xyz(config, PyOpenColorIO.ROLE_SCENE_LINEAR)).map(solved)


# --- [OPERATIONS] -----------------------------------------------------------------------


def adaptation(source: npt.NDArray[np.float64], target: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
    """Returns the float64 Bradford matrix between whites normalized to Y = 1."""
    cone = np.asarray(CAT_BRADFORD, dtype=np.float64)
    return np.linalg.solve(cone, ((cone @ target) / target[1] / ((cone @ source) / source[1]))[:, np.newaxis] * cone)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["White", "adaptation"]
