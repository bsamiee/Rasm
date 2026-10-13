"""Run Blender in a fresh `.artifacts/blender/evaluate/<name>/` tree of relocated user folders: checks, an extension cycle, or given arguments."""

import ast
from collections.abc import Callable
from functools import partial, reduce
from itertools import chain
import os
from pathlib import Path
import shutil
import subprocess
import sys
from typing import Annotated
import zipfile

from cyclopts import App, Parameter
from cyclopts.types import ResolvedExistingFile
from expression import Error, Ok, Result
from expression.extra.result import catch
import msgspec

# --- [TYPES] ----------------------------------------------------------------------------

type Row = Header | Outcome | Crash
type Failure = OSError | subprocess.CalledProcessError | KeyError | zipfile.BadZipFile | msgspec.DecodeError

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

# --- [MODELS] ---------------------------------------------------------------------------


class Header(msgspec.Struct, frozen=True, tag="blender", tag_field="kind"):
    """Blender build and argv of a process, with the check ids it declared."""

    version: str
    build_hash: str
    argv: list[str]
    checks: list[str]


class Outcome(msgspec.Struct, frozen=True, tag="check", tag_field="kind"):
    """One check's result, with whether its flip input changed the verdict."""

    id: str
    returned: object
    raised: str | None
    warnings: list[str]
    printed: list[str]
    verdict: bool
    flipped: bool | None


class Crash(msgspec.Struct, frozen=True, tag="crash", tag_field="kind"):
    """Check a Blender process stopped in, with its nonzero exit code and the crash report a signal leaves."""

    id: str
    code: int
    report: str | None


class Manifest(msgspec.Struct, frozen=True):
    """Extension manifest field naming its package."""

    id: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def tree(name: str) -> Result[Path, Failure]:
    """Fresh evaluation folder with every `FOLDERS` folder created before launch, Blender resolving a missing one to the next source, the user's profile last."""

    def made() -> Path:
        folder = ROOT / ".artifacts" / "blender" / "evaluate" / name
        shutil.rmtree(folder, ignore_errors=True)
        for child in FOLDERS.values():
            (folder / child).mkdir(parents=True, exist_ok=True)
        (folder / "rows.txt").touch()
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
    """Tree whose user `scripts/modules` folder, first on Blender's `sys.path` and untouched by its extension wheel sync, holds the runtime closure `fitout pack` bundles with the first-party package whole."""
    return (
        launched(folder, (sys.executable, "-m", "eng.python.fitout", "site", "--blender", blender, str(folder / FOLDERS["BLENDER_USER_SCRIPTS"] / "modules")))
        .bind(lambda done: catch(done.check_returncode, exception=subprocess.CalledProcessError)())
        .map(lambda _: folder)
    )


def script(folder: Path, mode: str, *arguments: str) -> tuple[str, ...]:
    """Background run of `in_blender.py`, the mode, rows file, and mode arguments after `--`."""
    return ("--background", "--python-exit-code", "1", "--python", str(Path(__file__).with_name("blender") / "in_blender.py"), "--", mode, str(folder / "rows.txt"), *arguments)


def rows(folder: Path) -> list[Row]:
    """Rows every Blender process of the folder wrote, in order."""
    return [msgspec.convert(ast.literal_eval(line), type=Row) for line in (folder / "rows.txt").read_text(encoding="utf-8").splitlines()]


def ran(folder: Path, blender: str, arguments: tuple[str, ...], planned: list[str]) -> Result[list[Row], Failure]:
    """Rows one process wrote, closed by a crash row for its first unfinished planned check on a nonzero exit, the header naming the plan when none is given."""
    before = len(rows(folder))

    def closed(done: subprocess.CompletedProcess[bytes]) -> list[Row]:
        new = rows(folder)[before:]
        finished = {row.id for row in new if isinstance(row, Outcome)}
        pending = next((identifier for identifier in planned or next((row.checks for row in new if isinstance(row, Header)), []) if identifier not in finished), "startup")
        reports = (str(found.rename(folder / f"{pending}.crash.txt")) for found in (folder / FOLDERS["TMPDIR"]).glob("*.crash.txt"))
        return [*new, Crash(id=pending, code=done.returncode, report=next(reports, None))] if done.returncode else new

    return launched(folder, (blender, *arguments)).map(closed)


