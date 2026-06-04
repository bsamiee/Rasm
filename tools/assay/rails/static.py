"""Static rail: five verbs as one Mode-filtered, language-routed fold over ``Claim.STATIC``.

The four executing adapters delegate to ``thin_rail``; ``plan`` short-circuits before the Engine.
A timeout/spawn fault short-circuits to the registry seam, while a non-zero exit already rode the
success channel as ``Completed{status=FAILED}``, so the fold consumes successes only.
"""

from dataclasses import dataclass
from hashlib import sha256
from typing import TYPE_CHECKING

from expression import Result  # noqa: TC002  # unconditional: beartype @checked resolves the handler forward-ref (PEP 649)
from expression.collections import block
from expression.extra.result import sequence
import msgspec

from tools.assay.composition.catalog import select  # intra-package import; tools.assay is the package root
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay.core.engine import fan_out  # intra-package import; tools.assay is the package root
from tools.assay.core.model import (  # intra-package import; tools.assay is the package root
    Artifact,
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Fault,  # noqa: TC001  # unconditional: @checked's beartype resolves the -> Result[Report, Fault] forward-ref under PEP 649
    fold,
    Language,
    Mode,
    Report,  # noqa: TC001  # unconditional: see Fault above (same forward-ref resolution)
)
from tools.assay.core.routing import route, Routed  # intra-package import; tools.assay is the package root


if TYPE_CHECKING:
    from expression.collections import Block

    from tools.assay.core.model import Completed  # intra-package import; tools.assay is the package root


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class StaticParams(BaseParams):
    """Static rail CLI params: inherits ``paths``/``language`` from ``BaseParams``, adds nothing.

    No ``strict`` field, so an empty static slice can never be hardened into a fault.
    """


# --- [OPERATIONS] -----------------------------------------------------------------------


def _languages(selected: Language | None) -> tuple[Language, ...]:
    # DOCS rides the fan: select(Claim.STATIC, DOCS) yields no row, so the empty slice folds to EMPTY without a guard.
    match selected:
        case None:
            return tuple(Language)
        case language:
            return (language,)


def _checks(routed: Routed, mode: Mode) -> tuple[Check, ...]:
    return tuple(Check(tool=t, paths=routed.files) for t in select(Claim.STATIC, routed.language) if t.mode is mode)


def _routed(languages: tuple[Language, ...], paths: tuple[str, ...]) -> Result[Block[Routed], Fault]:
    # route resolves one Language per call; the first routing Fault short-circuits the whole polyglot fan.
    return sequence(block.of_seq(route(language, paths) for language in languages))


def thin_rail(settings: AssaySettings, scope: ArtifactScope, params: StaticParams, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    """The shared body the four executing adapters parameterize: route -> fan_out -> fold.

    ``full`` differs from ``build`` by ``verb="full"`` only (both pass ``mode=Mode.BUILD``); the FULL
    routing scope is a ``CLOSURE``-arm escalation internal to ``routing``, never threaded here. A
    spawn/timeout/lease fault is the sole Error channel; an honest no-op slice folds to ``EMPTY``/``SKIP``.
    """
    return _routed(_languages(params.language), params.paths).bind(
        lambda routed: sequence(routed.collect(lambda r: block.of_seq(_dispatch(r, settings=settings, scope=scope, mode=mode)))).map(
            lambda done: fold(claim, verb, tuple(done))
        )
    )


def _dispatch(routed: Routed, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode) -> tuple[Result[Completed, Fault], ...]:
    # One fan_out per language so each argv tail is projected from its own Routed, never a shared head.
    checks = _checks(routed, mode)
    match checks:
        case ():
            return ()
        case _:
            return fan_out(checks, settings=settings, scope=scope, routed=routed)


# --- [COMPOSITION] ----------------------------------------------------------------------


def fix(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """The ``Mode.WRITE`` adapter: fan the mutating formatter/fixer rows for ``static fix``."""
    return thin_rail(settings, scope, params, claim=Claim.STATIC, verb="fix", mode=Mode.WRITE)


def report(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """The ``Mode.CHECK`` adapter: fan the non-mutating diagnostic ladder for ``static report``."""
    return thin_rail(settings, scope, params, claim=Claim.STATIC, verb="report", mode=Mode.CHECK)


def build(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """The ``Mode.BUILD`` adapter: fan the compile rows under the closure-leased artifact scope for ``static build``."""
    return thin_rail(settings, scope, params, claim=Claim.STATIC, verb="build", mode=Mode.BUILD)


def full(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """The ``.slnx``-parity adapter for ``static full``: ``build`` whose trigger-file edits route ``Scope.FULL`` inside ``routing``."""
    return thin_rail(settings, scope, params, claim=Claim.STATIC, verb="full", mode=Mode.BUILD)


def plan(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """The zero-run adapter: route, fold owners/triggers/closure-sha into ``notes``/``artifacts``, never reach the Engine.

    The closure sha is the same ``sha256(sorted-projects)[:16]`` recipe ``ArtifactScope.build`` keys its
    warm tree on, so a planned ``build-<closure>`` tree correlates with a subsequent ``build`` lease.
    """
    _ = scope  # plan runs zero checks: no artifact scope is spliced
    return _routed(_languages(params.language), params.paths).map(
        lambda routed: msgspec.structs.replace(
            fold(Claim.STATIC, "plan", ()),  # empty outcomes seed EMPTY status + zero counts via the sole deriver
            artifacts=_plan_artifacts(tuple(routed), settings),
            notes=_plan_notes(tuple(routed)),
        )
    )


def _plan_artifacts(routed: tuple[Routed, ...], settings: AssaySettings) -> tuple[Artifact, ...]:
    # Only a non-empty project closure plans a warm tree; the path is byte-identical to the one build leases.
    return tuple(
        Artifact(id=f"build-{(sha := _closure_sha(r))}", kind=ArtifactKind.SCOPE, path=ArtifactScope.build(settings, sha).path)
        for r in routed
        if r.projects
    )


def _plan_notes(routed: tuple[Routed, ...]) -> tuple[str, ...]:
    return tuple(note for r in routed for note in _routed_notes(r))


def _routed_notes(routed: Routed) -> tuple[str, ...]:
    match routed:
        case Routed(projects=projects) if projects:
            return (f"{routed.language.value}: scope={routed.scope.value} closure={_closure_sha(routed)} projects={len(projects)}",)
        case Routed(full_triggers=triggers) if triggers:
            return (
                f"{routed.language.value}: scope={routed.scope.value} files={len(routed.files)}",
                *(f"{routed.language.value}: full-trigger {t}" for t in triggers),
            )
        case _:
            return (f"{routed.language.value}: scope={routed.scope.value} files={len(routed.files)}",)


def _closure_sha(routed: Routed) -> str:
    # projects are already sorted by routing._resolve, so the planned build-<closure> path matches build's warm tree bit-for-bit.
    return sha256("\n".join(routed.projects).encode()).hexdigest()[:16]


# --- [EXPORTS] --------------------------------------------------------------------------

__all__: list[str] = ["StaticParams", "build", "fix", "full", "plan", "report", "thin_rail"]
