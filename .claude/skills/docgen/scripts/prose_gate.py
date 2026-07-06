#!/usr/bin/env python3
# ruff: noqa: T201 — stdout is this gate's output contract
"""Deterministic prose gates over durable markdown. Exit 0 iff no fail.

Usage: prose_gate.py [--json] [--cap N] <path>...
Paths: .md files or directories (walked for *.md); a missing path, non-markdown file, or
markdown-free directory emits a check=collect fail row — the gate never exits clean on
unscanned input. Checks: fence-geometry (line-length cap inside codemap/seams/tree-glyph
fences and README package card rows), hedge (word-bounded banned words and phrases),
meta-phrase, self-count (sentence-initial counts of own content), list-bloat (entry
sentence/char budget, roster entries exempt by code-span share), table-header (every
header-row cell is a bracketed `[UPPER_SNAKE]` token, separator arity must match),
version-anchor (semver triples, release bands, v-prefixed versions; the manifest owns pins) —
prose lines only throughout. Output: `file:line: FAIL <check> <detail>` per hit; --json
emits NDJSON rows {"file","line","check","status","detail"}. Unreadable or invalid-UTF-8
input emits one check=read fail row. Judgment-tier defects (enumeration anchoring,
altitude) are review work, not gates.
"""

# --- [RUNTIME_PRELUDE] -------------------------------------------------------------------

import argparse
import json
from pathlib import Path
import re
import sys
from typing import NamedTuple


# --- [CONSTANTS] -------------------------------------------------------------------------

CAP = 160
# Leader exemption is the record form only — the canonical word plus an optional parenthetical
# qualifier, closed by a colon; blockquoted, numbered, and bulleted example leaders all qualify.
EXAMPLE_LINE = re.compile(
    r"^\s*(?:>\s*)?(?:[-+*]|\d+[.)])?\s*(?:Rejected|Accepted|Near miss|Banned|Survivors)(?:\s*\([^)]*\))?:"
)
# List-entry budget from corpus evidence; roster-shaped entries (mostly code spans) are registry enumerations.
LIST_CHAR_CAP = 500
LIST_ITEM = re.compile(r"^\s*(?:[-+*]|\d+[.)])\s+\S")
LIST_SENTENCE_CAP = 3
ROSTER_SPAN_SHARE = 0.6
SENTENCE_END = re.compile(r"[.!?](?:\s|$)")
CARD_ROW = re.compile(r"^\s*-\s+`[^`]+`\s+-\s+")
# A fence opener indents at most 3 spaces; the full info string is significant (`text codemap`).
FENCE = re.compile(r"^(?P<indent> {0,3})(?P<marker>`{3,}|~{3,})(?P<info>.*)$")
GLYPHS = ("│", "├", "└", "⇄")
# Table header cells carry the bracketed token form; a header row is the row a separator row follows.
HEADER_CELL = re.compile(r"^\[[A-Z0-9_]+\]$")
TABLE_SEP = re.compile(r"^\s*\|(?:\s*:?-+:?\s*\|)+\s*$")
QUOTED_SPAN = re.compile(r'"[^"]*"')
YAML_KEY = re.compile(r"^[A-Za-z_][A-Za-z0-9_-]*\s*:")
HEDGE_PHRASE = re.compile(
    r"\b(?:is\s+expected\s+to|can\s+be|aims\s+to|is\s+designed\s+to|in\s+the\s+future"
    r"|eventually|as\s+needed|if\s+necessary)\b",
    re.IGNORECASE,
)
HEDGE_WORDS = re.compile(
    r"\b(should|could|would|might|maybe|perhaps|likely|probably|propose|consider|recommended"
    r"|ideally|experimental|we|our|you)\b",
    re.IGNORECASE,
)
MARKER_WORDS = re.compile(r"\b(TBD|TODO|FIXME)\b", re.IGNORECASE)
META_PHRASE = re.compile(
    r"\b(?:this\s+document|this\s+file\s+describes|this\s+page\s+describes|as\s+mentioned\s+above"
    r"|as\s+described\s+above|note\s+that|it\s+is\s+worth|in\s+this\s+section"
    r"|the\s+following\s+sections|as\s+of\s+20|per\s+research|at\s+the\s+time\s+of\s+writing)\b",
    re.IGNORECASE,
)
# Self-mirroring counts open a sentence about own content ("Fifteen classes are ..."); mid-sentence
# quantities are thresholds, consequences, or definitions and stay legal — judgment-tier, not gated.
SELF_COUNT = re.compile(
    r"(?:^|[.!?—]\s+)(Two|Three|Four|Five|Six|Seven|Eight|Nine|Ten|Eleven|Twelve|Thirteen"
    r"|Fourteen|Fifteen|Sixteen|Seventeen|Eighteen|Nineteen|Twenty|\d+)\s+(named\s+)?(classes|laws"
    r"|rules|sections|types|axes|fields|modes|tests|checks|steps|entries|forms|tiers|bands|devices"
    r"|archetypes|templates|references|tables|diagrams|cards|rows|columns|tokens|markers|vocabularies)\b"
)
# A release band (2.3+), a semver triple, or a v-prefixed version in prose; the manifest owns pins.
VERSION_ANCHOR = re.compile(r"\bv?\d+\.\d+(?:\.\d+)+\b|\b\d+\.\d+(?:\.\d+)?\+|\bv\d+\.\d+\b")
PATTERN_GATES: tuple[tuple[str, re.Pattern[str]], ...] = (
    ("hedge", HEDGE_WORDS),
    ("hedge", MARKER_WORDS),
    ("hedge", HEDGE_PHRASE),
    ("meta-phrase", META_PHRASE),
    ("self-count", SELF_COUNT),
    ("version-anchor", VERSION_ANCHOR),
)


