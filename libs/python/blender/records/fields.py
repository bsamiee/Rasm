"""Record field domains, presentation markers, union cases, and moves between records."""

from enum import auto, Enum, IntEnum
import functools
import math
import re
import sys
from typing import Annotated, ClassVar, Self

import colour
from expression import Nothing, Option, Some
import msgspec
import msgspec.inspect
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
    FOCAL_LENGTH = auto()
    ANGLE = auto()
    DURATION = auto()
    COLOR_TEMPERATURE = auto()
    IRRADIANCE = auto()
    LUMINANCE = auto()
    FRAME = auto()


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


type Seed = Annotated[int, msgspec.Meta(ge=0, le=2**24)]
type Finite = Annotated[float, msgspec.Meta(ge=-sys.float_info.max, le=sys.float_info.max)]
type Unit = Annotated[float, msgspec.Meta(ge=0.0, le=1.0)]
type Positive = Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max)]
type NonNegative = Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max)]
type AtLeastOne = Annotated[float, msgspec.Meta(ge=1.0, le=sys.float_info.max)]
type Side = Annotated[int, msgspec.Meta(ge=1)]
type Turn = Annotated[float, msgspec.Meta(ge=-math.pi, le=math.pi), Presentation(quantity=Some(Quantity.ANGLE))]
type Elevation = Annotated[float, msgspec.Meta(ge=-math.pi / 2.0, le=math.pi / 2.0), Presentation(quantity=Some(Quantity.ANGLE))]

# --- [MODELS] ---------------------------------------------------------------------------


class Presentation(msgspec.Struct, frozen=True, kw_only=True):
    """Display settings in parameter stored unit."""

    soft: Option[tuple[float, float]] = Nothing
    step: Option[float] = Nothing
    precision: Option[int] = Nothing
    quantity: Option[Quantity] = Nothing

    @classmethod
    def of(cls, annotation: object) -> Self:
        """Folds presentation markers inner to outer, outer settings overriding."""
        return functools.reduce(lambda inner, outer: cls(**{name: getattr(outer, name).or_else(getattr(inner, name)) for name in cls.__struct_fields__}), markers(annotation, cls), cls())


class Gated[T](msgspec.Struct, frozen=True, kw_only=True):
    """Value editable while row or section is disabled."""

    enabled: bool
    value: T


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


