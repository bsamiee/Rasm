"""Stage the pinned DuckDB extension files for one runtime identifier for packing.

The payload packs as contentFiles so consumer output carries duckdb_extensions/v<engine>/<platform>/,
the local repository layout a DuckDB extension_directory setting resolves for offline LOAD.
"""

# --- [IMPORTS] --------------------------------------------------------------------------

import gzip
from pathlib import Path
import shutil

import anyio
import cyclopts
import structlog

from eng.scripts.provision import duckdb_extension_archives, duckdb_platform, host_rid, REPO_ROOT, Rid

# --- [CONSTANTS] ------------------------------------------------------------------------

_WORK = REPO_ROOT / ".artifacts" / "native" / "duckdbextensions"

_log = structlog.get_logger(__name__)
app = cyclopts.App(name="stage-duckdbextensions-native")

# --- [OPERATIONS] -----------------------------------------------------------------------


async def _stage(rid: Rid) -> Path:
    """Decompress the provisioned extension archives into the staged extension directory layout."""
    version, archives = await duckdb_extension_archives(rid)
    destination = _WORK / "stage" / "contentFiles" / "duckdb_extensions" / f"v{version}" / duckdb_platform(rid)
    shutil.rmtree(_WORK / "stage", ignore_errors=True)
    destination.mkdir(parents=True)
    for archive in archives:
        with gzip.open(archive, "rb") as compressed, (destination / archive.name.removesuffix(".gz")).open("wb") as extension:
            shutil.copyfileobj(compressed, extension)
    return destination


@app.default
def main(rid: Rid | None = None) -> None:
    """Stage the DuckDB extension files for the given or host runtime identifier."""
    resolved = rid or host_rid()
    staged = anyio.run(_stage, resolved)
    _log.info("staged", rid=resolved, path=str(staged))


if __name__ == "__main__":
    app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["main"]
