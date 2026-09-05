"""Pinned build tool provisioning and the workspace, process, download, checkout, and vcpkg operations every script shares."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Callable, Iterable, Mapping, Sequence
from functools import partial
import hashlib
import os
from pathlib import Path
import platform
import re
import shutil
import tarfile
from typing import Literal
import zipfile

import anyio
import cyclopts
from expression import Error, Ok, Result
import httpx
import msgspec
import stamina
import structlog

# --- [TYPES] ----------------------------------------------------------------------------

type Rid = Literal["osx-arm64", "linux-x64", "linux-arm64", "win-x64"]
type ArchiveFormat = Literal["tar", "zip", "pkg", "nsis"]


class CommandFailed(msgspec.Struct, frozen=True, gc=False):
    """Build tool could not start or exited nonzero."""

    command: tuple[str, ...]
    detail: str


class DownloadFailed(msgspec.Struct, frozen=True, gc=False):
    """Transfer of a pinned file failed or its bytes hashed to another digest."""

    url: str
    detail: str


class PinMismatch(msgspec.Struct, frozen=True, gc=False):
    """Pinned input does not decode or disagrees with the source it pins."""

    subject: str
    detail: str


class HostUnsupported(msgspec.Struct, frozen=True, gc=False):
    """Host, environment, or runtime identifier the pins do not cover."""

    detail: str


class FileMissing(msgspec.Struct, frozen=True, gc=False):
    """Operation produced no file matching the name under the directory."""

    directory: Path
    name: str


class NoMutants(msgspec.Struct, frozen=True, gc=False):
    """Mutation run wrote a report holding zero mutants."""

    language: str
    report: Path


type Failure = CommandFailed | DownloadFailed | PinMismatch | HostUnsupported | FileMissing | NoMutants


class Dependency(msgspec.Struct, frozen=True, gc=False):
    """Vcpkg manifest dependency in its object form."""

    name: str


class PortManifest(msgspec.Struct, frozen=True, gc=False, rename={"version": "version-string", "baseline": "builtin-baseline"}):
    """Committed vcpkg manifest pinning one port, the vcpkg baseline, and the package version its build produces."""

    version: str
    baseline: str
    dependencies: list[str | Dependency]


class Asset(msgspec.Struct, frozen=True, gc=False):
    """Release asset of one runtime identifier with the path of the tool or library inside it."""

    name: str
    sha256: str
    format: ArchiveFormat
    path: str


class ReleaseManifest(msgspec.Struct, frozen=True, gc=False, rename={"version": "version-string"}):
    """Committed manifest pinning a release version, its download URL pattern, and one asset per runtime identifier."""

    version: str
    url: str
    runtimes: dict[str, Asset]


class ExtensionManifest(msgspec.Struct, frozen=True, gc=False, rename={"version": "version-string"}):
    """Committed manifest pinning the DuckDB engine version, the repository URL pattern, the platform per runtime identifier, and each extension digest."""

    version: str
    url: str
    platforms: dict[str, str]
    extensions: dict[str, dict[str, str]]


class BuildStep(msgspec.Struct, frozen=True, gc=False):
    """CMake configure and build of one source directory of a checkout."""

    source: str
    build: str
    target: str
    flags: list[str] = []


class Build(msgspec.Struct, frozen=True, gc=False):
    """CMake build of one runtime identifier with its shared flags, ordered steps, install prefix, and built library."""

    flags: list[str]
    steps: list[BuildStep]
    install: str
    library: str


class SourceManifest(msgspec.Struct, frozen=True, gc=False, rename={"version": "version-string"}):
    """Committed manifest pinning a git checkout, its submodules, and the CMake build per supported runtime identifier."""

    version: str
    url: str
    commit: str
    submodules: list[str]
    runtimes: dict[str, Build]


class Workspace(msgspec.Struct, frozen=True, gc=False):
    """Repository root, host runtime identifier, the vcpkg environment, and the directories every operation reads."""

    root: Path
    host: Rid
    environment: Mapping[str, str]
    binary_cache: Path
    downloads: Path
    cache: Path
    manifests: Path
    artifacts: Path


class ToolSet(msgspec.Struct, frozen=True, gc=False):
    """Pinned vcpkg executable and the environment its builds run under."""

    vcpkg: Path
    env: Mapping[str, str]


# --- [CONSTANTS] ------------------------------------------------------------------------

_VCPKG_URL = "https://github.com/microsoft/vcpkg"
_LOCK_FILE = "uv.lock"
_HOST_TOOLS = ("energyplus", "ktx")  # Executable folders linked under .cache/tools, the sqlite-vec loadable stays a stage input

_log = structlog.get_logger(__name__)
_app = cyclopts.App(name="provision")

# --- [WORKSPACE] ------------------------------------------------------------------------


def host_rid(system: str, machine: str) -> Result[Rid, HostUnsupported]:
    """Map the platform system and machine names to the runtime identifier of the host."""
    match system, machine.lower():
        case ("Darwin", "arm64" | "aarch64"):
            return Ok("osx-arm64")
        case ("Linux", "x86_64" | "amd64"):
            return Ok("linux-x64")
        case ("Linux", "arm64" | "aarch64"):
            return Ok("linux-arm64")
        case ("Windows", "x86_64" | "amd64"):
            return Ok("win-x64")
        case _:
            return Error(HostUnsupported(f"host {system}/{machine} maps to no runtime identifier"))


def system(rid: Rid) -> str:
    """Return the operating system segment of a runtime identifier."""
    return rid.partition("-")[0]


def repository_root(start: Path) -> Result[Path, Failure]:
    """Return the nearest ancestor of the start path holding the root lock file."""
    match [parent for parent in (start, *start.parents) if (parent / _LOCK_FILE).is_file()]:
        case [root, *_]:
            return Ok(root)
        case _:
            return Error(HostUnsupported(f"no ancestor of {start} holds {_LOCK_FILE}"))


def workspace(start: Path, host_system: str, host_machine: str) -> Result[Workspace, Failure]:
    """Detect the host and find the repository root, every cache and artifact directory derives from the root."""

    def build(root: Path, host: Rid) -> Workspace:
        binary_cache, downloads = root / ".cache" / "vcpkg-archives", root / ".cache" / "vcpkg-downloads"
        # vcpkg inherits the process environment with its cache directories, the native-cache action names the same paths
        environment = {**os.environ, "VCPKG_DEFAULT_BINARY_CACHE": str(binary_cache), "VCPKG_DOWNLOADS": str(downloads)}  # ruff:ignore[banned-api]
        return Workspace(root, host, environment, binary_cache, downloads, root / ".cache", root / "eng" / "native", root / ".artifacts" / "native")

    return repository_root(start).map2(host_rid(host_system, host_machine), build)


def read_manifest[T](path: Path, manifest: type[T]) -> Result[T, PinMismatch]:
    """Decode a committed JSON manifest into its struct."""
    try:
        decoded = msgspec.json.decode(path.read_bytes(), type=manifest)
    except (OSError, msgspec.DecodeError) as error:
        return Error(PinMismatch(f"Manifest {path}", f"does not decode, {error}"))
    return Ok(decoded)


def http_client() -> httpx.AsyncClient:
    """Return the client every pinned download shares, redirects followed and a long read timeout for release assets."""
    return httpx.AsyncClient(follow_redirects=True, timeout=httpx.Timeout(30.0, read=600.0))


# --- [PROCESSES] ------------------------------------------------------------------------


async def run(args: Sequence[str], cwd: Path, env: Mapping[str, str] | None = None, *, capture: bool = False) -> Result[str, CommandFailed]:
    """Run a build tool to completion on the inherited console and return its standard output when captured."""
    process = partial(anyio.run_process, args, cwd=cwd, env=env, stderr=None, check=False)
    try:
        completed = await (process() if capture else process(stdout=None))
    except OSError as error:
        return Error(CommandFailed(tuple(args), str(error)))
    if completed.returncode != 0:
        return Error(CommandFailed(tuple(args), f"exit status {completed.returncode}"))
    return Ok(completed.stdout.decode() if capture else "")


async def run_each(commands: Iterable[Sequence[str]], cwd: Path, env: Mapping[str, str] | None = None) -> Result[None, CommandFailed]:
    """Run dependent build tool commands in order, stopping at the first failure."""
    for command in commands:
        match await run(command, cwd, env):
            case Result(tag="error", error=failure):
                return Error(failure)
            case Result():
                pass
    return Ok(None)


async def checkout(root: Path, url: str, commit: str, submodules: Sequence[str] = ()) -> Result[None, CommandFailed]:
    """Ensure a checkout of a repository at a commit exists at the root with its submodules, fetching with depth one on a HEAD mismatch."""
    if not (root / ".git").is_dir():
        await anyio.Path(root).mkdir(parents=True, exist_ok=True)
        match await run_each([["git", "init", "--quiet"], ["git", "remote", "add", "origin", url]], root):
            case Result(tag="error", error=failure):
                return Error(failure)
            case Result():
                pass
    match await run(["git", "rev-parse", "HEAD"], root, capture=True):
        case Result(ok=head) if head.strip() == commit:
            fetch: list[list[str]] = []
        case _:
            fetch = [["git", "fetch", "--depth", "1", "origin", commit], ["git", "checkout", "--quiet", commit]]
    update = [["git", "submodule", "update", "--init", "--depth", "1", "--recommend-shallow", "--", *submodules]] if submodules else []
    return await run_each([*fetch, *update], root)


# --- [ARCHIVES] -------------------------------------------------------------------------


@stamina.retry(on=httpx.TransportError, attempts=3)
async def _transfer(client: httpx.AsyncClient, url: str, target: Path) -> str:
    """Stream a URL into the target file and return the SHA-256 digest of its bytes."""
    digest = hashlib.sha256()
    async with client.stream("GET", url) as response, await anyio.open_file(target, "wb") as blob:
        _ = response.raise_for_status()
        async for chunk in response.aiter_bytes():
            digest.update(chunk)
            _ = await blob.write(chunk)
    return digest.hexdigest()


async def pinned_file(client: httpx.AsyncClient, url: str, sha256: str, destination: Path) -> Result[Path, DownloadFailed]:
    """Fetch a pinned file under a temporary name, verify its digest, and place it at the destination, an existing file is kept."""
    if await anyio.Path(destination).is_file():
        return Ok(destination)
    await anyio.Path(destination.parent).mkdir(parents=True, exist_ok=True)
    temporary = destination.with_name(f"{destination.name}.partial")
    try:
        digest = await _transfer(client, url, temporary)
    except httpx.HTTPError as error:
        temporary.unlink(missing_ok=True)
        return Error(DownloadFailed(url, str(error)))
    if digest != sha256:
        temporary.unlink()
        return Error(DownloadFailed(url, f"the bytes hashed to {digest} against the pin {sha256}, the file was removed"))
    return Ok(temporary.replace(destination))


def _untar(archive: Path, destination: Path, member: str) -> None:
    with tarfile.open(archive) as tar:
        tar.extractall(destination, [entry for entry in tar.getmembers() if re.search(member, entry.name)], filter="data")


def _unzip(archive: Path, destination: Path, member: str) -> None:
    with zipfile.ZipFile(archive) as zipped:
        for info in zipped.infolist():
            _ = zipped.extract(info, destination) if re.search(member, info.filename) else None


def unpack(archive: Path, fmt: str, destination: Path, member: str = "") -> Result[None, PinMismatch]:
    """Extract a tarball or zip archive with the standard library, limited to the members matching the pattern."""
    try:
        (_unzip if fmt == "zip" else _untar)(archive, destination, member)
    except (OSError, tarfile.TarError, zipfile.BadZipFile) as error:
        return Error(PinMismatch(f"Archive {archive}", f"does not read as {fmt}, {error}"))
    return Ok(None)


async def _extract(archive: Path, asset: Asset, destination: Path, host: Rid, cwd: Path) -> Result[None, Failure]:
    """Unpack an archive into the destination with the tool its format needs on the host."""
    match asset.format, system(host):
        case ("tar" | "zip", _):
            return unpack(archive, asset.format, destination)
        case ("pkg", "osx"):
            expanded = destination / "expanded"  # pkgutil creates the destination and rejects an existing one
            match await run(["pkgutil", "--expand-full", str(archive), str(expanded)], cwd):
                case Result(tag="error", error=failure):
                    return Error(failure)
                case Result():
                    for prefix in sorted(expanded.glob("*.pkg/Payload/usr/local")):
                        _ = shutil.copytree(prefix, destination, symlinks=True, dirs_exist_ok=True)
                    shutil.rmtree(expanded)
                    return Ok(None)
        case ("nsis", "win"):  # NSIS takes /D last and unquoted
            return (await run([str(archive), "/S", f"/D={destination}"], cwd)).map(lambda _: None)
        case (name, host_system):
            return Error(HostUnsupported(f"archive format {name} needs a tool the {host_system} host lacks"))


async def pinned_tree(
    space: Workspace, client: httpx.AsyncClient, library: str, manifest: ReleaseManifest, asset: Asset, rid: Rid
) -> Result[Path, Failure]:
    """Ensure the pinned asset of a library is downloaded, verified, and extracted for a rid, and return the pinned path inside the tree."""
    root = space.cache / library / manifest.version
    tree = root / rid
    target = tree / asset.path
    if target.exists():
        return Ok(target)
    match await pinned_file(client, manifest.url.format(version=manifest.version, asset=asset.name), asset.sha256, root / asset.name):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=archive):
            pass
    staging = root / f"{rid}.staging"
    shutil.rmtree(staging, ignore_errors=True)
    shutil.rmtree(tree, ignore_errors=True)
    staging.mkdir(parents=True)
    match await _extract(archive, asset, staging, space.host, space.root):
        case Result(tag="error", error=extract_error):
            return Error(extract_error)
        case Result():
            _ = staging.rename(tree)
            return Ok(target) if target.exists() else Error(FileMissing(tree, asset.path))


async def release_tool(space: Workspace, client: httpx.AsyncClient, library: str) -> Result[Path, Failure]:
    """Ensure the pinned release of a host tool exists for the host and return its executable."""
    match read_manifest(space.manifests / library / "release.json", ReleaseManifest):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=manifest) if space.host not in manifest.runtimes:
            return Error(HostUnsupported(f"the manifest of {library} pins no asset for {space.host}"))
        case Result(ok=manifest):
            return await pinned_tree(space, client, library, manifest, manifest.runtimes[space.host], space.host)


async def extension_archives(
    space: Workspace, client: httpx.AsyncClient, library: str, manifest: ExtensionManifest, rid: Rid
) -> Result[list[Path], DownloadFailed]:
    """Ensure every pinned extension archive of a rid exists under the cache and return the files in name order."""
    root = space.cache / library / manifest.version / rid
    archives: list[Path] = []
    for name, digests in sorted(manifest.extensions.items()):
        url = manifest.url.format(version=manifest.version, platform=manifest.platforms[rid], extension=name)
        match await pinned_file(client, url, digests[rid], root / f"{name}.duckdb_extension.gz"):
            case Result(tag="error", error=failure):
                return Error(failure)
            case Result(ok=archive):
                archives.append(archive)
    return Ok(archives)


# --- [VCPKG] ----------------------------------------------------------------------------


def _baseline(space: Workspace) -> Result[str, PinMismatch]:
    """Return the vcpkg baseline every port manifest pins."""
    baselines: set[str] = set()
    for path in sorted(space.manifests.glob("*/vcpkg.json")):
        match read_manifest(path, PortManifest):
            case Result(tag="error", error=failure):
                return Error(failure)
            case Result(ok=manifest):
                baselines.add(manifest.baseline)
    match sorted(baselines):
        case [baseline]:
            return Ok(baseline)
        case found:
            return Error(PinMismatch("Port manifests", f"pin the baselines {', '.join(found)}, pin one builtin-baseline in every vcpkg.json"))


async def _vcpkg(space: Workspace) -> Result[Path, Failure]:
    """Ensure the vcpkg checkout at the pinned baseline is bootstrapped and return its executable."""
    root = space.cache / "vcpkg"
    windows = system(space.host) == "win"
    exe = root / ("vcpkg.exe" if windows else "vcpkg")
    match _baseline(space):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=baseline):
            pass
    match await checkout(root, _VCPKG_URL, baseline):
        case Result(tag="error", error=checkout_error):
            return Error(checkout_error)
        case Result() if exe.exists():
            return Ok(exe)
        case Result():
            bootstrap = root / ("bootstrap-vcpkg.bat" if windows else "bootstrap-vcpkg.sh")
            return (await run([str(bootstrap), "-disableMetrics"], root)).map(lambda _: exe)


async def _pkg_config(space: Workspace, vcpkg: Path) -> Result[Mapping[str, str], CommandFailed]:
    """Build pkgconf as a host tool where vcpkg finds no pkg-config and return the environment naming it."""
    if system(space.host) == "win":
        return Ok(space.environment)
    host_tools = space.cache / "vcpkg-hosttools"
    name, _, arch = space.host.partition("-")
    triplet = f"{arch}-{name}"
    tool = host_tools / "installed" / triplet / "tools" / "pkgconf" / "pkgconf"
    if not tool.exists():
        # The pkgconf port validates its own pc file with pkg-config, absent on this machine
        overlay = host_tools / "overlay"
        _ = shutil.copytree(vcpkg.parent / "ports" / "pkgconf", overlay / "pkgconf", dirs_exist_ok=True)
        portfile = overlay / "pkgconf" / "portfile.cmake"
        _ = portfile.write_text(portfile.read_text().replace("vcpkg_fixup_pkgconfig()", "vcpkg_fixup_pkgconfig(SKIP_CHECK)"))
        install = [str(vcpkg), "install", f"pkgconf:{triplet}", "--overlay-ports", str(overlay), "--x-install-root", str(host_tools / "installed")]
        match await run([*install, "--no-print-usage"], space.root):
            case Result(tag="error", error=failure):
                return Error(failure)
            case Result():
                pass
    return Ok({**space.environment, "PKG_CONFIG": str(tool)})


async def native_build_tools(space: Workspace) -> Result[ToolSet, Failure]:
    """Ensure vcpkg, its binary cache and downloads directories, and the pkgconf host tool exist and return the set."""
    space.binary_cache.mkdir(parents=True, exist_ok=True)
    space.downloads.mkdir(parents=True, exist_ok=True)
    match await _vcpkg(space):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=vcpkg):
            return (await _pkg_config(space, vcpkg)).map(lambda env: ToolSet(vcpkg, env))


# --- [TOOLS] ----------------------------------------------------------------------------


def _tool_link(space: Workspace, library: str, pinned: Path) -> Path:
    """Link the folder holding a pinned executable at the version-free tools/<library>, the path consumers read, and return the link."""
    link = space.cache / "tools" / library
    link.parent.mkdir(parents=True, exist_ok=True)
    link.unlink(missing_ok=True)
    link.symlink_to(os.path.relpath(pinned.parent, link.parent), target_is_directory=True)  # Relative, the tools tree restores from a cache
    return link


# --- [CLI] ------------------------------------------------------------------------------


def message(failure: Failure) -> str:
    """Render a failure as one sentence naming what happened, the cause, and the action."""
    match failure:
        case CommandFailed(command=command, detail=detail):
            return f"Command {' '.join(command)} failed, {detail}, read its output for the cause"
        case DownloadFailed(url=url, detail=detail):
            return f"Download of {url} failed, {detail}, check the network and the pin against the release"
        case PinMismatch(subject=subject, detail=detail):
            return f"{subject} {detail}, correct the pin or its source"
        case HostUnsupported(detail=detail):
            return f"Host is unsupported, {detail}, run the command through mise from the repository checkout on a supported host"
        case FileMissing(directory=directory, name=name):
            return f"No file matching {name} under {directory}, the step producing it wrote nothing there, read its output and the manifest"
        case NoMutants(language=language, report=report):
            return f"Mutation run of {language} discovered zero mutants, {report} holds none, point the mutate globs of its Stryker configuration at source with tests"


def exit_code[T](report: Callable[[T], None]) -> Callable[[Result[T, Failure]], int]:
    """Return the result action that reports an ok value, logs a failure, and yields the exit code."""

    def action(result: Result[T, Failure]) -> int:
        match result:
            case Result(tag="error", error=failure):
                _log.error(message(failure))
                return 1
            case Result(ok=value):
                report(value)
                return 0

    return action


async def _provision(start: Path, host_system: str, host_machine: str) -> Result[list[Path], Failure]:
    """Place every pinned tool and archive the host needs and return the placed paths."""
    match workspace(start, host_system, host_machine):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=space):
            pass
    match await native_build_tools(space):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=tools):
            placed = [tools.vcpkg]
    async with http_client() as client:
        for library in ("energyplus", "ktx", "sqlitevec"):
            match await release_tool(space, client, library):
                case Result(tag="error", error=failure):
                    return Error(failure)
                case Result(ok=path):
                    placed.append(_tool_link(space, library, path) if library in _HOST_TOOLS else path)
        match read_manifest(space.manifests / "duckdbextensions" / "extensions.json", ExtensionManifest):
            case Result(tag="error", error=failure):
                return Error(failure)
            case Result(ok=manifest):
                pass
        match await extension_archives(space, client, "duckdbextensions", manifest, space.host):
            case Result(tag="error", error=failure):
                return Error(failure)
            case Result(ok=archives):
                placed.extend(archives)
    # The user git config can be a read-only file a machine profile writes
    return (await run(["git", "lfs", "install", "--local"], space.root)).map(lambda _: placed)


def _report(placed: list[Path]) -> None:
    _log.info("provisioned", placed=[str(path) for path in placed])


_app.result_action = (exit_code(_report), "sys_exit")


@_app.default
def main() -> Result[list[Path], Failure]:
    """Provision the pinned build tools, the EnergyPlus, ktx, and sqlite-vec releases under tools, the DuckDB extension archives, and the git-lfs filters for the host."""
    return anyio.run(_provision, Path(__file__).resolve(), platform.system(), platform.machine())


if __name__ == "__main__":
    _app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Rid",
    "Failure",
    "PinMismatch",
    "HostUnsupported",
    "FileMissing",
    "NoMutants",
    "PortManifest",
    "ReleaseManifest",
    "ExtensionManifest",
    "Build",
    "SourceManifest",
    "Workspace",
    "ToolSet",
    "system",
    "repository_root",
    "workspace",
    "read_manifest",
    "http_client",
    "run",
    "run_each",
    "checkout",
    "unpack",
    "pinned_tree",
    "extension_archives",
    "native_build_tools",
    "exit_code",
    "main",
]
