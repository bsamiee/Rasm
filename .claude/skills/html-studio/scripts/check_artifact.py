#!/usr/bin/env -S uv run
# /// script
# requires-python = ">=3.13"
# dependencies = [
#   "beartype @ git+https://github.com/beartype/beartype.git@f370a0b1733413681e7a72bf36fbe839e60b3c85",
#   "coloraide", "cyclopts", "expression", "lxml", "msgspec", "pydantic", "tree-sitter", "tree-sitter-typescript"
# ]
# ///
# ruff: noqa: T201, D101, D102, D103
"""Single-file HTML artifact gate."""

# --- [RUNTIME_PRELUDE] -------------------------------------------------------------------

from collections.abc import Iterable, Sequence
from enum import StrEnum
from itertools import pairwise
from pathlib import Path
import re
import shutil
import subprocess
import sys
from typing import Annotated, Literal, Self, TYPE_CHECKING, TypeIs

from beartype import beartype
from coloraide import Color
from cyclopts import App, Parameter
from expression import Error, Ok
from lxml import html
from lxml.etree import XPath
import msgspec
from pydantic import TypeAdapter, ValidationError
from tree_sitter import Language, Parser
import tree_sitter_typescript


if TYPE_CHECKING:
    from expression import Result
    from tree_sitter import Node


# --- [TYPES] -----------------------------------------------------------------------------

type Status = Literal["ok", "warn", "fail"]
type JsonPayload = dict[str, object] | list[object]


class Check(StrEnum):
    A11Y_NAME = "a11y-name"
    BASE = "base"
    CSS_LAYER = "css-layer"
    DOCTYPE = "doctype"
    DUPLICATE_ID = "duplicate-id"
    EMBEDDED_STATE = "embedded-state"
    EXTERNAL_REF = "external-ref"
    FOCUS_VISIBLE = "focus-visible"
    HEADING_ORDER = "heading-order"
    INLINE_HANDLER = "inline-handler"
    INTERACTION = "interaction"
    JS_SINK = "js-sink"
    JS_SYNTAX = "js-syntax"
    PRINT = "print"
    RAW_HEX = "raw-hex"
    READ = "read"
    RESIDUE = "residue"
    RETURN_META = "return-meta"
    SCRIPT_COUNT = "script-count"
    SCRIPT_HAZARD = "script-hazard"
    SECRET = "secret"
    SIZE = "size"
    SRGB_MIX = "srgb-mix"
    STYLE_COUNT = "style-count"
    TITLE = "title"
    TOKEN_CONTRAST = "token-contrast"


class ScriptKind(StrEnum):
    EXECUTABLE = "executable"
    PAYLOAD = "payload"


# --- [CONSTANTS] -------------------------------------------------------------------------

BAD_REF = re.compile(r"^(?:/|//|[a-z][a-z0-9+.-]*:)", re.IGNORECASE)
COLOR_REF = re.compile(r"var\((--[\w-]+)\)|oklch\([^)]+\)|#[0-9a-fA-F]{3,8}\b")
CSS_DECL = re.compile(r"(?P<prop>--?[-\w]+)\s*:\s*(?P<value>[^;{}]+)")
CSS_IMPORT = re.compile(r"@import\s+(?:url\()?['\"]?([^'\"\s)]+)", re.IGNORECASE)
CSS_URL = re.compile(r"url\(\s*['\"]?([^'\"\s)]+)", re.IGNORECASE)
DOCTYPE = re.compile(r"^(?:\s|<!--.*?-->)*<!doctype\s+html", re.IGNORECASE | re.DOTALL)
HEX_COLOR = re.compile(r"#[0-9a-fA-F]{3,8}\b")
JSON_TYPES = frozenset({"application/json", "importmap", "speculationrules"})
REMOTE_LITERAL = re.compile(r"(?:https?:)?//|^/")
RESIDUE = re.compile(r"<!--\s*replace:", re.IGNORECASE)
SCRIPT_HAZARD = re.compile(r"[\u2028\u2029]")
SCRIPT_SINK_TEXT = re.compile(r"\b(?:fetch|import|Worker|SharedWorker|WebSocket|EventSource|sendBeacon|open)\s*\(", re.IGNORECASE)
SECRET = re.compile(
    r"AKIA[0-9A-Z]{16}|ghp_[A-Za-z0-9]{36,}|xox[baprs]-[A-Za-z0-9-]{10,}|sk-[A-Za-z0-9]{20,}|-----BEGIN [A-Z ]*PRIVATE KEY-----|eyJ[A-Za-z0-9_-]{8,}\.eyJ"
)
SRGB_MIX = re.compile(r"color-mix\(\s*in\s+srgb", re.IGNORECASE)
STYLE_ALLOWED_AT_RULES = ("@charset", "@layer", "@media", "@property", "@supports", "@keyframes", "@starting-style", "@page")
SURFACES = ("--bg", "--surface", "--raised", "--raised-2", "--overlay")
TEXT_TOKENS = ("--text", "--text-muted")
PAYLOAD_ADAPTER: TypeAdapter[JsonPayload] = TypeAdapter(JsonPayload)
ROW_ENCODER = msgspec.json.Encoder()
SIZE_WARN = 400 * 1024
MIN_CONTRAST = 4.5


