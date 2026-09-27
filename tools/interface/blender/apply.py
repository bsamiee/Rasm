"""Applies the interface to Blender with the packages `packages.toml` declares, the shared content, and the asset shelf catalog tabs, and upgrades the staged packages."""

from functools import partial
from pathlib import Path
from typing import Final

import anyio
import httpx
import msgspec

from interface.blender.content import assembled, look_development, surveyed
from interface.blender.packages import Access, decoded, resolution
from interface.blender.sessions import relaunch
from interface.host import application, Applied, Failed, Host, Outcome
from interface.render import Survey

# --- [CONSTANTS] ------------------------------------------------------------------------

CONNECTIONS: Final = httpx.Limits(max_connections=4)
WAIT: Final = httpx.Timeout(5.0, pool=None)

# --- [MODELS] ---------------------------------------------------------------------------


class Environment(msgspec.Struct, frozen=True, rename="upper"):
    """Variables of the server row naming the executable and the bridge port."""

    blender_path: str
    blender_mcp_port: int


class Server(msgspec.Struct, frozen=True):
    """Blender server row of `.mcp.json`."""

    env: Environment


class Servers(msgspec.Struct, frozen=True):
    """Server rows of `.mcp.json` the Blender run reads."""

    blender: Server


# --- [COMPOSITION] ----------------------------------------------------------------------


async def apply(host: Host) -> tuple[Outcome]:
    """Blender's outcome over the packages `packages.toml` declares at their staged builds, failed for an executable outside an application bundle or an unresolved package."""
    environment = msgspec.json.decode(host.servers, type=Servers, strict=False).blender.env
    executable = Path(environment.blender_path)
    declared = decoded(await anyio.Path(host.folder, host.app, "packages.toml").read_text(encoding="utf-8"))
    match await application(executable):
        case None:
            return (Failed(host.app, (f"{executable} is no application bundle's main executable",)),)
        case blender:
            async with httpx.AsyncClient(follow_redirects=True, limits=CONNECTIONS, timeout=WAIT) as client:
                content = (*(partial(surveyed, client, survey) for survey in Survey), partial(look_development, client), partial(assembled, executable))
                match await resolution(host, client, executable, declared, Access.OFFLINE, content):
                    case Failed() as failed:
                        return (failed,)
                    case catalog, rows, changes:
                        outcome = await relaunch(host, executable, environment.blender_mcp_port, blender, rows, catalog)
                        return (msgspec.structs.replace(outcome, changes=(*changes, *outcome.changes)),)


async def upgrade(host: Host) -> tuple[Outcome]:
    """Upgrades each package `packages.toml` declares to the newest build its source publishes."""
    executable = Path(msgspec.json.decode(host.servers, type=Servers, strict=False).blender.env.blender_path)
    declared = decoded(await anyio.Path(host.folder, host.app, "packages.toml").read_text(encoding="utf-8"))
    async with httpx.AsyncClient(follow_redirects=True, limits=CONNECTIONS, timeout=WAIT) as client:
        match await resolution(host, client, executable, declared, Access.ONLINE, ()):
            case Failed() as failed:
                return (failed,)
            case catalog, _, changes:
                return (Applied(host.app, catalog.version, catalog.settings, changes, (), {}),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["apply", "upgrade"]
