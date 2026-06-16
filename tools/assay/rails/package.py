"""Manage yak package publish, plan, and list rails.

Publish commits package directories through a same-filesystem swap with a recovery sentinel, then
extends the staged commit with policy-driven install/push and bridge refresh steps under slug leases.
Plan is publish's dry-run metadata evaluation; list rosters package projects and slugs.
"""

from dataclasses import dataclass
from enum import StrEnum
import fnmatch
from functools import reduce
from itertools import chain
import os
from pathlib import Path
from shutil import copy2, rmtree
from tempfile import mkdtemp
from typing import Final, override, Self, TYPE_CHECKING

from expression import Error, Ok, Result
from expression.collections import block
from expression.extra.result import sequence
import msgspec
import structlog

from tools.assay.composition.catalog import select
from tools.assay.composition.settings import (  # noqa: TC001  # beartype resolves these at import time, not under TYPE_CHECKING
    ArtifactScope,
    AssaySettings,
)
from tools.assay.core.engine import leased, proc_dead, run_check
from tools.assay.core.model import (
    ArtifactKind,
    Base,
    BaseParams,
    Check,
    Claim,
    Completed,
    Counts,
    Fault,
    fold,
    Input,
    Language,
    Match,
    Mode,
    PackageRun,
    Report,  # noqa: TC001  # unconditional: beartype @checked resolves the -> Result[Report, Fault] forward-ref under PEP 649
    Runner,
    Tool,
)
from tools.assay.core.routing import parse_csproj, Routed, Scope
from tools.assay.core.status import join, RailStatus
from tools.assay.rails.bridge import bridge_lease, client_run


if TYPE_CHECKING:
    from collections.abc import Iterable


# --- [TYPES] ----------------------------------------------------------------------------

type _Step = tuple[str, ...]


class _LifecycleStep(StrEnum):
    """Post-stage yak or bridge step with its run mode, lease requirement, and supervisor argv verb."""

    mode: Mode
    needs_bridge: bool
    wire: str
    INSTALL = "install", Mode.DEPLOY, False, ""
    PUSH = "push", Mode.PUBLISH, False, ""
    QUIT = "quit", Mode.CLIENT, True, "quit"
    REFRESH = "refresh", Mode.CLIENT, True, "status"

    def __new__(cls, value: str, mode: Mode, needs_bridge: bool, wire: str) -> Self:  # noqa: FBT001  # enum payload binder mirrors enum field order
        member = str.__new__(cls, value)
        member._value_, member.mode, member.needs_bridge, member.wire = value, mode, needs_bridge, wire
        return member


# --- [CONSTANTS] ------------------------------------------------------------------------

_RHP: Final[str] = ".rhp"
_COMMIT_PENDING: Final[str] = ".commit-pending.json"
_YAK_PLATFORM: Final[str] = "mac"
_YAK_DISTRIBUTION_GLOB: Final[str] = "*-rh9_*-mac.yak"
_RASM_BRIDGE_SLUG: Final[str] = "rasm-bridge"
_RASM_BRIDGE_SHELL_PROJECT: Final[str] = "tools/rhino-bridge/Shell/Shell.csproj"
_PACKAGE_STAGE: Final[str] = "package-stage"
_PACKAGE_ROOTS: Final[tuple[str, ...]] = ("apps", "tools")
_CSPROJ: Final[str] = ".csproj"
_YAK_SLUG_PROP: Final[str] = "YakPackageSlug"

