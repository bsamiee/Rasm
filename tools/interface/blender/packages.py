"""Blender's package rows from `packages.toml`, each staged as the archive its source, tool, or remote listing resolves, and the control extension Blender builds."""

from collections.abc import Awaitable, Callable, Coroutine
from enum import StrEnum
from functools import cache, partial
from glob import escape
import hashlib
from pathlib import Path, PurePosixPath
import runpy
import stat
from subprocess import CalledProcessError
from typing import Final, Self
import zipfile

import anyio
import httpx
import msgspec

from interface import roles
from interface.blender.catalog import Catalog, Listed, Local
from interface.host import bootstrap, Change, Failed, Host
from interface.report import ABSENT, digest

# --- [TYPES] ----------------------------------------------------------------------------

type Unresolved = ToolError | NoRelease | NoAsset | NoFile | NoPackage | Unstaged | Unlisted | Unfetched
type Resolved = tuple[Local | Listed, tuple[Change, ...]] | Unresolved
type Written = Path | ToolError | NoPackage | Unfetched


class Access(StrEnum):
    """Blender's network access flag, offline resolving each staged build and online each newest build."""

    OFFLINE = "--offline-mode"
    ONLINE = "--online-mode"


# --- [CONSTANTS] ------------------------------------------------------------------------

PART: Final = ".part"

# --- [MODELS] ---------------------------------------------------------------------------


class GitHub(msgspec.Struct, frozen=True):
    """GitHub repository whose newest release or branch head an archive row installs, a legacy add-on under the folder or repository name."""

    repository: str
    branch: str | None = None
    folder: str | None = None


class Tool(msgspec.Struct, frozen=True):
    """mise tool whose install holds the add-on file an archive row installs."""

    name: str
    file: str


class Libraries(msgspec.Struct, frozen=True):
    """Python packages an add-on imports from its own target folder, resolved against Blender's numpy."""

    target: str
    requirements: str | None = None
    packages: tuple[str, ...] = ()


class Patch(msgspec.Struct, frozen=True):
    """Exact correction of a staged source file, its text held once and its replacement."""

    file: str
    text: str
    replacement: str = ""


class Rpath(msgspec.Struct, frozen=True):
    """Run-path entry added to a staged executable, which is signed again."""

    file: str
    path: str


class Package(msgspec.Struct, frozen=True):
    """`packages.toml` row with its GitHub source or tool, the workspaces that place it, and the libraries, patches, and run paths staged into its archive."""

    id: str
    workspaces: tuple[str, ...] = ()
    source: GitHub | None = None
    tool: Tool | None = None
    libraries: Libraries | None = None
    patches: tuple[Patch, ...] = ()
    rpaths: tuple[Rpath, ...] = ()


class Packages(msgspec.Struct, frozen=True):
    """Declared `packages.toml` rows."""

    packages: tuple[Package, ...]


class Build(msgspec.Struct, frozen=True):
    """Build a staged archive records as its comment, its source address, tool file, or listing hash with the digest of its corrections."""

    source: str
    corrections: str = ""


class Manifest(msgspec.Struct, frozen=True):
    """Extension manifest id and asset shelves, each shelf idname with the ID types of the assets it shows."""

    id: str
    shelves: dict[str, frozenset[str]]


class Asset(msgspec.Struct, frozen=True):
    """Release asset by file name and download address."""

    name: str
    browser_download_url: str


class Release(msgspec.Struct, frozen=True):
    """GitHub release by tag with its assets."""

    tag_name: str
    assets: tuple[Asset, ...]


class Commit(msgspec.Struct, frozen=True):
    """Branch head commit."""

    sha: str


class Listing(msgspec.Struct, frozen=True):
    """Package a remote repository index lists with its version and archive address and hash."""

    id: str
    version: str
    archive_url: str
    archive_hash: str


class Index(msgspec.Struct, frozen=True):
    """Remote repository index Blender synced."""

    data: tuple[Listing, ...]


