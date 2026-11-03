"""Faults, their reasons, and result accumulation for every typography package."""

from collections.abc import Iterable
from enum import StrEnum
from typing import overload

from expression import Error, Ok, Result
from expression.collections import Block
import msgspec

# --- [TYPES] ----------------------------------------------------------------------------

type Faults = tuple[Fault, *tuple[Fault, ...]]
type Outcome[T] = Result[T, Faults]


class Reason(StrEnum):
    """Refusal kinds, each value the report template of its fault."""

    OUTSIDE = "{subject} accepts {member}, not {cause}"
    INVALID = "{subject} is invalid. {cause}"
    ABSENT = "{subject} holds no {member}"
    UNREADABLE = "{subject} is unreadable. {cause}"
    UNWRITABLE = "{subject} is unwritable. {cause}"
    FAILED = "{subject} failed. {cause}"
    UNMET = "{subject} misses {member}. {cause}"


# --- [MODELS] ---------------------------------------------------------------------------


class Fault(msgspec.Struct, frozen=True, kw_only=True):
    """Refused or unmet operation, its report text formatted from its reason."""

    reason: StrEnum
    subject: str
    member: str = ""
    cause: object = ""

    @property
    def message(self) -> str:
        """Report text of the fault."""
        return self.reason.format(subject=self.subject, member=self.member, cause=self.cause)

    @classmethod
    def of(cls, reason: StrEnum, subject: str, cause: object = "", /, *, member: str = "") -> Faults:
        """Returns a one-fault tuple over subject, the refused value or caught error as cause."""
        return (cls(reason=reason, subject=subject, member=member, cause=cause),)


# --- [OPERATIONS] -----------------------------------------------------------------------


def accumulated[T](results: Iterable[Outcome[T]]) -> Outcome[tuple[T, ...]]:
    """Combines results, keeping every fault in input order."""
    block: Block[Outcome[T]] = Block.of_seq(results)
    match tuple(*faults for faults in block.map(Result.swap).choose(Result.to_option)):
        case (head, *rest):
            return Error((head, *rest))
        case _:
            return Ok(tuple(block.choose(Result.to_option)))


@overload
def paired[A, B](a: Outcome[A], b: Outcome[B], /) -> Outcome[tuple[A, B]]: ...
@overload
def paired[A, B, C](a: Outcome[A], b: Outcome[B], c: Outcome[C], /) -> Outcome[tuple[A, B, C]]: ...
@overload
def paired[A, B, C, D](a: Outcome[A], b: Outcome[B], c: Outcome[C], d: Outcome[D], /) -> Outcome[tuple[A, B, C, D]]: ...
@overload
def paired[A, B, C, D, E](a: Outcome[A], b: Outcome[B], c: Outcome[C], d: Outcome[D], e: Outcome[E], /) -> Outcome[tuple[A, B, C, D, E]]: ...
def paired(*results: Outcome[object]) -> Outcome[tuple[object, ...]]:
    """Combines two to five results into one tuple, keeping every fault in input order."""
    return accumulated(results)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Fault", "Faults", "Outcome", "Reason", "accumulated", "paired"]
