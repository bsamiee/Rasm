"""Packs and installs Blender extensions, syncs their wheels into Blender import folder, and runs Blender in fresh user bases."""

lazy import sys

from cyclopts import App
lazy from expression import Result

lazy from eng.python.fitout.faults import Faults

# --- [OPERATIONS] -----------------------------------------------------------------------


def _reported(result: Result[int, Faults]) -> int:
    """Returns exit status, a lone fault's status or 1 for several, writing each fault to stderr."""
    match result.tag:
        case "ok":
            return result.ok
        case "error":
            sys.stderr.writelines(f"{fault.message}\n" for fault in result.error)
            return result.error[0].status if len(result.error) == 1 else 1


# --- [COMPOSITION] ----------------------------------------------------------------------

app = App(help=__doc__, result_action=(_reported, "sys_exit"))
app.command("eng.python.fitout.assembly:pack")
app.command("eng.python.fitout.install:install")
app.command("eng.python.fitout.assembly:site")
app.command("eng.python.fitout.evaluate:run")


if __name__ == "__main__":
    app()
