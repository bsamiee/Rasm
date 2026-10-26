"""Operator class plan each invoke form folds its members, flags, bases, lifetime, execute, and opening into."""

from collections.abc import Callable, Mapping
from typing import TYPE_CHECKING

import bpy
import msgspec

from blender.bpy.extension.registration import Acquisition
from blender.bpy.faults import BlenderResult

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Returns = set[rna_enums.OperatorReturnItems]
type Executed = Callable[[bpy.types.Operator, bpy.types.Context], Returns]
type Opening[T] = Callable[[bpy.types.Operator, bpy.types.Context, bpy.types.WindowManager, bpy.types.Event, T], BlenderResult[Returns]]

# --- [MODELS] ---------------------------------------------------------------------------


class Plan[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Operator class under construction: flags, property definitions, members, bases, release scopes, execute, and invoke opening."""

    flags: frozenset["rna_enums.OperatorTypeFlagItems"]
    annotations: Mapping[str, object]
    members: Mapping[str, object]
    execute: Executed
    opening: Opening[T]
    bases: tuple[type, ...] = (bpy.types.Operator,)
    lifetime: tuple[Acquisition, ...] = ()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Executed", "Opening", "Plan", "Returns"]
