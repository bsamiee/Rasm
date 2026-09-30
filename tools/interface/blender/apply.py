"""Applies the interface to Blender with the packages `packages.toml` declares at their staged builds, the look-development image, and the asset shelf catalogs."""

import anyio

from interface.blender import packages
from interface.blender.packages import Manifest
from interface.blender.rows import Local
from interface.blender.session import session
from interface.host import Applied, environment, Error, Failed, Host, outcome

# --- [COMPOSITION] ----------------------------------------------------------------------


async def apply(host: Host) -> tuple[Applied | Failed, ...]:
    """Blender's one outcome over the staging rows and, when every package, the extension, and the image staged, the session's rows."""
    if isinstance(variables := environment(packages.BlenderEnvironment, host.environ), Error):
        return (outcome(host.app, (variables,)),)
    bundle, declaration = await anyio.gather(packages.bundled(variables), packages.declared())
    if isinstance(installation := await packages.installation(host, bundle, declaration.repositories), Error):
        return (outcome(host.app, (installation,)),)
    (rows, lines), packed, looked = await anyio.gather(
        packages.resolution(host, installation, declaration.packages, upgrading=False), packages.packaged(host, bundle, installation), packages.look_development(host)
    )
    staged = (*lines, *looked, *((packed,) if isinstance(packed, Error) else ()))
    match packed:
        case (Manifest() as manifest, Local() as extension) if not any(isinstance(row, Error) for row in staged):
            return (outcome(host.app, (*staged, *await session(host, variables.blender_mcp_port, bundle, manifest, installation.essentials, (*rows, extension)))),)
        case _:
            return (outcome(host.app, staged),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["apply"]
