#!/usr/bin/env python3
# ruff: noqa: T201 — stdout is this gate's output contract
"""Validate every mermaid fence in the given files: graph-logic checks, then render. Exit 0 iff no fence fails.

Usage: validate_mermaid.py [--json] [--no-render] [--renderer CMD] <path>...
Paths: .md/.mmd files or directories (walked for *.md, *.mmd). Files without fences are skipped.
Output: `file:line: ok|FAIL <reason>|WARN <reason>` per fence; --json emits NDJSON rows
{"file","line","check","status","detail"} with check in render|frontmatter|logic|setup|read.
Logic rows fire only on findings: structural defects (orphan node, unreachable state, undefined
class, unknown node) fail; legibility-budget overruns and duplicate edges warn. --no-render skips
renderer resolution and reports logic and frontmatter checks alone. An unreadable file emits one
check=read fail row.
"""

# --- [RUNTIME_PRELUDE] -------------------------------------------------------------------
from __future__ import annotations

import argparse
from itertools import pairwise
import json
from pathlib import Path
import re
import shlex
import shutil
import subprocess
import sys
import tempfile
from typing import NamedTuple


# --- [CONSTANTS] -------------------------------------------------------------------------

BLOCKQUOTE = re.compile(r"^\s*(?:>\s?)+")
FENCE_OPEN = re.compile(r"^((?:\s*>)*\s*)(`{3,}|~{3,})\s*mermaid\b")
ENV_MARKERS = ("chrome", "chromium", "puppeteer", "browser", "libnss", "shared librar", "sandbox", "econnrefused", "enoent")
RENDER_TIMEOUT = 60
SUFFIXES = (".md", ".mmd")

# Legibility budgets per diagram family, mirroring the catalog law: past the ceiling a diagram splits, so overrun is a warn.
BUDGETS = {"flowchart-nodes": 12, "flowchart-edges": 12, "sequence-participants": 6, "state-states": 12, "er-entities": 8}

# Flowchart tokenization: edge runs (with optional edge-id prefix and inline label), shape-bracket
# payloads, and metadata that must vanish before endpoints are read.
FC_EDGE = re.compile(r"([<ox]?[-=.~]{2,}[>ox]?(?:\|[^|]*\|)?)")
FC_AT = re.compile(r"@\{[^}]*\}")
FC_STR = re.compile(r'("[^"]*"|`[^`]*`)')
FC_SHAPE = re.compile(r"(\[\[.*?\]\]|\[\(.*?\)\]|\(\(.*?\)\)|\(\[.*?\]\)|\{\{.*?\}\}|\[/.*?/\]|\[\\.*?\\\]|(?<![-=<>])>[^\]]*\]|\[.*?\]|\(.*?\)|\{.*?\})")
FC_SKIP = re.compile(r"^\s*(subgraph\b|end\b|direction\b|classDef\b|linkStyle\b|style\b|click\b|class\b|accTitle|accDescr)")
IDENT = re.compile(r"[A-Za-z0-9_]+")

ST_TRANSITION = re.compile(r"^\s*(\[\*\]|[\w.]+)\s*-->\s*(\[\*\]|[\w.]+)")
SQ_ARROW = re.compile(r"^\s*(\w+)\s*(?:<<-?>>|--?(?:>>|>|\)|x))\s*[+-]?\s*(\w+)\s*:")
ER_RELATION = re.compile(r"^\s*([\w-]+)\s*[|o}{]{2}[-.]{2}[|o}{]{2}\s*([\w-]+)\s*:")

# --- [MODELS] ----------------------------------------------------------------------------


class Fence(NamedTuple):
    """One mermaid fence: 1-based start line and dedented body."""

    line: int
    body: str


# --- [OPERATIONS] ------------------------------------------------------------------------


def collect(paths: list[str]) -> list[Path]:
    """Expand argv paths to .md/.mmd files, directories walked recursively.

    Returns:
        Unique files in first-seen order.
    """
    files: list[Path] = []
    for raw in paths:
        p = Path(raw)
        if p.is_dir():
            files.extend(sorted(q for s in SUFFIXES for q in p.rglob(f"*{s}")))
        elif p.suffix in SUFFIXES and p.is_file():
            files.append(p)
    return list(dict.fromkeys(files))


