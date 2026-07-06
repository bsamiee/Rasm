#!/usr/bin/env python3
# ruff: noqa: T201 — stdout is this gate's output contract
"""Self-containment gate for single-file HTML artifacts. Exit 0 iff no fail.

Usage: check_artifact.py [--json] <file.html>...
Checks: external-ref (any src/srcset/url() outside data:/#; any href with a scheme or //; any
javascript:, //, or http(s) URL literal inside an on* handler value), base (any <base href> that
rewrites relative resolution), doctype, title, dark-theme handling (prefers-color-scheme or
data-theme present), size warn >400KB, print warn (no @media print block), residue warn (template
replace-markers left in a filled artifact), script-hazard warn (raw U+2028/U+2029 breaking an
embedded JS string literal). Output: `file:line: FAIL|WARN <check> <detail>`; --json emits NDJSON
rows {"file","line","check","status","detail"}. An unreadable file emits one check=read fail row.
Rendering fidelity and runtime-built egress (fetch/XHR assembled at run time) are the browser and
CSP oracle's, not this gate's; this gate owns the static single-file contract.
"""

# --- [RUNTIME_PRELUDE] -------------------------------------------------------------------

import argparse
from html.parser import HTMLParser
import json
from pathlib import Path
import re
import sys
from typing import NamedTuple, override


# --- [CONSTANTS] -------------------------------------------------------------------------

CSS_IMPORT = re.compile(r"@import\s+['\"]([^'\"]+)", re.IGNORECASE)
CSS_URL = re.compile(r"url\(\s*['\"]?([^'\")]+)", re.IGNORECASE)
HANDLER_URL = re.compile(r"(javascript:|//|https?:)", re.IGNORECASE)
HREF_BLOCKED = re.compile(r"^(//|[a-z][a-z0-9+.-]*:)", re.IGNORECASE)
HREF_ALLOWED = re.compile(r"^(#|data:|mailto:)", re.IGNORECASE)
RESIDUE = re.compile(r"<!--\s*replace:", re.IGNORECASE)
SCRIPT_HAZARD = re.compile(r"[\u2028\u2029]")
SRC_ALLOWED = re.compile(r"^(#|data:)", re.IGNORECASE)
SRC_ATTRS = ("src", "srcset", "poster", "action", "data")
SIZE_WARN = 400 * 1024


# --- [MODELS] ----------------------------------------------------------------------------

class Row(NamedTuple):
    """One artifact-gate finding projected to an NDJSON row."""

    line: int
    check: str
    status: str
    detail: str


class Audit(HTMLParser):
    """Collects finding rows while walking one document."""

    def __init__(self) -> None:
        """Prime empty row/CSS accumulators over a fresh parser."""
        super().__init__(convert_charrefs=True)
        self.rows: list[Row] = []
        self.has_title = False
        self._in_style = False
        self.css: list[tuple[int, str]] = []

    @override
    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        line = self.getpos()[0]
        if tag == "title":
            self.has_title = True
        if tag == "style":
            self._in_style = True
        for name, value in attrs:
            if value is None:
                continue
            v = value.strip()
            if not v:
                continue
            if name == "srcset":
                # Candidates split on comma+whitespace; a bare comma stays inside data-URI base64.
                for candidate in re.split(r",\s+", v):
                    url = candidate.strip().split()[0] if candidate.strip() else ""
                    if url and not SRC_ALLOWED.match(url):
                        self.rows.append(Row(line, "external-ref", "fail", f"{tag}[srcset]={url[:120]}"))
            elif name in SRC_ATTRS:
                if not SRC_ALLOWED.match(v):
                    self.rows.append(Row(line, "external-ref", "fail", f"{tag}[{name}]={v[:120]}"))
            elif name == "href":
                if tag == "base":
                    self.rows.append(Row(line, "base", "fail", f"base[href]={v[:120]}"))
                elif HREF_BLOCKED.match(v) and not HREF_ALLOWED.match(v):
                    self.rows.append(Row(line, "external-ref", "fail", f"{tag}[href]={v[:120]}"))
            elif name.startswith("on"):
                if HANDLER_URL.search(v):
                    self.rows.append(Row(line, "external-ref", "fail", f"{tag}[{name}]={v[:120]}"))
            elif name == "style":
                self.css.append((line, v))

    @override
    def handle_endtag(self, tag: str) -> None:
        if tag == "style":
            self._in_style = False

    @override
    def handle_data(self, data: str) -> None:
        if self._in_style:
            self.css.append((self.getpos()[0], data))


# --- [OPERATIONS] ------------------------------------------------------------------------

def audit(path: Path) -> list[Row]:
    """Audit one document for the self-containment contract.

    Returns:
        Finding rows sorted by line; an unreadable file yields one read fault.
    """
    try:
        text = path.read_text(encoding="utf-8", errors="replace")
    except OSError as exc:
        return [Row(0, "read", "fail", type(exc).__name__)]
    parser = Audit()
    parser.feed(text)
    rows = parser.rows
    rows.extend(
        Row(base_line + css[: m.start()].count("\n"), "external-ref", "fail", frame.format(m.group(1).strip()[:120]))
        for base_line, css in parser.css
        for pattern, frame in ((CSS_URL, "url({})"), (CSS_IMPORT, "@import {}"))
        for m in pattern.finditer(css)
        if not SRC_ALLOWED.match(m.group(1).strip())
    )
    document_checks = (
        (bool(re.match(r"\s*<!doctype\s+html", text, re.IGNORECASE)), Row(1, "doctype", "fail", "missing <!doctype html>")),
        (parser.has_title, Row(1, "title", "fail", "missing <title>")),
        ("prefers-color-scheme" in text or "data-theme" in text, Row(1, "dark-theme", "fail", "no prefers-color-scheme or data-theme handling")),
    )
    rows.extend(row for present, row in document_checks if not present)
    if len(text.encode()) > SIZE_WARN:
        rows.append(Row(1, "size", "warn", f"{len(text.encode()) // 1024}KB > {SIZE_WARN // 1024}KB"))
    if "@media print" not in text:
        rows.append(Row(1, "print", "warn", "no @media print block"))
    rows.extend(
        Row(number, check, "warn", detail)
        for number, line_text in enumerate(text.split("\n"), start=1)
        for check, pattern, detail in (
            ("residue", RESIDUE, "template replace-marker remains"),
            ("script-hazard", SCRIPT_HAZARD, "raw U+2028/U+2029 line separator"),
        )
        if pattern.search(line_text)
    )
    return sorted(rows)


# --- [COMPOSITION] -----------------------------------------------------------------------

def main(argv: list[str]) -> int:
    """Audit each argv file; emit one row per finding.

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
        for row in audit(path):
            failed = failed or row.status == "fail"
            if ns.json:
                print(json.dumps({"file": str(path), "line": row.line, "check": row.check, "status": row.status, "detail": row.detail}))
            else:
                print(f"{path}:{row.line}: {row.status.upper()} {row.check} {row.detail}")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
