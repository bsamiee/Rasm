"""Assemble a local Blender extension from the locked Python runtime."""

import ast
import os
from pathlib import Path
import shutil
import subprocess
import tokenize
import tomllib
from typing import Annotated
from zipfile import BadZipFile, ZipFile

import anyio
from asttokens import ASTText
from asttokens.util import replace
from cyclopts import Parameter
from cyclopts.types import ResolvedPath
import httpx2
import msgspec
from packaging.utils import canonicalize_name, parse_wheel_filename
from pathops import PathOpsError  # type: ignore[attr-defined]
from result import as_async_result, as_result
import tomlkit

from eng.python.fitout.icons import outlined

# --- [MODELS] ---------------------------------------------------------------------------


class _Interpreter(msgspec.Struct, frozen=True):
    executable: str
    build_hash: str
    platform: str
    bundled: tuple[str, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


@as_async_result(OSError, subprocess.CalledProcessError, BadZipFile, httpx2.HTTPError, msgspec.DecodeError, SyntaxError, ValueError, PathOpsError)
async def pack(source: ResolvedPath, archive: ResolvedPath, /, *, modules: tuple[Path, ...] = (), blender: Annotated[str, Parameter(env_var="BLENDER_PATH")] = "blender") -> int:
    """Build a Blender extension at the requested archive path.

    Args:
        source: Extension directory containing its manifest.
        archive: ZIP file to create or replace.
        modules: First-party wheel modules in the extension's runtime import closure.
        blender: Blender executable, supplied by BLENDER_PATH when set.

    Returns:
        Native build status, or an input, process, archive, or network error.
    """
    workspace = Path(__file__).parents[3]
    stage = archive.parent / "stage"
    wheels = stage / "wheels"
    extensions = archive.parent / "extensions"
    lock = archive.parent / "pylock.toml"
    environment = {**os.environ, "BLENDER_USER_EXTENSIONS": str(extensions)}
    as_result(FileNotFoundError)(lambda: shutil.rmtree(stage))()
    shutil.copytree(source, stage, ignore=shutil.ignore_patterns("icons", "__pycache__", "*.pyc", "*.pyo"))
    wheels.mkdir()
    extensions.mkdir(exist_ok=True)
    host = await anyio.run_process(
        (
            blender,
            "--background",
            "--factory-startup",
            "--python-exit-code",
            "1",
            "--python-expr",
            (
                "import bpy, json, sys, sysconfig; from importlib.metadata import distributions; "
                "from bl_pkg.cli.blender_ext import platform_from_this_system; "
                "sys.stderr.write(json.dumps({'executable': sys.executable, 'build_hash': bpy.app.build_hash.decode(), 'platform': platform_from_this_system(), "
                "'bundled': [next(distributions(name='numpy', path=[sysconfig.get_path('purelib')])).name, "
                "*(name for name, library in (('opencolorio', bpy.app.ocio), ('openimageio', bpy.app.oiio)) if library.supported)]}))"
            ),
        ),
        env=environment,
        stdout=None,
    )
    interpreter = msgspec.json.decode(host.stderr, type=_Interpreter)
    declaration = tomllib.loads((workspace / "pyproject.toml").read_text())
    await anyio.run_process(
        (
            "uv",
            "export",
            "--frozen",
            "--no-default-groups",
            "--no-editable",
            "--format",
            "pylock.toml",
            *(argument for name in interpreter.bundled for argument in ("--no-emit-package", name)),
            "--output-file",
            lock,
        ),
        cwd=workspace,
        stdout=subprocess.DEVNULL,
        stderr=None,
    )
    await anyio.run_process(
        ("pip", "--python", interpreter.executable, "wheel", "--no-deps", "--no-require-hashes", "--requirement", lock, "--wheel-dir", wheels), cwd=workspace, stdout=None, stderr=None
    )
    (first_party,) = (wheel for wheel in wheels.glob("*.whl") if parse_wheel_filename(wheel.name)[0] == canonicalize_name(declaration["project"]["name"]))
    selected = frozenset(modules)
    packages = {module.parent for module in selected}
    with ZipFile(first_party) as wheel:
        members = tuple(
            member
            for member in wheel.infolist()
            for path in (Path(member.filename),)
            if path in selected or (path.suffix == ".pyi" and path.with_suffix(".py") in selected) or (path.suffix not in {".py", ".pyi"} and any(path.is_relative_to(package) for package in packages))
        )
        roots = {Path(member.filename).parts[0] for member in members}
        wheel.extractall(stage, members)
    first_party.unlink()
    for module in stage.rglob("*.py"):
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
            module.write_text(replace(text, edits), encoding=encoding)
    icons = source / "icons"
    previews = stage / icons.name
    previews.mkdir()
    preview_size = "256"
    for master in icons.glob("*.svg"):
        await anyio.run_process(("resvg", "--width", preview_size, "--height", preview_size, master, (previews / master.name).with_suffix(".png")), stdout=None, stderr=None)
    tools = icons / "tools"
    if glyphs := outlined(tools):
        async with httpx2.AsyncClient() as client:
            exporter = await client.get(f"https://raw.githubusercontent.com/blender/blender/{interpreter.build_hash}/release/datafiles/blender_icons_geom.py")
            exporter.raise_for_status()
        exporter_path = archive.parent / "blender_icons_geom.py"
        exporter_path.write_text(exporter.text)
        geometry = Path(__file__).with_name("geometry.py")
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
    process = await anyio.run_process((blender, "--factory-startup", "--command", "extension", "build", "--source-dir", stage, "--output-filepath", archive), env=environment, stdout=None, stderr=None)
    return process.returncode


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["pack"]
