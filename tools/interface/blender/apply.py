"""Applies the interface to Blender with the packages `packages.toml` declares, the shared content, and the asset shelf catalog tabs, and upgrades the staged packages."""

from functools import partial
from pathlib import Path

import anyio
import msgspec

from interface.blender.content import assembled, look_development
from interface.blender.packages import Access, decoded, Environment, resolution, upgrade
from interface.blender.sessions import relaunch
from interface.host import Applied, bundle, Failed, Host

# --- [COMPOSITION] ----------------------------------------------------------------------


async def apply(host: Host) -> tuple[Applied | Failed]:
    """Blender's outcome over the packages `packages.toml` declares at their staged builds, failed for an unresolved package."""
    environment = msgspec.convert(host.server("blender").env, Environment, strict=False)
    executable = Path(environment.blender_path)
    declared = decoded(await anyio.Path(Path(__file__).with_name("packages.toml")).read_text(encoding="utf-8"))
    blender = await bundle(executable.parents[2])
    match await resolution(host, executable, declared, Access.OFFLINE, (partial(look_development, host.client), partial(assembled, executable))):
        case Failed() as failed:
            return (failed,)
        case catalog, rows, changes:
            outcome = await relaunch(host, executable, environment.blender_mcp_port, blender, rows, catalog)
            return (msgspec.structs.replace(outcome, changes=(*changes, *outcome.changes)),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["apply", "upgrade"]
