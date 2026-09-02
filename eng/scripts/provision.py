"""Pinned build tool provisioning and the staging operations every native pipeline uses."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Callable
import hashlib
import os
from pathlib import Path
import platform
import shutil
from typing import Literal

import anyio
import cyclopts
import msgspec
import structlog

# --- [TYPES] ----------------------------------------------------------------------------

type Rid = Literal["osx-arm64", "linux-x64", "linux-arm64", "win-x64"]


class ToolSet(msgspec.Struct, frozen=True, gc=False):
    """Pinned vcpkg executable and the environment its builds require."""

    vcpkg: Path
    env: dict[str, str]


class _ExtensionPins(msgspec.Struct, frozen=True, gc=False):
    """Committed manifest pinning the DuckDB engine version and each extension digest per rid."""

    version: str = msgspec.field(name="version-string")
    extensions: dict[str, dict[str, str]] = msgspec.field(name="extensions")


class _LoadablePins(msgspec.Struct, frozen=True, gc=False):
    """Committed manifest pinning the sqlite-vec version and each loadable archive digest per rid."""

    version: str = msgspec.field(name="version-string")
    assets: dict[str, str] = msgspec.field(name="assets")


class _SourcePins(msgspec.Struct, frozen=True, gc=False):
    """Committed manifest pinning the emgucv release commit and its wrapper version."""

    version: str = msgspec.field(name="version-string")
    commit: str = msgspec.field(name="commit")


# --- [CONSTANTS] ------------------------------------------------------------------------

REPO_ROOT: Path = next(parent for parent in Path(__file__).resolve().parents if (parent / "uv.lock").is_file())

_VCPKG_URL = "https://github.com/microsoft/vcpkg"
_VCPKG_COMMIT = "30ef65cad98f08e7197c9a1656fbd871bcb72f2d"  # Equals the builtin-baseline in eng/native/lcms2/vcpkg.json
_VCPKG_ROOT = REPO_ROOT / ".cache" / "vcpkg"
_HOST_TOOLS = REPO_ROOT / ".cache" / "vcpkg-hosttools"

_ENERGYPLUS_RELEASES = "https://github.com/NatLabRockies/EnergyPlus/releases/download"
_ENERGYPLUS_VERSION = "25.2.0"  # EnergyPlus release the catalog's NREL.OpenStudio 3.11.0 translates models for
_ENERGYPLUS_ASSETS: dict[Rid, tuple[str, str]] = {
    "osx-arm64": ("EnergyPlus-25.2.0-cf7368216c-Darwin-macOS13-arm64.tar.gz", "e7976e82509d961bcf484963a1a7109db4cae318dfc318898f97183f4097deda")
}

_DUCKDB_EXTENSION_REPOSITORY = "https://extensions.duckdb.org"
_DUCKDB_EXTENSIONS_MANIFEST = REPO_ROOT / "eng" / "native" / "duckdbextensions" / "extensions.json"
_DUCKDB_PLATFORMS: dict[Rid, str] = {"osx-arm64": "osx_arm64", "linux-x64": "linux_amd64", "linux-arm64": "linux_arm64", "win-x64": "windows_amd64"}

_SQLITE_VEC_RELEASES = "https://github.com/asg017/sqlite-vec/releases/download"
_SQLITE_VEC_MANIFEST = REPO_ROOT / "eng" / "native" / "sqlitevec" / "loadable.json"
_SQLITE_VEC_PLATFORMS: dict[Rid, str] = {
    "osx-arm64": "macos-aarch64",
    "linux-x64": "linux-x86_64",
    "linux-arm64": "linux-aarch64",
    "win-x64": "windows-x86_64",
}

_EMGUCV_URL = "https://github.com/emgucv/emgucv"
_EMGUCV_MANIFEST = REPO_ROOT / "eng" / "native" / "emgucv" / "source.json"
_EMGUCV_SUBMODULES = (
    "opencv",
    "opencv_contrib",
    "eigen",
    "hdf5",
    "Emgu.CV.Extern/tesseract/libtesseract/tesseract-ocr.git",
    "Emgu.CV.Extern/tesseract/libtesseract/leptonica/leptonica.git",
)

_log = structlog.get_logger(__name__)
_app = cyclopts.App(name="provision")

# --- [OPERATIONS] -----------------------------------------------------------------------


async def run(args: list[str], cwd: Path, env: dict[str, str] | None = None) -> None:
    """Run a build tool to completion, inheriting the console."""
    merged = None if env is None else os.environ | env  # ruff:ignore[banned-api]
    _ = await anyio.run_process(args, cwd=cwd, env=merged, stdout=None, stderr=None)


async def _capture(args: list[str], cwd: Path = REPO_ROOT, *, check: bool = True) -> str:
    """Run a build tool and return its stdout."""
    return (await anyio.run_process(args, cwd=cwd, check=check)).stdout.decode()


def host_rid() -> Rid:
    """Return the runtime identifier of this machine."""
    match (platform.system(), platform.machine().lower()):
        case ("Darwin", "arm64" | "aarch64"):
            return "osx-arm64"
        case ("Linux", "x86_64" | "amd64"):
            return "linux-x64"
        case ("Linux", "arm64" | "aarch64"):
            return "linux-arm64"
        case ("Windows", "x86_64" | "amd64"):
            return "win-x64"
        case (system, machine):
            raise SystemExit(f"unsupported host {system}/{machine}")


def _host_triplet() -> str:
    system, _, arch = host_rid().partition("-")
    return f"{arch}-{'windows' if system == 'win' else system}"


async def _vcpkg() -> Path:
    exe = _VCPKG_ROOT / ("vcpkg.exe" if platform.system() == "Windows" else "vcpkg")
    if not (_VCPKG_ROOT / ".git").exists():
        _VCPKG_ROOT.mkdir(parents=True, exist_ok=True)
        await run(["git", "init", "--quiet"], _VCPKG_ROOT)
        await run(["git", "remote", "add", "origin", _VCPKG_URL], _VCPKG_ROOT)
    head = await _capture(["git", "rev-parse", "HEAD"], _VCPKG_ROOT, check=False)
    if head.strip() != _VCPKG_COMMIT:
        await run(["git", "fetch", "--depth", "1", "origin", _VCPKG_COMMIT], _VCPKG_ROOT)
        await run(["git", "checkout", "--quiet", _VCPKG_COMMIT], _VCPKG_ROOT)
    if not exe.exists():
        bootstrap = "bootstrap-vcpkg.bat" if platform.system() == "Windows" else "./bootstrap-vcpkg.sh"
        await run([bootstrap, "-disableMetrics"], _VCPKG_ROOT)
    return exe


async def _pkg_config(vcpkg: Path) -> dict[str, str]:
    if platform.system() == "Windows":
        return {}
    tool = _HOST_TOOLS / "installed" / _host_triplet() / "tools" / "pkgconf" / "pkgconf"
    if not tool.exists():
        # pkgconf port validates its own pc file with pkg-config, absent on this machine
        overlay = _HOST_TOOLS / "overlay"
        shutil.copytree(_VCPKG_ROOT / "ports" / "pkgconf", overlay / "pkgconf", dirs_exist_ok=True)
        portfile = overlay / "pkgconf" / "portfile.cmake"
        _ = portfile.write_text(portfile.read_text().replace("vcpkg_fixup_pkgconfig()", "vcpkg_fixup_pkgconfig(SKIP_CHECK)"))
        await run(
            [
                str(vcpkg),
                "install",
                f"pkgconf:{_host_triplet()}",
                "--overlay-ports",
                str(overlay),
                "--x-install-root",
                str(_HOST_TOOLS / "installed"),
                "--no-print-usage",
            ],
            REPO_ROOT,
        )
    return {"PKG_CONFIG": str(tool)}


async def _pinned_archive(name: str, version: str, url: str, sha256: str) -> Path:
    """Fetch a pinned release archive, verify its digest, and unpack it under the cache."""
    root = REPO_ROOT / ".cache" / name / version
    if root.is_dir():
        return root
    root.parent.mkdir(parents=True, exist_ok=True)
    archive = root.parent / url.rsplit("/", 1)[1]
    await run(["curl", "-fsSL", "--retry", "3", "-o", str(archive), url], REPO_ROOT)
    with archive.open("rb") as blob:
        digest = hashlib.file_digest(blob, "sha256").hexdigest()
    if digest != sha256:
        archive.unlink()
        raise SystemExit(f"{name} {version} digest {digest} does not match the pin {sha256}")
    staging = root.with_name(f"{version}.staging")
    shutil.rmtree(staging, ignore_errors=True)
    staging.mkdir(parents=True)
    await run(["tar", "-xzf", str(archive), "-C", str(staging), "--strip-components=1"], REPO_ROOT)
    archive.unlink()
    staging.rename(root)
    return root


async def _pinned_file(url: str, sha256: str, destination: Path) -> Path:
    """Fetch a pinned file, verify its digest, and place it at the destination."""
    if await anyio.Path(destination).is_file():
        return destination
    destination.parent.mkdir(parents=True, exist_ok=True)
    partial = destination.with_suffix(".partial")
    await run(["curl", "-fsSL", "--retry", "3", "-o", str(partial), url], REPO_ROOT)
    with partial.open("rb") as blob:
        digest = hashlib.file_digest(blob, "sha256").hexdigest()
    if digest != sha256:
        partial.unlink()
        raise SystemExit(f"{url} digest {digest} does not match the pin {sha256}")
    _ = partial.rename(destination)
    return destination


def duckdb_platform(rid: Rid) -> str:
    """Return the DuckDB platform directory name for a runtime identifier."""
    return _DUCKDB_PLATFORMS[rid]


async def duckdb_extension_archives(rid: Rid) -> tuple[str, list[Path]]:
    """Ensure the pinned DuckDB extension archives for a rid exist and return the engine version and files."""
    pins = msgspec.json.decode(_DUCKDB_EXTENSIONS_MANIFEST.read_bytes(), type=_ExtensionPins)
    root = REPO_ROOT / ".cache" / "duckdb-extensions" / pins.version / _DUCKDB_PLATFORMS[rid]
    archives: list[Path] = []
    for name, digests in sorted(pins.extensions.items()):
        match digests.get(rid):
            case str(sha256):
                url = f"{_DUCKDB_EXTENSION_REPOSITORY}/v{pins.version}/{_DUCKDB_PLATFORMS[rid]}/{name}.duckdb_extension.gz"
                archives.append(await _pinned_file(url, sha256, root / f"{name}.duckdb_extension.gz"))
            case _:
                raise SystemExit(f"no {name} extension pinned for {rid}")
    return pins.version, archives


async def sqlite_vec_archive(rid: Rid) -> tuple[str, Path]:
    """Ensure the pinned sqlite-vec loadable archive for a rid exists and return the version and file."""
    pins = msgspec.json.decode(_SQLITE_VEC_MANIFEST.read_bytes(), type=_LoadablePins)
    match pins.assets.get(rid):
        case str(sha256):
            name = f"sqlite-vec-{pins.version}-loadable-{_SQLITE_VEC_PLATFORMS[rid]}.tar.gz"
            url = f"{_SQLITE_VEC_RELEASES}/v{pins.version}/{name}"
            return pins.version, await _pinned_file(url, sha256, REPO_ROOT / ".cache" / "sqlite-vec" / pins.version / name)
        case _:
            raise SystemExit(f"no sqlite-vec loadable pinned for {rid}")


def emgucv_pins() -> tuple[str, str]:
    """Return the pinned emgucv wrapper version and release commit."""
    pins = msgspec.json.decode(_EMGUCV_MANIFEST.read_bytes(), type=_SourcePins)
    return pins.version, pins.commit


async def emgucv_source() -> Path:
    """Ensure the pinned emgucv checkout and its full-build submodules exist and return the source root."""
    _, commit = emgucv_pins()
    root = REPO_ROOT / ".cache" / "emgucv" / "src"
    if not (root / ".git").exists():
        root.mkdir(parents=True, exist_ok=True)
        await run(["git", "init", "--quiet"], root)
        await run(["git", "remote", "add", "origin", _EMGUCV_URL], root)
    head = await _capture(["git", "rev-parse", "HEAD"], root, check=False)
    if head.strip() != commit:
        await run(["git", "fetch", "--depth", "1", "origin", commit], root)
        await run(["git", "checkout", "--quiet", commit], root)
    await run(["git", "submodule", "update", "--init", "--depth", "1", "--recommend-shallow", "--", *_EMGUCV_SUBMODULES], root)
    return root


async def cmake_tool() -> Path:
    """Ensure the vcpkg-provisioned CMake exists and return its executable."""
    return Path((await _capture([str(await _vcpkg()), "fetch", "cmake"])).strip().splitlines()[-1])


async def _energyplus_exe() -> Path:
    """Ensure the pinned EnergyPlus runtime exists and return its executable."""
    rid = host_rid()
    match _ENERGYPLUS_ASSETS.get(rid):
        case (asset, sha256):
            url = f"{_ENERGYPLUS_RELEASES}/v{_ENERGYPLUS_VERSION}/{asset}"
            return await _pinned_archive("energyplus", _ENERGYPLUS_VERSION, url, sha256) / "energyplus"
        case _:
            raise SystemExit(f"no EnergyPlus {_ENERGYPLUS_VERSION} asset pinned for {rid}")


async def native_build_tools() -> ToolSet:
    """Ensure vcpkg and, except on Windows, the pkgconf host tool exist and return the set."""
    vcpkg = await _vcpkg()
    return ToolSet(vcpkg=vcpkg, env=await _pkg_config(vcpkg))


# --- [STAGING] --------------------------------------------------------------------------


class VcpkgTarget(msgspec.Struct, frozen=True, gc=False):
    """Vcpkg triplet, library directory, glob pattern, and canonical file name for a rid."""

    triplet: str
    lib_dir: str
    pattern: str
    file_name: str


def vcpkg_target(rid: Rid, stem: str, *, windows_stem: str | None = None, closure: bool = False) -> VcpkgTarget:
    """Return the dynamic-library vcpkg target for a rid, a closure target globs every built library."""
    system, _, arch = rid.partition("-")
    match system:
        case "win":
            base = windows_stem or stem
            return VcpkgTarget(f"{arch}-windows", "bin", "*.dll" if closure else f"{base}*.dll", f"{base}.dll")
        case "osx":
            return VcpkgTarget(f"{arch}-osx-dynamic", "lib", "*.dylib" if closure else f"lib{stem}*.dylib", f"lib{stem}.dylib")
        case _:
            return VcpkgTarget(f"{arch}-linux-dynamic", "lib", "*.so*" if closure else f"lib{stem}.so*", f"lib{stem}.so")


def vcpkg_args(manifest_root: Path, work: Path, triplet: str, *extra: str) -> list[str]:
    """Return the vcpkg install arguments for a manifest and work directory."""
    return ["--triplet", triplet, "--x-manifest-root", str(manifest_root), "--x-install-root", str(work / "installed"), *extra, "--no-print-usage"]


async def vcpkg_install(tools: ToolSet, work: Path, target: VcpkgTarget, args: list[str]) -> list[Path]:
    """Run vcpkg install and return the built real library files for a target."""
    await run([str(tools.vcpkg), "install", *args], REPO_ROOT, env=tools.env)
    source_dir = work / "installed" / target.triplet / target.lib_dir
    built = [path for path in sorted(source_dir.glob(target.pattern)) if path.is_file() and not path.is_symlink()]
    if not built:
        raise SystemExit(f"no library matching {target.pattern} under {source_dir}")
    return built


def manifest_version(path: Path, field: str = "version") -> str:
    """Read a version field from a JSON manifest."""
    match msgspec.json.decode(path.read_bytes(), type=dict[str, object]).get(field):
        case str(version):
            return version
        case _:
            raise SystemExit(f"no {field} string in {path}")


def stage_dir(work: Path, rid: Rid) -> Path:
    """Return the staged runtimes directory dotnet pack collects for a rid."""
    return work / "stage" / "runtimes" / rid / "native"


def stage_library(source: Path, work: Path, rid: Rid, file_name: str) -> Path:
    """Copy a built library into the staged runtimes layout."""
    destination = stage_dir(work, rid) / file_name
    destination.parent.mkdir(parents=True, exist_ok=True)
    _ = shutil.copy(source, destination)
    return destination


async def _install_name(dylib: Path) -> str:
    """Return the base name of the install name recorded in a dylib."""
    lines = (await _capture(["otool", "-L", str(dylib)])).splitlines()[1:]
    return Path(next(line.split()[0] for line in lines if line.strip())).name


async def _relink(staged: list[Path]) -> None:
    """Rewrite install names to @loader_path and re-sign, the set loads from its directory."""
    names = {path.name for path in staged}
    for path in staged:
        lines = (await _capture(["otool", "-L", str(path)])).splitlines()[1:]
        dependencies = [line.split()[0] for line in lines if line.strip() and Path(line.split()[0]).name in names]
        changes = [argument for dep in dependencies for argument in ("-change", dep, f"@loader_path/{Path(dep).name}")]
        await run(["install_name_tool", "-id", f"@loader_path/{path.name}", *changes, str(path)], REPO_ROOT)
        await run(["codesign", "--force", "--sign", "-", str(path)], REPO_ROOT)


async def stage_closure(built: list[Path], work: Path, rid: Rid, rename: Callable[[str], str] | None = None) -> Path:
    """Copy every built library into the staged runtimes layout, relinked to @loader_path on macOS."""
    destination = stage_dir(work, rid)
    shutil.rmtree(destination, ignore_errors=True)
    destination.mkdir(parents=True, exist_ok=True)
    staged: list[Path] = []
    for path in built:
        name = await _install_name(path) if rid.startswith("osx") else path.name
        copied = Path(shutil.copy(path, destination / (rename(name) if rename is not None else name)))
        await anyio.Path(copied).chmod(0o755)
        staged.append(copied)
    if rid.startswith("osx"):
        await _relink(staged)
    return destination


# --- [CLI] ------------------------------------------------------------------------------


@_app.default
def main() -> None:
    """Provision the pinned build tools, EnergyPlus runtime, and DuckDB and sqlite-vec archives for the host."""
    tools = anyio.run(native_build_tools)
    energyplus = anyio.run(_energyplus_exe)
    duckdb, extensions = anyio.run(duckdb_extension_archives, host_rid())
    sqlitevec, loadable = anyio.run(sqlite_vec_archive, host_rid())
    _log.info(
        "provisioned",
        vcpkg=str(tools.vcpkg),
        env=tools.env,
        energyplus=str(energyplus),
        duckdb=duckdb,
        extensions=[archive.name for archive in extensions],
        sqlitevec=sqlitevec,
        loadable=loadable.name,
    )


if __name__ == "__main__":
    _app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "REPO_ROOT",
    "Rid",
    "ToolSet",
    "VcpkgTarget",
    "cmake_tool",
    "duckdb_extension_archives",
    "duckdb_platform",
    "emgucv_pins",
    "emgucv_source",
    "host_rid",
    "main",
    "manifest_version",
    "native_build_tools",
    "run",
    "sqlite_vec_archive",
    "stage_closure",
    "stage_dir",
    "stage_library",
    "vcpkg_args",
    "vcpkg_install",
    "vcpkg_target",
]
