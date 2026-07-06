#!/usr/bin/env python3
# /// script
# requires-python = ">=3.13"
# dependencies = ["cyclopts", "msgspec", "networkx", "xxhash"]
# ///
"""Validate Mermaid fences through typed source analysis and SVG render proof."""

# ruff: noqa: T201, D101, D103

# --- [RUNTIME_PRELUDE] -------------------------------------------------------------------

from collections import Counter
from collections.abc import Iterable
from enum import StrEnum
from itertools import pairwise
from pathlib import Path
import re
import shlex
import shutil
import subprocess
import sys
from tempfile import TemporaryDirectory
from typing import Literal

from cyclopts import App
import msgspec
import networkx as nx
from xxhash import xxh3_128_hexdigest


# --- [TYPES] -----------------------------------------------------------------------------

type Status = Literal["ok", "warn", "fail"]


class Check(StrEnum):
    COLLECT = "collect"
    CONTRACT = "contract"
    FRONTMATTER = "frontmatter"
    LOGIC = "logic"
    READ = "read"
    RENDER = "render"
    SETUP = "setup"


class Family(StrEnum):
    ARCHITECTURE = "architecture"
    C4 = "c4"
    CLASS = "class"
    ER = "er"
    FLOWCHART = "flowchart"
    GANTT = "gantt"
    REQUIREMENT = "requirement"
    SEQUENCE = "sequence"
    STATE = "state"
    UNKNOWN = "unknown"


# --- [CONSTANTS] -------------------------------------------------------------------------

APP = App(name="validate-mermaid")
ENCODER = msgspec.json.Encoder()
ENV_MARKERS = ("chrome", "chromium", "puppeteer", "browser", "libnss", "sandbox", "econnrefused", "enoent")
RENDER_TIMEOUT = 60
SUFFIXES = frozenset({".md", ".mmd"})

ACCESS = re.compile(r"^\s*(accTitle|accDescr)(?:\s*:|\s*\{)")
BLOCKQUOTE = re.compile(r"^\s*(?:>\s?)+")
CLASS_ASSIGN = re.compile(r"^\s*class\s+([\w,\s.-]+?)\s+([\w,-]+)\s*;?\s*$")
CLASS_DEF = re.compile(r"^\s*classDef\s+([\w,-]+)\s+(.+)$")
DEPRECATED_INIT = re.compile(r"%%\{\s*init\s*:")
ER_RELATION = re.compile(r"^\s*([\w-]+)\s*[|o}{]{2}[-.]{2}[|o}{]{2}\s*([\w-]+)\s*:")
FC_EDGE = re.compile(r"([<ox]?[-=.~]{2,}[>ox]?(?:\|[^|]*\|)?)")
FC_EDGE_ID = re.compile(r"(\w+)@(?=[-=.~<])")
FC_SHAPE = re.compile(r"(\[\[.*?\]\]|\[\(.*?\)\]|\(\(.*?\)\)|\(\[.*?\]\)|\{\{.*?\}\}|\[/.*?/\]|\[\\.*?\\\]|(?<![-=<>])>[^\]]*\]|\[.*?\]|\(.*?\)|\{.*?\})")
FC_SKIP = re.compile(r"^\s*(subgraph\b|end\b|direction\b|classDef\b|linkStyle\b|style\b|click\b|class\b|accTitle|accDescr)")
FC_STR = re.compile(r'("[^"]*"|`[^`]*`)')
FENCE_OPEN = re.compile(r"^((?:\s*>)*\s*)(`{3,}|~{3,})\s*mermaid\b")
HEX_COLOR = re.compile(r"#[0-9A-Fa-f]{6}\b")
IDENT = re.compile(r"[A-Za-z0-9_]+")
LINK_STYLE = re.compile(r"^\s*linkStyle\s+([0-9,\s]+|default)\s+(.+)$")
SQ_ARROW = re.compile(r"^\s*([\w-]+)\s*(?:<<-?>>|--?(?:>>|>|\)|x))\s*[+-]?\s*([\w-]+)\s*:")
SQ_PARTICIPANT = re.compile(r"^(?:create\s+)?(?:participant|actor)\s+([\w-]+)(?:@\{.*\})?(?:\s+as\s+.+)?\s*$")
ST_TRANSITION = re.compile(r"^\s*(\[\*\]|[\w.]+)\s*-->\s*(\[\*\]|[\w.]+)")
THEME_BASE = re.compile(r"^\s*theme\s*:\s*base\s*$", re.MULTILINE)

