"""Blender's environment and the repository and package rows of `packages.toml`, each package a bundled add-on, a listed extension, or an archive staged from its GitHub source, tool, or patched listing, with the extension Blender builds and the look-development image."""

from collections.abc import Mapping
from glob import escape
from itertools import starmap
from pathlib import Path, PurePosixPath
import shutil
import stat
from typing import Annotated, Final
import zipfile

import anyio
import httpx
import msgspec
from pydantic import BaseModel, Field, FilePath

from interface import roles
from interface.blender.rows import Archive, Installation, Listed, Local, LOOK_DEVELOPMENT, Repository, stamp
from interface.frame import Task
from interface.host import Applied, bootstrap, Bundle, bundle, Change, downloaded, environment, Error, executed, Failed, fetched, Header, Host, Line, literal, outcome, parse
from interface.report import ABSENT, digest, subscript

# --- [TYPES] ----------------------------------------------------------------------------

type Origin = GitHub | Tool | Listing

# --- [CONSTANTS] ------------------------------------------------------------------------

EXTENSION_COMMAND: Final = ("--command", "extension")

# --- [MODELS] ---------------------------------------------------------------------------


class BlenderEnvironment(BaseModel, frozen=True):
    """Blender executable and MCP bridge port the process environment names."""

    blender_path: Annotated[FilePath, Field(alias="BLENDER_PATH")]
    blender_mcp_port: Annotated[int, Field(alias="BLENDER_MCP_PORT", ge=1, le=65535)]


class GitHub(msgspec.Struct, frozen=True, tag="github", tag_field="kind"):
    """GitHub repository whose newest release zip asset, or the head of the named branch, an archive row stages, a legacy add-on under the folder or repository name."""

    repository: str
    branch: str | None = None
    folder: str | None = None


class Tool(msgspec.Struct, frozen=True, tag="tool", tag_field="kind"):
    """mise tool whose install holds the add-on file an archive row stages."""

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
    """`packages.toml` row with the workspace tasks that place it, its GitHub or tool origin, and the libraries, patches, and run paths staged into its archive."""

    id: str
    workspaces: tuple[Task, ...] = ()
    origin: GitHub | Tool | None = None
    libraries: Libraries | None = None
    patches: tuple[Patch, ...] = ()
    rpaths: tuple[Rpath, ...] = ()


class Packages(msgspec.Struct, frozen=True):
    """Declared `packages.toml` repository and package rows."""

    repositories: tuple[Repository, ...]
    packages: tuple[Package, ...]


class Listing(msgspec.Struct, frozen=True):
    """Package a synced remote repository index lists at a version, with its archive address naming the archive hash."""

    id: str
    version: str
    archive_url: str


class Index(msgspec.Struct, frozen=True):
    """Remote repository index Blender synced."""

    data: tuple[Listing, ...]


class Asset(msgspec.Struct, frozen=True):
    """GitHub release asset by file name and download address."""

    name: str
    browser_download_url: str


class Release(msgspec.Struct, frozen=True):
    """GitHub repository's newest release by tag with its assets."""

    tag_name: str
    assets: tuple[Asset, ...]


class Commit(msgspec.Struct, frozen=True):
    """GitHub commit a branch head names."""

    sha: str


class Download(msgspec.Struct, frozen=True, tag=True):
    """Archive at an address: a release's zip asset, a branch head's source zip, or a listed archive."""

    url: str


class Installed(msgspec.Struct, frozen=True, tag=True):
    """Add-on file of a mise tool at an installed version."""

    name: str
    version: str
    file: str


class Build(msgspec.Struct, frozen=True):
    """Build a staged archive records as its comment: the source it came from and the digest of the corrections staged into it."""

    source: Download | Installed
    corrections: str


class Cached(msgspec.Struct, frozen=True):
    """Archive the cache holds with the build its comment records."""

    build: Build
    archive: Archive


class Manifest(msgspec.Struct, frozen=True):
    """Extension manifest id and asset shelves, each shelf idname with the catalog paths it shows."""

    id: str
    shelves: dict[str, tuple[str, ...]]


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [BUNDLE]
async def bundled(variables: BlenderEnvironment) -> Bundle:
    """Blender bundle holding the executable the environment names."""
    return await bundle(variables.blender_path.parents[2])


