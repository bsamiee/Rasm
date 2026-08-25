"""Gate routed Markdown files through the docs engines: Mermaid validation, the prose gate, and the planning-marker gate."""

from dataclasses import dataclass
from pathlib import Path, PurePosixPath
import re
from typing import TYPE_CHECKING

from expression import Result
from expression.collections import block
from expression.extra.result import sequence
import msgspec

from assay.composition.catalog import select
from assay.composition.settings import AssaySettings
from assay.composition.store import ArtifactScope
from assay.core.exec import Executor
from assay.core.model import (
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Completed,
    Fault,
    InprocThunk,
    Language,
    Match,
    Mode,
    RailStatus,
    receipt,
    Report,
    Runner,
    ToolArgs,
)
from assay.core.routing import route
from assay.diagnostics import fold

if TYPE_CHECKING:
    from assay.core.routing import Routed


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class DocsParams(BaseParams):
    """Parameters for the docs check subcommand."""

    strict: bool = False


class _Finding(msgspec.Struct, frozen=True):
    """One engine NDJSON row: ``check`` names the emitting check (validate-mermaid, prose-gate, research-*)."""

    file: str
    line: int
    status: str
    detail: str = ""
    check: str = ""


# --- [CONSTANTS] ------------------------------------------------------------------------

_FINDING = msgspec.json.Decoder(_Finding)
_FINDING_ROW = msgspec.json.Encoder()
_SEVERITY = {"fail": "error", "warn": "warning"}
_SUFFIXES: dict[str, frozenset[str]] = {"prose-gate": frozenset((".md",)), "planning-gate": frozenset((".md",))}

_STATUSES: dict[str, frozenset[str]] = {"RESEARCH": frozenset(("OPEN", "BLOCKED"))}
_SECTION = re.compile(r"^## \[\d{1,2}\]-\[([A-Z_]+)\]\s*$")
_HEADING = re.compile(r"^#{1,2} ")
_RESEARCH_HEADER = re.compile(r"^#{1,2} \[\d{1,2}\]-\[RESEARCH\]\s*$")
_RESEARCH_ROW = re.compile(r"^- \[([A-Z0-9_-]+)\](.*)$")
_RESEARCH_TAIL = re.compile(r"^-\[([A-Z]+)\]: *(.*)$")
_RESEARCH_RECORD = re.compile(r"^(?:: *| — | \()\S")
_TEMPLATE_LEADER = re.compile(r"^\[[A-Z0-9_]+\]-\[([A-Z|]+)\]:")


# --- [ERRORS] ---------------------------------------------------------------------------


class FaultedPromotion(Exception):  # ruff:ignore[error-suffix-on-exception-name]
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


def _masked(lines: tuple[str, ...]) -> tuple[tuple[bool, bool], ...]:
    rows: list[tuple[bool, bool]] = []
    comment = fence = False
    for line in lines:
        walled = line.lstrip().startswith("```")
        if fence or walled:
            rows.append((comment, True))
            fence = not walled if fence else True
            continue
        opened = comment or "<!--" in line
        rows.append((opened, False))
        comment = opened and "-->" not in line
    return tuple(rows)


def _fail(rel: str, line: int, check: str, detail: str) -> _Finding:
    return _Finding(file=rel, line=line, status="fail", detail=detail, check=check)


def _template_statuses(lines: tuple[str, ...], flags: tuple[tuple[bool, bool], ...], section: str) -> frozenset[str]:
    tracked, declared = "", set()
    for line, (comment, _fence) in zip(lines, flags, strict=True):
        if (head := _SECTION.match(line)) is not None:
            tracked = head.group(1)
        elif comment and tracked == section and (leader := _TEMPLATE_LEADER.match(line)) is not None:
            declared.update(set(leader.group(1).split("|")) - {"STATUS"})
    return frozenset(declared) | _STATUSES[section]


def _research_row(rel: str, number: int, line: str, statuses: frozenset[str]) -> tuple[_Finding, ...]:
    grammar = f"- [TOKEN]-[{'|'.join(sorted(statuses))}]: <question>; <route> or a settled - [TOKEN]: / - [TOKEN] — record"
    match _RESEARCH_ROW.match(line):
        case None:
            return (_fail(rel, number, "research-row", f"malformed research row; expected {grammar}"),)
        case found:
            tail = found.group(2)
    hyphen = (
        (_fail(rel, number, "research-row", f"hyphenated research token [{found.group(1)}]; tokens are UPPERCASE_SNAKE"),)
        if "-" in found.group(1)
        else ()
    )
    if _RESEARCH_RECORD.match(tail):
        return hyphen
    match _RESEARCH_TAIL.match(tail):
        case None:
            return (*hyphen, _fail(rel, number, "research-row", f"malformed research row; expected {grammar}"))
        case status_tail if status_tail.group(1) not in statuses:
            legal = "|".join(sorted(statuses))
            return (*hyphen, _fail(rel, number, "research-row", f"illegal research status [{status_tail.group(1)}]; legal: {legal}"))
        case status_tail if not status_tail.group(2).strip():
            return (*hyphen, _fail(rel, number, "research-row", "research row missing its question and verification route"))
        case _:
            return hyphen


