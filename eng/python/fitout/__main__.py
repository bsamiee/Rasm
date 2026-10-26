"""Packs and installs Blender extensions, syncs their wheels into Blender import folder, and runs Blender in fresh user bases."""

lazy import sys

from cyclopts import App
lazy from expression import Result

lazy from eng.python.fitout.faults import Fault

# --- [OPERATIONS] -----------------------------------------------------------------------


def _reported(result: Result[int, Fault]) -> int:
    """Returns exit status, writing any fault to stderr."""
    match result:
        case Result(tag="ok", ok=status):
            return status
        case Result(error=fault):
            sys.stderr.write(f"{fault.message}\n")
            return fault.status


# --- [COMPOSITION] ----------------------------------------------------------------------

app = App(help=__doc__, result_action=(_reported, "sys_exit"))
app.command("eng.python.fitout.assembly:pack")
app.command("eng.python.fitout.install:install")
app.command("eng.python.fitout.assembly:site")
app.command("eng.python.fitout.evaluate:run")


if __name__ == "__main__":
    app()
