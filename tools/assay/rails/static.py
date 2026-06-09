"""Static analysis rails: format-fix, diagnostic-check, build-compile, full-parity, and routing-plan.

Each public function routes input paths through language detection and project-closure
resolution, fans selected tools via leased exclusive execution, and folds results into
a typed Report. Build-mode rails acquire a stable per-closure artifact scope so
incremental MSBuild cache keys remain consistent across invocations.
"""

from dataclasses import dataclass, replace
from functools import reduce
from hashlib import sha256
from pathlib import PurePosixPath
from typing import TYPE_CHECKING

from expression import Error, Ok, Result
from expression.collections import block
from expression.extra.result import sequence
import msgspec

# beartype resolves TC001-suppressed imports at runtime via PEP 649 forward-ref evaluation — keep them unconditional.
from tools.assay.composition.catalog import select
from tools.assay.composition.settings import ArtifactScope, AssaySettings, Configuration  # noqa: TC001 — beartype runtime annotation consumer
from tools.assay.core.engine import argv_for, fan_out, leased
from tools.assay.core.model import (
    Artifact,
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Counts,
    Fault,  # noqa: TC001 — beartype runtime annotation consumer
    fold,
    Language,
    Match,
    Mode,
    Report,  # noqa: TC001 — beartype runtime annotation consumer
)
from tools.assay.core.routing import route, Routed  # noqa: TC001 — beartype runtime annotation consumer
from tools.assay.core.status import join, RailStatus


if TYPE_CHECKING:
    from expression.collections import Block

    from tools.assay.core.model import Completed


# --- [CONSTANTS] ------------------------------------------------------------------------

_PREVIEW_MODES: tuple[tuple[str, Mode], ...] = (("fix", Mode.WRITE), ("report", Mode.CHECK), ("build", Mode.BUILD))


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class StaticParams(BaseParams):
    """Shared parameter surface for all static rails."""


# --- [OPERATIONS] -----------------------------------------------------------------------


def _closure_sha(routed: Routed) -> str:
    return sha256("\n".join(routed.projects).encode()).hexdigest()[:16]


def _route_sha(routed: Routed) -> str:
    seed = "\n".join((*routed.projects, *routed.files, *routed.full_triggers)) or routed.language.name
    return sha256(seed.encode()).hexdigest()[:16]


def _languages(selected: Language | None, paths: tuple[str, ...]) -> tuple[Language, ...]:
    match selected:
        case Language() as language:
            return (language,)
        case None:
            suffixes = frozenset(PurePosixPath(p).suffix for p in paths if PurePosixPath(p).suffix)
            inferred = tuple(language for language in Language if suffixes & language.suffixes)
            return inferred or tuple(Language)


def _mode_family(mode: Mode) -> tuple[Mode, ...]:
    return (Mode.RESTORE, Mode.BUILD) if mode is Mode.BUILD else (mode,)


def _checks(routed: Routed, mode: Mode) -> tuple[Check, ...]:
    modes = _mode_family(mode)
    return tuple(Check(tool=t, paths=routed.files) for active in modes for t in select(Claim.STATIC, routed.language) if t.mode is active)


def _routed(languages: tuple[Language, ...], paths: tuple[str, ...], settings: AssaySettings) -> Result[Block[Routed], Fault]:
    return sequence(block.of_seq(route(language, paths, settings=settings) for language in languages))


def _empty_route(routed: Routed) -> bool:
    return (routed.language.strategy == "glob" and not routed.files) or (
        routed.language.strategy == "closure" and not routed.files and not routed.projects
    )


def _build_fan(checks: tuple[Check, ...], routed: Routed, settings: AssaySettings) -> tuple[Result[Completed, Fault], ...]:
    closure = _closure_sha(routed)
    stable_scope = ArtifactScope.build(settings, closure)
    outcome = leased(
        f"build-{closure}-{settings.configuration.value}",
        lambda _held: Ok(fan_out(checks, settings=settings, scope=stable_scope, routed=routed)),
        settings=settings,
        run_id=settings.run_id,
        project=f"{settings.configuration.value}:{','.join(routed.projects)}",
        mode="exclusive",
    )
    match outcome:
        case Result(tag="ok", ok=rows):
            return rows
        case Result(error=fault):
            return (Error(fault),)


def _write_fan(checks: tuple[Check, ...], routed: Routed, settings: AssaySettings, scope: ArtifactScope) -> tuple[Result[Completed, Fault], ...]:
    route_id = _route_sha(routed)
    outcome = leased(
        f"write-{routed.language.value}-{route_id}",
        lambda _held: Ok(fan_out(checks, settings=settings, scope=scope, routed=routed)),
        settings=settings,
        run_id=settings.run_id,
        project=",".join((*routed.projects, *routed.files)),
        mode="exclusive",
    )
    match outcome:
        case Result(tag="ok", ok=rows):
            return rows
        case Result(error=fault):
            return (Error(fault),)


def _dispatch(routed: Routed, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode) -> tuple[Result[Completed, Fault], ...]:
    if _empty_route(routed):
        return ()
    checks = _checks(routed, mode)
    match checks:
        case ():
            return ()
        case _ if mode is Mode.BUILD and routed.projects:
            return _build_fan(checks, routed, settings)
        case _ if mode.writes:
            return _write_fan(checks, routed, settings, scope)
        case _:
            return fan_out(checks, settings=settings, scope=scope, routed=routed)


