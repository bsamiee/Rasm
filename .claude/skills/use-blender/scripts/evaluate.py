"""Runs Blender in fresh `.artifacts/blender/evaluate/<name>/` tree of relocated user folders."""

import importlib
from itertools import takewhile
import os
from pathlib import Path
import shutil
import subprocess
import sys
from typing import Annotated

from cyclopts import App, Parameter
from cyclopts.types import ResolvedExistingFile
from expression import Result
from expression.extra.result import catch

# --- [TYPES] ----------------------------------------------------------------------------

type Failure = OSError | subprocess.CalledProcessError

# --- [CONSTANTS] ------------------------------------------------------------------------

ROOT = Path(__file__).resolve().parents[4]
SCRIPTS = ROOT / ".cache" / "blender" / "scripts"
MODULE_ROOT = Path(next(iter(importlib.import_module("blender").__path__))).parent
FOLDERS = {
    "BLENDER_USER_RESOURCES": ".",
    "BLENDER_USER_CONFIG": "config",
    "BLENDER_USER_SCRIPTS": "scripts",
    "BLENDER_USER_EXTENSIONS": "extensions",
    "BLENDER_USER_DATAFILES": "datafiles",
    "TMPDIR": ".",
}

# --- [OPERATIONS] -----------------------------------------------------------------------


def tree(name: str) -> Result[Path, Failure]:
    """Fresh evaluation folder holding every `FOLDERS` folder, Blender resolving a missing one to user profile."""

    def made() -> Path:
        folder = ROOT / ".artifacts" / "blender" / "evaluate" / name
        shutil.rmtree(folder, ignore_errors=True)
        for child in FOLDERS.values():
            (folder / child).mkdir(parents=True, exist_ok=True)
        return folder

    return catch(made, exception=OSError)()


def recorded(folder: Path, done: subprocess.CompletedProcess[bytes]) -> subprocess.CompletedProcess[bytes]:
    """`done` with output appended to `blender.log`."""
    with (folder / "blender.log").open("ab") as log:
        log.write(done.stdout)
    return done


def launched(folder: Path, environment: dict[str, str], command: tuple[str | Path, ...]) -> Result[subprocess.CompletedProcess[bytes], Failure]:
    """Windowless child run from repository root with `environment` over process environment, output appended to `blender.log`."""
    return catch(lambda: recorded(folder, subprocess.run(command, cwd=ROOT, env=os.environ | environment, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, check=False)), exception=OSError)()


def opened(folder: Path, environment: dict[str, str], blender: Path, arguments: tuple[str, ...]) -> Result[subprocess.CompletedProcess[bytes], Failure]:
    """Launches GUI through `open` as its own launchd job, waiting for quit.

    Dock quit signals every process of an instance's launchd job.

    Args:
        folder: Tree holding GUI merged output.
        environment: Variables for GUI as `--env` values, `open` passing no other.
        blender: Blender executable inside its application bundle.
        arguments: Blender arguments.

    Returns:
        `open` exit code with `open` and GUI output appended to `blender.log`, or launch or read error.
    """
    output = folder / "gui.log"
    pairs = (*("--env", f"{variable}={value}") for variable, value in environment.items())
    command = ("/usr/bin/open", "-n", "-g", "-W", "-a", str(blender.parents[2]), *pairs, "--stdout", str(output), "--stderr", str(output), "--args", *arguments)

    def gui() -> subprocess.CompletedProcess[bytes]:
        output.touch()
        started = subprocess.run(command, cwd=ROOT, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, check=False)
        return recorded(folder, subprocess.CompletedProcess(started.args, started.returncode, started.stdout + output.read_bytes()))

    return catch(gui, exception=OSError)()


def provisioned(folder: Path, environment: dict[str, str], blender: Path) -> Result[dict[str, str], Failure]:
    """Environment with `PYTHONPATH` at the folder the `blender` library imports from and `BLENDER_SYSTEM_SCRIPTS` at shared folder, `fitout site` syncing its `modules` to `fitout pack` distributions."""
    return (
        launched(folder, environment, (sys.executable, "-m", "eng.python.fitout", "site", "--blender", blender, SCRIPTS / "modules"))
        .bind(lambda done: catch(done.check_returncode, exception=subprocess.CalledProcessError)())
        .map(lambda _: environment | {"BLENDER_SYSTEM_SCRIPTS": str(SCRIPTS), "PYTHONPATH": str(MODULE_ROOT)})
    )


def run(
    name: str, /, *arguments: str, blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")], pycache: Annotated[Path, Parameter(env_var="PYTHONPYCACHEPREFIX")]
) -> Result[int, Failure]:
    """Runs Blender with `--python-use-system-env` in fresh tree, headless on `-b` or `--background` before any `--`, else as GUI through `open`.

    Args:
        name: Tree folder under `.artifacts/blender/evaluate/`.
        arguments: Blender arguments, after `--` when one opens with `-`.
        blender: Blender executable from `BLENDER_PATH`.
        pycache: Bytecode cache folder from `PYTHONPYCACHEPREFIX`.

    Returns:
        Blender or `open` exit code with output printed, or filesystem, launch, or package sync error.
    """

    def shown(done: subprocess.CompletedProcess[bytes]) -> int:
        sys.stdout.buffer.write(done.stdout)
        return done.returncode

    def started(folder: Path) -> Result[subprocess.CompletedProcess[bytes], Failure]:
        environment = {variable: str(folder / child) for variable, child in FOLDERS.items()} | {"PYTHONPYCACHEPREFIX": str(pycache)}
        given = ("--python-use-system-env", *arguments)
        background = not {"-b", "--background"}.isdisjoint(takewhile(lambda argument: argument != "--", arguments))
        return provisioned(folder, environment, blender).bind(lambda provided: launched(folder, provided, (blender, *given)) if background else opened(folder, provided, blender, given))

    return tree(name).bind(started).map(shown)


# --- [COMPOSITION] ----------------------------------------------------------------------


def exited(result: Result[int, Failure]) -> int:
    """Command exit code, error printed to stderr as exit code 1."""
    match result:
        case Result(tag="ok", ok=code):
            return code
        case Result(error=error):
            sys.stderr.write(f"{error}\n")
            return 1


app = App(help=__doc__, result_action=(exited, "sys_exit"))
app.command(run)

if __name__ == "__main__":
    app()