def fences(path: Path) -> list[Fence]:
    """Extract mermaid fences, blockquote prefixes stripped from quoted bodies.

    Returns:
        Fences in file order; a .mmd file is one fence at line 1. Read faults propagate as
        OSError for the caller to convert into a fault row.
    """
    text = path.read_text(encoding="utf-8", errors="replace")
    if path.suffix == ".mmd":
        return [Fence(1, text)] if text.strip() else []
    out: list[Fence] = []
    lines = text.splitlines()
    i = 0
    while i < len(lines):
        m = FENCE_OPEN.match(lines[i])
        if not m:
            i += 1
            continue
        quoted = ">" in m.group(1)
        close = re.compile(rf"^(?:\s*>)*\s*{re.escape(m.group(2)[0])}{{{len(m.group(2))},}}\s*$")
        body: list[str] = []
        j = i + 1
        while j < len(lines) and not close.match(lines[j]):
            body.append(BLOCKQUOTE.sub("", lines[j]) if quoted else lines[j])
            j += 1
        out.append(Fence(i + 2, "\n".join(body)))
        i = j + 1
    return out


# --- [LOGIC]


class Finding(NamedTuple):
    """One graph-logic verdict: fail for structural defects, warn for legibility pressure."""

    status: str
    detail: str


def _diagram_lines(body: str) -> tuple[str, list[str]]:
    """Strip frontmatter, comments, and accessibility directives.

    Returns:
        (header word, remaining lines after the header).
    """
    lines = body.splitlines()
    i = 0
    while i < len(lines) and not lines[i].strip():
        i += 1
    if i < len(lines) and lines[i].strip() == "---":
        i += 1
        while i < len(lines) and lines[i].strip() != "---":
            i += 1
        i += 1
    out: list[str] = []
    in_desc = False
    for ln in lines[i:]:
        s = ln.strip()
        if in_desc:
            in_desc = "}" not in s
            continue
        if s.startswith("accDescr") and "{" in s and "}" not in s:
            in_desc = True
            continue
        if s.startswith(("%%", "accTitle", "accDescr", "title ")) or not s:
            continue
        out.append(ln)
    return (out[0].strip().split()[0] if out else "", out[1:])


def _flowchart_logic(lines: list[str]) -> list[Finding]:
    """Orphan nodes, undefined classes, unknown class targets, duplicate edges, budgets.

    Returns:
        Findings in defect-class order; empty when the graph is clean.
    """
    class_defs, class_uses, class_nodes = {"default"}, set(), set()
    subgraphs, declared_only, endpoints = set(), set(), set()
    edges: list[tuple[str, str, str]] = []
    for raw in lines:
        if m := re.match(r"^\s*\w+@\{([^}]*)\}\s*;?\s*$", raw):
            keys = set(re.findall(r"([A-Za-z]\w*)\s*:", m.group(1)))
            if keys and keys <= {"animate", "animation", "curve"}:
                continue
        line = FC_AT.sub(" ", raw)
        line = FC_STR.sub(lambda m: f"QL{abs(hash(m.group(0))) & 0xFFFF:04x}", line)
        for node, names in re.findall(r"(\w+):::([\w,]+)", line):
            class_uses.update(names.split(","))
            class_nodes.add(node)
        line = re.sub(r":::[\w,]+", "", line)
        if m := re.match(r"^\s*subgraph\s+([\w-]+)", line):
            subgraphs.add(m.group(1))
            continue
        if m := re.match(r"^\s*classDef\s+([\w,]+)", line):
            class_defs.update(m.group(1).split(","))
            continue
        if m := re.match(r"^\s*class\s+([\w,\s]+?)\s+([\w,]+)\s*;?\s*$", line):
            class_nodes.update(t for t in re.split(r"[\s,]+", m.group(1)) if t)
            class_uses.update(m.group(2).split(","))
            continue
        if FC_SKIP.match(line):
            continue
        line = re.sub(r"(\w+)@(?=[-=.~<])", "", line)
        line = re.sub(r"--\s[^-]*?\s-->", "-->", line)
        line = re.sub(r"-\.\s[^.]*?\s\.->", "-.->", line)
        line = re.sub(r"==\s[^=]*?\s==>", "==>", line)
        line = FC_SHAPE.sub(" ", line)
        parts = FC_EDGE.split(line)
        groups = [(IDENT.findall(c), c) for c in parts[::2]]
        seps = parts[1::2]
        if not seps:
            declared_only.update(groups[0][0] if groups and groups[0][0] else [])
            continue
        idents = [g for g, _ in groups if g]
        for k, (a_set, b_set) in enumerate(pairwise(idents)):
            label = re.sub(r"\s+", "", seps[k]) if k < len(seps) else ""
            for a in a_set:
                for b in b_set:
                    edges.append((a, b, label))
                    endpoints.update((a, b))
    nodes = (declared_only | endpoints | class_nodes) - subgraphs
    findings = [Finding("fail", f"orphan-node:{n}") for n in sorted(declared_only - endpoints - subgraphs)]
    findings += [Finding("fail", f"undefined-class:{c}") for c in sorted(class_uses - class_defs)]
    findings += [Finding("fail", f"unknown-node-in-class:{n}") for n in sorted(class_nodes - declared_only - endpoints - subgraphs)]
    dupes = {e for e in edges if edges.count(e) > 1}
    findings += [Finding("warn", f"duplicate-edge:{a}-->{b}") for a, b, _ in sorted(dupes)]
    if len(nodes) > BUDGETS["flowchart-nodes"]:
        findings.append(Finding("warn", f"budget:{len(nodes)} nodes > {BUDGETS['flowchart-nodes']}"))
    if len(edges) > BUDGETS["flowchart-edges"]:
        findings.append(Finding("warn", f"budget:{len(edges)} edges > {BUDGETS['flowchart-edges']}"))
    return findings


