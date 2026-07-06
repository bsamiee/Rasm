#!/usr/bin/env python3
# ruff: noqa: T201 — stdout is this gate's output contract
"""Self-containment gate for single-file HTML artifacts. Exit 0 iff no fail.

Usage: check_artifact.py [--json] <file.html>...
Checks: external-ref (any src/srcset/poster/action/data outside data:/#; any href with a
scheme or //; url()/@import in CSS; absolute-URL literals feeding fetch/import/Worker/
WebSocket/sendBeacon/XMLHttpRequest/location inside script bodies or on* handlers; remote
references inside srcdoc), base (any <base href> rewriting relative resolution), doctype,
title (present and non-empty), dark-theme (prefers-color-scheme media query and data-theme
selector proven in parsed CSS, never in comments or prose), embedded-state (every JSON
script payload — application/json, any +json, importmap, speculationrules — must parse
whole), srgb-mix (any `color-mix(in srgb ...)` — derived color is OKLCH-only), size warn
>400KB on raw bytes, print warn (no @media print in CSS), residue warn (template
replace-markers), script-hazard warn (raw U+2028/U+2029 inside script bodies),
export-control warn (capture controls without a data-export egress), theme-tokens warn
(CSS present without semantic tokens), raw-hex warn (hex color in a non-custom-property
declaration — color reaches rules through tokens), interaction warn (button rules without
an `:active` state), secret warn (credential-shaped literals outside base64 blobs).
Output: `file:line: FAIL|WARN <check> <detail>`; --json emits NDJSON rows
{"file","line","check","status","detail"}. Unreadable or invalid-UTF-8 input emits one
check=read fail row. Rendering fidelity and runtime-built egress are the browser and CSP
oracle's; this gate owns the static single-file contract.
"""

# --- [RUNTIME_PRELUDE] -------------------------------------------------------------------

import argparse
from dataclasses import dataclass
from enum import StrEnum
from html.parser import HTMLParser
import json
from pathlib import Path
import re
import sys
from typing import Literal, override


# --- [TYPES] -------------------------------------------------------------------------------

type Status = Literal["fail", "warn"]


class Check(StrEnum):
    """Closed check vocabulary; every emitted row names one member."""

    COLLECT = "collect"
    READ = "read"
    EXTERNAL_REF = "external-ref"
    BASE = "base"
    DOCTYPE = "doctype"
    TITLE = "title"
    DARK_THEME = "dark-theme"
    EMBEDDED_STATE = "embedded-state"
    SRGB_MIX = "srgb-mix"
    EXPORT_CONTROL = "export-control"
    THEME_TOKENS = "theme-tokens"
    RAW_HEX = "raw-hex"
    INTERACTION = "interaction"
    SIZE = "size"
    PRINT = "print"
    RESIDUE = "residue"
    SCRIPT_HAZARD = "script-hazard"
    SECRET = "secret"


# --- [CONSTANTS] ---------------------------------------------------------------------------

