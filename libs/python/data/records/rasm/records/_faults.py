"""Faults, their reasons, and result accumulation."""

from collections.abc import Iterable
from datetime import date
from enum import StrEnum
from functools import partial, singledispatch
import os
import reprlib
from string import Formatter
import sys
import traceback
from types import FunctionType
from typing import Final, overload

from expression import Error, Ok, option, Result, try_downcast
import msgspec
import numpy as np

# --- [TYPES] ----------------------------------------------------------------------------

type Faults = tuple[Fault, *tuple[Fault, ...]]
type Outcome[T] = Result[T, Faults]


class Reason(StrEnum):
    """Refusal kinds, each value the report template of its fault, a cause the template names no field for following it as a sentence."""

    OUTSIDE = "{subject} accepts {member}, not {cause}"
    INVALID = "{subject} is invalid"
    ABSENT = "{subject} holds no {member}"
    UNMET = "{subject} misses {member}"
    FAILED = "{subject} failed"
    UNREADABLE = "{subject} is unreadable"
    UNWRITABLE = "{subject} is unwritable"


# --- [CONSTANTS] ------------------------------------------------------------------------

_BOUNDED: Final = reprlib.Repr(maxstring=sys.maxsize, maxother=sys.maxsize)

# --- [MODELS] ---------------------------------------------------------------------------


class Fault(msgspec.Struct, frozen=True, kw_only=True):
    """Refused or unmet operation, its subject and member held as the text they showed when it was built."""

    reason: StrEnum
    subject: str
    member: str | None = None
    cause: object | None = None

    @property
    def message(self) -> str:
        """Report text of the fault."""
        return self.filled(self.reason)

    def filled(self, template: str, /) -> str:
        """Returns a message template with the fault fields filled, the form a translated template takes, and a cause it names no field for as a following sentence."""
        cause = "".join(traceback.format_exception_only(self.cause, show_group=True)).rstrip() if isinstance(self.cause, BaseExceptionGroup) else self.cause
        text = template.format(subject=self.subject, member=self.member, cause=cause)
        return text if cause is None or any(field == "cause" for _, field, _, _ in Formatter().parse(template)) else f"{text}. {cause}"

    @classmethod
    def of(cls, reason: StrEnum, subject: object, cause: object | None = None, /, *, member: object | None = None) -> Faults:
        """Returns a one-fault tuple over the text subject and member show now, the refused value or caught error as cause."""
        return (cls(reason=reason, subject=shown(subject), member=None if member is None else shown(member), cause=cause),)


# --- [OPERATIONS] -----------------------------------------------------------------------


@singledispatch
def shown(value: object, /) -> str:
    """Returns the text of a fault value: each registered kind through its own text, any other value through `reprlib` with its container bounds and no cut of a name or reference."""
    return _BOUNDED.repr(value)


shown.register(str, str.__str__)
shown.register(os.PathLike, os.fsdecode)
shown.register(type | FunctionType, lambda value: value.__qualname__)
shown.register(date, str)
shown.register(np.ndarray, str)


def accumulated[T](results: Iterable[Outcome[T]]) -> Outcome[tuple[T, ...]]:
    """Combines results, keeping every fault in input order."""
    values, faults = list[T](), list[Fault]()
    for result in results:
        match result:
            case Ok(value):
                values.append(value)
            case Error(errors):
                faults.extend(errors)
    match faults:
        case [head, *rest]:
            return Error((head, *rest))
        case _:
            return Ok(tuple(values))


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


def typed[T](kind: type[T], subject: object, value: object, /) -> Outcome[T]:
    """Returns value narrowed to kind, `OUTSIDE` fault naming the kind and the class found."""
    return option.of_optional(try_downcast(kind, value)).to_result_with(partial(Fault.of, Reason.OUTSIDE, subject, type(value), member=kind))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Fault", "Faults", "Outcome", "Reason", "accumulated", "paired", "shown", "typed"]
