"""Shared content the host stages before a Blender run: the site survey rasters, the look-development environment image, and the asset library a background Blender builds."""

from math import ceil, log10
from pathlib import Path
import shutil

import anyio
import httpx
import pyproj
import tifffile

from interface.blender.packages import Access, background, downloaded, PART, Unfetched, Unresolved
from interface.host import Change
from interface.render import EPSG, LATITUDE, LONGITUDE, LOOK_DEVELOPMENT, Survey
from interface.report import ABSENT
from interface.units import Units

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [SITE]
def framed(count: int, left: float, top: float, pitch: float) -> tuple[int, float, float, float]:
    """Square raster's pixel count across with its left, top, and right edges at the metric resolution."""
    places = round(-log10(Units.METRIC.resolution))
    return count, round(left, places), round(top, places), round(left + count * pitch, places)


def georeferenced(path: Path) -> tuple[int, float, float, float] | None:
    """GeoTIFF frame from its width, tie point, and pixel scale, None while the file is absent."""
    if not path.is_file():
        return None
    with tifffile.TiffFile(path) as tiff:
        page = tiff.pages.first
        tie, scale = page.tags["ModelTiepointTag"].value, page.tags["ModelPixelScaleTag"].value
        return framed(page.shape[1], tie[3], tie[4], scale[0])


async def surveyed(client: httpx.AsyncClient, survey: Survey) -> tuple[Change, ...] | Unresolved:
    """Site survey raster over the imperial modeling extent in the site's UTM system, fetched while its frame differs from the declared one, with a change row when fetched."""
    extent, (x, y) = Units.IMPERIAL.extent, pyproj.Transformer.from_crs(4326, EPSG, always_xy=True).transform(LONGITUDE, LATITUDE)
    count, part = ceil(2 * extent / survey.pitch), anyio.Path(survey.path.with_suffix(PART))
    stated, held = framed(count, x - extent, y + extent, 2 * extent / count), await anyio.to_thread.run_sync(georeferenced, survey.path)
    if held == stated:
        return ()
    await part.parent.mkdir(parents=True, exist_ok=True)
    if isinstance(failed := await downloaded(client, survey.path.name, survey.address(f"{x - extent},{y - extent},{x + extent},{y + extent}", EPSG, count), part), Unfetched):
        return failed
    await part.replace(survey.path)
    return (Change(f"site.{survey.path.name}", ABSENT if held is None else str(held), str(stated)),)


# --- [LIBRARY]
async def look_development(client: httpx.AsyncClient) -> tuple[Change, ...] | Unresolved:
    """Look-development environment image extracted from its ambientCG set while absent, with a change row when extracted."""
    folder = anyio.Path(LOOK_DEVELOPMENT.parent)
    if await anyio.Path(LOOK_DEVELOPMENT).is_file():
        return ()
    url = str(httpx.URL("https://ambientcg.com/get", params={"file": f"{folder.name}.zip"}))
    async with anyio.TemporaryDirectory() as temporary:
        archive = anyio.Path(temporary, f"{folder.name}.zip")
        if isinstance(failed := await downloaded(client, LOOK_DEVELOPMENT.name, url, archive), Unfetched):
            return failed
        await anyio.to_thread.run_sync(shutil.unpack_archive, archive, folder)
    return (Change(f"library.{LOOK_DEVELOPMENT.name}", ABSENT, url),)


async def assembled(executable: Path) -> tuple[Change, ...] | Unresolved:
    """Change row of the shared asset library a background Blender rebuilds offline when its source stamp moved, or the failed command."""
    return await background(executable, Access.OFFLINE, "interface.blender.library", "built", tuple[Change, ...])


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["assembled", "look_development", "surveyed"]
