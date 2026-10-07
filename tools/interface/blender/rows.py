"""Blender interface records exchanged through host msgspec and Blender cattrs, with shared asset and package stamps."""

from collections.abc import Mapping
from importlib.machinery import BYTECODE_SUFFIXES
from importlib.util import source_from_cache
from typing import Final
import zipfile

from attrs import frozen
from cattrs.preconf.json import make_converter

from interface.frame import Task
from interface.render import MATERIALS
from interface.report import digest

# --- [TYPES] ----------------------------------------------------------------------------

type Install = Archived | Bundled | Listed

# --- [CONSTANTS] ------------------------------------------------------------------------

LOOK_DEVELOPMENT: Final = MATERIALS / "hdri" / "DaySkyHDRI069A_2K" / "DaySkyHDRI069A_2K_HDR.exr"

# --- [MODELS] ---------------------------------------------------------------------------


@frozen
class Repository:
    """Remote extension repository declaration."""

    module: str
    name: str
    url: str


@frozen
class Interpreter:
    """Blender's Python and bundled distribution versions keyed by canonical name."""

    executable: str
    version: str
    distributions: Mapping[str, str]


@frozen
class Installation:
    """Blender installation facts, enabled remote index paths by module, and repository change report."""

    report: str
    version: str
    repositories: Mapping[str, str]
    core: frozenset[str]
    essentials: frozenset[str]
    manifest_filename: str
    platform: str
    systems: frozenset[str]
    machines: Mapping[str, str]
    interpreter: Interpreter


@frozen
class Archive:
    """Staged package archive and member stamp."""

    path: str
    stamp: str


@frozen
class Archived:
    """Workspace package installed from a staged archive."""

    identity: str
    archive: Archive
    workspaces: tuple[Task, ...]


@frozen
class Bundled:
    """Workspace package Blender bundles."""

    identity: str
    workspaces: tuple[Task, ...]


@frozen
class Listed:
    """Workspace package installed at a remote repository's listed version."""

    identity: str
    repository: str
    version: str
    workspaces: tuple[Task, ...]


@frozen
class Launch:
    """Session arguments with unit enum name, extension manifest id, and optional VS Code and Inkscape executables."""

    report: str
    renders: str
    units: str
    extension: str
    port: int
    packages: tuple[Install, ...]
    editor: str | None
    inkscape: str | None


@frozen
class Width:
    """Stored region width and view2d zoom, with editor and region types as RNA names and numeric values."""

    editor: str
    space: int
    kind: str
    region: int
    width: int
    zoom: float


# --- [OPERATIONS] -----------------------------------------------------------------------


def stamp(files: Mapping[str, tuple[int, int]]) -> str:
    """Digest sorted relative paths, CRC-32 values, and sizes, excluding bytecode caches whose sources are present."""

    def cached(name: str) -> bool:
        try:
            return name.endswith(tuple(BYTECODE_SUFFIXES)) and source_from_cache(name) in files
        except ValueError:
            return False

    return digest(b"".join(f"{name}\0{crc}\0{size}\n".encode() for name, (crc, size) in sorted(files.items()) if not cached(name)))


def stamped(archive: zipfile.ZipFile) -> str:
    """Digest archive file members by path, CRC-32, and size."""
    return stamp({info.filename: (info.CRC, info.file_size) for info in archive.infolist() if not info.is_dir()})


# --- [COMPOSITION] ----------------------------------------------------------------------

JSON: Final = make_converter()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["JSON", "LOOK_DEVELOPMENT", "Archive", "Archived", "Bundled", "Install", "Installation", "Interpreter", "Launch", "Listed", "Repository", "Width", "stamp", "stamped"]
