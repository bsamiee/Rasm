"""Stage a pinned native library, and its binding sources when it has them, for a runtime identifier for packing.

Each CI host stages its own runtime identifier under .artifacts/native/<library>/stage, then one job collects the staged trees and runs the pack target alone.
On macOS every staged closure is rewritten to @loader_path and re-signed ad hoc, and dotnet default probing then loads the set from the output directory.
"""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Awaitable, Callable
from functools import partial
import gzip
import os
from pathlib import Path
import re
import shutil
import sys
import tarfile

import anyio
import cyclopts
import msgspec
import structlog

from eng.scripts.gen_gmsh_bindings import generate
from eng.scripts.provision import (
    cmake_tool,
    duckdb_extension_archives,
    duckdb_platform,
    emgucv_pins,
    emgucv_source,
    host_rid,
    native_build_tools,
    REPO_ROOT,
    Rid,
    run,
    sqlite_vec_archive,
    stage_closure,
    stage_dir,
    stage_library,
    vcpkg_args,
    vcpkg_install,
    vcpkg_target,
)

# --- [TYPES] ----------------------------------------------------------------------------

type _Stage = Callable[[Rid], Awaitable[Path]]
type _Managed = Callable[[Path, str, list[str]], Awaitable[Path]]


class _Dependency(msgspec.Struct, frozen=True, gc=False):
    """Vcpkg manifest dependency in its object form."""

    name: str


class _PortManifest(msgspec.Struct, frozen=True, gc=False):
    """Committed vcpkg manifest pinning one port and the package version its build produces."""

    version: str = msgspec.field(name="version-string")
    dependencies: list[str | _Dependency] = msgspec.field(name="dependencies")


class _Port(msgspec.Struct, frozen=True, gc=False):
    """Vcpkg-built library: port stem, Windows stem, closure staging, canonical renaming, overlay, and binding generation."""

    stem: str
    windows_stem: str | None = None
    closure: bool = False
    canonical: bool = False
    overlay: Callable[[Path], list[str]] | None = None
    managed: _Managed | None = None


# --- [CONSTANTS] ------------------------------------------------------------------------

_MANIFEST_ROOT = REPO_ROOT / "eng" / "native"
_WORK_ROOT = REPO_ROOT / ".artifacts" / "native"
_PORT_VERSION_FIELDS = ("version", "version-semver", "version-string", "version-date")
_GMSH_PORT_DISABLED = ("-DENABLE_MESH=OFF", "-DENABLE_EIGEN=OFF")
_SQLITE_VEC_FILE_NAMES: dict[Rid, str] = {"osx-arm64": "vec0.dylib", "linux-x64": "vec0.so", "linux-arm64": "vec0.so", "win-x64": "vec0.dll"}
_EMGUCV_BUILD_DIR = "build_arm64"  # Upstream's in-tree build folder name for the eigen, hdf5, and opencv steps
_EMGUCV_HDF5_FLAGS = (
    "-DBUILD_SHARED_LIBS:BOOL=OFF",
    "-DBUILD_TESTING:BOOL=FALSE",
    "-DHDF5_BUILD_EXAMPLES:BOOL=FALSE",
    "-DHDF5_BUILD_TOOLS:BOOL=FALSE",
    "-DHDF5_BUILD_UTILS:BOOL=FALSE",
)
_EMGUCV_OPENCV_FLAGS = (
    "-DOPENCV_FORCE_3RDPARTY_BUILD:BOOL=TRUE",
    "-DBUILD_PERF_TESTS=FALSE",
    "-DBUILD_TESTS:BOOL=FALSE",
    "-DBUILD_DOCS:BOOL=FALSE",
    "-DBUILD_JPEG:BOOL=TRUE",
    "-DBUILD_ZLIB:BOOL=TRUE",
    "-DBUILD_OPENEXR:BOOL=TRUE",
    "-DBUILD_PNG:BOOL=TRUE",
    "-DBUILD_TIFF:BOOL=TRUE",
    "-DWITH_OPENVINO:BOOL=FALSE",
    "-DWITH_WEBP:BOOL=OFF",
    "-DWITH_IPP:BOOL=OFF",
    "-DWITH_CUDA:BOOL=OFF",
    "-DWITH_OBSENSOR:BOOL=OFF",
    "-DWITH_TESSERACT:BOOL=OFF",
    "-DWITH_LAPACK:BOOL=OFF",
    "-DBUILD_opencv_ts:BOOL=OFF",
    "-DBUILD_opencv_java:BOOL=OFF",
    "-DBUILD_opencv_python2:BOOL=OFF",
    "-DBUILD_opencv_python3:BOOL=OFF",
    "-DBUILD_opencv_apps:BOOL=OFF",
    "-DBUILD_opencv_freetype:BOOL=FALSE",
    "-DBUILD_SHARED_LIBS:BOOL=OFF",
    "-DEMGU_CV_WITH_TESSERACT:BOOL=TRUE",
    "-DEMGU_CV_WITH_FREETYPE:BOOL=FALSE",
    "-DCMAKE_IGNORE_PREFIX_PATH:STRING=/usr/local;/opt/homebrew",  # Upstream ignores the runner's /usr/local, arm64 Homebrew installs to /opt/homebrew
)

