"""The ``Claim.PACKAGE`` yak lifecycle rail: ``stage``/``deploy``/``publish``/``list``/``plan`` of ``.yak`` distributions.

Resolves a project by ``YakPackageSlug``, evaluates MSBuild yak metadata, stages under a per-dir lease (keyed on the package
directory, never the slug — two slugs sharing one ``YakPackageDirectory`` serialize), runs ``yak build``/``install``/``push``
as catalog ``Tool`` rows, and commits the staged tree atomically (temp → rotate → swap, restore-on-``OSError``).

Invariants: the ``rasm-bridge`` slug is the lone lifecycle special-case — its ``deploy``/``publish`` brackets the steps with
``quit``/``refresh`` inside the shared ``bridge.lock`` so host-quit, install, and plugin-refresh are atomic under one lease.
A slug/``.rhp``/platform mismatch or non-executable ``YakPath`` faults as ``FAULTED`` *before* the lease; a ``yak build``
defect rides the success channel as ``Completed(FAILED)``. Counts derive solely in ``model.fold`` — this rail never sums.
"""

from dataclasses import dataclass
import fnmatch
from functools import reduce
from itertools import chain
import os
from pathlib import Path
from shutil import copy2, rmtree
from tempfile import mkdtemp
from typing import Final, TYPE_CHECKING

from expression import Error, Ok, Result
import msgspec

from tools.assay._TMP.composition.catalog import select  # noqa: PLC2701  # intra-staging import; _TMP is the package root
from tools.assay._TMP.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay._TMP.core.engine import leased, run_check  # noqa: PLC2701  # intra-staging import; _TMP is the package root
from tools.assay._TMP.core.model import (  # noqa: PLC2701  # intra-staging import; _TMP is the package root
    Base,
    BaseParams,
    Check,
    Claim,
    Completed,
    Fault,
    fold,
    Input,
    Language,
    Mode,
    PackageRun,
    Report,  # noqa: TC001  # unconditional so beartype @checked resolves the -> Result[Report, Fault] forward-ref under PEP 649
    Runner,
    Tool,
)
from tools.assay._TMP.core.routing import Routed, Scope  # noqa: PLC2701  # intra-staging import; _TMP is the package root
from tools.assay._TMP.core.status import RailStatus  # noqa: PLC2701  # intra-staging import; _TMP is the package root

# canonical Claim.BRIDGE Mode.CLIENT seam for the rasm-bridge quit/refresh lifecycle steps
from tools.assay._TMP.rails.bridge import _client_run as _bridge_client_run  # noqa: PLC2701  # intra-staging import; _TMP is the package root


if TYPE_CHECKING:
    from collections.abc import Callable, Iterable


# --- [TYPES] ----------------------------------------------------------------------------

type _Step = tuple[str, ...]  # one resolved yak/bridge argv tail (the command minus the launcher head)


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class PackageParams(BaseParams):
    """Per-verb params: ``slug`` keys ``YakPackageSlug`` resolution, ``version`` stamps ``YakVersion`` and ``yak build --version``.

    ``stage``/``deploy``/``publish``/``plan`` require a ``slug``; ``list`` ignores both.
    """

    slug: str = ""
    version: str = ""


class _MsbuildProps(Base, frozen=True):
    """The ``dotnet msbuild -getProperty:… -nologo`` JSON envelope: a ``Properties`` dict of evaluated yak metadata.

    The one-pass ``msgspec`` decode validates the envelope shape and ``YakMeta.from_props`` projects the string map onto
    the typed carrier — never a hand-rolled ``json.loads`` walk.
    """

    Properties: dict[str, str] = {}  # noqa: RUF012  # MSBuild wire key is PascalCase; default-empty so a missing block decodes total


