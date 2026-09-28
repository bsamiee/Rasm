"""Shared content the host stages before a Blender run: the look-development environment image and the asset library a background Blender builds."""

from pathlib import Path
import shutil

import anyio
import httpx

from interface.blender.packages import Access, background, Unfetched, Unresolved
from interface.host import Change, downloaded, Error
from interface.render import LOOK_DEVELOPMENT
from interface.report import ABSENT

# --- [OPERATIONS] -----------------------------------------------------------------------


async def look_development(client: httpx.AsyncClient) -> tuple[Change, ...] | Unresolved:
    """Look-development environment image extracted from its ambientCG set while absent, with a change row when extracted."""
    folder = anyio.Path(LOOK_DEVELOPMENT.parent)
    if await anyio.Path(LOOK_DEVELOPMENT).is_file():
        return ()
    url = str(httpx.URL("https://ambientcg.com/get", params={"file": f"{folder.name}.zip"}))
    async with anyio.TemporaryDirectory() as temporary:
        match await downloaded(client, url, Path(temporary, f"{folder.name}.zip")):
            case Error(text=text):
                return Unfetched(LOOK_DEVELOPMENT.name, url, text)
            case archive:
                await anyio.to_thread.run_sync(shutil.unpack_archive, archive, folder)
    return (Change(f"library.{LOOK_DEVELOPMENT.name}", ABSENT, url),)


async def assembled(executable: Path) -> tuple[Change, ...] | Unresolved:
    """Change row of the shared asset library a background Blender rebuilds offline when its source stamp moved, or the failed command."""
    return await background(executable, Access.OFFLINE, "interface.blender.library", "built", tuple[Change, ...])


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["assembled", "look_development"]