_META_PROPS: Final[tuple[str, ...]] = (
    "AssemblyName",
    "MSBuildProjectDirectory",
    "TargetDir",
    "TargetExt",
    "TargetFramework",
    "YakManifestDirectory",
    "YakPackageDirectory",
    "YakPackagePattern",
    "YakPackageSlug",
    "YakPath",
    "YakPlatform",
    "YakPushSource",
)
_MANIFEST_FILES: Final[tuple[str, ...]] = ("icon.png", "manifest.yml")
_ARTIFACT_SUFFIXES: Final[frozenset[str]] = frozenset({".dll", ".json", _RHP})
_HOST_EXCLUDES: Final[tuple[str, ...]] = (
    "Eto.*",
    "Eto.macOS.*",
    "Grasshopper.*",
    "Grasshopper2.*",
    "GrasshopperIO.*",
    "Microsoft.macOS.*",
    "Rhino.Runtime.Code.*",
    "Rhino.UI.*",
    "RhinoCodePlatform.Rhino3D.*",
    "RhinoCommon.*",
    "System.Drawing.Common.*",
)

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class PackageParams(BaseParams):
    """Parameters shared by package verbs."""

    slug: str = ""
    version: str = ""

    @override
    def _arity(self, verb: str) -> int:
        _ = verb
        return 0


class _CommitMarker(Base, frozen=True, gc=False):
    pid: int
    previous: str


class _MsbuildProps(Base, frozen=True):
    Properties: dict[str, str] = msgspec.field(default_factory=dict)


class YakMeta(Base, frozen=True, gc=False):
    """Validated yak package metadata."""

    project: str
    manifest_dir: Path
    target_dir: Path
    assembly_name: str
    target_ext: str
    yak_path: Path
    yak_platform: str
    yak_push_source: str
    package_dir: Path
    package_pattern: str
    target_framework: str = ""
    project_dir: Path = Path()

    @classmethod
    def from_props(cls, project: str, props: dict[str, str], settings: AssaySettings, slug: str) -> Result[YakMeta, Fault]:
        """Build validated yak metadata from MSBuild properties.

        Returns:
            Validated yak metadata, or a metadata/precondition fault.
        """
        missing = tuple(name for name in _META_PROPS if name != "YakPushSource" and not props.get(name))
        match missing:
            case ():
                return cls(
                    project=project,
                    manifest_dir=Path(props["YakManifestDirectory"]),
                    target_dir=Path(props["TargetDir"]),
                    assembly_name=props["AssemblyName"],
                    target_ext=props["TargetExt"],
                    yak_path=Path(props["YakPath"]),
                    yak_platform=props["YakPlatform"],
                    yak_push_source=props.get("YakPushSource", ""),
                    package_dir=Path(props["YakPackageDirectory"]),
                    package_pattern=props["YakPackagePattern"],
                    target_framework=props["TargetFramework"],
                    project_dir=Path(props["MSBuildProjectDirectory"]),
                ).validate(settings, slug, props["YakPackageSlug"])
            case names:
                return Error(Fault(("dotnet", "msbuild", project), message=f"missing MSBuild properties: {', '.join(names)}"))

    def validate(self, settings: AssaySettings, slug: str, evaluated_slug: str) -> Result[YakMeta, Fault]:
        """Validate package staging preconditions.

        Returns:
            Current metadata when staging preconditions hold, or a fault.
        """
        root = Path(str(settings.root)).resolve()
        project = (root / self.project).resolve()
        project_dir = _absolute(root, self.project_dir)
        manifest_dir = _absolute(root, self.manifest_dir)
        package_dir = _absolute(root, self.package_dir)
        expected = (project_dir / "bin" / settings.configuration.value / self.target_framework).resolve()
        resolved = _absolute(root, self.target_dir)
        checks: tuple[tuple[bool, str], ...] = (
            (project.is_file() and project.is_relative_to(root), f"package project escaped workspace: {self.project}"),
            (project_dir.is_relative_to(root), f"project directory escaped workspace: {self.project_dir}"),
            (manifest_dir.is_relative_to(root), f"manifest directory escaped workspace: {self.manifest_dir}"),
            (package_dir.is_relative_to(root), f"package directory escaped workspace: {self.package_dir}"),
            (_safe_package_pattern(self.package_pattern), f"unsafe package pattern for {slug}: {self.package_pattern}"),
            (evaluated_slug == slug, f"package slug mismatch for {self.project}: expected {slug}, evaluated {evaluated_slug}"),
            (self.target_ext == _RHP, f"package project must emit {_RHP} for {slug}: {self.project}"),
            (resolved.is_relative_to(root) and resolved == expected, f"refusing to clean unexpected output directory: {self.target_dir}"),
            (
                self.yak_platform == _YAK_PLATFORM and fnmatch.fnmatch(self.package_pattern, _YAK_DISTRIBUTION_GLOB),
                f"package distribution must match {_YAK_DISTRIBUTION_GLOB} for {slug}: {self.package_pattern}",
            ),
            (self.yak_path.is_file() and os.access(self.yak_path, os.X_OK), f"yak not executable at {self.yak_path}"),
        )
        return next((Error(Fault(("yak", slug), message=detail)) for ok, detail in checks if not ok), Ok(self))