# --- [MODELS] ----------------------------------------------------------------------------

class Fence(NamedTuple):
    """Open-fence state: closing requires the same glyph at >= width with an empty info string."""

    glyph: str
    width: int
    capped: bool

    def closed_by(self, m: re.Match[str] | None) -> bool:
        """Decide whether the matched line closes this fence.

        Returns:
            True when the marker shares the glyph at >= width with an empty info string.
        """
        return bool(
            m and m.group("marker")[0] == self.glyph
            and len(m.group("marker")) >= self.width
            and not m.group("info").strip()
        )


class Row(NamedTuple):
    """One prose-gate finding projected to an NDJSON fail row."""

    line: int
    check: str
    detail: str


# --- [OPERATIONS] ------------------------------------------------------------------------

def collect(paths: list[str]) -> tuple[list[Path], list[tuple[Path, Row]]]:
    """Expand argv paths to markdown files plus collection faults for unresolvable input.

    Returns:
        Unique files in first-seen order, and one fault row per missing path, non-markdown
        file, or directory holding no markdown — unscanned input is a failure, never silence.
    """
    files: list[Path] = []
    faults: list[tuple[Path, Row]] = []
    for raw in paths:
        p = Path(raw)
        if p.is_dir():
            found = sorted(p.rglob("*.md"))
            files.extend(found)
            if not found:
                faults.append((p, Row(0, "collect", "directory holds no markdown")))
        elif p.suffix == ".md" and p.is_file():
            files.append(p)
        else:
            faults.append((p, Row(0, "collect", "not a readable markdown file")))
    return list(dict.fromkeys(files)), faults


def frontmatter_end(lines: list[str]) -> int:
    """Return the 1-based line number of the closing frontmatter delimiter, 0 when absent.

    Frontmatter requires a first-line `---`, a closing `---`, and at least one YAML-shaped key
    between them; a leading horizontal rule or an unclosed block is body, never a skip region.
    """
    if not lines or lines[0].rstrip() != "---":
        return 0
    for n, line in enumerate(lines[1:], 2):
        if line.rstrip() == "---":
            return n if any(YAML_KEY.match(body) for body in lines[1 : n - 1]) else 0
    return 0


def strip_mentions(line: str) -> str:
    """Blank inline code spans (delimiter-length matched) and double-quoted spans.

    Returns:
        The line with mention spans replaced by single spaces; unpaired backtick runs stay text.
    """
    parts: list[str] = []
    i = 0
    while i < len(line):
        if line[i] != "`":
            parts.append(line[i])
            i += 1
            continue
        run = line[i:]
        width = len(run) - len(run.lstrip("`"))
        end = line.find("`" * width, i + width)
        if end < 0:
            parts.append(line[i])
            i += 1
        else:
            parts.append(" ")
            i = end + width
    return QUOTED_SPAN.sub(" ", "".join(parts))


