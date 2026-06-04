"""Leaseless docs rail: a thin fold over ``thin_rail`` driving ``mmdc`` render-as-validation.

Rendering a diagram via ``mmdc`` *is* its validation, so a parse/layout failure is a non-zero exit, never
a ``Fault``. ``mmdc`` has no affirmative success signal, so a clean run yields ``EMPTY``; ``--strict``
promotes that ambiguous ``EMPTY``/``SKIP`` to a hard ``FAULTED`` via the ``FaultedPromotion`` sentinel.
"""

from dataclasses import dataclass
from typing import TYPE_CHECKING

from expression import Result
import msgspec

from tools.assay.composition.catalog import select  # intra-package import; tools.assay is the package root
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay.core.engine import fan_out  # intra-package import; tools.assay is the package root
from tools.assay.core.model import (  # intra-package import; tools.assay is the package root
    BaseParams,
    Check,
    Claim,
    Fault,  # noqa: TC001  # unconditional so beartype @checked resolves the rail's -> Result[Report, Fault] forward-ref under PEP 649
    fold,
    Language,
    Mode,
    Report,  # noqa: TC001  # unconditional so beartype @checked resolves the rail's -> Result[Report, Fault] forward-ref under PEP 649
)
from tools.assay.core.routing import route  # intra-package import; tools.assay is the package root
from tools.assay.core.status import RailStatus  # intra-package import; tools.assay is the package root


if TYPE_CHECKING:
    from tools.assay.core.model import Completed  # intra-package import; tools.assay is the package root
    from tools.assay.core.routing import Routed  # intra-package import; tools.assay is the package root


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class DocsParams(BaseParams):
    """The ``docs check`` CLI params: the ``BaseParams`` leaf plus the ``--strict`` flag.

    ``strict`` is a rail-level flag (never a catalog-row field nor a ``RailStatus`` member): it promotes an
    ``EMPTY``/``SKIP`` fold to a ``FAULTED`` ``Fault`` at the registry seam (exit 2).
    """

    strict: bool = False


# --- [ERRORS] ---------------------------------------------------------------------------


class FaultedPromotion(Exception):  # noqa: N818  # sentinel, not an *Error condition: caught at the registry seam, mapped to Fault
    """The ``--strict`` promotion sentinel: its message IS the ``Fault`` the registry seam emits.

    A sentinel, not a domain error: the seam maps it to ``Fault{status=FAULTED}`` (exit 2) via
    ``str(promoted)`` and never reads a payload, so it carries none and never crosses a seam into domain logic.
    """

    def __init__(self) -> None:
        super().__init__("no docs changed")  # the `_guard` seam prefixes the canonical `strict:` failing-step tag


# --- [OPERATIONS] -----------------------------------------------------------------------


def check(settings: AssaySettings, scope: ArtifactScope, params: DocsParams) -> Result[Report, Fault]:
    """Bind ``Claim.DOCS`` over ``thin_rail`` as the lone ``docs check`` verb (``Mode.CHECK``: render-to-validate is read-only fan-out)."""
    return thin_rail(settings, scope, params, claim=Claim.DOCS, verb="check", mode=Mode.CHECK)


def thin_rail(settings: AssaySettings, scope: ArtifactScope, params: DocsParams, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    """Run the shared fold ``route -> select -> fan_out -> fold -> _strict`` -> ``Result[Report, Fault]``.

    A route ``Fault`` short-circuits on the Error channel; only the ``Ok`` change-set flows into
    ``_outcomes``. ``select`` is sliced by ``mode``, so the same ``thin_rail`` serves every rail unchanged.
    """
    return route(Language.DOCS, params.paths).map(
        lambda routed: _strict(_outcomes(routed, settings=settings, scope=scope, claim=claim, verb=verb, mode=mode), strict=params.strict)
    )


def _outcomes(routed: Routed, *, settings: AssaySettings, scope: ArtifactScope, claim: Claim, verb: str, mode: Mode) -> Report:
    # mmdc renders ONE -i <input> at a time and engine._argv projects tool.command (never a Check path field), so the input must
    # ride the command, spliced once per routed file. The walrus guard keeps only Completed slots so the Error channel never enters
    # the success monoid fold reduces — a contended or timed-out slot cannot mask a clean diagram.
    checks = tuple(
        Check(tool=msgspec.structs.replace(t, command=(*t.command, "-i", f)))
        for t in select(claim, routed.language)
        if t.mode is mode
        for f in routed.files
    )
    slots = fan_out(checks, settings=settings, scope=scope, routed=routed)
    return fold(claim, verb, tuple(done for slot in slots if (done := _done(slot)) is not None))


def _done(slot: Result[Completed, Fault]) -> Completed | None:
    # Success-channel filter: an Ok slot yields its receipt; an Error slot yields None so it drops out of the fold's monoid.
    match slot:
        case Result(tag="ok", ok=done):
            return done
        case _:
            return None


def _strict(report: Report, *, strict: bool) -> Report:
    # Only the ambiguous EMPTY (ran, no diagram affirmed) and SKIP (vacuous opt-out) promote; FAILED is a real defect that
    # rides its own status and must never be re-promoted, so every other shape is the identity.
    match (strict, report.status):
        case (True, RailStatus.EMPTY | RailStatus.SKIP):
            raise FaultedPromotion
        case _:
            return report


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DocsParams", "FaultedPromotion", "check", "thin_rail"]