CAPTURE_TAGS = frozenset({"input", "select", "textarea"})
CSS_COMMENT = re.compile(r"/\*.*?\*/", re.DOTALL)
CSS_IMPORT = re.compile(r"@import\s+['\"]([^'\"]+)", re.IGNORECASE)
CSS_URL = re.compile(r"url\(\s*['\"]?([^'\")]+)", re.IGNORECASE)
DOCTYPE = re.compile(r"^(?:\s|<!--.*?-->)*<!doctype\s+html", re.IGNORECASE | re.DOTALL)
HREF_ALLOWED = re.compile(r"^(#|data:|mailto:)", re.IGNORECASE)
HREF_BLOCKED = re.compile(r"^(//|[a-z][a-z0-9+.-]*:)", re.IGNORECASE)
JSON_SCRIPT_TYPES = frozenset({"application/json", "importmap", "speculationrules"})
RESIDUE = re.compile(r"<!--\s*replace:", re.IGNORECASE)
SCRIPT_HAZARD = re.compile(r"[\u2028\u2029]")
# A remote literal reaching an executable sink; runtime-assembled strings stay the CSP oracle's.
SCRIPT_SINK = re.compile(
    r"\b(?:fetch|import|Worker|SharedWorker|WebSocket|EventSource|sendBeacon|XMLHttpRequest"
    r"|location(?:\.href)?\s*=|window\.open)\b[^\n;]{0,80}?['\"](?:https?:)?//",
    re.IGNORECASE,
)
SECRET = re.compile(
    r"(AKIA[0-9A-Z]{16}|ghp_[A-Za-z0-9]{36,}|xox[baprs]-[A-Za-z0-9-]{10,}"
    r"|sk-[A-Za-z0-9]{20,}|-----BEGIN [A-Z ]*PRIVATE KEY-----|eyJ[A-Za-z0-9_-]{8,}\.eyJ)"
)
SRGB_MIX = re.compile(r"color-mix\(\s*in\s+srgb", re.IGNORECASE)
CSS_DECL = re.compile(r"(?P<prop>[-\w]+)\s*:(?P<value>[^;{}]*)")
HEX_COLOR = re.compile(r"#[0-9a-fA-F]{3,8}\b")
BUTTON_RULE = re.compile(r"(?:^|[,{}\s])(?:\.btn|button)\b", re.IGNORECASE)
ACTIVE_STATE = re.compile(r":active\b", re.IGNORECASE)
SRC_ALLOWED = re.compile(r"^(#|data:)", re.IGNORECASE)
SRC_ATTRS = frozenset({"src", "poster", "action", "data"})
SRCDOC_REMOTE = re.compile(r"\b(?:src|href)\s*=\s*['\"]?(?:https?:)?//", re.IGNORECASE)
SIZE_WARN = 400 * 1024
THEME_TOKENS = ("--bg", "--text", "--accent")


# --- [OPERATIONS] --------------------------------------------------------------------------

def srcset_urls(value: str) -> tuple[str, ...]:
    """Split a srcset into candidate URLs, honoring data: payload commas and bare-comma splits.

    Returns:
        Candidate URLs in declaration order.
    """
    urls: list[str] = []
    i = 0
    while i < len(value):
        while i < len(value) and value[i] in " \t\n,":
            i += 1
        if i >= len(value):
            break
        start = i
        if value.startswith("data:", i):
            descriptor = re.compile(r"\s+\S+\s*(?:,|$)|,(?=\s)|$").search(value, i)
            end = descriptor.start() if descriptor else len(value)
            urls.append(value[start:end].strip().split()[0])
            i = (descriptor.end() if descriptor else len(value))
        else:
            while i < len(value) and value[i] not in " \t\n,":
                i += 1
            urls.append(value[start:i])
            while i < len(value) and value[i] != ",":
                i += 1
    return tuple(url for url in urls if url)


# --- [MODELS] ------------------------------------------------------------------------------

@dataclass(frozen=True, slots=True, kw_only=True, order=True)
class Row:
    """One artifact-gate finding projected to an NDJSON row."""

    line: int
    check: Check
    status: Status
    detail: str


