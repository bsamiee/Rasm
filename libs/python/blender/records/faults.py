"""Fault reasons, fault, result type, result accumulation, and type narrowing."""

from collections.abc import Iterable
from enum import StrEnum
from itertools import chain
from typing import overload

from expression import Error, Ok, Option, Result, try_downcast
from expression.collections import Block
import msgspec

# --- [TYPES] ----------------------------------------------------------------------------

type Faults = tuple[Fault, *tuple[Fault, ...]]
type Outcome[T] = Result[T, Faults]


class Reason(StrEnum):
    """Refusal of a value, lookup, requirement, edit, registration, operation, or file, each value the report template of its fault."""

    OUTSIDE = "{subject} accepts {member}, not {cause}"
    INVALID = "{subject} is invalid. {cause}"
    ABSENT = "{subject} holds no {member}"
    TAKEN = "{subject} is taken at {member}"
    REQUIRED = "Requires {subject}"
    REFUSED = "{subject} refused edit. {cause}"
    UNREGISTERED = "Registration of {subject} refused. {cause}"
    ANSWERED = "{subject} answered {member}"
    FAILED = "{subject} failed. {cause}"
    UNREADABLE = "{subject} is unreadable. {cause}"
    UNWRITABLE = "{subject} is unwritable. {cause}"


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
        """Faults of one fault over subject, the refused value or caught error as cause."""
        return (cls(reason=reason, subject=subject, member=member, cause=cause),)


# --- [OPERATIONS] -----------------------------------------------------------------------


def accumulated[T](results: Iterable[Outcome[T]]) -> Outcome[tuple[T, ...]]:
    """Combines results, keeping every fault in input order."""
    block: Block[Outcome[T]] = Block.of_seq(results)
    match tuple(chain.from_iterable(block.map(Result.swap).choose(Result.to_option))):
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
@overload
def paired[A, B, C, D, E, G](a: Outcome[A], b: Outcome[B], c: Outcome[C], d: Outcome[D], e: Outcome[E], g: Outcome[G], /) -> Outcome[tuple[A, B, C, D, E, G]]: ...
def paired(*results: Outcome[object]) -> Outcome[tuple[object, ...]]:
    """Combines two to six results into one tuple, keeping every fault in input order."""
    return accumulated(results)


def typed[T](kind: type[T], subject: str, value: object, /) -> Outcome[T]:
    """Returns value narrowed to kind, domain fault naming the kind and the class found, kind and subject first so `partial(typed, kind, subject)` serves a chain."""
    return Option.of_optional(try_downcast(kind, value)).to_result_with(lambda: Fault.of(Reason.OUTSIDE, subject, type(value).__name__, member=kind.__name__))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Fault", "Faults", "Outcome", "Reason", "accumulated", "paired", "typed"]
