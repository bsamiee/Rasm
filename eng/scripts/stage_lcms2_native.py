"""Build the lcms2 shared library for a runtime identifier and stage it for packing.

CI matrix: macos arm64, ubuntu x64, windows x64 stage their runtime identifiers, then a job collects .artifacts/native/lcms2/stage and runs the eng pack-lcms2-native target alone.
"""

# --- [IMPORTS] --------------------------------------------------------------------------

from pathlib import Path

import anyio
import cyclopts
import structlog

from eng.scripts.provision import host_rid, native_build_tools, REPO_ROOT, Rid, stage_library, vcpkg_args, vcpkg_install, vcpkg_target

# --- [CONSTANTS] ------------------------------------------------------------------------

_MANIFEST_ROOT = REPO_ROOT / "eng" / "native" / "lcms2"
_WORK = REPO_ROOT / ".artifacts" / "native" / "lcms2"

_log = structlog.get_logger(__name__)
_app = cyclopts.App(name="stage-lcms2-native")

# --- [OPERATIONS] -----------------------------------------------------------------------


async def _stage(rid: Rid) -> Path:
    """Build the manifest with vcpkg and stage the library for the rid."""
    target = vcpkg_target(rid, "lcms2")
    tools = await native_build_tools()
    built = await vcpkg_install(tools, _WORK, target, vcpkg_args(_MANIFEST_ROOT, _WORK, target.triplet))
    return stage_library(built[0], _WORK, rid, target.file_name)


@_app.default
def main(rid: Rid | None = None) -> None:
    """Stage the lcms2 library for the given or host runtime identifier."""
    resolved = rid or host_rid()
    staged = anyio.run(_stage, resolved)
    _log.info("staged", rid=resolved, path=str(staged))


if __name__ == "__main__":
    _app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["main"]
