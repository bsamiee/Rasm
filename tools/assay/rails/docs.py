"""Gate routed Markdown files through the docs engines: Mermaid validation, the prose gate, and the planning-marker gate."""

from dataclasses import dataclass
from pathlib import Path, PurePosixPath
import re
from typing import TYPE_CHECKING

from expression import Result  # ruff:ignore[typing-only-third-party-import]  # beartype resolves return annotations at import time
from expression.collections import block
from expression.extra.result import sequence
import msgspec

from tools.assay.composition.catalog import select
from tools.assay.composition.settings import (
    AssaySettings,  # ruff:ignore[typing-only-first-party-import]  # beartype resolves rail annotations at import time
)
from tools.assay.composition.store import (
    ArtifactScope,  # ruff:ignore[typing-only-first-party-import]  # beartype resolves rail annotations at import time
)
from tools.assay.core.exec import Executor  # ruff:ignore[typing-only-first-party-import]  # beartype resolves the executor-port annotation at runtime
from tools.assay.core.model import (
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Completed,  # ruff:ignore[typing-only-first-party-import]  # _findings/_outcomes annotate the ordered fan-out outcomes
    Fault,  # ruff:ignore[typing-only-first-party-import]  # beartype resolves Result[Report, Fault] under PEP 649 at import time
    InprocThunk,  # ruff:ignore[typing-only-first-party-import]  # beartype resolves the _planning return annotation at import time
    Language,
    Match,
    Mode,
    RailStatus,
    receipt,
    Report,  # ruff:ignore[typing-only-first-party-import]  # beartype resolves Report in return annotations at import time
    Runner,
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
    """One engine NDJSON row: ``check`` names the emitting check (validate-mermaid, prose-gate, card-*, research-*)."""

    file: str
    line: int
    status: str
    detail: str = ""
    check: str = ""


# --- [CONSTANTS] ------------------------------------------------------------------------

_FINDING = msgspec.json.Decoder(_Finding)
_FINDING_ROW = msgspec.json.Encoder()
_SEVERITY = {"fail": "error", "warn": "warning"}
# Engine suffix ownership: prose-gate and planning-gate own Markdown only; unlisted engines take every routed docs suffix.
_SUFFIXES: dict[str, frozenset[str]] = {"prose-gate": frozenset((".md",)), "planning-gate": frozenset((".md",))}

# Planning-durable grammar. Card files (IDEAS.md/TASKLOG.md under libs/) carry `[SLUG]-[STATUS]:` leaders whose section
# status set and bullet labels come from each file's own source-only template comment, falling back to the canonical
# vocabularies below; design pages (libs/**/.planning/<sub>/*.md) end on one `## [NN]-[RESEARCH]` section whose rows are
# `- [TOKEN]-[OPEN|BLOCKED]: <question>; <route>` or a settled record form (`- [TOKEN]: <record>`, `- [TOKEN] — <record>`). The gate rejects only
# fake, wrong, or deformed markers — an illegal status, a malformed leader, an unknown bullet label, an absolute /Users/
# path in a card, a deleted, displaced, or duplicated RESEARCH section — never a missing optional field.
_CARD_FILES: frozenset[str] = frozenset(("IDEAS.md", "TASKLOG.md"))
# Ratified card vocabulary: the four core bullets are required on every open card; the rest are optional-by-condition.
_CARD_CORE: frozenset[str] = frozenset(("Capability", "Shape", "Unlocks", "Anchors"))
_CARD_BULLETS: frozenset[str] = frozenset((*_CARD_CORE, "Arms", "Route", "Tension", "Ripple", "Atomic"))
_STATUSES: dict[str, frozenset[str]] = {
    "OPEN": frozenset(("ACTIVE", "QUEUED", "BLOCKED")),
    "CLOSED": frozenset(("COMPLETE", "DROPPED")),
    "RESEARCH": frozenset(("OPEN", "BLOCKED")),
}
_LEADER = re.compile(r"^\[([A-Z0-9_-]+)\]-\[([A-Z]+)\]: *(.*)$")
# Slugs are UPPERCASE_SNAKE with no exceptions; a hyphen-bearing bracketed token is a defect at leaders and references alike.
_SLUG_HYPHEN = re.compile(r"\[[A-Z0-9_]*-[A-Z0-9_-]*\]")
_BULLET = re.compile(r"^- ([A-Z][A-Za-z]*): *(.*)$")
_SECTION = re.compile(r"^## \[\d{1,2}\]-\[([A-Z_]+)\]\s*$")
_HEADING = re.compile(r"^#{1,2} ")
_RESEARCH_HEADER = re.compile(r"^#{1,2} \[\d{1,2}\]-\[RESEARCH\]\s*$")
_RESEARCH_ROW = re.compile(r"^- \[([A-Z0-9_-]+)\](.*)$")
_RESEARCH_TAIL = re.compile(r"^-\[([A-Z]+)\]: *(.*)$")
# Settled record tails: `- [TOKEN]: <record>`, `- [TOKEN] — <record>`, and the qualified `- [TOKEN] (<qualifier>)...` form.
_RESEARCH_RECORD = re.compile(r"^(?:: *| — | \()\S")
# Template-comment leaders (`[ID]-[COMPLETE|DROPPED]:`, `[TOKEN]-[OPEN|BLOCKED]:`) extend the ratified status set;
# the bare `STATUS` placeholder adds nothing, leaving the canonical vocabulary intact.
_TEMPLATE_LEADER = re.compile(r"^\[[A-Z0-9_]+\]-\[([A-Z|]+)\]:")


# --- [ERRORS] ---------------------------------------------------------------------------


class FaultedPromotion(Exception):  # ruff:ignore[error-suffix-on-exception-name]  # sentinel, not an *Error condition: caught at the registry seam, mapped to Fault
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


def _masked(lines: tuple[str, ...]) -> tuple[tuple[bool, bool], ...]:
    # Per-line (comment, fence) membership; a delimiter line counts inside its span so template comments and fence walls never scan as content.
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
    # Ratified status set is fixed law a file's source-only template comment extends, never restricts: a partial or malformed comment
    # adds a status but can never reject a canonical one, so the check survives template evolution without a code edit.
    tracked, declared = "", set()
    for line, (comment, _fence) in zip(lines, flags, strict=True):
        if (head := _SECTION.match(line)) is not None:
            tracked = head.group(1)
        elif comment and tracked == section and (leader := _TEMPLATE_LEADER.match(line)) is not None:
            declared.update(set(leader.group(1).split("|")) - {"STATUS"})
    return frozenset(declared) | _STATUSES[section]


def _template_bullets(lines: tuple[str, ...], flags: tuple[tuple[bool, bool], ...]) -> frozenset[str]:
    # Ratified bullet vocabulary is fixed law a template comment extends, never restricts: the four core bullets card-core requires can
    # never scan as unknown, and a canonical optional label (Arms/Route/Atomic) stays valid whether or not a sparse comment enumerates it.
    labels = frozenset(found.group(1) for line, (comment, _fence) in zip(lines, flags, strict=True) if comment and (found := _BULLET.match(line)))
    return labels | _CARD_BULLETS


def _card_line(rel: str, number: int, line: str, statuses: frozenset[str], bullets: frozenset[str]) -> tuple[_Finding, ...]:
    slugs = tuple(_fail(rel, number, "card-slug", f"hyphenated slug {token}; slugs are UPPERCASE_SNAKE") for token in _SLUG_HYPHEN.findall(line))
    path = (*slugs, *((_fail(rel, number, "card-path", "absolute /Users/ path inside a card"),) if "/Users/" in line else ()))
    match (_LEADER.match(line) if line.startswith("[") else None, _BULLET.match(line)):
        case (None, _) if line.startswith("["):
            return (*path, _fail(rel, number, "card-leader", "malformed card leader; expected [SLUG]-[STATUS]: <thesis>"))
        case (found, _) if found is not None and found.group(2) not in statuses:
            return (*path, _fail(rel, number, "card-status", f"illegal card status [{found.group(2)}]; legal: {'|'.join(sorted(statuses))}"))
        case (found, _) if found is not None and not found.group(3).strip():
            return (*path, _fail(rel, number, "card-leader", "card leader missing thesis"))
        case (None, bullet) if bullet is not None and bullet.group(1) not in bullets:
            vocabulary = ", ".join(sorted(bullets))
            return (*path, _fail(rel, number, "card-bullet", f"unknown card bullet label {bullet.group(1)!r}; template vocabulary: {vocabulary}"))
        case _:
            return path


def _card_rows(rel: str, lines: tuple[str, ...], flags: tuple[tuple[bool, bool], ...]) -> tuple[_Finding, ...]:
    vocab = {section: _template_statuses(lines, flags, section) for section in ("OPEN", "CLOSED")}
    bullets = _template_bullets(lines, flags)
    section = ""
    card: tuple[int, set[str]] | None = None  # (leader line, seen bullet labels) of the open-section card being read
    rows: list[_Finding] = []

    def closed(next_card: tuple[int, set[str]] | None) -> tuple[int, set[str]] | None:
        # Every open card carries the four core bullets; closing a card at the next leader, section, or EOF settles the law.
        if card is not None and (missing := _CARD_CORE - card[1]):
            rows.append(_fail(rel, card[0], "card-core", f"open card missing required bullet(s): {', '.join(sorted(missing))}"))
        return next_card

    for number, (line, (comment, fence)) in enumerate(zip(lines, flags, strict=True), start=1):
        if (head := _SECTION.match(line)) is not None:
            card, section = closed(None), head.group(1)
        elif not comment and not fence and section in vocab:
            if line.startswith("["):
                well_formed = (found := _LEADER.match(line)) is not None and found.group(2) in vocab[section] and bool(found.group(3).strip())
                card = closed((number, set()) if section == "OPEN" and well_formed else None)
            elif card is not None and (bullet := _BULLET.match(line)) is not None:
                card[1].add(bullet.group(1))
            rows.extend(_card_line(rel, number, line, vocab[section], bullets))
    closed(None)
    return tuple(rows)


def _research_row(rel: str, number: int, line: str, statuses: frozenset[str]) -> tuple[_Finding, ...]:
    # Well-formed row shapes: the open-question row `- [TOKEN]-[STATUS]: <question>; <route>` and the settled record
    # tails `_RESEARCH_RECORD` admits; anything else starting `- [` is a deformed row.
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
        # Orphan evidence stays precise: only the full canonical row shape — legal status plus the `; <route>` tail — marks a deleted section.
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
    # Row grammar binds inside the section alone: the scan stops at the first heading after the RESEARCH header.
    stop = next((number for number, line, (_comment, fence) in after if not fence and _HEADING.match(line)), len(lines) + 1)
    rows = tuple(
        finding
        for number, line, (comment, fence) in after
        if number < stop and not fence and not comment and line.startswith("- [")
        for finding in _research_row(rel, number, line, statuses)
    )
    return (*deformed, *duplicates, *displaced, *rows)


def _planning_findings(rel: str, root: Path) -> tuple[_Finding, ...] | None:
    # Planning durables live under libs/: card files anywhere, design pages one level below a .planning folder.
    parts = PurePosixPath(rel).parts
    card = bool(parts) and parts[0] == "libs" and parts[-1] in _CARD_FILES
    page = bool(parts) and parts[0] == "libs" and not card and ".planning" in parts[:-1] and parts.index(".planning") < len(parts) - 2
    if not (card or page):
        return None
    try:
        text = (root / rel).read_text(encoding="utf-8", errors="replace")
    except OSError:
        return None
    lines = tuple(text.splitlines())
    flags = _masked(lines)
    return _card_rows(rel, lines, flags) if card else _research_rows(rel, lines, flags)


def _planning(root: Path) -> InprocThunk:
    """Build the INPROC planning-gate thunk validating card markers and RESEARCH sections over planning durables.

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
    # Each engine reads its file and writes to its own cache; assay passes only the input, never a sink placement.
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