BUDGETS = {"flowchart-edges": 12, "flowchart-nodes": 12, "sequence-participants": 6, "state-states": 12, "er-entities": 8, "class-classes": 10}
CANON = frozenset({"primary", "boundary", "success", "error", "external", "data", "payload", "recessed", "annotation"})
PALETTE = frozenset({
    "#036A96", "#14710A", "#1F1F1F", "#21222C", "#282A36", "#44475A", "#50FA7B", "#6272A4", "#644AC9", "#6C664B",
    "#846E15", "#8BE9FD", "#A3144D", "#A34D14", "#BD93F9", "#CB3A2A", "#CFCFDE", "#F1FA8C", "#F8F8F2", "#FF5555",
    "#FF79C6", "#FFB86C", "#FFFBEB",
})
RENDER_CONFIG = {
    "theme": "base",
    "deterministicIds": True,
    "deterministicIDSeed": "mermaid-corpus",
    "handDrawnSeed": 1001,
    "architecture": {"randomize": False, "seed": 1001},
    "flowchart": {"defaultRenderer": "elk"},
}
PUPPETEER_CONFIG = {
    "executablePath": "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
    "args": ["--no-sandbox", "--disable-dev-shm-usage"],
}
THEMED = frozenset({
    "architecture-beta", "classDiagram", "erDiagram", "eventmodeling", "flowchart", "gantt", "gitGraph", "graph", "journey",
    "pie", "quadrantChart", "radar-beta", "requirementDiagram", "sankey", "sankey-beta", "sequenceDiagram", "stateDiagram",
    "stateDiagram-v2", "swimlane", "timeline", "treemap", "treemap-beta", "venn-beta", "wardley-beta", "xychart", "xychart-beta",
})


# --- [MODELS] ----------------------------------------------------------------------------

class Row(msgspec.Struct, frozen=True):
    file: str
    line: int
    check: Check
    status: Status
    detail: str


class Fence(msgspec.Struct, frozen=True):
    file: str
    line: int
    body: str
    unclosed: bool = False


class LinkStyle(msgspec.Struct, frozen=True):
    line: str
    indices: tuple[int, ...]
    style: str
    default: bool = False


class ClassDef(msgspec.Struct, frozen=True):
    name: str
    style: str


class ClassUse(msgspec.Struct, frozen=True):
    target: str
    name: str


class Edge(msgspec.Struct, frozen=True):
    index: int
    source: str
    target: str
    token: str
    label: str


class Diagram(msgspec.Struct, frozen=True):
    fence: Fence
    family: Family
    header: str
    frontmatter: str
    lines: tuple[str, ...]
    edges: tuple[Edge, ...]
    links: tuple[LinkStyle, ...]
    defs: tuple[ClassDef, ...]
    uses: tuple[ClassUse, ...]
    containers: tuple[str, ...]


# --- [OPERATIONS] ------------------------------------------------------------------------

def emit(row: Row, json_mode: bool) -> None:
    print(ENCODER.encode(row).decode() if json_mode else f"{row.file}:{row.line}: {row.status.upper()} {row.check.value} {row.detail}")


def exit_code(rows: Iterable[Row]) -> int:
    return 1 if any(row.status == "fail" for row in rows) else 0


def row(fence: Fence, check: Check, status: Status, detail: str) -> Row:
    return Row(file=fence.file, line=fence.line, check=check, status=status, detail=detail)


