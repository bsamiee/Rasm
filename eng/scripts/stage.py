"""Stage a pinned native library, and its binding sources when it has them, for a runtime identifier for packing.

Each CI host stages its own runtime identifier under .artifacts/native/<library>/stage, then one job collects the staged trees and runs the pack target alone.
On macOS every staged closure is rewritten to @loader_path and re-signed ad hoc, and dotnet default probing then loads the set from the output directory.
A library with no asset or build pinned for the runtime identifier stages nothing and reports the unsupported outcome.
"""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Awaitable, Callable
from functools import partial
import gzip
import os
from pathlib import Path
import platform
import re
import shutil
import sys
from typing import Literal

import anyio
import cyclopts
from expression import Error, Ok, Result
import httpx
import msgspec
import structlog

from eng.scripts.gen_gmsh_bindings import generate
from eng.scripts.provision import (
    Build,
    checkout,
    exit_code,
    extension_archives,
    ExtensionManifest,
    Failure,
    FileMissing,
    http_client,
    native_build_tools,
    PinMismatch,
    pinned_tree,
    PortManifest,
    read_manifest,
    ReleaseManifest,
    Rid,
    run,
    run_each,
    SourceManifest,
    system,
    ToolSet,
    unpack,
    Workspace,
    workspace,
)

# --- [TYPES] ----------------------------------------------------------------------------

type Library = Literal["blosc2", "duckdbextensions", "emgucv", "ffmpeg", "gmsh", "lcms2", "sqlitevec", "z3"]


class Staged(msgspec.Struct, frozen=True, gc=False):
    """Files staged for a runtime identifier."""

    library: str
    rid: str
    path: Path


class Unsupported(msgspec.Struct, frozen=True, gc=False):
    """Manifest pins no asset or build for the runtime identifier, nothing is staged."""

    library: str
    rid: str


type Outcome = Staged | Unsupported
type _Stage = Callable[[Workspace, ToolSet, httpx.AsyncClient, Rid], Awaitable[Result[Outcome, Failure]]]
type _Managed = Callable[[Workspace, ToolSet, str, list[str]], Awaitable[Result[Path, Failure]]]


class _Port(msgspec.Struct, frozen=True, gc=False):
    """Vcpkg-built library with the port stems and the staging, renaming, overlay, and binding options of its port."""

    stem: str
    windows_stem: str | None = None
    closure: bool = False
    canonical: bool = False
    overlay: Callable[[Path, Path], Result[list[str], PinMismatch]] | None = None
    managed: _Managed | None = None


class _Target(msgspec.Struct, frozen=True, gc=False):
    """Vcpkg triplet, installed library directory, glob pattern, and canonical file name of a port for a rid."""

    triplet: str
    lib_dir: str
    pattern: str
    file_name: str


# --- [CONSTANTS] ------------------------------------------------------------------------

_PORT_VERSION_FIELDS = ("version", "version-semver", "version-string", "version-date")
_GMSH_PORT_DISABLED = ("-DENABLE_MESH=OFF", "-DENABLE_EIGEN=OFF")

_log = structlog.get_logger(__name__)
_app = cyclopts.App(name="stage")

# --- [VCPKG] ----------------------------------------------------------------------------


def _target(rid: Rid, port: _Port) -> _Target:
    """Return the dynamic-library vcpkg target of a port for a rid, a closure target globs every built library."""
    name, _, arch = rid.partition("-")
    match name:
        case "win":
            base = port.windows_stem or port.stem
            return _Target(f"{arch}-windows", "bin", "*.dll" if port.closure else f"{base}*.dll", f"{base}.dll")
        case "osx":
            return _Target(f"{arch}-osx-dynamic", "lib", "*.dylib" if port.closure else f"lib{port.stem}*.dylib", f"lib{port.stem}.dylib")
        case _:
            return _Target(f"{arch}-linux-dynamic", "lib", "*.so*" if port.closure else f"lib{port.stem}.so*", f"lib{port.stem}.so")


def _vcpkg_args(manifest_root: Path, install_root: Path, triplet: str, *extra: str) -> list[str]:
    return ["--triplet", triplet, "--x-manifest-root", str(manifest_root), "--x-install-root", str(install_root), *extra, "--no-print-usage"]