# --- [MODELS] ----------------------------------------------------------------------------


class Row(msgspec.Struct, frozen=True):
    file: str
    line: int
    check: Check
    status: Status
    detail: str


class Script(msgspec.Struct, frozen=True):
    line: int
    kind: ScriptKind
    media: str
    body: str


class Artifact(msgspec.Struct, frozen=True):
    path: str
    text: str
    raw_size: int
    document: html.HtmlElement
    css: str
    scripts: tuple[Script, ...]

    @classmethod
    def from_path(cls, path: Path) -> Result[Self, Row]:
        try:
            raw = path.read_bytes()
            text = raw.decode("utf-8")
            document = html.document_fromstring(text)
        except (OSError, UnicodeDecodeError, html.etree.ParserError) as exc:
            return Error(Row(str(path), 0, Check.READ, "fail", type(exc).__name__))
        return Ok(cls(str(path), text, len(raw), document, "\n".join(style.text or "" for style in document.iter("style")), scripts(document)))


# --- [OPERATIONS] ------------------------------------------------------------------------

_XPATH: dict[str, XPath] = {}


def _node_set(found: object) -> TypeIs[list[html.HtmlElement]]:
    return isinstance(found, list) and all(isinstance(node, html.HtmlElement) for node in found)


def q(root: html.HtmlElement, expr: str, /, **bindings: str) -> tuple[html.HtmlElement, ...]:
    """Run a compiled, memoized XPath with `$var` bindings; only node-set expressions are spelled here.

    Returns:
        The matched elements in document order.
    """
    found = _XPATH.setdefault(expr, XPath(expr, smart_strings=False))(root, **bindings)
    return tuple(found) if _node_set(found) else ()


def line(element: html.HtmlElement) -> int:
    return int(element.sourceline or 1)


def text(element: html.HtmlElement) -> str:
    return " ".join("".join(element.itertext()).split())


def scripts(document: html.HtmlElement) -> tuple[Script, ...]:
    def kind(media: str) -> ScriptKind:
        normalized = media.strip().lower()
        return ScriptKind.PAYLOAD if normalized in JSON_TYPES or normalized.endswith("+json") else ScriptKind.EXECUTABLE

    return tuple(
        Script(line(node), kind(media), media, node.text or "")
        for node in q(document, "//script")
        for media in [str(node.get("type") or "text/javascript")]
    )


def emit(row: Row, json_mode: bool) -> None:
    print(ROW_ENCODER.encode(row).decode("utf-8") if json_mode else f"{row.file}:{row.line}: {row.status.upper()} {row.check} {row.detail}")


def bad_reference(value: str) -> bool:
    ref = value.strip()
    return bool(ref and BAD_REF.match(ref) and not ref.lower().startswith(("data:", "blob:", "about:blank")))


def srcset_urls(value: str) -> tuple[str, ...]:
    urls: list[str] = []
    token: list[str] = []
    quoted, data = "", False
    for char in f"{value},":
        quoted = "" if quoted == char else char if char in "\"'" and not quoted else quoted
        data = data or "".join(token).lower().startswith("data:")
        if char == "," and not quoted and (not data or re.search(r"\s+\d+(?:w|x)\s*$", "".join(token))):
            urls.append("".join(token).strip().split()[0])
            token, data = [], False
        else:
            token.append(char)
    return tuple(url for url in urls if url)


