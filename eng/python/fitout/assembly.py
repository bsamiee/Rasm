# mypy: disable-error-code="attr-defined,no-any-unimported"

"""Assemble a local Blender extension from the locked Python runtime."""

import ast
import os
from pathlib import Path
import shutil
import subprocess
import tokenize
import tomllib
from zipfile import BadZipFile, ZipFile

import anyio
from asttokens import ASTText
from asttokens.util import replace
from expression.extra.result import catch
import httpx2
import msgspec
from packaging.requirements import Requirement
from packaging.utils import canonicalize_name, parse_wheel_filename
from pathops import PathOpsError
from result import as_async_result
import tomlkit

from eng.python.fitout.icons import outlined

# --- [MODELS] ---------------------------------------------------------------------------


class _Interpreter(msgspec.Struct, frozen=True):
    executable: str
    build_hash: str
    platform: str


# --- [ERRORS] ---------------------------------------------------------------------------

type PackError = OSError | subprocess.CalledProcessError | BadZipFile | httpx2.HTTPError | SyntaxError | ValueError | PathOpsError


# --- [OPERATIONS] -----------------------------------------------------------------------


@as_async_result(OSError, subprocess.CalledProcessError, BadZipFile, httpx2.HTTPError, SyntaxError, ValueError, PathOpsError)
async def pack(source: Path, archive: Path, blender: str, workspace: Path) -> int:
    """Build an archive carrying private first-party modules and host-native wheels.

    Args:
        source: Extension source directory containing its Blender manifest.
        archive: Native extension archive to replace.
        blender: Blender executable supplying the target interpreter.
        workspace: Root project containing the Python declaration and lock.

    Returns:
        Native build status, or an input, process, archive, or network error.
    """
    stage = archive.parent / "stage"
    wheels = stage / "wheels"
    extensions = archive.parent / "extensions"
    requirements = archive.parent / "requirements.txt"
    environment = {**os.environ, "BLENDER_USER_EXTENSIONS": str(extensions)}
    catch(shutil.rmtree, exception=FileNotFoundError)(stage).default_value(None)
    shutil.copytree(source, stage, ignore=shutil.ignore_patterns("tests", "__pycache__", "*.pyc", "*.pyo"))
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
                "import bpy, json, sys; from bl_pkg.cli.blender_ext import platform_from_this_system; "
                "sys.stderr.write(json.dumps({'executable': sys.executable, 'build_hash': bpy.app.build_hash.decode(), 'platform': platform_from_this_system()}))"
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
            "requirements.txt",
            *(argument for row in declaration["tool"]["uv"]["constraint-dependencies"] for argument in ("--no-emit-package", Requirement(row).name)),
            "--output-file",
            requirements,
        ),
        cwd=workspace,
        stdout=subprocess.DEVNULL,
        stderr=None,
    )
    await anyio.run_process(
        ("pip", "--python", interpreter.executable, "wheel", "--no-deps", "--no-require-hashes", "--requirement", requirements, "--wheel-dir", wheels), cwd=workspace, stdout=None, stderr=None
    )
    (first_party,) = (wheel for wheel in wheels.glob("*.whl") if parse_wheel_filename(wheel.name)[0] == canonicalize_name(declaration["project"]["name"]))
    with ZipFile(first_party) as wheel:
        members = tuple(member for member in wheel.infolist() if not Path(member.filename).parts[0].endswith(".dist-info"))
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
    if glyphs := outlined(source.parent / "icons"):
        async with httpx2.AsyncClient() as client:
            exporter = await client.get(f"https://raw.githubusercontent.com/blender/blender/{interpreter.build_hash}/release/datafiles/blender_icons_geom.py")
            exporter.raise_for_status()
        exporter_path = archive.parent / "blender_icons_geom.py"
        exporter_path.write_text(exporter.text)
        icons = Path(__file__).with_name("geometry.py")
        await anyio.run_process(
            (
                blender,
                "--background",
                "--factory-startup",
                "--python-exit-code",
                "1",
                "--python-expr",
                f"from pathlib import Path; import runpy; runpy.run_path({str(icons)!r})['packed']({glyphs!r}, Path({str(stage / 'icons')!r}), Path({str(exporter_path)!r}))",
            ),
            env=environment,
            stdout=None,
            stderr=None,
        )
    manifest = stage / "blender_manifest.toml"
    document = tomlkit.parse(manifest.read_text())
    document["platforms"] = [interpreter.platform]
    document["wheels"] = tomlkit.item([f"./wheels/{wheel.name}" for wheel in sorted(wheels.glob("*.whl"))]).multiline(multiline=True)
    manifest.write_text(tomlkit.dumps(document))
    process = await anyio.run_process((blender, "--factory-startup", "--command", "extension", "build", "--source-dir", stage, "--output-filepath", archive), env=environment, stdout=None, stderr=None)
    return process.returncode


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PackError", "pack"]
