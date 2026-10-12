"""Assemble Blender extensions and import folders from the locked Python runtime."""

import ast
import os
from pathlib import Path
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
            "--frozen",
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
    await anyio.run_process(("pip", "--python", interpreter.executable, *placement, "--no-deps", "--requirement", lock), stdout=None, stderr=None)
    return interpreter


@as_async_result(OSError, subprocess.CalledProcessError, BadZipFile, msgspec.DecodeError, SyntaxError, ValueError, PathOpsError)
async def pack(source: ResolvedPath, archive: ResolvedPath, /, *modules: Path, blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")]) -> int:
    """Build a Blender extension for the selected Blender interpreter.

    Args:
        source: Extension directory containing its manifest and assets.
        archive: ZIP file to create or replace.
        modules: First-party wheel modules in the extension's runtime import closure.
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
        (first_party,) = (wheel for wheel in wheels.glob("*.whl") if parse_wheel_filename(wheel.name)[0] == canonicalize_name(root.project.name))
        selected = frozenset(modules)
        packages = {module.parent for module in selected}
        code_suffixes = {".py", ".pyi"}
        with ZipFile(first_party) as wheel:
            entries = tuple((member, Path(member.filename)) for member in wheel.infolist())
            code_packages = {path.parent for _, path in entries if path.suffix in code_suffixes}
            members = tuple(
                member
                for member, path in entries
                if path in selected
                or (path.suffix == ".pyi" and path.with_suffix(".py") in selected)
                or (path.suffix not in code_suffixes and next((parent for parent in path.parents if parent in code_packages), None) in packages)
            )
            roots = {Path(member.filename).parts[0] for member in members}
            wheel.extractall(stage, members)
        first_party.unlink()
        additions = [member.filename for member in members]
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
        for master in icons.glob("*.svg"):
            previews.mkdir(exist_ok=True)
            preview = (previews / master.name).with_suffix(".png")
            await anyio.run_process(("resvg", "--width", size, "--height", size, master, preview), stdout=None, stderr=None)
            additions.append(preview.relative_to(stage).as_posix())
        for master in tools.glob("*.svg"):
            glyph = (previews / tools.name / master.name).with_suffix(".dat")
            glyph.parent.mkdir(parents=True, exist_ok=True)
            glyph.write_bytes(encoded(master))
            additions.append(glyph.relative_to(stage).as_posix())
        manifest = stage / "blender_manifest.toml"
        document = tomlkit.parse(manifest.read_text())
        document["platforms"] = [interpreter.platform]
        manifest_wheels = tomlkit.array().multiline(multiline=True)
        manifest_wheels.extend(f"./wheels/{wheel.name}" for wheel in sorted(wheels.glob("*.whl")))
        document["wheels"] = manifest_wheels
        if (build := document.get("build")) is not None and (paths := build.get("paths")):
            declared = {os.path.normpath(path).lower() for path in paths}  # ruff: ignore[blocking-path-method-in-async-function]
            paths.extend(path for path in additions if os.path.normpath(path).lower() not in declared)  # ruff: ignore[blocking-path-method-in-async-function]
        manifest.write_text(tomlkit.dumps(document))
        await anyio.Path(archive.parent).mkdir(parents=True, exist_ok=True)
        await anyio.run_process((blender, "--factory-startup", "--command", "extension", "build", "--source-dir", stage, "--output-filepath", archive), stdout=None, stderr=None)
    return 0


@as_async_result(OSError, subprocess.CalledProcessError, msgspec.DecodeError, ValueError)
async def site(target: ResolvedPath, /, *, blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")]) -> int:
    """Install the runtime closure `pack` bundles, with the first-party package whole, into a folder Blender's interpreter imports from.

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
