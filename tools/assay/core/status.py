"""Rail status values, their severity fold, and the bounded fault-step vocabulary."""

from enum import StrEnum
from functools import reduce
from typing import Self


# --- [TYPES] ----------------------------------------------------------------------------


class RailStatus(StrEnum):
    """Rail status with its wire token, exit code, and severity rank."""

    exit_code: int  # bound by __new__; not a real descriptor
    severity: int

    SKIP = "skip", 0, 0, "skipped"
    EMPTY = "empty", 0, 1
    OK = "ok", 0, 2
    DEGRADED = "degraded", 2, 3
    CANDIDATE = "candidate", 2, 4
    UNSUPPORTED = "unsupported", 3, 5
    BUSY = "busy", 5, 6
    TIMEOUT = "timeout", 5, 7
    FAILED = "failed", 1, 8
    FAULTED = "faulted", 2, 9

    def __new__(cls, value: str, exit_code: int, severity: int, *aliases: str) -> Self:
        """Bind the wire token, exit code, severity, and string aliases."""
        member = str.__new__(cls, value)
        member._value_ = value
        member.exit_code = exit_code
        member.severity = severity
        for alias in aliases:
            member._add_value_alias_(alias)  # Python 3.13+ raises on cross-member alias collisions
        return member

    @classmethod
    def from_returncode(cls, rc: int) -> RailStatus:
        """Project a process return code onto the nearest rail status.

        Returns:
            EMPTY for 0, BUSY for 5, TIMEOUT for 124, FAILED for everything else.
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


class Step(StrEnum):
    """Fault-step taxonomy whose declaration order drives prefix classification.

    ``scan=True`` members may appear as ``{step}:`` message prefixes; status-derived members stay classification-only.
    """

    scan: bool  # bound by __new__; True for the prefix-scan roster, False for status-derived classifications

    STRICT = "strict", True
    VALIDATION = "validation", True
    CONFIG = "config", True
    DISPATCH = "dispatch", True
    PARSE = "parse", True
    SPAWN = "spawn", True
    TIMEOUT = "timeout", False
    LEASE_BUSY = "lease_busy", False
    DEFECTS = "defects", False

    def __new__(cls, value: str, scan: bool) -> Self:  # noqa: FBT001  # positional enum-member payload, not a boolean knob
        """Bind the wire token and the prefix-scan roster flag."""
        member = str.__new__(cls, value)
        member._value_ = value
        member.scan = scan
        return member


# --- [OPERATIONS] -----------------------------------------------------------------------


def join(left: RailStatus, right: RailStatus) -> RailStatus:
    """Return the higher-severity status, keeping the left status on ties."""
    return left if left.severity >= right.severity else right


def fold(*members: RailStatus) -> RailStatus:
    """Reduce statuses under ``join``, seeded at ``EMPTY``.

    Returns:
        The highest-severity status among the supplied members.
    """
    return reduce(join, members, RailStatus.EMPTY)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RailStatus", "Step", "fold", "join"]
