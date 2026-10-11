"""ASC CDL grades and native reads of CC, CCC, and CDL files."""

import sys
from typing import Annotated, ClassVar, override

from expression import Error, Ok, Option, Some
import msgspec
import PyOpenColorIO

from rasm.imaging.chain import Effect, Stage
from rasm.imaging.faults import accumulated, ImagingLutFault, ImagingReadFault, ImagingResult

# --- [MODELS] ---------------------------------------------------------------------------


class Channel(msgspec.Struct, frozen=True, kw_only=True):
    """Slope, offset, and power of one channel."""

    slope: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Slope", description="Channel multiplier before offset")] = 1.0
    offset: Annotated[float, msgspec.Meta(ge=-sys.float_info.max, le=sys.float_info.max, title="Offset", description="Value added after slope")] = 0.0
    power: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Power", description="Exponent on nonnegative channel values")] = 1.0


class Cdl(Effect, frozen=True, kw_only=True, tag=12):
    """ASC slope, offset, power, and saturation in config's color timing space."""

    stage = Stage.CDL
    title = "ASC CDL"
    pointwise = True
    space: ClassVar[str] = PyOpenColorIO.ROLE_COLOR_TIMING
    red: Annotated[Channel, msgspec.Meta(title="Red", description="Red channel's slope, offset, and power")] = Channel()
    green: Annotated[Channel, msgspec.Meta(title="Green", description="Green channel's slope, offset, and power")] = Channel()
    blue: Annotated[Channel, msgspec.Meta(title="Blue", description="Blue channel's slope, offset, and power")] = Channel()
    saturation: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Saturation", description="Mix from Rec.709 luma to graded color")] = 1.0

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.CDLTransform]]:
        slope, offset, power = zip(*(msgspec.structs.astuple(channel) for channel in (self.red, self.green, self.blue)), strict=False)
        return Ok(Some(PyOpenColorIO.CDLTransform(slope=slope, offset=offset, power=power, sat=self.saturation)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def read(path: str) -> ImagingResult[tuple[tuple[Option[str], Cdl], ...]]:
    """Reads every editable ASC correction in file order.

    Args:
        path: CC, CCC, or CDL file selected for ASC import.

    Returns:
        `Ok` of correction ids and grades, with `Nothing` for unnamed corrections,
        else `Error` of an `ImagingLutFault` for OCIO's refusal or every
        `ImagingReadFault` for corrections outside declared parameter bounds.
    """

    def entry(correction: PyOpenColorIO.CDLTransform) -> ImagingResult[tuple[Option[str], Cdl]]:
        red, green, blue = ({"slope": slope, "offset": offset, "power": power} for slope, offset, power in zip(correction.getSlope(), correction.getOffset(), correction.getPower(), strict=False))
        try:
            grade = msgspec.convert({"red": red, "green": green, "blue": blue, "saturation": correction.getSat()}, type=Cdl)
        except msgspec.ValidationError as error:
            return Error((ImagingReadFault(request=path, error=error),))
        return Ok((Some(correction.getFormatMetadata().getID()).filter(bool), grade))

    try:
        group = PyOpenColorIO.CDLTransform.CreateGroupFromFile(path)
    except (PyOpenColorIO.Exception, PyOpenColorIO.ExceptionMissingFile) as error:
        return Error((ImagingLutFault(request=path, error=error),))
    return accumulated(map(entry, group))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Cdl", "Channel", "read"]
