"""Validate Mermaid diagrams in routed Markdown files."""

from dataclasses import dataclass
from pathlib import PurePosixPath
from typing import TYPE_CHECKING

from expression import Result  # noqa: TC002  # beartype resolves return annotations at import time
from expression.collections import block
from expression.extra.result import sequence
import msgspec

from tools.assay.composition.catalog import select
from tools.assay.composition.settings import AssaySettings  # noqa: TC001  # beartype resolves rail annotations at import time
from tools.assay.composition.store import ArtifactScope  # noqa: TC001  # beartype resolves rail annotations at import time
from tools.assay.core.exec import Executor  # noqa: TC001  # beartype resolves the executor-port annotation at runtime
from tools.assay.core.model import (
    Artifact,
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Completed,  # noqa: TC001  # _rows annotates the ordered fan-out outcomes
    Fault,  # noqa: TC001  # beartype resolves Result[Report, Fault] under PEP 649 at import time
    Language,
    Match,
    Mode,
    RailStatus,
    Report,  # noqa: TC001  # beartype resolves Report in return annotations at import time
    ToolArgs,
)
from tools.assay.core.routing import route
from tools.assay.diagnostics import fold


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
        """Initialize the fixed strict-mode sentinel message."""
        super().__init__("no docs changed")


# --- [OPERATIONS] -----------------------------------------------------------------------


def _sink_stem(rel: str) -> str:
    # Full relative-path slugs prevent concurrent README.md sinks from sharing one stem.
    p = PurePosixPath(rel)
    return "__".join((*p.parent.parts, p.stem)) or p.stem


def _produced(scope: ArtifactScope, stem: str) -> tuple[str, ...]:
    # mmdc writes the -o markdown sink (``<stem>.md``) plus one ``<stem>-<n>.svg`` sibling per diagram. The ``<stem>*`` glob
    # over-matches when one slug stem prefixes another (``a`` catches ``ab.md``), so keep only the exact ``<stem>.md`` sink and
    # its ``<stem>-`` svg siblings — the boundary char severs a longer co-resident stem.
    run_dir = scope.path.removeprefix(f"{scope.store.root}/")
    sink, sibling = f"{stem}.md", f"{stem}-"
    return tuple(p for p in scope.store.glob(f"{run_dir}/{stem}*") if (name := p.rsplit("/", 1)[-1]) == sink or name.startswith(sibling))


def _rows(
    scope: ArtifactScope, files: tuple[str, ...], stems: tuple[str, ...], done: tuple[Completed, ...]
) -> tuple[tuple[Match, ...], tuple[Artifact, ...]]:
    # Fan-out order aligns files with receipts; strict zip keeps routed-empty synthetic callers valid.
    # Artifact rows enumerate produced Markdown and SVG sinks so envelopes own the scope shape.
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


def _outcomes(
    routed: Routed, *, settings: AssaySettings, scope: ArtifactScope, claim: Claim, verb: str, mode: Mode, executor: Executor
) -> Result[Report, Fault]:
    # mmdc requires per-file -i/-a/-o placement; scope materialization stays at the ArtifactScope boundary.
    scope_dir = scope.ensure()
    pairs = tuple((t, f, _sink_stem(f)) for t in select(claim, routed.language) if t.mode is mode for f in routed.files)
    files = tuple(f for _, f, _ in pairs)
    stems = tuple(stem for *_, stem in pairs)
    checks = tuple(Check(tool=t, args=ToolArgs(input=f, sink_dir=scope_dir, sink=f"{scope_dir}/{stem}.md")) for t, f, stem in pairs)
    slots = executor.fan(checks, settings=settings, scope=scope, routed=routed)

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


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(settings: AssaySettings, scope: ArtifactScope, params: DocsParams, executor: Executor) -> Result[Report, Fault]:
    """Validate Mermaid diagrams across routed Markdown files, with optional strict EMPTY/SKIP promotion.

    Returns:
        Folded report, or a routing/spawn/strict-promotion fault.
    """
    return route(Language.DOCS, params.paths, settings=settings).bind(
        lambda routed: _outcomes(routed, settings=settings, scope=scope, claim=Claim.DOCS, verb="check", mode=Mode.CHECK, executor=executor).map(
            lambda report: _strict(report, strict=params.strict)
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DocsParams", "FaultedPromotion", "check"]