# --- [ERRORS] ---------------------------------------------------------------------------


class ToolError(msgspec.Struct, frozen=True):
    """Command a resolution ran that exited with an error."""

    command: tuple[str, ...]
    code: int

    @classmethod
    def raised(cls, error: CalledProcessError) -> Self:
        """Error of the failed process."""
        return cls(tuple(map(str, error.cmd)), error.returncode)

    @property
    def message(self) -> str:
        """Error line the outcome shows."""
        return f"`{' '.join(self.command)}` exited with code {self.code}"


class NoRelease(msgspec.Struct, frozen=True):
    """Repository that publishes no release."""

    repository: str

    @property
    def message(self) -> str:
        """Error line the outcome shows."""
        return f"{self.repository} publishes no release"


class NoAsset(msgspec.Struct, frozen=True):
    """Release with several zip assets and none for this platform."""

    repository: str
    names: tuple[str, ...]

    @property
    def message(self) -> str:
        """Error line the outcome shows."""
        return f"{self.repository} releases {', '.join(self.names)} and none names this platform"


class NoFile(msgspec.Struct, frozen=True):
    """Tool install that holds the declared file zero or several times."""

    tool: str
    file: str

    @property
    def message(self) -> str:
        """Error line the outcome shows."""
        return f"{self.tool} holds {self.file} zero or several times"


class NoPackage(msgspec.Struct, frozen=True):
    """Archive holding neither an extension manifest nor a package folder."""

    identity: str

    @property
    def message(self) -> str:
        """Error line the outcome shows."""
        return f"{self.identity} archive holds neither an extension manifest nor __init__.py"


class Unstaged(msgspec.Struct, frozen=True):
    """Source row with no staged build."""

    identity: str

    @property
    def message(self) -> str:
        """Error line the outcome shows."""
        return f"{self.identity} has no staged build. Run `python -m interface.cli upgrade blender` to stage its newest"


class Unlisted(msgspec.Struct, frozen=True):
    """Row without a source or tool that names no bundled add-on and that zero or several remote repositories list."""

    identity: str
    repositories: tuple[str, ...]

    @property
    def message(self) -> str:
        """Error line the outcome shows."""
        return f"{self.identity} is no bundled add-on and {len(self.repositories)} remote repositories list it {self.repositories}"


class Unfetched(msgspec.Struct, frozen=True):
    """Failed download."""

    identity: str
    url: str
    reason: str

    @property
    def message(self) -> str:
        """Error line the outcome shows."""
        return f"{self.identity} download from {self.url} fails with {self.reason}"


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [ARCHIVES]
@cache
def tooling(executable: Path) -> tuple[str, str]:
    """Extension manifest file name and this machine's split-platform archive tag, both from Blender's extension command-line tooling."""
    namespace = runpy.run_path(str(next(executable.parent.parent.glob("Resources/*/scripts/addons_core/bl_pkg/cli/blender_ext.py"))))
    return str(namespace["PKG_MANIFEST_FILENAME_TOML"]), str(namespace["platform_from_this_system"]()).replace("-", "_")


def commented(path: Path) -> Build | None:
    """Build the staged archive records as its comment, None for no archive or a listing's archive."""
    if path.exists():
        with zipfile.ZipFile(path) as archive:
            return msgspec.json.decode(archive.comment, type=Build) if archive.comment else None
    return None


def hashed(path: Path) -> str:
    """Staged file's sha256 in repository index form, empty when none is staged."""
    if path.exists():
        with path.open("rb") as handle:
            return f"sha256:{hashlib.file_digest(handle, 'sha256').hexdigest()}"
    return ""


def corrections(package: Package) -> str:
    """Digest of the libraries, patches, and run paths a row stages, empty when it stages none."""
    parts = (package.libraries, package.patches, package.rpaths)
    return digest(msgspec.json.encode(parts)) if any(parts) else ""