def _research_rows(rel: str, lines: tuple[str, ...], flags: tuple[tuple[bool, bool], ...]) -> tuple[_Finding, ...]:
    statuses = _template_statuses(lines, flags, "RESEARCH")
    numbered = tuple(zip(range(1, len(lines) + 1), lines, flags, strict=True))
    headers = tuple(number for number, line, (_comment, fence) in numbered if not fence and _RESEARCH_HEADER.match(line))
    deformed = tuple(
        _fail(rel, number, "research-section", f"deformed RESEARCH section marker {line.strip()!r}; expected ## [NN]-[RESEARCH]")
        for number, line, (_comment, fence) in numbered
        if not fence and line.startswith("#") and "[RESEARCH]" in line and not _RESEARCH_HEADER.match(line)
    )
    if not headers:
        orphan = next(
            (
                number
                for number, line, (comment, fence) in numbered
                if not fence
                and not comment
                and (row := _RESEARCH_ROW.match(line)) is not None
                and (row_tail := _RESEARCH_TAIL.match(row.group(2))) is not None
                and row_tail.group(1) in statuses
                and ";" in row_tail.group(2)
            ),
            None,
        )
        missing = () if orphan is None else (_fail(rel, orphan, "research-section", "research rows orphaned; terminal [RESEARCH] section missing"),)
        return (*deformed, *missing)
    after = tuple(entry for entry in numbered if entry[0] > headers[0])
    duplicates = tuple(_fail(rel, number, "research-section", "duplicate [RESEARCH] section") for number in headers[1:])
    displaced = tuple(
        _fail(rel, number, "research-section", "[RESEARCH] section not terminal; a section follows it")
        for number, line, (_comment, fence) in after
        if not fence and _HEADING.match(line) and number not in headers
    )
    stop = next((number for number, line, (_comment, fence) in after if not fence and _HEADING.match(line)), len(lines) + 1)
    rows = tuple(
        finding
        for number, line, (comment, fence) in after
        if number < stop and not fence and not comment and line.startswith("- [")
        for finding in _research_row(rel, number, line, statuses)
    )
    return (*deformed, *duplicates, *displaced, *rows)


def _planning_findings(rel: str, root: Path) -> tuple[_Finding, ...] | None:
    parts = PurePosixPath(rel).parts
    page = bool(parts) and parts[0] == "libs" and ".planning" in parts[:-1] and parts.index(".planning") < len(parts) - 2
    if not page:
        return None
    try:
        text = (root / rel).read_text(encoding="utf-8", errors="replace")
    except OSError:
        return None
    lines = tuple(text.splitlines())
    flags = _masked(lines)
    return _research_rows(rel, lines, flags)


def _planning(root: Path) -> InprocThunk:
    """Build the INPROC planning-gate thunk validating RESEARCH sections over planning durables.

    Findings are encoded into ``Completed.stdout`` as the same NDJSON rows the process engines print, so the
    fold, severity mapping, and report shape stay one contract across every docs engine.

    Returns:
        Callable that accepts a Check and returns a Completed: EMPTY off planning durables, OK clean, FAILED with findings.
    """

    def run(check: Check) -> Completed:
        rel = check.args.input
        argv = ("planning-gate", "check", rel)
        findings = _planning_findings(rel, root)
        if findings is None:
            return receipt(argv, 0, status=RailStatus.EMPTY)
        payload = b"\n".join(_FINDING_ROW.encode(row) for row in findings)
        return receipt(argv, 1 if findings else 0, stdout=payload, status=RailStatus.FAILED if findings else RailStatus.OK)

    return run


def _outcomes(
    routed: Routed, *, settings: AssaySettings, scope: ArtifactScope, claim: Claim, verb: str, mode: Mode, executor: Executor
) -> Result[Report, Fault]:
    thunk = _planning(Path(str(settings.root)))
    checks = tuple(
        Check(tool=t, args=ToolArgs(input=f), thunk=thunk if t.runner is Runner.INPROC else None)
        for t in select(claim, routed.language)
        if t.mode is mode
        for f in routed.files
        if PurePosixPath(f).suffix in _SUFFIXES.get(t.name, routed.language.suffixes)
    )
    slots = executor.fan(checks, settings=settings, scope=scope, routed=routed)

    def _promote(done: tuple[Completed, ...]) -> Report:
        base = fold(claim, verb, done)
        status = RailStatus.OK if done and base.status is RailStatus.EMPTY else base.status
        findings = _findings(done)
        return msgspec.structs.replace(base, status=status, results=findings or base.results)

    return sequence(block.of_seq(slots)).map(lambda done: _promote(tuple(done)))


def _strict(report: Report, *, strict: bool) -> Report:
    match (strict, report.status):
        case (True, RailStatus.EMPTY | RailStatus.SKIP):
            raise FaultedPromotion
        case _:
            return report


# --- [COMPOSITION] ----------------------------------------------------------------------


def check(settings: AssaySettings, scope: ArtifactScope, params: DocsParams, executor: Executor) -> Result[Report, Fault]:
    """Gate routed Markdown files through every docs engine, with optional strict EMPTY/SKIP promotion.

    Mermaid validation, the prose gate, and the in-process planning-marker gate fan per
    (engine, file); NDJSON findings fold into typed result rows with fail as error and
    warn as warning.

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