def _pinned_version(space: Workspace, tools: ToolSet, library: str) -> Result[str, PinMismatch]:
    """Return the manifest version-string after checking the port at the baseline declares the same version."""
    match read_manifest(space.manifests / library / "vcpkg.json", PortManifest):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=manifest):
            pass
    dependency = manifest.dependencies[0]
    port = tools.vcpkg.parent / "ports" / (dependency if isinstance(dependency, str) else dependency.name) / "vcpkg.json"
    fields = msgspec.json.decode(port.read_bytes(), type=dict[str, object])
    match [value for field in _PORT_VERSION_FIELDS if isinstance(value := fields.get(field), str)]:
        case [version, *_] if version == manifest.version:
            return Ok(version)
        case [version, *_]:
            return Error(PinMismatch(f"Manifest of {library}", f"pins version-string {manifest.version} against the baseline port version {version}"))
        case _:
            return Error(PinMismatch(f"Port manifest {port}", "declares no version field"))


async def _vcpkg_install(tools: ToolSet, space: Workspace, work: Path, target: _Target, args: list[str]) -> Result[list[Path], Failure]:
    """Run vcpkg install and return the built real library files for a target, symlinks excluded."""
    match await run([str(tools.vcpkg), "install", *args], space.root, tools.env):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result():
            source_dir = work / "installed" / target.triplet / target.lib_dir
            built = [path for path in sorted(source_dir.glob(target.pattern)) if path.is_file() and not path.is_symlink()]
            return Ok(built) if built else Error(FileMissing(source_dir, target.pattern))


# --- [STAGING] --------------------------------------------------------------------------


def _stage_dir(work: Path, rid: Rid) -> Path:
    return work / "stage" / "runtimes" / rid / "native"


def _stage_library(source: Path, work: Path, rid: Rid, file_name: str) -> Path:
    destination = _stage_dir(work, rid) / file_name
    destination.parent.mkdir(parents=True, exist_ok=True)
    return Path(shutil.copy(source, destination))


def _canonical(port: _Port, target: _Target, name: str) -> str:
    """Return the canonical file name for the port library and the unchanged name for its dependencies."""
    return target.file_name if port.canonical and name.removeprefix("lib").startswith(port.stem) else name


async def _install_names(path: Path, cwd: Path) -> Result[list[str], Failure]:
    """Return the install name recorded in a dylib followed by the install names of its dependencies."""
    match await run(["otool", "-L", str(path)], cwd, capture=True):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=text):
            return Ok([line.split()[0] for line in text.splitlines()[1:] if line.strip()])


async def _stage_closure(built: list[Path], work: Path, rid: Rid, cwd: Path, rename: Callable[[str], str]) -> Result[Path, Failure]:
    """Copy every built library into the staged runtimes layout, relinked to @loader_path and signed ad hoc on macOS."""
    destination = _stage_dir(work, rid)
    shutil.rmtree(destination, ignore_errors=True)
    destination.mkdir(parents=True)
    linked: dict[Path, list[str]] = {}
    for path in built:
        match await _install_names(path, cwd) if system(rid) == "osx" else Ok([path.name]):
            case Result(tag="error", error=failure):
                return Error(failure)
            case Result(ok=names):
                copied = Path(shutil.copy(path, destination / rename(Path(names[0]).name)))
                await anyio.Path(copied).chmod(0o755)
                linked[copied] = names
    if system(rid) != "osx":
        return Ok(destination)
    staged = {path.name for path in linked}
    for path, names in linked.items():
        changes = [argument for name in names if Path(name).name in staged for argument in ("-change", name, f"@loader_path/{Path(name).name}")]
        relink = [["install_name_tool", "-id", f"@loader_path/{path.name}", *changes, str(path)], ["codesign", "--force", "--sign", "-", str(path)]]
        match await run_each(relink, cwd):
            case Result(tag="error", error=failure):
                return Error(failure)
            case Result():
                pass
    return Ok(destination)


# --- [PORTS] ----------------------------------------------------------------------------