def extracted(source: Path, tree: Path, identity: str, folder: str, manifest: str) -> Path | NoPackage:
    """Package folder of the archive extracted into the tree in the form Blender installs, symbolic links kept."""
    with zipfile.ZipFile(source) as archive:
        members = [info for info in archive.infolist() if not info.is_dir()]
        names = [info.filename for info in members]
        shallowest = {name: min((each for each in names if PurePosixPath(each).name == name), key=lambda each: each.count("/"), default=None) for name in (manifest, "__init__.py")}
        match shallowest[manifest], shallowest["__init__.py"]:
            case str() as held, _:
                root, package = held.removesuffix(manifest), tree
            case None, str() as initial:
                root, package = initial.removesuffix("__init__.py"), tree / folder
            case _:
                return NoPackage(identity)
        for info in [info for info in members if info.filename.startswith(root)]:
            (path := package / info.filename.removeprefix(root)).parent.mkdir(parents=True, exist_ok=True)
            if stat.S_ISLNK(info.external_attr >> 16):
                path.symlink_to(archive.read(info).decode())
            else:
                path.write_bytes(archive.read(info))
    return package


def patched(package: Path, patches: tuple[Patch, ...]) -> Path:
    """Package folder with each patch's text replaced once in its file, line endings kept."""
    for patch in patches:
        path = package / patch.file
        path.write_text(path.read_text(encoding="utf-8", newline="").replace(patch.text, patch.replacement, 1), encoding="utf-8", newline="")
    return package


def zipped(tree: Path, target: Path, build: Build) -> Path:
    """Every file under the tree but bytecode caches zipped at the target, the build recorded as the archive comment."""
    with zipfile.ZipFile(target, "w", zipfile.ZIP_DEFLATED) as archive:
        archive.comment = msgspec.json.encode(build)
        for path in sorted(path for path in tree.rglob("*") if path.is_file() and "__pycache__" not in path.parts):
            archive.write(path, path.relative_to(tree).as_posix())
    return target


async def downloaded(client: httpx.AsyncClient, identity: str, url: str, target: anyio.Path) -> Unfetched | None:
    """Stream the address into the target, returning the failed download."""
    try:
        async with client.stream("GET", url) as response, await anyio.open_file(target, "wb") as sink:
            response.raise_for_status()
            async for chunk in response.aiter_bytes():
                await sink.write(chunk)
    except httpx.HTTPError as error:
        return Unfetched(identity, url, repr(error))
    return None


async def requested[T](client: httpx.AsyncClient, identity: str, url: str, kind: type[T]) -> T | Unfetched:
    """JSON the address answers decoded as the kind, or the failed download."""
    try:
        return msgspec.json.decode((await client.get(url)).raise_for_status().content, type=kind)
    except httpx.HTTPError as error:
        return Unfetched(identity, url, repr(error))


async def installed(executable: Path, root: Path, libraries: Libraries) -> None:
    """Install the add-on's libraries into its target folder with uv against the bundle's Python, numpy held at the bundled version."""
    python = next(executable.parent.parent.glob("Resources/*/python/bin/python3.*"))
    numpy = next(python.parent.parent.glob("lib/python3.*/site-packages/numpy-*.dist-info")).name.removeprefix("numpy-").removesuffix(".dist-info")
    async with anyio.TemporaryDirectory() as scratch:
        constraints = anyio.Path(scratch, "constraints.txt")
        await constraints.write_text(f"numpy=={numpy}\n", encoding="utf-8")
        requirements = () if libraries.requirements is None else ("--requirements", str(root / libraries.requirements))
        await anyio.run_process(["uv", "pip", "install", "--python", str(python), "--target", str(root / libraries.target), "--constraints", str(constraints), *requirements, *libraries.packages])


