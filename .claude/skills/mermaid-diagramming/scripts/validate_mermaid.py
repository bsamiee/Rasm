#!/usr/bin/env python3
# ruff: noqa: T201 — stdout is this gate's output contract
"""Validate every mermaid fence in the given files: graph-logic checks, then render. Exit 0 iff no fence fails.

Usage: validate_mermaid.py [--json] [--no-render] [--renderer CMD] <path>...
Paths: .md/.mmd files or directories (walked for *.md, *.mmd); a missing path or non-target
file emits a check=collect fail row. Files without fences are skipped. Output:
`file:line: ok|FAIL <reason>|WARN <reason>` per fence; --json emits NDJSON rows
{"file","line","check","status","detail"} with check in render|frontmatter|contract|logic|setup|read|collect.
Contract rows warn on an inconsistent accTitle/accDescr pair and on off-palette hexes in a
fence that declares themeVariables.
Logic rows fire only on findings: structural defects (orphan node, unreachable state, undefined
class, unknown node, unclosed fence or frontmatter, deprecated init directive) fail;
legibility-budget overruns, duplicate edges, and a diagram family without implemented logic
warn. --no-render skips renderer resolution and reports logic and frontmatter checks alone.
Unreadable or invalid-UTF-8 input emits one check=read fail row.
"""

# --- [RUNTIME_PRELUDE] -------------------------------------------------------------------

import argparse
from collections import Counter
from dataclasses import dataclass
from itertools import pairwise
import json
from pathlib import Path
import re
import shlex
import shutil
import subprocess
import sys
import tempfile
import zlib


# --- [CONSTANTS] ---------------------------------------------------------------------------

BLOCKQUOTE = re.compile(r"^\s*(?:>\s?)+")
DEPRECATED_INIT = re.compile(r"%%\{\s*init\s*:")
FENCE_OPEN = re.compile(r"^((?:\s*>)*\s*)(`{3,}|~{3,})\s*mermaid\b")
ENV_MARKERS = ("chrome", "chromium", "puppeteer", "browser", "libnss", "shared librar", "sandbox", "econnrefused", "enoent")
RENDER_TIMEOUT = 60
SUFFIXES = (".md", ".mmd")

# Legibility budgets per diagram family, mirroring the catalog law: past the ceiling a diagram splits, so overrun is a warn.
BUDGETS = {"flowchart-nodes": 12, "flowchart-edges": 12, "sequence-participants": 6, "state-states": 12, "er-entities": 8, "class-classes": 10}

# Committed-diagram color vocabulary: Dracula plus the Alucard light complement. A themed fence
# carrying a hex outside this set breaks the palette contract.
PALETTE = {
    "#282A36", "#21222C", "#44475A", "#6272A4", "#F8F8F2", "#8BE9FD", "#50FA7B", "#FFB86C",
    "#FF79C6", "#BD93F9", "#FF5555", "#F1FA8C",
    "#FFFBEB", "#6C664B", "#CFCFDE", "#1F1F1F", "#CB3A2A", "#A34D14", "#846E15", "#14710A",
    "#036A96", "#644AC9", "#A3144D",
}
HEX_COLOR = re.compile(r"#[0-9A-Fa-f]{6}\b")

# Flowchart tokenization: edge runs (with optional edge-id prefix and inline label), shape-bracket
# payloads, and metadata that must vanish before endpoints are read.
FC_EDGE = re.compile(r"([<ox]?[-=.~]{2,}[>ox]?(?:\|[^|]*\|)?)")
FC_AT = re.compile(r"@\{[^}]*\}")
FC_EDGE_ID = re.compile(r"(\w+)@(?=[-=.~<])")
FC_STR = re.compile(r'("[^"]*"|`[^`]*`)')
FC_SHAPE = re.compile(r"(\[\[.*?\]\]|\[\(.*?\)\]|\(\(.*?\)\)|\(\[.*?\]\)|\{\{.*?\}\}|\[/.*?/\]|\[\\.*?\\\]|(?<![-=<>])>[^\]]*\]|\[.*?\]|\(.*?\)|\{.*?\})")
FC_SKIP = re.compile(r"^\s*(subgraph\b|end\b|direction\b|classDef\b|linkStyle\b|style\b|click\b|class\b|accTitle|accDescr)")
IDENT = re.compile(r"[A-Za-z0-9_]+")

