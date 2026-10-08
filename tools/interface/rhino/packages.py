"""Rhino packages `packages.toml` declares, staged under the cache from each row's source and converged through `yak` while Rhino is closed."""

from collections.abc import Mapping, Sequence
from enum import StrEnum
from io import BytesIO
from pathlib import Path, PurePosixPath
import re
from typing import Final
import zipfile

import anyio
import httpx2
from lxml import etree
import msgspec

from interface.host import downloaded, executed, fetched, Host, Outcome, outcome, Result
from interface.report import ABSENT, Change, digest, Error, Header, subscript
from interface.rhino.session import Rhino
from interface.rhino.window import RibbonTab

# --- [TYPES] ----------------------------------------------------------------------------


class Origin(StrEnum):
    """Package source in Rhino's bundle or the Yak server."""

    BUNDLED = "bundled"
    PUBLISHED = "published"


# --- [CONSTANTS] ------------------------------------------------------------------------

TABLE: Final = "packages"

# --- [MODELS] ---------------------------------------------------------------------------


class Package(msgspec.Struct, frozen=True):
    """`packages.toml` row by yak id, the origin of its archive, and its commands by ribbon tab."""

    id: str
    source: Origin = Origin.PUBLISHED
    commands: dict[RibbonTab, tuple[str, ...]] = {}

    def archive(self, cache: Path) -> Path | None:
        """Staged yak archive under the cache, None for a package Rhino bundles."""
        return None if self.source is Origin.BUNDLED else cache / f"{self.id}.yak"


class Build(msgspec.Struct, frozen=True):
    """Yak package name and version a `manifest.yml` states, with the build's file entries by archive path."""

    name: str
    version: str
    entries: Mapping[str, bytes]

    @property
    def stamp(self) -> str:
        """Report text of the build, its version and the digest of its entries."""
        return f"{self.version} {digest(msgspec.msgpack.encode(self.entries, order='deterministic'))}"


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [ARCHIVES]
def manifest(content: bytes) -> Build:
    """Build a yak archive's bytes hold, named and versioned by its `manifest.yml`."""
    with zipfile.ZipFile(BytesIO(content)) as archive:
        entries = {entry.filename: archive.read(entry) for entry in archive.infolist() if not entry.is_dir()}
    return msgspec.convert({**msgspec.yaml.decode(entries["manifest.yml"], type=dict[str, object]), "entries": entries}, Build)


def concealed(source: Path) -> bytes:
    """Archive bytes with toolbar containers hidden, preserving entry order, XML comments, and namespace prefixes."""

    def hidden(content: bytes) -> bytes:
        root = etree.fromstring(content)
        for info in root.iterfind("tool_bar_groups/tool_bar_group/dock_bar_info"):
            info.set("visible", "False")
        return etree.tostring(root, xml_declaration=True, encoding="utf-8")

    buffer = BytesIO()
    with zipfile.ZipFile(source) as held, zipfile.ZipFile(buffer, "w") as made:
        for entry in held.infolist():
            made.writestr(entry, hidden(held.read(entry)) if PurePosixPath(entry.filename).suffix == ".rui" else held.read(entry))
    return buffer.getvalue()


async def stored(path: Path) -> bytes | None:
    """Bytes of an archive, None while the path holds none."""
    try:
        return await anyio.Path(path).read_bytes()
    except FileNotFoundError:
        return None


# --- [SOURCES]
async def declared() -> tuple[Package, ...]:
    """Package declarations in ribbon order."""

    class Packages(msgspec.Struct, frozen=True):
        packages: tuple[Package, ...]

    return msgspec.toml.decode(await anyio.Path(__file__).with_name("packages.toml").read_bytes(), type=Packages).packages


