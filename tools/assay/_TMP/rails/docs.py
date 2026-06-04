"""The docs rail: one thin fold over ``thin_rail`` driving ``mmdc`` render-as-validation.

A *thin* rail adds no ``Detail`` variant, no lease, no executor logic; it binds ``Claim.DOCS``
to the shared ``thin_rail`` and reuses the route/select/fan-out/fold pipeline verbatim. Docs is
deliberately leaseless: a docs check must never contend on the ``bridge``/``mutation``/``package``
locks, so its only backpressure is the engine's ``CapacityLimiter``.

The lone driven program is ``mmdc`` (``@mermaid-js/mermaid-cli``): rendering a diagram *is* its
validation, so a parse/layout failure surfaces as a non-zero exit (``Completed{status=FAILED}``),
never a ``Fault``. ``mmdc`` has no affirmative success signal, so a clean run yields ``EMPTY``, not
``OK``; ``--strict`` exists precisely to promote that ambiguous ``EMPTY``/``SKIP`` into a hard
``FAULTED`` when an agent asserts "docs changed and must validate". The promotion rides a
``FaultedPromotion`` sentinel the registry seam converts to ``Fault{status=FAULTED}`` (exit 2) —
the sentinel never crosses a seam as a raised exception into domain logic.

The ``Result`` Error channel (``FAULTED``/``BUSY``/``TIMEOUT``) is disjoint from the success
monoid the fold reduces, so a contended or timed-out slot can never mask a clean diagram.
"""

from dataclasses import dataclass
from typing import TYPE_CHECKING

from expression import Result

from tools.assay._TMP.composition.catalog import select  # noqa: PLC2701  # intra-staging import; _TMP is the package root
from tools.assay._TMP.core.engine import fan_out  # noqa: PLC2701  # intra-staging import; _TMP is the package root
from tools.assay._TMP.core.model import (  # noqa: PLC2701  # intra-staging import; _TMP is the package root
    BaseParams,
    Check,
    Claim,
    Fault,  # noqa: TC001  # unconditional so beartype @checked resolves the rail's -> Result[Report, Fault] forward-ref under PEP 649
    fold,
    Language,
    Mode,
    Report,  # noqa: TC001  # unconditional so beartype @checked resolves the rail's -> Result[Report, Fault] forward-ref under PEP 649
)
from tools.assay._TMP.core.routing import route  # noqa: PLC2701  # intra-staging import; _TMP is the package root
from tools.assay._TMP.core.status import RailStatus  # noqa: PLC2701  # intra-staging import; _TMP is the package root


if TYPE_CHECKING:
    from tools.assay._TMP.composition.settings import ArtifactScope, AssaySettings  # intra-staging import; _TMP is the package root
    from tools.assay._TMP.core.model import Completed  # intra-staging import; _TMP is the package root
    from tools.assay._TMP.core.routing import Routed  # intra-staging import; _TMP is the package root


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class DocsParams(BaseParams):
    """The ``docs check`` CLI params: the ``BaseParams`` leaf plus the ``--strict`` flag.

    ``strict`` is a *rail-level* flag, never a catalog-row field nor a ``RailStatus`` member: it
    promotes an ``EMPTY``/``SKIP`` fold to a ``FAULTED`` ``Fault`` at the registry seam (exit 2).
    """

    strict: bool = False


# --- [ERRORS] ---------------------------------------------------------------------------


class FaultedPromotion(Exception):  # noqa: N818  # sentinel, not an *Error condition: caught at the registry seam, mapped to Fault
    """The ``--strict`` promotion sentinel: carries the unpromoted ``Report`` to the registry seam.

    A sentinel rather than a domain error: it rides exactly the one ``raise`` in ``_strict`` and the
    one ``except`` in the registry seam, which converts the carried ``Report`` to a
    ``Fault{status=FAULTED}`` (exit 2). It never crosses a seam as a raised exception into domain logic.
    """

    report: Report

    def __init__(self, report: Report) -> None:
        self.report = report
        super().__init__("--strict: no docs changed")