def collect(paths: tuple[Path, ...]) -> tuple[tuple[Path, ...], tuple[Row, ...]]:
    files: list[Path] = []
    rows: list[Row] = []
    for path in paths:
        if path.is_dir():
            found = sorted(child for child in path.rglob("*") if child.suffix in SUFFIXES and child.is_file())
            files.extend(found)
            if not found:
                rows.append(Row(str(path), 0, Check.COLLECT, "fail", "empty target directory"))
        elif path.suffix in SUFFIXES and path.is_file():
            files.append(path)
        else:
            rows.append(Row(str(path), 0, Check.COLLECT, "fail", "not a markdown or mmd target"))
    return tuple(dict.fromkeys(files)), tuple(rows)


def read_fences(path: Path) -> tuple[tuple[Fence, ...], tuple[Row, ...]]:
    try:
        text = path.read_text(encoding="utf-8")
    except (OSError, UnicodeDecodeError) as exc:
        return (), (Row(str(path), 0, Check.READ, "fail", type(exc).__name__),)
    if path.suffix == ".mmd":
        return ((Fence(str(path), 1, text),) if text.strip() else ()), ()
    out: list[Fence] = []
    lines = text.splitlines()
    index = 0
    while index < len(lines):
        opened = FENCE_OPEN.match(lines[index])
        if opened is None:
            index += 1
            continue
        quoted = ">" in opened.group(1)
        marker = re.escape(opened.group(2)[0])
        closed_by = re.compile(rf"^(?:\s*>)*\s*{marker}{{{len(opened.group(2))},}}\s*$")
        body: list[str] = []
        cursor = index + 1
        while cursor < len(lines) and closed_by.match(lines[cursor]) is None:
            body.append(BLOCKQUOTE.sub("", lines[cursor]) if quoted else lines[cursor])
            cursor += 1
        out.append(Fence(str(path), index + 2, "\n".join(body), cursor == len(lines)))
        index = cursor + 1
    return tuple(out), ()


def family(header: str) -> Family:
    if header in {"flowchart", "graph", "swimlane", "railroad"}:
        return Family.FLOWCHART
    if header.startswith("stateDiagram"):
        return Family.STATE
    if header == "sequenceDiagram":
        return Family.SEQUENCE
    if header == "erDiagram":
        return Family.ER
    if header == "classDiagram":
        return Family.CLASS
    if header == "gantt":
        return Family.GANTT
    if header == "architecture-beta":
        return Family.ARCHITECTURE
    if header == "requirementDiagram":
        return Family.REQUIREMENT
    if header.startswith("C4"):
        return Family.C4
    return Family.UNKNOWN


def frontmatter(body: str) -> tuple[str, tuple[str, ...], tuple[Row, ...]]:
    lines = body.splitlines()
    if not lines or lines[0].strip() != "---":
        return "", tuple(lines), ()
    close = next((index for index, line in enumerate(lines[1:], 1) if line.strip() == "---"), -1)
    if close < 0:
        return "", (), (Row("-", 0, Check.LOGIC, "fail", "unclosed-frontmatter"),)
    return "\n".join(lines[1:close]), tuple(lines[close + 1:]), ()


def body_lines(lines: tuple[str, ...]) -> tuple[str, tuple[str, ...]]:
    body = tuple(line for line in lines if line.strip() and not line.strip().startswith("%%"))
    if not body:
        return "", ()
    return body[0].strip().split()[0], tuple(line for line in body[1:] if ACCESS.match(line) is None and not line.strip().startswith("title "))


def class_rows(lines: tuple[str, ...]) -> tuple[tuple[ClassDef, ...], tuple[ClassUse, ...]]:
    defs: list[ClassDef] = []
    uses: list[ClassUse] = []
    for line in lines:
        if match := CLASS_DEF.match(line):
            defs.extend(ClassDef(name, match.group(2)) for name in match.group(1).split(","))
        for target, names in re.findall(r"([\w.-]+):::([\w,-]+)", line):
            uses.extend(ClassUse(target, name) for name in names.split(","))
        if match := CLASS_ASSIGN.match(line):
            targets = [part for part in re.split(r"[\s,]+", match.group(1)) if part]
            uses.extend(ClassUse(target, name) for target in targets for name in match.group(2).split(","))
    return tuple(defs), tuple(uses)