def reported(folder: Path, found: list[Row]) -> int:
    """Write `rows.jsonl` with the first header, print one summary line, and return 1 on any failed or crashed check."""
    body = [row for row in found if not isinstance(row, Header)]
    (folder / "rows.jsonl").write_bytes(b"".join(msgspec.json.encode(row) + b"\n" for row in ([row for row in found if isinstance(row, Header)][:1] + body)))
    passed = sum(isinstance(row, Outcome) and row.verdict for row in body)
    crashed = sum(isinstance(row, Crash) for row in body)
    sys.stdout.write(f"{folder.name}: {passed} passed, {len(body) - passed - crashed} failed, {crashed} crashed, {folder / 'rows.jsonl'}\n")
    return int(passed < len(body) or not body)


def checks(path: ResolvedExistingFile, /, *, isolate: bool = False, falsify: bool = False, blender: Annotated[str, Parameter(env_var="BLENDER_PATH")] = "blender") -> Result[int, Failure]:
    """Run the checks a file declares as `@check("<id>", expect=<value or exception class>, flip=<input>)`, `check` needing no import.

    Args:
        path: Checks file, each check a function taking its `flip` input as one optional parameter.
        isolate: Run each check in its own Blender process, a crash ending only that check.
        falsify: Run each check again with its `flip` input and fail a check whose verdict holds.
        blender: Blender executable, supplied by BLENDER_PATH.

    Returns:
        Exit code 1 on any failed or crashed check, or the filesystem, launch, or package install error.
    """
    mode = "falsify" if falsify else "plain"

    def isolated(folder: Path, head: list[Row]) -> Result[list[Row], Failure]:
        return reduce(
            lambda done, identifier: done.bind(lambda found: ran(folder, blender, script(folder, "run", str(path), mode, identifier), [identifier]).map(lambda new: [*found, *new])),
            next((row.checks for row in head if isinstance(row, Header)), []),
            Result[list[Row], Failure](ok=head),
        )

    def everything(folder: Path) -> Result[int, Failure]:
        found = ran(folder, blender, script(folder, "declare", str(path)), ["declare"]).bind(partial(isolated, folder)) if isolate else ran(folder, blender, script(folder, "run", str(path), mode), [])
        return found.map(partial(reported, folder))

    return tree(path.stem).bind(lambda folder: provisioned(folder, blender)).bind(everything)


def extension(archive: ResolvedExistingFile, /, *, blender: Annotated[str, Parameter(env_var="BLENDER_PATH")] = "blender") -> Result[int, Failure]:
    """Validate, install, and cycle an extension archive through disable, enable, and unregister in a fresh tree.

    Args:
        archive: Extension ZIP file holding `blender_manifest.toml`.
        blender: Blender executable, supplied by BLENDER_PATH.

    Returns:
        Exit code 1 on any failed step, or the archive, filesystem, or launch error.
    """
    repository = "user_default"

    def identity() -> Result[str, Failure]:
        try:
            with zipfile.ZipFile(archive) as bundle:
                return Ok(msgspec.toml.decode(bundle.read("blender_manifest.toml"), type=Manifest).id)
        except (OSError, KeyError, zipfile.BadZipFile, msgspec.DecodeError) as error:
            return Error(error)

    def command(folder: Path, step: str, arguments: tuple[str, ...], held: Callable[[int], bool]) -> Result[Outcome, Failure]:
        return launched(folder, (blender, "--command", "extension", *arguments)).map(
            lambda done: Outcome(
                id=f"extension-{step}", returned=done.returncode, raised=None, warnings=[], printed=done.stdout.decode("utf-8", "replace").splitlines(), verdict=held(done.returncode), flipped=None
            )
        )

    def everything(identifier: str, folder: Path) -> Result[int, Failure]:
        def cycled(validated: Outcome, install: Outcome) -> Result[int, Failure]:
            return ran(folder, blender, script(folder, "cycle", f"bl_ext.{repository}.{identifier}"), []).map(lambda cycle: reported(folder, [validated, install, *cycle]))

        def installed(validated: Outcome) -> Result[int, Failure]:
            return command(
                folder, "install", ("install-file", "--repo", repository, "--enable", str(archive)), lambda _: (folder / FOLDERS["BLENDER_USER_EXTENSIONS"] / repository / identifier).is_dir()
            ).bind(lambda install: cycled(validated, install) if install.verdict else Ok(reported(folder, [validated, install])))

        return command(folder, "validate", ("validate", str(archive)), lambda code: code == 0).bind(lambda validated: installed(validated) if validated.verdict else Ok(reported(folder, [validated])))

    return identity().bind(lambda identifier: tree(identifier).bind(partial(everything, identifier)))


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
app.command(checks)
app.command(extension)
app.command(run)

if __name__ == "__main__":
    app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["app"]
