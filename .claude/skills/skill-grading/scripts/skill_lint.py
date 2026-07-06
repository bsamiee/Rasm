#!/usr/bin/env python3
# ruff: noqa: T201
"""Deterministic skill-quality lint. Exit 0 iff no fail rows.

Usage: skill_lint.py [--json] <skill-dir>...
Output: `file:line: FAIL|WARN <check> <detail>`; --json emits NDJSON rows.
"""

# --- [RUNTIME_PRELUDE] -------------------------------------------------------------------
from __future__ import annotations

import argparse
import json
from pathlib import Path
import re
import sys
from typing import NamedTuple
from urllib.parse import unquote, urlparse


# --- [CONSTANTS] -------------------------------------------------------------------------

ALLOWED_FIELDS = frozenset({
    "agent",
    "allowed-tools",
    "compatibility",
    "context",
    "description",
    "disable-model-invocation",
    "disallowed-tools",
    "license",
    "metadata",
    "name",
    "user-invocable",
})
BODY_WARN = 350
BODY_FAIL = 500
BOLD = re.compile(r"\*\*")
CAPS_LABEL = re.compile(r"^\s*-?\s*[A-Z][A-Z -]{2,}[A-Z]:")
CODE_PATH = re.compile(r"`((?:assets|references|scripts)/[A-Za-z0-9._/-]+)`")
FENCE = re.compile(r"^(\s*)(`{3,}|~{3,})")
FIELD = re.compile(r"^([A-Za-z0-9_-]+):(?:\s*(.*))?$")
LINK = re.compile(r"\[[^\]]+]\(([^)]+)\)")
NAME = re.compile(r"^(?!-)(?!.*--)[a-z0-9]+(?:-[a-z0-9]+)*(?<!-)$")
TABLE_SEP = re.compile(r"^\s*\|?\s*:?-{3,}:?\s*(?:\|\s*:?-{3,}:?\s*)+\|?\s*$")
# Pronoun scan runs on the description VOICE: quoted trigger phrases and code spans are mention, not use.
THIRD_PERSON_BLOCK = re.compile(r"\b(I|me|my|mine|we|us|our|ours|you|your|yours)\b", re.IGNORECASE)
TRIGGER = re.compile(r"\b(use when|use whenever|trigger(?:s|ed|ing)?(?: when| for)?|when .{0,80}\b(use|grade|lint|evaluat|improv))", re.IGNORECASE)


# --- [MODELS] ----------------------------------------------------------------------------


class Row(NamedTuple):
    """One lint finding projected to human or NDJSON output."""

    file: str
    line: int
    check: str
    status: str
    detail: str


class Source(NamedTuple):
    """One readable markdown source and its fence-free line map."""

    path: Path
    text: str
    lines: list[str]
    prose: list[tuple[int, str]]


# --- [OPERATIONS] ------------------------------------------------------------------------


def read(path: Path) -> tuple[str | None, Row | None]:
    """Read a text file with OSError converted into a fail row.

    Returns:
        File text plus no row, or no text plus one read-failure row.
    """
    try:
        return path.read_text(encoding="utf-8", errors="replace"), None
    except OSError as exc:
        return None, Row(str(path), 0, "read", "fail", type(exc).__name__)


def prose_lines(text: str) -> list[tuple[int, str]]:
    """Return non-fenced lines with original line numbers.

    Returns:
        Pairs of original line number and line text.
    """
    out: list[tuple[int, str]] = []
    in_fence = False
    marker = ""
    for number, line in enumerate(text.splitlines(), 1):
        fence = FENCE.match(line)
        if fence and not in_fence:
            in_fence = True
            marker = fence.group(2)[0] * len(fence.group(2))
            continue
        if in_fence:
            if fence and line.strip().startswith(marker):
                in_fence = False
            continue
        out.append((number, line))
    return out


def markdown_sources(skill: Path) -> tuple[list[Source], list[Row]]:
    """Collect SKILL.md and first-level reference markdown sources.

    Returns:
        Readable sources plus read-failure rows.
    """
    rows: list[Row] = []
    sources: list[Source] = []
    for path in (skill / "SKILL.md", *sorted((skill / "references").glob("*.md"))):
        text, fault = read(path)
        if fault:
            rows.append(fault)
        elif text is not None:
            sources.append(Source(path, text, text.splitlines(), prose_lines(text)))
    return sources, rows


