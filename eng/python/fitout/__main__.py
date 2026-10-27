"""Packs and installs Blender extensions, syncs their wheels into Blender import folder, and runs Blender in fresh user bases."""

lazy import subprocess
lazy import sys

from cyclopts import App
lazy from expression import Result

lazy from eng.python.fitout.faults import Fault, Faults

# --- [OPERATIONS] -----------------------------------------------------------------------


def _reported(result: Result[int, Faults] | None) -> int:
    """Returns zero, a command's exit status, a lone failed child process's status, or 1 for other faults, writing each fault to stderr."""
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

app = App(help=__doc__, result_action=(_reported, "sys_exit"))
app.command("eng.python.fitout.assembly:pack")
app.command("eng.python.fitout.install:install")
app.command("eng.python.fitout.assembly:site")
app.command("eng.python.fitout.evaluate:run")


if __name__ == "__main__":
    app()