# --- [TABLES] ---------------------------------------------------------------------------

_DECODER: Final[msgspec.json.Decoder[_MsbuildProps]] = msgspec.json.Decoder(_MsbuildProps)
_MARKER_DECODER: Final[msgspec.json.Decoder[_CommitMarker]] = msgspec.json.Decoder(_CommitMarker)

# Keyed by (verb, is_rasm_bridge_slug); publish folds the full stage->install->push pipeline, and the
# bridge slug additionally cycles the live host via quit before install and a status refresh after it.
_STEP_POLICY: Final[dict[tuple[str, bool], tuple[_LifecycleStep, ...]]] = {
    ("publish", False): (_LifecycleStep.INSTALL, _LifecycleStep.PUSH),
    ("publish", True): (_LifecycleStep.QUIT, _LifecycleStep.INSTALL, _LifecycleStep.REFRESH, _LifecycleStep.PUSH),
}

# --- [SERVICES] -------------------------------------------------------------------------

_LOG: structlog.stdlib.BoundLogger = structlog.get_logger("assay.package")

# --- [OPERATIONS] -----------------------------------------------------------------------


def _absolute(root: Path, path: Path) -> Path:
    return path.resolve() if path.is_absolute() else (root / path).resolve()


def _safe_package_pattern(pattern: str) -> bool:
    return bool(pattern) and "/" not in pattern and "\\" not in pattern and "\x00" not in pattern and ".." not in Path(pattern).parts


def _yak_tool(meta: YakMeta, command: _Step, mode: Mode) -> Result[Tool, Fault]:
    # Input.NONE: no changed-file routing tail; yak operates on committed package_dir.
    match select(Claim.PACKAGE, Language.CSHARP):
        case (base, *_):
            return Ok(msgspec.structs.replace(base, command=(str(meta.yak_path), *command), mode=mode))
        case _:
            return Error(Fault(("yak", *command), message="no yak catalog row for Claim.PACKAGE"))


def _yak_build_tail(meta: YakMeta, version: str) -> _Step:
    return ("build", "--platform", meta.yak_platform, "--version", version)


def _yak_install_tail(package_file: Path) -> _Step:
    return ("install", str(package_file))


def _yak_push_tail(meta: YakMeta, package_file: Path) -> _Step:
    source = ("--source", meta.yak_push_source) if meta.yak_push_source else ()
    return ("push", *source, str(package_file))


def _run_yak(meta: YakMeta, command: _Step, mode: Mode, *, cwd: Path, settings: AssaySettings, scope: ArtifactScope) -> Result[Completed, Fault]:
    # Non-zero yak exits stay on Completed, not Fault.
    return _yak_tool(meta, command, mode).bind(
        lambda tool: run_check(Check(tool=tool, cwd=cwd), settings=settings, scope=scope, routed=Routed(language=tool.language, scope=Scope.CHANGED))
    )


