"""Encodes icons, packs, installs, and publishes Blender extensions, syncs locked Python requirements and host constraints, and runs Blender in isolated profiles."""

lazy from pathlib import Path
lazy import subprocess
lazy import sys

from cyclopts import App
lazy from expression import Result

lazy from blender.records.faults import Fault, Outcome

# --- [OPERATIONS] -----------------------------------------------------------------------


def _reported(result: Outcome[int | Path] | None) -> int:
    """Returns command or failed child status, zero for help or a printed user base, and 1 for other faults."""
    match result:
        case None:
            return 0
        case Result(tag="ok", ok=int(status)):
            return status
        case Result(tag="ok", ok=Path() as base):
            sys.stdout.write(f"{base}\n")
            return 0
        case Result(error=faults):
            for fault in faults:
                for data, stream in ((error.stdout, sys.stdout.buffer), (error.stderr, sys.stderr.buffer)) if isinstance(error := fault.cause, subprocess.CalledProcessError) else ():
                    stream.writelines(filter(None, (data,)))
                sys.stderr.write(f"{fault.message}\n")
            match faults:
                case (Fault(cause=subprocess.CalledProcessError(returncode=status)),):
                    return status
                case _:
                    return 1


# --- [COMPOSITION] ----------------------------------------------------------------------

app = App(help=__doc__, result_action=(_reported, "sys_exit"))
app.command("eng.python.blender.icons:icons")
app.command("eng.python.blender.assembly:pack")
app.command("eng.python.blender.install:install")
app.command("eng.python.blender.assembly:publish")
app.command("eng.python.blender.assembly:site")
app.command("eng.python.blender.assembly:upgrade")
app.command("eng.python.blender.evaluate:run")


if __name__ == "__main__":
    app()