def _state_logic(lines: list[str]) -> list[Finding]:
    """Unreachable states (when a start marker exists), undefined classes, state budget.

    Returns:
        Findings in defect-class order; empty when the diagram is clean.
    """
    class_defs, class_uses = {"default"}, set()
    states, indeg = set(), set()
    has_start = False
    for raw in lines:
        line = raw
        for _, names in re.findall(r"([\w.]+):::([\w,]+)", line):
            class_uses.update(names.split(","))
        line = re.sub(r":::[\w,]+", "", line)
        s = line.strip()
        if m := re.match(r"^classDef\s+([\w,]+)", s):
            class_defs.update(m.group(1).split(","))
            continue
        if m := re.match(r"^class\s+([\w,\s]+?)\s+([\w,]+)\s*;?\s*$", s):
            class_uses.update(m.group(2).split(","))
            continue
        if s.startswith(("note", "direction", "}", "--")):
            continue
        if m := re.match(r'^state\s+"[^"]*"\s+as\s+([\w.]+)', s):
            states.add(m.group(1))
            continue
        if m := re.match(r"^state\s+([\w.]+)", s):
            states.add(m.group(1))
            continue
        if m := ST_TRANSITION.match(line):
            a, b = m.group(1), m.group(2)
            has_start = has_start or a == "[*]"
            states.update(t for t in (a, b) if t != "[*]")
            if b != "[*]":
                indeg.add(b)
            continue
        if m := re.match(r"^([\w.]+)\s*:", s):
            states.add(m.group(1))
        elif m := re.match(r"^([\w.]+)\s*\{", s):
            states.add(m.group(1))
    findings = [Finding("fail", f"undefined-class:{c}") for c in sorted(class_uses - class_defs)]
    if has_start:
        findings += [Finding("fail", f"unreachable-state:{n}") for n in sorted(states - indeg)]
    if len(states) > BUDGETS["state-states"]:
        findings.append(Finding("warn", f"budget:{len(states)} states > {BUDGETS['state-states']}"))
    return findings


def _sequence_logic(lines: list[str]) -> list[Finding]:
    """Orphan declared participants and the participant budget.

    Returns:
        Findings in defect-class order; empty when the diagram is clean.
    """
    declared, used = set(), set()
    for raw in lines:
        s = raw.strip()
        if m := re.match(r"^(?:create\s+)?(?:participant|actor)\s+(\w+)", s):
            declared.add(m.group(1))
            continue
        if m := re.match(r"^destroy\s+(\w+)", s):
            used.add(m.group(1))
            continue
        if m := re.match(r"^[Nn]ote\s+(?:over|left of|right of)\s+([\w,\s]+?):", s):
            used.update(t for t in re.split(r"[\s,]+", m.group(1)) if t)
            continue
        if m := SQ_ARROW.match(raw):
            used.update((m.group(1), m.group(2)))
    findings = [Finding("warn", f"orphan-participant:{p}") for p in sorted(declared - used)]
    if len(declared | used) > BUDGETS["sequence-participants"]:
        findings.append(Finding("warn", f"budget:{len(declared | used)} participants > {BUDGETS['sequence-participants']}"))
    return findings


def _er_logic(lines: list[str]) -> list[Finding]:
    """Orphan entities (attribute block, zero relations) and the entity budget.

    Returns:
        Findings in defect-class order; empty when the diagram is clean.
    """
    blocks, related = set(), set()
    for raw in lines:
        if m := ER_RELATION.match(raw):
            related.update((m.group(1), m.group(2)))
        elif m := re.match(r"^\s*([\w-]+)\s*\{", raw):
            blocks.add(m.group(1))
    findings = [Finding("warn", f"orphan-entity:{e}") for e in sorted(blocks - related)] if related else []
    entities = blocks | related
    if len(entities) > BUDGETS["er-entities"]:
        findings.append(Finding("warn", f"budget:{len(entities)} entities > {BUDGETS['er-entities']}"))
    return findings


