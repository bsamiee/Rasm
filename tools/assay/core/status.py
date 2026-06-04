"""Status algebra: one ``StrEnum`` carrying wire token, exit code, and severity fold rank."""

from enum import StrEnum
from functools import reduce
from typing import Self


# --- [TYPES] ----------------------------------------------------------------------------


class RailStatus(StrEnum):
    """Status algebra: wire token, exit code, and severity fold rank.

    ``FAULTED`` (severity 7) is the absorbing fold top; ``EMPTY`` (severity 1) is the seed.
    Members are ``str`` subclasses (``RailStatus.OK == "ok"``), so dispatch on
    ``case RailStatus.OK:`` (never ``case "ok":``) and close with ``assert_never``.
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

    def __new__(cls, value: str, exit_code: int, severity: int, *aliases: str) -> Self:
        """Bind the wire string as member identity and attach the exit-code/severity payloads."""
        member = str.__new__(cls, value)
        member._value_ = value
        member.exit_code = exit_code
        member.severity = severity
        for alias in aliases:
            member._add_value_alias_(alias)  # py3.13+ alias in _value2member_map_; fails loud on cross-member collision
        return member

    @classmethod
    def from_returncode(cls, rc: int) -> RailStatus:
        """Project a process return code onto a status (never ``OK``/``UNSUPPORTED``: those are non-process)."""
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
    """Binary max-by-severity (ties keep left); module-scoped because ``str.join`` owns the method name."""
    return left if left.severity >= right.severity else right


def fold(*members: RailStatus) -> RailStatus:
    """Reduce members under ``join`` seeded at ``EMPTY`` (rank 1, not ``SKIP``: a no-check rail stays distinct)."""
    return reduce(join, members, RailStatus.EMPTY)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["RailStatus", "fold", "join"]
