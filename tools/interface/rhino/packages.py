"""Rhino packages `packages.toml` declares, installed through `yak` at the newest build each row's source publishes for the Rhino that `yak` serves."""

from pathlib import Path
import re
from typing import Final
import zipfile

import anyio
import httpx
import msgspec

from interface.host import Applied, Change, Failed, gather, Host

# --- [TYPES] ----------------------------------------------------------------------------

type Converged = Change | NoRelease | None

# --- [CONSTANTS] ------------------------------------------------------------------------

YAK: Final = "yak"

# --- [MODELS] ---------------------------------------------------------------------------


class Source(msgspec.Struct, frozen=True):
    """GitHub repository whose newest successful run of a workflow on a branch uploads the package as an artifact, `{major}` standing for Rhino's major version."""

    repository: str
    branch: str
    workflow: str
    artifact: str


class Package(msgspec.Struct, frozen=True):
    """`packages.toml` row by yak id, built from its workflow artifact or plug-in project or else released on the Yak server, with the `window.Panel` names of the panels its plug-in adds and its commands by `window.Toolbar` name in ribbon order."""

    id: str
    source: Source | None = None
    project: str | None = None
    panels: tuple[str, ...] = ()
    commands: dict[str, tuple[str, ...]] = {}


class Packages(msgspec.Struct, frozen=True):
    """Declared `packages.toml` rows in ribbon order."""

    packages: tuple[Package, ...]


class Distribution(msgspec.Struct, frozen=True):
    """Yak server file of a version by the Rhino release and platform it targets."""

    rhino_version: str
    platform: str


class Version(msgspec.Struct, frozen=True):
    """Yak server version of a package with its distributions."""

    version: str
    distributions: tuple[Distribution, ...]


class Manifest(msgspec.Struct, frozen=True):
    """Version a yak package's manifest states."""

    version: str


class Rhino(msgspec.Struct, frozen=True):
    """Release and build of the Rhino the PATH's `yak` serves, the folder `yak` installs packages into, and the installed version by yak id."""

    major: int
    minor: int
    build: str
    directory: Path
    installed: dict[str, str]

    @classmethod
    async def served(cls) -> Rhino:
        """Rhino `yak version` and `yak list` report."""
        version, listing = (await anyio.run_process([YAK, "version"])).stdout.decode(), (await anyio.run_process([YAK, "list"])).stdout.decode()
        ((major, minor, build),) = re.findall(r"Rhino (\d+)\.(\d+) \(([^)]+)\)", version)
        (directory,) = re.findall(r"^Package directory: (.+)$", listing, re.MULTILINE)
        return cls(int(major), int(minor), build, Path(directory), dict(re.findall(r"^(.+) \(([^()]+)\)$", listing, re.MULTILINE)))

    def loads(self, distribution: Distribution) -> bool:
        """Whether this Rhino on macOS loads the distribution."""
        releases = re.findall(r"rh(\d+)(?:_(\d+))?", distribution.rhino_version)
        return distribution.platform != "win" and (distribution.rhino_version == "any" or any((int(major), int(minor or 0)) <= (self.major, self.minor) for major, minor in releases))


# --- [ERRORS] ---------------------------------------------------------------------------


class NoRelease(msgspec.Struct, frozen=True):
    """Package the Yak server publishes in no version with a distribution this Rhino on macOS loads."""

    identity: str


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [BUILDS]
async def packed(root: Path, project: str, folder: anyio.Path) -> Path:
    """Yak package the plug-in project's Release publish into the folder builds."""
    await anyio.run_process(["dotnet", "publish", str(root / project), "--configuration", "Release", "--output", str(folder)])
    (built,) = [path async for path in folder.glob("*.yak")]
    return Path(built)


