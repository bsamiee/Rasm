"""Provision the pinned build tools every native pipeline shares, cloned and cached once."""

# --- [IMPORTS] --------------------------------------------------------------------------

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


# --- [CONSTANTS] ------------------------------------------------------------------------

REPO_ROOT: Path = next(parent for parent in Path(__file__).resolve().parents if (parent / "uv.lock").is_file())

_VCPKG_URL = "https://github.com/microsoft/vcpkg"
_VCPKG_COMMIT = "30ef65cad98f08e7197c9a1656fbd871bcb72f2d"  # equals the builtin-baseline in eng/native/lcms2/vcpkg.json
_VCPKG_ROOT = REPO_ROOT / ".cache" / "vcpkg"
_HOST_TOOLS = REPO_ROOT / ".cache" / "vcpkg-hosttools"

_ENERGYPLUS_RELEASES = "https://github.com/NatLabRockies/EnergyPlus/releases/download"
_ENERGYPLUS_VERSION = "25.2.0"  # the EnergyPlus release the catalog's NREL.OpenStudio 3.11.0 translates models for
_ENERGYPLUS_ASSETS: dict[Rid, tuple[str, str]] = {"osx-arm64": ("EnergyPlus-25.2.0-cf7368216c-Darwin-macOS13-arm64.tar.gz", "e7976e82509d961bcf484963a1a7109db4cae318dfc318898f97183f4097deda")}

_DUCKDB_EXTENSION_REPOSITORY = "https://extensions.duckdb.org"
_DUCKDB_EXTENSIONS_MANIFEST = REPO_ROOT / "eng" / "native" / "duckdbextensions" / "extensions.json"
_DUCKDB_PLATFORMS: dict[Rid, str] = {"osx-arm64": "osx_arm64", "linux-x64": "linux_amd64", "linux-arm64": "linux_arm64", "win-x64": "windows_amd64"}

_log = structlog.get_logger(__name__)
app = cyclopts.App(name="provision")

# --- [OPERATIONS] -----------------------------------------------------------------------


async def run(args: list[str], cwd: Path, env: dict[str, str] | None = None) -> None:
    """Run one build tool to completion, inheriting the console."""
    merged = None if env is None else os.environ | env  # ruff:ignore[banned-api]
    _ = await anyio.run_process(args, cwd=cwd, env=merged, stdout=None, stderr=None)


async def capture(args: list[str], cwd: Path = REPO_ROOT, *, check: bool = True) -> str:
    """Run one build tool and return its stdout."""
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
    head = await capture(["git", "rev-parse", "HEAD"], _VCPKG_ROOT, check=False)
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
        # the pkgconf port validates its own pc file with pkg-config, the one program this machine lacks
        overlay = _HOST_TOOLS / "overlay"
        shutil.copytree(_VCPKG_ROOT / "ports" / "pkgconf", overlay / "pkgconf", dirs_exist_ok=True)
        portfile = overlay / "pkgconf" / "portfile.cmake"
        _ = portfile.write_text(portfile.read_text().replace("vcpkg_fixup_pkgconfig()", "vcpkg_fixup_pkgconfig(SKIP_CHECK)"))
        await run([str(vcpkg), "install", f"pkgconf:{_host_triplet()}", "--overlay-ports", str(overlay), "--x-install-root", str(_HOST_TOOLS / "installed"), "--no-print-usage"], REPO_ROOT)
    return {"PKG_CONFIG": str(tool)}


async def _pinned_archive(name: str, version: str, url: str, sha256: str) -> Path:
    """Fetch one pinned release archive, verify its digest, and unpack it under the cache."""
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
    """Fetch one pinned file, verify its digest, and place it at the destination."""
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
    """Return the DuckDB platform directory name for one runtime identifier."""
    return _DUCKDB_PLATFORMS[rid]


async def duckdb_extension_archives(rid: Rid) -> tuple[str, list[Path]]:
    """Ensure the pinned DuckDB extension archives for one rid exist and return the engine version and files."""
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


async def energyplus_exe() -> Path:
    """Ensure the pinned EnergyPlus runtime exists and return its executable."""
    rid = host_rid()
    match _ENERGYPLUS_ASSETS.get(rid):
        case (asset, sha256):
            url = f"{_ENERGYPLUS_RELEASES}/v{_ENERGYPLUS_VERSION}/{asset}"
            return await _pinned_archive("energyplus", _ENERGYPLUS_VERSION, url, sha256) / "energyplus"
        case _:
            raise SystemExit(f"no EnergyPlus {_ENERGYPLUS_VERSION} asset pinned for {rid}")


async def native_build_tools() -> ToolSet:
    """Ensure every pinned native build tool exists and return the set."""
    vcpkg = await _vcpkg()
    return ToolSet(vcpkg=vcpkg, env=await _pkg_config(vcpkg))


@app.default
def main() -> None:
    """Provision the pinned tool set for this machine."""
    tools = anyio.run(native_build_tools)
    energyplus = anyio.run(energyplus_exe)
    duckdb, extensions = anyio.run(duckdb_extension_archives, host_rid())
    _log.info("provisioned", vcpkg=str(tools.vcpkg), env=tools.env, energyplus=str(energyplus), duckdb=duckdb, extensions=[archive.name for archive in extensions])


if __name__ == "__main__":
    app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["REPO_ROOT", "Rid", "ToolSet", "capture", "duckdb_extension_archives", "duckdb_platform", "energyplus_exe", "host_rid", "main", "native_build_tools", "run"]
