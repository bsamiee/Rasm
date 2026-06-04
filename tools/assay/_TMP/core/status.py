"""The sole status algebra: one ``StrEnum`` carrying exit code and fold rank.

Each member rides three payloads through a 3-argument ``__new__``: the wire
``value``, an ``exit_code``, and a ``severity`` fold rank (a 7-fold total order
forming a bounded join semilattice). One member instance serves three
subsystems unchanged — Cyclopts choice token, ``msgspec`` encode/decode value,
and ``match`` discriminant — so the exit code originates in exactly one place
and ``__main__`` only ever returns ``report.status.exit_code``.
"""

from enum import StrEnum
from functools import reduce
from typing import Self


# --- [TYPES] ----------------------------------------------------------------------------


class RailStatus(StrEnum):
    """The whole status algebra: wire token, exit code, and severity fold rank.

    The member IS its wire string (the ``StrEnum`` contract), so one instance
    serves Cyclopts choices, ``msgspec`` (encode-by-``_value_``, decode by value
    and alias via ``_value2member_map_``), and ``match`` dispatch. ``FAULTED``
    (severity 7) is the absorbing fold top; ``EMPTY`` (severity 1) is the fold
    seed. Members are ``str`` subclasses, so ``RailStatus.OK == "ok"`` holds:
    restrict domain dispatch to ``case RailStatus.OK:`` (never ``case "ok":``)
    and close with ``assert_never`` so an added member breaks the type checker
    rather than silently falling through that equality footgun.
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
        """Bind the wire string as the member identity and attach the payloads.

        ``_add_value_alias_`` fails loud if an external spelling would shadow an
        existing value or alias, so an alias can never be shared across members.
        """
        member = str.__new__(cls, value)
        member._value_ = value
        member.exit_code = exit_code
        member.severity = severity
        for alias in aliases:
            member._add_value_alias_(alias)  # py3.13+ alias registration in _value2member_map_
        return member

    @classmethod
    def from_returncode(cls, rc: int) -> RailStatus:
        """Project a process return code onto a status.

        Never ``OK`` (only a parser affirms ``OK``) and never ``UNSUPPORTED``
        (a routing decision, not a process outcome).
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

    Module-scoped rather than a method because ``RailStatus`` is a ``str``
    subclass and ``str.join`` already owns the ``join`` name with an
    LSP-incompatible signature.
    """
    return left if left.severity >= right.severity else right


def fold(*members: RailStatus) -> RailStatus:
    """Reduce members under ``join`` seeded at ``EMPTY``.

    The seed is the second-lowest rank, not ``SKIP``, precisely so a rail that
    genuinely ran but had no checks (``EMPTY``) stays distinguishable from a
    vacuous per-check opt-out (``SKIP``).
    """
    return reduce(join, members, RailStatus.EMPTY)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["RailStatus", "fold", "join"]
