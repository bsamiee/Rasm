"""Build the c-blosc2 shared library for one runtime identifier and stage it for packing.

CI matrix: macos arm64, ubuntu x64, windows x64 each stage their runtime identifier, one job
collects .artifacts/native/blosc2/stage and runs the eng pack-blosc2-native target alone. On
macOS every install name is rewritten to @loader_path and re-signed ad hoc so dotnet default
probing loads the codec dependency closure from one output directory. Only the blosc2 library
itself takes the canonical name the managed DllImport probes for.
"""

# --- [IMPORTS] --------------------------------------------------------------------------

from pathlib import Path

import anyio
import cyclopts
import structlog

from eng.scripts.provision import host_rid, native_build_tools, REPO_ROOT, Rid, stage_closure, vcpkg_args, vcpkg_install, vcpkg_target

# --- [CONSTANTS] ------------------------------------------------------------------------

_MANIFEST_ROOT = REPO_ROOT / "eng" / "native" / "blosc2"
_WORK = REPO_ROOT / ".artifacts" / "native" / "blosc2"

_log = structlog.get_logger(__name__)
app = cyclopts.App(name="stage-blosc2-native")

# --- [OPERATIONS] -----------------------------------------------------------------------


async def _stage(rid: Rid) -> Path:
    """Build the manifest with vcpkg and stage every runtime library for one rid."""
    target = vcpkg_target(rid, "blosc2", windows_stem="libblosc2", closure=True)
    tools = await native_build_tools()
    built = await vcpkg_install(tools, _WORK, target, vcpkg_args(_MANIFEST_ROOT, _WORK, target.triplet))
    return await stage_closure(built, _WORK, rid, rename=lambda name: target.file_name if name.removeprefix("lib").startswith("blosc2") else name)


@app.default
def main(rid: Rid | None = None) -> None:
    """Stage the c-blosc2 libraries for the given or host runtime identifier."""
    resolved = rid or host_rid()
    staged = anyio.run(_stage, resolved)
    _log.info("staged", rid=resolved, libraries=len(list(staged.iterdir())), path=str(staged))


if __name__ == "__main__":
    app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["main"]
