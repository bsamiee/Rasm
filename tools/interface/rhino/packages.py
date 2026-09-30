"""Rhino packages `packages.toml` declares, staged under the cache from each row's source and converged through `yak` with every Rhino quit."""

from collections.abc import Iterable, Mapping, Sequence
from io import BytesIO
from pathlib import Path, PurePosixPath
import re
from typing import Final
import zipfile

import anyio
import httpx
from lxml import etree
import msgspec

from interface.host import Applied, Change, downloaded, Error, executed, Failed, fetched, Header, Host, outcome
from interface.report import ABSENT, digest, subscript
from interface.rhino.session import Rhino
from interface.rhino.window import RibbonTab

# --- [CONSTANTS] ------------------------------------------------------------------------

TABLE: Final = "packages"

# --- [MODELS] ---------------------------------------------------------------------------


class Source(msgspec.Struct, frozen=True):
    """GitHub repository whose newest workflow run on a branch uploading a named artifact holds the package, `{major}` standing for Rhino's major version."""

    repository: str
    branch: str
    artifact: str


class Package(msgspec.Struct, frozen=True):
    """`packages.toml` row by yak id, staged from its workflow artifact, its project's `pack` output, or the Yak server, with its commands by ribbon tab."""

    id: str
    source: Source | None = None
    project: str | None = None
    commands: dict[RibbonTab, tuple[str, ...]] = {}

    def archive(self, cache: Path) -> Path:
        """Staged yak package of the row under the cache folder."""
        return cache / f"{self.id}.yak"


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [ARCHIVES]
def manifest(content: bytes) -> tuple[str, dict[str, bytes]]:
    """Version a yak package's `manifest.yml` states and its file entries by name."""

    class Manifest(msgspec.Struct, frozen=True):
        version: str

    with zipfile.ZipFile(BytesIO(content)) as archive:
        return msgspec.yaml.decode(archive.read("manifest.yml"), type=Manifest).version, {entry.filename: archive.read(entry) for entry in archive.infolist() if not entry.is_dir()}


def stamped(version: str, entries: Mapping[str, bytes]) -> str:
    """Report text of a package build, its version and the digest of its entries in name order."""
    return f"{version} {digest(b''.join(entries[name] for name in sorted(entries)))}"


def concealed(source: Path) -> bytes:
    """Yak package of the source's entries in order, each `.rui` toolbar file with every group's container opening hidden and its comments and namespace prefixes kept."""

    def hidden(content: bytes) -> bytes:
        root = etree.fromstring(content)
        for info in root.iterfind("tool_bar_groups/tool_bar_group/dock_bar_info"):
            info.set("visible", str(False))
        return etree.tostring(root, xml_declaration=True, encoding="utf-8")

    buffer = BytesIO()
    with zipfile.ZipFile(source) as held, zipfile.ZipFile(buffer, "w") as made:
        for entry in held.infolist():
            made.writestr(entry, hidden(held.read(entry)) if PurePosixPath(entry.filename).suffix == ".rui" else held.read(entry))
    return buffer.getvalue()


def installed_entries(rhino: Rhino, identity: str, names: Iterable[str]) -> dict[str, bytes]:
    """Bytes of each named entry the folder of the version `yak list` names installed holds, files beside them ignored, none for a package it does not name."""
    match rhino.installed.get(identity):
        case str(version):
            folder = rhino.directory / identity / version
            return {name: path.read_bytes() for path in folder.rglob("*") if (name := path.relative_to(folder).as_posix()) in names}
        case None:
            return {}


async def stored(path: Path) -> bytes | None:
    """Bytes of a staged archive, None while the cache holds none."""
    try:
        return await anyio.Path(path).read_bytes()
    except FileNotFoundError:
        return None


# --- [SOURCES]
async def declared() -> tuple[Package, ...]:
    """Package rows `packages.toml` declares in ribbon order."""

    class Packages(msgspec.Struct, frozen=True):
        packages: tuple[Package, ...]

    return msgspec.toml.decode(await anyio.Path(__file__).with_name("packages.toml").read_bytes(), type=Packages).packages


async def packaged(folder: Path) -> Path | Error:
    """Yak package the folder holds, or the count it holds when that is not one."""
    match [Path(path) async for path in anyio.Path(folder).glob("*.yak")]:
        case [archive]:
            return archive
        case found:
            return Error(f"{folder} holds {len(found)} yak packages in place of one")


async def built(host: Host, rhino: Rhino, source: Source, folder: Path) -> Path | Error:
    """Yak package the newest unexpired upload of the source's artifact from a run on its branch holds, downloaded into the folder, or the reason none downloads."""
    major, _ = rhino.release
    branch, artifact = source.branch.format(major=major), source.artifact.format(major=major)
    newest = f'[.artifacts[] | select((.expired | not) and .workflow_run.head_branch == "{branch}")] | max_by(.created_at) | .workflow_run.id // empty'
    listed = await executed(("gh", "api", "-X", "GET", f"repos/{source.repository}/actions/artifacts", "-f", f"name={artifact}", "-f", "per_page=100", "--jq", newest), host.environ)
    match listed.split() if isinstance(listed, bytes) else listed:
        case Error() as failed:
            return failed
        case [run]:
            saved = await executed(("gh", "run", "download", run.decode(), "-R", source.repository, "-n", artifact, "-D", str(folder)), host.environ)
            return saved if isinstance(saved, Error) else await packaged(folder)
        case _:
            return Error(f"{source.repository} holds no unexpired {artifact} artifact from a run on {branch}")