class Frozen(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Record whose array fields, held directly or in an option, are read-only from construction."""

    def __post_init__(self) -> None:
        """Marks every array field read-only."""
        for field in msgspec.structs.fields(self):
            match getattr(self, field.name):
                case np.ndarray() as array:
                    array.setflags(write=False)
                case Option(tag="some", some=np.ndarray() as array):
                    array.setflags(write=False)
                case _:
                    pass


class Case(msgspec.Struct, metaclass=CaseMeta, frozen=True, kw_only=True, tag_field="kind", tag=True):
    """Union case record tagged by its class name, with display title."""

    title: ClassVar[str]


# --- [OPERATIONS] -----------------------------------------------------------------------


def heading(name: str) -> str:
    """Returns title-cased words split at underscores and camel-case boundaries."""
    return re.sub(r"(?<=[a-z])(?=[A-Z])", " ", name).replace("_", " ").title()


def markers[M](annotation: object, marker: type[M]) -> tuple[M, ...]:
    """Reads `marker` instances from annotation metadata, inner to outer, through type aliases."""
    return tuple(value for value in inspect_annotation(annotation, annotation_source=AnnotationSource.ANY, unpack_type_aliases="eager").metadata if isinstance(value, marker))


def bounds(kind: msgspec.inspect.Type) -> tuple[float, float]:
    """Returns first and last valid values a numeric kind declares, whole for integers, tightest across tuple items and record fields, infinite where open."""
    match kind:
        case msgspec.inspect.Metadata(type=inner):
            return bounds(inner)
        case msgspec.inspect.TupleType(item_types=items):
            sides = tuple(map(bounds, items))
        case msgspec.inspect.StructType(fields=fields):
            sides = tuple(bounds(field.type) for field in fields)
        case msgspec.inspect.IntType(ge=ge, gt=gt, le=le, lt=lt):
            return (float(ge) if ge is not None else float(gt + 1) if gt is not None else -math.inf, float(le) if le is not None else float(lt - 1) if lt is not None else math.inf)
        case msgspec.inspect.FloatType(ge=ge, gt=gt, le=le, lt=lt):
            return (
                float(ge) if ge is not None else math.nextafter(gt, math.inf) if gt is not None else -math.inf,
                float(le) if le is not None else math.nextafter(lt, -math.inf) if lt is not None else math.inf,
            )
        case _:
            return -math.inf, math.inf
    return max((low for low, _ in sides), default=-math.inf), min((high for _, high in sides), default=math.inf)


def blended[S: msgspec.Struct](start: S, end: S, amount: float, basis: npt.NDArray[np.float64]) -> S:
    """Moves each declared field of `start` toward `end` by `amount`, 1 reaching `end`.

    Numbers move linearly within their declared bounds, integers rounded; `Rgb` values move in Oklab over `basis`; nested records and
    equal-length tuples move per member; every other value takes `end` above half.

    Args:
        start: Record at amount 0.
        end: Record at amount 1.
        amount: Share of the move, past 1 extrapolating.
        basis: Matrix taking `Rgb` values to CIE XYZ.

    Returns:
        Moved record.
    """
    switch = 0.5

    def moved(kind: msgspec.inspect.Type, rest: object, preset: object) -> object:
        match kind, rest, preset:
            case _ if rest == preset:
                return preset
            case msgspec.inspect.Metadata(type=inner), _, _:
                return moved(inner, rest, preset)
            case msgspec.inspect.StructType(fields=fields), Rgb(), Rgb():
                ends = colour.XYZ_to_Oklab(np.stack((np.asarray(rest), np.asarray(preset))) @ basis.T)
                values = np.linalg.solve(basis, colour.Oklab_to_XYZ(ends[0] + amount * (ends[1] - ends[0])))
                return msgspec.structs.replace(preset, **{field.name: float(np.clip(value, *bounds(field.type))) for field, value in zip(fields, values)})
            case msgspec.inspect.StructType(fields=fields), msgspec.Struct(), msgspec.Struct() if type(rest) is type(preset):
                return msgspec.structs.replace(preset, **{field.name: moved(field.type, getattr(rest, field.name), getattr(preset, field.name)) for field in fields})
            case msgspec.inspect.TupleType(item_types=items), tuple(), tuple() if len(rest) == len(preset):
                return tuple(map(moved, items, rest, preset))
            case msgspec.inspect.VarTupleType(item_type=item), tuple(), tuple() if len(rest) == len(preset):
                return tuple(moved(item, *pair) for pair in zip(rest, preset))
            case msgspec.inspect.FloatType(), float() | int(), float() | int():
                return float(np.clip(rest + amount * (preset - rest), *bounds(kind)))
            case msgspec.inspect.IntType(), int(), int():
                return int(np.clip(round(rest + amount * (preset - rest)), *bounds(kind)))
            case msgspec.inspect.UnionType() | msgspec.inspect.AnyType(), _, _ if type(rest) is type(preset):
                return moved(msgspec.inspect.type_info(type(preset)), rest, preset)
            case _:
                return preset if amount > switch else rest

    match msgspec.inspect.type_info(type(end)):
        case msgspec.inspect.StructType(fields=fields):
            return msgspec.structs.replace(end, **{field.name: moved(field.type, getattr(start, field.name), getattr(end, field.name)) for field in fields})
        case _:
            return end if amount > switch else start


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = [
    "AtLeastOne",
    "Case",
    "CaseMeta",
    "Choice",
    "Elevation",
    "Finite",
    "Frozen",
    "Gated",
    "Knot",
    "NonNegative",
    "Positive",
    "Presentation",
    "Quantity",
    "Rgb",
    "Seed",
    "Side",
    "Turn",
    "Unit",
    "blended",
    "bounds",
    "heading",
    "markers",
]
