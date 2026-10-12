"""Assemble a local Blender extension from the locked Python runtime."""

import ast
from collections.abc import Mapping
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
from cyclopts.types import ResolvedPath
import httpx2
import msgspec
from packaging.requirements import Requirement
from packaging.utils import canonicalize_name, parse_wheel_filename
from pathops import PathOpsError  # type: ignore[attr-defined]
from result import as_async_result
import tomlkit

from eng.python.fitout.icons import outlined

# --- [MODELS] ---------------------------------------------------------------------------


class _Interpreter(msgspec.Struct, frozen=True):
    executable: str
    build_hash: str
    platform: str
    markers: dict[str, str]


class _Project(msgspec.Struct, frozen=True):
    name: str


class _Uv(msgspec.Struct, frozen=True, rename="kebab"):
    constraint_dependencies: tuple[str, ...]


class _Tool(msgspec.Struct, frozen=True):
    uv: _Uv


class _Declaration(msgspec.Struct, frozen=True):
    project: _Project
    tool: _Tool


# --- [OPERATIONS] -----------------------------------------------------------------------


async def _resolved(blender: str, environment: Mapping[str, str], declaration: _Declaration, lock: Path, *placement: str | Path) -> _Interpreter:
    """Blender's interpreter, with the locked runtime closure minus the providers whose root uv constraint markers hold for it exported to `lock` and placed by the pip `placement` command."""
    host = await anyio.run_process(
        (
            blender,
            "--background",
            "--factory-startup",
            "--python-exit-code",
            "1",
            "--python-expr",
            (
                "import bpy, json, sys; from bl_pkg.cli.blender_ext import platform_from_this_system; from packaging.markers import default_environment; "
                "sys.stderr.write(json.dumps({'executable': sys.executable, 'build_hash': bpy.app.build_hash.decode(), 'platform': platform_from_this_system(), 'markers': default_environment()}))"
            ),
        ),
        env=environment,
        stdout=None,
    )
    interpreter = msgspec.json.decode(host.stderr, type=_Interpreter)
    bundled = (requirement.name for requirement in map(Requirement, declaration.tool.uv.constraint_dependencies) if requirement.marker and requirement.marker.evaluate(interpreter.markers))
    await anyio.run_process(
        (
            "uv",
            "export",
            "--frozen",
            "--no-default-groups",
            "--no-editable",
            "--format",
            "pylock.toml",
            *(argument for name in bundled for argument in ("--no-emit-package", name)),
            "--output-file",
            lock,
        ),
        stdout=subprocess.DEVNULL,
        stderr=None,
    )
    await anyio.run_process(("pip", "--python", interpreter.executable, *placement, "--no-deps", "--no-require-hashes", "--requirement", lock), stdout=None, stderr=None)
    return interpreter


