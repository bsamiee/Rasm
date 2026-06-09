"""Docs rail: Mermaid diagram validation across Markdown files.

Fans out one mmdc invocation per (tool, file) pair, folds results into a
Report, and applies strict-mode promotion: EMPTY or SKIP outcomes raise
FaultedPromotion before the registry fault-wrapping seam, converting
absent-change ambiguity into an explicit fault.
"""

from dataclasses import dataclass
from typing import TYPE_CHECKING

from expression import Result  # noqa: TC002  # beartype resolves return annotations at import time
from expression.collections import block
from expression.extra.result import sequence
import msgspec

from tools.assay.composition.catalog import select
from tools.assay.composition.settings import (  # noqa: TC001  # beartype resolves ArtifactScope/AssaySettings in function annotations at import time
    ArtifactScope,
    AssaySettings,
)
from tools.assay.core.engine import fan_out
from tools.assay.core.model import (
    BaseParams,
    Check,
    Claim,
    Fault,  # noqa: TC001  # beartype resolves Result[Report, Fault] under PEP 649 at import time
    fold,
    Language,
    Mode,
    Report,  # noqa: TC001  # beartype resolves Report in return annotations at import time
)
from tools.assay.core.routing import route
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from tools.assay.core.routing import Routed


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class DocsParams(BaseParams):
    """Parameters for the docs check subcommand."""

    strict: bool = False


# --- [ERRORS] ---------------------------------------------------------------------------


class FaultedPromotion(Exception):  # noqa: N818  # sentinel, not an *Error condition: caught at the registry seam, mapped to Fault
    """Strict-mode promotion raised before registry fault wrapping."""

    def __init__(self) -> None:
        """Construct the sentinel with a fixed message."""
        super().__init__("no docs changed")


# --- [OPERATIONS] -----------------------------------------------------------------------


def _outcomes(routed: Routed, *, settings: AssaySettings, scope: ArtifactScope, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    # mmdc accepts one -i flag per invocation; files are cartesian-producted with tools here rather than inside fan_out.
    checks = tuple(
        Check(tool=msgspec.structs.replace(t, command=(*t.command, "-i", f)))
        for t in select(claim, routed.language)
        if t.mode is mode
        for f in routed.files
    )
    slots = fan_out(checks, settings=settings, scope=scope, routed=routed)
    return sequence(block.of_seq(slots)).map(lambda done: fold(claim, verb, tuple(done)))


def _strict(report: Report, *, strict: bool) -> Report:
    # Only EMPTY/SKIP are ambiguous in strict mode; real defects carry their status through.
    match (strict, report.status):
        case (True, RailStatus.EMPTY | RailStatus.SKIP):
            raise FaultedPromotion
        case _:
            return report


def _thin_rail(settings: AssaySettings, scope: ArtifactScope, params: DocsParams, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    """Run the shared docs route, fan-out, fold, and strict-mode promotion.

    Returns:
        Ok-folded Report on success; Fault on routing failure, spawn failure,
        or FaultedPromotion when strict mode rejects an EMPTY or SKIP outcome.
    """
    return route(Language.DOCS, params.paths, settings=settings).bind(
        lambda routed: _outcomes(routed, settings=settings, scope=scope, claim=claim, verb=verb, mode=mode).map(
            lambda report: _strict(report, strict=params.strict)
        )
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(settings: AssaySettings, scope: ArtifactScope, params: DocsParams) -> Result[Report, Fault]:
    """Run Mermaid diagram validation across all routed Markdown files.

    Returns:
        Ok-folded Report on success; Fault on routing failure or spawn failure.
    """
    return _thin_rail(settings, scope, params, claim=Claim.DOCS, verb="check", mode=Mode.CHECK)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DocsParams", "FaultedPromotion", "check"]
