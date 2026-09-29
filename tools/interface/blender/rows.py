"""Records the host and Blender's Python exchange as JSON, msgspec coding them on the host and cattrs inside Blender, and the file stamp both sides compute."""

from collections.abc import Mapping

from attrs import frozen

from interface.frame import Task
from interface.report import digest

# --- [MODELS] ---------------------------------------------------------------------------


@frozen
class Installation:
    """Blender's version, synced remote repository index files by module, bundled add-on ids, Essentials catalog paths, extension manifest file name, archive platform tag, platform system names, machine name replacements, interpreter, and bundled numpy version."""

    version: str
    repositories: Mapping[str, str]
    core: frozenset[str]
    essentials: frozenset[str]
    manifest_filename: str
    platform: str
    systems: frozenset[str]
    machines: Mapping[str, str]
    python: str
    numpy: str


@frozen
class Archive:
    """Staged package archive and the stamp of its members."""

    path: str
    stamp: str


@frozen
class Local:
    """Package the session installs from its staged archive, or enables from Blender's bundled add-ons without one, with the workspaces that place it."""

    identity: str
    archive: Archive | None
    workspaces: tuple[Task, ...]


@frozen
class Listed:
    """Package a synced remote repository lists at a version, installed there in the session, with the workspaces that place it."""

    identity: str
    repository: str
    version: str
    workspaces: tuple[Task, ...]


@frozen
class Launch:
    """Session call: its report file, render output folder, unit system member name, built extension's manifest id, bridge port, package rows, and the Visual Studio Code command-line and Inkscape executables found."""

    report: str
    renders: str
    units: str
    extension: str
    port: int
    packages: tuple[Local | Listed, ...]
    editor: str | None
    inkscape: str | None


# --- [OPERATIONS] -----------------------------------------------------------------------


def stamp(files: Mapping[str, tuple[int, int]]) -> str:
    """Digest of one line per relative path with its CRC-32 and size, in path order."""
    return digest(b"".join(f"{name}\0{crc}\0{size}\n".encode() for name, (crc, size) in sorted(files.items())))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Archive", "Installation", "Launch", "Listed", "Local", "stamp"]
