#!/usr/bin/env python3
# ruff: noqa: T201 — stdout is this gate's output contract
"""Deterministic prose gates over durable markdown. Exit 0 iff no fail.

Usage: prose_gate.py [--json] [--cap N] <path>...
Paths: .md files or directories (walked for *.md). Checks: fence-geometry (line-length cap inside
codemap/seams/tree-glyph fences and README package card rows), hedge (word-boundary banned list),
meta-phrase (fixed self-description/provenance list), self-count (sentence-initial counts of own
content), list-bloat (entry sentence/char budget, roster entries exempt by code-span share),
table-header (every header-row cell is a bracketed `[UPPER_SNAKE]` token) — prose lines only
throughout. Output: `file:line: FAIL <check> <detail>` per hit; --json emits
NDJSON rows {"file","line","check","status","detail"}. An unreadable file emits one check=read
fail row. Judgment-tier defects (enumeration anchoring, altitude) are review work, not gates.
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
# qualifier, closed by a colon; prose merely opening with a leader word carries no colon and is scanned.
EXAMPLE_LINE = re.compile(r"^\s*-?\s*(?:Rejected|Accepted|Near miss|Banned|Survivors)(?:\s*\([^)]*\))?:")
# List-entry budget from corpus evidence; roster-shaped entries (mostly code spans) are registry enumerations.
LIST_CHAR_CAP = 500
LIST_ITEM = re.compile(r"^\s*(-|\d+\.)\s+\S")
LIST_SENTENCE_CAP = 3
ROSTER_SPAN_SHARE = 0.6
SENTENCE_END = re.compile(r"[.!?](?:\s|$)")
FENCE = re.compile(r"^(\s*)(`{3,}|~{3,})\s*(\S*)")
MARKER_WORDS = re.compile(r"\b(TBD|TODO|FIXME)\b")
CARD_ROW = re.compile(r"^\s*-\s+`[^`]+`\s+-\s+")
CODE_SPAN = re.compile(r"`+[^`]*`+|\"[^\"]*\"")
# Table header cells carry the bracketed token form; a header row is the row a separator row follows.
HEADER_CELL = re.compile(r"^\[[A-Z0-9_]+\]$")
TABLE_SEP = re.compile(r"^\s*\|(?:\s*:?-{3,}:?\s*\|)+\s*$")
GLYPHS = ("│", "├", "└", "⇄")
HEDGE_PHRASES = (
    "is expected to", "can be", "aims to", "is designed to",
    "in the future", "eventually", "as needed", "if necessary",
)
HEDGE_WORDS = re.compile(
    r"\b(should|could|would|might|maybe|perhaps|likely|probably|propose|consider|recommended"
    r"|ideally|we|our|you)\b",
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
META_PHRASES = (
    "this document", "this file describes", "this page describes", "as mentioned above",
    "as described above", "note that", "it is worth", "in this section", "the following sections",
    "as of 20", "per research", "at the time of writing",
)


# --- [MODELS] ----------------------------------------------------------------------------

class Row(NamedTuple):
    """One prose-gate finding projected to an NDJSON fail row."""

    line: int
    check: str
    detail: str


# --- [OPERATIONS] ------------------------------------------------------------------------

def collect(paths: list[str]) -> list[Path]:
    """Expand argv paths to markdown files, directories walked recursively.

    Returns:
        Unique files in first-seen order.
    """
    files: list[Path] = []
    for raw in paths:
        p = Path(raw)
        if p.is_dir():
            files.extend(sorted(p.rglob("*.md")))
        elif p.suffix == ".md" and p.is_file():
            files.append(p)
    return list(dict.fromkeys(files))


def scan(path: Path, cap: int) -> list[Row]:
    """Scan one file for fence-geometry, hedge, and meta-phrase hits.

    Mention is not use: inline code spans are stripped and example-leader lines (Accepted,
    Rejected, Near miss, Banned, Survivors) are exempt before the list-bloat, hedge, meta, and
    self-count scans — quoting a banned word to legislate it is legal; the convention for feature
    vocabulary is backticks. An unreadable file yields one read fault so the run reports and
    fails rather than crashing.

    Returns:
        Finding rows in scan order.
    """
    try:
        text = path.read_text(encoding="utf-8", errors="replace")
    except OSError as exc:
        return [Row(0, "read", type(exc).__name__)]
    rows: list[Row] = []
    in_fence = False
    in_frontmatter = False
    fence_marker = ""
    fence_capped = False
    is_readme = path.name == "README.md"
    lines = text.splitlines()
    for n, line in enumerate(lines, 1):
        if n == 1 and line.rstrip() == "---":
            in_frontmatter = True
            continue
        if in_frontmatter:
            if line.rstrip() == "---":
                in_frontmatter = False
            continue
        m = FENCE.match(line)
        if m and not in_fence:
            in_fence, fence_marker = True, m.group(2)[0] * len(m.group(2))
            info = m.group(3).lower()
            fence_capped = "codemap" in info or "seams" in info
            continue
        if in_fence:
            if m and not m.group(3) and line.strip().startswith(fence_marker):
                in_fence = False
            elif (fence_capped or any(g in line for g in GLYPHS)) and len(line) > cap:
                rows.append(Row(n, "fence-geometry", f"line {len(line)} > cap {cap}"))
            continue
        if is_readme and CARD_ROW.match(line) and len(line) > cap:
            rows.append(Row(n, "fence-geometry", f"card row {len(line)} > cap {cap}"))
        if line.lstrip().startswith("|") and n < len(lines) and TABLE_SEP.match(lines[n]):
            rows.extend(
                Row(n, "table-header", cell)
                for cell in (c.strip() for c in line.strip().strip("|").split("|"))
                if cell and not HEADER_CELL.match(cell)
            )
        if EXAMPLE_LINE.match(line):
            continue
        prose = CODE_SPAN.sub(" ", line)
        if LIST_ITEM.match(line):
            span_share = 1 - (len(prose.strip()) / max(1, len(line.strip())))
            sentences = len(SENTENCE_END.findall(prose))
            if span_share < ROSTER_SPAN_SHARE:
                if sentences > LIST_SENTENCE_CAP:
                    rows.append(Row(n, "list-bloat", f"{sentences} sentences > cap {LIST_SENTENCE_CAP}"))
                elif len(line) > LIST_CHAR_CAP:
                    rows.append(Row(n, "list-bloat", f"entry {len(line)} chars > cap {LIST_CHAR_CAP}"))
        low = prose.lower()
        rows.extend(Row(n, "hedge", w.group(0)) for w in HEDGE_WORDS.finditer(prose))
        rows.extend(Row(n, "hedge", w.group(0)) for w in MARKER_WORDS.finditer(prose))
        rows.extend(Row(n, "hedge", phrase) for phrase in HEDGE_PHRASES if phrase in low)
        rows.extend(Row(n, "meta-phrase", phrase) for phrase in META_PHRASES if phrase in low)
        rows.extend(Row(n, "self-count", w.group(0).lstrip(".!?— ")) for w in SELF_COUNT.finditer(prose))
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

    failed = False
    for path in collect(ns.paths):
        for row in scan(path, ns.cap):
            failed = True
            if ns.json:
                print(json.dumps(
                    {"file": str(path), "line": row.line, "check": row.check, "status": "fail", "detail": row.detail}
                ))
            else:
                print(f"{path}:{row.line}: FAIL {row.check} {row.detail}")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
