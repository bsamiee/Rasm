"""Build the FFmpeg shared libraries for one runtime identifier and stage them for packing.

CI matrix: macos arm64, ubuntu x64, windows x64 each stage their runtime identifier, one job
collects .artifacts/native/ffmpeg/stage and runs the eng pack-ffmpeg-native target alone. On
macOS every install name is rewritten to @loader_path and re-signed ad hoc so dotnet default
probing loads the encoder-capable set from one output directory.
"""

# --- [IMPORTS] --------------------------------------------------------------------------

from pathlib import Path
import shutil

import anyio
import cyclopts
import msgspec
import structlog

from eng.scripts.provision import capture, host_rid, native_build_tools, REPO_ROOT, Rid, run

# --- [TYPES] ----------------------------------------------------------------------------


class _Target(msgspec.Struct, frozen=True, gc=False):
    """Vcpkg triplet and the library location dotnet default probing resolves for one rid."""

    triplet: str
    lib_dir: str
    pattern: str


# --- [CONSTANTS] ------------------------------------------------------------------------

_MANIFEST_ROOT = REPO_ROOT / "eng" / "native" / "ffmpeg"
_WORK = REPO_ROOT / ".artifacts" / "native" / "ffmpeg"

_log = structlog.get_logger(__name__)
app = cyclopts.App(name="stage-ffmpeg-native")

# --- [OPERATIONS] -----------------------------------------------------------------------


def _target(rid: Rid) -> _Target:
    system, _, arch = rid.partition("-")
    match system:
        case "win":
            return _Target(f"{arch}-windows", "bin", "*.dll")
        case "osx":
            return _Target(f"{arch}-osx-dynamic", "lib", "*.dylib")
        case _:
            return _Target(f"{arch}-linux-dynamic", "lib", "*.so*")


async def _install_name(dylib: Path) -> str:
    """Return the install name recorded in one dylib."""
    lines = (await capture(["otool", "-L", str(dylib)])).splitlines()[1:]
    return Path(next(line.split()[0] for line in lines if line.strip())).name


async def _relink(staged: list[Path]) -> None:
    """Rewrite install names to @loader_path and re-sign, so the set loads from one directory."""
    names = {path.name for path in staged}
    for path in staged:
        lines = (await capture(["otool", "-L", str(path)])).splitlines()[1:]
        dependencies = [line.split()[0] for line in lines if line.strip() and Path(line.split()[0]).name in names]
        changes = [argument for dep in dependencies for argument in ("-change", dep, f"@loader_path/{Path(dep).name}")]
        await run(["install_name_tool", "-id", f"@loader_path/{path.name}", *changes, str(path)], REPO_ROOT)
        await run(["codesign", "--force", "--sign", "-", str(path)], REPO_ROOT)


async def _stage(rid: Rid) -> Path:
    """Build the manifest with vcpkg and stage every runtime library for one rid."""
    target = _target(rid)
    tools = await native_build_tools()
    await run([str(tools.vcpkg), "install", "--triplet", target.triplet, "--x-manifest-root", str(_MANIFEST_ROOT), "--x-install-root", str(_WORK / "installed"), "--no-print-usage"], REPO_ROOT, env=tools.env)
    source_dir = _WORK / "installed" / target.triplet / target.lib_dir
    real = [path for path in sorted(source_dir.glob(target.pattern)) if path.is_file() and not path.is_symlink()]
    if not real:
        raise SystemExit(f"no libraries matching {target.pattern} under {source_dir}")
    destination = _WORK / "stage" / "runtimes" / rid / "native"
    shutil.rmtree(destination, ignore_errors=True)
    destination.mkdir(parents=True, exist_ok=True)
    staged: list[Path] = []
    for path in real:
        name = await _install_name(path) if rid.startswith("osx") else path.name
        copied = Path(shutil.copy(path, destination / name))
        await anyio.Path(copied).chmod(0o755)
        staged.append(copied)
    if rid.startswith("osx"):
        await _relink(staged)
    return destination


@app.default
def main(rid: Rid | None = None) -> None:
    """Stage the FFmpeg libraries for the given or host runtime identifier."""
    resolved = rid or host_rid()
    staged = anyio.run(_stage, resolved)
    _log.info("staged", rid=resolved, libraries=len(list(staged.iterdir())), path=str(staged))


if __name__ == "__main__":
    app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["main"]
