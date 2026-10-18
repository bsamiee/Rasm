"""Run Blender with given arguments in a fresh `.artifacts/blender/evaluate/<name>/` tree of relocated user folders."""

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
import msgspec

# --- [TYPES] ----------------------------------------------------------------------------

type Failure = OSError | subprocess.CalledProcessError

# --- [CONSTANTS] ------------------------------------------------------------------------

ROOT = Path(__file__).resolve().parents[4]
SCRIPTS = ROOT / ".cache" / "blender" / "scripts"
FOLDERS = {
    "BLENDER_USER_RESOURCES": ".",
    "BLENDER_USER_CONFIG": "config",
    "BLENDER_USER_SCRIPTS": "scripts",
    "BLENDER_USER_EXTENSIONS": "extensions",
    "BLENDER_USER_DATAFILES": "datafiles",
    "TMPDIR": "tmp",
}

# --- [MODELS] ---------------------------------------------------------------------------


class _Backend(msgspec.Struct, frozen=True, rename="kebab"):
    module_root: str


class _Uv(msgspec.Struct, frozen=True, rename="kebab"):
    build_backend: _Backend


class _Tool(msgspec.Struct, frozen=True):
    uv: _Uv


class _Project(msgspec.Struct, frozen=True):
    tool: _Tool


# --- [OPERATIONS] -----------------------------------------------------------------------


def tree(name: str) -> Result[Path, Failure]:
    """Fresh evaluation folder with every `FOLDERS` folder created before launch, Blender resolving a missing one to the next source, the user's profile last."""

    def made() -> Path:
        folder = ROOT / ".artifacts" / "blender" / "evaluate" / name
        shutil.rmtree(folder, ignore_errors=True)
        for child in FOLDERS.values():
            (folder / child).mkdir(parents=True)
        return folder

    return catch(made, exception=OSError)()


def recorded(folder: Path, done: subprocess.CompletedProcess[bytes]) -> subprocess.CompletedProcess[bytes]:
    """Process whose output is appended to the tree's `blender.log`."""
    with (folder / "blender.log").open("ab") as log:
        log.write(done.stdout)
    return done


def launched(folder: Path, environment: dict[str, str], command: tuple[str | Path, ...]) -> Result[subprocess.CompletedProcess[bytes], Failure]:
    """One windowless child process from the repository root with the given variables over the process environment, its merged output appended to `blender.log`."""
    return catch(lambda: recorded(folder, subprocess.run(command, cwd=ROOT, env=os.environ | environment, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, check=False)), exception=OSError)()


def opened(folder: Path, environment: dict[str, str], blender: Path, arguments: tuple[str, ...]) -> Result[subprocess.CompletedProcess[bytes], Failure]:
    """Start one GUI through `open` as a launchd job of its own and wait for it to quit.

    LaunchServices ties an application instance to every process of the launchd job it started in, and Dock's quit of that instance signals them all.

    Args:
        folder: Tree holding the GUI's merged output.
        environment: Variables reaching the GUI as `--env` values, `open` passing no other variable of the caller.
        blender: Blender executable inside its application bundle.
        arguments: Blender arguments.

    Returns:
        `open`'s exit code with its output and the GUI's merged output, appended to `blender.log`, or the launch or read error.
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
    """Environment with `PYTHONPATH` at the checkout's module root and `BLENDER_SYSTEM_SCRIPTS` at the shared folder whose `modules`, first on Blender's `sys.path` and untouched by its extension wheel sync, `fitout site` syncs to the distributions `fitout pack` bundles as wheels."""
    return (
        launched(folder, environment, (sys.executable, "-m", "eng.python.fitout", "site", "--blender", blender, SCRIPTS / "modules"))
        .bind(lambda done: catch(done.check_returncode, exception=subprocess.CalledProcessError)())
        .bind(lambda _: catch((ROOT / "pyproject.toml").read_bytes, exception=OSError)())
        .map(lambda project: environment | {"BLENDER_SYSTEM_SCRIPTS": str(SCRIPTS), "PYTHONPATH": str(ROOT / msgspec.toml.decode(project, type=_Project).tool.uv.build_backend.module_root)})
    )


def run(
    name: str, /, *arguments: str, blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")], pycache: Annotated[Path, Parameter(env_var="PYTHONPYCACHEPREFIX")]
) -> Result[int, Failure]:
    """Run one Blender process with `--python-use-system-env` ahead of the arguments in a fresh tree, a headless child with `-b` or `--background` before any `--` and a GUI through `open` without it.

    Args:
        name: Tree folder under `.artifacts/blender/evaluate/`.
        arguments: Blender arguments, after `--` when one opens with `-`.
        blender: Blender executable, supplied by BLENDER_PATH.
        pycache: Bytecode cache folder for Blender's Python, supplied by PYTHONPYCACHEPREFIX.

    Returns:
        Exit code of a headless Blender or of a GUI's `open` with the merged output printed, or the filesystem, launch, or package sync error.
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
    """Exit code of a command, its error printed to stderr as exit code 1."""
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

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["app"]
