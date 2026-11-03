"""Reconcile a declared font set from its originals: source, build, check, approve, run and publish each face."""

from enum import StrEnum
lazy import sys

from cyclopts import App, Parameter
import msgspec
lazy from expression import Result

from typography.faces.faults import Reason
lazy from typography.faces.faults import Outcome

from .commands import foundry

# --- [CONSTANTS] ------------------------------------------------------------------------

_EXITS: frozendict[StrEnum, int] = frozendict({Reason.OUTSIDE: 65, Reason.INVALID: 65, Reason.ABSENT: 65, Reason.UNMET: 65, Reason.UNREADABLE: 66, Reason.UNWRITABLE: 73, Reason.FAILED: 70})
_LINES = msgspec.json.Encoder(enc_hook=str)

# --- [OPERATIONS] -----------------------------------------------------------------------


def exited(result: Outcome[tuple[object, ...]] | None, /) -> int:
    """Writes each report as one JSON line on stdout, or each distinct fault message on stderr, and returns the exit status of the first fault's reason."""
    match result:
        case None:
            return 0
        case Result(tag="ok", ok=values):
            sys.stdout.buffer.write(_LINES.encode_lines(values))
            return 0
        case Result(error=faults):
            sys.stderr.writelines(dict.fromkeys(f"{fault.message}\n" for fault in faults))
            return _EXITS[faults[0].reason]


# --- [COMPOSITION] ----------------------------------------------------------------------

app = App(help=__doc__, result_action=(exited, "sys_exit"), default_parameter=Parameter(negative=()), error_formatter=str)
app.default(foundry)

if __name__ == "__main__":
    app()
