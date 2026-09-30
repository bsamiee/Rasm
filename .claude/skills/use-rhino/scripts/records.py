# /// script
# dependencies = ["msgspec"]
# ///
"""Records and faults every script returns inside and outside Rhino."""

from functools import reduce
from typing import Self

import msgspec

# --- [TYPES] ----------------------------------------------------------------------------

type Resolved[T] = T | Faults

# --- [MODELS] ---------------------------------------------------------------------------


class Record(msgspec.Struct, frozen=True, omit_defaults=True, repr_omit_defaults=True):
    """Immutable value that prints and encodes without its default fields."""


class Properties(Record, frozen=True):
    """Layer attributes or an object's overrides of its layer, `None` leaving an attribute unchanged."""

    color: str | None = None
    linetype: str | None = None
    print_color: str | None = None
    print_width: float | None = None
    material: str | None = None
    visible: bool | None = None
    locked: bool | None = None
    strings: dict[str, str] | None = None


class LayerRecord(Record, frozen=True):
    """Layer with its object count."""

    path: str
    properties: Properties
    objects: int


class MaterialRecord(Record, frozen=True):
    """Physically based render material a layer or object names."""

    name: str
    color: str
    roughness: float
    metallic: float
    opacity: float
    ior: float


class File[T](Record, frozen=True):
    """File an operation wrote, with the operation's own `detail`."""

    path: str
    bytes: int
    detail: T


# --- [ERRORS] ---------------------------------------------------------------------------


class Fault(Record, frozen=True):
    """Value that type `source` refused, with the alternatives it accepts."""

    source: type
    value: object
    accepted: tuple[object, ...] = ()


class Faults(Record, frozen=True):
    """Non-empty faults of one call, in the order its independent results raised them."""

    items: tuple[Fault, ...]

    def __add__(self, other: Self) -> Self:
        """Join two fault sets in order."""
        return type(self)((*self.items, *other.items))

    @classmethod
    def of(cls, *results: object) -> Self:
        """Join every `Fault` and `Faults` among `results`, at least one of them holding a fault."""
        return reduce(cls.__add__, (cls((result,)) if isinstance(result, Fault) else result for result in results if isinstance(result, (Fault, cls))))


# --- [OPERATIONS] -----------------------------------------------------------------------


def collect_faults(*results: object) -> Faults | None:
    """Return every fault among independent results, `None` when each succeeded."""
    return Faults.of(*results) if any(isinstance(result, (Fault, Faults)) for result in results) else None


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Fault", "Faults", "File", "LayerRecord", "MaterialRecord", "Properties", "Record", "Resolved", "collect_faults"]
