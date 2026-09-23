# /// script
# dependencies = ["msgspec"]
# ///
"""Records and faults every script returns, inside Rhino's Python and outside it."""

import msgspec

# --- [MODELS] ---------------------------------------------------------------------------


class Record(msgspec.Struct, frozen=True, omit_defaults=True, repr_omit_defaults=True):
    """Immutable value whose printed and encoded forms leave out fields at their default."""


class Properties(Record, frozen=True):
    """Attributes of a layer, or an object's overrides of its layer, where `None` leaves one unchanged."""

    color: str | None = None
    linetype: str | None = None
    print_color: str | None = None
    print_width: float | None = None
    material: str | None = None
    visible: bool | None = None
    locked: bool | None = None
    strings: dict[str, str] | None = None


class LayerRecord(Record, frozen=True):
    """One layer's full path, properties, and object count."""

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


class File[T](Record, frozen=True):
    """File an operation wrote with its size in bytes and what it holds beyond the document's objects."""

    path: str
    bytes: int
    detail: T


# --- [ERRORS] ---------------------------------------------------------------------------


class Fault(Record, frozen=True):
    """Value the type `source` refused, with the values it accepts in its place."""

    source: type
    value: object
    accepted: tuple[object, ...] = ()


# --- [OPERATIONS] -----------------------------------------------------------------------


def collect_faults(*results: object) -> tuple[Fault, ...]:
    """Return every fault among independent results, each a value or a tuple of results."""
    return tuple(fault for result in results for fault in (collect_faults(*result) if isinstance(result, tuple) else (result,)) if isinstance(fault, Fault))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Fault", "File", "LayerRecord", "MaterialRecord", "Properties", "Record", "collect_faults"]
