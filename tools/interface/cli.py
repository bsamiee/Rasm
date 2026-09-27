"""Applies the interface to each application whose folder holds an `apply.py`, or to the named one, upgrades the packages an application stages under `upgrade`, and prints every outcome as one JSON document."""

from collections.abc import Awaitable, Callable, Iterable
from functools import partial
from importlib import import_module
import os
from pathlib import Path
import subprocess
import sys
import traceback
from types import MappingProxyType
from typing import Annotated, Final

import anyio
import cyclopts
import httpx
from mcp import McpError
import msgspec
import psutil

from interface.host import Applied, Failed, Host, Outcome
from interface.units import Units

# --- [TYPES] ----------------------------------------------------------------------------

type Entry = Callable[[Host], Awaitable[tuple[Outcome, ...]]]

# --- [CONSTANTS] ------------------------------------------------------------------------

FOLDER: Final = Path(__file__).resolve().parent
ROOT: Final = FOLDER.parents[1]

# --- [OPERATIONS] -----------------------------------------------------------------------


async def outcomes(name: str, entry: Entry, servers: msgspec.Raw, units: Units) -> tuple[Outcome, ...]:
    """Outcomes of one application's entry, a process, IO, transport, or decode failure failing it with the error."""
    try:
        results = await entry(Host(name, FOLDER, ROOT, servers, os.environ, units))
    except* (OSError, subprocess.CalledProcessError, psutil.Error, msgspec.MsgspecError, McpError, httpx.HTTPError) as group:
        results = (Failed(name, tuple(line.strip() for line in traceback.format_exception_only(group, show_group=True))),)
    return results


async def run(entries: Iterable[tuple[str, Entry]], *, units: Annotated[Units, cyclopts.Parameter(accepts_keys=False, n_tokens=1)] = Units.IMPERIAL) -> bool:
    """Run the applications' entries concurrently in the unit system, print their outcomes as one JSON document, and return whether each applied."""
    servers = msgspec.json.decode(await anyio.Path(ROOT, ".mcp.json").read_bytes(), type=dict[str, msgspec.Raw])["mcpServers"]
    handles: list[anyio.TaskHandle[tuple[Outcome, ...]]] = []
    async with anyio.create_task_group() as group:
        handles.extend(group.start_soon(outcomes, name, entry, servers, units) for name, entry in entries)
    results = tuple(result for handle in handles for result in handle.return_value)
    sys.stdout.buffer.write(msgspec.json.format(msgspec.json.encode(results), indent=1) + b"\n")
    return all(isinstance(each, Applied) for each in results)


# --- [COMPOSITION] ----------------------------------------------------------------------

APPLICATIONS: Final = MappingProxyType({path.parent.name: import_module(f"{__package__}.{path.parent.name}.apply") for path in sorted(FOLDER.glob("*/apply.py"))})
UPGRADES: Final = MappingProxyType({name: module for name, module in APPLICATIONS.items() if "upgrade" in module.__all__})
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