def css_plain(css: str) -> str:
    out: list[str] = []
    i, quoted, comment = 0, "", False
    while i < len(css):
        pair = css[i : i + 2]
        comment = False if comment and pair == "*/" else True if not quoted and pair == "/*" else comment
        if pair in {"/*", "*/"}:
            out.extend("  ")
            i += 2
            continue
        quoted = "" if quoted == css[i] and (i == 0 or css[i - 1] != "\\") else css[i] if not comment and not quoted and css[i] in "\"'" else quoted
        out.append(" " if comment or quoted else css[i])
        i += 1
    return "".join(out)


def css_layer_rows(artifact: Artifact, css: str) -> tuple[Row, ...]:
    rows: list[Row] = []
    stack: list[bool] = []
    start = 0
    for index, char in enumerate(css_plain(css)):
        if char == "{":
            prelude = css_plain(css[start:index]).strip()
            layered = bool(stack and stack[-1]) or prelude.startswith("@layer")
            if prelude and not prelude.startswith(STYLE_ALLOWED_AT_RULES) and not layered:
                rows.append(Row(artifact.path, css[:index].count("\n") + 1, Check.CSS_LAYER, "fail", f"unlayered rule {prelude[:80]}"))
            stack.append(layered)
            start = index + 1
        elif char == "}":
            if stack:
                stack.pop()
            start = index + 1
    return tuple(rows)


def token_colors(css: str) -> dict[str, Color]:
    values = {match["prop"]: match["value"].strip() for match in CSS_DECL.finditer(css_plain(css)) if match["prop"].startswith("--")}

    def resolve(value: str, depth: int = 0) -> Color | None:
        if depth > 6:
            return None
        found = COLOR_REF.search(value)
        if found is None:
            return None
        if found.group(1):
            return resolve(values.get(found.group(1), ""), depth + 1)
        try:
            return Color(found.group(0))
        except ValueError:
            return None

    return {name: color for name, value in values.items() if (color := resolve(value)) is not None}


def contrast_rows(artifact: Artifact, css: str) -> tuple[Row, ...]:
    colors = token_colors(css)
    return tuple(
        Row(artifact.path, 1, Check.TOKEN_CONTRAST, "fail", f"{fg} on {bg} contrast {score:.2f}")
        for bg in SURFACES
        for fg in TEXT_TOKENS
        if (surface := colors.get(bg)) is not None
        if (copy := colors.get(fg)) is not None
        if (score := copy.contrast(surface, method="wcag21")) < MIN_CONTRAST
    )


def js_tree_rows(artifact: Artifact, script: Script) -> tuple[Row, ...]:
    parser = Parser()
    parser.language = Language(tree_sitter_typescript.language_typescript())
    tree = parser.parse(script.body.encode("utf-8"))
    rows = [Row(artifact.path, script.line, Check.JS_SYNTAX, "fail", "tree-sitter parse error")] if tree.root_node.has_error else []

    def walk(node: Node, *, sink: bool = False) -> Iterable[Row]:
        source = script.body[node.start_byte : node.end_byte]
        active = (
            sink or (node.type in {"call_expression", "new_expression"} and bool(SCRIPT_SINK_TEXT.search(source[:96]))) or node.type == "import_call"
        )
        if active and node.type in {"string", "template_string"} and REMOTE_LITERAL.search(source.strip("`'\"")):
            yield Row(artifact.path, script.line + node.start_point[0], Check.JS_SINK, "fail", source[:120])
        for child in node.children:
            yield from walk(child, sink=active)

    rows.extend(walk(tree.root_node))
    return tuple(rows)


def js_syntax_rows(artifact: Artifact, script: Script) -> tuple[Row, ...]:
    if shutil.which("node") is None:
        return (Row(artifact.path, script.line, Check.JS_SYNTAX, "warn", "node is unavailable"),)
    completed = subprocess.run(("node", "--check", "-"), input=script.body, text=True, capture_output=True, timeout=10, check=False)
    return () if completed.returncode == 0 else (Row(artifact.path, script.line, Check.JS_SYNTAX, "fail", completed.stderr.splitlines()[0][:120]),)


