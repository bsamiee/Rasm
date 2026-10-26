"""Record field domains, curve points, read-only array records, and union cases."""

import sys
from typing import Annotated

from expression import Option
import msgspec
import numpy as np

# --- [TYPES] ----------------------------------------------------------------------------

type Seed = Annotated[int, msgspec.Meta(ge=0, le=2**24)]
type Finite = Annotated[float, msgspec.Meta(ge=-sys.float_info.max, le=sys.float_info.max)]
type Unit = Annotated[float, msgspec.Meta(ge=0.0, le=1.0)]
type Positive = Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max)]
type NonNegative = Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max)]
type AtLeastOne = Annotated[float, msgspec.Meta(ge=1.0, le=sys.float_info.max)]
type Side = Annotated[int, msgspec.Meta(ge=1)]

# --- [MODELS] ---------------------------------------------------------------------------


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


class Case(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=True):
    """Union case record tagged by its class name."""


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["AtLeastOne", "Case", "Finite", "Frozen", "Knot", "NonNegative", "Positive", "Seed", "Side", "Unit"]
