"""Run yak package stage, deploy, publish, list, and plan rails."""

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
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import join, RailStatus

# Reuse the canonical bridge client seam for rasm-bridge lifecycle steps.
from tools.assay.rails.bridge import _client_run as _bridge_client_run  # noqa: PLC2701


if TYPE_CHECKING:
    from collections.abc import Iterable


# --- [TYPES] ----------------------------------------------------------------------------

type _Step = tuple[str, ...]


class _LifecycleStep(StrEnum):
    """Post-stage lifecycle step with its yak run mode and bridge-lock requirement."""

    mode: Mode
    needs_bridge: bool
    INSTALL = "install", Mode.DEPLOY, False
    PUSH = "push", Mode.PUBLISH, False
    QUIT = "quit", Mode.CLIENT, True
    REFRESH = "refresh", Mode.CLIENT, True

    def __new__(cls, value: str, mode: Mode, needs_bridge: bool) -> Self:  # noqa: FBT001  # enum payload binder mirrors enum field order
        """Bind the wire token, yak run mode, and bridge-lock flag payload."""
        member = str.__new__(cls, value)
        member._value_, member.mode, member.needs_bridge = value, mode, needs_bridge
        return member


# --- [CONSTANTS] ------------------------------------------------------------------------

_RHP: Final[str] = ".rhp"
_YAK_PLATFORM: Final[str] = "mac"
_YAK_DISTRIBUTION_GLOB: Final[str] = "*-rh9_*-mac.yak"
_RASM_BRIDGE_SLUG: Final[str] = "rasm-bridge"
_PACKAGE_STAGE: Final[str] = "package-stage"
_BRIDGE_LOCK: Final[str] = "bridge"
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


class _MsbuildProps(Base, frozen=True):
    Properties: dict[str, str] = msgspec.field(default_factory=dict)  # MSBuild wire key is PascalCase; missing block decodes total


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
        # YakPushSource is optional; every other metadata property must be present.
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


def _absolute(root: Path, path: Path) -> Path:
    return path.resolve() if path.is_absolute() else (root / path).resolve()


def _safe_package_pattern(pattern: str) -> bool:
    return bool(pattern) and "/" not in pattern and "\\" not in pattern and "\x00" not in pattern and ".." not in Path(pattern).parts


# --- [TABLES] ---------------------------------------------------------------------------

_DECODER: Final[msgspec.json.Decoder[_MsbuildProps]] = msgspec.json.Decoder(_MsbuildProps)

# Ordered post-stage step policy keyed by verb and rasm-bridge slug match.
_STEP_POLICY: Final[dict[tuple[str, bool], tuple[_LifecycleStep, ...]]] = {
    ("deploy", False): (_LifecycleStep.INSTALL,),
    ("deploy", True): (_LifecycleStep.QUIT, _LifecycleStep.INSTALL, _LifecycleStep.REFRESH),
    ("publish", False): (_LifecycleStep.INSTALL, _LifecycleStep.PUSH),
    ("publish", True): (_LifecycleStep.QUIT, _LifecycleStep.INSTALL, _LifecycleStep.REFRESH, _LifecycleStep.PUSH),
}


# --- [OPERATIONS] -----------------------------------------------------------------------


def _yak_tool(meta: YakMeta, command: _Step, mode: Mode) -> Result[Tool, Fault]:
    # Command carries the complete invocation; Input.NONE contributes only the empty routing tail.
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
    # Minimal C#/CHANGED Routed satisfies Input.NONE; non-zero yak exits stay on Completed.
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
    # Non-zero MSBuild emits plain error text, so decode failure becomes a bounded metadata Fault.
    try:
        return Ok(_DECODER.decode(done.stdout or b"{}").Properties)
    except msgspec.DecodeError:
        tail = (done.stdout or done.stderr or b"").decode(errors="replace").strip()[:512]
        return Error(Fault(("dotnet", "msbuild", project), message=f"msbuild metadata evaluation failed (exit {done.returncode}): {tail}"))


