#!/usr/bin/env python3
# ruff: noqa: T201 — stdout is this gate's output contract
"""Deterministic conformance gate over durable artifact instances. Exit 0 iff no fail.

Usage: check_instance.py [--json] <instance.md>...
Kind binds from the first content line, which must be the template H1 (suffix _DECISIONS,
_DIRECTIONS, _ROADMAP, _BLINDSPOTS, _CAPABILITIES); the template is the schema — sections,
per-section field names, and leader vocabularies derive from it at run time. Checks: collect
(unresolvable argv path), read (unreadable or invalid-UTF-8 input), template (unreadable
schema source), kind, slot-residue (unfilled template slots outside fences and code spans),
heading-census (missing, unknown, misordered, or misnumbered sections), leader-id (entry ids
are an optional capital kind letter then a zero-padded ordinal), leader-vocab (leader tokens
drawn from the template's declared vocabularies — a parameterized token such as
`[SUPERSEDED:<id>]` requires a well-formed id tail — plus the axis catalog for the blindspot
ledger), leader-unique (an entry id mints once per section; a cross-section repeat is a
reference), field-vocab (entry field names drawn from the template's fields for the active
section), mark-vocab (a field whose template value leads with a `[<VOCAB>]` slot requires the
instance field to lead with a token drawn from that declared vocabulary), entry-floor (a
non-blindspot entry-bearing artifact carries at least one entry — a fully conformant but
empty artifact is vacuous), section-span (a direction set spans at least two distinct tiers),
section-coverage (a direction set's wargame, when present, scores exactly the declared
directions — no unscored direction, no phantom score row), wargame-criteria (a direction
set's wargame criteria tokens are distinct — a repeated criterion double-counts its weight).
Output:
`file:line: FAIL <check> <detail>` per hit; --json emits NDJSON rows
{"file","line","check","status","detail"}.
"""

# --- [RUNTIME_PRELUDE] -------------------------------------------------------------------

import argparse
from dataclasses import dataclass
from enum import StrEnum
import json
from pathlib import Path
import re
import sys


# --- [TYPES] -------------------------------------------------------------------------------

class Check(StrEnum):
    """Closed check vocabulary; every emitted row names one member."""

    COLLECT = "collect"
    READ = "read"
    TEMPLATE = "template"
    KIND = "kind"
    SLOT_RESIDUE = "slot-residue"
    HEADING_CENSUS = "heading-census"
    LEADER_ID = "leader-id"
    LEADER_UNIQUE = "leader-unique"
    LEADER_VOCAB = "leader-vocab"
    FIELD_VOCAB = "field-vocab"
    MARK_VOCAB = "mark-vocab"
    ENTRY_FLOOR = "entry-floor"
    SECTION_SPAN = "section-span"
    SECTION_COVERAGE = "section-coverage"
    WARGAME_CRITERIA = "wargame-criteria"


# --- [CONSTANTS] ---------------------------------------------------------------------------