async def copied(path: Path, name: str, build: Build, target: Path) -> Path:
    """Tool's single-file add-on zipped at the target under its module name, the build recorded as its comment."""
    async with anyio.TemporaryDirectory() as scratch:
        await anyio.Path(path).copy(anyio.Path(scratch, name))
        return await anyio.to_thread.run_sync(zipped, Path(scratch), target, build)


async def repacked(client: httpx.AsyncClient, executable: Path, package: Package, folder: str, build: Build, target: Path) -> Written:
    """Source archive at the build's address, extracted, patched, given its libraries and run paths, and zipped at the target."""
    async with anyio.TemporaryDirectory() as scratch:
        source, tree, (manifest, _) = Path(scratch, "source.zip"), Path(scratch, "tree"), tooling(executable)
        if isinstance(failed := await downloaded(client, package.id, build.source, anyio.Path(source)), Unfetched):
            return failed
        match await anyio.to_thread.run_sync(extracted, source, tree, package.id, folder, manifest):
            case NoPackage() as missing:
                return missing
            case extract:
                root = await anyio.to_thread.run_sync(patched, extract, package.patches)
        try:
            if package.libraries is not None:
                await installed(executable, root, package.libraries)
            for rpath in package.rpaths:
                await anyio.run_process(["/usr/bin/install_name_tool", "-add_rpath", rpath.path, str(root / rpath.file)])
                await anyio.run_process(["/usr/bin/codesign", "--force", "--sign", "-", str(root / rpath.file)])
        except CalledProcessError as error:
            return ToolError.raised(error)
        return await anyio.to_thread.run_sync(zipped, tree, target, build)


async def rebuilt(client: httpx.AsyncClient, executable: Path, listing: Listing, patches: tuple[Patch, ...], build: Build, target: Path) -> Written:
    """Listed archive downloaded, patched, and zipped at the target."""
    async with anyio.TemporaryDirectory() as scratch:
        source, tree, (manifest, _) = Path(scratch, "source.zip"), Path(scratch, "tree"), tooling(executable)
        if isinstance(failed := await downloaded(client, listing.id, listing.archive_url, anyio.Path(source)), Unfetched):
            return failed
        match await anyio.to_thread.run_sync(extracted, source, tree, listing.id, listing.id, manifest):
            case NoPackage() as missing:
                return missing
            case extract:
                await anyio.to_thread.run_sync(patched, extract, patches)
        return await anyio.to_thread.run_sync(zipped, tree, target, build)


async def refreshed(client: httpx.AsyncClient, identity: str, url: str, target: Path, stated: str) -> tuple[Change, ...] | Unfetched:
    """File at the target, downloaded into place while its hash differs from the stated one, with a change row when downloaded."""
    if (held := await anyio.to_thread.run_sync(hashed, target)) == stated:
        return ()
    part = anyio.Path(target.with_suffix(PART))
    if isinstance(failed := await downloaded(client, identity, url, part), Unfetched):
        return failed
    await part.replace(target)
    return (Change(f"packages.{identity}.archive", held or ABSENT, stated),)


# --- [RESOLUTION]
def decoded(text: str) -> tuple[Package, ...]:
    """Rows a `packages.toml` text declares, each role placeholder as the display channels Blender's draw colors read."""

    def channels(rgb: roles.Rgb) -> str:
        return ", ".join(str(round(channel / 255, 4)) for channel in rgb)

    return msgspec.toml.decode(roles.rendered(text, channels), type=Packages).packages


async def background[T](executable: Path, access: Access, module: str, function: str, kind: type[T]) -> T | ToolError:
    """JSON a background Blender writes through the module's function at a scratch path, decoded as the kind, or the failed command."""
    async with anyio.TemporaryDirectory() as scratch:
        path = anyio.Path(scratch, f"{function}.json")
        try:
            await anyio.run_process((str(executable), "--background", access, "--python-exit-code", "1", "--python-expr", bootstrap(module, f"{function}({str(path)!r})")))
        except CalledProcessError as error:
            return ToolError.raised(error)
        return msgspec.json.decode(await path.read_bytes(), type=kind)


