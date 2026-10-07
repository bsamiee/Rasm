"""Applies the interface to Blender with the packages `packages.toml` declares at their staged builds, the look-development image, and the asset shelf catalogs."""

import anyio

from interface.blender import packages
from interface.blender.rows import Archived
from interface.blender.session import session
from interface.host import Error, Host, Outcome, outcome

# --- [COMPOSITION] ----------------------------------------------------------------------


async def apply(host: Host) -> tuple[Outcome, ...]:
    """Blender's one outcome over the staging rows and, when every package, the extension, and the image staged, the session's rows."""
    if isinstance(prepared := await packages.installation(host), Error):
        return (outcome(host.app, (prepared,)),)
    variables, bundle, declared, installation = prepared
    (rows, lines), packed, looked = await anyio.gather(
        packages.resolution(host, installation, declared, lambda package, _: packages.held(host, installation, package)), packages.packaged(host, bundle, installation), packages.look_development(host)
    )
    staged = (*lines, *looked, *((packed,) if isinstance(packed, Error) else ()))
    match packed:
        case (packages.Manifest() as manifest, Archived() as extension) if not any(isinstance(row, Error) for row in staged):
            return (outcome(host.app, (*staged, *await session(host, variables.blender_mcp_port, bundle, manifest, installation.essentials, (*rows, extension)))),)
        case _:
            return (outcome(host.app, staged),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["apply"]
