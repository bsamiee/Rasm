"""Fontspector runs over built files under its universal profile, and the program runner every check of another program goes through."""

from enum import auto, StrEnum
from pathlib import Path
from typing import override
lazy from functools import partial
lazy import subprocess

import msgspec
lazy from expression.extra.result import catch

lazy from rasm_records import Error, Fault, Ok, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------


class Severity(StrEnum):
    """Fontspector status of one subresult, in its own spelling."""

    @override
    @staticmethod
    def _generate_next_value_(name: str, start: int, count: int, last_values: list[str]) -> str:
        return name

    SKIP = auto()
    INFO = auto()
    PASS = auto()
    WARN = auto()
    FAIL = auto()
    ERROR = auto()
    FATAL = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class _Result(msgspec.Struct, frozen=True, kw_only=True):
    severity: Severity
    code: str | None = None
    message: str | None = None


class Check(msgspec.Struct, frozen=True, kw_only=True):
    """Fontspector check result on one font file or on the folder collection holding it."""

    check_id: str
    subresults: tuple[_Result, ...] = ()


class _Report(msgspec.Struct, frozen=True, kw_only=True):
    results: frozendict[str, frozendict[str, tuple[Check, ...]]]


# --- [OPERATIONS] -----------------------------------------------------------------------


def ran(*command: str, check: bool = True) -> Outcome[bytes]:
    """Runs a program to its exit and returns its stdout, `FAILED` for a failed launch or, under `check`, a nonzero exit with its stderr as cause."""
    try:
        return Ok(subprocess.run(command, capture_output=True, check=check).stdout)
    except OSError as error:
        return Error(Fault.of(Reason.FAILED, " ".join(command), error))
    except subprocess.CalledProcessError as error:
        return Error(Fault.of(Reason.FAILED, " ".join(command), error.stderr.decode().strip()))


def inspected(paths: tuple[Path, ...], /) -> Outcome[frozendict[Path, tuple[Check, ...]]]:
    """Runs fontspector once over built faces under its universal profile.

    Args:
        paths: Built font files.

    Returns:
        Ok(checks) per path holding the results on the file and on its folder's collection, `FAILED` for a failed launch, `INVALID` for an undecodable report.
    """
    return (
        ran("fontspector", "--quiet", "--skip-network", "--json", "-", *(str(path.absolute()) for path in paths), check=False)
        .bind(lambda raw: catch(lambda: msgspec.json.decode(raw, type=_Report), exception=msgspec.DecodeError)().map_error(partial(Fault.of, Reason.INVALID, "fontspector")))
        .map(
            lambda report: frozendict({
                path: tuple(check for tested in (path.absolute(), path.absolute().parent) for checks in report.results.get(str(tested), frozendict()).values() for check in checks) for path in paths
            })
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Check", "Severity", "inspected", "ran"]
