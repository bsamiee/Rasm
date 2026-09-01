"""Provision the pinned build tools every native pipeline shares, cloned and cached once."""

# --- [IMPORTS] --------------------------------------------------------------------------

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


# --- [CONSTANTS] ------------------------------------------------------------------------

REPO_ROOT: Path = next(parent for parent in Path(__file__).resolve().parents if (parent / "uv.lock").is_file())

_VCPKG_URL = "https://github.com/microsoft/vcpkg"
_VCPKG_COMMIT = "30ef65cad98f08e7197c9a1656fbd871bcb72f2d"  # equals the builtin-baseline in eng/native/lcms2/vcpkg.json
_VCPKG_ROOT = REPO_ROOT / ".cache" / "vcpkg"
_HOST_TOOLS = REPO_ROOT / ".cache" / "vcpkg-hosttools"

_log = structlog.get_logger(__name__)
app = cyclopts.App(name="provision")

# --- [OPERATIONS] -----------------------------------------------------------------------


async def run(args: list[str], cwd: Path, env: dict[str, str] | None = None) -> None:
    """Run one build tool to completion, inheriting the console."""
    merged = None if env is None else os.environ | env  # ruff:ignore[banned-api]
    _ = await anyio.run_process(args, cwd=cwd, env=merged, stdout=None, stderr=None)


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
    head = await anyio.run_process(["git", "rev-parse", "HEAD"], cwd=_VCPKG_ROOT, check=False)
    if head.stdout.decode().strip() != _VCPKG_COMMIT:
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


async def native_build_tools() -> ToolSet:
    """Ensure every pinned native build tool exists and return the set."""
    vcpkg = await _vcpkg()
    return ToolSet(vcpkg=vcpkg, env=await _pkg_config(vcpkg))


@app.default
def main() -> None:
    """Provision the pinned tool set for this machine."""
    tools = anyio.run(native_build_tools)
    _log.info("provisioned", vcpkg=str(tools.vcpkg), env=tools.env)


if __name__ == "__main__":
    app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["REPO_ROOT", "Rid", "ToolSet", "host_rid", "main", "native_build_tools", "run"]
