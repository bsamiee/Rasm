"""Blender faults and result type of Blender operations."""

from collections.abc import Callable
from pathlib import Path
import typing
from typing import override, Protocol, TYPE_CHECKING
import zoneinfo

from expression import Error, Ok, Result
import msgspec

from blender.imaging.faults import Fault, Faults, Refusal
from blender.rendering.faults import RenderingFault

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Requirement(Protocol):
    """Context requirement of operation or interface."""

    @property
    def message(self) -> str:
        """Poll message of unmet requirement."""
        ...


type EditRefusal = AttributeError | ReferenceError | RuntimeError | KeyError | TypeError | ValueError | SystemError | msgspec.EncodeError | OverflowError | MemoryError
type BlenderFault = (
    BlenderRegisterFault | BlenderEditFault | BlenderChoiceFault | BlenderAbsentFault | BlenderCollisionFault | BlenderUnmetFault | BlenderOperatorFault | BlenderZoneFault | RenderingFault
)
type BlenderFaults = Faults[BlenderFault]
type BlenderResult[T] = Result[T, BlenderFaults]

# --- [ERRORS] ---------------------------------------------------------------------------


class BlenderRegisterFault(Refusal[tuple[str, ...], ValueError | RuntimeError | TypeError | msgspec.ValidationError], frozen=True):
    """Class ordering, registration, or generated name refused."""

    @property
    @override
    def message(self) -> str:
        """Report text listing the refused names."""
        return f"Registration of {', '.join(self.request)} refused. {self.error}"


class BlenderEditFault(Refusal[str, EditRefusal], frozen=True):
    """Host property or function refusing edit."""

    template = "{request} refused edit. {error}"

    @classmethod
    def attempted[T](cls, request: str, call: Callable[[], T]) -> "BlenderResult[T]":
        """Returns host call result, edit fault naming request on refusal."""
        try:
            return Ok(call())
        except typing.get_args(EditRefusal.__value__) as error:
            return Error((cls(request=request, error=error),))


class BlenderChoiceFault(Fault, frozen=True, kw_only=True):
    """Identifier outside the set a host path accepts."""

    path: str
    identifier: str
    accepted: tuple[str, ...]

    @property
    @override
    def message(self) -> str:
        """Report text listing the accepted identifiers."""
        return f"{self.path} accepts {', '.join(self.accepted)}, not {self.identifier}"


class BlenderAbsentFault(Fault, frozen=True, kw_only=True):
    """Key absent from owner."""

    owner: str
    key: str | int

    @property
    @override
    def message(self) -> str:
        """Report text naming owner and key."""
        return f"{self.owner} holds no {self.key}"


class BlenderCollisionFault(Refusal[str, str | Path], frozen=True):
    """Requested name or placement occupied."""

    template = "{request} is taken at {error}"


class BlenderUnmetFault(Fault, frozen=True, kw_only=True):
    """Context lacking requirement."""

    requirement: Requirement

    @property
    @override
    def message(self) -> str:
        """Poll message of the requirement."""
        return self.requirement.message


class BlenderOperatorFault(Refusal[str, RuntimeError | frozenset["rna_enums.OperatorReturnItems"]], frozen=True):
    """Operator that refused its call or returned no success."""

    @property
    @override
    def message(self) -> str:
        """Report text naming the answer set or the raised error."""
        match self.error:
            case frozenset():
                return f"{self.request} answered {', '.join(sorted(self.error))}"
            case _:
                return f"{self.request} failed. {self.error}"


class BlenderZoneFault(Refusal[str, ValueError | zoneinfo.ZoneInfoNotFoundError], frozen=True, kw_only=True):
    """Time zone `zoneinfo.ZoneInfo` refused."""

    template = "Time zone {request} is unavailable. {error}"


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "BlenderAbsentFault",
    "BlenderChoiceFault",
    "BlenderCollisionFault",
    "BlenderEditFault",
    "BlenderFault",
    "BlenderFaults",
    "BlenderOperatorFault",
    "BlenderRegisterFault",
    "BlenderResult",
    "BlenderUnmetFault",
    "BlenderZoneFault",
    "Requirement",
]
