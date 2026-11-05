"""Builds Blender extensions from uv's lock and the host's bundled distributions, syncs their Python requirements, and publishes their static repository."""

lazy import ast
lazy from collections.abc import Iterable
lazy from functools import partial
lazy from hashlib import sha256
lazy from itertools import chain
lazy from pathlib import Path, PurePosixPath
lazy import shutil
lazy import subprocess
lazy import tempfile
lazy from typing import Annotated
lazy from zipfile import ZipFile

import msgspec
lazy from asttokens import ASTText
lazy from asttokens.util import replace
lazy from cyclopts import Parameter
lazy from cyclopts.types import ResolvedExistingDirectory, ResolvedExistingFile, ResolvedPath
lazy from expression import Option
lazy from expression.extra.result import catch
lazy import httpx2
lazy from packaging.markers import Marker
lazy from packaging.pylock import Pylock, PylockValidationError
lazy from packaging.requirements import InvalidRequirement, Requirement
lazy from packaging.utils import canonicalize_name, parse_wheel_filename
lazy import tomlkit

lazy from blender.records.faults import accumulated, Error, Fault, Ok, Outcome, paired, Reason
lazy from blender.records.stores import committed, files_copied, folder_made

# --- [TYPES] ----------------------------------------------------------------------------

type Blender = Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")]

# --- [CONSTANTS] ------------------------------------------------------------------------

BACKGROUND = frozenset({"-b", "--background", "-c", "--command"})

# --- [MODELS] ---------------------------------------------------------------------------


class Host(msgspec.Struct, frozen=True):
    """Facts the installed Blender states: its interpreter, user site layout, bundled distributions, factory tool text color, and wheel platform names."""

    executable: str
    version: str
    python: str
    user_site: str
    bundled: frozendict[str, str]
    text: tuple[float, float, float]
    platforms: frozendict[str, tuple[str, ...]]


class _Target(msgspec.Struct, frozen=True):
    """Extension platform as a wheel platform tag Blender names, a uv Python platform, and the marker values selecting it from `[tool.uv] environments`."""

    wheel: str
    python_platform: str
    sys_platform: str
    platform_machine: str


class _Pinned(msgspec.Struct, frozen=True):
    """File an extension's `[tool.blender.fetch]` table fetches at a pinned URL, checked by sha256."""

    url: str
    sha256: str


class _ProjectTable(msgspec.Struct, frozen=True):
    name: str


class _UvTable(msgspec.Struct, frozen=True):
    environments: tuple[str, ...] = ()


class _BlenderTable(msgspec.Struct, frozen=True):
    fetch: frozendict[str, _Pinned] = frozendict()


class _ToolTable(msgspec.Struct, frozen=True):
    uv: _UvTable = _UvTable()
    blender: _BlenderTable = _BlenderTable()


class _Pyproject(msgspec.Struct, frozen=True):
    """`pyproject.toml` tables `pack` and `site` read."""

    project: _ProjectTable
    tool: _ToolTable = _ToolTable()


# --- [TABLES] ---------------------------------------------------------------------------

_TARGETS = (
    _Target("macosx_11_0_arm64", "aarch64-apple-darwin", "darwin", "arm64"),
    _Target("manylinux_2_28_x86_64", "x86_64-manylinux_2_28", "linux", "x86_64"),
    _Target("manylinux_2_28_aarch64", "aarch64-manylinux_2_28", "linux", "aarch64"),
    _Target("win_amd64", "x86_64-pc-windows-msvc", "win32", "AMD64"),
)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PROCESSES]


def ran(*command: str | Path, stdout: int | None = None, stderr: int | None = None) -> Outcome[subprocess.CompletedProcess[bytes]]:
    """Runs a child process, a failed launch or nonzero exit as fault.

    Args:
        command: Executable and its arguments.
        stdout: Output stream target, inherited when None.
        stderr: Error stream target, inherited when None.

    Returns:
        Finished process, or fault of a failed launch or of the nonzero exit's `CalledProcessError`.
    """
    try:
        return Ok(subprocess.run(command, stdout=stdout, stderr=stderr, check=True))
    except (OSError, subprocess.CalledProcessError) as error:
        return Error(Fault.of(Reason.FAILED, str(command[0]), error))


