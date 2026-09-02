"""Build the z3 shared library for one runtime identifier and stage it for packing.

CI matrix: macos arm64, ubuntu x64, windows x64 each stage their runtime identifier, one job
collects .artifacts/native/z3/stage and runs the eng pack-z3-native and pack-z3-managed
targets alone. The managed binding sources stage from the same pinned archive the z3 port
downloads. Rasm.Z3 and Rasm.Native.Z3 always carry one upstream version.
"""

# --- [IMPORTS] --------------------------------------------------------------------------

from pathlib import Path
import re
import shutil
import sys
import tarfile

import anyio
import cyclopts
import structlog

from eng.scripts.provision import host_rid, manifest_version, native_build_tools, REPO_ROOT, Rid, run, stage_library, vcpkg_args, vcpkg_install, vcpkg_target

# --- [CONSTANTS] ------------------------------------------------------------------------

_MANIFEST_ROOT = REPO_ROOT / "eng" / "native" / "z3"
_WORK = REPO_ROOT / ".artifacts" / "native" / "z3"

_log = structlog.get_logger(__name__)
app = cyclopts.App(name="stage-z3-native")

# --- [OPERATIONS] -----------------------------------------------------------------------


def _search(pattern: str, text: str) -> str:
    match = re.search(pattern, text)
    if match is None:
        raise SystemExit(f"pattern {pattern!r} not found in the z3 port files")
    return match.group(1)


async def _source_root(vcpkg: Path, install_args: list[str]) -> Path:
    """Unpack the source archive the z3 port pins and return its root directory."""
    port = vcpkg.parent / "ports" / "z3"
    version = manifest_version(port / "vcpkg.json")
    portfile = (port / "portfile.cmake").read_text()
    repo = _search(r"REPO\s+(\S+)", portfile)
    ref = _search(r"REF\s+(\S+)", portfile).replace("${VERSION}", version)
    archive = vcpkg.parent / "downloads" / f"{repo.replace('/', '-')}-{ref}.tar.gz"
    if not archive.exists():  # A binary-cache hit builds nothing and downloads no source
        await run([str(vcpkg), "install", "--only-downloads", *install_args], REPO_ROOT)
    source = _WORK / "src"
    shutil.rmtree(source, ignore_errors=True)
    with tarfile.open(archive) as tar:
        wanted = [member for member in tar.getmembers() if re.search(r"/(scripts|src/api)/|^[^/]+/CMakeLists\.txt$", member.name)]
        tar.extractall(source, members=wanted, filter="data")
    return next(source.iterdir())


async def _stage_managed(vcpkg: Path, install_args: list[str]) -> Path:
    """Stage the binding sources and generate Native.cs and Enumerations.cs beside them."""
    root = await _source_root(vcpkg, install_args)
    names = _search(r"set\(Z3_API_HEADER_FILES_TO_SCAN\s+([^)]+)\)", (root / "CMakeLists.txt").read_text()).split()
    headers = [str(root / "src" / "api" / name) for name in names]
    managed = _WORK / "stage" / "managed"
    shutil.rmtree(managed, ignore_errors=True)
    managed.mkdir(parents=True)
    for path in sorted((root / "src" / "api" / "dotnet").glob("*.cs")):
        _ = shutil.copy(path, managed)
    for script in ("update_api.py", "mk_consts_files.py"):
        await run([sys.executable, str(root / "scripts" / script), *headers, "--dotnet-output-dir", str(managed)], REPO_ROOT)
    return managed


async def _stage(rid: Rid) -> Path:
    """Build the manifest with vcpkg, stage the library for one rid and the binding sources."""
    target = vcpkg_target(rid, "z3", windows_stem="libz3")
    tools = await native_build_tools()
    install_args = vcpkg_args(_MANIFEST_ROOT, _WORK, target.triplet)
    built = await vcpkg_install(tools, _WORK, target, install_args)
    destination = stage_library(built[0], _WORK, rid, target.file_name)
    _ = await _stage_managed(tools.vcpkg, install_args)
    return destination


@app.default
def main(rid: Rid | None = None) -> None:
    """Stage the z3 library for the given or host runtime identifier."""
    resolved = rid or host_rid()
    staged = anyio.run(_stage, resolved)
    _log.info("staged", rid=resolved, path=str(staged))


if __name__ == "__main__":
    app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["main"]
