#!/usr/bin/env python3
# ruff: noqa: T201 — stdout is this gate's output contract
"""Deterministic conformance gate over durable artifact instances. Exit 0 iff no fail.

Usage: check_instance.py [--json] <instance.md>...
Kind is detected from the H1 suffix token (_DECISIONS, _DIRECTIONS, _ROADMAP, _BLINDSPOTS,
_CAPABILITIES) and bound to its template. Checks: kind (H1 resolves to a known template),
slot-residue (unfilled template slots outside fences and code spans), heading-census (section
token sequence equals the template's, sanctioned optional sections may be absent, numbering
sequential from 01, present sections in template order), leader-vocab (leader tokens drawn from
the template's declared vocabularies, plus the axis catalog for the blindspot ledger).
Templates themselves fail slot-residue by construction;
the gate takes instances. Output: `file:line: FAIL <check> <detail>` per hit; --json emits
NDJSON rows {"file","line","check","status","detail"}. An unreadable file emits one read fail.
"""

# --- [RUNTIME_PRELUDE] -------------------------------------------------------------------

import argparse
import json
from pathlib import Path
import re
import sys
from typing import NamedTuple


# --- [CONSTANTS] -------------------------------------------------------------------------

AXES_PAGE = Path(__file__).resolve().parent.parent / "references" / "axes.md"
TEMPLATE_DIR = Path(__file__).resolve().parent.parent / "templates"
KIND_BY_SUFFIX = {
    "DECISIONS": "decision-record",
    "DIRECTIONS": "direction-set",
    "ROADMAP": "roadmap-brief",
    "BLINDSPOTS": "blindspot-ledger",
    "CAPABILITIES": "capability-entry",
}
OPTIONAL_SECTIONS = {"direction-set": {"WARGAME"}}
FENCE = re.compile(r"^\s*(`{3,}|~{3,})")
H1 = re.compile(r"^# \[(.+)\]\s*$")
HEADING = re.compile(r"^## \[(\d{2})\]-\[([A-Z0-9_]+)\]\s*$")
LEADER = re.compile(r"^\s*- \[([^\]]+)\](?:-\[([^\]]+)\])?(?:-\[([^\]]+)\])?:")
LEADER_ID = re.compile(r"^[A-Z]?\d{2,}$")
CODE_SPAN = re.compile(r"`+[^`]*`+")
SLOT = re.compile(r"<[a-z][a-z0-9|]*(?:-[a-z0-9|<>]+)*>")
VOCAB_LINE = re.compile(r"^\[([A-Z_]+)\]:(.+)$")
VOCAB_TOKEN = re.compile(r"`\[([A-Z0-9_]+)(?::<[a-z-]+>)?\]`")


# --- [MODELS] ----------------------------------------------------------------------------

class Row(NamedTuple):
    """One conformance finding projected to an NDJSON fail row."""

    line: int
    check: str
    detail: str


# --- [OPERATIONS] ------------------------------------------------------------------------

def headings(text: str) -> list[tuple[int, str, str]]:
    """Extract section headings as (line, number, token) outside fences.

    Returns:
        Heading triples in document order.
    """
    out: list[tuple[int, str, str]] = []
    in_fence = False
    for n, line in enumerate(text.splitlines(), 1):
        if FENCE.match(line):
            in_fence = not in_fence
            continue
        if not in_fence and (m := HEADING.match(line)):
            out.append((n, m.group(1), m.group(2)))
    return out


def vocabulary(kind: str) -> set[str]:
    """Collect the leader-token vocabulary: template declarations plus axis tokens.

    A declared token carrying a `:<slot>` tail admits any instance token sharing its prefix.

    Returns:
        Allowed uppercase tokens and prefixes.
    """
    allowed: set[str] = set()
    template = TEMPLATE_DIR / f"{kind}.md"
    for line in template.read_text(encoding="utf-8", errors="replace").splitlines():
        if VOCAB_LINE.match(line):
            allowed.update(m.group(1) for m in VOCAB_TOKEN.finditer(line))
    if kind == "blindspot-ledger" and AXES_PAGE.is_file():
        allowed.update(token for _, _, token in headings(AXES_PAGE.read_text(encoding="utf-8", errors="replace")))
    return allowed


def scan(path: Path) -> list[Row]:
    """Run all conformance checks over one instance file.

    Returns:
        Finding rows in scan order.
    """
    try:
        text = path.read_text(encoding="utf-8", errors="replace")
    except OSError as exc:
        return [Row(0, "read", type(exc).__name__)]
    rows: list[Row] = []
    lines = text.splitlines()
    h1 = next((m for line in lines if (m := H1.match(line))), None)
    suffix = h1.group(1).rsplit("_", 1)[-1] if h1 else ""
    kind = KIND_BY_SUFFIX.get(suffix, "")
    if not kind:
        return [Row(1, "kind", f"H1 suffix {suffix or 'missing'} resolves to no template")]

    allowed = vocabulary(kind)
    prefixes = {t.split(":")[0] for t in allowed}
    in_fence = False
    for n, line in enumerate(lines, 1):
        if FENCE.match(line):
            in_fence = not in_fence
            continue
        if in_fence:
            continue
        rows.extend(Row(n, "slot-residue", m.group(0)) for m in SLOT.finditer(CODE_SPAN.sub(" ", line)))
        if (m := LEADER.match(line)) is not None:
            if m.group(2) and not LEADER_ID.match(m.group(1)):
                rows.append(Row(n, "leader-id", f"[{m.group(1)}] outside the id grammar"))
            rows.extend(
                Row(n, "leader-vocab", f"[{token}] outside declared vocabulary")
                for token in (m.group(2), m.group(3))
                if token and token.split(":")[0] not in prefixes
            )

    template_text = (TEMPLATE_DIR / f"{kind}.md").read_text(encoding="utf-8", errors="replace")
    want = [token for _, _, token in headings(template_text)]
    got = headings(text)
    optional = OPTIONAL_SECTIONS.get(kind, set())
    required = [t for t in want if t not in optional]
    got_tokens = [t for _, _, t in got]
    present = set(got_tokens)
    if not set(required) <= present or [t for t in want if t in present] != got_tokens:
        rows.append(Row(0, "heading-census", f"sections {got_tokens} against template {want}"))
    for i, (n, num, _) in enumerate(got, 1):
        if int(num) != i:
            rows.append(Row(n, "heading-census", f"number [{num}] out of sequence, expected [{i:02d}]"))
    return rows


# --- [COMPOSITION] -----------------------------------------------------------------------

def main(argv: list[str]) -> int:
    """Run the conformance gate over argv instances; emit one row per hit.

    Returns:
        Process exit code: 0 clean, 1 any fail.
    """
    ap = argparse.ArgumentParser(add_help=True)
    ap.add_argument("--json", action="store_true")
    ap.add_argument("paths", nargs="+")
    ns = ap.parse_args(argv)

    failed = False
    for raw in ns.paths:
        path = Path(raw)
        for row in scan(path):
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
