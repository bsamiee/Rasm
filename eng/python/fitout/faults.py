"""Failure every fitout command returns, with message and exit status."""

from enum import StrEnum
lazy import subprocess

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
    """Failure of a fitout step, rendered as its message and exit status."""

    reason: Reason
    subject: str
    cause: BaseException | str

    @property
    def message(self) -> str:
        """Report text of the fault."""
        return self.reason.format(subject=self.subject, cause=self.cause)

    @property
    def status(self) -> int:
        """Exit status of a failed child process, else 1."""
        match self.cause:
            case subprocess.CalledProcessError(returncode=status):
                return status
            case _:
                return 1

    @classmethod
    def caught(cls, reason: Reason, subject: object, error: BaseException) -> Faults:
        """Faults of a caught error, one per leaf of a group."""
        match error:
            case BaseExceptionGroup(exceptions=leaves):
                return tuple(*cls.caught(reason, subject, leaf) for leaf in leaves)
            case _:
                return (cls(reason=reason, subject=str(subject), cause=error),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Fault", "Faults", "Reason"]