def _resolve_project(settings: AssaySettings, slug: str) -> Result[str, Fault]:
    # Resolve by slug, never by changed set, so package lifecycle cannot target ambiguous projects.
    return _package_projects(settings).bind(lambda projects: _slugged(settings, projects)).bind(lambda pairs: _lone_match(slug, pairs))


def _package_projects(settings: AssaySettings) -> Result[tuple[str, ...], Fault]:
    root = settings.root
    roots = tuple(root / name for name in _PACKAGE_ROOTS if (root / name).is_dir())
    found = tuple(sorted({p.relative_to(root).as_posix() for base in roots for p in base.rglob(f"*{_CSPROJ}")}))
    return Ok(found)


def _slugged(settings: AssaySettings, projects: tuple[str, ...]) -> Result[tuple[tuple[str, str], ...], Fault]:
    # Empty slug means non-yak project; I/O faults still dominate the sequence.
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
    # Read package slug directly from XML; absence means non-yak project.
    return _read_bytes(Path(str(settings.root / project))).map(_slug_from_bytes)


def _read_bytes(path: Path) -> Result[bytes, Fault]:
    try:
        return Ok(path.read_bytes())
    except FileNotFoundError:
        return Ok(b"")
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
    # Drop host-provided assemblies; require manifest and primary .rhp before yak build.
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
    # Rotate to .previous.<pid> before swap so mid-commit crashes remain recoverable.
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
    # Stage under package_dir's parent so the final replace is same-filesystem and lease-scoped.
    resource = f"{_PACKAGE_STAGE}-{meta.package_dir.name}"

    def locked(_held: object) -> Result[Report, Fault]:
        meta.package_dir.parent.mkdir(parents=True, exist_ok=True)
        staged = Path(mkdtemp(prefix=f"{meta.package_dir.name}.", dir=meta.package_dir.parent))
        outcome = _build_outputs(meta, settings, scope).bind(lambda built: _copy_after_build(meta, staged, slug, version, built, settings, scope))
        match outcome:
            case Result(tag="error"):
                rmtree(staged, ignore_errors=True)
                return outcome
            case _:
                return outcome

    return leased(resource, locked, settings=settings, run_id=settings.run_id, project=slug, mode="exclusive")


def _build_outputs(meta: YakMeta, settings: AssaySettings, scope: ArtifactScope) -> Result[Completed, Fault]:
    tool = Tool(
        "dotnet-build",
        Runner.DOTNET,
        ("build", meta.project, "-c", settings.configuration.value, "-v:quiet", "/clp:ErrorsOnly"),
        Input.NONE,
        Language.CSHARP,
        Claim.PACKAGE,
        mode=Mode.BUILD,
    )
    return run_check(
        Check(tool=tool, cwd=Path(str(settings.root))), settings=settings, scope=scope, routed=Routed(language=Language.CSHARP, scope=Scope.CHANGED)
    )


