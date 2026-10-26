"""Fault, result type, and result accumulation."""

from collections.abc import Iterable
from enum import StrEnum
from itertools import chain
from typing import overload

from expression import Error, Ok, Result
from expression.collections import Block
import msgspec

# --- [TYPES] ----------------------------------------------------------------------------

type Faults = tuple[Fault, *tuple[Fault, ...]]
type Outcome[T] = Result[T, Faults]


class RecordReason(StrEnum):
    """Numeric, file, and domain refusals, each value the report template of its fault."""

    NUMERIC = "Numeric operation for {subject} failed. {cause}"
    UNREADABLE = "{subject} is unreadable. {cause}"
    UNWRITABLE = "{subject} is unwritable. {cause}"
    OUTSIDE = "{subject} accepts {member}, not {cause}"


# --- [MODELS] ---------------------------------------------------------------------------


class Fault(msgspec.Struct, frozen=True, kw_only=True):
    """Refused or unmet operation, its report text formatted from its reason."""

    reason: StrEnum
    subject: str
    member: str = ""
    cause: BaseException | str = ""

    @property
    def message(self) -> str:
        """Report text of the fault."""
        return self.reason.format(subject=self.subject, member=self.member, cause=self.cause)


# --- [OPERATIONS] -----------------------------------------------------------------------


def accumulated[T](results: Iterable[Outcome[T]]) -> Outcome[tuple[T, ...]]:
    """Combines results, keeping every fault in input order."""
    block: Block[Outcome[T]] = Block.of_seq(results)
    faults: tuple[Fault, ...] = tuple(chain.from_iterable(block.map(Result.swap).choose(Result.to_option)))
    return Error((faults[0], *faults[1:])) if faults else Ok(tuple(block.choose(Result.to_option)))


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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Fault", "Faults", "Outcome", "RecordReason", "accumulated", "paired"]