# --- [DECLARATION]
async def declared() -> Packages:
    """Repository and package rows `packages.toml` declares beside this module, each role placeholder as the byte fractions `rna.Paint` writes a display color as."""
    text = await anyio.Path(__file__).with_name("packages.toml").read_text(encoding="utf-8")
    return msgspec.toml.decode(roles.substituted(text, lambda rgb: ", ".join(map(str, roles.fractions(rgb)))), type=Packages)


# --- [PROCESSES]
async def installation(host: Host, application: Bundle, repositories: tuple[Repository, ...]) -> Installation | Error:
    """Installation facts a background Blender writes offline under the user's preferences once it converged the declared repositories, or the failed run."""
    async with anyio.TemporaryDirectory() as temporary:
        path = anyio.Path(temporary, "installation.json")
        call = bootstrap("interface.blender.script", t"installation({str(path)}, {literal(repositories)})")
        if isinstance(failed := await executed((str(application.executable), "--background", "--offline-mode", "--python-exit-code", "1", "--python-expr", call), host.environ), Error):
            return failed
        return msgspec.json.decode(await path.read_bytes(), type=Installation)


# --- [ARCHIVES]
def stamped(archive: zipfile.ZipFile) -> str:
    """Stamp of the archive's file members by CRC-32 and size."""
    return stamp({info.filename: (info.CRC, info.file_size) for info in archive.infolist() if not info.is_dir()})


def sealed(path: Path) -> Archive:
    """Archive at the path with the stamp of its members."""
    with zipfile.ZipFile(path) as archive:
        return Archive(str(path), stamped(archive))


def commented(target: Path, label: str) -> Cached | Error:
    """Archive the cache holds at the target with the build its comment records, or the labeled error of no staged archive or a comment that records no build."""
    try:
        with zipfile.ZipFile(target) as archive:
            return Cached(msgspec.json.decode(archive.comment, type=Build), Archive(str(target), stamped(archive)))
    except FileNotFoundError:
        return Error(f"{label} has no staged archive at {target}")
    except msgspec.DecodeError as error:
        return Error(f"{label} archive {target} records no build: {error}")


def tagged(asset: str, machines: Mapping[str, str]) -> str:
    """Blender platform id of a split-platform asset name's trailing `<system>_<machine>` tag, the machine mapped through Blender's machine names."""
    system, _, machine = asset.removesuffix(".zip").rpartition("-")[2].partition("_")
    return f"{system}-{machines.get(machine, machine)}"


def extracted(source: Path, tree: Path, label: str, module: str, manifest_filename: str) -> Path | Error:
    """Package folder of the archive extracted into the tree in the form Blender installs, an extension at the tree root and a legacy add-on under its module folder, links kept as links."""
    with zipfile.ZipFile(source) as archive:
        members = [info for info in archive.infolist() if not info.is_dir()]

        def depth(filename: str) -> int:
            return filename.count("/")

        def shallowest(name: str) -> str | None:
            return min((info.filename for info in members if PurePosixPath(info.filename).name == name), key=depth, default=None)

        match shallowest(manifest_filename), shallowest("__init__.py"):
            case str() as held, _:
                prefix, root = held.removesuffix(manifest_filename), tree
            case None, str() as initial:
                prefix, root = initial.removesuffix("__init__.py"), tree / module
            case _:
                return Error(f"{label} archive holds neither {manifest_filename} nor __init__.py")
        for info in [info for info in members if info.filename.startswith(prefix)]:
            (path := root / info.filename.removeprefix(prefix)).parent.mkdir(parents=True, exist_ok=True)
            if stat.S_ISLNK(info.external_attr >> 16):
                path.symlink_to(archive.read(info).decode())
            else:
                path.write_bytes(archive.read(info))
    return root


def patched(root: Path, label: str, patches: tuple[Patch, ...]) -> Path | Error:
    """Root with each patch's text replaced once in its file, line endings kept, or the error naming each patch whose file holds its text other than once."""
    missed = []
    for index, patch in enumerate(patches):
        path = root / patch.file
        try:
            text = path.read_text(encoding="utf-8", newline="")
        except FileNotFoundError:
            text = ""
        if text.count(patch.text) == 1:
            path.write_text(text.replace(patch.text, patch.replacement), encoding="utf-8", newline="")
        else:
            missed.append(f"{subscript(f'{label}.patches', index)} {patch.file}")
    return Error(f"{', '.join(missed)} find no text held once to replace") if missed else root