_log = structlog.get_logger(__name__)
_app = cyclopts.App(name="stage")

# --- [PORTS] ----------------------------------------------------------------------------


def _work(library: str) -> Path:
    """Return the work directory of a library."""
    return _WORK_ROOT / library


def _search(pattern: str, text: str, where: str) -> str:
    """Return the first group of the pattern in the text."""
    match = re.search(pattern, text)
    if match is None:
        raise SystemExit(f"pattern {pattern!r} not found in {where}")
    return match.group(1)


def _port_version(path: Path) -> str:
    """Read the version of a port manifest, whichever version field it declares."""
    fields = msgspec.json.decode(path.read_bytes(), type=dict[str, object])
    versions = [value for field in _PORT_VERSION_FIELDS if isinstance(value := fields.get(field), str)]
    if not versions:
        raise SystemExit(f"no version field in {path}")
    return versions[0]


def _pinned_version(vcpkg: Path, library: str) -> str:
    """Return the manifest version-string after checking the port at the baseline declares the same version."""
    manifest = msgspec.json.decode((_MANIFEST_ROOT / library / "vcpkg.json").read_bytes(), type=_PortManifest)
    dependency = manifest.dependencies[0]
    port = dependency if isinstance(dependency, str) else dependency.name
    version = _port_version(vcpkg.parent / "ports" / port / "vcpkg.json")
    if version != manifest.version:
        raise SystemExit(f"{library} manifest version-string {manifest.version} does not match the baseline {port} port version {version}")
    return version


def _managed_dir(library: str) -> Path:
    """Return an empty staged managed directory for a library."""
    managed = _work(library) / "stage" / "managed"
    shutil.rmtree(managed, ignore_errors=True)
    managed.mkdir(parents=True)
    return managed


def _canonical(stem: str, file_name: str) -> Callable[[str], str]:
    """Return the rename that gives the library matching the stem its canonical file name."""
    return lambda name: file_name if name.removeprefix("lib").startswith(stem) else name


async def _source_root(vcpkg: Path, library: str, archive: str, member: str, install_args: list[str]) -> Path:
    """Unpack the members matching the pattern from the source archive the port pins and return the source root."""
    path = vcpkg.parent / "downloads" / archive
    if not path.exists():  # Binary-cache hits build nothing and download no source
        await run([str(vcpkg), "install", "--only-downloads", *install_args], REPO_ROOT)
    source = _work(library) / "src"
    shutil.rmtree(source, ignore_errors=True)
    with tarfile.open(path) as tar:
        tar.extractall(source, members=[entry for entry in tar.getmembers() if re.search(member, entry.name)], filter="data")
    return next(source.iterdir())