class Audit(HTMLParser):
    """Walks one document collecting evidence; the callback object is the platform-forced mutable seam."""

    def __init__(self) -> None:
        """Prime empty evidence accumulators over a fresh parser."""
        super().__init__(convert_charrefs=True)
        self.rows: list[Row] = []
        self.title_text: list[str] = []
        self.has_export = False
        self.captures = 0
        self.payloads: list[tuple[int, list[str]]] = []
        self.css: list[tuple[int, str]] = []
        self.scripts: list[tuple[int, list[str]]] = []
        self._in_title = False
        self._in_style = False
        self._script: tuple[int, bool] | None = None

    def _attr_rows(self, tag: str, name: str, v: str, line: int) -> None:
        """Judge one attribute value against the reference and capture policies."""
        if name == "srcset":
            self.rows.extend(
                Row(line=line, check=Check.EXTERNAL_REF, status="fail", detail=f"{tag}[srcset]={url[:120]}")
                for url in srcset_urls(v)
                if not SRC_ALLOWED.match(url)
            )
        elif name in SRC_ATTRS:
            if not SRC_ALLOWED.match(v):
                self.rows.append(Row(line=line, check=Check.EXTERNAL_REF, status="fail", detail=f"{tag}[{name}]={v[:120]}"))
        elif name == "href":
            if tag == "base":
                self.rows.append(Row(line=line, check=Check.BASE, status="fail", detail=f"base[href]={v[:120]}"))
            elif HREF_BLOCKED.match(v) and not HREF_ALLOWED.match(v):
                self.rows.append(Row(line=line, check=Check.EXTERNAL_REF, status="fail", detail=f"{tag}[href]={v[:120]}"))
        elif name == "srcdoc":
            if SRCDOC_REMOTE.search(v):
                self.rows.append(Row(line=line, check=Check.EXTERNAL_REF, status="fail", detail=f"{tag}[srcdoc] remote reference"))
        elif name.startswith("on"):
            if SCRIPT_SINK.search(v):
                self.rows.append(Row(line=line, check=Check.EXTERNAL_REF, status="fail", detail=f"{tag}[{name}]={v[:120]}"))
        elif name == "style":
            self.css.append((line, v))

    @override
    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        line = self.getpos()[0]
        if tag == "title":
            self._in_title = True
        if tag == "style":
            self._in_style = True
        if tag in CAPTURE_TAGS:
            self.captures += 1
        if tag == "script":
            media = next((v.strip().lower() for n, v in attrs if n == "type" and v), "text/javascript")
            as_json = media in JSON_SCRIPT_TYPES or media.endswith("+json")
            self._script = (line, as_json)
            (self.payloads if as_json else self.scripts).append((line, []))
        for name, value in attrs:
            if name == "data-export" or name.startswith("data-export-"):
                self.has_export = True
            if name in ("contenteditable", "draggable") and (value is None or value.strip().lower() != "false"):
                self.captures += 1
            if value is not None and value.strip():
                self._attr_rows(tag, name, value.strip(), line)

    @override
    def handle_endtag(self, tag: str) -> None:
        if tag == "title":
            self._in_title = False
        if tag == "style":
            self._in_style = False
        if tag == "script":
            self._script = None

    @override
    def handle_data(self, data: str) -> None:
        if self._in_title:
            self.title_text.append(data)
        if self._in_style:
            self.css.append((self.getpos()[0], data))
        if self._script is not None:
            target = self.payloads if self._script[1] else self.scripts
            target[-1][1].append(data)


# --- [OPERATIONS] --------------------------------------------------------------------------