def evaluate_meta(settings: AssaySettings, scope: ArtifactScope, project: str, slug: str, version: str) -> Result[YakMeta, Fault]:
    """Evaluate and validate yak metadata for one project.

    Returns:
        Validated yak metadata, or an MSBuild/decode/precondition fault.
    """
    query: _Step = (
        "msbuild",
        project,
        f"-p:Configuration={settings.configuration.value}",
        f"-p:Version={version}",
        f"-p:YakVersion={version}",
        *(f"-getProperty:{name}" for name in _META_PROPS),
        "-nologo",
    )
    tool = Tool("dotnet-msbuild", Runner.DOTNET, query, Input.NONE, Language.CSHARP, Claim.PACKAGE, mode=Mode.QUERY)
    check = Check(tool=tool, cwd=Path(str(settings.root)))
    routed = Routed(language=tool.language, scope=Scope.CHANGED)
    return run_check(check, settings=settings, scope=scope, routed=routed).bind(
        lambda done: _decode_props(project, done).bind(lambda props: YakMeta.from_props(project, props, settings, slug))
    )


def _decode_props(project: str, done: Completed) -> Result[dict[str, str], Fault]:
    try:
        return Ok(_DECODER.decode(done.stdout or b"{}").Properties)
    except msgspec.DecodeError:
        tail = (done.stdout or done.stderr or b"").decode(errors="replace").strip()[:512]
        return Error(Fault(("dotnet", "msbuild", project), message=f"msbuild metadata evaluation failed (exit {done.returncode}): {tail}"))


def _resolve_project(settings: AssaySettings, slug: str) -> Result[str, Fault]:
    return _package_projects(settings).bind(lambda projects: _slugged(settings, projects)).bind(lambda pairs: _lone_match(slug, pairs))


def _package_projects(settings: AssaySettings) -> Result[tuple[str, ...], Fault]:
    root = settings.root
    roots = tuple(root / name for name in _PACKAGE_ROOTS if (root / name).is_dir())
    found = tuple(sorted({p.relative_to(root).as_posix() for base in roots for p in base.rglob(f"*{_CSPROJ}")}))
    return Ok(found)


def _slugged(settings: AssaySettings, projects: tuple[str, ...]) -> Result[tuple[tuple[str, str], ...], Fault]:
    # Empty slug identifies a non-yak project; sequence short-circuits on the first I/O fault.
    return sequence(block.of_seq(_csproj_slug(settings, p) for p in projects)).map(lambda slugs: tuple(zip(projects, tuple(slugs), strict=True)))


def _lone_match(slug: str, pairs: tuple[tuple[str, str], ...]) -> Result[str, Fault]:
    matched = tuple(project for project, project_slug in pairs if project_slug == slug)
    match matched:
        case (only,):
            return Ok(only)
        case ():
            return Error(Fault(("package", slug), message=f"expected one package project for {slug}, found 0"))
        case _:
            return Error(Fault(("package", slug), message=f"expected one package project for {slug}, found {len(matched)} duplicates"))


def _csproj_slug(settings: AssaySettings, project: str) -> Result[str, Fault]:
    return _read_bytes(Path(str(settings.root / project))).map(lambda raw: next(iter(parse_csproj(raw, _YAK_SLUG_PROP)), ""))


def _read_bytes(path: Path) -> Result[bytes, Fault]:
    try:
        return Ok(path.read_bytes())
    except FileNotFoundError:
        return Ok(b"")
    except OSError as exc:
        return Error(Fault(("read", str(path)), message=str(exc)[:1024]))


def _stage_artifacts(meta: YakMeta, staged: Path, target_dir: Path, extra_dirs: tuple[Path, ...]) -> Result[Path, Fault]:
    # Host-provided assemblies are excluded; manifest and primary .rhp must exist before yak build.
    manifest = meta.manifest_dir / "manifest.yml"
    primary = target_dir / f"{meta.assembly_name}{meta.target_ext}"
    match (manifest.is_file(), primary.is_file()):
        case (False, _):
            return Error(Fault(("yak", "build"), message=f"missing yak manifest: {manifest}"))
        case (_, False):
            return Error(Fault(("yak", "build"), message=f"missing primary artifact: {primary}"))
        case _ if missing := tuple(path for path in extra_dirs if not path.is_dir()):
            return Error(Fault(("yak", "build"), message=f"missing package closure output: {', '.join(str(path) for path in missing)}"))
        case _:
            return _copy_tree(meta, staged, target_dir, extra_dirs)


