"""Define rail status values and severity folding."""

from enum import StrEnum
from functools import reduce
from typing import Self


# --- [TYPES] ----------------------------------------------------------------------------


class RailStatus(StrEnum):
    """Rail status with wire token, exit code, and severity rank.

    Attributes:
        exit_code: Process exit code for the status.
        severity: Rank used by status joins.

    """

    exit_code: int  # annotation-only: the metaclass skips it as a member, __new__ sets it
    severity: int

    SKIP = "skip", 0, 0, "skipped"
    EMPTY = "empty", 0, 1
    OK = "ok", 0, 2
    UNSUPPORTED = "unsupported", 3, 3
    BUSY = "busy", 5, 4
    TIMEOUT = "timeout", 5, 5
    FAILED = "failed", 1, 6
    FAULTED = "faulted", 2, 7

    def __new__(cls, value: str, exit_code: int, severity: int, *aliases: str) -> Self:  # noqa: D102  # enum payload binder
        member = str.__new__(cls, value)
        member._value_ = value
        member.exit_code = exit_code
        member.severity = severity
        for alias in aliases:
            member._add_value_alias_(alias)  # py3.13+ alias in _value2member_map_; fails loud on cross-member collision
        return member

    @classmethod
    def from_returncode(cls, rc: int) -> RailStatus:
        """Project a process return code onto a rail status.

        Args:
            rc: Process return code.

        Returns:
            `EMPTY` for zero, `BUSY` for 5, `TIMEOUT` for 124, otherwise `FAILED`.

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
    """Join two statuses by severity.

    Args:
        left: Left status.
        right: Right status.

    Returns:
        Higher-severity status, keeping the left value on ties.

    """
    return left if left.severity >= right.severity else right


def fold(*members: RailStatus) -> RailStatus:
    """Reduce statuses under `join`.

    Args:
        *members: Statuses to fold.

    Returns:
        Joined status seeded at `EMPTY`.

    """
    return reduce(join, members, RailStatus.EMPTY)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["RailStatus", "fold", "join"]