def zipped(tree: Path, target: Path, build: Build) -> str:
    """Stamp of the archive zipped at the target from every entry under the tree, each folder its own member that an overwriting install removes by, each link stored as what it resolves to, the build recorded as its comment."""
    with zipfile.ZipFile(target, "w", zipfile.ZIP_DEFLATED) as archive:
        archive.comment = msgspec.json.encode(build)
        for path in sorted(tree.rglob("*")):
            archive.write(path.resolve(strict=True), path.relative_to(tree).as_posix())
        return stamped(archive)


# --- [BUILDS]
async def upstream(host: Host, blender: Installation, origin: Origin, revision: str) -> Build | Error:
    """Newest build the origin publishes at the corrections revision: a release's zip asset for this platform, a branch head, a tool's add-on file at its current version, or a listed archive."""
    match origin:
        case GitHub(repository=repository, branch=str() as branch):
            if isinstance(commit := await fetched(host.client, f"https://api.github.com/repos/{repository}/commits/{branch}", Commit), Error):
                return commit
            return Build(Download(f"https://codeload.github.com/{repository}/legacy.zip/{commit.sha}"), revision)
        case GitHub(repository=repository):
            if isinstance(release := await fetched(host.client, f"https://api.github.com/repos/{repository}/releases/latest", Release), Error):
                return release
            zips = {asset.name: asset.browser_download_url for asset in release.assets if asset.name.endswith(".zip")}
            platforms = {name: platform for name in zips if (platform := tagged(name, blender.machines)).partition("-")[0] in blender.systems}
            match [name for name, platform in platforms.items() if platform == blender.platform] if platforms else [*zips]:
                case [asset]:
                    return Build(Download(zips[asset]), revision)
                case assets:
                    return Error(f"{repository} release {release.tag_name} holds {len(assets)} zip assets for {blender.platform} where a release row stages one")
        case Tool(name=name, file=file):
            if isinstance(current := await executed(("mise", "current", name), host.environ), Error):
                return current
            return Build(Installed(name, current.decode().strip(), file), revision)
        case Listing(archive_url=url):
            return Build(Download(url), revision)


async def unpacked(host: Host, blender: Installation, label: str, module: str, build: Build, tree: Path, archive: Path) -> Path | Error:
    """Package folder of the build's source under the tree in the form Blender installs, its fetched archive at the archive path, a tool's add-on file of its install at the version under the module file name."""
    match build.source:
        case Installed(name=name, version=version, file=file):
            if isinstance(where := await executed(("mise", "where", f"{name}@{version}"), host.environ), Error):
                return where
            match [path async for path in anyio.Path(where.decode().strip()).glob(f"**/{escape(file)}")]:
                case [path]:
                    await anyio.Path(tree).mkdir()
                    await path.copy(anyio.Path(tree, module))
                    return tree
                case paths:
                    return Error(f"{name} {version} install holds {file} {len(paths)} times where a tool row stages one")
        case Download(url=url):
            if isinstance(unfetched := await downloaded(host.client, url, archive), Error):
                return unfetched
            return await anyio.to_thread.run_sync(extracted, archive, tree, label, module, blender.manifest_filename)


async def vendored(host: Host, blender: Installation, root: Path, libraries: Libraries | None) -> Path | Error:
    """Root with an add-on's libraries installed by uv into its target folder against Blender's Python, numpy held at the bundled version, or the failed install."""
    if libraries is None:
        return root
    async with anyio.TemporaryDirectory() as temporary:
        constraints = anyio.Path(temporary, "constraints.txt")
        await constraints.write_text(f"numpy=={blender.numpy}\n", encoding="utf-8")
        requirements = () if libraries.requirements is None else ("--requirements", str(root / libraries.requirements))
        command = ("uv", "pip", "install", "--python", blender.python, "--target", str(root / libraries.target), "--constraints", str(constraints), *requirements, *libraries.packages)
        return failed if isinstance(failed := await executed(command, host.environ), Error) else root


async def signed(host: Host, root: Path, rpaths: tuple[Rpath, ...]) -> Path | Error:
    """Root with each executable given its run path and signed again, or the first run-path addition or ad hoc signature that fails."""
    for rpath in rpaths:
        executable = str(root / rpath.file)
        for command in (("/usr/bin/install_name_tool", "-add_rpath", rpath.path, executable), ("/usr/bin/codesign", "--force", "--sign", "-", executable)):
            if isinstance(failed := await executed(command, host.environ), Error):
                return failed
    return root