def _copy_tree(meta: YakMeta, staged: Path, target_dir: Path, extra_dirs: tuple[Path, ...]) -> Result[Path, Fault]:
    sources: Iterable[Path] = chain(
        (meta.manifest_dir / name for name in _MANIFEST_FILES if (meta.manifest_dir / name).is_file()),
        (
            p
            for root in (target_dir, *extra_dirs)
            for p in root.iterdir()
            if p.suffix in _ARTIFACT_SUFFIXES and not any(fnmatch.fnmatch(p.name, pattern) for pattern in _HOST_EXCLUDES)
        ),
    )
    try:
        tuple(copy2(src, staged / src.name) for src in sources)
    except OSError as exc:
        return Error(Fault(("yak", "build"), message=str(exc)[:1024]))
    return Ok(staged)


def _pending_marker(package_dir: Path) -> Path:
    return package_dir.with_name(f"{package_dir.name}{_COMMIT_PENDING}")


def _recover(meta: YakMeta, slug: str) -> Result[str, Fault]:
    """Heal an interrupted package-directory swap under the held stage lease.

    Returns:
        Recovery direction taken, or a BUSY fault while the marker pid is alive.
    """

    def decoded(raw: bytes) -> _CommitMarker | None:
        try:
            return _MARKER_DECODER.decode(raw)
        except msgspec.DecodeError:
            return None

    def settle(marker: Path, mark: _CommitMarker | None) -> Result[str, Fault]:
        match mark:
            case None:
                marker.unlink(missing_ok=True)
                _LOG.warning("package.recover", slug=slug, direction="clear", reason="undecodable marker")
                return Ok("clear")
            case _ if not proc_dead(mark.pid):
                return Error(Fault(("yak", "recover", slug), status=RailStatus.BUSY, message=f"package commit pending under live pid {mark.pid}"))
            case _:
                previous = Path(mark.previous)
                sibling = previous.parent == meta.package_dir.parent and previous.name.startswith(f"{meta.package_dir.name}.previous.")
                # Existing package_dir wins; missing package_dir plus sibling previous is the only rollback shape.
                direction = "forward" if meta.package_dir.exists() else ("back" if sibling and previous.is_dir() else "clear")
                try:
                    rmtree(previous, ignore_errors=True) if direction == "forward" and sibling else None
                    previous.replace(meta.package_dir) if direction == "back" else None
                    marker.unlink(missing_ok=True)
                except OSError as exc:
                    return Error(Fault(("yak", "recover", slug), message=str(exc)[:1024]))
                _LOG.warning("package.recover", slug=slug, direction=direction, pid=mark.pid, previous=mark.previous)
                return Ok(direction)

    marker = _pending_marker(meta.package_dir)
    return _read_bytes(marker).bind(lambda raw: Ok("absent") if not marker.is_file() else settle(marker, decoded(raw)))


def _commit(meta: YakMeta, staged: Path, slug: str) -> Result[Report, Fault]:
    # The sentinel brackets the swap so dead-pid recovery can distinguish forward and rollback shapes.
    previous = meta.package_dir.with_name(f"{meta.package_dir.name}.previous.{os.getpid()}")
    marker = _pending_marker(meta.package_dir)
    try:
        rmtree(previous, ignore_errors=True)
        marker.write_bytes(msgspec.json.encode(_CommitMarker(pid=os.getpid(), previous=str(previous))))
        meta.package_dir.replace(previous) if meta.package_dir.exists() else None
        staged.replace(meta.package_dir)
        rmtree(previous, ignore_errors=True)
    except OSError as exc:
        previous.replace(meta.package_dir) if previous.exists() and not meta.package_dir.exists() else None
        rmtree(staged, ignore_errors=True)
        return Error(Fault(("yak", "build", slug), message=str(exc)[:1024]))
    finally:
        # Clear only after commit or rollback has settled the directory shape.
        marker.unlink(missing_ok=True)
    return Ok(
        fold(
            Claim.PACKAGE,
            "stage",
            (Completed(("yak", "build", slug), 0, status=RailStatus.OK),),
            detail=PackageRun(stage=str(meta.package_dir), project=meta.project, pattern=meta.package_pattern, version=""),
        )
    )


