"""Imaging parameter values, presentation, and annotation markers."""

from enum import auto, Enum, IntEnum, StrEnum
import functools
import math
import re
import sys
from typing import Annotated, ClassVar, Final, Self

from expression import Nothing, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
from typing_inspection.introspection import AnnotationSource, inspect_annotation

# --- [TYPES] ----------------------------------------------------------------------------


class CaseMeta(msgspec.StructMeta):
    """Record metaclass deriving case subclass titles from class names."""

    def __init__(self, name: str, bases: tuple[type, ...], namespace: dict[str, object], /, **options: object) -> None:
        """Builds record class, titling subclasses that declare no title."""
        super().__init__(name, bases, namespace, **options)
        if "title" not in namespace and any(isinstance(base, CaseMeta) for base in bases):
            self.title = heading(name)


class Quantity(Enum):
    """Stored SI quantity or share of displayed frame."""

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
    """Menu choice with persistent integer identity, display text, and hover text."""

    label: str
    description: Option[str]

    def __new__(cls, value: int, *_: object) -> Self:
        """Constructs choice from declared integer, passing other values to `__init__`."""
        member = int.__new__(cls, value)
        member._value_ = value
        return member

    def __init__(self, _: int, label: str | None = None, description: str | None = None) -> None:
        """Sets declared label, or member name heading, and hover text."""
        self.label = Option.of_optional(label).default_with(lambda: heading(self.name))
        self.description = Option.of_optional(description)


class Interpolation(StrEnum):
    """Interpolation between color band stops."""

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
    """Hue interpolation direction."""

    NEAR = auto()
    FAR = auto()
    CW = auto()
    CCW = auto()


class GraphForm(Enum):
    """Parameter kind writing value into graph in place of socket."""

    IMAGE = auto()
    SETTING = auto()
    COLOR_SPACE = auto()


type Stops = Annotated[float, msgspec.Meta(ge=STOP_RANGE[0], le=STOP_RANGE[1]), Presentation(soft=Some((-10.0, 10.0)), step=Some(0.01), decimals=Some(3))]
type Seed = Annotated[int, msgspec.Meta(ge=0, le=2**24)]
type ColorSpace = Annotated[str, msgspec.Meta(min_length=1), GraphForm.COLOR_SPACE]
type Finite = Annotated[float, msgspec.Meta(ge=-sys.float_info.max, le=sys.float_info.max)]
type Unit = Annotated[float, msgspec.Meta(ge=0.0, le=1.0)]
type Positive = Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max)]
type NonNegative = Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max)]
type AtLeastOne = Annotated[float, msgspec.Meta(ge=1.0, le=sys.float_info.max)]
type FrameLength = Annotated[NonNegative, Presentation(soft=Some((0.0, 1.0)), quantity=Some(Quantity.FRAME_GREATER))]
type FrameExtent = Annotated[Positive, Presentation(soft=Some((0.0, 1.0)), quantity=Some(Quantity.FRAME_GREATER))]
type Turn = Annotated[float, msgspec.Meta(ge=-math.pi, le=math.pi), Presentation(quantity=Some(Quantity.ANGLE))]
type Elevation = Annotated[float, msgspec.Meta(ge=-math.pi / 2.0, le=math.pi / 2.0), Presentation(quantity=Some(Quantity.ANGLE))]
type Position = Annotated[tuple[Finite, Finite], Presentation(soft=Some((0.0, 1.0)), quantity=Some(Quantity.FRAME))]

# --- [CONSTANTS] ------------------------------------------------------------------------

STOP_RANGE: Final = (-32.0, 32.0)

# --- [MODELS] ---------------------------------------------------------------------------


class Presentation(msgspec.Struct, frozen=True, kw_only=True):
    """Display settings in parameter stored unit."""

    soft: Option[tuple[float, float]] = Nothing
    step: Option[float] = Nothing
    decimals: Option[int] = Nothing
    quantity: Option[Quantity] = Nothing

    @classmethod
    def of(cls, annotation: object) -> Self:
        """Folds presentation markers inner to outer, outer settings overriding."""
        return functools.reduce(lambda inner, outer: cls(**{name: getattr(outer, name).or_else(getattr(inner, name)) for name in cls.__struct_fields__}), markers(annotation, cls), cls())


class Gated[T](msgspec.Struct, frozen=True, kw_only=True):
    """Value editable while row or section is disabled."""

    enabled: bool
    value: T


class Motion(msgspec.Struct, frozen=True, kw_only=True):
    """Clock speed and frame hold of animated parameters."""

    speed: Annotated[NonNegative, msgspec.Meta(description="Clock frames per scene frame, still at 0"), Presentation(soft=Some((0.0, 4.0)))] = 1.0
    hold: Annotated[int, msgspec.Meta(ge=1, description="Scene frames each clock value holds")] = 1


class Rgb[T: float = float](msgspec.Struct, frozen=True, kw_only=True, array_like=True):
    """RGB components with declared channel constraints."""

    red: T
    green: T
    blue: T

    def __array__(self, dtype: npt.DTypeLike | None = None, *, copy: bool | None = None) -> npt.NDArray[np.float64]:
        """Returns components as three-element numpy array."""
        return np.array(msgspec.structs.astuple(self), dtype=dtype, copy=copy)


class Knot[X: float, Y: float](msgspec.Struct, frozen=True, kw_only=True, array_like=True):
    """Curve point with per-axis constraints."""

    x: X
    y: Y


class Stop(msgspec.Struct, frozen=True, kw_only=True):
    """Color at normalized color band position."""

    position: Unit
    color: Rgb


class Ramp(msgspec.Struct, frozen=True, kw_only=True):
    """Color band keeping hue and interpolation across modes."""

    stops: Annotated[tuple[Stop, ...], msgspec.Meta(min_length=1, max_length=32)] = (
        Stop(position=0.0, color=Rgb(red=0.0, green=0.0, blue=0.0)),
        Stop(position=1.0, color=Rgb(red=1.0, green=1.0, blue=1.0)),
    )
    interpolation: Interpolation = Interpolation.LINEAR
    mode: ColorMode = ColorMode.RGB
    hue_path: HuePath = HuePath.NEAR


class Case(msgspec.Struct, metaclass=CaseMeta, frozen=True, kw_only=True, tag_field="kind"):
    """Union case record with persistent tag and display title."""

    title: ClassVar[str]


# --- [OPERATIONS] -----------------------------------------------------------------------


def heading(name: str) -> str:
    """Returns title-cased words split at underscores and camel-case boundaries."""
    return re.sub(r"(?<=[a-z])(?=[A-Z])", " ", name).replace("_", " ").title()


def markers[M](annotation: object, marker: type[M]) -> tuple[M, ...]:
    """Reads `marker` instances from annotation metadata, inner to outer, through type aliases."""
    return tuple(value for value in inspect_annotation(annotation, annotation_source=AnnotationSource.ANY, unpack_type_aliases="eager").metadata if isinstance(value, marker))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = [
    "STOP_RANGE",
    "AtLeastOne",
    "Case",
    "CaseMeta",
    "Choice",
    "ColorMode",
    "ColorSpace",
    "Elevation",
    "Finite",
    "FrameExtent",
    "FrameLength",
    "Gated",
    "GraphForm",
    "HuePath",
    "Interpolation",
    "Knot",
    "Motion",
    "NonNegative",
    "Position",
    "Positive",
    "Presentation",
    "Quantity",
    "Ramp",
    "Rgb",
    "Seed",
    "Stop",
    "Stops",
    "Turn",
    "Unit",
    "heading",
    "markers",
]