def _search(pattern: str, text: str, where: str) -> Result[str, PinMismatch]:
    match re.search(pattern, text):
        case None:
            return Error(PinMismatch(where, f"holds no match for the pattern {pattern!r}"))
        case found:
            return Ok(found.group(1))


def _managed_dir(space: Workspace, library: str) -> Path:
    """Return an empty staged managed directory for a library."""
    managed = space.artifacts / library / "stage" / "managed"
    shutil.rmtree(managed, ignore_errors=True)
    managed.mkdir(parents=True)
    return managed


async def _source_root(space: Workspace, tools: ToolSet, library: str, archive: str, member: str, download_args: list[str]) -> Result[Path, Failure]:
    """Unpack the members matching the pattern from the source archive the port pins and return the source root."""
    path = space.downloads / archive
    if not path.exists():  # Binary-cache hits build nothing and download no source
        match await run([str(tools.vcpkg), "install", "--only-downloads", *download_args], space.root, tools.env):
            case Result(tag="error", error=failure):
                return Error(failure)
            case Result():
                pass
    source = space.artifacts / library / "src"
    shutil.rmtree(source, ignore_errors=True)
    return unpack(path, "tar", source, member).map(lambda _: next(source.iterdir()))


async def _z3_managed(space: Workspace, tools: ToolSet, version: str, download_args: list[str]) -> Result[Path, Failure]:
    """Stage the z3 binding sources and generate Native.cs and Enumerations.cs beside them."""
    portfile = (tools.vcpkg.parent / "ports" / "z3" / "portfile.cmake").read_text()
    match _search(r"REPO\s+(\S+)", portfile, "Portfile of z3").map2(
        _search(r"REF\s+(\S+)", portfile, "Portfile of z3"), lambda repo, ref: (repo, ref)
    ):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=(repo, ref)):
            pass
    archive = f"{repo.replace('/', '-')}-{ref.replace('${VERSION}', version)}.tar.gz"
    match await _source_root(space, tools, "z3", archive, r"/(scripts|src/api)/|^[^/]+/CMakeLists\.txt$", download_args):
        case Result(tag="error", error=source_error):
            return Error(source_error)
        case Result(ok=root):
            pass
    match _search(r"set\(Z3_API_HEADER_FILES_TO_SCAN\s+([^)]+)\)", (root / "CMakeLists.txt").read_text(), "CMakeLists.txt of z3"):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=names):
            pass
    headers = [str(root / "src" / "api" / name) for name in names.split()]
    managed = _managed_dir(space, "z3")
    for path in sorted((root / "src" / "api" / "dotnet").glob("*.cs")):
        _ = shutil.copy(path, managed)
    scripts = [
        [sys.executable, str(root / "scripts" / script), *headers, "--dotnet-output-dir", str(managed)]
        for script in ("update_api.py", "mk_consts_files.py")
    ]
    return (await run_each(scripts, space.root)).map(lambda _: managed)


async def _gmsh_managed(space: Workspace, tools: ToolSet, version: str, download_args: list[str]) -> Result[Path, Failure]:
    match await _source_root(space, tools, "gmsh", f"gmsh-{version}-source.tgz", r"^[^/]+/(api/|CMakeLists\.txt$)", download_args):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=root):
            managed = _managed_dir(space, "gmsh")
            return generate(root / "api", managed, version).map(lambda _: managed)


def _gmsh_overlay(vcpkg: Path, work: Path) -> Result[list[str], PinMismatch]:
    """Copy the pinned gmsh port with its mesh module and bundled Eigen re-enabled and return the overlay arguments."""
    root = work / "overlay"
    shutil.rmtree(root, ignore_errors=True)
    portfile = Path(shutil.copytree(vcpkg.parent / "ports" / "gmsh", root / "gmsh")) / "portfile.cmake"
    text = portfile.read_text()
    match [flag for flag in _GMSH_PORT_DISABLED if flag not in text]:
        case [flag, *_]:
            return Error(PinMismatch("Portfile of gmsh", f"holds no {flag} flag"))
        case _:
            for flag in _GMSH_PORT_DISABLED:
                text = text.replace(flag, flag.replace("=OFF", "=ON"))
            _ = portfile.write_text(text)
            return Ok(["--overlay-ports", str(root)])


