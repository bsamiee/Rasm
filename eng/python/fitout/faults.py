"""Failure every fitout command returns, with reason, subject, and cause."""

from enum import StrEnum

import msgspec

# --- [TYPES] ----------------------------------------------------------------------------

type Faults = tuple[Fault, ...]


class Reason(StrEnum):
    """Fitout refusals, each value the message template of its fault."""

    PROCESS = "{subject} failed. {cause}"
    INVALID = "{subject} is invalid. {cause}"
    UNWRITABLE = "{subject} is unwritable. {cause}"
    UNANSWERED = "{subject} gave no answer. {cause}"
    REMOTE = "{subject} raised in Blender. {cause}"


# --- [ERRORS] ---------------------------------------------------------------------------


class Fault(msgspec.Struct, frozen=True, kw_only=True):
    """Failure of a fitout step, its message formatted from its reason."""

    reason: Reason
    subject: str
    cause: BaseException | str

    @property
    def message(self) -> str:
        """Report text of the fault."""
        return self.reason.format(subject=self.subject, cause=self.cause)

    @classmethod
    def of(cls, reason: Reason, subject: object, cause: BaseException | str) -> Faults:
        """Faults of a cause, one per leaf of an exception group."""
        return tuple(*cls.of(reason, subject, leaf) for leaf in cause.exceptions) if isinstance(cause, BaseExceptionGroup) else (cls(reason=reason, subject=str(subject), cause=cause),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Fault", "Faults", "Reason"]
