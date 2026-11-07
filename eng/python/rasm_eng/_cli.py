"""Writes extension assets, packs and installs Blender extensions, syncs host constraints, and runs Blender in isolated profiles."""

from importlib.metadata import metadata
import sys
lazy import subprocess

from cyclopts import App, Parameter
lazy from expression import Result
lazy import msgspec

lazy from rasm_records import Fault, Outcome

# --- [CONSTANTS] ------------------------------------------------------------------------

_PACKAGE = __name__.partition(".")[0]

# --- [OPERATIONS] -----------------------------------------------------------------------


def _reported(result: Outcome[object] | None) -> int:
    """Returns a run's status, 0 after printing any other command's produced state as JSON, a lone failed child's status, 1 for other faults, and 0 for help."""
    match result:
        case None:
            return 0
        case Result(tag="ok", ok=int(status)):
            return status
        case Result(tag="ok", ok=state):
            sys.stdout.buffer.write(msgspec.json.encode(state, enc_hook=str) + b"\n")
            return 0
        case Result(error=(Fault(cause=subprocess.CalledProcessError(returncode=status)) as fault,)):
            sys.stderr.write(f"{fault.message}\n")
            return status
        case Result(error=faults):
            sys.stderr.writelines(f"{fault.message}\n" for fault in faults)
            return 1


# --- [COMPOSITION] ----------------------------------------------------------------------

app = App(help=metadata(_PACKAGE)["Summary"], result_action=(_reported, "sys_exit"), default_parameter=Parameter(negative=()))
app.command(f"{_PACKAGE}._assets:assets")
app.command(f"{_PACKAGE}._assembly:pack")
app.command(f"{_PACKAGE}._install:install")
app.command(f"{_PACKAGE}._assembly:upgrade")
app.command(f"{_PACKAGE}._evaluate:run")

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["app"]