def probed(blender: Path, /) -> Outcome[Host]:
    """Reads host facts from a factory-startup Blender.

    Args:
        blender: Blender executable.

    Returns:
        Host facts, or fault of the run or its answer.
    """
    probe = """\
import importlib.metadata, json, os, sys, sysconfig
import OpenImageIO, PyOpenColorIO, bpy
from bl_pkg.cli.blender_ext import blender_platforms_from_wheel_platform
sys.stderr.write(json.dumps({
    "executable": sys.executable,
    "version": bpy.app.version_string,
    "python": "{}.{}".format(*sys.version_info),
    "user_site": os.path.relpath(sysconfig.get_path("purelib", scheme=sysconfig.get_preferred_scheme("user"), vars={"userbase": sys.prefix}), sys.prefix),
    "bundled": {d.name: d.version for d in importlib.metadata.distributions(path=[sysconfig.get_path("purelib")]) if d.name} | {"openimageio": OpenImageIO.VERSION_STRING, "opencolorio": PyOpenColorIO.__version__},
    "text": tuple(bpy.context.preferences.themes[0].user_interface.wcol_tool.text),
    "platforms": {tag: blender_platforms_from_wheel_platform(tag) for tag in sys.argv[sys.argv.index("--") + 1:]},
}))
"""
    return ran(
        blender, "-b", "--factory-startup", "--python-exit-code", "1", "--python-expr", probe, "--", *(target.wheel for target in _TARGETS), stdout=subprocess.PIPE, stderr=subprocess.PIPE
    ).bind(lambda host: catch(lambda: msgspec.json.decode(host.stderr, type=Host), exception=msgspec.DecodeError)().map_error(partial(Fault.of, Reason.INVALID, str(blender))))


def _workspace(*arguments: str) -> Outcome[tuple[Path, ...]]:
    """Returns the paths one `uv workspace` subcommand prints."""
    return ran("uv", "workspace", *arguments, stdout=subprocess.PIPE).map(lambda found: tuple(map(Path, found.stdout.decode().splitlines())))


def _exported(host: Host, lock: Path, /, *options: str) -> Outcome[subprocess.CompletedProcess[bytes]]:
    """Writes locked requirements without Blender's bundled distributions."""
    return ran(
        "uv",
        "export",
        "--locked",
        "--no-default-groups",
        "--no-header",
        *chain.from_iterable(("--no-emit-package", name) for name in host.bundled),
        *options,
        "--output-file",
        lock,
        stdout=subprocess.DEVNULL,
    )


def _decoded[T](path: Path, form: type[T], /) -> Outcome[T]:
    """Decodes a TOML file through one record form."""
    try:
        return Ok(msgspec.toml.decode(path.read_bytes(), type=form))
    except (OSError, msgspec.DecodeError) as error:
        return Error(Fault.of(Reason.UNREADABLE, str(path), error))


# --- [FETCHES]


def _fetched(root: Path, pin: _Pinned, target: Path, /) -> Outcome[Path]:
    """Commits the pinned file to target through a content cache under the workspace root, a target holding those bytes kept, a download of other bytes as fault."""
    cached = root / ".cache/blender/files" / pin.sha256

    def downloaded() -> Outcome[Path]:
        try:
            data = httpx2.get(pin.url, follow_redirects=True, timeout=None).raise_for_status().content
        except httpx2.HTTPError as error:
            return Error(Fault.of(Reason.FAILED, pin.url, error))
        digest = sha256(data).hexdigest()
        return committed(cached, data) if digest == pin.sha256 else Error(Fault.of(Reason.OUTSIDE, pin.url, digest, member=f"sha256 {pin.sha256}"))

    def placed(found: Path) -> Outcome[Path]:
        return catch(found.read_bytes, exception=OSError)().map_error(partial(Fault.of, Reason.UNREADABLE, str(found))).bind(partial(committed, target))

    held = catch(target.read_bytes, exception=OSError)().map(lambda data: sha256(data).hexdigest()).default_value("")
    return Ok(target) if held == pin.sha256 else (Ok(cached) if cached.exists() else downloaded()).bind(placed)


