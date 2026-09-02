"""Stage the pinned sqlite-vec loadable extension for a runtime identifier for packing.

The loadable packs as runtimes/<rid>/native/, the flat file a SqliteConnection LoadExtension call resolves from consumer output.
"""

# --- [IMPORTS] --------------------------------------------------------------------------

from pathlib import Path
import shutil
import tarfile

import anyio
import cyclopts
import structlog

from eng.scripts.provision import host_rid, REPO_ROOT, Rid, sqlite_vec_archive, stage_dir

# --- [CONSTANTS] ------------------------------------------------------------------------

_WORK = REPO_ROOT / ".artifacts" / "native" / "sqlitevec"
_FILE_NAMES: dict[Rid, str] = {"osx-arm64": "vec0.dylib", "linux-x64": "vec0.so", "linux-arm64": "vec0.so", "win-x64": "vec0.dll"}

_log = structlog.get_logger(__name__)
_app = cyclopts.App(name="stage-sqlitevec-native")

# --- [OPERATIONS] -----------------------------------------------------------------------


async def _stage(rid: Rid) -> Path:
    """Extract the provisioned loadable archive into the staged runtimes layout."""
    _, archive = await sqlite_vec_archive(rid)
    file_name = _FILE_NAMES[rid]
    destination = stage_dir(_WORK, rid)
    shutil.rmtree(_WORK / "stage", ignore_errors=True)
    destination.mkdir(parents=True)
    with tarfile.open(archive) as tar:
        member = next((entry for entry in tar.getmembers() if Path(entry.name).name == file_name), None)
        source = tar.extractfile(member) if member is not None else None
        if source is None:
            raise SystemExit(f"no {file_name} in {archive.name}")
        with source, (destination / file_name).open("wb") as loadable:
            shutil.copyfileobj(source, loadable)
    return destination / file_name


@_app.default
def main(rid: Rid | None = None) -> None:
    """Stage the sqlite-vec loadable for the given or host runtime identifier."""
    resolved = rid or host_rid()
    staged = anyio.run(_stage, resolved)
    _log.info("staged", rid=resolved, path=str(staged))


if __name__ == "__main__":
    _app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["main"]
