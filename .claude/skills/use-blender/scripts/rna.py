# ty: ignore[not-iterable, unresolved-attribute, unresolved-import]
# mypy: disable-error-code="attr-defined, unreachable"
# ruff: file-ignore[import-private-name]
"""Stored RNA values of a struct as JSON and the function of every registered operator."""

from typing import Final, Protocol

import _bpy
import bpy

# --- [TYPES] ----------------------------------------------------------------------------


class BPyOpFunction(Protocol):
    """Operator function `bpy.ops.<module>.<name>` returns."""

    def __call__(self, *args: object, **kwargs: object) -> set[str]:
        """Return set of the operator, positional arguments the execution context and undo flag, keywords its properties."""
        ...

    def poll(self, context: str = ..., /) -> bool:
        """Whether the operator runs in the current context."""
        ...

    def idname(self) -> str:
        """Operator name as `<MODULE>_OT_<name>`."""
        ...

    def idname_py(self) -> str:
        """Operator name as `<module>.<name>`."""
        ...

    def get_rna_type(self) -> bpy.types.Struct:
        """RNA type of the operator's properties."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

DIGITS: Final = bpy.types.Object.bl_rna.properties["location"].precision

# --- [OPERATIONS] -----------------------------------------------------------------------


def stored(p: bpy.types.Property) -> bool:
    """Whether the property holds a value its struct stores, `is_deprecated` and class registration members, passwords, back pointers, active indexes, selection, and panel toggles left out."""
    match p:
        case (
            bpy.types.Property(is_deprecated=True)
            | bpy.types.Property(is_registered=True)
            | bpy.types.StringProperty(subtype="PASSWORD")
            | bpy.types.IntProperty(identifier="active_index")
            | bpy.types.BoolProperty(identifier="select")
        ):
            return False
        case bpy.types.BoolProperty(identifier=str() as name) if not {"expanded", "panel", "selector"}.isdisjoint(name.split("_")):
            return False
        case bpy.types.PointerProperty(fixed_type=bpy.types.ID()):
            return not p.is_readonly
        case bpy.types.PointerProperty(fixed_type=bpy.types.Node() | bpy.types.NodeTreeInterfaceItem() | bpy.types.Struct()):
            return False
        case bpy.types.PointerProperty():
            return p.is_readonly
        case bpy.types.CollectionProperty():
            return True
        case _:
            return not p.is_readonly


def plain(value: object) -> object:
    """JSON form of an RNA value, floats rounded to `Object.location` precision, IDs by name, and structs by stored properties."""
    match value:
        case float():
            return round(value, DIGITS)
        case str() | int() | None:
            return value
        case set():
            return sorted(value)
        case bpy.types.ID():
            return value.name
        case bpy.types.bpy_struct():
            return {p.identifier: plain(getattr(value, p.identifier)) for p in value.bl_rna.properties if stored(p)}
        case _:
            return [plain(v) for v in value]


def operators() -> tuple[BPyOpFunction, ...]:
    """Function of every registered operator, stock and add-on, as `_bpy.ops.create_function` builds it past any `bpy.ops` wrapper."""
    return tuple(_bpy.ops.create_function(prefix.lower(), name) for prefix, _, name in (idname.partition("_OT_") for idname in _bpy.ops.dir()))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DIGITS", "BPyOpFunction", "operators", "plain", "stored"]