KIND_BY_SUFFIX = {
    "DECISIONS": "decision-record",
    "DIRECTIONS": "direction-set",
    "ROADMAP": "roadmap-brief",
    "BLINDSPOTS": "blindspot-ledger",
    "CAPABILITIES": "capability-entry",
}
OPTIONAL_SECTIONS = {"direction-set": frozenset({"WARGAME"})}
# Kind-bound entry floor: a non-blindspot entry-bearing artifact carries at least this many entries; zero is vacuous.
ENTRY_FLOOR_BY_KIND = {"decision-record": 1, "roadmap-brief": 1, "capability-entry": 1}
# Kind-bound span law: (section, minimum) — the section needs that many entries with distinct primary tokens.
SECTION_SPAN_RULES = {"direction-set": ("DIRECTIONS", 2)}
# Kind-bound coverage law: (source, cover) — when the optional cover section is present its entry ids equal the source's.
SECTION_COVERAGE_RULES = {"direction-set": ("DIRECTIONS", "WARGAME")}
# Kind-bound criteria law: (section, bullet) — the wargame criteria bullet's bracket tokens are distinct.
WARGAME_CRITERIA_RULES = {"direction-set": ("WARGAME", "Criteria")}
BRACKET_TOKEN = re.compile(r"\[([A-Z0-9_]+)\]")
# A section-level bullet sits at column zero with a labeled field but no id — the frame and wargame carry these.
BULLET = re.compile(r"^- (?P<name>[A-Z][A-Za-z-]*): (?P<value>\S.*)$")
CODE_SPAN = re.compile(r"`+[^`]*`+")
# An entry leader sits at column zero; indented bracket bullets are field content, never entries.
ENTRY = re.compile(r"^- \[(?P<id>[^\]]+)\](?:-\[(?P<a>[^\]]+)\])?(?:-\[(?P<b>[^\]]+)\])?:")
# A fence opens at any indent — field continuations nest fenced snippets — and closes on its own glyph at >= width.
FENCE = re.compile(r"^(?P<indent> *)(?P<marker>`{3,}|~{3,})(?P<info>.*)$")
# A field line is exactly two-space indented; deeper lines are continuations of the field above.
FIELD = re.compile(r"^  - (?P<name>[A-Z][A-Za-z-]*): (?P<value>\S.*)$")
H1 = re.compile(r"^# \[(.+)\]\s*$")
HEADING = re.compile(r"^## \[(\d{2})\]-\[([A-Z0-9_]+)\]\s*$")
LEADER_ID = re.compile(r"^[A-Z]?\d{2,}$")
# A mark-slotted template value opens with `[<VOCAB>]`; the instance value opens with `[TOKEN]`.
MARK_LEAD = re.compile(r"^\[([A-Z0-9_]+)\]")
MARK_SLOT = re.compile(r"^\[<([A-Z_]+)>\]")
SLOT = re.compile(r"<[A-Za-z][A-Za-z0-9|]*(?:[-_][A-Za-z0-9|<>]+)*>")
VOCAB_LINE = re.compile(r"^\[([A-Z_]+)\]:(.+)$")
VOCAB_TOKEN = re.compile(r"`\[([A-Z0-9_]+)(:<[a-z-]+>)?\]`")


# --- [MODELS] ------------------------------------------------------------------------------

@dataclass(frozen=True, slots=True, kw_only=True)
class Line:
    """One content line admitted past fence and frontmatter state."""

    number: int
    text: str


@dataclass(frozen=True, slots=True, kw_only=True)
class Row:
    """One conformance finding projected to an NDJSON fail row."""

    line: int
    check: Check
    detail: str


@dataclass(frozen=True, slots=True, kw_only=True)
class Schema:
    """The template-derived contract one kind enforces: sections, fields, leader and mark vocabularies."""

    sections: tuple[str, ...]
    optional: frozenset[str]
    fields: dict[str, frozenset[str]]
    leaders: dict[str, bool]
    marks: dict[str, dict[str, bool]]


# --- [BOUNDARIES] --------------------------------------------------------------------------

SKILL_ROOT = Path(__file__).resolve().parent.parent
AXES_PAGE = SKILL_ROOT / "references" / "axes.md"
TEMPLATE_DIR = SKILL_ROOT / "templates"


# --- [OPERATIONS] --------------------------------------------------------------------------

def fence_closed(fence: tuple[str, int], m: re.Match[str] | None) -> bool:
    """Decide whether the matched line closes the open fence.

    Returns:
        True when the marker shares the glyph at >= width with an empty info string.
    """
    return bool(
        m and m.group("marker")[0] == fence[0]
        and len(m.group("marker")) >= fence[1]
        and not m.group("info").strip()
    )


