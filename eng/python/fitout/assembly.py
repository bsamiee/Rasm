"""Builds Blender extension archives and import folders from uv lock."""

import ast
from pathlib import Path, PurePosixPath
import shutil
import subprocess
import tempfile
from typing import Annotated
from zipfile import ZipFile

from asttokens import ASTText
from asttokens.util import replace
from cyclopts import Parameter
from cyclopts.types import ResolvedExistingDirectory, ResolvedExistingFile, ResolvedPath
from expression import Error, Ok, Result
from expression.extra.result import catch
import msgspec
from packaging.requirements import Requirement
from packaging.utils import canonicalize_name, parse_wheel_filename
import tomlkit

from eng.python.fitout.icons import encoded

# --- [MODELS] ---------------------------------------------------------------------------


class _Interpreter(msgspec.Struct, frozen=True):
    executable: str
    platform: str


class _Project(msgspec.Struct, frozen=True):
    name: str


class _Uv(msgspec.Struct, frozen=True, rename="kebab"):
    constraint_dependencies: tuple[str, ...]


class _Tool(msgspec.Struct, frozen=True):
    uv: _Uv


class _Root(msgspec.Struct, frozen=True):
    project: _Project
    tool: _Tool


# --- [OPERATIONS] -----------------------------------------------------------------------


def _ran(*command: str | Path, stdout: int | None = None, stderr: int | None = None) -> Result[subprocess.CompletedProcess[bytes], subprocess.CalledProcessError]:
    """Runs child process, nonzero exit as error."""
    return catch(lambda: subprocess.run(command, stdout=stdout, stderr=stderr, check=True), exception=subprocess.CalledProcessError)()


def _interpreter(blender: Path) -> Result[_Interpreter, subprocess.CalledProcessError]:
    """Returns Blender Python executable and host extension platform."""
    probe = "import json, sys; from bl_pkg.cli.blender_ext import platform_from_this_system; sys.stderr.write(json.dumps({'executable': sys.executable, 'platform': platform_from_this_system()}))"
    return _ran(blender, "--background", "--factory-startup", "--python-exit-code", "1", "--python-expr", probe, stderr=subprocess.PIPE).map(
        lambda host: msgspec.json.decode(host.stderr, type=_Interpreter)
    )


def _requirements(root: _Root, lock: Path, *exported: str) -> Result[Path, subprocess.CalledProcessError]:
    """Returns `lock` holding locked runtime closure without packages Blender bundles."""
    excluded = (*("--no-emit-package", Requirement(entry).name) for entry in root.tool.uv.constraint_dependencies)
    return _ran(
        "uv", "export", "--locked", "--no-default-groups", "--no-editable", "--no-hashes", "--format", "requirements.txt", *exported, *excluded, "--output-file", lock, stdout=subprocess.DEVNULL
    ).map(lambda _: lock)


