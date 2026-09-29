"""Case record conversion to the `result` dict, the artifact folder, and the unknown-object case the scripts share."""

from collections.abc import Iterable
from pathlib import Path
from typing import TYPE_CHECKING

import attrs
from cattrs.preconf.json import make_converter

if TYPE_CHECKING:
    import bpy

# --- [ERRORS] ---------------------------------------------------------------------------


@attrs.frozen
class UnknownObjects:
    """Object names absent from the scene."""

    names: tuple[str, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def repository() -> Path:
    """Nearest ancestor of `results.py` holding `.git`."""
    return next(parent for parent in Path(__file__).resolve().parents if (parent / ".git").exists())


def artifacts(*parts: str) -> Path:
    """Folder under the repository's `.artifacts/blender/`, created with its parents."""
    folder = repository().joinpath(".artifacts", "blender", *parts)
    folder.mkdir(parents=True, exist_ok=True)
    return folder


def unknown(objects: "bpy.types.bpy_prop_collection[bpy.types.Object]", names: Iterable[str]) -> UnknownObjects | None:
    """Names absent from `objects`, `None` when `objects` holds every name."""
    missing = tuple(name for name in names if name not in objects)
    return UnknownObjects(missing) if missing else None


def as_result(value: object) -> dict[str, object]:
    """`result` dict of a case record, the class name under `kind` at every level."""
    return {"kind": type(value).__name__, **JSON.unstructure_attrs_asdict(value)}


# --- [COMPOSITION] ----------------------------------------------------------------------

JSON = make_converter()
JSON.register_unstructure_hook_func(attrs.has, as_result)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["JSON", "UnknownObjects", "artifacts", "as_result", "repository", "unknown"]
