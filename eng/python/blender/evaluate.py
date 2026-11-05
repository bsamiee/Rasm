"""Runs Blender with an isolated profile and locked Python requirements."""

lazy from functools import partial
lazy from itertools import chain, takewhile
lazy import os
lazy from pathlib import Path
lazy import shutil
lazy import subprocess
lazy import sys
lazy from typing import Annotated

lazy from cyclopts import Parameter
lazy from expression.extra.result import catch
lazy import pathvalidate

lazy from blender.records.faults import Fault, Outcome, Reason
lazy from blender.records.stores import committed, folder_made
lazy from eng.python.blender.assembly import BACKGROUND, Blender, site

# --- [OPERATIONS] -----------------------------------------------------------------------


def run(name: str, /, *arguments: str, blender: Blender, pycache: Annotated[Path, Parameter(env_var="PYTHONPYCACHEPREFIX")], package: str | None = None, groups: tuple[str, ...] = ()) -> Outcome[int]:
    """Runs Blender in a fresh profile; a background flag before `--` selects headless mode.

    Blender resolves every user folder under `BLENDER_USER_RESOURCES`, lookups failing with no profile fallback.
    macOS GUIs start hidden through `watch-apps launch --wait`, which returns the runner's exit status.

    Args:
        name: Folder under `.artifacts/blender/evaluate/`.
        arguments: Blender arguments, after `--` when one opens with `-`.
        blender: Blender executable.
        pycache: Bytecode cache folder.
        package: Workspace member selecting locked runtime requirements, workspace root when omitted.
        groups: Additional uv dependency group selectors.

    Returns:
        Blender's exit status with output printed, or fault of a failed folder reset, package sync, or launch.
    """
    folder = Path(".artifacts/blender/evaluate").resolve() / name

    @catch(exception=OSError)
    def launched(userbase: Path) -> int:
        environment = frozendict(BLENDER_USER_RESOURCES=str(folder), TMPDIR=str(folder), PYTHONPYCACHEPREFIX=str(pycache), PYTHONUSERBASE=str(userbase))
        log = folder / "blender.log"
        command: tuple[str | Path, ...]
        if sys.platform != "darwin" or not BACKGROUND.isdisjoint(takewhile(lambda argument: argument != "--", arguments)):
            command, process_environment = (blender, "--python-use-system-env", *arguments), os.environ | environment
        else:
            written = f"import os\nwith open({str(log)!r}, 'ab', buffering=0) as sink:\n    os.dup2(sink.fileno(), 1)\n    os.dup2(1, 2)\nos.chdir({str(Path.cwd())!r})"
            variables = chain.from_iterable(("--environment", f"{key}={value}") for key, value in environment.items())
            option = "--arguments"
            relayed = chain.from_iterable((f"{option}={argument}",) if argument else (option, argument) for argument in ("--python-use-system-env", "--python-expr", written, *arguments))
            command, process_environment = ("watch-apps", "launch", blender.parents[2], "--new", "--wait", *variables, *relayed), None
        with log.open("a+b") as sink:
            code = subprocess.run(command, env=process_environment, stdout=sink, stderr=subprocess.STDOUT, check=False).returncode
            sink.seek(0)
            shutil.copyfileobj(sink, sys.stdout.buffer)
            return code

    return (
        catch(pathvalidate.validate_filename, exception=pathvalidate.ValidationError)(name, platform="auto", additional_reserved_names=(os.curdir, os.pardir), max_len=0)
        .map_error(partial(Fault.of, Reason.INVALID, name))
        .bind(lambda _: committed(folder, folder_made))
        .bind(lambda _: site(blender=blender, package=package, groups=groups))
        .bind(lambda userbase: launched(userbase).map_error(partial(Fault.of, Reason.FAILED, str(blender))))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["run"]
