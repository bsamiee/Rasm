"""Run static analysis, formatting, build, and routing-plan rails."""

from dataclasses import dataclass, replace
from hashlib import sha256
from pathlib import PurePosixPath
from typing import TYPE_CHECKING

from expression import Result  # noqa: TC002  # unconditional: beartype @checked resolves the handler forward-ref (PEP 649)
from expression.collections import block
from expression.extra.result import sequence
import msgspec

from tools.assay.composition.catalog import select
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay.core.engine import _argv, fan_out  # noqa: PLC2701  # plan preview must share the engine argv projection
from tools.assay.core.model import (
    Artifact,
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Fault,  # noqa: TC001  # unconditional: @checked's beartype resolves the -> Result[Report, Fault] forward-ref under PEP 649
    fold,
    Language,
    Match,
    Mode,
    Report,  # noqa: TC001  # unconditional: see Fault above (same forward-ref resolution)
)
from tools.assay.core.routing import (  # noqa: TC001  # unconditional: function signatures reference Routed at definition time; intra-package import
    route,
    Routed,
)
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from expression.collections import Block

    from tools.assay.core.model import Completed


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class StaticParams(BaseParams):
    """Parameters shared by static rails."""


# --- [OPERATIONS] -----------------------------------------------------------------------


def _languages(selected: Language | None, paths: tuple[str, ...]) -> tuple[Language, ...]:
    match selected:
        case Language() as language:
            return (language,)
        case None:
            suffixes = frozenset(PurePosixPath(p).suffix for p in paths if PurePosixPath(p).suffix)
            inferred = tuple(language for language in Language if suffixes & language.suffixes)
            return inferred or tuple(Language)


def _checks(routed: Routed, mode: Mode) -> tuple[Check, ...]:
    return tuple(Check(tool=t, paths=routed.files) for t in select(Claim.STATIC, routed.language) if t.mode is mode)


def _routed(languages: tuple[Language, ...], paths: tuple[str, ...], settings: AssaySettings) -> Result[Block[Routed], Fault]:
    return sequence(block.of_seq(route(language, paths, settings=settings) for language in languages))


def thin_rail(settings: AssaySettings, scope: ArtifactScope, params: StaticParams, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    """Run a static rail by routing languages and fanning selected tools.

    Returns:
        Folded static report, or routing/spawn fault.
    """
    return _routed(_languages(params.language, params.paths), params.paths, settings).bind(
        lambda routed: sequence(routed.collect(lambda r: block.of_seq(_dispatch(r, settings=settings, scope=scope, mode=mode)))).map(
            lambda done: fold(claim, verb, tuple(done))
        )
    )


def _dispatch(routed: Routed, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode) -> tuple[Result[Completed, Fault], ...]:
    # Empty scoped content means the target has no files/projects for that language.
    if routed.language.strategy == "glob" and not routed.files:
        return ()
    if routed.language.strategy == "closure" and not routed.files and not routed.projects:
        return ()
    checks = _checks(routed, mode)
    match checks:
        case ():
            return ()
        case _:
            return fan_out(checks, settings=settings, scope=scope, routed=routed)


# --- [TABLES] ---------------------------------------------------------------------------

_PREVIEW_MODES: tuple[tuple[str, Mode], ...] = (("fix", Mode.WRITE), ("report", Mode.CHECK), ("build", Mode.BUILD))


# --- [COMPOSITION] ----------------------------------------------------------------------


def fix(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Run mutating static fixer tools.

    Returns:
        Static fix report, or routing/spawn fault.
    """
    return thin_rail(settings, scope, params, claim=Claim.STATIC, verb="fix", mode=Mode.WRITE)


def report(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Run non-mutating static diagnostic tools.

    Returns:
        Static diagnostic report, or routing/spawn fault.
    """
    return thin_rail(settings, scope, params, claim=Claim.STATIC, verb="report", mode=Mode.CHECK)


def build(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Run build-mode static tools.

    Returns:
        Static build report, or routing/spawn fault.
    """
    return thin_rail(settings, scope, params, claim=Claim.STATIC, verb="build", mode=Mode.BUILD)


def full(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Run full static build parity.

    Returns:
        Full static report, or routing/spawn fault.
    """
    routed_params = replace(params, paths=params.paths or ("Workspace.slnx",))
    return thin_rail(settings, scope, routed_params, claim=Claim.STATIC, verb="full", mode=Mode.BUILD)


def plan(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Plan static routing without spawning tools.

    Returns:
        Static routing plan report, or routing fault.
    """
    _ = scope
    return _routed(_languages(params.language, params.paths), params.paths, settings).map(lambda routed: _plan_report(tuple(routed), settings, scope))


def _plan_report(routed: tuple[Routed, ...], settings: AssaySettings, scope: ArtifactScope) -> Report:
    # Route facts ride Match rows; closure shas are computed once for rows, notes, and artifacts.
    # The build artifact previews the real per-run scope the build rail opens: ArtifactScope.open(Claim.STATIC).path.
    build_scope = "/".join((str(settings.store_root), Claim.STATIC.value, settings.run_id))
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
    artifacts = tuple(Artifact(id=f"build-{sha}", kind=ArtifactKind.SCOPE, path=build_scope) for r, sha in routed_sha if r.projects)
    base = fold(Claim.STATIC, "plan", ())
    status = RailStatus.OK if any(r.files or r.projects for r, _ in routed_sha) else base.status
    return msgspec.structs.replace(base, status=status, results=rows, artifacts=artifacts, notes=notes)


def _preview(routed: Routed, verb: str, mode: Mode, settings: AssaySettings, scope: ArtifactScope) -> str:
    # Materialize the dotnet/tool argv each rail verb would spawn so plan previews commands without re-expanding per item.
    argvs = tuple(
        _argv(Check(tool=tool), routed, settings=settings, scope=scope) for tool in select(Claim.STATIC, routed.language) if tool.mode is mode
    )
    bodies = " ; ".join(" ".join(argv) for argv in argvs)
    return f"{routed.language!s} {verb}: {bodies}" if bodies else ""


def _closure_sha(routed: Routed) -> str:
    return sha256("\n".join(routed.projects).encode()).hexdigest()[:16]


# --- [EXPORTS] --------------------------------------------------------------------------

__all__: list[str] = ["StaticParams", "build", "fix", "full", "plan", "report", "thin_rail"]