def logic_findings(body: str) -> list[Finding]:
    """Dispatch graph-logic analysis by diagram header; unknown types return no findings.

    Returns:
        Findings in defect-class order; empty when the diagram is clean.
    """
    header, lines = _diagram_lines(body)
    if header in {"flowchart", "graph"}:
        return _flowchart_logic(lines)
    if header.startswith("stateDiagram"):
        return _state_logic(lines)
    if header == "sequenceDiagram":
        return _sequence_logic(lines)
    if header == "erDiagram":
        return _er_logic(lines)
    return []


def renderer(override: str | None) -> tuple[list[str], Path | None]:
    """Resolve the mmdc invocation: explicit override, pnpm workspace, PATH, then npx.

    Returns:
        (argv prefix, cwd for pnpm resolution); ([], None) when no renderer exists.
    """
    if override:
        return shlex.split(override), None
    probe = Path.cwd().resolve()
    for root in (probe, *probe.parents):
        if (root / "pnpm-lock.yaml").exists():
            return ["pnpm", "exec", "mmdc"], root
    if shutil.which("mmdc"):
        return ["mmdc"], None
    if shutil.which("npx"):
        return ["npx", "-y", "@mermaid-js/mermaid-cli"], None
    return [], None


def render(prefix: list[str], cwd: Path | None, body: str, workdir: Path) -> tuple[bool, str]:
    """Render one fence body to SVG through the resolved renderer.

    Returns:
        (ok, first stderr line on failure).
    """
    src = workdir / "fence.mmd"
    out = workdir / "fence.svg"
    src.write_text(body, encoding="utf-8")
    out.unlink(missing_ok=True)
    try:
        proc = subprocess.run(
            [*prefix, "-q", "-i", str(src), "-o", str(out)],
            cwd=cwd, capture_output=True, text=True, timeout=RENDER_TIMEOUT,
        )
    except subprocess.TimeoutExpired:
        return False, f"environment: render timeout ({RENDER_TIMEOUT}s)"
    if proc.returncode == 0 and out.exists():
        return True, ""
    raw = next((ln for ln in (proc.stderr or proc.stdout).splitlines() if ln.strip()), "renderer failed")
    stage = "environment" if any(k in raw.lower() for k in ENV_MARKERS) else "syntax"
    return False, f"{stage}: {raw.strip()[:280]}"


# --- [COMPOSITION] -----------------------------------------------------------------------


def main(argv: list[str]) -> int:
    """Render-validate every fence under argv paths; emit one row per fence.

    Returns:
        Process exit code: 0 no fence failed, 1 otherwise.
    """
    ap = argparse.ArgumentParser(add_help=True)
    ap.add_argument("--json", action="store_true")
    ap.add_argument("--no-render", action="store_true")
    ap.add_argument("--renderer", default=None)
    ap.add_argument("paths", nargs="+")
    ns = ap.parse_args(argv)

    def emit(file: str, line: int, check: str, status: str, detail: str) -> None:
        if ns.json:
            print(json.dumps({"file": file, "line": line, "check": check, "status": status, "detail": detail}))
        else:
            tag = {"ok": "ok", "warn": f"WARN {detail}", "fail": f"FAIL {detail}"}[status]
            print(f"{file}:{line}: {tag}")

    failed = False
    targets: list[tuple[Path, list[Fence]]] = []
    for path in collect(ns.paths):
        try:
            fence_list = fences(path)
        except OSError as exc:
            emit(str(path), 0, "read", "fail", type(exc).__name__)
            failed = True
            continue
        if fence_list:
            targets.append((path, fence_list))
    if not targets:
        return 1 if failed else 0
    if ns.no_render:
        for path, fence_list in targets:
            for fence in fence_list:
                if not fence.body.lstrip().startswith("---"):
                    emit(str(path), fence.line, "frontmatter", "warn", "no-frontmatter")
                for finding in logic_findings(fence.body):
                    failed = failed or finding.status == "fail"
                    emit(str(path), fence.line, "logic", finding.status, finding.detail)
        return 1 if failed else 0
    prefix, cwd = renderer(ns.renderer)
    if not prefix:
        emit("-", 0, "setup", "fail", "no mermaid renderer: need pnpm workspace mmdc, mmdc on PATH, or npx")
        return 1

    with tempfile.TemporaryDirectory(prefix="mermaid-validate-") as tmp:
        workdir = Path(tmp)
        for path, fence_list in targets:
            rel = str(path)
            for fence in fence_list:
                ok, detail = render(prefix, cwd, fence.body, workdir)
                failed = failed or not ok
                emit(rel, fence.line, "render", "ok" if ok else "fail", detail)
                if ok and not fence.body.lstrip().startswith("---"):
                    emit(rel, fence.line, "frontmatter", "warn", "no-frontmatter")
                for finding in logic_findings(fence.body):
                    failed = failed or finding.status == "fail"
                    emit(rel, fence.line, "logic", finding.status, finding.detail)
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
