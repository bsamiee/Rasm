"""Build the lcms2 shared library for one runtime identifier and stage it for packing.

CI matrix: macos arm64, ubuntu x64, windows x64 each stage their runtime identifier, one job
collects .artifacts/native/lcms2/stage and runs the eng pack-lcms2-native target alone.
"""

# --- [IMPORTS] --------------------------------------------------------------------------

from pathlib import Path
import shutil

import anyio
import cyclopts
import msgspec
import structlog

from eng.scripts.provision import host_rid, native_build_tools, REPO_ROOT, Rid, run

# --- [TYPES] ----------------------------------------------------------------------------


class _Target(msgspec.Struct, frozen=True, gc=False):
    """Vcpkg triplet and the library file dotnet default probing resolves for one rid."""

    triplet: str
    lib_dir: str
    pattern: str
    file_name: str


# --- [CONSTANTS] ------------------------------------------------------------------------

_MANIFEST_ROOT = REPO_ROOT / "eng" / "native" / "lcms2"
_WORK = REPO_ROOT / ".artifacts" / "native" / "lcms2"

_log = structlog.get_logger(__name__)
app = cyclopts.App(name="stage-lcms2-native")

# --- [OPERATIONS] -----------------------------------------------------------------------


def _target(rid: Rid) -> _Target:
    system, _, arch = rid.partition("-")
    match system:
        case "win":
            return _Target(f"{arch}-windows", "bin", "lcms2*.dll", "lcms2.dll")
        case "osx":
            return _Target(f"{arch}-osx-dynamic", "lib", "liblcms2*.dylib", "liblcms2.dylib")
        case _:
            return _Target(f"{arch}-linux-dynamic", "lib", "liblcms2.so*", "liblcms2.so")


async def _stage(rid: Rid) -> Path:
    target = _target(rid)
    tools = await native_build_tools()
    await run([str(tools.vcpkg), "install", "--triplet", target.triplet, "--x-manifest-root", str(_MANIFEST_ROOT), "--x-install-root", str(_WORK / "installed"), "--no-print-usage"], REPO_ROOT, env=tools.env)
    source_dir = _WORK / "installed" / target.triplet / target.lib_dir
    real = [path for path in sorted(source_dir.glob(target.pattern)) if path.is_file() and not path.is_symlink()]
    if not real:
        raise SystemExit(f"no library matching {target.pattern} under {source_dir}")
    destination = _WORK / "stage" / "runtimes" / rid / "native" / target.file_name
    destination.parent.mkdir(parents=True, exist_ok=True)
    _ = shutil.copy(real[0], destination)
    return destination


@app.default
def main(rid: Rid | None = None) -> None:
    """Stage the lcms2 library for the given or host runtime identifier."""
    resolved = rid or host_rid()
    staged = anyio.run(_stage, resolved)
    _log.info("staged", rid=resolved, path=str(staged))


if __name__ == "__main__":
    app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["main"]