def scan(path: Path, cap: int) -> list[Row]:
    """Scan one file for every gate check.

    Mention is not use: inline code spans and double-quoted spans are stripped and
    example-leader lines are exempt before the semantic scans — quoting a banned word to
    legislate it is legal; the convention for feature vocabulary is backticks. Unreadable
    or invalid-UTF-8 input yields one read fault so the run reports and fails.

    Returns:
        Finding rows in scan order.
    """
    try:
        text = path.read_text(encoding="utf-8")
    except (OSError, UnicodeDecodeError) as exc:
        return [Row(0, "read", type(exc).__name__)]
    rows: list[Row] = []
    lines = text.splitlines()
    skip_until = frontmatter_end(lines)
    fence: Fence | None = None
    is_readme = path.name == "README.md"
    for n, line in enumerate(lines, 1):
        if n <= skip_until:
            continue
        m = FENCE.match(line)
        if m and fence is None:
            marker, info = m.group("marker"), m.group("info").strip().lower()
            fence = Fence(marker[0], len(marker), "codemap" in info or "seams" in info)
            continue
        if fence is not None:
            if fence.closed_by(m):
                fence = None
            elif (fence.capped or any(g in line for g in GLYPHS)) and len(line) > cap:
                rows.append(Row(n, "fence-geometry", f"line {len(line)} > cap {cap}"))
            continue
        if is_readme and CARD_ROW.match(line) and len(line) > cap:
            rows.append(Row(n, "fence-geometry", f"card row {len(line)} > cap {cap}"))
        if line.lstrip().startswith("|") and n < len(lines) and TABLE_SEP.match(lines[n]):
            cells = [c.strip() for c in line.strip().strip("|").split("|")]
            seps = [c.strip() for c in lines[n].strip().strip("|").split("|")]
            rows.extend(Row(n, "table-header", cell or "<empty>") for cell in cells if not HEADER_CELL.match(cell))
            if len(seps) != len(cells):
                rows.append(Row(n, "table-header", f"separator cells {len(seps)} != header cells {len(cells)}"))
        if EXAMPLE_LINE.match(line):
            continue
        prose = strip_mentions(line)
        if LIST_ITEM.match(line):
            span_share = 1 - (len(prose.strip()) / max(1, len(line.strip())))
            sentences = len(SENTENCE_END.findall(prose))
            if span_share < ROSTER_SPAN_SHARE:
                if sentences > LIST_SENTENCE_CAP:
                    rows.append(Row(n, "list-bloat", f"{sentences} sentences > cap {LIST_SENTENCE_CAP}"))
                elif len(line) > LIST_CHAR_CAP:
                    rows.append(Row(n, "list-bloat", f"entry {len(line)} chars > cap {LIST_CHAR_CAP}"))
        rows.extend(
            Row(n, check, hit.group(0).lstrip(".!?— "))
            for check, pattern in PATTERN_GATES
            for hit in pattern.finditer(prose)
        )
    return rows


# --- [COMPOSITION] -----------------------------------------------------------------------

def main(argv: list[str]) -> int:
    """Run the gates over argv paths; emit one row per hit.

    Returns:
        Process exit code: 0 clean, 1 any fail.
    """
    ap = argparse.ArgumentParser(add_help=True)
    ap.add_argument("--json", action="store_true")
    ap.add_argument("--cap", type=int, default=CAP)
    ap.add_argument("paths", nargs="+")
    ns = ap.parse_args(argv)

    def emit(path: Path, row: Row) -> None:
        if ns.json:
            print(json.dumps(
                {"file": str(path), "line": row.line, "check": row.check, "status": "fail", "detail": row.detail}
            ))
        else:
            print(f"{path}:{row.line}: FAIL {row.check} {row.detail}")

    files, faults = collect(ns.paths)
    failed = bool(faults)
    for path, row in faults:
        emit(path, row)
    for path in files:
        for row in scan(path, ns.cap):
            failed = True
            emit(path, row)
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