async def _z3_managed(vcpkg: Path, version: str, install_args: list[str]) -> Path:
    """Stage the z3 binding sources and generate Native.cs and Enumerations.cs beside them."""
    portfile = (vcpkg.parent / "ports" / "z3" / "portfile.cmake").read_text()
    repo = _search(r"REPO\s+(\S+)", portfile, "the z3 portfile")
    ref = _search(r"REF\s+(\S+)", portfile, "the z3 portfile").replace("${VERSION}", version)
    archive = f"{repo.replace('/', '-')}-{ref}.tar.gz"
    root = await _source_root(vcpkg, "z3", archive, r"/(scripts|src/api)/|^[^/]+/CMakeLists\.txt$", install_args)
    names = _search(r"set\(Z3_API_HEADER_FILES_TO_SCAN\s+([^)]+)\)", (root / "CMakeLists.txt").read_text(), "the z3 CMakeLists.txt").split()
    headers = [str(root / "src" / "api" / name) for name in names]
    managed = _managed_dir("z3")
    for path in sorted((root / "src" / "api" / "dotnet").glob("*.cs")):
        _ = shutil.copy(path, managed)
    for script in ("update_api.py", "mk_consts_files.py"):
        await run([sys.executable, str(root / "scripts" / script), *headers, "--dotnet-output-dir", str(managed)], REPO_ROOT)
    return managed


async def _gmsh_managed(vcpkg: Path, version: str, install_args: list[str]) -> Path:
    """Generate the complete gmsh C# bindings from the pinned api definition."""
    root = await _source_root(vcpkg, "gmsh", f"gmsh-{version}-source.tgz", r"^[^/]+/(api/|CMakeLists\.txt$)", install_args)
    managed = _managed_dir("gmsh")
    _log.info("generated", functions=generate(root / "api", managed, version))
    return managed


def _gmsh_overlay(vcpkg: Path) -> list[str]:
    """Copy the pinned gmsh port with its mesh module and bundled Eigen re-enabled and return the overlay arguments."""
    root = _work("gmsh") / "overlay"
    shutil.rmtree(root, ignore_errors=True)
    shutil.copytree(vcpkg.parent / "ports" / "gmsh", root / "gmsh")
    portfile = root / "gmsh" / "portfile.cmake"
    text = portfile.read_text()
    for flag in _GMSH_PORT_DISABLED:
        if flag not in text:
            raise SystemExit(f"{flag} not found in the pinned gmsh portfile")
        text = text.replace(flag, flag.replace("=OFF", "=ON"))
    _ = portfile.write_text(text)
    return ["--overlay-ports", str(root)]


async def _stage_port(library: str, port: _Port, rid: Rid) -> Path:
    """Build the manifest with vcpkg, generate binding sources when the port declares them, and stage the library files."""
    work = _work(library)
    target = vcpkg_target(rid, port.stem, windows_stem=port.windows_stem, closure=port.closure)
    tools = await native_build_tools()
    version = _pinned_version(tools.vcpkg, library)
    install_args = vcpkg_args(_MANIFEST_ROOT / library, work, target.triplet, *(port.overlay(tools.vcpkg) if port.overlay is not None else []))
    built = await vcpkg_install(tools, work, target, install_args)
    if port.managed is not None:
        _ = await port.managed(tools.vcpkg, version, install_args)
    if not port.closure:
        return stage_library(built[0], work, rid, target.file_name)
    return await stage_closure(built, work, rid, _canonical(port.stem, target.file_name) if port.canonical else None)


# --- [RELEASES] -------------------------------------------------------------------------


async def _emgucv_build(src: Path) -> Path:
    """Run the upstream arm64 full recipe and return the built library."""
    cmake = await cmake_tool()
    sdk = (await anyio.run_process(["xcrun", "--sdk", "macosx", "--show-sdk-path"])).stdout.decode().strip()
    install = src / "platforms" / "macos" / _EMGUCV_BUILD_DIR / "install"
    arch = ("-DCMAKE_OSX_ARCHITECTURES=arm64", f"-DCMAKE_OSX_SYSROOT:STRING={sdk}", f"-DCMAKE_INSTALL_PREFIX:STRING={install}")
    find_root = f"-DCMAKE_FIND_ROOT_PATH:STRING={install}"
    contrib = (f"-DOPENCV_EXTRA_MODULES_PATH={src / 'opencv_contrib' / 'modules'}", f"-DEigen3_DIR:STRING={install / 'share' / 'eigen3' / 'cmake'}")
    steps = (
        (src / "eigen", src / "eigen" / _EMGUCV_BUILD_DIR, "install", arch),
        (src / "hdf5", src / "hdf5" / _EMGUCV_BUILD_DIR, "install", (*arch, find_root, *_EMGUCV_HDF5_FLAGS)),
        (src, src / "platforms" / "macos" / _EMGUCV_BUILD_DIR, "cvextern", (*arch, find_root, *_EMGUCV_OPENCV_FLAGS, *contrib)),
    )
    for source, build, target, flags in steps:
        await run([str(cmake), "-S", str(source), "-B", str(build), "-DCMAKE_BUILD_TYPE:STRING=Release", *flags], REPO_ROOT)
        await run([str(cmake), "--build", str(build), "--target", target, "--parallel", str(os.cpu_count() or 1)], REPO_ROOT)
    return src / "libs" / "runtimes" / "osx" / "native" / "libcvextern.dylib"  # Post-build lipo step writes here regardless of architecture


