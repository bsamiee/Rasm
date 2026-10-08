"""Blender environment, declared external packages, interface extension, and look-development image."""

from collections.abc import Awaitable, Callable, Mapping
from functools import partial
from glob import escape
from itertools import starmap
from pathlib import Path, PurePosixPath
import shutil
import stat
from typing import Annotated, Final
import zipfile

import anyio
import httpx2
import msgspec
from pydantic import BaseModel, Field, FilePath

from interface import roles
from interface.blender.rows import Archive, Archived, Bundled, Install, Installation, Listed, LOOK_DEVELOPMENT, Repository, stamped
from interface.frame import Task
from interface.host import bootstrap, Bundle, bundle, Change, downloaded, environment, Error, executed, fetched, Header, Host, Line, literal, Outcome, outcome, parse, Result
from interface.report import ABSENT, digest, subscript

# --- [TYPES] ----------------------------------------------------------------------------

type Origin = GitHub | Tool
type Source = Download | Installed
type Provenance = Origin | Listing | Source
type Stage = Callable[[Package, Provenance], Awaitable[Result[tuple[Archived, tuple[Change, ...]]]]]

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
    """`packages.toml` row with the workspace tasks that place it, its origin, and the libraries, patches, and run paths staged into its archive."""

    id: str
    workspaces: tuple[Task, ...] = ()
    origin: Origin | None = None
    libraries: Libraries | None = None
    patches: tuple[Patch, ...] = ()
    rpaths: tuple[Rpath, ...] = ()

    @property
    def label(self) -> str:
        """Report label of the row."""
        return subscript("packages", self.id)

    @property
    def module(self) -> str:
        """Name a legacy add-on installs under: a GitHub folder or repository name, a tool file's top-level name as a module file, else the id."""
        match self.origin:
            case GitHub(repository=repository, folder=folder):
                return folder or repository.partition("/")[2]
            case Tool(file=file):
                return f"{PurePosixPath(file).parts[0]}.py"
            case _:
                return self.id

    def archive(self, cache: Path) -> Path:
        """Staged archive of the row in the cache folder."""
        return cache / f"{self.id}.zip"


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

    source: Source
    corrections: str


class Manifest(msgspec.Struct, frozen=True):
    """Extension manifest id and asset shelves, each shelf idname with the catalog paths it shows."""

    id: str
    shelves: dict[str, tuple[str, ...]]


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [BUNDLE]
async def bundled(variables: BlenderEnvironment) -> Bundle:
    """Blender bundle holding the executable the environment names."""
    return await bundle(variables.blender_path.parents[2])


# --- [PROCESSES]
async def installation(host: Host) -> Result[tuple[BlenderEnvironment, Bundle, tuple[Package, ...], Installation]]:
    """Environment, bundle, declared package rows, and the installation facts a background Blender writes offline once it converged the declared repositories, each role placeholder in `packages.toml` the byte fractions `rna.Paint` writes a display color as, or the failed read or run."""
    if isinstance(variables := environment(BlenderEnvironment, host.environ), Error):
        return variables
    application, text = await anyio.gather(bundled(variables), anyio.Path(__file__).with_name("packages.toml").read_text(encoding="utf-8"))
    declaration = msgspec.toml.decode(roles.substituted(text, lambda rgb: ", ".join(map(str, roles.fractions(rgb)))), type=Packages)
    async with anyio.TemporaryDirectory() as temporary:
        path = anyio.Path(temporary, "installation.json")
        call = bootstrap("interface.blender.script", t"installation({str(path)}, {literal(declaration.repositories)})")
        if isinstance(failed := await executed((str(application.executable), "--background", "--offline-mode", "--python-exit-code", "1", "--python-expr", call), host.environ), Error):
            return failed
        return variables, application, declaration.packages, msgspec.json.decode(await path.read_bytes(), type=Installation)


# --- [ARCHIVES]
def sealed(path: Path) -> Archive:
    """Archive at the path with the stamp of its members."""
    with zipfile.ZipFile(path) as archive:
        return Archive(str(path), stamped(archive))


def commented(target: Path, package: Package) -> Result[tuple[Build, Archive]]:
    """Build the comment of the archive the cache holds at the target records, with the archive, or the error of no staged archive or a comment that records no build."""
    try:
        with zipfile.ZipFile(target) as archive:
            return msgspec.json.decode(archive.comment, type=Build), Archive(str(target), stamped(archive))
    except FileNotFoundError:
        return Error(f"{package.label} has no staged archive at {target}")
    except msgspec.DecodeError as error:
        return Error(f"{package.label} archive {target} records no build: {error}")


def tagged(asset: str, machines: Mapping[str, str]) -> str:
    """Blender platform id of a split-platform asset name's trailing `<system>_<machine>` tag, the machine mapped through Blender's machine names."""
    system, _, machine = asset.removesuffix(".zip").rpartition("-")[2].partition("_")
    return f"{system}-{machines.get(machine, machine)}"