def audit(path: Path) -> list[Row]:
    """Audit one document for the self-containment contract.

    Returns:
        Finding rows sorted by line; unreadable or invalid-UTF-8 input yields one read fault.
    """
    try:
        raw = path.read_bytes()
        text = raw.decode("utf-8")
    except (OSError, UnicodeDecodeError) as exc:
        return [Row(line=0, check=Check.READ, status="fail", detail=type(exc).__name__)]
    parser = Audit()
    parser.feed(text)
    parser.close()
    rows = parser.rows
    css_text = CSS_COMMENT.sub(" ", "\n".join(chunk for _, chunk in parser.css))
    rows.extend(
        Row(line=base_line, check=Check.EXTERNAL_REF, status="fail", detail=frame.format(m.group(1).strip()[:120]))
        for base_line, css in parser.css
        for pattern, frame in ((CSS_URL, "url({})"), (CSS_IMPORT, "@import {}"))
        for m in pattern.finditer(CSS_COMMENT.sub(" ", css))
        if not SRC_ALLOWED.match(m.group(1).strip())
    )
    rows.extend(
        Row(line=line, check=Check.EXTERNAL_REF, status="fail", detail=f"script sink: {m.group(0)[:120]}")
        for line, chunks in parser.scripts
        for m in SCRIPT_SINK.finditer("".join(chunks))
    )
    has_theme = "prefers-color-scheme" in css_text and "data-theme" in css_text
    document_checks: tuple[tuple[bool, Check, str], ...] = (
        (bool(DOCTYPE.match(text)), Check.DOCTYPE, "missing <!doctype html>"),
        (bool("".join(parser.title_text).strip()), Check.TITLE, "empty or missing <title>"),
        (has_theme, Check.DARK_THEME, "CSS lacks prefers-color-scheme media query with data-theme selectors"),
    )
    rows.extend(
        Row(line=1, check=check, status="fail", detail=detail)
        for present, check, detail in document_checks
        if not present
    )
    for payload_line, chunks in parser.payloads:
        payload = "".join(chunks)
        try:
            json.loads(payload)
        except json.JSONDecodeError as exc:
            rows.append(Row(line=payload_line, check=Check.EMBEDDED_STATE, status="fail", detail=f"payload does not parse: {exc.msg}"))
    if parser.captures and not parser.has_export:
        rows.append(Row(line=1, check=Check.EXPORT_CONTROL, status="warn", detail=f"{parser.captures} capture controls, no data-export egress"))
    if parser.css and not all(token in css_text for token in THEME_TOKENS):
        rows.append(Row(line=1, check=Check.THEME_TOKENS, status="warn", detail="style present without semantic theme tokens"))
    rows.extend(
        Row(line=base_line, check=Check.SRGB_MIX, status="fail", detail=f"color-mix in srgb: {m.group(0)[:80]}")
        for base_line, css in parser.css
        for m in SRGB_MIX.finditer(CSS_COMMENT.sub(" ", css))
    )
    rows.extend(
        Row(line=base_line, check=Check.RAW_HEX, status="warn", detail=f"{m['prop']}:{m['value'].strip()[:60]}")
        for base_line, css in parser.css
        for m in CSS_DECL.finditer(CSS_COMMENT.sub(" ", css))
        if not m["prop"].startswith("--") and HEX_COLOR.search(m["value"])
    )
    if BUTTON_RULE.search(css_text) and not ACTIVE_STATE.search(css_text):
        rows.append(Row(line=1, check=Check.INTERACTION, status="warn", detail="button rules without an :active state"))
    if len(raw) > SIZE_WARN:
        rows.append(Row(line=1, check=Check.SIZE, status="warn", detail=f"{len(raw) // 1024}KB > {SIZE_WARN // 1024}KB"))
    if "@media print" not in css_text:
        rows.append(Row(line=1, check=Check.PRINT, status="warn", detail="no @media print block"))
    hazards = (
        Row(line=line, check=Check.SCRIPT_HAZARD, status="warn", detail="raw U+2028/U+2029 line separator")
        for line, chunks in (*parser.scripts, *parser.payloads)
        if SCRIPT_HAZARD.search("".join(chunks))
    )
    rows.extend(hazards)
    rows.extend(
        Row(line=number, check=check, status="warn", detail=detail)
        for number, line_text in enumerate(text.split("\n"), start=1)
        for check, pattern, detail in (
            (Check.RESIDUE, RESIDUE, "template replace-marker remains"),
            (Check.SECRET, SECRET, "credential-shaped literal"),
        )
        if pattern.search(line_text) and not (check is Check.SECRET and ";base64," in line_text)
    )
    return sorted(rows)


# --- [COMPOSITION] -------------------------------------------------------------------------

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
        found = audit(path) if path.is_file() else [Row(line=0, check=Check.COLLECT, status="fail", detail="not a readable file")]
        for row in found:
            failed = failed or row.status == "fail"
            if ns.json:
                print(json.dumps({"file": str(path), "line": row.line, "check": row.check, "status": row.status, "detail": row.detail}))
            else:
                print(f"{path}:{row.line}: {row.status.upper()} {row.check} {row.detail}")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