async def cataloged(executable: Path, access: Access) -> tuple[Catalog, tuple[tuple[str, Listing], ...]] | ToolError:
    """Catalog a background Blender writes at the access with every package each synced repository index lists, or the failed command."""
    match await background(executable, access, "interface.blender.addons", "cataloged", Catalog):
        case ToolError() as failed:
            return failed
        case catalog:
            return catalog, tuple([
                (module, listing)
                for module, folder in catalog.repositories.items()
                for listing in msgspec.json.decode(await anyio.Path(folder, ".blender_ext", "index.json").read_bytes(), type=Index).data
            ])


async def located(client: httpx.AsyncClient, executable: Path, identity: str, origin: GitHub, access: Access, held: Build | None) -> str | Unstaged | NoRelease | NoAsset | Unfetched:
    """Address of the build a GitHub row's archive holds, the recorded one offline and the newest one online."""
    if access is Access.OFFLINE:
        return Unstaged(identity) if held is None else held.source
    api, codeload = f"https://api.github.com/repos/{origin.repository}", f"https://codeload.github.com/{origin.repository}/zip"
    match origin.branch:
        case str() as branch:
            match await requested(client, identity, f"{api}/commits/{branch}", Commit):
                case Commit(sha=sha):
                    return f"{codeload}/{sha}"
                case failed:
                    return failed
        case None:
            match await requested(client, identity, f"{api}/releases?per_page=1", tuple[Release, ...]):
                case (Release(tag_name=tag, assets=assets),):
                    zips, (_, platform) = [asset for asset in assets if asset.name.endswith(".zip")], tooling(executable)
                    match [asset for asset in zips if asset.name.endswith(f"-{platform}.zip")] or zips:
                        case []:
                            return f"{codeload}/{tag}"
                        case [asset]:
                            return asset.browser_download_url
                        case several:
                            return NoAsset(origin.repository, tuple(asset.name for asset in several))
                case Unfetched() as failed:
                    return failed
                case _:
                    return NoRelease(origin.repository)


async def fetched(target: Path, row: Local, held: Build | None, wanted: Build, write: Callable[[Build, Path], Awaitable[Written]]) -> Resolved:
    """Row with its archive at the target, written while its recorded build differs from the wanted one, with a change row when written."""
    if held == wanted:
        return row, ()
    match await write(wanted, target.with_suffix(PART)):
        case Path() as written:
            await anyio.Path(written).replace(target)
            return row, (Change(f"packages.{row.identity}.archive", ABSENT if held is None else msgspec.json.encode(held).decode(), msgspec.json.encode(wanted).decode()),)
        case failed:
            return failed


async def resolved(staging: Path, client: httpx.AsyncClient, executable: Path, package: Package, access: Access, core: frozenset[str], listings: tuple[tuple[str, Listing], ...]) -> Resolved:
    """Row the session receives with the archive the access resolves, or the reason it does not resolve."""
    target = Path(staging, f"{package.id}.zip")
    row = Local(package.id, str(target), package.workspaces)
    match package:
        case Package(source=GitHub() as origin):
            held = await anyio.to_thread.run_sync(commented, target)
            match await located(client, executable, package.id, origin, access, held):
                case str() as address:
                    return await fetched(target, row, held, Build(address, corrections(package)), partial(repacked, client, executable, package, origin.folder or origin.repository.partition("/")[2]))
                case unresolved:
                    return unresolved
        case Package(tool=Tool(name=name, file=file)):
            try:
                where = await anyio.run_process(["mise", "where", name])
            except CalledProcessError as error:
                return ToolError.raised(error)
            match [path async for path in anyio.Path(where.stdout.decode().strip()).glob(f"**/{escape(file)}")]:
                case [path]:
                    held = await anyio.to_thread.run_sync(commented, target)
                    return await fetched(target, row, held, Build(str(path)), partial(copied, Path(path), f"{PurePosixPath(file).parts[0]}.py"))
                case _:
                    return NoFile(name, file)
        case Package(id=identity) if identity in core:
            return Local(identity, None, package.workspaces), ()
        case Package(id=identity, patches=patches):
            match [(repository, listing) for repository, listing in listings if listing.id == identity]:
                case [(_, listing)] if patches:
                    held = await anyio.to_thread.run_sync(commented, target)
                    return await fetched(target, row, held, Build(listing.archive_hash, corrections(package)), partial(rebuilt, client, executable, listing, patches))
                case [(repository, listing)]:
                    match await refreshed(client, identity, listing.archive_url, target, listing.archive_hash):
                        case tuple() as changes:
                            return Listed(identity, str(target), repository, listing.version, package.workspaces), changes
                        case failed:
                            return failed
                case found:
                    return Unlisted(identity, tuple(repository for repository, _ in found))