def _inputs(root: Path, source: Path, /) -> Outcome[tuple[Path, ...]]:
    """Fetches the files a project's `[tool.blender.fetch]` table pins into its folder, where its modules and archive read them, every fault kept."""
    return _decoded(source / "pyproject.toml", _Pyproject).bind(lambda document: accumulated(_fetched(root, pin, source / relative) for relative, pin in document.tool.blender.fetch.items()))


# --- [PACKAGING]


def _locked(path: Path, /) -> Outcome[Pylock]:
    """Decodes a `pylock.toml` file through its standard model."""
    return _decoded(path, dict[str, object]).bind(lambda document: catch(Pylock.from_dict, exception=PylockValidationError)(document).map_error(partial(Fault.of, Reason.INVALID, str(path))))


def _selected(host: Host, requirements: Path, work: Path, target: _Target, /) -> Outcome[tuple[str, ...]]:
    """Returns the wheel file name uv selects per package for one target at the host's Python version, a platform wheel over a pure one."""
    chosen = work / f"pylock.{target.wheel}.toml"
    return (
        ran(
            "uv",
            "pip",
            "compile",
            requirements,
            "--no-deps",
            "--no-header",
            "--quiet",
            "--only-binary",
            ":all:",
            "--python-version",
            host.python,
            "--python-platform",
            target.python_platform,
            "--format",
            "pylock.toml",
            "--output-file",
            chosen,
        )
        .bind(lambda _: _locked(chosen))
        .map(
            lambda found: tuple(
                min(package.wheels, key=lambda wheel: any(tag.platform == "any" for tag in parse_wheel_filename(wheel.filename)[3])).filename
                for package in found.packages
                if package.wheels is not None
            )
        )
    )


def _wheel(root: Path, locked: Pylock, folder: Path, name: str, /) -> Outcome[Path]:
    """Fetches one wheel at the URL and sha256 the lock states for its file name, a wheel the lock lists at no URL absent."""
    return (
        Option
        .of_optional(
            next((_Pinned(url=wheel.url, sha256=wheel.hashes["sha256"]) for package in locked.packages for wheel in package.wheels or () if wheel.filename == name and wheel.url is not None), None)
        )
        .to_result_with(partial(Fault.of, Reason.ABSENT, "uv.lock", member=name))
        .bind(lambda pin: _fetched(root, pin, folder / name))
    )


def _vendored(stage: Path, wheels: Iterable[Path], /) -> None:
    """Extracts first-party wheels into the stage and rewrites absolute imports of their roots to package-relative ones."""
    roots: set[str] = set()
    for member in wheels:
        with ZipFile(member) as wheel:
            members = [name for name in wheel.namelist() if not PurePosixPath(name).parts[0].endswith(".dist-info")]
            wheel.extractall(stage, members)
        roots.update(PurePosixPath(name).parts[0] for name in members)
    for module in stage.rglob("*.py"):
        syntax = ASTText(text := module.read_text(newline=""), filename=str(module))
        if imports := [
            (*syntax.get_text_range(node, padded=False), ast.unparse(ast.ImportFrom(module=node.module, names=node.names, level=len(module.relative_to(stage).parent.parts) + 1)))
            for node in ast.walk(syntax.tree)
            if isinstance(node, ast.ImportFrom) and node.level == 0 and node.module is not None and node.module.split(".", 1)[0] in roots
        ]:
            module.write_text(replace(text, imports), newline="")


