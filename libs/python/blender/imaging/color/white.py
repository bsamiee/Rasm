"""Robertson temperature and Duv, scene-linear white picks, and Bradford adaptation."""

from typing import Annotated, ClassVar, Self

import colour
from expression import Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.imaging.color.processors import basis
from blender.imaging.faults import ImagingResult
from blender.records.fields import Presentation, Quantity, Rgb

# --- [MODELS] ---------------------------------------------------------------------------


class White(msgspec.Struct, frozen=True, kw_only=True):
    """Color temperature and signed Duv from the Planckian locus."""

    hottest: ClassVar[float] = 100000.0
    temperature: Annotated[
        float,
        msgspec.Meta(ge=1800.0, le=hottest, description="Correlated color temperature"),
        Presentation(soft=Some((2000.0, 11000.0)), step=Some(1.0), precision=Some(0), quantity=Some(Quantity.COLOR_TEMPERATURE)),
    ] = 6500.0
    duv: Annotated[float, msgspec.Meta(ge=-0.05, le=0.05, title="Duv", description="Distance from the Planckian locus in CIE 1960 uv, positive toward green"), Presentation(precision=Some(5))] = 0.0

    @property
    def xyz(self) -> npt.NDArray[np.float64]:
        """Float64 XYZ at Y = 1."""
        return np.asarray(colour.xy_to_XYZ(colour.UCS_uv_to_xy(colour.temperature.CCT_to_uv_Robertson1968(np.array((self.temperature, self.duv))))), dtype=np.float64)

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, color: Rgb) -> ImagingResult[Option[Self]]:
        """Solves white of scene-linear neutral pick, absent off the uv plane, at or past the hottest isotherm Robertson inverts, or outside field bounds."""

        def solved(matrix: npt.NDArray[np.float64]) -> Option[Self]:
            return (
                Some(colour.XYZ_to_UCS(matrix @ np.asarray(color)))
                .filter(lambda ucs: float(ucs.sum()) > 0.0)
                .bind(
                    lambda ucs: catch(
                        lambda: msgspec.convert(dict(zip(cls.__struct_fields__, colour.temperature.uv_to_CCT_Robertson1968(colour.UCS_to_uv(ucs)).tolist())), type=cls),
                        exception=msgspec.ValidationError,
                    )().to_option()
                )
                .filter(lambda white: white.temperature < cls.hottest)
            )

        return basis(config, PyOpenColorIO.ROLE_SCENE_LINEAR).map(solved)


# --- [OPERATIONS] -----------------------------------------------------------------------


def adaptation(source: npt.NDArray[np.float64], target: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
    """Returns float64 Bradford matrix between whites normalized to Y = 1."""
    return np.asarray(colour.adaptation.matrix_chromatic_adaptation_VonKries(source / source[1], target / target[1], transform="Bradford"), dtype=np.float64)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["White", "adaptation"]