ST_TRANSITION = re.compile(r"^\s*(\[\*\]|[\w.]+)\s*-->\s*(\[\*\]|[\w.]+)")
SQ_PARTICIPANT = re.compile(r"^(?:create\s+)?(?:participant|actor)\s+([\w-]+)(?:@\{.*\})?(?:\s+as\s+.+)?\s*$")
SQ_ARROW = re.compile(r"^\s*(\w+)\s*(?:<<-?>>|--?(?:>>|>|\)|x))\s*[+-]?\s*(\w+)\s*:")
ER_RELATION = re.compile(r"^\s*([\w-]+)\s*[|o}{]{2}[-.]{2}[|o}{]{2}\s*([\w-]+)\s*:")


# --- [MODELS] ------------------------------------------------------------------------------

@dataclass(frozen=True, slots=True, kw_only=True)
class Fence:
    """One mermaid fence: 1-based start line, dedented body, and closure evidence."""

    line: int
    body: str
    unclosed: bool = False


@dataclass(frozen=True, slots=True, kw_only=True)
class Finding:
    """One graph-logic verdict: fail for structural defects, warn for legibility pressure."""

    status: str
    detail: str


# --- [OPERATIONS] --------------------------------------------------------------------------

def collect(paths: list[str]) -> tuple[list[Path], list[Path]]:
    """Expand argv paths to .md/.mmd files plus faults for unresolvable input.

    Returns:
        (unique files in first-seen order, unresolvable paths) — unscanned input is a
        failure, never silence.
    """
    files: list[Path] = []
    faults: list[Path] = []
    for raw in paths:
        p = Path(raw)
        if p.is_dir():
            files.extend(sorted(q for s in SUFFIXES for q in p.rglob(f"*{s}")))
        elif p.suffix in SUFFIXES and p.is_file():
            files.append(p)
        else:
            faults.append(p)
    return list(dict.fromkeys(files)), faults


def fences(path: Path) -> list[Fence]:
    """Extract mermaid fences, blockquote prefixes stripped from quoted bodies.

    Returns:
        Fences in file order, an unclosed fence carrying its fault; a .mmd file is one fence
        at line 1. Read and decode faults propagate for the caller to convert into a fault row.
    """
    text = path.read_text(encoding="utf-8")
    if path.suffix == ".mmd":
        return [Fence(line=1, body=text)] if text.strip() else []
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
        out.append(Fence(line=i + 2, body="\n".join(body), unclosed=j == len(lines)))
        i = j + 1
    return out


# --- [GRAPH_LOGIC]

def _diagram_lines(body: str) -> tuple[str, list[str], list[Finding]]:
    """Strip frontmatter, comments, and accessibility directives.

    Returns:
        (header word, remaining lines after the header, structural faults) — an unclosed
        frontmatter block or a deprecated init directive is a fault, never a silent skip.
    """
    faults = [Finding(status="fail", detail="deprecated-init-directive")] if DEPRECATED_INIT.search(body) else []
    lines = body.splitlines()
    i = 0
    while i < len(lines) and not lines[i].strip():
        i += 1
    if i < len(lines) and lines[i].strip() == "---":
        close = next((k for k in range(i + 1, len(lines)) if lines[k].strip() == "---"), -1)
        if close < 0:
            return "", [], [*faults, Finding(status="fail", detail="unclosed-frontmatter")]
        i = close + 1
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
    return (out[0].strip().split()[0] if out else "", out[1:], faults)


def _string_token(m: re.Match[str]) -> str:
    """Replace a quoted payload with a deterministic identifier-safe token.

    Returns:
        A stable token derived from the payload content.
    """
    return f"QL{zlib.crc32(m.group(0).encode('utf-8')) & 0xFFFF:04x}"


