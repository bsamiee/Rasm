"""Checks other programs run over built files: OTS acceptance and the fontspector universal profile, each failure a finding of its build."""

from enum import auto, StrEnum
from pathlib import Path
lazy from collections.abc import Callable
lazy from functools import partial
lazy import subprocess

import msgspec
lazy from expression.extra.result import catch
lazy import ots

lazy from rasm_records import Error, Fault, Ok, Outcome, Reason
lazy from rasm_typography import Faces, Finding, OrnamentFace

# --- [TYPES] ----------------------------------------------------------------------------


class _Severity(StrEnum):
    """Fontspector status of one subresult, in its own spelling."""

    SKIP = "SKIP"
    INFO = "INFO"
    PASS = "PASS"
    WARN = "WARN"
    FAIL = "FAIL"
    ERROR = "ERROR"
    FATAL = "FATAL"


class _Program(StrEnum):
    """Requirement another program measures, its value the finding's requirement name."""

    SANITIZED = auto()
    PROFILE = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_FAILING = frozenset({_Severity.FAIL, _Severity.ERROR, _Severity.FATAL})
_ORNAMENT_EXCLUDED = frozenset({"opentype/monospace"})

# --- [MODELS] ---------------------------------------------------------------------------


class _Result(msgspec.Struct, frozen=True, kw_only=True):
    severity: _Severity
    code: str = ""
    message: str = ""


class _Check(msgspec.Struct, frozen=True, kw_only=True):
    """Fontspector check result on one font file or on the folder collection holding it."""

    check_id: str
    subresults: tuple[_Result, ...] = ()


class _Report(msgspec.Struct, frozen=True, kw_only=True):
    results: frozendict[str, frozendict[str, tuple[_Check, ...]]]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _ran(*command: str, check: bool = True) -> Outcome[bytes]:
    """Runs a program to its exit and returns its stdout, `FAILED` for a failed launch or, under `check`, a nonzero exit with its stderr as cause."""
    try:
        return Ok(subprocess.run(command, capture_output=True, check=check).stdout)
    except OSError as error:
        return Error(Fault.of(Reason.FAILED, " ".join(command), error))
    except subprocess.CalledProcessError as error:
        return Error(Fault.of(Reason.FAILED, " ".join(command), error.stderr.decode().strip()))


def _inspected(paths: tuple[Path, ...], /) -> Outcome[frozendict[Path, tuple[_Check, ...]]]:
    """Runs fontspector once over built faces under its universal profile.

    Args:
        paths: Built font files.

    Returns:
        Ok(checks) per path holding the results on the file and on its folder's collection, `FAILED` for a failed launch, `INVALID` for an undecodable report.
    """
    return (
        _ran("fontspector", "--quiet", "--skip-network", "--json", "-", *(str(path.absolute()) for path in paths), check=False)
        .bind(lambda raw: catch(lambda: msgspec.json.decode(raw, type=_Report), exception=msgspec.DecodeError)().map_error(partial(Fault.of, Reason.INVALID, "fontspector")))
        .map(
            lambda report: frozendict({
                path: tuple(check for tested in (path.absolute(), path.absolute().parent) for checks in report.results.get(str(tested), frozendict()).values() for check in checks) for path in paths
            })
        )
    )


def _breached(checks: Outcome[tuple[_Check, ...]], selected: Callable[[_Check], bool], /) -> tuple[tuple[str, str, str], ...]:
    """One breach per failed selected check, or one breach per fault where the run could not measure."""
    return checks.map(
        lambda found: tuple(
            (f"{check.check_id} {result.code}".strip(), result.severity, "WARN") for check in found if selected(check) for result in check.subresults if result.severity in _FAILING
        )
    ).default_with(lambda faults: tuple((fault.subject, fault.message, "measured") for fault in faults))


def _external(face: Faces, path: Path, checks: Outcome[tuple[_Check, ...]], /) -> tuple[Finding, ...]:
    """Findings of OTS on a built file and of its fontspector results, outside the check ids its face kind never meets.

    Args:
        face: Declared face.
        path: Built file.
        checks: Fontspector results on the file, from `_inspected`.

    Returns:
        Findings tagged `sanitized` and `profile`, an empty tuple passing.
    """
    excluded = _ORNAMENT_EXCLUDED if isinstance(face, OrnamentFace) else frozenset()
    sanitized = ots.sanitize(str(path), capture_output=True)  # type: ignore[no-untyped-call]
    return tuple(
        Finding(requirement=requirement, subject=subject, measured=measured, bound=bound)
        for requirement, breaches in (
            (_Program.SANITIZED, () if sanitized.returncode == 0 else ((path.name, sanitized.stderr.decode().strip() or f"exit {sanitized.returncode}", "accepted"),)),
            (_Program.PROFILE, _breached(checks, lambda check: check.check_id not in excluded)),
        )
        for subject, measured, bound in breaches
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
