"""Runs Blender with an isolated profile."""

import os
import sys
lazy from functools import partial
lazy from pathlib import Path
lazy import shutil
lazy import subprocess

from expression.extra.result import catch
lazy import pathvalidate

lazy from blender.records.faults import Fault, Outcome, paired, Reason
lazy from blender.records.stores import committed, folder_made
lazy from eng.python.blender.host import Blender, headless, probed, root, site

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PROCESSES]


@catch(exception=OSError)
def _launched(log: Path, blender: Path, arguments: tuple[str, ...], variables: dict[str, str], /) -> int:
    """Runs Blender without activation and forwards its output."""
    windowless = headless(arguments)
    command: tuple[str | Path, ...]
    if sys.platform == "darwin" and not windowless:
        relayed = ("--python-use-system-env", "--python", str(Path(__file__).with_name("streams.py")), *arguments)
        command, environment = (
            ("watch-apps", "launch", blender.parents[2], "--new", "--wait", "--environment", *(f"{key}={value}" for key, value in (variables | {"PWD": str(Path.cwd())}).items()), "--", *relayed),
            None,
        )
    else:
        command = (blender, "--python-use-system-env", *(() if windowless else ("--no-window-focus",)), *arguments)
        environment = os.environ | variables
    with log.open("a+b") as sink:
        try:
            return subprocess.run(command, env=environment, stdout=sink, stderr=subprocess.STDOUT, check=False).returncode
        finally:
            sink.seek(0)
            shutil.copyfileobj(sink, sys.stdout.buffer)


# --- [COMMANDS]


def run(name: str, /, *arguments: str, blender: Blender, package: str | None = None, groups: tuple[str, ...] = ()) -> Outcome[int]:
    """Runs Blender in a fresh profile; a background flag before `--` selects headless mode.

    Blender resolves every user folder under `BLENDER_USER_RESOURCES`, lookups failing with no profile fallback.
    Windows open without activation, macOS's through `watch-apps launch --wait`, which returns the runner's exit status.

    Args:
        name: Folder under `.artifacts/blender/evaluate/`.
        arguments: Blender arguments, after `--` when one opens with `-`.
        blender: Blender executable.
        package: Workspace member selecting locked runtime requirements, workspace root when omitted.
        groups: Additional uv dependency group selectors.

    Returns:
        Blender's exit status with output printed, or fault of a failed folder reset, package sync, or launch.
    """

    def launched(folder: Path, userbase: Path) -> Outcome[int]:
        variables = {"BLENDER_USER_RESOURCES": str(folder), "TMPDIR": str(folder), "PYTHONUSERBASE": str(userbase)} | (
            {"PYTHONPYCACHEPREFIX": sys.pycache_prefix} if sys.pycache_prefix is not None else {}
        )
        return _launched(folder / "blender.log", blender, arguments, variables).map_error(partial(Fault.of, Reason.FAILED, str(blender)))

    return (
        catch(pathvalidate.validate_filename, exception=pathvalidate.ValidationError)(name, platform="auto", additional_reserved_names=(os.curdir, os.pardir), max_len=0)
        .map_error(partial(Fault.of, Reason.INVALID, name))
        .bind(lambda _: root())
        .bind(lambda base: paired(committed(base / ".artifacts/blender/evaluate" / name, folder_made), probed(base, blender).bind(partial(site, base, package=package, groups=groups))))
        .bind(lambda held: launched(*held))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["run"]