def _stage_meta(settings: AssaySettings, scope: ArtifactScope, meta: YakMeta, slug: str, version: str) -> Result[Report, Fault]:
    # Temp dir is under package_dir's parent to keep the final rename on the same filesystem.
    resource = f"{_PACKAGE_STAGE}-{meta.package_dir.name}"

    def staged_build() -> Result[Report, Fault]:
        staged = Path(mkdtemp(prefix=f"{meta.package_dir.name}.", dir=meta.package_dir.parent))
        outcome = _build_outputs(meta, settings, scope, slug, version).bind(
            lambda built: _copy_after_build(meta, staged, slug, version, built, settings, scope)
        )
        match outcome:
            case Result(tag="error"):
                rmtree(staged, ignore_errors=True)
                return outcome
            case _:
                return outcome

    def locked(_held: object) -> Result[Report, Fault]:
        meta.package_dir.parent.mkdir(parents=True, exist_ok=True)
        return _recover(meta, slug).bind(lambda _direction: staged_build())

    return leased(resource, locked, settings=settings, run_id=settings.run_id, project=slug, mode="exclusive")


def _build_outputs(meta: YakMeta, settings: AssaySettings, scope: ArtifactScope, slug: str, version: str) -> Result[Completed, Fault]:
    projects = (_RASM_BRIDGE_SHELL_PROJECT, meta.project) if slug == _RASM_BRIDGE_SLUG else (meta.project,)

    def run_project(project: str) -> Result[Completed, Fault]:
        tool = Tool(
            "dotnet-build",
            Runner.DOTNET,
            ("build", project, "-c", settings.configuration.value, f"-p:Version={version}", "-v:quiet", "/clp:ErrorsOnly"),
            Input.NONE,
            Language.CSHARP,
            Claim.PACKAGE,
            mode=Mode.BUILD,
        )
        return run_check(
            Check(tool=tool, cwd=Path(str(settings.root))),
            settings=settings,
            scope=scope,
            routed=Routed(language=Language.CSHARP, scope=Scope.CHANGED),
        )

    def combined(done: tuple[Completed, ...]) -> Completed:
        terminal = next((row for row in done if row.status in {RailStatus.FAILED, RailStatus.FAULTED, RailStatus.TIMEOUT}), done[-1])
        return msgspec.structs.replace(
            terminal,
            status=reduce(lambda status, row: join(status, row.status), done, RailStatus.OK),
            notes=tuple(chain.from_iterable(row.notes for row in done)),
            artifacts=tuple(chain.from_iterable(row.artifacts for row in done)),
        )

    rows: Result[tuple[Completed, ...], Fault] = reduce(
        lambda acc, project: acc.bind(lambda done: run_project(project).map(lambda row: (*done, row))), projects, Ok(())
    )
    return rows.map(combined)


def _copy_after_build(
    meta: YakMeta, staged: Path, slug: str, version: str, built: Completed, settings: AssaySettings, scope: ArtifactScope
) -> Result[Report, Fault]:
    match built.status:
        case RailStatus.FAILED | RailStatus.FAULTED | RailStatus.TIMEOUT:
            rmtree(staged, ignore_errors=True)
            return Ok(fold(Claim.PACKAGE, "stage", (built,)))
        case _:
            target_dir = Path(scope.path) / "bin" / Path(meta.project).stem / settings.configuration.value.lower()
            extra_dirs = (
                (Path(scope.path) / "bin" / Path(_RASM_BRIDGE_SHELL_PROJECT).stem / settings.configuration.value.lower(),)
                if slug == _RASM_BRIDGE_SLUG
                else ()
            )
            return (
                _stage_artifacts(meta, staged, target_dir, extra_dirs)
                .bind(lambda _: _run_yak(meta, _yak_build_tail(meta, version), Mode.STAGE, cwd=staged, settings=settings, scope=scope))
                .bind(lambda done: _commit_or_fail(meta, staged, slug, version, done))
            )


