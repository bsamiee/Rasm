"""Status algebra: one ``StrEnum`` whose members carry exit code and fold rank.

Each member rides three ``__new__`` payloads — the wire ``value``, an
``exit_code``, and a ``severity`` fold rank forming a bounded join semilattice.
The single member instance serves Cyclopts choice tokens, ``msgspec``
encode/decode, and ``match`` dispatch, so the exit code lives in exactly one
place and ``__main__`` returns ``report.status.exit_code``.
"""

from enum import StrEnum
from functools import reduce
from typing import Self


# --- [TYPES] ----------------------------------------------------------------------------


class RailStatus(StrEnum):
    """Status algebra: wire token, exit code, and severity fold rank.

    The member IS its wire string, so one instance serves Cyclopts choices,
    ``msgspec`` (encode-by-``_value_``, decode by value/alias via
    ``_value2member_map_``), and ``match`` dispatch. ``FAULTED`` (severity 7) is
    the absorbing fold top; ``EMPTY`` (severity 1) is the fold seed.

    Members are ``str`` subclasses, so ``RailStatus.OK == "ok"`` holds: dispatch
    on ``case RailStatus.OK:`` (never ``case "ok":``) and close with
    ``assert_never`` so an added member breaks the type checker rather than
    silently slipping through that equality footgun.
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
        """Bind the wire string as member identity and attach the payloads."""
        member = str.__new__(cls, value)
        member._value_ = value
        member.exit_code = exit_code
        member.severity = severity
        for alias in aliases:
            member._add_value_alias_(alias)  # py3.13+ alias in _value2member_map_; fails loud on cross-member collision
        return member

    @classmethod
    def from_returncode(cls, rc: int) -> RailStatus:
        """Project a process return code onto a status.

        Never yields ``OK`` (only a parser affirms ``OK``) or ``UNSUPPORTED`` (a
        routing decision, not a process outcome).
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
    """Binary max-by-severity: the higher fold rank wins, ties keep left.

    Module-scoped, not a method: ``RailStatus`` is a ``str`` subclass and
    ``str.join`` already owns the ``join`` name with an LSP-incompatible signature.
    """
    return left if left.severity >= right.severity else right


def fold(*members: RailStatus) -> RailStatus:
    """Reduce members under ``join`` seeded at ``EMPTY``.

    The seed is ``EMPTY`` (rank 1), not ``SKIP`` (rank 0), so a rail that ran
    with no checks stays distinguishable from a vacuous per-check opt-out.
    """
    return reduce(join, members, RailStatus.EMPTY)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["RailStatus", "fold", "join"]
