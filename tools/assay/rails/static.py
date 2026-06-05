"""Run static analysis, formatting, build, and plan rails."""

from dataclasses import dataclass
from hashlib import sha256
from pathlib import PurePosixPath
from typing import TYPE_CHECKING

from expression import Result  # noqa: TC002  # unconditional: beartype @checked resolves the handler forward-ref (PEP 649)
from expression.collections import block
from expression.extra.result import sequence
import msgspec

from tools.assay.composition.catalog import select
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay.core.engine import fan_out
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
    """Parameters for static rails inherited from `BaseParams`."""


# --- [OPERATIONS] -----------------------------------------------------------------------


def _languages(selected: Language | None, paths: tuple[str, ...]) -> tuple[Language, ...]:
    match selected:
        case Language() as language:
            return (language,)
        case None:
            suffixes = frozenset(PurePosixPath(p).suffix for p in paths if PurePosixPath(p).suffix)
            inferred = next((lang for lang in Language if suffixes and suffixes <= lang.suffixes), None)
            return (inferred,) if inferred is not None else tuple(Language)


def _checks(routed: Routed, mode: Mode) -> tuple[Check, ...]:
    return tuple(Check(tool=t, paths=routed.files) for t in select(Claim.STATIC, routed.language) if t.mode is mode)


def _routed(languages: tuple[Language, ...], paths: tuple[str, ...]) -> Result[Block[Routed], Fault]:
    # route resolves one Language per call; the first routing Fault short-circuits the whole polyglot fan.
    return sequence(block.of_seq(route(language, paths) for language in languages))


def thin_rail(settings: AssaySettings, scope: ArtifactScope, params: StaticParams, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    """Run a static rail by routing languages and fanning selected tools.

    Args:
        settings: Runtime settings.
        scope: Artifact scope.
        params: Static params.
        claim: Claim to fold under.
        verb: Verb to fold under.
        mode: Tool mode to select.

    Returns:
        Result containing the folded static report or routing fault.

    """
    return _routed(_languages(params.language, params.paths), params.paths).bind(
        lambda routed: sequence(routed.collect(lambda r: block.of_seq(_dispatch(r, settings=settings, scope=scope, mode=mode)))).map(
            lambda done: fold(claim, verb, tuple(done))
        )
    )


def _dispatch(routed: Routed, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode) -> tuple[Result[Completed, Fault], ...]:
    # Skip a language with no scoped content: no file/project of that language was found in the
    # target. For glob-strategy languages (Python/TS/Bash/SQL/Docs) the signal is empty `routed.files`;
    # for closure-strategy languages (C#) it is empty `routed.projects` AND empty `routed.files`
    # (a full-scope escalation keeps files but may have projects). This prevents a Python-only target
    # from fanning C# tools and vice versa.
    if routed.language.strategy == "glob" and not routed.files:
        return ()
    if routed.language.strategy == "closure" and not routed.files and not routed.projects:
        return ()
    # One fan_out per language so each argv tail is projected from its own Routed, never a shared head.
    checks = _checks(routed, mode)
    match checks:
        case ():
            return ()
        case _:
            return fan_out(checks, settings=settings, scope=scope, routed=routed)


# --- [COMPOSITION] ----------------------------------------------------------------------


def fix(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Run mutating static fixer tools.

    Args:
        settings: Runtime settings.
        scope: Artifact scope.
        params: Static params.

    Returns:
        Result containing the fix report or routing fault.

    """
    return thin_rail(settings, scope, params, claim=Claim.STATIC, verb="fix", mode=Mode.WRITE)


def report(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Run non-mutating static diagnostic tools.

    Args:
        settings: Runtime settings.
        scope: Artifact scope.
        params: Static params.

    Returns:
        Result containing the diagnostic report or routing fault.

    """
    return thin_rail(settings, scope, params, claim=Claim.STATIC, verb="report", mode=Mode.CHECK)


def build(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Run build-mode static tools.

    Args:
        settings: Runtime settings.
        scope: Artifact scope.
        params: Static params.

    Returns:
        Result containing the build report or routing fault.

    """
    return thin_rail(settings, scope, params, claim=Claim.STATIC, verb="build", mode=Mode.BUILD)


def full(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Run full static build parity.

    Args:
        settings: Runtime settings.
        scope: Artifact scope.
        params: Static params.

    Returns:
        Result containing the full static report or routing fault.

    """
    return thin_rail(settings, scope, params, claim=Claim.STATIC, verb="full", mode=Mode.BUILD)


def plan(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """Plan static routing without spawning tools.

    Args:
        settings: Runtime settings.
        scope: Artifact scope.
        params: Static params.

    Returns:
        Result containing route facts and build-scope artifacts.

    """
    _ = scope  # plan runs zero checks: no artifact scope is spliced
    return _routed(_languages(params.language, params.paths), params.paths).map(lambda routed: _plan_report(tuple(routed), settings))


def _plan_report(routed: tuple[Routed, ...], settings: AssaySettings) -> Report:
    # Project per-language routing facts onto Match rows (detail=None; all per-language data rides results uniformly).
    # sha is pre-computed once per routed entry to avoid triple _closure_sha recompute across rows/notes/artifacts.
    # Status: OK when at least one language has a non-empty scope (files or projects); EMPTY otherwise.
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
    notes = tuple(
        f"{r.language!s}: scope={r.scope!s} closure={sha} projects={len(r.projects)}"
        if r.projects
        else f"{r.language!s}: scope={r.scope!s} files={len(r.files)}"
        for r, sha in routed_sha
    )
    artifacts = tuple(
        Artifact(id=f"build-{sha}", kind=ArtifactKind.SCOPE, path=str(settings.artifact(ArtifactKind.SCOPE, "build", sha)))
        for r, sha in routed_sha
        if r.projects
    )
    base = fold(Claim.STATIC, "plan", ())
    status = RailStatus.OK if any(r.files or r.projects for r, _ in routed_sha) else base.status
    return msgspec.structs.replace(base, status=status, results=rows, artifacts=artifacts, notes=notes)


def _closure_sha(routed: Routed) -> str:
    # projects are already sorted by routing._resolve, so the planned build-<closure> path matches build's warm tree bit-for-bit.
    return sha256("\n".join(routed.projects).encode()).hexdigest()[:16]


# --- [EXPORTS] --------------------------------------------------------------------------

__all__: list[str] = ["StaticParams", "build", "fix", "full", "plan", "report", "thin_rail"]