def content_lines(text: str) -> tuple[Line, ...]:
    """Admit lines outside fenced blocks; a fence closes only on its own glyph at >= width.

    Returns:
        Numbered content lines in document order.
    """
    admitted: list[Line] = []
    fence: tuple[str, int] | None = None
    for number, raw in enumerate(text.splitlines(), 1):
        m = FENCE.match(raw)
        if m and fence is None:
            fence = (m.group("marker")[0], len(m.group("marker")))
            continue
        if fence is not None:
            if fence_closed(fence, m):
                fence = None
            continue
        admitted.append(Line(number=number, text=raw))
    return tuple(admitted)


def headings(lines: tuple[Line, ...]) -> tuple[tuple[int, str, str], ...]:
    """Project section headings from admitted lines.

    Returns:
        (line, number, token) triples in document order.
    """
    return tuple((line.number, m.group(1), m.group(2)) for line in lines if (m := HEADING.match(line.text)))


def schema_for(kind: str) -> Schema | Row:
    """Derive the kind's schema from its template file; the template is the single source.

    Section order and per-section field names come from the template body; each `[NAME]:`
    vocabulary line declares a named token set, where a `:<id>` tail marks a token as
    id-parameterized. A field whose template value leads with a `[<NAME>]` slot binds that
    field to the `NAME` mark vocabulary; every other named vocabulary is a leader vocabulary,
    and the blindspot ledger additionally admits every axis heading token as a leader token.

    Returns:
        The derived schema, or one template fault row when a schema source is unreadable.
    """
    try:
        template = content_lines((TEMPLATE_DIR / f"{kind}.md").read_text(encoding="utf-8"))
        axis_text = AXES_PAGE.read_text(encoding="utf-8") if kind == "blindspot-ledger" and AXES_PAGE.is_file() else ""
    except (OSError, UnicodeDecodeError) as exc:
        return Row(line=0, check=Check.TEMPLATE, detail=f"{kind}: {type(exc).__name__}")
    named = {
        v.group(1): {m.group(1): bool(m.group(2)) for m in VOCAB_TOKEN.finditer(line.text)}
        for line in template
        if (v := VOCAB_LINE.match(line.text))
    }
    fields: dict[str, set[str]] = {}
    marks: dict[str, dict[str, bool]] = {}
    mark_names: set[str] = set()
    section = ""
    for line in template:
        if h := HEADING.match(line.text):
            section = h.group(2)
        elif section and (f := FIELD.match(line.text)):
            fields.setdefault(section, set()).add(f.group("name"))
            if (slot := MARK_SLOT.match(f.group("value"))) and slot.group(1) in named:
                marks[f.group("name")] = named[slot.group(1)]
                mark_names.add(slot.group(1))
    leaders = {
        token: requires_id
        for label, tokens in named.items() if label not in mark_names
        for token, requires_id in tokens.items()
    }
    leaders.update({token: False for _, _, token in headings(content_lines(axis_text))})
    return Schema(
        sections=tuple(token for _, _, token in headings(template)),
        optional=OPTIONAL_SECTIONS.get(kind, frozenset()),
        fields={section: frozenset(names) for section, names in fields.items()},
        leaders=leaders,
        marks=marks,
    )


def census(schema: Schema, got: tuple[tuple[int, str, str], ...]) -> list[Row]:
    """Judge the section census: presence, order, and numbering, each at its own line.

    Returns:
        One row per missing required section, unknown section, order break, or number break.
    """
    got_tokens = [token for _, _, token in got]
    present = set(got_tokens)
    anchor = got[0][0] if got else 1
    rows = [
        Row(line=anchor, check=Check.HEADING_CENSUS, detail=f"missing section [{token}]")
        for token in schema.sections
        if token not in present and token not in schema.optional
    ]
    rows.extend(
        Row(line=line, check=Check.HEADING_CENSUS, detail=f"unknown section [{token}]")
        for line, _, token in got
        if token not in schema.sections
    )
    expected_order = [token for token in schema.sections if token in present]
    if [token for token in got_tokens if token in schema.sections] != expected_order:
        rows.append(Row(line=anchor, check=Check.HEADING_CENSUS, detail=f"sections {got_tokens} out of template order"))
    rows.extend(
        Row(line=line, check=Check.HEADING_CENSUS, detail=f"number [{number}] out of sequence, expected [{index:02d}]")
        for index, (line, number, _) in enumerate(got, 1)
        if int(number) != index
    )
    return rows