def link_rows(lines: tuple[str, ...]) -> tuple[LinkStyle, ...]:
    out: list[LinkStyle] = []
    for line in lines:
        if match := LINK_STYLE.match(line):
            raw = match.group(1).strip()
            out.append(LinkStyle(line, () if raw == "default" else tuple(int(part) for part in raw.replace(" ", "").split(",") if part), match.group(2), raw == "default"))
    return tuple(out)


def flow_edges(lines: tuple[str, ...]) -> tuple[Edge, ...]:
    edges: list[Edge] = []
    for raw in lines:
        if FC_SKIP.match(raw):
            continue
        line = re.sub(r"\w+@\{[^}]*\}", " ", raw)
        line = FC_EDGE_ID.sub("", FC_STR.sub("QL", line))
        parts = FC_EDGE.split(FC_SHAPE.sub(" ", line))
        groups = [IDENT.findall(part) for part in parts[::2]]
        tokens = parts[1::2]
        nodes = [group for group in groups if group]
        for offset, (left, right) in enumerate(pairwise(nodes)):
            token = tokens[offset] if offset < len(tokens) else "-->"
            label = token.strip("|").strip() if "|" in token else ""
            edges.extend(Edge(len(edges), a, b, token, label) for a in left for b in right)
    return tuple(edges)


def containers(lines: tuple[str, ...]) -> tuple[str, ...]:
    return tuple(line.strip() for line in lines if re.match(r"^(subgraph|alt\b|par\b|critical\b|break\b|state\s+[\w.]+\s*\{)", line.strip()))


def parse(fence: Fence) -> tuple[Diagram | None, tuple[Row, ...]]:
    if fence.unclosed:
        return None, (row(fence, Check.LOGIC, "fail", "unclosed-fence"),)
    fm, source, faults = frontmatter(fence.body)
    faults = tuple(Row(fence.file, fence.line, fault.check, fault.status, fault.detail) for fault in faults)
    header, lines = body_lines(source)
    defs, uses = class_rows(lines)
    diagram = Diagram(fence, family(header), header, fm, lines, flow_edges(lines), link_rows(lines), defs, uses, containers(lines))
    return diagram, faults


def contract(diagram: Diagram) -> tuple[Row, ...]:
    body = diagram.fence.body
    access = [line.strip().split(":", 1)[0].split("{", 1)[0].strip() for line in body.splitlines() if ACCESS.match(line)]
    off_palette = sorted({hex_value.upper() for hex_value in HEX_COLOR.findall(body)} - PALETTE)
    defs = {definition.name: definition for definition in diagram.defs}
    used = {use.name for use in diagram.uses}
    rows = [row(diagram.fence, Check.FRONTMATTER, "warn", "no-frontmatter")] if not body.startswith("---") else []
    rows += [row(diagram.fence, Check.CONTRACT, "warn", "deprecated-init-directive")] if DEPRECATED_INIT.search(body) else []
    rows += [row(diagram.fence, Check.CONTRACT, "warn", "acc-order")] if access and access[:2] != ["accTitle", "accDescr"] else []
    rows += [row(diagram.fence, Check.CONTRACT, "warn", "acc-pair-incomplete")] if ("accTitle" in body) != ("accDescr" in body) else []
    rows += [row(diagram.fence, Check.CONTRACT, "warn", f"off-palette:{value}") for value in off_palette]
    rows += [row(diagram.fence, Check.CONTRACT, "warn", "floor:theme-base")] if diagram.header in THEMED and not THEME_BASE.search(diagram.frontmatter) else []
    rows += [row(diagram.fence, Check.CONTRACT, "warn", "floor:fontFamily")] if diagram.header in THEMED and "fontFamily" not in diagram.frontmatter else []
    rows += [row(diagram.fence, Check.CONTRACT, "warn", "floor:clusterBkg")] if diagram.family == Family.FLOWCHART and any(c.startswith("subgraph") for c in diagram.containers) and "clusterBkg" not in diagram.frontmatter else []
    rows += [row(diagram.fence, Check.CONTRACT, "warn", "flowchart-floor:canonical-class-count")] if diagram.family == Family.FLOWCHART and len(set(defs) & CANON) < 3 else []
    rows += [row(diagram.fence, Check.CONTRACT, "warn", f"class:non-canonical:{name}") for name in sorted(set(defs) - CANON)]
    rows += [row(diagram.fence, Check.CONTRACT, "warn", f"class:unused:{name}") for name in sorted(set(defs) - used)]
    rows += [row(diagram.fence, Check.CONTRACT, "warn", f"class:missing-color:{name}") for name, definition in sorted(defs.items()) if "color:" not in definition.style]
    return tuple(rows)