async def resolution(
    host: Host,
    client: httpx.AsyncClient,
    executable: Path,
    declared: tuple[Package, ...],
    access: Access,
    content: tuple[Callable[[], Coroutine[object, object, tuple[Change, ...] | Unresolved]], ...],
) -> tuple[Catalog, tuple[Local | Listed, ...], tuple[Change, ...]] | Failed:
    """Blender's catalog, session rows, and change rows once every row and content step resolves at the access, or every error."""
    staging, kept = anyio.Path(host.cache, f"{host.app}-packages"), {f"{package.id}.zip" for package in declared}
    await staging.mkdir(parents=True, exist_ok=True)
    for path in [path async for path in staging.iterdir() if path.name not in kept]:
        await path.unlink()
    match await cataloged(executable, access):
        case ToolError() as failed:
            return Failed(host.app, (failed.message,))
        case catalog, listings:
            row_handles, content_handles = list[anyio.TaskHandle[Resolved]](), list[anyio.TaskHandle[tuple[Change, ...] | Unresolved]]()
            async with anyio.create_task_group() as group:
                row_handles.extend(group.start_soon(resolved, Path(staging), client, executable, package, access, catalog.core, listings) for package in declared)
                content_handles.extend(group.start_soon(step) for step in content)
            row_results, content_results = tuple(handle.return_value for handle in row_handles), tuple(handle.return_value for handle in content_handles)
            match tuple(result.message for result in (*row_results, *content_results) if not isinstance(result, tuple)):
                case ():
                    rows = tuple(result for result in row_results if isinstance(result, tuple))
                    changes = (*(change for _, changes in rows for change in changes), *(change for changed in content_results if isinstance(changed, tuple) for change in changed))
                    return catalog, tuple(row for row, _ in rows), changes
                case errors:
                    return Failed(host.app, errors)


async def packaged(host: Host, executable: Path) -> tuple[Manifest, Local]:
    """Manifest and row of the extension package Blender builds from the extension folder and command aliases."""
    package, source, (name, _) = anyio.Path(host.artifacts, f"{host.app}-extension.zip"), anyio.Path(host.folder, host.app, "extension"), tooling(executable)
    await package.parent.mkdir(parents=True, exist_ok=True)
    async with anyio.TemporaryDirectory() as staging:
        folder = await source.copy(anyio.Path(staging, "extension"))
        await anyio.Path(host.folder, "aliases.txt").copy_into(folder)
        await anyio.run_process([str(executable), "--factory-startup", "--command", "extension", "build", "--source-dir", str(folder), "--output-filepath", str(package)])
    manifest = msgspec.toml.decode(await anyio.Path(source, name).read_bytes(), type=Manifest)
    return manifest, Local(manifest.id, str(package))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PART", "Access", "Manifest", "Package", "Unfetched", "Unresolved", "background", "decoded", "downloaded", "packaged", "resolution"]
