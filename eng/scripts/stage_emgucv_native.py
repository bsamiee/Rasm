"""Build the full Emgu CV native library for macOS arm64 and stage it for packing.

The recipe is the pinned checkout's own platforms/macos/configure arm64 full: static eigen and
hdf5 prebuilds feed one OpenCV configure with the contrib modules and tesseract, and the
cvextern target builds alone, in parallel. Build trees stay inside the cached checkout,
rebuilds are incremental, and a commit-keyed artifact under .cache makes a repeat run a no-op
that never touches CMake. CI packs from that artifact or rebuilds it on demand; the compile
never enters routine pipelines.
"""

# --- [IMPORTS] --------------------------------------------------------------------------

import os
from pathlib import Path
import shutil

import anyio
import cyclopts
import structlog

from eng.scripts.provision import cmake_tool, emgucv_pins, emgucv_source, host_rid, REPO_ROOT, Rid, run, stage_library

# --- [CONSTANTS] ------------------------------------------------------------------------

_WORK = REPO_ROOT / ".artifacts" / "native" / "emgucv"
_BUILD_DIR = "build_arm64"  # Upstream's in-tree build folder name for the eigen, hdf5, and opencv steps
_HDF5_FLAGS = ("-DBUILD_SHARED_LIBS:BOOL=OFF", "-DBUILD_TESTING:BOOL=FALSE", "-DHDF5_BUILD_EXAMPLES:BOOL=FALSE", "-DHDF5_BUILD_TOOLS:BOOL=FALSE", "-DHDF5_BUILD_UTILS:BOOL=FALSE")
_OPENCV_FLAGS = (
    "-DOPENCV_FORCE_3RDPARTY_BUILD:BOOL=TRUE",
    "-DBUILD_PERF_TESTS=FALSE",
    "-DBUILD_TESTS:BOOL=FALSE",
    "-DBUILD_DOCS:BOOL=FALSE",
    "-DBUILD_JPEG:BOOL=TRUE",
    "-DBUILD_ZLIB:BOOL=TRUE",
    "-DBUILD_OPENEXR:BOOL=TRUE",
    "-DBUILD_PNG:BOOL=TRUE",
    "-DBUILD_TIFF:BOOL=TRUE",
    "-DWITH_OPENVINO:BOOL=FALSE",
    "-DWITH_WEBP:BOOL=OFF",
    "-DWITH_IPP:BOOL=OFF",
    "-DWITH_CUDA:BOOL=OFF",
    "-DWITH_OBSENSOR:BOOL=OFF",
    "-DWITH_TESSERACT:BOOL=OFF",
    "-DWITH_LAPACK:BOOL=OFF",
    "-DBUILD_opencv_ts:BOOL=OFF",
    "-DBUILD_opencv_java:BOOL=OFF",
    "-DBUILD_opencv_python2:BOOL=OFF",
    "-DBUILD_opencv_python3:BOOL=OFF",
    "-DBUILD_opencv_apps:BOOL=OFF",
    "-DBUILD_opencv_freetype:BOOL=FALSE",
    "-DBUILD_SHARED_LIBS:BOOL=OFF",
    "-DEMGU_CV_WITH_TESSERACT:BOOL=TRUE",
    "-DEMGU_CV_WITH_FREETYPE:BOOL=FALSE",
    "-DCMAKE_IGNORE_PREFIX_PATH:STRING=/usr/local;/opt/homebrew",  # Upstream ignores the runner's /usr/local; arm64 Homebrew lives in /opt/homebrew
)

_log = structlog.get_logger(__name__)
app = cyclopts.App(name="stage-emgucv-native")

# --- [OPERATIONS] -----------------------------------------------------------------------


async def _configure(cmake: Path, source: Path, build: Path, flags: list[str]) -> None:
    """Configure one CMake tree at the upstream recipe's Release settings."""
    await run([str(cmake), "-S", str(source), "-B", str(build), "-DCMAKE_BUILD_TYPE:STRING=Release", *flags], REPO_ROOT)


async def _compile(cmake: Path, build: Path, target: str) -> None:
    """Build one CMake target across every core."""
    await run([str(cmake), "--build", str(build), "--target", target, "--parallel", str(os.cpu_count() or 1)], REPO_ROOT)


async def _build(src: Path) -> Path:
    """Run the upstream arm64 full recipe and return the built library."""
    cmake = await cmake_tool()
    sdk = (await anyio.run_process(["xcrun", "--sdk", "macosx", "--show-sdk-path"])).stdout.decode().strip()
    install = src / "platforms" / "macos" / _BUILD_DIR / "install"
    arch = ["-DCMAKE_OSX_ARCHITECTURES=arm64", f"-DCMAKE_OSX_SYSROOT:STRING={sdk}", f"-DCMAKE_INSTALL_PREFIX:STRING={install}"]
    await _configure(cmake, src / "eigen", src / "eigen" / _BUILD_DIR, arch)
    await _compile(cmake, src / "eigen" / _BUILD_DIR, "install")
    await _configure(cmake, src / "hdf5", src / "hdf5" / _BUILD_DIR, [*arch, f"-DCMAKE_FIND_ROOT_PATH:STRING={install}", *_HDF5_FLAGS])
    await _compile(cmake, src / "hdf5" / _BUILD_DIR, "install")
    build = src / "platforms" / "macos" / _BUILD_DIR
    contrib = [f"-DOPENCV_EXTRA_MODULES_PATH={src / 'opencv_contrib' / 'modules'}", f"-DEigen3_DIR:STRING={install / 'share' / 'eigen3' / 'cmake'}"]
    await _configure(cmake, src, build, [*arch, f"-DCMAKE_FIND_ROOT_PATH:STRING={install}", *_OPENCV_FLAGS, *contrib])
    await _compile(cmake, build, "cvextern")
    # The post-build lipo step assembles the deliverable here regardless of architecture
    return src / "libs" / "runtimes" / "osx" / "native" / "libcvextern.dylib"


async def _stage(rid: Rid) -> Path:
    """Reuse or build the commit-keyed library and stage it for one rid."""
    if rid != "osx-arm64":
        raise SystemExit(f"emgucv builds osx-arm64 only, not {rid}")
    version, commit = emgucv_pins()
    artifact = REPO_ROOT / ".cache" / "emgucv" / "artifacts" / commit / "libcvextern.dylib"
    if not artifact.is_file():
        built = await _build(await emgucv_source())
        artifact.parent.mkdir(parents=True, exist_ok=True)
        _ = shutil.copy(built, artifact)
    _log.info("resolved", version=version, commit=commit[:9], artifact=str(artifact))
    return stage_library(artifact, _WORK, rid, "libcvextern.dylib")


@app.default
def main(rid: Rid | None = None) -> None:
    """Stage the Emgu CV library for the given or host runtime identifier."""
    resolved = rid or host_rid()
    staged = anyio.run(_stage, resolved)
    _log.info("staged", rid=resolved, path=str(staged))


if __name__ == "__main__":
    app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["main"]