def style_logic(diagram: Diagram) -> tuple[Row, ...]:
    count = len(diagram.edges)
    rows = [
        row(diagram.fence, Check.LOGIC, "fail", f"linkStyle-out-of-range:{index}")
        for link in diagram.links
        for index in link.indices
        if index >= count
    ]
    styled = {index for link in diagram.links for index in link.indices}
    rows += [
        row(diagram.fence, Check.CONTRACT, "warn", f"edge:semantic-rail:{edge.index}")
        for edge in diagram.edges
        if any(word in f"{edge.label} {edge.source} {edge.target}".lower() for word in ("fault", "error", "reject", "trace", "data", "wire", "external", "receipt")) and edge.index not in styled
    ]
    rows += [
        row(diagram.fence, Check.CONTRACT, "warn", f"edge:fault-not-red:{edge.index}")
        for edge in diagram.edges
        for link in diagram.links
        if edge.index in link.indices and any(word in f"{edge.label} {edge.source} {edge.target}".lower() for word in ("fault", "error", "reject")) and "#FF5555" not in link.style.upper()
    ]
    if diagram.family == Family.SEQUENCE:
        grouped = any(line.strip().startswith(("rect ", "box ")) for line in diagram.lines)
        guarded = any(line.strip().startswith(("alt ", "par ", "critical", "break ")) for line in diagram.lines)
        rows += [row(diagram.fence, Check.CONTRACT, "warn", "sequence-floor:region-without-box-or-rect")] if guarded and not grouped else []
    if diagram.family == Family.C4 and "UpdateElementStyle" not in diagram.fence.body:
        rows += [row(diagram.fence, Check.CONTRACT, "warn", "c4-floor:update-style")]
    return tuple(rows)


