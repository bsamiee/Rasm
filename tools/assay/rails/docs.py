"""Validate Mermaid diagrams in Markdown files."""

from dataclasses import dataclass
from typing import TYPE_CHECKING

from expression import Result  # noqa: TC002  # unconditional: registry beartype resolves handler return annotations at runtime
from expression.collections import block
from expression.extra.result import sequence
import msgspec

from tools.assay.composition.catalog import select
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay.core.engine import fan_out
from tools.assay.core.model import (
    BaseParams,
    Check,
    Claim,
    Fault,  # noqa: TC001  # unconditional so beartype @checked resolves the rail's -> Result[Report, Fault] forward-ref under PEP 649
    fold,
    Language,
    Mode,
    Report,  # noqa: TC001  # unconditional so beartype @checked resolves the rail's -> Result[Report, Fault] forward-ref under PEP 649
)
from tools.assay.core.routing import route
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from tools.assay.core.routing import Routed


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class DocsParams(BaseParams):
    """Parameters for `docs check`."""

    strict: bool = False


# --- [ERRORS] ---------------------------------------------------------------------------


class FaultedPromotion(Exception):  # noqa: N818  # sentinel, not an *Error condition: caught at the registry seam, mapped to Fault
    """Strict-mode promotion raised before registry fault wrapping."""

    def __init__(self) -> None:
        """Create the strict-mode promotion sentinel."""
        super().__init__("no docs changed")


# --- [OPERATIONS] -----------------------------------------------------------------------


def _thin_rail(settings: AssaySettings, scope: ArtifactScope, params: DocsParams, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    """Run the shared docs route, fan-out, fold, and strict promotion body.

    Returns:
        Folded docs report, or strict-mode/spawn fault.
    """
    return route(Language.DOCS, params.paths, settings=settings).bind(
        lambda routed: _outcomes(routed, settings=settings, scope=scope, claim=claim, verb=verb, mode=mode).map(
            lambda report: _strict(report, strict=params.strict)
        )
    )


def _outcomes(routed: Routed, *, settings: AssaySettings, scope: ArtifactScope, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    # `mmdc` takes one `-i` input per command, so each routed file is spliced into tool.command.
    checks = tuple(
        Check(tool=msgspec.structs.replace(t, command=(*t.command, "-i", f)))
        for t in select(claim, routed.language)
        if t.mode is mode
        for f in routed.files
    )
    slots = fan_out(checks, settings=settings, scope=scope, routed=routed)
    return sequence(block.of_seq(slots)).map(lambda done: fold(claim, verb, tuple(done)))


def _strict(report: Report, *, strict: bool) -> Report:
    # Strict mode promotes only ambiguous `EMPTY`/`SKIP`; real defects keep their original status.
    match (strict, report.status):
        case (True, RailStatus.EMPTY | RailStatus.SKIP):
            raise FaultedPromotion
        case _:
            return report


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(settings: AssaySettings, scope: ArtifactScope, params: DocsParams) -> Result[Report, Fault]:
    """Run docs validation.

    Returns:
        Docs validation report, or routing/spawn fault.
    """
    return _thin_rail(settings, scope, params, claim=Claim.DOCS, verb="check", mode=Mode.CHECK)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DocsParams", "FaultedPromotion", "check"]