def dom_rows(artifact: Artifact) -> tuple[Row, ...]:
    document = artifact.document
    rows: list[Row] = []
    rows.extend(
        Row(artifact.path, line(node), Check.RETURN_META, "fail", "artifact-return meta is server-injected")
        for node in q(document, "//meta[@name='artifact-return']")
    )
    rows.extend(Row(artifact.path, line(node), Check.BASE, "fail", "base href rewrites relative resolution") for node in q(document, "//base[@href]"))
    ids = {
        value: q(document, "//*[@id=$ident]", ident=value) for value in {str(node.get("id")) for node in q(document, "//*[@id]") if node.get("id")}
    }
    rows.extend(
        Row(artifact.path, line(nodes[0]), Check.DUPLICATE_ID, "fail", f"#{key} appears {len(nodes)} times")
        for key, nodes in ids.items()
        if len(nodes) > 1
    )
    headings = tuple(
        (int(node.tag[1]), line(node))
        for node in q(document, "//*[self::h1 or self::h2 or self::h3 or self::h4 or self::h5 or self::h6][not(ancestor::template)]")
    )
    rows.extend(
        Row(artifact.path, here, Check.HEADING_ORDER, "fail", f"h{previous} to h{current}")
        for (previous, _), (current, here) in pairwise(headings)
        if current > previous + 1
    )
    rows.extend(
        Row(artifact.path, line(node), Check.A11Y_NAME, "fail", "icon-only button lacks accessible name")
        for node in q(document, "//button")
        if not text(node) and not any(node.get(attr) for attr in ("aria-label", "aria-labelledby", "title"))
    )
    rows.extend(
        Row(artifact.path, line(node), Check.INTERACTION, "fail", "toggle button lacks aria-pressed")
        for node in q(document, "//button[contains(@class,'toggle') or @role='switch' or @data-toggle]")
        if node.get("aria-pressed") is None
    )
    rows.extend(
        Row(artifact.path, line(node), Check.INLINE_HANDLER, "fail", name)
        for node in q(document, "//*[@*]")
        for name in node.attrib
        if name.lower().startswith("on")
    )
    return tuple(rows)


def reference_rows(artifact: Artifact) -> tuple[Row, ...]:
    rows = [
        Row(artifact.path, line(node), Check.EXTERNAL_REF, "fail", f"{node.tag}[{attr}]={link[:120]}")
        for node, attr, link, _pos in artifact.document.iterlinks()
        if attr != "srcset" and bad_reference(link)
    ]
    rows.extend(
        Row(artifact.path, line(node), Check.EXTERNAL_REF, "fail", f"{node.tag}[srcset]={url[:120]}")
        for node in q(artifact.document, "//*[@srcset]")
        for url in srcset_urls(str(node.get("srcset") or ""))
        if bad_reference(url)
    )
    rows.extend(
        Row(artifact.path, artifact.css[: match.start()].count("\n") + 1, Check.EXTERNAL_REF, "fail", f"css {match.group(1)[:120]}")
        for pattern in (CSS_URL, CSS_IMPORT)
        for match in pattern.finditer(css_plain(artifact.css))
        if bad_reference(match.group(1))
    )
    return tuple(rows)


def css_rows(artifact: Artifact) -> tuple[Row, ...]:
    plain = css_plain(artifact.css)
    rows: list[Row] = []
    rows.extend(css_layer_rows(artifact, artifact.css))
    rows.extend(contrast_rows(artifact, artifact.css))
    rows.extend(
        Row(artifact.path, artifact.css[: match.start()].count("\n") + 1, Check.SRGB_MIX, "fail", match.group(0))
        for match in SRGB_MIX.finditer(plain)
    )
    rows.extend(
        Row(artifact.path, artifact.css[: match.start()].count("\n") + 1, Check.RAW_HEX, "fail", f"{match['prop']}:{match['value'].strip()[:80]}")
        for match in CSS_DECL.finditer(plain)
        if HEX_COLOR.search(match["value"])
    )
    rows.append(Row(artifact.path, 1, Check.FOCUS_VISIBLE, "fail", "missing :focus-visible selector")) if ":focus-visible" not in plain else None
    rows.append(Row(artifact.path, 1, Check.FOCUS_VISIBLE, "fail", "outline:none without same-rule focus replacement")) if re.search(
        r":focus-visible[^{}]*{(?=[^{}]*outline\s*:\s*none)(?![^{}]*box-shadow)", plain
    ) else None
    rows.append(Row(artifact.path, 1, Check.INTERACTION, "warn", ".btn lacks transition")) if ".btn" in plain and not re.search(
        r"\.btn[^{]*{[^{}]*transition\s*:", plain
    ) else None
    rows.append(Row(artifact.path, 1, Check.PRINT, "fail", "missing @media print")) if "@media print" not in plain else None
    rows.append(Row(artifact.path, 1, Check.PRINT, "fail", ".export-bar not hidden in print")) if "export-bar" in artifact.text and not re.search(
        r"@media print[\s\S]*\.export-bar[^{]*{[^{}]*display\s*:\s*none", plain
    ) else None
    return tuple(rows)