def graph_logic(diagram: Diagram) -> tuple[Row, ...]:
    if diagram.family == Family.FLOWCHART:
        graph: nx.MultiDiGraph[str, dict[str, object], dict[str, object]] = nx.MultiDiGraph()
        graph.add_edges_from((edge.source, edge.target) for edge in diagram.edges)
        declared: set[str] = set()
        for line in diagram.lines:
            clean = FC_EDGE_ID.sub("", FC_STR.sub("QL", re.sub(r"\w+@\{[^}]*\}", " ", line)))
            if not FC_SKIP.match(clean) and FC_EDGE.search(clean) is None:
                declared.update(IDENT.findall(FC_SHAPE.sub(" ", clean)))
        endpoint = {node for edge in diagram.edges for node in (edge.source, edge.target)}
        rows = [row(diagram.fence, Check.LOGIC, "fail", f"orphan-node:{node}") for node in sorted(declared - endpoint)]
        rows += [
            row(diagram.fence, Check.LOGIC, "warn", f"duplicate-edge:{a}->{b}")
            for (a, b), count in Counter((edge.source, edge.target) for edge in diagram.edges).items()
            if count > 1
        ]
        rows += [row(diagram.fence, Check.LOGIC, "warn", f"budget:{len(graph.nodes)} nodes > {BUDGETS['flowchart-nodes']}")] if len(graph.nodes) > BUDGETS["flowchart-nodes"] else []
        rows += [row(diagram.fence, Check.LOGIC, "warn", f"budget:{len(diagram.edges)} edges > {BUDGETS['flowchart-edges']}")] if len(diagram.edges) > BUDGETS["flowchart-edges"] else []
        return tuple(rows)
    if diagram.family == Family.STATE:
        reach: nx.DiGraph[str, dict[str, object], dict[str, object]] = nx.DiGraph()
        states, starts = set[str](), set[str]()
        for line in diagram.lines:
            if match := ST_TRANSITION.match(line):
                left, right = match.group(1), match.group(2)
                states.update(node for node in (left, right) if node != "[*]")
                starts.update({right} if left == "[*]" and right != "[*]" else set())
                reach.add_edge(left, right)
        reachable = set().union(*(nx.descendants(reach, start) | {start} for start in starts), set()) if starts else states
        return tuple(row(diagram.fence, Check.LOGIC, "fail", f"unreachable-state:{state}") for state in sorted(states - reachable))
    if diagram.family == Family.SEQUENCE:
        declared, used = set[str](), set[str]()
        for line in diagram.lines:
            declared.update(match.group(1) for match in [SQ_PARTICIPANT.match(line.strip())] if match)
            used.update(name for match in [SQ_ARROW.match(line)] if match for name in (match.group(1), match.group(2)))
        rows = [row(diagram.fence, Check.LOGIC, "warn", f"orphan-participant:{name}") for name in sorted(declared - used)]
        rows += [row(diagram.fence, Check.LOGIC, "warn", f"budget:{len(declared | used)} participants > {BUDGETS['sequence-participants']}")] if len(declared | used) > BUDGETS["sequence-participants"] else []
        return tuple(rows)
    if diagram.family == Family.ER:
        blocks, related = set[str](), set[str]()
        for line in diagram.lines:
            related.update(name for match in [ER_RELATION.match(line)] if match for name in (match.group(1), match.group(2)))
            blocks.update(match.group(1) for match in [re.match(r"^\s*([\w-]+)\s*\{", line)] if match)
        return tuple(row(diagram.fence, Check.LOGIC, "warn", f"orphan-entity:{name}") for name in sorted(blocks - related))
    return (row(diagram.fence, Check.LOGIC, "warn", f"logic-unimplemented:{diagram.header}"),) if diagram.family == Family.UNKNOWN and diagram.header else ()


def class_logic(diagram: Diagram) -> tuple[Row, ...]:
    defs = {definition.name for definition in diagram.defs} | {"default"}
    targets = {edge.source for edge in diagram.edges} | {edge.target for edge in diagram.edges}
    return tuple(
        [row(diagram.fence, Check.LOGIC, "fail", f"undefined-class:{use.name}") for use in diagram.uses if use.name not in defs]
        + [
            row(diagram.fence, Check.LOGIC, "fail", f"unknown-node-in-class:{use.target}")
            for use in diagram.uses
            if diagram.family == Family.FLOWCHART and targets and use.target not in targets
        ]
    )


def resolve_renderer(override: str | None) -> tuple[tuple[str, ...], Path | None]:
    if override:
        argv = tuple(shlex.split(override))
        return (argv, None) if argv and shutil.which(argv[0]) else ((), None)
    probe = Path.cwd().resolve()
    for root in (probe, *probe.parents):
        if (root / "pnpm-lock.yaml").exists():
            return ("pnpm", "exec", "mmdc"), root
    return (("mmdc",), None) if shutil.which("mmdc") else (("npx", "-y", "-p", "@mermaid-js/mermaid-cli", "mmdc"), None) if shutil.which("npx") else ((), None)