async def _stage_emgucv(rid: Rid) -> Path:
    """Reuse or build the commit-keyed Emgu CV library and stage it for the rid."""
    if rid != "osx-arm64":
        raise SystemExit(f"emgucv builds osx-arm64 only, not {rid}")
    version, commit = emgucv_pins()
    artifact = REPO_ROOT / ".cache" / "emgucv" / "artifacts" / commit / "libcvextern.dylib"
    if not artifact.is_file():
        artifact.parent.mkdir(parents=True, exist_ok=True)
        _ = shutil.copy(await _emgucv_build(await emgucv_source()), artifact)
    _log.info("resolved", version=version, commit=commit[:9], artifact=str(artifact))
    return stage_library(artifact, _work("emgucv"), rid, "libcvextern.dylib")


async def _stage_duckdb_extensions(rid: Rid) -> Path:
    """Decompress the pinned DuckDB extension archives into the extension directory layout under contentFiles."""
    version, archives = await duckdb_extension_archives(rid)
    stage = _work("duckdbextensions") / "stage"
    destination = stage / "contentFiles" / "duckdb_extensions" / f"v{version}" / duckdb_platform(rid)
    shutil.rmtree(stage, ignore_errors=True)
    destination.mkdir(parents=True)
    for archive in archives:
        with gzip.open(archive, "rb") as compressed, (destination / archive.name.removesuffix(".gz")).open("wb") as extension:
            shutil.copyfileobj(compressed, extension)
    return destination


async def _stage_sqlite_vec(rid: Rid) -> Path:
    """Extract the pinned sqlite-vec loadable into the runtimes layout."""
    _, archive = await sqlite_vec_archive(rid)
    file_name = _SQLITE_VEC_FILE_NAMES[rid]
    work = _work("sqlitevec")
    destination = stage_dir(work, rid)
    shutil.rmtree(work / "stage", ignore_errors=True)
    destination.mkdir(parents=True)
    with tarfile.open(archive) as tar:
        member = next((entry for entry in tar.getmembers() if Path(entry.name).name == file_name), None)
        source = tar.extractfile(member) if member is not None else None
        if source is None:
            raise SystemExit(f"no {file_name} in {archive.name}")
        with source, (destination / file_name).open("wb") as loadable:
            shutil.copyfileobj(source, loadable)
    return destination / file_name


# --- [CLI] ------------------------------------------------------------------------------

_LIBRARIES: dict[str, _Stage] = {
    "blosc2": partial(_stage_port, "blosc2", _Port("blosc2", windows_stem="libblosc2", closure=True, canonical=True)),
    "duckdbextensions": _stage_duckdb_extensions,
    "emgucv": _stage_emgucv,
    "ffmpeg": partial(_stage_port, "ffmpeg", _Port("ffmpeg", closure=True)),
    "gmsh": partial(_stage_port, "gmsh", _Port("gmsh", overlay=_gmsh_overlay, managed=_gmsh_managed)),
    "lcms2": partial(_stage_port, "lcms2", _Port("lcms2")),
    "sqlitevec": _stage_sqlite_vec,
    "z3": partial(_stage_port, "z3", _Port("z3", windows_stem="libz3", managed=_z3_managed)),
}


@_app.default
def main(library: str, rid: Rid | None = None) -> None:
    """Stage the named library for the given or host runtime identifier."""
    match _LIBRARIES.get(library):
        case None:
            raise SystemExit(f"unknown library {library}, expected one of {', '.join(_LIBRARIES)}")
        case stage:
            resolved = rid or host_rid()
            staged = anyio.run(stage, resolved)
            _log.info("staged", library=library, rid=resolved, path=str(staged))


if __name__ == "__main__":
    _app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["main"]