async def downloaded(rhino: Rhino, source: Source, folder: anyio.Path) -> Path:
    """Archive the newest successful run of the source's workflow on its branch uploads, downloaded into the folder."""
    listed = await anyio.run_process([
        "gh",
        "run",
        "list",
        "-R",
        source.repository,
        "-w",
        source.workflow,
        "-b",
        source.branch.format(major=rhino.major),
        "-s",
        "success",
        "-L",
        "1",
        "--json",
        "databaseId",
        "-q",
        ".[0].databaseId",
    ])
    await anyio.run_process(["gh", "run", "download", listed.stdout.decode().strip(), "-R", source.repository, "-n", source.artifact.format(major=rhino.major), "-D", str(folder)])
    (archive,) = [path async for path in folder.iterdir()]
    return Path(archive)


# --- [INSTALLS]
async def installed(rhino: Rhino, identity: str, version: str, *install: str) -> Change:
    """Change row of the package reinstalled at the version from the `yak install` arguments."""
    await anyio.run_process([YAK, "uninstall", identity])
    await anyio.run_process([YAK, "install", *install])
    return Change(f"packages.{identity}", rhino.installed.get(identity, "absent"), version)


def held(rhino: Rhino, identity: str, archive: Path) -> tuple[str, bool]:
    """Version the archive's manifest states and whether the installed build is that version holding the archive's files byte for byte."""
    with zipfile.ZipFile(archive) as contents:
        version = msgspec.yaml.decode(contents.read("manifest.yml"), type=Manifest).version
        files = {entry.filename: contents.read(entry) for entry in contents.infolist() if not entry.is_dir()}
    folder = rhino.directory / identity / version
    return version, rhino.installed.get(identity) == version and files == {path.relative_to(folder).as_posix(): path.read_bytes() for path in folder.rglob("*") if path.is_file()}


async def converged(rhino: Rhino, identity: str, archive: Path) -> Change | None:
    """Change row of the archive installed while its version or files differ from the installed build, None while they match."""
    match await anyio.to_thread.run_sync(held, rhino, identity, archive):
        case version, False:
            return await installed(rhino, identity, version, str(archive))
        case _:
            return None


async def released(client: httpx.AsyncClient, rhino: Rhino, identity: str) -> Converged:
    """Change row of the newest Yak server version this Rhino loads installed over another version, None while it is installed."""
    versions = msgspec.json.decode((await client.get(f"https://yak.rhino3d.com/versions/{identity}")).raise_for_status().content, type=tuple[Version, ...])
    match next((each.version for each in versions if any(map(rhino.loads, each.distributions))), None):
        case None:
            return NoRelease(identity)
        case newest if rhino.installed.get(identity) == newest:
            return None
        case newest:
            return await installed(rhino, identity, newest, identity, newest)


async def upgraded(client: httpx.AsyncClient, rhino: Rhino, root: Path, temporary: anyio.Path, package: Package) -> Converged:
    """Change row of the package converged on the newest build its row names, None while that build is installed."""
    match package:
        case Package(id=identity, source=Source() as source):
            return await converged(rhino, identity, await downloaded(rhino, source, temporary / identity))
        case Package(id=identity, project=str() as project):
            return await converged(rhino, identity, await packed(root, project, temporary / identity))
        case Package(id=identity):
            return await released(client, rhino, identity)


async def declared() -> tuple[Package, ...]:
    """Package rows `packages.toml` declares in ribbon order."""
    return msgspec.toml.decode(await anyio.Path(__file__).with_name("packages.toml").read_bytes(), type=Packages).packages


# --- [COMPOSITION] ----------------------------------------------------------------------


async def upgrade(host: Host) -> tuple[Applied | Failed, ...]:
    """Installs each package `packages.toml` declares at the newest build its source publishes for the Rhino `yak` serves."""
    rhino, packages = await Rhino.served(), await declared()
    async with anyio.TemporaryDirectory() as temporary:
        results = await gather(upgraded(host.client, rhino, host.root, anyio.Path(temporary), package) for package in packages)
    changes = tuple(result for result in results if isinstance(result, Change))
    match tuple(f"{result.identity} publishes no version Rhino {rhino.major}.{rhino.minor} on macOS loads" for result in results if isinstance(result, NoRelease)):
        case ():
            return (Applied(host.app, rhino.build, rhino.directory, changes),)
        case errors:
            return (Failed(host.app, errors, changes),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Package", "declared", "upgrade"]