def _flowchart_logic(lines: list[str]) -> list[Finding]:
    """Orphan nodes, undefined classes, unknown class targets, duplicate edges, budgets.

    Edge ids (`e1@-->`) are first-class class targets, so `class e1 animate` never reads as
    an unknown node.

    Returns:
        Findings in defect-class order; empty when the graph is clean.
    """
    class_defs, class_uses, class_nodes = {"default"}, set[str](), set[str]()
    subgraphs, declared_only, endpoints, edge_ids = set[str](), set[str](), set[str](), set[str]()
    edges: list[tuple[str, str, str]] = []
    for raw in lines:
        if m := re.match(r"^\s*\w+@\{([^}]*)\}\s*;?\s*$", raw):
            keys = set(re.findall(r"([A-Za-z]\w*)\s*:", m.group(1)))
            if keys and keys <= {"animate", "animation", "curve"}:
                continue
        line = FC_AT.sub(" ", raw)
        line = FC_STR.sub(_string_token, line)
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
        edge_ids.update(FC_EDGE_ID.findall(line))
        line = FC_EDGE_ID.sub("", line)
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
    findings = [Finding(status="fail", detail=f"orphan-node:{n}") for n in sorted(declared_only - endpoints - subgraphs)]
    findings += [Finding(status="fail", detail=f"undefined-class:{c}") for c in sorted(class_uses - class_defs)]
    findings += [
        Finding(status="fail", detail=f"unknown-node-in-class:{n}")
        for n in sorted(class_nodes - declared_only - endpoints - subgraphs - edge_ids)
    ]
    findings += [
        Finding(status="warn", detail=f"duplicate-edge:{a}-->{b}")
        for (a, b, _), count in sorted(Counter(edges).items())
        if count > 1
    ]
    if len(nodes) > BUDGETS["flowchart-nodes"]:
        findings.append(Finding(status="warn", detail=f"budget:{len(nodes)} nodes > {BUDGETS['flowchart-nodes']}"))
    if len(edges) > BUDGETS["flowchart-edges"]:
        findings.append(Finding(status="warn", detail=f"budget:{len(edges)} edges > {BUDGETS['flowchart-edges']}"))
    return findings


def _state_logic(lines: list[str]) -> list[Finding]:
    """Unreachable states by graph reachability from start markers, undefined classes, budget.

    Composite interiors are scoped out by brace depth: only top-level states and transitions
    enter the reachability graph, so a nested `[*]` never poses as the top-level start.

    Returns:
        Findings in defect-class order; empty when the diagram is clean.
    """
    class_defs, class_uses = {"default"}, set[str]()
    states: set[str] = set()
    starts: set[str] = set()
    adjacency: dict[str, set[str]] = {}
    depth = 0
    for raw in lines:
        line = raw
        for _, names in re.findall(r"([\w.]+):::([\w,]+)", line):
            class_uses.update(names.split(","))
        line = re.sub(r":::[\w,]+", "", line)
        s = line.strip()
        closing = s.startswith("}")
        depth = max(0, depth - 1) if closing else depth
        opening = s.endswith("{")
        if m := re.match(r"^classDef\s+([\w,]+)", s):
            class_defs.update(m.group(1).split(","))
        elif m := re.match(r"^class\s+([\w,\s]+?)\s+([\w,]+)\s*;?\s*$", s):
            class_uses.update(m.group(2).split(","))
        elif depth == 0 and not closing:
            if m := re.match(r'^state\s+"[^"]*"\s+as\s+([\w.]+)', s):
                states.add(m.group(1))
            elif m := re.match(r"^state\s+([\w.]+)", s):
                states.add(m.group(1))
            elif m := ST_TRANSITION.match(line):
                a, b = m.group(1), m.group(2)
                states.update(t for t in (a, b) if t != "[*]")
                if a == "[*]" and b != "[*]":
                    starts.add(b)
                elif a != "[*]" and b != "[*]":
                    adjacency.setdefault(a, set()).add(b)
            elif m := re.match(r"^([\w.]+)\s*[:{]", s):
                states.add(m.group(1))
        depth += 1 if opening else 0
    reachable = set(starts)
    frontier = list(starts)
    while frontier:
        for successor in adjacency.get(frontier.pop(), ()):
            if successor not in reachable:
                reachable.add(successor)
                frontier.append(successor)
    findings = [Finding(status="fail", detail=f"undefined-class:{c}") for c in sorted(class_uses - class_defs)]
    if starts:
        findings += [Finding(status="fail", detail=f"unreachable-state:{n}") for n in sorted(states - reachable)]
    if len(states) > BUDGETS["state-states"]:
        findings.append(Finding(status="warn", detail=f"budget:{len(states)} states > {BUDGETS['state-states']}"))
    return findings