def _thin_rail(settings: AssaySettings, scope: ArtifactScope, params: StaticParams, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    return _routed(_languages(params.language, params.paths), params.paths, settings).bind(
        lambda routed: sequence(routed.collect(lambda r: block.of_seq(_dispatch(r, settings=settings, scope=scope, mode=mode)))).map(
            lambda done: fold(claim, verb, tuple(done))
        )
    )


def _combine_full(reports: tuple[Report, ...]) -> Report:
    status = reduce(join, (report.status for report in reports), RailStatus.EMPTY)
    counts = Counts(
        ok=sum(report.counts.ok for report in reports),
        failed=sum(report.counts.failed for report in reports),
        total=sum(report.counts.total for report in reports),
    )
    base = fold(Claim.STATIC, "full", ())
    return msgspec.structs.replace(
        base,
        status=status,
        counts=counts,
        results=tuple(row for report in reports for row in report.results),
        artifacts=tuple(artifact for report in reports for artifact in report.artifacts),
        notes=tuple(note for report in reports for note in report.notes),
    )


def _preview(routed: Routed, verb: str, mode: Mode, settings: AssaySettings, scope: ArtifactScope) -> str:
    # Use the stable build scope so the previewed argv matches the real leased path.
    active_scope = ArtifactScope.build(settings, _closure_sha(routed)) if mode is Mode.BUILD and routed.projects else scope
    argvs = tuple(
        argv_for(Check(tool=tool), routed, settings=settings, scope=active_scope)
        for active in _mode_family(mode)
        for tool in select(Claim.STATIC, routed.language)
        if tool.mode is active
    )
    bodies = " ; ".join(" ".join(argv) for argv in argvs)
    return f"{routed.language!s} {verb}: {bodies}" if bodies else ""


def _plan_report(routed: tuple[Routed, ...], settings: AssaySettings, scope: ArtifactScope) -> Report:
    # Compute SHA once per language so rows, notes, and artifacts share the same stable key.
    routed_sha = tuple((r, _closure_sha(r) if r.projects else "") for r in routed)
    rows = tuple(
        Match(
            id=str(r.language),
            kind=ArtifactKind.SCOPE,
            text=(
                f"scope={r.scope!s} files={len(r.files)} projects={len(r.projects)} sha={sha} triggers={len(r.full_triggers)} groups={len(r.groups)}"
            ),
        )
        for r, sha in routed_sha
    )
    previews = tuple(
        preview for r, _ in routed_sha for verb, mode in _PREVIEW_MODES for preview in (_preview(r, verb, mode, settings, scope),) if preview
    )
    notes = (
        *(
            f"{r.language!s}: scope={r.scope!s} closure={sha} projects={len(r.projects)}"
            if r.projects
            else f"{r.language!s}: scope={r.scope!s} files={len(r.files)}"
            for r, sha in routed_sha
        ),
        *previews,
    )
    artifacts = tuple(
        Artifact(id=f"build-{sha}", kind=ArtifactKind.SCOPE, path=ArtifactScope.build(settings, sha).path) for r, sha in routed_sha if r.projects
    )
    base = fold(Claim.STATIC, "plan", ())
    status = RailStatus.OK if any(r.files or r.projects for r, _ in routed_sha) else base.status
    return msgspec.structs.replace(base, status=status, results=rows, artifacts=artifacts, notes=notes)


# --- [COMPOSITION] ----------------------------------------------------------------------


def fix(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Apply mutating static fixers (whitespace, style, analyzer auto-corrections).

    Returns:
        Folded fix report on success; routing or spawn Fault on error.
    """
    return _thin_rail(settings, scope, params, claim=Claim.STATIC, verb="fix", mode=Mode.WRITE)


def report(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Emit static diagnostics without mutating source files.

    Returns:
        Folded diagnostic report on success; routing or spawn Fault on error.
    """
    return _thin_rail(settings, scope, params, claim=Claim.STATIC, verb="report", mode=Mode.CHECK)


def build(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Restore and compile the project closure, running MSBuild analyzers for compile-time proof.

    Returns:
        Folded build report on success; routing or spawn Fault on error.
    """
    return _thin_rail(settings, scope, params, claim=Claim.STATIC, verb="build", mode=Mode.BUILD)


def full(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Build DEBUG and RELEASE configurations against Workspace.slnx for full solution parity.

    Returns:
        Combined report across both configurations on success; routing or spawn Fault on error.
    """
    routed_params = replace(params, paths=params.paths or ("Workspace.slnx",))
    return sequence(
        block.of_seq(
            _thin_rail(settings.with_configuration(configuration), scope, routed_params, claim=Claim.STATIC, verb="full", mode=Mode.BUILD)
            for configuration in (Configuration.DEBUG, Configuration.RELEASE)
        )
    ).map(lambda b: _combine_full(tuple(b)))


def plan(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Resolve routing for all applicable languages and emit scope, file, and argv previews without spawning tools.

    Returns:
        Routing plan report on success; routing Fault on error.
    """
    return _routed(_languages(params.language, params.paths), params.paths, settings).map(lambda routed: _plan_report(tuple(routed), settings, scope))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__: list[str] = ["StaticParams", "build", "fix", "full", "plan", "report"]