def _copy_after_build(
    meta: YakMeta, staged: Path, slug: str, version: str, built: Completed, settings: AssaySettings, scope: ArtifactScope
) -> Result[Report, Fault]:
    match built.status:
        case RailStatus.FAILED | RailStatus.FAULTED | RailStatus.TIMEOUT:
            rmtree(staged, ignore_errors=True)
            return Ok(fold(Claim.PACKAGE, "stage", (built,)))
        case _:
            return (
                _stage_artifacts(meta, staged)
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
    # install/push are yak steps carrying their own tail; quit/refresh ride the bridge client under bridge.lock.
    # Refresh failure after install is recoverable by bridge launch, so bridge steps ride Completed(FAILED).
    match step:
        case _LifecycleStep.INSTALL:
            return _run_yak(meta, _yak_install_tail(package_file), step.mode, cwd=meta.package_dir, settings=settings, scope=scope)
        case _LifecycleStep.PUSH:
            return _run_yak(meta, _yak_push_tail(meta, package_file), step.mode, cwd=meta.package_dir, settings=settings, scope=scope)
        case _:
            return _bridge_client_run(settings, str(step))


def _resolve_package_file(meta: YakMeta) -> Result[Path, Fault]:
    # Resolve from committed package_dir so install/push never operate on ambiguous staged artifacts.
    matches = sorted(meta.package_dir.glob(meta.package_pattern))
    match matches:
        case [only]:
            return Ok(only)
        case _:
            return Error(Fault(("yak", "install"), message=f"expected one package for pattern {meta.package_pattern}, found {len(matches)}"))


def _finish(settings: AssaySettings, scope: ArtifactScope, meta: YakMeta, slug: str, verb: str, staged: Report) -> Result[Report, Fault]:
    # Stage verb and non-OK stage results short-circuit before post-stage policy.
    match (verb, staged.status):
        case ("stage", _) | (_, RailStatus.FAILED | RailStatus.FAULTED | RailStatus.TIMEOUT | RailStatus.BUSY):
            return Ok(staged)
        case _:
            steps = _STEP_POLICY.get((verb, slug == _RASM_BRIDGE_SLUG), ())
            return _resolve_package_file(meta).bind(lambda package_file: _drive_steps(settings, scope, meta, slug, verb, staged, package_file, steps))


def _drive_steps(
    settings: AssaySettings,
    scope: ArtifactScope,
    meta: YakMeta,
    slug: str,
    verb: str,
    staged: Report,
    package_file: Path,
    steps: tuple[_LifecycleStep, ...],
) -> Result[Report, Fault]:
    # Bridge-bound policy acquires bridge.lock once for quit -> install -> refresh.
    def run_steps(_held: object) -> Result[Report, Fault]:
        return _fold_steps(settings, scope, meta, verb, staged, package_file, steps)

    match any(step.needs_bridge for step in steps):
        case True:
            return leased(_BRIDGE_LOCK, run_steps, settings=settings, run_id=settings.run_id, project=slug, mode="exclusive")
        case False:
            return run_steps(None)


def _fold_steps(
    settings: AssaySettings, scope: ArtifactScope, meta: YakMeta, verb: str, staged: Report, package_file: Path, steps: tuple[_LifecycleStep, ...]
) -> Result[Report, Fault]:
    # Result accumulator appends Completed rows and short-circuits only on spawn or lease Faults; stage build evidence survives the merge.
    seed: Result[tuple[Completed, ...], Fault] = Ok(())
    folded = reduce(
        lambda acc, step: acc.bind(lambda done: _run_step(settings, scope, meta, package_file, step).map(lambda c: (*done, c))), steps, seed
    )
    return folded.map(lambda outcomes: _merge_stage(staged, fold(Claim.PACKAGE, verb, outcomes, detail=staged.detail)))


def _merge_stage(staged: Report, steps: Report) -> Report:
    # Stage results/counts/artifacts/notes ride ahead of step rows so build evidence and post-stage steps both survive one report.
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
    # Resolve and validate before leases, then stage and apply verb-specific post-stage steps.

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


# --- [COMPOSITION] ----------------------------------------------------------------------


def stage(settings: AssaySettings, scope: ArtifactScope, params: PackageParams) -> Result[Report, Fault]:
    """Stage one yak distribution.

    Returns:
        Package lifecycle report, or staging fault.
    """
    return _lifecycle(settings, scope, params, "stage")


def deploy(settings: AssaySettings, scope: ArtifactScope, params: PackageParams) -> Result[Report, Fault]:
    """Stage and install one yak distribution.

    Returns:
        Package lifecycle report, or staging/install fault.
    """
    return _lifecycle(settings, scope, params, "deploy")


def publish(settings: AssaySettings, scope: ArtifactScope, params: PackageParams) -> Result[Report, Fault]:
    """Stage, install, and publish one yak distribution.

    Returns:
        Package lifecycle report, or staging/install/publish fault.
    """
    return _lifecycle(settings, scope, params, "publish")


def list(settings: AssaySettings, scope: ArtifactScope, params: PackageParams) -> Result[Report, Fault]:  # noqa: A001  # registry binds the canonical verb name "list"
    """List package projects and slugs.

    Returns:
        Package roster report, or project discovery fault.
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
        Package plan report, or metadata evaluation fault.
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