def pack(source: ResolvedExistingDirectory, directory: ResolvedPath, /, *, version: str, maintainer: str, blender: Blender) -> Outcome[int]:
    """Builds one extension archive per `[tool.uv] environments` platform into `directory`, beside the folder `icons` wrote there.

    First-party packages become submodules and the locked third-party closure becomes wheels, without Blender's bundled distributions.

    Args:
        source: Extension directory with manifest, dependency rows, and assets.
        directory: Folder receiving the archives, its earlier archives removed.
        version: Semantic version written to manifest.
        maintainer: Maintainer written to manifest.
        blender: Blender executable.

    Returns:
        Zero, or fault of every failed child process, fetch, and folder write.
    """

    def manifested(stage: Path, built: Path, host: Host, targets: tuple[_Target, ...]) -> Outcome[None]:
        try:
            _vendored(stage, built.glob("*.whl"))
            document = tomlkit.parse((manifest := stage / "blender_manifest.toml").read_text())
            document.update(
                version=version,
                maintainer=maintainer,
                platforms=sorted(frozenset(chain.from_iterable(host.platforms[target.wheel] for target in targets))),
                wheels=tomlkit.item([f"./wheels/{wheel.name}" for wheel in sorted((stage / "wheels").glob("*.whl"))]).multiline(multiline=True),
            )
            manifest.write_text(tomlkit.dumps(document))
        except OSError as error:
            return Error(Fault.of(Reason.UNWRITABLE, str(stage), error))
        return Ok(None)

    def placed(archives: Path) -> Outcome[None]:
        try:
            directory.mkdir(parents=True, exist_ok=True)
            for archive in directory.glob("*.zip"):
                archive.unlink()
            for archive in archives.glob("*.zip"):
                shutil.move(archive, directory / archive.name)
        except OSError as error:
            return Error(Fault.of(Reason.UNWRITABLE, str(directory), error))
        return Ok(None)

    try:
        with tempfile.TemporaryDirectory() as folder:
            work = Path(folder)
            stage, built, archives, lock, requirements = work / "stage", work / "built", work / "archives", work / "pylock.toml", work / "requirements.txt"
            archives.mkdir()

            def assembled(host: Host, root: Path, project: _Pyproject, workspace: _Pyproject) -> Outcome[None]:
                markers = tuple(map(Marker, workspace.tool.uv.environments))
                targets = tuple(target for target in _TARGETS if any(marker.evaluate({"sys_platform": target.sys_platform, "platform_machine": target.platform_machine}) for marker in markers))
                selector = ("--package", project.project.name)
                return (
                    paired(
                        _exported(host, lock, "--format", "pylock.toml", "--no-emit-project", "--no-editable", *selector),
                        _exported(host, requirements, "--no-hashes", "--no-annotate", "--no-emit-workspace", "--no-emit-project", *selector),
                        _inputs(root, source).bind(lambda _: files_copied(((source, stage),))).bind(lambda _: files_copied(((directory / "icons", stage / "icons"),))),
                    )
                    .bind(lambda _: _locked(lock))
                    .bind(
                        lambda locked: (
                            accumulated(
                                ran("uv", "build", "--project", root, "--package", package.name, "--wheel", "--quiet", "--out-dir", built, stdout=subprocess.DEVNULL)
                                for package in locked.packages
                                if package.directory is not None
                            )
                            .bind(lambda _: accumulated(_selected(host, requirements, work, target) for target in targets))
                            .bind(lambda names: accumulated(_wheel(root, locked, stage / "wheels", name) for name in sorted({*group for group in names})))
                        )
                    )
                    .bind(lambda _: manifested(stage, built, host, targets))
                )

            return (
                paired(probed(blender), _workspace("dir"), _decoded(source / "pyproject.toml", _Pyproject))
                .bind(lambda held: _decoded(held[1][0] / "pyproject.toml", _Pyproject).bind(lambda workspace: assembled(held[0], held[1][0], held[2], workspace)))
                .bind(lambda _: ran(blender, "--factory-startup", "--command", "extension", "build", "--source-dir", stage, "--output-dir", archives, "--split-platforms"))
                .bind(lambda _: placed(archives))
                .map(lambda _: 0)
            )
    except OSError as error:
        return Error(Fault.of(Reason.FAILED, str(source), error))


