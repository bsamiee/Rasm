"""Run yak package stage, deploy, publish, list, and plan rails."""

from dataclasses import dataclass
import fnmatch
from functools import reduce
from itertools import chain
import os
from pathlib import Path
from shutil import copy2, rmtree
from tempfile import mkdtemp
from typing import Final, override, TYPE_CHECKING

from expression import Error, Ok, Result
from expression.collections import block
from expression.extra.result import sequence
import msgspec

from tools.assay.composition.catalog import select
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay.core.engine import leased, run_check
from tools.assay.core.model import (
    ArtifactKind,
    Base,
    BaseParams,
    Check,
    Claim,
    Completed,
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
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus

# canonical Claim.BRIDGE Mode.CLIENT seam for the rasm-bridge quit/refresh lifecycle steps
from tools.assay.rails.bridge import _client_run as _bridge_client_run  # noqa: PLC2701


if TYPE_CHECKING:
    from collections.abc import Callable, Iterable


# --- [TYPES] ----------------------------------------------------------------------------

type _Step = tuple[str, ...]  # one resolved yak/bridge argv tail (the command minus the launcher head)


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class PackageParams(BaseParams):
    """Parameters shared by package verbs.

    Attributes:
        slug: Yak package slug.
        version: Yak package version passed to build and plan operations.

    """

    slug: str = ""  # required by stage/deploy/publish/plan; list ignores both
    version: str = ""

    @override
    def _arity(self, verb: str) -> int:
        _ = verb
        return 0


class _MsbuildProps(Base, frozen=True):
    """MSBuild property query JSON envelope."""

    Properties: dict[str, str] = {}  # noqa: RUF012  # MSBuild wire key is PascalCase; default-empty so a missing block decodes total


class YakMeta(Base, frozen=True, gc=False):
    """Validated yak package metadata.

    Attributes:
        project: Package project path.
        manifest_dir: Directory containing yak manifest assets.
        target_dir: Build output directory.
        assembly_name: Primary assembly name.
        target_ext: Primary artifact extension.
        yak_path: Yak executable path.
        yak_platform: Yak platform token.
        yak_push_source: Optional yak push source.
        package_dir: Committed package directory.
        package_pattern: Expected package filename pattern.
        target_framework: Target framework.
        project_dir: Project directory.

    """

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
        """Build validated package metadata from MSBuild properties.

        Args:
            project: Project path.
            props: Evaluated MSBuild properties.
            settings: Runtime settings.
            slug: Requested package slug.

        Returns:
            Result containing validated metadata or a precondition fault.

        """
        # YakPushSource is optional (server push target); every other _META_PROPS member must be present + non-empty
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

        Args:
            settings: Runtime settings.
            slug: Requested package slug.
            evaluated_slug: Slug evaluated by MSBuild.

        Returns:
            Result containing this metadata or a precondition fault.

        """
        root = Path(str(settings.root)).resolve()  # output-dir containment guard is local-fs; UPath → Path
        expected = (self.project_dir / "bin" / settings.configuration.value / self.target_framework).resolve()
        resolved = self.target_dir.resolve()
        checks: tuple[tuple[bool, str], ...] = (
            (evaluated_slug == slug, f"package slug mismatch for {self.project}: expected {slug}, evaluated {evaluated_slug}"),
            (self.target_ext == _RHP, f"package project must emit {_RHP} for {slug}: {self.project}"),
            # containment guard: refuse to clean an output dir outside the worktree's bin/<config>/<tfm>
            (resolved.is_relative_to(root) and resolved == expected, f"refusing to clean unexpected output directory: {self.target_dir}"),
            (
                self.yak_platform == _YAK_PLATFORM and fnmatch.fnmatch(self.package_pattern, _YAK_DISTRIBUTION_GLOB),
                f"package distribution must match {_YAK_DISTRIBUTION_GLOB} for {slug}: {self.package_pattern}",
            ),
            # X_OK probe keeps a non-runnable YakPath a precondition fault, never a yak build spawn OSError
            (self.yak_path.is_file() and os.access(self.yak_path, os.X_OK), f"yak not executable at {self.yak_path}"),
        )
        return next((Error(Fault(("yak", slug), message=detail)) for ok, detail in checks if not ok), Ok(self))


# --- [CONSTANTS] ------------------------------------------------------------------------

_RHP: Final[str] = ".rhp"
_YAK_PLATFORM: Final[str] = "mac"  # the sole supported distribution host
_YAK_DISTRIBUTION_GLOB: Final[str] = "*-rh9_*-mac.yak"  # the validated YakPackagePattern shape
_RASM_BRIDGE_SLUG: Final[str] = "rasm-bridge"  # the lone lifecycle special-case taking the shared bridge.lock
_PACKAGE_STAGE: Final[str] = "package-stage"  # the per-dir stage lease resource stem
_BRIDGE_LOCK: Final[str] = "bridge"  # the global live-Rhino lease resource stem shared with rails/bridge.py
_PACKAGE_ROOTS: Final[tuple[str, ...]] = ("apps", "tools")  # the worktree subtrees holding packageable .csproj rows
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

# (verb, slug == rasm-bridge) -> ordered yak/bridge step kinds folded after a successful stage.
_STEP_POLICY: Final[dict[tuple[str, bool], tuple[str, ...]]] = {
    ("deploy", False): ("install",),
    ("deploy", True): ("quit", "install", "refresh"),
    ("publish", False): ("install", "push"),
    ("publish", True): ("quit", "install", "refresh", "push"),
}

_DECODER: Final[msgspec.json.Decoder[_MsbuildProps]] = msgspec.json.Decoder(_MsbuildProps)  # one-pass cached envelope decode


# --- [OPERATIONS] -----------------------------------------------------------------------


def _yak_tool(meta: YakMeta, command: _Step, mode: Mode) -> Result[Tool, Fault]:
    # command carries the complete invocation so the Input.NONE place projection appends one empty tail
    # and the spawned argv is exactly command; a missing catalog row faults rather than IndexError-ing.
    match select(Claim.PACKAGE, Language.CSHARP):
        case (base, *_):
            return Ok(msgspec.structs.replace(base, command=(str(meta.yak_path), *command), mode=mode))
        case _:
            return Error(Fault(("yak", *command), message="no yak catalog row for Claim.PACKAGE"))


def _yak_build_tail(meta: YakMeta, version: str) -> _Step:
    return ("build", "--platform", meta.yak_platform, "--version", version)  # run with cwd=stage


def _yak_install_tail(package_file: Path) -> _Step:
    return ("install", str(package_file))  # run with cwd=package_dir


def _yak_push_tail(meta: YakMeta, package_file: Path) -> _Step:
    source = ("--source", meta.yak_push_source) if meta.yak_push_source else ()  # --source only when declared
    return ("push", *source, str(package_file))


def _run_yak(meta: YakMeta, command: _Step, mode: Mode, *, cwd: Path, settings: AssaySettings, scope: ArtifactScope) -> Result[Completed, Fault]:
    # the Routed is the minimal C#/CHANGED shape place needs for an Input.NONE tool; the full argv lives
    # in _yak_tool's command. A non-zero exit rides Ok(Completed(FAILED)); only a spawn/timeout faults.
    return _yak_tool(meta, command, mode).bind(
        lambda tool: run_check(Check(tool=tool, cwd=cwd), settings=settings, scope=scope, routed=Routed(language=tool.language, scope=Scope.CHANGED))
    )


def evaluate_meta(settings: AssaySettings, scope: ArtifactScope, project: str, slug: str, version: str) -> Result[YakMeta, Fault]:
    """Evaluate and validate yak metadata for one project.

    Args:
        settings: Runtime settings.
        scope: Artifact scope.
        project: Project path.
        slug: Requested package slug.
        version: Requested package version.

    Returns:
        Result containing validated metadata or an evaluation fault.

    """
    query: _Step = (
        "msbuild",
        project,
        f"-p:Configuration={settings.configuration.value}",
        f"-p:YakVersion={version}",
        *(f"-getProperty:{name}" for name in _META_PROPS),
        "-nologo",
    )
    tool = Tool("dotnet-msbuild", Runner.DOTNET, query, Input.NONE, Language.CSHARP, Claim.PACKAGE, mode=Mode.QUERY)
    check = Check(tool=tool, cwd=Path(str(settings.root)))  # subprocess cwd is inherently local
    routed = Routed(language=tool.language, scope=Scope.CHANGED)
    return run_check(check, settings=settings, scope=scope, routed=routed).bind(
        lambda done: _decode_props(project, done).bind(lambda props: YakMeta.from_props(project, props, settings, slug))
    )


def _decode_props(project: str, done: Completed) -> Result[dict[str, str], Fault]:
    # codec boundary: a non-zero MSBuild exit emits an MSB####/restore error line (not the envelope) on
    # stdout, so the decode raises DecodeError. The Fault carries the bounded tail so the operator sees what failed.
    try:
        return Ok(_DECODER.decode(done.stdout or b"{}").Properties)
    except msgspec.DecodeError:
        tail = (done.stdout or done.stderr or b"").decode(errors="replace").strip()[:512]
        return Error(Fault(("dotnet", "msbuild", project), message=f"msbuild metadata evaluation failed (exit {done.returncode}): {tail}"))


def _resolve_project(settings: AssaySettings, slug: str) -> Result[str, Fault]:
    # resolve by slug, never by changed-set; zero or duplicate matches fault so the lifecycle never
    # operates on an ambiguous target.
    return _package_projects(settings).bind(lambda projects: _slugged(settings, projects)).bind(lambda pairs: _lone_match(slug, pairs))


def _package_projects(settings: AssaySettings) -> Result[tuple[str, ...], Fault]:
    root = settings.root
    roots = tuple(root / name for name in _PACKAGE_ROOTS if (root / name).is_dir())
    found = tuple(sorted({p.relative_to(root).as_posix() for base in roots for p in base.rglob(f"*{_CSPROJ}")}))
    return Ok(found)


def _slugged(settings: AssaySettings, projects: tuple[str, ...]) -> Result[tuple[tuple[str, str], ...], Fault]:
    # sequence collapses the per-project slugs into one rail (an IO read error dominates); absence is
    # already "" from _csproj_slug so a non-yak project rides through as an empty slug, not a fault.
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
    # direct XML read, never a full MSBuild evaluation; absence → "" so a non-yak project drops out
    return _read_bytes(Path(str(settings.root / project))).map(_slug_from_bytes)  # .csproj XML read is local; UPath → Path


def _read_bytes(path: Path) -> Result[bytes, Fault]:
    try:
        return Ok(path.read_bytes())
    except FileNotFoundError:
        return Ok(b"")  # absence is the non-yak/removed-project case; a real IO fault must not mask as "no slug"
    except OSError as exc:
        return Error(Fault(("read", str(path)), message=str(exc)[:1024]))


def _slug_from_bytes(raw: bytes) -> str:
    import xml.etree.ElementTree as ET  # noqa: PLC0415, S405  # trusted local .csproj XML, parsed at the slug-read boundary only

    try:
        tree = ET.fromstring(raw or b"<Project/>")  # noqa: S314  # trusted local .csproj XML, never network-sourced
    except ET.ParseError:
        return ""
    found = next((el.text for el in tree.iter() if el.tag.rpartition("}")[2] == _YAK_SLUG_PROP and el.text), None)
    return (found or "").strip()


def _stage_artifacts(meta: YakMeta, staged: Path) -> Result[Path, Fault]:
    # _HOST_EXCLUDES drops the Rhino/Eto/GH assemblies the host already provides. A missing manifest or
    # primary .rhp is a precondition Fault so an incomplete distribution never reaches yak build.
    manifest = meta.manifest_dir / "manifest.yml"
    primary = meta.target_dir / f"{meta.assembly_name}{meta.target_ext}"
    match (manifest.is_file(), primary.is_file()):
        case (False, _):
            return Error(Fault(("yak", "build"), message=f"missing yak manifest: {manifest}"))
        case (_, False):
            return Error(Fault(("yak", "build"), message=f"missing primary artifact: {primary}"))
        case _:
            return _copy_tree(meta, staged)


def _copy_tree(meta: YakMeta, staged: Path) -> Result[Path, Fault]:
    sources: Iterable[Path] = chain(
        (meta.manifest_dir / name for name in _MANIFEST_FILES if (meta.manifest_dir / name).is_file()),
        (
            p
            for p in meta.target_dir.iterdir()
            if p.suffix in _ARTIFACT_SUFFIXES and not any(fnmatch.fnmatch(p.name, pattern) for pattern in _HOST_EXCLUDES)
        ),
    )
    try:
        tuple(copy2(src, staged / src.name) for src in sources)
    except OSError as exc:
        return Error(Fault(("yak", "build"), message=str(exc)[:1024]))
    return Ok(staged)


def _commit(meta: YakMeta, staged: Path, slug: str) -> Result[Report, Fault]:
    # atomic rotate: rotating to .previous.<pid> before the swap makes a crash mid-rotate recoverable;
    # an OSError restores the rotated-away tree (only when the live dir is now absent) and discards staged.
    previous = meta.package_dir.with_name(f"{meta.package_dir.name}.previous.{os.getpid()}")
    try:
        rmtree(previous, ignore_errors=True)
        meta.package_dir.replace(previous) if meta.package_dir.exists() else None
        staged.replace(meta.package_dir)
        rmtree(previous, ignore_errors=True)
    except OSError as exc:
        previous.replace(meta.package_dir) if previous.exists() and not meta.package_dir.exists() else None
        rmtree(staged, ignore_errors=True)
        return Error(Fault(("yak", "build", slug), message=str(exc)[:1024]))
    return Ok(
        fold(
            Claim.PACKAGE,
            "stage",
            (Completed(("yak", "build", slug), 0, status=RailStatus.OK),),
            detail=PackageRun(stage=str(meta.package_dir), project=meta.project, pattern=meta.package_pattern, version=""),
        )
    )


def _stage_meta(settings: AssaySettings, scope: ArtifactScope, meta: YakMeta, slug: str, version: str) -> Result[Report, Fault]:
    # the staged temp lives under the package dir's parent so the final replace is a same-filesystem
    # atomic rename; the lease keys on package_dir so two slugs sharing one parent serialize.
    meta.package_dir.parent.mkdir(parents=True, exist_ok=True)
    staged = Path(mkdtemp(prefix=f"{meta.package_dir.name}.", dir=meta.package_dir.parent))
    resource = f"{_PACKAGE_STAGE}-{meta.package_dir.name}"
    splice = _stage_artifacts(meta, staged)

    def locked(_held: object) -> Result[Report, Fault]:
        return splice.bind(lambda _: _run_yak(meta, _yak_build_tail(meta, version), Mode.STAGE, cwd=staged, settings=settings, scope=scope)).bind(
            lambda done: _commit_or_fail(meta, staged, slug, version, done)
        )

    outcome = leased(resource, locked, settings=settings, run_id=settings.run_id, project=slug, mode="exclusive")
    match outcome:
        case Result(tag="error"):
            rmtree(staged, ignore_errors=True)
            return outcome
        case _:
            return outcome


def _commit_or_fail(meta: YakMeta, staged: Path, slug: str, version: str, done: Completed) -> Result[Report, Fault]:
    match done.status:
        case RailStatus.FAILED | RailStatus.FAULTED | RailStatus.TIMEOUT:
            rmtree(staged, ignore_errors=True)
            return Ok(fold(Claim.PACKAGE, "stage", (done,)))
        case _:
            return _commit(meta, staged, slug).map(lambda report: msgspec.structs.replace(report, detail=_stamp_version(report.detail, version)))


def _stamp_version(detail: object, version: str) -> PackageRun:
    # the commit fold leaves version=""; stamp the staged version here
    match detail:
        case PackageRun() as run:
            return msgspec.structs.replace(run, version=version)
        case _:
            return PackageRun(version=version)


def _run_step(settings: AssaySettings, scope: ArtifactScope, meta: YakMeta, package_file: Path, kind: str) -> Result[Completed, Fault]:
    # install/push are yak Check rows (cwd=package_dir); quit/refresh drive the live host through the
    # bridge client (Mode.CLIENT) under the already-held bridge.lock.
    match kind:
        case "install":
            return _run_yak(meta, _yak_install_tail(package_file), Mode.DEPLOY, cwd=meta.package_dir, settings=settings, scope=scope)
        case "push":
            return _run_yak(meta, _yak_push_tail(meta, package_file), Mode.PUBLISH, cwd=meta.package_dir, settings=settings, scope=scope)
        case _:
            return _run_bridge_client(settings, scope, kind)


def _run_bridge_client(settings: AssaySettings, scope: ArtifactScope, verb: str) -> Result[Completed, Fault]:
    # routes through the canonical rails.bridge._client_run seam; a refresh fault after install leaves the
    # new .rhp on disk while the host is down — recoverable by a bare bridge launch, so it rides Completed(FAILED).
    _ = scope  # the bridge client must stay on its canonical bin/ output; _client_run pins scope=None internally
    return _bridge_client_run(settings, verb)


def _resolve_package_file(meta: YakMeta) -> Result[Path, Fault]:
    # glob the committed package_dir (post-swap), not the staged temp, so the resolved path is the live
    # distribution; zero or many matches faults so install/push never operate on an ambiguous artifact.
    matches = sorted(meta.package_dir.glob(meta.package_pattern))
    match matches:
        case [only]:
            return Ok(only)
        case _:
            return Error(Fault(("yak", "install"), message=f"expected one package for pattern {meta.package_pattern}, found {len(matches)}"))


def _finish(settings: AssaySettings, scope: ArtifactScope, meta: YakMeta, slug: str, verb: str, staged: Report) -> Result[Report, Fault]:
    # fold the post-stage step policy keyed by (verb, slug == rasm-bridge); a stage verb or non-OK stage
    # short-circuits to the staged Report unchanged.
    match (verb, staged.status):
        case ("stage", _) | (_, RailStatus.FAILED | RailStatus.FAULTED | RailStatus.TIMEOUT | RailStatus.BUSY):
            return Ok(staged)
        case _:
            steps = _STEP_POLICY.get((verb, slug == _RASM_BRIDGE_SLUG), ())
            return _resolve_package_file(meta).bind(lambda package_file: _drive_steps(settings, scope, meta, slug, verb, staged, package_file, steps))


def _drive_steps(
    settings: AssaySettings, scope: ArtifactScope, meta: YakMeta, slug: str, verb: str, staged: Report, package_file: Path, steps: tuple[str, ...]
) -> Result[Report, Fault]:
    # the bridge-bound policy acquires bridge.lock once and runs the whole sequence inside it so
    # quit → install → refresh is atomic under one lease; a non-bridge policy runs the yak steps directly.
    needs_bridge = any(step in {"quit", "refresh"} for step in steps)
    run_steps: Callable[[object], Result[Report, Fault]] = lambda _held: _fold_steps(  # noqa: E731  # thunk closes over the lease boundary
        settings, scope, meta, verb, staged, package_file, steps
    )
    match needs_bridge:
        case True:
            return leased(_BRIDGE_LOCK, run_steps, settings=settings, run_id=settings.run_id, project=slug, mode="exclusive")
        case False:
            return run_steps(None)


def _fold_steps(
    settings: AssaySettings, scope: ArtifactScope, meta: YakMeta, verb: str, staged: Report, package_file: Path, steps: tuple[str, ...]
) -> Result[Report, Fault]:
    # reduce threads a Result accumulator: each step appends its Completed (incl. a FAILED defect) and
    # short-circuits on a spawn/lease Fault; the terminal fold derives counts/status once in model.fold.
    seed: Result[tuple[Completed, ...], Fault] = Ok(())
    folded = reduce(
        lambda acc, kind: acc.bind(lambda done: _run_step(settings, scope, meta, package_file, kind).map(lambda c: (*done, c))), steps, seed
    )
    return folded.map(lambda outcomes: fold(Claim.PACKAGE, verb, outcomes, detail=staged.detail))


def _lifecycle(settings: AssaySettings, scope: ArtifactScope, params: PackageParams, verb: str) -> Result[Report, Fault]:
    # shared resolve → evaluate → stage → finish fold: validate before any lease, stage under the per-dir
    # lease, then fold the (verb, slug) step policy.

    def staged_then_finish(meta: YakMeta) -> Result[Report, Fault]:
        return _stage_meta(settings, scope, meta, params.slug, params.version).bind(
            lambda staged: _finish(settings, scope, meta, params.slug, verb, staged)
        )

    return (
        _resolve_project(settings, params.slug)
        .bind(lambda project: evaluate_meta(settings, scope, project, params.slug, params.version))
        .bind(staged_then_finish)
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def stage(settings: AssaySettings, scope: ArtifactScope, params: PackageParams) -> Result[Report, Fault]:
    """Stage one yak distribution.

    Args:
        settings: Runtime settings.
        scope: Artifact scope.
        params: Package params.

    Returns:
        Result containing the stage report or operational fault.

    """
    return _lifecycle(settings, scope, params, "stage")


def deploy(settings: AssaySettings, scope: ArtifactScope, params: PackageParams) -> Result[Report, Fault]:
    """Stage and install one yak distribution.

    Args:
        settings: Runtime settings.
        scope: Artifact scope.
        params: Package params.

    Returns:
        Result containing the deploy report or operational fault.

    """
    return _lifecycle(settings, scope, params, "deploy")


def publish(settings: AssaySettings, scope: ArtifactScope, params: PackageParams) -> Result[Report, Fault]:
    """Stage, install, and publish one yak distribution.

    Args:
        settings: Runtime settings.
        scope: Artifact scope.
        params: Package params.

    Returns:
        Result containing the publish report or operational fault.

    """
    return _lifecycle(settings, scope, params, "publish")


def list(settings: AssaySettings, scope: ArtifactScope, params: PackageParams) -> Result[Report, Fault]:  # noqa: A001  # registry binds the canonical verb name "list"
    """List package projects and slugs.

    Args:
        settings: Runtime settings.
        scope: Artifact scope.
        params: Package params.

    Returns:
        Result containing package match rows or an enumeration fault.

    """
    _ = (scope, params)  # list enumerates the whole package set, not one slug
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

    Args:
        settings: Runtime settings.
        scope: Artifact scope.
        params: Package params.

    Returns:
        Result containing package metadata detail or an evaluation fault.

    """
    return (
        _resolve_project(settings, params.slug)
        .bind(lambda project: evaluate_meta(settings, scope, project, params.slug, params.version))
        .map(lambda meta: _plan_report(meta, params.version))
    )


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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PackageParams", "YakMeta", "deploy", "evaluate_meta", "list", "plan", "publish", "stage"]