def frontmatter(text: str, path: Path) -> tuple[dict[str, tuple[str, int]], list[Row]]:
    """Parse shallow YAML frontmatter fields needed by skill metadata checks.

    Returns:
        Parsed fields keyed by name plus frontmatter-failure rows.
    """
    lines = text.splitlines()
    if not lines or lines[0].strip() != "---":
        return {}, [Row(str(path), 1, "frontmatter-fields", "fail", "missing frontmatter")]
    fields: dict[str, tuple[str, int]] = {}
    rows: list[Row] = []
    index = 1
    while index < len(lines) and lines[index].strip() != "---":
        match = FIELD.match(lines[index])
        if not match:
            index += 1
            continue
        key, raw = match.group(1), (match.group(2) or "").strip()
        line = index + 1
        if raw in {">", ">-", "|", "|-"}:
            parts: list[str] = []
            index += 1
            while index < len(lines) and (lines[index].startswith(" ") or not lines[index].strip()):
                stripped = lines[index].strip()
                if stripped:
                    parts.append(stripped)
                index += 1
            fields[key] = (" ".join(parts), line)
            continue
        fields[key] = (raw.strip("'\""), line)
        index += 1
    if index >= len(lines):
        rows.append(Row(str(path), 1, "frontmatter-fields", "fail", "unterminated frontmatter"))
    return fields, rows


def check_frontmatter(skill: Path, source: Source) -> list[Row]:
    """Validate skill metadata name, description, and field vocabulary.

    Returns:
        Frontmatter finding rows.
    """
    fields, rows = frontmatter(source.text, source.path)
    name, name_line = fields.get("name", ("", 1))
    description, description_line = fields.get("description", ("", 1))
    rows.extend(
        Row(str(source.path), line, "frontmatter-fields", "fail", f"unknown key {key}")
        for key, (_value, line) in fields.items()
        if key not in ALLOWED_FIELDS
    )
    if not name:
        rows.append(Row(str(source.path), 1, "frontmatter-name", "fail", "missing name"))
    elif len(name) > 64 or not NAME.fullmatch(name) or name != skill.name:
        rows.append(Row(str(source.path), name_line, "frontmatter-name", "fail", f"{name!r} must equal {skill.name!r}"))
    if not description:
        rows.append(Row(str(source.path), 1, "frontmatter-description", "fail", "missing description"))
    else:
        if len(description) > 1024:
            rows.append(Row(str(source.path), description_line, "frontmatter-description", "fail", "description >1024 chars"))
        voice = re.sub(r'"[^"]*"|`[^`]*`', " ", description)
        if THIRD_PERSON_BLOCK.search(voice):
            rows.append(Row(str(source.path), description_line, "frontmatter-description", "fail", "first or second person pronoun"))
        if not TRIGGER.search(description):
            rows.append(Row(str(source.path), description_line, "frontmatter-description", "fail", "missing trigger language"))
    return rows


def body_size(source: Source) -> list[Row]:
    """Check SKILL.md line-count budget.

    Returns:
        Body-size finding rows.
    """
    count = len(source.lines)
    if count > BODY_FAIL:
        return [Row(str(source.path), BODY_FAIL + 1, "body-size", "fail", f"{count} lines > {BODY_FAIL}")]
    if count > BODY_WARN:
        return [Row(str(source.path), BODY_WARN + 1, "body-size", "warn", f"{count} lines > {BODY_WARN}")]
    return []


def path_target(raw: str) -> str | None:
    """Normalize a markdown or code-span path token to a relative disk target.

    Returns:
        Relative target path, or none for anchors and external URLs.
    """
    target = unquote(raw.strip().split()[0].split("#", 1)[0])
    parsed = urlparse(target)
    if not target or target.startswith("#"):
        return None
    if parsed.scheme or parsed.netloc:
        return None
    return target