def indexed(directory: Path, blender: Path, /) -> Outcome[Path]:
    """Writes `index.json` and `index.html` over the archives in `directory`.

    Args:
        directory: Static repository folder.
        blender: Blender executable.

    Returns:
        JSON listing path, or fault of the generator run.
    """
    return ran(blender, "--factory-startup", "--command", "extension", "server-generate", "--repo-dir", directory, "--html").map(lambda _: directory / "index.json")


def publish(directory: ResolvedExistingDirectory, /, *, tag: str, blender: Blender) -> Outcome[int]:
    """Uploads the archives to a release, then the archives and listing to the `<directory name>-updates` channel release.

    Args:
        directory: Static repository folder `pack` built, named by its extension.
        tag: Release tag receiving the archives.
        blender: Blender executable.

    Returns:
        Zero, or fault of the listing or an upload.
    """
    archives = sorted(directory.glob("*.zip"))
    return (
        indexed(directory, blender)
        .bind(lambda listing: ran("gh", "release", "upload", tag, *archives, "--clobber").map(lambda _: listing))
        .bind(lambda listing: ran("gh", "release", "upload", f"{directory.name}-updates", *archives, listing, listing.with_suffix(".html"), "--clobber"))
        .map(lambda _: 0)
    )


# --- [ENVIRONMENTS]


def site(target: ResolvedPath | None = None, /, *, blender: Blender, package: str | None = None, groups: tuple[str, ...] = ()) -> Outcome[Path]:
    """Reconciles locked requirements into the user base of one selection on the installed Blender version and fetches each workspace member's pinned files, editable projects kept.

    Selections keep separate user bases, since a sync removes what its lock omits from a base a concurrent run reads.

    Args:
        target: Python user base, the workspace's `.cache/blender/python/<version>/<package>+<group>...` when omitted.
        blender: Blender executable.
        package: Workspace member selecting locked runtime requirements, workspace root when omitted.
        groups: Additional uv dependency group selectors.

    Returns:
        Python user base, or fault of an export, host query, fetch, or package sync.
    """
    options = (*(("--package", package) if package is not None else ()), *chain.from_iterable(("--group", group) for group in groups))

    def synced(root: Path, host: Host, members: tuple[Path, ...]) -> Outcome[Path]:
        base = target if target is not None else root / ".cache/blender/python" / host.version / "+".join((package or root.name, *groups))
        lock = base / "pylock.toml"
        return (
            folder_made(base)
            .bind(lambda _: accumulated(_inputs(root, member) for member in members))
            .bind(lambda _: _exported(host, lock, "--format", "pylock.toml", *options))
            .bind(lambda _: ran("uv", "pip", "sync", "--python", host.executable, "--target", base / host.user_site, lock))
            .map(lambda _: base)
        )

    return paired(_workspace("dir"), probed(blender), _workspace("list", "--paths")).bind(lambda held: synced(held[0][0], held[1], held[2]))


def upgrade(*, blender: Blender) -> Outcome[int]:
    """Rewrites `[tool.uv] constraint-dependencies` from the host's bundled distributions and relocks.

    Args:
        blender: Blender executable.

    Returns:
        Zero, or fault of the probe, a bundled row no requirement parses, the file write, or `uv lock`.
    """

    def rewritten(root: Path, host: Host) -> Outcome[None]:
        manifest = root / "pyproject.toml"
        try:
            document = tomlkit.parse(manifest.read_text())
            rows = (f"{name}=={release}; python_full_version < '3.15'" for name, release in host.bundled.items())
            document["tool"]["uv"]["constraint-dependencies"] = sorted(rows, key=lambda row: canonicalize_name(Requirement(row).name))
            manifest.write_text(tomlkit.dumps(document))
        except (OSError, InvalidRequirement) as error:
            return Error(Fault.of(Reason.INVALID, str(manifest), error))
        return Ok(None)

    return paired(_workspace("dir"), probed(blender)).bind(lambda held: rewritten(held[0][0], held[1])).bind(lambda _: ran("uv", "lock")).map(lambda _: 0)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BACKGROUND", "Blender", "Host", "indexed", "pack", "probed", "publish", "ran", "site", "upgrade"]
