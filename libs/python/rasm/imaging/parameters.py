"""Domain values, presentation, and annotation markers for imaging parameters."""

from enum import auto, Enum, IntEnum, StrEnum
import sys
from typing import Annotated, ClassVar, Final, Self

from expression import Nothing, Option, Some
import msgspec
from typing_inspection.introspection import AnnotationSource, inspect_annotation

# --- [TYPES] ----------------------------------------------------------------------------


class Quantity(Enum):
    """Stored SI quantity or a fraction of the displayed frame."""

    LENGTH = auto()
    CAMERA_LENGTH = auto()
    ANGLE = auto()
    DURATION = auto()
    COLOR_TEMPERATURE = auto()
    IRRADIANCE = auto()
    LUMINANCE = auto()
    FRAME = auto()
    FRAME_WIDTH = auto()
    FRAME_HEIGHT = auto()
    FRAME_GREATER = auto()


class Choice(IntEnum):
    """Menu choice with persistent integer identity and display text."""

    label: str

    def __new__(cls, value: int, label: str) -> Self:
        """Constructs a choice with its declared identity and label."""
        member = int.__new__(cls, value)
        member._value_, member.label = value, label
        return member


class Interpolation(StrEnum):
    """Interpolation between a color band's stops."""

    CONSTANT = auto()
    LINEAR = auto()
    EASE = auto()
    B_SPLINE = auto()
    CARDINAL = auto()


class ColorMode(StrEnum):
    """Color coordinates a color band interpolates."""

    RGB = auto()
    HSV = auto()
    HSL = auto()


class HuePath(StrEnum):
    """Direction of hue interpolation along a color band."""

    NEAR = auto()
    FAR = auto()
    CW = auto()
    CCW = auto()


class GraphForm(Enum):
    """Parameter whose value writes a graph instead of an interface socket."""

    IMAGE = auto()
    SETTING = auto()
    COLOR_SPACE = auto()


type Stops = Annotated[float, msgspec.Meta(ge=STOP_RANGE[0], le=STOP_RANGE[1]), Presentation(soft=Some((-10.0, 10.0)), step=Some(0.01), decimals=Some(3))]
type Seed = Annotated[int, msgspec.Meta(ge=0, le=2**24)]
type ColorSpace = Annotated[str, msgspec.Meta(min_length=1), GraphForm.COLOR_SPACE]
type Finite = Annotated[float, msgspec.Meta(ge=-sys.float_info.max, le=sys.float_info.max)]
type Position = Annotated[tuple[Finite, Finite], Presentation(soft=Some((0.0, 1.0)), quantity=Some(Quantity.FRAME))]

# --- [CONSTANTS] ------------------------------------------------------------------------

STOP_RANGE: Final = (-32.0, 32.0)

# --- [MODELS] ---------------------------------------------------------------------------


class Presentation(msgspec.Struct, frozen=True, kw_only=True):
    """Optional display settings in the parameter's stored unit."""

    soft: Option[tuple[float, float]] = Nothing
    step: Option[float] = Nothing
    decimals: Option[int] = Nothing
    quantity: Option[Quantity] = Nothing


class Gated[T](msgspec.Struct, frozen=True, kw_only=True):
    """Editable value retained while its row or section is disabled."""

    enabled: bool
    value: T


class Motion(msgspec.Struct, frozen=True, kw_only=True):
    """Clock advancement and scene-frame hold for animated parameters."""

    speed: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Clock frames per scene frame, 0 holding a still"), Presentation(soft=Some((0.0, 4.0)))] = 1.0
    hold: Annotated[int, msgspec.Meta(ge=1, description="Scene frames each clock value holds")] = 1


class Rgb[T: float = float](msgspec.Struct, frozen=True, kw_only=True, array_like=True):
    """RGB components preserving their declared channel constraints."""

    red: T
    green: T
    blue: T


class Knot[X: float, Y: float](msgspec.Struct, frozen=True, kw_only=True, array_like=True):
    """Curve point whose axes retain their native constraints."""

    x: X
    y: Y


class Stop(msgspec.Struct, frozen=True, kw_only=True):
    """Color at a normalized position along a color band."""

    position: Annotated[float, msgspec.Meta(ge=0.0, le=1.0)]
    color: Rgb


class Ramp(msgspec.Struct, frozen=True, kw_only=True):
    """Color band retaining interpolation and hue settings across modes."""

    stops: Annotated[tuple[Stop, ...], msgspec.Meta(min_length=1, max_length=32)] = (
        Stop(position=0.0, color=Rgb(red=0.0, green=0.0, blue=0.0)),
        Stop(position=1.0, color=Rgb(red=1.0, green=1.0, blue=1.0)),
    )
    interpolation: Interpolation = Interpolation.LINEAR
    mode: ColorMode = ColorMode.RGB
    hue_path: HuePath = HuePath.NEAR


class Case(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind"):
    """Record alternative with a concrete persistent tag and display title."""

    title: ClassVar[str]


# --- [OPERATIONS] -----------------------------------------------------------------------


def markers[M](annotation: object, marker: type[M]) -> tuple[M, ...]:
    """Reads requested markers from the inner annotation outward through aliases."""
    return tuple(value for value in inspect_annotation(annotation, annotation_source=AnnotationSource.ANY, unpack_type_aliases="eager").metadata if isinstance(value, marker))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = [
    "STOP_RANGE",
    "Case",
    "Choice",
    "ColorMode",
    "ColorSpace",
    "Finite",
    "Gated",
    "GraphForm",
    "HuePath",
    "Interpolation",
    "Knot",
    "Motion",
    "Position",
    "Presentation",
    "Quantity",
    "Ramp",
    "Rgb",
    "Seed",
    "Stop",
    "Stops",
    "markers",
]
