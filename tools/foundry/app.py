"""Reconcile a declared font set from its originals: source, build, check, approve, run and publish each face."""

from functools import partial
from importlib.metadata import metadata
import sys

from cyclopts import App, Parameter
lazy from expression import Result
lazy import msgspec
lazy import sysexits

lazy from typography.builds.pipeline import Action, performed
lazy from typography.faces.faults import Outcome, Reason

# --- [OPERATIONS] -----------------------------------------------------------------------


def __getattr__(name: str, /) -> object:
    """Binds the action a registered command names to the pipeline run, importing the pipeline at that command's first use."""
    if name in app:
        return partial(performed, Action(name))
    raise AttributeError(f"module {__name__!r} has no attribute {name!r}", name=name, obj=sys.modules[__name__])


def exited(result: Outcome[tuple[object, ...]] | None, /) -> int:
    """Writes each report as one JSON line on stdout, or each distinct fault message on stderr, and returns the exit status of the first fault's reason."""
    match result:
        case None:
            return 0
        case Result(tag="ok", ok=values):
            sys.stdout.buffer.write(msgspec.json.Encoder().encode_lines(values))
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

app = App(help=metadata("foundry")["Summary"], result_action=(exited, "sys_exit"), default_parameter=Parameter(negative=()), error_formatter=str)
app.command(f"{__name__}:source", help="Write each selected face's UFO source from its originals.")
app.command(f"{__name__}:build", help="Compile each selected face whose inputs changed since its last build.")
app.command(f"{__name__}:check", help="Verify each selected face's current build.")
app.command(f"{__name__}:approve", help="Approve each selected face's shaping.")
app.command(f"{__name__}:run", help="Build and verify each selected face.")
app.command(f"{__name__}:publish", help="Publish each selected face's verified build into a destination folder.")

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["app"]