async def repacked(host: Host, blender: Installation, package: Package, label: str, module: str, build: Build, target: Path) -> str | Error:
    """Stamp of the archive the build stages at the target: its source unpacked in the form Blender installs, patched, given its libraries and run paths, and zipped with the build as its comment."""
    async with anyio.TemporaryDirectory() as temporary:
        tree = Path(temporary, "tree")
        if isinstance(root := await unpacked(host, blender, label, module, build, tree, Path(temporary, "source.zip")), Error):
            return root
        if isinstance(corrected := await anyio.to_thread.run_sync(patched, root, label, package.patches), Error):
            return corrected
        if isinstance(supplied := await vendored(host, blender, corrected, package.libraries), Error):
            return supplied
        if isinstance(sealed_root := await signed(host, supplied, package.rpaths), Error):
            return sealed_root
        return await anyio.to_thread.run_sync(zipped, tree, target, build)


# --- [RESOLUTION]
async def staged(host: Host, blender: Installation, package: Package, label: str, module: str, origin: Origin, *, upgrading: bool) -> tuple[Local, tuple[Change, ...]] | Error:
    """Row installing the package from its cached archive at the recorded build, or at the origin's newest build when upgrading, the archive rebuilt while its recorded build differs, with a change row when rebuilt."""
    vendoring = None if package.libraries is None else (package.libraries, blender.python, blender.numpy)
    target, revision = host.cache / f"{package.id}.zip", digest(msgspec.json.encode((vendoring, package.patches, package.rpaths)))
    held = await anyio.to_thread.run_sync(commented, target, label)
    recorded = msgspec.structs.replace(held.build, corrections=revision) if isinstance(held, Cached) else held
    wanted = await upstream(host, blender, origin, revision) if upgrading else recorded
    if isinstance(wanted, Error):
        return wanted
    if isinstance(held, Cached) and held.build == wanted:
        return Local(package.id, held.archive, package.workspaces), ()
    part = target.with_name(f"{target.name}.part")
    if isinstance(members := await repacked(host, blender, package, label, module, wanted, part), Error):
        return members
    await anyio.Path(part).replace(target)
    before = msgspec.json.encode(held.build).decode() if isinstance(held, Cached) else ABSENT
    return Local(package.id, Archive(str(target), members), package.workspaces), (Change(f"{label}.archive", before, msgspec.json.encode(wanted).decode()),)


async def resolved_row(host: Host, blender: Installation, package: Package, listings: tuple[tuple[str, Listing], ...], *, upgrading: bool) -> tuple[Local | Listed, tuple[Change, ...]] | Error:
    """Row the session receives for the package: a bundled add-on, a listed extension, or its staged archive, or the reason it resolves to none."""
    label = subscript("packages", package.id)
    match package:
        case Package(origin=GitHub(repository=repository, folder=folder) as origin):
            return await staged(host, blender, package, label, folder or repository.partition("/")[2], origin, upgrading=upgrading)
        case Package(origin=Tool(file=file) as origin):
            return await staged(host, blender, package, label, f"{PurePosixPath(file).parts[0]}.py", origin, upgrading=upgrading)
        case Package(id=identity) if identity in blender.core:
            return Local(identity, None, package.workspaces), ()
        case Package(id=identity, patches=patches):
            match [(repository, listing) for repository, listing in listings if listing.id == identity]:
                case [(_, listing)] if patches:
                    return await staged(host, blender, package, label, identity, listing, upgrading=upgrading)
                case [(repository, listing)]:
                    return Listed(identity, repository, listing.version, package.workspaces), ()
                case found:
                    return Error(f"{label} names no bundled add-on and {len(found)} synced repository listings where one resolves it")


async def indexed(repository: str, index: str) -> tuple[tuple[str, Listing], ...] | Error:
    """Every package the remote repository's synced index file lists, with the repository module, or the error of a repository never synced."""
    try:
        listed = await anyio.Path(index).read_bytes()
    except FileNotFoundError:
        return Error(f"repository {repository} holds no synced index at {index}")
    return tuple((repository, listing) for listing in msgspec.json.decode(listed, type=Index).data)