def render(prefix: tuple[str, ...], cwd: Path | None, diagram: Diagram, workdir: Path, cache_dir: Path) -> Row:
    cache_dir.mkdir(parents=True, exist_ok=True)
    config = ENCODER.encode(RENDER_CONFIG)
    key = xxh3_128_hexdigest(diagram.fence.body.encode() + b"\0".join(part.encode() for part in prefix) + config)
    cached = cache_dir / f"{key}.svg"
    if cached.exists() and cached.read_text(encoding="utf-8", errors="ignore").lstrip().startswith("<svg"):
        return row(diagram.fence, Check.RENDER, "ok", f"render-cache-hit:{cached}")
    src, out, cfg, browser = workdir / f"{key}.mmd", workdir / f"{key}.svg", workdir / "mermaid-config.json", workdir / "puppeteer-config.json"
    src.write_text(diagram.fence.body, encoding="utf-8")
    cfg.write_bytes(config)
    browser.write_bytes(ENCODER.encode(PUPPETEER_CONFIG))
    try:
        proc = subprocess.run(
            [*prefix, "-q", "-i", str(src), "-o", str(out), "-c", str(cfg), "-p", str(browser)],
            cwd=cwd,
            capture_output=True,
            text=True,
            timeout=RENDER_TIMEOUT,
            check=False,
        )
    except subprocess.TimeoutExpired:
        return row(diagram.fence, Check.RENDER, "fail", f"environment:render-timeout:{RENDER_TIMEOUT}s")
    except OSError as exc:
        return row(diagram.fence, Check.RENDER, "fail", f"environment:renderer-unavailable:{type(exc).__name__}")
    raw = next((line for line in (proc.stderr or proc.stdout).splitlines() if line.strip()), "renderer failed")
    if proc.returncode != 0 or not out.exists():
        stage = "environment" if any(marker in raw.lower() for marker in ENV_MARKERS) else "syntax"
        return row(diagram.fence, Check.RENDER, "fail", f"{stage}:{raw.strip()[:280]}")
    if not out.read_text(encoding="utf-8", errors="ignore").lstrip().startswith("<svg"):
        return row(diagram.fence, Check.RENDER, "fail", "syntax:invalid-svg-root")
    cached.write_bytes(out.read_bytes())
    return row(diagram.fence, Check.RENDER, "ok", f"rendered:{cached}")


def static_rows(fences: tuple[Fence, ...]) -> tuple[Row, ...]:
    rows: list[Row] = []
    for fence in fences:
        diagram, faults = parse(fence)
        rows.extend(faults)
        if diagram is not None:
            rows.extend((*contract(diagram), *style_logic(diagram), *graph_logic(diagram), *class_logic(diagram)))
    return tuple(rows)


# --- [COMPOSITION] -----------------------------------------------------------------------

@APP.default
def main(*paths: Path, json: bool = False, no_render: bool = False, renderer: str | None = None, cache_dir: Path = Path(".cache/mermaid"), jobs: int = 4) -> int:
    del jobs
    files, rows = collect(paths)
    fences: list[Fence] = []
    for file in files:
        found, read_rows = read_fences(file)
        fences.extend(found)
        rows = (*rows, *read_rows)
    parsed = tuple(parse(fence)[0] for fence in fences)
    diagrams = tuple(diagram for diagram in parsed if diagram is not None)
    rows = (*rows, *static_rows(tuple(fences)))
    if not no_render and diagrams:
        prefix, cwd = resolve_renderer(renderer)
        if not prefix:
            rows = (*rows, Row("-", 0, Check.SETUP, "fail", "no mermaid renderer"))
        else:
            with TemporaryDirectory(prefix="mermaid-validate-") as tmp:
                workdir = Path(tmp)
                rows = (*rows, *(render(prefix, cwd, diagram, workdir, cache_dir) for diagram in diagrams))
    for output in rows:
        emit(output, json)
    return exit_code(rows)


if __name__ == "__main__":
    sys.exit(APP())
