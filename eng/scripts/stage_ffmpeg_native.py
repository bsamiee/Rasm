"""Build the FFmpeg shared libraries for a runtime identifier and stage them for packing.

CI matrix: macos arm64, ubuntu x64, windows x64 stage their runtime identifiers, then a job collects .artifacts/native/ffmpeg/stage and runs the eng pack-ffmpeg-native target alone.
On macOS every install name is rewritten to @loader_path and re-signed ad hoc, dotnet default probing loads the encoder-capable set from the output directory.
"""

# --- [IMPORTS] --------------------------------------------------------------------------

from pathlib import Path

import anyio
import cyclopts
import structlog

from eng.scripts.provision import host_rid, native_build_tools, REPO_ROOT, Rid, stage_closure, vcpkg_args, vcpkg_install, vcpkg_target

# --- [CONSTANTS] ------------------------------------------------------------------------

_MANIFEST_ROOT = REPO_ROOT / "eng" / "native" / "ffmpeg"
_WORK = REPO_ROOT / ".artifacts" / "native" / "ffmpeg"

_log = structlog.get_logger(__name__)
_app = cyclopts.App(name="stage-ffmpeg-native")

# --- [OPERATIONS] -----------------------------------------------------------------------


async def _stage(rid: Rid) -> Path:
    """Build the manifest with vcpkg and stage every runtime library for the rid."""
    target = vcpkg_target(rid, "ffmpeg", closure=True)
    tools = await native_build_tools()
    built = await vcpkg_install(tools, _WORK, target, vcpkg_args(_MANIFEST_ROOT, _WORK, target.triplet))
    return await stage_closure(built, _WORK, rid)


@_app.default
def main(rid: Rid | None = None) -> None:
    """Stage the FFmpeg libraries for the given or host runtime identifier."""
    resolved = rid or host_rid()
    staged = anyio.run(_stage, resolved)
    _log.info("staged", rid=resolved, libraries=len(list(staged.iterdir())), path=str(staged))


if __name__ == "__main__":
    _app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["main"]