def pack(
    source: ResolvedExistingDirectory, directory: ResolvedPath, /, *, version: str, blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")]
) -> Result[int, OSError | subprocess.CalledProcessError | ExceptionGroup[subprocess.CalledProcessError]]:
    """Builds host platform extension archive, first-party packages as submodules and other distributions as wheels.

    Args:
        source: Extension directory with manifest and assets.
        directory: Folder built archive replaces.
        version: Semantic version written to manifest.
        blender: Blender executable.

    Returns:
        Zero, or failed child process, icon render failure group, or archive placement error.
    """
    root = msgspec.toml.decode(Path("pyproject.toml").read_bytes(), type=_Root)
    icons = source / "icons"

    def staged(stage: Path, interpreter: _Interpreter) -> None:
        wheels = stage / "wheels"
        (first_party,) = [wheel for wheel in wheels.glob("*.whl") if parse_wheel_filename(wheel.name)[0] == canonicalize_name(root.project.name)]
        with ZipFile(first_party) as wheel:
            members = [name for name in wheel.namelist() if not PurePosixPath(name).parts[0].endswith(".dist-info")]
            wheel.extractall(stage, members)
        first_party.unlink()
        roots = {PurePosixPath(name).parts[0] for name in members}
        for module in stage.rglob("*.py"):
            syntax = ASTText(text := module.read_text(newline=""), filename=str(module))
            module.write_text(
                replace(
                    text,
                    [
                        (*syntax.get_text_range(node, padded=False), ast.unparse(ast.ImportFrom(module=node.module, names=node.names, level=len(module.relative_to(stage).parent.parts) + 1)))
                        for node in ast.walk(syntax.tree)
                        if isinstance(node, ast.ImportFrom) and node.level == 0 and node.module is not None and node.module.split(".")[0] in roots
                    ],
                ),
                newline="",
            )
        document = tomlkit.parse((manifest := stage / "blender_manifest.toml").read_text())
        document["version"] = version
        document["platforms"] = [interpreter.platform]
        document["wheels"] = tomlkit.item([f"./wheels/{wheel.name}" for wheel in sorted(wheels.glob("*.whl"))]).multiline(multiline=True)
        manifest.write_text(tomlkit.dumps(document))

    with tempfile.TemporaryDirectory() as folder:
        stage = Path(folder, "stage")
        previews = stage / icons.name
        shutil.copytree(
            source, stage, ignore=lambda parent, names: [name for name in names if Path(parent, name) == source / "pyproject.toml" or (Path(parent).is_relative_to(icons) and name.endswith(".svg"))]
        )
        for master in (icons / "tools").glob("*.svg"):
            (previews / master.relative_to(icons)).with_suffix(".dat").write_bytes(encoded(master))
        failures = [*_ran("resvg", "--width", "256", "--height", "256", master, previews / f"{master.stem}.png").swap().to_option().to_list() for master in icons.glob("*.svg")]
        rendered: Result[None, OSError | subprocess.CalledProcessError | ExceptionGroup[subprocess.CalledProcessError]] = (
            Error(ExceptionGroup("Icon previews failed to render", failures)) if failures else Ok(None)
        )
        lock = Path(folder, "requirements.txt")
        built = Path(folder, "archives")
        built.mkdir()

        def placed() -> Result[None, OSError]:
            return (
                catch(lambda: shutil.rmtree(directory), exception=OSError)()
                .or_else_with(lambda error: Ok(None) if isinstance(error, FileNotFoundError) else Error(error))
                .bind(lambda _: catch(lambda: directory.parent.mkdir(parents=True, exist_ok=True), exception=OSError)())
                .bind(lambda _: catch(lambda: shutil.move(built, directory), exception=OSError)())
                .map(lambda _: None)
            )

        return (
            rendered
            .bind(lambda _: _requirements(root, lock))
            .bind(lambda _: _interpreter(blender))
            .bind(
                lambda interpreter: _ran("pip", "--python", interpreter.executable, "--disable-pip-version-check", "wheel", "--wheel-dir", stage / "wheels", "--no-deps", "--requirement", lock).map(
                    lambda _: staged(stage, interpreter)
                )
            )
            .bind(lambda _: _ran(blender, "--factory-startup", "--command", "extension", "build", "--source-dir", stage, "--output-dir", built, "--split-platforms"))
            .bind(lambda _: placed())
            .map(lambda _: 0)
        )


def site(target: ResolvedPath, /, *, blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")]) -> Result[int, subprocess.CalledProcessError]:
    """Syncs Blender import folder to distributions `pack` bundles, reinstalling only on version or tag change.

    Args:
        target: Folder of synced packages.
        blender: Blender executable.

    Returns:
        Zero, or failed child process.
    """
    root = msgspec.toml.decode(Path("pyproject.toml").read_bytes(), type=_Root)
    with tempfile.TemporaryDirectory() as folder:
        return (
            _requirements(root, Path(folder, "requirements.txt"), "--no-emit-project")
            .bind(lambda lock: _interpreter(blender).bind(lambda interpreter: _ran("uv", "pip", "sync", "--python", interpreter.executable, "--target", target, lock)))
            .map(lambda _: 0)
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["pack", "site"]
