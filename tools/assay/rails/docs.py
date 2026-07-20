"""Gate routed Markdown files through the skill engines: Mermaid validation and the prose gate."""

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
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Completed,  # noqa: TC001  # _findings/_outcomes annotate the ordered fan-out outcomes
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


class _Finding(msgspec.Struct, frozen=True):
    """One engine NDJSON row: ``check`` names the emitting check (validate-mermaid, prose-gate)."""

    file: str
    line: int
    status: str
    detail: str = ""
    check: str = ""


# --- [CONSTANTS] ------------------------------------------------------------------------

_FINDING = msgspec.json.Decoder(_Finding)
_SEVERITY = {"fail": "error", "warn": "warning"}
# Engine suffix ownership: prose-gate owns Markdown only; unlisted engines take every routed docs suffix.
_SUFFIXES: dict[str, frozenset[str]] = {"prose-gate": frozenset((".md",))}


# --- [ERRORS] ---------------------------------------------------------------------------


class FaultedPromotion(Exception):  # noqa: N818  # sentinel, not an *Error condition: caught at the registry seam, mapped to Fault
    """Strict-mode promotion raised before registry fault wrapping."""

    def __init__(self) -> None:
        """Initialize the fixed strict-mode sentinel message."""
        super().__init__("no docs changed")


# --- [OPERATIONS] -----------------------------------------------------------------------


def _decode(line: str) -> _Finding | None:
    try:
        return _FINDING.decode(line.encode())
    except msgspec.MsgspecError:
        return None


def _findings(done: tuple[Completed, ...]) -> tuple[Match, ...]:
    # The engines print one NDJSON row per finding to stdout; ``ok`` rows are passes and never surface.
    return tuple(
        Match(
            id=f"docs:{kind}",
            kind=ArtifactKind.CODE,
            text=f"docs: {found.file}:{found.line}: {kind}: {found.detail}",
            line=found.line,
            severity=severity,
            path=found.file,
            message=found.detail,
        )
        for outcome in done
        for raw in outcome.stdout.decode(errors="replace").splitlines()
        if (line := raw.strip()).startswith("{")
        for found in (_decode(line),)
        if found is not None and (severity := _SEVERITY.get(found.status)) is not None
        for kind in (found.check or "engine",)
    )


def _outcomes(
    routed: Routed, *, settings: AssaySettings, scope: ArtifactScope, claim: Claim, verb: str, mode: Mode, executor: Executor
) -> Result[Report, Fault]:
    # Each engine reads its file and writes to its own cache; assay passes only the input, never a sink placement.
    checks = tuple(
        Check(tool=t, args=ToolArgs(input=f))
        for t in select(claim, routed.language)
        if t.mode is mode
        for f in routed.files
        if PurePosixPath(f).suffix in _SUFFIXES.get(t.name, routed.language.suffixes)
    )
    slots = executor.fan(checks, settings=settings, scope=scope, routed=routed)

    def _promote(done: tuple[Completed, ...]) -> Report:
        base = fold(claim, verb, done)
        status = RailStatus.OK if done and base.status is RailStatus.EMPTY else base.status
        # The engines emit a structured NDJSON row for every failure, so parsed findings supersede fold's raw
        # stdout-tail defect rows; keep those only when nothing parsed (a tool crash), so a bare traceback surfaces.
        findings = _findings(done)
        return msgspec.structs.replace(base, status=status, results=findings or base.results)

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
    """Gate routed Markdown files through every docs engine, with optional strict EMPTY/SKIP promotion.

    Mermaid validation and the prose gate fan per (engine, file); NDJSON findings fold
    into typed result rows with fail as error and warn as warning.

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