def leader_rows(line: Line, schema: Schema) -> list[Row]:
    """Judge one entry leader: id grammar plus every trailing token against the vocabulary.

    A parameterized token (`SUPERSEDED:<id>` in the template) requires a well-formed id tail
    on the instance; a plain token rejects any tail.

    Returns:
        Finding rows for this leader.
    """
    m = ENTRY.match(line.text)
    if m is None:
        return []
    rows = [] if LEADER_ID.match(m.group("id")) else [
        Row(line=line.number, check=Check.LEADER_ID, detail=f"[{m.group('id')}] outside the id grammar")
    ]
    for token in (m.group("a"), m.group("b")):
        if token is None:
            continue
        name, _, tail = token.partition(":")
        requires_id = schema.leaders.get(name)
        valid = requires_id is not None and (bool(LEADER_ID.match(tail)) if requires_id else not tail)
        if not valid:
            rows.append(Row(line=line.number, check=Check.LEADER_VOCAB, detail=f"[{token}] outside declared vocabulary"))
    return rows


def criteria_rows(line: Line, section: str, rule: tuple[str, str] | None) -> list[Row]:
    """Judge one wargame criteria bullet: its bracket tokens are distinct.

    Returns:
        One row per repeated criterion token; empty off the criteria bullet.
    """
    if rule is None or section != rule[0]:
        return []
    b = BULLET.match(line.text)
    if b is None or b.group("name") != rule[1]:
        return []
    rows: list[Row] = []
    seen: set[str] = set()
    for token in BRACKET_TOKEN.findall(b.group("value")):
        if token in seen:
            rows.append(Row(line=line.number, check=Check.WARGAME_CRITERIA, detail=f"criterion [{token}] repeated"))
        seen.add(token)
    return rows


