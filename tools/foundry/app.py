"""Reconcile a declared font set from its originals: source, build, check, approve, run and publish each face."""

lazy import sys

from cyclopts import App, Parameter
lazy from expression import Result
lazy import msgspec
lazy import sysexits

from typography.builds.pipeline import performed
lazy from typography.faces.faults import Outcome, Reason

# --- [OPERATIONS] -----------------------------------------------------------------------


def exited(result: Outcome[tuple[object, ...]] | None, /) -> int:
    """Writes each report as one JSON line on stdout, or each distinct fault message on stderr, and returns the exit status of the first fault's reason."""
    match result:
        case None:
            return 0
        case Result(tag="ok", ok=values):
            sys.stdout.buffer.write(msgspec.json.Encoder(enc_hook=str).encode_lines(values))
            return 0
        case Result(error=faults):
            sys.stderr.writelines(dict.fromkeys(f"{fault.message}\n" for fault in faults))
            return {
                **dict.fromkeys((Reason.OUTSIDE, Reason.INVALID, Reason.ABSENT, Reason.UNMET), sysexits.EX_DATAERR),
                Reason.UNREADABLE: sysexits.EX_NOINPUT,
                Reason.UNWRITABLE: sysexits.EX_CANTCREAT,
                Reason.FAILED: sysexits.EX_SOFTWARE,
            }[faults[0].reason]


# --- [COMPOSITION] ----------------------------------------------------------------------

app = App(help=__doc__, result_action=(exited, "sys_exit"), default_parameter=Parameter(negative=()), error_formatter=str)
app.default(performed)

if __name__ == "__main__":
    app()
