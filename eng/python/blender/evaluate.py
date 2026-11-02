"""Runs Blender in a fresh user base under `.artifacts/blender/evaluate/<name>/` with repository modules."""

lazy from functools import partial
lazy from itertools import chain, takewhile
lazy import os
lazy from pathlib import Path
lazy import subprocess
lazy import sys
lazy from typing import Annotated

lazy from cyclopts import Parameter
lazy from expression import Result
lazy from expression.extra.result import catch
lazy import msgspec

lazy from eng.python.blender.assembly import Blender, reset, site
lazy from eng.python.blender.faults import Fault, Faults, Reason

# --- [OPERATIONS] -----------------------------------------------------------------------


def instances(line: bytes, /) -> dict[str, object] | None:
    """Reads the instances of one `watch-apps` line keyed by process ID, None for a `system` or `fail` line."""
    held = msgspec.json.decode(line, type=dict[str, dict[str, object]])
    return next((states for key, states in held.items() if key != "system" and all(pid.isdigit() for pid in states)), None)


def ended(state: object, /) -> int | None:
    """Reads an instance's exit status from its state, 1 for a signal, an unread status, or an instance no longer listed, None while it runs."""
    match state:
        case {"exited": {"status": int(status)}}:
            return status
        case {"exited": dict()} | None:
            return 1
        case _:
            return None


def exited(app: Path, pid: str, /) -> int:
    """Reads `watch-apps watch` lines until the instance exits or leaves the listing, returning its exit status."""
    reading, writing = os.pipe()
    with subprocess.Popen(("watch-apps", "watch", app), stdin=subprocess.DEVNULL, stdout=writing) as watch, os.fdopen(reading, "rb") as lines:
        os.close(writing)
        status = next((code for line in lines if (states := instances(line)) is not None and (code := ended(states.get(pid))) is not None), 1)
        watch.terminate()
        return status


def run(name: str, /, *arguments: str, blender: Blender, pycache: Annotated[Path, Parameter(env_var="PYTHONPYCACHEPREFIX")]) -> Result[int, Faults]:
    """Runs Blender with `--python-use-system-env` in a fresh user base, headless on `-b` or `--background` before any `--`, else as GUI.

    Blender resolves every user folder under `BLENDER_USER_RESOURCES`, lookups failing with no profile fallback.
    macOS GUIs start as a new hidden instance through `watch-apps launch`, and the run returns once that instance exits, which the runner's own quit call ends.

    Args:
        name: Folder under `.artifacts/blender/evaluate/`.
        arguments: Blender arguments, after `--` when one opens with `-`.
        blender: Blender executable.
        pycache: Bytecode cache folder.

    Returns:
        Blender's exit status with output printed, or fault of a failed folder reset, package sync, or launch.
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
        written = f"import os; os.dup2(os.open({str(log)!r}, os.O_WRONLY | os.O_APPEND | os.O_CREAT), 1); os.dup2(1, 2)"
        variables = chain.from_iterable(("--environment", f"{name}={value}") for name, value in environment.items())
        relayed = chain.from_iterable(("--arguments", argument) for argument in (*given[:1], "--python-expr", written, *given[1:]))
        app = blender.parents[2]
        if direct:
            with log.open("ab") as sink:
                code = subprocess.run((blender, *given), env=os.environ | environment, stdout=sink, stderr=subprocess.STDOUT, check=False).returncode
        else:
            opened = subprocess.run(("watch-apps", "launch", app, "--new", *variables, *relayed), stdin=subprocess.DEVNULL, capture_output=True, check=False)
            with log.open("ab") as sink:
                sink.write(opened.stdout + opened.stderr)
            started = [pid for line in opened.stdout.splitlines() if (states := instances(line)) is not None for pid in states]
            code = exited(app, started[-1]) if opened.returncode == 0 and started else max(opened.returncode, 1)
        sys.stdout.buffer.write(log.read_bytes())
        return code

    return reset(folder, Path.mkdir).bind(lambda _: site(scripts / "modules", blender=blender)).bind(lambda _: launched().map_error(partial(Fault.of, Reason.PROCESS, blender)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["run"]
