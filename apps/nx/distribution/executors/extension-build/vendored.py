# /// script
# requires-python = ">=3.13"
# dependencies = ["vendorize"]
# ///
"""Unpacks wheels into a staged extension as its own subpackages and rewrites every absolute import of them to a relative one."""

from pathlib import Path, PurePosixPath
import sys
from zipfile import ZipFile

from vendorize.import_rewrite import rewrite_imports_in_module

# --- [OPERATIONS] -----------------------------------------------------------------------


def unpacked(stage: Path, wheel: Path, /) -> frozenset[str]:
    """Extracts a wheel's importable members into the stage and returns their top-level names."""
    with ZipFile(wheel) as archive:
        members = [name for name in archive.namelist() if not PurePosixPath(name).parts[0].endswith((".dist-info", ".data"))]
        archive.extractall(stage, members)
    return frozenset(PurePosixPath(name).parts[0].partition(".")[0] for name in members)


# --- [COMPOSITION] ----------------------------------------------------------------------

stage, *wheels = map(Path, sys.argv[1:])
names = frozenset[str]().union(*(unpacked(stage, wheel) for wheel in wheels))
for module in stage.rglob("*.py"):
    module.write_text(rewrite_imports_in_module(module.read_text(encoding="utf-8", newline=""), names, len(module.parent.relative_to(stage).parts)), encoding="utf-8", newline="")

# --- [EXPORTS] -------------------------------------------------------------------------

__all__: list[str] = []