def _sequence_logic(lines: list[str]) -> list[Finding]:
    """Orphan declared participants and the participant budget.

    Returns:
        Findings in defect-class order; empty when the diagram is clean.
    """
    declared, used = set[str](), set[str]()
    for raw in lines:
        s = raw.strip()
        if m := SQ_PARTICIPANT.match(s):
            declared.add(m.group(1))
            continue
        if m := re.match(r"^destroy\s+([\w-]+)", s):
            used.add(m.group(1))
            continue
        if m := re.match(r"^[Nn]ote\s+(?:over|left of|right of)\s+([\w,\s]+?):", s):
            used.update(t for t in re.split(r"[\s,]+", m.group(1)) if t)
            continue
        if m := SQ_ARROW.match(raw):
            used.update((m.group(1), m.group(2)))
    findings = [Finding(status="warn", detail=f"orphan-participant:{p}") for p in sorted(declared - used)]
    if len(declared | used) > BUDGETS["sequence-participants"]:
        findings.append(Finding(status="warn", detail=f"budget:{len(declared | used)} participants > {BUDGETS['sequence-participants']}"))
    return findings


def _er_logic(lines: list[str]) -> list[Finding]:
    """Orphan entities (attribute block, zero relations) and the entity budget.

    Returns:
        Findings in defect-class order; empty when the diagram is clean.
    """
    blocks, related = set[str](), set[str]()
    for raw in lines:
        if m := ER_RELATION.match(raw):
            related.update((m.group(1), m.group(2)))
        elif m := re.match(r"^\s*([\w-]+)\s*\{", raw):
            blocks.add(m.group(1))
    findings = [Finding(status="warn", detail=f"orphan-entity:{e}") for e in sorted(blocks - related)]
    entities = blocks | related
    if len(entities) > BUDGETS["er-entities"]:
        findings.append(Finding(status="warn", detail=f"budget:{len(entities)} entities > {BUDGETS['er-entities']}"))
    return findings


def _class_logic(lines: list[str]) -> list[Finding]:
    """Undefined style classes, duplicate relations, and the class budget.

    Returns:
        Findings in defect-class order; empty when the diagram is clean.
    """
    class_defs, class_uses, names = {"default"}, set(), set()
    relations: list[tuple[str, str, str]] = []
    rel_op = re.compile(r"^\s*([\w.]+)(?:~[\w~]+~)?\s*(?:\"[^\"]*\"\s*)?(<\|--|<\|\.\.|\*--|o--|-->|\.\.>|\.\.\|>|--\*|--o|<--|--|\.\.)\s*(?:\"[^\"]*\"\s*)?([\w.]+)(?:~[\w~]+~)?")
    for raw in lines:
        s = raw.strip()
        for _, cls in re.findall(r"([\w.]+):::([\w,]+)", s):
            class_uses.update(cls.split(","))
        s = re.sub(r":::[\w,]+", "", s)
        if m := re.match(r"^classDef\s+([\w,]+)", s):
            class_defs.update(m.group(1).split(","))
            continue
        if s.startswith(("namespace", "note", "link", "click", "direction", "}", "<<")) or "()--" in s or "--()" in s:
            names.update((re.findall(r"\(\)--\s*([\w.]+)|([\w.]+)\s*\(\)--", s) and {t for pair in re.findall(r"\(\)--\s*([\w.]+)|([\w.]+)\s*\(\)--", s) for t in pair if t}) or set())
            continue
        if m := re.match(r"^class\s+([\w.]+)(?:~[\w~]+~)?", s):
            names.add(m.group(1))
            continue
        if m := rel_op.match(s):
            relations.append((m.group(1), m.group(2), m.group(3)))
            names.update((m.group(1), m.group(3)))
    findings = [Finding(status="fail", detail=f"undefined-class:{c}") for c in sorted(class_uses - class_defs)]
    findings += [
        Finding(status="warn", detail=f"duplicate-relation:{a}{op}{b}")
        for (a, op, b), count in sorted(Counter(relations).items())
        if count > 1
    ]
    if len(names) > BUDGETS["class-classes"]:
        findings.append(Finding(status="warn", detail=f"budget:{len(names)} classes > {BUDGETS['class-classes']}"))
    return findings


def _gantt_logic(lines: list[str]) -> list[Finding]:
    """Dependency-reference integrity: every `after`/`until` names a declared task id.

    Returns:
        Findings in defect-class order; empty when the diagram is clean.
    """
    modifiers = {"done", "active", "crit", "milestone", "vert"}
    declared: set[str] = set()
    refs: list[str] = []
    for raw in lines:
        s = raw.strip()
        if ":" not in s or s.split(":")[0].strip().split(" ")[0] in {"section", "dateFormat", "axisFormat", "excludes", "includes", "todayMarker", "tickInterval"}:
            continue
        fields = [f.strip() for f in s.split(":", 1)[1].split(",")]
        for field in fields:
            if m := re.match(r"^(?:after|until)\s+([\w\s]+)$", field):
                refs.extend(m.group(1).split())
            elif re.fullmatch(r"[A-Za-z_]\w*", field) and field not in modifiers:
                declared.add(field)
    return [Finding(status="fail", detail=f"unknown-task-ref:{r}") for r in sorted(set(refs) - declared)]


