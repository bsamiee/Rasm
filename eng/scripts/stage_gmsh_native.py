"""Build the gmsh shared library for one runtime identifier and stage it for packing.

CI matrix: macos arm64, ubuntu x64, windows x64 each stage their runtime identifier, one job
collects .artifacts/native/gmsh/stage and runs the eng pack-gmsh-native and pack-gmsh-managed
targets alone. The managed binding sources generate from the api definition inside the same
pinned archive the gmsh port downloads. Rasm.Gmsh and Rasm.Native.Gmsh always carry one
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
import structlog

from eng.scripts.gen_gmsh_bindings import generate
from eng.scripts.provision import host_rid, manifest_version, native_build_tools, REPO_ROOT, Rid, run, stage_library, vcpkg_args, vcpkg_install, vcpkg_target

# --- [CONSTANTS] ------------------------------------------------------------------------

_MANIFEST_ROOT = REPO_ROOT / "eng" / "native" / "gmsh"
_WORK = REPO_ROOT / ".artifacts" / "native" / "gmsh"
_PORT_FLIPS = ("-DENABLE_MESH=OFF", "-DENABLE_EIGEN=OFF")

_log = structlog.get_logger(__name__)
app = cyclopts.App(name="stage-gmsh-native")

# --- [OPERATIONS] -----------------------------------------------------------------------


def _version(vcpkg: Path) -> str:
    """Return the pinned port version after checking the manifest pin equals it."""
    port = manifest_version(vcpkg.parent / "ports" / "gmsh" / "vcpkg.json")
    manifest = manifest_version(_MANIFEST_ROOT / "vcpkg.json", "version-string")
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
    if not archive.exists():  # A binary-cache hit builds nothing and downloads no source
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
    target = vcpkg_target(rid, "gmsh")
    tools = await native_build_tools()
    version = _version(tools.vcpkg)
    install_args = vcpkg_args(_MANIFEST_ROOT, _WORK, target.triplet, "--overlay-ports", str(_overlay(tools.vcpkg)))
    built = await vcpkg_install(tools, _WORK, target, install_args)
    destination = stage_library(built[0], _WORK, rid, target.file_name)
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
