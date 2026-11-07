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

lazy from blender.records.faults import accumulated, Error, Fault, Ok, Outcome, paired, Reason
lazy from blender.records.stores import files_copied, folder_made
lazy from eng.python.blender.assets import fetched, Pin
lazy from eng.python.blender.host import Blender, declared, exported, Host, Manifest, probed, Pyproject, ran, root, Target, TARGETS

# --- [MODELS] ---------------------------------------------------------------------------


class _Selection(msgspec.Struct, frozen=True):
    """Lock selection across targets: the targets its environments admit, first-party packages to build, and each wheel to fetch."""

    targets: tuple[Target, ...]
    first_party: frozenset[str]
    wheels: frozenset[Pin]


class _Build(msgspec.Struct, frozen=True, kw_only=True):
    """One extension build: its folders, host, workspace, project, manifest kind, and the manifest rows release metadata states."""

    source: Path
    directory: Path
    blender: Path
    host: Host
    base: Path
    project: str
    kind: str
    stamp: frozendict[str, object]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SELECTION]


def _environment(target: Target, python: str, /) -> Environment:
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


def _picked(locked: Pylock, subject: str, python: str, wheels: Path, /) -> Outcome[_Selection]:
    """Selects from a lock per target its environments admit, a package with no fetchable wheel as fault, each wheel staged in the wheels folder."""
    version = Version(python).release[:2]
    admitted = tuple(target for target in TARGETS if any(marker.evaluate({"sys_platform": target.sys_platform, "platform_machine": target.platform_machine}) for marker in locked.environments or ()))
    try:
        picks = tuple(
            chain.from_iterable(
                locked.select(environment=_environment(target, python), tags=(*cpython_tags(version, platforms=target.tags), *compatible_tags(version, f"cp{version[0]}{version[1]}", target.tags)))
                for target in admitted
            )
        )
    except PylockSelectError as error:
        return Error(Fault.of(Reason.INVALID, subject, error))
    fetchable = frozenset(Pin(artifact.url, artifact.hashes["sha256"], wheels / artifact.filename) for _, artifact in picks if isinstance(artifact, PackageWheel) and artifact.url is not None)
    unwheeled = sorted({package.name for package, artifact in picks if not isinstance(artifact, PackageDirectory) and not (isinstance(artifact, PackageWheel) and artifact.url is not None)})
    first_party = frozenset(package.name for package, artifact in picks if isinstance(artifact, PackageDirectory))
    return Error(Fault.of(Reason.ABSENT, subject, member=f"wheel for {', '.join(unwheeled)}")) if unwheeled else Ok(_Selection(admitted, first_party, fetchable))


def _selected(path: Path, host: Host, wheels: Path, /) -> Outcome[_Selection]:
    """Decodes a `pylock.toml` and selects from it at the host's Python."""
    try:
        return _picked(Pylock.from_dict(msgspec.toml.decode(path.read_bytes())), str(path), host.python, wheels)
    except (OSError, msgspec.DecodeError, PylockValidationError) as error:
        return Error(Fault.of(Reason.UNREADABLE, str(path), error))