def _architecture_logic(lines: list[str]) -> list[Finding]:
    """Group-membership integrity, unknown edge endpoints, and orphan services.

    Returns:
        Findings in defect-class order; empty when the diagram is clean.
    """
    groups, members, in_refs = set(), set(), set()
    endpoints: set[str] = set()
    edge = re.compile(r"([\w-]+)(?:\{group\})?:[TBLR]\s*(?:<?-->?|--)\s*[TBLR]:([\w-]+)(?:\{group\})?")
    for raw in lines:
        s = raw.strip()
        if m := re.match(r"^group\s+([\w-]+)", s):
            groups.add(m.group(1))
        elif m := re.match(r"^(?:service|junction)\s+([\w-]+)(?:\([\w:-]+\))?(?:\[[^\]]*\])?(?:\s+in\s+([\w-]+))?", s):
            members.add(m.group(1))
            if m.group(2):
                in_refs.add(m.group(2))
        elif s.startswith("align"):
            continue
        if m := edge.search(s):
            endpoints.update((m.group(1), m.group(2)))
    findings = [Finding(status="fail", detail=f"unknown-group:{g}") for g in sorted(in_refs - groups)]
    findings += [Finding(status="fail", detail=f"unknown-service:{e}") for e in sorted(endpoints - members - groups)]
    findings += [Finding(status="warn", detail=f"orphan-service:{o}") for o in sorted(members - endpoints)]
    return findings


def _requirement_logic(lines: list[str]) -> list[Finding]:
    """Relation-vocabulary integrity, undeclared relation endpoints, and orphan requirements.

    Returns:
        Findings in defect-class order; empty when the diagram is clean.
    """
    vocabulary = {"contains", "copies", "derives", "satisfies", "verifies", "refines", "traces"}
    kinds = r"requirement|functionalRequirement|interfaceRequirement|performanceRequirement|physicalRequirement|designConstraint"
    declared, requirements, related = set(), set(), set()
    findings: list[Finding] = []
    for raw in lines:
        s = raw.strip()
        if m := re.match(rf"^({kinds}|element)\s+([\w-]+)\s*\{{", s):
            declared.add(m.group(2))
            if m.group(1) != "element":
                requirements.add(m.group(2))
        elif m := re.match(r"^([\w-]+)\s*(?:-\s*(\w+)\s*->|<-\s*(\w+)\s*-)\s*([\w-]+)", s):
            relation = m.group(2) or m.group(3)
            if relation not in vocabulary:
                findings.append(Finding(status="fail", detail=f"unknown-relation:{relation}"))
            related.update((m.group(1), m.group(4)))
    findings += [Finding(status="fail", detail=f"undeclared-endpoint:{e}") for e in sorted(related - declared)]
    findings += [Finding(status="warn", detail=f"orphan-requirement:{q}") for q in sorted(requirements - related)]
    return findings


def logic_findings(body: str) -> list[Finding]:
    """Dispatch graph-logic analysis by diagram header.

    A family without implemented logic warns `logic-unimplemented` so an all-green run never
    claims analysis it did not perform.

    Returns:
        Findings in defect-class order; empty when the diagram is clean.
    """
    header, lines, faults = _diagram_lines(body)
    if faults:
        return faults
    if header in {"flowchart", "graph"}:
        return _flowchart_logic(lines)
    if header.startswith("stateDiagram"):
        return _state_logic(lines)
    if header == "sequenceDiagram":
        return _sequence_logic(lines)
    if header == "erDiagram":
        return _er_logic(lines)
    if header == "classDiagram":
        return _class_logic(lines)
    if header == "gantt":
        return _gantt_logic(lines)
    if header == "architecture-beta":
        return _architecture_logic(lines)
    if header == "requirementDiagram":
        return _requirement_logic(lines)
    return [Finding(status="warn", detail=f"logic-unimplemented:{header}")] if header else []