def referenced_paths(source: Source) -> list[tuple[int, str]]:
    """Extract relative links and bundled-resource code paths from prose lines.

    Returns:
        Pairs of line number and referenced target.
    """
    return [
        (line, target)
        for line, text in source.prose
        for target in (
            *(path_target(match.group(1)) for match in LINK.finditer(text)),
            *(path_target(match.group(1)) for match in CODE_PATH.finditer(text)),
        )
        if target
    ]


def resolve(source: Source, skill: Path, target: str) -> Path:
    """Resolve bundled-root paths from the skill root and ordinary links from source parent.

    Returns:
        Absolute resolved target path.
    """
    base = skill if target.startswith(("assets/", "references/", "scripts/")) else source.path.parent
    return (base / target).resolve()


def path_reachability(skill: Path, sources: list[Source]) -> list[Row]:
    """Check referenced relative links and bundled paths.

    Returns:
        Reachability and reference-depth finding rows.
    """
    rows: list[Row] = []
    root = skill.resolve()
    for source in sources:
        for line, target in referenced_paths(source):
            resolved = resolve(source, skill, target)
            if root not in (resolved, *resolved.parents) or not resolved.exists():
                rows.append(Row(str(source.path), line, "path-reachability", "fail", f"unresolved {target}"))
            if source.path.parent.name == "references" and target.startswith("references/") and target.endswith(".md"):
                rows.append(Row(str(source.path), line, "reference-depth", "fail", f"nested reference {target}"))
    return rows


def prose_markers(source: Source) -> list[Row]:
    """Check bold prose and unbracketed caps labels outside fences.

    Returns:
        Prose marker finding rows.
    """
    return [
        Row(str(source.path), line, check, "fail", detail)
        for line, text in source.prose
        for check, detail in (
            ("bold-prose", "bold marker") if BOLD.search(text) else ("", ""),
            ("caps-label", text.strip()) if CAPS_LABEL.match(text) else ("", ""),
        )
        if check
    ]


def table_index(source: Source) -> list[Row]:
    """Warn when an enumerable markdown table lacks an [INDEX] header.

    Returns:
        Table-index warning rows.
    """
    rows: list[Row] = []
    lines = source.prose
    index = 0
    while index < len(lines) - 1:
        line_no, header = lines[index]
        if "|" not in header or not TABLE_SEP.match(lines[index + 1][1]):
            index += 1
            continue
        cursor = index + 2
        while cursor < len(lines) and "|" in lines[cursor][1].strip():
            cursor += 1
        if cursor - index - 2 > 2 and "[INDEX]" not in header:
            rows.append(Row(str(source.path), line_no, "table-index", "warn", "header lacks [INDEX]"))
        index = cursor
    return rows


def scan(skill: Path) -> list[Row]:
    """Run every lint check over one skill directory.

    Returns:
        Finding rows across all checks.
    """
    sources, rows = markdown_sources(skill)
    skill_source = next((source for source in sources if source.path.name == "SKILL.md"), None)
    if skill_source:
        rows.extend(check_frontmatter(skill, skill_source))
        rows.extend(body_size(skill_source))
    rows.extend(path_reachability(skill, sources))
    for source in sources:
        rows.extend(prose_markers(source))
        rows.extend(table_index(source))
    return rows


# --- [COMPOSITION] -----------------------------------------------------------------------


def emit(row: Row, *, json_output: bool) -> None:
    """Emit one lint row in human or NDJSON form."""
    if json_output:
        print(json.dumps({"file": row.file, "line": row.line, "check": row.check, "status": row.status, "detail": row.detail}))
    else:
        print(f"{row.file}:{row.line}: {row.status.upper()} {row.check} {row.detail}")


def main(argv: list[str]) -> int:
    """Run skill lint over argv skill directories.

    Returns:
        Process exit code.
    """
    parser = argparse.ArgumentParser(add_help=True)
    parser.add_argument("--json", action="store_true")
    parser.add_argument("skill_dirs", nargs="+")
    ns = parser.parse_args(argv)

    rows = [row for raw in ns.skill_dirs for row in scan(Path(raw))]
    for row in rows:
        emit(row, json_output=ns.json)
    return 1 if any(row.status == "fail" for row in rows) else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
