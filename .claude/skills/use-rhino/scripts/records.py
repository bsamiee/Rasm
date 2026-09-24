# /// script
# dependencies = ["msgspec"]
# ///
"""Records and faults every script returns inside and outside Rhino."""

import msgspec

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
    """Layer read back with its object count."""

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


# --- [OPERATIONS] -----------------------------------------------------------------------


def collect_faults(*results: object) -> tuple[Fault, ...]:
    """Return every fault among independent results, flattening nested tuples."""
    return tuple(fault for result in results for fault in (collect_faults(*result) if isinstance(result, tuple) else (result,)) if isinstance(fault, Fault))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Fault", "File", "LayerRecord", "MaterialRecord", "Properties", "Record", "collect_faults"]
