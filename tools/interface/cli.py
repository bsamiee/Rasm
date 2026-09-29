"""Applies the interface to each application whose folder holds an `apply.py`, or to the named one, upgrades the packages each `packages.py` stages under `upgrade`, and prints every outcome as one JSON document."""

from collections.abc import Awaitable, Callable, Iterable
from functools import partial
from importlib import import_module
from itertools import chain
import os
from pathlib import Path
import subprocess
import sys
import traceback
from typing import Annotated, Final

import anyio
import cyclopts
import httpx
from mcp import McpError
import msgspec
import psutil

from interface.host import Applied, Failed, Host
from interface.units import Units

# --- [CONSTANTS] ------------------------------------------------------------------------

FOLDER: Final = Path(__file__).resolve().parent
ROOT: Final = FOLDER.parents[1]

# --- [OPERATIONS] -----------------------------------------------------------------------


def leaves(group: BaseExceptionGroup[BaseException]) -> tuple[BaseException, ...]:
    """Every exception under the group, nested groups flattened."""
    return tuple(chain.from_iterable(leaves(error) if isinstance(error, BaseExceptionGroup) else (error,) for error in group.exceptions))


async def outcomes(name: str, entry: Callable[[Host], Awaitable[tuple[Applied | Failed, ...]]], units: Units, client: httpx.AsyncClient) -> tuple[Applied | Failed, ...]:
    """Outcomes of one application's entry, a process, IO, transport, or decode failure failing it with the error and each failed command's error output."""
    try:
        results = await entry(Host(name, ROOT, os.environ, units, client))
    except* (OSError, subprocess.CalledProcessError, psutil.Error, msgspec.MsgspecError, McpError, httpx.HTTPError) as group:
        errors = tuple(line.strip() for line in traceback.format_exception_only(group, show_group=True))
        stderr = tuple(line for error in leaves(group) if isinstance(error, subprocess.CalledProcessError) for line in error.stderr.decode().splitlines())
        results = (Failed(name, errors, stderr=stderr),)
    return results


async def run(
    entries: Iterable[tuple[str, Callable[[Host], Awaitable[tuple[Applied | Failed, ...]]]]], *, units: Annotated[Units, cyclopts.Parameter(accepts_keys=False, n_tokens=1)] = Units.IMPERIAL
) -> bool:
    """Run the applications' entries concurrently in the unit system over one HTTP client, print their outcomes as one JSON document, and return whether each applied, the exit code."""
    async with httpx.AsyncClient(follow_redirects=True, limits=httpx.Limits(max_connections=4), timeout=httpx.Timeout(5.0, pool=None)) as client:
        results = tuple(result for outcome in await anyio.gather(*(outcomes(name, entry, units, client) for name, entry in entries)) for result in outcome)
    sys.stdout.buffer.write(msgspec.json.format(msgspec.json.encode(results, enc_hook=str), indent=1) + b"\n")
    return all(isinstance(each, Applied) for each in results)


# --- [COMPOSITION] ----------------------------------------------------------------------

APPLICATIONS: Final = frozendict({path.parent.name: import_module(f"{__package__}.{path.parent.name}.apply") for path in sorted(FOLDER.glob("*/apply.py"))})
UPGRADES: Final = frozendict({path.parent.name: import_module(f"{__package__}.{path.parent.name}.packages") for path in sorted(FOLDER.glob("*/packages.py"))})
app = cyclopts.App(help=__doc__)
app.default(partial(run, tuple((name, module.apply) for name, module in APPLICATIONS.items())))
for name, module in APPLICATIONS.items():
    app.command(partial(run, ((name, module.apply),)), name=name, help=module.__doc__)
upgrade = cyclopts.App(name="upgrade", help="Upgrades the packages each application stages, or the named application's, to the newest build their sources publish.")
upgrade.default(partial(run, tuple((name, module.upgrade) for name, module in UPGRADES.items())))
for name, module in UPGRADES.items():
    upgrade.command(partial(run, ((name, module.upgrade),)), name=name, help=module.upgrade.__doc__)
app.command(upgrade)
if __name__ == "__main__":
    app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["app"]
