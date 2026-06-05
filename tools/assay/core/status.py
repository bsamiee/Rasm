"""Define rail status values and their severity fold."""

from enum import StrEnum
from functools import reduce
from typing import Self


# --- [TYPES] ----------------------------------------------------------------------------


class RailStatus(StrEnum):
    """Rail status with its wire token, exit code, and severity rank."""

    exit_code: int  # annotation-only field; __new__ binds the payload after enum construction
    severity: int

    SKIP = "skip", 0, 0, "skipped"
    EMPTY = "empty", 0, 1
    OK = "ok", 0, 2
    UNSUPPORTED = "unsupported", 3, 3
    BUSY = "busy", 5, 4
    TIMEOUT = "timeout", 5, 5
    FAILED = "failed", 1, 6
    FAULTED = "faulted", 2, 7

    def __new__(cls, value: str, exit_code: int, severity: int, *aliases: str) -> Self:
        """Bind the wire token, process exit code, severity, and aliases."""
        member = str.__new__(cls, value)
        member._value_ = value
        member.exit_code = exit_code
        member.severity = severity
        for alias in aliases:
            member._add_value_alias_(alias)  # Python 3.13+ alias registration rejects cross-member collisions
        return member

    @classmethod
    def from_returncode(cls, rc: int) -> RailStatus:
        """Project a process return code onto a rail status.

        Returns:
            Status represented by the process return code.
        """
        match rc:
            case 0:
                return cls.EMPTY
            case 5:
                return cls.BUSY
            case 124:
                return cls.TIMEOUT
            case _:
                return cls.FAILED


# --- [OPERATIONS] -----------------------------------------------------------------------


def join(left: RailStatus, right: RailStatus) -> RailStatus:
    """Return the higher-severity status, keeping the left status on ties."""
    return left if left.severity >= right.severity else right


def fold(*members: RailStatus) -> RailStatus:
    """Reduce statuses under `join`, seeded at `EMPTY`.

    Returns:
        Highest-severity status from the supplied members.
    """
    return reduce(join, members, RailStatus.EMPTY)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["RailStatus", "fold", "join"]