def extracted(source: Path, tree: Path, package: Package, manifest_filename: str) -> Result[Path]:
    """Package folder of the archive extracted into the tree in the form Blender installs, an extension at the tree root and a legacy add-on under its module folder, links kept as links."""
    with zipfile.ZipFile(source) as archive:
        members = sorted((info for info in archive.infolist() if not info.is_dir()), key=lambda info: info.filename.count("/"))

        def shallowest(name: str) -> str | None:
            return next((info.filename for info in members if PurePosixPath(info.filename).name == name), None)

        match shallowest(manifest_filename), shallowest("__init__.py"):
            case str() as held, _:
                prefix, root = held.removesuffix(manifest_filename), tree
            case None, str() as initial:
                prefix, root = initial.removesuffix("__init__.py"), tree / package.module
            case _:
                return Error(f"{package.label} archive holds neither {manifest_filename} nor __init__.py")
        for info in [info for info in members if info.filename.startswith(prefix)]:
            (path := root / info.filename.removeprefix(prefix)).parent.mkdir(parents=True, exist_ok=True)
            if stat.S_ISLNK(info.external_attr >> 16):
                path.symlink_to(archive.read(info).decode())
            else:
                path.write_bytes(archive.read(info))
    return root


def patched(root: Path, package: Package) -> Error | None:
    """Error naming each patch whose file holds its text other than once, each other patch's text replaced in its file with line endings kept."""
    missed = []
    for index, patch in enumerate(package.patches):
        path = root / patch.file
        try:
            text = path.read_text(encoding="utf-8", newline="")
        except FileNotFoundError:
            text = ""
        if text.count(patch.text) == 1:
            path.write_text(text.replace(patch.text, patch.replacement), encoding="utf-8", newline="")
        else:
            missed.append(f"{subscript(f'{package.label}.patches', index)} {patch.file}")
    return Error(f"{', '.join(missed)} find no text held once to replace") if missed else None


def zipped(tree: Path, target: Path, build: Build) -> str:
    """Stamp of the archive zipped at the target from every entry under the tree, each folder its own member that an overwriting install removes by, each link stored as what it resolves to, the build recorded as its comment."""
    with zipfile.ZipFile(target, "w", zipfile.ZIP_DEFLATED) as archive:
        archive.comment = msgspec.json.encode(build)
        for path in sorted(tree.rglob("*")):
            archive.write(path.resolve(strict=True), path.relative_to(tree).as_posix())
        return stamped(archive)


# --- [BUILDS]
async def upstream(host: Host, blender: Installation, origin: Provenance) -> Result[Source]:
    """Resolve a release, branch, tool, or listing to its source, retaining a recorded source."""
    match origin:
        case GitHub(repository=repository, branch=str() as branch):
            commit = await fetched(host.client, f"https://api.github.com/repos/{repository}/commits/{branch}", msgspec.json.Decoder(Commit).decode)
            return commit if isinstance(commit, Error) else Download(f"https://codeload.github.com/{repository}/legacy.zip/{commit.sha}")
        case GitHub(repository=repository):
            if isinstance(release := await fetched(host.client, f"https://api.github.com/repos/{repository}/releases/latest", msgspec.json.Decoder(Release).decode), Error):
                return release
            zips = {asset.name: asset.browser_download_url for asset in release.assets if asset.name.endswith(".zip")}
            platforms = {name: platform for name in zips if (platform := tagged(name, blender.machines)).partition("-")[0] in blender.systems}
            match [name for name, platform in platforms.items() if platform == blender.platform] if platforms else [*zips]:
                case [asset]:
                    return Download(zips[asset])
                case assets:
                    return Error(f"{repository} release {release.tag_name} holds {len(assets)} zip assets for {blender.platform} where a release row stages one")
        case Tool(name=name, file=file):
            current = await executed(("mise", "current", name), host.environ)
            return current if isinstance(current, Error) else Installed(name, current.decode().strip(), file)
        case Listing(archive_url=url):
            return Download(url)
        case Download() | Installed() as source:
            return source


async def unpacked(host: Host, blender: Installation, package: Package, source: Source, tree: Path) -> Result[Path]:
    """Package folder of the source under the tree in the form Blender installs, a fetched archive beside the tree as `source.zip`, a tool's add-on file of its install at the version under the module file name."""
    match source:
        case Installed(name=name, version=version, file=file):
            if isinstance(where := await executed(("mise", "where", f"{name}@{version}"), host.environ), Error):
                return where
            match [path async for path in anyio.Path(where.decode().strip()).glob(f"**/{escape(file)}")]:
                case [path]:
                    await anyio.Path(tree).mkdir()
                    await path.copy(anyio.Path(tree, package.module))
                    return tree
                case paths:
                    return Error(f"{name} {version} install holds {file} {len(paths)} times where a tool row stages one")
        case Download(url=url):
            if isinstance(archive := await downloaded(host.client, url, tree.with_name("source.zip")), Error):
                return archive
            return await anyio.to_thread.run_sync(extracted, archive, tree, package, blender.manifest_filename)