async def published(client: httpx2.AsyncClient, rhino: Rhino, identity: str, folder: Path) -> Result[Path]:
    """Archive in the folder downloaded from the newest compatible Yak distribution in server order, using Yak's non-strict compatibility rule."""

    class Distribution(msgspec.Struct, frozen=True):
        rhino_version: str
        platform: str
        url: str

    class Version(msgspec.Struct, frozen=True):
        distributions: tuple[Distribution, ...]

    def loads(distribution: Distribution) -> bool:
        """Whether this Rhino on macOS loads the distribution: any platform but Windows, and any Rhino or a release at or below this one."""
        tag = re.fullmatch(r"rh(\d+)(?:_(\d+))?|any", distribution.rhino_version)
        return distribution.platform != "win" and tag is not None and (tag[1] is None or (int(tag[1]), int(tag[2] or 0)) <= rhino.release)

    match await fetched(client, f"https://yak.rhino3d.com/versions/{identity}", msgspec.json.Decoder(tuple[Version, ...]).decode):
        case Error() as failed:
            return failed
        case versions:
            match next((each.url for version in versions for each in version.distributions if loads(each)), None):
                case None:
                    return Error(f"Yak server publishes no {identity} version Rhino {rhino.bundle.version} on macOS loads")
                case url:
                    return await downloaded(client, url, folder / f"{identity}.yak")


async def staged(host: Host, rhino: Rhino, package: Package) -> tuple[Result[Change], ...]:
    """Change of the package's archive under the cache written from its source with its toolbar containers hidden, none while the cache holds the same bytes or Rhino bundles the package, or the error of a source yielding none."""
    if (target := package.archive(host.cache)) is None:
        return ()
    async with anyio.TemporaryDirectory() as temporary:
        match await published(host.client, rhino, package.id, Path(temporary)):
            case Error() as failed:
                return (failed,)
            case Path() as archive:
                content = await anyio.to_thread.run_sync(concealed, archive)
    if (before := await stored(target)) == content:
        return ()
    part = anyio.Path(target.with_name(f"{target.name}.part"))
    await part.write_bytes(content)
    await part.replace(target)
    return (Change(subscript(TABLE, package.id), ABSENT if before is None else manifest(before).stamp, manifest(content).stamp),)


# --- [INSTALLS]
async def converged(environ: Mapping[str, str], rhino: Rhino, archive: Path) -> tuple[Result[Change], ...]:
    """Change of the package the archive holds, reinstalled while the installed build of its name differs in version or entries, the removal kept beside the error of an install failing after it, or the error of an absent archive."""
    match await stored(archive):
        case None:
            return (Error(f"{archive} holds no yak archive"),)
        case content:
            build = manifest(content)
    label, version = subscript(TABLE, build.name), rhino.installed.get(build.name)
    match version:
        case None:
            before = ABSENT
        case str():
            folder = anyio.Path(rhino.directory, build.name, version)
            if (held := Build(build.name, version, {entry: await path.read_bytes() async for path in folder.rglob("*") if (entry := path.relative_to(folder).as_posix()) in build.entries})) == build:
                return ()
            if isinstance(failed := await executed((str(rhino.yak), "uninstall", build.name), environ), Error):
                return (failed,)
            before = held.stamp
    added = await executed((str(rhino.yak), "install", str(archive)), environ)
    return (*(() if version is None else (Change(label, before, ABSENT),)), added) if isinstance(added, Error) else (Change(label, before, build.stamp),)


async def install(host: Host, rhino: Rhino, packages: Sequence[Package]) -> tuple[Result[Change], ...]:
    """Changes and errors of declared packages installed from their staged archives."""
    rows = [row for package in packages if (archive := package.archive(host.cache)) is not None for row in await converged(host.environ, rhino, archive)]
    return tuple(rows)


# --- [COMPOSITION] ----------------------------------------------------------------------


async def upgrade(host: Host) -> tuple[Outcome]:
    """Stages the newest build of each package `packages.toml` declares under the cache for the Rhino bundle `RHINO_PATH` names."""
    match await Rhino.resolve(host.environ):
        case Error() as failed:
            return (outcome(host.app, (failed,)),)
        case Rhino() as rhino:
            await anyio.Path(host.cache).mkdir(parents=True, exist_ok=True)
            results = await anyio.gather(*(staged(host, rhino, package) for package in await declared()))
            return (outcome(host.app, (Header(rhino.bundle.version, str(host.cache)), *(line for lines in results for line in lines))),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Build", "Package", "converged", "declared", "install", "manifest", "upgrade"]