def contract_findings(body: str) -> list[Finding]:
    """Committed-diagram contract checks: accessibility-pair consistency and palette fidelity.

    An accTitle without accDescr (or the reverse) is always a defect; a fence that declares
    `themeVariables` has opted into the palette contract, so an off-palette hex warns.

    Returns:
        Warn findings only; empty when the fence honors both contracts.
    """
    has_title, has_descr = "accTitle" in body, "accDescr" in body
    findings = [Finding(status="warn", detail="acc-pair-incomplete")] if has_title != has_descr else []
    if "themeVariables" in body:
        off = {h.upper() for h in HEX_COLOR.findall(body)} - PALETTE
        findings += [Finding(status="warn", detail=f"off-palette:{h}") for h in sorted(off)]
    return findings


# --- [RENDER]

def renderer(override: str | None) -> tuple[list[str], Path | None]:
    """Resolve the mmdc invocation: admitted override, pnpm workspace, PATH, then npx.

    Returns:
        (argv prefix, cwd for pnpm resolution); ([], None) when no renderer resolves — an
        override whose binary is absent does not resolve.
    """
    if override:
        argv = shlex.split(override)
        return (argv, None) if argv and shutil.which(argv[0]) else ([], None)
    probe = Path.cwd().resolve()
    for root in (probe, *probe.parents):
        if (root / "pnpm-lock.yaml").exists():
            return ["pnpm", "exec", "mmdc"], root
    if shutil.which("mmdc"):
        return ["mmdc"], None
    if shutil.which("npx"):
        return ["npx", "-y", "-p", "@mermaid-js/mermaid-cli", "mmdc"], None
    return [], None


def render(prefix: list[str], cwd: Path | None, body: str, workdir: Path) -> tuple[bool, str]:
    """Render one fence body to SVG through the resolved renderer.

    Returns:
        (ok, first stderr line on failure) — a vanished or non-executable renderer maps to an
        environment fault, never a traceback.
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
    except OSError as exc:
        return False, f"environment: renderer unavailable ({type(exc).__name__})"
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

    def fence_rows(path: str, fence: Fence) -> bool:
        fence_failed = False
        if fence.unclosed:
            emit(path, fence.line, "logic", "fail", "unclosed-fence")
            return True
        if not fence.body.lstrip().startswith("---"):
            emit(path, fence.line, "frontmatter", "warn", "no-frontmatter")
        for finding in contract_findings(fence.body):
            emit(path, fence.line, "contract", finding.status, finding.detail)
        for finding in logic_findings(fence.body):
            fence_failed = fence_failed or finding.status == "fail"
            emit(path, fence.line, "logic", finding.status, finding.detail)
        return fence_failed

    files, fault_paths = collect(ns.paths)
    failed = bool(fault_paths)
    for fault in fault_paths:
        emit(str(fault), 0, "collect", "fail", "not a markdown or mmd target")
    targets: list[tuple[Path, list[Fence]]] = []
    for path in files:
        try:
            fence_list = fences(path)
        except (OSError, UnicodeDecodeError) as exc:
            emit(str(path), 0, "read", "fail", type(exc).__name__)
            failed = True
            continue
        if fence_list:
            targets.append((path, fence_list))
    if ns.no_render or not targets:
        for path, fence_list in targets:
            for fence in fence_list:
                failed = fence_rows(str(path), fence) or failed
        return 1 if failed else 0
    prefix, cwd = renderer(ns.renderer)
    if not prefix:
        emit("-", 0, "setup", "fail", "no mermaid renderer: need an admitted --renderer, pnpm workspace mmdc, mmdc on PATH, or npx")
        return 1

    with tempfile.TemporaryDirectory(prefix="mermaid-validate-") as tmp:
        workdir = Path(tmp)
        for path, fence_list in targets:
            rel = str(path)
            for fence in fence_list:
                if fence.unclosed:
                    emit(rel, fence.line, "logic", "fail", "unclosed-fence")
                    failed = True
                    continue
                ok, detail = render(prefix, cwd, fence.body, workdir)
                failed = failed or not ok
                emit(rel, fence.line, "render", "ok" if ok else "fail", detail)
                if not fence.body.lstrip().startswith("---"):
                    emit(rel, fence.line, "frontmatter", "warn", "no-frontmatter")
                for finding in contract_findings(fence.body):
                    emit(rel, fence.line, "contract", finding.status, finding.detail)
                for finding in logic_findings(fence.body):
                    failed = failed or finding.status == "fail"
                    emit(rel, fence.line, "logic", finding.status, finding.detail)
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