async def vendored(host: Host, blender: Installation, root: Path, libraries: Libraries | None) -> Error | None:
    """Failure of installing the add-on's libraries by uv into its target folder against Blender's Python with numpy held at the bundled version."""
    if libraries is None:
        return None
    async with anyio.TemporaryDirectory() as temporary:
        constraints = anyio.Path(temporary, "constraints.txt")
        await constraints.write_text(f"numpy=={blender.interpreter.distributions['numpy']}\n", encoding="utf-8")
        requirements = () if libraries.requirements is None else ("--requirements", str(root / libraries.requirements))
        command = ("uv", "pip", "install", "--python", blender.interpreter.executable, "--target", str(root / libraries.target), "--constraints", str(constraints), *requirements, *libraries.packages)
        return failed if isinstance(failed := await executed(command, host.environ), Error) else None


async def signed(host: Host, root: Path, rpaths: tuple[Rpath, ...]) -> Error | None:
    """First failure of adding each executable's run path and signing it again ad hoc."""
    for rpath in rpaths:
        executable = str(root / rpath.file)
        for command in (("/usr/bin/install_name_tool", "-add_rpath", rpath.path, executable), ("/usr/bin/codesign", "--force", "--sign", "-", executable)):
            if isinstance(failed := await executed(command, host.environ), Error):
                return failed
    return None


async def repacked(host: Host, blender: Installation, package: Package, build: Build, target: Path) -> Result[str]:
    """Stamp of the archive the build stages at the target: its source unpacked in the form Blender installs, patched, given its libraries and run paths, and zipped with the build as its comment."""
    async with anyio.TemporaryDirectory() as temporary:
        tree = Path(temporary, "tree")
        if isinstance(root := await unpacked(host, blender, package, build.source, tree), Error):
            return root
        if failed := await anyio.to_thread.run_sync(patched, root, package) or await vendored(host, blender, root, package.libraries) or await signed(host, root, package.rpaths):
            return failed
        return await anyio.to_thread.run_sync(zipped, tree, target, build)


# --- [RESOLUTION]
async def held(host: Host, blender: Installation, package: Package) -> Result[tuple[Archived, tuple[Change, ...]]]:
    """Row installing the source the cached archive records at the package's current corrections, or the error of none staged."""
    match await anyio.to_thread.run_sync(commented, package.archive(host.cache), package):
        case (Build(source=source), _):
            return await staged(host, blender, package, source)
        case failed:
            return failed


async def staged(host: Host, blender: Installation, package: Package, origin: Provenance) -> Result[tuple[Archived, tuple[Change, ...]]]:
    """Row installing the provenance's source at the package's corrections, the cached archive rebuilt with a change row while the build it records differs."""
    if isinstance(source := await upstream(host, blender, origin), Error):
        return source
    target = package.archive(host.cache)
    wanted = Build(source, digest(msgspec.json.encode((package.libraries and (package.libraries, blender.interpreter), package.patches, package.rpaths), order="deterministic")))
    match await anyio.to_thread.run_sync(commented, target, package):
        case (build, Archive() as archive) if build == wanted:
            return Archived(package.id, archive, package.workspaces), ()
        case (Build() as build, _):
            before = msgspec.json.encode(build).decode()
        case Error():
            before = ABSENT
    part = target.with_name(f"{target.name}.part")
    if isinstance(stamp := await repacked(host, blender, package, wanted, part), Error):
        return stamp
    await anyio.Path(part).replace(target)
    return Archived(package.id, Archive(str(target), stamp), package.workspaces), (Change(f"{package.label}.archive", before, msgspec.json.encode(wanted).decode()),)


async def resolved(blender: Installation, stage: Stage, listings: tuple[tuple[str, Listing], ...], package: Package) -> Result[tuple[Install, tuple[Change, ...]]]:
    """Row the session receives for the package: its staged archive, a bundled add-on, or a listed extension, or the reason it resolves to none."""
    match package:
        case Package(origin=origin) if origin is not None:
            return await stage(package, origin)
        case Package(id=identity) if identity in blender.core:
            return Bundled(identity, package.workspaces), ()
        case Package(id=identity):
            match [(repository, listing) for repository, listing in listings if listing.id == identity]:
                case [(_, listing)] if package.libraries or package.patches or package.rpaths:
                    return await stage(package, listing)
                case [(repository, listing)]:
                    return Listed(identity, repository, listing.version, package.workspaces), ()
                case found:
                    return Error(f"{package.label} names no bundled add-on and {len(found)} synced repository listings where one resolves it")