def script_rows(artifact: Artifact) -> tuple[Row, ...]:
    executable = tuple(script for script in artifact.scripts if script.kind is ScriptKind.EXECUTABLE)
    rows = [Row(artifact.path, 1, Check.SCRIPT_COUNT, "fail", f"{len(executable)} executable scripts")]
    rows = [] if len(executable) == 1 else rows
    for script in artifact.scripts:
        if script.kind is ScriptKind.PAYLOAD:
            try:
                PAYLOAD_ADAPTER.validate_python(msgspec.json.decode(script.body.encode("utf-8")))
            except (msgspec.DecodeError, msgspec.ValidationError, ValidationError) as exc:
                rows.append(Row(artifact.path, script.line, Check.EMBEDDED_STATE, "fail", str(exc).splitlines()[0][:120]))
        else:
            rows.extend(js_syntax_rows(artifact, script))
            rows.extend(js_tree_rows(artifact, script))
        if SCRIPT_HAZARD.search(script.body):
            rows.append(Row(artifact.path, script.line, Check.SCRIPT_HAZARD, "warn", "raw U+2028/U+2029 line separator"))
    return tuple(rows)


@beartype
def audit(path: Path) -> tuple[Row, ...]:
    result = Artifact.from_path(path)
    if result.is_error():
        return (result.error,)
    artifact = result.ok
    base = (
        (bool(DOCTYPE.match(artifact.text)), Check.DOCTYPE, "missing <!doctype html>"),
        (bool(text(q(artifact.document, "//title")[0])) if q(artifact.document, "//title") else False, Check.TITLE, "empty or missing <title>"),
        (
            len(q(artifact.document, "/html/head/style")) == 1,
            Check.STYLE_COUNT,
            f"{len(q(artifact.document, '/html/head/style'))} document style blocks",
        ),
    )
    rows = [Row(artifact.path, 1, check, "fail", detail) for ok, check, detail in base if not ok]
    rows.extend(dom_rows(artifact) + reference_rows(artifact) + css_rows(artifact) + script_rows(artifact))
    rows.append(
        Row(artifact.path, 1, Check.SIZE, "warn", f"{artifact.raw_size // 1024}KB > {SIZE_WARN // 1024}KB")
    ) if artifact.raw_size > SIZE_WARN else None
    rows.extend(
        Row(artifact.path, number, check, "warn", detail)
        for number, value in enumerate(artifact.text.splitlines(), 1)
        for check, pattern, detail in (
            (Check.RESIDUE, RESIDUE, "template replace-marker remains"),
            (Check.SECRET, SECRET, "credential-shaped literal"),
        )
        if pattern.search(value) and not (check is Check.SECRET and ";base64," in value)
    )
    return tuple(sorted(rows, key=lambda row: (row.line, row.check, row.detail)))


# --- [COMPOSITION] -----------------------------------------------------------------------


def check(paths: Annotated[Sequence[Path], Parameter(name="paths")], *, json: bool = False) -> int:
    rows = tuple(
        row for path in paths for row in (audit(path) if path.is_file() else (Row(str(path), 0, Check.READ, "fail", "not a readable file"),))
    )
    for row in rows:
        emit(row, json)
    return 1 if any(row.status == "fail" for row in rows) else 0


app = App(default_command=check, result_action="return_int_as_exit_code_else_zero")

if __name__ == "__main__":
    sys.exit(app())
