"""Assemble Blender extensions and import folders from the locked Python runtime."""

import ast
from pathlib import Path, PurePosixPath
import shutil
import subprocess
import tokenize
from typing import Annotated
from zipfile import BadZipFile, ZipFile

import anyio
from asttokens import ASTText
from asttokens.util import replace
from cyclopts import Parameter
from cyclopts.types import ResolvedExistingFile, ResolvedPath
import msgspec
from packaging.requirements import Requirement
from packaging.utils import canonicalize_name, parse_wheel_filename
from pathops import PathOpsError  # type: ignore[attr-defined]
from result import as_async_result
import tomlkit

from eng.python.fitout.icons import encoded

# --- [MODELS] ---------------------------------------------------------------------------


class _Interpreter(msgspec.Struct, frozen=True):
    executable: str
    platform: str
    markers: dict[str, str]


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


async def _resolved(blender: Path, root: _Root, lock: Path, *placement: str | Path) -> _Interpreter:
    """Place the locked runtime closure using Blender's interpreter and bundled providers."""
    host = await anyio.run_process(
        (
            blender,
            "--background",
            "--factory-startup",
            "--python-exit-code",
            "1",
            "--python-expr",
            (
                "import json, sys; from bl_pkg.cli.blender_ext import platform_from_this_system; from packaging.markers import default_environment; "
                "sys.stderr.write(json.dumps({'executable': sys.executable, 'platform': platform_from_this_system(), 'markers': default_environment()}))"
            ),
        ),
        stdout=None,
    )
    interpreter = msgspec.json.decode(host.stderr, type=_Interpreter)
    bundled = (requirement.name for requirement in map(Requirement, root.tool.uv.constraint_dependencies) if requirement.marker and requirement.marker.evaluate(interpreter.markers))
    await anyio.run_process(
        (
            "uv",
            "export",
            "--locked",
            "--no-default-groups",
            "--no-editable",
            "--no-hashes",
            "--format",
            "requirements.txt",
            *(argument for name in bundled for argument in ("--no-emit-package", name)),
            "--output-file",
            lock,
        ),
        stdout=subprocess.DEVNULL,
        stderr=None,
    )
    await anyio.run_process(("pip", "--python", interpreter.executable, "--disable-pip-version-check", *placement, "--no-deps", "--requirement", lock), stdout=None, stderr=None)
    return interpreter


@as_async_result(OSError, subprocess.CalledProcessError, BadZipFile, msgspec.DecodeError, SyntaxError, ValueError, PathOpsError)
async def pack(source: ResolvedPath, archive: ResolvedPath, /, *, blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")]) -> int:
    """Build a Blender extension holding the first-party packages as submodules and every other distribution as a wheel.

    Args:
        source: Extension directory containing its manifest and assets.
        archive: ZIP file to create or replace.
        blender: Blender executable, supplied by BLENDER_PATH.

    Returns:
        Zero once built, or an input, process, or archive error.
    """
    root = msgspec.toml.decode(await anyio.Path("pyproject.toml").read_bytes(), type=_Root)
    icons = source / "icons"
    tools = icons / "tools"
    async with anyio.TemporaryDirectory() as folder:
        scratch = Path(folder)
        stage = scratch / "stage"
        wheels = stage / "wheels"
        previews = stage / icons.name
        shutil.copytree(source, stage, ignore=lambda parent, names: [name for name in names if Path(parent, name) in {icons, source / "pyproject.toml"}])
        lock = scratch / "requirements.txt"
        interpreter = await _resolved(blender, root, lock, "wheel", "--wheel-dir", wheels)
        (first_party,) = [wheel async for wheel in anyio.Path(wheels).glob("*.whl") if parse_wheel_filename(wheel.name)[0] == canonicalize_name(root.project.name)]
        with ZipFile(first_party) as wheel:
            members = [name for name in wheel.namelist() if not PurePosixPath(name).parts[0].endswith(".dist-info")]
            wheel.extractall(stage, members)
        await first_party.unlink()
        roots = {PurePosixPath(name).parts[0] for name in members}
        async for module in anyio.Path(stage).rglob("*.py"):
            with tokenize.open(module) as stream:
                text = stream.read()
                encoding = stream.encoding
            syntax = ASTText(text, filename=str(module))
            edits = [
                (*syntax.get_text_range(node, padded=False), ast.unparse(ast.ImportFrom(module=node.module, names=node.names, level=len(module.relative_to(stage).parent.parts) + 1)))
                for node in ast.walk(syntax.tree)
                if isinstance(node, ast.ImportFrom) and node.level == 0 and node.module is not None and node.module.split(".")[0] in roots
            ]
            if edits:
                await module.write_text(replace(text, edits), encoding=encoding)
        size = "256"
        async for master in anyio.Path(icons).glob("*.svg"):
            await anyio.Path(previews).mkdir(exist_ok=True)
            await anyio.run_process(("resvg", "--width", size, "--height", size, master, (previews / master.name).with_suffix(".png")), stdout=None, stderr=None)
        async for master in anyio.Path(tools).glob("*.svg"):
            glyph = anyio.Path(previews, tools.name, master.name).with_suffix(".dat")
            await glyph.parent.mkdir(parents=True, exist_ok=True)
            await glyph.write_bytes(encoded(Path(master)))
        manifest = anyio.Path(stage, "blender_manifest.toml")
        document = tomlkit.parse(await manifest.read_text())
        document["platforms"] = [interpreter.platform]
        manifest_wheels = tomlkit.array().multiline(multiline=True)
        manifest_wheels.extend(f"./wheels/{name}" for name in sorted([wheel.name async for wheel in anyio.Path(wheels).glob("*.whl")]))
        document["wheels"] = manifest_wheels
        await manifest.write_text(tomlkit.dumps(document))
        await anyio.Path(archive.parent).mkdir(parents=True, exist_ok=True)
        await anyio.run_process((blender, "--factory-startup", "--command", "extension", "build", "--source-dir", stage, "--output-filepath", archive), stdout=None, stderr=None)
    return 0


@as_async_result(OSError, subprocess.CalledProcessError, msgspec.DecodeError, ValueError)
async def site(target: ResolvedPath, /, *, blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")]) -> int:
    """Install the distributions `pack` bundles into a folder Blender's interpreter imports from.

    Args:
        target: Folder receiving the installed packages.
        blender: Blender executable, supplied by BLENDER_PATH.

    Returns:
        Zero once installed, or a process, decoding, or requirement error.
    """
    root = msgspec.toml.decode(await anyio.Path("pyproject.toml").read_bytes(), type=_Root)
    async with anyio.TemporaryDirectory() as scratch:
        lock = Path(scratch) / "requirements.txt"
        await _resolved(blender, root, lock, "install", "--no-compile", "--target", target)
    return 0


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["pack", "site"]