async def _stage_port(library: str, port: _Port, space: Workspace, tools: ToolSet, _client: httpx.AsyncClient, rid: Rid) -> Result[Outcome, Failure]:
    """Build the manifest with vcpkg, generate binding sources when the port declares them, and stage the library files."""
    work = space.artifacts / library
    target = _target(rid, port)
    overlay = port.overlay(tools.vcpkg, work) if port.overlay is not None else Ok([])
    match _pinned_version(space, tools, library).map2(overlay, lambda version, extra: (version, extra)):
        case Result(tag="error", error=version_error):
            return Error(version_error)
        case Result(ok=(version, extra)):
            pass
    match await _vcpkg_install(tools, space, work, target, _vcpkg_args(space.manifests / library, work / "installed", target.triplet, *extra)):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=built):
            pass
    # An installed port downloads nothing again, the source download runs against its own empty install root
    download_args = _vcpkg_args(space.manifests / library, work / "sources" / "installed", target.triplet, *extra)
    match await port.managed(space, tools, version, download_args) if port.managed is not None else Ok(work):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result():
            pass
    if not port.closure:
        return Ok(Staged(library, rid, _stage_library(built[0], work, rid, target.file_name)))
    return (await _stage_closure(built, work, rid, space.root, partial(_canonical, port, target))).map(lambda staged: Staged(library, rid, staged))


# --- [RELEASES] -------------------------------------------------------------------------


async def _emgucv_build(space: Workspace, tools: ToolSet, src: Path, build: Build, rid: Rid) -> Result[Path, Failure]:
    """Run the manifest's CMake steps for the rid in order and return the built library."""
    match await run([str(tools.vcpkg), "fetch", "cmake"], space.root, capture=True):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=text):
            cmake = text.strip().splitlines()[-1]
    match await run(["xcrun", "--sdk", "macosx", "--show-sdk-path"], space.root, capture=True) if system(rid) == "osx" else Ok(""):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=sdk):
            values = {"source": str(src), "install": str(src / build.install), "sdk": sdk.strip()}
    for step in build.steps:
        flags = [flag.format(**values) for flag in (*build.flags, *step.flags)]
        configure = [cmake, "-S", str(src / step.source), "-B", str(src / step.build), *flags]
        compile_step = [cmake, "--build", str(src / step.build), "--target", step.target, "--parallel", str(os.cpu_count() or 1)]
        match await run_each([configure, compile_step], space.root):
            case Result(tag="error", error=failure):
                return Error(failure)
            case Result():
                pass
    return Ok(src / build.library)


async def _stage_emgucv(space: Workspace, tools: ToolSet, _client: httpx.AsyncClient, rid: Rid) -> Result[Outcome, Failure]:
    """Reuse or build the commit-keyed Emgu CV library and stage it for the rid."""
    match read_manifest(space.manifests / "emgucv" / "source.json", SourceManifest):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=manifest) if rid not in manifest.runtimes:
            return Ok(Unsupported("emgucv", rid))
        case Result(ok=manifest):
            build = manifest.runtimes[rid]
    file_name = Path(build.library).name
    artifact = space.cache / "emgucv" / "artifacts" / manifest.commit / rid / file_name
    if not artifact.is_file():
        src = space.cache / "emgucv" / "src"
        match await checkout(src, manifest.url, manifest.commit, manifest.submodules):
            case Result(tag="error", error=checkout_error):
                return Error(checkout_error)
            case Result():
                pass
        match await _emgucv_build(space, tools, src, build, rid):
            case Result(tag="error", error=build_error):
                return Error(build_error)
            case Result(ok=library):
                artifact.parent.mkdir(parents=True, exist_ok=True)
                _ = shutil.move(
                    shutil.copy(library, artifact.with_name(f"{artifact.name}.partial")), artifact
                )  # The cached path never holds a partial copy
    return Ok(Staged("emgucv", rid, _stage_library(artifact, space.artifacts / "emgucv", rid, file_name)))


