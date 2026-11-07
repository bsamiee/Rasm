"""Writes extension assets, packs and installs Blender extensions, syncs host constraints, and runs Blender in isolated profiles."""

lazy import subprocess
lazy import sys

from cyclopts import App, Parameter
lazy from expression import Result

lazy from blender.records.faults import Fault, Outcome

# --- [OPERATIONS] -----------------------------------------------------------------------


def _reported(result: Outcome[int] | None) -> int:
    """Returns the status of a command, of a lone failed child, 1 for other faults, and 0 for help."""
    match result:
        case None:
            return 0
        case Result(tag="ok", ok=status):
            return status
        case Result(error=(Fault(cause=subprocess.CalledProcessError(returncode=status)) as fault,)):
            sys.stderr.write(f"{fault.message}\n")
            return status
        case Result(error=faults):
            sys.stderr.writelines(f"{fault.message}\n" for fault in faults)
            return 1


# --- [COMPOSITION] ----------------------------------------------------------------------

app = App(name=f"python -m {__package__}", help=__doc__, result_action=(_reported, "sys_exit"), default_parameter=Parameter(negative=()))
app.command(f"{__package__}.assets:assets")
app.command(f"{__package__}.assembly:pack")
app.command(f"{__package__}.install:install")
app.command(f"{__package__}.assembly:upgrade")
app.command(f"{__package__}.evaluate:run")


if __name__ == "__main__":
    app()