async def indexed(repository: str, index: str) -> Result[tuple[tuple[str, Listing], ...]]:
    """Every package the remote repository's synced index file lists, with the repository module, or the error of a repository never synced."""
    try:
        listed = await anyio.Path(index).read_bytes()
    except FileNotFoundError:
        return Error(f"repository {repository} holds no synced index at {index}")
    return tuple((repository, listing) for listing in msgspec.json.decode(listed, type=Index).data)


async def resolution(host: Host, blender: Installation, packages: tuple[Package, ...], stage: Stage) -> tuple[tuple[Install, ...], tuple[Line, ...]]:
    """Session rows of every package row that resolves, archive rows through the stage, with the report rows of the repositories the installation run converged and the change and error rows of resolving every package, the cache holding the staged archives alone once every row resolved."""
    await anyio.Path(host.cache).mkdir(parents=True, exist_ok=True)
    indexes = await anyio.gather(*starmap(indexed, blender.repositories.items()))
    listings = tuple(listing for index in indexes if not isinstance(index, Error) for listing in index)
    results = await anyio.gather(*(resolved(blender, stage, listings, package) for package in packages))
    rows = tuple(result for result in results if not isinstance(result, Error))
    if not (errors := tuple(failed for failed in (*indexes, *results) if isinstance(failed, Error))):
        kept = {Path(row.archive.path).name for row, _ in rows if isinstance(row, Archived)}
        for path in [path async for path in anyio.Path(host.cache).iterdir() if path.name not in kept]:
            await path.unlink()
    return tuple(row for row, _ in rows), (*parse(blender.report), *(change for _, changed in rows for change in changed), *errors)


# --- [CONTENT]
async def built(environ: Mapping[str, str], application: Bundle, source: Path, *output: str) -> Result[bytes]:
    """Output of Blender building the extension at the source with factory preferences and its manifest checks, or the failed build."""
    return await executed((str(application.executable), "--factory-startup", *EXTENSION_COMMAND, "build", "--source-dir", str(source), *output), environ)


async def packaged(host: Host, application: Bundle, blender: Installation) -> Result[tuple[Manifest, Archived]]:
    """Manifest of the extension folder and the row of the archive Blender builds from a copy of it with its linked modules resolved, or the failed build."""
    source = anyio.Path(Path(__file__).with_name("extension"))
    manifest = msgspec.toml.decode(await (source / blender.manifest_filename).read_bytes(), type=Manifest)
    target = anyio.Path(host.artifacts, f"{manifest.id}.zip")
    await target.parent.mkdir(parents=True, exist_ok=True)
    async with anyio.TemporaryDirectory() as temporary:
        folder = await source.copy(anyio.Path(temporary, manifest.id))
        if isinstance(failed := await built(host.environ, application, Path(folder), "--output-filepath", str(target)), Error):
            return failed
    return manifest, Archived(manifest.id, await anyio.to_thread.run_sync(sealed, Path(target)), ())


async def look_development(host: Host) -> tuple[Result[Change], ...]:
    """Look-development environment image extracted from its ambientCG set while absent, with a change row when extracted or the failed download."""
    if await anyio.Path(LOOK_DEVELOPMENT).exists():
        return ()
    folder = LOOK_DEVELOPMENT.parent
    url = str(httpx2.URL("https://ambientcg.com/get", params={"file": f"{folder.name}.zip"}))
    async with anyio.TemporaryDirectory() as temporary:
        if isinstance(archive := await downloaded(host.client, url, Path(temporary, f"{folder.name}.zip")), Error):
            return (archive,)
        await anyio.to_thread.run_sync(shutil.unpack_archive, archive, folder)
    return (Change(subscript("look_development", LOOK_DEVELOPMENT.name), ABSENT, url),)


# --- [COMPOSITION] ----------------------------------------------------------------------


async def upgrade(host: Host) -> tuple[Outcome]:
    """Upgrades each package `packages.toml` declares to the newest build its source publishes, its repositories converged and synced first."""
    if isinstance(prepared := await installation(host), Error):
        return (outcome(host.app, (prepared,)),)
    _, application, packages, blender = prepared
    if isinstance(unsynced := await executed((str(application.executable), "--online-mode", *EXTENSION_COMMAND, "sync"), host.environ), Error):
        return (outcome(host.app, (*parse(blender.report), unsynced)),)
    _, lines = await resolution(host, blender, packages, partial(staged, host, blender))
    return (outcome(host.app, (Header(blender.version, str(host.cache)), *lines)),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BlenderEnvironment", "Manifest", "built", "bundled", "held", "installation", "look_development", "packaged", "resolution", "upgrade"]