# --- [STAGE]


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
        exported(build.base, build.host, lock, "--no-emit-project", "--no-editable", "--package", build.project)
        .bind(lambda _: _selected(lock, build.host, stage / "wheels"))
        .bind(
            lambda selection: (
                paired(
                    accumulated(
                        ran("uv", "build", "--project", build.base, "--package", name, "--wheel", "--quiet", "--out-dir", built, stdout=subprocess.DEVNULL) for name in sorted(selection.first_party)
                    ),
                    fetched(build.base, selection.wheels),
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
    (stage / "pyproject.toml").unlink()
    document = tomlkit.parse(manifest.read_text())
    document.update({**build.stamp, **rows})
    manifest.write_text(tomlkit.dumps(document))


def _published(packed: Path, directory: Path, /) -> Outcome[None]:
    """Replaces archives after Blender finishes the build, then removes older archives."""
    try:
        if not (archives := {archive.name: archive for archive in packed.glob("*.zip")}):
            return Error(Fault.of(Reason.ABSENT, str(packed), member="Blender extension archive"))
        for name, archive in sorted(archives.items()):
            archive.replace(directory / name)
        for archive in directory.glob("*.zip"):
            if archive.name not in archives:
                archive.unlink()
    except OSError as error:
        return Error(Fault.of(Reason.UNWRITABLE, str(directory), error))
    return Ok(None)


def _archived(build: _Build, work: Path, packed: Path, /) -> Outcome[object]:
    """Builds the archives of one stage: an add-on's per platform its closure names, every other kind one archive of its files."""
    stage = work / "stage"
    return (
        files_copied(((build.source, stage),))
        .bind(lambda _: files_copied(((build.directory / "assets", stage),)))
        .bind(lambda _: _closure(build, stage, work) if build.kind == "add-on" else Ok({}))
        .bind(lambda rows: _stamped(build, stage, rows).map_error(partial(Fault.of, Reason.UNWRITABLE, str(stage))))
        .bind(
            lambda _: ran(
                build.blender, "--factory-startup", "--command", "extension", "build", "--source-dir", stage, "--output-dir", packed, *(("--split-platforms",) if build.kind == "add-on" else ())
            )
        )
        .bind(lambda _: _published(packed, build.directory / "archives"))
    )


def pack(source: ResolvedExistingDirectory, directory: ResolvedPath, /, *, version: str, maintainer: str, license: str, blender: Blender) -> Outcome[int]:  # ruff: ignore[builtin-argument-shadowing]
    """Builds an extension into the `archives` directory using its sibling `assets`.

    Args:
        source: Extension directory with manifest and dependency rows.
        directory: Parent of the generated assets and archives.
        version: Semantic version written to manifest.
        maintainer: Maintainer written to manifest.
        license: SPDX identifier written to manifest.
        blender: Blender executable.

    Returns:
        Zero, or fault of every failed child process, fetch, and folder write.
    """

    def within(build: _Build, work: Path) -> Outcome[object]:
        try:
            with tempfile.TemporaryDirectory(dir=work) as folder, tempfile.TemporaryDirectory(dir=build.directory) as archives:
                return _archived(build, Path(folder), Path(archives))
        except OSError as error:
            return Error(Fault.of(Reason.UNWRITABLE, str(error.filename or work), error))

    def built(workspace: tuple[Path, Host], project: Pyproject, manifest: Manifest) -> Outcome[object]:
        base, host = workspace
        build = _Build(
            source=source,
            directory=directory,
            blender=blender,
            host=host,
            base=base,
            project=project.project.name,
            kind=manifest.type,
            stamp=frozendict(version=version, maintainer=maintainer, license=[f"SPDX:{license}"]),
        )
        return paired(folder_made(base / ".cache/blender/stage"), folder_made(directory / "archives")).bind(lambda folders: within(build, folders[0]))

    workspace = root().bind(lambda base: probed(base, blender).map(lambda host: (base, host)))
    return paired(workspace, declared(source / "pyproject.toml", Pyproject), declared(source / "blender_manifest.toml", Manifest)).bind(lambda held: built(*held)).map(lambda _: 0)


# --- [HOST]


def upgrade(*, blender: Blender) -> Outcome[int]:
    """Rewrites `[tool.uv] constraint-dependencies` from the host's bundled distributions.

    Args:
        blender: Blender executable.

    Returns:
        Zero, or fault of the probe or the file write.
    """

    def rewritten(base: Path, host: Host) -> Outcome[int]:
        manifest = base / "pyproject.toml"
        try:
            document = tomlkit.parse(manifest.read_text())
            document["tool"]["uv"]["constraint-dependencies"] = tomlkit.item([
                f"{name}=={release}; python_version == '{python.major}.{python.minor}'"
                for python in (Version(host.python),)
                for name, release in sorted(host.bundled.items(), key=lambda row: canonicalize_name(row[0]))
            ]).multiline(multiline=True)
            manifest.write_text(tomlkit.dumps(document))
        except OSError as error:
            return Error(Fault.of(Reason.UNWRITABLE, str(manifest), error))
        return Ok(0)

    return root().bind(lambda base: probed(base, blender).bind(partial(rewritten, base)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["pack", "upgrade"]
