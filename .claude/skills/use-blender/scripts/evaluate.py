"""Run Blender with given arguments in a fresh `.artifacts/blender/evaluate/<name>/` tree of relocated user folders."""

from itertools import chain
import os
from pathlib import Path
import shutil
import subprocess
import sys
from typing import Annotated

from cyclopts import App, Parameter
from expression import Result
from expression.extra.result import catch

# --- [TYPES] ----------------------------------------------------------------------------

type Failure = OSError | subprocess.CalledProcessError

# --- [CONSTANTS] ------------------------------------------------------------------------

ROOT = Path(__file__).resolve().parents[4]
FOLDERS = {
    "BLENDER_USER_RESOURCES": ".",
    "BLENDER_USER_CONFIG": "config",
    "BLENDER_USER_SCRIPTS": "scripts",
    "BLENDER_USER_EXTENSIONS": "extensions",
    "BLENDER_USER_DATAFILES": "datafiles",
    "TMPDIR": "tmp",
}

# --- [OPERATIONS] -----------------------------------------------------------------------


def tree(name: str) -> Result[Path, Failure]:
    """Fresh evaluation folder with every `FOLDERS` folder created before launch, Blender resolving a missing one to the next source, the user's profile last."""

    def made() -> Path:
        folder = ROOT / ".artifacts" / "blender" / "evaluate" / name
        shutil.rmtree(folder, ignore_errors=True)
        for child in FOLDERS.values():
            (folder / child).mkdir(parents=True, exist_ok=True)
        return folder

    return catch(made, exception=OSError)()


def recorded(folder: Path, done: subprocess.CompletedProcess[bytes]) -> subprocess.CompletedProcess[bytes]:
    """Process whose output is appended to the tree's `blender.log`."""
    with (folder / "blender.log").open("ab") as log:
        log.write(done.stdout)
    return done


def launched(folder: Path, command: tuple[str, ...]) -> Result[subprocess.CompletedProcess[bytes], Failure]:
    """One windowless child process from the repository root with every `FOLDERS` variable naming its folder in the tree, its merged output appended to `blender.log`."""

    def run() -> subprocess.CompletedProcess[bytes]:
        environment = os.environ | {variable: str(folder / child) for variable, child in FOLDERS.items()}
        return recorded(folder, subprocess.run(command, cwd=ROOT, env=environment, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, check=False))

    return catch(run, exception=OSError)()


def opened(folder: Path, blender: str, arguments: tuple[str, ...]) -> Result[subprocess.CompletedProcess[bytes], Failure]:
    """Start one GUI through `open` as a launchd job of its own and wait for it to quit.

    LaunchServices ties an application instance to every process of the launchd job it started in, and Dock's quit of that instance signals them all.

    Args:
        folder: Tree whose `FOLDERS` variables reach the GUI as `--env` values.
        blender: Blender executable inside its application bundle.
        arguments: Blender arguments.

    Returns:
        `open`'s exit code with its output and the GUI's merged output, appended to `blender.log`, or the launch or read error.
    """

    def run() -> subprocess.CompletedProcess[bytes]:
        output = folder / "gui.log"
        output.touch()
        variables = chain.from_iterable(("--env", f"{variable}={folder / child}") for variable, child in FOLDERS.items())
        command = ("/usr/bin/open", "-n", "-g", "-W", "-a", blender.removesuffix("/Contents/MacOS/Blender"), *variables, "--stdout", str(output), "--stderr", str(output), "--args", *arguments)
        started = subprocess.run(command, cwd=ROOT, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, check=False)
        return recorded(folder, subprocess.CompletedProcess(started.args, started.returncode, started.stdout + output.read_bytes()))

    return catch(run, exception=OSError)()


def provisioned(folder: Path, blender: str) -> Result[Path, Failure]:
    """Tree whose user `scripts/modules` folder, first on Blender's `sys.path` and untouched by its extension wheel sync, holds the distributions `fitout pack` bundles as wheels."""
    return (
        launched(folder, (sys.executable, "-m", "eng.python.fitout", "site", "--blender", blender, str(folder / FOLDERS["BLENDER_USER_SCRIPTS"] / "modules")))
        .bind(lambda done: catch(done.check_returncode, exception=subprocess.CalledProcessError)())
        .map(lambda _: folder)
    )


def run(name: str, /, *arguments: str, blender: Annotated[str, Parameter(env_var="BLENDER_PATH")] = "blender") -> Result[int, Failure]:
    """Run one Blender process with the arguments in a fresh tree, a headless child with `--background` and a GUI through `open` without it.

    Args:
        name: Tree folder under `.artifacts/blender/evaluate/`.
        arguments: Blender arguments, after `--` when one opens with `-`.
        blender: Blender executable, supplied by BLENDER_PATH.

    Returns:
        Exit code of a headless Blender or of a GUI's `open` with the merged output printed, or the filesystem, launch, or package install error.
    """

    def shown(done: subprocess.CompletedProcess[bytes]) -> int:
        sys.stdout.buffer.write(done.stdout)
        return done.returncode

    def started(folder: Path) -> Result[subprocess.CompletedProcess[bytes], Failure]:
        return launched(folder, (blender, *arguments)) if "--background" in arguments or "-b" in arguments else opened(folder, blender, arguments)

    return tree(name).bind(lambda folder: provisioned(folder, blender)).bind(started).map(shown)


# --- [COMPOSITION] ----------------------------------------------------------------------


def exited(result: Result[int, Failure]) -> int:
    """Exit code of a command, its error printed to stderr as exit code 1."""
    match result:
        case Result(tag="ok", ok=code):
            return code
        case Result(error=error):
            sys.stderr.write(f"{error!r}\n")
            return 1


app = App(help=__doc__, result_action=(exited, "sys_exit"))
app.command(run)

if __name__ == "__main__":
    app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["app"]
