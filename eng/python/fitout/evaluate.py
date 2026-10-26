"""Runs Blender in a fresh user base under `.artifacts/blender/evaluate/<name>/` with repository modules."""

lazy from itertools import takewhile
lazy import os
lazy from pathlib import Path
lazy import shutil
lazy import subprocess
lazy import sys
lazy from typing import Annotated

lazy from cyclopts import Parameter
lazy from cyclopts.types import ResolvedExistingFile
lazy from expression import Error, Ok, Result
lazy from expression.extra.result import catch

lazy from eng.python.fitout.assembly import site
lazy from eng.python.fitout.faults import Fault

# --- [OPERATIONS] -----------------------------------------------------------------------


def run(
    name: str, /, *arguments: str, blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")], pycache: Annotated[Path, Parameter(env_var="PYTHONPYCACHEPREFIX")]
) -> Result[int, Fault]:
    """Runs Blender with `--python-use-system-env` in a fresh user base, headless on `-b` or `--background` before any `--`, else as GUI.

    Blender resolves every user folder under `BLENDER_USER_RESOURCES`, lookups failing with no profile fallback.
    macOS GUIs start through `open` as their own launchd job, which Dock quit signals whole.

    Args:
        name: Folder under `.artifacts/blender/evaluate/`.
        arguments: Blender arguments, after `--` when one opens with `-`.
        blender: Blender executable.
        pycache: Bytecode cache folder.

    Returns:
        Blender or `open` exit code with output printed, or fault of a failed folder reset or package sync.
    """
    folder = Path(".artifacts/blender/evaluate", name).resolve()
    scripts = Path(".cache/blender/scripts").resolve()
    environment = frozendict(
        BLENDER_USER_RESOURCES=str(folder), TMPDIR=str(folder), PYTHONPYCACHEPREFIX=str(pycache), BLENDER_SYSTEM_SCRIPTS=str(scripts), PYTHONPATH=str(Path("libs/python").resolve())
    )

    def launched(_: int) -> int:
        given = ("--python-use-system-env", *arguments)
        log = folder / "blender.log"
        background = not {"-b", "--background"}.isdisjoint(takewhile(lambda argument: argument != "--", arguments))
        command: tuple[str | Path, ...] = (
            (blender, *given)
            if background or sys.platform != "darwin"
            else (
                "/usr/bin/open",
                "-n",
                "-g",
                "-W",
                "-a",
                blender.parents[2],
                *(*("--env", f"{variable}={value}") for variable, value in environment.items()),
                "--stdout",
                log,
                "--stderr",
                log,
                "--args",
                *given,
            )
        )
        with log.open("ab") as sink:
            done = subprocess.run(command, env=os.environ | environment, stdout=sink, stderr=subprocess.STDOUT, check=False)
        sys.stdout.buffer.write(log.read_bytes())
        return done.returncode

    return (
        catch(shutil.rmtree, exception=OSError)(folder)
        .or_else_with(lambda error: Ok(None) if isinstance(error, FileNotFoundError) else Error(error))
        .bind(lambda _: catch(folder.mkdir, exception=OSError)(parents=True))
        .map_error(Fault.of)
        .bind(lambda _: site(scripts / "modules", blender=blender))
        .map(launched)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["run"]