async def _stage_duckdb_extensions(space: Workspace, _tools: ToolSet, client: httpx.AsyncClient, rid: Rid) -> Result[Outcome, Failure]:
    """Decompress the pinned DuckDB extension archives into the extension directory layout under contentFiles."""
    match read_manifest(space.manifests / "duckdbextensions" / "extensions.json", ExtensionManifest):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=manifest) if rid not in manifest.platforms:
            return Ok(Unsupported("duckdbextensions", rid))
        case Result(ok=manifest):
            pass
    match await extension_archives(space, client, "duckdbextensions", manifest, rid):
        case Result(tag="error", error=fetch_error):
            return Error(fetch_error)
        case Result(ok=archives):
            pass
    stage = space.artifacts / "duckdbextensions" / "stage"
    destination = stage / "contentFiles" / "duckdb_extensions" / f"v{manifest.version}" / manifest.platforms[rid]
    shutil.rmtree(stage, ignore_errors=True)
    destination.mkdir(parents=True)
    for archive in archives:
        with gzip.open(archive, "rb") as compressed, (destination / archive.name.removesuffix(".gz")).open("wb") as extension:
            shutil.copyfileobj(compressed, extension)
    return Ok(Staged("duckdbextensions", rid, destination))


async def _stage_sqlite_vec(space: Workspace, _tools: ToolSet, client: httpx.AsyncClient, rid: Rid) -> Result[Outcome, Failure]:
    match read_manifest(space.manifests / "sqlitevec" / "release.json", ReleaseManifest):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=manifest) if rid not in manifest.runtimes:
            return Ok(Unsupported("sqlitevec", rid))
        case Result(ok=manifest):
            pass
    work = space.artifacts / "sqlitevec"
    shutil.rmtree(work / "stage", ignore_errors=True)
    loadable = await pinned_tree(space, client, "sqlitevec", manifest, manifest.runtimes[rid], rid)
    return loadable.map(lambda path: Staged("sqlitevec", rid, _stage_library(path, work, rid, path.name)))


# --- [CLI] ------------------------------------------------------------------------------

_LIBRARIES: dict[Library, _Stage] = {
    "blosc2": partial(_stage_port, "blosc2", _Port("blosc2", windows_stem="libblosc2", closure=True, canonical=True)),
    "duckdbextensions": _stage_duckdb_extensions,
    "emgucv": _stage_emgucv,
    "ffmpeg": partial(_stage_port, "ffmpeg", _Port("ffmpeg", closure=True)),
    "gmsh": partial(_stage_port, "gmsh", _Port("gmsh", overlay=_gmsh_overlay, managed=_gmsh_managed)),
    "lcms2": partial(_stage_port, "lcms2", _Port("lcms2")),
    "sqlitevec": _stage_sqlite_vec,
    "z3": partial(_stage_port, "z3", _Port("z3", windows_stem="libz3", managed=_z3_managed)),
}


async def _stage(libraries: list[Library], rid: Rid | None, start: Path, host_system: str, host_machine: str) -> Result[list[Outcome], Failure]:
    match workspace(start, host_system, host_machine):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=space):
            pass
    match await native_build_tools(space):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=tools):
            outcomes: list[Outcome] = []
    async with http_client() as client:
        for library in libraries:
            match await _LIBRARIES[library](space, tools, client, rid or space.host):
                case Result(tag="error", error=failure):
                    return Error(failure)
                case Result(ok=outcome):
                    outcomes.append(outcome)
    return Ok(outcomes)


def _report(outcomes: list[Outcome]) -> None:
    for outcome in outcomes:
        match outcome:
            case Staged(library=library, rid=rid, path=path):
                _log.info("staged", library=library, rid=rid, path=str(path))
            case Unsupported(library=library, rid=rid):
                _log.info("unsupported", library=library, rid=rid, detail="the manifest pins no asset or build for the rid, nothing staged")


_app.result_action = (exit_code(_report), "sys_exit")


@_app.default
def main(library: Library | None = None, rid: Rid | None = None) -> Result[list[Outcome], Failure]:
    """Stage the named library, or every library, for the given or host runtime identifier."""
    libraries = [library] if library is not None else list(_LIBRARIES)
    return anyio.run(_stage, libraries, rid, Path(__file__).resolve(), platform.system(), platform.machine())


if __name__ == "__main__":
    _app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Library", "Outcome", "Staged", "Unsupported", "main"]