@as_async_result(OSError, subprocess.CalledProcessError, BadZipFile, httpx2.HTTPError, msgspec.DecodeError, SyntaxError, ValueError, PathOpsError)
async def pack(source: ResolvedPath, archive: ResolvedPath, /, *, modules: tuple[Path, ...] = (), blender: Annotated[str, Parameter(env_var="BLENDER_PATH")] = "blender") -> int:
    """Build a Blender extension at the requested archive path.

    Args:
        source: Extension directory containing its manifest.
        archive: ZIP file to create or replace.
        modules: First-party wheel modules in the extension's runtime import closure.
        blender: Blender executable, supplied by BLENDER_PATH when set.

    Returns:
        Zero once built, or an input, process, archive, or network error.
    """
    declaration = msgspec.toml.decode(await anyio.Path("pyproject.toml").read_bytes(), type=_Declaration)
    icons = source / "icons"
    async with anyio.TemporaryDirectory() as scratch:
        stage = Path(scratch, "stage")
        wheels = stage / "wheels"
        previews = stage / icons.name
        extensions = Path(scratch, "extensions")
        environment = {**os.environ, "BLENDER_USER_EXTENSIONS": str(extensions)}
        shutil.copytree(source, stage, ignore=lambda folder, names: [name for name in names if Path(folder, name) in {icons, source / "pyproject.toml"}])
        for folder in (wheels, previews, extensions):
            folder.mkdir()
        interpreter = await _resolved(blender, environment, declaration, Path(scratch, "pylock.toml"), "wheel", "--wheel-dir", wheels)
        (first_party,) = (wheel for wheel in wheels.glob("*.whl") if parse_wheel_filename(wheel.name)[0] == canonicalize_name(declaration.project.name))
        selected = frozenset(modules)
        packages = {module.parent for module in selected}
        with ZipFile(first_party) as wheel:
            members = tuple(
                member
                for member in wheel.infolist()
                for path in (Path(member.filename),)
                if path in selected
                or (path.suffix == ".pyi" and path.with_suffix(".py") in selected)
                or (path.suffix not in {".py", ".pyi"} and any(path.is_relative_to(package) for package in packages))
            )
            roots = {Path(member.filename).parts[0] for member in members}
            wheel.extractall(stage, members)
        first_party.unlink()
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
        preview_size = "256"
        for master in icons.glob("*.svg"):
            await anyio.run_process(("resvg", "--width", preview_size, "--height", preview_size, master, (previews / master.name).with_suffix(".png")), stdout=None, stderr=None)
        tools = icons / "tools"
        if glyphs := outlined(tools):
            async with httpx2.AsyncClient() as client:
                exporter = await client.get(f"https://raw.githubusercontent.com/blender/blender/{interpreter.build_hash}/release/datafiles/blender_icons_geom.py")
                exporter.raise_for_status()
            exporter_path = anyio.Path(scratch, "blender_icons_geom.py")
            await exporter_path.write_text(exporter.text, encoding="utf-8")
            geometry = Path(__file__).with_name("blender") / "geometry.py"
            await anyio.run_process(
                (
                    blender,
                    "--background",
                    "--factory-startup",
                    "--python-exit-code",
                    "1",
                    "--python-expr",
                    f"from pathlib import Path; import runpy; runpy.run_path({str(geometry)!r})['packed']({glyphs!r}, Path({str(previews / tools.name)!r}), Path({str(exporter_path)!r}))",
                ),
                env=environment,
                stdout=None,
                stderr=None,
            )
        manifest = stage / "blender_manifest.toml"
        document = tomlkit.parse(manifest.read_text())
        document["platforms"] = [interpreter.platform]
        manifest_wheels = tomlkit.array().multiline(multiline=True)
        manifest_wheels.extend(f"./wheels/{wheel.name}" for wheel in sorted(wheels.glob("*.whl")))
        document["wheels"] = manifest_wheels
        manifest.write_text(tomlkit.dumps(document))
        await anyio.Path(archive.parent).mkdir(parents=True, exist_ok=True)
        await anyio.run_process((blender, "--factory-startup", "--command", "extension", "build", "--source-dir", stage, "--output-filepath", archive), env=environment, stdout=None, stderr=None)
    return 0


@as_async_result(OSError, subprocess.CalledProcessError, msgspec.DecodeError, ValueError)
async def site(target: ResolvedPath, /, *, blender: Annotated[str, Parameter(env_var="BLENDER_PATH")] = "blender") -> int:
    """Install the runtime closure `pack` bundles, with the first-party package whole, into a folder Blender's interpreter imports from.

    Args:
        target: Folder receiving the installed packages.
        blender: Blender executable, supplied by BLENDER_PATH when set.

    Returns:
        Zero once installed, or a process, decoding, or requirement error.
    """
    async with anyio.TemporaryDirectory() as scratch:
        await _resolved(
            blender, os.environ, msgspec.toml.decode(await anyio.Path("pyproject.toml").read_bytes(), type=_Declaration), Path(scratch, "pylock.toml"), "install", "--no-compile", "--target", target
        )
    return 0


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["pack", "site"]