def _commit_or_fail(meta: YakMeta, staged: Path, slug: str, version: str, done: Completed) -> Result[Report, Fault]:
    match done.status:
        case RailStatus.FAILED | RailStatus.FAULTED | RailStatus.TIMEOUT:
            rmtree(staged, ignore_errors=True)
            return Ok(fold(Claim.PACKAGE, "stage", (done,)))
        case _:
            return _commit(meta, staged, slug).map(lambda report: msgspec.structs.replace(report, detail=_stamp_version(report.detail, version)))


def _stamp_version(detail: object, version: str) -> PackageRun:
    match detail:
        case PackageRun() as run:
            return msgspec.structs.replace(run, version=version)
        case _:
            return PackageRun(version=version)


def _run_step(settings: AssaySettings, scope: ArtifactScope, meta: YakMeta, package_file: Path, step: _LifecycleStep) -> Result[Completed, Fault]:
    # Refresh failure after install is recoverable via bridge relaunch; bridge steps fold into Completed, not Fault.
    match step:
        case _LifecycleStep.INSTALL:
            return _run_yak(meta, _yak_install_tail(package_file), step.mode, cwd=meta.package_dir, settings=settings, scope=scope)
        case _LifecycleStep.PUSH:
            return _run_yak(meta, _yak_push_tail(meta, package_file), step.mode, cwd=meta.package_dir, settings=settings, scope=scope)
        case _:
            return client_run(settings, step.wire)


def _resolve_package_file(meta: YakMeta) -> Result[Path, Fault]:
    # Resolves from committed package_dir so install/push never operate on a temp-staged artifact.
    matches = sorted(meta.package_dir.glob(meta.package_pattern))
    match matches:
        case [only]:
            return Ok(only)
        case _:
            return Error(Fault(("yak", "install"), message=f"expected one package for pattern {meta.package_pattern}, found {len(matches)}"))


def _finish(settings: AssaySettings, scope: ArtifactScope, meta: YakMeta, slug: str, verb: str, staged: Report) -> Result[Report, Fault]:
    # A non-OK stage commit short-circuits before any post-stage step; an empty policy yields the staged report verbatim.
    match staged.status:
        case RailStatus.FAILED | RailStatus.FAULTED | RailStatus.TIMEOUT | RailStatus.BUSY:
            return Ok(staged)
        case _ if not (steps := _STEP_POLICY.get((verb, slug == _RASM_BRIDGE_SLUG), ())):
            return Ok(staged)
        case _:
            return _resolve_package_file(meta).bind(lambda package_file: _drive_steps(settings, scope, meta, verb, staged, package_file, steps))


def _drive_steps(
    settings: AssaySettings, scope: ArtifactScope, meta: YakMeta, verb: str, staged: Report, package_file: Path, steps: tuple[_LifecycleStep, ...]
) -> Result[Report, Fault]:
    def run_steps() -> Result[Report, Fault]:
        return _fold_steps(settings, scope, meta, verb, staged, package_file, steps)

    match any(step.needs_bridge for step in steps):
        case True:
            return bridge_lease(settings, run_steps)
        case False:
            return run_steps()