class YakMeta(Base, frozen=True, gc=False):
    """The validated yak-distribution carrier: a typed ``Path``/``str`` projection of the MSBuild ``Properties`` map.

    A regular typed ``Base`` struct rather than a ``detail_type`` defstruct: it carries behavior (``validate``) and
    ``Path`` fields the wire never sees, so it is internal evidence, not a ``Detail`` variant. Validation runs inside
    ``evaluate_meta`` *before* the stage lease so a misconfigured project fails fast at exit 2 without ever stealing the
    lease or rotating a live package dir.
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
        """Project the MSBuild ``Properties`` map onto the typed carrier, faulting on any required-property gap.

        ``YakPushSource`` is optional (a server push target); every other ``_META_PROPS`` member must be present and
        non-empty. A gap carries the missing property names so the operator sees exactly which property the ``.csproj``
        failed to evaluate.
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
        """Affirm every staging precondition before the lease: slug/``.rhp``/output-containment/platform/glob/executable.

        The first failing ``(predicate, message)`` row short-circuits to ``Fault(FAULTED)`` (exit 2) so a misconfigured
        project never steals the per-dir stage lease nor rotates a live package directory. The target-dir containment
        guard refuses to clean an output directory outside the worktree's ``bin/<config>/<tfm>``, and the executable-bit
        probe (``os.access(X_OK)``) keeps a non-runnable ``YakPath`` a precondition fault rather than a ``yak build``
        spawn ``OSError``.
        """
        root = Path(str(settings.root)).resolve()  # output-dir containment guard is local-fs; UPath → Path
        expected = (self.project_dir / "bin" / settings.configuration.value / self.target_framework).resolve()
        resolved = self.target_dir.resolve()
        checks: tuple[tuple[bool, str], ...] = (
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


def _yak_tool(meta: YakMeta, command: _Step, mode: Mode) -> Tool:
    """Build one ``DIRECT`` yak ``Tool`` whose ``command`` head is the resolved ``YakPath`` and tail is the full step argv.

    Yak verbs are ``Tool`` rows, never inline argv builders: the ``command`` carries the complete invocation so the
    engine's ``Input.NONE`` ``place`` projection appends one empty tail and the spawned argv is exactly ``command``.
    """
    base = select(Claim.PACKAGE, None)  # the catalog yak row asserts Claim.PACKAGE owns the program; DIRECT/NONE axes ride through
    return msgspec.structs.replace(base[0], command=(str(meta.yak_path), *command), mode=mode)


def _yak_build_tail(meta: YakMeta, version: str) -> _Step:
    """The ``yak build --platform <plat> --version <ver>`` tail, run with ``cwd=stage``."""
    return ("build", "--platform", meta.yak_platform, "--version", version)


def _yak_install_tail(package_file: Path) -> _Step:
    """The ``yak install <package_file>`` tail, run with ``cwd=package_dir``."""
    return ("install", str(package_file))


def _yak_push_tail(meta: YakMeta, package_file: Path) -> _Step:
    """The ``yak push --source <push_source>? <package_file>`` tail; ``--source`` rides only when a push source is declared."""
    source = ("--source", meta.yak_push_source) if meta.yak_push_source else ()
    return ("push", *source, str(package_file))


def _run_yak(meta: YakMeta, command: _Step, mode: Mode, *, cwd: Path, settings: AssaySettings, scope: ArtifactScope) -> Result[Completed, Fault]:
    """Run one yak step as a ``Check`` through the engine: an ``Input.NONE`` ``DIRECT`` spawn under ``cwd``.

    The ``Routed`` is the minimal C#/``CHANGED`` shape ``place`` needs for an ``Input.NONE`` tool (one empty tail); the
    full yak argv lives in ``_yak_tool``'s ``command``. A non-zero exit rides ``Ok(Completed(FAILED))``; only a
    spawn/timeout failure takes the ``Error(Fault)`` channel at the engine boundary.
    """
    check = Check(tool=_yak_tool(meta, command, mode), cwd=cwd)
    routed = Routed(language=check.tool.language, scope=Scope.CHANGED)
    return run_check(check, settings=settings, scope=scope, routed=routed)


def evaluate_meta(settings: AssaySettings, scope: ArtifactScope, project: str, slug: str, version: str) -> Result[YakMeta, Fault]:
    """Evaluate and validate the yak metadata under one ``dotnet msbuild`` ``Mode.QUERY`` row (fail-fast before any lease).

    Validation precedes any lease or stage so a slug/``.rhp``/platform/glob/executable mismatch faults at exit 2 before a
    live package directory is touched. A non-zero MSBuild exit emits an error line (not the JSON envelope) on ``stdout``;
    ``_decode_props`` is the marked codec boundary that projects that malformed output to a contextful ``Fault`` carrying
    the MSBuild tail, rather than letting a raw ``msgspec.DecodeError`` escape mid-rail.
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
    """Decode the MSBuild ``-getProperty`` JSON envelope at the codec boundary; non-JSON output → a contextful ``Fault``.

    A non-zero MSBuild exit emits an error line (``MSB####``/restore noise) on ``stdout``, never the JSON envelope, so a
    bare ``_DECODER.decode`` raises ``msgspec.DecodeError`` mid-rail. The try/except is the marked codec boundary: a
    malformed envelope surfaces as ``Fault(("dotnet","msbuild",project))`` carrying the bounded MSBuild tail so the
    operator sees *what* failed, never a context-free ``JSON is malformed`` swallowed at a distant seam.
    """
    try:
        return Ok(_DECODER.decode(done.stdout or b"{}").Properties)
    except msgspec.DecodeError:
        tail = (done.stdout or done.stderr or b"").decode(errors="replace").strip()[:512]
        return Error(Fault(("dotnet", "msbuild", project), message=f"msbuild metadata evaluation failed (exit {done.returncode}): {tail}"))


def _resolve_project(settings: AssaySettings, slug: str) -> Result[str, Fault]:
    """Resolve the single ``*.csproj`` whose ``YakPackageSlug`` equals ``slug`` — by slug, never by changed-set.

    Zero matches or a duplicate slug is a ``Fault(FAULTED)`` so the lifecycle never operates on an ambiguous target.
    """
    return _package_projects(settings).bind(lambda projects: _select_by_slug(settings, slug, projects))


def _package_projects(settings: AssaySettings) -> Result[tuple[str, ...], Fault]:
    """Discover every ``*.csproj`` under the ``apps``/``tools`` roots as sorted root-relative POSIX rows."""
    root = settings.root
    roots = tuple(root / name for name in _PACKAGE_ROOTS if (root / name).is_dir())
    found = tuple(sorted({p.relative_to(root).as_posix() for base in roots for p in base.rglob(f"*{_CSPROJ}")}))
    return Ok(found)


def _select_by_slug(settings: AssaySettings, slug: str, projects: tuple[str, ...]) -> Result[str, Fault]:
    """Fold the project set to the lone ``YakPackageSlug == slug`` match; zero or duplicate matches fault."""
    matched = tuple(p for p in projects if _csproj_slug(settings, p) == slug)
    match matched:
        case (only,):
            return Ok(only)
        case ():
            return Error(Fault(("package", slug), message=f"expected one package project for {slug}, found 0"))
        case _:
            return Error(Fault(("package", slug), message=f"expected one package project for {slug}, found {len(matched)} duplicates"))


def _csproj_slug(settings: AssaySettings, project: str) -> str:
    """Read one project's declared ``YakPackageSlug`` from its ``.csproj`` XML via a lightweight element walk.

    A direct XML read, never a full MSBuild evaluation; absence returns ``""`` so a non-yak project drops out of the
    slug match.
    """
    return _slug_from_bytes(_read_bytes(Path(str(settings.root / project))))  # .csproj XML read is local; UPath → Path


def _read_bytes(path: Path) -> bytes:
    """Read project bytes at the marked filesystem boundary; an ``OSError`` degrades to empty bytes rather than raising."""
    try:
        return path.read_bytes()
    except OSError:
        return b""


def _slug_from_bytes(raw: bytes) -> str:
    """Extract ``<YakPackageSlug>…</YakPackageSlug>`` text from project XML; absence or malformed XML yields ``""``."""
    import xml.etree.ElementTree as ET  # noqa: PLC0415, S405  # trusted local .csproj XML, parsed at the slug-read boundary only

    try:
        tree = ET.fromstring(raw or b"<Project/>")  # noqa: S314  # trusted local .csproj XML, never network-sourced
    except ET.ParseError:
        return ""
    found = next((el.text for el in tree.iter() if el.tag.rpartition("}")[2] == _YAK_SLUG_PROP and el.text), None)
    return (found or "").strip()


def _stage_artifacts(meta: YakMeta, staged: Path) -> Result[Path, Fault]:
    """Copy the yak manifest plus the non-host ``.dll``/``.json``/``.rhp`` artifacts into the staged temp dir.

    The ``_HOST_EXCLUDES`` globs drop the Rhino/Eto/GH host assemblies the running host already provides. A missing
    ``manifest.yml`` or primary ``.rhp`` is a precondition ``Fault`` so an incomplete distribution never reaches
    ``yak build``; an ``OSError`` mid-copy is a stage ``Fault`` the caller rolls back.
    """
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
    """Execute the ``copy2`` fold over the manifest + filtered target artifacts at the marked filesystem boundary."""
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
    """Atomically rotate the package dir and swap in the staged tree: temp → ``.previous.<pid>`` → swap, restore-on-error.

    The rotate to ``.previous.<pid>`` before the swap makes a crash mid-rotate recoverable; the per-dir stage lease
    (keyed on ``package_dir``, not ``slug``) is the actual serializer when two projects publish into one directory. An
    ``OSError`` at any rotate/swap step restores the rotated-away previous tree (only when the live dir is now absent) and
    discards the staged temp, surfacing a ``Fault(FAULTED)``.
    """
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
    """Take the per-dir stage lease, run the ``yak build`` ``Check``, and commit atomically.

    The staged temp dir is allocated under the package dir's parent so the final ``replace`` is a same-filesystem atomic
    rename. The lease resource is derived from ``package_dir`` so two slugs sharing one ``YakPackageDirectory`` parent
    serialize correctly; a held live lease short-circuits to ``Fault(BUSY)`` (exit 5) without waiting. A ``yak build``
    defect rides ``Ok(Completed(FAILED))`` and folds to a ``FAILED`` ``Report`` without committing; a clean build commits
    the staged tree and stamps the ``version`` onto the ``PackageRun`` detail.
    """
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
    """Branch the staged build outcome: a ``FAILED`` yak build folds to a defect ``Report``; an ``OK`` build commits."""
    match done.status:
        case RailStatus.FAILED | RailStatus.FAULTED | RailStatus.TIMEOUT:
            rmtree(staged, ignore_errors=True)
            return Ok(fold(Claim.PACKAGE, "stage", (done,)))
        case _:
            return _commit(meta, staged, slug).map(lambda report: msgspec.structs.replace(report, detail=_stamp_version(report.detail, version)))


def _stamp_version(detail: object, version: str) -> PackageRun:
    """Stamp the staged ``version`` onto the committed ``PackageRun`` (the commit fold leaves ``version=""``)."""
    match detail:
        case PackageRun() as run:
            return msgspec.structs.replace(run, version=version)
        case _:
            return PackageRun(version=version)


def _run_step(settings: AssaySettings, scope: ArtifactScope, meta: YakMeta, package_file: Path, kind: str) -> Result[Completed, Fault]:
    """Run one post-stage lifecycle step: ``install``/``push`` are yak rows; ``quit``/``refresh`` are bridge client rows.

    ``install``/``push`` run as ``DIRECT`` yak ``Check`` rows with ``cwd=package_dir``; ``quit``/``refresh`` drive the
    live host through the bridge client (``Mode.CLIENT``) under the already-held ``bridge.lock``. Any non-zero exit rides
    ``Ok(Completed(FAILED))``.
    """
    match kind:
        case "install":
            return _run_yak(meta, _yak_install_tail(package_file), Mode.DEPLOY, cwd=meta.package_dir, settings=settings, scope=scope)
        case "push":
            return _run_yak(meta, _yak_push_tail(meta, package_file), Mode.PUBLISH, cwd=meta.package_dir, settings=settings, scope=scope)
        case _:
            return _run_bridge_client(settings, scope, kind)


def _run_bridge_client(settings: AssaySettings, scope: ArtifactScope, verb: str) -> Result[Completed, Fault]:
    """Drive one ``rasm-bridge`` ``Mode.CLIENT`` lifecycle verb (``quit``/``refresh``) through the canonical bridge seam.

    ``quit`` tears down the running host before the ``.rhp`` swap and ``refresh`` reloads the live plugin after install,
    the whole sequence under the shared ``bridge.lock`` so it is atomic under one lease. The invocation routes through
    ``rails.bridge._client_run`` — the single ``dotnet run --no-build --project <client> --configuration <conf> -- <verb>``
    surface — so the client project is resolved by ``--project`` (a bare ``dotnet run`` from the repo root finds no
    project) and the client stays on its canonical ``bin/`` output (no ``--artifacts-path`` splice). A ``refresh`` fault
    after ``install`` leaves the new ``.rhp`` on disk while the host is down — recoverable by a bare ``bridge launch``, so
    it rides ``Completed(FAILED)`` rather than a rollback.
    """
    _ = scope  # the bridge client must stay on its canonical bin/ output; _client_run pins scope=None internally
    return _bridge_client_run(settings, verb)


def _resolve_package_file(meta: YakMeta) -> Result[Path, Fault]:
    """Resolve the single committed ``.yak`` file matching the validated pattern (a deploy/publish precondition).

    Zero or many matches is a ``Fault`` so ``install``/``push`` never operate on an ambiguous artifact. The glob runs
    against the committed ``package_dir`` (post-swap), not the staged temp, so the resolved path is the live distribution.
    """
    matches = sorted(meta.package_dir.glob(meta.package_pattern))
    match matches:
        case [only]:
            return Ok(only)
        case _:
            return Error(Fault(("yak", "install"), message=f"expected one package for pattern {meta.package_pattern}, found {len(matches)}"))


def _finish(settings: AssaySettings, scope: ArtifactScope, meta: YakMeta, slug: str, verb: str, staged: Report) -> Result[Report, Fault]:
    """Fold the post-stage step policy keyed by ``(verb, slug == rasm-bridge)``: deploy installs, publish pushes.

    A ``stage`` verb (or a non-``OK`` stage) short-circuits to the staged ``Report`` unchanged. Otherwise the resolved
    package file feeds each policy step in order, and the step outcomes fold into the stage ``Report``'s status via
    ``model.fold`` while preserving the ``PackageRun`` detail.
    """
    match (verb, staged.status):
        case ("stage", _) | (_, RailStatus.FAILED | RailStatus.FAULTED | RailStatus.TIMEOUT | RailStatus.BUSY):
            return Ok(staged)
        case _:
            steps = _STEP_POLICY.get((verb, slug == _RASM_BRIDGE_SLUG), ())
            return _resolve_package_file(meta).bind(lambda package_file: _drive_steps(settings, scope, meta, slug, verb, staged, package_file, steps))


def _drive_steps(
    settings: AssaySettings, scope: ArtifactScope, meta: YakMeta, slug: str, verb: str, staged: Report, package_file: Path, steps: tuple[str, ...]
) -> Result[Report, Fault]:
    """Run the policy steps (under the bridge lease when the policy includes ``quit``/``refresh``) and fold one ``Report``.

    The bridge-bound policy (``rasm-bridge`` deploy/publish) acquires the global ``bridge.lock`` once and runs the whole
    step sequence inside it so ``quit → install → refresh`` is atomic under one lease; a non-bridge policy runs the yak
    steps directly. Either way the step ``Completed`` outcomes fold with the staged build outcome into one ``Report``
    whose status is the max-by-severity join and whose ``PackageRun`` detail is carried from the stage commit.
    """
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
    """Fold the ordered steps into one ``Result``, threading each ``Completed`` and short-circuiting on a spawn ``Fault``.

    A ``reduce`` threads a ``Result[tuple[Completed, ...], Fault]`` accumulator: each step appends its ``Completed`` on
    success (including a ``Completed(FAILED)`` defect) and short-circuits on a spawn/lease ``Fault``. The terminal fold
    projects the staged build receipt plus every step receipt into one ``Report`` keyed by ``verb`` with the stage's
    ``PackageRun`` detail, so ``counts``/``status`` derive once in ``model.fold``.
    """
    seed: Result[tuple[Completed, ...], Fault] = Ok(())
    folded = reduce(
        lambda acc, kind: acc.bind(lambda done: _run_step(settings, scope, meta, package_file, kind).map(lambda c: (*done, c))), steps, seed
    )
    return folded.map(lambda outcomes: fold(Claim.PACKAGE, verb, outcomes, detail=staged.detail))


def _lifecycle(settings: AssaySettings, scope: ArtifactScope, params: PackageParams, verb: str) -> Result[Report, Fault]:
    """The shared resolve → evaluate → stage → finish fold for ``stage``/``deploy``/``publish``.

    Evaluates+validates the yak metadata *before* any lease, stages under the per-dir lease, then folds the
    ``(verb, slug)`` step policy. ``stage`` finishes at the committed ``PackageRun`` ``Report``; ``deploy``/``publish``
    continue into the install/push (and bridge quit/refresh) steps.
    """

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
    """``package stage``: build + atomically commit one yak distribution under the per-dir stage lease.

    Validates the yak metadata at exit 2 before the lease, then copies manifest + non-host artifacts, runs ``yak build``,
    and rotates the package dir atomically into a ``PackageRun`` ``Report``. A ``yak build`` defect rides a ``FAILED``
    ``Report``, a held lease rides ``Fault(BUSY)`` (exit 5).
    """
    return _lifecycle(settings, scope, params, "stage")


def deploy(settings: AssaySettings, scope: ArtifactScope, params: PackageParams) -> Result[Report, Fault]:
    """``package deploy``: ``stage`` then ``yak install`` the committed ``.yak`` into the live host.

    The ``rasm-bridge`` slug runs ``quit → install → refresh`` inside the shared ``bridge.lock`` so the host quits, the
    package installs, and the live plugin refreshes atomically under one lease; every other slug runs a bare ``install``.
    The step outcomes fold into the staged ``Report`` carrying the ``PackageRun`` detail.
    """
    return _lifecycle(settings, scope, params, "deploy")


def publish(settings: AssaySettings, scope: ArtifactScope, params: PackageParams) -> Result[Report, Fault]:
    """``package publish``: ``stage`` then ``yak install`` + ``yak push`` to the configured server source.

    The ``rasm-bridge`` slug prepends ``quit`` and appends ``refresh`` around ``install`` before the ``push``; the
    ``push`` carries ``--source`` only when the project declares a ``YakPushSource``. The folded ``Report`` carries the
    committed ``PackageRun`` detail and the max-by-severity status of every step.
    """
    return _lifecycle(settings, scope, params, "publish")


def list(settings: AssaySettings, scope: ArtifactScope, params: PackageParams) -> Result[Report, Fault]:  # noqa: A001  # registry binds the canonical verb name "list"
    """``package list``: a zero-side-effect read folding every ``(slug, project)`` pair into ``Report.notes``.

    Short-circuits before any lease, MSBuild, or yak invocation. ``params`` is unused — ``list`` enumerates the whole
    package set, not one slug. No ``PackageRun`` detail.
    """
    _ = (scope, params)
    return _package_projects(settings).map(
        lambda projects: msgspec.structs.replace(
            fold(Claim.PACKAGE, "list", ()),
            status=RailStatus.OK,
            notes=tuple(f"{slug}={project}" for project in projects for slug in (_csproj_slug(settings, project),) if slug),
        )
    )


def plan(settings: AssaySettings, scope: ArtifactScope, params: PackageParams) -> Result[Report, Fault]:
    """``package plan``: a zero-side-effect read emitting the evaluated ``YakMeta`` into ``notes`` with an empty ``PackageRun``.

    Runs the same fail-fast resolve+validate path ``stage`` takes but takes no lease, runs no ``yak build``, and rotates
    no package dir, so the operator previews the resolved distribution shape (``stage=""``) before committing.
    """
    return (
        _resolve_project(settings, params.slug)
        .bind(lambda project: evaluate_meta(settings, scope, project, params.slug, params.version))
        .map(lambda meta: _plan_report(meta, params.version))
    )


def _plan_report(meta: YakMeta, version: str) -> Report:
    """Project the evaluated ``YakMeta`` into a ``plan`` ``Report``: meta rows in ``notes``, empty ``PackageRun`` detail."""
    notes = (
        f"project={meta.project}",
        f"package_dir={meta.package_dir}",
        f"manifest_dir={meta.manifest_dir}",
        f"target_dir={meta.target_dir}",
        f"pattern={meta.package_pattern}",
        f"platform={meta.yak_platform}",
        f"push_source={meta.yak_push_source}",
        f"yak_path={meta.yak_path}",
    )
    return msgspec.structs.replace(
        fold(Claim.PACKAGE, "plan", ()),
        status=RailStatus.OK,
        notes=notes,
        detail=PackageRun(project=meta.project, pattern=meta.package_pattern, version=version),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PackageParams", "YakMeta", "deploy", "evaluate_meta", "list", "plan", "publish", "stage"]