async def published(client: httpx.AsyncClient, rhino: Rhino, identity: str) -> str | Error:
    """Address of the first distribution this Rhino on macOS loads, in server order, of the newest Yak server version holding one, as yak's non-strict compatibility test picks it, or the reason none exists."""

    class Distribution(msgspec.Struct, frozen=True):
        rhino_version: str
        platform: str
        url: str

    class Version(msgspec.Struct, frozen=True):
        distributions: tuple[Distribution, ...]

    def loads(distribution: Distribution) -> bool:
        """Whether this Rhino on macOS loads the distribution: any platform but Windows, and any Rhino or a release at or below this one."""
        match distribution.platform, re.fullmatch(r"rh(\d+)(?:_(\d+))?|any", distribution.rhino_version):
            case "win", _:
                return False
            case _, None:
                return False
            case _, tag:
                return tag[1] is None or (int(tag[1]), int(tag[2] or 0)) <= rhino.release

    match await fetched(client, f"https://yak.rhino3d.com/versions/{identity}", tuple[Version, ...]):
        case Error() as failed:
            return failed
        case versions:
            return next(
                (each.url for version in versions for each in version.distributions if loads(each)), Error(f"Yak server publishes no {identity} version Rhino {rhino.bundle.version} on macOS loads")
            )


async def staged(host: Host, rhino: Rhino, package: Package) -> tuple[Change | Error, ...]:
    """Report rows of the package's archive under the cache written from its source with its toolbar files concealed, none while the staged archive holds the same bytes, and the error while its source yields none."""
    async with anyio.TemporaryDirectory() as temporary:
        match package:
            case Package(source=Source() as source):
                fetched = await built(host, rhino, source, Path(temporary))
            case Package(project=str() as project):
                fetched = await packaged(host.root / ".artifacts" / "rhino" / project)
            case Package(id=identity):
                match await published(host.client, rhino, identity):
                    case Error() as failed:
                        fetched = failed
                    case url:
                        fetched = await downloaded(host.client, url, Path(temporary, f"{identity}.yak"))
        match fetched:
            case Error() as failed:
                return (failed,)
            case Path() as archive:
                content = await anyio.to_thread.run_sync(concealed, archive)
    archive = package.archive(host.cache)
    if (before := await stored(archive)) == content:
        return ()
    part = anyio.Path(archive.with_name(f"{archive.name}.part"))
    await part.write_bytes(content)
    await part.replace(archive)
    return (Change(subscript(TABLE, package.id), ABSENT if before is None else stamped(*manifest(before)), stamped(*manifest(content))),)


# --- [INSTALLS]
async def converged(rhino: Rhino, identity: str, archive: Path, content: bytes) -> tuple[Change, ...]:
    """Change of the package reinstalled from its staged archive holding the content while the installed build differs from it, none while it matches."""
    version, entries = manifest(content)
    if (held := await anyio.to_thread.run_sync(installed_entries, rhino, identity, entries)) == entries:
        return ()
    await anyio.run_process([rhino.yak, "uninstall", identity])
    await anyio.run_process([rhino.yak, "install", archive])
    return (Change(subscript(TABLE, identity), ABSENT if identity not in rhino.installed else stamped(rhino.installed[identity], held), stamped(version, entries)),)


async def install(host: Host, rhino: Rhino, packages: Sequence[Package]) -> tuple[Change, ...] | Error:
    """Changes of each declared package converged on its staged archive in declared order and of each other installed package uninstalled, or an error naming each declared package the cache holds no archive of."""
    archives = {package.id: package.archive(host.cache) for package in packages}
    contents = {identity: content for identity, archive in archives.items() if (content := await stored(archive)) is not None}
    match [identity for identity in archives if identity not in contents]:
        case []:
            changes = [change for identity, content in contents.items() for change in await converged(rhino, identity, archives[identity], content)]
            if strays := tuple(identity for identity in rhino.installed if identity not in archives):
                await anyio.run_process([rhino.yak, "uninstall", *strays])
            return (*changes, *(Change(subscript(TABLE, identity), rhino.installed[identity], ABSENT) for identity in strays))
        case missing:
            return Error(f"{host.cache} holds no staged archive of {', '.join(missing)}")


# --- [COMPOSITION] ----------------------------------------------------------------------


async def upgrade(host: Host) -> tuple[Applied | Failed]:
    """Stages the newest build of each package `packages.toml` declares under the cache for the Rhino the `rhino-mcp-platform` server row names."""
    match await Rhino.resolve(host):
        case Error() as failed:
            return (outcome(host.app, (failed,)),)
        case Rhino() as rhino:
            await anyio.Path(host.cache).mkdir(parents=True, exist_ok=True)
            results = await anyio.gather(*(staged(host, rhino, package) for package in await declared()))
            return (outcome(host.app, (Header(rhino.bundle.version, str(host.cache)), *(line for lines in results for line in lines))),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Package", "declared", "install", "upgrade"]
