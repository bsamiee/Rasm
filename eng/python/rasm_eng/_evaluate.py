"""Runs Blender with an isolated profile."""

import os
import sys
lazy from functools import partial
lazy from pathlib import Path
lazy import shutil
lazy import subprocess

from expression.extra.result import catch
lazy from cyclopts.types import ResolvedExistingDirectory
lazy import pathvalidate

lazy from rasm_eng._host import _headless, _probed, _root, _site, Blender, entered
lazy from rasm_records import committed, Fault, folder_made, Ok, Outcome, paired, Reason

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PROCESSES]


@catch(exception=OSError)
def _launched(log: Path, blender: Path, arguments: tuple[str, ...], variables: dict[str, str], /) -> int:
    """Runs Blender without activation and forwards its output."""
    windowless = _headless(arguments)
    command: tuple[str | Path, ...]
    if sys.platform == "darwin" and not windowless:
        relayed = ("--python-use-system-env", "--python-expr", entered(Path(__file__).with_name("_streams.py"), log, Path.cwd()), *arguments)
        command, environment = (
            ("watch-apps", "launch", blender.parents[2], "--new", "--wait", "--environment", *(f"{key}={value}" for key, value in variables.items()), "--arguments", *relayed),
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


def run(name: str, /, *arguments: str, blender: Blender, project: ResolvedExistingDirectory | None = None, groups: tuple[str, ...] = ()) -> Outcome[int]:
    """Runs Blender in a fresh profile; a background flag before `--` selects headless mode.

    Blender resolves every user folder under `BLENDER_USER_RESOURCES`, lookups failing with no profile fallback.
    Windows open without activation, macOS's through `watch-apps launch --wait`, which returns the runner's exit status.

    Args:
        name: Folder under `.artifacts/blender/evaluate/`.
        arguments: Blender arguments, after `--` when one opens with `-`.
        blender: Blender executable.
        project: uv project whose locked closure fills the user base, Blender's bundled interpreter alone when omitted.
        groups: Additional uv dependency group selectors of that project.

    Returns:
        Blender's exit status with output printed, or fault of a failed folder reset, package sync, or launch.
    """

    def launched(folder: Path, userbase: Path | None) -> Outcome[int]:
        variables = (
            {"BLENDER_USER_RESOURCES": str(folder), "TMPDIR": str(folder)}
            | ({"PYTHONUSERBASE": str(userbase)} if userbase is not None else {})
            | ({"PYTHONPYCACHEPREFIX": sys.pycache_prefix} if sys.pycache_prefix is not None else {})
        )
        return _launched(folder / "blender.log", blender, arguments, variables).map_error(partial(Fault.of, Reason.FAILED, str(blender)))

    return (
        catch(pathvalidate.validate_filename, exception=pathvalidate.ValidationError)(name, platform="auto", additional_reserved_names=(os.curdir, os.pardir), max_len=0)
        .map_error(partial(Fault.of, Reason.INVALID, name))
        .bind(lambda _: _root())
        .bind(
            lambda base: paired(
                committed(base / ".artifacts/blender/evaluate" / name, folder_made),
                Ok(None) if project is None else _probed(base, blender).bind(lambda host: _site(base, host, project, groups=groups)),
            )
        )
        .bind(lambda held: launched(*held))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["run"]
