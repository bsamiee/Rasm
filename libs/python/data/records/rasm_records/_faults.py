"""Faults, their reasons, and result accumulation."""

from collections.abc import Iterable
from enum import StrEnum
from string import Formatter
import traceback
from typing import Never, overload

from expression import Option, Result
import msgspec

# --- [TYPES] ----------------------------------------------------------------------------

type Faults = tuple[Fault, *tuple[Fault, ...]]
type Outcome[T] = Result[T, Faults]

Ok = Result[Never, Faults].Ok
Error = Result[Never, Never].Error
Nothing = Option[Never].Nothing()


class Reason(StrEnum):
    """Refusal kinds, each value the report template of its fault, a cause the template names no field for following it as a sentence."""

    OUTSIDE = "{subject} accepts {member}, not {cause}"
    INVALID = "{subject} is invalid"
    ABSENT = "{subject} holds no {member}"
    UNMET = "{subject} misses {member}"
    FAILED = "{subject} failed"
    UNREADABLE = "{subject} is unreadable"
    UNWRITABLE = "{subject} is unwritable"


# --- [MODELS] ---------------------------------------------------------------------------


class Fault(msgspec.Struct, frozen=True, kw_only=True):
    """Refused or unmet operation, its report text formatted from its reason."""

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
    def of(cls, reason: StrEnum, subject: str, cause: object | None = None, /, *, member: str | None = None) -> Faults:
        """Returns a one-fault tuple over subject, the refused value or caught error as cause."""
        return (cls(reason=reason, subject=subject, member=member, cause=cause),)


# --- [OPERATIONS] -----------------------------------------------------------------------


def accumulated[T](results: Iterable[Outcome[T]]) -> Outcome[tuple[T, ...]]:
    """Combines results, keeping every fault in input order."""
    values, faults = list[T](), list[Fault]()
    for result in results:
        match result:
            case Result(tag="ok", ok=value):
                if not faults:
                    values.append(value)
            case Result(error=errors):
                values.clear()
                faults.extend(errors)
    remaining = iter(faults)
    head = next(remaining, None)
    return Error((head, *remaining)) if head is not None else Ok(tuple(values))


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
    """Returns value narrowed to kind, `OUTSIDE` fault naming the kind and the class found."""
    return Ok(value) if isinstance(value, kind) else Error(Fault.of(Reason.OUTSIDE, subject, type(value), member=kind.__name__))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Error", "Fault", "Faults", "Nothing", "Ok", "Outcome", "Reason", "accumulated", "paired", "typed"]