# --- [OPERATIONS] -----------------------------------------------------------------------


def check(settings: AssaySettings, scope: ArtifactScope, params: DocsParams) -> Result[Report, Fault]:
    """The lone ``docs check`` verb: a one-line adapter binding ``Claim.DOCS`` over ``thin_rail``.

    ``Mode.CHECK`` because render-to-validate is read-only fan-out; the load-bearing logic is the
    shared ``route ▷ select ▷ fan_out ▷ fold ▷ _strict`` pipeline, not this binding.
    """
    return thin_rail(settings, scope, params, claim=Claim.DOCS, verb="check", mode=Mode.CHECK)


def thin_rail(settings: AssaySettings, scope: ArtifactScope, params: DocsParams, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    """The shared thin-rail fold: ``route ▷ select ▷ fan_out ▷ fold ▷ _strict`` → ``Result[Report, Fault]``.

    A plain function (no ``@effect.result`` — it has no ``yield``). A route ``Fault`` short-circuits
    on the Error channel; only the ``Ok`` change-set flows through ``map`` into ``_outcomes``.
    ``select`` is sliced by ``mode`` so the rail-level ``Mode`` selects the matching catalog rows
    without a bespoke per-rail registry — the same ``thin_rail`` serves static/test/docs unchanged.
    """
    return route(Language.DOCS, params.paths).map(
        lambda routed: _strict(_outcomes(routed, settings=settings, scope=scope, claim=claim, verb=verb, mode=mode), strict=params.strict)
    )


def _outcomes(routed: Routed, *, settings: AssaySettings, scope: ArtifactScope, claim: Claim, verb: str, mode: Mode) -> Report:
    """Fan the routed inputs through the selected rows and fold the ``Completed`` receipts.

    The ``mode``-filtered slice of ``select(claim, routed.language)`` each binds to one ``Check``
    carrying the routed files as a single space-delimited ``glob`` field. ``fan_out`` is the
    leaseless executor; the walrus-guarded comprehension keeps only the ``Completed`` slots so the
    Error channel (``FAULTED``/``BUSY``/``TIMEOUT``) never enters the success monoid ``fold`` reduces,
    and a contended or timed-out slot cannot mask a clean diagram.
    """
    checks = tuple(Check(tool=t, glob=" ".join(routed.files)) for t in select(claim, routed.language) if t.mode is mode)
    slots = fan_out(checks, settings=settings, scope=scope, routed=routed)
    return fold(claim, verb, tuple(done for slot in slots if (done := _done(slot)) is not None))


def _done(slot: Result[Completed, Fault]) -> Completed | None:
    """Project one fan-out slot to its ``Completed`` success or ``None``: the success-channel filter.

    A statement-match on the ``expression`` tagged union (mirroring ``engine.leased``): an ``Ok``
    slot yields its receipt; an ``Error`` slot yields ``None`` so it drops out of the fold's monoid.
    """
    match slot:
        case Result(tag="ok", ok=done):
            return done
        case _:
            return None


def _strict(report: Report, *, strict: bool) -> Report:
    """Apply the ``--strict`` promotion: ``EMPTY``/``SKIP`` → ``FaultedPromotion``, else identity.

    Discriminates ``(strict, report.status)`` as a tuple pattern. Only the ambiguous ``EMPTY`` (ran,
    no diagram affirmed) and ``SKIP`` (vacuous opt-out) promote; ``FAILED`` is a real diagram defect
    that rides its own status and must never be re-promoted, so every other shape is the identity.
    """
    match (strict, report.status):
        case (True, RailStatus.EMPTY | RailStatus.SKIP):
            raise FaultedPromotion(report)
        case _:
            return report


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DocsParams", "FaultedPromotion", "check", "thin_rail"]
