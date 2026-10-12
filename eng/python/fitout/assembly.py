"""Assemble Blender extensions and import folders from the locked Python runtime."""

import ast
from pathlib import Path
import platform
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
from packaging.specifiers import SpecifierSet
from packaging.utils import canonicalize_name, parse_wheel_filename
from pathops import PathOpsError  # type: ignore[attr-defined]
from result import as_async_result
import tomlkit

from eng.python.fitout.icons import encoded

# --- [MODELS] ---------------------------------------------------------------------------


class _Project(msgspec.Struct, frozen=True, rename="kebab"):
    name: str
    requires_python: str


class _Uv(msgspec.Struct, frozen=True, rename="kebab"):
    constraint_dependencies: tuple[str, ...]


class _Tool(msgspec.Struct, frozen=True):
    uv: _Uv


class _Declaration(msgspec.Struct, frozen=True):
    project: _Project


class _Root(_Declaration, frozen=True):
    tool: _Tool


# --- [OPERATIONS] -----------------------------------------------------------------------


async def _locked(root: _Root, declared: _Project, lock: Path, *options: str) -> str:
    """Python version a project declares, the locked runtime closure exported to `lock` by uv `options` without the providers root uv constraint markers hold bundled at that version."""
    (version,) = (specifier.version for specifier in SpecifierSet(declared.requires_python) if specifier.operator == ">=")
    bundled = (
        requirement.name
        for requirement in map(Requirement, root.tool.uv.constraint_dependencies)
        if requirement.marker and requirement.marker.evaluate({"python_version": version, "python_full_version": version})
    )
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
            *options,
            "--output-file",
            lock,
        ),
        stdout=subprocess.DEVNULL,
        stderr=None,
    )
    return version


@as_async_result(OSError, subprocess.CalledProcessError, BadZipFile, msgspec.DecodeError, SyntaxError, ValueError, PathOpsError)
async def pack(source: ResolvedPath, archive: ResolvedPath, /, *modules: Path, blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")]) -> int:
    """Build a Blender extension for the Python version it declares and this platform at the requested archive path.

    Args:
        source: Extension directory containing its manifest and Python declaration.
        archive: ZIP file to create or replace.
        modules: First-party wheel modules in the extension's runtime import closure.
        blender: Blender executable, supplied by BLENDER_PATH.

    Returns:
        Zero once built, or an input, process, or archive error.
    """
    root = msgspec.toml.decode(await anyio.Path("pyproject.toml").read_bytes(), type=_Root)
    extension = msgspec.toml.decode(await anyio.Path(source / "pyproject.toml").read_bytes(), type=_Declaration)
    icons = source / "icons"
    tools = icons / "tools"
    async with anyio.TemporaryDirectory() as folder:
        scratch = Path(folder)
        stage = scratch / "stage"
        wheels = stage / "wheels"
        previews = stage / icons.name
        shutil.copytree(source, stage, ignore=lambda parent, names: [name for name in names if Path(parent, name) in {icons, source / "pyproject.toml"}])
        lock = scratch / "requirements.txt"
        pins = scratch / "pins.txt"
        version = await _locked(root, extension.project, lock, "--no-emit-project")
        await anyio.run_process(("uv", "pip", "compile", lock, "--python-version", version, "--no-deps", "--no-header", "--no-annotate", "--output-file", pins), stdout=None, stderr=None)
        await anyio.run_process(("pip", "download", "--no-deps", "--python-version", version, "--requirement", pins, "--dest", wheels), stdout=None, stderr=None)
        sources = tuple(path for path in wheels.iterdir() if path.suffix != ".whl")
        await anyio.run_process(("pip", "wheel", "--no-deps", "--wheel-dir", wheels, Path(), *sources), stdout=None, stderr=None)
        for path in sources:
            path.unlink()
        (first_party,) = (wheel for wheel in wheels.glob("*.whl") if parse_wheel_filename(wheel.name)[0] == canonicalize_name(root.project.name))
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
        size = "256"
        for master in icons.glob("*.svg"):
            previews.mkdir(exist_ok=True)
            await anyio.run_process(("resvg", "--width", size, "--height", size, master, (previews / master.name).with_suffix(".png")), stdout=None, stderr=None)
        for master in tools.glob("*.svg"):
            glyph = (previews / tools.name / master.name).with_suffix(".dat")
            glyph.parent.mkdir(parents=True, exist_ok=True)
            glyph.write_bytes(encoded(master))
        manifest = stage / "blender_manifest.toml"
        document = tomlkit.parse(manifest.read_text())
        manifest_names = {"darwin": "macos", "x86_64": "x64", "amd64": "x64", "aarch64": "arm64"}
        document["platforms"] = ["-".join(manifest_names.get(part, part) for part in (platform.system().lower(), platform.machine().lower()))]
        manifest_wheels = tomlkit.array().multiline(multiline=True)
        manifest_wheels.extend(f"./wheels/{wheel.name}" for wheel in sorted(wheels.glob("*.whl")))
        document["wheels"] = manifest_wheels
        manifest.write_text(tomlkit.dumps(document))
        await anyio.Path(archive.parent).mkdir(parents=True, exist_ok=True)
        await anyio.run_process((blender, "--factory-startup", "--command", "extension", "build", "--source-dir", stage, "--output-filepath", archive), stdout=None, stderr=None)
    return 0


@as_async_result(OSError, subprocess.CalledProcessError, msgspec.DecodeError, ValueError)
async def site(target: ResolvedPath, /) -> int:
    """Install the runtime closure `pack` bundles, with the first-party package whole, into a folder Blender's interpreter imports from.

    Args:
        target: Folder receiving the installed packages for the Python version the root project declares.

    Returns:
        Zero once installed, or a process, decoding, or requirement error.
    """
    root = msgspec.toml.decode(await anyio.Path("pyproject.toml").read_bytes(), type=_Root)
    async with anyio.TemporaryDirectory() as scratch:
        lock = Path(scratch) / "requirements.txt"
        version = await _locked(root, root.project, lock)
        await anyio.run_process(("uv", "pip", "install", "--python-version", version, "--target", target, "--requirement", lock), stdout=None, stderr=None)
    return 0


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["pack", "site"]