def scan(path: Path) -> list[Row]:
    """Run all conformance checks over one instance file.

    Returns:
        Finding rows in scan order.
    """
    try:
        text = path.read_text(encoding="utf-8")
    except (OSError, UnicodeDecodeError) as exc:
        return [Row(line=0, check=Check.READ, detail=type(exc).__name__)]
    lines = content_lines(text)
    first = next((line for line in lines if line.text.strip()), None)
    if first is None:
        return [Row(line=1, check=Check.KIND, detail="no content lines")]
    h1 = H1.match(first.text)
    if h1 is None:
        return [Row(line=first.number, check=Check.KIND, detail="first content line is not a template H1")]
    kind = KIND_BY_SUFFIX.get(h1.group(1).rsplit("_", 1)[-1], "")
    if not kind:
        return [Row(line=first.number, check=Check.KIND, detail=f"H1 [{h1.group(1)}] resolves to no template")]
    schema = schema_for(kind)
    if isinstance(schema, Row):
        return [schema]

    rows: list[Row] = []
    section = ""
    section_line = 0
    entry_count = 0
    seen_ids: dict[tuple[str, str], int] = {}
    span: dict[str, set[str]] = {}
    span_rule = SECTION_SPAN_RULES.get(kind)
    coverage_rule = SECTION_COVERAGE_RULES.get(kind)
    criteria_rule = WARGAME_CRITERIA_RULES.get(kind)
    covers: dict[str, dict[str, int]] = {}
    for line in lines:
        if m := HEADING.match(line.text):
            section = m.group(2)
            if span_rule and section == span_rule[0]:
                section_line = line.number
            if coverage_rule and section in coverage_rule:
                covers.setdefault(section, {})
        rows.extend(
            Row(line=line.number, check=Check.SLOT_RESIDUE, detail=m.group(0))
            for m in SLOT.finditer(CODE_SPAN.sub(" ", line.text))
        )
        rows.extend(criteria_rows(line, section, criteria_rule))
        rows.extend(leader_rows(line, schema))
        if e := ENTRY.match(line.text):
            entry_count += 1
            key = (section, e.group("id"))
            if key in seen_ids:
                rows.append(Row(
                    line=line.number, check=Check.LEADER_UNIQUE,
                    detail=f"[{e.group('id')}] reused in [{section}], first at line {seen_ids[key]}",
                ))
            else:
                seen_ids[key] = line.number
            if span_rule and section == span_rule[0] and e.group("a"):
                span.setdefault(section, set()).add(e.group("a"))
            if coverage_rule and section in coverage_rule:
                covers.setdefault(section, {}).setdefault(e.group("id"), line.number)
        declared = schema.fields.get(section, frozenset())
        if f := FIELD.match(line.text):
            name = f.group("name")
            if declared and name not in declared:
                rows.append(Row(
                    line=line.number, check=Check.FIELD_VOCAB,
                    detail=f"field {name} outside [{section}] vocabulary",
                ))
            if (allowed := schema.marks.get(name)) is not None:
                lead = MARK_LEAD.match(f.group("value"))
                if lead is None:
                    rows.append(Row(
                        line=line.number, check=Check.MARK_VOCAB,
                        detail=f"{name} value lacks a leading mark token",
                    ))
                elif lead.group(1) not in allowed:
                    rows.append(Row(
                        line=line.number, check=Check.MARK_VOCAB,
                        detail=f"[{lead.group(1)}] outside the {name} mark vocabulary",
                    ))
    if (floor := ENTRY_FLOOR_BY_KIND.get(kind)) and entry_count < floor:
        rows.append(Row(
            line=first.number, check=Check.ENTRY_FLOOR,
            detail=f"{kind} carries {entry_count} entries, minimum {floor}",
        ))
    if span_rule and len(span.get(span_rule[0], set())) < span_rule[1]:
        rows.append(Row(
            line=section_line, check=Check.SECTION_SPAN,
            detail=f"[{span_rule[0]}] spans {len(span.get(span_rule[0], set()))} distinct tiers, minimum {span_rule[1]}",
        ))
    if coverage_rule and (cover := coverage_rule[1]) in covers:
        source, source_ids, cover_ids = coverage_rule[0], covers.get(coverage_rule[0], {}), covers[cover]
        rows.extend(
            Row(line=at, check=Check.SECTION_COVERAGE, detail=f"[{source}] entry [{eid}] has no [{cover}] score row")
            for eid, at in source_ids.items() if eid not in cover_ids
        )
        rows.extend(
            Row(line=at, check=Check.SECTION_COVERAGE, detail=f"[{cover}] score row [{eid}] matches no [{source}] entry")
            for eid, at in cover_ids.items() if eid not in source_ids
        )
    rows.extend(census(schema, headings(lines)))
    return rows


# --- [COMPOSITION] -------------------------------------------------------------------------

def main(argv: list[str]) -> int:
    """Run the conformance gate over argv instances; emit one row per hit.

    Returns:
        Process exit code: 0 clean, 1 any fail.
    """
    ap = argparse.ArgumentParser(add_help=True)
    ap.add_argument("--json", action="store_true")
    ap.add_argument("paths", nargs="+")
    ns = ap.parse_args(argv)

    def emit(path: Path, row: Row) -> None:
        if ns.json:
            print(json.dumps(
                {"file": str(path), "line": row.line, "check": row.check, "status": "fail", "detail": row.detail}
            ))
        else:
            print(f"{path}:{row.line}: FAIL {row.check} {row.detail}")

    failed = False
    for raw in ns.paths:
        path = Path(raw)
        found = scan(path) if path.is_file() else [Row(line=0, check=Check.COLLECT, detail="not a readable file")]
        for row in found:
            failed = True
            emit(path, row)
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
