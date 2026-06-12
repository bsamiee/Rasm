"""Docs rail: Mermaid diagram validation across Markdown files.

Fans out one mmdc invocation per (tool, file) pair, folds results into a
Report, and applies strict-mode promotion: EMPTY or SKIP outcomes raise
FaultedPromotion before the registry fault-wrapping seam, converting
absent-change ambiguity into an explicit fault.
"""

from dataclasses import dataclass
from pathlib import PurePosixPath
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
    Artifact,
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Completed,  # noqa: TC001  # _rows annotates the ordered fan-out outcomes
    Fault,  # noqa: TC001  # beartype resolves Result[Report, Fault] under PEP 649 at import time
    fold,
    Language,
    Match,
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


def _sink_stem(rel: str) -> str:
    # Two routed README.md files share a stem; slug the full relative path so concurrent fan-out sinks
    # never clobber a shared {scope_dir}/{stem}.md. PurePosixPath strips the suffix once; the rest folds to a flat key.
    p = PurePosixPath(rel)
    return "__".join((*p.parent.parts, p.stem)) or p.stem


def _produced(scope: ArtifactScope, stem: str) -> tuple[str, ...]:
    # mmdc writes the -o markdown sink plus one -<n>.svg sibling per diagram; glob the scope dir for the slugged family.
    run_dir = scope.path.removeprefix(f"{scope.store.root}/")
    return scope.store.glob(f"{run_dir}/{stem}*")


def _rows(
    scope: ArtifactScope, files: tuple[str, ...], stems: tuple[str, ...], done: tuple[Completed, ...]
) -> tuple[tuple[Match, ...], tuple[Artifact, ...]]:
    # One result row per (file, mmdc receipt); severity tracks the receipt's exit. Artifact rows enumerate every
    # produced file (SVG + MD) so an agent reads paths straight from the envelope instead of inferring the scope shape.
    # fan_out preserves check order, so files and done align on the Ok rail (sequence short-circuits any Error
    # before the fold); strict=False keeps synthetic single-receipt callers from over-zipping a routed-empty set.
    results = tuple(
        Match(
            id=f"source:{rel}:1",
            kind=ArtifactKind.PROCESS,
            text=f"mmdc {rel}: {'ok' if d.status is not RailStatus.FAILED else 'failed'}",
            severity="failed" if d.status is RailStatus.FAILED else None,
        )
        for rel, d in zip(files, done, strict=False)
    )
    artifacts = tuple(Artifact(id=path.rsplit("/", 1)[-1], kind=ArtifactKind.SCOPE, path=path) for stem in stems for path in _produced(scope, stem))
    return results, artifacts


def _outcomes(routed: Routed, *, settings: AssaySettings, scope: ArtifactScope, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    # mmdc owns its input placement (Input.OWNED): each (tool, file) pair carries the full -i/-a/-o argv. Markdown mode
    # requires an -o path ending in .md and writes SVG siblings under -a; both ride the per-run artifact scope. The
    # scope dir is materialized through the store boundary (ArtifactScope.ensure) because open() no longer eagerly makedirs.
    scope_dir = scope.ensure()
    pairs = tuple((t, f, _sink_stem(f)) for t in select(claim, routed.language) if t.mode is mode for f in routed.files)
    files = tuple(f for _, f, _ in pairs)
    stems = tuple(stem for *_, stem in pairs)
    checks = tuple(
        Check(tool=msgspec.structs.replace(t, command=(*t.command, "-i", f, "-a", scope_dir, "-o", f"{scope_dir}/{stem}.md"))) for t, f, stem in pairs
    )
    slots = fan_out(checks, settings=settings, scope=scope, routed=routed)

    def _promote(done: tuple[Completed, ...]) -> Report:
        base = fold(claim, verb, done)
        results, artifacts = _rows(scope, files, stems, done)
        status = RailStatus.OK if results and base.status is RailStatus.EMPTY else base.status
        return msgspec.structs.replace(base, status=status, results=(*base.results, *results), artifacts=(*base.artifacts, *artifacts))

    return sequence(block.of_seq(slots)).map(lambda done: _promote(tuple(done)))


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