def _fold_steps(
    settings: AssaySettings, scope: ArtifactScope, meta: YakMeta, verb: str, staged: Report, package_file: Path, steps: tuple[_LifecycleStep, ...]
) -> Result[Report, Fault]:
    # Stage evidence stays in the accumulator; only spawn or lease Faults short-circuit.
    seed: Result[tuple[Completed, ...], Fault] = Ok(())
    folded = reduce(
        lambda acc, step: acc.bind(lambda done: _run_step(settings, scope, meta, package_file, step).map(lambda c: (*done, c))), steps, seed
    )
    return folded.map(lambda outcomes: _merge_stage(staged, fold(Claim.PACKAGE, verb, outcomes, detail=staged.detail, promote_empty=True)))


def _merge_stage(staged: Report, steps: Report) -> Report:
    # Stage rows lead so build evidence and post-stage outcomes survive in one report.
    return msgspec.structs.replace(
        steps,
        status=join(staged.status, steps.status),
        counts=Counts(
            ok=staged.counts.ok + steps.counts.ok, failed=staged.counts.failed + steps.counts.failed, total=staged.counts.total + steps.counts.total
        ),
        results=(*staged.results, *steps.results),
        artifacts=(*staged.artifacts, *steps.artifacts),
        notes=(*staged.notes, *steps.notes),
    )


def _lifecycle(settings: AssaySettings, scope: ArtifactScope, params: PackageParams, verb: str) -> Result[Report, Fault]:
    def staged_then_finish(meta: YakMeta) -> Result[Report, Fault]:
        return _stage_meta(settings, scope, meta, params.slug, params.version).bind(
            lambda staged: _finish(settings, scope, meta, params.slug, verb, staged)
        )

    def locked(_held: object) -> Result[Report, Fault]:
        return (
            _resolve_project(settings, params.slug)
            .bind(lambda project: evaluate_meta(settings, scope, project, params.slug, params.version))
            .bind(staged_then_finish)
        )

    return leased(f"package-{params.slug or 'default'}", locked, settings=settings, run_id=settings.run_id, project=params.slug, mode="exclusive")


def _plan_report(meta: YakMeta, version: str) -> Report:
    return msgspec.structs.replace(
        fold(Claim.PACKAGE, "plan", ()),
        status=RailStatus.OK,
        detail=PackageRun(
            project=meta.project,
            pattern=meta.package_pattern,
            version=version,
            manifest_dir=str(meta.manifest_dir),
            target_dir=str(meta.target_dir),
            package_dir=str(meta.package_dir),
            target_framework=meta.target_framework,
            platform=meta.yak_platform,
            push_source=meta.yak_push_source,
            yak_path=str(meta.yak_path),
        ),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def publish(settings: AssaySettings, scope: ArtifactScope, params: PackageParams) -> Result[Report, Fault]:
    """Run the full yak pipeline: stage commit, then install, push, and bridge refresh under lease.

    Returns:
        Package lifecycle report, or a stage/install/push fault.
    """
    return _lifecycle(settings, scope, params, "publish")


def list(  # noqa: A001
    settings: AssaySettings, scope: ArtifactScope, params: PackageParams
) -> Result[Report, Fault]:
    """List package projects and slugs.

    Returns:
        Package roster report, or a project-discovery fault.
    """
    _ = (scope, params)
    return (
        _package_projects(settings)
        .bind(lambda projects: _slugged(settings, projects))
        .map(
            lambda pairs: msgspec.structs.replace(
                fold(Claim.PACKAGE, "list", ()),
                status=RailStatus.OK,
                results=tuple(Match(id=slug, kind=ArtifactKind.SCOPE, text=project) for project, slug in pairs if slug),
            )
        )
    )


def plan(settings: AssaySettings, scope: ArtifactScope, params: PackageParams) -> Result[Report, Fault]:
    """Evaluate package metadata without staging.

    Returns:
        Package plan report, or a metadata-evaluation fault.
    """
    return (
        _resolve_project(settings, params.slug)
        .bind(lambda project: evaluate_meta(settings, scope, project, params.slug, params.version))
        .map(lambda meta: _plan_report(meta, params.version))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PackageParams", "YakMeta", "evaluate_meta", "list", "plan", "publish"]
