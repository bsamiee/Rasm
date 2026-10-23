"""ASC CDL grades and native reads of CC, CCC, and CDL files."""

from typing import Annotated, ClassVar, override

from expression import Error, Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.imaging.chain import Effect, Stage
from blender.imaging.faults import accumulated, colored, ImagingReadFault, ImagingResult
from blender.imaging.parameters import Finite, NonNegative, Positive

# --- [MODELS] ---------------------------------------------------------------------------


class Channel(msgspec.Struct, frozen=True, kw_only=True):
    """Slope, offset, and power of one channel."""

    slope: Annotated[NonNegative, msgspec.Meta(description="Channel multiplier before offset")] = 1.0
    offset: Annotated[Finite, msgspec.Meta(description="Value added after slope")] = 0.0
    power: Annotated[Positive, msgspec.Meta(description="Exponent on nonnegative channel values")] = 1.0


class Cdl(Effect, frozen=True, kw_only=True, tag=12):
    """ASC slope, offset, power, and saturation in config's color timing space."""

    stage = Stage.CDL
    title = "ASC CDL"
    pointwise = True
    space: ClassVar[str] = PyOpenColorIO.ROLE_COLOR_TIMING
    red: Annotated[Channel, msgspec.Meta(description="Red slope, offset, and power")] = Channel()
    green: Annotated[Channel, msgspec.Meta(description="Green slope, offset, and power")] = Channel()
    blue: Annotated[Channel, msgspec.Meta(description="Blue slope, offset, and power")] = Channel()
    saturation: Annotated[NonNegative, msgspec.Meta(description="Mix from Rec.709 luma to graded color")] = 1.0

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.CDLTransform]]:
        slope, offset, power = zip(*(msgspec.structs.astuple(channel) for channel in (self.red, self.green, self.blue)))
        return Ok(Some(PyOpenColorIO.CDLTransform(slope=slope, offset=offset, power=power, sat=self.saturation)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def read(path: str) -> ImagingResult[tuple[tuple[Option[str], Cdl], ...]]:
    """Reads every editable ASC correction in file order.

    Args:
        path: CC, CCC, or CDL file.

    Returns:
        `Ok` of correction IDs (`Nothing` when unnamed) and grades. `Error` of `ImagingColorFault` on OCIO refusal, or one `ImagingReadFault` per out-of-bounds correction.
    """

    def entry(correction: PyOpenColorIO.CDLTransform) -> ImagingResult[tuple[Option[str], Cdl]]:
        red, green, blue = ({"slope": slope, "offset": offset, "power": power} for slope, offset, power in zip(correction.getSlope(), correction.getOffset(), correction.getPower()))
        try:
            grade = msgspec.convert({"red": red, "green": green, "blue": blue, "saturation": correction.getSat()}, type=Cdl)
        except msgspec.ValidationError as error:
            return Error((ImagingReadFault(request=path, error=error),))
        return Ok((Some(correction.getFormatMetadata().getID()).filter(bool), grade))

    return colored(path, lambda: PyOpenColorIO.CDLTransform.CreateGroupFromFile(path)).bind(lambda group: accumulated(map(entry, group)))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Cdl", "Channel", "read"]
