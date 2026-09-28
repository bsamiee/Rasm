"""Installs a published Rhino plug-in's yak package into the Rhino the `yak` on PATH serves."""

from pathlib import Path
import re
import sys
import zipfile

import anyio
import cyclopts
import msgspec

# --- [MODELS] ---------------------------------------------------------------------------


class Manifest(msgspec.Struct, frozen=True):
    """Name and version a yak package's `manifest.yml` states."""

    name: str
    version: str


class Outcome(msgspec.Struct, frozen=True):
    """Package directory `yak list` names, the installed manifest, and whether the run replaced the package."""

    directory: str
    manifest: Manifest
    changed: bool


# --- [COMPOSITION] ----------------------------------------------------------------------


async def install(folder: Path) -> None:
    """Replaces the installed package with the folder's yak package when its installed files differ from what `yak install` writes, and prints the outcome as JSON."""
    (archive,) = [path async for path in anyio.Path(folder).glob("*.yak")]
    with zipfile.ZipFile(archive) as contents:
        manifest = msgspec.yaml.decode(contents.read("manifest.yml"), type=Manifest)
        written = {f"{manifest.version}/{entry.filename}": contents.read(entry) for entry in contents.infolist()} | {"manifest.txt": f"{manifest.version}\n".encode()}
    (directory,) = re.findall(r"^Package directory: (.+)$", (await anyio.run_process(["yak", "list"], stderr=None)).stdout.decode(), re.MULTILINE)
    package = anyio.Path(directory, manifest.name)
    if changed := written != {path.relative_to(package).as_posix(): await path.read_bytes() async for path in package.rglob("*") if await path.is_file()}:
        for command in (("uninstall", manifest.name), ("install", str(archive))):
            await anyio.run_process(["yak", *command], stderr=None)
    sys.stdout.buffer.write(msgspec.json.encode(Outcome(directory, manifest, changed)) + b"\n")


if __name__ == "__main__":
    cyclopts.run(install)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["install"]
