"""Publish a packed NuGet package or a Python package built from the root manifest to its registry."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Awaitable, Callable, Mapping
from pathlib import Path
import shutil
import tomllib
from typing import Annotated

import anyio
import cyclopts
from expression import Error, Ok, Result
import msgspec
from pydantic import Field, ValidationError
from pydantic_settings import BaseSettings, SettingsConfigDict
import structlog

from eng.scripts.provision import exit_code, Failure, FileMissing, HostUnsupported, message, PinMismatch, repository_root, run

# --- [TYPES] ----------------------------------------------------------------------------


class Publication(msgspec.Struct, frozen=True, gc=False):
    """Publication result with the registry, files, and requested dry-run mode."""

    registry: str
    files: tuple[str, ...]
    dry_run: bool = False


class Settings(BaseSettings):
    """Environment variables the publish command reads, the release workflow sets the api key."""

    model_config = SettingsConfigDict(case_sensitive=True)

    nuget_api_key: str = Field(validation_alias="NUGET_API_KEY")


# --- [CONSTANTS] ------------------------------------------------------------------------

_NUGET_SOURCE = "https://api.nuget.org/v3/index.json"

_log = structlog.get_logger(__name__)
_app = cyclopts.App(name="publish")

# --- [MANIFEST] -------------------------------------------------------------------------


def _dependencies(groups: Mapping[str, list[str | Mapping[str, str]]], name: str) -> list[str]:
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


async def _nuget(root: Path, project_root: Path) -> Result[Publication, Failure]:
    """Pack the project in Release and push the package identified by NuGet's output items."""
    try:
        settings = Settings()
    except ValidationError as error:
        return Error(HostUnsupported(f"the push needs NUGET_API_KEY, {error}"))
    match await run(
        ["dotnet", "pack", str(project_root), "--configuration", "Release", "--no-restore", "-getItem:NuGetPackOutput"], root, capture=True
    ):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=output):
            pass
    items = msgspec.json.decode(output, type=dict[str, dict[str, list[dict[str, str]]]])["Items"]["NuGetPackOutput"]
    if not (packages := tuple(item["FullPath"] for item in items if item["Extension"] == ".nupkg")):
        return Error(FileMissing(root / project_root, "NuGetPackOutput .nupkg"))
    (package,) = packages
    push = ["dotnet", "nuget", "push", package, "--api-key", settings.nuget_api_key, "--source", _NUGET_SOURCE, "--skip-duplicate"]
    return (await run(push, root)).map(lambda _: Publication("nuget.org", (package,)))


async def _pypi(root: Path, project_root: Path) -> Result[Publication, Failure]:
    """Build the package at the root from a generated manifest and publish its distributions through trusted publishing."""
    name = project_root.name  # The Nx project name and the distribution name, the release tag is <name>@<version>
    match await _version(root, name):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=version):
            pass
    manifest_name = "pyproject.toml"
    manifest = tomllib.loads((root / manifest_name).read_text())
    build_dir, out_dir = root / ".artifacts" / "python" / "build" / name, root / ".artifacts" / "python" / "dist" / name
    shutil.rmtree(build_dir, ignore_errors=True)
    _ = shutil.copytree(root / project_root, build_dir / name, ignore=shutil.ignore_patterns("__pycache__"))
    _ = (build_dir / manifest_name).write_text(
        _manifest(name, version, manifest["project"]["requires-python"], _dependencies(manifest.get("dependency-groups", {}), name))
    )
    match await run(["uv", "build", str(build_dir), "--out-dir", str(out_dir), "--clear", "--no-create-gitignore"], root):
        case Result(tag="error", error=build_error):
            return Error(build_error)
        case Result():
            pass
    if not (files := tuple(str(path.relative_to(root)) for path in sorted(out_dir.iterdir()))):
        return Error(FileMissing(out_dir, f"{name}-{version}*"))
    return (await run(["uv", "publish", "--trusted-publishing", "always", *files], root)).map(lambda _: Publication("pypi.org", files))


def _publish(command: Callable[[Path], Awaitable[Result[Publication, Failure]]], registry: str, *, dry_run: bool) -> Result[Publication, Failure]:
    """Skip publication for a dry run, otherwise run the command from the repository root."""
    return (
        Ok(Publication(registry, (), dry_run=True))
        if dry_run
        else repository_root(Path(__file__).resolve()).bind(lambda found: anyio.run(command, found))
    )


def _report(published: Publication) -> None:
    _log.info("publish skipped" if published.dry_run else "pushed", registry=published.registry, files=list(published.files))


_app.result_action = (exit_code(_report, message), "sys_exit")

# --- [CLI] ------------------------------------------------------------------------------


@_app.command
def nuget(project_root: Path, *, dry_run: Annotated[bool, cyclopts.Parameter(env_var="NX_DRY_RUN")] = False) -> Result[Publication, Failure]:
    """Pack the .NET project at the root in Release and push its package to nuget.org."""
    return _publish(lambda root: _nuget(root, project_root), "nuget.org", dry_run=dry_run)


@_app.command
def pypi(project_root: Path, *, dry_run: Annotated[bool, cyclopts.Parameter(env_var="NX_DRY_RUN")] = False) -> Result[Publication, Failure]:
    """Build the Python package at the root, versioned by its release tag, and publish its distributions to pypi.org."""
    return _publish(lambda root: _pypi(root, project_root), "pypi.org", dry_run=dry_run)


if __name__ == "__main__":
    _app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Publication", "Settings", "nuget", "pypi"]
