"""Runs Blender in a fresh user base under `.artifacts/blender/evaluate/<name>/` with repository modules."""

lazy from functools import partial
lazy from itertools import takewhile
lazy import os
lazy from pathlib import Path
lazy import subprocess
lazy import sys
lazy from typing import Annotated

lazy from cyclopts import Parameter
lazy from cyclopts.types import ResolvedExistingFile
lazy from expression import Result
lazy from expression.extra.result import catch

lazy from eng.python.fitout.assembly import reset, site
lazy from eng.python.fitout.faults import Fault, Faults, Reason

# --- [OPERATIONS] -----------------------------------------------------------------------


def run(
    name: str, /, *arguments: str, blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")], pycache: Annotated[Path, Parameter(env_var="PYTHONPYCACHEPREFIX")]
) -> Result[int, Faults]:
    """Runs Blender with `--python-use-system-env` in a fresh user base, headless on `-b` or `--background` before any `--`, else as GUI.

    Blender resolves every user folder under `BLENDER_USER_RESOURCES`, lookups failing with no profile fallback.
    macOS GUIs start through `open` as their own launchd job, which Dock quit signals whole.

    Args:
        name: Folder under `.artifacts/blender/evaluate/`.
        arguments: Blender arguments, after `--` when one opens with `-`.
        blender: Blender executable.
        pycache: Bytecode cache folder.

    Returns:
        Blender or `open` exit code with output printed, or fault of a failed folder reset, package sync, or launch.
    """
    folder = Path(".artifacts/blender/evaluate", name).resolve()
    scripts = Path(".cache/blender/scripts").resolve()
    environment = frozendict(
        BLENDER_USER_RESOURCES=str(folder), TMPDIR=str(folder), PYTHONPYCACHEPREFIX=str(pycache), BLENDER_SYSTEM_SCRIPTS=str(scripts), PYTHONPATH=str(Path("libs/python").resolve())
    )

    @catch(exception=OSError)
    def launched() -> int:
        given = ("--python-use-system-env", *arguments)
        log = folder / "blender.log"
        direct = sys.platform != "darwin" or not {"-b", "--background"}.isdisjoint(takewhile(lambda argument: argument != "--", arguments))
        variables = (*("--env", f"{name}={value}") for name, value in environment.items())
        opened: tuple[str | Path, ...] = ("/usr/bin/open", "-n", "-g", "-W", "-a", blender.parents[2], *variables, "--stdout", log, "--stderr", log, "--args")
        with log.open("ab") as sink:
            done = subprocess.run((blender, *given) if direct else (*opened, *given), env=os.environ | environment, stdout=sink, stderr=subprocess.STDOUT, check=False)
        sys.stdout.buffer.write(log.read_bytes())
        return done.returncode

    return reset(folder, Path.mkdir).bind(lambda _: site(scripts / "modules", blender=blender)).bind(lambda _: launched().map_error(partial(Fault.of, Reason.PROCESS, blender)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["run"]
