"""Faults every script returns, `result` dict conversion of case records, and `.artifacts/blender/` folders."""

from collections.abc import Iterable
from functools import reduce
from pathlib import Path
import sys
from typing import Self, TYPE_CHECKING

import attrs
from cattrs.preconf.json import make_converter

if TYPE_CHECKING or "bpy" in sys.modules:
    import bpy
if TYPE_CHECKING:
    from rna import BPyOpFunction

# --- [TYPES] ----------------------------------------------------------------------------

type Resolved[T] = T | Faults
type Refuser = type | BPyOpFunction

# --- [ERRORS] ---------------------------------------------------------------------------


@attrs.frozen
class Fault:
    """Value that type or operator `source` refused, with the alternatives it accepts."""

    source: Refuser
    value: object
    accepted: tuple[object, ...] = ()


@attrs.frozen
class Faults:
    """Non-empty faults of one call, in the order its independent results raised them."""

    items: tuple[Fault, ...]

    def __add__(self, other: Self) -> Self:
        """Return both fault sets joined in order."""
        return type(self)((*self.items, *other.items))

    @classmethod
    def of(cls, *results: object) -> Self:
        """Return every `Fault` and `Faults` among `results` joined, at least one of them holding a fault."""
        return reduce(cls.__add__, (cls((result,)) if isinstance(result, Fault) else result for result in results if isinstance(result, (Fault, cls))))


# --- [OPERATIONS] -----------------------------------------------------------------------


def collect_faults(*results: object) -> Faults | None:
    """Return every fault among independent results, `None` when each succeeded."""
    return Faults.of(*results) if any(isinstance(result, Fault | Faults) for result in results) else None


def unknown(objects: "bpy.types.bpy_prop_collection[bpy.types.Object]", names: Iterable[str]) -> Faults | None:
    """Return a fault per name `objects` lacks, each accepting the names it holds, `None` when it holds every name."""
    return collect_faults(*(Fault(bpy.types.Object, name, tuple(objects.keys())) for name in names if name not in objects))


def repository() -> Path:
    """Return the nearest ancestor of `results.py` holding `.git`."""
    return next(parent for parent in Path(__file__).resolve().parents if (parent / ".git").exists())


def artifacts(*parts: str) -> Path:
    """Return the folder under the repository's `.artifacts/blender/`, created with its parents."""
    folder = repository().joinpath(".artifacts", "blender", *parts)
    folder.mkdir(parents=True, exist_ok=True)
    return folder


def as_result(value: object) -> dict[str, object]:
    """Return the `result` dict of a case record, its class name under `kind` at every level and a fault's `source` by name."""
    return {"kind": type(value).__name__, **JSON.unstructure_attrs_asdict(value)}


# --- [COMPOSITION] ----------------------------------------------------------------------

JSON = make_converter()
JSON.register_unstructure_hook(Refuser, lambda source: source.__name__ if isinstance(source, type) else source.idname_py())
JSON.register_unstructure_hook_func(attrs.has, as_result)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["JSON", "Fault", "Faults", "Refuser", "Resolved", "artifacts", "as_result", "collect_faults", "repository", "unknown"]
