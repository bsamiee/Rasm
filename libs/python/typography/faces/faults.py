"""Faults, their reasons, and result accumulation for every typography package."""

from collections.abc import Iterable
from enum import StrEnum
from itertools import chain
from typing import Never

from expression import Result
import msgspec

# --- [TYPES] ----------------------------------------------------------------------------

type Faults = tuple[Fault, *tuple[Fault, ...]]
type Outcome[T] = Result[T, Faults]

Ok = Result[Never, Faults].Ok
Error = Result[Never, Never].Error


class Reason(StrEnum):
    """Fault kinds, each value its message template."""

    OUTSIDE = "{subject} accepts {member}, not {cause}"
    INVALID = "{subject} is invalid: {cause}"
    ABSENT = "{subject} holds no {member}"
    UNREADABLE = "{subject} is unreadable: {cause}"
    UNWRITABLE = "{subject} is unwritable: {cause}"
    FAILED = "{subject} failed: {cause}"
    UNMET = "{subject} misses {member}: {cause}"


# --- [MODELS] ---------------------------------------------------------------------------


class Fault(msgspec.Struct, frozen=True, kw_only=True):
    """Refused or failed operation with a message formatted from its reason."""

    reason: Reason
    subject: str
    member: str = ""
    cause: object = ""

    @property
    def message(self) -> str:
        """Message the reason template formats."""
        return self.reason.format(subject=self.subject, member=self.member, cause=self.cause)

    @classmethod
    def of(cls, reason: Reason, subject: str, cause: object = "", /, *, member: str = "") -> Faults:
        """Returns a one-fault tuple, the refused value or caught error as cause."""
        return (cls(reason=reason, subject=subject, member=member, cause=cause),)


# --- [OPERATIONS] -----------------------------------------------------------------------


def accumulated[T](results: Iterable[Outcome[T]]) -> Outcome[tuple[T, ...]]:
    """Combines results, keeping every fault in input order."""
    held = tuple(results)
    match tuple(*result.swap().default_value(()) for result in held):
        case (head, *rest):
            return Error((head, *rest))
        case _:
            return Ok(tuple(*result.to_option().to_list() for result in held))


def paired[A, B](a: Outcome[A], b: Outcome[B], /) -> Outcome[tuple[A, B]]:
    """Combines independent results, keeping faults from both in input order."""
    match a:
        case Result(tag="ok", ok=left):
            return b.map(lambda right: (left, right))
        case Result(error=faults):
            return Error((faults[0], *chain(faults[1:], b.swap().default_value(()))))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Error", "Fault", "Faults", "Ok", "Outcome", "Reason", "accumulated", "paired"]
