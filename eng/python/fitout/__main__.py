"""Package and install project extensions through Blender and Yak."""

from pathlib import Path
from subprocess import CalledProcessError
import sys
from typing import Annotated
from zipfile import BadZipFile, ZipFile

import anyio
from cyclopts import App, Parameter
from cyclopts.types import ResolvedDirectory, ResolvedExistingDirectory
from expression import Error, Ok, Result
import msgspec

# --- [TYPES] ----------------------------------------------------------------------------

type Failure = ArchiveSelection | OSError | CalledProcessError | BadZipFile | KeyError | msgspec.DecodeError

# --- [ERRORS] ---------------------------------------------------------------------------


class ArchiveSelection(msgspec.Struct, frozen=True):
    """Input directory containing zero or multiple extension archives."""

    directory: Path
    archives: tuple[Path, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


async def executed(directory: Path, *commands: tuple[str, ...]) -> Result[None, OSError | CalledProcessError]:
    """Run dependent native commands with their output attached, stopping at the first failure."""
    try:
        for command in commands:
            await anyio.run_process(command, cwd=directory, stdout=None, stderr=None)
    except (OSError, CalledProcessError) as error:
        return Error(error)
    return Ok(None)


def reported(result: Result[Path, Failure]) -> int:
    """Print the completed archive or failure and return the CLI exit code."""
    match result:
        case Result(tag="ok", ok=archive):
            sys.stdout.write(f"{archive}\n")
            return 0
        case Result(error=ArchiveSelection(directory=directory, archives=archives)):
            sys.stderr.write(f"{directory} must contain one .zip or .yak archive; found {len(archives)}: {', '.join(path.name for path in archives)}\n")
            return 1
        case Result(error=CalledProcessError() as error):
            sys.stderr.write(f"{error}\n")
            return error.returncode
        case Result(error=error):
            sys.stderr.write(f"{type(error).__name__}: {error}\n")
            return 1


# --- [COMPOSITION] ----------------------------------------------------------------------

app = App(help=__doc__, result_action=(reported, "sys_exit"))


@app.command
async def pack(source: ResolvedExistingDirectory, folder: ResolvedDirectory, /, *, blender: Annotated[str, Parameter(env_var="BLENDER_PATH")] = "blender") -> Result[Path, Failure]:
    """Build a Blender extension archive from its manifest."""
    try:
        folder.mkdir(parents=True, exist_ok=True)
        for archive in folder.glob("*.zip"):
            archive.unlink()
    except OSError as error:
        return Error(error)
    archive = folder / "extension.zip"
    return (await executed(source, (blender, "--factory-startup", "--command", "extension", "build", "--source-dir", str(source), "--output-filepath", str(archive)))).map(lambda _: archive)


@app.command
async def install(
    folder: ResolvedExistingDirectory, /, *, blender: Annotated[str, Parameter(env_var="BLENDER_PATH")] = "blender", yak: str = "yak", repository: str = "user_default"
) -> Result[Path, Failure]:
    """Replace the directory's packaged extension through its host; running applications load it on their next start."""

    def identity(archive: Path) -> str:
        """Read the Yak package name from its native archive manifest."""

        class Manifest(msgspec.Struct, frozen=True):
            """Yak manifest identity used for removal before installation."""

            name: str

        with ZipFile(archive) as package:
            return msgspec.yaml.decode(package.read("manifest.yml"), type=Manifest).name

    try:
        match [path for path in folder.iterdir() if path.suffix in {".zip", ".yak"} and path.is_file()]:
            case [archive]:
                commands = (
                    ((yak, "uninstall", identity(archive)), (yak, "install", archive.name))
                    if archive.suffix == ".yak"
                    else ((blender, "--command", "extension", "install-file", "--repo", repository, "--enable", archive.name),)
                )
            case archives:
                return Error(ArchiveSelection(folder, tuple(archives)))
    except (OSError, BadZipFile, KeyError, msgspec.DecodeError) as error:
        return Error(error)
    return (await executed(folder, *commands)).map(lambda _: archive)


if __name__ == "__main__":
    app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["app"]
