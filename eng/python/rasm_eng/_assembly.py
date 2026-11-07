"""Builds Blender extension archives from a stage overlaid with their assets and, for an add-on, uv's lock without the host's bundled distributions, and syncs host constraints."""

lazy import ast
lazy from collections.abc import Iterable
lazy from functools import partial
lazy from itertools import chain
lazy from pathlib import Path, PurePosixPath
lazy import subprocess
lazy import tempfile
lazy from zipfile import ZipFile

from expression.extra.result import catch
import msgspec
lazy from asttokens import ASTText
lazy from asttokens.util import replace
lazy from cyclopts.types import ResolvedExistingDirectory, ResolvedPath
lazy from packaging.markers import default_environment, Environment
lazy from packaging.pylock import PackageDirectory, PackageWheel, Pylock, PylockSelectError, PylockValidationError
lazy from packaging.tags import compatible_tags, cpython_tags
lazy from packaging.utils import canonicalize_name
lazy from packaging.version import Version
lazy import tomlkit

lazy from rasm_eng._assets import _fetched, _Pin
lazy from rasm_eng._host import _declared, _exported, _Host, _Manifest, _probed, _ran, _root, _Target, Blender, Deployment
lazy from rasm_records import accumulated, committed, Error, Fault, files_copied, folder_made, Ok, Outcome, paired, Reason

# --- [MODELS] ---------------------------------------------------------------------------


class _Selection(msgspec.Struct, frozen=True):
    """Lock selection across targets: the targets its environments admit, folders of first-party packages to build, and each wheel to fetch."""

    targets: tuple[_Target, ...]
    first_party: frozenset[Path]
    wheels: frozenset[_Pin]


class _Identity(msgspec.Struct, frozen=True):
    author: str
    license: str


class _Build(msgspec.Struct, frozen=True, kw_only=True):
    """One extension build: its folders, host, extension platforms, workspace, manifest kind, and the manifest rows release metadata states."""

    source: Path
    directory: Path
    blender: Path
    host: _Host
    targets: tuple[_Target, ...]
    base: Path
    kind: str
    stamp: frozendict[str, object]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SELECTION]


def _environment(target: _Target, python: str, /) -> Environment:
    """Returns the marker environment of a target at the host's Python release."""
    major, minor, *_ = Version(python).release
    return default_environment() | {
        "python_version": f"{major}.{minor}",
        "python_full_version": python,
        "sys_platform": target.sys_platform,
        "platform_machine": target.platform_machine,
        "os_name": target.os_name,
        "platform_system": target.platform_system,
    }


def _picked(locked: Pylock, path: Path, python: str, targets: tuple[_Target, ...], wheels: Path, /) -> Outcome[_Selection]:
    """Selects from a lock per target its environments admit, a package with no fetchable wheel as fault, each wheel staged in the wheels folder, each first-party folder read relative to the lock."""
    version = Version(python).release[:2]
    admitted = tuple(target for target in targets if any(marker.evaluate({"sys_platform": target.sys_platform, "platform_machine": target.platform_machine}) for marker in locked.environments or ()))
    try:
        picks = tuple(
            chain.from_iterable(
                locked.select(environment=_environment(target, python), tags=(*cpython_tags(version, platforms=target.tags), *compatible_tags(version, f"cp{version[0]}{version[1]}", target.tags)))
                for target in admitted
            )
        )
    except PylockSelectError as error:
        return Error(Fault.of(Reason.INVALID, str(path), error))
    fetchable = frozenset(_Pin(artifact.url, artifact.hashes["sha256"], wheels / artifact.filename) for _, artifact in picks if isinstance(artifact, PackageWheel) and artifact.url is not None)
    unwheeled = sorted({package.name for package, artifact in picks if not isinstance(artifact, PackageDirectory) and not (isinstance(artifact, PackageWheel) and artifact.url is not None)})
    first_party = frozenset(path.parent / artifact.path for _, artifact in picks if isinstance(artifact, PackageDirectory))
    return Error(Fault.of(Reason.ABSENT, str(path), member=f"wheel for {', '.join(unwheeled)}")) if unwheeled else Ok(_Selection(admitted, first_party, fetchable))


def _selected(path: Path, host: _Host, targets: tuple[_Target, ...], wheels: Path, /) -> Outcome[_Selection]:
    """Decodes a `pylock.toml` and selects from it at the host's Python."""
    try:
        return _picked(Pylock.from_dict(msgspec.toml.decode(path.read_bytes())), path, host.python, targets, wheels)
    except (OSError, msgspec.DecodeError, PylockValidationError) as error:
        return Error(Fault.of(Reason.UNREADABLE, str(path), error))


# --- [STAGES]


@catch(exception=OSError)
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


def _closure(build: _Build, stage: Path, work: Path, /) -> Outcome[dict[str, object]]:
    """Stages an add-on's locked closure, first-party packages as submodules and the rest as wheels, and returns the manifest rows naming it."""
    built, lock = work / "built", work / "pylock.toml"
    return (
        _exported(build.source, build.host, lock, "--no-emit-project", "--no-editable")
        .bind(lambda _: _selected(lock, build.host, build.targets, stage / "wheels"))
        .bind(
            lambda selection: (
                paired(
                    accumulated(_ran("uv", "build", "--wheel", "--quiet", "--out-dir", built, folder, stdout=subprocess.DEVNULL) for folder in sorted(selection.first_party)),
                    _fetched(build.base, selection.wheels),
                )
                .bind(lambda _: _vendored(stage, built.glob("*.whl")).map_error(partial(Fault.of, Reason.UNWRITABLE, str(stage))))
                .map(
                    lambda _: {
                        "platforms": sorted(frozenset(chain.from_iterable(build.host.platforms[target.tags[0]] for target in selection.targets))),
                        "wheels": tomlkit.item([f"./wheels/{name}" for name in sorted(pin.target.name for pin in selection.wheels)]).multiline(multiline=True),
                    }
                )
            )
        )
    )


