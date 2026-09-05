"""Publish a packed NuGet package or a Python package built from the root manifest to its registry."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Awaitable, Callable, Mapping
from pathlib import Path
import shutil
import tomllib

import anyio
import cyclopts
from expression import Error, Ok, Result
import msgspec
from pydantic import Field, ValidationError
from pydantic_settings import BaseSettings, SettingsConfigDict
import structlog

from eng.scripts.provision import exit_code, Failure, FileMissing, HostUnsupported, PinMismatch, repository_root, run

# --- [TYPES] ----------------------------------------------------------------------------


class Published(msgspec.Struct, frozen=True, gc=False):
    """Files pushed to a registry."""

    registry: str
    files: tuple[str, ...]


class Settings(BaseSettings):
    """Environment variables the publish command reads, the release workflow sets the api key."""

    model_config = SettingsConfigDict(case_sensitive=True)

    nuget_api_key: str = Field(default="", validation_alias="NUGET_API_KEY")


type _Command = Callable[[Path, Settings], Awaitable[Result[Published, Failure]]]
type _Groups = Mapping[str, list[str | Mapping[str, str]]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_NUGET_SOURCE = "https://api.nuget.org/v3/index.json"
_NUGET_OUTPUT = Path(".artifacts/dotnet/package/release")  # dotnet pack writes under the ArtifactsPath of the root Directory.Build.props
_PYTHON_BUILD = Path(".artifacts/python/build")
_PYTHON_DIST = Path(".artifacts/python/dist")

_log = structlog.get_logger(__name__)
_app = cyclopts.App(name="publish")

# --- [MANIFEST] -------------------------------------------------------------------------


def _dependencies(groups: _Groups, name: str) -> list[str]:
    """Return the rows of a dependency group with every included group expanded, a package without a group has no dependencies."""
    rows: list[str] = []
    for row in groups.get(name, []):
        rows.extend([row] if isinstance(row, str) else _dependencies(groups, row["include-group"]))
    return rows


def _manifest(name: str, version: str, requires_python: str, dependencies: list[str]) -> str:
    """Return the manifest of one package, the flat module beside it is the build input."""
    rows = msgspec.json.encode(dependencies).decode()
    return (
        f'[project]\nname = "{name}"\nversion = "{version}"\nrequires-python = "{requires_python}"\ndependencies = {rows}\n\n'
        '[build-system]\nrequires = ["uv_build"]\nbuild-backend = "uv_build"\n\n[tool.uv.build-backend]\nmodule-root = ""\n'
    )


async def _version(root: Path, project: str) -> Result[str, Failure]:
    """Return the version of the newest release tag of a project reachable from HEAD."""
    match await run(["git", "describe", "--tags", "--abbrev=0", "--match", f"{project}@*"], root, capture=True):
        case Result(tag="error"):
            return Error(
                PinMismatch(f"Release tag of {project}", "is absent from the history of HEAD, nx release creates it before the publish step")
            )
        case Result(ok=tag):
            return Ok(tag.strip().removeprefix(f"{project}@"))


# --- [OPERATIONS] -----------------------------------------------------------------------


async def _nuget(root: Path, settings: Settings, project_root: Path) -> Result[Published, Failure]:
    """Pack the project in Release and push the newest package it produced."""
    match await run(["dotnet", "pack", str(project_root), "--configuration", "Release", "--no-restore"], root):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result():
            pass
    packages = sorted((root / _NUGET_OUTPUT).glob(f"{project_root.name}.[0-9]*.nupkg"), key=lambda path: path.stat().st_mtime)
    if not packages:
        return Error(FileMissing(root / _NUGET_OUTPUT, f"{project_root.name}.*.nupkg"))
    if not settings.nuget_api_key:
        return Error(HostUnsupported("NUGET_API_KEY is empty and the push needs it"))
    package = str(packages[-1].relative_to(root))
    push = ["dotnet", "nuget", "push", package, "--api-key", settings.nuget_api_key, "--source", _NUGET_SOURCE, "--skip-duplicate"]
    return (await run(push, root)).map(lambda _: Published("nuget.org", (package,)))


async def _pypi(root: Path, _settings: Settings, project_root: Path) -> Result[Published, Failure]:
    """Build the package at the root from a generated manifest and publish its distributions through trusted publishing."""
    name = project_root.name  # The Nx project name and the distribution name, the release tag is <name>@<version>
    match await _version(root, name):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=version):
            pass
    manifest = tomllib.loads((root / "pyproject.toml").read_text())
    build_dir, out_dir = root / _PYTHON_BUILD / name, root / _PYTHON_DIST / name
    for directory in (build_dir, out_dir):
        shutil.rmtree(directory, ignore_errors=True)
    _ = shutil.copytree(root / project_root, build_dir / name, ignore=shutil.ignore_patterns("__pycache__"))
    _ = (build_dir / "pyproject.toml").write_text(
        _manifest(name, version, manifest["project"]["requires-python"], _dependencies(manifest.get("dependency-groups", {}), name))
    )
    match await run(["uv", "build", str(build_dir), "--out-dir", str(out_dir)], root):
        case Result(tag="error", error=build_error):
            return Error(build_error)
        case Result():
            pass
    files = tuple(str(path.relative_to(root)) for path in sorted(out_dir.iterdir()))
    if not files:
        return Error(FileMissing(out_dir, f"{name}-{version}*"))
    return (await run(["uv", "publish", "--trusted-publishing", "always", *files], root)).map(lambda _: Published("pypi.org", files))


def _publish(command: _Command) -> Result[Published, Failure]:
    """Read the environment, find the repository root, and run the publish command from the root."""
    try:
        settings = Settings()
    except ValidationError as error:
        return Error(HostUnsupported(f"the environment holds an invalid NUGET_API_KEY value, {error}"))
    return repository_root(Path(__file__).resolve()).bind(lambda found: anyio.run(command, found, settings))


def _report(published: Published) -> None:
    _log.info("pushed", registry=published.registry, files=list(published.files))


_app.result_action = (exit_code(_report), "sys_exit")

# --- [CLI] ------------------------------------------------------------------------------


@_app.command
def nuget(project_root: Path) -> Result[Published, Failure]:
    """Pack the .NET project at the root in Release and push its package to nuget.org."""
    return _publish(lambda root, settings: _nuget(root, settings, project_root))


@_app.command
def pypi(project_root: Path) -> Result[Published, Failure]:
    """Build the Python package at the root, versioned by its release tag, and publish its distributions to pypi.org."""
    return _publish(lambda root, settings: _pypi(root, settings, project_root))


if __name__ == "__main__":
    _app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Published", "Settings", "nuget", "pypi"]
