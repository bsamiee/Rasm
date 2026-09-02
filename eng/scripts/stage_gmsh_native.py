"""Build the gmsh shared library for one runtime identifier and stage it for packing.

CI matrix: macos arm64, ubuntu x64, windows x64 each stage their runtime identifier, one job
collects .artifacts/native/gmsh/stage and runs the eng pack-gmsh-native and pack-gmsh-managed
targets alone. The managed binding sources generate from the api definition inside the same
pinned archive the gmsh port downloads, so Rasm.Gmsh and Rasm.Native.Gmsh always carry one
upstream version. The staged overlay re-enables the mesh module and bundled Eigen the port
hard-codes off, everything else stays at the port's configuration.
"""

# --- [IMPORTS] --------------------------------------------------------------------------

from pathlib import Path
import re
import shutil
import tarfile

import anyio
import cyclopts
import msgspec
import structlog

from eng.scripts.gen_gmsh_bindings import generate
from eng.scripts.provision import host_rid, native_build_tools, REPO_ROOT, Rid, run

# --- [TYPES] ----------------------------------------------------------------------------


class _Target(msgspec.Struct, frozen=True, gc=False):
    """Vcpkg triplet and the library file dotnet default probing resolves for one rid."""

    triplet: str
    lib_dir: str
    pattern: str
    file_name: str


class _Manifest(msgspec.Struct, frozen=True, gc=False):
    """The one field read from a vcpkg manifest or port file."""

    version: str = msgspec.field(name="version-string")


class _Port(msgspec.Struct, frozen=True, gc=False):
    """The one field read from the vcpkg gmsh port manifest."""

    version: str


# --- [CONSTANTS] ------------------------------------------------------------------------

_MANIFEST_ROOT = REPO_ROOT / "eng" / "native" / "gmsh"
_WORK = REPO_ROOT / ".artifacts" / "native" / "gmsh"
_PORT_FLIPS = ("-DENABLE_MESH=OFF", "-DENABLE_EIGEN=OFF")

_log = structlog.get_logger(__name__)
app = cyclopts.App(name="stage-gmsh-native")

# --- [OPERATIONS] -----------------------------------------------------------------------


def _target(rid: Rid) -> _Target:
    system, _, arch = rid.partition("-")
    match system:
        case "win":
            return _Target(f"{arch}-windows", "bin", "gmsh*.dll", "gmsh.dll")
        case "osx":
            return _Target(f"{arch}-osx-dynamic", "lib", "libgmsh*.dylib", "libgmsh.dylib")
        case _:
            return _Target(f"{arch}-linux-dynamic", "lib", "libgmsh.so*", "libgmsh.so")


def _version(vcpkg: Path) -> str:
    """Return the pinned port version after checking the manifest pin equals it."""
    port = msgspec.json.decode((vcpkg.parent / "ports" / "gmsh" / "vcpkg.json").read_bytes(), type=_Port).version
    manifest = msgspec.json.decode((_MANIFEST_ROOT / "vcpkg.json").read_bytes(), type=_Manifest).version
    if port != manifest:
        raise SystemExit(f"manifest version-string {manifest} does not match the baseline gmsh port version {port}")
    return port


def _overlay(vcpkg: Path) -> Path:
    """Copy the pinned gmsh port and re-enable its mesh module and bundled Eigen."""
    root = _WORK / "overlay"
    shutil.rmtree(root, ignore_errors=True)
    shutil.copytree(vcpkg.parent / "ports" / "gmsh", root / "gmsh")
    portfile = root / "gmsh" / "portfile.cmake"
    text = portfile.read_text()
    for flag in _PORT_FLIPS:
        if flag not in text:
            raise SystemExit(f"{flag} not found in the pinned gmsh portfile")
        text = text.replace(flag, flag.replace("=OFF", "=ON"))
    _ = portfile.write_text(text)
    return root


async def _source_root(vcpkg: Path, version: str, install_args: list[str]) -> Path:
    """Unpack the source archive the gmsh port pins and return its root directory."""
    archive = vcpkg.parent / "downloads" / f"gmsh-{version}-source.tgz"
    if not archive.exists():  # a binary-cache hit builds nothing and downloads no source
        await run([str(vcpkg), "install", "--only-downloads", *install_args], REPO_ROOT)
    source = _WORK / "src"
    shutil.rmtree(source, ignore_errors=True)
    with tarfile.open(archive) as tar:
        wanted = [member for member in tar.getmembers() if re.search(r"^[^/]+/(api/|CMakeLists\.txt$)", member.name)]
        tar.extractall(source, members=wanted, filter="data")
    return next(source.iterdir())


async def _stage_managed(vcpkg: Path, version: str, install_args: list[str]) -> Path:
    """Generate the complete C# binding surface from the pinned api definition."""
    root = await _source_root(vcpkg, version, install_args)
    managed = _WORK / "stage" / "managed"
    shutil.rmtree(managed, ignore_errors=True)
    functions = generate(root / "api", managed, version)
    _log.info("generated", functions=functions)
    return managed


async def _stage(rid: Rid) -> Path:
    """Build the manifest with vcpkg, stage the library for one rid and the binding sources."""
    target = _target(rid)
    tools = await native_build_tools()
    version = _version(tools.vcpkg)
    install_args = ["--triplet", target.triplet, "--x-manifest-root", str(_MANIFEST_ROOT), "--x-install-root", str(_WORK / "installed"), "--overlay-ports", str(_overlay(tools.vcpkg)), "--no-print-usage"]
    await run([str(tools.vcpkg), "install", *install_args], REPO_ROOT, env=tools.env)
    source_dir = _WORK / "installed" / target.triplet / target.lib_dir
    real = [path for path in sorted(source_dir.glob(target.pattern)) if path.is_file() and not path.is_symlink()]
    if not real:
        raise SystemExit(f"no library matching {target.pattern} under {source_dir}")
    destination = _WORK / "stage" / "runtimes" / rid / "native" / target.file_name
    destination.parent.mkdir(parents=True, exist_ok=True)
    _ = shutil.copy(real[0], destination)
    _ = await _stage_managed(tools.vcpkg, version, install_args)
    return destination


@app.default
def main(rid: Rid | None = None) -> None:
    """Stage the gmsh library for the given or host runtime identifier."""
    resolved = rid or host_rid()
    staged = anyio.run(_stage, resolved)
    _log.info("staged", rid=resolved, path=str(staged))


if __name__ == "__main__":
    app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["main"]
