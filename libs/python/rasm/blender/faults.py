"""Blender failures and the result shared by Blender operations."""

from typing import Protocol, TYPE_CHECKING
import zoneinfo

from expression import Result
import msgspec

from rasm.imaging.faults import Faults, ImagingFaults, Refusal
from rasm.rendering.faults import RenderingFaults

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Requirement(Protocol):
    """What a context lacks for an operation or interface."""

    @property
    def message(self) -> str:
        """Poll message naming the unmet requirement."""
        ...


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


class BlenderEditFault(Refusal[str, AttributeError | ReferenceError | RuntimeError | KeyError | TypeError | ValueError | SystemError | msgspec.EncodeError | OverflowError | MemoryError], frozen=True):
    """Host property or function that refused an edit."""


class BlenderChoiceFault(msgspec.Struct, frozen=True, kw_only=True):
    """Identifier outside the set a host path accepts."""

    path: str
    identifier: str
    accepted: tuple[str, ...]


class BlenderAbsentFault(msgspec.Struct, frozen=True, kw_only=True):
    """Key absent from an owner."""

    owner: str
    key: str | int


class BlenderCollisionFault(Refusal[str, str], frozen=True):
    """Requested name or placement occupied."""


class BlenderUnmetFault(msgspec.Struct, frozen=True, kw_only=True):
    """Context lacking a requirement."""

    requirement: Requirement


class BlenderOperatorFault(Refusal[str, RuntimeError | frozenset["rna_enums.OperatorReturnItems"]], frozen=True):
    """Operator that refused its call or returned no success."""


class BlenderReadFault(Refusal[str, msgspec.DecodeError | TypeError | ValueError | OSError | KeyError | NotImplementedError | MemoryError], frozen=True):
    """File or stored form its reader refused."""


class BlenderWriteFault(Refusal[str, OSError], frozen=True):
    """Folder or file its writer refused."""


class BlenderZoneFault(Refusal[str, ValueError | zoneinfo.ZoneInfoNotFoundError], frozen=True, kw_only=True):
    """Native zone constructor that refused an identifier."""


class BlenderImagingFault(msgspec.Struct, frozen=True, kw_only=True):
    """Complete fault tuple from an imaging operation."""

    faults: ImagingFaults


class BlenderRenderingFault(msgspec.Struct, frozen=True, kw_only=True):
    """Complete fault tuple from a rendering operation."""

    faults: RenderingFaults


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