async def resolution(host: Host, blender: Installation, packages: tuple[Package, ...], *, upgrading: bool) -> tuple[tuple[Local | Listed, ...], tuple[Line, ...]]:
    """Session rows of every package row that resolves, with the report rows of the repositories the installation run converged and the change and error rows of resolving every package, the cache holding the staged archives alone once every row resolved."""
    await anyio.Path(host.cache).mkdir(parents=True, exist_ok=True)
    indexes = await anyio.gather(*starmap(indexed, blender.repositories.items()))
    listings = tuple(listing for index in indexes if not isinstance(index, Error) for listing in index)
    results = await anyio.gather(*(resolved_row(host, blender, package, listings, upgrading=upgrading) for package in packages))
    rows = tuple(result for result in results if not isinstance(result, Error))
    if not (errors := tuple(failed for failed in (*indexes, *results) if isinstance(failed, Error))):
        kept = {Path(row.archive.path).name for row, _ in rows if isinstance(row, Local) and row.archive is not None}
        for path in [path async for path in anyio.Path(host.cache).iterdir() if path.name not in kept]:
            await path.unlink()
    return tuple(row for row, _ in rows), (*parse(blender.report), *(change for _, changed in rows for change in changed), *errors)


# --- [CONTENT]
async def packaged(host: Host, application: Bundle, blender: Installation) -> tuple[Manifest, Local] | Error:
    """Manifest of the extension folder and the row of the archive Blender builds from a copy of it with its linked modules resolved, the build's extensions folder its own so the user's shared wheels stay, or the failed build."""
    source = anyio.Path(Path(__file__).with_name("extension"))
    manifest = msgspec.toml.decode(await (source / blender.manifest_filename).read_bytes(), type=Manifest)
    target = anyio.Path(host.artifacts, f"{manifest.id}.zip")
    await target.parent.mkdir(parents=True, exist_ok=True)
    async with anyio.TemporaryDirectory() as temporary:
        folder, extensions = await source.copy(anyio.Path(temporary, manifest.id)), anyio.Path(temporary, "extensions")
        await extensions.mkdir()
        command = (str(application.executable), "--factory-startup", *EXTENSION_COMMAND, "build", "--source-dir", str(folder), "--output-filepath", str(target))
        if isinstance(failed := await executed(command, {**host.environ, "BLENDER_USER_EXTENSIONS": str(extensions)}), Error):
            return failed
    return manifest, Local(manifest.id, await anyio.to_thread.run_sync(sealed, Path(target)), ())


async def look_development(host: Host) -> tuple[Change | Error, ...]:
    """Look-development environment image extracted from its ambientCG set while absent, with a change row when extracted or the failed download."""
    if await anyio.Path(LOOK_DEVELOPMENT).exists():
        return ()
    folder = LOOK_DEVELOPMENT.parent
    url = str(httpx.URL("https://ambientcg.com/get", params={"file": f"{folder.name}.zip"}))
    async with anyio.TemporaryDirectory() as temporary:
        if isinstance(archive := await downloaded(host.client, url, Path(temporary, f"{folder.name}.zip")), Error):
            return (archive,)
        await anyio.to_thread.run_sync(shutil.unpack_archive, archive, folder)
    return (Change(subscript("look_development", LOOK_DEVELOPMENT.name), ABSENT, url),)


# --- [COMPOSITION] ----------------------------------------------------------------------


async def upgrade(host: Host) -> tuple[Applied | Failed]:
    """Upgrades each package `packages.toml` declares to the newest build its source publishes, its repositories converged and synced first."""
    if isinstance(variables := environment(BlenderEnvironment, host.environ), Error):
        return (outcome(host.app, (variables,)),)
    application, declaration = await anyio.gather(bundled(variables), declared())
    if isinstance(blender := await installation(host, application, declaration.repositories), Error):
        return (outcome(host.app, (blender,)),)
    if isinstance(unsynced := await executed((str(application.executable), "--online-mode", *EXTENSION_COMMAND, "sync"), host.environ), Error):
        return (outcome(host.app, (*parse(blender.report), unsynced)),)
    _, lines = await resolution(host, blender, declaration.packages, upgrading=True)
    return (outcome(host.app, (Header(blender.version, str(host.cache)), *lines)),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BlenderEnvironment", "Manifest", "bundled", "declared", "installation", "look_development", "packaged", "resolution", "upgrade"]
