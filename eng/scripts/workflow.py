"""Run jobs of the CI workflow on the machine through act, the job containers and per-run volumes removed after the run."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Sequence
from pathlib import Path
import platform
from typing import Annotated

import anyio
import cyclopts
from expression import Error, Ok, Result
import msgspec
import structlog

from eng.scripts.provision import CommandFailed, exit_code, Failure, host_rid, message, repository_root, Rid, run

# --- [TYPES] ----------------------------------------------------------------------------


class Removed(msgspec.Struct, frozen=True, gc=False):
    """Job containers and per-run volumes removed after the act run."""

    containers: tuple[str, ...]
    volumes: tuple[str, ...]


# --- [CONSTANTS] ------------------------------------------------------------------------

_WORKFLOW = ".github/workflows/ci.yml"
_IMAGE = "ghcr.io/catthehacker/ubuntu:act-24.04"  # The setup action installs the toolchain, the image supplies sudo, apt, git, and a compiler
_LABELS = ("ubuntu-24.04", "ubuntu-24.04-arm")  # Runner labels of the Linux jobs, act skips a job with no image for its label
_SERVERS = Path(".cache") / "act"
_TOOLCACHE = "act-toolcache"  # Shared across runs, the one act volume the cleanup keeps
_PREFIX = "act-"  # Prefix of every container and volume act creates

_log = structlog.get_logger(__name__)
_app = cyclopts.App(name="workflow")

# --- [RUN] ------------------------------------------------------------------------------


def _architecture(host: Rid) -> str:
    """Return the Docker platform architecture of the host runtime identifier."""
    match host.partition("-")[2]:
        case "x64":
            return "amd64"
        case machine:
            return machine


async def _act(root: Path, host: Rid, args: Sequence[str]) -> Result[None, CommandFailed]:
    """Run act over the CI workflow on the host architecture with the repository's image and server paths, the arguments appended."""
    architecture = _architecture(host)
    command = [
        "act",
        "--rm",
        f"--workflows={_WORKFLOW}",
        f"--container-architecture=linux/{architecture}",
        "--container-daemon-socket=-",  # No job runs docker, and a socket under the home directory has no path inside the VM
        *(f"--platform={label}={_IMAGE}" for label in _LABELS),
        f"--action-cache-path={_SERVERS / 'actions'}",
        f"--cache-server-path={_SERVERS / 'cache'}",
        f"--artifact-server-path={_SERVERS / 'artifacts'}",
        *args,
    ]
    return (await run(command, root)).map(lambda _: None)


async def _names(root: Path, command: Sequence[str]) -> Result[list[str], CommandFailed]:
    """Return the act-prefixed names a docker listing prints, the shared tool cache excluded."""
    return (await run(command, root, capture=True)).map(
        lambda text: [name for name in text.split() if name.startswith(_PREFIX) and name != _TOOLCACHE]
    )


async def _clean(root: Path) -> Result[Removed, CommandFailed]:
    """Remove every act job container and per-run volume, act leaves both after an interrupted run or a failed job start."""
    match await _names(root, ["docker", "ps", "--all", "--format", "{{.Names}}"]):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=[]):
            containers: list[str] = []
        case Result(ok=containers):
            match await run(["docker", "rm", "--force", "--volumes", *containers], root, capture=True):
                case Result(tag="error", error=failure):
                    return Error(failure)
                case Result():
                    pass
    match await _names(root, ["docker", "volume", "ls", "--format", "{{.Name}}"]):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=[]):
            return Ok(Removed(tuple(containers), ()))
        case Result(ok=volumes):
            return (await run(["docker", "volume", "rm", *volumes], root, capture=True)).map(lambda _: Removed(tuple(containers), tuple(volumes)))


async def _workflow(start: Path, host_system: str, host_machine: str, args: Sequence[str]) -> Result[Removed, Failure]:
    """Run act against the Docker daemon the machine provides, and remove the job containers and per-run volumes on every exit."""
    match repository_root(start).map2(host_rid(host_system, host_machine), lambda root, host: (root, host)):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=(root, host)):
            pass
    try:
        outcome = await _act(root, host, args)
    finally:
        with anyio.CancelScope(shield=True):  # An interrupt cancels the run, the cleanup still completes
            cleaned = await _clean(root)
    return outcome.bind(lambda _: cleaned)


# --- [CLI] ------------------------------------------------------------------------------


def _report(removed: Removed) -> None:
    _log.info("workflow", removed_containers=list(removed.containers), removed_volumes=list(removed.volumes))


_app.result_action = (exit_code(_report, message), "sys_exit")


@_app.default
def main(*args: Annotated[str, cyclopts.Parameter(show=False, allow_leading_hyphen=True)]) -> Result[Removed, Failure]:
    """Run act over the CI workflow with the arguments, `--list` for the jobs and `--job=<job>` for one job.

    Args:
        args: Arguments act receives after the repository's flags.

    Returns:
        The removed containers and volumes, or the failure of the run or the cleanup.
    """
    return anyio.run(_workflow, Path(__file__).resolve(), platform.system(), platform.machine(), args)


if __name__ == "__main__":
    _app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Removed", "main"]