@catch(exception=OSError)
def _stamped(build: _Build, stage: Path, rows: dict[str, object], /) -> None:
    """Writes release metadata and the locked closure into the staged manifest."""
    manifest = stage / "blender_manifest.toml"
    document = tomlkit.parse(manifest.read_text())
    document.update({**build.stamp, **rows})
    manifest.write_text(tomlkit.dumps(document))


def _archived(build: _Build, work: Path, /) -> Outcome[Path]:
    """Builds the archives of one stage, the extension files Git tracks or leaves unignored, into the `archives` folder in one step: an add-on's per platform its closure names, every other kind one archive of its files."""
    stage = work / "stage"
    return (
        _ran("git", "-C", build.source, "ls-files", "-z", "--cached", "--others", "--exclude-standard", stdout=subprocess.PIPE)
        .bind(lambda listed: files_copied((build.source / name, stage / name) for name in listed.stdout.decode().split("\0") if name))
        .bind(lambda _: files_copied(((build.directory / "assets", stage),)))
        .bind(lambda _: _closure(build, stage, work) if build.kind == "add-on" else Ok({}))
        .bind(lambda rows: _stamped(build, stage, rows).map_error(partial(Fault.of, Reason.UNWRITABLE, str(stage))))
        .bind(
            lambda _: committed(
                build.directory / "archives",
                lambda archives: folder_made(archives).bind(
                    lambda _: _ran(
                        build.blender,
                        "--factory-startup",
                        "--command",
                        "extension",
                        "build",
                        "--source-dir",
                        stage,
                        "--output-dir",
                        archives,
                        *(("--split-platforms",) if build.kind == "add-on" else ()),
                    )
                ),
            )
        )
    )


def pack(source: ResolvedExistingDirectory, directory: ResolvedPath, /, *, version: str, blender: Blender, macos: Deployment) -> Outcome[tuple[Path, ...]]:
    """Builds an extension into the `archives` directory using its sibling `assets`.

    Args:
        source: Extension directory with manifest and dependency rows, the uv project whose lock names its closure.
        directory: Parent of the generated assets and archives.
        version: Semantic version written to manifest, the root `package.json` author and license beside it.
        blender: Blender executable.
        macos: macOS release the darwin wheels target.

    Returns:
        Archives written, or fault of every failed child process, fetch, and folder write.
    """

    def within(build: _Build, work: Path) -> Outcome[Path]:
        try:
            with tempfile.TemporaryDirectory(dir=work) as folder:
                return _archived(build, Path(folder))
        except OSError as error:
            return Error(Fault.of(Reason.UNWRITABLE, str(error.filename or work), error))

    def identified(base: Path) -> Outcome[_Identity]:
        try:
            return Ok(msgspec.json.decode((base / "package.json").read_bytes(), type=_Identity))
        except (OSError, msgspec.DecodeError) as error:
            return Error(Fault.of(Reason.UNREADABLE, str(base / "package.json"), error))

    def built(workspace: tuple[Path, _Host, _Identity], manifest: _Manifest) -> Outcome[Path]:
        base, host, identity = workspace
        build = _Build(
            source=source,
            directory=directory,
            blender=blender,
            host=host,
            targets=targets,
            base=base,
            kind=manifest.type,
            stamp=frozendict(version=version, maintainer=identity.author, license=[f"SPDX:{identity.license}"]),
        )
        return folder_made(base / ".cache/blender/stage").bind(partial(within, build))

    targets = _Target.every(macos)
    workspace = _root().bind(lambda base: paired(_probed(base, blender, *(target.tags[0] for target in targets)), identified(base)).map(lambda held: (base, *held)))
    return paired(workspace, _declared(source / "blender_manifest.toml", _Manifest)).bind(lambda held: built(*held)).map(lambda archives: tuple(sorted(archives.glob("*.zip"))))


# --- [HOSTS]


def upgrade(project: ResolvedExistingDirectory, /, *, blender: Blender) -> Outcome[Path]:
    """Rewrites a 3.13 uv root's `[tool.uv] constraint-dependencies` from the host's bundled distributions, the rows uv reads from the locking root alone.

    Args:
        project: uv root locking for Blender's interpreter.
        blender: Blender executable.

    Returns:
        Rewritten manifest, or fault of the probe or the file write.
    """

    def rewritten(host: _Host) -> Outcome[Path]:
        manifest = project / "pyproject.toml"
        try:
            document = tomlkit.parse(manifest.read_text())
            document["tool"]["uv"]["constraint-dependencies"] = tomlkit.item([
                f"{name}=={release}" for name, release in sorted(host.bundled.items(), key=lambda row: canonicalize_name(row[0]))
            ]).multiline(multiline=True)
            manifest.write_text(tomlkit.dumps(document))
        except OSError as error:
            return Error(Fault.of(Reason.UNWRITABLE, str(manifest), error))
        return Ok(manifest)

    return _root().bind(lambda base: _probed(base, blender).bind(rewritten))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["pack", "upgrade"]
