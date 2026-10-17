"""Blender failures and the result shared by Blender operations."""

from collections.abc import Callable
from importlib.resources.abc import Traversable
from pathlib import Path
import typing
from typing import Protocol, TYPE_CHECKING
import zoneinfo

from expression import Error, Ok, Result
import msgspec

from blender.imaging.faults import Faults, ImagingFaults, Refusal
from blender.rendering.faults import RenderingFaults

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Requirement(Protocol):
    """What a context lacks for an operation or interface."""

    @property
    def message(self) -> str:
        """Poll message naming the unmet requirement."""
        ...


type EditRefusal = AttributeError | ReferenceError | RuntimeError | KeyError | TypeError | ValueError | SystemError | msgspec.EncodeError | OverflowError | MemoryError
type BlenderFault = (
    BlenderRegisterFault
    | BlenderEditFault
    | BlenderChoiceFault
    | BlenderAbsentFault
    | BlenderCollisionFault
    | BlenderUnmetFault
    | BlenderOperatorFault
    | BlenderReadFault
    | BlenderWriteFault
    | BlenderZoneFault
    | BlenderImagingFault
    | BlenderRenderingFault
)
type BlenderFaults = Faults[BlenderFault]
type BlenderResult[T] = Result[T, BlenderFaults]

# --- [ERRORS] ---------------------------------------------------------------------------


class BlenderRegisterFault(Refusal[tuple[str, ...], ValueError | RuntimeError | TypeError | msgspec.ValidationError], frozen=True):
    """Class ordering, registration, or generated name refused."""


class BlenderEditFault(Refusal[str, EditRefusal], frozen=True):
    """Host property or function that refused an edit."""

    @classmethod
    def attempted[T](cls, request: str, call: Callable[[], T]) -> "BlenderResult[T]":
        """Answers a host call, or the edit fault naming its request when the host refuses it."""
        try:
            return Ok(call())
        except typing.get_args(EditRefusal.__value__) as error:
            return Error((cls(request=request, error=error),))


class BlenderChoiceFault(msgspec.Struct, frozen=True, kw_only=True):
    """Identifier outside the set a host path accepts."""

    path: str
    identifier: str
    accepted: tuple[str, ...]


class BlenderAbsentFault(msgspec.Struct, frozen=True, kw_only=True):
    """Key absent from an owner."""

    owner: str
    key: str | int


class BlenderCollisionFault(Refusal[str, str | Path], frozen=True):
    """Requested name or placement occupied."""


class BlenderUnmetFault(msgspec.Struct, frozen=True, kw_only=True):
    """Context lacking a requirement."""

    requirement: Requirement


class BlenderOperatorFault(Refusal[str, RuntimeError | frozenset["rna_enums.OperatorReturnItems"]], frozen=True):
    """Operator that refused its call or returned no success."""


class BlenderReadFault(Refusal[str | Traversable, msgspec.DecodeError | TypeError | ValueError | OSError | KeyError | NotImplementedError | MemoryError], frozen=True):
    """File or stored form its reader refused."""


class BlenderWriteFault(Refusal[str | Path, OSError], frozen=True):
    """Folder or file its writer refused."""


class BlenderZoneFault(Refusal[str, ValueError | zoneinfo.ZoneInfoNotFoundError], frozen=True, kw_only=True):
    """Native zone constructor that refused an identifier."""


class BlenderImagingFault(msgspec.Struct, frozen=True, kw_only=True):
    """Complete fault tuple from an imaging operation."""

    faults: ImagingFaults

    @classmethod
    def lifted(cls, faults: ImagingFaults) -> BlenderFaults:
        """Carries the imaging faults as one Blender fault."""
        return (cls(faults=faults),)


class BlenderRenderingFault(msgspec.Struct, frozen=True, kw_only=True):
    """Complete fault tuple from a rendering operation."""

    faults: RenderingFaults

    @classmethod
    def lifted(cls, faults: RenderingFaults) -> BlenderFaults:
        """Carries the rendering faults as one Blender fault."""
        return (cls(faults=faults),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "BlenderAbsentFault",
    "BlenderChoiceFault",
    "BlenderCollisionFault",
    "BlenderEditFault",
    "BlenderFault",
    "BlenderFaults",
    "BlenderImagingFault",
    "BlenderOperatorFault",
    "BlenderReadFault",
    "BlenderRegisterFault",
    "BlenderRenderingFault",
    "BlenderResult",
    "BlenderUnmetFault",
    "BlenderWriteFault",
    "BlenderZoneFault",
    "Requirement",
]
